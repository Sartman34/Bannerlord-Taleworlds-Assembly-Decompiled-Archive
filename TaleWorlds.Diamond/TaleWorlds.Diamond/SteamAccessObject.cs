using System;
using Newtonsoft.Json;

namespace TaleWorlds.Diamond;

[Serializable]
public class SteamAccessObject : AccessObject
{
	[JsonProperty]
	public string UserName { get; private set; }

	[JsonProperty]
	public string ExternalAccessToken { get; private set; }

	[JsonProperty]
	public int AppId { get; private set; }

	[JsonProperty]
	public string AppTicket { get; private set; }

	public SteamAccessObject()
	{
	}

	public SteamAccessObject(string userName, string externalAccessToken, int appId, string appTicket)
	{
		base.Type = "Steam";
		UserName = userName;
		ExternalAccessToken = externalAccessToken;
		AppId = appId;
		AppTicket = appTicket;
	}
}
