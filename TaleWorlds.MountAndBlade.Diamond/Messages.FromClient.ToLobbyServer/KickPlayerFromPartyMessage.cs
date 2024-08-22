using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;
using TaleWorlds.PlayerServices;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class KickPlayerFromPartyMessage : Message
{
	[JsonProperty]
	public PlayerId KickedPlayerId { get; private set; }

	public KickPlayerFromPartyMessage()
	{
	}

	public KickPlayerFromPartyMessage(PlayerId kickedPlayerId)
	{
		KickedPlayerId = kickedPlayerId;
	}
}
