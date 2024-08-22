namespace TaleWorlds.CampaignSystem;

public interface ISaveManager
{
	int GetAutoSaveInterval();

	void OnSaveOver(bool isSuccessful, string newSaveGameName);
}
