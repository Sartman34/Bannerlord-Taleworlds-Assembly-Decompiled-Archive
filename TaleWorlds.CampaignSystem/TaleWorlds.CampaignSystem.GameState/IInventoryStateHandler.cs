using TaleWorlds.CampaignSystem.Inventory;

namespace TaleWorlds.CampaignSystem.GameState;

public interface IInventoryStateHandler
{
	void FilterInventoryAtOpening(InventoryManager.InventoryCategoryType inventoryCategoryType);

	void ExecuteLootingScript();

	void ExecuteSellAllLoot();

	void ExecuteBuyConsumableItem();
}
