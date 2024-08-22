using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class ChangeGameTypesMessage : Message
{
	[JsonProperty]
	public string[] GameTypes { get; private set; }

	public ChangeGameTypesMessage()
	{
	}

	public ChangeGameTypesMessage(string[] gameTypes)
	{
		GameTypes = gameTypes;
	}
}
