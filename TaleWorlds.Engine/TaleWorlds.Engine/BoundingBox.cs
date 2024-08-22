using TaleWorlds.DotNet;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[EngineStruct("rglBounding_box", false)]
public struct BoundingBox
{
	[CustomEngineStructMemberData("box_min_")]
	public Vec3 min;

	[CustomEngineStructMemberData("box_max_")]
	public Vec3 max;

	[CustomEngineStructMemberData("box_center_")]
	public Vec3 center;

	[CustomEngineStructMemberData("radius_")]
	public float radius;
}
