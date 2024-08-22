using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.CharacterDevelopment;

public interface IHeroDeveloper
{
	int UnspentFocusPoints { get; set; }

	int UnspentAttributePoints { get; set; }

	int TotalXp { get; }

	Hero Hero { get; }

	void SetInitialLevel(int i);

	void AddSkillXp(SkillObject skill, float rawXp, bool isAffectedByFocusFactor = true, bool shouldNotify = true);

	void InitializeHeroDeveloper(bool isByNaturalGrowth = false, CharacterObject template = null);

	void ClearUnspentPoints();

	void AddFocus(SkillObject skill, int changeAmount, bool checkUnspentFocusPoints = true);

	void RemoveFocus(SkillObject skill, int changeAmount);

	void SetInitialSkillLevel(SkillObject skill, int newSkillValue);

	void InitializeSkillXp(SkillObject skill);

	void ClearHero();

	void AddPerk(PerkObject perk);

	float GetFocusFactor(SkillObject skill);

	void AddAttribute(CharacterAttribute attribute, int changeAmount, bool checkUnspentPoints = true);

	void RemoveAttribute(CharacterAttribute attrib, int changeAmount);

	void ChangeSkillLevel(SkillObject skill, int changeAmount, bool shouldNotify = true);

	int GetFocus(SkillObject skill);

	bool CanAddFocusToSkill(SkillObject skill);

	void AfterLoad();

	int GetTotalSkillPoints();

	int GetXpRequiredForLevel(int level);

	int GetRequiredFocusPointsToAddFocus(SkillObject skill);

	int GetSkillXpProgress(SkillObject skill);

	bool GetPerkValue(PerkObject perk);

	void DevelopCharacterStats();
}
