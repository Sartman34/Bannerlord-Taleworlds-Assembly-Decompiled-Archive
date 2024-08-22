using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Objects.Siege;

namespace TaleWorlds.MountAndBlade;

public class Mangonel : RangedSiegeWeapon, ISpawnable
{
	private const string BodyTag = "body";

	private const string RopeTag = "rope";

	private const string RotateTag = "rotate";

	private const string LeftTag = "left";

	private const string VerticalAdjusterTag = "vertical_adjuster";

	private static readonly ActionIndexCache act_usage_mangonel_idle = ActionIndexCache.Create("act_usage_mangonel_idle");

	private static readonly ActionIndexCache act_usage_mangonel_load_ammo_begin = ActionIndexCache.Create("act_usage_mangonel_load_ammo_begin");

	private static readonly ActionIndexCache act_usage_mangonel_load_ammo_end = ActionIndexCache.Create("act_usage_mangonel_load_ammo_end");

	private static readonly ActionIndexCache act_pickup_boulder_begin = ActionIndexCache.Create("act_pickup_boulder_begin");

	private static readonly ActionIndexCache act_pickup_boulder_end = ActionIndexCache.Create("act_pickup_boulder_end");

	private static readonly ActionIndexCache act_usage_mangonel_reload = ActionIndexCache.Create("act_usage_mangonel_reload");

	private static readonly ActionIndexCache act_usage_mangonel_reload_2 = ActionIndexCache.Create("act_usage_mangonel_reload_2");

	private static readonly ActionIndexCache act_usage_mangonel_reload_2_idle = ActionIndexCache.Create("act_usage_mangonel_reload_2_idle");

	private static readonly ActionIndexCache act_usage_mangonel_rotate_left = ActionIndexCache.Create("act_usage_mangonel_rotate_left");

	private static readonly ActionIndexCache act_usage_mangonel_rotate_right = ActionIndexCache.Create("act_usage_mangonel_rotate_right");

	private static readonly ActionIndexCache act_usage_mangonel_shoot = ActionIndexCache.Create("act_usage_mangonel_shoot");

	private static readonly ActionIndexCache act_usage_mangonel_big_idle = ActionIndexCache.Create("act_usage_mangonel_big_idle");

	private static readonly ActionIndexCache act_usage_mangonel_big_shoot = ActionIndexCache.Create("act_usage_mangonel_big_shoot");

	private static readonly ActionIndexCache act_usage_mangonel_big_reload = ActionIndexCache.Create("act_usage_mangonel_big_reload");

	private static readonly ActionIndexCache act_usage_mangonel_big_load_ammo_begin = ActionIndexCache.Create("act_usage_mangonel_big_load_ammo_begin");

	private static readonly ActionIndexCache act_usage_mangonel_big_load_ammo_end = ActionIndexCache.Create("act_usage_mangonel_big_load_ammo_end");

	private static readonly ActionIndexCache act_strike_bent_over = ActionIndexCache.Create("act_strike_bent_over");

	private string _missileBoneName = "end_throwarm";

	private List<StandingPoint> _rotateStandingPoints;

	private SynchedMissionObject _body;

	private SynchedMissionObject _rope;

	private GameEntity _verticalAdjuster;

	private MatrixFrame _verticalAdjusterStartingLocalFrame;

	private Skeleton _verticalAdjusterSkeleton;

	private Skeleton _bodySkeleton;

	private float _timeElapsedAfterLoading;

	private MatrixFrame[] _standingPointLocalIKFrames;

	private StandingPoint _reloadWithoutPilot;

	public string MangonelBodySkeleton = "mangonel_skeleton";

	public string MangonelBodyFire = "mangonel_fire";

	public string MangonelBodyReload = "mangonel_set_up";

	public string MangonelRopeFire = "mangonel_holder_fire";

	public string MangonelRopeReload = "mangonel_holder_set_up";

	public string MangonelAimAnimation = "mangonel_a_anglearm_state";

	public string ProjectileBoneName = "end_throwarm";

	public string IdleActionName;

	public string ShootActionName;

	public string Reload1ActionName;

	public string Reload2ActionName;

	public string RotateLeftActionName;

	public string RotateRightActionName;

