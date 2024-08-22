using TaleWorlds.DotNet;

namespace TaleWorlds.Engine;

[EngineStruct("rglIntersection::Intersection_type", false)]
public enum IntersectionType : uint
{
	Body,
	Terrain,
	Invalid
}
