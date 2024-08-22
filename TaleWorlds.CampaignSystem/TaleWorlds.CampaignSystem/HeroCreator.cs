using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem;

public static class HeroCreator
{
	public static Hero CreateHeroAtOccupation(Occupation neededOccupation, Settlement forcedHomeSettlement = null)
	{
		Settlement settlement = forcedHomeSettlement ?? SettlementHelper.GetRandomTown();
		IEnumerable<CharacterObject> enumerable = settlement.Culture.NotableAndWandererTemplates.Where((CharacterObject x) => x.Occupation == neededOccupation);
		int num = 0;
		foreach (CharacterObject item in enumerable)
		{
			int num2 = item.GetTraitLevel(DefaultTraits.Frequency) * 10;
			num += ((num2 > 0) ? num2 : 100);
		}
		if (!enumerable.Any())
		{
			return null;
		}
		CharacterObject template = null;
		int num3 = settlement.RandomIntWithSeed((uint)settlement.Notables.Count, 1, num);
		foreach (CharacterObject item2 in enumerable)
		{
			int num4 = item2.GetTraitLevel(DefaultTraits.Frequency) * 10;
			num3 -= ((num4 > 0) ? num4 : 100);
			if (num3 < 0)
			{
				template = item2;
				break;
			}
		}
		Hero hero = CreateSpecialHero(template, settlement);
		if (hero.HomeSettlement.IsVillage && hero.HomeSettlement.Village.Bound != null && hero.HomeSettlement.Village.Bound.IsCastle)
		{
			float value = MBRandom.RandomFloat * 20f;
			hero.AddPower(value);
		}
		if (neededOccupation != Occupation.Wanderer)
		{
			hero.ChangeState(Hero.CharacterStates.Active);
		}
		if (neededOccupation != Occupation.Wanderer)
		{
			EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
		}
		if (neededOccupation != Occupation.Wanderer)
		{
			int amount = 10000;
			GiveGoldAction.ApplyBetweenCharacters(null, hero, amount, disableNotification: true);
		}
		if (hero.Template?.HeroObject != null && hero.Template.HeroObject.Clan != null && hero.Template.HeroObject.Clan.IsMinorFaction)
		{
			hero.SupporterOf = hero.Template.HeroObject.Clan;
		}
		else
		{
			hero.SupporterOf = HeroHelper.GetRandomClanForNotable(hero);
		}
		if (neededOccupation != Occupation.Wanderer)
		{
			AddRandomVarianceToTraits(hero);
		}
		return hero;
	}

	private static Hero CreateNewHero(CharacterObject template, int age = -1)
	{
		Debug.Print("creating hero from template with id: " + template.StringId);
		CharacterObject newCharacter = CharacterObject.CreateFrom(template);
		Hero hero = Hero.CreateHero(newCharacter.StringId);
		hero.SetCharacterObject(newCharacter);
		newCharacter.HeroObject = hero;
		CampaignTime birthDay;
		switch (age)
		{
		case -1:
			birthDay = HeroHelper.GetRandomBirthDayForAge(Campaign.Current.Models.AgeModel.HeroComesOfAge + MBRandom.RandomInt(30));
			break;
		case 0:
			birthDay = CampaignTime.Now;
			break;
		default:
			if (hero.IsWanderer)
			{
				age = (int)template.Age;
				if (age < 20)
				{
					foreach (TraitObject item in TraitObject.All)
					{
						int num = 12 + 4 * template.GetTraitLevel(item);
						if (age < num)
						{
							age = num;
						}
					}
				}
				birthDay = HeroHelper.GetRandomBirthDayForAge(age);
			}
			else
			{
				birthDay = HeroHelper.GetRandomBirthDayForAge(age);
			}
			break;
		}
		newCharacter.HeroObject.SetBirthDay(birthDay);
		Settlement settlement = SettlementHelper.FindRandomSettlement((Settlement x) => x.IsTown && (newCharacter.Culture.StringId == "neutral_culture" || x.Culture == newCharacter.Culture));
		if (settlement == null)
		{
			settlement = SettlementHelper.FindRandomSettlement((Settlement x) => x.IsTown);
		}
		newCharacter.HeroObject.BornSettlement = settlement;
		newCharacter.HeroObject.StaticBodyProperties = BodyProperties.GetRandomBodyProperties(template.Race, template.IsFemale, template.GetBodyPropertiesMin(), template.GetBodyPropertiesMax(), 0, MBRandom.RandomInt(), newCharacter.HairTags, newCharacter.BeardTags, newCharacter.TattooTags).StaticProperties;
		newCharacter.HeroObject.Weight = 0f;
		newCharacter.HeroObject.Build = 0f;
		if (newCharacter.Age >= (float)Campaign.Current.Models.AgeModel.HeroComesOfAge)
		{
			newCharacter.HeroObject.HeroDeveloper.InitializeHeroDeveloper();
		}
		hero.PreferredUpgradeFormation = GetRandomPreferredUpgradeFormation();
		return newCharacter.HeroObject;
	}

