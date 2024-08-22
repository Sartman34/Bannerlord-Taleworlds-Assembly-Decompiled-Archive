using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace TaleWorlds.ScreenSystem;

public static class ScreenManager
{
	public delegate void OnPushScreenEvent(ScreenBase pushedScreen);

	public delegate void OnPopScreenEvent(ScreenBase poppedScreen);

	public delegate void OnControllerDisconnectedEvent();

	private static IScreenManagerEngineConnection _engineInterface;

	private static Vec2 _usableArea;

	private static List<ScreenLayer> _lateTickLayers;

	private static ObservableCollection<ScreenBase> _screenList;

	private static ObservableCollection<GlobalLayer> _globalLayers;

	private static List<ScreenLayer> _sortedLayers;

	private static ScreenLayer[] _sortedActiveLayersCopyForUpdate;

	private static bool _isSortedActiveLayersDirty;

	private static bool _focusedLayerChangedThisFrame;

	private static bool _isMouseInputActiveLastFrame;

	private static bool _isScreenDebugInformationEnabled;

	private static bool _activeMouseVisible;

	private static IReadOnlyList<int> _lastPressedKeys;

	private static bool _globalOrderDirty;

	private static bool _isRefreshActive;

	public static IScreenManagerEngineConnection EngineInterface => _engineInterface;

	public static float Scale { get; private set; }

	public static Vec2 UsableArea
	{
		get
		{
			return _usableArea;
		}
		private set
		{
			if (value != _usableArea)
			{
				_usableArea = value;
				OnUsableAreaChanged(_usableArea);
			}
		}
	}

	public static bool IsEnterButtonRDown => _engineInterface.GetIsEnterButtonRDown();

	public static List<ScreenLayer> SortedLayers
	{
		get
		{
			if (_isSortedActiveLayersDirty || _sortedLayers.Count != TopScreen?.Layers.Count + _globalLayers?.Count)
			{
				_isMouseInputActiveLastFrame = false;
				_sortedLayers.Clear();
				if (TopScreen != null)
				{
					for (int i = 0; i < TopScreen.Layers.Count; i++)
					{
						ScreenLayer screenLayer = TopScreen.Layers[i];
						if (screenLayer != null)
						{
							_sortedLayers.Add(screenLayer);
						}
					}
				}
				foreach (GlobalLayer globalLayer in _globalLayers)
				{
					_sortedLayers.Add(globalLayer.Layer);
				}
				_sortedLayers.Sort();
				_isSortedActiveLayersDirty = false;
			}
			return _sortedLayers;
		}
	}

	public static ScreenBase TopScreen { get; private set; }

	public static ScreenLayer FocusedLayer { get; private set; }

	public static ScreenLayer FirstHitLayer { get; private set; }

	public static event OnPushScreenEvent OnPushScreen;

	public static event OnPopScreenEvent OnPopScreen;

	public static event OnControllerDisconnectedEvent OnControllerDisconnected;

	public static event Action FocusGained;

	public static event Action<string, string, int, int> PlatformTextRequested;

	static ScreenManager()
	{
		Scale = 1f;
		_usableArea = new Vec2(1f, 1f);
		_sortedLayers = new List<ScreenLayer>(16);
		_sortedActiveLayersCopyForUpdate = new ScreenLayer[16];
		_isSortedActiveLayersDirty = true;
		_activeMouseVisible = true;
		_isRefreshActive = false;
		_globalLayers = new ObservableCollection<GlobalLayer>();
		_screenList = new ObservableCollection<ScreenBase>();
		_screenList.CollectionChanged += OnScreenListChanged;
		_globalLayers.CollectionChanged += OnGlobalListChanged;
		FocusedLayer = null;
		FirstHitLayer = null;
	}

	public static void Initialize(IScreenManagerEngineConnection engineInterface)
	{
		_engineInterface = engineInterface;
	}

