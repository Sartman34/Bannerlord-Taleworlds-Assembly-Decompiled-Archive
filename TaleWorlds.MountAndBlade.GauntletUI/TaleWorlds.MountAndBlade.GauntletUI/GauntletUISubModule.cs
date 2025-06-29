using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Information.RundownTooltip;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Options;
using TaleWorlds.GauntletUI.ExtraWidgets;
using TaleWorlds.GauntletUI.GamepadNavigation;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade.GauntletUI.SceneNotification;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletUISubModule : MBSubModuleBase
{
	private bool _initialized;

	private bool _isMultiplayer;

	private GauntletQueryManager _queryManager;

	private LoadingWindowManager _loadingWindowManager;

	private SpriteCategory _fullBackgroundCategory;

	private SpriteCategory _backgroundCategory;

	private SpriteCategory _fullscreensCategory;

	private bool _isTouchpadMouseActive;

	public static GauntletUISubModule Instance { get; private set; }

	protected override void OnSubModuleLoad()
	{
		base.OnSubModuleLoad();
		ResourceDepot resourceDepot = new ResourceDepot();
		resourceDepot.AddLocation(BasePath.Name, "GUI/GauntletUI/");
		List<string> list = new List<string>();
		string[] modulesNames = Utilities.GetModulesNames();
		for (int i = 0; i < modulesNames.Length; i++)
		{
			ModuleInfo moduleInfo = ModuleHelper.GetModuleInfo(modulesNames[i]);
			if (moduleInfo == null)
			{
				continue;
			}
			string folderPath = moduleInfo.FolderPath;
			if (Directory.Exists(folderPath + "/GUI/"))
			{
				resourceDepot.AddLocation(folderPath, "/GUI/");
			}
			foreach (SubModuleInfo subModule in moduleInfo.SubModules)
			{
				if (subModule != null && subModule.DLLExists && !string.IsNullOrEmpty(subModule.DLLName))
				{
					list.Add(subModule.DLLName);
				}
			}
		}
		resourceDepot.CollectResources();
		CustomWidgetManager.Initilize();
		BannerlordCustomWidgetManager.Initialize();
		UIResourceManager.Initialize(resourceDepot, list);
		UIResourceManager.WidgetFactory.GeneratedPrefabContext.CollectPrefabs();
		SpriteData spriteData = UIResourceManager.SpriteData;
		TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
		_fullBackgroundCategory = spriteData.SpriteCategories["ui_fullbackgrounds"];
		_fullBackgroundCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);
		_backgroundCategory = spriteData.SpriteCategories["ui_backgrounds"];
		_backgroundCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);
		_fullscreensCategory = spriteData.SpriteCategories["ui_fullscreens"];
		_fullscreensCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);
		SpriteCategory[] array = spriteData.SpriteCategories.Values.Where((SpriteCategory x) => x.AlwaysLoad).ToArray();
		int num = array.Length;
		float num2 = 0.2f / (float)(num - 1);
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Load(resourceContext, resourceDepot);
			Utilities.SetLoadingScreenPercentage(0.4f + (float)j * num2);
		}
		Utilities.SetLoadingScreenPercentage(0.6f);
		ScreenManager.OnControllerDisconnected += OnControllerDisconnected;
		ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Combine(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		Input.OnControllerTypeChanged = (Action<Input.ControllerTypes>)Delegate.Combine(Input.OnControllerTypeChanged, new Action<Input.ControllerTypes>(OnControllerTypeChanged));
		NativeOptions.GetConfig(NativeOptions.NativeOptionsType.DisplayMode);
		GauntletGamepadNavigationManager.Initialize();
		Instance = this;
	}

	private void OnControllerTypeChanged(Input.ControllerTypes newType)
	{
		bool isTouchpadMouseActive = _isTouchpadMouseActive;
		if (newType == Input.ControllerTypes.PlayStationDualSense || newType == Input.ControllerTypes.PlayStationDualShock)
		{
			_isTouchpadMouseActive = NativeOptions.GetConfig(NativeOptions.NativeOptionsType.EnableTouchpadMouse) != 0f;
		}
		if (isTouchpadMouseActive != _isTouchpadMouseActive && !(ScreenManager.TopScreen is GauntletInitialScreen))
		{
			TextObject textObject = new TextObject("{=qkPfC3Cb}Warning");
			InformationManager.ShowInquiry(new InquiryData(text: new TextObject("{=LDRV5PxX}Touchpad Mouse setting won't take affect correctly until returning to initial menu! Exiting to main menu is recommended!").ToString(), titleText: textObject.ToString(), isAffirmativeOptionShown: true, isNegativeOptionShown: false, affirmativeText: new TextObject("{=yS7PvrTD}OK").ToString(), negativeText: null, affirmativeAction: null, negativeAction: null), pauseGameActiveState: false, prioritize: true);
		}
	}

	private void OnControllerDisconnected()
	{
	}

	private void OnManagedOptionChanged(ManagedOptions.ManagedOptionsType changedManagedOptionsType)
	{
		switch (changedManagedOptionsType)
		{
		case ManagedOptions.ManagedOptionsType.Language:
			UIResourceManager.OnLanguageChange(BannerlordConfig.Language);
			ScreenManager.UpdateLayout();
			break;
		case ManagedOptions.ManagedOptionsType.UIScale:
			ScreenManager.OnScaleChange(BannerlordConfig.UIScale);
			break;
		}
	}

	protected override void OnSubModuleUnloaded()
	{
		ScreenManager.OnControllerDisconnected -= OnControllerDisconnected;
		ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Remove(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		Input.OnControllerTypeChanged = (Action<Input.ControllerTypes>)Delegate.Remove(Input.OnControllerTypeChanged, new Action<Input.ControllerTypes>(OnControllerTypeChanged));
		UIResourceManager.Clear();
		LoadingWindow.Destroy();
		if (GauntletGamepadNavigationManager.Instance != null)
		{
			GauntletGamepadNavigationManager.Instance.OnFinalize();
		}
		Instance = null;
		base.OnSubModuleUnloaded();
	}

	protected override void OnBeforeInitialModuleScreenSetAsRoot()
	{
		if (!_initialized)
		{
			if (!Utilities.CommandLineArgumentExists("VisualTests"))
			{
				GauntletInformationView.Initialize();
				GauntletGameNotification.Initialize();
				GauntletSceneNotification.Initialize();
				GauntletSceneNotification.Current.RegisterContextProvider(new NativeSceneNotificationContextProvider());
				GauntletChatLogView.Initialize();
				GauntletGamepadCursor.Initialize();
				InformationManager.RegisterTooltip<List<TooltipProperty>, PropertyBasedTooltipVM>(PropertyBasedTooltipVM.RefreshGenericPropertyBasedTooltip, "PropertyBasedTooltip");
				InformationManager.RegisterTooltip<RundownLineVM, RundownTooltipVM>(RundownTooltipVM.RefreshGenericRundownTooltip, "RundownTooltip");
				InformationManager.RegisterTooltip<string, HintVM>(HintVM.RefreshGenericHintTooltip, "HintTooltip");
				_queryManager = new GauntletQueryManager();
				_queryManager.Initialize();
				_queryManager.InitializeKeyVisuals();
			}
			_loadingWindowManager = new LoadingWindowManager();
			LoadingWindow.Initialize(_loadingWindowManager);
			UIResourceManager.OnLanguageChange(BannerlordConfig.Language);
			ScreenManager.OnScaleChange(BannerlordConfig.UIScale);
			_initialized = true;
		}
	}

	public override void OnMultiplayerGameStart(Game game, object starterObject)
	{
		base.OnMultiplayerGameStart(game, starterObject);
		if (!_isMultiplayer)
		{
			_loadingWindowManager.SetCurrentModeIsMultiplayer(isMultiplayer: true);
			_isMultiplayer = true;
		}
	}

	public override void OnGameEnd(Game game)
	{
		base.OnGameEnd(game);
		if (_isMultiplayer)
		{
			_loadingWindowManager.SetCurrentModeIsMultiplayer(isMultiplayer: false);
			_isMultiplayer = false;
		}
	}

	protected override void OnApplicationTick(float dt)
	{
		base.OnApplicationTick(dt);
		UIResourceManager.Update();
		if (GauntletGamepadNavigationManager.Instance != null && ScreenManager.GetMouseVisibility())
		{
			GauntletGamepadNavigationManager.Instance.Update(dt);
		}
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("clear", "chatlog")]
	public static string ClearChatLog(List<string> strings)
	{
		InformationManager.ClearAllMessages();
		return "Chatlog cleared!";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("can_focus_while_in_mission", "chatlog")]
	public static string SetCanFocusWhileInMission(List<string> strings)
	{
		if (strings[0] == "0" || strings[0] == "1")
		{
			GauntletChatLogView.Current.SetCanFocusWhileInMission(strings[0] == "1");
			return "Chat window will" + ((strings[0] == "1") ? " " : " NOT ") + " be able to gain focus now.";
		}
		return "Wrong input";
	}
}
