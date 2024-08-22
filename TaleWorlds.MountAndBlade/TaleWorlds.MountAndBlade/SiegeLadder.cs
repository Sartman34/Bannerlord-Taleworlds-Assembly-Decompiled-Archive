using System;
using System.Collections.Generic;
using System.Linq;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.Objects.Siege;

namespace TaleWorlds.MountAndBlade;

public class SiegeLadder : SiegeWeapon, IPrimarySiegeWeapon, IOrderableWithInteractionArea, IOrderable, ISpawnable
{
	[DefineSynchedMissionObjectType(typeof(SiegeLadder))]
	public struct SiegeLadderRecord : ISynchedMissionObjectReadableRecord
	{
		public bool IsStateLand { get; private set; }

		public int State { get; private set; }

		public int AnimationState { get; private set; }

		public float FallAngularSpeed { get; private set; }

		public MatrixFrame LadderFrame { get; private set; }

		public bool HasAnimation { get; private set; }

		public int LadderAnimationIndex { get; private set; }

		public float LadderAnimationProgress { get; private set; }

		public bool ReadFromNetwork(ref bool bufferReadValid)
		{
			IsStateLand = GameNetworkMessage.ReadBoolFromPacket(ref bufferReadValid);
			State = GameNetworkMessage.ReadIntFromPacket(CompressionMission.SiegeLadderStateCompressionInfo, ref bufferReadValid);
			AnimationState = GameNetworkMessage.ReadIntFromPacket(CompressionMission.SiegeLadderAnimationStateCompressionInfo, ref bufferReadValid);
			FallAngularSpeed = GameNetworkMessage.ReadFloatFromPacket(CompressionMission.SiegeMachineComponentAngularSpeedCompressionInfo, ref bufferReadValid);
			LadderFrame = GameNetworkMessage.ReadMatrixFrameFromPacket(ref bufferReadValid);
			HasAnimation = GameNetworkMessage.ReadBoolFromPacket(ref bufferReadValid);
			LadderAnimationIndex = -1;
			LadderAnimationProgress = 0f;
			if (HasAnimation)
			{
				LadderAnimationIndex = GameNetworkMessage.ReadIntFromPacket(CompressionBasic.AnimationIndexCompressionInfo, ref bufferReadValid);
				LadderAnimationProgress = GameNetworkMessage.ReadFloatFromPacket(CompressionBasic.AnimationProgressCompressionInfo, ref bufferReadValid);
			}
			return bufferReadValid;
		}
	}

	public enum LadderState
	{
		OnLand,
		FallToLand,
		BeingRaised,
		BeingRaisedStartFromGround,
		BeingRaisedStopped,
		OnWall,
		FallToWall,
		BeingPushedBack,
		BeingPushedBackStartFromWall,
		BeingPushedBackStopped,
		NumberOfStates
	}

	public enum LadderAnimationState
	{
		Static,
		Animated,
		PhysicallyDynamic,
		NumberOfStates
	}

	private static readonly ActionIndexCache act_usage_ladder_lift_from_left_1_start = ActionIndexCache.Create("act_usage_ladder_lift_from_left_1_start");

	private static readonly ActionIndexCache act_usage_ladder_lift_from_left_2_start = ActionIndexCache.Create("act_usage_ladder_lift_from_left_2_start");

	public const float ClimbingLimitRadian = -0.20135832f;

	private static readonly ActionIndexCache act_usage_ladder_lift_from_right_1_start = ActionIndexCache.Create("act_usage_ladder_lift_from_right_1_start");

	public const float ClimbingLimitDegree = -11.536982f;

	private static readonly ActionIndexCache act_usage_ladder_lift_from_right_2_start = ActionIndexCache.Create("act_usage_ladder_lift_from_right_2_start");

	public const float AutomaticUseActivationRange = 20f;

	private static readonly ActionIndexCache act_usage_ladder_pick_up_fork_begin = ActionIndexCache.Create("act_usage_ladder_pick_up_fork_begin");

	private static readonly ActionIndexCache act_usage_ladder_pick_up_fork_end = ActionIndexCache.Create("act_usage_ladder_pick_up_fork_end");

	private static readonly ActionIndexCache act_usage_ladder_push_back = ActionIndexCache.Create("act_usage_ladder_push_back");

	private static readonly ActionIndexCache act_usage_ladder_push_back_stopped = ActionIndexCache.Create("act_usage_ladder_push_back_stopped");

	public string AttackerTag = "attacker";

	public string DefenderTag = "defender";

	public string downStateEntityTag = "ladderDown";

	public string IdleAnimation = "siege_ladder_idle";

	public int _idleAnimationIndex = -1;

	public string RaiseAnimation = "siege_ladder_rise";

	public string RaiseAnimationWithoutRootBone = "siege_ladder_rise_wo_rootbone";

	public int _raiseAnimationWithoutRootBoneIndex = -1;

	public string PushBackAnimation = "siege_ladder_push_back";

	public int _pushBackAnimationIndex = -1;

	public string PushBackAnimationWithoutRootBone = "siege_ladder_push_back_wo_rootbone";

	public int _pushBackAnimationWithoutRootBoneIndex = -1;

	public string TrembleWallHeavyAnimation = "siege_ladder_stop_wall_heavy";

	public string TrembleWallLightAnimation = "siege_ladder_stop_wall_light";

	public string TrembleGroundAnimation = "siege_ladder_stop_ground_heavy";

	public string RightStandingPointTag = "right";

	public string LeftStandingPointTag = "left";

	public string FrontStandingPointTag = "front";

	public string PushForkItemID = "push_fork";

	public string upStateEntityTag = "ladderUp";

	public string BodyTag = "ladder_body";

	public string CollisionBodyTag = "ladder_collision_body";

	public string InitialWaitPositionTag = "initialwaitposition";

	private string _targetWallSegmentTag = "";

	public float LadderPushTreshold = 170f;

	public float LadderPushTresholdForOneAgent = 55f;

	private WallSegment _targetWallSegment;

	private string _sideTag;

	private int _trembleWallLightAnimationIndex = -1;

	public string BarrierTagToRemove = "barrier";

	private int _trembleGroundAnimationIndex = -1;

	public LadderState initialState;

	private int _trembleWallHeavyAnimationIndex = -1;

	public string IndestructibleMerlonsTag = string.Empty;

	private int _raiseAnimationIndex = -1;

	private bool _isNavigationMeshDisabled;

	private bool _isLadderPhysicsDisabled;

	private bool _isLadderCollisionPhysicsDisabled;

	private Timer _tickOccasionallyTimer;

	private float _upStateRotationRadian;

	private float _downStateRotationRadian;

	private float _fallAngularSpeed;

	private MatrixFrame _ladderDownFrame;

	private MatrixFrame _ladderUpFrame;

	private LadderAnimationState _animationState;

	private int _currentActionAgentCount;

	private LadderState _state;

	private List<GameEntity> _aiBarriers;

	private List<StandingPoint> _attackerStandingPoints;

	private StandingPointWithWeaponRequirement _pushingWithForkStandingPoint;

	private StandingPointWithWeaponRequirement _forkPickUpStandingPoint;

	private ItemObject _forkItem;

	private MatrixFrame[] _attackerStandingPointLocalIKFrames;

	private MatrixFrame _ladderInitialGlobalFrame;

	private SynchedMissionObject _ladderParticleObject;

	private SynchedMissionObject _ladderBodyObject;

	private SynchedMissionObject _ladderCollisionBodyObject;

	private SynchedMissionObject _ladderObject;

	private Skeleton _ladderSkeleton;

	private float _lastDotProductOfAnimationAndTargetRotation;

	private float _turningAngle;

	private LadderQueueManager _queueManagerForAttackers;

	private LadderQueueManager _queueManagerForDefenders;

	private Timer _forkReappearingTimer;

