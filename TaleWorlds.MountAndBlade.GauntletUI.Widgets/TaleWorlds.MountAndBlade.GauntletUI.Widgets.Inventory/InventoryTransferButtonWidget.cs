using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

public class InventoryTransferButtonWidget : ButtonWidget
{
	private bool _isSell;

	private bool _modifySiblingIndex;

	private Brush _buyBrush;

	private Brush _sellBrush;

	[Editor(false)]
	public bool IsSell
	{
		get
		{
			return _isSell;
		}
		set
		{
			if (_isSell != value)
			{
				_isSell = value;
				HandleVisuals();
				OnPropertyChanged(value, "IsSell");
			}
		}
	}

	[Editor(false)]
	public bool ModifySiblingIndex
	{
		get
		{
			return _modifySiblingIndex;
		}
		set
		{
			if (_modifySiblingIndex != value)
			{
				_modifySiblingIndex = value;
				HandleVisuals();
				OnPropertyChanged(value, "ModifySiblingIndex");
			}
		}
	}

	[Editor(false)]
	public Brush BuyBrush
	{
		get
		{
			return _buyBrush;
		}
		set
		{
			if (_buyBrush != value)
			{
				_buyBrush = value;
				OnPropertyChanged(value, "BuyBrush");
			}
		}
	}

	[Editor(false)]
	public Brush SellBrush
	{
		get
		{
			return _sellBrush;
		}
		set
		{
			if (_sellBrush != value)
			{
				_sellBrush = value;
				OnPropertyChanged(value, "SellBrush");
			}
		}
	}

	public InventoryTransferButtonWidget(UIContext context)
		: base(context)
	{
	}

	public void FireClickEvent()
	{
		if (IsSell)
		{
			EventFired("SellAction");
		}
		else
		{
			EventFired("BuyAction");
		}
	}

	private void HandleVisuals()
	{
		int index;
		Brush brush;
		if (IsSell)
		{
			index = 0;
			brush = SellBrush;
		}
		else
		{
			index = base.ParentWidget.ParentWidget.ChildCount - 1;
			brush = BuyBrush;
		}
		if (ModifySiblingIndex)
		{
			base.ParentWidget.SetSiblingIndex(index);
		}
		base.Brush = brush;
	}
}
