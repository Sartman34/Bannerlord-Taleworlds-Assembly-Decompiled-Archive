using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Helpers;

public static class PartyBaseHelper
{
	public static TextObject GetPartySizeText(PartyBase party)
	{
		if (party.NumberOfHealthyMembers == party.NumberOfAllMembers)
		{
			return new TextObject(party.NumberOfHealthyMembers.ToString());
		}
		MBTextManager.SetTextVariable("HEALTHY_NUM", party.NumberOfHealthyMembers);
		MBTextManager.SetTextVariable("WOUNDED_NUM", party.NumberOfAllMembers - party.NumberOfHealthyMembers);
		return GameTexts.FindText("str_party_health");
	}

	public static TextObject GetPartySizeText(int healtyNumber, int woundedNumber, bool isInspected)
	{
		string seed = "";
		if (isInspected)
		{
			if (woundedNumber == 0)
			{
				return new TextObject(healtyNumber);
			}
			TextObject textObject = GameTexts.FindText("str_party_health");
			textObject.SetTextVariable("HEALTHY_NUM", healtyNumber);
			textObject.SetTextVariable("WOUNDED_NUM", woundedNumber);
			return textObject;
		}
		string text = new int[4] { 0, 10, 100, 1000 }.Where((int t) => t < healtyNumber + woundedNumber).Aggregate(seed, (string current, int t) => current + "?");
		return new TextObject("{=!}" + text);
	}

	public static float FindPartySizeNormalLimit(MobileParty mobileParty)
	{
		return MathF.Max(0.1f, (float)mobileParty.LimitedPartySize / (float)mobileParty.Party.PartySizeLimit);
	}

	public static Hero GetCaptainOfTroop(PartyBase affectorParty, CharacterObject affectorCharacter)
	{
		foreach (TroopRosterElement item in affectorParty.MemberRoster.GetTroopRoster())
		{
			if (item.Character.IsHero && !item.Character.HeroObject.IsWounded && MBRandom.RandomFloat < 0.2f)
			{
				return item.Character.HeroObject;
			}
		}
		return affectorParty.LeaderHero;
	}

	public static string PrintRosterContents(TroopRoster roster)
	{
		MBStringBuilder mBStringBuilder = default(MBStringBuilder);
		mBStringBuilder.Initialize(16, "PrintRosterContents");
		for (int i = 0; i < roster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = roster.GetElementCopyAtIndex(i);
			TextObject value;
			if (elementCopyAtIndex.Character.IsHero)
			{
				value = elementCopyAtIndex.Character.Name;
			}
			else
			{
				TextObject textObject = new TextObject("{=fW0XS9JC}{ELEMENT_NUMBER} {ELEMENT_CHAR_NAME}");
				textObject.SetTextVariable("ELEMENT_NUMBER", elementCopyAtIndex.Number);
				textObject.SetTextVariable("ELEMENT_CHAR_NAME", elementCopyAtIndex.Character.Name);
				value = textObject;
			}
			mBStringBuilder.Append(value);
			if (i < roster.Count - 1)
			{
				mBStringBuilder.Append(", ");
			}
		}
		return mBStringBuilder.ToStringAndRelease();
	}

