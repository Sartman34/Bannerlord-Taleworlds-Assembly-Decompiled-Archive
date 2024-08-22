namespace TaleWorlds.MountAndBlade;

public class ResetAnimationOnStopUsageComponent : UsableMissionObjectComponent
{
	private readonly ActionIndexCache _successfulResetAction = ActionIndexCache.act_none;

	public ResetAnimationOnStopUsageComponent(ActionIndexCache successfulResetActionCode)
	{
		_successfulResetAction = successfulResetActionCode;
	}

	protected internal override void OnUseStopped(Agent userAgent, bool isSuccessful = true)
	{
		ActionIndexCache actionIndexCache = (isSuccessful ? _successfulResetAction : ActionIndexCache.act_none);
		if (actionIndexCache == ActionIndexCache.act_none)
		{
			userAgent.SetActionChannel(1, actionIndexCache, ignorePriority: false, 72uL);
		}
		userAgent.SetActionChannel(0, actionIndexCache, ignorePriority: false, 72uL);
	}
}
