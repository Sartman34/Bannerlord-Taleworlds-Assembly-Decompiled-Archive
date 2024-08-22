using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultEncounterModel : EncounterModel
{
	public override float EstimatedMaximumMobilePartySpeedExceptPlayer => 10f;

	public override float NeededMaximumDistanceForEncounteringMobileParty => 0.5f;

	public override float MaximumAllowedDistanceForEncounteringMobilePartyInArmy => 1.5f;

	public override float NeededMaximumDistanceForEncounteringTown => 0.05f;

	public override float NeededMaximumDistanceForEncounteringVillage => 1f;

	public override bool IsEncounterExemptFromHostileActions(PartyBase side1, PartyBase side2)
	{
		if (side1 != null && side2 != null && (!side1.IsMobile || !side1.MobileParty.AvoidHostileActions))
		{
			if (side2.IsMobile)
			{
				return side2.MobileParty.AvoidHostileActions;
			}
			return false;
		}
		return true;
	}

	public override Hero GetLeaderOfSiegeEvent(SiegeEvent siegeEvent, BattleSideEnum side)
	{
		return GetLeaderOfEventInternal(siegeEvent.GetSiegeEventSide(side).GetInvolvedPartiesForEventType().ToList());
	}

	public override Hero GetLeaderOfMapEvent(MapEvent mapEvent, BattleSideEnum side)
	{
		return GetLeaderOfEventInternal(mapEvent.GetMapEventSide(side).Parties.Select((MapEventParty x) => x.Party).ToList());
	}

	private bool IsArmyLeader(Hero hero)
	{
		if (hero.PartyBelongedTo?.Army != null)
		{
			return hero.PartyBelongedTo.Army.LeaderParty == hero.PartyBelongedTo;
		}
		return false;
	}

	private int GetLeadingScore(Hero hero)
	{
		if (!hero.IsKingdomLeader && !IsArmyLeader(hero))
		{
			return GetCharacterSergeantScore(hero);
		}
		return (int)hero.PartyBelongedTo.GetTotalStrengthWithFollowers();
	}

	private Hero GetLeaderOfEventInternal(List<PartyBase> allPartiesThatBelongToASide)
	{
		Hero hero = null;
		int num = 0;
		foreach (PartyBase item in allPartiesThatBelongToASide)
		{
			Hero leaderHero = item.LeaderHero;
			if (leaderHero == null)
			{
				continue;
			}
			int leadingScore = GetLeadingScore(leaderHero);
			if (hero == null)
			{
				hero = leaderHero;
				num = leadingScore;
			}
			bool isKingdomLeader = leaderHero.IsKingdomLeader;
			bool flag = IsArmyLeader(leaderHero);
			bool isKingdomLeader2 = hero.IsKingdomLeader;
			bool flag2 = IsArmyLeader(hero);
			if (isKingdomLeader)
			{
				if (!isKingdomLeader2 || leadingScore > num)
				{
					hero = leaderHero;
					num = leadingScore;
				}
			}
			else if (flag)
			{
				if ((!isKingdomLeader2 && !flag2) || (flag2 && !isKingdomLeader2 && leadingScore > num))
				{
					hero = leaderHero;
					num = leadingScore;
				}
			}
			else if (!isKingdomLeader2 && !flag2 && leadingScore > num)
			{
				hero = leaderHero;
				num = leadingScore;
			}
		}
		return hero;
	}

	public override int GetCharacterSergeantScore(Hero hero)
	{
		int num = 0;
		Clan clan = hero.Clan;
		if (clan != null)
		{
			num += clan.Tier * ((hero == clan.Leader) ? 100 : 20);
			if (clan.Kingdom != null && clan.Kingdom.Leader == hero)
			{
				num += 2000;
			}
		}
		MobileParty partyBelongedTo = hero.PartyBelongedTo;
		if (partyBelongedTo != null)
		{
			if (partyBelongedTo.Army != null && partyBelongedTo.Army.LeaderParty == partyBelongedTo)
			{
				num += partyBelongedTo.Army.Parties.Count * 200;
			}
			num += partyBelongedTo.MemberRoster.TotalManCount - partyBelongedTo.MemberRoster.TotalWounded;
		}
		return num;
	}

	public override IEnumerable<PartyBase> GetDefenderPartiesOfSettlement(Settlement settlement, MapEvent.BattleTypes mapEventType)
	{
		if (settlement.IsFortification)
		{
			return settlement.Town.GetDefenderParties(mapEventType);
		}
		if (settlement.IsVillage)
		{
			return settlement.Village.GetDefenderParties(mapEventType);
		}
		if (settlement.IsHideout)
		{
			return settlement.Hideout.GetDefenderParties(mapEventType);
		}
		return null;
	}

	public override PartyBase GetNextDefenderPartyOfSettlement(Settlement settlement, ref int partyIndex, MapEvent.BattleTypes mapEventType)
	{
		if (settlement.IsFortification)
		{
			return settlement.Town.GetNextDefenderParty(ref partyIndex, mapEventType);
		}
		if (settlement.IsVillage)
		{
			return settlement.Village.GetNextDefenderParty(ref partyIndex, mapEventType);
		}
		if (settlement.IsHideout)
		{
			return settlement.Hideout.GetNextDefenderParty(ref partyIndex, mapEventType);
		}
		return null;
	}

	public override MapEventComponent CreateMapEventComponentForEncounter(PartyBase attackerParty, PartyBase defenderParty, MapEvent.BattleTypes battleType)
	{
		MapEventComponent result = null;
		switch (battleType)
		{
		case MapEvent.BattleTypes.FieldBattle:
			result = FieldBattleEventComponent.CreateFieldBattleEvent(attackerParty, defenderParty);
			break;
		case MapEvent.BattleTypes.Raid:
			result = RaidEventComponent.CreateRaidEvent(attackerParty, defenderParty);
			break;
		case MapEvent.BattleTypes.Siege:
			Campaign.Current.MapEventManager.StartSiegeMapEvent(attackerParty, defenderParty);
			break;
		case MapEvent.BattleTypes.Hideout:
			result = HideoutEventComponent.CreateHideoutEvent(attackerParty, defenderParty);
			break;
		case MapEvent.BattleTypes.SallyOut:
			Campaign.Current.MapEventManager.StartSallyOutMapEvent(attackerParty, defenderParty);
			break;
		case MapEvent.BattleTypes.SiegeOutside:
			Campaign.Current.MapEventManager.StartSiegeOutsideMapEvent(attackerParty, defenderParty);
			break;
		}
		return result;
	}
}
