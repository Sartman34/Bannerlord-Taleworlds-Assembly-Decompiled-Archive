using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class SettlementGarrisonModel : GameModel
{
	public abstract ExplainedNumber CalculateGarrisonChange(Settlement settlement, bool includeDescriptions = false);

	public abstract ExplainedNumber CalculateGarrisonChangeAutoRecruitment(Settlement settlement, bool includeDescriptions = false);

	public abstract int FindNumberOfTroopsToTakeFromGarrison(MobileParty mobileParty, Settlement settlement, float idealGarrisonStrengthPerWalledCenter = 0f);

	public abstract int FindNumberOfTroopsToLeaveToGarrison(MobileParty mobileParty, Settlement settlement);
}
