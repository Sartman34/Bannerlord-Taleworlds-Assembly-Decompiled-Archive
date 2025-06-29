using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;

namespace Messages.FromLobbyServer.ToClient;

[Serializable]
public class GetRankedLeaderboardCountMessageResult : FunctionResult
{
	[JsonProperty]
	public int Count { get; private set; }

	public GetRankedLeaderboardCountMessageResult()
	{
	}

	public GetRankedLeaderboardCountMessageResult(int count)
	{
		Count = count;
	}
}
