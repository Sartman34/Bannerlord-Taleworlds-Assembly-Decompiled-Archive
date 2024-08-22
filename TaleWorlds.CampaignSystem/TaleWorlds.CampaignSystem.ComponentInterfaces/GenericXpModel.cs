using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class GenericXpModel : GameModel
{
	public abstract float GetXpMultiplier(Hero hero);
}
