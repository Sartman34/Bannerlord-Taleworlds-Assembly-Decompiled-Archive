using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultSettlementProsperityModel : SettlementProsperityModel
{
	private static readonly TextObject LoyaltyText = GameTexts.FindText("str_loyalty");

	private static readonly TextObject FoodShortageText = new TextObject("{=qTFKvGSg}Food Shortage");

	private static readonly TextObject ProsperityFromMarketText = new TextObject("{=RNT5hMVb}Goods From Market");

	private static readonly TextObject SurplusFoodText = GameTexts.FindText("str_surplus_food");

	private static readonly TextObject RaidedText = new TextObject("{=RVas572P}Raided");

	private static readonly TextObject HousingCostsText = new TextObject("{=ByRAgJy4}Housing Costs");

	public override ExplainedNumber CalculateProsperityChange(Town fortification, bool includeDescriptions = false)
	{
		ExplainedNumber explainedNumber = new ExplainedNumber(0f, includeDescriptions);
		CalculateProsperityChangeInternal(fortification, ref explainedNumber);
		return explainedNumber;
	}

	public override ExplainedNumber CalculateHearthChange(Village village, bool includeDescriptions = false)
	{
		ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions);
		CalculateHearthChangeInternal(village, ref result, includeDescriptions);
		return result;
	}

	private void CalculateHearthChangeInternal(Village village, ref ExplainedNumber result, bool includeDescriptions)
	{
		if (village.VillageState == Village.VillageStates.Normal)
		{
			result = new ExplainedNumber((village.Hearth < 300f) ? 0.6f : ((village.Hearth < 600f) ? 0.4f : 0.2f), includeDescriptions);
		}
		if (village.VillageState == Village.VillageStates.Looted)
		{
			result.Add(-1f, RaidedText);
		}
		if (village.Settlement.OwnerClan != null && village.Settlement.OwnerClan.Kingdom != null && village.Settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.GrazingRights))
		{
			result.Add(-0.25f, DefaultPolicies.GrazingRights.Name);
		}
		if (village.Bound != null && village.VillageState == Village.VillageStates.Normal)
		{
			if (village.Bound.Town.CurrentDefaultBuilding != null && village.Bound.Town.BuildingsInProgress.IsEmpty())
			{
				BuildingHelper.AddDefaultDailyBonus(village.Bound.Town, BuildingEffectEnum.VillageDevelopmentDaily, ref result);
			}
			PerkHelper.AddPerkBonusForTown(DefaultPerks.Medicine.BushDoctor, village.Bound.Town, ref result);
			PerkHelper.AddPerkBonusForTown(DefaultPerks.Athletics.Energetic, village.Bound.Town, ref result);
			PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.AidCorps, village.Bound.Town, ref result);
		}
		if (village.Settlement.OwnerClan.Culture.HasFeat(DefaultCulturalFeats.EmpireVillageHearthFeat) && result.ResultNumber >= 0f)
		{
			result.AddFactor(DefaultCulturalFeats.EmpireVillageHearthFeat.EffectBonus, GameTexts.FindText("str_culture"));
		}
		Campaign.Current.Models.IssueModel.GetIssueEffectsOfSettlement(DefaultIssueEffects.VillageHearth, village.Settlement, ref result);
	}

	private void CalculateProsperityChangeInternal(Town fortification, ref ExplainedNumber explainedNumber)
	{
		float foodChange = fortification.FoodChange;
		if (fortification.Owner.IsStarving)
		{
			ExplainedNumber bonuses = new ExplainedNumber((foodChange < 0f) ? ((int)foodChange) : 0);
			PerkHelper.AddPerkBonusForTown(DefaultPerks.Medicine.HelpingHands, fortification, ref bonuses);
			explainedNumber.Add(bonuses.ResultNumber * 0.5f, FoodShortageText);
		}
		if (fortification.IsTown)
		{
			if (fortification.Prosperity < 250f)
			{
				explainedNumber.Add(6f, HousingCostsText);
			}
			else if (fortification.Prosperity < 500f)
			{
				explainedNumber.Add(5f, HousingCostsText);
			}
			else if (fortification.Prosperity < 750f)
			{
				explainedNumber.Add(4f, HousingCostsText);
			}
			else if (fortification.Prosperity < 1000f)
			{
				explainedNumber.Add(3f, HousingCostsText);
			}
			else if (fortification.Prosperity < 1250f)
			{
				explainedNumber.Add(2f, HousingCostsText);
			}
			else if (fortification.Prosperity < 1500f)
			{
				explainedNumber.Add(1f, HousingCostsText);
			}
			if (fortification.Prosperity > 21000f)
			{
				explainedNumber.Add(-6f, HousingCostsText);
			}
			else if (fortification.Prosperity > 18000f)
			{
				explainedNumber.Add(-5f, HousingCostsText);
			}
			else if (fortification.Prosperity > 15000f)
			{
				explainedNumber.Add(-4f, HousingCostsText);
			}
			else if (fortification.Prosperity > 12000f)
			{
				explainedNumber.Add(-3f, HousingCostsText);
			}
			else if (fortification.Prosperity > 9000f)
			{
				explainedNumber.Add(-2f, HousingCostsText);
			}
			else if (fortification.Prosperity > 6000f)
			{
				explainedNumber.Add(-1f, HousingCostsText);
			}
		}
		int num = fortification.FoodStocksUpperLimit();
		int num2 = (int)(fortification.FoodStocks + foodChange) - num;
		if (num2 > 0)
		{
			explainedNumber.Add((float)num2 * 0.1f, SurplusFoodText);
		}
		if (fortification.IsTown)
		{
			int num3 = fortification.SoldItems.Sum((Town.SellLog x) => (x.Category.Properties == ItemCategory.Property.BonusToProsperity) ? x.Number : 0);
			if (num3 > 0)
			{
				explainedNumber.Add((float)num3 * 0.1f, ProsperityFromMarketText);
			}
		}
		PerkHelper.AddPerkBonusForTown(DefaultPerks.Medicine.PristineStreets, fortification, ref explainedNumber);
		if (PerkHelper.GetPerkValueForTown(DefaultPerks.Engineering.Apprenticeship, fortification))
		{
			float num4 = 0f;
			foreach (Building item in fortification.Buildings.Where((Building x) => !x.BuildingType.IsDefaultProject && x.CurrentLevel > 0))
			{
				_ = item;
				num4 += DefaultPerks.Engineering.Apprenticeship.SecondaryBonus;
			}
			if (num4 > 0f && explainedNumber.ResultNumber > 0f)
			{
				explainedNumber.AddFactor(num4, DefaultPerks.Engineering.Apprenticeship.Name);
			}
		}
		if (fortification.BuildingsInProgress.IsEmpty())
		{
			BuildingHelper.AddDefaultDailyBonus(fortification, BuildingEffectEnum.ProsperityDaily, ref explainedNumber);
		}
		foreach (Building building in fortification.Buildings)
		{
			float buildingEffectAmount = building.GetBuildingEffectAmount(BuildingEffectEnum.Prosperity);
			if (!building.BuildingType.IsDefaultProject && buildingEffectAmount > 0f && foodChange > 0f)
			{
				explainedNumber.Add(buildingEffectAmount, building.Name);
			}
			if (building.CurrentLevel > 0 && (building.BuildingType == DefaultBuildingTypes.SettlementAquaducts || building.BuildingType == DefaultBuildingTypes.CastleGranary || building.BuildingType == DefaultBuildingTypes.SettlementGranary))
			{
				PerkHelper.AddPerkBonusForTown(DefaultPerks.Medicine.CleanInfrastructure, fortification, ref explainedNumber);
			}
		}
		if (fortification.Loyalty > (float)Campaign.Current.Models.SettlementLoyaltyModel.ThresholdForProsperityBoost && foodChange > 0f)
		{
			explainedNumber.Add(Campaign.Current.Models.SettlementLoyaltyModel.HighLoyaltyProsperityEffect, LoyaltyText);
		}
		else if (fortification.Loyalty <= (float)Campaign.Current.Models.SettlementLoyaltyModel.ThresholdForProsperityPenalty)
		{
			explainedNumber.Add(Campaign.Current.Models.SettlementLoyaltyModel.LowLoyaltyProsperityEffect, LoyaltyText);
		}
		if (fortification.IsTown && !fortification.CurrentBuilding.IsCurrentlyDefault && fortification.Governor != null && fortification.Governor.GetPerkValue(DefaultPerks.Trade.TrickleDown))
		{
			explainedNumber.Add(DefaultPerks.Trade.TrickleDown.SecondaryBonus, DefaultPerks.Trade.TrickleDown.Name);
		}
		if (fortification.Settlement.OwnerClan.Kingdom != null)
		{
			if (fortification.Settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.RoadTolls))
			{
				explainedNumber.Add(-0.2f, DefaultPolicies.RoadTolls.Name);
			}
			if (fortification.Settlement.OwnerClan.Kingdom.RulingClan == fortification.Settlement.OwnerClan && fortification.Settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.ImperialTowns))
			{
				explainedNumber.Add(1f, DefaultPolicies.ImperialTowns.Name);
			}
			if (fortification.Settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.CrownDuty))
			{
				explainedNumber.Add(-1f, DefaultPolicies.CrownDuty.Name);
			}
			if (fortification.Settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.WarTax))
			{
				explainedNumber.Add(-1f, DefaultPolicies.WarTax.Name);
			}
		}
		GetSettlementProsperityChangeDueToIssues(fortification.Settlement, ref explainedNumber);
	}

	private void GetSettlementProsperityChangeDueToIssues(Settlement settlement, ref ExplainedNumber result)
	{
		Campaign.Current.Models.IssueModel.GetIssueEffectsOfSettlement(DefaultIssueEffects.SettlementProsperity, settlement, ref result);
	}
}
