using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public interface IFormationDeploymentPlan
{
	FormationClass Class { get; }

	FormationClass SpawnClass { get; }

	float PlannedWidth { get; }

	float PlannedDepth { get; }

	int PlannedTroopCount { get; }

	int PlannedFootTroopCount { get; }

	int PlannedMountedTroopCount { get; }

	bool HasDimensions { get; }

	bool HasSignificantMountedTroops { get; }

	bool HasFrame();

	FormationDeploymentFlank GetDefaultFlank(bool spawnWithHorses, int formationTroopCount, int infantryCount);

	FormationDeploymentOrder GetFlankDeploymentOrder(int offset = 0);

	MatrixFrame GetGroundFrame();

	Vec3 GetGroundPosition();

	Vec2 GetDirection();

	WorldPosition CreateNewDeploymentWorldPosition(WorldPosition.WorldPositionEnforcedCache worldPositionEnforcedCache);
}
