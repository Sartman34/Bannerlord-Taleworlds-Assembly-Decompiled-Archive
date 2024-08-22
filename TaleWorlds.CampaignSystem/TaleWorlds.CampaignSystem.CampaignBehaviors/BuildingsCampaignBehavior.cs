using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class BuildingsCampaignBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
		CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailyTickSettlement);
		CampaignEvents.OnBuildingLevelChangedEvent.AddNonSerializedListener(this, OnBuildingLevelChanged);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnNewGameCreated(CampaignGameStarter starter)
	{
		BuildDevelopmentsAtGameStart();
	}

	private void DecideProject(Town town)
	{
		if (town.Owner.Settlement.OwnerClan == Clan.PlayerClan || town.BuildingsInProgress.Count >= 3)
		{
			return;
		}
		List<Building> list = new List<Building>(town.BuildingsInProgress);
		int num = 100;
		for (int i = 0; i < num; i++)
		{
			Building nextBuilding = Campaign.Current.Models.BuildingScoreCalculationModel.GetNextBuilding(town);
			if (nextBuilding != null)
			{
				list.Add(nextBuilding);
				break;
			}
		}
		BuildingHelper.ChangeCurrentBuildingQueue(list, town);
	}

	private void DailyTickSettlement(Settlement settlement)
	{
		if (!settlement.IsFortification)
		{
			return;
		}
		Town town = settlement.Town;
		foreach (Building building in town.Buildings)
		{
			if (town.Owner.Settlement.SiegeEvent == null)
			{
				building.HitPointChanged(10f);
			}
		}
		DecideProject(town);
	}

	private void OnBuildingLevelChanged(Town town, Building building, int levelChange)
	{
		if (levelChange <= 0)
		{
			return;
		}
		if (town.Governor != null)
		{
			if (town.IsTown && town.Governor.GetPerkValue(DefaultPerks.Charm.MoralLeader))
			{
				foreach (Hero notable in town.Settlement.Notables)
				{
					ChangeRelationAction.ApplyRelationChangeBetweenHeroes(town.Settlement.OwnerClan.Leader, notable, MathF.Round(DefaultPerks.Charm.MoralLeader.SecondaryBonus));
				}
			}
			if (town.Governor.GetPerkValue(DefaultPerks.Engineering.Foreman))
			{
				town.Prosperity += DefaultPerks.Engineering.Foreman.SecondaryBonus;
			}
		}
		SkillLevelingManager.OnSettlementProjectFinished(town.Settlement);
	}

	private static void BuildDevelopmentsAtGameStart()
	{
		foreach (Settlement item in Settlement.All)
		{
			Town town = item.Town;
			if (town == null)
			{
				continue;
			}
			bool haveBuilding = false;
			int level = 0;
			if (town.IsTown)
			{
				foreach (BuildingType item2 in BuildingType.All)
				{
					if (item2.BuildingLocation != 0 || item2 == DefaultBuildingTypes.Fortifications)
					{
						continue;
					}
					GetBuildingProbability(out haveBuilding, out level);
					if (haveBuilding)
					{
						if (level > 3)
						{
							level = 3;
						}
						town.Buildings.Add(new Building(item2, town, 0f, level));
					}
				}
				foreach (BuildingType buildingType2 in BuildingType.All)
				{
					if (!town.Buildings.Any((Building k) => k.BuildingType == buildingType2) && buildingType2.BuildingLocation == BuildingLocation.Settlement)
					{
						town.Buildings.Add(new Building(buildingType2, town));
					}
				}
			}
			else if (town.IsCastle)
			{
				foreach (BuildingType item3 in BuildingType.All)
				{
					if (item3.BuildingLocation != BuildingLocation.Castle || item3 == DefaultBuildingTypes.Wall)
					{
						continue;
					}
					GetBuildingProbability(out haveBuilding, out level);
					if (haveBuilding)
					{
						if (level > 3)
						{
							level = 3;
						}
						town.Buildings.Add(new Building(item3, town, 0f, level));
					}
				}
				foreach (BuildingType buildingType in BuildingType.All)
				{
					if (!town.Buildings.Any((Building k) => k.BuildingType == buildingType) && buildingType.BuildingLocation == BuildingLocation.Castle)
					{
						town.Buildings.Add(new Building(buildingType, town));
					}
				}
			}
			int num = MBRandom.RandomInt(1, 4);
			int num2 = 1;
			foreach (BuildingType item4 in BuildingType.All)
			{
				if (item4.BuildingLocation == BuildingLocation.Daily)
				{
					Building building = new Building(item4, town, 0f, 1);
					town.Buildings.Add(building);
					if (num2 == num)
					{
						building.IsCurrentlyDefault = true;
					}
					num2++;
				}
			}
			foreach (Building item5 in town.Buildings.OrderByDescending((Building k) => k.CurrentLevel))
			{
				if (item5.CurrentLevel != 3 && item5.CurrentLevel != item5.BuildingType.StartLevel && item5.BuildingType.BuildingLocation != BuildingLocation.Daily)
				{
					town.BuildingsInProgress.Enqueue(item5);
				}
			}
		}
	}

	private static void GetBuildingProbability(out bool haveBuilding, out int level)
	{
		level = MBRandom.RandomInt(0, 7);
		if (level < 4)
		{
			haveBuilding = false;
			return;
		}
		haveBuilding = true;
		level -= 3;
	}
}