	public string LoadAmmoBeginActionName;

	public string LoadAmmoEndActionName;

	public string Reload2IdleActionName;

	public float ProjectileSpeed = 40f;

	private ActionIndexCache _idleAnimationActionIndex;

	private ActionIndexCache _shootAnimationActionIndex;

	private ActionIndexCache _reload1AnimationActionIndex;

	private ActionIndexCache _reload2AnimationActionIndex;

	private ActionIndexCache _rotateLeftAnimationActionIndex;

	private ActionIndexCache _rotateRightAnimationActionIndex;

	private ActionIndexCache _loadAmmoBeginAnimationActionIndex;

	private ActionIndexCache _loadAmmoEndAnimationActionIndex;

	private ActionIndexCache _reload2IdleActionIndex;

	private sbyte _missileBoneIndex;

	protected override float MaximumBallisticError => 1.5f;

	protected override float ShootingSpeed => ProjectileSpeed;

	protected override float HorizontalAimSensitivity
	{
		get
		{
			if (_defaultSide == BattleSideEnum.Defender)
			{
				return 0.25f;
			}
			float num = 0.05f;
			foreach (StandingPoint rotateStandingPoint in _rotateStandingPoints)
			{
				if (rotateStandingPoint.HasUser && !rotateStandingPoint.UserAgent.IsInBeingStruckAction)
				{
					num += 0.1f;
				}
			}
			return num;
		}
	}

	protected override float VerticalAimSensitivity => 0.1f;

	protected override Vec3 ShootingDirection
	{
		get
		{
			Mat3 rotation = _body.GameEntity.GetGlobalFrame().rotation;
			rotation.RotateAboutSide(0f - currentReleaseAngle);
			return rotation.TransformToParent(new Vec3(0f, -1f));
		}
	}

	protected override bool HasAmmo
	{
		get
		{
			if (!base.HasAmmo && base.CurrentlyUsedAmmoPickUpPoint == null && !LoadAmmoStandingPoint.HasUser)
			{
				return LoadAmmoStandingPoint.HasAIMovingTo;
			}
			return true;
		}
		set
		{
			base.HasAmmo = value;
		}
	}

	protected override void RegisterAnimationParameters()
	{
		SkeletonOwnerObjects = new SynchedMissionObject[2];
		Skeletons = new Skeleton[2];
		SkeletonNames = new string[1];
		FireAnimations = new string[2];
		FireAnimationIndices = new int[2];
		SetUpAnimations = new string[2];
		SetUpAnimationIndices = new int[2];
		SkeletonOwnerObjects[0] = _body;
		Skeletons[0] = _body.GameEntity.Skeleton;
		SkeletonNames[0] = MangonelBodySkeleton;
		FireAnimations[0] = MangonelBodyFire;
		FireAnimationIndices[0] = MBAnimation.GetAnimationIndexWithName(MangonelBodyFire);
		SetUpAnimations[0] = MangonelBodyReload;
		SetUpAnimationIndices[0] = MBAnimation.GetAnimationIndexWithName(MangonelBodyReload);
		SkeletonOwnerObjects[1] = _rope;
		Skeletons[1] = _rope.GameEntity.Skeleton;
		FireAnimations[1] = MangonelRopeFire;
		FireAnimationIndices[1] = MBAnimation.GetAnimationIndexWithName(MangonelRopeFire);
		SetUpAnimations[1] = MangonelRopeReload;
		SetUpAnimationIndices[1] = MBAnimation.GetAnimationIndexWithName(MangonelRopeReload);
		_missileBoneName = ProjectileBoneName;
		_idleAnimationActionIndex = ActionIndexCache.Create(IdleActionName);
		_shootAnimationActionIndex = ActionIndexCache.Create(ShootActionName);
		_reload1AnimationActionIndex = ActionIndexCache.Create(Reload1ActionName);
		_reload2AnimationActionIndex = ActionIndexCache.Create(Reload2ActionName);
		_rotateLeftAnimationActionIndex = ActionIndexCache.Create(RotateLeftActionName);
		_rotateRightAnimationActionIndex = ActionIndexCache.Create(RotateRightActionName);
		_loadAmmoBeginAnimationActionIndex = ActionIndexCache.Create(LoadAmmoBeginActionName);
		_loadAmmoEndAnimationActionIndex = ActionIndexCache.Create(LoadAmmoEndActionName);
		_reload2IdleActionIndex = ActionIndexCache.Create(Reload2IdleActionName);
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		return new MangonelAI(this);
	}

