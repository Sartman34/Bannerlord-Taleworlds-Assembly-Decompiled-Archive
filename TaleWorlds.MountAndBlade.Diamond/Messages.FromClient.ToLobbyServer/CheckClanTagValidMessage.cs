using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("LobbyServer", "Client")]
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