	public static Hero CreateSpecialHero(CharacterObject template, Settlement bornSettlement = null, Clan faction = null, Clan supporterOfClan = null, int age = -1)
	{
		Hero hero = CreateNewHero(template, age);
		CultureObject culture = template.Culture;
		if (culture == null && bornSettlement != null)
		{
			culture = bornSettlement.Culture;
		}
		else if (culture == null && faction != null)
		{
			culture = faction.Culture;
		}
		if (faction != null)
		{
			hero.Clan = faction;
		}
		if (supporterOfClan != null)
		{
			hero.SupporterOf = supporterOfClan;
		}
		if (bornSettlement != null)
		{
			hero.BornSettlement = bornSettlement;
		}
		hero.CharacterObject.Culture = culture;
		NameGenerator.Current.GenerateHeroNameAndHeroFullName(hero, out var firstName, out var fullName, useDeterministicValues: false);
		hero.SetName(fullName, firstName);
		ModifyAppearanceByTraits(hero);
		CampaignEventDispatcher.Instance.OnHeroCreated(hero);
		return hero;
	}

	public static Hero CreateRelativeNotableHero(Hero relative)
	{
		Hero hero = CreateHeroAtOccupation(relative.CharacterObject.Occupation, relative.HomeSettlement);
		hero.Culture = relative.Culture;
		BodyProperties bodyPropertiesMin = relative.CharacterObject.GetBodyPropertiesMin();
		BodyProperties bodyPropertiesMin2 = hero.CharacterObject.GetBodyPropertiesMin();
		int defaultFaceSeed = relative.CharacterObject.GetDefaultFaceSeed(1);
		string hairTags = relative.HairTags;
		string tattooTags = relative.TattooTags;
		hero.StaticBodyProperties = BodyProperties.GetRandomBodyProperties(hero.CharacterObject.Race, hero.IsFemale, bodyPropertiesMin, bodyPropertiesMin2, 1, defaultFaceSeed, hairTags, relative.BeardTags, tattooTags).StaticProperties;
		return hero;
	}

	public static bool CreateBasicHero(CharacterObject character, out Hero hero, string stringId = "")
	{
		if (string.IsNullOrEmpty(stringId))
		{
			hero = CreateNewHero(character);
			CampaignEventDispatcher.Instance.OnHeroCreated(hero);
			return true;
		}
		hero = Campaign.Current.CampaignObjectManager.Find<Hero>(stringId);
		if (hero != null)
		{
			return false;
		}
		hero = Hero.CreateHero(stringId);
		hero.SetCharacterObject(character);
		hero.HeroDeveloper.InitializeHeroDeveloper(isByNaturalGrowth: false, hero.Template);
		hero.StaticBodyProperties = character.GetBodyPropertiesMin().StaticProperties;
		hero.Weight = 0f;
		hero.Build = 0f;
		character.HeroObject = hero;
		hero.PreferredUpgradeFormation = GetRandomPreferredUpgradeFormation();
		CampaignEventDispatcher.Instance.OnHeroCreated(hero);
		return true;
	}

