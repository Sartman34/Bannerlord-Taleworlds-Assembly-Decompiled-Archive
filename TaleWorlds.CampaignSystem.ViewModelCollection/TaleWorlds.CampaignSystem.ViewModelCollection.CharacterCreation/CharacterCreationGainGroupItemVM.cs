using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;

public class CharacterCreationGainGroupItemVM : ViewModel
{
	private readonly TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation _characterCreation;

	private readonly int _currentIndex;

	private MBBindingList<CharacterCreationGainedSkillItemVM> _skills;

	private CharacterCreationGainedAttributeItemVM _attribute;

	public CharacterAttribute AttributeObj { get; private set; }

	[DataSourceProperty]
	public MBBindingList<CharacterCreationGainedSkillItemVM> Skills
	{
		get
		{
			return _skills;
		}
		set
		{
			if (value != _skills)
			{
				_skills = value;
				OnPropertyChangedWithValue(value, "Skills");
			}
		}
	}

	[DataSourceProperty]
	public CharacterCreationGainedAttributeItemVM Attribute
	{
		get
		{
			return _attribute;
		}
		set
		{
			if (value != _attribute)
			{
				_attribute = value;
				OnPropertyChangedWithValue(value, "Attribute");
			}
		}
	}

	public CharacterCreationGainGroupItemVM(CharacterAttribute attributeObj, TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation characterCreation, int currentIndex)
	{
		AttributeObj = attributeObj;
		_characterCreation = characterCreation;
		_currentIndex = currentIndex;
		Skills = new MBBindingList<CharacterCreationGainedSkillItemVM>();
		Attribute = new CharacterCreationGainedAttributeItemVM(AttributeObj);
		foreach (SkillObject skill in AttributeObj.Skills)
		{
			Skills.Add(new CharacterCreationGainedSkillItemVM(skill));
		}
	}

	public void ResetValues()
	{
		Attribute.ResetValues();
		Skills.ApplyActionOnAllItems(delegate(CharacterCreationGainedSkillItemVM s)
		{
			s.ResetValues();
		});
	}
}
