using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;

namespace TaleWorlds.CampaignSystem.Actions;

public static class LeaveSettlementAction
{
	public static void ApplyForParty(MobileParty mobileParty)
	{
		Settlement currentSettlement = mobileParty.CurrentSettlement;
		if (mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty)
		{
			foreach (MobileParty attachedParty in mobileParty.Army.LeaderParty.AttachedParties)
			{
				if (attachedParty == MobileParty.MainParty && PlayerEncounter.Current != null)
				{
					PlayerEncounter.Finish();
				}
				else if (attachedParty.CurrentSettlement == currentSettlement)
				{
					ApplyForParty(attachedParty);
				}
			}
			foreach (MobileParty party in mobileParty.Army.Parties)
			{
				if (party != mobileParty && party.MapEvent == null && party.CurrentSettlement == null)
				{
					party.Ai.SetMoveModeHold();
					party.Ai.SetMoveEscortParty(mobileParty);
				}
			}
		}
		if (mobileParty == MobileParty.MainParty && (MobileParty.MainParty.Army == null || MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty))
		{
			mobileParty.Ai.SetMoveModeHold();
		}
		mobileParty.CurrentSettlement = null;
		currentSettlement.SettlementComponent.OnPartyLeft(mobileParty);
		CampaignEventDispatcher.Instance.OnSettlementLeft(mobileParty, currentSettlement);
	}

	public static void ApplyForCharacterOnly(Hero hero)
	{
		Settlement currentSettlement = hero.CurrentSettlement;
		if (hero != null)
		{
			hero.StayingInSettlement = null;
		}
		Location location = currentSettlement.LocationComplex?.GetLocationOfCharacter(hero);
		if (location != null && location.GetLocationCharacter(hero) != null)
		{
			currentSettlement.LocationComplex.RemoveCharacterIfExists(hero);
			PlayerEncounter.LocationEncounter?.RemoveAccompanyingCharacter(hero);
		}
	}
}
