using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class HumanAIComponent : AgentComponent
{
	[EngineStruct("behavior_values_struct", false)]
	public struct BehaviorValues
	{
		public float y1;

		public float x2;

		public float y2;

		public float x3;

		public float y3;

		public float GetValueAt(float x)
		{
			if (x <= x2)
			{
				return (y2 - y1) * x / x2 + y1;
			}
			if (x <= x3)
			{
				return (y3 - y2) * (x - x2) / (x3 - x2) + y2;
			}
			return y3;
		}
	}

	public enum AISimpleBehaviorKind
	{
		GoToPos,
		Melee,
		Ranged,
		ChargeHorseback,
		RangedHorseback,
		AttackEntityMelee,
		AttackEntityRanged,
		Count
	}

	public enum BehaviorValueSet
	{
		Default,
		DefensiveArrangementMove,
		Follow,
		DefaultMove,
		Charge,
		DefaultDetached,
		Count
	}

	public enum UsableObjectInterestKind
	{
		None,
		MovingTo,
		Defending,
		Count
	}

	private const float AvoidPickUpIfLookAgentIsCloseDistance = 20f;

	private const float AvoidPickUpIfLookAgentIsCloseDistanceSquared = 400f;

	private const float ClosestMountSearchRangeSq = 6400f;

	public static bool FormationSpeedAdjustmentEnabled = true;

	private readonly BehaviorValues[] _behaviorValues;

	private bool _hasNewBehaviorValues;

	private readonly GameEntity[] _tempPickableEntities = new GameEntity[16];

	private readonly UIntPtr[] _pickableItemsId = new UIntPtr[16];

	private SpawnedItemEntity _itemToPickUp;

	private readonly MissionTimer _itemPickUpTickTimer;

	private bool _disablePickUpForAgent;

	private readonly MissionTimer _mountSearchTimer;

	private UsableMissionObject _objectOfInterest;

	private UsableObjectInterestKind _objectInterestKind;

	private bool _shouldCatchUpWithFormation;

	public Agent FollowedAgent { get; private set; }

	public bool ShouldCatchUpWithFormation
	{
		get
		{
			return _shouldCatchUpWithFormation;
		}
		private set
		{
			if (_shouldCatchUpWithFormation != value)
			{
				_shouldCatchUpWithFormation = value;
				Agent.SetShouldCatchUpWithFormation(value);
			}
		}
	}

	public bool IsDefending => _objectInterestKind == UsableObjectInterestKind.Defending;

	public HumanAIComponent(Agent agent)
		: base(agent)
	{
		_behaviorValues = new BehaviorValues[7];
		SetBehaviorValueSet(BehaviorValueSet.Default);
		Agent.SetAllBehaviorParams(_behaviorValues);
		_hasNewBehaviorValues = false;
		Agent agent2 = Agent;
		agent2.OnAgentWieldedItemChange = (Action)Delegate.Combine(agent2.OnAgentWieldedItemChange, new Action(DisablePickUpForAgentIfNeeded));
		Agent agent3 = Agent;
		agent3.OnAgentMountedStateChanged = (Action)Delegate.Combine(agent3.OnAgentMountedStateChanged, new Action(DisablePickUpForAgentIfNeeded));
		_itemPickUpTickTimer = new MissionTimer(2.5f + MBRandom.RandomFloat);
		_mountSearchTimer = new MissionTimer(2f + MBRandom.RandomFloat);
	}

	public void SetBehaviorParams(AISimpleBehaviorKind behavior, float y1, float x2, float y2, float x3, float y3)
	{
		_behaviorValues[(int)behavior].y1 = y1;
		_behaviorValues[(int)behavior].x2 = x2;
		_behaviorValues[(int)behavior].y2 = y2;
		_behaviorValues[(int)behavior].x3 = x3;
		_behaviorValues[(int)behavior].y3 = y3;
		_hasNewBehaviorValues = true;
	}

	public void SyncBehaviorParamsIfNecessary()
	{
		if (_hasNewBehaviorValues)
		{
			Agent.SetAllBehaviorParams(_behaviorValues);
			_hasNewBehaviorValues = false;
		}
	}

	public void DisablePickUpForAgentIfNeeded()
	{
		_disablePickUpForAgent = true;
		if (Agent.MountAgent == null)
		{
			if (Agent.HasLostShield())
			{
				_disablePickUpForAgent = false;
			}
			else
			{
				for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.ExtraWeaponSlot; equipmentIndex++)
				{
					MissionWeapon missionWeapon = Agent.Equipment[equipmentIndex];
					if (!missionWeapon.IsEmpty && missionWeapon.IsAnyConsumable())
					{
						_disablePickUpForAgent = false;
						break;
					}
				}
			}
		}
		if (_disablePickUpForAgent && Agent.Formation != null && MissionGameModels.Current.BattleBannerBearersModel.IsBannerSearchingAgent(Agent))
		{
			_disablePickUpForAgent = false;
		}
	}

	public override void OnTickAsAI(float dt)
	{
		SyncBehaviorParamsIfNecessary();
		if (_itemToPickUp != null)
		{
			if (!_itemToPickUp.IsAIMovingTo(Agent) || Agent.Mission.MissionEnded)
			{
				_itemToPickUp = null;
			}
			else if (_itemToPickUp.GameEntity == null)
			{
				Agent.StopUsingGameObject(isSuccessful: false);
			}
		}
		if (_itemPickUpTickTimer.Check(reset: true) && !Agent.Mission.MissionEnded)
		{
			EquipmentIndex wieldedItemIndex = Agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
			bool flag = ((wieldedItemIndex == EquipmentIndex.None) ? null : Agent.Equipment[wieldedItemIndex].CurrentUsageItem)?.IsRangedWeapon ?? false;
			if (!_disablePickUpForAgent && MissionGameModels.Current.ItemPickupModel.IsAgentEquipmentSuitableForPickUpAvailability(Agent) && Agent.CanBeAssignedForScriptedMovement() && Agent.CurrentWatchState == Agent.WatchState.Alarmed && (Agent.GetAgentFlags() & AgentFlag.CanAttack) != 0 && !IsInImportantCombatAction())
			{
				Agent targetAgent = Agent.GetTargetAgent();
				if (targetAgent == null || (targetAgent.Position.DistanceSquared(Agent.Position) > 400f && (!flag || IsAnyConsumableDepleted() || targetAgent.Position.DistanceSquared(Agent.Position) >= Agent.GetMissileRange() * 1.2f || Agent.GetLastTargetVisibilityState() != AITargetVisibilityState.TargetIsClear)))
				{
					float maximumForwardUnlimitedSpeed = Agent.MaximumForwardUnlimitedSpeed;
					if (_itemToPickUp == null)
					{
						Vec3 bMin = Agent.Position - new Vec3(maximumForwardUnlimitedSpeed, maximumForwardUnlimitedSpeed, 1f);
						Vec3 bMax = Agent.Position + new Vec3(maximumForwardUnlimitedSpeed, maximumForwardUnlimitedSpeed, 1.8f);
						_itemToPickUp = SelectPickableItem(bMin, bMax);
						if (_itemToPickUp != null)
						{
							RequestMoveToItem(_itemToPickUp);
						}
					}
				}
			}
		}
		if (_itemToPickUp != null && !Agent.IsRunningAway && Agent.AIMoveToGameObjectIsEnabled())
		{
			float num = (_itemToPickUp.IsBanner() ? MissionGameModels.Current.BattleBannerBearersModel.GetBannerInteractionDistance(Agent) : MissionGameModels.Current.AgentStatCalculateModel.GetInteractionDistance(Agent));
			num *= 3f;
			WorldFrame userFrameForAgent = _itemToPickUp.GetUserFrameForAgent(Agent);
			ref WorldPosition origin = ref userFrameForAgent.Origin;
			Vec3 targetPoint = Agent.Position;
			float distanceSq = origin.DistanceSquaredWithLimit(in targetPoint, num * num + 1E-05f);
			if (Agent.CanReachAndUseObject(_itemToPickUp, distanceSq))
			{
				Agent.UseGameObject(_itemToPickUp);
			}
		}
		if (Agent.CommonAIComponent == null || Agent.MountAgent != null || Agent.CommonAIComponent.IsRetreating || !_mountSearchTimer.Check(reset: true) || Agent.GetRidingOrder() != 1)
		{
			return;
		}
		Agent agent = FindReservedMount();
		if (agent == null || agent.State != AgentState.Active || agent.RiderAgent != null || !(Agent.Position.DistanceSquared(agent.Position) < 256f))
		{
			if (agent != null)
			{
				UnreserveMount(agent);
			}
			Agent agent2 = FindClosestMountAvailable();
			if (agent2 != null)
			{
				ReserveMount(agent2);
			}
		}
	}

	private Agent FindClosestMountAvailable()
	{
		float num = 6400f;
		Agent result = null;
		foreach (KeyValuePair<Agent, MissionTime> mountsWithoutRider in Mission.Current.MountsWithoutRiders)
		{
			Agent key = mountsWithoutRider.Key;
			if (key.IsActive() && key.CommonAIComponent.ReservedRiderAgentIndex < 0 && key.RiderAgent == null && !key.IsRunningAway && MissionGameModels.Current.AgentStatCalculateModel.CanAgentRideMount(Agent, key))
			{
				float num2 = Agent.Position.DistanceSquared(key.Position);
				if (num > num2)
				{
					result = key;
					num = num2;
				}
			}
		}
		return result;
	}

	private Agent FindReservedMount()
	{
		Agent result = null;
		int selectedMountIndex = Agent.GetSelectedMountIndex();
		if (selectedMountIndex >= 0)
		{
			foreach (KeyValuePair<Agent, MissionTime> mountsWithoutRider in Mission.Current.MountsWithoutRiders)
			{
				Agent key = mountsWithoutRider.Key;
				if (key.Index == selectedMountIndex)
				{
					result = key;
					break;
				}
			}
		}
		return result;
	}

	internal void ReserveMount(Agent mount)
	{
		Agent.SetSelectedMountIndex(mount.Index);
		mount.CommonAIComponent.OnMountReserved(Agent.Index);
	}

	internal void UnreserveMount(Agent mount)
	{
		Agent.SetSelectedMountIndex(-1);
		mount.CommonAIComponent.OnMountUnreserved();
	}

	public override void OnAgentRemoved()
	{
		Agent agent = FindReservedMount();
		if (agent != null)
		{
			UnreserveMount(agent);
		}
	}

	public override void OnComponentRemoved()
	{
		Agent agent = FindReservedMount();
		if (agent != null)
		{
			UnreserveMount(agent);
		}
	}

	public bool IsInImportantCombatAction()
	{
		Agent.ActionCodeType currentActionType = Agent.GetCurrentActionType(1);
		if (currentActionType != Agent.ActionCodeType.ReadyMelee && currentActionType != Agent.ActionCodeType.ReadyRanged && currentActionType != Agent.ActionCodeType.ReleaseMelee && currentActionType != Agent.ActionCodeType.ReleaseRanged && currentActionType != Agent.ActionCodeType.ReleaseThrowing)
		{
			return currentActionType == Agent.ActionCodeType.DefendShield;
		}
		return true;
	}

	private bool IsAnyConsumableDepleted()
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.ExtraWeaponSlot; equipmentIndex++)
		{
			MissionWeapon missionWeapon = Agent.Equipment[equipmentIndex];
			if (!missionWeapon.IsEmpty && missionWeapon.IsAnyConsumable() && missionWeapon.Amount == 0)
			{
				return true;
			}
		}
		return false;
	}

	private SpawnedItemEntity SelectPickableItem(Vec3 bMin, Vec3 bMax)
	{
		Agent targetAgent = Agent.GetTargetAgent();
		Vec3 v = ((targetAgent == null) ? Vec3.Invalid : (targetAgent.Position - Agent.Position));
		int num = Agent.Mission.Scene.SelectEntitiesInBoxWithScriptComponent<SpawnedItemEntity>(ref bMin, ref bMax, _tempPickableEntities, _pickableItemsId);
		float num2 = 0f;
		SpawnedItemEntity result = null;
		for (int i = 0; i < num; i++)
		{
			SpawnedItemEntity firstScriptOfType = _tempPickableEntities[i].GetFirstScriptOfType<SpawnedItemEntity>();
			bool flag = false;
			if (firstScriptOfType != null)
			{
				MissionWeapon weaponCopy = firstScriptOfType.WeaponCopy;
				flag = !weaponCopy.IsEmpty && (weaponCopy.IsShield() || weaponCopy.IsBanner() || firstScriptOfType.IsStuckMissile() || firstScriptOfType.IsQuiverAndNotEmpty());
			}
			if (!flag || firstScriptOfType.HasUser || (firstScriptOfType.HasAIMovingTo && !firstScriptOfType.IsAIMovingTo(Agent)) || !(firstScriptOfType.GameEntityWithWorldPosition.WorldPosition.GetNavMesh() != UIntPtr.Zero))
			{
				continue;
			}
			Vec3 v2 = firstScriptOfType.GetUserFrameForAgent(Agent).Origin.GetGroundVec3() - Agent.Position;
			float z = v2.z;
			v2.Normalize();
			if (targetAgent != null && !(v.Length - Vec3.DotProduct(v, v2) > targetAgent.MaximumForwardUnlimitedSpeed * 3f))
			{
				continue;
			}
			EquipmentIndex equipmentIndex = MissionEquipment.SelectWeaponPickUpSlot(Agent, firstScriptOfType.WeaponCopy, firstScriptOfType.IsStuckMissile());
			WorldPosition worldPosition = firstScriptOfType.GameEntityWithWorldPosition.WorldPosition;
			if (equipmentIndex == EquipmentIndex.None || !(worldPosition.GetNavMesh() != UIntPtr.Zero) || !MissionGameModels.Current.ItemPickupModel.IsItemAvailableForAgent(firstScriptOfType, Agent, equipmentIndex))
			{
				continue;
			}
			Agent agent = Agent;
			Vec2 position = worldPosition.AsVec2;
			if (agent.CanMoveDirectlyToPosition(in position) && (!Agent.Mission.IsPositionInsideAnyBlockerNavMeshFace2D(worldPosition.AsVec2) || !(TaleWorlds.Library.MathF.Abs(z) < 1.5f)))
			{
				float itemScoreForAgent = MissionGameModels.Current.ItemPickupModel.GetItemScoreForAgent(firstScriptOfType, Agent);
				if (itemScoreForAgent > num2)
				{
					result = firstScriptOfType;
					num2 = itemScoreForAgent;
				}
			}
		}
		return result;
	}

	internal void ItemPickupDone(SpawnedItemEntity spawnedItemEntity)
	{
		_itemToPickUp = null;
	}

	private void RequestMoveToItem(SpawnedItemEntity item)
	{
		item.MovingAgent?.StopUsingGameObject(isSuccessful: false);
		MoveToUsableGameObject(item, null);
	}

	public UsableMissionObject GetCurrentlyMovingGameObject()
	{
		return _objectOfInterest;
	}

	private void SetCurrentlyMovingGameObject(UsableMissionObject objectOfInterest)
	{
		_objectOfInterest = objectOfInterest;
		_objectInterestKind = ((_objectOfInterest != null) ? UsableObjectInterestKind.MovingTo : UsableObjectInterestKind.None);
	}

	public UsableMissionObject GetCurrentlyDefendingGameObject()
	{
		return _objectOfInterest;
	}

	private void SetCurrentlyDefendingGameObject(UsableMissionObject objectOfInterest)
	{
		_objectOfInterest = objectOfInterest;
		_objectInterestKind = ((_objectOfInterest != null) ? UsableObjectInterestKind.Defending : UsableObjectInterestKind.None);
	}

	public void MoveToUsableGameObject(UsableMissionObject usedObject, IDetachment detachment, Agent.AIScriptedFrameFlags scriptedFrameFlags = Agent.AIScriptedFrameFlags.NoAttack)
	{
		Agent.AIStateFlags |= Agent.AIStateFlag.UseObjectMoving;
		SetCurrentlyMovingGameObject(usedObject);
		usedObject.OnAIMoveToUse(Agent, detachment);
		WorldFrame userFrameForAgent = usedObject.GetUserFrameForAgent(Agent);
		Agent.SetScriptedPositionAndDirection(ref userFrameForAgent.Origin, userFrameForAgent.Rotation.f.AsVec2.RotationInRadians, addHumanLikeDelay: false, scriptedFrameFlags);
	}

	public void MoveToClear()
	{
		GetCurrentlyMovingGameObject()?.OnMoveToStopped(Agent);
		SetCurrentlyMovingGameObject(null);
		Agent.AIStateFlags &= ~Agent.AIStateFlag.UseObjectMoving;
	}

	public void StartDefendingGameObject(UsableMissionObject usedObject, IDetachment detachment)
	{
		SetCurrentlyDefendingGameObject(usedObject);
		usedObject.OnAIDefendBegin(Agent, detachment);
	}

	public void StopDefendingGameObject()
	{
		GetCurrentlyDefendingGameObject().OnAIDefendEnd(Agent);
		SetCurrentlyDefendingGameObject(null);
	}

	public bool IsInterestedInAnyGameObject()
	{
		return _objectInterestKind != UsableObjectInterestKind.None;
	}

	public bool IsInterestedInGameObject(UsableMissionObject usableMissionObject)
	{
		bool result = false;
		switch (_objectInterestKind)
		{
		case UsableObjectInterestKind.MovingTo:
			result = usableMissionObject == GetCurrentlyMovingGameObject();
			break;
		case UsableObjectInterestKind.Defending:
			result = usableMissionObject == GetCurrentlyDefendingGameObject();
			break;
		default:
			Debug.FailedAssert("Unexpected object interest kind: " + _objectInterestKind, "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\AgentComponents\\HumanAIComponent.cs", "IsInterestedInGameObject", 580);
			break;
		case UsableObjectInterestKind.None:
			break;
		}
		return result;
	}

	public void FollowAgent(Agent agent)
	{
		FollowedAgent = agent;
	}

	public float GetDesiredSpeedInFormation(bool isCharging)
	{
		if (ShouldCatchUpWithFormation && (!isCharging || !Mission.Current.IsMissionEnding))
		{
			float num = Agent.MountAgent?.MaximumForwardUnlimitedSpeed ?? Agent.MaximumForwardUnlimitedSpeed;
			bool flag = !isCharging;
			if (isCharging)
			{
				FormationQuerySystem closestEnemyFormation = Agent.Formation.QuerySystem.ClosestEnemyFormation;
				float num2 = float.MaxValue;
				float num3 = 4f * num * num;
				if (closestEnemyFormation != null)
				{
					WorldPosition medianPosition = Agent.Formation.QuerySystem.MedianPosition;
					WorldPosition medianPosition2 = closestEnemyFormation.MedianPosition;
					num2 = medianPosition.AsVec2.DistanceSquared(medianPosition2.AsVec2);
					if (num2 <= num3)
					{
						num2 = Agent.Formation.QuerySystem.MedianPosition.GetNavMeshVec3().DistanceSquared(closestEnemyFormation.MedianPosition.GetNavMeshVec3());
					}
				}
				flag = num2 > num3;
			}
			if (flag)
			{
				Vec2 v = Agent.Formation.GetCurrentGlobalPositionOfUnit(Agent, blendWithOrderDirection: true) - Agent.Position.AsVec2;
				float value = 0f - Agent.GetMovementDirection().DotProduct(v);
				value = TaleWorlds.Library.MathF.Clamp(value, 0f, 100f);
				float num4 = ((Agent.MountAgent != null) ? 4f : 2f);
				float num5 = (isCharging ? Agent.Formation.QuerySystem.FormationIntegrityData.AverageMaxUnlimitedSpeedExcludeFarAgents : Agent.Formation.QuerySystem.MovementSpeed) / num;
				return TaleWorlds.Library.MathF.Clamp((0.7f + 0.4f * ((num - value * num4) / (num + value * num4))) * num5, 0.3f, 1f);
			}
		}
		return 1f;
	}

	private bool GetFormationFrame(out WorldPosition formationPosition, out Vec2 formationDirection, out float speedLimit, out bool isSettingDestinationSpeed, out bool limitIsMultiplier, bool finalDestination = false)
	{
		Formation formation = Agent.Formation;
		isSettingDestinationSpeed = false;
		limitIsMultiplier = false;
		bool result = false;
		if (formation != null)
		{
			formationPosition = formation.GetOrderPositionOfUnit(Agent);
			formationDirection = formation.GetDirectionOfUnit(Agent);
		}
		else
		{
			formationPosition = WorldPosition.Invalid;
			formationDirection = Vec2.Invalid;
		}
		if (FormationSpeedAdjustmentEnabled && Agent.IsMount)
		{
			formationPosition = WorldPosition.Invalid;
			formationDirection = Vec2.Invalid;
			if (Agent.RiderAgent == null || (Agent.RiderAgent != null && (!Agent.RiderAgent.IsActive() || Agent.RiderAgent.Formation == null)))
			{
				speedLimit = -1f;
			}
			else
			{
				limitIsMultiplier = true;
				speedLimit = Agent.RiderAgent.HumanAIComponent.GetDesiredSpeedInFormation(formation.GetReadonlyMovementOrderReference().MovementState == MovementOrder.MovementStateEnum.Charge);
			}
		}
		else if (formation == null)
		{
			speedLimit = -1f;
		}
		else if (Agent.IsDetachedFromFormation)
		{
			speedLimit = -1f;
			WorldFrame? worldFrame = null;
			if (formation.GetReadonlyMovementOrderReference().MovementState != 0 || (Agent.Detachment != null && (!Agent.Detachment.IsLoose || formationPosition.IsValid)))
			{
				worldFrame = formation.GetDetachmentFrame(Agent);
			}
			if (worldFrame.HasValue)
			{
				formationDirection = worldFrame.Value.Rotation.f.AsVec2.Normalized();
				result = true;
			}
			else
			{
				formationDirection = Vec2.Invalid;
			}
		}
		else
		{
			switch (formation.GetReadonlyMovementOrderReference().MovementState)
			{
			case MovementOrder.MovementStateEnum.Hold:
				isSettingDestinationSpeed = true;
				if (FormationSpeedAdjustmentEnabled && ShouldCatchUpWithFormation)
				{
					limitIsMultiplier = true;
					speedLimit = GetDesiredSpeedInFormation(isCharging: false);
				}
				else
				{
					speedLimit = -1f;
				}
				result = true;
				break;
			case MovementOrder.MovementStateEnum.StandGround:
				formationDirection = Agent.Frame.rotation.f.AsVec2;
				speedLimit = -1f;
				result = true;
				break;
			case MovementOrder.MovementStateEnum.Charge:
				limitIsMultiplier = true;
				speedLimit = (FormationSpeedAdjustmentEnabled ? GetDesiredSpeedInFormation(isCharging: true) : (-1f));
				result = formationPosition.IsValid;
				break;
			case MovementOrder.MovementStateEnum.Retreat:
				speedLimit = -1f;
				break;
			default:
				Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\AgentComponents\\HumanAIComponent.cs", "GetFormationFrame", 767);
				speedLimit = -1f;
				break;
			}
		}
		return result;
	}

	public void AdjustSpeedLimit(Agent agent, float desiredSpeed, bool limitIsMultiplier)
	{
		if (agent.MissionPeer != null)
		{
			desiredSpeed = -1f;
		}
		Agent.SetMaximumSpeedLimit(desiredSpeed, limitIsMultiplier);
		agent.MountAgent?.SetMaximumSpeedLimit(desiredSpeed, limitIsMultiplier);
	}

	public void UpdateFormationMovement()
	{
		WorldPosition formationPosition;
		Vec2 formationDirection;
		float speedLimit;
		bool isSettingDestinationSpeed;
		bool limitIsMultiplier;
		bool formationFrame = GetFormationFrame(out formationPosition, out formationDirection, out speedLimit, out isSettingDestinationSpeed, out limitIsMultiplier);
		AdjustSpeedLimit(Agent, speedLimit, limitIsMultiplier);
		if (Agent.Controller == Agent.ControllerType.AI && Agent.Formation != null && Agent.Formation.GetReadonlyMovementOrderReference().OrderEnum != MovementOrder.MovementOrderEnum.Stop && Agent.Formation.GetReadonlyMovementOrderReference().OrderEnum != MovementOrder.MovementOrderEnum.Retreat && !Agent.IsRetreating())
		{
			FormationQuerySystem.FormationIntegrityDataGroup formationIntegrityData = Agent.Formation.QuerySystem.FormationIntegrityData;
			float num = formationIntegrityData.AverageMaxUnlimitedSpeedExcludeFarAgents * 3f;
			if (formationIntegrityData.DeviationOfPositionsExcludeFarAgents > num * 100f)
			{
				ShouldCatchUpWithFormation = false;
				Agent.SetFormationIntegrityData(Vec2.Zero, Vec2.Zero, Vec2.Zero, 0f, 0f);
			}
			else
			{
				Vec2 currentGlobalPositionOfUnit = Agent.Formation.GetCurrentGlobalPositionOfUnit(Agent, blendWithOrderDirection: true);
				float num2 = Agent.Position.AsVec2.Distance(currentGlobalPositionOfUnit);
				ShouldCatchUpWithFormation = num2 < num * 2f;
				Agent.SetFormationIntegrityData(ShouldCatchUpWithFormation ? currentGlobalPositionOfUnit : Vec2.Zero, Agent.Formation.CurrentDirection, formationIntegrityData.AverageVelocityExcludeFarAgents, formationIntegrityData.AverageMaxUnlimitedSpeedExcludeFarAgents, formationIntegrityData.DeviationOfPositionsExcludeFarAgents);
			}
		}
		else
		{
			ShouldCatchUpWithFormation = false;
		}
		bool flag = formationPosition.IsValid;
		if (!formationFrame || !flag)
		{
			Agent.SetFormationFrameDisabled();
			return;
		}
		if (!GameNetwork.IsMultiplayer && Agent.Mission.Mode == MissionMode.Deployment)
		{
			MBSceneUtilities.ProjectPositionToDeploymentBoundaries(Agent.Formation.Team.Side, ref formationPosition);
			flag = Agent.Mission.IsFormationUnitPositionAvailable(ref formationPosition, Agent.Team);
		}
		if (flag)
		{
			Agent.SetFormationFrameEnabled(formationPosition, formationDirection, Agent.Formation.GetReadonlyMovementOrderReference().GetTargetVelocity(), Agent.Formation.CalculateFormationDirectionEnforcingFactorForRank(((IFormationUnit)Agent).FormationRankIndex));
			float directionChangeTendency = 1f;
			if (Agent.Formation.ArrangementOrder.OrderEnum == ArrangementOrder.ArrangementOrderEnum.ShieldWall && !Agent.IsDetachedFromFormation)
			{
				directionChangeTendency = Agent.Formation.Arrangement.GetDirectionChangeTendencyOfUnit(Agent);
			}
			Agent.SetDirectionChangeTendency(directionChangeTendency);
		}
		else
		{
			Agent.SetFormationFrameDisabled();
		}
	}

	public override void OnRetreating()
	{
		base.OnRetreating();
		AdjustSpeedLimit(Agent, -1f, limitIsMultiplier: false);
	}

	public override void OnDismount(Agent mount)
	{
		base.OnDismount(mount);
		mount.SetMaximumSpeedLimit(-1f, isMultiplier: false);
	}

	public override void OnMount(Agent mount)
	{
		base.OnMount(mount);
		int selectedMountIndex = Agent.GetSelectedMountIndex();
		if (selectedMountIndex >= 0 && selectedMountIndex != mount.Index)
		{
			Agent agent = Mission.Current.FindAgentWithIndex(selectedMountIndex);
			if (agent != null)
			{
				UnreserveMount(agent);
			}
		}
		int reservedRiderAgentIndex = mount.CommonAIComponent.ReservedRiderAgentIndex;
		if (reservedRiderAgentIndex >= 0)
		{
			if (reservedRiderAgentIndex == Agent.Index)
			{
				UnreserveMount(mount);
			}
			else
			{
				Mission.Current.FindAgentWithIndex(reservedRiderAgentIndex)?.HumanAIComponent.UnreserveMount(mount);
			}
		}
	}

	public void SetBehaviorValueSet(BehaviorValueSet behaviorValueSet)
	{
		switch (behaviorValueSet)
		{
		case BehaviorValueSet.Default:
			SetBehaviorParams(AISimpleBehaviorKind.GoToPos, 3f, 7f, 5f, 20f, 6f);
			SetBehaviorParams(AISimpleBehaviorKind.Melee, 8f, 7f, 4f, 20f, 1f);
			SetBehaviorParams(AISimpleBehaviorKind.Ranged, 2f, 7f, 4f, 20f, 5f);
			SetBehaviorParams(AISimpleBehaviorKind.ChargeHorseback, 2f, 25f, 5f, 30f, 5f);
			SetBehaviorParams(AISimpleBehaviorKind.RangedHorseback, 2f, 15f, 6.5f, 30f, 5.5f);
			SetBehaviorParams(AISimpleBehaviorKind.AttackEntityMelee, 5f, 12f, 7.5f, 30f, 4f);
			SetBehaviorParams(AISimpleBehaviorKind.AttackEntityRanged, 5.5f, 12f, 8f, 30f, 4.5f);
			break;
		case BehaviorValueSet.DefensiveArrangementMove:
			SetBehaviorParams(AISimpleBehaviorKind.GoToPos, 3f, 8f, 5f, 20f, 6f);
			SetBehaviorParams(AISimpleBehaviorKind.Melee, 4f, 5f, 0f, 20f, 0f);
			SetBehaviorParams(AISimpleBehaviorKind.Ranged, 0f, 7f, 0f, 20f, 0f);
			SetBehaviorParams(AISimpleBehaviorKind.ChargeHorseback, 0f, 7f, 0f, 30f, 0f);
			SetBehaviorParams(AISimpleBehaviorKind.RangedHorseback, 0f, 15f, 0f, 30f, 0f);
			SetBehaviorParams(AISimpleBehaviorKind.AttackEntityMelee, 5f, 12f, 7.5f, 30f, 9f);
			SetBehaviorParams(AISimpleBehaviorKind.AttackEntityRanged, 0.55f, 12f, 0.8f, 30f, 0.45f);
			break;
		case BehaviorValueSet.Follow:
			SetBehaviorParams(AISimpleBehaviorKind.GoToPos, 3f, 7f, 5f, 20f, 6f);
			SetBehaviorParams(AISimpleBehaviorKind.Melee, 6f, 7f, 4f, 20f, 0f);
			SetBehaviorParams(AISimpleBehaviorKind.Ranged, 0f, 7f, 0f, 20f, 0f);
			SetBehaviorParams(AISimpleBehaviorKind.ChargeHorseback, 0f, 7f, 0f, 30f, 0f);
			SetBehaviorParams(AISimpleBehaviorKind.RangedHorseback, 0f, 15f, 0f, 30f, 0f);
			SetBehaviorParams(AISimpleBehaviorKind.AttackEntityMelee, 5f, 12f, 7.5f, 30f, 9f);
			SetBehaviorParams(AISimpleBehaviorKind.AttackEntityRanged, 0.55f, 12f, 0.8f, 30f, 0.45f);
			break;
		case BehaviorValueSet.DefaultMove:
			SetBehaviorParams(AISimpleBehaviorKind.GoToPos, 3f, 7f, 5f, 20f, 6f);
			SetBehaviorParams(AISimpleBehaviorKind.Melee, 8f, 7f, 5f, 20f, 0.01f);
			SetBehaviorParams(AISimpleBehaviorKind.Ranged, 0.02f, 7f, 0.04f, 20f, 0.03f);
			SetBehaviorParams(AISimpleBehaviorKind.ChargeHorseback, 10f, 7f, 5f, 30f, 0.05f);
			SetBehaviorParams(AISimpleBehaviorKind.RangedHorseback, 0.02f, 15f, 0.065f, 30f, 0.055f);
			SetBehaviorParams(AISimpleBehaviorKind.AttackEntityMelee, 5f, 12f, 7.5f, 30f, 9f);
			SetBehaviorParams(AISimpleBehaviorKind.AttackEntityRanged, 0.55f, 12f, 0.8f, 30f, 0.45f);
			break;
		case BehaviorValueSet.Charge:
			SetBehaviorParams(AISimpleBehaviorKind.GoToPos, 3f, 7f, 5f, 20f, 6f);
			SetBehaviorParams(AISimpleBehaviorKind.Melee, 8f, 7f, 4f, 20f, 1f);
			SetBehaviorParams(AISimpleBehaviorKind.Ranged, 2f, 7f, 4f, 20f, 5f);
			SetBehaviorParams(AISimpleBehaviorKind.ChargeHorseback, 2f, 25f, 5f, 30f, 5f);
			SetBehaviorParams(AISimpleBehaviorKind.RangedHorseback, 0f, 10f, 3f, 20f, 6f);
			SetBehaviorParams(AISimpleBehaviorKind.AttackEntityMelee, 5f, 12f, 7.5f, 30f, 9f);
			SetBehaviorParams(AISimpleBehaviorKind.AttackEntityRanged, 0.55f, 12f, 0.8f, 30f, 0.45f);
			break;
		case BehaviorValueSet.DefaultDetached:
			SetBehaviorParams(AISimpleBehaviorKind.GoToPos, 3f, 7f, 5f, 20f, 6f);
			SetBehaviorParams(AISimpleBehaviorKind.Melee, 8f, 7f, 4f, 20f, 1f);
			SetBehaviorParams(AISimpleBehaviorKind.Ranged, 0.2f, 7f, 0.4f, 20f, 0.5f);
			SetBehaviorParams(AISimpleBehaviorKind.ChargeHorseback, 2f, 25f, 5f, 30f, 5f);
			SetBehaviorParams(AISimpleBehaviorKind.RangedHorseback, 0.2f, 15f, 0.65f, 30f, 0.55f);
			SetBehaviorParams(AISimpleBehaviorKind.AttackEntityMelee, 5f, 12f, 7.5f, 30f, 4f);
			SetBehaviorParams(AISimpleBehaviorKind.AttackEntityRanged, 5.5f, 12f, 8f, 30f, 4.5f);
			break;
		}
	}

	public void RefreshBehaviorValues(MovementOrder.MovementOrderEnum movementOrder, ArrangementOrder.ArrangementOrderEnum arrangementOrder)
	{
		switch (movementOrder)
		{
		case MovementOrder.MovementOrderEnum.Charge:
		case MovementOrder.MovementOrderEnum.ChargeToTarget:
			SetBehaviorValueSet(BehaviorValueSet.Charge);
			return;
		default:
			switch (arrangementOrder)
			{
			case ArrangementOrder.ArrangementOrderEnum.Column:
				break;
			default:
				SetBehaviorValueSet(BehaviorValueSet.DefaultMove);
				return;
			case ArrangementOrder.ArrangementOrderEnum.Circle:
			case ArrangementOrder.ArrangementOrderEnum.ShieldWall:
			case ArrangementOrder.ArrangementOrderEnum.Square:
				SetBehaviorValueSet(BehaviorValueSet.DefensiveArrangementMove);
				return;
			}
			break;
		case MovementOrder.MovementOrderEnum.Follow:
			break;
		}
		SetBehaviorValueSet(BehaviorValueSet.Follow);
	}
}
