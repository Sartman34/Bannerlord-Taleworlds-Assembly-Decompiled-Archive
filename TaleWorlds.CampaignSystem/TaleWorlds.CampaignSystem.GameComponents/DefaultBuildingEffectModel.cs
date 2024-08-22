using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements.Buildings;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultBuildingEffectModel : BuildingEffectModel
{
	public override float GetBuildingEffectAmount(Building building, BuildingEffectEnum effect)
	{
		ExplainedNumber bonuses = new ExplainedNumber(building.BuildingType.GetBaseBuildingEffectAmount(effect, building.CurrentLevel));
		if (effect == BuildingEffectEnum.Foodstock && building.Town.Governor != null && building.Town.Governor.GetPerkValue(DefaultPerks.Engineering.Battlements) && (building.BuildingType == DefaultBuildingTypes.CastleGranary || building.BuildingType == DefaultBuildingTypes.SettlementGranary))
		{
			bonuses.Add(DefaultPerks.Engineering.Battlements.SecondaryBonus, DefaultPerks.Engineering.Battlements.Name);
		}
		if (building.Town.IsTown)
		{
			PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.Contractors, building.Town, ref bonuses);
		}
		if (building.Town.Governor != null && building.Town.Governor.GetPerkValue(DefaultPerks.Steward.MasterOfPlanning))
		{
			bonuses.AddFactor(DefaultPerks.Steward.MasterOfPlanning.SecondaryBonus, DefaultPerks.Steward.MasterOfPlanning.Name);
		}
		Hero governor = building.Town.Governor;
		if (governor != null && governor.GetPerkValue(DefaultPerks.Charm.PublicSpeaker) && (building.BuildingType == DefaultBuildingTypes.SettlementMarketplace || building.BuildingType == DefaultBuildingTypes.FestivalsAndGamesDaily || building.BuildingType == DefaultBuildingTypes.SettlementForum))
		{
			bonuses.AddFactor(DefaultPerks.Charm.PublicSpeaker.SecondaryBonus, DefaultPerks.Charm.PublicSpeaker.Name);
		}
		return bonuses.ResultNumber;
	}
}
