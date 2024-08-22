using System;
using MBHelpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace TaleWorlds.MountAndBlade;

public class CustomAgentApplyDamageModel : AgentApplyDamageModel
{
	private const float SallyOutSiegeEngineDamageMultiplier = 4.5f;

	public override float CalculateDamage(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float baseDamage)
	{
		BasicCharacterObject obj = (attackInformation.IsAttackerAgentMount ? attackInformation.AttackerRiderAgentCharacter : attackInformation.AttackerAgentCharacter);
		Formation attackerFormation = attackInformation.AttackerFormation;
		BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(attackerFormation);
		BasicCharacterObject basicCharacterObject = (attackInformation.IsVictimAgentMount ? attackInformation.VictimRiderAgentCharacter : attackInformation.VictimAgentCharacter);
		Formation victimFormation = attackInformation.VictimFormation;
		BannerComponent activeBanner2 = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(victimFormation);
		float num = 0f;
		FactoredNumber bonuses = new FactoredNumber(baseDamage);
		WeaponComponentData currentUsageItem = weapon.CurrentUsageItem;
		if (obj != null)
		{
			if (currentUsageItem != null)
			{
				if (currentUsageItem.IsMeleeWeapon)
				{
					if (activeBanner != null)
					{
						BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedMeleeDamage, activeBanner, ref bonuses);
						if (attackInformation.DoesVictimHaveMountAgent)
						{
							BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedMeleeDamageAgainstMountedTroops, activeBanner, ref bonuses);
						}
					}
				}
				else if (currentUsageItem.IsConsumable && activeBanner != null)
				{
					BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedRangedDamage, activeBanner, ref bonuses);
				}
			}
			if (collisionData.IsHorseCharge && activeBanner != null)
			{
				BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedChargeDamage, activeBanner, ref bonuses);
			}
		}
		float num2 = 1f;
		if (Mission.Current.IsSallyOutBattle)
		{
			DestructableComponent hitObjectDestructibleComponent = attackInformation.HitObjectDestructibleComponent;
			if (hitObjectDestructibleComponent != null && hitObjectDestructibleComponent.GameEntity.GetFirstScriptOfType<SiegeWeapon>() != null)
			{
				num2 *= 4.5f;
			}
		}
		bonuses = new FactoredNumber(bonuses.ResultNumber * num2);
		if (basicCharacterObject != null && currentUsageItem != null)
		{
			if (currentUsageItem.IsConsumable)
			{
				if (activeBanner2 != null)
				{
					BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedRangedAttackDamage, activeBanner2, ref bonuses);
				}
			}
			else if (currentUsageItem.IsMeleeWeapon && activeBanner2 != null)
			{
				BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedMeleeAttackDamage, activeBanner2, ref bonuses);
			}
		}
		num = bonuses.ResultNumber;
		return TaleWorlds.Library.MathF.Max(0f, num);
	}

	public override void DecideMissileWeaponFlags(Agent attackerAgent, MissionWeapon missileWeapon, ref WeaponFlags missileWeaponFlags)
	{
	}

	public override bool DecideCrushedThrough(Agent attackerAgent, Agent defenderAgent, float totalAttackEnergy, Agent.UsageDirection attackDirection, StrikeType strikeType, WeaponComponentData defendItem, bool isPassiveUsage)
	{
		EquipmentIndex wieldedItemIndex = attackerAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
		if (wieldedItemIndex == EquipmentIndex.None)
		{
			wieldedItemIndex = attackerAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
		}
		WeaponComponentData weaponComponentData = ((wieldedItemIndex != EquipmentIndex.None) ? attackerAgent.Equipment[wieldedItemIndex].CurrentUsageItem : null);
		if (weaponComponentData == null || isPassiveUsage || !weaponComponentData.WeaponFlags.HasAnyFlag(WeaponFlags.CanCrushThrough) || strikeType != 0 || attackDirection != 0)
		{
			return false;
		}
		float num = 58f;
		if (defendItem != null && defendItem.IsShield)
		{
			num *= 1.2f;
		}
		return totalAttackEnergy > num;
	}

	public override bool CanWeaponDismount(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		if (!MBMath.IsBetween((int)blow.VictimBodyPart, 0, 6))
		{
			return false;
		}
		if (!attackerAgent.HasMount && blow.StrikeType == StrikeType.Swing && blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.CanHook))
		{
			return true;
		}
		if (blow.StrikeType == StrikeType.Thrust)
		{
			return blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.CanDismount);
		}
		return false;
	}

	public override void CalculateDefendedBlowStunMultipliers(Agent attackerAgent, Agent defenderAgent, CombatCollisionResult collisionResult, WeaponComponentData attackerWeapon, WeaponComponentData defenderWeapon, out float attackerStunMultiplier, out float defenderStunMultiplier)
	{
		attackerStunMultiplier = 1f;
		defenderStunMultiplier = 1f;
	}

	public override bool CanWeaponKnockback(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		if (MBMath.IsBetween((int)collisionData.VictimHitBodyPart, 0, 6) && !attackerWeapon.WeaponFlags.HasAnyFlag(WeaponFlags.CanKnockDown))
		{
			if (!attackerWeapon.IsConsumable && (blow.BlowFlag & BlowFlags.CrushThrough) == 0)
			{
				if (blow.StrikeType == StrikeType.Thrust)
				{
					return blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.WideGrip);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool CanWeaponKnockDown(Agent attackerAgent, Agent victimAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		if (attackerWeapon.WeaponClass == WeaponClass.Boulder)
		{
			return true;
		}
		BoneBodyPartType victimHitBodyPart = collisionData.VictimHitBodyPart;
		bool flag = MBMath.IsBetween((int)victimHitBodyPart, 0, 6);
		if (!victimAgent.HasMount && victimHitBodyPart == BoneBodyPartType.Legs)
		{
			flag = true;
		}
		if (flag && blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.CanKnockDown))
		{
			if (!attackerWeapon.IsPolearm || blow.StrikeType != StrikeType.Thrust)
			{
				if (attackerWeapon.IsMeleeWeapon && blow.StrikeType == StrikeType.Swing)
				{
					return MissionCombatMechanicsHelper.DecideSweetSpotCollision(in collisionData);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override float GetDismountPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		float num = 0f;
		if (blow.StrikeType == StrikeType.Swing && blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.CanHook))
		{
			num += 0.25f;
		}
		return num;
	}

	public override float GetKnockBackPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		return 0f;
	}

	public override float GetKnockDownPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		float num = 0f;
		if (attackerWeapon.WeaponClass == WeaponClass.Boulder)
		{
			num += 0.25f;
		}
		else if (attackerWeapon.IsMeleeWeapon)
		{
			if (attackCollisionData.VictimHitBodyPart == BoneBodyPartType.Legs && blow.StrikeType == StrikeType.Swing)
			{
				num += 0.1f;
			}
			else if (attackCollisionData.VictimHitBodyPart == BoneBodyPartType.Head)
			{
				num += 0.15f;
			}
		}
		return num;
	}

	public override float GetHorseChargePenetration()
	{
		return 0.4f;
	}

	public override float CalculateStaggerThresholdDamage(Agent defenderAgent, in Blow blow)
	{
		ManagedParametersEnum managedParameterEnum = ((blow.DamageType == DamageTypes.Cut) ? ManagedParametersEnum.DamageInterruptAttackThresholdCut : ((blow.DamageType != DamageTypes.Pierce) ? ManagedParametersEnum.DamageInterruptAttackThresholdBlunt : ManagedParametersEnum.DamageInterruptAttackThresholdPierce));
		return ManagedParameters.Instance.GetManagedParameter(managedParameterEnum);
	}

	public override float CalculateAlternativeAttackDamage(BasicCharacterObject attackerCharacter, WeaponComponentData weapon)
	{
		if (weapon == null)
		{
			return 2f;
		}
		if (weapon.WeaponClass == WeaponClass.LargeShield)
		{
			return 2f;
		}
		if (weapon.WeaponClass == WeaponClass.SmallShield)
		{
			return 1f;
		}
		if (weapon.IsTwoHanded)
		{
			return 2f;
		}
		return 1f;
	}

	public override float CalculatePassiveAttackDamage(BasicCharacterObject attackerCharacter, in AttackCollisionData collisionData, float baseDamage)
	{
		return baseDamage;
	}

	public override MeleeCollisionReaction DecidePassiveAttackCollisionReaction(Agent attacker, Agent defender, bool isFatalHit)
	{
		return MeleeCollisionReaction.Bounced;
	}

	public override float CalculateShieldDamage(in AttackInformation attackInformation, float baseDamage)
	{
		baseDamage *= 1.25f;
		FactoredNumber bonuses = new FactoredNumber(baseDamage);
		Formation victimFormation = attackInformation.VictimFormation;
		BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(victimFormation);
		if (activeBanner != null)
		{
			BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedShieldDamage, activeBanner, ref bonuses);
		}
		return Math.Max(0f, bonuses.ResultNumber);
	}

	public override float GetDamageMultiplierForBodyPart(BoneBodyPartType bodyPart, DamageTypes type, bool isHuman, bool isMissile)
	{
		float result = 1f;
		switch (bodyPart)
		{
		case BoneBodyPartType.None:
			result = 1f;
			break;
		case BoneBodyPartType.Head:
			switch (type)
			{
			case DamageTypes.Invalid:
				result = 1.5f;
				break;
			case DamageTypes.Cut:
				result = 1.2f;
				break;
			case DamageTypes.Pierce:
				result = ((!isHuman) ? 1.2f : (isMissile ? 2f : 1.25f));
				break;
			case DamageTypes.Blunt:
				result = 1.2f;
				break;
			}
			break;
		case BoneBodyPartType.Neck:
			switch (type)
			{
			case DamageTypes.Invalid:
				result = 1.5f;
				break;
			case DamageTypes.Cut:
				result = 1.2f;
				break;
			case DamageTypes.Pierce:
				result = ((!isHuman) ? 1.2f : (isMissile ? 2f : 1.25f));
				break;
			case DamageTypes.Blunt:
				result = 1.2f;
				break;
			}
			break;
		case BoneBodyPartType.Chest:
		case BoneBodyPartType.Abdomen:
		case BoneBodyPartType.ShoulderLeft:
		case BoneBodyPartType.ShoulderRight:
		case BoneBodyPartType.ArmLeft:
		case BoneBodyPartType.ArmRight:
			result = ((!isHuman) ? 0.8f : 1f);
			break;
		case BoneBodyPartType.Legs:
			result = 0.8f;
			break;
		}
		return result;
	}

	public override bool CanWeaponIgnoreFriendlyFireChecks(WeaponComponentData weapon)
	{
		if (weapon != null && weapon.IsConsumable && weapon.WeaponFlags.HasAnyFlag(WeaponFlags.CanPenetrateShield) && weapon.WeaponFlags.HasAnyFlag(WeaponFlags.MultiplePenetration))
		{
			return true;
		}
		return false;
	}

	public override bool DecideAgentShrugOffBlow(Agent victimAgent, AttackCollisionData collisionData, in Blow blow)
	{
		return MissionCombatMechanicsHelper.DecideAgentShrugOffBlow(victimAgent, collisionData, in blow);
	}

	public override bool DecideAgentDismountedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		return MissionCombatMechanicsHelper.DecideAgentDismountedByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon, in blow);
	}

	public override bool DecideAgentKnockedBackByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		return MissionCombatMechanicsHelper.DecideAgentKnockedBackByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon, in blow);
	}

	public override bool DecideAgentKnockedDownByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		return MissionCombatMechanicsHelper.DecideAgentKnockedDownByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon, in blow);
	}

	public override bool DecideMountRearedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		return MissionCombatMechanicsHelper.DecideMountRearedByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon, in blow);
	}
}
