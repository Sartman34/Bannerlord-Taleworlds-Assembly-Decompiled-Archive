using System;
using TaleWorlds.Diamond.ChatSystem.Library;

namespace TaleWorlds.MountAndBlade.Diamond;

public interface IChatClientHandler
{
	void OnChatMessageReceived(Guid roomId, string roomName, string playerName, string textMessage, string textColor, MessageType type);
}
