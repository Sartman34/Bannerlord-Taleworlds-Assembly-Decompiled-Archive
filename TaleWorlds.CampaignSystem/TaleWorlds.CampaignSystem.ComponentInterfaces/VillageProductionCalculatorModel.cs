using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class VillageProductionCalculatorModel : GameModel
{
	public abstract float CalculateProductionSpeedOfItemCategory(ItemCategory item);

	public abstract float CalculateDailyProductionAmount(Village village, ItemObject item);

	public abstract float CalculateDailyFoodProductionAmount(Village village);
}