	public override void AfterMissionStart()
	{
		if (AmmoPickUpStandingPoints != null)
		{
			foreach (StandingPointWithWeaponRequirement ammoPickUpStandingPoint in AmmoPickUpStandingPoints)
			{
				ammoPickUpStandingPoint.LockUserFrames = true;
			}
		}
		UpdateProjectilePosition();
	}

	public override SiegeEngineType GetSiegeEngineType()
	{
		if (_defaultSide != BattleSideEnum.Attacker)
		{
			return DefaultSiegeEngineTypes.Catapult;
		}
		return DefaultSiegeEngineTypes.Onager;
	}

	protected internal override void OnInit()
	{
		List<SynchedMissionObject> list = base.GameEntity.CollectObjectsWithTag<SynchedMissionObject>("rope");
		if (list.Count > 0)
		{
			_rope = list[0];
		}
		list = base.GameEntity.CollectObjectsWithTag<SynchedMissionObject>("body");
		_body = list[0];
		_bodySkeleton = _body.GameEntity.Skeleton;
		RotationObject = _body;
		List<GameEntity> list2 = base.GameEntity.CollectChildrenEntitiesWithTag("vertical_adjuster");
		_verticalAdjuster = list2[0];
		_verticalAdjusterSkeleton = _verticalAdjuster.Skeleton;
		if (_verticalAdjusterSkeleton != null)
		{
			_verticalAdjusterSkeleton.SetAnimationAtChannel(MangonelAimAnimation, 0);
		}
		_verticalAdjusterStartingLocalFrame = _verticalAdjuster.GetFrame();
		_verticalAdjusterStartingLocalFrame = _body.GameEntity.GetBoneEntitialFrameWithIndex(0).TransformToLocal(_verticalAdjusterStartingLocalFrame);
		base.OnInit();
		timeGapBetweenShootActionAndProjectileLeaving = 0.23f;
		timeGapBetweenShootingEndAndReloadingStart = 0f;
		_rotateStandingPoints = new List<StandingPoint>();
		if (base.StandingPoints != null)
		{
			foreach (StandingPoint standingPoint in base.StandingPoints)
			{
				if (standingPoint.GameEntity.HasTag("rotate"))
				{
					if (standingPoint.GameEntity.HasTag("left") && _rotateStandingPoints.Count > 0)
					{
						_rotateStandingPoints.Insert(0, standingPoint);
					}
					else
					{
						_rotateStandingPoints.Add(standingPoint);
					}
				}
			}
			MatrixFrame globalFrame = _body.GameEntity.GetGlobalFrame();
			_standingPointLocalIKFrames = new MatrixFrame[base.StandingPoints.Count];
			for (int i = 0; i < base.StandingPoints.Count; i++)
			{
				_standingPointLocalIKFrames[i] = base.StandingPoints[i].GameEntity.GetGlobalFrame().TransformToLocal(globalFrame);
				base.StandingPoints[i].AddComponent(new ClearHandInverseKinematicsOnStopUsageComponent());
			}
		}
		_missileBoneIndex = Skeleton.GetBoneIndexFromName(Skeletons[0].GetName(), _missileBoneName);
		ApplyAimChange();
		foreach (StandingPoint reloadStandingPoint in ReloadStandingPoints)
		{
			if (reloadStandingPoint != base.PilotStandingPoint)
			{
				_reloadWithoutPilot = reloadStandingPoint;
			}
		}
		if (!GameNetwork.IsClientOrReplay)
		{
			SetActivationLoadAmmoPoint(activate: false);
		}
		EnemyRangeToStopUsing = 9f;
		SetScriptComponentToTick(GetTickRequirement());
	}

	protected internal override void OnEditorInit()
	{
	}

	protected override bool CanRotate()
	{
		if (base.State != 0 && base.State != WeaponState.LoadingAmmo)
		{
			return base.State == WeaponState.WaitingBeforeIdle;
		}
		return true;
	}

