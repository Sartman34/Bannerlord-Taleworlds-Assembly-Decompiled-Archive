using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class PlayerProgressionModel : GameModel
{
	public abstract float GetPlayerProgress();
}
