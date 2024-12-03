using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace TaleWorlds.CampaignSystem.Party;

public sealed class MobileParty : CampaignObjectBase, ILocatable<MobileParty>, IMapPoint, ITrackableCampaignObject, ITrackableBase, IMapEntity, IRandomOwner
{
	public enum PartyObjective
	{
		Neutral,
		Defensive,
		Aggressive,
		NumberOfPartyObjectives
	}

	internal struct CachedPartyVariables
	{
		internal bool IsAttachedArmyMember;

		internal bool IsArmyLeader;

		internal bool IsMoving;

		internal bool HasMapEvent;

		internal float NextMoveDistance;

		internal Vec2 CurrentPosition;

		internal Vec2 LastCurrentPosition;

		internal Vec2 NextPosition;

		internal Vec2 TargetPartyPositionAtFrameStart;

		internal PathFaceRecord TargetPartyCurrentNavigationFaceAtFrameStart;

		internal PathFaceRecord NextPathFaceRecord;
	}

	public const int DefaultPartyTradeInitialGold = 5000;

	public const int ClanRoleAssignmentMinimumSkillValue = 0;

	[SaveableField(1001)]
	private Settlement _currentSettlement;

	[CachedData]
	private MBList<MobileParty> _attachedParties;

	[SaveableField(1046)]
	private MobileParty _attachedTo;

	[SaveableField(1006)]
	public float HasUnpaidWages;

	[SaveableField(1060)]
	private Vec2 _eventPositionAdder;

	[SaveableField(1100)]
	private Vec2 _position2D;

	[SaveableField(1024)]
	private bool _isVisible;

	[CachedData]
	internal float _lastCalculatedSpeed = 1f;

	[SaveableField(1025)]
	private bool _isInspected;

	[SaveableField(1955)]
	private CampaignTime _disorganizedUntilTime;

	[CachedData]
	private int _partyPureSpeedLastCheckVersion = -1;

	[CachedData]
	private bool _partyLastCheckIsPrisoner;

	[CachedData]
	private ExplainedNumber _lastCalculatedBaseSpeedExplained;

	[CachedData]
	private bool _partyLastCheckAtNight;

	[CachedData]
	private int _itemRosterVersionNo = -1;

	[CachedData]
	private int _partySizeRatioLastCheckVersion = -1;

	[CachedData]
	private int _latestUsedPaymentRatio = -1;

	[CachedData]
	private float _cachedPartySizeRatio = 1f;

	[CachedData]
	private int _cachedPartySizeLimit;

	[SaveableField(1059)]
	private BesiegerCamp _besiegerCamp;

	[SaveableField(1048)]
	private MobileParty _targetParty;

	[SaveableField(1049)]
	private Settlement _targetSettlement;

	[SaveableField(1053)]
	private Vec2 _targetPosition;

	private int _doNotAttackMainParty;

	[SaveableField(1034)]
	private Settlement _customHomeSettlement;

	[SaveableField(1035)]
	private Army _army;

	[CachedData]
	private bool _isDisorganized;

	[SaveableField(1959)]
	private bool _isCurrentlyUsedByAQuest;

	[SaveableField(1956)]
	private int _partyTradeGold;

	[SaveableField(1063)]
	private CampaignTime _ignoredUntilTime;

	[SaveableField(1071)]
	public Vec2 AverageFleeTargetDirection;

	private bool _besiegerCampResetStarted;

	[CachedData]
	private int _locatorNodeIndex;

	[SaveableField(1120)]
	private Clan _actualClan;

	[SaveableField(1200)]
	private float _moraleDueToEvents;

	[CachedData]
	private PathFaceRecord _lastNavigationFace;

	[CachedData]
	private MapWeatherModel.WeatherEventEffectOnTerrain _lastWeatherTerrainEffect;

	[CachedData]
	private PathFaceRecord _currentNavigationFace;

	[SaveableField(210)]
	private PartyComponent _partyComponent;

	public static MobileParty MainParty => Campaign.Current.MainParty;

	public static MBReadOnlyList<MobileParty> All => Campaign.Current.MobileParties;

	public static MBReadOnlyList<MobileParty> AllCaravanParties => Campaign.Current.CaravanParties;

	public static MBReadOnlyList<MobileParty> AllBanditParties => Campaign.Current.BanditParties;

	public static MBReadOnlyList<MobileParty> AllLordParties => Campaign.Current.LordParties;

	public static MBReadOnlyList<MobileParty> AllGarrisonParties => Campaign.Current.GarrisonParties;

	public static MBReadOnlyList<MobileParty> AllMilitiaParties => Campaign.Current.MilitiaParties;

	public static MBReadOnlyList<MobileParty> AllVillagerParties => Campaign.Current.VillagerParties;

	public static MBReadOnlyList<MobileParty> AllCustomParties => Campaign.Current.CustomParties;

	public static MBReadOnlyList<MobileParty> AllPartiesWithoutPartyComponent => Campaign.Current.PartiesWithoutPartyComponent;

	public static int Count => Campaign.Current.MobileParties.Count;

	public static MobileParty ConversationParty => Campaign.Current.ConversationManager.ConversationParty;

	[SaveableProperty(1021)]
	private TextObject CustomName { get; set; }

	public TextObject Name
	{
		get
		{
			if (TextObject.IsNullOrEmpty(CustomName))
			{
				if (_partyComponent == null)
				{
					return new TextObject("{=!}unnamedMobileParty");
				}
				return _partyComponent.Name;
			}
			return CustomName;
		}
	}

	[SaveableProperty(1002)]
	public Settlement LastVisitedSettlement { get; private set; }

	[SaveableProperty(1004)]
	public Vec2 Bearing { get; internal set; }

	public MBReadOnlyList<MobileParty> AttachedParties => _attachedParties;

	[SaveableProperty(1009)]
	public float Aggressiveness { get; set; }

	public int PaymentLimit => _partyComponent?.WagePaymentLimit ?? Campaign.Current.Models.PartyWageModel.MaxWage;

	[SaveableProperty(1005)]
	public Vec2 ArmyPositionAdder { get; private set; }

	public Vec2 AiBehaviorTarget => Ai.BehaviorTarget;

	[SaveableProperty(1090)]
	public PartyObjective Objective { get; private set; }

	[CachedData]
	MobileParty ILocatable<MobileParty>.NextLocatable { get; set; }

	[SaveableProperty(1019)]
	public MobilePartyAi Ai { get; private set; }

	[SaveableProperty(1020)]
	public PartyBase Party { get; private set; }

	[SaveableProperty(1023)]
	public bool IsActive { get; set; }

	public CampaignTime DisorganizedUntilTime => _disorganizedUntilTime;

	[CachedData]
	public PartyThinkParams ThinkParamsCache { get; private set; }

	public float Speed => CalculateSpeed();

	public ExplainedNumber SpeedExplained
	{
		get
		{
			_lastCalculatedBaseSpeedExplained = Campaign.Current.Models.PartySpeedCalculatingModel.CalculateBaseSpeed(this, includeDescriptions: true);
			ExplainedNumber result = Campaign.Current.Models.PartySpeedCalculatingModel.CalculateFinalSpeed(this, _lastCalculatedBaseSpeedExplained);
			_lastCalculatedSpeed = result.ResultNumber;
			return result;
		}
	}

	public MobileParty ShortTermTargetParty => Ai.AiBehaviorPartyBase?.MobileParty;

	public Settlement ShortTermTargetSettlement => Ai.AiBehaviorPartyBase?.Settlement;

	public bool IsDisorganized => _isDisorganized;

	public bool IsCurrentlyUsedByAQuest => _isCurrentlyUsedByAQuest;

	[SaveableProperty(1050)]
	public AiBehavior ShortTermBehavior { get; internal set; }

	[SaveableProperty(1958)]
	public bool IsPartyTradeActive { get; private set; }

	public int PartyTradeGold
	{
		get
		{
			return _partyTradeGold;
		}
		set
		{
			_partyTradeGold = TaleWorlds.Library.MathF.Max(value, 0);
		}
	}

	[SaveableProperty(1957)]
	public int PartyTradeTaxGold { get; private set; }

	[SaveableProperty(1960)]
	public CampaignTime StationaryStartTime { get; private set; }

	[CachedData]
	public int VersionNo { get; private set; }

	[SaveableProperty(1080)]
	public bool ShouldJoinPlayerBattles { get; set; }

	[SaveableProperty(1081)]
	public bool IsDisbanding { get; set; }

	public int RandomValue => Party.RandomValue;

	public Settlement CurrentSettlement
	{
		get
		{
			return _currentSettlement;
		}
		set
		{
			if (value == _currentSettlement)
			{
				return;
			}
			if (_currentSettlement != null)
			{
				_currentSettlement.RemoveMobileParty(this);
			}
			_currentSettlement = value;
			if (_currentSettlement != null)
			{
				_currentSettlement.AddMobileParty(this);
				if (_currentSettlement.IsFortification)
				{
					Position2D = _currentSettlement.GatePosition;
				}
				LastVisitedSettlement = value;
			}
			foreach (MobileParty attachedParty in _attachedParties)
			{
				attachedParty.CurrentSettlement = value;
			}
			if (_currentSettlement != null && _currentSettlement.IsFortification)
			{
				ArmyPositionAdder = Vec2.Zero;
				ErrorPosition = Vec2.Zero;
				Bearing = Vec2.Zero;
				Party.AverageBearingRotation = 0f;
				foreach (MobileParty party in _currentSettlement.Parties)
				{
					party.Party.SetVisualAsDirty();
				}
			}
			Party.SetVisualAsDirty();
		}
	}

	public Settlement HomeSettlement
	{
		get
		{
			Settlement settlement = _customHomeSettlement;
			if (settlement == null)
			{
				PartyComponent partyComponent = _partyComponent;
				if (partyComponent == null)
				{
					return null;
				}
				settlement = partyComponent.HomeSettlement;
			}
			return settlement;
		}
	}

	public MobileParty AttachedTo
	{
		get
		{
			return _attachedTo;
		}
		set
		{
			if (_attachedTo != value)
			{
				SetAttachedToInternal(value);
			}
		}
	}

	public Army Army
	{
		get
		{
			return _army;
		}
		set
		{
			if (_army == value)
			{
				return;
			}
			UpdateVersionNo();
			if (_army != null)
			{
				_army.OnRemovePartyInternal(this);
			}
			_army = value;
			if (value == null)
			{
				if (this == MainParty && Game.Current.GameStateManager.ActiveState is MapState)
				{
					((MapState)Game.Current.GameStateManager.ActiveState).OnLeaveArmy();
				}
			}
			else
			{
				_army.OnAddPartyInternal(this);
				Ai.ResetNumberOfRecentFleeing();
			}
		}
	}

