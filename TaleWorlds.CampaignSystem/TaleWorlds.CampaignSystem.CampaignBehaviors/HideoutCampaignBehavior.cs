using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class HideoutCampaignBehavior : CampaignBehaviorBase
{
	private const int MaxDistanceSquaredBetweenHideoutAndBoundVillage = 1600;

	private const int HideoutClearRelationEffect = 2;

	private readonly int CanAttackHideoutStart = 23;

	private readonly int CanAttackHideoutEnd = 2;

	private float _hideoutWaitProgressHours;

	private float _hideoutWaitTargetHours;

	public void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
	{
		AddGameMenus(campaignGameStarter);
	}

	public void OnGameLoaded(CampaignGameStarter campaignGameStarter)
	{
		AddGameMenus(campaignGameStarter);
	}

	public override void RegisterEvents()
	{
		CampaignEvents.HourlyTickSettlementEvent.AddNonSerializedListener(this, HourlyTickSettlement);
		CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
		CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
		CampaignEvents.OnHideoutSpottedEvent.AddNonSerializedListener(this, OnHideoutSpotted);
	}

	private void OnHideoutSpotted(PartyBase party, PartyBase hideout)
	{
		SkillLevelingManager.OnHideoutSpotted(party.MobileParty, hideout);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_hideoutWaitProgressHours", ref _hideoutWaitProgressHours);
		dataStore.SyncData("_hideoutWaitTargetHours", ref _hideoutWaitTargetHours);
	}

	public void HourlyTickSettlement(Settlement settlement)
	{
		if (settlement.IsHideout && settlement.Hideout.IsInfested && !settlement.Hideout.IsSpotted)
		{
			float hideoutSpottingDistance = Campaign.Current.Models.MapVisibilityModel.GetHideoutSpottingDistance();
			float num = MobileParty.MainParty.Position2D.DistanceSquared(settlement.Position2D);
			float num2 = 1f - num / (hideoutSpottingDistance * hideoutSpottingDistance);
			if (num2 > 0f && settlement.Parties.Count > 0 && MBRandom.RandomFloat < num2 && !settlement.Hideout.IsSpotted)
			{
				settlement.Hideout.IsSpotted = true;
				settlement.IsVisible = true;
				CampaignEventDispatcher.Instance.OnHideoutSpotted(MobileParty.MainParty.Party, settlement.Party);
			}
		}
	}

	protected void AddGameMenus(CampaignGameStarter campaignGameStarter)
	{
		campaignGameStarter.AddGameMenu("hideout_place", "{=!}{HIDEOUT_TEXT}", game_menu_hideout_place_on_init);
		campaignGameStarter.AddGameMenuOption("hideout_place", "wait", "{=4Sb0d8FY}Wait until nightfall to attack", game_menu_wait_until_nightfall_on_condition, game_menu_wait_until_nightfall_on_consequence);
		campaignGameStarter.AddGameMenuOption("hideout_place", "attack", "{=zxMOqlhs}Attack", game_menu_attack_hideout_parties_on_condition, game_menu_encounter_attack_on_consequence);
		campaignGameStarter.AddGameMenuOption("hideout_place", "leave", "{=3sRdGQou}Leave", leave_on_condition, game_menu_hideout_leave_on_consequence, isLeave: true);
		campaignGameStarter.AddWaitGameMenu("hideout_wait", "{=VLLAOXve}Waiting until nightfall to ambush", null, hideout_wait_menu_on_condition, hideout_wait_menu_on_consequence, hideout_wait_menu_on_tick, GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption, GameOverlays.MenuOverlayType.None, _hideoutWaitTargetHours);
		campaignGameStarter.AddGameMenuOption("hideout_wait", "leave", "{=3sRdGQou}Leave", leave_on_condition, game_menu_hideout_leave_on_consequence, isLeave: true);
		campaignGameStarter.AddGameMenu("hideout_after_wait", "{=!}{HIDEOUT_TEXT}", hideout_after_wait_menu_on_init);
		campaignGameStarter.AddGameMenuOption("hideout_after_wait", "attack", "{=zxMOqlhs}Attack", game_menu_attack_hideout_parties_on_condition, game_menu_encounter_attack_on_consequence);
		campaignGameStarter.AddGameMenuOption("hideout_after_wait", "leave", "{=3sRdGQou}Leave", leave_on_condition, game_menu_hideout_leave_on_consequence, isLeave: true);
		campaignGameStarter.AddGameMenu("hideout_after_defeated_and_saved", "{=1zLZf5rw}The rest of your men rushed to your help, dragging you out to safety and driving the bandits back into hiding.", game_menu_hideout_after_defeated_and_saved_on_init);
		campaignGameStarter.AddGameMenuOption("hideout_after_defeated_and_saved", "leave", "{=3sRdGQou}Leave", leave_on_condition, game_menu_hideout_leave_on_consequence, isLeave: true);
	}

	private bool IsHideoutAttackableNow()
	{
		float currentHourInDay = CampaignTime.Now.CurrentHourInDay;
		if (CanAttackHideoutStart <= CanAttackHideoutEnd || (!(currentHourInDay >= (float)CanAttackHideoutStart) && !(currentHourInDay <= (float)CanAttackHideoutEnd)))
		{
			if (CanAttackHideoutStart < CanAttackHideoutEnd)
			{
				if (currentHourInDay >= (float)CanAttackHideoutStart)
				{
					return currentHourInDay <= (float)CanAttackHideoutEnd;
				}
				return false;
			}
			return false;
		}
		return true;
	}

	public bool hideout_wait_menu_on_condition(MenuCallbackArgs args)
	{
		return true;
	}

	public void hideout_wait_menu_on_tick(MenuCallbackArgs args, CampaignTime campaignTime)
	{
		_hideoutWaitProgressHours += (float)campaignTime.ToHours;
		if (_hideoutWaitTargetHours.ApproximatelyEqualsTo(0f))
		{
			CalculateHideoutAttackTime();
		}
		args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_hideoutWaitProgressHours / _hideoutWaitTargetHours);
	}

	public void hideout_wait_menu_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("hideout_after_wait");
	}

	private bool leave_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Leave;
		return true;
	}

	[GameMenuInitializationHandler("hideout_wait")]
	[GameMenuInitializationHandler("hideout_after_wait")]
	[GameMenuInitializationHandler("hideout_after_defeated_and_saved")]
	private static void game_menu_hideout_ui_place_on_init(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		args.MenuContext.SetBackgroundMeshName(currentSettlement.Hideout.WaitMeshName);
	}

	[GameMenuInitializationHandler("hideout_place")]
	private static void game_menu_hideout_sound_place_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetPanelSound("event:/ui/panels/settlement_hideout");
		Settlement currentSettlement = Settlement.CurrentSettlement;
		args.MenuContext.SetBackgroundMeshName(currentSettlement.Hideout.WaitMeshName);
	}

	private void game_menu_hideout_after_defeated_and_saved_on_init(MenuCallbackArgs args)
	{
		if (Settlement.CurrentSettlement.IsHideout && MobileParty.MainParty.CurrentSettlement == null)
		{
			PlayerEncounter.EnterSettlement();
		}
	}

	private void game_menu_hideout_place_on_init(MenuCallbackArgs args)
	{
		if (!Settlement.CurrentSettlement.IsHideout)
		{
			return;
		}
		_hideoutWaitProgressHours = 0f;
		if (!IsHideoutAttackableNow())
		{
			CalculateHideoutAttackTime();
		}
		else
		{
			_hideoutWaitTargetHours = 0f;
		}
		Settlement currentSettlement = Settlement.CurrentSettlement;
		int num = 0;
		foreach (MobileParty party in currentSettlement.Parties)
		{
			num += party.MemberRoster.TotalManCount - party.MemberRoster.TotalWounded;
		}
		GameTexts.SetVariable("HIDEOUT_DESCRIPTION", "{=DOmb81Mu}(Undefined hideout type)");
		if (currentSettlement.Culture.StringId.Equals("forest_bandits"))
		{
			GameTexts.SetVariable("HIDEOUT_DESCRIPTION", "{=cu2cLT5r}You spy though the trees what seems to be a clearing in the forest with what appears to be the outlines of a camp.");
		}
		if (currentSettlement.Culture.StringId.Equals("sea_raiders"))
		{
			GameTexts.SetVariable("HIDEOUT_DESCRIPTION", "{=bJ6ygV3P}As you travel along the coast, you see a sheltered cove with what appears to the outlines of a camp.");
		}
		if (currentSettlement.Culture.StringId.Equals("mountain_bandits"))
		{
			GameTexts.SetVariable("HIDEOUT_DESCRIPTION", "{=iyWUDSm8}Passing by the slopes of the mountains, you see an outcrop crowned with the ruins of an ancient fortress.");
		}
		if (currentSettlement.Culture.StringId.Equals("desert_bandits"))
		{
			GameTexts.SetVariable("HIDEOUT_DESCRIPTION", "{=b3iBOVXN}Passing by a wadi, you see what looks like a camouflaged well to tap the groundwater left behind by rare rainfalls.");
		}
		if (currentSettlement.Culture.StringId.Equals("steppe_bandits"))
		{
			GameTexts.SetVariable("HIDEOUT_DESCRIPTION", "{=5JaGVr0U}While traveling by a low range of hills, you see what appears to be the remains of a campsite in a stream gully.");
		}
		bool num2 = !currentSettlement.Hideout.NextPossibleAttackTime.IsPast;
		if (num2)
		{
			GameTexts.SetVariable("HIDEOUT_TEXT", "{=KLWn6yZQ}{HIDEOUT_DESCRIPTION} The remains of a fire suggest that it's been recently occupied, but its residents - whoever they are - are well-hidden for now.");
		}
		else if (num > 0)
		{
			GameTexts.SetVariable("HIDEOUT_TEXT", "{=prcBBqMR}{HIDEOUT_DESCRIPTION} You see armed men moving about. As you listen quietly, you hear scraps of conversation about raids, ransoms, and the best places to waylay travellers.");
		}
		else
		{
			GameTexts.SetVariable("HIDEOUT_TEXT", "{=gywyEgZa}{HIDEOUT_DESCRIPTION} There seems to be no one inside.");
		}
		if (!num2 && num > 0 && Hero.MainHero.IsWounded)
		{
			GameTexts.SetVariable("HIDEOUT_TEXT", "{=fMekM2UH}{HIDEOUT_DESCRIPTION} You can not attack since your wounds do not allow you.");
		}
		if (MobileParty.MainParty.CurrentSettlement == null)
		{
			PlayerEncounter.EnterSettlement();
		}
		_ = Settlement.CurrentSettlement.Hideout.IsInfested;
		Settlement settlement = (Settlement.CurrentSettlement.IsHideout ? Settlement.CurrentSettlement : null);
		if (PlayerEncounter.Battle != null)
		{
			bool num3 = PlayerEncounter.Battle.WinningSide == PlayerEncounter.Current.PlayerSide;
			PlayerEncounter.Update();
			if (num3 && PlayerEncounter.Battle == null && settlement != null)
			{
				SetCleanHideoutRelations(settlement);
				settlement = null;
			}
		}
	}

	private void CalculateHideoutAttackTime()
	{
		_hideoutWaitTargetHours = (((float)CanAttackHideoutStart > CampaignTime.Now.CurrentHourInDay) ? ((float)CanAttackHideoutStart - CampaignTime.Now.CurrentHourInDay) : (24f - CampaignTime.Now.CurrentHourInDay + (float)CanAttackHideoutStart));
	}

	private void SetCleanHideoutRelations(Settlement hideout)
	{
		List<Settlement> list = new List<Settlement>();
		foreach (Village allVillage in Campaign.Current.AllVillages)
		{
			if (allVillage.Settlement.Position2D.DistanceSquared(hideout.Position2D) <= 1600f)
			{
				list.Add(allVillage.Settlement);
			}
		}
		foreach (Settlement item in list)
		{
			if (item.Notables.Count > 0)
			{
				ChangeRelationAction.ApplyPlayerRelation(item.Notables.GetRandomElement(), 2, affectRelatives: true, showQuickNotification: false);
			}
		}
		if (Hero.MainHero.GetPerkValue(DefaultPerks.Charm.EffortForThePeople))
		{
			Town town = SettlementHelper.FindNearestTown(null, hideout).Town;
			Hero leader = town.OwnerClan.Leader;
			if (leader == Hero.MainHero)
			{
				town.Loyalty += 1f;
			}
			else
			{
				ChangeRelationAction.ApplyPlayerRelation(leader, (int)DefaultPerks.Charm.EffortForThePeople.PrimaryBonus);
			}
		}
		MBTextManager.SetTextVariable("RELATION_VALUE", (int)DefaultPerks.Charm.EffortForThePeople.PrimaryBonus);
		MBInformationManager.AddQuickInformation(new TextObject("{=o0qwDa0q}Your relation increased by {RELATION_VALUE} with nearby notables."));
	}

	private void hideout_after_wait_menu_on_init(MenuCallbackArgs args)
	{
		TextObject textObject = new TextObject("{=VbU8Ue0O}After waiting for a while you find a good opportunity to close in undetected beneath the shroud of the night.");
		if (Hero.MainHero.IsWounded)
		{
			TextObject textObject2 = new TextObject("{=fMekM2UH}{HIDEOUT_DESCRIPTION}. You can not attack since your wounds do not allow you.");
			textObject2.SetTextVariable("HIDEOUT_DESCRIPTION", textObject);
			MBTextManager.SetTextVariable("HIDEOUT_TEXT", textObject2);
		}
		else
		{
			MBTextManager.SetTextVariable("HIDEOUT_TEXT", textObject);
		}
	}

	private bool game_menu_attack_hideout_parties_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
		Hideout hideout = Settlement.CurrentSettlement.Hideout;
		if (!Hero.MainHero.IsWounded && Settlement.CurrentSettlement.MapFaction != PartyBase.MainParty.MapFaction && Settlement.CurrentSettlement.Parties.Any((MobileParty x) => x.IsBandit) && hideout.NextPossibleAttackTime.IsPast)
		{
			return IsHideoutAttackableNow();
		}
		return false;
	}

	private void game_menu_encounter_attack_on_consequence(MenuCallbackArgs args)
	{
		int playerMaximumTroopCountForHideoutMission = Campaign.Current.Models.BanditDensityModel.GetPlayerMaximumTroopCountForHideoutMission(MobileParty.MainParty);
		TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
		TroopRoster strongestAndPriorTroops = MobilePartyHelper.GetStrongestAndPriorTroops(MobileParty.MainParty, playerMaximumTroopCountForHideoutMission, includePlayer: true);
		troopRoster.Add(strongestAndPriorTroops);
		int maxSelectableTroopCount = Campaign.Current?.Models.BanditDensityModel.GetPlayerMaximumTroopCountForHideoutMission(MobileParty.MainParty) ?? 0;
		args.MenuContext.OpenTroopSelection(MobileParty.MainParty.MemberRoster, troopRoster, CanChangeStatusOfTroop, OnTroopRosterManageDone, maxSelectableTroopCount);
	}

	private void ArrangeHideoutTroopCountsForMission()
	{
		int numberOfMinimumBanditTroopsInHideoutMission = Campaign.Current.Models.BanditDensityModel.NumberOfMinimumBanditTroopsInHideoutMission;
		int num = Campaign.Current.Models.BanditDensityModel.NumberOfMaximumTroopCountForFirstFightInHideout + Campaign.Current.Models.BanditDensityModel.NumberOfMaximumTroopCountForBossFightInHideout;
		MBList<MobileParty> mBList = Settlement.CurrentSettlement.Parties.Where((MobileParty x) => x.IsBandit || x.IsBanditBossParty).ToMBList();
		int num2 = mBList.Sum((MobileParty x) => x.MemberRoster.TotalHealthyCount);
		if (num2 > num)
		{
			int num3 = num2 - num;
			mBList.RemoveAll((MobileParty x) => x.IsBanditBossParty || x.MemberRoster.TotalHealthyCount == 1);
			while (num3 > 0 && mBList.Count > 0)
			{
				MobileParty randomElement = mBList.GetRandomElement();
				MBList<TroopRosterElement> troopRoster = randomElement.MemberRoster.GetTroopRoster();
				List<(TroopRosterElement, float)> list = new List<(TroopRosterElement, float)>();
				foreach (TroopRosterElement item in troopRoster)
				{
					list.Add((item, item.Number - item.WoundedNumber));
				}
				TroopRosterElement troopRosterElement = MBRandom.ChooseWeighted(list);
				randomElement.MemberRoster.AddToCounts(troopRosterElement.Character, -1);
				num3--;
				if (randomElement.MemberRoster.TotalHealthyCount == 1)
				{
					mBList.Remove(randomElement);
				}
			}
		}
		else
		{
			if (num2 >= numberOfMinimumBanditTroopsInHideoutMission)
			{
				return;
			}
			int num4 = numberOfMinimumBanditTroopsInHideoutMission - num2;
			mBList.RemoveAll((MobileParty x) => x.MemberRoster.GetTroopRoster().All((TroopRosterElement y) => y.Number == 0 || y.Character.Culture.BanditBoss == y.Character || y.Character.IsHero));
			while (num4 > 0 && mBList.Count > 0)
			{
				MobileParty randomElement2 = mBList.GetRandomElement();
				MBList<TroopRosterElement> troopRoster2 = randomElement2.MemberRoster.GetTroopRoster();
				List<(TroopRosterElement, float)> list2 = new List<(TroopRosterElement, float)>();
				foreach (TroopRosterElement item2 in troopRoster2)
				{
					list2.Add((item2, item2.Number * ((item2.Character.Culture.BanditBoss != item2.Character && !item2.Character.IsHero) ? 1 : 0)));
				}
				TroopRosterElement troopRosterElement2 = MBRandom.ChooseWeighted(list2);
				randomElement2.MemberRoster.AddToCounts(troopRosterElement2.Character, 1);
				num4--;
			}
		}
	}

	private void OnTroopRosterManageDone(TroopRoster hideoutTroops)
	{
		ArrangeHideoutTroopCountsForMission();
		GameMenu.SwitchToMenu("hideout_place");
		Settlement.CurrentSettlement.Hideout.UpdateNextPossibleAttackTime();
		if (PlayerEncounter.IsActive)
		{
			PlayerEncounter.LeaveEncounter = false;
		}
		else
		{
			PlayerEncounter.Start();
			PlayerEncounter.Current.SetupFields(PartyBase.MainParty, Settlement.CurrentSettlement.Party);
		}
		if (PlayerEncounter.Battle == null)
		{
			PlayerEncounter.StartBattle();
			PlayerEncounter.Update();
		}
		CampaignMission.OpenHideoutBattleMission(Settlement.CurrentSettlement.Hideout.SceneName, hideoutTroops?.ToFlattenedRoster());
	}

	private bool CanChangeStatusOfTroop(CharacterObject character)
	{
		if (!character.IsPlayerCharacter)
		{
			return !character.IsNotTransferableInHideouts;
		}
		return false;
	}

	private bool game_menu_talk_to_leader_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
		PartyBase party = Settlement.CurrentSettlement.Parties[0].Party;
		if (party != null && party.LeaderHero != null)
		{
			return party.LeaderHero != Hero.MainHero;
		}
		return false;
	}

	private void game_menu_talk_to_leader_on_consequence(MenuCallbackArgs args)
	{
		PartyBase party = Settlement.CurrentSettlement.Parties[0].Party;
		ConversationCharacterData playerCharacterData = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty);
		ConversationCharacterData conversationPartnerData = new ConversationCharacterData(ConversationHelper.GetConversationCharacterPartyLeader(party), party);
		CampaignMission.OpenConversationMission(playerCharacterData, conversationPartnerData);
	}

	private bool game_menu_wait_until_nightfall_on_condition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Wait;
		if (Settlement.CurrentSettlement.Parties.Any((MobileParty t) => t != MobileParty.MainParty))
		{
			return !IsHideoutAttackableNow();
		}
		return false;
	}

	private void game_menu_wait_until_nightfall_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("hideout_wait");
	}

	private void game_menu_hideout_leave_on_consequence(MenuCallbackArgs args)
	{
		_ = Settlement.CurrentSettlement;
		if (MobileParty.MainParty.CurrentSettlement != null)
		{
			PlayerEncounter.LeaveSettlement();
		}
		PlayerEncounter.Finish();
	}
}
