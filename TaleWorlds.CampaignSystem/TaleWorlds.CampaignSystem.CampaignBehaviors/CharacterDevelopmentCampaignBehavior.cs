namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class CharacterDevelopmentCampaignBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, DailyTickHero);
	}

	private void DailyTickHero(Hero hero)
	{
		if (!hero.IsChild && hero.IsAlive && (hero.Clan != Clan.PlayerClan || (hero != Hero.MainHero && CampaignOptions.AutoAllocateClanMemberPerks)) && hero.PartyBelongedTo?.MapEvent == null)
		{
			hero.HeroDeveloper.DevelopCharacterStats();
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
