using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem;

public class DefaultSkillEffects
{
	private SkillEffect _effectOneHandedSpeed;

	private SkillEffect _effectOneHandedDamage;

	private SkillEffect _effectTwoHandedSpeed;

	private SkillEffect _effectTwoHandedDamage;

	private SkillEffect _effectPolearmSpeed;

	private SkillEffect _effectPolearmDamage;

	private SkillEffect _effectBowLevel;

	private SkillEffect _effectBowDamage;

	private SkillEffect _effectBowAccuracy;

	private SkillEffect _effectThrowingSpeed;

	private SkillEffect _effectThrowingDamage;

	private SkillEffect _effectThrowingAccuracy;

	private SkillEffect _effectCrossbowReloadSpeed;

	private SkillEffect _effectCrossbowAccuracy;

	private SkillEffect _effectHorseLevel;

	private SkillEffect _effectHorseSpeed;

	private SkillEffect _effectHorseManeuver;

	private SkillEffect _effectMountedWeaponDamagePenalty;

	private SkillEffect _effectMountedWeaponSpeedPenalty;

	private SkillEffect _effectDismountResistance;

	private SkillEffect _effectAthleticsSpeedFactor;

	private SkillEffect _effectAthleticsWeightFactor;

	private SkillEffect _effectKnockBackResistance;

	private SkillEffect _effectKnockDownResistance;

	private SkillEffect _effectSmithingLevel;

	private SkillEffect _effectTacticsAdvantage;

	private SkillEffect _effectTacticsTroopSacrificeReduction;

	private SkillEffect _effectTrackingLevel;

	private SkillEffect _effectTrackingRadius;

	private SkillEffect _effectTrackingSpottingDistance;

	private SkillEffect _effectTrackingTrackInformation;

	private SkillEffect _effectRogueryLootBonus;

	private SkillEffect _effectCharmRelationBonus;

	private SkillEffect _effectTradePenaltyReduction;

	private SkillEffect _effectSurgeonSurvivalBonus;

	private SkillEffect _effectSiegeEngineProductionBonus;

	private SkillEffect _effectTownProjectBuildingBonus;

	private SkillEffect _effectHealingRateBonusForHeroes;

	private SkillEffect _effectHealingRateBonusForRegulars;

	private SkillEffect _effectGovernorHealingRateBonus;

	private SkillEffect _effectLeadershipMoraleBonus;

	private SkillEffect _effectLeadershipGarrisonSizeBonus;

	private SkillEffect _effectStewardPartySizeBonus;

	private SkillEffect _effectEngineerLevel;

	private static DefaultSkillEffects Instance => Campaign.Current.DefaultSkillEffects;

	public static SkillEffect OneHandedSpeed => Instance._effectOneHandedSpeed;

	public static SkillEffect OneHandedDamage => Instance._effectOneHandedDamage;

	public static SkillEffect TwoHandedSpeed => Instance._effectTwoHandedSpeed;

	public static SkillEffect TwoHandedDamage => Instance._effectTwoHandedDamage;

	public static SkillEffect PolearmSpeed => Instance._effectPolearmSpeed;

	public static SkillEffect PolearmDamage => Instance._effectPolearmDamage;

	public static SkillEffect BowLevel => Instance._effectBowLevel;

	public static SkillEffect BowDamage => Instance._effectBowDamage;

	public static SkillEffect BowAccuracy => Instance._effectBowAccuracy;

	public static SkillEffect ThrowingSpeed => Instance._effectThrowingSpeed;

	public static SkillEffect ThrowingDamage => Instance._effectThrowingDamage;

	public static SkillEffect ThrowingAccuracy => Instance._effectThrowingAccuracy;

	public static SkillEffect CrossbowReloadSpeed => Instance._effectCrossbowReloadSpeed;

	public static SkillEffect CrossbowAccuracy => Instance._effectCrossbowAccuracy;

