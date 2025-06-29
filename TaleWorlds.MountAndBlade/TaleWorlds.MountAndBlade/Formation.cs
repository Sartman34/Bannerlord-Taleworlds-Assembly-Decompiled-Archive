using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;

namespace TaleWorlds.MountAndBlade;

public sealed class Formation : IFormation
{
	private class AgentArrangementData : IFormationUnit
	{
		public IFormationArrangement Formation { get; private set; }

		public int FormationFileIndex { get; set; } = -1;


		public int FormationRankIndex { get; set; } = -1;


		public IFormationUnit FollowedUnit { get; }

		public bool IsShieldUsageEncouraged => true;

		public bool IsPlayerUnit => false;

		public AgentArrangementData(int index, IFormationArrangement arrangement)
		{
			Formation = arrangement;
		}
	}

	public class RetreatPositionCacheSystem
	{
		private List<(Vec2, WorldPosition)> _retreatPositionDistance;

		public RetreatPositionCacheSystem(int cacheCount)
		{
			_retreatPositionDistance = new List<(Vec2, WorldPosition)>(2);
		}

		public WorldPosition GetRetreatPositionFromCache(Vec2 agentPosition)
		{
			for (int num = _retreatPositionDistance.Count - 1; num >= 0; num--)
			{
				if (_retreatPositionDistance[num].Item1.DistanceSquared(agentPosition) < 400f)
				{
					return _retreatPositionDistance[num].Item2;
				}
			}
			return WorldPosition.Invalid;
		}

		public void AddNewPositionToCache(Vec2 agentPostion, WorldPosition retreatingPosition)
		{
			if (_retreatPositionDistance.Count >= 2)
			{
				_retreatPositionDistance.RemoveAt(0);
			}
			_retreatPositionDistance.Add((agentPostion, retreatingPosition));
		}
	}

	public const float AveragePositionCalculatePeriod = 0.05f;

	public const int MinimumUnitSpacing = 0;

	public const int MaximumUnitSpacing = 2;

	public const int RetreatPositionDistanceCacheCount = 2;

	public const float RetreatPositionCacheUseDistanceSquared = 400f;

	private static Formation _simulationFormationTemp;

	private static int _simulationFormationUniqueIdentifier;

	public readonly Team Team;

	public readonly int Index;

	public readonly FormationClass FormationIndex;

	public Banner Banner;

	public bool HasBeenPositioned;

	public Vec2? ReferencePosition;

	private FormationClass _representativeClass = FormationClass.NumberOfAllFormations;

	private bool _logicalClassNeedsUpdate;

	private FormationClass _logicalClass = FormationClass.NumberOfAllFormations;

	private int[] _logicalClassCounts = new int[4];

	private Agent _playerOwner;

	private string _bannerCode;

	private bool _isAIControlled = true;

	private bool _enforceNotSplittableByAI = true;

	private WorldPosition _orderPosition;

	private Vec2 _direction;

	private int _unitSpacing;

	private Vec2 _orderLocalAveragePosition;

	private bool _orderLocalAveragePositionIsDirty = true;

	private int _formationOrderDefensivenessFactor = 2;

	private MovementOrder _movementOrder;

	private FacingOrder _facingOrder;

	private ArrangementOrder _arrangementOrder;

	private Timer _arrangementOrderTickOccasionallyTimer;

	private FormOrder _formOrder;

	private RidingOrder _ridingOrder;

	private FiringOrder _firingOrder;

	private Agent _captain;

	private Vec2 _smoothedAverageUnitPosition = Vec2.Invalid;

	private MBList<IDetachment> _detachments;

	private IFormationArrangement _arrangement;

	private int[] _agentIndicesCache;

	private MBList<Agent> _detachedUnits;

	private int _undetachableNonPlayerUnitCount;

	private MBList<Agent> _looseDetachedUnits;

	private bool? _overridenHasAnyMountedUnit;

	private bool _isArrangementShapeChanged;

	private int _currentSpawnIndex;

	private Formation _targetFormation;

	public RetreatPositionCacheSystem RetreatPositionCache { get; private set; } = new RetreatPositionCacheSystem(2);


	public int CountOfUnits => Arrangement.UnitCount + _detachedUnits.Count;

	public int CountOfDetachedUnits => _detachedUnits.Count;

	public int CountOfUndetachableNonPlayerUnits => _undetachableNonPlayerUnitCount;

	public int CountOfUnitsWithoutDetachedOnes => Arrangement.UnitCount + _looseDetachedUnits.Count;

	public MBReadOnlyList<IFormationUnit> UnitsWithoutLooseDetachedOnes => Arrangement.GetAllUnits();

	public int CountOfUnitsWithoutLooseDetachedOnes => Arrangement.UnitCount;

	public int CountOfDetachableNonplayerUnits => Arrangement.UnitCount - ((IsPlayerTroopInFormation || HasPlayerControlledTroop) ? 1 : 0) - CountOfUndetachableNonPlayerUnits;

	public Vec2 OrderPosition => _orderPosition.AsVec2;

	public Vec3 OrderGroundPosition => _orderPosition.GetGroundVec3();

	public bool OrderPositionIsValid => _orderPosition.IsValid;

	public float Depth => Arrangement.Depth;

	public float MinimumWidth => Arrangement.MinimumWidth;

	public float MaximumWidth => Arrangement.MaximumWidth;

	public float UnitDiameter => GetDefaultUnitDiameter(CalculateHasSignificantNumberOfMounted && !(RidingOrder == RidingOrder.RidingOrderDismount));

	public Vec2 Direction => _direction;

	public Vec2 CurrentDirection => (QuerySystem.EstimatedDirection * 0.8f + Direction * 0.2f).Normalized();

	public Vec2 SmoothedAverageUnitPosition => _smoothedAverageUnitPosition;

	public int UnitSpacing => _unitSpacing;

	public MBReadOnlyList<Agent> LooseDetachedUnits => _looseDetachedUnits;

	public MBReadOnlyList<Agent> DetachedUnits => _detachedUnits;

	public AttackEntityOrderDetachment AttackEntityOrderDetachment { get; private set; }

	public FormationAI AI { get; private set; }

	public Formation TargetFormation
	{
		get
		{
			return _targetFormation;
		}
		private set
		{
			if (_targetFormation != value)
			{
				_targetFormation = value;
				ApplyActionOnEachUnit(delegate(Agent agent)
				{
					agent.SetTargetFormationIndex(value?.Index ?? (-1));
				});
			}
		}
	}

	public FormationQuerySystem QuerySystem { get; private set; }

	public MBReadOnlyList<IDetachment> Detachments => _detachments;

	public int? OverridenUnitCount { get; private set; }

	public bool IsSpawning { get; private set; }

	public bool IsAITickedAfterSplit { get; set; }

	public bool HasPlayerControlledTroop { get; private set; }

	public bool IsPlayerTroopInFormation { get; private set; }

	public bool ContainsAgentVisuals { get; set; }

	public Agent PlayerOwner
	{
		get
		{
			return _playerOwner;
		}
		set
		{
			_playerOwner = value;
			SetControlledByAI(value == null);
		}
	}

