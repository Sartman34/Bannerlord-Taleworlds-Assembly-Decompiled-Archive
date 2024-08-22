using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.Actions;

public static class MergePartiesAction
{
	private static void ApplyInternal(PartyBase majorParty, PartyBase joinedParty)
	{
		int numberOfAllMembers = joinedParty.NumberOfAllMembers;
		majorParty.AddMembers(joinedParty.MemberRoster);
		majorParty.AddPrisoners(joinedParty.PrisonRoster);
		DestroyPartyAction.Apply(null, joinedParty.MobileParty);
		if (majorParty == PartyBase.MainParty)
		{
			MBTextManager.SetTextVariable("NUMBER_OF_TROOPS", numberOfAllMembers);
			MBInformationManager.AddQuickInformation(new TextObject("{=511LONpe}{NUMBER_OF_TROOPS} troops joined you."));
		}
	}

	public static void Apply(PartyBase majorParty, PartyBase joinedParty)
	{
		ApplyInternal(majorParty, joinedParty);
	}
}