	internal static void RefreshGlobalOrder()
	{
		if (_isRefreshActive)
		{
			return;
		}
		_isRefreshActive = true;
		int currentOrder = -2000;
		int currentOrder2 = 10000;
		for (int i = 0; i < SortedLayers.Count; i++)
		{
			if (SortedLayers[i] == null)
			{
				continue;
			}
			if (!SortedLayers[i].Finalized)
			{
				ScreenLayer screenLayer = SortedLayers[i];
				if (screenLayer != null && screenLayer.IsActive)
				{
					SortedLayers[i]?.RefreshGlobalOrder(ref currentOrder);
				}
				else
				{
					SortedLayers[i]?.RefreshGlobalOrder(ref currentOrder2);
				}
			}
			_globalOrderDirty = false;
		}
		_isRefreshActive = false;
	}

	public static void RemoveGlobalLayer(GlobalLayer layer)
	{
		TaleWorlds.Library.Debug.Print("RemoveGlobalLayer");
		_globalLayers.Remove(layer);
		layer.Layer.HandleDeactivate();
		_globalOrderDirty = true;
	}

	public static void AddGlobalLayer(GlobalLayer layer, bool isFocusable)
	{
		TaleWorlds.Library.Debug.Print("AddGlobalLayer");
		int index = _globalLayers.Count;
		for (int i = 0; i < _globalLayers.Count; i++)
		{
			if (_globalLayers[i].Layer.InputRestrictions.Order >= layer.Layer.InputRestrictions.Order)
			{
				index = i;
				break;
			}
		}
		_globalLayers.Insert(index, layer);
		layer.Layer.HandleActivate();
		_globalOrderDirty = true;
	}

	public static void OnConstrainStateChanged(bool isConstrained)
	{
		TaleWorlds.Library.Debug.Print("OnConstrainStateChanged: " + isConstrained);
		OnGameWindowFocusChange(!isConstrained);
	}

	public static bool ScreenTypeExistsAtList(ScreenBase screen)
	{
		Type type = screen.GetType();
		foreach (ScreenBase screen2 in _screenList)
		{
			if (screen2.GetType() == type)
			{
				return true;
			}
		}
		return false;
	}

	public static void UpdateLayout()
	{
		foreach (GlobalLayer globalLayer in _globalLayers)
		{
			globalLayer.UpdateLayout();
		}
		foreach (ScreenBase screen in _screenList)
		{
			screen.UpdateLayout();
		}
	}

	public static void SetSuspendLayer(ScreenLayer layer, bool isSuspended)
	{
		if (isSuspended)
		{
			layer.HandleDeactivate();
		}
		else
		{
			layer.HandleActivate();
		}
		layer.LastActiveState = !isSuspended;
	}

	public static void OnFinalize()
	{
		DeactivateAndFinalizeAllScreens();
		_screenList.CollectionChanged -= OnScreenListChanged;
		_globalLayers.CollectionChanged -= OnGlobalListChanged;
		_screenList = null;
		_globalLayers = null;
		FocusedLayer = null;
	}

	private static void DeactivateAndFinalizeAllScreens()
	{
		TaleWorlds.Library.Debug.Print("DeactivateAndFinalizeAllScreens");
		for (int num = _screenList.Count - 1; num >= 0; num--)
		{
			_screenList[num].HandlePause();
		}
		for (int num2 = _screenList.Count - 1; num2 >= 0; num2--)
		{
			_screenList[num2].HandleDeactivate();
		}
		for (int num3 = _screenList.Count - 1; num3 >= 0; num3--)
		{
			_screenList[num3].HandleFinalize();
		}
		_screenList.Clear();
		Common.MemoryCleanupGC();
	}

	internal static void UpdateLateTickLayers(List<ScreenLayer> layers)
	{
		_lateTickLayers = layers;
	}

	public static void Tick(float dt, bool activeMouseVisible)
	{
		for (int i = 0; i < _globalLayers.Count; i++)
		{
			_globalLayers[i]?.EarlyTick(dt);
		}
		Update();
		_lateTickLayers = null;
		if (TopScreen != null)
		{
			TopScreen.FrameTick(dt);
			FindPredecessor(TopScreen)?.IdleTick(dt);
		}
		for (int j = 0; j < _globalLayers.Count; j++)
		{
			_globalLayers[j]?.Tick(dt);
		}
		LateUpdate(dt, activeMouseVisible);
		ShowScreenDebugInformation();
	}

