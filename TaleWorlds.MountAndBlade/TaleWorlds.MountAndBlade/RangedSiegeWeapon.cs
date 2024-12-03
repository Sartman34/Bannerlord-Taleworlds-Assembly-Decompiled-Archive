using System;
using System.Collections.Generic;
using System.Linq;
using NetworkMessages.FromClient;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.Objects.Usables;

namespace TaleWorlds.MountAndBlade;

public abstract class RangedSiegeWeapon : SiegeWeapon
{
	[DefineSynchedMissionObjectType(typeof(RangedSiegeWeapon))]
	public struct RangedSiegeWeaponRecord : ISynchedMissionObjectReadableRecord
	{
		public int State { get; private set; }

		public float TargetDirection { get; private set; }

		public float TargetReleaseAngle { get; private set; }

		public int AmmoCount { get; private set; }

		public int ProjectileIndex { get; private set; }

		public bool ReadFromNetwork(ref bool bufferReadValid)
		{
			State = GameNetworkMessage.ReadIntFromPacket(CompressionMission.RangedSiegeWeaponStateCompressionInfo, ref bufferReadValid);
			TargetDirection = GameNetworkMessage.ReadFloatFromPacket(CompressionBasic.RadianCompressionInfo, ref bufferReadValid);
			TargetReleaseAngle = GameNetworkMessage.ReadFloatFromPacket(CompressionBasic.RadianCompressionInfo, ref bufferReadValid);
			AmmoCount = GameNetworkMessage.ReadIntFromPacket(CompressionMission.RangedSiegeWeaponAmmoCompressionInfo, ref bufferReadValid);
			ProjectileIndex = GameNetworkMessage.ReadIntFromPacket(CompressionMission.RangedSiegeWeaponAmmoIndexCompressionInfo, ref bufferReadValid);
			return bufferReadValid;
		}
	}

	public enum WeaponState
	{
		Invalid = -1,
		Idle,
		WaitingBeforeProjectileLeaving,
		Shooting,
		WaitingAfterShooting,
		WaitingBeforeReloading,
		LoadingAmmo,
		WaitingBeforeIdle,
		Reloading,
		ReloadingPaused,
		NumberOfStates
	}

	public enum FiringFocus
	{
		Troops,
		Walls,
		RangedSiegeWeapons,
		PrimarySiegeWeapons
	}

	public delegate void OnSiegeWeaponReloadDone();

	public enum CameraState
	{
		StickToWeapon,
		DontMove,
		MoveDownToReload,
		RememberLastShotDirection,
		FreeMove,
		ApproachToCamera
	}

	public const float DefaultDirectionRestriction = System.MathF.PI * 2f / 3f;

	public const string MultipleProjectileId = "grapeshot_fire_stack";

	public const string MultipleProjectileFlyingId = "grapeshot_fire_projectile";

	public const int MultipleProjectileCount = 5;

	public const string CanGoAmmoPickupTag = "can_pick_up_ammo";

	public const string DontApplySidePenaltyTag = "no_ammo_pick_up_penalty";

	public const string ReloadTag = "reload";

	public const string AmmoLoadTag = "ammoload";

	public const string CameraHolderTag = "cameraHolder";

	public const string ProjectileTag = "projectile";

	public string MissileItemID;

	protected bool UsesMouseForAiming;

	private WeaponState _state;

	public FiringFocus Focus;

	private int _projectileIndex;

	protected GameEntity MissileStartingPositionEntityForSimulation;

	protected Skeleton[] Skeletons;

	protected SynchedMissionObject[] SkeletonOwnerObjects;

	protected string[] SkeletonNames;

	protected string[] FireAnimations;

	protected string[] SetUpAnimations;

	protected int[] FireAnimationIndices;

	protected int[] SetUpAnimationIndices;

	protected SynchedMissionObject RotationObject;

	private MatrixFrame _rotationObjectInitialFrame;

	protected SoundEvent MoveSound;

	protected SoundEvent ReloadSound;

	protected int MoveSoundIndex = -1;

	protected int ReloadSoundIndex = -1;

	protected int ReloadEndSoundIndex = -1;

	protected ItemObject OriginalMissileItem;

	private WeaponStatsData _originalMissileWeaponStatsDataForTargeting;

	protected ItemObject LoadedMissileItem;

	protected List<StandingPoint> CanPickUpAmmoStandingPoints;

	protected List<StandingPoint> ReloadStandingPoints;

	protected List<StandingPointWithWeaponRequirement> AmmoPickUpStandingPoints;

	protected StandingPointWithWeaponRequirement LoadAmmoStandingPoint;

	protected Dictionary<StandingPoint, float> PilotReservePriorityValues = new Dictionary<StandingPoint, float>();

	protected Agent ReloaderAgent;

	protected StandingPoint ReloaderAgentOriginalPoint;

	protected bool AttackClickWillReload;

	protected bool WeaponNeedsClickToReload;

	public int startingAmmoCount = 20;

	protected int CurrentAmmo = 1;

	private bool _hasAmmo = true;

	protected float targetDirection;

	protected float targetReleaseAngle;

	protected float cameraDirection;

	protected float cameraReleaseAngle;

	protected float reloadTargetReleaseAngle;

	protected float maxRotateSpeed;

	protected float dontMoveTimer;

	private MatrixFrame cameraHolderInitialFrame;

	private CameraState cameraState;

	private bool _inputGiven;

	private float _inputX;

	private float _inputY;

	private bool _exactInputGiven;

	private float _inputTargetX;

	private float _inputTargetY;

	private Vec3 _ammoPickupCenter;

	protected float currentDirection;

	private Vec3 _originalDirection;

	protected float currentReleaseAngle;

	private float _lastSyncedDirection;

	private float _lastSyncedReleaseAngle;

	private float _syncTimer;

	public float TopReleaseAngleRestriction = System.MathF.PI / 2f;

	public float BottomReleaseAngleRestriction = -System.MathF.PI / 2f;

	protected float ReleaseAngleRestrictionCenter;

	protected float ReleaseAngleRestrictionAngle;

	private float animationTimeElapsed;

	protected float timeGapBetweenShootingEndAndReloadingStart = 0.6f;

	protected float timeGapBetweenShootActionAndProjectileLeaving;

	private int _currentReloaderCount;

	private Agent _lastShooterAgent;

	private float _lastCanPickUpAmmoStandingPointsSortedAngle = -System.MathF.PI;

	protected BattleSideEnum _defaultSide;

	private bool hasFrameChangedInPreviousFrame;

	protected SiegeMachineStonePile _stonePile;

	private bool _aiRequestsShoot;

	private bool _aiRequestsManualReload;

