using Helpers;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultSettlementGarrisonModel : SettlementGarrisonModel
{
	private static readonly TextObject TownWallsText = new TextObject("{=SlmhqqH8}Town Walls");

	private static readonly TextObject MoraleText = new TextObject("{=UjL7jVYF}Morale");

	private static readonly TextObject FoodShortageText = new TextObject("{=qTFKvGSg}Food Shortage");

	private static readonly TextObject SurplusFoodText = GameTexts.FindText("str_surplus_food");

	private static readonly TextObject RecruitFromCenterNotablesText = GameTexts.FindText("str_center_notables");

	private static readonly TextObject RecruitFromVillageNotablesText = GameTexts.FindText("str_village_notables");

	private static readonly TextObject VillageBeingRaided = GameTexts.FindText("str_village_being_raided");

	private static readonly TextObject VillageLooted = GameTexts.FindText("str_village_looted");

	private static readonly TextObject TownIsUnderSiege = GameTexts.FindText("str_villages_under_siege");

	private static readonly TextObject RetiredText = GameTexts.FindText("str_retired");

	private static readonly TextObject PaymentIsLessText = GameTexts.FindText("str_payment_is_less");

	private static readonly TextObject UnpaidWagesText = GameTexts.FindText("str_unpaid_wages");

	private static readonly TextObject RebellionText = GameTexts.FindText("str_rebel_settlement");

	public override ExplainedNumber CalculateGarrisonChange(Settlement settlement, bool includeDescriptions = false)
	{
		ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions);
		CalculateGarrisonChangeInternal(settlement, ref result);
		return result;
	}

	private static void CalculateGarrisonChangeInternal(Settlement settlement, ref ExplainedNumber result)
	{
		if (settlement.IsTown || settlement.IsCastle)
		{
			if (settlement.Town.GarrisonParty != null && settlement.Town.GarrisonParty.HasUnpaidWages > 0f)
			{
				int num = MathF.Min(settlement.Town.GarrisonParty.Party.NumberOfHealthyMembers, 5);
				result.Add(-num, UnpaidWagesText);
			}
			if (settlement.Town.GarrisonParty != null && ((float)settlement.Town.GarrisonParty.Party.NumberOfHealthyMembers + result.ResultNumber > (float)settlement.Town.GarrisonParty.LimitedPartySize || settlement.Town.GarrisonParty.IsWageLimitExceeded()))
			{
				int num2 = MathF.Max(settlement.Town.GarrisonParty.IsWageLimitExceeded() ? MathF.Min(20, MathF.Max(1, (int)((float)(settlement.Town.GarrisonParty.TotalWage - settlement.Town.GarrisonParty.PaymentLimit) / Campaign.Current.AverageWage / 5f))) : 0, Campaign.Current.Models.PartyDesertionModel.GetNumberOfDeserters(settlement.Town.GarrisonParty));
				result.Add(-num2, PaymentIsLessText);
			}
			if (settlement.OwnerClan.IsRebelClan && (settlement.OwnerClan.MapFaction == null || !settlement.OwnerClan.MapFaction.IsKingdomFaction))
			{
				result.Add(2f, RebellionText);
			}
			if (settlement.IsFortification && settlement.Town.GarrisonAutoRecruitmentIsEnabled)
			{
				GetSettlementGarrisonDueToAutoRecruitment(settlement, ref result);
			}
		}
		GetSettlementGarrisonChangeDueToIssues(settlement, ref result);
	}

	public override ExplainedNumber CalculateGarrisonChangeAutoRecruitment(Settlement settlement, bool includeDescriptions = false)
	{
		ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions);
		GetSettlementGarrisonDueToAutoRecruitment(settlement, ref result);
		return result;
	}

	private static void GetSettlementGarrisonDueToAutoRecruitment(Settlement settlement, ref ExplainedNumber result)
	{
		if (settlement.SiegeEvent == null && settlement.OwnerClan != null && settlement.IsFortification && settlement.Town.FoodChange > 0f && settlement.OwnerClan.Leader.PartyBelongedTo != null && settlement.Town.GarrisonParty != null && settlement.Town.GarrisonParty.CanPayMoreWage() && settlement.Town.GarrisonParty.Party.MemberRoster.TotalManCount < settlement.Town.GarrisonParty.LimitedPartySize && SettlementHelper.IsThereAnyVolunteerCanBeRecruitedForGarrison(settlement))
		{
			result.Add(1f, RecruitFromCenterNotablesText);
		}
	}

	private static void GetSettlementGarrisonChangeDueToIssues(Settlement settlement, ref ExplainedNumber result)
	{
		Campaign.Current.Models.IssueModel.GetIssueEffectsOfSettlement(DefaultIssueEffects.SettlementGarrison, settlement, ref result);
	}

	public override int FindNumberOfTroopsToTakeFromGarrison(MobileParty mobileParty, Settlement settlement, float defaultIdealGarrisonStrengthPerWalledCenter = 0f)
	{
		MobileParty garrisonParty = settlement.Town.GarrisonParty;
		float num = 0f;
		if (garrisonParty != null)
		{
			num = garrisonParty.Party.TotalStrength;
			float num2 = 100f;
			if (garrisonParty.HasLimitedWage())
			{
				num2 = (float)garrisonParty.PaymentLimit / Campaign.Current.AverageWage;
				num2 /= 1.5f;
			}
			else
			{
				num2 = ((defaultIdealGarrisonStrengthPerWalledCenter > 0.1f) ? defaultIdealGarrisonStrengthPerWalledCenter : FactionHelper.FindIdealGarrisonStrengthPerWalledCenter(mobileParty.MapFaction as Kingdom, settlement.OwnerClan));
				float num3 = FactionHelper.OwnerClanEconomyEffectOnGarrisonSizeConstant(settlement.OwnerClan);
				num2 *= num3;
				num2 *= (settlement.IsTown ? 2f : 1f);
			}
			int limitedPartySize = mobileParty.LimitedPartySize;
			int numberOfAllMembers = mobileParty.Party.NumberOfAllMembers;
			float num4 = (float)limitedPartySize / (float)numberOfAllMembers;
			float num5 = MathF.Min(11f, num4 * MathF.Sqrt(num4)) - 1f;
			float num6 = MathF.Pow(num / num2, 1.5f);
			float num7 = ((mobileParty.LeaderHero.Clan.Leader == mobileParty.LeaderHero) ? 2f : 1f);
			int num8 = 0;
			if (num5 * num6 * num7 > 1f)
			{
				num8 = MBRandom.RoundRandomized(num5 * num6 * num7);
			}
			int num9 = 25;
			num9 *= ((!settlement.IsTown) ? 1 : 2);
			if (num8 > garrisonParty.Party.MemberRoster.TotalRegulars - num9)
			{
				num8 = garrisonParty.Party.MemberRoster.TotalRegulars - num9;
			}
			return num8;
		}
		return 0;
	}

	public override int FindNumberOfTroopsToLeaveToGarrison(MobileParty mobileParty, Settlement settlement)
	{
		MobileParty garrisonParty = settlement.Town.GarrisonParty;
		float num = 0f;
		if (garrisonParty != null)
		{
			num = garrisonParty.Party.TotalStrength;
		}
		float num2 = 100f;
		if (garrisonParty != null && garrisonParty.HasLimitedWage())
		{
			num2 = (float)garrisonParty.PaymentLimit / Campaign.Current.AverageWage;
		}
		else
		{
			num2 = FactionHelper.FindIdealGarrisonStrengthPerWalledCenter(mobileParty.MapFaction as Kingdom, settlement.OwnerClan);
			float num3 = FactionHelper.OwnerClanEconomyEffectOnGarrisonSizeConstant(settlement.OwnerClan);
			float num4 = FactionHelper.SettlementProsperityEffectOnGarrisonSizeConstant(settlement.Town);
			float num5 = FactionHelper.SettlementFoodPotentialEffectOnGarrisonSizeConstant(settlement);
			num2 *= num3;
			num2 *= num4;
			num2 *= num5;
		}
		if (num < num2)
		{
			int numberOfRegularMembers = mobileParty.Party.NumberOfRegularMembers;
			float num6 = 1f + (float)mobileParty.Party.MemberRoster.TotalWoundedRegulars / (float)mobileParty.Party.NumberOfRegularMembers;
			int limitedPartySize = mobileParty.LimitedPartySize;
			float num7 = MathF.Pow(MathF.Min(2f, (float)numberOfRegularMembers / (float)limitedPartySize), 1.2f) * 0.75f;
			float num8 = (1f - num / num2) * (1f - num / num2);
			float num9 = 1f;
			if (mobileParty.Army != null)
			{
				num8 = MathF.Min(num8, 0.7f);
				num9 = 0.3f + mobileParty.Army.TotalStrength / mobileParty.Party.TotalStrength * 0.025f;
			}
			float num10 = (settlement.Town.IsOwnerUnassigned ? 0.75f : 0.5f);
			if (settlement.OwnerClan == mobileParty.LeaderHero.Clan || settlement.OwnerClan == mobileParty.Party.Owner.MapFaction.Leader.Clan)
			{
				num10 = 1f;
			}
			float num11 = MathF.Min(0.7f, num7 * num8 * num10 * num6 * num9);
			if ((float)numberOfRegularMembers * num11 > 1f)
			{
				return MBRandom.RoundRandomized((float)numberOfRegularMembers * num11);
			}
		}
		return 0;
	}
}
