using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class ChangeUsernameMessage : Message
{
	[JsonProperty]
	public string Username { get; private set; }

	public ChangeUsernameMessage()
	{
	}

	public ChangeUsernameMessage(string username)
	{
		Username = username;
	}
}