	public static SkillEffect HorseLevel => Instance._effectHorseLevel;

	public static SkillEffect HorseSpeed => Instance._effectHorseSpeed;

	public static SkillEffect HorseManeuver => Instance._effectHorseManeuver;

	public static SkillEffect MountedWeaponDamagePenalty => Instance._effectMountedWeaponDamagePenalty;

	public static SkillEffect MountedWeaponSpeedPenalty => Instance._effectMountedWeaponSpeedPenalty;

	public static SkillEffect DismountResistance => Instance._effectDismountResistance;

	public static SkillEffect AthleticsSpeedFactor => Instance._effectAthleticsSpeedFactor;

	public static SkillEffect AthleticsWeightFactor => Instance._effectAthleticsWeightFactor;

	public static SkillEffect KnockBackResistance => Instance._effectKnockBackResistance;

	public static SkillEffect KnockDownResistance => Instance._effectKnockDownResistance;

	public static SkillEffect SmithingLevel => Instance._effectSmithingLevel;

	public static SkillEffect TacticsAdvantage => Instance._effectTacticsAdvantage;

	public static SkillEffect TacticsTroopSacrificeReduction => Instance._effectTacticsTroopSacrificeReduction;

	public static SkillEffect TrackingRadius => Instance._effectTrackingRadius;

	public static SkillEffect TrackingLevel => Instance._effectTrackingLevel;

	public static SkillEffect TrackingSpottingDistance => Instance._effectTrackingSpottingDistance;

	public static SkillEffect TrackingTrackInformation => Instance._effectTrackingTrackInformation;

	public static SkillEffect RogueryLootBonus => Instance._effectRogueryLootBonus;

	public static SkillEffect CharmRelationBonus => Instance._effectCharmRelationBonus;

	public static SkillEffect TradePenaltyReduction => Instance._effectTradePenaltyReduction;

	public static SkillEffect SurgeonSurvivalBonus => Instance._effectSurgeonSurvivalBonus;

	public static SkillEffect SiegeEngineProductionBonus => Instance._effectSiegeEngineProductionBonus;

	public static SkillEffect TownProjectBuildingBonus => Instance._effectTownProjectBuildingBonus;

	public static SkillEffect HealingRateBonusForHeroes => Instance._effectHealingRateBonusForHeroes;

	public static SkillEffect HealingRateBonusForRegulars => Instance._effectHealingRateBonusForRegulars;

	public static SkillEffect GovernorHealingRateBonus => Instance._effectGovernorHealingRateBonus;

	public static SkillEffect LeadershipMoraleBonus => Instance._effectLeadershipMoraleBonus;

	public static SkillEffect LeadershipGarrisonSizeBonus => Instance._effectLeadershipGarrisonSizeBonus;

	public static SkillEffect StewardPartySizeBonus => Instance._effectStewardPartySizeBonus;

	public static SkillEffect EngineerLevel => Instance._effectEngineerLevel;

	public DefaultSkillEffects()
	{
		RegisterAll();
	}

