using System.Collections.Generic;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Tutorial;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;

public class GameMenuVM : ViewModel
{
	private bool _isInspected;

	private bool _plunderEventRegistered;

	private GameMenuManager _gameMenuManager;

	private Dictionary<GameMenuOption.LeaveType, GameKey> _shortcutKeys;

	private Dictionary<string, string> _menuTextAttributeStrings;

	private Dictionary<string, object> _menuTextAttributes;

	private TextObject _menuText = TextObject.Empty;

	private MBBindingList<GameMenuItemVM> _itemList;

	private MBBindingList<GameMenuItemProgressVM> _progressItemList;

	private string _titleText;

	private string _contextText;

	private string _background;

	private bool _isNight;

	private bool _isInSiegeMode;

	private bool _isEncounterMenu;

	private MBBindingList<GameMenuPlunderItemVM> _plunderItems;

	private string _latestTutorialElementID;

	private bool _isTavernButtonHighlightApplied;

	private bool _isSellPrisonerButtonHighlightApplied;

	private bool _isShopButtonHighlightApplied;

	private bool _isRecruitButtonHighlightApplied;

	private bool _isHostileActionButtonHighlightApplied;

	private bool _isTownBesiegeButtonHighlightApplied;

	private bool _isEnterTutorialVillageButtonHighlightApplied;

	private bool _requireContextTextUpdate;

	public bool IsInspected
	{
		set
		{
			if (_isInspected != value)
			{
				_isInspected = value;
			}
		}
	}

	public MenuContext MenuContext { get; private set; }

