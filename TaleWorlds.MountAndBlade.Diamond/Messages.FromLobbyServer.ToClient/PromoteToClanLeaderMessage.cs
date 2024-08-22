using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;
using TaleWorlds.PlayerServices;

namespace Messages.FromLobbyServer.ToClient;

[Serializable]
[MessageDescription("LobbyServer", "Client")]
public class PromoteToClanLeaderMessage : Message
{
	[JsonProperty]
	public PlayerId PromotedPlayerId { get; private set; }

	[JsonProperty]
	public bool DontUseNameForUnknownPlayer { get; private set; }

	public PromoteToClanLeaderMessage()
	{
	}

	public PromoteToClanLeaderMessage(PlayerId promotedPlayerId, bool dontUseNameForUnknownPlayer)
	{
		PromotedPlayerId = promotedPlayerId;
		DontUseNameForUnknownPlayer = dontUseNameForUnknownPlayer;
	}
}
