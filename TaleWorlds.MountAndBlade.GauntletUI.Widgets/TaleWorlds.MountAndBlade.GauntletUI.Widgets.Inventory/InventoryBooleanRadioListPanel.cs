using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

public class InventoryBooleanRadioListPanel : ListPanel
{
	private bool _isSelectedStateSet;

	private bool _isFirstSelected;

	[Editor(false)]
	public bool IsFirstSelected
	{
		get
		{
			return _isFirstSelected;
		}
		set
		{
			if (_isFirstSelected != value || !_isSelectedStateSet)
			{
				_isFirstSelected = value;
				OnPropertyChanged(value, "IsFirstSelected");
				_isSelectedStateSet = true;
				UpdateChildSelectedState();
			}
		}
	}

	[Editor(false)]
	public bool IsSecondSelected
	{
		get
		{
			return !_isFirstSelected;
		}
		set
		{
			if (_isFirstSelected != !value)
			{
				IsFirstSelected = !value;
				OnPropertyChanged(!value, "IsSecondSelected");
			}
		}
	}

	public InventoryBooleanRadioListPanel(UIContext context)
		: base(context)
	{
	}

	private void UpdateChildSelectedState()
	{
		if (base.ChildCount >= 2)
		{
			ButtonWidget buttonWidget = GetChild(1) as ButtonWidget;
			ButtonWidget buttonWidget2 = GetChild(0) as ButtonWidget;
			if (buttonWidget != null && buttonWidget2 != null)
			{
				buttonWidget.IsSelected = IsFirstSelected;
				buttonWidget2.IsSelected = !IsFirstSelected;
			}
		}
	}

	public override void OnChildSelected(Widget widget)
	{
		base.OnChildSelected(widget);
		int childIndex = GetChildIndex(widget);
		IsFirstSelected = childIndex == 1;
	}
}
