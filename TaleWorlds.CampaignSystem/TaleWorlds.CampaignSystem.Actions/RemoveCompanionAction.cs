using TaleWorlds.CampaignSystem.Party;

namespace TaleWorlds.CampaignSystem.Actions;

public static class RemoveCompanionAction
{
	public enum RemoveCompanionDetail
	{
		Fire,
		Death,
		AfterQuest,
		ByTurningToLord
	}

	private static void ApplyInternal(Clan clan, Hero companion, RemoveCompanionDetail detail)
	{
		PartyBase partyBase = ((companion.PartyBelongedTo != null) ? companion.PartyBelongedTo.Party : companion.CurrentSettlement?.Party);
		companion.CompanionOf = null;
		if (partyBase != null && detail != RemoveCompanionDetail.ByTurningToLord)
		{
			if (partyBase.LeaderHero != companion)
			{
				partyBase.MemberRoster.AddToCounts(companion.CharacterObject, -1);
			}
			else
			{
				partyBase.MemberRoster.AddToCounts(companion.CharacterObject, -1);
				partyBase.MobileParty.RemovePartyLeader();
				if (partyBase.MemberRoster.Count == 0)
				{
					DestroyPartyAction.Apply(null, partyBase.MobileParty);
				}
				else
				{
					DisbandPartyAction.StartDisband(partyBase.MobileParty);
				}
			}
		}
		if (detail == RemoveCompanionDetail.Fire)
		{
			companion.CompanionOf = null;
			if (companion.PartyBelongedToAsPrisoner != null)
			{
				EndCaptivityAction.ApplyByEscape(companion);
			}
			else
			{
				MakeHeroFugitiveAction.Apply(companion);
			}
			if (companion.IsWanderer)
			{
				companion.ResetEquipments();
			}
		}
		if (companion.GovernorOf != null)
		{
			ChangeGovernorAction.RemoveGovernorOf(companion);
		}
		CampaignEventDispatcher.Instance.OnCompanionRemoved(companion, detail);
	}

	public static void ApplyByFire(Clan clan, Hero companion)
	{
		ApplyInternal(clan, companion, RemoveCompanionDetail.Fire);
	}

	public static void ApplyAfterQuest(Clan clan, Hero companion)
	{
		ApplyInternal(clan, companion, RemoveCompanionDetail.AfterQuest);
	}

	public static void ApplyByDeath(Clan clan, Hero companion)
	{
		ApplyInternal(clan, companion, RemoveCompanionDetail.Death);
	}

	public static void ApplyByByTurningToLord(Clan clan, Hero companion)
	{
		ApplyInternal(clan, companion, RemoveCompanionDetail.ByTurningToLord);
	}
}
