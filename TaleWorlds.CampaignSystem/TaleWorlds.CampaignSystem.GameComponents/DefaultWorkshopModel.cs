using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultWorkshopModel : WorkshopModel
{
	public override int WarehouseCapacity => 6000;

	public override int DaysForPlayerSaveWorkshopFromBankruptcy => 3;

	public override int CapitalLowLimit => 5000;

	public override int InitialCapital => 10000;

	public override int DailyExpense => 100;

	public override int DefaultWorkshopCountInSettlement => 4;

	public override int MaximumWorkshopsPlayerCanHave => GetMaxWorkshopCountForClanTier(Campaign.Current.Models.ClanTierModel.MaxClanTier);

	public override ExplainedNumber GetEffectiveConversionSpeedOfProduction(Workshop workshop, float speed, bool includeDescription)
	{
		ExplainedNumber bonuses = new ExplainedNumber(speed, includeDescription, new TextObject("{=basevalue}Base"));
		Settlement settlement = workshop.Settlement;
		if (settlement.OwnerClan.Kingdom != null)
		{
			if (settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.ForgivenessOfDebts))
			{
				bonuses.AddFactor(-0.05f, DefaultPolicies.ForgivenessOfDebts.Name);
			}
			if (settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.StateMonopolies))
			{
				bonuses.AddFactor(-0.1f, DefaultPolicies.StateMonopolies.Name);
			}
		}
		PerkHelper.AddPerkBonusForTown(DefaultPerks.Trade.MercenaryConnections, settlement.Town, ref bonuses);
		PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Steward.Sweatshops, workshop.Owner.CharacterObject, isPrimaryBonus: true, ref bonuses);
		return bonuses;
	}

	public override int GetMaxWorkshopCountForClanTier(int tier)
	{
		return tier + 1;
	}

	public override int GetCostForPlayer(Workshop workshop)
	{
		return workshop.WorkshopType.EquipmentCost + (int)workshop.Settlement.Town.Prosperity * 3 + InitialCapital;
	}

	public override int GetCostForNotable(Workshop workshop)
	{
		return (workshop.WorkshopType.EquipmentCost + (int)workshop.Settlement.Town.Prosperity / 2 + workshop.Capital) / 2;
	}

	public override Hero GetNotableOwnerForWorkshop(Workshop workshop)
	{
		List<(Hero, float)> list = new List<(Hero, float)>();
		foreach (Hero notable in workshop.Settlement.Notables)
		{
			if (notable.IsAlive && notable != workshop.Owner)
			{
				int count = notable.OwnedWorkshops.Count;
				float item = Math.Max(notable.Power, 0f) / TaleWorlds.Library.MathF.Pow(10f, count);
				list.Add((notable, item));
			}
		}
		return MBRandom.ChooseWeighted(list);
	}

	public override int GetConvertProductionCost(WorkshopType workshopType)
	{
		return workshopType.EquipmentCost;
	}

	public override bool CanPlayerSellWorkshop(Workshop workshop, out TextObject explanation)
	{
		Campaign.Current.Models.WorkshopModel.GetCostForNotable(workshop);
		Hero notableOwnerForWorkshop = Campaign.Current.Models.WorkshopModel.GetNotableOwnerForWorkshop(workshop);
		explanation = ((notableOwnerForWorkshop == null) ? new TextObject("{=oqPf2Gdp}There isn't any prospective buyer in the town.") : TextObject.Empty);
		return notableOwnerForWorkshop != null;
	}

	public override float GetTradeXpPerWarehouseProduction(EquipmentElement production)
	{
		return (float)production.GetBaseValue() * 0.1f;
	}
}
