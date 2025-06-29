using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.Actions;

public static class StartBattleAction
{
	private static void ApplyInternal(PartyBase attackerParty, PartyBase defenderParty, object subject, MapEvent.BattleTypes battleType)
	{
		if (defenderParty.MapEvent == null)
		{
			Campaign.Current.Models.EncounterModel.CreateMapEventComponentForEncounter(attackerParty, defenderParty, battleType);
		}
		else
		{
			BattleSideEnum side = BattleSideEnum.Attacker;
			if (defenderParty.Side == BattleSideEnum.Attacker)
			{
				side = BattleSideEnum.Defender;
			}
			attackerParty.MapEventSide = defenderParty.MapEvent.GetMapEventSide(side);
		}
		if (defenderParty.MapEvent.IsPlayerMapEvent && !defenderParty.MapEvent.IsSallyOut && PlayerEncounter.Current != null && MobileParty.MainParty.CurrentSettlement != null)
		{
			PlayerEncounter.Current.InterruptEncounter("encounter_interrupted");
		}
		bool flag = (attackerParty.MobileParty?.Army == null || attackerParty.MobileParty?.Army.LeaderParty == attackerParty.MobileParty) && (defenderParty.MobileParty?.Army == null || defenderParty.MobileParty?.Army.LeaderParty == defenderParty.MobileParty);
		if (flag && defenderParty.IsSettlement && defenderParty.MapEvent != null && defenderParty.MapEvent.DefenderSide.Parties.Count > 1)
		{
			flag = false;
		}
		CampaignEventDispatcher.Instance.OnStartBattle(attackerParty, defenderParty, subject, flag);
	}

	public static void Apply(PartyBase attackerParty, PartyBase defenderParty)
	{
		MapEvent.BattleTypes battleTypes = MapEvent.BattleTypes.None;
		object obj = null;
		Settlement settlement;
		if (defenderParty.MapEvent == null)
		{
			if (attackerParty.MobileParty != null && attackerParty.MobileParty.IsGarrison)
			{
				settlement = attackerParty.MobileParty.CurrentSettlement;
				battleTypes = MapEvent.BattleTypes.SallyOut;
			}
			else if (attackerParty.MobileParty.CurrentSettlement != null)
			{
				settlement = attackerParty.MobileParty.CurrentSettlement;
			}
			else if (defenderParty.MobileParty.CurrentSettlement != null)
			{
				settlement = defenderParty.MobileParty.CurrentSettlement;
			}
			else if (attackerParty.MobileParty.BesiegedSettlement != null)
			{
				settlement = attackerParty.MobileParty.BesiegedSettlement;
				if (!defenderParty.IsSettlement)
				{
					battleTypes = MapEvent.BattleTypes.SiegeOutside;
				}
			}
			else if (defenderParty.MobileParty.BesiegedSettlement != null)
			{
				settlement = defenderParty.MobileParty.BesiegedSettlement;
				battleTypes = MapEvent.BattleTypes.SiegeOutside;
			}
			else
			{
				battleTypes = MapEvent.BattleTypes.FieldBattle;
				settlement = null;
			}
			if (settlement != null && battleTypes == MapEvent.BattleTypes.None)
			{
				if (settlement.IsTown)
				{
					battleTypes = MapEvent.BattleTypes.Siege;
				}
				else if (settlement.IsHideout)
				{
					battleTypes = MapEvent.BattleTypes.Hideout;
				}
				else if (settlement.IsVillage)
				{
					Debug.FailedAssert("Since villages can be raided or sieged, this block cannot decide if the battle is raid or siege.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Actions\\StartBattleAction.cs", "Apply", 116);
				}
				else
				{
					Debug.FailedAssert("Missing settlement type in StartBattleAction.GetGameAction", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Actions\\StartBattleAction.cs", "Apply", 120);
				}
			}
		}
		else
		{
			if (defenderParty.MapEvent.IsFieldBattle)
			{
				battleTypes = MapEvent.BattleTypes.FieldBattle;
			}
			else if (defenderParty.MapEvent.IsRaid)
			{
				battleTypes = MapEvent.BattleTypes.Raid;
			}
			else if (defenderParty.MapEvent.IsSiegeAssault)
			{
				battleTypes = MapEvent.BattleTypes.Siege;
			}
			else if (defenderParty.MapEvent.IsSallyOut)
			{
				battleTypes = MapEvent.BattleTypes.SallyOut;
			}
			else if (defenderParty.MapEvent.IsSiegeOutside)
			{
				battleTypes = MapEvent.BattleTypes.SiegeOutside;
			}
			else
			{
				Debug.FailedAssert("Missing mapEventType?", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Actions\\StartBattleAction.cs", "Apply", 148);
			}
			settlement = defenderParty.MapEvent.MapEventSettlement;
		}
		obj = obj ?? settlement;
		ApplyInternal(attackerParty, defenderParty, obj, battleTypes);
	}

	public static void ApplyStartBattle(MobileParty attackerParty, MobileParty defenderParty)
	{
		ApplyInternal(attackerParty.Party, defenderParty.Party, null, MapEvent.BattleTypes.FieldBattle);
	}

	public static void ApplyStartRaid(MobileParty attackerParty, Settlement settlement)
	{
		ApplyInternal(attackerParty.Party, settlement.Party, settlement, MapEvent.BattleTypes.Raid);
	}

	public static void ApplyStartSallyOut(Settlement settlement, MobileParty defenderParty)
	{
		ApplyInternal(settlement.Town.GarrisonParty.Party, defenderParty.Party, settlement, MapEvent.BattleTypes.SallyOut);
	}

	public static void ApplyStartAssaultAgainstWalls(MobileParty attackerParty, Settlement settlement)
	{
		ApplyInternal(attackerParty.Party, settlement.Party, settlement, MapEvent.BattleTypes.Siege);
	}
}
