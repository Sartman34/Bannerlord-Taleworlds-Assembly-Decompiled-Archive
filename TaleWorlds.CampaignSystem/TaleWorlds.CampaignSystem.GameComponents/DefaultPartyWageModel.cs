using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultPartyWageModel : PartyWageModel
{
	private static readonly TextObject _cultureText = GameTexts.FindText("str_culture");

	private static readonly TextObject _buildingEffects = GameTexts.FindText("str_building_effects");

	private const float MercenaryWageFactor = 1.5f;

	public override int MaxWage => 10000;

	public override int GetCharacterWage(CharacterObject character)
	{
		int num = character.Tier switch
		{
			0 => 1, 
			1 => 2, 
			2 => 3, 
			3 => 5, 
			4 => 8, 
			5 => 12, 
			6 => 17, 
			_ => 23, 
		};
		if (character.Occupation == Occupation.Mercenary)
		{
			num = (int)((float)num * 1.5f);
		}
		return num;
	}

	public override ExplainedNumber GetTotalWage(MobileParty mobileParty, bool includeDescriptions = false)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		bool flag = !mobileParty.HasPerk(DefaultPerks.Steward.AidCorps);
		int num13 = 0;
		int num14 = 0;
		for (int i = 0; i < mobileParty.MemberRoster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = mobileParty.MemberRoster.GetElementCopyAtIndex(i);
			CharacterObject character = elementCopyAtIndex.Character;
			int num15 = (flag ? elementCopyAtIndex.Number : (elementCopyAtIndex.Number - elementCopyAtIndex.WoundedNumber));
			if (character.IsHero)
			{
				if (elementCopyAtIndex.Character.HeroObject != character.HeroObject.Clan?.Leader)
				{
					num4 = ((mobileParty.LeaderHero == null || !mobileParty.LeaderHero.GetPerkValue(DefaultPerks.Steward.PaidInPromise)) ? (num4 + elementCopyAtIndex.Character.TroopWage) : (num4 + MathF.Round((float)elementCopyAtIndex.Character.TroopWage * (1f + DefaultPerks.Steward.PaidInPromise.PrimaryBonus))));
				}
				continue;
			}
			if (character.Tier < 4)
			{
				if (character.Culture.IsBandit)
				{
					num10 += elementCopyAtIndex.Character.TroopWage * elementCopyAtIndex.Number;
				}
				num2 += elementCopyAtIndex.Character.TroopWage * num15;
			}
			else if (character.Tier == 4)
			{
				if (character.Culture.IsBandit)
				{
					num11 += elementCopyAtIndex.Character.TroopWage * elementCopyAtIndex.Number;
				}
				num3 += elementCopyAtIndex.Character.TroopWage * num15;
			}
			else if (character.Tier > 4)
			{
				if (character.Culture.IsBandit)
				{
					num12 += elementCopyAtIndex.Character.TroopWage * elementCopyAtIndex.Number;
				}
				num4 += elementCopyAtIndex.Character.TroopWage * num15;
			}
			if (character.IsInfantry)
			{
				num5 += num15;
			}
			if (character.IsMounted)
			{
				num6 += num15;
			}
			if (character.Occupation == Occupation.CaravanGuard)
			{
				num13 += elementCopyAtIndex.Number;
			}
			if (character.Occupation == Occupation.Mercenary)
			{
				num14 += elementCopyAtIndex.Number;
			}
			if (character.IsRanged)
			{
				num7 += num15;
				if (character.Tier >= 4)
				{
					num8 += num15;
					num9 += elementCopyAtIndex.Character.TroopWage * elementCopyAtIndex.Number;
				}
			}
		}
		ExplainedNumber bonuses = new ExplainedNumber(0f, includeDescriptions: false, null);
		if (mobileParty.LeaderHero != null && mobileParty.LeaderHero.GetPerkValue(DefaultPerks.Roguery.DeepPockets))
		{
			num2 -= num10;
			num3 -= num11;
			num4 -= num12;
			int num16 = num10 + num11 + num12;
			bonuses.Add(num16);
			PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Roguery.DeepPockets, mobileParty.LeaderHero.CharacterObject, isPrimaryBonus: false, ref bonuses);
		}
		num = num2 + num3 + num4;
		if (mobileParty.HasPerk(DefaultPerks.Crossbow.PickedShots) && num8 > 0)
		{
			float num17 = (float)num9 * DefaultPerks.Crossbow.PickedShots.PrimaryBonus;
			num += (int)num17;
		}
		ExplainedNumber bonuses2 = new ExplainedNumber(num, includeDescriptions);
		ExplainedNumber explainedNumber = new ExplainedNumber(1f);
		if (mobileParty.IsGarrison && mobileParty.CurrentSettlement?.Town != null)
		{
			if (mobileParty.CurrentSettlement.IsTown)
			{
				PerkHelper.AddPerkBonusForTown(DefaultPerks.OneHanded.MilitaryTradition, mobileParty.CurrentSettlement.Town, ref bonuses2);
				PerkHelper.AddPerkBonusForTown(DefaultPerks.TwoHanded.Berserker, mobileParty.CurrentSettlement.Town, ref bonuses2);
				PerkHelper.AddPerkBonusForTown(DefaultPerks.Bow.HunterClan, mobileParty.CurrentSettlement.Town, ref bonuses2);
				float troopRatio = (float)num5 / (float)mobileParty.MemberRoster.TotalRegulars;
				CalculatePartialGarrisonWageReduction(troopRatio, mobileParty, DefaultPerks.Polearm.StandardBearer, ref bonuses2, isSecondaryEffect: true);
				float troopRatio2 = (float)num6 / (float)mobileParty.MemberRoster.TotalRegulars;
				CalculatePartialGarrisonWageReduction(troopRatio2, mobileParty, DefaultPerks.Riding.CavalryTactics, ref bonuses2, isSecondaryEffect: true);
				float troopRatio3 = (float)num7 / (float)mobileParty.MemberRoster.TotalRegulars;
				CalculatePartialGarrisonWageReduction(troopRatio3, mobileParty, DefaultPerks.Crossbow.PeasantLeader, ref bonuses2, isSecondaryEffect: true);
			}
			else if (mobileParty.CurrentSettlement.IsCastle)
			{
				PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.StiffUpperLip, mobileParty.CurrentSettlement.Town, ref bonuses2);
			}
			PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.DrillSergant, mobileParty.CurrentSettlement.Town, ref bonuses2);
			if (mobileParty.CurrentSettlement.Culture.HasFeat(DefaultCulturalFeats.EmpireGarrisonWageFeat))
			{
				bonuses2.AddFactor(DefaultCulturalFeats.EmpireGarrisonWageFeat.EffectBonus, GameTexts.FindText("str_culture"));
			}
			foreach (Building building in mobileParty.CurrentSettlement.Town.Buildings)
			{
				float buildingEffectAmount = building.GetBuildingEffectAmount(BuildingEffectEnum.GarrisonWageReduce);
				if (buildingEffectAmount > 0f)
				{
					explainedNumber.AddFactor(0f - buildingEffectAmount / 100f, building.Name);
				}
			}
		}
		bonuses2.Add(bonuses.ResultNumber);
		float value = ((mobileParty.LeaderHero != null && mobileParty.LeaderHero.Clan.Kingdom != null && !mobileParty.LeaderHero.Clan.IsUnderMercenaryService && mobileParty.LeaderHero.Clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.MilitaryCoronae)) ? 0.1f : 0f);
		if (mobileParty.HasPerk(DefaultPerks.Trade.SwordForBarter, checkSecondaryRole: true))
		{
			float num18 = (float)num13 / (float)mobileParty.MemberRoster.TotalRegulars;
			if (num18 > 0f)
			{
				float value2 = DefaultPerks.Trade.SwordForBarter.SecondaryBonus * num18;
				bonuses2.AddFactor(value2, DefaultPerks.Trade.SwordForBarter.Name);
			}
		}
		if (mobileParty.HasPerk(DefaultPerks.Steward.Contractors))
		{
			float num19 = (float)num14 / (float)mobileParty.MemberRoster.TotalRegulars;
			if (num19 > 0f)
			{
				float value3 = DefaultPerks.Steward.Contractors.PrimaryBonus * num19;
				bonuses2.AddFactor(value3, DefaultPerks.Steward.Contractors.Name);
			}
		}
		if (mobileParty.HasPerk(DefaultPerks.Trade.MercenaryConnections, checkSecondaryRole: true))
		{
			float num20 = (float)num14 / (float)mobileParty.MemberRoster.TotalRegulars;
			if (num20 > 0f)
			{
				float value4 = DefaultPerks.Trade.MercenaryConnections.SecondaryBonus * num20;
				bonuses2.AddFactor(value4, DefaultPerks.Trade.MercenaryConnections.Name);
			}
		}
		bonuses2.AddFactor(value, DefaultPolicies.MilitaryCoronae.Name);
		bonuses2.AddFactor(explainedNumber.ResultNumber - 1f, _buildingEffects);
		if (PartyBaseHelper.HasFeat(mobileParty.Party, DefaultCulturalFeats.AseraiIncreasedWageFeat))
		{
			bonuses2.AddFactor(DefaultCulturalFeats.AseraiIncreasedWageFeat.EffectBonus, _cultureText);
		}
		if (mobileParty.HasPerk(DefaultPerks.Steward.Frugal))
		{
			bonuses2.AddFactor(DefaultPerks.Steward.Frugal.PrimaryBonus, DefaultPerks.Steward.Frugal.Name);
		}
		if (mobileParty.Army != null && mobileParty.HasPerk(DefaultPerks.Steward.EfficientCampaigner, checkSecondaryRole: true))
		{
			bonuses2.AddFactor(DefaultPerks.Steward.EfficientCampaigner.SecondaryBonus, DefaultPerks.Steward.EfficientCampaigner.Name);
		}
		if (mobileParty.SiegeEvent != null && mobileParty.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(mobileParty.Party) && mobileParty.HasPerk(DefaultPerks.Steward.MasterOfWarcraft))
		{
			bonuses2.AddFactor(DefaultPerks.Steward.MasterOfWarcraft.PrimaryBonus, DefaultPerks.Steward.MasterOfWarcraft.Name);
		}
		if (mobileParty.EffectiveQuartermaster != null)
		{
			PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Steward.PriceOfLoyalty, mobileParty.EffectiveQuartermaster.CharacterObject, DefaultSkills.Steward, applyPrimaryBonus: true, ref bonuses2, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
		}
		if (mobileParty.CurrentSettlement != null && mobileParty.LeaderHero != null && mobileParty.LeaderHero.GetPerkValue(DefaultPerks.Trade.ContentTrades))
		{
			bonuses2.AddFactor(DefaultPerks.Trade.ContentTrades.SecondaryBonus, DefaultPerks.Trade.ContentTrades.Name);
		}
		return bonuses2;
	}

	private void CalculatePartialGarrisonWageReduction(float troopRatio, MobileParty mobileParty, PerkObject perk, ref ExplainedNumber garrisonWageReductionMultiplier, bool isSecondaryEffect)
	{
		if (troopRatio > 0f && mobileParty.CurrentSettlement.Town.Governor != null && PerkHelper.GetPerkValueForTown(perk, mobileParty.CurrentSettlement.Town))
		{
			garrisonWageReductionMultiplier.AddFactor(isSecondaryEffect ? (perk.SecondaryBonus * troopRatio) : (perk.PrimaryBonus * troopRatio), perk.Name);
		}
	}

	public override int GetTroopRecruitmentCost(CharacterObject troop, Hero buyerHero, bool withoutItemCost = false)
	{
		int num = 10 * MathF.Round((float)troop.Level * MathF.Pow(troop.Level, 0.65f) * 0.2f);
		num = ((troop.Level <= 1) ? 10 : ((troop.Level <= 6) ? 20 : ((troop.Level <= 11) ? 50 : ((troop.Level <= 16) ? 100 : ((troop.Level <= 21) ? 200 : ((troop.Level <= 26) ? 400 : ((troop.Level <= 31) ? 600 : ((troop.Level > 36) ? 1500 : 1000))))))));
		if (troop.Equipment.Horse.Item != null && !withoutItemCost)
		{
			num = ((troop.Level >= 26) ? (num + 500) : (num + 150));
		}
		bool flag = troop.Occupation == Occupation.Mercenary || troop.Occupation == Occupation.Gangster || troop.Occupation == Occupation.CaravanGuard;
		if (flag)
		{
			num = MathF.Round((float)num * 2f);
		}
		if (buyerHero != null)
		{
			ExplainedNumber explainedNumber = new ExplainedNumber(1f);
			if (troop.Tier >= 2 && buyerHero.GetPerkValue(DefaultPerks.Throwing.HeadHunter))
			{
				explainedNumber.AddFactor(DefaultPerks.Throwing.HeadHunter.SecondaryBonus);
			}
			if (troop.IsInfantry)
			{
				if (buyerHero.GetPerkValue(DefaultPerks.OneHanded.ChinkInTheArmor))
				{
					explainedNumber.AddFactor(DefaultPerks.OneHanded.ChinkInTheArmor.SecondaryBonus);
				}
				if (buyerHero.GetPerkValue(DefaultPerks.TwoHanded.ShowOfStrength))
				{
					explainedNumber.AddFactor(DefaultPerks.TwoHanded.ShowOfStrength.SecondaryBonus);
				}
				if (buyerHero.GetPerkValue(DefaultPerks.Polearm.HardyFrontline))
				{
					explainedNumber.AddFactor(DefaultPerks.Polearm.HardyFrontline.SecondaryBonus);
				}
				if (buyerHero.Culture.HasFeat(DefaultCulturalFeats.SturgianRecruitUpgradeFeat))
				{
					explainedNumber.AddFactor(DefaultCulturalFeats.SturgianRecruitUpgradeFeat.EffectBonus, GameTexts.FindText("str_culture"));
				}
			}
			else if (troop.IsRanged)
			{
				if (buyerHero.GetPerkValue(DefaultPerks.Bow.RenownedArcher))
				{
					explainedNumber.AddFactor(DefaultPerks.Bow.RenownedArcher.SecondaryBonus);
				}
				if (buyerHero.GetPerkValue(DefaultPerks.Crossbow.Piercer))
				{
					explainedNumber.AddFactor(DefaultPerks.Crossbow.Piercer.SecondaryBonus);
				}
			}
			if (troop.IsMounted && buyerHero.Culture.HasFeat(DefaultCulturalFeats.KhuzaitRecruitUpgradeFeat))
			{
				explainedNumber.AddFactor(DefaultCulturalFeats.KhuzaitRecruitUpgradeFeat.EffectBonus, GameTexts.FindText("str_culture"));
			}
			if (buyerHero.IsPartyLeader && buyerHero.GetPerkValue(DefaultPerks.Steward.Frugal))
			{
				explainedNumber.AddFactor(DefaultPerks.Steward.Frugal.SecondaryBonus);
			}
			if (flag)
			{
				if (buyerHero.GetPerkValue(DefaultPerks.Trade.SwordForBarter))
				{
					explainedNumber.AddFactor(DefaultPerks.Trade.SwordForBarter.PrimaryBonus);
				}
				if (buyerHero.GetPerkValue(DefaultPerks.Charm.SlickNegotiator))
				{
					explainedNumber.AddFactor(DefaultPerks.Charm.SlickNegotiator.PrimaryBonus);
				}
			}
			num = MathF.Max(1, MathF.Round((float)num * explainedNumber.ResultNumber));
		}
		return num;
	}
}
