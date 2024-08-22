using System.Collections.Generic;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class EncounterModel : GameModel
{
	public abstract float EstimatedMaximumMobilePartySpeedExceptPlayer { get; }

	public abstract float NeededMaximumDistanceForEncounteringMobileParty { get; }

	public abstract float MaximumAllowedDistanceForEncounteringMobilePartyInArmy { get; }

	public abstract float NeededMaximumDistanceForEncounteringTown { get; }

	public abstract float NeededMaximumDistanceForEncounteringVillage { get; }

	public abstract bool IsEncounterExemptFromHostileActions(PartyBase side1, PartyBase side2);

	public abstract Hero GetLeaderOfSiegeEvent(SiegeEvent siegeEvent, BattleSideEnum side);

	public abstract Hero GetLeaderOfMapEvent(MapEvent mapEvent, BattleSideEnum side);

	public abstract int GetCharacterSergeantScore(Hero hero);

	public abstract IEnumerable<PartyBase> GetDefenderPartiesOfSettlement(Settlement settlement, MapEvent.BattleTypes mapEventType);

	public abstract PartyBase GetNextDefenderPartyOfSettlement(Settlement settlement, ref int partyIndex, MapEvent.BattleTypes mapEventType);

	public abstract MapEventComponent CreateMapEventComponentForEncounter(PartyBase attackerParty, PartyBase defenderParty, MapEvent.BattleTypes battleType);
}
