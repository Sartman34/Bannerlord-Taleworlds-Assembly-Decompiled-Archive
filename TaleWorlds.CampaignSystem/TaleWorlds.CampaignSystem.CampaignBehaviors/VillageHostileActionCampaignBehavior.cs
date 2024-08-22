using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class VillageHostileActionCampaignBehavior : CampaignBehaviorBase
{
	private enum HostileAction
	{
		RaidTheVillage,
		TakeSupplies,
		GetVolunteers
	}

	private readonly TextObject EnemyNotAttackableTooltip = GameTexts.FindText("str_enemy_not_attackable_tooltip");

	private HostileAction _lastSelectedHostileAction;

	private Dictionary<string, CampaignTime> _villageLastHostileActionTimeDictionary = new Dictionary<string, CampaignTime>();

	private const float IntervalForHostileActionAsDay = 10f;

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_villageLastHostileActionTimeDictionary", ref _villageLastHostileActionTimeDictionary);
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener(this, OnAfterSessionLaunched);
		CampaignEvents.ItemsLooted.AddNonSerializedListener(this, OnItemsLooted);
	}

	private void OnItemsLooted(MobileParty mobileParty, ItemRoster lootedItems)
	{
		SkillLevelingManager.OnRaid(mobileParty, lootedItems);
	}

	private void OnAfterSessionLaunched(CampaignGameStarter campaignGameSystemStarter)
	{
		AddGameMenus(campaignGameSystemStarter);
	}

	public void AddGameMenus(CampaignGameStarter campaignGameSystemStarter)
	{
		campaignGameSystemStarter.AddGameMenuOption("village", "hostile_action", "{=GM3tAYMr}Take a hostile action", game_menu_village_hostile_action_on_condition, game_menu_village_hostile_action_on_consequence, isLeave: false, 2);
		campaignGameSystemStarter.AddGameMenu("village_hostile_action", "{=YVNZaVCA}What action do you have in mind?", game_menu_village_hostile_menu_on_init, GameOverlays.MenuOverlayType.SettlementWithBoth);
		campaignGameSystemStarter.AddGameMenuOption("village_hostile_action", "raid_village", "{=CTi0ml5F}Raid the village", game_menu_village_hostile_action_raid_village_on_condition, game_menu_village_hostile_action_raid_village_on_consequence);
		campaignGameSystemStarter.AddGameMenuOption("village_hostile_action", "force_peasants_to_give_volunteers", "{=RL8z99Dt}Force notables to give you recruits", game_menu_village_hostile_action_force_volunteers_condition, game_menu_village_hostile_action_force_volunteers_on_consequence);
		campaignGameSystemStarter.AddGameMenuOption("village_hostile_action", "force_peasants_to_give_supplies", "{=eAzwpqE1}Force peasants to give you goods", game_menu_village_hostile_action_take_food_on_condition, game_menu_village_hostile_action_take_food_on_consequence);
		campaignGameSystemStarter.AddGameMenuOption("village_hostile_action", "forget_it", "{=sP9ohQTs}Forget it", back_on_condition, game_menu_village_hostile_action_forget_it_on_consequence, isLeave: true);
		campaignGameSystemStarter.AddWaitGameMenu("raiding_village", "{=hWwr3mrC}You are raiding {VILLAGE_NAME}.", hostile_action_village_on_init, wait_menu_start_raiding_on_condition, wait_menu_end_raiding_on_consequence, wait_menu_raiding_village_on_tick, GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);
		campaignGameSystemStarter.AddGameMenuOption("raiding_village", "raiding_village_end", "{=M7CcfbIx}End Raiding", wait_menu_end_raiding_on_condition, wait_menu_end_raiding_on_consequence, isLeave: true);
		campaignGameSystemStarter.AddGameMenuOption("raiding_village", "leave_army", "{=hSdJ0UUv}Leave Army", wait_menu_end_raiding_at_army_by_leaving_on_condition, wait_menu_end_raiding_at_army_by_leaving_on_consequence, isLeave: true);
		campaignGameSystemStarter.AddGameMenuOption("raiding_village", "abandon_army", "{=0vnegjxf}Abandon Army", wait_menu_end_raiding_at_army_by_abandoning_on_condition, wait_menu_end_raiding_at_army_by_abandoning_on_consequence, isLeave: true);
		campaignGameSystemStarter.AddGameMenu("raid_village_no_resist_warn_player", "{=lOwjyUi5}Raiding this village will cause a war with {KINGDOM}.", game_menu_warn_player_on_init);
		campaignGameSystemStarter.AddGameMenuOption("raid_village_no_resist_warn_player", "raid_village_warn_continue", "{=DM6luo3c}Continue", game_menu_village_hostile_action_raid_village_warn_continue_on_condition, game_menu_village_hostile_action_raid_village_warn_continue_on_consequence);
		campaignGameSystemStarter.AddGameMenuOption("raid_village_no_resist_warn_player", "raid_village_warn_leave", "{=sP9ohQTs}Forget it", back_on_condition, game_menu_village_hostile_action_raid_village_warn_leave_on_consequence, isLeave: true);
		campaignGameSystemStarter.AddGameMenu("force_supplies_village", "{=EqFbNha8}The villagers grudgingly bring out what they have for you.", hostile_action_village_on_init);
		campaignGameSystemStarter.AddGameMenuOption("force_supplies_village", "force_supplies_village_continue", "{=DM6luo3c}Continue", continue_on_condition, village_force_supplies_ended_successfully_on_consequence, isLeave: true);
		campaignGameSystemStarter.AddGameMenu("force_volunteers_village", "{=BqkD4YWr}You manage to round up some men from the village who look like they might make decent recruits.", hostile_action_village_on_init);
		campaignGameSystemStarter.AddGameMenuOption("force_volunteers_village", "force_supplies_village_continue", "{=DM6luo3c}Continue", continue_on_condition, village_force_volunteers_ended_successfully_on_consequence, isLeave: true);
		campaignGameSystemStarter.AddGameMenu("village_looted", "{=NxcXfUxu}The village has been looted. A handful of souls scatter as you pass through the burnt-out houses.", null);
		campaignGameSystemStarter.AddGameMenuOption("village_looted", "leave", "{=2YYRyrOO}Leave...", back_on_condition, game_menu_settlement_leave_on_consequence, isLeave: true);
		campaignGameSystemStarter.AddGameMenu("village_player_raid_ended", "{=m1rzHfxI}{VILLAGE_ENCOUNTER_RESULT}", village_player_raid_ended_on_init);
		campaignGameSystemStarter.AddGameMenuOption("village_player_raid_ended", "continue", "{=DM6luo3c}Continue", continue_on_condition, village_player_raid_ended_on_consequence, isLeave: true);
		campaignGameSystemStarter.AddGameMenu("village_raid_ended_leaded_by_someone_else", "{=m1rzHfxI}{VILLAGE_ENCOUNTER_RESULT}", village_raid_ended_leaded_by_someone_else_on_init);
		campaignGameSystemStarter.AddGameMenuOption("village_raid_ended_leaded_by_someone_else", "continue", "{=DM6luo3c}Continue", continue_on_condition, village_raid_ended_leaded_by_someone_else_on_consequence, isLeave: true);
	}

	private static bool wait_menu_end_raiding_on_condition(MenuCallbackArgs args)
	{
		if (MobileParty.MainParty.Army == null || MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty)
		{
			args.optionLeaveType = GameMenuOption.LeaveType.Leave;
			return true;
		}
		return false;
	}

	private static bool wait_menu_end_raiding_at_army_by_leaving_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		if (MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty)
		{
			return MobileParty.MainParty.MapEvent == null;
		}
		return false;
	}

	private void village_force_supplies_ended_successfully_on_consequence(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		GameMenu.SwitchToMenu("village");
		ItemRoster itemRoster = new ItemRoster();
		int num = TaleWorlds.Library.MathF.Max((int)(Settlement.CurrentSettlement.Village.Hearth * 0.15f), 20);
		GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, num * Campaign.Current.Models.RaidModel.GoldRewardForEachLostHearth);
		for (int i = 0; i < Settlement.CurrentSettlement.Village.VillageType.Productions.Count; i++)
		{
			(ItemObject, float) tuple = Settlement.CurrentSettlement.Village.VillageType.Productions[i];
			ItemObject item = tuple.Item1;
			int num2 = (int)(tuple.Item2 / 60f * (float)num);
			if (num2 > 0)
			{
				itemRoster.AddToCounts(item, num2);
			}
		}
		if (!_villageLastHostileActionTimeDictionary.ContainsKey(Settlement.CurrentSettlement.StringId))
		{
			_villageLastHostileActionTimeDictionary.Add(Settlement.CurrentSettlement.StringId, CampaignTime.Now);
		}
		else
		{
			_villageLastHostileActionTimeDictionary[Settlement.CurrentSettlement.StringId] = CampaignTime.Now;
		}
		Settlement.CurrentSettlement.SettlementHitPoints -= Settlement.CurrentSettlement.SettlementHitPoints * 0.8f;
		InventoryManager.OpenScreenAsLoot(new Dictionary<PartyBase, ItemRoster> { 
		{
			PartyBase.MainParty,
			itemRoster
		} });
		bool attacked = MapEvent.PlayerMapEvent == null;
		SkillLevelingManager.OnForceSupplies(MobileParty.MainParty, itemRoster, attacked);
		PlayerEncounter.Current.ForceSupplies = false;
		PlayerEncounter.Current.FinalizeBattle();
	}

	private void village_force_volunteers_ended_successfully_on_consequence(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		GameMenu.SwitchToMenu("village");
		TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
		int num = (int)Math.Ceiling(Settlement.CurrentSettlement.Village.Hearth / 30f);
		if (MobileParty.MainParty.HasPerk(DefaultPerks.Roguery.InBestLight))
		{
			num += Settlement.CurrentSettlement.Notables.Count;
		}
		troopRoster.AddToCounts(Settlement.CurrentSettlement.Culture.BasicTroop, num);
		if (!_villageLastHostileActionTimeDictionary.ContainsKey(Settlement.CurrentSettlement.StringId))
		{
			_villageLastHostileActionTimeDictionary.Add(Settlement.CurrentSettlement.StringId, CampaignTime.Now);
		}
		else
		{
			_villageLastHostileActionTimeDictionary[Settlement.CurrentSettlement.StringId] = CampaignTime.Now;
		}
		Settlement.CurrentSettlement.SettlementHitPoints -= Settlement.CurrentSettlement.SettlementHitPoints * 0.8f;
		Settlement.CurrentSettlement.Village.Hearth -= num / 2;
		PartyScreenManager.OpenScreenAsLoot(troopRoster, TroopRoster.CreateDummyTroopRoster(), MobileParty.MainParty.CurrentSettlement.Name, troopRoster.TotalManCount);
		PlayerEncounter.Current.ForceVolunteers = false;
		SkillLevelingManager.OnForceVolunteers(MobileParty.MainParty, Settlement.CurrentSettlement.Party);
		PlayerEncounter.Current.FinalizeBattle();
	}

	private static bool game_menu_village_hostile_action_raid_village_warn_continue_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Raid;
		return true;
	}

	private static bool wait_menu_end_raiding_at_army_by_abandoning_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		if (MobileParty.MainParty.Army == null || MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty || MobileParty.MainParty.MapEvent == null)
		{
			return false;
		}
		args.Tooltip = GameTexts.FindText("str_abandon_army");
		args.Tooltip.SetTextVariable("INFLUENCE_COST", Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfAbandoningArmy());
		return true;
	}

	private static void wait_menu_end_raiding_at_army_by_leaving_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.Current.ForceRaid = false;
		PlayerEncounter.Finish();
		MobileParty.MainParty.Army = null;
	}

	private static void wait_menu_end_raiding_at_army_by_abandoning_on_consequence(MenuCallbackArgs args)
	{
		Clan.PlayerClan.Influence -= Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfAbandoningArmy();
		PlayerEncounter.Current.ForceRaid = false;
		PlayerEncounter.Finish();
		MobileParty.MainParty.Army = null;
	}

	private static void village_player_raid_ended_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.ExitToLast();
	}

	private static void village_raid_ended_leaded_by_someone_else_on_init(MenuCallbackArgs args)
	{
		if ((MobileParty.MainParty.Army == null && MobileParty.MainParty.ShortTermTargetParty?.Ai.AiBehaviorPartyBase != null && MobileParty.MainParty.ShortTermTargetParty.Ai.AiBehaviorPartyBase.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction)) || (MobileParty.MainParty.Ai.AiBehaviorPartyBase != null && MobileParty.MainParty.Ai.AiBehaviorPartyBase.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction)))
		{
			if ((MobileParty.MainParty.ShortTermTargetParty?.Ai.AiBehaviorPartyBase != null) ? MobileParty.MainParty.ShortTermTargetParty.Ai.AiBehaviorPartyBase.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction) : MobileParty.MainParty.Ai.AiBehaviorPartyBase.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
			{
				MBTextManager.SetTextVariable("VILLAGE_ENCOUNTER_RESULT", new TextObject("{=04tBEafz}Village is successfully raided by your help."));
			}
			else
			{
				MBTextManager.SetTextVariable("VILLAGE_ENCOUNTER_RESULT", new TextObject("{=2Ixb5OKD}Village is successfully saved by your help."));
			}
		}
		else if (MobileParty.MainParty.Army != null && (Settlement)MobileParty.MainParty.Army.AiBehaviorObject != null && ((Settlement)MobileParty.MainParty.Army.AiBehaviorObject).MapFaction != null)
		{
			if (((Settlement)MobileParty.MainParty.Army.AiBehaviorObject).MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
			{
				MBTextManager.SetTextVariable("VILLAGE_ENCOUNTER_RESULT", new TextObject("{=iyJaisRb}Village is successfully raided by the army you are following."));
			}
			else
			{
				MBTextManager.SetTextVariable("VILLAGE_ENCOUNTER_RESULT", new TextObject("{=JNGUIIQ1}Village is saved by the army you are following."));
			}
		}
		else
		{
			MBTextManager.SetTextVariable("VILLAGE_ENCOUNTER_RESULT", new TextObject("{=3OW1QQNx}The raid was ended by the battle outside of the village."));
		}
	}

	private static void village_player_raid_ended_on_init(MenuCallbackArgs args)
	{
		if (MobileParty.MainParty.LastVisitedSettlement != null && MobileParty.MainParty.LastVisitedSettlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
		{
			MBTextManager.SetTextVariable("VILLAGE_ENCOUNTER_RESULT", "{=4YuFTXxC}You successfully raided the village.");
		}
		else
		{
			MBTextManager.SetTextVariable("VILLAGE_ENCOUNTER_RESULT", "{=aih1Y62W}You have saved the village.");
		}
	}

	private static void village_raid_ended_leaded_by_someone_else_on_consequence(MenuCallbackArgs args)
	{
		if (MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty)
		{
			GameMenu.SwitchToMenu("army_wait");
		}
		else
		{
			GameMenu.ExitToLast();
		}
	}

	private static void game_menu_warn_player_on_init(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		MBTextManager.SetTextVariable("KINGDOM", currentSettlement.MapFaction.IsKingdomFaction ? ((Kingdom)currentSettlement.MapFaction).EncyclopediaTitle : currentSettlement.MapFaction.Name);
	}

	private static void game_menu_village_hostile_menu_on_init(MenuCallbackArgs args)
	{
		PlayerEncounter.LeaveEncounter = false;
		if (Campaign.Current.GameMenuManager.NextLocation != null)
		{
			PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(Campaign.Current.GameMenuManager.NextLocation, Campaign.Current.GameMenuManager.PreviousLocation);
			Campaign.Current.GameMenuManager.NextLocation = null;
			Campaign.Current.GameMenuManager.PreviousLocation = null;
		}
		else if (Settlement.CurrentSettlement.SettlementHitPoints <= 0f)
		{
			GameMenu.ActivateGameMenu("RaidCompleted");
		}
	}

	private static bool game_menu_village_hostile_action_on_condition(MenuCallbackArgs args)
	{
		Village village = Settlement.CurrentSettlement.Village;
		args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
		if ((MobileParty.MainParty.Army == null || MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty) && village != null)
		{
			if (Hero.MainHero.MapFaction != village.Owner.MapFaction)
			{
				return village.VillageState == Village.VillageStates.Normal;
			}
			return false;
		}
		return false;
	}

	private static void game_menu_village_hostile_action_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("village_hostile_action");
	}

	private bool game_menu_village_hostile_action_take_food_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveGoods;
		CheckVillageAttackableHonorably(args);
		if (_villageLastHostileActionTimeDictionary.ContainsKey(Settlement.CurrentSettlement.StringId))
		{
			if (_villageLastHostileActionTimeDictionary[Settlement.CurrentSettlement.StringId].ElapsedDaysUntilNow <= 10f)
			{
				args.IsEnabled = false;
				args.Tooltip = new TextObject("{=mvhyI8Hb}You have already done hostile action in this village recently.");
			}
			else
			{
				_villageLastHostileActionTimeDictionary.Remove(Settlement.CurrentSettlement.StringId);
			}
		}
		return true;
	}

	private void game_menu_village_hostile_action_forget_it_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("village");
	}

	private void game_menu_village_hostile_action_take_food_on_consequence(MenuCallbackArgs args)
	{
		_lastSelectedHostileAction = HostileAction.TakeSupplies;
		PlayerEncounter.Current.ForceSupplies = true;
		GameMenu.SwitchToMenu("encounter");
	}

	private bool game_menu_village_hostile_action_force_volunteers_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveTroops;
		CheckVillageAttackableHonorably(args);
		if (_villageLastHostileActionTimeDictionary.TryGetValue(Settlement.CurrentSettlement.StringId, out var value) && value.ElapsedDaysUntilNow <= 10f)
		{
			args.IsEnabled = false;
			args.Tooltip = new TextObject("{=mvhyI8Hb}You have already done hostile action in this village recently.");
		}
		else if (_villageLastHostileActionTimeDictionary.ContainsKey(Settlement.CurrentSettlement.StringId))
		{
			_villageLastHostileActionTimeDictionary.Remove(Settlement.CurrentSettlement.StringId);
		}
		return true;
	}

	private void game_menu_village_hostile_action_force_volunteers_on_consequence(MenuCallbackArgs args)
	{
		_lastSelectedHostileAction = HostileAction.GetVolunteers;
		PlayerEncounter.Current.ForceVolunteers = true;
		GameMenu.SwitchToMenu("encounter");
	}

	private bool game_menu_village_hostile_action_raid_village_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
		CheckVillageAttackableHonorably(args);
		return !FactionManager.IsAlliedWithFaction(Hero.MainHero.MapFaction, Settlement.CurrentSettlement.MapFaction);
	}

	private void game_menu_village_hostile_action_raid_village_warn_continue_on_consequence(MenuCallbackArgs args)
	{
		_lastSelectedHostileAction = HostileAction.RaidTheVillage;
		PlayerEncounter.Current.ForceRaid = true;
		GameMenu.SwitchToMenu("encounter");
	}

	private void game_menu_village_hostile_action_raid_village_warn_leave_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("village_hostile_action");
		PlayerEncounter.Finish();
	}

	private void game_menu_village_hostile_action_raid_village_on_consequence(MenuCallbackArgs args)
	{
		if (!FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, Settlement.CurrentSettlement.MapFaction))
		{
			GameMenu.SwitchToMenu("raid_village_no_resist_warn_player");
			return;
		}
		_lastSelectedHostileAction = HostileAction.RaidTheVillage;
		PlayerEncounter.Current.ForceRaid = true;
		GameMenu.SwitchToMenu("encounter");
	}

	private void game_menu_villagers_resist_attack_resistance_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("encounter");
	}

	private void CheckVillageAttackableHonorably(MenuCallbackArgs args)
	{
		IFaction faction = MobileParty.MainParty.CurrentSettlement?.MapFaction;
		CheckFactionAttackableHonorably(args, faction);
	}

	private void CheckFactionAttackableHonorably(MenuCallbackArgs args, IFaction faction)
	{
		if (faction.NotAttackableByPlayerUntilTime.IsFuture)
		{
			args.IsEnabled = false;
			args.Tooltip = EnemyNotAttackableTooltip;
		}
	}

	private bool game_menu_no_resist_plunder_village_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Raid;
		if (!IsThereAnyDefence())
		{
			return !FactionManager.IsAlliedWithFaction(Hero.MainHero.MapFaction, Settlement.CurrentSettlement.MapFaction);
		}
		return false;
	}

	private void game_menu_villagers_resist_on_init(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		_ = currentSettlement.Village;
		if (PlayerEncounter.Battle != null)
		{
			PlayerEncounter.Update();
		}
		else if (currentSettlement.SettlementHitPoints <= 0f)
		{
			GameMenu.SwitchToMenu("village_looted");
			return;
		}
		if (!game_menu_villagers_resist_attack_resistance_on_condition(args))
		{
			if (_lastSelectedHostileAction == HostileAction.TakeSupplies)
			{
				GameMenu.SwitchToMenu("village_take_food_confirm");
			}
			else if (_lastSelectedHostileAction == HostileAction.GetVolunteers)
			{
				GameMenu.SwitchToMenu("village_press_into_service_confirm");
			}
		}
		MBTextManager.SetTextVariable("STATE", GameTexts.FindText(IsThereAnyDefence() ? "str_raid_resist" : "str_village_raid_villagers_are_nonresistant"));
	}

	private static void game_menu_village_start_attack_on_init(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (PlayerEncounter.Battle != null)
		{
			PlayerEncounter.Update();
		}
		else if (currentSettlement.SettlementHitPoints <= 0f)
		{
			GameMenu.SwitchToMenu("village_looted");
		}
		float lastDemandSatisfiedTime = currentSettlement.Village.LastDemandSatisfiedTime;
		if (lastDemandSatisfiedTime > 0f && (Campaign.CurrentTime - lastDemandSatisfiedTime) / 24f < 7f)
		{
			GameTexts.SetVariable("STATE", GameTexts.FindText("str_villiger_recently_satisfied_demands"));
		}
		else
		{
			GameTexts.SetVariable("STATE", GameTexts.FindText("str_villigers_grab_their_tools"));
		}
	}

	private static bool game_menu_menu_village_take_food_success_take_supplies_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
		return true;
	}

	private bool game_menu_villagers_resist_attack_resistance_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Mission;
		_ = Settlement.CurrentSettlement;
		return IsThereAnyDefence();
	}

	private bool IsThereAnyDefence()
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (currentSettlement != null)
		{
			foreach (MobileParty party in currentSettlement.Parties)
			{
				if (!party.IsMainParty && party.MapFaction == currentSettlement.MapFaction && party.Party.NumberOfHealthyMembers > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void game_menu_menu_village_take_food_success_let_them_keep_it_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("village");
	}

	public static bool game_menu_menu_village_take_food_success_let_them_keep_it_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
		return true;
	}

	public static void hostile_action_village_on_init(MenuCallbackArgs args)
	{
		MBTextManager.SetTextVariable("VILLAGE_NAME", PlayerEncounter.EncounterSettlement.Name);
	}

	public static void wait_menu_raiding_village_on_tick(MenuCallbackArgs args, CampaignTime dt)
	{
		args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(1f - PlayerEncounter.Battle.MapEventSettlement.SettlementHitPoints);
	}

	public static bool wait_menu_start_raiding_on_condition(MenuCallbackArgs args)
	{
		MBTextManager.SetTextVariable("SETTLEMENT_NAME", PlayerEncounter.Battle.MapEventSettlement.Name);
		return true;
	}

	public static void wait_menu_end_raiding_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.Current.ForceRaid = false;
		PlayerEncounter.Finish();
	}

	private static void game_menu_settlement_leave_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.LeaveSettlement();
		PlayerEncounter.Finish();
		Campaign.Current.SaveHandler.SignalAutoSave();
	}

	[GameMenuInitializationHandler("village_player_raid_ended")]
	public static void game_menu_village_raid_ended_menu_sound_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("wait_raiding_village");
		if (MobileParty.MainParty.LastVisitedSettlement != null && MobileParty.MainParty.LastVisitedSettlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
		{
			args.MenuContext.SetAmbientSound("event:/map/ambient/node/settlements/2d/village_raided");
		}
	}

	[GameMenuInitializationHandler("village_looted")]
	[GameMenuInitializationHandler("village_raid_ended_leaded_by_someone_else")]
	[GameMenuInitializationHandler("raiding_village")]
	private static void game_menu_ui_village_hostile_raid_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("wait_raiding_village");
	}

	[GameMenuInitializationHandler("village_hostile_action")]
	[GameMenuInitializationHandler("force_volunteers_village")]
	[GameMenuInitializationHandler("force_supplies_village")]
	[GameMenuInitializationHandler("raid_village_no_resist_warn_player")]
	[GameMenuInitializationHandler("raid_village_resisted")]
	[GameMenuInitializationHandler("village_loot_no_resist")]
	[GameMenuInitializationHandler("village_take_food_confirm")]
	[GameMenuInitializationHandler("village_press_into_service_confirm")]
	[GameMenuInitializationHandler("menu_press_into_service_success")]
	[GameMenuInitializationHandler("menu_village_take_food_success")]
	public static void game_menu_village_menu_on_init(MenuCallbackArgs args)
	{
		Village village = Settlement.CurrentSettlement.Village;
		args.MenuContext.SetBackgroundMeshName(village.WaitMeshName);
	}

	private static bool continue_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private static bool back_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		return true;
	}
}
