using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class SettlementEconomyModel : GameModel
{
	public abstract float GetEstimatedDemandForCategory(Town town, ItemData itemData, ItemCategory category);

	public abstract float GetDailyDemandForCategory(Town town, ItemCategory category, int extraProsperity = 0);

	public abstract float GetDemandChangeFromValue(float purchaseValue);

	public abstract (float, float) GetSupplyDemandForCategory(Town town, ItemCategory category, float dailySupply, float dailyDemand, float oldSupply, float oldDemand);

	public abstract int GetTownGoldChange(Town town);
}
