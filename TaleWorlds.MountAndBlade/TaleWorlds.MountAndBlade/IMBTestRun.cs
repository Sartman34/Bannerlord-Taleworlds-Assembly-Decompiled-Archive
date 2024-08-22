using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

[ScriptingInterfaceBase]
internal interface IMBTestRun
{
	[EngineMethod("auto_continue", false)]
	int AutoContinue(int type);

	[EngineMethod("get_fps", false)]
	int GetFPS();

	[EngineMethod("enter_edit_mode", false)]
	bool EnterEditMode();

	[EngineMethod("open_scene", false)]
	bool OpenScene(string sceneName);

	[EngineMethod("close_scene", false)]
	bool CloseScene();

	[EngineMethod("save_scene", false)]
	bool SaveScene();

	[EngineMethod("open_default_scene", false)]
	bool OpenDefaultScene();

	[EngineMethod("leave_edit_mode", false)]
	bool LeaveEditMode();

	[EngineMethod("new_scene", false)]
	bool NewScene();

	[EngineMethod("start_mission", false)]
	void StartMission();
}