	private float _forkReappearingDelay = 10f;

	private SynchedMissionObject _forkEntity;

	public GameEntity InitialWaitPosition { get; private set; }

	public int OnWallNavMeshId { get; private set; }

	public int OverTheWallNavMeshID => 13;

	public LadderState State
	{
		get
		{
			return _state;
		}
		set
		{
			if (_state != value)
			{
				if (GameNetwork.IsServerOrRecorder)
				{
					GameNetwork.BeginBroadcastModuleEvent();
					GameNetwork.WriteMessage(new SetSiegeLadderState(base.Id, value));
					GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
				}
				_state = value;
				OnLadderStateChange();
				CalculateNavigationAndPhysics();
			}
		}
	}

	public MissionObject TargetCastlePosition => _targetWallSegment;

	public FormationAI.BehaviorSide WeaponSide { get; private set; }

	public float SiegeWeaponPriority => 8f;

	public bool HoldLadders => false;

	public bool SendLadders => State != LadderState.OnLand;

	public override SiegeEngineType GetSiegeEngineType()
	{
		return DefaultSiegeEngineTypes.Ladder;
	}

	protected internal override void OnInit()
	{
		base.OnInit();
		_tickOccasionallyTimer = new Timer(Mission.Current.CurrentTime, 0.2f + MBRandom.RandomFloat * 0.05f);
		_aiBarriers = base.Scene.FindEntitiesWithTag(BarrierTagToRemove).ToList();
		if (IndestructibleMerlonsTag != string.Empty)
		{
			foreach (GameEntity item in base.Scene.FindEntitiesWithTag(IndestructibleMerlonsTag))
			{
				DestructableComponent firstScriptOfType = item.GetFirstScriptOfType<DestructableComponent>();
				firstScriptOfType.SetDisabled();
				firstScriptOfType.CanBeDestroyedInitially = false;
			}
		}
		_attackerStandingPoints = base.GameEntity.CollectObjectsWithTag<StandingPoint>(AttackerTag);
		_pushingWithForkStandingPoint = base.GameEntity.CollectObjectsWithTag<StandingPointWithWeaponRequirement>(DefenderTag).FirstOrDefault();
		_pushingWithForkStandingPoint.AddComponent(new DropExtraWeaponOnStopUsageComponent());
		_forkPickUpStandingPoint = base.GameEntity.CollectObjectsWithTag<StandingPointWithWeaponRequirement>(AmmoPickUpTag).FirstOrDefault();
		_forkPickUpStandingPoint?.SetUsingBattleSide(BattleSideEnum.Defender);
		_ladderParticleObject = base.GameEntity.CollectObjectsWithTag<SynchedMissionObject>("particles").FirstOrDefault();
		_forkEntity = base.GameEntity.CollectObjectsWithTag<SynchedMissionObject>("push_fork").FirstOrDefault();
		if (base.StandingPoints != null)
		{
			foreach (StandingPoint standingPoint in base.StandingPoints)
			{
				if (!standingPoint.GameEntity.HasTag(AmmoPickUpTag))
				{
					standingPoint.AddComponent(new ResetAnimationOnStopUsageComponent(standingPoint.GameEntity.HasTag(DefenderTag) ? act_usage_ladder_push_back_stopped : ActionIndexCache.act_none));
					standingPoint.IsDeactivated = true;
				}
			}
		}
		_forkItem = Game.Current.ObjectManager.GetObject<ItemObject>(PushForkItemID);
		_pushingWithForkStandingPoint.InitRequiredWeapon(_forkItem);
		_forkPickUpStandingPoint.InitGivenWeapon(_forkItem);
		GameEntity gameEntity = base.GameEntity.CollectChildrenEntitiesWithTag(upStateEntityTag)[0];
		List<SynchedMissionObject> list = base.GameEntity.CollectObjectsWithTag<SynchedMissionObject>(downStateEntityTag);
		_ladderObject = list[0];
		_ladderSkeleton = _ladderObject.GameEntity.Skeleton;
		list = base.GameEntity.CollectObjectsWithTag<SynchedMissionObject>(BodyTag);
		_ladderBodyObject = list[0];
		list = base.GameEntity.CollectObjectsWithTag<SynchedMissionObject>(CollisionBodyTag);
		_ladderCollisionBodyObject = list[0];
		_ladderDownFrame = _ladderObject.GameEntity.GetFrame();
		_turningAngle = _downStateRotationRadian - _ladderDownFrame.rotation.GetEulerAngles().x;
		_ladderDownFrame.rotation.RotateAboutSide(_turningAngle);
		_ladderObject.GameEntity.SetFrame(ref _ladderDownFrame);
		MatrixFrame frame = gameEntity.GetFrame();
		frame.rotation = Mat3.Identity;
		frame.rotation.RotateAboutSide(_upStateRotationRadian);
		_ladderUpFrame = frame;
		_ladderUpFrame = _ladderObject.GameEntity.Parent.GetFrame().TransformToLocal(_ladderUpFrame);
		_ladderInitialGlobalFrame = _ladderObject.GameEntity.GetGlobalFrame();
		_attackerStandingPointLocalIKFrames = new MatrixFrame[_attackerStandingPoints.Count];
		MatrixFrame frame2 = _ladderObject.GameEntity.Parent.GetFrame();
		MatrixFrame matrixFrame = frame2;
		matrixFrame.rotation.RotateAboutForward(_turningAngle);
		State = initialState;
		for (int i = 0; i < _attackerStandingPoints.Count; i++)
		{
			MatrixFrame frame3 = _attackerStandingPoints[i].GameEntity.GetFrame();
			frame3 = matrixFrame.TransformToParent(frame3);
			frame3 = frame2.TransformToLocal(frame3);
			_attackerStandingPoints[i].GameEntity.SetFrame(ref frame3);
			_attackerStandingPointLocalIKFrames[i] = _attackerStandingPoints[i].GameEntity.GetGlobalFrame().TransformToLocal(_ladderInitialGlobalFrame);
			_attackerStandingPoints[i].AddComponent(new ClearHandInverseKinematicsOnStopUsageComponent());
		}
		CalculateNavigationAndPhysics();
		InitialWaitPosition = base.GameEntity.CollectChildrenEntitiesWithTag(InitialWaitPositionTag).FirstOrDefault();
		foreach (GameEntity item2 in base.Scene.FindEntitiesWithTag(_targetWallSegmentTag))
		{
			WallSegment firstScriptOfType2 = item2.GetFirstScriptOfType<WallSegment>();
			if (firstScriptOfType2 != null)
			{
				_targetWallSegment = firstScriptOfType2;
				_targetWallSegment.AttackerSiegeWeapon = this;
				break;
			}
		}
		switch (_sideTag)
		{
		case "left":
			WeaponSide = FormationAI.BehaviorSide.Left;
			break;
		case "middle":
			WeaponSide = FormationAI.BehaviorSide.Middle;
			break;
		case "right":
			WeaponSide = FormationAI.BehaviorSide.Right;
			break;
		default:
			WeaponSide = FormationAI.BehaviorSide.Middle;
			break;
		}
		SetForcedUse(value: false);
		LadderQueueManager[] array = base.GameEntity.GetScriptComponents<LadderQueueManager>().ToArray();
		MatrixFrame managedFrame = base.GameEntity.GetGlobalFrame().TransformToLocal(_ladderObject.GameEntity.GetGlobalFrame());
		int num = 0;
		int num2 = 1;
		for (int num3 = base.GameEntity.Name.Length - 1; num3 >= 0; num3--)
		{
			if (char.IsDigit(base.GameEntity.Name[num3]))
			{
				num += (base.GameEntity.Name[num3] - 48) * num2;
				num2 *= 10;
			}
			else if (num > 0)
			{
				break;
			}
		}
		if (array.Length != 0)
		{
			_queueManagerForAttackers = array[0];
			_queueManagerForAttackers.Initialize(OnWallNavMeshId, managedFrame, -managedFrame.rotation.f, BattleSideEnum.Attacker, 3, (float)Math.PI * 3f / 4f, 2f, 0.8f, 6f, 5f, blockUsage: false, 0.8f, num, 5f, doesManageMultipleIDs: false, -2, -2, num, 2);
		}
		if (array.Length > 1 && _pushingWithForkStandingPoint != null)
		{
			_queueManagerForDefenders = array[1];
			MatrixFrame globalFrame = _pushingWithForkStandingPoint.GameEntity.GetGlobalFrame();
			globalFrame.rotation.RotateAboutSide((float)Math.PI / 2f);
			globalFrame.origin -= globalFrame.rotation.u;
			globalFrame = base.GameEntity.GetGlobalFrame().TransformToLocal(globalFrame);
			_queueManagerForDefenders.Initialize(OnWallNavMeshId, globalFrame, managedFrame.rotation.f, BattleSideEnum.Defender, 1, (float)Math.PI * 9f / 10f, 0.5f, 0.8f, 6f, 5f, blockUsage: true, 0.8f, float.MaxValue, 5f, doesManageMultipleIDs: false, -2, -2, 0, 0);
		}
		base.GameEntity.Scene.MarkFacesWithIdAsLadder(OnWallNavMeshId, isLadder: true);
		EnemyRangeToStopUsing = 0f;
		_idleAnimationIndex = MBAnimation.GetAnimationIndexWithName(IdleAnimation);
		_raiseAnimationIndex = MBAnimation.GetAnimationIndexWithName(RaiseAnimation);
		_raiseAnimationWithoutRootBoneIndex = MBAnimation.GetAnimationIndexWithName(RaiseAnimationWithoutRootBone);
		_pushBackAnimationIndex = MBAnimation.GetAnimationIndexWithName(PushBackAnimation);
		_pushBackAnimationWithoutRootBoneIndex = MBAnimation.GetAnimationIndexWithName(PushBackAnimationWithoutRootBone);
		_trembleWallHeavyAnimationIndex = MBAnimation.GetAnimationIndexWithName(TrembleWallHeavyAnimation);
		_trembleWallLightAnimationIndex = MBAnimation.GetAnimationIndexWithName(TrembleWallLightAnimation);
		_trembleGroundAnimationIndex = MBAnimation.GetAnimationIndexWithName(TrembleGroundAnimation);
		SetUpStateVisibility(isVisible: false);
		SetScriptComponentToTick(GetTickRequirement());
		bool flag = false;
		foreach (GameEntity entityAndChild in _ladderObject.GameEntity.GetEntityAndChildren())
		{
			PhysicsShape bodyShape = entityAndChild.GetBodyShape();
			if (bodyShape != null)
			{
				PhysicsShape.AddPreloadQueueWithName(bodyShape.GetName(), entityAndChild.GetGlobalScale());
				flag = true;
			}
		}
		if (flag)
		{
			PhysicsShape.ProcessPreloadQueue();
		}
	}