	public static void LateTick(float dt)
	{
		if (_lateTickLayers != null)
		{
			for (int i = 0; i < _lateTickLayers.Count; i++)
			{
				if (!_lateTickLayers[i].Finalized)
				{
					_lateTickLayers[i].LateTick(dt);
				}
			}
			_lateTickLayers.Clear();
		}
		for (int j = 0; j < _globalLayers.Count; j++)
		{
			_globalLayers[j].LateTick(dt);
		}
	}

	public static void OnPlatformScreenKeyboardRequested(string initialText, string descriptionText, int maxLength, int keyboardTypeEnum)
	{
		ScreenManager.PlatformTextRequested?.Invoke(initialText, descriptionText, maxLength, keyboardTypeEnum);
	}

	public static void OnOnscreenKeyboardDone(string inputText)
	{
		FocusedLayer?.OnOnScreenKeyboardDone(inputText);
	}

	public static void OnOnscreenKeyboardCanceled()
	{
		FocusedLayer?.OnOnScreenKeyboardCanceled();
	}

	public static void OnGameWindowFocusChange(bool focusGained)
	{
		TaleWorlds.Library.Debug.Print("OnGameWindowFocusChange: " + focusGained);
		TaleWorlds.Library.Debug.Print("TopScreen: " + TopScreen?.GetType()?.Name);
		bool flag = false;
		if (!Debugger.IsAttached && !flag)
		{
			TopScreen?.OnFocusChangeOnGameWindow(focusGained);
		}
		if (focusGained)
		{
			ScreenManager.FocusGained?.Invoke();
		}
	}

	public static void ReplaceTopScreen(ScreenBase screen)
	{
		TaleWorlds.Library.Debug.Print("ReplaceToTopScreen");
		if (_screenList.Count > 0)
		{
			TopScreen.HandlePause();
			TopScreen.HandleDeactivate();
			TopScreen.HandleFinalize();
			ScreenManager.OnPopScreen?.Invoke(TopScreen);
			_screenList.Remove(TopScreen);
		}
		_screenList.Add(screen);
		screen.HandleInitialize();
		screen.HandleActivate();
		screen.HandleResume();
		_globalOrderDirty = true;
		ScreenManager.OnPushScreen?.Invoke(screen);
	}

	public static List<ScreenLayer> GetPersistentInputRestrictions()
	{
		List<ScreenLayer> list = new List<ScreenLayer>();
		foreach (GlobalLayer globalLayer in _globalLayers)
		{
			list.Add(globalLayer.Layer);
		}
		return list;
	}

	public static void SetAndActivateRootScreen(ScreenBase screen)
	{
		TaleWorlds.Library.Debug.Print("SetAndActivateRootScreen");
		if (TopScreen != null)
		{
			throw new Exception("TopScreen is not null.");
		}
		_screenList.Add(screen);
		screen.HandleInitialize();
		screen.HandleActivate();
		screen.HandleResume();
		_globalOrderDirty = true;
		ScreenManager.OnPushScreen?.Invoke(screen);
	}

