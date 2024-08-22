using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;
using TaleWorlds.MountAndBlade.Diamond;

namespace Messages.FromLobbyServer.ToClient;

[Serializable]
[MessageDescription("LobbyServer", "Client")]
public class JoinChatRoomMessage : Message
{
	[JsonProperty]
	public ChatRoomInformationForClient ChatRoomInformaton { get; private set; }

	public JoinChatRoomMessage()
	{
	}

	public JoinChatRoomMessage(ChatRoomInformationForClient chatRoomInformation)
	{
		ChatRoomInformaton = chatRoomInformation;
	}
}
