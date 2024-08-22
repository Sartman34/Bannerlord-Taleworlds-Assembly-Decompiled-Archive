using System;
using System.Collections.Generic;
using System.IO;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.InputSystem;
using TaleWorlds.Engine.Options;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade.GameKeyCategory;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View;

public class ViewSubModule : MBSubModuleBase
{
	private Dictionary<Tuple<Material, BannerCode>, Material> _bannerTexturedMaterialCache;

	private GameStateScreenManager _gameStateScreenManager;

	private bool _newGameInitialization;

	private static ViewSubModule _instance;

	private bool _initialized;

	public static Dictionary<Tuple<Material, BannerCode>, Material> BannerTexturedMaterialCache
	{
		get
		{
			return _instance._bannerTexturedMaterialCache;
		}
		set
		{
			_instance._bannerTexturedMaterialCache = value;
		}
	}

	public static GameStateScreenManager GameStateScreenManager => _instance._gameStateScreenManager;

	private void InitializeHotKeyManager(bool loadKeys)
	{
		string fileName = "BannerlordGameKeys.xml";
		HotKeyManager.Initialize(new PlatformFilePath(EngineFilePaths.ConfigsPath, fileName), !ScreenManager.IsEnterButtonRDown);
		HotKeyManager.RegisterInitialContexts(new List<GameKeyContext>
		{
			new GenericGameKeyContext(),
			new GenericCampaignPanelsGameKeyCategory(),
			new GenericPanelGameKeyCategory(),
			new ArmyManagementHotkeyCategory(),
			new BoardGameHotkeyCategory(),
			new ChatLogHotKeyCategory(),
			new CombatHotKeyCategory(),
			new CraftingHotkeyCategory(),
			new FaceGenHotkeyCategory(),
			new InventoryHotKeyCategory(),
			new PartyHotKeyCategory(),
			new MapHotKeyCategory(),
			new MapNotificationHotKeyCategory(),
			new MissionOrderHotkeyCategory(),
			new MultiplayerHotkeyCategory(),
			new ScoreboardHotKeyCategory(),
			new ConversationHotKeyCategory(),
			new CheatsHotKeyCategory(),
			new PhotoModeHotKeyCategory(),
			new PollHotkeyCategory()
		}, loadKeys);
	}

	private void InitializeBannerVisualManager()
	{
		if (BannerManager.Instance != null)
		{
			return;
		}
		BannerManager.Initialize();
		BannerManager.Instance.LoadBannerIcons(ModuleHelper.GetModuleFullPath("Native") + "ModuleData/banner_icons.xml");
		string[] modulesNames = Utilities.GetModulesNames();
		for (int i = 0; i < modulesNames.Length; i++)
		{
			ModuleInfo moduleInfo = ModuleHelper.GetModuleInfo(modulesNames[i]);
			if (moduleInfo != null && !moduleInfo.IsNative)
			{
				string text = moduleInfo.FolderPath + "/ModuleData/banner_icons.xml";
				if (File.Exists(text))
				{
					BannerManager.Instance.LoadBannerIcons(text);
				}
			}
		}
	}