	private static void ModifyAppearanceByTraits(Hero hero)
	{
		int num = MBRandom.RandomInt(0, 100);
		int num2 = MBRandom.RandomInt(0, 100);
		if (hero.Age >= 40f)
		{
			num -= 30;
			num2 += 30;
		}
		int hair = -1;
		int beard = -1;
		int tattoo = -1;
		bool flag = hero.HairTags.IsEmpty() && hero.BeardTags.IsEmpty();
		if (hero.GetTraitLevel(DefaultTraits.RomanHair) > 0 && !hero.IsFemale && flag)
		{
			hair = ((num >= 0) ? ((num < 20) ? 13 : ((num >= 70) ? 6 : 8)) : 0);
			beard = ((num2 >= 60) ? ((num2 >= 110) ? 14 : 13) : 0);
		}
		else if (hero.GetTraitLevel(DefaultTraits.CelticHair) > 0 && !hero.IsFemale && flag)
		{
			hair = ((num >= 0) ? ((num < 20) ? 13 : ((num < 40) ? 6 : ((num < 60) ? 14 : ((num >= 85) ? 7 : 2)))) : 0);
			beard = ((num2 < 40) ? 1 : ((num2 < 60) ? 2 : ((num2 >= 110) ? 5 : 3)));
		}
		else if (hero.GetTraitLevel(DefaultTraits.ArabianHair) > 0 && !hero.IsFemale && flag)
		{
			hair = ((num >= 0) ? ((num < 20) ? 13 : ((num < 40) ? 6 : ((num < 60) ? 2 : ((num >= 85) ? 7 : 11)))) : 0);
			beard = ((num2 >= 40) ? ((num2 < 50) ? 6 : ((num2 < 60) ? 12 : ((num2 < 70) ? 8 : ((num2 < 80) ? 15 : ((num2 >= 100) ? 17 : 9))))) : 0);
		}
		else if (hero.GetTraitLevel(DefaultTraits.RusHair) > 0 && !hero.IsFemale && flag)
		{
			hair = ((num >= 0) ? ((num < 40) ? 6 : ((num < 60) ? 12 : ((num >= 85) ? 2 : 11))) : 0);
			beard = ((num2 >= 30) ? ((num2 < 60) ? 13 : ((num2 < 70) ? 17 : ((num2 >= 90) ? 19 : 18))) : 0);
		}
		hero.ModifyHair(hair, beard, tattoo);
	}

	private static void AddRandomVarianceToTraits(Hero hero)
	{
		foreach (TraitObject item in TraitObject.All)
		{
			if (item != DefaultTraits.Honor && item != DefaultTraits.Mercy && item != DefaultTraits.Generosity && item != DefaultTraits.Valor && item != DefaultTraits.Calculating)
			{
				continue;
			}
			int num = hero.CharacterObject.GetTraitLevel(item);
			float num2 = MBRandom.RandomFloat;
			if (hero.IsPreacher && item == DefaultTraits.Generosity)
			{
				num2 = 0.5f;
			}
			if (hero.IsMerchant && item == DefaultTraits.Calculating)
			{
				num2 = 0.5f;
			}
			if ((double)num2 < 0.25)
			{
				num--;
				if (num < -1)
				{
					num = -1;
				}
			}
			if ((double)num2 > 0.75)
			{
				num++;
				if (num > 1)
				{
					num = 1;
				}
			}
			if (hero.IsGangLeader && (item == DefaultTraits.Mercy || item == DefaultTraits.Honor) && num > 0)
			{
				num = 0;
			}
			num = MBMath.ClampInt(num, item.MinValue, item.MaxValue);
			hero.SetTraitLevel(item, num);
		}
	}

