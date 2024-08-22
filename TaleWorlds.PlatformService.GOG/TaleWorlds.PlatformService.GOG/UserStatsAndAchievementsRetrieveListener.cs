using Galaxy.Api;

namespace TaleWorlds.PlatformService.GOG;

public class UserStatsAndAchievementsRetrieveListener : GlobalUserStatsAndAchievementsRetrieveListener
{
	public delegate void UserStatsAndAchievementsRetrieved(GalaxyID userID, bool success, FailureReason? failureReason);

	public event UserStatsAndAchievementsRetrieved OnUserStatsAndAchievementsRetrieved;

	public override void OnUserStatsAndAchievementsRetrieveSuccess(GalaxyID userID)
	{
		this.OnUserStatsAndAchievementsRetrieved?.Invoke(userID, success: true, null);
	}

	public override void OnUserStatsAndAchievementsRetrieveFailure(GalaxyID userID, FailureReason failureReason)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		this.OnUserStatsAndAchievementsRetrieved?.Invoke(userID, success: false, failureReason);
	}
}
