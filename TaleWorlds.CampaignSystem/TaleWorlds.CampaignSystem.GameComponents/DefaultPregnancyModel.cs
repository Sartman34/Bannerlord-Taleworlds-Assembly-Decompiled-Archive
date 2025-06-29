using System;
using System.Linq;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultPregnancyModel : PregnancyModel
{
	private const int MinPregnancyAge = 18;

	private const int MaxPregnancyAge = 45;

	public override float PregnancyDurationInDays => 36f;

	public override float MaternalMortalityProbabilityInLabor => 0.015f;

	public override float StillbirthProbability => 0.01f;

	public override float DeliveringFemaleOffspringProbability => 0.51f;

	public override float DeliveringTwinsProbability => 0.03f;

	private bool IsHeroAgeSuitableForPregnancy(Hero hero)
	{
		if (hero.Age >= 18f)
		{
			return hero.Age <= 45f;
		}
		return false;
	}

	public override float GetDailyChanceOfPregnancyForHero(Hero hero)
	{
		int num = hero.Children.Count + 1;
		float num2 = 4 + 4 * hero.Clan.Tier;
		int num3 = hero.Clan.Lords.Count((Hero x) => x.IsAlive);
		float num4 = ((hero != Hero.MainHero && hero.Spouse != Hero.MainHero) ? Math.Min(1f, (2f * num2 - (float)num3) / num2) : 1f);
		float num5 = (1.2f - (hero.Age - 18f) * 0.04f) / (float)(num * num) * 0.12f * num4;
		float baseNumber = ((hero.Spouse != null && IsHeroAgeSuitableForPregnancy(hero)) ? num5 : 0f);
		ExplainedNumber explainedNumber = new ExplainedNumber(baseNumber);
		if (hero.GetPerkValue(DefaultPerks.Charm.Virile) || hero.Spouse.GetPerkValue(DefaultPerks.Charm.Virile))
		{
			explainedNumber.AddFactor(DefaultPerks.Charm.Virile.PrimaryBonus, DefaultPerks.Charm.Virile.Name);
		}
		return explainedNumber.ResultNumber;
	}
}
