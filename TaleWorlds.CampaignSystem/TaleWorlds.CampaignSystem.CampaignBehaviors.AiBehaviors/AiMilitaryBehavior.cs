using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;

public class AiMilitaryBehavior : CampaignBehaviorBase
{
	private const int MinimumInfluenceNeededToCreateArmy = 50;

	private IDisbandPartyCampaignBehavior _disbandPartyCampaignBehavior;

	public override void RegisterEvents()
	{
		CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
		CampaignEvents.AiHourlyTickEvent.AddNonSerializedListener(this, AiHourlyTick);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
	}

	private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		_disbandPartyCampaignBehavior = Campaign.Current.GetCampaignBehavior<IDisbandPartyCampaignBehavior>();
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
	{
		if (mobileParty != null && mobileParty.IsBandit && settlement.IsHideout && mobileParty.DefaultBehavior != AiBehavior.GoToSettlement)
		{
			mobileParty.Ai.SetMoveGoToSettlement(settlement);
		}
	}

	private void FindBestTargetAndItsValueForFaction(Army.ArmyTypes missionType, PartyThinkParams p, float ourStrength, float newArmyCreatingAdditionalConstant = 1f)
	{
		MobileParty mobilePartyOf = p.MobilePartyOf;
		IFaction mapFaction = mobilePartyOf.MapFaction;
		if (mobilePartyOf.Army != null && mobilePartyOf.Army.LeaderParty != mobilePartyOf)
		{
			return;
		}
		float num = 1f;
		if (mobilePartyOf.Army != null)
		{
			float num2 = 0f;
			foreach (MobileParty party in mobilePartyOf.Army.Parties)
			{
				float num3 = (PartyBaseHelper.FindPartySizeNormalLimit(party) + 1f) * 0.5f;
				float num4 = party.PartySizeRatio / num3;
				num2 += num4;
			}
			num = num2 / (float)mobilePartyOf.Army.Parties.Count;
		}
		else if (newArmyCreatingAdditionalConstant <= 1.01f)
		{
			float num5 = (PartyBaseHelper.FindPartySizeNormalLimit(mobilePartyOf) + 1f) * 0.5f;
			num = mobilePartyOf.PartySizeRatio / num5;
		}
		else
		{
			num = 1f;
		}
		float num6 = MathF.Max(1f, MathF.Min((float)mobilePartyOf.MapFaction.Fiefs.Count / 5f, 2.5f));
		if (missionType == Army.ArmyTypes.Defender)
		{
			num6 = MathF.Pow(num6, 0.75f);
		}
		float partySizeScore = MathF.Min(1f, MathF.Pow(num, num6));
		AiBehavior aiBehavior = AiBehavior.Hold;
		switch (missionType)
		{
		case Army.ArmyTypes.Defender:
			aiBehavior = AiBehavior.DefendSettlement;
			break;
		case Army.ArmyTypes.Besieger:
			aiBehavior = AiBehavior.BesiegeSettlement;
			break;
		case Army.ArmyTypes.Raider:
			aiBehavior = AiBehavior.RaidSettlement;
			break;
		case Army.ArmyTypes.Patrolling:
			aiBehavior = AiBehavior.PatrolAroundPoint;
			break;
		}
		if (missionType == Army.ArmyTypes.Defender || missionType == Army.ArmyTypes.Patrolling)
		{
			CalculateMilitaryBehaviorForFactionSettlementsParallel(mapFaction, p, missionType, aiBehavior, ourStrength, partySizeScore, newArmyCreatingAdditionalConstant);
			return;
		}
		foreach (IFaction enemyFaction in FactionManager.GetEnemyFactions(mapFaction))
		{
			CalculateMilitaryBehaviorForFactionSettlementsParallel(enemyFaction, p, missionType, aiBehavior, ourStrength, partySizeScore, newArmyCreatingAdditionalConstant);
		}
	}

	private void CalculateMilitaryBehaviorForFactionSettlementsParallel(IFaction faction, PartyThinkParams p, Army.ArmyTypes missionType, AiBehavior aiBehavior, float ourStrength, float partySizeScore, float newArmyCreatingAdditionalConstant)
	{
		MobileParty mobilePartyOf = p.MobilePartyOf;
		int count = faction.Settlements.Count;
		float totalStrength = faction.TotalStrength;
		for (int i = 0; i < faction.Settlements.Count; i++)
		{
			Settlement settlement = faction.Settlements[i];
			if (CheckIfSettlementIsSuitableForMilitaryAction(settlement, mobilePartyOf, missionType))
			{
				CalculateMilitaryBehaviorForSettlement(settlement, missionType, aiBehavior, p, ourStrength, partySizeScore, count, totalStrength, newArmyCreatingAdditionalConstant);
			}
		}
	}

