using TaleWorlds.DotNet;

namespace TaleWorlds.Engine;

public abstract class MessageManagerBase : DotNetObject
{
	[EngineCallback]
	protected internal abstract void PostWarningLine(string text);

	[EngineCallback]
	protected internal abstract void PostSuccessLine(string text);

	[EngineCallback]
	protected internal abstract void PostMessageLineFormatted(string text, uint color);

	[EngineCallback]
	protected internal abstract void PostMessageLine(string text, uint color);
}
