using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public abstract class UsableMachine : SynchedMissionObject, IFocusable, IOrderable, IDetachment
{
	public const string UsableMachineParentTag = "machine_parent";

	public string PilotStandingPointTag = "Pilot";

	public string AmmoPickUpTag = "ammopickup";

	public string WaitStandingPointTag = "Wait";

	protected GameEntity ActiveWaitStandingPoint;

	private readonly List<UsableMissionObjectComponent> _components;

	private DestructableComponent _destructionComponent;

	protected bool _areUsableStandingPointsVacant = true;

	protected List<(int, StandingPoint)> _usableStandingPoints;

	protected bool _isDetachmentRecentlyEvaluated;

	private int _reevaluatedCount;

	private bool _isEvaluated;

	private float _cachedDetachmentWeight;

	protected float EnemyRangeToStopUsing;

	protected Vec2 MachinePositionOffsetToStopUsingLocal = Vec2.Zero;

	protected bool MakeVisibilityCheck = true;

	private UsableMachineAIBase _ai;

	private StandingPoint _currentlyUsedAmmoPickUpPoint;

	private QueryData<bool> _isDisabledForAttackerAIDueToEnemyInRange;

	private QueryData<bool> _isDisabledForDefenderAIDueToEnemyInRange;

	protected bool _isDisabledForAI;

	protected MBList<Formation> _userFormations;

	private bool _isMachineDeactivated;

	public MBList<StandingPoint> StandingPoints { get; private set; }

	public StandingPoint PilotStandingPoint { get; private set; }

	protected internal List<StandingPoint> AmmoPickUpPoints { get; private set; }

	protected List<GameEntity> WaitStandingPoints { get; private set; }

	public DestructableComponent DestructionComponent => _destructionComponent;

	public bool IsDestructible => DestructionComponent != null;

	public bool IsDestroyed
	{
		get
		{
			if (DestructionComponent != null)
			{
				return DestructionComponent.IsDestroyed;
			}
			return false;
		}
	}

	public Agent PilotAgent => PilotStandingPoint?.UserAgent;

	public bool IsLoose => false;

	public UsableMachineAIBase Ai
	{
		get
		{
			if (_ai == null)
			{
				_ai = CreateAIBehaviorObject();
			}
			return _ai;
		}
	}

	public virtual FocusableObjectType FocusableObjectType => FocusableObjectType.Item;

	public StandingPoint CurrentlyUsedAmmoPickUpPoint
	{
		get
		{
			return _currentlyUsedAmmoPickUpPoint;
		}
		set
		{
			_currentlyUsedAmmoPickUpPoint = value;
			SetScriptComponentToTick(GetTickRequirement());
		}
	}

	public bool HasAIPickingUpAmmo => CurrentlyUsedAmmoPickUpPoint != null;

	public MBReadOnlyList<Formation> UserFormations => _userFormations;

	public int UserCountNotInStruckAction
	{
		get
		{
			int num = 0;
			foreach (StandingPoint standingPoint in StandingPoints)
			{
				if (standingPoint.HasUser && !standingPoint.UserAgent.IsInBeingStruckAction)
				{
					num++;
				}
			}
			return num;
		}
	}

	public int UserCountIncludingInStruckAction
	{
		get
		{
			int num = 0;
			foreach (StandingPoint standingPoint in StandingPoints)
			{
				if (standingPoint.HasUser)
				{
					num++;
				}
			}
			return num;
		}
	}

	public virtual int MaxUserCount => StandingPoints.Count;

	public virtual bool HasWaitFrame => ActiveWaitStandingPoint != null;

	public MatrixFrame WaitFrame
	{
		get
		{
			if (ActiveWaitStandingPoint != null)
			{
				return ActiveWaitStandingPoint.GetGlobalFrame();
			}
			return default(MatrixFrame);
		}
	}

	public GameEntity WaitEntity => ActiveWaitStandingPoint;

	public virtual bool IsDeactivated
	{
		get
		{
			if (!_isMachineDeactivated)
			{
				return IsDestroyed;
			}
			return true;
		}
	}

	protected UsableMachine()
	{
		_components = new List<UsableMissionObjectComponent>();
	}

	public void AddComponent(UsableMissionObjectComponent component)
	{
		_components.Add(component);
		component.OnAdded(base.Scene);
		SetScriptComponentToTick(GetTickRequirement());
	}

	public void RemoveComponent(UsableMissionObjectComponent component)
	{
		component.OnRemoved();
		_components.Remove(component);
		SetScriptComponentToTick(GetTickRequirement());
	}

	public T GetComponent<T>() where T : UsableMissionObjectComponent
	{
		return _components.Find((UsableMissionObjectComponent c) => c is T) as T;
	}

	public virtual OrderType GetOrder(BattleSideEnum side)
	{
		return OrderType.Use;
	}

	public virtual UsableMachineAIBase CreateAIBehaviorObject()
	{
		return null;
	}

	public GameEntity GetValidStandingPointForAgent(Agent agent)
	{
		float num = float.MaxValue;
		StandingPoint standingPoint = null;
		foreach (StandingPoint standingPoint2 in StandingPoints)
		{
			if (!standingPoint2.IsDisabledForAgent(agent) && (!standingPoint2.HasUser || standingPoint2.HasAIUser))
			{
				float num2 = standingPoint2.GetUserFrameForAgent(agent).Origin.AsVec2.DistanceSquared(agent.Position.AsVec2);
				if (agent.CanReachAndUseObject(standingPoint2, num2) && num2 < num && TaleWorlds.Library.MathF.Abs(standingPoint2.GetUserFrameForAgent(agent).Origin.GetGroundVec3().z - agent.Position.z) < 1.5f)
				{
					num = num2;
					standingPoint = standingPoint2;
				}
			}
		}
		return standingPoint?.GameEntity;
	}

	public GameEntity GetValidStandingPointForAgentWithoutDistanceCheck(Agent agent)
	{
		float num = float.MaxValue;
		StandingPoint standingPoint = null;
		foreach (StandingPoint standingPoint2 in StandingPoints)
		{
			if (!standingPoint2.IsDisabledForAgent(agent) && (!standingPoint2.HasUser || standingPoint2.HasAIUser))
			{
				float num2 = standingPoint2.GetUserFrameForAgent(agent).Origin.AsVec2.DistanceSquared(agent.Position.AsVec2);
				if (num2 < num && TaleWorlds.Library.MathF.Abs(standingPoint2.GetUserFrameForAgent(agent).Origin.GetGroundVec3().z - agent.Position.z) < 1.5f)
				{
					num = num2;
					standingPoint = standingPoint2;
				}
			}
		}
		return standingPoint?.GameEntity;
	}

	public StandingPoint GetVacantStandingPointForAI(Agent agent)
	{
		if (PilotStandingPoint != null && !PilotStandingPoint.IsDisabledForAgent(agent) && !AmmoPickUpPoints.Contains(PilotStandingPoint))
		{
			return PilotStandingPoint;
		}
		float num = 100000000f;
		StandingPoint result = null;
		foreach (StandingPoint standingPoint in StandingPoints)
		{
			bool flag = true;
			if (AmmoPickUpPoints.Contains(standingPoint))
			{
				foreach (StandingPoint standingPoint2 in StandingPoints)
				{
					if (standingPoint2 is StandingPointWithWeaponRequirement && !AmmoPickUpPoints.Contains(standingPoint2) && (standingPoint2.IsDeactivated || standingPoint2.HasUser || standingPoint2.HasAIMovingTo))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag && !standingPoint.IsDisabledForAgent(agent))
			{
				float num2 = (agent.Position - standingPoint.GetUserFrameForAgent(agent).Origin.GetGroundVec3()).LengthSquared;
				if (!standingPoint.IsDisabledForPlayers)
				{
					num2 -= 100000f;
				}
				if (num2 < num)
				{
					num = num2;
					result = standingPoint;
				}
			}
		}
		return result;
	}

	public StandingPoint GetTargetStandingPointOfAIAgent(Agent agent)
	{
		foreach (StandingPoint standingPoint in StandingPoints)
		{
			if (standingPoint.IsAIMovingTo(agent))
			{
				return standingPoint;
			}
		}
		return null;
	}

	public override void OnMissionEnded()
	{
		foreach (StandingPoint standingPoint in StandingPoints)
		{
			standingPoint.UserAgent?.StopUsingGameObject();
			standingPoint.IsDeactivated = true;
		}
	}

	public override void SetVisibleSynched(bool value, bool forceChildrenVisible = false)
	{
		base.SetVisibleSynched(value, forceChildrenVisible);
	}

	public override void SetPhysicsStateSynched(bool value, bool setChildren = true)
	{
		base.SetPhysicsStateSynched(value, setChildren);
		SetAbilityOfFaces(value);
		foreach (StandingPoint standingPoint in StandingPoints)
		{
			standingPoint.OnParentMachinePhysicsStateChanged();
		}
	}

	protected internal override void OnEditorInit()
	{
		base.OnEditorInit();
		CollectAndSetStandingPoints();
	}

	protected internal override void OnInit()
	{
		base.OnInit();
		_isDisabledForAttackerAIDueToEnemyInRange = new QueryData<bool>(delegate
		{
			bool result2 = false;
			if (EnemyRangeToStopUsing > 0f)
			{
				Vec3 vec2 = base.GameEntity.GetGlobalFrame().rotation.TransformToParent(new Vec3(MachinePositionOffsetToStopUsingLocal));
				Vec3 position2 = base.GameEntity.GlobalPosition + vec2;
				Agent closestEnemyAgent2 = Mission.Current.GetClosestEnemyAgent(Mission.Current.Teams.Attacker, position2, EnemyRangeToStopUsing);
				result2 = closestEnemyAgent2 != null && closestEnemyAgent2.Position.z > position2.z - 2f && closestEnemyAgent2.Position.z < position2.z + 4f;
			}
			return result2;
		}, 1f);
		_isDisabledForDefenderAIDueToEnemyInRange = new QueryData<bool>(delegate
		{
			bool result = false;
			if (EnemyRangeToStopUsing > 0f)
			{
				Vec3 vec = base.GameEntity.GetGlobalFrame().rotation.TransformToParent(new Vec3(MachinePositionOffsetToStopUsingLocal));
				Vec3 position = base.GameEntity.GlobalPosition + vec;
				Agent closestEnemyAgent = Mission.Current.GetClosestEnemyAgent(Mission.Current.Teams.Defender, position, EnemyRangeToStopUsing);
				result = closestEnemyAgent != null && closestEnemyAgent.Position.z > position.z - 2f && closestEnemyAgent.Position.z < position.z + 4f;
			}
			return result;
		}, 1f);
		CollectAndSetStandingPoints();
		AmmoPickUpPoints = new List<StandingPoint>();
		_destructionComponent = base.GameEntity.GetFirstScriptOfType<DestructableComponent>();
		PilotStandingPoint = null;
		foreach (StandingPoint standingPoint in StandingPoints)
		{
			if (standingPoint.GameEntity.HasTag(PilotStandingPointTag))
			{
				PilotStandingPoint = standingPoint;
			}
			if (standingPoint.GameEntity.HasTag(AmmoPickUpTag))
			{
				AmmoPickUpPoints.Add(standingPoint);
			}
			standingPoint.InitializeDefendingAgents();
		}
		WaitStandingPoints = base.GameEntity.CollectChildrenEntitiesWithTag(WaitStandingPointTag);
		if (WaitStandingPoints.Count > 0)
		{
			ActiveWaitStandingPoint = WaitStandingPoints[0];
		}
		_userFormations = new MBList<Formation>();
		_usableStandingPoints = new List<(int, StandingPoint)>();
		SetScriptComponentToTick(GetTickRequirement());
	}

	private void CollectAndSetStandingPoints()
	{
		GameEntity parent = base.GameEntity.Parent;
		if ((object)parent != null && parent.HasTag("machine_parent"))
		{
			StandingPoints = base.GameEntity.Parent.CollectObjects<StandingPoint>();
		}
		else
		{
			StandingPoints = base.GameEntity.CollectObjects<StandingPoint>();
		}
	}

	public override TickRequirement GetTickRequirement()
	{
		bool flag = false;
		foreach (UsableMissionObjectComponent component in _components)
		{
			if (component.IsOnTickRequired())
			{
				flag = true;
				break;
			}
		}
		if (base.GameEntity.IsVisibleIncludeParents() && (flag || (!GameNetwork.IsClientOrReplay && HasAIPickingUpAmmo)))
		{
			return base.GetTickRequirement() | TickRequirement.Tick;
		}
		return base.GetTickRequirement();
	}

	protected internal override void OnTick(float dt)
	{
		base.OnTick(dt);
		if (MakeVisibilityCheck && !base.GameEntity.IsVisibleIncludeParents())
		{
			return;
		}
		if (!GameNetwork.IsClientOrReplay && HasAIPickingUpAmmo && !CurrentlyUsedAmmoPickUpPoint.HasAIMovingTo && !CurrentlyUsedAmmoPickUpPoint.HasAIUser)
		{
			CurrentlyUsedAmmoPickUpPoint = null;
		}
		foreach (UsableMissionObjectComponent component in _components)
		{
			component.OnTick(dt);
		}
		_ = GameNetwork.IsClientOrReplay;
	}

	private static string DebugGetMemberNameOf<T>(object instance, T sp) where T : class
	{
		Type type = instance.GetType();
		PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.GetMethod == null)
			{
				continue;
			}
			if (propertyInfo.GetValue(instance) == sp)
			{
				return propertyInfo.Name;
			}
			if (!propertyInfo.GetType().IsGenericType || (!(propertyInfo.GetType().GetGenericTypeDefinition() == typeof(List<>)) && !(propertyInfo.GetType().GetGenericTypeDefinition() == typeof(MBList<>)) && !(propertyInfo.GetType().GetGenericTypeDefinition() == typeof(MBReadOnlyList<>))) || !(propertyInfo.GetValue(instance) is IReadOnlyList<StandingPoint> readOnlyList))
			{
				continue;
			}
			for (int j = 0; j < readOnlyList.Count; j++)
			{
				StandingPoint standingPoint = readOnlyList[j];
				if (sp == standingPoint)
				{
					return propertyInfo.Name + "[" + j + "]";
				}
			}
		}
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.GetValue(instance) == sp)
			{
				return fieldInfo.Name;
			}
			if (!fieldInfo.FieldType.IsGenericType || (!(fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>)) && !(fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(MBList<>)) && !(fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(MBReadOnlyList<>))) || !(fieldInfo.GetValue(instance) is IReadOnlyList<StandingPoint> readOnlyList2))
			{
				continue;
			}
			for (int k = 0; k < readOnlyList2.Count; k++)
			{
				StandingPoint standingPoint2 = readOnlyList2[k];
				if (sp == standingPoint2)
				{
					return fieldInfo.Name + "[" + k + "]";
				}
			}
		}
		return null;
	}

	[Conditional("_RGL_KEEP_ASSERTS")]
	protected virtual void DebugTick(float dt)
	{
		if (!MBDebug.IsDisplayingHighLevelAI)
		{
			return;
		}
		foreach (StandingPoint standingPoint in StandingPoints)
		{
			_ = standingPoint.GameEntity.GlobalPosition;
			_ = Vec3.One / 3f;
			_ = standingPoint.IsDeactivated;
		}
	}

	protected internal override void OnEditorTick(float dt)
	{
		base.OnEditorTick(dt);
		foreach (UsableMissionObjectComponent component in _components)
		{
			component.OnEditorTick(dt);
		}
	}

	protected internal override void OnEditorValidate()
	{
		base.OnEditorValidate();
		foreach (UsableMissionObjectComponent component in _components)
		{
			component.OnEditorValidate();
		}
	}

	public virtual void OnFocusGain(Agent userAgent)
	{
		foreach (UsableMissionObjectComponent component in _components)
		{
			component.OnFocusGain(userAgent);
		}
	}

	public virtual void OnFocusLose(Agent userAgent)
	{
		foreach (UsableMissionObjectComponent component in _components)
		{
			component.OnFocusLose(userAgent);
		}
	}

	public virtual TextObject GetInfoTextForBeingNotInteractable(Agent userAgent)
	{
		return TextObject.Empty;
	}

	protected internal override void OnMissionReset()
	{
		base.OnMissionReset();
		foreach (UsableMissionObjectComponent component in _components)
		{
			component.OnMissionReset();
		}
	}

	public void Deactivate()
	{
		_isMachineDeactivated = true;
		foreach (StandingPoint standingPoint in StandingPoints)
		{
			standingPoint.IsDeactivated = true;
		}
	}

	public void Activate()
	{
		_isMachineDeactivated = false;
		foreach (StandingPoint standingPoint in StandingPoints)
		{
			standingPoint.IsDeactivated = false;
		}
	}

	public virtual bool IsDisabledForBattleSide(BattleSideEnum sideEnum)
	{
		return IsDeactivated;
	}

	public virtual bool IsDisabledForBattleSideAI(BattleSideEnum sideEnum)
	{
		if (_isDisabledForAI)
		{
			return true;
		}
		if (EnemyRangeToStopUsing <= 0f)
		{
			return false;
		}
		if (sideEnum != BattleSideEnum.None)
		{
			return IsDisabledDueToEnemyInRange(sideEnum);
		}
		return false;
	}

	public virtual bool ShouldAutoLeaveDetachmentWhenDisabled(BattleSideEnum sideEnum)
	{
		return true;
	}

	protected bool IsDisabledDueToEnemyInRange(BattleSideEnum sideEnum)
	{
		if (sideEnum == BattleSideEnum.Attacker)
		{
			return _isDisabledForAttackerAIDueToEnemyInRange.Value;
		}
		return _isDisabledForDefenderAIDueToEnemyInRange.Value;
	}

	public virtual bool AutoAttachUserToFormation(BattleSideEnum sideEnum)
	{
		return true;
	}

	public virtual bool HasToBeDefendedByUser(BattleSideEnum sideEnum)
	{
		return false;
	}

	public virtual void Disable()
	{
		foreach (Team item in Mission.Current.Teams.Where((Team t) => t.DetachmentManager.ContainsDetachment(this)))
		{
			item.DetachmentManager.DestroyDetachment(this);
		}
		foreach (StandingPoint standingPoint in StandingPoints)
		{
			if (!standingPoint.GameEntity.HasTag(AmmoPickUpTag))
			{
				if (standingPoint.HasUser)
				{
					standingPoint.UserAgent.StopUsingGameObject();
				}
				standingPoint.SetIsDeactivatedSynched(value: true);
			}
		}
	}

	protected override void OnRemoved(int removeReason)
	{
		base.OnRemoved(removeReason);
		foreach (UsableMissionObjectComponent component in _components)
		{
			component.OnRemoved();
		}
	}

	public override string ToString()
	{
		string text = string.Concat(GetType(), " with Components:");
		foreach (UsableMissionObjectComponent component in _components)
		{
			text = string.Concat(text, "[", component, "]");
		}
		return text;
	}

	public abstract TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject);

	public virtual StandingPoint GetBestPointAlternativeTo(StandingPoint standingPoint, Agent agent)
	{
		return standingPoint;
	}

	public virtual bool IsInRangeToCheckAlternativePoints(Agent agent)
	{
		float num = ((StandingPoints.Count > 0) ? (agent.GetInteractionDistanceToUsable(StandingPoints[0]) + 1f) : 2f);
		return base.GameEntity.GlobalPosition.DistanceSquared(agent.Position) < num * num;
	}

	void IDetachment.OnFormationLeave(Formation formation)
	{
		foreach (StandingPoint standingPoint in StandingPoints)
		{
			Agent userAgent = standingPoint.UserAgent;
			if (userAgent != null && userAgent.Formation == formation && !userAgent.IsPlayerControlled)
			{
				OnFormationLeaveHelper(formation, userAgent);
			}
			Agent movingAgent = standingPoint.MovingAgent;
			if (movingAgent != null && movingAgent.Formation == formation)
			{
				OnFormationLeaveHelper(formation, movingAgent);
			}
			for (int num = standingPoint.GetDefendingAgentCount() - 1; num >= 0; num--)
			{
				Agent agent = standingPoint.DefendingAgents[num];
				if (agent.Formation == formation)
				{
					OnFormationLeaveHelper(formation, agent);
				}
			}
		}
	}

	private void OnFormationLeaveHelper(Formation formation, Agent agent)
	{
		((IDetachment)this).RemoveAgent(agent);
		formation.AttachUnit(agent);
	}

	bool IDetachment.IsAgentUsingOrInterested(Agent agent)
	{
		foreach (StandingPoint standingPoint in StandingPoints)
		{
			if (agent.CurrentlyUsedGameObject == standingPoint || (agent.IsAIControlled && agent.AIInterestedInGameObject(standingPoint)))
			{
				return true;
			}
		}
		return false;
	}

	protected virtual float GetWeightOfStandingPoint(StandingPoint sp)
	{
		if (!sp.HasAIMovingTo)
		{
			return 0.6f;
		}
		return 0.2f;
	}

	float IDetachment.GetDetachmentWeight(BattleSideEnum side)
	{
		return GetDetachmentWeightAux(side);
	}

	protected virtual float GetDetachmentWeightAux(BattleSideEnum side)
	{
		if (IsDisabledForBattleSideAI(side))
		{
			return float.MinValue;
		}
		_usableStandingPoints.Clear();
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < StandingPoints.Count; i++)
		{
			StandingPoint standingPoint = StandingPoints[i];
			if (!standingPoint.IsUsableBySide(side))
			{
				continue;
			}
			if (!standingPoint.HasAIMovingTo)
			{
				if (!flag2)
				{
					_usableStandingPoints.Clear();
				}
				flag2 = true;
			}
			else if (flag2 || standingPoint.MovingAgent.Formation.Team.Side != side)
			{
				continue;
			}
			flag = true;
			_usableStandingPoints.Add((i, standingPoint));
		}
		_areUsableStandingPointsVacant = flag2;
		if (!flag)
		{
			return float.MinValue;
		}
		if (flag2)
		{
			return 1f;
		}
		if (!_isDetachmentRecentlyEvaluated)
		{
			return 0.1f;
		}
		return 0.01f;
	}

	void IDetachment.GetSlotIndexWeightTuples(List<(int, float)> slotIndexWeightTuples)
	{
		foreach (var usableStandingPoint in _usableStandingPoints)
		{
			StandingPoint item = usableStandingPoint.Item2;
			slotIndexWeightTuples.Add((usableStandingPoint.Item1, GetWeightOfStandingPoint(item) * ((!_areUsableStandingPointsVacant && item.HasRecentlyBeenRechecked) ? 0.1f : 1f)));
		}
	}

	bool IDetachment.IsSlotAtIndexAvailableForAgent(int slotIndex, Agent agent)
	{
		if (agent.CanBeAssignedForScriptedMovement() && !StandingPoints[slotIndex].IsDisabledForAgent(agent))
		{
			return !IsAgentOnInconvenientNavmesh(agent, StandingPoints[slotIndex]);
		}
		return false;
	}

	protected virtual bool IsAgentOnInconvenientNavmesh(Agent agent, StandingPoint standingPoint)
	{
		if (Mission.Current.MissionTeamAIType != Mission.MissionTeamAITypeEnum.Siege)
		{
			return false;
		}
		int currentNavigationFaceId = agent.GetCurrentNavigationFaceId();
		if (agent.Team.TeamAI is TeamAISiegeComponent teamAISiegeComponent)
		{
			if (teamAISiegeComponent is TeamAISiegeAttacker && currentNavigationFaceId % 10 == 1)
			{
				return true;
			}
			if (teamAISiegeComponent is TeamAISiegeDefender && currentNavigationFaceId % 10 != 1)
			{
				return true;
			}
			foreach (int difficultNavmeshID in teamAISiegeComponent.DifficultNavmeshIDs)
			{
				if (currentNavigationFaceId == difficultNavmeshID)
				{
					return true;
				}
			}
		}
		return false;
	}

	bool IDetachment.IsAgentEligible(Agent agent)
	{
		return true;
	}

	public void AddAgentAtSlotIndex(Agent agent, int slotIndex)
	{
		StandingPoint standingPoint = StandingPoints[slotIndex];
		if (standingPoint.HasAIMovingTo)
		{
			Agent movingAgent = standingPoint.MovingAgent;
			if (movingAgent != null)
			{
				((IDetachment)this).RemoveAgent(movingAgent);
				movingAgent.Formation?.AttachUnit(movingAgent);
			}
		}
		if (standingPoint.HasDefendingAgent)
		{
			for (int num = standingPoint.DefendingAgents.Count - 1; num >= 0; num--)
			{
				Agent agent2 = standingPoint.DefendingAgents[num];
				if (agent2 != null)
				{
					((IDetachment)this).RemoveAgent(agent2);
					agent2.Formation?.AttachUnit(agent2);
				}
			}
		}
		((IDetachment)this).AddAgent(agent, slotIndex);
		agent.Formation?.DetachUnit(agent, isLoose: false);
		agent.Detachment = this;
		agent.DetachmentWeight = 1f;
	}

	Agent IDetachment.GetMovingAgentAtSlotIndex(int slotIndex)
	{
		return StandingPoints[slotIndex].MovingAgent;
	}

	bool IDetachment.IsDetachmentRecentlyEvaluated()
	{
		return _isDetachmentRecentlyEvaluated;
	}

	void IDetachment.UnmarkDetachment()
	{
		_isDetachmentRecentlyEvaluated = false;
	}

	void IDetachment.MarkSlotAtIndex(int slotIndex)
	{
		int count = _usableStandingPoints.Count;
		if (++_reevaluatedCount >= count)
		{
			foreach (var usableStandingPoint in _usableStandingPoints)
			{
				usableStandingPoint.Item2.HasRecentlyBeenRechecked = false;
			}
			_isDetachmentRecentlyEvaluated = true;
			_reevaluatedCount = 0;
		}
		else
		{
			StandingPoints[slotIndex].HasRecentlyBeenRechecked = true;
		}
	}

	float? IDetachment.GetWeightOfNextSlot(BattleSideEnum side)
	{
		if (IsDisabledForBattleSideAI(side))
		{
			return null;
		}
		StandingPoint suitableStandingPointFor = GetSuitableStandingPointFor(side);
		if (suitableStandingPointFor != null)
		{
			return GetWeightOfStandingPoint(suitableStandingPointFor);
		}
		return null;
	}

	float IDetachment.GetExactCostOfAgentAtSlot(Agent candidate, int slotIndex)
	{
		StandingPoint standingPoint = StandingPoints[slotIndex];
		WorldPosition point = new WorldPosition(position: standingPoint.GameEntity.GlobalPosition, scene: candidate.Mission.Scene);
		WorldPosition point2 = candidate.GetWorldPosition();
		if (!standingPoint.Scene.GetPathDistanceBetweenPositions(ref point, ref point2, candidate.Monster.BodyCapsuleRadius, out var pathDistance))
		{
			return float.MaxValue;
		}
		return pathDistance;
	}

	List<float> IDetachment.GetTemplateCostsOfAgent(Agent candidate, List<float> oldValue)
	{
		List<float> list = oldValue ?? new List<float>(StandingPoints.Count);
		list.Clear();
		for (int i = 0; i < StandingPoints.Count; i++)
		{
			list.Add(float.MaxValue);
		}
		foreach (var usableStandingPoint in _usableStandingPoints)
		{
			float num = usableStandingPoint.Item2.GameEntity.GlobalPosition.Distance(candidate.Position);
			list[usableStandingPoint.Item1] = num * MissionGameModels.Current.AgentStatCalculateModel.GetDetachmentCostMultiplierOfAgent(candidate, this);
		}
		return list;
	}

	float IDetachment.GetTemplateWeightOfAgent(Agent candidate)
	{
		Scene scene = Mission.Current.Scene;
		Vec3 globalPosition = base.GameEntity.GlobalPosition;
		WorldPosition point = candidate.GetWorldPosition();
		WorldPosition point2 = new WorldPosition(scene, UIntPtr.Zero, globalPosition, hasValidZ: true);
		if (!scene.GetPathDistanceBetweenPositions(ref point2, ref point, candidate.Monster.BodyCapsuleRadius, out var pathDistance))
		{
			return float.MaxValue;
		}
		return pathDistance;
	}

	float IDetachment.GetWeightOfOccupiedSlot(Agent agent)
	{
		return GetWeightOfStandingPoint(StandingPoints.FirstOrDefault((StandingPoint sp) => sp.UserAgent == agent || sp.IsAIMovingTo(agent)));
	}

	WorldFrame? IDetachment.GetAgentFrame(Agent agent)
	{
		return null;
	}

	void IDetachment.RemoveAgent(Agent agent)
	{
		agent.StopUsingGameObjectMT(isSuccessful: true, Agent.StopUsingGameObjectFlags.None);
	}

	public int GetNumberOfUsableSlots()
	{
		return _usableStandingPoints.Count;
	}

	public bool IsStandingPointAvailableForAgent(Agent agent)
	{
		bool result = false;
		foreach (StandingPoint standingPoint in StandingPoints)
		{
			if (!standingPoint.IsDeactivated && (standingPoint.IsInstantUse || ((!standingPoint.HasUser || standingPoint.UserAgent == agent) && (!standingPoint.HasAIMovingTo || standingPoint.IsAIMovingTo(agent)))) && !standingPoint.IsDisabledForAgent(agent) && !IsStandingPointNotUsedOnAccountOfBeingAmmoLoad(standingPoint))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	float? IDetachment.GetWeightOfAgentAtNextSlot(List<Agent> candidates, out Agent match)
	{
		BattleSideEnum side = candidates[0].Team.Side;
		StandingPoint suitableStandingPointFor = GetSuitableStandingPointFor(side, null, candidates);
		if (suitableStandingPointFor != null)
		{
			match = UsableMachineAIBase.GetSuitableAgentForStandingPoint(this, suitableStandingPointFor, candidates, new List<Agent>());
			if (match != null)
			{
				return ((IDetachment)this).GetWeightOfNextSlot(side) * 1f;
			}
			return null;
		}
		match = null;
		return null;
	}

	float? IDetachment.GetWeightOfAgentAtNextSlot(List<(Agent, float)> candidates, out Agent match)
	{
		BattleSideEnum side = candidates[0].Item1.Team.Side;
		StandingPoint suitableStandingPointFor = GetSuitableStandingPointFor(side, null, null, candidates);
		if (suitableStandingPointFor != null)
		{
			float? weightOfNextSlot = ((IDetachment)this).GetWeightOfNextSlot(side);
			match = UsableMachineAIBase.GetSuitableAgentForStandingPoint(this, suitableStandingPointFor, candidates, new List<Agent>(), weightOfNextSlot.Value);
			if (match != null)
			{
				return weightOfNextSlot * 1f;
			}
			return null;
		}
		match = null;
		return null;
	}

	float? IDetachment.GetWeightOfAgentAtOccupiedSlot(Agent detachedAgent, List<Agent> candidates, out Agent match)
	{
		BattleSideEnum side = candidates[0].Team.Side;
		match = null;
		foreach (StandingPoint standingPoint in StandingPoints)
		{
			if (standingPoint.IsAIMovingTo(detachedAgent) || standingPoint.UserAgent == detachedAgent)
			{
				match = UsableMachineAIBase.GetSuitableAgentForStandingPoint(this, standingPoint, candidates, new List<Agent>());
				break;
			}
		}
		if (match != null)
		{
			return ((IDetachment)this).GetWeightOfNextSlot(side) * 1f * 0.5f;
		}
		return null;
	}

	void IDetachment.AddAgent(Agent agent, int slotIndex)
	{
		StandingPoint standingPoint = ((slotIndex == -1) ? GetSuitableStandingPointFor(agent.Team.Side, agent) : StandingPoints[slotIndex]);
		if (standingPoint != null)
		{
			if (standingPoint.HasAIMovingTo && !standingPoint.IsInstantUse)
			{
				standingPoint.MovingAgent.StopUsingGameObject();
			}
			agent.AIMoveToGameObjectEnable(standingPoint, this, Ai.GetScriptedFrameFlags(agent));
			if (standingPoint.GameEntity.HasTag(AmmoPickUpTag))
			{
				CurrentlyUsedAmmoPickUpPoint = standingPoint;
			}
		}
		else
		{
			TaleWorlds.Library.Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Objects\\Usables\\UsableMachine.cs", "AddAgent", 1367);
		}
	}

	void IDetachment.FormationStartUsing(Formation formation)
	{
		_userFormations.Add(formation);
	}

	void IDetachment.FormationStopUsing(Formation formation)
	{
		_userFormations.Remove(formation);
	}

	public bool IsUsedByFormation(Formation formation)
	{
		return _userFormations.Contains(formation);
	}

	void IDetachment.ResetEvaluation()
	{
		_isEvaluated = false;
	}

	bool IDetachment.IsEvaluated()
	{
		return _isEvaluated;
	}

	void IDetachment.SetAsEvaluated()
	{
		_isEvaluated = true;
	}

	float IDetachment.GetDetachmentWeightFromCache()
	{
		return _cachedDetachmentWeight;
	}

	float IDetachment.ComputeAndCacheDetachmentWeight(BattleSideEnum side)
	{
		_cachedDetachmentWeight = GetDetachmentWeightAux(side);
		return _cachedDetachmentWeight;
	}

	protected internal virtual bool IsStandingPointNotUsedOnAccountOfBeingAmmoLoad(StandingPoint standingPoint)
	{
		if (AmmoPickUpPoints.Contains(standingPoint))
		{
			if (StandingPoints.Any((StandingPoint standingPoint2) => (standingPoint2.IsDeactivated || standingPoint2.HasUser || standingPoint2.HasAIMovingTo) && !standingPoint2.GameEntity.HasTag(AmmoPickUpTag) && standingPoint2 is StandingPointWithWeaponRequirement))
			{
				return true;
			}
			if (HasAIPickingUpAmmo)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	protected virtual StandingPoint GetSuitableStandingPointFor(BattleSideEnum side, Agent agent = null, List<Agent> agents = null, List<(Agent, float)> agentValuePairs = null)
	{
		return StandingPoints.FirstOrDefault((StandingPoint sp) => !sp.IsDeactivated && (sp.IsInstantUse || (!sp.HasUser && !sp.HasAIMovingTo)) && (agent == null || !sp.IsDisabledForAgent(agent)) && (agents == null || agents.Any((Agent a) => !sp.IsDisabledForAgent(a))) && (agentValuePairs == null || agentValuePairs.Any(((Agent, float) avp) => !sp.IsDisabledForAgent(avp.Item1))) && !IsStandingPointNotUsedOnAccountOfBeingAmmoLoad(sp));
	}

	public abstract string GetDescriptionText(GameEntity gameEntity = null);
}
