using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade.ComponentInterfaces;

public abstract class StrikeMagnitudeCalculationModel : GameModel
{
	public abstract float CalculateStrikeMagnitudeForMissile(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float missileSpeed);

	public abstract float CalculateStrikeMagnitudeForSwing(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float swingSpeed, float impactPointAsPercent, float extraLinearSpeed);

	public abstract float CalculateStrikeMagnitudeForThrust(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float thrustSpeed, float extraLinearSpeed, bool isThrown = false);

	public abstract float ComputeRawDamage(DamageTypes damageType, float magnitude, float armorEffectiveness, float absorbedDamageRatio);

	public abstract float GetBluntDamageFactorByDamageType(DamageTypes damageType);

	public abstract float CalculateHorseArcheryFactor(BasicCharacterObject characterObject);

	public virtual float CalculateAdjustedArmorForBlow(float baseArmor, BasicCharacterObject attackerCharacter, BasicCharacterObject attackerCaptainCharacter, BasicCharacterObject victimCharacter, BasicCharacterObject victimCaptainCharacter, WeaponComponentData weaponComponent)
	{
		return baseArmor;
	}
}
