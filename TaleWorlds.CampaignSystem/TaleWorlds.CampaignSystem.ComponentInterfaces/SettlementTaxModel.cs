using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class SettlementTaxModel : GameModel
{
	public abstract float SettlementCommissionRateTown { get; }

	public abstract float SettlementCommissionRateVillage { get; }

	public abstract int SettlementCommissionDecreaseSecurityThreshold { get; }

	public abstract int MaximumDecreaseBasedOnSecuritySecurity { get; }

	public abstract float GetTownTaxRatio(Town town);

	public abstract float GetVillageTaxRatio();

	public abstract float GetTownCommissionChangeBasedOnSecurity(Town town, float commission);

	public abstract ExplainedNumber CalculateTownTax(Town town, bool includeDescriptions = false);

	public abstract int CalculateVillageTaxFromIncome(Village village, int marketIncome);
}
