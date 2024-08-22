using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.SaveLoad;

public class SavedGameTupleButtonWidget : ButtonWidget
{
	private SaveLoadScreenWidget _screenWidget;

	[Editor(false)]
	public SaveLoadScreenWidget ScreenWidget
	{
		get
		{
			return _screenWidget;
		}
		set
		{
			if (_screenWidget != value)
			{
				_screenWidget = value;
				OnPropertyChanged(value, "ScreenWidget");
			}
		}
	}

	public SavedGameTupleButtonWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnClick()
	{
		base.OnClick();
		if (ScreenWidget != null)
		{
			ScreenWidget.SetCurrentSaveTuple(this);
		}
	}

	private void OnSaveDeletion(Widget widget)
	{
		ScreenWidget.SetCurrentSaveTuple(null);
	}
}
