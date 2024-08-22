using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class InventoryCapacityModel : GameModel
{
	public abstract ExplainedNumber CalculateInventoryCapacity(MobileParty mobileParty, bool includeDescriptions = false, int additionalManOnFoot = 0, int additionalSpareMounts = 0, int additionalPackAnimals = 0, bool includeFollowers = false);

	public abstract int GetItemAverageWeight();
}
