using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.AI;

namespace TaleWorlds.MountAndBlade.Missions.Handlers;

public class SiegeDeploymentHandler : BattleDeploymentHandler
{
	private IMissionSiegeWeaponsController _defenderSiegeWeaponsController;

	private IMissionSiegeWeaponsController _attackerSiegeWeaponsController;

	public IEnumerable<DeploymentPoint> PlayerDeploymentPoints { get; private set; }

	public IEnumerable<DeploymentPoint> AllDeploymentPoints { get; private set; }

	public SiegeDeploymentHandler(bool isPlayerAttacker)
		: base(isPlayerAttacker)
	{
	}

	public override void OnBehaviorInitialize()
	{
		MissionSiegeEnginesLogic missionBehavior = base.Mission.GetMissionBehavior<MissionSiegeEnginesLogic>();
		_defenderSiegeWeaponsController = missionBehavior.GetSiegeWeaponsController(BattleSideEnum.Defender);
		_attackerSiegeWeaponsController = missionBehavior.GetSiegeWeaponsController(BattleSideEnum.Attacker);
	}

	public override void AfterStart()
	{
		base.AfterStart();
		AllDeploymentPoints = Mission.Current.ActiveMissionObjects.FindAllWithType<DeploymentPoint>();
		PlayerDeploymentPoints = AllDeploymentPoints.Where((DeploymentPoint dp) => dp.Side == base.team.Side);
		foreach (DeploymentPoint allDeploymentPoint in AllDeploymentPoints)
		{
			allDeploymentPoint.OnDeploymentStateChanged += OnDeploymentStateChange;
		}
		base.Mission.IsFormationUnitPositionAvailable_AdditionalCondition += Mission_IsFormationUnitPositionAvailable_AdditionalCondition;
	}

	public override void OnRemoveBehavior()
	{
		base.OnRemoveBehavior();
		base.Mission.IsFormationUnitPositionAvailable_AdditionalCondition -= Mission_IsFormationUnitPositionAvailable_AdditionalCondition;
	}

	public override void FinishDeployment()
	{
		foreach (DeploymentPoint allDeploymentPoint in AllDeploymentPoints)
		{
			allDeploymentPoint.OnDeploymentStateChanged -= OnDeploymentStateChange;
		}
		base.FinishDeployment();
	}

	public void DeployAllSiegeWeaponsOfPlayer()
	{
		BattleSideEnum side = (isPlayerAttacker ? BattleSideEnum.Attacker : BattleSideEnum.Defender);
		new SiegeWeaponAutoDeployer((from dp in base.Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>()
			where dp.Side == side
			select dp).ToList(), GetWeaponsControllerOfSide(side)).DeployAll(side);
	}

	public int GetMaxDeployableWeaponCountOfPlayer(Type weapon)
	{
		return GetWeaponsControllerOfSide(isPlayerAttacker ? BattleSideEnum.Attacker : BattleSideEnum.Defender).GetMaxDeployableWeaponCount(weapon);
	}

	public void DeployAllSiegeWeaponsOfAi()
	{
		BattleSideEnum side = ((!isPlayerAttacker) ? BattleSideEnum.Attacker : BattleSideEnum.Defender);
		new SiegeWeaponAutoDeployer((from dp in base.Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>()
			where dp.Side == side
			select dp).ToList(), GetWeaponsControllerOfSide(side)).DeployAll(side);
		RemoveDeploymentPoints(side);
	}

	public void RemoveDeploymentPoints(BattleSideEnum side)
	{
		DeploymentPoint[] array = (from dp in base.Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>()
			where dp.Side == side
			select dp).ToArray();
		foreach (DeploymentPoint deploymentPoint in array)
		{
			SynchedMissionObject[] array2 = deploymentPoint.DeployableWeapons.ToArray();
			foreach (SynchedMissionObject synchedMissionObject in array2)
			{
				if ((deploymentPoint.DeployedWeapon == null || !synchedMissionObject.GameEntity.IsVisibleIncludeParents()) && synchedMissionObject is SiegeWeapon siegeWeapon)
				{
					siegeWeapon.SetDisabledSynched();
				}
			}
			deploymentPoint.SetDisabledSynched();
		}
	}

