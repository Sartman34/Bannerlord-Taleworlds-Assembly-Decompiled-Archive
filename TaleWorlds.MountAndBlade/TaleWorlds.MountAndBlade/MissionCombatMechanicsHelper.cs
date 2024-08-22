using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public static class MissionCombatMechanicsHelper
{
	private const float SpeedBonusFactorForSwing = 0.7f;

	private const float SpeedBonusFactorForThrust = 0.5f;

	public static bool DecideAgentShrugOffBlow(Agent victimAgent, AttackCollisionData collisionData, in Blow blow)
	{
		bool result = false;
		if (victimAgent.Health - (float)collisionData.InflictedDamage >= 1f)
		{
			float num = MissionGameModels.Current.AgentApplyDamageModel.CalculateStaggerThresholdDamage(victimAgent, in blow);
			result = (float)collisionData.InflictedDamage <= num;
		}
		return result;
	}

	public static bool DecideAgentDismountedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		bool flag = false;
		int inflictedDamage = collisionData.InflictedDamage;
		bool flag2 = victimAgent.Health - (float)inflictedDamage >= 1f;
		bool flag3 = (blow.BlowFlag & BlowFlags.ShrugOff) != 0;
		if (attackerWeapon != null && flag2 && !flag3)
		{
			int num = (int)victimAgent.HealthLimit;
			if (MissionGameModels.Current.AgentApplyDamageModel.CanWeaponDismount(attackerAgent, attackerWeapon, in blow, in collisionData))
			{
				float dismountPenetration = MissionGameModels.Current.AgentApplyDamageModel.GetDismountPenetration(attackerAgent, attackerWeapon, in blow, in collisionData);
				float dismountResistance = MissionGameModels.Current.AgentStatCalculateModel.GetDismountResistance(victimAgent);
				flag = DecideCombatEffect(inflictedDamage, num, dismountResistance, dismountPenetration);
			}
			if (!flag)
			{
				flag = DecideWeaponKnockDown(attackerAgent, victimAgent, attackerWeapon, in collisionData, in blow);
			}
		}
		return flag;
	}

	public static bool DecideAgentKnockedBackByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		bool result = false;
		int num = (int)victimAgent.HealthLimit;
		int inflictedDamage = collisionData.InflictedDamage;
		bool flag = (blow.BlowFlag & BlowFlags.ShrugOff) != 0;
		if (collisionData.IsHorseCharge)
		{
			Vec3 victimPosition = victimAgent.Position;
			Vec2 chargerMovementDirection = attackerAgent.GetMovementDirection();
			Vec3 collisionPoint = collisionData.CollisionGlobalPosition;
			if (ChargeDamageDotProduct(in victimPosition, in chargerMovementDirection, in collisionPoint) >= 0.7f)
			{
				result = true;
			}
		}
		else if (collisionData.IsAlternativeAttack)
		{
			result = true;
		}
		else if (attackerWeapon != null && !flag && MissionGameModels.Current.AgentApplyDamageModel.CanWeaponKnockback(attackerAgent, attackerWeapon, in blow, in collisionData))
		{
			float knockBackPenetration = MissionGameModels.Current.AgentApplyDamageModel.GetKnockBackPenetration(attackerAgent, attackerWeapon, in blow, in collisionData);
			float knockBackResistance = MissionGameModels.Current.AgentStatCalculateModel.GetKnockBackResistance(victimAgent);
			result = DecideCombatEffect(inflictedDamage, num, knockBackResistance, knockBackPenetration);
		}
		return result;
	}

	public static bool DecideAgentKnockedDownByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		bool result = false;
		if ((blow.BlowFlag & BlowFlags.ShrugOff) == 0)
		{
			int num = (int)victimAgent.HealthLimit;
			float inflictedDamage = collisionData.InflictedDamage;
			bool flag = (blow.BlowFlag & BlowFlags.KnockBack) != 0;
			if (collisionData.IsHorseCharge && flag)
			{
				float horseChargePenetration = MissionGameModels.Current.AgentApplyDamageModel.GetHorseChargePenetration();
				float knockDownResistance = MissionGameModels.Current.AgentStatCalculateModel.GetKnockDownResistance(victimAgent);
				result = DecideCombatEffect(inflictedDamage, num, knockDownResistance, horseChargePenetration);
			}
			else if (attackerWeapon != null)
			{
				result = DecideWeaponKnockDown(attackerAgent, victimAgent, attackerWeapon, in collisionData, in blow);
			}
		}
		return result;
	}

	public static bool DecideMountRearedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		float damageMultiplierOfCombatDifficulty = Mission.Current.GetDamageMultiplierOfCombatDifficulty(victimAgent, attackerAgent);
		if (attackerWeapon != null && attackerWeapon.WeaponFlags.HasAnyFlag(WeaponFlags.WideGrip) && attackerWeapon.WeaponLength > 120 && blow.StrikeType == StrikeType.Thrust && collisionData.ThrustTipHit && attackerAgent != null && !attackerAgent.HasMount && victimAgent.GetAgentFlags().HasAnyFlag(AgentFlag.CanRear) && victimAgent.MovementVelocity.y > 5f && Vec3.DotProduct(blow.Direction, victimAgent.Frame.rotation.f) < -0.35f && Vec2.DotProduct(blow.GlobalPosition.AsVec2 - victimAgent.Position.AsVec2, victimAgent.GetMovementDirection()) > 0f)
		{
			return (float)collisionData.InflictedDamage >= ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.MakesRearAttackDamageThreshold) * damageMultiplierOfCombatDifficulty;
		}
		return false;
	}

	public static bool IsCollisionBoneDifferentThanWeaponAttachBone(in AttackCollisionData collisionData, int weaponAttachBoneIndex)
	{
		if (collisionData.AttackBoneIndex != -1 && weaponAttachBoneIndex != -1)
		{
			return weaponAttachBoneIndex != collisionData.AttackBoneIndex;
		}
		return false;
	}

	public static bool DecideSweetSpotCollision(in AttackCollisionData collisionData)
	{
		if (collisionData.AttackProgress >= 0.22f)
		{
			return collisionData.AttackProgress <= 0.55f;
		}
		return false;
	}

	public static void GetAttackCollisionResults(in AttackInformation attackInformation, bool crushedThrough, float momentumRemaining, in MissionWeapon attackerWeapon, bool cancelDamage, ref AttackCollisionData attackCollisionData, out CombatLogData combatLog, out int speedBonus)
	{
		float distance = 0f;
		if (attackCollisionData.IsMissile)
		{
			distance = (attackCollisionData.MissileStartingPosition - attackCollisionData.CollisionGlobalPosition).Length;
		}
		combatLog = new CombatLogData(attackInformation.IsVictimAgentSameWithAttackerAgent, attackInformation.IsAttackerAgentHuman, attackInformation.IsAttackerAgentMine, attackInformation.DoesAttackerHaveRiderAgent, attackInformation.IsAttackerAgentRiderAgentMine, attackInformation.IsAttackerAgentMount, attackInformation.IsVictimAgentHuman, attackInformation.IsVictimAgentMine, isVictimAgentDead: false, attackInformation.DoesVictimHaveRiderAgent, attackInformation.IsVictimAgentRiderAgentMine, attackInformation.IsVictimAgentMount, isVictimEntity: false, attackInformation.IsVictimRiderAgentSameAsAttackerAgent, crushedThrough: false, chamber: false, distance);
		bool flag = IsCollisionBoneDifferentThanWeaponAttachBone(in attackCollisionData, attackInformation.WeaponAttachBoneIndex);
		Vec2 agentVelocityContribution = GetAgentVelocityContribution(attackInformation.DoesAttackerHaveMountAgent, attackInformation.AttackerAgentMovementVelocity, attackInformation.AttackerAgentMountMovementDirection, attackInformation.AttackerMovementDirectionAsAngle);
		Vec2 agentVelocityContribution2 = GetAgentVelocityContribution(attackInformation.DoesVictimHaveMountAgent, attackInformation.VictimAgentMovementVelocity, attackInformation.VictimAgentMountMovementDirection, attackInformation.VictimMovementDirectionAsAngle);
		if (attackCollisionData.IsColliderAgent)
		{
			combatLog.IsRangedAttack = attackCollisionData.IsMissile;
			combatLog.HitSpeed = (attackCollisionData.IsMissile ? (agentVelocityContribution2.ToVec3() - attackCollisionData.MissileVelocity).Length : (agentVelocityContribution - agentVelocityContribution2).Length);
		}
		ComputeBlowMagnitude(in attackCollisionData, in attackInformation, attackerWeapon, momentumRemaining, cancelDamage, flag, agentVelocityContribution, agentVelocityContribution2, out attackCollisionData.BaseMagnitude, out var specialMagnitude, out attackCollisionData.MovementSpeedDamageModifier, out speedBonus);
		DamageTypes damageType = (combatLog.DamageType = ((attackerWeapon.IsEmpty || flag || attackCollisionData.IsAlternativeAttack || attackCollisionData.IsFallDamage || attackCollisionData.IsHorseCharge) ? DamageTypes.Blunt : ((DamageTypes)attackCollisionData.DamageType)));
		if (!attackCollisionData.IsColliderAgent && attackCollisionData.EntityExists)
		{
			string name = PhysicsMaterial.GetFromIndex(attackCollisionData.PhysicsMaterialIndex).Name;
			bool isWoodenBody = name == "wood" || name == "wood_weapon" || name == "wood_shield";
			attackCollisionData.BaseMagnitude *= GetEntityDamageMultiplier(attackInformation.IsAttackerAgentDoingPassiveAttack, attackerWeapon.CurrentUsageItem, damageType, isWoodenBody);
			attackCollisionData.InflictedDamage = MBMath.ClampInt((int)attackCollisionData.BaseMagnitude, 0, 2000);
			combatLog.InflictedDamage = attackCollisionData.InflictedDamage;
		}
		if (attackCollisionData.IsColliderAgent && !attackInformation.IsVictimAgentNull)
		{
			if (attackCollisionData.IsAlternativeAttack)
			{
				specialMagnitude = attackCollisionData.BaseMagnitude;
			}
			if (attackCollisionData.AttackBlockedWithShield)
			{
				ComputeBlowDamageOnShield(in attackInformation, in attackCollisionData, attackerWeapon.CurrentUsageItem, attackCollisionData.BaseMagnitude, out attackCollisionData.InflictedDamage);
				attackCollisionData.AbsorbedByArmor = attackCollisionData.InflictedDamage;
			}
			else if (attackCollisionData.MissileBlockedWithWeapon)
			{
				attackCollisionData.InflictedDamage = 0;
				attackCollisionData.AbsorbedByArmor = 0;
			}
			else
			{
				ComputeBlowDamage(in attackInformation, in attackCollisionData, attackerWeapon.CurrentUsageItem, damageType, specialMagnitude, speedBonus, cancelDamage, out attackCollisionData.InflictedDamage, out attackCollisionData.AbsorbedByArmor);
			}
			combatLog.InflictedDamage = attackCollisionData.InflictedDamage;
			combatLog.AbsorbedDamage = attackCollisionData.AbsorbedByArmor;
			combatLog.AttackProgress = attackCollisionData.AttackProgress;
		}
	}

	internal static void GetDefendCollisionResults(Agent attackerAgent, Agent defenderAgent, CombatCollisionResult collisionResult, int attackerWeaponSlotIndex, bool isAlternativeAttack, StrikeType strikeType, Agent.UsageDirection attackDirection, float collisionDistanceOnWeapon, float attackProgress, bool attackIsParried, bool isPassiveUsageHit, bool isHeavyAttack, ref float defenderStunPeriod, ref float attackerStunPeriod, ref bool crushedThrough, ref bool chamber)
	{
		MissionWeapon missionWeapon = ((attackerWeaponSlotIndex >= 0) ? attackerAgent.Equipment[attackerWeaponSlotIndex] : MissionWeapon.Invalid);
		WeaponComponentData weaponComponentData = (missionWeapon.IsEmpty ? null : missionWeapon.CurrentUsageItem);
		EquipmentIndex wieldedItemIndex = defenderAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
		if (wieldedItemIndex == EquipmentIndex.None)
		{
			wieldedItemIndex = defenderAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
		}
		ItemObject itemObject = ((wieldedItemIndex != EquipmentIndex.None) ? defenderAgent.Equipment[wieldedItemIndex].Item : null);
		WeaponComponentData weaponComponentData2 = ((wieldedItemIndex != EquipmentIndex.None) ? defenderAgent.Equipment[wieldedItemIndex].CurrentUsageItem : null);
		float num = 10f;
		attackerStunPeriod = ((strikeType == StrikeType.Thrust) ? ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.StunPeriodAttackerThrust) : ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.StunPeriodAttackerSwing));
		chamber = false;
		float num2 = 0f;
		if (!missionWeapon.IsEmpty)
		{
			float z = attackerAgent.GetCurWeaponOffset().z;
			float realWeaponLength = weaponComponentData.GetRealWeaponLength();
			float num3 = realWeaponLength + z;
			num2 = MBMath.ClampFloat((0.2f + collisionDistanceOnWeapon) / num3, 0.1f, 0.98f);
			float exraLinearSpeed = ComputeRelativeSpeedDiffOfAgents(attackerAgent, defenderAgent);
			float num4 = ((strikeType != StrikeType.Thrust) ? CombatStatCalculator.CalculateBaseBlowMagnitudeForSwing((float)missionWeapon.GetModifiedSwingSpeedForCurrentUsage() / 4.5454545f * SpeedGraphFunction(attackProgress, strikeType, attackDirection), realWeaponLength, missionWeapon.Item.Weight, weaponComponentData.Inertia, weaponComponentData.CenterOfMass, num2, exraLinearSpeed) : CombatStatCalculator.CalculateBaseBlowMagnitudeForThrust((float)missionWeapon.GetModifiedThrustSpeedForCurrentUsage() / 11.764706f * SpeedGraphFunction(attackProgress, strikeType, attackDirection), missionWeapon.Item.Weight, exraLinearSpeed));
			if (strikeType == StrikeType.Thrust)
			{
				num4 *= 0.8f;
			}
			else if (attackDirection == Agent.UsageDirection.AttackUp)
			{
				num4 *= 1.25f;
			}
			else if (isHeavyAttack)
			{
				num4 *= ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.HeavyAttackMomentumMultiplier);
			}
			num += num4;
		}
		float num5 = 1f;
		defenderStunPeriod = num * ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.StunMomentumTransferFactor);
		if (weaponComponentData2 != null)
		{
			if (weaponComponentData2.IsShield)
			{
				float managedParameter = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.StunDefendWeaponWeightOffsetShield);
				num5 += managedParameter * itemObject.Weight;
			}
			else
			{
				num5 = 0.9f;
				float managedParameter2 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.StunDefendWeaponWeightMultiplierWeaponWeight);
				num5 += managedParameter2 * itemObject.Weight;
				switch (itemObject.ItemType)
				{
				case ItemObject.ItemTypeEnum.TwoHandedWeapon:
					managedParameter2 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.StunDefendWeaponWeightBonusTwoHanded);
					break;
				case ItemObject.ItemTypeEnum.Polearm:
					num5 += ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.StunDefendWeaponWeightBonusPolearm);
					break;
				}
			}
			switch (collisionResult)
			{
			case CombatCollisionResult.Parried:
				attackerStunPeriod += TaleWorlds.Library.MathF.Min(0.15f, 0.12f * num5);
				num5 += ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.StunDefendWeaponWeightBonusActiveBlocked);
				break;
			case CombatCollisionResult.ChamberBlocked:
				attackerStunPeriod += TaleWorlds.Library.MathF.Min(0.25f, 0.25f * num5);
				num5 += ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.StunDefendWeaponWeightBonusChamberBlocked);
				chamber = true;
				break;
			}
		}
		if (!defenderAgent.GetIsLeftStance())
		{
			num5 += ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.StunDefendWeaponWeightBonusRightStance);
		}
		defenderStunPeriod /= num5;
		MissionGameModels.Current.AgentApplyDamageModel.CalculateDefendedBlowStunMultipliers(attackerAgent, defenderAgent, collisionResult, weaponComponentData, weaponComponentData2, out var attackerStunMultiplier, out var defenderStunMultiplier);
		attackerStunPeriod *= attackerStunMultiplier;
		defenderStunPeriod *= defenderStunMultiplier;
		float managedParameter3 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.StunPeriodMax);
		attackerStunPeriod = TaleWorlds.Library.MathF.Min(attackerStunPeriod, managedParameter3);
		defenderStunPeriod = TaleWorlds.Library.MathF.Min(defenderStunPeriod, managedParameter3);
		crushedThrough = !chamber && MissionGameModels.Current.AgentApplyDamageModel.DecideCrushedThrough(attackerAgent, defenderAgent, num, attackDirection, strikeType, weaponComponentData2, isPassiveUsageHit);
	}

	private static bool DecideWeaponKnockDown(Agent attackerAgent, Agent victimAgent, WeaponComponentData attackerWeapon, in AttackCollisionData collisionData, in Blow blow)
	{
		if (MissionGameModels.Current.AgentApplyDamageModel.CanWeaponKnockDown(attackerAgent, victimAgent, attackerWeapon, in blow, in collisionData))
		{
			float knockDownPenetration = MissionGameModels.Current.AgentApplyDamageModel.GetKnockDownPenetration(attackerAgent, attackerWeapon, in blow, in collisionData);
			float knockDownResistance = MissionGameModels.Current.AgentStatCalculateModel.GetKnockDownResistance(victimAgent, blow.StrikeType);
			return DecideCombatEffect(collisionData.InflictedDamage, victimAgent.HealthLimit, knockDownResistance, knockDownPenetration);
		}
		return false;
	}

	private static bool DecideCombatEffect(float inflictedDamage, float victimMaxHealth, float victimResistance, float attackPenetration)
	{
		float num = victimMaxHealth * Math.Max(0f, victimResistance - attackPenetration);
		return inflictedDamage >= num;
	}

	private static float ChargeDamageDotProduct(in Vec3 victimPosition, in Vec2 chargerMovementDirection, in Vec3 collisionPoint)
	{
		float b = Vec2.DotProduct((victimPosition.AsVec2 - collisionPoint.AsVec2).Normalized(), chargerMovementDirection);
		return TaleWorlds.Library.MathF.Max(0f, b);
	}

	private static float SpeedGraphFunction(float progress, StrikeType strikeType, Agent.UsageDirection attackDir)
	{
		bool num = strikeType == StrikeType.Thrust;
		bool flag = attackDir == Agent.UsageDirection.AttackUp;
		ManagedParametersEnum managedParameterEnum;
		ManagedParametersEnum managedParameterEnum2;
		ManagedParametersEnum managedParameterEnum3;
		ManagedParametersEnum managedParameterEnum4;
		if (num)
		{
			managedParameterEnum = ManagedParametersEnum.ThrustCombatSpeedGraphZeroProgressValue;
			managedParameterEnum2 = ManagedParametersEnum.ThrustCombatSpeedGraphFirstMaximumPoint;
			managedParameterEnum3 = ManagedParametersEnum.ThrustCombatSpeedGraphSecondMaximumPoint;
			managedParameterEnum4 = ManagedParametersEnum.ThrustCombatSpeedGraphOneProgressValue;
		}
		else if (flag)
		{
			managedParameterEnum = ManagedParametersEnum.OverSwingCombatSpeedGraphZeroProgressValue;
			managedParameterEnum2 = ManagedParametersEnum.OverSwingCombatSpeedGraphFirstMaximumPoint;
			managedParameterEnum3 = ManagedParametersEnum.OverSwingCombatSpeedGraphSecondMaximumPoint;
			managedParameterEnum4 = ManagedParametersEnum.OverSwingCombatSpeedGraphOneProgressValue;
		}
		else
		{
			managedParameterEnum = ManagedParametersEnum.SwingCombatSpeedGraphZeroProgressValue;
			managedParameterEnum2 = ManagedParametersEnum.SwingCombatSpeedGraphFirstMaximumPoint;
			managedParameterEnum3 = ManagedParametersEnum.SwingCombatSpeedGraphSecondMaximumPoint;
			managedParameterEnum4 = ManagedParametersEnum.SwingCombatSpeedGraphOneProgressValue;
		}
		float managedParameter = ManagedParameters.Instance.GetManagedParameter(managedParameterEnum);
		float managedParameter2 = ManagedParameters.Instance.GetManagedParameter(managedParameterEnum2);
		float managedParameter3 = ManagedParameters.Instance.GetManagedParameter(managedParameterEnum3);
		float managedParameter4 = ManagedParameters.Instance.GetManagedParameter(managedParameterEnum4);
		if (progress < managedParameter2)
		{
			return (1f - managedParameter) / managedParameter2 * progress + managedParameter;
		}
		if (managedParameter3 < progress)
		{
			return (managedParameter4 - 1f) / (1f - managedParameter3) * (progress - managedParameter3) + 1f;
		}
		return 1f;
	}

	private static float ConvertBaseAttackMagnitude(WeaponComponentData weapon, StrikeType strikeType, float baseMagnitude)
	{
		return baseMagnitude * ((strikeType == StrikeType.Thrust) ? weapon.ThrustDamageFactor : weapon.SwingDamageFactor);
	}

	private static Vec2 GetAgentVelocityContribution(bool hasAgentMountAgent, Vec2 agentMovementVelocity, Vec2 agentMountMovementDirection, float agentMovementDirectionAsAngle)
	{
		Vec2 zero = Vec2.Zero;
		if (hasAgentMountAgent)
		{
			zero = agentMovementVelocity.y * agentMountMovementDirection;
		}
		else
		{
			zero = agentMovementVelocity;
			zero.RotateCCW(agentMovementDirectionAsAngle);
		}
		return zero;
	}

	private static float GetEntityDamageMultiplier(bool isAttackerAgentDoingPassiveAttack, WeaponComponentData weapon, DamageTypes damageType, bool isWoodenBody)
	{
		float num = 1f;
		if (isAttackerAgentDoingPassiveAttack)
		{
			num *= 0.2f;
		}
		if (weapon != null)
		{
			if (weapon.WeaponFlags.HasAnyFlag(WeaponFlags.BonusAgainstShield))
			{
				num *= 1.2f;
			}
			switch (damageType)
			{
			case DamageTypes.Cut:
				num *= 0.8f;
				break;
			case DamageTypes.Pierce:
				num *= 0.1f;
				break;
			}
			if (isWoodenBody && weapon.WeaponFlags.HasAnyFlag(WeaponFlags.Burning))
			{
				num *= 1.5f;
			}
		}
		return num;
	}

	private static float ComputeSpeedBonus(float baseMagnitude, float baseMagnitudeWithoutSpeedBonus)
	{
		return baseMagnitude / baseMagnitudeWithoutSpeedBonus - 1f;
	}

	private static float ComputeRelativeSpeedDiffOfAgents(Agent agentA, Agent agentB)
	{
		Vec2 zero = Vec2.Zero;
		if (agentA.MountAgent != null)
		{
			zero = agentA.MountAgent.MovementVelocity.y * agentA.MountAgent.GetMovementDirection();
		}
		else
		{
			zero = agentA.MovementVelocity;
			zero.RotateCCW(agentA.MovementDirectionAsAngle);
		}
		Vec2 zero2 = Vec2.Zero;
		if (agentB.MountAgent != null)
		{
			zero2 = agentB.MountAgent.MovementVelocity.y * agentB.MountAgent.GetMovementDirection();
		}
		else
		{
			zero2 = agentB.MovementVelocity;
			zero2.RotateCCW(agentB.MovementDirectionAsAngle);
		}
		return (zero - zero2).Length;
	}

	private static void ComputeBlowDamage(in AttackInformation attackInformation, in AttackCollisionData attackCollisionData, WeaponComponentData attackerWeapon, DamageTypes damageType, float magnitude, int speedBonus, bool cancelDamage, out int inflictedDamage, out int absorbedByArmor)
	{
		float armorAmountFloat = attackInformation.ArmorAmountFloat;
		WeaponComponentData shieldOnBack = attackInformation.ShieldOnBack;
		AgentFlag victimAgentFlag = attackInformation.VictimAgentFlag;
		float victimAgentAbsorbedDamageRatio = attackInformation.VictimAgentAbsorbedDamageRatio;
		float damageMultiplierOfBone = attackInformation.DamageMultiplierOfBone;
		float combatDifficultyMultiplier = attackInformation.CombatDifficultyMultiplier;
		_ = attackCollisionData.CollisionGlobalPosition;
		bool attackBlockedWithShield = attackCollisionData.AttackBlockedWithShield;
		bool collidedWithShieldOnBack = attackCollisionData.CollidedWithShieldOnBack;
		bool isFallDamage = attackCollisionData.IsFallDamage;
		BasicCharacterObject attackerAgentCharacter = attackInformation.AttackerAgentCharacter;
		BasicCharacterObject attackerCaptainCharacter = attackInformation.AttackerCaptainCharacter;
		BasicCharacterObject victimAgentCharacter = attackInformation.VictimAgentCharacter;
		BasicCharacterObject victimCaptainCharacter = attackInformation.VictimCaptainCharacter;
		float num = 0f;
		if (!isFallDamage)
		{
			num = MissionGameModels.Current.StrikeMagnitudeModel.CalculateAdjustedArmorForBlow(armorAmountFloat, attackerAgentCharacter, attackerCaptainCharacter, victimAgentCharacter, victimCaptainCharacter, attackerWeapon);
		}
		if (collidedWithShieldOnBack && shieldOnBack != null)
		{
			num += 10f;
		}
		float absorbedDamageRatio = victimAgentAbsorbedDamageRatio;
		float num2 = MissionGameModels.Current.StrikeMagnitudeModel.ComputeRawDamage(damageType, magnitude, num, absorbedDamageRatio);
		float num3 = 1f;
		if (!attackBlockedWithShield && !isFallDamage)
		{
			num3 *= damageMultiplierOfBone;
			num3 *= combatDifficultyMultiplier;
		}
		num2 *= num3;
		inflictedDamage = MBMath.ClampInt(TaleWorlds.Library.MathF.Ceiling(num2), 0, 2000);
		int num4 = MBMath.ClampInt(TaleWorlds.Library.MathF.Ceiling(MissionGameModels.Current.StrikeMagnitudeModel.ComputeRawDamage(damageType, magnitude, 0f, absorbedDamageRatio) * num3), 0, 2000);
		absorbedByArmor = num4 - inflictedDamage;
	}

	private static void ComputeBlowDamageOnShield(in AttackInformation attackInformation, in AttackCollisionData attackCollisionData, WeaponComponentData attackerWeapon, float blowMagnitude, out int inflictedDamage)
	{
		inflictedDamage = 0;
		MissionWeapon victimShield = attackInformation.VictimShield;
		if (!victimShield.CurrentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.CanBlockRanged) || !attackInformation.CanGiveDamageToAgentShield)
		{
			return;
		}
		DamageTypes damageType = (DamageTypes)attackCollisionData.DamageType;
		int getModifiedArmorForCurrentUsage = victimShield.GetGetModifiedArmorForCurrentUsage();
		float absorbedDamageRatio = 1f;
		float num = MissionGameModels.Current.StrikeMagnitudeModel.ComputeRawDamage(damageType, blowMagnitude, getModifiedArmorForCurrentUsage, absorbedDamageRatio);
		if (attackCollisionData.IsMissile)
		{
			num = ((attackerWeapon.WeaponClass == WeaponClass.ThrowingAxe) ? (num * 0.3f) : ((attackerWeapon.WeaponClass == WeaponClass.Javelin) ? (num * 0.5f) : ((!attackerWeapon.WeaponFlags.HasAnyFlag(WeaponFlags.CanPenetrateShield) || !attackerWeapon.WeaponFlags.HasAnyFlag(WeaponFlags.MultiplePenetration)) ? (num * 0.15f) : (num * 0.5f))));
		}
		else
		{
			switch (attackCollisionData.DamageType)
			{
			case 1:
				num *= 0.5f;
				break;
			case 0:
			case 2:
				num *= 0.7f;
				break;
			}
		}
		if (attackerWeapon != null && attackerWeapon.WeaponFlags.HasAnyFlag(WeaponFlags.BonusAgainstShield))
		{
			num *= 2f;
		}
		if (num > 0f)
		{
			if (!attackInformation.IsVictimAgentLeftStance)
			{
				num *= ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.ShieldRightStanceBlockDamageMultiplier);
			}
			if (attackCollisionData.CorrectSideShieldBlock)
			{
				num *= ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.ShieldCorrectSideBlockDamageMultiplier);
			}
			num = MissionGameModels.Current.AgentApplyDamageModel.CalculateShieldDamage(in attackInformation, num);
			inflictedDamage = (int)num;
		}
	}

	public static float CalculateBaseMeleeBlowMagnitude(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, StrikeType strikeType, float progressEffect, float impactPointAsPercent, float exraLinearSpeed)
	{
		WeaponComponentData currentUsageItem = weapon.CurrentUsageItem;
		float num = TaleWorlds.Library.MathF.Sqrt(progressEffect);
		if (strikeType == StrikeType.Thrust)
		{
			exraLinearSpeed *= 0.5f;
			float thrustSpeed = (float)weapon.GetModifiedThrustSpeedForCurrentUsage() / 11.764706f * num;
			return MissionGameModels.Current.StrikeMagnitudeModel.CalculateStrikeMagnitudeForThrust(in attackInformation, in collisionData, in weapon, thrustSpeed, exraLinearSpeed);
		}
		exraLinearSpeed *= 0.7f;
		float swingSpeed = (float)weapon.GetModifiedSwingSpeedForCurrentUsage() / 4.5454545f * num;
		float num2 = MBMath.ClampFloat(0.4f / currentUsageItem.GetRealWeaponLength(), 0f, 1f);
		float num3 = TaleWorlds.Library.MathF.Min(0.93f, impactPointAsPercent);
		float num4 = TaleWorlds.Library.MathF.Min(0.93f, impactPointAsPercent + num2);
		float num5 = 0f;
		for (int i = 0; i < 5; i++)
		{
			float impactPointAsPercent2 = num3 + (float)i / 4f * (num4 - num3);
			float num6 = MissionGameModels.Current.StrikeMagnitudeModel.CalculateStrikeMagnitudeForSwing(in attackInformation, in collisionData, in weapon, swingSpeed, impactPointAsPercent2, exraLinearSpeed);
			if (num5 < num6)
			{
				num5 = num6;
			}
		}
		return num5;
	}

	private static void ComputeBlowMagnitude(in AttackCollisionData acd, in AttackInformation attackInformation, MissionWeapon weapon, float momentumRemaining, bool cancelDamage, bool hitWithAnotherBone, Vec2 attackerVelocity, Vec2 victimVelocity, out float baseMagnitude, out float specialMagnitude, out float movementSpeedDamageModifier, out int speedBonusInt)
	{
		StrikeType strikeType = (StrikeType)acd.StrikeType;
		Agent.UsageDirection attackDirection = acd.AttackDirection;
		bool attackerIsDoingPassiveAttack = !attackInformation.IsAttackerAgentNull && attackInformation.IsAttackerAgentHuman && attackInformation.IsAttackerAgentActive && attackInformation.IsAttackerAgentDoingPassiveAttack;
		movementSpeedDamageModifier = 0f;
		speedBonusInt = 0;
		if (acd.IsMissile)
		{
			ComputeBlowMagnitudeMissile(in attackInformation, in acd, in weapon, momentumRemaining, in victimVelocity, out baseMagnitude, out specialMagnitude);
		}
		else if (acd.IsFallDamage)
		{
			ComputeBlowMagnitudeFromFall(in attackInformation, in acd, out baseMagnitude, out specialMagnitude);
		}
		else if (acd.IsHorseCharge)
		{
			ComputeBlowMagnitudeFromHorseCharge(in attackInformation, in acd, attackerVelocity, victimVelocity, out baseMagnitude, out specialMagnitude);
		}
		else
		{
			ComputeBlowMagnitudeMelee(in attackInformation, in acd, momentumRemaining, cancelDamage, hitWithAnotherBone, strikeType, attackDirection, in weapon, attackerIsDoingPassiveAttack, attackerVelocity, victimVelocity, out baseMagnitude, out specialMagnitude, out movementSpeedDamageModifier, out speedBonusInt);
		}
		specialMagnitude = MBMath.ClampFloat(specialMagnitude, 0f, 500f);
	}

	private static void ComputeBlowMagnitudeMelee(in AttackInformation attackInformation, in AttackCollisionData collisionData, float momentumRemaining, bool cancelDamage, bool hitWithAnotherBone, StrikeType strikeType, Agent.UsageDirection attackDirection, in MissionWeapon weapon, bool attackerIsDoingPassiveAttack, Vec2 attackerVelocity, Vec2 victimVelocity, out float baseMagnitude, out float specialMagnitude, out float movementSpeedDamageModifier, out int speedBonusInt)
	{
		Vec3 attackerAgentCurrentWeaponOffset = attackInformation.AttackerAgentCurrentWeaponOffset;
		movementSpeedDamageModifier = 0f;
		speedBonusInt = 0;
		specialMagnitude = 0f;
		baseMagnitude = 0f;
		BasicCharacterObject attackerAgentCharacter = attackInformation.AttackerAgentCharacter;
		if (collisionData.IsAlternativeAttack)
		{
			WeaponComponentData currentUsageItem = weapon.CurrentUsageItem;
			baseMagnitude = MissionGameModels.Current.AgentApplyDamageModel.CalculateAlternativeAttackDamage(attackerAgentCharacter, currentUsageItem);
			baseMagnitude *= momentumRemaining;
			specialMagnitude = baseMagnitude;
			return;
		}
		Vec3 weaponBlowDir = collisionData.WeaponBlowDir;
		Vec2 vb = attackerVelocity - victimVelocity;
		float num = vb.Normalize();
		float num2 = Vec2.DotProduct(weaponBlowDir.AsVec2, vb);
		if (num2 > 0f)
		{
			num2 += 0.2f;
			num2 = TaleWorlds.Library.MathF.Min(num2, 1f);
		}
		float num3 = num * num2;
		if (weapon.IsEmpty)
		{
			baseMagnitude = SpeedGraphFunction(collisionData.AttackProgress, strikeType, attackDirection) * momentumRemaining * ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.FistFightDamageMultiplier);
			specialMagnitude = baseMagnitude;
			return;
		}
		float z = attackerAgentCurrentWeaponOffset.z;
		WeaponComponentData currentUsageItem2 = weapon.CurrentUsageItem;
		float num4 = currentUsageItem2.GetRealWeaponLength() + z;
		float impactPointAsPercent = MBMath.ClampFloat(collisionData.CollisionDistanceOnWeapon, -0.2f, num4) / num4;
		if (attackerIsDoingPassiveAttack)
		{
			if (!attackInformation.DoesAttackerHaveMountAgent && !attackInformation.DoesVictimHaveMountAgent && !attackInformation.IsVictimAgentMount)
			{
				baseMagnitude = 0f;
			}
			else
			{
				baseMagnitude = CombatStatCalculator.CalculateBaseBlowMagnitudeForPassiveUsage(weapon.Item.Weight, num3);
			}
			baseMagnitude = MissionGameModels.Current.AgentApplyDamageModel.CalculatePassiveAttackDamage(attackerAgentCharacter, in collisionData, baseMagnitude);
		}
		else
		{
			float num5 = SpeedGraphFunction(collisionData.AttackProgress, strikeType, attackDirection);
			baseMagnitude = CalculateBaseMeleeBlowMagnitude(in attackInformation, in collisionData, in weapon, strikeType, num5, impactPointAsPercent, num3);
			if (baseMagnitude >= 0f && num5 > 0.7f)
			{
				float baseMagnitudeWithoutSpeedBonus = CalculateBaseMeleeBlowMagnitude(in attackInformation, in collisionData, in weapon, strikeType, num5, impactPointAsPercent, 0f);
				movementSpeedDamageModifier = ComputeSpeedBonus(baseMagnitude, baseMagnitudeWithoutSpeedBonus);
				speedBonusInt = TaleWorlds.Library.MathF.Round(100f * movementSpeedDamageModifier);
				speedBonusInt = MBMath.ClampInt(speedBonusInt, -1000, 1000);
			}
		}
		baseMagnitude *= momentumRemaining;
		float num6 = 1f;
		if (hitWithAnotherBone)
		{
			num6 = ((strikeType != StrikeType.Thrust) ? ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.SwingHitWithArmDamageMultiplier) : ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.ThrustHitWithArmDamageMultiplier));
		}
		else if (strikeType == StrikeType.Thrust && !collisionData.ThrustTipHit && !collisionData.AttackBlockedWithShield)
		{
			num6 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.NonTipThrustHitDamageMultiplier);
		}
		baseMagnitude *= num6;
		if (attackInformation.AttackerAgent != null)
		{
			float weaponDamageMultiplier = MissionGameModels.Current.AgentStatCalculateModel.GetWeaponDamageMultiplier(attackInformation.AttackerAgent, currentUsageItem2);
			baseMagnitude *= weaponDamageMultiplier;
		}
		specialMagnitude = ConvertBaseAttackMagnitude(currentUsageItem2, strikeType, baseMagnitude);
	}

	private static void ComputeBlowMagnitudeFromHorseCharge(in AttackInformation attackInformation, in AttackCollisionData acd, Vec2 attackerAgentVelocity, Vec2 victimAgentVelocity, out float baseMagnitude, out float specialMagnitude)
	{
		Vec2 chargerMovementDirection = attackInformation.AttackerAgentMovementDirection;
		Vec2 vec = chargerMovementDirection * Vec2.DotProduct(victimAgentVelocity, chargerMovementDirection);
		Vec2 vec2 = attackerAgentVelocity - vec;
		ref readonly Vec3 victimAgentPosition = ref attackInformation.VictimAgentPosition;
		Vec3 collisionPoint = acd.CollisionGlobalPosition;
		float num = ChargeDamageDotProduct(in victimAgentPosition, in chargerMovementDirection, in collisionPoint);
		float num2 = vec2.Length * num;
		baseMagnitude = num2 * num2 * num * attackInformation.AttackerAgentMountChargeDamageProperty;
		specialMagnitude = baseMagnitude;
	}

	private static void ComputeBlowMagnitudeMissile(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float momentumRemaining, in Vec2 victimVelocity, out float baseMagnitude, out float specialMagnitude)
	{
		float missileSpeed = (attackInformation.IsVictimAgentNull ? collisionData.MissileVelocity.Length : (victimVelocity.ToVec3() - collisionData.MissileVelocity).Length);
		baseMagnitude = MissionGameModels.Current.StrikeMagnitudeModel.CalculateStrikeMagnitudeForMissile(in attackInformation, in collisionData, in weapon, missileSpeed);
		baseMagnitude *= momentumRemaining;
		if (attackInformation.AttackerAgent != null)
		{
			float weaponDamageMultiplier = MissionGameModels.Current.AgentStatCalculateModel.GetWeaponDamageMultiplier(attackInformation.AttackerAgent, weapon.CurrentUsageItem);
			baseMagnitude *= weaponDamageMultiplier;
		}
		specialMagnitude = baseMagnitude;
	}

	private static void ComputeBlowMagnitudeFromFall(in AttackInformation attackInformation, in AttackCollisionData acd, out float baseMagnitude, out float specialMagnitude)
	{
		float victimAgentScale = attackInformation.VictimAgentScale;
		float num = attackInformation.VictimAgentWeight * victimAgentScale * victimAgentScale;
		float num2 = TaleWorlds.Library.MathF.Sqrt(1f + attackInformation.VictimAgentTotalEncumbrance / num);
		float num3 = 0f - acd.VictimAgentCurVelocity.z;
		if (attackInformation.DoesVictimHaveMountAgent)
		{
			float managedParameter = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.FallSpeedReductionMultiplierForRiderDamage);
			num3 *= managedParameter;
		}
		float num4 = ((!attackInformation.IsVictimAgentHuman) ? 1.41f : 1f);
		float managedParameter2 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.FallDamageMultiplier);
		float managedParameter3 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.FallDamageAbsorption);
		baseMagnitude = (num3 * num3 * managedParameter2 - managedParameter3) * num2 * num4;
		baseMagnitude = MBMath.ClampFloat(baseMagnitude, 0f, 499.9f);
		specialMagnitude = baseMagnitude;
	}
}