	public BesiegerCamp BesiegerCamp
	{
		get
		{
			return _besiegerCamp;
		}
		set
		{
			if (_besiegerCamp == value || _besiegerCampResetStarted)
			{
				return;
			}
			_besiegerCampResetStarted = true;
			if (_besiegerCamp != null)
			{
				OnPartyLeftSiegeInternal();
			}
			_besiegerCamp = value;
			if (_besiegerCamp != null)
			{
				OnPartyJoinedSiegeInternal();
			}
			foreach (MobileParty attachedParty in _attachedParties)
			{
				attachedParty.BesiegerCamp = value;
			}
			Party.SetVisualAsDirty();
			_besiegerCampResetStarted = false;
		}
	}

	public AiBehavior DefaultBehavior => Ai.DefaultBehavior;

	public Settlement TargetSettlement
	{
		get
		{
			return _targetSettlement;
		}
		internal set
		{
			if (value != _targetSettlement)
			{
				_targetSettlement = value;
				Ai.DefaultBehaviorNeedsUpdate = true;
			}
		}
	}

	public Vec2 TargetPosition
	{
		get
		{
			return _targetPosition;
		}
		internal set
		{
			if (_targetPosition != value)
			{
				_targetPosition = value;
				Ai.DefaultBehaviorNeedsUpdate = true;
			}
		}
	}

	public MobileParty TargetParty
	{
		get
		{
			return _targetParty;
		}
		internal set
		{
			if (value != _targetParty)
			{
				_targetParty = value;
				Ai.DefaultBehaviorNeedsUpdate = true;
			}
		}
	}

	public Hero LeaderHero => PartyComponent?.Leader;

	[SaveableProperty(1070)]
	private Hero Scout { get; set; }

	[SaveableProperty(1072)]
	private Hero Engineer { get; set; }

	[SaveableProperty(1071)]
	private Hero Quartermaster { get; set; }

	[SaveableProperty(1073)]
	private Hero Surgeon { get; set; }

	public Hero Owner => _partyComponent?.PartyOwner;

	public Hero EffectiveScout
	{
		get
		{
			if (Scout == null || Scout.PartyBelongedTo != this)
			{
				return LeaderHero;
			}
			return Scout;
		}
	}

	public Hero EffectiveQuartermaster
	{
		get
		{
			if (Quartermaster == null || Quartermaster.PartyBelongedTo != this)
			{
				return LeaderHero;
			}
			return Quartermaster;
		}
	}

	public Hero EffectiveEngineer
	{
		get
		{
			if (Engineer == null || Engineer.PartyBelongedTo != this)
			{
				return LeaderHero;
			}
			return Engineer;
		}
	}

	public Hero EffectiveSurgeon
	{
		get
		{
			if (Surgeon == null || Surgeon.PartyBelongedTo != this)
			{
				return LeaderHero;
			}
			return Surgeon;
		}
	}

	public Clan ActualClan
	{
		get
		{
			return _actualClan;
		}
		set
		{
			if (_actualClan != value)
			{
				if (_actualClan != null && value != null && PartyComponent is WarPartyComponent warPartyComponent)
				{
					warPartyComponent.OnClanChange(_actualClan, value);
				}
				_actualClan = value;
			}
		}
	}

	public float RecentEventsMorale
	{
		get
		{
			return _moraleDueToEvents;
		}
		set
		{
			_moraleDueToEvents = value;
			if (_moraleDueToEvents < -50f)
			{
				_moraleDueToEvents = -50f;
			}
			else if (_moraleDueToEvents > 50f)
			{
				_moraleDueToEvents = 50f;
			}
		}
	}

	public float Morale
	{
		get
		{
			float resultNumber = Campaign.Current.Models.PartyMoraleModel.GetEffectivePartyMorale(this).ResultNumber;
			return (resultNumber < 0f) ? 0f : ((resultNumber > 100f) ? 100f : resultNumber);
		}
	}

	public ExplainedNumber MoraleExplained => Campaign.Current.Models.PartyMoraleModel.GetEffectivePartyMorale(this, includeDescription: true);

	int ILocatable<MobileParty>.LocatorNodeIndex
	{
		get
		{
			return _locatorNodeIndex;
		}
		set
		{
			_locatorNodeIndex = value;
		}
	}

	[CachedData]
	public PathFaceRecord CurrentNavigationFace
	{
		get
		{
			return _currentNavigationFace;
		}
		private set
		{
			_lastNavigationFace = CurrentNavigationFace;
			_currentNavigationFace = value;
		}
	}

	[CachedData]
	public Vec2 ErrorPosition { get; private set; }

