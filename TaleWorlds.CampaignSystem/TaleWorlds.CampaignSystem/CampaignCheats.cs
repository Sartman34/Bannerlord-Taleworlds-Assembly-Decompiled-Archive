using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.CampaignSystem;

public static class CampaignCheats
{
	public const string CampaignNotStarted = "Campaign was not started.";

	public const string CheatmodeDisabled = "Cheat mode is disabled!";

	public const string AchievementsAreDisabled = "Achievements are disabled!";

	public const string Help = "help";

	public const string EnterNumber = "Please enter a number";

	public const string EnterPositiveNumber = "Please enter a positive number";

	public const string SettlementNotFound = "Settlement is not found";

	public const string HeroNotFound = "Hero is not found";

	public const string KingdomNotFound = "Kingdom is not found";

	public const string VillageNotFound = "Village is not found";

	public const string FactionNotFound = "Faction is not found";

	public const string PartyNotFound = "Party is not found";

	public const string OK = "Success";

	public const string CheatNameSeparator = "|";

	public const string AiDisabledHelper = "Party AI can be enabled again by using \"campaign.control_party_ai_by_cheats\"";

	public static string ErrorType = "";

	public const int MaxSkillValue = 300;

	private static bool _mainPartyIsAttackable = true;

	public static bool MainPartyIsAttackable => _mainPartyIsAttackable;

	public static bool CheckCheatUsage(ref string ErrorType)
	{
		if (Campaign.Current == null)
		{
			ErrorType = "Campaign was not started.";
			return false;
		}
		if (!Game.Current.CheatMode)
		{
			ErrorType = "Cheat mode is disabled!";
			return false;
		}
		ErrorType = "";
		return true;
	}

	public static bool CheckParameters(List<string> strings, int ParameterCount)
	{
		if (strings.Count == 0)
		{
			return ParameterCount == 0;
		}
		return strings.Count == ParameterCount;
	}

