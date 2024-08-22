using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class CombatSimulationModel : GameModel
{
	public abstract int SimulateHit(CharacterObject strikerTroop, CharacterObject struckTroop, PartyBase strikerParty, PartyBase struckParty, float strikerAdvantage, MapEvent battle);

	public abstract (int defenderRounds, int attackerRounds) GetSimulationRoundsForBattle(MapEvent mapEvent, int numDefenders, int numAttackers);

	public abstract int GetNumberOfEquipmentsBuilt(Settlement settlement);

	public abstract float GetMaximumSiegeEquipmentProgress(Settlement settlement);

	public abstract float GetSettlementAdvantage(Settlement settlement);

	public abstract (float defenderAdvantage, float attackerAdvantage) GetBattleAdvantage(PartyBase defenderParty, PartyBase attackerParty, MapEvent.BattleTypes mapEventType, Settlement settlement);
}
