using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace TaleWorlds.CampaignSystem.Actions;

public static class GatherArmyAction
{
	private static void ApplyInternal(MobileParty leaderParty, Settlement targetSettlement, float playerInvolvement = 0f)
	{
		Army army = leaderParty.Army;
		army.AIBehavior = Army.AIBehaviorFlags.Gathering;
		CampaignEventDispatcher.Instance.OnArmyGathered(army, targetSettlement);
	}

	public static void Apply(MobileParty leaderParty, Settlement targetSettlement)
	{
		ApplyInternal(leaderParty, targetSettlement, (leaderParty == MobileParty.MainParty) ? 1f : 0f);
	}
}