	private bool CheckIfSettlementIsSuitableForMilitaryAction(Settlement settlement, MobileParty mobileParty, Army.ArmyTypes missionType)
	{
		if (Game.Current.CheatMode && !CampaignCheats.MainPartyIsAttackable && settlement.Party.MapEvent != null && settlement.Party.MapEvent == MapEvent.PlayerMapEvent)
		{
			return false;
		}
		if (((mobileParty.DefaultBehavior == AiBehavior.BesiegeSettlement && missionType == Army.ArmyTypes.Besieger) || (mobileParty.DefaultBehavior == AiBehavior.RaidSettlement && missionType == Army.ArmyTypes.Raider) || (mobileParty.DefaultBehavior == AiBehavior.DefendSettlement && missionType == Army.ArmyTypes.Defender)) && mobileParty.TargetSettlement == settlement)
		{
			return false;
		}
		if (missionType == Army.ArmyTypes.Raider)
		{
			float num = MathF.Max(100f, MathF.Min(250f, Campaign.Current.Models.MapDistanceModel.GetDistance(mobileParty.MapFaction.FactionMidSettlement, settlement.MapFaction.FactionMidSettlement)));
			if (Campaign.Current.Models.MapDistanceModel.GetDistance(mobileParty, settlement) > num)
			{
				return false;
			}
		}
		return true;
	}

	private void CalculateMilitaryBehaviorForSettlement(Settlement settlement, Army.ArmyTypes missionType, AiBehavior aiBehavior, PartyThinkParams p, float ourStrength, float partySizeScore, int numberOfEnemyFactionSettlements, float totalEnemyMobilePartyStrength, float newArmyCreatingAdditionalConstant = 1f)
	{
		if ((missionType != Army.ArmyTypes.Defender || settlement.LastAttackerParty == null || !settlement.LastAttackerParty.IsActive) && (missionType != Army.ArmyTypes.Raider || !settlement.IsVillage || settlement.Village.VillageState != 0) && (missionType != 0 || !settlement.IsFortification || (settlement.SiegeEvent != null && settlement.SiegeEvent.BesiegerCamp.LeaderParty.MapFaction != p.MobilePartyOf.MapFaction)) && (missionType != Army.ArmyTypes.Patrolling || settlement.IsCastle || !p.WillGatherAnArmy))
		{
			return;
		}
		MobileParty mobilePartyOf = p.MobilePartyOf;
		IFaction mapFaction = mobilePartyOf.MapFaction;
		float num = mobilePartyOf.Food;
		float num2 = 0f - mobilePartyOf.FoodChange;
		if (mobilePartyOf.Army != null && mobilePartyOf == mobilePartyOf.Army.LeaderParty)
		{
			foreach (MobileParty attachedParty in mobilePartyOf.Army.LeaderParty.AttachedParties)
			{
				num += attachedParty.Food;
				num2 += 0f - attachedParty.FoodChange;
			}
		}
		float num3 = MathF.Max(0f, num) / num2;
		float num4 = ((num3 < 5f) ? (0.1f + 0.9f * (num3 / 5f)) : 1f);
		float num5 = ((missionType != Army.ArmyTypes.Patrolling) ? Campaign.Current.Models.TargetScoreCalculatingModel.GetTargetScoreForFaction(settlement, missionType, mobilePartyOf, ourStrength, numberOfEnemyFactionSettlements, totalEnemyMobilePartyStrength) : Campaign.Current.Models.TargetScoreCalculatingModel.CalculatePatrollingScoreForSettlement(settlement, mobilePartyOf));
		num5 *= partySizeScore * num4 * newArmyCreatingAdditionalConstant;
		if (mobilePartyOf.Objective == MobileParty.PartyObjective.Defensive)
		{
			num5 = ((aiBehavior != AiBehavior.DefendSettlement && (aiBehavior != AiBehavior.PatrolAroundPoint || settlement.MapFaction != mapFaction)) ? (num5 * 0.8f) : (num5 * 1.2f));
		}
		else if (mobilePartyOf.Objective == MobileParty.PartyObjective.Aggressive)
		{
			num5 = ((aiBehavior != AiBehavior.BesiegeSettlement && aiBehavior != AiBehavior.RaidSettlement) ? (num5 * 0.8f) : (num5 * 1.2f));
		}
		if (!mobilePartyOf.IsDisbanding)
		{
			IDisbandPartyCampaignBehavior disbandPartyCampaignBehavior = _disbandPartyCampaignBehavior;
			if (disbandPartyCampaignBehavior == null || !disbandPartyCampaignBehavior.IsPartyWaitingForDisband(mobilePartyOf))
			{
				goto IL_0209;
			}
		}
		num5 *= 0.25f;
		goto IL_0209;
		IL_0209:
		AIBehaviorTuple item = new AIBehaviorTuple(settlement, aiBehavior, p.WillGatherAnArmy);
		(AIBehaviorTuple, float) value = (item, num5);
		p.AddBehaviorScore(in value);
	}

