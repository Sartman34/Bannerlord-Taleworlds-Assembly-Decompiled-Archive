using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.Lobby;
using TaleWorlds.MountAndBlade.Diamond.Lobby.LocalData;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.HostGame;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.HostGame.HostGameOptions;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.CustomGame;

public class MPCustomGameVM : ViewModel
{
	public enum CustomGameMode
	{
		CustomServer,
		PremadeGame
	}

	private readonly LobbyState _lobbyState;

	private List<GameServerEntry> _currentCustomGameList;

	private CustomGameMode _customGameMode;

	private bool _canJoinOfficialServersAsAdmin;

	private const string _officialServerAdminBadgeName = "badge_official_server_admin";

	private InputKeyItemVM _refreshInputKey;

	private bool _isEnabled;

	private bool _isRefreshed;

	private bool _isPlayerBasedCustomBattleEnabled;

	private bool _isPremadeGameEnabled;

	private bool _isInParty;

	private bool _isPartyLeader;

	private bool _isCustomServerActionsActive;

	private bool _isAnyGameSelected;

	private MPCustomGameItemVM _selectedGame;

	private MPCustomGameFiltersVM _filtersData;

	private MPHostGameVM _hostGame;

	private MPCustomGameSortControllerVM _sortController;

	private MBBindingList<MPCustomGameItemVM> _gameList;

	private MBBindingList<StringPairItemWithActionVM> _customServerActionsList;

	private string _createServerText;

	private string _closeText;

	private string _serversInfoText;

	private string _refreshText;

	private string _joinText;

	private string _serverNameText;

	private string _gameTypeText;

	private string _mapText;

	private string _playerCountText;

	private string _pingText;

	private string _passwordText;

	private string _firstFactionText;

	private string _secondFactionText;

	private string _regionText;

	private string _premadeMatchTypeText;

	private string _hostText;

	private HintViewModel _isPasswordProtectedHint;

	public static bool IsPingInfoAvailable => true;

	public InputKeyItemVM RefreshInputKey
	{
		get
		{
			return _refreshInputKey;
		}
		set
		{
			if (value != _refreshInputKey)
			{
				_refreshInputKey = value;
				OnPropertyChangedWithValue(value, "RefreshInputKey");
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
				if (IsEnabled)
				{
					ExecuteRefresh();
				}
			}
		}
	}

	[DataSourceProperty]
	public bool IsAnyGameSelected
	{
		get
		{
			return _isAnyGameSelected;
		}
		set
		{
			if (value != _isAnyGameSelected)
			{
				_isAnyGameSelected = value;
				OnPropertyChangedWithValue(value, "IsAnyGameSelected");
			}
		}
	}

	[DataSourceProperty]
	public MPCustomGameItemVM SelectedGame
	{
		get
		{
			return _selectedGame;
		}
		set
		{
			if (value != _selectedGame)
			{
				_selectedGame = value;
				OnPropertyChangedWithValue(value, "SelectedGame");
				IsAnyGameSelected = _selectedGame != null;
			}
		}
	}

	[DataSourceProperty]
	public MPCustomGameFiltersVM FiltersData
	{
		get
		{
			return _filtersData;
		}
		set
		{
			if (value != _filtersData)
			{
				_filtersData = value;
				OnPropertyChangedWithValue(value, "FiltersData");
			}
		}
	}

	[DataSourceProperty]
	public MPHostGameVM HostGame
	{
		get
		{
			return _hostGame;
		}
		set
		{
			if (value != _hostGame)
			{
				_hostGame = value;
				OnPropertyChangedWithValue(value, "HostGame");
			}
		}
	}

