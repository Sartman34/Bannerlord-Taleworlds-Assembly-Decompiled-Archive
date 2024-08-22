using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.GameState;

public class InventoryState : PlayerGameState
{
	private IInventoryStateHandler _handler;

	public override bool IsMenuState => true;

	public InventoryLogic InventoryLogic { get; private set; }

	public IInventoryStateHandler Handler
	{
		get
		{
			return _handler;
		}
		set
		{
			_handler = value;
		}
	}

	public void InitializeLogic(InventoryLogic inventoryLogic)
	{
		InventoryLogic = inventoryLogic;
	}
}