	public override TickRequirement GetTickRequirement()
	{
		if (base.GameEntity.IsVisibleIncludeParents())
		{
			return base.GetTickRequirement() | TickRequirement.Tick | TickRequirement.TickParallel;
		}
		return base.GetTickRequirement();
	}

	protected internal override void OnTick(float dt)
	{
		base.OnTick(dt);
		if (!base.GameEntity.IsVisibleIncludeParents())
		{
			return;
		}
		if (!GameNetwork.IsClientOrReplay)
		{
			foreach (StandingPointWithWeaponRequirement ammoPickUpStandingPoint in AmmoPickUpStandingPoints)
			{
				if (!ammoPickUpStandingPoint.HasUser)
				{
					continue;
				}
				Agent userAgent = ammoPickUpStandingPoint.UserAgent;
				ActionIndexValueCache currentActionValue = userAgent.GetCurrentActionValue(1);
				if (currentActionValue == act_pickup_boulder_begin)
				{
					continue;
				}
				if (currentActionValue == act_pickup_boulder_end)
				{
					MissionWeapon weapon = new MissionWeapon(OriginalMissileItem, null, null, 1);
					userAgent.EquipWeaponToExtraSlotAndWield(ref weapon);
					userAgent.StopUsingGameObject(isSuccessful: true, Agent.StopUsingGameObjectFlags.None);
					ConsumeAmmo();
					if (userAgent.IsAIControlled)
					{
						if (!LoadAmmoStandingPoint.HasUser && !LoadAmmoStandingPoint.IsDeactivated)
						{
							userAgent.AIMoveToGameObjectEnable(LoadAmmoStandingPoint, this, base.Ai.GetScriptedFrameFlags(userAgent));
							continue;
						}
						if (ReloaderAgentOriginalPoint != null && !ReloaderAgentOriginalPoint.HasUser && !ReloaderAgentOriginalPoint.HasAIMovingTo)
						{
							userAgent.AIMoveToGameObjectEnable(ReloaderAgentOriginalPoint, this, base.Ai.GetScriptedFrameFlags(userAgent));
							continue;
						}
						ReloaderAgent?.Formation?.AttachUnit(ReloaderAgent);
						ReloaderAgent = null;
					}
				}
				else if (!userAgent.SetActionChannel(1, act_pickup_boulder_begin, ignorePriority: false, 0uL) && userAgent.Controller != Agent.ControllerType.AI)
				{
					userAgent.StopUsingGameObject();
				}
			}
		}
		switch (base.State)
		{
		case WeaponState.WaitingBeforeIdle:
			_timeElapsedAfterLoading += dt;
			if (_timeElapsedAfterLoading > 1f)
			{
				base.State = WeaponState.Idle;
			}
			break;
		case WeaponState.LoadingAmmo:
			if (GameNetwork.IsClientOrReplay)
			{
				break;
			}
			if (LoadAmmoStandingPoint.HasUser)
			{
				Agent userAgent2 = LoadAmmoStandingPoint.UserAgent;
				if (userAgent2.GetCurrentActionValue(1) == _loadAmmoEndAnimationActionIndex)
				{
					EquipmentIndex wieldedItemIndex = userAgent2.GetWieldedItemIndex(Agent.HandIndex.MainHand);
					if (wieldedItemIndex != EquipmentIndex.None && userAgent2.Equipment[wieldedItemIndex].CurrentUsageItem.WeaponClass == OriginalMissileItem.PrimaryWeapon.WeaponClass)
					{
						ChangeProjectileEntityServer(userAgent2, userAgent2.Equipment[wieldedItemIndex].Item.StringId);
						userAgent2.RemoveEquippedWeapon(wieldedItemIndex);
						_timeElapsedAfterLoading = 0f;
						base.Projectile.SetVisibleSynched(value: true);
						base.State = WeaponState.WaitingBeforeIdle;
					}
					else
					{
						userAgent2.StopUsingGameObject(isSuccessful: true, Agent.StopUsingGameObjectFlags.None);
						if (!userAgent2.IsPlayerControlled)
						{
							SendAgentToAmmoPickup(userAgent2);
						}
					}
				}
				else
				{
					if (!(userAgent2.GetCurrentActionValue(1) != _loadAmmoBeginAnimationActionIndex) || userAgent2.SetActionChannel(1, _loadAmmoBeginAnimationActionIndex, ignorePriority: false, 0uL))
					{
						break;
					}
					for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
					{
						if (!userAgent2.Equipment[equipmentIndex].IsEmpty && userAgent2.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == OriginalMissileItem.PrimaryWeapon.WeaponClass)
						{
							userAgent2.RemoveEquippedWeapon(equipmentIndex);
						}
					}
					userAgent2.StopUsingGameObject(isSuccessful: true, Agent.StopUsingGameObjectFlags.None);
					if (!userAgent2.IsPlayerControlled)
					{
						SendAgentToAmmoPickup(userAgent2);
					}
				}
			}
			else if (LoadAmmoStandingPoint.HasAIMovingTo)
			{
				Agent movingAgent = LoadAmmoStandingPoint.MovingAgent;
				EquipmentIndex wieldedItemIndex2 = movingAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
				if (wieldedItemIndex2 == EquipmentIndex.None || movingAgent.Equipment[wieldedItemIndex2].CurrentUsageItem.WeaponClass != OriginalMissileItem.PrimaryWeapon.WeaponClass)
				{
					movingAgent.StopUsingGameObject(isSuccessful: true, Agent.StopUsingGameObjectFlags.None);
					SendAgentToAmmoPickup(movingAgent);
				}
			}
			break;
		case WeaponState.Reloading:
		case WeaponState.ReloadingPaused:
			break;
		}
	}

