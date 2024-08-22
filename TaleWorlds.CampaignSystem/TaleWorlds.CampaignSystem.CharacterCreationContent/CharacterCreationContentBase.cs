using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.CampaignSystem.CharacterCreationContent;

public abstract class CharacterCreationContentBase
{
	private CultureObject _culture;

	private Banner _banner;

	public static CharacterCreationContentBase Instance => (GameStateManager.Current.ActiveState as CharacterCreationState)?.CurrentCharacterCreationContent;

	protected virtual int ChildhoodAge => 7;

	protected virtual int EducationAge => 12;

	protected virtual int YouthAge => 17;

	protected virtual int AccomplishmentAge => 20;

	protected virtual int FocusToAdd => 1;

	protected virtual int SkillLevelToAdd => 10;

	protected virtual int AttributeLevelToAdd => 1;

	public virtual int FocusToAddByCulture => 1;

	public virtual int SkillLevelToAddByCulture => 10;

	protected int SelectedParentType { get; set; } = 1;


	protected int SelectedTitleType { get; set; }

	public abstract TextObject ReviewPageDescription { get; }

	protected FaceGenChar MotherFacegenCharacter { get; set; }

	protected FaceGenChar FatherFacegenCharacter { get; set; }

	protected BodyProperties PlayerBodyProperties { get; set; }

	protected Equipment PlayerStartEquipment { get; set; }

	protected Equipment PlayerCivilianEquipment { get; set; }

	public abstract IEnumerable<Type> CharacterCreationStages { get; }

	public void Initialize(CharacterCreation characterCreation)
	{
		OnInitialized(characterCreation);
		SetMainHeroInitialStats();
	}

	protected virtual void OnInitialized(CharacterCreation characterCreation)
	{
	}

	public void ApplySkillAndAttributeEffects(List<SkillObject> skills, int focusToAdd, int skillLevelToAdd, CharacterAttribute attribute, int attributeLevelToAdd, List<TraitObject> traits = null, int traitLevelToAdd = 0, int renownToAdd = 0, int goldToAdd = 0, int unspentFocusPoints = 0, int unspentAttributePoints = 0)
	{
		foreach (SkillObject skill in skills)
		{
			Hero.MainHero.HeroDeveloper.AddFocus(skill, focusToAdd, checkUnspentFocusPoints: false);
			if (Hero.MainHero.GetSkillValue(skill) == 1)
			{
				Hero.MainHero.HeroDeveloper.ChangeSkillLevel(skill, skillLevelToAdd - 1, shouldNotify: false);
			}
			else
			{
				Hero.MainHero.HeroDeveloper.ChangeSkillLevel(skill, skillLevelToAdd, shouldNotify: false);
			}
		}
		Hero.MainHero.HeroDeveloper.UnspentFocusPoints += unspentFocusPoints;
		Hero.MainHero.HeroDeveloper.UnspentAttributePoints += unspentAttributePoints;
		if (attribute != null)
		{
			Hero.MainHero.HeroDeveloper.AddAttribute(attribute, attributeLevelToAdd, checkUnspentPoints: false);
		}
		if (traits != null && traitLevelToAdd > 0 && traits.Count > 0)
		{
			foreach (TraitObject trait in traits)
			{
				Hero.MainHero.SetTraitLevel(trait, Hero.MainHero.GetTraitLevel(trait) + traitLevelToAdd);
			}
		}
		if (renownToAdd > 0)
		{
			GainRenownAction.Apply(Hero.MainHero, renownToAdd, doNotNotify: true);
		}
		if (goldToAdd > 0)
		{
			GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, goldToAdd, disableNotification: true);
		}
		Hero.MainHero.HeroDeveloper.SetInitialLevel(1);
	}

	public void SetPlayerBanner(Banner banner)
	{
		_banner = banner;
	}

	public Banner GetCurrentPlayerBanner()
	{
		return _banner;
	}

	public void SetSelectedCulture(CultureObject culture, CharacterCreation characterCreation)
	{
		_culture = culture;
		characterCreation.ResetMenuOptions();
		OnCultureSelected();
	}

	public void ApplyCulture(CharacterCreation characterCreation)
	{
		CharacterObject.PlayerCharacter.Culture = Instance._culture;
		Clan.PlayerClan.Culture = Instance._culture;
		Clan.PlayerClan.UpdateHomeSettlement(null);
		Hero.MainHero.BornSettlement = Clan.PlayerClan.HomeSettlement;
		OnApplyCulture();
	}

	public IEnumerable<CultureObject> GetCultures()
	{
		foreach (CultureObject objectType in MBObjectManager.Instance.GetObjectTypeList<CultureObject>())
		{
			if (objectType.IsMainCulture)
			{
				yield return objectType;
			}
		}
	}

	public CultureObject GetSelectedCulture()
	{
		return _culture;
	}

	public virtual int GetSelectedParentType()
	{
		return 0;
	}

	protected virtual void OnApplyCulture()
	{
	}

	protected virtual void OnCultureSelected()
	{
	}

	public virtual void OnCharacterCreationFinalized()
	{
	}

	private void SetMainHeroInitialStats()
	{
		Hero.MainHero.HeroDeveloper.ClearHero();
		Hero.MainHero.HitPoints = 100;
		foreach (SkillObject item in Skills.All)
		{
			Hero.MainHero.HeroDeveloper.InitializeSkillXp(item);
		}
		foreach (CharacterAttribute item2 in Attributes.All)
		{
			Hero.MainHero.HeroDeveloper.AddAttribute(item2, 2, checkUnspentPoints: false);
		}
	}
}
