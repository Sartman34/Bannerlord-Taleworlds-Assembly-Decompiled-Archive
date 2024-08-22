using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class BuyCosmeticMessage : Message
{
	[JsonProperty]
	public string CosmeticId { get; private set; }

	public BuyCosmeticMessage()
	{
	}

	public BuyCosmeticMessage(string cosmeticId)
	{
		CosmeticId = cosmeticId;
	}
}