	private void RegisterAll()
	{
		_effectOneHandedSpeed = Create("OneHandedSpeed");
		_effectOneHandedDamage = Create("OneHandedDamage");
		_effectTwoHandedSpeed = Create("TwoHandedSpeed");
		_effectTwoHandedDamage = Create("TwoHandedDamage");
		_effectPolearmSpeed = Create("PolearmSpeed");
		_effectPolearmDamage = Create("PolearmDamage");
		_effectBowLevel = Create("BowLevel");
		_effectBowDamage = Create("BowDamage");
		_effectBowAccuracy = Create("BowAccuracy");
		_effectThrowingSpeed = Create("ThrowingSpeed");
		_effectThrowingDamage = Create("ThrowingDamage");
		_effectThrowingAccuracy = Create("ThrowingAccuracy");
		_effectCrossbowReloadSpeed = Create("CrossbowReloadSpeed");
		_effectCrossbowAccuracy = Create("CrossbowAccuracy");
		_effectHorseLevel = Create("HorseLevel");
		_effectHorseSpeed = Create("HorseSpeed");
		_effectHorseManeuver = Create("HorseManeuver");
		_effectMountedWeaponDamagePenalty = Create("MountedWeaponDamagePenalty");
		_effectMountedWeaponSpeedPenalty = Create("MountedWeaponSpeedPenalty");
		_effectDismountResistance = Create("DismountResistance");
		_effectAthleticsSpeedFactor = Create("AthleticsSpeedFactor");
		_effectAthleticsWeightFactor = Create("AthleticsWeightFactor");
		_effectKnockBackResistance = Create("KnockBackResistance");
		_effectKnockDownResistance = Create("KnockDownResistance");
		_effectSmithingLevel = Create("SmithingLevel");
		_effectTacticsAdvantage = Create("TacticsAdvantage");
		_effectTacticsTroopSacrificeReduction = Create("TacticsTroopSacrificeReduction");
		_effectTrackingRadius = Create("TrackingRadius");
		_effectTrackingLevel = Create("TrackingLevel");
		_effectTrackingSpottingDistance = Create("TrackingSpottingDistance");
		_effectTrackingTrackInformation = Create("TrackingTrackInformation");
		_effectRogueryLootBonus = Create("RogueryLootBonus");
		_effectCharmRelationBonus = Create("CharmRelationBonus");
		_effectTradePenaltyReduction = Create("TradePenaltyReduction");
		_effectLeadershipMoraleBonus = Create("LeadershipMoraleBonus");
		_effectLeadershipGarrisonSizeBonus = Create("LeadershipGarrisonSizeBonus");
		_effectSurgeonSurvivalBonus = Create("SurgeonSurvivalBonus");
		_effectHealingRateBonusForHeroes = Create("HealingRateBonusForHeroes");
		_effectHealingRateBonusForRegulars = Create("HealingRateBonusForRegulars");
		_effectGovernorHealingRateBonus = Create("GovernorHealingRateBonus");
		_effectSiegeEngineProductionBonus = Create("SiegeEngineProductionBonus");
		_effectTownProjectBuildingBonus = Create("TownProjectBuildingBonus");
		_effectStewardPartySizeBonus = Create("StewardPartySizeBonus");
		_effectEngineerLevel = Create("EngineerLevel");
		InitializeAll();
	}

	private SkillEffect Create(string stringId)
	{
		return Game.Current.ObjectManager.RegisterPresumedObject(new SkillEffect(stringId));
	}

