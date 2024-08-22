using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.CampaignSystem.SceneInformationPopupTypes;

public static class CampaignSceneNotificationHelper
{
	public static SceneNotificationData.SceneNotificationCharacter GetBodyguardOfCulture(CultureObject culture)
	{
		string objectName = culture.StringId switch
		{
			"battania" => "battanian_fian_champion", 
			"khuzait" => "khuzait_khans_guard", 
			"vlandia" => "vlandian_banner_knight", 
			"aserai" => "mamluke_palace_guard", 
			"sturgia" => "sturgian_veteran_warrior", 
			"empire" => "imperial_legionary", 
			_ => "fighter_sturgia", 
		};
		return new SceneNotificationData.SceneNotificationCharacter(MBObjectManager.Instance.GetObject<CharacterObject>(objectName));
	}

	public static void RemoveWeaponsFromEquipment(ref Equipment equipment, bool removeHelmet = false, bool removeShoulder = false)
	{
		for (int i = 0; i < 5; i++)
		{
			equipment[i] = EquipmentElement.Invalid;
		}
		if (removeHelmet)
		{
			equipment[5] = EquipmentElement.Invalid;
		}
		if (removeShoulder)
		{
			equipment[9] = EquipmentElement.Invalid;
		}
	}

	public static string GetChildStageEquipmentIDFromCulture(CultureObject childCulture)
	{
		return childCulture.StringId switch
		{
			"empire" => "comingofage_kid_emp_cutscene_template", 
			"aserai" => "comingofage_kid_ase_cutscene_template", 
			"battania" => "comingofage_kid_bat_cutscene_template", 
			"khuzait" => "comingofage_kid_khu_cutscene_template", 
			"sturgia" => "comingofage_kid_stu_cutscene_template", 
			"vlandia" => "comingofage_kid_vla_cutscene_template", 
			_ => "comingofage_kid_emp_cutscene_template", 
		};
	}

	public static CharacterObject GetRandomTroopForCulture(CultureObject culture)
	{
		string objectName = "imperial_recruit";
		if (culture != null)
		{
			List<CharacterObject> list = new List<CharacterObject>();
			if (culture.BasicTroop != null)
			{
				list.Add(culture.BasicTroop);
			}
			if (culture.EliteBasicTroop != null)
			{
				list.Add(culture.EliteBasicTroop);
			}
			if (culture.MeleeMilitiaTroop != null)
			{
				list.Add(culture.MeleeMilitiaTroop);
			}
			if (culture.MeleeEliteMilitiaTroop != null)
			{
				list.Add(culture.MeleeEliteMilitiaTroop);
			}
			if (culture.RangedMilitiaTroop != null)
			{
				list.Add(culture.RangedMilitiaTroop);
			}
			if (culture.RangedEliteMilitiaTroop != null)
			{
				list.Add(culture.RangedEliteMilitiaTroop);
			}
			if (list.Count > 0)
			{
				return list[MBRandom.RandomInt(list.Count)];
			}
		}
		return Game.Current.ObjectManager.GetObject<CharacterObject>(objectName);
	}

	public static IEnumerable<Hero> GetMilitaryAudienceForHero(Hero hero, bool includeClanLeader = true, bool onlyClanMembers = false)
	{
		if (hero.Clan != null)
		{
			if (includeClanLeader && (hero.Clan.Leader?.IsAlive ?? false) && hero != hero.Clan.Leader)
			{
				yield return hero.Clan.Leader;
			}
			IOrderedEnumerable<Hero> orderedEnumerable = hero.Clan.Heroes.OrderBy((Hero h) => h.Level);
			foreach (Hero item in orderedEnumerable)
			{
				if (item != hero.Clan.Leader && item.IsAlive && !item.IsChild && item != hero)
				{
					yield return item;
				}
			}
		}
		if (onlyClanMembers)
		{
			yield break;
		}
		IOrderedEnumerable<Hero> orderedEnumerable2 = Hero.AllAliveHeroes.OrderBy((Hero h) => CharacterRelationManager.GetHeroRelation(hero, h));
		foreach (Hero item2 in orderedEnumerable2)
		{
			bool flag = item2 != null && item2.Clan != hero.Clan;
			if (item2.IsFriend(item2) && item2.IsLord && !item2.IsChild && item2 != hero && !flag)
			{
				yield return item2;
			}
		}
	}

	public static IEnumerable<Hero> GetMilitaryAudienceForKingdom(Kingdom kingdom, bool includeKingdomLeader = true)
	{
		if (includeKingdomLeader && (kingdom.Leader?.IsAlive ?? false))
		{
			yield return kingdom.Leader;
		}
		IOrderedEnumerable<Hero> orderedEnumerable = (from h in kingdom.Leader?.Clan.Heroes.WhereQ((Hero h) => h != h.Clan.Kingdom.Leader)
			orderby h.GetRelationWithPlayer()
			select h);
		foreach (Hero item in orderedEnumerable)
		{
			if (!item.IsChild && item != Hero.MainHero && item.IsAlive)
			{
				yield return item;
			}
		}
	}

	public static TextObject GetFormalDayAndSeasonText(CampaignTime time)
	{
		TextObject textObject = new TextObject("{=CpsPq6WD}the {DAY_ORDINAL} day of {SEASON_NAME}");
		TextObject variable = GameTexts.FindText("str_season_" + time.GetSeasonOfYear);
		TextObject variable2 = GameTexts.FindText("str_ordinal_number", (time.GetDayOfSeason + 1).ToString());
		textObject.SetTextVariable("SEASON_NAME", variable);
		textObject.SetTextVariable("DAY_ORDINAL", variable2);
		return textObject;
	}

	public static TextObject GetFormalNameForKingdom(Kingdom kingdom)
	{
		if (kingdom.Culture.StringId.Equals("empire", StringComparison.InvariantCultureIgnoreCase))
		{
			return kingdom.Name;
		}
		if (kingdom.Leader == Hero.MainHero)
		{
			return kingdom.InformalName;
		}
		return FactionHelper.GetFormalNameForFactionCulture(kingdom.Culture);
	}

	public static SceneNotificationData.SceneNotificationCharacter CreateNotificationCharacterFromHero(Hero hero, Equipment overridenEquipment = null, bool useCivilian = false, BodyProperties overriddenBodyProperties = default(BodyProperties), uint overriddenColor1 = uint.MaxValue, uint overriddenColor2 = uint.MaxValue, bool useHorse = false)
	{
		if (overriddenColor1 == uint.MaxValue)
		{
			overriddenColor1 = hero.MapFaction?.Color ?? hero.CharacterObject.Culture.Color;
		}
		if (overriddenColor2 == uint.MaxValue)
		{
			overriddenColor2 = hero.MapFaction?.Color2 ?? hero.CharacterObject.Culture.Color2;
		}
		if (overridenEquipment == null)
		{
			overridenEquipment = (useCivilian ? hero.CivilianEquipment : hero.BattleEquipment);
		}
		return new SceneNotificationData.SceneNotificationCharacter(hero.CharacterObject, overridenEquipment, overriddenBodyProperties, useCivilian, overriddenColor1, overriddenColor2, useHorse);
	}

	public static ItemObject GetDefaultHorseItem()
	{
		return Game.Current.ObjectManager.GetObjectTypeList<ItemObject>().First((ItemObject i) => i.ItemType == ItemObject.ItemTypeEnum.Horse && i.HasHorseComponent && i.HorseComponent.IsMount && i.HorseComponent.Monster.StringId == "horse");
	}
}
