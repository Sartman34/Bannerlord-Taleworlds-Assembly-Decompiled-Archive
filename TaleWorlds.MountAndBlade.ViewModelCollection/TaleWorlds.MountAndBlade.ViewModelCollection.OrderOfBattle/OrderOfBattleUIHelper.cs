using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;

internal static class OrderOfBattleUIHelper
{
	internal static Team.TroopFilter GetTroopFilterForClass(params FormationClass[] formationClasses)
	{
		Team.TroopFilter result = Team.TroopFilter.Melee;
		if (formationClasses.Length == 1)
		{
			FormationClass num = formationClasses[0];
			if (num == FormationClass.Infantry)
			{
				result = Team.TroopFilter.Melee;
			}
			if (num == FormationClass.Ranged)
			{
				result = Team.TroopFilter.Ranged;
			}
			if (num == FormationClass.Cavalry)
			{
				result = Team.TroopFilter.Mount | Team.TroopFilter.Melee;
			}
			if (num == FormationClass.HorseArcher)
			{
				result = Team.TroopFilter.Mount | Team.TroopFilter.Ranged;
			}
		}
		else if (formationClasses.Length == 2)
		{
			if (formationClasses[0] == FormationClass.Infantry && formationClasses[1] == FormationClass.Ranged)
			{
				result = Team.TroopFilter.Ranged | Team.TroopFilter.Melee;
			}
			if (formationClasses[0] == FormationClass.Cavalry && formationClasses[1] == FormationClass.HorseArcher)
			{
				result = Team.TroopFilter.Mount | Team.TroopFilter.Ranged | Team.TroopFilter.Melee;
			}
		}
		return result;
	}

	internal static Team.TroopFilter GetTroopFilterForFormationFilter(params FormationFilterType[] filterTypes)
	{
		if (filterTypes.Length == 0)
		{
			return (Team.TroopFilter)0;
		}
		Team.TroopFilter troopFilter = (Team.TroopFilter)0;
		if (filterTypes.Any((FormationFilterType f) => f == FormationFilterType.Heavy))
		{
			troopFilter |= Team.TroopFilter.Armor;
		}
		if (filterTypes.Any((FormationFilterType f) => f == FormationFilterType.Shield))
		{
			troopFilter |= Team.TroopFilter.Shield;
		}
		if (filterTypes.Any((FormationFilterType f) => f == FormationFilterType.Thrown))
		{
			troopFilter |= Team.TroopFilter.Thrown;
		}
		if (filterTypes.Any((FormationFilterType f) => f == FormationFilterType.Spear))
		{
			troopFilter |= Team.TroopFilter.Spear;
		}
		if (filterTypes.Any((FormationFilterType f) => f == FormationFilterType.HighTier))
		{
			troopFilter |= Team.TroopFilter.HighTier;
		}
		if (filterTypes.Any((FormationFilterType f) => f == FormationFilterType.LowTier))
		{
			troopFilter |= Team.TroopFilter.LowTier;
		}
		return troopFilter;
	}

	internal static List<Agent> GetExcludedAgentsForTransfer(OrderOfBattleFormationItemVM formationVM, FormationClass formationClass)
	{
		List<Agent> list = new List<Agent>();
		if (formationVM.HasCommander)
		{
			list.Add(formationVM.Commander.Agent);
		}
		if (formationVM.HeroTroops.Count > 0)
		{
			list.AddRange(formationVM.HeroTroops.Select((OrderOfBattleHeroItemVM t) => t.Agent));
		}
		foreach (Agent allUnit in formationVM.Formation.Arrangement.GetAllUnits())
		{
			if (allUnit.Banner != null || !IsAgentInFormationClass(allUnit, formationClass))
			{
				list.Add(allUnit);
			}
		}
		return list.Distinct().ToList();
	}

	internal static Tuple<Formation, int, Team.TroopFilter, List<Agent>> CreateMassTransferData(OrderOfBattleFormationClassVM affectedClass, FormationClass formationClass, Team.TroopFilter filter, int unitCount)
	{
		List<Agent> excludedAgentsForTransfer = GetExcludedAgentsForTransfer(affectedClass.BelongedFormationItem, formationClass);
		return new Tuple<Formation, int, Team.TroopFilter, List<Agent>>(affectedClass.BelongedFormationItem.Formation, unitCount, filter, excludedAgentsForTransfer);
	}

	internal static Tuple<Formation, int, Team.TroopFilter, List<Agent>> CreateMassTransferData(OrderOfBattleFormationItemVM affectedFormation, FormationClass formationClass, Team.TroopFilter filter, int unitCount)
	{
		List<Agent> excludedAgentsForTransfer = GetExcludedAgentsForTransfer(affectedFormation, formationClass);
		return new Tuple<Formation, int, Team.TroopFilter, List<Agent>>(affectedFormation.Formation, unitCount, filter, excludedAgentsForTransfer);
	}

	internal static (int, bool, bool) GetRelevantTroopTransferParameters(OrderOfBattleFormationClassVM classVM)
	{
		if (classVM == null)
		{
			return (0, false, false);
		}
		DeploymentFormationClass orderOfBattleFormationClass = classVM.Class.GetOrderOfBattleFormationClass();
		bool item = orderOfBattleFormationClass == DeploymentFormationClass.Ranged || orderOfBattleFormationClass == DeploymentFormationClass.HorseArcher;
		bool item2 = orderOfBattleFormationClass == DeploymentFormationClass.Cavalry || orderOfBattleFormationClass == DeploymentFormationClass.HorseArcher;
		return (GetTotalCountOfUnitsInClass(classVM.BelongedFormationItem.Formation, classVM.Class), item, item2);
	}

