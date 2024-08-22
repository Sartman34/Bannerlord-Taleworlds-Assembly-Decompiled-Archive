using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace TaleWorlds.CampaignSystem.Actions;

public static class SetPartyAiAction
{
	private enum SetPartyAiActionDetail
	{
		GoToSettlement,
		PatrolAroundSettlement,
		RaidSettlement,
		BesiegeSettlement,
		EngageParty,
		GoAroundParty,
		DefendParty,
		EscortParty
	}

	private static void ApplyInternal(MobileParty owner, IMapPoint subject, SetPartyAiActionDetail detail)
	{
		switch (detail)
		{
		case SetPartyAiActionDetail.GoToSettlement:
			if (owner.DefaultBehavior != AiBehavior.GoToSettlement || owner.TargetSettlement != subject)
			{
				owner.Ai.SetMoveGoToSettlement((Settlement)subject);
			}
			if (owner.Army != null)
			{
				owner.Army.ArmyType = Army.ArmyTypes.Patrolling;
				owner.Army.AIBehavior = Army.AIBehaviorFlags.GoToSettlement;
				owner.Army.AiBehaviorObject = subject;
			}
			break;
		case SetPartyAiActionDetail.PatrolAroundSettlement:
			if (owner.DefaultBehavior != AiBehavior.PatrolAroundPoint || owner.TargetSettlement != subject)
			{
				owner.Ai.SetMovePatrolAroundSettlement((Settlement)subject);
			}
			if (owner.Army != null)
			{
				owner.Army.ArmyType = Army.ArmyTypes.Patrolling;
				owner.Army.AIBehavior = Army.AIBehaviorFlags.Patrolling;
				owner.Army.AiBehaviorObject = subject;
			}
			break;
		case SetPartyAiActionDetail.RaidSettlement:
			if (owner.DefaultBehavior != AiBehavior.RaidSettlement || owner.TargetSettlement != subject)
			{
				owner.Ai.SetMoveRaidSettlement((Settlement)subject);
				if (owner.Army != null)
				{
					owner.Army.AIBehavior = Army.AIBehaviorFlags.TravellingToAssignment;
					owner.Army.ArmyType = Army.ArmyTypes.Raider;
					owner.Army.AiBehaviorObject = subject;
				}
			}
			break;
		case SetPartyAiActionDetail.BesiegeSettlement:
			if (owner.DefaultBehavior != AiBehavior.BesiegeSettlement || owner.TargetSettlement != subject)
			{
				owner.Ai.SetMoveBesiegeSettlement((Settlement)subject);
				if (owner.Army != null)
				{
					owner.Army.AIBehavior = Army.AIBehaviorFlags.TravellingToAssignment;
					owner.Army.ArmyType = Army.ArmyTypes.Besieger;
					owner.Army.AiBehaviorObject = subject;
				}
			}
			break;
		case SetPartyAiActionDetail.GoAroundParty:
			if (owner.DefaultBehavior != AiBehavior.GoAroundParty || owner != subject)
			{
				owner.Ai.SetMoveGoAroundParty((MobileParty)subject);
			}
			break;
		case SetPartyAiActionDetail.EngageParty:
			if (owner.DefaultBehavior != AiBehavior.EngageParty || owner != subject)
			{
				owner.Ai.SetMoveEngageParty((MobileParty)subject);
			}
			break;
		case SetPartyAiActionDetail.DefendParty:
			if (owner.DefaultBehavior != AiBehavior.DefendSettlement || owner != subject)
			{
				owner.Ai.SetMoveDefendSettlement((Settlement)subject);
				if (owner.Army != null)
				{
					owner.Army.AIBehavior = Army.AIBehaviorFlags.Defending;
					owner.Army.ArmyType = Army.ArmyTypes.Defender;
					owner.Army.AiBehaviorObject = subject;
				}
			}
			break;
		case SetPartyAiActionDetail.EscortParty:
			if (owner.DefaultBehavior != AiBehavior.EscortParty || owner.TargetParty != subject)
			{
				MobileParty mobileParty = (MobileParty)subject;
				owner.Ai.SetMoveEscortParty(mobileParty);
				if (owner.IsLordParty && mobileParty.IsLordParty && owner != MobileParty.MainParty && owner.Army == null && mobileParty.Army != null)
				{
					owner.Army = mobileParty.Army;
				}
			}
			break;
		}
	}

	public static void GetAction(MobileParty owner, Settlement settlement)
	{
		ApplyInternal(owner, settlement, SetPartyAiActionDetail.GoToSettlement);
	}

	public static void GetActionForVisitingSettlement(MobileParty owner, Settlement settlement)
	{
		ApplyInternal(owner, settlement, SetPartyAiActionDetail.GoToSettlement);
	}

	public static void GetActionForPatrollingAroundSettlement(MobileParty owner, Settlement settlement)
	{
		ApplyInternal(owner, settlement, SetPartyAiActionDetail.PatrolAroundSettlement);
	}

	public static void GetActionForRaidingSettlement(MobileParty owner, Settlement settlement)
	{
		ApplyInternal(owner, settlement, SetPartyAiActionDetail.RaidSettlement);
	}

	public static void GetActionForBesiegingSettlement(MobileParty owner, Settlement settlement)
	{
		ApplyInternal(owner, settlement, SetPartyAiActionDetail.BesiegeSettlement);
	}

	public static void GetActionForEngagingParty(MobileParty owner, MobileParty mobileParty)
	{
		ApplyInternal(owner, mobileParty, SetPartyAiActionDetail.EngageParty);
	}

	public static void GetActionForGoingAroundParty(MobileParty owner, MobileParty mobileParty)
	{
		ApplyInternal(owner, mobileParty, SetPartyAiActionDetail.GoAroundParty);
	}

	public static void GetActionForDefendingSettlement(MobileParty owner, Settlement settlement)
	{
		ApplyInternal(owner, settlement, SetPartyAiActionDetail.DefendParty);
	}

	public static void GetActionForEscortingParty(MobileParty owner, MobileParty mobileParty)
	{
		ApplyInternal(owner, mobileParty, SetPartyAiActionDetail.EscortParty);
	}
}
