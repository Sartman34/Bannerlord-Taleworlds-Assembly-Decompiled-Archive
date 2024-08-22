using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Objects.Siege;

namespace TaleWorlds.MountAndBlade;

public class Ballista : RangedSiegeWeapon, ISpawnable
{
	private static readonly ActionIndexCache act_usage_ballista_ammo_pick_up_end = ActionIndexCache.Create("act_usage_ballista_ammo_pick_up_end");

	private static readonly ActionIndexCache act_usage_ballista_ammo_pick_up_start = ActionIndexCache.Create("act_usage_ballista_ammo_pick_up_start");

	private static readonly ActionIndexCache act_usage_ballista_ammo_place_end = ActionIndexCache.Create("act_usage_ballista_ammo_place_end");

	private static readonly ActionIndexCache act_usage_ballista_ammo_place_start = ActionIndexCache.Create("act_usage_ballista_ammo_place_start");

	private static readonly ActionIndexCache act_usage_ballista_idle = ActionIndexCache.Create("act_usage_ballista_idle");

	private static readonly ActionIndexCache act_usage_ballista_reload = ActionIndexCache.Create("act_usage_ballista_reload");

	private static readonly ActionIndexCache act_strike_bent_over = ActionIndexCache.Create("act_strike_bent_over");

	public string NavelTag = "BallistaNavel";

	public string BodyTag = "BallistaBody";

	public float AnimationHeightDifference;

	private MatrixFrame _ballistaBodyInitialLocalFrame;

	private MatrixFrame _ballistaNavelInitialFrame;

	private MatrixFrame _pilotInitialLocalFrame;

	private MatrixFrame _pilotInitialLocalIKFrame;

	private MatrixFrame _missileInitialLocalFrame;

	[EditableScriptComponentVariable(true)]
	protected string IdleActionName = "act_usage_ballista_idle_attacker";

	[EditableScriptComponentVariable(true)]
	protected string ReloadActionName = "act_usage_ballista_reload_attacker";

	[EditableScriptComponentVariable(true)]
	protected string PlaceAmmoStartActionName = "act_usage_ballista_ammo_place_start_attacker";

	[EditableScriptComponentVariable(true)]
	protected string PlaceAmmoEndActionName = "act_usage_ballista_ammo_place_end_attacker";

	[EditableScriptComponentVariable(true)]
	protected string PickUpAmmoStartActionName = "act_usage_ballista_ammo_pick_up_start_attacker";

	[EditableScriptComponentVariable(true)]
	protected string PickUpAmmoEndActionName = "act_usage_ballista_ammo_pick_up_end_attacker";

	private ActionIndexCache _idleAnimationActionIndex;

	private ActionIndexCache _reloadAnimationActionIndex;

	private ActionIndexCache _placeAmmoStartAnimationActionIndex;

	private ActionIndexCache _placeAmmoEndAnimationActionIndex;

	private ActionIndexCache _pickUpAmmoStartAnimationActionIndex;

	private ActionIndexCache _pickUpAmmoEndAnimationActionIndex;

	private float _verticalOffsetAngle;

	[EditableScriptComponentVariable(false)]
	public float HorizontalDirectionRestriction = (float)Math.PI / 2f;

	public float BallistaShootingSpeed = 120f;

	private WeaponState _changeToState = WeaponState.Invalid;

	protected SynchedMissionObject ballistaBody { get; private set; }

	protected SynchedMissionObject ballistaNavel { get; private set; }

	public override float DirectionRestriction => HorizontalDirectionRestriction;

	protected override float ShootingSpeed => BallistaShootingSpeed;

	public override Vec3 CanShootAtPointCheckingOffset => new Vec3(0f, 0f, 0.5f);

	protected override float MaximumBallisticError => 0.5f;

	protected override float HorizontalAimSensitivity => 1f;

	protected override float VerticalAimSensitivity => 1f;

