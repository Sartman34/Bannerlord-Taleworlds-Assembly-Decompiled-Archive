using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace TaleWorlds.CampaignSystem;

public class Campaign : GameType
{
	[Flags]
	public enum PartyRestFlags : uint
	{
		None = 0u,
		SafeMode = 1u
	}

	public enum GameLoadingType
	{
		Tutorial,
		NewCampaign,
		SavedCampaign,
		Editor
	}

	public const float ConfigTimeMultiplier = 0.25f;

	private EntitySystem<CampaignEntityComponent> _campaignEntitySystem;

	public ITask CampaignLateAITickTask;

	[SaveableField(210)]
	private CampaignPeriodicEventManager _campaignPeriodicEventManager;

	[SaveableField(53)]
	private bool _isMainPartyWaiting;

	[SaveableField(344)]
	private string _newGameVersion;

	[SaveableField(78)]
	private MBList<string> _previouslyUsedModules;

	[SaveableField(81)]
	private MBList<string> _usedGameVersions;

	[SaveableField(77)]
	private Dictionary<CharacterObject, FormationClass> _playerFormationPreferences;

	[SaveableField(7)]
	private ICampaignBehaviorManager _campaignBehaviorManager;

	private CampaignTickCacheDataStore _tickData;

	[SaveableField(2)]
	public readonly CampaignOptions Options;

	public MBReadOnlyDictionary<CharacterObject, FormationClass> PlayerFormationPreferences;

	[SaveableField(13)]
	public ITournamentManager TournamentManager;

	public float MinSettlementX;

	public float MaxSettlementX;

	public float MinSettlementY;

	public float MaxSettlementY;

	[SaveableField(27)]
	public bool IsInitializedSinglePlayerReferences;

	private LocatorGrid<MobileParty> _mobilePartyLocator;

	private LocatorGrid<Settlement> _settlementLocator;

	private GameModels _gameModels;

	[SaveableField(31)]
	public CampaignTimeControlMode LastTimeControlMode = CampaignTimeControlMode.UnstoppablePlay;

	private IMapScene _mapSceneWrapper;

	public bool GameStarted;

	[SaveableField(44)]
	public PartyBase autoEnterTown;

	private GameLoadingType _gameLoadingType;

	public ConversationContext CurrentConversationContext;

	[CachedData]
	private float _dt;

	private CampaignTimeControlMode _timeControlMode;

	private int _curSessionFrame;

	[SaveableField(30)]
	public int MainHeroIllDays = -1;

	private MBCampaignEvent _dailyTickEvent;

	private MBCampaignEvent _hourlyTickEvent;

	[CachedData]
	private int _lastNonZeroDtFrame;

	[SaveableField(84)]
	public List<Settlement> BusyHideouts = new List<Settlement>();

	private MBList<Town> _towns;

	private MBList<Town> _castles;

	private MBList<Village> _villages;

	private MBList<Hideout> _hideouts;

	private MBReadOnlyList<CharacterObject> _characters;

	private MBReadOnlyList<WorkshopType> _workshops;

	private MBReadOnlyList<ItemModifier> _itemModifiers;

	private MBReadOnlyList<Concept> _concepts;

	private MBReadOnlyList<ItemModifierGroup> _itemModifierGroups;

	[SaveableField(79)]
	private int _lastPartyIndex;

	[SaveableField(61)]
	private PartyBase _cameraFollowParty;

	[SaveableField(64)]
	private readonly LogEntryHistory _logEntryHistory = new LogEntryHistory();

	[SaveableField(65)]
	public KingdomManager KingdomManager;

	public static float MapDiagonal { get; private set; }

	public static float MapDiagonalSquared { get; private set; }

	public static float MaximumDistanceBetweenTwoSettlements { get; private set; }

	public static Vec2 MapMinimumPosition { get; private set; }

	public static Vec2 MapMaximumPosition { get; private set; }

	public static float MapMaximumHeight { get; private set; }

	public static float AverageDistanceBetweenTwoFortifications { get; private set; }

	[CachedData]
	public float AverageWage { get; private set; }

	public string NewGameVersion => _newGameVersion;

	public MBReadOnlyList<string> PreviouslyUsedModules => _previouslyUsedModules;

	public MBReadOnlyList<string> UsedGameVersions => _usedGameVersions;

	[SaveableProperty(83)]
	public bool EnabledCheatsBefore { get; set; }

	[SaveableProperty(82)]
	public string PlatformID { get; private set; }

	internal CampaignEventDispatcher CampaignEventDispatcher { get; private set; }

	[SaveableProperty(80)]
	public string UniqueGameId { get; private set; }

	public SaveHandler SaveHandler { get; private set; }

	public override bool SupportsSaving => GameMode == CampaignGameMode.Campaign;

	[SaveableProperty(211)]
	public CampaignObjectManager CampaignObjectManager { get; private set; }

	public override bool IsDevelopment => GameMode == CampaignGameMode.Tutorial;

	[SaveableProperty(3)]
	public bool IsCraftingEnabled { get; set; } = true;


	[SaveableProperty(4)]
	public bool IsBannerEditorEnabled { get; set; } = true;


	[SaveableProperty(5)]
	public bool IsFaceGenEnabled { get; set; } = true;


	public ICampaignBehaviorManager CampaignBehaviorManager => _campaignBehaviorManager;

	[SaveableProperty(8)]
	public QuestManager QuestManager { get; private set; }

	[SaveableProperty(9)]
	public IssueManager IssueManager { get; private set; }

	[SaveableProperty(11)]
	public FactionManager FactionManager { get; private set; }

	[SaveableProperty(12)]
	public CharacterRelationManager CharacterRelationManager { get; private set; }

	[SaveableProperty(14)]
	public Romance Romance { get; private set; }

	[SaveableProperty(16)]
	public PlayerCaptivity PlayerCaptivity { get; private set; }

	[SaveableProperty(17)]
	internal Clan PlayerDefaultFaction { get; set; }

	public CampaignMission.ICampaignMissionManager CampaignMissionManager { get; set; }

	public ISkillLevelingManager SkillLevelingManager { get; set; }

	public IMapSceneCreator MapSceneCreator { get; set; }

	public override bool IsInventoryAccessibleAtMission => GameMode == CampaignGameMode.Tutorial;

	public GameMenuCallbackManager GameMenuCallbackManager { get; private set; }

	public VisualCreator VisualCreator { get; set; }

	[SaveableProperty(28)]
	public MapStateData MapStateData { get; private set; }

	public DefaultPerks DefaultPerks { get; private set; }

	public DefaultTraits DefaultTraits { get; private set; }

	public DefaultPolicies DefaultPolicies { get; private set; }

	public DefaultBuildingTypes DefaultBuildingTypes { get; private set; }

	public DefaultIssueEffects DefaultIssueEffects { get; private set; }

	public DefaultItems DefaultItems { get; private set; }

	public DefaultSiegeStrategies DefaultSiegeStrategies { get; private set; }

	internal MBReadOnlyList<PerkObject> AllPerks { get; private set; }

	public DefaultSkillEffects DefaultSkillEffects { get; private set; }

	public DefaultVillageTypes DefaultVillageTypes { get; private set; }

	internal MBReadOnlyList<TraitObject> AllTraits { get; private set; }

	internal MBReadOnlyList<MBEquipmentRoster> AllEquipmentRosters { get; private set; }

	public DefaultCulturalFeats DefaultFeats { get; private set; }

	internal MBReadOnlyList<PolicyObject> AllPolicies { get; private set; }

	internal MBReadOnlyList<BuildingType> AllBuildingTypes { get; private set; }

	internal MBReadOnlyList<IssueEffect> AllIssueEffects { get; private set; }

	internal MBReadOnlyList<SiegeStrategy> AllSiegeStrategies { get; private set; }

	internal MBReadOnlyList<VillageType> AllVillageTypes { get; private set; }

	internal MBReadOnlyList<SkillEffect> AllSkillEffects { get; private set; }

	internal MBReadOnlyList<FeatObject> AllFeats { get; private set; }

	internal MBReadOnlyList<SkillObject> AllSkills { get; private set; }

	internal MBReadOnlyList<SiegeEngineType> AllSiegeEngineTypes { get; private set; }

	internal MBReadOnlyList<ItemCategory> AllItemCategories { get; private set; }

