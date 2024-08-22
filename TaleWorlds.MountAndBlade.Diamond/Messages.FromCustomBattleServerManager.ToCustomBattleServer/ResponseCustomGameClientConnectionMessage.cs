using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;
using TaleWorlds.MountAndBlade.Diamond;

namespace Messages.FromCustomBattleServerManager.ToCustomBattleServer;

[Serializable]
[MessageDescription("CustomBattleServer", "CustomBattleServerManager")]
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
