using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class TroopSacrificeModel : GameModel
{
	public abstract int BreakOutArmyLeaderRelationPenalty { get; }

	public abstract int BreakOutArmyMemberRelationPenalty { get; }

	public abstract int GetLostTroopCountForBreakingInBesiegedSettlement(MobileParty party, SiegeEvent siegeEvent);

	public abstract int GetLostTroopCountForBreakingOutOfBesiegedSettlement(MobileParty party, SiegeEvent siegeEvent);

	public abstract int GetNumberOfTroopsSacrificedForTryingToGetAway(BattleSideEnum battleSide, MapEvent mapEvent);
}
