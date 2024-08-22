using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;

public class MapNavigationVM : ViewModel
{
	private INavigationHandler _navigationHandler;

	private Func<MapBarShortcuts> _getMapBarShortcuts;

	private MapBarShortcuts _shortcuts;

	private bool _latestIsGamepadActive;

	private bool _latestIsKingdomEnabled;

	private bool _latestIsClanEnabled;

	private readonly IViewDataTracker _viewDataTracker;

	private string _alertText;

	private bool _skillAlert;

	private bool _questsAlert;

	private bool _partyAlert;

	private bool _kingdomAlert;

	private bool _clanAlert;

	private bool _inventoryAlert;

	private bool _isKingdomEnabled;

	private bool _isClanEnabled;

	private bool _isQuestsEnabled;

	private bool _isEscapeMenuEnabled;

	private bool _isInventoryEnabled;

	private bool _isCharacterDeveloperEnabled;

	private bool _isPartyEnabled;

	private bool _isKingdomActive;

	private bool _isClanActive;

	private bool _isEscapeMenuActive;

	private bool _isQuestsActive;

	private bool _isInventoryActive;

	private bool _isCharacterDeveloperActive;

	private bool _isPartyActive;

	private HintViewModel _encyclopediaHint;

	private BasicTooltipViewModel _skillsHint;

	private BasicTooltipViewModel _escapeMenuHint;

	private BasicTooltipViewModel _questsHint;

	private BasicTooltipViewModel _inventoryHint;

	private BasicTooltipViewModel _partyHint;

	private HintViewModel _financeHint;

	private HintViewModel _centerCameraHint;

	private HintViewModel _campHint;

	private BasicTooltipViewModel _kingdomHint;

	private BasicTooltipViewModel _clanHint;

	private BasicTooltipViewModel _characterAlertHint;

	private BasicTooltipViewModel _questAlertHint;

	private BasicTooltipViewModel _partyAlertHint;

