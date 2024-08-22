using TaleWorlds.Library;

namespace TaleWorlds.GauntletUI.ExtraWidgets;

public static class CustomWidgetManager
{
	public static void Initilize()
	{
		WidgetInfo.Reload();
		Debug.Print("Loading GauntletUI Extra Custom Widgets");
	}
}
