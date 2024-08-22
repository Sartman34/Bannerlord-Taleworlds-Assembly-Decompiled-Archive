using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultBuildingScoreCalculationModel : BuildingScoreCalculationModel
{
	public override Building GetNextBuilding(Town town)
	{
		Building building = town.Buildings[MBRandom.RandomInt(0, town.Buildings.Count)];
		if (building.CurrentLevel != 3 && !town.BuildingsInProgress.Contains(building) && building.BuildingType.BuildingLocation != BuildingLocation.Daily)
		{
			return building;
		}
		return null;
	}
}
