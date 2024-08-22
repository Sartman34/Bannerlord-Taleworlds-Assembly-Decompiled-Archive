using System;
using TaleWorlds.DotNet;

namespace TaleWorlds.Engine;

[Flags]
[EngineStruct("rglPhysics_material::rglPhymat_flags", false)]
public enum PhysicsMaterialFlags : byte
{
	None = 0,
	DontStickMissiles = 1,
	Flammable = 2,
	RainSplashesEnabled = 4,
	AttacksCanPassThrough = 8
}
