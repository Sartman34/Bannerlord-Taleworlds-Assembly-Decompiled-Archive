using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace TaleWorlds.CampaignSystem.MapEvents;

public sealed class MapEvent : MBObjectBase, IMapEntity
{
	public enum BattleTypes
	{
		None,
		FieldBattle,
		Raid,
		IsForcingVolunteers,
		IsForcingSupplies,
		Siege,
		Hideout,
		SallyOut,
		SiegeOutside
	}

	public enum PowerCalculationContext
	{
		Default,
		PlainBattle,
		SteppeBattle,
		DesertBattle,
		DuneBattle,
		SnowBattle,
		ForestBattle,
		RiverCrossingBattle,
		Village,
		Siege
	}

	private const float BattleRetreatMinimumTime = 1f;

	private const float SiegeDefenderAdvantage = 2f;

	private const int MapEventSettlementSettingDistance = 3;

	[SaveableField(101)]
	private MapEventState _state;

	[SaveableField(102)]
	private MapEventSide[] _sides = new MapEventSide[2];

	public const float SiegeAdvantage = 1.5f;

	public bool DiplomaticallyFinished;

	[SaveableField(106)]
	private int _mapEventUpdateCount;

	[CachedData]
	internal PowerCalculationContext SimulationContext;

	[SaveableField(107)]
	private CampaignTime _nextSimulationTime;

	[SaveableField(108)]
	private CampaignTime _mapEventStartTime;

	[SaveableField(110)]
	private BattleTypes _mapEventType;

	[CachedData]
	private TerrainType _eventTerrainType;

	[CachedData]
	public IMapEventVisual MapEventVisual;

	[SaveableField(114)]
	private bool _isVisible;

	private bool _keepSiegeEvent;

	[SaveableField(116)]
	private bool FirstUpdateIsDone;

	[SaveableField(117)]
	private BattleState _battleState;

	private bool _isFinishCalled;

	private bool _battleResultsCalculated;

	private bool _battleResultsCommitted;

	private bool PlayerCaptured;

	private MapEventResultExplainer _battleResultExplainers;

	[SaveableField(125)]
	public float[] StrengthOfSide = new float[2];

	public static MapEvent PlayerMapEvent => MobileParty.MainParty?.MapEvent;

	public BattleSideEnum PlayerSide => PartyBase.MainParty.Side;

	internal IBattleObserver BattleObserver { get; set; }

	[SaveableProperty(105)]
	public MapEventComponent Component { get; private set; }

	public MapEventState State
	{
		get
		{
			return _state;
		}
		private set
		{
			if (_state != value)
			{
				if (IsPlayerMapEvent)
				{
					Debug.Print("Player MapEvent State: " + value);
				}
				_state = value;
			}
		}
	}

	public MapEventSide AttackerSide => _sides[1];

	public MapEventSide DefenderSide => _sides[0];

	public IEnumerable<PartyBase> InvolvedParties
	{
		get
		{
			MapEventSide[] sides = _sides;
			foreach (MapEventSide mapEventSide in sides)
			{
				foreach (MapEventParty party in mapEventSide.Parties)
				{
					yield return party.Party;
				}
			}
		}
	}

	[SaveableProperty(103)]
	public Settlement MapEventSettlement { get; private set; }

	internal bool AttackersRanAway { get; private set; }

	[SaveableProperty(111)]
	public Vec2 Position { get; private set; }

	public BattleTypes EventType => _mapEventType;

	public TerrainType EventTerrainType => _eventTerrainType;

	[SaveableProperty(113)]
	public bool IsInvulnerable { get; set; }

	public bool IsFieldBattle => _mapEventType == BattleTypes.FieldBattle;

	public bool IsRaid => _mapEventType == BattleTypes.Raid;

	public bool IsForcingVolunteers => _mapEventType == BattleTypes.IsForcingVolunteers;

	public bool IsForcingSupplies => _mapEventType == BattleTypes.IsForcingSupplies;

	public bool IsSiegeAssault => _mapEventType == BattleTypes.Siege;

	public bool IsHideoutBattle => _mapEventType == BattleTypes.Hideout;

	public bool IsSallyOut => _mapEventType == BattleTypes.SallyOut;

	public bool IsSiegeOutside => _mapEventType == BattleTypes.SiegeOutside;

