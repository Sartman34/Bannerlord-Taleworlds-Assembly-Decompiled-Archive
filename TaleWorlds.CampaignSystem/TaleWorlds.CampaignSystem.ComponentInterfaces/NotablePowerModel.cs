using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class NotablePowerModel : GameModel
{
	public abstract int RegularNotableMaxPowerLevel { get; }

	public abstract int NotableDisappearPowerLimit { get; }

	public abstract ExplainedNumber CalculateDailyPowerChangeForHero(Hero hero, bool includeDescriptions = false);

	public abstract TextObject GetPowerRankName(Hero hero);

	public abstract float GetInfluenceBonusToClan(Hero hero);

	public abstract int GetInitialPower();
}
