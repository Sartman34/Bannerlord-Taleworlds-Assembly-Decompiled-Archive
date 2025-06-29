using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;

namespace TaleWorlds.CampaignSystem.Actions;

public static class TakePrisonerAction
{
	private static void ApplyInternal(PartyBase capturerParty, Hero prisonerCharacter, bool isEventCalled = true)
	{
		if (prisonerCharacter.PartyBelongedTo != null)
		{
			if (prisonerCharacter.PartyBelongedTo.LeaderHero == prisonerCharacter)
			{
				prisonerCharacter.PartyBelongedTo.RemovePartyLeader();
			}
			prisonerCharacter.PartyBelongedTo.MemberRoster.RemoveTroop(prisonerCharacter.CharacterObject);
		}
		prisonerCharacter.CaptivityStartTime = CampaignTime.Now;
		prisonerCharacter.ChangeState(Hero.CharacterStates.Prisoner);
		capturerParty.AddPrisoner(prisonerCharacter.CharacterObject, 1);
		if (prisonerCharacter == Hero.MainHero)
		{
			PlayerCaptivity.StartCaptivity(capturerParty);
		}
		if (capturerParty.IsSettlement && prisonerCharacter.StayingInSettlement != null)
		{
			prisonerCharacter.StayingInSettlement = null;
		}
		if (isEventCalled)
		{
			CampaignEventDispatcher.Instance.OnHeroPrisonerTaken(capturerParty, prisonerCharacter);
		}
	}

	public static void Apply(PartyBase capturerParty, Hero prisonerCharacter)
	{
		ApplyInternal(capturerParty, prisonerCharacter);
	}

	public static void ApplyByTakenFromPartyScreen(FlattenedTroopRoster roster)
	{
		foreach (FlattenedTroopRosterElement item in roster)
		{
			if (item.Troop.IsHero)
			{
				ApplyInternal(PartyBase.MainParty, item.Troop.HeroObject);
			}
		}
		CampaignEventDispatcher.Instance.OnPrisonerTaken(roster);
	}
}