	[DataSourceProperty]
	public MPCustomGameSortControllerVM SortController
	{
		get
		{
			return _sortController;
		}
		set
		{
			if (value != _sortController)
			{
				_sortController = value;
				OnPropertyChangedWithValue(value, "SortController");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPCustomGameItemVM> GameList
	{
		get
		{
			return _gameList;
		}
		set
		{
			if (value != _gameList)
			{
				_gameList = value;
				OnPropertyChangedWithValue(value, "GameList");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel IsPasswordProtectedHint
	{
		get
		{
			return _isPasswordProtectedHint;
		}
		set
		{
			if (value != _isPasswordProtectedHint)
			{
				_isPasswordProtectedHint = value;
				OnPropertyChangedWithValue(value, "IsPasswordProtectedHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRefreshed
	{
		get
		{
			return _isRefreshed;
		}
		set
		{
			if (value != _isRefreshed)
			{
				_isRefreshed = value;
				OnPropertyChangedWithValue(value, "IsRefreshed");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPartyLeader
	{
		get
		{
			return _isPartyLeader;
		}
		set
		{
			if (value != _isPartyLeader)
			{
				_isPartyLeader = value;
				OnPropertyChangedWithValue(value, "IsPartyLeader");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInParty
	{
		get
		{
			return _isInParty;
		}
		set
		{
			if (value != _isInParty)
			{
				_isInParty = value;
				OnPropertyChangedWithValue(value, "IsInParty");
			}
		}
	}

	[DataSourceProperty]
	public string CreateServerText
	{
		get
		{
			return _createServerText;
		}
		set
		{
			if (value != _createServerText)
			{
				_createServerText = value;
				OnPropertyChangedWithValue(value, "CreateServerText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCustomServerActionsActive
	{
		get
		{
			return _isCustomServerActionsActive;
		}
		set
		{
			if (value != _isCustomServerActionsActive)
			{
				_isCustomServerActionsActive = value;
				OnPropertyChangedWithValue(value, "IsCustomServerActionsActive");
			}
		}
	}

	[DataSourceProperty]
	public string CloseText
	{
		get
		{
			return _closeText;
		}
		set
		{
			if (value != _closeText)
			{
				_closeText = value;
				OnPropertyChangedWithValue(value, "CloseText");
			}
		}
	}

	[DataSourceProperty]
	public string ServersInfoText
	{
		get
		{
			return _serversInfoText;
		}
		set
		{
			if (value != _serversInfoText)
			{
				_serversInfoText = value;
				OnPropertyChangedWithValue(value, "ServersInfoText");
			}
		}
	}

	[DataSourceProperty]
	public string RefreshText
	{
		get
		{
			return _refreshText;
		}
		set
		{
			if (value != _refreshText)
			{
				_refreshText = value;
				OnPropertyChangedWithValue(value, "RefreshText");
			}
		}
	}

	[DataSourceProperty]
	public string JoinText
	{
		get
		{
			return _joinText;
		}
		set
		{
			if (value != _joinText)
			{
				_joinText = value;
				OnPropertyChangedWithValue(value, "JoinText");
			}
		}
	}

	[DataSourceProperty]
	public string ServerNameText
	{
		get
		{
			return _serverNameText;
		}
		set
		{
			if (value != _serverNameText)
			{
				_serverNameText = value;
				OnPropertyChangedWithValue(value, "ServerNameText");
			}
		}
	}

	[DataSourceProperty]
	public string GameTypeText
	{
		get
		{
			return _gameTypeText;
		}
		set
		{
			if (value != _gameTypeText)
			{
				_gameTypeText = value;
				OnPropertyChangedWithValue(value, "GameTypeText");
			}
		}
	}

	[DataSourceProperty]
	public string MapText
	{
		get
		{
			return _mapText;
		}
		set
		{
			if (value != _mapText)
			{
				_mapText = value;
				OnPropertyChangedWithValue(value, "MapText");
			}
		}
	}

	[DataSourceProperty]
	public string PlayerCountText
	{
		get
		{
			return _playerCountText;
		}
		set
		{
			if (value != _playerCountText)
			{
				_playerCountText = value;
				OnPropertyChangedWithValue(value, "PlayerCountText");
			}
		}
	}

	[DataSourceProperty]
	public string PingText
	{
		get
		{
			return _pingText;
		}
		set
		{
			if (value != _pingText)
			{
				_pingText = value;
				OnPropertyChangedWithValue(value, "PingText");
			}
		}
	}

	[DataSourceProperty]
	public string PasswordText
	{
		get
		{
			return _passwordText;
		}
		set
		{
			if (value != _passwordText)
			{
				_passwordText = value;
				OnPropertyChangedWithValue(value, "PasswordText");
			}
		}
	}

	[DataSourceProperty]
	public string FirstFactionText
	{
		get
		{
			return _firstFactionText;
		}
		set
		{
			if (value != _firstFactionText)
			{
				_firstFactionText = value;
				OnPropertyChanged("FirstFactionText");
			}
		}
	}

	[DataSourceProperty]
	public string SecondFactionText
	{
		get
		{
			return _secondFactionText;
		}
		set
		{
			if (value != _secondFactionText)
			{
				_secondFactionText = value;
				OnPropertyChanged("SecondFactionText");
			}
		}
	}

	[DataSourceProperty]
	public string RegionText
	{
		get
		{
			return _regionText;
		}
		set
		{
			if (value != _regionText)
			{
				_regionText = value;
				OnPropertyChanged("RegionText");
			}
		}
	}

	[DataSourceProperty]
	public string PremadeMatchTypeText
	{
		get
		{
			return _premadeMatchTypeText;
		}
		set
		{
			if (value != _premadeMatchTypeText)
			{
				_premadeMatchTypeText = value;
				OnPropertyChanged("PremadeMatchTypeText");
			}
		}
	}

	[DataSourceProperty]
	public string HostText
	{
		get
		{
			return _hostText;
		}
		set
		{
			if (value != _hostText)
			{
				_hostText = value;
				OnPropertyChanged("HostText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayerBasedCustomBattleEnabled
	{
		get
		{
			return _isPlayerBasedCustomBattleEnabled;
		}
		set
		{
			if (_customGameMode == CustomGameMode.CustomServer)
			{
				CreateServerText = (value ? new TextObject("{=gzdNEM76}Create a Game").ToString() : new TextObject("{=LrE2cUnG}Currently Disabled").ToString());
				if (value != _isPlayerBasedCustomBattleEnabled)
				{
					_isPlayerBasedCustomBattleEnabled = value;
					OnPropertyChangedWithValue(value, "IsPlayerBasedCustomBattleEnabled");
				}
			}
		}
	}

	public bool IsPremadeGameEnabled
	{
		get
		{
			return _isPremadeGameEnabled;
		}
		set
		{
			if (_customGameMode == CustomGameMode.PremadeGame)
			{
				CreateServerText = (value ? new TextObject("{=gzdNEM76}Create a Game").ToString() : new TextObject("{=LrE2cUnG}Currently Disabled").ToString());
				if (value != _isPremadeGameEnabled)
				{
					_isPremadeGameEnabled = value;
					OnPropertyChangedWithValue(value, "IsPremadeGameEnabled");
				}
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringPairItemWithActionVM> CustomServerActionsList
	{
		get
		{
			return _customServerActionsList;
		}
		set
		{
			if (value != _customServerActionsList)
			{
				_customServerActionsList = value;
				OnPropertyChangedWithValue(value, "CustomServerActionsList");
			}
		}
	}

	public static event Action<bool> OnMapCheckingStateChanged;

	public MPCustomGameVM(LobbyState lobbyState, CustomGameMode customGameMode)
	{
		_lobbyState = lobbyState;
		_currentCustomGameList = new List<GameServerEntry>();
		_customGameMode = customGameMode;
		HostGame = new MPHostGameVM(_lobbyState, _customGameMode);
		FiltersData = new MPCustomGameFiltersVM();
		GameList = new MBBindingList<MPCustomGameItemVM>();
		SortController = new MPCustomGameSortControllerVM(ref _gameList, _customGameMode);
		CustomServerActionsList = new MBBindingList<StringPairItemWithActionVM>();
		_currentCustomGameList = new List<GameServerEntry>();
		if (customGameMode == CustomGameMode.CustomServer)
		{
			_lobbyState.RegisterForCustomServerAction(OnServerActionRequested);
		}
		UpdateCanJoinOfficialServersAsAdmin();
		InitializeCallbacks();
		RefreshValues();
	}

	private async void UpdateCanJoinOfficialServersAsAdmin()
	{
		_canJoinOfficialServersAsAdmin = (await NetworkMain.GameClient.GetPlayerBadges()).Any((Badge b) => b.StringId == "badge_official_server_admin");
	}

	private void InitializeCallbacks()
	{
		MPCustomGameFiltersVM filtersData = FiltersData;
		filtersData.OnFiltersApplied = (Action)Delegate.Combine(filtersData.OnFiltersApplied, new Action(RefreshFiltersAndSort));
	}

	private void FinalizeCallbacks()
	{
		MPCustomGameFiltersVM filtersData = FiltersData;
		filtersData.OnFiltersApplied = (Action)Delegate.Remove(filtersData.OnFiltersApplied, new Action(RefreshFiltersAndSort));
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		IsPasswordProtectedHint = new HintViewModel(new TextObject("{=dMdmyb3Y}Password Protected"));
		CreateServerText = new TextObject("{=gzdNEM76}Create a Game").ToString();
		CloseText = new TextObject("{=6MQaCah5}Join a Game").ToString();
		ServersInfoText = new TextObject("{=WOQZBmMx}Servers").ToString();
		RefreshText = new TextObject("{=qFPBhVh4}Refresh").ToString();
		JoinText = new TextObject("{=lWDq0Uss}JOIN").ToString();
		PasswordText = new TextObject("{=8nJFaJio}Password").ToString();
		ServerNameText = new TextObject("{=OVcoYxj1}Server Name").ToString();
		GameTypeText = new TextObject("{=JPimShCw}Game Type").ToString();
		MapText = new TextObject("{=w9m11T1y}Map").ToString();
		PlayerCountText = new TextObject("{=RfXJdNye}Players").ToString();
		PingText = new TextObject("{=7qySRF2T}Ping").ToString();
		FirstFactionText = new TextObject("{=FhnKJODX}Faction A").ToString();
		SecondFactionText = new TextObject("{=a9TcHtVw}Faction B").ToString();
		RegionText = new TextObject("{=uoVKchoC}Region").ToString();
		PremadeMatchTypeText = new TextObject("{=OzifZbSB}Match Type").ToString();
		HostText = new TextObject("{=2baWg4Gq}Host").ToString();
		GameList.ApplyActionOnAllItems(delegate(MPCustomGameItemVM x)
		{
			x.RefreshValues();
		});
		SortController.RefreshValues();
		FiltersData.RefreshValues();
		HostGame?.RefreshValues();
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		if (_lobbyState != null)
		{
			_lobbyState.UnregisterForCustomServerAction(OnServerActionRequested);
		}
		RefreshInputKey?.OnFinalize();
		FinalizeCallbacks();
	}

	public void OnTick(float dt)
	{
		for (int i = 0; i < GameList.Count; i++)
		{
			GameList[i].UpdateIsFavorite();
		}
	}

	public void RefreshPremadeGameList()
	{
		IsRefreshed = false;
		OnGameSelected(null);
		GameList.Clear();
		if (NetworkMain.GameClient.AvailablePremadeGames?.PremadeGameEntries != null)
		{
			PremadeGameEntry[] premadeGameEntries = NetworkMain.GameClient.AvailablePremadeGames.PremadeGameEntries;
			foreach (PremadeGameEntry premadeGameInfo in premadeGameEntries)
			{
				GameList.Add(new MPCustomGameItemVM(premadeGameInfo, OnJoinGame));
			}
		}
		IsRefreshed = true;
	}

	public void RefreshCustomGameServerList(AvailableCustomGames availableCustomGames)
	{
		OnGameSelected(null);
		IsRefreshed = false;
		_currentCustomGameList = availableCustomGames.CustomGameServerInfos;
		RefreshFiltersAndSort();
		IsRefreshed = true;
	}

	private void RefreshFiltersAndSort()
	{
		OnGameSelected(null);
		GameList.Clear();
		List<GameServerEntry> serverList = FiltersData.GetFilteredServerList(_currentCustomGameList);
		GameServerEntry.FilterGameServerEntriesBasedOnCrossplay(ref serverList, _lobbyState.HasCrossplayPrivilege == true);
		foreach (GameServerEntry item in serverList)
		{
			GameList.Add(new MPCustomGameItemVM(item, OnGameSelected, OnJoinGame, OnShowActionsForEntry, OnToggleFavoriteServer));
		}
		SortController.SortByCurrentState();
	}

	public async void ExecuteRefresh()
	{
		if (!IsEnabled)
		{
			return;
		}
		IsRefreshed = false;
		OnGameSelected(null);
		GameList.Clear();
		if (_customGameMode == CustomGameMode.CustomServer)
		{
			await NetworkMain.GameClient.GetCustomGameServerList();
			MultiplayerOptions.Instance.CurrentOptionsCategory = MultiplayerOptions.OptionsCategory.Default;
		}
		else if (_customGameMode == CustomGameMode.PremadeGame)
		{
			await NetworkMain.GameClient.GetPremadeGameList();
			MultiplayerOptions.Instance.CurrentOptionsCategory = MultiplayerOptions.OptionsCategory.PremadeMatch;
		}
		MultiplayerOptions.Instance.OnGameTypeChanged();
		foreach (GenericHostGameOptionDataVM generalOption in HostGame.HostGameOptions.GeneralOptions)
		{
			if (generalOption is MultipleSelectionHostGameOptionDataVM multipleSelectionHostGameOptionDataVM)
			{
				multipleSelectionHostGameOptionDataVM.RefreshList();
			}
		}
		IsRefreshed = true;
	}

	private void OnShowActionsForEntry(MPCustomGameItemVM serverVM)
	{
		if (serverVM?.GameServerInfo == null)
		{
			return;
		}
		CustomServerActionsList.Clear();
		List<CustomServerAction> customActionsForServer = _lobbyState.GetCustomActionsForServer(serverVM.GameServerInfo);
		if (customActionsForServer.Count > 0)
		{
			for (int i = 0; i < customActionsForServer.Count; i++)
			{
				CustomServerAction customServerAction = customActionsForServer[i];
				CustomServerActionsList.Add(new StringPairItemWithActionVM(ExecuteSelectCustomServerAction, customServerAction.Name, customServerAction.Name, customServerAction));
			}
		}
		if (CustomServerActionsList.Count > 0)
		{
			IsCustomServerActionsActive = false;
			IsCustomServerActionsActive = true;
		}
	}

	private void OnGameSelected(MPCustomGameItemVM gameItem)
	{
		if (SelectedGame != null)
		{
			SelectedGame.IsSelected = false;
		}
		SelectedGame = gameItem;
		if (SelectedGame != null)
		{
			SelectedGame.IsSelected = true;
		}
	}

	public void ExecuteJoinSelectedGame()
	{
		OnJoinGame(_selectedGame);
	}

	public void OnJoinGame(MPCustomGameItemVM gameItem)
	{
		if (gameItem == null)
		{
			Debug.FailedAssert("Server to join is null.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\CustomGame\\MPCustomGameVM.cs", "OnJoinGame", 284);
		}
		else if (gameItem.IsPasswordProtected)
		{
			string titleText = GameTexts.FindText("str_password_required").ToString();
			string text = GameTexts.FindText("str_enter_password").ToString();
			string affirmativeText = GameTexts.FindText("str_ok").ToString();
			string negativeText = GameTexts.FindText("str_cancel").ToString();
			InformationManager.ShowTextInquiry(new TextInquiryData(titleText, text, isAffirmativeOptionShown: true, isNegativeOptionShown: true, affirmativeText, negativeText, GetOnTryPasswordForServerAction(gameItem), null, shouldInputBeObfuscated: true));
		}
		else if (_customGameMode == CustomGameMode.CustomServer)
		{
			JoinCustomGame(gameItem.GameServerInfo);
		}
		else if (_customGameMode == CustomGameMode.PremadeGame)
		{
			JoinPremadeGame(gameItem.PremadeGameInfo);
		}
	}

	private void OnToggleFavoriteServer(MPCustomGameItemVM gameItem)
	{
		GameServerEntry gameServerInfo = gameItem.GameServerInfo;
		if (MultiplayerLocalDataManager.Instance.FavoriteServers.TryGetServerData(gameServerInfo, out var favoriteServerData))
		{
			MultiplayerLocalDataManager.Instance.FavoriteServers.RemoveEntry(favoriteServerData);
			return;
		}
		FavoriteServerData item = FavoriteServerData.CreateFrom(gameServerInfo);
		MultiplayerLocalDataManager.Instance.FavoriteServers.AddEntry(item);
	}

	private Action<string> GetOnTryPasswordForServerAction(MPCustomGameItemVM serverItem)
	{
		if (_customGameMode == CustomGameMode.CustomServer)
		{
			GameServerEntry serverInfo2 = serverItem.GameServerInfo;
			return delegate(string passwordInput)
			{
				JoinCustomGame(serverInfo2, passwordInput);
			};
		}
		if (_customGameMode == CustomGameMode.PremadeGame)
		{
			PremadeGameEntry serverInfo = serverItem.PremadeGameInfo;
			return delegate(string passwordInput)
			{
				JoinPremadeGame(serverInfo, passwordInput);
			};
		}
		return delegate
		{
			Debug.FailedAssert("Fell through game modes, should never happen", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\CustomGame\\MPCustomGameVM.cs", "GetOnTryPasswordForServerAction", 338);
		};
	}

	private List<CustomServerAction> OnServerActionRequested(GameServerEntry serverEntry)
	{
		List<CustomServerAction> list = new List<CustomServerAction>();
		if (_canJoinOfficialServersAsAdmin || !serverEntry.IsOfficial)
		{
			CustomServerAction item = new CustomServerAction(delegate
			{
				InformationManager.ShowTextInquiry(new TextInquiryData(new TextObject("{=*}Join as Admin").ToString(), new TextObject("{=*}Enter Admin Password").ToString(), isAffirmativeOptionShown: true, isNegativeOptionShown: true, new TextObject("{=*}Join").ToString(), new TextObject("{=*}Cancel").ToString(), delegate(string passwordInput)
				{
					JoinCustomGame(serverEntry, passwordInput, isJoinAsAdmin: true);
				}, null, shouldInputBeObfuscated: true));
			}, serverEntry, new TextObject("{=*}Join as Admin").ToString());
			list.Add(item);
		}
		return list;
	}

	private async void JoinCustomGame(GameServerEntry selectedServer, string passwordInput = "", bool isJoinAsAdmin = false)
	{
		MPCustomGameVM.OnMapCheckingStateChanged?.Invoke(obj: true);
		(bool, string) tuple = await MapCheckHelpers.CheckMaps(selectedServer);
		MPCustomGameVM.OnMapCheckingStateChanged?.Invoke(obj: false);
		if (tuple.Item1)
		{
			_lobbyState.OnClientRefusedToJoinCustomServer(selectedServer);
			string text = new TextObject("{=*}You don't have at least one map ({MAP_NAME}) being played on the server or the local map is not identical. Download all missing maps from the server if you would like to join it.").SetTextVariable("MAP_NAME", tuple.Item2).ToString();
			InformationManager.ShowInquiry(new InquiryData(GameTexts.FindText("str_couldnt_join_server").ToString(), text, isAffirmativeOptionShown: false, isNegativeOptionShown: true, "", GameTexts.FindText("str_dismiss").ToString(), null, null));
		}
		else if (!(await NetworkMain.GameClient.RequestJoinCustomGame(selectedServer.Id, passwordInput, isJoinAsAdmin)))
		{
			InformationManager.ShowInquiry(new InquiryData("", GameTexts.FindText("str_couldnt_join_server").ToString(), isAffirmativeOptionShown: true, isNegativeOptionShown: false, GameTexts.FindText("str_ok").ToString(), "", null, null));
		}
	}

	private void JoinPremadeGame(PremadeGameEntry selectedGame, string passwordInput = "")
	{
		NetworkMain.GameClient.RequestToJoinPremadeGame(selectedGame.Id, passwordInput);
	}

	private void ExecuteSelectCustomServerAction(object actionParam)
	{
		(actionParam as CustomServerAction).Execute();
	}

	public void SetRefreshInputKey(HotKey hotKey)
	{
		RefreshInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}
}
