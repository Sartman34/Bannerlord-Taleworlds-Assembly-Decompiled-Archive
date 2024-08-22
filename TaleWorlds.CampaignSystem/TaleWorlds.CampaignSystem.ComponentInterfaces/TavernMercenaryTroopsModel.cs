using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class TavernMercenaryTroopsModel : GameModel
{
	public abstract float RegularMercenariesSpawnChance { get; }
}
