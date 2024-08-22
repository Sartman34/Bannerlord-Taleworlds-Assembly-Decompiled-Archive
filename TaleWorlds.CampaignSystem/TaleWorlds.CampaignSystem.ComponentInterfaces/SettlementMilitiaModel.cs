using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class SettlementMilitiaModel : GameModel
{
	public abstract int MilitiaToSpawnAfterSiege(Town town);

	public abstract ExplainedNumber CalculateMilitiaChange(Settlement settlement, bool includeDescriptions = false);

	public abstract float CalculateEliteMilitiaSpawnChance(Settlement settlement);

	public abstract void CalculateMilitiaSpawnRate(Settlement settlement, out float meleeTroopRate, out float rangedTroopRate);
}
