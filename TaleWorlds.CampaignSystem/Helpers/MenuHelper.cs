using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Helpers;

public static class MenuHelper
{
	public static bool SetOptionProperties(MenuCallbackArgs args, bool canPlayerDo, bool shouldBeDisabled, TextObject disabledText)
	{
		if (canPlayerDo)
		{
			return true;
		}
		if (!shouldBeDisabled)
		{
			return false;
		}
		args.IsEnabled = false;
		args.Tooltip = disabledText;
		return true;
	}

	public static void SetIssueAndQuestDataForHero(MenuCallbackArgs args, Hero hero)
	{
		if (hero.Issue != null && hero.Issue.IssueQuest == null)
		{
			args.OptionQuestData |= GameMenuOption.IssueQuestFlags.AvailableIssue;
		}
		Campaign.Current.QuestManager.TrackedObjects.TryGetValue(hero, out var value);
		if (value != null)
		{
			for (int i = 0; i < value.Count; i++)
			{
				if (!value[i].IsTrackEnabled)
				{
					continue;
				}
				if (value[i].IsSpecialQuest)
				{
					if ((args.OptionQuestData & GameMenuOption.IssueQuestFlags.TrackedStoryQuest) == 0 && value[i].QuestGiver != hero)
					{
						args.OptionQuestData |= GameMenuOption.IssueQuestFlags.TrackedStoryQuest;
					}
					else if ((args.OptionQuestData & GameMenuOption.IssueQuestFlags.ActiveStoryQuest) == 0 && value[i].QuestGiver == hero)
					{
						args.OptionQuestData |= GameMenuOption.IssueQuestFlags.ActiveStoryQuest;
					}
				}
				else if ((args.OptionQuestData & GameMenuOption.IssueQuestFlags.TrackedIssue) == 0 && value[i].QuestGiver != hero)
				{
					args.OptionQuestData |= GameMenuOption.IssueQuestFlags.TrackedIssue;
				}
				else if ((args.OptionQuestData & GameMenuOption.IssueQuestFlags.ActiveIssue) == 0 && value[i].QuestGiver == hero)
				{
					args.OptionQuestData |= GameMenuOption.IssueQuestFlags.ActiveIssue;
				}
			}
		}
		if (hero.PartyBelongedTo != null && ((args.OptionQuestData & GameMenuOption.IssueQuestFlags.ActiveStoryQuest) == 0 || (args.OptionQuestData & GameMenuOption.IssueQuestFlags.ActiveIssue) == 0 || (args.OptionQuestData & GameMenuOption.IssueQuestFlags.TrackedIssue) == 0 || (args.OptionQuestData & GameMenuOption.IssueQuestFlags.TrackedStoryQuest) == 0))
		{
			Campaign.Current.QuestManager.TrackedObjects.TryGetValue(hero.PartyBelongedTo, out var value2);
			if (value2 != null)
			{
				for (int j = 0; j < value2.Count; j++)
				{
					if (!value2[j].IsTrackEnabled)
					{
						continue;
					}
					if (value2[j].IsSpecialQuest)
					{
						if ((args.OptionQuestData & GameMenuOption.IssueQuestFlags.TrackedStoryQuest) == 0 && value2[j].QuestGiver != hero)
						{
							args.OptionQuestData |= GameMenuOption.IssueQuestFlags.TrackedStoryQuest;
						}
						else if ((args.OptionQuestData & GameMenuOption.IssueQuestFlags.ActiveStoryQuest) == 0 && value2[j].QuestGiver == hero)
						{
							args.OptionQuestData |= GameMenuOption.IssueQuestFlags.ActiveStoryQuest;
						}
					}
					else if ((args.OptionQuestData & GameMenuOption.IssueQuestFlags.TrackedIssue) == 0 && value2[j].QuestGiver != hero)
					{
						args.OptionQuestData |= GameMenuOption.IssueQuestFlags.TrackedIssue;
					}
					else if ((args.OptionQuestData & GameMenuOption.IssueQuestFlags.ActiveIssue) == 0 && value2[j].QuestGiver == hero)
					{
						args.OptionQuestData |= GameMenuOption.IssueQuestFlags.ActiveIssue;
					}
				}
			}
		}
		if ((args.OptionQuestData & GameMenuOption.IssueQuestFlags.ActiveIssue) == 0 && hero.Issue?.IssueQuest != null && hero.Issue.IssueQuest.IsTrackEnabled)
		{
			args.OptionQuestData |= GameMenuOption.IssueQuestFlags.ActiveIssue;
		}
	}

