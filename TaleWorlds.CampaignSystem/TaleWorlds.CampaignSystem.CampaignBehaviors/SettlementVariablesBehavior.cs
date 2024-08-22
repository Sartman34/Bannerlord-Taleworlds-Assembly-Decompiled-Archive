using TaleWorlds.CampaignSystem.Settlements;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class SettlementVariablesBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.HourlyTickSettlementEvent.AddNonSerializedListener(this, HourlyTickSettlement);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void HourlyTickSettlement(Settlement settlement)
	{
		if ((settlement.IsVillage || settlement.IsFortification || settlement.IsHideout) && settlement.LastAttackerParty != null && settlement.LastThreatTime.ElapsedHoursUntilNow > 24f)
		{
			settlement.LastAttackerParty = null;
		}
	}
}