	public static TextObject PrintSummarisedItemRoster(ItemRoster items)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		ItemObject itemObject = null;
		ItemObject itemObject2 = null;
		ItemObject itemObject3 = null;
		ItemObject itemObject4 = null;
		for (int i = 0; i < items.Count; i++)
		{
			ItemRosterElement elementCopyAtIndex = items.GetElementCopyAtIndex(i);
			ItemObject item = elementCopyAtIndex.EquipmentElement.Item;
			int? obj;
			if (item.IsTradeGood)
			{
				if (itemObject3 != null)
				{
					_ = itemObject3.Value;
					if (0 == 0)
					{
						obj = itemObject3?.Value;
						goto IL_0083;
					}
				}
				obj = -1;
				goto IL_0083;
			}
			int? obj2;
			if (item.HasArmorComponent)
			{
				if (itemObject2 != null)
				{
					_ = itemObject2.Value;
					if (0 == 0)
					{
						obj2 = itemObject2?.Value;
						goto IL_0100;
					}
				}
				obj2 = -1;
				goto IL_0100;
			}
			int? obj3;
			if (item.WeaponComponent != null)
			{
				if (itemObject != null)
				{
					_ = itemObject.Value;
					if (0 == 0)
					{
						obj3 = itemObject?.Value;
						goto IL_017a;
					}
				}
				obj3 = -1;
				goto IL_017a;
			}
			int? obj4;
			if (itemObject4 != null)
			{
				_ = itemObject4.Value;
				if (0 == 0)
				{
					obj4 = itemObject4?.Value;
					goto IL_01e8;
				}
			}
			obj4 = -1;
			goto IL_01e8;
			IL_01e8:
			if (obj4 < item.Value)
			{
				num8 = elementCopyAtIndex.Amount;
				itemObject4 = item;
			}
			num7 += elementCopyAtIndex.Amount;
			continue;
			IL_0100:
			if (obj2 < item.Value)
			{
				num4 = elementCopyAtIndex.Amount;
				itemObject2 = item;
			}
			num3 += elementCopyAtIndex.Amount;
			continue;
			IL_017a:
			if (obj3 < item.Value)
			{
				num2 = elementCopyAtIndex.Amount;
				itemObject = item;
			}
			num += elementCopyAtIndex.Amount;
			continue;
			IL_0083:
			if (obj < item.Value)
			{
				num6 = elementCopyAtIndex.Amount;
				itemObject3 = item;
			}
			num5 += elementCopyAtIndex.Amount;
		}
		num5 -= num6;
		num3 -= num4;
		num -= num2;
		num7 -= num8;
		int[] array = new int[4] { num6, num4, num2, num8 };
		int[] array2 = new int[4] { num5, num3, num, num7 };
		ItemObject[] array3 = new ItemObject[4] { itemObject3, itemObject2, itemObject, itemObject4 };
		TextObject[,] array4 = new TextObject[4, 2]
		{
			{
				new TextObject("{=nc9KELFA}trade goods"),
				new TextObject("{=eVcvaxz6}trade good")
			},
			{
				new TextObject("{=YJJwR5PB}pieces of armour"),
				new TextObject("{=pF47ldtJ}piece of armour")
			},
			{
				new TextObject("{=ADabRUeh}weapons"),
				new TextObject("{=Rs8xhY46}weapon")
			},
			{
				new TextObject("{=Py5jvZWL}type of items"),
				new TextObject("{=2HmzaFVK}type of item")
			}
		};
		List<TextObject> list = new List<TextObject>();
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] != 0)
			{
				TextObject textObject = new TextObject("{=eBea9Ext}{VALUABLE_ITEM_COUNT} {VALUABLE_ITEM_NAME}{?IS_THERE_OTHER_ITEMS} and {?PLURAL}{OTHER_ITEMS_COUNT}other {OTHER_ITEMS_CATEGORY_PLURAL}{?}an other {OTHER_ITEMS_CATEGORY_SINGULAR}{\\?}{?}{\\?}");
				textObject.SetTextVariable("OTHER_ITEMS_COUNT", array2[j]);
				textObject.SetTextVariable("OTHER_ITEMS_CATEGORY_PLURAL", array4[j, 0]);
				textObject.SetTextVariable("OTHER_ITEMS_CATEGORY_SINGULAR", array4[j, 1]);
				textObject.SetTextVariable("VALUABLE_ITEM_COUNT", array[j]);
				textObject.SetTextVariable("VALUABLE_ITEM_NAME", array3[j].Name);
				textObject.SetTextVariable("IS_THERE_OTHER_ITEMS", (array2[j] > 0) ? 1 : 0);
				textObject.SetTextVariable("PLURAL", (array2[j] != 1) ? 1 : 0);
				list.Add(textObject);
			}
		}
		if (list.Count <= 0)
		{
			return TextObject.Empty;
		}
		return GameTexts.GameTextHelper.MergeTextObjectsWithComma(list, includeAnd: false);
	}

	public static TextObject PrintRegularTroopCategories(TroopRoster roster)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < roster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = roster.GetElementCopyAtIndex(i);
			CharacterObject character = elementCopyAtIndex.Character;
			if (character.IsHero || elementCopyAtIndex.Number == 0)
			{
				continue;
			}
			if (character.IsInfantry)
			{
				num += elementCopyAtIndex.Number;
			}
			else if (character.IsRanged)
			{
				if (character.IsMounted)
				{
					num4 += elementCopyAtIndex.Number;
				}
				else
				{
					num2 += elementCopyAtIndex.Number;
				}
			}
			else if (character.IsMounted)
			{
				num3 += elementCopyAtIndex.Number;
			}
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		if (num != 0)
		{
			dictionary.Add("Infantry", num);
		}
		if (num2 != 0)
		{
			dictionary.Add("Ranged", num2);
		}
		if (num3 != 0)
		{
			dictionary.Add("Cavalry", num3);
		}
		if (num4 != 0)
		{
			dictionary.Add("HorseArcher", num4);
		}
		List<TextObject> list = new List<TextObject>();
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			TextObject textObject = new TextObject("{=ksTDGuXs}{TROOP_TYPE_COUNT} {TROOP_TYPE} {?TROOP_TYPE_COUNT>1}troops{?}troop{\\?}");
			textObject.SetTextVariable("TROOP_TYPE_COUNT", item.Value);
			textObject.SetTextVariable("TROOP_TYPE", GameTexts.FindText("str_troop_type_name", item.Key));
			list.Add(textObject);
		}
		return GameTexts.GameTextHelper.MergeTextObjectsWithComma(list, includeAnd: true);
	}

	public static CharacterObject GetVisualPartyLeader(PartyBase party)
	{
		if (party == null)
		{
			return null;
		}
		if (party.LeaderHero == null)
		{
			TroopRoster memberRoster = party.MemberRoster;
			if ((object)memberRoster == null || memberRoster.TotalManCount <= 0)
			{
				return null;
			}
			return party.MemberRoster.GetCharacterAtIndex(0);
		}
		return party.LeaderHero.CharacterObject;
	}

	public static int GetSpeedLimitation(ItemRoster partyItemRoster, out ItemObject speedLimitationItem)
	{
		speedLimitationItem = null;
		int num = 100;
		foreach (ItemRosterElement item in partyItemRoster)
		{
			if (item.EquipmentElement.Item != null && item.EquipmentElement.Item.IsAnimal && num > item.EquipmentElement.GetModifiedMountSpeed(in EquipmentElement.Invalid))
			{
				num = item.EquipmentElement.GetModifiedMountSpeed(in EquipmentElement.Invalid);
				speedLimitationItem = item.EquipmentElement.Item;
			}
		}
		return num;
	}

	public static bool DoesSurrenderIsLogicalForParty(MobileParty ourParty, MobileParty enemyParty, float acceptablePowerRatio = 0.1f)
	{
		float num = enemyParty.Party.TotalStrength;
		float num2 = ourParty.Party.TotalStrength;
		LocatableSearchData<MobileParty> data = Campaign.Current.MobilePartyLocator.StartFindingLocatablesAroundPosition(enemyParty.Position2D, 6f);
		for (MobileParty mobileParty = Campaign.Current.MobilePartyLocator.FindNextLocatable(ref data); mobileParty != null; mobileParty = Campaign.Current.MobilePartyLocator.FindNextLocatable(ref data))
		{
			if (mobileParty != enemyParty && mobileParty != ourParty && mobileParty.Aggressiveness > 0.01f && mobileParty.CurrentSettlement == null)
			{
				if (mobileParty.MapFaction == enemyParty.MapFaction || (mobileParty.MapFaction.IsBanditFaction && enemyParty.MapFaction.IsBanditFaction))
				{
					num += mobileParty.Party.TotalStrength;
				}
				else if (mobileParty.MapFaction == ourParty.MapFaction || (mobileParty.MapFaction.IsBanditFaction && ourParty.MapFaction.IsBanditFaction))
				{
					num2 += mobileParty.Party.TotalStrength;
				}
			}
		}
		int num3 = 0;
		foreach (ItemRosterElement item in ourParty.ItemRoster)
		{
			num3 += item.EquipmentElement.GetBaseValue() * item.Amount;
		}
		int num4 = num3 + ((ourParty.LeaderHero != null) ? ourParty.LeaderHero.Gold : ourParty.PartyTradeGold);
		float num5 = 0.75f + 0.25f * MathF.Sqrt((float)num4 / 1000f);
		float num6 = num * acceptablePowerRatio * MathF.Min(2f, 1f / num5) * ourParty.Party.RandomFloat(0.5f, 1f);
		return num2 < num6;
	}

	public static bool HasFeat(PartyBase party, FeatObject feat)
	{
		if (party == null)
		{
			return false;
		}
		if (party.LeaderHero != null)
		{
			return party.LeaderHero.Culture.HasFeat(feat);
		}
		if (party.Culture != null)
		{
			return party.Culture.HasFeat(feat);
		}
		if (party.Owner != null)
		{
			return party.Owner.Culture.HasFeat(feat);
		}
		if (party.Settlement != null)
		{
			return party.Settlement.Culture.HasFeat(feat);
		}
		return false;
	}
}