	public bool IsSiegeAmbush => Component is SiegeAmbushEventComponent;

	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		private set
		{
			_isVisible = value;
			MapEventVisual?.SetVisibility(value);
		}
	}

	public bool IsPlayerMapEvent => this == PlayerMapEvent;

	public bool IsFinished => _state == MapEventState.WaitingRemoval;

	public BattleState BattleState
	{
		get
		{
			return _battleState;
		}
		internal set
		{
			if (value != _battleState)
			{
				if (IsPlayerMapEvent)
				{
					Debug.Print("Player MapEvent BattleState: " + value);
				}
				_battleState = value;
				if (_battleState == BattleState.AttackerVictory || _battleState == BattleState.DefenderVictory)
				{
					OnBattleWon(_battleState);
				}
			}
		}
	}

	public MapEventSide Winner
	{
		get
		{
			if (BattleState != BattleState.AttackerVictory)
			{
				if (BattleState != BattleState.DefenderVictory)
				{
					return null;
				}
				return DefenderSide;
			}
			return AttackerSide;
		}
	}

	public BattleSideEnum WinningSide
	{
		get
		{
			if (BattleState != BattleState.AttackerVictory)
			{
				if (BattleState != BattleState.DefenderVictory)
				{
					return BattleSideEnum.None;
				}
				return BattleSideEnum.Defender;
			}
			return BattleSideEnum.Attacker;
		}
	}

	public BattleSideEnum DefeatedSide
	{
		get
		{
			if (BattleState != BattleState.AttackerVictory)
			{
				if (BattleState != BattleState.DefenderVictory)
				{
					return BattleSideEnum.None;
				}
				return BattleSideEnum.Attacker;
			}
			return BattleSideEnum.Defender;
		}
	}

	public MapEventResultExplainer BattleResultExplainers => _battleResultExplainers;

	public bool IsFinalized => _state == MapEventState.WaitingRemoval;

	public CampaignTime BattleStartTime => _mapEventStartTime;

	public bool HasWinner
	{
		get
		{
			if (BattleState != BattleState.AttackerVictory)
			{
				return BattleState == BattleState.DefenderVictory;
			}
			return true;
		}
	}

	[SaveableProperty(123)]
	public bool IsPlayerSimulation { get; set; }

	Vec2 IMapEntity.InteractionPosition => Position;

	TextObject IMapEntity.Name => GetName();

	bool IMapEntity.IsMobileEntity => false;

	bool IMapEntity.ShowCircleAroundEntity => false;

	internal static void AutoGeneratedStaticCollectObjectsMapEvent(object o, List<object> collectedObjects)
	{
		((MapEvent)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(StrengthOfSide);
		collectedObjects.Add(_sides);
		CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime(_nextSimulationTime, collectedObjects);
		CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime(_mapEventStartTime, collectedObjects);
		collectedObjects.Add(Component);
		collectedObjects.Add(MapEventSettlement);
	}

	internal static object AutoGeneratedGetMemberValueComponent(object o)
	{
		return ((MapEvent)o).Component;
	}

	internal static object AutoGeneratedGetMemberValueMapEventSettlement(object o)
	{
		return ((MapEvent)o).MapEventSettlement;
	}

	internal static object AutoGeneratedGetMemberValuePosition(object o)
	{
		return ((MapEvent)o).Position;
	}

	internal static object AutoGeneratedGetMemberValueIsInvulnerable(object o)
	{
		return ((MapEvent)o).IsInvulnerable;
	}

	internal static object AutoGeneratedGetMemberValueIsPlayerSimulation(object o)
	{
		return ((MapEvent)o).IsPlayerSimulation;
	}

	internal static object AutoGeneratedGetMemberValueStrengthOfSide(object o)
	{
		return ((MapEvent)o).StrengthOfSide;
	}

	internal static object AutoGeneratedGetMemberValue_state(object o)
	{
		return ((MapEvent)o)._state;
	}

	internal static object AutoGeneratedGetMemberValue_sides(object o)
	{
		return ((MapEvent)o)._sides;
	}

	internal static object AutoGeneratedGetMemberValue_mapEventUpdateCount(object o)
	{
		return ((MapEvent)o)._mapEventUpdateCount;
	}

	internal static object AutoGeneratedGetMemberValue_nextSimulationTime(object o)
	{
		return ((MapEvent)o)._nextSimulationTime;
	}

	internal static object AutoGeneratedGetMemberValue_mapEventStartTime(object o)
	{
		return ((MapEvent)o)._mapEventStartTime;
	}

	internal static object AutoGeneratedGetMemberValue_mapEventType(object o)
	{
		return ((MapEvent)o)._mapEventType;
	}

	internal static object AutoGeneratedGetMemberValue_isVisible(object o)
	{
		return ((MapEvent)o)._isVisible;
	}

	internal static object AutoGeneratedGetMemberValueFirstUpdateIsDone(object o)
	{
		return ((MapEvent)o).FirstUpdateIsDone;
	}

	internal static object AutoGeneratedGetMemberValue_battleState(object o)
	{
		return ((MapEvent)o)._battleState;
	}

	public void BeginWait()
	{
		State = MapEventState.Wait;
	}

	public MapEventSide GetMapEventSide(BattleSideEnum side)
	{
		return _sides[(int)side];
	}

	internal TroopRoster GetMemberRosterReceivingLootShare(PartyBase party)
	{
		return _sides[(int)party.Side].MemberRosterForPlayerLootShare(party);
	}

	internal TroopRoster GetPrisonerRosterReceivingLootShare(PartyBase party)
	{
		return _sides[(int)party.Side].PrisonerRosterForPlayerLootShare(party);
	}

	internal ItemRoster GetItemRosterReceivingLootShare(PartyBase party)
	{
		return _sides[(int)party.Side].ItemRosterForPlayerLootShare(party);
	}

	public MBReadOnlyList<MapEventParty> PartiesOnSide(BattleSideEnum side)
	{
		return _sides[(int)side].Parties;
	}

	public void GetBattleRewards(PartyBase party, out float renownChange, out float influenceChange, out float moraleChange, out float goldChange, out float playerEarnedLootPercentage)
	{
		renownChange = 0f;
		influenceChange = 0f;
		moraleChange = 0f;
		goldChange = 0f;
		playerEarnedLootPercentage = 0f;
		MapEventSide[] sides = _sides;
		for (int i = 0; i < sides.Length; i++)
		{
			foreach (MapEventParty party2 in sides[i].Parties)
			{
				if (party == party2.Party)
				{
					renownChange = party2.GainedRenown;
					influenceChange = party2.GainedInfluence;
					moraleChange = party2.MoraleChange;
					goldChange = party2.PlunderedGold - party2.GoldLost;
					float num = GetMapEventSide(party2.Party.Side).CalculateTotalContribution();
					playerEarnedLootPercentage = (int)(100f * ((float)party2.ContributionToBattle / num));
				}
			}
		}
	}

	internal MapEvent()
	{
		MapEventVisual = Campaign.Current.VisualCreator.CreateMapEventVisual(this);
	}

	[LateLoadInitializationCallback]
	private void OnLateLoad(MetaData metaData, ObjectLoadData objectLoadData)
	{
		if (Component == null && MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.1.0"))
		{
			if (_mapEventType == BattleTypes.Raid)
			{
				float nextSettlementDamage = (float)objectLoadData.GetMemberValueBySaveId(109);
				int lootedItemCount = (int)objectLoadData.GetMemberValueBySaveId(112);
				float raidDamage = (float)objectLoadData.GetMemberValueBySaveId(115);
				Component = RaidEventComponent.CreateComponentForOldSaves(this, nextSettlementDamage, lootedItemCount, raidDamage);
			}
			else if (_mapEventType == BattleTypes.IsForcingSupplies)
			{
				Component = ForceSuppliesEventComponent.CreateComponentForOldSaves(this);
			}
			else if (_mapEventType == BattleTypes.IsForcingVolunteers)
			{
				Component = ForceVolunteersEventComponent.CreateComponentForOldSaves(this);
			}
			else if (_mapEventType == BattleTypes.IsForcingVolunteers)
			{
				Component = HideoutEventComponent.CreateComponentForOldSaves(this);
			}
			else if (_mapEventType == BattleTypes.FieldBattle)
			{
				Component = FieldBattleEventComponent.CreateComponentForOldSaves(this);
			}
		}
		Component?.AfterLoad(this);
	}

	internal void OnAfterLoad()
	{
		CacheSimulationData();
		_eventTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(Campaign.Current.MapSceneWrapper.GetFaceIndex(Position));
		if (!PartyBase.IsPositionOkForTraveling(Position))
		{
			Vec2 vec = CalculateMapEventPosition(AttackerSide.LeaderParty, DefenderSide.LeaderParty);
			if (vec != Position)
			{
				Vec2 vec2 = vec - Position;
				foreach (PartyBase involvedParty in InvolvedParties)
				{
					if (involvedParty.IsMobile && involvedParty.MobileParty.EventPositionAdder.IsNonZero())
					{
						involvedParty.MobileParty.EventPositionAdder += vec2;
					}
				}
				Position = vec;
			}
		}
		if (!IsFinalized)
		{
			MapEventVisual = Campaign.Current.VisualCreator.CreateMapEventVisual(this);
			MapEventVisual.Initialize(Position, GetBattleSizeValue(), AttackerSide.LeaderParty != PartyBase.MainParty && DefenderSide.LeaderParty != PartyBase.MainParty, IsVisible);
		}
		if (!MBSaveLoad.IsUpdatingGameVersion || !(MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.2.0")))
		{
			return;
		}
		if (!AttackerSide.Parties.Any() || !DefenderSide.Parties.Any())
		{
			if (InvolvedParties.ContainsQ(PlayerEncounter.EncounteredParty))
			{
				PlayerEncounter.Finish();
			}
			FinalizeEvent();
		}
		if (MapEventSettlement != null)
		{
			if (IsRaid && MapEventSettlement.Party.MapEvent == null)
			{
				FinalizeEvent();
			}
			else if (EventType == BattleTypes.Siege && MapEventSettlement.SiegeEvent == null)
			{
				FinalizeEvent();
			}
		}
	}

	internal void Initialize(PartyBase attackerParty, PartyBase defenderParty, MapEventComponent component = null, BattleTypes mapEventType = BattleTypes.None)
	{
		Component = component;
		FirstUpdateIsDone = false;
		AttackersRanAway = false;
		MapEventSettlement = null;
		_mapEventType = mapEventType;
		_mapEventUpdateCount = 0;
		_sides[0] = new MapEventSide(this, BattleSideEnum.Defender, defenderParty);
		_sides[1] = new MapEventSide(this, BattleSideEnum.Attacker, attackerParty);
		if (attackerParty.MobileParty == MobileParty.MainParty || defenderParty.MobileParty == MobileParty.MainParty)
		{
			if (mapEventType == BattleTypes.Raid)
			{
				Debug.Print(string.Concat("A raid mapEvent has been started on ", defenderParty.Name, "\n"), 0, Debug.DebugColor.DarkGreen, 64uL);
			}
			else if (defenderParty.IsSettlement && defenderParty.Settlement.IsFortification)
			{
				Debug.Print(string.Concat("A siege mapEvent has been started on ", defenderParty.Name, "\n"), 0, Debug.DebugColor.DarkCyan, 64uL);
			}
		}
		if (attackerParty.IsMobile && attackerParty.MobileParty.CurrentSettlement != null)
		{
			MapEventSettlement = attackerParty.MobileParty.CurrentSettlement;
		}
		else if (defenderParty.IsMobile && defenderParty.MobileParty.CurrentSettlement != null)
		{
			MapEventSettlement = defenderParty.MobileParty.CurrentSettlement;
		}
		else if ((!attackerParty.IsMobile || attackerParty.MobileParty.BesiegedSettlement == null) && defenderParty.IsMobile)
		{
			_ = defenderParty.MobileParty.BesiegedSettlement;
		}
		if (attackerParty.IsSettlement)
		{
			MapEventSettlement = attackerParty.Settlement;
		}
		else if (defenderParty.IsSettlement)
		{
			MapEventSettlement = defenderParty.Settlement;
			MapEventSettlement.LastAttackerParty = attackerParty.MobileParty;
		}
		if (IsFieldBattle)
		{
			MapEventSettlement = null;
			if (attackerParty == PartyBase.MainParty || defenderParty == PartyBase.MainParty)
			{
				Settlement settlement = SettlementHelper.FindNearestVillage((Settlement x) => x.Position2D.DistanceSquared(attackerParty.Position2D) < 9f);
				if (settlement != null)
				{
					MapEventSettlement = settlement;
				}
			}
		}
		Position = CalculateMapEventPosition(attackerParty, defenderParty);
		_eventTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(Campaign.Current.MapSceneWrapper.GetFaceIndex(Position));
		CacheSimulationData();
		attackerParty.MapEventSide = AttackerSide;
		defenderParty.MapEventSide = DefenderSide;
		if (MapEventSettlement != null && (mapEventType == BattleTypes.Siege || mapEventType == BattleTypes.SiegeOutside || mapEventType == BattleTypes.SallyOut || IsSiegeAmbush))
		{
			foreach (PartyBase item in MapEventSettlement.SiegeEvent.BesiegerCamp.GetInvolvedPartiesForEventType(mapEventType))
			{
				if (item.MapEventSide == null && (item != PartyBase.MainParty || item.MobileParty.Army != null) && (item.MobileParty.Army == null || item.MobileParty.Army.LeaderParty == item.MobileParty))
				{
					item.MapEventSide = ((mapEventType == BattleTypes.SallyOut) ? defenderParty.MapEventSide : attackerParty.MapEventSide);
				}
			}
		}
		if (defenderParty.IsMobile && defenderParty.MobileParty.BesiegedSettlement != null)
		{
			List<PartyBase> involvedPartiesForEventType = defenderParty.MobileParty.SiegeEvent.GetInvolvedPartiesForEventType(_mapEventType);
			PartyBase partyBase = (IsSiegeAssault ? attackerParty : defenderParty);
			foreach (PartyBase item2 in involvedPartiesForEventType)
			{
				if (item2 != partyBase && item2.IsMobile && item2 != PartyBase.MainParty && item2.MobileParty.BesiegedSettlement == defenderParty.MobileParty.BesiegedSettlement && (item2.MobileParty.Army == null || item2.MobileParty.Army.LeaderParty == item2.MobileParty))
				{
					item2.MapEventSide = DefenderSide;
				}
			}
		}
		State = MapEventState.Wait;
		_mapEventStartTime = CampaignTime.Now;
		_nextSimulationTime = CalculateNextSimulationTime();
		Component?.InitializeComponent();
		if (MapEventSettlement != null)
		{
			AddInsideSettlementParties(MapEventSettlement);
		}
		MapEventVisual.Initialize(Position, GetBattleSizeValue(), AttackerSide.LeaderParty != PartyBase.MainParty && DefenderSide.LeaderParty != PartyBase.MainParty, IsVisible);
		BattleState = BattleState.None;
		CampaignEventDispatcher.Instance.OnMapEventStarted(this, attackerParty, defenderParty);
	}

	private Vec2 CalculateMapEventPosition(PartyBase attackerParty, PartyBase defenderParty)
	{
		Vec2 result;
		if (defenderParty.IsSettlement)
		{
			result = defenderParty.Position2D;
		}
		else
		{
			result = attackerParty.Position2D + defenderParty.Position2D;
			result = new Vec2(result.x / 2f, result.y / 2f);
		}
		return result;
	}

	internal bool IsWinnerSide(BattleSideEnum side)
	{
		if (BattleState != BattleState.DefenderVictory || side != 0)
		{
			if (BattleState == BattleState.AttackerVictory)
			{
				return side == BattleSideEnum.Attacker;
			}
			return false;
		}
		return true;
	}

	private void AddInsideSettlementParties(Settlement relatedSettlement)
	{
		List<PartyBase> list = new List<PartyBase>();
		foreach (PartyBase item in relatedSettlement.GetInvolvedPartiesForEventType(_mapEventType))
		{
			if (item != PartyBase.MainParty && item.MobileParty?.AttachedTo != MobileParty.MainParty)
			{
				list.Add(item);
			}
		}
		foreach (PartyBase item2 in list)
		{
			if (MapEventSettlement.SiegeEvent != null)
			{
				if (MapEventSettlement.SiegeEvent.CanPartyJoinSide(item2, BattleSideEnum.Defender))
				{
					if (IsSallyOut)
					{
						item2.MapEventSide = AttackerSide;
					}
					else
					{
						item2.MapEventSide = DefenderSide;
					}
				}
				else if (item2.MobileParty != null && !item2.MobileParty.IsGarrison && !item2.MobileParty.IsMilitia)
				{
					LeaveSettlementAction.ApplyForParty(item2.MobileParty);
					item2.MobileParty.Ai.SetMoveModeHold();
				}
			}
			else if (CanPartyJoinBattle(item2, BattleSideEnum.Defender))
			{
				item2.MapEventSide = DefenderSide;
			}
			else if (CanPartyJoinBattle(item2, BattleSideEnum.Attacker))
			{
				item2.MapEventSide = AttackerSide;
			}
			else if (item2.MobileParty != null && !item2.MobileParty.IsGarrison && !item2.MobileParty.IsMilitia)
			{
				LeaveSettlementAction.ApplyForParty(item2.MobileParty);
			}
		}
	}

	private int GetBattleSizeValue()
	{
		if (IsSiegeAssault)
		{
			return 4;
		}
		int numberOfInvolvedMen = GetNumberOfInvolvedMen();
		if (numberOfInvolvedMen < 30)
		{
			return 0;
		}
		if (numberOfInvolvedMen < 80)
		{
			return 1;
		}
		if (numberOfInvolvedMen >= 120)
		{
			return 3;
		}
		return 2;
	}

	private static CampaignTime CalculateNextSimulationTime()
	{
		return CampaignTime.Now + CampaignTime.Minutes(30L);
	}

	internal void AddInvolvedPartyInternal(PartyBase involvedParty, BattleSideEnum side)
	{
		if (involvedParty.LeaderHero != null && involvedParty.LeaderHero.Clan == Clan.PlayerClan && involvedParty != PartyBase.MainParty && side == BattleSideEnum.Defender && AttackerSide.LeaderParty != null)
		{
			bool flag = false;
			foreach (PartyBase involvedParty2 in InvolvedParties)
			{
				if (involvedParty2 == PartyBase.MainParty)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Settlement settlement = Hero.MainHero.HomeSettlement;
				float num = Campaign.MapDiagonalSquared;
				foreach (Settlement item in Settlement.All)
				{
					if (item.IsVillage || item.IsFortification)
					{
						float num2 = item.Position2D.DistanceSquared(involvedParty.Position2D);
						if (num2 < num)
						{
							num = num2;
							settlement = item;
						}
					}
				}
				if (settlement != null)
				{
					TextObject textObject = GameTexts.FindText("str_party_attacked");
					textObject.SetTextVariable("CLAN_PARTY_NAME", involvedParty.Name);
					textObject.SetTextVariable("ENEMY_PARTY_NAME", AttackerSide.LeaderParty.Name);
					textObject.SetTextVariable("SETTLEMENT_NAME", settlement.Name);
					MBInformationManager.AddQuickInformation(textObject);
				}
			}
		}
		if (IsSiegeAssault && involvedParty.MobileParty != null && involvedParty.MobileParty.CurrentSettlement == null && side == BattleSideEnum.Defender)
		{
			_mapEventType = BattleTypes.SiegeOutside;
		}
		if (involvedParty.MobileParty != null && involvedParty.MobileParty.IsGarrison && side == BattleSideEnum.Attacker && IsSiegeOutside)
		{
			_mapEventType = BattleTypes.SallyOut;
			MapEventSettlement = involvedParty.MobileParty.CurrentSettlement;
		}
		involvedParty.ResetTempXp();
		if (involvedParty == MobileParty.MainParty.Party && !IsSiegeAssault && !IsRaid)
		{
			involvedParty.MobileParty.Ai.SetMoveModeHold();
		}
		if (involvedParty == PartyBase.MainParty)
		{
			involvedParty.MobileParty.Ai.ForceAiNoPathMode = false;
		}
		RecalculateRenownAndInfluenceValues(involvedParty);
		if (IsFieldBattle && involvedParty.IsMobile && involvedParty.MobileParty.BesiegedSettlement == null)
		{
			Vec2 vec = GetMapEventSide(side).LeaderParty.Position2D - Position;
			float a = vec.Normalize();
			if (involvedParty != GetMapEventSide(side).LeaderParty)
			{
				int num3 = GetMapEventSide(side).Parties.Count((MapEventParty p) => p.Party.IsMobile) - 1;
				involvedParty.MobileParty.EventPositionAdder = Position + vec * MathF.Max(a, 0.4f) + (num3 + 1) / 2 * ((num3 % 2 == 0) ? 1 : (-1)) * vec.RightVec() * 0.4f - (involvedParty.Position2D + involvedParty.MobileParty.ArmyPositionAdder);
			}
			else
			{
				involvedParty.MobileParty.EventPositionAdder = Position + vec * MathF.Max(a, 0.4f) - (involvedParty.Position2D + involvedParty.MobileParty.ArmyPositionAdder);
			}
		}
		involvedParty.SetVisualAsDirty();
		if (involvedParty.IsMobile && involvedParty.MobileParty.Army != null && involvedParty.MobileParty.Army.LeaderParty == involvedParty.MobileParty)
		{
			foreach (MobileParty attachedParty in involvedParty.MobileParty.Army.LeaderParty.AttachedParties)
			{
				attachedParty.Party.SetVisualAsDirty();
			}
		}
		if (HasWinner && involvedParty.MapEventSide.MissionSide != WinningSide && involvedParty.NumberOfHealthyMembers > 0)
		{
			BattleState = BattleState.None;
		}
		if (involvedParty.IsVisible)
		{
			IsVisible = true;
		}
		ResetUnsuitablePartiesThatWereTargetingThisMapEvent();
	}

	internal void PartyVisibilityChanged(PartyBase party, bool isPartyVisible)
	{
		if (isPartyVisible)
		{
			IsVisible = true;
			return;
		}
		bool isVisible = false;
		foreach (PartyBase involvedParty in InvolvedParties)
		{
			if (involvedParty != party && involvedParty.IsVisible)
			{
				isVisible = true;
				break;
			}
		}
		IsVisible = isVisible;
	}

	internal void RemoveInvolvedPartyInternal(PartyBase party)
	{
		party.SetVisualAsDirty();
		if (party.IsMobile && party.MobileParty.Army != null && party.MobileParty.Army.LeaderParty == party.MobileParty)
		{
			foreach (MobileParty attachedParty in party.MobileParty.Army.LeaderParty.AttachedParties)
			{
				attachedParty.Party.SetVisualAsDirty();
			}
		}
		if (IsFieldBattle && party.IsMobile)
		{
			party.MobileParty.EventPositionAdder = Vec2.Zero;
			MapEventSide[] sides = _sides;
			foreach (MapEventSide mapEventSide in sides)
			{
				MapEventParty[] array = mapEventSide.Parties.ToArray();
				Vec2 vec = mapEventSide.LeaderParty.Position2D - Position;
				float a = vec.Normalize();
				for (int j = 0; j < array.Length; j++)
				{
					PartyBase party2 = array[j].Party;
					if (party2.IsMobile && party2 != mapEventSide.LeaderParty)
					{
						party2.MobileParty.EventPositionAdder = Position + vec * MathF.Max(a, 0.4f) + (j + 1) / 2 * ((j % 2 == 0) ? 1 : (-1)) * vec.RightVec() * 0.4f - (party2.Position2D + party2.MobileParty.ArmyPositionAdder);
					}
				}
			}
		}
		if (IsSiegeOutside)
		{
			MapEventSide mapEventSide2 = ((MapEventSettlement != null) ? DefenderSide : AttackerSide);
			if (mapEventSide2.Parties.All((MapEventParty x) => x.Party.MobileParty == null || (MapEventSettlement != null && x.Party.MobileParty.CurrentSettlement == MapEventSettlement)) && MapEventSettlement != null)
			{
				_mapEventType = BattleTypes.Siege;
			}
		}
		if (party == PartyBase.MainParty && State == MapEventState.Wait)
		{
			AttackerSide.RemoveNearbyPartiesFromPlayerMapEvent();
			DefenderSide.RemoveNearbyPartiesFromPlayerMapEvent();
		}
		if (party.IsVisible)
		{
			PartyVisibilityChanged(party, isPartyVisible: false);
		}
		ResetUnsuitablePartiesThatWereTargetingThisMapEvent();
		if (party.IsMobile && !party.MobileParty.IsCurrentlyUsedByAQuest && party.SiegeEvent == null && (party.MobileParty.Army == null || party.MobileParty.Army.LeaderParty == party.MobileParty))
		{
			party.MobileParty.Ai.SetMoveModeHold();
		}
	}

	public int GetNumberOfInvolvedMen()
	{
		return DefenderSide.RecalculateMemberCountOfSide() + AttackerSide.RecalculateMemberCountOfSide();
	}

	public int GetNumberOfInvolvedMen(BattleSideEnum side)
	{
		return GetMapEventSide(side).RecalculateMemberCountOfSide();
	}

	private void LootDefeatedParties(out bool playerCaptured, LootCollector lootCollector)
	{
		GetMapEventSide(DefeatedSide).CollectAll(lootCollector, out playerCaptured);
	}

	internal void AddCasualtiesInBattle(TroopRoster troopRoster, LootCollector lootCollector)
	{
		lootCollector.CasualtiesInBattle.Add(troopRoster);
	}

	private int CalculatePlunderedGold()
	{
		float num = 0f;
		foreach (MapEventParty party2 in GetMapEventSide(DefeatedSide).Parties)
		{
			PartyBase party = party2.Party;
			if (party.LeaderHero != null)
			{
				int num2 = Campaign.Current.Models.BattleRewardModel.CalculateGoldLossAfterDefeat(party.LeaderHero);
				num += (float)num2;
				party2.GoldLost = num2;
			}
			else if (party.IsMobile && party.MobileParty.IsPartyTradeActive)
			{
				int num3 = (int)(party.MobileParty.IsBandit ? ((float)party.MobileParty.PartyTradeGold * 0.5f) : ((float)party.MobileParty.PartyTradeGold * 0.1f));
				num += (float)num3;
				party2.GoldLost = num3;
			}
		}
		return (int)num;
	}

	private void CalculateRenownShares(MapEventResultExplainer resultExplainers = null, bool forScoreboard = false)
	{
		if (BattleState == BattleState.AttackerVictory || BattleState == BattleState.DefenderVictory)
		{
			((BattleState == BattleState.AttackerVictory) ? AttackerSide : DefenderSide).DistributeRenownAndInfluence(resultExplainers, forScoreboard);
		}
	}

	private void CalculateLootShares(LootCollector lootCollector)
	{
		if (BattleState == BattleState.AttackerVictory || BattleState == BattleState.DefenderVictory)
		{
			((BattleState == BattleState.AttackerVictory) ? AttackerSide : DefenderSide).DistributeLootAmongWinners(lootCollector);
		}
	}

	private int GetSimulatedDamage(CharacterObject strikerTroop, CharacterObject strikedTroop, PartyBase strikerParty, PartyBase strikedParty, float strikerAdvantage)
	{
		return Campaign.Current.Models.CombatSimulationModel.SimulateHit(strikerTroop, strikedTroop, strikerParty, strikedParty, strikerAdvantage, this);
	}

	private void SimulateBattleForRound(BattleSideEnum side, float advantage)
	{
		bool flag = false;
		if (AttackerSide.NumRemainingSimulationTroops == 0 || DefenderSide.NumRemainingSimulationTroops == 0 || SimulateSingleHit((int)side, (int)(1 - side), advantage))
		{
			bool isRoundWinnerDetermined = false;
			BattleState calculateWinner = GetCalculateWinner(ref isRoundWinnerDetermined);
			if (calculateWinner != 0)
			{
				BattleState = calculateWinner;
			}
			else if (isRoundWinnerDetermined)
			{
				BattleObserver?.BattleResultsReady();
			}
		}
	}

	private bool SimulateSingleHit(int strikerSideIndex, int strikedSideIndex, float strikerAdvantage)
	{
		MapEventSide mapEventSide = _sides[strikerSideIndex];
		MapEventSide mapEventSide2 = _sides[strikedSideIndex];
		UniqueTroopDescriptor uniqueTroopDescriptor = mapEventSide.SelectRandomSimulationTroop();
		UniqueTroopDescriptor uniqueTroopDescriptor2 = mapEventSide2.SelectRandomSimulationTroop();
		CharacterObject allocatedTroop = mapEventSide.GetAllocatedTroop(uniqueTroopDescriptor);
		CharacterObject allocatedTroop2 = mapEventSide2.GetAllocatedTroop(uniqueTroopDescriptor2);
		PartyBase allocatedTroopParty = mapEventSide.GetAllocatedTroopParty(uniqueTroopDescriptor);
		PartyBase allocatedTroopParty2 = mapEventSide2.GetAllocatedTroopParty(uniqueTroopDescriptor2);
		int num = GetSimulatedDamage(allocatedTroop, allocatedTroop2, allocatedTroopParty, allocatedTroopParty2, strikerAdvantage);
		if (num > 0)
		{
			if (IsPlayerSimulation && allocatedTroopParty2 == PartyBase.MainParty)
			{
				float playerTroopsReceivedDamageMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
				num = MBRandom.RoundRandomized((float)num * playerTroopsReceivedDamageMultiplier);
			}
			DamageTypes damageType = ((MBRandom.RandomFloat < 0.3f) ? DamageTypes.Blunt : DamageTypes.Cut);
			bool flag = mapEventSide2.ApplySimulationDamageToSelectedTroop(num, damageType, allocatedTroopParty);
			mapEventSide.ApplySimulatedHitRewardToSelectedTroop(allocatedTroop, allocatedTroop2, num, flag);
			if (IsPlayerSimulation && allocatedTroopParty == PartyBase.MainParty && flag)
			{
				CampaignEventDispatcher.Instance.OnPlayerPartyKnockedOrKilledTroop(allocatedTroop2);
			}
			return flag;
		}
		return false;
	}

	private bool GetAttackersRunAwayChance()
	{
		if (_mapEventUpdateCount <= 1)
		{
			return false;
		}
		if (AttackerSide.LeaderParty.LeaderHero == null)
		{
			return false;
		}
		if (IsSallyOut)
		{
			return false;
		}
		float num = 0f;
		foreach (MapEventParty party in AttackerSide.Parties)
		{
			num += party.Party.TotalStrength;
		}
		float num2 = 0f;
		foreach (MapEventParty party2 in DefenderSide.Parties)
		{
			num2 += party2.Party.TotalStrength;
		}
		if (IsSiegeAssault)
		{
			num *= 2f / 3f;
		}
		if (num2 > num * 1.1f)
		{
			float randomFloat = MBRandom.RandomFloat;
			float num3 = ((_mapEventUpdateCount < 16) ? MathF.Sqrt((float)_mapEventUpdateCount / 16f) : 1f);
			return randomFloat * num3 > num / (num2 * 1.1f);
		}
		return false;
	}

	internal void Update()
	{
		if (_isFinishCalled)
		{
			return;
		}
		bool finish = false;
		if (_sides[0].LeaderParty == null || _sides[1].LeaderParty == null || !_sides[0].LeaderParty.MapFaction.IsAtWarWith(_sides[1].LeaderParty.MapFaction))
		{
			DiplomaticallyFinished = true;
		}
		if (!DiplomaticallyFinished)
		{
			Component?.Update(ref finish);
			if (((DefenderSide.TroopCount > 0 && AttackerSide.TroopCount > 0) || (!FirstUpdateIsDone && (DefenderSide.TroopCount > 0 || _mapEventType != BattleTypes.Raid))) && _nextSimulationTime.IsPast)
			{
				AttackersRanAway = _mapEventType != BattleTypes.Siege && _mapEventType != BattleTypes.SallyOut && _mapEventType != BattleTypes.SiegeOutside && _mapEventType != BattleTypes.Raid && GetAttackersRunAwayChance();
				_mapEventUpdateCount++;
				if (!AttackersRanAway)
				{
					SimulateBattleSessionForMapEvent();
					_nextSimulationTime = CalculateNextSimulationTime();
					FirstUpdateIsDone = true;
				}
				else
				{
					finish = true;
				}
			}
			if ((_mapEventType != BattleTypes.Raid || DefenderSide.Parties.Count > 1) && BattleState != 0)
			{
				finish = true;
			}
		}
		else
		{
			finish = true;
			foreach (PartyBase involvedParty in InvolvedParties)
			{
				if (involvedParty.IsMobile && involvedParty.MobileParty != MobileParty.MainParty && (involvedParty.MobileParty.Army == null || involvedParty.MobileParty.Army.LeaderParty == involvedParty.MobileParty))
				{
					involvedParty.MobileParty.Ai.RecalculateShortTermAi();
				}
			}
		}
		if (finish)
		{
			Component?.Finish();
			if (!IsPlayerMapEvent || PlayerEncounter.Current == null)
			{
				FinishBattle();
			}
		}
	}

	public void FinishBattleAndKeepSiegeEvent()
	{
		_keepSiegeEvent = true;
		FinishBattle();
	}

	private void CheckSiegeStageChange()
	{
		if (MapEventSettlement != null && IsSiegeAssault)
		{
			int num = AttackerSide.Parties.Sum((MapEventParty party) => party.Party.NumberOfHealthyMembers);
			int num2 = DefenderSide.Parties.Sum((MapEventParty party) => party.Party.NumberOfHealthyMembers);
			if (num == 0)
			{
			}
		}
	}

	public void SimulateBattleSetup(FlattenedTroopRoster[] priorTroops)
	{
		if (IsSiegeAssault)
		{
			CheckSiegeStageChange();
		}
		MapEventSide[] sides = _sides;
		foreach (MapEventSide mapEventSide in sides)
		{
			FlattenedTroopRoster flattenedTroopRoster = ((priorTroops != null) ? priorTroops[(int)mapEventSide.MissionSide] : null);
			mapEventSide.MakeReadyForSimulation(flattenedTroopRoster, flattenedTroopRoster?.Count() ?? (-1));
		}
		_battleState = BattleState.None;
	}

	public void SimulateBattleForRounds(int simulationRoundsDefender, int simulationRoundsAttacker)
	{
		bool isRoundWinnerDetermined = false;
		BattleState = GetCalculateWinner(ref isRoundWinnerDetermined);
		(float defenderAdvantage, float attackerAdvantage) battleAdvantage = Campaign.Current.Models.CombatSimulationModel.GetBattleAdvantage(DefenderSide.LeaderParty, AttackerSide.LeaderParty, _mapEventType, MapEventSettlement);
		float item = battleAdvantage.defenderAdvantage;
		float item2 = battleAdvantage.attackerAdvantage;
		int num = 0;
		while (0 < simulationRoundsAttacker + simulationRoundsDefender && BattleState == BattleState.None)
		{
			float num2 = (float)simulationRoundsAttacker / (float)(simulationRoundsAttacker + simulationRoundsDefender);
			if (MBRandom.RandomFloat < num2)
			{
				simulationRoundsAttacker--;
				SimulateBattleForRound(BattleSideEnum.Attacker, item2);
			}
			else
			{
				simulationRoundsDefender--;
				SimulateBattleForRound(BattleSideEnum.Defender, item);
			}
			num++;
		}
	}

	private void SimulateBattleSessionForMapEvent()
	{
		SimulateBattleSetup(null);
		var (simulationRoundsDefender, simulationRoundsAttacker) = Campaign.Current.Models.CombatSimulationModel.GetSimulationRoundsForBattle(this, DefenderSide.NumRemainingSimulationTroops, AttackerSide.NumRemainingSimulationTroops);
		SimulateBattleForRounds(simulationRoundsDefender, simulationRoundsAttacker);
		SimulateBattleEndSession();
	}

	internal void SimulatePlayerEncounterBattle()
	{
		var (simulationRoundsDefender, simulationRoundsAttacker) = Campaign.Current.Models.CombatSimulationModel.GetSimulationRoundsForBattle(this, DefenderSide.NumRemainingSimulationTroops, AttackerSide.NumRemainingSimulationTroops);
		SimulateBattleForRounds(simulationRoundsDefender, simulationRoundsAttacker);
	}

	private void SimulateBattleEndSession()
	{
		CommitXpGains();
		ApplyRenownAndInfluenceChanges();
		ApplyRewardsAndChanges();
		MapEventSide[] sides = _sides;
		for (int i = 0; i < sides.Length; i++)
		{
			sides[i].EndSimulation();
		}
	}

	private void OnBattleWon(BattleState winnerSide)
	{
		CalculateBattleResults(forScoreBoard: true);
		BattleObserver?.BattleResultsReady();
	}

	private BattleState GetCalculateWinner(ref bool isRoundWinnerDetermined)
	{
		BattleState result = BattleState.None;
		int num = AttackerSide.NumRemainingSimulationTroops;
		int num2 = DefenderSide.NumRemainingSimulationTroops;
		if (IsPlayerSimulation && !Hero.MainHero.IsWounded && InvolvedParties.Contains(PartyBase.MainParty))
		{
			if (PartyBase.MainParty.Side == BattleSideEnum.Attacker)
			{
				if (num == 0)
				{
					isRoundWinnerDetermined = true;
				}
				num++;
			}
			else if (PartyBase.MainParty.Side == BattleSideEnum.Defender)
			{
				if (num2 == 0)
				{
					isRoundWinnerDetermined = true;
				}
				num2++;
			}
		}
		if (num == 0)
		{
			result = BattleState.DefenderVictory;
		}
		else if (num2 == 0)
		{
			result = BattleState.AttackerVictory;
		}
		return result;
	}

	public void SetOverrideWinner(BattleSideEnum winner)
	{
		BattleState = winner switch
		{
			BattleSideEnum.Defender => BattleState.DefenderVictory, 
			BattleSideEnum.Attacker => BattleState.AttackerVictory, 
			_ => BattleState.None, 
		};
	}

	public void SetDefenderPulledBack()
	{
		BattleState = BattleState.DefenderPullBack;
	}

	public void ResetBattleState()
	{
		BattleState = BattleState.None;
	}

	internal bool CheckIfOneSideHasLost()
	{
		int num = DefenderSide.RecalculateMemberCountOfSide();
		int num2 = AttackerSide.RecalculateMemberCountOfSide();
		if (BattleState == BattleState.None && (num == 0 || num2 == 0))
		{
			BattleState = ((num2 <= 0) ? BattleState.DefenderVictory : BattleState.AttackerVictory);
		}
		if (BattleState != BattleState.AttackerVictory)
		{
			return BattleState == BattleState.DefenderVictory;
		}
		return true;
	}

	internal ItemRoster ItemRosterForPlayerLootShare(PartyBase party)
	{
		return GetMapEventSide(party.Side).ItemRosterForPlayerLootShare(party);
	}

	public bool IsPlayerSergeant()
	{
		if (IsPlayerMapEvent && GetLeaderParty(PlayerSide) != PartyBase.MainParty && MobileParty.MainParty.Army != null)
		{
			return MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty;
		}
		return false;
	}

	private void FinishBattle()
	{
		List<MobileParty> list = new List<MobileParty>();
		if (AttackersRanAway)
		{
			foreach (MapEventParty party in AttackerSide.Parties)
			{
				if (party.Party.IsMobile)
				{
					list.Add(party.Party.MobileParty);
				}
			}
		}
		_isFinishCalled = true;
		if (!_battleResultsCalculated)
		{
			CalculateBattleResults();
		}
		ApplyBattleResults();
		FinalizeEventAux();
		if (!AttackersRanAway)
		{
			return;
		}
		foreach (MobileParty item in list)
		{
			if (item.IsActive && item.AttachedTo == null)
			{
				if (item.BesiegerCamp != null)
				{
					item.BesiegerCamp = null;
				}
				item.TeleportPartyToSafePosition();
				item.Ai.SetMoveModeHold();
			}
		}
	}

	public override string ToString()
	{
		return string.Concat("Battle: ", AttackerSide.LeaderParty?.Name, " x ", DefenderSide.LeaderParty.Name);
	}

	internal void CalculateBattleResults(bool forScoreBoard = false)
	{
		if (_battleResultsCalculated)
		{
			return;
		}
		_battleResultsCalculated = !forScoreBoard;
		LootCollector lootCollector = new LootCollector();
		if (IsPlayerMapEvent)
		{
			_battleResultExplainers = new MapEventResultExplainer();
			if (PlayerEncounter.EncounteredPartySurrendered)
			{
				_sides[(int)DefeatedSide.GetOppositeSide()].ResetContributionToBattleToStrength();
			}
		}
		if (BattleState == BattleState.AttackerVictory || BattleState == BattleState.DefenderVictory)
		{
			int num = CalculatePlunderedGold();
			if (!forScoreBoard)
			{
				LootDefeatedParties(out PlayerCaptured, lootCollector);
				CalculatePlunderedGoldShares(num, _battleResultExplainers);
			}
			if (!forScoreBoard)
			{
				CalculateLootShares(lootCollector);
			}
			CalculateRenownShares(_battleResultExplainers, forScoreBoard);
		}
	}

	private void CalculatePlunderedGoldShares(float totalPlunderedGold, MapEventResultExplainer resultExplainers = null)
	{
		if (BattleState == BattleState.AttackerVictory || BattleState == BattleState.DefenderVictory)
		{
			((BattleState == BattleState.AttackerVictory) ? AttackerSide : DefenderSide).CalculatePlunderedGoldShare(totalPlunderedGold, resultExplainers);
		}
	}

	internal void ApplyBattleResults()
	{
		if (!_battleResultsCommitted)
		{
			CommitXpGains();
			ApplyRenownAndInfluenceChanges();
			ApplyRewardsAndChanges();
			_battleResultsCommitted = true;
		}
	}

	private void ApplyRewardsAndChanges()
	{
		MapEventSide[] sides = _sides;
		for (int i = 0; i < sides.Length; i++)
		{
			sides[i].ApplyFinalRewardsAndChanges();
		}
	}

	internal void ApplyRenownAndInfluenceChanges()
	{
		MapEventSide[] sides = _sides;
		for (int i = 0; i < sides.Length; i++)
		{
			sides[i].ApplyRenownAndInfluenceChanges();
		}
	}

	private void CommitXpGains()
	{
		MapEventSide[] sides = _sides;
		for (int i = 0; i < sides.Length; i++)
		{
			sides[i].CommitXpGains();
		}
	}

	internal void ResetBattleResults()
	{
		_battleResultsCommitted = false;
	}

	public void FinalizeEvent()
	{
		FinalizeEventAux();
	}

	private void FinalizeEventAux()
	{
		if (IsFinalized)
		{
			return;
		}
		State = MapEventState.WaitingRemoval;
		CampaignEventDispatcher.Instance.OnMapEventEnded(this);
		bool flag = false;
		if (MapEventSettlement != null)
		{
			if ((IsSiegeAssault || IsSiegeOutside || IsSallyOut) && MapEventSettlement.SiegeEvent != null)
			{
				MapEventSettlement.SiegeEvent.OnBeforeSiegeEventEnd(BattleState, _mapEventType);
			}
			if (!_keepSiegeEvent && (IsSiegeAssault || IsSiegeOutside))
			{
				switch (BattleState)
				{
				case BattleState.AttackerVictory:
					CampaignEventDispatcher.Instance.SiegeCompleted(MapEventSettlement, AttackerSide.LeaderParty.MobileParty, isWin: true, _mapEventType);
					flag = true;
					break;
				case BattleState.DefenderVictory:
					MapEventSettlement.SiegeEvent?.BesiegerCamp.RemoveAllSiegeParties();
					CampaignEventDispatcher.Instance.SiegeCompleted(MapEventSettlement, AttackerSide.LeaderParty.MobileParty, isWin: false, _mapEventType);
					break;
				}
			}
			else if (IsSallyOut && MapEventSettlement.Town != null && MapEventSettlement.Town.GarrisonParty != null && MapEventSettlement.Town.GarrisonParty.IsActive)
			{
				MapEventSettlement.Town.GarrisonParty.Ai.SetMoveModeHold();
			}
			Component?.FinalizeComponent();
		}
		MapEventSide[] sides = _sides;
		foreach (MapEventSide obj in sides)
		{
			obj.UpdatePartiesMoveState();
			obj.HandleMapEventEnd();
		}
		MapEventVisual?.OnMapEventEnd();
		foreach (PartyBase involvedParty in InvolvedParties)
		{
			if (involvedParty.IsMobile)
			{
				involvedParty.MobileParty.EventPositionAdder = Vec2.Zero;
			}
			involvedParty.SetVisualAsDirty();
			if (!involvedParty.IsMobile || involvedParty.MobileParty.Army == null || involvedParty.MobileParty.Army.LeaderParty != involvedParty.MobileParty)
			{
				continue;
			}
			foreach (MobileParty attachedParty in involvedParty.MobileParty.Army.LeaderParty.AttachedParties)
			{
				attachedParty.Party.SetVisualAsDirty();
			}
		}
		if (_mapEventType != BattleTypes.Siege && _mapEventType != BattleTypes.SiegeOutside && _mapEventType != BattleTypes.SallyOut)
		{
			foreach (PartyBase involvedParty2 in InvolvedParties)
			{
				if (involvedParty2.IsMobile && involvedParty2 != PartyBase.MainParty && involvedParty2.MobileParty.BesiegedSettlement != null && (involvedParty2.MobileParty.Army == null || involvedParty2.MobileParty.Army.LeaderParty == involvedParty2.MobileParty))
				{
					if (involvedParty2.IsActive)
					{
						EncounterManager.StartSettlementEncounter(involvedParty2.MobileParty, involvedParty2.MobileParty.BesiegedSettlement);
					}
					else
					{
						involvedParty2.MobileParty.BesiegerCamp = null;
					}
				}
			}
		}
		if (flag)
		{
			MapEventSettlement.Militia += Campaign.Current.Models.SettlementMilitiaModel.MilitiaToSpawnAfterSiege(MapEventSettlement.Town);
		}
		sides = _sides;
		for (int i = 0; i < sides.Length; i++)
		{
			sides[i].Clear();
		}
	}

	public bool HasTroopsOnBothSides()
	{
		bool num = PartiesOnSide(BattleSideEnum.Attacker).Any((MapEventParty party) => party.Party.NumberOfHealthyMembers > 0);
		bool flag = PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty party) => party.Party.NumberOfHealthyMembers > 0);
		return num && flag;
	}

	public PartyBase GetLeaderParty(BattleSideEnum side)
	{
		return _sides[(int)side].LeaderParty;
	}

	public float GetRenownValue(BattleSideEnum side)
	{
		return _sides[(int)side].RenownValue;
	}

	public float GetRenownValueAtMapEventEnd(BattleSideEnum side)
	{
		return _sides[(int)side].RenownAtMapEventEnd;
	}

	public void RecalculateRenownAndInfluenceValues(PartyBase party)
	{
		StrengthOfSide[(int)party.Side] += party.TotalStrength;
		MapEventSide[] sides = _sides;
		for (int i = 0; i < sides.Length; i++)
		{
			sides[i].CalculateRenownAndInfluenceValues(StrengthOfSide);
		}
	}

	public void RecalculateStrengthOfSides()
	{
		MapEventSide[] sides = _sides;
		foreach (MapEventSide mapEventSide in sides)
		{
			StrengthOfSide[(int)mapEventSide.MissionSide] = mapEventSide.RecalculateStrengthOfSide();
		}
	}

	public void DoSurrender(BattleSideEnum side)
	{
		GetMapEventSide(side).Surrender();
		BattleState = ((side != 0) ? BattleState.DefenderVictory : BattleState.AttackerVictory);
	}

	internal BattleSideEnum GetOtherSide(BattleSideEnum side)
	{
		if (side != BattleSideEnum.Attacker)
		{
			return BattleSideEnum.Attacker;
		}
		return BattleSideEnum.Defender;
	}

	private void ResetUnsuitablePartiesThatWereTargetingThisMapEvent()
	{
		LocatableSearchData<MobileParty> data = MobileParty.StartFindingLocatablesAroundPosition(Position, 15f);
		for (MobileParty mobileParty = MobileParty.FindNextLocatable(ref data); mobileParty != null; mobileParty = MobileParty.FindNextLocatable(ref data))
		{
			if (!mobileParty.IsMainParty && mobileParty.ShortTermBehavior == AiBehavior.EngageParty && (mobileParty.ShortTermTargetParty == GetLeaderParty(BattleSideEnum.Attacker).MobileParty || mobileParty.ShortTermTargetParty == GetLeaderParty(BattleSideEnum.Defender).MobileParty) && !CanPartyJoinBattle(mobileParty.Party, BattleSideEnum.Attacker) && !CanPartyJoinBattle(mobileParty.Party, BattleSideEnum.Defender))
			{
				mobileParty.Ai.SetMoveModeHold();
			}
		}
	}

	private void CacheSimulationData()
	{
		_sides[0].CacheLeaderSimulationModifier();
		_sides[1].CacheLeaderSimulationModifier();
		SimulationContext = DetermineContext();
	}

	private PowerCalculationContext DetermineContext()
	{
		PowerCalculationContext result = PowerCalculationContext.Default;
		MapWeatherModel.WeatherEvent weatherEventInPosition = Campaign.Current.Models.MapWeatherModel.GetWeatherEventInPosition(Position);
		if (weatherEventInPosition == MapWeatherModel.WeatherEvent.Snowy || weatherEventInPosition == MapWeatherModel.WeatherEvent.Blizzard)
		{
			result = PowerCalculationContext.SnowBattle;
		}
		switch (EventType)
		{
		case BattleTypes.FieldBattle:
		case BattleTypes.SallyOut:
		case BattleTypes.SiegeOutside:
			switch (EventTerrainType)
			{
			case TerrainType.Steppe:
				result = PowerCalculationContext.SteppeBattle;
				break;
			case TerrainType.Plain:
				result = PowerCalculationContext.PlainBattle;
				break;
			case TerrainType.Desert:
				result = PowerCalculationContext.DesertBattle;
				break;
			case TerrainType.Dune:
				result = PowerCalculationContext.DuneBattle;
				break;
			case TerrainType.Forest:
				result = PowerCalculationContext.ForestBattle;
				break;
			case TerrainType.Water:
			case TerrainType.Swamp:
			case TerrainType.Bridge:
			case TerrainType.River:
			case TerrainType.Fording:
			case TerrainType.Lake:
				result = PowerCalculationContext.RiverCrossingBattle;
				break;
			}
			break;
		case BattleTypes.Raid:
		case BattleTypes.IsForcingVolunteers:
		case BattleTypes.IsForcingSupplies:
			result = PowerCalculationContext.Village;
			break;
		case BattleTypes.Siege:
			result = PowerCalculationContext.Siege;
			break;
		}
		return result;
	}

	bool IMapEntity.OnMapClick(bool followModifierUsed)
	{
		return false;
	}

	void IMapEntity.OnOpenEncyclopedia()
	{
	}

	void IMapEntity.OnHover()
	{
		InformationManager.ShowTooltip(typeof(MapEvent), this);
	}

	bool IMapEntity.IsEnemyOf(IFaction faction)
	{
		return false;
	}

	bool IMapEntity.IsAllyOf(IFaction faction)
	{
		return false;
	}

	public void GetMountAndHarnessVisualIdsForPartyIcon(out string mountStringId, out string harnessStringId)
	{
		mountStringId = "";
		harnessStringId = "";
	}

	void IMapEntity.OnPartyInteraction(MobileParty mobileParty)
	{
	}

	public bool CanPartyJoinBattle(PartyBase party, BattleSideEnum side)
	{
		if (GetMapEventSide(side).Parties.All((MapEventParty x) => !x.Party.MapFaction.IsAtWarWith(party.MapFaction)))
		{
			return GetMapEventSide(GetOtherSide(side)).Parties.All((MapEventParty x) => x.Party.MapFaction.IsAtWarWith(party.MapFaction));
		}
		return false;
	}

	public void GetStrengthsRelativeToParty(BattleSideEnum partySide, out float partySideStrength, out float opposingSideStrength)
	{
		partySideStrength = 0.1f;
		opposingSideStrength = 0.1f;
		if (this != null)
		{
			foreach (PartyBase involvedParty in InvolvedParties)
			{
				if (involvedParty.Side == partySide)
				{
					partySideStrength += involvedParty.TotalStrength;
				}
				else
				{
					opposingSideStrength += involvedParty.TotalStrength;
				}
			}
			return;
		}
		Debug.FailedAssert("Cannot retrieve party strengths. MapEvent parameter is null.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\MapEvents\\MapEvent.cs", "GetStrengthsRelativeToParty", 1940);
	}

	public bool CheckIfBattleShouldContinueAfterBattleMission(CampaignBattleResult campaignBattleResult)
	{
		if (PlayerEncounter.PlayerSurrender || campaignBattleResult == null || campaignBattleResult.EnemyRetreated)
		{
			return false;
		}
		bool flag = IsSiegeAssault && BattleState == BattleState.AttackerVictory;
		MapEventSide mapEventSide = GetMapEventSide(PlayerSide);
		bool flag2 = (campaignBattleResult.PlayerDefeat && mapEventSide.GetTotalHealthyTroopCountOfSide() >= 1) || ((campaignBattleResult.PlayerVictory || campaignBattleResult.EnemyPulledBack) && DefeatedSide != BattleSideEnum.None && GetMapEventSide(DefeatedSide).GetTotalHealthyTroopCountOfSide() >= 1);
		if (!IsHideoutBattle && !flag && flag2)
		{
			return !mapEventSide.IsSurrendered;
		}
		return false;
	}
}
