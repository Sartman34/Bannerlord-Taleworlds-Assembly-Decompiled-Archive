using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultPartyHealingModel : PartyHealingModel
{
	private const int StarvingEffectHeroes = -19;

	private const int FortificationEffectForHeroes = 8;

	private const int FortificationEffectForRegulars = 10;

	private const int BaseDailyHealingForHeroes = 11;

	private const int BaseDailyHealingForTroops = 5;

	private const int SkillEXPFromHealingTroops = 5;

	private const float StarvingWoundedEffectRatio = 0.25f;

	private const float StarvingWoundedEffectRatioForGarrison = 0.1f;

	private static readonly TextObject _starvingText = new TextObject("{=jZYUdkXF}Starving");

	private static readonly TextObject _settlementText = new TextObject("{=M0Gpl0dH}In Settlement");

	public override float GetSurgeryChance(PartyBase party)
	{
		int num = party.MobileParty?.EffectiveSurgeon?.GetSkillValue(DefaultSkills.Medicine) ?? 0;
		return 0.0015f * (float)num;
	}

	public override float GetSiegeBombardmentHitSurgeryChance(PartyBase party)
	{
		float num = 0f;
		if (party != null && party.IsMobile && party.MobileParty.HasPerk(DefaultPerks.Medicine.SiegeMedic))
		{
			num += DefaultPerks.Medicine.SiegeMedic.PrimaryBonus;
		}
		return num;
	}

	public override float GetSurvivalChance(PartyBase party, CharacterObject character, DamageTypes damageType, bool canDamageKillEvenIfBlunt, PartyBase enemyParty = null)
	{
		if ((damageType == DamageTypes.Blunt && !canDamageKillEvenIfBlunt) || (character.IsHero && CampaignOptions.BattleDeath == CampaignOptions.Difficulty.VeryEasy) || (character.IsPlayerCharacter && CampaignOptions.BattleDeath == CampaignOptions.Difficulty.Easy))
		{
			return 1f;
		}
		ExplainedNumber stat = new ExplainedNumber(1f);
		if (party?.MobileParty != null)
		{
			MobileParty mobileParty = party.MobileParty;
			SkillHelper.AddSkillBonusForParty(DefaultSkills.Medicine, DefaultSkillEffects.SurgeonSurvivalBonus, mobileParty, ref stat);
			if (enemyParty?.MobileParty != null && enemyParty.MobileParty.HasPerk(DefaultPerks.Medicine.DoctorsOath))
			{
				SkillHelper.AddSkillBonusForParty(DefaultSkills.Medicine, DefaultSkillEffects.SurgeonSurvivalBonus, enemyParty.MobileParty, ref stat);
				SkillLevelingManager.OnSurgeryApplied(enemyParty.MobileParty, surgerySuccess: false, character.Tier);
			}
			stat.Add((float)character.Level * 0.02f);
			if (!character.IsHero && party.MapEvent != null && character.Tier < 3)
			{
				PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.PhysicianOfPeople, party.MobileParty, isPrimaryBonus: false, ref stat);
			}
			if (character.IsHero)
			{
				stat.Add(character.GetTotalArmorSum() * 0.01f);
				stat.Add(character.Age * -0.01f);
				stat.AddFactor(50f);
			}
			ExplainedNumber explainedNumber = new ExplainedNumber(1f / stat.ResultNumber);
			if (character.IsHero)
			{
				if (party.IsMobile && party.MobileParty.HasPerk(DefaultPerks.Medicine.CheatDeath, checkSecondaryRole: true))
				{
					explainedNumber.AddFactor(DefaultPerks.Medicine.CheatDeath.SecondaryBonus, DefaultPerks.Medicine.CheatDeath.Name);
				}
				if (character.HeroObject.Clan == Clan.PlayerClan)
				{
					float clanMemberDeathChanceMultiplier = Campaign.Current.Models.DifficultyModel.GetClanMemberDeathChanceMultiplier();
					if (!clanMemberDeathChanceMultiplier.ApproximatelyEqualsTo(0f))
					{
						explainedNumber.AddFactor(clanMemberDeathChanceMultiplier, GameTexts.FindText("str_game_difficulty"));
					}
				}
			}
			return 1f - MBMath.ClampFloat(explainedNumber.ResultNumber, 0f, 1f);
		}
		if (character.IsHero && character.HeroObject.IsPrisoner)
		{
			return 1f - character.Age * 0.0035f;
		}
		if (stat.ResultNumber.ApproximatelyEqualsTo(0f))
		{
			return 0f;
		}
		return 1f - 1f / stat.ResultNumber;
	}

	public override int GetSkillXpFromHealingTroop(PartyBase party)
	{
		return 5;
	}

	public override ExplainedNumber GetDailyHealingForRegulars(MobileParty party, bool includeDescriptions = false)
	{
		ExplainedNumber stat = new ExplainedNumber(0f, includeDescriptions);
		if (party.Party.IsStarving || (party.IsGarrison && party.CurrentSettlement.IsStarving))
		{
			if (party.IsGarrison)
			{
				if (SettlementHelper.IsGarrisonStarving(party.CurrentSettlement))
				{
					int num = MBRandom.RoundRandomized((float)party.MemberRoster.TotalRegulars * 0.1f);
					stat.Add(-num, _starvingText);
				}
			}
			else
			{
				int totalRegulars = party.MemberRoster.TotalRegulars;
				stat.Add((float)(-totalRegulars) * 0.25f, _starvingText);
			}
		}
		else
		{
			stat = new ExplainedNumber(5f, includeDescriptions);
			if (party.IsGarrison)
			{
				if (party.CurrentSettlement.IsTown)
				{
					SkillHelper.AddSkillBonusForTown(DefaultSkills.Medicine, DefaultSkillEffects.GovernorHealingRateBonus, party.CurrentSettlement.Town, ref stat);
				}
			}
			else
			{
				SkillHelper.AddSkillBonusForParty(DefaultSkills.Medicine, DefaultSkillEffects.HealingRateBonusForRegulars, party, ref stat);
			}
			if (!party.IsGarrison && !party.IsMilitia)
			{
				if (!party.IsMoving)
				{
					PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.TriageTent, party, isPrimaryBonus: true, ref stat);
					PerkHelper.AddPerkBonusForParty(DefaultPerks.Athletics.AGoodDaysRest, party, isPrimaryBonus: true, ref stat);
				}
				else
				{
					PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.WalkItOff, party, isPrimaryBonus: true, ref stat);
					PerkHelper.AddPerkBonusForParty(DefaultPerks.Athletics.WalkItOff, party, isPrimaryBonus: true, ref stat);
				}
			}
			if (party.Morale >= Campaign.Current.Models.PartyMoraleModel.HighMoraleValue)
			{
				PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.BestMedicine, party, isPrimaryBonus: true, ref stat);
			}
			if (party.CurrentSettlement != null && !party.CurrentSettlement.IsHideout)
			{
				if (party.CurrentSettlement.IsFortification)
				{
					stat.Add(10f, _settlementText);
				}
				if (party.SiegeEvent == null && !party.CurrentSettlement.IsUnderSiege && !party.CurrentSettlement.IsRaided && !party.CurrentSettlement.IsUnderRaid)
				{
					if (party.CurrentSettlement.IsTown)
					{
						PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.PristineStreets, party, isPrimaryBonus: false, ref stat);
					}
					PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.GoodLogdings, party, isPrimaryBonus: true, ref stat);
				}
			}
			else if (!party.IsMoving && party.LastVisitedSettlement != null && party.LastVisitedSettlement.IsVillage && party.LastVisitedSettlement.Position2D.DistanceSquared(party.Position2D) < 2f && !party.LastVisitedSettlement.IsUnderRaid && !party.LastVisitedSettlement.IsRaided)
			{
				PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.BushDoctor, party, isPrimaryBonus: false, ref stat);
			}
			if (party.Army != null)
			{
				PerkHelper.AddPerkBonusForParty(DefaultPerks.Scouting.Rearguard, party, isPrimaryBonus: true, ref stat);
			}
			if (party.ItemRoster.FoodVariety > 0 && party.HasPerk(DefaultPerks.Medicine.PerfectHealth))
			{
				stat.AddFactor((float)party.ItemRoster.FoodVariety * DefaultPerks.Medicine.PerfectHealth.PrimaryBonus, DefaultPerks.Medicine.PerfectHealth.Name);
			}
			if (party.HasPerk(DefaultPerks.Medicine.HelpingHands))
			{
				float value = (float)MathF.Floor((float)party.MemberRoster.TotalManCount / 10f) * DefaultPerks.Medicine.HelpingHands.PrimaryBonus;
				stat.AddFactor(value, DefaultPerks.Medicine.HelpingHands.Name);
			}
		}
		return stat;
	}

	public override ExplainedNumber GetDailyHealingHpForHeroes(MobileParty party, bool includeDescriptions = false)
	{
		if (party.Party.IsStarving && party.CurrentSettlement == null)
		{
			return new ExplainedNumber(-19f, includeDescriptions, _starvingText);
		}
		ExplainedNumber stat = new ExplainedNumber(11f, includeDescriptions);
		if (!party.IsGarrison && !party.IsMilitia)
		{
			if (!party.IsMoving)
			{
				PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.TriageTent, party, isPrimaryBonus: true, ref stat);
				PerkHelper.AddPerkBonusForParty(DefaultPerks.Athletics.AGoodDaysRest, party, isPrimaryBonus: true, ref stat);
			}
			else
			{
				PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.WalkItOff, party, isPrimaryBonus: true, ref stat);
				PerkHelper.AddPerkBonusForParty(DefaultPerks.Athletics.WalkItOff, party, isPrimaryBonus: true, ref stat);
			}
		}
		if (party.Morale >= Campaign.Current.Models.PartyMoraleModel.HighMoraleValue)
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.BestMedicine, party, isPrimaryBonus: true, ref stat);
		}
		if (party.CurrentSettlement != null && !party.CurrentSettlement.IsHideout)
		{
			if (party.CurrentSettlement.IsFortification)
			{
				stat.Add(8f, _settlementText);
			}
			if (party.CurrentSettlement.IsTown)
			{
				PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.PristineStreets, party, isPrimaryBonus: false, ref stat);
			}
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.GoodLogdings, party, isPrimaryBonus: true, ref stat);
		}
		else if (!party.IsMoving && party.LastVisitedSettlement != null && party.LastVisitedSettlement.IsVillage && party.LastVisitedSettlement.Position2D.DistanceSquared(party.Position2D) < 2f && !party.LastVisitedSettlement.IsUnderRaid && !party.LastVisitedSettlement.IsRaided)
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.BushDoctor, party, isPrimaryBonus: false, ref stat);
		}
		SkillHelper.AddSkillBonusForParty(DefaultSkills.Medicine, DefaultSkillEffects.HealingRateBonusForHeroes, party, ref stat);
		return stat;
	}

	public override int GetHeroesEffectedHealingAmount(Hero hero, float healingRate)
	{
		ExplainedNumber bonuses = new ExplainedNumber(healingRate);
		PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Medicine.SelfMedication, hero.CharacterObject, isPrimaryBonus: true, ref bonuses);
		float resultNumber = bonuses.ResultNumber;
		if (resultNumber - (float)(int)resultNumber > MBRandom.RandomFloat)
		{
			return (int)resultNumber + 1;
		}
		return (int)resultNumber;
	}

	public override int GetBattleEndHealingAmount(MobileParty party, Hero hero)
	{
		float num = 0f;
		if (hero.GetPerkValue(DefaultPerks.Medicine.PreventiveMedicine))
		{
			num += (float)(hero.MaxHitPoints - hero.HitPoints) * DefaultPerks.Medicine.PreventiveMedicine.SecondaryBonus;
		}
		if (party.MapEventSide == party.MapEvent.AttackerSide && hero.GetPerkValue(DefaultPerks.Medicine.WalkItOff))
		{
			num += DefaultPerks.Medicine.WalkItOff.SecondaryBonus;
		}
		return MathF.Round(num);
	}
}
