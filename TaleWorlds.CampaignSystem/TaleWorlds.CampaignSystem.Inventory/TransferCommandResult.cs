using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.Inventory;

public class TransferCommandResult
{
	public CharacterObject TransferCharacter { get; private set; }

	public bool IsCivilianEquipment { get; private set; }

	public Equipment TransferEquipment
	{
		get
		{
			if (!IsCivilianEquipment)
			{
				return TransferCharacter?.FirstBattleEquipment;
			}
			return TransferCharacter?.FirstCivilianEquipment;
		}
	}

	public InventoryLogic.InventorySide ResultSide { get; private set; }

	public ItemRosterElement EffectedItemRosterElement { get; private set; }

	public int EffectedNumber { get; private set; }

	public int FinalNumber { get; private set; }

	public EquipmentIndex EffectedEquipmentIndex { get; private set; }

	public TransferCommandResult()
	{
	}

	public TransferCommandResult(InventoryLogic.InventorySide resultSide, ItemRosterElement effectedItemRosterElement, int effectedNumber, int finalNumber, EquipmentIndex effectedEquipmentIndex, CharacterObject transferCharacter, bool isCivilianEquipment)
	{
		ResultSide = resultSide;
		EffectedItemRosterElement = effectedItemRosterElement;
		EffectedNumber = effectedNumber;
		FinalNumber = finalNumber;
		EffectedEquipmentIndex = effectedEquipmentIndex;
		TransferCharacter = transferCharacter;
		IsCivilianEquipment = isCivilianEquipment;
	}
}
