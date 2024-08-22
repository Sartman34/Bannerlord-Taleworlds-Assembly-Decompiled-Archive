using System.Linq;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultSettlementFoodModel : SettlementFoodModel
{
	private static readonly TextObject ProsperityText = GameTexts.FindText("str_prosperity");

	private static readonly TextObject GarrisonText = GameTexts.FindText("str_garrison");

	private static readonly TextObject LandsAroundSettlementText = GameTexts.FindText("str_lands_around_settlement");

	private static readonly TextObject NormalVillagesText = GameTexts.FindText("str_normal_villages");

	private static readonly TextObject RaidedVillagesText = GameTexts.FindText("str_raided_villages");

	private static readonly TextObject VillagesUnderSiegeText = GameTexts.FindText("str_villages_under_siege");

	private static readonly TextObject FoodBoughtByCiviliansText = GameTexts.FindText("str_food_bought_by_civilians");

	private const int FoodProductionPerVillage = 10;

	public override int FoodStocksUpperLimit => 100;

	public override int NumberOfProsperityToEatOneFood => 40;

	public override int NumberOfMenOnGarrisonToEatOneFood => 20;

	public override int CastleFoodStockUpperLimitBonus => 150;

	public override ExplainedNumber CalculateTownFoodStocksChange(Town town, bool includeMarketStocks = true, bool includeDescriptions = false)
	{
		return CalculateTownFoodChangeInternal(town, includeMarketStocks, includeDescriptions);
	}

	private ExplainedNumber CalculateTownFoodChangeInternal(Town town, bool includeMarketStocks, bool includeDescriptions)
	{
		ExplainedNumber explainedNumber = new ExplainedNumber(0f, includeDescriptions);
		float num2;
		if (!town.IsUnderSiege)
		{
			int num = (town.IsTown ? 15 : 10);
			explainedNumber.Add(num, LandsAroundSettlementText);
			num2 = (0f - town.Prosperity) / (float)NumberOfProsperityToEatOneFood;
		}
		else
		{
			num2 = (0f - town.Prosperity) / (float)NumberOfProsperityToEatOneFood;
		}
		int num3 = town.GarrisonParty?.Party.NumberOfAllMembers ?? 0;
		num3 = -num3 / NumberOfMenOnGarrisonToEatOneFood;
		float num4 = 0f;
		float num5 = 0f;
		if (town.Governor != null)
		{
			if (town.IsUnderSiege)
			{
				if (town.Governor.GetPerkValue(DefaultPerks.Steward.Gourmet))
				{
					num5 += DefaultPerks.Steward.Gourmet.SecondaryBonus;
				}
				if (town.Governor.GetPerkValue(DefaultPerks.Medicine.TriageTent))
				{
					num4 += DefaultPerks.Medicine.TriageTent.SecondaryBonus;
				}
			}
			if (town.Governor.GetPerkValue(DefaultPerks.Steward.MasterOfWarcraft))
			{
				num4 += DefaultPerks.Steward.MasterOfWarcraft.SecondaryBonus;
			}
		}
		num2 += num2 * num4;
		num3 += (int)((float)num3 * (num4 + num5));
		explainedNumber.Add(num2, ProsperityText);
		explainedNumber.Add(num3, GarrisonText);
		if (town.Settlement.OwnerClan?.Kingdom != null && town.Settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.HuntingRights))
		{
			explainedNumber.Add(2f, DefaultPolicies.HuntingRights.Name);
		}
		if (!town.IsUnderSiege)
		{
			foreach (Village boundVillage in town.Owner.Settlement.BoundVillages)
			{
				int num6 = 0;
				if (boundVillage.VillageState == Village.VillageStates.Normal)
				{
					num6 = (boundVillage.GetHearthLevel() + 1) * 6;
					explainedNumber.Add(num6, boundVillage.Name);
				}
				else
				{
					num6 = 0;
					explainedNumber.Add(num6, boundVillage.Name);
				}
			}
			float effectOfBuildings = town.GetEffectOfBuildings(BuildingEffectEnum.FoodProduction);
			if (effectOfBuildings > 0f)
			{
				explainedNumber.Add(effectOfBuildings, includeDescriptions ? GameTexts.FindText("str_building_bonus") : TextObject.Empty);
			}
		}
		else if (town.Governor != null && town.Governor.GetPerkValue(DefaultPerks.Roguery.DirtyFighting))
		{
			explainedNumber.Add(DefaultPerks.Roguery.DirtyFighting.SecondaryBonus, DefaultPerks.Roguery.DirtyFighting.Name);
		}
		else
		{
			explainedNumber.Add(0f, VillagesUnderSiegeText);
		}
		if (includeMarketStocks)
		{
			foreach (Town.SellLog soldItem in town.SoldItems)
			{
				if (soldItem.Category.Properties == ItemCategory.Property.BonusToFoodStores)
				{
					explainedNumber.Add(soldItem.Number, includeDescriptions ? soldItem.Category.GetName() : TextObject.Empty);
				}
			}
		}
		GetSettlementFoodChangeDueToIssues(town, ref explainedNumber);
		return explainedNumber;
	}

	private int CalculateFoodPurchasedFromMarket(Town town)
	{
		return town.SoldItems.Sum((Town.SellLog x) => (x.Category.Properties == ItemCategory.Property.BonusToFoodStores) ? x.Number : 0);
	}

	private static void GetSettlementFoodChangeDueToIssues(Town town, ref ExplainedNumber explainedNumber)
	{
		Campaign.Current.Models.IssueModel.GetIssueEffectsOfSettlement(DefaultIssueEffects.SettlementFood, town.Settlement, ref explainedNumber);
	}
}
