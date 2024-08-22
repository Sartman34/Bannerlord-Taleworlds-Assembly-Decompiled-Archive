using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Source.Missions;

public class SimpleMountedPlayerMissionController : MissionLogic
{
	private Game _game;

	public SimpleMountedPlayerMissionController()
	{
		_game = Game.Current;
	}

	public override void AfterStart()
	{
		BasicCharacterObject @object = _game.ObjectManager.GetObject<BasicCharacterObject>("aserai_tribal_horseman");
		MatrixFrame matrixFrame = Mission.Current.Scene.FindEntityWithTag("sp_play")?.GetGlobalFrame() ?? MatrixFrame.Identity;
		AgentBuildData agentBuildData = new AgentBuildData(new BasicBattleAgentOrigin(@object));
		AgentBuildData agentBuildData2 = agentBuildData.InitialPosition(in matrixFrame.origin);
		Vec2 direction = matrixFrame.rotation.f.AsVec2.Normalized();
		agentBuildData2.InitialDirection(in direction).Controller(Agent.ControllerType.Player);
		base.Mission.SpawnAgent(agentBuildData).WieldInitialWeapons();
	}

	public override bool MissionEnded(ref MissionResult missionResult)
	{
		if (base.Mission.InputManager.IsGameKeyPressed(4))
		{
			return true;
		}
		return false;
	}
}