	protected override void RegisterAnimationParameters()
	{
		SkeletonOwnerObjects = new SynchedMissionObject[1];
		Skeletons = new Skeleton[1];
		SkeletonOwnerObjects[0] = ballistaBody;
		Skeletons[0] = ballistaBody.GameEntity.Skeleton;
		base.SkeletonName = "ballista_skeleton";
		base.FireAnimation = "ballista_fire";
		base.FireAnimationIndex = MBAnimation.GetAnimationIndexWithName("ballista_fire");
		base.SetUpAnimation = "ballista_set_up";
		base.SetUpAnimationIndex = MBAnimation.GetAnimationIndexWithName("ballista_set_up");
		_idleAnimationActionIndex = ActionIndexCache.Create(IdleActionName);
		_reloadAnimationActionIndex = ActionIndexCache.Create(ReloadActionName);
		_placeAmmoStartAnimationActionIndex = ActionIndexCache.Create(PlaceAmmoStartActionName);
		_placeAmmoEndAnimationActionIndex = ActionIndexCache.Create(PlaceAmmoEndActionName);
		_pickUpAmmoStartAnimationActionIndex = ActionIndexCache.Create(PickUpAmmoStartActionName);
		_pickUpAmmoEndAnimationActionIndex = ActionIndexCache.Create(PickUpAmmoEndActionName);
	}

	public override SiegeEngineType GetSiegeEngineType()
	{
		return DefaultSiegeEngineTypes.Ballista;
	}

	protected internal override void OnInit()
	{
		ballistaBody = base.GameEntity.CollectObjectsWithTag<SynchedMissionObject>(BodyTag)[0];
		ballistaNavel = base.GameEntity.CollectObjectsWithTag<SynchedMissionObject>(NavelTag)[0];
		RotationObject = this;
		base.OnInit();
		UsesMouseForAiming = true;
		GetSoundEventIndices();
		_ballistaNavelInitialFrame = ballistaNavel.GameEntity.GetFrame();
		MatrixFrame globalFrame = ballistaBody.GameEntity.GetGlobalFrame();
		_ballistaBodyInitialLocalFrame = ballistaBody.GameEntity.GetFrame();
		MatrixFrame globalFrame2 = base.PilotStandingPoint.GameEntity.GetGlobalFrame();
		_pilotInitialLocalFrame = base.PilotStandingPoint.GameEntity.GetFrame();
		_pilotInitialLocalIKFrame = globalFrame2.TransformToLocal(globalFrame);
		_missileInitialLocalFrame = base.Projectile.GameEntity.GetFrame();
		base.PilotStandingPoint.AddComponent(new ClearHandInverseKinematicsOnStopUsageComponent());
		MissileStartingPositionEntityForSimulation = base.Projectile.GameEntity.Parent.GetChildren().FirstOrDefault((GameEntity x) => x.Name == "projectile_leaving_position");
		EnemyRangeToStopUsing = 7f;
		AttackClickWillReload = true;
		WeaponNeedsClickToReload = true;
		SetScriptComponentToTick(GetTickRequirement());
		Vec3 shootingDirection = ShootingDirection;
		Vec3 v = new Vec3(0f, shootingDirection.AsVec2.Length, shootingDirection.z);
		_verticalOffsetAngle = Vec3.AngleBetweenTwoVectors(v, Vec3.Forward);
		ApplyAimChange();
	}

	protected override bool CanRotate()
	{
		return base.State != WeaponState.Shooting;
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		return new BallistaAI(this);
	}

	protected override void OnRangedSiegeWeaponStateChange()
	{
		base.OnRangedSiegeWeaponStateChange();
		switch (base.State)
		{
		case WeaponState.WaitingBeforeProjectileLeaving:
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
			if (base.AmmoCount > 0)
			{
				if (!GameNetwork.IsClientOrReplay)
				{
					ConsumeAmmo();
				}
				else
				{
					SetAmmo(base.AmmoCount - 1);
				}
			}
			break;
		}
	}

	protected override void HandleUserAiming(float dt)
	{
		if (base.PilotAgent == null)
		{
			targetReleaseAngle = 0f;
		}
		base.HandleUserAiming(dt);
	}

