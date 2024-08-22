using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class HeroDeathProbabilityCalculationModel : GameModel
{
	public abstract float CalculateHeroDeathProbability(Hero hero);
}
