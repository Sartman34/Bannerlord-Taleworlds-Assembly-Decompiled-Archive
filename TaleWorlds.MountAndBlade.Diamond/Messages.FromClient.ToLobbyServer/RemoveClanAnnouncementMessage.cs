using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class RemoveClanAnnouncementMessage : Message
{
	[JsonProperty]
	public int AnnouncementId { get; private set; }

	public RemoveClanAnnouncementMessage()
	{
	}

	public RemoveClanAnnouncementMessage(int announcementId)
	{
		AnnouncementId = announcementId;
	}
}
