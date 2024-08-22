using Galaxy.Api;
using TaleWorlds.Library;

namespace TaleWorlds.PlatformService.GOG;

public class GogServicesConnectionStateListener : GlobalGogServicesConnectionStateListener
{
	public override void OnConnectionStateChange(GogServicesConnectionState connected)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Debug.Print("Connection state to GOG services changed to " + connected);
	}
}
