using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.SaveLoad;

public class SaveLoadScreenWidget : Widget
{
	private SavedGameTupleButtonWidget _currentSelectedTuple;

	private ButtonWidget _loadButton;

	private bool _isSaving;

	[Editor(false)]
	public ButtonWidget LoadButton
	{
		get
		{
			return _loadButton;
		}
		set
		{
			if (_loadButton != value)
			{
				_loadButton = value;
				OnPropertyChanged(value, "LoadButton");
			}
		}
	}

	[Editor(false)]
	public bool IsSaving
	{
		get
		{
			return _isSaving;
		}
		set
		{
			if (_isSaving != value)
			{
				_isSaving = value;
				OnPropertyChanged(value, "IsSaving");
			}
		}
	}

	public SaveLoadScreenWidget(UIContext context)
		: base(context)
	{
	}

	public void SetCurrentSaveTuple(SavedGameTupleButtonWidget tuple)
	{
		if (tuple != null)
		{
			LoadButton.IsVisible = true;
			_currentSelectedTuple = tuple;
		}
		else
		{
			LoadButton.IsEnabled = false;
			_currentSelectedTuple = null;
		}
	}
}
