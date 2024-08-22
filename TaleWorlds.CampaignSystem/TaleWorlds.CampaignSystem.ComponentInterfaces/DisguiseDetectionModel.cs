using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class DisguiseDetectionModel : GameModel
{
	public abstract float CalculateDisguiseDetectionProbability(Settlement settlement);
}
