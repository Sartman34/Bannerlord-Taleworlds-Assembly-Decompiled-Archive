using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class ChangeClanFactionMessage : Message
{
	[JsonProperty]
	public string NewFaction { get; private set; }

	public ChangeClanFactionMessage()
	{
	}

	public ChangeClanFactionMessage(string newFaction)
	{
		NewFaction = newFaction;
	}
}
