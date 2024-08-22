using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;
using TaleWorlds.PlayerServices;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class AddFriendMessage : Message
{
	[JsonProperty]
	public PlayerId FriendId { get; private set; }

	[JsonProperty]
	public bool DontUseNameForUnknownPlayer { get; private set; }

	public AddFriendMessage()
	{
	}

	public AddFriendMessage(PlayerId friendId, bool dontUseNameForUnknownPlayer)
	{
		FriendId = friendId;
		DontUseNameForUnknownPlayer = dontUseNameForUnknownPlayer;
	}
}
