using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Helpers;

public static class SettlementHelper
{
	private static readonly string[] StuffToCarryForMan = new string[8] { "_to_carry_foods_basket_apple", "_to_carry_merchandise_hides_b", "_to_carry_bed_convolute_g", "_to_carry_bed_convolute_a", "_to_carry_bd_fabric_c", "_to_carry_bd_basket_a", "practice_spear_t1", "simple_sparth_axe_t2" };

	private static readonly string[] StuffToCarryForWoman = new string[4] { "_to_carry_kitchen_pot_c", "_to_carry_arm_kitchen_pot_c", "_to_carry_foods_basket_apple", "_to_carry_bd_basket_a" };

	private static int _stuffToCarryIndex = MBRandom.NondeterministicRandomInt % 1024;

	public static string GetRandomStuff(bool isFemale)
	{
		string result = ((!isFemale) ? StuffToCarryForMan[_stuffToCarryIndex % StuffToCarryForMan.Length] : StuffToCarryForWoman[_stuffToCarryIndex % StuffToCarryForWoman.Length]);
		_stuffToCarryIndex++;
		return result;
	}

	public static Settlement FindNearestSettlement(Func<Settlement, bool> condition, IMapPoint toMapPoint = null)
	{
		Settlement result = null;
		float maximumDistance = Campaign.MapDiagonal * 2f;
		IMapPoint fromMapPoint = toMapPoint ?? MobileParty.MainParty;
		foreach (Settlement item in Settlement.All)
		{
			if ((condition == null || condition(item)) && Campaign.Current.Models.MapDistanceModel.GetDistance(fromMapPoint, item, maximumDistance, out var distance))
			{
				result = item;
				maximumDistance = distance;
			}
		}
		return result;
	}

	public static Settlement FindNearestHideout(Func<Settlement, bool> condition = null, IMapPoint toMapPoint = null)
	{
		Settlement result = null;
		float maximumDistance = 1E+09f;
		IMapPoint fromMapPoint = toMapPoint ?? MobileParty.MainParty;
		foreach (Hideout item in Hideout.All)
		{
			Settlement settlement = item.Settlement;
			if ((condition == null || condition(settlement)) && Campaign.Current.Models.MapDistanceModel.GetDistance(fromMapPoint, settlement, maximumDistance, out var distance))
			{
				result = settlement;
				maximumDistance = distance;
			}
		}
		return result;
	}

	public static Settlement FindNearestTown(Func<Settlement, bool> condition = null, IMapPoint toMapPoint = null)
	{
		Settlement result = null;
		float maximumDistance = 1E+09f;
		IMapPoint fromMapPoint = toMapPoint ?? MobileParty.MainParty;
		foreach (Town allTown in Town.AllTowns)
		{
			Settlement settlement = allTown.Settlement;
			if ((condition == null || condition(settlement)) && Campaign.Current.Models.MapDistanceModel.GetDistance(fromMapPoint, settlement, maximumDistance, out var distance))
			{
				result = settlement;
				maximumDistance = distance;
			}
		}
		return result;
	}

	public static Settlement FindNearestFortification(Func<Settlement, bool> condition = null, IMapPoint toMapPoint = null)
	{
		Settlement result = null;
		float maximumDistance = 1E+09f;
		IMapPoint fromMapPoint = toMapPoint ?? MobileParty.MainParty;
		foreach (Town allTown in Town.AllTowns)
		{
			Settlement settlement = allTown.Settlement;
			if ((condition == null || condition(settlement)) && Campaign.Current.Models.MapDistanceModel.GetDistance(fromMapPoint, settlement, maximumDistance, out var distance))
			{
				result = settlement;
				maximumDistance = distance;
			}
		}
		foreach (Town allCastle in Town.AllCastles)
		{
			Settlement settlement2 = allCastle.Settlement;
			if ((condition == null || condition(settlement2)) && Campaign.Current.Models.MapDistanceModel.GetDistance(fromMapPoint, settlement2, maximumDistance, out var distance2))
			{
				result = settlement2;
				maximumDistance = distance2;
			}
		}
		return result;
	}

