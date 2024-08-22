using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

[ScriptingInterfaceBase]
internal interface IMBWorld
{
	[EngineMethod("get_global_time", false)]
	float GetGlobalTime(MBCommon.TimeType timeType);

	[EngineMethod("get_last_messages", false)]
	string GetLastMessages();

	[EngineMethod("get_game_type", false)]
	int GetGameType();

	[EngineMethod("set_game_type", false)]
	void SetGameType(int gameType);

	[EngineMethod("pause_game", false)]
	void PauseGame();

	[EngineMethod("unpause_game", false)]
	void UnpauseGame();

	[EngineMethod("set_mesh_used", false)]
	void SetMeshUsed(string meshName);

	[EngineMethod("set_material_used", false)]
	void SetMaterialUsed(string materialName);

	[EngineMethod("set_body_used", false)]
	void SetBodyUsed(string bodyName);

	[EngineMethod("fix_skeletons", false)]
	void FixSkeletons();

	[EngineMethod("check_resource_modifications", false)]
	void CheckResourceModifications();
}
