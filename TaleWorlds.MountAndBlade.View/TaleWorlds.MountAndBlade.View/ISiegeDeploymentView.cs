using TaleWorlds.Engine;

namespace TaleWorlds.MountAndBlade.View;

public interface ISiegeDeploymentView
{
	void OnEntitySelection(GameEntity selectedEntity);

	void OnEntityHover(GameEntity hoveredEntity);
}