	public static Settlement FindNearestCastle(Func<Settlement, bool> condition = null, IMapPoint toMapPoint = null)
	{
		Settlement result = null;
		float maximumDistance = 1E+09f;
		IMapPoint fromMapPoint = toMapPoint ?? MobileParty.MainParty;
		foreach (Town allCastle in Town.AllCastles)
		{
			Settlement settlement = allCastle.Settlement;
			if ((condition == null || condition(settlement)) && Campaign.Current.Models.MapDistanceModel.GetDistance(fromMapPoint, settlement, maximumDistance, out var distance))
			{
				result = settlement;
				maximumDistance = distance;
			}
		}
		return result;
	}

	public static Settlement FindNearestVillage(Func<Settlement, bool> condition = null, IMapPoint toMapPoint = null)
	{
		Settlement result = null;
		float maximumDistance = 1E+09f;
		IMapPoint fromMapPoint = toMapPoint ?? MobileParty.MainParty;
		foreach (Village item in Village.All)
		{
			Settlement settlement = item.Settlement;
			if ((condition == null || condition(settlement)) && Campaign.Current.Models.MapDistanceModel.GetDistance(fromMapPoint, settlement, maximumDistance, out var distance))
			{
				result = settlement;
				maximumDistance = distance;
			}
		}
		return result;
	}

	private static SettlementComponent FindNearestSettlementToMapPointInternal(IMapPoint mapPoint, IEnumerable<SettlementComponent> settlementsToIterate, Func<Settlement, bool> condition = null)
	{
		SettlementComponent result = null;
		float maximumDistance = Campaign.MapDiagonal * 2f;
		foreach (SettlementComponent item in settlementsToIterate)
		{
			if ((condition == null || condition(item.Settlement)) && Campaign.Current.Models.MapDistanceModel.GetDistance(mapPoint, item.Settlement, maximumDistance, out var distance))
			{
				result = item;
				maximumDistance = distance;
			}
		}
		return result;
	}

	public static int FindNextSettlementAroundMapPoint(IMapPoint mapPoint, float maxDistance, int lastIndex)
	{
		for (int i = lastIndex + 1; i < Settlement.All.Count; i++)
		{
			Settlement toSettlement = Settlement.All[i];
			if (Campaign.Current.Models.MapDistanceModel.GetDistance(mapPoint, toSettlement, maxDistance, out var _))
			{
				return i;
			}
		}
		return -1;
	}

	private static Settlement FindRandomInternal(Func<Settlement, bool> condition, IEnumerable<Settlement> settlementsToIterate)
	{
		List<Settlement> list = new List<Settlement>();
		foreach (Settlement item in settlementsToIterate)
		{
			if (condition(item))
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			return list[MBRandom.RandomInt(list.Count)];
		}
		return null;
	}

	public static Settlement FindRandomSettlement(Func<Settlement, bool> condition = null)
	{
		return FindRandomInternal(condition, Settlement.All);
	}

	public static Settlement FindRandomHideout(Func<Settlement, bool> condition = null)
	{
		return FindRandomInternal(condition, Hideout.All.Select((Hideout x) => x.Settlement));
	}

	public static void TakeEnemyVillagersOutsideSettlements(Settlement settlementWhichChangedFaction)
	{
		if (settlementWhichChangedFaction.IsFortification)
		{
			bool flag;
			do
			{
				flag = false;
				MobileParty mobileParty = null;
				foreach (MobileParty party in settlementWhichChangedFaction.Parties)
				{
					if (party.IsVillager && party.HomeSettlement.IsVillage && party.HomeSettlement.Village.Bound == settlementWhichChangedFaction && party.HomeSettlement.MapFaction != settlementWhichChangedFaction.MapFaction)
					{
						mobileParty = party;
						flag = true;
						break;
					}
				}
				if (flag && mobileParty.MapEvent == null)
				{
					LeaveSettlementAction.ApplyForParty(mobileParty);
					mobileParty.Ai.SetMoveModeHold();
				}
			}
			while (flag);
			bool flag2;
			do
			{
				flag2 = false;
				MobileParty mobileParty2 = null;
				foreach (MobileParty party2 in settlementWhichChangedFaction.Parties)
				{
					if (party2.IsCaravan && FactionManager.IsAtWarAgainstFaction(party2.MapFaction, settlementWhichChangedFaction.MapFaction))
					{
						mobileParty2 = party2;
						flag2 = true;
						break;
					}
				}
				if (flag2 && mobileParty2.MapEvent == null)
				{
					LeaveSettlementAction.ApplyForParty(mobileParty2);
					mobileParty2.Ai.SetMoveModeHold();
				}
			}
			while (flag2);
			foreach (MobileParty item in MobileParty.All)
			{
				if ((item.IsVillager || item.IsCaravan) && item.TargetSettlement == settlementWhichChangedFaction && item.CurrentSettlement != settlementWhichChangedFaction)
				{
					item.Ai.SetMoveModeHold();
				}
			}
		}
		if (!settlementWhichChangedFaction.IsVillage)
		{
			return;
		}
		foreach (MobileParty allVillagerParty in MobileParty.AllVillagerParties)
		{
			if (allVillagerParty.HomeSettlement == settlementWhichChangedFaction && allVillagerParty.CurrentSettlement != settlementWhichChangedFaction)
			{
				if (allVillagerParty.CurrentSettlement != null && allVillagerParty.MapEvent == null)
				{
					LeaveSettlementAction.ApplyForParty(allVillagerParty);
					allVillagerParty.Ai.SetMoveModeHold();
				}
				else
				{
					allVillagerParty.Ai.SetMoveModeHold();
				}
			}
		}
	}

