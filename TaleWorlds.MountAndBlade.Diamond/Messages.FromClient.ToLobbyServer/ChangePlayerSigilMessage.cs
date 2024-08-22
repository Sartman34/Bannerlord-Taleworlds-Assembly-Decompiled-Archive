using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class ChangePlayerSigilMessage : Message
{
	[JsonProperty]
	public string SigilId { get; private set; }

	public ChangePlayerSigilMessage()
	{
	}

	public ChangePlayerSigilMessage(string sigilId)
	{
		SigilId = sigilId;
	}
}