	internal static OrderOfBattleFormationClassVM GetFormationClassWithExtremumWeight(List<OrderOfBattleFormationClassVM> classes, bool isMinimum)
	{
		if (classes.Count == 0)
		{
			return null;
		}
		if (classes.Count == 1)
		{
			return classes[0];
		}
		OrderOfBattleFormationClassVM orderOfBattleFormationClassVM = classes[0];
		for (int i = 1; i < classes.Count; i++)
		{
			if ((isMinimum && classes[i].Weight < orderOfBattleFormationClassVM.Weight) || (!isMinimum && classes[i].Weight > orderOfBattleFormationClassVM.Weight))
			{
				orderOfBattleFormationClassVM = classes[i];
			}
		}
		return orderOfBattleFormationClassVM;
	}

	internal static List<OrderOfBattleFormationClassVM> GetMatchingClasses(List<OrderOfBattleFormationItemVM> formationList, OrderOfBattleFormationClassVM formationClass, Func<OrderOfBattleFormationClassVM, bool> predicate = null)
	{
		List<OrderOfBattleFormationClassVM> list = new List<OrderOfBattleFormationClassVM>();
		for (int i = 0; i < formationList.Count; i++)
		{
			OrderOfBattleFormationItemVM orderOfBattleFormationItemVM = formationList[i];
			for (int j = 0; j < orderOfBattleFormationItemVM.Classes.Count; j++)
			{
				OrderOfBattleFormationClassVM orderOfBattleFormationClassVM = orderOfBattleFormationItemVM.Classes[j];
				if (orderOfBattleFormationClassVM.Class == formationClass.Class && (predicate == null || predicate(orderOfBattleFormationClassVM)))
				{
					list.Add(orderOfBattleFormationClassVM);
				}
			}
		}
		return list;
	}

	internal static bool IsAgentInFormationClass(Agent agent, FormationClass fc)
	{
		return fc switch
		{
			FormationClass.Infantry => QueryLibrary.IsInfantry(agent), 
			FormationClass.Ranged => QueryLibrary.IsRanged(agent), 
			FormationClass.Cavalry => QueryLibrary.IsCavalry(agent), 
			FormationClass.HorseArcher => QueryLibrary.IsRangedCavalry(agent), 
			_ => false, 
		};
	}

	private static List<Agent> GetBannerBearersOfFormation(Formation formation)
	{
		List<Agent> list = Mission.Current?.GetMissionBehavior<BannerBearerLogic>()?.GetFormationBannerBearers(formation);
		if (list != null)
		{
			return list;
		}
		return new List<Agent>();
	}

	private static int GetCountOfUnitsInClass(OrderOfBattleFormationClassVM classVM, bool includeHeroes, bool includeBannerBearers)
	{
		Formation formation = classVM.BelongedFormationItem.Formation;
		FormationClass fc = classVM.Class;
		return formation.GetCountOfUnitsWithCondition(delegate(Agent agent)
		{
			if (!includeHeroes && agent.IsHero)
			{
				return false;
			}
			return (includeBannerBearers || agent.Banner == null) && IsAgentInFormationClass(agent, fc);
		});
	}

	internal static int GetTotalCountOfUnitsInClass(Formation formation, FormationClass fc)
	{
		return fc switch
		{
			FormationClass.Infantry => TaleWorlds.Library.MathF.Round(formation.QuerySystem.InfantryUnitRatio * (float)formation.CountOfUnits), 
			FormationClass.Ranged => TaleWorlds.Library.MathF.Round(formation.QuerySystem.RangedUnitRatio * (float)formation.CountOfUnits), 
			FormationClass.Cavalry => TaleWorlds.Library.MathF.Round(formation.QuerySystem.CavalryUnitRatio * (float)formation.CountOfUnits), 
			FormationClass.HorseArcher => TaleWorlds.Library.MathF.Round(formation.QuerySystem.RangedCavalryUnitRatio * (float)formation.CountOfUnits), 
			_ => 0, 
		};
	}

	internal static int GetCountOfRealUnitsInClass(OrderOfBattleFormationClassVM classVM)
	{
		return GetTotalCountOfUnitsInClass(classVM.BelongedFormationItem.Formation, classVM.Class);
	}

	internal static int GetVisibleCountOfUnitsInClass(OrderOfBattleFormationClassVM classVM)
	{
		OrderOfBattleFormationItemVM belongedFormationItem = classVM.BelongedFormationItem;
		Formation formation = classVM.BelongedFormationItem.Formation;
		if (belongedFormationItem.Classes.Where((OrderOfBattleFormationClassVM c) => !c.IsUnset).ToList().Count == 1)
		{
			return classVM.BelongedFormationItem.Formation.CountOfUnits;
		}
		int num = GetCountOfUnitsInClass(classVM, includeHeroes: true, includeBannerBearers: false);
		if (classVM.Class == FormationClass.Infantry || classVM.Class == FormationClass.Cavalry)
		{
			num += GetBannerBearersOfFormation(formation).Count;
		}
		return num;
	}
}
