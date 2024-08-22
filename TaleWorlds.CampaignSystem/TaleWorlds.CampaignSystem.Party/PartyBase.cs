using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace TaleWorlds.CampaignSystem.Party;

public sealed class PartyBase : IBattleCombatant, IRandomOwner
{
	private static readonly HashSet<TerrainType> ValidTerrainTypes = new HashSet<TerrainType>
	{
		TerrainType.Snow,
		TerrainType.Steppe,
		TerrainType.Plain,
		TerrainType.Desert,
		TerrainType.Swamp,
		TerrainType.Dune,
		TerrainType.Bridge,
		TerrainType.Forest,
		TerrainType.Fording
	};

	[SaveableField(15)]
	private int _remainingFoodPercentage;

	[SaveableField(182)]
	private CampaignTime _lastEatingTime = CampaignTime.Now;

	[SaveableField(8)]
	private Hero _customOwner;

	[SaveableField(9)]
	private int _index;

	[SaveableField(200)]
	private MapEventSide _mapEventSide;

	[CachedData]
	private int _lastMemberRosterVersionNo;

	[CachedData]
	private int _partyMemberSizeLastCheckVersion;

	[CachedData]
	private int _cachedPartyMemberSizeLimit;

	[CachedData]
	private int _prisonerSizeLastCheckVersion;

	[CachedData]
	private int _cachedPrisonerSizeLimit;

	[CachedData]
	private int _lastNumberOfMenWithHorseVersionNo;

	[CachedData]
	private int _lastNumberOfMenPerTierVersionNo;

	[SaveableField(17)]
	private int _numberOfMenWithHorse;

	private int[] _numberOfHealthyMenPerTier;

	[CachedData]
	private float _cachedTotalStrength;

	public Vec2 Position2D
	{
		get
		{
			if (!IsMobile)
			{
				return Settlement.Position2D;
			}
			return MobileParty.Position2D;
		}
	}

	public bool IsVisible
	{
		get
		{
			if (!IsMobile)
			{
				return Settlement.IsVisible;
			}
			return MobileParty.IsVisible;
		}
	}

	public bool IsActive
	{
		get
		{
			if (!IsMobile)
			{
				return Settlement.IsActive;
			}
			return MobileParty.IsActive;
		}
	}

	public SiegeEvent SiegeEvent
	{
		get
		{
			if (!IsMobile)
			{
				return Settlement.SiegeEvent;
			}
			return MobileParty.SiegeEvent;
		}
	}

	[SaveableProperty(1)]
	public Settlement Settlement { get; private set; }

	[SaveableProperty(2)]
	public MobileParty MobileParty { get; private set; }

	public bool IsSettlement => Settlement != null;

	public bool IsMobile => MobileParty != null;

	[SaveableProperty(3)]
	public TroopRoster MemberRoster { get; private set; }

	[SaveableProperty(4)]
	public TroopRoster PrisonRoster { get; private set; }

	[SaveableProperty(5)]
	public ItemRoster ItemRoster { get; private set; }

	public TextObject Name
	{
		get
		{
			if (!IsSettlement)
			{
				if (!IsMobile)
				{
					return TextObject.Empty;
				}
				return MobileParty.Name;
			}
			return Settlement.Name;
		}
	}

	public float DaysStarving
	{
		get
		{
			if (!IsStarving)
			{
				return 0f;
			}
			return _lastEatingTime.ElapsedDaysUntilNow;
		}
	}

	public int RemainingFoodPercentage
	{
		get
		{
			return _remainingFoodPercentage;
		}
		set
		{
			_remainingFoodPercentage = value;
		}
	}

	public bool IsStarving => _remainingFoodPercentage < 0;

	public string Id => MobileParty?.StringId ?? Settlement.StringId;

	public Hero Owner
	{
		get
		{
			Hero hero = _customOwner;
			if (hero == null)
			{
				if (!IsMobile)
				{
					return Settlement.Owner;
				}
				hero = MobileParty.Owner;
			}
			return hero;
		}
	}

	public Hero LeaderHero => MobileParty?.LeaderHero;

	public static PartyBase MainParty
	{
		get
		{
			if (Campaign.Current == null)
			{
				return null;
			}
			return Campaign.Current.MainParty.Party;
		}
	}