	public WeaponState State
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
					GameNetwork.WriteMessage(new SetRangedSiegeWeaponState(base.Id, value));
					GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
				}
				_state = value;
				OnRangedSiegeWeaponStateChange();
			}
		}
	}

	protected virtual float MaximumBallisticError => 1f;

	protected abstract float ShootingSpeed { get; }

	public virtual Vec3 CanShootAtPointCheckingOffset => Vec3.Zero;

	public GameEntity cameraHolder { get; private set; }

	protected SynchedMissionObject Projectile { get; private set; }

	protected Vec3 MissleStartingPositionForSimulation
	{
		get
		{
			if (MissileStartingPositionEntityForSimulation != null)
			{
				return MissileStartingPositionEntityForSimulation.GlobalPosition;
			}
			return Projectile?.GameEntity.GlobalPosition ?? Vec3.Zero;
		}
	}

	protected string SkeletonName
	{
		set
		{
			SkeletonNames = new string[1] { value };
		}
	}

	protected string FireAnimation
	{
		set
		{
			FireAnimations = new string[1] { value };
		}
	}

	protected string SetUpAnimation
	{
		set
		{
			SetUpAnimations = new string[1] { value };
		}
	}

	protected int FireAnimationIndex
	{
		set
		{
			FireAnimationIndices = new int[1] { value };
		}
	}

	protected int SetUpAnimationIndex
	{
		set
		{
			SetUpAnimationIndices = new int[1] { value };
		}
	}

	public int AmmoCount
	{
		get
		{
			return CurrentAmmo;
		}
		protected set
		{
			CurrentAmmo = value;
		}
	}

	protected virtual bool HasAmmo
	{
		get
		{
			return _hasAmmo;
		}
		set
		{
			_hasAmmo = value;
		}
	}

	public virtual float DirectionRestriction => System.MathF.PI * 2f / 3f;

	public Vec3 OriginalDirection => _originalDirection;

	protected virtual float HorizontalAimSensitivity => 0.2f;

	protected virtual float VerticalAimSensitivity => 0.2f;

	protected virtual Vec3 ShootingDirection => Projectile.GameEntity.GetGlobalFrame().rotation.u;

	public virtual Vec3 ProjectileEntityCurrentGlobalPosition => Projectile.GameEntity.GetGlobalFrame().origin;

	public override BattleSideEnum Side
	{
		get
		{
			if (base.PilotAgent != null)
			{
				return base.PilotAgent.Team.Side;
			}
			return _defaultSide;
		}
	}

	public event OnSiegeWeaponReloadDone OnReloadDone;

	public event Action<RangedSiegeWeapon, Agent> OnAgentLoadsMachine;

	protected virtual void ConsumeAmmo()
	{
		AmmoCount--;
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SetRangedSiegeWeaponAmmo(base.Id, AmmoCount));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		UpdateAmmoMesh();
		CheckAmmo();
	}

	public virtual void SetAmmo(int ammoLeft)
	{
		if (AmmoCount != ammoLeft)
		{
			AmmoCount = ammoLeft;
			UpdateAmmoMesh();
			CheckAmmo();
		}
	}

	protected virtual void CheckAmmo()
	{
		if (AmmoCount > 0)
		{
			return;
		}
		HasAmmo = false;
		SetForcedUse(value: false);
		foreach (StandingPointWithWeaponRequirement ammoPickUpStandingPoint in AmmoPickUpStandingPoints)
		{
			ammoPickUpStandingPoint.IsDeactivated = true;
		}
	}

	protected abstract void RegisterAnimationParameters();

	protected abstract void GetSoundEventIndices();

	protected void ChangeProjectileEntityServer(Agent loadingAgent, string missileItemID)
	{
		List<SynchedMissionObject> list = base.GameEntity.CollectObjectsWithTag<SynchedMissionObject>("projectile");
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].GameEntity.HasTag(missileItemID))
			{
				Projectile = list[i];
				_projectileIndex = i;
				break;
			}
		}
		LoadedMissileItem = Game.Current.ObjectManager.GetObject<ItemObject>(missileItemID);
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new RangedSiegeWeaponChangeProjectile(base.Id, _projectileIndex));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		this.OnAgentLoadsMachine?.Invoke(this, loadingAgent);
	}

	public void ChangeProjectileEntityClient(int index)
	{
		List<SynchedMissionObject> list = base.GameEntity.CollectObjectsWithTag<SynchedMissionObject>("projectile");
		Projectile = list[index];
		_projectileIndex = index;
	}

	protected internal override void OnInit()
	{
		base.OnInit();
		DestructableComponent destructableComponent = base.GameEntity.GetScriptComponents<DestructableComponent>().FirstOrDefault();
		if (destructableComponent != null)
		{
			_defaultSide = destructableComponent.BattleSide;
		}
		else
		{
			Debug.FailedAssert("Ranged siege weapons must have destructible component.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Objects\\Siege\\RangedSiegeWeapon.cs", "OnInit", 413);
		}
		ReleaseAngleRestrictionCenter = (TopReleaseAngleRestriction + BottomReleaseAngleRestriction) * 0.5f;
		ReleaseAngleRestrictionAngle = TopReleaseAngleRestriction - BottomReleaseAngleRestriction;
		currentReleaseAngle = (_lastSyncedReleaseAngle = ReleaseAngleRestrictionCenter);
		OriginalMissileItem = Game.Current.ObjectManager.GetObject<ItemObject>(MissileItemID);
		LoadedMissileItem = OriginalMissileItem;
		_originalMissileWeaponStatsDataForTargeting = new MissionWeapon(OriginalMissileItem, null, null).GetWeaponStatsDataForUsage(0);
		if (RotationObject == null)
		{
			RotationObject = this;
		}
		_rotationObjectInitialFrame = RotationObject.GameEntity.GetFrame();
		_originalDirection = RotationObject.GameEntity.GetGlobalFrame().rotation.f;
		_originalDirection.RotateAboutZ(System.MathF.PI);
		currentDirection = (_lastSyncedDirection = 0f);
		_syncTimer = 0f;
		List<GameEntity> list = base.GameEntity.CollectChildrenEntitiesWithTag("cameraHolder");
		if (list.Count > 0)
		{
			cameraHolder = list[0];
			cameraHolderInitialFrame = cameraHolder.GetFrame();
			if (GameNetwork.IsClientOrReplay)
			{
				MakeVisibilityCheck = false;
			}
		}
		List<SynchedMissionObject> list2 = base.GameEntity.CollectObjectsWithTag<SynchedMissionObject>("projectile");
		foreach (SynchedMissionObject item in list2)
		{
			item.GameEntity.SetVisibilityExcludeParents(visible: false);
		}
		Projectile = list2.FirstOrDefault((SynchedMissionObject x) => x.GameEntity.HasTag(MissileItemID));
		Projectile.GameEntity.SetVisibilityExcludeParents(visible: true);
		MissileStartingPositionEntityForSimulation = base.GameEntity.GetChildren().FirstOrDefault((GameEntity x) => x.Name == "clean")?.GetChildren().FirstOrDefault((GameEntity x) => x.Name == "projectile_leaving_position");
		targetDirection = currentDirection;
		targetReleaseAngle = currentReleaseAngle;
		CanPickUpAmmoStandingPoints = new List<StandingPoint>();
		ReloadStandingPoints = new List<StandingPoint>();
		AmmoPickUpStandingPoints = new List<StandingPointWithWeaponRequirement>();
		if (base.StandingPoints != null)
		{
			foreach (StandingPoint standingPoint in base.StandingPoints)
			{
				standingPoint.AddComponent(new ResetAnimationOnStopUsageComponent(ActionIndexCache.act_none));
				if (standingPoint.GameEntity.HasTag("reload"))
				{
					ReloadStandingPoints.Add(standingPoint);
				}
				if (standingPoint.GameEntity.HasTag("can_pick_up_ammo"))
				{
					CanPickUpAmmoStandingPoints.Add(standingPoint);
				}
			}
		}
		List<StandingPointWithWeaponRequirement> list3 = base.StandingPoints.OfType<StandingPointWithWeaponRequirement>().ToList();
		List<StandingPointWithWeaponRequirement> list4 = new List<StandingPointWithWeaponRequirement>();
		foreach (StandingPointWithWeaponRequirement item2 in list3)
		{
			if (item2.GameEntity.HasTag(AmmoPickUpTag))
			{
				AmmoPickUpStandingPoints.Add(item2);
				item2.InitGivenWeapon(OriginalMissileItem);
				item2.SetupOnUsingStoppedBehavior(autoAttach: false, OnAmmoPickupUsingCancelled);
			}
			else
			{
				list4.Add(item2);
				item2.SetupOnUsingStoppedBehavior(autoAttach: false, OnLoadingAmmoPointUsingCancelled);
				item2.InitRequiredWeaponClasses(OriginalMissileItem.PrimaryWeapon.WeaponClass);
			}
		}
		if (AmmoPickUpStandingPoints.Count > 1)
		{
			_stonePile = AmmoPickUpStandingPoints[0].GameEntity.Parent.GetFirstScriptOfType<SiegeMachineStonePile>();
			_ammoPickupCenter = default(Vec3);
			foreach (StandingPointWithWeaponRequirement ammoPickUpStandingPoint in AmmoPickUpStandingPoints)
			{
				ammoPickUpStandingPoint.SetHasAlternative(hasAlternative: true);
				_ammoPickupCenter += ammoPickUpStandingPoint.GameEntity.GlobalPosition;
			}
			_ammoPickupCenter /= (float)AmmoPickUpStandingPoints.Count;
		}
		else
		{
			_ammoPickupCenter = base.GameEntity.GlobalPosition;
		}
		list4.Sort(delegate(StandingPointWithWeaponRequirement element1, StandingPointWithWeaponRequirement element2)
		{
			if (element1.GameEntity.GlobalPosition.DistanceSquared(_ammoPickupCenter) > element2.GameEntity.GlobalPosition.DistanceSquared(_ammoPickupCenter))
			{
				return 1;
			}
			return (element1.GameEntity.GlobalPosition.DistanceSquared(_ammoPickupCenter) < element2.GameEntity.GlobalPosition.DistanceSquared(_ammoPickupCenter)) ? (-1) : 0;
		});
		LoadAmmoStandingPoint = list4.FirstOrDefault();
		SortCanPickUpAmmoStandingPoints();
		Vec3 vec = base.PilotStandingPoint.GameEntity.GlobalPosition - base.GameEntity.GlobalPosition;
		foreach (StandingPoint canPickUpAmmoStandingPoint in CanPickUpAmmoStandingPoints)
		{
			if (canPickUpAmmoStandingPoint != base.PilotStandingPoint)
			{
				float length = (canPickUpAmmoStandingPoint.GameEntity.GlobalPosition - base.GameEntity.GlobalPosition + vec).Length;
				PilotReservePriorityValues.Add(canPickUpAmmoStandingPoint, length);
			}
		}
		AmmoCount = startingAmmoCount;
		UpdateAmmoMesh();
		RegisterAnimationParameters();
		GetSoundEventIndices();
		InitAnimations();
		SetScriptComponentToTick(GetTickRequirement());
	}

	private void SortCanPickUpAmmoStandingPoints()
	{
		if (!(MBMath.GetSmallestDifferenceBetweenTwoAngles(_lastCanPickUpAmmoStandingPointsSortedAngle, currentDirection) > System.MathF.PI * 3f / 50f))
		{
			return;
		}
		_lastCanPickUpAmmoStandingPointsSortedAngle = currentDirection;
		int signOfAmmoPile = Math.Sign(Vec3.DotProduct(base.GameEntity.GetGlobalFrame().rotation.s, _ammoPickupCenter - base.GameEntity.GlobalPosition));
		CanPickUpAmmoStandingPoints.Sort(delegate(StandingPoint element1, StandingPoint element2)
		{
			Vec3 vec = _ammoPickupCenter - element1.GameEntity.GlobalPosition;
			Vec3 vec2 = _ammoPickupCenter - element2.GameEntity.GlobalPosition;
			float num = vec.LengthSquared;
			float num2 = vec2.LengthSquared;
			float num3 = Vec3.DotProduct(base.GameEntity.GetGlobalFrame().rotation.s, element1.GameEntity.GlobalPosition - base.GameEntity.GlobalPosition);
			float num4 = Vec3.DotProduct(base.GameEntity.GetGlobalFrame().rotation.s, element2.GameEntity.GlobalPosition - base.GameEntity.GlobalPosition);
			if (!element1.GameEntity.HasTag("no_ammo_pick_up_penalty") && signOfAmmoPile != Math.Sign(num3))
			{
				num += num3 * num3 * 64f;
			}
			if (!element2.GameEntity.HasTag("no_ammo_pick_up_penalty") && signOfAmmoPile != Math.Sign(num4))
			{
				num2 += num4 * num4 * 64f;
			}
			if (element1.GameEntity.HasTag(PilotStandingPointTag))
			{
				num += 25f;
			}
			else if (element2.GameEntity.HasTag(PilotStandingPointTag))
			{
				num2 += 25f;
			}
			if (num > num2)
			{
				return 1;
			}
			return (num < num2) ? (-1) : 0;
		});
	}

	protected internal override void OnEditorInit()
	{
		List<SynchedMissionObject> list = base.GameEntity.CollectObjectsWithTag<SynchedMissionObject>("projectile");
		if (list.Count > 0)
		{
			Projectile = list[0];
		}
	}

	private void InitAnimations()
	{
		for (int i = 0; i < Skeletons.Length; i++)
		{
			Skeletons[i].SetAnimationAtChannel(SetUpAnimations[i], 0, 1f, 0f);
			Skeletons[i].SetAnimationParameterAtChannel(0, 1f);
			Skeletons[i].TickAnimations(0.0001f, MatrixFrame.Identity, tickAnimsForChildren: true);
		}
	}

	protected internal override void OnMissionReset()
	{
		base.OnMissionReset();
		Projectile.GameEntity.SetVisibilityExcludeParents(visible: true);
		foreach (StandingPoint standingPoint in base.StandingPoints)
		{
			standingPoint.UserAgent?.StopUsingGameObject();
			standingPoint.IsDeactivated = false;
		}
		_state = WeaponState.Idle;
		currentDirection = (_lastSyncedDirection = 0f);
		_syncTimer = 0f;
		currentReleaseAngle = (_lastSyncedReleaseAngle = ReleaseAngleRestrictionCenter);
		targetDirection = currentDirection;
		targetReleaseAngle = currentReleaseAngle;
		ApplyCurrentDirectionToEntity();
		AmmoCount = startingAmmoCount;
		UpdateAmmoMesh();
		if (MoveSound != null)
		{
			MoveSound.Stop();
			MoveSound = null;
		}
		hasFrameChangedInPreviousFrame = false;
		Skeleton[] skeletons = Skeletons;
		for (int i = 0; i < skeletons.Length; i++)
		{
			skeletons[i].Freeze(p: false);
		}
		foreach (StandingPointWithWeaponRequirement ammoPickUpStandingPoint in AmmoPickUpStandingPoints)
		{
			ammoPickUpStandingPoint.IsDeactivated = false;
		}
		InitAnimations();
		UpdateProjectilePosition();
		if (!GameNetwork.IsClientOrReplay)
		{
			SetActivationLoadAmmoPoint(activate: false);
		}
	}

	public override void WriteToNetwork()
	{
		base.WriteToNetwork();
		GameNetworkMessage.WriteIntToPacket((int)State, CompressionMission.RangedSiegeWeaponStateCompressionInfo);
		GameNetworkMessage.WriteFloatToPacket(targetDirection, CompressionBasic.RadianCompressionInfo);
		GameNetworkMessage.WriteFloatToPacket(targetReleaseAngle, CompressionBasic.RadianCompressionInfo);
		GameNetworkMessage.WriteIntToPacket(AmmoCount, CompressionMission.RangedSiegeWeaponAmmoCompressionInfo);
		GameNetworkMessage.WriteIntToPacket(_projectileIndex, CompressionMission.RangedSiegeWeaponAmmoIndexCompressionInfo);
	}

	protected virtual void UpdateProjectilePosition()
	{
	}

	public override bool IsInRangeToCheckAlternativePoints(Agent agent)
	{
		float num = ((AmmoPickUpStandingPoints.Count > 0) ? (agent.GetInteractionDistanceToUsable(AmmoPickUpStandingPoints[0]) + 2f) : 2f);
		return _ammoPickupCenter.DistanceSquared(agent.Position) < num * num;
	}

	public override StandingPoint GetBestPointAlternativeTo(StandingPoint standingPoint, Agent agent)
	{
		if (AmmoPickUpStandingPoints.Contains(standingPoint))
		{
			IEnumerable<StandingPointWithWeaponRequirement> enumerable = AmmoPickUpStandingPoints.Where((StandingPointWithWeaponRequirement sp) => !sp.IsDeactivated && (sp.IsInstantUse || (!sp.HasUser && !sp.HasAIMovingTo)) && !sp.IsDisabledForAgent(agent));
			float num = standingPoint.GameEntity.GlobalPosition.DistanceSquared(agent.Position);
			StandingPoint result = standingPoint;
			{
				foreach (StandingPointWithWeaponRequirement item in enumerable)
				{
					float num2 = item.GameEntity.GlobalPosition.DistanceSquared(agent.Position);
					if (num2 < num)
					{
						num = num2;
						result = item;
					}
				}
				return result;
			}
		}
		return standingPoint;
	}

	protected virtual void OnRangedSiegeWeaponStateChange()
	{
		switch (State)
		{
		case WeaponState.Reloading:
			if (ReloadSound != null && ReloadSound.IsValid)
			{
				if (ReloadSound.IsPaused())
				{
					ReloadSound.Resume();
				}
				else
				{
					ReloadSound.PlayInPosition(base.GameEntity.GetGlobalFrame().origin);
				}
			}
			else
			{
				ReloadSound = SoundEvent.CreateEvent(ReloadSoundIndex, base.Scene);
				ReloadSound.PlayInPosition(base.GameEntity.GetGlobalFrame().origin);
			}
			break;
		case WeaponState.ReloadingPaused:
			if (ReloadSound != null && ReloadSound.IsValid)
			{
				ReloadSound.Pause();
			}
			break;
		case WeaponState.WaitingBeforeProjectileLeaving:
			AttackClickWillReload = WeaponNeedsClickToReload;
			break;
		case WeaponState.Shooting:
			if (cameraHolder != null)
			{
				cameraState = CameraState.DontMove;
				dontMoveTimer = 0.35f;
			}
			break;
		case WeaponState.LoadingAmmo:
			if (ReloadSound != null && ReloadSound.IsValid)
			{
				ReloadSound.Stop();
			}
			ReloadSound = null;
			Mission.Current.MakeSound(ReloadEndSoundIndex, base.GameEntity.GetGlobalFrame().origin, soundCanBePredicted: true, isReliable: false, -1, -1);
			break;
		case WeaponState.WaitingAfterShooting:
			AttackClickWillReload = WeaponNeedsClickToReload;
			CheckAmmo();
			break;
		case WeaponState.WaitingBeforeReloading:
			AttackClickWillReload = false;
			if (cameraHolder != null)
			{
				cameraState = CameraState.MoveDownToReload;
			}
			CheckAmmo();
			break;
		case WeaponState.Idle:
		case WeaponState.WaitingBeforeIdle:
			if (cameraState == CameraState.FreeMove)
			{
				cameraState = CameraState.ApproachToCamera;
			}
			else
			{
				cameraState = CameraState.StickToWeapon;
			}
			break;
		default:
			Debug.FailedAssert("Invalid WeaponState.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Objects\\Siege\\RangedSiegeWeapon.cs", "OnRangedSiegeWeaponStateChange", 854);
			break;
		}
		if (GameNetwork.IsClientOrReplay)
		{
			return;
		}
		switch (State)
		{
		case WeaponState.Reloading:
		{
			for (int j = 0; j < SkeletonOwnerObjects.Length; j++)
			{
				if (SkeletonOwnerObjects[j].GameEntity.IsSkeletonAnimationPaused())
				{
					SkeletonOwnerObjects[j].ResumeSkeletonAnimationSynched();
				}
				else
				{
					SkeletonOwnerObjects[j].SetAnimationAtChannelSynched(SetUpAnimations[j], 0);
				}
			}
			_currentReloaderCount = 1;
			break;
		}
		case WeaponState.ReloadingPaused:
		{
			SynchedMissionObject[] skeletonOwnerObjects = SkeletonOwnerObjects;
			for (int k = 0; k < skeletonOwnerObjects.Length; k++)
			{
				skeletonOwnerObjects[k].PauseSkeletonAnimationSynched();
			}
			break;
		}
		case WeaponState.WaitingBeforeProjectileLeaving:
		{
			for (int i = 0; i < SkeletonOwnerObjects.Length; i++)
			{
				SkeletonOwnerObjects[i].SetAnimationAtChannelSynched(FireAnimations[i], 0);
			}
			break;
		}
		case WeaponState.Shooting:
			ShootProjectile();
			break;
		case WeaponState.LoadingAmmo:
			SetActivationLoadAmmoPoint(activate: true);
			ReloaderAgent = null;
			break;
		case WeaponState.WaitingBeforeIdle:
			SendReloaderAgentToOriginalPoint();
			SetActivationLoadAmmoPoint(activate: false);
			break;
		default:
			Debug.FailedAssert("Invalid WeaponState.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Objects\\Siege\\RangedSiegeWeapon.cs", "OnRangedSiegeWeaponStateChange", 927);
			break;
		case WeaponState.Idle:
		case WeaponState.WaitingAfterShooting:
		case WeaponState.WaitingBeforeReloading:
			break;
		}
	}

	protected virtual void SetActivationLoadAmmoPoint(bool activate)
	{
	}

	protected float GetDetachmentWeightAuxForExternalAmmoWeapons(BattleSideEnum side)
	{
		if (IsDisabledForBattleSideAI(side))
		{
			return float.MinValue;
		}
		_usableStandingPoints.Clear();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = !base.PilotStandingPoint.HasUser && !base.PilotStandingPoint.HasAIMovingTo && (ReloaderAgent == null || ReloaderAgentOriginalPoint != base.PilotStandingPoint);
		int num = -1;
		StandingPoint standingPoint = null;
		bool flag4 = false;
		for (int i = 0; i < base.StandingPoints.Count; i++)
		{
			StandingPoint standingPoint2 = base.StandingPoints[i];
			if (!standingPoint2.GameEntity.HasTag("can_pick_up_ammo"))
			{
				continue;
			}
			if (ReloaderAgent == null || standingPoint2 != ReloaderAgentOriginalPoint)
			{
				if (standingPoint2.IsUsableBySide(side))
				{
					if (!standingPoint2.HasAIMovingTo)
					{
						if (!flag2)
						{
							_usableStandingPoints.Clear();
							if (num != -1)
							{
								num = -1;
							}
						}
						flag2 = true;
					}
					else if (flag2 || standingPoint2.MovingAgent.Formation.Team.Side != side)
					{
						continue;
					}
					flag = true;
					_usableStandingPoints.Add((i, standingPoint2));
					if (flag3 && base.PilotStandingPoint == standingPoint2)
					{
						num = _usableStandingPoints.Count - 1;
					}
				}
				else if (flag3 && standingPoint2.HasAIUser && (standingPoint == null || PilotReservePriorityValues[standingPoint2] > PilotReservePriorityValues[standingPoint] || flag4))
				{
					standingPoint = standingPoint2;
					flag4 = false;
				}
			}
			else if (flag3 && standingPoint == null)
			{
				standingPoint = standingPoint2;
				flag4 = true;
			}
		}
		if (standingPoint != null)
		{
			if (flag4)
			{
				ReloaderAgentOriginalPoint = base.PilotStandingPoint;
			}
			else
			{
				Agent userAgent = standingPoint.UserAgent;
				userAgent.StopUsingGameObjectMT(isSuccessful: true, Agent.StopUsingGameObjectFlags.DoNotWieldWeaponAfterStoppingUsingGameObject);
				userAgent.AIMoveToGameObjectEnable(base.PilotStandingPoint, this, base.Ai.GetScriptedFrameFlags(userAgent));
			}
			if (num != -1)
			{
				_usableStandingPoints.RemoveAt(num);
				num = -1;
			}
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
		if (base.GameEntity.IsVisibleIncludeParents())
		{
			return TickRequirement.Tick | base.GetTickRequirement();
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
			UpdateState(dt);
			if (base.PilotAgent != null && !base.PilotAgent.IsInBeingStruckAction)
			{
				if (base.PilotAgent.MovementFlags.HasAnyFlag(Agent.MovementControlFlag.AttackMask))
				{
					if (State == WeaponState.Idle)
					{
						_aiRequestsShoot = false;
						Shoot();
					}
					else if (State == WeaponState.WaitingAfterShooting && AttackClickWillReload)
					{
						_aiRequestsManualReload = false;
						ManualReload();
					}
				}
				if (_aiRequestsManualReload)
				{
					ManualReload();
				}
				if (_aiRequestsShoot)
				{
					Shoot();
				}
			}
			_aiRequestsShoot = false;
			_aiRequestsManualReload = false;
		}
		HandleUserAiming(dt);
	}

	protected virtual float CalculateShootingRange(float heightDifference)
	{
		return Mission.GetMissileRange(ShootingSpeed, heightDifference);
	}

	protected static bool ApproachToAngle(ref float angle, float angleToApproach, bool isMouse, float speed_limit, float dt, float sensitivity)
	{
		speed_limit = TaleWorlds.Library.MathF.Abs(speed_limit);
		if (angle != angleToApproach)
		{
			float num = sensitivity * dt;
			float num2 = TaleWorlds.Library.MathF.Abs(angle - angleToApproach);
			if (isMouse)
			{
				num *= TaleWorlds.Library.MathF.Max(num2 * 8f, 0.15f);
			}
			if (speed_limit > 0f)
			{
				num = TaleWorlds.Library.MathF.Min(num, speed_limit * dt);
			}
			if (num2 <= num)
			{
				angle = angleToApproach;
			}
			else
			{
				angle += num * (float)TaleWorlds.Library.MathF.Sign(angleToApproach - angle);
			}
			return true;
		}
		return false;
	}

	protected virtual void HandleUserAiming(float dt)
	{
		bool flag = false;
		float horizontalAimSensitivity = HorizontalAimSensitivity;
		float verticalAimSensitivity = VerticalAimSensitivity;
		bool flag2 = false;
		if (cameraState != CameraState.DontMove)
		{
			if (_inputGiven)
			{
				flag2 = true;
				if (CanRotate())
				{
					if (_inputX != 0f)
					{
						targetDirection += horizontalAimSensitivity * dt * _inputX;
						targetDirection = MBMath.WrapAngle(targetDirection);
						targetDirection = MBMath.ClampAngle(targetDirection, currentDirection, 0.7f);
						targetDirection = MBMath.ClampAngle(targetDirection, 0f, DirectionRestriction);
					}
					if (_inputY != 0f)
					{
						targetReleaseAngle += verticalAimSensitivity * dt * _inputY;
						targetReleaseAngle = MBMath.ClampAngle(targetReleaseAngle, currentReleaseAngle + 0.049999997f, 0.6f);
						targetReleaseAngle = MBMath.ClampAngle(targetReleaseAngle, ReleaseAngleRestrictionCenter, ReleaseAngleRestrictionAngle);
					}
				}
				_inputGiven = false;
				_inputX = 0f;
				_inputY = 0f;
			}
			else if (_exactInputGiven)
			{
				bool flag3 = false;
				if (CanRotate())
				{
					if (targetDirection != _inputTargetX)
					{
						float num = horizontalAimSensitivity * dt;
						if (TaleWorlds.Library.MathF.Abs(targetDirection - _inputTargetX) < num)
						{
							targetDirection = _inputTargetX;
						}
						else if (targetDirection < _inputTargetX)
						{
							targetDirection += num;
							flag3 = true;
						}
						else
						{
							targetDirection -= num;
							flag3 = true;
						}
						targetDirection = MBMath.WrapAngle(targetDirection);
						targetDirection = MBMath.ClampAngle(targetDirection, currentDirection, 0.7f);
						targetDirection = MBMath.ClampAngle(targetDirection, 0f, DirectionRestriction);
					}
					if (targetReleaseAngle != _inputTargetY)
					{
						float num2 = verticalAimSensitivity * dt;
						if (TaleWorlds.Library.MathF.Abs(targetReleaseAngle - _inputTargetY) < num2)
						{
							targetReleaseAngle = _inputTargetY;
						}
						else if (targetReleaseAngle < _inputTargetY)
						{
							targetReleaseAngle += num2;
							flag3 = true;
						}
						else
						{
							targetReleaseAngle -= num2;
							flag3 = true;
						}
						targetReleaseAngle = MBMath.ClampAngle(targetReleaseAngle, currentReleaseAngle + 0.049999997f, 0.6f);
						targetReleaseAngle = MBMath.ClampAngle(targetReleaseAngle, ReleaseAngleRestrictionCenter, ReleaseAngleRestrictionAngle);
					}
				}
				else
				{
					flag3 = true;
				}
				if (!flag3)
				{
					_exactInputGiven = false;
				}
			}
		}
		switch (cameraState)
		{
		case CameraState.StickToWeapon:
			flag = ApproachToAngle(ref currentDirection, targetDirection, UsesMouseForAiming, -1f, dt, horizontalAimSensitivity) || flag;
			flag = ApproachToAngle(ref currentReleaseAngle, targetReleaseAngle, UsesMouseForAiming, -1f, dt, verticalAimSensitivity) || flag;
			cameraDirection = currentDirection;
			cameraReleaseAngle = currentReleaseAngle;
			break;
		case CameraState.DontMove:
			dontMoveTimer -= dt;
			if (dontMoveTimer < 0f)
			{
				if (!AttackClickWillReload)
				{
					cameraState = CameraState.MoveDownToReload;
					maxRotateSpeed = 0f;
					reloadTargetReleaseAngle = MBMath.ClampAngle((TaleWorlds.Library.MathF.Abs(currentReleaseAngle) > 0.17453292f) ? 0f : currentReleaseAngle, currentReleaseAngle - 0.049999997f, 0.6f);
					targetDirection = cameraDirection;
					cameraReleaseAngle = targetReleaseAngle;
				}
				else
				{
					cameraState = CameraState.StickToWeapon;
				}
			}
			break;
		case CameraState.MoveDownToReload:
			maxRotateSpeed += dt * 1.2f;
			maxRotateSpeed = TaleWorlds.Library.MathF.Min(maxRotateSpeed, 1f);
			flag = ApproachToAngle(ref currentReleaseAngle, reloadTargetReleaseAngle, UsesMouseForAiming, 0.4f + maxRotateSpeed, dt, verticalAimSensitivity) || flag;
			flag = ApproachToAngle(ref cameraDirection, targetDirection, UsesMouseForAiming, -1f, dt, horizontalAimSensitivity) || flag;
			flag = ApproachToAngle(ref cameraReleaseAngle, reloadTargetReleaseAngle, UsesMouseForAiming, 0.5f + maxRotateSpeed, dt, verticalAimSensitivity) || flag;
			if (!flag)
			{
				cameraState = CameraState.RememberLastShotDirection;
			}
			break;
		case CameraState.RememberLastShotDirection:
			if (State == WeaponState.Idle || flag2)
			{
				cameraState = CameraState.FreeMove;
				this.OnReloadDone?.Invoke();
			}
			break;
		case CameraState.FreeMove:
			flag = ApproachToAngle(ref cameraDirection, targetDirection, UsesMouseForAiming, -1f, dt, horizontalAimSensitivity) || flag;
			flag = ApproachToAngle(ref cameraReleaseAngle, targetReleaseAngle, UsesMouseForAiming, -1f, dt, verticalAimSensitivity) || flag;
			maxRotateSpeed = 0f;
			break;
		case CameraState.ApproachToCamera:
			maxRotateSpeed += 0.9f * dt + maxRotateSpeed * 2f * dt;
			flag = ApproachToAngle(ref cameraDirection, targetDirection, UsesMouseForAiming, -1f, dt, horizontalAimSensitivity) || flag;
			flag = ApproachToAngle(ref cameraReleaseAngle, targetReleaseAngle, UsesMouseForAiming, -1f, dt, verticalAimSensitivity) || flag;
			flag = ApproachToAngle(ref currentDirection, targetDirection, UsesMouseForAiming, maxRotateSpeed, dt, horizontalAimSensitivity) || flag;
			flag = ApproachToAngle(ref currentReleaseAngle, targetReleaseAngle, UsesMouseForAiming, maxRotateSpeed, dt, verticalAimSensitivity) || flag;
			if (!flag)
			{
				cameraState = CameraState.StickToWeapon;
			}
			break;
		}
		if (cameraHolder != null)
		{
			MatrixFrame frame = cameraHolderInitialFrame;
			frame.rotation.RotateAboutForward(cameraDirection - currentDirection);
			frame.rotation.RotateAboutSide(cameraReleaseAngle - currentReleaseAngle);
			cameraHolder.SetFrame(ref frame);
			frame = cameraHolder.GetGlobalFrame();
			frame.rotation.s.z = 0f;
			frame.rotation.s.Normalize();
			frame.rotation.u = Vec3.CrossProduct(frame.rotation.s, frame.rotation.f);
			frame.rotation.u.Normalize();
			frame.rotation.f = Vec3.CrossProduct(frame.rotation.u, frame.rotation.s);
			frame.rotation.f.Normalize();
			cameraHolder.SetGlobalFrame(in frame);
		}
		if (flag && !hasFrameChangedInPreviousFrame)
		{
			OnRotationStarted();
		}
		else if (!flag && hasFrameChangedInPreviousFrame)
		{
			OnRotationStopped();
		}
		hasFrameChangedInPreviousFrame = flag;
		if ((flag && GameNetwork.IsClient && base.PilotAgent == Agent.Main) || GameNetwork.IsServerOrRecorder)
		{
			float num3 = ((GameNetwork.IsClient && base.PilotAgent == Agent.Main) ? 0.0001f : 0.02f);
			if (_syncTimer > 0.2f && (TaleWorlds.Library.MathF.Abs(currentDirection - _lastSyncedDirection) > num3 || TaleWorlds.Library.MathF.Abs(currentReleaseAngle - _lastSyncedReleaseAngle) > num3))
			{
				_lastSyncedDirection = currentDirection;
				_lastSyncedReleaseAngle = currentReleaseAngle;
				MissionLobbyComponent missionBehavior = Mission.Current.GetMissionBehavior<MissionLobbyComponent>();
				if ((missionBehavior == null || missionBehavior.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Ending) && GameNetwork.IsClient && base.PilotAgent == Agent.Main)
				{
					GameNetwork.BeginModuleEventAsClient();
					GameNetwork.WriteMessage(new SetMachineRotation(base.Id, currentDirection, currentReleaseAngle));
					GameNetwork.EndModuleEventAsClient();
				}
				if (GameNetwork.IsServerOrRecorder)
				{
					GameNetwork.BeginBroadcastModuleEvent();
					GameNetwork.WriteMessage(new SetMachineTargetRotation(base.Id, currentDirection, currentReleaseAngle));
					GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.ExcludeTargetPlayer | GameNetwork.EventBroadcastFlags.AddToMissionRecord, base.PilotAgent?.MissionPeer?.GetNetworkPeer());
				}
			}
		}
		_syncTimer += dt;
		if (_syncTimer >= 1f)
		{
			_syncTimer -= 1f;
		}
		if (flag)
		{
			ApplyAimChange();
		}
	}

	public void GiveInput(float inputX, float inputY)
	{
		_exactInputGiven = false;
		_inputGiven = true;
		_inputX = inputX;
		_inputY = inputY;
		_inputX = MBMath.ClampFloat(_inputX, -1f, 1f);
		_inputY = MBMath.ClampFloat(_inputY, -1f, 1f);
	}

	public void GiveExactInput(float targetX, float targetY)
	{
		_exactInputGiven = true;
		_inputGiven = false;
		_inputTargetX = MBMath.ClampAngle(targetX, 0f, DirectionRestriction);
		_inputTargetY = MBMath.ClampAngle(targetY, ReleaseAngleRestrictionCenter, ReleaseAngleRestrictionAngle);
	}

	protected virtual bool CanRotate()
	{
		return State == WeaponState.Idle;
	}

	protected virtual void ApplyAimChange()
	{
		if (CanRotate())
		{
			ApplyCurrentDirectionToEntity();
			return;
		}
		targetDirection = currentDirection;
		targetReleaseAngle = currentReleaseAngle;
	}

	protected virtual void ApplyCurrentDirectionToEntity()
	{
		MatrixFrame frame = _rotationObjectInitialFrame;
		frame.rotation.RotateAboutUp(currentDirection);
		RotationObject.GameEntity.SetFrame(ref frame);
	}

	public virtual float GetTargetDirection(Vec3 target)
	{
		MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
		globalFrame.rotation.RotateAboutUp(System.MathF.PI);
		return globalFrame.TransformToLocal(target).AsVec2.RotationInRadians;
	}

	public virtual float GetTargetReleaseAngle(Vec3 target)
	{
		return Mission.GetMissileVerticalAimCorrection(target - MissleStartingPositionForSimulation, ShootingSpeed, ref _originalMissileWeaponStatsDataForTargeting, ItemObject.GetAirFrictionConstant(OriginalMissileItem.PrimaryWeapon.WeaponClass, OriginalMissileItem.PrimaryWeapon.WeaponFlags));
	}

	public virtual bool AimAtThreat(Threat threat)
	{
		Vec3 target = threat.Position + GetEstimatedTargetMovementVector(threat.Position, threat.GetVelocity());
		float angle = GetTargetDirection(target);
		float angle2 = GetTargetReleaseAngle(target);
		angle = MBMath.ClampAngle(angle, 0f, DirectionRestriction);
		angle2 = MBMath.ClampAngle(angle2, ReleaseAngleRestrictionCenter, ReleaseAngleRestrictionAngle);
		if (!_exactInputGiven || angle != _inputTargetX || angle2 != _inputTargetY)
		{
			GiveExactInput(angle, angle2);
		}
		if (TaleWorlds.Library.MathF.Abs(currentDirection - _inputTargetX) < 0.001f)
		{
			return TaleWorlds.Library.MathF.Abs(currentReleaseAngle - _inputTargetY) < 0.001f;
		}
		return false;
	}

	public virtual void AimAtRotation(float horizontalRotation, float verticalRotation)
	{
		horizontalRotation = MBMath.ClampFloat(horizontalRotation, -System.MathF.PI, System.MathF.PI);
		verticalRotation = MBMath.ClampFloat(verticalRotation, -System.MathF.PI, System.MathF.PI);
		horizontalRotation = MBMath.ClampAngle(horizontalRotation, 0f, DirectionRestriction);
		verticalRotation = MBMath.ClampAngle(verticalRotation, ReleaseAngleRestrictionCenter, ReleaseAngleRestrictionAngle);
		if (!_exactInputGiven || horizontalRotation != _inputTargetX || verticalRotation != _inputTargetY)
		{
			GiveExactInput(horizontalRotation, verticalRotation);
		}
	}

	protected void OnLoadingAmmoPointUsingCancelled(Agent agent, bool isCanceledBecauseOfAnimation)
	{
		if (agent.IsAIControlled)
		{
			if (isCanceledBecauseOfAnimation)
			{
				SendAgentToAmmoPickup(agent);
			}
			else
			{
				SendReloaderAgentToOriginalPoint();
			}
		}
	}

	protected void OnAmmoPickupUsingCancelled(Agent agent, bool isCanceledBecauseOfAnimation)
	{
		if (agent.IsAIControlled)
		{
			SendAgentToAmmoPickup(agent);
		}
	}

	protected void SendAgentToAmmoPickup(Agent agent)
	{
		ReloaderAgent = agent;
		EquipmentIndex wieldedItemIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
		if (wieldedItemIndex != EquipmentIndex.None && agent.Equipment[wieldedItemIndex].CurrentUsageItem.WeaponClass == OriginalMissileItem.PrimaryWeapon.WeaponClass)
		{
			agent.AIMoveToGameObjectEnable(LoadAmmoStandingPoint, this, base.Ai.GetScriptedFrameFlags(agent));
			return;
		}
		StandingPoint standingPoint = base.AmmoPickUpPoints.FirstOrDefault((StandingPoint x) => !x.HasUser);
		if (standingPoint != null)
		{
			agent.AIMoveToGameObjectEnable(standingPoint, this, base.Ai.GetScriptedFrameFlags(agent));
		}
		else
		{
			SendReloaderAgentToOriginalPoint();
		}
	}

	protected void SendReloaderAgentToOriginalPoint()
	{
		if (ReloaderAgent == null)
		{
			return;
		}
		if (ReloaderAgentOriginalPoint != null && !ReloaderAgentOriginalPoint.HasAIMovingTo && !ReloaderAgentOriginalPoint.HasUser)
		{
			if (ReloaderAgent.InteractingWithAnyGameObject())
			{
				ReloaderAgent.StopUsingGameObject(isSuccessful: true, Agent.StopUsingGameObjectFlags.None);
			}
			ReloaderAgent.AIMoveToGameObjectEnable(ReloaderAgentOriginalPoint, this, base.Ai.GetScriptedFrameFlags(ReloaderAgent));
		}
		else if (ReloaderAgentOriginalPoint == null || (ReloaderAgentOriginalPoint.MovingAgent != ReloaderAgent && ReloaderAgentOriginalPoint.UserAgent != ReloaderAgent))
		{
			if (ReloaderAgent.IsUsingGameObject)
			{
				ReloaderAgent.StopUsingGameObject();
			}
			ReloaderAgent = null;
		}
	}

	private void UpdateState(float dt)
	{
		if (LoadAmmoStandingPoint != null)
		{
			if (ReloaderAgent != null)
			{
				if (!ReloaderAgent.IsActive() || ReloaderAgent.Detachment != this)
				{
					ReloaderAgent = null;
				}
				else if (ReloaderAgentOriginalPoint.UserAgent == ReloaderAgent)
				{
					ReloaderAgent = null;
				}
			}
			if (State == WeaponState.LoadingAmmo && ReloaderAgent == null && !LoadAmmoStandingPoint.HasUser)
			{
				SortCanPickUpAmmoStandingPoints();
				StandingPoint standingPoint = null;
				StandingPoint standingPoint2 = null;
				foreach (StandingPoint canPickUpAmmoStandingPoint in CanPickUpAmmoStandingPoints)
				{
					if (canPickUpAmmoStandingPoint.HasUser && canPickUpAmmoStandingPoint.UserAgent.IsAIControlled)
					{
						if (canPickUpAmmoStandingPoint != base.PilotStandingPoint)
						{
							standingPoint = canPickUpAmmoStandingPoint;
							break;
						}
						standingPoint2 = canPickUpAmmoStandingPoint;
					}
				}
				if (standingPoint == null && standingPoint2 != null)
				{
					standingPoint = standingPoint2;
				}
				if (standingPoint != null)
				{
					if (HasAmmo)
					{
						Agent userAgent = standingPoint.UserAgent;
						userAgent.StopUsingGameObject(isSuccessful: true, Agent.StopUsingGameObjectFlags.DoNotWieldWeaponAfterStoppingUsingGameObject);
						ReloaderAgentOriginalPoint = standingPoint;
						SendAgentToAmmoPickup(userAgent);
					}
					else
					{
						_isDisabledForAI = true;
					}
				}
			}
		}
		switch (State)
		{
		case WeaponState.Reloading:
		{
			int num = 0;
			if (ReloadStandingPoints.Count == 0)
			{
				if (base.PilotAgent != null && !base.PilotAgent.IsInBeingStruckAction)
				{
					num = 1;
				}
			}
			else
			{
				foreach (StandingPoint reloadStandingPoint in ReloadStandingPoints)
				{
					if (reloadStandingPoint.HasUser && !reloadStandingPoint.UserAgent.IsInBeingStruckAction)
					{
						num++;
					}
				}
			}
			if (num == 0)
			{
				State = WeaponState.ReloadingPaused;
				break;
			}
			if (_currentReloaderCount != num)
			{
				_currentReloaderCount = num;
				float animationSpeed = TaleWorlds.Library.MathF.Sqrt(_currentReloaderCount);
				for (int j = 0; j < SkeletonOwnerObjects.Length; j++)
				{
					float animationParameterAtChannel2 = SkeletonOwnerObjects[j].GameEntity.Skeleton.GetAnimationParameterAtChannel(0);
					SkeletonOwnerObjects[j].SetAnimationAtChannelSynched(SetUpAnimations[j], 0, animationSpeed);
					if (animationParameterAtChannel2 > 0f)
					{
						SkeletonOwnerObjects[j].SetAnimationChannelParameterSynched(0, animationParameterAtChannel2);
					}
				}
			}
			for (int k = 0; k < Skeletons.Length; k++)
			{
				int animationIndexAtChannel2 = Skeletons[k].GetAnimationIndexAtChannel(0);
				float animationParameterAtChannel3 = Skeletons[k].GetAnimationParameterAtChannel(0);
				if (animationIndexAtChannel2 == SetUpAnimationIndices[k] && animationParameterAtChannel3 >= 0.9999f)
				{
					State = WeaponState.LoadingAmmo;
					animationTimeElapsed = 0f;
				}
			}
			break;
		}
		case WeaponState.ReloadingPaused:
			if (ReloadStandingPoints.Count == 0)
			{
				if (base.PilotAgent != null && !base.PilotAgent.IsInBeingStruckAction)
				{
					State = WeaponState.Reloading;
				}
				break;
			}
			{
				foreach (StandingPoint reloadStandingPoint2 in ReloadStandingPoints)
				{
					if (reloadStandingPoint2.HasUser && !reloadStandingPoint2.UserAgent.IsInBeingStruckAction)
					{
						State = WeaponState.Reloading;
						break;
					}
				}
				break;
			}
		case WeaponState.WaitingBeforeReloading:
			animationTimeElapsed += dt;
			if (!(animationTimeElapsed >= timeGapBetweenShootingEndAndReloadingStart) || (cameraState != CameraState.RememberLastShotDirection && cameraState != CameraState.FreeMove && cameraState != 0 && !(cameraHolder == null)))
			{
				break;
			}
			if (ReloadStandingPoints.Count == 0)
			{
				if (base.PilotAgent != null && !base.PilotAgent.IsInBeingStruckAction)
				{
					State = WeaponState.Reloading;
				}
				break;
			}
			{
				foreach (StandingPoint reloadStandingPoint3 in ReloadStandingPoints)
				{
					if (reloadStandingPoint3.HasUser && !reloadStandingPoint3.UserAgent.IsInBeingStruckAction)
					{
						State = WeaponState.Reloading;
						break;
					}
				}
				break;
			}
		case WeaponState.WaitingBeforeProjectileLeaving:
			animationTimeElapsed += dt;
			if (animationTimeElapsed >= timeGapBetweenShootActionAndProjectileLeaving)
			{
				State = WeaponState.Shooting;
			}
			break;
		case WeaponState.Shooting:
		{
			for (int i = 0; i < Skeletons.Length; i++)
			{
				int animationIndexAtChannel = Skeletons[i].GetAnimationIndexAtChannel(0);
				float animationParameterAtChannel = Skeletons[i].GetAnimationParameterAtChannel(0);
				if (animationIndexAtChannel == FireAnimationIndices[i] && animationParameterAtChannel >= 0.9999f)
				{
					State = ((!AttackClickWillReload) ? WeaponState.WaitingBeforeReloading : WeaponState.WaitingAfterShooting);
					animationTimeElapsed = 0f;
				}
			}
			break;
		}
		default:
			Debug.FailedAssert("Invalid WeaponState.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Objects\\Siege\\RangedSiegeWeapon.cs", "UpdateState", 1899);
			break;
		case WeaponState.Idle:
		case WeaponState.WaitingAfterShooting:
		case WeaponState.LoadingAmmo:
		case WeaponState.WaitingBeforeIdle:
			break;
		}
	}

	public bool Shoot()
	{
		_lastShooterAgent = base.PilotAgent;
		if (State == WeaponState.Idle)
		{
			State = WeaponState.WaitingBeforeProjectileLeaving;
			if (!GameNetwork.IsClientOrReplay)
			{
				animationTimeElapsed = 0f;
			}
			return true;
		}
		return false;
	}

	public void ManualReload()
	{
		if (AttackClickWillReload)
		{
			State = WeaponState.WaitingBeforeReloading;
		}
	}

	public void AiRequestsShoot()
	{
		_aiRequestsShoot = true;
	}

	public void AiRequestsManualReload()
	{
		_aiRequestsManualReload = true;
	}

	private Vec3 GetBallisticErrorAppliedDirection(float BallisticErrorAmount)
	{
		Mat3 mat = default(Mat3);
		mat.f = ShootingDirection;
		mat.u = Vec3.Up;
		Mat3 mat2 = mat;
		mat2.Orthonormalize();
		float a = MBRandom.RandomFloat * (System.MathF.PI * 2f);
		mat2.RotateAboutForward(a);
		float f = BallisticErrorAmount * MBRandom.RandomFloat;
		mat2.RotateAboutSide(f.ToRadians());
		return mat2.f;
	}

	private void ShootProjectile()
	{
		if (LoadedMissileItem.StringId == "grapeshot_fire_stack")
		{
			ItemObject @object = Game.Current.ObjectManager.GetObject<ItemObject>("grapeshot_fire_projectile");
			for (int i = 0; i < 5; i++)
			{
				ShootProjectileAux(@object, randomizeMissileSpeed: true);
			}
		}
		else
		{
			ShootProjectileAux(LoadedMissileItem, randomizeMissileSpeed: false);
		}
		_lastShooterAgent = null;
	}

	private void ShootProjectileAux(ItemObject missileItem, bool randomizeMissileSpeed)
	{
		Mat3 identity = Mat3.Identity;
		float num = ShootingSpeed;
		if (randomizeMissileSpeed)
		{
			num *= MBRandom.RandomFloatRanged(0.9f, 1.1f);
			identity.f = GetBallisticErrorAppliedDirection(2.5f);
			identity.Orthonormalize();
		}
		else
		{
			identity.f = GetBallisticErrorAppliedDirection(MaximumBallisticError);
			identity.Orthonormalize();
		}
		Mission.Current.AddCustomMissile(_lastShooterAgent, new MissionWeapon(missileItem, null, _lastShooterAgent.Origin?.Banner, 1), ProjectileEntityCurrentGlobalPosition, identity.f, identity, LoadedMissileItem.PrimaryWeapon.MissileSpeed, num, addRigidBody: false, this);
	}

	protected void OnRotationStarted()
	{
		if (MoveSound == null || !MoveSound.IsValid)
		{
			MoveSound = SoundEvent.CreateEvent(MoveSoundIndex, base.Scene);
			MoveSound.PlayInPosition(RotationObject.GameEntity.GlobalPosition);
		}
	}

	protected void OnRotationStopped()
	{
		MoveSound.Stop();
		MoveSound = null;
	}

	public abstract override SiegeEngineType GetSiegeEngineType();

	public bool CanShootAtBox(Vec3 boxMin, Vec3 boxMax, uint attempts = 5u)
	{
		Vec3 v;
		Vec3 vec = (v = (boxMin + boxMax) / 2f);
		v.z = boxMin.z;
		Vec3 v2 = vec;
		v2.z = boxMax.z;
		uint num = attempts;
		do
		{
			Vec3 target = Vec3.Lerp(v, v2, (float)num / (float)attempts);
			if (CanShootAtPoint(target))
			{
				return true;
			}
			num--;
		}
		while (num != 0);
		return false;
	}

	public bool CanShootAtBoxSimplified(Vec3 boxMin, Vec3 boxMax)
	{
		Vec3 vec = (boxMin + boxMax) / 2f;
		Vec3 target = vec;
		target.z = boxMax.z;
		if (!CanShootAtPoint(vec))
		{
			return CanShootAtPoint(target);
		}
		return true;
	}

	public bool CanShootAtThreat(Threat threat)
	{
		Vec3 targetingOffset = threat.WeaponEntity.GetTargetingOffset();
		Vec3 vec = threat.BoundingBoxMax + targetingOffset;
		Vec3 vec2 = threat.BoundingBoxMin + targetingOffset;
		Vec3 vec3 = (vec + vec2) * 0.5f;
		Vec3 estimatedTargetMovementVector = GetEstimatedTargetMovementVector(vec3, threat.GetVelocity());
		vec3 += estimatedTargetMovementVector;
		vec += estimatedTargetMovementVector;
		Vec3 target = vec3;
		target.z = vec.z;
		if (!CanShootAtPoint(vec3))
		{
			return CanShootAtPoint(target);
		}
		return true;
	}

	public Vec3 GetEstimatedTargetMovementVector(Vec3 targetCurrentPosition, Vec3 targetVelocity)
	{
		if (targetVelocity != Vec3.Zero)
		{
			return targetVelocity * ((base.GameEntity.GlobalPosition - targetCurrentPosition).Length / ShootingSpeed + timeGapBetweenShootActionAndProjectileLeaving);
		}
		return Vec3.Zero;
	}

	public bool CanShootAtAgent(Agent agent)
	{
		Vec3 boxMax = agent.CollisionCapsule.GetBoxMax();
		Vec3 target = (agent.CollisionCapsule.GetBoxMin() + boxMax) / 2f;
		return CanShootAtPoint(target);
	}

	public bool CanShootAtPoint(Vec3 target)
	{
		float num = GetTargetReleaseAngle(target);
		if (num < BottomReleaseAngleRestriction || num > TopReleaseAngleRestriction)
		{
			return false;
		}
		float f = (target.AsVec2 - ProjectileEntityCurrentGlobalPosition.AsVec2).Normalized().AngleBetween(OriginalDirection.AsVec2.Normalized());
		if (DirectionRestriction / 2f - TaleWorlds.Library.MathF.Abs(f) < 0f)
		{
			return false;
		}
		if (Side == BattleSideEnum.Attacker)
		{
			foreach (SiegeWeapon item in Mission.Current.GetAttackerWeaponsForFriendlyFirePreventing())
			{
				if (item.GameEntity != null && item.GameEntity.IsVisibleIncludeParents())
				{
					Vec3 vec = (item.GameEntity.PhysicsGlobalBoxMin + item.GameEntity.PhysicsGlobalBoxMax) * 0.5f;
					if ((MBMath.GetClosestPointInLineSegmentToPoint(vec, MissleStartingPositionForSimulation, target) - vec).LengthSquared < 100f)
					{
						return false;
					}
				}
			}
		}
		Vec3 missleStartingPositionForSimulation = MissleStartingPositionForSimulation;
		Vec3 vec2 = ((MissileStartingPositionEntityForSimulation == null) ? CanShootAtPointCheckingOffset : Vec3.Zero);
		Vec3 target2 = target;
		return base.Scene.CheckPointCanSeePoint(missleStartingPositionForSimulation + vec2, target2);
	}

	protected internal virtual bool IsTargetValid(ITargetable target)
	{
		return true;
	}

	public override OrderType GetOrder(BattleSideEnum side)
	{
		if (!base.IsDestroyed)
		{
			if (Side != side)
			{
				return OrderType.AttackEntity;
			}
			return OrderType.Use;
		}
		return OrderType.None;
	}

	protected override GameEntity GetEntityToAttachNavMeshFaces()
	{
		return RotationObject.GameEntity;
	}

	public abstract float ProcessTargetValue(float baseValue, TargetFlags flags);

	public override void OnAfterReadFromNetwork((BaseSynchedMissionObjectReadableRecord, ISynchedMissionObjectReadableRecord) synchedMissionObjectReadableRecord)
	{
		base.OnAfterReadFromNetwork(synchedMissionObjectReadableRecord);
		RangedSiegeWeaponRecord rangedSiegeWeaponRecord = (RangedSiegeWeaponRecord)(object)synchedMissionObjectReadableRecord.Item2;
		_state = (WeaponState)rangedSiegeWeaponRecord.State;
		targetDirection = rangedSiegeWeaponRecord.TargetDirection;
		targetReleaseAngle = MBMath.ClampFloat(rangedSiegeWeaponRecord.TargetReleaseAngle, BottomReleaseAngleRestriction, TopReleaseAngleRestriction);
		AmmoCount = rangedSiegeWeaponRecord.AmmoCount;
		currentDirection = targetDirection;
		currentReleaseAngle = targetReleaseAngle;
		currentDirection = targetDirection;
		currentReleaseAngle = targetReleaseAngle;
		ApplyCurrentDirectionToEntity();
		CheckAmmo();
		UpdateAmmoMesh();
		ChangeProjectileEntityClient(rangedSiegeWeaponRecord.ProjectileIndex);
	}

	protected virtual void UpdateAmmoMesh()
	{
		GameEntity gameEntity = AmmoPickUpStandingPoints[0].GameEntity;
		int num = 20 - AmmoCount;
		while (gameEntity.Parent != null)
		{
			for (int i = 0; i < gameEntity.MultiMeshComponentCount; i++)
			{
				MetaMesh metaMesh = gameEntity.GetMetaMesh(i);
				for (int j = 0; j < metaMesh.MeshCount; j++)
				{
					metaMesh.GetMeshAtIndex(j).SetVectorArgument(0f, num, 0f, 0f);
				}
			}
			gameEntity = gameEntity.Parent;
		}
	}

	protected override bool IsAnyUserBelongsToFormation(Formation formation)
	{
		return base.IsAnyUserBelongsToFormation(formation) | (ReloaderAgent?.Formation == formation);
	}
}
