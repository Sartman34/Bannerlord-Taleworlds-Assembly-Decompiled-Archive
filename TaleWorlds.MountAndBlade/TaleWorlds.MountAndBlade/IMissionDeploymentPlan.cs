using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public interface IMissionDeploymentPlan
{
	bool IsPlanMadeForBattleSide(BattleSideEnum side, DeploymentPlanType planType);

	bool IsPositionInsideDeploymentBoundaries(BattleSideEnum battleSide, in Vec2 position);

	bool HasDeploymentBoundaries(BattleSideEnum side);

	MBReadOnlyList<(string id, List<Vec2> points)> GetDeploymentBoundaries(BattleSideEnum side);

	Vec2 GetClosestDeploymentBoundaryPosition(BattleSideEnum battleSide, in Vec2 position, bool withNavMesh = false, float positionZ = 0f);

	int GetTroopCountForSide(BattleSideEnum side, DeploymentPlanType planType);

	Vec3 GetMeanPositionOfPlan(BattleSideEnum battleSide, DeploymentPlanType planType);

	MatrixFrame GetBattleSideDeploymentFrame(BattleSideEnum side);

	IFormationDeploymentPlan GetFormationPlan(BattleSideEnum side, FormationClass fClass, DeploymentPlanType planType);
}
