using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class TargetScoreCalculatingModel : GameModel
{
	public abstract float TravelingToAssignmentFactor { get; }

	public abstract float BesiegingFactor { get; }

	public abstract float AssaultingTownFactor { get; }

	public abstract float RaidingFactor { get; }

	public abstract float DefendingFactor { get; }

	public abstract float GetTargetScoreForFaction(Settlement targetSettlement, Army.ArmyTypes missionType, MobileParty mobileParty, float ourStrength, int numberOfEnemyFactionSettlements = -1, float totalEnemyMobilePartyStrength = -1f);

	public abstract float CalculatePatrollingScoreForSettlement(Settlement targetSettlement, MobileParty mobileParty);

	public abstract float CurrentObjectiveValue(MobileParty mobileParty);
}
