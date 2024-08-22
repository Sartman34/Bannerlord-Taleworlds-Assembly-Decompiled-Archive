using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;
using TaleWorlds.MountAndBlade.Diamond;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class ResponseCustomGameClientConnectionMessage : Message
{
	[JsonProperty]
	public PlayerJoinGameResponseDataFromHost[] PlayerJoinData { get; private set; }

	public ResponseCustomGameClientConnectionMessage()
	{
	}

	public ResponseCustomGameClientConnectionMessage(PlayerJoinGameResponseDataFromHost[] playerJoinData)
	{
		PlayerJoinData = playerJoinData;
	}
}
