using System;
using System.Collections.Generic;
using System.Linq;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace TaleWorlds.MountAndBlade;

public class DestructableComponent : SynchedMissionObject, IFocusable
{
	[DefineSynchedMissionObjectType(typeof(DestructableComponent))]
	public struct DestructableComponentRecord : ISynchedMissionObjectReadableRecord
	{
		public float HitPoint { get; private set; }

		public int DestructionState { get; private set; }

		public int ForceIndex { get; private set; }

		public bool IsMissionObject { get; private set; }

		public DestructableComponentRecord(float hitPoint, int destructionState, int forceIndex, bool isMissionObject)
		{
			HitPoint = hitPoint;
			DestructionState = destructionState;
			ForceIndex = forceIndex;
			IsMissionObject = isMissionObject;
		}

		public bool ReadFromNetwork(ref bool bufferReadValid)
		{
			HitPoint = GameNetworkMessage.ReadFloatFromPacket(CompressionMission.UsableGameObjectHealthCompressionInfo, ref bufferReadValid);
			DestructionState = GameNetworkMessage.ReadIntFromPacket(CompressionMission.UsableGameObjectDestructionStateCompressionInfo, ref bufferReadValid);
			ForceIndex = -1;
			if (DestructionState != 0)
			{
				IsMissionObject = GameNetworkMessage.ReadBoolFromPacket(ref bufferReadValid);
				if (IsMissionObject)
				{
					ForceIndex = GameNetworkMessage.ReadMissionObjectIdFromPacket(ref bufferReadValid).Id;
				}
			}
			return bufferReadValid;
		}
	}

	public delegate void OnHitTakenAndDestroyedDelegate(DestructableComponent target, Agent attackerAgent, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, int inflictedDamage);

	public const string CleanStateTag = "operational";

	public static float MaxBlowMagnitude = 20f;

	public string DestructionStates;

	public bool DestroyedByStoneOnly;

	public bool CanBeDestroyedInitially = true;

	public float MaxHitPoint = 100f;

	public bool DestroyOnAnyHit;

	public bool PassHitOnToParent;

	public string ReferenceEntityTag;

	public string HeavyHitParticlesTag;

	public float HeavyHitParticlesThreshold = 5f;

	public string ParticleEffectOnDestroy = "";

	public string SoundEffectOnDestroy = "";

	public float SoundAndParticleEffectHeightOffset;

	public float SoundAndParticleEffectForwardOffset;

	public BattleSideEnum BattleSide = BattleSideEnum.None;

	[EditableScriptComponentVariable(false)]
	public Func<int, int, int, int> OnCalculateDestructionStateIndex;

	private float _hitPoint;

	private string OriginalStateTag = "operational";

	private GameEntity _referenceEntity;

	private GameEntity _previousState;

	private GameEntity _originalState;

	private string[] _destructionStates;

	private int _currentStateIndex;

	private IEnumerable<GameEntity> _heavyHitParticles;

	public float HitPoint
	{
		get
		{
			return _hitPoint;
		}
		set
		{
			if (!_hitPoint.Equals(value))
			{
				_hitPoint = TaleWorlds.Library.MathF.Max(value, 0f);
				if (GameNetwork.IsServerOrRecorder)
				{
					GameNetwork.BeginBroadcastModuleEvent();
					GameNetwork.WriteMessage(new SyncObjectHitpoints(base.Id, value));
					GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
				}
			}
		}
	}

	public FocusableObjectType FocusableObjectType => FocusableObjectType.None;

	public bool IsDestroyed => HitPoint <= 0f;

	public GameEntity CurrentState { get; private set; }

	private bool HasDestructionState
	{
		get
		{
			if (_destructionStates != null)
			{
				return !_destructionStates.IsEmpty();
			}
			return false;
		}
	}

	public event Action OnNextDestructionState;

	public event OnHitTakenAndDestroyedDelegate OnDestroyed;

	public event OnHitTakenAndDestroyedDelegate OnHitTaken;

