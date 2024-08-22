using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

[ScriptingInterfaceBase]
internal interface IMBActionSet
{
	[EngineMethod("get_index_with_id", false)]
	int GetIndexWithID(string id);

	[EngineMethod("get_name_with_index", false)]
	string GetNameWithIndex(int index);

	[EngineMethod("get_skeleton_name", false)]
	string GetSkeletonName(int index);

	[EngineMethod("get_number_of_action_sets", false)]
	int GetNumberOfActionSets();

	[EngineMethod("get_number_of_monster_usage_sets", false)]
	int GetNumberOfMonsterUsageSets();

	[EngineMethod("get_animation_name", false)]
	string GetAnimationName(int index, int actionNo);

	[EngineMethod("are_actions_alternatives", false)]
	bool AreActionsAlternatives(int index, int actionNo1, int actionNo2);

	[EngineMethod("get_bone_index_with_id", false)]
	sbyte GetBoneIndexWithId(string actionSetId, string boneId);

	[EngineMethod("get_bone_has_parent_bone", false)]
	bool GetBoneHasParentBone(string actionSetId, sbyte boneIndex);
}
