namespace TaleWorlds.MountAndBlade;

public class AgentHumanAILogic : MissionLogic
{
	public override void OnAgentCreated(Agent agent)
	{
		base.OnAgentCreated(agent);
		if (agent.IsAIControlled && agent.IsHuman)
		{
			agent.AddComponent(new HumanAIComponent(agent));
		}
	}

	protected internal override void OnAgentControllerChanged(Agent agent, Agent.ControllerType oldController)
	{
		base.OnAgentControllerChanged(agent, oldController);
		if (agent.IsHuman)
		{
			if (agent.Controller == Agent.ControllerType.AI)
			{
				agent.AddComponent(new HumanAIComponent(agent));
			}
			else if (oldController == Agent.ControllerType.AI && agent.HumanAIComponent != null)
			{
				agent.RemoveComponent(agent.HumanAIComponent);
			}
		}
	}
}