	protected internal override void OnTickParallel(float dt)
	{
		base.OnTickParallel(dt);
		if (!base.GameEntity.IsVisibleIncludeParents())
		{
			return;
		}
		if (base.State == WeaponState.WaitingBeforeProjectileLeaving)
		{
			UpdateProjectilePosition();
		}
		if (_verticalAdjusterSkeleton != null)
		{
			float parameter = MBMath.ClampFloat((currentReleaseAngle - BottomReleaseAngleRestriction) / (TopReleaseAngleRestriction - BottomReleaseAngleRestriction), 0f, 1f);
			_verticalAdjusterSkeleton.SetAnimationParameterAtChannel(0, parameter);
		}
		MatrixFrame frame = Skeletons[0].GetBoneEntitialFrameWithIndex(0).TransformToParent(_verticalAdjusterStartingLocalFrame);
		_verticalAdjuster.SetFrame(ref frame);
		MatrixFrame boundEntityGlobalFrame = _body.GameEntity.GetGlobalFrame();
		for (int i = 0; i < base.StandingPoints.Count; i++)
		{
			if (!base.StandingPoints[i].HasUser)
			{
				continue;
			}
			if (base.StandingPoints[i].UserAgent.IsInBeingStruckAction)
			{
				base.StandingPoints[i].UserAgent.ClearHandInverseKinematics();
			}
			else if (base.StandingPoints[i] != base.PilotStandingPoint)
			{
				if (base.StandingPoints[i].UserAgent.GetCurrentActionValue(1) != _reload2IdleActionIndex)
				{
					base.StandingPoints[i].UserAgent.SetHandInverseKinematicsFrameForMissionObjectUsage(in _standingPointLocalIKFrames[i], in boundEntityGlobalFrame);
				}
				else
				{
					base.StandingPoints[i].UserAgent.ClearHandInverseKinematics();
				}
			}
			else
			{
				base.StandingPoints[i].UserAgent.SetHandInverseKinematicsFrameForMissionObjectUsage(in _standingPointLocalIKFrames[i], in boundEntityGlobalFrame);
			}
		}
		if (!GameNetwork.IsClientOrReplay)
		{
			for (int j = 0; j < _rotateStandingPoints.Count; j++)
			{
				StandingPoint standingPoint = _rotateStandingPoints[j];
				if (standingPoint.HasUser && !standingPoint.UserAgent.SetActionChannel(1, (j == 0) ? _rotateLeftAnimationActionIndex : _rotateRightAnimationActionIndex, ignorePriority: false, 0uL) && standingPoint.UserAgent.Controller != Agent.ControllerType.AI)
				{
					standingPoint.UserAgent.StopUsingGameObjectMT();
				}
			}
			if (base.PilotAgent != null)
			{
				ActionIndexValueCache currentActionValue = base.PilotAgent.GetCurrentActionValue(1);
				if (base.State == WeaponState.WaitingBeforeProjectileLeaving)
				{
					if (base.PilotAgent.IsInBeingStruckAction)
					{
						if (currentActionValue != ActionIndexValueCache.act_none && currentActionValue != act_strike_bent_over)
						{
							base.PilotAgent.SetActionChannel(1, act_strike_bent_over, ignorePriority: false, 0uL);
						}
					}
					else if (!base.PilotAgent.SetActionChannel(1, _shootAnimationActionIndex, ignorePriority: false, 0uL) && base.PilotAgent.Controller != Agent.ControllerType.AI)
					{
						base.PilotAgent.StopUsingGameObjectMT();
					}
				}
				else if (!base.PilotAgent.SetActionChannel(1, _idleAnimationActionIndex, ignorePriority: false, 0uL) && currentActionValue != _reload1AnimationActionIndex && currentActionValue != _shootAnimationActionIndex && base.PilotAgent.Controller != Agent.ControllerType.AI)
				{
					base.PilotAgent.StopUsingGameObjectMT();
				}
			}
			if (_reloadWithoutPilot.HasUser)
			{
				Agent userAgent = _reloadWithoutPilot.UserAgent;
				if (!userAgent.SetActionChannel(1, _reload2IdleActionIndex, ignorePriority: false, 0uL) && userAgent.GetCurrentActionValue(1) != _reload2AnimationActionIndex && userAgent.Controller != Agent.ControllerType.AI)
				{
					userAgent.StopUsingGameObjectMT();
				}
			}
		}
		WeaponState state = base.State;
		if (state != WeaponState.Reloading)
		{
			return;
		}
		foreach (StandingPoint reloadStandingPoint in ReloadStandingPoints)
		{
			if (!reloadStandingPoint.HasUser)
			{
				continue;
			}
			ActionIndexValueCache currentActionValue2 = reloadStandingPoint.UserAgent.GetCurrentActionValue(1);
			if (currentActionValue2 == _reload1AnimationActionIndex || currentActionValue2 == _reload2AnimationActionIndex)
			{
				reloadStandingPoint.UserAgent.SetCurrentActionProgress(1, _bodySkeleton.GetAnimationParameterAtChannel(0));
			}
			else if (!GameNetwork.IsClientOrReplay)
			{
				ActionIndexCache actionIndexCache = ((reloadStandingPoint == base.PilotStandingPoint) ? _reload1AnimationActionIndex : _reload2AnimationActionIndex);
				if (!reloadStandingPoint.UserAgent.SetActionChannel(1, actionIndexCache, ignorePriority: false, 0uL, 0f, 1f, -0.2f, 0.4f, _bodySkeleton.GetAnimationParameterAtChannel(0)) && reloadStandingPoint.UserAgent.Controller != Agent.ControllerType.AI)
				{
					reloadStandingPoint.UserAgent.StopUsingGameObjectMT();
				}
			}
		}
	}

