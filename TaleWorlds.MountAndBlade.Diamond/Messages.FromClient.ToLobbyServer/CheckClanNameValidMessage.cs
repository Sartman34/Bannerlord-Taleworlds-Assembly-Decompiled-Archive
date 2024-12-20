using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class CheckClanNameValidMessage : Message
{
	[JsonProperty]
	public string ClanName { get; private set; }

	public CheckClanNameValidMessage()
	{
	}

	public CheckClanNameValidMessage(string clanName)
	{
		ClanName = clanName;
	}
}
