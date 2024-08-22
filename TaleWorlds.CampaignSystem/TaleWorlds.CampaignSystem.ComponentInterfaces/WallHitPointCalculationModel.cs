using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class WallHitPointCalculationModel : GameModel
{
	public abstract float CalculateMaximumWallHitPoint(Town town);
}
