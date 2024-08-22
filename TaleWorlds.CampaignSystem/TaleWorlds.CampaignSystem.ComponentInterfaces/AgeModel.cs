using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class AgeModel : GameModel
{
	public abstract int BecomeInfantAge { get; }

	public abstract int BecomeChildAge { get; }

	public abstract int BecomeTeenagerAge { get; }

	public abstract int HeroComesOfAge { get; }

	public abstract int BecomeOldAge { get; }

	public abstract int MaxAge { get; }

	public abstract void GetAgeLimitForLocation(CharacterObject character, out int minimumAge, out int maximumAge, string additionalTags = "");

	public abstract float GetSkillScalingModifierForAge(Hero hero, SkillObject skill, bool isByNaturalGrowth);
}
