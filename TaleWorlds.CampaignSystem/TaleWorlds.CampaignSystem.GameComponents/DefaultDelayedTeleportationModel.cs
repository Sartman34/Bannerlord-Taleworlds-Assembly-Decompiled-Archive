using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultDelayedTeleportationModel : DelayedTeleportationModel
{
	private const float MaximumDistanceForDelay = 300f;

	public override float DefaultTeleportationSpeed => 0.24f;

	public override ExplainedNumber GetTeleportationDelayAsHours(Hero teleportingHero, PartyBase target)
	{
		float distance = 300f;
		IMapPoint mapPoint = teleportingHero.GetMapPoint();
		if (mapPoint != null)
		{
			if (target.IsSettlement)
			{
				if (teleportingHero.CurrentSettlement != null && teleportingHero.CurrentSettlement == target.Settlement)
				{
					distance = 0f;
				}
				else
				{
					Campaign.Current.Models.MapDistanceModel.GetDistance(mapPoint, target.Settlement, 300f, out distance);
				}
			}
			else if (target.IsMobile)
			{
				Campaign.Current.Models.MapDistanceModel.GetDistance(mapPoint, target.MobileParty, 300f, out distance);
			}
		}
		return new ExplainedNumber(distance * DefaultTeleportationSpeed);
	}
}
