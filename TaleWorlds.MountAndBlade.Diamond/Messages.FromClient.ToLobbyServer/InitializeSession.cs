using System;
using Newtonsoft.Json;
using TaleWorlds.Diamond;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;

namespace Messages.FromClient.ToLobbyServer;

[Serializable]
[MessageDescription("Client", "LobbyServer")]
public class InitializeSession : LoginMessage
{
	[JsonProperty]
	public PlayerId PlayerId { get; private set; }

	[JsonProperty]
	public string PlayerName { get; private set; }

	[JsonProperty]
	public AccessObject AccessObject { get; private set; }

	[JsonProperty]
	public ApplicationVersion ApplicationVersion { get; private set; }

	[JsonProperty]
	public string ConnectionPassword { get; private set; }

	[JsonProperty]
	public ModuleInfoModel[] LoadedModules { get; private set; }

	public InitializeSession()
	{
	}

	public InitializeSession(PlayerId playerId, string playerName, AccessObject accessObject, ApplicationVersion applicationVersion, string connectionPassword, ModuleInfoModel[] loadedModules)
		: base(playerId.ConvertToPeerId())
	{
		PlayerId = playerId;
		PlayerName = playerName;
		AccessObject = accessObject;
		ApplicationVersion = applicationVersion;
		ConnectionPassword = connectionPassword;
		LoadedModules = loadedModules;
	}
}
