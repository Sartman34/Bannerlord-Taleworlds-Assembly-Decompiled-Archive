using System;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Tutorial;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;

public class MapBarVM : ViewModel
{
	private IMapStateHandler _mapStateHandler;

	private readonly TextObject _needToBePartOfKingdomText;

	private readonly TextObject _cannotGatherWhileInEventText;

	private readonly TextObject _needToBeLeaderToManageText;

	private readonly TextObject _mercenaryCannotManageText;

	private readonly TextObject _cannotGatherWhileInConversationText;

	private readonly TextObject _cannotGatherWhileInSiegeText;

	private readonly Action _openArmyManagement;

	private float _refreshTimeSpan;

	private string _latestTutorialElementID;

	private bool _isGatherArmyVisible;

	private MapInfoVM _mapInfo;

	private MapTimeControlVM _mapTimeControl;

	private MapNavigationVM _mapNavigation;

	private bool _isEnabled;

	private bool _isCameraCentered;

	private bool _canGatherArmy;

	private bool _isInInfoMode;

	private string _currentScreen;

	private HintViewModel _gatherArmyHint;

	private ElementNotificationVM _tutorialNotification;

	[DataSourceProperty]
	public MapInfoVM MapInfo
	{
		get
		{
			return _mapInfo;
		}
		set
		{
			if (value != _mapInfo)
			{
				_mapInfo = value;
				OnPropertyChangedWithValue(value, "MapInfo");
			}
		}
	}

	[DataSourceProperty]
	public MapTimeControlVM MapTimeControl
	{
		get
		{
			return _mapTimeControl;
		}
		set
		{
			if (value != _mapTimeControl)
			{
				_mapTimeControl = value;
				OnPropertyChangedWithValue(value, "MapTimeControl");
			}
		}
	}

	[DataSourceProperty]
	public MapNavigationVM MapNavigation
	{
		get
		{
			return _mapNavigation;
		}
		set
		{
			if (value != _mapNavigation)
			{
				_mapNavigation = value;
				OnPropertyChangedWithValue(value, "MapNavigation");
			}
		}
	}

