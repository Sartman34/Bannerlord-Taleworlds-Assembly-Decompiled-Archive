using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade.ComponentInterfaces;

public abstract class AutoBlockModel : GameModel
{
	public abstract Agent.UsageDirection GetBlockDirection(Mission mission);
}
