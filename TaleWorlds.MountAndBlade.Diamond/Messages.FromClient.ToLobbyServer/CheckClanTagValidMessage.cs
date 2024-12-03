using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class CheckClanTagValidMessage : Message
{
	[JsonProperty]
	public string ClanTag { get; private set; }

	public CheckClanTagValidMessage()
	{
	}

	public CheckClanTagValidMessage(string clanTag)
	{
		ClanTag = clanTag;
	}
}