	private void AiHourlyTick(MobileParty mobileParty, PartyThinkParams p)
	{
		if (mobileParty.IsMilitia || mobileParty.IsCaravan || mobileParty.IsVillager || mobileParty.IsBandit || mobileParty.IsDisbanding || mobileParty.LeaderHero == null || (mobileParty.MapFaction != Clan.PlayerClan.MapFaction && !mobileParty.MapFaction.IsKingdomFaction) || mobileParty.CurrentSettlement?.SiegeEvent != null)
		{
			return;
		}
		if (mobileParty.Army != null)
		{
			mobileParty.Ai.SetInitiative(0.33f, 0.33f, 24f);
			if (mobileParty.Army.LeaderParty == mobileParty && (mobileParty.Army.AIBehavior == Army.AIBehaviorFlags.Gathering || mobileParty.Army.AIBehavior == Army.AIBehaviorFlags.WaitingForArmyMembers))
			{
				mobileParty.Ai.SetInitiative(0.33f, 1f, 24f);
				p.DoNotChangeBehavior = true;
			}
			else if (mobileParty.Army.AIBehavior == Army.AIBehaviorFlags.Patrolling)
			{
				mobileParty.Ai.SetInitiative(1f, 1f, 24f);
			}
			else if (mobileParty.Army.AIBehavior == Army.AIBehaviorFlags.Defending && mobileParty.Army.LeaderParty == mobileParty && mobileParty.Army.AiBehaviorObject != null && mobileParty.Army.AiBehaviorObject is Settlement && ((Settlement)mobileParty.Army.AiBehaviorObject).GatePosition.DistanceSquared(mobileParty.Position2D) < 100f)
			{
				mobileParty.Ai.SetInitiative(1f, 1f, 24f);
			}
			if (mobileParty.Army.LeaderParty != mobileParty)
			{
				return;
			}
		}
		else if (mobileParty.DefaultBehavior == AiBehavior.DefendSettlement || mobileParty.Objective == MobileParty.PartyObjective.Defensive)
		{
			mobileParty.Ai.SetInitiative(0.33f, 1f, 2f);
		}
		float num = 1f;
		if (mobileParty.Army != null)
		{
			float num2 = 0f;
			foreach (MobileParty party in mobileParty.Army.Parties)
			{
				float num3 = (PartyBaseHelper.FindPartySizeNormalLimit(party) + 1f) * 0.5f;
				float num4 = party.PartySizeRatio / num3;
				num2 += num4;
			}
			num = num2 / (float)mobileParty.Army.Parties.Count;
		}
		else
		{
			float num5 = (PartyBaseHelper.FindPartySizeNormalLimit(mobileParty) + 1f) * 0.5f;
			num = mobileParty.PartySizeRatio / num5;
		}
		float y = MathF.Max(1f, MathF.Min((float)mobileParty.MapFaction.Fiefs.Count / 5f, 2.5f));
		float num6 = MathF.Min(1f, MathF.Pow(num, y));
		float num7 = mobileParty.Food;
		float num8 = 0f - mobileParty.FoodChange;
		int num9 = 1;
		if (mobileParty.Army != null && mobileParty == mobileParty.Army.LeaderParty)
		{
			foreach (MobileParty attachedParty in mobileParty.Army.LeaderParty.AttachedParties)
			{
				num7 += attachedParty.Food;
				num8 += 0f - attachedParty.FoodChange;
				num9++;
			}
		}
		float num10 = MathF.Max(0f, num7) / num8;
		float num11 = ((num10 < 5f) ? (0.1f + 0.9f * (num10 / 5f)) : 1f);
		float totalStrengthWithFollowers = mobileParty.GetTotalStrengthWithFollowers(includeNonAttachedArmyMembers: false);
		float num12;
		if ((mobileParty.DefaultBehavior == AiBehavior.BesiegeSettlement || mobileParty.DefaultBehavior == AiBehavior.RaidSettlement || mobileParty.DefaultBehavior == AiBehavior.DefendSettlement) && mobileParty.TargetSettlement != null)
		{
			num12 = Campaign.Current.Models.TargetScoreCalculatingModel.CurrentObjectiveValue(mobileParty);
			num12 *= ((mobileParty.MapEvent == null || mobileParty.SiegeEvent == null) ? (num11 * num6) : 1f);
			if (mobileParty.SiegeEvent != null)
			{
				float num13 = 0f;
				foreach (PartyBase item2 in (mobileParty.DefaultBehavior == AiBehavior.BesiegeSettlement) ? mobileParty.SiegeEvent.BesiegedSettlement.GetInvolvedPartiesForEventType() : mobileParty.SiegeEvent.BesiegerCamp.GetInvolvedPartiesForEventType())
				{
					num13 += item2.TotalStrength;
				}
				float x2 = totalStrengthWithFollowers / num13;
				float num14 = MathF.Max(1f, MathF.Pow(x2, 1.75f) * 0.15f);
				num12 *= num14;
			}
			if (!mobileParty.IsDisbanding)
			{
				IDisbandPartyCampaignBehavior disbandPartyCampaignBehavior = _disbandPartyCampaignBehavior;
				if (disbandPartyCampaignBehavior == null || !disbandPartyCampaignBehavior.IsPartyWaitingForDisband(mobileParty))
				{
					goto IL_048d;
				}
			}
			num12 *= 0.25f;
			goto IL_048d;
		}
		goto IL_04c0;
		IL_048d:
		p.CurrentObjectiveValue = num12;
		AiBehavior defaultBehavior = mobileParty.DefaultBehavior;
		AIBehaviorTuple item = new AIBehaviorTuple(mobileParty.TargetSettlement, defaultBehavior);
		(AIBehaviorTuple, float) value = (item, num12);
		p.AddBehaviorScore(in value);
		goto IL_04c0;
		IL_04c0:
		p.Initialization();
		bool flag = false;
		float newArmyCreatingAdditionalConstant = 1f;
		float num15 = totalStrengthWithFollowers;
		if (mobileParty.LeaderHero != null && mobileParty.Army == null && mobileParty.LeaderHero.Clan != null && mobileParty.PartySizeRatio > 0.6f && (mobileParty.LeaderHero.Clan.Leader == mobileParty.LeaderHero || (mobileParty.LeaderHero.Clan.Leader.PartyBelongedTo == null && mobileParty.LeaderHero.Clan.WarPartyComponents != null && mobileParty.LeaderHero.Clan.WarPartyComponents.FirstOrDefault() == mobileParty.WarPartyComponent)))
		{
			int traitLevel = mobileParty.LeaderHero.GetTraitLevel(DefaultTraits.Calculating);
			IFaction mapFaction = mobileParty.MapFaction;
			Kingdom kingdom = (Kingdom)mapFaction;
			int count = ((Kingdom)mapFaction).Armies.Count;
			int num16 = 50 + count * count * 20 + mobileParty.LeaderHero.RandomInt(20) + traitLevel * 20;
			float num17 = 1f - (float)count * 0.2f;
			flag = mobileParty.LeaderHero.Clan.Influence > (float)num16 && mobileParty.MapFaction.IsKingdomFaction && !mobileParty.LeaderHero.Clan.IsUnderMercenaryService && FactionManager.GetEnemyFactions(mobileParty.MapFaction as Kingdom).AnyQ((IFaction x) => x.Fiefs.Any());
			if (flag)
			{
				float num18 = ((kingdom.Armies.Count == 0) ? (1f + MathF.Sqrt((int)CampaignTime.Now.ToDays - kingdom.LastArmyCreationDay) * 0.15f) : 1f);
				float num19 = (10f + MathF.Sqrt(MathF.Min(900f, mobileParty.LeaderHero.Clan.Influence))) / 50f;
				float num20 = MathF.Sqrt(mobileParty.PartySizeRatio);
				newArmyCreatingAdditionalConstant = num18 * num19 * num17 * num20;
				num15 = mobileParty.Party.TotalStrength;
				List<MobileParty> mobilePartiesToCallToArmy = Campaign.Current.Models.ArmyManagementCalculationModel.GetMobilePartiesToCallToArmy(mobileParty);
				if (mobilePartiesToCallToArmy.Count == 0)
				{
					flag = false;
				}
				else
				{
					foreach (MobileParty item3 in mobilePartiesToCallToArmy)
					{
						num15 += item3.Party.TotalStrength;
					}
				}
			}
		}
		for (int i = 0; i < 4; i++)
		{
			if (flag)
			{
				p.WillGatherAnArmy = true;
				FindBestTargetAndItsValueForFaction((Army.ArmyTypes)i, p, num15, newArmyCreatingAdditionalConstant);
			}
			p.WillGatherAnArmy = false;
			FindBestTargetAndItsValueForFaction((Army.ArmyTypes)i, p, totalStrengthWithFollowers);
		}
	}
}
