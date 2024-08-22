using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class PartyTradeModel : GameModel
{
	public abstract int CaravanTransactionHighestValueItemCount { get; }

	public abstract int SmallCaravanFormingCostForPlayer { get; }

	public abstract int LargeCaravanFormingCostForPlayer { get; }

	public abstract float GetTradePenaltyFactor(MobileParty party);
}