	[DataSourceProperty]
	public BasicTooltipViewModel PartyAlertHint
	{
		get
		{
			return _partyAlertHint;
		}
		set
		{
			if (value != _partyAlertHint)
			{
				_partyAlertHint = value;
				OnPropertyChangedWithValue(value, "PartyAlertHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel CharacterAlertHint
	{
		get
		{
			return _characterAlertHint;
		}
		set
		{
			if (value != _characterAlertHint)
			{
				_characterAlertHint = value;
				OnPropertyChangedWithValue(value, "CharacterAlertHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel QuestAlertHint
	{
		get
		{
			return _questAlertHint;
		}
		set
		{
			if (value != _questAlertHint)
			{
				_questAlertHint = value;
				OnPropertyChangedWithValue(value, "QuestAlertHint");
			}
		}
	}

	[DataSourceProperty]
	public string AlertText
	{
		get
		{
			return _alertText;
		}
		set
		{
			if (value != _alertText)
			{
				_alertText = value;
				OnPropertyChangedWithValue(value, "AlertText");
			}
		}
	}

	[DataSourceProperty]
	public bool SkillAlert
	{
		get
		{
			return _skillAlert;
		}
		set
		{
			if (value != _skillAlert)
			{
				_skillAlert = value;
				OnPropertyChangedWithValue(value, "SkillAlert");
			}
		}
	}

	[DataSourceProperty]
	public bool QuestsAlert
	{
		get
		{
			return _questsAlert;
		}
		set
		{
			if (value != _questsAlert)
			{
				_questsAlert = value;
				OnPropertyChangedWithValue(value, "QuestsAlert");
			}
		}
	}

	[DataSourceProperty]
	public bool PartyAlert
	{
		get
		{
			return _partyAlert;
		}
		set
		{
			if (value != _partyAlert)
			{
				_partyAlert = value;
				OnPropertyChangedWithValue(value, "PartyAlert");
			}
		}
	}

	[DataSourceProperty]
	public bool KingdomAlert
	{
		get
		{
			return _kingdomAlert;
		}
		set
		{
			if (value != _kingdomAlert)
			{
				_kingdomAlert = value;
				OnPropertyChangedWithValue(value, "KingdomAlert");
			}
		}
	}

	[DataSourceProperty]
	public bool ClanAlert
	{
		get
		{
			return _clanAlert;
		}
		set
		{
			if (value != _clanAlert)
			{
				_clanAlert = value;
				OnPropertyChangedWithValue(value, "ClanAlert");
			}
		}
	}

	[DataSourceProperty]
	public bool InventoryAlert
	{
		get
		{
			return _inventoryAlert;
		}
		set
		{
			if (value != _inventoryAlert)
			{
				_inventoryAlert = value;
				OnPropertyChangedWithValue(value, "InventoryAlert");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEscapeMenuEnabled
	{
		get
		{
			return _isEscapeMenuEnabled;
		}
		set
		{
			if (value != _isEscapeMenuEnabled)
			{
				_isEscapeMenuEnabled = value;
				OnPropertyChangedWithValue(value, "IsEscapeMenuEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsKingdomEnabled
	{
		get
		{
			return _isKingdomEnabled;
		}
		set
		{
			if (value != _isKingdomEnabled)
			{
				_isKingdomEnabled = value;
				OnPropertyChangedWithValue(value, "IsKingdomEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPartyEnabled
	{
		get
		{
			return _isPartyEnabled;
		}
		set
		{
			if (value != _isPartyEnabled)
			{
				_isPartyEnabled = value;
				OnPropertyChangedWithValue(value, "IsPartyEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInventoryEnabled
	{
		get
		{
			return _isInventoryEnabled;
		}
		set
		{
			if (value != _isInventoryEnabled)
			{
				_isInventoryEnabled = value;
				OnPropertyChangedWithValue(value, "IsInventoryEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsQuestsEnabled
	{
		get
		{
			return _isQuestsEnabled;
		}
		set
		{
			if (value != _isQuestsEnabled)
			{
				_isQuestsEnabled = value;
				OnPropertyChangedWithValue(value, "IsQuestsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCharacterDeveloperEnabled
	{
		get
		{
			return _isCharacterDeveloperEnabled;
		}
		set
		{
			if (value != _isCharacterDeveloperEnabled)
			{
				_isCharacterDeveloperEnabled = value;
				OnPropertyChangedWithValue(value, "IsCharacterDeveloperEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsClanEnabled
	{
		get
		{
			return _isClanEnabled;
		}
		set
		{
			if (value != _isClanEnabled)
			{
				_isClanEnabled = value;
				OnPropertyChangedWithValue(value, "IsClanEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsKingdomActive
	{
		get
		{
			return _isKingdomActive;
		}
		set
		{
			if (value != _isKingdomActive)
			{
				_isKingdomActive = value;
				OnPropertyChangedWithValue(value, "IsKingdomActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPartyActive
	{
		get
		{
			return _isPartyActive;
		}
		set
		{
			if (value != _isPartyActive)
			{
				_isPartyActive = value;
				OnPropertyChangedWithValue(value, "IsPartyActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInventoryActive
	{
		get
		{
			return _isInventoryActive;
		}
		set
		{
			if (value != _isInventoryActive)
			{
				_isInventoryActive = value;
				OnPropertyChangedWithValue(value, "IsInventoryActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsQuestsActive
	{
		get
		{
			return _isQuestsActive;
		}
		set
		{
			if (value != _isQuestsActive)
			{
				_isQuestsActive = value;
				OnPropertyChangedWithValue(value, "IsQuestsActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCharacterDeveloperActive
	{
		get
		{
			return _isCharacterDeveloperActive;
		}
		set
		{
			if (value != _isCharacterDeveloperActive)
			{
				_isCharacterDeveloperActive = value;
				OnPropertyChangedWithValue(value, "IsCharacterDeveloperActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsClanActive
	{
		get
		{
			return _isClanActive;
		}
		set
		{
			if (value != _isClanActive)
			{
				_isClanActive = value;
				OnPropertyChangedWithValue(value, "IsClanActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEscapeMenuActive
	{
		get
		{
			return _isEscapeMenuActive;
		}
		set
		{
			if (value != _isEscapeMenuActive)
			{
				_isEscapeMenuActive = value;
				OnPropertyChangedWithValue(value, "IsEscapeMenuActive");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel FinanceHint
	{
		get
		{
			return _financeHint;
		}
		set
		{
			if (value != _financeHint)
			{
				_financeHint = value;
				OnPropertyChangedWithValue(value, "FinanceHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel EncyclopediaHint
	{
		get
		{
			return _encyclopediaHint;
		}
		set
		{
			if (value != _encyclopediaHint)
			{
				_encyclopediaHint = value;
				OnPropertyChangedWithValue(value, "EncyclopediaHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel EscapeMenuHint
	{
		get
		{
			return _escapeMenuHint;
		}
		set
		{
			if (value != _escapeMenuHint)
			{
				_escapeMenuHint = value;
				OnPropertyChangedWithValue(value, "EscapeMenuHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel SkillsHint
	{
		get
		{
			return _skillsHint;
		}
		set
		{
			if (value != _skillsHint)
			{
				_skillsHint = value;
				OnPropertyChangedWithValue(value, "SkillsHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel QuestsHint
	{
		get
		{
			return _questsHint;
		}
		set
		{
			if (value != _questsHint)
			{
				_questsHint = value;
				OnPropertyChangedWithValue(value, "QuestsHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel InventoryHint
	{
		get
		{
			return _inventoryHint;
		}
		set
		{
			if (value != _inventoryHint)
			{
				_inventoryHint = value;
				OnPropertyChangedWithValue(value, "InventoryHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel PartyHint
	{
		get
		{
			return _partyHint;
		}
		set
		{
			if (value != _partyHint)
			{
				_partyHint = value;
				OnPropertyChangedWithValue(value, "PartyHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel KingdomHint
	{
		get
		{
			return _kingdomHint;
		}
		set
		{
			if (value != _kingdomHint)
			{
				_kingdomHint = value;
				OnPropertyChangedWithValue(value, "KingdomHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel ClanHint
	{
		get
		{
			return _clanHint;
		}
		set
		{
			if (value != _clanHint)
			{
				_clanHint = value;
				OnPropertyChangedWithValue(value, "ClanHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CenterCameraHint
	{
		get
		{
			return _centerCameraHint;
		}
		set
		{
			if (value != _centerCameraHint)
			{
				_centerCameraHint = value;
				OnPropertyChangedWithValue(value, "CenterCameraHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CampHint
	{
		get
		{
			return _campHint;
		}
		set
		{
			if (value != _campHint)
			{
				_campHint = value;
				OnPropertyChangedWithValue(value, "CampHint");
			}
		}
	}

	public MapNavigationVM(INavigationHandler navigationHandler, Func<MapBarShortcuts> getMapBarShortcuts)
	{
		_navigationHandler = navigationHandler;
		_getMapBarShortcuts = getMapBarShortcuts;
		_viewDataTracker = Campaign.Current.GetCampaignBehavior<IViewDataTracker>();
		IsKingdomEnabled = Hero.MainHero.MapFaction?.IsKingdomFaction ?? false;
		IsPartyEnabled = true;
		IsInventoryEnabled = true;
		IsClanEnabled = true;
		IsCharacterDeveloperEnabled = true;
		IsQuestsEnabled = true;
		IsKingdomActive = false;
		IsPartyActive = false;
		IsInventoryActive = false;
		IsClanActive = false;
		IsCharacterDeveloperActive = false;
		IsQuestsActive = false;
		SkillsHint = new BasicTooltipViewModel();
		EscapeMenuHint = new BasicTooltipViewModel();
		QuestsHint = new BasicTooltipViewModel();
		InventoryHint = new BasicTooltipViewModel();
		PartyHint = new BasicTooltipViewModel();
		KingdomHint = new BasicTooltipViewModel();
		ClanHint = new BasicTooltipViewModel();
		CharacterAlertHint = new BasicTooltipViewModel();
		QuestAlertHint = new BasicTooltipViewModel();
		PartyAlertHint = new BasicTooltipViewModel();
		TaleWorlds.InputSystem.Input.OnGamepadActiveStateChanged = (Action)Delegate.Combine(TaleWorlds.InputSystem.Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
		RefreshValues();
	}

	private void OnGamepadActiveStateChanged()
	{
		RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		_shortcuts = _getMapBarShortcuts();
		EncyclopediaHint = new HintViewModel(GameTexts.FindText("str_encyclopedia"));
		if (TaleWorlds.InputSystem.Input.IsGamepadActive)
		{
			SkillsHint.SetHintCallback(() => GameTexts.FindText("str_character").ToString());
			EscapeMenuHint.SetHintCallback(() => GameTexts.FindText("str_escape_menu").ToString());
			QuestsHint.SetHintCallback(() => GameTexts.FindText("str_quest").ToString());
			InventoryHint.SetHintCallback(() => GameTexts.FindText("str_inventory").ToString());
			PartyHint.SetHintCallback(() => GameTexts.FindText("str_party").ToString());
			KingdomHint.SetHintCallback(() => GameTexts.FindText("str_kingdom").ToString());
			ClanHint.SetHintCallback(() => GameTexts.FindText("str_clan").ToString());
		}
		else
		{
			SkillsHint.SetHintCallback(delegate
			{
				GameTexts.SetVariable("TEXT", GameTexts.FindText("str_character").ToString());
				GameTexts.SetVariable("HOTKEY", _shortcuts.CharacterHotkey);
				return GameTexts.FindText("str_hotkey_with_hint").ToString();
			});
			EscapeMenuHint.SetHintCallback(delegate
			{
				GameTexts.SetVariable("TEXT", GameTexts.FindText("str_escape_menu"));
				GameTexts.SetVariable("HOTKEY", _shortcuts.EscapeMenuHotkey);
				return GameTexts.FindText("str_hotkey_with_hint").ToString();
			});
			QuestsHint.SetHintCallback(delegate
			{
				GameTexts.SetVariable("TEXT", GameTexts.FindText("str_quest").ToString());
				GameTexts.SetVariable("HOTKEY", _shortcuts.QuestHotkey);
				return GameTexts.FindText("str_hotkey_with_hint").ToString();
			});
			InventoryHint.SetHintCallback(delegate
			{
				GameTexts.SetVariable("TEXT", GameTexts.FindText("str_inventory").ToString());
				GameTexts.SetVariable("HOTKEY", _shortcuts.InventoryHotkey);
				return GameTexts.FindText("str_hotkey_with_hint").ToString();
			});
			PartyHint.SetHintCallback(delegate
			{
				GameTexts.SetVariable("TEXT", GameTexts.FindText("str_party").ToString());
				GameTexts.SetVariable("HOTKEY", _shortcuts.PartyHotkey);
				return GameTexts.FindText("str_hotkey_with_hint").ToString();
			});
		}
		CampHint = new HintViewModel(GameTexts.FindText("str_camp"));
		FinanceHint = new HintViewModel(GameTexts.FindText("str_finance"));
		CenterCameraHint = new HintViewModel(GameTexts.FindText("str_return_to_hero"));
		IFaction mapFaction = Hero.MainHero.MapFaction;
		if (mapFaction != null && mapFaction.IsKingdomFaction)
		{
			KingdomHint.SetHintCallback(() => GameTexts.FindText("str_kingdom").ToString());
		}
		else
		{
			KingdomHint.SetHintCallback(() => GameTexts.FindText("str_need_to_be_a_part_of_kingdom").ToString());
		}
		AlertText = GameTexts.FindText("str_map_bar_alert").ToString();
		CharacterAlertHint.SetHintCallback(() => _viewDataTracker.GetCharacterNotificationText());
		QuestAlertHint.SetHintCallback(() => _viewDataTracker.GetQuestNotificationText());
		PartyAlertHint.SetHintCallback(() => _viewDataTracker.GetPartyNotificationText());
		Refresh();
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		TaleWorlds.InputSystem.Input.OnGamepadActiveStateChanged = (Action)Delegate.Remove(TaleWorlds.InputSystem.Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
		_navigationHandler = null;
		_getMapBarShortcuts = null;
	}

	public void Refresh()
	{
		RefreshAlertValues();
		_viewDataTracker.UpdatePartyNotification();
	}

	public void Tick()
	{
		RefreshPermissionValues();
		RefreshStates();
	}

	private void RefreshPermissionValues()
	{
		NavigationPermissionItem kingdomPermission = _navigationHandler.KingdomPermission;
		IsKingdomEnabled = kingdomPermission.IsAuthorized;
		NavigationPermissionItem clanPermission = _navigationHandler.ClanPermission;
		IsClanEnabled = clanPermission.IsAuthorized;
		bool flag = false;
		bool flag2 = false;
		if (_latestIsKingdomEnabled != IsKingdomEnabled)
		{
			flag = true;
			_latestIsKingdomEnabled = IsKingdomEnabled;
		}
		if (_latestIsClanEnabled != IsClanEnabled)
		{
			flag2 = true;
			_latestIsClanEnabled = IsClanEnabled;
		}
		if (_latestIsGamepadActive != TaleWorlds.InputSystem.Input.IsGamepadActive)
		{
			flag = true;
			flag2 = true;
			_latestIsGamepadActive = TaleWorlds.InputSystem.Input.IsGamepadActive;
		}
		if (flag2)
		{
			UpdateClanHint(clanPermission);
		}
		if (flag)
		{
			UpdateKingdomHint(kingdomPermission);
		}
	}

	private void UpdateKingdomHint(NavigationPermissionItem kingdomPermission)
	{
		if (IsKingdomEnabled)
		{
			if (TaleWorlds.InputSystem.Input.IsGamepadActive)
			{
				KingdomHint.SetHintCallback(() => GameTexts.FindText("str_kingdom").ToString());
				return;
			}
			KingdomHint.SetHintCallback(delegate
			{
				GameTexts.SetVariable("TEXT", GameTexts.FindText("str_kingdom").ToString());
				GameTexts.SetVariable("HOTKEY", _shortcuts.KingdomHotkey);
				return GameTexts.FindText("str_hotkey_with_hint").ToString();
			});
		}
		else
		{
			KingdomHint.SetHintCallback(() => kingdomPermission.ReasonString?.ToString());
		}
	}

	private void UpdateClanHint(NavigationPermissionItem clanPermission)
	{
		if (IsClanEnabled)
		{
			if (TaleWorlds.InputSystem.Input.IsGamepadActive)
			{
				ClanHint.SetHintCallback(() => GameTexts.FindText("str_clan").ToString());
				return;
			}
			ClanHint.SetHintCallback(delegate
			{
				GameTexts.SetVariable("TEXT", GameTexts.FindText("str_clan").ToString());
				GameTexts.SetVariable("HOTKEY", _shortcuts.ClanHotkey);
				return GameTexts.FindText("str_hotkey_with_hint").ToString();
			});
		}
		else
		{
			ClanHint.SetHintCallback(() => clanPermission.ReasonString?.ToString());
		}
	}

	private void RefreshAlertValues()
	{
		QuestsAlert = _viewDataTracker.IsQuestNotificationActive;
		SkillAlert = _viewDataTracker.IsCharacterNotificationActive;
		PartyAlert = _viewDataTracker.IsPartyNotificationActive;
	}

	private void RefreshStates()
	{
		IsPartyEnabled = _navigationHandler.PartyEnabled;
		IsInventoryEnabled = _navigationHandler.InventoryEnabled;
		IsCharacterDeveloperEnabled = _navigationHandler.CharacterDeveloperEnabled;
		IsQuestsEnabled = _navigationHandler.QuestsEnabled;
		IsEscapeMenuEnabled = _navigationHandler.EscapeMenuEnabled;
		IsKingdomActive = _navigationHandler.KingdomActive;
		IsPartyActive = _navigationHandler.PartyActive;
		IsInventoryActive = _navigationHandler.InventoryActive;
		IsClanActive = _navigationHandler.ClanActive;
		IsCharacterDeveloperActive = _navigationHandler.CharacterDeveloperActive;
		IsQuestsActive = _navigationHandler.QuestsActive;
		IsEscapeMenuActive = _navigationHandler.EscapeMenuActive;
	}

	public void ExecuteOpenQuests()
	{
		_navigationHandler.OpenQuests();
	}

	public void ExecuteOpenInventory()
	{
		_navigationHandler.OpenInventory();
	}

	public void ExecuteOpenParty()
	{
		_navigationHandler.OpenParty();
	}

	public void ExecuteOpenCharacterDeveloper()
	{
		_navigationHandler.OpenCharacterDeveloper();
	}

	public void ExecuteOpenKingdom()
	{
		_navigationHandler.OpenKingdom();
	}

	public void ExecuteOpenClan()
	{
		_navigationHandler.OpenClan();
	}

	public void ExecuteOpenEscapeMenu()
	{
		_navigationHandler.OpenEscapeMenu();
	}

	public void ExecuteOpenMainHeroEncyclopedia()
	{
		Campaign.Current.EncyclopediaManager.GoToLink(Hero.MainHero.EncyclopediaLink);
	}

	public void ExecuteOpenMainHeroClanEncyclopedia()
	{
		Campaign.Current.EncyclopediaManager.GoToLink(Hero.MainHero.Clan.EncyclopediaLink);
	}

	public void ExecuteOpenMainHeroKingdomEncyclopedia()
	{
		if (Hero.MainHero.MapFaction != null)
		{
			Campaign.Current.EncyclopediaManager.GoToLink(Hero.MainHero.MapFaction.EncyclopediaLink);
		}
	}
}
