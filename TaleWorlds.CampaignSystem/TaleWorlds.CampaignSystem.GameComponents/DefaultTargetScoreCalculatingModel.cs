using System;
using System.Linq;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultTargetScoreCalculatingModel : TargetScoreCalculatingModel
{
	private const float SiegeBaseValueFactor = 0.8f;

	private const float RaidBaseValueFactor = 0.28f;

	private const float DefenceBaseValueFactor = 1.28f;

	private const float DefenceVillageBaseValueFactor = 1.28f;

	private const float DefenceFollowEnemyBaseValueFactor = 0.8f;

	private const float MeaningfulCohesionThresholdForArmy = 40f;

	public override float TravelingToAssignmentFactor => 1.33f;

	public override float BesiegingFactor => 1.67f;

	public override float AssaultingTownFactor => 2f;

	public override float RaidingFactor => 1.67f;

	public override float DefendingFactor => 1.67f;

	private float ReasonableDistanceForBesiegingTown => (127f + 2.27f * Campaign.AverageDistanceBetweenTwoFortifications) / 2f;

	private float ReasonableDistanceForBesiegingCastle => (106f + 1.89f * Campaign.AverageDistanceBetweenTwoFortifications) / 2f;

	private float ReasonableDistanceForRaiding => (106f + 1.89f * Campaign.AverageDistanceBetweenTwoFortifications) / 2f;

	private float ReasonableDistanceForDefendingTownOrCastle => (160f + 2.84f * Campaign.AverageDistanceBetweenTwoFortifications) / 2f;

	private float ReasonableDistanceForDefendingVillage => (80f + 1.42f * Campaign.AverageDistanceBetweenTwoFortifications) / 2f;

	private float DistanceOfMobilePartyDivider => (254f + 4.54f * Campaign.AverageDistanceBetweenTwoFortifications) / 2f;

	private float RaidDistanceLimit => (318f + 5.68f * Campaign.AverageDistanceBetweenTwoFortifications) / 2f;

	private float GiveUpDistanceLimit => (127f + 2.27f * Campaign.AverageDistanceBetweenTwoFortifications) / 2f;

	public override float CurrentObjectiveValue(MobileParty mobileParty)
	{
		float totalStrengthWithFollowers = mobileParty.GetTotalStrengthWithFollowers(includeNonAttachedArmyMembers: false);
		float result = 0f;
		if (mobileParty.TargetSettlement == null)
		{
			return 0f;
		}
		if (mobileParty.DefaultBehavior != AiBehavior.BesiegeSettlement && mobileParty.DefaultBehavior != AiBehavior.RaidSettlement && mobileParty.DefaultBehavior != AiBehavior.DefendSettlement)
		{
			return result;
		}
		int count = mobileParty.TargetSettlement.MapFaction.Settlements.Count;
		float totalStrength = mobileParty.TargetSettlement.MapFaction.TotalStrength;
		result = GetTargetScoreForFaction(mobileParty.TargetSettlement, (mobileParty.DefaultBehavior != AiBehavior.BesiegeSettlement) ? ((mobileParty.DefaultBehavior == AiBehavior.RaidSettlement) ? Army.ArmyTypes.Raider : Army.ArmyTypes.Defender) : Army.ArmyTypes.Besieger, mobileParty, totalStrengthWithFollowers, count, totalStrength);
		switch (mobileParty.DefaultBehavior)
		{
		case AiBehavior.BesiegeSettlement:
			result *= ((mobileParty.Party.MapEvent == null && mobileParty.TargetSettlement.SiegeEvent != null && mobileParty.TargetSettlement.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(mobileParty.Party)) ? BesiegingFactor : ((mobileParty.Party.MapEvent != null && mobileParty.Party.MapEvent.MapEventSettlement == mobileParty.TargetSettlement) ? AssaultingTownFactor : TravelingToAssignmentFactor));
			break;
		case AiBehavior.RaidSettlement:
			result *= ((mobileParty.Party.MapEvent != null && mobileParty.MapEvent.MapEventSettlement == mobileParty.TargetSettlement) ? RaidingFactor : TravelingToAssignmentFactor);
			break;
		case AiBehavior.DefendSettlement:
			result *= ((mobileParty.Party.MapEvent != null && mobileParty.MapEvent.MapEventSettlement == mobileParty.TargetSettlement) ? DefendingFactor : TravelingToAssignmentFactor);
			break;
		}
		return result;
	}

	public override float CalculatePatrollingScoreForSettlement(Settlement settlement, MobileParty mobileParty)
	{
		bool flag = mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty && mobileParty.Army.AIBehavior != Army.AIBehaviorFlags.Gathering && mobileParty.Army.AIBehavior != Army.AIBehaviorFlags.WaitingForArmyMembers;
		if (mobileParty.Army != null && !flag && mobileParty.Army.Cohesion > (float)mobileParty.Army.CohesionThresholdForDispersion)
		{
			return 0f;
		}
		float num = ((mobileParty.LeaderHero != null && mobileParty.LeaderHero.Clan != null && mobileParty.LeaderHero.Clan.Fiefs.Count > 0) ? (mobileParty.LeaderHero.RandomFloat(0.2f, 0.4f) + (float)TaleWorlds.Library.MathF.Min(4, mobileParty.LeaderHero.Clan.Fiefs.Count - 1) * 0.05f + mobileParty.LeaderHero.RandomFloatWithSeed((uint)CampaignTime.Now.ToHours, 0.2f)) : 0.5f);
		float num2 = 1f - num + ((mobileParty.LeaderHero?.Clan != null && (settlement.OwnerClan == mobileParty.LeaderHero.Clan || mobileParty.LeaderHero.Clan.Settlements.Count == 0)) ? num : 0f);
		float length = (settlement.Position2D - mobileParty.Position2D).Length;
		float num3 = 1f - (length + 50f) / Campaign.MapDiagonal;
		num3 = (mobileParty.IsBandit ? (num3 * num3 * num3 * num3 * num3 * num3) : (num3 * num3 * num3 * num3));
		float num4 = 1f;
		if (settlement.MapFaction == mobileParty.MapFaction)
		{
			float numberOfEnemiesSpottedAround = settlement.NumberOfEnemiesSpottedAround;
			float numberOfAlliesSpottedAround = settlement.NumberOfAlliesSpottedAround;
			float num5 = TaleWorlds.Library.MathF.Max(0f, numberOfEnemiesSpottedAround - numberOfAlliesSpottedAround * 0.25f);
			if (num5 > 1f)
			{
				int num6 = 0;
				foreach (WarPartyComponent warPartyComponent in mobileParty.MapFaction.WarPartyComponents)
				{
					MobileParty mobileParty2 = warPartyComponent.MobileParty;
					if (mobileParty2 != mobileParty && (mobileParty2.Army == null || mobileParty2.Army != mobileParty.Army) && (mobileParty2.Army == null || mobileParty2.Army.LeaderParty == mobileParty) && mobileParty2.DefaultBehavior == AiBehavior.PatrolAroundPoint && mobileParty2.TargetSettlement == settlement)
					{
						num6++;
					}
				}
				num4 += TaleWorlds.Library.MathF.Pow(TaleWorlds.Library.MathF.Min(10f, num5), 0.25f) - (float)num6;
			}
			else
			{
				num4 += num5;
			}
		}
		float num7 = ((mobileParty.Army != null && mobileParty.Army.LeaderParty != mobileParty) ? (((float)mobileParty.Army.CohesionThresholdForDispersion - mobileParty.Army.Cohesion) / (float)mobileParty.Army.CohesionThresholdForDispersion) : 1f);
		return (mobileParty.MapFaction.IsMinorFaction ? 1.5f : 1f) * num4 * num2 * num3 * num7 * 0.36f;
	}

	public override float GetTargetScoreForFaction(Settlement targetSettlement, Army.ArmyTypes missionType, MobileParty mobileParty, float ourStrength, int numberOfEnemyFactionSettlements = -1, float totalEnemyMobilePartyStrength = -1f)
	{
		float powerScore = 0f;
		float distanceScore = 0f;
		float settlementImportanceScore = 0f;
		return GetTargetScoreForFaction(targetSettlement, missionType, mobileParty, ourStrength, out powerScore, out distanceScore, out settlementImportanceScore, numberOfEnemyFactionSettlements, totalEnemyMobilePartyStrength);
	}

	private float GetTargetScoreForFaction(Settlement targetSettlement, Army.ArmyTypes missionType, MobileParty mobileParty, float ourStrength, out float powerScore, out float distanceScore, out float settlementImportanceScore, int numberOfEnemyFactionSettlements = -1, float totalEnemyMobilePartyStrength = -1f)
	{
		IFaction mapFaction = mobileParty.MapFaction;
		if (((missionType == Army.ArmyTypes.Besieger || missionType == Army.ArmyTypes.Raider) && !FactionManager.IsAtWarAgainstFaction(targetSettlement.MapFaction, mapFaction)) || (missionType == Army.ArmyTypes.Raider && (targetSettlement.Village.VillageState != 0 || targetSettlement.Party.MapEvent != null) && (mobileParty.MapEvent == null || mobileParty.MapEvent.MapEventSettlement != targetSettlement)) || (missionType == Army.ArmyTypes.Besieger && (targetSettlement.Party.MapEvent != null || targetSettlement.SiegeEvent != null) && (targetSettlement.SiegeEvent == null || targetSettlement.SiegeEvent.BesiegerCamp.LeaderParty.MapFaction != mobileParty.MapFaction) && (mobileParty.MapEvent == null || mobileParty.MapEvent.MapEventSettlement != targetSettlement)) || (missionType == Army.ArmyTypes.Defender && (targetSettlement.LastAttackerParty == null || !targetSettlement.LastAttackerParty.IsActive || targetSettlement.LastAttackerParty.MapFaction == mobileParty.MapFaction || targetSettlement.MapFaction != mobileParty.MapFaction)))
		{
			powerScore = 0f;
			distanceScore = 0f;
			settlementImportanceScore = 0f;
			return 0f;
		}
		if (mobileParty.Objective == MobileParty.PartyObjective.Defensive && (missionType == Army.ArmyTypes.Besieger || missionType == Army.ArmyTypes.Raider))
		{
			powerScore = 0f;
			distanceScore = 0f;
			settlementImportanceScore = 0f;
			return 0f;
		}
		if (mobileParty.Objective == MobileParty.PartyObjective.Aggressive && (missionType == Army.ArmyTypes.Defender || missionType == Army.ArmyTypes.Patrolling))
		{
			powerScore = 0f;
			distanceScore = 0f;
			settlementImportanceScore = 0f;
			return 0f;
		}
		if (missionType == Army.ArmyTypes.Defender)
		{
			MobileParty lastAttackerParty = targetSettlement.LastAttackerParty;
			if (lastAttackerParty == null || !mobileParty.MapFaction.IsAtWarWith(lastAttackerParty.MapFaction))
			{
				powerScore = 0f;
				distanceScore = 0f;
				settlementImportanceScore = 0f;
				return 0f;
			}
		}
		if (mobileParty.Army == null && missionType == Army.ArmyTypes.Besieger && ((targetSettlement.Party.MapEvent != null && targetSettlement.Party.MapEvent.AttackerSide.LeaderParty != mobileParty.Party) || (targetSettlement.Party.SiegeEvent != null && mobileParty.BesiegedSettlement != targetSettlement)))
		{
			powerScore = 0f;
			distanceScore = 0f;
			settlementImportanceScore = 0f;
			return 0f;
		}
		float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(mapFaction.FactionMidSettlement, targetSettlement);
		float distance2 = Campaign.Current.Models.MapDistanceModel.GetDistance(mobileParty, targetSettlement);
		float num = Campaign.MapDiagonalSquared;
		float num2 = Campaign.MapDiagonalSquared;
		int num3 = 0;
		int num4 = 0;
		Settlement settlement = null;
		Settlement settlement2 = null;
		foreach (Settlement settlement3 in mobileParty.MapFaction.Settlements)
		{
			if (settlement3.IsTown)
			{
				float num5 = settlement3.Position2D.DistanceSquared(targetSettlement.Position2D);
				if (num > num5)
				{
					num = num5;
					settlement = settlement3;
				}
				if (num2 > num5)
				{
					num2 = num5;
					settlement2 = settlement3;
				}
				num3++;
				num4++;
			}
			else if (settlement3.IsCastle)
			{
				float num6 = settlement3.Position2D.DistanceSquared(targetSettlement.Position2D);
				if (num2 > num6)
				{
					num2 = num6;
					settlement2 = settlement3;
				}
				num4++;
			}
		}
		if (settlement2 != null)
		{
			num2 = Campaign.Current.Models.MapDistanceModel.GetDistance(targetSettlement, settlement2);
		}
		if (settlement == settlement2)
		{
			num = num2;
		}
		else if (settlement != null)
		{
			num = Campaign.Current.Models.MapDistanceModel.GetDistance(targetSettlement, settlement);
		}
		float num7 = 1f;
		float num8 = TaleWorlds.Library.MathF.Min(2f, TaleWorlds.Library.MathF.Sqrt(num4)) / 2f;
		float num9 = TaleWorlds.Library.MathF.Min(2f, TaleWorlds.Library.MathF.Sqrt(num3)) / 2f;
		if (num8 > 0f && num9 < 1f)
		{
			num8 += 1f - num9;
		}
		num7 += 0.5f * (2f - (num8 + num9));
		float num10 = missionType switch
		{
			Army.ArmyTypes.Besieger => TaleWorlds.Library.MathF.Max(0f, distance - Campaign.AverageDistanceBetweenTwoFortifications) * 0.15f + distance2 * 0.15f * num7 + num * 0.5f * num9 + num2 * 0.2f * num8, 
			Army.ArmyTypes.Raider => TaleWorlds.Library.MathF.Max(0f, distance - Campaign.AverageDistanceBetweenTwoFortifications) * 0.15f + distance2 * 0.5f * num7 + num * 0.2f * num9 + num2 * 0.15f * num8, 
			_ => TaleWorlds.Library.MathF.Max(0f, distance - Campaign.AverageDistanceBetweenTwoFortifications) * 0.15f + distance2 * 0.5f * num7 + num * 0.25f * num9 + num2 * 0.1f * num8, 
		};
		float num11 = missionType switch
		{
			Army.ArmyTypes.Besieger => targetSettlement.IsTown ? ReasonableDistanceForBesiegingTown : ReasonableDistanceForBesiegingCastle, 
			Army.ArmyTypes.Defender => targetSettlement.IsVillage ? ReasonableDistanceForDefendingVillage : ReasonableDistanceForDefendingTownOrCastle, 
			_ => ReasonableDistanceForRaiding, 
		};
		distanceScore = ((num10 < num11) ? (1f + (1f - num10 / num11) * 0.5f) : (num11 / num10 * (num11 / num10) * ((missionType != Army.ArmyTypes.Defender) ? (num11 / num10) : 1f)));
		if (distanceScore < 0.1f)
		{
			powerScore = 0f;
			distanceScore = 0f;
			settlementImportanceScore = 0f;
			return 0f;
		}
		float num12 = 1f;
		if (mobileParty.Army != null && mobileParty.Army.Cohesion < 40f)
		{
			num12 *= mobileParty.Army.Cohesion / 40f;
		}
		if (num12 < 0.25f)
		{
			powerScore = 0f;
			distanceScore = 0f;
			settlementImportanceScore = 0f;
			return 0f;
		}
		if (missionType == Army.ArmyTypes.Defender)
		{
			float num13 = 0f;
			float num14 = 0f;
			foreach (WarPartyComponent warPartyComponent in mapFaction.WarPartyComponents)
			{
				MobileParty mobileParty2 = warPartyComponent.MobileParty;
				if (mobileParty2 == mobileParty || (mobileParty2.Army != null && mobileParty2.Army == mobileParty.Army) || mobileParty2.AttachedTo != null)
				{
					continue;
				}
				if (mobileParty2.Army != null)
				{
					Army army = mobileParty2.Army;
					if (((army.AIBehavior == Army.AIBehaviorFlags.Gathering || army.AIBehavior == Army.AIBehaviorFlags.WaitingForArmyMembers) && army.AiBehaviorObject == targetSettlement) || (army.AIBehavior != Army.AIBehaviorFlags.Gathering && army.AIBehavior != Army.AIBehaviorFlags.WaitingForArmyMembers && army.AiBehaviorObject == targetSettlement) || (army.LeaderParty.TargetParty != null && (army.LeaderParty.TargetParty == targetSettlement.LastAttackerParty || (army.LeaderParty.TargetParty.MapEvent != null && army.LeaderParty.TargetParty.MapEvent == targetSettlement.LastAttackerParty.MapEvent) || (army.LeaderParty.TargetParty.BesiegedSettlement != null && army.LeaderParty.TargetParty.BesiegedSettlement == targetSettlement.LastAttackerParty.BesiegedSettlement))))
					{
						num14 += army.TotalStrength;
					}
				}
				else if ((mobileParty2.DefaultBehavior == AiBehavior.DefendSettlement && mobileParty2.TargetSettlement == targetSettlement) || (mobileParty2.TargetParty != null && (mobileParty2.TargetParty == targetSettlement.LastAttackerParty || (mobileParty2.TargetParty.MapEvent != null && mobileParty2.TargetParty.MapEvent == targetSettlement.LastAttackerParty.MapEvent) || (mobileParty2.TargetParty.BesiegedSettlement != null && mobileParty2.TargetParty.BesiegedSettlement == targetSettlement.LastAttackerParty.BesiegedSettlement))))
				{
					num14 += mobileParty2.Party.TotalStrength;
				}
			}
			float num15 = 0f;
			float num16 = 0f;
			float num17 = 0f;
			MobileParty lastAttackerParty2 = targetSettlement.LastAttackerParty;
			if ((targetSettlement.LastAttackerParty.MapEvent != null && targetSettlement.LastAttackerParty.MapEvent.MapEventSettlement == targetSettlement) || targetSettlement.LastAttackerParty.BesiegedSettlement == targetSettlement)
			{
				LocatableSearchData<MobileParty> data = MobileParty.StartFindingLocatablesAroundPosition(targetSettlement.GatePosition, 6f);
				for (MobileParty mobileParty3 = MobileParty.FindNextLocatable(ref data); mobileParty3 != null; mobileParty3 = MobileParty.FindNextLocatable(ref data))
				{
					if (mobileParty3.Aggressiveness > 0f && mobileParty3.MapFaction == lastAttackerParty2.MapFaction)
					{
						num13 += ((mobileParty3.Aggressiveness > 0.5f) ? 1f : (mobileParty3.Aggressiveness * 2f)) * mobileParty3.Party.TotalStrength;
					}
				}
			}
			else
			{
				num13 = lastAttackerParty2.Army?.TotalStrength ?? lastAttackerParty2.Party.TotalStrength;
			}
			num15 = ourStrength + num14;
			num16 = TaleWorlds.Library.MathF.Max(100f, num13) * 1.1f;
			num17 = num16 * 2.5f;
			powerScore = ((num15 >= num17) ? (num17 / num15 * (num17 / num15)) : TaleWorlds.Library.MathF.Min(1f, num15 / num16 * (num15 / num16)));
			if (num15 < num16)
			{
				powerScore *= 0.9f;
			}
			if (ourStrength < num13)
			{
				powerScore *= TaleWorlds.Library.MathF.Pow(ourStrength / num13, 0.25f);
			}
		}
		else
		{
			float num18 = targetSettlement.Party.TotalStrength;
			float num19 = 0f;
			bool flag = Hero.MainHero.CurrentSettlement == targetSettlement;
			foreach (MobileParty party in targetSettlement.Parties)
			{
				if (party.Aggressiveness > 0.01f || party.IsGarrison || party.IsMilitia)
				{
					float num20 = ((party == MobileParty.MainParty) ? 0.5f : ((party.Army != null && party.Army.LeaderParty == MobileParty.MainParty) ? 0.8f : 1f));
					float num21 = (flag ? 0.8f : 1f);
					num18 += num20 * num21 * party.Party.TotalStrength;
					if (!party.IsGarrison && !party.IsMilitia && party.LeaderHero != null)
					{
						num19 += num20 * num21 * party.Party.TotalStrength;
					}
				}
			}
			float num22 = 0f;
			float num23 = 0f;
			num23 = ((missionType != 0 || mobileParty.BesiegedSettlement == targetSettlement) ? 1f : (targetSettlement.IsTown ? 4f : 3f));
			float num24 = TaleWorlds.Library.MathF.Min(1f, distance2 / DistanceOfMobilePartyDivider);
			num23 *= 1f - 0.6f * (1f - num24) * (1f - num24);
			if (num18 < 100f && missionType == Army.ArmyTypes.Besieger)
			{
				num23 *= 0.5f + 0.5f * (num18 / 100f);
			}
			if ((mobileParty.MapEvent == null || mobileParty.MapEvent.MapEventSettlement != targetSettlement) && targetSettlement.MapFaction.IsKingdomFaction)
			{
				if (numberOfEnemyFactionSettlements < 0)
				{
					numberOfEnemyFactionSettlements = targetSettlement.MapFaction.Settlements.Count;
				}
				if (totalEnemyMobilePartyStrength < 0f)
				{
					totalEnemyMobilePartyStrength = targetSettlement.MapFaction.TotalStrength;
				}
				totalEnemyMobilePartyStrength *= 0.5f;
				float b = (totalEnemyMobilePartyStrength - num19) / ((float)numberOfEnemyFactionSettlements + 10f);
				num22 = TaleWorlds.Library.MathF.Max(0f, b) * num23;
			}
			float num25 = ((missionType == Army.ArmyTypes.Besieger) ? (1.25f + 0.25f * (float)targetSettlement.Town.GetWallLevel()) : 1f);
			if (missionType == Army.ArmyTypes.Besieger && targetSettlement.Town.FoodStocks < 100f)
			{
				num25 -= 0.5f * (num25 - 1f) * ((100f - targetSettlement.Town.FoodStocks) / 100f);
			}
			float num26 = ((missionType == Army.ArmyTypes.Besieger && mobileParty.LeaderHero != null) ? (mobileParty.LeaderHero.RandomFloat(0.1f) + (TaleWorlds.Library.MathF.Max(TaleWorlds.Library.MathF.Min(1.2f, mobileParty.Aggressiveness), 0.8f) - 0.8f) * 0.5f) : 0f);
			float num27 = num18 * (num25 - num26) + num22 + 0.1f;
			if (ourStrength < num27 * ((missionType == Army.ArmyTypes.Besieger) ? 1f : 0.6f))
			{
				powerScore = 0f;
				settlementImportanceScore = 1f;
				return 0f;
			}
			float num28 = 0f;
			if ((missionType == Army.ArmyTypes.Besieger && distance2 < RaidDistanceLimit) || (missionType == Army.ArmyTypes.Raider && targetSettlement.Party.MapEvent != null))
			{
				LocatableSearchData<MobileParty> data2 = MobileParty.StartFindingLocatablesAroundPosition((mobileParty.SiegeEvent != null && mobileParty.SiegeEvent.BesiegedSettlement == targetSettlement) ? mobileParty.Position2D : targetSettlement.GatePosition, 9f);
				for (MobileParty mobileParty4 = MobileParty.FindNextLocatable(ref data2); mobileParty4 != null; mobileParty4 = MobileParty.FindNextLocatable(ref data2))
				{
					if (mobileParty4.CurrentSettlement != targetSettlement && mobileParty4.Aggressiveness > 0.01f && mobileParty4.MapFaction == targetSettlement.Party.MapFaction)
					{
						float num29 = ((mobileParty4 == MobileParty.MainParty || (mobileParty4.Army != null && mobileParty4.Army.LeaderParty == MobileParty.MainParty)) ? 0.5f : 1f);
						float num30 = 1f;
						if (mobileParty.MapEvent != null && mobileParty.MapEvent.MapEventSettlement == targetSettlement)
						{
							float num31 = mobileParty4.Position2D.Distance(mobileParty.Position2D);
							num30 = 1f - num31 / 16f;
						}
						num28 += num30 * mobileParty4.Party.TotalStrength * num29;
					}
				}
				if (num28 < ourStrength)
				{
					num28 = TaleWorlds.Library.MathF.Max(0f, num28 - ourStrength * 0.33f);
				}
				num27 += num28;
				num27 -= num22;
				if (targetSettlement.MapFaction.IsKingdomFaction)
				{
					if (numberOfEnemyFactionSettlements < 0)
					{
						numberOfEnemyFactionSettlements = targetSettlement.MapFaction.Settlements.Count;
					}
					if (totalEnemyMobilePartyStrength < 0f)
					{
						totalEnemyMobilePartyStrength = targetSettlement.MapFaction.TotalStrength;
					}
					totalEnemyMobilePartyStrength *= 0.5f;
					float b2 = (totalEnemyMobilePartyStrength - (num19 + num28)) / ((float)numberOfEnemyFactionSettlements + 10f);
					num22 = TaleWorlds.Library.MathF.Max(0f, b2) * num23;
				}
				num27 += num22;
			}
			float num32 = ((missionType == Army.ArmyTypes.Raider) ? 0.6f : 0.4f);
			float num33 = ((missionType == Army.ArmyTypes.Raider) ? 0.9f : 0.8f);
			float num34 = ((missionType == Army.ArmyTypes.Raider) ? 2.5f : 3f);
			float num35 = ourStrength / num27;
			powerScore = ((ourStrength > num27 * num34) ? 1f : ((num35 > 2f) ? (num33 + (1f - num33) * ((num35 - 2f) / (num34 - 2f))) : ((num35 > 1f) ? (num32 + (num33 - num32) * ((num35 - 1f) / 1f)) : (num32 * 0.9f * num35 * num35))));
		}
		powerScore = ((powerScore > 1f) ? 1f : powerScore);
		float num36 = ((missionType == Army.ArmyTypes.Raider) ? targetSettlement.GetSettlementValueForEnemyHero(mobileParty.LeaderHero) : targetSettlement.GetSettlementValueForFaction(mapFaction));
		float y = (targetSettlement.IsVillage ? 0.5f : 0.33f);
		settlementImportanceScore = TaleWorlds.Library.MathF.Pow(num36 / 50000f, y);
		float num37 = 1f;
		if (missionType == Army.ArmyTypes.Raider)
		{
			if (targetSettlement.Village.Bound.Town.FoodStocks < 100f)
			{
				settlementImportanceScore *= 1f + 0.3f * ((100f - targetSettlement.Village.Bound.Town.FoodStocks) / 100f);
			}
			settlementImportanceScore *= 1.5f;
			num37 += ((mobileParty.Army != null) ? 0.5f : 1f) * ((mobileParty.LeaderHero != null && mobileParty.LeaderHero.Clan != null && mobileParty.LeaderHero.Clan.Gold < 10000) ? ((10000f - (float)mobileParty.LeaderHero.Clan.Gold) / 20000f) : 0f);
		}
		float num38 = missionType switch
		{
			Army.ArmyTypes.Besieger => 0.8f, 
			Army.ArmyTypes.Defender => targetSettlement.IsVillage ? 1.28f : 1.28f, 
			_ => 0.28f * (1f + (1f - targetSettlement.SettlementHitPoints)), 
		};
		if (missionType == Army.ArmyTypes.Defender && ((targetSettlement.IsFortification && targetSettlement.LastAttackerParty.BesiegedSettlement != targetSettlement) || (!targetSettlement.IsFortification && targetSettlement.LastAttackerParty.MapEvent == null)))
		{
			MobileParty lastAttackerParty3 = targetSettlement.LastAttackerParty;
			float distance3 = Campaign.Current.Models.MapDistanceModel.GetDistance(lastAttackerParty3, targetSettlement);
			float num39 = TaleWorlds.Library.MathF.Min(GiveUpDistanceLimit, distance3) / GiveUpDistanceLimit;
			num38 = num39 * 0.8f + (1f - num39) * num38;
		}
		float num40 = 1f;
		if ((missionType == Army.ArmyTypes.Raider || missionType == Army.ArmyTypes.Besieger) && targetSettlement.OwnerClan != null && mobileParty.LeaderHero != null)
		{
			int relationWithClan = mobileParty.LeaderHero.Clan.GetRelationWithClan(targetSettlement.OwnerClan);
			if (relationWithClan > 0)
			{
				num40 = 1f - ((missionType == Army.ArmyTypes.Besieger) ? 0.4f : 0.8f) * (TaleWorlds.Library.MathF.Sqrt(relationWithClan) / 10f);
			}
			else if (relationWithClan < 0)
			{
				num40 = 1f + ((missionType == Army.ArmyTypes.Besieger) ? 0.1f : 0.05f) * (TaleWorlds.Library.MathF.Sqrt(-relationWithClan) / 10f);
			}
		}
		float num41 = 1f;
		if (mobileParty.MapFaction != null && mobileParty.MapFaction.IsKingdomFaction && mobileParty.MapFaction.Leader == Hero.MainHero && (missionType != Army.ArmyTypes.Defender || (targetSettlement.LastAttackerParty != null && targetSettlement.LastAttackerParty.MapFaction != Hero.MainHero.MapFaction)))
		{
			StanceLink stanceLink = ((missionType != Army.ArmyTypes.Defender) ? Hero.MainHero.MapFaction.GetStanceWith(targetSettlement.MapFaction) : Hero.MainHero.MapFaction.GetStanceWith(targetSettlement.LastAttackerParty.MapFaction));
			if (stanceLink != null)
			{
				if (stanceLink.BehaviorPriority == 1)
				{
					switch (missionType)
					{
					case Army.ArmyTypes.Besieger:
					case Army.ArmyTypes.Raider:
						num41 = 0.65f;
						break;
					case Army.ArmyTypes.Defender:
						num41 = 1.1f;
						break;
					}
				}
				else if (stanceLink.BehaviorPriority == 2 && (missionType == Army.ArmyTypes.Besieger || missionType == Army.ArmyTypes.Raider))
				{
					num41 = 1.3f;
				}
			}
		}
		float num42 = 1f;
		if (mobileParty.SiegeEvent != null && mobileParty.SiegeEvent.BesiegedSettlement == targetSettlement)
		{
			num42 = 4f;
		}
		float num43 = 1f;
		if (missionType == Army.ArmyTypes.Raider && mobileParty.MapEvent != null && mobileParty.MapEvent.IsRaid)
		{
			num43 = ((mobileParty.MapEvent.MapEventSettlement == targetSettlement) ? 1.3f : 0.3f);
		}
		float num44 = 1f;
		if (targetSettlement.SiegeEvent != null && targetSettlement.SiegeEvent.BesiegerCamp.LeaderParty.MapFaction == mobileParty.MapFaction)
		{
			float num45 = targetSettlement.SiegeEvent.BesiegerCamp.GetInvolvedPartiesForEventType().Sum((PartyBase x) => x.TotalStrength) / targetSettlement.GetInvolvedPartiesForEventType().Sum((PartyBase x) => x.TotalStrength);
			num44 += Math.Max(0f, 3f - num45);
		}
		float num46 = num40 * distanceScore * powerScore * settlementImportanceScore * num37 * num38 * num41 * num12 * num42 * num43 * num44;
		if (mobileParty.Objective == MobileParty.PartyObjective.Defensive && missionType == Army.ArmyTypes.Defender)
		{
			num46 *= 1.2f;
		}
		else if (mobileParty.Objective == MobileParty.PartyObjective.Aggressive && (missionType == Army.ArmyTypes.Besieger || missionType == Army.ArmyTypes.Raider))
		{
			num46 *= 1.2f;
		}
		return (num46 < 0f) ? 0f : num46;
	}
}
