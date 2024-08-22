using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class RaidModel : GameModel
{
	public abstract int GoldRewardForEachLostHearth { get; }

	public abstract MBReadOnlyList<(ItemObject, float)> GetCommonLootItemScores();

	public abstract float CalculateHitDamage(MapEventSide attackerSide, float settlementHitPoints);
}