	private DestructableComponent()
	{
	}

	protected internal override void OnInit()
	{
		base.OnInit();
		_hitPoint = MaxHitPoint;
		_referenceEntity = (string.IsNullOrEmpty(ReferenceEntityTag) ? base.GameEntity : base.GameEntity.GetChildren().FirstOrDefault((GameEntity x) => x.HasTag(ReferenceEntityTag)));
		if (!string.IsNullOrEmpty(DestructionStates))
		{
			_destructionStates = DestructionStates.Replace(" ", string.Empty).Split(new char[1] { ',' });
			bool flag = false;
			string[] destructionStates = _destructionStates;
			foreach (string item in destructionStates)
			{
				if (string.IsNullOrEmpty(item))
				{
					continue;
				}
				GameEntity gameEntity = base.GameEntity.GetChildren().FirstOrDefault((GameEntity x) => x.Name == item);
				if (gameEntity != null)
				{
					PhysicsShape physicsShape = null;
					gameEntity.AddBodyFlags(BodyFlags.Moveable);
					physicsShape = gameEntity.GetBodyShape();
					if (physicsShape != null)
					{
						PhysicsShape.AddPreloadQueueWithName(physicsShape.GetName(), gameEntity.GetGlobalScale());
						flag = true;
					}
					continue;
				}
				GameEntity gameEntity2 = GameEntity.Instantiate(null, item, callScriptCallbacks: false);
				List<GameEntity> children = new List<GameEntity>();
				gameEntity2.GetChildrenRecursive(ref children);
				children.Add(gameEntity2);
				PhysicsShape physicsShape2 = null;
				foreach (GameEntity item2 in children)
				{
					physicsShape2 = item2.GetBodyShape();
					if (physicsShape2 != null)
					{
						Vec3 globalScale = item2.GetGlobalScale();
						Vec3 globalScale2 = _referenceEntity.GetGlobalScale();
						PhysicsShape.AddPreloadQueueWithName(scale: new Vec3(globalScale.x * globalScale2.x, globalScale.y * globalScale2.y, globalScale.z * globalScale2.z), bodyName: physicsShape2.GetName());
						flag = true;
					}
				}
			}
			if (flag)
			{
				PhysicsShape.ProcessPreloadQueue();
			}
		}
		_originalState = GetOriginalState(base.GameEntity);
		if (_originalState == null)
		{
			_originalState = base.GameEntity;
		}
		CurrentState = _originalState;
		_originalState.AddBodyFlags(BodyFlags.Moveable);
		List<GameEntity> children2 = new List<GameEntity>();
		base.GameEntity.GetChildrenRecursive(ref children2);
		foreach (GameEntity item3 in children2.Where((GameEntity child) => child.BodyFlag.HasAnyFlag(BodyFlags.Dynamic)))
		{
			item3.SetPhysicsState(isEnabled: false, setChildren: true);
			item3.SetFrameChanged();
		}
		_heavyHitParticles = base.GameEntity.CollectChildrenEntitiesWithTag(HeavyHitParticlesTag);
		base.GameEntity.SetAnimationSoundActivation(activate: true);
	}

	public GameEntity GetOriginalState(GameEntity parent)
	{
		int childCount = parent.ChildCount;
		for (int i = 0; i < childCount; i++)
		{
			GameEntity child = parent.GetChild(i);
			if (!child.HasScriptOfType<DestructableComponent>())
			{
				if (child.HasTag(OriginalStateTag))
				{
					return child;
				}
				GameEntity originalState = GetOriginalState(child);
				if (originalState != null)
				{
					return originalState;
				}
			}
		}
		return null;
	}

	protected internal override void OnEditorInit()
	{
		base.OnEditorInit();
		_referenceEntity = (string.IsNullOrEmpty(ReferenceEntityTag) ? base.GameEntity : base.GameEntity.GetChildren().FirstOrDefault((GameEntity x) => x.HasTag(ReferenceEntityTag)));
	}

