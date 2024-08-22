using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultDailyTroopXpBonusModel : DailyTroopXpBonusModel
{
	public override int CalculateDailyTroopXpBonus(Town town)
	{
		return CalculateTroopXpBonusInternal(town);
	}

	private int CalculateTroopXpBonusInternal(Town town)
	{
		ExplainedNumber bonuses = new ExplainedNumber(0f, includeDescriptions: false, null);
		foreach (Building building in town.Buildings)
		{
			float buildingEffectAmount = building.GetBuildingEffectAmount(BuildingEffectEnum.Experience);
			if (buildingEffectAmount > 0f)
			{
				bonuses.Add(buildingEffectAmount, building.Name);
			}
		}
		PerkHelper.AddPerkBonusForTown(DefaultPerks.Leadership.RaiseTheMeek, town, ref bonuses);
		PerkHelper.AddPerkBonusForTown(DefaultPerks.TwoHanded.ProjectileDeflection, town, ref bonuses);
		return (int)bonuses.ResultNumber;
	}

	public override float CalculateGarrisonXpBonusMultiplier(Town town)
	{
		return 1f;
	}
}