	public static void SetIssueAndQuestDataForLocations(MenuCallbackArgs args, List<Location> locations)
	{
		GameMenuOption.IssueQuestFlags issueQuestFlags = Campaign.Current.IssueManager.CheckIssueForMenuLocations(locations, getIssuesWithoutAQuest: true);
		args.OptionQuestData |= issueQuestFlags;
		args.OptionQuestData |= Campaign.Current.QuestManager.CheckQuestForMenuLocations(locations);
	}

	public static void DecideMenuState()
	{
		string genericStateMenu = Campaign.Current.Models.EncounterGameMenuModel.GetGenericStateMenu();
		if (!string.IsNullOrEmpty(genericStateMenu))
		{
			GameMenu.SwitchToMenu(genericStateMenu);
		}
		else
		{
			GameMenu.ExitToLast();
		}
	}

	public static bool EncounterAttackCondition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
		if (MapEvent.PlayerMapEvent == null)
		{
			return false;
		}
		MapEvent battle = PlayerEncounter.Battle;
		Settlement settlement = battle?.MapEventSettlement;
		if (battle != null && settlement != null && settlement.IsFortification && battle.IsSiegeAssault && PlayerSiege.PlayerSiegeEvent != null && !PlayerSiege.PlayerSiegeEvent.BesiegerCamp.IsPreparationComplete)
		{
			return false;
		}
		bool result = battle != null && (battle.HasTroopsOnBothSides() || battle.IsSiegeAssault) && MapEvent.PlayerMapEvent.GetLeaderParty(PartyBase.MainParty.OpponentSide) != null;
		if (Hero.MainHero.IsWounded)
		{
			args.Tooltip = new TextObject("{=UL8za0AO}You are wounded.");
			args.IsEnabled = false;
		}
		return result;
	}

	public static bool EncounterCaptureEnemyCondition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Surrender;
		MapEvent battle = PlayerEncounter.Battle;
		return battle?.PartiesOnSide(battle.GetOtherSide(battle.PlayerSide)).All((MapEventParty party) => !party.Party.IsSettlement && party.Party.NumberOfHealthyMembers == 0) ?? false;
	}

	public static void EncounterAttackConsequence(MenuCallbackArgs args)
	{
		MapEvent battle = PlayerEncounter.Battle;
		PartyBase leaderParty = battle.GetLeaderParty(PartyBase.MainParty.OpponentSide);
		BeHostileAction.ApplyEncounterHostileAction(PartyBase.MainParty, leaderParty);
		if (PlayerEncounter.Current == null)
		{
			return;
		}
		Settlement mapEventSettlement = MobileParty.MainParty.MapEvent.MapEventSettlement;
		if (mapEventSettlement != null && !battle.IsSallyOut && !battle.IsSiegeOutside)
		{
			if (mapEventSettlement.IsFortification)
			{
				if (battle.IsRaid)
				{
					PlayerEncounter.StartVillageBattleMission();
				}
				else if (battle.IsSiegeAmbush)
				{
					PlayerEncounter.StartSiegeAmbushMission();
				}
				else if (battle.IsSiegeAssault)
				{
					if (PlayerSiege.PlayerSiegeEvent == null && PartyBase.MainParty.Side == BattleSideEnum.Attacker)
					{
						PlayerSiege.StartPlayerSiege(MobileParty.MainParty.Party.Side, isSimulation: false, mapEventSettlement);
					}
					else
					{
						if (PlayerSiege.PlayerSiegeEvent != null && !PlayerSiege.PlayerSiegeEvent.GetSiegeEventSide(PlayerSiege.PlayerSide.GetOppositeSide()).GetInvolvedPartiesForEventType().Any((PartyBase party) => party.NumberOfHealthyMembers > 0))
						{
							PlayerEncounter.Update();
							return;
						}
						if (PlayerSiege.BesiegedSettlement != null && PlayerSiege.BesiegedSettlement.CurrentSiegeState == Settlement.SiegeState.InTheLordsHall)
						{
							FlattenedTroopRoster priorityListForLordsHallFightMission = Campaign.Current.Models.SiegeLordsHallFightModel.GetPriorityListForLordsHallFightMission(MapEvent.PlayerMapEvent, BattleSideEnum.Defender, Campaign.Current.Models.SiegeLordsHallFightModel.MaxDefenderSideTroopCount);
							int num = MathF.Max(1, MathF.Min(Campaign.Current.Models.SiegeLordsHallFightModel.MaxAttackerSideTroopCount, MathF.Round((float)priorityListForLordsHallFightMission.Troops.Count() * Campaign.Current.Models.SiegeLordsHallFightModel.AttackerDefenderTroopCountRatio)));
							TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
							MobileParty mobileParty = ((MobileParty.MainParty.Army != null) ? MobileParty.MainParty.Army.LeaderParty : MobileParty.MainParty);
							troopRoster.Add(mobileParty.MemberRoster);
							foreach (MobileParty attachedParty in mobileParty.AttachedParties)
							{
								troopRoster.Add(attachedParty.MemberRoster);
							}
							TroopRoster troopRoster2 = TroopRoster.CreateDummyTroopRoster();
							FlattenedTroopRoster flattenedTroopRoster = troopRoster.ToFlattenedRoster();
							flattenedTroopRoster.RemoveIf((FlattenedTroopRosterElement x) => x.IsWounded);
							troopRoster2.Add(MobilePartyHelper.GetStrongestAndPriorTroops(flattenedTroopRoster, num, includePlayer: true));
							args.MenuContext.OpenTroopSelection(troopRoster, troopRoster2, (CharacterObject character) => !character.IsPlayerCharacter, LordsHallTroopRosterManageDone, num, num);
						}
						else
						{
							PlayerSiege.StartSiegeMission(mapEventSettlement);
						}
					}
				}
			}
			else if (mapEventSettlement.IsVillage)
			{
				PlayerEncounter.StartVillageBattleMission();
			}
			else if (mapEventSettlement.IsHideout)
			{
				CampaignMission.OpenHideoutBattleMission("sea_bandit_a", null);
			}
		}
		else
		{
			MapPatchData mapPatchAtPosition = Campaign.Current.MapSceneWrapper.GetMapPatchAtPosition(MobileParty.MainParty.Position2D);
			string battleSceneForMapPatch = PlayerEncounter.GetBattleSceneForMapPatch(mapPatchAtPosition);
			MissionInitializerRecord rec = new MissionInitializerRecord(battleSceneForMapPatch);
			rec.TerrainType = (int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace);
			rec.DamageToPlayerMultiplier = Campaign.Current.Models.DifficultyModel.GetDamageToPlayerMultiplier();
			rec.DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
			rec.DamageFromPlayerToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
			rec.NeedsRandomTerrain = false;
			rec.PlayingInCampaignMode = true;
			rec.RandomTerrainSeed = MBRandom.RandomInt(10000);
			rec.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.GetLogicalPosition());
			rec.SceneHasMapPatch = true;
			rec.DecalAtlasGroup = 2;
			rec.PatchCoordinates = mapPatchAtPosition.normalizedCoordinates;
			rec.PatchEncounterDir = (battle.AttackerSide.LeaderParty.Position2D - battle.DefenderSide.LeaderParty.Position2D).Normalized();
			float timeOfDay = Campaign.CurrentTime % 24f;
			if (Campaign.Current != null)
			{
				rec.TimeOfDay = timeOfDay;
			}
			bool flag = MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty involvedParty) => involvedParty.Party.IsMobile && involvedParty.Party.MobileParty.IsCaravan);
			bool flag2 = MapEvent.PlayerMapEvent.MapEventSettlement == null && MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty involvedParty) => involvedParty.Party.IsMobile && involvedParty.Party.MobileParty.IsVillager);
			if (flag || flag2)
			{
				CampaignMission.OpenCaravanBattleMission(rec, flag);
			}
			else
			{
				CampaignMission.OpenBattleMission(rec);
			}
		}
		PlayerEncounter.StartAttackMission();
		MapEvent.PlayerMapEvent.BeginWait();
	}

	private static void LordsHallTroopRosterManageDone(TroopRoster selectedTroops)
	{
		MapEvent.PlayerMapEvent.ResetBattleState();
		int wallLevel = PlayerSiege.BesiegedSettlement.Town.GetWallLevel();
		CampaignMission.OpenSiegeLordsHallFightMission(PlayerSiege.BesiegedSettlement.LocationComplex.GetLocationWithId("lordshall").GetSceneName(wallLevel), selectedTroops.ToFlattenedRoster());
	}

	private static void LordsHallTroopRosterManageDoneForSimulation(TroopRoster selectedTroops)
	{
		EncounterOrderAttack(selectedTroops);
	}

	private static void EncounterOrderAttack(TroopRoster selectedTroopsForPlayerSide)
	{
		MapEvent battle = PlayerEncounter.Battle;
		if (PlayerSiege.PlayerSiegeEvent != null)
		{
			ISiegeEventSide siegeEventSide = PlayerSiege.PlayerSiegeEvent.GetSiegeEventSide(PlayerSiege.PlayerSide.GetOppositeSide());
			if (siegeEventSide != null && !siegeEventSide.GetInvolvedPartiesForEventType().Any((PartyBase party) => party.NumberOfHealthyMembers > 0) && (battle == null || !battle.GetMapEventSide(battle.GetOtherSide(battle.PlayerSide)).Parties.Any((MapEventParty party) => party.Party.NumberOfHealthyMembers > 0)))
			{
				PlayerEncounter.Update();
				return;
			}
		}
		PartyBase leaderParty = battle.GetLeaderParty(PartyBase.MainParty.OpponentSide);
		MobileParty mobileParty = MobileParty.MainParty.AttachedTo ?? MobileParty.MainParty;
		if (leaderParty.SiegeEvent?.BesiegerCamp != null && !leaderParty.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(leaderParty) && mobileParty.BesiegerCamp == null)
		{
			mobileParty.BesiegerCamp = leaderParty.SiegeEvent.BesiegerCamp;
		}
		BeHostileAction.ApplyEncounterHostileAction(PartyBase.MainParty, leaderParty);
		if (PlayerEncounter.Current != null)
		{
			GameMenu.ExitToLast();
			if (selectedTroopsForPlayerSide != null && PlayerSiege.BesiegedSettlement != null && PlayerSiege.BesiegedSettlement.CurrentSiegeState == Settlement.SiegeState.InTheLordsHall)
			{
				FlattenedTroopRoster priorityListForLordsHallFightMission = Campaign.Current.Models.SiegeLordsHallFightModel.GetPriorityListForLordsHallFightMission(MapEvent.PlayerMapEvent, BattleSideEnum.Defender, Campaign.Current.Models.SiegeLordsHallFightModel.MaxDefenderSideTroopCount);
				PlayerEncounter.InitSimulation(selectedTroopsForPlayerSide.ToFlattenedRoster(), priorityListForLordsHallFightMission);
			}
			else
			{
				PlayerEncounter.InitSimulation(null, null);
			}
			if (PlayerEncounter.Current != null && PlayerEncounter.Current.BattleSimulation != null)
			{
				((MapState)Game.Current.GameStateManager.ActiveState).StartBattleSimulation();
			}
		}
	}

	public static void EncounterOrderAttackConsequence(MenuCallbackArgs args)
	{
		if (PlayerSiege.BesiegedSettlement != null && PlayerSiege.BesiegedSettlement.CurrentSiegeState == Settlement.SiegeState.InTheLordsHall)
		{
			FlattenedTroopRoster priorityListForLordsHallFightMission = Campaign.Current.Models.SiegeLordsHallFightModel.GetPriorityListForLordsHallFightMission(MapEvent.PlayerMapEvent, BattleSideEnum.Defender, Campaign.Current.Models.SiegeLordsHallFightModel.MaxDefenderSideTroopCount);
			int num = MathF.Max(1, MathF.Min(Campaign.Current.Models.SiegeLordsHallFightModel.MaxAttackerSideTroopCount, MathF.Round((float)priorityListForLordsHallFightMission.Troops.Count() * Campaign.Current.Models.SiegeLordsHallFightModel.AttackerDefenderTroopCountRatio)));
			TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
			MobileParty mobileParty = ((MobileParty.MainParty.Army != null) ? MobileParty.MainParty.Army.LeaderParty : MobileParty.MainParty);
			troopRoster.Add(mobileParty.MemberRoster);
			foreach (MobileParty attachedParty in mobileParty.AttachedParties)
			{
				troopRoster.Add(attachedParty.MemberRoster);
			}
			TroopRoster troopRoster2 = TroopRoster.CreateDummyTroopRoster();
			FlattenedTroopRoster flattenedTroopRoster = troopRoster.ToFlattenedRoster();
			flattenedTroopRoster.RemoveIf((FlattenedTroopRosterElement x) => x.IsWounded);
			troopRoster2.Add(MobilePartyHelper.GetStrongestAndPriorTroops(flattenedTroopRoster, num, includePlayer: false));
			args.MenuContext.OpenTroopSelection(troopRoster, troopRoster2, (CharacterObject character) => !character.IsPlayerCharacter, LordsHallTroopRosterManageDoneForSimulation, num, num);
		}
		else
		{
			EncounterOrderAttack(null);
		}
	}

	public static void EncounterCaptureTheEnemyOnConsequence(MenuCallbackArgs args)
	{
		MapEvent.PlayerMapEvent.SetOverrideWinner(MapEvent.PlayerMapEvent.PlayerSide);
		PlayerEncounter.Update();
	}

	public static void EncounterLeaveConsequence()
	{
		Settlement currentSettlement = MobileParty.MainParty.CurrentSettlement;
		MapEvent mapEvent = ((PlayerEncounter.Battle != null) ? PlayerEncounter.Battle : PlayerEncounter.EncounteredBattle);
		int numberOfInvolvedMen = mapEvent.GetNumberOfInvolvedMen(PartyBase.MainParty.Side);
		PlayerEncounter.Finish(MobileParty.MainParty.CurrentSettlement?.SiegeEvent == null || MobileParty.MainParty.CurrentSettlement?.MapFaction != MobileParty.MainParty.MapFaction);
		if (MobileParty.MainParty.BesiegerCamp != null)
		{
			MobileParty.MainParty.BesiegerCamp = null;
		}
		if (mapEvent != null && !mapEvent.IsRaid && numberOfInvolvedMen == PartyBase.MainParty.NumberOfHealthyMembers)
		{
			mapEvent.SimulateBattleSetup(PlayerEncounter.Current?.BattleSimulation?.SelectedTroops);
			mapEvent.SimulateBattleForRounds((PartyBase.MainParty.Side == BattleSideEnum.Attacker) ? 1 : 0, (PartyBase.MainParty.Side != BattleSideEnum.Attacker) ? 1 : 0);
		}
		if (currentSettlement != null)
		{
			EncounterManager.StartSettlementEncounter(MobileParty.MainParty, currentSettlement);
		}
	}
}
