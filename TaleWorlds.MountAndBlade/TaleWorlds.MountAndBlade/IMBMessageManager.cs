using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

[ScriptingInterfaceBase]
internal interface IMBMessageManager
{
	[EngineMethod("display_message", false)]
	void DisplayMessage(string message);

	[EngineMethod("display_message_with_color", false)]
	void DisplayMessageWithColor(string message, uint color);

	[EngineMethod("set_message_manager", false)]
	void SetMessageManager(MessageManagerBase messageManager);
}
