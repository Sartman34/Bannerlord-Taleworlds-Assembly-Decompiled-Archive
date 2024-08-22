namespace TaleWorlds.MountAndBlade;

public interface IUsable
{
	void OnUse(Agent userAgent);

	void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex);
}
