using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;
using TaleWorlds.MountAndBlade.Diamond;

namespace Messages.FromLobbyServer.ToClient;

[Serializable]
[MessageDescription("LobbyServer", "Client")]
public class FriendListMessage : Message
{
	[JsonProperty]
	public FriendInfo[] Friends { get; private set; }

	public FriendListMessage()
	{
	}

	public FriendListMessage(FriendInfo[] friends)
	{
		Friends = friends;
	}
}
