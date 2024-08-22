using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class CompanionHiringPriceCalculationModel : GameModel
{
	public abstract int GetCompanionHiringPrice(Hero companion);
}
