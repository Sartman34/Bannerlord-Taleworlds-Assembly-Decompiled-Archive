using System.Collections.Generic;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class MissionBoundaryPlacer : MissionLogic
{
	public const string DefaultWalkAreaBoundaryName = "walk_area";

	public override void EarlyStart()
	{
		AddMissionBoundaries();
	}

	public void AddMissionBoundaries()
	{
		string boundaryName;
		List<Vec2> sceneBoundaryPoints = MBSceneUtilities.GetSceneBoundaryPoints(base.Mission.Scene, out boundaryName);
		base.Mission.Boundaries.Add(boundaryName, sceneBoundaryPoints);
	}
}
