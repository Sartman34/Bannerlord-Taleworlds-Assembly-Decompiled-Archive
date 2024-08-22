using TaleWorlds.GauntletUI;
using TaleWorlds.TwoDimension.Standalone;

namespace TaleWorlds.MountAndBlade.Launcher.Library;

public class StandaloneInputService : IInputService
{
	bool IInputService.MouseEnabled => true;

	bool IInputService.KeyboardEnabled => true;

	bool IInputService.GamepadEnabled => false;

	public StandaloneInputService(GraphicsForm graphicsForm)
	{
	}
}
