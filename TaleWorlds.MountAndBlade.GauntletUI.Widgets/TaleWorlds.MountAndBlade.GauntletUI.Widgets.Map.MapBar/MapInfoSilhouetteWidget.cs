using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Map.MapBar;

public class MapInfoSilhouetteWidget : Widget
{
	private string _currentScreen;

	[Editor(false)]
	public string CurrentScreen
	{
		get
		{
			return _currentScreen;
		}
		set
		{
			if (_currentScreen != value)
			{
				_currentScreen = value;
				if (ContainsState(_currentScreen))
				{
					SetState(_currentScreen);
				}
				else
				{
					SetState("Default");
				}
				OnPropertyChanged(value, "CurrentScreen");
			}
		}
	}

	public MapInfoSilhouetteWidget(UIContext context)
		: base(context)
	{
		AddState("MapScreen");
		AddState("InventoryGauntletScreen");
		AddState("GauntletPartyScreen");
		AddState("GauntletCharacterDeveloperScreen");
		AddState("GauntletClanScreen");
		AddState("GauntletQuestsScreen");
	}
}
