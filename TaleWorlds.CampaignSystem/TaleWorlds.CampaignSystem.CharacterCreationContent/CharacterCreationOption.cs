using System.Linq;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.CharacterCreationContent;

public class CharacterCreationOption
{
	public TextObject Text;

	public TextObject PositiveEffectText;

	public TextObject DescriptionText;

	public CharacterCreationOnCondition OnCondition;

	public CharacterCreationOnSelect OnSelect;

	public CharacterCreationApplyFinalEffects ApplyFinalEffects;

	private readonly MBList<SkillObject> _affectedSkills;

	private readonly MBList<TraitObject> _affectedTraits;

	public readonly CharacterAttribute EffectedAttribute;

	public readonly int FocusToAdd;

	public readonly int UnspentFocusToAdd;

	public readonly int UnspentAttributeToAdd;

	public readonly int SkillLevelToAdd;

	public readonly int AttributeLevelToAdd;

	public readonly int TraitLevelToAdd;

	public readonly int RenownToAdd;

	public readonly int GoldToAdd;

	public int Id { get; }

	public MBReadOnlyList<SkillObject> AffectedSkills => _affectedSkills;

	public MBReadOnlyList<TraitObject> AffectedTraits => _affectedTraits;

	public CharacterCreationOption(int id, MBList<SkillObject> affectedSkills, CharacterAttribute effectedAttribute, int focusToAdd, int skillLevelToAdd, int attributeLevelToAdd, TextObject text, CharacterCreationOnCondition onCondition, CharacterCreationOnSelect onSelect, CharacterCreationApplyFinalEffects applyFinalEffects, TextObject description = null, MBList<TraitObject> affectedTraits = null, int traitLevelToAdd = 0, int renownToAdd = 0, int goldToAdd = 0, int unspentFocusPoint = 0, int unspentAttributePoint = 0)
	{
		Id = id;
		EffectedAttribute = effectedAttribute;
		Text = text;
		OnCondition = onCondition;
		OnSelect = onSelect;
		ApplyFinalEffects = applyFinalEffects;
		DescriptionText = description ?? TextObject.Empty;
		FocusToAdd = focusToAdd;
		UnspentFocusToAdd = unspentFocusPoint;
		SkillLevelToAdd = skillLevelToAdd;
		AttributeLevelToAdd = attributeLevelToAdd;
		TraitLevelToAdd = traitLevelToAdd;
		RenownToAdd = renownToAdd;
		GoldToAdd = goldToAdd;
		UnspentAttributeToAdd = unspentAttributePoint;
		_affectedSkills = affectedSkills;
		if (affectedTraits != null)
		{
			_affectedTraits = affectedTraits;
		}
		PositiveEffectText = SetTextVariables(affectedSkills, EffectedAttribute, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, affectedTraits, TraitLevelToAdd, RenownToAdd, GoldToAdd, UnspentFocusToAdd, UnspentAttributeToAdd);
	}

