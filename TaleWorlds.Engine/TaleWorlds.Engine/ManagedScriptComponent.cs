using System;
using TaleWorlds.DotNet;

namespace TaleWorlds.Engine;

[EngineClass("rglManaged_script_component")]
public sealed class ManagedScriptComponent : ScriptComponent
{
	public ScriptComponentBehavior ScriptComponentBehavior => EngineApplicationInterface.IScriptComponent.GetScriptComponentBehavior(base.Pointer);

	public void SetVariableEditorWidgetStatus(string field, bool enabled)
	{
		EngineApplicationInterface.IScriptComponent.SetVariableEditorWidgetStatus(base.Pointer, field, enabled);
	}

	private ManagedScriptComponent()
	{
	}

	internal ManagedScriptComponent(UIntPtr pointer)
		: base(pointer)
	{
	}
}