	internal MBReadOnlyList<CharacterAttribute> AllCharacterAttributes { get; private set; }

	internal MBReadOnlyList<ItemObject> AllItems { get; private set; }

	[SaveableProperty(100)]
	internal MapTimeTracker MapTimeTracker { get; private set; }

	public bool TimeControlModeLock { get; private set; }

	public CampaignTimeControlMode TimeControlMode
	{
		get
		{
			return _timeControlMode;
		}
		set
		{
			if (!TimeControlModeLock && value != _timeControlMode)
			{
				_timeControlMode = value;
			}
		}
	}

	public bool IsMapTooltipLongForm { get; set; }

	public float SpeedUpMultiplier { get; set; } = 4f;


	public float CampaignDt => _dt;

	public bool TrueSight { get; set; }

	public static Campaign Current { get; private set; }

	[SaveableProperty(36)]
	public CampaignTime CampaignStartTime { get; private set; }

	[SaveableProperty(37)]
	public CampaignGameMode GameMode { get; private set; }

	[SaveableProperty(38)]
	public float PlayerProgress { get; private set; }

	public GameMenuManager GameMenuManager { get; private set; }

	public GameModels Models => _gameModels;

	public SandBoxManager SandBoxManager { get; private set; }

	public GameLoadingType CampaignGameLoadingType => _gameLoadingType;

	[SaveableProperty(40)]
	public SiegeEventManager SiegeEventManager { get; internal set; }

	[SaveableProperty(41)]
	public MapEventManager MapEventManager { get; internal set; }

	internal CampaignEvents CampaignEvents { get; private set; }

	public MenuContext CurrentMenuContext
	{
		get
		{
			GameStateManager gameStateManager = base.CurrentGame.GameStateManager;
			if (gameStateManager.ActiveState is TutorialState tutorialState)
			{
				return tutorialState.MenuContext;
			}
			if (gameStateManager.ActiveState is MapState mapState)
			{
				return mapState.MenuContext;
			}
			if (gameStateManager.ActiveState?.Predecessor != null && gameStateManager.ActiveState.Predecessor is MapState mapState2)
			{
				return mapState2.MenuContext;
			}
			return null;
		}
	}

	internal List<MBCampaignEvent> CustomPeriodicCampaignEvents { get; private set; }

	public bool IsMainPartyWaiting
	{
		get
		{
			return _isMainPartyWaiting;
		}
		private set
		{
			_isMainPartyWaiting = value;
		}
	}

	[SaveableProperty(45)]
	private int _curMapFrame { get; set; }

	internal LocatorGrid<Settlement> SettlementLocator => _settlementLocator ?? (_settlementLocator = new LocatorGrid<Settlement>());

	internal LocatorGrid<MobileParty> MobilePartyLocator => _mobilePartyLocator ?? (_mobilePartyLocator = new LocatorGrid<MobileParty>());

	public IMapScene MapSceneWrapper => _mapSceneWrapper;

	[SaveableProperty(54)]
	public PlayerEncounter PlayerEncounter { get; internal set; }

	[CachedData]
	internal LocationEncounter LocationEncounter { get; set; }

	internal NameGenerator NameGenerator { get; private set; }

	[SaveableProperty(58)]
	public BarterManager BarterManager { get; private set; }

	[SaveableProperty(69)]
	public bool IsMainHeroDisguised { get; set; }

	[SaveableProperty(70)]
	public bool DesertionEnabled { get; set; }

	public Vec2 DefaultStartingPosition => new Vec2(685.3f, 410.9f);

	public Equipment DeadBattleEquipment { get; set; }

	public Equipment DeadCivilianEquipment { get; set; }

	public static float CurrentTime => (float)CampaignTime.Now.ToHours;

	public MBReadOnlyList<CampaignEntityComponent> CampaignEntityComponents => _campaignEntitySystem.Components;

	public MBReadOnlyList<Hero> AliveHeroes => CampaignObjectManager.AliveHeroes;

	public MBReadOnlyList<Hero> DeadOrDisabledHeroes => CampaignObjectManager.DeadOrDisabledHeroes;

	public MBReadOnlyList<MobileParty> MobileParties => CampaignObjectManager.MobileParties;

	public MBReadOnlyList<MobileParty> CaravanParties => CampaignObjectManager.CaravanParties;

	public MBReadOnlyList<MobileParty> VillagerParties => CampaignObjectManager.VillagerParties;

	public MBReadOnlyList<MobileParty> MilitiaParties => CampaignObjectManager.MilitiaParties;

	public MBReadOnlyList<MobileParty> GarrisonParties => CampaignObjectManager.GarrisonParties;

	public MBReadOnlyList<MobileParty> CustomParties => CampaignObjectManager.CustomParties;

	public MBReadOnlyList<MobileParty> LordParties => CampaignObjectManager.LordParties;

	public MBReadOnlyList<MobileParty> BanditParties => CampaignObjectManager.BanditParties;

	public MBReadOnlyList<MobileParty> PartiesWithoutPartyComponent => CampaignObjectManager.PartiesWithoutPartyComponent;

	public MBReadOnlyList<Settlement> Settlements => CampaignObjectManager.Settlements;

	public IEnumerable<IFaction> Factions => CampaignObjectManager.Factions;

	public MBReadOnlyList<Kingdom> Kingdoms => CampaignObjectManager.Kingdoms;

	public MBReadOnlyList<Clan> Clans => CampaignObjectManager.Clans;

	public MBReadOnlyList<CharacterObject> Characters => _characters;

	public MBReadOnlyList<WorkshopType> Workshops => _workshops;

	public MBReadOnlyList<ItemModifier> ItemModifiers => _itemModifiers;

	public MBReadOnlyList<ItemModifierGroup> ItemModifierGroups => _itemModifierGroups;

	public MBReadOnlyList<Concept> Concepts => _concepts;

	[SaveableProperty(60)]
	public MobileParty MainParty { get; private set; }

	public PartyBase CameraFollowParty
	{
		get
		{
			return _cameraFollowParty;
		}
		set
		{
			_cameraFollowParty = value;
		}
	}

	[SaveableProperty(62)]
	public CampaignInformationManager CampaignInformationManager { get; set; }

	[SaveableProperty(63)]
	public VisualTrackerManager VisualTrackerManager { get; set; }

	public LogEntryHistory LogEntryHistory => _logEntryHistory;

	public EncyclopediaManager EncyclopediaManager { get; private set; }

	public InventoryManager InventoryManager { get; private set; }

	public PartyScreenManager PartyScreenManager { get; private set; }

	public ConversationManager ConversationManager { get; private set; }

	public bool IsDay => !IsNight;

	public bool IsNight => CampaignTime.Now.IsNightTime;

	[SaveableProperty(68)]
	public HeroTraitDeveloper PlayerTraitDeveloper { get; private set; }

	public override bool IsPartyWindowAccessibleAtMission => GameMode == CampaignGameMode.Tutorial;

	internal MBReadOnlyList<Town> AllTowns => _towns;

	internal MBReadOnlyList<Town> AllCastles => _castles;

	internal MBReadOnlyList<Village> AllVillages => _villages;

	internal MBReadOnlyList<Hideout> AllHideouts => _hideouts;

	public Campaign(CampaignGameMode gameMode)
	{
		GameMode = gameMode;
		Options = new CampaignOptions();
		MapTimeTracker = new MapTimeTracker(CampaignData.CampaignStartTime);
		CampaignStartTime = MapTimeTracker.Now;
		CampaignObjectManager = new CampaignObjectManager();
		CurrentConversationContext = ConversationContext.Default;
		QuestManager = new QuestManager();
		IssueManager = new IssueManager();
		FactionManager = new FactionManager();
		CharacterRelationManager = new CharacterRelationManager();
		Romance = new Romance();
		PlayerCaptivity = new PlayerCaptivity();
		BarterManager = new BarterManager();
		GameMenuCallbackManager = new GameMenuCallbackManager();
		_campaignPeriodicEventManager = new CampaignPeriodicEventManager();
		_tickData = new CampaignTickCacheDataStore();
	}

	public void InitializeMainParty()
	{
		InitializeSinglePlayerReferences();
		MainParty.InitializeMobilePartyAtPosition(base.CurrentGame.ObjectManager.GetObject<PartyTemplateObject>("main_hero_party_template"), DefaultStartingPosition);
		MainParty.ActualClan = Clan.PlayerClan;
		MainParty.PartyComponent = new LordPartyComponent(Hero.MainHero, Hero.MainHero);
		MainParty.ItemRoster.AddToCounts(DefaultItems.Grain, 1);
	}

