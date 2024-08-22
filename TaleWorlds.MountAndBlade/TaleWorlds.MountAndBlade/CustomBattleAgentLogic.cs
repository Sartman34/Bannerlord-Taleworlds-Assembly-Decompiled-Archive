using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade;

public class CustomBattleAgentLogic : MissionLogic
{
	public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		if (affectedAgent.Character != null && affectorAgent?.Character != null && affectedAgent.State == AgentState.Active)
		{
			bool isFatal = affectedAgent.Health - (float)blow.InflictedDamage < 1f;
			bool isTeamKill = affectedAgent.Team.Side == affectorAgent.Team.Side;
			affectorAgent.Origin.OnScoreHit(affectedAgent.Character, affectorAgent.Formation?.Captain?.Character, blow.InflictedDamage, isFatal, isTeamKill, affectorWeapon.CurrentUsageItem);
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		if ((affectorAgent == null && affectedAgent.IsMount && agentState == AgentState.Routed) || affectedAgent.Origin == null)
		{
			return;
		}
		switch (agentState)
		{
		case AgentState.Unconscious:
			affectedAgent.Origin.SetWounded();
			if (affectedAgent == base.Mission.MainAgent)
			{
				BecomeGhost();
			}
			break;
		case AgentState.Killed:
			affectedAgent.Origin.SetKilled();
			break;
		default:
			affectedAgent.Origin.SetRouted();
			break;
		}
	}

	private void BecomeGhost()
	{
		Agent leader = base.Mission.PlayerEnemyTeam.Leader;
		if (leader != null)
		{
			leader.Controller = Agent.ControllerType.AI;
		}
		base.Mission.MainAgent.Controller = Agent.ControllerType.AI;
	}
}
