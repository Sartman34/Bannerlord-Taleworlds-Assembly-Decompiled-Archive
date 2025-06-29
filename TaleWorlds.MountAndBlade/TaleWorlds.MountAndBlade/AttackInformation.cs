using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public struct AttackInformation
{
	public Agent AttackerAgent;

	public Agent VictimAgent;

	public float ArmorAmountFloat;

	public WeaponComponentData ShieldOnBack;

	public AgentFlag VictimAgentFlag;

	public float VictimAgentAbsorbedDamageRatio;

	public float DamageMultiplierOfBone;

	public float CombatDifficultyMultiplier;

	public MissionWeapon VictimMainHandWeapon;

	public MissionWeapon VictimShield;

	public bool CanGiveDamageToAgentShield;

	public bool IsVictimAgentLeftStance;

	public bool IsFriendlyFire;

	public bool DoesAttackerHaveMountAgent;

	public bool DoesVictimHaveMountAgent;

	public Vec2 AttackerAgentMovementVelocity;

	public Vec2 AttackerAgentMountMovementDirection;

	public float AttackerMovementDirectionAsAngle;

	public Vec2 VictimAgentMovementVelocity;

	public Vec2 VictimAgentMountMovementDirection;

	public float VictimMovementDirectionAsAngle;

	public bool IsVictimAgentSameWithAttackerAgent;

	public bool IsAttackerAgentMine;

	public bool DoesAttackerHaveRiderAgent;

	public bool IsAttackerAgentRiderAgentMine;

	public bool IsAttackerAgentMount;

	public bool IsVictimAgentMine;

	public bool DoesVictimHaveRiderAgent;

	public bool IsVictimAgentRiderAgentMine;

	public bool IsVictimAgentMount;

	public bool IsAttackerAgentNull;

	public bool IsAttackerAIControlled;

	public BasicCharacterObject AttackerAgentCharacter;

	public BasicCharacterObject AttackerRiderAgentCharacter;

	public IAgentOriginBase AttackerAgentOrigin;

	public IAgentOriginBase AttackerRiderAgentOrigin;

	public BasicCharacterObject VictimAgentCharacter;

	public BasicCharacterObject VictimRiderAgentCharacter;

	public IAgentOriginBase VictimAgentOrigin;

	public IAgentOriginBase VictimRiderAgentOrigin;

	public Vec2 AttackerAgentMovementDirection;

	public Vec3 AttackerAgentVelocity;

	public float AttackerAgentMountChargeDamageProperty;

	public Vec3 AttackerAgentCurrentWeaponOffset;

	public bool IsAttackerAgentHuman;

	public bool IsAttackerAgentActive;

	public bool IsAttackerAgentDoingPassiveAttack;

	public bool IsVictimAgentNull;

	public float VictimAgentScale;

	public float VictimAgentWeight;

	public float VictimAgentHealth;

	public float VictimAgentMaxHealth;

	public float VictimAgentTotalEncumbrance;

	public bool IsVictimAgentHuman;

	public Vec3 VictimAgentVelocity;

	public Vec3 VictimAgentPosition;

	public int WeaponAttachBoneIndex;

	public MissionWeapon OffHandItem;

	public bool IsHeadShot;

	public bool IsVictimRiderAgentSameAsAttackerAgent;

	public BasicCharacterObject AttackerCaptainCharacter;

	public BasicCharacterObject VictimCaptainCharacter;

	public Formation AttackerFormation;

	public Formation VictimFormation;

	public float AttackerHitPointRate;

	public float VictimHitPointRate;

	public bool IsAttackerPlayer;

	public bool IsVictimPlayer;

	public DestructableComponent HitObjectDestructibleComponent;

	public AttackInformation(Agent attackerAgent, Agent victimAgent, GameEntity hitObject, in AttackCollisionData attackCollisionData, in MissionWeapon attackerWeapon)
	{
		AttackerAgent = attackerAgent;
		VictimAgent = victimAgent;
		IsAttackerAgentNull = attackerAgent == null;
		IsVictimAgentNull = victimAgent == null;
		ArmorAmountFloat = 0f;
		if (!IsVictimAgentNull)
		{
			ArmorAmountFloat = victimAgent.GetBaseArmorEffectivenessForBodyPart(attackCollisionData.VictimHitBodyPart);
		}
		ShieldOnBack = null;
		if (!IsVictimAgentNull && (victimAgent.GetAgentFlags() & AgentFlag.CanWieldWeapon) != 0)
		{
			EquipmentIndex wieldedItemIndex = victimAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
			for (int i = 0; i < 4; i++)
			{
				WeaponComponentData currentUsageItem = victimAgent.Equipment[i].CurrentUsageItem;
				if (i != (int)wieldedItemIndex && currentUsageItem != null && currentUsageItem.IsShield)
				{
					ShieldOnBack = currentUsageItem;
					break;
				}
			}
		}
		VictimShield = MissionWeapon.Invalid;
		VictimMainHandWeapon = MissionWeapon.Invalid;
		if (!IsVictimAgentNull && (victimAgent.GetAgentFlags() & AgentFlag.CanWieldWeapon) != 0)
		{
			EquipmentIndex wieldedItemIndex2 = victimAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
			if (wieldedItemIndex2 != EquipmentIndex.None)
			{
				VictimShield = victimAgent.Equipment[wieldedItemIndex2];
			}
			EquipmentIndex wieldedItemIndex3 = victimAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
			if (wieldedItemIndex3 != EquipmentIndex.None)
			{
				VictimMainHandWeapon = victimAgent.Equipment[wieldedItemIndex3];
			}
		}
		AttackerAgentMountMovementDirection = default(Vec2);
		if (!IsAttackerAgentNull && attackerAgent.HasMount)
		{
			AttackerAgentMountMovementDirection = attackerAgent.MountAgent.GetMovementDirection();
		}
		VictimAgentMountMovementDirection = default(Vec2);
		if (!IsVictimAgentNull && victimAgent.HasMount)
		{
			VictimAgentMountMovementDirection = victimAgent.MountAgent.GetMovementDirection();
		}
		IsVictimAgentSameWithAttackerAgent = !IsAttackerAgentNull && attackerAgent == victimAgent;
		WeaponAttachBoneIndex = ((!attackerWeapon.IsEmpty && !IsAttackerAgentNull && attackerAgent.IsHuman) ? attackerAgent.Monster.GetBoneToAttachForItemFlags(attackerWeapon.Item.ItemFlags) : (-1));
		DestructableComponent destructableComponent = (HitObjectDestructibleComponent = hitObject?.GetFirstScriptOfTypeInFamily<DestructableComponent>());
		IsFriendlyFire = !IsAttackerAgentNull && (IsVictimAgentSameWithAttackerAgent || (!IsVictimAgentNull && victimAgent.IsFriendOf(attackerAgent)) || (destructableComponent != null && destructableComponent.BattleSide == attackerAgent.Team?.Side));
		OffHandItem = default(MissionWeapon);
		if (!IsAttackerAgentNull && (attackerAgent.GetAgentFlags() & AgentFlag.CanWieldWeapon) != 0)
		{
			EquipmentIndex wieldedItemIndex4 = attackerAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
			if (wieldedItemIndex4 != EquipmentIndex.None)
			{
				OffHandItem = attackerAgent.Equipment[wieldedItemIndex4];
			}
		}
		IsHeadShot = attackCollisionData.VictimHitBodyPart == BoneBodyPartType.Head;
		VictimAgentAbsorbedDamageRatio = 0f;
		DamageMultiplierOfBone = 0f;
		VictimMovementDirectionAsAngle = 0f;
		VictimAgentScale = 0f;
		VictimAgentHealth = 0f;
		VictimAgentMaxHealth = 0f;
		VictimAgentWeight = 0f;
		VictimAgentTotalEncumbrance = 0f;
		CombatDifficultyMultiplier = 1f;
		VictimHitPointRate = 0f;
		VictimAgentFlag = AgentFlag.CanAttack;
		IsVictimAgentLeftStance = false;
		DoesVictimHaveMountAgent = false;
		IsVictimAgentMine = false;
		DoesVictimHaveRiderAgent = false;
		IsVictimAgentRiderAgentMine = false;
		IsVictimAgentMount = false;
		IsVictimAgentHuman = false;
		IsVictimRiderAgentSameAsAttackerAgent = false;
		IsVictimPlayer = false;
		VictimAgentCharacter = null;
		VictimRiderAgentCharacter = null;
		VictimAgentMovementVelocity = default(Vec2);
		VictimAgentPosition = default(Vec3);
		VictimAgentVelocity = default(Vec3);
		VictimCaptainCharacter = null;
		VictimAgentOrigin = null;
		VictimRiderAgentOrigin = null;
		VictimFormation = null;
		if (!IsVictimAgentNull)
		{
			IsVictimAgentMount = victimAgent.IsMount;
			IsVictimAgentMine = victimAgent.IsMine;
			IsVictimAgentHuman = victimAgent.IsHuman;
			IsVictimAgentLeftStance = victimAgent.GetIsLeftStance();
			DoesVictimHaveMountAgent = victimAgent.HasMount;
			DoesVictimHaveRiderAgent = victimAgent.RiderAgent != null;
			IsVictimRiderAgentSameAsAttackerAgent = DoesVictimHaveRiderAgent && victimAgent.RiderAgent == attackerAgent;
			IsVictimPlayer = victimAgent.IsPlayerControlled;
			VictimAgentAbsorbedDamageRatio = victimAgent.Monster.AbsorbedDamageRatio;
			DamageMultiplierOfBone = MissionGameModels.Current.AgentApplyDamageModel.GetDamageMultiplierForBodyPart((attackCollisionData.CollisionBoneIndex != -1) ? victimAgent.AgentVisuals.GetBoneTypeData(attackCollisionData.CollisionBoneIndex).BodyPartType : BoneBodyPartType.None, (DamageTypes)attackCollisionData.DamageType, IsVictimAgentHuman, attackCollisionData.IsMissile);
			VictimMovementDirectionAsAngle = victimAgent.MovementDirectionAsAngle;
			VictimAgentScale = victimAgent.AgentScale;
			VictimAgentHealth = victimAgent.Health;
			VictimAgentMaxHealth = victimAgent.HealthLimit;
			VictimAgentWeight = (victimAgent.IsMount ? victimAgent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot].Weight : ((float)victimAgent.Monster.Weight));
			VictimAgentTotalEncumbrance = victimAgent.GetTotalEncumbrance();
			CombatDifficultyMultiplier = Mission.Current.GetDamageMultiplierOfCombatDifficulty(victimAgent, attackerAgent);
			VictimHitPointRate = victimAgent.Health / victimAgent.HealthLimit;
			VictimAgentMovementVelocity = victimAgent.MovementVelocity;
			VictimAgentVelocity = victimAgent.Velocity;
			VictimAgentPosition = victimAgent.Position;
			VictimAgentFlag = victimAgent.GetAgentFlags();
			VictimAgentCharacter = victimAgent.Character;
			VictimAgentOrigin = victimAgent.Origin;
			if (DoesVictimHaveRiderAgent)
			{
				Agent riderAgent = victimAgent.RiderAgent;
				IsVictimAgentRiderAgentMine = riderAgent.IsMine;
				VictimRiderAgentCharacter = riderAgent.Character;
				VictimRiderAgentOrigin = riderAgent.Origin;
				Agent agent = riderAgent.Formation?.Captain;
				VictimCaptainCharacter = ((riderAgent == agent) ? null : agent?.Character);
				VictimFormation = riderAgent.Formation;
			}
			else
			{
				Agent agent2 = victimAgent.Formation?.Captain;
				VictimCaptainCharacter = ((victimAgent == agent2) ? null : agent2?.Character);
				VictimFormation = victimAgent.Formation;
			}
		}
		AttackerMovementDirectionAsAngle = 0f;
		AttackerAgentMountChargeDamageProperty = 0f;
		DoesAttackerHaveMountAgent = false;
		IsAttackerAgentMine = false;
		DoesAttackerHaveRiderAgent = false;
		IsAttackerAgentRiderAgentMine = false;
		IsAttackerAgentMount = false;
		IsAttackerAgentHuman = false;
		IsAttackerAgentActive = false;
		IsAttackerAgentDoingPassiveAttack = false;
		IsAttackerPlayer = false;
		AttackerAgentMovementVelocity = default(Vec2);
		AttackerAgentCharacter = null;
		AttackerRiderAgentCharacter = null;
		AttackerAgentOrigin = null;
		AttackerRiderAgentOrigin = null;
		AttackerAgentMovementDirection = default(Vec2);
		AttackerAgentVelocity = default(Vec3);
		AttackerAgentCurrentWeaponOffset = default(Vec3);
		IsAttackerAIControlled = false;
		AttackerCaptainCharacter = null;
		AttackerFormation = null;
		AttackerHitPointRate = 0f;
		if (!IsAttackerAgentNull)
		{
			DoesAttackerHaveMountAgent = attackerAgent.HasMount;
			IsAttackerAgentMine = attackerAgent.IsMine;
			IsAttackerAgentMount = attackerAgent.IsMount;
			IsAttackerAgentHuman = attackerAgent.IsHuman;
			IsAttackerAgentActive = attackerAgent.IsActive();
			IsAttackerAgentDoingPassiveAttack = attackerAgent.IsDoingPassiveAttack;
			DoesAttackerHaveRiderAgent = attackerAgent.RiderAgent != null;
			IsAttackerAIControlled = attackerAgent.IsAIControlled;
			IsAttackerPlayer = attackerAgent.IsPlayerControlled;
			AttackerMovementDirectionAsAngle = attackerAgent.MovementDirectionAsAngle;
			AttackerAgentMountChargeDamageProperty = attackerAgent.GetAgentDrivenPropertyValue(DrivenProperty.MountChargeDamage);
			AttackerHitPointRate = attackerAgent.Health / attackerAgent.HealthLimit;
			AttackerAgentMovementVelocity = attackerAgent.MovementVelocity;
			AttackerAgentMovementDirection = attackerAgent.GetMovementDirection();
			AttackerAgentVelocity = attackerAgent.Velocity;
			if (IsAttackerAgentActive)
			{
				AttackerAgentCurrentWeaponOffset = attackerAgent.GetCurWeaponOffset();
			}
			AttackerAgentCharacter = attackerAgent.Character;
			AttackerAgentOrigin = attackerAgent.Origin;
			if (DoesAttackerHaveRiderAgent)
			{
				Agent riderAgent2 = attackerAgent.RiderAgent;
				IsAttackerAgentRiderAgentMine = riderAgent2.IsMine;
				AttackerRiderAgentCharacter = riderAgent2.Character;
				AttackerRiderAgentOrigin = riderAgent2.Origin;
				Agent agent3 = riderAgent2.Formation?.Captain;
				AttackerCaptainCharacter = ((riderAgent2 == agent3) ? null : agent3?.Character);
				AttackerFormation = riderAgent2.Formation;
			}
			else
			{
				Agent agent4 = attackerAgent.Formation?.Captain;
				AttackerCaptainCharacter = ((attackerAgent == agent4) ? null : agent4?.Character);
				AttackerFormation = attackerAgent.Formation;
			}
		}
		CanGiveDamageToAgentShield = true;
		if (!IsVictimAgentSameWithAttackerAgent)
		{
			CanGiveDamageToAgentShield = Mission.Current.CanGiveDamageToAgentShield(attackerAgent, attackerWeapon.CurrentUsageItem, victimAgent);
		}
	}

	public AttackInformation(Agent attackerAgent, Agent victimAgent, float armorAmountFloat, WeaponComponentData shieldOnBack, AgentFlag victimAgentFlag, float victimAgentAbsorbedDamageRatio, float damageMultiplierOfBone, float combatDifficultyMultiplier, MissionWeapon victimMainHandWeapon, MissionWeapon victimShield, bool canGiveDamageToAgentShield, bool isVictimAgentLeftStance, bool isFriendlyFire, bool doesAttackerHaveMountAgent, bool doesVictimHaveMountAgent, Vec2 attackerAgentMovementVelocity, Vec2 attackerAgentMountMovementDirection, float attackerMovementDirectionAsAngle, Vec2 victimAgentMovementVelocity, Vec2 victimAgentMountMovementDirection, float victimMovementDirectionAsAngle, bool isVictimAgentSameWithAttackerAgent, bool isAttackerAgentMine, bool doesAttackerHaveRiderAgent, bool isAttackerAgentRiderAgentMine, bool isAttackerAgentMount, bool isVictimAgentMine, bool doesVictimHaveRiderAgent, bool isVictimAgentRiderAgentMine, bool isVictimAgentMount, bool isAttackerAgentNull, bool isAttackerAIControlled, BasicCharacterObject attackerAgentCharacter, BasicCharacterObject attackerRiderAgentCharacter, IAgentOriginBase attackerAgentOrigin, IAgentOriginBase attackerRiderAgentOrigin, BasicCharacterObject victimAgentCharacter, BasicCharacterObject victimRiderAgentCharacter, IAgentOriginBase victimAgentOrigin, IAgentOriginBase victimRiderAgentOrigin, Vec2 attackerAgentMovementDirection, Vec3 attackerAgentVelocity, float attackerAgentMountChargeDamageProperty, Vec3 attackerAgentCurrentWeaponOffset, bool isAttackerAgentHuman, bool isAttackerAgentActive, bool isAttackerAgentDoingPassiveAttack, bool isVictimAgentNull, float victimAgentScale, float victimAgentHealth, float victimAgentMaxHealth, float victimAgentWeight, float victimAgentTotalEncumbrance, bool isVictimAgentHuman, Vec3 victimAgentVelocity, Vec3 victimAgentPosition, int weaponAttachBoneIndex, MissionWeapon offHandItem, bool isHeadShot, bool isVictimRiderAgentSameAsAttackerAgent, bool isAttackerPlayer, bool isVictimPlayer, DestructableComponent hitObjectDestructibleComponent)
	{
		AttackerAgent = attackerAgent;
		VictimAgent = victimAgent;
		ArmorAmountFloat = armorAmountFloat;
		ShieldOnBack = shieldOnBack;
		VictimAgentFlag = victimAgentFlag;
		VictimAgentAbsorbedDamageRatio = victimAgentAbsorbedDamageRatio;
		DamageMultiplierOfBone = damageMultiplierOfBone;
		CombatDifficultyMultiplier = combatDifficultyMultiplier;
		VictimMainHandWeapon = victimMainHandWeapon;
		VictimShield = victimShield;
		CanGiveDamageToAgentShield = canGiveDamageToAgentShield;
		IsVictimAgentLeftStance = isVictimAgentLeftStance;
		IsFriendlyFire = isFriendlyFire;
		DoesAttackerHaveMountAgent = doesAttackerHaveMountAgent;
		DoesVictimHaveMountAgent = doesVictimHaveMountAgent;
		AttackerAgentMovementVelocity = attackerAgentMovementVelocity;
		AttackerAgentMountMovementDirection = attackerAgentMountMovementDirection;
		AttackerMovementDirectionAsAngle = attackerMovementDirectionAsAngle;
		VictimAgentMovementVelocity = victimAgentMovementVelocity;
		VictimAgentMountMovementDirection = victimAgentMountMovementDirection;
		VictimMovementDirectionAsAngle = victimMovementDirectionAsAngle;
		IsVictimAgentSameWithAttackerAgent = isVictimAgentSameWithAttackerAgent;
		IsAttackerAgentMine = isAttackerAgentMine;
		DoesAttackerHaveRiderAgent = doesAttackerHaveRiderAgent;
		IsAttackerAgentRiderAgentMine = isAttackerAgentRiderAgentMine;
		IsAttackerAgentMount = isAttackerAgentMount;
		IsVictimAgentMine = isVictimAgentMine;
		DoesVictimHaveRiderAgent = doesVictimHaveRiderAgent;
		IsVictimAgentRiderAgentMine = isVictimAgentRiderAgentMine;
		IsVictimAgentMount = isVictimAgentMount;
		IsAttackerAgentNull = isAttackerAgentNull;
		IsAttackerAIControlled = isAttackerAIControlled;
		AttackerAgentCharacter = attackerAgentCharacter;
		AttackerRiderAgentCharacter = attackerRiderAgentCharacter;
		AttackerAgentOrigin = attackerAgentOrigin;
		AttackerRiderAgentOrigin = attackerRiderAgentOrigin;
		VictimAgentCharacter = victimAgentCharacter;
		VictimRiderAgentCharacter = victimRiderAgentCharacter;
		VictimAgentOrigin = victimAgentOrigin;
		VictimRiderAgentOrigin = victimRiderAgentOrigin;
		AttackerAgentMovementDirection = attackerAgentMovementDirection;
		AttackerAgentVelocity = attackerAgentVelocity;
		AttackerAgentMountChargeDamageProperty = attackerAgentMountChargeDamageProperty;
		AttackerAgentCurrentWeaponOffset = attackerAgentCurrentWeaponOffset;
		IsAttackerAgentHuman = isAttackerAgentHuman;
		IsAttackerAgentActive = isAttackerAgentActive;
		IsAttackerAgentDoingPassiveAttack = isAttackerAgentDoingPassiveAttack;
		VictimAgentScale = victimAgentScale;
		IsVictimAgentNull = isVictimAgentNull;
		VictimAgentHealth = victimAgentHealth;
		VictimAgentMaxHealth = victimAgentMaxHealth;
		VictimAgentWeight = victimAgentWeight;
		VictimAgentTotalEncumbrance = victimAgentTotalEncumbrance;
		IsVictimAgentHuman = isVictimAgentHuman;
		VictimAgentVelocity = victimAgentVelocity;
		VictimAgentPosition = victimAgentPosition;
		WeaponAttachBoneIndex = weaponAttachBoneIndex;
		OffHandItem = offHandItem;
		IsHeadShot = isHeadShot;
		IsVictimRiderAgentSameAsAttackerAgent = isVictimRiderAgentSameAsAttackerAgent;
		AttackerCaptainCharacter = null;
		VictimCaptainCharacter = null;
		VictimFormation = null;
		AttackerFormation = null;
		AttackerHitPointRate = 1f;
		VictimHitPointRate = 1f;
		IsAttackerPlayer = isAttackerPlayer;
		IsVictimPlayer = isVictimPlayer;
		HitObjectDestructibleComponent = hitObjectDestructibleComponent;
	}
}
