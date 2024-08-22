using System;

namespace TaleWorlds.CampaignSystem.Inventory;

[Flags]
public enum InventoryItemType
{
	None = 0,
	Weapon = 1,
	Shield = 2,
	HeadArmor = 4,
	BodyArmor = 8,
	LegArmor = 0x10,
	HandArmor = 0x20,
	Horse = 0x40,
	HorseHarness = 0x80,
	Goods = 0x100,
	Book = 0x200,
	Animal = 0x400,
	Cape = 0x800,
	Banner = 0x1000,
	HorseCategory = 0xC0,
	Armors = 0x83C,
	Equipable = 0x18FF,
	All = 0xFFF
}
