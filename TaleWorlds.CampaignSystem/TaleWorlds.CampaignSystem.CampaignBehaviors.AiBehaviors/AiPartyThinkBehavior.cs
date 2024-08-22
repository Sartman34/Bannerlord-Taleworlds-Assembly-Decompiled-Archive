using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;

public class AiPartyThinkBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.TickPartialHourlyAiEvent.AddNonSerializedListener(this, PartyHourlyAiTick);
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
		CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
		CampaignEvents.MakePeace.AddNonSerializedListener(this, OnMakePeace);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void PartyHourlyAiTick(MobileParty mobileParty)
	{
		if (mobileParty.Ai.IsDisabled || mobileParty.Ai.DoNotMakeNewDecisions)
		{
			return;
		}
		bool flag = mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty;
		int num = ((flag || mobileParty.Ai.RethinkAtNextHourlyTick || (mobileParty.MapEvent != null && (mobileParty.MapEvent.IsRaid || mobileParty.MapEvent.IsSiegeAssault))) ? 1 : 6);
		if (flag && MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty == mobileParty && (mobileParty.CurrentSettlement != null || (mobileParty.LastVisitedSettlement != null && mobileParty.MapEvent == null && mobileParty.LastVisitedSettlement.Position2D.Distance(mobileParty.Position2D) < 1f)))
		{
			num = 6;
		}
		if (mobileParty.Ai.HourCounter % num == 0 && mobileParty != MobileParty.MainParty && (mobileParty.MapEvent == null || (mobileParty.Party == mobileParty.MapEvent.AttackerSide.LeaderParty && (mobileParty.MapEvent.IsRaid || mobileParty.MapEvent.IsSiegeAssault))))
		{
			mobileParty.Ai.HourCounter = 0;
			Army.AIBehaviorFlags aIBehaviorFlags = (flag ? mobileParty.Army.AIBehavior : Army.AIBehaviorFlags.Unassigned);
			IMapPoint mapPoint = (flag ? mobileParty.Army.AiBehaviorObject : null);
			mobileParty.Ai.RethinkAtNextHourlyTick = false;
			PartyThinkParams thinkParamsCache = mobileParty.ThinkParamsCache;
			thinkParamsCache.Reset(mobileParty);
			CampaignEventDispatcher.Instance.AiHourlyTick(mobileParty, thinkParamsCache);
			AIBehaviorTuple aIBehaviorTuple = new AIBehaviorTuple(null, AiBehavior.Hold);
			AIBehaviorTuple aIBehaviorTuple2 = new AIBehaviorTuple(null, AiBehavior.Hold);
			float num2 = -1f;
			float num3 = -1f;
			foreach (var aIBehaviorScore in thinkParamsCache.AIBehaviorScores)
			{
				float item = aIBehaviorScore.Item2;
				if (item > num2)
				{
					num2 = item;
					(aIBehaviorTuple, _) = aIBehaviorScore;
				}
				if (item > num3 && !aIBehaviorScore.Item1.WillGatherArmy)
				{
					num3 = item;
					(aIBehaviorTuple2, _) = aIBehaviorScore;
				}
			}
			if (mobileParty.DefaultBehavior == AiBehavior.Hold || mobileParty.Ai.RethinkAtNextHourlyTick || (thinkParamsCache.CurrentObjectiveValue < 0.05f && (mobileParty.DefaultBehavior == AiBehavior.BesiegeSettlement || mobileParty.DefaultBehavior == AiBehavior.RaidSettlement || mobileParty.DefaultBehavior == AiBehavior.DefendSettlement)))
			{
				num2 = 1f;
			}
			double num4 = ((aIBehaviorTuple.AiBehavior == AiBehavior.PatrolAroundPoint || aIBehaviorTuple.AiBehavior == AiBehavior.GoToSettlement) ? 0.03 : 0.1);
			num4 *= (double)(aIBehaviorTuple.WillGatherArmy ? 2f : ((mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty) ? 0.33f : 1f));
			bool flag2 = mobileParty.Army != null;
			for (int i = 0; i < num; i++)
			{
				if (flag2)
				{
					break;
				}
				flag2 = MBRandom.RandomFloat < num2;
			}
			if (((double)num2 > num4 && flag2) || (num2 > 0.01f && mobileParty.MapEvent == null && mobileParty.Army == null && mobileParty.DefaultBehavior == AiBehavior.Hold))
			{
				if (mobileParty.MapEvent != null && mobileParty.Party == mobileParty.MapEvent.AttackerSide.LeaderParty && !thinkParamsCache.DoNotChangeBehavior && (aIBehaviorTuple.Party != mobileParty.MapEvent.MapEventSettlement || (aIBehaviorTuple.AiBehavior != AiBehavior.RaidSettlement && aIBehaviorTuple.AiBehavior != AiBehavior.BesiegeSettlement && aIBehaviorTuple.AiBehavior != AiBehavior.AssaultSettlement)))
				{
					if (PlayerEncounter.Current != null && PlayerEncounter.Battle == mobileParty.MapEvent)
					{
						PlayerEncounter.Finish();
					}
					if (mobileParty.MapEvent != null)
					{
						mobileParty.MapEvent.FinalizeEvent();
					}
					if (mobileParty.SiegeEvent != null)
					{
						mobileParty.SiegeEvent.FinalizeSiegeEvent();
					}
					if (mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty)
					{
						foreach (MobileParty party in mobileParty.Army.Parties)
						{
							party.Ai.SetMoveEscortParty(mobileParty);
						}
					}
				}
				if ((double)num2 <= num4)
				{
					aIBehaviorTuple = aIBehaviorTuple2;
				}
				bool flag3 = aIBehaviorTuple.AiBehavior == AiBehavior.RaidSettlement || aIBehaviorTuple.AiBehavior == AiBehavior.BesiegeSettlement || aIBehaviorTuple.AiBehavior == AiBehavior.DefendSettlement || aIBehaviorTuple.AiBehavior == AiBehavior.PatrolAroundPoint;
				if (mobileParty.Army != null && mobileParty.Army.LeaderParty != mobileParty && aIBehaviorTuple.AiBehavior != AiBehavior.EscortParty && (mobileParty.Army.LeaderParty.MapEvent == null || mobileParty.Army.LeaderParty.MapEvent.MapEventSettlement == null || aIBehaviorTuple.Party != mobileParty.Army.LeaderParty.MapEvent.MapEventSettlement))
				{
					mobileParty.Army = null;
				}
				if (mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty && (mobileParty.CurrentSettlement == null || mobileParty.CurrentSettlement.SiegeEvent == null) && !(aIBehaviorTuple.AiBehavior == AiBehavior.GoAroundParty || aIBehaviorTuple.AiBehavior == AiBehavior.PatrolAroundPoint || aIBehaviorTuple.AiBehavior == AiBehavior.GoToSettlement || flag3))
				{
					DisbandArmyAction.ApplyByUnknownReason(mobileParty.Army);
				}
				if (flag3 && mobileParty.Army == null && aIBehaviorTuple.WillGatherArmy && !mobileParty.LeaderHero.Clan.IsUnderMercenaryService)
				{
					bool flag4 = MBRandom.RandomFloat < num2;
					if (aIBehaviorTuple.AiBehavior == AiBehavior.DefendSettlement || flag4)
					{
						Army.ArmyTypes selectedArmyType = ((aIBehaviorTuple.AiBehavior != AiBehavior.BesiegeSettlement) ? ((aIBehaviorTuple.AiBehavior == AiBehavior.RaidSettlement) ? Army.ArmyTypes.Raider : Army.ArmyTypes.Defender) : Army.ArmyTypes.Besieger);
						((Kingdom)mobileParty.MapFaction).CreateArmy(mobileParty.LeaderHero, aIBehaviorTuple.Party as Settlement, selectedArmyType);
					}
				}
				else if (!thinkParamsCache.DoNotChangeBehavior)
				{
					if (aIBehaviorTuple.AiBehavior == AiBehavior.PatrolAroundPoint)
					{
						SetPartyAiAction.GetActionForPatrollingAroundSettlement(mobileParty, (Settlement)aIBehaviorTuple.Party);
					}
					else if (aIBehaviorTuple.AiBehavior == AiBehavior.GoToSettlement)
					{
						if (MobilePartyHelper.GetCurrentSettlementOfMobilePartyForAICalculation(mobileParty) != aIBehaviorTuple.Party)
						{
							SetPartyAiAction.GetActionForVisitingSettlement(mobileParty, (Settlement)aIBehaviorTuple.Party);
						}
					}
					else if (aIBehaviorTuple.AiBehavior == AiBehavior.EscortParty)
					{
						SetPartyAiAction.GetActionForEscortingParty(mobileParty, (MobileParty)aIBehaviorTuple.Party);
					}
					else if (aIBehaviorTuple.AiBehavior == AiBehavior.RaidSettlement)
					{
						if (mobileParty.MapEvent == null || !mobileParty.MapEvent.IsRaid || mobileParty.MapEvent.MapEventSettlement != aIBehaviorTuple.Party)
						{
							SetPartyAiAction.GetActionForRaidingSettlement(mobileParty, (Settlement)aIBehaviorTuple.Party);
						}
					}
					else if (aIBehaviorTuple.AiBehavior == AiBehavior.BesiegeSettlement)
					{
						SetPartyAiAction.GetActionForBesiegingSettlement(mobileParty, (Settlement)aIBehaviorTuple.Party);
					}
					else if (aIBehaviorTuple.AiBehavior == AiBehavior.DefendSettlement && mobileParty.CurrentSettlement != aIBehaviorTuple.Party)
					{
						SetPartyAiAction.GetActionForDefendingSettlement(mobileParty, (Settlement)aIBehaviorTuple.Party);
					}
					else if (aIBehaviorTuple.AiBehavior == AiBehavior.GoAroundParty)
					{
						SetPartyAiAction.GetActionForGoingAroundParty(mobileParty, (MobileParty)aIBehaviorTuple.Party);
					}
				}
			}
			else if (mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty && mobileParty.Army.AIBehavior != Army.AIBehaviorFlags.Gathering && mobileParty.Army.AIBehavior != Army.AIBehaviorFlags.WaitingForArmyMembers)
			{
				DisbandArmyAction.ApplyByUnknownReason(mobileParty.Army);
			}
			else if (mobileParty.Army != null && mobileParty.CurrentSettlement == null && mobileParty != mobileParty.Army.LeaderParty && !thinkParamsCache.DoNotChangeBehavior)
			{
				SetPartyAiAction.GetActionForEscortingParty(mobileParty, mobileParty.Army.LeaderParty);
			}
			if (MobileParty.MainParty.Army != null && mobileParty.Equals(MobileParty.MainParty.Army?.LeaderParty) && (aIBehaviorFlags != mobileParty.Army.AIBehavior || mobileParty.Army.AiBehaviorObject != mapPoint))
			{
				Army.ArmyLeaderThinkReason behaviorChangeExplanation = Army.GetBehaviorChangeExplanation(aIBehaviorFlags, mobileParty.Army.AIBehavior);
				CampaignEventDispatcher.Instance.OnArmyLeaderThink(mobileParty.LeaderHero, behaviorChangeExplanation);
			}
		}
		mobileParty.Ai.HourCounter++;
	}

	private void OnMakePeace(IFaction faction1, IFaction faction2, MakePeaceAction.MakePeaceDetail detail)
	{
		if (faction1.IsKingdomFaction && faction2.IsKingdomFaction)
		{
			FactionHelper.FinishAllRelatedHostileActions((Kingdom)faction1, (Kingdom)faction2);
		}
		else if (faction1.IsKingdomFaction || faction2.IsKingdomFaction)
		{
			if (faction1.IsKingdomFaction)
			{
				FactionHelper.FinishAllRelatedHostileActionsOfFactionToFaction((Clan)faction2, (Kingdom)faction1);
				FactionHelper.FinishAllRelatedHostileActionsOfFactionToFaction((Kingdom)faction1, (Clan)faction2);
			}
			else
			{
				FactionHelper.FinishAllRelatedHostileActionsOfFactionToFaction((Clan)faction1, (Kingdom)faction2);
				FactionHelper.FinishAllRelatedHostileActionsOfFactionToFaction((Kingdom)faction2, (Clan)faction1);
			}
		}
		else
		{
			FactionHelper.FinishAllRelatedHostileActions((Clan)faction1, (Clan)faction2);
		}
	}

	private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
	{
		foreach (WarPartyComponent warPartyComponent in clan.WarPartyComponents)
		{
			if (warPartyComponent.MobileParty.TargetSettlement != null)
			{
				CheckMobilePartyActionAccordingToSettlement(warPartyComponent.MobileParty, warPartyComponent.MobileParty.TargetSettlement);
			}
		}
	}

	private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail)
	{
		foreach (WarPartyComponent warPartyComponent in faction1.WarPartyComponents)
		{
			if (warPartyComponent.MobileParty.TargetSettlement != null)
			{
				CheckMobilePartyActionAccordingToSettlement(warPartyComponent.MobileParty, warPartyComponent.MobileParty.TargetSettlement);
			}
		}
		foreach (WarPartyComponent warPartyComponent2 in faction2.WarPartyComponents)
		{
			if (warPartyComponent2.MobileParty.TargetSettlement != null)
			{
				CheckMobilePartyActionAccordingToSettlement(warPartyComponent2.MobileParty, warPartyComponent2.MobileParty.TargetSettlement);
			}
		}
	}

	private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
	{
		HandlePartyActionsAfterSettlementOwnerChange(settlement);
	}

	private void HandlePartyActionsAfterSettlementOwnerChange(Settlement settlement)
	{
		foreach (MobileParty item in MobileParty.All)
		{
			CheckMobilePartyActionAccordingToSettlement(item, settlement);
		}
	}

	private void CheckMobilePartyActionAccordingToSettlement(MobileParty mobileParty, Settlement settlement)
	{
		if (mobileParty.BesiegedSettlement == settlement)
		{
			return;
		}
		if (mobileParty.Army == null)
		{
			Settlement targetSettlement = mobileParty.TargetSettlement;
			if (targetSettlement == null || (targetSettlement != settlement && (!targetSettlement.IsVillage || targetSettlement.Village.Bound != settlement)))
			{
				return;
			}
			if (mobileParty.MapEvent == null)
			{
				if (mobileParty.CurrentSettlement == null)
				{
					mobileParty.Ai.SetMoveModeHold();
					return;
				}
				mobileParty.Ai.SetMoveGoToSettlement(mobileParty.CurrentSettlement);
				mobileParty.Ai.RecalculateShortTermAi();
			}
			else
			{
				mobileParty.Ai.RethinkAtNextHourlyTick = true;
			}
		}
		else
		{
			if (mobileParty.Army.LeaderParty != mobileParty)
			{
				return;
			}
			Army army = mobileParty.Army;
			if (army.AiBehaviorObject == settlement || (army.AiBehaviorObject != null && ((Settlement)army.AiBehaviorObject).IsVillage && ((Settlement)army.AiBehaviorObject).Village.Bound == settlement))
			{
				army.AIBehavior = Army.AIBehaviorFlags.Unassigned;
				army.AiBehaviorObject = null;
				if (army.LeaderParty.MapEvent == null)
				{
					army.LeaderParty.Ai.SetMoveModeHold();
				}
				else
				{
					army.LeaderParty.Ai.RethinkAtNextHourlyTick = true;
				}
				army.FinishArmyObjective();
			}
		}
	}
}
