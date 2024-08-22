using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class PartyImpairmentModel : GameModel
{
	public abstract float GetDisorganizedStateDuration(MobileParty party);

	public abstract float GetVulnerabilityStateDuration(PartyBase party);

	public abstract float GetSiegeExpectedVulnerabilityTime();

	public abstract bool CanGetDisorganized(PartyBase partyBase);
}
