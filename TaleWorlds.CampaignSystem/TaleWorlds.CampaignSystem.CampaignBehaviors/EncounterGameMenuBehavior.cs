using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class EncounterGameMenuBehavior : CampaignBehaviorBase
{
	private static readonly TextObject EnemyNotAttackableTooltip = GameTexts.FindText("str_enemy_not_attackable_tooltip");

	private int _intEncounterVariable;

	private TroopRoster _breakInOutCasualties;

	private int _breakInOutArmyCasualties;

	private SettlementAccessModel.AccessDetails _accessDetails;

	private TroopRoster _getAwayCasualties;

	private ItemRoster _getAwayLostBaggage;

	private bool _playerIsAlreadyInCastle;

	private const float SmugglingCrimeRate = 10f;

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_breakInOutCasualties", ref _breakInOutCasualties);
		dataStore.SyncData("_intEncounterVariable", ref _intEncounterVariable);
		dataStore.SyncData("_breakInOutArmyCasualties", ref _breakInOutArmyCasualties);
		dataStore.SyncData("_getAwayCasualties", ref _getAwayCasualties);
		dataStore.SyncData("_getAwayLostBaggage", ref _getAwayLostBaggage);
		dataStore.SyncData("_playerIsAlreadyInCastle", ref _playerIsAlreadyInCastle);
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
		CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
		CampaignEvents.MapEventStarted.AddNonSerializedListener(this, OnMapEventStarted);
		CampaignEvents.OnSiegeEventStartedEvent.AddNonSerializedListener(this, OnSiegeEventStarted);
	}

	private void OnSiegeEventStarted(SiegeEvent siegeEvent)
	{
		IFaction mapFaction = siegeEvent.BesiegerCamp.LeaderParty.MapFaction;
		if (siegeEvent.IsPlayerSiegeEvent && mapFaction != null && mapFaction.NotAttackableByPlayerUntilTime.IsFuture)
		{
			mapFaction.NotAttackableByPlayerUntilTime = CampaignTime.Zero;
		}
	}

	private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
	{
		if (mapEvent.IsPlayerMapEvent && attackerParty.MapFaction != null && attackerParty.MapFaction.NotAttackableByPlayerUntilTime.IsFuture)
		{
			attackerParty.MapFaction.NotAttackableByPlayerUntilTime = CampaignTime.Zero;
		}
	}

	private void OnSettlementLeft(MobileParty party, Settlement settlement)
	{
		if (party == MobileParty.MainParty)
		{
			_playerIsAlreadyInCastle = false;
		}
	}

	private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		InitializeAccessDetails();
		AddGameMenus(campaignGameStarter);
	}

	private void InitializeAccessDetails()
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (currentSettlement != null && (currentSettlement.IsFortification || currentSettlement.IsVillage))
		{
			Campaign.Current.Models.SettlementAccessModel.CanMainHeroEnterSettlement(Settlement.CurrentSettlement, out _accessDetails);
		}
	}

	private void AddGameMenus(CampaignGameStarter gameSystemInitializer)
	{
		gameSystemInitializer.AddGameMenu("taken_prisoner", "{=ezClQMBj}Your enemies take you as a prisoner.", game_menu_taken_prisoner_on_init);
		gameSystemInitializer.AddGameMenuOption("taken_prisoner", "taken_prisoner_continue", "{=WVkc4UgX}Continue.", game_menu_taken_prisoner_continue_on_condition, game_menu_taken_prisoner_continue_on_consequence);
		gameSystemInitializer.AddGameMenu("defeated_and_taken_prisoner", "{=ezClQMBj}Your enemies take you as a prisoner.", game_menu_taken_prisoner_on_init);
		gameSystemInitializer.AddGameMenuOption("defeated_and_taken_prisoner", "taken_prisoner_continue", "{=WVkc4UgX}Continue.", game_menu_taken_prisoner_continue_on_condition, game_menu_taken_prisoner_continue_on_consequence);
		gameSystemInitializer.AddGameMenu("encounter_meeting", "{=!}.", game_menu_encounter_meeting_on_init);
		gameSystemInitializer.AddGameMenu("join_encounter", "{=jKWJpIES}{JOIN_ENCOUNTER_TEXT}. You decide to...", game_menu_join_encounter_on_init);
		gameSystemInitializer.AddGameMenuOption("join_encounter", "join_encounter_help_attackers", "{=h3yEHb4U}Help {ATTACKER}.", game_menu_join_encounter_help_attackers_on_condition, game_menu_join_encounter_help_attackers_on_consequence);
		gameSystemInitializer.AddGameMenuOption("join_encounter", "join_encounter_help_defenders", "{=FwIgakj8}Help {DEFENDER}.", game_menu_join_encounter_help_defenders_on_condition, game_menu_join_encounter_help_defenders_on_consequence);
		gameSystemInitializer.AddGameMenuOption("join_encounter", "join_encounter_abandon", "{=Nr49hlfC}Abandon army.", game_menu_join_encounter_abandon_army_on_condition, game_menu_encounter_abandon_on_consequence);
		gameSystemInitializer.AddGameMenuOption("join_encounter", "join_encounter_leave", "{=!}{LEAVE_TEXT}", game_menu_join_encounter_leave_no_army_on_condition, delegate
		{
			if (MobileParty.MainParty.SiegeEvent != null && MobileParty.MainParty.SiegeEvent.BesiegerCamp != null && MobileParty.MainParty.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(PartyBase.MainParty))
			{
				MobileParty.MainParty.BesiegerCamp = null;
			}
			PlayerEncounter.Finish();
		}, isLeave: true);
		gameSystemInitializer.AddGameMenu("join_sally_out", "{=CcNVobQU}Garrison of the settlement you are in decided to sally out. You decide to...", game_menu_join_sally_out_on_init, GameOverlays.MenuOverlayType.Encounter);
		gameSystemInitializer.AddGameMenuOption("join_sally_out", "join_siege_event", "{=IaA8sbY2}Join the sally out", game_menu_join_sally_out_event_on_condition, game_menu_join_sally_out_on_consequence);
		gameSystemInitializer.AddGameMenuOption("join_sally_out", "join_siege_event_break_in", "{=z1RHDsOG}Stay in settlement", game_menu_stay_in_settlement_on_condition, game_menu_stay_in_settlement_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("join_siege_event", "{=xNyKVMHx}{JOIN_SIEGE_TEXT} You decide to...", game_menu_join_siege_event_on_init);
		gameSystemInitializer.AddGameMenuOption("join_siege_event", "join_siege_event", "{=ZVsJf5Ff}Join the continuing siege.", game_menu_join_siege_event_on_condition, game_menu_join_siege_event_on_consequence);
		gameSystemInitializer.AddGameMenuOption("join_siege_event", "attack_besiegers", "{=CVg3P07C}Assault the siege camp.", attack_besieger_side_on_condition, game_menu_join_encounter_help_defenders_on_consequence);
		gameSystemInitializer.AddGameMenuOption("join_siege_event", "join_siege_event_break_in", "{=XAvwP3Ce}Break in to help the defenders", break_in_to_help_defender_side_on_condition, game_menu_join_siege_event_on_defender_side_on_consequence);
		gameSystemInitializer.AddGameMenuOption("join_siege_event", "join_encounter_leave", "{=ebUwP3Q3}Don't get involved.", game_menu_join_encounter_leave_on_condition, delegate
		{
			PlayerEncounter.Finish();
			if (Hero.MainHero.PartyBelongedTo != null && Hero.MainHero.PartyBelongedTo.Army != null && Hero.MainHero.PartyBelongedTo.Army.LeaderParty != MobileParty.MainParty)
			{
				Hero.MainHero.PartyBelongedTo.Army = null;
				MobileParty.MainParty.Ai.SetMoveModeHold();
			}
		}, isLeave: true);
		gameSystemInitializer.AddGameMenu("siege_attacker_left", "{=LR6Y57Rq}Attackers abandoned the siege.", null);
		gameSystemInitializer.AddGameMenuOption("siege_attacker_left", "siege_attacker_left_return_to_settlement", "{=j7bZRFxc}Return to {SETTLEMENT}.", game_menu_siege_attacker_left_return_to_settlement_on_condition, game_menu_siege_attacker_left_return_to_settlement_on_consequence);
		gameSystemInitializer.AddGameMenuOption("siege_attacker_left", "siege_attacker_left_leave", "{=mfAP8Wlq}Leave settlement.", game_menu_siege_attacker_left_leave_on_condition, delegate
		{
			PlayerEncounter.Finish();
		}, isLeave: true);
		gameSystemInitializer.AddGameMenu("siege_attacker_defeated", "{=njbpMLdJ}Attackers have been defeated.", null);
		gameSystemInitializer.AddGameMenuOption("siege_attacker_defeated", "siege_attacker_defeated_return_to_settlement", "{=j7bZRFxc}Return to {SETTLEMENT}.", game_menu_siege_attacker_left_return_to_settlement_on_condition, game_menu_siege_attacker_left_return_to_settlement_on_consequence);
		gameSystemInitializer.AddGameMenuOption("siege_attacker_defeated", "siege_attacker_defeated_leave", "{=mfAP8Wlq}Leave settlement.", game_menu_siege_attacker_defeated_leave_on_condition, delegate
		{
			PlayerEncounter.Finish();
		}, isLeave: true);
		gameSystemInitializer.AddGameMenu("encounter", "{=!}{ENCOUNTER_TEXT}", game_menu_encounter_on_init, GameOverlays.MenuOverlayType.Encounter);
		gameSystemInitializer.AddGameMenuOption("encounter", "continue_preparations", "{=FOoMM4AU}Continue siege preparations.", game_menu_town_besiege_continue_siege_on_condition, game_menu_town_besiege_continue_siege_on_consequence);
		gameSystemInitializer.AddGameMenuOption("encounter", "village_raid_action", "{=lvttCRi8}Plunder the village, then raze it.", game_menu_village_hostile_action_on_condition, game_menu_village_raid_no_resist_on_consequence);
		gameSystemInitializer.AddGameMenuOption("encounter", "village_force_volunteer_action", "{=9YHjPkb8}Force notables to give you recruits.", game_menu_village_hostile_action_on_condition, game_menu_village_force_volunteers_no_resist_loot_on_consequence);
		gameSystemInitializer.AddGameMenuOption("encounter", "village_force_supplies_action", "{=JMzyh6Gl}Force people to give you supplies.", game_menu_village_hostile_action_on_condition, game_menu_village_force_supplies_no_resist_loot_on_consequence);
		gameSystemInitializer.AddGameMenuOption("encounter", "attack", "{=o1pZHZOF}{ATTACK_TEXT}!", game_menu_encounter_attack_on_condition, game_menu_encounter_attack_on_consequence);
		gameSystemInitializer.AddGameMenuOption("encounter", "capture_the_enemy", "{=27yneDGL}Capture the enemy.", game_menu_encounter_capture_the_enemy_on_condition, game_menu_capture_the_enemy_on_consequence);
		gameSystemInitializer.AddGameMenuOption("encounter", "str_order_attack", "{=!}{SEND_TROOPS_TEXT}", game_menu_encounter_order_attack_on_condition, game_menu_encounter_order_attack_on_consequence);
		gameSystemInitializer.AddGameMenuOption("encounter", "leave_soldiers_behind", "{=qNgGoqmI}Try to get away.", game_menu_encounter_leave_your_soldiers_behind_on_condition, delegate
		{
			GameMenu.SwitchToMenu("try_to_get_away");
		});
		gameSystemInitializer.AddGameMenuOption("encounter", "surrender", "{=3nT5wWzb}Surrender.", game_menu_encounter_surrender_on_condition, game_menu_encounter_surrender_on_consequence);
		gameSystemInitializer.AddGameMenuOption("encounter", "leave", "{=2YYRyrOO}Leave...", game_menu_encounter_leave_on_condition, game_menu_encounter_leave_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenuOption("encounter", "abandon_army", "{=Nr49hlfC}Abandon army.", game_menu_encounter_abandon_army_on_condition, game_menu_encounter_abandon_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenuOption("encounter", "go_back_to_settlement", "{=j7bZRFxc}Return to {SETTLEMENT}.", game_menu_sally_out_go_back_to_settlement_on_condition, game_menu_sally_out_go_back_to_settlement_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("army_encounter", "{=!}{ARMY_ENCOUNTER_TEXT}", game_menu_army_encounter_on_init);
		gameSystemInitializer.AddGameMenuOption("army_encounter", "army_talk_to_leader", "{=tYVW8iQN}Talk to army leader", game_menu_army_talk_to_leader_on_condition, game_menu_army_talk_to_leader_on_consequence);
		gameSystemInitializer.AddGameMenuOption("army_encounter", "army_talk_to_other_members", "{=b7APCGY2}Talk to other members", game_menu_army_talk_to_other_members_on_condition, game_menu_army_talk_to_other_members_on_consequence);
		gameSystemInitializer.AddGameMenuOption("army_encounter", "army_join_army", "{=N4Qa0WsT}Join army", game_menu_army_join_on_condition, game_menu_army_join_on_consequence);
		gameSystemInitializer.AddGameMenuOption("army_encounter", "army_attack_army", "{=0URijoc0}Attack army", game_menu_army_attack_on_condition, game_menu_army_attack_on_consequence);
		gameSystemInitializer.AddGameMenuOption("army_encounter", "army_leave", "{=2YYRyrOO}Leave...", game_menu_army_leave_on_condition, army_encounter_leave_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("game_menu_army_talk_to_other_members", "{=yYTotiqW}Talk to...", game_menu_army_talk_to_other_members_on_init);
		gameSystemInitializer.AddGameMenuOption("game_menu_army_talk_to_other_members", "game_menu_army_talk_to_other_members_item", "{=!}{CHAR_NAME}", game_menu_army_talk_to_other_members_item_on_condition, game_menu_army_talk_to_other_members_item_on_consequence, isLeave: false, -1, isRepeatable: true);
		gameSystemInitializer.AddGameMenuOption("game_menu_army_talk_to_other_members", "game_menu_army_talk_to_other_members_back", GameTexts.FindText("str_back").ToString(), game_menu_army_talk_to_other_members_back_on_condition, game_menu_army_talk_to_other_members_back_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("try_to_get_away", "{=bwuawgrW}As the highest tactics skilled member {HIGHEST_TACTICS_SKILLED_MEMBER} ({HIGHEST_TACTICS_SKILL}) devise a plan to disperse into the wilderness to break away from your enemies. You and most men may escape with your lives, but as many as {NEEEDED_MEN_COUNT} {SOLDIER_OR_SOLDIERS} may be lost and part of your baggage could be captured.", game_menu_leave_soldiers_behind_on_init, GameOverlays.MenuOverlayType.Encounter);
		gameSystemInitializer.AddGameMenuOption("try_to_get_away", "try_to_get_away_accept", "{=DbOv36TA}Go ahead with that.", game_menu_try_to_get_away_accept_on_condition, game_menu_encounter_leave_your_soldiers_behind_accept_on_consequence);
		gameSystemInitializer.AddGameMenuOption("try_to_get_away", "try_to_get_away_reject", "{=f1etg9oL}Think of something else.", game_menu_try_to_get_away_reject_on_condition, delegate
		{
			GameMenu.SwitchToMenu("encounter");
		}, isLeave: true);
		gameSystemInitializer.AddGameMenu("try_to_get_away_debrief", "{=ruU70rFl}You disperse into the shrubs and bushes. The enemies halt and seem to hesitate for a while before resuming their pursuit. {CASUALTIES_AND_LOST_BAGGAGE}", game_menu_get_away_debrief_on_init, GameOverlays.MenuOverlayType.Encounter);
		gameSystemInitializer.AddGameMenuOption("try_to_get_away_debrief", "try_to_get_away_continue", "{=veWOovVv}Continue...", game_menu_try_to_get_away_continue_on_condition, game_menu_try_to_get_away_end);
		gameSystemInitializer.AddGameMenu("assault_town", "", game_menu_town_assault_on_init);
		gameSystemInitializer.AddGameMenu("assault_town_order_attack", "", game_menu_town_assault_order_attack_on_init);
		gameSystemInitializer.AddGameMenu("town_outside", "{=!}{TOWN_TEXT}", game_menu_town_outside_on_init);
		gameSystemInitializer.AddGameMenuOption("town_outside", "approach_gates", "{=XlbDnuJx}Approach the gates and hail the guard.", game_menu_castle_outside_approach_gates_on_condition, game_menu_town_outside_approach_gates_on_consequence);
		gameSystemInitializer.AddGameMenuOption("town_outside", "town_disguise_yourself", "{=VCREeAF1}Disguise yourself and sneak through the gate. ({SNEAK_CHANCE}%)", game_menu_town_disguise_yourself_on_condition, game_menu_town_disguise_yourself_on_consequence);
		gameSystemInitializer.AddGameMenuOption("town_outside", "town_besiege", "{=WdIGdHuL}Besiege the town.", game_menu_town_town_besiege_on_condition, game_menu_town_town_besiege_on_consequence);
		gameSystemInitializer.AddGameMenuOption("town_outside", "town_enter_cheat", "{=!}Enter town (Cheat).", game_menu_town_outside_cheat_enter_on_condition, game_menu_town_outside_enter_on_consequence);
		gameSystemInitializer.AddGameMenuOption("town_outside", "town_outside_leave", "{=2YYRyrOO}Leave...", game_menu_castle_outside_leave_on_condition, game_menu_castle_outside_leave_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("menu_sneak_into_town_succeeded", "{=pSSDfAjR}Disguised in the garments of a poor pilgrim, you fool the guards and make your way into the town.", null);
		gameSystemInitializer.AddGameMenuOption("menu_sneak_into_town_succeeded", "str_continue", "{=DM6luo3c}Continue", menu_sneak_into_town_succeeded_continue_on_condition, menu_sneak_into_town_succeeded_continue_on_consequence);
		gameSystemInitializer.AddGameMenu("menu_sneak_into_town_caught", "{=u7yLV7Vr}As you try to sneak in, one of the guards recognizes you and raises the alarm! Another quickly slams the gate shut behind you, and you have no choice but to give up.", game_menu_sneak_into_town_caught_on_init);
		gameSystemInitializer.AddGameMenuOption("menu_sneak_into_town_caught", "mno_sneak_caught_surrender", "{=3nT5wWzb}Surrender.", mno_sneak_caught_surrender_on_condition, mno_sneak_caught_surrender_on_consequence);
		gameSystemInitializer.AddGameMenu("menu_captivity_castle_taken_prisoner", "{=AFJ3BvTH}You are quickly surrounded by guards who take away your weapons. With curses and insults, they throw you into the dungeon where you must while away the miserable days of your captivity.", null);
		gameSystemInitializer.AddGameMenuOption("menu_captivity_castle_taken_prisoner", "mno_sneak_caught_surrender", "{=veWOovVv}Continue...", game_menu_captivity_castle_taken_prisoner_cont_on_condition, game_menu_captivity_castle_taken_prisoner_cont_on_consequence);
		gameSystemInitializer.AddGameMenuOption("menu_captivity_castle_taken_prisoner", "cheat_continue", "{=!}Cheat : Leave.", game_menu_captivity_taken_prisoner_cheat_on_condition, game_menu_captivity_taken_prisoner_cheat_on_consequence);
		gameSystemInitializer.AddGameMenu("fortification_crime_rating", "{=!}{FORTIFICATION_CRIME_RATING_TEXT}", game_menu_fortification_high_crime_rating_on_init);
		gameSystemInitializer.AddGameMenuOption("fortification_crime_rating", "fortification_crime_rating_continue", "{=WVkc4UgX}Continue.", game_menu_fortification_high_crime_rating_continue_on_condition, game_menu_fortification_high_crime_rating_continue_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("army_left_settlement_due_to_war_declaration", "{=!}{ARMY_LEFT_SETTLEMENT_DUE_TO_WAR_TEXT}", game_menu_army_left_settlement_due_to_war_on_init);
		gameSystemInitializer.AddGameMenuOption("army_left_settlement_due_to_war_declaration", "army_left_settlement_due_to_war_declaration_continue", "{=WVkc4UgX}Continue.", game_menu_army_left_settlement_due_to_war_on_condition, game_menu_army_left_settlement_due_to_war_on_consequence);
		gameSystemInitializer.AddGameMenu("castle_outside", "{=!}{TOWN_TEXT}", game_menu_castle_outside_on_init);
		gameSystemInitializer.AddGameMenuOption("castle_outside", "approach_gates", "{=XlbDnuJx}Approach the gates and hail the guard.", game_menu_castle_outside_approach_gates_on_condition, game_menu_castle_outside_approach_gates_on_consequence);
		gameSystemInitializer.AddGameMenuOption("castle_outside", "town_besiege", "{=UzMYZgoE}Besiege the castle.", game_menu_town_town_besiege_on_condition, game_menu_town_town_besiege_on_consequence);
		gameSystemInitializer.AddGameMenuOption("castle_outside", "town_outside_leave", "{=2YYRyrOO}Leave...", game_menu_castle_outside_leave_on_condition, game_menu_castle_outside_leave_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("town_guard", "{=SxkaQbSa}You approach the gate. The men on the walls watch you closely.", null);
		gameSystemInitializer.AddGameMenuOption("town_guard", "request_meeting_commander", "{=RSQbOjub}Request a meeting with someone.", game_menu_request_meeting_someone_on_condition, game_menu_request_meeting_someone_on_consequence);
		gameSystemInitializer.AddGameMenuOption("town_guard", "guard_discuss_criminal_surrender", "{=ACvQdkG8}Discuss the terms of your surrender", outside_menu_criminal_on_condition, outside_menu_criminal_on_consequence);
		gameSystemInitializer.AddGameMenuOption("town_guard", "guard_back", GameTexts.FindText("str_back").ToString(), game_menu_castle_outside_leave_on_condition, game_menu_town_guard_back_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("castle_guard", "{=SxkaQbSa}You approach the gate. The men on the walls watch you closely.", null);
		gameSystemInitializer.AddGameMenuOption("castle_guard", "request_shelter", "{=mG9jW8Fp}Request entry to the castle.", game_menu_town_guard_request_shelter_on_condition, game_menu_request_entry_to_castle_on_consequence);
		gameSystemInitializer.AddGameMenuOption("castle_guard", "request_meeting_commander", "{=RSQbOjub}Request a meeting with someone.", game_menu_request_meeting_someone_on_condition, game_menu_request_meeting_someone_on_consequence);
		gameSystemInitializer.AddGameMenuOption("castle_guard", "guard_back", GameTexts.FindText("str_back").ToString(), game_menu_castle_outside_leave_on_condition, game_menu_town_guard_back_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("menu_castle_entry_granted", "{=Mg1PotzO}After a brief wait, the guards open the gates for you and allow your party inside.", null);
		gameSystemInitializer.AddGameMenuOption("menu_castle_entry_granted", "str_continue", "{=bLNocKd1}Continue..", game_request_entry_to_castle_approved_continue_on_condition, game_request_entry_to_castle_approved_continue_on_consequence);
		gameSystemInitializer.AddGameMenu("menu_castle_entry_denied", "{=QpQQJjD6}The lord of this castle has forbidden you from coming inside these walls, and the guard sergeant informs you that his men will fire if you attempt to come any closer.", menu_castle_entry_denied_on_init);
		gameSystemInitializer.AddGameMenuOption("menu_castle_entry_denied", "str_continue", "{=veWOovVv}Continue...", null, game_request_entry_to_castle_rejected_continue_on_consequence);
		gameSystemInitializer.AddGameMenu("request_meeting", "{=pBAx7jTM}With whom do you want to meet?", game_menu_town_menu_request_meeting_on_init);
		gameSystemInitializer.AddGameMenuOption("request_meeting", "request_meeting_with", "{=!}{HERO_TO_MEET.LINK}", game_menu_request_meeting_with_on_condition, game_menu_request_meeting_with_on_consequence, isLeave: false, -1, isRepeatable: true);
		gameSystemInitializer.AddGameMenuOption("request_meeting", "meeting_town_leave", "{=3nbuRBJK}Forget it.", game_meeting_town_leave_on_condition, game_menu_request_meeting_town_leave_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenuOption("request_meeting", "meeting_castle_leave", "{=3nbuRBJK}Forget it.", game_meeting_castle_leave_on_condition, game_menu_request_meeting_castle_leave_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("request_meeting_with_besiegers", "{=pBAx7jTM}With whom do you want to meet?", game_menu_town_menu_request_meeting_with_besiegers_on_init);
		gameSystemInitializer.AddGameMenuOption("request_meeting_with_besiegers", "request_meeting_with", "{=!}{PARTY_LEADER.LINK}", game_menu_request_meeting_with_besiegers_on_condition, game_menu_request_meeting_with_besiegers_on_consequence, isLeave: false, -1, isRepeatable: true);
		gameSystemInitializer.AddGameMenuOption("request_meeting_with_besiegers", "request_meeting_town_leave", "{=3nbuRBJK}Forget it.", game_meeting_town_leave_on_condition, game_menu_request_meeting_town_leave_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenuOption("request_meeting_with_besiegers", "request_meeting_castle_leave", "{=3nbuRBJK}Forget it.", game_meeting_castle_leave_on_condition, game_menu_request_meeting_castle_leave_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("village_outside", "{=!}.", VillageOutsideOnInit);
		gameSystemInitializer.AddGameMenu("village_loot_complete", "{=qt5bkw8l}On your orders your troops sack the village, pillaging everything of any value, and then put the buildings to the torch. From the coins and valuables that are found, you get your share.", game_menu_village_loot_complete_on_init);
		gameSystemInitializer.AddGameMenuOption("village_loot_complete", "continue", "{=veWOovVv}Continue...", game_menu_village_loot_complete_continue_on_condition, game_menu_village_loot_complete_continue_on_consequence);
		gameSystemInitializer.AddGameMenu("raid_interrupted", "{=KW7amS8c}While your troops are pillaging the countryside, you receive news that the enemy is approaching. You quickly gather up your soldiers and prepare for battle.", null);
		gameSystemInitializer.AddGameMenuOption("raid_interrupted", "continue", "{=veWOovVv}Continue...", game_menu_raid_interrupted_continue_on_condition, game_menu_raid_interrupted_continue_on_consequence);
		gameSystemInitializer.AddGameMenu("encounter_interrupted", "{=lKWflUid}While you are waiting in {DEFENDER}, {ATTACKER} started an attack on it.", game_menu_encounter_interrupted_on_init);
		gameSystemInitializer.AddGameMenuOption("encounter_interrupted", "encounter_interrupted_help_attackers", "{=h3yEHb4U}Help {ATTACKER}.", game_menu_join_encounter_help_attackers_on_condition, game_menu_join_encounter_help_attackers_on_consequence);
		gameSystemInitializer.AddGameMenuOption("encounter_interrupted", "encounter_interrupted_help_defenders", "{=FwIgakj8}Help {DEFENDER}.", game_menu_join_encounter_help_defenders_on_condition, game_menu_join_encounter_help_defenders_on_consequence);
		gameSystemInitializer.AddGameMenuOption("encounter_interrupted", "leave", "{=UgfmaQgx}Leave {DEFENDER}", game_menu_encounter_interrupted_leave_on_condition, game_menu_encounter_interrupted_leave_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("encounter_interrupted_siege_preparations", "{=ABeCWcLi}While you are resting, you hear news that a force led by {ATTACKER} has arrived outside the walls of {DEFENDER} and is beginning preparations for a siege.", game_menu_encounter_interrupted_siege_preparations_on_init);
		gameSystemInitializer.AddGameMenuOption("encounter_interrupted_siege_preparations", "encounter_interrupted_siege_preparations_join_defend", "{=Lxx97yNh}Join the defense of {SETTLEMENT}", game_menu_encounter_interrupted_siege_preparations_join_defend_on_condition, game_menu_encounter_interrupted_siege_preparations_join_defend_on_consequence);
		gameSystemInitializer.AddGameMenuOption("encounter_interrupted_siege_preparations", "encounter_interrupted_siege_preparations_break_out_of_town", "{=ybzBF59f}Break out of {SETTLEMENT}.", game_menu_encounter_interrupted_siege_preparations_break_out_of_town_on_condition, game_menu_encounter_interrupted_break_out_of_town_on_consequence);
		gameSystemInitializer.AddGameMenuOption("encounter_interrupted_siege_preparations", "encounter_interrupted_siege_preparations_leave_town", "{=FILG5eZD}Leave {SETTLEMENT}.", game_menu_encounter_interrupted_siege_preparations_leave_town_on_condition, game_menu_encounter_interrupted_leave_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("encounter_interrupted_raid_started", "{=7o4AfEhN}While you are resting, you hear news that a force led by {ATTACKER} has arrived outside of {DEFENDER} to raid it.", game_menu_encounter_interrupted_by_raid_on_init);
		gameSystemInitializer.AddGameMenuOption("encounter_interrupted_raid_started", "encounter_interrupted_raid_started_leave", "{=WVkc4UgX}Continue.", game_menu_encounter_interrupted_by_raid_continue_on_condition, game_menu_encounter_interrupted_continue_on_consequence);
		gameSystemInitializer.AddGameMenu("continue_siege_after_attack", "{=CVp0j9al}You have defeated the enemies outside the walls. Now you decide to...", null);
		gameSystemInitializer.AddGameMenuOption("continue_siege_after_attack", "continue_siege", "{=zeKvSEpN}Continue the siege", continue_siege_after_attack_on_condition, continue_siege_after_attack_on_consequence);
		gameSystemInitializer.AddGameMenuOption("continue_siege_after_attack", "leave_siege", "{=b7UHp4J9}Leave the siege", leave_siege_after_attack_on_condition, leave_siege_after_attack_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenuOption("continue_siege_after_attack", "leave_army", "{=hSdJ0UUv}Leave Army", leave_army_after_attack_on_condition, leave_army_after_attack_on_consequence, isLeave: true);
		gameSystemInitializer.AddGameMenu("town_caught_by_guards", "{=gVuF84RZ}Guards caught you", null);
		gameSystemInitializer.AddGameMenuOption("town_caught_by_guards", "town_caught_by_guards_criminal_outside_menu_give_yourself_up", "{=DM6luo3c}Continue", outside_menu_criminal_on_condition, caught_outside_menu_criminal_on_consequence);
		gameSystemInitializer.AddGameMenuOption("town_caught_by_guards", "town_caught_by_guards_enemy_outside_menu_give_yourself_up", "{=DM6luo3c}Continue", caught_outside_menu_enemy_on_condition, caught_outside_menu_enemy_on_consequence);
		gameSystemInitializer.AddGameMenu("break_in_menu", "{=sd15CQHI}You devised a plan to distract the besiegers so you can rush the fortress gates, expecting the defenders to let you in. You and most of your men may get through, but as many as {POSSIBLE_CASUALTIES} {?PLURAL}troops{?}troop{\\?} may be lost.", break_in_out_menu_on_init, GameOverlays.MenuOverlayType.Encounter);
		gameSystemInitializer.AddGameMenuOption("break_in_menu", "break_in_menu_accept", "{=DbOv36TA}Go ahead with that.", break_in_menu_accept_on_condition, break_in_menu_accept_on_consequence);
		gameSystemInitializer.AddGameMenuOption("break_in_menu", "break_in_menu_reject", "{=f1etg9oL}Think of something else.", break_in_menu_reject_on_condition, break_in_menu_reject_on_consequence);
		gameSystemInitializer.AddGameMenu("break_in_debrief_menu", "{=PHe0oco1}You fought your way through the attackers to reach the gates. The defenders open them quickly to let you through. When the gates are safely closed behind you, you take a quick tally only to see you have lost the following: {CASUALTIES}.{OTHER_CASUALTIES}", break_in_out_debrief_menu_on_init);
		gameSystemInitializer.AddGameMenuOption("break_in_debrief_menu", "break_in_debrief_continue", "{=DM6luo3c}Continue", break_in_debrief_continue_on_condition, break_in_debrief_continue_on_consequence);
		gameSystemInitializer.AddGameMenu("break_out_menu", "{=J1aEaygO}You devised a plan to fight your way through the attackers to escape from the fortress. You and most of your men may survive, but as many as {POSSIBLE_CASUALTIES} {?PLURAL}troops{?}troop{\\?} may be lost.", break_in_out_menu_on_init, GameOverlays.MenuOverlayType.Encounter);
		gameSystemInitializer.AddGameMenuOption("break_out_menu", "break_out_menu_accept", "{=DbOv36TA}Go ahead with that.", break_out_menu_accept_on_condition, break_out_menu_accept_on_consequence);
		gameSystemInitializer.AddGameMenuOption("break_out_menu", "break_out_menu_reject", "{=f1etg9oL}Think of something else.", break_out_menu_reject_on_condition, break_out_menu_reject_on_consequence);
		gameSystemInitializer.AddGameMenu("break_out_debrief_menu", "{=OzyrsZZK}You fought your way through the attackers to escape from the settlement. When, after some time your forces regroup, you take a quick tally only to see you have lost the following: {CASUALTIES}.{OTHER_CASUALTIES}", break_in_out_debrief_menu_on_init);
		gameSystemInitializer.AddGameMenuOption("break_out_debrief_menu", "break_out_debrief_continue", "{=DM6luo3c}Continue", break_out_debrief_continue_on_condition, break_out_debrief_continue_on_consequence);
	}

	private bool game_menu_encounter_army_lead_inf_on_condition(MenuCallbackArgs args)
	{
		bool flag = MobileParty.MainParty.MapEvent != null && MobileParty.MainParty.MapEvent.PlayerSide == BattleSideEnum.Attacker && MobileParty.MainParty.MapEvent.DefenderSide.TroopCount == 0;
		if (MobileParty.MainParty.MapEvent != null && PlayerEncounter.CheckIfLeadingAvaliable() && !flag)
		{
			return MobileParty.MainParty.MapEvent.PartiesOnSide(MobileParty.MainParty.MapEvent.PlayerSide).Any((MapEventParty party) => party.Party.MemberRoster.GetTroopRoster().Any((TroopRosterElement tr) => tr.Character != null && tr.Character.GetFormationClass() == FormationClass.Infantry));
		}
		return false;
	}

	private void game_menu_encounter_army_lead_inf_on_consequence(MenuCallbackArgs args)
	{
		game_menu_encounter_attack_on_consequence(args);
	}

	private bool game_menu_encounter_army_lead_arc_on_condition(MenuCallbackArgs args)
	{
		bool flag = MobileParty.MainParty.MapEvent != null && MobileParty.MainParty.MapEvent.PlayerSide == BattleSideEnum.Attacker && MobileParty.MainParty.MapEvent.DefenderSide.TroopCount == 0;
		if (MobileParty.MainParty.MapEvent != null && PlayerEncounter.CheckIfLeadingAvaliable() && !flag)
		{
			return MobileParty.MainParty.MapEvent.PartiesOnSide(MobileParty.MainParty.MapEvent.PlayerSide).Any((MapEventParty party) => party.Party.MemberRoster.GetTroopRoster().Any((TroopRosterElement tr) => tr.Character != null && tr.Character.GetFormationClass() == FormationClass.Ranged));
		}
		return false;
	}

	private void game_menu_encounter_army_lead_arc_on_consequence(MenuCallbackArgs args)
	{
		game_menu_encounter_attack_on_consequence(args);
	}

	private bool game_menu_encounter_army_lead_cav_on_condition(MenuCallbackArgs args)
	{
		bool flag = MobileParty.MainParty.MapEvent != null && MobileParty.MainParty.MapEvent.PlayerSide == BattleSideEnum.Attacker && MobileParty.MainParty.MapEvent.DefenderSide.TroopCount == 0;
		if (MobileParty.MainParty.MapEvent != null && PlayerEncounter.CheckIfLeadingAvaliable() && !flag)
		{
			return MobileParty.MainParty.MapEvent.PartiesOnSide(MobileParty.MainParty.MapEvent.PlayerSide).Any((MapEventParty party) => party.Party.MemberRoster.GetTroopRoster().Any((TroopRosterElement tr) => tr.Character != null && tr.Character.GetFormationClass() == FormationClass.Cavalry));
		}
		return false;
	}

	private void game_menu_encounter_army_lead_cav_on_consequence(MenuCallbackArgs args)
	{
		game_menu_encounter_attack_on_consequence(args);
	}

	public static void game_menu_captivity_taken_prisoner_cheat_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.Finish();
	}

	private bool game_menu_encounter_army_lead_har_on_condition(MenuCallbackArgs args)
	{
		bool flag = MobileParty.MainParty.MapEvent != null && MobileParty.MainParty.MapEvent.PlayerSide == BattleSideEnum.Attacker && MobileParty.MainParty.MapEvent.DefenderSide.TroopCount == 0;
		if (MobileParty.MainParty.MapEvent != null && PlayerEncounter.CheckIfLeadingAvaliable() && !flag)
		{
			return MobileParty.MainParty.MapEvent.PartiesOnSide(MobileParty.MainParty.MapEvent.PlayerSide).Any((MapEventParty party) => party.Party.MemberRoster.GetTroopRoster().Any((TroopRosterElement tr) => tr.Character != null && tr.Character.GetFormationClass() == FormationClass.HorseArcher));
		}
		return false;
	}

	private void game_menu_encounter_army_lead_har_on_consequence(MenuCallbackArgs args)
	{
		game_menu_encounter_attack_on_consequence(args);
	}

	private void game_menu_join_encounter_on_init(MenuCallbackArgs args)
	{
		if (MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.2.0") && PlayerEncounter.Current == null)
		{
			GameMenu.ExitToLast();
			return;
		}
		MapEvent encounteredBattle = PlayerEncounter.EncounteredBattle;
		PartyBase leaderParty = encounteredBattle.GetLeaderParty(BattleSideEnum.Attacker);
		PartyBase leaderParty2 = encounteredBattle.GetLeaderParty(BattleSideEnum.Defender);
		if (leaderParty.IsMobile && leaderParty.MobileParty.Army != null)
		{
			MBTextManager.SetTextVariable("ATTACKER", leaderParty.MobileParty.ArmyName);
		}
		else
		{
			MBTextManager.SetTextVariable("ATTACKER", leaderParty.Name);
		}
		if (leaderParty2.IsMobile && leaderParty2.MobileParty.Army != null)
		{
			MBTextManager.SetTextVariable("DEFENDER", leaderParty2.MobileParty.ArmyName);
		}
		else
		{
			MBTextManager.SetTextVariable("DEFENDER", leaderParty2.Name);
		}
		if (encounteredBattle.IsSallyOut)
		{
			MBTextManager.SetTextVariable("JOIN_ENCOUNTER_TEXT", GameTexts.FindText("str_defenders_make_sally_out"));
			StringHelpers.SetCharacterProperties("BESIEGER_LEADER", Campaign.Current.Models.EncounterModel.GetLeaderOfMapEvent(encounteredBattle, BattleSideEnum.Defender).CharacterObject);
		}
		else if (leaderParty2.IsSettlement)
		{
			TextObject text = new TextObject("{=kDiN9iYw}{ATTACKER} is besieging the walls of {DEFENDER}");
			if (encounteredBattle.IsSiegeAssault)
			{
				Settlement.SiegeState currentSiegeState = leaderParty2.Settlement.CurrentSiegeState;
				if (currentSiegeState != 0 && currentSiegeState == Settlement.SiegeState.InTheLordsHall)
				{
					text = new TextObject("{=oXY2wnic}{ATTACKER} is fighting inside the lord's hall of {DEFENDER}");
				}
			}
			else if (encounteredBattle.IsRaid)
			{
				text = ((encounteredBattle.DefenderSide.TroopCount <= 0) ? new TextObject("{=BExNNwm0}{ATTACKER} is raiding {DEFENDER}") : new TextObject("{=kvNQLcCb}{ATTACKER} is fighting in {DEFENDER}"));
			}
			MBTextManager.SetTextVariable("JOIN_ENCOUNTER_TEXT", text);
		}
		else
		{
			MBTextManager.SetTextVariable("JOIN_ENCOUNTER_TEXT", GameTexts.FindText("str_come_across_battle"));
		}
	}

	private bool game_menu_join_encounter_help_attackers_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
		MapEvent encounteredBattle = PlayerEncounter.EncounteredBattle;
		IFaction mapFaction = encounteredBattle.GetLeaderParty(BattleSideEnum.Defender).MapFaction;
		CheckFactionAttackableHonorably(args, mapFaction);
		return encounteredBattle.CanPartyJoinBattle(PartyBase.MainParty, BattleSideEnum.Attacker);
	}

	private void game_menu_join_encounter_help_attackers_on_consequence(MenuCallbackArgs args)
	{
		if (PlayerEncounter.InsideSettlement && PlayerEncounter.EncounterSettlement.IsUnderSiege)
		{
			PlayerEncounter.LeaveSettlement();
		}
		PlayerEncounter.JoinBattle(BattleSideEnum.Attacker);
		if (PlayerEncounter.Battle.DefenderSide.TroopCount > 0)
		{
			GameMenu.SwitchToMenu("encounter");
		}
		else if (MobileParty.MainParty.Army != null)
		{
			if (MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty)
			{
				if (!MobileParty.MainParty.Army.LeaderParty.AttachedParties.Contains(MobileParty.MainParty))
				{
					MobileParty.MainParty.Army.AddPartyToMergedParties(MobileParty.MainParty);
					Campaign.Current.CameraFollowParty = MobileParty.MainParty.Army.LeaderParty.Party;
					CampaignEventDispatcher.Instance.OnArmyOverlaySetDirty();
				}
				if (PlayerEncounter.Battle.IsRaid)
				{
					GameMenu.SwitchToMenu("raiding_village");
				}
				else
				{
					GameMenu.SwitchToMenu("army_wait");
				}
			}
			else if (PlayerEncounter.Battle.IsRaid)
			{
				GameMenu.SwitchToMenu("raiding_village");
				MobileParty.MainParty.Ai.SetMoveModeHold();
			}
			else
			{
				GameMenu.SwitchToMenu("encounter");
			}
		}
		else if (PlayerEncounter.Battle.IsRaid)
		{
			GameMenu.SwitchToMenu("raiding_village");
			MobileParty.MainParty.Ai.SetMoveModeHold();
		}
		else
		{
			GameMenu.SwitchToMenu("menu_siege_strategies");
			MobileParty.MainParty.Ai.SetMoveModeHold();
		}
	}

	private bool game_menu_join_encounter_abandon_army_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		if (MobileParty.MainParty.Army != null)
		{
			return MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty;
		}
		return false;
	}

	private bool game_menu_join_encounter_help_defenders_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
		MapEvent encounteredBattle = PlayerEncounter.EncounteredBattle;
		IFaction mapFaction = encounteredBattle.GetLeaderParty(BattleSideEnum.Attacker).MapFaction;
		CheckFactionAttackableHonorably(args, mapFaction);
		return encounteredBattle.CanPartyJoinBattle(PartyBase.MainParty, BattleSideEnum.Defender);
	}

	public static bool game_menu_captivity_castle_taken_prisoner_cont_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private void game_menu_join_encounter_help_defenders_on_consequence(MenuCallbackArgs args)
	{
		if (PlayerEncounter.EncounteredParty?.MapEvent != null)
		{
			PlayerEncounter.JoinBattle(BattleSideEnum.Defender);
			GameMenu.ActivateGameMenu("encounter");
		}
		else if (PlayerEncounter.Current != null)
		{
			if (PlayerEncounter.EncounterSettlement != null && PlayerEncounter.EncounterSettlement.SiegeEvent != null && !PlayerEncounter.EncounterSettlement.MapFaction.IsAtWarWith(MobileParty.MainParty.MapFaction))
			{
				PlayerEncounter.RestartPlayerEncounter(PlayerEncounter.EncounterSettlement.SiegeEvent.BesiegerCamp.LeaderParty.Party, PartyBase.MainParty, forcePlayerOutFromSettlement: false);
			}
			GameMenu.ActivateGameMenu("encounter");
		}
	}

	private void game_menu_join_siege_event_on_init(MenuCallbackArgs args)
	{
		if (PlayerEncounter.Current != null && PlayerEncounter.EncounterSettlement.Party.SiegeEvent == null)
		{
			PlayerEncounter.Finish();
		}
	}

	private void game_menu_join_sally_out_on_init(MenuCallbackArgs args)
	{
		if (PlayerEncounter.Current != null)
		{
			PlayerEncounter.Current.IsPlayerWaiting = false;
		}
	}

	private bool game_menu_join_siege_event_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
		if (DiplomacyHelper.DidMainHeroSwornNotToAttackFaction(Settlement.CurrentSettlement.MapFaction, out var explanation))
		{
			args.IsEnabled = false;
			args.Tooltip = explanation;
		}
		return Settlement.CurrentSettlement.SiegeEvent?.CanPartyJoinSide(MobileParty.MainParty.Party, BattleSideEnum.Attacker) ?? false;
	}

	private void game_menu_join_siege_event_on_consequence(MenuCallbackArgs args)
	{
		if (Settlement.CurrentSettlement.SiegeEvent.BesiegerCamp.LeaderParty.IsMainParty || Settlement.CurrentSettlement.SiegeEvent.CanPartyJoinSide(MobileParty.MainParty.Party, BattleSideEnum.Attacker))
		{
			MobileParty leaderParty = Settlement.CurrentSettlement.SiegeEvent.BesiegerCamp.LeaderParty;
			if (!leaderParty.IsMainParty)
			{
				MobileParty.MainParty.Ai.SetMoveEscortParty(leaderParty);
			}
			if (Settlement.CurrentSettlement.Party.MapEvent != null)
			{
				PlayerEncounter.JoinBattle(BattleSideEnum.Attacker);
				GameMenu.SwitchToMenu("encounter");
				return;
			}
			Settlement currentSettlement = Settlement.CurrentSettlement;
			if (Hero.MainHero.CurrentSettlement != null)
			{
				PlayerEncounter.LeaveSettlement();
			}
			PlayerEncounter.Finish();
			MobileParty.MainParty.BesiegerCamp = currentSettlement.SiegeEvent.BesiegerCamp;
			PlayerSiege.StartPlayerSiege(BattleSideEnum.Attacker, isSimulation: false, currentSettlement);
			PlayerSiege.StartSiegePreparation();
			Campaign.Current.TimeControlMode = CampaignTimeControlMode.UnstoppablePlay;
		}
		else
		{
			Debug.FailedAssert("Player should not be able to join this siege.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CampaignBehaviors\\EncounterGameMenuBehavior.cs", "game_menu_join_siege_event_on_consequence", 670);
		}
	}

	private bool game_menu_join_sally_out_event_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
		return true;
	}

	private bool game_menu_stay_in_settlement_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		return true;
	}

	private void game_menu_join_sally_out_on_consequence(MenuCallbackArgs args)
	{
		PartyBase sallyOutDefenderLeader = MapEventHelper.GetSallyOutDefenderLeader();
		EncounterManager.StartPartyEncounter(MobileParty.MainParty.Party, sallyOutDefenderLeader);
	}

	private void game_menu_stay_in_settlement_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("menu_siege_strategies");
		if (PlayerEncounter.Current != null)
		{
			PlayerEncounter.Current.IsPlayerWaiting = false;
		}
	}

	private bool break_in_to_help_defender_side_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
		return common_join_siege_event_button_condition(args);
	}

	private bool common_join_siege_event_button_condition(MenuCallbackArgs args)
	{
		SiegeEvent siegeEvent = Settlement.CurrentSettlement.SiegeEvent;
		if (siegeEvent != null)
		{
			MobileParty mainParty = MobileParty.MainParty;
			int lostTroopCountForBreakingInBesiegedSettlement = Campaign.Current.Models.TroopSacrificeModel.GetLostTroopCountForBreakingInBesiegedSettlement(mainParty, siegeEvent);
			int num = mainParty.Army?.TotalRegularCount ?? mainParty.MemberRoster.TotalRegulars;
			if (DiplomacyHelper.DidMainHeroSwornNotToAttackFaction(siegeEvent.BesiegerCamp.LeaderParty.MapFaction, out var explanation))
			{
				args.IsEnabled = false;
				args.Tooltip = explanation;
			}
			else if (lostTroopCountForBreakingInBesiegedSettlement > num)
			{
				args.IsEnabled = false;
				args.Tooltip = new TextObject("{=MTbOGRCF}You don't have enough men!");
			}
			return siegeEvent.CanPartyJoinSide(MobileParty.MainParty.Party, BattleSideEnum.Defender);
		}
		return false;
	}

	private bool attack_besieger_side_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
		return common_join_siege_event_button_condition(args);
	}

	private void game_menu_join_siege_event_on_defender_side_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("break_in_menu");
	}

	private bool game_menu_join_encounter_leave_no_army_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		MBTextManager.SetTextVariable("LEAVE_TEXT", "{=ebUwP3Q3}Don't get involved.");
		if (MobileParty.MainParty.Army != null)
		{
			return MobileParty.MainParty.Army?.LeaderParty == MobileParty.MainParty;
		}
		return true;
	}

	private bool game_menu_join_encounter_leave_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		MobileParty mobileParty = ((Settlement.CurrentSettlement.SiegeEvent != null) ? Settlement.CurrentSettlement.SiegeEvent.BesiegerCamp.LeaderParty : null);
		if (mobileParty != null)
		{
			return !mobileParty.IsMainParty;
		}
		return true;
	}

	private bool game_menu_siege_attacker_left_return_to_settlement_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
		GameTexts.SetVariable("SETTLEMENT", MobileParty.MainParty.LastVisitedSettlement.Name);
		return true;
	}

	private void game_menu_siege_attacker_left_return_to_settlement_on_consequence(MenuCallbackArgs args)
	{
		if (PlayerEncounter.Current != null)
		{
			PlayerEncounter.Finish(forcePlayerOutFromSettlement: false);
		}
		if (MobileParty.MainParty.AttachedTo == null)
		{
			EncounterManager.StartSettlementEncounter(MobileParty.MainParty, MobileParty.MainParty.LastVisitedSettlement);
		}
		else
		{
			EncounterManager.StartSettlementEncounter(MobileParty.MainParty.AttachedTo, MobileParty.MainParty.LastVisitedSettlement);
		}
		if (PlayerEncounter.Current != null && PlayerEncounter.LocationEncounter == null)
		{
			PlayerEncounter.EnterSettlement();
		}
		string genericStateMenu = Campaign.Current.Models.EncounterGameMenuModel.GetGenericStateMenu();
		if (string.IsNullOrEmpty(genericStateMenu))
		{
			GameMenu.ExitToLast();
		}
		else
		{
			GameMenu.SwitchToMenu(genericStateMenu);
		}
	}

	private bool game_menu_siege_attacker_left_leave_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		return true;
	}

	private bool game_menu_siege_attacker_defeated_leave_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		return true;
	}

	private bool game_menu_encounter_cheat_on_condition(MenuCallbackArgs args)
	{
		return Game.Current.CheatMode;
	}

	private void game_menu_encounter_interrupted_on_init(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		PartyBase leaderParty = PlayerEncounter.EncounteredBattle.GetLeaderParty(BattleSideEnum.Attacker);
		MBTextManager.SetTextVariable("ATTACKER", leaderParty.Name);
		MBTextManager.SetTextVariable("DEFENDER", currentSettlement.Name);
	}

	private void game_menu_encounter_interrupted_siege_preparations_on_init(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		TextObject name = Settlement.CurrentSettlement.SiegeEvent.BesiegerCamp.LeaderParty.Name;
		TextObject text = args.MenuContext.GameMenu.GetText();
		text.SetTextVariable("ATTACKER", name);
		text.SetTextVariable("DEFENDER", currentSettlement.Name);
		if (PlayerEncounter.Current != null)
		{
			PlayerEncounter.Current.IsPlayerWaiting = false;
		}
	}

	private bool game_menu_encounter_interrupted_siege_preparations_leave_town_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		args.MenuContext.GameMenu.GetText().SetTextVariable("SETTLEMENT", Settlement.CurrentSettlement.Name);
		return !FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, Hero.MainHero.CurrentSettlement.SiegeEvent.BesiegerCamp.LeaderParty.MapFaction);
	}

	private void game_menu_encounter_interrupted_by_raid_on_init(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		TextObject name = currentSettlement.Party.MapEvent.GetLeaderParty(currentSettlement.Party.OpponentSide).Name;
		TextObject text = args.MenuContext.GameMenu.GetText();
		text.SetTextVariable("ATTACKER", name);
		text.SetTextVariable("DEFENDER", currentSettlement.Name);
		if (PlayerEncounter.Current != null)
		{
			PlayerEncounter.Current.IsPlayerWaiting = false;
		}
	}

	private bool game_menu_encounter_interrupted_by_raid_continue_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private void game_menu_settlement_hide_and_wait_on_consequence(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (currentSettlement.SiegeEvent?.BesiegerCamp.LeaderParty != null)
		{
			GameMenu.SwitchToMenu("encounter_interrupted_siege_preparations");
		}
		else if (currentSettlement.IsTown)
		{
			GameMenu.SwitchToMenu("town");
		}
		else if (currentSettlement.IsCastle)
		{
			GameMenu.SwitchToMenu("castle");
		}
	}

	private bool game_menu_settlement_hide_and_wait_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		return true;
	}

	private bool wait_menu_settlement_hide_and_wait_on_condition(MenuCallbackArgs args)
	{
		args.MenuContext.GameMenu.GetText().SetTextVariable("SETTLEMENT", Settlement.CurrentSettlement.Name);
		Campaign.Current.TimeControlMode = CampaignTimeControlMode.StoppableFastForward;
		args.optionLeaveType = GameMenuOption.LeaveType.Wait;
		return true;
	}

	private bool game_menu_encounter_interrupted_siege_preparations_break_out_of_town_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.LeaveTroopsAndFlee;
		args.MenuContext.GameMenu.GetText().SetTextVariable("SETTLEMENT", Settlement.CurrentSettlement.Name);
		MobileParty mainParty = MobileParty.MainParty;
		SiegeEvent siegeEvent = Settlement.CurrentSettlement.SiegeEvent;
		int lostTroopCountForBreakingOutOfBesiegedSettlement = Campaign.Current.Models.TroopSacrificeModel.GetLostTroopCountForBreakingOutOfBesiegedSettlement(mainParty, siegeEvent);
		int num = ((mainParty.Army != null && mainParty.Army.LeaderParty == mainParty) ? mainParty.Army.TotalRegularCount : mainParty.MemberRoster.TotalRegulars);
		if (mainParty.Army != null && mainParty.Army.LeaderParty != MobileParty.MainParty)
		{
			args.IsEnabled = true;
			TextObject textObject = new TextObject("{=!}If you break out from the siege, you will also leave the army. This is a dishonorable act and you will lose relations with all army member lords.{newline}• Army Leader: {ARMY_LEADER_RELATION_PENALTY}{newline}• Army Members: {ARMY_MEMBER_RELATION_PENALTY}");
			textObject.SetTextVariable("ARMY_LEADER_RELATION_PENALTY", Campaign.Current.Models.TroopSacrificeModel.BreakOutArmyLeaderRelationPenalty);
			textObject.SetTextVariable("ARMY_MEMBER_RELATION_PENALTY", Campaign.Current.Models.TroopSacrificeModel.BreakOutArmyMemberRelationPenalty);
			args.Tooltip = textObject;
		}
		if (lostTroopCountForBreakingOutOfBesiegedSettlement > num)
		{
			args.Tooltip = new TextObject("{=MTbOGRCF}You don't have enough men!");
			args.IsEnabled = false;
		}
		return FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, siegeEvent.BesiegerCamp.LeaderParty.MapFaction);
	}

	private bool game_menu_encounter_interrupted_siege_preparations_hide_in_town_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Wait;
		IFaction mapFaction = Hero.MainHero.MapFaction;
		SiegeEvent siegeEvent = Settlement.CurrentSettlement.SiegeEvent;
		IFaction mapFaction2 = Settlement.CurrentSettlement.MapFaction;
		if (mapFaction != siegeEvent.BesiegerCamp.LeaderParty.MapFaction)
		{
			if (!FactionManager.IsAtWarAgainstFaction(mapFaction2, mapFaction))
			{
				return FactionManager.IsNeutralWithFaction(mapFaction2, mapFaction);
			}
			return true;
		}
		return false;
	}

	private void game_menu_encounter_interrupted_break_out_of_town_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("break_out_menu");
	}

	private void game_menu_encounter_interrupted_siege_preparations_join_defend_on_consequence(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		PlayerSiege.StartPlayerSiege(BattleSideEnum.Defender);
		MobileParty.MainParty.Ai.SetMoveDefendSettlement(currentSettlement);
		PlayerSiege.StartSiegePreparation();
	}

	private bool game_menu_encounter_interrupted_siege_preparations_join_defend_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
		GameTexts.SetVariable("SETTLEMENT", Settlement.CurrentSettlement.Name);
		return Settlement.CurrentSettlement.SiegeEvent.CanPartyJoinSide(PartyBase.MainParty, BattleSideEnum.Defender);
	}

	private void game_menu_encounter_interrupted_leave_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.Finish();
	}

	public static void menu_sneak_into_town_succeeded_continue_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("town");
	}

	public static bool menu_sneak_into_town_succeeded_continue_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	public static void game_menu_sneak_into_town_caught_on_init(MenuCallbackArgs args)
	{
		ChangeCrimeRatingAction.Apply(Settlement.CurrentSettlement.MapFaction, 10f);
	}

	public static void mno_sneak_caught_surrender_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("menu_captivity_castle_taken_prisoner");
	}

	public static bool mno_sneak_caught_surrender_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Surrender;
		return true;
	}

	private void game_menu_encounter_interrupted_continue_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("join_encounter");
	}

	private bool game_menu_encounter_interrupted_leave_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		return true;
	}

	private void game_menu_town_assault_on_init(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("encounter");
		game_menu_encounter_attack_on_consequence(args);
	}

	private void game_menu_town_assault_order_attack_on_init(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("encounter");
		game_menu_encounter_order_attack_on_consequence(args);
	}

	private void game_menu_army_encounter_on_init(MenuCallbackArgs args)
	{
		if (PlayerEncounter.LeaveEncounter)
		{
			PlayerEncounter.Finish();
		}
		else if ((PlayerEncounter.Battle != null && PlayerEncounter.Battle.AttackerSide.LeaderParty != PartyBase.MainParty && PlayerEncounter.Battle.DefenderSide.LeaderParty != PartyBase.MainParty) || PlayerEncounter.MeetingDone)
		{
			if (PlayerEncounter.Battle == null)
			{
				PlayerEncounter.StartBattle();
			}
			if (PlayerEncounter.BattleChallenge)
			{
				GameMenu.SwitchToMenu("duel_starter_menu");
			}
			else
			{
				GameMenu.SwitchToMenu("encounter");
			}
		}
		else if (PlayerEncounter.EncounteredMobileParty.SiegeEvent != null && Settlement.CurrentSettlement != null)
		{
			GameMenu.SwitchToMenu("join_siege_event");
		}
		else if (PlayerEncounter.EncounteredMobileParty != null && PlayerEncounter.EncounteredMobileParty.Army != null)
		{
			MBTextManager.SetTextVariable("ARMY", PlayerEncounter.EncounteredMobileParty.Army.Name);
			MBTextManager.SetTextVariable("ARMY_ENCOUNTER_TEXT", GameTexts.FindText("str_you_have_encountered_ARMY"), sendClients: true);
		}
	}

	private void game_menu_encounter_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetPanelSound("event:/ui/panels/battle/slide_in");
		if (PlayerEncounter.Battle == null)
		{
			if (MobileParty.MainParty.MapEvent != null)
			{
				PlayerEncounter.Init();
			}
			else
			{
				PlayerEncounter.StartBattle();
			}
		}
		PlayerEncounter.Update();
		UpdateVillageHostileActionEncounter(args);
		if (PlayerEncounter.Current == null)
		{
			Campaign.Current.SaveHandler.SignalAutoSave();
		}
	}

	private void UpdateVillageHostileActionEncounter(MenuCallbackArgs args)
	{
		MapEvent battle = PlayerEncounter.Battle;
		if (!(Game.Current.GameStateManager.ActiveState is MapState) || battle?.MapEventSettlement == null || !battle.MapEventSettlement.IsVillage || !battle.DefenderSide.LeaderParty.IsSettlement || battle.AttackerSide != battle.GetMapEventSide(battle.PlayerSide))
		{
			return;
		}
		bool flag = battle.DefenderSide.Parties.All((MapEventParty x) => x.Party.MemberRoster.TotalHealthyCount == 0);
		bool flag2 = ConsiderMilitiaSurrenderPossibility();
		if (!(flag || flag2))
		{
			return;
		}
		if (!flag)
		{
			for (int num = battle.DefenderSide.Parties.Count - 1; num >= 0; num--)
			{
				if (battle.DefenderSide.Parties[num].Party.IsMobile)
				{
					battle.DefenderSide.Parties[num].Party.MapEventSide = null;
				}
			}
			if (!battle.IsRaid)
			{
				battle.SetOverrideWinner(BattleSideEnum.Attacker);
			}
		}
		if (battle.IsRaid)
		{
			game_menu_village_raid_no_resist_on_consequence(args);
		}
		else if (battle.IsForcingSupplies)
		{
			game_menu_village_force_supplies_no_resist_loot_on_consequence(args);
		}
		else if (battle.IsForcingVolunteers)
		{
			game_menu_village_force_volunteers_no_resist_loot_on_consequence(args);
		}
	}

	public static bool game_menu_captivity_taken_prisoner_cheat_on_condition(MenuCallbackArgs args)
	{
		return Game.Current.IsDevelopmentMode;
	}

	private bool ConsiderMilitiaSurrenderPossibility()
	{
		bool result = false;
		MapEvent battle = PlayerEncounter.Battle;
		if ((battle.IsRaid || battle.IsForcingSupplies || battle.IsForcingVolunteers) && battle.MapEventSettlement.IsVillage)
		{
			Settlement mapEventSettlement = battle.MapEventSettlement;
			float num = 0f;
			bool flag = false;
			foreach (MapEventParty party in battle.DefenderSide.Parties)
			{
				num += party.Party.TotalStrength;
				if (party.Party.IsMobile && party.Party.MobileParty.IsLordParty)
				{
					flag = true;
				}
			}
			float num2 = 0f;
			foreach (MapEventParty party2 in battle.AttackerSide.Parties)
			{
				if (!party2.Party.IsMobile || party2.Party.MobileParty.Army == null)
				{
					num2 += party2.Party.TotalStrength;
				}
				else
				{
					if (!party2.Party.IsMobile || party2.Party.MobileParty.Army == null || party2.Party.MobileParty.Army.LeaderParty != party2.Party.MobileParty)
					{
						continue;
					}
					foreach (MobileParty attachedParty in party2.Party.MobileParty.Army.LeaderParty.AttachedParties)
					{
						num2 += attachedParty.Party.TotalStrength;
					}
				}
			}
			float num3 = ((mapEventSettlement.OwnerClan?.Leader?.PartyBelongedTo != null) ? mapEventSettlement.OwnerClan.Leader.PartyBelongedTo.Party.RandomFloatWithSeed(1u, 0.05f, 0.15f) : 0.1f);
			result = !flag && num2 * num3 > num;
		}
		return result;
	}

	private bool game_menu_encounter_leave_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		if ((MobileParty.MainParty.Army == null || MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty) && MapEventHelper.CanLeaveBattle(MobileParty.MainParty))
		{
			if (MobileParty.MainParty.MapEvent.IsSallyOut && MobileParty.MainParty.MapEvent.PlayerSide != 0)
			{
				return MobileParty.MainParty.CurrentSettlement == null;
			}
			return true;
		}
		return false;
	}

	private bool game_menu_sally_out_go_back_to_settlement_on_condition(MenuCallbackArgs args)
	{
		if (MobileParty.MainParty.MapEvent != null && MobileParty.MainParty.MapEvent.IsSallyOut && MobileParty.MainParty.MapEvent.PlayerSide == BattleSideEnum.Attacker && MobileParty.MainParty.CurrentSettlement != null)
		{
			bool flag = Campaign.Current.Models.EncounterModel.GetLeaderOfMapEvent(MobileParty.MainParty.MapEvent, MobileParty.MainParty.MapEvent.PlayerSide) == Hero.MainHero;
			if ((MobileParty.MainParty.Army == null || MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty || flag) && (MobileParty.MainParty.SiegeEvent == null || !MobileParty.MainParty.SiegeEvent.BesiegerCamp.IsBesiegerSideParty(MobileParty.MainParty)) && !PlayerEncounter.Current.CheckIfBattleShouldContinueAfterBattleMission())
			{
				args.optionLeaveType = GameMenuOption.LeaveType.Leave;
				GameTexts.SetVariable("SETTLEMENT", MobileParty.MainParty.LastVisitedSettlement.Name);
				return true;
			}
		}
		return false;
	}

	private void game_menu_sally_out_go_back_to_settlement_consequence(MenuCallbackArgs args)
	{
		MapEvent playerMapEvent = MapEvent.PlayerMapEvent;
		playerMapEvent.BeginWait();
		if (Campaign.Current.Models.EncounterModel.GetLeaderOfMapEvent(playerMapEvent, playerMapEvent.PlayerSide) == Hero.MainHero)
		{
			PlayerEncounter.Current.FinalizeBattle();
			PlayerEncounter.Current.SetupFields(Settlement.CurrentSettlement.SiegeEvent.BesiegerCamp.LeaderParty.Party, PartyBase.MainParty);
		}
		else
		{
			PlayerEncounter.LeaveBattle();
		}
		GameMenu.SwitchToMenu("menu_siege_strategies");
	}

	private bool game_menu_encounter_abandon_army_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		if (MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty && !MobileParty.MainParty.MapEvent.IsSallyOut)
		{
			return MapEventHelper.CanLeaveBattle(MobileParty.MainParty);
		}
		return false;
	}

	private void game_menu_army_talk_to_leader_on_consequence(MenuCallbackArgs args)
	{
		Campaign.Current.CurrentConversationContext = ConversationContext.PartyEncounter;
		args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
		ConversationCharacterData playerCharacterData = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty);
		ConversationCharacterData conversationPartnerData = new ConversationCharacterData(ConversationHelper.GetConversationCharacterPartyLeader(PlayerEncounter.EncounteredParty), PlayerEncounter.EncounteredParty);
		PlayerEncounter.SetMeetingDone();
		CampaignMapConversation.OpenConversation(playerCharacterData, conversationPartnerData);
	}

	private bool game_menu_army_talk_to_leader_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
		if (PlayerEncounter.EncounteredParty?.LeaderHero != null)
		{
			MenuHelper.SetIssueAndQuestDataForHero(args, PlayerEncounter.EncounteredParty.LeaderHero);
		}
		return true;
	}

	public static void game_menu_captivity_castle_taken_prisoner_cont_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.ExitToLast();
		PartyBase.MainParty.AddElementToMemberRoster(CharacterObject.PlayerCharacter, -1, insertAtFront: true);
		TakePrisonerAction.Apply(Settlement.CurrentSettlement.Party, Hero.MainHero);
	}

	private bool game_menu_army_talk_to_other_members_on_condition(MenuCallbackArgs args)
	{
		foreach (MobileParty attachedParty in PlayerEncounter.EncounteredMobileParty.Army.LeaderParty.AttachedParties)
		{
			Hero leaderHero = attachedParty.LeaderHero;
			if (leaderHero != null)
			{
				MenuHelper.SetIssueAndQuestDataForHero(args, leaderHero);
			}
		}
		args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
		if (!FactionManager.IsAtWarAgainstFaction(MobileParty.MainParty.MapFaction, PlayerEncounter.EncounteredMobileParty.MapFaction))
		{
			return PlayerEncounter.EncounteredMobileParty.Army.LeaderParty.AttachedParties.Count > 0;
		}
		return false;
	}

	private void game_menu_army_talk_to_other_members_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("game_menu_army_talk_to_other_members");
	}

	private void game_menu_army_talk_to_other_members_on_init(MenuCallbackArgs args)
	{
		if (PlayerEncounter.LeaveEncounter)
		{
			PlayerEncounter.Finish();
			return;
		}
		args.MenuContext.SetRepeatObjectList(PlayerEncounter.EncounteredMobileParty.Army.LeaderParty.AttachedParties.ToList());
		if (PlayerEncounter.EncounteredMobileParty.MapFaction.IsAtWarWith(MobileParty.MainParty.MapFaction))
		{
			GameMenu.SwitchToMenu("encounter");
		}
	}

	private bool game_menu_army_talk_to_other_members_item_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
		MobileParty mobileParty = args.MenuContext.GetCurrentRepeatableObject() as MobileParty;
		MBTextManager.SetTextVariable("CHAR_NAME", mobileParty?.LeaderHero.Name);
		if (mobileParty != null && mobileParty.LeaderHero != null)
		{
			MenuHelper.SetIssueAndQuestDataForHero(args, mobileParty.LeaderHero);
		}
		return true;
	}

	private void game_menu_army_talk_to_other_members_item_on_consequence(MenuCallbackArgs args)
	{
		MobileParty mobileParty = args.MenuContext.GetSelectedObject() as MobileParty;
		Campaign.Current.CurrentConversationContext = ConversationContext.PartyEncounter;
		CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty), new ConversationCharacterData(ConversationHelper.GetConversationCharacterPartyLeader(mobileParty.Party), mobileParty.Party));
	}

	private bool game_menu_army_talk_to_other_members_back_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		return true;
	}

	private void game_menu_army_talk_to_other_members_back_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("army_encounter");
	}

	private bool game_menu_army_attack_on_condition(MenuCallbackArgs args)
	{
		CheckEnemyAttackableHonorably(args);
		args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
		return MobileParty.MainParty.MapFaction.IsAtWarWith(PlayerEncounter.EncounteredMobileParty.MapFaction);
	}

	private void CheckEnemyAttackableHonorably(MenuCallbackArgs args)
	{
		if ((MobileParty.MainParty.Army == null || MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty) && !PlayerEncounter.PlayerIsDefender)
		{
			IFaction mapFaction = PlayerEncounter.EncounteredParty.MapFaction;
			if (mapFaction != null && mapFaction.NotAttackableByPlayerUntilTime.IsFuture)
			{
				args.IsEnabled = false;
				args.Tooltip = EnemyNotAttackableTooltip;
			}
		}
	}

	private void CheckFactionAttackableHonorably(MenuCallbackArgs args, IFaction faction)
	{
		if (faction.NotAttackableByPlayerUntilTime.IsFuture)
		{
			args.IsEnabled = false;
			args.Tooltip = EnemyNotAttackableTooltip;
		}
	}

	private void CheckFortificationAttackableHonorably(MenuCallbackArgs args)
	{
		if (MobileParty.MainParty.Army == null || MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty)
		{
			IFaction mapFaction = PlayerEncounter.EncounterSettlement.MapFaction;
			if (mapFaction != null && mapFaction.NotAttackableByPlayerUntilTime.IsFuture)
			{
				args.IsEnabled = false;
				args.Tooltip = EnemyNotAttackableTooltip;
			}
		}
	}

	private bool game_menu_army_leave_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		return true;
	}

	private void game_menu_army_attack_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("encounter");
	}

	private bool game_menu_army_join_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		if (PlayerEncounter.EncounteredMobileParty.MapFaction != MobileParty.MainParty.MapFaction)
		{
			return false;
		}
		if (PlayerEncounter.EncounteredMobileParty.Army == MobileParty.MainParty.Army)
		{
			return false;
		}
		if (PlayerEncounter.EncounteredMobileParty.MapFaction != null)
		{
			foreach (Kingdom item in Kingdom.All)
			{
				if (item.IsAtWarWith(Clan.PlayerClan.MapFaction) && item.NotAttackableByPlayerUntilTime.IsFuture)
				{
					args.IsEnabled = false;
					args.Tooltip = GameTexts.FindText("str_cant_join_army_safe_passage");
				}
			}
		}
		if (MobileParty.MainParty.Army == null)
		{
			return PlayerEncounter.EncounteredMobileParty.MapFaction == MobileParty.MainParty.MapFaction;
		}
		return false;
	}

	private void game_menu_army_join_on_consequence(MenuCallbackArgs args)
	{
		MobileParty.MainParty.Army = PlayerEncounter.EncounteredMobileParty.Army;
		MobileParty.MainParty.Army.AddPartyToMergedParties(MobileParty.MainParty);
		PlayerEncounter.Finish();
	}

	private void army_encounter_leave_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.Finish();
	}

	private void game_menu_encounter_leave_on_consequence(MenuCallbackArgs args)
	{
		Settlement besiegedSettlement = MobileParty.MainParty.BesiegedSettlement;
		if (besiegedSettlement != null && besiegedSettlement.CurrentSiegeState == Settlement.SiegeState.InTheLordsHall)
		{
			TextObject textObject = new TextObject("{=h3YuHSRb}Are you sure you want to abandon the siege?");
			InformationManager.ShowInquiry(new InquiryData(GameTexts.FindText("str_decision").ToString(), textObject.ToString(), isAffirmativeOptionShown: true, isNegativeOptionShown: true, GameTexts.FindText("str_yes").ToString(), GameTexts.FindText("str_no").ToString(), delegate
			{
				MenuHelper.EncounterLeaveConsequence();
			}, null));
		}
		else
		{
			MenuHelper.EncounterLeaveConsequence();
		}
	}

	private void game_menu_encounter_abandon_on_consequence(MenuCallbackArgs args)
	{
		((PlayerEncounter.Battle != null) ? PlayerEncounter.Battle : PlayerEncounter.EncounteredBattle).BeginWait();
		MobileParty.MainParty.Ai.SetMoveModeHold();
		Hero.MainHero.PartyBelongedTo.Army = null;
		PlayerEncounter.Finish();
		if (MobileParty.MainParty.BesiegerCamp != null)
		{
			MobileParty.MainParty.BesiegerCamp = null;
		}
	}

	private bool game_menu_encounter_surrender_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Surrender;
		if (MobileParty.MainParty.MapEvent != null && !MapEventHelper.CanLeaveBattle(MobileParty.MainParty) && PartyBase.MainParty.Side == BattleSideEnum.Defender && MobileParty.MainParty.MapEvent.DefenderSide.TroopCount == MobileParty.MainParty.Party.NumberOfHealthyMembers)
		{
			return true;
		}
		return false;
	}

	private void game_menu_encounter_surrender_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.PlayerSurrender = true;
		PlayerEncounter.Update();
		if (!Hero.MainHero.CanBecomePrisoner())
		{
			GameMenu.ActivateGameMenu("menu_captivity_end_no_more_enemies");
		}
	}

	private bool game_menu_encounter_attack_on_condition(MenuCallbackArgs args)
	{
		CheckEnemyAttackableHonorably(args);
		return MenuHelper.EncounterAttackCondition(args);
	}

	private bool game_menu_encounter_capture_the_enemy_on_condition(MenuCallbackArgs args)
	{
		return MenuHelper.EncounterCaptureEnemyCondition(args);
	}

	private void game_menu_encounter_attack_on_consequence(MenuCallbackArgs args)
	{
		MenuHelper.EncounterAttackConsequence(args);
	}

	private void game_menu_capture_the_enemy_on_consequence(MenuCallbackArgs args)
	{
		MenuHelper.EncounterCaptureTheEnemyOnConsequence(args);
	}

	private bool game_menu_encounter_order_attack_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.OrderTroopsToAttack;
		MapEvent playerMapEvent = MapEvent.PlayerMapEvent;
		if (playerMapEvent != null)
		{
			CheckEnemyAttackableHonorably(args);
			int num = 0;
			foreach (MapEventParty item in playerMapEvent.PartiesOnSide(MobileParty.MainParty.Party.Side))
			{
				num += item.Party.MemberRoster.Sum((TroopRosterElement x) => x.Character.IsHero ? ((x.Character != CharacterObject.PlayerCharacter && !x.Character.HeroObject.IsWounded) ? 1 : 0) : (x.Number - x.WoundedNumber));
			}
			if (playerMapEvent.HasTroopsOnBothSides() && playerMapEvent.GetLeaderParty(PartyBase.MainParty.OpponentSide) != null && num > 0)
			{
				if (MobileParty.MainParty.MemberRoster.Sum((TroopRosterElement x) => x.Character.IsHero ? ((x.Character != CharacterObject.PlayerCharacter && !x.Character.HeroObject.IsWounded) ? 1 : 0) : (x.Number - x.WoundedNumber)) > 0)
				{
					MBTextManager.SetTextVariable("SEND_TROOPS_TEXT", "{=QfMeoKOm}Send troops.");
				}
				else
				{
					MBTextManager.SetTextVariable("SEND_TROOPS_TEXT", "{=jo3UHKMD}Leave it to the others.");
				}
				if (playerMapEvent.IsInvulnerable)
				{
					playerMapEvent.IsInvulnerable = false;
				}
				IFaction mapFaction = PlayerEncounter.EncounteredParty.MapFaction;
				if (mapFaction == null || mapFaction.NotAttackableByPlayerUntilTime.IsPast)
				{
					args.Tooltip = TooltipHelper.GetSendTroopsPowerContextTooltipForMapEvent();
				}
				return true;
			}
		}
		return false;
	}

	private void game_menu_encounter_order_attack_on_consequence(MenuCallbackArgs args)
	{
		MenuHelper.EncounterOrderAttackConsequence(args);
	}

	private bool game_menu_encounter_leave_your_soldiers_behind_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.LeaveTroopsAndFlee;
		if (PartyBase.MainParty.Side == BattleSideEnum.Defender && PlayerEncounter.Battle.DefenderSide.LeaderParty == PartyBase.MainParty && !MobileParty.MainParty.MapEvent.HasWinner)
		{
			int num = (_intEncounterVariable = Campaign.Current.Models.TroopSacrificeModel.GetNumberOfTroopsSacrificedForTryingToGetAway(PlayerEncounter.Current.PlayerSide, PlayerEncounter.Battle));
			int num2 = PartyBase.MainParty.NumberOfRegularMembers;
			if (MobileParty.MainParty.Army != null)
			{
				foreach (MobileParty attachedParty in MobileParty.MainParty.Army.LeaderParty.AttachedParties)
				{
					num2 += attachedParty.Party.NumberOfRegularMembers;
				}
			}
			if (num < 0 || num2 < num)
			{
				args.Tooltip = new TextObject("{=MTbOGRCF}You don't have enough men!");
				args.IsEnabled = false;
			}
			return true;
		}
		return false;
	}

	private void game_menu_leave_soldiers_behind_on_init(MenuCallbackArgs args)
	{
		Hero heroWithHighestSkill = MobilePartyHelper.GetHeroWithHighestSkill(MobileParty.MainParty, DefaultSkills.Tactics);
		int content = heroWithHighestSkill?.GetSkillValue(DefaultSkills.Tactics) ?? 0;
		MBTextManager.SetTextVariable("HIGHEST_TACTICS_SKILL", content);
		MBTextManager.SetTextVariable("HIGHEST_TACTICS_SKILLED_MEMBER", (heroWithHighestSkill != null && heroWithHighestSkill != Hero.MainHero) ? heroWithHighestSkill.Name : GameTexts.FindText("str_you"));
		MBTextManager.SetTextVariable("NEEEDED_MEN_COUNT", _intEncounterVariable);
		MBTextManager.SetTextVariable("SOLDIER_OR_SOLDIERS", (_intEncounterVariable <= 1) ? GameTexts.FindText("str_soldier") : GameTexts.FindText("str_soldiers"));
	}

	public static void game_request_entry_to_castle_approved_continue_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("castle");
	}

	public static bool game_request_entry_to_castle_approved_continue_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	public static void game_request_entry_to_castle_rejected_continue_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("castle_outside");
	}

	public static void menu_castle_entry_denied_on_init(MenuCallbackArgs args)
	{
	}

	private void game_menu_encounter_leave_your_soldiers_behind_accept_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.SacrificeTroops(_intEncounterVariable, out _getAwayCasualties, out _getAwayLostBaggage);
		CampaignEventDispatcher.Instance.OnPlayerDesertedBattle(_intEncounterVariable);
		if (MobileParty.MainParty.BesiegerCamp != null)
		{
			MobileParty.MainParty.BesiegerCamp = null;
		}
		if (Campaign.Current.CurrentMenuContext != null)
		{
			GameMenu.SwitchToMenu("try_to_get_away_debrief");
		}
		else
		{
			GameMenu.ActivateGameMenu("try_to_get_away_debrief");
		}
	}

	private void game_menu_get_away_debrief_on_init(MenuCallbackArgs args)
	{
		TextObject textObject = GameTexts.FindText("str_STR1_space_STR2");
		if (_getAwayCasualties != null)
		{
			TextObject textObject2;
			if (_getAwayCasualties.Count > 0)
			{
				textObject2 = new TextObject("{=NhHzgs5e}When, after some time your forces regroup, you take a quick tally of your troops only to see you have lost: {CASUALTIES}.");
				textObject2.SetTextVariable("CASUALTIES", PartyBaseHelper.PrintRegularTroopCategories(_getAwayCasualties));
			}
			else
			{
				textObject2 = new TextObject("{=bXzAluln}When, after some time your forces regroup, you take a quick tally of your troops to see that your forces are intact.");
			}
			textObject.SetTextVariable("STR1", textObject2);
		}
		if (_getAwayLostBaggage != null)
		{
			TextObject textObject3 = TextObject.Empty;
			if (_getAwayLostBaggage.Count > 0)
			{
				textObject3 = new TextObject("{=mrjz1ka4}And in your lost baggage you had: {LOST_BAGGAGE}.");
				textObject3.SetTextVariable("LOST_BAGGAGE", PartyBaseHelper.PrintSummarisedItemRoster(_getAwayLostBaggage));
			}
			textObject.SetTextVariable("STR2", textObject3);
		}
		MBTextManager.SetTextVariable("CASUALTIES_AND_LOST_BAGGAGE", textObject);
	}

	private bool game_menu_try_to_get_away_continue_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private bool game_menu_try_to_get_away_reject_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		return true;
	}

	private bool game_menu_try_to_get_away_accept_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private void game_menu_try_to_get_away_end(MenuCallbackArgs args)
	{
		foreach (MapEventParty item in PlayerEncounter.Battle.PartiesOnSide(BattleSideEnum.Defender))
		{
			if (item.Party.MobileParty != null)
			{
				if (item.Party.MobileParty.BesiegerCamp != null)
				{
					item.Party.MobileParty.BesiegerCamp = null;
				}
				if (item.Party.MobileParty.CurrentSettlement != null && item.Party == PartyBase.MainParty)
				{
					LeaveSettlementAction.ApplyForParty(item.Party.MobileParty);
				}
			}
		}
		PlayerEncounter.Battle.DiplomaticallyFinished = true;
		PlayerEncounter.ProtectPlayerSide();
		PlayerEncounter.Finish();
	}

	private bool game_menu_town_besiege_continue_siege_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		PartyBase encounteredParty = PlayerEncounter.EncounteredParty;
		if (encounteredParty == null)
		{
			return false;
		}
		MapEvent encounteredBattle = PlayerEncounter.EncounteredBattle;
		if (encounteredBattle != null && encounteredBattle.GetLeaderParty(PartyBase.MainParty.Side) == PartyBase.MainParty && encounteredParty.IsSettlement && encounteredParty.Settlement.IsFortification && FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, encounteredParty.MapFaction) && encounteredParty.Settlement.IsUnderSiege)
		{
			return encounteredParty.Settlement.CurrentSiegeState == Settlement.SiegeState.OnTheWalls;
		}
		return false;
	}

	private void game_menu_town_besiege_continue_siege_on_consequence(MenuCallbackArgs args)
	{
		if (PlayerEncounter.Battle != null)
		{
			PlayerEncounter.Finish();
		}
		PlayerSiege.StartSiegePreparation();
	}

	private bool game_menu_village_hostile_action_on_condition(MenuCallbackArgs args)
	{
		if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsVillage)
		{
			args.optionLeaveType = GameMenuOption.LeaveType.Raid;
			MapEvent battle = PlayerEncounter.Battle;
			if (PartyBase.MainParty.Side == BattleSideEnum.Attacker)
			{
				return !battle.PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty party) => party.Party.NumberOfHealthyMembers > 0);
			}
			return false;
		}
		return false;
	}

	private void game_menu_village_raid_no_resist_on_consequence(MenuCallbackArgs args)
	{
		BeHostileAction.ApplyEncounterHostileAction(PartyBase.MainParty, Settlement.CurrentSettlement.Party);
		if (PlayerEncounter.Current != null)
		{
			if (PlayerEncounter.InsideSettlement)
			{
				PlayerEncounter.LeaveSettlement();
			}
			GameMenu.ActivateGameMenu("raiding_village");
		}
	}

	private void game_menu_village_force_supplies_no_resist_loot_on_consequence(MenuCallbackArgs args)
	{
		BeHostileAction.ApplyMinorCoercionHostileAction(PartyBase.MainParty, Settlement.CurrentSettlement.Party);
		GameMenu.ActivateGameMenu("force_supplies_village");
	}

	private void game_menu_village_force_volunteers_no_resist_loot_on_consequence(MenuCallbackArgs args)
	{
		BeHostileAction.ApplyMajorCoercionHostileAction(PartyBase.MainParty, Settlement.CurrentSettlement.Party);
		GameMenu.ActivateGameMenu("force_volunteers_village");
	}

	private void game_menu_taken_prisoner_on_init(MenuCallbackArgs args)
	{
	}

	private bool game_menu_taken_prisoner_continue_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private void game_menu_taken_prisoner_continue_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.ExitToLast();
	}

	private void game_menu_encounter_meeting_on_init(MenuCallbackArgs args)
	{
		if (PlayerEncounter.Current != null && ((PlayerEncounter.Battle != null && PlayerEncounter.Battle.AttackerSide.LeaderParty != PartyBase.MainParty && PlayerEncounter.Battle.DefenderSide.LeaderParty != PartyBase.MainParty) || PlayerEncounter.MeetingDone))
		{
			if (PlayerEncounter.LeaveEncounter)
			{
				PlayerEncounter.Finish();
				return;
			}
			if (PlayerEncounter.Battle == null)
			{
				PlayerEncounter.StartBattle();
			}
			if (PlayerEncounter.BattleChallenge)
			{
				GameMenu.SwitchToMenu("duel_starter_menu");
			}
			else
			{
				GameMenu.SwitchToMenu("encounter");
			}
		}
		else
		{
			PlayerEncounter.DoMeeting();
		}
	}

	private void VillageOutsideOnInit(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("village");
	}

	private void game_menu_town_outside_on_init(MenuCallbackArgs args)
	{
		Settlement encounterSettlement = PlayerEncounter.EncounterSettlement;
		args.MenuTitle = encounterSettlement.Name;
		TextObject empty = TextObject.Empty;
		Campaign.Current.Models.SettlementAccessModel.CanMainHeroEnterSettlement(encounterSettlement, out _accessDetails);
		SettlementAccessModel.AccessLevel accessLevel = _accessDetails.AccessLevel;
		int num = (int)accessLevel;
		if (num != 0)
		{
			if (num != 1 || _accessDetails.AccessLimitationReason != SettlementAccessModel.AccessLimitationReason.CrimeRating)
			{
				goto IL_010d;
			}
			empty = GameTexts.FindText("str_gate_down_criminal_text");
			empty.SetTextVariable("FACTION", Settlement.CurrentSettlement.MapFaction.Name);
		}
		else if (_accessDetails.AccessLimitationReason == SettlementAccessModel.AccessLimitationReason.HostileFaction)
		{
			if (encounterSettlement.InRebelliousState)
			{
				empty = GameTexts.FindText("str_gate_down_enemy_text_castle_low_loyalty");
				empty.SetTextVariable("FACTION_INFORMAL_NAME", encounterSettlement.MapFaction.InformalName);
			}
			else
			{
				empty = GameTexts.FindText("str_gate_down_enemy_text_castle");
			}
		}
		else
		{
			if (_accessDetails.AccessLimitationReason != SettlementAccessModel.AccessLimitationReason.CrimeRating)
			{
				goto IL_010d;
			}
			empty = GameTexts.FindText("str_gate_down_criminal_text");
			empty.SetTextVariable("FACTION", Settlement.CurrentSettlement.MapFaction.Name);
		}
		goto IL_0146;
		IL_0146:
		empty.SetTextVariable("SETTLEMENT_NAME", encounterSettlement.EncyclopediaLinkWithName);
		empty.SetTextVariable("FACTION_TERM", encounterSettlement.MapFaction.EncyclopediaLinkWithName);
		MBTextManager.SetTextVariable("TOWN_TEXT", empty);
		if (_accessDetails.PreliminaryActionObligation == SettlementAccessModel.PreliminaryActionObligation.Optional && _accessDetails.PreliminaryActionType == SettlementAccessModel.PreliminaryActionType.FaceCharges)
		{
			GameMenu.SwitchToMenu("town_inside_criminal");
		}
		else if (_accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.FullAccess && _accessDetails.AccessMethod == SettlementAccessModel.AccessMethod.Direct)
		{
			GameMenu.SwitchToMenu("town");
		}
		return;
		IL_010d:
		if (encounterSettlement.InRebelliousState)
		{
			empty = GameTexts.FindText("str_settlement_not_allowed_text_low_loyalty");
			empty.SetTextVariable("FACTION_INFORMAL_NAME", encounterSettlement.MapFaction.InformalName);
		}
		else
		{
			empty = GameTexts.FindText("str_settlement_not_allowed_text");
		}
		goto IL_0146;
	}

	private void game_menu_fortification_high_crime_rating_on_init(MenuCallbackArgs args)
	{
		TextObject textObject = new TextObject("{=DdeIg5hz}As you move through the streets, you hear whispers of an upcoming war between your faction and {SETTLEMENT_FACTION}. Upon hearing this, you slink away without attracting any suspicion.");
		textObject.SetTextVariable("SETTLEMENT_FACTION", Settlement.CurrentSettlement.MapFaction.EncyclopediaLinkWithName);
		MBTextManager.SetTextVariable("FORTIFICATION_CRIME_RATING_TEXT", textObject);
	}

	private bool game_menu_fortification_high_crime_rating_continue_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private void game_menu_army_left_settlement_due_to_war_on_init(MenuCallbackArgs args)
	{
		TextObject textObject = new TextObject("{=Nsb6SD4y}After receiving word of an upcoming war against {ENEMY_FACTION}, {ARMY_NAME} decided to leave {SETTLEMENT_NAME}.");
		textObject.SetTextVariable("ENEMY_FACTION", Settlement.CurrentSettlement.MapFaction.EncyclopediaLinkWithName);
		textObject.SetTextVariable("ARMY_NAME", MobileParty.MainParty.Army.Name);
		textObject.SetTextVariable("SETTLEMENT_NAME", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
		MBTextManager.SetTextVariable("ARMY_LEFT_SETTLEMENT_DUE_TO_WAR_TEXT", textObject);
	}

	private bool game_menu_army_left_settlement_due_to_war_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private void game_menu_castle_outside_on_init(MenuCallbackArgs args)
	{
		Settlement encounterSettlement = PlayerEncounter.EncounterSettlement;
		args.MenuTitle = encounterSettlement.Name;
		Campaign.Current.Models.SettlementAccessModel.CanMainHeroEnterSettlement(encounterSettlement, out _accessDetails);
		TextObject empty = TextObject.Empty;
		SettlementAccessModel.AccessLevel accessLevel = _accessDetails.AccessLevel;
		int num = (int)accessLevel;
		if (num != 0)
		{
			if (num != 1 || _accessDetails.AccessLimitationReason != SettlementAccessModel.AccessLimitationReason.CrimeRating)
			{
				empty = ((encounterSettlement.OwnerClan != Hero.MainHero.Clan) ? GameTexts.FindText("str_castle_text_1") : GameTexts.FindText("str_castle_text_yours"));
			}
			else
			{
				empty.SetTextVariable("FACTION", Settlement.CurrentSettlement.MapFaction.Name);
				empty = GameTexts.FindText("str_gate_down_criminal_text");
			}
		}
		else if (_accessDetails.AccessLimitationReason == SettlementAccessModel.AccessLimitationReason.HostileFaction)
		{
			empty = GameTexts.FindText("str_gate_down_enemy_text_castle");
		}
		else if (_accessDetails.AccessLimitationReason == SettlementAccessModel.AccessLimitationReason.CrimeRating)
		{
			empty.SetTextVariable("FACTION", Settlement.CurrentSettlement.MapFaction.Name);
			empty = GameTexts.FindText("str_gate_down_criminal_text");
		}
		else
		{
			empty = GameTexts.FindText("str_settlement_not_allowed_text");
		}
		encounterSettlement.OwnerClan.Leader.SetPropertiesToTextObject(empty, "LORD");
		empty.SetTextVariable("FACTION_TERM", encounterSettlement.MapFaction.EncyclopediaLinkWithName);
		empty.SetTextVariable("SETTLEMENT_NAME", encounterSettlement.EncyclopediaLinkWithName);
		MBTextManager.SetTextVariable("TOWN_TEXT", empty);
		if (_accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.FullAccess && (_accessDetails.AccessMethod == SettlementAccessModel.AccessMethod.Direct || (_playerIsAlreadyInCastle && _accessDetails.AccessMethod == SettlementAccessModel.AccessMethod.ByRequest)))
		{
			GameMenu.SwitchToMenu("castle");
		}
		else
		{
			_playerIsAlreadyInCastle = false;
		}
	}

	private void game_menu_army_left_settlement_due_to_war_on_consequence(MenuCallbackArgs args)
	{
		MobileParty leaderParty = MobileParty.MainParty.Army.LeaderParty;
		Settlement currentSettlement = Settlement.CurrentSettlement;
		LeaveSettlementAction.ApplyForParty(leaderParty);
		SetPartyAiAction.GetActionForPatrollingAroundSettlement(leaderParty, currentSettlement);
	}

	private void game_menu_town_outside_approach_gates_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("town_guard");
	}

	private bool game_menu_castle_outside_approach_gates_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
		return true;
	}

	private void game_menu_castle_outside_approach_gates_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("castle_guard");
	}

	private void game_menu_fortification_high_crime_rating_continue_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.Finish();
	}

	private bool outside_menu_criminal_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
		if (_accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.LimitedAccess)
		{
			return _accessDetails.AccessLimitationReason == SettlementAccessModel.AccessLimitationReason.CrimeRating;
		}
		return false;
	}

	private void outside_menu_criminal_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("town_discuss_criminal_surrender");
	}

	private void caught_outside_menu_criminal_on_consequence(MenuCallbackArgs args)
	{
		ChangeCrimeRatingAction.Apply(Settlement.CurrentSettlement.MapFaction, 10f);
		GameMenu.SwitchToMenu("town_inside_criminal");
	}

	private bool caught_outside_menu_enemy_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Surrender;
		return Hero.MainHero.MapFaction.IsAtWarWith(Settlement.CurrentSettlement.MapFaction);
	}

	private void caught_outside_menu_enemy_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("taken_prisoner");
	}

	private bool game_menu_town_disguise_yourself_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.SneakIn;
		MBTextManager.SetTextVariable("SNEAK_CHANCE", MathF.Round(Campaign.Current.Models.DisguiseDetectionModel.CalculateDisguiseDetectionProbability(Settlement.CurrentSettlement) * 100f));
		if (_accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.LimitedAccess)
		{
			return _accessDetails.LimitedAccessSolution == SettlementAccessModel.LimitedAccessSolution.Disguise;
		}
		return false;
	}

	private void game_menu_town_disguise_yourself_on_consequence(MenuCallbackArgs args)
	{
		bool num = Campaign.Current.Models.DisguiseDetectionModel.CalculateDisguiseDetectionProbability(Settlement.CurrentSettlement) > MBRandom.RandomFloat;
		SkillLevelingManager.OnMainHeroDisguised(num);
		Campaign.Current.IsMainHeroDisguised = true;
		if (num)
		{
			GameMenu.SwitchToMenu("menu_sneak_into_town_succeeded");
		}
		else
		{
			GameMenu.SwitchToMenu("menu_sneak_into_town_caught");
		}
	}

	private void game_menu_castle_town_sneak_grappling_hook_on_consequence(MenuCallbackArgs args)
	{
		bool flag = false;
		ItemObject @object = MBObjectManager.Instance.GetObject<ItemObject>("grappling_hook");
		for (int i = 0; i < PartyBase.MainParty.ItemRoster.Count; i++)
		{
			if (PartyBase.MainParty.ItemRoster.GetElementCopyAtIndex(i).EquipmentElement.Item == @object)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			Campaign.Current.GameMenuManager.SetNextMenu("town_stealth");
			PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(LocationComplex.Current.GetLocationWithId("center"));
		}
		else
		{
			MBInformationManager.AddQuickInformation(new TextObject("{=!}TODO You have not any grappling hook!"));
		}
	}

	private bool game_menu_town_sneak_grappling_hook_on_condition(MenuCallbackArgs args)
	{
		return Campaign.Current.IsNight;
	}

	private bool game_menu_town_town_besiege_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.BesiegeTown;
		CheckFortificationAttackableHonorably(args);
		if (FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, Settlement.CurrentSettlement.MapFaction) && PartyBase.MainParty.NumberOfHealthyMembers > 0)
		{
			return !Settlement.CurrentSettlement.IsUnderSiege;
		}
		return false;
	}

	private void leave_siege_after_attack_on_consequence(MenuCallbackArgs args)
	{
		MobileParty.MainParty.BesiegerCamp = null;
		GameMenu.ExitToLast();
	}

	private bool leave_siege_after_attack_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		if (MobileParty.MainParty.Army != null)
		{
			return MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty;
		}
		return true;
	}

	private bool leave_army_after_attack_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		if (MobileParty.MainParty.Army != null)
		{
			return MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty;
		}
		return false;
	}

	private void leave_army_after_attack_on_consequence(MenuCallbackArgs args)
	{
		if (PlayerEncounter.Current != null)
		{
			PlayerEncounter.Finish();
		}
		else
		{
			GameMenu.ExitToLast();
		}
		if (Settlement.CurrentSettlement != null)
		{
			LeaveSettlementAction.ApplyForParty(MobileParty.MainParty);
			PartyBase.MainParty.SetVisualAsDirty();
		}
		MobileParty.MainParty.Army = null;
	}

	private void game_menu_town_town_besiege_on_consequence(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (PlayerEncounter.Current != null)
		{
			PlayerEncounter.Finish();
		}
		Campaign.Current.SiegeEventManager.StartSiegeEvent(currentSettlement, MobileParty.MainParty);
		PlayerSiege.StartPlayerSiege(BattleSideEnum.Attacker);
		PlayerSiege.StartSiegePreparation();
	}

	private void continue_siege_after_attack_on_consequence(MenuCallbackArgs args)
	{
		PlayerSiege.StartSiegePreparation();
	}

	private bool continue_siege_after_attack_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private bool game_menu_town_outside_cheat_enter_on_condition(MenuCallbackArgs args)
	{
		return Game.Current.IsDevelopmentMode;
	}

	private void game_menu_town_outside_enter_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("town");
		PlayerEncounter.LocationEncounter.IsInsideOfASettlement = true;
	}

	private bool game_menu_castle_outside_leave_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		return true;
	}

	private void game_menu_castle_outside_leave_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.Finish();
	}

	private bool game_menu_town_guard_request_shelter_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
		if (_accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.NoAccess && _accessDetails.AccessLimitationReason == SettlementAccessModel.AccessLimitationReason.CrimeRating)
		{
			args.Tooltip = new TextObject("{=03DZpTYi}You are a wanted criminal.");
			args.IsEnabled = false;
		}
		List<Location> locations = Settlement.CurrentSettlement.LocationComplex.FindAll((string x) => x == "lordshall" || x == "prison").ToList();
		MenuHelper.SetIssueAndQuestDataForLocations(args, locations);
		return true;
	}

	private void game_menu_request_entry_to_castle_on_consequence(MenuCallbackArgs args)
	{
		_ = Settlement.CurrentSettlement;
		if (_accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.FullAccess)
		{
			_playerIsAlreadyInCastle = true;
			GameMenu.SwitchToMenu("menu_castle_entry_granted");
		}
		else
		{
			GameMenu.SwitchToMenu("menu_castle_entry_denied");
		}
	}

	private bool game_menu_request_meeting_someone_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
		List<Location> locations = Settlement.CurrentSettlement.LocationComplex.FindAll((string x) => x == "lordshall").ToList();
		MenuHelper.SetIssueAndQuestDataForLocations(args, locations);
		bool disableOption;
		TextObject disabledText;
		bool result = Campaign.Current.Models.SettlementAccessModel.IsRequestMeetingOptionAvailable(Settlement.CurrentSettlement, out disableOption, out disabledText);
		args.Tooltip = disabledText;
		args.IsEnabled = !disableOption;
		return result;
	}

	private void game_menu_request_meeting_someone_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("request_meeting");
	}

	private void game_menu_town_guard_back_on_consequence(MenuCallbackArgs args)
	{
		if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsCastle)
		{
			GameMenu.SwitchToMenu("castle_outside");
		}
		else
		{
			GameMenu.SwitchToMenu("town_outside");
		}
	}

	private void game_menu_town_menu_request_meeting_on_init(MenuCallbackArgs args)
	{
		List<Hero> heroesToMeetInTown = TownHelpers.GetHeroesToMeetInTown(Settlement.CurrentSettlement);
		args.MenuContext.SetRepeatObjectList(heroesToMeetInTown);
	}

	private bool game_menu_request_meeting_with_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
		if (args.MenuContext.GetCurrentRepeatableObject() is Hero hero)
		{
			StringHelpers.SetCharacterProperties("HERO_TO_MEET", hero.CharacterObject);
			MenuHelper.SetIssueAndQuestDataForHero(args, hero);
			return true;
		}
		return false;
	}

	private void game_menu_town_menu_request_meeting_with_besiegers_on_init(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (currentSettlement.SiegeEvent == null)
		{
			if (MobileParty.MainParty.BesiegerCamp == null)
			{
				PlayerSiege.ClosePlayerSiege();
			}
			if (currentSettlement.IsTown)
			{
				GameMenu.SwitchToMenu("town");
				return;
			}
			if (currentSettlement.IsCastle)
			{
				GameMenu.SwitchToMenu("castle");
				return;
			}
			Debug.FailedAssert("non-fortification under siege", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CampaignBehaviors\\EncounterGameMenuBehavior.cs", "game_menu_town_menu_request_meeting_with_besiegers_on_init", 2444);
		}
		List<MobileParty> list = new List<MobileParty>();
		if (currentSettlement.SiegeEvent.BesiegerCamp.LeaderParty.Army != null)
		{
			list.Add(currentSettlement.SiegeEvent.BesiegerCamp.LeaderParty.Army.LeaderParty);
		}
		else
		{
			list.Add(currentSettlement.SiegeEvent.BesiegerCamp.LeaderParty);
		}
		args.MenuContext.SetRepeatObjectList(list.AsReadOnly());
	}

	private bool game_menu_request_meeting_with_besiegers_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (currentSettlement.SiegeEvent != null)
		{
			MobileParty mobileParty = ((currentSettlement.SiegeEvent.BesiegerCamp.LeaderParty.Army != null) ? currentSettlement.SiegeEvent.BesiegerCamp.LeaderParty.Army.LeaderParty : currentSettlement.SiegeEvent.BesiegerCamp.LeaderParty);
			StringHelpers.SetCharacterProperties("PARTY_LEADER", mobileParty.LeaderHero.CharacterObject);
			return true;
		}
		return false;
	}

	private string GetMeetingScene(out string sceneLevel)
	{
		string sceneID = GameSceneDataManager.Instance.MeetingScenes.GetRandomElementWithPredicate((MeetingSceneData x) => x.Culture == Settlement.CurrentSettlement.Culture).SceneID;
		if (string.IsNullOrEmpty(sceneID))
		{
			sceneID = GameSceneDataManager.Instance.MeetingScenes.GetRandomElement().SceneID;
		}
		sceneLevel = "";
		if (Settlement.CurrentSettlement.IsFortification)
		{
			sceneLevel = Campaign.Current.Models.LocationModel.GetUpgradeLevelTag(Settlement.CurrentSettlement.Town.GetWallLevel());
		}
		return sceneID;
	}

	private void game_menu_request_meeting_with_besiegers_on_consequence(MenuCallbackArgs args)
	{
		string sceneLevel;
		string meetingScene = GetMeetingScene(out sceneLevel);
		MobileParty mobileParty = (MobileParty)args.MenuContext.GetSelectedObject();
		ConversationCharacterData playerCharacterData = new ConversationCharacterData(Hero.MainHero.CharacterObject, PartyBase.MainParty, noHorse: true);
		ConversationCharacterData conversationPartnerData = new ConversationCharacterData(ConversationHelper.GetConversationCharacterPartyLeader(mobileParty.Party), mobileParty.Party);
		CampaignMission.OpenConversationMission(playerCharacterData, conversationPartnerData, meetingScene, sceneLevel);
	}

	private void game_menu_request_meeting_with_on_consequence(MenuCallbackArgs args)
	{
		string sceneLevel;
		string meetingScene = GetMeetingScene(out sceneLevel);
		Hero hero = (Hero)args.MenuContext.GetSelectedObject();
		ConversationCharacterData playerCharacterData = new ConversationCharacterData(Hero.MainHero.CharacterObject, PartyBase.MainParty);
		ConversationCharacterData conversationPartnerData = new ConversationCharacterData(hero.CharacterObject, hero.PartyBelongedTo?.Party, noHorse: true);
		CampaignMission.OpenConversationMission(playerCharacterData, conversationPartnerData, meetingScene, sceneLevel);
	}

	private bool game_meeting_town_leave_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		return Settlement.CurrentSettlement.IsTown;
	}

	private bool game_meeting_castle_leave_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		return Settlement.CurrentSettlement.IsCastle;
	}

	private void game_menu_request_meeting_town_leave_on_consequence(MenuCallbackArgs args)
	{
		if (PlayerSiege.PlayerSiegeEvent != null && PlayerSiege.PlayerSiegeEvent.BesiegedSettlement == Settlement.CurrentSettlement)
		{
			GameMenu.ExitToLast();
			PlayerEncounter.LeaveEncounter = false;
			if (Hero.MainHero.CurrentSettlement != null && PlayerSiege.PlayerSiegeEvent == null)
			{
				PlayerEncounter.LeaveSettlement();
			}
			if (PlayerSiege.PlayerSiegeEvent.BesiegedSettlement.SiegeEvent != null)
			{
				PlayerSiege.StartSiegePreparation();
			}
			else
			{
				PlayerEncounter.Finish();
			}
		}
		else
		{
			GameMenu.SwitchToMenu("town_guard");
		}
	}

	private void game_menu_request_meeting_castle_leave_on_consequence(MenuCallbackArgs args)
	{
		if (PlayerSiege.PlayerSiegeEvent != null && PlayerSiege.PlayerSiegeEvent.BesiegedSettlement == Settlement.CurrentSettlement)
		{
			GameMenu.ExitToLast();
			PlayerEncounter.LeaveEncounter = false;
			if (Hero.MainHero.CurrentSettlement != null && PlayerSiege.PlayerSiegeEvent == null)
			{
				PlayerEncounter.LeaveSettlement();
			}
			if (PlayerSiege.PlayerSiegeEvent.BesiegedSettlement.SiegeEvent != null)
			{
				PlayerSiege.StartSiegePreparation();
			}
			else
			{
				PlayerEncounter.Finish();
			}
		}
		else
		{
			GameMenu.SwitchToMenu("castle_guard");
		}
	}

	private void game_menu_village_loot_complete_on_init(MenuCallbackArgs args)
	{
		PlayerEncounter.Update();
	}

	private void game_menu_village_loot_complete_continue_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.Finish();
	}

	private bool game_menu_village_loot_complete_continue_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private void game_menu_raid_interrupted_continue_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("encounter");
	}

	private bool game_menu_raid_interrupted_continue_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private void break_out_menu_accept_on_consequence(MenuCallbackArgs args)
	{
		BreakInOutBesiegedSettlementAction.ApplyBreakOut(out _breakInOutCasualties, out _breakInOutArmyCasualties);
		GameMenu.SwitchToMenu("break_out_debrief_menu");
	}

	private bool break_out_menu_accept_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.LeaveTroopsAndFlee;
		MobileParty mainParty = MobileParty.MainParty;
		SiegeEvent siegeEvent = Settlement.CurrentSettlement.SiegeEvent;
		int lostTroopCountForBreakingOutOfBesiegedSettlement = Campaign.Current.Models.TroopSacrificeModel.GetLostTroopCountForBreakingOutOfBesiegedSettlement(mainParty, siegeEvent);
		int num = ((mainParty.Army != null && mainParty.Army.LeaderParty == mainParty) ? mainParty.Army.TotalRegularCount : mainParty.MemberRoster.TotalRegulars);
		if (lostTroopCountForBreakingOutOfBesiegedSettlement > num)
		{
			args.IsEnabled = false;
			args.Tooltip = new TextObject("{=MTbOGRCF}You don't have enough men!");
		}
		return true;
	}

	private void break_in_menu_accept_on_consequence(MenuCallbackArgs args)
	{
		BreakInOutBesiegedSettlementAction.ApplyBreakIn(out _breakInOutCasualties, out _breakInOutArmyCasualties);
		GameMenu.SwitchToMenu("break_in_debrief_menu");
	}

	private bool break_in_menu_accept_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.LeaveTroopsAndFlee;
		MobileParty mainParty = MobileParty.MainParty;
		SiegeEvent siegeEvent = Settlement.CurrentSettlement.SiegeEvent;
		int num = ((siegeEvent != null) ? Campaign.Current.Models.TroopSacrificeModel.GetLostTroopCountForBreakingInBesiegedSettlement(mainParty, siegeEvent) : 0);
		int num2 = mainParty.Army?.TotalRegularCount ?? mainParty.MemberRoster.TotalRegulars;
		if (num > num2)
		{
			args.IsEnabled = false;
			args.Tooltip = new TextObject("{=MTbOGRCF}You don't have enough men!");
		}
		return true;
	}

	private void break_out_menu_reject_on_consequence(MenuCallbackArgs args)
	{
		if (PlayerSiege.PlayerSiegeEvent != null)
		{
			GameMenu.SwitchToMenu("menu_siege_strategies");
		}
		else
		{
			GameMenu.SwitchToMenu("encounter_interrupted_siege_preparations");
		}
	}

	private bool break_out_menu_reject_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
		return true;
	}

	private void break_in_menu_reject_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("join_siege_event");
	}

	private bool break_in_menu_reject_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
		return true;
	}

	private void break_in_out_menu_on_init(MenuCallbackArgs args)
	{
		if (PlayerEncounter.Current != null && PlayerEncounter.EncounterSettlement.Party.SiegeEvent == null)
		{
			PlayerEncounter.Finish();
			return;
		}
		MobileParty mainParty = MobileParty.MainParty;
		SiegeEvent siegeEvent = Settlement.CurrentSettlement.SiegeEvent;
		int lostTroopCountForBreakingInBesiegedSettlement = Campaign.Current.Models.TroopSacrificeModel.GetLostTroopCountForBreakingInBesiegedSettlement(mainParty, siegeEvent);
		TextObject text = args.MenuContext.GameMenu.GetText();
		text.SetTextVariable("POSSIBLE_CASUALTIES", lostTroopCountForBreakingInBesiegedSettlement);
		text.SetTextVariable("PLURAL", (lostTroopCountForBreakingInBesiegedSettlement > 1) ? 1 : 0);
	}

	private void break_in_out_debrief_menu_on_init(MenuCallbackArgs args)
	{
		if (PlayerEncounter.Current != null && PlayerEncounter.EncounterSettlement.Party.SiegeEvent == null)
		{
			PlayerEncounter.Finish();
			return;
		}
		TextObject text = args.MenuContext.GameMenu.GetText();
		text.SetTextVariable("CASUALTIES", PartyBaseHelper.PrintRegularTroopCategories(_breakInOutCasualties));
		if (_breakInOutArmyCasualties > 0)
		{
			TextObject textObject = new TextObject("{=hxnCr8bm} Other parties of your army lost {NUMBER} {?PLURAL}troops{?}troop{\\?}.");
			textObject.SetTextVariable("NUMBER", _breakInOutArmyCasualties);
			textObject.SetTextVariable("PLURAL", (_breakInOutArmyCasualties > 1) ? 1 : 0);
			text.SetTextVariable("OTHER_CASUALTIES", textObject);
		}
		else
		{
			text.SetTextVariable("OTHER_CASUALTIES", TextObject.Empty);
		}
	}

	private void break_out_debrief_continue_on_consequence(MenuCallbackArgs args)
	{
		Settlement besiegedSettlement = PlayerSiege.PlayerSiegeEvent.BesiegedSettlement;
		PlayerEncounter.Finish();
		besiegedSettlement.Party.SetVisualAsDirty();
		if (PlayerSiege.PlayerSiegeEvent != null)
		{
			PlayerSiege.ClosePlayerSiege();
		}
		PlayerEncounter.ProtectPlayerSide();
	}

	private bool break_out_debrief_continue_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private bool break_in_debrief_continue_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private void break_in_debrief_continue_on_consequence(MenuCallbackArgs args)
	{
		if (Hero.MainHero.CurrentSettlement == null)
		{
			PlayerEncounter.EnterSettlement();
		}
		if (PlayerSiege.PlayerSiegeEvent == null)
		{
			PlayerSiege.StartPlayerSiege(BattleSideEnum.Defender);
		}
		if (Hero.MainHero.CurrentSettlement.Party.MapEvent != null)
		{
			GameMenu.SwitchToMenu("join_encounter");
		}
		else
		{
			PlayerSiege.StartSiegePreparation();
		}
	}

	[GameMenuInitializationHandler("army_encounter")]
	[GameMenuInitializationHandler("game_menu_army_talk_to_other_members")]
	private static void army_encounter_background_on_init(MenuCallbackArgs args)
	{
		if (PlayerEncounter.EncounteredMobileParty != null && PlayerEncounter.EncounteredMobileParty.Army != null)
		{
			args.MenuContext.SetBackgroundMeshName(PlayerEncounter.EncounteredMobileParty.Army.Kingdom.Culture.EncounterBackgroundMesh);
		}
		else
		{
			args.MenuContext.SetBackgroundMeshName("wait_fallback");
		}
	}

	[GameMenuInitializationHandler("castle_outside")]
	[GameMenuInitializationHandler("town_outside")]
	[GameMenuInitializationHandler("fortification_crime_rating")]
	[GameMenuInitializationHandler("village_outside")]
	[GameMenuInitializationHandler("menu_sneak_into_town_succeeded")]
	private static void encounter_menu_ui_castle_on_init(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		args.MenuContext.SetBackgroundMeshName(currentSettlement.SettlementComponent.WaitMeshName);
	}

	[GameMenuInitializationHandler("menu_castle_taken")]
	[GameMenuInitializationHandler("menu_settlement_taken")]
	private static void encounter_menu_settlement_taken_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("encounter_win");
	}

	[GameMenuInitializationHandler("encounter_meeting")]
	private static void game_menu_encounter_meeting_background_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName(PlayerEncounter.EncounteredParty.MapFaction.Culture.EncounterBackgroundMesh);
	}

	[GameMenuInitializationHandler("menu_castle_entry_denied")]
	private static void game_menu_castle_guard_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("encounter_guards");
	}

	[GameMenuInitializationHandler("break_in_menu")]
	[GameMenuInitializationHandler("break_in_debrief_menu")]
	[GameMenuInitializationHandler("break_out_menu")]
	[GameMenuInitializationHandler("break_out_debrief_menu")]
	[GameMenuInitializationHandler("continue_siege_after_attack")]
	[GameMenuInitializationHandler("siege_attacker_defeated")]
	[GameMenuInitializationHandler("siege_attacker_left")]
	private static void game_menu_siege_background_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("wait_besieging");
	}
}
