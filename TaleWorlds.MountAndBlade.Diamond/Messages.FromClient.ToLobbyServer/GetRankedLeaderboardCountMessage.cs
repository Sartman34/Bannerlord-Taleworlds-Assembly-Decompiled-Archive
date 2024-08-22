using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class GetRankedLeaderboardCountMessage : Message
{
	[JsonProperty]
	public string GameType { get; private set; }

	public GetRankedLeaderboardCountMessage()
	{
	}

	public GetRankedLeaderboardCountMessage(string gameType)
	{
		GameType = gameType;
	}
}
