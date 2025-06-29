using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class PartiesSellPrisonerCampaignBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
		CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailyTickSettlement);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
	{
		if (!settlement.IsFortification || mobileParty == null || mobileParty.PrisonRoster.Count <= 0 || mobileParty.MapFaction == null || mobileParty.IsMainParty || mobileParty.IsDisbanding || mobileParty.MapFaction.IsAtWarWith(settlement.MapFaction))
		{
			return;
		}
		if (mobileParty.MapFaction.IsKingdomFaction && mobileParty.ActualClan != null)
		{
			TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
			foreach (TroopRosterElement item in mobileParty.PrisonRoster.GetTroopRoster())
			{
				if (!item.Character.IsHero || item.Character.HeroObject.MapFaction.IsAtWarWith(settlement.MapFaction))
				{
					troopRoster.Add(item);
				}
			}
			if (troopRoster.Count > 0)
			{
				SellPrisonersAction.ApplyForSelectedPrisoners(mobileParty.Party, settlement.Party, troopRoster);
			}
		}
		else
		{
			SellPrisonersAction.ApplyForAllPrisoners(mobileParty.Party, settlement.Party);
		}
	}

	private void DailyTickSettlement(Settlement settlement)
	{
		if (!settlement.IsFortification)
		{
			return;
		}
		TroopRoster prisonRoster = settlement.Party.PrisonRoster;
		if (prisonRoster.TotalRegulars <= 0)
		{
			return;
		}
		int num = ((settlement.Owner == Hero.MainHero) ? (prisonRoster.TotalManCount - settlement.Party.PrisonerSizeLimit) : MBRandom.RoundRandomized((float)prisonRoster.TotalRegulars * 0.1f));
		if (num <= 0)
		{
			return;
		}
		TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
		IEnumerable<TroopRosterElement> enumerable;
		if (settlement.Owner != Hero.MainHero)
		{
			enumerable = prisonRoster.GetTroopRoster().AsEnumerable();
		}
		else
		{
			IEnumerable<TroopRosterElement> enumerable2 = from t in prisonRoster.GetTroopRoster()
				orderby t.Character.Tier
				select t;
			enumerable = enumerable2;
		}
		foreach (TroopRosterElement item in enumerable)
		{
			if (!item.Character.IsHero)
			{
				int num2 = Math.Min(num, item.Number);
				num -= num2;
				troopRoster.AddToCounts(item.Character, num2);
				if (num <= 0)
				{
					break;
				}
			}
		}
		if (troopRoster.TotalManCount > 0)
		{
			SellPrisonersAction.ApplyForSelectedPrisoners(settlement.Party, null, troopRoster);
		}
	}
}
