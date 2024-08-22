using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.CharacterCreationContent;

public class CharacterCreationMenu
{
	public enum MenuTypes
	{
		MultipleChoice,
		SelectAllThatApply
	}

	public readonly MenuTypes MenuType;

	public readonly TextObject Title;

	public readonly TextObject Text;

	public readonly CharacterCreationOnInit OnInit;

	private readonly MBList<CharacterCreationCategory> _characterCreationCategories;

	public readonly List<int> SelectedOptions;

	public MBReadOnlyList<CharacterCreationCategory> CharacterCreationCategories => _characterCreationCategories;

	public CharacterCreationCategory AddMenuCategory(CharacterCreationOnCondition condition = null)
	{
		CharacterCreationCategory characterCreationCategory = new CharacterCreationCategory(condition);
		_characterCreationCategories.Add(characterCreationCategory);
		return characterCreationCategory;
	}

	public CharacterCreationMenu(TextObject title, TextObject text, CharacterCreationOnInit onInit, MenuTypes menuType = MenuTypes.MultipleChoice)
	{
		Title = title;
		Text = text;
		OnInit = onInit;
		SelectedOptions = new List<int>();
		_characterCreationCategories = new MBList<CharacterCreationCategory>();
		MenuType = menuType;
	}

	public void ApplyFinalEffect(CharacterCreation characterCreation)
	{
		foreach (int selectedOption in SelectedOptions)
		{
			foreach (CharacterCreationCategory characterCreationCategory in CharacterCreationCategories)
			{
				if (characterCreationCategory.CategoryCondition == null || characterCreationCategory.CategoryCondition())
				{
					CharacterCreationOption characterCreationOption = characterCreationCategory.CharacterCreationOptions.Find((CharacterCreationOption o) => o.Id == selectedOption);
					if (characterCreationOption.ApplyFinalEffects != null)
					{
						List<SkillObject> skills = characterCreationOption.AffectedSkills?.ToList();
						List<TraitObject> traits = characterCreationOption.AffectedTraits?.ToList();
						CharacterCreationContentBase.Instance.ApplySkillAndAttributeEffects(skills, characterCreationOption.FocusToAdd, characterCreationOption.SkillLevelToAdd, characterCreationOption.EffectedAttribute, characterCreationOption.AttributeLevelToAdd, traits, characterCreationOption.TraitLevelToAdd, characterCreationOption.RenownToAdd, characterCreationOption.GoldToAdd, characterCreationOption.UnspentFocusToAdd, characterCreationOption.UnspentAttributeToAdd);
						characterCreationOption.ApplyFinalEffects(characterCreation);
					}
				}
			}
		}
	}
}
