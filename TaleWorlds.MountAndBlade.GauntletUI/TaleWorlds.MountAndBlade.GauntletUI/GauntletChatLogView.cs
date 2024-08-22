using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletChatLogView : GlobalLayer
{
	private MPChatVM _dataSource;

	private ChatLogMessageManager _chatLogMessageManager;

	private bool _canFocusWhileInMission = true;

	private bool _isTeamChatAvailable;

	private IGauntletMovie _movie;

	private bool _isEnabled = true;

	private const int MaxHistoryCountForSingleplayer = 250;

	private const int MaxHistoryCountForMultiplayer = 100;

	public static GauntletChatLogView Current { get; private set; }

	public GauntletChatLogView()
	{
		_dataSource = new MPChatVM();
		_dataSource.SetGetKeyTextFromKeyIDFunc(GetToggleChatKeyText);
		_dataSource.SetGetCycleChannelKeyTextFunc(GetCycleChannelsKeyText);
		_dataSource.SetGetSendMessageKeyTextFunc(GetSendMessageKeyText);
		_dataSource.SetGetCancelSendingKeyTextFunc(GetCancelSendingKeyText);
		GauntletLayer gauntletLayer = new GauntletLayer(300);
		_movie = gauntletLayer.LoadMovie("SPChatLog", _dataSource);
		gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ChatLogHotKeyCategory"));
		base.Layer = gauntletLayer;
		_chatLogMessageManager = new ChatLogMessageManager(_dataSource);
		MessageManager.SetMessageManager(_chatLogMessageManager);
		ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Combine(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionsChanged));
	}

	public static void Initialize()
	{
		if (Current == null)
		{
			Current = new GauntletChatLogView();
			ScreenManager.AddGlobalLayer(Current, isFocusable: false);
		}
	}

	private void OnManagedOptionsChanged(ManagedOptions.ManagedOptionsType changedManagedOptionsType)
	{
		bool num = changedManagedOptionsType == ManagedOptions.ManagedOptionsType.HideBattleUI && TaleWorlds.MountAndBlade.Mission.Current != null && BannerlordConfig.HideBattleUI;
		bool flag = changedManagedOptionsType == ManagedOptions.ManagedOptionsType.EnableSingleplayerChatBox && !GameNetwork.IsMultiplayer && !BannerlordConfig.EnableSingleplayerChatBox;
		bool flag2 = changedManagedOptionsType == ManagedOptions.ManagedOptionsType.EnableMultiplayerChatBox && GameNetwork.IsMultiplayer && !BannerlordConfig.EnableMultiplayerChatBox;
		if (num || flag || flag2)
		{
			_dataSource.Clear();
			CloseChat();
		}
	}

	private void CloseChat()
	{
		if (_dataSource.IsInspectingMessages)
		{
			_dataSource.StopInspectingMessages();
			ScreenManager.TryLoseFocus(base.Layer);
		}
		else if (_dataSource.IsTypingText)
		{
			_dataSource.StopTyping(resetWrittenText: true);
			ScreenManager.TryLoseFocus(base.Layer);
		}
	}

	protected override void OnTick(float dt)
	{
		if (!_isEnabled)
		{
			CloseChat();
			return;
		}
		base.OnTick(dt);
		if (_dataSource.IsChatAllowedByOptions())
		{
			_chatLogMessageManager.Update();
		}
		_dataSource.UpdateObjects(Game.Current, TaleWorlds.MountAndBlade.Mission.Current);
		_dataSource.Tick(dt);
	}

	protected override void OnLateTick(float dt)
	{
		base.OnLateTick(dt);
		MPChatVM dataSource = _dataSource;
		if (dataSource != null && dataSource.IsChatAllowedByOptions())
		{
			HandleInput();
		}
	}

	private void HandleInput()
	{
		bool inputEnabled = false;
		bool flag = true;
		_isTeamChatAvailable = true;
		InputContext inputContext = null;
		if (ScreenManager.TopScreen is MissionScreen)
		{
			MissionScreen missionScreen = (MissionScreen)ScreenManager.TopScreen;
			if (missionScreen.SceneLayer != null)
			{
				inputEnabled = true;
				inputContext = missionScreen.SceneLayer.Input;
			}
		}
		else if (ScreenManager.TopScreen is IGauntletChatLogHandlerScreen)
		{
			((IGauntletChatLogHandlerScreen)ScreenManager.TopScreen).TryUpdateChatLogLayerParameters(ref _isTeamChatAvailable, ref inputEnabled, ref inputContext);
		}
		else if (ScreenManager.TopScreen is GauntletInitialScreen)
		{
			inputEnabled = false;
		}
		else
		{
			ScreenLayer screenLayer = null;
			if (ScreenManager.TopScreen?.Layers != null)
			{
				for (int i = 0; i < ScreenManager.TopScreen.Layers.Count; i++)
				{
					if (ScreenManager.TopScreen.Layers[i]._categoryId == "SceneLayer")
					{
						screenLayer = ScreenManager.TopScreen.Layers[i];
						break;
					}
				}
			}
			if (screenLayer != null)
			{
				inputEnabled = true;
				flag = true;
				inputContext = screenLayer.Input;
			}
			_dataSource.ShowHideShowHint = screenLayer != null;
		}
		if (ScreenManager.FocusedLayer is GauntletLayer gauntletLayer && gauntletLayer != base.Layer && gauntletLayer.UIContext.EventManager.FocusedWidget is EditableTextWidget)
		{
			inputEnabled = false;
		}
		bool flag2 = false;
		bool flag3 = false;
		if (inputEnabled)
		{
			if (inputContext != null && !inputContext.IsCategoryRegistered(HotKeyManager.GetCategory("ChatLogHotKeyCategory")))
			{
				inputContext.RegisterHotKeyCategory(HotKeyManager.GetCategory("ChatLogHotKeyCategory"));
			}
			if (flag)
			{
				if (inputContext != null && inputContext.IsGameKeyReleased(6) && _canFocusWhileInMission)
				{
					_dataSource.TypeToChannelAll(startTyping: true);
					flag2 = true;
				}
				else if (inputContext != null && inputContext.IsGameKeyReleased(7) && _canFocusWhileInMission && _isTeamChatAvailable)
				{
					_dataSource.TypeToChannelTeam(startTyping: true);
					flag2 = true;
				}
				if (base.Layer.Input.IsHotKeyReleased("ToggleEscapeMenu") || base.Layer.Input.IsHotKeyReleased("Exit"))
				{
					bool isGamepadActive = Input.IsGamepadActive;
					_dataSource.StopTyping(isGamepadActive);
					flag3 = true;
				}
				else if (base.Layer.Input.IsGameKeyReleased(8) || base.Layer.Input.IsHotKeyReleased("FinalizeChatAlternative") || base.Layer.Input.IsHotKeyReleased("SendMessage"))
				{
					if ((Input.IsGamepadActive && base.Layer.Input.IsHotKeyReleased("SendMessage")) || !Input.IsGamepadActive)
					{
						_dataSource.SendCurrentlyTypedMessage();
					}
					_dataSource.StopTyping();
					flag3 = true;
				}
				if (inputContext != null && (inputContext.IsGameKeyDownAndReleased(8) || inputContext.IsHotKeyDownAndReleased("FinalizeChatAlternative")) && _canFocusWhileInMission)
				{
					if (_dataSource.ActiveChannelType == ChatChannelType.NaN)
					{
						_dataSource.TypeToChannelAll(startTyping: true);
					}
					else
					{
						_dataSource.StartTyping();
					}
					flag2 = true;
				}
				if (base.Layer.Input.IsHotKeyReleased("CycleChatTypes"))
				{
					if (_dataSource.ActiveChannelType == ChatChannelType.Team)
					{
						_dataSource.TypeToChannelAll();
					}
					else if (_dataSource.ActiveChannelType == ChatChannelType.All && _isTeamChatAvailable)
					{
						_dataSource.TypeToChannelTeam();
					}
				}
			}
			else if (inputContext != null && (inputContext.IsGameKeyReleased(8) || inputContext.IsHotKeyReleased("FinalizeChatAlternative")) && _canFocusWhileInMission)
			{
				if (!_dataSource.IsInspectingMessages)
				{
					_dataSource.StartInspectingMessages();
					flag2 = true;
				}
				else
				{
					_dataSource.StopInspectingMessages();
					flag3 = true;
				}
			}
		}
		else
		{
			bool num = _dataSource.IsTypingText || _dataSource.IsInspectingMessages;
			if (_dataSource.IsTypingText)
			{
				_dataSource.StopTyping();
			}
			else if (_dataSource.IsInspectingMessages)
			{
				_dataSource.StopInspectingMessages();
			}
			if (num)
			{
				base.Layer.InputRestrictions.ResetInputRestrictions();
				flag3 = true;
			}
		}
		if (flag2)
		{
			UpdateFocusLayer();
			ScreenManager.TrySetFocus(base.Layer);
		}
		else if (flag3)
		{
			UpdateFocusLayer();
			ScreenManager.TryLoseFocus(base.Layer);
		}
		if ((flag2 || flag3) && ScreenManager.TopScreen is MissionScreen { SceneLayer: not null } missionScreen2)
		{
			missionScreen2.Mission.GetMissionBehavior<MissionMainAgentController>().IsChatOpen = flag2 && !flag3;
		}
	}

	private void UpdateFocusLayer()
	{
		if (_dataSource.IsTypingText || _dataSource.IsInspectingMessages)
		{
			if (_dataSource.IsTypingText && !base.Layer.IsFocusLayer)
			{
				base.Layer.IsFocusLayer = true;
			}
			base.Layer.InputRestrictions.SetInputRestrictions();
		}
		else
		{
			base.Layer.IsFocusLayer = false;
			base.Layer.InputRestrictions.ResetInputRestrictions();
		}
	}

	public void SetCanFocusWhileInMission(bool canFocusInMission)
	{
		_canFocusWhileInMission = canFocusInMission;
	}

	public void OnSupportedFeaturesReceived(SupportedFeatures supportedFeatures)
	{
		SetEnabled(supportedFeatures.SupportsFeatures(Features.TextChat));
	}

	public void SetEnabled(bool isEnabled)
	{
		if (_isEnabled != isEnabled)
		{
			_isEnabled = isEnabled;
		}
	}

	public void LoadMovie(bool forMultiplayer)
	{
		if (_movie != null)
		{
			(base.Layer as GauntletLayer)?.ReleaseMovie(_movie);
		}
		if (forMultiplayer)
		{
			Game.Current?.GetGameHandler<ChatBox>().InitializeForMultiplayer();
			_movie = (base.Layer as GauntletLayer)?.LoadMovie("MPChatLog", _dataSource);
			_dataSource.SetMessageHistoryCapacity(100);
			return;
		}
		SetEnabled(isEnabled: true);
		Game.Current?.GetGameHandler<ChatBox>().InitializeForSinglePlayer();
		_movie = (base.Layer as GauntletLayer)?.LoadMovie("SPChatLog", _dataSource);
		_dataSource.ChatBoxSizeX = BannerlordConfig.ChatBoxSizeX;
		_dataSource.ChatBoxSizeY = BannerlordConfig.ChatBoxSizeY;
		_dataSource.SetMessageHistoryCapacity(250);
	}

	private TextObject GetToggleChatKeyText()
	{
		if (Input.IsGamepadActive)
		{
			return Game.Current?.GameTextManager?.GetHotKeyGameTextFromKeyID("controllerloption");
		}
		return Game.Current?.GameTextManager?.GetHotKeyGameTextFromKeyID("enter");
	}

	private TextObject GetCycleChannelsKeyText()
	{
		return Game.Current?.GameTextManager?.GetHotKeyGameText("ChatLogHotKeyCategory", "CycleChatTypes") ?? TextObject.Empty;
	}

	private TextObject GetSendMessageKeyText()
	{
		return Game.Current?.GameTextManager?.GetHotKeyGameText("ChatLogHotKeyCategory", "SendMessage") ?? TextObject.Empty;
	}

	private TextObject GetCancelSendingKeyText()
	{
		return Game.Current?.GameTextManager?.GetHotKeyGameText("GenericPanelGameKeyCategory", "Exit") ?? TextObject.Empty;
	}
}
