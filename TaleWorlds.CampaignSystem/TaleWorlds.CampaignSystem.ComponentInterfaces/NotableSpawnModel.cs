using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class NotableSpawnModel : GameModel
{
	public abstract int GetTargetNotableCountForSettlement(Settlement settlement, Occupation occupation);
}
