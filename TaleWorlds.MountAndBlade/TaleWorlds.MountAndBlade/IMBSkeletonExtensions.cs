using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

[ScriptingInterfaceBase]
internal interface IMBSkeletonExtensions
{
	[EngineMethod("create_agent_skeleton", false)]
	Skeleton CreateAgentSkeleton(string skeletonName, bool isHumanoid, int actionSetIndex, string monsterUsageSetName, ref AnimationSystemData animationSystemData);

	[EngineMethod("create_simple_skeleton", false)]
	Skeleton CreateSimpleSkeleton(string skeletonName);

	[EngineMethod("create_with_action_set", false)]
	Skeleton CreateWithActionSet(ref AnimationSystemData animationSystemData);

	[EngineMethod("get_skeleton_face_animation_time", false)]
	float GetSkeletonFaceAnimationTime(UIntPtr entityId);

	[EngineMethod("set_skeleton_face_animation_time", false)]
	void SetSkeletonFaceAnimationTime(UIntPtr entityId, float time);

	[EngineMethod("get_skeleton_face_animation_name", false)]
	string GetSkeletonFaceAnimationName(UIntPtr entityId);

	[EngineMethod("get_bone_entitial_frame_at_animation_progress", false)]
	void GetBoneEntitialFrameAtAnimationProgress(UIntPtr skeletonPointer, sbyte boneIndex, int animationIndex, float progress, ref MatrixFrame outFrame);

	[EngineMethod("get_bone_entitial_frame", false)]
	void GetBoneEntitialFrame(UIntPtr skeletonPointer, sbyte bone, bool useBoneMapping, bool forceToUpdate, ref MatrixFrame outFrame);

	[EngineMethod("set_animation_at_channel", false)]
	void SetAnimationAtChannel(UIntPtr skeletonPointer, int animationIndex, int channelNo, float animationSpeedMultiplier, float blendInPeriod, float startProgress);

	[EngineMethod("get_action_at_channel", false)]
	int GetActionAtChannel(UIntPtr skeletonPointer, int channelNo);

	[EngineMethod("set_facial_animation_of_channel", false)]
	void SetFacialAnimationOfChannel(UIntPtr skeletonPointer, int channel, string facialAnimationName, bool playSound, bool loop);

	[EngineMethod("set_agent_action_channel", false)]
	void SetAgentActionChannel(UIntPtr skeletonPointer, int actionChannelNo, int actionIndex, float channelParameter, float blendPeriodOverride, bool forceFaceMorphRestart);

	[EngineMethod("does_action_continue_with_current_action_at_channel", false)]
	bool DoesActionContinueWithCurrentActionAtChannel(UIntPtr skeletonPointer, int actionChannelNo, int actionIndex);

	[EngineMethod("tick_action_channels", false)]
	void TickActionChannels(UIntPtr skeletonPointer);
}
