using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace TaleWorlds.CampaignSystem.Settlements;

public sealed class Settlement : MBObjectBase, ILocatable<Settlement>, IMapPoint, ITrackableCampaignObject, ITrackableBase, ISiegeEventSide, IMapEntity, IRandomOwner
{
	public enum SiegeState
	{
		OnTheWalls,
		InTheLordsHall,
		Invalid
	}

	[SaveableField(102)]
	public int NumberOfLordPartiesTargeting;

	[CachedData]
	private int _numberOfLordPartiesAt;

	[SaveableField(104)]
	public int CanBeClaimed;

	[SaveableField(105)]
	public float ClaimValue;

	[SaveableField(106)]
	public Hero ClaimedBy;

	[SaveableField(107)]
	public bool HasVisited;

	[SaveableField(110)]
	public float LastVisitTimeOfOwner;

	[SaveableField(113)]
	private bool _isVisible;

	[CachedData]
	private int _locatorNodeIndex;

	[SaveableField(117)]
	private Settlement _nextLocatable;

	[CachedData]
	private float _oldProsperityObsolete = -1f;

	[SaveableField(119)]
	private float _readyMilitia;

	[SaveableField(120)]
	private MBList<float> _settlementWallSectionHitPointsRatioList = new MBList<float>();

	[CachedData]
	private MBList<MobileParty> _partiesCache;

	[CachedData]
	private MBList<Hero> _heroesWithoutPartyCache;

	[CachedData]
	private MBList<Hero> _notablesCache;

	private Vec2 _gatePosition;

	private Vec2 _position;

	public CultureObject Culture;

	private TextObject _name;

	[SaveableField(129)]
	private MBList<Village> _boundVillages;

	[SaveableField(131)]
	private MobileParty _lastAttackerParty;

	[SaveableField(148)]
	private MBList<SiegeEvent.SiegeEngineMissile> _siegeEngineMissiles;

	public Town Town;

	public Village Village;

	public Hideout Hideout;

	[CachedData]
	public MilitiaPartyComponent MilitiaPartyComponent;

	[SaveableField(145)]
	public readonly ItemRoster Stash;

	[SaveableProperty(101)]
	public PartyBase Party { get; private set; }

	public int NumberOfLordPartiesAt => _numberOfLordPartiesAt;

	[SaveableProperty(116)]
	public int BribePaid { get; set; }

	[SaveableProperty(111)]
	public SiegeEvent SiegeEvent { get; set; }

	[SaveableProperty(112)]
	public bool IsActive { get; set; }

	public Hero Owner => OwnerClan.Leader;

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

	[CachedData]
	public bool IsInspected { get; set; }

	public int WallSectionCount
	{
		get
		{
			if (!IsFortification)
			{
				return 0;
			}
			return 2;
		}
	}

	int ILocatable<Settlement>.LocatorNodeIndex
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

	[SaveableProperty(115)]
	public float NumberOfEnemiesSpottedAround { get; set; }

	[SaveableProperty(128)]
	public float NumberOfAlliesSpottedAround { get; set; }

	Settlement ILocatable<Settlement>.NextLocatable
	{
		get
		{
			return _nextLocatable;
		}
		set
		{
			_nextLocatable = value;
		}
	}

	public int RandomValue => Party.RandomValue;

	public Vec2 GetPosition2D => Position2D;

	public float Militia
	{
		get
		{
			return (float)((MilitiaPartyComponent != null && MilitiaPartyComponent.MobileParty.IsActive) ? MilitiaPartyComponent.MobileParty.Party.NumberOfAllMembers : 0) + _readyMilitia;
		}
		set
		{
			int num = ((MilitiaPartyComponent != null && MilitiaPartyComponent.MobileParty.IsActive) ? MilitiaPartyComponent.MobileParty.Party.NumberOfAllMembers : 0);
			_readyMilitia = value - (float)num;
			if (_readyMilitia < (float)(-num))
			{
				_readyMilitia = -num;
			}
			if (_readyMilitia < -1f || _readyMilitia > 1f)
			{
				if (MilitiaPartyComponent != null)
				{
					TransferReadyMilitiasToMilitiaParty();
				}
				else
				{
					SpawnMilitiaParty();
				}
			}
		}
	}

	public MBReadOnlyList<float> SettlementWallSectionHitPointsRatioList => _settlementWallSectionHitPointsRatioList;

	public float SettlementTotalWallHitPoints
	{
		get
		{
			float num = 0f;
			foreach (float settlementWallSectionHitPointsRatio in _settlementWallSectionHitPointsRatioList)
			{
				num += settlementWallSectionHitPointsRatio;
			}
			return num * MaxHitPointsOfOneWallSection;
		}
	}

	public float MaxHitPointsOfOneWallSection
	{
		get
		{
			if (WallSectionCount == 0)
			{
				return 0f;
			}
			return MaxWallHitPoints / (float)WallSectionCount;
		}
	}

	[SaveableProperty(121)]
	public float SettlementHitPoints { get; internal set; }

	public float MaxWallHitPoints => Campaign.Current.Models.WallHitPointCalculationModel.CalculateMaximumWallHitPoint(Town);

	public MBReadOnlyList<MobileParty> Parties => _partiesCache;

