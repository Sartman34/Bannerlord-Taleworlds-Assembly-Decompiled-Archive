using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultCombatSimulationModel : CombatSimulationModel
{
	public override int SimulateHit(CharacterObject strikerTroop, CharacterObject struckTroop, PartyBase strikerParty, PartyBase struckParty, float strikerAdvantage, MapEvent battle)
	{
		MilitaryPowerModel militaryPowerModel = Campaign.Current.Models.MilitaryPowerModel;
		float troopPower = militaryPowerModel.GetTroopPower(strikerTroop, strikerParty.Side, strikerParty.MapEvent.SimulationContext, strikerParty.MapEventSide.LeaderSimulationModifier);
		float troopPower2 = militaryPowerModel.GetTroopPower(struckTroop, struckParty.Side, struckParty.MapEvent.SimulationContext, struckParty.MapEventSide.LeaderSimulationModifier);
		int num = (int)((0.5f + 0.5f * MBRandom.RandomFloat) * (40f * MathF.Pow(troopPower / troopPower2, 0.7f) * strikerAdvantage));
		ExplainedNumber effectiveDamage = new ExplainedNumber(num);
		if (strikerParty.IsMobile && struckParty.IsMobile)
		{
			CalculateSimulationDamagePerkEffects(strikerTroop, struckTroop, strikerParty.MobileParty, struckParty.MobileParty, ref effectiveDamage, battle);
		}
		return (int)effectiveDamage.ResultNumber;
	}

	private void CalculateSimulationDamagePerkEffects(CharacterObject strikerTroop, CharacterObject struckTroop, MobileParty strikerParty, MobileParty struckParty, ref ExplainedNumber effectiveDamage, MapEvent battle)
	{
		if (strikerParty.HasPerk(DefaultPerks.Tactics.TightFormations) && strikerTroop.IsInfantry && struckTroop.IsMounted)
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Tactics.TightFormations, strikerParty, isPrimaryBonus: true, ref effectiveDamage);
		}
		if (struckParty.HasPerk(DefaultPerks.Tactics.LooseFormations) && struckTroop.IsInfantry && strikerTroop.IsRanged)
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Tactics.LooseFormations, struckParty, isPrimaryBonus: true, ref effectiveDamage);
		}
		TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(strikerParty.CurrentNavigationFace);
		if (strikerParty.HasPerk(DefaultPerks.Tactics.ExtendedSkirmish) && (faceTerrainType == TerrainType.Snow || faceTerrainType == TerrainType.Forest))
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Tactics.ExtendedSkirmish, strikerParty, isPrimaryBonus: true, ref effectiveDamage);
		}
		if (strikerParty.HasPerk(DefaultPerks.Tactics.DecisiveBattle) && (faceTerrainType == TerrainType.Plain || faceTerrainType == TerrainType.Steppe || faceTerrainType == TerrainType.Desert))
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Tactics.DecisiveBattle, strikerParty, isPrimaryBonus: true, ref effectiveDamage);
		}
		if (!strikerParty.IsBandit && struckParty.IsBandit && strikerParty.HasPerk(DefaultPerks.Tactics.LawKeeper))
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Tactics.LawKeeper, strikerParty, isPrimaryBonus: true, ref effectiveDamage);
		}
		if (strikerParty.HasPerk(DefaultPerks.Tactics.Coaching))
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Tactics.Coaching, strikerParty, isPrimaryBonus: true, ref effectiveDamage);
		}
		if (struckParty.HasPerk(DefaultPerks.Tactics.EliteReserves) && struckTroop.Tier >= 3)
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Tactics.EliteReserves, struckParty, isPrimaryBonus: true, ref effectiveDamage);
		}
		if (strikerParty.HasPerk(DefaultPerks.Tactics.Encirclement) && strikerParty.MemberRoster.TotalHealthyCount > struckParty.MemberRoster.TotalHealthyCount)
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Tactics.Encirclement, strikerParty, isPrimaryBonus: true, ref effectiveDamage);
		}
		if (strikerParty.HasPerk(DefaultPerks.Tactics.Counteroffensive, checkSecondaryRole: true) && strikerParty.MemberRoster.TotalHealthyCount < struckParty.MemberRoster.TotalHealthyCount)
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Tactics.Counteroffensive, strikerParty, isPrimaryBonus: false, ref effectiveDamage);
		}
		bool flag = false;
		foreach (MapEventParty item in battle.PartiesOnSide(BattleSideEnum.Defender))
		{
			if (item.Party == struckParty.Party)
			{
				flag = true;
				break;
			}
		}
		bool flag2 = !flag;
		bool flag3 = flag2;
		if (battle.IsSiegeAssault && flag2 && strikerParty.HasPerk(DefaultPerks.Tactics.Besieged))
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Tactics.Besieged, strikerParty, isPrimaryBonus: true, ref effectiveDamage);
		}
		if (flag && strikerParty.HasPerk(DefaultPerks.Scouting.Vanguard))
		{
			effectiveDamage.AddFactor(DefaultPerks.Scouting.Vanguard.PrimaryBonus, DefaultPerks.Scouting.Vanguard.Name);
		}
		if ((battle.IsSiegeOutside || battle.IsSallyOut) && flag3 && strikerParty.HasPerk(DefaultPerks.Scouting.Rearguard))
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Scouting.Rearguard, strikerParty, isPrimaryBonus: false, ref effectiveDamage);
		}
		if (battle.IsSallyOut && flag && strikerParty.HasPerk(DefaultPerks.Scouting.Vanguard, checkSecondaryRole: true))
		{
			effectiveDamage.AddFactor(DefaultPerks.Scouting.Vanguard.SecondaryBonus, DefaultPerks.Scouting.Vanguard.Name);
		}
		if (battle.IsFieldBattle && flag2 && strikerParty.HasPerk(DefaultPerks.Tactics.Counteroffensive))
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Tactics.Counteroffensive, strikerParty, isPrimaryBonus: true, ref effectiveDamage);
		}
		if (strikerParty.Army != null && strikerParty.LeaderHero != null && strikerParty.Army.LeaderParty == strikerParty && strikerParty.LeaderHero.GetPerkValue(DefaultPerks.Tactics.TacticalMastery))
		{
			PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Tactics.TacticalMastery, strikerParty.LeaderHero.CharacterObject, DefaultSkills.Tactics, applyPrimaryBonus: true, ref effectiveDamage, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus);
		}
	}

	public override float GetMaximumSiegeEquipmentProgress(Settlement settlement)
	{
		float num = 0f;
		if (settlement.SiegeEvent != null && settlement.IsFortification)
		{
			foreach (SiegeEvent.SiegeEngineConstructionProgress item in settlement.SiegeEvent.GetSiegeEventSide(BattleSideEnum.Attacker).SiegeEngines.AllSiegeEngines())
			{
				if (!item.IsConstructed && item.Progress > num)
				{
					num = item.Progress;
				}
			}
		}
		return num;
	}

	public override int GetNumberOfEquipmentsBuilt(Settlement settlement)
	{
		if (settlement.SiegeEvent != null && settlement.IsFortification)
		{
			settlement.Town.GetWallLevel();
			bool flag = false;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (SiegeEvent.SiegeEngineConstructionProgress item in settlement.SiegeEvent.GetSiegeEventSide(BattleSideEnum.Attacker).SiegeEngines.AllSiegeEngines())
			{
				if (item.IsConstructed)
				{
					if (item.SiegeEngine == DefaultSiegeEngineTypes.Ram)
					{
						flag = true;
					}
					else if (item.SiegeEngine == DefaultSiegeEngineTypes.SiegeTower)
					{
						num++;
					}
					else if (item.SiegeEngine == DefaultSiegeEngineTypes.Trebuchet || item.SiegeEngine == DefaultSiegeEngineTypes.Onager || item.SiegeEngine == DefaultSiegeEngineTypes.Ballista)
					{
						num2++;
					}
					else if (item.SiegeEngine == DefaultSiegeEngineTypes.FireOnager || item.SiegeEngine == DefaultSiegeEngineTypes.FireBallista)
					{
						num3++;
					}
				}
			}
			return (flag ? 1 : 0) + num + num2 + num3;
		}
		return 0;
	}

	public override float GetSettlementAdvantage(Settlement settlement)
	{
		if (settlement.SiegeEvent != null && settlement.IsFortification)
		{
			int wallLevel = settlement.Town.GetWallLevel();
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (SiegeEvent.SiegeEngineConstructionProgress item in settlement.SiegeEvent.GetSiegeEventSide(BattleSideEnum.Attacker).SiegeEngines.AllSiegeEngines())
			{
				if (!item.IsConstructed)
				{
					continue;
				}
				if (item.SiegeEngine == DefaultSiegeEngineTypes.Ram || item.SiegeEngine == DefaultSiegeEngineTypes.ImprovedRam)
				{
					if (item.SiegeEngine == DefaultSiegeEngineTypes.ImprovedRam)
					{
						flag2 = true;
					}
					flag = true;
				}
				else if (item.SiegeEngine == DefaultSiegeEngineTypes.SiegeTower)
				{
					num++;
				}
				else if (item.SiegeEngine == DefaultSiegeEngineTypes.Trebuchet || item.SiegeEngine == DefaultSiegeEngineTypes.Onager || item.SiegeEngine == DefaultSiegeEngineTypes.Ballista)
				{
					num2++;
				}
				else if (item.SiegeEngine == DefaultSiegeEngineTypes.FireOnager || item.SiegeEngine == DefaultSiegeEngineTypes.FireBallista)
				{
					num3++;
				}
			}
			float num4 = 4f + (float)(wallLevel - 1);
			if (settlement.SettlementTotalWallHitPoints < 1E-05f)
			{
				num4 *= 0.25f;
			}
			float num5 = 1f + num4;
			float num6 = 1f + ((flag || num > 0) ? 0.12f : 0f) + (flag2 ? 0.24f : (flag ? 0.16f : 0f)) + ((num > 1) ? 0.24f : ((num == 1) ? 0.16f : 0f)) + (float)num2 * 0.08f + (float)num3 * 0.12f;
			float baseNumber = num5 / num6;
			ExplainedNumber effectiveAdvantage = new ExplainedNumber(baseNumber);
			ISiegeEventSide siegeEventSide = settlement.SiegeEvent.GetSiegeEventSide(BattleSideEnum.Attacker);
			CalculateSettlementAdvantagePerkEffects(settlement, ref effectiveAdvantage, siegeEventSide);
			return effectiveAdvantage.ResultNumber;
		}
		if (settlement.IsVillage)
		{
			return 1.25f;
		}
		return 1f;
	}

	private void CalculateSettlementAdvantagePerkEffects(Settlement settlement, ref ExplainedNumber effectiveAdvantage, ISiegeEventSide opposingSide)
	{
		if (opposingSide.GetInvolvedPartiesForEventType().Any((PartyBase x) => x.MobileParty.HasPerk(DefaultPerks.Tactics.OnTheMarch)))
		{
			effectiveAdvantage.AddFactor(DefaultPerks.Tactics.OnTheMarch.PrimaryBonus, DefaultPerks.Tactics.OnTheMarch.Name);
		}
		if (PerkHelper.GetPerkValueForTown(DefaultPerks.Tactics.OnTheMarch, settlement.Town))
		{
			PerkHelper.AddPerkBonusForTown(DefaultPerks.Tactics.OnTheMarch, settlement.Town, ref effectiveAdvantage);
		}
	}

	public override (int defenderRounds, int attackerRounds) GetSimulationRoundsForBattle(MapEvent mapEvent, int numDefenders, int numAttackers)
	{
		if (mapEvent.IsInvulnerable)
		{
			return (defenderRounds: 0, attackerRounds: 0);
		}
		MapEvent.BattleTypes eventType = mapEvent.EventType;
		Settlement mapEventSettlement = mapEvent.MapEventSettlement;
		if (eventType == MapEvent.BattleTypes.Siege)
		{
			float num = GetSettlementAdvantage(mapEventSettlement) * 0.7f;
			float num2 = 1f + MathF.Pow(numDefenders, 0.3f);
			float num3 = MathF.Max(num2 * num, (float)((numDefenders + 1) / (numAttackers + 1)));
			if ((mapEventSettlement.IsTown && numDefenders > 100) || (mapEventSettlement.IsCastle && numDefenders > 30))
			{
				return (defenderRounds: MathF.Round(0.5f + num3), attackerRounds: MathF.Round(0.5f + num2));
			}
		}
		int item;
		int item2;
		if (numDefenders <= 10)
		{
			item = MBRandom.RoundRandomized(MathF.Min((float)numAttackers * 3f, (float)numDefenders * 0.3f));
			item2 = MBRandom.RoundRandomized(MathF.Min((float)numDefenders * 3f, (float)numAttackers * 0.3f));
		}
		else
		{
			item = MBRandom.RoundRandomized(MathF.Min((float)numAttackers * 2f, MathF.Pow(numDefenders, 0.6f)));
			item2 = MBRandom.RoundRandomized(MathF.Min((float)numDefenders * 2f, MathF.Pow(numAttackers, 0.6f)));
		}
		return (defenderRounds: item, attackerRounds: item2);
	}

	public override (float defenderAdvantage, float attackerAdvantage) GetBattleAdvantage(PartyBase defenderParty, PartyBase attackerParty, MapEvent.BattleTypes mapEventType, Settlement settlement)
	{
		float num = 1f;
		float item = 1f * PartyBattleAdvantage(defenderParty, attackerParty);
		num *= PartyBattleAdvantage(attackerParty, defenderParty);
		if (mapEventType == MapEvent.BattleTypes.Siege)
		{
			num *= 0.9f;
		}
		return (defenderAdvantage: item, attackerAdvantage: num);
	}

	private float PartyBattleAdvantage(PartyBase party, PartyBase opposingParty)
	{
		float num = 1f;
		if (party.LeaderHero != null)
		{
			int skillValue = party.LeaderHero.GetSkillValue(DefaultSkills.Tactics);
			float num2 = DefaultSkillEffects.TacticsAdvantage.PrimaryBonus * (float)skillValue * 0.01f;
			num += num2;
			if (party.LeaderHero.GetPerkValue(DefaultPerks.Scouting.Patrols) && opposingParty.Culture.IsBandit)
			{
				num += DefaultPerks.Scouting.Patrols.SecondaryBonus * num;
			}
		}
		if (party.IsMobile && opposingParty.IsMobile && party.LeaderHero != null && opposingParty.LeaderHero != null && party.MobileParty.HasPerk(DefaultPerks.Tactics.PreBattleManeuvers, checkSecondaryRole: true))
		{
			int num3 = party.LeaderHero.GetSkillValue(DefaultSkills.Tactics) - opposingParty.LeaderHero.GetSkillValue(DefaultSkills.Tactics);
			if (num3 > 0)
			{
				num += (float)num3 * 0.01f;
			}
		}
		return num;
	}
}
