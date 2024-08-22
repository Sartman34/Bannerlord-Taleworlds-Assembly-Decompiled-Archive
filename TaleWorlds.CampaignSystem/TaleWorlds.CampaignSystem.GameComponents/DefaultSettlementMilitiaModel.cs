using System;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultSettlementMilitiaModel : SettlementMilitiaModel
{
	private static readonly TextObject BaseText = new TextObject("{=militarybase}Base");

	private static readonly TextObject FromHearthsText = new TextObject("{=ecdZglky}From Hearths");

	private static readonly TextObject FromProsperityText = new TextObject("{=cTmiNAlI}From Prosperity");

	private static readonly TextObject RetiredText = new TextObject("{=gHnfFi1s}Retired");

	private static readonly TextObject MilitiaFromMarketText = new TextObject("{=7ve3bQxg}Weapons From Market");

	private static readonly TextObject FoodShortageText = new TextObject("{=qTFKvGSg}Food Shortage");

	private static readonly TextObject LowLoyaltyText = new TextObject("{=SJ2qsRdF}Low Loyalty");

	private static readonly TextObject CultureText = GameTexts.FindText("str_culture");

	private const int AutoSpawnMilitiaDayMultiplierAfterSiege = 20;

	private const int MinimumAutoSpawnedMilitiaAfterSiege = 30;

	public override int MilitiaToSpawnAfterSiege(Town town)
	{
		return Math.Max(30, (int)(town.MilitiaChange * 20f));
	}

	public override ExplainedNumber CalculateMilitiaChange(Settlement settlement, bool includeDescriptions = false)
	{
		return CalculateMilitiaChangeInternal(settlement, includeDescriptions);
	}

	public override float CalculateEliteMilitiaSpawnChance(Settlement settlement)
	{
		float num = 0f;
		Hero hero = null;
		if (settlement.IsFortification && settlement.Town.Governor != null)
		{
			hero = settlement.Town.Governor;
		}
		else if (settlement.IsVillage && settlement.Village.TradeBound?.Town.Governor != null)
		{
			hero = settlement.Village.TradeBound.Town.Governor;
		}
		if (hero != null && hero.GetPerkValue(DefaultPerks.Leadership.CitizenMilitia))
		{
			num += DefaultPerks.Leadership.CitizenMilitia.PrimaryBonus;
		}
		return num;
	}

	public override void CalculateMilitiaSpawnRate(Settlement settlement, out float meleeTroopRate, out float rangedTroopRate)
	{
		meleeTroopRate = 0.5f;
		rangedTroopRate = 1f - meleeTroopRate;
	}

	private static ExplainedNumber CalculateMilitiaChangeInternal(Settlement settlement, bool includeDescriptions = false)
	{
		ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions);
		float militia = settlement.Militia;
		if (settlement.IsFortification)
		{
			result.Add(2f, BaseText);
		}
		float value = (0f - militia) * 0.025f;
		result.Add(value, RetiredText);
		if (settlement.IsVillage)
		{
			float value2 = settlement.Village.Hearth / 400f;
			result.Add(value2, FromHearthsText);
		}
		else if (settlement.IsFortification)
		{
			float num = settlement.Town.Prosperity / 1000f;
			result.Add(num, FromProsperityText);
			if (settlement.Town.InRebelliousState)
			{
				float num2 = MBMath.Map(settlement.Town.Loyalty, 0f, Campaign.Current.Models.SettlementLoyaltyModel.RebelliousStateStartLoyaltyThreshold, Campaign.Current.Models.SettlementLoyaltyModel.MilitiaBoostPercentage, 0f);
				float value3 = TaleWorlds.Library.MathF.Abs(num * (num2 * 0.01f));
				result.Add(value3, LowLoyaltyText);
			}
		}
		if (settlement.IsTown)
		{
			int num3 = settlement.Town.SoldItems.Sum((Town.SellLog x) => (x.Category.Properties == ItemCategory.Property.BonusToMilitia) ? x.Number : 0);
			if (num3 > 0)
			{
				result.Add(0.2f * (float)num3, MilitiaFromMarketText);
			}
			if (settlement.OwnerClan.Kingdom != null)
			{
				if (settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.Serfdom) && settlement.IsTown)
				{
					result.Add(-1f, DefaultPolicies.Serfdom.Name);
				}
				if (settlement.OwnerClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.Cantons))
				{
					result.Add(1f, DefaultPolicies.Cantons.Name);
				}
			}
			if (settlement.OwnerClan.Culture.HasFeat(DefaultCulturalFeats.BattanianMilitiaFeat))
			{
				result.Add(DefaultCulturalFeats.BattanianMilitiaFeat.EffectBonus, CultureText);
			}
		}
		if (settlement.IsCastle || settlement.IsTown)
		{
			if (settlement.Town.BuildingsInProgress.IsEmpty())
			{
				BuildingHelper.AddDefaultDailyBonus(settlement.Town, BuildingEffectEnum.MilitiaDaily, ref result);
			}
			foreach (Building building in settlement.Town.Buildings)
			{
				if (!building.BuildingType.IsDefaultProject)
				{
					float buildingEffectAmount = building.GetBuildingEffectAmount(BuildingEffectEnum.Militia);
					if (buildingEffectAmount > 0f)
					{
						result.Add(buildingEffectAmount, building.Name);
					}
				}
			}
			if (settlement.IsCastle && settlement.Town.InRebelliousState)
			{
				float resultNumber = result.ResultNumber;
				float num4 = 0f;
				foreach (Building building2 in settlement.Town.Buildings)
				{
					if (!(num4 < 1f) || (building2.BuildingType.IsDefaultProject && settlement.Town.CurrentBuilding != building2))
					{
						continue;
					}
					float buildingEffectAmount2 = building2.GetBuildingEffectAmount(BuildingEffectEnum.ReduceMilitia);
					if (buildingEffectAmount2 > 0f)
					{
						float num5 = buildingEffectAmount2 * 0.01f;
						num4 += num5;
						if (num4 > 1f)
						{
							num5 -= num4 - 1f;
						}
						float value4 = resultNumber * (0f - num5);
						result.Add(value4, building2.Name);
					}
				}
			}
			GetSettlementMilitiaChangeDueToPolicies(settlement, ref result);
			GetSettlementMilitiaChangeDueToPerks(settlement, ref result);
			GetSettlementMilitiaChangeDueToIssues(settlement, ref result);
		}
		return result;
	}

	private static void GetSettlementMilitiaChangeDueToPerks(Settlement settlement, ref ExplainedNumber result)
	{
		if (settlement.Town != null && settlement.Town.Governor != null)
		{
			PerkHelper.AddPerkBonusForTown(DefaultPerks.OneHanded.SwiftStrike, settlement.Town, ref result);
			PerkHelper.AddPerkBonusForTown(DefaultPerks.Polearm.KeepAtBay, settlement.Town, ref result);
			PerkHelper.AddPerkBonusForTown(DefaultPerks.Polearm.Drills, settlement.Town, ref result);
			PerkHelper.AddPerkBonusForTown(DefaultPerks.Bow.MerryMen, settlement.Town, ref result);
			PerkHelper.AddPerkBonusForTown(DefaultPerks.Crossbow.LongShots, settlement.Town, ref result);
			PerkHelper.AddPerkBonusForTown(DefaultPerks.Throwing.ThrowingCompetitions, settlement.Town, ref result);
			if (settlement.IsUnderSiege)
			{
				PerkHelper.AddPerkBonusForTown(DefaultPerks.Roguery.ArmsDealer, settlement.Town, ref result);
			}
			PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.SevenVeterans, settlement.Town, ref result);
		}
	}

	private static void GetSettlementMilitiaChangeDueToPolicies(Settlement settlement, ref ExplainedNumber result)
	{
		Kingdom kingdom = settlement.OwnerClan.Kingdom;
		if (kingdom != null && kingdom.ActivePolicies.Contains(DefaultPolicies.Citizenship))
		{
			result.Add(1f, DefaultPolicies.Citizenship.Name);
		}
	}

	private static void GetSettlementMilitiaChangeDueToIssues(Settlement settlement, ref ExplainedNumber result)
	{
		Campaign.Current.Models.IssueModel.GetIssueEffectsOfSettlement(DefaultIssueEffects.SettlementMilitia, settlement, ref result);
	}
}
