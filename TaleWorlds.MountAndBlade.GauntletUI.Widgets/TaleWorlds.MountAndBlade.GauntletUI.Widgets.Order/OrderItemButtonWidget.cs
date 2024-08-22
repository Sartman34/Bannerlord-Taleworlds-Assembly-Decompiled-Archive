using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Order;

public class OrderItemButtonWidget : ButtonWidget
{
	private int _selectionState;

	private string _orderIconID;

	private Widget _iconWidget;

	private ImageWidget _selectionVisualWidget;

	[Editor(false)]
	public int SelectionState
	{
		get
		{
			return _selectionState;
		}
		set
		{
			if (_selectionState != value)
			{
				_selectionState = value;
				OnPropertyChanged(value, "SelectionState");
				SelectionStateChanged();
			}
		}
	}

	[Editor(false)]
	public string OrderIconID
	{
		get
		{
			return _orderIconID;
		}
		set
		{
			if (_orderIconID != value)
			{
				_orderIconID = value;
				OnPropertyChanged(value, "OrderIconID");
				UpdateIcon();
			}
		}
	}

	[Editor(false)]
	public Widget IconWidget
	{
		get
		{
			return _iconWidget;
		}
		set
		{
			if (_iconWidget != value)
			{
				_iconWidget = value;
				OnPropertyChanged(value, "IconWidget");
				UpdateIcon();
			}
		}
	}

	[Editor(false)]
	public ImageWidget SelectionVisualWidget
	{
		get
		{
			return _selectionVisualWidget;
		}
		set
		{
			if (_selectionVisualWidget != value)
			{
				_selectionVisualWidget = value;
				OnPropertyChanged(value, "SelectionVisualWidget");
				if (value != null)
				{
					value.AddState("Disabled");
					value.AddState("PartiallyActive");
					value.AddState("Active");
				}
			}
		}
	}

	public OrderItemButtonWidget(UIContext context)
		: base(context)
	{
	}

	private void SelectionStateChanged()
	{
		switch (SelectionState)
		{
		case 3:
			SelectionVisualWidget.SetState("Active");
			break;
		case 2:
			SelectionVisualWidget.SetState("PartiallyActive");
			break;
		case 0:
			SelectionVisualWidget.SetState("Disabled");
			break;
		default:
			SelectionVisualWidget.SetState("Default");
			break;
		}
	}

	private void UpdateIcon()
	{
		if (IconWidget != null && !string.IsNullOrEmpty(OrderIconID))
		{
			IconWidget.Sprite = base.Context.SpriteData.GetSprite("Order\\ItemIcons\\OI" + OrderIconID);
		}
	}
}