	public MBReadOnlyList<Hero> HeroesWithoutParty => _heroesWithoutPartyCache;

	public MBReadOnlyList<Hero> Notables => _notablesCache;

	[SaveableProperty(152)]
	public SettlementComponent SettlementComponent { get; private set; }

	public Vec2 GatePosition
	{
		get
		{
			return _gatePosition;
		}
		private set
		{
			_gatePosition = value;
			Campaign current = Campaign.Current;
			if (current.MapSceneWrapper != null)
			{
				CurrentNavigationFace = current.MapSceneWrapper.GetFaceIndex(_gatePosition);
			}
		}
	}

	public Vec2 Position2D
	{
		get
		{
			return _position;
		}
		private set
		{
			_position = value;
			Campaign.Current.SettlementLocator.UpdateLocator(this);
		}
	}

	public PathFaceRecord CurrentNavigationFace { get; private set; }

	public IFaction MapFaction => Town?.MapFaction ?? Village?.Bound.MapFaction ?? Hideout?.MapFaction ?? null;

	public TextObject Name
	{
		get
		{
			return _name;
		}
		set
		{
			SetName(value);
		}
	}

	public TextObject EncyclopediaText { get; private set; }

	public string EncyclopediaLink => (Campaign.Current.EncyclopediaManager.GetIdentifier(typeof(Settlement)) + "-" + base.StringId) ?? "";

	public TextObject EncyclopediaLinkWithName => HyperlinkTexts.GetSettlementHyperlinkText(EncyclopediaLink, Name);

	[SaveableProperty(122)]
	public int GarrisonWagePaymentLimit { get; private set; }

	public ItemRoster ItemRoster => Party.ItemRoster;

	public MBReadOnlyList<Village> BoundVillages => _boundVillages;

	public MobileParty LastAttackerParty
	{
		get
		{
			return _lastAttackerParty;
		}
		set
		{
			if (_lastAttackerParty == value)
			{
				return;
			}
			_lastAttackerParty = value;
			if (value != null && (IsFortification || IsVillage))
			{
				foreach (Settlement item in All)
				{
					if ((item.IsFortification || item.IsVillage) && item.LastAttackerParty == value)
					{
						item.LastAttackerParty = null;
					}
				}
			}
			_lastAttackerParty = value;
			if (_lastAttackerParty != null)
			{
				LastThreatTime = CampaignTime.Now;
			}
		}
	}

	[SaveableProperty(137)]
	public CampaignTime LastThreatTime { get; private set; }

	[SaveableProperty(149)]
	public SiegeEvent.SiegeEnginesContainer SiegeEngines { get; private set; }

	public MBReadOnlyList<SiegeEvent.SiegeEngineMissile> SiegeEngineMissiles => _siegeEngineMissiles;

	public BattleSideEnum BattleSide => BattleSideEnum.Defender;

	[SaveableProperty(150)]
	public int NumberOfTroopsKilledOnSide { get; private set; }

	[SaveableProperty(151)]
	public SiegeStrategy SiegeStrategy { get; private set; }

	[SaveableProperty(133)]
	public List<Alley> Alleys { get; private set; }

	public bool IsTown
	{
		get
		{
			if (Town != null)
			{
				return Town.IsTown;
			}
			return false;
		}
	}

	public bool IsCastle
	{
		get
		{
			if (Town != null)
			{
				return Town.IsCastle;
			}
			return false;
		}
	}

	public bool IsFortification
	{
		get
		{
			if (!IsTown)
			{
				return IsCastle;
			}
			return true;
		}
	}

	public bool IsVillage => Village != null;

	public bool IsHideout => Hideout != null;

	public bool IsStarving
	{
		get
		{
			if (Town != null)
			{
				return Town.FoodStocks <= 0f;
			}
			return false;
		}
	}

	public bool IsRaided
	{
		get
		{
			if (IsVillage)
			{
				return Village.VillageState == Village.VillageStates.Looted;
			}
			return false;
		}
	}

	public bool InRebelliousState
	{
		get
		{
			if (IsTown || IsCastle)
			{
				return Town.InRebelliousState;
			}
			return false;
		}
	}

	public bool IsUnderRaid
	{
		get
		{
			if (Party.MapEvent != null)
			{
				return Party.MapEvent.IsRaid;
			}
			return false;
		}
	}

	public bool IsUnderSiege => SiegeEvent != null;

	[SaveableProperty(138)]
	public LocationComplex LocationComplex { get; private set; }

	public static Settlement CurrentSettlement
	{
		get
		{
			if (PlayerCaptivity.CaptorParty != null && PlayerCaptivity.CaptorParty.IsSettlement)
			{
				return PlayerCaptivity.CaptorParty.Settlement;
			}
			if (PlayerEncounter.EncounterSettlement != null)
			{
				return PlayerEncounter.EncounterSettlement;
			}
			if (MobileParty.MainParty.CurrentSettlement != null)
			{
				return MobileParty.MainParty.CurrentSettlement;
			}
			return null;
		}
	}

	public static MBReadOnlyList<Settlement> All => Campaign.Current.Settlements;

	public static Settlement GetFirst => All.FirstOrDefault();

