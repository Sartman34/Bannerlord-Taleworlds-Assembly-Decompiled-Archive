using System;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class DesertionCampaignBehavior : CampaignBehaviorBase
{
	private int _numberOfDesertersFromLordParty;

	public override void RegisterEvents()
	{
		CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener(this, DailyTickParty);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	public void DailyTickParty(MobileParty mobileParty)
	{
		if (Campaign.Current.DesertionEnabled && mobileParty.IsActive && !mobileParty.IsDisbanding && mobileParty.Party.MapEvent == null && (mobileParty.IsLordParty || mobileParty.IsCaravan))
		{
			TroopRoster desertedTroopList = null;
			if (mobileParty.MemberRoster.TotalRegulars > 0)
			{
				PartiesCheckDesertionDueToMorale(mobileParty, ref desertedTroopList);
				PartiesCheckDesertionDueToPartySizeExceedsPaymentRatio(mobileParty, ref desertedTroopList);
			}
			if (desertedTroopList != null && desertedTroopList.Count > 0)
			{
				CampaignEventDispatcher.Instance.OnTroopsDeserted(mobileParty, desertedTroopList);
			}
			if (mobileParty.Party.NumberOfAllMembers <= 0)
			{
				DestroyPartyAction.Apply(null, mobileParty);
			}
		}
	}

	private void PartiesCheckForTroopDesertionEffectiveMorale(MobileParty party, int stackNo, int desertIfMoraleIsLessThanValue, out int numberOfDeserters, out int numberOfWoundedDeserters)
	{
		int num = 0;
		int num2 = 0;
		float morale = party.Morale;
		if (party.IsActive && party.MemberRoster.Count > 0)
		{
			TroopRosterElement elementCopyAtIndex = party.MemberRoster.GetElementCopyAtIndex(stackNo);
			float num3 = TaleWorlds.Library.MathF.Pow((float)elementCopyAtIndex.Character.Level / 100f, 0.1f * (((float)desertIfMoraleIsLessThanValue - morale) / (float)desertIfMoraleIsLessThanValue));
			for (int i = 0; i < elementCopyAtIndex.Number - elementCopyAtIndex.WoundedNumber; i++)
			{
				if (num3 < MBRandom.RandomFloat)
				{
					num++;
				}
			}
			for (int j = 0; j < elementCopyAtIndex.WoundedNumber; j++)
			{
				if (num3 < MBRandom.RandomFloat)
				{
					num2++;
				}
			}
		}
		numberOfDeserters = num;
		numberOfWoundedDeserters = num2;
	}

	private void PartiesCheckDesertionDueToPartySizeExceedsPaymentRatio(MobileParty mobileParty, ref TroopRoster desertedTroopList)
	{
		int partySizeLimit = mobileParty.Party.PartySizeLimit;
		if ((mobileParty.IsLordParty || mobileParty.IsCaravan) && mobileParty.Party.NumberOfAllMembers > partySizeLimit && mobileParty != MobileParty.MainParty && mobileParty.MapEvent == null)
		{
			int num = mobileParty.Party.NumberOfAllMembers - partySizeLimit;
			for (int i = 0; i < num; i++)
			{
				CharacterObject character = mobileParty.MapFaction.BasicTroop;
				int num2 = 99;
				bool flag = false;
				for (int j = 0; j < mobileParty.MemberRoster.Count; j++)
				{
					CharacterObject characterAtIndex = mobileParty.MemberRoster.GetCharacterAtIndex(j);
					if (!characterAtIndex.IsHero && characterAtIndex.Level < num2 && mobileParty.MemberRoster.GetElementNumber(j) > 0)
					{
						num2 = characterAtIndex.Level;
						character = characterAtIndex;
						flag = mobileParty.MemberRoster.GetElementWoundedNumber(j) > 0;
					}
				}
				if (num2 < 99)
				{
					if (flag)
					{
						mobileParty.MemberRoster.AddToCounts(character, -1, insertAtFront: false, -1);
					}
					else
					{
						mobileParty.MemberRoster.AddToCounts(character, -1);
					}
				}
			}
		}
		bool flag2 = mobileParty.IsWageLimitExceeded();
		if (!(mobileParty.Party.NumberOfAllMembers > mobileParty.LimitedPartySize || flag2))
		{
			return;
		}
		int numberOfDeserters = Campaign.Current.Models.PartyDesertionModel.GetNumberOfDeserters(mobileParty);
		int num4;
		for (int k = 0; k < numberOfDeserters; k += num4)
		{
			if (mobileParty.MemberRoster.TotalRegulars <= 0)
			{
				break;
			}
			int stackNo = -1;
			int num3 = 9;
			num4 = 1;
			for (int l = 0; l < mobileParty.MemberRoster.Count; l++)
			{
				if (mobileParty.MemberRoster.TotalRegulars <= 0)
				{
					break;
				}
				CharacterObject characterAtIndex2 = mobileParty.MemberRoster.GetCharacterAtIndex(l);
				int elementNumber = mobileParty.MemberRoster.GetElementNumber(l);
				if (!characterAtIndex2.IsHero && elementNumber > 0 && characterAtIndex2.Tier < num3)
				{
					num3 = characterAtIndex2.Tier;
					stackNo = l;
					num4 = Math.Min(elementNumber, numberOfDeserters - k);
				}
			}
			MobilePartyHelper.DesertTroopsFromParty(mobileParty, stackNo, num4, 0, ref desertedTroopList);
		}
	}

	private bool PartiesCheckDesertionDueToMorale(MobileParty party, ref TroopRoster desertedTroopList)
	{
		int moraleThresholdForTroopDesertion = Campaign.Current.Models.PartyDesertionModel.GetMoraleThresholdForTroopDesertion(party);
		bool result = false;
		if (party.Morale < (float)moraleThresholdForTroopDesertion && party.MemberRoster.TotalManCount > 0)
		{
			for (int num = party.MemberRoster.Count - 1; num >= 0; num--)
			{
				if (!party.MemberRoster.GetCharacterAtIndex(num).IsHero)
				{
					int numberOfDeserters = 0;
					int numberOfWoundedDeserters = 0;
					PartiesCheckForTroopDesertionEffectiveMorale(party, num, moraleThresholdForTroopDesertion, out numberOfDeserters, out numberOfWoundedDeserters);
					if (numberOfDeserters + numberOfWoundedDeserters > 0)
					{
						if (party.IsLordParty && party.MapFaction.IsKingdomFaction)
						{
							_numberOfDesertersFromLordParty++;
						}
						result = true;
						MobilePartyHelper.DesertTroopsFromParty(party, num, numberOfDeserters, numberOfWoundedDeserters, ref desertedTroopList);
					}
				}
			}
		}
		return result;
	}
}
