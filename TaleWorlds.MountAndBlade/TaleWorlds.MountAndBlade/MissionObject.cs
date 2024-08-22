using System.Collections.Generic;
using System.Linq;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public abstract class MissionObject : ScriptComponentBehavior
{
	protected enum DynamicNavmeshLocalIds
	{
		Inside = 1,
		Enter,
		Exit,
		Blocker,
		Extra1,
		Extra2,
		Extra3,
		Reserved1,
		Reserved2,
		Count
	}

	public const int MaxNavMeshPerDynamicObject = 10;

	[EditableScriptComponentVariable(true)]
	protected string NavMeshPrefabName = "";

	protected int DynamicNavmeshIdStart;

	private Mission Mission => Mission.Current;

	public MissionObjectId Id { get; set; }

	public bool IsDisabled { get; private set; }

	public bool CreatedAtRuntime => Id.CreatedAtRuntime;

	public MissionObject()
	{
		MissionObjectId id = new MissionObjectId(-1);
		Id = id;
	}

	public virtual void SetAbilityOfFaces(bool enabled)
	{
		if (DynamicNavmeshIdStart > 0)
		{
			base.GameEntity.Scene.SetAbilityOfFacesWithId(DynamicNavmeshIdStart + 1, enabled);
			base.GameEntity.Scene.SetAbilityOfFacesWithId(DynamicNavmeshIdStart + 2, enabled);
			base.GameEntity.Scene.SetAbilityOfFacesWithId(DynamicNavmeshIdStart + 3, enabled);
			base.GameEntity.Scene.SetAbilityOfFacesWithId(DynamicNavmeshIdStart + 4, enabled);
			base.GameEntity.Scene.SetAbilityOfFacesWithId(DynamicNavmeshIdStart + 5, enabled);
			base.GameEntity.Scene.SetAbilityOfFacesWithId(DynamicNavmeshIdStart + 6, enabled);
			base.GameEntity.Scene.SetAbilityOfFacesWithId(DynamicNavmeshIdStart + 7, enabled);
		}
	}

	protected internal override void OnInit()
	{
		base.OnInit();
		if (!GameNetwork.IsClientOrReplay)
		{
			AttachDynamicNavmeshToEntity();
			SetAbilityOfFaces(base.GameEntity != null && base.GameEntity.IsVisibleIncludeParents());
		}
	}

	protected virtual void AttachDynamicNavmeshToEntity()
	{
		if (NavMeshPrefabName.Length > 0)
		{
			DynamicNavmeshIdStart = Mission.Current.GetNextDynamicNavMeshIdStart();
			base.GameEntity.Scene.ImportNavigationMeshPrefab(NavMeshPrefabName, DynamicNavmeshIdStart);
			GetEntityToAttachNavMeshFaces().AttachNavigationMeshFaces(DynamicNavmeshIdStart + 1, isConnected: false);
			GetEntityToAttachNavMeshFaces().AttachNavigationMeshFaces(DynamicNavmeshIdStart + 2, isConnected: true);
			GetEntityToAttachNavMeshFaces().AttachNavigationMeshFaces(DynamicNavmeshIdStart + 3, isConnected: true);
			GetEntityToAttachNavMeshFaces().AttachNavigationMeshFaces(DynamicNavmeshIdStart + 4, isConnected: false, isBlocker: true);
			SetAbilityOfFaces(base.GameEntity != null && base.GameEntity.GetPhysicsState());
		}
	}

	protected virtual GameEntity GetEntityToAttachNavMeshFaces()
	{
		return base.GameEntity;
	}

	protected internal override bool OnCheckForProblems()
	{
		base.OnCheckForProblems();
		bool result = false;
		List<GameEntity> children = new List<GameEntity>();
		children.Add(base.GameEntity);
		base.GameEntity.GetChildrenRecursive(ref children);
		bool flag = false;
		foreach (GameEntity item in children)
		{
			flag = flag || (item.HasPhysicsDefinitionWithoutFlags(1) && !item.PhysicsDescBodyFlag.HasAnyFlag(BodyFlags.CommonCollisionExcludeFlagsForMissile));
		}
		Vec3 scaleVector = base.GameEntity.GetGlobalFrame().rotation.GetScaleVector();
		bool flag2 = !(MathF.Abs(scaleVector.x - scaleVector.y) < 0.01f) || !(MathF.Abs(scaleVector.x - scaleVector.z) < 0.01f);
		if (flag && flag2)
		{
			MBEditor.AddEntityWarning(base.GameEntity, "Mission object has non-uniform scale and physics object. This is not supported because any attached focusable item to this mesh will not work within this configuration.");
			result = true;
		}
		return result;
	}

	protected internal override void OnPreInit()
	{
		base.OnPreInit();
		if (Mission != null)
		{
			int id = -1;
			bool createdAtRuntime;
			if (Mission.IsLoadingFinished)
			{
				createdAtRuntime = true;
				if (!GameNetwork.IsClientOrReplay)
				{
					id = Mission.GetFreeRuntimeMissionObjectId();
				}
			}
			else
			{
				createdAtRuntime = false;
				id = Mission.GetFreeSceneMissionObjectId();
			}
			Id = new MissionObjectId(id, createdAtRuntime);
			Mission.AddActiveMissionObject(this);
		}
		base.GameEntity.SetAsReplayEntity();
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	protected internal virtual void OnMissionReset()
	{
	}

	public virtual void AfterMissionStart()
	{
	}

	public virtual void OnMissionEnded()
	{
	}

	protected internal virtual bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
	{
		reportDamage = false;
		return false;
	}

	public void SetDisabled(bool isParentObject = false)
	{
		if (!GameNetwork.IsClientOrReplay)
		{
			SetAbilityOfFaces(enabled: false);
		}
		if (isParentObject && base.GameEntity != null)
		{
			List<GameEntity> children = new List<GameEntity>();
			base.GameEntity.GetChildrenRecursive(ref children);
			foreach (MissionObject item in from sc in children.SelectMany((GameEntity ac) => ac.GetScriptComponents())
				where sc is MissionObject
				select sc as MissionObject)
			{
				item.SetDisabled();
			}
		}
		Mission.Current.DeactivateMissionObject(this);
		IsDisabled = true;
	}

	public void SetDisabledAndMakeInvisible(bool isParentObject = false)
	{
		if (isParentObject && base.GameEntity != null)
		{
			List<GameEntity> children = new List<GameEntity>();
			base.GameEntity.GetChildrenRecursive(ref children);
			foreach (MissionObject item in from sc in children.SelectMany((GameEntity ac) => ac.GetScriptComponents())
				where sc is MissionObject
				select sc as MissionObject)
			{
				item.SetDisabledAndMakeInvisible();
			}
		}
		Mission.Current.DeactivateMissionObject(this);
		IsDisabled = true;
		if (base.GameEntity != null)
		{
			base.GameEntity.SetVisibilityExcludeParents(visible: false);
			SetScriptComponentToTick(GetTickRequirement());
		}
	}

	protected override void OnRemoved(int removeReason)
	{
		base.OnRemoved(removeReason);
		if (!GameNetwork.IsClientOrReplay)
		{
			SetAbilityOfFaces(enabled: false);
		}
		if (Mission != null)
		{
			Mission.OnMissionObjectRemoved(this, removeReason);
		}
	}

	public virtual void OnEndMission()
	{
	}

	protected internal override bool MovesEntity()
	{
		return true;
	}

	public virtual void AddStuckMissile(GameEntity missileEntity)
	{
		base.GameEntity.AddChild(missileEntity);
	}
}