	public static void CleanAndPushScreen(ScreenBase screen)
	{
		TaleWorlds.Library.Debug.Print("CleanAndPushScreen");
		DeactivateAndFinalizeAllScreens();
		_screenList.Add(screen);
		screen.HandleInitialize();
		screen.HandleActivate();
		screen.HandleResume();
		_globalOrderDirty = true;
		ScreenManager.OnPushScreen?.Invoke(screen);
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("cb_clear_siege_machine_selection", "ui")]
	public static string ClearSiegeMachineSelection(List<string> args)
	{
		ScreenBase screenBase = _screenList.FirstOrDefault((ScreenBase x) => x.GetType().GetMethod("ClearSiegeMachineSelections") != null);
		screenBase?.GetType().GetMethod("ClearSiegeMachineSelections").Invoke(screenBase, null);
		return "Siege machine selections have been cleared.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("cb_copy_battle_layout_to_clipboard", "ui")]
	public static string CopyCustomBattle(List<string> args)
	{
		ScreenBase screenBase = _screenList.FirstOrDefault((ScreenBase x) => x.GetType().GetMethod("CopyBattleLayoutToClipboard") != null);
		if (screenBase != null)
		{
			screenBase.GetType().GetMethod("CopyBattleLayoutToClipboard").Invoke(screenBase, null);
			return "Custom battle layout has been copied to clipboard as text.";
		}
		return "Something went wrong";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("cb_apply_battle_layout_from_string", "ui")]
	public static string ApplyCustomBattleLayout(List<string> args)
	{
		ScreenBase screenBase = _screenList.FirstOrDefault((ScreenBase x) => x.GetType().GetMethod("ApplyCustomBattleLayout") != null);
		if (screenBase != null && args.Count > 0)
		{
			string text = args.Aggregate((string i, string j) => i + " " + j);
			if (text.Count() > 5)
			{
				screenBase.GetType().GetMethod("ApplyCustomBattleLayout").Invoke(screenBase, new object[1] { text });
				return "Applied new layout from text.";
			}
			return "Argument is not right.";
		}
		return "Something went wrong.";
	}

	public static void PushScreen(ScreenBase screen)
	{
		TaleWorlds.Library.Debug.Print("PushScreen");
		if (_screenList.Count > 0)
		{
			TopScreen.HandlePause();
			if (TopScreen.IsActive)
			{
				TopScreen.HandleDeactivate();
			}
		}
		_screenList.Add(screen);
		screen.HandleInitialize();
		screen.HandleActivate();
		screen.HandleResume();
		_globalOrderDirty = true;
		ScreenManager.OnPushScreen?.Invoke(screen);
	}

	public static void PopScreen()
	{
		TaleWorlds.Library.Debug.Print("PopScreen");
		if (_screenList.Count > 0)
		{
			TopScreen.HandlePause();
			TopScreen.HandleDeactivate();
			TopScreen.HandleFinalize();
			TaleWorlds.Library.Debug.Print("PopScreen - " + TopScreen.GetType().ToString());
			ScreenManager.OnPopScreen?.Invoke(TopScreen);
			_screenList.Remove(TopScreen);
		}
		if (_screenList.Count > 0)
		{
			ScreenBase topScreen = TopScreen;
			TopScreen.HandleActivate();
			if (topScreen == TopScreen)
			{
				TopScreen.HandleResume();
			}
		}
		_globalOrderDirty = true;
	}

	public static void CleanScreens()
	{
		TaleWorlds.Library.Debug.Print("CleanScreens");
		while (_screenList.Count > 0)
		{
			TopScreen.HandlePause();
			TopScreen.HandleDeactivate();
			TopScreen.HandleFinalize();
			ScreenManager.OnPopScreen?.Invoke(TopScreen);
			_screenList.Remove(TopScreen);
		}
		_globalOrderDirty = true;
	}

	private static ScreenBase FindPredecessor(ScreenBase screen)
	{
		ScreenBase result = null;
		int num = _screenList.IndexOf(screen);
		if (num > 0)
		{
			result = _screenList[num - 1];
		}
		return result;
	}

	public static void Update(IReadOnlyList<int> lastKeysPressed)
	{
		_lastPressedKeys = lastKeysPressed;
		ScreenBase topScreen = TopScreen;
		if (topScreen != null && topScreen.IsActive)
		{
			TopScreen.Update(_lastPressedKeys);
		}
		for (int i = 0; i < _globalLayers.Count; i++)
		{
			GlobalLayer globalLayer = _globalLayers[i];
			if (globalLayer.Layer.IsActive)
			{
				globalLayer.Update(_lastPressedKeys);
			}
		}
	}

	private static bool? GetMouseInput()
	{
		bool flag = false;
		if (Input.IsKeyDown(InputKey.LeftMouseButton) || Input.IsKeyDown(InputKey.RightMouseButton) || Input.IsKeyDown(InputKey.MiddleMouseButton) || Input.IsKeyDown(InputKey.X1MouseButton) || Input.IsKeyDown(InputKey.X2MouseButton) || Input.IsKeyDown(IsEnterButtonRDown ? InputKey.ControllerRDown : InputKey.ControllerRRight))
		{
			flag = true;
		}
		if (!_isMouseInputActiveLastFrame && flag)
		{
			flag = true;
		}
		else
		{
			if (!_isMouseInputActiveLastFrame || flag)
			{
				return null;
			}
			flag = false;
		}
		_isMouseInputActiveLastFrame = flag;
		return flag;
	}

	public static void EarlyUpdate(Vec2 usableArea)
	{
		UsableArea = usableArea;
		RefreshGlobalOrder();
		InputType inputType = InputType.None;
		for (int i = 0; i < SortedLayers.Count; i++)
		{
			ScreenLayer screenLayer = SortedLayers[i];
			if (screenLayer != null && screenLayer.IsActive)
			{
				SortedLayers[i].MouseEnabled = true;
			}
		}
		bool? mouseInput = GetMouseInput();
		for (int num = SortedLayers.Count - 1; num >= 0; num--)
		{
			ScreenLayer screenLayer2 = SortedLayers[num];
			if (screenLayer2 != null && screenLayer2.IsActive && !screenLayer2.Finalized)
			{
				bool? isMousePressed = null;
				if (mouseInput == false)
				{
					isMousePressed = false;
				}
				InputType inputType2 = InputType.None;
				InputUsageMask inputUsageMask = screenLayer2.InputUsageMask;
				screenLayer2.ScreenOrderInLastFrame = num;
				screenLayer2.IsHitThisFrame = false;
				if (screenLayer2.HitTest())
				{
					if (FirstHitLayer == null)
					{
						FirstHitLayer = screenLayer2;
						_engineInterface.ActivateMouseCursor(screenLayer2.ActiveCursor);
					}
					if (!inputType.HasAnyFlag(InputType.MouseButton) && inputUsageMask.HasAnyFlag(InputUsageMask.MouseButtons))
					{
						isMousePressed = mouseInput;
						inputType2 |= InputType.MouseButton;
						inputType |= InputType.MouseButton;
						screenLayer2.IsHitThisFrame = true;
					}
					if (!inputType.HasAnyFlag(InputType.MouseWheel) && inputUsageMask.HasAnyFlag(InputUsageMask.MouseWheels))
					{
						inputType2 |= InputType.MouseWheel;
						inputType |= InputType.MouseWheel;
						screenLayer2.IsHitThisFrame = true;
					}
				}
				if (!inputType.HasAnyFlag(InputType.Key) && FocusTest(screenLayer2))
				{
					inputType2 |= InputType.Key;
					inputType |= InputType.Key;
				}
				screenLayer2.EarlyProcessEvents(inputType2, isMousePressed);
			}
		}
	}

	private static void Update()
	{
		int num = 0;
		for (int i = 0; i < SortedLayers.Count; i++)
		{
			if (SortedLayers[i].IsActive)
			{
				num++;
			}
		}
		if (_sortedActiveLayersCopyForUpdate.Length < num)
		{
			_sortedActiveLayersCopyForUpdate = new ScreenLayer[num];
		}
		int num2 = 0;
		for (int j = 0; j < SortedLayers.Count; j++)
		{
			ScreenLayer screenLayer = SortedLayers[j];
			if (screenLayer.IsActive)
			{
				_sortedActiveLayersCopyForUpdate[num2] = screenLayer;
				num2++;
			}
		}
		for (int num3 = num2 - 1; num3 >= 0; num3--)
		{
			ScreenLayer screenLayer2 = _sortedActiveLayersCopyForUpdate[num3];
			if (!screenLayer2.Finalized)
			{
				screenLayer2.ProcessEvents();
			}
		}
		for (int k = 0; k < _sortedActiveLayersCopyForUpdate.Length; k++)
		{
			_sortedActiveLayersCopyForUpdate[k] = null;
		}
	}

	private static void LateUpdate(float dt, bool activeMouseVisible)
	{
		for (int i = 0; i < SortedLayers.Count; i++)
		{
			ScreenLayer screenLayer = SortedLayers[i];
			if (screenLayer != null && screenLayer.IsActive)
			{
				screenLayer.LateProcessEvents();
			}
		}
		for (int j = 0; j < SortedLayers.Count; j++)
		{
			ScreenLayer screenLayer2 = SortedLayers[j];
			if (screenLayer2 != null && screenLayer2.IsActive)
			{
				screenLayer2.OnLateUpdate(dt);
				if (screenLayer2 != FocusedLayer || _focusedLayerChangedThisFrame)
				{
					screenLayer2.Input.ResetLastDownKeys();
				}
			}
		}
		if (!_focusedLayerChangedThisFrame)
		{
			FocusedLayer?.Input?.UpdateLastDownKeys();
		}
		_focusedLayerChangedThisFrame = false;
		FirstHitLayer = null;
		UpdateMouseVisibility(activeMouseVisible);
		if (_globalOrderDirty)
		{
			RefreshGlobalOrder();
		}
	}

	internal static void UpdateMouseVisibility(bool activeMouseVisible)
	{
		for (int i = 0; i < SortedLayers.Count; i++)
		{
			ScreenLayer screenLayer = SortedLayers[i];
			if (screenLayer.IsActive && screenLayer.InputRestrictions.MouseVisibility)
			{
				if (!_activeMouseVisible)
				{
					SetMouseVisible(value: true);
				}
				return;
			}
		}
		if (_activeMouseVisible)
		{
			SetMouseVisible(value: false);
		}
	}

	public static bool IsControllerActive()
	{
		if (Input.IsControllerConnected && Input.IsGamepadActive && !Input.IsMouseActive)
		{
			return _engineInterface.GetMouseVisible();
		}
		return false;
	}

	public static bool IsMouseCursorHidden()
	{
		if (!Input.IsMouseActive)
		{
			return _engineInterface.GetMouseVisible();
		}
		return false;
	}

	public static bool IsMouseCursorActive()
	{
		if (Input.IsMouseActive)
		{
			return _engineInterface.GetMouseVisible();
		}
		return false;
	}

	public static bool IsLayerBlockedAtPosition(ScreenLayer layer, Vector2 position)
	{
		for (int num = SortedLayers.Count - 1; num >= 0; num--)
		{
			ScreenLayer screenLayer = SortedLayers[num];
			if (layer == screenLayer)
			{
				return false;
			}
			if (screenLayer != null && screenLayer.IsActive && !screenLayer.Finalized && screenLayer.HitTest(position))
			{
				if (screenLayer.InputUsageMask.HasAnyFlag(InputUsageMask.MouseButtons))
				{
					return layer != SortedLayers[num];
				}
				if (screenLayer.InputUsageMask.HasAnyFlag(InputUsageMask.MouseWheels))
				{
					return layer != SortedLayers[num];
				}
			}
		}
		return false;
	}

	private static void SetMouseVisible(bool value)
	{
		_activeMouseVisible = value;
		_engineInterface.SetMouseVisible(value);
	}

	public static bool GetMouseVisibility()
	{
		return _activeMouseVisible;
	}

	public static void TrySetFocus(ScreenLayer layer)
	{
		if ((FocusedLayer != null && FocusedLayer.InputRestrictions.Order > layer.InputRestrictions.Order && layer.IsActive) || (!layer.IsFocusLayer && !layer.FocusTest()))
		{
			return;
		}
		if (FocusedLayer != layer)
		{
			_focusedLayerChangedThisFrame = true;
			if (FocusedLayer != null)
			{
				FocusedLayer.OnLoseFocus();
			}
		}
		FocusedLayer = layer;
	}

	public static void TryLoseFocus(ScreenLayer layer)
	{
		if (FocusedLayer != layer)
		{
			return;
		}
		FocusedLayer?.OnLoseFocus();
		for (int num = SortedLayers.Count - 1; num >= 0; num--)
		{
			ScreenLayer screenLayer = SortedLayers[num];
			if (screenLayer.IsActive && screenLayer.IsFocusLayer && layer != screenLayer)
			{
				FocusedLayer = screenLayer;
				_focusedLayerChangedThisFrame = true;
				return;
			}
		}
		FocusedLayer = null;
	}

	private static bool FocusTest(ScreenLayer layer)
	{
		if (Input.IsGamepadActive && layer.InputRestrictions.CanOverrideFocusOnHit)
		{
			return layer.IsHitThisFrame;
		}
		if (FocusedLayer == layer)
		{
			return true;
		}
		return false;
	}

	public static void OnScaleChange(float newScale)
	{
		Scale = newScale;
		foreach (GlobalLayer globalLayer in _globalLayers)
		{
			globalLayer.UpdateLayout();
		}
		foreach (ScreenBase screen in _screenList)
		{
			screen.UpdateLayout();
		}
	}

	public static void OnControllerDisconnect()
	{
		ScreenManager.OnControllerDisconnected?.Invoke();
	}

	private static void OnScreenListChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		TaleWorlds.Library.Debug.Print("OnScreenListChanged");
		_isSortedActiveLayersDirty = true;
		ObservableCollection<ScreenBase> screenList = _screenList;
		if (screenList != null && screenList.Count > 0)
		{
			if (TopScreen != null)
			{
				TopScreen.OnAddLayer -= OnLayerAddedToTopLayer;
				TopScreen.OnRemoveLayer -= OnLayerRemovedFromTopLayer;
			}
			TopScreen = _screenList[_screenList.Count - 1];
			if (TopScreen != null)
			{
				TopScreen.OnAddLayer += OnLayerAddedToTopLayer;
				TopScreen.OnRemoveLayer += OnLayerRemovedFromTopLayer;
			}
		}
		else
		{
			if (TopScreen != null)
			{
				TopScreen.OnAddLayer -= OnLayerAddedToTopLayer;
				TopScreen.OnRemoveLayer -= OnLayerRemovedFromTopLayer;
			}
			TopScreen = null;
		}
		_isSortedActiveLayersDirty = true;
	}

