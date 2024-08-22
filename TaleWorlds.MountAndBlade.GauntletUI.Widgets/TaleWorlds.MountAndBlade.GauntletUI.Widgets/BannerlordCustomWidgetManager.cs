using TaleWorlds.GauntletUI;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public static class BannerlordCustomWidgetManager
{
	public static void Initialize()
	{
		WidgetInfo.Reload();
		Debug.Print("Loading GauntletUI Bannerlord Custom Widgets");
	}
}
