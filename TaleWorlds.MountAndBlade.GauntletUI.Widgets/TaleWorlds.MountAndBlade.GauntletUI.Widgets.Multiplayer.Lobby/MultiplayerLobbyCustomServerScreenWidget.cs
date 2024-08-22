using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Multiplayer.Lobby;

public class MultiplayerLobbyCustomServerScreenWidget : Widget
{
	private readonly Action<Widget> _createGameClickHandler;

	private readonly Action<Widget> _closeCreatePanelClickHandler;

	private bool _isCreateGamePanelActive;

	private ListPanel _serverListPanel;

	private ButtonWidget _joinServerButton;

	private ButtonWidget _hostServerButton;

	private ButtonWidget _createGameButton;

	private ButtonWidget _closeCreatePanelButton;

	private Widget _joinGamePanel;

	private Widget _createGamePanel;

	private ButtonWidget _refreshButton;

	private Widget _filtersPanel;

	private TextWidget _serverCountText;

	private TextWidget _infoText;

	private bool _isPlayerBasedCustomBattleEnabled;

	private bool _isPremadeGameEnabled;

	private bool _isPartyLeader;

	private bool _isInParty;

	private bool _isAnyGameSelected;

	public NavigationScopeTargeter FilterSearchBarScope { get; set; }

	public NavigationScopeTargeter FilterButtonsScope { get; set; }

	[Editor(false)]
	public ListPanel ServerListPanel
	{
		get
		{
			return _serverListPanel;
		}
		set
		{
			if (_serverListPanel != value)
			{
				_serverListPanel?.ItemAddEventHandlers.Remove(ServerListItemsChanged);
				_serverListPanel?.ItemAfterRemoveEventHandlers.Remove(ServerListItemsChanged);
				_serverListPanel?.SelectEventHandlers.Remove(ServerSelectionChanged);
				_serverListPanel = value;
				_serverListPanel?.ItemAddEventHandlers.Add(ServerListItemsChanged);
				_serverListPanel?.ItemAfterRemoveEventHandlers.Add(ServerListItemsChanged);
				_serverListPanel?.SelectEventHandlers.Add(ServerSelectionChanged);
				OnPropertyChanged(value, "ServerListPanel");
			}
		}
	}

	[Editor(false)]
	public ButtonWidget JoinServerButton
	{
		get
		{
			return _joinServerButton;
		}
		set
		{
			if (_joinServerButton != value)
			{
				_joinServerButton = value;
				OnPropertyChanged(value, "JoinServerButton");
			}
		}
	}

	[Editor(false)]
	public ButtonWidget HostServerButton
	{
		get
		{
			return _hostServerButton;
		}
		set
		{
			if (_hostServerButton != value)
			{
				_hostServerButton = value;
				OnPropertyChanged(value, "HostServerButton");
			}
		}
	}

	[Editor(false)]
	public ButtonWidget CreateGameButton
	{
		get
		{
			return _createGameButton;
		}
		set
		{
			if (_createGameButton != value)
			{
				_createGameButton?.ClickEventHandlers.Remove(_createGameClickHandler);
				_createGameButton = value;
				_createGameButton?.ClickEventHandlers.Add(_createGameClickHandler);
				OnPropertyChanged(value, "CreateGameButton");
			}
		}
	}

	[Editor(false)]
	public ButtonWidget CloseCreatePanelButton
	{
		get
		{
			return _closeCreatePanelButton;
		}
		set
		{
			if (_closeCreatePanelButton != value)
			{
				_closeCreatePanelButton?.ClickEventHandlers.Remove(_closeCreatePanelClickHandler);
				_closeCreatePanelButton = value;
				_closeCreatePanelButton?.ClickEventHandlers.Add(_closeCreatePanelClickHandler);
				OnPropertyChanged(value, "CloseCreatePanelButton");
			}
		}
	}

	[Editor(false)]
	public Widget JoinGamePanel
	{
		get
		{
			return _joinGamePanel;
		}
		set
		{
			if (_joinGamePanel != value)
			{
				_joinGamePanel = value;
				OnPropertyChanged(value, "JoinGamePanel");
			}
		}
	}

	[Editor(false)]
	public Widget CreateGamePanel
	{
		get
		{
			return _createGamePanel;
		}
		set
		{
			if (_createGamePanel != value)
			{
				_createGamePanel = value;
				OnPropertyChanged(value, "CreateGamePanel");
			}
		}
	}

	[Editor(false)]
	public ButtonWidget RefreshButton
	{
		get
		{
			return _refreshButton;
		}
		set
		{
			if (_refreshButton != value)
			{
				_refreshButton?.ClickEventHandlers.Remove(RefreshClicked);
				_refreshButton = value;
				_refreshButton?.ClickEventHandlers.Add(RefreshClicked);
				OnPropertyChanged(value, "RefreshButton");
			}
		}
	}

	[Editor(false)]
	public Widget FiltersPanel
	{
		get
		{
			return _filtersPanel;
		}
		set
		{
			if (_filtersPanel != value)
			{
				_filtersPanel = value;
				OnPropertyChanged(value, "FiltersPanel");
				FiltersPanelUpdated();
			}
		}
	}

	[Editor(false)]
	public TextWidget ServerCountText
	{
		get
		{
			return _serverCountText;
		}
		set
		{
			if (_serverCountText != value)
			{
				_serverCountText = value;
				OnPropertyChanged(value, "ServerCountText");
			}
		}
	}

