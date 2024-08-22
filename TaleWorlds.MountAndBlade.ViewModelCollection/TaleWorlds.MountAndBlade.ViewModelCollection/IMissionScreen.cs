namespace TaleWorlds.MountAndBlade.ViewModelCollection;

public interface IMissionScreen
{
	bool GetDisplayDialog();

	float GetCameraElevation();

	void SetOrderFlagVisibility(bool value);

	string GetFollowText();

	string GetFollowPartyText();
}