	private float GetCurrentLadderAngularSpeed(int animationIndex)
	{
		float animationParameterAtChannel = _ladderSkeleton.GetAnimationParameterAtChannel(0);
		MatrixFrame boneEntitialFrameWithIndex = _ladderSkeleton.GetBoneEntitialFrameWithIndex(0);
		if (animationParameterAtChannel <= 0.01f)
		{
			return 0f;
		}
		_ladderSkeleton.SetAnimationParameterAtChannel(0, animationParameterAtChannel - 0.01f);
		_ladderSkeleton.TickAnimationsAndForceUpdate(0.0001f, _ladderObject.GameEntity.GetGlobalFrame(), tickAnimsForChildren: false);
		MatrixFrame boneEntitialFrameWithIndex2 = _ladderSkeleton.GetBoneEntitialFrameWithIndex(0);
		Vec2 vec = new Vec2(boneEntitialFrameWithIndex.rotation.f.y, boneEntitialFrameWithIndex.rotation.f.z);
		Vec2 vec2 = new Vec2(boneEntitialFrameWithIndex2.rotation.f.y, boneEntitialFrameWithIndex2.rotation.f.z);
		return (vec.RotationInRadians - vec2.RotationInRadians) / (MBAnimation.GetAnimationDuration(animationIndex) * 0.01f);
	}

	private void OnLadderStateChange()
	{
		GameEntity gameEntity = _ladderObject.GameEntity;
		if (State != LadderState.OnWall)
		{
			SetVisibilityOfAIBarriers(visibility: true);
		}
		switch (State)
		{
		case LadderState.OnLand:
			_animationState = LadderAnimationState.Static;
			break;
		case LadderState.OnWall:
			_animationState = LadderAnimationState.Static;
			SetVisibilityOfAIBarriers(visibility: false);
			break;
		case LadderState.FallToWall:
			if (GameNetwork.IsClientOrReplay)
			{
				int animationIndexAtChannel = _ladderSkeleton.GetAnimationIndexAtChannel(0);
				if (animationIndexAtChannel != _trembleWallHeavyAnimationIndex && animationIndexAtChannel != _trembleWallLightAnimationIndex)
				{
					gameEntity.SetFrame(ref _ladderUpFrame);
					_ladderSkeleton.SetAnimationAtChannel((_fallAngularSpeed < -0.5f) ? _trembleWallHeavyAnimationIndex : _trembleWallLightAnimationIndex, 0);
					_animationState = LadderAnimationState.Static;
				}
			}
			else
			{
				State = LadderState.OnWall;
				_ladderParticleObject?.BurstParticlesSynched(doChildren: false);
			}
			break;
		case LadderState.FallToLand:
			if (_ladderSkeleton.GetAnimationIndexAtChannel(0) != _trembleGroundAnimationIndex)
			{
				gameEntity.SetFrame(ref _ladderDownFrame);
				_ladderSkeleton.SetAnimationAtChannel(_trembleGroundAnimationIndex, 0);
				_animationState = LadderAnimationState.Static;
			}
			if (!GameNetwork.IsClientOrReplay)
			{
				State = LadderState.OnLand;
			}
			break;
		case LadderState.BeingRaisedStartFromGround:
		{
			_animationState = LadderAnimationState.Animated;
			MatrixFrame frame3 = gameEntity.GetFrame();
			frame3.rotation.RotateAboutSide(-(float)Math.PI / 2f);
			gameEntity.SetFrame(ref frame3);
			_ladderSkeleton.SetAnimationAtChannel(_raiseAnimationIndex, 0);
			_ladderSkeleton.ForceUpdateBoneFrames();
			_lastDotProductOfAnimationAndTargetRotation = -1000f;
			if (!GameNetwork.IsClientOrReplay)
			{
				_currentActionAgentCount = 1;
				State = LadderState.BeingRaised;
			}
			break;
		}
		case LadderState.BeingPushedBackStartFromWall:
			_animationState = LadderAnimationState.Animated;
			_ladderSkeleton.SetAnimationAtChannel(_pushBackAnimationIndex, 0);
			_ladderSkeleton.TickAnimationsAndForceUpdate(0.0001f, gameEntity.GetGlobalFrame(), tickAnimsForChildren: false);
			_lastDotProductOfAnimationAndTargetRotation = -1000f;
			if (!GameNetwork.IsClientOrReplay)
			{
				_currentActionAgentCount = 1;
				State = LadderState.BeingPushedBack;
			}
			break;
		case LadderState.BeingRaisedStopped:
		{
			_animationState = LadderAnimationState.PhysicallyDynamic;
			MatrixFrame frame2 = gameEntity.GetGlobalFrame().TransformToParent(_ladderSkeleton.GetBoneEntitialFrameWithIndex(0));
			frame2.rotation.RotateAboutForward((float)Math.PI / 2f);
			_fallAngularSpeed = GetCurrentLadderAngularSpeed(_raiseAnimationIndex);
			float animationParameterAtChannel2 = _ladderSkeleton.GetAnimationParameterAtChannel(0);
			gameEntity.SetGlobalFrame(in frame2);
			_ladderSkeleton.SetAnimationAtChannel(_raiseAnimationWithoutRootBoneIndex, 0);
			_ladderSkeleton.SetAnimationParameterAtChannel(0, animationParameterAtChannel2);
			_ladderSkeleton.TickAnimationsAndForceUpdate(0.0001f, gameEntity.GetGlobalFrame(), tickAnimsForChildren: false);
			_ladderSkeleton.SetAnimationAtChannel(_idleAnimationIndex, 0);
			_ladderObject.SetLocalPositionSmoothStep(ref _ladderDownFrame.origin);
			if (!GameNetwork.IsClientOrReplay)
			{
				State = LadderState.BeingPushedBack;
			}
			break;
		}
		case LadderState.BeingPushedBackStopped:
		{
			_animationState = LadderAnimationState.PhysicallyDynamic;
			MatrixFrame frame = gameEntity.GetGlobalFrame().TransformToParent(_ladderSkeleton.GetBoneEntitialFrameWithIndex(0));
			frame.rotation.RotateAboutForward((float)Math.PI / 2f);
			_fallAngularSpeed = GetCurrentLadderAngularSpeed(_pushBackAnimationIndex);
			float animationParameterAtChannel = _ladderSkeleton.GetAnimationParameterAtChannel(0);
			gameEntity.SetGlobalFrame(in frame);
			_ladderSkeleton.SetAnimationAtChannel(_pushBackAnimationWithoutRootBoneIndex, 0);
			_ladderSkeleton.SetAnimationParameterAtChannel(0, animationParameterAtChannel);
			_ladderSkeleton.TickAnimationsAndForceUpdate(0.0001f, gameEntity.GetGlobalFrame(), tickAnimsForChildren: false);
			_ladderSkeleton.SetAnimationAtChannel(_idleAnimationIndex, 0);
			_ladderObject.SetLocalPositionSmoothStep(ref _ladderUpFrame.origin);
			if (!GameNetwork.IsClientOrReplay)
			{
				State = LadderState.BeingRaised;
			}
			_ladderSkeleton.ForceUpdateBoneFrames();
			break;
		}
		case LadderState.BeingRaised:
		case LadderState.BeingPushedBack:
			break;
		}
	}