	[DataSourceProperty]
	public bool IsNight
	{
		get
		{
			return _isNight;
		}
		set
		{
			if (value != _isNight)
			{
				_isNight = value;
				OnPropertyChangedWithValue(value, "IsNight");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInSiegeMode
	{
		get
		{
			return _isInSiegeMode;
		}
		set
		{
			if (value != _isInSiegeMode)
			{
				_isInSiegeMode = value;
				OnPropertyChangedWithValue(value, "IsInSiegeMode");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEncounterMenu
	{
		get
		{
			return _isEncounterMenu;
		}
		set
		{
			if (value != _isEncounterMenu)
			{
				_isEncounterMenu = value;
				OnPropertyChangedWithValue(value, "IsEncounterMenu");
			}
		}
	}

	[DataSourceProperty]
	public string TitleText
	{
		get
		{
			return _titleText;
		}
		set
		{
			if (value != _titleText)
			{
				_titleText = value;
				OnPropertyChangedWithValue(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string ContextText
	{
		get
		{
			return _contextText;
		}
		set
		{
			if (value != _contextText)
			{
				_contextText = value;
				OnPropertyChangedWithValue(value, "ContextText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<GameMenuItemVM> ItemList
	{
		get
		{
			return _itemList;
		}
		set
		{
			if (value != _itemList)
			{
				_itemList = value;
				OnPropertyChangedWithValue(value, "ItemList");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<GameMenuItemProgressVM> ProgressItemList
	{
		get
		{
			return _progressItemList;
		}
		set
		{
			if (value != _progressItemList)
			{
				_progressItemList = value;
				OnPropertyChangedWithValue(value, "ProgressItemList");
			}
		}
	}

	[DataSourceProperty]
	public string Background
	{
		get
		{
			return _background;
		}
		set
		{
			if (value != _background)
			{
				_background = value;
				OnPropertyChangedWithValue(value, "Background");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<GameMenuPlunderItemVM> PlunderItems
	{
		get
		{
			return _plunderItems;
		}
		set
		{
			if (value != _plunderItems)
			{
				_plunderItems = value;
				OnPropertyChangedWithValue(value, "PlunderItems");
			}
		}
	}

	public GameMenuVM(MenuContext menuContext)
	{
		MenuContext = menuContext;
		_gameMenuManager = Campaign.Current.GameMenuManager;
		ItemList = new MBBindingList<GameMenuItemVM>();
		ProgressItemList = new MBBindingList<GameMenuItemProgressVM>();
		_shortcutKeys = new Dictionary<GameMenuOption.LeaveType, GameKey>();
		_menuTextAttributeStrings = new Dictionary<string, string>();
		_menuTextAttributes = new Dictionary<string, object>();
		Background = menuContext.CurrentBackgroundMeshName;
		IsInSiegeMode = PlayerSiege.PlayerSiegeEvent != null;
		Game.Current.EventManager.RegisterEvent<TutorialNotificationElementChangeEvent>(OnTutorialNotificationElementIDChange);
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		ItemList.ApplyActionOnAllItems(delegate(GameMenuItemVM x)
		{
			x.RefreshValues();
		});
		ProgressItemList.ApplyActionOnAllItems(delegate(GameMenuItemProgressVM x)
		{
			x.RefreshValues();
		});
		Refresh(forceUpdateItems: true);
	}

	public void Refresh(bool forceUpdateItems)
	{
		TitleText = MenuContext.GameMenu.MenuTitle?.ToString();
		TaleWorlds.CampaignSystem.GameMenus.GameMenu gameMenu = MenuContext.GameMenu;
		IsEncounterMenu = gameMenu != null && gameMenu.OverlayType == GameOverlays.MenuOverlayType.Encounter;
		Background = (string.IsNullOrEmpty(MenuContext.CurrentBackgroundMeshName) ? "wait_guards_stop" : MenuContext.CurrentBackgroundMeshName);
		if (forceUpdateItems)
		{
			ItemList.Clear();
			ProgressItemList.Clear();
			int virtualMenuOptionAmount = _gameMenuManager.GetVirtualMenuOptionAmount(MenuContext);
			for (int i = 0; i < virtualMenuOptionAmount; i++)
			{
				_gameMenuManager.SetCurrentRepeatableIndex(MenuContext, i);
				if (_gameMenuManager.GetVirtualMenuOptionConditionsHold(MenuContext, i))
				{
					TextObject textObject;
					TextObject textObject2;
					if (_gameMenuManager.GetVirtualGameMenuOption(MenuContext, i).IsRepeatable)
					{
						textObject = new TextObject(_gameMenuManager.GetVirtualMenuOptionText(MenuContext, i).ToString());
						textObject2 = new TextObject(_gameMenuManager.GetVirtualMenuOptionText2(MenuContext, i).ToString());
					}
					else
					{
						textObject = _gameMenuManager.GetVirtualMenuOptionText(MenuContext, i);
						textObject2 = _gameMenuManager.GetVirtualMenuOptionText2(MenuContext, i);
					}
					TextObject virtualMenuOptionTooltip = _gameMenuManager.GetVirtualMenuOptionTooltip(MenuContext, i);
					TextObject textObject3 = textObject;
					TextObject textObject4 = textObject2;
					TextObject tooltip = virtualMenuOptionTooltip;
					TaleWorlds.CampaignSystem.GameMenus.GameMenu.MenuAndOptionType virtualMenuAndOptionType = _gameMenuManager.GetVirtualMenuAndOptionType(MenuContext);
					GameMenuOption virtualGameMenuOption = _gameMenuManager.GetVirtualGameMenuOption(MenuContext, i);
					GameKey shortcutKey = (_shortcutKeys.ContainsKey(virtualGameMenuOption.OptionLeaveType) ? _shortcutKeys[virtualGameMenuOption.OptionLeaveType] : null);
					GameMenuItemVM gameMenuItemVM = new GameMenuItemVM(MenuContext, i, textObject3, (textObject4 == TextObject.Empty) ? textObject3 : textObject4, tooltip, virtualMenuAndOptionType, virtualGameMenuOption, shortcutKey);
					if (!string.IsNullOrEmpty(_latestTutorialElementID))
					{
						gameMenuItemVM.IsHighlightEnabled = gameMenuItemVM.OptionID == _latestTutorialElementID;
					}
					ItemList.Add(gameMenuItemVM);
					if (virtualMenuAndOptionType == TaleWorlds.CampaignSystem.GameMenus.GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption || virtualMenuAndOptionType == TaleWorlds.CampaignSystem.GameMenus.GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption)
					{
						ProgressItemList.Add(new GameMenuItemProgressVM(MenuContext, i));
					}
				}
			}
		}
		if (MobileParty.MainParty?.MapEvent != null)
		{
			int[] array = new int[2];
			foreach (PartyBase involvedParty in MobileParty.MainParty.MapEvent.InvolvedParties)
			{
				_ = involvedParty.Side;
				_ = PartyBase.MainParty.Side;
				if (involvedParty.Side != BattleSideEnum.None)
				{
					array[(int)involvedParty.Side] += involvedParty.NumberOfHealthyMembers;
				}
			}
			if (MobileParty.MainParty.MapEvent.IsRaid && !_plunderEventRegistered)
			{
				PlunderItems = new MBBindingList<GameMenuPlunderItemVM>();
				CampaignEvents.ItemsLooted.AddNonSerializedListener(this, OnItemsPlundered);
				_plunderEventRegistered = true;
			}
		}
		else if (_plunderEventRegistered)
		{
			CampaignEvents.ItemsLooted.ClearListeners(this);
			_plunderEventRegistered = false;
			PlunderItems?.Clear();
		}
		_requireContextTextUpdate = true;
	}

	public void OnFrameTick()
	{
		IsInSiegeMode = PlayerSiege.PlayerSiegeEvent != null;
		if (_requireContextTextUpdate)
		{
			_menuText = _gameMenuManager.GetMenuText(MenuContext);
			ContextText = _menuText.ToString();
			_menuTextAttributes.Clear();
			_menuTextAttributeStrings.Clear();
			if (_menuText?.Attributes != null)
			{
				foreach (KeyValuePair<string, object> attribute in _menuText.Attributes)
				{
					_menuTextAttributes[attribute.Key] = attribute.Value;
					_menuTextAttributeStrings[attribute.Key] = attribute.Value.ToString();
				}
			}
			_requireContextTextUpdate = false;
		}
		foreach (GameMenuItemVM item in ItemList)
		{
			item.Refresh();
		}
		foreach (GameMenuItemProgressVM progressItem in ProgressItemList)
		{
			progressItem.OnTick();
		}
		if (Campaign.Current.GameMode == CampaignGameMode.Campaign)
		{
			IsNight = Campaign.Current.IsNight;
		}
		_requireContextTextUpdate = IsMenuTextChanged();
	}

	private bool IsMenuTextChanged()
	{
		TextObject textObject = _gameMenuManager?.GetMenuText(MenuContext);
		if (_menuText != textObject)
		{
			return true;
		}
		if (_menuTextAttributes.Count != _menuText?.Attributes?.Count)
		{
			return true;
		}
		foreach (string key in _menuTextAttributes.Keys)
		{
			object value = null;
			object obj = _menuTextAttributes[key];
			TextObject menuText = _menuText;
			if (menuText == null || !menuText.Attributes.TryGetValue(key, out value))
			{
				return true;
			}
			if (obj != value)
			{
				return true;
			}
			if (_menuTextAttributeStrings[key] != value.ToString())
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateMenuContext(MenuContext newMenuContext)
	{
		MenuContext = newMenuContext;
		Refresh(forceUpdateItems: true);
	}

	public override void OnFinalize()
	{
		CampaignEvents.ItemsLooted.ClearListeners(this);
		Game.Current.EventManager.UnregisterEvent<TutorialNotificationElementChangeEvent>(OnTutorialNotificationElementIDChange);
		_gameMenuManager = null;
		MenuContext = null;
		ItemList.ApplyActionOnAllItems(delegate(GameMenuItemVM x)
		{
			x.OnFinalize();
		});
		ItemList.Clear();
		ItemList = null;
	}

	public void AddHotKey(GameMenuOption.LeaveType leaveType, GameKey gameKey)
	{
		if (_shortcutKeys.ContainsKey(leaveType))
		{
			_shortcutKeys[leaveType] = gameKey;
		}
		else
		{
			_shortcutKeys.Add(leaveType, gameKey);
		}
	}

	private void OnItemsPlundered(MobileParty mobileParty, ItemRoster newItems)
	{
		if (mobileParty != MobileParty.MainParty)
		{
			return;
		}
		foreach (ItemRosterElement newItem in newItems)
		{
			for (int i = 0; i < newItem.Amount; i++)
			{
				PlunderItems.Add(new GameMenuPlunderItemVM(newItem));
			}
		}
	}

	public void ExecuteLink(string link)
	{
		Campaign.Current.EncyclopediaManager.GoToLink(link);
	}

	protected virtual GameMenuItemVM CreateGameMenuItemVM(int indexOfMenuCondition)
	{
		GameMenuOption virtualGameMenuOption = _gameMenuManager.GetVirtualGameMenuOption(MenuContext, indexOfMenuCondition);
		GameKey shortcutKey = (_shortcutKeys.ContainsKey(virtualGameMenuOption.OptionLeaveType) ? _shortcutKeys[virtualGameMenuOption.OptionLeaveType] : null);
		return new GameMenuItemVM(MenuContext, indexOfMenuCondition, TextObject.Empty, TextObject.Empty, TextObject.Empty, TaleWorlds.CampaignSystem.GameMenus.GameMenu.MenuAndOptionType.RegularMenuOption, virtualGameMenuOption, shortcutKey);
	}

	private void OnTutorialNotificationElementIDChange(TutorialNotificationElementChangeEvent obj)
	{
		if (obj.NewNotificationElementID != _latestTutorialElementID)
		{
			_latestTutorialElementID = obj.NewNotificationElementID;
		}
		if (_latestTutorialElementID != null)
		{
			if (_latestTutorialElementID != string.Empty)
			{
				if (_latestTutorialElementID == "town_backstreet" && !_isTavernButtonHighlightApplied)
				{
					_isTavernButtonHighlightApplied = SetGameMenuButtonHighlightState(_latestTutorialElementID, state: true);
				}
				else if (_latestTutorialElementID != "town_backstreet" && _isTavernButtonHighlightApplied)
				{
					_isTavernButtonHighlightApplied = !SetGameMenuButtonHighlightState("town_backstreet", state: false);
				}
				if (_latestTutorialElementID == "sell_all_prisoners" && !_isSellPrisonerButtonHighlightApplied)
				{
					_isSellPrisonerButtonHighlightApplied = SetGameMenuButtonHighlightState(_latestTutorialElementID, state: true);
				}
				else if (_latestTutorialElementID != "sell_all_prisoners" && _isSellPrisonerButtonHighlightApplied)
				{
					_isSellPrisonerButtonHighlightApplied = !SetGameMenuButtonHighlightState("sell_all_prisoners", state: false);
				}
				if (_latestTutorialElementID == "storymode_tutorial_village_buy" && !_isShopButtonHighlightApplied)
				{
					_isShopButtonHighlightApplied = SetGameMenuButtonHighlightState(_latestTutorialElementID, state: true);
				}
				else if (_latestTutorialElementID != "storymode_tutorial_village_buy" && _isShopButtonHighlightApplied)
				{
					_isShopButtonHighlightApplied = !SetGameMenuButtonHighlightState("storymode_tutorial_village_buy", state: false);
				}
				if (_latestTutorialElementID == "storymode_tutorial_village_recruit" && !_isRecruitButtonHighlightApplied)
				{
					_isRecruitButtonHighlightApplied = SetGameMenuButtonHighlightState(_latestTutorialElementID, state: true);
				}
				else if (_latestTutorialElementID != "storymode_tutorial_village_recruit" && _isRecruitButtonHighlightApplied)
				{
					_isRecruitButtonHighlightApplied = !SetGameMenuButtonHighlightState("storymode_tutorial_village_recruit", state: false);
				}
				if (_latestTutorialElementID == "hostile_action" && !_isHostileActionButtonHighlightApplied)
				{
					_isHostileActionButtonHighlightApplied = SetGameMenuButtonHighlightState(_latestTutorialElementID, state: true);
				}
				else if (_latestTutorialElementID != "hostile_action" && _isHostileActionButtonHighlightApplied)
				{
					_isHostileActionButtonHighlightApplied = !SetGameMenuButtonHighlightState("hostile_action", state: false);
				}
				if (_latestTutorialElementID == "town_besiege" && !_isTownBesiegeButtonHighlightApplied)
				{
					_isTownBesiegeButtonHighlightApplied = SetGameMenuButtonHighlightState(_latestTutorialElementID, state: true);
				}
				else if (_latestTutorialElementID != "town_besiege" && _isTownBesiegeButtonHighlightApplied)
				{
					_isTownBesiegeButtonHighlightApplied = !SetGameMenuButtonHighlightState("town_besiege", state: false);
				}
				if (_latestTutorialElementID == "storymode_tutorial_village_enter" && !_isEnterTutorialVillageButtonHighlightApplied)
				{
					_isEnterTutorialVillageButtonHighlightApplied = SetGameMenuButtonHighlightState(_latestTutorialElementID, state: true);
				}
				else if (_latestTutorialElementID != "storymode_tutorial_village_enter" && _isEnterTutorialVillageButtonHighlightApplied)
				{
					_isEnterTutorialVillageButtonHighlightApplied = !SetGameMenuButtonHighlightState("storymode_tutorial_village_enter", state: false);
				}
			}
			else
			{
				if (_isTavernButtonHighlightApplied)
				{
					_isTavernButtonHighlightApplied = !SetGameMenuButtonHighlightState("town_backstreet", state: false);
				}
				if (_isSellPrisonerButtonHighlightApplied)
				{
					_isSellPrisonerButtonHighlightApplied = !SetGameMenuButtonHighlightState("sell_all_prisoners", state: false);
				}
				if (_isShopButtonHighlightApplied)
				{
					_isShopButtonHighlightApplied = !SetGameMenuButtonHighlightState("storymode_tutorial_village_buy", state: false);
				}
				if (_isRecruitButtonHighlightApplied)
				{
					_isRecruitButtonHighlightApplied = !SetGameMenuButtonHighlightState("storymode_tutorial_village_recruit", state: false);
				}
				if (_isHostileActionButtonHighlightApplied)
				{
					_isHostileActionButtonHighlightApplied = !SetGameMenuButtonHighlightState("hostile_action", state: false);
				}
				if (_isTownBesiegeButtonHighlightApplied)
				{
					_isTownBesiegeButtonHighlightApplied = !SetGameMenuButtonHighlightState("town_besiege", state: false);
				}
				if (_isEnterTutorialVillageButtonHighlightApplied)
				{
					_isEnterTutorialVillageButtonHighlightApplied = !SetGameMenuButtonHighlightState("storymode_tutorial_village_enter", state: false);
				}
			}
		}
		else
		{
			if (_isTavernButtonHighlightApplied)
			{
				_isTavernButtonHighlightApplied = !SetGameMenuButtonHighlightState("town_backstreet", state: false);
			}
			if (_isSellPrisonerButtonHighlightApplied)
			{
				_isSellPrisonerButtonHighlightApplied = !SetGameMenuButtonHighlightState("sell_all_prisoners", state: false);
			}
			if (_isShopButtonHighlightApplied)
			{
				_isShopButtonHighlightApplied = !SetGameMenuButtonHighlightState("storymode_tutorial_village_buy", state: false);
			}
			if (_isRecruitButtonHighlightApplied)
			{
				_isRecruitButtonHighlightApplied = !SetGameMenuButtonHighlightState("storymode_tutorial_village_recruit", state: false);
			}
			if (_isHostileActionButtonHighlightApplied)
			{
				_isHostileActionButtonHighlightApplied = !SetGameMenuButtonHighlightState("hostile_action", state: false);
			}
			if (_isTownBesiegeButtonHighlightApplied)
			{
				_isTownBesiegeButtonHighlightApplied = !SetGameMenuButtonHighlightState("town_besiege", state: false);
			}
			if (_isEnterTutorialVillageButtonHighlightApplied)
			{
				_isEnterTutorialVillageButtonHighlightApplied = !SetGameMenuButtonHighlightState("storymode_tutorial_village_enter", state: false);
			}
		}
	}

	private bool SetGameMenuButtonHighlightState(string buttonID, bool state)
	{
		foreach (GameMenuItemVM item in ItemList)
		{
			if (item.OptionID == buttonID)
			{
				item.IsHighlightEnabled = state;
				return true;
			}
		}
		return false;
	}
}
