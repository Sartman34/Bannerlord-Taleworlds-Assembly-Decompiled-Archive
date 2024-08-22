using System;
using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade;

public class BattleObserverMissionLogic : MissionLogic
{
	private int[] _builtAgentCountForSides;

	private int[] _removedAgentCountForSides;

	public IBattleObserver BattleObserver { get; private set; }

	public void SetObserver(IBattleObserver observer)
	{
		BattleObserver = observer;
	}

	public override void AfterStart()
	{
		_builtAgentCountForSides = new int[2];
		_removedAgentCountForSides = new int[2];
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		if (agent.IsHuman)
		{
			BattleSideEnum side = agent.Team.Side;
			BattleObserver.TroopNumberChanged(side, agent.Origin.BattleCombatant, agent.Character, 1);
			_builtAgentCountForSides[(int)side]++;
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		if (affectedAgent.IsHuman)
		{
			BattleSideEnum side = affectedAgent.Team.Side;
			switch (agentState)
			{
			case AgentState.Routed:
				BattleObserver.TroopNumberChanged(side, affectedAgent.Origin.BattleCombatant, affectedAgent.Character, -1, 0, 0, 1);
				break;
			case AgentState.Unconscious:
				BattleObserver.TroopNumberChanged(side, affectedAgent.Origin.BattleCombatant, affectedAgent.Character, -1, 0, 1);
				break;
			case AgentState.Killed:
				BattleObserver.TroopNumberChanged(side, affectedAgent.Origin.BattleCombatant, affectedAgent.Character, -1, 1);
				break;
			default:
				throw new ArgumentOutOfRangeException("agentState", agentState, null);
			}
			_removedAgentCountForSides[(int)side]++;
			if (affectorAgent != null && affectorAgent.IsHuman && (agentState == AgentState.Unconscious || agentState == AgentState.Killed))
			{
				BattleObserver.TroopNumberChanged(affectorAgent.Team.Side, affectorAgent.Origin.BattleCombatant, affectorAgent.Character, 0, 0, 0, 0, 1);
			}
		}
	}

	public override void OnMissionResultReady(MissionResult missionResult)
	{
		if (missionResult.PlayerVictory)
		{
			BattleObserver.BattleResultsReady();
		}
	}

	public float GetDeathToBuiltAgentRatioForSide(BattleSideEnum side)
	{
		return (float)_removedAgentCountForSides[(int)side] / (float)_builtAgentCountForSides[(int)side];
	}
}