	private static void OnLayerAddedToTopLayer(ScreenLayer layer)
	{
		_isSortedActiveLayersDirty = true;
	}

	private static void OnLayerRemovedFromTopLayer(ScreenLayer layer)
	{
		_isSortedActiveLayersDirty = true;
	}

	private static void OnGlobalListChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		_isSortedActiveLayersDirty = true;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_screen_debug_information_enabled", "ui")]
	public static string SetScreenDebugInformationEnabled(List<string> args)
	{
		string result = "Usage: ui.set_screen_debug_information_enabled [True/False]";
		if (args.Count != 1)
		{
			return result;
		}
		if (bool.TryParse(args[0], out var result2))
		{
			SetScreenDebugInformationEnabled(result2);
			return "Success.";
		}
		return result;
	}

	public static void SetScreenDebugInformationEnabled(bool isEnabled)
	{
		_isScreenDebugInformationEnabled = isEnabled;
	}

	private static void ShowScreenDebugInformation()
	{
		if (!_isScreenDebugInformationEnabled)
		{
			return;
		}
		_engineInterface.BeginDebugPanel("Screen Debug Information");
		for (int i = 0; i < SortedLayers.Count; i++)
		{
			ScreenLayer screenLayer = SortedLayers[i];
			if (_engineInterface.DrawDebugTreeNode($"{screenLayer.GetType().Name}###{screenLayer.Name}.{i}.{screenLayer.Name.GetDeterministicHashCode()}"))
			{
				screenLayer.DrawDebugInfo();
				_engineInterface.PopDebugTreeNode();
			}
		}
		_engineInterface.EndDebugPanel();
	}

	private static void OnUsableAreaChanged(Vec2 newUsableArea)
	{
		UpdateLayout();
	}
}
