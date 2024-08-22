using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MBHelpers;

public static class BannerHelper
{
	public static void AddBannerBonusForBanner(BannerEffect bannerEffect, BannerComponent bannerComponent, ref FactoredNumber bonuses)
	{
		if (bannerComponent != null && bannerComponent.BannerEffect == bannerEffect)
		{
			AddBannerEffectToStat(ref bonuses, bannerEffect.IncrementType, bannerComponent.GetBannerEffectBonus());
		}
	}

	private static void AddBannerEffectToStat(ref FactoredNumber stat, BannerEffect.EffectIncrementType effectIncrementType, float number)
	{
		switch (effectIncrementType)
		{
		case BannerEffect.EffectIncrementType.Add:
			stat.Add(number);
			break;
		case BannerEffect.EffectIncrementType.AddFactor:
			stat.AddFactor(number);
			break;
		}
	}
}