	[DataSourceProperty]
	public bool IsGatherArmyVisible
	{
		get
		{
			return _isGatherArmyVisible;
		}
		set
		{
			if (value != _isGatherArmyVisible)
			{
				_isGatherArmyVisible = value;
				OnPropertyChangedWithValue(value, "IsGatherArmyVisible");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInInfoMode
	{
		get
		{
			return _isInInfoMode;
		}
		set
		{
			if (value != _isInInfoMode)
			{
				_isInInfoMode = value;
				OnPropertyChangedWithValue(value, "IsInInfoMode");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value != _isEnabled)
			{
				_isEnabled = value;
				OnPropertyChangedWithValue(value, "IsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool CanGatherArmy
	{
		get
		{
			return _canGatherArmy;
		}
		set
		{
			if (value != _canGatherArmy)
			{
				_canGatherArmy = value;
				OnPropertyChangedWithValue(value, "CanGatherArmy");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel GatherArmyHint
	{
		get
		{
			return _gatherArmyHint;
		}
		set
		{
			if (value != _gatherArmyHint)
			{
				_gatherArmyHint = value;
				OnPropertyChangedWithValue(value, "GatherArmyHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCameraCentered
	{
		get
		{
			return _isCameraCentered;
		}
		set
		{
			if (value != _isCameraCentered)
			{
				_isCameraCentered = value;
				OnPropertyChangedWithValue(value, "IsCameraCentered");
			}
		}
	}

	[DataSourceProperty]
	public string CurrentScreen
	{
		get
		{
			return _currentScreen;
		}
		set
		{
			if (_currentScreen != value)
			{
				_currentScreen = value;
				OnPropertyChangedWithValue(value, "CurrentScreen");
			}
		}
	}

	[DataSourceProperty]
	public ElementNotificationVM TutorialNotification
	{
		get
		{
			return _tutorialNotification;
		}
		set
		{
			if (value != _tutorialNotification)
			{
				_tutorialNotification = value;
				OnPropertyChangedWithValue(value, "TutorialNotification");
			}
		}
	}

	public MapBarVM(INavigationHandler navigationHandler, IMapStateHandler mapStateHandler, Func<MapBarShortcuts> getMapBarShortcuts, Action openArmyManagement)
	{
		_openArmyManagement = openArmyManagement;
		_mapStateHandler = mapStateHandler;
		_refreshTimeSpan = ((Campaign.Current.GetSimplifiedTimeControlMode() == CampaignTimeControlMode.UnstoppableFastForward) ? 0.1f : 2f);
		_needToBePartOfKingdomText = GameTexts.FindText("str_need_to_be_a_part_of_kingdom");
		_cannotGatherWhileInEventText = GameTexts.FindText("str_cannot_gather_army_while_in_event");
		_needToBeLeaderToManageText = GameTexts.FindText("str_need_to_be_leader_of_army_to_manage");
		_mercenaryCannotManageText = GameTexts.FindText("str_mercenary_cannot_manage_army");
		_cannotGatherWhileInConversationText = GameTexts.FindText("str_cannot_gather_army_during_conversation");
		_cannotGatherWhileInSiegeText = GameTexts.FindText("str_cannot_gather_army_during_siege");
		TutorialNotification = new ElementNotificationVM();
		MapInfo = new MapInfoVM();
		MapTimeControl = new MapTimeControlVM(getMapBarShortcuts, OnTimeControlChange, delegate
		{
			mapStateHandler.ResetCamera(resetDistance: false, teleportToMainParty: false);
		});
		MapNavigation = new MapNavigationVM(navigationHandler, getMapBarShortcuts);
		GatherArmyHint = new HintViewModel();
		OnRefresh();
		IsEnabled = true;
		Game.Current.EventManager.RegisterEvent<TutorialNotificationElementChangeEvent>(OnTutorialNotificationElementIDChange);
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		MapInfo.RefreshValues();
		MapTimeControl.RefreshValues();
		MapNavigation.RefreshValues();
	}

	public void OnRefresh()
	{
		MapInfo.Refresh();
		MapTimeControl.Refresh();
		MapNavigation.Refresh();
	}

	public void Tick(float dt)
	{
		int simplifiedTimeControlMode = (int)Campaign.Current.GetSimplifiedTimeControlMode();
		_refreshTimeSpan -= dt;
		if (_refreshTimeSpan < 0f)
		{
			OnRefresh();
			_refreshTimeSpan = ((simplifiedTimeControlMode == 2) ? 0.1f : 0.2f);
		}
		MapInfo.Tick();
		MapTimeControl.Tick();
		MapNavigation.Tick();
		if (_mapStateHandler != null)
		{
			IsCameraCentered = _mapStateHandler.IsCameraLockedToPlayerParty();
		}
		IsGatherArmyVisible = GetIsGatherArmyVisible();
		if (IsGatherArmyVisible)
		{
			UpdateCanGatherArmyAndReason();
		}
	}

	private void UpdateCanGatherArmyAndReason()
	{
		bool canGatherArmy = true;
		TextObject textObject = null;
		IFaction mapFaction = Hero.MainHero.MapFaction;
		if (mapFaction != null && !mapFaction.IsKingdomFaction)
		{
			textObject = _needToBePartOfKingdomText;
			canGatherArmy = false;
		}
		else if (MobileParty.MainParty.MapEvent != null)
		{
			textObject = _cannotGatherWhileInEventText;
			canGatherArmy = false;
		}
		else if (MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty)
		{
			textObject = _needToBeLeaderToManageText;
			canGatherArmy = false;
		}
		else if (Clan.PlayerClan.IsUnderMercenaryService)
		{
			textObject = _mercenaryCannotManageText;
			canGatherArmy = false;
		}
		else if (PlayerEncounter.Current != null && PlayerEncounter.EncounterSettlement == null)
		{
			textObject = _cannotGatherWhileInConversationText;
			canGatherArmy = false;
		}
		else if (PlayerSiege.PlayerSiegeEvent != null)
		{
			textObject = _cannotGatherWhileInSiegeText;
			canGatherArmy = false;
		}
		CanGatherArmy = canGatherArmy;
		GatherArmyHint.HintText = (CanGatherArmy ? TextObject.Empty : textObject);
	}

	private bool GetIsGatherArmyVisible()
	{
		if (MapTimeControl.IsInMap && MobileParty.MainParty?.Army == null && !Hero.MainHero.IsPrisoner && Hero.MainHero.PartyBelongedTo != null && MobileParty.MainParty.MapEvent == null)
		{
			return MapTimeControl.IsCenterPanelEnabled;
		}
		return false;
	}

	private void OnTimeControlChange()
	{
		_refreshTimeSpan = ((Campaign.Current.GetSimplifiedTimeControlMode() == CampaignTimeControlMode.UnstoppableFastForward) ? 0.1f : 2f);
	}

	private void ExecuteResetCamera()
	{
		_mapStateHandler?.FastMoveCameraToMainParty();
	}

	public void ExecuteArmyManagement()
	{
		_openArmyManagement();
	}

	private void OnTutorialNotificationElementIDChange(TutorialNotificationElementChangeEvent obj)
	{
		if (!(obj.NewNotificationElementID != _latestTutorialElementID))
		{
			return;
		}
		if (_latestTutorialElementID != null)
		{
			TutorialNotification.ElementID = string.Empty;
		}
		_latestTutorialElementID = obj.NewNotificationElementID;
		if (_latestTutorialElementID != null)
		{
			TutorialNotification.ElementID = _latestTutorialElementID;
			if (_latestTutorialElementID == "PartySpeedLabel" && !MapInfo.IsInfoBarExtended)
			{
				MapInfo.IsInfoBarExtended = true;
			}
		}
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		_mapStateHandler = null;
		_mapNavigation?.OnFinalize();
		_mapTimeControl?.OnFinalize();
		_mapInfo = null;
		_mapNavigation = null;
		_mapTimeControl = null;
		Game.Current?.EventManager?.UnregisterEvent<TutorialNotificationElementChangeEvent>(OnTutorialNotificationElementIDChange);
	}
}
