using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class SettlementProsperityModel : GameModel
{
	public abstract ExplainedNumber CalculateProsperityChange(Town fortification, bool includeDescriptions = false);

	public abstract ExplainedNumber CalculateHearthChange(Village village, bool includeDescriptions = false);
}
