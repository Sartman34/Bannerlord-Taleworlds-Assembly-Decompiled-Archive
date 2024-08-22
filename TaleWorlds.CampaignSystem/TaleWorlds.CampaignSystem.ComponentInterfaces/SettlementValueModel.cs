using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class SettlementValueModel : GameModel
{
	public abstract float CalculateSettlementValueForFaction(Settlement settlement, IFaction faction);

	public abstract float CalculateSettlementBaseValue(Settlement settlement);

	public abstract float CalculateSettlementValueForEnemyHero(Settlement settlement, Hero hero);
}