	[LoadInitializationCallback]
	private void OnLoad(MetaData metaData, ObjectLoadData objectLoadData)
	{
		_campaignEntitySystem = new EntitySystem<CampaignEntityComponent>();
		PlayerFormationPreferences = _playerFormationPreferences.GetReadOnlyDictionary();
		SpeedUpMultiplier = 4f;
		if (UniqueGameId == null && MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.2.2"))
		{
			UniqueGameId = "oldSave";
		}
		if (BusyHideouts == null)
		{
			BusyHideouts = new List<Settlement>();
		}
	}

	private void InitializeForSavedGame()
	{
		foreach (Settlement item in Settlement.All)
		{
			item.Party.OnFinishLoadState();
		}
		foreach (MobileParty item2 in MobileParties.ToList())
		{
			item2.Party.OnFinishLoadState();
		}
		foreach (Settlement item3 in Settlement.All)
		{
			item3.OnFinishLoadState();
		}
		GameMenuCallbackManager = new GameMenuCallbackManager();
		GameMenuCallbackManager.OnGameLoad();
		IssueManager.InitializeForSavedGame();
		MinSettlementX = 1000f;
		MinSettlementY = 1000f;
		foreach (Settlement item4 in Settlement.All)
		{
			if (item4.Position2D.x < MinSettlementX)
			{
				MinSettlementX = item4.Position2D.x;
			}
			if (item4.Position2D.y < MinSettlementY)
			{
				MinSettlementY = item4.Position2D.y;
			}
			if (item4.Position2D.x > MaxSettlementX)
			{
				MaxSettlementX = item4.Position2D.x;
			}
			if (item4.Position2D.y > MaxSettlementY)
			{
				MaxSettlementY = item4.Position2D.y;
			}
		}
	}

	private void OnGameLoaded(CampaignGameStarter starter)
	{
		_tickData = new CampaignTickCacheDataStore();
		base.ObjectManager.PreAfterLoad();
		CampaignObjectManager.PreAfterLoad();
		base.ObjectManager.AfterLoad();
		CampaignObjectManager.AfterLoad();
		CharacterRelationManager.AfterLoad();
		CampaignEventDispatcher.Instance.OnGameEarlyLoaded(starter);
		TroopRoster.CalculateCachedStatsOnLoad();
		CampaignEventDispatcher.Instance.OnGameLoaded(starter);
		InitializeForSavedGame();
		_tickData.InitializeDataCache();
	}

	private void OnDataLoadFinished(CampaignGameStarter starter)
	{
		_towns = (from x in Settlement.All
			where x.IsTown
			select x.Town).ToMBList();
		_castles = (from x in Settlement.All
			where x.IsCastle
			select x.Town).ToMBList();
		_villages = (from x in Settlement.All
			where x.Village != null
			select x.Village).ToMBList();
		_hideouts = (from x in Settlement.All
			where x.IsHideout
			select x.Hideout).ToMBList();
		_campaignPeriodicEventManager.InitializeTickers();
		CreateCampaignEvents();
	}

	private void OnSessionStart(CampaignGameStarter starter)
	{
		CampaignEventDispatcher.Instance.OnSessionStart(starter);
		CampaignEventDispatcher.Instance.OnAfterSessionStart(starter);
		CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailyTickSettlement);
		ConversationManager.Build();
		foreach (Settlement settlement in Settlements)
		{
			settlement.OnSessionStart();
		}
		IsCraftingEnabled = true;
		IsBannerEditorEnabled = true;
		IsFaceGenEnabled = true;
		MapEventManager.OnAfterLoad();
		KingdomManager.RegisterEvents();
		KingdomManager.OnSessionStart();
		CampaignInformationManager.RegisterEvents();
	}

	private void DailyTickSettlement(Settlement settlement)
	{
		if (settlement.IsVillage)
		{
			settlement.Village.DailyTick();
		}
		else if (settlement.Town != null)
		{
			settlement.Town.DailyTick();
		}
	}

	private void GameInitTick()
	{
		foreach (Settlement item in Settlement.All)
		{
			item.Party.UpdateVisibilityAndInspected();
		}
		foreach (MobileParty mobileParty in MobileParties)
		{
			mobileParty.Party.UpdateVisibilityAndInspected();
		}
	}

	internal void HourlyTick(MBCampaignEvent campaignEvent, object[] delegateParams)
	{
		CampaignEventDispatcher.Instance.HourlyTick();
		(Game.Current.GameStateManager.ActiveState as MapState)?.OnHourlyTick();
	}

	internal void DailyTick(MBCampaignEvent campaignEvent, object[] delegateParams)
	{
		PlayerProgress = (PlayerProgress + Current.Models.PlayerProgressionModel.GetPlayerProgress()) / 2f;
		Debug.Print("Before Daily Tick: " + CampaignTime.Now.ToString());
		CampaignEventDispatcher.Instance.DailyTick();
		if ((int)CampaignStartTime.ElapsedDaysUntilNow % 7 == 0)
		{
			CampaignEventDispatcher.Instance.WeeklyTick();
			OnWeeklyTick();
		}
	}

	public void WaitAsyncTasks()
	{
		if (CampaignLateAITickTask != null)
		{
			CampaignLateAITickTask.Wait();
		}
	}

	private void OnWeeklyTick()
	{
		LogEntryHistory.DeleteOutdatedLogs();
	}

	public CampaignTimeControlMode GetSimplifiedTimeControlMode()
	{
		switch (TimeControlMode)
		{
		case CampaignTimeControlMode.Stop:
			return CampaignTimeControlMode.Stop;
		case CampaignTimeControlMode.StoppablePlay:
			if (!IsMainPartyWaiting)
			{
				return CampaignTimeControlMode.StoppablePlay;
			}
			return CampaignTimeControlMode.Stop;
		case CampaignTimeControlMode.UnstoppablePlay:
			return CampaignTimeControlMode.UnstoppablePlay;
		case CampaignTimeControlMode.StoppableFastForward:
			if (!IsMainPartyWaiting)
			{
				return CampaignTimeControlMode.StoppableFastForward;
			}
			return CampaignTimeControlMode.Stop;
		case CampaignTimeControlMode.UnstoppableFastForward:
		case CampaignTimeControlMode.UnstoppableFastForwardForPartyWaitTime:
			return CampaignTimeControlMode.UnstoppableFastForward;
		default:
			return CampaignTimeControlMode.Stop;
		}
	}

	private void CheckMainPartyNeedsUpdate()
	{
		MobileParty.MainParty.Ai.CheckPartyNeedsUpdate();
	}

	private void TickMapTime(float realDt)
	{
		float num = 0f;
		float speedUpMultiplier = SpeedUpMultiplier;
		float num2 = 0.25f * realDt;
		IsMainPartyWaiting = MobileParty.MainParty.ComputeIsWaiting();
		switch (TimeControlMode)
		{
		case CampaignTimeControlMode.StoppablePlay:
			if (!IsMainPartyWaiting)
			{
				num = num2;
			}
			break;
		case CampaignTimeControlMode.UnstoppablePlay:
			num = num2;
			break;
		case CampaignTimeControlMode.StoppableFastForward:
			if (!IsMainPartyWaiting)
			{
				num = num2 * speedUpMultiplier;
			}
			break;
		case CampaignTimeControlMode.UnstoppableFastForward:
		case CampaignTimeControlMode.UnstoppableFastForwardForPartyWaitTime:
			num = num2 * speedUpMultiplier;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case CampaignTimeControlMode.Stop:
		case CampaignTimeControlMode.FastForwardStop:
			break;
		}
		_dt = num;
		MapTimeTracker.Tick(4320f * num);
	}

	public void OnGameOver()
	{
		if (CampaignOptions.IsIronmanMode)
		{
			SaveHandler.QuickSaveCurrentGame();
		}
	}

	internal void RealTick(float realDt)
	{
		WaitAsyncTasks();
		CheckMainPartyNeedsUpdate();
		TickMapTime(realDt);
		foreach (CampaignEntityComponent component in _campaignEntitySystem.GetComponents())
		{
			component.OnTick(realDt, _dt);
		}
		if (!GameStarted)
		{
			GameStarted = true;
			_tickData.InitializeDataCache();
			SiegeEventManager.Tick(_dt);
		}
		_tickData.RealTick(_dt, realDt);
		SiegeEventManager.Tick(_dt);
	}

