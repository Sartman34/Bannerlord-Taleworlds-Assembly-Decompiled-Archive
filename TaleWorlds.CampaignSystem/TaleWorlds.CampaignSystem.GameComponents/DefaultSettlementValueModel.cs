using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultSettlementValueModel : SettlementValueModel
{
	private const float BenefitRatioForFaction = 0.33f;

	private const float CastleMultiplier = 1.25f;

	private const float SameMapFactionMultiplier = 1.1f;

	private const float SameCultureMultiplier = 1.1f;

	private const float BeingOwnerMultiplier = 1.1f;

	public override float CalculateSettlementBaseValue(Settlement settlement)
	{
		float num = (settlement.IsCastle ? 1.25f : 1f);
		float value = settlement.GetValue();
		Settlement settlement2 = (settlement.IsVillage ? settlement.Village.Bound : settlement);
		float baseGeographicalAdvantage = GetBaseGeographicalAdvantage(settlement2);
		return num * value * baseGeographicalAdvantage * 0.33f;
	}

	public override float CalculateSettlementValueForFaction(Settlement settlement, IFaction faction)
	{
		float num = (settlement.IsCastle ? 1.25f : 1f);
		float num2 = ((settlement.MapFaction == faction.MapFaction) ? 1.1f : 1f);
		float num3 = ((settlement.Culture == faction?.Culture) ? 1.1f : 1f);
		float value = settlement.GetValue();
		Settlement settlement2 = (settlement.IsVillage ? settlement.Village.Bound : settlement);
		float num4 = GeographicalAdvantageForFaction(settlement2, faction);
		return value * num * num2 * num3 * num4 * 0.33f;
	}

	public override float CalculateSettlementValueForEnemyHero(Settlement settlement, Hero hero)
	{
		float num = (settlement.IsCastle ? 1.25f : 1f);
		float num2 = ((settlement.OwnerClan == hero.Clan) ? 1.1f : 1f);
		float num3 = ((settlement.Culture == hero.Culture) ? 1.1f : 1f);
		float value = settlement.GetValue();
		Settlement settlement2 = (settlement.IsVillage ? settlement.Village.Bound : settlement);
		float num4 = GeographicalAdvantageForFaction(settlement2, hero.MapFaction);
		return value * num * num3 * num2 * num4 * 0.33f;
	}

	private float GetBaseGeographicalAdvantage(Settlement settlement)
	{
		float num = Campaign.Current.Models.MapDistanceModel.GetDistance(settlement.MapFaction.FactionMidSettlement, settlement) / Campaign.AverageDistanceBetweenTwoFortifications;
		return 1f / (1f + num);
	}

	private float GeographicalAdvantageForFaction(Settlement settlement, IFaction faction)
	{
		Settlement factionMidSettlement = faction.FactionMidSettlement;
		float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, factionMidSettlement);
		float distanceToClosestNonAllyFortification = faction.DistanceToClosestNonAllyFortification;
		if (faction.FactionMidSettlement.MapFaction != faction)
		{
			return MathF.Clamp(Campaign.AverageDistanceBetweenTwoFortifications / (distance + 0.1f), 0f, 4f);
		}
		if (settlement.MapFaction == faction && distance < distanceToClosestNonAllyFortification)
		{
			return MathF.Clamp(Campaign.AverageDistanceBetweenTwoFortifications / (distanceToClosestNonAllyFortification - distance), 1f, 4f);
		}
		float num = (distance - distanceToClosestNonAllyFortification) / Campaign.AverageDistanceBetweenTwoFortifications;
		return 1f / (1f + num);
	}
}
