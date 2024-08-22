using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class DailyTroopXpBonusModel : GameModel
{
	public abstract int CalculateDailyTroopXpBonus(Town town);

	public abstract float CalculateGarrisonXpBonusMultiplier(Town town);
}