	public void SetTimeSpeed(int speed)
	{
		switch (speed)
		{
		case 0:
			if (TimeControlMode == CampaignTimeControlMode.UnstoppableFastForward || TimeControlMode == CampaignTimeControlMode.StoppableFastForward)
			{
				TimeControlMode = CampaignTimeControlMode.FastForwardStop;
			}
			else if (TimeControlMode != CampaignTimeControlMode.FastForwardStop && TimeControlMode != 0)
			{
				TimeControlMode = CampaignTimeControlMode.Stop;
			}
			break;
		case 1:
			if (((TimeControlMode == CampaignTimeControlMode.Stop || TimeControlMode == CampaignTimeControlMode.FastForwardStop) && MainParty.DefaultBehavior == AiBehavior.Hold) || IsMainPartyWaiting || (MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty))
			{
				TimeControlMode = CampaignTimeControlMode.UnstoppablePlay;
			}
			else
			{
				TimeControlMode = CampaignTimeControlMode.StoppablePlay;
			}
			break;
		case 2:
			if (((TimeControlMode == CampaignTimeControlMode.Stop || TimeControlMode == CampaignTimeControlMode.FastForwardStop) && MainParty.DefaultBehavior == AiBehavior.Hold) || IsMainPartyWaiting || (MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty))
			{
				TimeControlMode = CampaignTimeControlMode.UnstoppableFastForward;
			}
			else
			{
				TimeControlMode = CampaignTimeControlMode.StoppableFastForward;
			}
			break;
		}
	}

	public static void LateAITick()
	{
		Current.LateAITickAux();
	}

	internal void LateAITickAux()
	{
		if (_dt > 0f || _curSessionFrame < 3)
		{
			PartiesThink(_dt);
		}
	}

	internal void Tick()
	{
		_curMapFrame++;
		_curSessionFrame++;
		if (_dt > 0f || _curSessionFrame < 3)
		{
			CampaignEventDispatcher.Instance.Tick(_dt);
			_campaignPeriodicEventManager.OnTick(_dt);
			MapEventManager.Tick();
			_lastNonZeroDtFrame = _curMapFrame;
			_campaignPeriodicEventManager.MobilePartyHourlyTick();
		}
		if (_dt > 0f)
		{
			_campaignPeriodicEventManager.TickPeriodicEvents();
		}
		_tickData.Tick();
		Current.PlayerCaptivity.Update(_dt);
		if (_dt > 0f || (MobileParty.MainParty.MapEvent == null && _curMapFrame == _lastNonZeroDtFrame + 1))
		{
			EncounterManager.Tick(_dt);
			if (Game.Current.GameStateManager.ActiveState is MapState { AtMenu: not false } mapState && !mapState.MenuContext.GameMenu.IsWaitActive)
			{
				_dt = 0f;
			}
		}
		if (_dt > 0f || _curSessionFrame < 3)
		{
			_campaignPeriodicEventManager.TickPartialHourlyAi();
		}
		if (Game.Current.GameStateManager.ActiveState is MapState { AtMenu: false })
		{
			string genericStateMenu = Models.EncounterGameMenuModel.GetGenericStateMenu();
			if (!string.IsNullOrEmpty(genericStateMenu))
			{
				GameMenu.ActivateGameMenu(genericStateMenu);
			}
		}
		CampaignLateAITickTask.Invoke();
	}

	private void CreateCampaignEvents()
	{
		long numTicks = (CampaignTime.Now - CampaignData.CampaignStartTime).NumTicks;
		CampaignTime initialWait = CampaignTime.Days(1f);
		if (numTicks % 864000000 != 0L)
		{
			initialWait = CampaignTime.Days((float)(numTicks % 864000000) / 864000000f);
		}
		_dailyTickEvent = CampaignPeriodicEventManager.CreatePeriodicEvent(CampaignTime.Days(1f), initialWait);
		_dailyTickEvent.AddHandler(DailyTick);
		CampaignTime initialWait2 = CampaignTime.Hours(0.5f);
		if (numTicks % 36000000 != 0L)
		{
			initialWait2 = CampaignTime.Hours((float)(numTicks % 36000000) / 36000000f);
		}
		_hourlyTickEvent = CampaignPeriodicEventManager.CreatePeriodicEvent(CampaignTime.Hours(1f), initialWait2);
		_hourlyTickEvent.AddHandler(HourlyTick);
	}

	private void PartiesThink(float dt)
	{
		for (int i = 0; i < MobileParties.Count; i++)
		{
			MobileParties[i].Ai.Tick(dt);
		}
	}

	public TComponent GetEntityComponent<TComponent>() where TComponent : CampaignEntityComponent
	{
		EntitySystem<CampaignEntityComponent> campaignEntitySystem = _campaignEntitySystem;
		if (campaignEntitySystem == null)
		{
			return null;
		}
		return campaignEntitySystem.GetComponent<TComponent>();
	}

	public TComponent AddEntityComponent<TComponent>() where TComponent : CampaignEntityComponent, new()
	{
		return _campaignEntitySystem.AddComponent<TComponent>();
	}

	public T GetCampaignBehavior<T>()
	{
		return _campaignBehaviorManager.GetBehavior<T>();
	}

	public IEnumerable<T> GetCampaignBehaviors<T>()
	{
		return _campaignBehaviorManager.GetBehaviors<T>();
	}

	public void AddCampaignBehaviorManager(ICampaignBehaviorManager manager)
	{
		_campaignBehaviorManager = manager;
	}

	internal int GeneratePartyId(PartyBase party)
	{
		int lastPartyIndex = _lastPartyIndex;
		_lastPartyIndex++;
		return lastPartyIndex;
	}

	private void LoadMapScene()
	{
		_mapSceneWrapper = MapSceneCreator.CreateMapScene();
		_mapSceneWrapper.SetSceneLevels(new List<string> { "level_1", "level_2", "level_3", "siege", "raid", "burned" });
		_mapSceneWrapper.Load();
		_mapSceneWrapper.GetMapBorders(out var minimumPosition, out var maximumPosition, out var maximumHeight);
		MapMinimumPosition = minimumPosition;
		MapMaximumPosition = maximumPosition;
		MapMaximumHeight = maximumHeight;
		MapDiagonal = MapMinimumPosition.Distance(MapMaximumPosition);
		MapDiagonalSquared = MapDiagonal * MapDiagonal;
		UpdateMaximumDistanceBetweenTwoSettlements();
		MapWeatherModel mapWeatherModel = Current.Models.MapWeatherModel;
		if (mapWeatherModel != null)
		{
			byte[] array = new byte[2097152];
			_mapSceneWrapper.GetSnowAmountData(array);
			mapWeatherModel.InitializeSnowAndRainAmountData(array);
		}
	}

	public void UpdateMaximumDistanceBetweenTwoSettlements()
	{
		MaximumDistanceBetweenTwoSettlements = Current.Models.MapDistanceModel.MaximumDistanceBetweenTwoSettlements;
	}

	private void InitializeCachedLists()
	{
		MBObjectManager objectManager = Game.Current.ObjectManager;
		_characters = objectManager.GetObjectTypeList<CharacterObject>();
		_workshops = objectManager.GetObjectTypeList<WorkshopType>();
		_itemModifiers = objectManager.GetObjectTypeList<ItemModifier>();
		_itemModifierGroups = objectManager.GetObjectTypeList<ItemModifierGroup>();
		_concepts = objectManager.GetObjectTypeList<Concept>();
	}

	private void InitializeDefaultEquipments()
	{
		DeadBattleEquipment = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("default_battle_equipment_roster_neutral").DefaultEquipment;
		DeadCivilianEquipment = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("default_civilian_equipment_roster_neutral").DefaultEquipment;
	}

	public override void OnDestroy()
	{
		WaitAsyncTasks();
		GameTexts.ClearInstance();
		_mapSceneWrapper?.Destroy();
		ConversationManager.Clear();
		MBTextManager.ClearAll();
		GameSceneDataManager.Destroy();
		CampaignInformationManager.DeRegisterEvents();
		MBSaveLoad.OnGameDestroy();
		Current = null;
	}

