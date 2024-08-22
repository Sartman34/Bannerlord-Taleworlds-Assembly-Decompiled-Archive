using TaleWorlds.InputSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public interface IGauntletChatLogHandlerScreen
{
	void TryUpdateChatLogLayerParameters(ref bool isTeamChatAvailable, ref bool inputEnabled, ref InputContext inputContext);
}
