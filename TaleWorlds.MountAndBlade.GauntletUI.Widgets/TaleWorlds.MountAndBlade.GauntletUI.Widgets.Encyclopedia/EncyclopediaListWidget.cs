using System.Linq;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Encyclopedia;

public class EncyclopediaListWidget : Widget
{
	private bool _isDirty;

	private bool _isListSizeInitialized;

	private string _lastSelectedItemId;

	private ListPanel _itemList;

	private ScrollbarWidget _itemListScroll;

	[Editor(false)]
	public string LastSelectedItemId
	{
		get
		{
			return _lastSelectedItemId;
		}
		set
		{
			if (_lastSelectedItemId != value)
			{
				_lastSelectedItemId = value;
				OnPropertyChanged(value, "LastSelectedItemId");
				_isDirty = true;
			}
		}
	}

	public ListPanel ItemList
	{
		get
		{
			return _itemList;
		}
		set
		{
			if (_itemList != value)
			{
				_itemList = value;
				_isDirty = true;
				_itemList.EventFire += OnListItemAdded;
			}
		}
	}

	public ScrollbarWidget ItemListScroll
	{
		get
		{
			return _itemListScroll;
		}
		set
		{
			if (_itemListScroll != value)
			{
				_itemListScroll = value;
				_isDirty = true;
			}
		}
	}

	public EncyclopediaListWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (!_isListSizeInitialized && ItemListScroll != null && ItemListScroll.Size.Y != 0f)
		{
			_isListSizeInitialized = true;
			_isDirty = true;
		}
		if (_isDirty)
		{
			_isDirty = false;
			UpdateScrollPosition();
		}
	}

	private void UpdateScrollPosition()
	{
		if (!string.IsNullOrEmpty(LastSelectedItemId) && ItemList != null && ItemListScroll != null)
		{
			Widget widget = ItemList.AllChildren.FirstOrDefault((Widget x) => x is EncyclopediaListItemButtonWidget encyclopediaListItemButtonWidget && encyclopediaListItemButtonWidget.ListItemId == LastSelectedItemId);
			if (widget != null && widget.IsVisible)
			{
				float num = widget.ScaledSuggestedHeight + widget.ScaledMarginTop + widget.ScaledMarginBottom - 2f * base._scaleToUse;
				int visibleSiblingIndex = widget.GetVisibleSiblingIndex();
				float valueForced = num * (float)visibleSiblingIndex - ItemListScroll.Size.Y / 2f;
				ItemListScroll.SetValueForced(valueForced);
			}
		}
	}

	private void OnListItemAdded(Widget widget, string eventName, object[] eventArgs)
	{
		if (eventName == "ItemAdd" && eventArgs.Length != 0 && eventArgs[0] is EncyclopediaListItemButtonWidget)
		{
			_isDirty = true;
		}
	}
}
