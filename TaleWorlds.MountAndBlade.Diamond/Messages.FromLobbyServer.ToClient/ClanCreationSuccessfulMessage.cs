using System;
using TaleWorlds.Diamond;

namespace Messages.FromLobbyServer.ToClient;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class ClanCreationSuccessfulMessage : Message
{
}
