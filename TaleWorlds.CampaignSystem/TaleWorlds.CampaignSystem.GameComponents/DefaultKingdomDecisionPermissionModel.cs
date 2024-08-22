using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultKingdomDecisionPermissionModel : KingdomDecisionPermissionModel
{
	public override bool IsPolicyDecisionAllowed(PolicyObject policy)
	{
		return true;
	}

	public override bool IsWarDecisionAllowedBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2, out TextObject reason)
	{
		reason = TextObject.Empty;
		return true;
	}

	public override bool IsPeaceDecisionAllowedBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2, out TextObject reason)
	{
		reason = TextObject.Empty;
		return true;
	}

	public override bool IsAnnexationDecisionAllowed(Settlement annexedSettlement)
	{
		return true;
	}

	public override bool IsExpulsionDecisionAllowed(Clan expelledClan)
	{
		return true;
	}

	public override bool IsKingSelectionDecisionAllowed(Kingdom kingdom)
	{
		return true;
	}
}
