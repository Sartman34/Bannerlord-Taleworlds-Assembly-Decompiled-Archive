using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class CharacterDevelopmentModel : GameModel
{
	public abstract int MaxAttribute { get; }

	public abstract int MaxFocusPerSkill { get; }

	public abstract int MaxSkillRequiredForEpicPerkBonus { get; }

	public abstract int MinSkillRequiredForEpicPerkBonus { get; }

	public abstract int FocusPointsPerLevel { get; }

	public abstract int FocusPointsAtStart { get; }

	public abstract int AttributePointsAtStart { get; }

	public abstract int LevelsPerAttributePoint { get; }

	public abstract List<Tuple<SkillObject, int>> GetSkillsDerivedFromTraits(Hero hero = null, CharacterObject templateCharacter = null, bool isByNaturalGrowth = false);

	public abstract int SkillsRequiredForLevel(int level);

	public abstract int GetMaxSkillPoint();

	public abstract int GetXpRequiredForSkillLevel(int skillLevel);

	public abstract int GetSkillLevelChange(Hero hero, SkillObject skill, float skillXp);

	public abstract int GetXpAmountForSkillLevelChange(Hero hero, SkillObject skill, int skillLevelChange);

	public abstract void GetTraitLevelForTraitXp(Hero hero, TraitObject trait, int newValue, out int traitLevel, out int traitXp);

	public abstract int GetTraitXpRequiredForTraitLevel(TraitObject trait, int traitLevel);

	public abstract ExplainedNumber CalculateLearningLimit(int attributeValue, int focusValue, TextObject attributeName, bool includeDescriptions = false);

	public abstract float CalculateLearningRate(Hero hero, SkillObject skill);

	public abstract ExplainedNumber CalculateLearningRate(int attributeValue, int focusValue, int skillValue, int characterLevel, TextObject attributeName, bool includeDescriptions = false);

	public abstract SkillObject GetNextSkillToAddFocus(Hero hero);

	public abstract CharacterAttribute GetNextAttributeToUpgrade(Hero hero);

	public abstract PerkObject GetNextPerkToChoose(Hero hero, PerkObject perk);
}