	protected internal override void OnEditorVariableChanged(string variableName)
	{
		base.OnEditorVariableChanged(variableName);
		if (variableName.Equals(ReferenceEntityTag))
		{
			_referenceEntity = (string.IsNullOrEmpty(ReferenceEntityTag) ? base.GameEntity : base.GameEntity.GetChildren().SingleOrDefault((GameEntity x) => x.HasTag(ReferenceEntityTag)));
		}
	}

	protected internal override void OnMissionReset()
	{
		base.OnMissionReset();
		Reset();
	}

	public void Reset()
	{
		RestoreEntity();
		_hitPoint = MaxHitPoint;
		_currentStateIndex = 0;
	}

	private void RestoreEntity()
	{
		if (_destructionStates != null)
		{
			int i;
			for (i = 0; i < _destructionStates.Length; i++)
			{
				base.GameEntity.GetChildren().FirstOrDefault((GameEntity x) => x.Name == _destructionStates[i].ToString())?.Skeleton?.SetAnimationAtChannel(-1, 0);
			}
		}
		if (CurrentState != _originalState)
		{
			CurrentState.SetVisibilityExcludeParents(visible: false);
			CurrentState = _originalState;
		}
		CurrentState.SetVisibilityExcludeParents(visible: true);
		CurrentState.SetPhysicsState(isEnabled: true, setChildren: true);
		CurrentState.SetFrameChanged();
	}

	protected internal override void OnEditorTick(float dt)
	{
		base.OnEditorTick(dt);
		if (_referenceEntity != null && _referenceEntity != base.GameEntity && MBEditor.IsEntitySelected(_referenceEntity))
		{
			new Vec3(-2f, -0.5f, -1f);
			new Vec3(2f, 0.5f, 1f);
			MatrixFrame output = MatrixFrame.Identity;
			GameEntity gameEntity = _referenceEntity;
			while (gameEntity.Parent != null)
			{
				gameEntity = gameEntity.Parent;
			}
			gameEntity.GetMeshBendedFrame(_referenceEntity.GetGlobalFrame(), ref output);
		}
	}

	public void TriggerOnHit(Agent attackerAgent, int inflictedDamage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior)
	{
		OnHit(attackerAgent, inflictedDamage, impactPosition, impactDirection, in weapon, attackerScriptComponentBehavior, out var _);
	}