	public Vec2 EventPositionAdder
	{
		get
		{
			return _eventPositionAdder;
		}
		set
		{
			ErrorPosition += _eventPositionAdder;
			_eventPositionAdder = value;
			ErrorPosition -= _eventPositionAdder;
		}
	}

	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			if (_isVisible != value)
			{
				_isVisible = value;
				Party.OnVisibilityChanged(value);
			}
		}
	}

	public Vec2 Position2D
	{
		get
		{
			return _position2D;
		}
		set
		{
			if (_position2D != value)
			{
				_position2D = value;
				Campaign current = Campaign.Current;
				current.MobilePartyLocator.UpdateLocator(this);
				if (current.MapSceneWrapper != null)
				{
					CurrentNavigationFace = current.MapSceneWrapper.GetFaceIndex(_position2D);
				}
			}
		}
	}

	public bool IsInspected
	{
		get
		{
			if (Army == null || Army != MainParty.Army)
			{
				return _isInspected;
			}
			return true;
		}
		set
		{
			_isInspected = value;
		}
	}

	public Vec2 GetPosition2D => Position2D;

	public int TotalWage => (int)Campaign.Current.Models.PartyWageModel.GetTotalWage(this).ResultNumber;

	public ExplainedNumber TotalWageExplained => Campaign.Current.Models.PartyWageModel.GetTotalWage(this, includeDescriptions: true);

	public MapEvent MapEvent => Party.MapEvent;

	public TroopRoster MemberRoster => Party.MemberRoster;

	public TroopRoster PrisonRoster => Party.PrisonRoster;

	public ItemRoster ItemRoster => Party.ItemRoster;

	public bool IsMainParty => this == MainParty;

	public IFaction MapFaction
	{
		get
		{
			if (ActualClan != null)
			{
				return ActualClan.MapFaction;
			}
			if (Party.Owner != null)
			{
				if (Party.Owner == Hero.MainHero)
				{
					return Party.Owner.MapFaction;
				}
				if (Party.Owner.IsNotable)
				{
					return Party.Owner.HomeSettlement.MapFaction;
				}
				if ((IsMilitia || IsGarrison || IsVillager) && HomeSettlement?.OwnerClan != null)
				{
					return HomeSettlement.OwnerClan.MapFaction;
				}
				if (IsCaravan || IsBanditBossParty)
				{
					return Party.Owner.MapFaction;
				}
				if (_isCurrentlyUsedByAQuest && Party.Owner != null)
				{
					return Party.Owner.MapFaction;
				}
				return (LeaderHero != null) ? LeaderHero.MapFaction : null;
			}
			if (HomeSettlement != null)
			{
				return HomeSettlement.OwnerClan.MapFaction;
			}
			return (LeaderHero != null) ? LeaderHero.MapFaction : null;
		}
	}

	public TextObject ArmyName
	{
		get
		{
			if (Army == null || Army.LeaderParty != this)
			{
				return Name;
			}
			return Army.Name;
		}
	}

	public SiegeEvent SiegeEvent => BesiegerCamp?.SiegeEvent;

	public float Food => (float)Party.RemainingFoodPercentage * 0.01f + (float)TotalFoodAtInventory;

	public int TotalFoodAtInventory => ItemRoster.TotalFood;

	public float TotalWeightCarried => ItemRoster.TotalWeight;

	public float SeeingRange => Campaign.Current.Models.MapVisibilityModel.GetPartySpottingRange(this).ResultNumber;

	public Settlement BesiegedSettlement => BesiegerCamp?.SiegeEvent.BesiegedSettlement;

	public bool IsEngaging
	{
		get
		{
			if (DefaultBehavior != AiBehavior.EngageParty)
			{
				return ShortTermBehavior == AiBehavior.EngageParty;
			}
			return true;
		}
	}

	internal bool IsCurrentlyEngagingSettlement
	{
		get
		{
			if (ShortTermBehavior != AiBehavior.GoToSettlement && ShortTermBehavior != AiBehavior.RaidSettlement)
			{
				return ShortTermBehavior == AiBehavior.AssaultSettlement;
			}
			return true;
		}
	}

	internal bool IsCurrentlyEngagingParty => ShortTermBehavior == AiBehavior.EngageParty;

	public bool IsCurrentlyGoingToSettlement => ShortTermBehavior == AiBehavior.GoToSettlement;

	public float PartySizeRatio
	{
		get
		{
			int versionNo = Party.MemberRoster.VersionNo;
			float cachedPartySizeRatio = _cachedPartySizeRatio;
			if (_partySizeRatioLastCheckVersion != versionNo || this == MainParty)
			{
				_partySizeRatioLastCheckVersion = versionNo;
				_cachedPartySizeRatio = (float)Party.NumberOfAllMembers / (float)Party.PartySizeLimit;
				cachedPartySizeRatio = _cachedPartySizeRatio;
			}
			return cachedPartySizeRatio;
		}
	}

	public int LimitedPartySize
	{
		get
		{
			if (HasLimitedWage())
			{
				int paymentLimit = Party.MobileParty.PaymentLimit;
				if (_latestUsedPaymentRatio != paymentLimit || this == MainParty)
				{
					_latestUsedPaymentRatio = paymentLimit;
					int num = Math.Min((LeaderHero != null && Party.Owner != null && Party.Owner.Clan != null && LeaderHero != Party.Owner.Clan.Leader) ? LeaderHero.CharacterObject.TroopWage : 0, TotalWage);
					int a = (int)((float)(PaymentLimit - num) / Campaign.Current.AverageWage) + 1;
					return _cachedPartySizeLimit = TaleWorlds.Library.MathF.Max(1, TaleWorlds.Library.MathF.Min(a, Party.PartySizeLimit));
				}
				return _cachedPartySizeLimit;
			}
			return Party.PartySizeLimit;
		}
	}

	public Vec2 VisualPosition2DWithoutError => Position2D + EventPositionAdder + ArmyPositionAdder;

	public bool IsMoving
	{
		get
		{
			if (IsMainParty)
			{
				return !Campaign.Current.IsMainPartyWaiting;
			}
			if (!(Position2D != TargetPosition))
			{
				if (MapEvent == null && BesiegedSettlement == null && CurrentSettlement == null)
				{
					return ShortTermBehavior != AiBehavior.Hold;
				}
				return false;
			}
			return true;
		}
	}

	public bool ShouldBeIgnored => _ignoredUntilTime.IsFuture;

	public float FoodChange
	{
		get
		{
			ExplainedNumber baseConsumption = Campaign.Current.Models.MobilePartyFoodConsumptionModel.CalculateDailyBaseFoodConsumptionf(this);
			return Campaign.Current.Models.MobilePartyFoodConsumptionModel.CalculateDailyFoodConsumptionf(this, baseConsumption).ResultNumber;
		}
	}

	public float BaseFoodChange => Campaign.Current.Models.MobilePartyFoodConsumptionModel.CalculateDailyBaseFoodConsumptionf(this).ResultNumber;

	public ExplainedNumber FoodChangeExplained
	{
		get
		{
			ExplainedNumber baseConsumption = Campaign.Current.Models.MobilePartyFoodConsumptionModel.CalculateDailyBaseFoodConsumptionf(this, includeDescription: true);
			return Campaign.Current.Models.MobilePartyFoodConsumptionModel.CalculateDailyFoodConsumptionf(this, baseConsumption);
		}
	}

	public float HealingRateForRegulars => Campaign.Current.Models.PartyHealingModel.GetDailyHealingForRegulars(this).ResultNumber;

	public ExplainedNumber HealingRateForRegularsExplained => Campaign.Current.Models.PartyHealingModel.GetDailyHealingForRegulars(this, includeDescriptions: true);

	public float HealingRateForHeroes => Campaign.Current.Models.PartyHealingModel.GetDailyHealingHpForHeroes(this).ResultNumber;

	public ExplainedNumber HealingRateForHeroesExplained => Campaign.Current.Models.PartyHealingModel.GetDailyHealingHpForHeroes(this, includeDescriptions: true);

	public ExplainedNumber SeeingRangeExplanation => Campaign.Current.Models.MapVisibilityModel.GetPartySpottingRange(this, includeDescriptions: true);

	public int InventoryCapacity => (int)Campaign.Current.Models.InventoryCapacityModel.CalculateInventoryCapacity(this).ResultNumber;

	public ExplainedNumber InventoryCapacityExplainedNumber => Campaign.Current.Models.InventoryCapacityModel.CalculateInventoryCapacity(this, includeDescriptions: true);

	public MapEventSide MapEventSide
	{
		get
		{
			return Party.MapEventSide;
		}
		set
		{
			Party.MapEventSide = value;
		}
	}

	bool IMapEntity.ShowCircleAroundEntity => CurrentSettlement == null;

	Vec2 IMapEntity.InteractionPosition => Position2D;

	bool IMapEntity.IsMobileEntity => true;

	public CaravanPartyComponent CaravanPartyComponent => _partyComponent as CaravanPartyComponent;

	public WarPartyComponent WarPartyComponent => _partyComponent as WarPartyComponent;

	public BanditPartyComponent BanditPartyComponent => _partyComponent as BanditPartyComponent;

	public LordPartyComponent LordPartyComponent => _partyComponent as LordPartyComponent;

	public PartyComponent PartyComponent
	{
		get
		{
			return _partyComponent;
		}
		set
		{
			if (_partyComponent != value)
			{
				if (_partyComponent != null)
				{
					_partyComponent.Finish();
				}
				Campaign.Current.CampaignObjectManager.BeforePartyComponentChanged(this);
				_partyComponent = value;
				UpdatePartyComponentFlags();
				Campaign.Current.CampaignObjectManager.AfterPartyComponentChanged(this);
				if (_partyComponent != null)
				{
					_partyComponent.Initialize(this);
				}
			}
		}
	}

	[CachedData]
	public bool IsMilitia { get; private set; }

	[CachedData]
	public bool IsLordParty { get; private set; }

	[CachedData]
	public bool IsVillager { get; private set; }

	[CachedData]
	public bool IsCaravan { get; private set; }

	[CachedData]
	public bool IsGarrison { get; private set; }

	[CachedData]
	public bool IsCustomParty { get; private set; }

	[CachedData]
	public bool IsBandit { get; private set; }

	public bool IsBanditBossParty
	{
		get
		{
			if (IsBandit)
			{
				return BanditPartyComponent.IsBossParty;
			}
			return false;
		}
	}

	public bool AvoidHostileActions
	{
		get
		{
			if (_partyComponent != null)
			{
				return _partyComponent.AvoidHostileActions;
			}
			return false;
		}
	}

	internal static void AutoGeneratedStaticCollectObjectsMobileParty(object o, List<object> collectedObjects)
	{
		((MobileParty)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_currentSettlement);
		collectedObjects.Add(_attachedTo);
		CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime(_disorganizedUntilTime, collectedObjects);
		collectedObjects.Add(_besiegerCamp);
		collectedObjects.Add(_targetParty);
		collectedObjects.Add(_targetSettlement);
		collectedObjects.Add(_customHomeSettlement);
		collectedObjects.Add(_army);
		CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime(_ignoredUntilTime, collectedObjects);
		collectedObjects.Add(_actualClan);
		collectedObjects.Add(_partyComponent);
		collectedObjects.Add(CustomName);
		collectedObjects.Add(LastVisitedSettlement);
		collectedObjects.Add(Ai);
		collectedObjects.Add(Party);
		CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime(StationaryStartTime, collectedObjects);
		collectedObjects.Add(Scout);
		collectedObjects.Add(Engineer);
		collectedObjects.Add(Quartermaster);
		collectedObjects.Add(Surgeon);
	}

	internal static object AutoGeneratedGetMemberValueCustomName(object o)
	{
		return ((MobileParty)o).CustomName;
	}

	internal static object AutoGeneratedGetMemberValueLastVisitedSettlement(object o)
	{
		return ((MobileParty)o).LastVisitedSettlement;
	}

	internal static object AutoGeneratedGetMemberValueBearing(object o)
	{
		return ((MobileParty)o).Bearing;
	}

	internal static object AutoGeneratedGetMemberValueAggressiveness(object o)
	{
		return ((MobileParty)o).Aggressiveness;
	}

	internal static object AutoGeneratedGetMemberValueArmyPositionAdder(object o)
	{
		return ((MobileParty)o).ArmyPositionAdder;
	}

	internal static object AutoGeneratedGetMemberValueObjective(object o)
	{
		return ((MobileParty)o).Objective;
	}

	internal static object AutoGeneratedGetMemberValueAi(object o)
	{
		return ((MobileParty)o).Ai;
	}

	internal static object AutoGeneratedGetMemberValueParty(object o)
	{
		return ((MobileParty)o).Party;
	}

	internal static object AutoGeneratedGetMemberValueIsActive(object o)
	{
		return ((MobileParty)o).IsActive;
	}

	internal static object AutoGeneratedGetMemberValueShortTermBehavior(object o)
	{
		return ((MobileParty)o).ShortTermBehavior;
	}

	internal static object AutoGeneratedGetMemberValueIsPartyTradeActive(object o)
	{
		return ((MobileParty)o).IsPartyTradeActive;
	}

	internal static object AutoGeneratedGetMemberValuePartyTradeTaxGold(object o)
	{
		return ((MobileParty)o).PartyTradeTaxGold;
	}

	internal static object AutoGeneratedGetMemberValueStationaryStartTime(object o)
	{
		return ((MobileParty)o).StationaryStartTime;
	}

	internal static object AutoGeneratedGetMemberValueShouldJoinPlayerBattles(object o)
	{
		return ((MobileParty)o).ShouldJoinPlayerBattles;
	}

	internal static object AutoGeneratedGetMemberValueIsDisbanding(object o)
	{
		return ((MobileParty)o).IsDisbanding;
	}

	internal static object AutoGeneratedGetMemberValueScout(object o)
	{
		return ((MobileParty)o).Scout;
	}

	internal static object AutoGeneratedGetMemberValueEngineer(object o)
	{
		return ((MobileParty)o).Engineer;
	}

	internal static object AutoGeneratedGetMemberValueQuartermaster(object o)
	{
		return ((MobileParty)o).Quartermaster;
	}

	internal static object AutoGeneratedGetMemberValueSurgeon(object o)
	{
		return ((MobileParty)o).Surgeon;
	}

	internal static object AutoGeneratedGetMemberValueHasUnpaidWages(object o)
	{
		return ((MobileParty)o).HasUnpaidWages;
	}

	internal static object AutoGeneratedGetMemberValueAverageFleeTargetDirection(object o)
	{
		return ((MobileParty)o).AverageFleeTargetDirection;
	}

	internal static object AutoGeneratedGetMemberValue_currentSettlement(object o)
	{
		return ((MobileParty)o)._currentSettlement;
	}

	internal static object AutoGeneratedGetMemberValue_attachedTo(object o)
	{
		return ((MobileParty)o)._attachedTo;
	}

	internal static object AutoGeneratedGetMemberValue_eventPositionAdder(object o)
	{
		return ((MobileParty)o)._eventPositionAdder;
	}

	internal static object AutoGeneratedGetMemberValue_position2D(object o)
	{
		return ((MobileParty)o)._position2D;
	}

	internal static object AutoGeneratedGetMemberValue_isVisible(object o)
	{
		return ((MobileParty)o)._isVisible;
	}

	internal static object AutoGeneratedGetMemberValue_isInspected(object o)
	{
		return ((MobileParty)o)._isInspected;
	}

	internal static object AutoGeneratedGetMemberValue_disorganizedUntilTime(object o)
	{
		return ((MobileParty)o)._disorganizedUntilTime;
	}

	internal static object AutoGeneratedGetMemberValue_besiegerCamp(object o)
	{
		return ((MobileParty)o)._besiegerCamp;
	}

	internal static object AutoGeneratedGetMemberValue_targetParty(object o)
	{
		return ((MobileParty)o)._targetParty;
	}

	internal static object AutoGeneratedGetMemberValue_targetSettlement(object o)
	{
		return ((MobileParty)o)._targetSettlement;
	}

	internal static object AutoGeneratedGetMemberValue_targetPosition(object o)
	{
		return ((MobileParty)o)._targetPosition;
	}

	internal static object AutoGeneratedGetMemberValue_customHomeSettlement(object o)
	{
		return ((MobileParty)o)._customHomeSettlement;
	}

	internal static object AutoGeneratedGetMemberValue_army(object o)
	{
		return ((MobileParty)o)._army;
	}

	internal static object AutoGeneratedGetMemberValue_isCurrentlyUsedByAQuest(object o)
	{
		return ((MobileParty)o)._isCurrentlyUsedByAQuest;
	}

	internal static object AutoGeneratedGetMemberValue_partyTradeGold(object o)
	{
		return ((MobileParty)o)._partyTradeGold;
	}

	internal static object AutoGeneratedGetMemberValue_ignoredUntilTime(object o)
	{
		return ((MobileParty)o)._ignoredUntilTime;
	}

	internal static object AutoGeneratedGetMemberValue_actualClan(object o)
	{
		return ((MobileParty)o)._actualClan;
	}

	internal static object AutoGeneratedGetMemberValue_moraleDueToEvents(object o)
	{
		return ((MobileParty)o)._moraleDueToEvents;
	}

	internal static object AutoGeneratedGetMemberValue_partyComponent(object o)
	{
		return ((MobileParty)o)._partyComponent;
	}

	public bool HasLimitedWage()
	{
		return PaymentLimit != Campaign.Current.Models.PartyWageModel.MaxWage;
	}

	public bool CanPayMoreWage()
	{
		if (HasLimitedWage())
		{
			return PaymentLimit > TotalWage;
		}
		return true;
	}

	public bool IsWageLimitExceeded()
	{
		if (HasLimitedWage())
		{
			return PaymentLimit < TotalWage;
		}
		return false;
	}

	public void SetWagePaymentLimit(int newLimit)
	{
		PartyComponent?.SetWagePaymentLimit(newLimit);
	}

	public void SetCustomHomeSettlement(Settlement customHomeSettlement)
	{
		_customHomeSettlement = customHomeSettlement;
	}

	private void SetAttachedToInternal(MobileParty value)
	{
		if (_attachedTo != null)
		{
			_attachedTo.RemoveAttachedPartyInternal(this);
			if (Party.MapEventSide != null)
			{
				Party.MapEventSide.HandleMapEventEndForPartyInternal(Party);
				Party.MapEventSide = null;
			}
			if (BesiegerCamp != null)
			{
				BesiegerCamp = null;
			}
			OnAttachedToRemoved();
		}
		_attachedTo = value;
		if (_attachedTo != null)
		{
			_attachedTo.AddAttachedPartyInternal(this);
			Party.MapEventSide = _attachedTo.Party.MapEventSide;
			BesiegerCamp = _attachedTo.BesiegerCamp;
			CurrentSettlement = _attachedTo.CurrentSettlement;
		}
		Party.SetVisualAsDirty();
	}

	private void AddAttachedPartyInternal(MobileParty mobileParty)
	{
		if (_attachedParties == null)
		{
			_attachedParties = new MBList<MobileParty>();
		}
		_attachedParties.Add(mobileParty);
		if (CampaignEventDispatcher.Instance != null)
		{
			CampaignEventDispatcher.Instance.OnPartyAttachedAnotherParty(mobileParty);
		}
	}

	private void RemoveAttachedPartyInternal(MobileParty mobileParty)
	{
		_attachedParties.Remove(mobileParty);
	}

	private void OnAttachedToRemoved()
	{
		ErrorPosition += ArmyPositionAdder;
		ArmyPositionAdder = Vec2.Zero;
		if (!IsVisible)
		{
			ErrorPosition = Vec2.Zero;
		}
		if (CurrentSettlement != null)
		{
			Ai.SetMoveGoToSettlement(CurrentSettlement);
		}
		else
		{
			Ai.SetMoveModeHold();
		}
	}

	public MobileParty()
	{
		_isVisible = false;
		IsActive = true;
		_isCurrentlyUsedByAQuest = false;
		Party = new PartyBase(this);
		InitMembers();
		InitCached();
		Initialize();
	}

	private void InitMembers()
	{
		if (_attachedParties == null)
		{
			_attachedParties = new MBList<MobileParty>();
		}
	}

	public void SetPartyScout(Hero hero)
	{
		RemoveHeroPerkRole(hero);
		Scout = hero;
	}

	public void SetPartyQuartermaster(Hero hero)
	{
		RemoveHeroPerkRole(hero);
		Quartermaster = hero;
	}

	public void SetPartyEngineer(Hero hero)
	{
		RemoveHeroPerkRole(hero);
		Engineer = hero;
	}

	public void SetPartySurgeon(Hero hero)
	{
		RemoveHeroPerkRole(hero);
		Surgeon = hero;
	}

	private void InitializeMobilePartyWithPartyTemplate(PartyTemplateObject pt, Vec2 position, int troopNumberLimit)
	{
		if (troopNumberLimit != 0)
		{
			FillPartyStacks(pt, troopNumberLimit);
		}
		CreateFigure(position, 0f);
		Ai.SetMoveModeHold();
	}

	public void InitializeMobilePartyAroundPosition(TroopRoster memberRoster, TroopRoster prisonerRoster, Vec2 position, float spawnRadius, float minSpawnRadius = 0f)
	{
		position = MobilePartyHelper.FindReachablePointAroundPosition(position, spawnRadius, minSpawnRadius);
		InitializeMobilePartyWithRosterInternal(memberRoster, prisonerRoster, position);
	}

	public override void Initialize()
	{
		base.Initialize();
		Aggressiveness = 1f;
		Ai = new MobilePartyAi(this);
		CampaignEventDispatcher.Instance.OnPartyVisibilityChanged(Party);
	}

	public void InitializeMobilePartyAtPosition(TroopRoster memberRoster, TroopRoster prisonerRoster, Vec2 position)
	{
		InitializeMobilePartyWithRosterInternal(memberRoster, prisonerRoster, position);
	}

	public void InitializeMobilePartyAtPosition(PartyTemplateObject pt, Vec2 position, int troopNumberLimit = -1)
	{
		InitializeMobilePartyWithPartyTemplate(pt, position, troopNumberLimit);
	}

	public void InitializeMobilePartyAroundPosition(PartyTemplateObject pt, Vec2 position, float spawnRadius, float minSpawnRadius = 0f, int troopNumberLimit = -1)
	{
		position = MobilePartyHelper.FindReachablePointAroundPosition(position, spawnRadius, minSpawnRadius);
		InitializeMobilePartyWithPartyTemplate(pt, position, troopNumberLimit);
	}

	private void InitializeMobilePartyWithRosterInternal(TroopRoster memberRoster, TroopRoster prisonerRoster, Vec2 position)
	{
		MemberRoster.Add(memberRoster);
		PrisonRoster.Add(prisonerRoster);
		CreateFigure(position, 0f);
		Ai.SetMoveModeHold();
	}

	internal void StartUp()
	{
		Ai.StartUp();
	}

	[LateLoadInitializationCallback]
	private void OnLateLoad(MetaData metaData, ObjectLoadData objectLoadData)
	{
		if (!(MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.1.0")))
		{
			return;
		}
		PartyBase partyBase = (PartyBase)objectLoadData.GetMemberValueBySaveId(1052);
		IMapEntity mapEntity = null;
		if (partyBase != null)
		{
			if (partyBase.IsSettlement)
			{
				mapEntity = partyBase.Settlement;
			}
			else if (partyBase.IsMobile)
			{
				mapEntity = partyBase.MobileParty;
			}
		}
		object memberValueBySaveId = objectLoadData.GetMemberValueBySaveId(1036);
		object memberValueBySaveId2 = objectLoadData.GetMemberValueBySaveId(1037);
		object memberValueBySaveId3 = objectLoadData.GetMemberValueBySaveId(1064);
		object memberValueBySaveId4 = objectLoadData.GetMemberValueBySaveId(1065);
		object memberValueBySaveId5 = objectLoadData.GetMemberValueBySaveId(1047);
		object memberValueBySaveId6 = objectLoadData.GetMemberValueBySaveId(1051);
		object memberValueBySaveId7 = objectLoadData.GetMemberValueBySaveId(1038);
		object memberValueBySaveId8 = objectLoadData.GetMemberValueBySaveId(1039);
		object memberValueBySaveId9 = objectLoadData.GetMemberValueBySaveId(1055);
		object memberValueBySaveId10 = objectLoadData.GetMemberValueBySaveId(1054);
		object memberValueBySaveId11 = objectLoadData.GetMemberValueBySaveId(1062);
		object memberValueBySaveId12 = objectLoadData.GetMemberValueBySaveId(1061);
		object fieldValueBySaveId = objectLoadData.GetFieldValueBySaveId(1070);
		object memberValueBySaveId13 = objectLoadData.GetMemberValueBySaveId(1022);
		object obj = mapEntity ?? objectLoadData.GetMemberValueBySaveId(1056);
		object memberValueBySaveId14 = objectLoadData.GetMemberValueBySaveId(1074);
		if (memberValueBySaveId != null)
		{
			Ai.InitializeForOldSaves((float)memberValueBySaveId, (float)memberValueBySaveId2, (CampaignTime)memberValueBySaveId3, (int)memberValueBySaveId4, (AiBehavior)memberValueBySaveId5, (Vec2)memberValueBySaveId6, (bool)memberValueBySaveId7, (bool)memberValueBySaveId8, (MoveModeType)memberValueBySaveId9, (MobileParty)memberValueBySaveId10, (Vec2)memberValueBySaveId11, (Vec2)memberValueBySaveId12, (Vec2)fieldValueBySaveId, (Vec2)memberValueBySaveId13, (IMapEntity)obj, ((CampaignTime?)memberValueBySaveId14) ?? CampaignTime.Never);
		}
		UpdatePartyComponentFlags();
		if (IsGarrison || IsLordParty)
		{
			object memberValueBySaveId15 = objectLoadData.GetMemberValueBySaveId(1010);
			if (memberValueBySaveId15 != null)
			{
				SetWagePaymentLimit((int)memberValueBySaveId15);
			}
		}
	}

	public override string ToString()
	{
		return base.StringId + ":" + Party.Index;
	}

	TextObject ITrackableBase.GetName()
	{
		return Name;
	}

	public void ValidateSpeed()
	{
		CalculateSpeed();
	}

	public void ChangePartyLeader(Hero newLeader)
	{
		if (newLeader == null || !MemberRoster.Contains(newLeader.CharacterObject))
		{
			Debug.FailedAssert(string.Concat(newLeader?.Name, " is not a member of ", Name, "!\nParty leader did not change."), "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Party\\MobileParty.cs", "ChangePartyLeader", 851);
		}
		else
		{
			if (IsLordParty)
			{
				(_partyComponent as LordPartyComponent)?.ChangePartyOwner(newLeader);
			}
			PartyComponent.ChangePartyLeader(newLeader);
		}
	}

	public void RemovePartyLeader()
	{
		if (LeaderHero != null)
		{
			if (MapEvent == null)
			{
				Ai.SetMoveModeHold();
			}
			PartyComponent.ChangePartyLeader(null);
		}
	}

	private void RecoverPositionsForNavMeshUpdate()
	{
		if (Position2D.IsNonZero() && !PartyBase.IsPositionOkForTraveling(Position2D))
		{
			Debug.Print("Position of " + base.StringId + " is not valid! (" + Position2D.x + ", " + Position2D.y + ") Party will be moved to a valid position.");
			Position2D = CurrentSettlement?.GatePosition ?? SettlementHelper.FindNearestVillage(null, this).GatePosition;
		}
		if (CurrentSettlement != null)
		{
			float epsilon = (CurrentSettlement.IsFortification ? Campaign.Current.Models.EncounterModel.NeededMaximumDistanceForEncounteringTown : Campaign.Current.Models.EncounterModel.NeededMaximumDistanceForEncounteringVillage);
			if (!CurrentSettlement.GatePosition.NearlyEquals(Position2D, epsilon))
			{
				Debug.Print("Position of " + base.StringId + " is not valid! (" + Position2D.x + ", " + Position2D.y + ") Party will be moved to a valid position.");
				Position2D = CurrentSettlement.GatePosition;
			}
		}
		Ai.RecoverPositionsForNavMeshUpdate();
	}

	public void OnGameInitialized()
	{
		RecoverPositionsForNavMeshUpdate();
		Campaign current = Campaign.Current;
		if (current.MapSceneWrapper != null)
		{
			CurrentNavigationFace = current.MapSceneWrapper.GetFaceIndex(Position2D);
		}
		Ai.OnGameInitialized();
		MobileParty mobileParty = Campaign.Current.CampaignObjectManager.Find((MobileParty x) => x.StringId == base.StringId);
		if (this != mobileParty)
		{
			DestroyPartyAction.Apply(null, this);
		}
	}

	[LoadInitializationCallback]
	private void OnLoad(MetaData metaData, ObjectLoadData objectLoadData)
	{
		object memberValueBySaveId = objectLoadData.GetMemberValueBySaveId(1032);
		if (memberValueBySaveId != null)
		{
			_doNotAttackMainParty = (int)memberValueBySaveId;
		}
	}

	protected override void PreAfterLoad()
	{
		UpdatePartyComponentFlags();
		PartyComponent?.Initialize(this);
		Ai.PreAfterLoad();
		if (_disorganizedUntilTime.IsFuture)
		{
			_isDisorganized = true;
		}
		if (MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion.IsOlderThan(ApplicationVersion.FromString("v1.2.11.45697")) && ((LeaderHero != null && this != MainParty && LeaderHero.PartyBelongedTo != this) || (MapEvent == null && base.StringId.Contains("troops_of_"))))
		{
			DestroyPartyAction.Apply(null, this);
		}
	}

	protected override void OnBeforeLoad()
	{
		Ai.OnBeforeLoad();
		InitMembers();
		InitCached();
		_attachedTo?.AddAttachedPartyInternal(this);
	}

	private void InitCached()
	{
		Ai?.InitCached();
		((ILocatable<MobileParty>)this).LocatorNodeIndex = -1;
		ThinkParamsCache = new PartyThinkParams(this);
		ResetCached();
	}

	private void ResetCached()
	{
		_partySizeRatioLastCheckVersion = -1;
		_latestUsedPaymentRatio = -1;
		_cachedPartySizeRatio = 1f;
		VersionNo = 0;
		_partyPureSpeedLastCheckVersion = -1;
		_itemRosterVersionNo = -1;
		Party.InitCache();
	}

	protected override void AfterLoad()
	{
		Party.AfterLoad();
		if (IsGarrison && MapEvent == null && SiegeEvent == null && TargetParty != null && CurrentSettlement != null)
		{
			Ai.SetMoveModeHold();
		}
		if (CurrentSettlement != null && !CurrentSettlement.Parties.Contains(this))
		{
			CurrentSettlement.AddMobileParty(this);
			foreach (MobileParty attachedParty in _attachedParties)
			{
				if (Army.LeaderParty != this)
				{
					CurrentSettlement.AddMobileParty(attachedParty);
				}
			}
		}
		if (_doNotAttackMainParty > 0)
		{
			Ai.DoNotAttackMainPartyUntil = CampaignTime.HoursFromNow(_doNotAttackMainParty);
		}
		if (IsCaravan && Army != null)
		{
			Army = null;
		}
		if (MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.1.0") && (PaymentLimit == 2000 || (this == MainParty && PaymentLimit == 0)))
		{
			SetWagePaymentLimit(Campaign.Current.Models.PartyWageModel.MaxWage);
		}
		if (MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.2.0") && IsCaravan && Owner == Hero.MainHero && ActualClan == null)
		{
			ActualClan = Owner.Clan;
		}
		if (!MBSaveLoad.IsUpdatingGameVersion || !(MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.1.4")))
		{
			return;
		}
		if (TargetParty != null)
		{
			IFaction mapFaction = TargetParty.MapFaction;
			if (mapFaction == null || !mapFaction.IsAtWarWith(MapFaction))
			{
				goto IL_025e;
			}
		}
		if (TargetSettlement != null)
		{
			IFaction mapFaction2 = TargetSettlement.MapFaction;
			if (mapFaction2 == null || !mapFaction2.IsAtWarWith(MapFaction))
			{
				goto IL_025e;
			}
		}
		if (ShortTermTargetParty != null)
		{
			MobileParty shortTermTargetParty = ShortTermTargetParty;
			if (shortTermTargetParty != null && shortTermTargetParty.MapFaction?.IsAtWarWith(MapFaction) == true)
			{
				return;
			}
			goto IL_025e;
		}
		return;
		IL_025e:
		Ai.SetMoveModeHold();
	}

	internal void OnFinishLoadState()
	{
		Campaign.Current.MobilePartyLocator.UpdateLocator(this);
	}

	internal void HourlyTick()
	{
		if (IsActive)
		{
			if (LeaderHero != null && CurrentSettlement != null && CurrentSettlement == LeaderHero.HomeSettlement)
			{
				LeaderHero.PassedTimeAtHomeSettlement++;
			}
			Ai.HourlyTick();
		}
	}

	internal void DailyTick()
	{
		RecentEventsMorale -= RecentEventsMorale * 0.1f;
		if (LeaderHero != null)
		{
			LeaderHero.PassedTimeAtHomeSettlement *= 0.9f;
		}
	}

	internal void TickForStationaryMobileParty(ref CachedPartyVariables variables, float dt, float realDt)
	{
		if (StationaryStartTime == CampaignTime.Never)
		{
			StationaryStartTime = CampaignTime.Now;
		}
		CheckIsDisorganized();
		DoUpdatePosition(ref variables, dt, realDt);
		DoErrorCorrections(ref variables, realDt);
	}

	internal void TickForMovingMobileParty(ref CachedPartyVariables variables, float dt, float realDt)
	{
		ComputeNextMoveDistance(ref variables, dt);
		CommonMovingPartyTick(ref variables, dt, realDt);
	}

	internal void TickForMovingArmyLeader(ref CachedPartyVariables variables, float dt, float realDt)
	{
		ComputeNextMoveDistanceForArmyLeader(ref variables, dt);
		CommonMovingPartyTick(ref variables, dt, realDt);
	}

	internal void CommonMovingPartyTick(ref CachedPartyVariables variables, float dt, float realDt)
	{
		StationaryStartTime = CampaignTime.Never;
		CheckIsDisorganized();
		Ai.DoAiPathMode(ref variables);
		DoUpdatePosition(ref variables, dt, realDt);
		DoErrorCorrections(ref variables, realDt);
	}

	internal void InitializeCachedPartyVariables(ref CachedPartyVariables variables)
	{
		variables.HasMapEvent = MapEvent != null;
		variables.CurrentPosition = Position2D;
		variables.TargetPartyPositionAtFrameStart = Vec2.Invalid;
		variables.LastCurrentPosition = Position2D;
		variables.IsAttachedArmyMember = false;
		variables.IsMoving = IsMoving || IsMainParty;
		variables.IsArmyLeader = false;
		if (Army != null)
		{
			if (Army.LeaderParty == this)
			{
				variables.IsArmyLeader = true;
			}
			else if (Army.LeaderParty.AttachedParties.Contains(this))
			{
				variables.IsAttachedArmyMember = true;
				variables.IsMoving = IsMoving || Army.LeaderParty.IsMoving;
			}
		}
	}

	internal void ComputeNextMoveDistanceForArmyLeader(ref CachedPartyVariables variables, float dt)
	{
		if (dt > 0f)
		{
			CalculateSpeedForPartyUnified();
			variables.NextMoveDistance = Speed * dt;
		}
		else
		{
			variables.NextMoveDistance = 0f;
		}
	}

	internal void ComputeNextMoveDistance(ref CachedPartyVariables variables, float dt)
	{
		if (dt > 0f)
		{
			CalculateSpeed();
			variables.NextMoveDistance = Speed * dt;
		}
		else
		{
			variables.NextMoveDistance = 0f;
		}
	}

	internal void UpdateStationaryTimer()
	{
		if (!IsMoving)
		{
			if (StationaryStartTime == CampaignTime.Never)
			{
				StationaryStartTime = CampaignTime.Now;
			}
		}
		else
		{
			StationaryStartTime = CampaignTime.Never;
		}
	}

	private void CheckIsDisorganized()
	{
		if (_isDisorganized && _disorganizedUntilTime.IsPast)
		{
			SetDisorganized(isDisorganized: false);
		}
	}

	public void SetDisorganized(bool isDisorganized)
	{
		if (isDisorganized)
		{
			_disorganizedUntilTime = CampaignTime.HoursFromNow(Campaign.Current.Models.PartyImpairmentModel.GetDisorganizedStateDuration(this));
		}
		_isDisorganized = isDisorganized;
		UpdateVersionNo();
	}

	internal void DoUpdatePosition(ref CachedPartyVariables variables, float dt, float realDt)
	{
		variables.NextPosition = variables.CurrentPosition;
		Vec2 vec = variables.CurrentPosition + EventPositionAdder + ArmyPositionAdder;
		Vec2 vec2;
		if (variables.IsAttachedArmyMember)
		{
			if (variables.HasMapEvent || CurrentSettlement != null)
			{
				vec2 = Vec2.Zero;
			}
			else
			{
				Vec2 vec3 = (variables.HasMapEvent ? Army.LeaderParty.Position2D : Army.LeaderParty.Ai.NextTargetPosition);
				Army.LeaderParty.Ai.GetTargetPositionAndFace(ref variables, out var finalTargetPosition, out var _, out var _);
				Vec2 armyFacing = (((vec3 - Army.LeaderParty.Position2D).LengthSquared < 0.0025000002f) ? Vec2.FromRotation(Army.LeaderParty.Party.AverageBearingRotation) : (vec3 - Army.LeaderParty.Position2D).Normalized());
				Vec2 vec4 = armyFacing.TransformToParentUnitF(Army.GetRelativePositionForParty(this, armyFacing));
				vec2 = vec3 + vec4 - vec;
				if ((finalTargetPosition + vec4 - vec).LengthSquared < 0.010000001f || vec2.LengthSquared < 0.010000001f)
				{
					vec2 = Vec2.Zero;
				}
				float num = vec2.LeftVec().Normalized().DotProduct(Army.LeaderParty.Position2D + vec4 - vec);
				vec2.RotateCCW((num < 0f) ? TaleWorlds.Library.MathF.Max(num * 2f, -System.MathF.PI / 4f) : TaleWorlds.Library.MathF.Min(num * 2f, System.MathF.PI / 4f));
			}
		}
		else
		{
			vec2 = (variables.HasMapEvent ? Party.MapEvent.Position : Ai.NextTargetPosition) - vec;
		}
		float num2 = vec2.Normalize();
		if (num2 < variables.NextMoveDistance)
		{
			variables.NextMoveDistance = num2;
		}
		if (BesiegedSettlement != null || CurrentSettlement != null || (!(variables.NextMoveDistance > 0f) && !variables.HasMapEvent))
		{
			return;
		}
		bool flag = false;
		Vec2 vec5 = Bearing;
		if (num2 > 0f)
		{
			flag = true;
			vec5 = vec2;
			if (!variables.IsAttachedArmyMember || !variables.HasMapEvent)
			{
				Bearing = vec5;
			}
		}
		else if (variables.IsAttachedArmyMember && variables.HasMapEvent)
		{
			vec5 = (Bearing = Army.LeaderParty.Bearing);
			flag = true;
		}
		if (flag)
		{
			float num3 = MBMath.WrapAngle(Bearing.RotationInRadians - Party.AverageBearingRotation);
			float num4 = (variables.HasMapEvent ? realDt : dt);
			Party.AverageBearingRotation += num3 * TaleWorlds.Library.MathF.Min(num4 * 30f, 1f);
			Party.AverageBearingRotation = MBMath.WrapAngle(Party.AverageBearingRotation);
		}
		variables.NextPosition = variables.CurrentPosition + vec5 * variables.NextMoveDistance;
	}

	internal void DoErrorCorrections(ref CachedPartyVariables variables, float realDt)
	{
		float lengthSquared = ErrorPosition.LengthSquared;
		if (lengthSquared > 0f)
		{
			if (CurrentSettlement != null || !IsVisible)
			{
				ErrorPosition = Vec2.Zero;
			}
			if ((double)lengthSquared <= 49.0 * (double)realDt * (double)realDt)
			{
				ErrorPosition = Vec2.Zero;
			}
			else
			{
				ErrorPosition -= ErrorPosition.Normalized() * (7f * realDt);
			}
		}
	}

	internal void TickForMobileParty2(ref CachedPartyVariables variables, float realDt, ref int gridChangeCount, ref MobileParty[] gridChangeMobilePartyList)
	{
		variables.NextPathFaceRecord = Campaign.Current.MapSceneWrapper.GetFaceIndex(variables.NextPosition);
		if (!(variables.NextMoveDistance > 0f) || !variables.IsMoving || BesiegedSettlement != null || variables.HasMapEvent)
		{
			return;
		}
		if (variables.IsAttachedArmyMember && (Army.LeaderParty.Position2D - (Position2D + ArmyPositionAdder)).Length > 0.25f)
		{
			_position2D = Army.LeaderParty.Position2D;
			ArmyPositionAdder += variables.NextPosition - Position2D;
			return;
		}
		PathFaceRecord nextPathFaceRecord = variables.NextPathFaceRecord;
		if (CurrentNavigationFace.IsValid() && CurrentNavigationFace.FaceIslandIndex == nextPathFaceRecord.FaceIslandIndex)
		{
			SetPositionParallel(variables, variables.NextPosition, ref gridChangeCount, ref gridChangeMobilePartyList);
		}
	}

	private void SetPositionParallel(CachedPartyVariables variables, Vec2 value, ref int GridChangeCounter, ref MobileParty[] GridChangeList)
	{
		if (_position2D != value)
		{
			_position2D = value;
			if (!Campaign.Current.MobilePartyLocator.CheckWhetherPositionsAreInSameNode(value, this))
			{
				int num = Interlocked.Increment(ref GridChangeCounter);
				GridChangeList[num] = this;
			}
			CurrentNavigationFace = variables.NextPathFaceRecord;
		}
	}

	public void SetCustomName(TextObject name)
	{
		CustomName = name;
	}

	public void SetPartyUsedByQuest(bool isActivelyUsed)
	{
		if (_isCurrentlyUsedByAQuest != isActivelyUsed)
		{
			_isCurrentlyUsedByAQuest = isActivelyUsed;
			CampaignEventDispatcher.Instance.OnMobilePartyQuestStatusChanged(this, isActivelyUsed);
		}
	}

	public void ResetTargetParty()
	{
		TargetParty = null;
	}

	public void IgnoreForHours(float hours)
	{
		_ignoredUntilTime = CampaignTime.HoursFromNow(hours);
	}

	public void IgnoreByOtherPartiesTill(CampaignTime time)
	{
		_ignoredUntilTime = time;
	}

	internal void OnRemovedFromArmyInternal()
	{
		ResetTargetParty();
		if (!IsActive || LeaderHero == null)
		{
			return;
		}
		if (BesiegedSettlement != null && Army.LeaderParty != this)
		{
			if (BesiegedSettlement.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(Party))
			{
				return;
			}
			if (this != MainParty)
			{
				if (MapEvent == null)
				{
					Ai.SetMoveBesiegeSettlement(BesiegedSettlement);
				}
			}
			else
			{
				Ai.SetMoveModeHold();
			}
		}
		else if (CurrentSettlement == null)
		{
			Ai.SetMoveModeHold();
		}
		else if (Party.MapEvent == null)
		{
			Ai.SetMoveGoToSettlement(CurrentSettlement);
		}
	}

	private void OnRemoveParty()
	{
		Army = null;
		CurrentSettlement = null;
		AttachedTo = null;
		BesiegerCamp = null;
		List<Settlement> list = new List<Settlement>();
		if (CurrentSettlement != null)
		{
			list.Add(CurrentSettlement);
		}
		else if ((IsGarrison || IsMilitia || IsBandit || IsVillager) && HomeSettlement != null)
		{
			list.Add(HomeSettlement);
		}
		PartyComponent?.Finish();
		ActualClan = null;
		Campaign.Current.CampaignObjectManager.RemoveMobileParty(this);
		foreach (Settlement item in list)
		{
			item.SettlementComponent.OnRelatedPartyRemoved(this);
		}
	}

	public void SetPartyObjective(PartyObjective objective)
	{
		Objective = objective;
	}

	public void UpdateVersionNo()
	{
		VersionNo++;
	}

	private bool IsLastSpeedCacheInvalid()
	{
		Hero leaderHero = LeaderHero;
		bool flag = !IsActive || leaderHero == null || leaderHero.PartyBelongedToAsPrisoner != null;
		bool isNight = Campaign.Current.IsNight;
		if (_lastNavigationFace.FaceIndex == CurrentNavigationFace.FaceIndex && _partyLastCheckIsPrisoner == flag)
		{
			return _partyLastCheckAtNight != isNight;
		}
		return true;
	}

	private bool IsBaseSpeedCacheInvalid()
	{
		MapWeatherModel.WeatherEventEffectOnTerrain weatherEffectOnTerrainForPosition = Campaign.Current.Models.MapWeatherModel.GetWeatherEffectOnTerrainForPosition(Position2D);
		if (_partyPureSpeedLastCheckVersion == VersionNo && _itemRosterVersionNo == Party.ItemRoster.VersionNo)
		{
			return _lastWeatherTerrainEffect != weatherEffectOnTerrainForPosition;
		}
		return true;
	}

	private float CalculateSpeedForPartyUnified()
	{
		bool flag = false;
		if (IsBaseSpeedCacheInvalid())
		{
			if (Army != null && Army.LeaderParty.AttachedParties.Contains(this))
			{
				_lastCalculatedBaseSpeedExplained = Army.LeaderParty._lastCalculatedBaseSpeedExplained;
			}
			else
			{
				_lastCalculatedBaseSpeedExplained = Campaign.Current.Models.PartySpeedCalculatingModel.CalculateBaseSpeed(this);
			}
			MapWeatherModel.WeatherEventEffectOnTerrain weatherEffectOnTerrainForPosition = Campaign.Current.Models.MapWeatherModel.GetWeatherEffectOnTerrainForPosition(Position2D);
			_partyPureSpeedLastCheckVersion = VersionNo;
			_itemRosterVersionNo = Party.ItemRoster.VersionNo;
			_lastWeatherTerrainEffect = weatherEffectOnTerrainForPosition;
			flag = true;
		}
		if (flag)
		{
			_lastCalculatedSpeed = Campaign.Current.Models.PartySpeedCalculatingModel.CalculateFinalSpeed(this, _lastCalculatedBaseSpeedExplained).ResultNumber;
		}
		else if (IsLastSpeedCacheInvalid())
		{
			Hero leaderHero = LeaderHero;
			bool partyLastCheckIsPrisoner = !IsActive || leaderHero == null || leaderHero.PartyBelongedToAsPrisoner != null;
			bool isNight = Campaign.Current.IsNight;
			_lastNavigationFace = CurrentNavigationFace;
			_partyLastCheckIsPrisoner = partyLastCheckIsPrisoner;
			_partyLastCheckAtNight = isNight;
			_lastCalculatedSpeed = Campaign.Current.Models.PartySpeedCalculatingModel.CalculateFinalSpeed(this, _lastCalculatedBaseSpeedExplained).ResultNumber;
		}
		return _lastCalculatedSpeed;
	}

	private float CalculateSpeed()
	{
		if (Army != null)
		{
			if (Army.LeaderParty.AttachedParties.Contains(this))
			{
				Vec2 armyFacing = (((Army.LeaderParty.MapEvent != null) ? Army.LeaderParty.Position2D : Army.LeaderParty.Ai.NextTargetPosition) - Army.LeaderParty.Position2D).Normalized();
				Vec2 vec = Army.LeaderParty.Position2D + armyFacing.TransformToParentUnitF(Army.GetRelativePositionForParty(this, armyFacing));
				float num = Bearing.DotProduct(vec - VisualPosition2DWithoutError);
				return Army.LeaderParty._lastCalculatedSpeed * MBMath.ClampFloat(1f + num * 1f, 0.7f, 1.3f);
			}
		}
		else if (DefaultBehavior == AiBehavior.EscortParty && TargetParty != null && _lastCalculatedSpeed > TargetParty._lastCalculatedSpeed)
		{
			return TargetParty._lastCalculatedSpeed;
		}
		return CalculateSpeedForPartyUnified();
	}

	public bool IsSpotted()
	{
		return IsVisible;
	}

	public int AddElementToMemberRoster(CharacterObject element, int numberToAdd, bool insertAtFront = false)
	{
		return Party.AddElementToMemberRoster(element, numberToAdd, insertAtFront);
	}

	public int AddPrisoner(CharacterObject element, int numberToAdd)
	{
		return Party.AddPrisoner(element, numberToAdd);
	}

	public Vec3 GetLogicalPosition()
	{
		float height = 0f;
		Campaign.Current.MapSceneWrapper.GetHeightAtPoint(Position2D, ref height);
		return new Vec3(Position2D.x, Position2D.y, height);
	}

	public float GetTotalStrengthWithFollowers(bool includeNonAttachedArmyMembers = true)
	{
		MobileParty mobileParty = ((DefaultBehavior == AiBehavior.EscortParty) ? TargetParty : this);
		float num = mobileParty.Party.TotalStrength;
		if (mobileParty.Army != null && mobileParty == mobileParty.Army.LeaderParty)
		{
			foreach (MobileParty party in mobileParty.Army.Parties)
			{
				if (party.Army.LeaderParty != party && (party.AttachedTo != null || includeNonAttachedArmyMembers))
				{
					num += party.Party.TotalStrength;
				}
			}
		}
		return num;
	}

	private void FillPartyStacks(PartyTemplateObject pt, int troopNumberLimit = -1)
	{
		if (IsBandit)
		{
			float playerProgress = Campaign.Current.PlayerProgress;
			float num = 0.4f + 0.8f * playerProgress;
			int num2 = MBRandom.RandomInt(2);
			float num3 = ((num2 == 0) ? MBRandom.RandomFloat : (MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * 4f));
			float num4 = ((num2 == 0) ? (num3 * 0.8f + 0.2f) : (1f + num3));
			float randomFloat = MBRandom.RandomFloat;
			float randomFloat2 = MBRandom.RandomFloat;
			float randomFloat3 = MBRandom.RandomFloat;
			float f = ((pt.Stacks.Count > 0) ? ((float)pt.Stacks[0].MinValue + num * num4 * randomFloat * (float)(pt.Stacks[0].MaxValue - pt.Stacks[0].MinValue)) : 0f);
			float f2 = ((pt.Stacks.Count > 1) ? ((float)pt.Stacks[1].MinValue + num * num4 * randomFloat2 * (float)(pt.Stacks[1].MaxValue - pt.Stacks[1].MinValue)) : 0f);
			float f3 = ((pt.Stacks.Count > 2) ? ((float)pt.Stacks[2].MinValue + num * num4 * randomFloat3 * (float)(pt.Stacks[2].MaxValue - pt.Stacks[2].MinValue)) : 0f);
			AddElementToMemberRoster(pt.Stacks[0].Character, MBRandom.RoundRandomized(f));
			if (pt.Stacks.Count > 1)
			{
				AddElementToMemberRoster(pt.Stacks[1].Character, MBRandom.RoundRandomized(f2));
			}
			if (pt.Stacks.Count > 2)
			{
				AddElementToMemberRoster(pt.Stacks[2].Character, MBRandom.RoundRandomized(f3));
			}
			return;
		}
		if (troopNumberLimit < 0)
		{
			float playerProgress2 = Campaign.Current.PlayerProgress;
			for (int i = 0; i < pt.Stacks.Count; i++)
			{
				int numberToAdd = (int)(playerProgress2 * (float)(pt.Stacks[i].MaxValue - pt.Stacks[i].MinValue)) + pt.Stacks[i].MinValue;
				CharacterObject character = pt.Stacks[i].Character;
				AddElementToMemberRoster(character, numberToAdd);
			}
			return;
		}
		for (int j = 0; j < troopNumberLimit; j++)
		{
			int num5 = -1;
			float num6 = 0f;
			for (int k = 0; k < pt.Stacks.Count; k++)
			{
				num6 += ((IsGarrison && pt.Stacks[k].Character.IsRanged) ? 6f : ((IsGarrison && !pt.Stacks[k].Character.IsMounted) ? 2f : 1f)) * ((float)(pt.Stacks[k].MaxValue + pt.Stacks[k].MinValue) / 2f);
			}
			float num7 = MBRandom.RandomFloat * num6;
			for (int l = 0; l < pt.Stacks.Count; l++)
			{
				num7 -= ((IsGarrison && pt.Stacks[l].Character.IsRanged) ? 6f : ((IsGarrison && !pt.Stacks[l].Character.IsMounted) ? 2f : 1f)) * ((float)(pt.Stacks[l].MaxValue + pt.Stacks[l].MinValue) / 2f);
				if (num7 < 0f)
				{
					num5 = l;
					break;
				}
			}
			if (num5 < 0)
			{
				num5 = 0;
			}
			CharacterObject character2 = pt.Stacks[num5].Character;
			AddElementToMemberRoster(character2, 1);
		}
		_ = IsVillager;
	}

	private void OnPartyJoinedSiegeInternal()
	{
		_besiegerCamp.AddSiegePartyInternal(this);
		_besiegerCamp.SetSiegeCampPartyPosition(this);
	}

	private void OnPartyLeftSiegeInternal()
	{
		_besiegerCamp.RemoveSiegePartyInternal(this);
		EventPositionAdder = Vec2.Zero;
		ErrorPosition = Vec2.Zero;
	}

	public bool HasPerk(PerkObject perk, bool checkSecondaryRole = false)
	{
		switch (checkSecondaryRole ? perk.SecondaryRole : perk.PrimaryRole)
		{
		case SkillEffect.PerkRole.Scout:
			return EffectiveScout?.GetPerkValue(perk) ?? false;
		case SkillEffect.PerkRole.Engineer:
			return EffectiveEngineer?.GetPerkValue(perk) ?? false;
		case SkillEffect.PerkRole.Quartermaster:
			return EffectiveQuartermaster?.GetPerkValue(perk) ?? false;
		case SkillEffect.PerkRole.Surgeon:
			return EffectiveSurgeon?.GetPerkValue(perk) ?? false;
		case SkillEffect.PerkRole.PartyLeader:
			return LeaderHero?.GetPerkValue(perk) ?? false;
		case SkillEffect.PerkRole.ArmyCommander:
			return Army?.LeaderParty?.LeaderHero?.GetPerkValue(perk) ?? false;
		case SkillEffect.PerkRole.PartyMember:
			foreach (TroopRosterElement item in MemberRoster.GetTroopRoster())
			{
				if (item.Character.IsHero && item.Character.HeroObject.GetPerkValue(perk))
				{
					return true;
				}
			}
			return false;
		case SkillEffect.PerkRole.Personal:
			Debug.FailedAssert("personal perk is called in party", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Party\\MobileParty.cs", "HasPerk", 1986);
			return LeaderHero?.GetPerkValue(perk) ?? false;
		case SkillEffect.PerkRole.ClanLeader:
			if (LeaderHero != null)
			{
				return LeaderHero.Clan?.Leader?.GetPerkValue(perk) ?? false;
			}
			return false;
		default:
			return false;
		}
	}

	public SkillEffect.PerkRole GetHeroPerkRole(Hero hero)
	{
		if (Engineer == hero)
		{
			return SkillEffect.PerkRole.Engineer;
		}
		if (Quartermaster == hero)
		{
			return SkillEffect.PerkRole.Quartermaster;
		}
		if (Surgeon == hero)
		{
			return SkillEffect.PerkRole.Surgeon;
		}
		if (Scout == hero)
		{
			return SkillEffect.PerkRole.Scout;
		}
		return SkillEffect.PerkRole.None;
	}

	public void SetHeroPerkRole(Hero hero, SkillEffect.PerkRole perkRole)
	{
		switch (perkRole)
		{
		case SkillEffect.PerkRole.Surgeon:
			SetPartySurgeon(hero);
			break;
		case SkillEffect.PerkRole.Engineer:
			SetPartyEngineer(hero);
			break;
		case SkillEffect.PerkRole.Scout:
			SetPartyScout(hero);
			break;
		case SkillEffect.PerkRole.Quartermaster:
			SetPartyQuartermaster(hero);
			break;
		case SkillEffect.PerkRole.None:
		case SkillEffect.PerkRole.Ruler:
		case SkillEffect.PerkRole.ClanLeader:
		case SkillEffect.PerkRole.Governor:
		case SkillEffect.PerkRole.ArmyCommander:
		case SkillEffect.PerkRole.PartyLeader:
		case SkillEffect.PerkRole.PartyOwner:
		case SkillEffect.PerkRole.PartyMember:
		case SkillEffect.PerkRole.Personal:
		case SkillEffect.PerkRole.Captain:
		case SkillEffect.PerkRole.NumberOfPerkRoles:
			break;
		}
	}

	public void RemoveHeroPerkRole(Hero hero)
	{
		if (Engineer == hero)
		{
			Engineer = null;
		}
		if (Quartermaster == hero)
		{
			Quartermaster = null;
		}
		if (Surgeon == hero)
		{
			Surgeon = null;
		}
		if (Scout == hero)
		{
			Scout = null;
		}
		ResetCached();
	}

	public Hero GetRoleHolder(SkillEffect.PerkRole perkRole)
	{
		return perkRole switch
		{
			SkillEffect.PerkRole.PartyLeader => LeaderHero, 
			SkillEffect.PerkRole.Surgeon => Surgeon, 
			SkillEffect.PerkRole.Engineer => Engineer, 
			SkillEffect.PerkRole.Quartermaster => Quartermaster, 
			SkillEffect.PerkRole.Scout => Scout, 
			_ => null, 
		};
	}

	public Hero GetEffectiveRoleHolder(SkillEffect.PerkRole perkRole)
	{
		return perkRole switch
		{
			SkillEffect.PerkRole.PartyLeader => LeaderHero, 
			SkillEffect.PerkRole.Surgeon => EffectiveSurgeon, 
			SkillEffect.PerkRole.Engineer => EffectiveEngineer, 
			SkillEffect.PerkRole.Quartermaster => EffectiveQuartermaster, 
			SkillEffect.PerkRole.Scout => EffectiveScout, 
			_ => null, 
		};
	}

	public int GetNumDaysForFoodToLast()
	{
		int totalFood = ItemRoster.TotalFood;
		totalFood *= 100;
		if (this == MainParty)
		{
			totalFood += Party.RemainingFoodPercentage;
		}
		return (int)((float)totalFood / (100f * (0f - FoodChange)));
	}

	public Vec3 GetPosition()
	{
		return GetLogicalPosition();
	}

	public float GetTrackDistanceToMainAgent()
	{
		return GetPosition().Distance(Hero.MainHero.GetPosition());
	}

	public bool CheckTracked(BasicCharacterObject basicCharacter)
	{
		if (!MemberRoster.GetTroopRoster().Any((TroopRosterElement t) => t.Character == basicCharacter))
		{
			return PrisonRoster.GetTroopRoster().Any((TroopRosterElement t) => t.Character == basicCharacter);
		}
		return true;
	}

	private Settlement DetermineRelatedBesiegedSettlementWhileDestroyingParty()
	{
		Settlement settlement = BesiegedSettlement;
		if (settlement == null)
		{
			settlement = ((ShortTermBehavior == AiBehavior.AssaultSettlement) ? ShortTermTargetSettlement : null);
		}
		if (settlement == null && (IsGarrison || IsMilitia) && CurrentSettlement != null)
		{
			MapEvent mapEvent = CurrentSettlement.LastAttackerParty?.MapEvent;
			if (mapEvent != null && (mapEvent.IsSiegeAssault || mapEvent.IsSiegeOutside || mapEvent.IsSallyOut) && mapEvent.DefeatedSide != BattleSideEnum.None && mapEvent.State == MapEventState.WaitingRemoval)
			{
				settlement = CurrentSettlement;
			}
		}
		return settlement;
	}

	public void RemoveParty()
	{
		IsActive = false;
		IsVisible = false;
		Settlement settlement = DetermineRelatedBesiegedSettlementWhileDestroyingParty();
		Campaign current = Campaign.Current;
		AttachedTo = null;
		BesiegerCamp = null;
		ReleaseHeroPrisoners();
		ItemRoster.Clear();
		MemberRoster.Reset();
		PrisonRoster.Reset();
		Campaign.Current.MobilePartyLocator.RemoveLocatable(this);
		Campaign.Current.VisualTrackerManager.RemoveTrackedObject(this, forceRemove: true);
		CampaignEventDispatcher.Instance.OnPartyRemoved(Party);
		GC.SuppressFinalize(Party);
		foreach (MobileParty mobileParty in current.MobileParties)
		{
			bool flag = mobileParty.Ai.AiBehaviorPartyBase == Party || (mobileParty.TargetSettlement != null && mobileParty.TargetSettlement == settlement && mobileParty.CurrentSettlement != settlement) || (mobileParty.ShortTermTargetSettlement != null && mobileParty.ShortTermTargetSettlement == settlement && mobileParty.CurrentSettlement != settlement);
			if (mobileParty.TargetParty != null && mobileParty.TargetParty == this)
			{
				mobileParty.ResetTargetParty();
				flag = true;
			}
			if (flag && mobileParty.TargetSettlement != null && (mobileParty.MapEvent == null || mobileParty.MapEvent.IsFinalized) && mobileParty.DefaultBehavior == AiBehavior.GoToSettlement)
			{
				Settlement targetSettlement = mobileParty.TargetSettlement;
				mobileParty.Ai.SetMoveModeHold();
				mobileParty.Ai.SetNavigationModeHold();
				mobileParty.Ai.SetMoveGoToSettlement(targetSettlement);
				flag = false;
			}
			if (flag)
			{
				mobileParty.Ai.SetMoveModeHold();
				mobileParty.Ai.SetNavigationModeHold();
			}
		}
		OnRemoveParty();
		_customHomeSettlement = null;
	}

	private void ReleaseHeroPrisoners()
	{
		for (int num = PrisonRoster.Count - 1; num >= 0; num--)
		{
			if (PrisonRoster.GetElementNumber(num) > 0)
			{
				TroopRosterElement elementCopyAtIndex = PrisonRoster.GetElementCopyAtIndex(num);
				if (elementCopyAtIndex.Character.IsHero && !elementCopyAtIndex.Character.IsPlayerCharacter)
				{
					EndCaptivityAction.ApplyByReleasedByChoice(elementCopyAtIndex.Character.HeroObject);
				}
			}
		}
	}

	private void CreateFigure(Vec2 position, float spawnRadius)
	{
		Vec2 accessiblePointNearPosition = Campaign.Current.MapSceneWrapper.GetAccessiblePointNearPosition(position, spawnRadius);
		Position2D = new Vec2(accessiblePointNearPosition.x, accessiblePointNearPosition.y);
		Vec2 bearing = new Vec2(1f, 0f);
		float angleInRadians = MBRandom.RandomFloat * 2f * System.MathF.PI;
		bearing.RotateCCW(angleInRadians);
		Bearing = bearing;
		Party.UpdateVisibilityAndInspected();
		StartUp();
	}

	internal void SendPartyToReachablePointAroundPosition(Vec2 centerPosition, float distanceLimit, float innerCenterMinimumDistanceLimit = 0f)
	{
		Ai.SetMoveGoToPoint(MobilePartyHelper.FindReachablePointAroundPosition(centerPosition, distanceLimit, innerCenterMinimumDistanceLimit));
	}

	internal void TeleportPartyToSafePosition(float maxRadius = 3.3f, float minRadius = 3f)
	{
		for (int i = 0; i < 15; i++)
		{
			Vec2 vec = MobilePartyHelper.FindReachablePointAroundPosition(Position2D, maxRadius, minRadius);
			bool flag = true;
			LocatableSearchData<MobileParty> data = StartFindingLocatablesAroundPosition(vec, 1f);
			for (MobileParty mobileParty = FindNextLocatable(ref data); mobileParty != null; mobileParty = FindNextLocatable(ref data))
			{
				if (mobileParty.MapFaction.IsAtWarWith(MapFaction))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				Position2D = vec;
				break;
			}
		}
	}

	public static LocatableSearchData<MobileParty> StartFindingLocatablesAroundPosition(Vec2 position, float radius)
	{
		return Campaign.Current.MobilePartyLocator.StartFindingLocatablesAroundPosition(position, radius);
	}

	public static MobileParty FindNextLocatable(ref LocatableSearchData<MobileParty> data)
	{
		return Campaign.Current.MobilePartyLocator.FindNextLocatable(ref data);
	}

	internal void OnHeroAdded(Hero hero)
	{
		hero.OnAddedToParty(this);
	}

	internal void OnHeroRemoved(Hero hero)
	{
		hero.OnRemovedFromParty(this);
	}

	internal void CheckExitingSettlementParallel(ref int exitingPartyCount, ref MobileParty[] exitingPartyList)
	{
		if (!Ai.IsDisabled && ShortTermBehavior != 0 && CurrentSettlement != null && (!IsCurrentlyGoingToSettlement || ShortTermTargetSettlement != CurrentSettlement) && !IsMainParty && (Army == null || AttachedTo == null || Army.LeaderParty == this))
		{
			int num = Interlocked.Increment(ref exitingPartyCount);
			exitingPartyList[num] = this;
		}
	}

	public bool ComputeIsWaiting()
	{
		if (!((2f * Position2D - TargetPosition - Ai.NextTargetPosition).LengthSquared < 1E-05f) && DefaultBehavior != 0)
		{
			if ((DefaultBehavior == AiBehavior.EngageParty || DefaultBehavior == AiBehavior.EscortParty) && Ai.AiBehaviorPartyBase != null && Ai.AiBehaviorPartyBase.IsValid && Ai.AiBehaviorPartyBase.IsActive && Ai.AiBehaviorPartyBase.IsMobile)
			{
				return Ai.AiBehaviorPartyBase.MobileParty.CurrentSettlement != null;
			}
			return false;
		}
		return true;
	}

	void IMapEntity.OnOpenEncyclopedia()
	{
		if (IsLordParty && LeaderHero != null)
		{
			Campaign.Current.EncyclopediaManager.GoToLink(LeaderHero.EncyclopediaLink);
		}
	}

	bool IMapEntity.OnMapClick(bool followModifierUsed)
	{
		if (IsMainParty)
		{
			MainParty.Ai.SetMoveModeHold();
		}
		else if (followModifierUsed)
		{
			MainParty.Ai.SetMoveEscortParty(this);
		}
		else
		{
			MainParty.Ai.SetMoveEngageParty(this);
		}
		return true;
	}

	void IMapEntity.OnHover()
	{
		if (Army?.LeaderParty == this)
		{
			InformationManager.ShowTooltip(typeof(Army), Army, false, true);
		}
		else
		{
			InformationManager.ShowTooltip(typeof(MobileParty), this, false, true);
		}
	}

	bool IMapEntity.IsEnemyOf(IFaction faction)
	{
		return FactionManager.IsAtWarAgainstFaction(MapFaction, faction);
	}

	bool IMapEntity.IsAllyOf(IFaction faction)
	{
		return FactionManager.IsAlliedWithFaction(MapFaction, faction);
	}

	public void GetMountAndHarnessVisualIdsForPartyIcon(out string mountStringId, out string harnessStringId)
	{
		mountStringId = "";
		harnessStringId = "";
		_partyComponent?.GetMountAndHarnessVisualIdsForPartyIcon(Party, out mountStringId, out harnessStringId);
	}

	void IMapEntity.OnPartyInteraction(MobileParty engagingParty)
	{
		MobileParty mobileParty = this;
		if (mobileParty.AttachedTo != null && engagingParty != mobileParty.AttachedTo)
		{
			mobileParty = mobileParty.AttachedTo;
		}
		bool flag = false;
		if (mobileParty.CurrentSettlement != null)
		{
			if (mobileParty.MapEvent != null)
			{
				flag = mobileParty.MapEvent.MapEventSettlement == mobileParty.CurrentSettlement && (mobileParty.MapEvent.AttackerSide.LeaderParty.MapFaction == engagingParty.MapFaction || mobileParty.MapEvent.DefenderSide.LeaderParty.MapFaction == engagingParty.MapFaction);
			}
		}
		else
		{
			flag = engagingParty != MainParty || !mobileParty.IsEngaging || mobileParty.ShortTermTargetParty != MainParty;
		}
		if (flag)
		{
			if (engagingParty == MainParty)
			{
				(Game.Current.GameStateManager.ActiveState as MapState)?.OnMainPartyEncounter();
			}
			EncounterManager.StartPartyEncounter(engagingParty.Party, mobileParty.Party);
		}
	}

	public void InitializePartyTrade(int initialGold)
	{
		IsPartyTradeActive = true;
		PartyTradeGold = initialGold;
	}

	public void AddTaxGold(int amount)
	{
		PartyTradeTaxGold += amount;
	}

	public static MobileParty CreateParty(string stringId, PartyComponent component, PartyComponent.OnPartyComponentCreatedDelegate delegateFunction = null)
	{
		stringId = Campaign.Current.CampaignObjectManager.FindNextUniqueStringId<MobileParty>(stringId);
		MobileParty mobileParty = new MobileParty();
		mobileParty.StringId = stringId;
		mobileParty._partyComponent = component;
		mobileParty.UpdatePartyComponentFlags();
		component?.SetMobilePartyInternal(mobileParty);
		delegateFunction?.Invoke(mobileParty);
		mobileParty.PartyComponent?.Initialize(mobileParty);
		Campaign.Current.CampaignObjectManager.AddMobileParty(mobileParty);
		CampaignEventDispatcher.Instance.OnMobilePartyCreated(mobileParty);
		return mobileParty;
	}

	public void UpdatePartyComponentFlags()
	{
		IsLordParty = _partyComponent is LordPartyComponent;
		IsVillager = _partyComponent is VillagerPartyComponent;
		IsMilitia = _partyComponent is MilitiaPartyComponent;
		IsCaravan = _partyComponent is CaravanPartyComponent;
		IsGarrison = _partyComponent is GarrisonPartyComponent;
		IsCustomParty = _partyComponent is CustomPartyComponent;
		IsBandit = _partyComponent is BanditPartyComponent;
	}
}
