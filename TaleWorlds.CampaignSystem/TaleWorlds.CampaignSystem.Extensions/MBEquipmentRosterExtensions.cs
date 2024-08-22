using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.Extensions;

public static class MBEquipmentRosterExtensions
{
	public static MBReadOnlyList<MBEquipmentRoster> All => Campaign.Current.AllEquipmentRosters;

	public static IEnumerable<Equipment> GetCivilianEquipments(this MBEquipmentRoster instance)
	{
		return instance.AllEquipments.Where((Equipment x) => x.IsCivilian);
	}

	public static IEnumerable<Equipment> GetBattleEquipments(this MBEquipmentRoster instance)
	{
		return instance.AllEquipments.Where((Equipment x) => !x.IsCivilian);
	}

	public static Equipment GetRandomCivilianEquipment(this MBEquipmentRoster instance)
	{
		return instance.AllEquipments.GetRandomElementWithPredicate((Equipment x) => x.IsCivilian);
	}
}
