using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.Inventory;

public struct TransferCommand
{
	public enum CommandType
	{
		Transfer
	}

	public CommandType TypeOfCommand { get; private set; }

	public Equipment CharacterEquipment
	{
		get
		{
			if (!IsCivilianEquipment)
			{
				return Character?.FirstBattleEquipment;
			}
			return Character?.FirstCivilianEquipment;
		}
	}

	public InventoryLogic.InventorySide FromSide { get; private set; }

	public InventoryLogic.InventorySide ToSide { get; private set; }

	public EquipmentIndex FromEquipmentIndex { get; private set; }

	public EquipmentIndex ToEquipmentIndex { get; private set; }

	public int Amount { get; private set; }

	public ItemRosterElement ElementToTransfer { get; private set; }

	public CharacterObject Character { get; private set; }

	public bool IsCivilianEquipment { get; private set; }

	public static TransferCommand Transfer(int amount, InventoryLogic.InventorySide fromSide, InventoryLogic.InventorySide toSide, ItemRosterElement elementToTransfer, EquipmentIndex fromEquipmentIndex, EquipmentIndex toEquipmentIndex, CharacterObject character, bool civilianEquipment)
	{
		TransferCommand result = default(TransferCommand);
		result.TypeOfCommand = CommandType.Transfer;
		result.FromSide = fromSide;
		result.ToSide = toSide;
		result.ElementToTransfer = elementToTransfer;
		result.FromEquipmentIndex = fromEquipmentIndex;
		result.ToEquipmentIndex = toEquipmentIndex;
		result.Character = character;
		result.Amount = amount;
		result.IsCivilianEquipment = civilianEquipment;
		return result;
	}
}
