using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class UpdateUsingClanSigil : Message
{
	[JsonProperty]
	public bool IsUsed { get; private set; }

	public UpdateUsingClanSigil()
	{
	}

	public UpdateUsingClanSigil(bool isUsed)
	{
		IsUsed = isUsed;
	}
}
