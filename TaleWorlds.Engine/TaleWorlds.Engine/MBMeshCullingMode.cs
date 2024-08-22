using TaleWorlds.DotNet;

namespace TaleWorlds.Engine;

[EngineStruct("rglCull_mode", false)]
public enum MBMeshCullingMode : byte
{
	None,
	Backfaces,
	Frontfaces,
	Count
}