	[Editor(false)]
	public TextWidget InfoText
	{
		get
		{
			return _infoText;
		}
		set
		{
			if (_infoText != value)
			{
				_infoText = value;
				OnPropertyChanged(value, "InfoText");
			}
		}
	}

	[Editor(false)]
	public bool IsPartyLeader
	{
		get
		{
			return _isPartyLeader;
		}
		set
		{
			if (_isPartyLeader != value)
			{
				_isPartyLeader = value;
				OnPropertyChanged(value, "IsPartyLeader");
				OnUpdateJoinServerEnabled();
				OnUpdateCreateServerEnabled();
			}
		}
	}

	[Editor(false)]
	public bool IsInParty
	{
		get
		{
			return _isInParty;
		}
		set
		{
			if (_isInParty != value)
			{
				_isInParty = value;
				OnPropertyChanged(value, "IsInParty");
				OnUpdateJoinServerEnabled();
				OnUpdateCreateServerEnabled();
			}
		}
	}

	[Editor(false)]
	public bool IsAnyGameSelected
	{
		get
		{
			return _isAnyGameSelected;
		}
		set
		{
			if (_isAnyGameSelected != value)
			{
				_isAnyGameSelected = value;
				OnPropertyChanged(value, "IsAnyGameSelected");
				OnUpdateJoinServerEnabled();
			}
		}
	}

	[Editor(false)]
	public bool IsPlayerBasedCustomBattleEnabled
	{
		get
		{
			return _isPlayerBasedCustomBattleEnabled;
		}
		set
		{
			if (!value && _isCreateGamePanelActive)
			{
				_isCreateGamePanelActive = false;
				UpdatePanels();
			}
			if (_isPlayerBasedCustomBattleEnabled != value)
			{
				_isPlayerBasedCustomBattleEnabled = value;
				OnPropertyChanged(value, "IsPlayerBasedCustomBattleEnabled");
			}
			OnUpdateCreateServerEnabled();
		}
	}

	[Editor(false)]
	public bool IsPremadeGameEnabled
	{
		get
		{
			return _isPremadeGameEnabled;
		}
		set
		{
			if (!value && _isCreateGamePanelActive)
			{
				_isCreateGamePanelActive = false;
				UpdatePanels();
			}
			if (_isPremadeGameEnabled != value)
			{
				_isPremadeGameEnabled = value;
				OnPropertyChanged(value, "IsPremadeGameEnabled");
			}
			OnUpdateCreateServerEnabled();
		}
	}

	public MultiplayerLobbyCustomServerScreenWidget(UIContext context)
		: base(context)
	{
		_createGameClickHandler = OnCreateGameClick;
		_closeCreatePanelClickHandler = OnCloseCreatePanelClick;
		_isCreateGamePanelActive = false;
	}

	private void OnCreateGameClick(Widget widget)
	{
		_isCreateGamePanelActive = true;
		UpdatePanels();
	}

	private void OnCloseCreatePanelClick(Widget widget)
	{
		_isCreateGamePanelActive = false;
		UpdatePanels();
	}

	private void UpdatePanels()
	{
		JoinGamePanel.IsVisible = !_isCreateGamePanelActive;
		CreateGameButton.IsVisible = !_isCreateGamePanelActive;
		CreateGamePanel.IsVisible = _isCreateGamePanelActive;
		CloseCreatePanelButton.IsVisible = _isCreateGamePanelActive;
		RefreshButton.IsVisible = !_isCreateGamePanelActive;
		ServerCountText.IsVisible = !_isCreateGamePanelActive;
		InfoText.IsVisible = !_isCreateGamePanelActive;
		JoinServerButton.IsVisible = !_isCreateGamePanelActive;
		HostServerButton.IsVisible = _isCreateGamePanelActive;
		FiltersPanel.SetState(_isCreateGamePanelActive ? "Disabled" : "Default");
		if (FilterSearchBarScope != null)
		{
			FilterSearchBarScope.IsScopeDisabled = _isCreateGamePanelActive;
		}
		if (FilterButtonsScope != null)
		{
			FilterButtonsScope.IsScopeDisabled = _isCreateGamePanelActive;
		}
	}

	private void FiltersPanelUpdated()
	{
		FiltersPanel.AddState("Disabled");
	}

	private void ServerListItemsChanged(Widget widget)
	{
		ServerCountText.IntText = ServerListPanel.ChildCount;
	}

	private void ServerListItemsChanged(Widget parentWidget, Widget addedWidget)
	{
		ServerCountText.IntText = ServerListPanel.ChildCount;
	}

	private void ServerSelectionChanged(Widget child)
	{
		OnUpdateJoinServerEnabled();
	}

	private void OnUpdateJoinServerEnabled()
	{
		if (JoinServerButton != null && ServerListPanel != null)
		{
			JoinServerButton.IsEnabled = IsAnyGameSelected && (IsPartyLeader || !IsInParty);
		}
	}

	private void OnUpdateCreateServerEnabled()
	{
		bool flag = (IsPlayerBasedCustomBattleEnabled || IsPremadeGameEnabled) && (IsPartyLeader || !IsInParty);
		if (CreateGameButton != null && ServerListPanel != null)
		{
			CreateGameButton.IsVisible = flag;
		}
		if (CreateGamePanel.IsVisible && !flag)
		{
			_isCreateGamePanelActive = false;
			UpdatePanels();
		}
	}

	private void RefreshClicked(Widget widget)
	{
		JoinServerButton.IsEnabled = false;
	}
}
