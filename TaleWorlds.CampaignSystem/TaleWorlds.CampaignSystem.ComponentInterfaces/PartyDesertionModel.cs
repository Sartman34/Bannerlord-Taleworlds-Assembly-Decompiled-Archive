using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class PartyDesertionModel : GameModel
{
	public abstract int GetNumberOfDeserters(MobileParty mobileParty);

	public abstract int GetMoraleThresholdForTroopDesertion(MobileParty mobileParty);
}
