using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class PartySizeLimitModel : GameModel
{
	public abstract ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false);

	public abstract ExplainedNumber GetPartyPrisonerSizeLimit(PartyBase party, bool includeDescriptions = false);

	public abstract int GetTierPartySizeEffect(int tier);

	public abstract int GetAssumedPartySizeForLordParty(Hero leaderHero, IFaction partyMapFaction, Clan actualClan);
}
