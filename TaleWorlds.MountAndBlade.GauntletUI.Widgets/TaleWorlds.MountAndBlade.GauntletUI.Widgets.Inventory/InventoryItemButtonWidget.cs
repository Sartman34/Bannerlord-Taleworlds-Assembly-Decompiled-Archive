using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

public abstract class InventoryItemButtonWidget : ButtonWidget
{
	private bool _isRightSide;

	private int _itemType;

	private int _equipmentIndex;

	private InventoryScreenWidget _screenWidget;

	[Editor(false)]
	public bool IsRightSide
	{
		get
		{
			return _isRightSide;
		}
		set
		{
			if (_isRightSide != value)
			{
				_isRightSide = value;
				OnPropertyChanged(value, "IsRightSide");
			}
		}
	}

	[Editor(false)]
	public int ItemType
	{
		get
		{
			return _itemType;
		}
		set
		{
			if (_itemType != value)
			{
				_itemType = value;
				OnPropertyChanged(value, "ItemType");
				ItemTypeUpdated();
			}
		}
	}

	[Editor(false)]
	public int EquipmentIndex
	{
		get
		{
			return _equipmentIndex;
		}
		set
		{
			if (_equipmentIndex != value)
			{
				_equipmentIndex = value;
				OnPropertyChanged(value, "EquipmentIndex");
			}
		}
	}

	public InventoryScreenWidget ScreenWidget
	{
		get
		{
			if (_screenWidget == null)
			{
				AssignScreenWidget();
			}
			return _screenWidget;
		}
	}

	protected InventoryItemButtonWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnDragBegin()
	{
		ScreenWidget?.ItemWidgetDragBegin(this);
		base.OnDragBegin();
	}

	protected override bool OnDrop()
	{
		ScreenWidget?.ItemWidgetDrop(this);
		return base.OnDrop();
	}

	public virtual void ResetIsSelected()
	{
		base.IsSelected = false;
	}

	public void PreviewItem()
	{
		EventFired("PreviewItem");
	}

	public void SellItem()
	{
		EventFired("SellItem");
	}

	public void EquipItem()
	{
		EventFired("EquipItem");
	}

	public void UnequipItem()
	{
		EventFired("UnequipItem");
	}

	private void AssignScreenWidget()
	{
		Widget widget = this;
		while (widget != base.EventManager.Root && _screenWidget == null)
		{
			if (widget is InventoryScreenWidget)
			{
				_screenWidget = (InventoryScreenWidget)widget;
			}
			else
			{
				widget = widget.ParentWidget;
			}
		}
	}

	private void ItemTypeUpdated()
	{
		AudioProperty audioProperty = base.Brush.SoundProperties.GetEventAudioProperty("DragEnd");
		if (audioProperty == null)
		{
			audioProperty = new AudioProperty();
			base.Brush.SoundProperties.AddEventSound("DragEnd", audioProperty);
		}
		audioProperty.AudioName = GetSound(ItemType);
	}

	private string GetSound(int typeID)
	{
		switch (typeID)
		{
		case 1:
			return "inventory/horse";
		case 2:
			return "inventory/onehanded";
		case 3:
			return "inventory/twohanded";
		case 4:
			return "inventory/polearm";
		case 5:
		case 6:
			return "inventory/quiver";
		case 7:
			return "inventory/shield";
		case 8:
			return "inventory/bow";
		case 9:
			return "inventory/crossbow";
		case 10:
			return "inventory/throwing";
		case 11:
			return "inventory/sack";
		case 12:
			return "inventory/helmet";
		case 13:
			return "inventory/leather";
		case 14:
		case 15:
			return "inventory/leather_lite";
		case 19:
			return "inventory/animal";
		case 20:
			return "inventory/book";
		case 21:
		case 22:
			return "inventory/leather";
		case 23:
			return "inventory/horsearmor";
		case 24:
			return "inventory/perk";
		case 25:
			return "inventory/leather";
		case 26:
			return "inventory/siegeweapon";
		default:
			return "inventory/leather";
		}
	}
}
