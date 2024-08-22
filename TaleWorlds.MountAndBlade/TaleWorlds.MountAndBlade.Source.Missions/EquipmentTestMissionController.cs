using TaleWorlds.Core;
using TaleWorlds.Engine;

namespace TaleWorlds.MountAndBlade.Source.Missions;

public class EquipmentTestMissionController : MissionLogic
{
	public override void AfterStart()
	{
		base.AfterStart();
		GameEntity entity = base.Mission.Scene.FindEntityWithTag("spawnpoint_player");
		base.Mission.SpawnAgent(new AgentBuildData(Game.Current.PlayerTroop).Team(base.Mission.AttackerTeam).InitialFrameFromSpawnPointEntity(entity).CivilianEquipment(civilianEquipment: false)
			.Controller(Agent.ControllerType.Player));
	}
}