	public string BannerCode
	{
		get
		{
			return _bannerCode;
		}
		set
		{
			_bannerCode = value;
			if (GameNetwork.IsServer)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new InitializeFormation(this, Team.TeamIndex, _bannerCode));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
			}
		}
	}

	public bool IsSplittableByAI
	{
		get
		{
			if (IsAIOwned)
			{
				return IsConvenientForTransfer;
			}
			return false;
		}
	}

	public bool IsAIOwned
	{
		get
		{
			if (!_enforceNotSplittableByAI)
			{
				if (!IsAIControlled)
				{
					if (!Team.IsPlayerGeneral)
					{
						if (Team.IsPlayerSergeant)
						{
							return PlayerOwner != Agent.Main;
						}
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsConvenientForTransfer
	{
		get
		{
			if (Mission.Current.MissionTeamAIType == Mission.MissionTeamAITypeEnum.Siege && Team.Side == BattleSideEnum.Attacker)
			{
				return QuerySystem.InsideCastleUnitCountIncludingUnpositioned == 0;
			}
			return true;
		}
	}

	public bool EnforceNotSplittableByAI => _enforceNotSplittableByAI;

	public bool IsAIControlled => _isAIControlled;

	public Vec2 OrderLocalAveragePosition
	{
		get
		{
			if (_orderLocalAveragePositionIsDirty)
			{
				_orderLocalAveragePositionIsDirty = false;
				_orderLocalAveragePosition = default(Vec2);
				if (UnitsWithoutLooseDetachedOnes.Count > 0)
				{
					int num = 0;
					foreach (IFormationUnit unitsWithoutLooseDetachedOne in UnitsWithoutLooseDetachedOnes)
					{
						Vec2? localPositionOfUnitOrDefault = Arrangement.GetLocalPositionOfUnitOrDefault(unitsWithoutLooseDetachedOne);
						if (localPositionOfUnitOrDefault.HasValue)
						{
							_orderLocalAveragePosition += localPositionOfUnitOrDefault.Value;
							num++;
						}
					}
					if (num > 0)
					{
						_orderLocalAveragePosition *= 1f / (float)num;
					}
				}
			}
			return _orderLocalAveragePosition;
		}
	}

	public FacingOrder FacingOrder
	{
		get
		{
			return _facingOrder;
		}
		set
		{
			_facingOrder = value;
		}
	}

	public ArrangementOrder ArrangementOrder
	{
		get
		{
			return _arrangementOrder;
		}
		set
		{
			if (value.OrderType != _arrangementOrder.OrderType)
			{
				_arrangementOrder.OnCancel(this);
				int arrangementOrderDefensivenessChange = ArrangementOrder.GetArrangementOrderDefensivenessChange(_arrangementOrder.OrderEnum, value.OrderEnum);
				if (arrangementOrderDefensivenessChange != 0 && MovementOrder.GetMovementOrderDefensiveness(_movementOrder.OrderEnum) != 0)
				{
					_formationOrderDefensivenessFactor += arrangementOrderDefensivenessChange;
					UpdateAgentDrivenPropertiesBasedOnOrderDefensiveness();
				}
				if (FormOrder.OrderEnum == FormOrder.FormOrderEnum.Custom)
				{
					FormOrder = FormOrder.FormOrderCustom(TransformCustomWidthBetweenArrangementOrientations(_arrangementOrder.OrderEnum, value.OrderEnum, Arrangement.FlankWidth));
				}
				_arrangementOrder = value;
				_arrangementOrder.OnApply(this);
				this.OnAfterArrangementOrderApplied?.Invoke(this, _arrangementOrder.OrderEnum);
			}
			else
			{
				_arrangementOrder.SoftUpdate(this);
			}
		}
	}

	public FormOrder FormOrder
	{
		get
		{
			return _formOrder;
		}
		set
		{
			_formOrder = value;
			_formOrder.OnApply(this);
		}
	}

	public RidingOrder RidingOrder
	{
		get
		{
			return _ridingOrder;
		}
		set
		{
			if (_ridingOrder != value)
			{
				_ridingOrder = value;
				ApplyActionOnEachUnit(delegate(Agent agent)
				{
					agent.SetRidingOrder(value.OrderEnum);
				});
				Arrangement_OnShapeChanged();
			}
		}
	}

	public FiringOrder FiringOrder
	{
		get
		{
			return _firingOrder;
		}
		set
		{
			if (_firingOrder != value)
			{
				_firingOrder = value;
				ApplyActionOnEachUnit(delegate(Agent agent)
				{
					agent.SetFiringOrder(value.OrderEnum);
				});
			}
		}
	}

	private bool IsSimulationFormation => Team == null;

	public bool HasAnyMountedUnit
	{
		get
		{
			if (_overridenHasAnyMountedUnit.HasValue)
			{
				return _overridenHasAnyMountedUnit.Value;
			}
			int num = (int)(QuerySystem.GetRangedCavalryUnitRatioWithoutExpiration * (float)CountOfUnits + 1E-05f);
			int num2 = (int)(QuerySystem.GetCavalryUnitRatioWithoutExpiration * (float)CountOfUnits + 1E-05f);
			return num + num2 > 0;
		}
	}

	public float Width
	{
		get
		{
			return Arrangement.Width;
		}
		private set
		{
			Arrangement.Width = value;
		}
	}

	public bool IsDeployment => Mission.Current.GetMissionBehavior<BattleDeploymentHandler>() != null;

	public FormationClass RepresentativeClass => _representativeClass;

	public FormationClass LogicalClass => _logicalClass;

	public IEnumerable<FormationClass> SecondaryLogicalClasses
	{
		get
		{
			FormationClass primaryLogicalClass = LogicalClass;
			if (primaryLogicalClass == FormationClass.NumberOfAllFormations)
			{
				yield break;
			}
			List<(FormationClass, int)> list = new List<(FormationClass, int)>();
			for (int i = 0; i < _logicalClassCounts.Length; i++)
			{
				if (_logicalClassCounts[i] > 0)
				{
					list.Add(((FormationClass)i, _logicalClassCounts[i]));
				}
			}
			if (list.Count <= 0)
			{
				yield break;
			}
			list.Sort(Comparer<(FormationClass, int)>.Create(((FormationClass fClass, int count) x, (FormationClass fClass, int count) y) => (x.count < y.count) ? 1 : ((x.count > y.count) ? (-1) : 0)));
			foreach (var item in list)
			{
				if (item.Item1 != primaryLogicalClass)
				{
					yield return item.Item1;
				}
			}
		}
	}

	public IFormationArrangement Arrangement
	{
		get
		{
			return _arrangement;
		}
		set
		{
			if (_arrangement != null)
			{
				_arrangement.OnWidthChanged -= Arrangement_OnWidthChanged;
				_arrangement.OnShapeChanged -= Arrangement_OnShapeChanged;
			}
			_arrangement = value;
			if (_arrangement != null)
			{
				_arrangement.OnWidthChanged += Arrangement_OnWidthChanged;
				_arrangement.OnShapeChanged += Arrangement_OnShapeChanged;
			}
			Arrangement_OnWidthChanged();
			Arrangement_OnShapeChanged();
		}
	}

	public FormationClass PhysicalClass => QuerySystem.MainClass;

	public IEnumerable<FormationClass> SecondaryPhysicalClasses
	{
		get
		{
			FormationClass primaryPhysicalClass = PhysicalClass;
			if (primaryPhysicalClass != 0 && QuerySystem.InfantryUnitRatio > 0f)
			{
				yield return FormationClass.Infantry;
			}
			if (primaryPhysicalClass != FormationClass.Ranged && QuerySystem.RangedUnitRatio > 0f)
			{
				yield return FormationClass.Ranged;
			}
			if (primaryPhysicalClass != FormationClass.Cavalry && QuerySystem.CavalryUnitRatio > 0f)
			{
				yield return FormationClass.Cavalry;
			}
			if (primaryPhysicalClass != FormationClass.HorseArcher && QuerySystem.RangedCavalryUnitRatio > 0f)
			{
				yield return FormationClass.HorseArcher;
			}
		}
	}

	public float Interval
	{
		get
		{
			if (CalculateHasSignificantNumberOfMounted && !(RidingOrder == RidingOrder.RidingOrderDismount))
			{
				return CavalryInterval(UnitSpacing);
			}
			return InfantryInterval(UnitSpacing);
		}
	}

	public bool CalculateHasSignificantNumberOfMounted
	{
		get
		{
			if (_overridenHasAnyMountedUnit.HasValue)
			{
				return _overridenHasAnyMountedUnit.Value;
			}
			return QuerySystem.CavalryUnitRatio + QuerySystem.RangedCavalryUnitRatio >= 0.1f;
		}
	}

	public float Distance
	{
		get
		{
			if (CalculateHasSignificantNumberOfMounted && !(RidingOrder == RidingOrder.RidingOrderDismount))
			{
				return CavalryDistance(UnitSpacing);
			}
			return InfantryDistance(UnitSpacing);
		}
	}

	public Vec2 CurrentPosition => QuerySystem.GetAveragePositionWithMaxAge(0.1f) + CurrentDirection.TransformToParentUnitF(-OrderLocalAveragePosition);

	public Agent Captain
	{
		get
		{
			return _captain;
		}
		set
		{
			if (_captain != value)
			{
				_captain = value;
				OnCaptainChanged();
			}
		}
	}

	public float MinimumDistance => GetDefaultMinimumDistance(HasAnyMountedUnit && !(RidingOrder == RidingOrder.RidingOrderDismount));

	public bool IsLoose => ArrangementOrder.GetUnitLooseness(ArrangementOrder.OrderEnum);

	public float MinimumInterval => GetDefaultMinimumInterval(HasAnyMountedUnit && !(RidingOrder == RidingOrder.RidingOrderDismount));

	public float MaximumInterval => GetDefaultMaximumInterval(HasAnyMountedUnit && !(RidingOrder == RidingOrder.RidingOrderDismount));

	public float MaximumDistance => GetDefaultMaximumDistance(HasAnyMountedUnit && !(RidingOrder == RidingOrder.RidingOrderDismount));

	internal bool PostponeCostlyOperations { get; private set; }

	public event Action<Formation, Agent> OnUnitAdded;

	public event Action<Formation, Agent> OnUnitRemoved;

	public event Action<Formation> OnUnitCountChanged;

	public event Action<Formation> OnUnitSpacingChanged;

	public event Action<Formation> OnTick;

	public event Action<Formation> OnWidthChanged;

	public event Action<Formation, MovementOrder.MovementOrderEnum> OnBeforeMovementOrderApplied;

	public event Action<Formation, ArrangementOrder.ArrangementOrderEnum> OnAfterArrangementOrderApplied;

	public Formation(Team team, int index)
	{
		Team = team;
		Index = index;
		FormationIndex = (FormationClass)index;
		IsSpawning = false;
		Reset();
	}

	~Formation()
	{
		if (!IsSimulationFormation)
		{
			_simulationFormationTemp = null;
		}
	}

	bool IFormation.GetIsLocalPositionAvailable(Vec2 localPosition, Vec2? nearestAvailableUnitPositionLocal)
	{
		Vec2 vec = Direction.TransformToParentUnitF(localPosition);
		WorldPosition unitPosition = CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.NavMeshVec3);
		unitPosition.SetVec2(OrderPosition + vec);
		WorldPosition nearestAvailableUnitPosition = WorldPosition.Invalid;
		if (nearestAvailableUnitPositionLocal.HasValue)
		{
			vec = Direction.TransformToParentUnitF(nearestAvailableUnitPositionLocal.Value);
			nearestAvailableUnitPosition = CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.NavMeshVec3);
			nearestAvailableUnitPosition.SetVec2(OrderPosition + vec);
		}
		float manhattanDistance = TaleWorlds.Library.MathF.Abs(localPosition.x) + TaleWorlds.Library.MathF.Abs(localPosition.y) + (Interval + Distance) * 2f;
		return Mission.Current.IsFormationUnitPositionAvailable(ref _orderPosition, ref unitPosition, ref nearestAvailableUnitPosition, manhattanDistance, Team);
	}

	IFormationUnit IFormation.GetClosestUnitTo(Vec2 localPosition, MBList<IFormationUnit> unitsWithSpaces, float? maxDistance)
	{
		Vec2 vec = Direction.TransformToParentUnitF(localPosition);
		Vec2 position = OrderPosition + vec;
		return GetClosestUnitToAux(position, unitsWithSpaces, maxDistance);
	}

	IFormationUnit IFormation.GetClosestUnitTo(IFormationUnit targetUnit, MBList<IFormationUnit> unitsWithSpaces, float? maxDistance)
	{
		return GetClosestUnitToAux(((Agent)targetUnit).Position.AsVec2, unitsWithSpaces, maxDistance);
	}

	void IFormation.SetUnitToFollow(IFormationUnit unit, IFormationUnit toFollow, Vec2 vector)
	{
		Agent obj = unit as Agent;
		Agent followAgent = toFollow as Agent;
		obj.SetColumnwiseFollowAgent(followAgent, ref vector);
	}

	bool IFormation.BatchUnitPositions(MBArrayList<Vec2i> orderedPositionIndices, MBArrayList<Vec2> orderedLocalPositions, MBList2D<int> availabilityTable, MBList2D<WorldPosition> globalPositionTable, int fileCount, int rankCount)
	{
		if (_orderPosition.IsValid && _orderPosition.GetNavMesh() != UIntPtr.Zero)
		{
			Mission.Current.BatchFormationUnitPositions(orderedPositionIndices, orderedLocalPositions, availabilityTable, globalPositionTable, _orderPosition, Direction, fileCount, rankCount);
			return true;
		}
		return false;
	}

	public WorldPosition CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache worldPositionEnforcedCache)
	{
		if (!OrderPositionIsValid)
		{
			TaleWorlds.Library.Debug.Print("Formation order position is not valid. Team: " + Team.TeamIndex + ", Formation: " + (int)FormationIndex, 0, TaleWorlds.Library.Debug.DebugColor.Yellow);
		}
		switch (worldPositionEnforcedCache)
		{
		case WorldPosition.WorldPositionEnforcedCache.NavMeshVec3:
			_orderPosition.GetNavMeshVec3();
			break;
		case WorldPosition.WorldPositionEnforcedCache.GroundVec3:
			_orderPosition.GetGroundVec3();
			break;
		}
		return _orderPosition;
	}

	public void SetMovementOrder(MovementOrder input)
	{
		this.OnBeforeMovementOrderApplied?.Invoke(this, input.OrderEnum);
		if (input.OrderEnum == MovementOrder.MovementOrderEnum.Invalid)
		{
			input = MovementOrder.MovementOrderStop;
		}
		bool num = !_movementOrder.AreOrdersPracticallySame(_movementOrder, input, IsAIControlled);
		if (num)
		{
			_movementOrder.OnCancel(this);
		}
		if (num)
		{
			if (MovementOrder.GetMovementOrderDefensivenessChange(_movementOrder.OrderEnum, input.OrderEnum) != 0)
			{
				if (MovementOrder.GetMovementOrderDefensiveness(input.OrderEnum) == 0)
				{
					_formationOrderDefensivenessFactor = 0;
				}
				else
				{
					_formationOrderDefensivenessFactor = MovementOrder.GetMovementOrderDefensiveness(input.OrderEnum) + ArrangementOrder.GetArrangementOrderDefensiveness(_arrangementOrder.OrderEnum);
				}
				UpdateAgentDrivenPropertiesBasedOnOrderDefensiveness();
			}
			_movementOrder = input;
			_movementOrder.OnApply(this);
		}
		SetTargetFormation(null);
	}

	public void SetControlledByAI(bool isControlledByAI, bool enforceNotSplittableByAI = false)
	{
		if (_isAIControlled == isControlledByAI)
		{
			return;
		}
		_isAIControlled = isControlledByAI;
		if (_isAIControlled)
		{
			if (AI.ActiveBehavior != null && CountOfUnits > 0)
			{
				bool forceTickOccasionally = Mission.Current.ForceTickOccasionally;
				Mission.Current.ForceTickOccasionally = true;
				BehaviorComponent activeBehavior = AI.ActiveBehavior;
				AI.Tick();
				Mission.Current.ForceTickOccasionally = forceTickOccasionally;
				if (activeBehavior == AI.ActiveBehavior)
				{
					AI.ActiveBehavior.OnBehaviorActivated();
				}
				SetMovementOrder(AI.ActiveBehavior.CurrentOrder);
			}
			_enforceNotSplittableByAI = enforceNotSplittableByAI;
		}
		else
		{
			_enforceNotSplittableByAI = false;
		}
	}

	public void SetTargetFormation(Formation targetFormation)
	{
		TargetFormation = targetFormation;
	}

	public void OnDeploymentFinished()
	{
		AI?.OnDeploymentFinished();
	}

	public void ResetArrangementOrderTickTimer()
	{
		_arrangementOrderTickOccasionallyTimer = new Timer(Mission.Current.CurrentTime, 0.5f);
	}

	public void SetPositioning(WorldPosition? position = null, Vec2? direction = null, int? unitSpacing = null)
	{
		Vec2 orderPosition = OrderPosition;
		Vec2 direction2 = Direction;
		WorldPosition? newPosition = null;
		bool flag = false;
		bool flag2 = false;
		if (position.HasValue && position.Value.IsValid)
		{
			if (!HasBeenPositioned && !IsSimulationFormation)
			{
				HasBeenPositioned = true;
			}
			if (position.Value.AsVec2 != OrderPosition)
			{
				if (!Mission.Current.IsPositionInsideBoundaries(position.Value.AsVec2))
				{
					Vec2 closestBoundaryPosition = Mission.Current.GetClosestBoundaryPosition(position.Value.AsVec2);
					if (OrderPosition != closestBoundaryPosition)
					{
						WorldPosition value = position.Value;
						value.SetVec2(closestBoundaryPosition);
						newPosition = value;
					}
				}
				else
				{
					newPosition = position;
				}
			}
		}
		if (direction.HasValue && Direction != direction.Value)
		{
			flag = true;
		}
		if (unitSpacing.HasValue && UnitSpacing != unitSpacing.Value)
		{
			flag2 = true;
		}
		if (newPosition.HasValue || flag || flag2)
		{
			Arrangement.BeforeFormationFrameChange();
			if (newPosition.HasValue)
			{
				_orderPosition = newPosition.Value;
			}
			if (flag)
			{
				_direction = direction.Value;
			}
			if (flag2)
			{
				_unitSpacing = unitSpacing.Value;
				this.OnUnitSpacingChanged?.Invoke(this);
				Arrangement_OnShapeChanged();
				Arrangement.AreLocalPositionsDirty = true;
			}
			if (!IsSimulationFormation && Arrangement.IsTurnBackwardsNecessary(orderPosition, newPosition, direction2, flag, direction))
			{
				Arrangement.TurnBackwards();
			}
			Arrangement.OnFormationFrameChanged();
			if (newPosition.HasValue)
			{
				ArrangementOrder.OnOrderPositionChanged(this, orderPosition);
			}
		}
	}

	public int GetCountOfUnitsWithCondition(Func<Agent, bool> function)
	{
		int num = 0;
		foreach (IFormationUnit allUnit in Arrangement.GetAllUnits())
		{
			if (function((Agent)allUnit))
			{
				num++;
			}
		}
		foreach (Agent detachedUnit in _detachedUnits)
		{
			if (function(detachedUnit))
			{
				num++;
			}
		}
		return num;
	}

	public ref readonly MovementOrder GetReadonlyMovementOrderReference()
	{
		return ref _movementOrder;
	}

	public Agent GetFirstUnit()
	{
		return GetUnitWithIndex(0);
	}

	public int GetCountOfUnitsBelongingToLogicalClass(FormationClass logicalClass)
	{
		return _logicalClassCounts[(int)logicalClass];
	}

	public int GetCountOfUnitsBelongingToPhysicalClass(FormationClass physicalClass, bool excludeBannerBearers)
	{
		int num = 0;
		foreach (IFormationUnit allUnit in Arrangement.GetAllUnits())
		{
			bool flag = false;
			switch (physicalClass)
			{
			case FormationClass.Infantry:
				flag = (excludeBannerBearers ? QueryLibrary.IsInfantryWithoutBanner((Agent)allUnit) : QueryLibrary.IsInfantry((Agent)allUnit));
				break;
			case FormationClass.Ranged:
				flag = (excludeBannerBearers ? QueryLibrary.IsRangedWithoutBanner((Agent)allUnit) : QueryLibrary.IsRanged((Agent)allUnit));
				break;
			case FormationClass.Cavalry:
				flag = (excludeBannerBearers ? QueryLibrary.IsCavalryWithoutBanner((Agent)allUnit) : QueryLibrary.IsCavalry((Agent)allUnit));
				break;
			case FormationClass.HorseArcher:
				flag = (excludeBannerBearers ? QueryLibrary.IsRangedCavalryWithoutBanner((Agent)allUnit) : QueryLibrary.IsRangedCavalry((Agent)allUnit));
				break;
			}
			if (flag)
			{
				num++;
			}
		}
		foreach (Agent detachedUnit in _detachedUnits)
		{
			bool flag2 = false;
			switch (physicalClass)
			{
			case FormationClass.Infantry:
				flag2 = (excludeBannerBearers ? QueryLibrary.IsInfantryWithoutBanner(detachedUnit) : QueryLibrary.IsInfantry(detachedUnit));
				break;
			case FormationClass.Ranged:
				flag2 = (excludeBannerBearers ? QueryLibrary.IsRangedWithoutBanner(detachedUnit) : QueryLibrary.IsRanged(detachedUnit));
				break;
			case FormationClass.Cavalry:
				flag2 = (excludeBannerBearers ? QueryLibrary.IsCavalryWithoutBanner(detachedUnit) : QueryLibrary.IsCavalry(detachedUnit));
				break;
			case FormationClass.HorseArcher:
				flag2 = (excludeBannerBearers ? QueryLibrary.IsRangedCavalryWithoutBanner(detachedUnit) : QueryLibrary.IsRangedCavalry(detachedUnit));
				break;
			}
			if (flag2)
			{
				num++;
			}
		}
		return num;
	}

	public void SetSpawnIndex(int value = 0)
	{
		_currentSpawnIndex = value;
	}

	public int GetNextSpawnIndex()
	{
		int currentSpawnIndex = _currentSpawnIndex;
		_currentSpawnIndex++;
		return currentSpawnIndex;
	}

	public Agent GetUnitWithIndex(int unitIndex)
	{
		if (Arrangement.GetAllUnits().Count > unitIndex)
		{
			return (Agent)Arrangement.GetAllUnits()[unitIndex];
		}
		unitIndex -= Arrangement.GetAllUnits().Count;
		if (_detachedUnits.Count > unitIndex)
		{
			return _detachedUnits[unitIndex];
		}
		return null;
	}

	public Vec2 GetAveragePositionOfUnits(bool excludeDetachedUnits, bool excludePlayer)
	{
		int num = (excludeDetachedUnits ? CountOfUnitsWithoutDetachedOnes : CountOfUnits);
		if (num > 0)
		{
			Vec2 zero = Vec2.Zero;
			foreach (Agent allUnit in Arrangement.GetAllUnits())
			{
				if (!excludePlayer || !allUnit.IsMainAgent)
				{
					zero += allUnit.Position.AsVec2;
				}
				else
				{
					num--;
				}
			}
			if (excludeDetachedUnits)
			{
				for (int i = 0; i < _looseDetachedUnits.Count; i++)
				{
					zero += _looseDetachedUnits[i].Position.AsVec2;
				}
			}
			else
			{
				for (int j = 0; j < _detachedUnits.Count; j++)
				{
					zero += _detachedUnits[j].Position.AsVec2;
				}
			}
			if (num > 0)
			{
				return zero * (1f / (float)num);
			}
		}
		return Vec2.Invalid;
	}

	public Agent GetMedianAgent(bool excludeDetachedUnits, bool excludePlayer, Vec2 averagePosition)
	{
		excludeDetachedUnits = excludeDetachedUnits && CountOfUnitsWithoutDetachedOnes > 0;
		excludePlayer = excludePlayer && (CountOfUndetachableNonPlayerUnits > 0 || CountOfDetachableNonplayerUnits > 0);
		float num = float.MaxValue;
		Agent result = null;
		foreach (Agent allUnit in Arrangement.GetAllUnits())
		{
			if (!excludePlayer || !allUnit.IsMainAgent)
			{
				float num2 = allUnit.Position.AsVec2.DistanceSquared(averagePosition);
				if (num2 <= num)
				{
					result = allUnit;
					num = num2;
				}
			}
		}
		if (excludeDetachedUnits)
		{
			for (int i = 0; i < _looseDetachedUnits.Count; i++)
			{
				float num3 = _looseDetachedUnits[i].Position.AsVec2.DistanceSquared(averagePosition);
				if (num3 <= num)
				{
					result = _looseDetachedUnits[i];
					num = num3;
				}
			}
		}
		else
		{
			for (int j = 0; j < _detachedUnits.Count; j++)
			{
				float num4 = _detachedUnits[j].Position.AsVec2.DistanceSquared(averagePosition);
				if (num4 <= num)
				{
					result = _detachedUnits[j];
					num = num4;
				}
			}
		}
		return result;
	}

	public Agent.UnderAttackType GetUnderAttackTypeOfUnits(float timeLimit = 3f)
	{
		float num = float.MinValue;
		float num2 = float.MinValue;
		timeLimit += MBCommon.GetTotalMissionTime();
		foreach (IFormationUnit allUnit in Arrangement.GetAllUnits())
		{
			num = TaleWorlds.Library.MathF.Max(num, ((Agent)allUnit).LastMeleeHitTime);
			num2 = TaleWorlds.Library.MathF.Max(num2, ((Agent)allUnit).LastRangedHitTime);
			if (num2 >= 0f && num2 < timeLimit)
			{
				return Agent.UnderAttackType.UnderRangedAttack;
			}
			if (num >= 0f && num < timeLimit)
			{
				return Agent.UnderAttackType.UnderMeleeAttack;
			}
		}
		for (int i = 0; i < _detachedUnits.Count; i++)
		{
			num = TaleWorlds.Library.MathF.Max(num, _detachedUnits[i].LastMeleeHitTime);
			num2 = TaleWorlds.Library.MathF.Max(num2, _detachedUnits[i].LastRangedHitTime);
			if (num2 >= 0f && num2 < timeLimit)
			{
				return Agent.UnderAttackType.UnderRangedAttack;
			}
			if (num >= 0f && num < timeLimit)
			{
				return Agent.UnderAttackType.UnderMeleeAttack;
			}
		}
		return Agent.UnderAttackType.NotUnderAttack;
	}

	public Agent.MovementBehaviorType GetMovementTypeOfUnits()
	{
		float curMissionTime = MBCommon.GetTotalMissionTime();
		int retreatingCount = 0;
		int attackingCount = 0;
		ApplyActionOnEachUnit(delegate(Agent agent)
		{
			if (agent.IsAIControlled && (agent.IsRetreating() || (agent.Formation != null && agent.Formation._movementOrder.OrderType == OrderType.Retreat)))
			{
				retreatingCount++;
			}
			if (curMissionTime - agent.LastMeleeAttackTime < 3f)
			{
				attackingCount++;
			}
		});
		if (CountOfUnits > 0 && (float)retreatingCount / (float)CountOfUnits > 0.3f)
		{
			return Agent.MovementBehaviorType.Flee;
		}
		if (attackingCount > 0)
		{
			return Agent.MovementBehaviorType.Engaged;
		}
		return Agent.MovementBehaviorType.Idle;
	}

	public IEnumerable<Agent> GetUnitsWithoutDetachedOnes()
	{
		foreach (IFormationUnit allUnit in Arrangement.GetAllUnits())
		{
			yield return allUnit as Agent;
		}
		for (int i = 0; i < _looseDetachedUnits.Count; i++)
		{
			yield return _looseDetachedUnits[i];
		}
	}

	public Vec2 GetWallDirectionOfRelativeFormationLocation(Agent unit)
	{
		if (unit.IsDetachedFromFormation)
		{
			return Vec2.Invalid;
		}
		Vec2? localWallDirectionOfRelativeFormationLocation = Arrangement.GetLocalWallDirectionOfRelativeFormationLocation(unit);
		if (localWallDirectionOfRelativeFormationLocation.HasValue)
		{
			return Direction.TransformToParentUnitF(localWallDirectionOfRelativeFormationLocation.Value);
		}
		return Vec2.Invalid;
	}

	public Vec2 GetDirectionOfUnit(Agent unit)
	{
		if (unit.IsDetachedFromFormation)
		{
			return unit.GetMovementDirection();
		}
		Vec2? localDirectionOfUnitOrDefault = Arrangement.GetLocalDirectionOfUnitOrDefault(unit);
		if (localDirectionOfUnitOrDefault.HasValue)
		{
			return Direction.TransformToParentUnitF(localDirectionOfUnitOrDefault.Value);
		}
		return unit.GetMovementDirection();
	}

	private WorldPosition GetOrderPositionOfUnitAux(Agent unit)
	{
		WorldPosition? worldPositionOfUnitOrDefault = Arrangement.GetWorldPositionOfUnitOrDefault(unit);
		if (worldPositionOfUnitOrDefault.HasValue)
		{
			return worldPositionOfUnitOrDefault.Value;
		}
		if (!OrderPositionIsValid)
		{
			WorldPosition worldPosition = unit.GetWorldPosition();
			TaleWorlds.Library.Debug.Print(string.Concat("Formation order position is not valid. Team: ", Team.TeamIndex, ", Formation: ", (int)FormationIndex, "Unit Pos: ", worldPosition.GetGroundVec3(), "Mission Mode: ", Mission.Current.Mode.ToString()), 0, TaleWorlds.Library.Debug.DebugColor.Yellow);
		}
		WorldPosition result = _movementOrder.CreateNewOrderWorldPosition(this, WorldPosition.WorldPositionEnforcedCache.NavMeshVec3);
		if (result.GetNavMesh() == UIntPtr.Zero || !Mission.Current.IsPositionInsideBoundaries(result.AsVec2))
		{
			return unit.GetWorldPosition();
		}
		return result;
	}

	public WorldPosition GetOrderPositionOfUnit(Agent unit)
	{
		if (unit.IsDetachedFromFormation && (_movementOrder.MovementState != 0 || !unit.Detachment.IsLoose || unit.Mission.Mode == MissionMode.Deployment))
		{
			WorldFrame? detachmentFrame = GetDetachmentFrame(unit);
			if (detachmentFrame.HasValue)
			{
				return detachmentFrame.Value.Origin;
			}
			return WorldPosition.Invalid;
		}
		switch (_movementOrder.MovementState)
		{
		case MovementOrder.MovementStateEnum.Charge:
			if (unit.Mission.Mode == MissionMode.Deployment)
			{
				return GetOrderPositionOfUnitAux(unit);
			}
			if (!OrderPositionIsValid)
			{
				WorldPosition worldPosition = unit.GetWorldPosition();
				TaleWorlds.Library.Debug.Print("Formation order position is not valid. Team: " + Team.TeamIndex + ", Formation: " + (int)FormationIndex + "Unit Pos: " + worldPosition.GetGroundVec3(), 0, TaleWorlds.Library.Debug.DebugColor.Yellow);
			}
			return _movementOrder.CreateNewOrderWorldPosition(this, WorldPosition.WorldPositionEnforcedCache.None);
		case MovementOrder.MovementStateEnum.Hold:
			return GetOrderPositionOfUnitAux(unit);
		case MovementOrder.MovementStateEnum.Retreat:
			return WorldPosition.Invalid;
		case MovementOrder.MovementStateEnum.StandGround:
			return unit.GetWorldPosition();
		default:
			TaleWorlds.Library.Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Formation.cs", "GetOrderPositionOfUnit", 1567);
			return WorldPosition.Invalid;
		}
	}

	public Vec2 GetCurrentGlobalPositionOfUnit(Agent unit, bool blendWithOrderDirection)
	{
		if (unit.IsDetachedFromFormation)
		{
			return unit.Position.AsVec2;
		}
		Vec2? localPositionOfUnitOrDefaultWithAdjustment = Arrangement.GetLocalPositionOfUnitOrDefaultWithAdjustment(unit, blendWithOrderDirection ? ((QuerySystem.EstimatedInterval - Interval) * 0.9f) : 0f);
		if (localPositionOfUnitOrDefaultWithAdjustment.HasValue)
		{
			return (blendWithOrderDirection ? CurrentDirection : QuerySystem.EstimatedDirection).TransformToParentUnitF(localPositionOfUnitOrDefaultWithAdjustment.Value) + CurrentPosition;
		}
		return unit.Position.AsVec2;
	}

	public float GetAverageMaximumMovementSpeedOfUnits()
	{
		if (CountOfUnitsWithoutDetachedOnes == 0)
		{
			return 0.1f;
		}
		float num = 0f;
		foreach (Agent unitsWithoutDetachedOne in GetUnitsWithoutDetachedOnes())
		{
			num += unitsWithoutDetachedOne.RunSpeedCached;
		}
		return num / (float)CountOfUnitsWithoutDetachedOnes;
	}

	public float GetMovementSpeedOfUnits()
	{
		ArrangementOrder.GetMovementSpeedRestriction(out var runRestriction, out var walkRestriction);
		if (!runRestriction.HasValue && !walkRestriction.HasValue)
		{
			runRestriction = 1f;
		}
		if (walkRestriction.HasValue)
		{
			if (CountOfUnits == 0)
			{
				return 0.1f;
			}
			IEnumerable<Agent> source;
			if (CountOfUnitsWithoutDetachedOnes != 0)
			{
				source = GetUnitsWithoutDetachedOnes();
			}
			else
			{
				IEnumerable<Agent> detachedUnits = _detachedUnits;
				source = detachedUnits;
			}
			return source.Min((Agent u) => u.WalkSpeedCached) * walkRestriction.Value;
		}
		if (CountOfUnits == 0)
		{
			return 0.1f;
		}
		IEnumerable<Agent> source2;
		if (CountOfUnitsWithoutDetachedOnes != 0)
		{
			source2 = GetUnitsWithoutDetachedOnes();
		}
		else
		{
			IEnumerable<Agent> detachedUnits = _detachedUnits;
			source2 = detachedUnits;
		}
		float num = source2.Average((Agent u) => u.RunSpeedCached);
		FormationQuerySystem.FormationIntegrityDataGroup formationIntegrityData = QuerySystem.FormationIntegrityData;
		if (formationIntegrityData.DeviationOfPositionsExcludeFarAgents < formationIntegrityData.AverageMaxUnlimitedSpeedExcludeFarAgents * 0.25f)
		{
			return num * runRestriction.Value;
		}
		return num;
	}

	public float GetFormationPower()
	{
		float sum = 0f;
		ApplyActionOnEachUnit(delegate(Agent agent)
		{
			sum += agent.CharacterPowerCached;
		});
		return sum;
	}

	public float GetFormationMeleeFightingPower()
	{
		float sum = 0f;
		ApplyActionOnEachUnit(delegate(Agent agent)
		{
			sum += agent.CharacterPowerCached * ((FormationIndex == FormationClass.Ranged || FormationIndex == FormationClass.HorseArcher) ? 0.4f : 1f);
		});
		return sum;
	}

	internal IDetachment GetDetachmentForDebug(Agent agent)
	{
		return Detachments.FirstOrDefault((IDetachment d) => d.IsAgentUsingOrInterested(agent));
	}

	public IDetachment GetDetachmentOrDefault(Agent agent)
	{
		return agent.Detachment;
	}

	public WorldFrame? GetDetachmentFrame(Agent agent)
	{
		return agent.Detachment.GetAgentFrame(agent);
	}

	public Vec2 GetMiddleFrontUnitPositionOffset()
	{
		Vec2 localPositionOfReservedUnitPosition = Arrangement.GetLocalPositionOfReservedUnitPosition();
		return Direction.TransformToParentUnitF(localPositionOfReservedUnitPosition);
	}

	public List<IFormationUnit> GetUnitsToPopWithReferencePosition(int count, Vec3 targetPosition)
	{
		int num = TaleWorlds.Library.MathF.Min(count, Arrangement.UnitCount);
		List<IFormationUnit> list = ((num == 0) ? new List<IFormationUnit>() : Arrangement.GetUnitsToPop(num, targetPosition));
		int num2 = count - list.Count;
		if (num2 > 0)
		{
			List<Agent> list2 = _looseDetachedUnits.Take(num2).ToList();
			num2 -= list2.Count;
			list.AddRange(list2);
		}
		if (num2 > 0)
		{
			IEnumerable<Agent> enumerable = _detachedUnits.Take(num2);
			num2 -= enumerable.Count();
			list.AddRange(enumerable);
		}
		return list;
	}

	public List<IFormationUnit> GetUnitsToPop(int count)
	{
		int num = TaleWorlds.Library.MathF.Min(count, Arrangement.UnitCount);
		List<IFormationUnit> list = ((num == 0) ? new List<IFormationUnit>() : Arrangement.GetUnitsToPop(num));
		int num2 = count - list.Count;
		if (num2 > 0)
		{
			List<Agent> list2 = _looseDetachedUnits.Take(num2).ToList();
			num2 -= list2.Count;
			list.AddRange(list2);
		}
		if (num2 > 0)
		{
			IEnumerable<Agent> enumerable = _detachedUnits.Take(num2);
			num2 -= enumerable.Count();
			list.AddRange(enumerable);
		}
		return list;
	}

	public IEnumerable<(WorldPosition, Vec2)> GetUnavailableUnitPositionsAccordingToNewOrder(Formation simulationFormation, in WorldPosition position, in Vec2 direction, float width, int unitSpacing)
	{
		return GetUnavailableUnitPositionsAccordingToNewOrder(this, simulationFormation, position, direction, Arrangement, width, unitSpacing);
	}

	public void GetUnitSpawnFrameWithIndex(int unitIndex, in WorldPosition formationPosition, in Vec2 formationDirection, float width, int unitCount, int unitSpacing, bool isMountedFormation, out WorldPosition? unitSpawnPosition, out Vec2? unitSpawnDirection)
	{
		GetUnitPositionWithIndexAccordingToNewOrder(null, unitIndex, in formationPosition, in formationDirection, Arrangement, width, unitSpacing, unitCount, isMountedFormation, Index, out unitSpawnPosition, out unitSpawnDirection, out var _);
	}

	public void GetUnitPositionWithIndexAccordingToNewOrder(Formation simulationFormation, int unitIndex, in WorldPosition formationPosition, in Vec2 formationDirection, float width, int unitSpacing, out WorldPosition? unitSpawnPosition, out Vec2? unitSpawnDirection)
	{
		GetUnitPositionWithIndexAccordingToNewOrder(simulationFormation, unitIndex, in formationPosition, in formationDirection, Arrangement, width, unitSpacing, Arrangement.UnitCount, HasAnyMountedUnit, Index, out unitSpawnPosition, out unitSpawnDirection, out var _);
	}

	public void GetUnitPositionWithIndexAccordingToNewOrder(Formation simulationFormation, int unitIndex, in WorldPosition formationPosition, in Vec2 formationDirection, float width, int unitSpacing, int overridenUnitCount, out WorldPosition? unitPosition, out Vec2? unitDirection)
	{
		GetUnitPositionWithIndexAccordingToNewOrder(simulationFormation, unitIndex, in formationPosition, in formationDirection, Arrangement, width, unitSpacing, overridenUnitCount, HasAnyMountedUnit, Index, out unitPosition, out unitDirection, out var _);
	}

	public void GetUnitPositionWithIndexAccordingToNewOrder(Formation simulationFormation, int unitIndex, in WorldPosition formationPosition, in Vec2 formationDirection, float width, int unitSpacing, out WorldPosition? unitSpawnPosition, out Vec2? unitSpawnDirection, out float actualWidth)
	{
		GetUnitPositionWithIndexAccordingToNewOrder(simulationFormation, unitIndex, in formationPosition, in formationDirection, Arrangement, width, unitSpacing, Arrangement.UnitCount, HasAnyMountedUnit, Index, out unitSpawnPosition, out unitSpawnDirection, out actualWidth);
	}

	public bool HasUnitsWithCondition(Func<Agent, bool> function)
	{
		foreach (IFormationUnit allUnit in Arrangement.GetAllUnits())
		{
			if (function((Agent)allUnit))
			{
				return true;
			}
		}
		for (int i = 0; i < _detachedUnits.Count; i++)
		{
			if (function(_detachedUnits[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasUnitsWithCondition(Func<Agent, bool> function, out Agent result)
	{
		foreach (IFormationUnit allUnit in Arrangement.GetAllUnits())
		{
			if (function((Agent)allUnit))
			{
				result = (Agent)allUnit;
				return true;
			}
		}
		for (int i = 0; i < _detachedUnits.Count; i++)
		{
			if (function(_detachedUnits[i]))
			{
				result = _detachedUnits[i];
				return true;
			}
		}
		result = null;
		return false;
	}

	public bool HasAnyEnemyFormationsThatIsNotEmpty()
	{
		foreach (Team team in Mission.Current.Teams)
		{
			if (!team.IsEnemyOf(Team))
			{
				continue;
			}
			foreach (Formation item in team.FormationsIncludingSpecialAndEmpty)
			{
				if (item.CountOfUnits > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasUnitWithConditionLimitedRandom(Func<Agent, bool> function, int startingIndex, int willBeCheckedUnitCount, out Agent resultAgent)
	{
		int unitCount = Arrangement.UnitCount;
		int count = _detachedUnits.Count;
		if (unitCount + count <= willBeCheckedUnitCount)
		{
			return HasUnitsWithCondition(function, out resultAgent);
		}
		for (int i = 0; i < willBeCheckedUnitCount; i++)
		{
			if (startingIndex < unitCount)
			{
				int index = MBRandom.RandomInt(unitCount);
				if (function((Agent)Arrangement.GetAllUnits()[index]))
				{
					resultAgent = (Agent)Arrangement.GetAllUnits()[index];
					return true;
				}
			}
			else if (count > 0)
			{
				int index = MBRandom.RandomInt(count);
				if (function(_detachedUnits[index]))
				{
					resultAgent = _detachedUnits[index];
					return true;
				}
			}
		}
		resultAgent = null;
		return false;
	}

	public int[] CollectUnitIndices()
	{
		if (_agentIndicesCache == null || _agentIndicesCache.Length != CountOfUnits)
		{
			_agentIndicesCache = new int[CountOfUnits];
		}
		int num = 0;
		foreach (IFormationUnit allUnit in Arrangement.GetAllUnits())
		{
			_agentIndicesCache[num] = ((Agent)allUnit).Index;
			num++;
		}
		for (int i = 0; i < _detachedUnits.Count; i++)
		{
			_agentIndicesCache[num] = _detachedUnits[i].Index;
			num++;
		}
		return _agentIndicesCache;
	}

	public void ApplyActionOnEachUnit(Action<Agent> action, Agent ignoreAgent = null)
	{
		if (ignoreAgent == null)
		{
			foreach (Agent allUnit in Arrangement.GetAllUnits())
			{
				action(allUnit);
			}
			for (int i = 0; i < _detachedUnits.Count; i++)
			{
				action(_detachedUnits[i]);
			}
			return;
		}
		foreach (Agent allUnit2 in Arrangement.GetAllUnits())
		{
			if (allUnit2 != ignoreAgent)
			{
				action(allUnit2);
			}
		}
		for (int j = 0; j < _detachedUnits.Count; j++)
		{
			Agent agent2 = _detachedUnits[j];
			if (agent2 != ignoreAgent)
			{
				action(agent2);
			}
		}
	}

	public void ApplyActionOnEachAttachedUnit(Action<Agent> action)
	{
		foreach (Agent allUnit in Arrangement.GetAllUnits())
		{
			action(allUnit);
		}
	}

	public void ApplyActionOnEachDetachedUnit(Action<Agent> action)
	{
		for (int i = 0; i < _detachedUnits.Count; i++)
		{
			action(_detachedUnits[i]);
		}
	}

	public void ApplyActionOnEachUnitViaBackupList(Action<Agent> action)
	{
		if (Arrangement.GetAllUnits().Count > 0)
		{
			IFormationUnit[] array = Arrangement.GetAllUnits().ToArray();
			foreach (IFormationUnit formationUnit in array)
			{
				action((Agent)formationUnit);
			}
		}
		if (_detachedUnits.Count > 0)
		{
			Agent[] array2 = _detachedUnits.ToArray();
			foreach (Agent obj in array2)
			{
				action(obj);
			}
		}
	}

	public void ApplyActionOnEachUnit(Action<Agent, List<WorldPosition>> action, List<WorldPosition> list)
	{
		foreach (IFormationUnit allUnit in Arrangement.GetAllUnits())
		{
			action((Agent)allUnit, list);
		}
		for (int i = 0; i < _detachedUnits.Count; i++)
		{
			action(_detachedUnits[i], list);
		}
	}

	public int CountUnitsOnNavMeshIDMod10(int navMeshID, bool includeOnlyPositionedUnits)
	{
		int num = 0;
		foreach (IFormationUnit allUnit in Arrangement.GetAllUnits())
		{
			if (((Agent)allUnit).GetCurrentNavigationFaceId() % 10 == navMeshID && (!includeOnlyPositionedUnits || Arrangement.GetUnpositionedUnits() == null || Arrangement.GetUnpositionedUnits().IndexOf(allUnit) < 0))
			{
				num++;
			}
		}
		if (!includeOnlyPositionedUnits)
		{
			foreach (Agent detachedUnit in _detachedUnits)
			{
				if (detachedUnit.GetCurrentNavigationFaceId() % 10 == navMeshID)
				{
					num++;
				}
			}
		}
		return num;
	}

	public void OnAgentControllerChanged(Agent agent, Agent.ControllerType oldController)
	{
		Agent.ControllerType controller = agent.Controller;
		if (oldController != Agent.ControllerType.Player && controller == Agent.ControllerType.Player)
		{
			HasPlayerControlledTroop = true;
			if (!GameNetwork.IsMultiplayer)
			{
				TryRelocatePlayerUnit();
			}
			if (!agent.IsDetachableFromFormation)
			{
				OnUndetachableNonPlayerUnitRemoved(agent);
			}
		}
		else if (oldController == Agent.ControllerType.Player && controller != Agent.ControllerType.Player)
		{
			HasPlayerControlledTroop = false;
			if (!agent.IsDetachableFromFormation)
			{
				OnUndetachableNonPlayerUnitAdded(agent);
			}
		}
	}

	public void OnMassUnitTransferStart()
	{
		PostponeCostlyOperations = true;
	}

	public void OnMassUnitTransferEnd()
	{
		ReapplyFormOrder();
		QuerySystem.Expire();
		Team.QuerySystem.ExpireAfterUnitAddRemove();
		if (_logicalClassNeedsUpdate)
		{
			CalculateLogicalClass();
		}
		if (CountOfUnits == 0)
		{
			_representativeClass = FormationClass.NumberOfAllFormations;
		}
		if (Mission.Current.IsTeleportingAgents)
		{
			SetPositioning(_orderPosition);
			ApplyActionOnEachUnit(delegate(Agent agent)
			{
				agent.UpdateCachedAndFormationValues(updateOnlyMovement: true, arrangementChangeAllowed: false);
			});
		}
		PostponeCostlyOperations = false;
	}

	public void OnBatchUnitRemovalStart()
	{
		PostponeCostlyOperations = true;
		Arrangement.OnBatchRemoveStart();
	}

	public void OnBatchUnitRemovalEnd()
	{
		Arrangement.OnBatchRemoveEnd();
		ReapplyFormOrder();
		QuerySystem.ExpireAfterUnitAddRemove();
		Team.QuerySystem.ExpireAfterUnitAddRemove();
		if (_logicalClassNeedsUpdate)
		{
			CalculateLogicalClass();
		}
		PostponeCostlyOperations = false;
	}

	public void OnUnitAddedOrRemoved()
	{
		if (!PostponeCostlyOperations)
		{
			ReapplyFormOrder();
			QuerySystem.ExpireAfterUnitAddRemove();
			Team?.QuerySystem.ExpireAfterUnitAddRemove();
		}
		this.OnUnitCountChanged?.Invoke(this);
	}

	public void OnAgentLostMount(Agent agent)
	{
		if (!agent.IsDetachedFromFormation)
		{
			_arrangement.OnUnitLostMount(agent);
		}
	}

	public void OnFormationDispersed()
	{
		Arrangement.OnFormationDispersed();
		ApplyActionOnEachUnit(delegate(Agent agent)
		{
			agent.UpdateCachedAndFormationValues(updateOnlyMovement: true, arrangementChangeAllowed: false);
		});
	}

	public void OnUnitDetachmentChanged(Agent unit, bool isOldDetachmentLoose, bool isNewDetachmentLoose)
	{
		if (isOldDetachmentLoose && !isNewDetachmentLoose)
		{
			_looseDetachedUnits.Remove(unit);
		}
		else if (!isOldDetachmentLoose && isNewDetachmentLoose)
		{
			_looseDetachedUnits.Add(unit);
		}
	}

	public void OnUndetachableNonPlayerUnitAdded(Agent unit)
	{
		if (unit.Formation == this && !unit.IsPlayerControlled)
		{
			_undetachableNonPlayerUnitCount++;
		}
	}

	public void OnUndetachableNonPlayerUnitRemoved(Agent unit)
	{
		if (unit.Formation == this && !unit.IsPlayerControlled)
		{
			_undetachableNonPlayerUnitCount--;
		}
	}

	public void ResetMovementOrderPositionCache()
	{
		_movementOrder.ResetPositionCache();
	}

	public void Reset()
	{
		Arrangement = new LineFormation(this);
		_arrangementOrderTickOccasionallyTimer = new Timer(Mission.Current.CurrentTime, 0.5f);
		ResetAux();
		FacingOrder = FacingOrder.FacingOrderLookAtEnemy;
		_enforceNotSplittableByAI = false;
		ContainsAgentVisuals = false;
		PlayerOwner = null;
	}

	public IEnumerable<Formation> Split(int count = 2)
	{
		foreach (Formation item in Team.FormationsIncludingEmpty)
		{
			item.PostponeCostlyOperations = true;
		}
		IEnumerable<Formation> enumerable = Team.MasterOrderController.SplitFormation(this, count);
		if (enumerable.Count() > 1 && Team != null)
		{
			foreach (Formation item2 in enumerable)
			{
				item2.QuerySystem.Expire();
			}
		}
		foreach (Formation item3 in Team.FormationsIncludingEmpty)
		{
			item3.CalculateLogicalClass();
			item3.PostponeCostlyOperations = false;
		}
		if (CountOfUnits == 0)
		{
			_representativeClass = FormationClass.NumberOfAllFormations;
		}
		return enumerable;
	}

	public void TransferUnits(Formation target, int unitCount)
	{
		PostponeCostlyOperations = true;
		target.PostponeCostlyOperations = true;
		Team.MasterOrderController.TransferUnits(this, target, unitCount);
		CalculateLogicalClass();
		target.CalculateLogicalClass();
		if (CountOfUnits == 0)
		{
			_representativeClass = FormationClass.NumberOfAllFormations;
		}
		PostponeCostlyOperations = false;
		target.PostponeCostlyOperations = false;
		QuerySystem.Expire();
		target.QuerySystem.Expire();
		Team.QuerySystem.ExpireAfterUnitAddRemove();
		target.Team.QuerySystem.ExpireAfterUnitAddRemove();
	}

	public void TransferUnitsAux(Formation target, int unitCount, bool isPlayerOrder, bool useSelectivePop)
	{
		if (!isPlayerOrder && !IsSplittableByAI)
		{
			return;
		}
		MBDebug.Print(FormationIndex.GetName() + " has " + CountOfUnits + " units, " + target.FormationIndex.GetName() + " has " + target.CountOfUnits + " units");
		MBDebug.Print(string.Concat(Team.Side, " ", FormationIndex.GetName(), " transfers ", unitCount, " units to ", target.FormationIndex.GetName()));
		if (unitCount == 0)
		{
			return;
		}
		if (target.CountOfUnits == 0)
		{
			target.CopyOrdersFrom(this);
			target.SetPositioning(_orderPosition, _direction, _unitSpacing);
		}
		BattleBannerBearersModel battleBannerBearersModel = MissionGameModels.Current.BattleBannerBearersModel;
		List<IFormationUnit> list;
		if (battleBannerBearersModel.GetFormationBanner(this) == null)
		{
			list = (useSelectivePop ? GetUnitsToPopWithReferencePosition(unitCount, target.OrderPositionIsValid ? target.OrderPosition.ToVec3() : target.QuerySystem.MedianPosition.GetGroundVec3()) : GetUnitsToPop(unitCount).ToList());
		}
		else
		{
			List<Agent> formationBannerBearers = battleBannerBearersModel.GetFormationBannerBearers(this);
			int count = Math.Min(CountOfUnits, unitCount + formationBannerBearers.Count);
			list = (useSelectivePop ? GetUnitsToPopWithReferencePosition(count, target.OrderPositionIsValid ? target.OrderPosition.ToVec3() : target.QuerySystem.MedianPosition.GetGroundVec3()) : GetUnitsToPop(count).ToList());
			foreach (Agent item in formationBannerBearers)
			{
				if (list.Count > unitCount)
				{
					list.Remove(item);
					continue;
				}
				break;
			}
			if (list.Count > unitCount)
			{
				int num = list.Count - unitCount;
				list.RemoveRange(list.Count - num, num);
			}
		}
		if (battleBannerBearersModel.GetFormationBanner(target) != null)
		{
			foreach (Agent formationBannerBearer in battleBannerBearersModel.GetFormationBannerBearers(target))
			{
				if (formationBannerBearer.Formation == this && !list.Contains(formationBannerBearer))
				{
					int num2 = list.FindIndex((IFormationUnit unit) => unit is Agent agent && agent.Banner == null);
					if (num2 < 0)
					{
						break;
					}
					list[num2] = formationBannerBearer;
				}
			}
		}
		foreach (Agent item2 in list)
		{
			item2.Formation = target;
		}
		Team.TriggerOnFormationsChanged(this);
		Team.TriggerOnFormationsChanged(target);
		MBDebug.Print(FormationIndex.GetName() + " has " + CountOfUnits + " units, " + target.FormationIndex.GetName() + " has " + target.CountOfUnits + " units");
	}

	[Conditional("DEBUG")]
	public void DebugArrangements()
	{
		foreach (Team team in Mission.Current.Teams)
		{
			foreach (Formation item in team.FormationsIncludingSpecialAndEmpty)
			{
				if (item.CountOfUnits > 0)
				{
					item.ApplyActionOnEachUnit(delegate(Agent agent)
					{
						agent.AgentVisuals.SetContourColor(null);
					});
				}
			}
		}
		ApplyActionOnEachUnit(delegate(Agent agent)
		{
			agent.AgentVisuals.SetContourColor(4294901760u);
		});
		Vec3 vec = Direction.ToVec3();
		vec.RotateAboutZ(System.MathF.PI / 2f);
		_ = IsSimulationFormation;
		_ = vec * Width * 0.5f;
		_ = Direction.ToVec3() * Depth * 0.5f;
		_ = OrderPositionIsValid;
		QuerySystem.MedianPosition.SetVec2(CurrentPosition);
		ApplyActionOnEachUnit(delegate(Agent agent)
		{
			WorldPosition orderPositionOfUnit = GetOrderPositionOfUnit(agent);
			if (orderPositionOfUnit.IsValid)
			{
				Vec2 directionOfUnit = GetDirectionOfUnit(agent);
				directionOfUnit.Normalize();
				directionOfUnit *= 0.1f;
				_ = orderPositionOfUnit.GetGroundVec3() + directionOfUnit.ToVec3();
				_ = orderPositionOfUnit.GetGroundVec3() - directionOfUnit.LeftVec().ToVec3();
				_ = orderPositionOfUnit.GetGroundVec3() + directionOfUnit.LeftVec().ToVec3();
				_ = "(" + ((IFormationUnit)agent).FormationFileIndex + "," + ((IFormationUnit)agent).FormationRankIndex + ")";
			}
		});
		_ = OrderPositionIsValid;
		foreach (IDetachment detachment in Detachments)
		{
			_ = detachment is UsableMachine;
			_ = detachment is RangedSiegeWeapon;
		}
		if (Arrangement is ColumnFormation)
		{
			ApplyActionOnEachUnit(delegate(Agent agent)
			{
				agent.GetFollowedUnit();
				_ = "(" + ((IFormationUnit)agent).FormationFileIndex + "," + ((IFormationUnit)agent).FormationRankIndex + ")";
			});
		}
	}

	public void AddUnit(Agent unit)
	{
		int countOfUnits = CountOfUnits;
		if (Arrangement.AddUnit(unit) && Mission.Current.HasMissionBehavior<AmmoSupplyLogic>() && Mission.Current.GetMissionBehavior<AmmoSupplyLogic>().IsAgentEligibleForAmmoSupply(unit))
		{
			unit.SetScriptedCombatFlags(unit.GetScriptedCombatFlags() | Agent.AISpecialCombatModeFlags.IgnoreAmmoLimitForRangeCalculation);
			unit.ResetAiWaitBeforeShootFactor();
			unit.UpdateAgentStats();
		}
		if (unit.IsPlayerControlled)
		{
			HasPlayerControlledTroop = true;
		}
		if (unit.IsPlayerTroop)
		{
			IsPlayerTroopInFormation = true;
		}
		if (!unit.IsDetachableFromFormation && !unit.IsPlayerControlled)
		{
			OnUndetachableNonPlayerUnitAdded(unit);
		}
		if (unit.Character != null)
		{
			FormationClass formationClass = Team.Mission.GetAgentTroopClass(Team.Side, unit.Character).DefaultClass();
			_logicalClassCounts[(int)formationClass]++;
			if (_logicalClass != formationClass)
			{
				if (PostponeCostlyOperations)
				{
					_logicalClassNeedsUpdate = true;
				}
				else
				{
					CalculateLogicalClass();
					_logicalClassNeedsUpdate = false;
				}
			}
		}
		_movementOrder.OnUnitJoinOrLeave(this, unit, isJoining: true);
		unit.SetTargetFormationIndex(TargetFormation?.Index ?? (-1));
		unit.SetFiringOrder(FiringOrder.OrderEnum);
		unit.SetRidingOrder(RidingOrder.OrderEnum);
		OnUnitAddedOrRemoved();
		this.OnUnitAdded?.Invoke(this, unit);
		if (countOfUnits == 0 && CountOfUnits > 0)
		{
			Team.TeamAI?.OnUnitAddedToFormationForTheFirstTime(this);
		}
	}

	public void RemoveUnit(Agent unit)
	{
		if (unit.IsDetachedFromFormation)
		{
			unit.Detachment.RemoveAgent(unit);
			_detachedUnits.Remove(unit);
			_looseDetachedUnits.Remove(unit);
			unit.Detachment = null;
			unit.DetachmentWeight = -1f;
		}
		else
		{
			Arrangement.RemoveUnit(unit);
		}
		if (unit.Character != null)
		{
			FormationClass formationClass = Team.Mission.GetAgentTroopClass(Team.Side, unit.Character).DefaultClass();
			_logicalClassCounts[(int)formationClass]--;
			if (_logicalClass == formationClass)
			{
				if (PostponeCostlyOperations)
				{
					_logicalClassNeedsUpdate = true;
				}
				else
				{
					CalculateLogicalClass();
					_logicalClassNeedsUpdate = false;
				}
			}
		}
		if (unit.IsPlayerTroop)
		{
			IsPlayerTroopInFormation = false;
		}
		if (unit.IsPlayerControlled)
		{
			HasPlayerControlledTroop = false;
		}
		if (unit == Captain && !unit.CanLeadFormationsRemotely)
		{
			Captain = null;
		}
		if (!unit.IsDetachableFromFormation && !unit.IsPlayerControlled)
		{
			OnUndetachableNonPlayerUnitRemoved(unit);
		}
		if (Mission.Current.Mode != MissionMode.Deployment && !IsAIControlled && CountOfUnits == 0)
		{
			SetControlledByAI(isControlledByAI: true);
		}
		_movementOrder.OnUnitJoinOrLeave(this, unit, isJoining: false);
		unit.SetTargetFormationIndex(-1);
		unit.SetFiringOrder(FiringOrder.RangedWeaponUsageOrderEnum.FireAtWill);
		unit.SetRidingOrder(RidingOrder.RidingOrderEnum.Free);
		OnUnitAddedOrRemoved();
		this.OnUnitRemoved?.Invoke(this, unit);
	}

	public void DetachUnit(Agent unit, bool isLoose)
	{
		Arrangement.RemoveUnit(unit);
		_detachedUnits.Add(unit);
		if (isLoose)
		{
			_looseDetachedUnits.Add(unit);
		}
		unit.SetBehaviorValueSet(HumanAIComponent.BehaviorValueSet.DefaultDetached);
		OnUnitAttachedOrDetached();
	}

	public void AttachUnit(Agent unit)
	{
		_detachedUnits.Remove(unit);
		_looseDetachedUnits.Remove(unit);
		Arrangement.AddUnit(unit);
		unit.Detachment = null;
		unit.DetachmentWeight = -1f;
		_movementOrder.OnUnitJoinOrLeave(this, unit, isJoining: true);
		OnUnitAttachedOrDetached();
	}

	public void SwitchUnitLocations(Agent firstUnit, Agent secondUnit)
	{
		if (!firstUnit.IsDetachedFromFormation && !secondUnit.IsDetachedFromFormation && (((IFormationUnit)firstUnit).FormationFileIndex != -1 || ((IFormationUnit)secondUnit).FormationFileIndex != -1))
		{
			if (((IFormationUnit)firstUnit).FormationFileIndex == -1)
			{
				Arrangement.SwitchUnitLocationsWithUnpositionedUnit(secondUnit, firstUnit);
			}
			else if (((IFormationUnit)secondUnit).FormationFileIndex == -1)
			{
				Arrangement.SwitchUnitLocationsWithUnpositionedUnit(firstUnit, secondUnit);
			}
			else
			{
				Arrangement.SwitchUnitLocations(firstUnit, secondUnit);
			}
		}
	}

	public void Tick(float dt)
	{
		if (Team.HasTeamAi && (IsAIControlled || Team.IsPlayerSergeant) && CountOfUnitsWithoutDetachedOnes > 0)
		{
			AI.Tick();
		}
		else
		{
			IsAITickedAfterSplit = true;
		}
		int num = 0;
		while (!_movementOrder.IsApplicable(this) && num++ < 10)
		{
			SetMovementOrder(_movementOrder.GetSubstituteOrder(this));
		}
		Formation targetFormation = TargetFormation;
		if (targetFormation != null && targetFormation.CountOfUnits <= 0)
		{
			TargetFormation = null;
		}
		if (_arrangementOrderTickOccasionallyTimer.Check(Mission.Current.CurrentTime))
		{
			_arrangementOrder.TickOccasionally(this);
		}
		_movementOrder.Tick(this);
		WorldPosition value = _movementOrder.CreateNewOrderWorldPosition(this, WorldPosition.WorldPositionEnforcedCache.None);
		Vec2 direction = _facingOrder.GetDirection(this, _movementOrder._targetAgent);
		if (value.IsValid || direction.IsValid)
		{
			SetPositioning(value, direction);
		}
		TickDetachments(dt);
		this.OnTick?.Invoke(this);
		SmoothAverageUnitPosition(dt);
		if (_isArrangementShapeChanged)
		{
			_isArrangementShapeChanged = false;
		}
	}

	public void JoinDetachment(IDetachment detachment)
	{
		if (!Team.DetachmentManager.ContainsDetachment(detachment))
		{
			Team.DetachmentManager.MakeDetachment(detachment);
		}
		_detachments.Add(detachment);
		Team.DetachmentManager.OnFormationJoinDetachment(this, detachment);
	}

	public void FormAttackEntityDetachment(GameEntity targetEntity)
	{
		AttackEntityOrderDetachment = new AttackEntityOrderDetachment(targetEntity);
		JoinDetachment(AttackEntityOrderDetachment);
	}

	public void LeaveDetachment(IDetachment detachment)
	{
		detachment.OnFormationLeave(this);
		_detachments.Remove(detachment);
		Team.DetachmentManager.OnFormationLeaveDetachment(this, detachment);
	}

	public void DisbandAttackEntityDetachment()
	{
		if (AttackEntityOrderDetachment != null)
		{
			Team.DetachmentManager.DestroyDetachment(AttackEntityOrderDetachment);
			AttackEntityOrderDetachment = null;
		}
	}

	public void Rearrange(IFormationArrangement arrangement)
	{
		if (!(Arrangement.GetType() == arrangement.GetType()))
		{
			IFormationArrangement arrangement2 = Arrangement;
			Arrangement = arrangement;
			arrangement2.RearrangeTo(arrangement);
			arrangement.RearrangeFrom(arrangement2);
			arrangement2.RearrangeTransferUnits(arrangement);
			ReapplyFormOrder();
			_movementOrder.OnArrangementChanged(this);
		}
	}

	public void TickForColumnArrangementInitialPositioning(Formation formation)
	{
		if ((ReferencePosition.Value - OrderPosition).LengthSquared >= 1f && !IsDeployment)
		{
			ArrangementOrder.RearrangeAux(this, isDirectly: true);
		}
	}

	public float CalculateFormationDirectionEnforcingFactorForRank(int rankIndex)
	{
		if (rankIndex == -1)
		{
			return 0f;
		}
		return ArrangementOrder.CalculateFormationDirectionEnforcingFactorForRank(rankIndex, Arrangement.RankCount);
	}

	public void BeginSpawn(int unitCount, bool isMounted)
	{
		IsSpawning = true;
		OverridenUnitCount = unitCount;
		_overridenHasAnyMountedUnit = isMounted;
	}

	public void EndSpawn()
	{
		IsSpawning = false;
		OverridenUnitCount = null;
		_overridenHasAnyMountedUnit = null;
	}

	public override int GetHashCode()
	{
		return (int)(Team.TeamIndex * 10 + FormationIndex);
	}

	internal bool IsUnitDetachedForDebug(Agent unit)
	{
		return _detachedUnits.Contains(unit);
	}

	internal IEnumerable<IFormationUnit> GetUnitsToPopWithPriorityFunction(int count, Func<Agent, int> priorityFunction, List<Agent> excludedHeroes, bool excludeBannerman)
	{
		List<IFormationUnit> list = new List<IFormationUnit>();
		if (count <= 0)
		{
			return list;
		}
		Func<Agent, bool> selectCondition = (Agent agent) => !excludedHeroes.Contains(agent) && (!excludeBannerman || agent.Banner == null);
		List<Agent> list2 = (from unit in _arrangement.GetAllUnits().Concat(_detachedUnits)
			where unit is Agent arg2 && selectCondition(arg2)
			select unit as Agent).ToList();
		if (list2.IsEmpty())
		{
			return list;
		}
		int num = count;
		int bestFit = int.MaxValue;
		while (num > 0 && bestFit > 0 && list2.Count > 0)
		{
			bestFit = list2.Max((Agent unit) => priorityFunction(unit));
			Func<IFormationUnit, bool> bestFitCondition = (IFormationUnit unit) => unit is Agent arg && selectCondition(arg) && priorityFunction(arg) == bestFit;
			int num2 = Math.Min(num, _arrangement.GetAllUnits().Count((IFormationUnit unit) => bestFitCondition(unit)));
			if (num2 > 0)
			{
				IEnumerable<IFormationUnit> toPop3 = _arrangement.GetUnitsToPopWithCondition(num2, bestFitCondition);
				if (!toPop3.IsEmpty())
				{
					list.AddRange(toPop3);
					num -= toPop3.Count();
					list2.RemoveAll((Agent unit) => toPop3.Contains(unit));
				}
			}
			if (num > 0)
			{
				IEnumerable<Agent> toPop2 = _looseDetachedUnits.Where((Agent agent) => bestFitCondition(agent)).Take(num);
				if (!toPop2.IsEmpty())
				{
					list.AddRange(toPop2);
					num -= toPop2.Count();
					list2.RemoveAll((Agent unit) => toPop2.Contains(unit));
				}
			}
			if (num <= 0)
			{
				continue;
			}
			IEnumerable<Agent> toPop = _detachedUnits.Where((Agent agent) => bestFitCondition(agent)).Take(num);
			if (!toPop.IsEmpty())
			{
				list.AddRange(toPop);
				num -= toPop.Count();
				list2.RemoveAll((Agent unit) => toPop.Contains(unit));
			}
		}
		return list;
	}

	internal void TransferUnitsWithPriorityFunction(Formation target, int unitCount, Func<Agent, int> priorityFunction, bool excludeBannerman, List<Agent> excludedAgents)
	{
		MBDebug.Print(FormationIndex.GetName() + " has " + CountOfUnits + " units, " + target.FormationIndex.GetName() + " has " + target.CountOfUnits + " units");
		MBDebug.Print(Team.Side.ToString() + " " + FormationIndex.GetName() + " transfers " + unitCount + " units to " + target.FormationIndex.GetName());
		if (unitCount == 0)
		{
			return;
		}
		if (target.CountOfUnits == 0)
		{
			target.CopyOrdersFrom(this);
			target.SetPositioning(_orderPosition, _direction, _unitSpacing);
		}
		foreach (Agent item in new List<IFormationUnit>(GetUnitsToPopWithPriorityFunction(unitCount, priorityFunction, excludedAgents, excludeBannerman)))
		{
			item.Formation = target;
		}
		Team.TriggerOnFormationsChanged(this);
		Team.TriggerOnFormationsChanged(target);
		MBDebug.Print(FormationIndex.GetName() + " has " + CountOfUnits + " units, " + target.FormationIndex.GetName() + " has " + target.CountOfUnits + " units");
	}

	private IFormationUnit GetClosestUnitToAux(Vec2 position, MBReadOnlyList<IFormationUnit> unitsWithSpaces, float? maxDistance)
	{
		if (unitsWithSpaces == null)
		{
			unitsWithSpaces = Arrangement.GetAllUnits();
		}
		IFormationUnit result = null;
		float num = (maxDistance.HasValue ? (maxDistance.Value * maxDistance.Value) : float.MaxValue);
		for (int i = 0; i < unitsWithSpaces.Count; i++)
		{
			IFormationUnit formationUnit = unitsWithSpaces[i];
			if (formationUnit != null)
			{
				float num2 = ((Agent)formationUnit).Position.AsVec2.DistanceSquared(position);
				if (num > num2)
				{
					num = num2;
					result = formationUnit;
				}
			}
		}
		return result;
	}

	private void CopyOrdersFrom(Formation target)
	{
		SetMovementOrder(target._movementOrder);
		FormOrder = target.FormOrder;
		SetPositioning(null, null, target.UnitSpacing);
		RidingOrder = target.RidingOrder;
		FiringOrder = target.FiringOrder;
		SetControlledByAI(target.IsAIControlled || !target.Team.IsPlayerGeneral);
		if (target.AI.Side != FormationAI.BehaviorSide.BehaviorSideNotSet)
		{
			AI.Side = target.AI.Side;
		}
		SetMovementOrder(target._movementOrder);
		TargetFormation = target.TargetFormation;
		FacingOrder = target.FacingOrder;
		ArrangementOrder = target.ArrangementOrder;
	}

	private void TickDetachments(float dt)
	{
		if (IsDeployment)
		{
			return;
		}
		for (int num = _detachments.Count - 1; num >= 0; num--)
		{
			IDetachment detachment = _detachments[num];
			UsableMachine usableMachine = detachment as UsableMachine;
			if (usableMachine?.Ai != null)
			{
				usableMachine.Ai.Tick(null, this, Team, dt);
				if (usableMachine.Ai.HasActionCompleted || (usableMachine.IsDisabledForBattleSideAI(Team.Side) && usableMachine.ShouldAutoLeaveDetachmentWhenDisabled(Team.Side)))
				{
					LeaveDetachment(detachment);
				}
			}
		}
	}

	[Conditional("DEBUG")]
	private void TickOrderDebug()
	{
		WorldPosition medianPosition = QuerySystem.MedianPosition;
		WorldPosition worldPosition = CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.GroundVec3);
		medianPosition.SetVec2(QuerySystem.AveragePosition);
		if (worldPosition.IsValid)
		{
			if (!_movementOrder.GetPosition(this).IsValid)
			{
				if (AI != null)
				{
					_ = AI.ActiveBehavior;
				}
			}
			else if (AI != null)
			{
				_ = AI.ActiveBehavior;
			}
		}
		else if (AI != null)
		{
			_ = AI.ActiveBehavior;
		}
	}

	[Conditional("DEBUG")]
	private void TickDebug(float dt)
	{
		if (MBDebug.IsDisplayingHighLevelAI && !IsSimulationFormation && _movementOrder.OrderEnum == MovementOrder.MovementOrderEnum.FollowEntity)
		{
			_ = _movementOrder.TargetEntity.Name;
		}
	}

	private void OnUnitAttachedOrDetached()
	{
		ReapplyFormOrder();
	}

	[Conditional("DEBUG")]
	private void DebugAssertDetachments()
	{
	}

	private void SetOrderPosition(WorldPosition pos)
	{
		_orderPosition = pos;
	}

	private int GetHeroPointForCaptainSelection(Agent agent)
	{
		return agent.Character.Level + 100 * agent.Character.GetSkillValue(DefaultSkills.Charm);
	}

	private void OnCaptainChanged()
	{
		ApplyActionOnEachUnit(delegate(Agent agent)
		{
			agent.UpdateAgentProperties();
		});
	}

	private void UpdateAgentDrivenPropertiesBasedOnOrderDefensiveness()
	{
		ApplyActionOnEachUnit(delegate(Agent agent)
		{
			agent.Defensiveness = _formationOrderDefensivenessFactor;
		});
	}

	private void ResetAux()
	{
		if (_detachments != null)
		{
			for (int num = _detachments.Count - 1; num >= 0; num--)
			{
				LeaveDetachment(_detachments[num]);
			}
		}
		else
		{
			_detachments = new MBList<IDetachment>();
		}
		_detachedUnits = new MBList<Agent>();
		_looseDetachedUnits = new MBList<Agent>();
		AttackEntityOrderDetachment = null;
		AI = new FormationAI(this);
		QuerySystem = new FormationQuerySystem(this);
		SetPositioning(null, Vec2.Forward, 1);
		SetMovementOrder(MovementOrder.MovementOrderStop);
		if (_overridenHasAnyMountedUnit.HasValue && _overridenHasAnyMountedUnit == true)
		{
			ArrangementOrder = ArrangementOrder.ArrangementOrderSkein;
		}
		else
		{
			FormOrder = FormOrder.FormOrderWide;
			ArrangementOrder = ArrangementOrder.ArrangementOrderLine;
		}
		RidingOrder = RidingOrder.RidingOrderFree;
		FiringOrder = FiringOrder.FiringOrderFireAtWill;
		Width = 0f * (Interval + UnitDiameter) + UnitDiameter;
		HasBeenPositioned = false;
		_currentSpawnIndex = 0;
		IsPlayerTroopInFormation = false;
		HasPlayerControlledTroop = false;
	}

	private void ResetForSimulation()
	{
		Arrangement.Reset();
		ResetAux();
	}

	private void TryRelocatePlayerUnit()
	{
		if (HasPlayerControlledTroop || IsPlayerTroopInFormation)
		{
			IFormationUnit playerUnit = Arrangement.GetPlayerUnit();
			if (playerUnit != null && playerUnit.FormationFileIndex >= 0 && playerUnit.FormationRankIndex >= 0)
			{
				Arrangement.SwitchUnitLocationsWithBackMostUnit(playerUnit);
			}
		}
	}

	private void ReapplyFormOrder()
	{
		FormOrder formOrder = FormOrder;
		if (FormOrder.OrderEnum == FormOrder.FormOrderEnum.Custom && ArrangementOrder.OrderEnum != 0)
		{
			formOrder.CustomFlankWidth = Arrangement.FlankWidth;
		}
		FormOrder = formOrder;
	}

	private void CalculateLogicalClass()
	{
		int num = 0;
		FormationClass logicalClass = FormationClass.NumberOfAllFormations;
		for (int i = 0; i < _logicalClassCounts.Length; i++)
		{
			FormationClass formationClass = (FormationClass)i;
			int num2 = _logicalClassCounts[i];
			if (num2 > num)
			{
				num = num2;
				logicalClass = formationClass;
			}
		}
		_logicalClass = logicalClass;
		if (_logicalClass != FormationClass.NumberOfAllFormations)
		{
			_representativeClass = _logicalClass;
		}
	}

	private void SmoothAverageUnitPosition(float dt)
	{
		_smoothedAverageUnitPosition = ((!_smoothedAverageUnitPosition.IsValid) ? QuerySystem.AveragePosition : Vec2.Lerp(_smoothedAverageUnitPosition, QuerySystem.AveragePosition, dt * 3f));
	}

	private void Arrangement_OnWidthChanged()
	{
		this.OnWidthChanged?.Invoke(this);
	}

	private void Arrangement_OnShapeChanged()
	{
		_orderLocalAveragePositionIsDirty = true;
		_isArrangementShapeChanged = true;
		if (!GameNetwork.IsMultiplayer)
		{
			TryRelocatePlayerUnit();
		}
	}

	public static float GetLastSimulatedFormationsOccupationWidthIfLesserThanActualWidth(Formation simulationFormation)
	{
		float occupationWidth = simulationFormation.Arrangement.GetOccupationWidth(simulationFormation.OverridenUnitCount.GetValueOrDefault());
		if (simulationFormation.Width > occupationWidth)
		{
			return occupationWidth;
		}
		return -1f;
	}

	public static List<WorldFrame> GetFormationFramesForBeforeFormationCreation(float width, int manCount, bool areMounted, WorldPosition spawnOrigin, Mat3 spawnRotation)
	{
		List<AgentArrangementData> list = new List<AgentArrangementData>();
		Formation formation = new Formation(null, -1);
		formation.SetOrderPosition(spawnOrigin);
		formation._direction = spawnRotation.f.AsVec2;
		LineFormation lineFormation = new LineFormation(formation);
		lineFormation.Width = width;
		for (int i = 0; i < manCount; i++)
		{
			list.Add(new AgentArrangementData(i, lineFormation));
		}
		lineFormation.OnFormationFrameChanged();
		foreach (AgentArrangementData item in list)
		{
			lineFormation.AddUnit(item);
		}
		List<WorldFrame> list2 = new List<WorldFrame>();
		int cachedOrderedAndAvailableUnitPositionIndicesCount = lineFormation.GetCachedOrderedAndAvailableUnitPositionIndicesCount();
		for (int j = 0; j < cachedOrderedAndAvailableUnitPositionIndicesCount; j++)
		{
			Vec2i cachedOrderedAndAvailableUnitPositionIndexAt = lineFormation.GetCachedOrderedAndAvailableUnitPositionIndexAt(j);
			WorldPosition globalPositionAtIndex = lineFormation.GetGlobalPositionAtIndex(cachedOrderedAndAvailableUnitPositionIndexAt.X, cachedOrderedAndAvailableUnitPositionIndexAt.Y);
			list2.Add(new WorldFrame(spawnRotation, globalPositionAtIndex));
		}
		return list2;
	}

	public static float GetDefaultUnitDiameter(bool isMounted)
	{
		if (isMounted)
		{
			return ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.QuadrupedalRadius) * 2f;
		}
		return ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalRadius) * 2f;
	}

	public static float GetDefaultMinimumInterval(bool isMounted)
	{
		if (!isMounted)
		{
			return InfantryInterval(0);
		}
		return CavalryInterval(0);
	}

	public static float GetDefaultMaximumInterval(bool isMounted)
	{
		if (!isMounted)
		{
			return InfantryInterval(2);
		}
		return CavalryInterval(2);
	}

	public static float GetDefaultMinimumDistance(bool isMounted)
	{
		if (!isMounted)
		{
			return InfantryDistance(0);
		}
		return CavalryDistance(0);
	}

	public static float GetDefaultMaximumDistance(bool isMounted)
	{
		if (!isMounted)
		{
			return InfantryDistance(2);
		}
		return CavalryDistance(2);
	}

	public static float InfantryInterval(int unitSpacing)
	{
		return 0.38f * (float)unitSpacing;
	}

	public static float CavalryInterval(int unitSpacing)
	{
		return 0.18f + 0.32f * (float)unitSpacing;
	}

	public static float InfantryDistance(int unitSpacing)
	{
		return 0.4f * (float)unitSpacing;
	}

	public static float CavalryDistance(int unitSpacing)
	{
		return 1.7f + 0.3f * (float)unitSpacing;
	}

	public static bool IsDefenseRelatedAIDrivenComponent(DrivenProperty drivenProperty)
	{
		if (drivenProperty != DrivenProperty.AIDecideOnAttackChance && drivenProperty != DrivenProperty.AIAttackOnDecideChance && drivenProperty != DrivenProperty.AIAttackOnParryChance && drivenProperty != DrivenProperty.AiUseShieldAgainstEnemyMissileProbability)
		{
			return drivenProperty == DrivenProperty.AiDefendWithShieldDecisionChanceValue;
		}
		return true;
	}

	private static void GetUnitPositionWithIndexAccordingToNewOrder(Formation simulationFormation, int unitIndex, in WorldPosition formationPosition, in Vec2 formationDirection, IFormationArrangement arrangement, float width, int unitSpacing, int unitCount, bool isMounted, int index, out WorldPosition? unitPosition, out Vec2? unitDirection, out float actualWidth)
	{
		unitPosition = null;
		unitDirection = null;
		if (simulationFormation == null)
		{
			if (_simulationFormationTemp == null || _simulationFormationUniqueIdentifier != index)
			{
				_simulationFormationTemp = new Formation(null, -1);
			}
			simulationFormation = _simulationFormationTemp;
		}
		if (simulationFormation.UnitSpacing != unitSpacing || TaleWorlds.Library.MathF.Abs(simulationFormation.Width - width + 1E-05f) >= simulationFormation.Interval + simulationFormation.UnitDiameter - 1E-05f || !simulationFormation.OrderPositionIsValid || !simulationFormation.OrderGroundPosition.NearlyEquals(formationPosition.GetGroundVec3(), 0.1f) || !simulationFormation.Direction.NearlyEquals(formationDirection, 0.1f) || simulationFormation.Arrangement.GetType() != arrangement.GetType())
		{
			simulationFormation._overridenHasAnyMountedUnit = isMounted;
			simulationFormation.ResetForSimulation();
			simulationFormation.SetPositioning(null, null, unitSpacing);
			simulationFormation.OverridenUnitCount = unitCount;
			simulationFormation.SetPositioning(formationPosition, formationDirection);
			simulationFormation.Rearrange(arrangement.Clone(simulationFormation));
			simulationFormation.Arrangement.DeepCopyFrom(arrangement);
			simulationFormation.Width = width;
			_simulationFormationUniqueIdentifier = index;
		}
		actualWidth = simulationFormation.Width;
		if (width >= actualWidth)
		{
			Vec2? vec = simulationFormation.Arrangement.GetLocalPositionOfUnitOrDefault(unitIndex);
			if (!vec.HasValue)
			{
				vec = simulationFormation.Arrangement.CreateNewPosition(unitIndex);
			}
			if (vec.HasValue)
			{
				Vec2 vec2 = simulationFormation.Direction.TransformToParentUnitF(vec.Value);
				WorldPosition value = simulationFormation.CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.None);
				value.SetVec2(value.AsVec2 + vec2);
				unitPosition = value;
				unitDirection = formationDirection;
			}
		}
	}

	private static IEnumerable<(WorldPosition, Vec2)> GetUnavailableUnitPositionsAccordingToNewOrder(Formation formation, Formation simulationFormation, WorldPosition position, Vec2 direction, IFormationArrangement arrangement, float width, int unitSpacing)
	{
		if (simulationFormation == null)
		{
			if (_simulationFormationTemp == null || _simulationFormationUniqueIdentifier != formation.Index)
			{
				_simulationFormationTemp = new Formation(null, -1);
			}
			simulationFormation = _simulationFormationTemp;
		}
		if (simulationFormation.UnitSpacing != unitSpacing || TaleWorlds.Library.MathF.Abs(simulationFormation.Width - width) >= simulationFormation.Interval + simulationFormation.UnitDiameter || !simulationFormation.OrderPositionIsValid || !simulationFormation.OrderGroundPosition.NearlyEquals(position.GetGroundVec3(), 0.1f) || !simulationFormation.Direction.NearlyEquals(direction, 0.1f) || simulationFormation.Arrangement.GetType() != arrangement.GetType())
		{
			simulationFormation._overridenHasAnyMountedUnit = formation.HasAnyMountedUnit;
			simulationFormation.ResetForSimulation();
			simulationFormation.SetPositioning(null, null, unitSpacing);
			simulationFormation.OverridenUnitCount = formation.CountOfUnitsWithoutDetachedOnes;
			simulationFormation.SetPositioning(position, direction);
			simulationFormation.Rearrange(arrangement.Clone(simulationFormation));
			simulationFormation.Arrangement.DeepCopyFrom(arrangement);
			simulationFormation.Width = width;
			_simulationFormationUniqueIdentifier = formation.Index;
		}
		IEnumerable<Vec2> unavailableUnitPositions = simulationFormation.Arrangement.GetUnavailableUnitPositions();
		foreach (Vec2 item2 in unavailableUnitPositions)
		{
			Vec2 vec = simulationFormation.Direction.TransformToParentUnitF(item2);
			WorldPosition item = simulationFormation.CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.None);
			item.SetVec2(item.AsVec2 + vec);
			yield return (item, direction);
		}
	}

	private static float TransformCustomWidthBetweenArrangementOrientations(ArrangementOrder.ArrangementOrderEnum orderTypeOld, ArrangementOrder.ArrangementOrderEnum orderTypeNew, float currentCustomWidth)
	{
		if (orderTypeOld != ArrangementOrder.ArrangementOrderEnum.Column && orderTypeNew == ArrangementOrder.ArrangementOrderEnum.Column)
		{
			return currentCustomWidth * 0.1f;
		}
		if (orderTypeOld == ArrangementOrder.ArrangementOrderEnum.Column && orderTypeNew != ArrangementOrder.ArrangementOrderEnum.Column)
		{
			return currentCustomWidth / 0.1f;
		}
		return currentCustomWidth;
	}
}
