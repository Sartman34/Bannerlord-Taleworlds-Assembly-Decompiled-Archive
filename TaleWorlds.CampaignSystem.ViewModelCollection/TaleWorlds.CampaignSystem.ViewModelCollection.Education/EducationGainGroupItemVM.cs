using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Education;

public class EducationGainGroupItemVM : ViewModel
{
	private MBBindingList<EducationGainedSkillItemVM> _skills;

	private EducationGainedAttributeItemVM _attribute;

	public CharacterAttribute AttributeObj { get; private set; }

	[DataSourceProperty]
	public MBBindingList<EducationGainedSkillItemVM> Skills
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
	public EducationGainedAttributeItemVM Attribute
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

	public EducationGainGroupItemVM(CharacterAttribute attributeObj)
	{
		AttributeObj = attributeObj;
		Skills = new MBBindingList<EducationGainedSkillItemVM>();
		Attribute = new EducationGainedAttributeItemVM(AttributeObj);
		foreach (SkillObject skill in AttributeObj.Skills)
		{
			Skills.Add(new EducationGainedSkillItemVM(skill));
		}
	}

	public void ResetValues()
	{
		Attribute.ResetValues();
		Skills.ApplyActionOnAllItems(delegate(EducationGainedSkillItemVM s)
		{
			s.ResetValues();
		});
	}
}
