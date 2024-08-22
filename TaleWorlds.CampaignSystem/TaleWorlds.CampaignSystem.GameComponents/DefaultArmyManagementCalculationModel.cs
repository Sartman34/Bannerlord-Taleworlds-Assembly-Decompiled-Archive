using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultArmyManagementCalculationModel : ArmyManagementCalculationModel
{
	private const float MobilePartySizeRatioToCallToArmy = 0.6f;

	private const float MinimumNeededFoodInDaysToCallToArmy = 5f;

	private static readonly TextObject _numberOfPartiesText = GameTexts.FindText("str_number_of_parties");

	private static readonly TextObject _numberOfStarvingPartiesText = GameTexts.FindText("str_number_of_starving_parties");

	private static readonly TextObject _numberOfLowMoralePartiesText = GameTexts.FindText("str_number_of_low_morale_parties");

	private static readonly TextObject _numberOfLessMemberPartiesText = GameTexts.FindText("str_number_of_less_member_parties");

	private float _minimumPartySizeScoreNeeded = 0.4f;

	public override int InfluenceValuePerGold => 40;

	public override int AverageCallToArmyCost => 20;

	public override int CohesionThresholdForDispersion => 10;

	public override float DailyBeingAtArmyInfluenceAward(MobileParty armyMemberParty)
	{
		float num = (armyMemberParty.Party.TotalStrength + 20f) / 200f;
		if (PartyBaseHelper.HasFeat(armyMemberParty.Party, DefaultCulturalFeats.EmpireArmyInfluenceFeat))
		{
			num += num * DefaultCulturalFeats.EmpireArmyInfluenceFeat.EffectBonus;
		}
		return num;
	}

	public override int CalculatePartyInfluenceCost(MobileParty armyLeaderParty, MobileParty party)
	{
		if (armyLeaderParty.LeaderHero != null && party.LeaderHero != null && armyLeaderParty.LeaderHero.Clan == party.LeaderHero.Clan)
		{
			return 0;
		}
		float num = armyLeaderParty.LeaderHero.GetRelation(party.LeaderHero);
		float partySizeScore = GetPartySizeScore(party);
		float b = MathF.Round(party.Party.TotalStrength);
		float num2 = ((num < 0f) ? (1f + MathF.Sqrt(MathF.Abs(MathF.Max(-100f, num))) / 10f) : (1f - MathF.Sqrt(MathF.Abs(MathF.Min(100f, num))) / 20f));
		float num3 = 0.5f + MathF.Min(1000f, b) / 100f;
		float num4 = 0.5f + 1f * (1f - (partySizeScore - _minimumPartySizeScoreNeeded) / (1f - _minimumPartySizeScoreNeeded));
		float num5 = 1f + 1f * MathF.Pow(MathF.Min(Campaign.MapDiagonal * 10f, MathF.Max(1f, Campaign.Current.Models.MapDistanceModel.GetDistance(armyLeaderParty, party)) / Campaign.MapDiagonal), 0.67f);
		float num6 = ((party.LeaderHero != null) ? party.LeaderHero.RandomFloat(0.75f, 1.25f) : 1f);
		float num7 = 1f;
		float num8 = 1f;
		float num9 = 1f;
		if (armyLeaderParty.LeaderHero?.Clan.Kingdom != null)
		{
			if (armyLeaderParty.LeaderHero.Clan.Tier >= 5 && armyLeaderParty.LeaderHero.Clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.Marshals))
			{
				num7 -= 0.1f;
			}
			if (armyLeaderParty.LeaderHero.Clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.RoyalCommissions))
			{
				num7 = ((armyLeaderParty.LeaderHero != armyLeaderParty.LeaderHero.Clan.Kingdom.Leader) ? (num7 + 0.1f) : (num7 - 0.3f));
			}
			if (party.LeaderHero != null)
			{
				if (armyLeaderParty.LeaderHero.Clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.LordsPrivyCouncil) && party.LeaderHero.Clan.Tier <= 4)
				{
					num7 += 0.2f;
				}
				if (armyLeaderParty.LeaderHero.Clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.Senate) && party.LeaderHero.Clan.Tier <= 2)
				{
					num7 += 0.1f;
				}
			}
			if (armyLeaderParty.LeaderHero.GetPerkValue(DefaultPerks.Leadership.InspiringLeader))
			{
				num8 += DefaultPerks.Leadership.InspiringLeader.PrimaryBonus;
			}
			if (armyLeaderParty.LeaderHero.GetPerkValue(DefaultPerks.Tactics.CallToArms))
			{
				num8 += DefaultPerks.Tactics.CallToArms.SecondaryBonus;
			}
		}
		if (PartyBaseHelper.HasFeat(armyLeaderParty.Party, DefaultCulturalFeats.VlandianArmyInfluenceFeat))
		{
			num9 += DefaultCulturalFeats.VlandianArmyInfluenceFeat.EffectBonus;
		}
		return (int)(0.65f * num2 * num3 * num6 * num5 * num4 * num7 * num8 * num9 * (float)AverageCallToArmyCost);
	}

	public override List<MobileParty> GetMobilePartiesToCallToArmy(MobileParty leaderParty)
	{
		List<MobileParty> list = new List<MobileParty>();
		bool flag = false;
		bool flag2 = false;
		if (leaderParty.LeaderHero != null)
		{
			foreach (Settlement settlement in leaderParty.MapFaction.Settlements)
			{
				if (settlement.IsFortification && settlement.SiegeEvent != null)
				{
					flag = true;
					if (settlement.OwnerClan == leaderParty.LeaderHero.Clan)
					{
						flag2 = true;
					}
				}
			}
		}
		int b = ((leaderParty.MapFaction.IsKingdomFaction && (Kingdom)leaderParty.MapFaction != null) ? ((Kingdom)leaderParty.MapFaction).Armies.Count : 0);
		float num = (0.55f - (float)MathF.Min(2, b) * 0.05f - ((Hero.MainHero.MapFaction == leaderParty.MapFaction) ? 0.05f : 0f)) * (1f - 0.5f * MathF.Sqrt(MathF.Min(leaderParty.LeaderHero.Clan.Influence, 900f)) * (1f / 30f));
		num *= (flag2 ? 1.25f : 1f);
		num *= (flag ? 1.125f : 1f);
		num *= leaderParty.LeaderHero.RandomFloat(0.85f, 1f);
		float num2 = MathF.Min(leaderParty.LeaderHero.Clan.Influence, 900f) * MathF.Min(1f, num);
		List<(MobileParty, float)> list2 = new List<(MobileParty, float)>();
		foreach (WarPartyComponent warPartyComponent in leaderParty.MapFaction.WarPartyComponents)
		{
			MobileParty mobileParty = warPartyComponent.MobileParty;
			Hero leaderHero = mobileParty.LeaderHero;
			if (!mobileParty.IsLordParty || mobileParty.Army != null || mobileParty == leaderParty || leaderHero == null || mobileParty.IsMainParty || leaderHero == leaderHero.MapFaction.Leader || mobileParty.Ai.DoNotMakeNewDecisions || mobileParty.CurrentSettlement?.SiegeEvent != null || mobileParty.IsDisbanding || !(mobileParty.Food > 0f - mobileParty.FoodChange * 5f) || !(mobileParty.PartySizeRatio > 0.6f) || !leaderHero.CanLeadParty() || mobileParty.MapEvent != null || mobileParty.BesiegedSettlement != null)
			{
				continue;
			}
			IDisbandPartyCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<IDisbandPartyCampaignBehavior>();
			if (campaignBehavior != null && campaignBehavior.IsPartyWaitingForDisband(mobileParty))
			{
				continue;
			}
			bool flag3 = false;
			foreach (var item3 in list2)
			{
				if (item3.Item1 == mobileParty)
				{
					flag3 = true;
					break;
				}
			}
			if (!flag3)
			{
				int num3 = Campaign.Current.Models.ArmyManagementCalculationModel.CalculatePartyInfluenceCost(leaderParty, mobileParty);
				float totalStrength = mobileParty.Party.TotalStrength;
				float num4 = 1f - (float)mobileParty.Party.MemberRoster.TotalWounded / (float)mobileParty.Party.MemberRoster.TotalManCount;
				float item = totalStrength / ((float)num3 + 0.1f) * num4;
				list2.Add((mobileParty, item));
			}
		}
		int num6;
		do
		{
			float num5 = 0.01f;
			num6 = -1;
			for (int i = 0; i < list2.Count; i++)
			{
				(MobileParty, float) tuple = list2[i];
				if (tuple.Item2 > num5)
				{
					num6 = i;
					num5 = tuple.Item2;
				}
			}
			if (num6 >= 0)
			{
				MobileParty item2 = list2[num6].Item1;
				int num7 = Campaign.Current.Models.ArmyManagementCalculationModel.CalculatePartyInfluenceCost(leaderParty, item2);
				list2[num6] = (item2, 0f);
				if (num2 > (float)num7)
				{
					num2 -= (float)num7;
					list.Add(item2);
				}
			}
		}
		while (num6 >= 0);
		return list;
	}

	public override int CalculateTotalInfluenceCost(Army army, float percentage)
	{
		int num = 0;
		foreach (MobileParty item in army.Parties.Where((MobileParty p) => !p.IsMainParty))
		{
			num += CalculatePartyInfluenceCost(army.LeaderParty, item);
		}
		ExplainedNumber explainedNumber = new ExplainedNumber(num);
		if (army.LeaderParty.MapFaction.IsKingdomFaction && ((Kingdom)army.LeaderParty.MapFaction).ActivePolicies.Contains(DefaultPolicies.RoyalCommissions))
		{
			explainedNumber.AddFactor(-0.3f);
		}
		if (army.LeaderParty.LeaderHero.GetPerkValue(DefaultPerks.Tactics.Encirclement))
		{
			explainedNumber.AddFactor(DefaultPerks.Tactics.Encirclement.SecondaryBonus);
		}
		return MathF.Ceiling(explainedNumber.ResultNumber * percentage / 100f);
	}

	public override float GetPartySizeScore(MobileParty party)
	{
		return MathF.Min(1f, party.PartySizeRatio);
	}

	public override ExplainedNumber CalculateDailyCohesionChange(Army army, bool includeDescriptions = false)
	{
		ExplainedNumber cohesionChange = new ExplainedNumber(-2f, includeDescriptions);
		CalculateCohesionChangeInternal(army, ref cohesionChange);
		if (army.LeaderParty.HasPerk(DefaultPerks.Tactics.HordeLeader, checkSecondaryRole: true))
		{
			cohesionChange.AddFactor(DefaultPerks.Tactics.HordeLeader.SecondaryBonus, DefaultPerks.Tactics.HordeLeader.Name);
		}
		SiegeEvent siegeEvent = army.LeaderParty.SiegeEvent;
		if (siegeEvent != null && siegeEvent.BesiegerCamp.IsBesiegerSideParty(army.LeaderParty) && army.LeaderParty.HasPerk(DefaultPerks.Engineering.CampBuilding))
		{
			cohesionChange.AddFactor(DefaultPerks.Engineering.CampBuilding.PrimaryBonus, DefaultPerks.Engineering.CampBuilding.Name);
		}
		if (PartyBaseHelper.HasFeat(army.LeaderParty?.Party, DefaultCulturalFeats.SturgianArmyCohesionFeat))
		{
			cohesionChange.AddFactor(DefaultCulturalFeats.SturgianArmyCohesionFeat.EffectBonus, GameTexts.FindText("str_culture"));
		}
		return cohesionChange;
	}

	private void CalculateCohesionChangeInternal(Army army, ref ExplainedNumber cohesionChange)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		foreach (MobileParty attachedParty in army.LeaderParty.AttachedParties)
		{
			if (attachedParty.Party.IsStarving)
			{
				num++;
			}
			if (attachedParty.Morale <= 25f)
			{
				num2++;
			}
			if (attachedParty.Party.NumberOfHealthyMembers <= 10)
			{
				num3++;
			}
			num4++;
		}
		cohesionChange.Add(-num4, _numberOfPartiesText);
		cohesionChange.Add(-((num + 1) / 2), _numberOfStarvingPartiesText);
		cohesionChange.Add(-((num2 + 1) / 2), _numberOfLowMoralePartiesText);
		cohesionChange.Add(-((num3 + 1) / 2), _numberOfLessMemberPartiesText);
	}

	public override int CalculateNewCohesion(Army army, PartyBase newParty, int calculatedCohesion, int sign)
	{
		if (army == null)
		{
			return calculatedCohesion;
		}
		sign = MathF.Sign(sign);
		int num = ((sign == 1) ? (army.Parties.Count - 1) : army.Parties.Count);
		int num2 = (calculatedCohesion * num + 100 * sign) / (num + sign);
		if (num2 <= 100)
		{
			if (num2 >= 0)
			{
				return num2;
			}
			return 0;
		}
		return 100;
	}

	public override int GetCohesionBoostInfluenceCost(Army army, int percentageToBoost = 100)
	{
		return CalculateTotalInfluenceCost(army, percentageToBoost);
	}

	public override int GetCohesionBoostGoldCost(Army army, float percentageToBoost = 100f)
	{
		return CalculateTotalInfluenceCost(army, percentageToBoost) * InfluenceValuePerGold;
	}

	public override int GetPartyRelation(Hero hero)
	{
		if (hero == null)
		{
			return -101;
		}
		if (hero == Hero.MainHero)
		{
			return 101;
		}
		return Hero.MainHero.GetRelation(hero);
	}

	public override int GetPartyStrength(PartyBase party)
	{
		return MathF.Round(party.TotalStrength);
	}

	public override bool CheckPartyEligibility(MobileParty party)
	{
		if (party.Army == null && GetPartySizeScore(party) > _minimumPartySizeScoreNeeded && party.MapEvent == null)
		{
			return party.SiegeEvent == null;
		}
		return false;
	}
}