	private void SetVisibilityOfAIBarriers(bool visibility)
	{
		foreach (GameEntity aiBarrier in _aiBarriers)
		{
			aiBarrier.SetVisibilityExcludeParents(visibility);
		}
	}

	public override OrderType GetOrder(BattleSideEnum side)
	{
		if (side != BattleSideEnum.Attacker)
		{
			return OrderType.Move;
		}
		return base.GetOrder(side);
	}

	private void CalculateNavigationAndPhysics()
	{
		if (!GameNetwork.IsClientOrReplay)
		{
			bool flag = State != LadderState.OnWall;
			if (_isNavigationMeshDisabled != flag)
			{
				_isNavigationMeshDisabled = flag;
				SetAbilityOfFaces(!_isNavigationMeshDisabled);
			}
		}
		bool flag2 = (State == LadderState.BeingRaisedStartFromGround || State == LadderState.BeingRaised) && _animationState != LadderAnimationState.PhysicallyDynamic;
		bool flag3 = true;
		if (_isLadderPhysicsDisabled != flag2)
		{
			_isLadderPhysicsDisabled = flag2;
			_ladderBodyObject.GameEntity.SetPhysicsState(!_isLadderPhysicsDisabled, setChildren: true);
		}
		if (!flag2)
		{
			MatrixFrame frame = _ladderObject.GameEntity.GetGlobalFrame().TransformToParent(_ladderSkeleton.GetBoneEntitialFrameWithIndex(0));
			frame.rotation.RotateAboutForward((float)Math.PI / 2f);
			_ladderBodyObject.GameEntity.SetGlobalFrame(in frame);
			flag3 = State != LadderState.BeingPushedBack || frame.rotation.f.z < 0f;
			if (!flag3)
			{
				float y = TaleWorlds.Library.MathF.Min(2.01f - frame.rotation.u.z * 2f, 1f);
				frame.rotation.ApplyScaleLocal(new Vec3(1f, y, 1f));
				_ladderCollisionBodyObject.GameEntity.SetGlobalFrame(in frame);
			}
		}
		if (_isLadderCollisionPhysicsDisabled != flag3)
		{
			_isLadderCollisionPhysicsDisabled = flag3;
			_ladderCollisionBodyObject.GameEntity.SetPhysicsState(!_isLadderCollisionPhysicsDisabled, setChildren: true);
		}
	}

	public bool HasCompletedAction()
	{
		return State == LadderState.OnWall;
	}

	private ActionIndexCache GetActionCodeToUseForStandingPoint(StandingPoint standingPoint)
	{
		GameEntity gameEntity = standingPoint.GameEntity;
		if (!gameEntity.HasTag(RightStandingPointTag))
		{
			if (!gameEntity.HasTag(FrontStandingPointTag))
			{
				return act_usage_ladder_lift_from_left_2_start;
			}
			return act_usage_ladder_lift_from_left_1_start;
		}
		if (!gameEntity.HasTag(FrontStandingPointTag))
		{
			return act_usage_ladder_lift_from_right_2_start;
		}
		return act_usage_ladder_lift_from_right_1_start;
	}

	public override bool IsDisabledForBattleSide(BattleSideEnum sideEnum)
	{
		if (sideEnum == BattleSideEnum.Attacker)
		{
			if (State != LadderState.FallToLand && State != LadderState.FallToWall && State != LadderState.OnWall && (State != LadderState.BeingPushedBack || _animationState == LadderAnimationState.PhysicallyDynamic) && State != LadderState.BeingPushedBackStartFromWall)
			{
				return State == LadderState.BeingPushedBackStopped;
			}
			return true;
		}
		if (State != 0 && State != LadderState.FallToLand && State != LadderState.BeingRaised && State != LadderState.BeingRaisedStartFromGround)
		{
			return State == LadderState.FallToWall;
		}
		return true;
	}