	public bool LevelMaskIsDirty { get; private set; }

	public int Index
	{
		get
		{
			return _index;
		}
		private set
		{
			_index = value;
		}
	}

	public bool IsValid => Index >= 0;

	public IMapEntity MapEntity
	{
		get
		{
			if (IsMobile)
			{
				return MobileParty;
			}
			return Settlement;
		}
	}

	public IFaction MapFaction
	{
		get
		{
			if (MobileParty != null)
			{
				return MobileParty.MapFaction;
			}
			if (Settlement != null)
			{
				return Settlement.MapFaction;
			}
			return null;
		}
	}

	[SaveableProperty(210)]
	public int RandomValue { get; private set; } = MBRandom.RandomInt(1, int.MaxValue);


	public CultureObject Culture => MapFaction.Culture;

	public Tuple<uint, uint> PrimaryColorPair
	{
		get
		{
			if (MapFaction == null)
			{
				return new Tuple<uint, uint>(4291609515u, 4291609515u);
			}
			return new Tuple<uint, uint>(MapFaction.Color, MapFaction.Color2);
		}
	}

	public Tuple<uint, uint> AlternativeColorPair
	{
		get
		{
			if (MapFaction == null)
			{
				return new Tuple<uint, uint>(4291609515u, 4291609515u);
			}
			return new Tuple<uint, uint>(MapFaction.AlternativeColor, MapFaction.AlternativeColor2);
		}
	}

	public Banner Banner
	{
		get
		{
			if (LeaderHero == null)
			{
				return MapFaction?.Banner;
			}
			return LeaderHero.ClanBanner;
		}
	}

	public MapEvent MapEvent => _mapEventSide?.MapEvent;

