using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

[ScriptingInterfaceBase]
internal interface IMBScreen
{
	[EngineMethod("on_exit_button_click", false)]
	void OnExitButtonClick();

	[EngineMethod("on_edit_mode_enter_press", false)]
	void OnEditModeEnterPress();

	[EngineMethod("on_edit_mode_enter_release", false)]
	void OnEditModeEnterRelease();
}