	private TextObject SetTextVariables(MBList<SkillObject> skills, CharacterAttribute attribute, int focusToAdd = 0, int skillLevelToAdd = 0, int attributeLevelToAdd = 0, MBList<TraitObject> traits = null, int traitLevelToAdd = 0, int renownToAdd = 0, int goldToAdd = 0, int unspentFocustoAdd = 0, int unspentAttributeToAdd = 0)
	{
		TextObject empty = TextObject.Empty;
		if (skills.Count == 3)
		{
			empty = new TextObject("{=jeWV2uV3}{EXP_VALUE} Skill {?IS_PLURAL_SKILL}Levels{?}Level{\\?} and {FOCUS_VALUE} Focus {?IS_PLURAL_FOCUS}Points{?}Point{\\?} to {SKILL_ONE}, {SKILL_TWO} and {SKILL_THREE}{NEWLINE}{ATTR_VALUE} Attribute {?IS_PLURAL_ATR}Points{?}Point{\\?} to {ATTR_NAME}{TRAIT_DESC}{RENOWN_DESC}{GOLD_DESC}");
			empty.SetTextVariable("SKILL_ONE", skills.ElementAt(0).Name);
			empty.SetTextVariable("SKILL_TWO", skills.ElementAt(1).Name);
			empty.SetTextVariable("SKILL_THREE", skills.ElementAt(2).Name);
		}
		else if (skills.Count == 2)
		{
			empty = new TextObject("{=5JTEvvaO}{EXP_VALUE} Skill {?IS_PLURAL_SKILL}Levels{?}Level{\\?} and {FOCUS_VALUE} Focus {?IS_PLURAL_FOCUS}Points{?}Point{\\?} to {SKILL_ONE} and {SKILL_TWO}{NEWLINE}{ATTR_VALUE} Attribute {?IS_PLURAL_ATR}Points{?}Point{\\?} to {ATTR_NAME}{TRAIT_DESC}{RENOWN_DESC}{GOLD_DESC}");
			empty.SetTextVariable("SKILL_ONE", skills.ElementAt(0).Name);
			empty.SetTextVariable("SKILL_TWO", skills.ElementAt(1).Name);
		}
		else if (skills.Count == 1)
		{
			empty = new TextObject("{=uw2kKrQk}{EXP_VALUE} Skill {?IS_PLURAL_SKILL}Levels{?}Level{\\?} and {FOCUS_VALUE} Focus {?IS_PLURAL_FOCUS}Points{?}Point{\\?} to {SKILL_ONE}{NEWLINE}{ATTR_VALUE} Attribute {?IS_PLURAL_ATR}Points{?}Point{\\?} to {ATTR_NAME}{TRAIT_DESC}{RENOWN_DESC}{GOLD_DESC}");
			empty.SetTextVariable("SKILL_ONE", skills.ElementAt(0).Name);
		}
		else
		{
			empty = new TextObject("{=NDWdnpI5}{UNSPENT_FOCUS_VALUE} unspent Focus {?IS_PLURAL_FOCUS}Points{?}Point{\\?}{NEWLINE}{UNSPENT_ATTR_VALUE} unspent Attribute {?IS_PLURAL_ATR}Points{?}Point{\\?}");
		}
		empty.SetTextVariable("NEWLINE", "\n");
		if (skills.Count > 0)
		{
			empty.SetTextVariable("FOCUS_VALUE", focusToAdd);
			empty.SetTextVariable("EXP_VALUE", skillLevelToAdd);
			empty.SetTextVariable("ATTR_VALUE", attributeLevelToAdd);
			empty.SetTextVariable("IS_PLURAL_SKILL", (skillLevelToAdd > 1) ? 1 : 0);
			empty.SetTextVariable("IS_PLURAL_FOCUS", (focusToAdd > 1) ? 1 : 0);
			empty.SetTextVariable("IS_PLURAL_ATR", (attributeLevelToAdd > 1) ? 1 : 0);
		}
		else
		{
			empty.SetTextVariable("IS_PLURAL_FOCUS", (unspentFocustoAdd > 1) ? 1 : 0);
			empty.SetTextVariable("IS_PLURAL_ATR", (unspentAttributeToAdd > 1) ? 1 : 0);
		}
		if (attribute != null)
		{
			empty.SetTextVariable("ATTR_NAME", attribute.Name);
		}
		empty.SetTextVariable("UNSPENT_FOCUS_VALUE", unspentFocustoAdd);
		empty.SetTextVariable("UNSPENT_ATTR_VALUE", unspentAttributeToAdd);
		if (traits != null && traits.Count > 0 && traits.Count < 4)
		{
			TextObject textObject = TextObject.Empty;
			if (traits.Count == 1)
			{
				textObject = new TextObject("{=DuQvj7zd}\n+{VALUE} to {TRAIT_NAME}");
				textObject.SetTextVariable("TRAIT_NAME", traits.ElementAt(0).Name);
			}
			else if (traits.Count == 2)
			{
				textObject = new TextObject("{=F1syZDs4}\n+{VALUE} to {TRAIT_NAME_ONE} and {TRAIT_NAME_TWO}");
				textObject.SetTextVariable("TRAIT_NAME_ONE", traits.ElementAt(0).Name);
				textObject.SetTextVariable("TRAIT_NAME_TWO", traits.ElementAt(1).Name);
			}
			else if (traits.Count == 3)
			{
				textObject = new TextObject("{=i20baAus}\n+{VALUE} to {TRAIT_NAME_ONE}, {TRAIT_NAME_TWO} and {TRAIT_NAME_THREE}");
				textObject.SetTextVariable("TRAIT_NAME_ONE", traits.ElementAt(0).Name);
				textObject.SetTextVariable("TRAIT_NAME_TWO", traits.ElementAt(1).Name);
				textObject.SetTextVariable("TRAIT_NAME_THREE", traits.ElementAt(2).Name);
			}
			if (textObject != TextObject.Empty)
			{
				empty.SetTextVariable("TRAIT_DESC", textObject);
				textObject.SetTextVariable("VALUE", traitLevelToAdd);
			}
		}
		else
		{
			empty.SetTextVariable("TRAIT_DESC", TextObject.Empty);
		}
		if (renownToAdd > 0)
		{
			TextObject textObject2 = new TextObject("{=KXtaJNo4}\n+{VALUE} renown");
			textObject2.SetTextVariable("VALUE", renownToAdd);
			empty.SetTextVariable("RENOWN_DESC", textObject2);
		}
		else
		{
			empty.SetTextVariable("RENOWN_DESC", TextObject.Empty);
		}
		if (goldToAdd > 0)
		{
			TextObject textObject3 = new TextObject("{=YBqmnNGv}\n+{VALUE} gold");
			textObject3.SetTextVariable("VALUE", goldToAdd);
			empty.SetTextVariable("GOLD_DESC", textObject3);
		}
		else
		{
			empty.SetTextVariable("GOLD_DESC", TextObject.Empty);
		}
		return empty;
	}
}
