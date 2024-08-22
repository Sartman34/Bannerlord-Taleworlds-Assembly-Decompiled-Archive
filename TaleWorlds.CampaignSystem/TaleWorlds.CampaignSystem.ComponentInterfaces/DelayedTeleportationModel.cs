using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class DelayedTeleportationModel : GameModel
{
	public abstract float DefaultTeleportationSpeed { get; }

	public abstract ExplainedNumber GetTeleportationDelayAsHours(Hero teleportingHero, PartyBase target);
}