	public MapEventSide MapEventSide
	{
		get
		{
			return _mapEventSide;
		}
		set
		{
			if (_mapEventSide == value)
			{
				return;
			}
			if (value != null && IsMobile && MapEvent != null && MapEvent.DefenderSide.LeaderParty == this)
			{
				Debug.FailedAssert($"Double MapEvent For {Name}", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Party\\PartyBase.cs", "MapEventSide", 246);
			}
			if (_mapEventSide != null)
			{
				_mapEventSide.RemovePartyInternal(this);
			}
			_mapEventSide = value;
			if (_mapEventSide != null)
			{
				_mapEventSide.AddPartyInternal(this);
			}
			if (MobileParty == null)
			{
				return;
			}
			foreach (MobileParty attachedParty in MobileParty.AttachedParties)
			{
				attachedParty.Party.MapEventSide = _mapEventSide;
			}
		}
	}

	public BattleSideEnum Side => MapEventSide?.MissionSide ?? BattleSideEnum.None;

	public BattleSideEnum OpponentSide
	{
		get
		{
			if (Side == BattleSideEnum.Attacker)
			{
				return BattleSideEnum.Defender;
			}
			return BattleSideEnum.Attacker;
		}
	}

	public int PartySizeLimit
	{
		get
		{
			int versionNo = MemberRoster.VersionNo;
			if (_partyMemberSizeLastCheckVersion != versionNo || _cachedPartyMemberSizeLimit == 0)
			{
				_partyMemberSizeLastCheckVersion = versionNo;
				_cachedPartyMemberSizeLimit = (int)Campaign.Current.Models.PartySizeLimitModel.GetPartyMemberSizeLimit(this).ResultNumber;
			}
			return _cachedPartyMemberSizeLimit;
		}
	}

	public int PrisonerSizeLimit
	{
		get
		{
			int versionNo = MemberRoster.VersionNo;
			if (_prisonerSizeLastCheckVersion != versionNo || _cachedPrisonerSizeLimit == 0)
			{
				_prisonerSizeLastCheckVersion = versionNo;
				_cachedPrisonerSizeLimit = (int)Campaign.Current.Models.PartySizeLimitModel.GetPartyPrisonerSizeLimit(this).ResultNumber;
			}
			return _cachedPrisonerSizeLimit;
		}
	}

	public ExplainedNumber PartySizeLimitExplainer => Campaign.Current.Models.PartySizeLimitModel.GetPartyMemberSizeLimit(this, includeDescriptions: true);

	public ExplainedNumber PrisonerSizeLimitExplainer => Campaign.Current.Models.PartySizeLimitModel.GetPartyPrisonerSizeLimit(this, includeDescriptions: true);

	public int NumberOfHealthyMembers => MemberRoster.TotalManCount - MemberRoster.TotalWounded;

	public int NumberOfRegularMembers => MemberRoster.TotalRegulars;

	public int NumberOfWoundedTotalMembers => MemberRoster.TotalWounded;

	public int NumberOfAllMembers => MemberRoster.TotalManCount;

	public int NumberOfPrisoners => PrisonRoster.TotalManCount;

	public int NumberOfMounts => ItemRoster.NumberOfMounts;

	public int NumberOfPackAnimals => ItemRoster.NumberOfPackAnimals;

	public IEnumerable<CharacterObject> PrisonerHeroes
	{
		get
		{
			for (int i = 0; i < PrisonRoster.Count; i++)
			{
				if (PrisonRoster.GetElementNumber(i) > 0)
				{
					TroopRosterElement elementCopyAtIndex = PrisonRoster.GetElementCopyAtIndex(i);
					if (elementCopyAtIndex.Character.IsHero)
					{
						yield return elementCopyAtIndex.Character;
					}
				}
			}
		}
	}

	public int NumberOfMenWithHorse
	{
		get
		{
			if (_lastNumberOfMenWithHorseVersionNo != MemberRoster.VersionNo)
			{
				RecalculateNumberOfMenWithHorses();
				_lastNumberOfMenWithHorseVersionNo = MemberRoster.VersionNo;
			}
			return _numberOfMenWithHorse;
		}
	}

	public int NumberOfMenWithoutHorse => NumberOfAllMembers - NumberOfMenWithHorse;

	public int InventoryCapacity
	{
		get
		{
			if (MobileParty == null)
			{
				return 100;
			}
			return (int)Campaign.Current.Models.InventoryCapacityModel.CalculateInventoryCapacity(MobileParty).ResultNumber;
		}
	}

	public float TotalStrength
	{
		get
		{
			if (_lastMemberRosterVersionNo == MemberRoster.VersionNo)
			{
				return _cachedTotalStrength;
			}
			_cachedTotalStrength = CalculateStrength();
			_lastMemberRosterVersionNo = MemberRoster.VersionNo;
			return _cachedTotalStrength;
		}
	}

	[SaveableProperty(12)]
	public float AverageBearingRotation { get; set; }

	public BasicCultureObject BasicCulture => Culture;

	public BasicCharacterObject General
	{
		get
		{
			if (MobileParty?.Army != null)
			{
				return MobileParty.Army.LeaderParty?.LeaderHero?.CharacterObject;
			}
			return LeaderHero?.CharacterObject;
		}
	}

	[CachedData]
	public bool IsVisualDirty { get; private set; }

	internal static void AutoGeneratedStaticCollectObjectsPartyBase(object o, List<object> collectedObjects)
	{
		((PartyBase)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	private void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime(_lastEatingTime, collectedObjects);
		collectedObjects.Add(_customOwner);
		collectedObjects.Add(_mapEventSide);
		collectedObjects.Add(Settlement);
		collectedObjects.Add(MobileParty);
		collectedObjects.Add(MemberRoster);
		collectedObjects.Add(PrisonRoster);
		collectedObjects.Add(ItemRoster);
	}

	internal static object AutoGeneratedGetMemberValueSettlement(object o)
	{
		return ((PartyBase)o).Settlement;
	}

	internal static object AutoGeneratedGetMemberValueMobileParty(object o)
	{
		return ((PartyBase)o).MobileParty;
	}

	internal static object AutoGeneratedGetMemberValueMemberRoster(object o)
	{
		return ((PartyBase)o).MemberRoster;
	}

	internal static object AutoGeneratedGetMemberValuePrisonRoster(object o)
	{
		return ((PartyBase)o).PrisonRoster;
	}

	internal static object AutoGeneratedGetMemberValueItemRoster(object o)
	{
		return ((PartyBase)o).ItemRoster;
	}

	internal static object AutoGeneratedGetMemberValueRandomValue(object o)
	{
		return ((PartyBase)o).RandomValue;
	}

	internal static object AutoGeneratedGetMemberValueAverageBearingRotation(object o)
	{
		return ((PartyBase)o).AverageBearingRotation;
	}

	internal static object AutoGeneratedGetMemberValue_remainingFoodPercentage(object o)
	{
		return ((PartyBase)o)._remainingFoodPercentage;
	}

	internal static object AutoGeneratedGetMemberValue_lastEatingTime(object o)
	{
		return ((PartyBase)o)._lastEatingTime;
	}

	internal static object AutoGeneratedGetMemberValue_customOwner(object o)
	{
		return ((PartyBase)o)._customOwner;
	}

	internal static object AutoGeneratedGetMemberValue_index(object o)
	{
		return ((PartyBase)o)._index;
	}

	internal static object AutoGeneratedGetMemberValue_mapEventSide(object o)
	{
		return ((PartyBase)o)._mapEventSide;
	}

	internal static object AutoGeneratedGetMemberValue_numberOfMenWithHorse(object o)
	{
		return ((PartyBase)o)._numberOfMenWithHorse;
	}

	public void OnVisibilityChanged(bool value)
	{
		MapEvent?.PartyVisibilityChanged(this, value);
		CampaignEventDispatcher.Instance.OnPartyVisibilityChanged(this);
		SetVisualAsDirty();
	}

	public void OnConsumedFood()
	{
		_lastEatingTime = CampaignTime.Now;
	}

	public void SetCustomOwner(Hero customOwner)
	{
		_customOwner = customOwner;
	}

	public void SetLevelMaskIsDirty()
	{
		LevelMaskIsDirty = true;
	}

	public void OnLevelMaskUpdated()
	{
		LevelMaskIsDirty = false;
	}

	int IBattleCombatant.GetTacticsSkillAmount()
	{
		if (LeaderHero != null)
		{
			return LeaderHero.GetSkillValue(DefaultSkills.Tactics);
		}
		return 0;
	}

	internal void AfterLoad()
	{
		if (RandomValue == 0)
		{
			RandomValue = MBRandom.RandomInt(1, int.MaxValue);
		}
		TroopRoster prisonRoster = PrisonRoster;
		if ((object)prisonRoster != null && prisonRoster.Contains(CharacterObject.PlayerCharacter) && (this != Hero.MainHero.PartyBelongedToAsPrisoner || (Hero.MainHero.PartyBelongedTo != null && Hero.MainHero.PartyBelongedToAsPrisoner != null)))
		{
			if (Hero.MainHero.PartyBelongedTo == MainParty?.MobileParty)
			{
				PrisonRoster.AddToCounts(CharacterObject.PlayerCharacter, -1);
			}
			else
			{
				PlayerCaptivity.CaptorParty = this;
			}
		}
		if (IsMobile && MobileParty.IsCaravan && !MobileParty.IsCurrentlyUsedByAQuest && _customOwner != null && MobileParty.Owner != Owner)
		{
			SetCustomOwner(null);
		}
		foreach (TroopRosterElement item in PrisonRoster.GetTroopRoster())
		{
			if (item.Character.HeroObject != null && item.Character.HeroObject.PartyBelongedToAsPrisoner == null)
			{
				PrisonRoster.RemoveTroop(item.Character);
			}
		}
		if (MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.2.0"))
		{
			MemberRoster.RemoveZeroCounts();
		}
	}

	internal void InitCache()
	{
		_partyMemberSizeLastCheckVersion = -1;
		_prisonerSizeLastCheckVersion = -1;
		_lastNumberOfMenWithHorseVersionNo = -1;
		_lastNumberOfMenPerTierVersionNo = -1;
		_lastMemberRosterVersionNo = -1;
	}

	[LoadInitializationCallback]
	private void OnLoad(MetaData metaData, ObjectLoadData objectLoadData)
	{
		InitCache();
	}

	public int GetNumberOfHealthyMenOfTier(int tier)
	{
		if (tier < 0)
		{
			Debug.FailedAssert("Requested men count for negative tier.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Party\\PartyBase.cs", "GetNumberOfHealthyMenOfTier", 461);
			return 0;
		}
		bool flag = false;
		if (_numberOfHealthyMenPerTier == null || tier >= _numberOfHealthyMenPerTier.Length)
		{
			int num = TaleWorlds.Library.MathF.Max(tier, 6);
			_numberOfHealthyMenPerTier = new int[num + 1];
			flag = true;
		}
		else if (_lastNumberOfMenPerTierVersionNo != MemberRoster.VersionNo)
		{
			flag = true;
		}
		if (flag)
		{
			for (int i = 0; i < _numberOfHealthyMenPerTier.Length; i++)
			{
				_numberOfHealthyMenPerTier[i] = 0;
			}
			for (int j = 0; j < MemberRoster.Count; j++)
			{
				CharacterObject characterAtIndex = MemberRoster.GetCharacterAtIndex(j);
				if (characterAtIndex != null && !characterAtIndex.IsHero)
				{
					int tier2 = characterAtIndex.Tier;
					if (tier2 >= 0 && tier2 < _numberOfHealthyMenPerTier.Length)
					{
						int num2 = MemberRoster.GetElementNumber(j) - MemberRoster.GetElementWoundedNumber(j);
						_numberOfHealthyMenPerTier[tier2] += num2;
					}
				}
			}
			_lastNumberOfMenPerTierVersionNo = MemberRoster.VersionNo;
		}
		return _numberOfHealthyMenPerTier[tier];
	}

	public PartyBase(MobileParty mobileParty)
		: this(mobileParty, null)
	{
	}

	public PartyBase(Settlement settlement)
		: this(null, settlement)
	{
	}

	private PartyBase(MobileParty mobileParty, Settlement settlement)
	{
		Index = Campaign.Current.GeneratePartyId(this);
		MobileParty = mobileParty;
		Settlement = settlement;
		ItemRoster = new ItemRoster();
		MemberRoster = new TroopRoster(this);
		PrisonRoster = new TroopRoster(this);
		MemberRoster.NumberChangedCallback = MemberRosterNumberChanged;
		PrisonRoster.IsPrisonRoster = true;
	}

	private void RecalculateNumberOfMenWithHorses()
	{
		_numberOfMenWithHorse = 0;
		for (int i = 0; i < MemberRoster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = MemberRoster.GetElementCopyAtIndex(i);
			if (elementCopyAtIndex.Character != null && elementCopyAtIndex.Character.IsMounted)
			{
				_numberOfMenWithHorse += elementCopyAtIndex.Number;
			}
		}
	}

	public int GetNumberOfMenWith(TraitObject trait)
	{
		int num = 0;
		foreach (TroopRosterElement item in MemberRoster.GetTroopRoster())
		{
			if (item.Character.GetTraitLevel(trait) > 0)
			{
				num += item.Number;
			}
		}
		return num;
	}

	public int AddPrisoner(CharacterObject element, int numberToAdd)
	{
		return PrisonRoster.AddToCounts(element, numberToAdd);
	}

	public int AddMember(CharacterObject element, int numberToAdd, int numberToAddWounded = 0)
	{
		return MemberRoster.AddToCounts(element, numberToAdd, insertAtFront: false, numberToAddWounded);
	}

	public void AddPrisoners(TroopRoster roster)
	{
		foreach (TroopRosterElement item in roster.GetTroopRoster())
		{
			AddPrisoner(item.Character, item.Number);
		}
	}

	public void AddMembers(TroopRoster roster)
	{
		MemberRoster.Add(roster);
	}

	public override string ToString()
	{
		if (!IsSettlement)
		{
			return MobileParty.Name.ToString();
		}
		return Settlement.Name.ToString();
	}

	public void PlaceRandomPositionAroundPosition(Vec2 centerPosition, float radius)
	{
		Vec2 vec = new Vec2(0f, 0f);
		PathFaceRecord faceIndex;
		PathFaceRecord faceIndex2;
		do
		{
			vec.x = centerPosition.x + MBRandom.RandomFloat * radius * 2f - radius;
			vec.y = centerPosition.y + MBRandom.RandomFloat * radius * 2f - radius;
			faceIndex = Campaign.Current.MapSceneWrapper.GetFaceIndex(vec);
			faceIndex2 = Campaign.Current.MapSceneWrapper.GetFaceIndex(centerPosition);
		}
		while (!Campaign.Current.MapSceneWrapper.AreFacesOnSameIsland(faceIndex, faceIndex2, ignoreDisabled: false));
		if (IsMobile)
		{
			MobileParty.Position2D = vec;
			MobileParty.Ai.SetMoveModeHold();
		}
	}

	public int AddElementToMemberRoster(CharacterObject element, int numberToAdd, bool insertAtFront = false)
	{
		return MemberRoster.AddToCounts(element, numberToAdd, insertAtFront);
	}

	public void AddToMemberRosterElementAtIndex(int index, int numberToAdd, int woundedCount = 0)
	{
		MemberRoster.AddToCountsAtIndex(index, numberToAdd, woundedCount);
	}

	public void WoundMemberRosterElements(CharacterObject elementObj, int numberToWound)
	{
		MemberRoster.AddToCounts(elementObj, 0, insertAtFront: false, numberToWound);
	}

	public void WoundMemberRosterElementsWithIndex(int elementIndex, int numberToWound)
	{
		MemberRoster.AddToCountsAtIndex(elementIndex, 0, numberToWound);
	}

	private float CalculateStrength()
	{
		float num = 0f;
		float leaderModifier = 0f;
		MapEvent.PowerCalculationContext context = MapEvent.PowerCalculationContext.Default;
		BattleSideEnum side = BattleSideEnum.Defender;
		if (MapEvent != null)
		{
			leaderModifier = Campaign.Current.Models.MilitaryPowerModel.GetLeaderModifierInMapEvent(MapEvent, Side);
			context = MapEvent.SimulationContext;
		}
		for (int i = 0; i < MemberRoster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = MemberRoster.GetElementCopyAtIndex(i);
			if (elementCopyAtIndex.Character != null)
			{
				float troopPower = Campaign.Current.Models.MilitaryPowerModel.GetTroopPower(elementCopyAtIndex.Character, side, context, leaderModifier);
				num += (float)(elementCopyAtIndex.Number - elementCopyAtIndex.WoundedNumber) * troopPower;
			}
		}
		return num;
	}

	internal bool GetCharacterFromPartyRank(int partyRank, out CharacterObject character, out PartyBase party, out int stackIndex, bool includeWoundeds = false)
	{
		for (int i = 0; i < MemberRoster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = MemberRoster.GetElementCopyAtIndex(i);
			int num = elementCopyAtIndex.Number - ((!includeWoundeds) ? elementCopyAtIndex.WoundedNumber : 0);
			partyRank -= num;
			if (!elementCopyAtIndex.Character.IsHero && partyRank < 0)
			{
				character = elementCopyAtIndex.Character;
				party = this;
				stackIndex = i;
				return true;
			}
		}
		character = null;
		party = null;
		stackIndex = 0;
		return false;
	}

	public static bool IsPositionOkForTraveling(Vec2 position)
	{
		IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
		PathFaceRecord faceIndex = mapSceneWrapper.GetFaceIndex(position);
		if (!faceIndex.IsValid())
		{
			return false;
		}
		TerrainType faceTerrainType = mapSceneWrapper.GetFaceTerrainType(faceIndex);
		return ValidTerrainTypes.Contains(faceTerrainType);
	}

	private void MemberRosterNumberChanged(bool numberChanged, bool woundedNumberChanged, bool heroNumberChanged)
	{
		if (numberChanged || heroNumberChanged)
		{
			CampaignEventDispatcher.Instance.OnPartySizeChanged(this);
		}
	}

	public void UpdateVisibilityAndInspected(float mainPartySeeingRange = 0f)
	{
		bool isVisible = false;
		bool isInspected = false;
		if (IsSettlement)
		{
			isVisible = true;
			if (Settlement.SettlementComponent is ISpottable { IsSpotted: false })
			{
				isVisible = false;
			}
			if (isVisible)
			{
				isInspected = CalculateSettlementInspected(Settlement, mainPartySeeingRange);
			}
		}
		else if (MobileParty.IsActive)
		{
			if (Campaign.Current.TrueSight)
			{
				isVisible = true;
			}
			else if (MobileParty.CurrentSettlement == null || MobileParty.LeaderHero?.ClanBanner != null || (MobileParty.MapEvent != null && MobileParty.MapEvent.IsSiegeAssault && MobileParty.Party.Side == BattleSideEnum.Attacker))
			{
				CalculateVisibilityAndInspected(MobileParty, out isVisible, out isInspected, mainPartySeeingRange);
			}
		}
		if (IsSettlement)
		{
			Settlement.IsVisible = isVisible;
			Settlement.IsInspected = isInspected;
		}
		else
		{
			MobileParty.IsVisible = isVisible;
			MobileParty.IsInspected = isInspected;
		}
	}

	private static void CalculateVisibilityAndInspected(IMapPoint mapPoint, out bool isVisible, out bool isInspected, float mainPartySeeingRange = 0f)
	{
		isInspected = false;
		MobileParty mobileParty = mapPoint as MobileParty;
		if (mobileParty?.Army != null && mobileParty.Army.LeaderParty.AttachedParties.IndexOf(mobileParty) >= 0)
		{
			isVisible = mobileParty.Army.LeaderParty.IsVisible;
			return;
		}
		if (mobileParty != null && MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement.SiegeEvent != null && MobileParty.MainParty.CurrentSettlement.SiegeEvent.BesiegerCamp.IsBesiegerSideParty(mobileParty))
		{
			isVisible = true;
			return;
		}
		float num = CalculateVisibilityRangeOfMapPoint(mapPoint, mainPartySeeingRange);
		isVisible = num > 1f && mapPoint.IsActive;
		if (isVisible)
		{
			if (mapPoint.IsInspected)
			{
				isInspected = true;
			}
			else
			{
				isInspected = 1f / num < Campaign.Current.Models.MapVisibilityModel.GetPartyRelativeInspectionRange(mapPoint);
			}
		}
	}

	private static bool CalculateSettlementInspected(IMapPoint mapPoint, float mainPartySeeingRange = 0f)
	{
		return 1f / CalculateVisibilityRangeOfMapPoint(mapPoint, mainPartySeeingRange) < Campaign.Current.Models.MapVisibilityModel.GetPartyRelativeInspectionRange(mapPoint);
	}

	private static float CalculateVisibilityRangeOfMapPoint(IMapPoint mapPoint, float mainPartySeeingRange)
	{
		MobileParty mainParty = MobileParty.MainParty;
		float lengthSquared = (mainParty.Position2D - mapPoint.Position2D).LengthSquared;
		float num = mainPartySeeingRange;
		if (mainPartySeeingRange == 0f)
		{
			num = mainParty.SeeingRange;
		}
		float num2 = num * num / lengthSquared;
		float num3 = 0.25f;
		if (mapPoint is MobileParty party)
		{
			num3 = Campaign.Current.Models.MapVisibilityModel.GetPartySpottingDifficulty(mainParty, party);
		}
		return num2 / num3;
	}

	public void SetAsCameraFollowParty()
	{
		Campaign.Current.CameraFollowParty = this;
	}

	internal void OnFinishLoadState()
	{
		SetVisualAsDirty();
		MobileParty?.OnFinishLoadState();
		MemberRoster.NumberChangedCallback = MemberRosterNumberChanged;
	}

	public void SetVisualAsDirty()
	{
		IsVisualDirty = true;
	}

	public void OnVisualsUpdated()
	{
		IsVisualDirty = false;
	}

	internal void OnHeroAdded(Hero heroObject)
	{
		MobileParty?.OnHeroAdded(heroObject);
	}

	internal void OnHeroRemoved(Hero heroObject)
	{
		MobileParty?.OnHeroRemoved(heroObject);
	}

	internal void OnHeroAddedAsPrisoner(Hero heroObject)
	{
		heroObject.OnAddedToPartyAsPrisoner(this);
	}

	internal void OnHeroRemovedAsPrisoner(Hero heroObject)
	{
		heroObject.OnRemovedFromPartyAsPrisoner(this);
	}

	public void ResetTempXp()
	{
		MemberRoster.ClearTempXp();
	}

	public void OnGameInitialized()
	{
		if (IsMobile)
		{
			MobileParty.OnGameInitialized();
		}
		else if (IsSettlement)
		{
			Settlement.OnGameInitialized();
		}
	}
}