	[SaveableProperty(142)]
	public SiegeState CurrentSiegeState { get; private set; }

	public Clan OwnerClan
	{
		get
		{
			if (Village != null)
			{
				return Village.Bound.OwnerClan;
			}
			if (Town != null)
			{
				return Town.OwnerClan;
			}
			if (IsHideout)
			{
				return Hideout.MapFaction as Clan;
			}
			return null;
		}
	}

	public bool IsAlerted => NumberOfEnemiesSpottedAround >= 1f;

	bool IMapEntity.IsMobileEntity => false;

	bool IMapEntity.ShowCircleAroundEntity => true;

	Vec2 IMapEntity.InteractionPosition => GatePosition;

	internal static void AutoGeneratedStaticCollectObjectsSettlement(object o, List<object> collectedObjects)
	{
		((Settlement)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(ClaimedBy);
		collectedObjects.Add(Stash);
		collectedObjects.Add(_nextLocatable);
		collectedObjects.Add(_settlementWallSectionHitPointsRatioList);
		collectedObjects.Add(_boundVillages);
		collectedObjects.Add(_lastAttackerParty);
		collectedObjects.Add(_siegeEngineMissiles);
		collectedObjects.Add(Party);
		collectedObjects.Add(SiegeEvent);
		collectedObjects.Add(SettlementComponent);
		CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime(LastThreatTime, collectedObjects);
		collectedObjects.Add(SiegeEngines);
		collectedObjects.Add(SiegeStrategy);
		collectedObjects.Add(Alleys);
		collectedObjects.Add(LocationComplex);
	}

	internal static object AutoGeneratedGetMemberValueParty(object o)
	{
		return ((Settlement)o).Party;
	}

	internal static object AutoGeneratedGetMemberValueBribePaid(object o)
	{
		return ((Settlement)o).BribePaid;
	}

	internal static object AutoGeneratedGetMemberValueSiegeEvent(object o)
	{
		return ((Settlement)o).SiegeEvent;
	}

	internal static object AutoGeneratedGetMemberValueIsActive(object o)
	{
		return ((Settlement)o).IsActive;
	}

	internal static object AutoGeneratedGetMemberValueNumberOfEnemiesSpottedAround(object o)
	{
		return ((Settlement)o).NumberOfEnemiesSpottedAround;
	}

	internal static object AutoGeneratedGetMemberValueNumberOfAlliesSpottedAround(object o)
	{
		return ((Settlement)o).NumberOfAlliesSpottedAround;
	}

	internal static object AutoGeneratedGetMemberValueSettlementHitPoints(object o)
	{
		return ((Settlement)o).SettlementHitPoints;
	}

	internal static object AutoGeneratedGetMemberValueSettlementComponent(object o)
	{
		return ((Settlement)o).SettlementComponent;
	}

	internal static object AutoGeneratedGetMemberValueGarrisonWagePaymentLimit(object o)
	{
		return ((Settlement)o).GarrisonWagePaymentLimit;
	}

	internal static object AutoGeneratedGetMemberValueLastThreatTime(object o)
	{
		return ((Settlement)o).LastThreatTime;
	}

	internal static object AutoGeneratedGetMemberValueSiegeEngines(object o)
	{
		return ((Settlement)o).SiegeEngines;
	}

	internal static object AutoGeneratedGetMemberValueNumberOfTroopsKilledOnSide(object o)
	{
		return ((Settlement)o).NumberOfTroopsKilledOnSide;
	}

	internal static object AutoGeneratedGetMemberValueSiegeStrategy(object o)
	{
		return ((Settlement)o).SiegeStrategy;
	}

	internal static object AutoGeneratedGetMemberValueAlleys(object o)
	{
		return ((Settlement)o).Alleys;
	}

	internal static object AutoGeneratedGetMemberValueLocationComplex(object o)
	{
		return ((Settlement)o).LocationComplex;
	}

	internal static object AutoGeneratedGetMemberValueCurrentSiegeState(object o)
	{
		return ((Settlement)o).CurrentSiegeState;
	}

	internal static object AutoGeneratedGetMemberValueNumberOfLordPartiesTargeting(object o)
	{
		return ((Settlement)o).NumberOfLordPartiesTargeting;
	}

	internal static object AutoGeneratedGetMemberValueCanBeClaimed(object o)
	{
		return ((Settlement)o).CanBeClaimed;
	}

	internal static object AutoGeneratedGetMemberValueClaimValue(object o)
	{
		return ((Settlement)o).ClaimValue;
	}

	internal static object AutoGeneratedGetMemberValueClaimedBy(object o)
	{
		return ((Settlement)o).ClaimedBy;
	}

	internal static object AutoGeneratedGetMemberValueHasVisited(object o)
	{
		return ((Settlement)o).HasVisited;
	}

	internal static object AutoGeneratedGetMemberValueLastVisitTimeOfOwner(object o)
	{
		return ((Settlement)o).LastVisitTimeOfOwner;
	}

	internal static object AutoGeneratedGetMemberValueStash(object o)
	{
		return ((Settlement)o).Stash;
	}

	internal static object AutoGeneratedGetMemberValue_isVisible(object o)
	{
		return ((Settlement)o)._isVisible;
	}

	internal static object AutoGeneratedGetMemberValue_nextLocatable(object o)
	{
		return ((Settlement)o)._nextLocatable;
	}

	internal static object AutoGeneratedGetMemberValue_readyMilitia(object o)
	{
		return ((Settlement)o)._readyMilitia;
	}

	internal static object AutoGeneratedGetMemberValue_settlementWallSectionHitPointsRatioList(object o)
	{
		return ((Settlement)o)._settlementWallSectionHitPointsRatioList;
	}

	internal static object AutoGeneratedGetMemberValue_boundVillages(object o)
	{
		return ((Settlement)o)._boundVillages;
	}

	internal static object AutoGeneratedGetMemberValue_lastAttackerParty(object o)
	{
		return ((Settlement)o)._lastAttackerParty;
	}

	internal static object AutoGeneratedGetMemberValue_siegeEngineMissiles(object o)
	{
		return ((Settlement)o)._siegeEngineMissiles;
	}

	public void SetWallSectionHitPointsRatioAtIndex(int index, float hitPointsRatio)
	{
		_settlementWallSectionHitPointsRatioList[index] = MBMath.ClampFloat(hitPointsRatio, 0f, 1f);
	}

	public Vec3 GetLogicalPosition()
	{
		float height = 0f;
		Campaign.Current.MapSceneWrapper.GetHeightAtPoint(Position2D, ref height);
		return new Vec3(Position2D.x, Position2D.y, height);
	}

	public void SetGarrisonWagePaymentLimit(int limit)
	{
		GarrisonWagePaymentLimit = limit;
	}

	public IEnumerable<PartyBase> GetInvolvedPartiesForEventType(MapEvent.BattleTypes mapEventType = MapEvent.BattleTypes.Siege)
	{
		return Campaign.Current.Models.EncounterModel.GetDefenderPartiesOfSettlement(this, mapEventType);
	}

	public PartyBase GetNextInvolvedPartyForEventType(ref int partyIndex, MapEvent.BattleTypes mapEventType = MapEvent.BattleTypes.Siege)
	{
		return Campaign.Current.Models.EncounterModel.GetNextDefenderPartyOfSettlement(this, ref partyIndex, mapEventType);
	}

	public bool HasInvolvedPartyForEventType(PartyBase party, MapEvent.BattleTypes mapEventType = MapEvent.BattleTypes.Siege)
	{
		int partyIndex = -1;
		for (PartyBase nextDefenderPartyOfSettlement = Campaign.Current.Models.EncounterModel.GetNextDefenderPartyOfSettlement(this, ref partyIndex, mapEventType); nextDefenderPartyOfSettlement != null; nextDefenderPartyOfSettlement = Campaign.Current.Models.EncounterModel.GetNextDefenderPartyOfSettlement(this, ref partyIndex, mapEventType))
		{
			if (nextDefenderPartyOfSettlement == party)
			{
				return true;
			}
		}
		return false;
	}

	internal void AddBoundVillageInternal(Village village)
	{
		_boundVillages.Add(village);
	}

	internal void RemoveBoundVillageInternal(Village village)
	{
		_boundVillages.Remove(village);
	}

	private void SetName(TextObject name)
	{
		_name = name;
		SetNameAttributes();
	}

	private void SetNameAttributes()
	{
		_name.SetTextVariable("IS_SETTLEMENT", 1);
		_name.SetTextVariable("IS_CASTLE", IsCastle ? 1 : 0);
		_name.SetTextVariable("IS_TOWN", IsTown ? 1 : 0);
		_name.SetTextVariable("IS_HIDEOUT", IsHideout ? 1 : 0);
	}

	private void InitSettlement()
	{
		_partiesCache = new MBList<MobileParty>();
		_heroesWithoutPartyCache = new MBList<Hero>();
		_notablesCache = new MBList<Hero>();
		_boundVillages = new MBList<Village>();
		SettlementHitPoints = 1f;
		CurrentSiegeState = SiegeState.OnTheWalls;
		float currentTime = Campaign.CurrentTime;
		LastVisitTimeOfOwner = currentTime;
	}

	public bool IsUnderRebellionAttack()
	{
		if (Party.MapEvent != null && Party.MapEvent.IsSiegeAssault)
		{
			Hero owner = Party.MapEvent.AttackerSide.LeaderParty.MobileParty.Party.Owner;
			if (owner != null && owner.Clan.IsRebelClan)
			{
				return true;
			}
		}
		return false;
	}

	public Settlement()
		: this(new TextObject("{=!}unnamed"), null, null)
	{
	}

	public Settlement(TextObject name, LocationComplex locationComplex, PartyTemplateObject pt)
	{
		_name = name;
		_isVisible = true;
		IsActive = true;
		Party = new PartyBase(this);
		InitSettlement();
		_position = Vec2.Zero;
		LocationComplex = locationComplex;
		Alleys = new List<Alley>();
		HasVisited = false;
		Stash = new ItemRoster();
	}

	public float GetSettlementValueForEnemyHero(Hero hero)
	{
		return Campaign.Current.Models.SettlementValueModel.CalculateSettlementValueForEnemyHero(this, hero);
	}

	public float GetValue(Hero hero = null, bool countAlsoBoundedSettlements = true)
	{
		float num = 0f;
		if (IsVillage)
		{
			num = 100000f + Village.Hearth * 250f;
			num *= ((Village.VillageState == Village.VillageStates.Looted) ? 0.8f : ((Village.VillageState == Village.VillageStates.BeingRaided) ? 0.85f : (0.8f + (0.667f + 0.333f * Village.Settlement.SettlementHitPoints) * 0.2f)));
		}
		else if (IsCastle)
		{
			num = 250000f + Town.Prosperity * 1000f;
		}
		else if (IsTown)
		{
			num = 750000f + Town.Prosperity * 1000f;
		}
		float num2 = 1f;
		if (hero != null && hero.Clan.Settlements.Count > 0)
		{
			float value = TaleWorlds.Library.MathF.Pow(Campaign.Current.Models.MapDistanceModel.GetDistance(hero.Clan.FactionMidSettlement, this) / Campaign.AverageDistanceBetweenTwoFortifications * 4f, 2f);
			value = TaleWorlds.Library.MathF.Clamp(value, 0f, 100f);
			value -= 16f;
			num2 *= (100f - value) / 100f;
		}
		if (countAlsoBoundedSettlements)
		{
			foreach (Village boundVillage in BoundVillages)
			{
				num += boundVillage.Settlement.GetValue(hero, countAlsoBoundedSettlements: false);
			}
		}
		return num * num2;
	}

	public override TextObject GetName()
	{
		return Name;
	}

	public float GetSettlementValueForFaction(IFaction faction)
	{
		return Campaign.Current.Models.SettlementValueModel.CalculateSettlementValueForFaction(this, faction);
	}

	public override string ToString()
	{
		return Name.ToString();
	}

	internal void AddMobileParty(MobileParty mobileParty)
	{
		if (!_partiesCache.Contains(mobileParty))
		{
			_partiesCache.Add(mobileParty);
			if (mobileParty.IsLordParty)
			{
				_numberOfLordPartiesAt++;
			}
		}
		else
		{
			Debug.FailedAssert("mobileParty is already in mobileParties List!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Settlements\\Settlement.cs", "AddMobileParty", 648);
		}
	}

	internal void RemoveMobileParty(MobileParty mobileParty)
	{
		if (_partiesCache.Contains(mobileParty))
		{
			_partiesCache.Remove(mobileParty);
			if (mobileParty.IsLordParty)
			{
				_numberOfLordPartiesAt--;
			}
		}
		else
		{
			Debug.FailedAssert("mobileParty is not in mobileParties List", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Settlements\\Settlement.cs", "RemoveMobileParty", 667);
		}
	}

	internal void AddHeroWithoutParty(Hero individual)
	{
		if (!_heroesWithoutPartyCache.Contains(individual))
		{
			_heroesWithoutPartyCache.Add(individual);
			CollectNotablesToCache();
		}
		else
		{
			Debug.FailedAssert("Notable is already in Notable List!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Settlements\\Settlement.cs", "AddHeroWithoutParty", 686);
		}
	}

	internal void RemoveHeroWithoutParty(Hero individual)
	{
		if (_heroesWithoutPartyCache.Contains(individual))
		{
			_heroesWithoutPartyCache.Remove(individual);
			CollectNotablesToCache();
		}
		else
		{
			Debug.FailedAssert("Notable is not in Notable List", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Settlements\\Settlement.cs", "RemoveHeroWithoutParty", 700);
		}
	}

	private void CollectNotablesToCache()
	{
		_notablesCache.Clear();
		foreach (Hero item in HeroesWithoutParty)
		{
			if (item.IsNotable)
			{
				_notablesCache.Add(item);
			}
		}
	}

	public override void Deserialize(MBObjectManager objectManager, XmlNode node)
	{
		bool isInitialized = base.IsInitialized;
		base.Deserialize(objectManager, node);
		Name = new TextObject(node.Attributes["name"].Value);
		Position2D = new Vec2((float)Convert.ToDouble(node.Attributes["posX"].Value), (float)Convert.ToDouble(node.Attributes["posY"].Value));
		GatePosition = Position2D;
		if (node.Attributes["gate_posX"] != null)
		{
			GatePosition = new Vec2((float)Convert.ToDouble(node.Attributes["gate_posX"].Value), (float)Convert.ToDouble(node.Attributes["gate_posY"].Value));
		}
		Culture = objectManager.ReadObjectReferenceFromXml<CultureObject>("culture", node);
		EncyclopediaText = ((node.Attributes["text"] != null) ? new TextObject(node.Attributes["text"].Value) : TextObject.Empty);
		if (Campaign.Current != null && Campaign.Current.MapSceneWrapper != null && !Campaign.Current.MapSceneWrapper.GetFaceIndex(Position2D).IsValid())
		{
			Debug.Print(string.Concat("Center position of settlement(", GetName(), ") is invalid"));
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "Components")
			{
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					SetSettlementComponent((SettlementComponent)objectManager.CreateObjectFromXmlNode(childNode2));
				}
			}
			if (childNode.Name == "Locations")
			{
				LocationComplexTemplate complexTemplate = (LocationComplexTemplate)objectManager.ReadObjectReferenceFromXml("complex_template", typeof(LocationComplexTemplate), childNode);
				if (!isInitialized)
				{
					LocationComplex = new LocationComplex(complexTemplate);
				}
				else
				{
					LocationComplex.Initialize(complexTemplate);
				}
				foreach (XmlNode childNode3 in childNode.ChildNodes)
				{
					if (childNode3.Name == "Location")
					{
						Location locationWithId = LocationComplex.GetLocationWithId(childNode3.Attributes["id"].Value);
						if (childNode3.Attributes["max_prosperity"] != null)
						{
							locationWithId.ProsperityMax = int.Parse(childNode3.Attributes["max_prosperity"].Value);
						}
						bool flag = false;
						for (int i = 0; i < 4; i++)
						{
							string name = "scene_name" + ((i > 0) ? ("_" + i) : "");
							string text = ((childNode3.Attributes[name] != null) ? childNode3.Attributes[name].Value : "");
							flag = flag || !string.IsNullOrEmpty(text);
							locationWithId.SetSceneName(i, text);
						}
					}
				}
			}
			if (!(childNode.Name == "CommonAreas"))
			{
				continue;
			}
			int num = 0;
			foreach (XmlNode childNode4 in childNode.ChildNodes)
			{
				if (childNode4.Name == "Area")
				{
					string value = childNode4.Attributes["name"].Value;
					string tag = "alley_" + (num + 1);
					if (!isInitialized)
					{
						Alleys.Add(new Alley(this, tag, new TextObject(value)));
					}
					else
					{
						Alleys[num].Initialize(this, tag, new TextObject(value));
					}
					num++;
				}
			}
			foreach (Alley alley in Alleys)
			{
				foreach (Alley alley2 in Alleys)
				{
				}
			}
		}
		if (!isInitialized)
		{
			Clan clan = objectManager.ReadObjectReferenceFromXml<Clan>("owner", node);
			if (clan != null && Town != null)
			{
				Town.OwnerClan = clan;
			}
		}
		SetNameAttributes();
	}

	public void OnFinishLoadState()
	{
		if (IsFortification)
		{
			foreach (Building building in Town.Buildings)
			{
				if (building.BuildingType.IsDefaultProject && building.CurrentLevel != 1)
				{
					building.CurrentLevel = 1;
				}
			}
		}
		Party.UpdateVisibilityAndInspected();
	}

	public void OnGameInitialized()
	{
		Campaign current = Campaign.Current;
		CurrentNavigationFace = current.MapSceneWrapper.GetFaceIndex(GatePosition);
	}

	public void OnGameCreated()
	{
		SettlementComponent?.OnInit();
		CreateFigure();
		Party.SetLevelMaskIsDirty();
		for (int i = 0; i < WallSectionCount; i++)
		{
			_settlementWallSectionHitPointsRatioList.Add(1f);
		}
	}

	public void OnSessionStart()
	{
		Party?.SetVisualAsDirty();
	}

	[LoadInitializationCallback]
	private void OnLoad()
	{
		((ILocatable<Settlement>)this).LocatorNodeIndex = -1;
		_partiesCache = new MBList<MobileParty>();
		_heroesWithoutPartyCache = new MBList<Hero>();
		_notablesCache = new MBList<Hero>();
	}

	[LateLoadInitializationCallback]
	private void OnLateLoad(MetaData metaData, ObjectLoadData objectLoadData)
	{
		if (MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.2.0"))
		{
			_oldProsperityObsolete = (float)objectLoadData.GetMemberValueBySaveId(118);
		}
	}

	public static Settlement Find(string idString)
	{
		return MBObjectManager.Instance.GetObject<Settlement>(idString);
	}

	public static Settlement FindFirst(Func<Settlement, bool> predicate)
	{
		return All.FirstOrDefault(predicate);
	}

	public static IEnumerable<Settlement> FindAll(Func<Settlement, bool> predicate)
	{
		return All.Where(predicate);
	}

	public static LocatableSearchData<Settlement> StartFindingLocatablesAroundPosition(Vec2 position, float radius)
	{
		return Campaign.Current.SettlementLocator.StartFindingLocatablesAroundPosition(position, radius);
	}

	public static Settlement FindNextLocatable(ref LocatableSearchData<Settlement> data)
	{
		return Campaign.Current.SettlementLocator.FindNextLocatable(ref data);
	}

	public void OnPlayerEncounterFinish()
	{
		LocationComplex?.ClearTempCharacters();
	}

	TextObject ITrackableBase.GetName()
	{
		return Name;
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
		if (!Notables.Any((Hero t) => t.CharacterObject == basicCharacter) && !Party.PrisonRoster.GetTroopRoster().Any((TroopRosterElement t) => t.Character == basicCharacter))
		{
			return Parties.Any((MobileParty p) => p.CheckTracked(basicCharacter));
		}
		return true;
	}

	private void CreateFigure()
	{
	}

	public void SetNextSiegeState()
	{
		if (CurrentSiegeState != SiegeState.InTheLordsHall)
		{
			CurrentSiegeState++;
		}
	}

	public void ResetSiegeState()
	{
		CurrentSiegeState = SiegeState.OnTheWalls;
	}

	public void AddGarrisonParty(bool addInitialGarrison = false)
	{
		GarrisonPartyComponent.CreateGarrisonParty("garrison_party_" + base.StringId + "_" + OwnerClan.StringId + "_1", this, addInitialGarrison);
	}

	protected override void AfterLoad()
	{
		if (SiegeEvent != null && SiegeEvent.BesiegerCamp.LeaderParty == null)
		{
			SiegeEvent.FinalizeSiegeEvent();
		}
		_notablesCache = new MBList<Hero>();
		CollectNotablesToCache();
		Party.AfterLoad();
		foreach (Alley alley in Alleys)
		{
			alley.AfterLoad();
		}
		if (Town != null && _oldProsperityObsolete > 0f)
		{
			Town.Prosperity = _oldProsperityObsolete;
		}
	}

	private void SpawnMilitiaParty()
	{
		MilitiaPartyComponent.CreateMilitiaParty("militias_of_" + base.StringId + "_aaa1", this);
		TransferReadyMilitiasToMilitiaParty();
	}

	private void TransferReadyMilitiasToMilitiaParty()
	{
		if (_readyMilitia >= 1f)
		{
			int num = TaleWorlds.Library.MathF.Floor(_readyMilitia);
			_readyMilitia -= num;
			AddMilitiasToParty(MilitiaPartyComponent.MobileParty, num);
		}
		else if ((int)_readyMilitia < -1)
		{
			int num2 = TaleWorlds.Library.MathF.Ceiling(_readyMilitia);
			_readyMilitia -= num2;
			RemoveMilitiasFromParty(MilitiaPartyComponent.MobileParty, -num2);
		}
	}

	private void AddMilitiasToParty(MobileParty militaParty, int militiaToAdd)
	{
		Campaign.Current.Models.SettlementMilitiaModel.CalculateMilitiaSpawnRate(this, out var meleeTroopRate, out var _);
		AddTroopToMilitiaParty(militaParty, Culture.MeleeMilitiaTroop, Culture.MeleeEliteMilitiaTroop, meleeTroopRate, ref militiaToAdd);
		AddTroopToMilitiaParty(militaParty, Culture.RangedMilitiaTroop, Culture.RangedEliteMilitiaTroop, 1f, ref militiaToAdd);
	}

	private void AddTroopToMilitiaParty(MobileParty militaParty, CharacterObject militiaTroop, CharacterObject eliteMilitiaTroop, float troopRatio, ref int numberToAddRemaining)
	{
		if (numberToAddRemaining <= 0)
		{
			return;
		}
		int num = MBRandom.RoundRandomized(troopRatio * (float)numberToAddRemaining);
		float num2 = Campaign.Current.Models.SettlementMilitiaModel.CalculateEliteMilitiaSpawnChance(this);
		for (int i = 0; i < num; i++)
		{
			if (MBRandom.RandomFloat < num2)
			{
				militaParty.MemberRoster.AddToCounts(eliteMilitiaTroop, 1);
			}
			else
			{
				militaParty.MemberRoster.AddToCounts(militiaTroop, 1);
			}
		}
		numberToAddRemaining -= num;
	}

	private static void RemoveMilitiasFromParty(MobileParty militaParty, int numberToRemove)
	{
		if (militaParty.MemberRoster.TotalManCount <= numberToRemove)
		{
			militaParty.MemberRoster.Clear();
			return;
		}
		float num = (float)numberToRemove / (float)militaParty.MemberRoster.TotalManCount;
		int num2 = numberToRemove;
		for (int i = 0; i < militaParty.MemberRoster.Count; i++)
		{
			int num3 = MBRandom.RoundRandomized((float)militaParty.MemberRoster.GetElementNumber(i) * num);
			if (num3 > num2)
			{
				num3 = num2;
			}
			militaParty.MemberRoster.AddToCountsAtIndex(i, -num3, 0, 0, removeDepleted: false);
			num2 -= num3;
			if (num2 <= 0)
			{
				break;
			}
		}
		militaParty.MemberRoster.RemoveZeroCounts();
	}

	public void SetSiegeStrategy(SiegeStrategy strategy)
	{
		SiegeStrategy = strategy;
	}

	public void InitializeSiegeEventSide()
	{
		SiegeStrategy = DefaultSiegeStrategies.Custom;
		NumberOfTroopsKilledOnSide = 0;
		SiegeEvent.SiegeEngineConstructionProgress siegePreparations = null;
		SiegeEngines = new SiegeEvent.SiegeEnginesContainer(BattleSideEnum.Defender, siegePreparations);
		_siegeEngineMissiles = new MBList<SiegeEvent.SiegeEngineMissile>();
		SetPrebuiltSiegeEngines();
	}

	public void OnTroopsKilledOnSide(int killCount)
	{
		NumberOfTroopsKilledOnSide += killCount;
	}

	public void AddSiegeEngineMissile(SiegeEvent.SiegeEngineMissile missile)
	{
		_siegeEngineMissiles.Add(missile);
	}

	public void RemoveDeprecatedMissiles()
	{
		_siegeEngineMissiles.RemoveAll((SiegeEvent.SiegeEngineMissile missile) => missile.CollisionTime.IsPast);
	}

	private void SetPrebuiltSiegeEngines()
	{
		foreach (SiegeEngineType item in Campaign.Current.Models.SiegeEventModel.GetPrebuiltSiegeEnginesOfSettlement(this))
		{
			float siegeEngineHitPoints = Campaign.Current.Models.SiegeEventModel.GetSiegeEngineHitPoints(SiegeEvent, item, BattleSideEnum.Defender);
			SiegeEvent.SiegeEngineConstructionProgress siegeEngineConstructionProgress = new SiegeEvent.SiegeEngineConstructionProgress(item, 1f, siegeEngineHitPoints);
			SiegeEngines.AddPrebuiltEngineToReserve(siegeEngineConstructionProgress);
			SiegeEvent.CreateSiegeObject(siegeEngineConstructionProgress, SiegeEvent.GetSiegeEventSide(BattleSideEnum.Defender));
		}
	}

	public void GetAttackTarget(ISiegeEventSide siegeEventSide, SiegeEngineType siegeEngine, int siegeEngineSlot, out SiegeBombardTargets targetType, out int targetIndex)
	{
		targetType = SiegeBombardTargets.None;
		targetIndex = -1;
		SiegeEvent.FindAttackableRangedEngineWithHighestPriority(siegeEventSide, siegeEngineSlot, out var targetIndex2, out var targetPriority);
		if (targetIndex2 != -1)
		{
			float num = targetPriority;
			if (MBRandom.RandomFloat * num < targetPriority)
			{
				targetIndex = targetIndex2;
				targetType = SiegeBombardTargets.RangedEngines;
			}
		}
	}

	public void FinalizeSiegeEvent()
	{
		ResetSiegeState();
		SiegeEvent = null;
		Party.SetLevelMaskIsDirty();
		Party.SetVisualAsDirty();
	}

	bool IMapEntity.OnMapClick(bool followModifierUsed)
	{
		if (IsVisible)
		{
			MobileParty.MainParty.Ai.SetMoveGoToSettlement(this);
			return true;
		}
		return false;
	}

	void IMapEntity.OnOpenEncyclopedia()
	{
		Campaign.Current.EncyclopediaManager.GoToLink(EncyclopediaLink);
	}

	void IMapEntity.OnHover()
	{
		InformationManager.ShowTooltip(typeof(Settlement), this, false);
	}

	bool IMapEntity.IsEnemyOf(IFaction faction)
	{
		return FactionManager.IsAtWarAgainstFaction(MapFaction, faction);
	}

	bool IMapEntity.IsAllyOf(IFaction faction)
	{
		return FactionManager.IsAlliedWithFaction(MapFaction, faction);
	}

	public void OnPartyInteraction(MobileParty mobileParty)
	{
		if (mobileParty.ShortTermTargetSettlement != null && (mobileParty.ShortTermTargetSettlement.Party.SiegeEvent == null || mobileParty == MobileParty.MainParty || mobileParty.MapFaction == mobileParty.ShortTermTargetSettlement.SiegeEvent.BesiegerCamp.LeaderParty.MapFaction) && (mobileParty.ShortTermTargetSettlement.Party.MapEvent == null || mobileParty == MobileParty.MainParty || mobileParty.MapFaction == mobileParty.ShortTermTargetSettlement.Party.MapEvent.AttackerSide.LeaderParty.MapFaction || (mobileParty.ShortTermTargetSettlement.Party.MapEvent.IsSallyOut && mobileParty.MapFaction == mobileParty.ShortTermTargetSettlement.Party.MapEvent.DefenderSide.LeaderParty.MapFaction)))
		{
			if (mobileParty == MobileParty.MainParty && (mobileParty.ShortTermTargetSettlement.Party.MapEvent == null || !mobileParty.ShortTermTargetSettlement.Party.MapEvent.IsRaid || mobileParty.ShortTermTargetSettlement.Party.MapEvent.DefenderSide.NumRemainingSimulationTroops > 0))
			{
				(Game.Current.GameStateManager.ActiveState as MapState)?.OnMainPartyEncounter();
			}
			if (mobileParty.ShortTermTargetSettlement.Party.MapEvent != null && mobileParty.ShortTermTargetSettlement.Party.MapEvent.IsRaid && mobileParty.DefaultBehavior == AiBehavior.RaidSettlement)
			{
				mobileParty.Ai.RethinkAtNextHourlyTick = true;
				mobileParty.Ai.SetMoveModeHold();
			}
			else
			{
				EncounterManager.StartSettlementEncounter(mobileParty, mobileParty.ShortTermTargetSettlement);
			}
		}
	}

	public void GetMountAndHarnessVisualIdsForPartyIcon(out string mountStringId, out string harnessStringId)
	{
		mountStringId = "";
		harnessStringId = "";
	}

	public void SetSettlementComponent(SettlementComponent settlementComponent)
	{
		settlementComponent.Owner = Party;
		SettlementComponent = settlementComponent;
		if (SettlementComponent is Town)
		{
			Town = SettlementComponent as Town;
		}
		else if (SettlementComponent is Village)
		{
			Village = SettlementComponent as Village;
		}
		else if (SettlementComponent is Hideout)
		{
			Hideout = SettlementComponent as Hideout;
		}
	}
}
