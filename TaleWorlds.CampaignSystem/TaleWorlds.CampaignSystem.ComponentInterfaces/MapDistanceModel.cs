using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class MapDistanceModel : GameModel
{
	public abstract float MaximumDistanceBetweenTwoSettlements { get; set; }

	public abstract float GetDistance(Settlement fromSettlement, Settlement toSettlement);

	public abstract float GetDistance(MobileParty fromParty, Settlement toSettlement);

	public abstract float GetDistance(MobileParty fromParty, MobileParty toParty);

	public abstract bool GetDistance(Settlement fromSettlement, Settlement toSettlement, float maximumDistance, out float distance);

	public abstract bool GetDistance(MobileParty fromParty, Settlement toSettlement, float maximumDistance, out float distance);

	public abstract bool GetDistance(IMapPoint fromMapPoint, MobileParty toParty, float maximumDistance, out float distance);

	public abstract bool GetDistance(IMapPoint fromMapPoint, Settlement toSettlement, float maximumDistance, out float distance);

	public abstract bool GetDistance(IMapPoint fromMapPoint, in Vec2 toPoint, float maximumDistance, out float distance);

	public abstract Settlement GetClosestSettlementForNavigationMesh(PathFaceRecord face);
}
