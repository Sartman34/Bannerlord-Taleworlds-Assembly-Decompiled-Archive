namespace TaleWorlds.MountAndBlade;

public class AgentCommonAILogic : MissionLogic
{
	public override void OnAgentCreated(Agent agent)
	{
		base.OnAgentCreated(agent);
		if (agent.IsAIControlled)
		{
			agent.AddComponent(new CommonAIComponent(agent));
		}
	}

	protected internal override void OnAgentControllerChanged(Agent agent, Agent.ControllerType oldController)
	{
		base.OnAgentControllerChanged(agent, oldController);
		if (agent.Controller == Agent.ControllerType.AI)
		{
			agent.AddComponent(new CommonAIComponent(agent));
		}
		else if (oldController == Agent.ControllerType.AI && agent.CommonAIComponent != null)
		{
			agent.RemoveComponent(agent.CommonAIComponent);
		}
	}
}
