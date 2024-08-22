using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

public class DeploymentMissionView : MissionView
{
	private OrderTroopPlacer _orderTroopPlacer;

	private MissionDeploymentBoundaryMarker _deploymentBoundaryMarkerHandler;

	private MissionEntitySelectionUIHandler _entitySelectionHandler;

	public OnPlayerDeploymentFinishDelegate OnDeploymentFinish;

	public override void AfterStart()
	{
		_orderTroopPlacer = base.Mission.GetMissionBehavior<OrderTroopPlacer>();
		_entitySelectionHandler = base.Mission.GetMissionBehavior<MissionEntitySelectionUIHandler>();
		_deploymentBoundaryMarkerHandler = base.Mission.GetMissionBehavior<MissionDeploymentBoundaryMarker>();
	}

	public override void OnInitialDeploymentPlanMadeForSide(BattleSideEnum side, bool isFirstPlan)
	{
		if (side == base.Mission.PlayerTeam.Side && base.Mission.DeploymentPlan.HasDeploymentBoundaries(base.Mission.PlayerTeam.Side))
		{
			_orderTroopPlacer?.RestrictOrdersToDeploymentBoundaries(enabled: true);
		}
	}

	public override void OnDeploymentFinished()
	{
		OnDeploymentFinish();
		if (_entitySelectionHandler != null)
		{
			base.Mission.RemoveMissionBehavior(_entitySelectionHandler);
		}
		if (_deploymentBoundaryMarkerHandler != null)
		{
			if (base.Mission.DeploymentPlan.HasDeploymentBoundaries(base.Mission.PlayerTeam.Side))
			{
				_orderTroopPlacer?.RestrictOrdersToDeploymentBoundaries(enabled: false);
			}
			base.Mission.RemoveMissionBehavior(_deploymentBoundaryMarkerHandler);
		}
		if (!base.Mission.HasMissionBehavior<MissionBoundaryWallView>())
		{
			MissionBoundaryWallView missionView = new MissionBoundaryWallView();
			base.MissionScreen.AddMissionView(missionView);
		}
	}
}
