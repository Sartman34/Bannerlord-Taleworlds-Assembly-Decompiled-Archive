using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultDiplomacyModel : DiplomacyModel
{
	private struct WarStats
	{
		public Clan RulingClan;

		public float Strength;

		public float ValueOfSettlements;

		public float TotalStrengthOfEnemies;
	}

	private const int DailyValueFactorForTributes = 70;

	private static float HearthRiskValueFactor = 500f;

	private static float LordRiskValueFactor = 1000f;

	private static float FoodRiskValueFactor = 750f;

	private static float GarrisonRiskValueFactor = 2000f;

	private static float SiegeRiskValueFactor = 3000f;

	private static float LoyalityRiskValueFactor = 500f;

	private static float ProsperityValueFactor = 50f;

	private static float HappenedSiegesDifFactor = 1500f;

	private static float HappenedRaidsDifFactor = 500f;

	private static float StrengthValueFactor = 100f;

	private static TextObject _personalityEffectText = new TextObject("{=HDBryERe}Personalities");

	private const float strengthFactor = 50f;

	private static float _MaxValue = 10000000f;

	private static float _MeaningfulValue = 2000000f;

	private static float _MinValue = 10000f;

	public override int MinimumRelationWithConversationCharacterToJoinKingdom => -10;

	public override int GiftingTownRelationshipBonus => 20;

	public override int GiftingCastleRelationshipBonus => 10;

	public override int MaxRelationLimit => 100;

	public override int MinRelationLimit => -100;

	public override int MaxNeutralRelationLimit => 10;

	public override int MinNeutralRelationLimit => -10;

	public override float GetStrengthThresholdForNonMutualWarsToBeIgnoredToJoinKingdom(Kingdom kingdomToJoin)
	{
		return kingdomToJoin.TotalStrength * 0.05f;
	}

	public override float GetClanStrength(Clan clan)
	{
		float num = 0f;
		foreach (Hero hero in clan.Heroes)
		{
			num += GetHeroCommandingStrengthForClan(hero);
		}
		float num2 = 1.2f;
		float num3 = clan.Influence * num2;
		float num4 = 4f;
		float num5 = (float)clan.Settlements.Count * num4;
		return num + num3 + num5;
	}

	public override float GetHeroCommandingStrengthForClan(Hero hero)
	{
		if (hero.IsAlive)
		{
			float num = 1f;
			float num2 = 1f;
			float num3 = 1f;
			float num4 = 1f;
			float num5 = 0.1f;
			float num6 = 5f;
			float num7 = 0f;
			float num8 = (float)hero.GetSkillValue(DefaultSkills.Tactics) * num;
			float num9 = (float)hero.GetSkillValue(DefaultSkills.Steward) * num2;
			float num10 = (float)hero.GetSkillValue(DefaultSkills.Trade) * num3;
			float num11 = (float)hero.GetSkillValue(DefaultSkills.Leadership) * num4;
			int num12 = ((hero.GetTraitLevel(DefaultTraits.Commander) > 0) ? 300 : 0);
			float num13 = (float)hero.Gold * num5;
			float num14 = ((hero.PartyBelongedTo != null) ? (num6 * hero.PartyBelongedTo.Party.TotalStrength) : 0f);
			float num15 = 0f;
			if (hero.Clan.Leader == hero)
			{
				num15 += 500f;
			}
			float num16 = 0f;
			if (hero.Father == hero.Clan.Leader || hero.Clan.Leader.Father == hero || hero.Mother == hero.Clan.Leader || hero.Clan.Leader.Mother == hero)
			{
				num16 += 100f;
			}
			float num17 = 0f;
			if (hero.IsNoncombatant)
			{
				num17 -= 250f;
			}
			float num18 = 0f;
			if (hero.GovernorOf != null)
			{
				num18 -= 250f;
			}
			num7 = (float)num12 + num8 + num9 + num10 + num11 + num13 + num14 + num15 + num16 + num17 + num18;
			if (!(num7 > 0f))
			{
				return 0f;
			}
			return num7;
		}
		return 0f;
	}

	public override float GetHeroGoverningStrengthForClan(Hero hero)
	{
		if (hero.IsAlive)
		{
			float num = 0.3f;
			float num2 = 0.9f;
			float num3 = 0.8f;
			float num4 = 1.2f;
			float num5 = 1f;
			float num6 = 0.005f;
			float num7 = 2f;
			float num8 = (float)hero.GetSkillValue(DefaultSkills.Tactics) * num;
			float num9 = (float)hero.GetSkillValue(DefaultSkills.Charm) * num2;
			float num10 = (float)hero.GetSkillValue(DefaultSkills.Engineering) * num3;
			float num11 = (float)hero.GetSkillValue(DefaultSkills.Steward) * num7;
			float num12 = (float)hero.GetSkillValue(DefaultSkills.Trade) * num4;
			float num13 = (float)hero.GetSkillValue(DefaultSkills.Leadership) * num5;
			int num14 = ((hero.GetTraitLevel(DefaultTraits.Honor) > 0) ? 100 : 0);
			float num15 = (float)MathF.Min(100000, hero.Gold) * num6;
			float num16 = 0f;
			if (hero.Spouse == hero.Clan.Leader)
			{
				num16 += 1000f;
			}
			if (hero.Father == hero.Clan.Leader || hero.Clan.Leader.Father == hero || hero.Mother == hero.Clan.Leader || hero.Clan.Leader.Mother == hero)
			{
				num16 += 750f;
			}
			if (hero.Siblings.Contains(hero.Clan.Leader))
			{
				num16 += 500f;
			}
			return (float)num14 + num8 + num11 + num12 + num13 + num15 + num16 + num9 + num10;
		}
		return 0f;
	}

	public override float GetRelationIncreaseFactor(Hero hero1, Hero hero2, float relationChange)
	{
		ExplainedNumber stat = new ExplainedNumber(relationChange);
		Hero hero3 = ((!hero1.IsHumanPlayerCharacter && !hero2.IsHumanPlayerCharacter) ? ((MBRandom.RandomFloat < 0.5f) ? hero1 : hero2) : (hero1.IsHumanPlayerCharacter ? hero1 : hero2));
		SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Charm, DefaultSkillEffects.CharmRelationBonus, hero3.CharacterObject, ref stat);
		if (hero1.IsFemale != hero2.IsFemale)
		{
			if (hero3.GetPerkValue(DefaultPerks.Charm.InBloom))
			{
				stat.AddFactor(DefaultPerks.Charm.InBloom.PrimaryBonus);
			}
		}
		else if (hero3.GetPerkValue(DefaultPerks.Charm.YoungAndRespectful))
		{
			stat.AddFactor(DefaultPerks.Charm.YoungAndRespectful.PrimaryBonus);
		}
		if (hero3.GetPerkValue(DefaultPerks.Charm.GoodNatured) && hero2.GetTraitLevel(DefaultTraits.Mercy) > 0)
		{
			stat.Add(DefaultPerks.Charm.GoodNatured.SecondaryBonus, DefaultPerks.Charm.GoodNatured.Name);
		}
		if (hero3.GetPerkValue(DefaultPerks.Charm.Tribute) && hero2.GetTraitLevel(DefaultTraits.Mercy) < 0)
		{
			stat.Add(DefaultPerks.Charm.Tribute.SecondaryBonus, DefaultPerks.Charm.Tribute.Name);
		}
		return stat.ResultNumber;
	}

	public override int GetInfluenceAwardForSettlementCapturer(Settlement settlement)
	{
		if (settlement.IsTown || settlement.IsCastle)
		{
			int num = (settlement.IsTown ? 30 : 10);
			int num2 = 0;
			foreach (Village boundVillage in settlement.BoundVillages)
			{
				num2 += GetInfluenceAwardForSettlementCapturer(boundVillage.Settlement);
			}
			return num + num2;
		}
		return 10;
	}

	public override float GetHourlyInfluenceAwardForBeingArmyMember(MobileParty mobileParty)
	{
		float totalStrength = mobileParty.Party.TotalStrength;
		float num = 0.0001f * (20f + totalStrength);
		if (mobileParty.BesiegedSettlement != null || mobileParty.MapEvent != null)
		{
			num *= 2f;
		}
		return num;
	}

	public override float GetHourlyInfluenceAwardForRaidingEnemyVillage(MobileParty mobileParty)
	{
		int num = 0;
		foreach (MapEventParty party in mobileParty.MapEvent.AttackerSide.Parties)
		{
			if (party.Party.MobileParty == mobileParty || (party.Party.MobileParty?.Army != null && party.Party.MobileParty.Army.LeaderParty == mobileParty))
			{
				num += party.Party.MemberRoster.TotalManCount;
			}
		}
		return (MathF.Sqrt(num) + 2f) / 240f;
	}

	public override float GetHourlyInfluenceAwardForBesiegingEnemyFortification(MobileParty mobileParty)
	{
		int num = 0;
		foreach (PartyBase item in mobileParty.BesiegedSettlement.SiegeEvent.GetSiegeEventSide(BattleSideEnum.Attacker).GetInvolvedPartiesForEventType())
		{
			if (item.MobileParty == mobileParty || (item.MobileParty.Army != null && item.MobileParty.Army.LeaderParty == mobileParty))
			{
				num += item.MemberRoster.TotalManCount;
			}
		}
		return (MathF.Sqrt(num) + 2f) / 240f;
	}

	public override float GetScoreOfClanToJoinKingdom(Clan clan, Kingdom kingdom)
	{
		if (clan.Kingdom != null && clan.Kingdom.RulingClan == clan)
		{
			return -100000000f;
		}
		int relationBetweenClans = FactionManager.GetRelationBetweenClans(kingdom.RulingClan, clan);
		int num = 0;
		int num2 = 0;
		foreach (Clan clan2 in kingdom.Clans)
		{
			int relationBetweenClans2 = FactionManager.GetRelationBetweenClans(clan, clan2);
			num += relationBetweenClans2;
			num2++;
		}
		float num3 = ((num2 > 0) ? ((float)num / (float)num2) : 0f);
		float num4 = MathF.Max(-100f, MathF.Min(100f, (float)relationBetweenClans + num3));
		float num5 = MathF.Min(2f, MathF.Max(0.33f, 1f + MathF.Sqrt(MathF.Abs(num4)) * ((num4 < 0f) ? (-0.067f) : 0.1f)));
		float num6 = 1f + ((kingdom.Culture == clan.Culture) ? 0.15f : ((kingdom.Leader == Hero.MainHero) ? 0f : (-0.15f)));
		float num7 = clan.CalculateTotalSettlementBaseValue();
		float num8 = clan.CalculateTotalSettlementValueForFaction(kingdom);
		int commanderLimit = clan.CommanderLimit;
		float num9 = 0f;
		float num10 = 0f;
		if (!clan.IsMinorFaction)
		{
			float num11 = 0f;
			foreach (Town fief in kingdom.Fiefs)
			{
				num11 += fief.Settlement.GetSettlementValueForFaction(kingdom);
			}
			int num12 = 0;
			foreach (Clan clan3 in kingdom.Clans)
			{
				if (!clan3.IsUnderMercenaryService && clan3 != clan)
				{
					num12 += clan3.CommanderLimit;
				}
			}
			num9 = num11 / (float)(num12 + commanderLimit);
			num10 = 0f - (float)(num12 * num12) * 100f + 10000f;
		}
		_ = clan.Leader.Gold;
		_ = 0.5f * MathF.Min(1000000f, (clan.Kingdom != null) ? ((float)clan.Kingdom.KingdomBudgetWallet) : 0f) / ((clan.Kingdom != null) ? ((float)clan.Kingdom.Clans.Count + 1f) : 2f);
		float num13 = 0.15f;
		float num14 = num9 * MathF.Sqrt(commanderLimit) * num13 * 0.2f;
		num14 *= num5 * num6;
		num14 += (clan.MapFaction.IsAtWarWith(kingdom) ? (num8 - num7) : 0f);
		num14 += num10;
		if (clan.Kingdom != null && clan.Kingdom.Leader == Hero.MainHero && num14 > 0f)
		{
			num14 *= 0.2f;
		}
		return num14;
	}

	public override float GetScoreOfClanToLeaveKingdom(Clan clan, Kingdom kingdom)
	{
		int relationBetweenClans = FactionManager.GetRelationBetweenClans(kingdom.RulingClan, clan);
		int num = 0;
		int num2 = 0;
		foreach (Clan clan2 in kingdom.Clans)
		{
			int relationBetweenClans2 = FactionManager.GetRelationBetweenClans(clan, clan2);
			num += relationBetweenClans2;
			num2++;
		}
		float num3 = ((num2 > 0) ? ((float)num / (float)num2) : 0f);
		float num4 = MathF.Max(-100f, MathF.Min(100f, (float)relationBetweenClans + num3));
		float num5 = MathF.Min(2f, MathF.Max(0.33f, 1f + MathF.Sqrt(MathF.Abs(num4)) * ((num4 < 0f) ? (-0.067f) : 0.1f)));
		float num6 = 1f + ((kingdom.Culture == clan.Culture) ? 0.15f : ((kingdom.Leader == Hero.MainHero) ? 0f : (-0.15f)));
		float num7 = clan.CalculateTotalSettlementBaseValue();
		float num8 = clan.CalculateTotalSettlementValueForFaction(kingdom);
		int commanderLimit = clan.CommanderLimit;
		float num9 = 0f;
		if (!clan.IsMinorFaction)
		{
			float num10 = 0f;
			foreach (Town fief in kingdom.Fiefs)
			{
				num10 += fief.Settlement.GetSettlementValueForFaction(kingdom);
			}
			int num11 = 0;
			foreach (Clan clan3 in kingdom.Clans)
			{
				if (!clan3.IsUnderMercenaryService && clan3 != clan)
				{
					num11 += clan3.CommanderLimit;
				}
			}
			num9 = num10 / (float)(num11 + commanderLimit);
		}
		float num12 = HeroHelper.CalculateReliabilityConstant(clan.Leader);
		float b = (float)(CampaignTime.Now - clan.LastFactionChangeTime).ToDays;
		float num13 = 4000f * (15f - MathF.Sqrt(MathF.Min(225f, b)));
		int num14 = 0;
		int num15 = 0;
		foreach (Town fief2 in clan.Fiefs)
		{
			if (fief2.IsCastle)
			{
				num15++;
			}
			else
			{
				num14++;
			}
		}
		float num16 = -70000f - (float)num15 * 10000f - (float)num14 * 30000f;
		_ = clan.Leader.Gold;
		_ = 0.5f * (float)MathF.Min(1000000, (clan.Kingdom != null) ? clan.Kingdom.KingdomBudgetWallet : 0) / ((float)clan.Kingdom.Clans.Count + 1f);
		float num17 = 0.15f;
		num16 /= num17;
		float num18 = (0f - num9) * MathF.Sqrt(commanderLimit) * num17 * 0.2f + num16 * num12 + (0f - num13);
		num18 *= num5 * num6;
		num18 = ((!(num5 < 1f) || !(num7 - num8 < 0f)) ? (num18 + (num7 - num8)) : (num18 + num5 * (num7 - num8)));
		if (num5 < 1f)
		{
			num18 += (1f - num5) * 200000f;
		}
		if (kingdom.Leader == Hero.MainHero)
		{
			num18 = ((!(num18 > 0f)) ? (num18 * 5f) : (num18 * 0.2f));
		}
		return num18 + ((kingdom.Leader == Hero.MainHero) ? (0f - 1000000f * num5) : 0f);
	}

	public override float GetScoreOfKingdomToGetClan(Kingdom kingdom, Clan clan)
	{
		float num = MathF.Min(2f, MathF.Max(0.33f, 1f + 0.02f * (float)FactionManager.GetRelationBetweenClans(kingdom.RulingClan, clan)));
		float num2 = 1f + ((kingdom.Culture == clan.Culture) ? 1f : 0f);
		int commanderLimit = clan.CommanderLimit;
		float num3 = (clan.TotalStrength + 150f * (float)commanderLimit) * 20f;
		float powerRatioToEnemies = FactionHelper.GetPowerRatioToEnemies(kingdom);
		float num4 = HeroHelper.CalculateReliabilityConstant(clan.Leader);
		float num5 = 1f / MathF.Max(0.4f, MathF.Min(2.5f, MathF.Sqrt(powerRatioToEnemies)));
		num3 *= num5;
		return (clan.CalculateTotalSettlementValueForFaction(kingdom) * 0.1f + num3) * num * num2 * num4;
	}

	public override float GetScoreOfKingdomToSackClan(Kingdom kingdom, Clan clan)
	{
		float num = MathF.Min(2f, MathF.Max(0.33f, 1f + 0.02f * (float)FactionManager.GetRelationBetweenClans(kingdom.RulingClan, clan)));
		float num2 = 1f + ((kingdom.Culture == clan.Culture) ? 1f : 0.5f);
		int commanderLimit = clan.CommanderLimit;
		float num3 = (clan.TotalStrength + 150f * (float)commanderLimit) * 20f;
		float num4 = clan.CalculateTotalSettlementValueForFaction(kingdom);
		return 10f - 1f * num3 * num2 * num - num4;
	}

	public override float GetScoreOfMercenaryToJoinKingdom(Clan mercenaryClan, Kingdom kingdom)
	{
		int num = ((mercenaryClan.Kingdom == kingdom) ? mercenaryClan.MercenaryAwardMultiplier : Campaign.Current.Models.MinorFactionsModel.GetMercenaryAwardFactorToJoinKingdom(mercenaryClan, kingdom));
		float num2 = mercenaryClan.TotalStrength + (float)mercenaryClan.CommanderLimit * 50f;
		int mercenaryAwardFactorToJoinKingdom = Campaign.Current.Models.MinorFactionsModel.GetMercenaryAwardFactorToJoinKingdom(mercenaryClan, kingdom, neededAmountForClanToJoinCalculation: true);
		if (kingdom.Leader == Hero.MainHero)
		{
			return 0f;
		}
		return (float)(num - mercenaryAwardFactorToJoinKingdom) * num2 * 0.5f;
	}

	public override float GetScoreOfMercenaryToLeaveKingdom(Clan mercenaryClan, Kingdom kingdom)
	{
		float num = 0.005f * MathF.Min(200f, mercenaryClan.LastFactionChangeTime.ElapsedDaysUntilNow);
		return 10000f * num - 5000f - GetScoreOfMercenaryToJoinKingdom(mercenaryClan, kingdom);
	}

	public override float GetScoreOfKingdomToHireMercenary(Kingdom kingdom, Clan mercenaryClan)
	{
		int num = 0;
		foreach (Clan clan in kingdom.Clans)
		{
			num += clan.CommanderLimit;
		}
		int num2 = ((num < 12) ? ((12 - num) * 100) : 0);
		int count = kingdom.Settlements.Count;
		int num3 = ((count < 40) ? ((40 - count) * 30) : 0);
		return num2 + num3;
	}

	public override float GetScoreOfKingdomToSackMercenary(Kingdom kingdom, Clan mercenaryClan)
	{
		float b = (((float)kingdom.Leader.Gold > 20000f) ? (MathF.Sqrt((float)kingdom.Leader.Gold / 20000f) - 1f) : (-1f));
		int relationBetweenClans = FactionManager.GetRelationBetweenClans(kingdom.RulingClan, mercenaryClan);
		float num = MathF.Min(5f, FactionHelper.GetPowerRatioToEnemies(kingdom));
		return (MathF.Min(2f + (float)relationBetweenClans / 100f - num, b) * -1f - 0.1f) * 50f * mercenaryClan.TotalStrength * 5f;
	}

	public override float GetScoreOfDeclaringPeace(IFaction factionDeclaresPeace, IFaction factionDeclaredPeace, IFaction evaluatingClan, out TextObject peaceReason)
	{
		float num = 0f - GetScoreOfWarInternal(factionDeclaresPeace, factionDeclaredPeace, evaluatingClan, evaluatingPeace: true, out peaceReason);
		float num2 = 1f;
		if (num > 0f)
		{
			float num3 = ((factionDeclaredPeace.Leader == Hero.MainHero) ? 0.12f : ((Hero.MainHero.MapFaction == factionDeclaredPeace) ? 0.24f : 0.36f));
			num2 *= num3 + (0.84f - num3) * (100f - factionDeclaredPeace.Aggressiveness) * 0.01f;
		}
		int num4 = ((factionDeclaredPeace.Leader == Hero.MainHero || factionDeclaresPeace.Leader == Hero.MainHero) ? (MathF.Min(Hero.MainHero.Level + 1, 31) * 20) : 0);
		int num5 = -(int)MathF.Min(180000f, (MathF.Min(10000f, factionDeclaresPeace.TotalStrength) + 2000f + (float)num4) * (MathF.Min(10000f, factionDeclaredPeace.TotalStrength) + 2000f + (float)num4) * 0.00018f);
		return (int)(num2 * num) + num5;
	}

	private float GetWarFatiqueScoreNew(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		foreach (Town fief in factionDeclaresWar.Fiefs)
		{
			int num8 = 1;
			if (fief.OwnerClan == evaluatingClan || (evaluatingClan.IsKingdomFaction && fief.OwnerClan.Leader == evaluatingClan.Leader))
			{
				num8 = 3;
			}
			int num9 = ((!fief.Settlement.IsTown) ? 1 : 2);
			num += ((fief.Loyalty < 50f) ? ((50f - fief.Loyalty) * MathF.Min(6000f, fief.Prosperity) * (float)num8 * (float)num9 * 0.00166f) : 0f);
			num2 += (float)num9 * ((fief.FoodStocks < 100f) ? ((100f - fief.FoodStocks) * (float)num8) : 0f);
			num6 += num8 * num9;
			if (fief.GarrisonParty == null)
			{
				num3 += 100f * (float)num8 * (float)num9;
			}
			else
			{
				float num10 = fief.GarrisonParty.Party.TotalStrength + (fief.Settlement.MilitiaPartyComponent?.MobileParty.Party.TotalStrength ?? 0f);
				num3 += ((num10 < (float)(200 * num9)) ? (0.25f * ((float)(200 * num9) - num10) * (float)num8 * (float)num9) : 0f);
			}
			if (fief.IsUnderSiege && fief.Settlement.SiegeEvent != null && fief.Settlement.SiegeEvent.BesiegerCamp.LeaderParty.MapFaction == factionDeclaredWar && (MobileParty.MainParty.SiegeEvent == null || MobileParty.MainParty.SiegeEvent.BesiegedSettlement != fief.Settlement))
			{
				num7 += 100 * num8 * num9;
			}
			foreach (Village village in fief.Villages)
			{
				float num11 = MathF.Max(0f, 400f - village.Hearth) * 0.2f;
				num11 += (float)((village.VillageState == Village.VillageStates.Looted) ? 20 : 0);
				num4 += num11 * (float)num8;
				num5 += num8;
			}
		}
		float num12 = 0f;
		float num13 = 0f;
		int num14 = 0;
		if (factionDeclaresWar.IsKingdomFaction)
		{
			foreach (Clan clan in ((Kingdom)factionDeclaresWar).Clans)
			{
				int num15 = 1;
				if (clan == evaluatingClan || (evaluatingClan.IsKingdomFaction && clan.Leader == evaluatingClan.Leader))
				{
					num15 = 3;
				}
				int partyLimitForTier = Campaign.Current.Models.ClanTierModel.GetPartyLimitForTier(clan, clan.Tier);
				if (partyLimitForTier > clan.WarPartyComponents.Count)
				{
					num12 += 100f * (float)(partyLimitForTier - clan.WarPartyComponents.Count * num15);
				}
				foreach (WarPartyComponent warPartyComponent in clan.WarPartyComponents)
				{
					if (warPartyComponent.MobileParty.PartySizeRatio < 0.9f)
					{
						num12 += 100f * (0.9f - warPartyComponent.MobileParty.PartySizeRatio) * (float)num15;
					}
					if (warPartyComponent.Party.TotalStrength > (float)warPartyComponent.Party.PartySizeLimit)
					{
						num13 += (warPartyComponent.Party.TotalStrength - (float)warPartyComponent.Party.PartySizeLimit) * (float)num15;
					}
				}
				num14 += partyLimitForTier * num15;
			}
		}
		int num16 = 0;
		int num17 = 0;
		int num18 = 0;
		int num19 = 0;
		foreach (StanceLink stance in factionDeclaresWar.Stances)
		{
			if (stance.Faction1 == factionDeclaresWar && stance.Faction2 == factionDeclaredWar)
			{
				num16 = stance.SuccessfulSieges2;
				num17 = stance.SuccessfulRaids2;
				num18 = stance.SuccessfulSieges1;
				num19 = stance.SuccessfulRaids1;
			}
			else if (stance.Faction1 == factionDeclaredWar && stance.Faction2 == factionDeclaresWar)
			{
				num16 = stance.SuccessfulSieges1;
				num17 = stance.SuccessfulRaids1;
				num18 = stance.SuccessfulSieges2;
				num19 = stance.SuccessfulRaids2;
			}
		}
		int b = ((!evaluatingClan.IsKingdomFaction) ? evaluatingClan.Leader.GetTraitLevel(DefaultTraits.Calculating) : 0);
		float num20 = 1f + 0.2f * (float)MathF.Min(2, MathF.Max(-2, b));
		int count = factionDeclaresWar.Fiefs.Count;
		int num21 = factionDeclaresWar.Fiefs.Count * 3;
		float num22 = MathF.Max(0f, (float)num16 - (float)num18 * 0.5f) / ((float)count + 5f) * HappenedSiegesDifFactor * 100f * num20;
		float num23 = MathF.Max(0f, (float)num17 - (float)num19 * 0.5f) / ((float)num21 + 5f) * HappenedRaidsDifFactor * 100f * num20;
		float num24 = num12 / (float)(num14 + 2) * LordRiskValueFactor;
		float num25 = num13 / (float)(num14 + 2) * 0.5f * LordRiskValueFactor;
		float num26 = num / (float)(num6 + 2) * LoyalityRiskValueFactor;
		float num27 = num4 / (float)(num5 + 2) * HearthRiskValueFactor;
		float num28 = num2 / (float)(num6 + 2) * FoodRiskValueFactor;
		float num29 = num3 / (float)(num6 + 2) * GarrisonRiskValueFactor;
		float num30 = (float)(num7 / (num6 + 2)) * SiegeRiskValueFactor;
		return MathF.Min(300000f, num30 + num24 - num25 + num26 + num27 + num28 + num29 + num22 + num23);
	}

	private WarStats CalculateWarStats(IFaction faction, IFaction targetFaction)
	{
		float num = faction.TotalStrength * 0.85f;
		float num2 = 0f;
		int num3 = 0;
		foreach (Town fief in faction.Fiefs)
		{
			num3 += (fief.IsCastle ? 1 : 2);
		}
		if (faction.IsKingdomFaction)
		{
			foreach (Clan clan in ((Kingdom)faction).Clans)
			{
				if (!clan.IsUnderMercenaryService)
				{
					int partyLimitForTier = Campaign.Current.Models.ClanTierModel.GetPartyLimitForTier(clan, clan.Tier);
					num2 += (float)partyLimitForTier * 80f * ((clan.Leader == clan.MapFaction.Leader) ? 1.25f : 1f);
				}
			}
		}
		num += num2;
		Clan rulingClan = (faction.IsClan ? (faction as Clan) : (faction as Kingdom).RulingClan);
		float valueOfSettlements = faction.Fiefs.Sum((Town f) => (float)(f.IsTown ? 2000 : 1000) + f.Prosperity * 0.33f) * ProsperityValueFactor;
		float num4 = 0f;
		float num5 = 0f;
		foreach (StanceLink stance in faction.Stances)
		{
			if (!stance.IsAtWar || stance.Faction1 == targetFaction || stance.Faction2 == targetFaction || (stance.Faction2.IsMinorFaction && stance.Faction2.Leader != Hero.MainHero))
			{
				continue;
			}
			IFaction faction2 = ((stance.Faction1 == faction) ? stance.Faction2 : stance.Faction1);
			if (faction2.IsKingdomFaction)
			{
				foreach (Clan clan2 in ((Kingdom)faction2).Clans)
				{
					if (!clan2.IsUnderMercenaryService)
					{
						num4 += (float)clan2.Tier * 80f * ((clan2.Leader == clan2.MapFaction.Leader) ? 1.5f : 1f);
					}
				}
			}
			num5 += faction2.TotalStrength;
		}
		num5 += num4;
		num *= MathF.Sqrt(MathF.Sqrt(MathF.Min(num3 + 4, 40))) / 2.5f;
		WarStats result = default(WarStats);
		result.RulingClan = rulingClan;
		result.Strength = num;
		result.ValueOfSettlements = valueOfSettlements;
		result.TotalStrengthOfEnemies = num5;
		return result;
	}

	private (Kingdom kingdom1, float kingdom1Score, Kingdom kingdom2, float kingdom2Score) GetTopDogs()
	{
		(Kingdom, Kingdom, Kingdom) tuple = MBMath.MaxElements3(Kingdom.All, (Kingdom k) => k.TotalStrength);
		Kingdom item = tuple.Item1;
		Kingdom item2 = tuple.Item2;
		Kingdom item3 = tuple.Item3;
		float num = item?.TotalStrength ?? 400f;
		float num2 = item2?.TotalStrength ?? 300f;
		float num3 = item3?.TotalStrength ?? item2?.TotalStrength ?? 200f;
		if (num3 <= 3000f)
		{
			num3 = 3000f;
		}
		float item4 = num / num3;
		float item5 = num2 / num3;
		return (kingdom1: item, kingdom1Score: item4, kingdom2: item2, kingdom2Score: item5);
	}

	private float GetTopDogScore(IFaction factionDeclaresWar, IFaction factionDeclaredWar)
	{
		float result = 0f;
		var (kingdom, num, kingdom2, num2) = GetTopDogs();
		if (kingdom == factionDeclaresWar)
		{
			return 0f;
		}
		if (factionDeclaredWar == kingdom)
		{
			result = StrengthValueFactor * 2f * (factionDeclaresWar.TotalStrength + 1f) * (0.3f * (num - 0.9f));
		}
		else if (factionDeclaredWar.IsAtWarWith(kingdom))
		{
			result = (0f - StrengthValueFactor) * 2f * (factionDeclaresWar.TotalStrength + 1f) * (0.2f * (num - 0.9f));
		}
		if (factionDeclaredWar == kingdom2)
		{
			result = StrengthValueFactor * 2f * (factionDeclaresWar.TotalStrength + 1f) * (0.3f * (num2 - 0.9f));
		}
		else if (factionDeclaredWar.IsAtWarWith(kingdom))
		{
			result = (0f - StrengthValueFactor) * 2f * (factionDeclaresWar.TotalStrength + 1f) * (0.2f * (num2 - 0.9f));
		}
		return result;
	}

	private float GetBottomScore(IFaction factionDeclaresWar, IFaction factionDeclaredWar)
	{
		float result = 0f;
		var (kingdom, num, _, _) = GetTopDogs();
		if (factionDeclaredWar == kingdom)
		{
			result = StrengthValueFactor * factionDeclaresWar.TotalStrength * (0.2f * num);
		}
		return result;
	}

	private float CalculateClanRiskScoreOfWar(float squareRootOfPowerRatio, IFaction factionDeclaredWar, IFaction evaluatingClan)
	{
		float num = 0f;
		if (squareRootOfPowerRatio > 0.5f)
		{
			foreach (Town fief in evaluatingClan.Fiefs)
			{
				float num2 = Campaign.MapDiagonal * 2f;
				float num3 = Campaign.MapDiagonal * 2f;
				foreach (Town fief2 in factionDeclaredWar.Fiefs)
				{
					if (fief2.IsTown)
					{
						float length = (fief.Settlement.GetPosition2D - fief2.Settlement.GetPosition2D).Length;
						if (length < num2)
						{
							num3 = num2;
							num2 = length;
						}
						else if (length < num3)
						{
							num3 = length;
						}
					}
				}
				float num4 = (num2 + num3) / 2f;
				if (num4 < Campaign.AverageDistanceBetweenTwoFortifications * 3f)
				{
					float num5 = MathF.Min(Campaign.AverageDistanceBetweenTwoFortifications * 3f - num4, Campaign.AverageDistanceBetweenTwoFortifications * 2f) / (Campaign.AverageDistanceBetweenTwoFortifications * 2f);
					float num6 = MathF.Min(7.5f, (squareRootOfPowerRatio - 0.5f) * 5f);
					num6 += 0.5f;
					int num7 = ((!fief.IsTown) ? 1 : 2);
					num += num5 * num6 * (float)num7 * (2000f + MathF.Min(8000f, fief.Prosperity));
				}
			}
		}
		return num;
	}

	private float GetScoreOfWarInternal(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan, bool evaluatingPeace, out TextObject reason)
	{
		reason = TextObject.Empty;
		if (factionDeclaresWar.MapFaction == factionDeclaredWar.MapFaction)
		{
			return 0f;
		}
		WarStats faction1Stats = CalculateWarStats(factionDeclaresWar, factionDeclaredWar);
		WarStats faction2Stats = CalculateWarStats(factionDeclaredWar, factionDeclaresWar);
		float distance = GetDistance(factionDeclaresWar, factionDeclaredWar);
		float num = (483f + 8.63f * Campaign.AverageDistanceBetweenTwoFortifications) / 2f;
		float num2 = ((factionDeclaresWar.Leader == Hero.MainHero || factionDeclaredWar.Leader == Hero.MainHero) ? (-300000f) : (-400000f));
		float num3;
		if (distance - Campaign.AverageDistanceBetweenTwoFortifications * 1.5f > num)
		{
			num3 = num2;
		}
		else if (distance - Campaign.AverageDistanceBetweenTwoFortifications * 1.5f < 0f)
		{
			num3 = 0f;
		}
		else
		{
			float num4 = num - Campaign.AverageDistanceBetweenTwoFortifications * 1.5f;
			float num5 = (0f - num2) / MathF.Pow(num4, 1.6f);
			float num6 = distance - Campaign.AverageDistanceBetweenTwoFortifications * 1.5f;
			num3 = num2 + num5 * MathF.Pow(MathF.Pow(num4 - num6, 8f), 0.2f);
			if (num3 > 0f)
			{
				num3 = 0f;
			}
		}
		float num7 = 1f - MathF.Pow(num3 / num2, 0.55f);
		num7 = 0.1f + num7 * 0.9f;
		float num8 = (evaluatingClan.IsKingdomFaction ? 0f : evaluatingClan.Leader.RandomFloat(-20000f, 20000f));
		int valorLevelOfEvaluatingClan = MathF.Max(-2, MathF.Min(2, evaluatingClan.Leader.GetTraitLevel(DefaultTraits.Valor)));
		float num9 = CalculateBenefitScore(ref faction1Stats, ref faction2Stats, valorLevelOfEvaluatingClan, evaluatingPeace, distance);
		float num10 = CalculateBenefitScore(ref faction2Stats, ref faction1Stats, valorLevelOfEvaluatingClan, evaluatingPeace, distance, calculatingRisk: true);
		float num11 = 0f;
		float squareRootOfPowerRatio = MathF.Min(2f, MathF.Sqrt((faction2Stats.Strength + 1000f) / (faction1Stats.Strength + 1000f)));
		if (evaluatingClan.IsKingdomFaction)
		{
			int num12 = 0;
			foreach (Clan clan in ((Kingdom)evaluatingClan).Clans)
			{
				num11 += CalculateClanRiskScoreOfWar(squareRootOfPowerRatio, factionDeclaredWar, clan);
				num12++;
			}
			if (num12 > 0)
			{
				num11 /= (float)num12;
			}
		}
		else
		{
			num11 = CalculateClanRiskScoreOfWar(squareRootOfPowerRatio, factionDeclaredWar, evaluatingClan);
		}
		num11 = MathF.Min(200000f, num11);
		float warFatiqueScoreNew = GetWarFatiqueScoreNew(factionDeclaresWar, factionDeclaredWar, evaluatingClan);
		float topDogScore = GetTopDogScore(factionDeclaresWar, factionDeclaredWar);
		int relationWithClan = faction1Stats.RulingClan.GetRelationWithClan(faction2Stats.RulingClan);
		int relationWithClan2 = evaluatingClan.Leader.Clan.GetRelationWithClan(faction2Stats.RulingClan);
		float num13 = (float)(relationWithClan + relationWithClan2) / 2f;
		num9 *= 0.7f + 0.3f * (100f - num13) * 0.01f;
		float num14 = ((!(num13 < 0f)) ? 0f : ((factionDeclaresWar.TotalStrength > factionDeclaredWar.TotalStrength * 2f) ? (-500f * num13) : (-500f * (factionDeclaresWar.TotalStrength / (2f * factionDeclaredWar.TotalStrength)) * (factionDeclaresWar.TotalStrength / (2f * factionDeclaredWar.TotalStrength)) * num13)));
		num14 *= ((factionDeclaredWar.Leader == Hero.MainHero) ? 1.5f : 1f);
		float num15 = 1f + 0.002f * factionDeclaredWar.Aggressiveness * ((factionDeclaredWar.Leader == Hero.MainHero) ? 1.5f : 1f);
		num9 *= num15;
		if (factionDeclaredWar.Leader == Hero.MainHero && evaluatingPeace)
		{
			num10 /= num15;
		}
		float num16 = 0.3f * MathF.Min(100000f, factionDeclaredWar.Settlements.Sum((Settlement s) => (s.Culture != factionDeclaresWar.Culture || !s.IsFortification) ? 0f : (s.Town.Prosperity * 0.5f * ProsperityValueFactor)));
		int num17 = 0;
		foreach (Town fief in factionDeclaresWar.Fiefs)
		{
			num17 += ((!fief.IsTown) ? 1 : 2);
		}
		if (num17 > 0)
		{
			_ = 16;
		}
		float num18 = 0.1f + 0.9f * MathF.Min(MathF.Min(num9, num10), 100000f) / 100000f;
		float num19 = num9 - num10;
		if (!evaluatingClan.IsKingdomFaction && evaluatingClan.Leader != evaluatingClan.MapFaction.Leader)
		{
			if (num19 > 0f && evaluatingClan.Leader.GetTraitLevel(DefaultTraits.Mercy) != 0)
			{
				num19 *= 1f - 0.1f * (float)MathF.Min(2, MathF.Max(-2, evaluatingClan.Leader.GetTraitLevel(DefaultTraits.Mercy)));
			}
			if (num19 < 0f && evaluatingClan.Leader.GetTraitLevel(DefaultTraits.Valor) != 0)
			{
				num19 *= 1f - 0.1f * (float)MathF.Min(2, MathF.Max(-2, evaluatingClan.Leader.GetTraitLevel(DefaultTraits.Valor)));
			}
		}
		float num20 = 0f;
		if (!evaluatingClan.IsKingdomFaction && faction1Stats.Strength > faction2Stats.Strength && evaluatingClan.Leader.GetTraitLevel(DefaultTraits.Mercy) > 0)
		{
			num20 -= MathF.Min((faction1Stats.Strength + 500f) / (faction2Stats.Strength + 500f) - 1f, 2f) * 5000f * (float)evaluatingClan.Leader.GetTraitLevel(DefaultTraits.Mercy);
		}
		if (!evaluatingClan.IsKingdomFaction && faction1Stats.Strength < faction2Stats.Strength && evaluatingClan.Leader.GetTraitLevel(DefaultTraits.Valor) > 0)
		{
			num20 += MathF.Min((faction2Stats.Strength + 500f) / (faction1Stats.Strength + 500f) - 1f, 2f) * 5000f * (float)evaluatingClan.Leader.GetTraitLevel(DefaultTraits.Valor);
		}
		float num21 = 0f;
		float num22 = 0f;
		StanceLink stanceWith = factionDeclaresWar.GetStanceWith(factionDeclaredWar);
		int num23 = 0;
		int num24 = 0;
		if (stanceWith.IsAtWar)
		{
			float elapsedDaysUntilNow = stanceWith.WarStartDate.ElapsedDaysUntilNow;
			int num25 = 60;
			float num26 = 5f;
			num21 = ((elapsedDaysUntilNow > (float)num25) ? ((elapsedDaysUntilNow - (float)num25) * -400f) : (((float)num25 - elapsedDaysUntilNow) * num26 * (400f + 0.2f * MathF.Min(6000f, MathF.Min(faction1Stats.Strength, faction2Stats.Strength)))));
			if (num21 < 0f && !evaluatingClan.IsKingdomFaction)
			{
				int traitLevel = evaluatingClan.Leader.GetTraitLevel(DefaultTraits.Valor);
				if (traitLevel < 0)
				{
					num21 *= 1f - (float)MathF.Max(traitLevel, -2) * 0.25f;
				}
				else if (traitLevel > 0)
				{
					num21 *= 1f - (float)MathF.Min(traitLevel, 2) * 0.175f;
				}
			}
			foreach (Hero hero in factionDeclaresWar.Heroes)
			{
				int num27 = ((hero.Clan != evaluatingClan) ? 1 : 3);
				float num28 = ((hero.Clan.Leader == hero) ? 1.5f : 1f);
				double num29 = ((hero == hero.MapFaction.Leader) ? 1.5 : 1.0);
				if (hero.IsPrisoner && hero.IsLord && hero.PartyBelongedToAsPrisoner != null && hero.PartyBelongedToAsPrisoner.MapFaction == factionDeclaredWar)
				{
					num23 += (int)(num29 * (double)num28 * (double)num27 * 3000.0);
				}
			}
			num24 = num23;
			foreach (Hero hero2 in factionDeclaredWar.Heroes)
			{
				double num30 = ((hero2 == hero2.MapFaction.Leader) ? 1.5 : 1.0);
				float num31 = ((hero2.Clan.Leader == hero2) ? 1.5f : 1f);
				if (hero2.IsPrisoner && hero2.IsLord && hero2.PartyBelongedToAsPrisoner != null && hero2.PartyBelongedToAsPrisoner.MapFaction == factionDeclaresWar)
				{
					num23 -= (int)(num30 * (double)num31 * 2500.0);
				}
			}
		}
		else
		{
			float elapsedDaysUntilNow2 = stanceWith.PeaceDeclarationDate.ElapsedDaysUntilNow;
			num22 = ((elapsedDaysUntilNow2 > 60f) ? 0f : ((60f - elapsedDaysUntilNow2) * (0f - (400f + 0.2f * MathF.Min(6000f, MathF.Min(faction1Stats.Strength, faction2Stats.Strength))))));
			if (num22 < 0f && !evaluatingClan.IsKingdomFaction)
			{
				int traitLevel2 = evaluatingClan.Leader.GetTraitLevel(DefaultTraits.Honor);
				if (traitLevel2 > 0)
				{
					num22 *= 1f + (float)MathF.Min(traitLevel2, 2) * 0.25f;
				}
				else if (traitLevel2 < 0)
				{
					num22 *= 1f + (float)MathF.Max(traitLevel2, -2) * 0.175f;
				}
			}
		}
		int num32 = (factionDeclaresWar.IsKingdomFaction ? (((Kingdom)factionDeclaresWar).PoliticalStagnation * 1000) : 0);
		float num33 = num16 + num8 * num18 + num3 * num18 + num19 + num21 + num22 + (float)num32 * num18 - num11 * num18 + num14 - (float)num23 + num20 * num18;
		float num34 = StrengthValueFactor * 0.5f * Kingdom.All.Sum((Kingdom k) => (!k.IsAtWarWith(factionDeclaresWar) || !k.IsAtWarWith(factionDeclaredWar) || k.IsMinorFaction) ? 0f : MathF.Min(k.TotalStrength, factionDeclaredWar.TotalStrength));
		float num35 = topDogScore - num34 - warFatiqueScoreNew;
		num35 *= num18;
		if (evaluatingPeace)
		{
			num33 += (float)((factionDeclaredWar.Leader == Hero.MainHero) ? 10000 : 0);
			if (num33 > 0f)
			{
				num33 += num35;
				if (num33 < 0f)
				{
					num33 *= 0.5f;
				}
			}
			else
			{
				num33 += num35 * 0.75f;
			}
		}
		else
		{
			num33 += num35;
		}
		if (evaluatingPeace)
		{
			float num36 = warFatiqueScoreNew;
			float num37 = 0f - num19;
			float num38 = (0f - num21) * 2f;
			int num39 = num23 * 3 + num24;
			float num40 = (0f - num11) * 3f;
			float num41 = num33 * 0.5f;
			float num42 = (0f - num3) * 0.5f;
			if (num41 > num40 && num41 > (float)num23 && num41 > warFatiqueScoreNew && num41 > num37 && num41 > num38 && num41 > num42)
			{
				reason = new TextObject("{=3JGFdaT7}The {ENEMY_KINGDOM_INFORMAL_NAME} are willing to pay considerable tribute.");
			}
			else if (num40 > (float)num39 && num40 > warFatiqueScoreNew && num40 > num37 && num40 > num38 && num40 > num42)
			{
				reason = new TextObject("{=eH0roDGM}Our clan's lands are vulnerable. I owe it to those under my protection to seek peace.");
			}
			else if ((float)num39 > num36 && (float)num39 > num37 && (float)num39 > num38 && (float)num23 > num42)
			{
				reason = new TextObject("{=TQmPcVRZ}Too many of our nobles are in captivity. We should make peace to free them.");
			}
			else if (num36 >= num37 && num36 >= num38 && warFatiqueScoreNew > num42)
			{
				reason = new TextObject("{=QQtJobYP}We need time to recover from the hardships of war.");
			}
			else if (num38 >= num37 && num38 > num42)
			{
				reason = new TextObject("{=lV0VOn99}This war has gone on too long.");
			}
			else if (num37 > num42)
			{
				if (faction1Stats.TotalStrengthOfEnemies > 0f && faction1Stats.Strength < faction1Stats.TotalStrengthOfEnemies + faction2Stats.Strength)
				{
					reason = new TextObject("{=nuqv4GAA}We have too many enemies. We need to make peace with at least some of them.");
				}
				else if (faction1Stats.Strength < faction2Stats.Strength)
				{
					reason = new TextObject("{=JOe3BC41}The {ENEMY_KINGDOM_INFORMAL_NAME} is currently more powerful than us. We need time to build up our strength.");
				}
				else if (faction1Stats.ValueOfSettlements > faction2Stats.ValueOfSettlements)
				{
					reason = new TextObject("{=HqJSNG3M}Our realm is currently doing well, but we stand to lose this wealth if we go on fighting.");
				}
				else
				{
					reason = new TextObject("{=vwjs6EjJ}On balance, the gains we stand to make are not worth the costs and risks. ");
				}
			}
			else
			{
				reason = new TextObject("{=i0h0LKa0}Our borders are far from those of the enemy. It is too arduous to pursue this war.");
			}
			if (!TextObject.IsNullOrEmpty(reason))
			{
				reason.SetTextVariable("ENEMY_KINGDOM_INFORMAL_NAME", factionDeclaredWar.InformalName);
			}
		}
		else
		{
			float num43 = num19;
			int num44 = ((relationWithClan2 < 0) ? (-relationWithClan2 * 1000) : 0);
			int num45 = ((stanceWith.Faction1 != evaluatingClan.MapFaction) ? ((stanceWith.TotalTributePaidby1 != 0) ? stanceWith.TotalTributePaidby2 : (-stanceWith.TotalTributePaidby1)) : ((stanceWith.TotalTributePaidby1 != 0) ? stanceWith.TotalTributePaidby1 : (-stanceWith.TotalTributePaidby2)));
			int num46 = num45 * 70;
			int num47 = num32;
			float num48 = topDogScore;
			float num49 = (100f - factionDeclaredWar.Aggressiveness) * 1000f;
			float num50 = 0f;
			if (factionDeclaredWar.Culture != factionDeclaresWar.Culture)
			{
				int num51 = 0;
				foreach (Town fief2 in factionDeclaredWar.Fiefs)
				{
					if (fief2.Culture == factionDeclaresWar.Culture)
					{
						num51++;
					}
				}
				num50 = MathF.Pow(num51, 0.7f) * 30000f;
			}
			if ((float)num46 > num43 && num46 > num44 && num46 > num47 && (float)num46 > num50 && (float)num46 > num48 && (float)num46 > num49)
			{
				if (num45 > 1000)
				{
					reason = new TextObject("{=Kt8tBtBG}We are paying too much tribute to the {ENEMY_KINGDOM_INFORMAL_NAME}.");
				}
				else
				{
					reason = new TextObject("{=qI4cicQz}It is a disgrace to keep paying tribute to the  {ENEMY_KINGDOM_INFORMAL_NAME}.");
				}
			}
			else if ((float)num44 > num43 && num44 > num47 && (float)num44 > num50 && (float)num44 > num48 && (float)num44 > num49)
			{
				reason = new TextObject("{=dov3iRlt}{ENEMY_RULER.NAME} of the {ENEMY_KINGDOM_INFORMAL_NAME} is vile and dangerous. We must deal with {?ENEMY_RULER.GENDER}her{?}him{\\?} before it is too late.");
			}
			else if (num43 > (float)num47 && num43 > num50 && num43 > num48)
			{
				if (faction1Stats.TotalStrengthOfEnemies == 0f && faction1Stats.Strength < faction2Stats.Strength)
				{
					reason = new TextObject("{=1aQAmENB}The  {ENEMY_KINGDOM_INFORMAL_NAME} may be strong but their lands are rich and ripe for the taking.");
				}
				else if (faction1Stats.Strength > faction2Stats.Strength)
				{
					reason = new TextObject("{=az3K3j4C}Right now we are stronger than the {ENEMY_KINGDOM_INFORMAL_NAME}. We should strike while we can.");
				}
			}
			else if ((float)num47 > num50 && (float)num47 > num48 && (float)num47 > num49)
			{
				reason = new TextObject("{=pmg9KCqf}We have been at peace too long. Our men grow restless.");
			}
			else if (num50 > num48 && num50 > num49)
			{
				reason = new TextObject("{=79lEPn1u}The {ENEMY_KINGDOM_INFORMAL_NAME} have occupied our ancestral lands and they oppress our kinfolk.");
			}
			else if (num49 > num48)
			{
				reason = new TextObject("{=bHf8aMtt}The {ENEMY_KINGDOM_INFORMAL_NAME} have been acting aggressively. We should teach them a lesson.");
			}
			else
			{
				reason = new TextObject("{=gsmmoKNd}The {ENEMY_KINGDOM_INFORMAL_NAME} will devour all of Calradia if we do not stop them.");
			}
			if (!TextObject.IsNullOrEmpty(reason))
			{
				reason.SetTextVariable("ENEMY_KINGDOM_INFORMAL_NAME", factionDeclaredWar.InformalName);
				reason.SetCharacterProperties("ENEMY_RULER", factionDeclaredWar.Leader.CharacterObject);
			}
		}
		return num7 * num33;
	}

	private float CalculateBenefitScore(ref WarStats faction1Stats, ref WarStats faction2Stats, int valorLevelOfEvaluatingClan, bool evaluatingPeace, float distanceToClosestEnemyFief, bool calculatingRisk = false)
	{
		float valueOfSettlements = faction2Stats.ValueOfSettlements;
		float num = MathF.Clamp((valueOfSettlements > _MeaningfulValue) ? ((valueOfSettlements - _MeaningfulValue) * 0.5f + _MinValue + _MeaningfulValue) : (valueOfSettlements + _MinValue), _MinValue, _MaxValue);
		float num2 = 100f;
		float num3 = (faction2Stats.Strength + num2) / (faction1Stats.Strength + num2);
		float num4 = 0f;
		float num5 = ((num3 > 1f) ? num3 : (1f / num3));
		if (num5 > 3f)
		{
			num4 = MathF.Min(0.4f, (num5 / 3f - 1f) * 0.1f);
		}
		float num6 = MathF.Pow(num3, 1.1f + num4);
		if (!calculatingRisk)
		{
			float x = MathF.Min(1f, (MathF.Min(MathF.Max(faction2Stats.Strength, 10000f), faction1Stats.Strength) + num2) / (faction2Stats.Strength + faction1Stats.TotalStrengthOfEnemies + num2));
			num6 /= MathF.Pow(x, (0.5f - (float)valorLevelOfEvaluatingClan * 0.1f) * (evaluatingPeace ? 1.1f : 1f));
		}
		else
		{
			float x2 = MathF.Min(1f, (MathF.Min(MathF.Max(faction1Stats.Strength, 10000f), faction2Stats.Strength) + num2) / (faction1Stats.Strength + faction2Stats.TotalStrengthOfEnemies + num2));
			num6 *= MathF.Pow(x2, (0.4f - (float)valorLevelOfEvaluatingClan * 0.1f) * (evaluatingPeace ? 1.1f : 1f));
		}
		float value = 1f / (1f + num6);
		value = MathF.Clamp(value, 0.01f, 0.99f);
		float num7 = num * value;
		float b = Campaign.AverageDistanceBetweenTwoFortifications * 3f / (Campaign.AverageDistanceBetweenTwoFortifications * 3f + distanceToClosestEnemyFief + distanceToClosestEnemyFief * 0.25f);
		return num7 * MathF.Max(0.25f, b);
	}

	private (Settlement, float)[] GetClosestSettlementsToOtherFactionsNearestSettlementToMidPoint(IFaction faction1, IFaction faction2)
	{
		Settlement toSettlement = null;
		float num = float.MaxValue;
		foreach (Town fief in faction1.Fiefs)
		{
			float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(fief.Settlement, faction1.FactionMidSettlement);
			if (num > distance)
			{
				toSettlement = fief.Settlement;
				num = distance;
			}
		}
		(Settlement, float)[] array = new(Settlement, float)[3]
		{
			(null, float.MaxValue),
			(null, float.MaxValue),
			(null, float.MaxValue)
		};
		foreach (Town fief2 in faction2.Fiefs)
		{
			float distance2 = Campaign.Current.Models.MapDistanceModel.GetDistance(fief2.Settlement, toSettlement);
			if (!(distance2 < array[2].Item2))
			{
				continue;
			}
			if (distance2 < array[1].Item2)
			{
				if (distance2 < array[0].Item2)
				{
					array[2] = array[1];
					array[1] = array[0];
					array[0].Item1 = fief2.Settlement;
					array[0].Item2 = distance2;
				}
				else
				{
					array[2] = array[1];
					array[1].Item1 = fief2.Settlement;
					array[1].Item2 = distance2;
				}
			}
			else
			{
				array[2].Item1 = fief2.Settlement;
				array[2].Item2 = distance2;
			}
		}
		return array;
	}

	private float GetDistance(IFaction factionDeclaresWar, IFaction factionDeclaredWar)
	{
		if (factionDeclaresWar.Fiefs.Count == 0 || factionDeclaredWar.Fiefs.Count == 0)
		{
			if (factionDeclaresWar.Leader == Hero.MainHero || factionDeclaredWar.Leader == Hero.MainHero)
			{
				return 100f;
			}
			return 0.4f * (factionDeclaresWar.InitialPosition - factionDeclaredWar.InitialPosition).Length;
		}
		(Settlement, float)[] closestSettlementsToOtherFactionsNearestSettlementToMidPoint = GetClosestSettlementsToOtherFactionsNearestSettlementToMidPoint(factionDeclaredWar, factionDeclaresWar);
		(Settlement, float)[] closestSettlementsToOtherFactionsNearestSettlementToMidPoint2 = GetClosestSettlementsToOtherFactionsNearestSettlementToMidPoint(factionDeclaresWar, factionDeclaredWar);
		float[] array = new float[3]
		{
			float.MaxValue,
			float.MaxValue,
			float.MaxValue
		};
		(Settlement, float)[] array2 = closestSettlementsToOtherFactionsNearestSettlementToMidPoint;
		for (int i = 0; i < array2.Length; i++)
		{
			(Settlement, float) tuple = array2[i];
			if (tuple.Item1 == null)
			{
				continue;
			}
			(Settlement, float)[] array3 = closestSettlementsToOtherFactionsNearestSettlementToMidPoint2;
			for (int j = 0; j < array3.Length; j++)
			{
				(Settlement, float) tuple2 = array3[j];
				if (tuple2.Item1 == null)
				{
					continue;
				}
				float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(tuple.Item1, tuple2.Item1);
				if (!(distance < array[2]))
				{
					continue;
				}
				if (distance < array[1])
				{
					if (distance < array[0])
					{
						array[2] = array[1];
						array[1] = array[0];
						array[0] = distance;
					}
					else
					{
						array[2] = array[1];
						array[1] = distance;
					}
				}
				else
				{
					array[2] = distance;
				}
			}
		}
		float num = array[0];
		float num2 = ((array[1] < float.MaxValue) ? array[1] : num) * 0.67f;
		float num3 = ((array[2] < float.MaxValue) ? array[2] : ((num2 < float.MaxValue) ? num2 : num)) * 0.33f;
		return (num + num2 + num3) / 2f;
	}

	public override float GetScoreOfDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan, out TextObject warReason)
	{
		float scoreOfWarInternal = GetScoreOfWarInternal(factionDeclaresWar, factionDeclaredWar, evaluatingClan, evaluatingPeace: false, out warReason);
		StanceLink stanceWith = factionDeclaresWar.GetStanceWith(factionDeclaredWar);
		int num = 0;
		if (stanceWith.IsNeutral)
		{
			int dailyTributePaid = stanceWith.GetDailyTributePaid(factionDeclaredWar);
			float num2 = (float)evaluatingClan.Leader.Gold + (evaluatingClan.MapFaction.IsKingdomFaction ? (0.5f * ((float)((Kingdom)evaluatingClan.MapFaction).KingdomBudgetWallet / ((float)((Kingdom)evaluatingClan.MapFaction).Clans.Count + 1f))) : 0f);
			float num3 = ((evaluatingClan.IsKingdomFaction || evaluatingClan.Leader == null) ? 1f : ((num2 < 50000f) ? (1f + 0.5f * ((50000f - num2) / 50000f)) : ((num2 > 200000f) ? MathF.Max(0.5f, MathF.Sqrt(200000f / num2)) : 1f)));
			num = GetValueOfDailyTribute(dailyTributePaid);
			num = (int)((float)num * num3);
		}
		int num4 = -(int)MathF.Min(120000f, (MathF.Min(10000f, factionDeclaresWar.TotalStrength) * 0.8f + 2000f) * (MathF.Min(10000f, factionDeclaredWar.TotalStrength) * 0.8f + 2000f) * 0.0012f);
		return scoreOfWarInternal + (float)num4 - (float)num;
	}

	private static int GetWarFatiqueScore(IFaction factionDeclaresWar, IFaction factionDeclaredWar)
	{
		int num = 0;
		StanceLink stanceWith = factionDeclaresWar.GetStanceWith(factionDeclaredWar);
		float num2 = (float)(CampaignTime.Now - stanceWith.WarStartDate).ToDays;
		float num3 = ((factionDeclaresWar.IsMinorFaction && factionDeclaresWar != MobileParty.MainParty.MapFaction) ? 40f : 60f);
		if (num2 < num3)
		{
			int num4 = (((factionDeclaredWar != MobileParty.MainParty.MapFaction || factionDeclaresWar.IsMinorFaction) && (factionDeclaresWar != MobileParty.MainParty.MapFaction || factionDeclaredWar.IsMinorFaction)) ? 1 : 2);
			float num5 = ((factionDeclaresWar.IsMinorFaction && factionDeclaresWar != MobileParty.MainParty.MapFaction) ? 1000f : 2000f);
			num = (int)(MathF.Pow(num3 - num2, 1.3f) * num5 * (float)num4);
		}
		return num + 60000;
	}

	public override float GetScoreOfLettingPartyGo(MobileParty party, MobileParty partyToLetGo)
	{
		float num = 0f;
		BattleSideEnum battleSideEnum = BattleSideEnum.Attacker;
		if (battleSideEnum == BattleSideEnum.Attacker)
		{
			num = 0.98f;
		}
		float num2 = 0f;
		for (int i = 0; i < partyToLetGo.ItemRoster.Count; i++)
		{
			ItemRosterElement elementCopyAtIndex = partyToLetGo.ItemRoster.GetElementCopyAtIndex(i);
			num2 += (float)(elementCopyAtIndex.Amount * elementCopyAtIndex.EquipmentElement.GetBaseValue());
		}
		float num3 = 0f;
		for (int j = 0; j < party.ItemRoster.Count; j++)
		{
			ItemRosterElement elementCopyAtIndex2 = party.ItemRoster.GetElementCopyAtIndex(j);
			num3 += (float)(elementCopyAtIndex2.Amount * elementCopyAtIndex2.EquipmentElement.GetBaseValue());
		}
		float num4 = 0f;
		foreach (TroopRosterElement item in party.MemberRoster.GetTroopRoster())
		{
			num4 += MathF.Min(1000f, 10f * (float)item.Character.Level * MathF.Sqrt(item.Character.Level));
		}
		float num5 = 0f;
		foreach (TroopRosterElement item2 in partyToLetGo.MemberRoster.GetTroopRoster())
		{
			num5 += MathF.Min(1000f, 10f * (float)item2.Character.Level * MathF.Sqrt(item2.Character.Level));
		}
		float num6 = 0f;
		foreach (TroopRosterElement item3 in ((battleSideEnum == BattleSideEnum.Attacker) ? partyToLetGo : party).MemberRoster.GetTroopRoster())
		{
			if (item3.Character.IsHero)
			{
				num6 += 500f;
			}
			num6 += (float)Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(item3.Character, (battleSideEnum == BattleSideEnum.Attacker) ? partyToLetGo.LeaderHero : party.LeaderHero) * 0.3f;
		}
		float num7 = (party.IsPartyTradeActive ? ((float)party.PartyTradeGold) : 0f);
		num7 += ((party.LeaderHero != null) ? ((float)party.LeaderHero.Gold * 0.15f) : 0f);
		float num8 = (partyToLetGo.IsPartyTradeActive ? ((float)partyToLetGo.PartyTradeGold) : 0f);
		num7 += ((partyToLetGo.LeaderHero != null) ? ((float)partyToLetGo.LeaderHero.Gold * 0.15f) : 0f);
		float num9 = num5 + 10000f;
		if (partyToLetGo.BesiegedSettlement != null)
		{
			num9 += 20000f;
		}
		return -1000f + (1f - num) * num4 - num * num9 - num * num8 + (1f - num) * num7 + num * num6 + (num3 * (1f - num) - num * num2);
	}

	public override float GetValueOfHeroForFaction(Hero examinedHero, IFaction targetFaction, bool forMarriage = false)
	{
		return GetHeroCommandingStrengthForClan(examinedHero) * 10f;
	}

	public override int GetRelationCostOfExpellingClanFromKingdom()
	{
		return -20;
	}

	public override int GetInfluenceCostOfSupportingClan()
	{
		return 50;
	}

	public override int GetInfluenceCostOfExpellingClan(Clan proposingClan)
	{
		ExplainedNumber cost = new ExplainedNumber(200f);
		GetPerkEffectsOnKingdomDecisionInfluenceCost(proposingClan, ref cost);
		return MathF.Round(cost.ResultNumber);
	}

	public override int GetInfluenceCostOfProposingPeace(Clan proposingClan)
	{
		ExplainedNumber cost = new ExplainedNumber(100f);
		GetPerkEffectsOnKingdomDecisionInfluenceCost(proposingClan, ref cost);
		return MathF.Round(cost.ResultNumber);
	}

	public override int GetInfluenceCostOfProposingWar(Clan proposingClan)
	{
		ExplainedNumber cost = new ExplainedNumber(200f);
		if (proposingClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.WarTax) && proposingClan == proposingClan.Kingdom.RulingClan)
		{
			cost.AddFactor(1f);
		}
		GetPerkEffectsOnKingdomDecisionInfluenceCost(proposingClan, ref cost);
		return MathF.Round(cost.ResultNumber);
	}

	public override int GetInfluenceValueOfSupportingClan()
	{
		return GetInfluenceCostOfSupportingClan() / 4;
	}

	public override int GetRelationValueOfSupportingClan()
	{
		return 1;
	}

	public override int GetInfluenceCostOfAnnexation(Clan proposingClan)
	{
		ExplainedNumber cost = new ExplainedNumber(200f);
		if (proposingClan.Kingdom != null)
		{
			if (proposingClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.FeudalInheritance))
			{
				cost.AddFactor(1f);
			}
			if (proposingClan.Kingdom.ActivePolicies.Contains(DefaultPolicies.PrecarialLandTenure) && proposingClan == proposingClan.Kingdom.RulingClan)
			{
				cost.AddFactor(-0.5f);
			}
		}
		GetPerkEffectsOnKingdomDecisionInfluenceCost(proposingClan, ref cost);
		return MathF.Round(cost.ResultNumber);
	}

	public override int GetInfluenceCostOfChangingLeaderOfArmy()
	{
		return 30;
	}

	public override int GetInfluenceCostOfDisbandingArmy()
	{
		int num = 30;
		if (Clan.PlayerClan.Kingdom != null && Clan.PlayerClan == Clan.PlayerClan.Kingdom.RulingClan)
		{
			num /= 2;
		}
		return num;
	}

	public override int GetRelationCostOfDisbandingArmy(bool isLeaderParty)
	{
		if (!isLeaderParty)
		{
			return -1;
		}
		return -4;
	}

	public override int GetInfluenceCostOfPolicyProposalAndDisavowal(Clan proposerClan)
	{
		ExplainedNumber cost = new ExplainedNumber(100f);
		GetPerkEffectsOnKingdomDecisionInfluenceCost(proposerClan, ref cost);
		return MathF.Round(cost.ResultNumber);
	}

	public override int GetInfluenceCostOfAbandoningArmy()
	{
		return 2;
	}

	private void GetPerkEffectsOnKingdomDecisionInfluenceCost(Clan proposingClan, ref ExplainedNumber cost)
	{
		if (proposingClan.Leader.GetPerkValue(DefaultPerks.Charm.Firebrand))
		{
			cost.AddFactor(DefaultPerks.Charm.Firebrand.PrimaryBonus, DefaultPerks.Charm.Firebrand.Name);
		}
	}

	private int GetBaseRelationBetweenHeroes(Hero hero1, Hero hero2)
	{
		return CharacterRelationManager.GetHeroRelation(hero1, hero2);
	}

	public override int GetBaseRelation(Hero hero1, Hero hero2)
	{
		return GetBaseRelationBetweenHeroes(hero1, hero2);
	}

	public override int GetEffectiveRelation(Hero hero1, Hero hero2)
	{
		GetHeroesForEffectiveRelation(hero1, hero2, out var effectiveHero, out var effectiveHero2);
		if (effectiveHero == null || effectiveHero2 == null)
		{
			return 0;
		}
		int effectiveRelation = GetBaseRelationBetweenHeroes(effectiveHero, effectiveHero2);
		GetPersonalityEffects(ref effectiveRelation, hero1, effectiveHero2);
		return MBMath.ClampInt(effectiveRelation, MinRelationLimit, MaxRelationLimit);
	}

	public override void GetHeroesForEffectiveRelation(Hero hero1, Hero hero2, out Hero effectiveHero1, out Hero effectiveHero2)
	{
		effectiveHero1 = ((hero1.Clan != null) ? hero1.Clan.Leader : hero1);
		effectiveHero2 = ((hero2.Clan != null) ? hero2.Clan.Leader : hero2);
		if (effectiveHero1 == effectiveHero2 || (hero1.IsPlayerCompanion && hero2.IsHumanPlayerCharacter) || (hero1.IsPlayerCompanion && hero2.IsHumanPlayerCharacter))
		{
			effectiveHero1 = hero1;
			effectiveHero2 = hero2;
		}
	}

	public override int GetRelationChangeAfterClanLeaderIsDead(Hero deadLeader, Hero relationHero)
	{
		return (int)((float)CharacterRelationManager.GetHeroRelation(deadLeader, relationHero) * 0.7f);
	}

	public override int GetRelationChangeAfterVotingInSettlementOwnerPreliminaryDecision(Hero supporter, bool hasHeroVotedAgainstOwner)
	{
		int num;
		if (hasHeroVotedAgainstOwner)
		{
			num = -20;
			if (supporter.Culture.HasFeat(DefaultCulturalFeats.SturgianDecisionPenaltyFeat))
			{
				num += (int)((float)num * DefaultCulturalFeats.SturgianDecisionPenaltyFeat.EffectBonus);
			}
		}
		else
		{
			num = 5;
		}
		return num;
	}

	private void GetPersonalityEffects(ref int effectiveRelation, Hero hero1, Hero effectiveHero2)
	{
		GetTraitEffect(ref effectiveRelation, hero1, effectiveHero2, DefaultTraits.Honor, 2);
		GetTraitEffect(ref effectiveRelation, hero1, effectiveHero2, DefaultTraits.Valor, 1);
		GetTraitEffect(ref effectiveRelation, hero1, effectiveHero2, DefaultTraits.Mercy, 1);
	}

	private void GetTraitEffect(ref int effectiveRelation, Hero hero1, Hero effectiveHero2, TraitObject trait, int effectMagnitude)
	{
		int traitLevel = hero1.GetTraitLevel(trait);
		int traitLevel2 = effectiveHero2.GetTraitLevel(trait);
		int num = traitLevel * traitLevel2;
		if (num > 0)
		{
			effectiveRelation += effectMagnitude;
		}
		else if (num < 0)
		{
			effectiveRelation -= effectMagnitude;
		}
	}

	public override int GetCharmExperienceFromRelationGain(Hero hero, float relationChange, ChangeRelationAction.ChangeRelationDetail detail)
	{
		float num = 20f;
		if (detail == ChangeRelationAction.ChangeRelationDetail.Emissary)
		{
			num = (hero.IsNotable ? (num * 20f) : ((hero.MapFaction != null && hero.MapFaction.Leader == hero) ? (num * 30f) : ((hero.Clan == null || hero.Clan.Leader != hero) ? (num * 10f) : (num * 20f))));
		}
		else if (!hero.IsNotable)
		{
			if (hero.MapFaction != null && hero.MapFaction.Leader == hero)
			{
				num *= 30f;
			}
			else if (hero.Clan != null && hero.Clan.Leader == hero)
			{
				num *= 20f;
			}
		}
		return MathF.Round(num * relationChange);
	}

	public override uint GetNotificationColor(ChatNotificationType notificationType)
	{
		return notificationType switch
		{
			ChatNotificationType.Default => 10066329u, 
			ChatNotificationType.Neutral => 12303291u, 
			ChatNotificationType.PlayerClanPositive => 3407803u, 
			ChatNotificationType.PlayerClanNegative => 16750899u, 
			ChatNotificationType.PlayerFactionPositive => 2284902u, 
			ChatNotificationType.PlayerFactionNegative => 14509602u, 
			ChatNotificationType.PlayerFactionIndirectPositive => 12298820u, 
			ChatNotificationType.PlayerFactionIndirectNegative => 13382502u, 
			ChatNotificationType.Civilian => 10053324u, 
			ChatNotificationType.PlayerFactionCivilian => 11163101u, 
			ChatNotificationType.PlayerClanCivilian => 15623935u, 
			ChatNotificationType.Political => 6724044u, 
			ChatNotificationType.PlayerFactionPolitical => 5614301u, 
			ChatNotificationType.PlayerClanPolitical => 6745855u, 
			_ => 13369548u, 
		};
	}

	public override float DenarsToInfluence()
	{
		return 0.002f;
	}

	public override bool CanSettlementBeGifted(Settlement settlementToGift)
	{
		if (settlementToGift.Town != null)
		{
			return !settlementToGift.Town.IsOwnerUnassigned;
		}
		return false;
	}

	public override IEnumerable<BarterGroup> GetBarterGroups()
	{
		return new BarterGroup[6]
		{
			new GoldBarterGroup(),
			new ItemBarterGroup(),
			new PrisonerBarterGroup(),
			new FiefBarterGroup(),
			new OtherBarterGroup(),
			new DefaultsBarterGroup()
		};
	}

	public override int GetValueOfDailyTribute(int dailyTributeAmount)
	{
		return dailyTributeAmount * 70;
	}

	public override int GetDailyTributeForValue(int value)
	{
		return value / 70 / 10 * 10;
	}

	public override bool IsClanEligibleToBecomeRuler(Clan clan)
	{
		if (!clan.IsEliminated && clan.Leader.IsAlive)
		{
			return !clan.IsUnderMercenaryService;
		}
		return false;
	}
}