	public static Hero DeliverOffSpring(Hero mother, Hero father, bool isOffspringFemale)
	{
		Debug.SilentAssert(mother.CharacterObject.Race == father.CharacterObject.Race, "", getDump: false, "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\HeroCreator.cs", "DeliverOffSpring", 484);
		Hero hero = CreateNewHero(isOffspringFemale ? mother.CharacterObject : father.CharacterObject, 0);
		hero.ClearTraits();
		float randomFloat = MBRandom.RandomFloat;
		int num = 0;
		num = ((!((double)randomFloat < 0.1)) ? (((double)randomFloat < 0.5) ? 1 : ((!((double)randomFloat < 0.9)) ? 3 : 2)) : 0);
		List<TraitObject> list = DefaultTraits.Personality.ToList();
		list.Shuffle();
		for (int i = 0; i < Math.Min(list.Count, num); i++)
		{
			int value = (((double)MBRandom.RandomFloat < 0.5) ? MBRandom.RandomInt(list[i].MinValue, 0) : MBRandom.RandomInt(1, list[i].MaxValue + 1));
			hero.SetTraitLevel(list[i], value);
		}
		foreach (TraitObject item in TraitObject.All.Except(DefaultTraits.Personality))
		{
			hero.SetTraitLevel(item, ((double)MBRandom.RandomFloat < 0.5) ? mother.GetTraitLevel(item) : father.GetTraitLevel(item));
		}
		hero.SetNewOccupation(isOffspringFemale ? mother.Occupation : father.Occupation);
		int becomeChildAge = Campaign.Current.Models.AgeModel.BecomeChildAge;
		hero.CharacterObject.IsFemale = isOffspringFemale;
		hero.Mother = mother;
		hero.Father = father;
		MBEquipmentRoster randomElementInefficiently = Campaign.Current.Models.EquipmentSelectionModel.GetEquipmentRostersForDeliveredOffspring(hero).GetRandomElementInefficiently();
		if (randomElementInefficiently != null)
		{
			Equipment randomElementInefficiently2 = randomElementInefficiently.GetCivilianEquipments().GetRandomElementInefficiently();
			EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, randomElementInefficiently2);
			Equipment equipment = new Equipment(isCivilian: false);
			equipment.FillFrom(randomElementInefficiently2, useSourceEquipmentType: false);
			EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment);
		}
		else
		{
			Debug.FailedAssert("Equipment template not found", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\HeroCreator.cs", "DeliverOffSpring", 549);
		}
		NameGenerator.Current.GenerateHeroNameAndHeroFullName(hero, out var firstName, out var fullName, useDeterministicValues: false);
		hero.SetName(fullName, firstName);
		hero.HeroDeveloper.InitializeHeroDeveloper(isByNaturalGrowth: true);
		BodyProperties bodyProperties = mother.BodyProperties;
		BodyProperties bodyProperties2 = father.BodyProperties;
		int seed = MBRandom.RandomInt();
		string hairTags = (isOffspringFemale ? mother.HairTags : father.HairTags);
		string tattooTags = (isOffspringFemale ? mother.TattooTags : father.TattooTags);
		hero.StaticBodyProperties = BodyProperties.GetRandomBodyProperties(mother.CharacterObject.Race, isOffspringFemale, bodyProperties, bodyProperties2, 1, seed, hairTags, father.BeardTags, tattooTags).StaticProperties;
		hero.BornSettlement = DecideBornSettlement(hero);
		if (mother == Hero.MainHero || father == Hero.MainHero)
		{
			hero.Clan = Clan.PlayerClan;
			hero.Culture = Hero.MainHero.Culture;
		}
		else
		{
			hero.Clan = father.Clan;
			hero.Culture = (((double)MBRandom.RandomFloat < 0.5) ? father.Culture : mother.Culture);
		}
		CampaignEventDispatcher.Instance.OnHeroCreated(hero, isBornNaturally: true);
		int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
		if (hero.Age > (float)becomeChildAge || (hero.Age == (float)becomeChildAge && hero.BirthDay.GetDayOfYear < CampaignTime.Now.GetDayOfYear))
		{
			CampaignEventDispatcher.Instance.OnHeroGrowsOutOfInfancy(hero);
		}
		if (hero.Age > (float)heroComesOfAge || (hero.Age == (float)heroComesOfAge && hero.BirthDay.GetDayOfYear < CampaignTime.Now.GetDayOfYear))
		{
			CampaignEventDispatcher.Instance.OnHeroComesOfAge(hero);
		}
		return hero;
	}

	private static Settlement DecideBornSettlement(Hero child)
	{
		Settlement settlement;
		if (child.Mother.CurrentSettlement != null && (child.Mother.CurrentSettlement.IsTown || child.Mother.CurrentSettlement.IsVillage))
		{
			settlement = child.Mother.CurrentSettlement;
		}
		else if (child.Mother.PartyBelongedTo != null || child.Mother.PartyBelongedToAsPrisoner != null)
		{
			IMapPoint toMapPoint;
			if (child.Mother.PartyBelongedToAsPrisoner != null)
			{
				IMapPoint mapPoint;
				if (!child.Mother.PartyBelongedToAsPrisoner.IsMobile)
				{
					IMapPoint settlement2 = child.Mother.PartyBelongedToAsPrisoner.Settlement;
					mapPoint = settlement2;
				}
				else
				{
					IMapPoint settlement2 = child.Mother.PartyBelongedToAsPrisoner.MobileParty;
					mapPoint = settlement2;
				}
				toMapPoint = mapPoint;
			}
			else
			{
				toMapPoint = child.Mother.PartyBelongedTo;
			}
			settlement = SettlementHelper.FindNearestTown(null, toMapPoint);
		}
		else
		{
			settlement = child.Mother.HomeSettlement;
		}
		if (settlement == null)
		{
			settlement = ((child.Mother.Clan.Settlements.Count > 0) ? child.Mother.Clan.Settlements.GetRandomElement() : Town.AllTowns.GetRandomElement().Settlement);
		}
		return settlement;
	}

	private static FormationClass GetRandomPreferredUpgradeFormation()
	{
		int num = MBRandom.RandomInt(10);
		if (num < 4)
		{
			return (FormationClass)num;
		}
		return FormationClass.NumberOfAllFormations;
	}
}
