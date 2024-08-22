using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class NotablesCampaignBehavior : CampaignBehaviorBase
{
	private const int GoldLimitForNotablesToStartGainingPower = 10000;

	private const int GoldLimitForNotablesToStartLosingPower = 5000;

	private const int GoldNeededToGainOnePower = 500;

	private const int CaravanGoldLowLimit = 5000;

	private const int RemoveNotableCharacterAfterDays = 7;

	private Dictionary<Settlement, int> _settlementPassedDaysForWeeklyTick;

	public NotablesCampaignBehavior()
	{
		_settlementPassedDaysForWeeklyTick = new Dictionary<Settlement, int>();
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener(this, OnNewGameCreatedPartialFollowUp);
		CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
		CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
		CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
		CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, WeeklyTick);
		CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, DailyTickHero);
		CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailyTickSettlement);
	}

	private void WeeklyTick()
	{
		foreach (Hero item in Hero.DeadOrDisabledHeroes.ToList())
		{
			if (item.IsDead && item.IsNotable && item.DeathDay.ElapsedDaysUntilNow >= 7f)
			{
				Campaign.Current.CampaignObjectManager.UnregisterDeadHero(item);
			}
		}
	}

	private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
	{
		WeeklyTick();
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_settlementPassedDaysForWeeklyTick", ref _settlementPassedDaysForWeeklyTick);
	}

	public void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
	{
		SpawnNotablesAtGameStart();
	}

	private void DetermineRelation(Hero hero1, Hero hero2, float randomValue, float chanceOfConflict)
	{
		float num = 0.3f;
		if (randomValue < num)
		{
			int num2 = (int)((num - randomValue) * (num - randomValue) / (num * num) * 100f);
			if (num2 > 0)
			{
				ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero1, hero2, num2);
			}
		}
		else if (randomValue > 1f - chanceOfConflict)
		{
			int num3 = -(int)((randomValue - (1f - chanceOfConflict)) * (randomValue - (1f - chanceOfConflict)) / (chanceOfConflict * chanceOfConflict) * 100f);
			if (num3 < 0)
			{
				ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero1, hero2, num3);
			}
		}
	}

	public void SetInitialRelationsBetweenNotablesAndLords()
	{
		foreach (Settlement item in Settlement.All)
		{
			for (int i = 0; i < item.Notables.Count; i++)
			{
				Hero hero = item.Notables[i];
				foreach (Hero lord in item.MapFaction.Lords)
				{
					if (lord != hero && lord == lord.Clan.Leader && lord.MapFaction == item.MapFaction)
					{
						float chanceOfConflict = (float)HeroHelper.NPCPersonalityClashWithNPC(hero, lord) * 0.01f * 2.5f;
						float randomFloat = MBRandom.RandomFloat;
						float num = Campaign.MapDiagonal;
						foreach (Settlement settlement in lord.Clan.Settlements)
						{
							float num2 = ((item == settlement) ? 0f : settlement.Position2D.Distance(item.Position2D));
							if (num2 < num)
							{
								num = num2;
							}
						}
						float num3 = ((num < 100f) ? (1f - num / 100f) : 0f);
						float num4 = num3 * MBRandom.RandomFloat + (1f - num3);
						if (MBRandom.RandomFloat < 0.2f)
						{
							num4 = 1f / (0.5f + 0.5f * num4);
						}
						randomFloat *= num4;
						if (randomFloat > 1f)
						{
							randomFloat = 1f;
						}
						DetermineRelation(hero, lord, randomFloat, chanceOfConflict);
					}
					for (int j = i + 1; j < item.Notables.Count; j++)
					{
						Hero hero2 = item.Notables[j];
						float chanceOfConflict2 = (float)HeroHelper.NPCPersonalityClashWithNPC(hero, hero2) * 0.01f * 2.5f;
						float randomValue = MBRandom.RandomFloat;
						if (hero.CharacterObject.Occupation == hero2.CharacterObject.Occupation)
						{
							randomValue = 1f - 0.25f * MBRandom.RandomFloat;
						}
						DetermineRelation(hero, hero2, randomValue, chanceOfConflict2);
					}
				}
			}
		}
	}

	public void OnNewGameCreatedPartialFollowUp(CampaignGameStarter starter, int i)
	{
		if (i != 1)
		{
			return;
		}
		SetInitialRelationsBetweenNotablesAndLords();
		int num = 50;
		for (int j = 0; j < num; j++)
		{
			foreach (Hero allAliveHero in Hero.AllAliveHeroes)
			{
				if (allAliveHero.IsNotable)
				{
					UpdateNotableSupport(allAliveHero);
				}
			}
		}
	}

	private void DailyTickSettlement(Settlement settlement)
	{
		if (_settlementPassedDaysForWeeklyTick.ContainsKey(settlement))
		{
			_settlementPassedDaysForWeeklyTick[settlement]++;
			if (_settlementPassedDaysForWeeklyTick[settlement] == 7)
			{
				SettlementHelper.SpawnNotablesIfNeeded(settlement);
				_settlementPassedDaysForWeeklyTick[settlement] = 0;
			}
		}
		else
		{
			_settlementPassedDaysForWeeklyTick.Add(settlement, 0);
		}
	}

	private void UpdateNotableRelations(Hero notable)
	{
		foreach (Clan item in Clan.All)
		{
			if (item == Clan.PlayerClan || item.Leader == null || item.IsEliminated)
			{
				continue;
			}
			int relation = notable.GetRelation(item.Leader);
			if (relation > 0)
			{
				float num = (float)relation / 1000f;
				if (MBRandom.RandomFloat < num)
				{
					ChangeRelationAction.ApplyRelationChangeBetweenHeroes(notable, item.Leader, -20);
				}
			}
			else if (relation < 0)
			{
				float num2 = (float)(-relation) / 1000f;
				if (MBRandom.RandomFloat < num2)
				{
					ChangeRelationAction.ApplyRelationChangeBetweenHeroes(notable, item.Leader, 20);
				}
			}
		}
	}

	private void UpdateNotableSupport(Hero notable)
	{
		if (notable.SupporterOf == null)
		{
			foreach (Clan nonBanditFaction in Clan.NonBanditFactions)
			{
				if (nonBanditFaction.Leader != null)
				{
					int relation = notable.GetRelation(nonBanditFaction.Leader);
					if (relation > 50)
					{
						float num = (float)(relation - 50) / 2000f;
						if (MBRandom.RandomFloat < num)
						{
							notable.SupporterOf = nonBanditFaction;
						}
					}
				}
			}
			return;
		}
		int relation2 = notable.GetRelation(notable.SupporterOf.Leader);
		if (relation2 < 0)
		{
			notable.SupporterOf = null;
		}
		else if (relation2 < 50)
		{
			float num2 = (float)(50 - relation2) / 500f;
			if (MBRandom.RandomFloat < num2)
			{
				notable.SupporterOf = null;
			}
		}
	}

	private void BalanceGoldAndPowerOfNotable(Hero notable)
	{
		if (notable.Gold > 10500)
		{
			int num = (notable.Gold - 10000) / 500;
			GiveGoldAction.ApplyBetweenCharacters(notable, null, num * 500, disableNotification: true);
			notable.AddPower(num);
		}
		else if (notable.Gold < 4500 && notable.Power > 0f)
		{
			int num2 = (5000 - notable.Gold) / 500;
			GiveGoldAction.ApplyBetweenCharacters(null, notable, num2 * 500, disableNotification: true);
			notable.AddPower(-num2);
		}
	}

	private void DailyTickHero(Hero hero)
	{
		if (hero.IsNotable && hero.CurrentSettlement != null)
		{
			if (MBRandom.RandomFloat < 0.01f)
			{
				UpdateNotableRelations(hero);
			}
			UpdateNotableSupport(hero);
			BalanceGoldAndPowerOfNotable(hero);
			ManageCaravanExpensesOfNotable(hero);
			CheckAndMakeNotableDisappear(hero);
		}
	}

	private void CheckAndMakeNotableDisappear(Hero notable)
	{
		if (notable.OwnedWorkshops.IsEmpty() && notable.OwnedCaravans.IsEmpty() && notable.OwnedAlleys.IsEmpty() && notable.CanDie(KillCharacterAction.KillCharacterActionDetail.Lost) && notable.CanHaveQuestsOrIssues() && notable.Power < (float)Campaign.Current.Models.NotablePowerModel.NotableDisappearPowerLimit)
		{
			float randomFloat = MBRandom.RandomFloat;
			float notableDisappearProbability = GetNotableDisappearProbability(notable);
			if (randomFloat < notableDisappearProbability)
			{
				KillCharacterAction.ApplyByRemove(notable);
				notable.Issue?.CompleteIssueWithAiLord(notable.CurrentSettlement.OwnerClan.Leader);
			}
		}
	}

	private void ManageCaravanExpensesOfNotable(Hero notable)
	{
		for (int num = notable.OwnedCaravans.Count - 1; num >= 0; num--)
		{
			CaravanPartyComponent caravanPartyComponent = notable.OwnedCaravans[num];
			int totalWage = caravanPartyComponent.MobileParty.TotalWage;
			if (caravanPartyComponent.MobileParty.PartyTradeGold >= totalWage)
			{
				caravanPartyComponent.MobileParty.PartyTradeGold -= totalWage;
			}
			else
			{
				int num2 = TaleWorlds.Library.MathF.Min(totalWage, notable.Gold);
				notable.Gold -= num2;
			}
			if (caravanPartyComponent.MobileParty.PartyTradeGold < 5000)
			{
				int num3 = TaleWorlds.Library.MathF.Min(5000 - caravanPartyComponent.MobileParty.PartyTradeGold, notable.Gold);
				caravanPartyComponent.MobileParty.PartyTradeGold += num3;
				notable.Gold -= num3;
			}
		}
	}

	private float GetNotableDisappearProbability(Hero hero)
	{
		return ((float)Campaign.Current.Models.NotablePowerModel.NotableDisappearPowerLimit - hero.Power) / (float)Campaign.Current.Models.NotablePowerModel.NotableDisappearPowerLimit * 0.02f;
	}

	private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
	{
		if (!victim.IsNotable)
		{
			return;
		}
		if (victim.Power >= (float)Campaign.Current.Models.NotablePowerModel.NotableDisappearPowerLimit)
		{
			Hero hero = HeroCreator.CreateRelativeNotableHero(victim);
			if (victim.CurrentSettlement != null)
			{
				ChangeDeadNotable(victim, hero, victim.CurrentSettlement);
			}
			{
				foreach (CaravanPartyComponent item in victim.OwnedCaravans.ToList())
				{
					CaravanPartyComponent.TransferCaravanOwnership(item.MobileParty, hero, hero.CurrentSettlement);
				}
				return;
			}
		}
		foreach (CaravanPartyComponent item2 in victim.OwnedCaravans.ToList())
		{
			DestroyPartyAction.Apply(null, item2.MobileParty);
		}
	}

	private void ChangeDeadNotable(Hero deadNotable, Hero newNotable, Settlement notableSettlement)
	{
		EnterSettlementAction.ApplyForCharacterOnly(newNotable, notableSettlement);
		foreach (Hero allAliveHero in Hero.AllAliveHeroes)
		{
			if (newNotable != allAliveHero)
			{
				int relation = deadNotable.GetRelation(allAliveHero);
				if (Math.Abs(relation) >= 20 || (relation != 0 && allAliveHero.CurrentSettlement == notableSettlement))
				{
					newNotable.SetPersonalRelation(allAliveHero, relation);
				}
			}
		}
		if (deadNotable.Issue != null)
		{
			Campaign.Current.IssueManager.ChangeIssueOwner(deadNotable.Issue, newNotable);
		}
	}

	private void SpawnNotablesAtGameStart()
	{
		foreach (Settlement item in Settlement.All)
		{
			if (item.IsTown)
			{
				int targetNotableCountForSettlement = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(item, Occupation.Artisan);
				for (int i = 0; i < targetNotableCountForSettlement; i++)
				{
					HeroCreator.CreateHeroAtOccupation(Occupation.Artisan, item);
				}
				int targetNotableCountForSettlement2 = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(item, Occupation.Merchant);
				for (int j = 0; j < targetNotableCountForSettlement2; j++)
				{
					HeroCreator.CreateHeroAtOccupation(Occupation.Merchant, item);
				}
				int targetNotableCountForSettlement3 = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(item, Occupation.GangLeader);
				for (int k = 0; k < targetNotableCountForSettlement3; k++)
				{
					HeroCreator.CreateHeroAtOccupation(Occupation.GangLeader, item);
				}
			}
			else if (item.IsVillage)
			{
				int targetNotableCountForSettlement4 = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(item, Occupation.RuralNotable);
				for (int l = 0; l < targetNotableCountForSettlement4; l++)
				{
					HeroCreator.CreateHeroAtOccupation(Occupation.RuralNotable, item);
				}
				int targetNotableCountForSettlement5 = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(item, Occupation.Headman);
				for (int m = 0; m < targetNotableCountForSettlement5; m++)
				{
					HeroCreator.CreateHeroAtOccupation(Occupation.Headman, item);
				}
			}
		}
	}
}
