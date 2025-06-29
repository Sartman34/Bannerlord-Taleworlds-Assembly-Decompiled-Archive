using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using TaleWorlds.AchievementSystem;
using TaleWorlds.ActivitySystem;
using TaleWorlds.Avatar.PlayerServices;
using TaleWorlds.Core;
using TaleWorlds.Diamond.ClientApplication;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Options;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlatformService;
using TaleWorlds.SaveSystem;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade;

public sealed class Module : DotNetObject, IGameStateManagerOwner
{
	public enum XmlInformationType
	{
		Parameters,
		MbObjectType
	}

	private enum StartupType
	{
		None,
		TestMode,
		GameServer,
		Singleplayer,
		Multiplayer,
		Count
	}

	private TestContext _testContext;

	private List<MissionInfo> _missionInfos;

	private Dictionary<string, Type> _loadedSubmoduleTypes;

	private readonly MBList<MBSubModuleBase> _submodules;

	private SingleThreadedSynchronizationContext _synchronizationContext;

	private bool _enableCoreContentOnReturnToRoot;

	private bool _splashScreenPlayed;

	private List<InitialStateOption> _initialStateOptions;

	private IEditorMissionTester _editorMissionTester;

	private Dictionary<string, MultiplayerGameMode> _multiplayerGameModesWithNames;

	private MBList<MultiplayerGameTypeInfo> _multiplayerGameTypes = new MBList<MultiplayerGameTypeInfo>();

	private bool _isShuttingDown;

	public GameTextManager GlobalTextManager { get; private set; }

	public JobManager JobManager { get; private set; }

	public MBReadOnlyList<MBSubModuleBase> SubModules => _submodules;

	public GameStateManager GlobalGameStateManager { get; private set; }

	public bool ReturnToEditorState { get; private set; }

	public bool LoadingFinished { get; private set; }

	public bool IsOnlyCoreContentEnabled { get; private set; }

	public bool MultiplayerRequested
	{
		get
		{
			if (StartupInfo.StartupType != GameStartupType.Multiplayer && PlatformServices.SessionInvitationType != SessionInvitationType.Multiplayer)
			{
				return PlatformServices.IsPlatformRequestedMultiplayer;
			}
			return true;
		}
	}

	public GameStartupInfo StartupInfo { get; private set; }

	public static Module CurrentModule { get; private set; }

	public event Action SkinsXMLHasChanged;

	public event Action ImguiProfilerTick;

	private Module()
	{
		MBDebug.Print("Creating module...");
		StartupInfo = new GameStartupInfo();
		_testContext = new TestContext();
		_loadedSubmoduleTypes = new Dictionary<string, Type>();
		_submodules = new MBList<MBSubModuleBase>();
		GlobalGameStateManager = new GameStateManager(this, GameStateManager.GameStateManagerType.Global);
		GameStateManager.Current = GlobalGameStateManager;
		GlobalTextManager = new GameTextManager();
		JobManager = new JobManager();
	}

	internal static void CreateModule()
	{
		CurrentModule = new Module();
		Utilities.SetLoadingScreenPercentage(0.4f);
	}

	private void AddSubModule(Assembly subModuleAssembly, string name)
	{
		Type type = subModuleAssembly.GetType(name);
		_loadedSubmoduleTypes.Add(name, type);
		Managed.AddTypes(CollectModuleAssemblyTypes(subModuleAssembly));
	}

	private Dictionary<string, Type> CollectModuleAssemblyTypes(Assembly moduleAssembly)
	{
		try
		{
			Dictionary<string, Type> dictionary = new Dictionary<string, Type>();
			Type[] types = moduleAssembly.GetTypes();
			foreach (Type type in types)
			{
				if (typeof(ManagedObject).IsAssignableFrom(type) || typeof(DotNetObject).IsAssignableFrom(type))
				{
					dictionary.Add(type.Name, type);
				}
			}
			return dictionary;
		}
		catch (Exception ex)
		{
			MBDebug.Print("Error while getting types and loading" + ex.Message);
			if (ex is ReflectionTypeLoadException ex2)
			{
				string text = "";
				Exception[] loaderExceptions = ex2.LoaderExceptions;
				foreach (Exception ex3 in loaderExceptions)
				{
					MBDebug.Print("Loader Exceptions: " + ex3.Message);
					text = text + ex3.Message + Environment.NewLine;
				}
				TaleWorlds.Library.Debug.SetCrashReportCustomString(text);
				Type[] types = ex2.Types;
				foreach (Type type2 in types)
				{
					if (type2 != null)
					{
						MBDebug.Print("Loaded Types: " + type2.FullName);
					}
				}
			}
			if (ex.InnerException != null)
			{
				MBDebug.Print("Inner excetion: " + ex.StackTrace);
			}
			throw;
		}
	}

