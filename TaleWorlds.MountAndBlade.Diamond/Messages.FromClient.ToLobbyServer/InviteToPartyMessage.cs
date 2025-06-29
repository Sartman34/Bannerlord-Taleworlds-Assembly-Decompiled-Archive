using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;
using TaleWorlds.PlayerServices;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class InviteToPartyMessage : Message
{
	[JsonProperty]
	public PlayerId InvitedPlayerId { get; private set; }

	[JsonProperty]
	public bool DontUseNameForUnknownPlayer { get; private set; }

	public InviteToPartyMessage()
	{
	}

	public InviteToPartyMessage(PlayerId invitedPlayerId, bool dontUseNameForUnknownPlayer)
	{
		InvitedPlayerId = invitedPlayerId;
		DontUseNameForUnknownPlayer = dontUseNameForUnknownPlayer;
	}
}
