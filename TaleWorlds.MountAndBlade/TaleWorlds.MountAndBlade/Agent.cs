using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetworkMessages.FromClient;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public sealed class Agent : DotNetObject, IAgent, IFocusable, IUsable, IFormationUnit, ITrackableBase
{
	public class Hitter
	{
		public const float AssistMinDamage = 35f;

		public readonly MissionPeer HitterPeer;

		public readonly bool IsFriendlyHit;

		public float Damage { get; private set; }

		public Hitter(MissionPeer peer, float damage, bool isFriendlyHit)
		{
			HitterPeer = peer;
			Damage = damage;
			IsFriendlyHit = isFriendlyHit;
		}

		public void IncreaseDamage(float amount)
		{
			Damage += amount;
		}
	}

	public struct AgentLastHitInfo
	{
		private BasicMissionTimer _lastBlowTimer;

		public int LastBlowOwnerId { get; private set; }

		public AgentAttackType LastBlowAttackType { get; private set; }

		public bool CanOverrideBlow
		{
			get
			{
				if (LastBlowOwnerId >= 0)
				{
					return _lastBlowTimer.ElapsedTime <= 5f;
				}
				return false;
			}
		}

		public void Initialize()
		{
			LastBlowOwnerId = -1;
			LastBlowAttackType = AgentAttackType.Standard;
			_lastBlowTimer = new BasicMissionTimer();
		}

		public void RegisterLastBlow(int ownerId, AgentAttackType attackType)
		{
			_lastBlowTimer.Reset();
			LastBlowOwnerId = ownerId;
			LastBlowAttackType = attackType;
		}
	}

	public struct AgentPropertiesModifiers
	{
		public bool resetAiWaitBeforeShootFactor;
	}

	public struct StackArray8Agent
	{
		private Agent _element0;

		private Agent _element1;

		private Agent _element2;

		private Agent _element3;

		private Agent _element4;

		private Agent _element5;

		private Agent _element6;

		private Agent _element7;

		public const int Length = 8;

		public Agent this[int index]
		{
			get
			{
				return index switch
				{
					0 => _element0, 
					1 => _element1, 
					2 => _element2, 
					3 => _element3, 
					4 => _element4, 
					5 => _element5, 
					6 => _element6, 
					7 => _element7, 
					_ => null, 
				};
			}
			set
			{
				switch (index)
				{
				case 0:
					_element0 = value;
					break;
				case 1:
					_element1 = value;
					break;
				case 2:
					_element2 = value;
					break;
				case 3:
					_element3 = value;
					break;
				case 4:
					_element4 = value;
					break;
				case 5:
					_element5 = value;
					break;
				case 6:
					_element6 = value;
					break;
				case 7:
					_element7 = value;
					break;
				}
			}
		}
	}

	public enum ActionStage
	{
		None = -1,
		AttackReady,
		AttackQuickReady,
		AttackRelease,
		ReloadMidPhase,
		ReloadLastPhase,
		Defend,
		DefendParry,
		NumActionStages
	}

	[Flags]
	public enum AIScriptedFrameFlags
	{
		None = 0,
		GoToPosition = 1,
		NoAttack = 2,
		ConsiderRotation = 4,
		NeverSlowDown = 8,
		DoNotRun = 0x10,
		GoWithoutMount = 0x20,
		RangerCanMoveForClearTarget = 0x80,
		InConversation = 0x100,
		Crouch = 0x200
	}

	[Flags]
	public enum AISpecialCombatModeFlags
	{
		None = 0,
		AttackEntity = 1,
		SurroundAttackEntity = 2,
		IgnoreAmmoLimitForRangeCalculation = 0x400
	}

	[Flags]
	[EngineStruct("Ai_state_flag", false)]
	public enum AIStateFlag : uint
	{
		None = 0u,
		Cautious = 1u,
		Alarmed = 2u,
		Paused = 4u,
		UseObjectMoving = 8u,
		UseObjectUsing = 0x10u,
		UseObjectWaiting = 0x20u,
		Guard = 0x40u,
		ColumnwiseFollow = 0x80u
	}

	public enum WatchState
	{
		Patrolling,
		Cautious,
		Alarmed
	}

	public enum MortalityState
	{
		Mortal,
		Invulnerable,
		Immortal
	}

	[EngineStruct("Agent_controller_type", false)]
	public enum ControllerType
	{
		None,
		AI,
		Player,
		Count
	}

	public enum CreationType
	{
		Invalid,
		FromRoster,
		FromHorseObj,
		FromCharacterObj
	}

	[Flags]
	public enum EventControlFlag : uint
	{
		Dismount = 1u,
		Mount = 2u,
		Rear = 4u,
		Jump = 8u,
		Wield0 = 0x10u,
		Wield1 = 0x20u,
		Wield2 = 0x40u,
		Wield3 = 0x80u,
		Sheath0 = 0x100u,
		Sheath1 = 0x200u,
		ToggleAlternativeWeapon = 0x400u,
		Walk = 0x800u,
		Run = 0x1000u,
		Crouch = 0x2000u,
		Stand = 0x4000u,
		Kick = 0x8000u,
		DoubleTapToDirectionUp = 0x10000u,
		DoubleTapToDirectionDown = 0x20000u,
		DoubleTapToDirectionLeft = 0x30000u,
		DoubleTapToDirectionRight = 0x40000u,
		DoubleTapToDirectionMask = 0x70000u
	}

	public enum FacialAnimChannel
	{
		High,
		Mid,
		Low,
		num_facial_anim_channels
	}

	[EngineStruct("Action_code_type", false)]
	public enum ActionCodeType
	{
		Other = 0,
		DefendFist = 1,
		DefendShield = 2,
		DefendForward2h = 3,
		DefendUp2h = 4,
		DefendRight2h = 5,
		DefendLeft2h = 6,
		DefendForward1h = 7,
		DefendUp1h = 8,
		DefendRight1h = 9,
		DefendLeft1h = 10,
		DefendForwardStaff = 11,
		DefendUpStaff = 12,
		DefendRightStaff = 13,
		DefendLeftStaff = 14,
		ReadyRanged = 15,
		ReleaseRanged = 16,
		ReleaseThrowing = 17,
		Reload = 18,
		ReadyMelee = 19,
		ReleaseMelee = 20,
		ParriedMelee = 21,
		BlockedMelee = 22,
		Fall = 23,
		JumpStart = 24,
		Jump = 25,
		JumpEnd = 26,
		JumpEndHard = 27,
		Kick = 28,
		KickContinue = 29,
		KickHit = 30,
		WeaponBash = 31,
		PassiveUsage = 32,
		EquipUnequip = 33,
		Idle = 34,
		Guard = 35,
		Mount = 36,
		Dismount = 37,
		Dash = 38,
		MountQuickStop = 39,
		HitObject = 40,
		Sit = 41,
		SitOnTheFloor = 42,
		SitOnAThrone = 43,
		LadderRaise = 44,
		LadderRaiseEnd = 45,
		Rear = 46,
		StrikeLight = 47,
		StrikeMedium = 48,
		StrikeHeavy = 49,
		StrikeKnockBack = 50,
		MountStrike = 51,
		Count = 52,
		StrikeBegin = 47,
		StrikeEnd = 51,
		DefendAllBegin = 1,
		DefendAllEnd = 15,
		AttackMeleeAllBegin = 19,
		AttackMeleeAllEnd = 23,
		CombatAllBegin = 1,
		CombatAllEnd = 23,
		JumpAllBegin = 24,
		JumpAllEnd = 28
	}

	[EngineStruct("Agent_guard_mode", false)]
	public enum GuardMode
	{
		None = -1,
		Up,
		Down,
		Left,
		Right
	}

	public enum HandIndex
	{
		MainHand,
		OffHand
	}

	[EngineStruct("rglInt8", false)]
	public enum KillInfo : sbyte
	{
		Invalid = -1,
		Headshot,
		CouchedLance,
		Punch,
		MountHit,
		Bow,
		Crossbow,
		ThrowingAxe,
		ThrowingKnife,
		Javelin,
		Stone,
		Pistol,
		Musket,
		OneHandedSword,
		TwoHandedSword,
		OneHandedAxe,
		TwoHandedAxe,
		Mace,
		Spear,
		Morningstar,
		Maul,
		Backstabbed,
		Gravity,
		ShieldBash,
		WeaponBash,
		Kick,
		TeamSwitch
	}

	public enum MovementBehaviorType
	{
		Engaged,
		Idle,
		Flee
	}

	[Flags]
	public enum MovementControlFlag : uint
	{
		Forward = 1u,
		Backward = 2u,
		StrafeRight = 4u,
		StrafeLeft = 8u,
		TurnRight = 0x10u,
		TurnLeft = 0x20u,
		AttackLeft = 0x40u,
		AttackRight = 0x80u,
		AttackUp = 0x100u,
		AttackDown = 0x200u,
		DefendLeft = 0x400u,
		DefendRight = 0x800u,
		DefendUp = 0x1000u,
		DefendDown = 0x2000u,
		DefendAuto = 0x4000u,
		DefendBlock = 0x8000u,
		Action = 0x10000u,
		AttackMask = 0x3C0u,
		DefendMask = 0x7C00u,
		DefendDirMask = 0x3C00u,
		MoveMask = 0x3Fu
	}

	public enum UnderAttackType
	{
		NotUnderAttack,
		UnderMeleeAttack,
		UnderRangedAttack
	}

	[EngineStruct("Usage_direction", false)]
	public enum UsageDirection
	{
		None = -1,
		AttackUp = 0,
		AttackDown = 1,
		AttackLeft = 2,
		AttackRight = 3,
		AttackBegin = 0,
		AttackEnd = 4,
		DefendUp = 4,
		DefendDown = 5,
		DefendLeft = 6,
		DefendRight = 7,
		DefendBegin = 4,
		DefendAny = 8,
		DefendEnd = 9,
		AttackAny = 9
	}

	[EngineStruct("Weapon_wield_action_type", false)]
	public enum WeaponWieldActionType
	{
		WithAnimation,
		Instant,
		InstantAfterPickUp,
		WithAnimationUninterruptible
	}

	[Flags]
	public enum StopUsingGameObjectFlags : byte
	{
		None = 0,
		AutoAttachAfterStoppingUsingGameObject = 1,
		DoNotWieldWeaponAfterStoppingUsingGameObject = 2,
		DefendAfterStoppingUsingGameObject = 4
	}

	public delegate void OnAgentHealthChangedDelegate(Agent agent, float oldHealth, float newHealth);

	public delegate void OnMountHealthChangedDelegate(Agent agent, Agent mount, float oldHealth, float newHealth);

	public delegate void OnMainAgentWieldedItemChangeDelegate();

	public const float BecomeTeenagerAge = 14f;

	public const float MaxMountInteractionDistance = 1.75f;

	public const float DismountVelocityLimit = 0.5f;

	public const float HealthDyingThreshold = 1f;

	public const float CachedAndFormationValuesUpdateTime = 0.5f;

	public const float MaxInteractionDistance = 3f;

	public const float MaxFocusDistance = 10f;

	private const float ChainAttackDetectionTimeout = 0.75f;

	public static readonly ActionIndexCache[] DefaultTauntActions = new ActionIndexCache[4]
	{
		ActionIndexCache.Create("act_taunt_cheer_1"),
		ActionIndexCache.Create("act_taunt_cheer_2"),
		ActionIndexCache.Create("act_taunt_cheer_3"),
		ActionIndexCache.Create("act_taunt_cheer_4")
	};

	private static readonly object _stopUsingGameObjectLock = new object();

	private static readonly object _pathCheckObjectLock = new object();

	public OnMainAgentWieldedItemChangeDelegate OnMainAgentWieldedItemChange;

	public Action OnAgentMountedStateChanged;

	public Action OnAgentWieldedItemChange;

	public float LastDetachmentTickAgentTime;

	public MissionPeer OwningAgentMissionPeer;

	public MissionRepresentativeBase MissionRepresentative;

	private readonly MBList<AgentComponent> _components;

	private readonly CreationType _creationType;

	private readonly List<AgentController> _agentControllers;

	private readonly Timer _cachedAndFormationValuesUpdateTimer;

	private ControllerType _agentControllerType = ControllerType.AI;

	private Agent _cachedMountAgent;

	private Agent _cachedRiderAgent;

	private BasicCharacterObject _character;

	private uint? _clothingColor1;

	private uint? _clothingColor2;

	private EquipmentIndex _equipmentOnMainHandBeforeUsingObject;

	private EquipmentIndex _equipmentOnOffHandBeforeUsingObject;

	private float _defensiveness;

	private UIntPtr _positionPointer;

	private UIntPtr _pointer;

	private UIntPtr _flagsPointer;

	private UIntPtr _indexPointer;

	private UIntPtr _statePointer;

	private float _lastQuickReadyDetectedTime;

	private Agent _lookAgentCache;

	private IDetachment _detachment;

	private readonly MBList<Hitter> _hitterList;

	private List<(MissionWeapon, MatrixFrame, sbyte)> _attachedWeapons;

	private float _health;

	private MissionPeer _missionPeer;

	private TextObject _name;

	private float _removalTime;

	private List<CompositeComponent> _synchedBodyComponents;

	private Formation _formation;

	private bool _checkIfTargetFrameIsChanged;

	private AgentPropertiesModifiers _propertyModifiers;

	private int _usedObjectPreferenceIndex = -1;

	private bool _isDeleted;

	private bool _wantsToYell;

	private float _yellTimer;

	private Vec3 _lastSynchedTargetDirection;

	private Vec2 _lastSynchedTargetPosition;

	private AgentLastHitInfo _lastHitInfo;

	private ClothSimulatorComponent _capeClothSimulator;

	private bool _isRemoved;

	private WeakReference<MBAgentVisuals> _visualsWeakRef = new WeakReference<MBAgentVisuals>(null);

	private int _creationIndex;

	private bool _canLeadFormationsRemotely;

	private bool _isDetachableFromFormation = true;

	private ItemObject _formationBanner;

	public float DetachmentWeight;

	public static Agent Main => Mission.Current?.MainAgent;

	public bool IsPlayerControlled
	{
		get
		{
			if (!IsMine)
			{
				return MissionPeer != null;
			}
			return true;
		}
	}

	public bool IsMine => Controller == ControllerType.Player;

	public bool IsMainAgent => this == Main;

	public bool IsHuman => (GetAgentFlags() & AgentFlag.IsHumanoid) != 0;

	public bool IsMount => (GetAgentFlags() & AgentFlag.Mountable) != 0;

	public bool IsAIControlled
	{
		get
		{
			if (Controller == ControllerType.AI)
			{
				return !GameNetwork.IsClientOrReplay;
			}
			return false;
		}
	}

	public bool IsPlayerTroop
	{
		get
		{
			if (!GameNetwork.IsMultiplayer && Origin != null)
			{
				return Origin.Troop == Game.Current.PlayerTroop;
			}
			return false;
		}
	}

	public bool IsUsingGameObject => CurrentlyUsedGameObject != null;

	public bool CanLeadFormationsRemotely => _canLeadFormationsRemotely;

	public bool IsDetachableFromFormation => _isDetachableFromFormation;

	public float AgentScale => MBAPI.IMBAgent.GetAgentScale(GetPtr());

	public bool CrouchMode => MBAPI.IMBAgent.GetCrouchMode(GetPtr());

	public bool WalkMode => MBAPI.IMBAgent.GetWalkMode(GetPtr());

	public Vec3 Position => AgentHelper.GetAgentPosition(PositionPointer);

	public Vec3 VisualPosition => MBAPI.IMBAgent.GetVisualPosition(GetPtr());

	public Vec2 MovementVelocity => MBAPI.IMBAgent.GetMovementVelocity(GetPtr());

	public Vec3 AverageVelocity => MBAPI.IMBAgent.GetAverageVelocity(GetPtr());

	public float MaximumForwardUnlimitedSpeed => MBAPI.IMBAgent.GetMaximumForwardUnlimitedSpeed(GetPtr());

	public float MovementDirectionAsAngle => MBAPI.IMBAgent.GetMovementDirectionAsAngle(GetPtr());

	public bool IsLookRotationInSlowMotion => MBAPI.IMBAgent.IsLookRotationInSlowMotion(GetPtr());

	public AgentPropertiesModifiers PropertyModifiers => _propertyModifiers;

	public MBActionSet ActionSet => new MBActionSet(MBAPI.IMBAgent.GetActionSetNo(GetPtr()));

	public MBReadOnlyList<AgentComponent> Components => _components;

	public MBReadOnlyList<Hitter> HitterList => _hitterList;

	public GuardMode CurrentGuardMode => MBAPI.IMBAgent.GetCurrentGuardMode(GetPtr());

	public Agent ImmediateEnemy => MBAPI.IMBAgent.GetImmediateEnemy(GetPtr());

	public bool IsDoingPassiveAttack => MBAPI.IMBAgent.GetIsDoingPassiveAttack(GetPtr());

	public bool IsPassiveUsageConditionsAreMet => MBAPI.IMBAgent.GetIsPassiveUsageConditionsAreMet(GetPtr());

	public float CurrentAimingError => MBAPI.IMBAgent.GetCurrentAimingError(GetPtr());

	public float CurrentAimingTurbulance => MBAPI.IMBAgent.GetCurrentAimingTurbulance(GetPtr());

	public UsageDirection AttackDirection => MBAPI.IMBAgent.GetAttackDirectionUsage(GetPtr());

	public float WalkingSpeedLimitOfMountable => MBAPI.IMBAgent.GetWalkSpeedLimitOfMountable(GetPtr());

	public Agent RiderAgent => GetRiderAgentAux();

	public bool HasMount => MountAgent != null;

	public bool CanLogCombatFor
	{
		get
		{
			if (RiderAgent == null || RiderAgent.IsAIControlled)
			{
				if (!IsMount)
				{
					return !IsAIControlled;
				}
				return false;
			}
			return true;
		}
	}

	public float MissileRangeAdjusted => GetMissileRangeWithHeightDifference();

	public float MaximumMissileRange => GetMissileRange();

	FocusableObjectType IFocusable.FocusableObjectType
	{
		get
		{
			if (!IsMount)
			{
				return FocusableObjectType.Agent;
			}
			return FocusableObjectType.Mount;
		}
	}

	public string Name
	{
		get
		{
			if (MissionPeer == null)
			{
				return _name.ToString();
			}
			return MissionPeer.Name;
		}
	}

	public AgentMovementLockedState MovementLockedState => GetMovementLockedState();

	public Monster Monster { get; }

	public bool IsRunningAway { get; private set; }

	public BodyProperties BodyPropertiesValue { get; private set; }

	public CommonAIComponent CommonAIComponent { get; private set; }

	public HumanAIComponent HumanAIComponent { get; private set; }

	public int BodyPropertiesSeed { get; internal set; }

	public float LastRangedHitTime { get; private set; } = float.MinValue;


	public float LastMeleeHitTime { get; private set; } = float.MinValue;


	public float LastRangedAttackTime { get; private set; } = float.MinValue;


	public float LastMeleeAttackTime { get; private set; } = float.MinValue;


	public bool IsFemale { get; set; }

	public ItemObject Banner => Equipment?.GetBanner();

	public ItemObject FormationBanner => _formationBanner;

	public MissionWeapon WieldedWeapon
	{
		get
		{
			EquipmentIndex wieldedItemIndex = GetWieldedItemIndex(HandIndex.MainHand);
			if (wieldedItemIndex < EquipmentIndex.WeaponItemBeginSlot)
			{
				return MissionWeapon.Invalid;
			}
			return Equipment[wieldedItemIndex];
		}
	}

	public bool IsItemUseDisabled { get; set; }

	public bool SyncHealthToAllClients { get; private set; }

	public UsableMissionObject CurrentlyUsedGameObject { get; private set; }

	public bool CombatActionsEnabled
	{
		get
		{
			if (CurrentlyUsedGameObject != null)
			{
				return !CurrentlyUsedGameObject.DisableCombatActionsOnUse;
			}
			return true;
		}
	}

	public Mission Mission { get; private set; }

	public bool IsHero
	{
		get
		{
			if (Character != null)
			{
				return Character.IsHero;
			}
			return false;
		}
	}

	public int Index { get; }

	public MissionEquipment Equipment { get; private set; }

	public TextObject AgentRole { get; set; }

	public bool HasBeenBuilt { get; private set; }

	public MortalityState CurrentMortalityState { get; private set; }

	public Equipment SpawnEquipment { get; private set; }

	public FormationPositionPreference FormationPositionPreference { get; set; }

	public bool RandomizeColors { get; private set; }

	public float CharacterPowerCached { get; private set; }

	public float WalkSpeedCached { get; private set; }

	public float RunSpeedCached { get; private set; }

	public IAgentOriginBase Origin { get; set; }

	public Team Team { get; private set; }

	public int KillCount { get; set; }

	public AgentDrivenProperties AgentDrivenProperties { get; private set; }

	public float BaseHealthLimit { get; set; }

	public string HorseCreationKey { get; private set; }

	public float HealthLimit { get; set; }

	public bool IsRangedCached => Equipment.ContainsNonConsumableRangedWeaponWithAmmo();

	public bool HasMeleeWeaponCached => Equipment.ContainsMeleeWeapon();

	public bool HasShieldCached => Equipment.ContainsShield();

	public bool HasSpearCached => Equipment.ContainsSpear();

	public bool HasThrownCached => Equipment.ContainsThrownWeapon();

	public AIStateFlag AIStateFlags
	{
		get
		{
			return MBAPI.IMBAgent.GetAIStateFlags(GetPtr());
		}
		set
		{
			MBAPI.IMBAgent.SetAIStateFlags(GetPtr(), value);
		}
	}

	public MatrixFrame Frame
	{
		get
		{
			MatrixFrame outFrame = default(MatrixFrame);
			MBAPI.IMBAgent.GetRotationFrame(GetPtr(), ref outFrame);
			return outFrame;
		}
	}

	public MovementControlFlag MovementFlags
	{
		get
		{
			return (MovementControlFlag)MBAPI.IMBAgent.GetMovementFlags(GetPtr());
		}
		set
		{
			MBAPI.IMBAgent.SetMovementFlags(GetPtr(), value);
		}
	}

	public Vec2 MovementInputVector
	{
		get
		{
			return MBAPI.IMBAgent.GetMovementInputVector(GetPtr());
		}
		set
		{
			MBAPI.IMBAgent.SetMovementInputVector(GetPtr(), value);
		}
	}

	public CapsuleData CollisionCapsule
	{
		get
		{
			CapsuleData value = default(CapsuleData);
			MBAPI.IMBAgent.GetCollisionCapsule(GetPtr(), ref value);
			return value;
		}
	}

	public Vec3 CollisionCapsuleCenter
	{
		get
		{
			CapsuleData collisionCapsule = CollisionCapsule;
			return (collisionCapsule.GetBoxMax() + collisionCapsule.GetBoxMin()) * 0.5f;
		}
	}

	public MBAgentVisuals AgentVisuals
	{
		get
		{
			if (!_visualsWeakRef.TryGetTarget(out var target))
			{
				target = MBAPI.IMBAgent.GetAgentVisuals(GetPtr());
				_visualsWeakRef.SetTarget(target);
			}
			return target;
		}
	}

	public bool HeadCameraMode
	{
		get
		{
			return MBAPI.IMBAgent.GetHeadCameraMode(GetPtr());
		}
		set
		{
			MBAPI.IMBAgent.SetHeadCameraMode(GetPtr(), value);
		}
	}

	public Agent MountAgent
	{
		get
		{
			return GetMountAgentAux();
		}
		private set
		{
			SetMountAgent(value);
			UpdateAgentStats();
		}
	}

	public IDetachment Detachment
	{
		get
		{
			return _detachment;
		}
		set
		{
			_detachment = value;
			if (_detachment != null)
			{
				Formation?.Team.DetachmentManager.RemoveScoresOfAgentFromDetachments(this);
			}
		}
	}

	public bool IsPaused
	{
		get
		{
			return AIStateFlags.HasAnyFlag(AIStateFlag.Paused);
		}
		set
		{
			if (value)
			{
				AIStateFlags |= AIStateFlag.Paused;
			}
			else
			{
				AIStateFlags &= ~AIStateFlag.Paused;
			}
		}
	}

	public bool IsDetachedFromFormation => _detachment != null;

	public WatchState CurrentWatchState
	{
		get
		{
			AIStateFlag aIStateFlags = AIStateFlags;
			if ((aIStateFlags & AIStateFlag.Alarmed) == AIStateFlag.Alarmed)
			{
				return WatchState.Alarmed;
			}
			if ((aIStateFlags & AIStateFlag.Cautious) == AIStateFlag.Cautious)
			{
				return WatchState.Cautious;
			}
			return WatchState.Patrolling;
		}
		private set
		{
			AIStateFlag aIStateFlag = AIStateFlags;
			switch (value)
			{
			case WatchState.Patrolling:
				aIStateFlag &= ~(AIStateFlag.Cautious | AIStateFlag.Alarmed);
				break;
			case WatchState.Cautious:
				aIStateFlag |= AIStateFlag.Cautious;
				aIStateFlag &= ~AIStateFlag.Alarmed;
				break;
			case WatchState.Alarmed:
				aIStateFlag |= AIStateFlag.Alarmed;
				aIStateFlag &= ~AIStateFlag.Cautious;
				break;
			default:
				TaleWorlds.Library.Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Agent.cs", "CurrentWatchState", 900);
				break;
			}
			AIStateFlags = aIStateFlag;
		}
	}

	public float Defensiveness
	{
		get
		{
			return _defensiveness;
		}
		set
		{
			if (TaleWorlds.Library.MathF.Abs(value - _defensiveness) > 0.0001f)
			{
				_defensiveness = value;
				UpdateAgentProperties();
			}
		}
	}

	public Formation Formation
	{
		get
		{
			return _formation;
		}
		set
		{
			if (_formation == value)
			{
				return;
			}
			if (GameNetwork.IsServer && HasBeenBuilt && Mission.GetMissionBehavior<MissionNetworkComponent>() != null)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new AgentSetFormation(Index, value?.Index ?? (-1)));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
			}
			SetNativeFormationNo(value?.Index ?? (-1));
			IDetachment detachment = null;
			float detachmentWeight = 0f;
			if (_formation != null)
			{
				if (IsDetachedFromFormation)
				{
					detachment = Detachment;
					detachmentWeight = DetachmentWeight;
				}
				_formation.RemoveUnit(this);
				foreach (IDetachment detachment2 in _formation.Detachments)
				{
					if (!detachment2.IsUsedByFormation(value))
					{
						Team.DetachmentManager.RemoveScoresOfAgentFromDetachment(this, detachment2);
					}
				}
			}
			_formation = value;
			if (_formation != null)
			{
				if (!_formation.HasBeenPositioned)
				{
					_formation.SetPositioning(GetWorldPosition(), LookDirection.AsVec2);
				}
				_formation.AddUnit(this);
				if (detachment != null && _formation.Detachments.IndexOf(detachment) >= 0 && detachment.IsStandingPointAvailableForAgent(this))
				{
					detachment.AddAgent(this);
					_formation.DetachUnit(this, detachment.IsLoose);
					Detachment = detachment;
					DetachmentWeight = detachmentWeight;
				}
			}
			UpdateCachedAndFormationValues(_formation != null && _formation.PostponeCostlyOperations, arrangementChangeAllowed: false);
		}
	}

	IFormationUnit IFormationUnit.FollowedUnit
	{
		get
		{
			if (!IsActive())
			{
				return null;
			}
			if (IsAIControlled)
			{
				return this.GetFollowedUnit();
			}
			return null;
		}
	}

	public bool IsShieldUsageEncouraged
	{
		get
		{
			if (Formation.FiringOrder.OrderEnum != FiringOrder.RangedWeaponUsageOrderEnum.HoldYourFire)
			{
				return !Equipment.HasAnyWeaponWithFlags(WeaponFlags.RangedWeapon | WeaponFlags.NotUsableWithOneHand);
			}
			return true;
		}
	}

	public bool IsPlayerUnit
	{
		get
		{
			if (!IsPlayerControlled)
			{
				return IsPlayerTroop;
			}
			return true;
		}
	}

	public ControllerType Controller
	{
		get
		{
			return GetController();
		}
		set
		{
			ControllerType controller = Controller;
			if (value == controller)
			{
				return;
			}
			SetController(value);
			bool flag = value == ControllerType.Player;
			if (flag)
			{
				Mission.MainAgent = this;
				SetAgentFlags(GetAgentFlags() | AgentFlag.CanRide);
			}
			if (Formation != null)
			{
				Formation.OnAgentControllerChanged(this, controller);
			}
			if (value != ControllerType.AI && GetAgentFlags().HasAnyFlag(AgentFlag.IsHumanoid))
			{
				MountAgent?.SetMaximumSpeedLimit(-1f, isMultiplier: false);
				SetMaximumSpeedLimit(-1f, isMultiplier: false);
				if (WalkMode)
				{
					EventControlFlags |= EventControlFlag.Run;
				}
			}
			foreach (MissionBehavior missionBehavior in Mission.MissionBehaviors)
			{
				missionBehavior.OnAgentControllerChanged(this, controller);
			}
			if (flag)
			{
				foreach (MissionBehavior missionBehavior2 in Mission.MissionBehaviors)
				{
					missionBehavior2.OnAgentControllerSetToPlayer(Mission.MainAgent);
				}
			}
			if (GameNetwork.IsServer)
			{
				NetworkCommunicator networkCommunicator = MissionPeer?.GetNetworkPeer();
				if (networkCommunicator != null && !networkCommunicator.IsServerPeer)
				{
					GameNetwork.BeginModuleEventAsServer(networkCommunicator);
					GameNetwork.WriteMessage(new SetAgentIsPlayer(Index, Controller != ControllerType.AI));
					GameNetwork.EndModuleEventAsServer();
				}
			}
		}
	}

	public uint ClothingColor1
	{
		get
		{
			if (_clothingColor1.HasValue)
			{
				return _clothingColor1.Value;
			}
			if (Team != null)
			{
				return Team.Color;
			}
			TaleWorlds.Library.Debug.FailedAssert("Clothing color is not set.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Agent.cs", "ClothingColor1", 1116);
			return uint.MaxValue;
		}
	}

	public uint ClothingColor2
	{
		get
		{
			if (_clothingColor2.HasValue)
			{
				return _clothingColor2.Value;
			}
			return ClothingColor1;
		}
	}

	public MatrixFrame LookFrame
	{
		get
		{
			MatrixFrame result = default(MatrixFrame);
			result.origin = Position;
			result.rotation = LookRotation;
			return result;
		}
	}

	public float LookDirectionAsAngle
	{
		get
		{
			return MBAPI.IMBAgent.GetLookDirectionAsAngle(GetPtr());
		}
		set
		{
			MBAPI.IMBAgent.SetLookDirectionAsAngle(GetPtr(), value);
		}
	}

	public Mat3 LookRotation
	{
		get
		{
			Mat3 result = default(Mat3);
			result.f = LookDirection;
			result.u = Vec3.Up;
			result.s = Vec3.CrossProduct(result.f, result.u);
			result.s.Normalize();
			result.u = Vec3.CrossProduct(result.s, result.f);
			return result;
		}
	}

	public bool IsLookDirectionLocked
	{
		get
		{
			return MBAPI.IMBAgent.GetIsLookDirectionLocked(GetPtr());
		}
		set
		{
			MBAPI.IMBAgent.SetIsLookDirectionLocked(GetPtr(), value);
		}
	}

	public bool IsCheering
	{
		get
		{
			ActionIndexValueCache currentActionValue = GetCurrentActionValue(1);
			for (int i = 0; i < DefaultTauntActions.Length; i++)
			{
				if (DefaultTauntActions[i] != null && DefaultTauntActions[i] == currentActionValue)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsInBeingStruckAction
	{
		get
		{
			if (!MBMath.IsBetween((int)GetCurrentActionType(1), 47, 51))
			{
				return MBMath.IsBetween((int)GetCurrentActionType(0), 47, 51);
			}
			return true;
		}
	}

	public MissionPeer MissionPeer
	{
		get
		{
			return _missionPeer;
		}
		set
		{
			if (_missionPeer == value)
			{
				return;
			}
			MissionPeer missionPeer = _missionPeer;
			_missionPeer = value;
			if (missionPeer != null && missionPeer.ControlledAgent == this)
			{
				missionPeer.ControlledAgent = null;
			}
			if (_missionPeer != null && _missionPeer.ControlledAgent != this)
			{
				_missionPeer.ControlledAgent = this;
				if (GameNetwork.IsServerOrRecorder)
				{
					SyncHealthToClients();
					this.OnAgentHealthChanged?.Invoke(this, Health, Health);
				}
			}
			if (value != null)
			{
				Controller = (value.IsMine ? ControllerType.Player : ControllerType.None);
			}
			if (GameNetwork.IsServer && IsHuman && !_isDeleted)
			{
				NetworkCommunicator networkCommunicator = value?.GetNetworkPeer();
				SetNetworkPeer(networkCommunicator);
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new SetAgentPeer(Index, networkCommunicator));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
			}
		}
	}

	public BasicCharacterObject Character
	{
		get
		{
			return _character;
		}
		set
		{
			_character = value;
			if (value != null)
			{
				Health = _character.HitPoints;
				BaseHealthLimit = _character.MaxHitPoints();
				HealthLimit = BaseHealthLimit;
				CharacterPowerCached = value.GetPower();
				_name = value.Name;
				IsFemale = value.IsFemale;
			}
		}
	}

	IMissionTeam IAgent.Team => Team;

	IFormationArrangement IFormationUnit.Formation => _formation.Arrangement;

	int IFormationUnit.FormationFileIndex { get; set; }

	int IFormationUnit.FormationRankIndex { get; set; }

	private UIntPtr Pointer => _pointer;

	private UIntPtr FlagsPointer => _flagsPointer;

	private UIntPtr PositionPointer => _positionPointer;

	public Vec3 LookDirection
	{
		get
		{
			return MBAPI.IMBAgent.GetLookDirection(GetPtr());
		}
		set
		{
			MBAPI.IMBAgent.SetLookDirection(GetPtr(), value);
		}
	}

	public float Health
	{
		get
		{
			return _health;
		}
		set
		{
			float num = ((!value.ApproximatelyEqualsTo(0f)) ? TaleWorlds.Library.MathF.Ceiling(value) : 0);
			if (!_health.ApproximatelyEqualsTo(num))
			{
				float health = _health;
				_health = num;
				if (GameNetwork.IsServerOrRecorder)
				{
					SyncHealthToClients();
				}
				this.OnAgentHealthChanged?.Invoke(this, health, _health);
				if (RiderAgent != null)
				{
					RiderAgent.OnMountHealthChanged?.Invoke(RiderAgent, this, health, _health);
				}
			}
		}
	}

	public float Age
	{
		get
		{
			return BodyPropertiesValue.Age;
		}
		set
		{
			BodyPropertiesValue = new BodyProperties(new DynamicBodyProperties(value, BodyPropertiesValue.Weight, BodyPropertiesValue.Build), BodyPropertiesValue.StaticProperties);
			BodyProperties bodyPropertiesValue = BodyPropertiesValue;
			BodyPropertiesValue = bodyPropertiesValue;
		}
	}

	public Vec3 Velocity
	{
		get
		{
			Vec2 movementVelocity = MBAPI.IMBAgent.GetMovementVelocity(GetPtr());
			Vec3 v = new Vec3(movementVelocity);
			return Frame.rotation.TransformToParent(v);
		}
	}

	public EventControlFlag EventControlFlags
	{
		get
		{
			return (EventControlFlag)MBAPI.IMBAgent.GetEventControlFlags(GetPtr());
		}
		set
		{
			MBAPI.IMBAgent.SetEventControlFlags(GetPtr(), value);
		}
	}

	public AgentState State
	{
		get
		{
			return AgentHelper.GetAgentState(_statePointer);
		}
		set
		{
			if (State != value)
			{
				MBAPI.IMBAgent.SetStateFlags(GetPtr(), value);
			}
		}
	}

	public MissionWeapon WieldedOffhandWeapon
	{
		get
		{
			EquipmentIndex wieldedItemIndex = GetWieldedItemIndex(HandIndex.OffHand);
			if (wieldedItemIndex < EquipmentIndex.WeaponItemBeginSlot)
			{
				return MissionWeapon.Invalid;
			}
			return Equipment[wieldedItemIndex];
		}
	}

	public event OnAgentHealthChangedDelegate OnAgentHealthChanged;

	public event OnMountHealthChangedDelegate OnMountHealthChanged;

	internal Agent(Mission mission, Mission.AgentCreationResult creationResult, CreationType creationType, Monster monster)
	{
		AgentRole = TextObject.Empty;
		Mission = mission;
		Index = creationResult.Index;
		_pointer = creationResult.AgentPtr;
		_positionPointer = creationResult.PositionPtr;
		_flagsPointer = creationResult.FlagsPtr;
		_indexPointer = creationResult.IndexPtr;
		_statePointer = creationResult.StatePtr;
		_lastHitInfo = default(AgentLastHitInfo);
		_lastHitInfo.Initialize();
		MBAPI.IMBAgent.SetMonoObject(GetPtr(), this);
		Monster = monster;
		KillCount = 0;
		HasBeenBuilt = false;
		_creationType = creationType;
		_agentControllers = new List<AgentController>();
		_components = new MBList<AgentComponent>();
		_hitterList = new MBList<Hitter>();
		((IFormationUnit)this).FormationFileIndex = -1;
		((IFormationUnit)this).FormationRankIndex = -1;
		_synchedBodyComponents = null;
		_cachedAndFormationValuesUpdateTimer = new Timer(Mission.CurrentTime, 0.45f + MBRandom.RandomFloat * 0.1f);
	}

	bool IAgent.IsEnemyOf(IAgent agent)
	{
		return IsEnemyOf((Agent)agent);
	}

	bool IAgent.IsFriendOf(IAgent agent)
	{
		return IsFriendOf((Agent)agent);
	}

	Vec3 ITrackableBase.GetPosition()
	{
		return Position;
	}

	TextObject ITrackableBase.GetName()
	{
		if (Character != null)
		{
			return new TextObject(Character.Name.ToString());
		}
		return TextObject.Empty;
	}

	[MBCallback]
	internal void SetAgentAIPerformingRetreatBehavior(bool isAgentAIPerformingRetreatBehavior)
	{
		if (!GameNetwork.IsClientOrReplay && Mission != null)
		{
			IsRunningAway = isAgentAIPerformingRetreatBehavior;
		}
	}

	[MBCallback]
	public float GetMissileRangeWithHeightDifferenceAux(float targetZ)
	{
		return MBAPI.IMBAgent.GetMissileRangeWithHeightDifference(GetPtr(), targetZ);
	}

	[MBCallback]
	internal int GetFormationUnitSpacing()
	{
		return Formation.UnitSpacing;
	}

	[MBCallback]
	public string GetSoundAndCollisionInfoClassName()
	{
		return Monster.SoundAndCollisionInfoClassName;
	}

	[MBCallback]
	internal bool IsInSameFormationWith(Agent otherAgent)
	{
		Formation formation = otherAgent.Formation;
		if (Formation != null && formation != null)
		{
			return Formation == formation;
		}
		return false;
	}

	[MBCallback]
	internal void OnWeaponSwitchingToAlternativeStart(EquipmentIndex slotIndex, int usageIndex)
	{
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new StartSwitchingWeaponUsageIndex(Index, slotIndex, usageIndex, MovementFlagToDirection(MovementFlags)));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
	}

	[MBCallback]
	internal void OnWeaponReloadPhaseChange(EquipmentIndex slotIndex, short reloadPhase)
	{
		Equipment.SetReloadPhaseOfSlot(slotIndex, reloadPhase);
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SetWeaponReloadPhase(Index, slotIndex, reloadPhase));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
	}

	[MBCallback]
	internal void OnWeaponAmmoReload(EquipmentIndex slotIndex, EquipmentIndex ammoSlotIndex, short totalAmmo)
	{
		if (Equipment[slotIndex].CurrentUsageItem.IsRangedWeapon)
		{
			Equipment.SetReloadedAmmoOfSlot(slotIndex, ammoSlotIndex, totalAmmo);
			if (GameNetwork.IsServerOrRecorder)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new SetWeaponAmmoData(Index, slotIndex, ammoSlotIndex, totalAmmo));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
			}
		}
		UpdateAgentProperties();
	}

	[MBCallback]
	internal void OnWeaponAmmoConsume(EquipmentIndex slotIndex, short totalAmmo)
	{
		if (Equipment[slotIndex].CurrentUsageItem.IsRangedWeapon)
		{
			Equipment.SetConsumedAmmoOfSlot(slotIndex, totalAmmo);
			if (GameNetwork.IsServerOrRecorder)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new SetWeaponAmmoData(Index, slotIndex, EquipmentIndex.None, totalAmmo));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
			}
		}
		UpdateAgentProperties();
	}

	[MBCallback]
	internal void OnShieldDamaged(EquipmentIndex slotIndex, int inflictedDamage)
	{
		int num = TaleWorlds.Library.MathF.Max(0, Equipment[slotIndex].HitPoints - inflictedDamage);
		ChangeWeaponHitPoints(slotIndex, (short)num);
		if (num == 0)
		{
			RemoveEquippedWeapon(slotIndex);
		}
	}

	[MBCallback]
	internal void OnWeaponAmmoRemoved(EquipmentIndex slotIndex)
	{
		if (!Equipment[slotIndex].AmmoWeapon.IsEmpty)
		{
			Equipment.SetConsumedAmmoOfSlot(slotIndex, 0);
		}
	}

	[MBCallback]
	internal void OnMount(Agent mount)
	{
		if (!GameNetwork.IsClientOrReplay)
		{
			if (mount.IsAIControlled && mount.IsRetreating(isComponentAssured: false))
			{
				mount.StopRetreatingMoraleComponent();
			}
			CheckToDropFlaggedItem();
		}
		if (HasBeenBuilt)
		{
			foreach (AgentComponent component in _components)
			{
				component.OnMount(mount);
			}
			Mission.OnAgentMount(this);
		}
		UpdateAgentStats();
		OnAgentMountedStateChanged?.Invoke();
		if (GameNetwork.IsServerOrRecorder)
		{
			mount.SyncHealthToClients();
		}
	}

	[MBCallback]
	internal void OnDismount(Agent mount)
	{
		if (!GameNetwork.IsClientOrReplay)
		{
			Formation?.OnAgentLostMount(this);
			CheckToDropFlaggedItem();
		}
		foreach (AgentComponent component in _components)
		{
			component.OnDismount(mount);
		}
		Mission.OnAgentDismount(this);
		if (IsActive())
		{
			UpdateAgentStats();
			OnAgentMountedStateChanged?.Invoke();
		}
	}

	[MBCallback]
	internal void OnAgentAlarmedStateChanged(AIStateFlag flag)
	{
		foreach (MissionBehavior missionBehavior in Mission.Current.MissionBehaviors)
		{
			missionBehavior.OnAgentAlarmedStateChanged(this, flag);
		}
	}

	[MBCallback]
	internal void OnRetreating()
	{
		if (GameNetwork.IsClientOrReplay || Mission == null || Mission.MissionEnded)
		{
			return;
		}
		if (IsUsingGameObject)
		{
			StopUsingGameObjectMT();
		}
		foreach (AgentComponent component in _components)
		{
			component.OnRetreating();
		}
	}

	[MBCallback]
	internal void UpdateMountAgentCache(Agent newMountAgent)
	{
		_cachedMountAgent = newMountAgent;
	}

	[MBCallback]
	internal void UpdateRiderAgentCache(Agent newRiderAgent)
	{
		_cachedRiderAgent = newRiderAgent;
		if (newRiderAgent == null)
		{
			Mission.Current.AddMountWithoutRider(this);
		}
		else
		{
			Mission.Current.RemoveMountWithoutRider(this);
		}
	}

	[MBCallback]
	public void UpdateAgentStats()
	{
		if (IsActive())
		{
			UpdateAgentProperties();
		}
	}

	[MBCallback]
	public float GetWeaponInaccuracy(EquipmentIndex weaponSlotIndex, int weaponUsageIndex)
	{
		WeaponComponentData weaponComponentDataForUsage = Equipment[weaponSlotIndex].GetWeaponComponentDataForUsage(weaponUsageIndex);
		int effectiveSkill = MissionGameModels.Current.AgentStatCalculateModel.GetEffectiveSkill(this, weaponComponentDataForUsage.RelevantSkill);
		return MissionGameModels.Current.AgentStatCalculateModel.GetWeaponInaccuracy(this, weaponComponentDataForUsage, effectiveSkill);
	}

	[MBCallback]
	public float DebugGetHealth()
	{
		return Health;
	}

	public void SetTargetPosition(Vec2 value)
	{
		MBAPI.IMBAgent.SetTargetPosition(GetPtr(), ref value);
	}

	public void SetGuardState(Agent guardedAgent, bool isGuarding)
	{
		if (isGuarding)
		{
			AIStateFlags |= AIStateFlag.Guard;
		}
		else
		{
			AIStateFlags &= ~AIStateFlag.Guard;
		}
		SetGuardedAgent(guardedAgent);
	}

	public void SetCanLeadFormationsRemotely(bool value)
	{
		_canLeadFormationsRemotely = value;
	}

	public void SetAveragePingInMilliseconds(double averagePingInMilliseconds)
	{
		MBAPI.IMBAgent.SetAveragePingInMilliseconds(GetPtr(), averagePingInMilliseconds);
	}

	public void SetTargetPositionAndDirection(Vec2 targetPosition, Vec3 targetDirection)
	{
		MBAPI.IMBAgent.SetTargetPositionAndDirection(GetPtr(), ref targetPosition, ref targetDirection);
	}

	public void SetWatchState(WatchState watchState)
	{
		CurrentWatchState = watchState;
	}

	[MBCallback]
	internal void OnWieldedItemIndexChange(bool isOffHand, bool isWieldedInstantly, bool isWieldedOnSpawn)
	{
		if (IsMainAgent)
		{
			OnMainAgentWieldedItemChange?.Invoke();
		}
		OnAgentWieldedItemChange?.Invoke();
		if (GameNetwork.IsServerOrRecorder)
		{
			int mainHandCurUsageIndex = 0;
			EquipmentIndex wieldedItemIndex = GetWieldedItemIndex(HandIndex.MainHand);
			if (wieldedItemIndex != EquipmentIndex.None)
			{
				mainHandCurUsageIndex = Equipment[wieldedItemIndex].CurrentUsageIndex;
			}
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SetWieldedItemIndex(Index, isOffHand, isWieldedInstantly, isWieldedOnSpawn, GetWieldedItemIndex(isOffHand ? HandIndex.OffHand : HandIndex.MainHand), mainHandCurUsageIndex));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		CheckEquipmentForCapeClothSimulationStateChange();
	}

	public void SetFormationBanner(ItemObject banner)
	{
		_formationBanner = banner;
	}

	public void SetIsAIPaused(bool isPaused)
	{
		IsPaused = isPaused;
	}

	public void ResetEnemyCaches()
	{
		MBAPI.IMBAgent.ResetEnemyCaches(GetPtr());
	}

	public void SetTargetPositionSynched(ref Vec2 targetPosition)
	{
		if (MovementLockedState != 0 && !(GetTargetPosition() != targetPosition))
		{
			return;
		}
		if (GameNetwork.IsClientOrReplay)
		{
			_lastSynchedTargetPosition = targetPosition;
			_checkIfTargetFrameIsChanged = true;
			return;
		}
		SetTargetPosition(targetPosition);
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SetAgentTargetPosition(Index, ref targetPosition));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
	}

	public void SetTargetPositionAndDirectionSynched(ref Vec2 targetPosition, ref Vec3 targetDirection)
	{
		if (MovementLockedState != 0 && !(GetTargetDirection() != targetDirection))
		{
			return;
		}
		if (GameNetwork.IsClientOrReplay)
		{
			_lastSynchedTargetDirection = targetDirection;
			_checkIfTargetFrameIsChanged = true;
			return;
		}
		SetTargetPositionAndDirection(targetPosition, targetDirection);
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SetAgentTargetPositionAndDirection(Index, ref targetPosition, ref targetDirection));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
	}

	public void SetBodyArmorMaterialType(ArmorComponent.ArmorMaterialTypes bodyArmorMaterialType)
	{
		MBAPI.IMBAgent.SetBodyArmorMaterialType(GetPtr(), bodyArmorMaterialType);
	}

	public void SetUsedGameObjectForClient(UsableMissionObject usedObject)
	{
		CurrentlyUsedGameObject = usedObject;
		usedObject.OnUse(this);
		Mission.OnObjectUsed(this, usedObject);
	}

	public void SetTeam(Team team, bool sync)
	{
		if (Team == team)
		{
			return;
		}
		Team team2 = Team;
		Team?.RemoveAgentFromTeam(this);
		Team = team;
		Team?.AddAgentToTeam(this);
		SetTeamInternal(team?.MBTeam ?? MBTeam.InvalidTeam);
		if (sync && GameNetwork.IsServer && Mission.HasMissionBehavior<MissionNetworkComponent>())
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new AgentSetTeam(Index, team?.TeamIndex ?? (-1)));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		foreach (MissionBehavior missionBehavior in Mission.Current.MissionBehaviors)
		{
			missionBehavior.OnAgentTeamChanged(team2, team, this);
		}
	}

	public void SetClothingColor1(uint color)
	{
		_clothingColor1 = color;
	}

	public void SetClothingColor2(uint color)
	{
		_clothingColor2 = color;
	}

	public void SetWieldedItemIndexAsClient(HandIndex handIndex, EquipmentIndex equipmentIndex, bool isWieldedInstantly, bool isWieldedOnSpawn, int mainHandCurrentUsageIndex)
	{
		MBAPI.IMBAgent.SetWieldedItemIndexAsClient(GetPtr(), (int)handIndex, (int)equipmentIndex, isWieldedInstantly, isWieldedOnSpawn, mainHandCurrentUsageIndex);
	}

	public void SetPreciseRangedAimingEnabled(bool set)
	{
		if (set)
		{
			SetScriptedFlags(GetScriptedFlags() | AIScriptedFrameFlags.RangerCanMoveForClearTarget);
		}
		else
		{
			SetScriptedFlags(GetScriptedFlags() & ~AIScriptedFrameFlags.RangerCanMoveForClearTarget);
		}
	}

	public void SetAsConversationAgent(bool set)
	{
		if (set)
		{
			SetScriptedFlags(GetScriptedFlags() | AIScriptedFrameFlags.InConversation);
			DisableLookToPointOfInterest();
		}
		else
		{
			SetScriptedFlags(GetScriptedFlags() & ~AIScriptedFrameFlags.InConversation);
		}
	}

	public void SetCrouchMode(bool set)
	{
		if (set)
		{
			SetScriptedFlags(GetScriptedFlags() | AIScriptedFrameFlags.Crouch);
		}
		else
		{
			SetScriptedFlags(GetScriptedFlags() & ~AIScriptedFrameFlags.Crouch);
		}
	}

	public void SetWeaponAmountInSlot(EquipmentIndex equipmentSlot, short amount, bool enforcePrimaryItem)
	{
		MBAPI.IMBAgent.SetWeaponAmountInSlot(GetPtr(), (int)equipmentSlot, amount, enforcePrimaryItem);
	}

	public void SetWeaponAmmoAsClient(EquipmentIndex equipmentIndex, EquipmentIndex ammoEquipmentIndex, short ammo)
	{
		MBAPI.IMBAgent.SetWeaponAmmoAsClient(GetPtr(), (int)equipmentIndex, (int)ammoEquipmentIndex, ammo);
	}

	public void SetWeaponReloadPhaseAsClient(EquipmentIndex equipmentIndex, short reloadState)
	{
		MBAPI.IMBAgent.SetWeaponReloadPhaseAsClient(GetPtr(), (int)equipmentIndex, reloadState);
	}

	public void SetReloadAmmoInSlot(EquipmentIndex equipmentIndex, EquipmentIndex ammoSlotIndex, short reloadedAmmo)
	{
		MBAPI.IMBAgent.SetReloadAmmoInSlot(GetPtr(), (int)equipmentIndex, (int)ammoSlotIndex, reloadedAmmo);
	}

	public void SetUsageIndexOfWeaponInSlotAsClient(EquipmentIndex slotIndex, int usageIndex)
	{
		MBAPI.IMBAgent.SetUsageIndexOfWeaponInSlotAsClient(GetPtr(), (int)slotIndex, usageIndex);
	}

	public void SetRandomizeColors(bool shouldRandomize)
	{
		RandomizeColors = shouldRandomize;
	}

	[MBCallback]
	internal void OnRemoveWeapon(EquipmentIndex slotIndex)
	{
		RemoveEquippedWeapon(slotIndex);
	}

	public void SetFormationFrameDisabled()
	{
		MBAPI.IMBAgent.SetFormationFrameDisabled(GetPtr());
	}

	public void SetFormationFrameEnabled(WorldPosition position, Vec2 direction, Vec2 positionVelocity, float formationDirectionEnforcingFactor)
	{
		MBAPI.IMBAgent.SetFormationFrameEnabled(GetPtr(), position, direction, positionVelocity, formationDirectionEnforcingFactor);
		if (Mission.IsTeleportingAgents)
		{
			TeleportToPosition(position.GetGroundVec3());
		}
	}

	public void SetShouldCatchUpWithFormation(bool value)
	{
		MBAPI.IMBAgent.SetShouldCatchUpWithFormation(GetPtr(), value);
	}

	public void SetFormationIntegrityData(Vec2 position, Vec2 currentFormationDirection, Vec2 averageVelocityOfCloseAgents, float averageMaxUnlimitedSpeedOfCloseAgents, float deviationOfPositions)
	{
		MBAPI.IMBAgent.SetFormationIntegrityData(GetPtr(), position, currentFormationDirection, averageVelocityOfCloseAgents, averageMaxUnlimitedSpeedOfCloseAgents, deviationOfPositions);
	}

	public void SetGuardedAgent(Agent guardedAgent)
	{
		int guardedAgentIndex = guardedAgent?.Index ?? (-1);
		MBAPI.IMBAgent.SetGuardedAgentIndex(GetPtr(), guardedAgentIndex);
	}

	[MBCallback]
	internal void OnWeaponUsageIndexChange(EquipmentIndex slotIndex, int usageIndex)
	{
		Equipment.SetUsageIndexOfSlot(slotIndex, usageIndex);
		UpdateAgentProperties();
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new WeaponUsageIndexChangeMessage(Index, slotIndex, usageIndex));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
	}

	public void SetCurrentActionProgress(int channelNo, float progress)
	{
		MBAPI.IMBAgent.SetCurrentActionProgress(GetPtr(), channelNo, progress);
	}

	public void SetCurrentActionSpeed(int channelNo, float speed)
	{
		MBAPI.IMBAgent.SetCurrentActionSpeed(GetPtr(), channelNo, speed);
	}

	public bool SetActionChannel(int channelNo, ActionIndexCache actionIndexCache, bool ignorePriority = false, ulong additionalFlags = 0uL, float blendWithNextActionFactor = 0f, float actionSpeed = 1f, float blendInPeriod = -0.2f, float blendOutPeriodToNoAnim = 0.4f, float startProgress = 0f, bool useLinearSmoothing = false, float blendOutPeriod = -0.2f, int actionShift = 0, bool forceFaceMorphRestart = true)
	{
		int index = actionIndexCache.Index;
		return MBAPI.IMBAgent.SetActionChannel(GetPtr(), channelNo, index + actionShift, additionalFlags, ignorePriority, blendWithNextActionFactor, actionSpeed, blendInPeriod, blendOutPeriodToNoAnim, startProgress, useLinearSmoothing, blendOutPeriod, forceFaceMorphRestart);
	}

	public bool SetActionChannel(int channelNo, ActionIndexValueCache actionIndexCache, bool ignorePriority = false, ulong additionalFlags = 0uL, float blendWithNextActionFactor = 0f, float actionSpeed = 1f, float blendInPeriod = -0.2f, float blendOutPeriodToNoAnim = 0.4f, float startProgress = 0f, bool useLinearSmoothing = false, float blendOutPeriod = -0.2f, int actionShift = 0, bool forceFaceMorphRestart = true)
	{
		int index = actionIndexCache.Index;
		return MBAPI.IMBAgent.SetActionChannel(GetPtr(), channelNo, index + actionShift, additionalFlags, ignorePriority, blendWithNextActionFactor, actionSpeed, blendInPeriod, blendOutPeriodToNoAnim, startProgress, useLinearSmoothing, blendOutPeriod, forceFaceMorphRestart);
	}

	[MBCallback]
	internal void OnWeaponAmountChange(EquipmentIndex slotIndex, short amount)
	{
		Equipment.SetAmountOfSlot(slotIndex, amount);
		UpdateAgentProperties();
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SetWeaponNetworkData(Index, slotIndex, amount));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
	}

	public void SetAttackState(int attackState)
	{
		MBAPI.IMBAgent.SetAttackState(GetPtr(), attackState);
	}

	public void SetAIBehaviorParams(HumanAIComponent.AISimpleBehaviorKind behavior, float y1, float x2, float y2, float x3, float y3)
	{
		MBAPI.IMBAgent.SetAIBehaviorParams(GetPtr(), (int)behavior, y1, x2, y2, x3, y3);
	}

	public void SetAllBehaviorParams(HumanAIComponent.BehaviorValues[] behaviorParams)
	{
		MBAPI.IMBAgent.SetAllAIBehaviorParams(GetPtr(), behaviorParams);
	}

	public void SetMovementDirection(in Vec2 direction)
	{
		MBAPI.IMBAgent.SetMovementDirection(GetPtr(), in direction);
	}

	public void SetScriptedFlags(AIScriptedFrameFlags flags)
	{
		MBAPI.IMBAgent.SetScriptedFlags(GetPtr(), (int)flags);
	}

	public void SetScriptedCombatFlags(AISpecialCombatModeFlags flags)
	{
		MBAPI.IMBAgent.SetScriptedCombatFlags(GetPtr(), (int)flags);
	}

	public void SetScriptedPositionAndDirection(ref WorldPosition scriptedPosition, float scriptedDirection, bool addHumanLikeDelay, AIScriptedFrameFlags additionalFlags = AIScriptedFrameFlags.None)
	{
		MBAPI.IMBAgent.SetScriptedPositionAndDirection(GetPtr(), ref scriptedPosition, scriptedDirection, addHumanLikeDelay, (int)additionalFlags);
		if (Mission.IsTeleportingAgents && scriptedPosition.AsVec2 != Position.AsVec2)
		{
			TeleportToPosition(scriptedPosition.GetGroundVec3());
		}
	}

	public void SetScriptedPosition(ref WorldPosition position, bool addHumanLikeDelay, AIScriptedFrameFlags additionalFlags = AIScriptedFrameFlags.None)
	{
		MBAPI.IMBAgent.SetScriptedPosition(GetPtr(), ref position, addHumanLikeDelay, (int)additionalFlags);
		if (Mission.IsTeleportingAgents && position.AsVec2 != Position.AsVec2)
		{
			TeleportToPosition(position.GetGroundVec3());
		}
	}

	public void SetScriptedTargetEntityAndPosition(GameEntity target, WorldPosition position, AISpecialCombatModeFlags additionalFlags = AISpecialCombatModeFlags.None, bool ignoreIfAlreadyAttacking = false)
	{
		MBAPI.IMBAgent.SetScriptedTargetEntity(GetPtr(), target.Pointer, ref position, (int)additionalFlags, ignoreIfAlreadyAttacking);
	}

	public void SetAgentExcludeStateForFaceGroupId(int faceGroupId, bool isExcluded)
	{
		MBAPI.IMBAgent.SetAgentExcludeStateForFaceGroupId(GetPtr(), faceGroupId, isExcluded);
	}

	public void SetLookAgent(Agent agent)
	{
		_lookAgentCache = agent;
		MBAPI.IMBAgent.SetLookAgent(GetPtr(), agent?.GetPtr() ?? UIntPtr.Zero);
	}

	public void SetInteractionAgent(Agent agent)
	{
		MBAPI.IMBAgent.SetInteractionAgent(GetPtr(), agent?.GetPtr() ?? UIntPtr.Zero);
	}

	public void SetLookToPointOfInterest(Vec3 point)
	{
		MBAPI.IMBAgent.SetLookToPointOfInterest(GetPtr(), point);
	}

	public void SetAgentFlags(AgentFlag agentFlags)
	{
		MBAPI.IMBAgent.SetAgentFlags(GetPtr(), (uint)agentFlags);
	}

	public void SetSelectedMountIndex(int mountIndex)
	{
		MBAPI.IMBAgent.SetSelectedMountIndex(GetPtr(), mountIndex);
	}

	public int GetSelectedMountIndex()
	{
		return MBAPI.IMBAgent.GetSelectedMountIndex(GetPtr());
	}

	public int GetFiringOrder()
	{
		return MBAPI.IMBAgent.GetFiringOrder(GetPtr());
	}

	public void SetFiringOrder(FiringOrder.RangedWeaponUsageOrderEnum order)
	{
		MBAPI.IMBAgent.SetFiringOrder(GetPtr(), (int)order);
	}

	public int GetRidingOrder()
	{
		return MBAPI.IMBAgent.GetRidingOrder(GetPtr());
	}

	public void SetRidingOrder(RidingOrder.RidingOrderEnum order)
	{
		MBAPI.IMBAgent.SetRidingOrder(GetPtr(), (int)order);
	}

	public int GetTargetFormationIndex()
	{
		return MBAPI.IMBAgent.GetTargetFormationIndex(GetPtr());
	}

	public void SetTargetFormationIndex(int targetFormationIndex)
	{
		MBAPI.IMBAgent.SetTargetFormationIndex(GetPtr(), targetFormationIndex);
	}

	public void SetAgentFacialAnimation(FacialAnimChannel channel, string animationName, bool loop)
	{
		MBAPI.IMBAgent.SetAgentFacialAnimation(GetPtr(), (int)channel, animationName, loop);
	}

	public bool SetHandInverseKinematicsFrame(ref MatrixFrame leftGlobalFrame, ref MatrixFrame rightGlobalFrame)
	{
		return MBAPI.IMBAgent.SetHandInverseKinematicsFrame(GetPtr(), ref leftGlobalFrame, ref rightGlobalFrame);
	}

	public void SetNativeFormationNo(int formationNo)
	{
		MBAPI.IMBAgent.SetFormationNo(GetPtr(), formationNo);
	}

	public void SetDirectionChangeTendency(float tendency)
	{
		MBAPI.IMBAgent.SetDirectionChangeTendency(GetPtr(), tendency);
	}

	public float GetBattleImportance()
	{
		float num = Character?.GetBattlePower() ?? 1f;
		if (Team != null && this == Team.GeneralAgent)
		{
			num *= 2f;
		}
		else if (Formation != null && this == Formation.Captain)
		{
			num *= 1.2f;
		}
		return num;
	}

	public void SetSynchedPrefabComponentVisibility(int componentIndex, bool visibility)
	{
		_synchedBodyComponents[componentIndex].SetVisible(visibility);
		AgentVisuals.LazyUpdateAgentRendererData();
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SetAgentPrefabComponentVisibility(Index, componentIndex, visibility));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
	}

	public void SetActionSet(ref AnimationSystemData animationSystemData)
	{
		MBAPI.IMBAgent.SetActionSet(GetPtr(), ref animationSystemData);
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SetAgentActionSet(Index, animationSystemData));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
	}

	public void SetColumnwiseFollowAgent(Agent followAgent, ref Vec2 followPosition)
	{
		if (IsAIControlled)
		{
			int followAgentIndex = followAgent?.Index ?? (-1);
			MBAPI.IMBAgent.SetColumnwiseFollowAgent(GetPtr(), followAgentIndex, ref followPosition);
			this.SetFollowedUnit(followAgent);
		}
	}

	public void SetHandInverseKinematicsFrameForMissionObjectUsage(in MatrixFrame localIKFrame, in MatrixFrame boundEntityGlobalFrame, float animationHeightDifference = 0f)
	{
		if (GetCurrentActionValue(1) != ActionIndexValueCache.act_none && GetActionChannelWeight(1) > 0f)
		{
			MBAPI.IMBAgent.SetHandInverseKinematicsFrameForMissionObjectUsage(GetPtr(), in localIKFrame, in boundEntityGlobalFrame, animationHeightDifference);
		}
		else
		{
			ClearHandInverseKinematics();
		}
	}

	public void SetWantsToYell()
	{
		_wantsToYell = true;
		_yellTimer = MBRandom.RandomFloat * 0.3f + 0.1f;
	}

	public void SetCapeClothSimulator(GameEntityComponent clothSimulatorComponent)
	{
		ClothSimulatorComponent capeClothSimulator = clothSimulatorComponent as ClothSimulatorComponent;
		_capeClothSimulator = capeClothSimulator;
	}

	public Vec2 GetTargetPosition()
	{
		return MBAPI.IMBAgent.GetTargetPosition(GetPtr());
	}

	public Vec3 GetTargetDirection()
	{
		return MBAPI.IMBAgent.GetTargetDirection(GetPtr());
	}

	public float GetAimingTimer()
	{
		return MBAPI.IMBAgent.GetAimingTimer(GetPtr());
	}

	public float GetInteractionDistanceToUsable(IUsable usable)
	{
		if (usable is Agent agent)
		{
			if (!agent.IsMount)
			{
				return 3f;
			}
			return 1.75f;
		}
		if (usable is SpawnedItemEntity spawnedItemEntity && spawnedItemEntity.IsBanner())
		{
			return 3f;
		}
		float interactionDistance = MissionGameModels.Current.AgentStatCalculateModel.GetInteractionDistance(this);
		if (!(usable is StandingPoint))
		{
			return interactionDistance;
		}
		if (!IsAIControlled || !WalkMode)
		{
			return 1f;
		}
		return 0.5f;
	}

	public TextObject GetInfoTextForBeingNotInteractable(Agent userAgent)
	{
		if (IsMount && !userAgent.CheckSkillForMounting(this))
		{
			return GameTexts.FindText("str_ui_riding_skill_not_adequate_to_mount");
		}
		return TextObject.Empty;
	}

	public T GetController<T>() where T : AgentController
	{
		for (int i = 0; i < _agentControllers.Count; i++)
		{
			if (_agentControllers[i] is T)
			{
				return (T)_agentControllers[i];
			}
		}
		return null;
	}

	public EquipmentIndex GetWieldedItemIndex(HandIndex index)
	{
		return MBAPI.IMBAgent.GetWieldedItemIndex(GetPtr(), (int)index);
	}

	public float GetTrackDistanceToMainAgent()
	{
		float result = -1f;
		if (Main != null)
		{
			result = Main.Position.Distance(Position);
		}
		return result;
	}

	public string GetDescriptionText(GameEntity gameEntity = null)
	{
		return Name;
	}

	public GameEntity GetWeaponEntityFromEquipmentSlot(EquipmentIndex slotIndex)
	{
		return new GameEntity(MBAPI.IMBAgent.GetWeaponEntityFromEquipmentSlot(GetPtr(), (int)slotIndex));
	}

	public WorldPosition GetRetreatPos()
	{
		return MBAPI.IMBAgent.GetRetreatPos(GetPtr());
	}

	public AIScriptedFrameFlags GetScriptedFlags()
	{
		return (AIScriptedFrameFlags)MBAPI.IMBAgent.GetScriptedFlags(GetPtr());
	}

	public AISpecialCombatModeFlags GetScriptedCombatFlags()
	{
		return (AISpecialCombatModeFlags)MBAPI.IMBAgent.GetScriptedCombatFlags(GetPtr());
	}

	public GameEntity GetSteppedEntity()
	{
		UIntPtr steppedEntityId = MBAPI.IMBAgent.GetSteppedEntityId(GetPtr());
		if (!(steppedEntityId != UIntPtr.Zero))
		{
			return null;
		}
		return new GameEntity(steppedEntityId);
	}

	public AnimFlags GetCurrentAnimationFlag(int channelNo)
	{
		return (AnimFlags)MBAPI.IMBAgent.GetCurrentAnimationFlags(GetPtr(), channelNo);
	}

	public ActionIndexCache GetCurrentAction(int channelNo)
	{
		return new ActionIndexCache(MBAPI.IMBAgent.GetCurrentAction(GetPtr(), channelNo));
	}

	public ActionIndexValueCache GetCurrentActionValue(int channelNo)
	{
		return new ActionIndexValueCache(MBAPI.IMBAgent.GetCurrentAction(GetPtr(), channelNo));
	}

	public ActionCodeType GetCurrentActionType(int channelNo)
	{
		return (ActionCodeType)MBAPI.IMBAgent.GetCurrentActionType(GetPtr(), channelNo);
	}

	public ActionStage GetCurrentActionStage(int channelNo)
	{
		return (ActionStage)MBAPI.IMBAgent.GetCurrentActionStage(GetPtr(), channelNo);
	}

	public UsageDirection GetCurrentActionDirection(int channelNo)
	{
		return (UsageDirection)MBAPI.IMBAgent.GetCurrentActionDirection(GetPtr(), channelNo);
	}

	public int GetCurrentActionPriority(int channelNo)
	{
		return MBAPI.IMBAgent.GetCurrentActionPriority(GetPtr(), channelNo);
	}

	public float GetCurrentActionProgress(int channelNo)
	{
		return MBAPI.IMBAgent.GetCurrentActionProgress(GetPtr(), channelNo);
	}

	public float GetActionChannelWeight(int channelNo)
	{
		return MBAPI.IMBAgent.GetActionChannelWeight(GetPtr(), channelNo);
	}

	public float GetActionChannelCurrentActionWeight(int channelNo)
	{
		return MBAPI.IMBAgent.GetActionChannelCurrentActionWeight(GetPtr(), channelNo);
	}

	public WorldFrame GetWorldFrame()
	{
		return new WorldFrame(LookRotation, GetWorldPosition());
	}

	public float GetLookDownLimit()
	{
		return MBAPI.IMBAgent.GetLookDownLimit(GetPtr());
	}

	public float GetEyeGlobalHeight()
	{
		return MBAPI.IMBAgent.GetEyeGlobalHeight(GetPtr());
	}

	public float GetMaximumSpeedLimit()
	{
		return MBAPI.IMBAgent.GetMaximumSpeedLimit(GetPtr());
	}

	public Vec2 GetCurrentVelocity()
	{
		return MBAPI.IMBAgent.GetCurrentVelocity(GetPtr());
	}

	public float GetTurnSpeed()
	{
		return MBAPI.IMBAgent.GetTurnSpeed(GetPtr());
	}

	public float GetCurrentSpeedLimit()
	{
		return MBAPI.IMBAgent.GetCurrentSpeedLimit(GetPtr());
	}

	public Vec2 GetMovementDirection()
	{
		return MBAPI.IMBAgent.GetMovementDirection(GetPtr());
	}

	public Vec3 GetCurWeaponOffset()
	{
		return MBAPI.IMBAgent.GetCurWeaponOffset(GetPtr());
	}

	public bool GetIsLeftStance()
	{
		return MBAPI.IMBAgent.GetIsLeftStance(GetPtr());
	}

	public float GetPathDistanceToPoint(ref Vec3 point)
	{
		return MBAPI.IMBAgent.GetPathDistanceToPoint(GetPtr(), ref point);
	}

	public int GetCurrentNavigationFaceId()
	{
		return MBAPI.IMBAgent.GetCurrentNavigationFaceId(GetPtr());
	}

	public WorldPosition GetWorldPosition()
	{
		return MBAPI.IMBAgent.GetWorldPosition(GetPtr());
	}

	public Agent GetLookAgent()
	{
		return _lookAgentCache;
	}

	public Agent GetTargetAgent()
	{
		return MBAPI.IMBAgent.GetTargetAgent(GetPtr());
	}

	public void SetTargetAgent(Agent agent)
	{
		MBAPI.IMBAgent.SetTargetAgent(GetPtr(), agent?.Index ?? (-1));
	}

	public void SetAutomaticTargetSelection(bool enable)
	{
		MBAPI.IMBAgent.SetAutomaticTargetSelection(GetPtr(), enable);
	}

	public AgentFlag GetAgentFlags()
	{
		return AgentHelper.GetAgentFlags(FlagsPointer);
	}

	public string GetAgentFacialAnimation()
	{
		return MBAPI.IMBAgent.GetAgentFacialAnimation(GetPtr());
	}

	public string GetAgentVoiceDefinition()
	{
		return MBAPI.IMBAgent.GetAgentVoiceDefinition(GetPtr());
	}

	public Vec3 GetEyeGlobalPosition()
	{
		return MBAPI.IMBAgent.GetEyeGlobalPosition(GetPtr());
	}

	public Vec3 GetChestGlobalPosition()
	{
		return MBAPI.IMBAgent.GetChestGlobalPosition(GetPtr());
	}

	public MovementControlFlag GetDefendMovementFlag()
	{
		return MBAPI.IMBAgent.GetDefendMovementFlag(GetPtr());
	}

	public UsageDirection GetAttackDirection()
	{
		return MBAPI.IMBAgent.GetAttackDirection(GetPtr());
	}

	public WeaponInfo GetWieldedWeaponInfo(HandIndex handIndex)
	{
		bool isMeleeWeapon = false;
		bool isRangedWeapon = false;
		if (MBAPI.IMBAgent.GetWieldedWeaponInfo(GetPtr(), (int)handIndex, ref isMeleeWeapon, ref isRangedWeapon))
		{
			return new WeaponInfo(isValid: true, isMeleeWeapon, isRangedWeapon);
		}
		return new WeaponInfo(isValid: false, isMeleeWeapon: false, isRangedWeapon: false);
	}

	public Vec2 GetBodyRotationConstraint(int channelIndex = 1)
	{
		return MBAPI.IMBAgent.GetBodyRotationConstraint(GetPtr(), channelIndex).AsVec2;
	}

	public float GetTotalEncumbrance()
	{
		return AgentDrivenProperties.ArmorEncumbrance + AgentDrivenProperties.WeaponsEncumbrance;
	}

	public T GetComponent<T>() where T : AgentComponent
	{
		for (int i = 0; i < _components.Count; i++)
		{
			if (_components[i] is T)
			{
				return (T)_components[i];
			}
		}
		return null;
	}

	public float GetAgentDrivenPropertyValue(DrivenProperty type)
	{
		return AgentDrivenProperties.GetStat(type);
	}

	public UsableMachine GetSteppedMachine()
	{
		GameEntity gameEntity = GetSteppedEntity();
		while (gameEntity != null && !gameEntity.HasScriptOfType<UsableMachine>())
		{
			gameEntity = gameEntity.Parent;
		}
		if (gameEntity != null)
		{
			return gameEntity.GetFirstScriptOfType<UsableMachine>();
		}
		return null;
	}

	public int GetAttachedWeaponsCount()
	{
		return _attachedWeapons?.Count ?? 0;
	}

	public MissionWeapon GetAttachedWeapon(int index)
	{
		return _attachedWeapons[index].Item1;
	}

	public MatrixFrame GetAttachedWeaponFrame(int index)
	{
		return _attachedWeapons[index].Item2;
	}

	public sbyte GetAttachedWeaponBoneIndex(int index)
	{
		return _attachedWeapons[index].Item3;
	}

	public void DeleteAttachedWeapon(int index)
	{
		_attachedWeapons.RemoveAt(index);
		MBAPI.IMBAgent.DeleteAttachedWeaponFromBone(Pointer, index);
	}

	public bool HasRangedWeapon(bool checkHasAmmo = false)
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
		{
			if (!Equipment[equipmentIndex].IsEmpty && Equipment[equipmentIndex].GetRangedUsageIndex() >= 0 && (!checkHasAmmo || Equipment.HasAmmo(equipmentIndex, out var _, out var _, out var _)))
			{
				return true;
			}
		}
		return false;
	}

	public void GetFormationFileAndRankInfo(out int fileIndex, out int rankIndex)
	{
		fileIndex = ((IFormationUnit)this).FormationFileIndex;
		rankIndex = ((IFormationUnit)this).FormationRankIndex;
	}

	public void GetFormationFileAndRankInfo(out int fileIndex, out int rankIndex, out int fileCount, out int rankCount)
	{
		fileIndex = ((IFormationUnit)this).FormationFileIndex;
		rankIndex = ((IFormationUnit)this).FormationRankIndex;
		if (((IFormationUnit)this).Formation is LineFormation lineFormation)
		{
			lineFormation.GetFormationInfo(out fileCount, out rankCount);
			return;
		}
		fileCount = -1;
		rankCount = -1;
	}

	internal Vec2 GetWallDirectionOfRelativeFormationLocation()
	{
		return Formation.GetWallDirectionOfRelativeFormationLocation(this);
	}

	public void SetMortalityState(MortalityState newState)
	{
		CurrentMortalityState = newState;
	}

	public void ToggleInvulnerable()
	{
		if (CurrentMortalityState == MortalityState.Invulnerable)
		{
			CurrentMortalityState = MortalityState.Mortal;
		}
		else
		{
			CurrentMortalityState = MortalityState.Invulnerable;
		}
	}

	public float GetArmLength()
	{
		return Monster.ArmLength * AgentScale;
	}

	public float GetArmWeight()
	{
		return Monster.ArmWeight * AgentScale;
	}

	public void GetRunningSimulationDataUntilMaximumSpeedReached(ref float combatAccelerationTime, ref float maxSpeed, float[] speedValues)
	{
		MBAPI.IMBAgent.GetRunningSimulationDataUntilMaximumSpeedReached(GetPtr(), ref combatAccelerationTime, ref maxSpeed, speedValues);
	}

	public void SetMaximumSpeedLimit(float maximumSpeedLimit, bool isMultiplier)
	{
		MBAPI.IMBAgent.SetMaximumSpeedLimit(GetPtr(), maximumSpeedLimit, isMultiplier);
	}

	public float GetBaseArmorEffectivenessForBodyPart(BoneBodyPartType bodyPart)
	{
		if (!IsHuman)
		{
			return GetAgentDrivenPropertyValue(DrivenProperty.ArmorTorso);
		}
		switch (bodyPart)
		{
		case BoneBodyPartType.None:
			return 0f;
		case BoneBodyPartType.Head:
		case BoneBodyPartType.Neck:
			return GetAgentDrivenPropertyValue(DrivenProperty.ArmorHead);
		case BoneBodyPartType.Legs:
			return GetAgentDrivenPropertyValue(DrivenProperty.ArmorLegs);
		case BoneBodyPartType.ArmLeft:
		case BoneBodyPartType.ArmRight:
			return GetAgentDrivenPropertyValue(DrivenProperty.ArmorArms);
		case BoneBodyPartType.Chest:
		case BoneBodyPartType.Abdomen:
		case BoneBodyPartType.ShoulderLeft:
		case BoneBodyPartType.ShoulderRight:
			return GetAgentDrivenPropertyValue(DrivenProperty.ArmorTorso);
		default:
			TaleWorlds.Library.Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Agent.cs", "GetBaseArmorEffectivenessForBodyPart", 2827);
			return GetAgentDrivenPropertyValue(DrivenProperty.ArmorTorso);
		}
	}

	public AITargetVisibilityState GetLastTargetVisibilityState()
	{
		return (AITargetVisibilityState)MBAPI.IMBAgent.GetLastTargetVisibilityState(GetPtr());
	}

	public float GetMissileRange()
	{
		return MBAPI.IMBAgent.GetMissileRange(GetPtr());
	}

	public ItemObject GetWeaponToReplaceOnQuickAction(SpawnedItemEntity spawnedItem, out EquipmentIndex possibleSlotIndex)
	{
		EquipmentIndex equipmentIndex = (possibleSlotIndex = MissionEquipment.SelectWeaponPickUpSlot(this, spawnedItem.WeaponCopy, spawnedItem.IsStuckMissile()));
		if (equipmentIndex != EquipmentIndex.None && !Equipment[equipmentIndex].IsEmpty && ((!spawnedItem.IsStuckMissile() && !spawnedItem.WeaponCopy.IsAnyConsumable()) || Equipment[equipmentIndex].Item.PrimaryWeapon.WeaponClass != spawnedItem.WeaponCopy.Item.PrimaryWeapon.WeaponClass || !Equipment[equipmentIndex].IsAnyConsumable() || Equipment[equipmentIndex].Amount == Equipment[equipmentIndex].ModifiedMaxAmount))
		{
			return Equipment[equipmentIndex].Item;
		}
		return null;
	}

	public Hitter GetAssistingHitter(MissionPeer killerPeer)
	{
		Hitter hitter = null;
		foreach (Hitter hitter2 in HitterList)
		{
			if (hitter2.HitterPeer != killerPeer && (hitter == null || hitter2.Damage > hitter.Damage))
			{
				hitter = hitter2;
			}
		}
		if (hitter != null && hitter.Damage >= 35f)
		{
			return hitter;
		}
		return null;
	}

	public bool CanReachAgent(Agent otherAgent)
	{
		float interactionDistanceToUsable = GetInteractionDistanceToUsable(otherAgent);
		return Position.DistanceSquared(otherAgent.Position) < interactionDistanceToUsable * interactionDistanceToUsable;
	}

	public bool CanInteractWithAgent(Agent otherAgent, float userAgentCameraElevation)
	{
		bool flag = false;
		foreach (MissionBehavior missionBehavior in Mission.Current.MissionBehaviors)
		{
			flag = flag || missionBehavior.IsThereAgentAction(this, otherAgent);
		}
		if (!flag)
		{
			return false;
		}
		bool flag2 = CanReachAgent(otherAgent);
		if (otherAgent.IsMount)
		{
			if ((MountAgent == null && GetCurrentActionValue(0) != ActionIndexValueCache.act_none) || (MountAgent != null && !IsOnLand()))
			{
				return false;
			}
			if (otherAgent.RiderAgent == null)
			{
				if (MountAgent == null && flag2 && CheckSkillForMounting(otherAgent))
				{
					return otherAgent.GetCurrentActionType(0) != ActionCodeType.Rear;
				}
				return false;
			}
			if (otherAgent != MountAgent)
			{
				return false;
			}
			if (flag2 && userAgentCameraElevation < GetLookDownLimit() + 0.4f && GetCurrentVelocity().LengthSquared < 0.25f)
			{
				return otherAgent.GetCurrentActionType(0) != ActionCodeType.Rear;
			}
			return false;
		}
		return IsOnLand() && flag2;
	}

	public bool CanBeAssignedForScriptedMovement()
	{
		if (IsActive() && IsAIControlled && !IsDetachedFromFormation && !IsRunningAway && (GetScriptedFlags() & AIScriptedFrameFlags.GoToPosition) == 0)
		{
			return !InteractingWithAnyGameObject();
		}
		return false;
	}

	public bool CanReachAndUseObject(UsableMissionObject gameObject, float distanceSq)
	{
		if (CanReachObject(gameObject, distanceSq))
		{
			return CanUseObject(gameObject);
		}
		return false;
	}

	public bool CanReachObject(UsableMissionObject gameObject, float distanceSq)
	{
		if (IsItemUseDisabled || IsUsingGameObject)
		{
			return false;
		}
		float interactionDistanceToUsable = GetInteractionDistanceToUsable(gameObject);
		if (distanceSq <= interactionDistanceToUsable * interactionDistanceToUsable)
		{
			return TaleWorlds.Library.MathF.Abs(gameObject.InteractionEntity.GlobalPosition.z - Position.z) <= interactionDistanceToUsable * 2f;
		}
		return false;
	}

	public bool CanUseObject(UsableMissionObject gameObject)
	{
		if (!gameObject.IsDisabledForAgent(this))
		{
			return gameObject.IsUsableByAgent(this);
		}
		return false;
	}

	public bool CanMoveDirectlyToPosition(in Vec2 position)
	{
		return MBAPI.IMBAgent.CanMoveDirectlyToPosition(GetPtr(), in position);
	}

	public bool CanInteractableWeaponBePickedUp(SpawnedItemEntity spawnedItem)
	{
		if (spawnedItem.IsBanner() && !MissionGameModels.Current.BattleBannerBearersModel.IsInteractableFormationBanner(spawnedItem, this))
		{
			return false;
		}
		if (GetWeaponToReplaceOnQuickAction(spawnedItem, out var possibleSlotIndex) == null)
		{
			return possibleSlotIndex == EquipmentIndex.None;
		}
		return true;
	}

	public bool CanQuickPickUp(SpawnedItemEntity spawnedItem)
	{
		if (spawnedItem.IsBanner() && !MissionGameModels.Current.BattleBannerBearersModel.IsInteractableFormationBanner(spawnedItem, this))
		{
			return false;
		}
		return MissionEquipment.SelectWeaponPickUpSlot(this, spawnedItem.WeaponCopy, spawnedItem.IsStuckMissile()) != EquipmentIndex.None;
	}

	public bool CanTeleport()
	{
		if (Mission.IsTeleportingAgents)
		{
			if (Formation != null && Mission.Mode == MissionMode.Deployment)
			{
				return Formation.GetReadonlyMovementOrderReference().OrderEnum == MovementOrder.MovementOrderEnum.Move;
			}
			return true;
		}
		return false;
	}

	public bool IsActive()
	{
		return State == AgentState.Active;
	}

	public bool IsRetreating()
	{
		return MBAPI.IMBAgent.IsRetreating(GetPtr());
	}

	public bool IsFadingOut()
	{
		return MBAPI.IMBAgent.IsFadingOut(GetPtr());
	}

	public void SetAgentDrivenPropertyValueFromConsole(DrivenProperty type, float val)
	{
		AgentDrivenProperties.SetStat(type, val);
	}

	public bool IsOnLand()
	{
		return MBAPI.IMBAgent.IsOnLand(GetPtr());
	}

	public bool IsSliding()
	{
		return MBAPI.IMBAgent.IsSliding(GetPtr());
	}

	public bool IsSitting()
	{
		ActionCodeType currentActionType = GetCurrentActionType(0);
		if (currentActionType != ActionCodeType.Sit && currentActionType != ActionCodeType.SitOnTheFloor)
		{
			return currentActionType == ActionCodeType.SitOnAThrone;
		}
		return true;
	}

	public bool IsReleasingChainAttack()
	{
		bool result = false;
		if (Mission.Current.CurrentTime - _lastQuickReadyDetectedTime < 0.75f && GetCurrentActionStage(1) == ActionStage.AttackRelease)
		{
			result = true;
		}
		return result;
	}

	public bool IsCameraAttachable()
	{
		if (!_isDeleted && (!_isRemoved || _removalTime + 2.1f > Mission.CurrentTime) && IsHuman && AgentVisuals != null && AgentVisuals.IsValid())
		{
			if (!GameNetwork.IsSessionActive)
			{
				return _agentControllerType != ControllerType.None;
			}
			return true;
		}
		return false;
	}

	public bool IsSynchedPrefabComponentVisible(int componentIndex)
	{
		return _synchedBodyComponents[componentIndex].GetVisible();
	}

	public bool IsEnemyOf(Agent otherAgent)
	{
		return MBAPI.IMBAgent.IsEnemy(GetPtr(), otherAgent.GetPtr());
	}

	public bool IsFriendOf(Agent otherAgent)
	{
		return MBAPI.IMBAgent.IsFriend(GetPtr(), otherAgent.GetPtr());
	}

	public void OnFocusGain(Agent userAgent)
	{
	}

	public void OnFocusLose(Agent userAgent)
	{
	}

	public void OnItemRemovedFromScene()
	{
		StopUsingGameObjectMT(isSuccessful: false);
	}

	public void OnUse(Agent userAgent)
	{
		Mission.OnAgentInteraction(userAgent, this);
	}

	public void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
	{
	}

	public void OnWeaponDrop(EquipmentIndex equipmentSlot)
	{
		MissionWeapon droppedWeapon = Equipment[equipmentSlot];
		Equipment[equipmentSlot] = MissionWeapon.Invalid;
		WeaponEquipped(equipmentSlot, in WeaponData.InvalidWeaponData, null, in WeaponData.InvalidWeaponData, null, null, removeOldWeaponFromScene: false, isWieldedOnSpawn: false);
		foreach (AgentComponent component in _components)
		{
			component.OnWeaponDrop(droppedWeapon);
		}
	}

	public void OnItemPickup(SpawnedItemEntity spawnedItemEntity, EquipmentIndex weaponPickUpSlotIndex, out bool removeWeapon)
	{
		removeWeapon = true;
		bool flag = true;
		MissionWeapon weaponCopy = spawnedItemEntity.WeaponCopy;
		if (weaponPickUpSlotIndex == EquipmentIndex.None)
		{
			weaponPickUpSlotIndex = MissionEquipment.SelectWeaponPickUpSlot(this, weaponCopy, spawnedItemEntity.IsStuckMissile());
		}
		bool flag2 = false;
		switch (weaponPickUpSlotIndex)
		{
		case EquipmentIndex.ExtraWeaponSlot:
			if (!GameNetwork.IsClientOrReplay)
			{
				flag2 = true;
				if (!Equipment[weaponPickUpSlotIndex].IsEmpty)
				{
					DropItem(weaponPickUpSlotIndex, Equipment[weaponPickUpSlotIndex].Item.PrimaryWeapon.WeaponClass);
				}
			}
			break;
		default:
		{
			int num = 0;
			if ((spawnedItemEntity.IsStuckMissile() || spawnedItemEntity.WeaponCopy.IsAnyConsumable()) && !Equipment[weaponPickUpSlotIndex].IsEmpty && Equipment[weaponPickUpSlotIndex].IsSameType(weaponCopy) && Equipment[weaponPickUpSlotIndex].IsAnyConsumable())
			{
				num = Equipment[weaponPickUpSlotIndex].ModifiedMaxAmount - Equipment[weaponPickUpSlotIndex].Amount;
			}
			if (num > 0)
			{
				short num2 = (short)TaleWorlds.Library.MathF.Min(num, weaponCopy.Amount);
				if (num2 != weaponCopy.Amount)
				{
					removeWeapon = false;
					if (!GameNetwork.IsClientOrReplay)
					{
						spawnedItemEntity.ConsumeWeaponAmount(num2);
						if (GameNetwork.IsServer)
						{
							GameNetwork.BeginBroadcastModuleEvent();
							GameNetwork.WriteMessage(new ConsumeWeaponAmount(spawnedItemEntity.Id, num2));
							GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
						}
					}
				}
				if (!GameNetwork.IsClientOrReplay)
				{
					SetWeaponAmountInSlot(weaponPickUpSlotIndex, (short)(Equipment[weaponPickUpSlotIndex].Amount + num2), enforcePrimaryItem: true);
					if (GetWieldedItemIndex(HandIndex.MainHand) == EquipmentIndex.None && (weaponCopy.Item.PrimaryWeapon.IsRangedWeapon || weaponCopy.Item.PrimaryWeapon.IsMeleeWeapon))
					{
						flag2 = true;
					}
				}
			}
			else if (!GameNetwork.IsClientOrReplay)
			{
				flag2 = true;
				if (!Equipment[weaponPickUpSlotIndex].IsEmpty)
				{
					DropItem(weaponPickUpSlotIndex, weaponCopy.Item.PrimaryWeapon.WeaponClass);
				}
			}
			break;
		}
		case EquipmentIndex.None:
			break;
		}
		if (!GameNetwork.IsClientOrReplay)
		{
			flag = MissionEquipment.DoesWeaponFitToSlot(weaponPickUpSlotIndex, weaponCopy);
			if (flag)
			{
				EquipWeaponFromSpawnedItemEntity(weaponPickUpSlotIndex, spawnedItemEntity, removeWeapon);
				if (flag2)
				{
					EquipmentIndex slotIndex = weaponPickUpSlotIndex;
					if (weaponCopy.Item.PrimaryWeapon.AmmoClass == weaponCopy.Item.PrimaryWeapon.WeaponClass)
					{
						for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < weaponPickUpSlotIndex; equipmentIndex++)
						{
							if (!Equipment[equipmentIndex].IsEmpty && weaponCopy.IsEqualTo(Equipment[equipmentIndex]))
							{
								slotIndex = equipmentIndex;
								break;
							}
						}
					}
					TryToWieldWeaponInSlot(slotIndex, WeaponWieldActionType.InstantAfterPickUp, isWieldedOnSpawn: false);
				}
				for (int i = 0; i < _components.Count; i++)
				{
					_components[i].OnItemPickup(spawnedItemEntity);
				}
				if (Controller == ControllerType.AI)
				{
					HumanAIComponent.ItemPickupDone(spawnedItemEntity);
				}
			}
		}
		if (flag)
		{
			Mission.TriggerOnItemPickUpEvent(this, spawnedItemEntity);
		}
	}

	public bool CheckTracked(BasicCharacterObject basicCharacter)
	{
		return Character == basicCharacter;
	}

	public bool CheckPathToAITargetAgentPassesThroughNavigationFaceIdFromDirection(int navigationFaceId, Vec3 direction, float overridenCostForFaceId)
	{
		return MBAPI.IMBAgent.CheckPathToAITargetAgentPassesThroughNavigationFaceIdFromDirection(GetPtr(), navigationFaceId, ref direction, overridenCostForFaceId);
	}

	public void CheckEquipmentForCapeClothSimulationStateChange()
	{
		if (!(_capeClothSimulator != null))
		{
			return;
		}
		bool flag = false;
		EquipmentIndex wieldedItemIndex = GetWieldedItemIndex(HandIndex.OffHand);
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.ExtraWeaponSlot; equipmentIndex++)
		{
			MissionWeapon missionWeapon = Equipment[equipmentIndex];
			if (!missionWeapon.IsEmpty && missionWeapon.IsShield() && equipmentIndex != wieldedItemIndex)
			{
				flag = true;
				break;
			}
		}
		_capeClothSimulator.SetMaxDistanceMultiplier(flag ? 0f : 1f);
	}

	public void CheckToDropFlaggedItem()
	{
		if (!GetAgentFlags().HasAnyFlag(AgentFlag.CanWieldWeapon))
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			EquipmentIndex wieldedItemIndex = GetWieldedItemIndex((HandIndex)i);
			if (wieldedItemIndex != EquipmentIndex.None && Equipment[wieldedItemIndex].Item.ItemFlags.HasAnyFlag(ItemFlags.DropOnAnyAction))
			{
				DropItem(wieldedItemIndex);
			}
		}
	}

	public bool CheckSkillForMounting(Agent mountAgent)
	{
		int effectiveSkill = MissionGameModels.Current.AgentStatCalculateModel.GetEffectiveSkill(this, DefaultSkills.Riding);
		if ((GetAgentFlags() & AgentFlag.CanRide) != 0)
		{
			return (float)effectiveSkill >= mountAgent.GetAgentDrivenPropertyValue(DrivenProperty.MountDifficulty);
		}
		return false;
	}

	public void InitializeSpawnEquipment(Equipment spawnEquipment)
	{
		SpawnEquipment = spawnEquipment;
	}

	public void InitializeMissionEquipment(MissionEquipment missionEquipment, Banner banner)
	{
		Equipment = missionEquipment ?? new MissionEquipment(SpawnEquipment, banner);
	}

	public void InitializeAgentProperties(Equipment spawnEquipment, AgentBuildData agentBuildData)
	{
		_propertyModifiers = default(AgentPropertiesModifiers);
		AgentDrivenProperties = new AgentDrivenProperties();
		float[] values = AgentDrivenProperties.InitializeDrivenProperties(this, spawnEquipment, agentBuildData);
		UpdateDrivenProperties(values);
		if (IsMount && RiderAgent == null)
		{
			Mission.Current.AddMountWithoutRider(this);
		}
	}

	public void UpdateFormationOrders()
	{
		if (Formation != null && !IsRetreating())
		{
			EnforceShieldUsage(ArrangementOrder.GetShieldDirectionOfUnit(Formation, this, Formation.ArrangementOrder.OrderEnum));
		}
	}

	public void UpdateWeapons()
	{
		MBAPI.IMBAgent.UpdateWeapons(GetPtr());
	}

	public void UpdateAgentProperties()
	{
		if (AgentDrivenProperties != null)
		{
			float[] values = AgentDrivenProperties.UpdateDrivenProperties(this);
			UpdateDrivenProperties(values);
		}
	}

	public void UpdateCustomDrivenProperties()
	{
		if (AgentDrivenProperties != null)
		{
			UpdateDrivenProperties(AgentDrivenProperties.Values);
		}
	}

	public void UpdateBodyProperties(BodyProperties bodyProperties)
	{
		BodyPropertiesValue = bodyProperties;
	}

	public void UpdateSyncHealthToAllClients(bool value)
	{
		SyncHealthToAllClients = value;
	}

	public void UpdateSpawnEquipmentAndRefreshVisuals(Equipment newSpawnEquipment)
	{
		SpawnEquipment = newSpawnEquipment;
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SynchronizeAgentSpawnEquipment(Index, SpawnEquipment));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		AgentVisuals.ClearVisualComponents(removeSkeleton: false);
		Mission.OnEquipItemsFromSpawnEquipment(this, CreationType.FromCharacterObj);
		AgentVisuals.ClearAllWeaponMeshes();
		Equipment.FillFrom(SpawnEquipment, Origin?.Banner);
		CheckEquipmentForCapeClothSimulationStateChange();
		EquipItemsFromSpawnEquipment(neededBatchedItems: true);
		UpdateAgentProperties();
		if (!Mission.Current.DoesMissionRequireCivilianEquipment && !GameNetwork.IsClientOrReplay)
		{
			WieldInitialWeapons();
		}
		PreloadForRendering();
	}

	public void UpdateCachedAndFormationValues(bool updateOnlyMovement, bool arrangementChangeAllowed)
	{
		if (!IsActive())
		{
			return;
		}
		if (!updateOnlyMovement)
		{
			WalkSpeedCached = MountAgent?.WalkingSpeedLimitOfMountable ?? Monster.WalkingSpeedLimit;
			RunSpeedCached = MaximumForwardUnlimitedSpeed;
		}
		if (GameNetwork.IsClientOrReplay)
		{
			return;
		}
		if (!updateOnlyMovement && !IsDetachedFromFormation)
		{
			Formation?.Arrangement.OnTickOccasionallyOfUnit(this, arrangementChangeAllowed);
		}
		if (IsAIControlled)
		{
			HumanAIComponent.UpdateFormationMovement();
		}
		if (!updateOnlyMovement)
		{
			Formation?.Team.DetachmentManager.TickAgent(this);
		}
		if (!updateOnlyMovement && IsAIControlled)
		{
			UpdateFormationOrders();
			if (Formation != null)
			{
				GetFormationFileAndRankInfo(out var fileIndex, out var rankIndex, out var fileCount, out var rankCount);
				Vec2 wallDirectionOfRelativeFormationLocation = GetWallDirectionOfRelativeFormationLocation();
				MBAPI.IMBAgent.SetFormationInfo(GetPtr(), fileIndex, rankIndex, fileCount, rankCount, wallDirectionOfRelativeFormationLocation, Formation.UnitSpacing);
			}
		}
	}

	public void UpdateLastRangedAttackTimeDueToAnAttack(float newTime)
	{
		LastRangedAttackTime = newTime;
	}

	public void InvalidateTargetAgent()
	{
		MBAPI.IMBAgent.InvalidateTargetAgent(GetPtr());
	}

	public void InvalidateAIWeaponSelections()
	{
		MBAPI.IMBAgent.InvalidateAIWeaponSelections(GetPtr());
	}

	public void ResetLookAgent()
	{
		SetLookAgent(null);
	}

	public void ResetGuard()
	{
		MBAPI.IMBAgent.ResetGuard(GetPtr());
	}

	public void ResetAgentProperties()
	{
		AgentDrivenProperties = null;
	}

	public void ResetAiWaitBeforeShootFactor()
	{
		_propertyModifiers.resetAiWaitBeforeShootFactor = true;
	}

	public void ClearTargetFrame()
	{
		_checkIfTargetFrameIsChanged = false;
		if (MovementLockedState != 0)
		{
			ClearTargetFrameAux();
			if (GameNetwork.IsServerOrRecorder)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new ClearAgentTargetFrame(Index));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
			}
		}
	}

	public void ClearEquipment()
	{
		MBAPI.IMBAgent.ClearEquipment(GetPtr());
	}

	public void ClearHandInverseKinematics()
	{
		MBAPI.IMBAgent.ClearHandInverseKinematics(GetPtr());
	}

	public void ClearAttachedWeapons()
	{
		_attachedWeapons?.Clear();
	}

	public void SetDetachableFromFormation(bool value)
	{
		bool isDetachableFromFormation = _isDetachableFromFormation;
		if (isDetachableFromFormation == value)
		{
			return;
		}
		if (isDetachableFromFormation)
		{
			if (IsDetachedFromFormation)
			{
				_detachment.RemoveAgent(this);
				_formation?.AttachUnit(this);
			}
			_formation?.Team?.DetachmentManager.RemoveScoresOfAgentFromDetachments(this);
		}
		_isDetachableFromFormation = value;
		if (!IsPlayerControlled)
		{
			if (isDetachableFromFormation)
			{
				_formation?.OnUndetachableNonPlayerUnitAdded(this);
			}
			else
			{
				_formation?.OnUndetachableNonPlayerUnitRemoved(this);
			}
		}
	}

	public void EnforceShieldUsage(UsageDirection shieldDirection)
	{
		MBAPI.IMBAgent.EnforceShieldUsage(GetPtr(), shieldDirection);
	}

	public bool ObjectHasVacantPosition(UsableMissionObject gameObject)
	{
		if (gameObject.HasUser)
		{
			return gameObject.HasAIUser;
		}
		return true;
	}

	public bool InteractingWithAnyGameObject()
	{
		if (!IsUsingGameObject)
		{
			if (IsAIControlled)
			{
				return this.AIInterestedInAnyGameObject();
			}
			return false;
		}
		return true;
	}

	private void StopUsingGameObjectAux(bool isSuccessful, StopUsingGameObjectFlags flags)
	{
		UsableMachine usableMachine = ((Controller != ControllerType.AI || Formation == null) ? null : (Formation.GetDetachmentOrDefault(this) as UsableMachine));
		if (usableMachine == null)
		{
			flags &= ~StopUsingGameObjectFlags.AutoAttachAfterStoppingUsingGameObject;
		}
		UsableMissionObject currentlyUsedGameObject = CurrentlyUsedGameObject;
		UsableMissionObject movingToOrDefendingObject = null;
		if (!IsUsingGameObject && IsAIControlled)
		{
			movingToOrDefendingObject = ((!this.AIMoveToGameObjectIsEnabled()) ? HumanAIComponent.GetCurrentlyDefendingGameObject() : HumanAIComponent.GetCurrentlyMovingGameObject());
		}
		if (IsUsingGameObject)
		{
			bool num = CurrentlyUsedGameObject.LockUserFrames || CurrentlyUsedGameObject.LockUserPositions;
			if (GameNetwork.IsServerOrRecorder)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new StopUsingObject(Index, isSuccessful));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
			}
			CurrentlyUsedGameObject.OnUseStopped(this, isSuccessful, _usedObjectPreferenceIndex);
			CurrentlyUsedGameObject = null;
			if (IsAIControlled)
			{
				this.AIUseGameObjectDisable();
			}
			_usedObjectPreferenceIndex = -1;
			if (num)
			{
				ClearTargetFrame();
			}
		}
		else if (IsAIControlled)
		{
			if (this.AIDefendGameObjectIsEnabled())
			{
				this.AIDefendGameObjectDisable();
			}
			else
			{
				this.AIMoveToGameObjectDisable();
			}
		}
		if (State == AgentState.Active)
		{
			if (IsAIControlled)
			{
				DisableScriptedMovement();
				if (usableMachine != null)
				{
					foreach (StandingPoint standingPoint in usableMachine.StandingPoints)
					{
						standingPoint.FavoredUser = this;
					}
				}
			}
			AfterStoppedUsingMissionObject(usableMachine, currentlyUsedGameObject, movingToOrDefendingObject, isSuccessful, flags);
		}
		Mission.OnObjectStoppedBeingUsed(this, CurrentlyUsedGameObject);
		_components.ForEach(delegate(AgentComponent ac)
		{
			ac.OnStopUsingGameObject();
		});
	}

	public void StopUsingGameObjectMT(bool isSuccessful = true, StopUsingGameObjectFlags flags = StopUsingGameObjectFlags.AutoAttachAfterStoppingUsingGameObject)
	{
		lock (_stopUsingGameObjectLock)
		{
			StopUsingGameObjectAux(isSuccessful, flags);
		}
	}

	public void StopUsingGameObject(bool isSuccessful = true, StopUsingGameObjectFlags flags = StopUsingGameObjectFlags.AutoAttachAfterStoppingUsingGameObject)
	{
		using (new TWParallel.RecursiveSingleThreadTestBlock(TWParallel.RecursiveSingleThreadTestData.GlobalData))
		{
			StopUsingGameObjectAux(isSuccessful, flags);
		}
	}

	public void HandleStopUsingAction()
	{
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage(new RequestStopUsingObject());
			GameNetwork.EndModuleEventAsClient();
		}
		else
		{
			StopUsingGameObject(isSuccessful: false);
		}
	}

	public void HandleStartUsingAction(UsableMissionObject targetObject, int preferenceIndex)
	{
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage(new RequestUseObject(targetObject.Id, preferenceIndex));
			GameNetwork.EndModuleEventAsClient();
		}
		else
		{
			UseGameObject(targetObject, preferenceIndex);
		}
	}

	public AgentController AddController(Type type)
	{
		AgentController agentController = null;
		if (type.IsSubclassOf(typeof(AgentController)))
		{
			agentController = Activator.CreateInstance(type) as AgentController;
		}
		if (agentController != null)
		{
			agentController.Owner = this;
			agentController.Mission = Mission;
			_agentControllers.Add(agentController);
			agentController.OnInitialize();
		}
		return agentController;
	}

	public AgentController RemoveController(Type type)
	{
		for (int i = 0; i < _agentControllers.Count; i++)
		{
			if (type.IsInstanceOfType(_agentControllers[i]))
			{
				AgentController result = _agentControllers[i];
				_agentControllers.RemoveAt(i);
				return result;
			}
		}
		return null;
	}

	public bool CanThrustAttackStickToBone(BoneBodyPartType bodyPart)
	{
		if (IsHuman)
		{
			BoneBodyPartType[] array = new BoneBodyPartType[8]
			{
				BoneBodyPartType.Abdomen,
				BoneBodyPartType.Legs,
				BoneBodyPartType.Chest,
				BoneBodyPartType.Neck,
				BoneBodyPartType.ShoulderLeft,
				BoneBodyPartType.ShoulderRight,
				BoneBodyPartType.ArmLeft,
				BoneBodyPartType.ArmRight
			};
			for (int i = 0; i < array.Length; i++)
			{
				if (bodyPart == array[i])
				{
					return true;
				}
			}
		}
		return false;
	}

	public void StartSwitchingWeaponUsageIndexAsClient(EquipmentIndex equipmentIndex, int usageIndex, UsageDirection currentMovementFlagUsageDirection)
	{
		MBAPI.IMBAgent.StartSwitchingWeaponUsageIndexAsClient(GetPtr(), (int)equipmentIndex, usageIndex, currentMovementFlagUsageDirection);
	}

	public void TryToWieldWeaponInSlot(EquipmentIndex slotIndex, WeaponWieldActionType type, bool isWieldedOnSpawn)
	{
		MBAPI.IMBAgent.TryToWieldWeaponInSlot(GetPtr(), (int)slotIndex, (int)type, isWieldedOnSpawn);
	}

	public void PrepareWeaponForDropInEquipmentSlot(EquipmentIndex slotIndex, bool dropWithHolster)
	{
		MBAPI.IMBAgent.PrepareWeaponForDropInEquipmentSlot(GetPtr(), (int)slotIndex, dropWithHolster);
	}

	public void AddHitter(MissionPeer peer, float damage, bool isFriendlyHit)
	{
		Hitter hitter = _hitterList.Find((Hitter h) => h.HitterPeer == peer && h.IsFriendlyHit == isFriendlyHit);
		if (hitter == null)
		{
			hitter = new Hitter(peer, damage, isFriendlyHit);
			_hitterList.Add(hitter);
		}
		else
		{
			hitter.IncreaseDamage(damage);
		}
	}

	public void TryToSheathWeaponInHand(HandIndex handIndex, WeaponWieldActionType type)
	{
		MBAPI.IMBAgent.TryToSheathWeaponInHand(GetPtr(), (int)handIndex, (int)type);
	}

	public void RemoveHitter(MissionPeer peer, bool isFriendlyHit)
	{
		Hitter hitter = _hitterList.Find((Hitter h) => h.HitterPeer == peer && h.IsFriendlyHit == isFriendlyHit);
		if (hitter != null)
		{
			_hitterList.Remove(hitter);
		}
	}

	public void Retreat(WorldPosition retreatPos)
	{
		MBAPI.IMBAgent.SetRetreatMode(GetPtr(), retreatPos, retreat: true);
	}

	public void StopRetreating()
	{
		MBAPI.IMBAgent.SetRetreatMode(GetPtr(), WorldPosition.Invalid, retreat: false);
		IsRunningAway = false;
	}

	public void UseGameObject(UsableMissionObject usedObject, int preferenceIndex = -1)
	{
		if (usedObject.LockUserFrames)
		{
			WorldFrame userFrameForAgent = usedObject.GetUserFrameForAgent(this);
			SetTargetPositionAndDirection(userFrameForAgent.Origin.AsVec2, userFrameForAgent.Rotation.f);
			SetScriptedFlags(GetScriptedFlags() | AIScriptedFrameFlags.NoAttack);
		}
		else if (usedObject.LockUserPositions)
		{
			SetTargetPosition(usedObject.GetUserFrameForAgent(this).Origin.AsVec2);
			SetScriptedFlags(GetScriptedFlags() | AIScriptedFrameFlags.NoAttack);
		}
		if (IsActive() && IsAIControlled && this.AIMoveToGameObjectIsEnabled())
		{
			this.AIMoveToGameObjectDisable();
			Formation?.Team.DetachmentManager.RemoveScoresOfAgentFromDetachments(this);
		}
		CurrentlyUsedGameObject = usedObject;
		_usedObjectPreferenceIndex = preferenceIndex;
		if (IsAIControlled)
		{
			this.AIUseGameObjectEnable();
		}
		_equipmentOnMainHandBeforeUsingObject = GetWieldedItemIndex(HandIndex.MainHand);
		_equipmentOnOffHandBeforeUsingObject = GetWieldedItemIndex(HandIndex.OffHand);
		usedObject.OnUse(this);
		Mission.OnObjectUsed(this, usedObject);
		if (usedObject.IsInstantUse && !GameNetwork.IsClientOrReplay && IsActive() && InteractingWithAnyGameObject())
		{
			StopUsingGameObject();
		}
	}

	public void StartFadingOut()
	{
		MBAPI.IMBAgent.StartFadingOut(GetPtr());
	}

	public void SetRenderCheckEnabled(bool value)
	{
		MBAPI.IMBAgent.SetRenderCheckEnabled(GetPtr(), value);
	}

	public bool GetRenderCheckEnabled()
	{
		return MBAPI.IMBAgent.GetRenderCheckEnabled(GetPtr());
	}

	public Vec3 ComputeAnimationDisplacement(float dt)
	{
		return MBAPI.IMBAgent.ComputeAnimationDisplacement(GetPtr(), dt);
	}

	public void TickActionChannels(float dt)
	{
		MBAPI.IMBAgent.TickActionChannels(GetPtr(), dt);
	}

	public void LockAgentReplicationTableDataWithCurrentReliableSequenceNo(NetworkCommunicator peer)
	{
		MBDebug.Print("peer: " + peer.UserName + " index: " + Index + " name: " + Name);
		MBAPI.IMBAgent.LockAgentReplicationTableDataWithCurrentReliableSequenceNo(GetPtr(), peer.Index);
	}

	public void TeleportToPosition(Vec3 position)
	{
		if (MountAgent != null)
		{
			MBAPI.IMBAgent.SetPosition(MountAgent.GetPtr(), ref position);
		}
		MBAPI.IMBAgent.SetPosition(GetPtr(), ref position);
		if (RiderAgent != null)
		{
			MBAPI.IMBAgent.SetPosition(RiderAgent.GetPtr(), ref position);
		}
	}

	public void FadeOut(bool hideInstantly, bool hideMount)
	{
		MBAPI.IMBAgent.FadeOut(GetPtr(), hideInstantly);
		if (hideMount && HasMount)
		{
			MountAgent.FadeOut(hideMount, hideMount: false);
		}
	}

	public void FadeIn()
	{
		MBAPI.IMBAgent.FadeIn(GetPtr());
	}

	public void DisableScriptedMovement()
	{
		MBAPI.IMBAgent.DisableScriptedMovement(GetPtr());
	}

	public void DisableScriptedCombatMovement()
	{
		MBAPI.IMBAgent.DisableScriptedCombatMovement(GetPtr());
	}

	public void ForceAiBehaviorSelection()
	{
		MBAPI.IMBAgent.ForceAiBehaviorSelection(GetPtr());
	}

	public bool HasPathThroughNavigationFaceIdFromDirectionMT(int navigationFaceId, Vec2 direction)
	{
		lock (_pathCheckObjectLock)
		{
			return MBAPI.IMBAgent.HasPathThroughNavigationFaceIdFromDirection(GetPtr(), navigationFaceId, ref direction);
		}
	}

	public bool HasPathThroughNavigationFaceIdFromDirection(int navigationFaceId, Vec2 direction)
	{
		return MBAPI.IMBAgent.HasPathThroughNavigationFaceIdFromDirection(GetPtr(), navigationFaceId, ref direction);
	}

	public void DisableLookToPointOfInterest()
	{
		MBAPI.IMBAgent.DisableLookToPointOfInterest(GetPtr());
	}

	public CompositeComponent AddPrefabComponentToBone(string prefabName, sbyte boneIndex)
	{
		return MBAPI.IMBAgent.AddPrefabToAgentBone(GetPtr(), prefabName, boneIndex);
	}

	public void MakeVoice(SkinVoiceManager.SkinVoiceType voiceType, SkinVoiceManager.CombatVoiceNetworkPredictionType predictionType)
	{
		MBAPI.IMBAgent.MakeVoice(GetPtr(), voiceType.Index, (int)predictionType);
	}

	public void WieldNextWeapon(HandIndex weaponIndex, WeaponWieldActionType wieldActionType = WeaponWieldActionType.WithAnimation)
	{
		MBAPI.IMBAgent.WieldNextWeapon(GetPtr(), (int)weaponIndex, (int)wieldActionType);
	}

	public MovementControlFlag AttackDirectionToMovementFlag(UsageDirection direction)
	{
		return MBAPI.IMBAgent.AttackDirectionToMovementFlag(GetPtr(), direction);
	}

	public MovementControlFlag DefendDirectionToMovementFlag(UsageDirection direction)
	{
		return MBAPI.IMBAgent.DefendDirectionToMovementFlag(GetPtr(), direction);
	}

	public bool KickClear()
	{
		return MBAPI.IMBAgent.KickClear(GetPtr());
	}

	public UsageDirection PlayerAttackDirection()
	{
		return MBAPI.IMBAgent.PlayerAttackDirection(GetPtr());
	}

	public (sbyte, sbyte) GetRandomPairOfRealBloodBurstBoneIndices()
	{
		sbyte item = -1;
		sbyte item2 = -1;
		if (Monster.BloodBurstBoneIndices.Length != 0)
		{
			int num = MBRandom.RandomInt(Monster.BloodBurstBoneIndices.Length / 2);
			item = Monster.BloodBurstBoneIndices[num * 2];
			item2 = Monster.BloodBurstBoneIndices[num * 2 + 1];
		}
		return (item, item2);
	}

	public void CreateBloodBurstAtLimb(sbyte realBoneIndex, float scale)
	{
		MBAPI.IMBAgent.CreateBloodBurstAtLimb(GetPtr(), realBoneIndex, scale);
	}

	public void AddComponent(AgentComponent agentComponent)
	{
		_components.Add(agentComponent);
		if (agentComponent is CommonAIComponent commonAIComponent)
		{
			CommonAIComponent = commonAIComponent;
		}
		else if (agentComponent is HumanAIComponent humanAIComponent)
		{
			HumanAIComponent = humanAIComponent;
		}
	}

	public bool RemoveComponent(AgentComponent agentComponent)
	{
		bool num = _components.Remove(agentComponent);
		if (num)
		{
			agentComponent.OnComponentRemoved();
			if (CommonAIComponent == agentComponent)
			{
				CommonAIComponent = null;
				return num;
			}
			if (HumanAIComponent == agentComponent)
			{
				HumanAIComponent = null;
			}
		}
		return num;
	}

	public void HandleTaunt(int tauntIndex, bool isDefaultTaunt)
	{
		if (tauntIndex < 0)
		{
			return;
		}
		if (isDefaultTaunt)
		{
			ActionIndexCache actionIndexCache = DefaultTauntActions[tauntIndex];
			SetActionChannel(1, actionIndexCache, ignorePriority: false, 0uL);
			MakeVoice(SkinVoiceManager.VoiceType.Victory, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
		}
		else if (!GameNetwork.IsClientOrReplay)
		{
			ActionIndexCache suitableTauntAction = CosmeticsManagerHelper.GetSuitableTauntAction(this, tauntIndex);
			if (suitableTauntAction.Index >= 0)
			{
				SetActionChannel(1, suitableTauntAction, ignorePriority: false, 0uL);
			}
		}
	}

	public void HandleBark(int indexOfBark)
	{
		if (indexOfBark < SkinVoiceManager.VoiceType.MpBarks.Length && !GameNetwork.IsClientOrReplay)
		{
			MakeVoice(SkinVoiceManager.VoiceType.MpBarks[indexOfBark], SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			if (GameNetwork.IsMultiplayer)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new BarkAgent(Index, indexOfBark));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.ExcludeOtherTeamPlayers, MissionPeer.GetNetworkPeer());
			}
		}
	}

	public void HandleDropWeapon(bool isDefendPressed, EquipmentIndex forcedSlotIndexToDropWeaponFrom)
	{
		ActionCodeType currentActionType = GetCurrentActionType(1);
		if (State != AgentState.Active || currentActionType == ActionCodeType.ReleaseMelee || currentActionType == ActionCodeType.ReleaseRanged || currentActionType == ActionCodeType.ReleaseThrowing || currentActionType == ActionCodeType.WeaponBash)
		{
			return;
		}
		EquipmentIndex equipmentIndex = forcedSlotIndexToDropWeaponFrom;
		if (equipmentIndex == EquipmentIndex.None)
		{
			EquipmentIndex wieldedItemIndex = GetWieldedItemIndex(HandIndex.MainHand);
			EquipmentIndex wieldedItemIndex2 = GetWieldedItemIndex(HandIndex.OffHand);
			if (wieldedItemIndex2 >= EquipmentIndex.WeaponItemBeginSlot && isDefendPressed)
			{
				equipmentIndex = wieldedItemIndex2;
			}
			else if (wieldedItemIndex >= EquipmentIndex.WeaponItemBeginSlot)
			{
				equipmentIndex = wieldedItemIndex;
			}
			else if (wieldedItemIndex2 >= EquipmentIndex.WeaponItemBeginSlot)
			{
				equipmentIndex = wieldedItemIndex2;
			}
			else
			{
				for (EquipmentIndex equipmentIndex2 = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex2 < EquipmentIndex.ExtraWeaponSlot; equipmentIndex2++)
				{
					if (Equipment[equipmentIndex2].IsEmpty || !Equipment[equipmentIndex2].Item.PrimaryWeapon.IsConsumable)
					{
						continue;
					}
					if (Equipment[equipmentIndex2].Item.PrimaryWeapon.IsRangedWeapon)
					{
						if (Equipment[equipmentIndex2].Amount == 0)
						{
							equipmentIndex = equipmentIndex2;
							break;
						}
						continue;
					}
					bool flag = false;
					for (EquipmentIndex equipmentIndex3 = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex3 < EquipmentIndex.ExtraWeaponSlot; equipmentIndex3++)
					{
						if (!Equipment[equipmentIndex3].IsEmpty && Equipment[equipmentIndex3].HasAnyUsageWithAmmoClass(Equipment[equipmentIndex2].Item.PrimaryWeapon.WeaponClass) && Equipment[equipmentIndex2].Amount > 0)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						equipmentIndex = equipmentIndex2;
						break;
					}
				}
			}
		}
		if (equipmentIndex != EquipmentIndex.None && !Equipment[equipmentIndex].IsEmpty)
		{
			DropItem(equipmentIndex);
			UpdateAgentProperties();
		}
	}

	public void DropItem(EquipmentIndex itemIndex, WeaponClass pickedUpItemType = WeaponClass.Undefined)
	{
		if (Equipment[itemIndex].CurrentUsageItem.WeaponFlags.HasAllFlags(WeaponFlags.AffectsArea | WeaponFlags.Burning))
		{
			MatrixFrame boneEntitialFrameWithIndex = AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex(Monster.MainHandItemBoneIndex);
			MatrixFrame globalFrame = AgentVisuals.GetGlobalFrame();
			MatrixFrame matrixFrame = globalFrame.TransformToParent(boneEntitialFrameWithIndex);
			Vec3 vec = globalFrame.origin + globalFrame.rotation.f - matrixFrame.origin;
			vec.Normalize();
			Mat3 identity = Mat3.Identity;
			identity.f = vec;
			identity.Orthonormalize();
			Mission.Current.OnAgentShootMissile(this, itemIndex, matrixFrame.origin, vec, identity, hasRigidBody: false, isPrimaryWeaponShot: false, -1);
			RemoveEquippedWeapon(itemIndex);
		}
		else
		{
			MBAPI.IMBAgent.DropItem(GetPtr(), (int)itemIndex, (int)pickedUpItemType);
		}
	}

	public void EquipItemsFromSpawnEquipment(bool neededBatchedItems)
	{
		Mission.OnEquipItemsFromSpawnEquipmentBegin(this, _creationType);
		switch (_creationType)
		{
		case CreationType.FromRoster:
		case CreationType.FromCharacterObj:
		{
			for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
			{
				WeaponData weaponData = WeaponData.InvalidWeaponData;
				WeaponStatsData[] weaponStatsData = null;
				WeaponData ammoWeaponData = WeaponData.InvalidWeaponData;
				WeaponStatsData[] ammoWeaponStatsData = null;
				if (!Equipment[equipmentIndex].IsEmpty)
				{
					weaponData = Equipment[equipmentIndex].GetWeaponData(neededBatchedItems);
					weaponStatsData = Equipment[equipmentIndex].GetWeaponStatsData();
					ammoWeaponData = Equipment[equipmentIndex].GetAmmoWeaponData(neededBatchedItems);
					ammoWeaponStatsData = Equipment[equipmentIndex].GetAmmoWeaponStatsData();
				}
				WeaponEquipped(equipmentIndex, in weaponData, weaponStatsData, in ammoWeaponData, ammoWeaponStatsData, null, removeOldWeaponFromScene: true, isWieldedOnSpawn: true);
				weaponData.DeinitializeManagedPointers();
				ammoWeaponData.DeinitializeManagedPointers();
				for (int i = 0; i < Equipment[equipmentIndex].GetAttachedWeaponsCount(); i++)
				{
					MatrixFrame attachLocalFrame = Equipment[equipmentIndex].GetAttachedWeaponFrame(i);
					MissionWeapon weapon = Equipment[equipmentIndex].GetAttachedWeapon(i);
					AttachWeaponToWeaponAux(equipmentIndex, ref weapon, null, ref attachLocalFrame);
				}
			}
			AddSkinMeshes(!neededBatchedItems);
			break;
		}
		}
		UpdateAgentProperties();
		Mission.OnEquipItemsFromSpawnEquipment(this, _creationType);
		CheckEquipmentForCapeClothSimulationStateChange();
	}

	public void WieldInitialWeapons(WeaponWieldActionType wieldActionType = WeaponWieldActionType.InstantAfterPickUp, Equipment.InitialWeaponEquipPreference initialWeaponEquipPreference = TaleWorlds.Core.Equipment.InitialWeaponEquipPreference.Any)
	{
		EquipmentIndex mainHandWeaponIndex = GetWieldedItemIndex(HandIndex.MainHand);
		EquipmentIndex offHandWeaponIndex = GetWieldedItemIndex(HandIndex.OffHand);
		SpawnEquipment.GetInitialWeaponIndicesToEquip(out mainHandWeaponIndex, out offHandWeaponIndex, out var _, initialWeaponEquipPreference);
		if (offHandWeaponIndex != EquipmentIndex.None)
		{
			TryToWieldWeaponInSlot(offHandWeaponIndex, wieldActionType, isWieldedOnSpawn: true);
		}
		if (mainHandWeaponIndex != EquipmentIndex.None)
		{
			TryToWieldWeaponInSlot(mainHandWeaponIndex, wieldActionType, isWieldedOnSpawn: true);
			if (GetWieldedItemIndex(HandIndex.MainHand) == EquipmentIndex.None)
			{
				WieldNextWeapon(HandIndex.MainHand, wieldActionType);
			}
		}
	}

	public void ChangeWeaponHitPoints(EquipmentIndex slotIndex, short hitPoints)
	{
		Equipment.SetHitPointsOfSlot(slotIndex, hitPoints);
		SetWeaponHitPointsInSlot(slotIndex, hitPoints);
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SetWeaponNetworkData(Index, slotIndex, hitPoints));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		foreach (AgentComponent component in _components)
		{
			component.OnWeaponHPChanged(Equipment[slotIndex].Item, hitPoints);
		}
	}

	public bool HasWeapon()
	{
		for (int i = 0; i < 5; i++)
		{
			WeaponComponentData currentUsageItem = Equipment[i].CurrentUsageItem;
			if (currentUsageItem != null && currentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.WeaponMask))
			{
				return true;
			}
		}
		return false;
	}

	public void AttachWeaponToWeapon(EquipmentIndex slotIndex, MissionWeapon weapon, GameEntity weaponEntity, ref MatrixFrame attachLocalFrame)
	{
		Equipment.AttachWeaponToWeaponInSlot(slotIndex, ref weapon, ref attachLocalFrame);
		AttachWeaponToWeaponAux(slotIndex, ref weapon, weaponEntity, ref attachLocalFrame);
	}

	public void AttachWeaponToBone(MissionWeapon weapon, GameEntity weaponEntity, sbyte boneIndex, ref MatrixFrame attachLocalFrame)
	{
		if (_attachedWeapons == null)
		{
			_attachedWeapons = new List<(MissionWeapon, MatrixFrame, sbyte)>();
		}
		_attachedWeapons.Add((weapon, attachLocalFrame, boneIndex));
		AttachWeaponToBoneAux(ref weapon, weaponEntity, boneIndex, ref attachLocalFrame);
	}

	public void RestoreShieldHitPoints()
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.ExtraWeaponSlot; equipmentIndex++)
		{
			if (!Equipment[equipmentIndex].IsEmpty && Equipment[equipmentIndex].CurrentUsageItem.IsShield)
			{
				ChangeWeaponHitPoints(equipmentIndex, Equipment[equipmentIndex].ModifiedMaxHitPoints);
			}
		}
	}

	public void Die(Blow b, KillInfo overrideKillInfo = KillInfo.Invalid)
	{
		if (Formation != null)
		{
			Formation.Team.QuerySystem.RegisterDeath();
			if (b.IsMissile)
			{
				Formation.Team.QuerySystem.RegisterDeathByRanged();
			}
		}
		Health = 0f;
		if (overrideKillInfo != KillInfo.TeamSwitch && (b.OwnerId == -1 || b.OwnerId == Index) && IsHuman && _lastHitInfo.CanOverrideBlow)
		{
			b.OwnerId = _lastHitInfo.LastBlowOwnerId;
			b.AttackType = _lastHitInfo.LastBlowAttackType;
		}
		MBAPI.IMBAgent.Die(GetPtr(), ref b, (sbyte)overrideKillInfo);
	}

	public void MakeDead(bool isKilled, ActionIndexValueCache actionIndex)
	{
		MBAPI.IMBAgent.MakeDead(GetPtr(), isKilled, actionIndex.Index);
	}

	public void RegisterBlow(Blow blow, in AttackCollisionData collisionData)
	{
		HandleBlow(ref blow, in collisionData);
	}

	public void CreateBlowFromBlowAsReflection(in Blow blow, in AttackCollisionData collisionData, out Blow outBlow, out AttackCollisionData outCollisionData)
	{
		outBlow = blow;
		outBlow.InflictedDamage = blow.SelfInflictedDamage;
		outBlow.GlobalPosition = Position;
		outBlow.BoneIndex = 0;
		outBlow.BlowFlag = BlowFlags.None;
		outCollisionData = collisionData;
		outCollisionData.UpdateCollisionPositionAndBoneForReflect(collisionData.InflictedDamage, Position, 0);
	}

	public void Tick(float dt)
	{
		if (IsActive())
		{
			if (GetCurrentActionStage(1) == ActionStage.AttackQuickReady)
			{
				_lastQuickReadyDetectedTime = Mission.Current.CurrentTime;
			}
			if (_checkIfTargetFrameIsChanged)
			{
				Vec2 vec = ((MovementLockedState != 0) ? GetTargetPosition() : LookFrame.origin.AsVec2);
				Vec3 vec2 = ((MovementLockedState != 0) ? GetTargetDirection() : LookFrame.rotation.f);
				switch (MovementLockedState)
				{
				case AgentMovementLockedState.PositionLocked:
					_checkIfTargetFrameIsChanged = _lastSynchedTargetPosition != vec;
					break;
				case AgentMovementLockedState.FrameLocked:
					_checkIfTargetFrameIsChanged = _lastSynchedTargetPosition != vec || _lastSynchedTargetDirection != vec2;
					break;
				}
				if (_checkIfTargetFrameIsChanged)
				{
					if (MovementLockedState == AgentMovementLockedState.FrameLocked)
					{
						SetTargetPositionAndDirection(MBMath.Lerp(vec, _lastSynchedTargetPosition, 5f * dt, 0.005f), MBMath.Lerp(vec2, _lastSynchedTargetDirection, 5f * dt, 0.005f));
					}
					else
					{
						SetTargetPosition(MBMath.Lerp(vec, _lastSynchedTargetPosition, 5f * dt, 0.005f));
					}
				}
			}
			if (Mission.AllowAiTicking && IsAIControlled)
			{
				TickAsAI(dt);
			}
			if (_wantsToYell)
			{
				if (_yellTimer > 0f)
				{
					_yellTimer -= dt;
				}
				else
				{
					MakeVoice((MountAgent != null) ? SkinVoiceManager.VoiceType.HorseRally : SkinVoiceManager.VoiceType.Yell, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					_wantsToYell = false;
				}
			}
			if (IsPlayerControlled && IsCheering && MovementInputVector != Vec2.Zero)
			{
				SetActionChannel(1, ActionIndexCache.act_none, ignorePriority: false, 0uL);
			}
		}
		else if (MissionPeer?.ControlledAgent == this && !IsCameraAttachable())
		{
			MissionPeer.ControlledAgent = null;
		}
	}

	[Conditional("DEBUG")]
	public void DebugMore()
	{
		MBAPI.IMBAgent.DebugMore(GetPtr());
	}

	public void Mount(Agent mountAgent)
	{
		bool flag = mountAgent.GetCurrentActionType(0) == ActionCodeType.Rear;
		if (MountAgent == null && mountAgent.RiderAgent == null)
		{
			if (CheckSkillForMounting(mountAgent) && !flag && GetCurrentActionValue(0) == ActionIndexValueCache.act_none)
			{
				EventControlFlags |= EventControlFlag.Mount;
				SetInteractionAgent(mountAgent);
			}
		}
		else if (MountAgent == mountAgent && !flag)
		{
			EventControlFlags |= EventControlFlag.Dismount;
		}
	}

	public void EquipWeaponToExtraSlotAndWield(ref MissionWeapon weapon)
	{
		if (!Equipment[EquipmentIndex.ExtraWeaponSlot].IsEmpty)
		{
			DropItem(EquipmentIndex.ExtraWeaponSlot);
		}
		EquipWeaponWithNewEntity(EquipmentIndex.ExtraWeaponSlot, ref weapon);
		TryToWieldWeaponInSlot(EquipmentIndex.ExtraWeaponSlot, WeaponWieldActionType.InstantAfterPickUp, isWieldedOnSpawn: false);
	}

	public void RemoveEquippedWeapon(EquipmentIndex slotIndex)
	{
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new RemoveEquippedWeapon(Index, slotIndex));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		Equipment[slotIndex] = MissionWeapon.Invalid;
		WeaponEquipped(slotIndex, in WeaponData.InvalidWeaponData, null, in WeaponData.InvalidWeaponData, null, null, removeOldWeaponFromScene: true, isWieldedOnSpawn: false);
		UpdateAgentProperties();
	}

	public void EquipWeaponWithNewEntity(EquipmentIndex slotIndex, ref MissionWeapon weapon)
	{
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new EquipWeaponWithNewEntity(Index, slotIndex, weapon));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		Equipment[slotIndex] = weapon;
		WeaponData weaponData = WeaponData.InvalidWeaponData;
		WeaponStatsData[] weaponStatsData = null;
		WeaponData ammoWeaponData = WeaponData.InvalidWeaponData;
		WeaponStatsData[] ammoWeaponStatsData = null;
		if (!weapon.IsEmpty)
		{
			weaponData = weapon.GetWeaponData(needBatchedVersionForMeshes: true);
			weaponStatsData = weapon.GetWeaponStatsData();
			ammoWeaponData = weapon.GetAmmoWeaponData(needBatchedVersion: true);
			ammoWeaponStatsData = weapon.GetAmmoWeaponStatsData();
		}
		WeaponEquipped(slotIndex, in weaponData, weaponStatsData, in ammoWeaponData, ammoWeaponStatsData, null, removeOldWeaponFromScene: true, isWieldedOnSpawn: true);
		weaponData.DeinitializeManagedPointers();
		ammoWeaponData.DeinitializeManagedPointers();
		for (int i = 0; i < weapon.GetAttachedWeaponsCount(); i++)
		{
			MissionWeapon weapon2 = weapon.GetAttachedWeapon(i);
			MatrixFrame attachLocalFrame = weapon.GetAttachedWeaponFrame(i);
			if (GameNetwork.IsServerOrRecorder)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new AttachWeaponToWeaponInAgentEquipmentSlot(weapon2, Index, slotIndex, attachLocalFrame));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
			}
			AttachWeaponToWeaponAux(slotIndex, ref weapon2, null, ref attachLocalFrame);
		}
		UpdateAgentProperties();
	}

	public void EquipWeaponFromSpawnedItemEntity(EquipmentIndex slotIndex, SpawnedItemEntity spawnedItemEntity, bool removeWeapon)
	{
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new EquipWeaponFromSpawnedItemEntity(Index, slotIndex, spawnedItemEntity.Id, removeWeapon));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		if (spawnedItemEntity.GameEntity.Parent != null && spawnedItemEntity.GameEntity.Parent.HasScriptOfType<SpawnedItemEntity>())
		{
			SpawnedItemEntity firstScriptOfType = spawnedItemEntity.GameEntity.Parent.GetFirstScriptOfType<SpawnedItemEntity>();
			int attachmentIndex = -1;
			for (int i = 0; i < firstScriptOfType.GameEntity.ChildCount; i++)
			{
				if (firstScriptOfType.GameEntity.GetChild(i) == spawnedItemEntity.GameEntity)
				{
					attachmentIndex = i;
					break;
				}
			}
			firstScriptOfType.WeaponCopy.RemoveAttachedWeapon(attachmentIndex);
		}
		if (!removeWeapon)
		{
			return;
		}
		if (!Equipment[slotIndex].IsEmpty)
		{
			using (new TWSharedMutexWriteLock(Scene.PhysicsAndRayCastLock))
			{
				spawnedItemEntity.GameEntity.Remove(73);
				return;
			}
		}
		GameEntity gameEntity = spawnedItemEntity.GameEntity;
		using (new TWSharedMutexWriteLock(Scene.PhysicsAndRayCastLock))
		{
			gameEntity.RemovePhysicsMT();
		}
		gameEntity.RemoveScriptComponent(spawnedItemEntity.ScriptComponent.Pointer, 10);
		gameEntity.SetVisibilityExcludeParents(visible: true);
		MissionWeapon weaponCopy = spawnedItemEntity.WeaponCopy;
		Equipment[slotIndex] = weaponCopy;
		WeaponData weaponData = weaponCopy.GetWeaponData(needBatchedVersionForMeshes: true);
		WeaponStatsData[] weaponStatsData = weaponCopy.GetWeaponStatsData();
		WeaponData ammoWeaponData = weaponCopy.GetAmmoWeaponData(needBatchedVersion: true);
		WeaponStatsData[] ammoWeaponStatsData = weaponCopy.GetAmmoWeaponStatsData();
		WeaponEquipped(slotIndex, in weaponData, weaponStatsData, in ammoWeaponData, ammoWeaponStatsData, gameEntity, removeOldWeaponFromScene: true, isWieldedOnSpawn: false);
		weaponData.DeinitializeManagedPointers();
		for (int j = 0; j < weaponCopy.GetAttachedWeaponsCount(); j++)
		{
			MatrixFrame attachLocalFrame = weaponCopy.GetAttachedWeaponFrame(j);
			MissionWeapon weapon = weaponCopy.GetAttachedWeapon(j);
			AttachWeaponToWeaponAux(slotIndex, ref weapon, null, ref attachLocalFrame);
		}
		UpdateAgentProperties();
	}

	public void PreloadForRendering()
	{
		PreloadForRenderingAux();
	}

	public int AddSynchedPrefabComponentToBone(string prefabName, sbyte boneIndex)
	{
		if (_synchedBodyComponents == null)
		{
			_synchedBodyComponents = new List<CompositeComponent>();
		}
		if (!GameEntity.PrefabExists(prefabName))
		{
			MBDebug.ShowWarning("Missing prefab for agent logic :" + prefabName);
			prefabName = "rock_001";
		}
		CompositeComponent item = AddPrefabComponentToBone(prefabName, boneIndex);
		int count = _synchedBodyComponents.Count;
		_synchedBodyComponents.Add(item);
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new AddPrefabComponentToAgentBone(Index, prefabName, boneIndex));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		return count;
	}

	public bool WillDropWieldedShield(SpawnedItemEntity spawnedItem)
	{
		EquipmentIndex wieldedItemIndex = GetWieldedItemIndex(HandIndex.OffHand);
		if (wieldedItemIndex != EquipmentIndex.None && spawnedItem.WeaponCopy.CurrentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.NotUsableWithOneHand) && spawnedItem.WeaponCopy.HasAllUsagesWithAnyWeaponFlag(WeaponFlags.NotUsableWithOneHand))
		{
			bool flag = false;
			for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.ExtraWeaponSlot; equipmentIndex++)
			{
				if (equipmentIndex != wieldedItemIndex && !Equipment[equipmentIndex].IsEmpty && Equipment[equipmentIndex].IsShield())
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public bool HadSameTypeOfConsumableOrShieldOnSpawn(WeaponClass weaponClass)
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.ExtraWeaponSlot; equipmentIndex++)
		{
			if (SpawnEquipment[equipmentIndex].IsEmpty)
			{
				continue;
			}
			foreach (WeaponComponentData weapon in SpawnEquipment[equipmentIndex].Item.Weapons)
			{
				if ((weapon.IsConsumable || weapon.IsShield) && weapon.WeaponClass == weaponClass)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanAIWieldAsRangedWeapon(MissionWeapon weapon)
	{
		ItemObject item = weapon.Item;
		if (IsAIControlled && item != null)
		{
			return !item.ItemFlags.HasAnyFlag(ItemFlags.NotStackable);
		}
		return true;
	}

	public override int GetHashCode()
	{
		return _creationIndex;
	}

	public bool TryGetImmediateEnemyAgentMovementData(out float maximumForwardUnlimitedSpeed, out Vec3 position)
	{
		return MBAPI.IMBAgent.TryGetImmediateEnemyAgentMovementData(GetPtr(), out maximumForwardUnlimitedSpeed, out position);
	}

	public bool HasLostShield()
	{
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.ExtraWeaponSlot; equipmentIndex++)
		{
			if (Equipment[equipmentIndex].IsEmpty && SpawnEquipment[equipmentIndex].Item != null && SpawnEquipment[equipmentIndex].Item.PrimaryWeapon.IsShield)
			{
				return true;
			}
		}
		return false;
	}

	internal void SetMountAgentBeforeBuild(Agent mount)
	{
		MountAgent = mount;
	}

	internal void SetMountInitialValues(TextObject name, string horseCreationKey)
	{
		_name = name;
		HorseCreationKey = horseCreationKey;
	}

	internal void SetInitialAgentScale(float initialScale)
	{
		MBAPI.IMBAgent.SetAgentScale(GetPtr(), initialScale);
	}

	internal void InitializeAgentRecord()
	{
		MBAPI.IMBAgent.InitializeAgentRecord(GetPtr());
	}

	internal void OnDelete()
	{
		_isDeleted = true;
		MissionPeer = null;
	}

	internal void OnFleeing()
	{
		RelieveFromCaptaincy();
		if (Formation != null)
		{
			Formation.Team.DetachmentManager.OnAgentRemoved(this);
			Formation = null;
		}
	}

	internal void OnRemove()
	{
		_isRemoved = true;
		_removalTime = Mission.CurrentTime;
		Origin?.OnAgentRemoved(Health);
		RelieveFromCaptaincy();
		Team?.OnAgentRemoved(this);
		if (Formation != null)
		{
			Formation.Team.DetachmentManager.OnAgentRemoved(this);
			Formation = null;
		}
		if (IsUsingGameObject && !GameNetwork.IsClientOrReplay && Mission != null && !Mission.MissionEnded)
		{
			StopUsingGameObject(isSuccessful: false);
		}
		foreach (AgentComponent component in _components)
		{
			component.OnAgentRemoved();
		}
	}

	internal void InitializeComponents()
	{
		foreach (AgentComponent component in _components)
		{
			component.Initialize();
		}
	}

	internal void Build(AgentBuildData agentBuildData, int creationIndex)
	{
		BuildAux();
		HasBeenBuilt = true;
		Controller = ((!GetAgentFlags().HasAnyFlag(AgentFlag.IsHumanoid)) ? ControllerType.AI : agentBuildData.AgentController);
		Formation = (IsMount ? null : agentBuildData?.AgentFormation);
		MissionGameModels.Current?.AgentStatCalculateModel.InitializeMissionEquipment(this);
		InitializeAgentProperties(SpawnEquipment, agentBuildData);
		_creationIndex = creationIndex;
		if (!GameNetwork.IsServerOrRecorder)
		{
			return;
		}
		foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
		{
			if (!networkPeer.IsMine && networkPeer.IsSynchronized)
			{
				LockAgentReplicationTableDataWithCurrentReliableSequenceNo(networkPeer);
			}
		}
	}

	private void PreloadForRenderingAux()
	{
		MBAPI.IMBAgent.PreloadForRendering(GetPtr());
	}

	internal void Clear()
	{
		Mission = null;
		_pointer = UIntPtr.Zero;
		_positionPointer = UIntPtr.Zero;
		_flagsPointer = UIntPtr.Zero;
		_indexPointer = UIntPtr.Zero;
		_statePointer = UIntPtr.Zero;
	}

	public bool HasPathThroughNavigationFacesIDFromDirection(int navigationFaceID_1, int navigationFaceID_2, int navigationFaceID_3, Vec2 direction)
	{
		return MBAPI.IMBAgent.HasPathThroughNavigationFacesIDFromDirection(GetPtr(), navigationFaceID_1, navigationFaceID_2, navigationFaceID_3, ref direction);
	}

	public bool HasPathThroughNavigationFacesIDFromDirectionMT(int navigationFaceID_1, int navigationFaceID_2, int navigationFaceID_3, Vec2 direction)
	{
		lock (_pathCheckObjectLock)
		{
			return MBAPI.IMBAgent.HasPathThroughNavigationFacesIDFromDirection(GetPtr(), navigationFaceID_1, navigationFaceID_2, navigationFaceID_3, ref direction);
		}
	}

	private void AfterStoppedUsingMissionObject(UsableMachine usableMachine, UsableMissionObject usedObject, UsableMissionObject movingToOrDefendingObject, bool isSuccessful, StopUsingGameObjectFlags flags)
	{
		if (IsAIControlled)
		{
			if (flags.HasAnyFlag(StopUsingGameObjectFlags.AutoAttachAfterStoppingUsingGameObject))
			{
				Formation?.AttachUnit(this);
			}
			if (flags.HasAnyFlag(StopUsingGameObjectFlags.DefendAfterStoppingUsingGameObject))
			{
				UsableMissionObject usedObject2 = usedObject ?? movingToOrDefendingObject;
				this.AIDefendGameObjectEnable(usedObject2, usableMachine);
			}
		}
		if (usedObject is StandingPoint { AutoEquipWeaponsOnUseStopped: not false } && !flags.HasAnyFlag(StopUsingGameObjectFlags.DoNotWieldWeaponAfterStoppingUsingGameObject))
		{
			bool flag = !isSuccessful;
			bool flag2 = _equipmentOnMainHandBeforeUsingObject != EquipmentIndex.None;
			if (_equipmentOnOffHandBeforeUsingObject != EquipmentIndex.None)
			{
				WeaponWieldActionType type = ((!flag || flag2) ? WeaponWieldActionType.Instant : WeaponWieldActionType.WithAnimation);
				TryToWieldWeaponInSlot(_equipmentOnOffHandBeforeUsingObject, type, isWieldedOnSpawn: false);
			}
			if (flag2)
			{
				WeaponWieldActionType type2 = ((!flag) ? WeaponWieldActionType.Instant : WeaponWieldActionType.WithAnimation);
				TryToWieldWeaponInSlot(_equipmentOnMainHandBeforeUsingObject, type2, isWieldedOnSpawn: false);
			}
		}
	}

	private UIntPtr GetPtr()
	{
		return Pointer;
	}

	private void SetWeaponHitPointsInSlot(EquipmentIndex equipmentIndex, short hitPoints)
	{
		MBAPI.IMBAgent.SetWeaponHitPointsInSlot(GetPtr(), (int)equipmentIndex, hitPoints);
	}

	private AgentMovementLockedState GetMovementLockedState()
	{
		return MBAPI.IMBAgent.GetMovementLockedState(GetPtr());
	}

	private void AttachWeaponToBoneAux(ref MissionWeapon weapon, GameEntity weaponEntity, sbyte boneIndex, ref MatrixFrame attachLocalFrame)
	{
		WeaponData weaponData = weapon.GetWeaponData(needBatchedVersionForMeshes: true);
		MBAPI.IMBAgent.AttachWeaponToBone(Pointer, in weaponData, weapon.GetWeaponStatsData(), weapon.WeaponsCount, weaponEntity?.Pointer ?? UIntPtr.Zero, boneIndex, ref attachLocalFrame);
		weaponData.DeinitializeManagedPointers();
	}

	private Agent GetRiderAgentAux()
	{
		return _cachedRiderAgent;
	}

	private void AttachWeaponToWeaponAux(EquipmentIndex slotIndex, ref MissionWeapon weapon, GameEntity weaponEntity, ref MatrixFrame attachLocalFrame)
	{
		WeaponData weaponData = weapon.GetWeaponData(needBatchedVersionForMeshes: true);
		MBAPI.IMBAgent.AttachWeaponToWeaponInSlot(Pointer, in weaponData, weapon.GetWeaponStatsData(), weapon.WeaponsCount, weaponEntity?.Pointer ?? UIntPtr.Zero, (int)slotIndex, ref attachLocalFrame);
		weaponData.DeinitializeManagedPointers();
	}

	private Agent GetMountAgentAux()
	{
		return _cachedMountAgent;
	}

	private void SetMountAgent(Agent mountAgent)
	{
		int mountAgentIndex = mountAgent?.Index ?? (-1);
		MBAPI.IMBAgent.SetMountAgent(GetPtr(), mountAgentIndex);
	}

	private void RelieveFromCaptaincy()
	{
		if (_canLeadFormationsRemotely && Team != null)
		{
			foreach (Formation item in Team.FormationsIncludingSpecialAndEmpty)
			{
				if (item.Captain == this)
				{
					item.Captain = null;
				}
			}
			return;
		}
		if (Formation != null && Formation.Captain == this)
		{
			Formation.Captain = null;
		}
	}

	private void SetTeamInternal(MBTeam team)
	{
		MBAPI.IMBAgent.SetTeam(GetPtr(), team.Index);
	}

	private ControllerType GetController()
	{
		return _agentControllerType;
	}

	private void SetController(ControllerType controllerType)
	{
		if (controllerType != _agentControllerType)
		{
			if (controllerType == ControllerType.Player && IsDetachedFromFormation)
			{
				_detachment.RemoveAgent(this);
				_formation?.AttachUnit(this);
			}
			_agentControllerType = controllerType;
			MBAPI.IMBAgent.SetController(GetPtr(), controllerType);
		}
	}

	private void WeaponEquipped(EquipmentIndex equipmentSlot, in WeaponData weaponData, WeaponStatsData[] weaponStatsData, in WeaponData ammoWeaponData, WeaponStatsData[] ammoWeaponStatsData, GameEntity weaponEntity, bool removeOldWeaponFromScene, bool isWieldedOnSpawn)
	{
		MBAPI.IMBAgent.WeaponEquipped(GetPtr(), (int)equipmentSlot, in weaponData, weaponStatsData, (weaponStatsData != null) ? weaponStatsData.Length : 0, in ammoWeaponData, ammoWeaponStatsData, (ammoWeaponStatsData != null) ? ammoWeaponStatsData.Length : 0, weaponEntity?.Pointer ?? UIntPtr.Zero, removeOldWeaponFromScene, isWieldedOnSpawn);
		CheckEquipmentForCapeClothSimulationStateChange();
	}

	private Agent GetRiderAgent()
	{
		return MBAPI.IMBAgent.GetRiderAgent(GetPtr());
	}

	public void SetInitialFrame(in Vec3 initialPosition, in Vec2 initialDirection, bool canSpawnOutsideOfMissionBoundary = false)
	{
		MBAPI.IMBAgent.SetInitialFrame(GetPtr(), in initialPosition, in initialDirection, canSpawnOutsideOfMissionBoundary);
	}

	private void UpdateDrivenProperties(float[] values)
	{
		MBAPI.IMBAgent.UpdateDrivenProperties(GetPtr(), values);
	}

	private void UpdateLastAttackAndHitTimes(Agent attackerAgent, bool isMissile)
	{
		float currentTime = Mission.CurrentTime;
		if (isMissile)
		{
			LastRangedHitTime = currentTime;
		}
		else
		{
			LastMeleeHitTime = currentTime;
		}
		if (attackerAgent != this && attackerAgent != null)
		{
			if (isMissile)
			{
				attackerAgent.LastRangedAttackTime = currentTime;
			}
			else
			{
				attackerAgent.LastMeleeAttackTime = currentTime;
			}
		}
	}

	private void SetNetworkPeer(NetworkCommunicator newPeer)
	{
		MBAPI.IMBAgent.SetNetworkPeer(GetPtr(), newPeer?.Index ?? (-1));
	}

	private void ClearTargetFrameAux()
	{
		MBAPI.IMBAgent.ClearTargetFrame(GetPtr());
	}

	[Conditional("_RGL_KEEP_ASSERTS")]
	private void CheckUnmanagedAgentValid()
	{
		AgentHelper.GetAgentIndex(_indexPointer);
	}

	private void BuildAux()
	{
		MBAPI.IMBAgent.Build(GetPtr(), Monster.EyeOffsetWrtHead);
	}

	private float GetMissileRangeWithHeightDifference()
	{
		if (IsMount || (!IsRangedCached && !HasThrownCached) || Formation == null || Formation.QuerySystem.ClosestEnemyFormation == null)
		{
			return 0f;
		}
		return GetMissileRangeWithHeightDifferenceAux(Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.GetNavMeshZ());
	}

	private void AddSkinMeshes(bool useGPUMorph)
	{
		bool prepareImmediately = this == Main;
		SkinMask skinMeshesMask = SpawnEquipment.GetSkinMeshesMask();
		bool isFemale = IsFemale && BodyPropertiesValue.Age >= 14f;
		SkinGenerationParams skinParams = new SkinGenerationParams((int)skinMeshesMask, SpawnEquipment.GetUnderwearType(isFemale), (int)SpawnEquipment.BodyMeshType, (int)SpawnEquipment.HairCoverType, (int)SpawnEquipment.BeardCoverType, (int)SpawnEquipment.BodyDeformType, prepareImmediately, Character.FaceDirtAmount, IsFemale ? 1 : 0, Character.Race, useTranslucency: false, useTesselation: false);
		bool useFaceCache = Character != null && Character.FaceMeshCache;
		AgentVisuals.AddSkinMeshes(skinParams, BodyPropertiesValue, useGPUMorph, useFaceCache);
	}

	private void HandleBlow(ref Blow b, in AttackCollisionData collisionData)
	{
		b.BaseMagnitude = TaleWorlds.Library.MathF.Min(b.BaseMagnitude, 1000f);
		b.DamagedPercentage = (float)b.InflictedDamage / HealthLimit;
		Agent agent = ((b.OwnerId != -1) ? Mission.FindAgentWithIndex(b.OwnerId) : null);
		if (!b.BlowFlag.HasAnyFlag(BlowFlags.NoSound))
		{
			bool isCriticalBlow = b.IsBlowCrit(Monster.HitPoints * 4);
			bool isLowBlow = b.IsBlowLow(Monster.HitPoints);
			bool isOwnerHumanoid = agent?.IsHuman ?? true;
			bool isNonTipThrust = b.BlowFlag.HasAnyFlag(BlowFlags.NonTipThrust);
			int hitSound = b.WeaponRecord.GetHitSound(isOwnerHumanoid, isCriticalBlow, isLowBlow, isNonTipThrust, b.AttackType, b.DamageType);
			float soundParameterForArmorType = GetSoundParameterForArmorType(GetProtectorArmorMaterialOfBone(b.BoneIndex));
			SoundEventParameter parameter = new SoundEventParameter("Armor Type", soundParameterForArmorType);
			Mission.MakeSound(hitSound, b.GlobalPosition, soundCanBePredicted: false, isReliable: true, b.OwnerId, Index, ref parameter);
			if (b.IsMissile && agent != null)
			{
				int soundCodeMissionCombatPlayerhit = CombatSoundContainer.SoundCodeMissionCombatPlayerhit;
				Mission.MakeSoundOnlyOnRelatedPeer(soundCodeMissionCombatPlayerhit, b.GlobalPosition, agent.Index);
			}
			Mission.AddSoundAlarmFactorToAgents(b.OwnerId, b.GlobalPosition, 15f);
		}
		if (b.InflictedDamage <= 0)
		{
			return;
		}
		UpdateLastAttackAndHitTimes(agent, b.IsMissile);
		float health = Health;
		float num = (((float)b.InflictedDamage > health) ? health : ((float)b.InflictedDamage));
		float num2 = health - num;
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		if (CurrentMortalityState != MortalityState.Immortal && !Mission.DisableDying)
		{
			Health = num2;
		}
		if (agent != null && agent != this && IsHuman)
		{
			if (agent.IsMount && agent.RiderAgent != null)
			{
				_lastHitInfo.RegisterLastBlow(agent.RiderAgent.Index, b.AttackType);
			}
			else if (agent.IsHuman)
			{
				_lastHitInfo.RegisterLastBlow(b.OwnerId, b.AttackType);
			}
		}
		Mission.OnAgentHit(this, agent, in b, in collisionData, isBlocked: false, num);
		if (Health < 1f)
		{
			KillInfo overrideKillInfo = (b.IsFallDamage ? KillInfo.Gravity : KillInfo.Invalid);
			Die(b, overrideKillInfo);
		}
		HandleBlowAux(ref b);
	}

	private void HandleBlowAux(ref Blow b)
	{
		MBAPI.IMBAgent.HandleBlowAux(GetPtr(), ref b);
	}

	private ArmorComponent.ArmorMaterialTypes GetProtectorArmorMaterialOfBone(sbyte boneIndex)
	{
		if (boneIndex >= 0)
		{
			EquipmentIndex equipmentIndex = EquipmentIndex.None;
			switch (AgentVisuals.GetBoneTypeData(boneIndex).BodyPartType)
			{
			case BoneBodyPartType.Chest:
			case BoneBodyPartType.Abdomen:
			case BoneBodyPartType.ShoulderLeft:
			case BoneBodyPartType.ShoulderRight:
				equipmentIndex = EquipmentIndex.Body;
				break;
			case BoneBodyPartType.ArmLeft:
			case BoneBodyPartType.ArmRight:
				equipmentIndex = EquipmentIndex.Gloves;
				break;
			case BoneBodyPartType.Legs:
				equipmentIndex = EquipmentIndex.Leg;
				break;
			case BoneBodyPartType.Head:
			case BoneBodyPartType.Neck:
				equipmentIndex = EquipmentIndex.NumAllWeaponSlots;
				break;
			}
			if (equipmentIndex != EquipmentIndex.None && SpawnEquipment[equipmentIndex].Item != null)
			{
				return SpawnEquipment[equipmentIndex].Item.ArmorComponent.MaterialType;
			}
		}
		return ArmorComponent.ArmorMaterialTypes.None;
	}

	private void TickAsAI(float dt)
	{
		for (int i = 0; i < _components.Count; i++)
		{
			_components[i].OnTickAsAI(dt);
		}
		if (Formation != null && _cachedAndFormationValuesUpdateTimer.Check(Mission.CurrentTime))
		{
			UpdateCachedAndFormationValues(updateOnlyMovement: false, arrangementChangeAllowed: true);
		}
	}

	private void SyncHealthToClients()
	{
		if (SyncHealthToAllClients && (!IsMount || RiderAgent != null))
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SetAgentHealth(Index, (int)Health));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
			return;
		}
		NetworkCommunicator networkCommunicator = ((!IsMount) ? MissionPeer?.GetNetworkPeer() : RiderAgent?.MissionPeer?.GetNetworkPeer());
		if (networkCommunicator != null && !networkCommunicator.IsServerPeer)
		{
			GameNetwork.BeginModuleEventAsServer(networkCommunicator);
			GameNetwork.WriteMessage(new SetAgentHealth(Index, (int)Health));
			GameNetwork.EndModuleEventAsServer();
		}
	}

	public static UsageDirection MovementFlagToDirection(MovementControlFlag flag)
	{
		if (flag.HasAnyFlag(MovementControlFlag.AttackDown))
		{
			return UsageDirection.AttackDown;
		}
		if (flag.HasAnyFlag(MovementControlFlag.AttackUp))
		{
			return UsageDirection.AttackUp;
		}
		if (flag.HasAnyFlag(MovementControlFlag.AttackLeft))
		{
			return UsageDirection.AttackLeft;
		}
		if (flag.HasAnyFlag(MovementControlFlag.AttackRight))
		{
			return UsageDirection.AttackRight;
		}
		if (flag.HasAnyFlag(MovementControlFlag.DefendDown))
		{
			return UsageDirection.DefendDown;
		}
		if (flag.HasAnyFlag(MovementControlFlag.DefendUp))
		{
			return UsageDirection.AttackEnd;
		}
		if (flag.HasAnyFlag(MovementControlFlag.DefendLeft))
		{
			return UsageDirection.DefendLeft;
		}
		if (flag.HasAnyFlag(MovementControlFlag.DefendRight))
		{
			return UsageDirection.DefendRight;
		}
		return UsageDirection.None;
	}

	public static UsageDirection GetActionDirection(int actionIndex)
	{
		return MBAPI.IMBAgent.GetActionDirection(actionIndex);
	}

	public static int GetMonsterUsageIndex(string monsterUsage)
	{
		return MBAPI.IMBAgent.GetMonsterUsageIndex(monsterUsage);
	}

	private static float GetSoundParameterForArmorType(ArmorComponent.ArmorMaterialTypes armorMaterialType)
	{
		return (float)armorMaterialType * 0.1f;
	}
}
