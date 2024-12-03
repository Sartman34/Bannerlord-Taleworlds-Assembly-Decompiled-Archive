using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade.Network;

namespace TaleWorlds.MountAndBlade;

public sealed class Mission : DotNetObject, IMission
{
	public class MBBoundaryCollection : IDictionary<string, ICollection<Vec2>>, ICollection<KeyValuePair<string, ICollection<Vec2>>>, IEnumerable<KeyValuePair<string, ICollection<Vec2>>>, IEnumerable, INotifyCollectionChanged
	{
		private readonly Mission _mission;

		public int Count => MBAPI.IMBMission.GetBoundaryCount(_mission.Pointer);

		public bool IsReadOnly => false;

		public ICollection<string> Keys
		{
			get
			{
				List<string> list = new List<string>();
				using IEnumerator<KeyValuePair<string, ICollection<Vec2>>> enumerator = GetEnumerator();
				while (enumerator.MoveNext())
				{
					list.Add(enumerator.Current.Key);
				}
				return list;
			}
		}

		public ICollection<ICollection<Vec2>> Values
		{
			get
			{
				List<ICollection<Vec2>> list = new List<ICollection<Vec2>>();
				using IEnumerator<KeyValuePair<string, ICollection<Vec2>>> enumerator = GetEnumerator();
				while (enumerator.MoveNext())
				{
					list.Add(enumerator.Current.Value);
				}
				return list;
			}
		}

		public ICollection<Vec2> this[string name]
		{
			get
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				List<Vec2> boundaryPoints = GetBoundaryPoints(name);
				if (boundaryPoints.Count == 0)
				{
					throw new KeyNotFoundException();
				}
				return boundaryPoints;
			}
			set
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				Add(name, value);
			}
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		IEnumerator IEnumerable.GetEnumerator()
		{
			int count = Count;
			for (int i = 0; i < count; i++)
			{
				string boundaryName = MBAPI.IMBMission.GetBoundaryName(_mission.Pointer, i);
				List<Vec2> boundaryPoints = GetBoundaryPoints(boundaryName);
				yield return new KeyValuePair<string, ICollection<Vec2>>(boundaryName, boundaryPoints);
			}
		}

		public IEnumerator<KeyValuePair<string, ICollection<Vec2>>> GetEnumerator()
		{
			int count = Count;
			for (int i = 0; i < count; i++)
			{
				string boundaryName = MBAPI.IMBMission.GetBoundaryName(_mission.Pointer, i);
				List<Vec2> boundaryPoints = GetBoundaryPoints(boundaryName);
				yield return new KeyValuePair<string, ICollection<Vec2>>(boundaryName, boundaryPoints);
			}
		}

		public float GetBoundaryRadius(string name)
		{
			return MBAPI.IMBMission.GetBoundaryRadius(_mission.Pointer, name);
		}

		public void GetOrientedBoundariesBox(out Vec2 boxMinimum, out Vec2 boxMaximum, float rotationInRadians = 0f)
		{
			Vec2 side = Vec2.Side;
			side.RotateCCW(rotationInRadians);
			Vec2 vb = side.LeftVec();
			boxMinimum = new Vec2(float.MaxValue, float.MaxValue);
			boxMaximum = new Vec2(float.MinValue, float.MinValue);
			foreach (ICollection<Vec2> value in Values)
			{
				foreach (Vec2 item in value)
				{
					float num = Vec2.DotProduct(item, side);
					float num2 = Vec2.DotProduct(item, vb);
					boxMinimum.x = ((num < boxMinimum.x) ? num : boxMinimum.x);
					boxMinimum.y = ((num2 < boxMinimum.y) ? num2 : boxMinimum.y);
					boxMaximum.x = ((num > boxMaximum.x) ? num : boxMaximum.x);
					boxMaximum.y = ((num2 > boxMaximum.y) ? num2 : boxMaximum.y);
				}
			}
		}

		internal MBBoundaryCollection(Mission mission)
		{
			_mission = mission;
		}

		public void Add(KeyValuePair<string, ICollection<Vec2>> item)
		{
			Add(item.Key, item.Value);
		}

