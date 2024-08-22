using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.Actions;

public static class LeaveTroopsToSettlementAction
{
	private static void ApplyInternal(MobileParty mobileParty, Settlement settlement, int numberOfTroopsWillBeLeft, bool archersAreHighPriority)
	{
		TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
		int num = numberOfTroopsWillBeLeft;
		for (int i = 0; i < MathF.Abs(numberOfTroopsWillBeLeft); i++)
		{
			CharacterObject characterObject = null;
			num--;
			int num2 = ((!archersAreHighPriority) ? 1 : 4);
			for (int j = 0; j < num2; j++)
			{
				PartyBase party;
				int stackIndex;
				if (numberOfTroopsWillBeLeft > 0)
				{
					int partyRank = MBRandom.RandomInt(mobileParty.MemberRoster.TotalRegulars);
					CharacterObject character = null;
					mobileParty.Party.GetCharacterFromPartyRank(partyRank, out character, out party, out stackIndex, includeWoundeds: true);
					if (character.IsRanged)
					{
						characterObject = character;
						break;
					}
					if (!archersAreHighPriority || !character.IsMounted || characterObject == null)
					{
						characterObject = character;
					}
				}
				else
				{
					int partyRank2 = settlement.Town.GarrisonParty.MemberRoster.TotalHeroes + MBRandom.RandomInt(settlement.Town.GarrisonParty.MemberRoster.TotalRegulars);
					CharacterObject character2 = null;
					settlement.Town.GarrisonParty.Party.GetCharacterFromPartyRank(partyRank2, out character2, out party, out stackIndex, includeWoundeds: true);
					characterObject = character2;
				}
			}
			if (numberOfTroopsWillBeLeft > 0)
			{
				foreach (TroopRosterElement item in mobileParty.Party.MemberRoster.GetTroopRoster())
				{
					if (item.Character == characterObject)
					{
						if (item.WoundedNumber > 0)
						{
							troopRoster.AddToCounts(characterObject, 1, insertAtFront: false, 1);
							mobileParty.MemberRoster.AddToCounts(characterObject, -1, insertAtFront: false, -1);
						}
						else
						{
							troopRoster.AddToCounts(characterObject, 1);
							mobileParty.AddElementToMemberRoster(characterObject, -1);
						}
						break;
					}
				}
				continue;
			}
			foreach (TroopRosterElement item2 in settlement.Town.GarrisonParty.MemberRoster.GetTroopRoster())
			{
				if (item2.Character == characterObject)
				{
					if (item2.Number - item2.WoundedNumber > 0)
					{
						troopRoster.AddToCounts(characterObject, 1);
						settlement.Town.GarrisonParty.MemberRoster.AddToCounts(characterObject, -1);
					}
					else
					{
						troopRoster.AddToCounts(characterObject, 1, insertAtFront: false, 1);
						settlement.Town.GarrisonParty.MemberRoster.AddToCounts(characterObject, -1, insertAtFront: false, -1);
					}
					break;
				}
			}
		}
		if (troopRoster.Count <= 0)
		{
			return;
		}
		if (numberOfTroopsWillBeLeft > 0)
		{
			CampaignEventDispatcher.Instance.OnTroopGivenToSettlement(mobileParty.LeaderHero, settlement, troopRoster);
			if (settlement.Town.GarrisonParty == null)
			{
				settlement.AddGarrisonParty();
			}
			while (troopRoster.Count > 0)
			{
				TroopRosterElement elementCopyAtIndex = troopRoster.GetElementCopyAtIndex(0);
				troopRoster.AddToCounts(elementCopyAtIndex.Character, -elementCopyAtIndex.Number);
				settlement.Town.GarrisonParty.MemberRoster.AddToCounts(elementCopyAtIndex.Character, elementCopyAtIndex.Number);
			}
			if (mobileParty.LeaderHero == null || settlement.OwnerClan == mobileParty.LeaderHero.Clan)
			{
				return;
			}
			float num3 = 0f;
			foreach (TroopRosterElement item3 in troopRoster.GetTroopRoster())
			{
				float troopPower = Campaign.Current.Models.MilitaryPowerModel.GetTroopPower(item3.Character, BattleSideEnum.Defender, MapEvent.PowerCalculationContext.Siege, 0f);
				num3 += troopPower * (float)item3.Number;
			}
			GainKingdomInfluenceAction.ApplyForLeavingTroopToGarrison(mobileParty.LeaderHero, num3 / 3f);
		}
		else
		{
			while (troopRoster.Count > 0)
			{
				TroopRosterElement elementCopyAtIndex2 = troopRoster.GetElementCopyAtIndex(0);
				troopRoster.AddToCounts(elementCopyAtIndex2.Character, -elementCopyAtIndex2.Number);
				mobileParty.MemberRoster.AddToCounts(elementCopyAtIndex2.Character, elementCopyAtIndex2.Number);
			}
		}
	}

	public static void Apply(MobileParty mobileParty, Settlement settlement, int number, bool archersAreHighPriority)
	{
		ApplyInternal(mobileParty, settlement, number, archersAreHighPriority);
	}
}