	protected override float GetDetachmentWeightAux(BattleSideEnum side)
	{
		if (side == BattleSideEnum.Attacker)
		{
			return base.GetDetachmentWeightAux(side);
		}
		if (IsDisabledForBattleSideAI(side))
		{
			return float.MinValue;
		}
		_usableStandingPoints.Clear();
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < base.StandingPoints.Count; i++)
		{
			StandingPoint standingPoint = base.StandingPoints[i];
			if (!standingPoint.IsUsableBySide(side) || (standingPoint == _forkPickUpStandingPoint && !_pushingWithForkStandingPoint.IsUsableBySide(side)))
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

	public override TickRequirement GetTickRequirement()
	{
		return base.GetTickRequirement() | TickRequirement.Tick | TickRequirement.TickParallel;
	}

	protected internal override void OnTick(float dt)
	{
		base.OnTick(dt);
		if (_tickOccasionallyTimer.Check(Mission.Current.CurrentTime))
		{
			TickRare();
		}
		if (!GameNetwork.IsClientOrReplay && _forkReappearingTimer != null && _forkReappearingTimer.Check(Mission.Current.CurrentTime))
		{
			_forkPickUpStandingPoint.SetIsDeactivatedSynched(value: false);
			_forkEntity.SetVisibleSynched(value: true);
		}
		int num = 0;
		int num2 = 0;
		GameEntity gameEntity = _ladderObject.GameEntity;
		if (!GameNetwork.IsClientOrReplay)
		{
			if (_queueManagerForAttackers != null)
			{
				if (_queueManagerForAttackers.IsDeactivated)
				{
					if (State == LadderState.OnWall)
					{
						_queueManagerForAttackers.Activate();
					}
				}
				else if (State == LadderState.OnLand)
				{
					_queueManagerForAttackers.Deactivate();
				}
			}
			if (_queueManagerForDefenders != null && _queueManagerForDefenders.IsDeactivated != (State != LadderState.OnWall))
			{
				if (State != LadderState.OnWall)
				{
					_queueManagerForDefenders.DeactivateImmediate();
				}
				else
				{
					_queueManagerForDefenders.Activate();
				}
			}
			int animationIndexAtChannel = _ladderSkeleton.GetAnimationIndexAtChannel(0);
			bool flag = false;
			if (animationIndexAtChannel >= 0)
			{
				flag = animationIndexAtChannel == _trembleGroundAnimationIndex || animationIndexAtChannel == _trembleWallHeavyAnimationIndex || animationIndexAtChannel == _trembleWallLightAnimationIndex;
				if (flag)
				{
					flag = _ladderSkeleton.GetAnimationParameterAtChannel(0) < 1f;
				}
			}
			num += ((_pushingWithForkStandingPoint.HasUser && !_pushingWithForkStandingPoint.UserAgent.IsInBeingStruckAction) ? 1 : 0);
			foreach (StandingPoint attackerStandingPoint in _attackerStandingPoints)
			{
				if (attackerStandingPoint.HasUser && !attackerStandingPoint.UserAgent.IsInBeingStruckAction)
				{
					num2++;
				}
			}
			foreach (StandingPoint standingPoint in base.StandingPoints)
			{
				GameEntity gameEntity2 = standingPoint.GameEntity;
				if (!gameEntity2.HasTag(AmmoPickUpTag))
				{
					bool flag2 = false;
					if ((!standingPoint.HasUser || standingPoint.UserAgent.IsInBeingStruckAction) && State == LadderState.BeingRaised && gameEntity2.HasTag(AttackerTag))
					{
						float animationParameterAtChannel = _ladderSkeleton.GetAnimationParameterAtChannel(0);
						float animationDuration = MBAnimation.GetAnimationDuration(_ladderSkeleton.GetAnimationIndexAtChannel(0));
						int animationIndex = MBActionSet.GetAnimationIndexOfAction(actionIndexCache: GetActionCodeToUseForStandingPoint(standingPoint), actionSet: MBGlobals.GetActionSetWithSuffix(Game.Current.DefaultMonster, isFemale: false, "_warrior"));
						flag2 = animationParameterAtChannel * animationDuration / TaleWorlds.Library.MathF.Max(MBAnimation.GetAnimationDuration(animationIndex), 0.01f) > 0.98f;
					}
					standingPoint.SetIsDeactivatedSynched(flag2 || State == LadderState.BeingPushedBackStopped || (gameEntity2.HasTag(AttackerTag) && (State == LadderState.OnWall || State == LadderState.FallToWall || (State == LadderState.BeingPushedBack && _animationState != LadderAnimationState.PhysicallyDynamic) || State == LadderState.BeingPushedBackStartFromWall)) || (gameEntity2.HasTag(DefenderTag) && (State == LadderState.OnLand || _animationState == LadderAnimationState.PhysicallyDynamic || State == LadderState.BeingRaisedStopped || flag || State == LadderState.FallToLand || State == LadderState.BeingRaised || State == LadderState.BeingRaisedStartFromGround || !CanLadderBePushed())));
				}
			}
			if (_forkPickUpStandingPoint.HasUser)
			{
				Agent userAgent = _forkPickUpStandingPoint.UserAgent;
				ActionIndexValueCache currentActionValue = userAgent.GetCurrentActionValue(1);
				if (!(currentActionValue == act_usage_ladder_pick_up_fork_begin))
				{
					if (currentActionValue == act_usage_ladder_pick_up_fork_end)
					{
						MissionWeapon weapon = new MissionWeapon(_forkItem, null, null);
						userAgent.EquipWeaponToExtraSlotAndWield(ref weapon);
						_forkPickUpStandingPoint.UserAgent.StopUsingGameObject();
						_forkPickUpStandingPoint.SetIsDeactivatedSynched(value: true);
						_forkEntity.SetVisibleSynched(value: false);
						_forkReappearingTimer = new Timer(Mission.Current.CurrentTime, _forkReappearingDelay);
						if (userAgent.IsAIControlled)
						{
							StandingPoint suitableStandingPointFor = GetSuitableStandingPointFor(userAgent.Team.Side, userAgent);
							if (suitableStandingPointFor != null)
							{
								((IDetachment)this).AddAgent(userAgent, -1);
								if (userAgent.Formation != null)
								{
									userAgent.Formation.DetachUnit(userAgent, ((IDetachment)this).IsLoose);
									userAgent.Detachment = this;
									userAgent.DetachmentWeight = GetWeightOfStandingPoint(suitableStandingPointFor);
								}
							}
						}
					}
					else if (!_forkPickUpStandingPoint.UserAgent.SetActionChannel(1, act_usage_ladder_pick_up_fork_begin, ignorePriority: false, 0uL))
					{
						_forkPickUpStandingPoint.UserAgent.StopUsingGameObject();
					}
				}
			}
			else if (_forkPickUpStandingPoint.HasAIMovingTo)
			{
				Agent movingAgent = _forkPickUpStandingPoint.MovingAgent;
				if (movingAgent.Team != null && !_pushingWithForkStandingPoint.IsUsableBySide(movingAgent.Team.Side))
				{
					movingAgent.StopUsingGameObject();
				}
			}
		}
		switch (State)
		{
		case LadderState.OnLand:
		case LadderState.FallToLand:
			if (!GameNetwork.IsClientOrReplay && num2 > 0)
			{
				State = LadderState.BeingRaisedStartFromGround;
			}
			break;
		case LadderState.OnWall:
		case LadderState.FallToWall:
			if (num > 0 && !GameNetwork.IsClientOrReplay)
			{
				State = LadderState.BeingPushedBackStartFromWall;
			}
			break;
		case LadderState.BeingRaised:
		case LadderState.BeingRaisedStartFromGround:
		case LadderState.BeingPushedBackStopped:
			if (_animationState == LadderAnimationState.Animated)
			{
				float animationParameterAtChannel4 = _ladderSkeleton.GetAnimationParameterAtChannel(0);
				float animationDuration2 = MBAnimation.GetAnimationDuration(_ladderSkeleton.GetAnimationIndexAtChannel(0));
				foreach (StandingPoint attackerStandingPoint2 in _attackerStandingPoints)
				{
					if (attackerStandingPoint2.HasUser)
					{
						MBActionSet actionSet = attackerStandingPoint2.UserAgent.ActionSet;
						ActionIndexCache actionCodeToUseForStandingPoint2 = GetActionCodeToUseForStandingPoint(attackerStandingPoint2);
						ActionIndexValueCache currentActionValue2 = attackerStandingPoint2.UserAgent.GetCurrentActionValue(1);
						if (currentActionValue2 == actionCodeToUseForStandingPoint2)
						{
							int animationIndexOfAction = MBActionSet.GetAnimationIndexOfAction(actionSet, actionCodeToUseForStandingPoint2);
							float progress = MBMath.ClampFloat(animationParameterAtChannel4 * animationDuration2 / TaleWorlds.Library.MathF.Max(MBAnimation.GetAnimationDuration(animationIndexOfAction), 0.01f), 0f, 1f);
							attackerStandingPoint2.UserAgent.SetCurrentActionProgress(1, progress);
						}
						else if (MBAnimation.GetActionType(currentActionValue2) == Agent.ActionCodeType.LadderRaiseEnd)
						{
							float animationDuration3 = MBAnimation.GetAnimationDuration(MBActionSet.GetAnimationIndexOfAction(actionSet, currentActionValue2));
							float num4 = animationDuration2 - animationDuration3;
							float progress2 = MBMath.ClampFloat((animationParameterAtChannel4 * animationDuration2 - num4) / TaleWorlds.Library.MathF.Max(animationDuration3, 0.01f), 0f, 1f);
							attackerStandingPoint2.UserAgent.SetCurrentActionProgress(1, progress2);
						}
					}
				}
				bool flag4 = false;
				if (!GameNetwork.IsClientOrReplay)
				{
					if (num2 > 0)
					{
						if (num2 != _currentActionAgentCount)
						{
							_currentActionAgentCount = num2;
							float animationSpeed2 = TaleWorlds.Library.MathF.Sqrt(_currentActionAgentCount);
							float animationParameterAtChannel5 = _ladderSkeleton.GetAnimationParameterAtChannel(0);
							_ladderObject.SetAnimationAtChannelSynched(_raiseAnimationIndex, 0, animationSpeed2);
							if (animationParameterAtChannel5 > 0f)
							{
								_ladderObject.SetAnimationChannelParameterSynched(0, animationParameterAtChannel5);
							}
						}
						foreach (StandingPoint attackerStandingPoint3 in _attackerStandingPoints)
						{
							if (!attackerStandingPoint3.HasUser)
							{
								continue;
							}
							ActionIndexCache actionCodeToUseForStandingPoint3 = GetActionCodeToUseForStandingPoint(attackerStandingPoint3);
							Agent userAgent3 = attackerStandingPoint3.UserAgent;
							ActionIndexValueCache currentActionValue3 = userAgent3.GetCurrentActionValue(1);
							if (currentActionValue3 != actionCodeToUseForStandingPoint3 && MBAnimation.GetActionType(currentActionValue3) != Agent.ActionCodeType.LadderRaiseEnd)
							{
								if (!userAgent3.SetActionChannel(1, actionCodeToUseForStandingPoint3, ignorePriority: false, 0uL) && !userAgent3.IsAIControlled)
								{
									userAgent3.StopUsingGameObject(isSuccessful: false);
								}
							}
							else if (MBAnimation.GetActionType(currentActionValue3) == Agent.ActionCodeType.LadderRaiseEnd)
							{
								attackerStandingPoint3.UserAgent.ClearTargetFrame();
							}
						}
					}
					else
					{
						State = LadderState.BeingRaisedStopped;
						flag4 = true;
					}
				}
				if (!flag4)
				{
					MatrixFrame frame3 = gameEntity.GetGlobalFrame().TransformToParent(_ladderSkeleton.GetBoneEntitialFrameWithIndex(0));
					frame3.rotation.RotateAboutForward((float)Math.PI / 2f);
					if ((animationParameterAtChannel4 > 0.9f && animationParameterAtChannel4 != 1f) || frame3.rotation.f.z <= 0.2f)
					{
						_animationState = LadderAnimationState.PhysicallyDynamic;
						_fallAngularSpeed = GetCurrentLadderAngularSpeed(_raiseAnimationIndex);
						gameEntity.SetGlobalFrame(in frame3);
						_ladderSkeleton.SetAnimationAtChannel(_raiseAnimationWithoutRootBoneIndex, 0);
						_ladderSkeleton.SetAnimationParameterAtChannel(0, animationParameterAtChannel4);
						_ladderSkeleton.TickAnimationsAndForceUpdate(0.0001f, gameEntity.GetGlobalFrame(), tickAnimsForChildren: false);
						_ladderSkeleton.SetAnimationAtChannel(_idleAnimationIndex, 0);
						_ladderObject.SetLocalPositionSmoothStep(ref _ladderUpFrame.origin);
					}
				}
			}
			else
			{
				if (_animationState != LadderAnimationState.PhysicallyDynamic)
				{
					break;
				}
				MatrixFrame frame4 = gameEntity.GetFrame();
				frame4.rotation.RotateAboutSide(_fallAngularSpeed * dt);
				gameEntity.SetFrame(ref frame4);
				MatrixFrame matrixFrame2 = gameEntity.GetFrame().TransformToParent(_ladderSkeleton.GetBoneEntitialFrameWithIndex(0));
				float num5 = Vec3.DotProduct(matrixFrame2.rotation.f, _ladderUpFrame.rotation.f);
				if (_fallAngularSpeed < 0f && num5 > 0.95f && num5 < _lastDotProductOfAnimationAndTargetRotation)
				{
					gameEntity.SetFrame(ref _ladderUpFrame);
					_ladderSkeleton.SetAnimationParameterAtChannel(0, 0f);
					_ladderSkeleton.TickAnimationsAndForceUpdate(0.0001f, gameEntity.GetGlobalFrame(), tickAnimsForChildren: false);
					_animationState = LadderAnimationState.Static;
					_ladderSkeleton.SetAnimationAtChannel((_fallAngularSpeed < -0.5f) ? _trembleWallHeavyAnimationIndex : _trembleWallLightAnimationIndex, 0);
					if (!GameNetwork.IsClientOrReplay)
					{
						State = LadderState.FallToWall;
					}
				}
				_fallAngularSpeed -= dt * 2f * TaleWorlds.Library.MathF.Max(0.3f, 1f - matrixFrame2.rotation.u.z);
				_lastDotProductOfAnimationAndTargetRotation = num5;
			}
			break;
		case LadderState.BeingRaisedStopped:
		case LadderState.BeingPushedBack:
		case LadderState.BeingPushedBackStartFromWall:
			if (_animationState == LadderAnimationState.Animated)
			{
				float animationParameterAtChannel2 = _ladderSkeleton.GetAnimationParameterAtChannel(0);
				if (_pushingWithForkStandingPoint.HasUser)
				{
					ActionIndexCache actionIndexCache = act_usage_ladder_push_back;
					if (_pushingWithForkStandingPoint.UserAgent.GetCurrentActionValue(1) == actionIndexCache)
					{
						_pushingWithForkStandingPoint.UserAgent.SetCurrentActionProgress(1, animationParameterAtChannel2);
					}
				}
				bool flag3 = false;
				if (!GameNetwork.IsClientOrReplay)
				{
					if (num > 0)
					{
						if (num != _currentActionAgentCount)
						{
							_currentActionAgentCount = num;
							float animationSpeed = TaleWorlds.Library.MathF.Sqrt(_currentActionAgentCount);
							float animationParameterAtChannel3 = _ladderSkeleton.GetAnimationParameterAtChannel(0);
							_ladderObject.SetAnimationAtChannelSynched(PushBackAnimation, 0, animationSpeed);
							if (animationParameterAtChannel3 > 0f)
							{
								_ladderObject.SetAnimationChannelParameterSynched(0, animationParameterAtChannel3);
							}
						}
						if (_pushingWithForkStandingPoint.HasUser)
						{
							ActionIndexCache actionIndexCache2 = act_usage_ladder_push_back;
							Agent userAgent2 = _pushingWithForkStandingPoint.UserAgent;
							if (userAgent2.GetCurrentActionValue(1) != actionIndexCache2 && animationParameterAtChannel2 < 1f && !userAgent2.SetActionChannel(1, actionIndexCache2, ignorePriority: false, 0uL) && !userAgent2.IsAIControlled)
							{
								userAgent2.StopUsingGameObject(isSuccessful: false);
							}
						}
					}
					else
					{
						State = LadderState.BeingPushedBackStopped;
						flag3 = true;
					}
				}
				if (!flag3)
				{
					MatrixFrame frame = gameEntity.GetGlobalFrame().TransformToParent(_ladderSkeleton.GetBoneEntitialFrameWithIndex(0));
					frame.rotation.RotateAboutForward((float)Math.PI / 2f);
					if (animationParameterAtChannel2 > 0.9999f || frame.rotation.f.z >= 0f)
					{
						_animationState = LadderAnimationState.PhysicallyDynamic;
						_fallAngularSpeed = GetCurrentLadderAngularSpeed(_pushBackAnimationIndex);
						gameEntity.SetGlobalFrame(in frame);
						_ladderSkeleton.SetAnimationAtChannel(_pushBackAnimationWithoutRootBoneIndex, 0);
						_ladderSkeleton.SetAnimationParameterAtChannel(0, animationParameterAtChannel2);
						_ladderSkeleton.TickAnimationsAndForceUpdate(0.0001f, gameEntity.GetGlobalFrame(), tickAnimsForChildren: false);
						_ladderSkeleton.SetAnimationAtChannel(_idleAnimationIndex, 0);
						_ladderObject.SetLocalPositionSmoothStep(ref _ladderDownFrame.origin);
					}
				}
			}
			else
			{
				if (_animationState != LadderAnimationState.PhysicallyDynamic)
				{
					break;
				}
				MatrixFrame frame2 = gameEntity.GetFrame();
				frame2.rotation.RotateAboutSide(_fallAngularSpeed * dt);
				gameEntity.SetFrame(ref frame2);
				MatrixFrame matrixFrame = gameEntity.GetFrame().TransformToParent(_ladderSkeleton.GetBoneEntitialFrameWithIndex(0));
				matrixFrame.rotation.RotateAboutForward((float)Math.PI / 2f);
				float num3 = Vec3.DotProduct(matrixFrame.rotation.f, _ladderDownFrame.rotation.f);
				if (_fallAngularSpeed > 0f && num3 > 0.95f && num3 < _lastDotProductOfAnimationAndTargetRotation)
				{
					_animationState = LadderAnimationState.Static;
					gameEntity.SetFrame(ref _ladderDownFrame);
					_ladderSkeleton.SetAnimationParameterAtChannel(0, 0f);
					_ladderSkeleton.TickAnimationsAndForceUpdate(0.0001f, gameEntity.GetGlobalFrame(), tickAnimsForChildren: false);
					_ladderSkeleton.SetAnimationAtChannel(_trembleGroundAnimationIndex, 0);
					_animationState = LadderAnimationState.Static;
					if (!GameNetwork.IsClientOrReplay)
					{
						State = LadderState.FallToLand;
					}
				}
				_fallAngularSpeed += dt * 2f * TaleWorlds.Library.MathF.Max(0.3f, 1f - matrixFrame.rotation.u.z);
				_lastDotProductOfAnimationAndTargetRotation = num3;
			}
			break;
		default:
			Debug.FailedAssert("Invalid ladder action state.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Objects\\Siege\\SiegeLadder.cs", "OnTick", 1259);
			break;
		}
		CalculateNavigationAndPhysics();
	}

	protected internal override void OnTickParallel(float dt)
	{
		base.OnTickParallel(dt);
		for (int i = 0; i < _attackerStandingPoints.Count; i++)
		{
			if (!_attackerStandingPoints[i].HasUser)
			{
				continue;
			}
			if (!_attackerStandingPoints[i].UserAgent.IsInBeingStruckAction)
			{
				if (_attackerStandingPoints[i].UserAgent.GetCurrentAction(1) != GetActionCodeToUseForStandingPoint(_attackerStandingPoints[i]))
				{
					MatrixFrame localIKFrame = _attackerStandingPointLocalIKFrames[i];
					localIKFrame.rotation = Mat3.Lerp(localIKFrame.rotation, _ladderInitialGlobalFrame.TransformToLocal(_attackerStandingPoints[i].UserAgent.Frame).rotation, TaleWorlds.Library.MathF.Clamp(TaleWorlds.Library.MathF.Lerp(0f, 1f - _turningAngle * 1.2f, TaleWorlds.Library.MathF.Pow(_attackerStandingPoints[i].UserAgent.GetCurrentActionProgress(1), 6f)), 0f, 1f));
					_attackerStandingPoints[i].UserAgent.SetHandInverseKinematicsFrameForMissionObjectUsage(in localIKFrame, in _ladderInitialGlobalFrame);
				}
				else
				{
					_attackerStandingPoints[i].UserAgent.SetHandInverseKinematicsFrameForMissionObjectUsage(in _attackerStandingPointLocalIKFrames[i], in _ladderInitialGlobalFrame);
				}
			}
			else
			{
				_attackerStandingPoints[i].UserAgent.ClearHandInverseKinematics();
			}
		}
	}

	private void TickRare()
	{
		if (GameNetwork.IsReplay)
		{
			return;
		}
		float num = 20f + (base.ForcedUse ? 3f : 0f);
		num *= num;
		GameEntity gameEntity = base.GameEntity;
		Mission.TeamCollection teams = Mission.Current.Teams;
		int count = teams.Count;
		Vec3 globalPosition = gameEntity.GlobalPosition;
		for (int i = 0; i < count; i++)
		{
			Team team = teams[i];
			if (team.Side != BattleSideEnum.Attacker)
			{
				continue;
			}
			SetForcedUse(value: false);
			foreach (Formation item in team.FormationsIncludingSpecialAndEmpty)
			{
				if (item.CountOfUnits > 0 && item.QuerySystem.MedianPosition.AsVec2.DistanceSquared(globalPosition.AsVec2) < num && item.QuerySystem.MedianPosition.GetNavMeshZ() - globalPosition.z < 4f)
				{
					SetForcedUse(value: true);
					break;
				}
			}
		}
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		return new SiegeLadderAI(this);
	}

	public void SetUpStateVisibility(bool isVisible)
	{
		base.GameEntity.CollectChildrenEntitiesWithTag(upStateEntityTag)[0].SetVisibilityExcludeParents(isVisible);
	}

	private void FlushQueueManager()
	{
		_queueManagerForAttackers?.FlushQueueManager();
	}

	private void FlushNeighborQueueManagers()
	{
		foreach (SiegeLadder item in (from sl in Mission.Current.ActiveMissionObjects.FindAllWithType<SiegeLadder>()
			where sl.WeaponSide == WeaponSide
			select sl).ToList())
		{
			if (item != this)
			{
				item.FlushQueueManager();
			}
		}
	}

	private bool CanLadderBePushed()
	{
		float num = 0f;
		GameEntity gameEntity = _ladderObject.GameEntity;
		gameEntity.GetPhysicsMinMax(includeChildren: true, out var bbmin, out var bbmax, returnLocal: false);
		float searchRadius = (bbmax - bbmin).AsVec2.Length * 0.5f;
		AgentProximityMap.ProximityMapSearchStruct searchStruct = AgentProximityMap.BeginSearch(Mission.Current, gameEntity.GlobalPosition.AsVec2, searchRadius);
		while (searchStruct.LastFoundAgent != null)
		{
			Agent lastFoundAgent = searchStruct.LastFoundAgent;
			if (lastFoundAgent.GetSteppedMachine() == this)
			{
				float num2 = (lastFoundAgent.Position.z - bbmin.z) / (bbmax.z - bbmin.z) * 100f;
				if (num2 > LadderPushTresholdForOneAgent)
				{
					return false;
				}
				num += num2;
			}
			AgentProximityMap.FindNext(Mission.Current, ref searchStruct);
		}
		return num <= LadderPushTreshold;
	}

	private void InformNeighborQueueManagers(LadderQueueManager ladderQueueManager)
	{
		foreach (SiegeLadder item in (from sl in Mission.Current.ActiveMissionObjects.FindAllWithType<SiegeLadder>()
			where sl.WeaponSide == WeaponSide && sl._queueManagerForAttackers != null
			select sl).ToList())
		{
			if (item != this && item._queueManagerForAttackers != null)
			{
				item._queueManagerForAttackers.AssignNeighborQueueManager(ladderQueueManager);
				_queueManagerForAttackers?.AssignNeighborQueueManager(item._queueManagerForAttackers);
			}
		}
	}

	public override void SetAbilityOfFaces(bool enabled)
	{
		base.SetAbilityOfFaces(enabled);
		base.GameEntity.Scene.SetAbilityOfFacesWithId(OnWallNavMeshId, enabled);
		if (Mission.Current != null)
		{
			if (enabled)
			{
				FlushNeighborQueueManagers();
				InformNeighborQueueManagers(_queueManagerForAttackers);
			}
			else
			{
				InformNeighborQueueManagers(null);
				_queueManagerForAttackers?.AssignNeighborQueueManager(null);
			}
		}
	}

	protected internal override void OnMissionReset()
	{
		_ladderSkeleton.SetAnimationAtChannel(-1, 0);
		if (initialState == LadderState.OnLand)
		{
			if (!GameNetwork.IsClientOrReplay)
			{
				State = LadderState.OnLand;
			}
			_ladderObject.GameEntity.SetFrame(ref _ladderDownFrame);
		}
		else
		{
			if (!GameNetwork.IsClientOrReplay)
			{
				State = LadderState.OnWall;
			}
			_ladderObject.GameEntity.SetFrame(ref _ladderUpFrame);
		}
	}

	public override string GetDescriptionText(GameEntity gameEntity)
	{
		if (!gameEntity.HasTag(AmmoPickUpTag))
		{
			return new TextObject("{=G0AWk1rX}Ladder").ToString();
		}
		return new TextObject("{=F9AQxCax}Fork").ToString();
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		TextObject textObject = ((!usableGameObject.GameEntity.HasTag(AmmoPickUpTag)) ? (usableGameObject.GameEntity.HasTag(AttackerTag) ? new TextObject("{=kbNcm68J}{KEY} Lift") : new TextObject("{=MdQJxiGz}{KEY} Push")) : new TextObject("{=bNYm3K6b}{KEY} Pick Up"));
		textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
		return textObject;
	}

	public override void WriteToNetwork()
	{
		base.WriteToNetwork();
		GameNetworkMessage.WriteBoolToPacket(initialState == LadderState.OnLand);
		GameNetworkMessage.WriteIntToPacket((int)State, CompressionMission.SiegeLadderStateCompressionInfo);
		GameNetworkMessage.WriteIntToPacket((int)_animationState, CompressionMission.SiegeLadderAnimationStateCompressionInfo);
		GameNetworkMessage.WriteFloatToPacket(_fallAngularSpeed, CompressionMission.SiegeMachineComponentAngularSpeedCompressionInfo);
		GameNetworkMessage.WriteMatrixFrameToPacket(_ladderObject.GameEntity.GetGlobalFrame());
		int animationIndexAtChannel = _ladderSkeleton.GetAnimationIndexAtChannel(0);
		GameNetworkMessage.WriteBoolToPacket(animationIndexAtChannel >= 0);
		if (animationIndexAtChannel >= 0)
		{
			GameNetworkMessage.WriteIntToPacket(animationIndexAtChannel, CompressionBasic.AnimationIndexCompressionInfo);
			GameNetworkMessage.WriteFloatToPacket(_ladderSkeleton.GetAnimationParameterAtChannel(0), CompressionBasic.AnimationProgressCompressionInfo);
		}
	}

	bool IOrderableWithInteractionArea.IsPointInsideInteractionArea(Vec3 point)
	{
		GameEntity gameEntity = base.GameEntity.CollectChildrenEntitiesWithTag("ui_interaction").FirstOrDefault();
		if (gameEntity == null)
		{
			return false;
		}
		return gameEntity.GlobalPosition.AsVec2.DistanceSquared(point.AsVec2) < 25f;
	}

	public override TargetFlags GetTargetFlags()
	{
		TargetFlags targetFlags = TargetFlags.None;
		targetFlags |= TargetFlags.IsFlammable;
		targetFlags |= TargetFlags.IsSiegeEngine;
		targetFlags |= TargetFlags.IsAttacker;
		if (HasCompletedAction() || IsDeactivated)
		{
			targetFlags |= TargetFlags.NotAThreat;
		}
		return targetFlags;
	}

	public override float GetTargetValue(List<Vec3> weaponPos)
	{
		return 10f * GetUserMultiplierOfWeapon() * GetDistanceMultiplierOfWeapon(weaponPos[0]);
	}

	protected override float GetDistanceMultiplierOfWeapon(Vec3 weaponPos)
	{
		if (!(GetMinimumDistanceBetweenPositions(weaponPos) < 10f))
		{
			return 0.9f;
		}
		return 1f;
	}

	protected override StandingPoint GetSuitableStandingPointFor(BattleSideEnum side, Agent agent = null, List<Agent> agents = null, List<(Agent, float)> agentValuePairs = null)
	{
		if (side == BattleSideEnum.Attacker)
		{
			return _attackerStandingPoints.FirstOrDefault((StandingPoint sp) => !sp.IsDeactivated && (sp.IsInstantUse || (!sp.HasUser && !sp.HasAIMovingTo)));
		}
		if (_pushingWithForkStandingPoint.IsDeactivated || (!_pushingWithForkStandingPoint.IsInstantUse && (_pushingWithForkStandingPoint.HasUser || _pushingWithForkStandingPoint.HasAIMovingTo)))
		{
			return null;
		}
		return _pushingWithForkStandingPoint;
	}

	public void SetSpawnedFromSpawner()
	{
		_spawnedFromSpawner = true;
	}

	public void AssignParametersFromSpawner(string sideTag, string targetWallSegment, int onWallNavMeshId, float downStateRotationRadian, float upperStateRotationRadian, string barrierTagToRemove, string indestructibleMerlonsTag)
	{
		_sideTag = sideTag;
		_targetWallSegmentTag = targetWallSegment;
		OnWallNavMeshId = onWallNavMeshId;
		_downStateRotationRadian = downStateRotationRadian;
		_upStateRotationRadian = upperStateRotationRadian;
		BarrierTagToRemove = barrierTagToRemove;
		IndestructibleMerlonsTag = indestructibleMerlonsTag;
	}

	public override void OnAfterReadFromNetwork((BaseSynchedMissionObjectReadableRecord, ISynchedMissionObjectReadableRecord) synchedMissionObjectReadableRecord)
	{
		base.OnAfterReadFromNetwork(synchedMissionObjectReadableRecord);
		SiegeLadderRecord siegeLadderRecord = (SiegeLadderRecord)(object)synchedMissionObjectReadableRecord.Item2;
		initialState = ((!siegeLadderRecord.IsStateLand) ? LadderState.OnWall : LadderState.OnLand);
		_state = (LadderState)siegeLadderRecord.State;
		_animationState = (LadderAnimationState)siegeLadderRecord.AnimationState;
		_fallAngularSpeed = siegeLadderRecord.FallAngularSpeed;
		_lastDotProductOfAnimationAndTargetRotation = -1000f;
		siegeLadderRecord.LadderFrame.rotation.Orthonormalize();
		GameEntity gameEntity = _ladderObject.GameEntity;
		MatrixFrame frame = siegeLadderRecord.LadderFrame;
		gameEntity.SetGlobalFrame(in frame);
		if (siegeLadderRecord.LadderAnimationIndex >= 0)
		{
			_ladderSkeleton.SetAnimationAtChannel(siegeLadderRecord.LadderAnimationIndex, 0);
			_ladderSkeleton.SetAnimationParameterAtChannel(0, MBMath.ClampFloat(siegeLadderRecord.LadderAnimationProgress, 0f, 1f));
			_ladderSkeleton.ForceUpdateBoneFrames();
		}
	}

	public bool GetNavmeshFaceIds(out List<int> navmeshFaceIds)
	{
		navmeshFaceIds = new List<int> { OnWallNavMeshId };
		return true;
	}
}
