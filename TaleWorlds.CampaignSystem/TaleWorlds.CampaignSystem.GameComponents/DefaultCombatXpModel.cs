using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultCombatXpModel : CombatXpModel
{
	public override float CaptainRadius => 10f;

	public override SkillObject GetSkillForWeapon(WeaponComponentData weapon, bool isSiegeEngineHit)
	{
		SkillObject result = DefaultSkills.Athletics;
		if (isSiegeEngineHit)
		{
			result = DefaultSkills.Engineering;
		}
		else if (weapon != null)
		{
			result = weapon.RelevantSkill;
		}
		return result;
	}

	public override void GetXpFromHit(CharacterObject attackerTroop, CharacterObject captain, CharacterObject attackedTroop, PartyBase party, int damage, bool isFatal, MissionTypeEnum missionType, out int xpAmount)
	{
		int num = attackedTroop.MaxHitPoints();
		MilitaryPowerModel militaryPowerModel = Campaign.Current.Models.MilitaryPowerModel;
		float defaultTroopPower = militaryPowerModel.GetDefaultTroopPower(attackedTroop);
		float defaultTroopPower2 = militaryPowerModel.GetDefaultTroopPower(attackerTroop);
		float leaderModifier = 0f;
		float contextModifier = 0f;
		if (party?.MapEvent != null)
		{
			contextModifier = militaryPowerModel.GetContextModifier(attackedTroop, party.Side, party.MapEvent.SimulationContext);
			leaderModifier = party.MapEventSide.LeaderSimulationModifier;
		}
		float troopPower = militaryPowerModel.GetTroopPower(defaultTroopPower, leaderModifier, contextModifier);
		float troopPower2 = militaryPowerModel.GetTroopPower(defaultTroopPower2, leaderModifier, contextModifier);
		float num2 = 0.4f * (troopPower2 + 0.5f) * (troopPower + 0.5f) * (float)(MathF.Min(damage, num) + (isFatal ? num : 0));
		num2 *= missionType switch
		{
			MissionTypeEnum.Battle => 1f, 
			MissionTypeEnum.SimulationBattle => 0.9f, 
			MissionTypeEnum.Tournament => 0.33f, 
			MissionTypeEnum.PracticeFight => 0.0625f, 
			MissionTypeEnum.NoXp => 0f, 
			_ => 1f, 
		};
		ExplainedNumber xpToGain = new ExplainedNumber(num2);
		if (party != null)
		{
			GetBattleXpBonusFromPerks(party, ref xpToGain, attackerTroop);
		}
		if (captain != null && captain.IsHero && captain.GetPerkValue(DefaultPerks.Leadership.InspiringLeader))
		{
			xpToGain.AddFactor(DefaultPerks.Leadership.InspiringLeader.SecondaryBonus, DefaultPerks.Leadership.InspiringLeader.Name);
		}
		xpAmount = MathF.Round(xpToGain.ResultNumber);
	}

	public override float GetXpMultiplierFromShotDifficulty(float shotDifficulty)
	{
		if (shotDifficulty > 14.4f)
		{
			shotDifficulty = 14.4f;
		}
		return MBMath.Lerp(0f, 2f, (shotDifficulty - 1f) / 13.4f);
	}

	private void GetBattleXpBonusFromPerks(PartyBase party, ref ExplainedNumber xpToGain, CharacterObject troop)
	{
		if (party.IsMobile && party.MobileParty.LeaderHero != null)
		{
			if (!troop.IsRanged && party.MobileParty.HasPerk(DefaultPerks.OneHanded.Trainer, checkSecondaryRole: true))
			{
				xpToGain.AddFactor(DefaultPerks.OneHanded.Trainer.SecondaryBonus, DefaultPerks.OneHanded.Trainer.Name);
			}
			if (troop.HasThrowingWeapon() && party.MobileParty.HasPerk(DefaultPerks.Throwing.Resourceful, checkSecondaryRole: true))
			{
				xpToGain.AddFactor(DefaultPerks.Throwing.Resourceful.SecondaryBonus, DefaultPerks.Throwing.Resourceful.Name);
			}
			if (troop.IsInfantry)
			{
				if (party.MobileParty.HasPerk(DefaultPerks.OneHanded.CorpsACorps))
				{
					xpToGain.AddFactor(DefaultPerks.OneHanded.CorpsACorps.PrimaryBonus, DefaultPerks.OneHanded.CorpsACorps.Name);
				}
				if (party.MobileParty.HasPerk(DefaultPerks.TwoHanded.BaptisedInBlood, checkSecondaryRole: true))
				{
					xpToGain.AddFactor(DefaultPerks.TwoHanded.BaptisedInBlood.SecondaryBonus, DefaultPerks.TwoHanded.BaptisedInBlood.Name);
				}
			}
			if (party.MobileParty.HasPerk(DefaultPerks.OneHanded.LeadByExample))
			{
				xpToGain.AddFactor(DefaultPerks.OneHanded.LeadByExample.PrimaryBonus, DefaultPerks.OneHanded.LeadByExample.Name);
			}
			if (troop.IsRanged)
			{
				if (party.MobileParty.HasPerk(DefaultPerks.Crossbow.MountedCrossbowman, checkSecondaryRole: true))
				{
					xpToGain.AddFactor(DefaultPerks.Crossbow.MountedCrossbowman.SecondaryBonus, DefaultPerks.Crossbow.MountedCrossbowman.Name);
				}
				if (party.MobileParty.HasPerk(DefaultPerks.Bow.BullsEye))
				{
					xpToGain.AddFactor(DefaultPerks.Bow.BullsEye.PrimaryBonus, DefaultPerks.Bow.BullsEye.Name);
				}
			}
			if (troop.Culture.IsBandit && party.MobileParty.HasPerk(DefaultPerks.Roguery.NoRestForTheWicked))
			{
				xpToGain.AddFactor(DefaultPerks.Roguery.NoRestForTheWicked.PrimaryBonus, DefaultPerks.Roguery.NoRestForTheWicked.Name);
			}
		}
		if (party.IsMobile && party.MobileParty.IsGarrison && party.MobileParty.CurrentSettlement?.Town.Governor != null)
		{
			PerkHelper.AddPerkBonusForTown(DefaultPerks.TwoHanded.ProjectileDeflection, party.MobileParty.CurrentSettlement.Town, ref xpToGain);
			if (troop.IsMounted)
			{
				PerkHelper.AddPerkBonusForTown(DefaultPerks.Polearm.Guards, party.MobileParty.CurrentSettlement.Town, ref xpToGain);
			}
		}
	}
}