	private void InitializeAll()
	{
		_effectOneHandedSpeed.Initialize(new TextObject("{=hjxRvb9l}One handed weapon speed: +{a0}%"), new SkillObject[1] { DefaultSkills.OneHanded }, SkillEffect.PerkRole.Personal, 0.07f);
		_effectOneHandedDamage.Initialize(new TextObject("{=baUFKAbd}One handed weapon damage: +{a0}%"), new SkillObject[1] { DefaultSkills.OneHanded }, SkillEffect.PerkRole.Personal, 0.15f);
		_effectTwoHandedSpeed.Initialize(new TextObject("{=Np94rYMz}Two handed weapon speed: +{a0}%"), new SkillObject[1] { DefaultSkills.TwoHanded }, SkillEffect.PerkRole.Personal, 0.06f);
		_effectTwoHandedDamage.Initialize(new TextObject("{=QkbbLb4v}Two handed weapon damage: +{a0}%"), new SkillObject[1] { DefaultSkills.TwoHanded }, SkillEffect.PerkRole.Personal, 0.16f);
		_effectPolearmSpeed.Initialize(new TextObject("{=2ATI9qVM}Polearm weapon speed: +{a0}%"), new SkillObject[1] { DefaultSkills.Polearm }, SkillEffect.PerkRole.Personal, 0.06f);
		_effectPolearmDamage.Initialize(new TextObject("{=17cIGVQE}Polearm weapon damage: +{a0}%"), new SkillObject[1] { DefaultSkills.Polearm }, SkillEffect.PerkRole.Personal, 0.07f);
		_effectBowLevel.Initialize(new TextObject("{=XN7BX0qP}Max usable bow difficulty: {a0}"), new SkillObject[1] { DefaultSkills.Bow }, SkillEffect.PerkRole.Personal, 1f);
		_effectBowDamage.Initialize(new TextObject("{=RUZHJMQO}Bow Damage: +{a0}%"), new SkillObject[1] { DefaultSkills.Bow }, SkillEffect.PerkRole.Personal, 0.11f);
		_effectBowAccuracy.Initialize(new TextObject("{=sQCS90Wq}Bow Accuracy: +{a0}%"), new SkillObject[1] { DefaultSkills.Bow }, SkillEffect.PerkRole.Personal, 0.09f);
		_effectThrowingSpeed.Initialize(new TextObject("{=Z0CoeojG}Thrown weapon speed: +{a0}%"), new SkillObject[1] { DefaultSkills.Throwing }, SkillEffect.PerkRole.Personal, 0.07f);
		_effectThrowingDamage.Initialize(new TextObject("{=TQMGppEk}Thrown weapon damage: +{a0}%"), new SkillObject[1] { DefaultSkills.Throwing }, SkillEffect.PerkRole.Personal, 0.06f);
		_effectThrowingAccuracy.Initialize(new TextObject("{=SfKrjKuO}Thrown weapon accuracy: +{a0}%"), new SkillObject[1] { DefaultSkills.Throwing }, SkillEffect.PerkRole.Personal, 0.06f);
		_effectCrossbowReloadSpeed.Initialize(new TextObject("{=W0Zu4iDz}Crossbow reload speed: +{a0}%"), new SkillObject[1] { DefaultSkills.Crossbow }, SkillEffect.PerkRole.Personal, 0.07f);
		_effectCrossbowAccuracy.Initialize(new TextObject("{=JwWnpD40}Crossbow accuracy: +{a0}%"), new SkillObject[1] { DefaultSkills.Crossbow }, SkillEffect.PerkRole.Personal, 0.05f);
		_effectHorseLevel.Initialize(new TextObject("{=8uBbbwY9}Max mount difficulty: {a0}"), new SkillObject[1] { DefaultSkills.Riding }, SkillEffect.PerkRole.Personal, 1f);
		_effectHorseSpeed.Initialize(new TextObject("{=Y07OcP1T}Horse speed: +{a0}"), new SkillObject[1] { DefaultSkills.Riding }, SkillEffect.PerkRole.Personal, 0.2f);
		_effectHorseManeuver.Initialize(new TextObject("{=AahNTeXY}Horse maneuver: +{a0}"), new SkillObject[1] { DefaultSkills.Riding }, SkillEffect.PerkRole.Personal, 0.04f);
		_effectMountedWeaponDamagePenalty.Initialize(new TextObject("{=0dbwEczK}Mounted weapon damage penalty: {a0}%"), new SkillObject[1] { DefaultSkills.Riding }, SkillEffect.PerkRole.Personal, -0.2f, SkillEffect.PerkRole.None, 0f, SkillEffect.EffectIncrementType.Add, 20f);
		_effectMountedWeaponSpeedPenalty.Initialize(new TextObject("{=oE5etyy0}Mounted weapon speed & reload penalty: {a0}%"), new SkillObject[1] { DefaultSkills.Riding }, SkillEffect.PerkRole.Personal, -0.3f, SkillEffect.PerkRole.None, 0f, SkillEffect.EffectIncrementType.Add, 30f);
		_effectDismountResistance.Initialize(new TextObject("{=kbHJVxAo}Dismount resistance: {a0}% of max. hitpoints"), new SkillObject[1] { DefaultSkills.Riding }, SkillEffect.PerkRole.Personal, 0.1f, SkillEffect.PerkRole.None, 0f, SkillEffect.EffectIncrementType.Add, 40f);
		_effectAthleticsSpeedFactor.Initialize(new TextObject("{=rgb6vdon}Running speed increased by {a0}%"), new SkillObject[1] { DefaultSkills.Athletics }, SkillEffect.PerkRole.Personal, 0.1f);
		_effectAthleticsWeightFactor.Initialize(new TextObject("{=WaUuhxwv}Weight penalty reduced by: {a0}%"), new SkillObject[1] { DefaultSkills.Athletics }, SkillEffect.PerkRole.Personal, 0.1f);
		_effectKnockBackResistance.Initialize(new TextObject("{=TyjDHQUv}Knock back resistance: {a0}% of max. hitpoints"), new SkillObject[1] { DefaultSkills.Athletics }, SkillEffect.PerkRole.Personal, 0.1f, SkillEffect.PerkRole.None, 0f, SkillEffect.EffectIncrementType.Add, 15f);
		_effectKnockDownResistance.Initialize(new TextObject("{=tlNZIH3l}Knock down resistance: {a0}% of max. hitpoints"), new SkillObject[1] { DefaultSkills.Athletics }, SkillEffect.PerkRole.Personal, 0.1f, SkillEffect.PerkRole.None, 0f, SkillEffect.EffectIncrementType.Add, 40f);
		_effectSmithingLevel.Initialize(new TextObject("{=ImN8Cfk6}Max difficulty of weapon that can be smithed without penalty: {a0}"), new SkillObject[1] { DefaultSkills.Crafting }, SkillEffect.PerkRole.Personal, 1f);
		_effectTacticsAdvantage.Initialize(new TextObject("{=XO3SOlZx}Simulation advantage: +{a0}%"), new SkillObject[1] { DefaultSkills.Tactics }, SkillEffect.PerkRole.Personal, 0.1f);
		_effectTacticsTroopSacrificeReduction.Initialize(new TextObject("{=VHdyQYKI}Decrease the sacrificed troop number when trying to get away +{a0}%"), new SkillObject[1] { DefaultSkills.Tactics }, SkillEffect.PerkRole.Personal, 0.1f);
		_effectTrackingRadius.Initialize(new TextObject("{=kqJipMqc}Track detection radius +{a0}%"), new SkillObject[1] { DefaultSkills.Scouting }, SkillEffect.PerkRole.Scout, 0.1f, SkillEffect.PerkRole.None, 0.05f, SkillEffect.EffectIncrementType.Add);
		_effectTrackingLevel.Initialize(new TextObject("{=aGecGUub}Max track difficulty that can be detected: {a0}"), new SkillObject[1] { DefaultSkills.Scouting }, SkillEffect.PerkRole.Scout, 1f, SkillEffect.PerkRole.None, 1f, SkillEffect.EffectIncrementType.Add);
		_effectTrackingSpottingDistance.Initialize(new TextObject("{=lbrOAvKj}Spotting distance +{a0}%"), new SkillObject[1] { DefaultSkills.Scouting }, SkillEffect.PerkRole.Scout, 0.06f, SkillEffect.PerkRole.None, 0.03f, SkillEffect.EffectIncrementType.Add);
		_effectTrackingTrackInformation.Initialize(new TextObject("{=uNls3bOP}Track information level: {a0}"), new SkillObject[1] { DefaultSkills.Scouting }, SkillEffect.PerkRole.Scout, 0.04f, SkillEffect.PerkRole.None, 0.03f, SkillEffect.EffectIncrementType.Add);
		_effectRogueryLootBonus.Initialize(new TextObject("{=bN3bLDb2}Battle Loot +{a0}%"), new SkillObject[1] { DefaultSkills.Roguery }, SkillEffect.PerkRole.PartyLeader, 0.25f);
		_effectCharmRelationBonus.Initialize(new TextObject("{=c5dsio8Q}Relation increase with NPCs +{a0}%"), new SkillObject[1] { DefaultSkills.Charm }, SkillEffect.PerkRole.Personal, 0.5f);
		_effectTradePenaltyReduction.Initialize(new TextObject("{=uq7JwT1Z}Trade penalty Reduction +{a0}%"), new SkillObject[1] { DefaultSkills.Trade }, SkillEffect.PerkRole.PartyLeader, 0.2f);
		_effectLeadershipMoraleBonus.Initialize(new TextObject("{=n3bFiuVu}Increase morale of the parties under your command +{a0}"), new SkillObject[1] { DefaultSkills.Leadership }, SkillEffect.PerkRole.Personal, 0.1f, SkillEffect.PerkRole.None, 0f, SkillEffect.EffectIncrementType.Add);
		_effectLeadershipGarrisonSizeBonus.Initialize(new TextObject("{=cSt26auo}Increase garrison size by +{a0}"), new SkillObject[1] { DefaultSkills.Leadership }, SkillEffect.PerkRole.Personal, 0.2f, SkillEffect.PerkRole.None, 0f, SkillEffect.EffectIncrementType.Add);
		_effectSurgeonSurvivalBonus.Initialize(new TextObject("{=w4BzNJYl}Casualty survival chance +{a0}%"), new SkillObject[1] { DefaultSkills.Medicine }, SkillEffect.PerkRole.Surgeon, 0.01f, SkillEffect.PerkRole.None, 0.01f, SkillEffect.EffectIncrementType.Add);
		_effectHealingRateBonusForHeroes.Initialize(new TextObject("{=fUvs4g40}Healing rate increase for heroes +{a0}%"), new SkillObject[1] { DefaultSkills.Medicine }, SkillEffect.PerkRole.Surgeon, 0.5f, SkillEffect.PerkRole.None, 0.05f);
		_effectHealingRateBonusForRegulars.Initialize(new TextObject("{=A310vHqJ}Healing rate increase for troops +{a0}%"), new SkillObject[1] { DefaultSkills.Medicine }, SkillEffect.PerkRole.Surgeon, 1f, SkillEffect.PerkRole.None, 0.05f);
		_effectGovernorHealingRateBonus.Initialize(new TextObject("{=6mQGst9s}Healing rate increase +{a0}%"), new SkillObject[1] { DefaultSkills.Medicine }, SkillEffect.PerkRole.Governor, 0.1f);
		_effectSiegeEngineProductionBonus.Initialize(new TextObject("{=spbYlf0y}Faster siege engine production +{a0}%"), new SkillObject[1] { DefaultSkills.Engineering }, SkillEffect.PerkRole.Engineer, 0.1f, SkillEffect.PerkRole.None, 0.05f);
		_effectTownProjectBuildingBonus.Initialize(new TextObject("{=2paRqO8u}Faster building production +{a0}%"), new SkillObject[1] { DefaultSkills.Engineering }, SkillEffect.PerkRole.Governor, 0.25f);
		_effectStewardPartySizeBonus.Initialize(new TextObject("{=jNDUXetG}Increase party size by +{a0}"), new SkillObject[1] { DefaultSkills.Steward }, SkillEffect.PerkRole.Quartermaster, 0.25f, SkillEffect.PerkRole.None, 0f, SkillEffect.EffectIncrementType.Add);
		_effectEngineerLevel.Initialize(new TextObject("{=aQduWCrg}Max difficulty of siege engine that can be built: {a0}"), new SkillObject[1] { DefaultSkills.Engineering }, SkillEffect.PerkRole.Engineer, 1f, SkillEffect.PerkRole.None, 0f, SkillEffect.EffectIncrementType.Add);
	}
}