	protected override void SetActivationLoadAmmoPoint(bool activate)
	{
		LoadAmmoStandingPoint.SetIsDeactivatedSynched(!activate);
	}

	protected override void UpdateProjectilePosition()
	{
		MatrixFrame frame = Skeletons[0].GetBoneEntitialFrameWithIndex(_missileBoneIndex);
		base.Projectile.GameEntity.SetFrame(ref frame);
	}

	protected override void OnRangedSiegeWeaponStateChange()
	{
		base.OnRangedSiegeWeaponStateChange();
		switch (base.State)
		{
		case WeaponState.WaitingBeforeIdle:
			UpdateProjectilePosition();
			break;
		case WeaponState.Shooting:
			if (!GameNetwork.IsClientOrReplay)
			{
				base.Projectile.SetVisibleSynched(value: false);
			}
			else
			{
				base.Projectile.GameEntity.SetVisibilityExcludeParents(visible: false);
			}
			break;
		case WeaponState.Idle:
			if (!GameNetwork.IsClientOrReplay)
			{
				base.Projectile.SetVisibleSynched(value: true);
			}
			else
			{
				base.Projectile.GameEntity.SetVisibilityExcludeParents(visible: true);
			}
			break;
		}
	}

	protected override void GetSoundEventIndices()
	{
		MoveSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/move");
		ReloadSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/reload");
		ReloadEndSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/mangonel/reload_end");
	}