	protected override void ApplyAimChange()
	{
		MatrixFrame frame = _ballistaNavelInitialFrame;
		frame.rotation.RotateAboutUp(currentDirection);
		ballistaNavel.GameEntity.SetFrame(ref frame);
		MatrixFrame m = _ballistaNavelInitialFrame.TransformToLocal(_pilotInitialLocalFrame);
		MatrixFrame frame2 = frame.TransformToParent(m);
		base.PilotStandingPoint.GameEntity.SetFrame(ref frame2);
		MatrixFrame frame3 = _ballistaBodyInitialLocalFrame;
		frame3.rotation.RotateAboutSide(0f - currentReleaseAngle + _verticalOffsetAngle);
		ballistaBody.GameEntity.SetFrame(ref frame3);
	}

	protected override void ApplyCurrentDirectionToEntity()
	{
		ApplyAimChange();
	}

	protected override void GetSoundEventIndices()
	{
		MoveSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/ballista/move");
		ReloadSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/ballista/reload");
		ReloadEndSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/siege/ballista/reload_end");
	}

	protected internal override bool IsTargetValid(ITargetable target)
	{
		return !(target is ICastleKeyPosition);
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
		if (_changeToState != WeaponState.Invalid)
		{
			base.State = _changeToState;
			_changeToState = WeaponState.Invalid;
		}
	}

	protected internal override void OnTickParallel(float dt)
	{
		base.OnTickParallel(dt);
		if (!base.GameEntity.IsVisibleIncludeParents())
		{
			return;
		}
		if (base.PilotAgent != null)
		{
			Agent pilotAgent = base.PilotAgent;
			ref MatrixFrame pilotInitialLocalIKFrame = ref _pilotInitialLocalIKFrame;
			MatrixFrame boundEntityGlobalFrame = ballistaBody.GameEntity.GetGlobalFrame();
			pilotAgent.SetHandInverseKinematicsFrameForMissionObjectUsage(in pilotInitialLocalIKFrame, in boundEntityGlobalFrame, AnimationHeightDifference);
			ActionIndexValueCache currentActionValue = base.PilotAgent.GetCurrentActionValue(1);
			if (currentActionValue == _pickUpAmmoEndAnimationActionIndex || currentActionValue == _placeAmmoStartAnimationActionIndex)
			{
				MatrixFrame boneEntitialFrame = base.PilotAgent.AgentVisuals.GetBoneEntitialFrame(base.PilotAgent.Monster.MainHandItemBoneIndex, useBoneMapping: false);
				boneEntitialFrame = base.PilotAgent.AgentVisuals.GetGlobalFrame().TransformToParent(boneEntitialFrame);
				base.Projectile.GameEntity.SetGlobalFrame(in boneEntitialFrame);
			}
			else
			{
				base.Projectile.GameEntity.SetFrame(ref _missileInitialLocalFrame);
			}
		}
		if (GameNetwork.IsClientOrReplay)
		{
			return;
		}
		switch (base.State)
		{
		case WeaponState.Reloading:
			if (base.PilotAgent != null && !base.PilotAgent.SetActionChannel(1, _reloadAnimationActionIndex, ignorePriority: false, 0uL) && base.PilotAgent.Controller != Agent.ControllerType.AI)
			{
				base.PilotAgent.StopUsingGameObjectMT();
			}
			return;
		case WeaponState.LoadingAmmo:
		{
			bool value = false;
			if (base.PilotAgent != null)
			{
				ActionIndexValueCache currentActionValue2 = base.PilotAgent.GetCurrentActionValue(1);
				if (currentActionValue2 != _pickUpAmmoStartAnimationActionIndex && currentActionValue2 != _pickUpAmmoEndAnimationActionIndex && currentActionValue2 != _placeAmmoStartAnimationActionIndex && currentActionValue2 != _placeAmmoEndAnimationActionIndex && !base.PilotAgent.SetActionChannel(1, _pickUpAmmoStartAnimationActionIndex, ignorePriority: false, 0uL) && base.PilotAgent.Controller != Agent.ControllerType.AI)
				{
					base.PilotAgent.StopUsingGameObjectMT();
				}
				else if (currentActionValue2 == _pickUpAmmoEndAnimationActionIndex || currentActionValue2 == _placeAmmoStartAnimationActionIndex)
				{
					value = true;
				}
				else if (currentActionValue2 == _placeAmmoEndAnimationActionIndex)
				{
					value = true;
					_changeToState = WeaponState.WaitingBeforeIdle;
				}
			}
			base.Projectile.SetVisibleSynched(value);
			return;
		}
		case WeaponState.WaitingBeforeIdle:
			if (base.PilotAgent == null)
			{
				_changeToState = WeaponState.Idle;
			}
			else if (base.PilotAgent.GetCurrentActionValue(1) != _placeAmmoEndAnimationActionIndex)
			{
				if (base.PilotAgent.Controller != Agent.ControllerType.AI)
				{
					base.PilotAgent.StopUsingGameObjectMT();
				}
				_changeToState = WeaponState.Idle;
			}
			else if (base.PilotAgent.GetCurrentActionProgress(1) > 0.9999f)
			{
				_changeToState = WeaponState.Idle;
				if (base.PilotAgent != null && !base.PilotAgent.SetActionChannel(1, _idleAnimationActionIndex, ignorePriority: false, 0uL) && base.PilotAgent.Controller != Agent.ControllerType.AI)
				{
					base.PilotAgent.StopUsingGameObjectMT();
				}
			}
			return;
		}
		if (base.PilotAgent == null)
		{
			return;
		}
		if (base.PilotAgent.IsInBeingStruckAction)
		{
			if (base.PilotAgent.GetCurrentActionValue(1) != act_strike_bent_over)
			{
				base.PilotAgent.SetActionChannel(1, act_strike_bent_over, ignorePriority: false, 0uL);
			}
		}
		else if (!base.PilotAgent.SetActionChannel(1, _idleAnimationActionIndex, ignorePriority: false, 0uL) && base.PilotAgent.Controller != Agent.ControllerType.AI)
		{
			base.PilotAgent.StopUsingGameObjectMT();
		}
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		TextObject textObject = new TextObject("{=fEQAPJ2e}{KEY} Use");
		textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
		return textObject;
	}