	protected internal override bool OnHit(Agent attackerAgent, int inflictedDamage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
	{
		reportDamage = false;
		if (base.IsDisabled)
		{
			return true;
		}
		if (weapon.IsEmpty && !(attackerScriptComponentBehavior is BatteringRam))
		{
			inflictedDamage = 0;
		}
		else if (DestroyedByStoneOnly)
		{
			WeaponComponentData currentUsageItem = weapon.CurrentUsageItem;
			if ((currentUsageItem.WeaponClass != WeaponClass.Stone && currentUsageItem.WeaponClass != WeaponClass.Boulder) || !currentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.NotUsableWithOneHand))
			{
				inflictedDamage = 0;
			}
		}
		bool isDestroyed = IsDestroyed;
		if (DestroyOnAnyHit)
		{
			inflictedDamage = (int)(MaxHitPoint + 1f);
		}
		if (inflictedDamage > 0 && !isDestroyed)
		{
			HitPoint -= inflictedDamage;
			if ((float)inflictedDamage > HeavyHitParticlesThreshold)
			{
				BurstHeavyHitParticles();
			}
			int state = CalculateNextDestructionLevel(inflictedDamage);
			if (!IsDestroyed)
			{
				this.OnHitTaken?.Invoke(this, attackerAgent, in weapon, attackerScriptComponentBehavior, inflictedDamage);
			}
			else if (IsDestroyed && !isDestroyed)
			{
				Mission.Current.OnObjectDisabled(this);
				this.OnHitTaken?.Invoke(this, attackerAgent, in weapon, attackerScriptComponentBehavior, inflictedDamage);
				this.OnDestroyed?.Invoke(this, attackerAgent, in weapon, attackerScriptComponentBehavior, inflictedDamage);
				MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
				globalFrame.origin += globalFrame.rotation.u * SoundAndParticleEffectHeightOffset + globalFrame.rotation.f * SoundAndParticleEffectForwardOffset;
				globalFrame.rotation.Orthonormalize();
				if (ParticleEffectOnDestroy != "")
				{
					Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(ParticleEffectOnDestroy), globalFrame);
				}
				if (SoundEffectOnDestroy != "")
				{
					Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(SoundEffectOnDestroy), globalFrame.origin, soundCanBePredicted: false, isReliable: true, attackerAgent?.Index ?? (-1), -1);
				}
			}
			SetDestructionLevel(state, -1, inflictedDamage, impactPosition, impactDirection);
			reportDamage = true;
		}
		return !PassHitOnToParent;
	}

	public void BurstHeavyHitParticles()
	{
		foreach (GameEntity heavyHitParticle in _heavyHitParticles)
		{
			heavyHitParticle.BurstEntityParticle(doChildren: false);
		}
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new BurstAllHeavyHitParticles(base.Id));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
	}

	private int CalculateNextDestructionLevel(int inflictedDamage)
	{
		if (HasDestructionState)
		{
			int num = _destructionStates.Length;
			float num2 = MaxHitPoint / (float)num;
			float num3 = MaxHitPoint;
			int num4 = 0;
			while (num3 - num2 >= HitPoint)
			{
				num3 -= num2;
				num4++;
			}
			return OnCalculateDestructionStateIndex?.Invoke(num4, inflictedDamage, DestructionStates.Length) ?? num4;
		}
		if (IsDestroyed)
		{
			return _currentStateIndex + 1;
		}
		return _currentStateIndex;
	}

	public void SetDestructionLevel(int state, int forcedId, float blowMagnitude, Vec3 blowPosition, Vec3 blowDirection, bool noEffects = false)
	{
		if (_currentStateIndex == state)
		{
			return;
		}
		float blowMagnitude2 = MBMath.ClampFloat(blowMagnitude, 1f, MaxBlowMagnitude);
		_currentStateIndex = state;
		ReplaceEntityWithBrokenEntity(forcedId);
		if (CurrentState != null)
		{
			foreach (GameEntity item in from child in CurrentState.GetChildren()
				where child.BodyFlag.HasAnyFlag(BodyFlags.Dynamic)
				select child)
			{
				item.SetPhysicsState(isEnabled: true, setChildren: true);
				item.SetFrameChanged();
			}
			if (!GameNetwork.IsDedicatedServer && !noEffects)
			{
				CurrentState.BurstEntityParticle(doChildren: true);
				ApplyPhysics(blowMagnitude2, blowPosition, blowDirection);
			}
			this.OnNextDestructionState?.Invoke();
		}
		if (!GameNetwork.IsServerOrRecorder)
		{
			return;
		}
		if (CurrentState != null)
		{
			MissionObject firstScriptOfType = CurrentState.GetFirstScriptOfType<MissionObject>();
			if (firstScriptOfType != null)
			{
				forcedId = firstScriptOfType.Id.Id;
			}
		}
		GameNetwork.BeginBroadcastModuleEvent();
		GameNetwork.WriteMessage(new SyncObjectDestructionLevel(base.Id, state, forcedId, blowMagnitude2, blowPosition, blowDirection));
		GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
	}

	private void ApplyPhysics(float blowMagnitude, Vec3 blowPosition, Vec3 blowDirection)
	{
		if (!(CurrentState != null))
		{
			return;
		}
		IEnumerable<GameEntity> enumerable = from child in CurrentState.GetChildren()
			where child.HasBody() && child.BodyFlag.HasAnyFlag(BodyFlags.Dynamic) && !child.HasScriptOfType<SpawnedItemEntity>()
			select child;
		int num = enumerable.Count();
		float num2 = ((num > 1) ? (blowMagnitude / (float)num) : blowMagnitude);
		foreach (GameEntity item in enumerable)
		{
			item.ApplyLocalImpulseToDynamicBody(Vec3.Zero, blowDirection * num2);
			Mission.Current.AddTimerToDynamicEntity(item, 10f + MBRandom.RandomFloat * 2f);
		}
	}

	private void ReplaceEntityWithBrokenEntity(int forcedId)
	{
		_previousState = CurrentState;
		_previousState.SetVisibilityExcludeParents(visible: false);
		if (!HasDestructionState)
		{
			return;
		}
		CurrentState = AddBrokenEntity(_destructionStates[_currentStateIndex - 1], out var newCreated);
		if (newCreated)
		{
			if (_originalState != base.GameEntity)
			{
				base.GameEntity.AddChild(CurrentState, autoLocalizeFrame: true);
			}
			if (forcedId == -1)
			{
				return;
			}
			MissionObject firstScriptOfType = CurrentState.GetFirstScriptOfType<MissionObject>();
			if (firstScriptOfType != null)
			{
				firstScriptOfType.Id = new MissionObjectId(forcedId, createdAtRuntime: true);
				{
					foreach (GameEntity child in CurrentState.GetChildren())
					{
						MissionObject firstScriptOfType2 = child.GetFirstScriptOfType<MissionObject>();
						if (firstScriptOfType2 != null && firstScriptOfType2.Id.CreatedAtRuntime)
						{
							firstScriptOfType2.Id = new MissionObjectId(++forcedId, createdAtRuntime: true);
						}
					}
					return;
				}
			}
			MBDebug.ShowWarning("Current destruction state doesn't have mission object script component.");
		}
		else
		{
			MatrixFrame frame = _previousState.GetFrame();
			CurrentState.SetFrame(ref frame);
		}
	}

	protected internal override bool MovesEntity()
	{
		return true;
	}

	public void PreDestroy()
	{
		this.OnDestroyed?.Invoke(this, null, in MissionWeapon.Invalid, null, 0);
		SetVisibleSynched(value: false, forceChildrenVisible: true);
	}

	private GameEntity AddBrokenEntity(string prefab, out bool newCreated)
	{
		if (!string.IsNullOrEmpty(prefab))
		{
			GameEntity gameEntity = base.GameEntity.GetChildren().FirstOrDefault((GameEntity x) => x.Name == prefab);
			if (gameEntity != null)
			{
				gameEntity.SetVisibilityExcludeParents(visible: true);
				if (!GameNetwork.IsClientOrReplay)
				{
					gameEntity.GetScriptComponents<MissionObject>().FirstOrDefault()?.SetAbilityOfFaces(enabled: true);
				}
				newCreated = false;
			}
			else
			{
				gameEntity = GameEntity.Instantiate(Mission.Current.Scene, prefab, _referenceEntity.GetGlobalFrame());
				if (gameEntity != null)
				{
					gameEntity.SetMobility(GameEntity.Mobility.stationary);
				}
				if (base.GameEntity.Parent != null)
				{
					base.GameEntity.Parent.AddChild(gameEntity, autoLocalizeFrame: true);
				}
				newCreated = true;
			}
			if (_referenceEntity.Skeleton != null && gameEntity.Skeleton != null)
			{
				Skeleton skeleton = ((CurrentState != _originalState) ? CurrentState : _referenceEntity).Skeleton;
				int animationIndexAtChannel = skeleton.GetAnimationIndexAtChannel(0);
				float animationParameterAtChannel = skeleton.GetAnimationParameterAtChannel(0);
				if (animationIndexAtChannel != -1)
				{
					gameEntity.Skeleton.SetAnimationAtChannel(animationIndexAtChannel, 0, 1f, -1f, animationParameterAtChannel);
					gameEntity.ResumeSkeletonAnimation();
				}
			}
			return gameEntity;
		}
		newCreated = false;
		return null;
	}

	public override void WriteToNetwork()
	{
		base.WriteToNetwork();
		GameNetworkMessage.WriteFloatToPacket(TaleWorlds.Library.MathF.Max(HitPoint, 0f), CompressionMission.UsableGameObjectHealthCompressionInfo);
		GameNetworkMessage.WriteIntToPacket(_currentStateIndex, CompressionMission.UsableGameObjectDestructionStateCompressionInfo);
		if (_currentStateIndex != 0)
		{
			MissionObject firstScriptOfType = CurrentState.GetFirstScriptOfType<MissionObject>();
			GameNetworkMessage.WriteBoolToPacket(firstScriptOfType != null);
			if (firstScriptOfType != null)
			{
				GameNetworkMessage.WriteMissionObjectIdToPacket(firstScriptOfType.Id);
			}
		}
	}

	public override void AddStuckMissile(GameEntity missileEntity)
	{
		if (CurrentState != null)
		{
			CurrentState.AddChild(missileEntity);
		}
		else
		{
			base.GameEntity.AddChild(missileEntity);
		}
	}

	protected internal override bool OnCheckForProblems()
	{
		bool result = base.OnCheckForProblems();
		if ((string.IsNullOrEmpty(ReferenceEntityTag) ? base.GameEntity : base.GameEntity.GetChildren().FirstOrDefault((GameEntity x) => x.HasTag(ReferenceEntityTag))) == null)
		{
			MBEditor.AddEntityWarning(base.GameEntity, "Reference entity must be assigned. Root entity is " + base.GameEntity.Root.Name + ", child is " + base.GameEntity.Name);
			result = true;
		}
		string[] array = DestructionStates.Replace(" ", string.Empty).Split(new char[1] { ',' });
		foreach (string destructionState in array)
		{
			if (!string.IsNullOrEmpty(destructionState) && !(base.GameEntity.GetChildren().FirstOrDefault((GameEntity x) => x.Name == destructionState) != null) && GameEntity.Instantiate(null, destructionState, callScriptCallbacks: false) == null)
			{
				MBEditor.AddEntityWarning(base.GameEntity, "Destruction state '" + destructionState + "' is not valid.");
				result = true;
			}
		}
		return result;
	}

	public void OnFocusGain(Agent userAgent)
	{
	}

	public void OnFocusLose(Agent userAgent)
	{
	}

	public TextObject GetInfoTextForBeingNotInteractable(Agent userAgent)
	{
		return new TextObject();
	}

	public string GetDescriptionText(GameEntity gameEntity = null)
	{
		if (int.TryParse(gameEntity.Name.Split(new char[1] { '_' }).Last(), out var result))
		{
			string name = gameEntity.Name;
			name = name.Remove(name.Length - result.ToString().Length);
			name += "x";
			if (GameTexts.TryGetText("str_destructible_component", out var textObject, name))
			{
				return textObject.ToString();
			}
			return "";
		}
		if (GameTexts.TryGetText("str_destructible_component", out var textObject2, gameEntity.Name))
		{
			return textObject2.ToString();
		}
		return "";
	}

	public override void OnAfterReadFromNetwork((BaseSynchedMissionObjectReadableRecord, ISynchedMissionObjectReadableRecord) synchedMissionObjectReadableRecord)
	{
		base.OnAfterReadFromNetwork(synchedMissionObjectReadableRecord);
		DestructableComponentRecord destructableComponentRecord = (DestructableComponentRecord)(object)synchedMissionObjectReadableRecord.Item2;
		HitPoint = destructableComponentRecord.HitPoint;
		if (destructableComponentRecord.DestructionState != 0)
		{
			if (IsDestroyed)
			{
				this.OnDestroyed?.Invoke(this, null, in MissionWeapon.Invalid, null, 0);
			}
			SetDestructionLevel(destructableComponentRecord.DestructionState, destructableComponentRecord.ForceIndex, 0f, Vec3.Zero, Vec3.Zero, noEffects: true);
		}
	}
}
