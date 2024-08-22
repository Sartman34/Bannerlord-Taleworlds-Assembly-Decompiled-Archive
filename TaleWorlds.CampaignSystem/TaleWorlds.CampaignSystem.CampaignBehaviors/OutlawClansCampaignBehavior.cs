namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class OutlawClansCampaignBehavior : CampaignBehaviorBase
{
	private static void MakeOutlawFactionsEnemyToKingdomFactions()
	{
		foreach (Clan item in Clan.All)
		{
			if (!item.IsMinorFaction || !item.IsOutlaw)
			{
				continue;
			}
			foreach (Kingdom item2 in Kingdom.All)
			{
				if (item2.Culture == item.Culture)
				{
					FactionManager.DeclareWar(item2, item, isAtConstantWar: true);
				}
			}
		}
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
	}

	private void OnNewGameCreated(CampaignGameStarter starter)
	{
		MakeOutlawFactionsEnemyToKingdomFactions();
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
