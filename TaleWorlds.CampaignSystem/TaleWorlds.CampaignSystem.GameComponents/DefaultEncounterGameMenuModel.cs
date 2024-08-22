using System.Linq;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultEncounterGameMenuModel : EncounterGameMenuModel
{
	protected PartyBase GetEncounteredPartyBase(PartyBase attackerParty, PartyBase defenderParty)
	{
		if (attackerParty == PartyBase.MainParty || defenderParty == PartyBase.MainParty)
		{
			if (attackerParty != PartyBase.MainParty)
			{
				return attackerParty;
			}
			return defenderParty;
		}
		if (defenderParty.MapEvent == null)
		{
			return attackerParty;
		}
		return defenderParty;
	}

	public override string GetEncounterMenu(PartyBase attackerParty, PartyBase defenderParty, out bool startBattle, out bool joinBattle)
	{
		PartyBase encounteredPartyBase = GetEncounteredPartyBase(attackerParty, defenderParty);
		joinBattle = false;
		startBattle = false;
		if (defenderParty == null)
		{
			return "camp";
		}
		if (encounteredPartyBase.IsSettlement)
		{
			Settlement settlement = encounteredPartyBase.Settlement;
			if (settlement.IsVillage)
			{
				if (encounteredPartyBase.MapEvent != null && encounteredPartyBase.MapEvent.IsRaid)
				{
					if (MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty && encounteredPartyBase.MapEvent.AttackerSide.LeaderParty == MobileParty.MainParty.Army.LeaderParty.Party && encounteredPartyBase.MapEvent.DefenderSide.TroopCount <= 0)
					{
						joinBattle = true;
						if (!encounteredPartyBase.MapEvent.IsRaid)
						{
							return "army_wait";
						}
						return "raiding_village";
					}
					if ((MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty && attackerParty == MobileParty.MainParty.Army.LeaderParty.Party) || (MobileParty.MainParty.CurrentSettlement == settlement && MobileParty.MainParty.MapFaction == settlement.MapFaction))
					{
						joinBattle = true;
						return "encounter";
					}
					return "join_encounter";
				}
				if (settlement.MapFaction == MobileParty.MainParty.MapFaction && MobileParty.MainParty.Army != null && attackerParty == MobileParty.MainParty.Army.LeaderParty.Party && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty)
				{
					return "army_wait_at_settlement";
				}
				return "village_outside";
			}
			if (settlement.IsFortification)
			{
				if (PlayerSiege.PlayerSiegeEvent != null && PlayerSiege.PlayerSide == BattleSideEnum.Defender && settlement.Party.MapEvent == null)
				{
					return "menu_siege_strategies";
				}
				if (settlement.Party.SiegeEvent != null && ((settlement.Party.MapEvent == null && (settlement.Town.GarrisonParty == null || settlement.Town.GarrisonParty.MapEvent == null || settlement.Town.GarrisonParty.MapEvent.IsSallyOut)) || MobileParty.MainParty.MapFaction == settlement.MapFaction))
				{
					if (settlement.Party.SiegeEvent.BesiegerCamp.LeaderParty == MobileParty.MainParty)
					{
						return "continue_siege_after_attack";
					}
					if (MobileParty.MainParty.BesiegedSettlement == null && MobileParty.MainParty.CurrentSettlement == null)
					{
						if (settlement.Party.SiegeEvent.BesiegerCamp.LeaderParty.MapEvent != null && settlement.Party.SiegeEvent.BesiegerCamp.LeaderParty.MapEvent.IsSiegeOutside)
						{
							return "join_encounter";
						}
						return "join_siege_event";
					}
				}
				if (settlement.Party.MapEvent != null)
				{
					if ((MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty && attackerParty == MobileParty.MainParty.Army.LeaderParty.Party) || (MobileParty.MainParty.CurrentSettlement == settlement && settlement.Party.MapEvent.CanPartyJoinBattle(PartyBase.MainParty, settlement.BattleSide)))
					{
						return "encounter";
					}
					return "join_siege_event";
				}
				if (settlement.MapFaction == MobileParty.MainParty.MapFaction && MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty)
				{
					return "army_wait_at_settlement";
				}
				if (settlement.IsCastle)
				{
					return "castle_outside";
				}
				return "town_outside";
			}
			if (settlement.IsHideout)
			{
				return "hideout_place";
			}
			if (settlement.SettlementComponent is RetirementSettlementComponent)
			{
				return "retirement_place";
			}
			return "";
		}
		if (encounteredPartyBase.MapEvent != null)
		{
			if (MobileParty.MainParty.CurrentSettlement != null || (MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty))
			{
				joinBattle = true;
				return "encounter";
			}
			if (encounteredPartyBase.SiegeEvent != null && encounteredPartyBase.MapEvent.IsSiegeAssault)
			{
				return "join_siege_event";
			}
			return "join_encounter";
		}
		if (encounteredPartyBase.IsMobile && ((encounteredPartyBase.MobileParty.IsGarrison && MobileParty.MainParty.BesiegedSettlement != null) || (MobileParty.MainParty.CurrentSettlement != null && encounteredPartyBase.MobileParty.BesiegedSettlement == MobileParty.MainParty.CurrentSettlement)))
		{
			startBattle = true;
			joinBattle = false;
			return "encounter";
		}
		if (encounteredPartyBase.IsMobile)
		{
			return "encounter_meeting";
		}
		return null;
	}

	public override string GetRaidCompleteMenu()
	{
		return "village_loot_complete";
	}

	public override string GetNewPartyJoinMenu(MobileParty newParty)
	{
		if (PartyBase.MainParty.MapEvent.IsRaid && PartyBase.MainParty.MapEvent.AttackerSide.LeaderParty.MapFaction != PartyBase.MainParty.MapEvent.MapEventSettlement.MapFaction)
		{
			if (MobileParty.MainParty.CurrentSettlement != null || (MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty))
			{
				return "encounter";
			}
			return "join_encounter";
		}
		return null;
	}

	public override string GetGenericStateMenu()
	{
		MobileParty mainParty = MobileParty.MainParty;
		if (PlayerEncounter.Current != null && PlayerEncounter.CurrentBattleSimulation != null)
		{
			return null;
		}
		if (mainParty.MapEvent != null)
		{
			return "encounter";
		}
		if (mainParty.BesiegerCamp != null)
		{
			return "menu_siege_strategies";
		}
		if (mainParty.AttachedTo != null)
		{
			if ((mainParty.AttachedTo.CurrentSettlement != null && !mainParty.AttachedTo.CurrentSettlement.IsUnderSiege) || (mainParty.AttachedTo.LastVisitedSettlement != null && mainParty.AttachedTo.LastVisitedSettlement.IsVillage && mainParty.AttachedTo.LastVisitedSettlement.Position2D.DistanceSquared(mainParty.AttachedTo.Position2D) < 1f))
			{
				return "army_wait_at_settlement";
			}
			if (mainParty.AttachedTo.CurrentSettlement != null && mainParty.AttachedTo.CurrentSettlement.IsUnderSiege)
			{
				if (PlayerEncounter.Current != null && PlayerEncounter.Current.IsPlayerWaiting)
				{
					return "encounter_interrupted_siege_preparations";
				}
				return "menu_siege_strategies";
			}
			return "army_wait";
		}
		if (mainParty.CurrentSettlement != null)
		{
			Settlement currentSettlement = mainParty.CurrentSettlement;
			if (currentSettlement.IsFortification)
			{
				if (currentSettlement.Party.SiegeEvent != null && ((currentSettlement.Party.MapEvent == null && (currentSettlement.Town.GarrisonParty == null || currentSettlement.Town.GarrisonParty.MapEvent == null)) || MobileParty.MainParty.MapFaction == currentSettlement.MapFaction))
				{
					if (currentSettlement.Party.SiegeEvent.BesiegerCamp.LeaderParty == MobileParty.MainParty)
					{
						return "continue_siege_after_attack";
					}
					if (MobileParty.MainParty.BesiegedSettlement == null && MobileParty.MainParty.CurrentSettlement == null)
					{
						return "join_siege_event";
					}
					if (mainParty.CurrentSettlement.Party.MapEvent != null && mainParty.CurrentSettlement.Party.MapEvent.InvolvedParties.Contains(PartyBase.MainParty))
					{
						return "encounter";
					}
					if (PlayerEncounter.Current != null && PlayerEncounter.Current.IsPlayerWaiting)
					{
						return "encounter_interrupted_siege_preparations";
					}
					return "menu_siege_strategies";
				}
				if (currentSettlement.Party.MapEvent != null)
				{
					if (MobileParty.MainParty.MapFaction == currentSettlement.MapFaction)
					{
						return "encounter";
					}
					return "join_encounter";
				}
				if (currentSettlement.MapFaction == MobileParty.MainParty.MapFaction && MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty)
				{
					return "army_wait_at_settlement";
				}
				if (PlayerEncounter.Current != null && PlayerEncounter.Current.IsPlayerWaiting && currentSettlement.IsFortification)
				{
					return "town_wait_menus";
				}
				if (currentSettlement.IsCastle)
				{
					return "castle_outside";
				}
				return "town_outside";
			}
			if (currentSettlement.IsHideout)
			{
				return "hideout_place";
			}
		}
		else if (Settlement.CurrentSettlement != null)
		{
			Settlement currentSettlement2 = Settlement.CurrentSettlement;
			if (currentSettlement2.IsVillage)
			{
				if (currentSettlement2.IsUnderRaid)
				{
					return "encounter_interrupted_raid_started";
				}
				if (PlayerEncounter.Current != null && PlayerEncounter.Current.IsPlayerWaiting)
				{
					return "village_wait_menus";
				}
			}
		}
		return null;
	}
}
