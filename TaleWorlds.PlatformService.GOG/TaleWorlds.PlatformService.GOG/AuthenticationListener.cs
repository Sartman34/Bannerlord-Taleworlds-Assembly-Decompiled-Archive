using Galaxy.Api;
using TaleWorlds.Library;

namespace TaleWorlds.PlatformService.GOG;

public class AuthenticationListener : GlobalAuthListener
{
	private GOGPlatformServices _gogPlatformServices;

	public bool GotResult { get; private set; }

	public AuthenticationListener(GOGPlatformServices gogPlatformServices)
	{
		_gogPlatformServices = gogPlatformServices;
	}

	public override void OnAuthSuccess()
	{
		Debug.Print("Successfully signed in");
		GalaxyInstance.User().GetGalaxyID();
		GotResult = true;
	}

	public override void OnAuthFailure(FailureReason failureReason)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Debug.Print("Failed to sign in for reason " + failureReason);
		GotResult = true;
	}

	public override void OnAuthLost()
	{
		Debug.Print("Authorization lost");
		GotResult = true;
	}
}
