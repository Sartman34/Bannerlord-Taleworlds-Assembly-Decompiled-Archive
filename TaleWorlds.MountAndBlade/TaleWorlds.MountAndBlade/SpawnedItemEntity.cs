using System;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public class SpawnedItemEntity : UsableMissionObject
{
	private class ClientSyncData
	{
		public MatrixFrame Frame;

		public GameEntity Parent;

		public Timer Timer;
	}

	private static readonly ActionIndexCache act_pickup_down_begin = ActionIndexCache.Create("act_pickup_down_begin");

	private static readonly ActionIndexCache act_pickup_down_end = ActionIndexCache.Create("act_pickup_down_end");

	private static readonly ActionIndexCache act_pickup_down_begin_left_stance = ActionIndexCache.Create("act_pickup_down_begin_left_stance");

	private static readonly ActionIndexCache act_pickup_down_end_left_stance = ActionIndexCache.Create("act_pickup_down_end_left_stance");

	private static readonly ActionIndexCache act_pickup_down_left_begin = ActionIndexCache.Create("act_pickup_down_left_begin");

	private static readonly ActionIndexCache act_pickup_down_left_end = ActionIndexCache.Create("act_pickup_down_left_end");

	private static readonly ActionIndexCache act_pickup_down_left_begin_left_stance = ActionIndexCache.Create("act_pickup_down_left_begin_left_stance");

	private static readonly ActionIndexCache act_pickup_down_left_end_left_stance = ActionIndexCache.Create("act_pickup_down_left_end_left_stance");

	private static readonly ActionIndexCache act_pickup_middle_begin = ActionIndexCache.Create("act_pickup_middle_begin");

	private static readonly ActionIndexCache act_pickup_middle_end = ActionIndexCache.Create("act_pickup_middle_end");

	private static readonly ActionIndexCache act_pickup_middle_begin_left_stance = ActionIndexCache.Create("act_pickup_middle_begin_left_stance");

	private static readonly ActionIndexCache act_pickup_middle_end_left_stance = ActionIndexCache.Create("act_pickup_middle_end_left_stance");

	private static readonly ActionIndexCache act_pickup_middle_left_begin = ActionIndexCache.Create("act_pickup_middle_left_begin");

	private static readonly ActionIndexCache act_pickup_middle_left_end = ActionIndexCache.Create("act_pickup_middle_left_end");

	private static readonly ActionIndexCache act_pickup_middle_left_begin_left_stance = ActionIndexCache.Create("act_pickup_middle_left_begin_left_stance");

	private static readonly ActionIndexCache act_pickup_middle_left_end_left_stance = ActionIndexCache.Create("act_pickup_middle_left_end_left_stance");

	private static readonly ActionIndexCache act_pickup_up_begin = ActionIndexCache.Create("act_pickup_up_begin");

	private static readonly ActionIndexCache act_pickup_up_end = ActionIndexCache.Create("act_pickup_up_end");

	private static readonly ActionIndexCache act_pickup_up_begin_left_stance = ActionIndexCache.Create("act_pickup_up_begin_left_stance");

	private static readonly ActionIndexCache act_pickup_up_end_left_stance = ActionIndexCache.Create("act_pickup_up_end_left_stance");

	private static readonly ActionIndexCache act_pickup_up_left_begin = ActionIndexCache.Create("act_pickup_up_left_begin");

	private static readonly ActionIndexCache act_pickup_up_left_end = ActionIndexCache.Create("act_pickup_up_left_end");

	private static readonly ActionIndexCache act_pickup_up_left_begin_left_stance = ActionIndexCache.Create("act_pickup_up_left_begin_left_stance");

	private static readonly ActionIndexCache act_pickup_up_left_end_left_stance = ActionIndexCache.Create("act_pickup_up_left_end_left_stance");

	private static readonly ActionIndexCache act_pickup_from_right_down_horseback_begin = ActionIndexCache.Create("act_pickup_from_right_down_horseback_begin");

	private static readonly ActionIndexCache act_pickup_from_right_down_horseback_end = ActionIndexCache.Create("act_pickup_from_right_down_horseback_end");

	private static readonly ActionIndexCache act_pickup_from_right_down_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_right_down_horseback_left_begin");

	private static readonly ActionIndexCache act_pickup_from_right_down_horseback_left_end = ActionIndexCache.Create("act_pickup_from_right_down_horseback_left_end");

	private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_begin = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_begin");

	private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_end = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_end");

	private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_left_begin");

	private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_left_end = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_left_end");

	private static readonly ActionIndexCache act_pickup_from_right_up_horseback_begin = ActionIndexCache.Create("act_pickup_from_right_up_horseback_begin");

	private static readonly ActionIndexCache act_pickup_from_right_up_horseback_end = ActionIndexCache.Create("act_pickup_from_right_up_horseback_end");

	private static readonly ActionIndexCache act_pickup_from_right_up_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_right_up_horseback_left_begin");

	private static readonly ActionIndexCache act_pickup_from_right_up_horseback_left_end = ActionIndexCache.Create("act_pickup_from_right_up_horseback_left_end");

	private static readonly ActionIndexCache act_pickup_from_left_down_horseback_begin = ActionIndexCache.Create("act_pickup_from_left_down_horseback_begin");

	private static readonly ActionIndexCache act_pickup_from_left_down_horseback_end = ActionIndexCache.Create("act_pickup_from_left_down_horseback_end");

	private static readonly ActionIndexCache act_pickup_from_left_down_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_left_down_horseback_left_begin");

	private static readonly ActionIndexCache act_pickup_from_left_down_horseback_left_end = ActionIndexCache.Create("act_pickup_from_left_down_horseback_left_end");

	private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_begin = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_begin");

	private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_end = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_end");

	private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_left_begin");

	private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_left_end = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_left_end");

	private static readonly ActionIndexCache act_pickup_from_left_up_horseback_begin = ActionIndexCache.Create("act_pickup_from_left_up_horseback_begin");

	private static readonly ActionIndexCache act_pickup_from_left_up_horseback_end = ActionIndexCache.Create("act_pickup_from_left_up_horseback_end");

	private static readonly ActionIndexCache act_pickup_from_left_up_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_left_up_horseback_left_begin");

	private static readonly ActionIndexCache act_pickup_from_left_up_horseback_left_end = ActionIndexCache.Create("act_pickup_from_left_up_horseback_left_end");

	private static readonly ActionIndexCache act_pickup_boulder_begin = ActionIndexCache.Create("act_pickup_boulder_begin");

	private static readonly ActionIndexCache act_pickup_boulder_end = ActionIndexCache.Create("act_pickup_boulder_end");

	private MissionWeapon _weapon;

	private bool _hasLifeTime;

	public string WeaponName = "";

	private const float LongLifeTime = 180f;

	private const float DisablePhysicsTime = 10f;

	private const float QuickFadeoutLifeTime = 5f;

	private const float TotalFadeOutInDuration = 0.5f;

	private const float PreventStationaryCheckTime = 1f;

	private Timer _disablePhysicsTimer;

	private bool _physicsStopped;

	private bool _readyToBeDeleted;

	private Timer _deletionTimer;

	private int _usedChannelIndex;

	private ActionIndexCache _progressActionIndex;

	private ActionIndexCache _successActionIndex;

	private float _lastSoundPlayTime;

	private const float MinSoundDelay = 0.333f;

	private SoundEvent _rollingSoundEvent;

	private ClientSyncData _clientSyncData;

	private GameEntity _ownerGameEntity;

	private Vec3 _fakeSimulationVelocity;

	private GameEntity _groundEntityWhenDisabled;

	public MissionWeapon WeaponCopy => _weapon;

	public bool HasLifeTime
	{
		get
		{
			return _hasLifeTime;
		}
		set
		{
			if (_hasLifeTime != value)
			{
				_hasLifeTime = value;
				SetScriptComponentToTickMT(GetTickRequirement());
			}
		}
	}

	private bool PhysicsStopped
	{
		get
		{
			return _physicsStopped;
		}
		set
		{
			if (_physicsStopped != value)
			{
				_physicsStopped = value;
				SetScriptComponentToTickMT(GetTickRequirement());
			}
		}
	}

	public bool IsRemoved => _ownerGameEntity == null;

	protected internal override bool LockUserFrames => false;

	public Mission.WeaponSpawnFlags SpawnFlags { get; private set; }

	public TextObject GetActionMessage(ItemObject weaponToReplaceWith, bool fillUp)
	{
		GameTexts.SetVariable("USE_KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
		MBTextManager.SetTextVariable("KEY", GameTexts.FindText("str_ui_agent_interaction_use"));
		if (weaponToReplaceWith == null)
		{
			MBTextManager.SetTextVariable("ACTION", fillUp ? GameTexts.FindText("str_ui_fill") : GameTexts.FindText("str_ui_equip"));
		}
		else
		{
			MBTextManager.SetTextVariable("ACTION", GameTexts.FindText("str_ui_swap"));
			MBTextManager.SetTextVariable("ITEM_NAME", weaponToReplaceWith.Name);
		}
		return GameTexts.FindText("str_key_action");
	}

	public TextObject GetDescriptionMessage(bool fillUp)
	{
		if (!fillUp)
		{
			return _weapon.GetModifiedItemName();
		}
		return GameTexts.FindText("str_inventory_weapon", ((int)_weapon.CurrentUsageItem.WeaponClass).ToString());
	}

	public void Initialize(MissionWeapon weapon, bool hasLifeTime, Mission.WeaponSpawnFlags spawnFlags, in Vec3 fakeSimulationVelocity)
	{
		_weapon = weapon;
		HasLifeTime = hasLifeTime;
		SpawnFlags = spawnFlags;
		_fakeSimulationVelocity = fakeSimulationVelocity;
		if (HasLifeTime)
		{
			float duration = 0f;
			if (!_weapon.IsEmpty)
			{
				duration = (_weapon.Item.ItemFlags.HasAnyFlag(ItemFlags.QuickFadeOut) ? 5f : 180f);
				base.IsDeactivated = _weapon.Item.ItemFlags.HasAnyFlag(ItemFlags.CannotBePickedUp);
				if (_weapon.CurrentUsageItem.WeaponFlags.HasAllFlags(WeaponFlags.RangedWeapon | WeaponFlags.NotUsableWithOneHand | WeaponFlags.Consumable))
				{
					_lastSoundPlayTime = 0.333f;
				}
				else
				{
					_lastSoundPlayTime = -0.333f;
				}
			}
			else
			{
				base.IsDeactivated = true;
			}
			_deletionTimer = new Timer(Mission.Current.CurrentTime, duration);
		}
		else
		{
			_deletionTimer = new Timer(Mission.Current.CurrentTime, float.MaxValue);
		}
		if (spawnFlags.HasAnyFlag(Mission.WeaponSpawnFlags.WithPhysics))
		{
			_disablePhysicsTimer = new Timer(Mission.Current.CurrentTime, 10f);
		}
	}

	protected internal override void OnInit()
	{
		base.OnInit();
		_ownerGameEntity = base.GameEntity;
		if (!string.IsNullOrEmpty(WeaponName))
		{
			ItemObject @object = Game.Current.ObjectManager.GetObject<ItemObject>(WeaponName);
			_weapon = new MissionWeapon(@object, null, null);
		}
		SetScriptComponentToTick(GetTickRequirement());
	}

	public override TickRequirement GetTickRequirement()
	{
		if (GameNetwork.IsClientOrReplay || base.HasUser || !PhysicsStopped)
		{
			return base.GetTickRequirement() | TickRequirement.Tick | TickRequirement.TickParallel2;
		}
		if (HasLifeTime)
		{
			if (!base.GetTickRequirement().HasAnyFlag(TickRequirement.Tick | TickRequirement.TickParallel2))
			{
				return base.GetTickRequirement() | TickRequirement.TickOccasionally;
			}
			return base.GetTickRequirement() | TickRequirement.Tick | TickRequirement.TickParallel2;
		}
		return base.GetTickRequirement();
	}

	protected internal override void OnTick(float dt)
	{
		base.OnTick(dt);
		if (!GameNetwork.IsClientOrReplay || _clientSyncData == null)
		{
			return;
		}
		if (_clientSyncData.Timer.Check(Mission.Current.CurrentTime))
		{
			_ownerGameEntity.SetAlpha(1f);
			_clientSyncData = null;
			return;
		}
		float duration = _clientSyncData.Timer.Duration;
		float num = MBMath.ClampFloat(_clientSyncData.Timer.ElapsedTime() / duration, 0f, 1f);
		if (num < (1f - 0.1f / duration) * 0.5f)
		{
			_ownerGameEntity.SetAlpha(1f - num * 2f);
		}
		else if (num < (1f + 0.1f / duration) * 0.5f)
		{
			_ownerGameEntity.SetAlpha(0f);
			_ownerGameEntity.SetGlobalFrame(in _clientSyncData.Frame);
			_clientSyncData.Parent?.AddChild(_ownerGameEntity, autoLocalizeFrame: true);
			_clientSyncData.Timer.Reset(Mission.Current.CurrentTime - duration * (1f + 0.1f / duration) * 0.5f);
		}
		else
		{
			_ownerGameEntity.SetAlpha(num * 2f - 1f);
		}
	}

	protected internal override void OnTickParallel2(float dt)
	{
		base.OnTickParallel2(dt);
		if (GameNetwork.IsClientOrReplay)
		{
			return;
		}
		if (base.HasUser)
		{
			ActionIndexValueCache currentActionValue = base.UserAgent.GetCurrentActionValue(_usedChannelIndex);
			if (currentActionValue == _successActionIndex)
			{
				base.UserAgent.StopUsingGameObjectMT(base.UserAgent.CanUseObject(this));
			}
			else if (currentActionValue != _progressActionIndex)
			{
				base.UserAgent.StopUsingGameObjectMT(isSuccessful: false);
			}
		}
		else if (HasLifeTime && _deletionTimer.Check(Mission.Current.CurrentTime))
		{
			_readyToBeDeleted = true;
		}
		if (PhysicsStopped)
		{
			return;
		}
		if (_ownerGameEntity != null)
		{
			if (_weapon.IsBanner())
			{
				MatrixFrame frame = _ownerGameEntity.GetGlobalFrame();
				_fakeSimulationVelocity.z -= dt * 9.8f;
				frame.origin += _fakeSimulationVelocity * dt;
				_ownerGameEntity.SetGlobalFrameMT(in frame);
				using (new TWSharedMutexReadLock(Scene.PhysicsAndRayCastLock))
				{
					if (_ownerGameEntity.Scene.GetGroundHeightAtPositionMT(frame.origin) > frame.origin.z + 0.3f)
					{
						PhysicsStopped = true;
					}
					return;
				}
			}
			Vec3 globalPosition = _ownerGameEntity.GlobalPosition;
			if (globalPosition.z <= CompressionBasic.PositionCompressionInfo.GetMinimumValue() + 5f)
			{
				_readyToBeDeleted = true;
			}
			if (_ownerGameEntity.BodyFlag.HasAnyFlag(BodyFlags.Dynamic))
			{
				MatrixFrame frame2 = _ownerGameEntity.GetGlobalFrame();
				if (!frame2.rotation.IsUnit())
				{
					frame2.rotation.Orthonormalize();
					_ownerGameEntity.SetGlobalFrame(in frame2);
				}
				bool flag = _disablePhysicsTimer.Check(Mission.Current.CurrentTime);
				if (flag || _disablePhysicsTimer.ElapsedTime() > 1f)
				{
					bool flag2;
					using (new TWSharedMutexUpgradeableReadLock(Scene.PhysicsAndRayCastLock))
					{
						flag2 = flag || _ownerGameEntity.IsDynamicBodyStationaryMT();
						if (flag2)
						{
							_groundEntityWhenDisabled = TryFindProperGroundEntityForSpawnedEntity();
							if (_groundEntityWhenDisabled != null)
							{
								_groundEntityWhenDisabled.AddChild(base.GameEntity, autoLocalizeFrame: true);
							}
							using (new TWSharedMutexWriteLock(Scene.PhysicsAndRayCastLock))
							{
								if (!_weapon.IsEmpty && !_ownerGameEntity.BodyFlag.HasAnyFlag(BodyFlags.Disabled))
								{
									_ownerGameEntity.DisableDynamicBodySimulationMT();
								}
								else
								{
									_ownerGameEntity.RemovePhysicsMT();
								}
							}
						}
					}
					if (flag2)
					{
						ClampEntityPositionForStoppingIfNeeded();
						PhysicsStopped = true;
						if ((!base.IsDeactivated || _groundEntityWhenDisabled != null) && !_weapon.IsEmpty && GameNetwork.IsServerOrRecorder)
						{
							GameNetwork.BeginBroadcastModuleEvent();
							GameNetwork.WriteMessage(new StopPhysicsAndSetFrameOfMissionObject(base.Id, _groundEntityWhenDisabled?.GetFirstScriptOfType<MissionObject>().Id ?? MissionObjectId.Invalid, _ownerGameEntity.GetFrame()));
							GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
						}
					}
				}
				if (PhysicsStopped || !(_disablePhysicsTimer.ElapsedTime() > 0.2f))
				{
					return;
				}
				_ownerGameEntity.GetPhysicsMinMax(includeChildren: true, out var bbmin, out var bbmax, returnLocal: true);
				MatrixFrame globalFrame = _ownerGameEntity.GetGlobalFrame();
				MatrixFrame previousGlobalFrame = _ownerGameEntity.GetPreviousGlobalFrame();
				Vec3 v = globalFrame.TransformToParent(bbmin);
				Vec3 v2 = previousGlobalFrame.TransformToParent(bbmin);
				Vec3 v3 = globalFrame.TransformToParent(bbmax);
				Vec3 v4 = previousGlobalFrame.TransformToParent(bbmax);
				Vec3 vec = Vec3.Vec3Min(v, v3);
				Vec3 vec2 = Vec3.Vec3Min(v2, v4);
				float waterLevelAtPositionMT;
				using (new TWSharedMutexReadLock(Scene.PhysicsAndRayCastLock))
				{
					waterLevelAtPositionMT = Mission.Current.GetWaterLevelAtPositionMT(vec.AsVec2);
				}
				bool flag3 = vec.z < waterLevelAtPositionMT;
				if (!(vec2.z < waterLevelAtPositionMT) && flag3)
				{
					Vec3 linearVelocityMT;
					using (new TWSharedMutexReadLock(Scene.PhysicsAndRayCastLock))
					{
						linearVelocityMT = _ownerGameEntity.GetLinearVelocityMT();
					}
					float num = _ownerGameEntity.Mass * linearVelocityMT.Length;
					if (num > 0f)
					{
						num *= 0.0625f;
						num = TaleWorlds.Library.MathF.Min(num, 1f);
						Vec3 position = globalPosition;
						position.z = waterLevelAtPositionMT;
						SoundEventParameter parameter = new SoundEventParameter("Size", num);
						Mission.Current.MakeSound(ItemPhysicsSoundContainer.SoundCodePhysicsWater, position, soundCanBePredicted: false, isReliable: true, -1, -1, ref parameter);
					}
				}
			}
			else
			{
				PhysicsStopped = true;
			}
		}
		else
		{
			PhysicsStopped = true;
		}
	}

	private GameEntity TryFindProperGroundEntityForSpawnedEntity()
	{
		_ownerGameEntity.GetPhysicsMinMax(includeChildren: true, out var bbmin, out var bbmax, returnLocal: false);
		float num = bbmax.z - bbmin.z;
		bbmin.z = bbmax.z - 0.001f;
		Vec3 vec = (bbmax + bbmin) * 0.5f;
		_ownerGameEntity.Scene.RayCastForClosestEntityOrTerrainMT(vec, vec - new Vec3(0f, 0f, num + 0.05f), out var collisionDistance, out var closestPoint, out _groundEntityWhenDisabled, 0.01f, BodyFlags.CommonCollisionExcludeFlags);
		_groundEntityWhenDisabled = _groundEntityWhenDisabled?.GetFirstScriptOfTypeInFamily<MissionObject>()?.GameEntity;
		if (TaleWorlds.Library.MathF.Abs(closestPoint.z - vec.z) <= num + 0.05f)
		{
			if (_groundEntityWhenDisabled != null)
			{
				return _groundEntityWhenDisabled;
			}
			return null;
		}
		_ownerGameEntity.Scene.BoxCast(bbmin, bbmax, castSupportRay: false, Vec3.Zero, -Vec3.Up, num + 0.05f, out collisionDistance, out closestPoint, out _groundEntityWhenDisabled, BodyFlags.CommonCollisionExcludeFlags);
		_groundEntityWhenDisabled = _groundEntityWhenDisabled?.GetFirstScriptOfTypeInFamily<MissionObject>()?.GameEntity;
		if (_groundEntityWhenDisabled != null && TaleWorlds.Library.MathF.Abs(closestPoint.z - vec.z) <= num + 0.05f)
		{
			return _groundEntityWhenDisabled;
		}
		return null;
	}

	protected internal override void OnTickOccasionally(float currentFrameDeltaTime)
	{
		OnTickParallel2(currentFrameDeltaTime);
	}

	private void ClampEntityPositionForStoppingIfNeeded()
	{
		GameEntity gameEntity = base.GameEntity;
		float minimumValue = CompressionBasic.PositionCompressionInfo.GetMinimumValue();
		float maximumValue = CompressionBasic.PositionCompressionInfo.GetMaximumValue();
		bool valueClamped;
		Vec3 localPosition = gameEntity.GetFrame().origin.ClampedCopy(minimumValue, maximumValue, out valueClamped);
		if (valueClamped)
		{
			gameEntity.SetLocalPosition(localPosition);
		}
	}

	protected internal override void OnPreInit()
	{
		base.OnPreInit();
		if (base.CreatedAtRuntime)
		{
			Mission.Current.AddSpawnedItemEntityCreatedAtRuntime(this);
		}
	}

	protected override void OnRemoved(int removeReason)
	{
		if (base.HasUser && !GameNetwork.IsClientOrReplay)
		{
			base.UserAgent.StopUsingGameObject(isSuccessful: false);
		}
		base.OnRemoved(removeReason);
		InvalidateWeakPointersIfValid();
		_ownerGameEntity = null;
		base.UserAgent?.OnItemRemovedFromScene();
		MovingAgent?.OnItemRemovedFromScene();
	}

	public void AttachWeaponToWeapon(MissionWeapon attachedWeapon, ref MatrixFrame attachLocalFrame)
	{
		_weapon.AttachWeapon(attachedWeapon, ref attachLocalFrame);
	}

	public bool IsReadyToBeDeleted()
	{
		if ((base.HasUser || !_readyToBeDeleted) && (!(_groundEntityWhenDisabled != null) || _groundEntityWhenDisabled.HasScene()))
		{
			if (_groundEntityWhenDisabled != null && !_groundEntityWhenDisabled.IsVisibleIncludeParents())
			{
				if (_groundEntityWhenDisabled.HasBody())
				{
					return _groundEntityWhenDisabled.BodyFlag.HasAnyFlag(BodyFlags.Disabled);
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
	{
		base.OnUseStopped(userAgent, isSuccessful, preferenceIndex);
		if (isSuccessful)
		{
			if (_clientSyncData != null)
			{
				_clientSyncData = null;
				base.GameEntity.SetAlpha(1f);
			}
			userAgent.OnItemPickup(this, (EquipmentIndex)preferenceIndex, out var removeWeapon);
			if (removeWeapon)
			{
				_readyToBeDeleted = true;
				PhysicsStopped = true;
				base.IsDeactivated = true;
			}
		}
	}

	public override void OnUse(Agent userAgent)
	{
		base.OnUse(userAgent);
		if (GameNetwork.IsClientOrReplay)
		{
			return;
		}
		MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
		float z = globalFrame.origin.z;
		z = Math.Max(z, globalFrame.origin.z + globalFrame.rotation.u.z * (float)_weapon.CurrentUsageItem.WeaponLength * 0.0075f);
		float eyeGlobalHeight = userAgent.GetEyeGlobalHeight();
		bool isLeftStance = userAgent.GetIsLeftStance();
		ItemObject.ItemTypeEnum itemType = _weapon.Item.ItemType;
		if (userAgent.HasMount)
		{
			_usedChannelIndex = 1;
			MatrixFrame frame = userAgent.Frame;
			bool flag = Vec2.DotProduct(frame.rotation.f.AsVec2.LeftVec(), (base.GameEntity.GetGlobalFrame().origin - frame.origin).AsVec2) > 0f;
			if (z < eyeGlobalHeight * 0.7f + userAgent.Position.z)
			{
				if (_weapon.Item.ItemFlags.HasAnyFlag(ItemFlags.AttachmentMask) || itemType == ItemObject.ItemTypeEnum.Bow || itemType == ItemObject.ItemTypeEnum.Shield)
				{
					_progressActionIndex = (flag ? act_pickup_from_left_down_horseback_left_begin : act_pickup_from_right_down_horseback_left_begin);
					_successActionIndex = (flag ? act_pickup_from_left_down_horseback_left_end : act_pickup_from_right_down_horseback_left_end);
				}
				else
				{
					_progressActionIndex = (flag ? act_pickup_from_left_down_horseback_begin : act_pickup_from_right_down_horseback_begin);
					_successActionIndex = (flag ? act_pickup_from_left_down_horseback_end : act_pickup_from_right_down_horseback_end);
				}
			}
			else if (z < eyeGlobalHeight * 1.1f + userAgent.Position.z)
			{
				if (_weapon.Item.ItemFlags.HasAnyFlag(ItemFlags.AttachmentMask) || itemType == ItemObject.ItemTypeEnum.Bow || itemType == ItemObject.ItemTypeEnum.Shield)
				{
					_progressActionIndex = (flag ? act_pickup_from_left_middle_horseback_left_begin : act_pickup_from_right_middle_horseback_left_begin);
					_successActionIndex = (flag ? act_pickup_from_left_middle_horseback_left_end : act_pickup_from_right_middle_horseback_left_end);
				}
				else
				{
					_progressActionIndex = (flag ? act_pickup_from_left_middle_horseback_begin : act_pickup_from_right_middle_horseback_begin);
					_successActionIndex = (flag ? act_pickup_from_left_middle_horseback_end : act_pickup_from_right_middle_horseback_end);
				}
			}
			else if (_weapon.Item.ItemFlags.HasAnyFlag(ItemFlags.AttachmentMask) || itemType == ItemObject.ItemTypeEnum.Bow || itemType == ItemObject.ItemTypeEnum.Shield)
			{
				_progressActionIndex = (flag ? act_pickup_from_left_up_horseback_left_begin : act_pickup_from_right_up_horseback_left_begin);
				_successActionIndex = (flag ? act_pickup_from_left_up_horseback_left_end : act_pickup_from_right_up_horseback_left_end);
			}
			else
			{
				_progressActionIndex = (flag ? act_pickup_from_left_up_horseback_begin : act_pickup_from_right_up_horseback_begin);
				_successActionIndex = (flag ? act_pickup_from_left_up_horseback_end : act_pickup_from_right_up_horseback_end);
			}
		}
		else if (_weapon.CurrentUsageItem.WeaponFlags.HasAllFlags(WeaponFlags.RangedWeapon | WeaponFlags.NotUsableWithOneHand | WeaponFlags.Consumable))
		{
			_usedChannelIndex = 0;
			_progressActionIndex = act_pickup_boulder_begin;
			_successActionIndex = act_pickup_boulder_end;
		}
		else if (z < eyeGlobalHeight * 0.4f + userAgent.Position.z)
		{
			_usedChannelIndex = 0;
			if (_weapon.Item.ItemFlags.HasAnyFlag(ItemFlags.AttachmentMask) || itemType == ItemObject.ItemTypeEnum.Bow || itemType == ItemObject.ItemTypeEnum.Shield)
			{
				_progressActionIndex = (isLeftStance ? act_pickup_down_left_begin_left_stance : act_pickup_down_left_begin);
				_successActionIndex = (isLeftStance ? act_pickup_down_left_end_left_stance : act_pickup_down_left_end);
			}
			else
			{
				_progressActionIndex = (isLeftStance ? act_pickup_down_begin_left_stance : act_pickup_down_begin);
				_successActionIndex = (isLeftStance ? act_pickup_down_end_left_stance : act_pickup_down_end);
			}
		}
		else if (z < eyeGlobalHeight * 1.1f + userAgent.Position.z)
		{
			_usedChannelIndex = 1;
			if (_weapon.Item.ItemFlags.HasAnyFlag(ItemFlags.AttachmentMask) || itemType == ItemObject.ItemTypeEnum.Bow || itemType == ItemObject.ItemTypeEnum.Shield)
			{
				_progressActionIndex = (isLeftStance ? act_pickup_middle_left_begin_left_stance : act_pickup_middle_left_begin);
				_successActionIndex = (isLeftStance ? act_pickup_middle_left_end_left_stance : act_pickup_middle_left_end);
			}
			else
			{
				_progressActionIndex = (isLeftStance ? act_pickup_middle_begin_left_stance : act_pickup_middle_begin);
				_successActionIndex = (isLeftStance ? act_pickup_middle_end_left_stance : act_pickup_middle_end);
			}
		}
		else
		{
			_usedChannelIndex = 1;
			if (_weapon.Item.ItemFlags.HasAnyFlag(ItemFlags.AttachmentMask) || itemType == ItemObject.ItemTypeEnum.Bow || itemType == ItemObject.ItemTypeEnum.Shield)
			{
				_progressActionIndex = (isLeftStance ? act_pickup_up_left_begin_left_stance : act_pickup_up_left_begin);
				_successActionIndex = (isLeftStance ? act_pickup_up_left_end_left_stance : act_pickup_up_left_end);
			}
			else
			{
				_progressActionIndex = (isLeftStance ? act_pickup_up_begin_left_stance : act_pickup_up_begin);
				_successActionIndex = (isLeftStance ? act_pickup_up_end_left_stance : act_pickup_up_end);
			}
		}
		userAgent.SetActionChannel(_usedChannelIndex, _progressActionIndex, ignorePriority: false, 0uL);
	}

	public override bool IsDisabledForAgent(Agent agent)
	{
		if (_weapon.IsAnyConsumable() && _weapon.Amount == 0)
		{
			return true;
		}
		if (_weapon.IsBanner())
		{
			return !MissionGameModels.Current.BattleBannerBearersModel.IsInteractableFormationBanner(this, agent);
		}
		return false;
	}

	protected internal override void OnPhysicsCollision(ref PhysicsContact contact)
	{
		if (GameNetwork.IsDedicatedServer || contact.NumberOfContactPairs <= 0)
		{
			return;
		}
		PhysicsContactInfo physicsContactInfo = default(PhysicsContactInfo);
		bool flag = false;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < contact.NumberOfContactPairs; i++)
		{
			for (int j = 0; j < contact[i].NumberOfContacts; j++)
			{
				if (!flag || contact[i][j].Impulse.LengthSquared > physicsContactInfo.Impulse.LengthSquared)
				{
					physicsContactInfo = contact[i][j];
					flag = true;
				}
			}
			switch (contact[i].ContactEventType)
			{
			case PhysicsEventType.CollisionStart:
				num++;
				break;
			case PhysicsEventType.CollisionStay:
				num2++;
				break;
			case PhysicsEventType.CollisionEnd:
				num3++;
				break;
			default:
				Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Objects\\Usables\\SpawnedItemEntity.cs", "OnPhysicsCollision", 780);
				break;
			}
		}
		if (num2 > 0)
		{
			PlayPhysicsRollSound(physicsContactInfo.Impulse, physicsContactInfo.Position, physicsContactInfo.PhysicsMaterial1);
		}
		else if (num > 0)
		{
			PlayPhysicsCollisionSound(physicsContactInfo.Impulse, physicsContactInfo.PhysicsMaterial1, physicsContactInfo.Position);
		}
	}

	private void PlayPhysicsCollisionSound(Vec3 impulse, PhysicsMaterial collidedMat, Vec3 collisionPoint)
	{
		float num = _deletionTimer.ElapsedTime();
		if (impulse.LengthSquared > 0.0025000002f && _lastSoundPlayTime + 0.333f < num)
		{
			int num2 = -1;
			int num3 = -1;
			int num4 = -1;
			_lastSoundPlayTime = num;
			WeaponClass weaponClass = _weapon.CurrentUsageItem.WeaponClass;
			float length = impulse.Length;
			bool flag = false;
			switch (weaponClass)
			{
			case WeaponClass.Arrow:
			case WeaponClass.Bolt:
				num2 = ItemPhysicsSoundContainer.SoundCodePhysicsArrowlikeDefault;
				num3 = ItemPhysicsSoundContainer.SoundCodePhysicsArrowlikeWood;
				num4 = ItemPhysicsSoundContainer.SoundCodePhysicsArrowlikeStone;
				break;
			case WeaponClass.Bow:
			case WeaponClass.Crossbow:
			case WeaponClass.Javelin:
				num2 = ItemPhysicsSoundContainer.SoundCodePhysicsBowlikeDefault;
				num3 = ItemPhysicsSoundContainer.SoundCodePhysicsBowlikeWood;
				num4 = ItemPhysicsSoundContainer.SoundCodePhysicsBowlikeStone;
				break;
			case WeaponClass.Dagger:
			case WeaponClass.ThrowingAxe:
			case WeaponClass.ThrowingKnife:
				num2 = ItemPhysicsSoundContainer.SoundCodePhysicsDaggerlikeDefault;
				num3 = ItemPhysicsSoundContainer.SoundCodePhysicsDaggerlikeWood;
				num4 = ItemPhysicsSoundContainer.SoundCodePhysicsDaggerlikeStone;
				break;
			case WeaponClass.OneHandedPolearm:
			case WeaponClass.TwoHandedPolearm:
			case WeaponClass.LowGripPolearm:
				num2 = ItemPhysicsSoundContainer.SoundCodePhysicsSpearlikeDefault;
				num3 = ItemPhysicsSoundContainer.SoundCodePhysicsSpearlikeWood;
				num4 = ItemPhysicsSoundContainer.SoundCodePhysicsSpearlikeStone;
				break;
			case WeaponClass.SmallShield:
			case WeaponClass.LargeShield:
				num2 = ItemPhysicsSoundContainer.SoundCodePhysicsShieldlikeDefault;
				num3 = ItemPhysicsSoundContainer.SoundCodePhysicsShieldlikeWood;
				num4 = ItemPhysicsSoundContainer.SoundCodePhysicsShieldlikeStone;
				break;
			case WeaponClass.OneHandedSword:
			case WeaponClass.OneHandedAxe:
			case WeaponClass.Mace:
				num2 = ItemPhysicsSoundContainer.SoundCodePhysicsSwordlikeDefault;
				num3 = ItemPhysicsSoundContainer.SoundCodePhysicsSwordlikeWood;
				num4 = ItemPhysicsSoundContainer.SoundCodePhysicsSwordlikeStone;
				break;
			case WeaponClass.TwoHandedSword:
			case WeaponClass.TwoHandedAxe:
			case WeaponClass.TwoHandedMace:
				num2 = ItemPhysicsSoundContainer.SoundCodePhysicsGreatswordlikeDefault;
				num3 = ItemPhysicsSoundContainer.SoundCodePhysicsGreatswordlikeWood;
				num4 = ItemPhysicsSoundContainer.SoundCodePhysicsGreatswordlikeStone;
				break;
			case WeaponClass.Stone:
				num2 = ItemPhysicsSoundContainer.SoundCodePhysicsSpearlikeDefault;
				num3 = ItemPhysicsSoundContainer.SoundCodePhysicsSpearlikeWood;
				num4 = ItemPhysicsSoundContainer.SoundCodePhysicsSpearlikeStone;
				break;
			case WeaponClass.Boulder:
				num2 = ItemPhysicsSoundContainer.SoundCodePhysicsBoulderDefault;
				num3 = ItemPhysicsSoundContainer.SoundCodePhysicsBoulderWood;
				num4 = ItemPhysicsSoundContainer.SoundCodePhysicsBoulderStone;
				flag = true;
				break;
			default:
				num2 = ItemPhysicsSoundContainer.SoundCodePhysicsSpearlikeDefault;
				num3 = ItemPhysicsSoundContainer.SoundCodePhysicsSpearlikeWood;
				num4 = ItemPhysicsSoundContainer.SoundCodePhysicsSpearlikeStone;
				break;
			}
			if (!flag)
			{
				length *= 1f / 6f;
				length = MBMath.ClampFloat(length, 0f, 1f);
			}
			else
			{
				length = (length - 7f) * (1f / 33f) * 0.1f + 0.9f;
				length = MBMath.ClampFloat(length, 0.9f, 1f);
			}
			string name = collidedMat.Name;
			int soundIndex = num2;
			if (name.Contains("wood"))
			{
				soundIndex = num3;
			}
			else if (name.Contains("stone"))
			{
				soundIndex = num4;
			}
			SoundEventParameter parameter = new SoundEventParameter("Force", length);
			Mission.Current.MakeSound(soundIndex, collisionPoint, soundCanBePredicted: true, isReliable: false, -1, -1, ref parameter);
		}
	}

	private void PlayPhysicsRollSound(Vec3 impulse, Vec3 collisionPoint, PhysicsMaterial collidedMat)
	{
		WeaponComponentData currentUsageItem = _weapon.CurrentUsageItem;
		if (currentUsageItem.WeaponClass != WeaponClass.Boulder || !currentUsageItem.WeaponFlags.HasAllFlags(WeaponFlags.RangedWeapon | WeaponFlags.NotUsableWithOneHand | WeaponFlags.Consumable))
		{
			return;
		}
		float num = _deletionTimer.ElapsedTime();
		if (!(impulse.LengthSquared > 0.0001f) || !(_lastSoundPlayTime + 0.333f < num))
		{
			return;
		}
		if (_rollingSoundEvent == null || !_rollingSoundEvent.IsValid)
		{
			_lastSoundPlayTime = num;
			int soundCodeId = ItemPhysicsSoundContainer.SoundCodePhysicsBoulderDefault;
			string name = collidedMat.Name;
			if (name.Contains("stone"))
			{
				soundCodeId = ItemPhysicsSoundContainer.SoundCodePhysicsBoulderStone;
			}
			else if (name.Contains("wood"))
			{
				soundCodeId = ItemPhysicsSoundContainer.SoundCodePhysicsBoulderWood;
			}
			_rollingSoundEvent = SoundEvent.CreateEvent(soundCodeId, Mission.Current.Scene);
			_rollingSoundEvent.PlayInPosition(collisionPoint);
		}
		float value = impulse.Length * (1f / 30f);
		value = MBMath.ClampFloat(value, 0f, 1f);
		_rollingSoundEvent.SetParameter("Force", value);
		_rollingSoundEvent.SetPosition(collisionPoint);
	}

	public bool IsStuckMissile()
	{
		return SpawnFlags.HasAnyFlag(Mission.WeaponSpawnFlags.AsMissile);
	}

	public bool IsQuiverAndNotEmpty()
	{
		if (_weapon.Item.PrimaryWeapon.IsConsumable)
		{
			return _weapon.Amount > 0;
		}
		return false;
	}

	public bool IsBanner()
	{
		return _weapon.IsBanner();
	}

	public override TextObject GetInfoTextForBeingNotInteractable(Agent userAgent)
	{
		if (!base.IsDeactivated && _weapon.IsAnyConsumable() && _weapon.Amount == 0)
		{
			return GameTexts.FindText("str_ui_empty_quiver");
		}
		return base.GetInfoTextForBeingNotInteractable(userAgent);
	}

	public void StopPhysicsAndSetFrameForClient(MatrixFrame frame, GameEntity parent)
	{
		if (parent != null)
		{
			frame = parent.GetGlobalFrame().TransformToParent(frame);
		}
		frame.rotation.Orthonormalize();
		_clientSyncData = new ClientSyncData();
		_clientSyncData.Frame = frame;
		_clientSyncData.Timer = new Timer(Mission.Current.CurrentTime, 0.5f, autoReset: false);
		_clientSyncData.Parent = parent;
		if (!PhysicsStopped)
		{
			PhysicsStopped = true;
			GameEntity gameEntity = base.GameEntity;
			if (!_weapon.IsEmpty && !gameEntity.BodyFlag.HasAnyFlag(BodyFlags.Disabled))
			{
				gameEntity.DisableDynamicBodySimulation();
			}
			else
			{
				gameEntity.RemovePhysics();
			}
		}
	}

	public void ConsumeWeaponAmount(short consumedAmount)
	{
		_weapon.Consume(consumedAmount);
	}

	public override string GetDescriptionText(GameEntity gameEntity = null)
	{
		return string.Empty;
	}

	public void RequestDeletionOnNextTick()
	{
		_deletionTimer = new Timer(Mission.Current.CurrentTime, -1f);
	}
}