	private void InitializeSubModules()
	{
		Managed.AddConstructorDelegateOfClass<SpawnedItemEntity>();
		foreach (Type value in _loadedSubmoduleTypes.Values)
		{
			MBSubModuleBase mBSubModuleBase = (MBSubModuleBase)value.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, new Type[0], null).Invoke(new object[0]);
			_submodules.Add(mBSubModuleBase);
			mBSubModuleBase.OnSubModuleLoad();
		}
	}

	private void FinalizeSubModules()
	{
		foreach (MBSubModuleBase submodule in _submodules)
		{
			submodule.OnSubModuleUnloaded();
		}
	}

	public Type GetSubModule(string name)
	{
		return _loadedSubmoduleTypes[name];
	}

	[MBCallback]
	internal void Initialize()
	{
		MBDebug.Print("Module Initialize begin...");
		TWParallel.InitializeAndSetImplementation(new NativeParallelDriver());
		MBSaveLoad.SetSaveDriver(new AsyncFileSaveDriver());
		ProcessApplicationArguments();
		SetWindowTitle();
		_initialStateOptions = new List<InitialStateOption>();
		FillMultiplayerGameTypes();
		if (!GameNetwork.IsDedicatedServer && !MBDebug.TestModeEnabled)
		{
			MBDebug.Print("Loading platform services...");
			LoadPlatformServices();
		}
		string[] modulesNames = Utilities.GetModulesNames();
		List<string> list = new List<string>();
		string[] array = modulesNames;
		for (int i = 0; i < array.Length; i++)
		{
			ModuleInfo moduleInfo = ModuleHelper.GetModuleInfo(array[i]);
			if (moduleInfo != null)
			{
				list.Add(moduleInfo.FolderPath);
			}
		}
		LocalizedTextManager.LoadLocalizationXmls(list.ToArray());
		GlobalTextManager.LoadDefaultTexts();
		IsOnlyCoreContentEnabled = Utilities.IsOnlyCoreContentEnabled();
		NativeConfig.OnConfigChanged();
		LoadSubModules();
		MBDebug.Print("Adding trace listener...");
		MBDebug.Print("MBModuleBase Initialize begin...");
		MBDebug.Print("MBModuleBase Initialize end...");
		GameNetwork.FindGameNetworkMessages();
		GameNetwork.FindSynchedMissionObjectTypes();
		HasTableauCache.CollectTableauCacheTypes();
		MBDebug.Print("Module Initialize end...");
		MBDebug.TestModeEnabled = Utilities.CommandLineArgumentExists("/runTest");
		FindMissions();
		NativeOptions.ReadRGLConfigFiles();
		BannerlordConfig.Initialize();
		EngineController.ConfigChange += OnConfigChanged;
		EngineController.OnConstrainedStateChanged += OnConstrainedStateChange;
		ScreenManager.FocusGained += OnFocusGained;
		ScreenManager.PlatformTextRequested += OnPlatformTextRequested;
		PlatformServices.Instance.OnTextEnteredFromPlatform += OnTextEnteredFromPlatform;
		SaveManager.InitializeGlobalDefinitionContext();
		EnsureAsyncJobsAreFinished();
	}

	private void OnPlatformTextRequested(string initialText, string descriptionText, int maxLength, int keyboardTypeEnum)
	{
		PlatformServices.Instance?.ShowGamepadTextInput(descriptionText, initialText, (uint)maxLength, keyboardTypeEnum == 2);
	}

	private void SetWindowTitle()
	{
		string applicationName = Utilities.GetApplicationName();
		string title = ((StartupInfo.StartupType == GameStartupType.Singleplayer) ? (applicationName + " - Singleplayer") : ((StartupInfo.StartupType == GameStartupType.Multiplayer) ? (applicationName + " - Multiplayer") : ((StartupInfo.StartupType != GameStartupType.GameServer) ? applicationName : ("[" + Utilities.GetCurrentProcessID() + "] " + applicationName + " Dedicated Server Port:" + StartupInfo.ServerPort))));
		title = Utilities.ProcessWindowTitle(title);
		Utilities.SetWindowTitle(title);
	}

	private void EnsureAsyncJobsAreFinished()
	{
		if (!GameNetwork.IsDedicatedServer)
		{
			while (!MBMusicManager.IsCreationCompleted())
			{
				Thread.Sleep(1);
			}
		}
		if (!GameNetwork.IsDedicatedServer && !MBDebug.TestModeEnabled)
		{
			while (!AchievementManager.AchievementService.IsInitializationCompleted())
			{
				Thread.Sleep(1);
			}
		}
	}

	private void ProcessApplicationArguments()
	{
		StartupInfo.StartupType = GameStartupType.None;
		string[] array = Utilities.GetFullCommandLineString().Split(new char[1] { ' ' });
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].ToLowerInvariant();
			if (text == "/dedicatedmatchmakingserver".ToLower())
			{
				int serverPort = Convert.ToInt32(array[i + 1]);
				string serverRegion = array[i + 2];
				sbyte serverPriority = Convert.ToSByte(array[i + 3]);
				string serverGameMode = array[i + 4];
				i += 4;
				StartupInfo.StartupType = GameStartupType.GameServer;
				StartupInfo.DedicatedServerType = DedicatedServerType.Matchmaker;
				StartupInfo.ServerPort = serverPort;
				StartupInfo.ServerRegion = serverRegion;
				StartupInfo.ServerPriority = serverPriority;
				StartupInfo.ServerGameMode = serverGameMode;
			}
			else if (text == "/dedicatedcustomserver".ToLower())
			{
				int serverPort2 = Convert.ToInt32(array[i + 1]);
				string serverRegion2 = array[i + 2];
				int permission = Convert.ToInt32(array[i + 3]);
				i += 3;
				StartupInfo.StartupType = GameStartupType.GameServer;
				StartupInfo.DedicatedServerType = DedicatedServerType.Custom;
				StartupInfo.ServerPort = serverPort2;
				StartupInfo.ServerRegion = serverRegion2;
				StartupInfo.Permission = permission;
			}
			else if (text == "/dedicatedcommunityserver".ToLower())
			{
				int serverPort3 = Convert.ToInt32(array[i + 1]);
				i++;
				StartupInfo.StartupType = GameStartupType.GameServer;
				StartupInfo.DedicatedServerType = DedicatedServerType.Community;
				StartupInfo.ServerPort = serverPort3;
			}
			else if (text == "/dedicatedcustomserverconfigfile".ToLower())
			{
				string customGameServerConfigFile = array[i + 1];
				i++;
				StartupInfo.CustomGameServerConfigFile = customGameServerConfigFile;
			}
			else if (text == "/dedicatedcustomservernameoverride".ToLower())
			{
				string customGameServerNameOverride = array[i + 1];
				i++;
				StartupInfo.CustomGameServerNameOverride = customGameServerNameOverride;
			}
			else if (text == "/dedicatedcustomserverpasswordoverride".ToLower())
			{
				string customGameServerPasswordOverride = array[i + 1];
				i++;
				StartupInfo.CustomGameServerPasswordOverride = customGameServerPasswordOverride;
			}
			else if (text == "/dedicatedcustomserverauthtoken".ToLower())
			{
				string customGameServerAuthToken = array[i + 1];
				i++;
				StartupInfo.CustomGameServerAuthToken = customGameServerAuthToken;
			}
			else if (text == "/dedicatedcustomserverDontAllowOptionalModules".ToLower())
			{
				StartupInfo.CustomGameServerAllowsOptionalModules = false;
			}
			else if (text == "/playerHostedDedicatedServer".ToLower())
			{
				StartupInfo.PlayerHostedDedicatedServer = true;
			}
			else if (text == "/singleplatform")
			{
				StartupInfo.IsSinglePlatformServer = true;
			}
			else if (text == "/customserverhost")
			{
				string customServerHostIP = array[i + 1];
				i++;
				StartupInfo.CustomServerHostIP = customServerHostIP;
			}
			else if (text == "/singleplayer".ToLower())
			{
				StartupInfo.StartupType = GameStartupType.Singleplayer;
			}
			else if (text == "/multiplayer".ToLower())
			{
				StartupInfo.StartupType = GameStartupType.Multiplayer;
			}
			else if (text == "/clientConfigurationCategory".ToLower())
			{
				ClientApplicationConfiguration.SetDefualtConfigurationCategory(array[i + 1]);
				i++;
			}
			else if (text == "/overridenusername".ToLower())
			{
				string overridenUserName = array[i + 1];
				StartupInfo.OverridenUserName = overridenUserName;
				i++;
			}
			else if (text.StartsWith("-AUTH_PASSWORD".ToLowerInvariant()))
			{
				StartupInfo.EpicExchangeCode = text.Split(new char[1] { '=' })[1];
			}
			else if (text == "/continuegame".ToLower())
			{
				StartupInfo.IsContinueGame = true;
			}
			else if (text == "/serverbandwidthlimitmbps".ToLower())
			{
				double serverBandwidthLimitInMbps = Convert.ToDouble(array[i + 1]);
				StartupInfo.ServerBandwidthLimitInMbps = serverBandwidthLimitInMbps;
				i++;
			}
			else if (text == "/tickrate".ToLower())
			{
				int serverTickRate = Convert.ToInt32(array[i + 1]);
				StartupInfo.ServerTickRate = serverTickRate;
				i++;
			}
		}
	}

	internal void OnApplicationTick(float dt)
	{
		bool isOnlyCoreContentEnabled = IsOnlyCoreContentEnabled;
		IsOnlyCoreContentEnabled = Utilities.IsOnlyCoreContentEnabled();
		if (isOnlyCoreContentEnabled != IsOnlyCoreContentEnabled && isOnlyCoreContentEnabled)
		{
			InitialState initialState;
			if ((initialState = GameStateManager.Current.ActiveState as InitialState) != null)
			{
				Utilities.DisableCoreGame();
				InformationManager.ShowInquiry(new InquiryData(new TextObject("{=CaSafuAH}Content Download Complete").ToString(), new TextObject("{=1nKa4pQX}Rest of the game content has been downloaded.").ToString(), isAffirmativeOptionShown: true, isNegativeOptionShown: false, new TextObject("{=yS7PvrTD}OK").ToString(), null, delegate
				{
					initialState.RefreshContentState();
				}, null));
			}
			else
			{
				InformationManager.ShowInquiry(new InquiryData(new TextObject("{=CaSafuAH}Content Download Complete").ToString(), new TextObject("{=BFhMw4bl}Rest of the game content has been downloaded. Do you want to return to the main menu?").ToString(), isAffirmativeOptionShown: true, isNegativeOptionShown: true, new TextObject("{=aeouhelq}Yes").ToString(), new TextObject("{=8OkPHu4f}No").ToString(), OnConfirmReturnToMainMenu, null));
				_enableCoreContentOnReturnToRoot = true;
			}
		}
		if (_synchronizationContext == null)
		{
			_synchronizationContext = new SingleThreadedSynchronizationContext();
			SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
		}
		_testContext.OnApplicationTick(dt);
		if (!GameNetwork.MultiplayerDisabled)
		{
			OnNetworkTick(dt);
		}
		if (GameStateManager.Current == null)
		{
			GameStateManager.Current = GlobalGameStateManager;
		}
		if (GameStateManager.Current == GlobalGameStateManager)
		{
			if (LoadingFinished && GlobalGameStateManager.ActiveState == null)
			{
				if (ReturnToEditorState)
				{
					ReturnToEditorState = false;
					SetEditorScreenAsRootScreen();
				}
				else
				{
					SetInitialModuleScreenAsRootScreen();
				}
			}
			GlobalGameStateManager.OnTick(dt);
		}
		Utilities.RunJobs();
		PlatformServices.Instance?.Tick(dt);
		_synchronizationContext.Tick();
		if (GameManagerBase.Current != null)
		{
			GameManagerBase.Current.OnTick(dt);
		}
		foreach (MBSubModuleBase subModule in SubModules)
		{
			subModule.OnApplicationTick(dt);
		}
		JobManager.OnTick(dt);
		AvatarServices.UpdateAvatarServices(dt);
	}

	private void OnConfirmReturnToMainMenu()
	{
		MBGameManager.EndGame();
	}

	private void OnNetworkTick(float dt)
	{
		foreach (MBSubModuleBase subModule in SubModules)
		{
			subModule.OnNetworkTick(dt);
		}
	}

	[MBCallback]
	internal void RunTest(string commandLine)
	{
		MBDebug.Print(" TEST MODE ENABLED. Command line string: " + commandLine);
		_testContext.RunTestAux(commandLine);
	}

	[MBCallback]
	internal void TickTest(float dt)
	{
		_testContext.TickTest(dt);
	}

	[MBCallback]
	internal void OnDumpCreated()
	{
		if (TestCommonBase.BaseInstance != null)
		{
			TestCommonBase.BaseInstance.ToggleTimeoutTimer();
			TestCommonBase.BaseInstance.StartTimeoutTimer();
		}
	}

	[MBCallback]
	internal void OnDumpCreationStarted()
	{
		if (TestCommonBase.BaseInstance != null)
		{
			TestCommonBase.BaseInstance.ToggleTimeoutTimer();
		}
	}

	public static void GetMetaMeshPackageMapping(Dictionary<string, string> metaMeshPackageMappings)
	{
		foreach (ItemObject objectType in Game.Current.ObjectManager.GetObjectTypeList<ItemObject>())
		{
			if (objectType.HasArmorComponent)
			{
				string value = ((objectType.Culture != null) ? objectType.Culture.StringId : "shared") + "_armor";
				metaMeshPackageMappings[objectType.MultiMeshName] = value;
				metaMeshPackageMappings[objectType.MultiMeshName + "_converted"] = value;
				metaMeshPackageMappings[objectType.MultiMeshName + "_converted_slim"] = value;
				metaMeshPackageMappings[objectType.MultiMeshName + "_slim"] = value;
			}
			if (objectType.WeaponComponent != null)
			{
				string value2 = ((objectType.Culture != null) ? objectType.Culture.StringId : "shared") + "_weapon";
				metaMeshPackageMappings[objectType.MultiMeshName] = value2;
				if (objectType.HolsterMeshName != null)
				{
					metaMeshPackageMappings[objectType.HolsterMeshName] = value2;
				}
				if (objectType.HolsterWithWeaponMeshName != null)
				{
					metaMeshPackageMappings[objectType.HolsterWithWeaponMeshName] = value2;
				}
			}
			if (objectType.HasHorseComponent)
			{
				string value3 = "horses";
				metaMeshPackageMappings[objectType.MultiMeshName] = value3;
			}
			if (objectType.IsFood)
			{
				string value4 = "food";
				metaMeshPackageMappings[objectType.MultiMeshName] = value4;
			}
		}
		foreach (CraftingPiece objectType2 in Game.Current.ObjectManager.GetObjectTypeList<CraftingPiece>())
		{
			string value5 = ((objectType2.Culture != null) ? objectType2.Culture.StringId : "shared") + "_crafting";
			metaMeshPackageMappings[objectType2.MeshName] = value5;
		}
	}

	public static void GetItemMeshNames(HashSet<string> itemMeshNames)
	{
		foreach (ItemObject objectType in Game.Current.ObjectManager.GetObjectTypeList<ItemObject>())
		{
			if (!objectType.IsCraftedWeapon)
			{
				itemMeshNames.Add(objectType.MultiMeshName);
			}
			if (objectType.PrimaryWeapon != null)
			{
				if (objectType.FlyingMeshName != null && !objectType.FlyingMeshName.IsEmpty())
				{
					itemMeshNames.Add(objectType.FlyingMeshName);
				}
				if (objectType.HolsterMeshName != null && !objectType.HolsterMeshName.IsEmpty())
				{
					itemMeshNames.Add(objectType.HolsterMeshName);
				}
				if (objectType.HolsterWithWeaponMeshName != null && !objectType.HolsterWithWeaponMeshName.IsEmpty())
				{
					itemMeshNames.Add(objectType.HolsterWithWeaponMeshName);
				}
			}
			if (!objectType.HasHorseComponent)
			{
				continue;
			}
			foreach (KeyValuePair<string, bool> additionalMeshesName in objectType.HorseComponent.AdditionalMeshesNameList)
			{
				if (additionalMeshesName.Key != null && !additionalMeshesName.Key.IsEmpty())
				{
					itemMeshNames.Add(additionalMeshesName.Key);
				}
			}
		}
	}

	[MBCallback]
	internal string GetMetaMeshPackageMapping()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		GetMetaMeshPackageMapping(dictionary);
		string text = "";
		foreach (string key in dictionary.Keys)
		{
			text = text + key + "|" + dictionary[key] + ",";
		}
		return text;
	}

	[MBCallback]
	internal string GetItemMeshNames()
	{
		HashSet<string> hashSet = new HashSet<string>();
		GetItemMeshNames(hashSet);
		foreach (CraftingPiece objectType in MBObjectManager.Instance.GetObjectTypeList<CraftingPiece>())
		{
			hashSet.Add(objectType.MeshName);
			if (objectType.BladeData != null)
			{
				hashSet.Add(objectType.BladeData.HolsterMeshName);
			}
		}
		foreach (BannerIconGroup bannerIconGroup in BannerManager.Instance.BannerIconGroups)
		{
			foreach (KeyValuePair<int, BannerIconData> allIcon in bannerIconGroup.AllIcons)
			{
				if (allIcon.Value.MaterialName != "")
				{
					hashSet.Add(allIcon.Value.MaterialName + allIcon.Value.TextureIndex);
				}
			}
		}
		string text = "";
		foreach (string item in hashSet)
		{
			if (item != null && !item.IsEmpty())
			{
				text = text + item + "#";
			}
		}
		return text;
	}

	[MBCallback]
	internal string GetHorseMaterialNames()
	{
		HashSet<string> hashSet = new HashSet<string>();
		string text = "";
		foreach (ItemObject objectType in Game.Current.ObjectManager.GetObjectTypeList<ItemObject>())
		{
			if (!objectType.HasHorseComponent || objectType.HorseComponent.HorseMaterialNames == null || objectType.HorseComponent.HorseMaterialNames.Count <= 0)
			{
				continue;
			}
			foreach (HorseComponent.MaterialProperty horseMaterialName in objectType.HorseComponent.HorseMaterialNames)
			{
				hashSet.Add(horseMaterialName.Name);
			}
		}
		foreach (string item in hashSet)
		{
			if (item != null && !item.IsEmpty())
			{
				text = text + item + "#";
			}
		}
		return text;
	}

	public void SetInitialModuleScreenAsRootScreen()
	{
		if (GameStateManager.Current != GlobalGameStateManager)
		{
			GameStateManager.Current = GlobalGameStateManager;
		}
		foreach (MBSubModuleBase subModule in SubModules)
		{
			subModule.OnBeforeInitialModuleScreenSetAsRoot();
		}
		if (GameNetwork.IsDedicatedServer)
		{
			return;
		}
		string text = ModuleHelper.GetModuleFullPath("Native") + "Videos/TWLogo_and_Partners.ivf";
		string text2 = ModuleHelper.GetModuleFullPath("Native") + "Videos/TWLogo_and_Partners.ogg";
		if (!_splashScreenPlayed && File.Exists(text) && (text2 == "" || File.Exists(text2)) && !Debugger.IsAttached)
		{
			VideoPlaybackState videoPlaybackState = GlobalGameStateManager.CreateState<VideoPlaybackState>();
			videoPlaybackState.SetStartingParameters(text, text2, string.Empty);
			videoPlaybackState.SetOnVideoFinisedDelegate(delegate
			{
				OnInitialModuleScreenActivated(isFromSplashScreenVideo: true);
			});
			GlobalGameStateManager.CleanAndPushState(videoPlaybackState);
			_splashScreenPlayed = true;
		}
		else
		{
			OnInitialModuleScreenActivated(isFromSplashScreenVideo: false);
		}
	}

	private void OnInitialModuleScreenActivated(bool isFromSplashScreenVideo)
	{
		Utilities.EnableGlobalLoadingWindow();
		LoadingWindow.EnableGlobalLoadingWindow();
		if (!StartupInfo.IsContinueGame)
		{
			StartupInfo.IsContinueGame = PlatformServices.IsPlatformRequestedContinueGame && !IsOnlyCoreContentEnabled;
		}
		if (_enableCoreContentOnReturnToRoot)
		{
			Utilities.DisableCoreGame();
			_enableCoreContentOnReturnToRoot = false;
		}
		if (IsOnlyCoreContentEnabled && PlatformServices.SessionInvitationType == SessionInvitationType.Multiplayer)
		{
			PlatformServices.OnSessionInvitationHandled();
		}
		if (IsOnlyCoreContentEnabled && PlatformServices.IsPlatformRequestedMultiplayer)
		{
			PlatformServices.OnPlatformMultiplayerRequestHandled();
		}
		if (IsOnlyCoreContentEnabled || !MultiplayerRequested)
		{
			GlobalGameStateManager.CleanAndPushState(GlobalGameStateManager.CreateState<InitialState>());
		}
		foreach (MBSubModuleBase subModule in SubModules)
		{
			subModule.OnInitialState();
		}
	}

	private void OnSignInStateUpdated(bool isLoggedIn, TextObject message)
	{
		if (!isLoggedIn && !(GlobalGameStateManager.ActiveState is ProfileSelectionState))
		{
			GlobalGameStateManager.CleanAndPushState(GlobalGameStateManager.CreateState<ProfileSelectionState>());
		}
	}

	[MBCallback]
	internal bool SetEditorScreenAsRootScreen()
	{
		if (GameStateManager.Current != GlobalGameStateManager)
		{
			GameStateManager.Current = GlobalGameStateManager;
		}
		if (!(GlobalGameStateManager.ActiveState is EditorState))
		{
			GlobalGameStateManager.CleanAndPushState(GameStateManager.Current.CreateState<EditorState>());
			return true;
		}
		return false;
	}

	private bool CheckAssemblyForMissionMethods(Assembly assembly)
	{
		Assembly assembly2 = Assembly.GetAssembly(typeof(MissionMethod));
		if (assembly == assembly2)
		{
			return true;
		}
		AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
		for (int i = 0; i < referencedAssemblies.Length; i++)
		{
			if (referencedAssemblies[i].FullName == assembly2.FullName)
			{
				return true;
			}
		}
		return false;
	}

	private void FindMissions()
	{
		MBDebug.Print("Searching Mission Methods");
		_missionInfos = new List<MissionInfo>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		List<Type> list = new List<Type>();
		Assembly[] array = assemblies;
		foreach (Assembly assembly in array)
		{
			if (!CheckAssemblyForMissionMethods(assembly))
			{
				continue;
			}
			foreach (Type item in assembly.GetTypesSafe())
			{
				object[] customAttributesSafe = item.GetCustomAttributesSafe(typeof(MissionManager), inherit: true);
				if (customAttributesSafe != null && customAttributesSafe.Length != 0)
				{
					list.Add(item);
				}
			}
		}
		MBDebug.Print("Found " + list.Count + " mission managers");
		foreach (Type item2 in list)
		{
			MethodInfo[] methods = item2.GetMethods(BindingFlags.Static | BindingFlags.Public);
			foreach (MethodInfo methodInfo in methods)
			{
				object[] customAttributesSafe2 = methodInfo.GetCustomAttributesSafe(typeof(MissionMethod), inherit: true);
				if (customAttributesSafe2 != null && customAttributesSafe2.Length != 0)
				{
					MissionMethod missionMethod = customAttributesSafe2[0] as MissionMethod;
					MissionInfo missionInfo = new MissionInfo();
					missionInfo.Creator = methodInfo;
					missionInfo.Manager = item2;
					missionInfo.UsableByEditor = missionMethod.UsableByEditor;
					missionInfo.Name = methodInfo.Name;
					if (missionInfo.Name.StartsWith("Open"))
					{
						missionInfo.Name = missionInfo.Name.Substring(4);
					}
					if (missionInfo.Name.EndsWith("Mission"))
					{
						missionInfo.Name = missionInfo.Name.Substring(0, missionInfo.Name.Length - 7);
					}
					missionInfo.Name = missionInfo.Name + "[" + item2.Name + "]";
					_missionInfos.Add(missionInfo);
				}
			}
		}
		MBDebug.Print("Found " + _missionInfos.Count + " missions");
	}

	[MBCallback]
	internal string GetMissionControllerClassNames()
	{
		string text = "";
		for (int i = 0; i < _missionInfos.Count; i++)
		{
			MissionInfo missionInfo = _missionInfos[i];
			if (missionInfo.UsableByEditor)
			{
				text += missionInfo.Name;
				if (i + 1 != _missionInfos.Count)
				{
					text += " ";
				}
			}
		}
		return text;
	}

	private void LoadPlatformServices()
	{
		IPlatformServices platformServices = null;
		Assembly assembly = null;
		PlatformInitParams platformInitParams = new PlatformInitParams();
		if (ApplicationPlatform.CurrentPlatform == Platform.WindowsSteam)
		{
			assembly = AssemblyLoader.LoadFrom(ManagedDllFolder.Name + "TaleWorlds.PlatformService.Steam.dll");
		}
		else if (ApplicationPlatform.CurrentPlatform == Platform.WindowsEpic)
		{
			assembly = AssemblyLoader.LoadFrom(ManagedDllFolder.Name + "TaleWorlds.PlatformService.Epic.dll");
			platformInitParams.Add("ExchangeCode", StartupInfo.EpicExchangeCode);
		}
		else if (ApplicationPlatform.CurrentPlatform == Platform.WindowsGOG)
		{
			assembly = AssemblyLoader.LoadFrom(ManagedDllFolder.Name + "TaleWorlds.PlatformService.GOG.dll");
			platformInitParams.Add("AchievementDataXmlPath", ModuleHelper.GetModuleFullPath("Native") + "ModuleData/AchievementData/gog_achievement_data.xml");
		}
		else if (ApplicationPlatform.CurrentPlatform == Platform.GDKDesktop || ApplicationPlatform.CurrentPlatform == Platform.Durango)
		{
			assembly = AssemblyLoader.LoadFrom(ManagedDllFolder.Name + "TaleWorlds.PlatformService.GDK.dll");
		}
		else if (ApplicationPlatform.CurrentPlatform == Platform.Orbis)
		{
			assembly = AssemblyLoader.LoadFrom(ManagedDllFolder.Name + "TaleWorlds.PlatformService.PS.dll");
			platformInitParams.Add("AchievementDataXmlPath", ModuleHelper.GetModuleFullPath("Native") + "ModuleData/AchievementData/ps_achievement_data.xml");
		}
		else if (ApplicationPlatform.CurrentPlatform == Platform.WindowsNoPlatform)
		{
			string userName = "TestUser" + DateTime.Now.Ticks % 10000;
			if (!string.IsNullOrEmpty(StartupInfo.OverridenUserName))
			{
				userName = StartupInfo.OverridenUserName;
			}
			platformServices = new TestPlatformServices(userName);
		}
		if (assembly != null)
		{
			List<Type> typesSafe = assembly.GetTypesSafe();
			Type type = null;
			foreach (Type item in typesSafe)
			{
				if (item.GetInterfaces().Contains(typeof(IPlatformServices)))
				{
					type = item;
					break;
				}
			}
			platformServices = (IPlatformServices)type.GetConstructor(new Type[1] { typeof(PlatformInitParams) }).Invoke(new object[1] { platformInitParams });
		}
		if (platformServices != null)
		{
			PlatformServices.Setup(platformServices);
			PlatformServices.OnSessionInvitationAccepted = (Action<SessionInvitationType>)Delegate.Combine(PlatformServices.OnSessionInvitationAccepted, new Action<SessionInvitationType>(OnSessionInvitationAccepted));
			PlatformServices.OnPlatformRequestedMultiplayer = (Action)Delegate.Combine(PlatformServices.OnPlatformRequestedMultiplayer, new Action(OnPlatformRequestedMultiplayer));
			BannerlordFriendListService bannerlordFriendListService = new BannerlordFriendListService();
			ClanFriendListService clanFriendListService = new ClanFriendListService();
			RecentPlayersFriendListService recentPlayersFriendListService = new RecentPlayersFriendListService();
			PlatformServices.Initialize(new IFriendListService[3] { bannerlordFriendListService, clanFriendListService, recentPlayersFriendListService });
			AchievementManager.AchievementService = platformServices.GetAchievementService();
			ActivityManager.ActivityService = platformServices.GetActivityService();
		}
	}

	private void OnSessionInvitationAccepted(SessionInvitationType targetGameType)
	{
		if (targetGameType == SessionInvitationType.Multiplayer)
		{
			if (IsOnlyCoreContentEnabled)
			{
				PlatformServices.OnSessionInvitationHandled();
			}
			else
			{
				JobManager.AddJob(new OnSessionInvitationAcceptedJob(targetGameType));
			}
		}
	}

	private void OnPlatformRequestedMultiplayer()
	{
		if (IsOnlyCoreContentEnabled)
		{
			PlatformServices.OnPlatformMultiplayerRequestHandled();
		}
		else
		{
			JobManager.AddJob(new OnPlatformRequestedMultiplayerJob());
		}
	}

	private void LoadSubModules()
	{
		MBDebug.Print("Loading submodules...");
		List<ModuleInfo> list = new List<ModuleInfo>();
		string[] modulesNames = Utilities.GetModulesNames();
		for (int i = 0; i < modulesNames.Length; i++)
		{
			ModuleInfo moduleInfo = ModuleHelper.GetModuleInfo(modulesNames[i]);
			if (moduleInfo != null)
			{
				list.Add(moduleInfo);
				XmlResource.GetMbprojxmls(modulesNames[i]);
				XmlResource.GetXmlListAndApply(modulesNames[i]);
			}
		}
		string configName = Common.ConfigName;
		foreach (ModuleInfo item in list)
		{
			foreach (SubModuleInfo subModule in item.SubModules)
			{
				if (!CheckIfSubmoduleCanBeLoadable(subModule) || _loadedSubmoduleTypes.ContainsKey(subModule.SubModuleClassType))
				{
					continue;
				}
				string path = System.IO.Path.Combine(item.FolderPath, "bin", configName);
				string text = System.IO.Path.Combine(path, subModule.DLLName);
				string text2 = ManagedDllFolder.Name + subModule.DLLName;
				foreach (string assembly in subModule.Assemblies)
				{
					string text3 = System.IO.Path.Combine(path, assembly);
					string assemblyFile = ManagedDllFolder.Name + assembly;
					if (File.Exists(text3))
					{
						AssemblyLoader.LoadFrom(text3);
					}
					else
					{
						AssemblyLoader.LoadFrom(assemblyFile);
					}
				}
				if (File.Exists(text))
				{
					Assembly subModuleAssembly = AssemblyLoader.LoadFrom(text);
					AddSubModule(subModuleAssembly, subModule.SubModuleClassType);
				}
				else if (File.Exists(text2))
				{
					Assembly subModuleAssembly2 = AssemblyLoader.LoadFrom(text2);
					AddSubModule(subModuleAssembly2, subModule.SubModuleClassType);
				}
				else
				{
					string lpText = "Cannot find: " + text;
					string lpCaption = "Error";
					TaleWorlds.Library.Debug.ShowMessageBox(lpText, lpCaption, 4u);
				}
			}
		}
		InitializeSubModules();
	}

	public bool CheckIfSubmoduleCanBeLoadable(SubModuleInfo subModuleInfo)
	{
		if (subModuleInfo.Tags.Count > 0)
		{
			foreach (Tuple<SubModuleInfo.SubModuleTags, string> tag in subModuleInfo.Tags)
			{
				if (!GetSubModuleValiditiy(tag.Item1, tag.Item2))
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool GetSubModuleValiditiy(SubModuleInfo.SubModuleTags tag, string value)
	{
		Platform result;
		switch (tag)
		{
		case SubModuleInfo.SubModuleTags.ExclusivePlatform:
			if (Enum.TryParse<Platform>(value, out result))
			{
				return ApplicationPlatform.CurrentPlatform == result;
			}
			break;
		case SubModuleInfo.SubModuleTags.RejectedPlatform:
			if (Enum.TryParse<Platform>(value, out result))
			{
				return ApplicationPlatform.CurrentPlatform != result;
			}
			break;
		case SubModuleInfo.SubModuleTags.DedicatedServerType:
			switch (value.ToLower())
			{
			case "none":
				if (StartupInfo.DedicatedServerType == DedicatedServerType.None)
				{
					return true;
				}
				return false;
			case "both":
			case "all":
				if (StartupInfo.DedicatedServerType != 0)
				{
					return true;
				}
				return false;
			case "custom":
				if (StartupInfo.DedicatedServerType == DedicatedServerType.Custom)
				{
					return true;
				}
				return false;
			case "matchmaker":
				if (StartupInfo.DedicatedServerType == DedicatedServerType.Matchmaker)
				{
					return true;
				}
				return false;
			case "community":
				if (StartupInfo.DedicatedServerType == DedicatedServerType.Community)
				{
					return true;
				}
				return false;
			}
			break;
		case SubModuleInfo.SubModuleTags.IsNoRenderModeElement:
			return value.Equals("false");
		case SubModuleInfo.SubModuleTags.DependantRuntimeLibrary:
		{
			if (Enum.TryParse<Runtime>(value, out var result2))
			{
				return ApplicationPlatform.CurrentRuntimeLibrary == result2;
			}
			break;
		}
		case SubModuleInfo.SubModuleTags.PlayerHostedDedicatedServer:
		{
			string text = value.ToLower();
			if (StartupInfo.PlayerHostedDedicatedServer)
			{
				return text.Equals("true");
			}
			return text.Equals("false");
		}
		}
		return true;
	}

	[MBCallback]
	internal static void MBThrowException()
	{
		TaleWorlds.Library.Debug.FailedAssert("MBThrowException", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Module.cs", "MBThrowException", 1424);
	}

	[MBCallback]
	internal void OnEnterEditMode(bool isFirstTime)
	{
	}

	[MBCallback]
	internal static Module GetInstance()
	{
		return CurrentModule;
	}

	[MBCallback]
	internal static string GetGameStatus()
	{
		if (TestCommonBase.BaseInstance != null)
		{
			return TestCommonBase.BaseInstance.GetGameStatus();
		}
		return "";
	}

	private void FinalizeModule()
	{
		if (Game.Current != null)
		{
			Game.Current.OnFinalize();
		}
		if (TestCommonBase.BaseInstance != null)
		{
			TestCommonBase.BaseInstance.OnFinalize();
		}
		_testContext.FinalizeContext();
		MBInformationManager.Clear();
		InformationManager.Clear();
		ScreenManager.OnFinalize();
		BannerlordConfig.Save();
		FinalizeSubModules();
		PlatformServices.Instance?.Terminate();
		Common.MemoryCleanupGC();
		GC.WaitForPendingFinalizers();
	}

	internal static void FinalizeCurrentModule()
	{
		CurrentModule.FinalizeModule();
		CurrentModule = null;
	}

	[MBCallback]
	internal void SetLoadingFinished()
	{
		LoadingFinished = true;
	}

	[MBCallback]
	internal void OnCloseSceneEditorPresentation()
	{
		GameStateManager.Current.PopState();
	}

	[MBCallback]
	internal void OnSceneEditorModeOver()
	{
		GameStateManager.Current.PopState();
	}

	private void OnConfigChanged()
	{
		foreach (MBSubModuleBase subModule in SubModules)
		{
			subModule.OnConfigChanged();
		}
	}

	private void OnConstrainedStateChange(bool isConstrained)
	{
		if (!isConstrained)
		{
			PlatformServices.Instance.OnFocusGained();
		}
	}

	private void OnFocusGained()
	{
		PlatformServices.Instance.OnFocusGained();
	}

	private void OnTextEnteredFromPlatform(string text)
	{
		ScreenManager.OnOnscreenKeyboardDone(text);
	}

	[MBCallback]
	internal void OnSkinsXMLHasChanged()
	{
		if (this.SkinsXMLHasChanged != null)
		{
			this.SkinsXMLHasChanged();
		}
	}

	[MBCallback]
	internal void OnImguiProfilerTick()
	{
		if (this.ImguiProfilerTick != null)
		{
			this.ImguiProfilerTick();
		}
	}

	[MBCallback]
	internal static string CreateProcessedSkinsXMLForNative(out string baseSkinsXmlPath)
	{
		List<string> usedPaths;
		XDocument xDocument = MBObjectManager.ToXDocument(MBObjectManager.GetMergedXmlForNative("soln_skins", out usedPaths));
		for (int i = 0; i < xDocument.Descendants("race").Count(); i++)
		{
			for (int j = i + 1; j < xDocument.Descendants("race").Count(); j++)
			{
				if (xDocument.Descendants("race").ElementAt(i).FirstAttribute.ToString() == xDocument.Descendants("race").ElementAt(j).FirstAttribute.ToString())
				{
					xDocument.Descendants("race").ElementAt(i).Add(xDocument.Descendants("race").ElementAt(j).Descendants());
					xDocument.Descendants("race").ElementAt(j).Remove();
					j--;
				}
			}
		}
		XmlDocument xmlDocument = MBObjectManager.ToXmlDocument(xDocument);
		System.IO.StringWriter stringWriter = new System.IO.StringWriter();
		XmlTextWriter w = new XmlTextWriter(stringWriter);
		xmlDocument.WriteTo(w);
		baseSkinsXmlPath = usedPaths[0];
		return stringWriter.ToString();
	}

	[MBCallback]
	internal static string CreateProcessedActionSetsXMLForNative()
	{
		XmlDocument mergedXmlForNative = MBObjectManager.GetMergedXmlForNative("soln_action_sets", out var _);
		Dictionary<string, XElement> dictionary = new Dictionary<string, XElement>();
		XDocument xDocument = MBObjectManager.ToXDocument(mergedXmlForNative);
		IEnumerable<XElement> source = xDocument.Descendants("action_set");
		for (int i = 0; i < source.Count(); i++)
		{
			XElement xElement = source.ElementAt(i);
			string key = xElement.FirstAttribute.ToString();
			if (dictionary.ContainsKey(key))
			{
				dictionary[key].Add(xElement.Descendants());
				xElement.Remove();
				i--;
			}
			else
			{
				dictionary.Add(key, xElement);
			}
		}
		mergedXmlForNative = MBObjectManager.ToXmlDocument(xDocument);
		System.IO.StringWriter stringWriter = new System.IO.StringWriter();
		XmlTextWriter w = new XmlTextWriter(stringWriter);
		mergedXmlForNative.WriteTo(w);
		return stringWriter.ToString();
	}

	[MBCallback]
	internal static string CreateProcessedActionTypesXMLForNative()
	{
		List<string> usedPaths;
		XmlDocument mergedXmlForNative = MBObjectManager.GetMergedXmlForNative("soln_action_types", out usedPaths);
		System.IO.StringWriter stringWriter = new System.IO.StringWriter();
		XmlTextWriter w = new XmlTextWriter(stringWriter);
		mergedXmlForNative.WriteTo(w);
		return stringWriter.ToString();
	}

	[MBCallback]
	internal static string CreateProcessedAnimationsXMLForNative(out string animationsXmlPaths)
	{
		List<string> usedPaths;
		XmlDocument mergedXmlForNative = MBObjectManager.GetMergedXmlForNative("soln_animations", out usedPaths);
		System.IO.StringWriter stringWriter = new System.IO.StringWriter();
		XmlTextWriter w = new XmlTextWriter(stringWriter);
		mergedXmlForNative.WriteTo(w);
		animationsXmlPaths = "";
		for (int i = 0; i < usedPaths.Count; i++)
		{
			animationsXmlPaths += usedPaths[i];
			if (i != usedPaths.Count - 1)
			{
				animationsXmlPaths += "\n";
			}
		}
		return stringWriter.ToString();
	}

	[MBCallback]
	internal static string CreateProcessedVoiceDefinitionsXMLForNative()
	{
		XmlDocument mergedXmlForNative = MBObjectManager.GetMergedXmlForNative("soln_voice_definitions", out var _);
		XDocument xDocument = MBObjectManager.ToXDocument(mergedXmlForNative);
		XElement xElement = xDocument.Descendants("voice_type_declarations").First();
		int num;
		for (num = 1; num < xDocument.Descendants("voice_type_declarations").Count(); num++)
		{
			xElement.Add(xDocument.Descendants("voice_type_declarations").ElementAt(num).Descendants());
			xDocument.Descendants("voice_type_declarations").ElementAt(num).Remove();
			num--;
		}
		for (int i = 0; i < xDocument.Descendants("voice_definition").Count(); i++)
		{
			for (int j = i + 1; j < xDocument.Descendants("voice_definition").Count(); j++)
			{
				if (xDocument.Descendants("voice_definition").ElementAt(i).FirstAttribute.ToString() == xDocument.Descendants("voice_definition").ElementAt(j).FirstAttribute.ToString())
				{
					xDocument.Descendants("voice_definition").ElementAt(i).Add(xDocument.Descendants("voice_definition").ElementAt(j).Descendants());
					xDocument.Descendants("voice_definition").ElementAt(j).Remove();
					j--;
				}
			}
		}
		mergedXmlForNative = MBObjectManager.ToXmlDocument(xDocument);
		System.IO.StringWriter stringWriter = new System.IO.StringWriter();
		XmlTextWriter w = new XmlTextWriter(stringWriter);
		mergedXmlForNative.WriteTo(w);
		return stringWriter.ToString();
	}

	[MBCallback]
	internal static string CreateProcessedModuleDataXMLForNative(string xmlType)
	{
		List<string> usedPaths;
		XmlDocument xmlDocument = MBObjectManager.GetMergedXmlForNative("soln_" + xmlType, out usedPaths);
		if (xmlType == "full_movement_sets")
		{
			XDocument xDocument = MBObjectManager.ToXDocument(xmlDocument);
			for (int i = 0; i < xDocument.Descendants("full_movement_set").Count(); i++)
			{
				for (int j = i + 1; j < xDocument.Descendants("full_movement_set").Count(); j++)
				{
					if (xDocument.Descendants("full_movement_set").ElementAt(i).FirstAttribute.ToString() == xDocument.Descendants("full_movement_set").ElementAt(j).FirstAttribute.ToString())
					{
						xDocument.Descendants("full_movement_set").ElementAt(i).Add(xDocument.Descendants("full_movement_set").ElementAt(j).Descendants());
						xDocument.Descendants("full_movement_set").ElementAt(j).Remove();
						j--;
					}
				}
			}
			xmlDocument = MBObjectManager.ToXmlDocument(xDocument);
		}
		System.IO.StringWriter stringWriter = new System.IO.StringWriter();
		XmlTextWriter w = new XmlTextWriter(stringWriter);
		xmlDocument.WriteTo(w);
		return stringWriter.ToString();
	}

	public void ClearStateOptions()
	{
		_initialStateOptions.Clear();
	}

	public void AddInitialStateOption(InitialStateOption initialStateOption)
	{
		_initialStateOptions.Add(initialStateOption);
	}

	public IEnumerable<InitialStateOption> GetInitialStateOptions()
	{
		return _initialStateOptions.OrderBy((InitialStateOption s) => s.OrderIndex);
	}

	public InitialStateOption GetInitialStateOptionWithId(string id)
	{
		foreach (InitialStateOption initialStateOption in _initialStateOptions)
		{
			if (initialStateOption.Id == id)
			{
				return initialStateOption;
			}
		}
		return null;
	}

	public void ExecuteInitialStateOptionWithId(string id)
	{
		GetInitialStateOptionWithId(id)?.DoAction();
	}

	void IGameStateManagerOwner.OnStateStackEmpty()
	{
	}

	void IGameStateManagerOwner.OnStateChanged(GameState oldState)
	{
	}

	public void SetEditorMissionTester(IEditorMissionTester editorMissionTester)
	{
		_editorMissionTester = editorMissionTester;
	}

	[MBCallback]
	internal void StartMissionForEditor(string missionName, string sceneName, string levels)
	{
		if (_editorMissionTester != null)
		{
			_editorMissionTester.StartMissionForEditor(missionName, sceneName, levels);
		}
	}

	[MBCallback]
	internal void StartMissionForReplayEditor(string missionName, string sceneName, string levels, string fileName, bool record, float startTime, float endTime)
	{
		if (_editorMissionTester != null)
		{
			_editorMissionTester.StartMissionForReplayEditor(missionName, sceneName, levels, fileName, record, startTime, endTime);
		}
	}

	public void StartMissionForEditorAux(string missionName, string sceneName, string levels, bool forReplay, string replayFileName, bool isRecord)
	{
		GameStateManager.Current = Game.Current.GameStateManager;
		ReturnToEditorState = true;
		MissionInfo missionInfo = _missionInfos.Find((MissionInfo mi) => mi.Name == missionName);
		if (missionInfo == null)
		{
			missionInfo = _missionInfos.Find((MissionInfo mi) => mi.Name.Contains(missionName));
		}
		if (forReplay)
		{
			missionInfo.Creator.Invoke(null, new object[2] { replayFileName, isRecord });
		}
		else
		{
			missionInfo.Creator.Invoke(null, new object[2] { sceneName, levels });
		}
	}

	private void FillMultiplayerGameTypes()
	{
		_multiplayerGameModesWithNames = new Dictionary<string, MultiplayerGameMode>();
		_multiplayerGameTypes = new MBList<MultiplayerGameTypeInfo>();
	}

	public MultiplayerGameMode GetMultiplayerGameMode(string gameType)
	{
		if (_multiplayerGameModesWithNames.TryGetValue(gameType, out var value))
		{
			return value;
		}
		return null;
	}

	public void AddMultiplayerGameMode(MultiplayerGameMode multiplayerGameMode)
	{
		_multiplayerGameModesWithNames.Add(multiplayerGameMode.Name, multiplayerGameMode);
		_multiplayerGameTypes.Add(new MultiplayerGameTypeInfo("Native", multiplayerGameMode.Name));
	}

	public MBReadOnlyList<MultiplayerGameTypeInfo> GetMultiplayerGameTypes()
	{
		return _multiplayerGameTypes;
	}

	public bool StartMultiplayerGame(string multiplayerGameType, string scene)
	{
		if (_multiplayerGameModesWithNames.TryGetValue(multiplayerGameType, out var value))
		{
			value.StartMultiplayerGame(scene);
			return true;
		}
		return false;
	}

	public async void ShutDownWithDelay(string reason, int seconds)
	{
		if (!_isShuttingDown)
		{
			_isShuttingDown = true;
			for (int i = 0; i < seconds; i++)
			{
				int num = seconds - i;
				string text = "Shutting down in " + num + " seconds with reason '" + reason + "'";
				TaleWorlds.Library.Debug.Print(text);
				Console.WriteLine(text);
				await Task.Delay(1000);
			}
			if (Game.Current != null)
			{
				TaleWorlds.Library.Debug.Print("Active game exist during ShutDownWithDelay");
				MBGameManager.EndGame();
			}
			Utilities.QuitGame();
		}
	}
}
