using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Missions.Handlers;

public class BattleDeploymentHandler : DeploymentHandler
{
	public event Action OnDeploymentReady;

	public event Action OnAIDeploymentReady;

	public BattleDeploymentHandler(bool isPlayerAttacker)
		: base(isPlayerAttacker)
	{
	}

	public override void OnTeamDeployed(Team team)
	{
		if (team.IsPlayerTeam)
		{
			this.OnDeploymentReady?.Invoke();
		}
		else
		{
			this.OnAIDeploymentReady?.Invoke();
		}
	}

	public override void FinishDeployment()
	{
		base.FinishDeployment();
		Mission obj = base.Mission ?? Mission.Current;
		obj.GetMissionBehavior<DeploymentMissionController>().FinishDeployment();
		obj.IsTeleportingAgents = false;
	}

	public Vec2 GetEstimatedAverageDefenderPosition()
	{
		base.Mission.GetFormationSpawnFrame(BattleSideEnum.Defender, FormationClass.Infantry, isReinforcement: false, out var spawnPosition, out var _);
		return spawnPosition.AsVec2;
	}
}
