using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade;

public class SiegeDeploymentMissionController : DeploymentMissionController
{
	private SiegeDeploymentHandler _siegeDeploymentHandler;

	public SiegeDeploymentMissionController(bool isPlayerAttacker)
		: base(isPlayerAttacker)
	{
	}

	public override void OnBehaviorInitialize()
	{
		base.OnBehaviorInitialize();
		_siegeDeploymentHandler = base.Mission.GetMissionBehavior<SiegeDeploymentHandler>();
	}

	public override void AfterStart()
	{
		base.Mission.GetMissionBehavior<DeploymentHandler>().InitializeDeploymentPoints();
		base.AfterStart();
	}

	protected override void SetupTeamsOfSide(BattleSideEnum side)
	{
		Team team = ((side == BattleSideEnum.Attacker) ? base.Mission.AttackerTeam : base.Mission.DefenderTeam);
		if (team == base.Mission.PlayerTeam)
		{
			_siegeDeploymentHandler.RemoveUnavailableDeploymentPoints(side);
			_siegeDeploymentHandler.UnHideDeploymentPoints(side);
			_siegeDeploymentHandler.DeployAllSiegeWeaponsOfPlayer();
		}
		else
		{
			_siegeDeploymentHandler.DeployAllSiegeWeaponsOfAi();
		}
		MissionAgentSpawnLogic.SetSpawnTroops(side, spawnTroops: true, enforceSpawning: true);
		foreach (GameEntity item in base.Mission.GetActiveEntitiesWithScriptComponentOfType<SiegeWeapon>())
		{
			SiegeWeapon siegeWeapon = item.GetScriptComponents<SiegeWeapon>().FirstOrDefault();
			if (siegeWeapon != null && siegeWeapon.GetSide() == side)
			{
				siegeWeapon.TickAuxForInit();
			}
		}
		SetupTeamsOfSideAux(side);
		if (team != base.Mission.PlayerTeam)
		{
			return;
		}
		foreach (Formation item2 in team.FormationsIncludingEmpty)
		{
			if (item2.CountOfUnits > 0)
			{
				item2.SetControlledByAI(isControlledByAI: true);
			}
		}
	}

	public override void OnBeforeDeploymentFinished()
	{
		BattleSideEnum side = base.Mission.PlayerTeam.Side;
		_siegeDeploymentHandler.RemoveDeploymentPoints(side);
		foreach (SiegeLadder item in (from sl in Mission.Current.ActiveMissionObjects.FindAllWithType<SiegeLadder>()
			where !sl.GameEntity.IsVisibleIncludeParents()
			select sl).ToList())
		{
			item.SetDisabledSynched();
		}
		OnSideDeploymentFinished(side);
	}

	public override void OnAfterDeploymentFinished()
	{
		base.Mission.RemoveMissionBehavior(_siegeDeploymentHandler);
	}

	public List<ItemObject> GetSiegeMissiles()
	{
		List<ItemObject> list = new List<ItemObject>();
		ItemObject @object = MBObjectManager.Instance.GetObject<ItemObject>("grapeshot_fire_projectile");
		list.Add(@object);
		foreach (GameEntity item in Mission.Current.GetActiveEntitiesWithScriptComponentOfType<RangedSiegeWeapon>())
		{
			RangedSiegeWeapon firstScriptOfType = item.GetFirstScriptOfType<RangedSiegeWeapon>();
			if (!string.IsNullOrEmpty(firstScriptOfType.MissileItemID))
			{
				ItemObject object2 = MBObjectManager.Instance.GetObject<ItemObject>(firstScriptOfType.MissileItemID);
				if (!list.Contains(object2))
				{
					list.Add(object2);
				}
			}
		}
		foreach (GameEntity item2 in Mission.Current.GetActiveEntitiesWithScriptComponentOfType<StonePile>())
		{
			StonePile firstScriptOfType2 = item2.GetFirstScriptOfType<StonePile>();
			if (!string.IsNullOrEmpty(firstScriptOfType2.GivenItemID))
			{
				ItemObject object3 = MBObjectManager.Instance.GetObject<ItemObject>(firstScriptOfType2.GivenItemID);
				if (!list.Contains(object3))
				{
					list.Add(object3);
				}
			}
		}
		return list;
	}
}