	protected override void ApplyAimChange()
	{
		base.ApplyAimChange();
		ShootingDirection.Normalize();
	}

	public override string GetDescriptionText(GameEntity gameEntity = null)
	{
		if (!gameEntity.HasTag(AmmoPickUpTag))
		{
			return new TextObject("{=NbpcDXtJ}Mangonel").ToString();
		}
		return new TextObject("{=pzfbPbWW}Boulder").ToString();
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		TextObject textObject = (usableGameObject.GameEntity.HasTag("reload") ? new TextObject((base.PilotStandingPoint == usableGameObject) ? "{=fEQAPJ2e}{KEY} Use" : "{=Na81xuXn}{KEY} Rearm") : (usableGameObject.GameEntity.HasTag("rotate") ? new TextObject("{=5wx4BF5h}{KEY} Rotate") : (usableGameObject.GameEntity.HasTag(AmmoPickUpTag) ? new TextObject("{=bNYm3K6b}{KEY} Pick Up") : ((!usableGameObject.GameEntity.HasTag("ammoload")) ? new TextObject("{=fEQAPJ2e}{KEY} Use") : new TextObject("{=ibC4xPoo}{KEY} Load Ammo")))));
		textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
		return textObject;
	}

	public override TargetFlags GetTargetFlags()
	{
		TargetFlags targetFlags = TargetFlags.None;
		targetFlags |= TargetFlags.IsFlammable;
		targetFlags |= TargetFlags.IsSiegeEngine;
		if (Side == BattleSideEnum.Attacker)
		{
			targetFlags |= TargetFlags.IsAttacker;
		}
		if (base.IsDestroyed || IsDeactivated)
		{
			targetFlags |= TargetFlags.NotAThreat;
		}
		if (Side == BattleSideEnum.Attacker && DebugSiegeBehavior.DebugDefendState == DebugSiegeBehavior.DebugStateDefender.DebugDefendersToMangonels)
		{
			targetFlags |= TargetFlags.DebugThreat;
		}
		if (Side == BattleSideEnum.Defender && DebugSiegeBehavior.DebugAttackState == DebugSiegeBehavior.DebugStateAttacker.DebugAttackersToMangonels)
		{
			targetFlags |= TargetFlags.DebugThreat;
		}
		return targetFlags;
	}

	public override float GetTargetValue(List<Vec3> weaponPos)
	{
		return 40f * GetUserMultiplierOfWeapon() * GetDistanceMultiplierOfWeapon(weaponPos[0]) * GetHitPointMultiplierOfWeapon();
	}

	public override float ProcessTargetValue(float baseValue, TargetFlags flags)
	{
		if (flags.HasAnyFlag(TargetFlags.NotAThreat))
		{
			return -1000f;
		}
		if (flags.HasAnyFlag(TargetFlags.IsSiegeEngine))
		{
			baseValue *= 10000f;
		}
		if (flags.HasAnyFlag(TargetFlags.IsStructure))
		{
			baseValue *= 2.5f;
		}
		if (flags.HasAnyFlag(TargetFlags.IsSmall))
		{
			baseValue *= 8f;
		}
		if (flags.HasAnyFlag(TargetFlags.IsMoving))
		{
			baseValue *= 8f;
		}
		if (flags.HasAnyFlag(TargetFlags.DebugThreat))
		{
			baseValue *= 10000f;
		}
		if (flags.HasAnyFlag(TargetFlags.IsSiegeTower))
		{
			baseValue *= 8f;
		}
		return baseValue;
	}

	protected override float GetDetachmentWeightAux(BattleSideEnum side)
	{
		return GetDetachmentWeightAuxForExternalAmmoWeapons(side);
	}

	public void SetSpawnedFromSpawner()
	{
		_spawnedFromSpawner = true;
	}
}
