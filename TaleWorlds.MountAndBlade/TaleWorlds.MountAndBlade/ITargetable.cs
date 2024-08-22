using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public interface ITargetable
{
	TargetFlags GetTargetFlags();

	float GetTargetValue(List<Vec3> referencePositions);

	GameEntity GetTargetEntity();

	Vec3 GetTargetingOffset();

	BattleSideEnum GetSide();

	GameEntity Entity();
}
