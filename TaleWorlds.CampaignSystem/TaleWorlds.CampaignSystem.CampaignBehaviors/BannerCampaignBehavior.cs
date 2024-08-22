using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class BannerCampaignBehavior : CampaignBehaviorBase
{
	private const int BannerLevel1CooldownDays = 4;

	private const int BannerLevel2CooldownDays = 8;

	private const int BannerLevel3CooldownDays = 12;

	private const float BannerItemUpdateChance = 0.1f;

	private Dictionary<Hero, CampaignTime> _heroNextBannerLootTime = new Dictionary<Hero, CampaignTime>();

	public override void RegisterEvents()
	{
		CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, GiveBannersToHeroes);
		CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, DailyTickHero);
		CampaignEvents.CollectLootsEvent.AddNonSerializedListener(this, CollectLoots);
		CampaignEvents.HeroComesOfAgeEvent.AddNonSerializedListener(this, OnHeroComesOfAge);
		CampaignEvents.HeroCreated.AddNonSerializedListener(this, OnHeroCreated);
		CampaignEvents.OnCompanionClanCreatedEvent.AddNonSerializedListener(this, OnCompanionClanCreated);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_heroNextBannerLootTime", ref _heroNextBannerLootTime);
	}

	private void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
	{
		GiveBannersToHeroes();
	}

	private void GiveBannersToHeroes()
	{
		foreach (Hero allAliveHero in Hero.AllAliveHeroes)
		{
			if (CanBannerBeGivenToHero(allAliveHero))
			{
				ItemObject randomBannerItemForHero = BannerHelper.GetRandomBannerItemForHero(allAliveHero);
				if (randomBannerItemForHero != null)
				{
					allAliveHero.BannerItem = new EquipmentElement(randomBannerItemForHero);
				}
			}
		}
	}

	private void DailyTickHero(Hero hero)
	{
		if (hero.Clan == Clan.PlayerClan)
		{
			return;
		}
		EquipmentElement bannerItem = hero.BannerItem;
		BannerItemModel bannerItemModel = Campaign.Current.Models.BannerItemModel;
		if (bannerItem.IsInvalid() || !bannerItemModel.CanBannerBeUpdated(bannerItem.Item) || !(MBRandom.RandomFloat < 0.1f))
		{
			return;
		}
		int bannerLevel = ((BannerComponent)bannerItem.Item.ItemComponent).BannerLevel;
		int bannerItemLevelForHero = bannerItemModel.GetBannerItemLevelForHero(hero);
		if (bannerLevel != bannerItemLevelForHero)
		{
			ItemObject upgradeBannerForHero = GetUpgradeBannerForHero(hero, bannerItemLevelForHero);
			if (upgradeBannerForHero != null)
			{
				hero.BannerItem = new EquipmentElement(upgradeBannerForHero);
			}
		}
	}

	private ItemObject GetUpgradeBannerForHero(Hero hero, int upgradeBannerLevel)
	{
		ItemObject item = hero.BannerItem.Item;
		foreach (ItemObject possibleRewardBannerItem in Campaign.Current.Models.BannerItemModel.GetPossibleRewardBannerItems())
		{
			BannerComponent bannerComponent = (BannerComponent)possibleRewardBannerItem.ItemComponent;
			if (possibleRewardBannerItem.Culture == item.Culture && bannerComponent.BannerLevel == upgradeBannerLevel && bannerComponent.BannerEffect == ((BannerComponent)item.ItemComponent).BannerEffect)
			{
				return possibleRewardBannerItem;
			}
		}
		return BannerHelper.GetRandomBannerItemForHero(hero);
	}

	private void CollectLoots(MapEvent mapEvent, PartyBase party, Dictionary<PartyBase, ItemRoster> loot, ItemRoster lootedItems, MBList<TroopRosterElement> lootedCasualties, float lootAmount)
	{
		if (party != PartyBase.MainParty || !mapEvent.IsPlayerMapEvent || mapEvent.WinningSide != mapEvent.PlayerSide)
		{
			return;
		}
		ItemObject[] array = new ItemObject[2];
		if (mapEvent.IsHideoutBattle)
		{
			array[0] = GetBannerRewardForHideoutBattle();
		}
		else if (mapEvent.AttackerSide.MissionSide == mapEvent.PlayerSide && mapEvent.IsSiegeAssault)
		{
			array[0] = GetBannerRewardForCapturingFortification(mapEvent.MapEventSettlement);
		}
		array[1] = GetBannerRewardForDefeatingNoble(mapEvent, lootedCasualties);
		foreach (ItemObject itemObject in array)
		{
			if (itemObject != null)
			{
				lootedItems.AddToCounts(itemObject, 1);
			}
		}
	}

	private void OnHeroComesOfAge(Hero hero)
	{
		if (CanBannerBeGivenToHero(hero))
		{
			ItemObject randomBannerItemForHero = BannerHelper.GetRandomBannerItemForHero(hero);
			if (randomBannerItemForHero != null)
			{
				hero.BannerItem = new EquipmentElement(randomBannerItemForHero);
			}
		}
	}

	private void OnHeroCreated(Hero hero, bool isBornNaturally = false)
	{
		if (CanBannerBeGivenToHero(hero))
		{
			ItemObject randomBannerItemForHero = BannerHelper.GetRandomBannerItemForHero(hero);
			if (randomBannerItemForHero != null)
			{
				hero.BannerItem = new EquipmentElement(randomBannerItemForHero);
			}
		}
	}

	private void OnCompanionClanCreated(Clan clan)
	{
		Hero leader = clan.Leader;
		if (leader.BannerItem.IsInvalid())
		{
			ItemObject randomBannerItemForHero = BannerHelper.GetRandomBannerItemForHero(leader);
			if (randomBannerItemForHero != null)
			{
				leader.BannerItem = new EquipmentElement(randomBannerItemForHero);
			}
		}
	}

	private bool CanBannerBeLootedFromHero(Hero hero)
	{
		if (_heroNextBannerLootTime.ContainsKey(hero))
		{
			return _heroNextBannerLootTime[hero].IsPast;
		}
		return true;
	}

	private int GetCooldownDays(int bannerLevel)
	{
		if (bannerLevel == 1)
		{
			return 4;
		}
		if (bannerLevel == 1)
		{
			return 8;
		}
		return 12;
	}

	private void LogBannerLootForHero(Hero hero, int bannerLevel)
	{
		CampaignTime value = CampaignTime.DaysFromNow(GetCooldownDays(bannerLevel));
		if (!_heroNextBannerLootTime.ContainsKey(hero))
		{
			_heroNextBannerLootTime.Add(hero, value);
		}
		else
		{
			_heroNextBannerLootTime[hero] = value;
		}
	}

	private ItemObject GetBannerRewardForHideoutBattle()
	{
		if (MBRandom.RandomFloat <= Campaign.Current.Models.BattleRewardModel.DestroyHideoutBannerLootChance)
		{
			return Campaign.Current.Models.BannerItemModel.GetPossibleRewardBannerItems().ToMBList().GetRandomElementWithPredicate((ItemObject i) => ((BannerComponent)i.ItemComponent).BannerLevel == 1 && (i.Culture == null || i.Culture.StringId == "neutral_culture"));
		}
		return null;
	}

	private ItemObject GetBannerRewardForCapturingFortification(Settlement settlement)
	{
		if (MBRandom.RandomFloat <= Campaign.Current.Models.BattleRewardModel.CaptureSettlementBannerLootChance)
		{
			MBList<ItemObject> mBList = Campaign.Current.Models.BannerItemModel.GetPossibleRewardBannerItems().ToMBList();
			mBList.Shuffle();
			int wallLevel = settlement.Town.GetWallLevel();
			foreach (ItemObject item in mBList)
			{
				if (((BannerComponent)item.ItemComponent).BannerLevel == wallLevel && (item.Culture == null || item.Culture.StringId == "neutral_culture" || item.Culture == settlement.Culture))
				{
					return item;
				}
			}
		}
		return null;
	}

	private ItemObject GetBannerRewardForDefeatingNoble(MapEvent mapEvent, MBList<TroopRosterElement> lootedCasualties)
	{
		Hero hero = null;
		foreach (MapEventParty item in mapEvent.PartiesOnSide(mapEvent.DefeatedSide))
		{
			if (item.Party.IsMobile && item.Party.MobileParty.Army != null)
			{
				hero = item.Party.MobileParty.Army.ArmyOwner;
				if (hero.BannerItem.IsInvalid())
				{
					hero = null;
				}
				break;
			}
		}
		if (hero == null)
		{
			hero = lootedCasualties.GetRandomElementWithPredicate((TroopRosterElement t) => t.Character.IsHero && !t.Character.HeroObject.BannerItem.IsInvalid()).Character?.HeroObject;
		}
		if (hero != null && CanBannerBeLootedFromHero(hero))
		{
			float num = ((hero.Clan?.Kingdom?.RulingClan.Leader == hero) ? Campaign.Current.Models.BattleRewardModel.DefeatKingdomRulerBannerLootChance : ((hero.Clan?.Leader != hero) ? Campaign.Current.Models.BattleRewardModel.DefeatRegularHeroBannerLootChance : Campaign.Current.Models.BattleRewardModel.DefeatClanLeaderBannerLootChance));
			if (MBRandom.RandomFloat <= num)
			{
				LogBannerLootForHero(hero, ((BannerComponent)hero.BannerItem.Item.ItemComponent).BannerLevel);
				return hero.BannerItem.Item;
			}
		}
		return null;
	}

	private bool CanBannerBeGivenToHero(Hero hero)
	{
		int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
		if (hero.Occupation == Occupation.Lord && hero.Age >= (float)heroComesOfAge && hero.BannerItem.IsInvalid())
		{
			return hero.Clan != Clan.PlayerClan;
		}
		return false;
	}
}