	public void InitializeSinglePlayerReferences()
	{
		IsInitializedSinglePlayerReferences = true;
		InitializeGamePlayReferences();
	}

	private void CreateLists()
	{
		AllPerks = MBObjectManager.Instance.GetObjectTypeList<PerkObject>();
		AllTraits = MBObjectManager.Instance.GetObjectTypeList<TraitObject>();
		AllEquipmentRosters = MBObjectManager.Instance.GetObjectTypeList<MBEquipmentRoster>();
		AllPolicies = MBObjectManager.Instance.GetObjectTypeList<PolicyObject>();
		AllBuildingTypes = MBObjectManager.Instance.GetObjectTypeList<BuildingType>();
		AllIssueEffects = MBObjectManager.Instance.GetObjectTypeList<IssueEffect>();
		AllSiegeStrategies = MBObjectManager.Instance.GetObjectTypeList<SiegeStrategy>();
		AllVillageTypes = MBObjectManager.Instance.GetObjectTypeList<VillageType>();
		AllSkillEffects = MBObjectManager.Instance.GetObjectTypeList<SkillEffect>();
		AllFeats = MBObjectManager.Instance.GetObjectTypeList<FeatObject>();
		AllSkills = MBObjectManager.Instance.GetObjectTypeList<SkillObject>();
		AllSiegeEngineTypes = MBObjectManager.Instance.GetObjectTypeList<SiegeEngineType>();
		AllItemCategories = MBObjectManager.Instance.GetObjectTypeList<ItemCategory>();
		AllCharacterAttributes = MBObjectManager.Instance.GetObjectTypeList<CharacterAttribute>();
		AllItems = MBObjectManager.Instance.GetObjectTypeList<ItemObject>();
	}

	private void CalculateCachedValues()
	{
		CalculateAverageDistanceBetweenTowns();
		CalculateAverageWage();
	}

	private void CalculateAverageWage()
	{
		float num = 0f;
		float num2 = 0f;
		foreach (CultureObject objectType in MBObjectManager.Instance.GetObjectTypeList<CultureObject>())
		{
			if (!objectType.IsMainCulture)
			{
				continue;
			}
			foreach (PartyTemplateStack stack in objectType.DefaultPartyTemplate.Stacks)
			{
				int troopWage = stack.Character.TroopWage;
				float num3 = (float)(stack.MaxValue + stack.MinValue) * 0.5f;
				num += (float)troopWage * num3;
				num2 += num3;
			}
		}
		if (num2 > 0f)
		{
			AverageWage = num / num2;
		}
	}

	private void CalculateAverageDistanceBetweenTowns()
	{
		if (GameMode == CampaignGameMode.Tutorial)
		{
			return;
		}
		float num = 0f;
		int num2 = 0;
		foreach (Town allTown in AllTowns)
		{
			float num3 = 5000f;
			foreach (Town allTown2 in AllTowns)
			{
				if (allTown != allTown2)
				{
					float distance = Current.Models.MapDistanceModel.GetDistance(allTown.Settlement, allTown2.Settlement);
					if (distance < num3)
					{
						num3 = distance;
					}
				}
			}
			num += num3;
			num2++;
		}
		AverageDistanceBetweenTwoFortifications = num / (float)num2;
	}

