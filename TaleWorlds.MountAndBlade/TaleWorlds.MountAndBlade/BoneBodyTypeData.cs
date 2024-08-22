using TaleWorlds.DotNet;

namespace TaleWorlds.MountAndBlade;

[EngineStruct("Bone_body_type_data", false)]
public struct BoneBodyTypeData
{
	[CustomEngineStructMemberData(true)]
	public readonly BoneBodyPartType BodyPartType;

	[CustomEngineStructMemberData(true)]
	public readonly sbyte Priority;

	[CustomEngineStructMemberData(true)]
	public readonly SkeletonModelBoundsRecFlags DataFlags;
}