	public static Settlement GetRandomTown(Clan fromFaction = null)
	{
		int num = 0;
		foreach (Settlement settlement in Campaign.Current.Settlements)
		{
			if ((fromFaction == null || settlement.MapFaction == fromFaction) && (settlement.IsTown || settlement.IsVillage))
			{
				num++;
			}
		}
		int num2 = MBRandom.RandomInt(0, num - 1);
		foreach (Settlement settlement2 in Campaign.Current.Settlements)
		{
			if ((fromFaction == null || settlement2.MapFaction == fromFaction) && (settlement2.IsTown || settlement2.IsVillage))
			{
				num2--;
				if (num2 < 0)
				{
					return settlement2;
				}
			}
		}
		return null;
	}

	public static Settlement GetBestSettlementToSpawnAround(Hero hero)
	{
		Settlement result = null;
		float num = -1f;
		uint num2 = 0u;
		using (List<Hero>.Enumerator enumerator = hero.Clan.Lords.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current != hero)
			{
				num2++;
			}
		}
		IFaction mapFaction = hero.MapFaction;
		foreach (Settlement item in Settlement.All)
		{
			if (item.Party.MapEvent == null)
			{
				IFaction mapFaction2 = item.MapFaction;
				float num3 = 0.0001f;
				if (mapFaction2 == mapFaction)
				{
					num3 = 1f;
				}
				else if (FactionManager.IsAlliedWithFaction(mapFaction2, mapFaction))
				{
					num3 = 0.01f;
				}
				else if (FactionManager.IsNeutralWithFaction(mapFaction2, mapFaction))
				{
					num3 = 0.0005f;
				}
				float num4 = 0f;
				if (item.IsTown)
				{
					num4 = 1f;
				}
				else if (item.IsCastle)
				{
					num4 = 0.9f;
				}
				else if (item.IsVillage)
				{
					num4 = 0.8f;
				}
				else if (item.IsHideout)
				{
					num4 = ((mapFaction2 == mapFaction) ? 0.2f : 0f);
				}
				float num5 = ((item.Town != null && item.Town.GarrisonParty != null && item.OwnerClan == hero.Clan) ? (item.Town.GarrisonParty.Party.TotalStrength / (item.IsTown ? 60f : 30f)) : 1f);
				float num6 = ((item.IsUnderRaid || item.IsUnderSiege) ? 0.1f : 1f);
				float num7 = ((item.OwnerClan == hero.Clan) ? 1f : 0.25f);
				float num8 = item.RandomFloatWithSeed(num2, 0.5f, 1f);
				float num9 = 1f - hero.MapFaction.InitialPosition.Distance(item.Position2D) / Campaign.MapDiagonal;
				num9 *= num9;
				float num10 = num3 * num4 * num6 * num7 * num5 * num8 * num9;
				if (num10 > num)
				{
					num = num10;
					result = item;
				}
			}
		}
		return result;
	}

	public static IEnumerable<Hero> GetAllHeroesOfSettlement(Settlement settlement, bool includePrisoners)
	{
		foreach (MobileParty party in settlement.Parties)
		{
			if (party.LeaderHero != null)
			{
				yield return party.LeaderHero;
			}
		}
		foreach (Hero item in settlement.HeroesWithoutParty)
		{
			yield return item;
		}
		if (!includePrisoners)
		{
			yield break;
		}
		foreach (TroopRosterElement item2 in settlement.Party.PrisonRoster.GetTroopRoster())
		{
			if (item2.Character.IsHero)
			{
				yield return item2.Character.HeroObject;
			}
		}
	}

	public static int NumberOfVolunteersCanBeRecruitedForGarrison(Settlement settlement)
	{
		int num = 0;
		Hero leader = settlement.OwnerClan.Leader;
		foreach (Hero notable in settlement.Notables)
		{
			if (!notable.IsAlive)
			{
				continue;
			}
			int num2 = Campaign.Current.Models.VolunteerModel.MaximumIndexHeroCanRecruitFromHero(leader, notable);
			for (int i = 0; i < num2; i++)
			{
				if (notable.VolunteerTypes[i] != null)
				{
					num++;
				}
			}
		}
		foreach (Village boundVillage in settlement.BoundVillages)
		{
			if (boundVillage.VillageState == Village.VillageStates.Normal)
			{
				num += NumberOfVolunteersCanBeRecruitedForGarrison(boundVillage.Settlement);
			}
		}
		return num;
	}

	public static bool IsThereAnyVolunteerCanBeRecruitedForGarrison(Settlement settlement)
	{
		Hero leader = settlement.OwnerClan.Leader;
		foreach (Hero notable in settlement.Notables)
		{
			if (!notable.IsAlive)
			{
				continue;
			}
			int num = Campaign.Current.Models.VolunteerModel.MaximumIndexHeroCanRecruitFromHero(leader, notable);
			for (int i = 0; i < num; i++)
			{
				if (notable.VolunteerTypes[i] != null)
				{
					return true;
				}
			}
		}
		foreach (Village boundVillage in settlement.BoundVillages)
		{
			if (boundVillage.VillageState == Village.VillageStates.Normal && IsThereAnyVolunteerCanBeRecruitedForGarrison(boundVillage.Settlement))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsGarrisonStarving(Settlement settlement)
	{
		bool result = false;
		if (settlement.IsStarving)
		{
			result = settlement.Town.FoodChange < 0f - settlement.Town.Prosperity / (float)Campaign.Current.Models.SettlementFoodModel.NumberOfProsperityToEatOneFood;
		}
		return result;
	}

	public static void SpawnNotablesIfNeeded(Settlement settlement)
	{
		if (!settlement.IsTown && !settlement.IsVillage)
		{
			return;
		}
		List<Occupation> list = new List<Occupation>();
		if (settlement.IsTown)
		{
			list = new List<Occupation>
			{
				Occupation.GangLeader,
				Occupation.Artisan,
				Occupation.Merchant
			};
		}
		else if (settlement.IsVillage)
		{
			list = new List<Occupation>
			{
				Occupation.RuralNotable,
				Occupation.Headman
			};
		}
		float randomFloat = MBRandom.RandomFloat;
		float num = 0f;
		int num2 = 0;
		foreach (Occupation item in list)
		{
			num2 += Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, item);
		}
		num = ((settlement.Notables.Count > 0) ? ((float)(num2 - settlement.Notables.Count) / (float)num2) : 1f);
		num *= TaleWorlds.Library.MathF.Pow(num, 0.36f);
		if (!(randomFloat <= num))
		{
			return;
		}
		MBList<Occupation> mBList = new MBList<Occupation>();
		foreach (Occupation item2 in list)
		{
			int num3 = 0;
			foreach (Hero notable in settlement.Notables)
			{
				if (notable.CharacterObject.Occupation == item2)
				{
					num3++;
				}
			}
			int targetNotableCountForSettlement = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, item2);
			if (num3 < targetNotableCountForSettlement)
			{
				mBList.Add(item2);
			}
		}
		if (mBList.Count > 0)
		{
			EnterSettlementAction.ApplyForCharacterOnly(HeroCreator.CreateHeroAtOccupation(mBList.GetRandomElement(), settlement), settlement);
		}
	}
}
