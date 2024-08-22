using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class ViewDataTrackerCampaignBehavior : CampaignBehaviorBase, IViewDataTracker
{
	private readonly TextObject _characterNotificationText = new TextObject("{=rlqjkZ9Q}You have {NUMBER} new perks available for selection.");

	private readonly TextObject _questNotificationText = new TextObject("{=FAIYN0vN}You have {NUMBER} new updates to your quests.");

	private readonly TextObject _recruitNotificationText = new TextObject("{=PJMbfSPJ}You have {NUMBER} new prisoners to recruit.");

	private Dictionary<CharacterObject, int> _examinedPrisonerCharacterList = new Dictionary<CharacterObject, int>();

	private int _numOfRecruitablePrisoners;

	private List<JournalLog> _unExaminedQuestLogs = new List<JournalLog>();

	private List<Army> _unExaminedArmies = new List<Army>();

	private bool _isCharacterNotificationActive;

	private int _numOfPerks;

	private bool _isMapBarExtended;

	private List<string> _inventoryItemLocks;

	[SaveableField(21)]
	private Dictionary<int, Tuple<int, int>> _inventorySortPreferences;

	private int _partySortType;

	private bool _isPartySortAscending;

	private List<string> _partyTroopLocks;

	private List<string> _partyPrisonerLocks;

	private List<Hero> _encyclopediaBookmarkedHeroes;

	private List<Clan> _encyclopediaBookmarkedClans;

	private List<Concept> _encyclopediaBookmarkedConcepts;

	private List<Kingdom> _encyclopediaBookmarkedKingdoms;

	private List<Settlement> _encyclopediaBookmarkedSettlements;

	private List<CharacterObject> _encyclopediaBookmarkedUnits;

	private QuestBase _questSelection;

	[SaveableField(51)]
	private int _questSortTypeSelection;

	public bool IsPartyNotificationActive { get; private set; }

	public bool IsQuestNotificationActive => _unExaminedQuestLogs.Count > 0;

	public List<JournalLog> UnExaminedQuestLogs => _unExaminedQuestLogs;

	public List<Army> UnExaminedArmies => _unExaminedArmies;

	public int NumOfKingdomArmyNotifications => UnExaminedArmies.Count;

	public bool IsCharacterNotificationActive => _isCharacterNotificationActive;

	public ViewDataTrackerCampaignBehavior()
	{
		_inventoryItemLocks = new List<string>();
		_partyPrisonerLocks = new List<string>();
		_partyTroopLocks = new List<string>();
		_encyclopediaBookmarkedClans = new List<Clan>();
		_encyclopediaBookmarkedConcepts = new List<Concept>();
		_encyclopediaBookmarkedHeroes = new List<Hero>();
		_encyclopediaBookmarkedKingdoms = new List<Kingdom>();
		_encyclopediaBookmarkedSettlements = new List<Settlement>();
		_encyclopediaBookmarkedUnits = new List<CharacterObject>();
		_inventorySortPreferences = new Dictionary<int, Tuple<int, int>>();
	}

	public string GetPartyNotificationText()
	{
		_recruitNotificationText.SetTextVariable("NUMBER", _numOfRecruitablePrisoners);
		return _recruitNotificationText.ToString();
	}

	public void ClearPartyNotification()
	{
		IsPartyNotificationActive = false;
		_numOfRecruitablePrisoners = 0;
	}

	public void UpdatePartyNotification()
	{
		UpdatePrisonerRecruitValue();
	}

	private void UpdatePrisonerRecruitValue()
	{
		Dictionary<CharacterObject, int> dictionary = new Dictionary<CharacterObject, int>();
		foreach (TroopRosterElement item in MobileParty.MainParty.PrisonRoster.GetTroopRoster())
		{
			int num = Campaign.Current.Models.PrisonerRecruitmentCalculationModel.CalculateRecruitableNumber(PartyBase.MainParty, item.Character);
			if (_examinedPrisonerCharacterList.TryGetValue(item.Character, out var value))
			{
				if (value != num)
				{
					_examinedPrisonerCharacterList[item.Character] = num;
					if (value < num)
					{
						IsPartyNotificationActive = true;
						_numOfRecruitablePrisoners += num - value;
					}
				}
			}
			else
			{
				_examinedPrisonerCharacterList.Add(item.Character, num);
				if (num > 0)
				{
					IsPartyNotificationActive = true;
					_numOfRecruitablePrisoners += num;
				}
			}
			dictionary.Add(item.Character, num);
		}
		_examinedPrisonerCharacterList = dictionary;
	}

	public string GetQuestNotificationText()
	{
		_questNotificationText.SetTextVariable("NUMBER", _unExaminedQuestLogs.Count);
		return _questNotificationText.ToString();
	}

	public void OnQuestLogExamined(JournalLog log)
	{
		if (_unExaminedQuestLogs.Contains(log))
		{
			_unExaminedQuestLogs.Remove(log);
		}
	}

	private void OnQuestLogAdded(QuestBase obj, bool hideInformation)
	{
		_unExaminedQuestLogs.Add(obj.JournalEntries[obj.JournalEntries.Count - 1]);
	}

	private void OnIssueLogAdded(IssueBase obj, bool hideInformation)
	{
		_unExaminedQuestLogs.Add(obj.JournalEntries[obj.JournalEntries.Count - 1]);
	}

	public void OnArmyExamined(Army army)
	{
		_unExaminedArmies.Remove(army);
	}

	private void OnArmyDispersed(Army arg1, Army.ArmyDispersionReason arg2, bool isPlayersArmy)
	{
		Army item;
		if (isPlayersArmy && (item = _unExaminedArmies.SingleOrDefault((Army a) => a == arg1)) != null)
		{
			_unExaminedArmies.Remove(item);
		}
	}

	private void OnNewArmyCreated(Army army)
	{
		if (army.Kingdom == Hero.MainHero.MapFaction && army.LeaderParty != MobileParty.MainParty)
		{
			_unExaminedArmies.Add(army);
		}
	}

	public void ClearCharacterNotification()
	{
		_isCharacterNotificationActive = false;
		_numOfPerks = 0;
	}

	public string GetCharacterNotificationText()
	{
		_characterNotificationText.SetTextVariable("NUMBER", _numOfPerks);
		return _characterNotificationText.ToString();
	}

	private void OnHeroGainedSkill(Hero hero, SkillObject skill, int change = 1, bool shouldNotify = true)
	{
		if ((hero == Hero.MainHero || hero.Clan == Clan.PlayerClan) && PerkHelper.AvailablePerkCountOfHero(hero) > 0)
		{
			_isCharacterNotificationActive = true;
			_numOfPerks++;
		}
	}

	private void OnHeroLevelledUp(Hero hero, bool shouldNotify)
	{
		if (hero == Hero.MainHero)
		{
			_isCharacterNotificationActive = true;
		}
	}

	private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
	{
		UpdatePartyNotification();
		UpdatePrisonerRecruitValue();
	}

	public bool GetMapBarExtendedState()
	{
		return _isMapBarExtended;
	}

	public void SetMapBarExtendedState(bool isExtended)
	{
		_isMapBarExtended = isExtended;
	}

	public void SetInventoryLocks(IEnumerable<string> locks)
	{
		_inventoryItemLocks = locks.ToList();
	}

	public IEnumerable<string> GetInventoryLocks()
	{
		return _inventoryItemLocks;
	}

	public void InventorySetSortPreference(int inventoryMode, int sortOption, int sortState)
	{
		_inventorySortPreferences[inventoryMode] = new Tuple<int, int>(sortOption, sortState);
	}

	public Tuple<int, int> InventoryGetSortPreference(int inventoryMode)
	{
		if (_inventorySortPreferences.TryGetValue(inventoryMode, out var value))
		{
			return value;
		}
		return new Tuple<int, int>(0, 0);
	}

	public void SetPartyTroopLocks(IEnumerable<string> locks)
	{
		_partyTroopLocks = locks.ToList();
	}

	public void SetPartyPrisonerLocks(IEnumerable<string> locks)
	{
		_partyPrisonerLocks = locks.ToList();
	}

	public void SetPartySortType(int sortType)
	{
		_partySortType = sortType;
	}

	public void SetIsPartySortAscending(bool isAscending)
	{
		_isPartySortAscending = isAscending;
	}

	public IEnumerable<string> GetPartyTroopLocks()
	{
		return _partyTroopLocks;
	}

	public IEnumerable<string> GetPartyPrisonerLocks()
	{
		return _partyPrisonerLocks;
	}

	public int GetPartySortType()
	{
		return _partySortType;
	}

	public bool GetIsPartySortAscending()
	{
		return _isPartySortAscending;
	}

	public void AddEncyclopediaBookmarkToItem(Hero item)
	{
		_encyclopediaBookmarkedHeroes.Add(item);
	}

	public void AddEncyclopediaBookmarkToItem(Clan clan)
	{
		_encyclopediaBookmarkedClans.Add(clan);
	}

	public void AddEncyclopediaBookmarkToItem(Concept concept)
	{
		_encyclopediaBookmarkedConcepts.Add(concept);
	}

	public void AddEncyclopediaBookmarkToItem(Kingdom kingdom)
	{
		_encyclopediaBookmarkedKingdoms.Add(kingdom);
	}

	public void AddEncyclopediaBookmarkToItem(Settlement settlement)
	{
		_encyclopediaBookmarkedSettlements.Add(settlement);
	}

	public void AddEncyclopediaBookmarkToItem(CharacterObject unit)
	{
		_encyclopediaBookmarkedUnits.Add(unit);
	}

	public void RemoveEncyclopediaBookmarkFromItem(Hero hero)
	{
		_encyclopediaBookmarkedHeroes.Remove(hero);
	}

	public void RemoveEncyclopediaBookmarkFromItem(Clan clan)
	{
		_encyclopediaBookmarkedClans.Remove(clan);
	}

	public void RemoveEncyclopediaBookmarkFromItem(Concept concept)
	{
		_encyclopediaBookmarkedConcepts.Remove(concept);
	}

	public void RemoveEncyclopediaBookmarkFromItem(Kingdom kingdom)
	{
		_encyclopediaBookmarkedKingdoms.Remove(kingdom);
	}

	public void RemoveEncyclopediaBookmarkFromItem(Settlement settlement)
	{
		_encyclopediaBookmarkedSettlements.Remove(settlement);
	}

	public void RemoveEncyclopediaBookmarkFromItem(CharacterObject unit)
	{
		_encyclopediaBookmarkedUnits.Remove(unit);
	}

	public bool IsEncyclopediaBookmarked(Hero hero)
	{
		return _encyclopediaBookmarkedHeroes.Contains(hero);
	}

	public bool IsEncyclopediaBookmarked(Clan clan)
	{
		return _encyclopediaBookmarkedClans.Contains(clan);
	}

	public bool IsEncyclopediaBookmarked(Concept concept)
	{
		return _encyclopediaBookmarkedConcepts.Contains(concept);
	}

	public bool IsEncyclopediaBookmarked(Kingdom kingdom)
	{
		return _encyclopediaBookmarkedKingdoms.Contains(kingdom);
	}

	public bool IsEncyclopediaBookmarked(Settlement settlement)
	{
		return _encyclopediaBookmarkedSettlements.Contains(settlement);
	}

	public bool IsEncyclopediaBookmarked(CharacterObject unit)
	{
		return _encyclopediaBookmarkedUnits.Contains(unit);
	}

	public void SetQuestSelection(QuestBase selection)
	{
		_questSelection = selection;
	}

	public QuestBase GetQuestSelection()
	{
		return _questSelection;
	}

	public override void RegisterEvents()
	{
		CampaignEvents.HeroGainedSkill.AddNonSerializedListener(this, OnHeroGainedSkill);
		CampaignEvents.HeroLevelledUp.AddNonSerializedListener(this, OnHeroLevelledUp);
		CampaignEvents.ArmyCreated.AddNonSerializedListener(this, OnNewArmyCreated);
		CampaignEvents.ArmyDispersed.AddNonSerializedListener(this, OnArmyDispersed);
		CampaignEvents.QuestLogAddedEvent.AddNonSerializedListener(this, OnQuestLogAdded);
		CampaignEvents.IssueLogAddedEvent.AddNonSerializedListener(this, OnIssueLogAdded);
		CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
	}

	public void SetQuestSortTypeSelection(int questSortTypeSelection)
	{
		_questSortTypeSelection = questSortTypeSelection;
	}

	public int GetQuestSortTypeSelection()
	{
		return _questSortTypeSelection;
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_isMapBarExtended", ref _isMapBarExtended);
		dataStore.SyncData("_inventoryItemLocks", ref _inventoryItemLocks);
		dataStore.SyncData("_inventorySortPreferences", ref _inventorySortPreferences);
		dataStore.SyncData("_partySortType", ref _partySortType);
		dataStore.SyncData("_isPartySortAscending", ref _isPartySortAscending);
		dataStore.SyncData("_partyTroopLocks", ref _partyTroopLocks);
		dataStore.SyncData("_partyPrisonerLocks", ref _partyPrisonerLocks);
		dataStore.SyncData("_encyclopediaBookmarkedHeroes", ref _encyclopediaBookmarkedHeroes);
		dataStore.SyncData("_encyclopediaBookmarkedClans", ref _encyclopediaBookmarkedClans);
		dataStore.SyncData("_encyclopediaBookmarkedConcepts", ref _encyclopediaBookmarkedConcepts);
		dataStore.SyncData("_encyclopediaBookmarkedKingdoms", ref _encyclopediaBookmarkedKingdoms);
		dataStore.SyncData("_encyclopediaBookmarkedSettlements", ref _encyclopediaBookmarkedSettlements);
		dataStore.SyncData("_encyclopediaBookmarkedUnits", ref _encyclopediaBookmarkedUnits);
		dataStore.SyncData("_questSelection", ref _questSelection);
		dataStore.SyncData("_unExaminedQuestLogs", ref _unExaminedQuestLogs);
		dataStore.SyncData("_unExaminedArmies", ref _unExaminedArmies);
		dataStore.SyncData("_isCharacterNotificationActive", ref _isCharacterNotificationActive);
		dataStore.SyncData("_numOfPerks", ref _numOfPerks);
		dataStore.SyncData("_examinedPrisonerCharacterList", ref _examinedPrisonerCharacterList);
	}
}
