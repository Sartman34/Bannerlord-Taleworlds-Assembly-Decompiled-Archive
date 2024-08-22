using TaleWorlds.Engine;

namespace TaleWorlds.MountAndBlade;

public interface IEntityFactory
{
	GameEntity MakeEntity(params object[] paramObjects);
}
