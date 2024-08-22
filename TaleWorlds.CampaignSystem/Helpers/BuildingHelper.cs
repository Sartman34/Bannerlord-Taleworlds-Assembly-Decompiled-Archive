using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Library;

namespace Helpers;

public static class BuildingHelper
{
	public static void ChangeCurrentBuilding(BuildingType buildingType, Town town)
	{
		Queue<Building> queue = new Queue<Building>();
		foreach (Building building in town.Buildings)
		{
			if (building.BuildingType == buildingType)
			{
				queue.Enqueue(building);
				break;
			}
		}
		foreach (Building item in town.BuildingsInProgress)
		{
			queue.Enqueue(item);
		}
		town.BuildingsInProgress = queue;
	}

	public static void ChangeDefaultBuilding(Building newDefault, Town town)
	{
		foreach (Building building in town.Buildings)
		{
			if (building.IsCurrentlyDefault)
			{
				building.IsCurrentlyDefault = false;
			}
			if (building == newDefault)
			{
				building.IsCurrentlyDefault = true;
			}
		}
	}

	public static void ChangeCurrentBuildingQueue(List<Building> buildings, Town town)
	{
		town.BuildingsInProgress = new Queue<Building>();
		foreach (Building building in buildings)
		{
			if (!building.BuildingType.IsDefaultProject)
			{
				town.BuildingsInProgress.Enqueue(building);
			}
		}
	}

	public static float GetProgressOfBuilding(Building building, Town town)
	{
		foreach (Building building2 in town.Buildings)
		{
			if (building2 == building)
			{
				return building.BuildingProgress / (float)building.GetConstructionCost();
			}
		}
		Debug.FailedAssert(string.Concat(building.Name, "is not a project of", town.Name), "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Helpers.cs", "GetProgressOfBuilding", 6252);
		return 0f;
	}

	public static int GetDaysToComplete(Building building, Town town)
	{
		BuildingConstructionModel buildingConstructionModel = Campaign.Current.Models.BuildingConstructionModel;
		foreach (Building building2 in town.Buildings)
		{
			if (building2 != building)
			{
				continue;
			}
			float num = (float)building.GetConstructionCost() - building.BuildingProgress;
			int num2 = (int)town.Construction;
			if (num2 != 0)
			{
				int num3 = (int)(num / (float)num2);
				int num4 = (town.IsCastle ? buildingConstructionModel.CastleBoostCost : buildingConstructionModel.TownBoostCost);
				if (town.BoostBuildingProcess >= num4)
				{
					int num5 = town.BoostBuildingProcess / num4;
					if (num3 > num5)
					{
						int num6 = num5 * num2;
						int num7 = Campaign.Current.Models.BuildingConstructionModel.CalculateDailyConstructionPowerWithoutBoost(town);
						return num5 + MathF.Max((int)((num - (float)num6) / (float)num7), 1);
					}
				}
				return MathF.Max(num3, 1);
			}
			return -1;
		}
		Debug.FailedAssert(string.Concat(building.Name, "is not a project of", town.Name), "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Helpers.cs", "GetDaysToComplete", 6294);
		return 0;
	}

	public static int GetTierOfBuilding(BuildingType buildingType, Town town)
	{
		foreach (Building building in town.Buildings)
		{
			if (building.BuildingType == buildingType)
			{
				return building.CurrentLevel;
			}
		}
		Debug.FailedAssert(string.Concat(buildingType.Name, "is not a project of", town.Name), "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Helpers.cs", "GetTierOfBuilding", 6308);
		return 0;
	}

	public static void BoostBuildingProcessWithGold(int gold, Town town)
	{
		if (gold < town.BoostBuildingProcess)
		{
			GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, town.BoostBuildingProcess - gold);
		}
		else if (gold > town.BoostBuildingProcess)
		{
			GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, gold - town.BoostBuildingProcess);
		}
		town.BoostBuildingProcess = gold;
	}

	public static void AddDefaultDailyBonus(Town fortification, BuildingEffectEnum effect, ref ExplainedNumber result)
	{
		float buildingEffectAmount = Campaign.Current.Models.BuildingEffectModel.GetBuildingEffectAmount(fortification.CurrentDefaultBuilding, effect);
		result.Add(buildingEffectAmount, fortification.CurrentDefaultBuilding.BuildingType.Name);
	}
}
