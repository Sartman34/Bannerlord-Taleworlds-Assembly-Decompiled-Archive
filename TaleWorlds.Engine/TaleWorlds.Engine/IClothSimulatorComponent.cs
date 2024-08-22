using System;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface IClothSimulatorComponent
{
	[EngineMethod("set_maxdistance_multiplier", false)]
	void SetMaxDistanceMultiplier(UIntPtr cloth_pointer, float multiplier);
}
