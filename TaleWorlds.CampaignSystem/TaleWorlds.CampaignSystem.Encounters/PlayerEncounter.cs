using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.Encounters;

public class PlayerEncounter
{
	[SaveableField(0)]
	public static bool EncounteredPartySurrendered;

	[SaveableField(1)]
	public bool FirstInit = true;

	public const float JoiningRadius = 3f;

	[SaveableField(5)]
	public bool IsEnemy;

	[SaveableField(7)]
	public float PlayerPartyInitialStrength;

	[SaveableField(8)]
	private CampaignBattleResult _campaignBattleResult;

	[SaveableField(9)]
	public float PartiesStrengthRatioBeforePlayerJoin;

	[SaveableField(10)]
	public bool ForceRaid;

	[SaveableField(11)]
	public bool ForceSallyOut;

	[SaveableField(32)]
	public bool ForceVolunteers;

	[SaveableField(33)]
	public bool ForceSupplies;

	[SaveableField(34)]
	private bool _isSiegeInterruptedByEnemyDefection;

	public BattleSimulation BattleSimulation;

	[SaveableField(13)]
	private MapEvent _mapEvent;

	[SaveableField(14)]
	private PlayerEncounterState _mapEventState;

	[SaveableField(15)]
	private PartyBase _encounteredParty;

	[SaveableField(16)]
	private PartyBase _attackerParty;

	[SaveableField(17)]
	private PartyBase _defenderParty;

	[SaveableField(18)]
	private List<Hero> _helpedHeroes;

	[SaveableField(19)]
	private List<TroopRosterElement> _capturedHeroes;

	[SaveableField(20)]
	private List<TroopRosterElement> _freedHeroes;

	[SaveableField(22)]
	private bool _leaveEncounter;

	[SaveableField(23)]
	private bool _playerSurrender;

	[SaveableField(24)]
	private bool _enemySurrender;

	[SaveableField(25)]
	private bool _battleChallenge;

	[SaveableField(26)]
	private bool _meetingDone;

	[SaveableField(27)]
	private bool _stateHandled;

	[SaveableField(36)]
	private ItemRoster _alternativeRosterToReceiveLootItems;

	[SaveableField(37)]
	private TroopRoster _alternativeRosterToReceiveLootPrisoners;

	[SaveableField(38)]
	private TroopRoster _alternativeRosterToReceiveLootMembers;

	[SaveableField(51)]
	private bool _doesBattleContinue;

	[SaveableField(52)]
	private bool _isSallyOutAmbush;

	public static PlayerEncounter Current => Campaign.Current.PlayerEncounter;

	public static LocationEncounter LocationEncounter
	{
		get
		{
			return Campaign.Current.LocationEncounter;
		}
		set
		{
			Campaign.Current.LocationEncounter = value;
		}
	}

	public static MapEvent Battle
	{
		get
		{
			if (Current == null)
			{
				return null;
			}
			return Current._mapEvent;
		}
	}

	public static PartyBase EncounteredParty
	{
		get
		{
			if (Current != null)
			{
				return Current._encounteredParty;
			}
			return null;
		}
	}

	public static MobileParty EncounteredMobileParty => EncounteredParty?.MobileParty;

	public static MapEvent EncounteredBattle
	{
		get
		{
			if (Current._encounteredParty.MapEvent != null)
			{
				return Current._encounteredParty.MapEvent;
			}
			if (Current._encounteredParty.IsSettlement && Current._encounteredParty.SiegeEvent?.BesiegerCamp.LeaderParty.MapEvent != null)
			{
				return Current._encounteredParty.SiegeEvent.BesiegerCamp.LeaderParty.MapEvent;
			}
			return null;
		}
	}

	public static BattleState BattleState => Current._mapEvent.BattleState;

	public static BattleSideEnum WinningSide => Current._mapEvent.WinningSide;

	public static bool BattleChallenge
	{
		get
		{
			return Current._battleChallenge;
		}
		set
		{
			Current._battleChallenge = value;
		}
	}

	public static bool PlayerIsDefender => Current.PlayerSide == BattleSideEnum.Defender;

	public static bool PlayerIsAttacker => Current.PlayerSide == BattleSideEnum.Attacker;

	public static bool LeaveEncounter
	{
		get
		{
			return Current._leaveEncounter;
		}
		set
		{
			Current._leaveEncounter = value;
		}
	}

	public static bool MeetingDone => Current._meetingDone;

	public static bool PlayerSurrender
	{
		get
		{
			return Current._playerSurrender;
		}
		set
		{
			if (value)
			{
				Current.PlayerSurrenderInternal();
			}
		}
	}

	public static bool EnemySurrender
	{
		get
		{
			return Current._enemySurrender;
		}
		set
		{
			if (value)
			{
				Current.EnemySurrenderInternal();
			}
		}
	}

	public static bool IsActive => Current != null;

	[SaveableProperty(2)]
	public BattleSideEnum OpponentSide { get; private set; }

	[SaveableProperty(3)]
	public BattleSideEnum PlayerSide { get; private set; }

	[SaveableProperty(6)]
	public bool IsJoinedBattle { get; private set; }

	public static bool InsideSettlement
	{
		get
		{
			if (MobileParty.MainParty.IsActive)
			{
				return MobileParty.MainParty.CurrentSettlement != null;
			}
			return false;
		}
	}

	public static CampaignBattleResult CampaignBattleResult
	{
		get
		{
			return Current._campaignBattleResult;
		}
		set
		{
			Current._campaignBattleResult = value;
		}
	}

	public static BattleSimulation CurrentBattleSimulation
	{
		get
		{
			if (Current == null)
			{
				return null;
			}
			return Current.BattleSimulation;
		}
	}

	public PlayerEncounterState EncounterState
	{
		get
		{
			return _mapEventState;
		}
		private set
		{
			_mapEventState = value;
		}
	}

	public ItemRoster RosterToReceiveLootItems
	{
		get
		{
			if (_alternativeRosterToReceiveLootItems == null)
			{
				_alternativeRosterToReceiveLootItems = new ItemRoster();
			}
			return _alternativeRosterToReceiveLootItems;
		}
	}

	public TroopRoster RosterToReceiveLootPrisoners
	{
		get
		{
			if (_alternativeRosterToReceiveLootPrisoners == null)
			{
				_alternativeRosterToReceiveLootPrisoners = TroopRoster.CreateDummyTroopRoster();
			}
			return _alternativeRosterToReceiveLootPrisoners;
		}
	}

	public TroopRoster RosterToReceiveLootMembers
	{
		get
		{
			if (_alternativeRosterToReceiveLootMembers == null)
			{
				_alternativeRosterToReceiveLootMembers = TroopRoster.CreateDummyTroopRoster();
			}
			return _alternativeRosterToReceiveLootMembers;
		}
	}

	public static Settlement EncounterSettlement => Current?.EncounterSettlementAux;

	[SaveableProperty(28)]
	public Settlement EncounterSettlementAux { get; private set; }

	[SaveableProperty(50)]
	public bool IsPlayerWaiting { get; set; }

