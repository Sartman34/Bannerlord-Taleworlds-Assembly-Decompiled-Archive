using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class HeroAgentSpawnCampaignBehavior : CampaignBehaviorBase
{
	[NonSerialized]
	private bool _addNotableHelperCharacters;

	private static Location Prison => LocationComplex.Current.GetLocationWithId("prison");

	private static Location LordsHall => LocationComplex.Current.GetLocationWithId("lordshall");

	private static Location Tavern => LocationComplex.Current.GetLocationWithId("tavern");

	private static Location Alley => LocationComplex.Current.GetLocationWithId("alley");

	private static Location Center => LocationComplex.Current.GetLocationWithId("center");

	private static Location VillageCenter => LocationComplex.Current.GetLocationWithId("village_center");

	public override void RegisterEvents()
	{
		CampaignEvents.PrisonersChangeInSettlement.AddNonSerializedListener(this, OnPrisonersChangeInSettlement);
		CampaignEvents.OnGovernorChangedEvent.AddNonSerializedListener(this, OnGovernorChanged);
		CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
		CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
		CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this, OnHeroPrisonerTaken);
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoadFinished);
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, LocationCharactersAreReadyToSpawn);
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, OnMissionEnded);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnGovernorChanged(Town town, Hero oldGovernor, Hero newGovernor)
	{
		if (oldGovernor != null && oldGovernor.IsAlive)
		{
			LocationCharacter locationCharacterOfHero = town.Settlement.LocationComplex.GetLocationCharacterOfHero(oldGovernor);
			if (locationCharacterOfHero != null)
			{
				Campaign.Current.Models.SettlementAccessModel.CanMainHeroEnterLordsHall(town.Settlement, out var accessDetails);
				Location locationOfCharacter = town.Settlement.LocationComplex.GetLocationOfCharacter(oldGovernor);
				if (LocationComplex.Current != null)
				{
					Location location = ((accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.FullAccess) ? LordsHall : (town.IsTown ? Tavern : Center));
					if (location != locationOfCharacter)
					{
						town.Settlement.LocationComplex.ChangeLocation(locationCharacterOfHero, locationOfCharacter, location);
					}
				}
				else
				{
					Debug.Print("LocationComplex is null");
					Debug.FailedAssert("LocationComplex is null", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CampaignBehaviors\\HeroAgentSpawnCampaignBehavior.cs", "OnGovernorChanged", 67);
				}
			}
		}
		if (newGovernor == null)
		{
			return;
		}
		LocationCharacter locationCharacterOfHero2 = town.Settlement.LocationComplex.GetLocationCharacterOfHero(newGovernor);
		if (locationCharacterOfHero2 == null)
		{
			return;
		}
		Location locationOfCharacter2 = town.Settlement.LocationComplex.GetLocationOfCharacter(newGovernor);
		if (LocationComplex.Current != null)
		{
			if (locationOfCharacter2 != LordsHall)
			{
				town.Settlement.LocationComplex.ChangeLocation(locationCharacterOfHero2, locationOfCharacter2, LordsHall);
			}
		}
		else
		{
			Debug.Print("LocationComplex is null");
			Debug.FailedAssert("LocationComplex is null", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CampaignBehaviors\\HeroAgentSpawnCampaignBehavior.cs", "OnGovernorChanged", 88);
		}
	}

	private void OnMissionEnded(IMission mission)
	{
		if (LocationComplex.Current != null && PlayerEncounter.LocationEncounter != null && Settlement.CurrentSettlement != null && !Hero.MainHero.IsPrisoner && !Settlement.CurrentSettlement.IsUnderSiege)
		{
			AddSettlementLocationCharacters(Settlement.CurrentSettlement);
			_addNotableHelperCharacters = true;
		}
	}

	public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
	{
		if (LocationComplex.Current == null || PlayerEncounter.LocationEncounter == null)
		{
			return;
		}
		if (mobileParty != null)
		{
			if (mobileParty == MobileParty.MainParty)
			{
				AddSettlementLocationCharacters(settlement);
				_addNotableHelperCharacters = true;
			}
			else if (MobileParty.MainParty.CurrentSettlement == settlement && (settlement.IsFortification || settlement.IsVillage))
			{
				AddPartyHero(mobileParty, settlement);
			}
		}
		else if (MobileParty.MainParty.CurrentSettlement == settlement && hero != null)
		{
			if (hero.IsNotable)
			{
				AddNotableLocationCharacter(hero, settlement);
			}
			else if (hero.IsWanderer)
			{
				AddWandererLocationCharacter(hero, settlement);
			}
		}
	}

	public void OnSettlementLeft(MobileParty mobileParty, Settlement settlement)
	{
		if (mobileParty != MobileParty.MainParty && MobileParty.MainParty.CurrentSettlement == settlement && mobileParty.LeaderHero != null && LocationComplex.Current != null)
		{
			LocationComplex.Current.GetLocationOfCharacter(mobileParty.LeaderHero)?.RemoveCharacter(mobileParty.LeaderHero);
		}
	}

	private void OnGameLoadFinished()
	{
		if (Settlement.CurrentSettlement != null && !Hero.MainHero.IsPrisoner && LocationComplex.Current != null && PlayerEncounter.LocationEncounter != null && !Settlement.CurrentSettlement.IsUnderSiege)
		{
			AddSettlementLocationCharacters(Settlement.CurrentSettlement);
		}
	}

	private void AddSettlementLocationCharacters(Settlement settlement)
	{
		if (!settlement.IsFortification && !settlement.IsVillage)
		{
			return;
		}
		List<MobileParty> partiesToBeSpawn = Settlement.CurrentSettlement.Parties.ToList();
		if (settlement.IsFortification)
		{
			AddLordsHallCharacters(settlement, ref partiesToBeSpawn);
			RefreshPrisonCharacters(settlement);
			AddCompanionsAndClanMembersToSettlement(settlement);
			if (settlement.IsFortification)
			{
				AddNotablesAndWanderers(settlement);
			}
		}
		else if (settlement.IsVillage)
		{
			AddHeroesWithoutPartyCharactersToVillage(settlement);
			AddCompanionsAndClanMembersToSettlement(settlement);
		}
		foreach (MobileParty item in partiesToBeSpawn)
		{
			AddPartyHero(item, settlement);
		}
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
	{
		Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
		if (_addNotableHelperCharacters && (CampaignMission.Current.Location == Center || CampaignMission.Current.Location == VillageCenter))
		{
			SpawnNotableHelperCharacters(settlement);
			_addNotableHelperCharacters = false;
		}
	}

	private void AddCompanionsAndClanMembersToSettlement(Settlement settlement)
	{
		if (!settlement.IsFortification && !settlement.IsVillage)
		{
			return;
		}
		foreach (Hero lord in Clan.PlayerClan.Lords)
		{
			int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
			if (lord != Hero.MainHero && !(lord.Age < (float)heroComesOfAge) && !lord.IsPrisoner && lord.CurrentSettlement == settlement && (lord.GovernorOf == null || lord.GovernorOf != settlement.Town))
			{
				Location location;
				if (settlement.IsFortification)
				{
					Campaign.Current.Models.SettlementAccessModel.CanMainHeroEnterLordsHall(settlement, out var accessDetails);
					location = ((accessDetails.AccessLevel == SettlementAccessModel.AccessLevel.FullAccess) ? LordsHall : (settlement.IsTown ? Tavern : Center));
				}
				else
				{
					location = VillageCenter;
				}
				uint color = (uint)(((int?)lord.MapFaction?.Color) ?? (-3357781));
				uint color2 = (uint)(((int?)lord.MapFaction?.Color) ?? (-3357781));
				Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(lord.CharacterObject.Race);
				AgentData agentData = new AgentData(new PartyAgentOrigin(PartyBase.MainParty, lord.CharacterObject)).Monster(baseMonsterFromRace).NoHorses(noHorses: true).ClothingColor1(color)
					.ClothingColor2(color2);
				location.AddCharacter(new LocationCharacter(agentData, SandBoxManager.Instance.AgentBehaviorManager.AddCompanionBehaviors, "sp_notable", fixedLocation: true, LocationCharacter.CharacterRelations.Friendly, null, !PlayerEncounter.LocationEncounter.Settlement.IsVillage, isFixedCharacter: false, null, isHidden: false, isVisualTracked: true));
			}
		}
		foreach (Hero companion in Hero.MainHero.CompanionsInParty)
		{
			if (!companion.IsWounded && !PlayerEncounter.LocationEncounter.CharactersAccompanyingPlayer.Exists((AccompanyingCharacter x) => x.LocationCharacter.Character.HeroObject == companion))
			{
				uint color3 = (uint)(((int?)companion.MapFaction?.Color) ?? (-3357781));
				uint color4 = (uint)(((int?)companion.MapFaction?.Color) ?? (-3357781));
				Monster baseMonsterFromRace2 = FaceGen.GetBaseMonsterFromRace(companion.CharacterObject.Race);
				Location location = (settlement.IsFortification ? Center : VillageCenter);
				AgentData agentData2 = new AgentData(new PartyAgentOrigin(PartyBase.MainParty, companion.CharacterObject)).Monster(baseMonsterFromRace2).NoHorses(noHorses: true).ClothingColor1(color3)
					.ClothingColor2(color4);
				location.AddCharacter(new LocationCharacter(agentData2, SandBoxManager.Instance.AgentBehaviorManager.AddCompanionBehaviors, "sp_notable", fixedLocation: true, LocationCharacter.CharacterRelations.Friendly, null, !PlayerEncounter.LocationEncounter.Settlement.IsVillage, isFixedCharacter: false, null, isHidden: false, isVisualTracked: true));
			}
		}
	}

	private void AddPartyHero(MobileParty mobileParty, Settlement settlement)
	{
		Hero leaderHero = mobileParty.LeaderHero;
		if (leaderHero != null && leaderHero != Hero.MainHero)
		{
			uint color = (uint)(((int?)leaderHero.MapFaction?.Color) ?? (-3357781));
			uint color2 = (uint)(((int?)leaderHero.MapFaction?.Color) ?? (-3357781));
			Tuple<string, Monster> actionSetAndMonster = GetActionSetAndMonster(leaderHero.CharacterObject);
			AgentData agentData = new AgentData(new PartyAgentOrigin(mobileParty.Party, leaderHero.CharacterObject)).Monster(actionSetAndMonster.Item2).NoHorses(noHorses: true).ClothingColor1(color)
				.ClothingColor2(color2);
			string spawnTag = "sp_notable";
			(settlement.IsFortification ? LordsHall : ((!settlement.IsVillage) ? Center : VillageCenter))?.AddCharacter(new LocationCharacter(agentData, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, spawnTag, fixedLocation: true, LocationCharacter.CharacterRelations.Neutral, actionSetAndMonster.Item1, !settlement.IsVillage));
		}
	}

	private void OnHeroPrisonerTaken(PartyBase capturerParty, Hero prisoner)
	{
		if (capturerParty.IsSettlement)
		{
			OnPrisonersChangeInSettlement(capturerParty.Settlement, null, prisoner, takenFromDungeon: false);
		}
	}

	public void OnPrisonersChangeInSettlement(Settlement settlement, FlattenedTroopRoster prisonerRoster, Hero prisonerHero, bool takenFromDungeon)
	{
		if (settlement == null || !settlement.IsFortification || LocationComplex.Current != settlement.LocationComplex)
		{
			return;
		}
		if (prisonerHero != null)
		{
			SendPrisonerHeroToNextLocation(settlement, prisonerHero, takenFromDungeon);
		}
		if (prisonerRoster == null)
		{
			return;
		}
		foreach (FlattenedTroopRosterElement item in prisonerRoster)
		{
			if (item.Troop.IsHero)
			{
				SendPrisonerHeroToNextLocation(settlement, item.Troop.HeroObject, takenFromDungeon);
			}
		}
	}

	private void SendPrisonerHeroToNextLocation(Settlement settlement, Hero hero, bool takenFromDungeon)
	{
		Location locationOfCharacter = LocationComplex.Current.GetLocationOfCharacter(hero);
		Location location = DecideNewLocationOnPrisonerChange(settlement, hero, takenFromDungeon);
		LocationCharacter locationCharacterOfHero = LocationComplex.Current.GetLocationCharacterOfHero(hero);
		if (locationCharacterOfHero == null)
		{
			if (location != null)
			{
				AddHeroToDecidedLocation(location, hero, settlement);
			}
		}
		else if (locationOfCharacter != location)
		{
			LocationComplex.Current.ChangeLocation(locationCharacterOfHero, locationOfCharacter, location);
		}
	}

	private Location DecideNewLocationOnPrisonerChange(Settlement settlement, Hero hero, bool takenFromDungeon)
	{
		if (hero.IsPrisoner)
		{
			if (!takenFromDungeon)
			{
				return Prison;
			}
			return null;
		}
		if (!settlement.IsFortification)
		{
			return VillageCenter;
		}
		if (hero.IsWanderer && settlement.IsTown)
		{
			return Tavern;
		}
		if (hero.CharacterObject.Occupation == Occupation.Lord)
		{
			return LordsHall;
		}
		return Center;
	}

	private void AddHeroToDecidedLocation(Location location, Hero hero, Settlement settlement)
	{
		if (location == Prison)
		{
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(hero.CharacterObject.Race, "_settlement");
			AgentData agentData = new AgentData(new SimpleAgentOrigin(hero.CharacterObject)).NoWeapons(noWeapons: true).Monster(monsterWithSuffix).NoHorses(noHorses: true);
			location.AddCharacter(new LocationCharacter(agentData, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, "sp_prisoner", fixedLocation: true, LocationCharacter.CharacterRelations.Neutral, ActionSetCode.GenerateActionSetNameWithSuffix(agentData.AgentMonster, agentData.AgentIsFemale, "_villager"), useCivilianEquipment: true));
		}
		else if (location == VillageCenter)
		{
			Monster monsterWithSuffix2 = FaceGen.GetMonsterWithSuffix(hero.CharacterObject.Race, "_settlement");
			AgentData agentData2 = new AgentData(new PartyAgentOrigin(null, hero.CharacterObject)).Monster(monsterWithSuffix2);
			location.AddCharacter(new LocationCharacter(agentData2, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, "sp_notable_rural_notable", fixedLocation: false, LocationCharacter.CharacterRelations.Neutral, null, useCivilianEquipment: true));
		}
		else if (location == Tavern)
		{
			Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(hero.CharacterObject.Race);
			AgentData agentData3 = new AgentData(new PartyAgentOrigin(PartyBase.MainParty, hero.CharacterObject)).Monster(baseMonsterFromRace).NoHorses(noHorses: true);
			location.AddCharacter(new LocationCharacter(agentData3, SandBoxManager.Instance.AgentBehaviorManager.AddCompanionBehaviors, null, fixedLocation: true, LocationCharacter.CharacterRelations.Friendly, null, !PlayerEncounter.LocationEncounter.Settlement.IsVillage, isFixedCharacter: false, null, isHidden: false, isVisualTracked: true));
		}
		else if (location == LordsHall)
		{
			Tuple<string, Monster> actionSetAndMonster = GetActionSetAndMonster(hero.CharacterObject);
			AgentData agentData4 = new AgentData(new SimpleAgentOrigin(hero.CharacterObject)).Monster(actionSetAndMonster.Item2).NoHorses(noHorses: true);
			location.AddCharacter(new LocationCharacter(agentData4, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, null, fixedLocation: true, LocationCharacter.CharacterRelations.Neutral, actionSetAndMonster.Item1, useCivilianEquipment: true));
		}
		else if (location == Center)
		{
			if (hero.IsNotable)
			{
				AddNotableLocationCharacter(hero, settlement);
				return;
			}
			Monster monsterWithSuffix3 = FaceGen.GetMonsterWithSuffix(hero.CharacterObject.Race, "_settlement");
			AgentData agentData5 = new AgentData(new PartyAgentOrigin(null, hero.CharacterObject)).Monster(monsterWithSuffix3);
			location.AddCharacter(new LocationCharacter(agentData5, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, "sp_notable_rural_notable", fixedLocation: false, LocationCharacter.CharacterRelations.Neutral, null, useCivilianEquipment: true));
		}
	}

	private void AddLordsHallCharacters(Settlement settlement, ref List<MobileParty> partiesToBeSpawn)
	{
		Hero hero = null;
		Hero hero2 = null;
		if (settlement.MapFaction.IsKingdomFaction)
		{
			Hero leader = ((Kingdom)settlement.MapFaction).Leader;
			if (leader.CurrentSettlement == settlement)
			{
				hero = leader;
			}
			if (leader.Spouse != null && leader.Spouse.CurrentSettlement == settlement)
			{
				hero2 = leader.Spouse;
			}
		}
		if (hero == null && settlement.OwnerClan.Leader.CurrentSettlement == settlement)
		{
			hero = settlement.OwnerClan.Leader;
		}
		if (hero2 == null && settlement.OwnerClan.Leader.Spouse != null && settlement.OwnerClan.Leader.Spouse.CurrentSettlement == settlement)
		{
			hero2 = settlement.OwnerClan.Leader.Spouse;
		}
		bool flag = false;
		if (hero != null && hero != Hero.MainHero)
		{
			uint color = (uint)(((int?)hero.MapFaction?.Color) ?? (-3357781));
			uint color2 = (uint)(((int?)hero.MapFaction?.Color) ?? (-3357781));
			flag = true;
			Tuple<string, Monster> actionSetAndMonster = GetActionSetAndMonster(hero.CharacterObject);
			AgentData agentData = new AgentData(new PartyAgentOrigin(null, hero.CharacterObject)).Monster(actionSetAndMonster.Item2).NoHorses(noHorses: true).ClothingColor1(color)
				.ClothingColor2(color2);
			LordsHall.AddCharacter(new LocationCharacter(agentData, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, "sp_throne", fixedLocation: true, LocationCharacter.CharacterRelations.Neutral, actionSetAndMonster.Item1, useCivilianEquipment: true));
			if (hero.PartyBelongedTo != null && partiesToBeSpawn.Contains(hero.PartyBelongedTo))
			{
				partiesToBeSpawn.Remove(hero.PartyBelongedTo);
			}
		}
		if (hero2 != null && hero2 != Hero.MainHero)
		{
			uint color3 = (uint)(((int?)hero2.MapFaction?.Color) ?? (-3357781));
			uint color4 = (uint)(((int?)hero2.MapFaction?.Color) ?? (-3357781));
			Tuple<string, Monster> actionSetAndMonster2 = GetActionSetAndMonster(hero2.CharacterObject);
			AgentData agentData2 = new AgentData(new PartyAgentOrigin(null, hero2.CharacterObject)).Monster(actionSetAndMonster2.Item2).NoHorses(noHorses: true).ClothingColor1(color3)
				.ClothingColor2(color4);
			LordsHall.AddCharacter(new LocationCharacter(agentData2, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, flag ? "sp_notable" : "sp_throne", fixedLocation: true, LocationCharacter.CharacterRelations.Neutral, actionSetAndMonster2.Item1, useCivilianEquipment: true));
			if (hero2.PartyBelongedTo != null && partiesToBeSpawn.Contains(hero2.PartyBelongedTo))
			{
				partiesToBeSpawn.Remove(hero2.PartyBelongedTo);
			}
		}
		int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
		foreach (Hero item in settlement.HeroesWithoutParty)
		{
			if (item != hero && item != hero2 && item.Age >= (float)heroComesOfAge && !item.IsPrisoner && (item.Clan != Clan.PlayerClan || (item.GovernorOf != null && item.GovernorOf == settlement.Town)))
			{
				Tuple<string, Monster> actionSetAndMonster3 = GetActionSetAndMonster(item.CharacterObject);
				uint color5 = (uint)(((int?)item.MapFaction?.Color) ?? (-3357781));
				uint color6 = (uint)(((int?)item.MapFaction?.Color) ?? (-3357781));
				AgentData agentData3 = new AgentData(new SimpleAgentOrigin(item.CharacterObject)).Monster(actionSetAndMonster3.Item2).NoHorses(noHorses: true).ClothingColor1(color5)
					.ClothingColor2(color6);
				LordsHall.AddCharacter(new LocationCharacter(agentData3, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, "sp_notable", fixedLocation: true, LocationCharacter.CharacterRelations.Neutral, actionSetAndMonster3.Item1, useCivilianEquipment: true));
			}
		}
	}

	private void RefreshPrisonCharacters(Settlement settlement)
	{
		Prison.RemoveAllHeroCharactersFromPrison();
		List<CharacterObject> prisonerHeroes = settlement.SettlementComponent.GetPrisonerHeroes();
		if (Campaign.Current.GameMode != CampaignGameMode.Campaign)
		{
			for (int i = 0; i < 5; i++)
			{
				prisonerHeroes.Add(Game.Current.ObjectManager.GetObject<CharacterObject>("townsman_empire"));
			}
		}
		foreach (CharacterObject item in prisonerHeroes)
		{
			uint color = (uint)(((int?)item.HeroObject?.MapFaction?.Color) ?? (-3357781));
			uint color2 = (uint)(((int?)item.HeroObject?.MapFaction?.Color) ?? (-3357781));
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(item.Race, "_settlement");
			AgentData agentData = new AgentData(new SimpleAgentOrigin(item)).NoWeapons(noWeapons: true).Monster(monsterWithSuffix).NoHorses(noHorses: true)
				.ClothingColor1(color)
				.ClothingColor2(color2);
			Prison.AddCharacter(new LocationCharacter(agentData, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, "sp_prisoner", fixedLocation: true, LocationCharacter.CharacterRelations.Neutral, ActionSetCode.GenerateActionSetNameWithSuffix(agentData.AgentMonster, agentData.AgentIsFemale, "_villager"), useCivilianEquipment: true));
		}
	}

	private void AddNotablesAndWanderers(Settlement settlement)
	{
		if (Campaign.Current.GameMode != CampaignGameMode.Campaign)
		{
			return;
		}
		foreach (Hero notable in settlement.Notables)
		{
			AddNotableLocationCharacter(notable, settlement);
		}
		foreach (Hero item in settlement.HeroesWithoutParty.Where((Hero x) => x.IsWanderer || x.IsPlayerCompanion))
		{
			if (item.GovernorOf == null || item.GovernorOf != settlement.Town)
			{
				AddWandererLocationCharacter(item, settlement);
			}
		}
	}

	private void AddWandererLocationCharacter(Hero wanderer, Settlement settlement)
	{
		bool num = settlement.Culture.StringId.ToLower() == "aserai" || settlement.Culture.StringId.ToLower() == "khuzait";
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(wanderer.CharacterObject.Race, "_settlement");
		string actionSetCode = (num ? ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, wanderer.IsFemale, "_warrior_in_aserai_tavern") : ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, wanderer.IsFemale, "_warrior_in_tavern"));
		LocationCharacter locationCharacter = new LocationCharacter(new AgentData(new PartyAgentOrigin(null, wanderer.CharacterObject)).Monster(monsterWithSuffix).NoHorses(noHorses: true), SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, "npc_common", fixedLocation: true, LocationCharacter.CharacterRelations.Neutral, actionSetCode, useCivilianEquipment: true);
		if (settlement.IsCastle)
		{
			Center.AddCharacter(locationCharacter);
		}
		else if (settlement.IsTown)
		{
			Location location = null;
			IAlleyCampaignBehavior campaignBehavior = CampaignBehaviorBase.GetCampaignBehavior<IAlleyCampaignBehavior>();
			location = ((campaignBehavior == null || !campaignBehavior.IsHeroAlleyLeaderOfAnyPlayerAlley(wanderer)) ? Tavern : Alley);
			location.AddCharacter(locationCharacter);
		}
		else
		{
			VillageCenter.AddCharacter(locationCharacter);
		}
	}

	private void AddNotableLocationCharacter(Hero notable, Settlement settlement)
	{
		string text = null;
		text = (notable.IsArtisan ? "_villager_artisan" : (notable.IsMerchant ? "_villager_merchant" : (notable.IsPreacher ? "_villager_preacher" : (notable.IsGangLeader ? "_villager_gangleader" : (notable.IsRuralNotable ? "_villager_ruralnotable" : (notable.IsFemale ? "_lord" : "_villager_merchant"))))));
		string text2 = (notable.IsArtisan ? "sp_notable_artisan" : (notable.IsMerchant ? "sp_notable_merchant" : (notable.IsPreacher ? "sp_notable_preacher" : (notable.IsGangLeader ? "sp_notable_gangleader" : (notable.IsRuralNotable ? "sp_notable_rural_notable" : ((notable.GovernorOf == notable.CurrentSettlement.Town) ? "sp_governor" : "sp_notable"))))));
		MBReadOnlyList<Workshop> ownedWorkshops = notable.OwnedWorkshops;
		if (ownedWorkshops.Count != 0)
		{
			for (int i = 0; i < ownedWorkshops.Count; i++)
			{
				if (!ownedWorkshops[i].WorkshopType.IsHidden)
				{
					text2 = text2 + "_" + ownedWorkshops[i].Tag;
					break;
				}
			}
		}
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(notable.CharacterObject.Race, "_settlement");
		AgentData agentData = new AgentData(new PartyAgentOrigin(null, notable.CharacterObject)).Monster(monsterWithSuffix).NoHorses(noHorses: true);
		LocationCharacter locationCharacter = new LocationCharacter(agentData, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, text2, fixedLocation: true, LocationCharacter.CharacterRelations.Neutral, ActionSetCode.GenerateActionSetNameWithSuffix(agentData.AgentMonster, notable.IsFemale, text), useCivilianEquipment: true);
		if (settlement.IsVillage)
		{
			VillageCenter.AddCharacter(locationCharacter);
		}
		else
		{
			Center.AddCharacter(locationCharacter);
		}
	}

	private void AddHeroesWithoutPartyCharactersToVillage(Settlement settlement)
	{
		foreach (Hero item in settlement.HeroesWithoutParty)
		{
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(item.CharacterObject.Race, "_settlement");
			AgentData agentData = new AgentData(new PartyAgentOrigin(null, item.CharacterObject)).Monster(monsterWithSuffix);
			VillageCenter.AddCharacter(new LocationCharacter(agentData, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, "sp_notable_rural_notable", fixedLocation: false, LocationCharacter.CharacterRelations.Neutral, null, useCivilianEquipment: true));
		}
	}

	private void SpawnNotableHelperCharacters(Settlement settlement)
	{
		int num = settlement.Notables.Count((Hero x) => x.IsGangLeader);
		int characterToSpawnCount = settlement.Notables.Count((Hero x) => x.IsPreacher);
		int characterToSpawnCount2 = settlement.Notables.Count((Hero x) => x.IsArtisan);
		int characterToSpawnCount3 = settlement.Notables.Count((Hero x) => x.IsRuralNotable || x.IsHeadman);
		int characterToSpawnCount4 = settlement.Notables.Count((Hero x) => x.IsMerchant);
		SpawnNotableHelperCharacter(settlement.Culture.GangleaderBodyguard, "_gangleader_bodyguard", "sp_gangleader_bodyguard", num * 2);
		SpawnNotableHelperCharacter(settlement.Culture.PreacherNotary, "_merchant_notary", "sp_preacher_notary", characterToSpawnCount);
		SpawnNotableHelperCharacter(settlement.Culture.ArtisanNotary, "_merchant_notary", "sp_artisan_notary", characterToSpawnCount2);
		SpawnNotableHelperCharacter(settlement.Culture.RuralNotableNotary, "_merchant_notary", "sp_rural_notable_notary", characterToSpawnCount3);
		SpawnNotableHelperCharacter(settlement.Culture.MerchantNotary, "_merchant_notary", "sp_merchant_notary", characterToSpawnCount4);
	}

	private void SpawnNotableHelperCharacter(CharacterObject character, string actionSetSuffix, string tag, int characterToSpawnCount)
	{
		Location location = Center ?? VillageCenter;
		while (characterToSpawnCount > 0)
		{
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(character.Race, "_settlement");
			Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(character, out var minimumAge, out var maximumAge, "Notary");
			AgentData agentData = new AgentData(new SimpleAgentOrigin(character)).Monster(monsterWithSuffix).NoHorses(noHorses: true).Age(MBRandom.RandomInt(minimumAge, maximumAge));
			LocationCharacter locationCharacter = new LocationCharacter(agentData, SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, tag, fixedLocation: true, LocationCharacter.CharacterRelations.Neutral, ActionSetCode.GenerateActionSetNameWithSuffix(agentData.AgentMonster, agentData.AgentIsFemale, actionSetSuffix), useCivilianEquipment: true);
			location.AddCharacter(locationCharacter);
			characterToSpawnCount--;
		}
	}

	private static Tuple<string, Monster> GetActionSetAndMonster(CharacterObject character)
	{
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(character.Race, "_settlement");
		return new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, character.IsFemale, "_lord"), monsterWithSuffix);
	}
}
