using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;

namespace Messages.FromLobbyServer.ToClient;

[Serializable]
[MessageDescription("LobbyServer", "Client")]
public class ChatRoomClosedMessage : Message
{
	[JsonProperty]
	public Guid ChatRoomId { get; private set; }

	public ChatRoomClosedMessage()
	{
	}

	public ChatRoomClosedMessage(Guid chatRoomId)
	{
		ChatRoomId = chatRoomId;
	}
}
