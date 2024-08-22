using System;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface IScriptComponent
{
	[EngineMethod("get_script_component_behavior", false)]
	ScriptComponentBehavior GetScriptComponentBehavior(UIntPtr pointer);

	[EngineMethod("set_variable_editor_widget_status", false)]
	void SetVariableEditorWidgetStatus(UIntPtr pointer, string field, bool enabled);
}
