using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class NotablePowerManagementBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.HeroCreated.AddNonSerializedListener(this, OnHeroCreated);
		CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, DailyTickHero);
		CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, OnRaidCompleted);
	}

	private void OnHeroCreated(Hero hero, bool isMaternal)
	{
		if (hero.IsNotable)
		{
			hero.AddPower(Campaign.Current.Models.NotablePowerModel.GetInitialPower());
		}
	}

	private void DailyTickHero(Hero hero)
	{
		if (hero.IsAlive && hero.IsNotable)
		{
			hero.AddPower(Campaign.Current.Models.NotablePowerModel.CalculateDailyPowerChangeForHero(hero).ResultNumber);
		}
	}

	private void OnRaidCompleted(BattleSideEnum winnerSide, RaidEventComponent mapEvent)
	{
		foreach (Hero notable in mapEvent.MapEventSettlement.Notables)
		{
			notable.AddPower(-5f);
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
