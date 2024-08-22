using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;
using TaleWorlds.PlayerServices;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("LobbyServer", "Client")]
public class KickFromClanMessage : Message
{
	[JsonProperty]
	public PlayerId PlayerId { get; private set; }

	public KickFromClanMessage()
	{
	}

	public KickFromClanMessage(PlayerId playerId)
	{
		PlayerId = playerId;
	}
}
