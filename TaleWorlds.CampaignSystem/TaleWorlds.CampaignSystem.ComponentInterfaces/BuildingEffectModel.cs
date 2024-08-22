using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class BuildingEffectModel : GameModel
{
	public abstract float GetBuildingEffectAmount(Building building, BuildingEffectEnum effect);
}