	protected override void OnSubModuleLoad()
	{
		base.OnSubModuleLoad();
		_instance = this;
		InitializeHotKeyManager(loadKeys: false);
		InitializeBannerVisualManager();
		CraftedDataViewManager.Initialize();
		ScreenManager.OnPushScreen += OnScreenManagerPushScreen;
		_gameStateScreenManager = new GameStateScreenManager();
		Module.CurrentModule.GlobalGameStateManager.RegisterListener(_gameStateScreenManager);
		MBMusicManager.Create();
		TextObject coreContentDisabledReason = new TextObject("{=V8BXjyYq}Disabled during installation.");
		if (Utilities.EditModeEnabled)
		{
			Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Editor", new TextObject("{=bUh0x6rA}Editor"), -1, delegate
			{
				MBInitialScreenBase.OnEditModeEnterPress();
			}, () => (Module.CurrentModule.IsOnlyCoreContentEnabled, coreContentDisabledReason)));
		}
		Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Options", new TextObject("{=NqarFr4P}Options"), 9998, delegate
		{
			ScreenManager.PushScreen(ViewCreator.CreateOptionsScreen(fromMainMenu: true));
		}, () => (false, TextObject.Empty)));
		Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Credits", new TextObject("{=ODQmOrIw}Credits"), 9999, delegate
		{
			ScreenManager.PushScreen(ViewCreator.CreateCreditsScreen());
		}, () => (false, TextObject.Empty)));
		Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Exit", new TextObject("{=YbpzLHzk}Exit Game"), 10000, delegate
		{
			MBInitialScreenBase.DoExitButtonAction();
		}, () => (Module.CurrentModule.IsOnlyCoreContentEnabled, coreContentDisabledReason)));
		Module.CurrentModule.ImguiProfilerTick += OnImguiProfilerTick;
		Input.OnControllerTypeChanged = (Action<Input.ControllerTypes>)Delegate.Combine(Input.OnControllerTypeChanged, new Action<Input.ControllerTypes>(OnControllerTypeChanged));
		NativeOptions.OnNativeOptionChanged = (NativeOptions.OnNativeOptionChangedDelegate)Delegate.Combine(NativeOptions.OnNativeOptionChanged, new NativeOptions.OnNativeOptionChangedDelegate(OnNativeOptionChanged));
		ViewModel.CollectPropertiesAndMethods();
		HyperlinkTexts.IsPlayStationGamepadActive = GetIsPlaystationGamepadActive;
		EngineController.OnConstrainedStateChanged += OnConstrainedStateChange;
	}

	private void OnConstrainedStateChange(bool isConstrained)
	{
		ScreenManager.OnConstrainStateChanged(isConstrained);
	}

	private bool GetIsPlaystationGamepadActive()
	{
		if (Input.IsGamepadActive)
		{
			if (Input.ControllerType != Input.ControllerTypes.PlayStationDualSense)
			{
				return Input.ControllerType == Input.ControllerTypes.PlayStationDualShock;
			}
			return true;
		}
		return false;
	}

	private void OnControllerTypeChanged(Input.ControllerTypes newType)
	{
		ReInitializeHotKeyManager();
	}

	private void OnNativeOptionChanged(NativeOptions.NativeOptionsType changedNativeOptionsType)
	{
		if (changedNativeOptionsType == NativeOptions.NativeOptionsType.EnableTouchpadMouse)
		{
			ReInitializeHotKeyManager();
		}
	}

	private void ReInitializeHotKeyManager()
	{
		InitializeHotKeyManager(loadKeys: true);
	}

	protected override void OnSubModuleUnloaded()
	{
		ScreenManager.OnPushScreen -= OnScreenManagerPushScreen;
		NativeOptions.OnNativeOptionChanged = (NativeOptions.OnNativeOptionChangedDelegate)Delegate.Remove(NativeOptions.OnNativeOptionChanged, new NativeOptions.OnNativeOptionChangedDelegate(OnNativeOptionChanged));
		TableauCacheManager.ClearManager();
		BannerlordTableauManager.ClearManager();
		CraftedDataViewManager.Clear();
		Module.CurrentModule.ImguiProfilerTick -= OnImguiProfilerTick;
		Input.OnControllerTypeChanged = (Action<Input.ControllerTypes>)Delegate.Remove(Input.OnControllerTypeChanged, new Action<Input.ControllerTypes>(OnControllerTypeChanged));
		_instance = null;
		EngineController.OnConstrainedStateChanged -= OnConstrainedStateChange;
		base.OnSubModuleUnloaded();
	}

	protected override void OnBeforeInitialModuleScreenSetAsRoot()
	{
		if (!_initialized)
		{
			HotKeyManager.Load();
			BannerlordTableauManager.InitializeCharacterTableauRenderSystem();
			TableauCacheManager.InitializeManager();
			_initialized = true;
		}
	}

	protected override void OnApplicationTick(float dt)
	{
		base.OnApplicationTick(dt);
		if (Input.DebugInput.IsHotKeyPressed("ToggleUI"))
		{
			MBDebug.DisableUI(new List<string>());
		}
		HotKeyManager.Tick(dt);
		MBMusicManager.Current?.Update(dt);
		TableauCacheManager.Current?.Tick();
	}

	protected override void AfterAsyncTickTick(float dt)
	{
		MBMusicManager.Current?.Update(dt);
	}

	protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
	{
		MissionWeapon.OnGetWeaponDataHandler = ItemCollectionElementViewExtensions.OnGetWeaponData;
	}

	public override void OnCampaignStart(Game game, object starterObject)
	{
		Game.Current.GameStateManager.RegisterListener(_gameStateScreenManager);
		_newGameInitialization = false;
	}

	public override void OnMultiplayerGameStart(Game game, object starterObject)
	{
		Game.Current.GameStateManager.RegisterListener(_gameStateScreenManager);
	}

	public override void OnGameLoaded(Game game, object initializerObject)
	{
		Game.Current.GameStateManager.RegisterListener(_gameStateScreenManager);
	}

	public override void OnGameInitializationFinished(Game game)
	{
		base.OnGameInitializationFinished(game);
		foreach (ItemObject objectType in Game.Current.ObjectManager.GetObjectTypeList<ItemObject>())
		{
			if (objectType.MultiMeshName != "")
			{
				MBUnusedResourceManager.SetMeshUsed(objectType.MultiMeshName);
			}
			HorseComponent horseComponent = objectType.HorseComponent;
			if (horseComponent != null)
			{
				foreach (KeyValuePair<string, bool> additionalMeshesName in horseComponent.AdditionalMeshesNameList)
				{
					MBUnusedResourceManager.SetMeshUsed(additionalMeshesName.Key);
				}
			}
			if (objectType.PrimaryWeapon != null)
			{
				MBUnusedResourceManager.SetMeshUsed(objectType.HolsterMeshName);
				MBUnusedResourceManager.SetMeshUsed(objectType.HolsterWithWeaponMeshName);
				MBUnusedResourceManager.SetMeshUsed(objectType.FlyingMeshName);
				MBUnusedResourceManager.SetBodyUsed(objectType.BodyName);
				MBUnusedResourceManager.SetBodyUsed(objectType.HolsterBodyName);
				MBUnusedResourceManager.SetBodyUsed(objectType.CollisionBodyName);
			}
		}
	}

	public override void BeginGameStart(Game game)
	{
		base.BeginGameStart(game);
		Game.Current.BannerVisualCreator = new BannerVisualCreator();
	}

	public override bool DoLoading(Game game)
	{
		if (_newGameInitialization)
		{
			return true;
		}
		_newGameInitialization = true;
		return _newGameInitialization;
	}

	public override void OnGameEnd(Game game)
	{
		MissionWeapon.OnGetWeaponDataHandler = null;
		CraftedDataViewManager.Clear();
	}

	private void OnImguiProfilerTick()
	{
		TableauCacheManager.Current.OnImguiProfilerTick();
	}

	private void OnScreenManagerPushScreen(ScreenBase pushedScreen)
	{
	}
}