	public override string GetDescriptionText(GameEntity gameEntity = null)
	{
		return new TextObject("{=abbALYlp}Ballista").ToString();
	}

	protected override void UpdateAmmoMesh()
	{
		int num = 8 - base.AmmoCount;
		foreach (GameEntity child in base.GameEntity.GetChildren())
		{
			for (int i = 0; i < child.MultiMeshComponentCount; i++)
			{
				MetaMesh metaMesh = child.GetMetaMesh(i);
				for (int j = 0; j < metaMesh.MeshCount; j++)
				{
					metaMesh.GetMeshAtIndex(j).SetVectorArgument(0f, num, 0f, 0f);
				}
			}
		}
	}

	public override float ProcessTargetValue(float baseValue, TargetFlags flags)
	{
		if (flags.HasAnyFlag(TargetFlags.NotAThreat))
		{
			return -1000f;
		}
		if (flags.HasAnyFlag(TargetFlags.IsSiegeEngine))
		{
			baseValue *= 0.2f;
		}
		if (flags.HasAnyFlag(TargetFlags.IsStructure))
		{
			baseValue *= 0.05f;
		}
		if (flags.HasAnyFlag(TargetFlags.DebugThreat))
		{
			baseValue *= 10000f;
		}
		return baseValue;
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
		targetFlags |= TargetFlags.IsSmall;
		if (base.IsDestroyed || IsDeactivated)
		{
			targetFlags |= TargetFlags.NotAThreat;
		}
		if (Side == BattleSideEnum.Attacker && DebugSiegeBehavior.DebugDefendState == DebugSiegeBehavior.DebugStateDefender.DebugDefendersToBallistae)
		{
			targetFlags |= TargetFlags.DebugThreat;
		}
		if (Side == BattleSideEnum.Defender && DebugSiegeBehavior.DebugAttackState == DebugSiegeBehavior.DebugStateAttacker.DebugAttackersToBallistae)
		{
			targetFlags |= TargetFlags.DebugThreat;
		}
		return targetFlags;
	}

	public override float GetTargetValue(List<Vec3> weaponPos)
	{
		return 30f * GetUserMultiplierOfWeapon() * GetDistanceMultiplierOfWeapon(weaponPos[0]) * GetHitPointMultiplierOfWeapon();
	}

	public void SetSpawnedFromSpawner()
	{
		_spawnedFromSpawner = true;
	}
}