	public void RemoveUnavailableDeploymentPoints(BattleSideEnum side)
	{
		IMissionSiegeWeaponsController weapons = ((side == BattleSideEnum.Defender) ? _defenderSiegeWeaponsController : _attackerSiegeWeaponsController);
		DeploymentPoint[] array = (from dp in base.Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>()
			where dp.Side == side
			select dp).ToArray();
		foreach (DeploymentPoint deploymentPoint in array)
		{
			if (deploymentPoint.DeployableWeaponTypes.Any((Type wt) => weapons.GetMaxDeployableWeaponCount(wt) > 0))
			{
				continue;
			}
			foreach (SiegeWeapon item in deploymentPoint.DeployableWeapons.Select((SynchedMissionObject sw) => sw as SiegeWeapon))
			{
				item.SetDisabledSynched();
			}
			deploymentPoint.SetDisabledSynched();
		}
	}

	public void UnHideDeploymentPoints(BattleSideEnum side)
	{
		foreach (DeploymentPoint item in from dp in base.Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>()
			where !dp.IsDisabled && dp.Side == side
			select dp)
		{
			item.Show();
		}
	}

	public int GetDeployableWeaponCountOfPlayer(Type weapon)
	{
		return GetWeaponsControllerOfSide(isPlayerAttacker ? BattleSideEnum.Attacker : BattleSideEnum.Defender).GetMaxDeployableWeaponCount(weapon) - PlayerDeploymentPoints.Count((DeploymentPoint dp) => dp.IsDeployed && MissionSiegeWeaponsController.GetWeaponType(dp.DeployedWeapon) == weapon);
	}

	protected bool Mission_IsFormationUnitPositionAvailable_AdditionalCondition(WorldPosition position, Team team)
	{
		if (team != null && team.Side == BattleSideEnum.Defender)
		{
			Scene scene = base.Mission.Scene;
			Vec3 globalPosition = scene.FindEntityWithTag("defender_infantry").GlobalPosition;
			WorldPosition position2 = new WorldPosition(scene, UIntPtr.Zero, globalPosition, hasValidZ: false);
			return scene.DoesPathExistBetweenPositions(position2, position);
		}
		return true;
	}

	private void OnDeploymentStateChange(DeploymentPoint deploymentPoint, SynchedMissionObject targetObject)
	{
		if (!deploymentPoint.IsDeployed && base.team.DetachmentManager.ContainsDetachment(deploymentPoint.DisbandedWeapon as IDetachment))
		{
			base.team.DetachmentManager.DestroyDetachment(deploymentPoint.DisbandedWeapon as IDetachment);
		}
		if (targetObject is SiegeWeapon missionWeapon)
		{
			IMissionSiegeWeaponsController weaponsControllerOfSide = GetWeaponsControllerOfSide(deploymentPoint.Side);
			if (deploymentPoint.IsDeployed)
			{
				weaponsControllerOfSide.OnWeaponDeployed(missionWeapon);
			}
			else
			{
				weaponsControllerOfSide.OnWeaponUndeployed(missionWeapon);
			}
		}
	}

	private IMissionSiegeWeaponsController GetWeaponsControllerOfSide(BattleSideEnum side)
	{
		if (side != 0)
		{
			return _attackerSiegeWeaponsController;
		}
		return _defenderSiegeWeaponsController;
	}

	[Conditional("DEBUG")]
	private void AssertSiegeWeapons(IEnumerable<DeploymentPoint> allDeploymentPoints)
	{
		HashSet<SynchedMissionObject> hashSet = new HashSet<SynchedMissionObject>();
		foreach (SynchedMissionObject item in allDeploymentPoints.SelectMany((DeploymentPoint amo) => amo.DeployableWeapons))
		{
			if (!hashSet.Add(item))
			{
				break;
			}
		}
	}
}
