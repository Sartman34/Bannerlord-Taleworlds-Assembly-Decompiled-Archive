using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Helpers;

public static class EquipmentHelper
{
	public static void AssignHeroEquipmentFromEquipment(Hero hero, Equipment equipment)
	{
		Equipment equipment2 = (equipment.IsCivilian ? hero.CivilianEquipment : hero.BattleEquipment);
		for (int i = 0; i < 12; i++)
		{
			equipment2[i] = new EquipmentElement(equipment[i].Item, equipment[i].ItemModifier);
		}
	}
}