		public void Clear()
		{
			using IEnumerator<KeyValuePair<string, ICollection<Vec2>>> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				Remove(enumerator.Current.Key);
			}
		}

		public bool Contains(KeyValuePair<string, ICollection<Vec2>> item)
		{
			return ContainsKey(item.Key);
		}

		public void CopyTo(KeyValuePair<string, ICollection<Vec2>>[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			using IEnumerator<KeyValuePair<string, ICollection<Vec2>>> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, ICollection<Vec2>> current = enumerator.Current;
				array[arrayIndex] = current;
				arrayIndex++;
				if (arrayIndex >= array.Length)
				{
					throw new ArgumentException("Not enough size in array.");
				}
			}
		}

		public bool Remove(KeyValuePair<string, ICollection<Vec2>> item)
		{
			return Remove(item.Key);
		}

		public void Add(string name, ICollection<Vec2> points)
		{
			Add(name, points, isAllowanceInside: true);
		}

		public void Add(string name, ICollection<Vec2> points, bool isAllowanceInside)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (points == null)
			{
				throw new ArgumentNullException("points");
			}
			if (points.Count < 3)
			{
				throw new ArgumentException("At least three points are required.");
			}
			bool num = MBAPI.IMBMission.AddBoundary(_mission.Pointer, name, points.ToArray(), points.Count, isAllowanceInside);
			if (!num)
			{
				throw new ArgumentException("An element with the same name already exists.");
			}
			if (num)
			{
				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, name));
			}
			foreach (Team team in Current.Teams)
			{
				foreach (Formation item in team.FormationsIncludingSpecialAndEmpty)
				{
					item.ResetMovementOrderPositionCache();
				}
			}
		}

		public bool ContainsKey(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return GetBoundaryPoints(name).Count > 0;
		}

		public bool Remove(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			bool flag = MBAPI.IMBMission.RemoveBoundary(_mission.Pointer, name);
			if (flag)
			{
				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, name));
			}
			foreach (Team team in Current.Teams)
			{
				foreach (Formation item in team.FormationsIncludingSpecialAndEmpty)
				{
					item.ResetMovementOrderPositionCache();
				}
			}
			return flag;
		}

		public bool TryGetValue(string name, out ICollection<Vec2> points)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			points = GetBoundaryPoints(name);
			return points.Count > 0;
		}

		private List<Vec2> GetBoundaryPoints(string name)
		{
			List<Vec2> list = new List<Vec2>();
			Vec2[] array = new Vec2[10];
			for (int i = 0; i < 1000; i += 10)
			{
				int retrievedPointCount = -1;
				MBAPI.IMBMission.GetBoundaryPoints(_mission.Pointer, name, i, array, 10, ref retrievedPointCount);
				list.AddRange(array.Take(retrievedPointCount));
				if (retrievedPointCount < 10)
				{
					break;
				}
			}
			return list;
		}
	}

	public class DynamicallyCreatedEntity
	{
		public string Prefab;

		public MissionObjectId ObjectId;

		public MatrixFrame Frame;

		public List<MissionObjectId> ChildObjectIds;

		public DynamicallyCreatedEntity(string prefab, MissionObjectId objectId, MatrixFrame frame, ref List<MissionObjectId> childObjectIds)
		{
			Prefab = prefab;
			ObjectId = objectId;
			Frame = frame;
			ChildObjectIds = childObjectIds;
		}
	}

	[Flags]
	[EngineStruct("Weapon_spawn_flag", false)]
	public enum WeaponSpawnFlags : uint
	{
		None = 0u,
		WithHolster = 1u,
		WithoutHolster = 2u,
		AsMissile = 4u,
		WithPhysics = 8u,
		WithStaticPhysics = 0x10u,
		UseAnimationSpeed = 0x20u,
		CannotBePickedUp = 0x40u
	}

	[EngineStruct("Mission_combat_type", false)]
	public enum MissionCombatType
	{
		Combat,
		ArenaCombat,
		NoCombat
	}

	public enum BattleSizeType
	{
		Battle,
		Siege,
		SallyOut
	}

	[EngineStruct("Agent_creation_result", false)]
	internal struct AgentCreationResult
	{
		internal int Index;

		internal UIntPtr AgentPtr;

		internal UIntPtr PositionPtr;

		internal UIntPtr IndexPtr;

		internal UIntPtr FlagsPtr;

		internal UIntPtr StatePtr;
	}

	public struct TimeSpeedRequest
	{
		public float RequestedTimeSpeed { get; private set; }

		public int RequestID { get; private set; }

		public TimeSpeedRequest(float requestedTime, int requestID)
		{
			RequestedTimeSpeed = requestedTime;
			RequestID = requestID;
		}
	}

	private enum GetNearbyAgentsAuxType
	{
		Friend = 1,
		Enemy,
		All
	}

	public static class MissionNetworkHelper
	{
		public static Agent GetAgentFromIndex(int agentIndex, bool canBeNull = false)
		{
			Agent agent = Current.FindAgentWithIndex(agentIndex);
			if (!canBeNull && agent == null && agentIndex >= 0)
			{
				TaleWorlds.Library.Debug.Print("Agent with index: " + agentIndex + " could not be found while reading reference from packet.");
				throw new MBNotFoundException("Agent with index: " + agentIndex + " could not be found while reading reference from packet.");
			}
			return agent;
		}

		public static MBTeam GetMBTeamFromTeamIndex(int teamIndex)
		{
			if (Current == null)
			{
				throw new Exception("Mission.Current is null!");
			}
			if (teamIndex < 0)
			{
				return MBTeam.InvalidTeam;
			}
			return new MBTeam(Current, teamIndex);
		}

		public static Team GetTeamFromTeamIndex(int teamIndex)
		{
			if (Current == null)
			{
				throw new Exception("Mission.Current is null!");
			}
			if (teamIndex < 0)
			{
				return Team.Invalid;
			}
			MBTeam mBTeamFromTeamIndex = GetMBTeamFromTeamIndex(teamIndex);
			return Current.Teams.Find(mBTeamFromTeamIndex);
		}

		public static MissionObject GetMissionObjectFromMissionObjectId(MissionObjectId missionObjectId)
		{
			if (Current == null)
			{
				throw new Exception("Mission.Current is null!");
			}
			if (missionObjectId.Id < 0)
			{
				return null;
			}
			MissionObject missionObject = Current.MissionObjects.FirstOrDefault((MissionObject mo) => mo.Id == missionObjectId);
			if (missionObject == null)
			{
				object[] obj = new object[5] { "MissionObject with ID: ", missionObjectId.Id, " runtime: ", null, null };
				bool createdAtRuntime = missionObjectId.CreatedAtRuntime;
				obj[3] = createdAtRuntime.ToString();
				obj[4] = " could not be found.";
				MBDebug.Print(string.Concat(obj));
			}
			return missionObject;
		}

		public static CombatLogData GetCombatLogDataForCombatLogNetworkMessage(CombatLogNetworkMessage message)
		{
			if (Current == null)
			{
				throw new Exception("Mission.Current is null!");
			}
			Agent agentFromIndex = GetAgentFromIndex(message.AttackerAgentIndex);
			Agent agentFromIndex2 = GetAgentFromIndex(message.VictimAgentIndex, canBeNull: true);
			bool num = agentFromIndex != null;
			bool isAttackerAgentHuman = num && agentFromIndex.IsHuman;
			bool isAttackerAgentMine = num && agentFromIndex.IsMine;
			bool flag = num && agentFromIndex.RiderAgent != null;
			bool isAttackerAgentRiderAgentMine = flag && agentFromIndex.RiderAgent.IsMine;
			bool isAttackerAgentMount = num && agentFromIndex.IsMount;
			bool isVictimAgentDead = agentFromIndex2 != null && agentFromIndex2.Health <= 0f;
			bool isVictimRiderAgentSameAsAttackerAgent = agentFromIndex != null && agentFromIndex2?.RiderAgent == agentFromIndex;
			CombatLogData result = new CombatLogData(agentFromIndex == agentFromIndex2, isAttackerAgentHuman, isAttackerAgentMine, flag, isAttackerAgentRiderAgentMine, isAttackerAgentMount, agentFromIndex2?.IsHuman ?? false, agentFromIndex2?.IsMine ?? false, isVictimAgentDead, agentFromIndex2?.RiderAgent != null, agentFromIndex2?.RiderAgent?.IsMine ?? false, agentFromIndex2?.IsMount ?? false, message.IsVictimEntity, isVictimRiderAgentSameAsAttackerAgent, message.CrushedThrough, message.Chamber, message.Distance);
			result.DamageType = message.DamageType;
			result.IsRangedAttack = message.IsRangedAttack;
			result.IsFriendlyFire = message.IsFriendlyFire;
			result.IsFatalDamage = message.IsFatalDamage;
			result.BodyPartHit = message.BodyPartHit;
			result.HitSpeed = message.HitSpeed;
			result.InflictedDamage = message.InflictedDamage;
			result.AbsorbedDamage = message.AbsorbedDamage;
			result.ModifiedDamage = message.ModifiedDamage;
			result.VictimAgentName = agentFromIndex2?.MissionPeer?.DisplayedName ?? agentFromIndex2?.Name ?? "";
			return result;
		}
	}

	public class Missile : MBMissile
	{
		public GameEntity Entity { get; private set; }

		public MissionWeapon Weapon { get; set; }

		public Agent ShooterAgent { get; set; }

		public MissionObject MissionObjectToIgnore { get; set; }

		public Missile(Mission mission, GameEntity entity)
			: base(mission)
		{
			Entity = entity;
		}

		public void CalculatePassbySoundParametersMT(ref SoundEventParameter soundEventParameter)
		{
			if (Weapon.CurrentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.CanPenetrateShield))
			{
				soundEventParameter.Update("impactModifier", 0.3f);
			}
		}

		public void CalculateBounceBackVelocity(Vec3 rotationSpeed, AttackCollisionData collisionData, out Vec3 velocity, out Vec3 angularVelocity)
		{
			Vec3 missileVelocity = collisionData.MissileVelocity;
			float num = (float)Weapon.CurrentUsageItem.WeaponLength * 0.01f * Weapon.Item.ScaleFactor;
			PhysicsMaterial fromIndex = PhysicsMaterial.GetFromIndex(collisionData.PhysicsMaterialIndex);
			float num2;
			float num3;
			if (fromIndex.IsValid)
			{
				num2 = fromIndex.GetDynamicFriction();
				num3 = fromIndex.GetRestitution();
			}
			else
			{
				num2 = 0.3f;
				num3 = 0.4f;
			}
			PhysicsMaterial fromName = PhysicsMaterial.GetFromName(Weapon.Item.PrimaryWeapon.PhysicsMaterial);
			float num4;
			float num5;
			if (fromName.IsValid)
			{
				num4 = fromName.GetDynamicFriction();
				num5 = fromName.GetRestitution();
			}
			else
			{
				num4 = 0.3f;
				num5 = 0.4f;
			}
			float num6 = (num2 + num4) * 0.5f;
			float num7 = (num3 + num5) * 0.5f;
			Vec3 vec = missileVelocity.Reflect(collisionData.CollisionGlobalNormal);
			float num8 = Vec3.DotProduct(vec, collisionData.CollisionGlobalNormal);
			Vec3 vec2 = collisionData.CollisionGlobalNormal.RotateAboutAnArbitraryVector(Vec3.CrossProduct(vec, collisionData.CollisionGlobalNormal).NormalizedCopy(), System.MathF.PI / 2f);
			float num9 = Vec3.DotProduct(vec, vec2);
			velocity = collisionData.CollisionGlobalNormal * (num7 * num8) + vec2 * (num9 * num6);
			velocity += collisionData.CollisionGlobalNormal;
			angularVelocity = -Vec3.CrossProduct(collisionData.CollisionGlobalNormal, velocity);
			float lengthSquared = angularVelocity.LengthSquared;
			float weight = Weapon.GetWeight();
			float num10;
			switch (Weapon.CurrentUsageItem.WeaponClass)
			{
			case WeaponClass.Arrow:
			case WeaponClass.Bolt:
				num10 = 0.25f * weight * 0.055f * 0.055f + 0.08333333f * weight * num * num;
				break;
			case WeaponClass.ThrowingKnife:
				num10 = 0.25f * weight * 0.2f * 0.2f + 0.08333333f * weight * num * num;
				num10 += 0.5f * weight * 0.2f * 0.2f;
				_ = rotationSpeed * num3;
				angularVelocity = Entity.GetGlobalFrame().rotation.TransformToParent(rotationSpeed * num3);
				break;
			case WeaponClass.ThrowingAxe:
				num10 = 0.25f * weight * 0.2f * 0.2f + 0.08333333f * weight * num * num;
				num10 += 0.5f * weight * 0.2f * 0.2f;
				_ = rotationSpeed * num3;
				angularVelocity = Entity.GetGlobalFrame().rotation.TransformToParent(rotationSpeed * num3);
				break;
			case WeaponClass.Javelin:
				num10 = 0.25f * weight * 0.155f * 0.155f + 0.08333333f * weight * num * num;
				break;
			case WeaponClass.Stone:
				num10 = 0.4f * weight * 0.1f * 0.1f;
				break;
			case WeaponClass.Boulder:
				num10 = 0.4f * weight * 0.4f * 0.4f;
				break;
			default:
				TaleWorlds.Library.Debug.FailedAssert("Unknown missile type!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", "CalculateBounceBackVelocity", 266);
				num10 = 0f;
				break;
			}
			float num11 = 0.5f * num10 * lengthSquared;
			float length = missileVelocity.Length;
			float num12 = TaleWorlds.Library.MathF.Sqrt((0.5f * weight * length * length - num11) * 2f / weight);
			velocity *= num12 / length;
			float maximumValue = CompressionMission.SpawnedItemVelocityCompressionInfo.GetMaximumValue();
			float maximumValue2 = CompressionMission.SpawnedItemAngularVelocityCompressionInfo.GetMaximumValue();
			if (velocity.LengthSquared > maximumValue * maximumValue)
			{
				velocity = velocity.NormalizedCopy() * maximumValue;
			}
			if (angularVelocity.LengthSquared > maximumValue2 * maximumValue2)
			{
				angularVelocity = angularVelocity.NormalizedCopy() * maximumValue2;
			}
		}
	}

	public struct SpectatorData
	{
		public Agent AgentToFollow { get; private set; }

		public IAgentVisual AgentVisualToFollow { get; private set; }

		public SpectatorCameraTypes CameraType { get; private set; }

		public SpectatorData(Agent agentToFollow, IAgentVisual agentVisualToFollow, SpectatorCameraTypes cameraType)
		{
			AgentToFollow = agentToFollow;
			CameraType = cameraType;
			AgentVisualToFollow = agentVisualToFollow;
		}
	}

	public enum State
	{
		NewlyCreated,
		Initializing,
		Continuing,
		EndingNextFrame,
		Over
	}

	private class DynamicEntityInfo
	{
		public GameEntity Entity;

		public TaleWorlds.Core.Timer TimerToDisable;
	}

	public enum BattleSizeQualifier
	{
		Small,
		Medium
	}

	public enum MissionTeamAITypeEnum
	{
		NoTeamAI,
		FieldBattle,
		Siege,
		SallyOut
	}

	public enum MissileCollisionReaction
	{
		Invalid = -1,
		Stick,
		PassThrough,
		BounceBack,
		BecomeInvisible,
		Count
	}

	public delegate void OnBeforeAgentRemovedDelegate(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow);

	public sealed class TeamCollection : List<Team>
	{
		private Mission _mission;

		private Team _playerTeam;

		public Team Attacker { get; private set; }

		public Team Defender { get; private set; }

		public Team AttackerAlly { get; private set; }

		public Team DefenderAlly { get; private set; }

		public Team Player
		{
			get
			{
				return _playerTeam;
			}
			set
			{
				if (_playerTeam != value)
				{
					SetPlayerTeamAux((value == null) ? (-1) : IndexOf(value));
				}
			}
		}

		public Team PlayerEnemy { get; private set; }

		public Team PlayerAlly { get; private set; }

		private int TeamCountNative => MBAPI.IMBMission.GetNumberOfTeams(_mission.Pointer);

		public event Action<Team, Team> OnPlayerTeamChanged;

		public TeamCollection(Mission mission)
			: base((IEnumerable<Team>)new List<Team>())
		{
			_mission = mission;
		}

		private MBTeam AddNative()
		{
			return new MBTeam(_mission, MBAPI.IMBMission.AddTeam(_mission.Pointer));
		}

		public new void Add(Team t)
		{
			MBDebug.ShowWarning("Pre-created Team can not be added to TeamCollection!");
		}

		public Team Add(BattleSideEnum side, uint color = uint.MaxValue, uint color2 = uint.MaxValue, Banner banner = null, bool isPlayerGeneral = true, bool isPlayerSergeant = false, bool isSettingRelations = true)
		{
			MBDebug.Print("----------Mission-AddTeam-" + side);
			Team team = new Team(AddNative(), side, _mission, color, color2, banner);
			if (!GameNetwork.IsClientOrReplay)
			{
				team.SetPlayerRole(isPlayerGeneral, isPlayerSergeant);
			}
			base.Add(team);
			foreach (MissionBehavior missionBehavior in _mission.MissionBehaviors)
			{
				missionBehavior.OnAddTeam(team);
			}
			if (isSettingRelations)
			{
				SetRelations(team);
			}
			switch (side)
			{
			case BattleSideEnum.Attacker:
				if (Attacker == null)
				{
					Attacker = team;
				}
				else if (AttackerAlly == null)
				{
					AttackerAlly = team;
				}
				break;
			case BattleSideEnum.Defender:
				if (Defender == null)
				{
					Defender = team;
				}
				else if (DefenderAlly == null)
				{
					DefenderAlly = team;
				}
				break;
			}
			AdjustPlayerTeams();
			foreach (MissionBehavior missionBehavior2 in _mission.MissionBehaviors)
			{
				missionBehavior2.AfterAddTeam(team);
			}
			return team;
		}

		public Team Find(MBTeam mbTeam)
		{
			if (mbTeam.IsValid)
			{
				for (int i = 0; i < base.Count; i++)
				{
					Team team = base[i];
					if (team.MBTeam == mbTeam)
					{
						return team;
					}
				}
			}
			return Team.Invalid;
		}

		public void ClearResources()
		{
			Attacker = null;
			AttackerAlly = null;
			Defender = null;
			DefenderAlly = null;
			_playerTeam = null;
			PlayerEnemy = null;
			PlayerAlly = null;
			Team.Invalid = null;
		}

		public new void Clear()
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					enumerator.Current.Clear();
				}
			}
			base.Clear();
			ClearResources();
			MBAPI.IMBMission.ResetTeams(_mission.Pointer);
		}

		private void SetRelations(Team team)
		{
			BattleSideEnum side = team.Side;
			for (int i = 0; i < base.Count; i++)
			{
				Team team2 = base[i];
				if (side.IsOpponentOf(team2.Side))
				{
					team.SetIsEnemyOf(team2, isEnemyOf: true);
				}
			}
		}

		private void SetPlayerTeamAux(int index)
		{
			Team playerTeam = _playerTeam;
			_playerTeam = ((index == -1) ? null : base[index]);
			AdjustPlayerTeams();
			this.OnPlayerTeamChanged?.Invoke(playerTeam, _playerTeam);
		}

		private void AdjustPlayerTeams()
		{
			if (Player == null)
			{
				PlayerEnemy = null;
				PlayerAlly = null;
			}
			else if (Player == Attacker)
			{
				if (Defender != null && Player.IsEnemyOf(Defender))
				{
					PlayerEnemy = Defender;
				}
				else
				{
					PlayerEnemy = null;
				}
				if (AttackerAlly != null && Player.IsFriendOf(AttackerAlly))
				{
					PlayerAlly = AttackerAlly;
				}
				else
				{
					PlayerAlly = null;
				}
			}
			else if (Player == Defender)
			{
				if (Attacker != null && Player.IsEnemyOf(Attacker))
				{
					PlayerEnemy = Attacker;
				}
				else
				{
					PlayerEnemy = null;
				}
				if (DefenderAlly != null && Player.IsFriendOf(DefenderAlly))
				{
					PlayerAlly = DefenderAlly;
				}
				else
				{
					PlayerAlly = null;
				}
			}
		}
	}

	public const int MaxRuntimeMissionObjects = 4095;

	private int _lastSceneMissionObjectIdCount;

	private int _lastRuntimeMissionObjectIdCount;

	private bool _isMainAgentObjectInteractionEnabled = true;

	private List<TimeSpeedRequest> _timeSpeedRequests = new List<TimeSpeedRequest>();

	private bool _isMainAgentItemInteractionEnabled = true;

	private readonly MBList<MissionObject> _activeMissionObjects;

	private readonly MBList<MissionObject> _missionObjects;

	private readonly List<SpawnedItemEntity> _spawnedItemEntitiesCreatedAtRuntime;

	private readonly MBList<DynamicallyCreatedEntity> _addedEntitiesInfo;

	private readonly Stack<(int, float)> _emptyRuntimeMissionObjectIds;

	private static bool _isCameraFirstPerson = false;

	private MissionMode _missionMode;

	private float _cachedMissionTime;

	private static readonly object GetNearbyAgentsAuxLock = new object();

	public const int MaxNavMeshId = 1000000;

	private const float NavigationMeshHeightLimit = 1.5f;

	private const float SpeedBonusFactorForSwing = 0.7f;

	private const float SpeedBonusFactorForThrust = 0.5f;

	private const float _exitTimeInSeconds = 0.6f;

	private const int MaxNavMeshPerDynamicObject = 10;

	private readonly List<DynamicEntityInfo> _dynamicEntities = new List<DynamicEntityInfo>();

	private Dictionary<int, Missile> _missiles;

	private bool _missionEnded;

	public bool DisableDying;

	public bool ForceNoFriendlyFire;

	private int _nextDynamicNavMeshIdStart = 1000010;

	public bool IsFriendlyMission = true;

	public const int MaxDamage = 2000;

	public BasicCultureObject MusicCulture;

	private List<IMissionListener> _listeners = new List<IMissionListener>();

	private MissionState _missionState;

	private MissionDeploymentPlan _deploymentPlan;

	private readonly object _lockHelper = new object();

	private List<MissionBehavior> _otherMissionBehaviors;

	private MBList<Agent> _activeAgents;

	private BasicMissionTimer _leaveMissionTimer;

	private readonly MBList<KeyValuePair<Agent, MissionTime>> _mountsWithoutRiders;

	public bool IsOrderMenuOpen;

	public bool IsTransferMenuOpen;

	public bool IsInPhotoMode;

	private Agent _mainAgent;

	private Action _onLoadingEndedAction;

	private TaleWorlds.Core.Timer _inMissionLoadingScreenTimer;

	public bool AllowAiTicking = true;

	private int _agentCreationIndex;

	private readonly MBList<FleePosition>[] _fleePositions = new MBList<FleePosition>[3];

	private bool _doesMissionRequireCivilianEquipment;

	public IAgentVisualCreator AgentVisualCreator;

	private readonly int[] _initialAgentCountPerSide = new int[2];

	private readonly int[] _removedAgentCountPerSide = new int[2];

	private ConcurrentQueue<CombatLogData> _combatLogsCreated = new ConcurrentQueue<CombatLogData>();

	private MBList<Agent> _allAgents;

	private List<SiegeWeapon> _attackerWeaponsForFriendlyFirePreventing = new List<SiegeWeapon>();

	private float _missionEndTime;

	public float MissionCloseTimeAfterFinish = 30f;

	private static Mission _current = null;

	public float NextCheckTimeEndMission = 10f;

	public int NumOfFormationsSpawnedTeamOne;

	private SoundEvent _ambientSoundEvent;

	private readonly BattleSpawnPathSelector _battleSpawnPathSelector;

	private int _agentCount;

	public int NumOfFormationsSpawnedTeamTwo;

	private bool tickCompleted = true;

	internal UIntPtr Pointer { get; private set; }

	public bool IsFinalized => Pointer == UIntPtr.Zero;

	public static Mission Current
	{
		get
		{
			_ = _current;
			return _current;
		}
		private set
		{
			if (value == null)
			{
				_ = _current;
			}
			_current = value;
		}
	}

	private MissionInitializerRecord InitializerRecord { get; set; }

	public string SceneName => InitializerRecord.SceneName;

	public string SceneLevels => InitializerRecord.SceneLevels;

	public float DamageToPlayerMultiplier => InitializerRecord.DamageToPlayerMultiplier;

	public float DamageToFriendsMultiplier => InitializerRecord.DamageToFriendsMultiplier;

	public float DamageFromPlayerToFriendsMultiplier => InitializerRecord.DamageFromPlayerToFriendsMultiplier;

	public bool HasValidTerrainType => InitializerRecord.TerrainType >= 0;

	public TerrainType TerrainType
	{
		get
		{
			if (!HasValidTerrainType)
			{
				return TerrainType.Water;
			}
			return (TerrainType)InitializerRecord.TerrainType;
		}
	}

	public Scene Scene { get; private set; }

	public MBReadOnlyList<MissionObject> ActiveMissionObjects => _activeMissionObjects;

	public MBReadOnlyList<MissionObject> MissionObjects => _missionObjects;

	public MBReadOnlyList<DynamicallyCreatedEntity> AddedEntitiesInfo => _addedEntitiesInfo;

	public MBBoundaryCollection Boundaries { get; private set; }

	public bool IsMainAgentObjectInteractionEnabled
	{
		get
		{
			switch (_missionMode)
			{
			case MissionMode.Conversation:
			case MissionMode.Barter:
			case MissionMode.Deployment:
			case MissionMode.Replay:
			case MissionMode.CutScene:
				return false;
			default:
				if (!MissionEnded)
				{
					return _isMainAgentObjectInteractionEnabled;
				}
				return false;
			}
		}
		set
		{
			_isMainAgentObjectInteractionEnabled = value;
		}
	}

	public bool IsMainAgentItemInteractionEnabled
	{
		get
		{
			switch (_missionMode)
			{
			case MissionMode.Conversation:
			case MissionMode.Barter:
			case MissionMode.Deployment:
			case MissionMode.Replay:
			case MissionMode.CutScene:
				return false;
			default:
				return _isMainAgentItemInteractionEnabled;
			}
		}
		set
		{
			_isMainAgentItemInteractionEnabled = value;
		}
	}

	public bool IsTeleportingAgents { get; set; }

	public bool ForceTickOccasionally { get; set; }

	public MissionCombatType CombatType
	{
		get
		{
			return (MissionCombatType)MBAPI.IMBMission.GetCombatType(Pointer);
		}
		set
		{
			MBAPI.IMBMission.SetCombatType(Pointer, (int)value);
		}
	}

	public MissionMode Mode => _missionMode;

	public float CurrentTime => _cachedMissionTime;

	public bool PauseAITick
	{
		get
		{
			return MBAPI.IMBMission.GetPauseAITick(Pointer);
		}
		set
		{
			MBAPI.IMBMission.SetPauseAITick(Pointer, value);
		}
	}

	public int EnemyAlarmStateIndicator => MBAPI.IMBMission.GetEnemyAlarmStateIndicator(Pointer);

	public float PlayerAlarmIndicator => MBAPI.IMBMission.GetPlayerAlarmIndicator(Pointer);

	public bool IsLoadingFinished => MBAPI.IMBMission.GetIsLoadingFinished(Pointer);

	public bool CameraIsFirstPerson
	{
		get
		{
			return _isCameraFirstPerson;
		}
		set
		{
			if (_isCameraFirstPerson != value)
			{
				_isCameraFirstPerson = value;
				MBAPI.IMBMission.SetCameraIsFirstPerson(value);
				ResetFirstThirdPersonView();
			}
		}
	}

	public static float CameraAddedDistance
	{
		get
		{
			return BannerlordConfig.CombatCameraDistance;
		}
		set
		{
			if (value != BannerlordConfig.CombatCameraDistance)
			{
				BannerlordConfig.CombatCameraDistance = value;
			}
		}
	}

	public float ClearSceneTimerElapsedTime => MBAPI.IMBMission.GetClearSceneTimerElapsedTime(Pointer);

	public bool MissionEnded
	{
		get
		{
			return _missionEnded;
		}
		private set
		{
			if (!_missionEnded && value)
			{
				MissionIsEnding = true;
				foreach (MissionObject missionObject in MissionObjects)
				{
					missionObject.OnMissionEnded();
				}
				MissionIsEnding = false;
			}
			_missionEnded = value;
		}
	}

	public bool MissionIsEnding { get; private set; }

	public MBReadOnlyList<KeyValuePair<Agent, MissionTime>> MountsWithoutRiders => _mountsWithoutRiders;

	public BattleSideEnum RetreatSide { get; private set; } = BattleSideEnum.None;


	public bool IsFastForward { get; private set; }

	public bool FixedDeltaTimeMode { get; set; }

	public float FixedDeltaTime { get; set; }

	public State CurrentState { get; private set; }

	public TeamCollection Teams { get; private set; }

	public Team AttackerTeam => Teams.Attacker;

	public Team DefenderTeam => Teams.Defender;

	public Team AttackerAllyTeam => Teams.AttackerAlly;

	public Team DefenderAllyTeam => Teams.DefenderAlly;

	public Team PlayerTeam
	{
		get
		{
			return Teams.Player;
		}
		set
		{
			Teams.Player = value;
		}
	}

	public Team PlayerEnemyTeam => Teams.PlayerEnemy;

	public Team PlayerAllyTeam => Teams.PlayerAlly;

	public Team SpectatorTeam { get; set; }

	IMissionTeam IMission.PlayerTeam => PlayerTeam;

	public bool IsMissionEnding
	{
		get
		{
			if (CurrentState != State.Over)
			{
				return MissionEnded;
			}
			return false;
		}
	}

	public List<MissionLogic> MissionLogics { get; }

	public List<MissionBehavior> MissionBehaviors { get; }

	public IEnumerable<Missile> Missiles => _missiles.Values;

	public IInputContext InputManager { get; set; }

	public bool NeedsMemoryCleanup { get; internal set; }

	public Agent MainAgent
	{
		get
		{
			return _mainAgent;
		}
		set
		{
			_mainAgent = value;
			if (this.OnMainAgentChanged != null)
			{
				this.OnMainAgentChanged(this, null);
			}
			if (!GameNetwork.IsClient)
			{
				MainAgentServer = _mainAgent;
			}
		}
	}

	public IMissionDeploymentPlan DeploymentPlan => _deploymentPlan;

	public Agent MainAgentServer { get; set; }

	public bool HasSpawnPath => _battleSpawnPathSelector.IsInitialized;

	public bool IsFieldBattle => MissionTeamAIType == MissionTeamAITypeEnum.FieldBattle;

	public bool IsSiegeBattle => MissionTeamAIType == MissionTeamAITypeEnum.Siege;

	public bool IsSallyOutBattle => MissionTeamAIType == MissionTeamAITypeEnum.SallyOut;

	public MBReadOnlyList<Agent> AllAgents => _allAgents;

	public MBReadOnlyList<Agent> Agents => _activeAgents;

	public bool IsInventoryAccessAllowed
	{
		get
		{
			if (Game.Current.GameType.IsInventoryAccessibleAtMission || _isScreenAccessAllowed)
			{
				return IsInventoryAccessible;
			}
			return false;
		}
	}

	public bool IsInventoryAccessible { private get; set; }

	public MissionResult MissionResult { get; private set; }

	public bool IsQuestScreenAccessible { private get; set; }

	private bool _isScreenAccessAllowed
	{
		get
		{
			if (Mode != MissionMode.Stealth && Mode != MissionMode.Battle && Mode != MissionMode.Deployment && Mode != MissionMode.Duel)
			{
				return Mode != MissionMode.CutScene;
			}
			return false;
		}
	}

	public bool IsQuestScreenAccessAllowed
	{
		get
		{
			if (Game.Current.GameType.IsQuestScreenAccessibleAtMission || _isScreenAccessAllowed)
			{
				return IsQuestScreenAccessible;
			}
			return false;
		}
	}

	public bool IsCharacterWindowAccessible { private get; set; }

	public bool IsCharacterWindowAccessAllowed
	{
		get
		{
			if (Game.Current.GameType.IsCharacterWindowAccessibleAtMission || _isScreenAccessAllowed)
			{
				return IsCharacterWindowAccessible;
			}
			return false;
		}
	}

	public bool IsPartyWindowAccessible { private get; set; }

	public bool IsPartyWindowAccessAllowed
	{
		get
		{
			if (Game.Current.GameType.IsPartyWindowAccessibleAtMission || _isScreenAccessAllowed)
			{
				return IsPartyWindowAccessible;
			}
			return false;
		}
	}

	public bool IsKingdomWindowAccessible { private get; set; }

	public bool IsKingdomWindowAccessAllowed
	{
		get
		{
			if (Game.Current.GameType.IsKingdomWindowAccessibleAtMission || _isScreenAccessAllowed)
			{
				return IsKingdomWindowAccessible;
			}
			return false;
		}
	}

	public bool IsClanWindowAccessible { private get; set; }

	public bool IsClanWindowAccessAllowed
	{
		get
		{
			if (Game.Current.GameType.IsClanWindowAccessibleAtMission && _isScreenAccessAllowed)
			{
				return IsClanWindowAccessible;
			}
			return false;
		}
	}

	public bool IsEncyclopediaWindowAccessible { private get; set; }

	public bool IsEncyclopediaWindowAccessAllowed
	{
		get
		{
			if (Game.Current.GameType.IsEncyclopediaWindowAccessibleAtMission || _isScreenAccessAllowed)
			{
				return IsEncyclopediaWindowAccessible;
			}
			return false;
		}
	}

	public bool IsBannerWindowAccessible { private get; set; }

	public bool IsBannerWindowAccessAllowed
	{
		get
		{
			if (Game.Current.GameType.IsBannerWindowAccessibleAtMission || _isScreenAccessAllowed)
			{
				return IsBannerWindowAccessible;
			}
			return false;
		}
	}

	public bool DoesMissionRequireCivilianEquipment
	{
		get
		{
			return _doesMissionRequireCivilianEquipment;
		}
		set
		{
			_doesMissionRequireCivilianEquipment = value;
		}
	}

	public MissionTeamAITypeEnum MissionTeamAIType { get; set; }

	public MissionTimeTracker MissionTimeTracker { get; private set; }

	private Lazy<MissionRecorder> _recorder => new Lazy<MissionRecorder>(() => new MissionRecorder(this));

	public MissionRecorder Recorder => _recorder.Value;

	public event PropertyChangedEventHandler OnMissionReset;

	public event Func<WorldPosition, Team, bool> IsFormationUnitPositionAvailable_AdditionalCondition;

	public event Func<Agent, bool> CanAgentRout_AdditionalCondition;

	public event Func<Agent, WorldPosition?> GetOverriddenFleePositionForAgent;

	public event Func<bool> IsAgentInteractionAllowed_AdditionalCondition;

	public event Action<Agent, SpawnedItemEntity> OnItemPickUp;

	public event PropertyChangedEventHandler OnMainAgentChanged;

	public event Func<BattleSideEnum, BasicCharacterObject, FormationClass> GetAgentTroopClass_Override;

	public event Action<Agent, SpawnedItemEntity> OnItemDrop;

	public event Func<bool> AreOrderGesturesEnabled_AdditionalCondition;

	public event Func<bool> IsBattleInRetreatEvent;

	public event OnBeforeAgentRemovedDelegate OnBeforeAgentRemoved;

	public IEnumerable<GameEntity> GetActiveEntitiesWithScriptComponentOfType<T>()
	{
		return from amo in _activeMissionObjects
			where amo is T
			select amo.GameEntity;
	}

	public void AddActiveMissionObject(MissionObject missionObject)
	{
		_missionObjects.Add(missionObject);
		_activeMissionObjects.Add(missionObject);
	}

	public void ActivateMissionObject(MissionObject missionObject)
	{
		_activeMissionObjects.Add(missionObject);
	}

	public void DeactivateMissionObject(MissionObject missionObject)
	{
		_activeMissionObjects.Remove(missionObject);
	}

	private void FinalizeMission()
	{
		TeamAISiegeComponent.OnMissionFinalize();
		MBAPI.IMBMission.FinalizeMission(Pointer);
		Pointer = UIntPtr.Zero;
	}

	public void SetMissionCombatType(MissionCombatType missionCombatType)
	{
		MBAPI.IMBMission.SetCombatType(Pointer, (int)missionCombatType);
	}

	public void ConversationCharacterChanged()
	{
		foreach (IMissionListener listener in _listeners)
		{
			listener.OnConversationCharacterChanged();
		}
	}

	public void SetMissionMode(MissionMode newMode, bool atStart)
	{
		if (_missionMode == newMode)
		{
			return;
		}
		MissionMode missionMode = _missionMode;
		_missionMode = newMode;
		if (CurrentState == State.Over)
		{
			return;
		}
		for (int i = 0; i < MissionBehaviors.Count; i++)
		{
			MissionBehaviors[i].OnMissionModeChange(missionMode, atStart);
		}
		foreach (IMissionListener listener in _listeners)
		{
			listener.OnMissionModeChange(missionMode, atStart);
		}
	}

	private AgentCreationResult CreateAgentInternal(AgentFlag agentFlags, int forcedAgentIndex, bool isFemale, ref AgentSpawnData spawnData, ref AgentCapsuleData capsuleData, ref AnimationSystemData animationSystemData, int instanceNo)
	{
		return MBAPI.IMBMission.CreateAgent(Pointer, (ulong)agentFlags, forcedAgentIndex, isFemale, ref spawnData, ref capsuleData.BodyCap, ref capsuleData.CrouchedBodyCap, ref animationSystemData, instanceNo);
	}

	[UsedImplicitly]
	[MBCallback]
	internal void UpdateMissionTimeCache(float curTime)
	{
		_cachedMissionTime = curTime;
	}

	public float GetAverageFps()
	{
		return MBAPI.IMBMission.GetAverageFps(Pointer);
	}

	public static bool ToggleDisableFallAvoid()
	{
		return MBAPI.IMBMission.ToggleDisableFallAvoid();
	}

	public bool IsPositionInsideBoundaries(Vec2 position)
	{
		return MBAPI.IMBMission.IsPositionInsideBoundaries(Pointer, position);
	}

	public bool IsPositionInsideAnyBlockerNavMeshFace2D(Vec2 position)
	{
		return MBAPI.IMBMission.IsPositionInsideAnyBlockerNavMeshFace2D(Pointer, position);
	}

	private bool IsFormationUnitPositionAvailableAux(ref WorldPosition formationPosition, ref WorldPosition unitPosition, ref WorldPosition nearestAvailableUnitPosition, float manhattanDistance)
	{
		return MBAPI.IMBMission.IsFormationUnitPositionAvailable(Pointer, ref formationPosition, ref unitPosition, ref nearestAvailableUnitPosition, manhattanDistance);
	}

	public Agent RayCastForClosestAgent(Vec3 sourcePoint, Vec3 targetPoint, out float collisionDistance, int excludedAgentIndex = -1, float rayThickness = 0.01f)
	{
		collisionDistance = float.NaN;
		return MBAPI.IMBMission.RayCastForClosestAgent(Pointer, sourcePoint, targetPoint, excludedAgentIndex, ref collisionDistance, rayThickness);
	}

	public bool RayCastForClosestAgentsLimbs(Vec3 sourcePoint, Vec3 targetPoint, int excludeAgentIndex, out float collisionDistance, ref int agentIndex, ref sbyte boneIndex)
	{
		collisionDistance = float.NaN;
		return MBAPI.IMBMission.RayCastForClosestAgentsLimbs(Pointer, sourcePoint, targetPoint, excludeAgentIndex, ref collisionDistance, ref agentIndex, ref boneIndex);
	}

	public bool RayCastForClosestAgentsLimbs(Vec3 sourcePoint, Vec3 targetPoint, out float collisionDistance, ref int agentIndex, ref sbyte boneIndex)
	{
		collisionDistance = float.NaN;
		return MBAPI.IMBMission.RayCastForClosestAgentsLimbs(Pointer, sourcePoint, targetPoint, -1, ref collisionDistance, ref agentIndex, ref boneIndex);
	}

	public bool RayCastForGivenAgentsLimbs(Vec3 sourcePoint, Vec3 rayFinishPoint, int givenAgentIndex, ref float collisionDistance, ref sbyte boneIndex)
	{
		return MBAPI.IMBMission.RayCastForGivenAgentsLimbs(Pointer, sourcePoint, rayFinishPoint, givenAgentIndex, ref collisionDistance, ref boneIndex);
	}

	internal AgentProximityMap.ProximityMapSearchStructInternal ProximityMapBeginSearch(Vec2 searchPos, float searchRadius)
	{
		return MBAPI.IMBMission.ProximityMapBeginSearch(Pointer, searchPos, searchRadius);
	}

	internal float ProximityMapMaxSearchRadius()
	{
		return MBAPI.IMBMission.ProximityMapMaxSearchRadius(Pointer);
	}

	public float GetBiggestAgentCollisionPadding()
	{
		return MBAPI.IMBMission.GetBiggestAgentCollisionPadding(Pointer);
	}

	public void SetMissionCorpseFadeOutTimeInSeconds(float corpseFadeOutTimeInSeconds)
	{
		MBAPI.IMBMission.SetMissionCorpseFadeOutTimeInSeconds(Pointer, corpseFadeOutTimeInSeconds);
	}

	public void SetReportStuckAgentsMode(bool value)
	{
		MBAPI.IMBMission.SetReportStuckAgentsMode(Pointer, value);
	}

	internal void BatchFormationUnitPositions(MBArrayList<Vec2i> orderedPositionIndices, MBArrayList<Vec2> orderedLocalPositions, MBList2D<int> availabilityTable, MBList2D<WorldPosition> globalPositionTable, WorldPosition orderPosition, Vec2 direction, int fileCount, int rankCount)
	{
		MBAPI.IMBMission.BatchFormationUnitPositions(Pointer, orderedPositionIndices.RawArray, orderedLocalPositions.RawArray, availabilityTable.RawArray, globalPositionTable.RawArray, orderPosition, direction, fileCount, rankCount);
	}

	internal void ProximityMapFindNext(ref AgentProximityMap.ProximityMapSearchStructInternal searchStruct)
	{
		MBAPI.IMBMission.ProximityMapFindNext(Pointer, ref searchStruct);
	}

	[UsedImplicitly]
	[MBCallback]
	public void ResetMission()
	{
		IMissionListener[] array = _listeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnResetMission();
		}
		foreach (Agent activeAgent in _activeAgents)
		{
			activeAgent.OnRemove();
		}
		foreach (Agent allAgent in _allAgents)
		{
			allAgent.OnDelete();
		}
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnClearScene();
		}
		NumOfFormationsSpawnedTeamOne = 0;
		NumOfFormationsSpawnedTeamTwo = 0;
		foreach (Team team in Teams)
		{
			team.Reset();
		}
		MBAPI.IMBMission.ClearScene(Pointer);
		_activeAgents.Clear();
		_allAgents.Clear();
		_mountsWithoutRiders.Clear();
		MainAgent = null;
		ClearMissiles();
		_missiles.Clear();
		_agentCount = 0;
		for (int j = 0; j < 2; j++)
		{
			_initialAgentCountPerSide[j] = 0;
			_removedAgentCountPerSide[j] = 0;
		}
		ResetMissionObjects();
		RemoveSpawnedMissionObjects();
		_activeMissionObjects.Clear();
		_activeMissionObjects.AddRange(MissionObjects);
		Scene.ClearDecals();
		this.OnMissionReset?.Invoke(this, null);
	}

	public void Initialize()
	{
		Current = this;
		CurrentState = State.Initializing;
		MissionInitializerRecord rec = InitializerRecord;
		MBAPI.IMBMission.InitializeMission(Pointer, ref rec);
	}

	[UsedImplicitly]
	[MBCallback]
	internal void OnSceneCreated(Scene scene)
	{
		Scene = scene;
	}

	[UsedImplicitly]
	[MBCallback]
	internal void TickAgentsAndTeams(float dt)
	{
		TickAgentsAndTeamsImp(dt);
	}

	public void TickAgentsAndTeamsAsync(float dt)
	{
		MBAPI.IMBMission.tickAgentsAndTeamsAsync(Pointer, dt);
	}

	internal void Tick(float dt)
	{
		MBAPI.IMBMission.Tick(Pointer, dt);
	}

	internal void IdleTick(float dt)
	{
		MBAPI.IMBMission.IdleTick(Pointer, dt);
	}

	public void MakeSound(int soundIndex, Vec3 position, bool soundCanBePredicted, bool isReliable, int relatedAgent1, int relatedAgent2)
	{
		MBAPI.IMBMission.MakeSound(Pointer, soundIndex, position, soundCanBePredicted, isReliable, relatedAgent1, relatedAgent2);
	}

	public void MakeSound(int soundIndex, Vec3 position, bool soundCanBePredicted, bool isReliable, int relatedAgent1, int relatedAgent2, ref SoundEventParameter parameter)
	{
		MBAPI.IMBMission.MakeSoundWithParameter(Pointer, soundIndex, position, soundCanBePredicted, isReliable, relatedAgent1, relatedAgent2, parameter);
	}

	public void MakeSoundOnlyOnRelatedPeer(int soundIndex, Vec3 position, int relatedAgent)
	{
		MBAPI.IMBMission.MakeSoundOnlyOnRelatedPeer(Pointer, soundIndex, position, relatedAgent);
	}

	public void AddSoundAlarmFactorToAgents(int ownerId, Vec3 position, float alarmFactor)
	{
		MBAPI.IMBMission.AddSoundAlarmFactorToAgents(Pointer, ownerId, position, alarmFactor);
	}

	public void AddDynamicallySpawnedMissionObjectInfo(DynamicallyCreatedEntity entityInfo)
	{
		_addedEntitiesInfo.Add(entityInfo);
	}

	private void RemoveDynamicallySpawnedMissionObjectInfo(MissionObjectId id)
	{
		DynamicallyCreatedEntity dynamicallyCreatedEntity = _addedEntitiesInfo.FirstOrDefault((DynamicallyCreatedEntity x) => x.ObjectId == id);
		if (dynamicallyCreatedEntity != null)
		{
			_addedEntitiesInfo.Remove(dynamicallyCreatedEntity);
		}
	}

	private int AddMissileAux(int forcedMissileIndex, bool isPrediction, Agent shooterAgent, in WeaponData weaponData, WeaponStatsData[] weaponStatsData, float damageBonus, ref Vec3 position, ref Vec3 direction, ref Mat3 orientation, float baseSpeed, float speed, bool addRigidBody, GameEntity gameEntityToIgnore, bool isPrimaryWeaponShot, out GameEntity missileEntity)
	{
		UIntPtr missileEntity2;
		int result = MBAPI.IMBMission.AddMissile(Pointer, isPrediction, shooterAgent.Index, in weaponData, weaponStatsData, weaponStatsData.Length, damageBonus, ref position, ref direction, ref orientation, baseSpeed, speed, addRigidBody, gameEntityToIgnore?.Pointer ?? UIntPtr.Zero, forcedMissileIndex, isPrimaryWeaponShot, out missileEntity2);
		missileEntity = (isPrediction ? null : new GameEntity(missileEntity2));
		return result;
	}

	private int AddMissileSingleUsageAux(int forcedMissileIndex, bool isPrediction, Agent shooterAgent, in WeaponData weaponData, in WeaponStatsData weaponStatsData, float damageBonus, ref Vec3 position, ref Vec3 direction, ref Mat3 orientation, float baseSpeed, float speed, bool addRigidBody, GameEntity gameEntityToIgnore, bool isPrimaryWeaponShot, out GameEntity missileEntity)
	{
		UIntPtr missileEntity2;
		int result = MBAPI.IMBMission.AddMissileSingleUsage(Pointer, isPrediction, shooterAgent.Index, in weaponData, in weaponStatsData, damageBonus, ref position, ref direction, ref orientation, baseSpeed, speed, addRigidBody, gameEntityToIgnore?.Pointer ?? UIntPtr.Zero, forcedMissileIndex, isPrimaryWeaponShot, out missileEntity2);
		missileEntity = (isPrediction ? null : new GameEntity(missileEntity2));
		return result;
	}

	public Vec3 GetMissileCollisionPoint(Vec3 missileStartingPosition, Vec3 missileDirection, float missileSpeed, in WeaponData weaponData)
	{
		return MBAPI.IMBMission.GetMissileCollisionPoint(Pointer, missileStartingPosition, missileDirection, missileSpeed, in weaponData);
	}

	public void RemoveMissileAsClient(int missileIndex)
	{
		MBAPI.IMBMission.RemoveMissile(Pointer, missileIndex);
	}

	public static float GetMissileVerticalAimCorrection(Vec3 vecToTarget, float missileStartingSpeed, ref WeaponStatsData weaponStatsData, float airFrictionConstant)
	{
		return MBAPI.IMBMission.GetMissileVerticalAimCorrection(vecToTarget, missileStartingSpeed, ref weaponStatsData, airFrictionConstant);
	}

	public static float GetMissileRange(float missileStartingSpeed, float heightDifference)
	{
		return MBAPI.IMBMission.GetMissileRange(missileStartingSpeed, heightDifference);
	}

	public void PrepareMissileWeaponForDrop(int missileIndex)
	{
		MBAPI.IMBMission.PrepareMissileWeaponForDrop(Pointer, missileIndex);
	}

	public void AddParticleSystemBurstByName(string particleSystem, MatrixFrame frame, bool synchThroughNetwork)
	{
		MBAPI.IMBMission.AddParticleSystemBurstByName(Pointer, particleSystem, ref frame, synchThroughNetwork);
	}

	public Vec2 GetClosestBoundaryPosition(Vec2 position)
	{
		return MBAPI.IMBMission.GetClosestBoundaryPosition(Pointer, position);
	}

	private void ResetMissionObjects()
	{
		for (int num = _dynamicEntities.Count - 1; num >= 0; num--)
		{
			DynamicEntityInfo dynamicEntityInfo = _dynamicEntities[num];
			dynamicEntityInfo.Entity.RemoveEnginePhysics();
			dynamicEntityInfo.Entity.Remove(74);
			_dynamicEntities.RemoveAt(num);
		}
		foreach (MissionObject missionObject in MissionObjects)
		{
			if (missionObject.CreatedAtRuntime)
			{
				break;
			}
			missionObject.OnMissionReset();
		}
	}

	private void RemoveSpawnedMissionObjects()
	{
		MissionObject[] array = _missionObjects.ToArray();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			MissionObject missionObject = array[num];
			if (!missionObject.CreatedAtRuntime)
			{
				break;
			}
			if (missionObject.GameEntity != null)
			{
				missionObject.GameEntity.RemoveAllChildren();
				missionObject.GameEntity.Remove(75);
			}
		}
		_spawnedItemEntitiesCreatedAtRuntime.Clear();
		_lastRuntimeMissionObjectIdCount = 0;
		_emptyRuntimeMissionObjectIds.Clear();
		_addedEntitiesInfo.Clear();
	}

	public int GetFreeRuntimeMissionObjectId()
	{
		float totalMissionTime = MBCommon.GetTotalMissionTime();
		int result = -1;
		if (_emptyRuntimeMissionObjectIds.Count > 0)
		{
			if (totalMissionTime - _emptyRuntimeMissionObjectIds.Peek().Item2 > 30f || _lastRuntimeMissionObjectIdCount >= 4095)
			{
				result = _emptyRuntimeMissionObjectIds.Pop().Item1;
			}
			else
			{
				result = _lastRuntimeMissionObjectIdCount;
				_lastRuntimeMissionObjectIdCount++;
			}
		}
		else if (_lastRuntimeMissionObjectIdCount < 4095)
		{
			result = _lastRuntimeMissionObjectIdCount;
			_lastRuntimeMissionObjectIdCount++;
		}
		return result;
	}

	private void ReturnRuntimeMissionObjectId(int id)
	{
		_emptyRuntimeMissionObjectIds.Push((id, MBCommon.GetTotalMissionTime()));
	}

	public int GetFreeSceneMissionObjectId()
	{
		int lastSceneMissionObjectIdCount = _lastSceneMissionObjectIdCount;
		_lastSceneMissionObjectIdCount++;
		return lastSceneMissionObjectIdCount;
	}

	public void SetCameraFrame(ref MatrixFrame cameraFrame, float zoomFactor)
	{
		SetCameraFrame(ref cameraFrame, zoomFactor, ref cameraFrame.origin);
	}

	public void SetCameraFrame(ref MatrixFrame cameraFrame, float zoomFactor, ref Vec3 attenuationPosition)
	{
		cameraFrame.Fill();
		MBAPI.IMBMission.SetCameraFrame(Pointer, ref cameraFrame, zoomFactor, ref attenuationPosition);
	}

	public MatrixFrame GetCameraFrame()
	{
		return MBAPI.IMBMission.GetCameraFrame(Pointer);
	}

	public void ResetFirstThirdPersonView()
	{
		MBAPI.IMBMission.ResetFirstThirdPersonView(Pointer);
	}

	internal void UpdateSceneTimeSpeed()
	{
		if (!(Scene != null))
		{
			return;
		}
		float num = 1f;
		int num2 = -1;
		for (int i = 0; i < _timeSpeedRequests.Count; i++)
		{
			if (_timeSpeedRequests[i].RequestedTimeSpeed < num)
			{
				num = _timeSpeedRequests[i].RequestedTimeSpeed;
				num2 = _timeSpeedRequests[i].RequestID;
			}
		}
		if (!Scene.TimeSpeed.ApproximatelyEqualsTo(num))
		{
			if (num2 != -1)
			{
				TaleWorlds.Library.Debug.Print($"Updated mission time speed with request ID:{num2}, time speed{num}");
			}
			else
			{
				TaleWorlds.Library.Debug.Print($"Reverted time speed back to default({num})");
			}
			Scene.TimeSpeed = num;
		}
	}

	public void AddTimeSpeedRequest(TimeSpeedRequest request)
	{
		_timeSpeedRequests.Add(request);
	}

	[Conditional("_RGL_KEEP_ASSERTS")]
	private void AssertTimeSpeedRequestDoesntExist(TimeSpeedRequest request)
	{
		for (int i = 0; i < _timeSpeedRequests.Count; i++)
		{
			_ = _timeSpeedRequests[i].RequestID;
			_ = request.RequestID;
		}
	}

	public void RemoveTimeSpeedRequest(int timeSpeedRequestID)
	{
		int index = -1;
		for (int i = 0; i < _timeSpeedRequests.Count; i++)
		{
			if (_timeSpeedRequests[i].RequestID == timeSpeedRequestID)
			{
				index = i;
			}
		}
		_timeSpeedRequests.RemoveAt(index);
	}

	public bool GetRequestedTimeSpeed(int timeSpeedRequestID, out float requestedTime)
	{
		for (int i = 0; i < _timeSpeedRequests.Count; i++)
		{
			if (_timeSpeedRequests[i].RequestID == timeSpeedRequestID)
			{
				requestedTime = _timeSpeedRequests[i].RequestedTimeSpeed;
				return true;
			}
		}
		requestedTime = 0f;
		return false;
	}

	public void ClearAgentActions()
	{
		MBAPI.IMBMission.ClearAgentActions(Pointer);
	}

	public void ClearMissiles()
	{
		MBAPI.IMBMission.ClearMissiles(Pointer);
	}

	public void ClearCorpses(bool isMissionReset)
	{
		MBAPI.IMBMission.ClearCorpses(Pointer, isMissionReset);
	}

	private Agent FindAgentWithIndexAux(int index)
	{
		if (index >= 0)
		{
			return MBAPI.IMBMission.FindAgentWithIndex(Pointer, index);
		}
		return null;
	}

	private Agent GetClosestEnemyAgent(MBTeam team, Vec3 position, float radius)
	{
		return MBAPI.IMBMission.GetClosestEnemy(Pointer, team.Index, position, radius);
	}

	private Agent GetClosestAllyAgent(MBTeam team, Vec3 position, float radius)
	{
		return MBAPI.IMBMission.GetClosestAlly(Pointer, team.Index, position, radius);
	}

	private int GetNearbyEnemyAgentCount(MBTeam team, Vec2 position, float radius)
	{
		int allyCount = 0;
		int enemyCount = 0;
		MBAPI.IMBMission.GetAgentCountAroundPosition(Pointer, team.Index, position, radius, ref allyCount, ref enemyCount);
		return enemyCount;
	}

	public bool IsAgentInProximityMap(Agent agent)
	{
		return MBAPI.IMBMission.IsAgentInProximityMap(Pointer, agent.Index);
	}

	public void OnMissionStateActivate()
	{
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnMissionStateActivated();
		}
	}

	public void OnMissionStateDeactivate()
	{
		if (MissionBehaviors == null)
		{
			return;
		}
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnMissionStateDeactivated();
		}
	}

	public void OnMissionStateFinalize(bool forceClearGPUResources)
	{
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnMissionStateFinalized();
		}
		if (GameNetwork.IsSessionActive && GetMissionBehavior<MissionNetworkComponent>() != null)
		{
			RemoveMissionBehavior(GetMissionBehavior<MissionNetworkComponent>());
		}
		for (int num = MissionBehaviors.Count - 1; num >= 0; num--)
		{
			RemoveMissionBehavior(MissionBehaviors[num]);
		}
		MissionLogics.Clear();
		Scene = null;
		Current = null;
		ClearUnreferencedResources(forceClearGPUResources);
	}

	public void ClearUnreferencedResources(bool forceClearGPUResources)
	{
		Common.MemoryCleanupGC();
		if (forceClearGPUResources)
		{
			MBAPI.IMBMission.ClearResources(Pointer);
		}
	}

	internal void OnEntityHit(GameEntity entity, Agent attackerAgent, int inflictedDamage, DamageTypes damageType, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon)
	{
		bool flag = false;
		while (entity != null)
		{
			bool flag2 = false;
			foreach (MissionObject scriptComponent in entity.GetScriptComponents<MissionObject>())
			{
				if (scriptComponent.OnHit(attackerAgent, inflictedDamage, impactPosition, impactDirection, in weapon, null, out var reportDamage))
				{
					flag2 = true;
				}
				flag = flag || reportDamage;
			}
			if (flag2)
			{
				break;
			}
			entity = entity.Parent;
		}
		if (flag && attackerAgent != null && !attackerAgent.IsMount && !attackerAgent.IsAIControlled)
		{
			CombatLogData combatLog = new CombatLogData(isVictimAgentSameAsAttackerAgent: false, attackerAgent.IsHuman, attackerAgent.IsMine, attackerAgent.RiderAgent != null, attackerAgent.RiderAgent?.IsMine ?? false, attackerAgent.IsMount, isVictimAgentHuman: false, isVictimAgentMine: false, isVictimAgentDead: false, doesVictimAgentHaveRiderAgent: false, isVictimAgentRiderAgentIsMine: false, isVictimAgentMount: false, isVictimEntity: true, isVictimRiderAgentSameAsAttackerAgent: false, crushedThrough: false, chamber: false, 0f);
			combatLog.DamageType = damageType;
			combatLog.InflictedDamage = inflictedDamage;
			AddCombatLogSafe(attackerAgent, null, entity, combatLog);
		}
	}

	public float GetMainAgentMaxCameraZoom()
	{
		if (MainAgent != null)
		{
			return MissionGameModels.Current.AgentStatCalculateModel.GetMaxCameraZoom(MainAgent);
		}
		return 1f;
	}

	public WorldPosition GetBestSlopeTowardsDirection(ref WorldPosition centerPosition, float halfSize, ref WorldPosition referencePosition)
	{
		return MBAPI.IMBMission.GetBestSlopeTowardsDirection(Pointer, ref centerPosition, halfSize, ref referencePosition);
	}

	public WorldPosition GetBestSlopeAngleHeightPosForDefending(WorldPosition enemyPosition, WorldPosition defendingPosition, int sampleSize, float distanceRatioAllowedFromDefendedPos, float distanceSqrdAllowedFromBoundary, float cosinusOfBestSlope, float cosinusOfMaxAcceptedSlope, float minSlopeScore, float maxSlopeScore, float excessiveSlopePenalty, float nearConeCenterRatio, float nearConeCenterBonus, float heightDifferenceCeiling, float maxDisplacementPenalty)
	{
		return MBAPI.IMBMission.GetBestSlopeAngleHeightPosForDefending(Pointer, enemyPosition, defendingPosition, sampleSize, distanceRatioAllowedFromDefendedPos, distanceSqrdAllowedFromBoundary, cosinusOfBestSlope, cosinusOfMaxAcceptedSlope, minSlopeScore, maxSlopeScore, excessiveSlopePenalty, nearConeCenterRatio, nearConeCenterBonus, heightDifferenceCeiling, maxDisplacementPenalty);
	}

	public Vec2 GetAveragePositionOfAgents(List<Agent> agents)
	{
		int num = 0;
		Vec2 zero = Vec2.Zero;
		foreach (Agent agent in agents)
		{
			num++;
			zero += agent.Position.AsVec2;
		}
		if (num == 0)
		{
			return Vec2.Invalid;
		}
		return zero * (1f / (float)num);
	}

	private void GetNearbyAgentsAux(Vec2 center, float radius, MBTeam team, GetNearbyAgentsAuxType type, MBList<Agent> resultList)
	{
		EngineStackArray.StackArray40Int agentIds = default(EngineStackArray.StackArray40Int);
		lock (GetNearbyAgentsAuxLock)
		{
			int num = 0;
			while (true)
			{
				int retrievedAgentCount = -1;
				MBAPI.IMBMission.GetNearbyAgentsAux(Pointer, center, radius, team.Index, (int)type, num, ref agentIds, ref retrievedAgentCount);
				for (int i = 0; i < retrievedAgentCount; i++)
				{
					Agent item = DotNetObject.GetManagedObjectWithId(agentIds[i]) as Agent;
					resultList.Add(item);
				}
				if (retrievedAgentCount < 40)
				{
					break;
				}
				num += 40;
			}
		}
	}

	private int GetNearbyAgentsCountAux(Vec2 center, float radius, MBTeam team, GetNearbyAgentsAuxType type)
	{
		int num = 0;
		EngineStackArray.StackArray40Int agentIds = default(EngineStackArray.StackArray40Int);
		lock (GetNearbyAgentsAuxLock)
		{
			int num2 = 0;
			while (true)
			{
				int retrievedAgentCount = -1;
				MBAPI.IMBMission.GetNearbyAgentsAux(Pointer, center, radius, team.Index, (int)type, num2, ref agentIds, ref retrievedAgentCount);
				num += retrievedAgentCount;
				if (retrievedAgentCount < 40)
				{
					break;
				}
				num2 += 40;
			}
			return num;
		}
	}

	public void SetRandomDecideTimeOfAgentsWithIndices(int[] agentIndices, float? minAIReactionTime = null, float? maxAIReactionTime = null)
	{
		if (!minAIReactionTime.HasValue || !maxAIReactionTime.HasValue)
		{
			maxAIReactionTime = -1f;
			minAIReactionTime = maxAIReactionTime;
		}
		MBAPI.IMBMission.SetRandomDecideTimeOfAgents(Pointer, agentIndices.Length, agentIndices, minAIReactionTime.Value, maxAIReactionTime.Value);
	}

	public void SetBowMissileSpeedModifier(float modifier)
	{
		MBAPI.IMBMission.SetBowMissileSpeedModifier(Pointer, modifier);
	}

	public void SetCrossbowMissileSpeedModifier(float modifier)
	{
		MBAPI.IMBMission.SetCrossbowMissileSpeedModifier(Pointer, modifier);
	}

	public void SetThrowingMissileSpeedModifier(float modifier)
	{
		MBAPI.IMBMission.SetThrowingMissileSpeedModifier(Pointer, modifier);
	}

	public void SetMissileRangeModifier(float modifier)
	{
		MBAPI.IMBMission.SetMissileRangeModifier(Pointer, modifier);
	}

	public void SetLastMovementKeyPressed(Agent.MovementControlFlag lastMovementKeyPressed)
	{
		MBAPI.IMBMission.SetLastMovementKeyPressed(Pointer, lastMovementKeyPressed);
	}

	public Vec2 GetWeightedPointOfEnemies(Agent agent, Vec2 basePoint)
	{
		return MBAPI.IMBMission.GetWeightedPointOfEnemies(Pointer, agent.Index, basePoint);
	}

	public bool GetPathBetweenPositions(ref NavigationData navData)
	{
		return MBAPI.IMBMission.GetNavigationPoints(Pointer, ref navData);
	}

	public void SetNavigationFaceCostWithIdAroundPosition(int navigationFaceId, Vec3 position, float cost)
	{
		MBAPI.IMBMission.SetNavigationFaceCostWithIdAroundPosition(Pointer, navigationFaceId, position, cost);
	}

	public WorldPosition GetStraightPathToTarget(Vec2 targetPosition, WorldPosition startingPosition, float samplingDistance = 1f, bool stopAtObstacle = true)
	{
		return MBAPI.IMBMission.GetStraightPathToTarget(Pointer, targetPosition, startingPosition, samplingDistance, stopAtObstacle);
	}

	public void FastForwardMission(float startTime, float endTime)
	{
		MBAPI.IMBMission.FastForwardMission(Pointer, startTime, endTime);
	}

	public int GetDebugAgent()
	{
		return MBAPI.IMBMission.GetDebugAgent(Pointer);
	}

	public void AddAiDebugText(string str)
	{
		MBAPI.IMBMission.AddAiDebugText(Pointer, str);
	}

	public void SetDebugAgent(int index)
	{
		MBAPI.IMBMission.SetDebugAgent(Pointer, index);
	}

	public static float GetFirstPersonFov()
	{
		return BannerlordConfig.FirstPersonFov;
	}

	public float GetWaterLevelAtPosition(Vec2 position)
	{
		return MBAPI.IMBMission.GetWaterLevelAtPosition(Pointer, position);
	}

	public float GetWaterLevelAtPositionMT(Vec2 position)
	{
		return MBAPI.IMBMission.GetWaterLevelAtPosition(Pointer, position);
	}

	public float GetRemovedAgentRatioForSide(BattleSideEnum side)
	{
		float result = 0f;
		if (side == BattleSideEnum.NumSides)
		{
			TaleWorlds.Library.Debug.FailedAssert("Cannot get removed agent count for side. Invalid battle side passed!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", "GetRemovedAgentRatioForSide", 637);
		}
		float num = _initialAgentCountPerSide[(int)side];
		if (num > 0f && _agentCount > 0)
		{
			result = TaleWorlds.Library.MathF.Min((float)_removedAgentCountPerSide[(int)side] / num, 1f);
		}
		return result;
	}

	public void MakeDeploymentPlanForSide(BattleSideEnum battleSide, DeploymentPlanType planType, float spawnPathOffset = 0f)
	{
		if (_deploymentPlan.IsPlanMadeForBattleSide(battleSide, planType))
		{
			return;
		}
		_deploymentPlan.PlanBattleDeployment(battleSide, planType, spawnPathOffset);
		if (!_deploymentPlan.IsPlanMadeForBattleSide(battleSide, out var isFirstPlan, planType) || planType != 0)
		{
			return;
		}
		foreach (IMissionListener listener in _listeners)
		{
			listener.OnInitialDeploymentPlanMade(battleSide, isFirstPlan);
		}
	}

	public void MakeDefaultDeploymentPlans()
	{
		for (int i = 0; i < 2; i++)
		{
			BattleSideEnum battleSide = (BattleSideEnum)i;
			MakeDeploymentPlanForSide(battleSide, DeploymentPlanType.Initial);
			MakeDeploymentPlanForSide(battleSide, DeploymentPlanType.Reinforcement);
		}
	}

	public ref readonly List<SiegeWeapon> GetAttackerWeaponsForFriendlyFirePreventing()
	{
		return ref _attackerWeaponsForFriendlyFirePreventing;
	}

	public void ClearDeploymentPlanForSide(BattleSideEnum battleSide, DeploymentPlanType planType)
	{
		_deploymentPlan.ClearDeploymentPlanForSide(battleSide, planType);
	}

	public void ClearAddedTroopsInDeploymentPlan(BattleSideEnum battleSide, DeploymentPlanType planType)
	{
		_deploymentPlan.ClearAddedTroopsForBattleSide(battleSide, planType);
	}

	public void SetDeploymentPlanSpawnWithHorses(BattleSideEnum side, bool spawnWithHorses)
	{
		_deploymentPlan.SetSpawnWithHorsesForSide(side, spawnWithHorses);
	}

	public void UpdateReinforcementPlan(BattleSideEnum side)
	{
		_deploymentPlan.UpdateReinforcementPlan(side);
	}

	public void AddTroopsToDeploymentPlan(BattleSideEnum side, DeploymentPlanType planType, FormationClass fClass, int footTroopCount, int mountedTroopCount)
	{
		_deploymentPlan.AddTroopsForBattleSide(side, planType, fClass, footTroopCount, mountedTroopCount);
	}

	public bool TryRemakeInitialDeploymentPlanForBattleSide(BattleSideEnum battleSide)
	{
		if (_deploymentPlan.IsPlanMadeForBattleSide(battleSide, DeploymentPlanType.Initial))
		{
			float spawnPathOffsetForSide = _deploymentPlan.GetSpawnPathOffsetForSide(battleSide, DeploymentPlanType.Initial);
			(int, int)[] array = new(int, int)[11];
			foreach (Agent item in _allAgents.Where((Agent agent) => agent.IsHuman && agent.Team != null && agent.Team.Side == battleSide && agent.Formation != null))
			{
				int formationIndex = (int)item.Formation.FormationIndex;
				(int, int) tuple = array[formationIndex];
				array[formationIndex] = (item.HasMount ? (tuple.Item1, tuple.Item2 + 1) : (tuple.Item1 + 1, tuple.Item2));
			}
			if (!_deploymentPlan.IsInitialPlanSuitableForFormations(battleSide, array))
			{
				_deploymentPlan.ClearAddedTroopsForBattleSide(battleSide, DeploymentPlanType.Initial);
				_deploymentPlan.ClearDeploymentPlanForSide(battleSide, DeploymentPlanType.Initial);
				for (int i = 0; i < 11; i++)
				{
					var (num, num2) = array[i];
					if (num + num2 > 0)
					{
						_deploymentPlan.AddTroopsForBattleSide(battleSide, DeploymentPlanType.Initial, (FormationClass)i, num, num2);
					}
				}
				MakeDeploymentPlanForSide(battleSide, DeploymentPlanType.Initial, spawnPathOffsetForSide);
				return _deploymentPlan.IsPlanMadeForBattleSide(battleSide, DeploymentPlanType.Initial);
			}
		}
		return false;
	}

	public void AutoDeployTeamUsingTeamAI(Team team, bool autoAssignDetachments = true)
	{
		List<Formation> list = team.FormationsIncludingEmpty.ToList();
		bool allowAiTicking = AllowAiTicking;
		bool forceTickOccasionally = ForceTickOccasionally;
		bool isTeleportingAgents = IsTeleportingAgents;
		AllowAiTicking = true;
		ForceTickOccasionally = true;
		IsTeleportingAgents = true;
		OrderController orderController = (team.IsPlayerTeam ? team.PlayerOrderController : team.MasterOrderController);
		orderController.SelectAllFormations();
		SetDefaultFormationOrders(orderController);
		team.ResetTactic();
		team.Tick(0f);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].ApplyActionOnEachUnit(delegate(Agent agent)
			{
				agent.UpdateCachedAndFormationValues(updateOnlyMovement: false, arrangementChangeAllowed: false);
			});
		}
		orderController.ClearSelectedFormations();
		if (autoAssignDetachments)
		{
			AutoAssignDetachmentsForDeployment(team);
		}
		IsTeleportingAgents = isTeleportingAgents;
		ForceTickOccasionally = forceTickOccasionally;
		AllowAiTicking = allowAiTicking;
	}

	public void AutoDeployTeamUsingDeploymentPlan(Team team)
	{
		List<Formation> list = team.FormationsIncludingEmpty.ToList();
		if (list.Count <= 0)
		{
			return;
		}
		bool isTeleportingAgents = IsTeleportingAgents;
		IsTeleportingAgents = true;
		OrderController orderController = (team.IsPlayerTeam ? team.PlayerOrderController : team.MasterOrderController);
		orderController.SelectAllFormations();
		SetDefaultFormationOrders(orderController);
		orderController.ClearSelectedFormations();
		DeployTeamUsingDeploymentPlan(team, list);
		foreach (Formation item in list)
		{
			item.ApplyActionOnEachUnit(delegate(Agent agent)
			{
				agent.UpdateCachedAndFormationValues(updateOnlyMovement: true, arrangementChangeAllowed: false);
			});
		}
		IsTeleportingAgents = isTeleportingAgents;
	}

	public void AutoAssignDetachmentsForDeployment(Team team)
	{
		List<Formation> list = team.FormationsIncludingEmpty.ToList();
		bool allowAiTicking = AllowAiTicking;
		bool isTeleportingAgents = IsTeleportingAgents;
		AllowAiTicking = true;
		IsTeleportingAgents = true;
		if (!team.DetachmentManager.Detachments.IsEmpty())
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i].ApplyActionOnEachUnit(delegate(Agent agent)
				{
					agent.Formation?.Team.DetachmentManager.TickAgent(agent);
				});
			}
			int num = 0;
			int num2 = 0;
			foreach (var detachment in team.DetachmentManager.Detachments)
			{
				num += detachment.Item1.GetNumberOfUsableSlots();
			}
			foreach (Formation item in team.FormationsIncludingEmpty)
			{
				num2 += item.CountOfDetachableNonplayerUnits;
			}
			for (int j = 0; j < TaleWorlds.Library.MathF.Min(num, num2); j++)
			{
				team.DetachmentManager.TickDetachments();
			}
			for (int k = 0; k < list.Count; k++)
			{
				list[k].ApplyActionOnEachUnit(delegate(Agent agent)
				{
					if (agent.Detachment != null && !(agent.Detachment is UsableMachine))
					{
						agent.UpdateCachedAndFormationValues(updateOnlyMovement: false, arrangementChangeAllowed: false);
					}
				});
			}
		}
		IsTeleportingAgents = isTeleportingAgents;
		AllowAiTicking = allowAiTicking;
	}

	private void DeployTeamUsingDeploymentPlan(Team team, List<Formation> formations)
	{
		if (_deploymentPlan.IsPlanMadeForBattleSide(team.Side, DeploymentPlanType.Initial))
		{
			foreach (Formation formation in formations)
			{
				IFormationDeploymentPlan formationPlan = _deploymentPlan.GetFormationPlan(team.Side, formation.FormationIndex, DeploymentPlanType.Initial);
				GetFormationSpawnFrame(formation.Team.Side, formation.FormationIndex, isReinforcement: false, out var spawnPosition, out var spawnDirection);
				if (formationPlan.HasDimensions)
				{
					formation.FormOrder = FormOrder.FormOrderCustom(formationPlan.PlannedWidth);
				}
				formation.SetMovementOrder(MovementOrder.MovementOrderMove(spawnPosition));
				formation.FacingOrder = FacingOrder.FacingOrderLookAtDirection(spawnDirection);
				formation.SetPositioning(spawnPosition, spawnDirection, formation.ArrangementOrder.GetUnitSpacing());
				formation.ApplyActionOnEachUnit(delegate(Agent agent)
				{
					agent.UpdateCachedAndFormationValues(updateOnlyMovement: true, arrangementChangeAllowed: false);
				});
				formation.SetMovementOrder(MovementOrder.MovementOrderStop);
			}
			return;
		}
		TaleWorlds.Library.Debug.FailedAssert("Failed to deploy team. Initial deployment plan is not made yet.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", "DeployTeamUsingDeploymentPlan", 1112);
	}

	private void SetDefaultFormationOrders(OrderController orderController)
	{
		orderController.SetOrder(OrderType.AIControlOff);
		orderController.SetFormationUpdateEnabledAfterSetOrder(value: false);
		orderController.SetOrder(OrderType.Mount);
		orderController.SetOrder(OrderType.FireAtWill);
		orderController.SetOrder(OrderType.ArrangementLine);
		orderController.SetOrder(OrderType.StandYourGround);
		orderController.SetOrder(IsSiegeBattle ? OrderType.AIControlOn : OrderType.AIControlOff);
		orderController.SetFormationUpdateEnabledAfterSetOrder(value: true);
	}

	public WorldPosition GetAlternatePositionForNavmeshlessOrOutOfBoundsPosition(Vec2 directionTowards, WorldPosition originalPosition, ref float positionPenalty)
	{
		return MBAPI.IMBMission.GetAlternatePositionForNavmeshlessOrOutOfBoundsPosition(Pointer, ref directionTowards, ref originalPosition, ref positionPenalty);
	}

	public int GetNextDynamicNavMeshIdStart()
	{
		int nextDynamicNavMeshIdStart = _nextDynamicNavMeshIdStart;
		_nextDynamicNavMeshIdStart += 10;
		return nextDynamicNavMeshIdStart;
	}

	public FormationClass GetAgentTroopClass(BattleSideEnum battleSide, BasicCharacterObject agentCharacter)
	{
		if (this.GetAgentTroopClass_Override != null)
		{
			return this.GetAgentTroopClass_Override(battleSide, agentCharacter);
		}
		FormationClass formationClass = agentCharacter.GetFormationClass();
		if (IsSiegeBattle || (IsSallyOutBattle && battleSide == BattleSideEnum.Attacker))
		{
			formationClass = formationClass.DismountedClass();
		}
		return formationClass;
	}

	[UsedImplicitly]
	[MBCallback]
	public WorldPosition GetClosestFleePositionForAgent(Agent agent)
	{
		if (this.GetOverriddenFleePositionForAgent != null)
		{
			WorldPosition? worldPosition = this.GetOverriddenFleePositionForAgent(agent);
			if (worldPosition.HasValue)
			{
				return worldPosition.Value;
			}
		}
		WorldPosition worldPosition2 = agent.GetWorldPosition();
		float maximumForwardUnlimitedSpeed = agent.MaximumForwardUnlimitedSpeed;
		Team team = agent.Team;
		BattleSideEnum side = BattleSideEnum.None;
		bool runnerHasMount = agent.MountAgent != null;
		if (team != null)
		{
			team.UpdateCachedEnemyDataForFleeing();
			side = team.Side;
		}
		MBReadOnlyList<FleePosition> availableFleePositions = ((MissionTeamAIType == MissionTeamAITypeEnum.SallyOut && agent.IsMount) ? GetFleePositionsForSide(BattleSideEnum.Attacker) : GetFleePositionsForSide(side));
		return GetClosestFleePosition(availableFleePositions, worldPosition2, maximumForwardUnlimitedSpeed, runnerHasMount, team?.CachedEnemyDataForFleeing);
	}

	public WorldPosition GetClosestFleePositionForFormation(Formation formation)
	{
		WorldPosition medianPosition = formation.QuerySystem.MedianPosition;
		float movementSpeedMaximum = formation.QuerySystem.MovementSpeedMaximum;
		bool runnerHasMount = formation.QuerySystem.IsCavalryFormation || formation.QuerySystem.IsRangedCavalryFormation;
		Team team = formation.Team;
		team.UpdateCachedEnemyDataForFleeing();
		MBReadOnlyList<FleePosition> fleePositionsForSide = GetFleePositionsForSide(team.Side);
		return GetClosestFleePosition(fleePositionsForSide, medianPosition, movementSpeedMaximum, runnerHasMount, team.CachedEnemyDataForFleeing);
	}

	private WorldPosition GetClosestFleePosition(MBReadOnlyList<FleePosition> availableFleePositions, WorldPosition runnerPosition, float runnerSpeed, bool runnerHasMount, MBReadOnlyList<(float, WorldPosition, int, Vec2, Vec2, bool)> chaserData)
	{
		int num = chaserData?.Count ?? 0;
		if (availableFleePositions.Count > 0)
		{
			float[] array = new float[availableFleePositions.Count];
			WorldPosition[] array2 = new WorldPosition[availableFleePositions.Count];
			for (int i = 0; i < availableFleePositions.Count; i++)
			{
				array[i] = 1f;
				array2[i] = new WorldPosition(Scene, UIntPtr.Zero, availableFleePositions[i].GetClosestPointToEscape(runnerPosition.AsVec2), hasValidZ: false);
				array2[i].SetVec2(array2[i].AsVec2 - runnerPosition.AsVec2);
			}
			for (int j = 0; j < num; j++)
			{
				float item = chaserData[j].Item1;
				if (item <= 0f)
				{
					continue;
				}
				Vec2 asVec = chaserData[j].Item2.AsVec2;
				int item2 = chaserData[j].Item3;
				Vec2 vec;
				if (item2 > 1)
				{
					Vec2 item3 = chaserData[j].Item4;
					Vec2 item4 = chaserData[j].Item5;
					vec = MBMath.GetClosestPointInLineSegmentToPoint(runnerPosition.AsVec2, item3, item4) - runnerPosition.AsVec2;
				}
				else
				{
					vec = asVec - runnerPosition.AsVec2;
				}
				for (int k = 0; k < availableFleePositions.Count; k++)
				{
					float num2 = vec.DotProduct(array2[k].AsVec2.Normalized());
					if (!(num2 <= 0f))
					{
						float num3 = TaleWorlds.Library.MathF.Max(TaleWorlds.Library.MathF.Abs(vec.DotProduct(array2[k].AsVec2.LeftVec().Normalized())) / item, 1f);
						float num4 = TaleWorlds.Library.MathF.Max(num2 / runnerSpeed, 1f);
						if (!(num4 <= num3))
						{
							float num5 = num4 / num3;
							num5 /= num2;
							array[k] += num5 * (float)item2;
						}
					}
				}
			}
			for (int l = 0; l < availableFleePositions.Count; l++)
			{
				WorldPosition point = new WorldPosition(Scene, UIntPtr.Zero, availableFleePositions[l].GetClosestPointToEscape(runnerPosition.AsVec2), hasValidZ: false);
				if (Scene.GetPathDistanceBetweenPositions(ref runnerPosition, ref point, 0f, out var pathDistance))
				{
					array[l] *= pathDistance;
				}
				else
				{
					array[l] = float.MaxValue;
				}
			}
			int num6 = -1;
			float num7 = float.MaxValue;
			for (int m = 0; m < availableFleePositions.Count; m++)
			{
				if (num7 > array[m])
				{
					num6 = m;
					num7 = array[m];
				}
			}
			if (num6 >= 0)
			{
				Vec3 closestPointToEscape = availableFleePositions[num6].GetClosestPointToEscape(runnerPosition.AsVec2);
				return new WorldPosition(Scene, UIntPtr.Zero, closestPointToEscape, hasValidZ: false);
			}
		}
		float[] array3 = new float[4];
		for (int n = 0; n < num; n++)
		{
			Vec2 asVec2 = chaserData[n].Item2.AsVec2;
			int item5 = chaserData[n].Item3;
			Vec2 vec2;
			if (item5 > 1)
			{
				Vec2 item6 = chaserData[n].Item4;
				Vec2 item7 = chaserData[n].Item5;
				vec2 = MBMath.GetClosestPointInLineSegmentToPoint(runnerPosition.AsVec2, item6, item7) - runnerPosition.AsVec2;
			}
			else
			{
				vec2 = asVec2 - runnerPosition.AsVec2;
			}
			float num8 = vec2.Length;
			if (chaserData[n].Item6)
			{
				num8 *= 0.5f;
			}
			if (runnerHasMount)
			{
				num8 *= 2f;
			}
			float num9 = MBMath.ClampFloat(1f - (num8 - 40f) / 40f, 0.01f, 1f);
			Vec2 vec3 = vec2.Normalized();
			float num10 = 1.2f;
			float num11 = num9 * (float)item5 * num10;
			float num12 = num11 * TaleWorlds.Library.MathF.Abs(vec3.x);
			float num13 = num11 * TaleWorlds.Library.MathF.Abs(vec3.y);
			array3[(!(vec3.y < 0f)) ? 1u : 0u] -= num13;
			array3[(vec3.x < 0f) ? 2 : 3] -= num12;
			array3[(vec3.y < 0f) ? 1u : 0u] += num13;
			array3[(vec3.x < 0f) ? 3 : 2] += num12;
		}
		float num14 = 0.04f;
		Scene.GetBoundingBox(out var min, out var max);
		Vec2 closestBoundaryPosition = GetClosestBoundaryPosition(new Vec2(runnerPosition.X, min.y));
		Vec2 closestBoundaryPosition2 = GetClosestBoundaryPosition(new Vec2(runnerPosition.X, max.y));
		Vec2 closestBoundaryPosition3 = GetClosestBoundaryPosition(new Vec2(min.x, runnerPosition.Y));
		Vec2 closestBoundaryPosition4 = GetClosestBoundaryPosition(new Vec2(max.x, runnerPosition.Y));
		float num15 = closestBoundaryPosition2.y - closestBoundaryPosition.y;
		float num16 = closestBoundaryPosition4.x - closestBoundaryPosition3.x;
		array3[0] += (num15 - (runnerPosition.Y - closestBoundaryPosition.y)) * num14;
		array3[1] += (num15 - (closestBoundaryPosition2.y - runnerPosition.Y)) * num14;
		array3[2] += (num16 - (runnerPosition.X - closestBoundaryPosition3.x)) * num14;
		array3[3] += (num16 - (closestBoundaryPosition4.x - runnerPosition.X)) * num14;
		return new WorldPosition(position: new Vec3((array3[0] >= array3[1] && array3[0] >= array3[2] && array3[0] >= array3[3]) ? new Vec2(closestBoundaryPosition.x, closestBoundaryPosition.y) : ((array3[1] >= array3[2] && array3[1] >= array3[3]) ? new Vec2(closestBoundaryPosition2.x, closestBoundaryPosition2.y) : ((!(array3[2] >= array3[3])) ? new Vec2(closestBoundaryPosition4.x, closestBoundaryPosition4.y) : new Vec2(closestBoundaryPosition3.x, closestBoundaryPosition3.y))), runnerPosition.GetNavMeshZ()), scene: Scene, navMesh: UIntPtr.Zero, hasValidZ: false);
	}

	public MBReadOnlyList<FleePosition> GetFleePositionsForSide(BattleSideEnum side)
	{
		int num;
		switch (side)
		{
		case BattleSideEnum.NumSides:
			TaleWorlds.Library.Debug.FailedAssert("Flee position with invalid battle side field found!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", "GetFleePositionsForSide", 1700);
			return null;
		default:
			num = (int)(side + 1);
			break;
		case BattleSideEnum.None:
			num = 0;
			break;
		}
		int num2 = num;
		return _fleePositions[num2];
	}

	public void AddToWeaponListForFriendlyFirePreventing(SiegeWeapon weapon)
	{
		_attackerWeaponsForFriendlyFirePreventing.Add(weapon);
	}

	public Mission(MissionInitializerRecord rec, MissionState missionState)
	{
		Pointer = MBAPI.IMBMission.CreateMission(this);
		_spawnedItemEntitiesCreatedAtRuntime = new List<SpawnedItemEntity>();
		_missionObjects = new MBList<MissionObject>();
		_activeMissionObjects = new MBList<MissionObject>();
		_mountsWithoutRiders = new MBList<KeyValuePair<Agent, MissionTime>>();
		_addedEntitiesInfo = new MBList<DynamicallyCreatedEntity>();
		_emptyRuntimeMissionObjectIds = new Stack<(int, float)>();
		Boundaries = new MBBoundaryCollection(this);
		InitializerRecord = rec;
		CurrentState = State.NewlyCreated;
		IsInventoryAccessible = false;
		IsQuestScreenAccessible = true;
		IsCharacterWindowAccessible = true;
		IsPartyWindowAccessible = true;
		IsKingdomWindowAccessible = true;
		IsClanWindowAccessible = true;
		IsBannerWindowAccessible = false;
		IsEncyclopediaWindowAccessible = true;
		_missiles = new Dictionary<int, Missile>();
		_activeAgents = new MBList<Agent>(256);
		_allAgents = new MBList<Agent>(256);
		for (int i = 0; i < 3; i++)
		{
			_fleePositions[i] = new MBList<FleePosition>(32);
		}
		for (int j = 0; j < 2; j++)
		{
			_initialAgentCountPerSide[j] = 0;
			_removedAgentCountPerSide[j] = 0;
		}
		MissionBehaviors = new List<MissionBehavior>();
		MissionLogics = new List<MissionLogic>();
		_otherMissionBehaviors = new List<MissionBehavior>();
		_missionState = missionState;
		_battleSpawnPathSelector = new BattleSpawnPathSelector(this);
		Teams = new TeamCollection(this);
		_deploymentPlan = new MissionDeploymentPlan(this);
		MissionTimeTracker = new MissionTimeTracker();
	}

	public void AddFleePosition(FleePosition fleePosition)
	{
		BattleSideEnum side = fleePosition.GetSide();
		switch (side)
		{
		case BattleSideEnum.NumSides:
			TaleWorlds.Library.Debug.FailedAssert("Flee position with invalid battle side field found!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", "AddFleePosition", 1781);
			break;
		case BattleSideEnum.None:
		{
			for (int i = 0; i < _fleePositions.Length; i++)
			{
				_fleePositions[i].Add(fleePosition);
			}
			break;
		}
		default:
		{
			int num = (int)(side + 1);
			_fleePositions[num].Add(fleePosition);
			break;
		}
		}
	}

	private void FreeResources()
	{
		MainAgent = null;
		Teams.ClearResources();
		SpectatorTeam = null;
		_activeAgents = null;
		_allAgents = null;
		if (GameNetwork.NetworkPeersValid)
		{
			foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
			{
				MissionPeer component = networkPeer.GetComponent<MissionPeer>();
				if (component != null)
				{
					component.ClearAllVisuals(freeResources: true);
					networkPeer.RemoveComponent(component);
				}
				MissionRepresentativeBase component2 = networkPeer.GetComponent<MissionRepresentativeBase>();
				if (component2 != null)
				{
					networkPeer.RemoveComponent(component2);
				}
			}
		}
		if (GameNetwork.DisconnectedNetworkPeers != null)
		{
			TaleWorlds.Library.Debug.Print("DisconnectedNetworkPeers.Clear()", 0, TaleWorlds.Library.Debug.DebugColor.White, 17179869184uL);
			GameNetwork.DisconnectedNetworkPeers.Clear();
		}
		_missionState = null;
	}

	public void RetreatMission()
	{
		foreach (MissionLogic missionLogic in MissionLogics)
		{
			missionLogic.OnRetreatMission();
		}
		if (MBEditor.EditModeEnabled && MBEditor.IsEditModeOn)
		{
			MBEditor.LeaveEditMissionMode();
		}
		else
		{
			EndMission();
		}
	}

	public void SurrenderMission()
	{
		foreach (MissionLogic missionLogic in MissionLogics)
		{
			missionLogic.OnSurrenderMission();
		}
		if (MBEditor.EditModeEnabled && MBEditor.IsEditModeOn)
		{
			MBEditor.LeaveEditMissionMode();
		}
		else
		{
			EndMission();
		}
	}

	public bool HasMissionBehavior<T>() where T : MissionBehavior
	{
		return GetMissionBehavior<T>() != null;
	}

	[UsedImplicitly]
	[MBCallback]
	internal void OnAgentAddedAsCorpse(Agent affectedAgent)
	{
		if (GameNetwork.IsClientOrReplay)
		{
			return;
		}
		for (int i = 0; i < affectedAgent.GetAttachedWeaponsCount(); i++)
		{
			if (affectedAgent.GetAttachedWeapon(i).Item.ItemFlags.HasAnyFlag(ItemFlags.CanBePickedUpFromCorpse))
			{
				SpawnAttachedWeaponOnCorpse(affectedAgent, i, -1);
			}
		}
		affectedAgent.ClearAttachedWeapons();
	}

	public void SpawnAttachedWeaponOnCorpse(Agent agent, int attachedWeaponIndex, int forcedSpawnIndex)
	{
		agent.AgentVisuals.GetSkeleton().ForceUpdateBoneFrames();
		MissionWeapon attachedWeapon = agent.GetAttachedWeapon(attachedWeaponIndex);
		GameEntity attachedWeaponEntity = agent.AgentVisuals.GetAttachedWeaponEntity(attachedWeaponIndex);
		attachedWeaponEntity.CreateAndAddScriptComponent(typeof(SpawnedItemEntity).Name);
		SpawnedItemEntity firstScriptOfType = attachedWeaponEntity.GetFirstScriptOfType<SpawnedItemEntity>();
		if (forcedSpawnIndex >= 0)
		{
			firstScriptOfType.Id = new MissionObjectId(forcedSpawnIndex, createdAtRuntime: true);
		}
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SpawnAttachedWeaponOnCorpse(agent.Index, attachedWeaponIndex, firstScriptOfType.Id.Id));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		SpawnWeaponAux(attachedWeaponEntity, attachedWeapon, WeaponSpawnFlags.AsMissile | WeaponSpawnFlags.WithStaticPhysics, Vec3.Zero, Vec3.Zero, hasLifeTime: false);
	}

	public void AddMountWithoutRider(Agent mount)
	{
		_mountsWithoutRiders.Add(new KeyValuePair<Agent, MissionTime>(mount, MissionTime.Now));
	}

	public void RemoveMountWithoutRider(Agent mount)
	{
		for (int i = 0; i < _mountsWithoutRiders.Count; i++)
		{
			if (_mountsWithoutRiders[i].Key == mount)
			{
				_mountsWithoutRiders.RemoveAt(i);
				break;
			}
		}
	}

	[UsedImplicitly]
	[MBCallback]
	internal void OnAgentDeleted(Agent affectedAgent)
	{
		if (affectedAgent == null)
		{
			return;
		}
		affectedAgent.State = AgentState.Deleted;
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnAgentDeleted(affectedAgent);
		}
		_allAgents.Remove(affectedAgent);
		affectedAgent.OnDelete();
		affectedAgent.SetTeam(null, sync: false);
	}

	[UsedImplicitly]
	[MBCallback]
	internal void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		this.OnBeforeAgentRemoved?.Invoke(affectedAgent, affectorAgent, agentState, killingBlow);
		affectedAgent.State = agentState;
		if (affectorAgent != null && affectorAgent.Team != affectedAgent.Team)
		{
			affectorAgent.KillCount++;
		}
		affectedAgent.Team?.DeactivateAgent(affectedAgent);
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnEarlyAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);
		}
		foreach (MissionBehavior missionBehavior2 in MissionBehaviors)
		{
			missionBehavior2.OnAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);
		}
		bool num = MainAgent == affectedAgent;
		if (num)
		{
			affectedAgent.OnMainAgentWieldedItemChange = null;
			MainAgent = null;
		}
		affectedAgent.OnAgentWieldedItemChange = null;
		affectedAgent.OnAgentMountedStateChanged = null;
		if (affectedAgent.Team != null && affectedAgent.Team.Side != BattleSideEnum.None)
		{
			_removedAgentCountPerSide[(int)affectedAgent.Team.Side]++;
		}
		_activeAgents.Remove(affectedAgent);
		affectedAgent.OnRemove();
		if (affectedAgent.IsMount && affectedAgent.RiderAgent == null)
		{
			RemoveMountWithoutRider(affectedAgent);
		}
		if (num)
		{
			affectedAgent.Team.DelegateCommandToAI();
		}
		if (GameNetwork.IsClientOrReplay || agentState == AgentState.Routed || !affectedAgent.GetAgentFlags().HasAnyFlag(AgentFlag.CanWieldWeapon))
		{
			return;
		}
		EquipmentIndex wieldedItemIndex = affectedAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
		if (wieldedItemIndex == EquipmentIndex.ExtraWeaponSlot)
		{
			WeaponComponentData currentUsageItem = affectedAgent.Equipment[wieldedItemIndex].CurrentUsageItem;
			if (currentUsageItem != null && currentUsageItem.WeaponClass == WeaponClass.Banner)
			{
				affectedAgent.DropItem(EquipmentIndex.ExtraWeaponSlot);
			}
		}
	}

	public void OnObjectDisabled(DestructableComponent destructionComponent)
	{
		destructionComponent.GameEntity.GetFirstScriptOfType<UsableMachine>()?.Disable();
		destructionComponent?.SetAbilityOfFaces(enabled: false);
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnObjectDisabled(destructionComponent);
		}
	}

	public MissionObjectId SpawnWeaponAsDropFromMissile(int missileIndex, MissionObject attachedMissionObject, in MatrixFrame attachLocalFrame, WeaponSpawnFlags spawnFlags, in Vec3 velocity, in Vec3 angularVelocity, int forcedSpawnIndex)
	{
		PrepareMissileWeaponForDrop(missileIndex);
		Missile missile = _missiles[missileIndex];
		attachedMissionObject?.AddStuckMissile(missile.Entity);
		if (attachedMissionObject != null)
		{
			GameEntity entity = missile.Entity;
			MatrixFrame frame = attachedMissionObject.GameEntity.GetGlobalFrame().TransformToParent(attachLocalFrame);
			entity.SetGlobalFrame(in frame);
		}
		else
		{
			missile.Entity.SetGlobalFrame(in attachLocalFrame);
		}
		missile.Entity.CreateAndAddScriptComponent(typeof(SpawnedItemEntity).Name);
		SpawnedItemEntity firstScriptOfType = missile.Entity.GetFirstScriptOfType<SpawnedItemEntity>();
		if (forcedSpawnIndex >= 0)
		{
			firstScriptOfType.Id = new MissionObjectId(forcedSpawnIndex, createdAtRuntime: true);
		}
		SpawnWeaponAux(missile.Entity, missile.Weapon, spawnFlags, velocity, angularVelocity, hasLifeTime: true);
		return firstScriptOfType.Id;
	}

	[UsedImplicitly]
	[MBCallback]
	internal void SpawnWeaponAsDropFromAgent(Agent agent, EquipmentIndex equipmentIndex, ref Vec3 velocity, ref Vec3 angularVelocity, WeaponSpawnFlags spawnFlags)
	{
		Vec3 velocity2 = agent.Velocity;
		if ((velocity - velocity2).LengthSquared > 100f)
		{
			Vec3 vec = (velocity - velocity2).NormalizedCopy() * 10f;
			velocity = velocity2 + vec;
		}
		SpawnWeaponAsDropFromAgentAux(agent, equipmentIndex, ref velocity, ref angularVelocity, spawnFlags, -1);
	}

	public void SpawnWeaponAsDropFromAgentAux(Agent agent, EquipmentIndex equipmentIndex, ref Vec3 velocity, ref Vec3 angularVelocity, WeaponSpawnFlags spawnFlags, int forcedSpawnIndex)
	{
		agent.AgentVisuals.GetSkeleton().ForceUpdateBoneFrames();
		agent.PrepareWeaponForDropInEquipmentSlot(equipmentIndex, (spawnFlags & WeaponSpawnFlags.WithHolster) != 0);
		GameEntity weaponEntityFromEquipmentSlot = agent.GetWeaponEntityFromEquipmentSlot(equipmentIndex);
		weaponEntityFromEquipmentSlot.CreateAndAddScriptComponent(typeof(SpawnedItemEntity).Name);
		SpawnedItemEntity firstScriptOfType = weaponEntityFromEquipmentSlot.GetFirstScriptOfType<SpawnedItemEntity>();
		if (forcedSpawnIndex >= 0)
		{
			firstScriptOfType.Id = new MissionObjectId(forcedSpawnIndex, createdAtRuntime: true);
		}
		float maximumValue = CompressionMission.SpawnedItemVelocityCompressionInfo.GetMaximumValue();
		float maximumValue2 = CompressionMission.SpawnedItemAngularVelocityCompressionInfo.GetMaximumValue();
		if (velocity.LengthSquared > maximumValue * maximumValue)
		{
			velocity = velocity.NormalizedCopy() * maximumValue;
		}
		if (angularVelocity.LengthSquared > maximumValue2 * maximumValue2)
		{
			angularVelocity = angularVelocity.NormalizedCopy() * maximumValue2;
		}
		MissionWeapon weapon = agent.Equipment[equipmentIndex];
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SpawnWeaponAsDropFromAgent(agent.Index, equipmentIndex, velocity, angularVelocity, spawnFlags, firstScriptOfType.Id.Id));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		SpawnWeaponAux(weaponEntityFromEquipmentSlot, weapon, spawnFlags, velocity, angularVelocity, hasLifeTime: true);
		if (!GameNetwork.IsClientOrReplay)
		{
			for (int i = 0; i < weapon.GetAttachedWeaponsCount(); i++)
			{
				if (weapon.GetAttachedWeapon(i).Item.ItemFlags.HasAnyFlag(ItemFlags.CanBePickedUpFromCorpse))
				{
					SpawnAttachedWeaponOnSpawnedWeapon(firstScriptOfType, i, -1);
				}
			}
		}
		agent.OnWeaponDrop(equipmentIndex);
		this.OnItemDrop?.Invoke(agent, firstScriptOfType);
	}

	public void SpawnAttachedWeaponOnSpawnedWeapon(SpawnedItemEntity spawnedWeapon, int attachmentIndex, int forcedSpawnIndex)
	{
		GameEntity child = spawnedWeapon.GameEntity.GetChild(attachmentIndex);
		child.CreateAndAddScriptComponent(typeof(SpawnedItemEntity).Name);
		SpawnedItemEntity firstScriptOfType = child.GetFirstScriptOfType<SpawnedItemEntity>();
		if (forcedSpawnIndex >= 0)
		{
			firstScriptOfType.Id = new MissionObjectId(forcedSpawnIndex, createdAtRuntime: true);
		}
		SpawnWeaponAux(child, spawnedWeapon.WeaponCopy.GetAttachedWeapon(attachmentIndex), WeaponSpawnFlags.AsMissile | WeaponSpawnFlags.WithStaticPhysics, Vec3.Zero, Vec3.Zero, hasLifeTime: false);
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SpawnAttachedWeaponOnSpawnedWeapon(spawnedWeapon.Id, attachmentIndex, firstScriptOfType.Id.Id));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
	}

	public GameEntity SpawnWeaponWithNewEntity(ref MissionWeapon weapon, WeaponSpawnFlags spawnFlags, MatrixFrame frame)
	{
		return SpawnWeaponWithNewEntityAux(weapon, spawnFlags, frame, -1, null, hasLifeTime: false);
	}

	public GameEntity SpawnWeaponWithNewEntityAux(MissionWeapon weapon, WeaponSpawnFlags spawnFlags, MatrixFrame frame, int forcedSpawnIndex, MissionObject attachedMissionObject, bool hasLifeTime)
	{
		GameEntity gameEntity = GameEntityExtensions.Instantiate(Scene, weapon, spawnFlags.HasAnyFlag(WeaponSpawnFlags.WithHolster), needBatchedVersion: true);
		gameEntity.CreateAndAddScriptComponent(typeof(SpawnedItemEntity).Name);
		SpawnedItemEntity firstScriptOfType = gameEntity.GetFirstScriptOfType<SpawnedItemEntity>();
		if (forcedSpawnIndex >= 0)
		{
			firstScriptOfType.Id = new MissionObjectId(forcedSpawnIndex, createdAtRuntime: true);
		}
		attachedMissionObject?.GameEntity.AddChild(gameEntity);
		if (attachedMissionObject != null)
		{
			MatrixFrame frame2 = attachedMissionObject.GameEntity.GetGlobalFrame().TransformToParent(frame);
			gameEntity.SetGlobalFrame(in frame2);
		}
		else
		{
			gameEntity.SetGlobalFrame(in frame);
		}
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new SpawnWeaponWithNewEntity(weapon, spawnFlags, firstScriptOfType.Id.Id, frame, attachedMissionObject.Id, isVisible: true, hasLifeTime));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
			for (int i = 0; i < weapon.GetAttachedWeaponsCount(); i++)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new AttachWeaponToSpawnedWeapon(weapon.GetAttachedWeapon(i), firstScriptOfType.Id, weapon.GetAttachedWeaponFrame(i)));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
			}
		}
		Vec3 zero = Vec3.Zero;
		SpawnWeaponAux(gameEntity, weapon, spawnFlags, zero, zero, hasLifeTime);
		return gameEntity;
	}

	public void AttachWeaponWithNewEntityToSpawnedWeapon(MissionWeapon weapon, SpawnedItemEntity spawnedItem, MatrixFrame attachLocalFrame)
	{
		GameEntity gameEntity = GameEntityExtensions.Instantiate(Scene, weapon, showHolsterWithWeapon: false, needBatchedVersion: true);
		spawnedItem.GameEntity.AddChild(gameEntity);
		gameEntity.SetFrame(ref attachLocalFrame);
		spawnedItem.AttachWeaponToWeapon(weapon, ref attachLocalFrame);
	}

	private void SpawnWeaponAux(GameEntity weaponEntity, MissionWeapon weapon, WeaponSpawnFlags spawnFlags, Vec3 velocity, Vec3 angularVelocity, bool hasLifeTime)
	{
		SpawnedItemEntity firstScriptOfType = weaponEntity.GetFirstScriptOfType<SpawnedItemEntity>();
		bool flag = weapon.IsBanner();
		MissionWeapon weapon2 = weapon;
		bool hasLifeTime2 = !flag && hasLifeTime;
		WeaponSpawnFlags spawnFlags2 = spawnFlags;
		Vec3 fakeSimulationVelocity = (flag ? velocity : Vec3.Zero);
		firstScriptOfType.Initialize(weapon2, hasLifeTime2, spawnFlags2, in fakeSimulationVelocity);
		if (spawnFlags.HasAnyFlag(WeaponSpawnFlags.WithPhysics | WeaponSpawnFlags.WithStaticPhysics))
		{
			BodyFlags bodyFlags = BodyFlags.OnlyCollideWithRaycast | BodyFlags.DroppedItem;
			if (weapon.Item.ItemFlags.HasAnyFlag(ItemFlags.CannotBePickedUp) || spawnFlags.HasAnyFlag(WeaponSpawnFlags.CannotBePickedUp))
			{
				bodyFlags |= BodyFlags.DoNotCollideWithRaycast;
			}
			weaponEntity.BodyFlag |= bodyFlags;
			WeaponData weaponData = weapon.GetWeaponData(needBatchedVersionForMeshes: true);
			RecalculateBody(ref weaponData, weapon.Item.ItemComponent, weapon.Item.WeaponDesign, ref spawnFlags);
			if (flag)
			{
				weaponEntity.AddPhysics(weaponData.BaseWeight, weaponData.CenterOfMassShift, weaponData.Shape, velocity, angularVelocity, PhysicsMaterial.GetFromIndex(weaponData.PhysicsMaterialIndex), isStatic: true, 0);
				weaponData.DeinitializeManagedPointers();
			}
			else if (spawnFlags.HasAnyFlag(WeaponSpawnFlags.WithPhysics | WeaponSpawnFlags.WithStaticPhysics))
			{
				weaponEntity.AddPhysics(weaponData.BaseWeight, weaponData.CenterOfMassShift, weaponData.Shape, velocity, angularVelocity, PhysicsMaterial.GetFromIndex(weaponData.PhysicsMaterialIndex), spawnFlags.HasAnyFlag(WeaponSpawnFlags.WithStaticPhysics), 0);
			}
			weaponData.DeinitializeManagedPointers();
		}
	}

	public void OnEquipItemsFromSpawnEquipmentBegin(Agent agent, Agent.CreationType creationType)
	{
		foreach (IMissionListener listener in _listeners)
		{
			listener.OnEquipItemsFromSpawnEquipmentBegin(agent, creationType);
		}
	}

	public void OnEquipItemsFromSpawnEquipment(Agent agent, Agent.CreationType creationType)
	{
		foreach (IMissionListener listener in _listeners)
		{
			listener.OnEquipItemsFromSpawnEquipment(agent, creationType);
		}
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("flee_enemies", "mission")]
	public static string MakeEnemiesFleeCheat(List<string> strings)
	{
		if (!GameNetwork.IsClientOrReplay)
		{
			if (Current != null && Current.Agents != null)
			{
				foreach (Agent item in Current.Agents.Where((Agent agent) => agent.IsHuman && agent.IsEnemyOf(Agent.Main)))
				{
					item.CommonAIComponent?.Panic();
				}
				return "enemies are fleeing";
			}
			return "mission is not available";
		}
		return "does not work in multiplayer";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("flee_team", "mission")]
	public static string MakeTeamFleeCheat(List<string> strings)
	{
		if (!GameNetwork.IsClientOrReplay)
		{
			if (Current != null && Current.Agents != null)
			{
				string text = "Usage 1: flee_team [ Attacker | AttackerAlly | Defender | DefenderAlly ]\nUsage 2: flee_team [ Attacker | AttackerAlly | Defender | DefenderAlly ] [FormationNo]";
				if (strings.IsEmpty() || strings[0] == "help")
				{
					return "makes an entire team or a team's formation flee battle.\n" + text;
				}
				if (strings.Count >= 3)
				{
					return "invalid number of parameters.\n" + text;
				}
				string text2 = strings[0];
				Team targetTeam = null;
				switch (text2.ToLower())
				{
				case "attacker":
					targetTeam = Current.AttackerTeam;
					break;
				case "attackerally":
					targetTeam = Current.AttackerAllyTeam;
					break;
				case "defender":
					targetTeam = Current.DefenderTeam;
					break;
				case "defenderally":
					targetTeam = Current.DefenderAllyTeam;
					break;
				}
				if (targetTeam == null)
				{
					return "given team is not valid";
				}
				Formation targetFormation = null;
				if (strings.Count == 2)
				{
					int num = 8;
					int num2 = int.Parse(strings[1]);
					if (num2 < 0 || num2 >= num)
					{
						return "invalid formation index. formation index should be between [0, " + (num - 1) + "]";
					}
					FormationClass formationClass = (FormationClass)num2;
					targetFormation = targetTeam.GetFormation(formationClass);
				}
				if (targetFormation == null)
				{
					foreach (Agent item in Current.Agents.Where((Agent agent) => agent.IsHuman && agent.Team == targetTeam))
					{
						item.CommonAIComponent?.Panic();
					}
					return "agents in team: " + text2 + " are fleeing";
				}
				foreach (Agent item2 in Current.Agents.Where((Agent agent) => agent.IsHuman && agent.Formation == targetFormation))
				{
					item2.CommonAIComponent?.Panic();
				}
				return "agents in team: " + text2 + " and formation: " + (int)targetFormation.FormationIndex + " (" + targetFormation.FormationIndex.ToString() + ") are fleeing";
			}
			return "mission is not available";
		}
		return "does not work in multiplayer";
	}

	public void RecalculateBody(ref WeaponData weaponData, ItemComponent itemComponent, WeaponDesign craftedWeaponData, ref WeaponSpawnFlags spawnFlags)
	{
		WeaponComponent weaponComponent = (WeaponComponent)itemComponent;
		ItemObject item = weaponComponent.Item;
		if (spawnFlags.HasAnyFlag(WeaponSpawnFlags.WithHolster))
		{
			weaponData.Shape = (string.IsNullOrEmpty(item.HolsterBodyName) ? null : PhysicsShape.GetFromResource(item.HolsterBodyName));
		}
		else
		{
			weaponData.Shape = (string.IsNullOrEmpty(item.BodyName) ? null : PhysicsShape.GetFromResource(item.BodyName));
		}
		PhysicsShape physicsShape = weaponData.Shape;
		if (physicsShape == null)
		{
			TaleWorlds.Library.Debug.FailedAssert("Item has no body! Applying a default body, but this should not happen! Check this!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", "RecalculateBody", 2614);
			physicsShape = PhysicsShape.GetFromResource("bo_axe_short");
		}
		if (!weaponComponent.Item.ItemFlags.HasAnyFlag(ItemFlags.DoNotScaleBodyAccordingToWeaponLength))
		{
			if (spawnFlags.HasAnyFlag(WeaponSpawnFlags.WithHolster) || !item.RecalculateBody)
			{
				weaponData.Shape = physicsShape;
			}
			else
			{
				PhysicsShape physicsShape2 = (weaponData.Shape = physicsShape.CreateCopy());
				float num = (float)weaponComponent.PrimaryWeapon.WeaponLength * 0.01f;
				if (craftedWeaponData != null)
				{
					physicsShape2.Clear();
					physicsShape2.InitDescription();
					float num2 = 0f;
					float num3 = 0f;
					float z = 0f;
					for (int i = 0; i < craftedWeaponData.UsedPieces.Length; i++)
					{
						WeaponDesignElement weaponDesignElement = craftedWeaponData.UsedPieces[i];
						if (weaponDesignElement.IsValid)
						{
							float scaledPieceOffset = weaponDesignElement.ScaledPieceOffset;
							float num4 = craftedWeaponData.PiecePivotDistances[i];
							float num5 = num4 + scaledPieceOffset - weaponDesignElement.ScaledDistanceToPreviousPiece;
							float num6 = num4 - scaledPieceOffset + weaponDesignElement.ScaledDistanceToNextPiece;
							num2 = TaleWorlds.Library.MathF.Min(num5, num2);
							if (num6 > num3)
							{
								num3 = num6;
								z = (num6 + num5) * 0.5f;
							}
						}
					}
					WeaponDesignElement weaponDesignElement2 = craftedWeaponData.UsedPieces[2];
					if (weaponDesignElement2.IsValid)
					{
						float scaledPieceOffset2 = weaponDesignElement2.ScaledPieceOffset;
						num2 -= scaledPieceOffset2;
					}
					physicsShape2.AddCapsule(new CapsuleData(0.035f, new Vec3(0f, 0f, craftedWeaponData.CraftedWeaponLength), new Vec3(0f, 0f, num2)));
					bool flag = false;
					if (craftedWeaponData.UsedPieces[1].IsValid)
					{
						float z2 = craftedWeaponData.PiecePivotDistances[1];
						physicsShape2.AddCapsule(new CapsuleData(0.05f, new Vec3(-0.1f, 0f, z2), new Vec3(0.1f, 0f, z2)));
						flag = true;
					}
					if (weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.OneHandedAxe || weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.TwoHandedAxe || weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.ThrowingAxe)
					{
						WeaponDesignElement weaponDesignElement3 = craftedWeaponData.UsedPieces[0];
						float num7 = craftedWeaponData.PiecePivotDistances[0];
						float z3 = num7 + weaponDesignElement3.CraftingPiece.Length * 0.8f;
						float z4 = num7 - weaponDesignElement3.CraftingPiece.Length * 0.8f;
						float z5 = num7 + weaponDesignElement3.CraftingPiece.Length;
						float z6 = num7 - weaponDesignElement3.CraftingPiece.Length;
						float bladeWidth = weaponDesignElement3.CraftingPiece.BladeData.BladeWidth;
						physicsShape2.AddCapsule(new CapsuleData(0.05f, new Vec3(0f, 0f, z3), new Vec3(0f - bladeWidth, 0f, z5)));
						physicsShape2.AddCapsule(new CapsuleData(0.05f, new Vec3(0f, 0f, z4), new Vec3(0f - bladeWidth, 0f, z6)));
						physicsShape2.AddCapsule(new CapsuleData(0.05f, new Vec3(0f - bladeWidth, 0f, z5), new Vec3(0f - bladeWidth, 0f, z6)));
						flag = true;
					}
					if (weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.TwoHandedPolearm || weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.Javelin)
					{
						float z7 = craftedWeaponData.PiecePivotDistances[0];
						physicsShape2.AddCapsule(new CapsuleData(0.025f, new Vec3(-0.05f, 0f, z7), new Vec3(0.05f, 0f, z7)));
						flag = true;
					}
					if (!flag)
					{
						physicsShape2.AddCapsule(new CapsuleData(0.025f, new Vec3(-0.05f, 0f, z), new Vec3(0.05f, 0f, z)));
					}
				}
				else
				{
					weaponData.Shape.Prepare();
					int num8 = physicsShape.CapsuleCount();
					if (num8 == 0)
					{
						TaleWorlds.Library.Debug.FailedAssert("Item has 0 body parts. Applying a default body, but this should not happen! Check this!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", "RecalculateBody", 2752);
						return;
					}
					switch (weaponComponent.PrimaryWeapon.WeaponClass)
					{
					case WeaponClass.Dagger:
					case WeaponClass.OneHandedSword:
					case WeaponClass.TwoHandedSword:
					case WeaponClass.ThrowingKnife:
					{
						CapsuleData data3 = default(CapsuleData);
						physicsShape2.GetCapsule(ref data3, 0);
						float radius3 = data3.Radius;
						Vec3 p5 = data3.P1;
						Vec3 p6 = data3.P2;
						physicsShape2.SetCapsule(new CapsuleData(radius3, new Vec3(p5.x, p5.y, p5.z * num), p6), 0);
						break;
					}
					case WeaponClass.OneHandedAxe:
					case WeaponClass.TwoHandedAxe:
					case WeaponClass.Mace:
					case WeaponClass.TwoHandedMace:
					case WeaponClass.OneHandedPolearm:
					case WeaponClass.TwoHandedPolearm:
					case WeaponClass.LowGripPolearm:
					case WeaponClass.Arrow:
					case WeaponClass.Bolt:
					case WeaponClass.Crossbow:
					case WeaponClass.ThrowingAxe:
					case WeaponClass.Javelin:
					case WeaponClass.Banner:
					{
						CapsuleData data = default(CapsuleData);
						physicsShape2.GetCapsule(ref data, 0);
						float radius = data.Radius;
						Vec3 p = data.P1;
						Vec3 p2 = data.P2;
						physicsShape2.SetCapsule(new CapsuleData(radius, new Vec3(p.x, p.y, p.z * num), p2), 0);
						for (int j = 1; j < num8; j++)
						{
							CapsuleData data2 = default(CapsuleData);
							physicsShape2.GetCapsule(ref data2, j);
							float radius2 = data2.Radius;
							Vec3 p3 = data2.P1;
							Vec3 p4 = data2.P2;
							physicsShape2.SetCapsule(new CapsuleData(radius2, new Vec3(p3.x, p3.y, p3.z * num), new Vec3(p4.x, p4.y, p4.z * num)), j);
						}
						break;
					}
					case WeaponClass.SmallShield:
					case WeaponClass.LargeShield:
						TaleWorlds.Library.Debug.FailedAssert("Shields should not have recalculate body flag.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", "RecalculateBody", 2826);
						break;
					}
				}
			}
		}
		weaponData.CenterOfMassShift = weaponData.Shape.GetWeaponCenterOfMass();
	}

	[UsedImplicitly]
	[MBCallback]
	internal void OnPreTick(float dt)
	{
		waitTickCompletion();
		for (int num = MissionBehaviors.Count - 1; num >= 0; num--)
		{
			MissionBehaviors[num].OnPreMissionTick(dt);
		}
		TickDebugAgents();
	}

	[UsedImplicitly]
	[MBCallback]
	internal void ApplySkeletonScaleToAllEquippedItems(string itemName)
	{
		int count = Agents.Count;
		for (int i = 0; i < count; i++)
		{
			for (int j = 0; j < 12; j++)
			{
				EquipmentElement equipmentElement = Agents[i].SpawnEquipment[j];
				if (!equipmentElement.IsEmpty && equipmentElement.Item.StringId == itemName && equipmentElement.Item.HorseComponent?.SkeletonScale != null)
				{
					Agents[i].AgentVisuals.ApplySkeletonScale(equipmentElement.Item.HorseComponent.SkeletonScale.MountSitBoneScale, equipmentElement.Item.HorseComponent.SkeletonScale.MountRadiusAdder, equipmentElement.Item.HorseComponent.SkeletonScale.BoneIndices, equipmentElement.Item.HorseComponent.SkeletonScale.Scales);
					break;
				}
			}
		}
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_facial_anim_to_agent", "mission")]
	public static string SetFacialAnimToAgent(List<string> strings)
	{
		Mission current = Current;
		if (current != null)
		{
			if (strings.Count != 2)
			{
				return "Enter agent index and animation name please";
			}
			if (int.TryParse(strings[0], out var result) && result >= 0)
			{
				foreach (Agent agent in current.Agents)
				{
					if (agent.Index == result)
					{
						agent.SetAgentFacialAnimation(Agent.FacialAnimChannel.High, strings[1], loop: true);
						return "Done";
					}
				}
			}
			return "Please enter a valid agent index";
		}
		return "Mission could not be found";
	}

	private void waitTickCompletion()
	{
		while (!tickCompleted)
		{
			Thread.Sleep(1);
		}
	}

	public void TickAgentsAndTeamsImp(float dt)
	{
		foreach (Agent allAgent in AllAgents)
		{
			allAgent.Tick(dt);
		}
		foreach (Team team in Teams)
		{
			team.Tick(dt);
		}
		tickCompleted = true;
		foreach (MBSubModuleBase subModule in Module.GetInstance().SubModules)
		{
			subModule.AfterAsyncTickTick(dt);
		}
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("formation_speed_adjustment_enabled", "ai")]
	public static string EnableSpeedAdjustmentCommand(List<string> strings)
	{
		if (!GameNetwork.IsSessionActive)
		{
			HumanAIComponent.FormationSpeedAdjustmentEnabled = !HumanAIComponent.FormationSpeedAdjustmentEnabled;
			string text = "Speed Adjustment ";
			if (HumanAIComponent.FormationSpeedAdjustmentEnabled)
			{
				return text + "enabled";
			}
			return text + "disabled";
		}
		return "Does not work on multiplayer.";
	}

	public void OnTick(float dt, float realDt, bool updateCamera, bool doAsyncAITick)
	{
		ApplyGeneratedCombatLogs();
		if (InputManager == null)
		{
			InputManager = new EmptyInputContext();
		}
		MissionTimeTracker.Tick(dt);
		CheckMissionEnd(CurrentTime);
		if (IsFastForward && MissionEnded)
		{
			IsFastForward = false;
		}
		if (CurrentState != State.Continuing)
		{
			return;
		}
		if (_inMissionLoadingScreenTimer != null && _inMissionLoadingScreenTimer.Check(CurrentTime))
		{
			_inMissionLoadingScreenTimer = null;
			_onLoadingEndedAction?.Invoke();
			LoadingWindow.DisableGlobalLoadingWindow();
		}
		for (int num = MissionBehaviors.Count - 1; num >= 0; num--)
		{
			MissionBehaviors[num].OnPreDisplayMissionTick(dt);
		}
		if (!GameNetwork.IsDedicatedServer && updateCamera)
		{
			_missionState.Handler.UpdateCamera(this, realDt);
		}
		tickCompleted = false;
		for (int num2 = MissionBehaviors.Count - 1; num2 >= 0; num2--)
		{
			MissionBehaviors[num2].OnMissionTick(dt);
		}
		for (int num3 = _dynamicEntities.Count - 1; num3 >= 0; num3--)
		{
			DynamicEntityInfo dynamicEntityInfo = _dynamicEntities[num3];
			if (dynamicEntityInfo.TimerToDisable.Check(CurrentTime))
			{
				dynamicEntityInfo.Entity.RemoveEnginePhysics();
				dynamicEntityInfo.Entity.Remove(79);
				_dynamicEntities.RemoveAt(num3);
			}
		}
		HandleSpawnedItems();
		DebugNetworkEventStatistics.EndTick(dt);
		if (CurrentState == State.Continuing && IsFriendlyMission && !IsInPhotoMode)
		{
			if (InputManager.IsGameKeyDown(4))
			{
				OnEndMissionRequest();
			}
			else
			{
				_leaveMissionTimer = null;
			}
		}
		if (doAsyncAITick)
		{
			TickAgentsAndTeamsAsync(dt);
		}
		else
		{
			TickAgentsAndTeamsImp(dt);
		}
	}

	public void RemoveSpawnedItemsAndMissiles()
	{
		ClearMissiles();
		_missiles.Clear();
		RemoveSpawnedMissionObjects();
	}

	public void AfterStart()
	{
		_activeAgents.Clear();
		_allAgents.Clear();
		foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
		{
			subModule.OnBeforeMissionBehaviorInitialize(this);
		}
		for (int i = 0; i < MissionBehaviors.Count; i++)
		{
			MissionBehaviors[i].OnBehaviorInitialize();
		}
		foreach (MBSubModuleBase subModule2 in Module.CurrentModule.SubModules)
		{
			subModule2.OnMissionBehaviorInitialize(this);
		}
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.EarlyStart();
		}
		_battleSpawnPathSelector.Initialize();
		_deploymentPlan.CreateReinforcementPlans();
		foreach (MissionBehavior missionBehavior2 in MissionBehaviors)
		{
			missionBehavior2.AfterStart();
		}
		foreach (MissionObject missionObject in MissionObjects)
		{
			missionObject.AfterMissionStart();
		}
		if (MissionGameModels.Current.ApplyWeatherEffectsModel != null)
		{
			MissionGameModels.Current.ApplyWeatherEffectsModel.ApplyWeatherEffects();
		}
		CurrentState = State.Continuing;
	}

	public void OnEndMissionRequest()
	{
		foreach (MissionLogic missionLogic in MissionLogics)
		{
			bool canLeave;
			InquiryData inquiryData = missionLogic.OnEndMissionRequest(out canLeave);
			if (!canLeave)
			{
				_leaveMissionTimer = null;
				return;
			}
			if (inquiryData != null)
			{
				_leaveMissionTimer = null;
				InformationManager.ShowInquiry(inquiryData, pauseGameActiveState: true);
				return;
			}
		}
		if (_leaveMissionTimer != null)
		{
			if (_leaveMissionTimer.ElapsedTime > 0.6f)
			{
				_leaveMissionTimer = null;
				EndMission();
			}
		}
		else
		{
			_leaveMissionTimer = new BasicMissionTimer();
		}
	}

	public float GetMissionEndTimeInSeconds()
	{
		return 0.6f;
	}

	public float GetMissionEndTimerValue()
	{
		if (_leaveMissionTimer == null)
		{
			return -1f;
		}
		return _leaveMissionTimer.ElapsedTime;
	}

	private void ApplyGeneratedCombatLogs()
	{
		if (!_combatLogsCreated.IsEmpty)
		{
			CombatLogData result;
			while (_combatLogsCreated.TryDequeue(out result))
			{
				CombatLogManager.GenerateCombatLog(result);
			}
		}
	}

	public int GetMemberCountOfSide(BattleSideEnum side)
	{
		int num = 0;
		foreach (Team team in Teams)
		{
			if (team.Side == side)
			{
				num += team.ActiveAgents.Count;
			}
		}
		return num;
	}

	public Path GetInitialSpawnPath()
	{
		return _battleSpawnPathSelector.InitialPath;
	}

	public SpawnPathData GetInitialSpawnPathDataOfSide(BattleSideEnum battleSide)
	{
		_battleSpawnPathSelector.GetInitialPathDataOfSide(battleSide, out var pathPathData);
		return pathPathData;
	}

	public MBReadOnlyList<SpawnPathData> GetReinforcementPathsDataOfSide(BattleSideEnum battleSide)
	{
		return _battleSpawnPathSelector.GetReinforcementPathsDataOfSide(battleSide);
	}

	public void GetTroopSpawnFrameWithIndex(AgentBuildData buildData, int troopSpawnIndex, int troopSpawnCount, out Vec3 troopSpawnPosition, out Vec2 troopSpawnDirection)
	{
		Formation agentFormation = buildData.AgentFormation;
		BasicCharacterObject agentCharacter = buildData.AgentCharacter;
		troopSpawnPosition = Vec3.Invalid;
		WorldPosition spawnPosition;
		Vec2 spawnDirection;
		if (buildData.AgentSpawnsIntoOwnFormation)
		{
			spawnPosition = agentFormation.CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.GroundVec3);
			spawnDirection = agentFormation.Direction;
		}
		else
		{
			IAgentOriginBase agentOrigin = buildData.AgentOrigin;
			bool agentIsReinforcement = buildData.AgentIsReinforcement;
			BattleSideEnum side = buildData.AgentTeam.Side;
			if (buildData.AgentSpawnsUsingOwnTroopClass)
			{
				FormationClass agentTroopClass = GetAgentTroopClass(side, agentCharacter);
				GetFormationSpawnFrame(side, agentTroopClass, agentIsReinforcement, out spawnPosition, out spawnDirection);
			}
			else if (agentCharacter.IsHero && agentOrigin != null && agentOrigin.BattleCombatant != null && agentCharacter == agentOrigin.BattleCombatant.General && GetFormationSpawnClass(side, FormationClass.NumberOfRegularFormations, agentIsReinforcement) == FormationClass.NumberOfRegularFormations)
			{
				GetFormationSpawnFrame(side, FormationClass.NumberOfRegularFormations, agentIsReinforcement, out spawnPosition, out spawnDirection);
			}
			else
			{
				GetFormationSpawnFrame(side, agentFormation.FormationIndex, agentIsReinforcement, out spawnPosition, out spawnDirection);
			}
		}
		bool isMountedFormation = !buildData.AgentNoHorses && agentFormation.HasAnyMountedUnit;
		agentFormation.GetUnitSpawnFrameWithIndex(troopSpawnIndex, in spawnPosition, in spawnDirection, agentFormation.Width, troopSpawnCount, agentFormation.UnitSpacing, isMountedFormation, out var unitSpawnPosition, out var unitSpawnDirection);
		if (unitSpawnPosition.HasValue && buildData.MakeUnitStandOutDistance != 0f)
		{
			unitSpawnPosition.Value.SetVec2(unitSpawnPosition.Value.AsVec2 + unitSpawnDirection.Value * buildData.MakeUnitStandOutDistance);
		}
		if (unitSpawnPosition.HasValue)
		{
			if (unitSpawnPosition.Value.GetNavMesh() == UIntPtr.Zero)
			{
				troopSpawnPosition = Scene.GetLastPointOnNavigationMeshFromWorldPositionToDestination(ref spawnPosition, unitSpawnPosition.Value.AsVec2);
			}
			else
			{
				troopSpawnPosition = unitSpawnPosition.Value.GetGroundVec3();
			}
		}
		if (!troopSpawnPosition.IsValid)
		{
			troopSpawnPosition = spawnPosition.GetGroundVec3();
		}
		troopSpawnDirection = (unitSpawnDirection.HasValue ? unitSpawnDirection.Value : spawnDirection);
	}

	public void GetFormationSpawnFrame(BattleSideEnum side, FormationClass formationClass, bool isReinforcement, out WorldPosition spawnPosition, out Vec2 spawnDirection)
	{
		DeploymentPlanType planType = (isReinforcement ? DeploymentPlanType.Reinforcement : DeploymentPlanType.Initial);
		IFormationDeploymentPlan formationPlan = _deploymentPlan.GetFormationPlan(side, formationClass, planType);
		spawnPosition = formationPlan.CreateNewDeploymentWorldPosition(WorldPosition.WorldPositionEnforcedCache.GroundVec3);
		spawnDirection = formationPlan.GetDirection();
	}

	public WorldFrame GetBattleSideInitialSpawnPathFrame(BattleSideEnum battleSide, float pathOffset = 0f)
	{
		SpawnPathData initialSpawnPathDataOfSide = GetInitialSpawnPathDataOfSide(battleSide);
		if (initialSpawnPathDataOfSide.IsValid)
		{
			initialSpawnPathDataOfSide.GetOrientedSpawnPathPosition(out var spawnPathPosition, out var spawnPathDirection, pathOffset);
			Mat3 identity = Mat3.Identity;
			identity.RotateAboutUp(spawnPathDirection.RotationInRadians);
			return new WorldFrame(origin: new WorldPosition(Scene, UIntPtr.Zero, spawnPathPosition.ToVec3(), hasValidZ: false), rotation: identity);
		}
		return WorldFrame.Invalid;
	}

	private void BuildAgent(Agent agent, AgentBuildData agentBuildData)
	{
		if (agent == null)
		{
			throw new MBNullParameterException("agent");
		}
		agent.Build(agentBuildData, _agentCreationIndex);
		_agentCreationIndex++;
		if (!agent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot].IsEmpty)
		{
			EquipmentElement equipmentElement = agent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot];
			if (equipmentElement.Item.HorseComponent.BodyLength != 0)
			{
				agent.SetInitialAgentScale(0.01f * (float)equipmentElement.Item.HorseComponent.BodyLength);
			}
		}
		agent.EquipItemsFromSpawnEquipment(neededBatchedItems: true);
		agent.InitializeAgentRecord();
		agent.AgentVisuals.BatchLastLodMeshes();
		agent.PreloadForRendering();
		ActionIndexValueCache currentActionValue = agent.GetCurrentActionValue(0);
		if (currentActionValue != ActionIndexValueCache.act_none)
		{
			agent.SetActionChannel(0, currentActionValue, ignorePriority: false, 0uL, 0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloat * 0.8f);
		}
		agent.InitializeComponents();
		if (agent.Controller == Agent.ControllerType.Player)
		{
			ResetFirstThirdPersonView();
		}
		_activeAgents.Add(agent);
		_allAgents.Add(agent);
	}

	private Agent CreateAgent(Monster monster, bool isFemale, int instanceNo, Agent.CreationType creationType, float stepSize, int forcedAgentIndex, int weight, BasicCharacterObject characterObject)
	{
		AnimationSystemData animationSystemData = monster.FillAnimationSystemData(stepSize, hasClippingPlane: false, isFemale);
		AgentCapsuleData capsuleData = monster.FillCapsuleData();
		AgentSpawnData spawnData = monster.FillSpawnData(null);
		AgentCreationResult creationResult = CreateAgentInternal(monster.Flags, forcedAgentIndex, isFemale, ref spawnData, ref capsuleData, ref animationSystemData, instanceNo);
		Agent agent = new Agent(this, creationResult, creationType, monster);
		agent.Character = characterObject;
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnAgentCreated(agent);
		}
		return agent;
	}

	public void SetBattleAgentCount(int agentCount)
	{
		if (_agentCount == 0 || _agentCount > agentCount)
		{
			_agentCount = agentCount;
		}
	}

	public Vec2 GetFormationSpawnPosition(BattleSideEnum side, FormationClass formationClass, bool isReinforcement)
	{
		DeploymentPlanType planType = (isReinforcement ? DeploymentPlanType.Reinforcement : DeploymentPlanType.Initial);
		return _deploymentPlan.GetFormationPlan(side, formationClass, planType).CreateNewDeploymentWorldPosition(WorldPosition.WorldPositionEnforcedCache.None).AsVec2;
	}

	public FormationClass GetFormationSpawnClass(BattleSideEnum side, FormationClass formationClass, bool isReinforcement)
	{
		DeploymentPlanType planType = (isReinforcement ? DeploymentPlanType.Reinforcement : DeploymentPlanType.Initial);
		return _deploymentPlan.GetFormationPlan(side, formationClass, planType).SpawnClass;
	}

	public Agent SpawnAgent(AgentBuildData agentBuildData, bool spawnFromAgentVisuals = false)
	{
		BasicCharacterObject agentCharacter = agentBuildData.AgentCharacter;
		if (agentCharacter == null)
		{
			throw new MBNullParameterException("npcCharacterObject");
		}
		int forcedAgentIndex = -1;
		if (agentBuildData.AgentIndexOverriden)
		{
			forcedAgentIndex = agentBuildData.AgentIndex;
		}
		Agent agent = CreateAgent(agentBuildData.AgentMonster, agentBuildData.GenderOverriden ? agentBuildData.AgentIsFemale : agentCharacter.IsFemale, 0, Agent.CreationType.FromCharacterObj, agentCharacter.GetStepSize(), forcedAgentIndex, agentBuildData.AgentMonster.Weight, agentCharacter);
		agent.FormationPositionPreference = agentCharacter.FormationPositionPreference;
		float num = (agentBuildData.AgeOverriden ? ((float)agentBuildData.AgentAge) : agentCharacter.Age);
		if (num == 0f)
		{
			agentBuildData.Age(29);
		}
		else if (MBBodyProperties.GetMaturityType(num) < BodyMeshMaturityType.Teenager && (Mode == MissionMode.Battle || Mode == MissionMode.Duel || Mode == MissionMode.Tournament || Mode == MissionMode.Stealth))
		{
			agentBuildData.Age(27);
		}
		if (agentBuildData.BodyPropertiesOverriden)
		{
			agent.UpdateBodyProperties(agentBuildData.AgentBodyProperties);
			if (!agentBuildData.AgeOverriden)
			{
				agent.Age = agentCharacter.Age;
			}
		}
		agent.BodyPropertiesSeed = agentBuildData.AgentEquipmentSeed;
		if (agentBuildData.AgeOverriden)
		{
			agent.Age = agentBuildData.AgentAge;
		}
		if (agentBuildData.GenderOverriden)
		{
			agent.IsFemale = agentBuildData.AgentIsFemale;
		}
		agent.SetTeam(agentBuildData.AgentTeam, sync: false);
		agent.SetClothingColor1(agentBuildData.AgentClothingColor1);
		agent.SetClothingColor2(agentBuildData.AgentClothingColor2);
		agent.SetRandomizeColors(agentBuildData.RandomizeColors);
		agent.Origin = agentBuildData.AgentOrigin;
		Formation agentFormation = agentBuildData.AgentFormation;
		if (agentFormation != null && !agentFormation.HasBeenPositioned)
		{
			SetFormationPositioningFromDeploymentPlan(agentFormation);
		}
		if (!agentBuildData.AgentInitialPosition.HasValue)
		{
			BattleSideEnum side = agentBuildData.AgentTeam.Side;
			Vec3 troopSpawnPosition = Vec3.Invalid;
			Vec2 spawnDirection = Vec2.Invalid;
			if (agentCharacter == Game.Current.PlayerTroop && _deploymentPlan.HasPlayerSpawnFrame(side))
			{
				_deploymentPlan.GetPlayerSpawnFrame(side, out var position, out var direction);
				troopSpawnPosition = position.GetGroundVec3();
				spawnDirection = direction;
			}
			else if (agentFormation != null)
			{
				int num2;
				int num3;
				if (agentBuildData.AgentSpawnsIntoOwnFormation)
				{
					num2 = agentFormation.CountOfUnits;
					num3 = num2 + 1;
				}
				else if (agentBuildData.AgentFormationTroopSpawnIndex >= 0 && agentBuildData.AgentFormationTroopSpawnCount > 0)
				{
					num2 = agentBuildData.AgentFormationTroopSpawnIndex;
					num3 = agentBuildData.AgentFormationTroopSpawnCount;
				}
				else
				{
					num2 = agentFormation.GetNextSpawnIndex();
					num3 = num2 + 1;
				}
				if (num2 >= num3)
				{
					num3 = num2 + 1;
				}
				GetTroopSpawnFrameWithIndex(agentBuildData, num2, num3, out troopSpawnPosition, out spawnDirection);
			}
			else
			{
				GetFormationSpawnFrame(side, FormationClass.NumberOfAllFormations, agentBuildData.AgentIsReinforcement, out var spawnPosition, out spawnDirection);
				troopSpawnPosition = spawnPosition.GetGroundVec3();
			}
			agentBuildData.InitialPosition(in troopSpawnPosition).InitialDirection(in spawnDirection);
		}
		Vec3 initialPosition = agentBuildData.AgentInitialPosition.GetValueOrDefault();
		Vec2 initialDirection = agentBuildData.AgentInitialDirection.GetValueOrDefault();
		agent.SetInitialFrame(in initialPosition, in initialDirection, agentBuildData.AgentCanSpawnOutsideOfMissionBoundary);
		if (agentCharacter.AllEquipments == null)
		{
			TaleWorlds.Library.Debug.Print("characterObject.AllEquipments is null for \"" + agentCharacter.StringId + "\".");
		}
		if (agentCharacter.AllEquipments != null && agentCharacter.AllEquipments.Any((Equipment eq) => eq == null))
		{
			TaleWorlds.Library.Debug.Print("Character with id \"" + agentCharacter.StringId + "\" has a null equipment in its AllEquipments.");
		}
		if (agentCharacter.AllEquipments.Where((Equipment eq) => eq.IsCivilian) == null)
		{
			agentBuildData.CivilianEquipment(civilianEquipment: false);
		}
		if (agentCharacter.IsHero)
		{
			agentBuildData.FixedEquipment(fixedEquipment: true);
		}
		Equipment equipment = ((agentBuildData.AgentOverridenSpawnEquipment != null) ? agentBuildData.AgentOverridenSpawnEquipment.Clone() : (agentBuildData.AgentFixedEquipment ? agentCharacter.GetFirstEquipment(agentBuildData.AgentCivilianEquipment).Clone() : Equipment.GetRandomEquipmentElements(agent.Character, !Game.Current.GameType.IsCoreOnlyGameMode, agentBuildData.AgentCivilianEquipment, agentBuildData.AgentEquipmentSeed)));
		Agent agent2 = null;
		if (agentBuildData.AgentNoHorses)
		{
			equipment[EquipmentIndex.ArmorItemEndSlot] = default(EquipmentElement);
			equipment[EquipmentIndex.HorseHarness] = default(EquipmentElement);
		}
		if (agentBuildData.AgentNoWeapons)
		{
			equipment[EquipmentIndex.WeaponItemBeginSlot] = default(EquipmentElement);
			equipment[EquipmentIndex.Weapon1] = default(EquipmentElement);
			equipment[EquipmentIndex.Weapon2] = default(EquipmentElement);
			equipment[EquipmentIndex.Weapon3] = default(EquipmentElement);
			equipment[EquipmentIndex.ExtraWeaponSlot] = default(EquipmentElement);
		}
		if (agentCharacter.IsHero)
		{
			ItemObject itemObject = null;
			ItemObject item = equipment[EquipmentIndex.ExtraWeaponSlot].Item;
			if (item != null && item.IsBannerItem && item.BannerComponent != null)
			{
				itemObject = item;
				equipment[EquipmentIndex.ExtraWeaponSlot] = default(EquipmentElement);
			}
			else if (agentBuildData.AgentBannerItem != null)
			{
				itemObject = agentBuildData.AgentBannerItem;
			}
			if (itemObject != null)
			{
				agent.SetFormationBanner(itemObject);
			}
		}
		else if (agentBuildData.AgentBannerItem != null)
		{
			equipment[EquipmentIndex.Weapon1] = default(EquipmentElement);
			equipment[EquipmentIndex.Weapon2] = default(EquipmentElement);
			equipment[EquipmentIndex.Weapon3] = default(EquipmentElement);
			if (agentBuildData.AgentBannerReplacementWeaponItem != null)
			{
				equipment[EquipmentIndex.WeaponItemBeginSlot] = new EquipmentElement(agentBuildData.AgentBannerReplacementWeaponItem);
			}
			else
			{
				equipment[EquipmentIndex.WeaponItemBeginSlot] = default(EquipmentElement);
			}
			equipment[EquipmentIndex.ExtraWeaponSlot] = new EquipmentElement(agentBuildData.AgentBannerItem);
			if (agentBuildData.AgentOverridenSpawnMissionEquipment != null)
			{
				agentBuildData.AgentOverridenSpawnMissionEquipment[EquipmentIndex.ExtraWeaponSlot] = new MissionWeapon(agentBuildData.AgentBannerItem, null, agentBuildData.AgentBanner);
			}
		}
		if (agentBuildData.AgentNoArmor)
		{
			equipment[EquipmentIndex.Gloves] = default(EquipmentElement);
			equipment[EquipmentIndex.Body] = default(EquipmentElement);
			equipment[EquipmentIndex.Cape] = default(EquipmentElement);
			equipment[EquipmentIndex.NumAllWeaponSlots] = default(EquipmentElement);
			equipment[EquipmentIndex.Leg] = default(EquipmentElement);
		}
		for (int i = 0; i < 5; i++)
		{
			if (!equipment[(EquipmentIndex)i].IsEmpty && equipment[(EquipmentIndex)i].Item.ItemFlags.HasAnyFlag(ItemFlags.CannotBePickedUp))
			{
				equipment[(EquipmentIndex)i] = default(EquipmentElement);
			}
		}
		agent.InitializeSpawnEquipment(equipment);
		agent.InitializeMissionEquipment(agentBuildData.AgentOverridenSpawnMissionEquipment, agentBuildData.AgentBanner);
		if (agent.RandomizeColors)
		{
			agent.Equipment.SetGlossMultipliersOfWeaponsRandomly(agentBuildData.AgentEquipmentSeed);
		}
		ItemObject item2 = equipment[EquipmentIndex.ArmorItemEndSlot].Item;
		if (item2 != null && item2.HasHorseComponent && item2.HorseComponent.IsRideable)
		{
			int forcedAgentMountIndex = -1;
			if (agentBuildData.AgentMountIndexOverriden)
			{
				forcedAgentMountIndex = agentBuildData.AgentMountIndex;
			}
			EquipmentElement mount = equipment[EquipmentIndex.ArmorItemEndSlot];
			EquipmentElement mountHarness = equipment[EquipmentIndex.HorseHarness];
			initialPosition = agentBuildData.AgentInitialPosition.GetValueOrDefault();
			initialDirection = agentBuildData.AgentInitialDirection.GetValueOrDefault();
			agent2 = CreateHorseAgentFromRosterElements(mount, mountHarness, in initialPosition, in initialDirection, forcedAgentMountIndex, agentBuildData.AgentMountKey);
			Equipment spawnEquipment = new Equipment
			{
				[EquipmentIndex.ArmorItemEndSlot] = equipment[EquipmentIndex.ArmorItemEndSlot],
				[EquipmentIndex.HorseHarness] = equipment[EquipmentIndex.HorseHarness]
			};
			agent2.InitializeSpawnEquipment(spawnEquipment);
			agent.SetMountAgentBeforeBuild(agent2);
		}
		if (spawnFromAgentVisuals || !GameNetwork.IsClientOrReplay)
		{
			agent.Equipment.CheckLoadedAmmos();
		}
		if (!agentBuildData.BodyPropertiesOverriden)
		{
			agent.UpdateBodyProperties(agentCharacter.GetBodyProperties(equipment, agentBuildData.AgentEquipmentSeed));
		}
		if (GameNetwork.IsServerOrRecorder && agent.RiderAgent == null)
		{
			Vec3 valueOrDefault = agentBuildData.AgentInitialPosition.GetValueOrDefault();
			Vec2 valueOrDefault2 = agentBuildData.AgentInitialDirection.GetValueOrDefault();
			if (agent.IsMount)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new CreateFreeMountAgent(agent, valueOrDefault, valueOrDefault2));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
			}
			else
			{
				bool flag = agentBuildData.AgentMissionPeer != null;
				NetworkCommunicator peer = (flag ? agentBuildData.AgentMissionPeer.GetNetworkPeer() : agentBuildData.OwningAgentMissionPeer?.GetNetworkPeer());
				bool flag2 = agent.MountAgent != null && agent.MountAgent.RiderAgent == agent;
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new CreateAgent(agent.Index, agent.Character, agent.Monster, agent.SpawnEquipment, agent.Equipment, agent.BodyPropertiesValue, agent.BodyPropertiesSeed, agent.IsFemale, agent.Team?.TeamIndex ?? (-1), agent.Formation?.Index ?? (-1), agent.ClothingColor1, agent.ClothingColor2, flag2 ? agent.MountAgent.Index : (-1), agent.MountAgent?.SpawnEquipment, flag, valueOrDefault, valueOrDefault2, peer));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
			}
		}
		MultiplayerMissionAgentVisualSpawnComponent missionBehavior = GetMissionBehavior<MultiplayerMissionAgentVisualSpawnComponent>();
		if (missionBehavior != null && agentBuildData.AgentMissionPeer != null && agentBuildData.AgentMissionPeer.IsMine && agentBuildData.AgentVisualsIndex == 0)
		{
			missionBehavior.OnMyAgentSpawned();
		}
		if (agent2 != null)
		{
			BuildAgent(agent2, agentBuildData);
			foreach (MissionBehavior missionBehavior2 in MissionBehaviors)
			{
				missionBehavior2.OnAgentBuild(agent2, null);
			}
		}
		BuildAgent(agent, agentBuildData);
		if (agentBuildData.AgentMissionPeer != null)
		{
			agent.MissionPeer = agentBuildData.AgentMissionPeer;
		}
		if (agentBuildData.OwningAgentMissionPeer != null)
		{
			agent.OwningAgentMissionPeer = agentBuildData.OwningAgentMissionPeer;
		}
		foreach (MissionBehavior missionBehavior3 in MissionBehaviors)
		{
			missionBehavior3.OnAgentBuild(agent, agentBuildData.AgentBanner ?? agentBuildData.AgentTeam?.Banner);
		}
		agent.AgentVisuals.CheckResources(addToQueue: true);
		if (agent.IsAIControlled)
		{
			if (agent2 == null)
			{
				AgentFlag agentFlags = agent.GetAgentFlags() & ~AgentFlag.CanRide;
				agent.SetAgentFlags(agentFlags);
			}
			else if (agent.Formation == null)
			{
				agent.SetRidingOrder(RidingOrder.RidingOrderEnum.Mount);
			}
		}
		return agent;
	}

	public void SetInitialAgentCountForSide(BattleSideEnum side, int agentCount)
	{
		if (side >= BattleSideEnum.Defender && side < BattleSideEnum.NumSides)
		{
			_initialAgentCountPerSide[(int)side] = agentCount;
		}
		else
		{
			TaleWorlds.Library.Debug.FailedAssert("Cannot set initial agent count.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", "SetInitialAgentCountForSide", 4146);
		}
	}

	public void SetFormationPositioningFromDeploymentPlan(Formation formation)
	{
		IFormationDeploymentPlan formationPlan = _deploymentPlan.GetFormationPlan(formation.Team.Side, formation.FormationIndex, DeploymentPlanType.Initial);
		if (formationPlan.PlannedTroopCount > 0 && formationPlan.HasDimensions)
		{
			formation.FormOrder = FormOrder.FormOrderCustom(formationPlan.PlannedWidth);
		}
		formation.SetPositioning(formationPlan.CreateNewDeploymentWorldPosition(WorldPosition.WorldPositionEnforcedCache.None), formationPlan.GetDirection());
	}

	public Agent SpawnMonster(ItemRosterElement rosterElement, ItemRosterElement harnessRosterElement, in Vec3 initialPosition, in Vec2 initialDirection, int forcedAgentIndex = -1)
	{
		return SpawnMonster(rosterElement.EquipmentElement, harnessRosterElement.EquipmentElement, in initialPosition, in initialDirection, forcedAgentIndex);
	}

	public Agent SpawnMonster(EquipmentElement equipmentElement, EquipmentElement harnessRosterElement, in Vec3 initialPosition, in Vec2 initialDirection, int forcedAgentIndex = -1)
	{
		Agent agent = CreateHorseAgentFromRosterElements(equipmentElement, harnessRosterElement, in initialPosition, in initialDirection, forcedAgentIndex, MountCreationKey.GetRandomMountKeyString(equipmentElement.Item, MBRandom.RandomInt()));
		Equipment spawnEquipment = new Equipment
		{
			[EquipmentIndex.ArmorItemEndSlot] = equipmentElement,
			[EquipmentIndex.HorseHarness] = harnessRosterElement
		};
		agent.InitializeSpawnEquipment(spawnEquipment);
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new CreateFreeMountAgent(agent, initialPosition, initialDirection));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		BuildAgent(agent, null);
		return agent;
	}

	public Agent SpawnTroop(IAgentOriginBase troopOrigin, bool isPlayerSide, bool hasFormation, bool spawnWithHorse, bool isReinforcement, int formationTroopCount, int formationTroopIndex, bool isAlarmed, bool wieldInitialWeapons, bool forceDismounted, Vec3? initialPosition, Vec2? initialDirection, string specialActionSetSuffix = null, ItemObject bannerItem = null, FormationClass formationIndex = FormationClass.NumberOfAllFormations, bool useTroopClassForSpawn = false)
	{
		BasicCharacterObject troop = troopOrigin.Troop;
		Team agentTeam = GetAgentTeam(troopOrigin, isPlayerSide);
		if (troop.IsPlayerCharacter && !forceDismounted)
		{
			spawnWithHorse = true;
		}
		AgentBuildData agentBuildData = new AgentBuildData(troop).Team(agentTeam).Banner(troopOrigin.Banner).ClothingColor1(agentTeam.Color)
			.ClothingColor2(agentTeam.Color2)
			.TroopOrigin(troopOrigin)
			.NoHorses(!spawnWithHorse)
			.CivilianEquipment(DoesMissionRequireCivilianEquipment)
			.SpawnsUsingOwnTroopClass(useTroopClassForSpawn);
		if (hasFormation)
		{
			Formation formation = null;
			formation = ((formationIndex != FormationClass.NumberOfAllFormations) ? agentTeam.GetFormation(formationIndex) : agentTeam.GetFormation(GetAgentTroopClass(agentTeam.Side, troop)));
			agentBuildData.Formation(formation);
			agentBuildData.FormationTroopSpawnCount(formationTroopCount).FormationTroopSpawnIndex(formationTroopIndex);
		}
		if (!troop.IsPlayerCharacter)
		{
			agentBuildData.IsReinforcement(isReinforcement);
		}
		if (bannerItem != null)
		{
			if (bannerItem.IsBannerItem && bannerItem.BannerComponent != null)
			{
				agentBuildData.BannerItem(bannerItem);
				ItemObject bannerBearerReplacementWeapon = MissionGameModels.Current.BattleBannerBearersModel.GetBannerBearerReplacementWeapon(troop);
				agentBuildData.BannerReplacementWeaponItem(bannerBearerReplacementWeapon);
			}
			else
			{
				TaleWorlds.Library.Debug.FailedAssert(string.Concat("Passed banner item with name: ", bannerItem.Name, " is not a proper banner item"), "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", "SpawnTroop", 4253);
				TaleWorlds.Library.Debug.Print(string.Concat("Invalid banner item: ", bannerItem.Name, " is passed to a troop to be spawned"), 0, TaleWorlds.Library.Debug.DebugColor.Yellow);
			}
		}
		if (initialPosition.HasValue)
		{
			Vec3 position = initialPosition.Value;
			agentBuildData.InitialPosition(in position);
			Vec2 direction = initialDirection.Value;
			agentBuildData.InitialDirection(in direction);
		}
		if (spawnWithHorse)
		{
			agentBuildData.MountKey(MountCreationKey.GetRandomMountKeyString(troop.Equipment[EquipmentIndex.ArmorItemEndSlot].Item, troop.GetMountKeySeed()));
		}
		if (isPlayerSide && troop == Game.Current.PlayerTroop)
		{
			agentBuildData.Controller(Agent.ControllerType.Player);
		}
		Agent agent = SpawnAgent(agentBuildData);
		if (agent.Character.IsHero)
		{
			agent.SetAgentFlags(agent.GetAgentFlags() | AgentFlag.IsUnique);
		}
		if (agent.IsAIControlled && isAlarmed)
		{
			agent.SetWatchState(Agent.WatchState.Alarmed);
		}
		if (wieldInitialWeapons)
		{
			agent.WieldInitialWeapons();
		}
		if (!string.IsNullOrEmpty(specialActionSetSuffix))
		{
			AnimationSystemData animationSystemData = agentBuildData.AgentMonster.FillAnimationSystemData(MBGlobals.GetActionSetWithSuffix(agentBuildData.AgentMonster, agentBuildData.AgentIsFemale, specialActionSetSuffix), agent.Character.GetStepSize(), hasClippingPlane: false);
			agent.SetActionSet(ref animationSystemData);
		}
		return agent;
	}

	public Agent ReplaceBotWithPlayer(Agent botAgent, MissionPeer missionPeer)
	{
		if (!GameNetwork.IsClientOrReplay && botAgent != null)
		{
			if (GameNetwork.IsServer)
			{
				NetworkCommunicator networkPeer = missionPeer.GetNetworkPeer();
				if (!networkPeer.IsServerPeer)
				{
					GameNetwork.BeginModuleEventAsServer(networkPeer);
					GameNetwork.WriteMessage(new ReplaceBotWithPlayer(networkPeer, botAgent.Index, botAgent.Health, botAgent.MountAgent?.Health ?? (-1f)));
					GameNetwork.EndModuleEventAsServer();
				}
			}
			if (botAgent.Formation != null)
			{
				botAgent.Formation.PlayerOwner = botAgent;
			}
			botAgent.OwningAgentMissionPeer = null;
			botAgent.MissionPeer = missionPeer;
			botAgent.Formation = missionPeer.ControlledFormation;
			AgentFlag agentFlags = botAgent.GetAgentFlags();
			if (!agentFlags.HasAnyFlag(AgentFlag.CanRide))
			{
				botAgent.SetAgentFlags(agentFlags | AgentFlag.CanRide);
			}
			missionPeer.BotsUnderControlAlive--;
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new BotsControlledChange(missionPeer.GetNetworkPeer(), missionPeer.BotsUnderControlAlive, missionPeer.BotsUnderControlTotal));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
			if (botAgent.Formation != null)
			{
				missionPeer.Team.AssignPlayerAsSergeantOfFormation(missionPeer, missionPeer.ControlledFormation.FormationIndex);
			}
			return botAgent;
		}
		return null;
	}

	private Agent CreateHorseAgentFromRosterElements(EquipmentElement mount, EquipmentElement mountHarness, in Vec3 initialPosition, in Vec2 initialDirection, int forcedAgentMountIndex, string horseCreationKey)
	{
		HorseComponent horseComponent = mount.Item.HorseComponent;
		Agent agent = CreateAgent(horseComponent.Monster, isFemale: false, 0, Agent.CreationType.FromHorseObj, 1f, forcedAgentMountIndex, (int)mount.Weight, null);
		agent.SetInitialFrame(in initialPosition, in initialDirection);
		agent.BaseHealthLimit = mount.GetModifiedMountHitPoints();
		agent.HealthLimit = agent.BaseHealthLimit;
		agent.Health = agent.HealthLimit;
		agent.SetMountInitialValues(mount.GetModifiedItemName(), horseCreationKey);
		return agent;
	}

	public void OnAgentInteraction(Agent requesterAgent, Agent targetAgent)
	{
		if (requesterAgent == Agent.Main && targetAgent.IsMount)
		{
			Agent.Main.Mount(targetAgent);
		}
		else
		{
			if (!targetAgent.IsHuman)
			{
				return;
			}
			foreach (MissionBehavior missionBehavior in MissionBehaviors)
			{
				missionBehavior.OnAgentInteraction(requesterAgent, targetAgent);
			}
		}
	}

	[UsedImplicitly]
	[MBCallback]
	public void EndMission()
	{
		TaleWorlds.Library.Debug.Print("I called EndMission", 0, TaleWorlds.Library.Debug.DebugColor.White, 17179869184uL);
		_missionEndTime = -1f;
		NextCheckTimeEndMission = -1f;
		MissionEnded = true;
		CurrentState = State.EndingNextFrame;
	}

	private void EndMissionInternal()
	{
		MBDebug.Print("I called EndMissionInternal", 0, TaleWorlds.Library.Debug.DebugColor.White, 17179869184uL);
		_deploymentPlan.ClearAll();
		IMissionListener[] array = _listeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnEndMission();
		}
		StopSoundEvents();
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnEndMissionInternal();
		}
		foreach (Agent agent in Agents)
		{
			agent.OnRemove();
		}
		foreach (Agent allAgent in AllAgents)
		{
			allAgent.OnDelete();
			allAgent.Clear();
		}
		Teams.Clear();
		foreach (MissionObject missionObject in MissionObjects)
		{
			missionObject.OnEndMission();
		}
		CurrentState = State.Over;
		FreeResources();
		FinalizeMission();
	}

	private void StopSoundEvents()
	{
		if (_ambientSoundEvent != null)
		{
			_ambientSoundEvent.Stop();
		}
	}

	public void AddMissionBehavior(MissionBehavior missionBehavior)
	{
		MissionBehaviors.Add(missionBehavior);
		missionBehavior.Mission = this;
		switch (missionBehavior.BehaviorType)
		{
		case MissionBehaviorType.Logic:
			MissionLogics.Add(missionBehavior as MissionLogic);
			break;
		case MissionBehaviorType.Other:
			_otherMissionBehaviors.Add(missionBehavior);
			break;
		}
		missionBehavior.OnCreated();
	}

	public T GetMissionBehavior<T>() where T : class, IMissionBehavior
	{
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			if (missionBehavior is T result)
			{
				return result;
			}
		}
		return null;
	}

	public void RemoveMissionBehavior(MissionBehavior missionBehavior)
	{
		missionBehavior.OnRemoveBehavior();
		switch (missionBehavior.BehaviorType)
		{
		case MissionBehaviorType.Logic:
			MissionLogics.Remove(missionBehavior as MissionLogic);
			break;
		case MissionBehaviorType.Other:
			_otherMissionBehaviors.Remove(missionBehavior);
			break;
		default:
			TaleWorlds.Library.Debug.FailedAssert("Invalid behavior type", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", "RemoveMissionBehavior", 4547);
			break;
		}
		MissionBehaviors.Remove(missionBehavior);
		missionBehavior.Mission = null;
	}

	public void JoinEnemyTeam()
	{
		if (PlayerTeam == DefenderTeam)
		{
			Agent leader = AttackerTeam.Leader;
			if (leader != null)
			{
				if (MainAgent != null && MainAgent.IsActive())
				{
					MainAgent.Controller = Agent.ControllerType.AI;
				}
				leader.Controller = Agent.ControllerType.Player;
				PlayerTeam = AttackerTeam;
			}
		}
		else if (PlayerTeam == AttackerTeam)
		{
			Agent leader2 = DefenderTeam.Leader;
			if (leader2 != null)
			{
				if (MainAgent != null && MainAgent.IsActive())
				{
					MainAgent.Controller = Agent.ControllerType.AI;
				}
				leader2.Controller = Agent.ControllerType.Player;
				PlayerTeam = DefenderTeam;
			}
		}
		else
		{
			TaleWorlds.Library.Debug.FailedAssert("Player is neither attacker nor defender.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", "JoinEnemyTeam", 4591);
		}
	}

	public void OnEndMissionResult()
	{
		MissionLogic[] array = MissionLogics.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnBattleEnded();
		}
		RetreatMission();
	}

	public bool IsAgentInteractionAllowed()
	{
		if (this.IsAgentInteractionAllowed_AdditionalCondition != null)
		{
			Delegate[] invocationList = this.IsAgentInteractionAllowed_AdditionalCondition.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				object obj;
				if ((obj = invocationList[i].DynamicInvoke()) is bool && !(bool)obj)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsOrderGesturesEnabled()
	{
		if (this.AreOrderGesturesEnabled_AdditionalCondition != null)
		{
			Delegate[] invocationList = this.AreOrderGesturesEnabled_AdditionalCondition.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				object obj;
				if ((obj = invocationList[i].DynamicInvoke()) is bool && !(bool)obj)
				{
					return false;
				}
			}
		}
		return true;
	}

	public List<EquipmentElement> GetExtraEquipmentElementsForCharacter(BasicCharacterObject character, bool getAllEquipments = false)
	{
		List<EquipmentElement> list = new List<EquipmentElement>();
		foreach (MissionLogic missionLogic in MissionLogics)
		{
			List<EquipmentElement> extraEquipmentElementsForCharacter = missionLogic.GetExtraEquipmentElementsForCharacter(character, getAllEquipments);
			if (extraEquipmentElementsForCharacter != null)
			{
				list.AddRange(extraEquipmentElementsForCharacter);
			}
		}
		return list;
	}

	private bool CheckMissionEnded()
	{
		foreach (MissionLogic missionLogic in MissionLogics)
		{
			MissionResult missionResult = null;
			if (missionLogic.MissionEnded(ref missionResult))
			{
				TaleWorlds.Library.Debug.Print("CheckMissionEnded::ended");
				MissionResult = missionResult;
				MissionEnded = true;
				MissionResultReady(missionResult);
				return true;
			}
		}
		return false;
	}

	private void MissionResultReady(MissionResult missionResult)
	{
		foreach (MissionLogic missionLogic in MissionLogics)
		{
			missionLogic.OnMissionResultReady(missionResult);
		}
	}

	private void CheckMissionEnd(float currentTime)
	{
		if (!GameNetwork.IsClient && currentTime > NextCheckTimeEndMission)
		{
			if (CurrentState == State.Continuing)
			{
				if (MissionEnded)
				{
					return;
				}
				NextCheckTimeEndMission += 0.1f;
				CheckMissionEnded();
				if (!MissionEnded)
				{
					return;
				}
				_missionEndTime = currentTime + MissionCloseTimeAfterFinish;
				NextCheckTimeEndMission += 5f;
				{
					foreach (MissionLogic missionLogic in MissionLogics)
					{
						missionLogic.ShowBattleResults();
					}
					return;
				}
			}
			if (currentTime > _missionEndTime)
			{
				EndMissionInternal();
			}
			else
			{
				NextCheckTimeEndMission += 5f;
			}
		}
		else if (CurrentState != State.Continuing && currentTime > NextCheckTimeEndMission)
		{
			EndMissionInternal();
		}
	}

	public bool IsPlayerCloseToAnEnemy(float distance = 5f)
	{
		if (MainAgent == null)
		{
			return false;
		}
		Vec3 position = MainAgent.Position;
		float num = distance * distance;
		AgentProximityMap.ProximityMapSearchStruct searchStruct = AgentProximityMap.BeginSearch(this, position.AsVec2, distance);
		while (searchStruct.LastFoundAgent != null)
		{
			Agent lastFoundAgent = searchStruct.LastFoundAgent;
			if (lastFoundAgent != MainAgent && lastFoundAgent.GetAgentFlags().HasAnyFlag(AgentFlag.CanAttack) && lastFoundAgent.Position.DistanceSquared(position) <= num && (!lastFoundAgent.IsAIControlled || lastFoundAgent.CurrentWatchState == Agent.WatchState.Alarmed) && lastFoundAgent.IsEnemyOf(MainAgent) && !lastFoundAgent.IsRetreating())
			{
				return true;
			}
			AgentProximityMap.FindNext(this, ref searchStruct);
		}
		return false;
	}

	public Vec3 GetRandomPositionAroundPoint(Vec3 center, float minDistance, float maxDistance, bool nearFirst = false)
	{
		Vec3 vec = new Vec3(-1f);
		vec.RotateAboutZ(System.MathF.PI * 2f * MBRandom.RandomFloat);
		float num = maxDistance - minDistance;
		if (nearFirst)
		{
			for (int num2 = 4; num2 > 0; num2--)
			{
				for (int i = 0; (float)i <= 10f; i++)
				{
					vec.RotateAboutZ(System.MathF.PI * 2f / 5f);
					Vec3 position = center + vec * (minDistance + num / (float)num2);
					if (Scene.GetNavigationMeshForPosition(ref position))
					{
						return position;
					}
				}
			}
		}
		else
		{
			for (int j = 1; j < 5; j++)
			{
				for (int k = 0; (float)k <= 10f; k++)
				{
					vec.RotateAboutZ(System.MathF.PI * 2f / 5f);
					Vec3 position2 = center + vec * (minDistance + num / (float)j);
					if (Scene.GetNavigationMeshForPosition(ref position2))
					{
						return position2;
					}
				}
			}
		}
		return center;
	}

	public WorldPosition FindBestDefendingPosition(WorldPosition enemyPosition, WorldPosition defendedPosition)
	{
		return GetBestSlopeAngleHeightPosForDefending(enemyPosition, defendedPosition, 10, 0.5f, 4f, 0.5f, 0.70710677f, 0.1f, 1f, 0.7f, 0.5f, 1.2f, 20f, 0.6f);
	}

	public WorldPosition FindPositionWithBiggestSlopeTowardsDirectionInSquare(ref WorldPosition center, float halfSize, ref WorldPosition referencePosition)
	{
		return GetBestSlopeTowardsDirection(ref center, halfSize, ref referencePosition);
	}

	public void AddCustomMissile(Agent shooterAgent, MissionWeapon missileWeapon, Vec3 position, Vec3 direction, Mat3 orientation, float baseSpeed, float speed, bool addRigidBody, MissionObject missionObjectToIgnore, int forcedMissileIndex = -1)
	{
		WeaponData weaponData = missileWeapon.GetWeaponData(needBatchedVersionForMeshes: true);
		int num;
		GameEntity missileEntity;
		if (missileWeapon.WeaponsCount == 1)
		{
			WeaponStatsData weaponStatsData = missileWeapon.GetWeaponStatsDataForUsage(0);
			num = AddMissileSingleUsageAux(forcedMissileIndex, isPrediction: false, shooterAgent, in weaponData, in weaponStatsData, 0f, ref position, ref direction, ref orientation, baseSpeed, speed, addRigidBody, missionObjectToIgnore?.GameEntity, isPrimaryWeaponShot: false, out missileEntity);
		}
		else
		{
			WeaponStatsData[] weaponStatsData2 = missileWeapon.GetWeaponStatsData();
			num = AddMissileAux(forcedMissileIndex, isPrediction: false, shooterAgent, in weaponData, weaponStatsData2, 0f, ref position, ref direction, ref orientation, baseSpeed, speed, addRigidBody, missionObjectToIgnore?.GameEntity, isPrimaryWeaponShot: false, out missileEntity);
		}
		weaponData.DeinitializeManagedPointers();
		Missile value = new Missile(this, missileEntity)
		{
			ShooterAgent = shooterAgent,
			Weapon = missileWeapon,
			MissionObjectToIgnore = missionObjectToIgnore,
			Index = num
		};
		_missiles.Add(num, value);
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new CreateMissile(num, shooterAgent.Index, EquipmentIndex.None, missileWeapon, position, direction, speed, orientation, addRigidBody, missionObjectToIgnore.Id, isPrimaryWeaponShot: false));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
	}

	[UsedImplicitly]
	[MBCallback]
	internal void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, bool isPrimaryWeaponShot, int forcedMissileIndex)
	{
		float damageBonus = 0f;
		MissionWeapon weapon;
		if (shooterAgent.Equipment[weaponIndex].CurrentUsageItem.IsRangedWeapon && shooterAgent.Equipment[weaponIndex].CurrentUsageItem.IsConsumable)
		{
			weapon = shooterAgent.Equipment[weaponIndex];
		}
		else
		{
			weapon = shooterAgent.Equipment[weaponIndex].AmmoWeapon;
			if (shooterAgent.Equipment[weaponIndex].CurrentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.HasString))
			{
				damageBonus = shooterAgent.Equipment[weaponIndex].GetModifiedThrustDamageForCurrentUsage();
			}
		}
		weapon.Amount = 1;
		WeaponData weaponData = weapon.GetWeaponData(needBatchedVersionForMeshes: true);
		Vec3 direction = velocity;
		float speed = direction.Normalize();
		bool flag = GameNetwork.IsClient && forcedMissileIndex == -1;
		float baseSpeed = shooterAgent.Equipment[shooterAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand)].GetModifiedMissileSpeedForCurrentUsage();
		int num;
		GameEntity missileEntity;
		if (weapon.WeaponsCount == 1)
		{
			WeaponStatsData weaponStatsData = weapon.GetWeaponStatsDataForUsage(0);
			num = AddMissileSingleUsageAux(forcedMissileIndex, flag, shooterAgent, in weaponData, in weaponStatsData, damageBonus, ref position, ref direction, ref orientation, baseSpeed, speed, hasRigidBody, null, isPrimaryWeaponShot, out missileEntity);
		}
		else
		{
			WeaponStatsData[] weaponStatsData2 = weapon.GetWeaponStatsData();
			num = AddMissileAux(forcedMissileIndex, flag, shooterAgent, in weaponData, weaponStatsData2, damageBonus, ref position, ref direction, ref orientation, baseSpeed, speed, hasRigidBody, null, isPrimaryWeaponShot, out missileEntity);
		}
		weaponData.DeinitializeManagedPointers();
		if (!flag)
		{
			Missile value = new Missile(this, missileEntity)
			{
				ShooterAgent = shooterAgent,
				Weapon = weapon,
				Index = num
			};
			missileEntity.ManualInvalidate();
			_missiles.Add(num, value);
			if (GameNetwork.IsServerOrRecorder)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new CreateMissile(num, shooterAgent.Index, weaponIndex, MissionWeapon.Invalid, position, direction, speed, orientation, hasRigidBody, MissionObjectId.Invalid, isPrimaryWeaponShot));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
			}
		}
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnAgentShootMissile(shooterAgent, weaponIndex, position, velocity, orientation, hasRigidBody, forcedMissileIndex);
		}
		shooterAgent?.UpdateLastRangedAttackTimeDueToAnAttack(MBCommon.GetTotalMissionTime());
	}

	[UsedImplicitly]
	[MBCallback]
	internal AgentState GetAgentState(Agent affectorAgent, Agent agent, DamageTypes damageType, WeaponFlags weaponFlags)
	{
		float useSurgeryProbability;
		float agentStateProbability = MissionGameModels.Current.AgentDecideKilledOrUnconsciousModel.GetAgentStateProbability(affectorAgent, agent, damageType, weaponFlags, out useSurgeryProbability);
		AgentState agentState = AgentState.None;
		bool usedSurgery = false;
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			if (missionBehavior is IAgentStateDecider)
			{
				agentState = (missionBehavior as IAgentStateDecider).GetAgentState(agent, agentStateProbability, out usedSurgery);
				break;
			}
		}
		if (agentState == AgentState.None)
		{
			float randomFloat = MBRandom.RandomFloat;
			if (randomFloat < agentStateProbability)
			{
				agentState = AgentState.Killed;
				usedSurgery = true;
			}
			else
			{
				agentState = AgentState.Unconscious;
				if (randomFloat > 1f - useSurgeryProbability)
				{
					usedSurgery = true;
				}
			}
		}
		if (usedSurgery && affectorAgent != null && affectorAgent.Team != null && agent.Team != null && affectorAgent.Team == agent.Team)
		{
			usedSurgery = false;
		}
		for (int i = 0; i < MissionBehaviors.Count; i++)
		{
			MissionBehaviors[i].OnGetAgentState(agent, usedSurgery);
		}
		return agentState;
	}

	public void OnAgentMount(Agent agent)
	{
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnAgentMount(agent);
		}
	}

	public void OnAgentDismount(Agent agent)
	{
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnAgentDismount(agent);
		}
	}

	public void OnObjectUsed(Agent userAgent, UsableMissionObject usableGameObject)
	{
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnObjectUsed(userAgent, usableGameObject);
		}
	}

	public void OnObjectStoppedBeingUsed(Agent userAgent, UsableMissionObject usableGameObject)
	{
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnObjectStoppedBeingUsed(userAgent, usableGameObject);
		}
	}

	public void InitializeStartingBehaviors(MissionLogic[] logicBehaviors, MissionBehavior[] otherBehaviors, MissionNetwork[] networkBehaviors)
	{
		foreach (MissionLogic missionBehavior in logicBehaviors)
		{
			AddMissionBehavior(missionBehavior);
		}
		foreach (MissionNetwork missionBehavior2 in networkBehaviors)
		{
			AddMissionBehavior(missionBehavior2);
		}
		foreach (MissionBehavior missionBehavior3 in otherBehaviors)
		{
			AddMissionBehavior(missionBehavior3);
		}
	}

	public Agent GetClosestEnemyAgent(Team team, Vec3 position, float radius)
	{
		return GetClosestEnemyAgent(team.MBTeam, position, radius);
	}

	public Agent GetClosestAllyAgent(Team team, Vec3 position, float radius)
	{
		return GetClosestAllyAgent(team.MBTeam, position, radius);
	}

	public int GetNearbyEnemyAgentCount(Team team, Vec2 position, float radius)
	{
		return GetNearbyEnemyAgentCount(team.MBTeam, position, radius);
	}

	public bool HasAnyAgentsOfSideInRange(Vec3 origin, float radius, BattleSideEnum side)
	{
		Team team = ((side == BattleSideEnum.Attacker) ? AttackerTeam : DefenderTeam);
		return MBAPI.IMBMission.HasAnyAgentsOfTeamAround(Pointer, origin, radius, team.MBTeam.Index);
	}

	private void HandleSpawnedItems()
	{
		if (GameNetwork.IsClientOrReplay)
		{
			return;
		}
		int num = 0;
		for (int num2 = _spawnedItemEntitiesCreatedAtRuntime.Count - 1; num2 >= 0; num2--)
		{
			SpawnedItemEntity spawnedItemEntity = _spawnedItemEntitiesCreatedAtRuntime[num2];
			if (!spawnedItemEntity.IsRemoved)
			{
				if (!spawnedItemEntity.IsDeactivated && !spawnedItemEntity.HasUser && spawnedItemEntity.HasLifeTime && !spawnedItemEntity.HasAIMovingTo && (num > 500 || spawnedItemEntity.IsReadyToBeDeleted()))
				{
					spawnedItemEntity.GameEntity.Remove(80);
				}
				else
				{
					num++;
				}
			}
			if (spawnedItemEntity.IsRemoved)
			{
				_spawnedItemEntitiesCreatedAtRuntime.RemoveAt(num2);
			}
		}
	}

	public bool OnMissionObjectRemoved(MissionObject missionObject, int removeReason)
	{
		if (!GameNetwork.IsClientOrReplay && missionObject.CreatedAtRuntime)
		{
			ReturnRuntimeMissionObjectId(missionObject.Id.Id);
			if (GameNetwork.IsServerOrRecorder)
			{
				RemoveDynamicallySpawnedMissionObjectInfo(missionObject.Id);
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new RemoveMissionObject(missionObject.Id));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
			}
		}
		_activeMissionObjects.Remove(missionObject);
		return _missionObjects.Remove(missionObject);
	}

	public bool AgentLookingAtAgent(Agent agent1, Agent agent2)
	{
		Vec3 v = agent2.Position - agent1.Position;
		float num = v.Normalize();
		float num2 = Vec3.DotProduct(v, agent1.LookDirection);
		if (num2 < 1f && num2 > 0.86f)
		{
			return num < 4f;
		}
		return false;
	}

	public Agent FindAgentWithIndex(int agentId)
	{
		return FindAgentWithIndexAux(agentId);
	}

	public static Agent.UnderAttackType GetUnderAttackTypeOfAgents(IEnumerable<Agent> agents, float timeLimit = 3f)
	{
		float num = float.MinValue;
		float num2 = float.MinValue;
		timeLimit += MBCommon.GetTotalMissionTime();
		foreach (Agent agent in agents)
		{
			num = TaleWorlds.Library.MathF.Max(num, agent.LastMeleeHitTime);
			num2 = TaleWorlds.Library.MathF.Max(num2, agent.LastRangedHitTime);
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

	public static Team GetAgentTeam(IAgentOriginBase troopOrigin, bool isPlayerSide)
	{
		if (Current == null)
		{
			TaleWorlds.Library.Debug.FailedAssert("Mission current is null", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Mission.cs", "GetAgentTeam", 5282);
			return null;
		}
		Team team = null;
		if (troopOrigin.IsUnderPlayersCommand)
		{
			return Current.PlayerTeam;
		}
		if (isPlayerSide)
		{
			if (Current.PlayerAllyTeam != null)
			{
				return Current.PlayerAllyTeam;
			}
			return Current.PlayerTeam;
		}
		return Current.PlayerEnemyTeam;
	}

	public static float GetBattleSizeOffset(int battleSize, Path path)
	{
		if (path != null && path.NumberOfPoints > 1)
		{
			float totalLength = path.GetTotalLength();
			float normalizationFactor = 800f / totalLength;
			float battleSizeFactor = GetBattleSizeFactor(battleSize, normalizationFactor);
			return 0f - 0.44f * battleSizeFactor;
		}
		return 0f;
	}

	public void OnRenderingStarted()
	{
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnRenderingStarted();
		}
	}

	public static float GetBattleSizeFactor(int battleSize, float normalizationFactor)
	{
		float value = -1f;
		if (battleSize > 0)
		{
			value = 0.04f + 0.08f * TaleWorlds.Library.MathF.Pow(battleSize, 0.4f);
			value *= normalizationFactor;
		}
		return MBMath.ClampFloat(value, 0.15f, 1f);
	}

	public Agent.MovementBehaviorType GetMovementTypeOfAgents(IEnumerable<Agent> agents)
	{
		float totalMissionTime = MBCommon.GetTotalMissionTime();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (Agent agent in agents)
		{
			num++;
			if (agent.IsAIControlled && (agent.IsRetreating() || (agent.Formation != null && agent.Formation.GetReadonlyMovementOrderReference().OrderType == OrderType.Retreat)))
			{
				num2++;
			}
			if (totalMissionTime - agent.LastMeleeAttackTime < 3f)
			{
				num3++;
			}
		}
		if ((float)num2 * 1f / (float)num > 0.3f)
		{
			return Agent.MovementBehaviorType.Flee;
		}
		if (num3 > 0)
		{
			return Agent.MovementBehaviorType.Engaged;
		}
		return Agent.MovementBehaviorType.Idle;
	}

	public void ShowInMissionLoadingScreen(int durationInSecond, Action onLoadingEndedAction)
	{
		_inMissionLoadingScreenTimer = new TaleWorlds.Core.Timer(CurrentTime, durationInSecond);
		_onLoadingEndedAction = onLoadingEndedAction;
		LoadingWindow.EnableGlobalLoadingWindow();
	}

	public bool CanAgentRout(Agent agent)
	{
		if ((agent.IsRunningAway || (agent.CommonAIComponent != null && agent.CommonAIComponent.IsRetreating)) && agent.RiderAgent == null)
		{
			if (this.CanAgentRout_AdditionalCondition == null)
			{
				return true;
			}
			return this.CanAgentRout_AdditionalCondition(agent);
		}
		return false;
	}

	internal bool CanGiveDamageToAgentShield(Agent attacker, WeaponComponentData attackerWeapon, Agent defender)
	{
		if (MissionGameModels.Current.AgentApplyDamageModel.CanWeaponIgnoreFriendlyFireChecks(attackerWeapon))
		{
			return true;
		}
		return !CancelsDamageAndBlocksAttackBecauseOfNonEnemyCase(attacker, defender);
	}

	[UsedImplicitly]
	[MBCallback]
	internal void MeleeHitCallback(ref AttackCollisionData collisionData, Agent attacker, Agent victim, GameEntity realHitEntity, ref float inOutMomentumRemaining, ref MeleeCollisionReaction colReaction, CrushThroughState crushThroughState, Vec3 blowDir, Vec3 swingDir, ref HitParticleResultData hitParticleResultData, bool crushedThroughWithoutAgentCollision)
	{
		hitParticleResultData.Reset();
		bool flag = collisionData.CollisionResult == CombatCollisionResult.Parried || collisionData.CollisionResult == CombatCollisionResult.Blocked || collisionData.CollisionResult == CombatCollisionResult.ChamberBlocked;
		if (collisionData.IsAlternativeAttack && !flag && victim != null && victim.IsHuman && collisionData.CollisionBoneIndex != -1 && (collisionData.VictimHitBodyPart == BoneBodyPartType.ArmLeft || collisionData.VictimHitBodyPart == BoneBodyPartType.ArmRight) && victim.IsHuman)
		{
			colReaction = MeleeCollisionReaction.ContinueChecking;
		}
		if (colReaction == MeleeCollisionReaction.ContinueChecking)
		{
			return;
		}
		bool flag2 = false;
		bool num = CancelsDamageAndBlocksAttackBecauseOfNonEnemyCase(attacker, victim);
		bool flag3 = victim != null && victim.CurrentMortalityState == Agent.MortalityState.Invulnerable;
		if (!num)
		{
			flag2 = flag3 || (flag && !collisionData.AttackBlockedWithShield);
		}
		else
		{
			collisionData.AttackerStunPeriod = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.StunPeriodAttackerFriendlyFire);
			flag2 = true;
		}
		int affectorWeaponSlotOrMissileIndex = collisionData.AffectorWeaponSlotOrMissileIndex;
		MissionWeapon attackerWeapon = ((affectorWeaponSlotOrMissileIndex >= 0) ? attacker.Equipment[affectorWeaponSlotOrMissileIndex] : MissionWeapon.Invalid);
		if (crushThroughState == CrushThroughState.CrushedThisFrame && !collisionData.IsAlternativeAttack)
		{
			UpdateMomentumRemaining(ref inOutMomentumRemaining, default(Blow), in collisionData, attacker, victim, in attackerWeapon, isCrushThrough: true);
		}
		WeaponComponentData shieldOnBack = null;
		CombatLogData combatLog = default(CombatLogData);
		if (!flag2)
		{
			GetAttackCollisionResults(attacker, victim, realHitEntity, inOutMomentumRemaining, in attackerWeapon, crushThroughState != CrushThroughState.None, flag2, crushedThroughWithoutAgentCollision, ref collisionData, out shieldOnBack, out combatLog);
			if (!collisionData.IsAlternativeAttack && attacker.IsDoingPassiveAttack && !GameNetwork.IsSessionActive && ManagedOptions.GetConfig(ManagedOptions.ManagedOptionsType.ReportDamage) > 0f)
			{
				if (attacker.HasMount)
				{
					if (attacker.IsMainAgent)
					{
						InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("ui_delivered_couched_lance_damage").ToString(), Color.ConvertStringToColor("#AE4AD9FF")));
					}
					else if (victim != null && victim.IsMainAgent)
					{
						InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("ui_received_couched_lance_damage").ToString(), Color.ConvertStringToColor("#D65252FF")));
					}
				}
				else if (attacker.IsMainAgent)
				{
					InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("ui_delivered_braced_polearm_damage").ToString(), Color.ConvertStringToColor("#AE4AD9FF")));
				}
				else if (victim != null && victim.IsMainAgent)
				{
					InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("ui_received_braced_polearm_damage").ToString(), Color.ConvertStringToColor("#D65252FF")));
				}
			}
			if (collisionData.CollidedWithShieldOnBack && shieldOnBack != null && victim != null && victim.IsMainAgent)
			{
				InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("ui_hit_shield_on_back").ToString(), Color.ConvertStringToColor("#FFFFFFFF")));
			}
		}
		else
		{
			collisionData.InflictedDamage = 0;
			collisionData.BaseMagnitude = 0f;
			collisionData.AbsorbedByArmor = 0;
			collisionData.SelfInflictedDamage = 0;
		}
		if (!crushedThroughWithoutAgentCollision)
		{
			Blow blow = CreateMeleeBlow(attacker, victim, in collisionData, in attackerWeapon, crushThroughState, blowDir, swingDir, flag2);
			if (!flag && ((victim != null && victim.IsActive()) || realHitEntity != null))
			{
				RegisterBlow(attacker, victim, realHitEntity, blow, ref collisionData, in attackerWeapon, ref combatLog);
			}
			UpdateMomentumRemaining(ref inOutMomentumRemaining, blow, in collisionData, attacker, victim, in attackerWeapon, isCrushThrough: false);
			bool isFatalHit = victim != null && victim.Health <= 0f;
			bool isShruggedOff = (blow.BlowFlag & BlowFlags.ShrugOff) != 0;
			DecideAgentHitParticles(attacker, victim, in blow, in collisionData, ref hitParticleResultData);
			DecideWeaponCollisionReaction(blow, in collisionData, attacker, victim, in attackerWeapon, isFatalHit, isShruggedOff, out colReaction);
		}
		else
		{
			colReaction = MeleeCollisionReaction.ContinueChecking;
		}
		foreach (MissionBehavior missionBehavior in Current.MissionBehaviors)
		{
			missionBehavior.OnMeleeHit(attacker, victim, flag2, collisionData);
		}
	}

	private bool HitWithAnotherBone(in AttackCollisionData collisionData, Agent attacker, in MissionWeapon attackerWeapon)
	{
		int weaponAttachBoneIndex = ((!attackerWeapon.IsEmpty && attacker != null && attacker.IsHuman) ? attacker.Monster.GetBoneToAttachForItemFlags(attackerWeapon.Item.ItemFlags) : (-1));
		return MissionCombatMechanicsHelper.IsCollisionBoneDifferentThanWeaponAttachBone(in collisionData, weaponAttachBoneIndex);
	}

	private void DecideAgentHitParticles(Agent attacker, Agent victim, in Blow blow, in AttackCollisionData collisionData, ref HitParticleResultData hprd)
	{
		if (victim != null && (blow.InflictedDamage > 0 || victim.Health <= 0f))
		{
			if (!blow.WeaponRecord.HasWeapon() || blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.NoBlood) || collisionData.IsAlternativeAttack)
			{
				MissionGameModels.Current.DamageParticleModel.GetMeleeAttackSweatParticles(attacker, victim, in blow, in collisionData, out hprd);
			}
			else
			{
				MissionGameModels.Current.DamageParticleModel.GetMeleeAttackBloodParticles(attacker, victim, in blow, in collisionData, out hprd);
			}
		}
	}

	private void DecideWeaponCollisionReaction(Blow registeredBlow, in AttackCollisionData collisionData, Agent attacker, Agent defender, in MissionWeapon attackerWeapon, bool isFatalHit, bool isShruggedOff, out MeleeCollisionReaction colReaction)
	{
		if (collisionData.IsColliderAgent && collisionData.StrikeType == 1 && collisionData.CollisionHitResultFlags.HasAnyFlag(CombatHitResultFlags.HitWithStartOfTheAnimation))
		{
			colReaction = MeleeCollisionReaction.Staggered;
			return;
		}
		if (!collisionData.IsColliderAgent && collisionData.PhysicsMaterialIndex != -1 && PhysicsMaterial.GetFromIndex(collisionData.PhysicsMaterialIndex).GetFlags().HasAnyFlag(PhysicsMaterialFlags.AttacksCanPassThrough))
		{
			colReaction = MeleeCollisionReaction.SlicedThrough;
			return;
		}
		if (!collisionData.IsColliderAgent || registeredBlow.InflictedDamage <= 0)
		{
			colReaction = MeleeCollisionReaction.Bounced;
			return;
		}
		if (collisionData.StrikeType == 1 && attacker.IsDoingPassiveAttack)
		{
			colReaction = MissionGameModels.Current.AgentApplyDamageModel.DecidePassiveAttackCollisionReaction(attacker, defender, isFatalHit);
			return;
		}
		if (HitWithAnotherBone(in collisionData, attacker, in attackerWeapon))
		{
			colReaction = MeleeCollisionReaction.Bounced;
			return;
		}
		WeaponClass weaponClass = ((!attackerWeapon.IsEmpty) ? attackerWeapon.CurrentUsageItem.WeaponClass : WeaponClass.Undefined);
		if ((!attackerWeapon.IsEmpty && !isFatalHit && isShruggedOff) || (attackerWeapon.IsEmpty && defender != null && defender.IsHuman && !collisionData.IsAlternativeAttack && (collisionData.VictimHitBodyPart == BoneBodyPartType.Chest || collisionData.VictimHitBodyPart == BoneBodyPartType.ShoulderLeft || collisionData.VictimHitBodyPart == BoneBodyPartType.ShoulderRight || collisionData.VictimHitBodyPart == BoneBodyPartType.Abdomen || collisionData.VictimHitBodyPart == BoneBodyPartType.Legs)))
		{
			colReaction = MeleeCollisionReaction.Bounced;
		}
		else if (((weaponClass == WeaponClass.OneHandedAxe || weaponClass == WeaponClass.TwoHandedAxe) && !isFatalHit && (float)collisionData.InflictedDamage < defender.HealthLimit * 0.5f) || (attackerWeapon.IsEmpty && !collisionData.IsAlternativeAttack && collisionData.AttackDirection == Agent.UsageDirection.AttackUp) || (collisionData.ThrustTipHit && collisionData.DamageType == 1 && !attackerWeapon.IsEmpty && defender.CanThrustAttackStickToBone(collisionData.VictimHitBodyPart)))
		{
			colReaction = MeleeCollisionReaction.Stuck;
		}
		else
		{
			colReaction = MeleeCollisionReaction.SlicedThrough;
		}
		if ((collisionData.AttackBlockedWithShield || collisionData.CollidedWithShieldOnBack) && colReaction == MeleeCollisionReaction.SlicedThrough)
		{
			colReaction = MeleeCollisionReaction.Bounced;
		}
	}

	private void RegisterBlow(Agent attacker, Agent victim, GameEntity realHitEntity, Blow b, ref AttackCollisionData collisionData, in MissionWeapon attackerWeapon, ref CombatLogData combatLogData)
	{
		b.VictimBodyPart = collisionData.VictimHitBodyPart;
		if (!collisionData.AttackBlockedWithShield)
		{
			if (collisionData.IsColliderAgent)
			{
				if (b.SelfInflictedDamage > 0 && attacker != null && attacker.IsActive() && attacker.IsFriendOf(victim))
				{
					attacker.CreateBlowFromBlowAsReflection(in b, in collisionData, out var outBlow, out var outCollisionData);
					if (victim.IsMount && attacker.MountAgent != null)
					{
						attacker.MountAgent.RegisterBlow(outBlow, in outCollisionData);
					}
					else
					{
						attacker.RegisterBlow(outBlow, in outCollisionData);
					}
				}
				if (b.InflictedDamage > 0)
				{
					combatLogData.IsFatalDamage = victim != null && victim.Health - (float)b.InflictedDamage < 1f;
					combatLogData.InflictedDamage = b.InflictedDamage - combatLogData.ModifiedDamage;
					PrintAttackCollisionResults(attacker, victim, realHitEntity, ref collisionData, ref combatLogData);
				}
				victim.RegisterBlow(b, in collisionData);
			}
			else if (collisionData.EntityExists)
			{
				MissionWeapon weapon = (b.IsMissile ? _missiles[b.WeaponRecord.AffectorWeaponSlotOrMissileIndex].Weapon : ((attacker != null && b.WeaponRecord.HasWeapon()) ? attacker.Equipment[b.WeaponRecord.AffectorWeaponSlotOrMissileIndex] : MissionWeapon.Invalid));
				OnEntityHit(realHitEntity, attacker, b.InflictedDamage, (DamageTypes)collisionData.DamageType, b.GlobalPosition, b.SwingDirection, in weapon);
				if (attacker != null && b.SelfInflictedDamage > 0)
				{
					attacker.CreateBlowFromBlowAsReflection(in b, in collisionData, out var outBlow2, out var outCollisionData2);
					attacker.RegisterBlow(outBlow2, in outCollisionData2);
				}
			}
		}
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnRegisterBlow(attacker, victim, realHitEntity, b, ref collisionData, in attackerWeapon);
		}
	}

	private void UpdateMomentumRemaining(ref float momentumRemaining, Blow b, in AttackCollisionData collisionData, Agent attacker, Agent victim, in MissionWeapon attackerWeapon, bool isCrushThrough)
	{
		float num = momentumRemaining;
		momentumRemaining = 0f;
		if (isCrushThrough)
		{
			momentumRemaining = num * 0.3f;
		}
		else
		{
			if (b.InflictedDamage <= 0 || collisionData.AttackBlockedWithShield || collisionData.CollidedWithShieldOnBack || !collisionData.IsColliderAgent || collisionData.IsHorseCharge)
			{
				return;
			}
			if (attacker != null && attacker.IsDoingPassiveAttack)
			{
				momentumRemaining = num * 0.5f;
			}
			else if (!HitWithAnotherBone(in collisionData, attacker, in attackerWeapon) && !attackerWeapon.IsEmpty && b.StrikeType != StrikeType.Thrust && !attackerWeapon.IsEmpty && attackerWeapon.CurrentUsageItem.CanHitMultipleTargets)
			{
				momentumRemaining = num * (1f - b.AbsorbedByArmor / (float)b.InflictedDamage);
				momentumRemaining *= 0.5f;
				if (momentumRemaining < 0.25f)
				{
					momentumRemaining = 0f;
				}
			}
		}
	}

	private Blow CreateMissileBlow(Agent attackerAgent, in AttackCollisionData collisionData, in MissionWeapon attackerWeapon, Vec3 missilePosition, Vec3 missileStartingPosition)
	{
		Blow result = new Blow(attackerAgent?.Index ?? (-1));
		result.BlowFlag = (attackerWeapon.CurrentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.CanKnockDown) ? BlowFlags.KnockDown : BlowFlags.None);
		result.Direction = collisionData.MissileVelocity.NormalizedCopy();
		result.SwingDirection = result.Direction;
		result.GlobalPosition = collisionData.CollisionGlobalPosition;
		result.BoneIndex = collisionData.CollisionBoneIndex;
		result.StrikeType = (StrikeType)collisionData.StrikeType;
		result.DamageType = (DamageTypes)collisionData.DamageType;
		result.VictimBodyPart = collisionData.VictimHitBodyPart;
		sbyte weaponAttachBoneIndex = attackerAgent?.Monster.GetBoneToAttachForItemFlags(attackerWeapon.Item.ItemFlags) ?? (-1);
		result.WeaponRecord.FillAsMissileBlow(attackerWeapon.Item, attackerWeapon.CurrentUsageItem, collisionData.AffectorWeaponSlotOrMissileIndex, weaponAttachBoneIndex, missileStartingPosition, missilePosition, collisionData.MissileVelocity);
		result.BaseMagnitude = collisionData.BaseMagnitude;
		result.MovementSpeedDamageModifier = collisionData.MovementSpeedDamageModifier;
		result.AbsorbedByArmor = collisionData.AbsorbedByArmor;
		result.InflictedDamage = collisionData.InflictedDamage;
		result.SelfInflictedDamage = collisionData.SelfInflictedDamage;
		result.DamageCalculated = true;
		return result;
	}

	[UsedImplicitly]
	[MBCallback]
	internal float OnAgentHitBlocked(Agent affectedAgent, Agent affectorAgent, ref AttackCollisionData collisionData, Vec3 blowDirection, Vec3 swingDirection, bool isMissile)
	{
		Blow b;
		if (isMissile)
		{
			Missile missile = _missiles[collisionData.AffectorWeaponSlotOrMissileIndex];
			MissionWeapon attackerWeapon = missile.Weapon;
			b = CreateMissileBlow(affectorAgent, in collisionData, in attackerWeapon, missile.GetPosition(), collisionData.MissileStartingPosition);
		}
		else
		{
			int affectorWeaponSlotOrMissileIndex = collisionData.AffectorWeaponSlotOrMissileIndex;
			MissionWeapon attackerWeapon2 = ((affectorWeaponSlotOrMissileIndex >= 0) ? affectorAgent.Equipment[affectorWeaponSlotOrMissileIndex] : MissionWeapon.Invalid);
			b = CreateMeleeBlow(affectorAgent, affectedAgent, in collisionData, in attackerWeapon2, CrushThroughState.None, blowDirection, swingDirection, cancelDamage: true);
		}
		return OnAgentHit(affectedAgent, affectorAgent, in b, in collisionData, isBlocked: true, 0f);
	}

	private Blow CreateMeleeBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, in MissionWeapon attackerWeapon, CrushThroughState crushThroughState, Vec3 blowDirection, Vec3 swingDirection, bool cancelDamage)
	{
		Blow blow = new Blow(attackerAgent.Index);
		blow.VictimBodyPart = collisionData.VictimHitBodyPart;
		bool flag = HitWithAnotherBone(in collisionData, attackerAgent, in attackerWeapon);
		if (collisionData.IsAlternativeAttack)
		{
			blow.AttackType = (attackerWeapon.IsEmpty ? AgentAttackType.Kick : AgentAttackType.Bash);
		}
		else
		{
			blow.AttackType = AgentAttackType.Standard;
		}
		sbyte weaponAttachBoneIndex = (sbyte)(attackerWeapon.IsEmpty ? (-1) : attackerAgent.Monster.GetBoneToAttachForItemFlags(attackerWeapon.Item.ItemFlags));
		blow.WeaponRecord.FillAsMeleeBlow(attackerWeapon.Item, attackerWeapon.CurrentUsageItem, collisionData.AffectorWeaponSlotOrMissileIndex, weaponAttachBoneIndex);
		blow.StrikeType = (StrikeType)collisionData.StrikeType;
		blow.DamageType = ((!attackerWeapon.IsEmpty && !flag && !collisionData.IsAlternativeAttack) ? ((DamageTypes)collisionData.DamageType) : DamageTypes.Blunt);
		blow.NoIgnore = collisionData.IsAlternativeAttack;
		blow.AttackerStunPeriod = collisionData.AttackerStunPeriod;
		blow.DefenderStunPeriod = collisionData.DefenderStunPeriod;
		blow.BlowFlag = BlowFlags.None;
		blow.GlobalPosition = collisionData.CollisionGlobalPosition;
		blow.BoneIndex = collisionData.CollisionBoneIndex;
		blow.Direction = blowDirection;
		blow.SwingDirection = swingDirection;
		if (cancelDamage)
		{
			blow.BaseMagnitude = 0f;
			blow.MovementSpeedDamageModifier = 0f;
			blow.InflictedDamage = 0;
			blow.SelfInflictedDamage = 0;
			blow.AbsorbedByArmor = 0f;
		}
		else
		{
			blow.BaseMagnitude = collisionData.BaseMagnitude;
			blow.MovementSpeedDamageModifier = collisionData.MovementSpeedDamageModifier;
			blow.InflictedDamage = collisionData.InflictedDamage;
			blow.SelfInflictedDamage = collisionData.SelfInflictedDamage;
			blow.AbsorbedByArmor = collisionData.AbsorbedByArmor;
		}
		blow.DamageCalculated = true;
		if (crushThroughState != 0)
		{
			blow.BlowFlag |= BlowFlags.CrushThrough;
		}
		if (blow.StrikeType == StrikeType.Thrust && !collisionData.ThrustTipHit)
		{
			blow.BlowFlag |= BlowFlags.NonTipThrust;
		}
		if (collisionData.IsColliderAgent)
		{
			if (MissionGameModels.Current.AgentApplyDamageModel.DecideAgentShrugOffBlow(victimAgent, collisionData, in blow))
			{
				blow.BlowFlag |= BlowFlags.ShrugOff;
			}
			if (victimAgent.IsHuman)
			{
				Agent mountAgent = victimAgent.MountAgent;
				if (mountAgent != null)
				{
					if (mountAgent.RiderAgent == victimAgent && MissionGameModels.Current.AgentApplyDamageModel.DecideAgentDismountedByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon.CurrentUsageItem, in blow))
					{
						blow.BlowFlag |= BlowFlags.CanDismount;
					}
				}
				else
				{
					if (MissionGameModels.Current.AgentApplyDamageModel.DecideAgentKnockedBackByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon.CurrentUsageItem, in blow))
					{
						blow.BlowFlag |= BlowFlags.KnockBack;
					}
					if (MissionGameModels.Current.AgentApplyDamageModel.DecideAgentKnockedDownByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon.CurrentUsageItem, in blow))
					{
						blow.BlowFlag |= BlowFlags.KnockDown;
					}
				}
			}
			else if (victimAgent.IsMount && MissionGameModels.Current.AgentApplyDamageModel.DecideMountRearedByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon.CurrentUsageItem, in blow))
			{
				blow.BlowFlag |= BlowFlags.MakesRear;
			}
		}
		return blow;
	}

	internal float OnAgentHit(Agent affectedAgent, Agent affectorAgent, in Blow b, in AttackCollisionData collisionData, bool isBlocked, float damagedHp)
	{
		float shotDifficulty = -1f;
		bool isSiegeEngineHit = false;
		int affectorWeaponSlotOrMissileIndex = b.WeaponRecord.AffectorWeaponSlotOrMissileIndex;
		bool isMissile = b.IsMissile;
		int inflictedDamage = b.InflictedDamage;
		float hitDistance = (b.IsMissile ? (b.GlobalPosition - b.WeaponRecord.StartingPosition).Length : 0f);
		MissionWeapon affectorWeapon;
		if (isMissile)
		{
			affectorWeapon = _missiles[affectorWeaponSlotOrMissileIndex].Weapon;
			isSiegeEngineHit = _missiles[affectorWeaponSlotOrMissileIndex].MissionObjectToIgnore != null;
		}
		else
		{
			affectorWeapon = ((affectorAgent != null && affectorWeaponSlotOrMissileIndex >= 0) ? affectorAgent.Equipment[affectorWeaponSlotOrMissileIndex] : MissionWeapon.Invalid);
		}
		if (affectorAgent != null && isMissile)
		{
			shotDifficulty = GetShootDifficulty(affectedAgent, affectorAgent, b.VictimBodyPart == BoneBodyPartType.Head);
		}
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnAgentHit(affectedAgent, affectorAgent, in affectorWeapon, in b, in collisionData);
			missionBehavior.OnScoreHit(affectedAgent, affectorAgent, affectorWeapon.CurrentUsageItem, isBlocked, isSiegeEngineHit, in b, in collisionData, damagedHp, hitDistance, shotDifficulty);
		}
		foreach (AgentComponent component in affectedAgent.Components)
		{
			component.OnHit(affectorAgent, inflictedDamage, in affectorWeapon);
		}
		affectedAgent.CheckToDropFlaggedItem();
		return inflictedDamage;
	}

	[UsedImplicitly]
	[MBCallback]
	internal void MissileAreaDamageCallback(ref AttackCollisionData collisionDataInput, ref Blow blowInput, Agent alreadyDamagedAgent, Agent shooterAgent, bool isBigExplosion)
	{
		float num = (isBigExplosion ? 2.8f : 1.2f);
		float num2 = (isBigExplosion ? 1.6f : 1f);
		float num3 = 1f;
		if (collisionDataInput.MissileVelocity.LengthSquared < 484f)
		{
			num2 *= 0.8f;
			num3 = 0.5f;
		}
		AttackCollisionData attackCollisionData = collisionDataInput;
		blowInput.VictimBodyPart = collisionDataInput.VictimHitBodyPart;
		List<Agent> list = new List<Agent>();
		AgentProximityMap.ProximityMapSearchStruct searchStruct = AgentProximityMap.BeginSearch(this, blowInput.GlobalPosition.AsVec2, num, extendRangeByBiggestAgentCollisionPadding: true);
		while (searchStruct.LastFoundAgent != null)
		{
			Agent lastFoundAgent = searchStruct.LastFoundAgent;
			if (lastFoundAgent.CurrentMortalityState != Agent.MortalityState.Invulnerable && lastFoundAgent != shooterAgent && lastFoundAgent != alreadyDamagedAgent)
			{
				list.Add(lastFoundAgent);
			}
			AgentProximityMap.FindNext(this, ref searchStruct);
		}
		foreach (Agent item in list)
		{
			Blow b = blowInput;
			b.DamageCalculated = false;
			attackCollisionData = collisionDataInput;
			float num4 = float.MaxValue;
			float num5 = 0f;
			sbyte collisionBoneIndexForAreaDamage = -1;
			Skeleton skeleton = item.AgentVisuals.GetSkeleton();
			sbyte boneCount = skeleton.GetBoneCount();
			MatrixFrame globalFrame = item.AgentVisuals.GetGlobalFrame();
			for (sbyte b2 = 0; b2 < boneCount; b2++)
			{
				num5 = globalFrame.TransformToParent(skeleton.GetBoneEntitialFrame(b2).origin).DistanceSquared(blowInput.GlobalPosition);
				if (num5 < num4)
				{
					collisionBoneIndexForAreaDamage = b2;
					num4 = num5;
				}
			}
			if (num4 <= num * num)
			{
				float num6 = TaleWorlds.Library.MathF.Sqrt(num4);
				float num7 = 1f;
				float num8 = 1f;
				if (num6 > num2)
				{
					num7 = MBMath.Lerp(1f, 3f, (num6 - num2) / (num - num2));
					num8 = 1f / (num7 * num7);
				}
				num8 *= num3;
				attackCollisionData.SetCollisionBoneIndexForAreaDamage(collisionBoneIndexForAreaDamage);
				MissionWeapon attackerWeapon = _missiles[attackCollisionData.AffectorWeaponSlotOrMissileIndex].Weapon;
				GetAttackCollisionResults(shooterAgent, item, null, 1f, in attackerWeapon, crushedThrough: false, cancelDamage: false, crushedThroughWithoutAgentCollision: false, ref attackCollisionData, out var _, out var combatLog);
				b.BaseMagnitude = attackCollisionData.BaseMagnitude;
				b.MovementSpeedDamageModifier = attackCollisionData.MovementSpeedDamageModifier;
				b.InflictedDamage = attackCollisionData.InflictedDamage;
				b.SelfInflictedDamage = attackCollisionData.SelfInflictedDamage;
				b.AbsorbedByArmor = attackCollisionData.AbsorbedByArmor;
				b.DamageCalculated = true;
				b.InflictedDamage = TaleWorlds.Library.MathF.Round((float)b.InflictedDamage * num8);
				b.SelfInflictedDamage = TaleWorlds.Library.MathF.Round((float)b.SelfInflictedDamage * num8);
				combatLog.ModifiedDamage = TaleWorlds.Library.MathF.Round((float)combatLog.ModifiedDamage * num8);
				RegisterBlow(shooterAgent, item, null, b, ref attackCollisionData, in attackerWeapon, ref combatLog);
			}
		}
	}

	[UsedImplicitly]
	[MBCallback]
	internal void OnMissileRemoved(int missileIndex)
	{
		_missiles.Remove(missileIndex);
	}

	[UsedImplicitly]
	[MBCallback]
	internal bool MissileHitCallback(out int extraHitParticleIndex, ref AttackCollisionData collisionData, Vec3 missileStartingPosition, Vec3 missilePosition, Vec3 missileAngularVelocity, Vec3 movementVelocity, MatrixFrame attachGlobalFrame, MatrixFrame affectedShieldGlobalFrame, int numDamagedAgents, Agent attacker, Agent victim, GameEntity hitEntity)
	{
		Missile missile = _missiles[collisionData.AffectorWeaponSlotOrMissileIndex];
		MissionWeapon attackerWeapon = missile.Weapon;
		WeaponFlags missileWeaponFlags = attackerWeapon.CurrentUsageItem.WeaponFlags;
		float num = 1f;
		WeaponComponentData shieldOnBack = null;
		MissionGameModels.Current.AgentApplyDamageModel.DecideMissileWeaponFlags(attacker, missile.Weapon, ref missileWeaponFlags);
		extraHitParticleIndex = -1;
		MissileCollisionReaction missileCollisionReaction = MissileCollisionReaction.Invalid;
		bool flag = !GameNetwork.IsSessionActive;
		bool missileHasPhysics = collisionData.MissileHasPhysics;
		PhysicsMaterial fromIndex = PhysicsMaterial.GetFromIndex(collisionData.PhysicsMaterialIndex);
		PhysicsMaterialFlags num2 = (fromIndex.IsValid ? fromIndex.GetFlags() : PhysicsMaterialFlags.None);
		bool flag2 = (missileWeaponFlags & WeaponFlags.AmmoSticksWhenShot) != 0;
		bool flag3 = (num2 & PhysicsMaterialFlags.DontStickMissiles) == 0;
		bool flag4 = (num2 & PhysicsMaterialFlags.AttacksCanPassThrough) != 0;
		MissionObject missionObject = null;
		if (victim == null && hitEntity != null)
		{
			GameEntity gameEntity = hitEntity;
			do
			{
				missionObject = gameEntity.GetFirstScriptOfType<MissionObject>();
				gameEntity = gameEntity.Parent;
			}
			while (missionObject == null && gameEntity != null);
			hitEntity = missionObject?.GameEntity;
		}
		MissileCollisionReaction missileCollisionReaction2 = (flag4 ? MissileCollisionReaction.PassThrough : (missileWeaponFlags.HasAnyFlag(WeaponFlags.Burning) ? MissileCollisionReaction.BecomeInvisible : ((!flag3 || !flag2) ? MissileCollisionReaction.BounceBack : MissileCollisionReaction.Stick)));
		bool flag5 = false;
		bool flag6 = victim != null && victim.CurrentMortalityState == Agent.MortalityState.Invulnerable;
		CombatLogData combatLog;
		if (collisionData.MissileGoneUnderWater || collisionData.MissileGoneOutOfBorder || flag6)
		{
			missileCollisionReaction = MissileCollisionReaction.BecomeInvisible;
		}
		else if (victim == null)
		{
			if (hitEntity != null)
			{
				GetAttackCollisionResults(attacker, victim, hitEntity, num, in attackerWeapon, crushedThrough: false, cancelDamage: false, crushedThroughWithoutAgentCollision: false, ref collisionData, out shieldOnBack, out combatLog);
				Blow b = CreateMissileBlow(attacker, in collisionData, in attackerWeapon, missilePosition, missileStartingPosition);
				RegisterBlow(attacker, null, hitEntity, b, ref collisionData, in attackerWeapon, ref combatLog);
			}
			missileCollisionReaction = missileCollisionReaction2;
		}
		else if (collisionData.AttackBlockedWithShield)
		{
			GetAttackCollisionResults(attacker, victim, hitEntity, num, in attackerWeapon, crushedThrough: false, cancelDamage: false, crushedThroughWithoutAgentCollision: false, ref collisionData, out shieldOnBack, out combatLog);
			if (!collisionData.IsShieldBroken)
			{
				MakeSound(ItemPhysicsSoundContainer.SoundCodePhysicsArrowlikeStone, collisionData.CollisionGlobalPosition, soundCanBePredicted: false, isReliable: false, -1, -1);
			}
			bool flag7 = false;
			if (missileWeaponFlags.HasAnyFlag(WeaponFlags.CanPenetrateShield))
			{
				if (!collisionData.IsShieldBroken)
				{
					EquipmentIndex wieldedItemIndex = victim.GetWieldedItemIndex(Agent.HandIndex.OffHand);
					if ((float)collisionData.InflictedDamage > ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.ShieldPenetrationOffset) + ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.ShieldPenetrationFactor) * (float)victim.Equipment[wieldedItemIndex].GetGetModifiedArmorForCurrentUsage())
					{
						flag7 = true;
					}
				}
				else
				{
					flag7 = true;
				}
			}
			if (flag7)
			{
				victim.MakeVoice(SkinVoiceManager.VoiceType.Pain, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
				num *= 0.4f + MBRandom.RandomFloat * 0.2f;
				missileCollisionReaction = MissileCollisionReaction.PassThrough;
			}
			else
			{
				missileCollisionReaction = (collisionData.IsShieldBroken ? MissileCollisionReaction.BecomeInvisible : missileCollisionReaction2);
			}
		}
		else if (collisionData.MissileBlockedWithWeapon)
		{
			GetAttackCollisionResults(attacker, victim, hitEntity, num, in attackerWeapon, crushedThrough: false, cancelDamage: false, crushedThroughWithoutAgentCollision: false, ref collisionData, out shieldOnBack, out combatLog);
			missileCollisionReaction = MissileCollisionReaction.BounceBack;
		}
		else
		{
			if (attacker != null && attacker.IsFriendOf(victim))
			{
				if (ForceNoFriendlyFire)
				{
					flag5 = true;
				}
				else if (!missileHasPhysics)
				{
					if (flag)
					{
						if (attacker.Controller == Agent.ControllerType.AI)
						{
							flag5 = true;
						}
					}
					else if ((MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent.GetIntValue() <= 0 && MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent.GetIntValue() <= 0) || Mode == MissionMode.Duel)
					{
						flag5 = true;
					}
				}
			}
			else if (victim.IsHuman && attacker != null && !attacker.IsEnemyOf(victim))
			{
				flag5 = true;
			}
			else if (flag && attacker != null && attacker.Controller == Agent.ControllerType.AI && victim.RiderAgent != null && attacker.IsFriendOf(victim.RiderAgent))
			{
				flag5 = true;
			}
			if (flag5)
			{
				if (flag && attacker != null && attacker == Agent.Main && attacker.IsFriendOf(victim))
				{
					InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("ui_you_hit_a_friendly_troop").ToString(), Color.ConvertStringToColor("#D65252FF")));
				}
				missileCollisionReaction = MissileCollisionReaction.BecomeInvisible;
			}
			else
			{
				bool flag8 = (missileWeaponFlags & WeaponFlags.MultiplePenetration) != 0;
				GetAttackCollisionResults(attacker, victim, null, num, in attackerWeapon, crushedThrough: false, cancelDamage: false, crushedThroughWithoutAgentCollision: false, ref collisionData, out shieldOnBack, out combatLog);
				Blow blow = CreateMissileBlow(attacker, in collisionData, in attackerWeapon, missilePosition, missileStartingPosition);
				if (collisionData.IsColliderAgent && flag8 && numDamagedAgents > 0)
				{
					blow.InflictedDamage /= numDamagedAgents;
					blow.SelfInflictedDamage /= numDamagedAgents;
					combatLog.InflictedDamage = blow.InflictedDamage - combatLog.ModifiedDamage;
				}
				if (collisionData.IsColliderAgent)
				{
					if (MissionGameModels.Current.AgentApplyDamageModel.DecideAgentShrugOffBlow(victim, collisionData, in blow))
					{
						blow.BlowFlag |= BlowFlags.ShrugOff;
					}
					else if (victim.IsHuman)
					{
						Agent mountAgent = victim.MountAgent;
						if (mountAgent != null)
						{
							if (mountAgent.RiderAgent == victim && MissionGameModels.Current.AgentApplyDamageModel.DecideAgentDismountedByBlow(attacker, victim, in collisionData, attackerWeapon.CurrentUsageItem, in blow))
							{
								blow.BlowFlag |= BlowFlags.CanDismount;
							}
						}
						else
						{
							if (MissionGameModels.Current.AgentApplyDamageModel.DecideAgentKnockedBackByBlow(attacker, victim, in collisionData, attackerWeapon.CurrentUsageItem, in blow))
							{
								blow.BlowFlag |= BlowFlags.KnockBack;
							}
							if (MissionGameModels.Current.AgentApplyDamageModel.DecideAgentKnockedDownByBlow(attacker, victim, in collisionData, attackerWeapon.CurrentUsageItem, in blow))
							{
								blow.BlowFlag |= BlowFlags.KnockDown;
							}
						}
					}
				}
				if (victim.State == AgentState.Active)
				{
					RegisterBlow(attacker, victim, null, blow, ref collisionData, in attackerWeapon, ref combatLog);
				}
				extraHitParticleIndex = MissionGameModels.Current.DamageParticleModel.GetMissileAttackParticle(attacker, victim, in blow, in collisionData);
				if (flag8 && numDamagedAgents < 3)
				{
					missileCollisionReaction = MissileCollisionReaction.PassThrough;
				}
				else
				{
					missileCollisionReaction = missileCollisionReaction2;
					if (missileCollisionReaction2 == MissileCollisionReaction.Stick && !collisionData.CollidedWithShieldOnBack)
					{
						bool flag9 = CombatType == MissionCombatType.Combat;
						if (flag9)
						{
							bool flag10 = victim.IsHuman && collisionData.VictimHitBodyPart == BoneBodyPartType.Head;
							flag9 = victim.State != AgentState.Active || !flag10;
						}
						if (flag9)
						{
							float managedParameter = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.MissileMinimumDamageToStick);
							float num3 = 2f * managedParameter;
							if ((float)blow.InflictedDamage < managedParameter && blow.AbsorbedByArmor > num3 && !GameNetwork.IsClientOrReplay)
							{
								missileCollisionReaction = MissileCollisionReaction.BounceBack;
							}
						}
						else
						{
							missileCollisionReaction = MissileCollisionReaction.BecomeInvisible;
						}
					}
				}
			}
		}
		if (collisionData.CollidedWithShieldOnBack && shieldOnBack != null && victim != null && victim.IsMainAgent)
		{
			InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("ui_hit_shield_on_back").ToString(), Color.ConvertStringToColor("#FFFFFFFF")));
		}
		MatrixFrame attachLocalFrame;
		bool isAttachedFrameLocal;
		if (!collisionData.MissileHasPhysics && missileCollisionReaction == MissileCollisionReaction.Stick)
		{
			attachLocalFrame = CalculateAttachedLocalFrame(in attachGlobalFrame, collisionData, missile.Weapon.CurrentUsageItem, victim, hitEntity, movementVelocity, missileAngularVelocity, affectedShieldGlobalFrame, shouldMissilePenetrate: true, out isAttachedFrameLocal);
		}
		else
		{
			attachLocalFrame = attachGlobalFrame;
			attachLocalFrame.origin.z = Math.Max(attachLocalFrame.origin.z, -100f);
			missionObject = null;
			isAttachedFrameLocal = false;
		}
		Vec3 velocity = Vec3.Zero;
		Vec3 angularVelocity = Vec3.Zero;
		if (missileCollisionReaction == MissileCollisionReaction.BounceBack)
		{
			WeaponFlags weaponFlags = missileWeaponFlags & WeaponFlags.AmmoBreakOnBounceBackMask;
			if ((weaponFlags == WeaponFlags.AmmoCanBreakOnBounceBack && collisionData.MissileVelocity.Length > ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BreakableProjectileMinimumBreakSpeed)) || weaponFlags == WeaponFlags.AmmoBreaksOnBounceBack)
			{
				missileCollisionReaction = MissileCollisionReaction.BecomeInvisible;
				extraHitParticleIndex = ParticleSystemManager.GetRuntimeIdByName("psys_game_broken_arrow");
			}
			else
			{
				missile.CalculateBounceBackVelocity(missileAngularVelocity, collisionData, out velocity, out angularVelocity);
			}
		}
		HandleMissileCollisionReaction(collisionData.AffectorWeaponSlotOrMissileIndex, missileCollisionReaction, attachLocalFrame, isAttachedFrameLocal, attacker, victim, collisionData.AttackBlockedWithShield, collisionData.CollisionBoneIndex, missionObject, velocity, angularVelocity, -1);
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnMissileHit(attacker, victim, flag5, collisionData);
		}
		return missileCollisionReaction != MissileCollisionReaction.PassThrough;
	}

	public void HandleMissileCollisionReaction(int missileIndex, MissileCollisionReaction collisionReaction, MatrixFrame attachLocalFrame, bool isAttachedFrameLocal, Agent attackerAgent, Agent attachedAgent, bool attachedToShield, sbyte attachedBoneIndex, MissionObject attachedMissionObject, Vec3 bounceBackVelocity, Vec3 bounceBackAngularVelocity, int forcedSpawnIndex)
	{
		Missile missile = _missiles[missileIndex];
		MissionObjectId missionObjectId = new MissionObjectId(-1, createdAtRuntime: true);
		switch (collisionReaction)
		{
		case MissileCollisionReaction.BecomeInvisible:
			missile.Entity.Remove(81);
			break;
		case MissileCollisionReaction.Stick:
			missile.Entity.SetVisibilityExcludeParents(visible: true);
			if (attachedAgent != null)
			{
				PrepareMissileWeaponForDrop(missileIndex);
				if (attachedToShield)
				{
					EquipmentIndex wieldedItemIndex = attachedAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
					attachedAgent.AttachWeaponToWeapon(wieldedItemIndex, missile.Weapon, missile.Entity, ref attachLocalFrame);
				}
				else
				{
					attachedAgent.AttachWeaponToBone(missile.Weapon, missile.Entity, attachedBoneIndex, ref attachLocalFrame);
				}
			}
			else
			{
				Vec3 velocity = Vec3.Zero;
				missionObjectId = SpawnWeaponAsDropFromMissile(missileIndex, attachedMissionObject, in attachLocalFrame, WeaponSpawnFlags.AsMissile | WeaponSpawnFlags.WithStaticPhysics, in velocity, in velocity, forcedSpawnIndex);
			}
			break;
		case MissileCollisionReaction.BounceBack:
			missile.Entity.SetVisibilityExcludeParents(visible: true);
			missionObjectId = SpawnWeaponAsDropFromMissile(missileIndex, null, in attachLocalFrame, WeaponSpawnFlags.AsMissile | WeaponSpawnFlags.WithPhysics, in bounceBackVelocity, in bounceBackAngularVelocity, forcedSpawnIndex);
			break;
		}
		bool flag = collisionReaction != MissileCollisionReaction.PassThrough;
		if (GameNetwork.IsServerOrRecorder)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage(new HandleMissileCollisionReaction(missileIndex, collisionReaction, attachLocalFrame, isAttachedFrameLocal, attackerAgent.Index, attachedAgent?.Index ?? (-1), attachedToShield, attachedBoneIndex, attachedMissionObject?.Id ?? MissionObjectId.Invalid, bounceBackVelocity, bounceBackAngularVelocity, missionObjectId.Id));
			GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
		}
		else if (GameNetwork.IsClientOrReplay && flag)
		{
			RemoveMissileAsClient(missileIndex);
		}
		foreach (MissionBehavior missionBehavior in MissionBehaviors)
		{
			missionBehavior.OnMissileCollisionReaction(collisionReaction, attackerAgent, attachedAgent, attachedBoneIndex);
		}
	}

	[UsedImplicitly]
	[MBCallback]
	internal void MissileCalculatePassbySoundParametersCallbackMT(int missileIndex, ref SoundEventParameter soundEventParameter)
	{
		_missiles[missileIndex].CalculatePassbySoundParametersMT(ref soundEventParameter);
	}

	[UsedImplicitly]
	[MBCallback]
	internal void ChargeDamageCallback(ref AttackCollisionData collisionData, Blow blow, Agent attacker, Agent victim)
	{
		if (victim.CurrentMortalityState == Agent.MortalityState.Invulnerable || (attacker.RiderAgent != null && !attacker.IsEnemyOf(victim)))
		{
			return;
		}
		GetAttackCollisionResults(attacker, victim, null, 1f, in MissionWeapon.Invalid, crushedThrough: false, cancelDamage: false, crushedThroughWithoutAgentCollision: false, ref collisionData, out var shieldOnBack, out var combatLog);
		if (collisionData.CollidedWithShieldOnBack && shieldOnBack != null && victim != null && victim.IsMainAgent)
		{
			InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("ui_hit_shield_on_back").ToString(), Color.ConvertStringToColor("#FFFFFFFF")));
		}
		if ((float)collisionData.InflictedDamage > 0f)
		{
			blow.BaseMagnitude = collisionData.BaseMagnitude;
			blow.MovementSpeedDamageModifier = collisionData.MovementSpeedDamageModifier;
			blow.InflictedDamage = collisionData.InflictedDamage;
			blow.SelfInflictedDamage = collisionData.SelfInflictedDamage;
			blow.AbsorbedByArmor = collisionData.AbsorbedByArmor;
			blow.DamageCalculated = true;
			if (MissionGameModels.Current.AgentApplyDamageModel.DecideAgentKnockedBackByBlow(attacker, victim, in collisionData, null, in blow))
			{
				blow.BlowFlag |= BlowFlags.KnockBack;
			}
			else
			{
				blow.BlowFlag &= ~BlowFlags.KnockBack;
			}
			if (MissionGameModels.Current.AgentApplyDamageModel.DecideAgentKnockedDownByBlow(attacker, victim, in collisionData, null, in blow))
			{
				blow.BlowFlag |= BlowFlags.KnockDown;
			}
			Blow b = blow;
			MissionWeapon attackerWeapon = default(MissionWeapon);
			RegisterBlow(attacker, victim, null, b, ref collisionData, in attackerWeapon, ref combatLog);
		}
	}

	[UsedImplicitly]
	[MBCallback]
	internal void FallDamageCallback(ref AttackCollisionData collisionData, Blow b, Agent attacker, Agent victim)
	{
		if (victim.CurrentMortalityState == Agent.MortalityState.Invulnerable)
		{
			return;
		}
		GetAttackCollisionResults(attacker, victim, null, 1f, in MissionWeapon.Invalid, crushedThrough: false, cancelDamage: false, crushedThroughWithoutAgentCollision: false, ref collisionData, out var _, out var combatLog);
		b.BaseMagnitude = collisionData.BaseMagnitude;
		b.MovementSpeedDamageModifier = collisionData.MovementSpeedDamageModifier;
		b.InflictedDamage = collisionData.InflictedDamage;
		b.SelfInflictedDamage = collisionData.SelfInflictedDamage;
		b.AbsorbedByArmor = collisionData.AbsorbedByArmor;
		b.DamageCalculated = true;
		if (b.InflictedDamage > 0)
		{
			Agent riderAgent = victim.RiderAgent;
			Blow b2 = b;
			MissionWeapon attackerWeapon = default(MissionWeapon);
			RegisterBlow(attacker, victim, null, b2, ref collisionData, in attackerWeapon, ref combatLog);
			if (riderAgent != null)
			{
				FallDamageCallback(ref collisionData, b, riderAgent, riderAgent);
			}
		}
	}

	public void KillAgentsOnEntity(GameEntity entity, Agent destroyerAgent, bool burnAgents)
	{
		if (entity == null)
		{
			return;
		}
		int ownerId;
		sbyte attackBoneIndex;
		if (destroyerAgent != null)
		{
			ownerId = destroyerAgent.Index;
			attackBoneIndex = destroyerAgent.Monster.MainHandItemBoneIndex;
		}
		else
		{
			ownerId = -1;
			attackBoneIndex = -1;
		}
		entity.GetPhysicsMinMax(includeChildren: true, out var bbmin, out var bbmax, returnLocal: false);
		Vec2 vec = (bbmax.AsVec2 + bbmin.AsVec2) * 0.5f;
		float searchRadius = (bbmax.AsVec2 - bbmin.AsVec2).Length * 0.5f;
		Blow blow = new Blow(ownerId);
		blow.DamageCalculated = true;
		blow.BaseMagnitude = 2000f;
		blow.InflictedDamage = 2000;
		blow.Direction = new Vec3(0f, 0f, -1f);
		blow.DamageType = DamageTypes.Blunt;
		blow.BoneIndex = 0;
		blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, 0);
		if (burnAgents)
		{
			blow.WeaponRecord.WeaponFlags |= WeaponFlags.AffectsArea | WeaponFlags.Burning;
			blow.WeaponRecord.CurrentPosition = blow.GlobalPosition;
			blow.WeaponRecord.StartingPosition = blow.GlobalPosition;
		}
		Vec2 asVec = entity.GetGlobalFrame().TransformToParent(vec.ToVec3()).AsVec2;
		List<Agent> list = new List<Agent>();
		AgentProximityMap.ProximityMapSearchStruct searchStruct = AgentProximityMap.BeginSearch(this, asVec, searchRadius);
		while (searchStruct.LastFoundAgent != null)
		{
			Agent lastFoundAgent = searchStruct.LastFoundAgent;
			GameEntity gameEntity = lastFoundAgent.GetSteppedEntity();
			while (gameEntity != null && !(gameEntity == entity))
			{
				gameEntity = gameEntity.Parent;
			}
			if (gameEntity != null)
			{
				list.Add(lastFoundAgent);
			}
			AgentProximityMap.FindNext(this, ref searchStruct);
		}
		foreach (Agent item in list)
		{
			blow.GlobalPosition = item.Position;
			AttackCollisionData collisionData = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(_attackBlockedWithShield: false, _correctSideShieldBlock: false, _isAlternativeAttack: false, _isColliderAgent: true, _collidedWithShieldOnBack: false, _isMissile: false, _isMissileBlockedWithWeapon: false, _missileHasPhysics: false, _entityExists: false, _thrustTipHit: false, _missileGoneUnderWater: false, _missileGoneOutOfBorder: false, CombatCollisionResult.StrikeAgent, -1, 0, 2, blow.BoneIndex, BoneBodyPartType.Abdomen, attackBoneIndex, Agent.UsageDirection.AttackLeft, -1, CombatHitResultFlags.NormalHit, 0.5f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, Vec3.Up, blow.Direction, blow.GlobalPosition, Vec3.Zero, Vec3.Zero, item.Velocity, Vec3.Up);
			item.RegisterBlow(blow, in collisionData);
		}
	}

	public void KillAgentCheat(Agent agent)
	{
		if (!GameNetwork.IsClientOrReplay)
		{
			Agent agent2 = MainAgent ?? agent;
			Blow blow = new Blow(agent2.Index);
			blow.DamageType = DamageTypes.Blunt;
			blow.BoneIndex = agent.Monster.HeadLookDirectionBoneIndex;
			blow.GlobalPosition = agent.Position;
			blow.GlobalPosition.z += agent.GetEyeGlobalHeight();
			blow.BaseMagnitude = 2000f;
			blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
			blow.InflictedDamage = 2000;
			blow.SwingDirection = agent.LookDirection;
			if (InputManager.IsGameKeyDown(2))
			{
				blow.SwingDirection = agent.Frame.rotation.TransformToParent(new Vec3(-1f));
				blow.SwingDirection.Normalize();
			}
			else if (InputManager.IsGameKeyDown(3))
			{
				blow.SwingDirection = agent.Frame.rotation.TransformToParent(new Vec3(1f));
				blow.SwingDirection.Normalize();
			}
			else if (InputManager.IsGameKeyDown(1))
			{
				blow.SwingDirection = agent.Frame.rotation.TransformToParent(new Vec3(0f, -1f));
				blow.SwingDirection.Normalize();
			}
			else if (InputManager.IsGameKeyDown(0))
			{
				blow.SwingDirection = agent.Frame.rotation.TransformToParent(new Vec3(0f, 1f));
				blow.SwingDirection.Normalize();
			}
			blow.Direction = blow.SwingDirection;
			blow.DamageCalculated = true;
			sbyte mainHandItemBoneIndex = agent2.Monster.MainHandItemBoneIndex;
			AttackCollisionData collisionData = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(_attackBlockedWithShield: false, _correctSideShieldBlock: false, _isAlternativeAttack: false, _isColliderAgent: true, _collidedWithShieldOnBack: false, _isMissile: false, _isMissileBlockedWithWeapon: false, _missileHasPhysics: false, _entityExists: false, _thrustTipHit: false, _missileGoneUnderWater: false, _missileGoneOutOfBorder: false, CombatCollisionResult.StrikeAgent, -1, 0, 2, blow.BoneIndex, BoneBodyPartType.Head, mainHandItemBoneIndex, Agent.UsageDirection.AttackLeft, -1, CombatHitResultFlags.NormalHit, 0.5f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, Vec3.Up, blow.Direction, blow.GlobalPosition, Vec3.Zero, Vec3.Zero, agent.Velocity, Vec3.Up);
			agent.RegisterBlow(blow, in collisionData);
		}
	}

	public bool KillCheats(bool killAll, bool killEnemy, bool killHorse, bool killYourself)
	{
		bool result = false;
		if (!GameNetwork.IsClientOrReplay)
		{
			if (killYourself)
			{
				if (MainAgent != null)
				{
					if (killHorse)
					{
						if (MainAgent.MountAgent != null)
						{
							Agent mountAgent = MainAgent.MountAgent;
							KillAgentCheat(mountAgent);
							result = true;
						}
					}
					else
					{
						Agent mainAgent = MainAgent;
						KillAgentCheat(mainAgent);
						result = true;
					}
				}
			}
			else
			{
				bool flag = false;
				int num = Agents.Count - 1;
				while (num >= 0 && !flag)
				{
					Agent agent = Agents[num];
					if (agent != MainAgent && agent.GetAgentFlags().HasAnyFlag(AgentFlag.CanAttack) && PlayerTeam != null)
					{
						if (killEnemy)
						{
							if (agent.Team.IsValid && PlayerTeam.IsEnemyOf(agent.Team))
							{
								if (killHorse && agent.HasMount)
								{
									if (agent.MountAgent != null)
									{
										KillAgentCheat(agent.MountAgent);
										if (!killAll)
										{
											flag = true;
										}
										result = true;
									}
								}
								else
								{
									KillAgentCheat(agent);
									if (!killAll)
									{
										flag = true;
									}
									result = true;
								}
							}
						}
						else if (agent.Team.IsValid && PlayerTeam.IsFriendOf(agent.Team))
						{
							if (killHorse)
							{
								if (agent.MountAgent != null)
								{
									KillAgentCheat(agent.MountAgent);
									if (!killAll)
									{
										flag = true;
									}
									result = true;
								}
							}
							else
							{
								KillAgentCheat(agent);
								if (!killAll)
								{
									flag = true;
								}
								result = true;
							}
						}
					}
					num--;
				}
			}
		}
		return result;
	}

	private bool CancelsDamageAndBlocksAttackBecauseOfNonEnemyCase(Agent attacker, Agent victim)
	{
		if (victim == null || attacker == null)
		{
			return false;
		}
		bool num = !GameNetwork.IsSessionActive || ForceNoFriendlyFire || (MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent.GetIntValue() <= 0 && MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent.GetIntValue() <= 0) || Mode == MissionMode.Duel || attacker.Controller == Agent.ControllerType.AI;
		bool flag = attacker.IsFriendOf(victim);
		if (!(num && flag))
		{
			if (victim.IsHuman)
			{
				if (!flag)
				{
					return !attacker.IsEnemyOf(victim);
				}
				return false;
			}
			return false;
		}
		return true;
	}

	public float GetDamageMultiplierOfCombatDifficulty(Agent victimAgent, Agent attackerAgent = null)
	{
		if (MissionGameModels.Current.MissionDifficultyModel != null)
		{
			return MissionGameModels.Current.MissionDifficultyModel.GetDamageMultiplierOfCombatDifficulty(victimAgent, attackerAgent);
		}
		return 1f;
	}

	public float GetShootDifficulty(Agent affectedAgent, Agent affectorAgent, bool isHeadShot)
	{
		Vec2 vec = affectedAgent.MovementVelocity - affectorAgent.MovementVelocity;
		Vec3 va = new Vec3(vec.x, vec.y);
		Vec3 vb = affectedAgent.Position - affectorAgent.Position;
		float num = vb.Normalize();
		float num2 = va.Normalize();
		float length = Vec3.CrossProduct(va, vb).Length;
		float num3 = MBMath.ClampFloat(0.3f * ((4f + num) / 4f) * ((4f + length * num2) / 4f), 1f, 12f);
		if (isHeadShot)
		{
			num3 *= 1.2f;
		}
		return num3;
	}

	private MatrixFrame CalculateAttachedLocalFrame(in MatrixFrame attachedGlobalFrame, AttackCollisionData collisionData, WeaponComponentData missileWeapon, Agent affectedAgent, GameEntity hitEntity, Vec3 missileMovementVelocity, Vec3 missileRotationSpeed, MatrixFrame shieldGlobalFrame, bool shouldMissilePenetrate, out bool isAttachedFrameLocal)
	{
		isAttachedFrameLocal = false;
		MatrixFrame matrixFrame = attachedGlobalFrame;
		bool isNonZero = missileWeapon.RotationSpeed.IsNonZero;
		bool flag = affectedAgent != null && !collisionData.AttackBlockedWithShield && missileWeapon.WeaponFlags.HasAnyFlag(WeaponFlags.AmmoSticksWhenShot);
		float managedParameter = ManagedParameters.Instance.GetManagedParameter((!flag) ? ManagedParametersEnum.ObjectMinPenetration : (isNonZero ? ManagedParametersEnum.RotatingProjectileMinPenetration : ManagedParametersEnum.ProjectileMinPenetration));
		float managedParameter2 = ManagedParameters.Instance.GetManagedParameter((!flag) ? ManagedParametersEnum.ObjectMaxPenetration : (isNonZero ? ManagedParametersEnum.RotatingProjectileMaxPenetration : ManagedParametersEnum.ProjectileMaxPenetration));
		Vec3 vec = missileMovementVelocity;
		float num = vec.Normalize();
		float num2 = MBMath.ClampFloat(flag ? ((float)collisionData.InflictedDamage / affectedAgent.HealthLimit) : (num / ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.ProjectileMaxPenetrationSpeed)), 0f, 1f);
		if (shouldMissilePenetrate)
		{
			float num3 = managedParameter + (managedParameter2 - managedParameter) * num2;
			matrixFrame.origin += vec * num3;
		}
		if (missileRotationSpeed.IsNonZero)
		{
			float managedParameter3 = ManagedParameters.Instance.GetManagedParameter(flag ? ManagedParametersEnum.AgentProjectileNormalWeight : ManagedParametersEnum.ProjectileNormalWeight);
			Vec3 vec2 = missileWeapon.GetMissileStartingFrame().TransformToParent(missileRotationSpeed);
			Vec3 vec3 = -collisionData.CollisionGlobalNormal;
			float num4 = vec2.x * vec2.x;
			float num5 = vec2.y * vec2.y;
			float num6 = vec2.z * vec2.z;
			int i = ((!(num4 > num5) || !(num4 > num6)) ? ((num5 > num6) ? 1 : 2) : 0);
			vec3 -= vec3.ProjectOnUnitVector(matrixFrame.rotation[i]);
			Vec3 v = Vec3.CrossProduct(vec, vec3.NormalizedCopy());
			float num7 = v.Normalize();
			matrixFrame.rotation.RotateAboutAnArbitraryVector(v, num7 * managedParameter3);
		}
		if (!collisionData.AttackBlockedWithShield && affectedAgent != null)
		{
			float num8 = Vec3.DotProduct(collisionData.CollisionGlobalNormal, vec) + 1f;
			if (num8 > 0.5f)
			{
				matrixFrame.origin -= num8 * 0.1f * collisionData.CollisionGlobalNormal;
			}
		}
		matrixFrame = matrixFrame.TransformToParent(missileWeapon.GetMissileStartingFrame().TransformToParent(missileWeapon.StickingFrame)).TransformToParent(missileWeapon.GetMissileStartingFrame());
		if (collisionData.AttackBlockedWithShield)
		{
			matrixFrame = shieldGlobalFrame.TransformToLocal(matrixFrame);
			isAttachedFrameLocal = true;
		}
		else if (affectedAgent != null)
		{
			if (flag)
			{
				MBAgentVisuals agentVisuals = affectedAgent.AgentVisuals;
				matrixFrame = agentVisuals.GetGlobalFrame().TransformToParent(agentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex(collisionData.CollisionBoneIndex)).GetUnitRotFrame(affectedAgent.AgentScale)
					.TransformToLocalNonOrthogonal(ref matrixFrame);
				isAttachedFrameLocal = true;
			}
		}
		else if (hitEntity != null)
		{
			if (collisionData.CollisionBoneIndex >= 0)
			{
				matrixFrame = hitEntity.Skeleton.GetBoneEntitialFrameWithIndex(collisionData.CollisionBoneIndex).TransformToLocalNonOrthogonal(ref matrixFrame);
				isAttachedFrameLocal = true;
			}
			else
			{
				matrixFrame = hitEntity.GetGlobalFrame().TransformToLocalNonOrthogonal(ref matrixFrame);
				isAttachedFrameLocal = true;
			}
		}
		else
		{
			matrixFrame.origin.z = Math.Max(matrixFrame.origin.z, -100f);
		}
		return matrixFrame;
	}

	[UsedImplicitly]
	[MBCallback]
	internal void GetDefendCollisionResults(Agent attackerAgent, Agent defenderAgent, CombatCollisionResult collisionResult, int attackerWeaponSlotIndex, bool isAlternativeAttack, StrikeType strikeType, Agent.UsageDirection attackDirection, float collisionDistanceOnWeapon, float attackProgress, bool attackIsParried, bool isPassiveUsageHit, bool isHeavyAttack, ref float defenderStunPeriod, ref float attackerStunPeriod, ref bool crushedThrough)
	{
		bool chamber = false;
		MissionCombatMechanicsHelper.GetDefendCollisionResults(attackerAgent, defenderAgent, collisionResult, attackerWeaponSlotIndex, isAlternativeAttack, strikeType, attackDirection, collisionDistanceOnWeapon, attackProgress, attackIsParried, isPassiveUsageHit, isHeavyAttack, ref defenderStunPeriod, ref attackerStunPeriod, ref crushedThrough, ref chamber);
		if ((crushedThrough || chamber) && (attackerAgent.CanLogCombatFor || defenderAgent.CanLogCombatFor))
		{
			CombatLogData combatLog = new CombatLogData(isVictimAgentSameAsAttackerAgent: false, attackerAgent.IsHuman, attackerAgent.IsMine, attackerAgent.RiderAgent != null, attackerAgent.RiderAgent != null && attackerAgent.RiderAgent.IsMine, attackerAgent.IsMount, defenderAgent.IsHuman, defenderAgent.IsMine, defenderAgent.Health <= 0f, defenderAgent.HasMount, defenderAgent.RiderAgent != null && defenderAgent.RiderAgent.IsMine, defenderAgent.IsMount, isVictimEntity: false, defenderAgent.RiderAgent == attackerAgent, crushedThrough, chamber, 0f);
			AddCombatLogSafe(attackerAgent, defenderAgent, null, combatLog);
		}
	}

	private CombatLogData GetAttackCollisionResults(Agent attackerAgent, Agent victimAgent, GameEntity hitObject, float momentumRemaining, in MissionWeapon attackerWeapon, bool crushedThrough, bool cancelDamage, bool crushedThroughWithoutAgentCollision, ref AttackCollisionData attackCollisionData, out WeaponComponentData shieldOnBack, out CombatLogData combatLog)
	{
		AttackInformation attackInformation = new AttackInformation(attackerAgent, victimAgent, hitObject, in attackCollisionData, in attackerWeapon);
		shieldOnBack = attackInformation.ShieldOnBack;
		MissionCombatMechanicsHelper.GetAttackCollisionResults(in attackInformation, crushedThrough, momentumRemaining, in attackerWeapon, cancelDamage, ref attackCollisionData, out combatLog, out var _);
		float num = attackCollisionData.InflictedDamage;
		if (num > 0f)
		{
			float num2 = MissionGameModels.Current.AgentApplyDamageModel.CalculateDamage(in attackInformation, in attackCollisionData, in attackerWeapon, num);
			combatLog.ModifiedDamage = TaleWorlds.Library.MathF.Round(num2 - num);
			attackCollisionData.InflictedDamage = TaleWorlds.Library.MathF.Round(num2);
		}
		else
		{
			combatLog.ModifiedDamage = 0;
			attackCollisionData.InflictedDamage = 0;
		}
		if (!attackCollisionData.IsFallDamage && attackInformation.IsFriendlyFire)
		{
			if (!attackInformation.IsAttackerAIControlled && GameNetwork.IsSessionActive)
			{
				int num3 = (attackCollisionData.IsMissile ? MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent.GetIntValue() : MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent.GetIntValue());
				attackCollisionData.SelfInflictedDamage = TaleWorlds.Library.MathF.Round((float)attackCollisionData.InflictedDamage * ((float)num3 * 0.01f));
				int num4 = (attackCollisionData.IsMissile ? MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent.GetIntValue() : MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent.GetIntValue());
				attackCollisionData.InflictedDamage = TaleWorlds.Library.MathF.Round((float)attackCollisionData.InflictedDamage * ((float)num4 * 0.01f));
				combatLog.InflictedDamage = attackCollisionData.InflictedDamage;
			}
			combatLog.IsFriendlyFire = true;
		}
		if (attackCollisionData.AttackBlockedWithShield && attackCollisionData.InflictedDamage > 0 && attackInformation.VictimShield.HitPoints - attackCollisionData.InflictedDamage <= 0)
		{
			attackCollisionData.IsShieldBroken = true;
		}
		if (!crushedThroughWithoutAgentCollision)
		{
			combatLog.BodyPartHit = attackCollisionData.VictimHitBodyPart;
			combatLog.IsVictimEntity = hitObject != null;
		}
		return combatLog;
	}

	private void PrintAttackCollisionResults(Agent attackerAgent, Agent victimAgent, GameEntity hitEntity, ref AttackCollisionData attackCollisionData, ref CombatLogData combatLog)
	{
		if (attackCollisionData.IsColliderAgent && !attackCollisionData.AttackBlockedWithShield && attackerAgent != null && (attackerAgent.CanLogCombatFor || victimAgent.CanLogCombatFor) && victimAgent.State == AgentState.Active)
		{
			AddCombatLogSafe(attackerAgent, victimAgent, hitEntity, combatLog);
		}
	}

	private void AddCombatLogSafe(Agent attackerAgent, Agent victimAgent, GameEntity hitEntity, CombatLogData combatLog)
	{
		combatLog.SetVictimAgent(victimAgent);
		if (GameNetwork.IsServerOrRecorder)
		{
			CombatLogNetworkMessage message = new CombatLogNetworkMessage(attackerAgent.Index, victimAgent?.Index ?? (-1), hitEntity, combatLog);
			NetworkCommunicator networkCommunicator = ((attackerAgent == null) ? null : (attackerAgent.IsHuman ? attackerAgent : attackerAgent.RiderAgent))?.MissionPeer?.Peer.Communicator as NetworkCommunicator;
			NetworkCommunicator networkCommunicator2 = ((victimAgent == null) ? null : (victimAgent.IsHuman ? victimAgent : victimAgent.RiderAgent))?.MissionPeer?.Peer.Communicator as NetworkCommunicator;
			if (networkCommunicator != null && !networkCommunicator.IsServerPeer)
			{
				GameNetwork.BeginModuleEventAsServer(networkCommunicator);
				GameNetwork.WriteMessage(message);
				GameNetwork.EndModuleEventAsServer();
			}
			if (networkCommunicator2 != null && !networkCommunicator2.IsServerPeer && networkCommunicator2 != networkCommunicator)
			{
				GameNetwork.BeginModuleEventAsServer(networkCommunicator2);
				GameNetwork.WriteMessage(message);
				GameNetwork.EndModuleEventAsServer();
			}
		}
		_combatLogsCreated.Enqueue(combatLog);
	}

	public MissionObject CreateMissionObjectFromPrefab(string prefab, MatrixFrame frame)
	{
		if (!GameNetwork.IsClientOrReplay)
		{
			GameEntity gameEntity = GameEntity.Instantiate(Scene, prefab, frame);
			MissionObject firstScriptOfType = gameEntity.GetFirstScriptOfType<MissionObject>();
			List<MissionObjectId> childObjectIds = new List<MissionObjectId>();
			foreach (GameEntity child in gameEntity.GetChildren())
			{
				MissionObject firstScriptOfType2;
				if ((firstScriptOfType2 = child.GetFirstScriptOfType<MissionObject>()) != null)
				{
					childObjectIds.Add(firstScriptOfType2.Id);
				}
			}
			if (GameNetwork.IsServerOrRecorder)
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage(new CreateMissionObject(firstScriptOfType.Id, prefab, frame, childObjectIds));
				GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
				AddDynamicallySpawnedMissionObjectInfo(new DynamicallyCreatedEntity(prefab, firstScriptOfType.Id, frame, ref childObjectIds));
			}
			return firstScriptOfType;
		}
		return null;
	}

	public int GetNearbyAllyAgentsCount(Vec2 center, float radius, Team team)
	{
		return GetNearbyAgentsCountAux(center, radius, team.MBTeam, GetNearbyAgentsAuxType.Friend);
	}

	public MBList<Agent> GetNearbyAllyAgents(Vec2 center, float radius, Team team, MBList<Agent> agents)
	{
		agents.Clear();
		GetNearbyAgentsAux(center, radius, team.MBTeam, GetNearbyAgentsAuxType.Friend, agents);
		return agents;
	}

	public MBList<Agent> GetNearbyEnemyAgents(Vec2 center, float radius, Team team, MBList<Agent> agents)
	{
		agents.Clear();
		GetNearbyAgentsAux(center, radius, team.MBTeam, GetNearbyAgentsAuxType.Enemy, agents);
		return agents;
	}

	public MBList<Agent> GetNearbyAgents(Vec2 center, float radius, MBList<Agent> agents)
	{
		agents.Clear();
		GetNearbyAgentsAux(center, radius, MBTeam.InvalidTeam, GetNearbyAgentsAuxType.All, agents);
		return agents;
	}

	public bool IsFormationUnitPositionAvailable(ref WorldPosition formationPosition, ref WorldPosition unitPosition, ref WorldPosition nearestAvailableUnitPosition, float manhattanDistance, Team team)
	{
		if (!formationPosition.IsValid || formationPosition.GetNavMesh() == UIntPtr.Zero || !unitPosition.IsValid || unitPosition.GetNavMesh() == UIntPtr.Zero)
		{
			return false;
		}
		if (this.IsFormationUnitPositionAvailable_AdditionalCondition != null && !this.IsFormationUnitPositionAvailable_AdditionalCondition(unitPosition, team))
		{
			return false;
		}
		if (Mode == MissionMode.Deployment && DeploymentPlan.HasDeploymentBoundaries(team.Side))
		{
			IMissionDeploymentPlan deploymentPlan = DeploymentPlan;
			BattleSideEnum side = team.Side;
			Vec2 position = unitPosition.AsVec2;
			if (!deploymentPlan.IsPositionInsideDeploymentBoundaries(side, in position))
			{
				return false;
			}
		}
		return IsFormationUnitPositionAvailableAux(ref formationPosition, ref unitPosition, ref nearestAvailableUnitPosition, manhattanDistance);
	}

	public bool IsOrderPositionAvailable(in WorldPosition orderPosition, Team team)
	{
		if (!orderPosition.IsValid || orderPosition.GetNavMesh() == UIntPtr.Zero)
		{
			return false;
		}
		if (this.IsFormationUnitPositionAvailable_AdditionalCondition != null && !this.IsFormationUnitPositionAvailable_AdditionalCondition(orderPosition, team))
		{
			return false;
		}
		return IsPositionInsideBoundaries(orderPosition.AsVec2);
	}

	public bool IsFormationUnitPositionAvailable(ref WorldPosition unitPosition, Team team)
	{
		WorldPosition formationPosition = unitPosition;
		float manhattanDistance = 1f;
		WorldPosition nearestAvailableUnitPosition = WorldPosition.Invalid;
		return IsFormationUnitPositionAvailable(ref formationPosition, ref unitPosition, ref nearestAvailableUnitPosition, manhattanDistance, team);
	}

	public bool HasSceneMapPatch()
	{
		return InitializerRecord.SceneHasMapPatch;
	}

	public bool GetPatchSceneEncounterPosition(out Vec3 position)
	{
		if (InitializerRecord.SceneHasMapPatch)
		{
			Vec2 patchCoordinates = InitializerRecord.PatchCoordinates;
			float northRotation = Scene.GetNorthRotation();
			Boundaries.GetOrientedBoundariesBox(out var boxMinimum, out var boxMaximum, northRotation);
			Vec2 side = Vec2.Side;
			side.RotateCCW(northRotation);
			Vec2 vec = side.LeftVec();
			Vec2 vec2 = boxMaximum - boxMinimum;
			Vec2 position2 = boxMinimum.x * side + boxMinimum.y * vec + vec2.x * patchCoordinates.x * side + vec2.y * patchCoordinates.y * vec;
			position = position2.ToVec3(Scene.GetTerrainHeight(position2));
			return true;
		}
		position = Vec3.Invalid;
		return false;
	}

	public bool GetPatchSceneEncounterDirection(out Vec2 direction)
	{
		if (InitializerRecord.SceneHasMapPatch)
		{
			float northRotation = Scene.GetNorthRotation();
			direction = InitializerRecord.PatchEncounterDir;
			direction.RotateCCW(northRotation);
			return true;
		}
		direction = Vec2.Invalid;
		return false;
	}

	private void TickDebugAgents()
	{
	}

	public void AddTimerToDynamicEntity(GameEntity gameEntity, float timeToKill = 10f)
	{
		DynamicEntityInfo item = new DynamicEntityInfo
		{
			Entity = gameEntity,
			TimerToDisable = new TaleWorlds.Core.Timer(CurrentTime, timeToKill)
		};
		_dynamicEntities.Add(item);
	}

	public void AddListener(IMissionListener listener)
	{
		_listeners.Add(listener);
	}

	public void RemoveListener(IMissionListener listener)
	{
		_listeners.Remove(listener);
	}

	public void OnAgentFleeing(Agent agent)
	{
		for (int num = MissionBehaviors.Count - 1; num >= 0; num--)
		{
			MissionBehaviors[num].OnAgentFleeing(agent);
		}
		agent.OnFleeing();
	}

	public void OnAgentPanicked(Agent agent)
	{
		for (int num = MissionBehaviors.Count - 1; num >= 0; num--)
		{
			MissionBehaviors[num].OnAgentPanicked(agent);
		}
	}

	public void OnDeploymentFinished()
	{
		foreach (Team team in Teams)
		{
			if (team.TeamAI != null)
			{
				team.TeamAI.OnDeploymentFinished();
			}
		}
		for (int num = MissionBehaviors.Count - 1; num >= 0; num--)
		{
			MissionBehaviors[num].OnDeploymentFinished();
		}
	}

	public void SetFastForwardingFromUI(bool fastForwarding)
	{
		IsFastForward = fastForwarding;
	}

	public bool CheckIfBattleInRetreat()
	{
		return this.IsBattleInRetreatEvent?.Invoke() ?? false;
	}

	public void AddSpawnedItemEntityCreatedAtRuntime(SpawnedItemEntity spawnedItemEntity)
	{
		_spawnedItemEntitiesCreatedAtRuntime.Add(spawnedItemEntity);
	}

	public void TriggerOnItemPickUpEvent(Agent agent, SpawnedItemEntity spawnedItemEntity)
	{
		this.OnItemPickUp?.Invoke(agent, spawnedItemEntity);
	}

	[UsedImplicitly]
	[MBCallback]
	internal static void DebugLogNativeMissionNetworkEvent(int eventEnum, string eventName, int bitCount)
	{
		int eventType = eventEnum + CompressionBasic.NetworkComponentEventTypeFromServerCompressionInfo.GetMaximumValue() + 1;
		DebugNetworkEventStatistics.StartEvent(eventName, eventType);
		DebugNetworkEventStatistics.AddDataToStatistic(bitCount);
		DebugNetworkEventStatistics.EndEvent();
	}

	[UsedImplicitly]
	[MBCallback]
	internal void PauseMission()
	{
		_missionState.Paused = true;
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("toggleDisableDying", "mission")]
	public static string ToggleDisableDying(List<string> strings)
	{
		if (!GameNetwork.IsSessionActive)
		{
			int result = 0;
			if (strings.Count > 0 && !int.TryParse(strings[0], out result))
			{
				return "Please write the arguments in the correct format. Correct format is: 'toggleDisableDying [index]' or just 'toggleDisableDying' for making all agents invincible.";
			}
			if (Current != null)
			{
				if (strings.Count == 0 || result == -1)
				{
					Current.DisableDying = !Current.DisableDying;
					if (Current.DisableDying)
					{
						return "Dying disabled for all";
					}
					return "Dying not disabled for all";
				}
				Agent agent = Current.FindAgentWithIndex(result);
				if (agent != null)
				{
					agent.ToggleInvulnerable();
					return "Disable Dying for agent " + result + ": " + (agent.CurrentMortalityState == Agent.MortalityState.Invulnerable);
				}
				return "Invalid agent index " + result;
			}
			return "No active mission found";
		}
		return "Does not work on multiplayer.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("toggleDisableDyingTeam", "mission")]
	public static string ToggleDisableDyingTeam(List<string> strings)
	{
		if (!GameNetwork.IsSessionActive)
		{
			int result = 0;
			if (strings.Count > 0 && !int.TryParse(strings[0], out result))
			{
				return "Please write the arguments in the correct format. Correct format is: 'toggleDisableDyingTeam [team_no]' for making all active agents of a team invincible.";
			}
			int num = 0;
			foreach (Agent allAgent in Current.AllAgents)
			{
				if (allAgent.Team != null && allAgent.Team.MBTeam.Index == result)
				{
					allAgent.ToggleInvulnerable();
					num++;
				}
			}
			return "Toggled invulnerability for active agents of team " + result.ToString() + ", agent count: " + num;
		}
		return "Does not work on multiplayer.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("killAgent", "mission")]
	public static string KillAgent(List<string> strings)
	{
		if (!GameNetwork.IsSessionActive)
		{
			if (Current != null)
			{
				if (strings.Count == 0 || !int.TryParse(strings[0], out var result))
				{
					return "Please write the arguments in the correct format. Correct format is: 'killAgent [index]'";
				}
				Agent agent = Current.FindAgentWithIndex(result);
				if (agent != null)
				{
					if (agent.State == AgentState.Active)
					{
						Current.KillAgentCheat(agent);
						return "Agent " + result + " died.";
					}
					return "Agent " + result + " already dead.";
				}
				return "Agent " + result + " not found.";
			}
			return "Current mission does not exist.";
		}
		return "Does not work on multiplayer.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_battering_ram_speed", "mission")]
	public static string IncreaseBatteringRamSpeeds(List<string> strings)
	{
		if (!GameNetwork.IsSessionActive)
		{
			if (strings.Count == 0 || !float.TryParse(strings[0], out var result))
			{
				return "Please enter a speed value";
			}
			foreach (MissionObject activeMissionObject in Current.ActiveMissionObjects)
			{
				if (activeMissionObject.GameEntity.HasScriptOfType<BatteringRam>())
				{
					activeMissionObject.GameEntity.GetFirstScriptOfType<BatteringRam>().MovementComponent.MaxSpeed = result;
					activeMissionObject.GameEntity.GetFirstScriptOfType<BatteringRam>().MovementComponent.MinSpeed = result;
				}
			}
			return "Battering ram max speed increased.";
		}
		return "Does not work on multiplayer.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("set_siege_tower_speed", "mission")]
	public static string IncreaseSiegeTowerSpeed(List<string> strings)
	{
		if (!GameNetwork.IsSessionActive)
		{
			if (strings.Count == 0 || !float.TryParse(strings[0], out var result))
			{
				return "Please enter a speed value";
			}
			foreach (MissionObject activeMissionObject in Current.ActiveMissionObjects)
			{
				if (activeMissionObject.GameEntity.HasScriptOfType<SiegeTower>())
				{
					activeMissionObject.GameEntity.GetFirstScriptOfType<SiegeTower>().MovementComponent.MaxSpeed = result;
				}
			}
			return "Siege tower max speed increased.";
		}
		return "Does not work on multiplayer.";
	}

	[CommandLineFunctionality.CommandLineArgumentFunction("reload_managed_core_params", "game")]
	public static string LoadParamsDebug(List<string> strings)
	{
		if (!GameNetwork.IsSessionActive)
		{
			ManagedParameters.Instance.Initialize(ModuleHelper.GetXmlPath("Native", "managed_core_parameters"));
			return "Managed core parameters reloaded.";
		}
		return "Does not work on multiplayer.";
	}
}