	internal static void AutoGeneratedStaticCollectObjectsPlayerEncounter(object o, List<object> collectedObjects)
	{
		((PlayerEncounter)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected virtual void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		collectedObjects.Add(_campaignBattleResult);
		collectedObjects.Add(_mapEvent);
		collectedObjects.Add(_encounteredParty);
		collectedObjects.Add(_attackerParty);
		collectedObjects.Add(_defenderParty);
		collectedObjects.Add(_helpedHeroes);
		collectedObjects.Add(_capturedHeroes);
		collectedObjects.Add(_freedHeroes);
		collectedObjects.Add(_alternativeRosterToReceiveLootItems);
		collectedObjects.Add(_alternativeRosterToReceiveLootPrisoners);
		collectedObjects.Add(_alternativeRosterToReceiveLootMembers);
		collectedObjects.Add(EncounterSettlementAux);
	}

	internal static object AutoGeneratedGetMemberValueOpponentSide(object o)
	{
		return ((PlayerEncounter)o).OpponentSide;
	}

	internal static object AutoGeneratedGetMemberValuePlayerSide(object o)
	{
		return ((PlayerEncounter)o).PlayerSide;
	}

	internal static object AutoGeneratedGetMemberValueIsJoinedBattle(object o)
	{
		return ((PlayerEncounter)o).IsJoinedBattle;
	}

	internal static object AutoGeneratedGetMemberValueEncounterSettlementAux(object o)
	{
		return ((PlayerEncounter)o).EncounterSettlementAux;
	}

	internal static object AutoGeneratedGetMemberValueIsPlayerWaiting(object o)
	{
		return ((PlayerEncounter)o).IsPlayerWaiting;
	}

	internal static object AutoGeneratedGetMemberValueFirstInit(object o)
	{
		return ((PlayerEncounter)o).FirstInit;
	}

	internal static object AutoGeneratedGetMemberValueIsEnemy(object o)
	{
		return ((PlayerEncounter)o).IsEnemy;
	}

	internal static object AutoGeneratedGetMemberValuePlayerPartyInitialStrength(object o)
	{
		return ((PlayerEncounter)o).PlayerPartyInitialStrength;
	}

	internal static object AutoGeneratedGetMemberValuePartiesStrengthRatioBeforePlayerJoin(object o)
	{
		return ((PlayerEncounter)o).PartiesStrengthRatioBeforePlayerJoin;
	}

	internal static object AutoGeneratedGetMemberValueForceRaid(object o)
	{
		return ((PlayerEncounter)o).ForceRaid;
	}

	internal static object AutoGeneratedGetMemberValueForceSallyOut(object o)
	{
		return ((PlayerEncounter)o).ForceSallyOut;
	}

	internal static object AutoGeneratedGetMemberValueForceVolunteers(object o)
	{
		return ((PlayerEncounter)o).ForceVolunteers;
	}

	internal static object AutoGeneratedGetMemberValueForceSupplies(object o)
	{
		return ((PlayerEncounter)o).ForceSupplies;
	}

	internal static object AutoGeneratedGetMemberValue_campaignBattleResult(object o)
	{
		return ((PlayerEncounter)o)._campaignBattleResult;
	}

	internal static object AutoGeneratedGetMemberValue_isSiegeInterruptedByEnemyDefection(object o)
	{
		return ((PlayerEncounter)o)._isSiegeInterruptedByEnemyDefection;
	}

	internal static object AutoGeneratedGetMemberValue_mapEvent(object o)
	{
		return ((PlayerEncounter)o)._mapEvent;
	}

	internal static object AutoGeneratedGetMemberValue_mapEventState(object o)
	{
		return ((PlayerEncounter)o)._mapEventState;
	}

	internal static object AutoGeneratedGetMemberValue_encounteredParty(object o)
	{
		return ((PlayerEncounter)o)._encounteredParty;
	}

	internal static object AutoGeneratedGetMemberValue_attackerParty(object o)
	{
		return ((PlayerEncounter)o)._attackerParty;
	}

	internal static object AutoGeneratedGetMemberValue_defenderParty(object o)
	{
		return ((PlayerEncounter)o)._defenderParty;
	}

	internal static object AutoGeneratedGetMemberValue_helpedHeroes(object o)
	{
		return ((PlayerEncounter)o)._helpedHeroes;
	}

	internal static object AutoGeneratedGetMemberValue_capturedHeroes(object o)
	{
		return ((PlayerEncounter)o)._capturedHeroes;
	}

	internal static object AutoGeneratedGetMemberValue_freedHeroes(object o)
	{
		return ((PlayerEncounter)o)._freedHeroes;
	}

	internal static object AutoGeneratedGetMemberValue_leaveEncounter(object o)
	{
		return ((PlayerEncounter)o)._leaveEncounter;
	}

	internal static object AutoGeneratedGetMemberValue_playerSurrender(object o)
	{
		return ((PlayerEncounter)o)._playerSurrender;
	}

	internal static object AutoGeneratedGetMemberValue_enemySurrender(object o)
	{
		return ((PlayerEncounter)o)._enemySurrender;
	}

	internal static object AutoGeneratedGetMemberValue_battleChallenge(object o)
	{
		return ((PlayerEncounter)o)._battleChallenge;
	}

	internal static object AutoGeneratedGetMemberValue_meetingDone(object o)
	{
		return ((PlayerEncounter)o)._meetingDone;
	}

	internal static object AutoGeneratedGetMemberValue_stateHandled(object o)
	{
		return ((PlayerEncounter)o)._stateHandled;
	}

	internal static object AutoGeneratedGetMemberValue_alternativeRosterToReceiveLootItems(object o)
	{
		return ((PlayerEncounter)o)._alternativeRosterToReceiveLootItems;
	}

	internal static object AutoGeneratedGetMemberValue_alternativeRosterToReceiveLootPrisoners(object o)
	{
		return ((PlayerEncounter)o)._alternativeRosterToReceiveLootPrisoners;
	}

	internal static object AutoGeneratedGetMemberValue_alternativeRosterToReceiveLootMembers(object o)
	{
		return ((PlayerEncounter)o)._alternativeRosterToReceiveLootMembers;
	}

	internal static object AutoGeneratedGetMemberValue_doesBattleContinue(object o)
	{
		return ((PlayerEncounter)o)._doesBattleContinue;
	}

	internal static object AutoGeneratedGetMemberValue_isSallyOutAmbush(object o)
	{
		return ((PlayerEncounter)o)._isSallyOutAmbush;
	}

	private PlayerEncounter()
	{
	}

	public void OnLoad()
	{
		if (InsideSettlement && Battle == null)
		{
			CreateLocationEncounter(MobileParty.MainParty.CurrentSettlement);
		}
		else if (Current != null && EncounterSettlement != null && EncounterSettlement.IsVillage && Current.IsPlayerWaiting)
		{
			CreateLocationEncounter(EncounterSettlementAux);
		}
	}

	public static void RestartPlayerEncounter(PartyBase defenderParty, PartyBase attackerParty, bool forcePlayerOutFromSettlement = true)
	{
		if (Current != null)
		{
			Finish(forcePlayerOutFromSettlement);
		}
		Start();
		Current.SetupFields(attackerParty, defenderParty);
	}

	internal static void SimulateBattle()
	{
		Battle.SimulatePlayerEncounterBattle();
	}

	internal void Init(PartyBase attackerParty, PartyBase defenderParty, Settlement settlement = null)
	{
		EncounterSettlementAux = ((settlement != null) ? settlement : (defenderParty.IsSettlement ? defenderParty.Settlement : attackerParty.Settlement));
		EncounteredPartySurrendered = false;
		PlayerPartyInitialStrength = PartyBase.MainParty.TotalStrength;
		SetupFields(attackerParty, defenderParty);
		if (defenderParty.MapEvent != null && attackerParty != MobileParty.MainParty.Party && defenderParty != MobileParty.MainParty.Party)
		{
			_mapEvent = defenderParty.MapEvent;
			if (_mapEvent.CanPartyJoinBattle(PartyBase.MainParty, BattleSideEnum.Defender))
			{
				MobileParty.MainParty.Party.MapEventSide = _mapEvent.DefenderSide;
			}
			else if (_mapEvent.CanPartyJoinBattle(PartyBase.MainParty, BattleSideEnum.Attacker))
			{
				MobileParty.MainParty.Party.MapEventSide = _mapEvent.AttackerSide;
			}
		}
		bool joinBattle = false;
		bool startBattle = false;
		string encounterMenu = Campaign.Current.Models.EncounterGameMenuModel.GetEncounterMenu(attackerParty, defenderParty, out startBattle, out joinBattle);
		if (!string.IsNullOrEmpty(encounterMenu))
		{
			if (startBattle)
			{
				StartBattle();
			}
			if (joinBattle)
			{
				if (MobileParty.MainParty.MapEvent == null)
				{
					if (defenderParty.MapEvent != null)
					{
						if (defenderParty.MapEvent.CanPartyJoinBattle(PartyBase.MainParty, BattleSideEnum.Attacker))
						{
							JoinBattle(BattleSideEnum.Attacker);
						}
						else if (defenderParty.MapEvent.CanPartyJoinBattle(PartyBase.MainParty, BattleSideEnum.Defender))
						{
							JoinBattle(BattleSideEnum.Defender);
						}
						else
						{
							Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Encounters\\PlayerEncounter.cs", "Init", 461);
						}
					}
					else
					{
						Debug.FailedAssert("If there is no map event we should create one in order to join battle", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Encounters\\PlayerEncounter.cs", "Init", 466);
					}
				}
				CheckNearbyPartiesToJoinPlayerMapEvent();
			}
			if (attackerParty == PartyBase.MainParty && defenderParty.IsSettlement && !defenderParty.Settlement.IsUnderRaid && !defenderParty.Settlement.IsUnderSiege)
			{
				EnterSettlement();
			}
			GameMenu.ActivateGameMenu(encounterMenu);
		}
		else if (attackerParty == PartyBase.MainParty && defenderParty.IsSettlement && !defenderParty.Settlement.IsUnderRaid && !defenderParty.Settlement.IsUnderSiege)
		{
			EnterSettlement();
		}
	}

	public static void Init()
	{
		if (Current == null)
		{
			Start();
		}
		Current.InitAux();
	}

	private void InitAux()
	{
		if (MobileParty.MainParty.MapEvent != null)
		{
			_mapEvent = MobileParty.MainParty.MapEvent;
			SetupFields(_mapEvent.AttackerSide.LeaderParty, _mapEvent.DefenderSide.LeaderParty);
			CheckNearbyPartiesToJoinPlayerMapEvent();
		}
	}

	public void SetupFields(PartyBase attackerParty, PartyBase defenderParty)
	{
		_attackerParty = attackerParty;
		_defenderParty = defenderParty;
		MobileParty mobileParty = ((defenderParty.IsMobile && defenderParty != PartyBase.MainParty && defenderParty.MobileParty != MobileParty.MainParty.AttachedTo) ? defenderParty.MobileParty : ((attackerParty.IsMobile && attackerParty != PartyBase.MainParty && attackerParty.MobileParty != MobileParty.MainParty.AttachedTo) ? attackerParty.MobileParty : null));
		if (_defenderParty.IsSettlement)
		{
			EncounterSettlementAux = defenderParty.Settlement;
		}
		else if (_attackerParty.IsSettlement)
		{
			EncounterSettlementAux = _attackerParty.Settlement;
		}
		else if (mobileParty.BesiegerCamp != null)
		{
			EncounterSettlementAux = mobileParty.BesiegerCamp.SiegeEvent.BesiegedSettlement;
		}
		_encounteredParty = ((mobileParty != null) ? mobileParty.Party : EncounterSettlementAux?.Party);
		if (MapEvent.PlayerMapEvent != null)
		{
			PlayerSide = MapEvent.PlayerMapEvent.PlayerSide;
		}
		else if (defenderParty == PartyBase.MainParty || (defenderParty.MobileParty != null && defenderParty.MobileParty == MobileParty.MainParty.AttachedTo) || (defenderParty.IsSettlement && (defenderParty.Settlement.MapFaction == MobileParty.MainParty.MapFaction || MobileParty.MainParty.CurrentSettlement == defenderParty.Settlement)))
		{
			PlayerSide = BattleSideEnum.Defender;
		}
		else
		{
			PlayerSide = BattleSideEnum.Attacker;
		}
		OpponentSide = PlayerSide.GetOppositeSide();
		if (!_encounteredParty.IsSettlement)
		{
			MobileParty.MainParty.Ai.SetMoveModeHold();
		}
	}

	internal void OnPartyJoinEncounter(MobileParty newParty)
	{
		if (Battle == null)
		{
			return;
		}
		if (Battle.CanPartyJoinBattle(PartyBase.MainParty, PartyBase.MainParty.Side))
		{
			newParty.Party.MapEventSide = PartyBase.MainParty.MapEventSide;
		}
		else if (newParty != MobileParty.MainParty || !Battle.IsRaid || Battle.AttackerSide.LeaderParty == MobileParty.MainParty.Party || Battle.DefenderSide.TroopCount != 0)
		{
			MobileParty.MainParty.Ai.SetMoveModeHold();
			string newPartyJoinMenu = Campaign.Current.Models.EncounterGameMenuModel.GetNewPartyJoinMenu(newParty);
			if (Battle.CanPartyJoinBattle(PartyBase.MainParty, PartyBase.MainParty.OpponentSide))
			{
				newParty.Party.MapEventSide = PartyBase.MainParty.MapEventSide.OtherSide;
			}
			if (!string.IsNullOrEmpty(newPartyJoinMenu))
			{
				GameMenu.SwitchToMenu(newPartyJoinMenu);
			}
		}
	}

	private void CheckNearbyPartiesToJoinPlayerMapEvent()
	{
		if (_mapEvent == null || _mapEvent.IsRaid || _mapEvent.IsSiegeAssault || _mapEvent.IsForcingSupplies || _mapEvent.IsForcingVolunteers || (_mapEvent.MapEventSettlement != null && _mapEvent.MapEventSettlement.IsHideout))
		{
			return;
		}
		List<MobileParty> partiesToJoinPlayerSide = new List<MobileParty>();
		List<MobileParty> partiesToJoinEnemySide = new List<MobileParty>();
		foreach (MapEventParty item in _mapEvent.PartiesOnSide(PlayerSide))
		{
			if (item.Party.IsMobile)
			{
				partiesToJoinPlayerSide.Add(item.Party.MobileParty);
			}
		}
		foreach (MapEventParty item2 in _mapEvent.PartiesOnSide(PlayerSide.GetOppositeSide()))
		{
			if (item2.Party.IsMobile)
			{
				partiesToJoinEnemySide.Add(item2.Party.MobileParty);
			}
		}
		Current.FindNonAttachedNpcPartiesWhoWillJoinEvent(ref partiesToJoinPlayerSide, ref partiesToJoinEnemySide);
		foreach (MobileParty item3 in partiesToJoinPlayerSide)
		{
			_mapEvent.GetMapEventSide(PlayerSide).AddNearbyPartyToPlayerMapEvent(item3);
		}
		foreach (MobileParty item4 in partiesToJoinEnemySide)
		{
			_mapEvent.GetMapEventSide(PlayerSide.GetOppositeSide()).AddNearbyPartyToPlayerMapEvent(item4);
		}
	}

	private static TerrainType GetTerrainByCount(List<TerrainType> terrainTypeSamples, TerrainType currentPositionTerrainType)
	{
		for (int i = 0; i < terrainTypeSamples.Count; i++)
		{
			if (terrainTypeSamples[i] == TerrainType.Snow)
			{
				terrainTypeSamples[i] = TerrainType.Plain;
			}
		}
		if (currentPositionTerrainType == TerrainType.Plain || currentPositionTerrainType == TerrainType.Desert || currentPositionTerrainType == TerrainType.Swamp || currentPositionTerrainType == TerrainType.Steppe)
		{
			int num = (int)((float)terrainTypeSamples.Count * 0.33f);
			for (int j = 0; j < num; j++)
			{
				terrainTypeSamples.Add(currentPositionTerrainType);
			}
		}
		Dictionary<TerrainType, int> dictionary = new Dictionary<TerrainType, int>();
		foreach (TerrainType terrainTypeSample in terrainTypeSamples)
		{
			if (!dictionary.ContainsKey(terrainTypeSample))
			{
				dictionary.Add(terrainTypeSample, 1);
			}
			else
			{
				dictionary[terrainTypeSample]++;
			}
		}
		KeyValuePair<TerrainType, int> keyValuePair = new KeyValuePair<TerrainType, int>(TerrainType.Plain, 0);
		foreach (KeyValuePair<TerrainType, int> item in dictionary)
		{
			if ((item.Key == TerrainType.Plain || item.Key == TerrainType.Desert || item.Key == TerrainType.Swamp || item.Key == TerrainType.Steppe) && item.Value > keyValuePair.Value)
			{
				keyValuePair = item;
			}
		}
		return keyValuePair.Key;
	}

	private static List<TerrainType> GetSceneProperties(List<TerrainType> terrainTypeSamples, int forestCount, out ForestDensity forestDensity)
	{
		forestDensity = ForestDensity.None;
		float num = (float)forestCount / (float)terrainTypeSamples.Count;
		if (num > 0.1f && num < 0.5f)
		{
			forestDensity = ForestDensity.Low;
		}
		else if (num >= 0.5f)
		{
			forestDensity = ForestDensity.High;
		}
		List<TerrainType> list = new List<TerrainType>();
		foreach (TerrainType terrainTypeSample in terrainTypeSamples)
		{
			if (!list.Contains(terrainTypeSample))
			{
				list.Add(terrainTypeSample);
			}
		}
		return list;
	}

	public static string GetBattleSceneForMapPatch(MapPatchData mapPatch)
	{
		string text = "";
		MBList<SingleplayerBattleSceneData> mBList = GameSceneDataManager.Instance.SingleplayerBattleScenes.Where((SingleplayerBattleSceneData scene) => scene.MapIndices.Contains(mapPatch.sceneIndex)).ToMBList();
		if (mBList.IsEmpty())
		{
			Debug.FailedAssert("Battle scene for map patch with scene index " + mapPatch.sceneIndex + " does not exist. Picking a random scene", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Encounters\\PlayerEncounter.cs", "GetBattleSceneForMapPatch", 735);
			return GameSceneDataManager.Instance.SingleplayerBattleScenes.GetRandomElement().SceneID;
		}
		if (mBList.Count > 1)
		{
			Debug.FailedAssert("Multiple battle scenes for map patch with scene index " + mapPatch.sceneIndex + " are defined. Picking a matching scene randomly", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Encounters\\PlayerEncounter.cs", "GetBattleSceneForMapPatch", 740);
			return mBList.GetRandomElement().SceneID;
		}
		return mBList[0].SceneID;
	}

	public static string GetConversationSceneForMapPosition(Vec2 position2D)
	{
		TerrainType currentPositionTerrainType;
		List<TerrainType> environmentTerrainTypesCount = Campaign.Current.MapSceneWrapper.GetEnvironmentTerrainTypesCount(position2D, out currentPositionTerrainType);
		int num = 0;
		foreach (TerrainType item in environmentTerrainTypesCount)
		{
			num += ((item == TerrainType.Forest) ? 1 : 0);
		}
		TerrainType terrainByCount = GetTerrainByCount(environmentTerrainTypesCount, currentPositionTerrainType);
		ForestDensity forestDensity;
		List<TerrainType> sceneProperties = GetSceneProperties(environmentTerrainTypesCount, num, out forestDensity);
		int num2 = 0;
		Dictionary<ConversationSceneData, int> dictionary = new Dictionary<ConversationSceneData, int>();
		foreach (ConversationSceneData conversationScene in GameSceneDataManager.Instance.ConversationScenes)
		{
			if (conversationScene.Terrain != terrainByCount)
			{
				continue;
			}
			int num3 = 0;
			if ((forestDensity == ForestDensity.None && conversationScene.ForestDensity == ForestDensity.None) || (forestDensity != 0 && conversationScene.ForestDensity != 0))
			{
				num3++;
			}
			int num4 = 2 - MathF.Abs(forestDensity - conversationScene.ForestDensity);
			num3 += num4;
			foreach (TerrainType item2 in sceneProperties)
			{
				if (conversationScene.TerrainTypes.Contains(item2))
				{
					num3 += 3;
				}
			}
			dictionary.Add(conversationScene, num3);
			if (num3 > num2)
			{
				num2 = num3;
			}
		}
		MBList<ConversationSceneData> mBList = new MBList<ConversationSceneData>();
		foreach (KeyValuePair<ConversationSceneData, int> item3 in dictionary)
		{
			if (item3.Value == num2)
			{
				mBList.Add(item3.Key);
			}
		}
		return ((mBList.Count > 0) ? mBList.GetRandomElement() : GameSceneDataManager.Instance.ConversationScenes.GetRandomElement()).SceneID;
	}

	private MapEvent StartBattleInternal()
	{
		if (_mapEvent == null)
		{
			if (ForceRaid)
			{
				_mapEvent = RaidEventComponent.CreateRaidEvent(_attackerParty, _defenderParty).MapEvent;
			}
			else if (ForceSallyOut)
			{
				_mapEvent = Campaign.Current.MapEventManager.StartSallyOutMapEvent(_attackerParty, _defenderParty);
			}
			else if (ForceVolunteers)
			{
				_mapEvent = ForceVolunteersEventComponent.CreateForceSuppliesEvent(_attackerParty, _defenderParty).MapEvent;
			}
			else if (ForceSupplies)
			{
				_mapEvent = ForceSuppliesEventComponent.CreateForceSuppliesEvent(_attackerParty, _defenderParty).MapEvent;
			}
			else if (_defenderParty.IsSettlement)
			{
				if (_defenderParty.Settlement.IsFortification)
				{
					_mapEvent = Campaign.Current.MapEventManager.StartSiegeMapEvent(_attackerParty, _defenderParty);
				}
				else if (_defenderParty.Settlement.IsVillage)
				{
					_mapEvent = RaidEventComponent.CreateRaidEvent(_attackerParty, _defenderParty).MapEvent;
				}
				else if (_defenderParty.Settlement.IsHideout)
				{
					_mapEvent = HideoutEventComponent.CreateHideoutEvent(_attackerParty, _defenderParty).MapEvent;
				}
				else
				{
					Debug.FailedAssert("Proper mapEvent type could not be set for the battle.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Encounters\\PlayerEncounter.cs", "StartBattleInternal", 860);
				}
			}
			else if (_isSallyOutAmbush)
			{
				_mapEvent = SiegeAmbushEventComponent.CreateSiegeAmbushEvent(_attackerParty, _defenderParty).MapEvent;
			}
			else if (_attackerParty.IsMobile && _attackerParty.MobileParty.CurrentSettlement != null && _attackerParty.MobileParty.CurrentSettlement.SiegeEvent != null)
			{
				_mapEvent = Campaign.Current.MapEventManager.StartSallyOutMapEvent(_attackerParty, _defenderParty);
			}
			else if (_defenderParty.IsMobile && _defenderParty.MobileParty.BesiegedSettlement != null)
			{
				_mapEvent = Campaign.Current.MapEventManager.StartSiegeOutsideMapEvent(_attackerParty, _defenderParty);
			}
			else
			{
				_mapEvent = FieldBattleEventComponent.CreateFieldBattleEvent(_attackerParty, _defenderParty).MapEvent;
			}
		}
		CheckNearbyPartiesToJoinPlayerMapEvent();
		return _mapEvent;
	}

	public static MapEvent StartBattle()
	{
		return Current.StartBattleInternal();
	}

	private void JoinBattleInternal(BattleSideEnum side)
	{
		PlayerSide = side;
		switch (side)
		{
		case BattleSideEnum.Defender:
			OpponentSide = BattleSideEnum.Attacker;
			break;
		case BattleSideEnum.Attacker:
			OpponentSide = BattleSideEnum.Defender;
			break;
		}
		if (EncounteredBattle != null)
		{
			_mapEvent = EncounteredBattle;
			_encounteredParty = ((PlayerSide == BattleSideEnum.Attacker) ? EncounteredBattle.DefenderSide.LeaderParty : EncounteredBattle.AttackerSide.LeaderParty);
			PartiesStrengthRatioBeforePlayerJoin = CalculateStrengthOfParties();
			PartyBase.MainParty.MapEventSide = EncounteredBattle.GetMapEventSide(side);
			EncounterSettlementAux = _mapEvent.MapEventSettlement;
			if (EncounteredBattle.IsSiegeAssault && PlayerSide == BattleSideEnum.Attacker)
			{
				MobileParty.MainParty.BesiegerCamp = _encounteredParty.SiegeEvent.BesiegerCamp;
			}
			IsJoinedBattle = true;
			CheckNearbyPartiesToJoinPlayerMapEvent();
		}
		else
		{
			Finish(InsideSettlement);
		}
	}

	private float CalculateStrengthOfParties()
	{
		float num = 0f;
		float num2 = 0f;
		foreach (MapEventParty party in _mapEvent.DefenderSide.Parties)
		{
			num += party.Party.TotalStrength;
		}
		foreach (MapEventParty party2 in _mapEvent.AttackerSide.Parties)
		{
			num2 += party2.Party.TotalStrength;
		}
		return num / num2;
	}

	public static void JoinBattle(BattleSideEnum side)
	{
		Current.JoinBattleInternal(side);
	}

	private void PlayerSurrenderInternal()
	{
		_playerSurrender = true;
		if (Battle == null)
		{
			StartBattle();
		}
		_mapEvent.DoSurrender(PartyBase.MainParty.Side);
		MobileParty.MainParty.BesiegerCamp = null;
	}

	private void EnemySurrenderInternal()
	{
		_enemySurrender = true;
		_mapEvent.DoSurrender(PartyBase.MainParty.OpponentSide);
	}

	public static void Start()
	{
		Campaign.Current.PlayerEncounter = new PlayerEncounter();
	}

	public static void ProtectPlayerSide(float hoursToProtect = 1f)
	{
		MobileParty.MainParty.TeleportPartyToSafePosition();
		MobileParty.MainParty.IgnoreForHours(hoursToProtect);
	}

	public static void Finish(bool forcePlayerOutFromSettlement = true)
	{
		if (MobileParty.MainParty.Army == null || MobileParty.MainParty.Army.LeaderParty == EncounteredMobileParty)
		{
			Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
		}
		if (Campaign.Current.CurrentMenuContext != null)
		{
			GameMenu.ExitToLast();
		}
		int num;
		if (Current != null)
		{
			if (PlayerSiege.PlayerSiegeEvent != null && PlayerSiege.PlayerSide == BattleSideEnum.Attacker && MobileParty.MainParty.MapEvent != null && !MobileParty.MainParty.MapEvent.IsSiegeAssault && MobileParty.MainParty.MapEvent.HasWinner && MobileParty.MainParty.MapEvent.PlayerSide == BattleSideEnum.Defender)
			{
				num = ((MobileParty.MainParty.BesiegedSettlement != null) ? 1 : 0);
				if (num != 0)
				{
					goto IL_00b5;
				}
			}
			else
			{
				num = 0;
			}
			if (Current._isSiegeInterruptedByEnemyDefection)
			{
				goto IL_00b5;
			}
			goto IL_010e;
		}
		goto IL_01c1;
		IL_00b5:
		if (Hero.MainHero.PartyBelongedToAsPrisoner == null && !Current._leaveEncounter && Current._encounteredParty.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
		{
			GameMenu.ActivateGameMenu("continue_siege_after_attack");
			if (Current._isSiegeInterruptedByEnemyDefection)
			{
				Current._isSiegeInterruptedByEnemyDefection = false;
			}
		}
		goto IL_010e;
		IL_010e:
		if ((num != 0 || Current._isSiegeInterruptedByEnemyDefection) && Hero.MainHero.PartyBelongedToAsPrisoner != null && Current._leaveEncounter)
		{
			MobileParty.MainParty.BesiegerCamp = null;
		}
		Current.FirstInit = true;
		EncounterSettlement?.OnPlayerEncounterFinish();
		Current.FinalizeBattle();
		Current.FinishEncounterInternal();
		if (CurrentBattleSimulation != null)
		{
			MapState mapState = Game.Current.GameStateManager.LastOrDefault<MapState>();
			if (mapState != null && mapState.IsSimulationActive)
			{
				mapState.EndBattleSimulation();
			}
			Current.BattleSimulation = null;
		}
		if (InsideSettlement && MobileParty.MainParty.AttachedTo == null && forcePlayerOutFromSettlement)
		{
			LeaveSettlement();
		}
		goto IL_01c1;
		IL_01c1:
		Campaign.Current.PlayerEncounter = null;
		Campaign.Current.LocationEncounter = null;
		MobileParty.MainParty.Ai.SetMoveModeHold();
	}

	private void FinishEncounterInternal()
	{
		if (_encounteredParty != null && _encounteredParty.IsMobile && MobileParty.MainParty.AttachedTo == null && FactionManager.IsAtWarAgainstFaction(_encounteredParty.MapFaction, PartyBase.MainParty.MapFaction) && _encounteredParty.MobileParty.IsActive)
		{
			MobileParty.MainParty.TeleportPartyToSafePosition(0.3f, 0.1f);
			_encounteredParty.MobileParty.Ai.SetDoNotAttackMainParty(2);
		}
	}

	private void UpdateInternal()
	{
		_mapEvent = MapEvent.PlayerMapEvent;
		if (EncounteredPartySurrendered && EncounterState == PlayerEncounterState.Begin)
		{
			EncounterState = PlayerEncounterState.Wait;
		}
		_stateHandled = false;
		while (!_stateHandled)
		{
			if (Current._leaveEncounter)
			{
				Finish();
				_stateHandled = true;
			}
			if (!_stateHandled)
			{
				switch (EncounterState)
				{
				case PlayerEncounterState.Begin:
					DoBegin();
					break;
				case PlayerEncounterState.Wait:
					DoWait();
					break;
				case PlayerEncounterState.PrepareResults:
					DoPrepareResults();
					break;
				case PlayerEncounterState.ApplyResults:
					DoApplyResults();
					break;
				case PlayerEncounterState.PlayerVictory:
					DoPlayerVictory();
					break;
				case PlayerEncounterState.PlayerTotalDefeat:
					DoPlayerDefeat();
					break;
				case PlayerEncounterState.CaptureHeroes:
					DoCaptureHeroes();
					break;
				case PlayerEncounterState.FreeHeroes:
					DoFreeHeroes();
					break;
				case PlayerEncounterState.LootParty:
					DoLootParty();
					break;
				case PlayerEncounterState.LootInventory:
					DoLootInventory();
					break;
				case PlayerEncounterState.End:
					DoEnd();
					break;
				default:
					Debug.FailedAssert("[DEBUG]Invalid map event state: " + _mapEventState, "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Encounters\\PlayerEncounter.cs", "UpdateInternal", 1132);
					break;
				}
			}
		}
	}

	private void EndBattleByCheatInternal(bool playerWon)
	{
		if (!playerWon)
		{
			return;
		}
		foreach (MapEventParty item in _mapEvent.PartiesOnSide(OpponentSide))
		{
			for (int i = 0; i < item.Party.MemberRoster.Count; i++)
			{
				int elementNumber = item.Party.MemberRoster.GetElementNumber(i);
				int elementWoundedNumber = item.Party.MemberRoster.GetElementWoundedNumber(i);
				int maxValue = elementNumber - elementWoundedNumber;
				int num = elementWoundedNumber + MBRandom.RandomInt(maxValue);
				num = ((num <= 0 && elementNumber >= 0) ? 1 : num);
				item.Party.MemberRoster.SetElementNumber(i, num);
				item.Party.MemberRoster.SetElementWoundedNumber(i, num);
			}
		}
	}

	public static void EndBattleByCheat(bool playerWon)
	{
		Current.EndBattleByCheatInternal(playerWon);
	}

	public static void Update()
	{
		Current.UpdateInternal();
	}

	private void DoBegin()
	{
		EncounterState = PlayerEncounterState.Wait;
		_stateHandled = true;
	}

	public static void DoMeeting()
	{
		Current.DoMeetingInternal();
	}

	public static void SetMeetingDone()
	{
		Current._meetingDone = true;
	}

	private void DoMeetingInternal()
	{
		PartyBase partyBase = _encounteredParty;
		if (partyBase.IsSettlement)
		{
			foreach (MapEventParty party in MobileParty.MainParty.MapEvent.DefenderSide.Parties)
			{
				if (!party.Party.IsSettlement)
				{
					partyBase = party.Party;
					break;
				}
			}
		}
		EncounterState = PlayerEncounterState.Begin;
		_stateHandled = true;
		if (PlayerIsAttacker && _defenderParty.IsMobile && _defenderParty.MobileParty.Army != null && _defenderParty.MobileParty.Army.LeaderParty == _defenderParty.MobileParty && (_defenderParty.SiegeEvent != null || (!_defenderParty.MobileParty.MapFaction.IsAtWarWith(MobileParty.MainParty.MapFaction) && !_defenderParty.MobileParty.Army.LeaderParty.AttachedParties.Contains(MobileParty.MainParty))))
		{
			GameMenu.SwitchToMenu("army_encounter");
			return;
		}
		Campaign.Current.CurrentConversationContext = ConversationContext.PartyEncounter;
		_meetingDone = true;
		CharacterObject conversationCharacterPartyLeader = ConversationHelper.GetConversationCharacterPartyLeader(partyBase);
		ConversationCharacterData playerCharacterData = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, noHorse: true);
		ConversationCharacterData conversationPartnerData = new ConversationCharacterData(conversationCharacterPartyLeader, partyBase, noHorse: true);
		CampaignMapConversation.OpenConversation(playerCharacterData, conversationPartnerData);
	}

	private void ContinueBattle()
	{
		Debug.Print("[PlayerEncounter.ContinueBattle Start]");
		MapEventSide mapEventSide = _mapEvent.GetMapEventSide(_mapEvent.PlayerSide);
		MapEventSide otherSide = mapEventSide.OtherSide;
		_mapEvent.RecalculateStrengthOfSides();
		float num = _mapEvent.StrengthOfSide[(int)mapEventSide.MissionSide];
		float num2 = _mapEvent.StrengthOfSide[(int)otherSide.MissionSide];
		float num3 = num / (num + num2);
		Debug.Print("playerSideStrength: " + num);
		Debug.Print("otherSideStrength: " + num2);
		Debug.Print("playerSideStrengthRatio: " + num3);
		if (num3 >= 0.95f && otherSide.GetTotalHealthyHeroCountOfSide() <= 0)
		{
			Debug.Print("Player side wins according to the strength ratio.");
			_mapEvent?.SetOverrideWinner(_mapEvent.PlayerSide);
			EnemySurrender = true;
			EncounterState = PlayerEncounterState.PrepareResults;
		}
		else if (num3 < 0.05f && mapEventSide.GetTotalHealthyHeroCountOfSide() <= 0)
		{
			Debug.Print("Other side wins according to the strength ratio.");
			_mapEvent?.SetOverrideWinner(otherSide.MissionSide);
			EncounterState = PlayerEncounterState.PrepareResults;
		}
		else
		{
			Debug.Print("Battle continues.");
			Debug.Print("Other side strength by party:");
			foreach (MapEventParty party in otherSide.Parties)
			{
				Debug.Print(string.Concat("party: ", party.Party.Id, ": ", party.Party.Name, ", strength: ", party.Party.TotalStrength, ", healthy count: ", party.Party.MemberRoster.TotalHealthyCount, ", wounded count: ", party.Party.MemberRoster.TotalWounded));
			}
			_mapEvent.AttackerSide.CommitXpGains();
			_mapEvent.DefenderSide.CommitXpGains();
			_mapEvent.ApplyRenownAndInfluenceChanges();
			_mapEvent.SetOverrideWinner(BattleSideEnum.None);
			if (_mapEvent.IsSiegeAssault && otherSide == _mapEvent.AttackerSide)
			{
				CampaignBattleResult campaignBattleResult = _campaignBattleResult;
				if (campaignBattleResult != null && campaignBattleResult.EnemyRetreated)
				{
					List<MapEventParty> list = _mapEvent.AttackerSide.Parties.ToList();
					_mapEvent.FinishBattleAndKeepSiegeEvent();
					_mapEvent = null;
					foreach (MapEventParty item in list)
					{
						item.Party.SetVisualAsDirty();
						if (item.Party.IsMobile)
						{
							item.Party.MobileParty.Ai.SetMoveBesiegeSettlement(Settlement.CurrentSettlement);
							item.Party.MobileParty.Ai.RecalculateShortTermAi();
						}
					}
					GameMenu.ActivateGameMenu("menu_siege_strategies");
				}
			}
			_campaignBattleResult = null;
			_stateHandled = true;
		}
		Debug.Print("[PlayerEncounter.ContinueBattle End]");
	}

	private void DoWait()
	{
		MBTextManager.SetTextVariable("PARTY", MapEvent.PlayerMapEvent.GetLeaderParty(PartyBase.MainParty.OpponentSide).Name);
		if (!EncounteredPartySurrendered)
		{
			MBTextManager.SetTextVariable("ENCOUNTER_TEXT", GameTexts.FindText("str_you_have_encountered_PARTY"), sendClients: true);
		}
		else
		{
			MBTextManager.SetTextVariable("ENCOUNTER_TEXT", GameTexts.FindText("str_you_have_encountered_PARTY_they_surrendered"), sendClients: true);
		}
		if (CheckIfBattleShouldContinueAfterBattleMission())
		{
			ContinueBattle();
			return;
		}
		if (_mapEvent != null && _mapEvent.IsSiegeAssault)
		{
			_mapEvent.CheckIfOneSideHasLost();
			_campaignBattleResult = CampaignBattleResult.GetResult(_mapEvent.BattleState);
		}
		if (_campaignBattleResult != null && _campaignBattleResult.BattleResolved)
		{
			if (_campaignBattleResult.PlayerVictory)
			{
				_mapEvent?.SetOverrideWinner(PartyBase.MainParty.Side);
			}
			else
			{
				bool flag = true;
				if (_mapEvent != null && _mapEvent.IsHideoutBattle)
				{
					_mapEvent.MapEventSettlement.Hideout.UpdateNextPossibleAttackTime();
					if (_mapEvent.GetMapEventSide(PlayerSide).RecalculateMemberCountOfSide() > 0)
					{
						flag = false;
					}
				}
				if (flag)
				{
					_mapEvent?.SetOverrideWinner(PartyBase.MainParty.OpponentSide);
				}
			}
			EncounterState = PlayerEncounterState.PrepareResults;
		}
		else if (BattleSimulation != null && (BattleState == BattleState.AttackerVictory || BattleState == BattleState.DefenderVictory))
		{
			if (_mapEvent.WinningSide == PlayerSide)
			{
				EnemySurrender = true;
			}
			else
			{
				int totalManCount = MobileParty.MainParty.MemberRoster.TotalManCount;
				int totalWounded = MobileParty.MainParty.MemberRoster.TotalWounded;
				if (totalManCount - totalWounded == 0)
				{
					PlayerSurrender = true;
				}
			}
			EncounterState = PlayerEncounterState.PrepareResults;
		}
		else if (BattleSimulation != null && BattleSimulation.IsSimulationFinished && _mapEvent?.MapEventSettlement != null && BattleState == BattleState.None && _mapEvent.IsSiegeAssault && PlayerSiege.PlayerSiegeEvent != null)
		{
			_stateHandled = true;
			PlayerSiege.PlayerSiegeEvent.BreakSiegeEngine(PlayerSiege.PlayerSiegeEvent.GetSiegeEventSide(_mapEvent.PlayerSide), DefaultSiegeEngineTypes.Preparations);
		}
		else if (_mapEvent != null && (!_mapEvent.IsRaid || PlayerSurrender) && _mapEvent.HasWinner)
		{
			EncounterState = PlayerEncounterState.PrepareResults;
		}
		else
		{
			_stateHandled = true;
			if (IsJoinedBattle && Campaign.Current.CurrentMenuContext != null && Campaign.Current.CurrentMenuContext.GameMenu.StringId == "join_encounter")
			{
				LeaveBattle();
			}
			if (_mapEvent != null && _mapEvent.IsHideoutBattle)
			{
				_mapEvent.MapEventSettlement.Hideout.UpdateNextPossibleAttackTime();
			}
		}
	}

	public static bool CheckIfLeadingAvaliable()
	{
		bool flag = Hero.MainHero.PartyBelongedTo != null && !Hero.MainHero.IsWounded;
		bool flag2 = Hero.MainHero.PartyBelongedTo != null && Hero.MainHero.PartyBelongedTo.Army != null && Hero.MainHero.PartyBelongedTo.Army.ArmyOwner != Hero.MainHero;
		bool flag3 = false;
		foreach (MapEventParty item in MobileParty.MainParty.MapEvent.PartiesOnSide(MobileParty.MainParty.MapEvent.PlayerSide))
		{
			if (item.Party != MobileParty.MainParty.Party && item.Party.LeaderHero != null && item.Party.LeaderHero.Clan.Renown > Clan.PlayerClan.Renown)
			{
				flag3 = true;
				break;
			}
		}
		if (flag)
		{
			return flag2 || flag3;
		}
		return false;
	}

	public static Hero GetLeadingHero()
	{
		if (Hero.MainHero.PartyBelongedTo != null && Hero.MainHero.PartyBelongedTo.Army != null)
		{
			return MobileParty.MainParty.Army.ArmyOwner;
		}
		foreach (MapEventParty item in MobileParty.MainParty.MapEvent.PartiesOnSide(MobileParty.MainParty.MapEvent.PlayerSide))
		{
			if (item.Party != MobileParty.MainParty.Party && item.Party.LeaderHero != null && item.Party.LeaderHero.Clan.Renown > Clan.PlayerClan.Renown)
			{
				return item.Party.LeaderHero;
			}
		}
		return Hero.MainHero;
	}

	private void DoPrepareResults()
	{
		_mapEvent.CalculateBattleResults();
		EncounterState = PlayerEncounterState.ApplyResults;
	}

	public static void SetPlayerVictorious()
	{
		Current.SetPlayerVictoriousInternal();
	}

	public void SetIsSallyOutAmbush(bool value)
	{
		if (Current._isSallyOutAmbush && !value)
		{
			_campaignBattleResult = null;
		}
		Current._isSallyOutAmbush = value;
	}

	public void SetPlayerSiegeInterruptedByEnemyDefection()
	{
		Current._isSiegeInterruptedByEnemyDefection = true;
	}

	private void SetPlayerVictoriousInternal()
	{
		if (PlayerSide == BattleSideEnum.Attacker || PlayerSide == BattleSideEnum.Defender)
		{
			_mapEvent.SetOverrideWinner(PlayerSide);
		}
	}

	public static void SetPlayerSiegeContinueWithDefenderPullBack()
	{
		Current._mapEvent.SetDefenderPulledBack();
	}

	private void DoApplyResults()
	{
		CampaignEventDispatcher.Instance.OnPlayerBattleEnd(_mapEvent);
		_mapEvent.ApplyBattleResults();
		if (_mapEvent.WinningSide == PartyBase.MainParty.Side)
		{
			EncounterState = PlayerEncounterState.PlayerVictory;
		}
		else if (_mapEvent.DefeatedSide == PartyBase.MainParty.Side)
		{
			EncounterState = PlayerEncounterState.PlayerTotalDefeat;
		}
		else
		{
			EncounterState = PlayerEncounterState.End;
		}
	}

	public static void StartAttackMission()
	{
		Current._campaignBattleResult = new CampaignBattleResult();
	}

	private void DoPlayerVictory()
	{
		if (_helpedHeroes != null)
		{
			if (_helpedHeroes.Count > 0)
			{
				if (_helpedHeroes[0].DeathMark == KillCharacterAction.KillCharacterActionDetail.None)
				{
					Campaign.Current.CurrentConversationContext = ConversationContext.PartyEncounter;
					ConversationCharacterData playerCharacterData = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty);
					ConversationCharacterData conversationPartnerData = new ConversationCharacterData(_helpedHeroes[0].CharacterObject, _helpedHeroes[0].PartyBelongedTo.Party);
					CampaignMapConversation.OpenConversation(playerCharacterData, conversationPartnerData);
				}
				_helpedHeroes.RemoveAt(0);
				_stateHandled = true;
			}
			else
			{
				EncounterState = PlayerEncounterState.CaptureHeroes;
			}
			return;
		}
		_helpedHeroes = new List<Hero>();
		foreach (PartyBase involvedParty in MapEvent.PlayerMapEvent.InvolvedParties)
		{
			if (involvedParty != PartyBase.MainParty && involvedParty.Side == PartyBase.MainParty.Side && involvedParty.Owner != null && involvedParty.Owner != Hero.MainHero && involvedParty.LeaderHero != null && (MapEvent.PlayerMapEvent.AttackerSide.LeaderParty == involvedParty || MapEvent.PlayerMapEvent.DefenderSide.LeaderParty == involvedParty) && involvedParty.MobileParty != null && (involvedParty.MobileParty.Army == null || involvedParty.MobileParty.Army != MobileParty.MainParty.Army) && Campaign.Current.Models.BattleRewardModel.GetPlayerGainedRelationAmount(MapEvent.PlayerMapEvent, involvedParty.LeaderHero) > 0)
			{
				_helpedHeroes.Add(involvedParty.LeaderHero);
			}
		}
	}

	private void DoPlayerDefeat()
	{
		bool playerSurrender = PlayerSurrender;
		Finish();
		if (MobileParty.MainParty.BesiegerCamp != null)
		{
			if (MobileParty.MainParty.BesiegerCamp != null)
			{
				MobileParty.MainParty.BesiegerCamp = null;
			}
			else
			{
				PlayerSiege.ClosePlayerSiege();
			}
		}
		if (Hero.MainHero.DeathMark != KillCharacterAction.KillCharacterActionDetail.DiedInBattle)
		{
			GameMenu.ActivateGameMenu(playerSurrender ? "taken_prisoner" : "defeated_and_taken_prisoner");
		}
		_stateHandled = true;
	}

	private void DoCaptureHeroes()
	{
		TroopRoster prisonerRosterReceivingLootShare = _mapEvent.GetPrisonerRosterReceivingLootShare(PartyBase.MainParty);
		if (_capturedHeroes == null)
		{
			_capturedHeroes = prisonerRosterReceivingLootShare.RemoveIf((TroopRosterElement lordElement) => lordElement.Character.IsHero).ToList();
		}
		if (_capturedHeroes.Count > 0)
		{
			TroopRosterElement troopRosterElement = _capturedHeroes[_capturedHeroes.Count - 1];
			Campaign.Current.CurrentConversationContext = ConversationContext.CapturedLord;
			ConversationCharacterData playerCharacterData = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty);
			ConversationCharacterData conversationPartnerData = new ConversationCharacterData(troopRosterElement.Character, null, noHorse: true, noWeapon: true, spawnAfterFight: true);
			if (InsideSettlement && Settlement.CurrentSettlement.IsHideout)
			{
				CampaignMission.OpenConversationMission(playerCharacterData, conversationPartnerData);
			}
			else
			{
				CampaignMapConversation.OpenConversation(playerCharacterData, conversationPartnerData);
			}
			Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
			{
				_capturedHeroes.RemoveRange(_capturedHeroes.Count - 1, 1);
			};
			_stateHandled = true;
		}
		else
		{
			EncounterState = PlayerEncounterState.FreeHeroes;
		}
	}

	private void DoFreeHeroes()
	{
		TroopRoster memberRosterReceivingLootShare = _mapEvent.GetMemberRosterReceivingLootShare(PartyBase.MainParty);
		if (_freedHeroes == null)
		{
			_freedHeroes = memberRosterReceivingLootShare.RemoveIf((TroopRosterElement lordElement) => lordElement.Character.IsHero).ToList();
		}
		if (_freedHeroes.AnyQ((TroopRosterElement h) => h.Character.HeroObject.IsPrisoner))
		{
			TroopRosterElement troopRosterElement = _freedHeroes.Last((TroopRosterElement h) => h.Character.HeroObject.IsPrisoner);
			Campaign.Current.CurrentConversationContext = ConversationContext.FreedHero;
			ConversationCharacterData playerCharacterData = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty);
			ConversationCharacterData conversationPartnerData = new ConversationCharacterData(troopRosterElement.Character, null, noHorse: true, noWeapon: true);
			CampaignMapConversation.OpenConversation(playerCharacterData, conversationPartnerData);
			_stateHandled = true;
		}
		else
		{
			EncounterState = PlayerEncounterState.LootParty;
		}
	}

	private void DoLootInventory()
	{
		ItemRoster itemRosterReceivingLootShare = _mapEvent.GetItemRosterReceivingLootShare(PartyBase.MainParty);
		if (itemRosterReceivingLootShare.Count > 0)
		{
			InventoryManager.OpenScreenAsLoot(new Dictionary<PartyBase, ItemRoster> { 
			{
				PartyBase.MainParty,
				itemRosterReceivingLootShare
			} });
			_stateHandled = true;
		}
		EncounterState = PlayerEncounterState.End;
	}

	private void DoLootParty()
	{
		TroopRoster memberRosterReceivingLootShare = _mapEvent.GetMemberRosterReceivingLootShare(PartyBase.MainParty);
		TroopRoster prisonerRosterReceivingLootShare = _mapEvent.GetPrisonerRosterReceivingLootShare(PartyBase.MainParty);
		if (memberRosterReceivingLootShare.Count > 0 || prisonerRosterReceivingLootShare.Count > 0)
		{
			PartyScreenManager.OpenScreenAsLoot(memberRosterReceivingLootShare, prisonerRosterReceivingLootShare, TextObject.Empty, memberRosterReceivingLootShare.TotalManCount + prisonerRosterReceivingLootShare.TotalManCount);
			_stateHandled = true;
		}
		EncounterState = PlayerEncounterState.LootInventory;
	}

	public static void SacrificeTroops(int num, out TroopRoster losses, out ItemRoster lostBaggage)
	{
		Current.SacrificeTroopsImp(num, out losses, out lostBaggage);
	}

	private void SacrificeTroopsImp(int num, out TroopRoster losses, out ItemRoster lostBaggage)
	{
		losses = new TroopRoster(null);
		lostBaggage = new ItemRoster();
		_mapEvent.GetMapEventSide(PlayerSide).MakeReadyForSimulation(null);
		RemoveRandomTroops(num, losses);
		RemoveRandomItems(lostBaggage);
	}

	private void RemoveRandomItems(ItemRoster lostBaggage)
	{
		foreach (ItemRosterElement item in new ItemRoster(PartyBase.MainParty.ItemRoster))
		{
			if (!item.EquipmentElement.Item.NotMerchandise)
			{
				int num = MBRandom.RoundRandomized((float)item.Amount * 0.15f);
				PartyBase.MainParty.ItemRoster.AddToCounts(item.EquipmentElement, -num);
				lostBaggage.AddToCounts(item.EquipmentElement, num);
			}
		}
	}

	public void RemoveRandomTroops(int num, TroopRoster sacrifiedTroops)
	{
		int num2 = MobileParty.MainParty.Party.NumberOfRegularMembers;
		if (MobileParty.MainParty.Army != null)
		{
			foreach (MobileParty attachedParty in MobileParty.MainParty.Army.LeaderParty.AttachedParties)
			{
				num2 += attachedParty.Party.NumberOfRegularMembers;
			}
		}
		float sacrifaceRatio = (float)num / (float)num2;
		SacrifaceTroopsWithRatio(MobileParty.MainParty, sacrifaceRatio, sacrifiedTroops);
		if (MobileParty.MainParty.Army == null)
		{
			return;
		}
		foreach (MobileParty attachedParty2 in MobileParty.MainParty.Army.LeaderParty.AttachedParties)
		{
			SacrifaceTroopsWithRatio(attachedParty2, sacrifaceRatio, sacrifiedTroops);
		}
	}

	private void SacrifaceTroopsWithRatio(MobileParty mobileParty, float sacrifaceRatio, TroopRoster sacrifiedTroops)
	{
		int num = MBRandom.RoundRandomized((float)mobileParty.Party.NumberOfRegularMembers * sacrifaceRatio);
		for (int i = 0; i < num; i++)
		{
			float num2 = 100f;
			TroopRosterElement troopRosterElement = mobileParty.Party.MemberRoster.GetTroopRoster().FirstOrDefault();
			foreach (TroopRosterElement item in mobileParty.Party.MemberRoster.GetTroopRoster())
			{
				float num3 = (float)item.Character.Level - ((item.WoundedNumber > 0) ? 0.5f : 0f) - MBRandom.RandomFloat * 0.5f;
				if (!item.Character.IsHero && num3 < num2 && item.Number > 0)
				{
					num2 = num3;
					troopRosterElement = item;
				}
			}
			mobileParty.MemberRoster.AddToCounts(troopRosterElement.Character, -1, insertAtFront: false, (troopRosterElement.WoundedNumber > 0) ? (-1) : 0);
			sacrifiedTroops.AddToCounts(troopRosterElement.Character, 1);
		}
	}

	private void DoEnd()
	{
		bool num = _mapEvent?.IsSiegeAssault ?? false;
		bool isHideoutBattle = _mapEvent.IsHideoutBattle;
		bool flag = num && MobileParty.MainParty.MapEvent != null && MobileParty.MainParty.MapEvent == _mapEvent;
		bool flag2 = MobileParty.MainParty.MapEvent != null && PlayerSide == BattleSideEnum.Attacker;
		bool isRaid = _mapEvent.IsRaid;
		bool isForcingVolunteers = _mapEvent.IsForcingVolunteers;
		bool isForcingSupplies = _mapEvent.IsForcingSupplies;
		bool flag3 = BattleSimulation != null && _mapEvent.WinningSide != PlayerSide;
		Settlement mapEventSettlement = _mapEvent.MapEventSettlement;
		BattleState battleState = _mapEvent.BattleState;
		_stateHandled = true;
		if (!flag3)
		{
			Finish();
		}
		else
		{
			Battle.ResetBattleResults();
		}
		if (num)
		{
			if (mapEventSettlement == null)
			{
				return;
			}
			if (flag)
			{
				if (flag2)
				{
					EncounterManager.StartSettlementEncounter((MobileParty.MainParty.Army != null) ? MobileParty.MainParty.Army.LeaderParty : MobileParty.MainParty, mapEventSettlement);
					GameMenu.SwitchToMenu("menu_settlement_taken");
				}
			}
			else if (InsideSettlement)
			{
				LeaveSettlement();
			}
		}
		else if (isRaid || isForcingVolunteers || isForcingSupplies)
		{
			if ((_attackerParty.IsMobile && _attackerParty.MobileParty.Army != null && _attackerParty.MobileParty.Army.LeaderParty != _attackerParty.MobileParty) || !flag2 || _attackerParty != MobileParty.MainParty.Party)
			{
				return;
			}
			EncounterManager.StartSettlementEncounter(MobileParty.MainParty, mapEventSettlement);
			Current.ForceSupplies = isForcingSupplies;
			Current.ForceVolunteers = isForcingVolunteers;
			Current.ForceRaid = isRaid;
			BeHostileAction.ApplyEncounterHostileAction(PartyBase.MainParty, Settlement.CurrentSettlement.Party);
			if (isForcingSupplies)
			{
				GameMenu.SwitchToMenu("force_supplies_village");
			}
			else if (isForcingVolunteers)
			{
				GameMenu.SwitchToMenu("force_volunteers_village");
			}
			else if (isRaid)
			{
				if (InsideSettlement)
				{
					LeaveSettlementAction.ApplyForParty(MobileParty.MainParty);
				}
				StartBattle();
				GameMenu.SwitchToMenu("raiding_village");
				Current.ForceRaid = false;
				Current.ForceVolunteers = false;
				Current.ForceSupplies = false;
			}
		}
		else if (isHideoutBattle)
		{
			if (mapEventSettlement == null)
			{
				return;
			}
			switch (battleState)
			{
			case BattleState.AttackerVictory:
				if (mapEventSettlement.Parties.Count > 0)
				{
					foreach (MobileParty item in new List<MobileParty>(mapEventSettlement.Parties))
					{
						LeaveSettlementAction.ApplyForParty(item);
						item.Ai.SetDoNotAttackMainParty(3);
					}
				}
				mapEventSettlement.Hideout.IsSpotted = false;
				mapEventSettlement.IsVisible = false;
				break;
			case BattleState.None:
				EncounterManager.StartSettlementEncounter(MobileParty.MainParty, mapEventSettlement);
				GameMenu.SwitchToMenu("hideout_after_defeated_and_saved");
				break;
			}
		}
		else if (flag3)
		{
			EncounterState = PlayerEncounterState.Begin;
			GameMenu.SwitchToMenu("encounter");
		}
	}

	public bool CheckIfBattleShouldContinueAfterBattleMission()
	{
		if (_doesBattleContinue || _campaignBattleResult != null)
		{
			_doesBattleContinue = _mapEvent.CheckIfBattleShouldContinueAfterBattleMission(_campaignBattleResult);
		}
		return _doesBattleContinue;
	}

	public void FinalizeBattle()
	{
		if (_mapEvent == null)
		{
			return;
		}
		if (_mapEvent.HasWinner || _mapEvent.DiplomaticallyFinished || _mapEvent.IsSiegeAmbush || (_mapEvent.IsRaid && _mapEvent.MapEventSettlement.SettlementHitPoints.ApproximatelyEqualsTo(0f)))
		{
			MobileParty mobileParty = _mapEvent.AttackerSide.LeaderParty.MobileParty;
			bool flag = _mapEvent.IsRaid && _mapEvent.BattleState == BattleState.AttackerVictory && !_mapEvent.MapEventSettlement.SettlementHitPoints.ApproximatelyEqualsTo(0f);
			Settlement mapEventSettlement = _mapEvent.MapEventSettlement;
			_mapEvent.FinalizeEvent();
			_mapEvent = null;
			if (mobileParty != MobileParty.MainParty && flag)
			{
				mobileParty.Ai.SetMoveRaidSettlement(mapEventSettlement);
				mobileParty.Ai.RecalculateShortTermAi();
			}
		}
		else
		{
			LeaveBattle();
		}
	}

	public void FindNonAttachedNpcPartiesWhoWillJoinEvent(ref List<MobileParty> partiesToJoinPlayerSide, ref List<MobileParty> partiesToJoinEnemySide)
	{
		LocatableSearchData<MobileParty> data = MobileParty.StartFindingLocatablesAroundPosition(_mapEvent?.Position ?? MobileParty.MainParty.Position2D, 4f);
		MobileParty mobileParty = MobileParty.FindNextLocatable(ref data);
		List<MobileParty> list = new List<MobileParty>();
		List<MobileParty> list2 = new List<MobileParty>();
		while (mobileParty != null)
		{
			if (mobileParty != MobileParty.MainParty && mobileParty.MapEvent == null && mobileParty.SiegeEvent == null && mobileParty.CurrentSettlement == null && mobileParty.AttachedTo == null && (mobileParty.IsLordParty || mobileParty.IsBandit || mobileParty.ShouldJoinPlayerBattles))
			{
				if (!mobileParty.MapFaction.IsAtWarWith(MobileParty.MainParty.MapFaction) && mobileParty.MapFaction.IsAtWarWith(EncounteredParty.MapFaction))
				{
					list.Add(mobileParty);
				}
				if (mobileParty.MapFaction.IsAtWarWith(MobileParty.MainParty.MapFaction) && !mobileParty.MapFaction.IsAtWarWith(EncounteredParty.MapFaction))
				{
					list2.Add(mobileParty);
				}
			}
			mobileParty = MobileParty.FindNextLocatable(ref data);
		}
		if (list2.AnyQ((MobileParty t) => t.ShouldBeIgnored) || partiesToJoinEnemySide.AnyQ((MobileParty t) => t.ShouldBeIgnored))
		{
			Debug.Print("Ally parties wont join player encounter since there is an ignored party in enemy side");
			list.Clear();
		}
		if (list.AnyQ((MobileParty t) => t.ShouldBeIgnored) || partiesToJoinPlayerSide.AnyQ((MobileParty t) => t != MobileParty.MainParty && t.ShouldBeIgnored))
		{
			Debug.Print("Enemy parties wont join player encounter since there is an ignored party in ally side");
			list2.Clear();
		}
		partiesToJoinPlayerSide.AddRange(list.Except(partiesToJoinPlayerSide));
		partiesToJoinEnemySide.AddRange(list2.Except(partiesToJoinEnemySide));
	}

	public void FindAllNpcPartiesWhoWillJoinEvent(ref List<MobileParty> partiesToJoinPlayerSide, ref List<MobileParty> partiesToJoinEnemySide)
	{
		FindNonAttachedNpcPartiesWhoWillJoinEvent(ref partiesToJoinPlayerSide, ref partiesToJoinEnemySide);
		foreach (MobileParty item in partiesToJoinPlayerSide.ToList())
		{
			partiesToJoinPlayerSide.AddRange(item.AttachedParties.Except(partiesToJoinPlayerSide));
		}
		foreach (MobileParty item2 in partiesToJoinEnemySide.ToList())
		{
			partiesToJoinEnemySide.AddRange(item2.AttachedParties.Except(partiesToJoinEnemySide));
		}
	}

	public static void EnterSettlement()
	{
		Settlement encounterSettlement = EncounterSettlement;
		CreateLocationEncounter(encounterSettlement);
		EnterSettlementAction.ApplyForParty(MobileParty.MainParty, encounterSettlement);
	}

	private static void CreateLocationEncounter(Settlement settlement)
	{
		if (settlement.IsTown)
		{
			LocationEncounter = new TownEncounter(settlement);
		}
		else if (settlement.IsVillage)
		{
			LocationEncounter = new VillageEncounter(settlement);
		}
		else if (settlement.IsCastle)
		{
			LocationEncounter = new CastleEncounter(settlement);
		}
	}

	public static void LeaveBattle()
	{
		MapEvent playerMapEvent = MapEvent.PlayerMapEvent;
		bool flag = false;
		if (playerMapEvent != null)
		{
			int numberOfInvolvedMen = playerMapEvent.GetNumberOfInvolvedMen(PartyBase.MainParty.Side);
			Army playerArmy = MobileParty.MainParty.Army;
			if ((PartyBase.MainParty.MapEventSide.LeaderParty != PartyBase.MainParty && PartyBase.MainParty.MapEventSide.Parties.Any((MapEventParty p) => p.IsNpcParty && (playerArmy == null || p.Party.MobileParty?.Army != playerArmy))) || (PartyBase.MainParty.MapEvent.IsSallyOut && Campaign.Current.Models.EncounterModel.GetLeaderOfMapEvent(PartyBase.MainParty.MapEvent, PartyBase.MainParty.MapEventSide.MissionSide) != Hero.MainHero))
			{
				PartyBase.MainParty.MapEventSide = null;
			}
			else
			{
				playerMapEvent.FinalizeEvent();
			}
			flag = numberOfInvolvedMen > PartyBase.MainParty.NumberOfHealthyMembers && playerMapEvent.AttackerSide.LeaderParty != PartyBase.MainParty && playerMapEvent.DefenderSide.LeaderParty != PartyBase.MainParty;
		}
		if (CurrentBattleSimulation != null)
		{
			MapState mapState = Game.Current.GameStateManager.LastOrDefault<MapState>();
			if (mapState != null && mapState.IsSimulationActive)
			{
				mapState.EndBattleSimulation();
			}
			Current.BattleSimulation = null;
			Current._mapEvent.BattleObserver = null;
		}
		Current.IsJoinedBattle = false;
		Current._mapEvent = null;
		if (flag && !playerMapEvent.HasWinner)
		{
			playerMapEvent.SimulateBattleSetup(Current.BattleSimulation?.SelectedTroops);
		}
	}

	public static void LeaveSettlement()
	{
		LeaveSettlementAction.ApplyForParty(MobileParty.MainParty);
		LocationEncounter = null;
		PartyBase.MainParty.SetVisualAsDirty();
	}

	public static void InitSimulation(FlattenedTroopRoster selectedTroopsForPlayerSide, FlattenedTroopRoster selectedTroopsForOtherSide)
	{
		if (Current != null)
		{
			Current.BattleSimulation = new BattleSimulation(selectedTroopsForPlayerSide, selectedTroopsForOtherSide);
		}
	}

	public void InterruptEncounter(string encounterInterrupedType)
	{
		_ = Game.Current.GameStateManager.ActiveState;
		if (MapEvent.PlayerMapEvent != null)
		{
			LeaveBattle();
		}
		GameMenu.ActivateGameMenu(encounterInterrupedType);
	}

	public static void StartSiegeAmbushMission()
	{
		Settlement mapEventSettlement = Battle.MapEventSettlement;
		SiegeEvent playerSiegeEvent = PlayerSiege.PlayerSiegeEvent;
		switch (mapEventSettlement.CurrentSiegeState)
		{
		case Settlement.SiegeState.OnTheWalls:
		{
			List<MissionSiegeWeapon> preparedAndActiveSiegeEngines = playerSiegeEvent.GetPreparedAndActiveSiegeEngines(playerSiegeEvent.GetSiegeEventSide(BattleSideEnum.Attacker));
			List<MissionSiegeWeapon> preparedAndActiveSiegeEngines2 = playerSiegeEvent.GetPreparedAndActiveSiegeEngines(playerSiegeEvent.GetSiegeEventSide(BattleSideEnum.Defender));
			bool hasAnySiegeTower = preparedAndActiveSiegeEngines.Exists((MissionSiegeWeapon data) => data.Type == DefaultSiegeEngineTypes.SiegeTower);
			int wallLevel = mapEventSettlement.Town.GetWallLevel();
			CampaignMission.OpenSiegeMissionWithDeployment(mapEventSettlement.LocationComplex.GetLocationWithId("center").GetSceneName(wallLevel), mapEventSettlement.SettlementWallSectionHitPointsRatioList.ToArray(), hasAnySiegeTower, preparedAndActiveSiegeEngines, preparedAndActiveSiegeEngines2, Current.PlayerSide == BattleSideEnum.Attacker, wallLevel, isSallyOut: true);
			break;
		}
		case Settlement.SiegeState.InTheLordsHall:
		case Settlement.SiegeState.Invalid:
			Debug.FailedAssert("Siege state is invalid!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Encounters\\PlayerEncounter.cs", "StartSiegeAmbushMission", 2223);
			break;
		}
	}

	public static void StartVillageBattleMission()
	{
		Settlement mapEventSettlement = Battle.MapEventSettlement;
		int upgradeLevel = ((!mapEventSettlement.IsTown) ? 1 : mapEventSettlement.Town.GetWallLevel());
		CampaignMission.OpenBattleMission(mapEventSettlement.LocationComplex.GetScene("village_center", upgradeLevel), usesTownDecalAtlas: true);
	}

	public static void StartCombatMissionWithDialogueInTownCenter(CharacterObject characterToTalkTo)
	{
		int wallLevel = Settlement.CurrentSettlement.Town.GetWallLevel();
		CampaignMission.OpenCombatMissionWithDialogue(Settlement.CurrentSettlement.LocationComplex.GetScene("center", wallLevel), characterToTalkTo, wallLevel);
	}

	public static void StartHostileAction()
	{
		Current.StartHostileActionInternal();
	}

	private void StartHostileActionInternal()
	{
		if (_mapEvent != null)
		{
			if (InsideSettlement)
			{
				LeaveSettlement();
			}
			Update();
		}
	}

	public void FinishRaid()
	{
		bool diplomaticallyFinished = Battle.DiplomaticallyFinished;
		PartyBase leaderParty = Battle.AttackerSide.LeaderParty;
		Finish();
		if (!diplomaticallyFinished)
		{
			if (leaderParty == MobileParty.MainParty.Party)
			{
				GameMenu.ActivateGameMenu("village_player_raid_ended");
			}
			else
			{
				GameMenu.ActivateGameMenu("village_raid_ended_leaded_by_someone_else");
			}
		}
	}

	public static void GetBattleRewards(out float renownChange, out float influenceChange, out float moraleChange, out float goldChange, out float playerEarnedLootPercentage, ref ExplainedNumber renownExplainedNumber, ref ExplainedNumber influenceExplainedNumber, ref ExplainedNumber moraleExplainedNumber)
	{
		if (Current == null)
		{
			renownChange = 0f;
			influenceChange = 0f;
			moraleChange = 0f;
			goldChange = 0f;
			playerEarnedLootPercentage = 0f;
		}
		else
		{
			Current.GetBattleRewardsInternal(out renownChange, out influenceChange, out moraleChange, out goldChange, out playerEarnedLootPercentage, ref renownExplainedNumber, ref influenceExplainedNumber, ref moraleExplainedNumber);
		}
	}

	private void GetBattleRewardsInternal(out float renownChange, out float influenceChange, out float moraleChange, out float goldChange, out float playerEarnedLootPercentage, ref ExplainedNumber renownExplainedNumber, ref ExplainedNumber influenceExplainedNumber, ref ExplainedNumber moraleExplainedNumber)
	{
		MapEventResultExplainer battleResultExplainers = _mapEvent.BattleResultExplainers;
		_mapEvent.GetBattleRewards(PartyBase.MainParty, out renownChange, out influenceChange, out moraleChange, out goldChange, out playerEarnedLootPercentage);
		if (battleResultExplainers != null)
		{
			renownExplainedNumber = battleResultExplainers.RenownExplainedNumber;
			influenceExplainedNumber = battleResultExplainers.InfluenceExplainedNumber;
			moraleExplainedNumber = battleResultExplainers.MoraleExplainedNumber;
		}
	}

	public float GetPlayerStrengthRatioInEncounter()
	{
		List<MobileParty> partiesToJoinPlayerSide = new List<MobileParty> { MobileParty.MainParty };
		List<MobileParty> partiesToJoinEnemySide = new List<MobileParty> { MobileParty.ConversationParty };
		FindAllNpcPartiesWhoWillJoinEvent(ref partiesToJoinPlayerSide, ref partiesToJoinEnemySide);
		float num = 0f;
		float num2 = 0f;
		foreach (MobileParty item in partiesToJoinPlayerSide)
		{
			if (item != null)
			{
				num += item.Party.TotalStrength;
			}
			else
			{
				Debug.FailedAssert("Player side party null", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Encounters\\PlayerEncounter.cs", "GetPlayerStrengthRatioInEncounter", 2332);
			}
		}
		foreach (MobileParty item2 in partiesToJoinEnemySide)
		{
			if (item2 != null)
			{
				num2 += item2.Party.TotalStrength;
			}
			else
			{
				Debug.FailedAssert("Opponent side party null", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Encounters\\PlayerEncounter.cs", "GetPlayerStrengthRatioInEncounter", 2344);
			}
		}
		if (num2 <= 0f)
		{
			num2 = 1E-05f;
		}
		return num / num2;
	}
}
