using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.CharacterCreationContent;

public class CharacterCreationCategory
{
	public CharacterCreationOnCondition CategoryCondition { get; private set; }

	public List<CharacterCreationOption> CharacterCreationOptions { get; private set; }

	public CharacterCreationCategory(CharacterCreationOnCondition condition = null)
	{
		CategoryCondition = condition;
		CharacterCreationOptions = new List<CharacterCreationOption>();
	}

	public void AddCategoryOption(TextObject text, MBList<SkillObject> effectedSkills, CharacterAttribute effectedAttribute, int focusToAdd, int skillLevelToAdd, int attributeLevelToAdd, CharacterCreationOnCondition optionCondition, CharacterCreationOnSelect onSelect, CharacterCreationApplyFinalEffects onApply, TextObject descriptionText = null, MBList<TraitObject> effectedTraits = null, int traitLevelToAdd = 0, int renownToAdd = 0, int goldToAdd = 0, int unspentFocusPoint = 0, int unspentAttributePoint = 0)
	{
		CharacterCreationOption item = new CharacterCreationOption(CharacterCreationOptions.Count + 1, effectedSkills, effectedAttribute, focusToAdd, skillLevelToAdd, attributeLevelToAdd, text, optionCondition, onSelect, onApply, descriptionText, effectedTraits, traitLevelToAdd, renownToAdd, goldToAdd, unspentFocusPoint, unspentAttributePoint);
		CharacterCreationOptions.Add(item);
	}
}