	public static bool CheckHelp(List<string> strings)
	{
		if (strings.Count == 0)
		{
			return false;
		}
		return strings[0].ToLower() == "help";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_hero_crafting_stamina", "campaign")]
	public static string SetCraftingStamina(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_hero_crafting_stamina [HeroName] | [Stamina]\".";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		ICraftingCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();
		if (campaignBehavior == null)
		{
			return "Can not found ICrafting Campaign Behavior!\n" + text;
		}
		int result = 0;
		if (!int.TryParse(separatedNames[1], out result) || result < 0 || result > 100)
		{
			return "Please enter a valid number between 0-100 number is: " + result + "\n" + text;
		}
		Hero hero = GetHero(separatedNames[0]);
		if (hero != null)
		{
			int value = (int)((float)(campaignBehavior.GetMaxHeroCraftingStamina(hero) * result) / 100f);
			campaignBehavior.SetHeroCraftingStamina(hero, value);
			return "Success";
		}
		return "Hero is not found\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_hero_culture", "campaign")]
	public static string SetHeroCulture(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_hero_culture [HeroName] | [CultureName]\".";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		string objectName = separatedNames[1].ToLower().Replace(" ", "");
		CultureObject @object = Campaign.Current.ObjectManager.GetObject<CultureObject>(objectName);
		if (@object == null)
		{
			return "Culture couldn't be found!\n" + text;
		}
		Hero hero = GetHero(separatedNames[0]);
		if (hero != null)
		{
			if (hero.Culture == @object)
			{
				return $"Hero culture is already {@object.Name}";
			}
			hero.Culture = @object;
			return "Success";
		}
		return "Hero is not found\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_clan_culture", "campaign")]
	public static string SetClanCulture(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_clan_culture [ClanName] | [CultureName]\".";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> parameters = GetSeparatedNames(strings, "|");
		if (parameters.Count != 2)
		{
			return text;
		}
		string objectName = parameters[1].ToLower().Replace(" ", "");
		CultureObject @object = Campaign.Current.ObjectManager.GetObject<CultureObject>(objectName);
		if (@object == null)
		{
			return "Culture couldn't be found!\n" + text;
		}
		Clan clan = Campaign.Current.Clans.FirstOrDefault((Clan x) => string.Equals(x.Name.ToString().Replace(" ", ""), parameters[0].Replace(" ", ""), StringComparison.OrdinalIgnoreCase) && !x.IsEliminated);
		if (clan != null)
		{
			if (clan.Culture == @object)
			{
				return $"Clan culture is already {@object.Name}";
			}
			clan.Culture = @object;
			return "Success";
		}
		return "Clan couldn't be found\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("make_hero_wounded", "campaign")]
	public static string MakeHeroWounded(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.make_hero_wounded [HeroName]\".";
		if (!CheckParameters(strings, 0) && !CheckHelp(strings))
		{
			Hero hero = GetHero(ConcatenateString(strings));
			if (hero != null)
			{
				hero.MakeWounded();
				return "Success";
			}
			return "Hero is not found\n" + text;
		}
		return text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("reset_player_skills_level_and_perks", "campaign")]
	public static string ResetPlayerSkillsLevelAndPerk(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckParameters(strings, 0) && !CheckHelp(strings))
		{
			Hero.MainHero.CharacterObject.Level = 0;
			Hero.MainHero.HeroDeveloper.ClearHero();
			return "Success";
		}
		return "Format is \"campaign.reset_player_skills_level_and_perks\".";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_skills_of_hero", "campaign")]
	public static string SetSkillsOfGivenHero(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_skills_of_hero [HeroName] | [Level]\".";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		int result = -1;
		Hero hero = null;
		if (!int.TryParse(separatedNames[1], out result))
		{
			return "Level must be a number\n" + text;
		}
		hero = GetHero(separatedNames[0]);
		if (hero != null)
		{
			if (result > 0 && result <= 300)
			{
				hero.CharacterObject.Level = 0;
				hero.HeroDeveloper.ClearHero();
				int num = TaleWorlds.Library.MathF.Min(result / 25 + 1, 10);
				int maxFocusPerSkill = Campaign.Current.Models.CharacterDevelopmentModel.MaxFocusPerSkill;
				foreach (SkillObject item in Skills.All)
				{
					if (hero.HeroDeveloper.GetFocus(item) + num > maxFocusPerSkill)
					{
						num = maxFocusPerSkill;
					}
					hero.HeroDeveloper.AddFocus(item, num, checkUnspentFocusPoints: false);
					hero.HeroDeveloper.SetInitialSkillLevel(item, result);
				}
				hero.HeroDeveloper.UnspentFocusPoints = 0;
				return $"{hero.Name}'s skills are set to level {result}.";
			}
			return $"Level must be between 0 - {300}.";
		}
		return "Hero is not found\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_settlements_visible", "campaign")]
	public static string SetAllSettlementsVisible(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckParameters(strings, 1) && !CheckHelp(strings) && int.TryParse(strings[0], out var result) && (result == 2 || result == 1 || result == 0))
		{
			foreach (Settlement item in Settlement.All)
			{
				bool isInspected = (item.IsVisible = result != 0 && (!item.IsHideout || result == 1));
				item.IsInspected = isInspected;
			}
			return "Success";
		}
		return "Format is \"campaign.set_settlements_visible [2(no hideouts)/1(all)/0(none)]\".";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_skill_main_hero", "campaign")]
	public static string SetSkillMainHero(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_skill_main_hero [SkillName] | [LevelValue]\".";
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2 || CheckHelp(strings))
		{
			return text;
		}
		foreach (SkillObject item in Skills.All)
		{
			if (string.Equals(item.Name.ToString().Replace(" ", ""), separatedNames[0].Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
			{
				int result = 1;
				if (!int.TryParse(separatedNames[1], out result))
				{
					return "Please enter a number\n" + text;
				}
				if (result <= 0 || result > 300)
				{
					return $"Level must be between 0 - {300}.";
				}
				Hero.MainHero.HeroDeveloper.SetInitialSkillLevel(item, result);
				Hero.MainHero.HeroDeveloper.InitializeSkillXp(item);
				return "Success";
			}
		}
		return "Skill is not found.\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_all_skills_main_hero", "campaign")]
	public static string SetAllSkillsMainHero(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		int result = 1;
		string text = "Format is \"campaign.set_all_skills_main_hero [LevelValue]\".";
		if (strings.IsEmpty() || !int.TryParse(strings[0], out result))
		{
			return "Please enter a number\n" + text;
		}
		if (result <= 0 || result > 300)
		{
			return $"Level must be between 0 - {300}.";
		}
		if (CheckParameters(strings, 1) && !CheckHelp(strings))
		{
			foreach (SkillObject item in Skills.All)
			{
				Hero.MainHero.HeroDeveloper.SetInitialSkillLevel(item, result);
				Hero.MainHero.HeroDeveloper.InitializeSkillXp(item);
			}
			return "Success";
		}
		return text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_skill_of_all_companions", "campaign")]
	public static string SetSkillCompanion(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_skill_of_all_companions [SkillName] | [LevelValue]\".";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		foreach (SkillObject item in Skills.All)
		{
			if (!string.Equals(item.Name.ToString().Replace(" ", ""), separatedNames[0].Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			int result = 1;
			if (!int.TryParse(separatedNames[1], out result))
			{
				return "Please enter a number\n" + text;
			}
			if (result <= 0 || result > 300)
			{
				return $"Level must be between 0 - {300}.";
			}
			foreach (Hero companion in Clan.PlayerClan.Companions)
			{
				companion.HeroDeveloper.SetInitialSkillLevel(item, result);
				companion.HeroDeveloper.InitializeSkillXp(item);
			}
			return "Success";
		}
		return "Skill is not found.\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_all_companion_skills", "campaign")]
	public static string SetAllSkillsOfAllCompanions(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_all_companion_skills [LevelValue]\".";
		if (!CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		foreach (SkillObject item in Skills.All)
		{
			int result = 1;
			if (strings.Count == 0 || !int.TryParse(strings[0], out result))
			{
				return "Please enter a number\n" + text;
			}
			if (result <= 0 || result > 300)
			{
				return $"Level must be between 0 - {300}.";
			}
			foreach (Hero companion in Clan.PlayerClan.Companions)
			{
				companion.HeroDeveloper.SetInitialSkillLevel(item, result);
				companion.HeroDeveloper.InitializeSkillXp(item);
			}
		}
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_all_heroes_skills", "campaign")]
	public static string SetAllHeroSkills(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_all_heroes_skills [LevelValue]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return text;
		}
		if (strings.Count == 0 || !int.TryParse(strings[0], out var result))
		{
			return "Please enter a positive number\n" + text;
		}
		foreach (Hero item in Hero.AllAliveHeroes.Where((Hero x) => x.IsActive && x.PartyBelongedTo != null).ToList())
		{
			foreach (SkillObject item2 in Skills.All)
			{
				if (result <= 0 || result > 300)
				{
					return $"Level must be between 0 - {300}.";
				}
				item.HeroDeveloper.SetInitialSkillLevel(item2, result);
				item.HeroDeveloper.InitializeSkillXp(item2);
			}
		}
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_loyalty_of_settlement", "campaign")]
	public static string SetLoyaltyOfSettlement(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_loyalty_of_settlement [SettlementName] | [loyalty]\".";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		int result = 0;
		if (!int.TryParse(separatedNames[1], out result))
		{
			return "Please enter a positive number\n" + text;
		}
		if (result > 100 || result < 0)
		{
			return "Loyalty has to be in the range of 0 to 100";
		}
		string text2 = separatedNames[0];
		Settlement settlement = GetSettlement(text2);
		if (settlement == null)
		{
			return "Settlement is not found: " + text2 + "\n" + text;
		}
		if (settlement.IsVillage)
		{
			return "Settlement must be castle or town";
		}
		settlement.Town.Loyalty = result;
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_main_party_position", "campaign")]
	public static string PrintMainPartyPosition(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return "Format is \"campaign.print_main_party_position\".";
		}
		return MobileParty.MainParty.Position2D.x + " " + MobileParty.MainParty.Position2D.y;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("start_world_war", "campaign")]
	public static string StartWorldWar(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return "Format is \"campaign.start_world_war\".";
		}
		foreach (Kingdom item in Kingdom.All)
		{
			foreach (Kingdom item2 in Kingdom.All)
			{
				if (item != item2 && !FactionManager.IsAtWarAgainstFaction(item, item2))
				{
					DeclareWarAction.ApplyByDefault(item, item2);
				}
			}
		}
		return "All kingdoms are at war with each other!";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("start_world_peace", "campaign")]
	public static string StartWorldPeace(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return "Format is \"campaign.start_world_peace\".";
		}
		foreach (Kingdom item in Kingdom.All)
		{
			foreach (Kingdom item2 in Kingdom.All)
			{
				if (item != item2 && FactionManager.IsAtWarAgainstFaction(item, item2))
				{
					MakePeaceAction.Apply(item, item2);
				}
			}
		}
		return "All kingdoms are at peace with each other!";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_modified_item", "campaign")]
	public static string AddModifiedItem(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_modified_item [ItemName] | [ModifierName]\".";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		string itemObjectName = separatedNames[0];
		string itemModifierName = separatedNames[1];
		ItemObject itemObject = GetItemObject(itemObjectName);
		if (itemObject != null)
		{
			ItemModifier itemModifier = GetItemModifier(itemModifierName);
			if (itemModifier != null)
			{
				EquipmentElement rosterElement = new EquipmentElement(itemObject, itemModifier);
				MobileParty.MainParty.ItemRoster.AddToCounts(rosterElement, 5);
				return "Success";
			}
			return "Cant find the modifier.\n" + text;
		}
		return "Cant find the item.\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_player_name", "campaign")]
	public static string SetPlayerName(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return "Format is \"campaign.set_player_name [HeroName]\".";
		}
		string value = ConcatenateString(strings);
		TextObject textObject = GameTexts.FindText("str_generic_character_firstname");
		textObject.SetTextVariable("CHARACTER_FIRSTNAME", new TextObject(value));
		TextObject textObject2 = GameTexts.FindText("str_generic_character_name");
		textObject2.SetTextVariable("CHARACTER_NAME", new TextObject(value));
		Hero.MainHero.SetName(textObject2, textObject);
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_crafting_materials", "campaign")]
	public static string AddCraftingMaterials(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return "Format is \"campaign.add_crafting_materials\".";
		}
		for (int i = 0; i < 9; i++)
		{
			PartyBase.MainParty.ItemRoster.AddToCounts(Campaign.Current.Models.SmithingModel.GetCraftingMaterialItem((CraftingMaterials)i), 100);
		}
		return "100 pieces for each crafting material is added to the player inventory.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_hero_relation", "campaign")]
	public static string AddHeroRelation(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_hero_relation [HeroName]/All | [Value] \".\n";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		if (int.TryParse(separatedNames[1], out var result))
		{
			string text2 = separatedNames[0];
			Hero hero = GetHero(text2);
			if (hero == Hero.MainHero)
			{
				return "Can not add relation with yourself.";
			}
			if (hero != null)
			{
				ChangeRelationAction.ApplyPlayerRelation(hero, result);
				return "Success";
			}
			if (string.Equals(text2, "all", StringComparison.OrdinalIgnoreCase))
			{
				foreach (Hero allAliveHero in Hero.AllAliveHeroes)
				{
					if (!allAliveHero.IsHumanPlayerCharacter)
					{
						ChangeRelationAction.ApplyPlayerRelation(allAliveHero, result);
					}
				}
				return "Success";
			}
			return "Hero is not found\n" + text;
		}
		return "Please enter a number\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("heal_main_party", "campaign")]
	public static string HealMainParty(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return "Format is \"campaign.heal_main_party\".";
		}
		if (MobileParty.MainParty.MapEvent == null)
		{
			for (int i = 0; i < PartyBase.MainParty.MemberRoster.Count; i++)
			{
				TroopRosterElement elementCopyAtIndex = PartyBase.MainParty.MemberRoster.GetElementCopyAtIndex(i);
				if (elementCopyAtIndex.Character.IsHero)
				{
					elementCopyAtIndex.Character.HeroObject.Heal(elementCopyAtIndex.Character.HeroObject.MaxHitPoints);
				}
				else
				{
					MobileParty.MainParty.Party.AddToMemberRosterElementAtIndex(i, 0, -PartyBase.MainParty.MemberRoster.GetElementWoundedNumber(i));
				}
			}
			return "Success";
		}
		return "Main party shouldn't be in a map event.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("declare_war", "campaign")]
	public static string DeclareWar(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is campaign.declare_war [Faction1] | [Faction2]\".";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		string text2 = separatedNames[0].ToLower().Replace(" ", "");
		string text3 = separatedNames[1].ToLower().Replace(" ", "");
		IFaction faction = null;
		IFaction faction2 = null;
		foreach (IFaction faction3 in Campaign.Current.Factions)
		{
			string text4 = faction3.Name.ToString().ToLower().Replace(" ", "");
			if (text4 == text2)
			{
				faction = faction3;
			}
			else if (text4 == text3)
			{
				faction2 = faction3;
			}
		}
		if (faction == null)
		{
			return "Faction is not found: " + text2 + "\n" + text;
		}
		if (faction2 == null)
		{
			return "Faction is not found: " + text3 + "\n" + text;
		}
		if (faction == faction2 || faction.MapFaction == faction2.MapFaction)
		{
			return "Can't declare between same factions";
		}
		if (!faction.IsMapFaction)
		{
			return string.Concat(faction.Name, " is bound to a kingdom.");
		}
		if (!faction2.IsMapFaction)
		{
			return string.Concat(faction.Name, " is bound to a kingdom.");
		}
		DeclareWarAction.ApplyByDefault(faction, faction2);
		return string.Concat("War declared between ", faction.Name, " and ", faction2.Name);
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("declare_peace", "campaign")]
	public static string DeclarePeace(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "campaign.declare_peace [Faction1] | [Faction2]";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		string text2 = separatedNames[0].ToLower().Replace(" ", "");
		string text3 = separatedNames[1].ToLower().Replace(" ", "");
		IFaction faction = null;
		IFaction faction2 = null;
		foreach (IFaction faction3 in Campaign.Current.Factions)
		{
			string text4 = faction3.Name.ToString().ToLower().Replace(" ", "");
			if (text4 == text2)
			{
				faction = faction3;
			}
			else if (text4 == text3)
			{
				faction2 = faction3;
			}
		}
		if (faction == null)
		{
			return "Faction is not found: " + text2 + "\n" + text;
		}
		if (faction2 == null)
		{
			return "Faction is not found: " + text3 + "\n" + text;
		}
		if (faction == faction2 || faction.MapFaction == faction2.MapFaction)
		{
			return "Can't declare between same factions";
		}
		if (!faction.IsMapFaction)
		{
			return string.Concat(faction.Name, " is bound to a kingdom.");
		}
		if (!faction2.IsMapFaction)
		{
			return string.Concat(faction.Name, " is bound to a kingdom.");
		}
		if (faction.GetStanceWith(faction2).IsAtConstantWar)
		{
			return "There is constant war between factions, peace can't be declared";
		}
		MakePeaceAction.Apply(faction, faction2);
		return string.Concat("Peace declared between ", faction.Name, " and ", faction2.Name);
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_item_to_main_party", "campaign")]
	public static string AddItemToMainParty(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_item_to_main_party [ItemObject] | [Amount]\"\n If amount is not entered only 1 item will be given.";
		if (CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		ItemObject itemObject = GetItemObject(separatedNames[0]);
		if (itemObject != null)
		{
			int result = 1;
			if (strings.Count == 1)
			{
				PartyBase.MainParty.ItemRoster.AddToCounts(itemObject, result);
				return string.Concat(itemObject.Name, " has been given to the main party.");
			}
			if (separatedNames.Count == 2 && (!int.TryParse(separatedNames[1], out result) || result < 1))
			{
				return "Please enter a positive number\n" + text;
			}
			PartyBase.MainParty.ItemRoster.AddToCounts(itemObject, result);
			return string.Concat(itemObject.Name, " has been given to the main party.");
		}
		return "Item is not found\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_all_crafting_materials_to_main_party", "campaign")]
	public static string AddCraftingMaterialItemsToMainParty(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_all_crafting_materials_to_main_party [Amount]\n If amount is not entered only 1 item per material will be given.\".";
		if (strings.Count > 1 || CheckHelp(strings))
		{
			return text;
		}
		int result = 1;
		if (strings.Count == 1 && (!int.TryParse(strings[0], out result) || result < 1))
		{
			return "Please enter a positive number\n" + text;
		}
		for (CraftingMaterials craftingMaterials = CraftingMaterials.IronOre; craftingMaterials < CraftingMaterials.NumCraftingMats; craftingMaterials++)
		{
			ItemObject craftingMaterialItem = Campaign.Current.Models.SmithingModel.GetCraftingMaterialItem(craftingMaterials);
			PartyBase.MainParty.ItemRoster.AddToCounts(craftingMaterialItem, result);
		}
		return "Crafting materials have been given to the main party.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("kill_capturer_party", "campaign")]
	public static string KillCapturerParty(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return "Format is \"campaign.kill_capturer_party\".";
		}
		if (PlayerCaptivity.IsCaptive)
		{
			if (PlayerCaptivity.CaptorParty.IsSettlement)
			{
				return "Can't destroy settlement";
			}
			GameMenu.SwitchToMenu("menu_captivity_end_by_party_removed");
			DestroyPartyAction.Apply(null, PlayerCaptivity.CaptorParty.MobileParty);
			return "Success";
		}
		return "Player is not captive.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_influence", "campaign")]
	public static string AddInfluence(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckHelp(strings))
		{
			return "Format is \"campaign.add_influence [Number]\". If Number is not entered, 100 influence will be added.";
		}
		int result = 100;
		bool flag = false;
		if (!CheckParameters(strings, 0))
		{
			flag = int.TryParse(strings[0], out result);
		}
		if (flag || CheckParameters(strings, 0))
		{
			float num = MBMath.ClampFloat(Hero.MainHero.Clan.Influence + (float)result, 0f, float.MaxValue);
			float num2 = num - Hero.MainHero.Clan.Influence;
			ChangeClanInfluenceAction.Apply(Clan.PlayerClan, num);
			return $"The influence of player is changed by {num2}.";
		}
		return "Please enter a positive number\nFormat is \"campaign.add_influence [Number]\".";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_renown_to_clan", "campaign")]
	public static string AddRenown(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_renown [ClanName] | [PositiveNumber]\". \n If number is not specified, 100 will be added. \n If clan name is not specified, player clan will get the renown.";
		if (CheckHelp(strings))
		{
			return text;
		}
		int result = 100;
		string text2 = "";
		Hero hero = Hero.MainHero;
		bool flag = false;
		if (CheckParameters(strings, 1))
		{
			if (!int.TryParse(strings[0], out result))
			{
				result = 100;
				text2 = ConcatenateString(strings);
				hero = GetClanLeader(text2);
				flag = true;
			}
		}
		else if (!CheckParameters(strings, 0))
		{
			List<string> separatedNames = GetSeparatedNames(strings, "|");
			if (separatedNames.Count == 2 && !int.TryParse(separatedNames[1], out result))
			{
				return "Please enter a positive number\n" + text;
			}
			text2 = separatedNames[0];
			hero = GetClanLeader(text2);
			flag = true;
		}
		if (hero != null)
		{
			if (result > 0)
			{
				GainRenownAction.Apply(hero, result);
				return $"Added {result} renown to " + hero.Clan.Name;
			}
			return "Please enter a positive number\n" + text;
		}
		if (flag)
		{
			return "Clan: " + text2 + " not found.\n" + text;
		}
		return "Wrong Input.\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_gold_to_hero", "campaign")]
	public static string AddGoldToHero(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_gold_to_hero [HeroName] | [PositiveNumber]\".\n If number is not specified, 1000 will be added. \n If hero name is not specified, player's gold will change.";
		if (CheckHelp(strings))
		{
			return text;
		}
		int result = 1000;
		Hero hero = Hero.MainHero;
		if (CheckParameters(strings, 0))
		{
			GiveGoldAction.ApplyBetweenCharacters(null, hero, result, disableNotification: true);
			return "Success";
		}
		if (CheckParameters(strings, 1) && !int.TryParse(strings[0], out result))
		{
			result = 1000;
			hero = GetHero(ConcatenateString(strings));
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count == 2)
		{
			if (!int.TryParse(separatedNames[1], out result))
			{
				return "Please enter a number\n" + text;
			}
			hero = GetHero(separatedNames[0]);
		}
		if (separatedNames.Count == 1 && !int.TryParse(separatedNames[0], out result))
		{
			hero = GetHero(separatedNames[0]);
		}
		if (hero == null)
		{
			return "Hero is not found\n" + text;
		}
		if (hero.Gold + result < 0 || hero.Gold + result > 100000000)
		{
			return "Hero's gold must be between 0-100000000.";
		}
		GiveGoldAction.ApplyBetweenCharacters(null, hero, result, disableNotification: true);
		return $"{hero.Name}'s denars changed by {result}.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_gold_to_all_heroes", "campaign")]
	public static string AddGoldToAllHeroes(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_gold_to_all_heroes [PositiveNumber]\".\n If number is not specified, 100 will be added.";
		if (CheckHelp(strings))
		{
			return text;
		}
		int result = 1000;
		bool flag = false;
		if (!CheckParameters(strings, 0))
		{
			flag = int.TryParse(strings[0], out result);
		}
		if (flag || CheckParameters(strings, 0))
		{
			if (result < 1)
			{
				return "Please enter a positive number\n" + text;
			}
			foreach (Hero allAliveHero in Hero.AllAliveHeroes)
			{
				if (allAliveHero != null)
				{
					GiveGoldAction.ApplyBetweenCharacters(null, allAliveHero, result, disableNotification: true);
				}
			}
			return $"All party's denars changed by {result}.";
		}
		return "Wrong input.\nFormat is \"campaign.add_gold_to_all_heroes [Number]\".";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("activate_all_policies_for_player_kingdom", "campaign")]
	public static string ActivateAllPolicies(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return "Format is \"campaign.activate_all_policies_for_player_kingdom";
		}
		if (Clan.PlayerClan.Kingdom != null)
		{
			Kingdom kingdom = Clan.PlayerClan.Kingdom;
			foreach (PolicyObject item in PolicyObject.All)
			{
				if (!kingdom.ActivePolicies.Contains(item))
				{
					kingdom.AddPolicy(item);
				}
			}
			return "All policies are now active for player kingdom.";
		}
		return "Player is not in a kingdom.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_building_level", "campaign")]
	public static string AddDevelopment(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_building_level [SettlementName] | [Building]\".";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		Settlement settlement = GetSettlement(separatedNames[0]);
		if (settlement != null && settlement.IsFortification)
		{
			BuildingType buildingType = null;
			foreach (BuildingType item in BuildingType.All)
			{
				if (item.Name.ToString().ToLower().Replace(" ", "")
					.Equals(separatedNames[1].ToString().ToLower().Replace(" ", "")))
				{
					if (item.BuildingLocation == BuildingLocation.Castle && settlement.IsCastle)
					{
						buildingType = item;
						break;
					}
					if (settlement.IsTown && (item.BuildingLocation == BuildingLocation.Settlement || item.BuildingLocation == BuildingLocation.Daily))
					{
						buildingType = item;
						break;
					}
				}
			}
			if (buildingType == null)
			{
				return "Development could not be found.\n" + text;
			}
			foreach (Building building in settlement.Town.Buildings)
			{
				if (building.BuildingType == buildingType)
				{
					if (building.CurrentLevel < 3)
					{
						building.CurrentLevel++;
						return string.Concat(buildingType.Name, " level increased to ", building.CurrentLevel, " at ", settlement.Name);
					}
					return string.Concat(buildingType.Name, " is already at top level!");
				}
			}
		}
		return "Settlement is not found\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_progress_to_current_building", "campaign")]
	public static string AddDevelopmentProgress(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_progress_to_current_building [SettlementName] | [Progress (0-100)]\".";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		if (int.TryParse(separatedNames[1], out var result))
		{
			if (result > 100 || result < 0)
			{
				return "Progress must be between 0 and 100.";
			}
			Settlement settlement = GetSettlement(separatedNames[0]);
			if (settlement != null && settlement.IsFortification)
			{
				Building currentBuilding = settlement.Town.CurrentBuilding;
				if (currentBuilding != null)
				{
					if (!currentBuilding.BuildingType.IsDefaultProject)
					{
						settlement.Town.BuildingsInProgress.Peek().BuildingProgress += ((float)currentBuilding.GetConstructionCost() - currentBuilding.BuildingProgress) * (float)result / 100f;
						return "Development progress increased to " + (int)(settlement.Town.BuildingsInProgress.Peek().BuildingProgress * 100f) + " at " + settlement.Name;
					}
					return "Currently there are no buildings in queue.";
				}
			}
			return "Settlement is not found\n" + text;
		}
		return "Please enter a positive number\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_current_building", "campaign")]
	public static string SetCurrentDevelopment(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_current_building [SettlementName] | [BuildingTypeName]\".";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		Settlement settlement = GetSettlement(separatedNames[0]);
		if (settlement != null && settlement.IsFortification)
		{
			BuildingType buildingType = null;
			bool flag = true;
			foreach (BuildingType item in BuildingType.All)
			{
				if (separatedNames[1].Replace(" ", "").Equals(item.Name.ToString().Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
				{
					buildingType = item;
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				foreach (Building building in settlement.Town.Buildings)
				{
					if (building.BuildingType == buildingType && building.CurrentLevel < 3)
					{
						BuildingHelper.ChangeCurrentBuilding(buildingType, settlement.Town);
						return string.Concat("Current building changed to ", building.BuildingType.Name, " at ", settlement.Name);
					}
				}
			}
			return "Building type is not found.\n" + text;
		}
		return "Settlement is not found\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_skill_xp_to_hero", "campaign")]
	public static string AddSkillXpToHero(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		Hero mainHero = Hero.MainHero;
		int result = 100;
		string text = "Format is \"campaign.add_skill_xp_to_hero [HeroName] | [SkillName] | [PositiveNumber]\".";
		if (CheckHelp(strings))
		{
			return text;
		}
		if (CheckParameters(strings, 0))
		{
			if (mainHero != null)
			{
				string text2 = "";
				{
					foreach (SkillObject item in Skills.All)
					{
						mainHero.HeroDeveloper.AddSkillXp(item, result);
						int num = (int)(mainHero.HeroDeveloper.GetFocusFactor(item) * (float)result);
						text2 += $"{result} xp is modified to {num} xp due to focus point factor \nand added to the {mainHero.Name}'s {item.Name} skill.\n";
					}
					return text2;
				}
			}
			return "Wrong Input.\n" + text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count == 1)
		{
			string text3 = "";
			if (int.TryParse(separatedNames[0], out result))
			{
				if (result <= 0)
				{
					return "Please enter a positive number\n" + text;
				}
				{
					foreach (SkillObject item2 in Skills.All)
					{
						mainHero.HeroDeveloper.AddSkillXp(item2, result);
						int num2 = (int)(mainHero.HeroDeveloper.GetFocusFactor(item2) * (float)result);
						text3 += $"{result} xp is modified to {num2} xp due to focus point factor \nand added to the {mainHero.Name}'s {item2.Name} skill.\n";
					}
					return text3;
				}
			}
			mainHero = GetHero(separatedNames[0]);
			result = 100;
			if (mainHero == null)
			{
				mainHero = Hero.MainHero;
				string text4 = separatedNames[0];
				{
					foreach (SkillObject item3 in Skills.All)
					{
						if (item3.Name.ToString().Replace(" ", "").Equals(text4.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase) || item3.StringId == text4.Replace(" ", ""))
						{
							if (mainHero.GetSkillValue(item3) < 300)
							{
								mainHero.HeroDeveloper.AddSkillXp(item3, result);
								int num2 = (int)(mainHero.HeroDeveloper.GetFocusFactor(item3) * (float)result);
								return $"Input {result} xp is modified to {num2} xp due to focus point factor \nand added to the {mainHero.Name}'s {item3.Name} skill. ";
							}
							return $"{item3} value for {mainHero} is already at max.. ";
						}
					}
					return text;
				}
			}
			{
				foreach (SkillObject item4 in Skills.All)
				{
					mainHero.HeroDeveloper.AddSkillXp(item4, result);
					int num2 = (int)(mainHero.HeroDeveloper.GetFocusFactor(item4) * (float)result);
					text3 += $"{result} xp is modified to {num2} xp due to focus point factor \nand added to the {mainHero.Name}'s {item4.Name} skill.\n";
				}
				return text3;
			}
		}
		if (separatedNames.Count == 2)
		{
			mainHero = GetHero(separatedNames[0]);
			if (mainHero == null)
			{
				mainHero = Hero.MainHero;
				if (!int.TryParse(separatedNames[1], out result))
				{
					return text;
				}
				if (result <= 0)
				{
					return "Please enter a positive number\n" + text;
				}
				string text5 = separatedNames[0];
				foreach (SkillObject item5 in Skills.All)
				{
					if (item5.Name.ToString().Replace(" ", "").Equals(text5.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase) || item5.StringId == text5.Replace(" ", ""))
					{
						if (mainHero.GetSkillValue(item5) < 300)
						{
							mainHero.HeroDeveloper.AddSkillXp(item5, result);
							int num3 = (int)(mainHero.HeroDeveloper.GetFocusFactor(item5) * (float)result);
							return $"Input {result} xp is modified to {num3} xp due to focus point factor \nand added to the {mainHero.Name}'s {item5.Name} skill. ";
						}
						return $"{item5} value for {mainHero} is already at max.. ";
					}
				}
				return "Skill not found.\n" + text;
			}
			if (!int.TryParse(separatedNames[1], out result))
			{
				result = 100;
				string text6 = separatedNames[1];
				foreach (SkillObject item6 in Skills.All)
				{
					if (item6.Name.ToString().Replace(" ", "").Equals(text6.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase) || item6.StringId == text6.Replace(" ", ""))
					{
						if (mainHero.GetSkillValue(item6) < 300)
						{
							mainHero.HeroDeveloper.AddSkillXp(item6, result);
							int num4 = (int)(mainHero.HeroDeveloper.GetFocusFactor(item6) * (float)result);
							return $"Input {result} xp is modified to {num4} xp due to focus point factor \nand added to the {mainHero.Name}'s {item6.Name} skill. ";
						}
						return $"{item6} value for {mainHero} is already at max.. ";
					}
				}
				return "Skill not found.\n" + text;
			}
			if (result <= 0)
			{
				return "Please enter a positive number\n" + text;
			}
			using List<SkillObject>.Enumerator enumerator = Skills.All.GetEnumerator();
			if (enumerator.MoveNext())
			{
				SkillObject current7 = enumerator.Current;
				if (mainHero.GetSkillValue(current7) < 300)
				{
					mainHero.HeroDeveloper.AddSkillXp(current7, result);
					int num5 = (int)(mainHero.HeroDeveloper.GetFocusFactor(current7) * (float)result);
					return $"Input {result} xp is modified to {num5} xp due to focus point factor \nand added to the {mainHero.Name}'s {current7.Name} skill. ";
				}
				return $"{current7} value for {mainHero} is already at max.. ";
			}
		}
		if (separatedNames.Count == 3)
		{
			if (!int.TryParse(separatedNames[2], out result) || result < 0)
			{
				return "Please enter a positive number\n" + text;
			}
			mainHero = GetHero(separatedNames[0]);
			if (mainHero == null)
			{
				return "Hero is not found\n" + text;
			}
			string text7 = separatedNames[1];
			foreach (SkillObject item7 in Skills.All)
			{
				if (item7.Name.ToString().Replace(" ", "").Equals(text7.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase) || item7.StringId == text7.Replace(" ", ""))
				{
					if (mainHero.GetSkillValue(item7) < 300)
					{
						mainHero.HeroDeveloper.AddSkillXp(item7, result);
						int num6 = (int)(mainHero.HeroDeveloper.GetFocusFactor(item7) * (float)result);
						return $"Input {result} xp is modified to {num6} xp due to focus point factor \nand added to the {mainHero.Name}'s {item7.Name} skill. ";
					}
					return $"{item7} value for {mainHero} is already at max.. ";
				}
			}
			return "Skill not found.\n" + text;
		}
		return text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_prisoners", "campaign")]
	public static string PrintPrisoners(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return "Format is \"campaign.print_prisoners\".";
		}
		string text = "";
		foreach (Hero allAliveHero in Hero.AllAliveHeroes)
		{
			if (allAliveHero.IsPrisoner)
			{
				text = string.Concat(text, allAliveHero.Name, "    (captor: ", allAliveHero.PartyBelongedToAsPrisoner.Name, ")\n");
			}
		}
		return text + "\nSuccess";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_companions", "campaign")]
	public static string AddCompanions(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_companions [Number]\".";
		if (!CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		if (!int.TryParse(strings[0], out var result))
		{
			return "Invalid number.\n" + text;
		}
		if (result <= 0)
		{
			return "Please enter a positive number\n" + text;
		}
		for (int i = 0; i < result; i++)
		{
			AddCompanion(new List<string>());
		}
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_companion", "campaign")]
	public static string AddCompanion(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return "Format is \"campaign.add_companion\".";
		}
		CharacterObject wanderer = CharacterObject.PlayerCharacter.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate((CharacterObject x) => x.Occupation == Occupation.Wanderer);
		Settlement randomElementWithPredicate = Settlement.All.GetRandomElementWithPredicate((Settlement settlement) => settlement.Culture == wanderer.Culture && settlement.IsTown);
		Hero hero = HeroCreator.CreateSpecialHero(wanderer, randomElementWithPredicate);
		GiveGoldAction.ApplyBetweenCharacters(null, hero, 20000, disableNotification: true);
		hero.SetHasMet();
		hero.ChangeState(Hero.CharacterStates.Active);
		AddCompanionAction.Apply(Clan.PlayerClan, hero);
		AddHeroToPartyAction.Apply(hero, MobileParty.MainParty);
		return "companion has been added.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_player_reputation_trait", "campaign")]
	public static string SetPlayerReputationTrait(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_player_reputation_trait [Trait] | [Number]\".";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		if (int.TryParse(separatedNames[1], out var result))
		{
			string text2 = separatedNames[0];
			foreach (TraitObject item in TraitObject.All)
			{
				if (item.Name.ToString().Replace(" ", "").Equals(text2.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase) || item.StringId == text2.Replace(" ", ""))
				{
					if (result >= item.MinValue && result <= item.MaxValue)
					{
						Hero.MainHero.SetTraitLevel(item, result);
						Campaign.Current.PlayerTraitDeveloper.UpdateTraitXPAccordingToTraitLevels();
						return $"Set {item.Name} to {result}.";
					}
					return $"Number must be between {item.MinValue} and {item.MaxValue}.";
				}
			}
			return "Trait not found\n" + text;
		}
		return "Please enter a number\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_player_traits", "campaign")]
	public static string PrintPlayerTrait(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckHelp(strings))
		{
			return "Format is \"campaign.print_player_traits\".";
		}
		string text = "";
		foreach (TraitObject item in TraitObject.All)
		{
			text = text + item.Name.ToString() + " Trait Level:  " + Hero.MainHero.GetTraitLevel(item) + " Trait Xp: " + Campaign.Current.PlayerTraitDeveloper.GetPropertyValue(item) + "\n";
		}
		return text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_horse", "campaign")]
	public static string AddHorse(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_horse [Number]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return text;
		}
		int result = 1;
		if (!int.TryParse(strings[0], out result))
		{
			return "Please enter a number\n" + text;
		}
		if (result > 0)
		{
			ItemObject itemObject = Items.All.FirstOrDefault((ItemObject x) => x.IsMountable);
			PartyBase.MainParty.ItemRoster.AddToCounts(itemObject, result);
			return $"Added {result} {itemObject.Name} to player inventory.";
		}
		return "Nothing added.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("give_settlement_to_player", "campaign")]
	public static string GiveSettlementToPlayer(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.give_settlement_to_player [SettlementName/SettlementId]\nWrite \"campaign.give_settlement_to_player help\" to list available settlements.\nWrite \"campaign.give_settlement_to_player Calradia\" to give all settlements to player.";
		if (CheckParameters(strings, 0))
		{
			return text;
		}
		string text2 = ConcatenateString(strings);
		if (text2.ToLower() == "help")
		{
			string text3 = "";
			text3 += "\n";
			text3 += "Available settlements";
			text3 += "\n";
			text3 += "==============================";
			text3 += "\n";
			{
				foreach (Settlement objectType in MBObjectManager.Instance.GetObjectTypeList<Settlement>())
				{
					text3 = string.Concat(text3, "Id: ", objectType.StringId, " Name: ", objectType.Name, "\n");
				}
				return text3;
			}
		}
		string value = text2;
		MBReadOnlyList<Settlement> objectTypeList = MBObjectManager.Instance.GetObjectTypeList<Settlement>();
		if (text2.ToLower().Replace(" ", "") == "calradia")
		{
			foreach (Settlement item in objectTypeList)
			{
				if (item.IsCastle || item.IsTown)
				{
					ChangeOwnerOfSettlementAction.ApplyByDefault(Hero.MainHero, item);
				}
			}
			return "You own all of Calradia now!";
		}
		Settlement settlement = GetSettlement(text2);
		if (settlement == null)
		{
			foreach (Settlement item2 in objectTypeList)
			{
				if (item2.Name.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase))
				{
					settlement = item2;
					break;
				}
			}
		}
		if (settlement == null)
		{
			return "Given settlement name or id could not be found.\n" + text;
		}
		if (settlement.IsVillage)
		{
			return "Settlement must be castle or town.";
		}
		ChangeOwnerOfSettlementAction.ApplyByDefault(Hero.MainHero, settlement);
		return string.Concat(settlement.Name, " has been given to the player.");
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("give_settlement_to_kingdom", "campaign")]
	public static string GiveSettlementToKingdom(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.give_settlement_to_kingdom [SettlementName] | [KingdomName]";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		Settlement settlement = GetSettlement(separatedNames[0]);
		if (settlement == null)
		{
			return "Given settlement name could not be found.\n" + text;
		}
		if (settlement.IsVillage)
		{
			settlement = settlement.Village.Bound;
		}
		Kingdom kingdom = GetKingdom(separatedNames[1]);
		if (kingdom == null)
		{
			return "Given kingdom could not be found.\n" + text;
		}
		if (settlement.MapFaction == kingdom)
		{
			return "Kingdom already owns the settlement.";
		}
		if (settlement.IsVillage)
		{
			return "Settlement must be castle or town.";
		}
		ChangeOwnerOfSettlementAction.ApplyByDefault(kingdom.Leader, settlement);
		return string.Concat(settlement.Name, $" has been given to {kingdom.Leader.Name}.");
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_power_to_notable", "campaign")]
	public static string AddPowerToNotable(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is campaign.add_power_to_notable [HeroName] | [Number]";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		if (!int.TryParse(separatedNames[1], out var result))
		{
			return "Please enter a positive number\n" + text;
		}
		if (result <= 0)
		{
			return "Please enter a positive number\n" + text;
		}
		Hero hero = GetHero(separatedNames[0]);
		if (hero == null)
		{
			return "Hero is not found\n" + text;
		}
		if (!hero.IsNotable)
		{
			return "Hero is not a notable.";
		}
		hero.AddPower(result);
		return $"{hero.Name} power is {hero.Power}";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("kill_hero", "campaign")]
	public static string KillHero(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.kill_hero [HeroName]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return text;
		}
		string text2 = ConcatenateString(strings);
		Hero hero = GetHero(text2);
		if (hero != null)
		{
			if (!hero.IsAlive)
			{
				return "Hero " + text2 + " is already dead.";
			}
			if (!hero.CanDie(KillCharacterAction.KillCharacterActionDetail.Murdered))
			{
				return "Hero cant die!";
			}
			if (hero == Hero.MainHero)
			{
				return "Hero " + text2 + " is main hero. Use [campaingn.make_main_hero_ill] to kill main hero.";
			}
			KillCharacterAction.ApplyByMurder(hero);
			return "Hero " + text2.ToLower() + " is killed.";
		}
		return "Hero is not found: " + text2.ToLower() + "\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("make_main_hero_ill", "campaign")]
	private static string KillMainHero(List<string> strings)
	{
		string ErrorType = "";
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		Campaign.Current.MainHeroIllDays = 500;
		Hero.MainHero.HitPoints = Hero.MainHero.CharacterObject.MaxHitPoints();
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_character_feats", "campaign")]
	public static string PrintCharacterFeats(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.print_character_feats [HeroName]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return text;
		}
		string text2 = ConcatenateString(strings);
		Hero hero = GetHero(text2);
		string text3 = "";
		if (hero != null)
		{
			foreach (FeatObject item in FeatObject.All)
			{
				text3 = string.Concat(text3, "\n", item.Name, " :", hero.Culture.HasFeat(item).ToString());
			}
			return text3;
		}
		return "Hero is not found: " + text2 + "\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("make_hero_fugitive", "campaign")]
	public static string MakeHeroFugitive(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.make_hero_fugitive [HeroName]";
		if (CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return text;
		}
		string text2 = ConcatenateString(strings);
		Hero hero = GetHero(text2);
		if (hero != null)
		{
			if (!hero.IsAlive)
			{
				return "Hero " + text2 + " is dead.";
			}
			if (hero.PartyBelongedTo != null)
			{
				if (hero.PartyBelongedTo == MobileParty.MainParty)
				{
					return "You cannot be fugitive when you are in your main party.";
				}
				DestroyPartyAction.Apply(null, hero.PartyBelongedTo);
			}
			MakeHeroFugitiveAction.Apply(hero);
			return "Hero " + text2.ToLower() + " is now fugitive.";
		}
		return "Hero is not found: " + text2.ToLower() + "\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("leave_faction", "campaign")]
	public static string LeaveFaction(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckHelp(strings))
		{
			return "Format is \"campaign.leave_faction\".";
		}
		if (Hero.MainHero.MapFaction == Clan.PlayerClan)
		{
			return "Function execution failed.";
		}
		if (Hero.MainHero.MapFaction.Leader == Hero.MainHero)
		{
			string text = "";
			text = ((Hero.MainHero.MapFaction.Name.ToString().ToLower() == "empire") ? "lord_1_1" : ((Hero.MainHero.MapFaction.Name.ToString().ToLower() == "sturgia") ? "lord_2_1" : ((Hero.MainHero.MapFaction.Name.ToString().ToLower() == "aserai") ? "lord_3_1" : ((Hero.MainHero.MapFaction.Name.ToString().ToLower() == "vlandia") ? "lord_4_1" : ((Hero.MainHero.MapFaction.Name.ToString().ToLower() == "battania") ? "lord_5_1" : ((!(Hero.MainHero.MapFaction.Name.ToString().ToLower() == "khuzait")) ? "lord_1_1" : "lord_6_1"))))));
			Hero heroObject = Game.Current.ObjectManager.GetObject<CharacterObject>(text).HeroObject;
			if (!Hero.MainHero.MapFaction.IsKingdomFaction)
			{
				(Hero.MainHero.MapFaction as Clan).SetLeader(heroObject);
			}
			else
			{
				ChangeRulingClanAction.Apply(Hero.MainHero.MapFaction as Kingdom, heroObject.Clan);
			}
		}
		ChangeKingdomAction.ApplyByLeaveKingdom(Hero.MainHero.Clan);
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("lead_your_faction", "campaign")]
	public static string LeadYourFaction(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckHelp(strings))
		{
			return "Format is \"campaign.lead_your_faction\".";
		}
		if (Hero.MainHero.MapFaction.Leader != Hero.MainHero)
		{
			if (Hero.MainHero.MapFaction.IsKingdomFaction)
			{
				ChangeRulingClanAction.Apply(Hero.MainHero.MapFaction as Kingdom, Clan.PlayerClan);
			}
			else
			{
				(Hero.MainHero.MapFaction as Clan).SetLeader(Hero.MainHero);
			}
			return "Success";
		}
		return "Function execution failed.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_heroes_suitable_for_marriage", "campaign")]
	public static string PrintHeroesSuitableForMarriage(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckHelp(strings))
		{
			return "Format is \"print_heroes_suitable_for_marriage\".";
		}
		List<Hero> list = new List<Hero>();
		List<Hero> list2 = new List<Hero>();
		foreach (Kingdom item in Kingdom.All)
		{
			foreach (Hero lord in item.Lords)
			{
				if (lord.CanMarry())
				{
					if (lord.IsFemale)
					{
						list.Add(lord);
					}
					else
					{
						list2.Add(lord);
					}
				}
			}
		}
		string text = "Maidens:\n";
		string text2 = "Suitors:\n";
		foreach (Hero item2 in list)
		{
			TextObject textObject = ((item2.PartyBelongedTo == null) ? TextObject.Empty : item2.PartyBelongedTo.Name);
			text = string.Concat(text, "Name: ", item2.Name, " --- Clan: ", item2.Clan, " --- Party:", textObject, "\n");
		}
		foreach (Hero item3 in list2)
		{
			TextObject textObject2 = ((item3.PartyBelongedTo == null) ? TextObject.Empty : item3.PartyBelongedTo.Name);
			text2 = string.Concat(text2, "Name: ", item3.Name, " --- Clan: ", item3.Clan, " --- Party:", textObject2, "\n");
		}
		return text + "\n" + text2;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("marry_player_with_hero", "campaign")]
	public static string MarryPlayerWithHero(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.marry_player_with_hero [HeroName]\".";
		if (!CheckParameters(strings, 0) && !CheckHelp(strings))
		{
			if (!Campaign.Current.Models.MarriageModel.IsSuitableForMarriage(Hero.MainHero))
			{
				return "Main hero is not suitable for marriage";
			}
			string text2 = ConcatenateString(strings);
			Hero hero = GetHero(text2);
			if (hero != null)
			{
				MarriageModel marriageModel = Campaign.Current.Models.MarriageModel;
				if (marriageModel.IsCoupleSuitableForMarriage(Hero.MainHero, hero))
				{
					MarriageAction.Apply(Hero.MainHero, hero);
					return "Success";
				}
				if (!marriageModel.IsSuitableForMarriage(hero))
				{
					return $"Hero: {hero.Name} is not suitable for marriage.";
				}
				if (!marriageModel.IsClanSuitableForMarriage(hero.Clan))
				{
					return $"{hero.Name}'s clan is not suitable for marriage.";
				}
				if (!marriageModel.IsClanSuitableForMarriage(Hero.MainHero.Clan))
				{
					return "Main hero's clan is not suitable for marriage.";
				}
				if (Hero.MainHero.Clan?.Leader == Hero.MainHero && hero.Clan?.Leader == hero)
				{
					return "Clan leaders are not suitable for marriage.";
				}
				if (!hero.IsFemale)
				{
					return "Hero is not female.";
				}
				DefaultMarriageModel obj = new DefaultMarriageModel();
				if ((bool)typeof(DefaultMarriageModel).GetMethod("AreHeroesRelated", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(obj, new object[2]
				{
					Hero.MainHero,
					hero
				}))
				{
					return "Heroes are related.";
				}
				Hero courtedHeroInOtherClan = Romance.GetCourtedHeroInOtherClan(Hero.MainHero, hero);
				if (courtedHeroInOtherClan != null && courtedHeroInOtherClan != hero)
				{
					return $"{courtedHeroInOtherClan.Name} has courted {Hero.MainHero.Name}.";
				}
				Hero courtedHeroInOtherClan2 = Romance.GetCourtedHeroInOtherClan(hero, Hero.MainHero);
				if (courtedHeroInOtherClan2 != null && courtedHeroInOtherClan2 != Hero.MainHero)
				{
					return $"{courtedHeroInOtherClan2.Name} has courted {hero.Name}.";
				}
				return string.Concat("Marriage is not suitable between ", Hero.MainHero.Name, " and ", hero.Name, "\n");
			}
			return "Hero is not found: " + text2.ToLower() + "\n" + text;
		}
		return text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("is_hero_suitable_for_marriage_with_player", "campaign")]
	public static string IsHeroSuitableForMarriageWithPlayer(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.is_hero_suitable_for_marriage_with_player [HeroName]\".";
		if (!CheckParameters(strings, 0) && !CheckHelp(strings))
		{
			string text2 = ConcatenateString(strings);
			Hero hero = GetHero(text2);
			if (hero != null)
			{
				if (Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(Hero.MainHero, hero))
				{
					return string.Concat("Marriage is suitable between ", Hero.MainHero.Name, " and ", hero.Name, "\n");
				}
				return string.Concat("Marriage is not suitable between ", Hero.MainHero.Name, " and ", hero.Name, "\n");
			}
			return "Hero is not found: " + text2.ToLower() + "\n" + text;
		}
		return text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("start_player_vs_world_war", "campaign")]
	public static string StartPlayerVsWorldWar(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckHelp(strings))
		{
			return "Format is \"campaign.start_player_vs_world_war\".";
		}
		foreach (IFaction faction in Campaign.Current.Factions)
		{
			if ((faction != Clan.PlayerClan || faction != Hero.MainHero.MapFaction) && !faction.IsEliminated && (faction.IsKingdomFaction || faction.IsMinorFaction))
			{
				DeclareWarAction.ApplyByDefault(faction, Clan.PlayerClan);
			}
		}
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("start_player_vs_world_truce", "campaign")]
	public static string StartPlayerVsWorldTruce(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckHelp(strings))
		{
			return "Format is \"campaign.start_player_vs_world_truce\".";
		}
		foreach (IFaction faction in Campaign.Current.Factions)
		{
			if (faction != Clan.PlayerClan || faction != Hero.MainHero.MapFaction)
			{
				MakePeaceAction.Apply(faction, Clan.PlayerClan);
			}
		}
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("create_player_kingdom", "campaign")]
	public static string CreatePlayerKingdom(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckHelp(strings) || !CheckParameters(strings, 0))
		{
			return "Format is \"campaign.create_player_kingdom\".";
		}
		Campaign.Current.KingdomManager.CreateKingdom(Clan.PlayerClan.Name, Clan.PlayerClan.InformalName, Clan.PlayerClan.Culture, Clan.PlayerClan);
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("create_random_clan", "campaign")]
	public static string CreateRandomClan(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.create_random_clan [KingdomName]\".";
		if (CheckHelp(strings) || CheckParameters(strings, 0))
		{
			return text;
		}
		Kingdom kingdom = null;
		kingdom = ((strings.Count <= 0) ? Kingdom.All.GetRandomElement() : GetKingdom(ConcatenateString(strings)));
		if (kingdom == null)
		{
			return "Kingdom is not valid!\n" + text;
		}
		CultureObject culture = kingdom.Culture;
		Settlement settlement = kingdom.Settlements.FirstOrDefault((Settlement x) => x.IsTown) ?? kingdom.Settlements.GetRandomElement() ?? Settlement.All.FirstOrDefault((Settlement x) => x.IsTown && x.Culture == culture);
		TextObject name = NameGenerator.Current.GenerateClanName(culture, settlement);
		Clan clan = Clan.CreateClan("test_clan_" + Clan.All.Count);
		clan.InitializeClan(name, new TextObject("{=!}informal"), Kingdom.All.GetRandomElement().Culture, Banner.CreateRandomClanBanner());
		CharacterObject characterObject = culture.LordTemplates.FirstOrDefault((CharacterObject x) => x.Occupation == Occupation.Lord);
		if (characterObject == null)
		{
			return "Can't find a proper lord template.\n" + text;
		}
		Settlement bornSettlement = kingdom.Settlements.GetRandomElement() ?? Settlement.All.FirstOrDefault((Settlement x) => x.IsTown && x.Culture == culture);
		Hero hero = HeroCreator.CreateSpecialHero(characterObject, bornSettlement, clan, null, MBRandom.RandomInt(18, 36));
		hero.HeroDeveloper.InitializeHeroDeveloper();
		hero.ChangeState(Hero.CharacterStates.Active);
		clan.SetLeader(hero);
		ChangeKingdomAction.ApplyByJoinToKingdom(clan, kingdom, showNotification: false);
		EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
		GiveGoldAction.ApplyBetweenCharacters(null, hero, 15000);
		return $"{clan.Name} is added to {kingdom.Name}. Its leader is: {hero.Name}";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("join_kingdom", "campaign")]
	public static string JoinKingdom(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.join_kingdom[KingdomName / FirstTwoCharactersOfKingdomName]\".\nWrite \"campaign.join_kingdom help\" to list available Kingdoms.";
		if (CheckParameters(strings, 0))
		{
			return text;
		}
		string text2 = ConcatenateString(strings).Replace(" ", "");
		if (text2.ToLower() == "help")
		{
			string text3 = "";
			text3 += "\n";
			text3 += "Format is \"campaign.join_kingdom [KingdomName/FirstTwoCharacterOfKingdomName]\".";
			text3 += "\n";
			text3 += "Available Kingdoms";
			text3 += "\n";
			{
				foreach (Kingdom item in Kingdom.All)
				{
					text3 = text3 + "Kingdom name " + item.Name.ToString() + "\n";
				}
				return text3;
			}
		}
		Kingdom kingdom = null;
		foreach (Kingdom item2 in Kingdom.All)
		{
			if (item2.Name.ToString().Equals(text2, StringComparison.OrdinalIgnoreCase))
			{
				kingdom = item2;
				break;
			}
			if (text2.Length >= 2 && item2.Name.ToString().ToLower().Substring(0, 2)
				.Equals(text2.ToLower().Substring(0, 2)))
			{
				kingdom = item2;
				break;
			}
		}
		if (kingdom == null)
		{
			return "Kingdom is not found: " + text2 + "\n" + text;
		}
		if (Hero.MainHero.Clan.Kingdom == kingdom)
		{
			return "Already in kingdom";
		}
		ChangeKingdomAction.ApplyByJoinToKingdom(Hero.MainHero.Clan, kingdom);
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("join_kingdom_as_mercenary", "campaign")]
	public static string JoinKingdomAsMercenary(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.join_kingdom_as_mercenary[KingdomName / FirstTwoCharactersOfKingdomName]\".\nWrite \"campaign.join_kingdom_as_mercenary help\" to list available Kingdoms.";
		if (CheckParameters(strings, 0))
		{
			return text;
		}
		string text2 = ConcatenateString(strings).Replace(" ", "");
		if (text2.ToLower() == "help")
		{
			string text3 = "";
			text3 += "\n";
			text3 += "Format is \"campaign.join_kingdom_as_mercenary [KingdomName/FirstTwoCharacterOfKingdomName]\".";
			text3 += "\n";
			text3 += "Available Kingdoms";
			text3 += "\n";
			{
				foreach (Kingdom item in Kingdom.All)
				{
					text3 = text3 + "Kingdom name " + item.Name.ToString() + "\n";
				}
				return text3;
			}
		}
		Kingdom kingdom = null;
		foreach (Kingdom item2 in Kingdom.All)
		{
			if (item2.Name.ToString().Equals(text2, StringComparison.OrdinalIgnoreCase))
			{
				kingdom = item2;
				break;
			}
			if (text2.Length >= 2 && item2.Name.ToString().ToLower().Substring(0, 2)
				.Equals(text2.ToLower().Substring(0, 2)))
			{
				kingdom = item2;
				break;
			}
		}
		if (kingdom == null)
		{
			return "Kingdom is not found: " + text2 + "\n" + text;
		}
		ChangeKingdomAction.ApplyByJoinFactionAsMercenary(Hero.MainHero.Clan, kingdom);
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_criminal_rating", "campaign")]
	public static string SetCriminalRating(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		int result = 0;
		if (CheckHelp(strings))
		{
			string text = "";
			text += "\n";
			text += "Format is \"campaign.set_criminal_rating [Faction] | [Gold]\".";
			text += "\n";
			text += "Available Factions";
			text += "\n";
			foreach (Kingdom item in Kingdom.All)
			{
				text = text + "Faction name " + item.Name.ToString() + "\n";
			}
			{
				foreach (Clan nonBanditFaction in Clan.NonBanditFactions)
				{
					text = text + "Faction name " + nonBanditFaction.Name.ToString() + "\n";
				}
				return text;
			}
		}
		string text2 = "Format is \"campaign.set_criminal_rating [FactionName] | [Value]\".\nWrite \"campaign.set_criminal_rating help\" to list available Factions.";
		if (CheckParameters(strings, 0))
		{
			return text2;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text2;
		}
		if (!int.TryParse(separatedNames[1], out result))
		{
			return text2;
		}
		string text3 = separatedNames[0];
		foreach (Clan nonBanditFaction2 in Clan.NonBanditFactions)
		{
			if (nonBanditFaction2.Name.ToString().Replace(" ", "").Equals(text3.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
			{
				ChangeCrimeRatingAction.Apply(nonBanditFaction2, (float)result - nonBanditFaction2.MainHeroCrimeRating);
				return "Success";
			}
		}
		foreach (Kingdom item2 in Kingdom.All)
		{
			if (item2.Name.ToString().Replace(" ", "").Equals(text3.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
			{
				ChangeCrimeRatingAction.Apply(item2, (float)result - item2.MainHeroCrimeRating);
				return "Success";
			}
		}
		return "Faction is not found: " + text3 + "\n" + text2;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_criminal_ratings", "campaign")]
	public static string PrintCriminalRatings(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return "Format is \"campaign.print_criminal_ratings";
		}
		string text = "";
		bool flag = true;
		foreach (Kingdom item in Kingdom.All)
		{
			if (item.MainHeroCrimeRating > 0f)
			{
				text = string.Concat(text, item.Name, "   criminal rating: ", item.MainHeroCrimeRating, "\n");
				flag = false;
			}
		}
		text += "-----------\n";
		foreach (Clan nonBanditFaction in Clan.NonBanditFactions)
		{
			if (nonBanditFaction.MainHeroCrimeRating > 0f)
			{
				text = string.Concat(text, nonBanditFaction.Name, "   criminal rating: ", nonBanditFaction.MainHeroCrimeRating, "\n");
				flag = false;
			}
		}
		if (flag)
		{
			return "You don't have any criminal rating.";
		}
		return text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_main_hero_age", "campaign")]
	public static string SetMainHeroAge(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_main_hero_age [Age]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return text;
		}
		int result = 1;
		if (!int.TryParse(strings[0], out result))
		{
			return "Please enter a number\n" + text;
		}
		if (result < Campaign.Current.Models.AgeModel.HeroComesOfAge || result > Campaign.Current.Models.AgeModel.MaxAge)
		{
			return $"Age must be between {Campaign.Current.Models.AgeModel.HeroComesOfAge} - {Campaign.Current.Models.AgeModel.MaxAge}";
		}
		Hero.MainHero.SetBirthDay(HeroHelper.GetRandomBirthDayForAge(result));
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_main_party_attackable", "campaign")]
	public static string SetMainPartyAttackable(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is campaign.set_main_party_attackable [1/0]\".";
		if (CheckHelp(strings) || !CheckParameters(strings, 1))
		{
			return text;
		}
		if (strings[0] == "0" || strings[0] == "1")
		{
			return "Main party is" + ((_mainPartyIsAttackable = strings[0] == "1") ? " " : " NOT ") + "attackable.";
		}
		return "Wrong input.\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_morale_to_party", "campaign")]
	public static string AddMoraleToParty(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_morale_to_party [HeroName] | [Number]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return text;
		}
		int result = 10;
		Hero hero = Hero.MainHero;
		string text2 = "";
		bool flag = false;
		if (CheckParameters(strings, 1))
		{
			if (!int.TryParse(strings[0], out result))
			{
				result = 10;
				text2 = strings[0];
				hero = GetHero(text2);
				flag = true;
			}
		}
		else
		{
			List<string> separatedNames = GetSeparatedNames(strings, "|");
			if (separatedNames.Count != 2)
			{
				return text;
			}
			if (!int.TryParse(separatedNames[1], out result))
			{
				result = 100;
				text2 = separatedNames[0];
				hero = GetHero(text2);
			}
			else
			{
				text2 = separatedNames[0];
				hero = GetHero(text2);
			}
			flag = true;
		}
		if (hero != null)
		{
			MobileParty partyBelongedTo = hero.PartyBelongedTo;
			if (partyBelongedTo != null)
			{
				float num = MBMath.ClampFloat(partyBelongedTo.RecentEventsMorale + (float)result, 0f, float.MaxValue);
				float num2 = num - partyBelongedTo.RecentEventsMorale;
				partyBelongedTo.RecentEventsMorale = num;
				return $"The base morale of {hero.Name}'s party changed by {num2}.";
			}
			return "Hero: " + text2 + " does not belonged to any party.\n" + text;
		}
		if (flag)
		{
			return "Hero: " + text2 + " not found.\n" + text;
		}
		return "Wrong input.\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("boost_cohesion_of_army", "campaign")]
	public static string BoostCohesionOfArmy(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"boost_cohesion_of_army [ArmyLeaderName]\".";
		if (CheckHelp(strings))
		{
			return text;
		}
		Hero hero = Hero.MainHero;
		string text2 = "";
		Army army = hero.PartyBelongedTo.Army;
		if (!CheckParameters(strings, 0))
		{
			text2 = ConcatenateString(strings.GetRange(0, strings.Count));
			hero = GetHero(text2);
			if (hero == null)
			{
				return "Hero: " + text2 + " not found.\n" + text;
			}
			if (hero.PartyBelongedTo == null)
			{
				return "Hero: " + text2 + " does not belong to any army.";
			}
			army = hero.PartyBelongedTo.Army;
			if (army == null)
			{
				return "Hero: " + text2 + " does not belong to any army.";
			}
		}
		if (army != null)
		{
			army.Cohesion = 100f;
			return $"{hero.Name}'s army cohesion is boosted.";
		}
		return "Wrong input.\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("boost_cohesion_of_all_armies", "campaign")]
	public static string BoostCohersionOfAllArmies(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckHelp(strings))
		{
			return "Format is \"boost_cohersion_of_all_armies\".";
		}
		if (CheckParameters(strings, 0))
		{
			foreach (MobileParty item in MobileParty.All)
			{
				if (item.Army != null)
				{
					item.Army.Cohesion = 100f;
				}
			}
			return "All armies cohesion are boosted.";
		}
		return "Wrong input.\nFormat is \"boost_cohesion_of_all_armies\".";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_focus_points_to_hero", "campaign")]
	public static string AddFocusPointCheat(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_focus_points_to_hero [HeroName] | [PositiveNumber]\".";
		if (CheckHelp(strings))
		{
			return text;
		}
		bool flag = false;
		int num = 1;
		Hero mainHero;
		if (CheckParameters(strings, 0))
		{
			Hero.MainHero.HeroDeveloper.UnspentFocusPoints = MBMath.ClampInt(Hero.MainHero.HeroDeveloper.UnspentFocusPoints + 1, 0, int.MaxValue);
			mainHero = Hero.MainHero;
			return $"{num} focus points added to the {mainHero.Name}. ";
		}
		int result = 0;
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count == 1)
		{
			bool flag2 = int.TryParse(separatedNames[0], out result);
			if (result <= 0 && flag2)
			{
				return "Please enter a positive number\n" + text;
			}
			Hero.MainHero.HeroDeveloper.UnspentFocusPoints = MBMath.ClampInt(Hero.MainHero.HeroDeveloper.UnspentFocusPoints + result, 0, int.MaxValue);
			mainHero = Hero.MainHero;
			flag = true;
			num = result;
		}
		else
		{
			if (separatedNames.Count != 2)
			{
				return text;
			}
			if (int.TryParse(separatedNames[1], out result))
			{
				mainHero = GetHero(separatedNames[0]);
				if (mainHero != null)
				{
					mainHero.HeroDeveloper.UnspentFocusPoints = MBMath.ClampInt(mainHero.HeroDeveloper.UnspentFocusPoints + result, 0, int.MaxValue);
					flag = true;
					num = result;
				}
			}
			else
			{
				mainHero = GetHero(separatedNames[0]);
				if (mainHero != null)
				{
					mainHero.HeroDeveloper.UnspentFocusPoints = MBMath.ClampInt(mainHero.HeroDeveloper.UnspentFocusPoints + 1, 0, int.MaxValue);
					flag = true;
				}
			}
		}
		if (flag)
		{
			return $"{num} focus points added to the {mainHero.Name}. ";
		}
		return "Hero is not found\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_attribute_points_to_hero", "campaign")]
	public static string AddAttributePointsCheat(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_attribute_points_to_hero [HeroName] | [PositiveNumber]\".";
		if (CheckHelp(strings))
		{
			return text;
		}
		bool flag = false;
		int num = 1;
		Hero mainHero;
		if (CheckParameters(strings, 0))
		{
			Hero.MainHero.HeroDeveloper.UnspentAttributePoints = MBMath.ClampInt(Hero.MainHero.HeroDeveloper.UnspentAttributePoints + 1, 0, int.MaxValue);
			mainHero = Hero.MainHero;
			return $"{num} attribute points added to the {mainHero.Name}. ";
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count == 1)
		{
			if (!int.TryParse(separatedNames[0], out var result) || result <= 0)
			{
				return "Please enter a positive number\n" + text;
			}
			Hero.MainHero.HeroDeveloper.UnspentAttributePoints = MBMath.ClampInt(Hero.MainHero.HeroDeveloper.UnspentAttributePoints + result, 0, int.MaxValue);
			mainHero = Hero.MainHero;
			flag = true;
			num = result;
		}
		else
		{
			if (separatedNames.Count != 2)
			{
				return text;
			}
			mainHero = GetHero(separatedNames[0]);
			if (mainHero != null)
			{
				flag = true;
				if (int.TryParse(separatedNames[1], out var result2))
				{
					num = result2;
				}
				mainHero.HeroDeveloper.UnspentAttributePoints = MBMath.ClampInt(mainHero.HeroDeveloper.UnspentAttributePoints + num, 0, int.MaxValue);
			}
		}
		if (flag)
		{
			return $"{num} attribute points added to the {mainHero.Name}. ";
		}
		return "Hero is not found\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_tournaments", "campaign")]
	public static string PrintSettlementsWithTournament(List<string> strings)
	{
		string ErrorType = "";
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!Campaign.Current.IsDay)
		{
			return "Cant print tournaments. Wait day light.";
		}
		string text = "";
		foreach (Town allTown in Town.AllTowns)
		{
			if (Campaign.Current.TournamentManager.GetTournamentGame(allTown) != null)
			{
				text = string.Concat(text, allTown.Name, "\n");
			}
		}
		return text;
	}

	public static string ConvertListToMultiLine(List<string> strings)
	{
		string text = "";
		foreach (string @string in strings)
		{
			text = text + @string + "\n";
		}
		return text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_all_issues", "campaign")]
	public static string PrintAllIssues(List<string> strings)
	{
		string ErrorType = "";
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Total issue count : " + Campaign.Current.IssueManager.Issues.Count + "\n";
		int num = 0;
		foreach (KeyValuePair<Hero, IssueBase> issue in Campaign.Current.IssueManager.Issues)
		{
			text = string.Concat(text, ++num, ") ", issue.Value.Title, ", ", issue.Key, ": ", issue.Value.IssueSettlement, "\n");
		}
		return text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_issues", "campaign")]
	public static string PrintIssues(List<string> strings)
	{
		string ErrorType = "";
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "";
		Dictionary<Type, string> dictionary = new Dictionary<Type, string>();
		foreach (KeyValuePair<Hero, IssueBase> issue in Campaign.Current.IssueManager.Issues)
		{
			if (!dictionary.ContainsKey(issue.Value.GetType()))
			{
				dictionary.Add(issue.Value.GetType(), string.Concat(issue.Value.Title, ", ", issue.Key, ": ", issue.Value.IssueSettlement, "\n"));
			}
		}
		foreach (KeyValuePair<Type, string> item in dictionary)
		{
			text += item.Value;
		}
		return text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("give_workshop_to_player", "campaign")]
	public static string GiveWorkshopToPlayer(List<string> strings)
	{
		string text = "Format is \"campaign.give_workshop_to_player [SettlementName] | [workshop_no]\".";
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (!CheckCheatUsage(ref ErrorType))
		{
			return text;
		}
		if (CheckHelp(strings) || separatedNames.Count != 2)
		{
			if (Settlement.CurrentSettlement != null)
			{
				if (Settlement.CurrentSettlement.Town == null)
				{
					return "Settlement should be town\n";
				}
				if (separatedNames.Count == 1)
				{
					if (int.TryParse(strings[0], out var result))
					{
						if (result > 0 && result < Settlement.CurrentSettlement.Town.Workshops.Length)
						{
							Workshop workshop = Settlement.CurrentSettlement.Town.Workshops[result];
							ChangeOwnerOfWorkshopAction.ApplyByPlayerBuying(workshop);
							return $"Gave {workshop.WorkshopType.Name} to {Hero.MainHero.Name}";
						}
						return $"There is no workshop with no {result}.";
					}
					return "Please enter a number\n" + text;
				}
				string text2 = text;
				for (int i = 0; i < Settlement.CurrentSettlement.Town.Workshops.Length; i++)
				{
					Workshop workshop2 = Settlement.CurrentSettlement.Town.Workshops[i];
					text2 = string.Concat(text2, "\n", i, " : ", workshop2.Name, " - owner : ", (workshop2.Owner != null) ? workshop2.Owner.Name.ToString() : "");
					if (workshop2.WorkshopType.IsHidden)
					{
						text2 += "(hidden)";
					}
				}
				return text2;
			}
			return "You need to be in a settlement to see the workshops available.";
		}
		Settlement settlement = GetSettlement(separatedNames[0]);
		if (settlement == null || !settlement.IsTown)
		{
			return "Settlement should be a town.";
		}
		if (int.TryParse(separatedNames[1], out var result2))
		{
			if (result2 >= 0 && result2 < settlement.Town.Workshops.Length)
			{
				Workshop workshop3 = settlement.Town.Workshops[result2];
				if (workshop3.WorkshopType.IsHidden)
				{
					return "Workshop is hidden.";
				}
				ChangeOwnerOfWorkshopAction.ApplyByPlayerBuying(workshop3);
				return "Success";
			}
			return $"There is no workshop with no {result2}.";
		}
		return "Please enter a number\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("conceive_child", "campaign")]
	public static string MakePregnant(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (Hero.MainHero.Spouse == null)
		{
			Hero hero = Hero.AllAliveHeroes.FirstOrDefault((Hero t) => t != Hero.MainHero && Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(Hero.MainHero, t));
			if (hero != null)
			{
				MarriageAction.Apply(Hero.MainHero, hero);
				if (Hero.MainHero.IsFemale ? (!Hero.MainHero.IsPregnant) : (!Hero.MainHero.Spouse.IsPregnant))
				{
					MakePregnantAction.Apply(Hero.MainHero.IsFemale ? Hero.MainHero : Hero.MainHero.Spouse);
					return "Success";
				}
				return "You are expecting a child already.";
			}
			return "error";
		}
		if (Hero.MainHero.IsFemale ? (!Hero.MainHero.IsPregnant) : (!Hero.MainHero.Spouse.IsPregnant))
		{
			MakePregnantAction.Apply(Hero.MainHero.IsFemale ? Hero.MainHero : Hero.MainHero.Spouse);
			return "Success";
		}
		return "You are expecting a child already.";
	}

	public static Hero GenerateChild(Hero hero, bool isFemale, CultureObject culture)
	{
		if (hero.Spouse == null)
		{
			List<Hero> list = Hero.AllAliveHeroes.ToList();
			list.Shuffle();
			Hero hero2 = list.FirstOrDefault((Hero t) => t != hero && Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(hero, t));
			if (hero2 != null)
			{
				MarriageAction.Apply(hero, hero2);
				if (hero.IsFemale ? (!hero.IsPregnant) : (!hero.Spouse.IsPregnant))
				{
					MakePregnantAction.Apply(hero.IsFemale ? hero : hero.Spouse);
				}
			}
		}
		Hero hero3 = (hero.IsFemale ? hero : hero.Spouse);
		Hero spouse = hero3.Spouse;
		Hero hero4 = HeroCreator.DeliverOffSpring(hero3, spouse, isFemale);
		hero4.Culture = culture;
		hero3.IsPregnant = false;
		return hero4;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_prisoner_to_party", "campaign")]
	public static string AddPrisonerToParty(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_prisoner_to_party [PrisonerName] | [CapturerName]\".";
		if (CheckHelp(strings) || CheckParameters(strings, 0) || CheckParameters(strings, 1))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		string heroName = separatedNames[0].Trim();
		string heroName2 = separatedNames[1].Trim();
		Hero hero = GetHero(heroName);
		Hero hero2 = GetHero(heroName2);
		if (hero == null || hero2 == null)
		{
			return "Can't find one of the heroes.\n" + text;
		}
		if (!hero2.IsActive || hero2.PartyBelongedTo == null)
		{
			return "Capturer hero is not active!";
		}
		if (!hero.IsActive || hero.IsHumanPlayerCharacter || (hero.Occupation != Occupation.Lord && hero.Occupation != Occupation.Wanderer))
		{
			return "Hero can't be taken as a prisoner!";
		}
		if (!FactionManager.IsAtWarAgainstFaction(hero.MapFaction, hero2.MapFaction))
		{
			return "Factions are not at war!";
		}
		if (hero.PartyBelongedTo != null)
		{
			if (hero.PartyBelongedTo.MapEvent != null)
			{
				return "prisoners party shouldn't be in a map event.";
			}
			if (hero.PartyBelongedTo.LeaderHero == hero)
			{
				DestroyPartyAction.Apply(null, hero.PartyBelongedTo);
			}
			else
			{
				hero.PartyBelongedTo.MemberRoster.RemoveTroop(hero.CharacterObject);
			}
		}
		if (hero.IsPrisoner)
		{
			EndCaptivityAction.ApplyByEscape(hero);
		}
		if (hero.CurrentSettlement != null)
		{
			LeaveSettlementAction.ApplyForCharacterOnly(hero);
		}
		if (hero2.IsHumanPlayerCharacter)
		{
			hero.SetHasMet();
		}
		TakePrisonerAction.Apply(hero2.PartyBelongedTo.Party, hero);
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_random_prisoner_hero", "campaign")]
	public static string AddRandomPrisonerHeroCheat(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckHelp(strings))
		{
			return "Format is \"campaign.add_random_prisoner_hero\".";
		}
		if (!Hero.MainHero.IsPartyLeader)
		{
			return $"{Hero.MainHero.Name} is not a party leader.";
		}
		Hero randomElementWithPredicate = Hero.AllAliveHeroes.GetRandomElementWithPredicate((Hero x) => !x.CharacterObject.IsPlayerCharacter && x.IsActive && x.PartyBelongedTo == null && !x.IsPrisoner && x.CharacterObject.Occupation == Occupation.Lord);
		if (randomElementWithPredicate == null)
		{
			return "There is not any available heroes right now.";
		}
		if (randomElementWithPredicate.CurrentSettlement != null)
		{
			LeaveSettlementAction.ApplyForCharacterOnly(randomElementWithPredicate);
		}
		bool num = randomElementWithPredicate.PartyBelongedTo?.LeaderHero == randomElementWithPredicate;
		MobileParty partyBelongedTo = randomElementWithPredicate.PartyBelongedTo;
		TakePrisonerAction.Apply(PartyBase.MainParty, randomElementWithPredicate);
		randomElementWithPredicate.SetHasMet();
		if (num)
		{
			DisbandPartyAction.StartDisband(partyBelongedTo);
			partyBelongedTo.IsDisbanding = true;
		}
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("control_party_ai_by_cheats", "campaign")]
	public static string ControlPartyAIByCheats(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string result = "Format is \"campaign.control_party_ai_by_cheats [HeroName] | [0|1] \".";
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (CheckParameters(strings, 0) || CheckHelp(strings) || separatedNames.Count == 1)
		{
			return result;
		}
		if (separatedNames.Count != 2)
		{
			return result;
		}
		if (separatedNames[1] != "0" && separatedNames[1] != "1")
		{
			return result;
		}
		Hero hero = GetHero(separatedNames[0]);
		bool enable = separatedNames[1] == "1";
		ControlPartyAIByCheatsInternal(hero, enable, out var resultDescription);
		return resultDescription;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("ai_siege_settlement", "campaign")]
	public static string AISiegeSettlement(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.ai_siege_settlement [HeroName] | [SettlementName]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings) || CheckParameters(strings, 1))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		Hero hero = GetHero(separatedNames[0]);
		Settlement settlement = GetSettlement(separatedNames[1]);
		if (hero == null)
		{
			return "Hero is not found\n" + text;
		}
		if (settlement == null)
		{
			return "Settlement is not found\n" + text;
		}
		if (!settlement.IsFortification)
		{
			return "Settlement is not a fortification (Town or Castle)";
		}
		if (hero.MapFaction == settlement.MapFaction)
		{
			return $"Hero Faction: {hero.MapFaction.Name} and Settlement Faction: {settlement.MapFaction.Name} are the same";
		}
		if (!FactionManager.IsAtWarAgainstFaction(hero.MapFaction, settlement.MapFaction))
		{
			return $"Hero Faction: {hero.MapFaction.Name} and Settlement Faction: {settlement.MapFaction.Name} are not at war, you can use \"campaign.declare_war\" cheat";
		}
		if (ControlPartyAIByCheatsInternal(hero, enable: true, out var resultDescription))
		{
			SetPartyAiAction.GetActionForBesiegingSettlement(hero.PartyBelongedTo, settlement);
			return resultDescription + "\nParty AI can be enabled again by using \"campaign.control_party_ai_by_cheats\"\nSuccess";
		}
		return resultDescription;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("ai_raid_village", "campaign")]
	public static string AIRaidVillage(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.ai_raid_village [HeroName] | [VillageName]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings) || CheckParameters(strings, 1))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		Hero hero = GetHero(separatedNames[0]);
		Settlement settlement = GetSettlement(separatedNames[1]);
		if (hero == null)
		{
			return "Hero is not found\n" + text;
		}
		if (settlement == null)
		{
			return "Settlement is not found\n" + text;
		}
		if (!settlement.IsVillage)
		{
			return "Settlement is not a village.";
		}
		if (hero.MapFaction == settlement.MapFaction)
		{
			return $"Hero Faction: {hero.MapFaction.Name} and Village Faction: {settlement.MapFaction.Name} are the same";
		}
		if (!FactionManager.IsAtWarAgainstFaction(hero.MapFaction, settlement.MapFaction))
		{
			return $"Hero Faction: {hero.MapFaction.Name} and Village Faction: {settlement.MapFaction.Name} are not at war, you can use \"campaign.declare_war\" cheat";
		}
		if (settlement.IsUnderRaid)
		{
			return "Village is already under raid.";
		}
		if (ControlPartyAIByCheatsInternal(hero, enable: true, out var resultDescription))
		{
			SetPartyAiAction.GetActionForRaidingSettlement(hero.PartyBelongedTo, settlement);
			return resultDescription + "\nParty AI can be enabled again by using \"campaign.control_party_ai_by_cheats\"\nSuccess";
		}
		return resultDescription;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("ai_attack_party", "campaign")]
	public static string AIAttackParty(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.ai_attack_party [AttackerHeroName] | [HeroName]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings) || CheckParameters(strings, 1))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		Hero hero = GetHero(separatedNames[0]);
		Hero hero2 = GetHero(separatedNames[1]);
		if (hero == null || hero2 == null)
		{
			return "Hero is not found\n" + text;
		}
		if (hero2.PartyBelongedTo == null)
		{
			return "Second hero is not in a party";
		}
		if (hero.MapFaction == hero2.MapFaction)
		{
			return $"Attacker Hero Faction: {hero.MapFaction.Name} and Other Hero Faction: {hero2.MapFaction.Name} are the same";
		}
		if (!FactionManager.IsAtWarAgainstFaction(hero.MapFaction, hero2.MapFaction))
		{
			return $"Attacker Hero Faction: {hero.MapFaction.Name} and Other Hero Faction: {hero2.MapFaction.Name} are not at war, you can use \"campaign.declare_war\" cheat";
		}
		if (ControlPartyAIByCheatsInternal(hero, enable: true, out var resultDescription))
		{
			SetPartyAiAction.GetActionForEngagingParty(hero.PartyBelongedTo, hero2.PartyBelongedTo);
			return resultDescription + "\nParty AI can be enabled again by using \"campaign.control_party_ai_by_cheats\"\nSuccess";
		}
		return resultDescription;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("ai_defend_settlement", "campaign")]
	public static string AIDefendSettlement(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.ai_defend_settlement [HeroName] | [SettlementName]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings) || CheckParameters(strings, 1))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		Hero hero = GetHero(separatedNames[0]);
		Settlement settlement = GetSettlement(separatedNames[1]);
		if (hero == null)
		{
			return "Hero is not found\n" + text;
		}
		if (settlement == null)
		{
			return "Settlement is not found\n" + text;
		}
		if (FactionManager.IsAtWarAgainstFaction(hero.MapFaction, settlement.MapFaction))
		{
			return $"Hero Faction: {hero.MapFaction.Name} and Settlement Faction: {settlement.MapFaction.Name} are at war.";
		}
		if (!settlement.IsUnderRaid && !settlement.IsUnderSiege)
		{
			return "Settlement is not under siege nor raid";
		}
		if (ControlPartyAIByCheatsInternal(hero, enable: true, out var resultDescription))
		{
			SetPartyAiAction.GetActionForDefendingSettlement(hero.PartyBelongedTo, settlement);
			return resultDescription + "\nParty AI can be enabled again by using \"campaign.control_party_ai_by_cheats\"\nSuccess";
		}
		return resultDescription;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("ai_goto_settlement", "campaign")]
	public static string AIGotoSettlement(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.ai_goto_settlement [HeroName] | [SettlementName]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings) || CheckParameters(strings, 1))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 2)
		{
			return text;
		}
		Hero hero = GetHero(separatedNames[0]);
		Settlement settlement = GetSettlement(separatedNames[1]);
		if (hero == null)
		{
			return "Hero is not found\n" + text;
		}
		if (settlement == null)
		{
			return "Settlement is not found\n" + text;
		}
		if (FactionManager.IsAtWarAgainstFaction(hero.MapFaction, settlement.MapFaction))
		{
			return $"Hero Faction: {hero.MapFaction.Name} and Settlement Faction: {settlement.MapFaction.Name} are at war";
		}
		if (ControlPartyAIByCheatsInternal(hero, enable: true, out var resultDescription))
		{
			SetPartyAiAction.GetActionForVisitingSettlement(hero.PartyBelongedTo, settlement);
			return resultDescription + "\nParty AI can be enabled again by using \"campaign.control_party_ai_by_cheats\"\nSuccess";
		}
		return resultDescription;
	}

	public static List<string> GetSeparatedNames(List<string> strings, string separator)
	{
		List<string> list = new List<string>();
		List<int> list2 = new List<int>(strings.Count);
		for (int i = 0; i < strings.Count; i++)
		{
			if (strings[i] == separator)
			{
				list2.Add(i);
			}
		}
		list2.Add(strings.Count);
		int num = 0;
		for (int j = 0; j < list2.Count; j++)
		{
			int num2 = list2[j];
			string item = ConcatenateString(strings.GetRange(num, num2 - num));
			num = num2 + 1;
			list.Add(item);
		}
		return list;
	}

	private static bool ControlPartyAIByCheatsInternal(Hero hero, bool enable, out string resultDescription)
	{
		if (hero == null)
		{
			resultDescription = "Hero is not found";
			return false;
		}
		if (hero == Hero.MainHero)
		{
			resultDescription = "Hero cannot be MainHero";
			return false;
		}
		MobileParty partyBelongedTo = hero.PartyBelongedTo;
		if (partyBelongedTo == null)
		{
			resultDescription = "Hero is not part of a party";
			return false;
		}
		if (partyBelongedTo.Army != null && partyBelongedTo.Army.LeaderParty != partyBelongedTo)
		{
			resultDescription = "Party AI cannot be changed while party is part of an army and not the leader of the army";
			return false;
		}
		partyBelongedTo.Ai.SetDoNotMakeNewDecisions(enable);
		resultDescription = (enable ? $"Party AI of {hero} is controlled by cheats" : $"Party AI of {hero} isn't controlled by cheats");
		return true;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("clear_settlement_defense", "campaign")]
	public static string ClearSettlementDefense(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.clear_settlement_defense [SettlementName]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return text;
		}
		Settlement settlement = GetSettlement(ConcatenateString(strings.GetRange(0, strings.Count)));
		if (settlement == null)
		{
			return "Settlement is not found\n" + text;
		}
		settlement.Militia = 0f;
		MobileParty mobileParty = (settlement.IsFortification ? settlement.Town.GarrisonParty : null);
		if (mobileParty != null)
		{
			DestroyPartyAction.Apply(null, mobileParty);
		}
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_party_prisoners", "campaign")]
	public static string PrintPartyPrisoners(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.print_party_prisoners [PartyName]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return text;
		}
		string text2 = ConcatenateString(strings);
		foreach (MobileParty item in MobileParty.All)
		{
			if (!string.Equals(text2.Replace(" ", ""), item.Name.ToString().Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			string text3 = "";
			foreach (TroopRosterElement item2 in item.PrisonRoster.GetTroopRoster())
			{
				text3 = string.Concat(text3, item2.Character.Name, " count: ", item.PrisonRoster.GetTroopCount(item2.Character), "\n");
			}
			if (string.IsNullOrEmpty(text3))
			{
				return "There is not any prisoners in the party right now.";
			}
			return text3;
		}
		return "Party is not found: " + text2 + "\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_prisoners_xp", "campaign")]
	public static string AddPrisonersXp(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_prisoners_xp [Amount]\".";
		if (!CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		int result = 1;
		if (!int.TryParse(strings[0], out result) || result < 1)
		{
			return "Please enter a positive number\n" + text;
		}
		for (int i = 0; i < MobileParty.MainParty.PrisonRoster.Count; i++)
		{
			MobileParty.MainParty.PrisonRoster.SetElementXp(i, MobileParty.MainParty.PrisonRoster.GetElementXp(i) + result);
			InformationManager.DisplayMessage(new InformationMessage("[DEBUG] " + result + " xp given to " + MobileParty.MainParty.PrisonRoster.GetElementCopyAtIndex(i).Character.Name));
		}
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_settlement_variable", "campaign")]
	public static string SetSettlementVariable(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_settlement_variable [SettlementName/SettlementID] | [VariableName] | [Value]\". Available variables:\nProsperity\nSecurity\nFood\nLoyalty\nMilitia\nHearth";
		if (CheckParameters(strings, 0) || CheckParameters(strings, 1) || CheckParameters(strings, 2) || CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count != 3)
		{
			return text;
		}
		string text2 = separatedNames[1].ToLower();
		string s = separatedNames[2].ToLower();
		string settlementName = separatedNames[0];
		Settlement settlement = Settlement.FindFirst((Settlement x) => string.Compare(x.Name.ToString().Replace(" ", ""), settlementName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) == 0);
		if (settlement == null)
		{
			return "Settlement is not found: " + settlementName + "\n" + text;
		}
		bool flag = false;
		if (settlement.IsVillage)
		{
			if (text2.Equals("hearth") || text2.Equals("militia"))
			{
				flag = true;
			}
		}
		else if (text2.Equals("prosperity") || text2.Equals("militia") || text2.Equals("security") || text2.Equals("loyalty") || text2.Equals("food"))
		{
			flag = true;
		}
		if (!flag)
		{
			return "Settlement don't have variable: " + text2;
		}
		float result = -333f;
		if (!float.TryParse(s, out result))
		{
			return "Please enter a number\n" + text;
		}
		switch (text2.Replace(" ", ""))
		{
		case "prosperity":
			settlement.Town.Prosperity = result;
			break;
		case "militia":
			settlement.Militia = result;
			break;
		case "hearth":
			settlement.Village.Hearth = result;
			break;
		case "security":
			settlement.Town.Security = result;
			break;
		case "loyalty":
			settlement.Town.Loyalty = result;
			break;
		case "food":
			settlement.Town.FoodStocks = result;
			break;
		default:
			return "Invalid variable: " + text2 + "\n" + text;
		}
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_hero_trait", "campaign")]
	public static string SetHeroTrait(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.set_hero_trait [HeroName] | [Trait]  | [Value]\".";
		if (CheckHelp(strings))
		{
			return text;
		}
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count < 3)
		{
			return text;
		}
		if (!int.TryParse(separatedNames[2], out var _))
		{
			return "Please enter a number\n" + text;
		}
		Hero hero = GetHero(separatedNames[0]);
		if (hero != null)
		{
			if (int.TryParse(separatedNames[2], out var result2))
			{
				string text2 = separatedNames[1];
				foreach (TraitObject item in TraitObject.All)
				{
					if (!item.Name.ToString().Replace(" ", "").Equals(text2.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase) && !(item.StringId == text2.Replace(" ", "")))
					{
						continue;
					}
					int traitLevel = hero.GetTraitLevel(item);
					if (result2 >= item.MinValue && result2 <= item.MaxValue)
					{
						hero.SetTraitLevel(item, result2);
						if (hero == Hero.MainHero)
						{
							Campaign.Current.PlayerTraitDeveloper.UpdateTraitXPAccordingToTraitLevels();
							CampaignEventDispatcher.Instance.OnPlayerTraitChanged(item, traitLevel);
						}
						Campaign.Current.PlayerTraitDeveloper.UpdateTraitXPAccordingToTraitLevels();
						CampaignEventDispatcher.Instance.OnPlayerTraitChanged(item, traitLevel);
						return $"{separatedNames[0]} 's {item.Name} trait has been set to {result2}.";
					}
					return $"Number must be between {item.MinValue} and {item.MaxValue}.";
				}
			}
			return "Trait not found.\n" + text;
		}
		return "Hero: " + separatedNames[0] + " not found.\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_hero_traits", "campaign")]
	public static string PrintHeroTraits(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckParameters(strings, 0))
		{
			return "Format is \"campaign.print_hero_traits [HeroName]\".";
		}
		Hero hero = null;
		string text = ConcatenateString(strings);
		hero = GetHero(text);
		if (hero != null)
		{
			string text2 = null;
			{
				foreach (TraitObject item in TraitObject.All)
				{
					text2 = string.Concat(text2, item.Name, " ", hero.GetTraitLevel(item).ToString(), "\n");
				}
				return text2;
			}
		}
		return "Hero: " + text + " not found.\nFormat is \"campaign.print_hero_traits [HeroName]\".";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("remove_militas_from_settlement", "campaign")]
	public static string RemoveMilitiasFromSettlement(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return "Format is \"campaign.remove_militas_from_settlement [SettlementName]\".";
		}
		string concatenated = ConcatenateString(strings);
		Settlement settlement = Settlement.FindFirst((Settlement x) => string.Compare(x.Name.ToString().Replace(" ", ""), concatenated, StringComparison.OrdinalIgnoreCase) == 0);
		if (settlement == null)
		{
			return "Settlement is not found: " + concatenated;
		}
		if (settlement.Party.MapEvent != null)
		{
			return "Settlement, " + concatenated + " is in a MapEvent, try later to remove them";
		}
		List<MobileParty> list = new List<MobileParty>();
		foreach (MobileParty item in MobileParty.All.Where((MobileParty x) => x.IsMilitia && x.CurrentSettlement == settlement))
		{
			if (item.MapEvent != null)
			{
				return "Milita in " + concatenated + " are in a MapEvent, try later to remove them";
			}
			list.Add(item);
		}
		foreach (MobileParty item2 in list)
		{
			item2.RemoveParty();
		}
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("cancel_quest", "campaign")]
	public static string CancelQuestCheat(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.cancel_quest [quest name]\".";
		if (CheckHelp(strings))
		{
			return text;
		}
		string text2 = "";
		for (int i = 0; i < strings.Count; i++)
		{
			text2 += strings[i];
			if (i + 1 != strings.Count)
			{
				text2 += " ";
			}
		}
		if (text2.IsEmpty())
		{
			return text;
		}
		QuestBase questBase = null;
		int num = 0;
		foreach (QuestBase quest in Campaign.Current.QuestManager.Quests)
		{
			if (text2.Replace(" ", "").Equals(quest.Title.ToString().Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
			{
				num++;
				if (num == 1)
				{
					questBase = quest;
				}
			}
		}
		if (questBase == null)
		{
			return "Quest not found.\n" + text;
		}
		if (num > 1)
		{
			return "There are more than one quest with the name: " + text2;
		}
		questBase.CompleteQuestWithCancel(new TextObject("{=!}Quest is canceled by cheat."));
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("kick_companion", "campaign")]
	public static string KickAllCompanionsFromParty(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.kick_companion [CompanionName] or [all](kicks all companions) or [noargument](kicks first companion if any) \".";
		if (CheckHelp(strings))
		{
			return text;
		}
		IEnumerable<TroopRosterElement> enumerable = from h in MobileParty.MainParty.MemberRoster.GetTroopRoster()
			where h.Character != null && h.Character.IsHero && h.Character.HeroObject.IsWanderer
			select h;
		if (enumerable.IsEmpty())
		{
			return "There are no companions in your party.";
		}
		if (strings.IsEmpty())
		{
			RemoveCompanionAction.ApplyByFire(Clan.PlayerClan, enumerable.First().Character.HeroObject);
			return "Success";
		}
		if (strings[0].ToLower() == "all")
		{
			foreach (TroopRosterElement item in enumerable)
			{
				RemoveCompanionAction.ApplyByFire(Clan.PlayerClan, item.Character.HeroObject);
			}
			return "Success";
		}
		foreach (TroopRosterElement item2 in enumerable)
		{
			if (item2.Character.Name.ToString().ToLower().Replace(" ", "")
				.Contains(strings[0].ToLower().Replace(" ", "")))
			{
				RemoveCompanionAction.ApplyByFire(Clan.PlayerClan, item2.Character.HeroObject);
				return "Success";
			}
		}
		return "No companion named: " + strings[0] + " is found.\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_money_to_main_party", "campaign")]
	public static string AddMoneyToMainParty(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_money_to_main_party [Amount]\".";
		if (!CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		if (int.TryParse(strings[0], out var result) && result > 0)
		{
			GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, result);
			return "Main hero gained " + result + " gold.";
		}
		return "Please enter a positive number\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_troops_xp", "campaign")]
	public static string AddTroopsXp(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_troops_xp [Amount]\".";
		if (!CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return text;
		}
		int result = 1;
		if (!int.TryParse(strings[0], out result) || result < 1)
		{
			return "Please enter a positive number\n" + text;
		}
		for (int i = 0; i < MobileParty.MainParty.MemberRoster.Count; i++)
		{
			MobileParty.MainParty.MemberRoster.SetElementXp(i, MobileParty.MainParty.MemberRoster.GetElementXp(i) + result);
			InformationManager.DisplayMessage(new InformationMessage("[DEBUG] " + result + " xp given to " + MobileParty.MainParty.MemberRoster.GetElementCopyAtIndex(i).Character.Name));
		}
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("toggle_information_restrictions", "campaign")]
	public static string ToggleInformationRestrictions(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckHelp(strings))
		{
			return "Format is \"campaign.toggle_information_restrictions\".";
		}
		if (!(Campaign.Current.Models.InformationRestrictionModel is DefaultInformationRestrictionModel defaultInformationRestrictionModel))
		{
			return "DefaultInformationRestrictionModel is missing.";
		}
		defaultInformationRestrictionModel.IsDisabledByCheat = !defaultInformationRestrictionModel.IsDisabledByCheat;
		return "Information restrictions are " + (defaultInformationRestrictionModel.IsDisabledByCheat ? "disabled" : "enabled") + ".";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_campaign_speed_multiplier", "campaign")]
	public static string SetCampaignSpeed(List<string> strings)
	{
		string result = "Format is \"campaign.set_campaign_speed_multiplier  [positive speedUp multiplier]\".";
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return result;
		}
		if (float.TryParse(strings[0], out var result2) && result2 > 0f)
		{
			if (result2 <= 15f)
			{
				Campaign.Current.SpeedUpMultiplier = result2;
				return "Success";
			}
			Campaign.Current.SpeedUpMultiplier = 15f;
			return "Campaign speed is set to " + 15f + ". which is the maximum value for speed up multiplier!";
		}
		return result;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_gameplay_statistics", "campaign")]
	public static string PrintGameplayStatistics(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckHelp(strings))
		{
			return "Format is \"statistics.print_gameplay_statistics\".";
		}
		IStatisticsCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<IStatisticsCampaignBehavior>();
		if (campaignBehavior == null)
		{
			return "Can not find IStatistics Campaign Behavior!";
		}
		string text = "";
		text += "---------------------------GENERAL---------------------------\n";
		text = text + "Played Time in Campaign Time(Days): " + campaignBehavior.GetTotalTimePlayed().ToDays + "\n";
		text = text + "Played Time in Real Time: " + campaignBehavior.GetTotalTimePlayedInSeconds() + "\n";
		text = text + "Number of children born: " + campaignBehavior.GetNumberOfChildrenBorn() + "\n";
		text = text + "Total influence earned: " + campaignBehavior.GetTotalInfluenceEarned() + "\n";
		text = text + "Number of issues solved: " + campaignBehavior.GetNumberOfIssuesSolved() + "\n";
		text = text + "Number of tournaments won: " + campaignBehavior.GetNumberOfTournamentWins() + "\n";
		text = text + "Best tournament rank: " + campaignBehavior.GetHighestTournamentRank() + "\n";
		text = text + "Number of prisoners recruited: " + campaignBehavior.GetNumberOfPrisonersRecruited() + "\n";
		text = text + "Number of troops recruited: " + campaignBehavior.GetNumberOfTroopsRecruited() + "\n";
		text = text + "Number of enemy clans defected: " + campaignBehavior.GetNumberOfClansDefected() + "\n";
		text = text + "Total crime rating gained: " + campaignBehavior.GetTotalCrimeRatingGained() + "\n";
		text += "---------------------------BATTLE---------------------------\n";
		text = text + "Battles Won / Lost: " + campaignBehavior.GetNumberOfBattlesWon() + " / " + campaignBehavior.GetNumberOfBattlesLost() + "\n";
		text = text + "Largest battle won as the leader: " + campaignBehavior.GetLargestBattleWonAsLeader() + "\n";
		text = text + "Largest army formed by the player: " + campaignBehavior.GetLargestArmyFormedByPlayer() + "\n";
		text = text + "Number of enemy clans destroyed: " + campaignBehavior.GetNumberOfEnemyClansDestroyed() + "\n";
		text = text + "Heroes killed in battle: " + campaignBehavior.GetNumberOfHeroesKilledInBattle() + "\n";
		text = text + "Troops killed or knocked out in person: " + campaignBehavior.GetNumberOfTroopsKnockedOrKilledByPlayer() + "\n";
		text = text + "Troops killed or knocked out by player party: " + campaignBehavior.GetNumberOfTroopsKnockedOrKilledAsParty() + "\n";
		text = text + "Number of hero prisoners taken: " + campaignBehavior.GetNumberOfHeroPrisonersTaken() + "\n";
		text = text + "Number of troop prisoners taken: " + campaignBehavior.GetNumberOfTroopPrisonersTaken() + "\n";
		text = text + "Number of captured towns: " + campaignBehavior.GetNumberOfTownsCaptured() + "\n";
		text = text + "Number of captured castles: " + campaignBehavior.GetNumberOfCastlesCaptured() + "\n";
		text = text + "Number of cleared hideouts: " + campaignBehavior.GetNumberOfHideoutsCleared() + "\n";
		text = text + "Number of raided villages: " + campaignBehavior.GetNumberOfVillagesRaided() + "\n";
		text = text + "Number of days spent as prisoner: " + campaignBehavior.GetTimeSpentAsPrisoner().ToDays + "\n";
		text += "---------------------------FINANCES---------------------------\n";
		text = text + "Total denars earned: " + campaignBehavior.GetTotalDenarsEarned() + "\n";
		text = text + "Total denars earned from caravans: " + campaignBehavior.GetDenarsEarnedFromCaravans() + "\n";
		text = text + "Total denars earned from workshops: " + campaignBehavior.GetDenarsEarnedFromWorkshops() + "\n";
		text = text + "Total denars earned from ransoms: " + campaignBehavior.GetDenarsEarnedFromRansoms() + "\n";
		text = text + "Total denars earned from taxes: " + campaignBehavior.GetDenarsEarnedFromTaxes() + "\n";
		text = text + "Total denars earned from tributes: " + campaignBehavior.GetDenarsEarnedFromTributes() + "\n";
		text = text + "Total denars paid in tributes: " + campaignBehavior.GetDenarsPaidAsTributes() + "\n";
		text += "---------------------------CRAFTING---------------------------\n";
		text = text + "Number of weapons crafted: " + campaignBehavior.GetNumberOfWeaponsCrafted() + "\n";
		text = text + "Most expensive weapon crafted: " + campaignBehavior.GetMostExpensiveItemCrafted().Item1 + " - " + campaignBehavior.GetMostExpensiveItemCrafted().Item2 + "\n";
		text = text + "Numbere of crafting parts unlocked: " + campaignBehavior.GetNumberOfCraftingPartsUnlocked() + "\n";
		text = text + "Number of crafting orders completed: " + campaignBehavior.GetNumberOfCraftingOrdersCompleted() + "\n";
		text += "---------------------------COMPANIONS---------------------------\n";
		text = text + "Number of hired companions: " + campaignBehavior.GetNumberOfCompanionsHired() + "\n";
		text = text + "Companion with most issues solved: " + campaignBehavior.GetCompanionWithMostIssuesSolved().name + " - " + campaignBehavior.GetCompanionWithMostIssuesSolved().value + "\n";
		return text + "Companion with most kills: " + campaignBehavior.GetCompanionWithMostKills().name + " - " + campaignBehavior.GetCompanionWithMostKills().value + "\n";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_armies_and_parties_visible", "campaign")]
	public static string SetAllArmiesAndPartiesVisible(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 1) || CheckHelp(strings))
		{
			return "Format is \"campaign.set_armies_and_parties_visible [1/0]\".";
		}
		Campaign.Current.TrueSight = strings[0] == "1";
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_strength_of_lord_parties", "campaign")]
	public static string PrintStrengthOfLordParties(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (MobileParty allLordParty in MobileParty.AllLordParties)
		{
			stringBuilder.AppendLine(string.Concat(allLordParty.Name, " strength: ", allLordParty.Party.TotalStrength));
		}
		stringBuilder.AppendLine("Success");
		return stringBuilder.ToString();
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_strength_of_factions", "campaign")]
	public static string PrintStrengthOfFactions(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Clan item in Clan.All)
		{
			stringBuilder.AppendLine(string.Concat(item.Name, " strength: ", item.TotalStrength));
		}
		stringBuilder.AppendLine("Success");
		return stringBuilder.ToString();
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("print_influence_change_of_clan", "campaign")]
	public static string PrintInfluenceChangeOfClan(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string result = "Format is \"campaign.print_influence_change_of_clan [ClanName]\".";
		if (CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return result;
		}
		string clanName = ConcatenateString(strings);
		StringBuilder stringBuilder = new StringBuilder();
		Clan clan = Campaign.Current.Clans.FirstOrDefault((Clan x) => string.Equals(x.Name.ToString().Replace(" ", ""), clanName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) && !x.IsEliminated);
		if (clan == null)
		{
			return result;
		}
		if (clan != null)
		{
			foreach (var line in Campaign.Current.Models.ClanPoliticsModel.CalculateInfluenceChange(clan, includeDescriptions: true).GetLines())
			{
				stringBuilder.AppendLine(line.name + ": " + line.number);
			}
		}
		stringBuilder.AppendLine("Success");
		return stringBuilder.ToString();
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_supporters_for_main_hero", "campaign")]
	public static string AddSupportersForMainHero(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Usage : campaign.add_supporters_for_main_hero [Number]";
		if (CheckHelp(strings) || !CheckParameters(strings, 1))
		{
			return string.Concat("" + "Usage : campaign.add_supporters_for_main_hero [Number]", "\n");
		}
		string text2 = "";
		if (int.TryParse(strings[0], out var result) && result > 0)
		{
			for (int i = 0; i < result; i++)
			{
				Hero randomElementWithPredicate = Hero.AllAliveHeroes.GetRandomElementWithPredicate((Hero x) => !x.IsChild && x.SupporterOf != Clan.PlayerClan);
				if (randomElementWithPredicate != null)
				{
					randomElementWithPredicate.SupporterOf = Clan.PlayerClan;
					text2 = text2 + "supporter added: " + randomElementWithPredicate.Name.ToString() + "\n";
				}
			}
			return text2 + "\nSuccess";
		}
		return "Please enter a positive number\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("show_hideouts", "campaign")]
	public static string ShowHideouts(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 1) || CheckHelp(strings) || !int.TryParse(strings[0], out var result) || (result != 1 && result != 2))
		{
			return "Format is \"campaign.show_hideouts [1/2]\n 1: Show infested hideouts\n2: Show all hideouts\".";
		}
		foreach (Settlement item in Settlement.All)
		{
			if (item.IsHideout && (result != 1 || item.Hideout.IsInfested))
			{
				Hideout hideout = item.Hideout;
				hideout.IsSpotted = true;
				hideout.Owner.Settlement.IsVisible = true;
			}
		}
		return ((result == 1) ? "Infested" : "All") + " hideouts is visible now.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("hide_hideouts", "campaign")]
	public static string HideHideouts(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		foreach (Settlement item in Settlement.All)
		{
			if (item.IsHideout)
			{
				Hideout hideout = item.Hideout;
				hideout.IsSpotted = false;
				hideout.Owner.Settlement.IsVisible = false;
			}
		}
		return "All hideouts should be invisible now.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("unlock_all_crafting_pieces", "campaign")]
	public static string UnlockCraftingPieces(List<string> strings)
	{
		string ErrorType = "";
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (!CheckParameters(strings, 0) || CheckHelp(strings))
		{
			return "Format is \"campaign.unlock_all_crafting_pieces\".";
		}
		CraftingCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<CraftingCampaignBehavior>();
		if (campaignBehavior == null)
		{
			return "Can not find Crafting Campaign Behavior!";
		}
		Type typeFromHandle = typeof(CraftingCampaignBehavior);
		FieldInfo field = typeFromHandle.GetField("_openedPartsDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
		FieldInfo field2 = typeFromHandle.GetField("_openNewPartXpDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
		Dictionary<CraftingTemplate, List<CraftingPiece>> dictionary = (Dictionary<CraftingTemplate, List<CraftingPiece>>)field.GetValue(campaignBehavior);
		Dictionary<CraftingTemplate, float> dictionary2 = (Dictionary<CraftingTemplate, float>)field2.GetValue(campaignBehavior);
		MethodInfo method = typeFromHandle.GetMethod("OpenPart", BindingFlags.Instance | BindingFlags.NonPublic);
		foreach (CraftingTemplate item in CraftingTemplate.All)
		{
			if (!dictionary.ContainsKey(item))
			{
				dictionary.Add(item, new List<CraftingPiece>());
			}
			if (!dictionary2.ContainsKey(item))
			{
				dictionary2.Add(item, 0f);
			}
			foreach (CraftingPiece piece in item.Pieces)
			{
				object[] parameters = new object[3] { piece, item, false };
				method.Invoke(campaignBehavior, parameters);
			}
		}
		field.SetValue(campaignBehavior, dictionary);
		field2.SetValue(campaignBehavior, dictionary2);
		return "Success";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("rebellion_enabled", "campaign")]
	public static string SetRebellionEnabled(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is campaign.rebellion_enabled [1/0]\".";
		if (CheckHelp(strings) || !CheckParameters(strings, 1))
		{
			return text;
		}
		if (strings[0] == "0" || strings[0] == "1")
		{
			RebellionsCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<RebellionsCampaignBehavior>();
			if (campaignBehavior != null)
			{
				FieldInfo field = typeof(RebellionsCampaignBehavior).GetField("_rebellionEnabled", BindingFlags.Instance | BindingFlags.NonPublic);
				field.SetValue(campaignBehavior, strings[0] == "1");
				return "Rebellion is" + (((bool)field.GetValue(campaignBehavior)) ? " enabled" : " disabled");
			}
			return "Rebellions Campaign behavior not found.";
		}
		return "Wrong input.\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_troops", "campaign")]
	public static string AddTroopsToParty(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		if (CheckParameters(strings, 0))
		{
			return "Write \"campaign.add_troops help\" for help";
		}
		string text = "Usage : \"campaign.add_troops [TroopId] | [Number] | [PartyName]\". Party name is optional.";
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (CheckHelp(strings) || separatedNames.Count < 2)
		{
			string text2 = "";
			text2 += text;
			text2 += "\n";
			text2 += "\n";
			text2 += "Available troops";
			text2 += "\n";
			text2 += "==============================";
			text2 += "\n";
			{
				foreach (CharacterObject objectType in MBObjectManager.Instance.GetObjectTypeList<CharacterObject>())
				{
					if (objectType.Occupation == Occupation.Soldier || objectType.Occupation == Occupation.Gangster)
					{
						text2 = string.Concat(text2, "Id: ", objectType.StringId, " Name: ", objectType.Name, "\n");
					}
				}
				return text2;
			}
		}
		string text3 = separatedNames[0];
		CharacterObject characterObject = MBObjectManager.Instance.GetObject<CharacterObject>(text3);
		if (characterObject == null)
		{
			foreach (CharacterObject objectType2 in MBObjectManager.Instance.GetObjectTypeList<CharacterObject>())
			{
				if ((objectType2.Occupation == Occupation.Soldier || objectType2.Occupation == Occupation.Gangster) && objectType2.StringId == text3.ToLower())
				{
					characterObject = objectType2;
					break;
				}
			}
		}
		if (characterObject == null)
		{
			return "Troop with id \"" + text3 + "\" could not be found.\n" + text;
		}
		if (!int.TryParse(separatedNames[1], out var result) || result < 1)
		{
			return "Please enter a positive number\n" + text;
		}
		MobileParty mobileParty = PartyBase.MainParty.MobileParty;
		if (separatedNames.Count == 3)
		{
			foreach (MobileParty item in MobileParty.All)
			{
				if (string.Equals(separatedNames[2].Replace(" ", ""), item.Name.ToString().Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
				{
					mobileParty = item;
					break;
				}
			}
		}
		if (mobileParty.MapEvent != null)
		{
			return "Party shouldn't be in a map event.";
		}
		typeof(DefaultPartySizeLimitModel).GetField("_addAdditionalPartySizeAsCheat", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, true);
		mobileParty.AddElementToMemberRoster(characterObject, result);
		return string.Concat(mobileParty.Name.ToString(), " gained ", result, " of ", characterObject.Name, ".");
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_random_hero_to_party", "campaign")]
	public static string AddRandomHeroToParty(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_random_hero_to_party [PartyLeaderName]\".";
		if (CheckHelp(strings))
		{
			return text;
		}
		Hero hero = Hero.MainHero;
		string text2 = hero.Name.ToString();
		Hero randomElement = Hero.AllAliveHeroes.GetRandomElement();
		if (!CheckParameters(strings, 0))
		{
			text2 = ConcatenateString(strings.GetRange(0, strings.Count));
			hero = GetHero(text2);
		}
		if (hero != null)
		{
			if (hero.IsPartyLeader)
			{
				for (int i = 0; i < 1000; i++)
				{
					if (randomElement.PartyBelongedTo == null && randomElement.PartyBelongedToAsPrisoner == null)
					{
						break;
					}
					randomElement = Hero.AllAliveHeroes.GetRandomElement();
				}
				if (randomElement.PartyBelongedTo != null || randomElement.PartyBelongedToAsPrisoner != null)
				{
					return "There is not any suitable hero right now.";
				}
				if (hero.Equals(Hero.MainHero))
				{
					typeof(DefaultPartySizeLimitModel).GetField("_addAdditionalPartySizeAsCheat", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, true);
				}
				hero.PartyBelongedTo.AddElementToMemberRoster(randomElement.CharacterObject, 1);
				return $"{randomElement.Name} is added to {hero.Name}'s party.";
			}
			return text2 + " is not a party leader.";
		}
		return text2 + " is not found.\n" + text;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("add_prisoner", "campaign")]
	public static string AddPrisoner(List<string> strings)
	{
		if (!CheckCheatUsage(ref ErrorType))
		{
			return ErrorType;
		}
		string text = "Format is \"campaign.add_prisoner [TroopName] | [PositiveNumber]\".";
		if (CheckHelp(strings))
		{
			return text;
		}
		int result = 1;
		string text2 = "looter";
		CharacterObject characterObject = MBObjectManager.Instance.GetObject<CharacterObject>(text2);
		bool flag = false;
		List<string> separatedNames = GetSeparatedNames(strings, "|");
		if (separatedNames.Count == 1)
		{
			if (!int.TryParse(separatedNames[0], out result))
			{
				result = 1;
				text2 = separatedNames[0].Replace(" ", "");
				characterObject = MBObjectManager.Instance.GetObject<CharacterObject>(text2);
				flag = true;
			}
		}
		else if (separatedNames.Count == 2)
		{
			if (!int.TryParse(separatedNames[1], out result))
			{
				return "Please enter a positive number\n" + text;
			}
			text2 = separatedNames[0].Replace(" ", "");
			characterObject = MBObjectManager.Instance.GetObject<CharacterObject>(text2);
			flag = true;
		}
		if (characterObject == null)
		{
			foreach (CharacterObject objectType in MBObjectManager.Instance.GetObjectTypeList<CharacterObject>())
			{
				if (objectType.Occupation == Occupation.Soldier && string.Equals(objectType.Name.ToString(), text2, StringComparison.OrdinalIgnoreCase))
				{
					characterObject = objectType;
					break;
				}
			}
		}
		if (characterObject != null)
		{
			if (result > 0)
			{
				typeof(DefaultPartySizeLimitModel).GetField("_addAdditionalPartySizeAsCheat", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, true);
				PartyBase.MainParty.AddPrisoner(characterObject, result);
				return string.Concat("Main hero gained ", result, " of ", characterObject.Name, " as prisoner.");
			}
			return "Please enter a positive number\n" + text;
		}
		if (flag)
		{
			return "Troop: " + text2 + " not found.\n" + text;
		}
		return "Wrong input.\n" + text;
	}

	public static Hero GetHero(string heroName)
	{
		foreach (Hero allAliveHero in Hero.AllAliveHeroes)
		{
			if (string.Equals(allAliveHero.Name.ToString().Replace(" ", ""), heroName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
			{
				return allAliveHero;
			}
		}
		foreach (Hero deadOrDisabledHero in Hero.DeadOrDisabledHeroes)
		{
			if (string.Equals(deadOrDisabledHero.Name.ToString(), heroName, StringComparison.OrdinalIgnoreCase))
			{
				return deadOrDisabledHero;
			}
		}
		return null;
	}

	public static ItemObject GetItemObject(string itemObjectName)
	{
		foreach (ItemObject objectType in Campaign.Current.ObjectManager.GetObjectTypeList<ItemObject>())
		{
			if (string.Equals(objectType.Name.ToString().Replace(" ", ""), itemObjectName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
			{
				return objectType;
			}
		}
		return null;
	}

	public static Clan GetClan(string clanName)
	{
		foreach (Clan nonBanditFaction in Clan.NonBanditFactions)
		{
			if (string.Equals(nonBanditFaction.Name.ToString(), clanName, StringComparison.OrdinalIgnoreCase))
			{
				return nonBanditFaction;
			}
		}
		return null;
	}

	public static Hero GetClanLeader(string clanName)
	{
		foreach (Clan nonBanditFaction in Clan.NonBanditFactions)
		{
			if (string.Equals(nonBanditFaction.Name.ToString().Replace(" ", ""), clanName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
			{
				return nonBanditFaction.Leader;
			}
		}
		return null;
	}

	public static Kingdom GetKingdom(string kingdomName)
	{
		foreach (Kingdom item in Kingdom.All)
		{
			if (string.Equals(item.Name.ToString().Replace(" ", ""), kingdomName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
			{
				return item;
			}
		}
		return null;
	}

	public static Settlement GetSettlement(string settlementName)
	{
		foreach (Settlement item in Settlement.All)
		{
			if (string.Equals(item.Name.ToString(), settlementName, StringComparison.OrdinalIgnoreCase) || string.Equals(item.Name.ToString().Replace(" ", ""), settlementName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
			{
				return item;
			}
		}
		return null;
	}

	public static Settlement GetDefaultSettlement()
	{
		Settlement settlement2 = Hero.MainHero.HomeSettlement;
		if (settlement2 == null)
		{
			settlement2 = Kingdom.All.GetRandomElement().Settlements.GetRandomElementWithPredicate((Settlement settlement) => settlement.IsTown);
		}
		return settlement2;
	}

	public static string ConcatenateString(List<string> strings)
	{
		if (strings == null || strings.IsEmpty())
		{
			return string.Empty;
		}
		string text = strings[0];
		if (strings.Count > 1)
		{
			for (int i = 1; i < strings.Count; i++)
			{
				text = text + " " + strings[i];
			}
		}
		return text;
	}

	public static ItemModifier GetItemModifier(string itemModifierName)
	{
		foreach (ItemModifier objectType in Campaign.Current.ObjectManager.GetObjectTypeList<ItemModifier>())
		{
			if (string.Equals(objectType.Name.ToString().Replace(" ", ""), itemModifierName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
			{
				return objectType;
			}
		}
		return null;
	}
}
