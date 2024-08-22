using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

public class InventoryItemPreviewWidget : Widget
{
	private Widget _lastFocusedWidget;

	private ItemTableauWidget _itemTableau;

	private bool _isPreviewOpen;

	[Editor(false)]
	public bool IsPreviewOpen
	{
		get
		{
			return _isPreviewOpen;
		}
		set
		{
			if (_isPreviewOpen != value)
			{
				if (!value)
				{
					base.EventManager.SetWidgetFocused(_lastFocusedWidget);
					_lastFocusedWidget = null;
				}
				_isPreviewOpen = value;
				base.IsVisible = value;
				OnPropertyChanged(value, "IsPreviewOpen");
			}
		}
	}

	[Editor(false)]
	public ItemTableauWidget ItemTableau
	{
		get
		{
			return _itemTableau;
		}
		set
		{
			if (_itemTableau != value)
			{
				_itemTableau = value;
				OnPropertyChanged(value, "ItemTableau");
			}
		}
	}

	public InventoryItemPreviewWidget(UIContext context)
		: base(context)
	{
	}

	public void SetLastFocusedItem(Widget lastFocusedWidget)
	{
		_lastFocusedWidget = lastFocusedWidget;
	}
}
