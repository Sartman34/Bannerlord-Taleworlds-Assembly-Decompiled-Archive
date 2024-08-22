using System.Collections.Generic;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultTroopSupplierProbabilityModel : TroopSupplierProbabilityModel
{
	public override void EnqueueTroopSpawnProbabilitiesAccordingToUnitSpawnPrioritization(MapEventParty battleParty, FlattenedTroopRoster priorityTroops, bool includePlayer, int sizeOfSide, bool forcePriorityTroops, List<(FlattenedTroopRosterElement, MapEventParty, float)> priorityList)
	{
		UnitSpawnPrioritizations unitSpawnPrioritizations = UnitSpawnPrioritizations.HighLevel;
		bool flag = PlayerEncounter.Battle?.IsSiegeAmbush ?? false;
		if (battleParty.Party == PartyBase.MainParty && !flag)
		{
			unitSpawnPrioritizations = Game.Current.UnitSpawnPrioritization;
		}
		if (unitSpawnPrioritizations != 0 && !forcePriorityTroops)
		{
			StackArray.StackArray8Int stackArray8Int = default(StackArray.StackArray8Int);
			int num = 0;
			foreach (FlattenedTroopRosterElement troop2 in battleParty.Troops)
			{
				if (CanTroopJoinBattle(troop2, includePlayer))
				{
					stackArray8Int[(int)troop2.Troop.DefaultFormationClass]++;
					num++;
				}
			}
			StackArray.StackArray8Int stackArray8Int2 = default(StackArray.StackArray8Int);
			float num2 = 1000f;
			{
				foreach (FlattenedTroopRosterElement troop3 in battleParty.Troops)
				{
					if (!CanTroopJoinBattle(troop3, includePlayer))
					{
						continue;
					}
					CharacterObject troop = troop3.Troop;
					FormationClass formationClass = troop.GetFormationClass();
					float num3;
					if (priorityTroops != null && IsPriorityTroop(troop3, priorityTroops))
					{
						num3 = num2--;
					}
					else
					{
						float num4 = (float)stackArray8Int[(int)formationClass] / (float)((unitSpawnPrioritizations == UnitSpawnPrioritizations.Homogeneous) ? (stackArray8Int2[(int)formationClass] + 1) : num);
						num3 = (troop.IsHero ? num2-- : num4);
						if (!troop.IsHero && (unitSpawnPrioritizations == UnitSpawnPrioritizations.HighLevel || unitSpawnPrioritizations == UnitSpawnPrioritizations.LowLevel))
						{
							num3 += (float)troop.Level;
							if (unitSpawnPrioritizations == UnitSpawnPrioritizations.LowLevel)
							{
								num3 *= -1f;
							}
						}
					}
					stackArray8Int[(int)formationClass]--;
					stackArray8Int2[(int)formationClass]++;
					priorityList.Add((troop3, battleParty, num3));
				}
				return;
			}
		}
		int numberOfHealthyMembers = battleParty.Party.NumberOfHealthyMembers;
		foreach (FlattenedTroopRosterElement troop4 in battleParty.Troops)
		{
			if (!CanTroopJoinBattle(troop4, includePlayer))
			{
				continue;
			}
			float num5 = 1f;
			if (troop4.Troop.IsHero)
			{
				num5 *= 150f;
				if (priorityTroops != null)
				{
					UniqueTroopDescriptor descriptor = priorityTroops.FindIndexOfCharacter(troop4.Troop);
					if (descriptor.IsValid)
					{
						num5 *= 100f;
						priorityTroops.Remove(descriptor);
					}
				}
				if (troop4.Troop.HeroObject.IsHumanPlayerCharacter)
				{
					num5 *= 10f;
				}
				priorityList.Add((troop4, battleParty, num5));
				continue;
			}
			int num6 = 0;
			int num7 = 0;
			for (int i = 0; i < battleParty.Party.MemberRoster.Count; i++)
			{
				TroopRosterElement elementCopyAtIndex = battleParty.Party.MemberRoster.GetElementCopyAtIndex(i);
				if (!elementCopyAtIndex.Character.IsHero)
				{
					if (elementCopyAtIndex.Character == troop4.Troop)
					{
						num6 = i - num7;
						break;
					}
				}
				else
				{
					num7++;
				}
			}
			int num8 = (int)(100f / MathF.Pow(1.2f, num6));
			if (num8 < 10)
			{
				num8 = 10;
			}
			int num9 = numberOfHealthyMembers / sizeOfSide * 100;
			if (num9 < 10)
			{
				num9 = 10;
			}
			int num10 = 0;
			if (priorityTroops != null)
			{
				UniqueTroopDescriptor descriptor2 = priorityTroops.FindIndexOfCharacter(troop4.Troop);
				if (descriptor2.IsValid)
				{
					num10 = 20000;
					priorityTroops.Remove(descriptor2);
				}
			}
			num5 = num10 + MBRandom.RandomInt((int)((float)num8 * 0.5f + (float)num9 * 0.5f));
			priorityList.Add((troop4, battleParty, num5));
		}
	}

	private bool IsPriorityTroop(FlattenedTroopRosterElement troop, FlattenedTroopRoster priorityTroops)
	{
		foreach (FlattenedTroopRosterElement priorityTroop in priorityTroops)
		{
			if (priorityTroop.Troop == troop.Troop)
			{
				return true;
			}
		}
		return false;
	}

	private bool CanTroopJoinBattle(FlattenedTroopRosterElement troopRoster, bool includePlayer)
	{
		if (!troopRoster.IsWounded && !troopRoster.IsRouted && !troopRoster.IsKilled)
		{
			if (!includePlayer)
			{
				return !troopRoster.Troop.IsPlayerCharacter;
			}
			return true;
		}
		return false;
	}
}
