using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;

public class CharacterCreationGainedPropertiesVM : ViewModel
{
	private readonly TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation _characterCreation;

	private readonly int _currentIndex;

	private readonly Dictionary<CharacterAttribute, Tuple<int, int>> _affectedAttributesMap;

	private readonly Dictionary<SkillObject, Tuple<int, int>> _affectedSkillMap;

	private MBBindingList<CharacterCreationGainGroupItemVM> _gainGroups;

	private MBBindingList<EncyclopediaTraitItemVM> _gainedTraits;

	[DataSourceProperty]
	public MBBindingList<CharacterCreationGainGroupItemVM> GainGroups
	{
		get
		{
			return _gainGroups;
		}
		set
		{
			if (value != _gainGroups)
			{
				_gainGroups = value;
				OnPropertyChangedWithValue(value, "GainGroups");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<EncyclopediaTraitItemVM> GainedTraits
	{
		get
		{
			return _gainedTraits;
		}
		set
		{
			if (value != _gainedTraits)
			{
				_gainedTraits = value;
				OnPropertyChangedWithValue(value, "GainedTraits");
			}
		}
	}

	public CharacterCreationGainedPropertiesVM(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation characterCreation, int currentIndex)
	{
		_characterCreation = characterCreation;
		_currentIndex = currentIndex;
		_affectedAttributesMap = new Dictionary<CharacterAttribute, Tuple<int, int>>();
		_affectedSkillMap = new Dictionary<SkillObject, Tuple<int, int>>();
		GainGroups = new MBBindingList<CharacterCreationGainGroupItemVM>();
		foreach (CharacterAttribute item in Attributes.All)
		{
			GainGroups.Add(new CharacterCreationGainGroupItemVM(item, _characterCreation, _currentIndex));
		}
		GainedTraits = new MBBindingList<EncyclopediaTraitItemVM>();
		UpdateValues();
	}

	public void UpdateValues()
	{
		_affectedAttributesMap.Clear();
		_affectedSkillMap.Clear();
		GainGroups.ApplyActionOnAllItems(delegate(CharacterCreationGainGroupItemVM g)
		{
			g.ResetValues();
		});
		PopulateInitialValues();
		PopulateGainedAttributeValues();
		PopulateGainedTraitValues();
		foreach (KeyValuePair<CharacterAttribute, Tuple<int, int>> item in _affectedAttributesMap)
		{
			GetItemFromAttribute(item.Key).SetValue(item.Value.Item1, item.Value.Item2);
		}
		foreach (KeyValuePair<SkillObject, Tuple<int, int>> item2 in _affectedSkillMap)
		{
			GetItemFromSkill(item2.Key).SetValue(item2.Value.Item1, item2.Value.Item2);
		}
	}

	private void PopulateInitialValues()
	{
		foreach (SkillObject item in Skills.All)
		{
			int focus = Hero.MainHero.HeroDeveloper.GetFocus(item);
			if (_affectedSkillMap.ContainsKey(item))
			{
				Tuple<int, int> tuple = _affectedSkillMap[item];
				_affectedSkillMap[item] = new Tuple<int, int>(tuple.Item1 + focus, 0);
			}
			else
			{
				_affectedSkillMap.Add(item, new Tuple<int, int>(focus, 0));
			}
		}
		foreach (CharacterAttribute item2 in Attributes.All)
		{
			int attributeValue = Hero.MainHero.GetAttributeValue(item2);
			if (_affectedAttributesMap.ContainsKey(item2))
			{
				Tuple<int, int> tuple2 = _affectedAttributesMap[item2];
				_affectedAttributesMap[item2] = new Tuple<int, int>(tuple2.Item1 + attributeValue, 0);
			}
			else
			{
				_affectedAttributesMap.Add(item2, new Tuple<int, int>(attributeValue, 0));
			}
		}
	}

	private void PopulateGainedAttributeValues()
	{
		for (int i = 0; i < _characterCreation.CharacterCreationMenuCount; i++)
		{
			int selectedOptionId = (_characterCreation.GetSelectedOptions(i).Any() ? _characterCreation.GetSelectedOptions(i).First() : (-1));
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			if (selectedOptionId == -1)
			{
				continue;
			}
			CharacterCreationOption characterCreationOption = _characterCreation.GetCurrentMenuOptions(i).SingleOrDefault((CharacterCreationOption o) => o.Id == selectedOptionId);
			if (characterCreationOption == null)
			{
				continue;
			}
			if (i == _currentIndex)
			{
				num3 = characterCreationOption.AttributeLevelToAdd;
			}
			else
			{
				num4 += characterCreationOption.AttributeLevelToAdd;
			}
			if (characterCreationOption.EffectedAttribute != null)
			{
				if (_affectedAttributesMap.ContainsKey(characterCreationOption.EffectedAttribute))
				{
					Tuple<int, int> tuple = _affectedAttributesMap[characterCreationOption.EffectedAttribute];
					_affectedAttributesMap[characterCreationOption.EffectedAttribute] = new Tuple<int, int>(tuple.Item1 + num4, tuple.Item2 + num3);
				}
				else
				{
					_affectedAttributesMap.Add(characterCreationOption.EffectedAttribute, new Tuple<int, int>(num4, num3));
				}
			}
			if (i == _currentIndex)
			{
				num = characterCreationOption.FocusToAdd;
			}
			else
			{
				num2 += characterCreationOption.FocusToAdd;
			}
			foreach (SkillObject affectedSkill in characterCreationOption.AffectedSkills)
			{
				if (_affectedSkillMap.ContainsKey(affectedSkill))
				{
					Tuple<int, int> tuple2 = _affectedSkillMap[affectedSkill];
					_affectedSkillMap[affectedSkill] = new Tuple<int, int>(tuple2.Item1 + num2, tuple2.Item2 + num);
				}
				else
				{
					_affectedSkillMap.Add(affectedSkill, new Tuple<int, int>(num2, num));
				}
			}
		}
	}

	private void PopulateGainedTraitValues()
	{
		GainedTraits.Clear();
		for (int i = 0; i < _characterCreation.CharacterCreationMenuCount; i++)
		{
			int selectedOptionId = (_characterCreation.GetSelectedOptions(i).Any() ? _characterCreation.GetSelectedOptions(i).First() : (-1));
			if (selectedOptionId == -1)
			{
				continue;
			}
			CharacterCreationOption characterCreationOption = _characterCreation.GetCurrentMenuOptions(i).SingleOrDefault((CharacterCreationOption o) => o.Id == selectedOptionId);
			if (characterCreationOption?.AffectedTraits == null || characterCreationOption.AffectedTraits.Count <= 0)
			{
				continue;
			}
			foreach (TraitObject effectedTrait in characterCreationOption.AffectedTraits)
			{
				if (GainedTraits.FirstOrDefault((EncyclopediaTraitItemVM t) => t.TraitId == effectedTrait.StringId) == null)
				{
					GainedTraits.Add(new EncyclopediaTraitItemVM(effectedTrait, 1));
				}
			}
		}
	}

	private CharacterCreationGainedAttributeItemVM GetItemFromAttribute(CharacterAttribute attribute)
	{
		return GainGroups.SingleOrDefault((CharacterCreationGainGroupItemVM g) => g.AttributeObj == attribute)?.Attribute;
	}

	private CharacterCreationGainedSkillItemVM GetItemFromSkill(SkillObject skill)
	{
		return GainGroups.SingleOrDefault((CharacterCreationGainGroupItemVM g) => g.Skills.SingleOrDefault((CharacterCreationGainedSkillItemVM s) => s.SkillObj == skill) != null)?.Skills.SingleOrDefault((CharacterCreationGainedSkillItemVM s) => s.SkillObj == skill);
	}
}
