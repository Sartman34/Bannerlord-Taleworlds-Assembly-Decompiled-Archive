using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultCharacterDevelopmentModel : CharacterDevelopmentModel
{
	private const int MaxCharacterLevels = 62;

	private const int SkillPointsAtLevel1 = 1;

	private const int SkillPointsGainNeededInitialValue = 1000;

	private const int SkillPointsGainNeededIncreasePerLevel = 1000;

	private readonly int[] _skillsRequiredForLevel = new int[63];

	private const int FocusPointsPerLevelConst = 1;

	private const int LevelsPerAttributePointConst = 4;

	private const int FocusPointsAtStartConst = 5;

	private const int AttributePointsAtStartConst = 15;

	private const int MaxSkillLevels = 1024;

	private readonly int[] _xpRequiredForSkillLevel = new int[1024];

	private const int XpRequirementForFirstLevel = 30;

	private const int MaxSkillPoint = int.MaxValue;

	private const float BaseLearningRate = 1.25f;

	private const int TraitThreshold2 = 4000;

	private const int TraitMaxValue1 = 2500;

	private const int TraitThreshold1 = 1000;

	private const int TraitMaxValue2 = 6000;

	private const int SkillLevelVariant = 10;

	private static readonly TextObject _skillFocusText = new TextObject("{=MRktqZwu}Skill Focus");

	private static readonly TextObject _overLimitText = new TextObject("{=bcA7ZuyO}Learning Limit Exceeded");

	public override int MaxFocusPerSkill => 5;

	public override int MaxAttribute => 10;

	public override int AttributePointsAtStart => 15;

	public override int LevelsPerAttributePoint => 4;

	public override int FocusPointsPerLevel => 1;

	public override int FocusPointsAtStart => 5;

	public override int MaxSkillRequiredForEpicPerkBonus => 250;

	public override int MinSkillRequiredForEpicPerkBonus => 200;

	public DefaultCharacterDevelopmentModel()
	{
		InitializeSkillsRequiredForLevel();
		InitializeXpRequiredForSkillLevel();
	}

	public override List<Tuple<SkillObject, int>> GetSkillsDerivedFromTraits(Hero hero = null, CharacterObject templateCharacter = null, bool isByNaturalGrowth = false)
	{
		List<Tuple<SkillObject, int>> list = new List<Tuple<SkillObject, int>>();
		Occupation occupation = hero?.Occupation ?? templateCharacter.Occupation;
		if (templateCharacter == null)
		{
			templateCharacter = hero.CharacterObject;
		}
		int num = templateCharacter.GetTraitLevel(DefaultTraits.Commander);
		int num2 = templateCharacter.GetTraitLevel(DefaultTraits.Manager);
		int num3 = templateCharacter.GetTraitLevel(DefaultTraits.Trader);
		int num4 = templateCharacter.GetTraitLevel(DefaultTraits.Politician);
		int traitLevel = templateCharacter.GetTraitLevel(DefaultTraits.Siegecraft);
		int traitLevel2 = templateCharacter.GetTraitLevel(DefaultTraits.SergeantCommandSkills);
		int traitLevel3 = templateCharacter.GetTraitLevel(DefaultTraits.ScoutSkills);
		int traitLevel4 = templateCharacter.GetTraitLevel(DefaultTraits.Surgery);
		int traitLevel5 = templateCharacter.GetTraitLevel(DefaultTraits.Blacksmith);
		int num5 = templateCharacter.GetTraitLevel(DefaultTraits.RogueSkills);
		int a = templateCharacter.GetTraitLevel(DefaultTraits.Fighter);
		switch (occupation)
		{
		case Occupation.Merchant:
			a = 3;
			num2 = 6;
			num3 = 8;
			num4 = 5;
			num = 2;
			break;
		case Occupation.GangLeader:
			a = 6;
			num2 = 3;
			num3 = 3;
			num4 = 5;
			num = 3;
			num5 = 6;
			break;
		case Occupation.Artisan:
		case Occupation.Headman:
		case Occupation.RuralNotable:
			a = 4;
			num2 = 4;
			num3 = 2;
			num4 = 5;
			break;
		case Occupation.Preacher:
			a = 2;
			num4 = 7;
			break;
		}
		int item = TaleWorlds.Library.MathF.Max(num * 10 + MBRandom.RandomInt(10), traitLevel2 * 5 + MBRandom.RandomInt(10));
		int item2 = TaleWorlds.Library.MathF.Max(num * 5 + MBRandom.RandomInt(10), traitLevel2 * 10 + MBRandom.RandomInt(10));
		int num6 = num2 * 10 + MBRandom.RandomInt(10);
		int val = num3 * 10 + MBRandom.RandomInt(10);
		int item3 = traitLevel * 10 + MBRandom.RandomInt(10);
		int item4 = traitLevel3 * 10 + MBRandom.RandomInt(10);
		int item5 = num4 * 10 + MBRandom.RandomInt(10);
		int item6 = num5 * 10 + MBRandom.RandomInt(10);
		int item7 = traitLevel4 * 10 + MBRandom.RandomInt(10);
		int item8 = traitLevel5 * 10 + MBRandom.RandomInt(10);
		val = Math.Max(num6 - 20, val);
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Steward, num6));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Trade, val));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Crafting, item8));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Medicine, item7));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Roguery, item6));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Leadership, item));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Tactics, item2));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Engineering, item3));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Scouting, item4));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Charm, item5));
		a = TaleWorlds.Library.MathF.Max(a, templateCharacter.GetTraitLevel(DefaultTraits.KnightFightingSkills));
		a = TaleWorlds.Library.MathF.Max(a, templateCharacter.GetTraitLevel(DefaultTraits.CavalryFightingSkills));
		a = TaleWorlds.Library.MathF.Max(a, templateCharacter.GetTraitLevel(DefaultTraits.HorseArcherFightingSkills));
		a = TaleWorlds.Library.MathF.Max(a, templateCharacter.GetTraitLevel(DefaultTraits.HopliteFightingSkills));
		a = TaleWorlds.Library.MathF.Max(a, templateCharacter.GetTraitLevel(DefaultTraits.PeltastFightingSkills));
		a = TaleWorlds.Library.MathF.Max(a, templateCharacter.GetTraitLevel(DefaultTraits.HuscarlFightingSkills));
		a = TaleWorlds.Library.MathF.Max(a, templateCharacter.GetTraitLevel(DefaultTraits.ArcherFIghtingSkills));
		a = TaleWorlds.Library.MathF.Max(a, templateCharacter.GetTraitLevel(DefaultTraits.CrossbowmanStyle));
		int num7 = a * 30 + MBRandom.RandomInt(10);
		int num8 = a * 30 + MBRandom.RandomInt(10);
		int num9 = a * 30 + MBRandom.RandomInt(10);
		int num10 = a * 25 + MBRandom.RandomInt(10);
		int num11 = a * 20 + MBRandom.RandomInt(10);
		int num12 = a * 20 + MBRandom.RandomInt(10);
		int num13 = a * 20 + MBRandom.RandomInt(10);
		int num14 = a * 20 + MBRandom.RandomInt(10);
		if (templateCharacter.GetTraitLevel(DefaultTraits.KnightFightingSkills) > 0)
		{
			num14 += 50;
			num7 += 10;
			num9 += 20;
			num10 -= 30;
			num11 -= 30;
			num12 -= 30;
			num13 += 10;
		}
		if (templateCharacter.GetTraitLevel(DefaultTraits.CavalryFightingSkills) > 0)
		{
			num14 += 50;
			num9 += 10;
			num12 += 30;
			num10 -= 20;
			num11 -= 40;
			num8 -= 20;
			num13 -= 10;
		}
		if (templateCharacter.GetTraitLevel(DefaultTraits.HorseArcherFightingSkills) > 0)
		{
			num14 += 40;
			num10 += 30;
			num9 -= 10;
			num8 -= 30;
			num11 -= 10;
			num12 -= 10;
			num13 -= 10;
		}
		if (templateCharacter.GetTraitLevel(DefaultTraits.ArcherFIghtingSkills) > 0)
		{
			num8 -= 20;
			num9 -= 30;
			num14 -= 30;
			num11 -= 20;
			num12 -= 20;
			num10 += 60;
			num7 -= 10;
			num13 += 10;
		}
		if (templateCharacter.GetTraitLevel(DefaultTraits.HuscarlFightingSkills) > 0)
		{
			num8 += 50;
			num9 += 20;
			num10 -= 20;
			num11 -= 20;
			num12 -= 20;
			num13 += 10;
			num14 -= 20;
		}
		if (templateCharacter.GetTraitLevel(DefaultTraits.PeltastFightingSkills) > 0)
		{
			num12 += 30;
			num13 += 30;
			num7 += 10;
			num8 -= 20;
			num9 -= 20;
			num10 -= 20;
			num11 -= 20;
			num14 -= 10;
		}
		if (templateCharacter.GetTraitLevel(DefaultTraits.HopliteFightingSkills) > 0)
		{
			num9 += 20;
			num7 += 10;
			num8 -= 10;
			num13 += 10;
			num10 -= 20;
			num11 -= 20;
			num14 -= 10;
			num12 -= 20;
		}
		if (templateCharacter.GetTraitLevel(DefaultTraits.CrossbowmanStyle) > 0)
		{
			num11 += 60;
			num12 -= 20;
			num9 -= 20;
			num8 -= 10;
			num10 -= 20;
			num13 -= 10;
			num14 -= 20;
		}
		if (occupation == Occupation.Lord)
		{
			num14 += 20;
			num14 = TaleWorlds.Library.MathF.Max(num14, 100);
		}
		if (occupation == Occupation.Wanderer)
		{
			if (num7 < a * 30)
			{
				num7 = MBRandom.RandomInt(5);
			}
			if (num8 < a * 30)
			{
				num8 = MBRandom.RandomInt(5);
			}
			if (num9 < a * 30)
			{
				num9 = MBRandom.RandomInt(5);
			}
			if (num10 < a * 25)
			{
				num10 = MBRandom.RandomInt(5);
			}
			if (num11 < a * 20)
			{
				num11 = MBRandom.RandomInt(5);
			}
			if (num12 < a * 20)
			{
				num12 = MBRandom.RandomInt(5);
			}
			if (num13 < a * 20)
			{
				num13 = MBRandom.RandomInt(5);
			}
			if (num14 < a * 20)
			{
				num14 = MBRandom.RandomInt(5);
			}
		}
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.OneHanded, num7));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.TwoHanded, num8));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Polearm, num9));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Bow, num10));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Crossbow, num11));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Throwing, num12));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Athletics, num13));
		list.Add(new Tuple<SkillObject, int>(DefaultSkills.Riding, num14));
		if (hero != null)
		{
			for (int num15 = list.Count - 1; num15 >= 0; num15--)
			{
				SkillObject item9 = list[num15].Item1;
				int item10 = list[num15].Item2;
				float skillScalingModifierForAge = Campaign.Current.Models.AgeModel.GetSkillScalingModifierForAge(hero, item9, isByNaturalGrowth);
				int item11 = TaleWorlds.Library.MathF.Floor((float)item10 * skillScalingModifierForAge);
				list[num15] = new Tuple<SkillObject, int>(item9, item11);
			}
		}
		return list;
	}

	private void InitializeSkillsRequiredForLevel()
	{
		int num = 1000;
		int num2 = 1;
		_skillsRequiredForLevel[0] = 0;
		_skillsRequiredForLevel[1] = 1;
		for (int i = 2; i < _skillsRequiredForLevel.Length; i++)
		{
			num2 += num;
			_skillsRequiredForLevel[i] = num2;
			num += 1000 + num / 5;
		}
	}

	public override int SkillsRequiredForLevel(int level)
	{
		if (level > 62)
		{
			return int.MaxValue;
		}
		return _skillsRequiredForLevel[level];
	}

	public override int GetMaxSkillPoint()
	{
		return int.MaxValue;
	}

	private void InitializeXpRequiredForSkillLevel()
	{
		int num = 30;
		_xpRequiredForSkillLevel[0] = num;
		for (int i = 1; i < 1024; i++)
		{
			num += 10 + i;
			_xpRequiredForSkillLevel[i] = _xpRequiredForSkillLevel[i - 1] + num;
		}
	}

	public override int GetXpRequiredForSkillLevel(int skillLevel)
	{
		if (skillLevel > 1024)
		{
			skillLevel = 1024;
		}
		if (skillLevel <= 0)
		{
			return 0;
		}
		return _xpRequiredForSkillLevel[skillLevel - 1];
	}

	public override int GetSkillLevelChange(Hero hero, SkillObject skill, float skillXp)
	{
		int num = 0;
		int skillValue = hero.GetSkillValue(skill);
		for (int i = 0; i < 1024 - skillValue; i++)
		{
			int num2 = skillValue + i;
			if (num2 < 1023)
			{
				if (!(skillXp >= (float)_xpRequiredForSkillLevel[num2]))
				{
					break;
				}
				num++;
			}
		}
		return num;
	}

	public override int GetXpAmountForSkillLevelChange(Hero hero, SkillObject skill, int skillLevelChange)
	{
		int skillValue = hero.GetSkillValue(skill);
		return _xpRequiredForSkillLevel[skillValue + skillLevelChange] - _xpRequiredForSkillLevel[skillValue];
	}

	public override void GetTraitLevelForTraitXp(Hero hero, TraitObject trait, int xpValue, out int traitLevel, out int clampedTraitXp)
	{
		clampedTraitXp = xpValue;
		int num = ((trait.MinValue < -1) ? (-6000) : ((trait.MinValue == -1) ? (-2500) : 0));
		int num2 = ((trait.MaxValue > 1) ? 6000 : ((trait.MaxValue == 1) ? 2500 : 0));
		if (xpValue > num2)
		{
			clampedTraitXp = num2;
		}
		else if (xpValue < num)
		{
			clampedTraitXp = num;
		}
		traitLevel = ((clampedTraitXp <= -4000) ? (-2) : ((clampedTraitXp <= -1000) ? (-1) : ((clampedTraitXp >= 1000) ? ((clampedTraitXp < 4000) ? 1 : 2) : 0)));
		if (traitLevel < trait.MinValue)
		{
			traitLevel = trait.MinValue;
		}
		else if (traitLevel > trait.MaxValue)
		{
			traitLevel = trait.MaxValue;
		}
	}

	public override int GetTraitXpRequiredForTraitLevel(TraitObject trait, int traitLevel)
	{
		if (traitLevel >= -1)
		{
			return traitLevel switch
			{
				1 => 1000, 
				0 => 0, 
				-1 => -1000, 
				_ => 4000, 
			};
		}
		return -4000;
	}

	public override ExplainedNumber CalculateLearningLimit(int attributeValue, int focusValue, TextObject attributeName, bool includeDescriptions = false)
	{
		ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions);
		result.Add((attributeValue - 1) * 10, attributeName);
		result.Add(focusValue * 30, _skillFocusText);
		result.LimitMin(0f);
		return result;
	}

	public override float CalculateLearningRate(Hero hero, SkillObject skill)
	{
		int level = hero.Level;
		int attributeValue = hero.GetAttributeValue(skill.CharacterAttribute);
		int focus = hero.HeroDeveloper.GetFocus(skill);
		int skillValue = hero.GetSkillValue(skill);
		return CalculateLearningRate(attributeValue, focus, skillValue, level, skill.CharacterAttribute.Name).ResultNumber;
	}

	public override ExplainedNumber CalculateLearningRate(int attributeValue, int focusValue, int skillValue, int characterLevel, TextObject attributeName, bool includeDescriptions = false)
	{
		ExplainedNumber result = new ExplainedNumber(1.25f, includeDescriptions);
		result.AddFactor(0.4f * (float)attributeValue, attributeName);
		result.AddFactor((float)focusValue * 1f, _skillFocusText);
		int num = TaleWorlds.Library.MathF.Round(CalculateLearningLimit(attributeValue, focusValue, null).ResultNumber);
		if (skillValue > num)
		{
			int num2 = skillValue - num;
			result.AddFactor(-1f - 0.1f * (float)num2, _overLimitText);
		}
		result.LimitMin(0f);
		return result;
	}

	public override SkillObject GetNextSkillToAddFocus(Hero hero)
	{
		SkillObject result = null;
		float num = float.MinValue;
		foreach (SkillObject item in Skills.All)
		{
			if (hero.HeroDeveloper.CanAddFocusToSkill(item))
			{
				int attributeValue = hero.GetAttributeValue(item.CharacterAttribute);
				int focus = hero.HeroDeveloper.GetFocus(item);
				float num2 = (float)hero.GetSkillValue(item) - CalculateLearningLimit(attributeValue, focus, null).ResultNumber;
				if (num2 > num)
				{
					num = num2;
					result = item;
				}
			}
		}
		return result;
	}

	public override CharacterAttribute GetNextAttributeToUpgrade(Hero hero)
	{
		CharacterAttribute result = null;
		float num = float.MinValue;
		foreach (CharacterAttribute item in Attributes.All)
		{
			int attributeValue = hero.GetAttributeValue(item);
			if (attributeValue >= MaxAttribute)
			{
				continue;
			}
			float num2 = 0f;
			if (attributeValue == 0)
			{
				num2 = float.MaxValue;
			}
			else
			{
				foreach (SkillObject skill in item.Skills)
				{
					float num3 = TaleWorlds.Library.MathF.Max(0f, (float)(75 + hero.GetSkillValue(skill)) - CalculateLearningLimit(attributeValue, hero.HeroDeveloper.GetFocus(skill), null).ResultNumber);
					num2 += num3;
				}
				int num4 = 1;
				foreach (CharacterAttribute item2 in Attributes.All)
				{
					if (item2 != item)
					{
						int attributeValue2 = hero.GetAttributeValue(item2);
						if (num4 < attributeValue2)
						{
							num4 = attributeValue2;
						}
					}
				}
				float num5 = TaleWorlds.Library.MathF.Sqrt((float)num4 / (float)attributeValue);
				num2 *= num5;
			}
			if (num2 > num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	public override PerkObject GetNextPerkToChoose(Hero hero, PerkObject perk)
	{
		PerkObject result = perk;
		if (perk.AlternativePerk != null && MBRandom.RandomFloat < 0.5f)
		{
			result = perk.AlternativePerk;
		}
		return result;
	}
}
