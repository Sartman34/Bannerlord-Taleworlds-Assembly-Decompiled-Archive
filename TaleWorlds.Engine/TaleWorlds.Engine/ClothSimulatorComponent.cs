using System;
using TaleWorlds.DotNet;

namespace TaleWorlds.Engine;

[EngineClass("rglCloth_simulator_component")]
public sealed class ClothSimulatorComponent : GameEntityComponent
{
	internal ClothSimulatorComponent(UIntPtr pointer)
		: base(pointer)
	{
	}

	public void SetMaxDistanceMultiplier(float multiplier)
	{
		EngineApplicationInterface.IClothSimulatorComponent.SetMaxDistanceMultiplier(base.Pointer, multiplier);
	}
}
