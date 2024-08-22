using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class SettlementFoodModel : GameModel
{
	public abstract int FoodStocksUpperLimit { get; }

	public abstract int NumberOfProsperityToEatOneFood { get; }

	public abstract int NumberOfMenOnGarrisonToEatOneFood { get; }

	public abstract int CastleFoodStockUpperLimitBonus { get; }

	public abstract ExplainedNumber CalculateTownFoodStocksChange(Town town, bool includeMarketStocks = true, bool includeDescriptions = false);
}