	public void InitializeGamePlayReferences()
	{
		base.CurrentGame.PlayerTroop = base.CurrentGame.ObjectManager.GetObject<CharacterObject>("main_hero");
		if (Hero.MainHero.Mother != null)
		{
			Hero.MainHero.Mother.SetHasMet();
		}
		if (Hero.MainHero.Father != null)
		{
			Hero.MainHero.Father.SetHasMet();
		}
		PlayerDefaultFaction = CampaignObjectManager.Find<Clan>("player_faction");
		GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, 1000, disableNotification: true);
	}

	private void InitializeScenes()
	{
		foreach (ModuleInfo module in ModuleHelper.GetModules())
		{
			string text = ModuleHelper.GetModuleFullPath(module.Id) + "ModuleData/";
			string path = text + "sp_battle_scenes.xml";
			string path2 = text + "conversation_scenes.xml";
			string path3 = text + "meeting_scenes.xml";
			if (File.Exists(path))
			{
				GameSceneDataManager.Instance.LoadSPBattleScenes(path);
			}
			if (File.Exists(path2))
			{
				GameSceneDataManager.Instance.LoadConversationScenes(path2);
			}
			if (File.Exists(path3))
			{
				GameSceneDataManager.Instance.LoadMeetingScenes(path3);
			}
		}
	}

	public void SetLoadingParameters(GameLoadingType gameLoadingType)
	{
		Current = this;
		_gameLoadingType = gameLoadingType;
		if (gameLoadingType == GameLoadingType.SavedCampaign)
		{
			Current.GameStarted = true;
		}
	}

	public void AddCampaignEventReceiver(CampaignEventReceiver receiver)
	{
		CampaignEventDispatcher.AddCampaignEventReceiver(receiver);
	}

	protected override void OnInitialize()
	{
		CampaignEvents = new CampaignEvents();
		CustomPeriodicCampaignEvents = new List<MBCampaignEvent>();
		CampaignEventDispatcher = new CampaignEventDispatcher(new CampaignEventReceiver[3] { CampaignEvents, IssueManager, QuestManager });
		SandBoxManager = Game.Current.AddGameHandler<SandBoxManager>();
		SaveHandler = new SaveHandler();
		VisualCreator = new VisualCreator();
		GameMenuManager = new GameMenuManager();
		if (_gameLoadingType != GameLoadingType.Editor)
		{
			CreateManagers();
		}
		CampaignGameStarter campaignGameStarter = new CampaignGameStarter(GameMenuManager, ConversationManager, base.CurrentGame.GameTextManager);
		SandBoxManager.Initialize(campaignGameStarter);
		base.GameManager.InitializeGameStarter(base.CurrentGame, campaignGameStarter);
		GameSceneDataManager.Initialize();
		if (_gameLoadingType == GameLoadingType.NewCampaign || _gameLoadingType == GameLoadingType.SavedCampaign)
		{
			InitializeScenes();
		}
		base.GameManager.OnGameStart(base.CurrentGame, campaignGameStarter);
		base.CurrentGame.SetBasicModels(campaignGameStarter.Models);
		_gameModels = base.CurrentGame.AddGameModelsManager<GameModels>(campaignGameStarter.Models);
		base.CurrentGame.CreateGameManager();
		if (_gameLoadingType == GameLoadingType.SavedCampaign)
		{
			InitializeDefaultCampaignObjects();
		}
		base.GameManager.BeginGameStart(base.CurrentGame);
		if (_gameLoadingType != GameLoadingType.SavedCampaign)
		{
			OnNewCampaignStart();
		}
		CreateLists();
		InitializeBasicObjectXmls();
		if (_gameLoadingType != GameLoadingType.SavedCampaign)
		{
			base.GameManager.OnNewCampaignStart(base.CurrentGame, campaignGameStarter);
		}
		SandBoxManager.OnCampaignStart(campaignGameStarter, base.GameManager, _gameLoadingType == GameLoadingType.SavedCampaign);
		if (_gameLoadingType != GameLoadingType.SavedCampaign)
		{
			AddCampaignBehaviorManager(new CampaignBehaviorManager(campaignGameStarter.CampaignBehaviors));
			base.GameManager.OnAfterCampaignStart(base.CurrentGame);
		}
		else
		{
			base.GameManager.OnGameLoaded(base.CurrentGame, campaignGameStarter);
			_campaignBehaviorManager.InitializeCampaignBehaviors(campaignGameStarter.CampaignBehaviors);
			_campaignBehaviorManager.LoadBehaviorData();
			_campaignBehaviorManager.RegisterEvents();
		}
		Current.GetCampaignBehavior<ICraftingCampaignBehavior>()?.InitializeCraftingElements();
		campaignGameStarter.UnregisterNonReadyObjects();
		if (_gameLoadingType == GameLoadingType.SavedCampaign)
		{
			InitializeCampaignObjectsOnAfterLoad();
		}
		else if (_gameLoadingType == GameLoadingType.NewCampaign || _gameLoadingType == GameLoadingType.Tutorial)
		{
			CampaignObjectManager.InitializeOnNewGame();
		}
		InitializeCachedLists();
		InitializeDefaultEquipments();
		NameGenerator.Initialize();
		base.CurrentGame.OnGameStart();
		base.GameManager.OnGameInitializationFinished(base.CurrentGame);
	}

	private void CalculateCachedStatsOnLoad()
	{
		ItemRoster.CalculateCachedStatsOnLoad();
	}

	private void InitializeBasicObjectXmls()
	{
		base.ObjectManager.LoadXML("SPCultures");
		base.ObjectManager.LoadXML("Concepts");
	}

	private void InitializeDefaultCampaignObjects()
	{
		base.CurrentGame.InitializeDefaultGameObjects();
		DefaultItems = new DefaultItems();
		base.CurrentGame.LoadBasicFiles();
		base.ObjectManager.LoadXML("Items");
		base.ObjectManager.LoadXML("EquipmentRosters");
		base.ObjectManager.LoadXML("partyTemplates");
		WeaponDescription @object = MBObjectManager.Instance.GetObject<WeaponDescription>("OneHandedBastardSwordAlternative");
		if (@object != null)
		{
			@object.IsHiddenFromUI = true;
		}
		WeaponDescription object2 = MBObjectManager.Instance.GetObject<WeaponDescription>("OneHandedBastardAxeAlternative");
		if (object2 != null)
		{
			object2.IsHiddenFromUI = true;
		}
		DefaultIssueEffects = new DefaultIssueEffects();
		DefaultTraits = new DefaultTraits();
		DefaultPolicies = new DefaultPolicies();
		DefaultPerks = new DefaultPerks();
		DefaultBuildingTypes = new DefaultBuildingTypes();
		DefaultVillageTypes = new DefaultVillageTypes();
		DefaultSiegeStrategies = new DefaultSiegeStrategies();
		DefaultSkillEffects = new DefaultSkillEffects();
		DefaultFeats = new DefaultCulturalFeats();
	}

	private void InitializeManagers()
	{
		KingdomManager = new KingdomManager();
		CampaignInformationManager = new CampaignInformationManager();
		VisualTrackerManager = new VisualTrackerManager();
		TournamentManager = new TournamentManager();
	}

	private void InitializeCampaignObjectsOnAfterLoad()
	{
		CampaignObjectManager.InitializeOnLoad();
		FactionManager.AfterLoad();
		List<PerkObject> collection = AllPerks.Where((PerkObject x) => !x.IsTrash).ToList();
		AllPerks = new MBReadOnlyList<PerkObject>(collection);
		LogEntryHistory.OnAfterLoad();
		foreach (Kingdom kingdom in Kingdoms)
		{
			foreach (Army army in kingdom.Armies)
			{
				army.OnAfterLoad();
			}
		}
	}

	private void OnNewCampaignStart()
	{
		Game.Current.PlayerTroop = null;
		MapStateData = new MapStateData();
		InitializeDefaultCampaignObjects();
		MainParty = MBObjectManager.Instance.CreateObject<MobileParty>("player_party");
		MainParty.Ai.SetAsMainParty();
		InitializeManagers();
	}

	protected override void BeforeRegisterTypes(MBObjectManager objectManager)
	{
		objectManager.RegisterType<FeatObject>("Feat", "Feats", 0u);
	}

	protected override void OnRegisterTypes(MBObjectManager objectManager)
	{
		objectManager.RegisterType<MobileParty>("MobileParty", "MobileParties", 14u, autoCreateInstance: true, isTemporary: true);
		objectManager.RegisterType<CharacterObject>("NPCCharacter", "NPCCharacters", 16u);
		if (GameMode == CampaignGameMode.Tutorial)
		{
			objectManager.RegisterType<BasicCharacterObject>("NPCCharacter", "MPCharacters", 43u);
		}
		objectManager.RegisterType<CultureObject>("Culture", "SPCultures", 17u);
		objectManager.RegisterType<Clan>("Faction", "Factions", 18u, autoCreateInstance: true, isTemporary: true);
		objectManager.RegisterType<PerkObject>("Perk", "Perks", 19u);
		objectManager.RegisterType<Kingdom>("Kingdom", "Kingdoms", 20u, autoCreateInstance: true, isTemporary: true);
		objectManager.RegisterType<TraitObject>("Trait", "Traits", 21u);
		objectManager.RegisterType<VillageType>("VillageType", "VillageTypes", 22u);
		objectManager.RegisterType<BuildingType>("BuildingType", "BuildingTypes", 23u);
		objectManager.RegisterType<PartyTemplateObject>("PartyTemplate", "partyTemplates", 24u);
		objectManager.RegisterType<Settlement>("Settlement", "Settlements", 25u);
		objectManager.RegisterType<WorkshopType>("WorkshopType", "WorkshopTypes", 26u);
		objectManager.RegisterType<Village>("Village", "Components", 27u);
		objectManager.RegisterType<Hideout>("Hideout", "Components", 30u);
		objectManager.RegisterType<Town>("Town", "Components", 31u);
		objectManager.RegisterType<Hero>("Hero", "Heroes", 32u, autoCreateInstance: true, isTemporary: true);
		objectManager.RegisterType<MenuContext>("MenuContext", "MenuContexts", 35u);
		objectManager.RegisterType<PolicyObject>("Policy", "Policies", 36u);
		objectManager.RegisterType<Concept>("Concept", "Concepts", 37u);
		objectManager.RegisterType<IssueEffect>("IssueEffect", "IssueEffects", 39u);
		objectManager.RegisterType<SiegeStrategy>("SiegeStrategy", "SiegeStrategies", 40u);
		objectManager.RegisterType<SkillEffect>("SkillEffect", "SkillEffects", 53u);
		objectManager.RegisterType<LocationComplexTemplate>("LocationComplexTemplate", "LocationComplexTemplates", 42u);
		objectManager.RegisterType<RetirementSettlementComponent>("RetirementSettlementComponent", "Components", 56u);
	}

	private void CreateManagers()
	{
		EncyclopediaManager = new EncyclopediaManager();
		InventoryManager = new InventoryManager();
		PartyScreenManager = new PartyScreenManager();
		ConversationManager = new ConversationManager();
		NameGenerator = new NameGenerator();
		SkillLevelingManager = new DefaultSkillLevelingManager();
	}

	private void OnNewGameCreated(CampaignGameStarter gameStarter)
	{
		OnNewGameCreatedInternal();
		base.GameManager?.OnNewGameCreated(base.CurrentGame, gameStarter);
		CampaignEventDispatcher.Instance.OnNewGameCreated(gameStarter);
		OnAfterNewGameCreatedInternal();
	}

	private void OnNewGameCreatedInternal()
	{
		UniqueGameId = MiscHelper.GenerateCampaignId(12);
		_newGameVersion = MBSaveLoad.CurrentVersion.ToString();
		PlatformID = ApplicationPlatform.CurrentPlatform.ToString();
		PlayerTraitDeveloper = new HeroTraitDeveloper(Hero.MainHero);
		TimeControlMode = CampaignTimeControlMode.Stop;
		_campaignEntitySystem = new EntitySystem<CampaignEntityComponent>();
		SiegeEventManager = new SiegeEventManager();
		MapEventManager = new MapEventManager();
		autoEnterTown = null;
		MinSettlementX = 1000f;
		MinSettlementY = 1000f;
		foreach (Settlement item in Settlement.All)
		{
			if (item.Position2D.x < MinSettlementX)
			{
				MinSettlementX = item.Position2D.x;
			}
			if (item.Position2D.y < MinSettlementY)
			{
				MinSettlementY = item.Position2D.y;
			}
			if (item.Position2D.x > MaxSettlementX)
			{
				MaxSettlementX = item.Position2D.x;
			}
			if (item.Position2D.y > MaxSettlementY)
			{
				MaxSettlementY = item.Position2D.y;
			}
		}
		CampaignBehaviorManager.RegisterEvents();
		CameraFollowParty = MainParty.Party;
	}

	private void OnAfterNewGameCreatedInternal()
	{
		Hero.MainHero.Gold = 1000;
		ChangeClanInfluenceAction.Apply(Clan.PlayerClan, 0f - Clan.PlayerClan.Influence);
		Hero.MainHero.ChangeState(Hero.CharacterStates.Active);
		GameInitTick();
		_playerFormationPreferences = new Dictionary<CharacterObject, FormationClass>();
		PlayerFormationPreferences = _playerFormationPreferences.GetReadOnlyDictionary();
		Current.DesertionEnabled = true;
	}

	protected override void DoLoadingForGameType(GameTypeLoadingStates gameTypeLoadingState, out GameTypeLoadingStates nextState)
	{
		nextState = GameTypeLoadingStates.None;
		switch (gameTypeLoadingState)
		{
		case GameTypeLoadingStates.InitializeFirstStep:
			base.CurrentGame.Initialize();
			nextState = GameTypeLoadingStates.WaitSecondStep;
			break;
		case GameTypeLoadingStates.WaitSecondStep:
			nextState = GameTypeLoadingStates.LoadVisualsThirdState;
			break;
		case GameTypeLoadingStates.LoadVisualsThirdState:
			if (GameMode == CampaignGameMode.Campaign)
			{
				LoadMapScene();
			}
			nextState = GameTypeLoadingStates.PostInitializeFourthState;
			break;
		case GameTypeLoadingStates.PostInitializeFourthState:
		{
			CampaignGameStarter gameStarter = SandBoxManager.GameStarter;
			if (_gameLoadingType == GameLoadingType.SavedCampaign)
			{
				OnDataLoadFinished(gameStarter);
				CalculateCachedValues();
				DeterminedSavedStats(_gameLoadingType);
				foreach (Settlement item in Settlement.All)
				{
					item.Party.OnGameInitialized();
				}
				foreach (MobileParty item2 in MobileParties.ToList())
				{
					item2.Party.OnGameInitialized();
				}
				CalculateCachedStatsOnLoad();
				OnGameLoaded(gameStarter);
				OnSessionStart(gameStarter);
				foreach (Hero allAliveHero in Hero.AllAliveHeroes)
				{
					allAliveHero.CheckInvalidEquipmentsAndReplaceIfNeeded();
				}
				foreach (Hero deadOrDisabledHero in Hero.DeadOrDisabledHeroes)
				{
					deadOrDisabledHero.CheckInvalidEquipmentsAndReplaceIfNeeded();
				}
			}
			else if (_gameLoadingType == GameLoadingType.NewCampaign)
			{
				OnDataLoadFinished(gameStarter);
				CalculateCachedValues();
				MBSaveLoad.OnNewGame();
				InitializeMainParty();
				DeterminedSavedStats(_gameLoadingType);
				foreach (Settlement item3 in Settlement.All)
				{
					item3.Party.OnGameInitialized();
				}
				foreach (MobileParty item4 in MobileParties.ToList())
				{
					item4.Party.OnGameInitialized();
				}
				foreach (Settlement item5 in Settlement.All)
				{
					item5.OnGameCreated();
				}
				foreach (Clan item6 in Clan.All)
				{
					item6.OnGameCreated();
				}
				MBObjectManager.Instance.RemoveTemporaryTypes();
				OnNewGameCreated(gameStarter);
				OnSessionStart(gameStarter);
				Debug.Print("Finished starting a new game.");
			}
			base.GameManager.OnAfterGameInitializationFinished(base.CurrentGame, gameStarter);
			break;
		}
		}
	}

	private void DeterminedSavedStats(GameLoadingType gameLoadingType)
	{
		if (_previouslyUsedModules == null)
		{
			_previouslyUsedModules = new MBList<string>();
		}
		string[] moduleNames = SandBoxManager.Instance.ModuleManager.ModuleNames;
		foreach (string item in moduleNames)
		{
			if (!_previouslyUsedModules.Contains(item))
			{
				_previouslyUsedModules.Add(item);
			}
		}
		if (_usedGameVersions == null)
		{
			_usedGameVersions = new MBList<string>();
		}
		string text = MBSaveLoad.LastLoadedGameVersion.ToString();
		if (_usedGameVersions.Count <= 0 || !_usedGameVersions[_usedGameVersions.Count - 1].Equals(text))
		{
			_usedGameVersions.Add(text);
		}
	}

	public override void OnMissionIsStarting(string missionName, MissionInitializerRecord rec)
	{
		if (rec.PlayingInCampaignMode)
		{
			CampaignEventDispatcher.Instance.BeforeMissionOpened();
		}
	}

	public override void InitializeParameters()
	{
		ManagedParameters.Instance.Initialize(ModuleHelper.GetXmlPath("Native", "managed_campaign_parameters"));
	}

	public void SetTimeControlModeLock(bool isLocked)
	{
		TimeControlModeLock = isLocked;
	}

	public void OnPlayerCharacterChanged(out bool isMainPartyChanged)
	{
		isMainPartyChanged = false;
		if (MobileParty.MainParty != Hero.MainHero.PartyBelongedTo)
		{
			isMainPartyChanged = true;
		}
		MainParty = Hero.MainHero.PartyBelongedTo;
		if (Hero.MainHero.CurrentSettlement != null && !Hero.MainHero.IsPrisoner)
		{
			if (MainParty == null)
			{
				LeaveSettlementAction.ApplyForCharacterOnly(Hero.MainHero);
			}
			else
			{
				LeaveSettlementAction.ApplyForParty(MainParty);
			}
		}
		if (Hero.MainHero.IsFugitive)
		{
			Hero.MainHero.ChangeState(Hero.CharacterStates.Active);
		}
		PlayerTraitDeveloper = new HeroTraitDeveloper(Hero.MainHero);
		if (MainParty == null)
		{
			MainParty = MobileParty.CreateParty("player_party_" + Hero.MainHero.StringId, new LordPartyComponent(Hero.MainHero, Hero.MainHero), delegate(MobileParty mobileParty)
			{
				mobileParty.ActualClan = Clan.PlayerClan;
			});
			isMainPartyChanged = true;
			if (Hero.MainHero.IsPrisoner)
			{
				MainParty.InitializeMobilePartyAtPosition(base.CurrentGame.ObjectManager.GetObject<PartyTemplateObject>("main_hero_party_template"), Hero.MainHero.GetPosition().AsVec2, 0);
				MainParty.IsActive = false;
			}
			else
			{
				Vec2 position = ((Hero.MainHero.GetPosition().AsVec2 != Vec2.Zero) ? Hero.MainHero.GetPosition().AsVec2 : SettlementHelper.FindRandomSettlement((Settlement s) => s.OwnerClan != null && !s.OwnerClan.IsAtWarWith(Clan.PlayerClan)).GetPosition2D);
				MainParty.InitializeMobilePartyAtPosition(base.CurrentGame.ObjectManager.GetObject<PartyTemplateObject>("main_hero_party_template"), position, 0);
				MainParty.IsActive = true;
				MainParty.MemberRoster.AddToCounts(Hero.MainHero.CharacterObject, 1, insertAtFront: true);
			}
		}
		Current.MainParty.Ai.SetAsMainParty();
		PartyBase.MainParty.ItemRoster.UpdateVersion();
		PartyBase.MainParty.MemberRoster.UpdateVersion();
		if (MobileParty.MainParty.IsActive)
		{
			PartyBase.MainParty.SetAsCameraFollowParty();
		}
		PartyBase.MainParty.UpdateVisibilityAndInspected();
		if (Hero.MainHero.Mother != null)
		{
			Hero.MainHero.Mother.SetHasMet();
		}
		if (Hero.MainHero.Father != null)
		{
			Hero.MainHero.Father.SetHasMet();
		}
		MainParty.SetWagePaymentLimit(Current.Models.PartyWageModel.MaxWage);
	}

	public void SetPlayerFormationPreference(CharacterObject character, FormationClass formation)
	{
		if (!_playerFormationPreferences.ContainsKey(character))
		{
			_playerFormationPreferences.Add(character, formation);
		}
		else
		{
			_playerFormationPreferences[character] = formation;
		}
	}

	public override void OnStateChanged(TaleWorlds.Core.GameState oldState)
	{
	}

	internal static void AutoGeneratedStaticCollectObjectsCampaign(object o, List<object> collectedObjects)
	{
		((Campaign)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(Options);
		collectedObjects.Add(TournamentManager);
		collectedObjects.Add(autoEnterTown);
		collectedObjects.Add(BusyHideouts);
		collectedObjects.Add(KingdomManager);
		collectedObjects.Add(_campaignPeriodicEventManager);
		collectedObjects.Add(_previouslyUsedModules);
		collectedObjects.Add(_usedGameVersions);
		collectedObjects.Add(_playerFormationPreferences);
		collectedObjects.Add(_campaignBehaviorManager);
		collectedObjects.Add(_cameraFollowParty);
		collectedObjects.Add(_logEntryHistory);
		collectedObjects.Add(CampaignObjectManager);
		collectedObjects.Add(QuestManager);
		collectedObjects.Add(IssueManager);
		collectedObjects.Add(FactionManager);
		collectedObjects.Add(CharacterRelationManager);
		collectedObjects.Add(Romance);
		collectedObjects.Add(PlayerCaptivity);
		collectedObjects.Add(PlayerDefaultFaction);
		collectedObjects.Add(MapStateData);
		collectedObjects.Add(MapTimeTracker);
		CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime(CampaignStartTime, collectedObjects);
		collectedObjects.Add(SiegeEventManager);
		collectedObjects.Add(MapEventManager);
		collectedObjects.Add(PlayerEncounter);
		collectedObjects.Add(BarterManager);
		collectedObjects.Add(MainParty);
		collectedObjects.Add(CampaignInformationManager);
		collectedObjects.Add(VisualTrackerManager);
		collectedObjects.Add(PlayerTraitDeveloper);
	}

	internal static object AutoGeneratedGetMemberValueEnabledCheatsBefore(object o)
	{
		return ((Campaign)o).EnabledCheatsBefore;
	}

	internal static object AutoGeneratedGetMemberValuePlatformID(object o)
	{
		return ((Campaign)o).PlatformID;
	}

	internal static object AutoGeneratedGetMemberValueUniqueGameId(object o)
	{
		return ((Campaign)o).UniqueGameId;
	}

	internal static object AutoGeneratedGetMemberValueCampaignObjectManager(object o)
	{
		return ((Campaign)o).CampaignObjectManager;
	}

	internal static object AutoGeneratedGetMemberValueIsCraftingEnabled(object o)
	{
		return ((Campaign)o).IsCraftingEnabled;
	}

	internal static object AutoGeneratedGetMemberValueIsBannerEditorEnabled(object o)
	{
		return ((Campaign)o).IsBannerEditorEnabled;
	}

	internal static object AutoGeneratedGetMemberValueIsFaceGenEnabled(object o)
	{
		return ((Campaign)o).IsFaceGenEnabled;
	}

	internal static object AutoGeneratedGetMemberValueQuestManager(object o)
	{
		return ((Campaign)o).QuestManager;
	}

	internal static object AutoGeneratedGetMemberValueIssueManager(object o)
	{
		return ((Campaign)o).IssueManager;
	}

	internal static object AutoGeneratedGetMemberValueFactionManager(object o)
	{
		return ((Campaign)o).FactionManager;
	}

	internal static object AutoGeneratedGetMemberValueCharacterRelationManager(object o)
	{
		return ((Campaign)o).CharacterRelationManager;
	}

	internal static object AutoGeneratedGetMemberValueRomance(object o)
	{
		return ((Campaign)o).Romance;
	}

	internal static object AutoGeneratedGetMemberValuePlayerCaptivity(object o)
	{
		return ((Campaign)o).PlayerCaptivity;
	}

	internal static object AutoGeneratedGetMemberValuePlayerDefaultFaction(object o)
	{
		return ((Campaign)o).PlayerDefaultFaction;
	}

	internal static object AutoGeneratedGetMemberValueMapStateData(object o)
	{
		return ((Campaign)o).MapStateData;
	}

	internal static object AutoGeneratedGetMemberValueMapTimeTracker(object o)
	{
		return ((Campaign)o).MapTimeTracker;
	}

	internal static object AutoGeneratedGetMemberValueCampaignStartTime(object o)
	{
		return ((Campaign)o).CampaignStartTime;
	}

	internal static object AutoGeneratedGetMemberValueGameMode(object o)
	{
		return ((Campaign)o).GameMode;
	}

	internal static object AutoGeneratedGetMemberValuePlayerProgress(object o)
	{
		return ((Campaign)o).PlayerProgress;
	}

	internal static object AutoGeneratedGetMemberValueSiegeEventManager(object o)
	{
		return ((Campaign)o).SiegeEventManager;
	}

	internal static object AutoGeneratedGetMemberValueMapEventManager(object o)
	{
		return ((Campaign)o).MapEventManager;
	}

	internal static object AutoGeneratedGetMemberValue_curMapFrame(object o)
	{
		return ((Campaign)o)._curMapFrame;
	}

	internal static object AutoGeneratedGetMemberValuePlayerEncounter(object o)
	{
		return ((Campaign)o).PlayerEncounter;
	}

	internal static object AutoGeneratedGetMemberValueBarterManager(object o)
	{
		return ((Campaign)o).BarterManager;
	}

	internal static object AutoGeneratedGetMemberValueIsMainHeroDisguised(object o)
	{
		return ((Campaign)o).IsMainHeroDisguised;
	}

	internal static object AutoGeneratedGetMemberValueDesertionEnabled(object o)
	{
		return ((Campaign)o).DesertionEnabled;
	}

	internal static object AutoGeneratedGetMemberValueMainParty(object o)
	{
		return ((Campaign)o).MainParty;
	}

	internal static object AutoGeneratedGetMemberValueCampaignInformationManager(object o)
	{
		return ((Campaign)o).CampaignInformationManager;
	}

	internal static object AutoGeneratedGetMemberValueVisualTrackerManager(object o)
	{
		return ((Campaign)o).VisualTrackerManager;
	}

	internal static object AutoGeneratedGetMemberValuePlayerTraitDeveloper(object o)
	{
		return ((Campaign)o).PlayerTraitDeveloper;
	}

	internal static object AutoGeneratedGetMemberValueOptions(object o)
	{
		return ((Campaign)o).Options;
	}

	internal static object AutoGeneratedGetMemberValueTournamentManager(object o)
	{
		return ((Campaign)o).TournamentManager;
	}

	internal static object AutoGeneratedGetMemberValueIsInitializedSinglePlayerReferences(object o)
	{
		return ((Campaign)o).IsInitializedSinglePlayerReferences;
	}

	internal static object AutoGeneratedGetMemberValueLastTimeControlMode(object o)
	{
		return ((Campaign)o).LastTimeControlMode;
	}

	internal static object AutoGeneratedGetMemberValueautoEnterTown(object o)
	{
		return ((Campaign)o).autoEnterTown;
	}

	internal static object AutoGeneratedGetMemberValueMainHeroIllDays(object o)
	{
		return ((Campaign)o).MainHeroIllDays;
	}

	internal static object AutoGeneratedGetMemberValueBusyHideouts(object o)
	{
		return ((Campaign)o).BusyHideouts;
	}

	internal static object AutoGeneratedGetMemberValueKingdomManager(object o)
	{
		return ((Campaign)o).KingdomManager;
	}

	internal static object AutoGeneratedGetMemberValue_campaignPeriodicEventManager(object o)
	{
		return ((Campaign)o)._campaignPeriodicEventManager;
	}

	internal static object AutoGeneratedGetMemberValue_isMainPartyWaiting(object o)
	{
		return ((Campaign)o)._isMainPartyWaiting;
	}

	internal static object AutoGeneratedGetMemberValue_newGameVersion(object o)
	{
		return ((Campaign)o)._newGameVersion;
	}

	internal static object AutoGeneratedGetMemberValue_previouslyUsedModules(object o)
	{
		return ((Campaign)o)._previouslyUsedModules;
	}

	internal static object AutoGeneratedGetMemberValue_usedGameVersions(object o)
	{
		return ((Campaign)o)._usedGameVersions;
	}

	internal static object AutoGeneratedGetMemberValue_playerFormationPreferences(object o)
	{
		return ((Campaign)o)._playerFormationPreferences;
	}

	internal static object AutoGeneratedGetMemberValue_campaignBehaviorManager(object o)
	{
		return ((Campaign)o)._campaignBehaviorManager;
	}

	internal static object AutoGeneratedGetMemberValue_lastPartyIndex(object o)
	{
		return ((Campaign)o)._lastPartyIndex;
	}

	internal static object AutoGeneratedGetMemberValue_cameraFollowParty(object o)
	{
		return ((Campaign)o)._cameraFollowParty;
	}

	internal static object AutoGeneratedGetMemberValue_logEntryHistory(object o)
	{
		return ((Campaign)o)._logEntryHistory;
	}
}
