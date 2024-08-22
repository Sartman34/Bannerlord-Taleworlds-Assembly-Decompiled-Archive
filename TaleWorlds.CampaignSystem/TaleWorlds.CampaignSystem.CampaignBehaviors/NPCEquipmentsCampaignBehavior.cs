namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class NPCEquipmentsCampaignBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this, OnNewGameCreatedPartialFollowUpEnd);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnNewGameCreatedPartialFollowUpEnd(CampaignGameStarter starter)
	{
		foreach (CharacterObject item in CharacterObject.All)
		{
			_ = item.IsTemplate;
		}
	}
}
