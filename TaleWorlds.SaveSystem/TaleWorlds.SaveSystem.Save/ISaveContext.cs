using TaleWorlds.SaveSystem.Definition;

namespace TaleWorlds.SaveSystem.Save;

internal interface ISaveContext
{
	DefinitionContext DefinitionContext { get; }

	int AddOrGetStringId(string text);

	int GetObjectId(object target);

	int GetContainerId(object target);

	int GetStringId(string target);
}
