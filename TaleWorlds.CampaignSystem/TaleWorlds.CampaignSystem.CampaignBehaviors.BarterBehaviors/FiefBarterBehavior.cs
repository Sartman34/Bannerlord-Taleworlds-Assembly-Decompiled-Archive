using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors.BarterBehaviors;

public class FiefBarterBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.BarterablesRequested.AddNonSerializedListener(this, CheckForBarters);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	public void CheckForBarters(BarterData args)
	{
		if (args.OffererHero == null || args.OtherHero == null || !args.OffererHero.GetPerkValue(DefaultPerks.Trade.EverythingHasAPrice))
		{
			return;
		}
		foreach (Settlement item in Settlement.All)
		{
			if (!item.IsVillage)
			{
				if (item.OwnerClan?.Leader == args.OffererHero && !args.OtherHero.Clan.IsUnderMercenaryService)
				{
					Barterable barterable = new FiefBarterable(item, args.OffererHero, args.OtherHero);
					args.AddBarterable<FiefBarterGroup>(barterable);
				}
				else if (item.OwnerClan?.Leader == args.OtherHero && !args.OffererHero.Clan.IsUnderMercenaryService)
				{
					Barterable barterable2 = new FiefBarterable(item, args.OtherHero, args.OffererHero);
					args.AddBarterable<FiefBarterGroup>(barterable2);
				}
			}
		}
	}
}
