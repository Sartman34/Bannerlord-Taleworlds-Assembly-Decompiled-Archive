using System.Linq;
using System.Numerics;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.ExtraWidgets;
using TaleWorlds.GauntletUI.GamepadNavigation;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

public class InventoryScreenWidget : Widget
{
	private readonly int TooltipHideFrameLength = 2;

	private InventoryItemButtonWidget _currentSelectedItemWidget;

	private InventoryItemButtonWidget _currentSelectedOtherItemWidget;

	private InventoryItemButtonWidget _currentHoveredItemWidget;

	private InventoryItemButtonWidget _currentDraggedItemWidget;

	private InventoryItemButtonWidget _lastDisplayedTooltipItem;

	private int _tooltipHiddenFrameCount;

	private bool _eventsRegistered;

	private int _scrollToBannersInFrames = -1;

	private InputKeyVisualWidget _previousCharacterInputKeyVisual;

	private InputKeyVisualWidget _nextCharacterInputKeyVisual;

	private InventoryItemTupleWidget _newAddedItem;

	private Widget _previousCharacterInputVisualParent;

	private Widget _nextCharacterInputVisualParent;

	private InputKeyVisualWidget _transferInputKeyVisualWidget;

	private RichTextWidget _tradeLabel;

	private Widget _inventoryTooltip;

	private InventoryEquippedItemControlsBrushWidget _equippedItemControls;

	private InventoryItemPreviewWidget _itemPreviewWidget;

	private int _transactionCount;

	private bool _isInWarSet;

	private int _targetEquipmentIndex;

	private TextWidget _otherInventoryGoldText;

	private Widget _otherInventoryGoldImage;

	private ScrollablePanel _otherInventoryListWidget;

	private ScrollablePanel _playerInventoryListWidget;

	private bool _focusLostThisFrame;

	private bool _isFocusedOnItemList;

	private bool _isBannerTutorialActive;

	private int _bannerTypeCode;

	[Editor(false)]
	public InputKeyVisualWidget TransferInputKeyVisualWidget
	{
		get
		{
			return _transferInputKeyVisualWidget;
		}
		set
		{
			if (_transferInputKeyVisualWidget != value)
			{
				_transferInputKeyVisualWidget = value;
				OnPropertyChanged(value, "TransferInputKeyVisualWidget");
			}
		}
	}

	public Widget PreviousCharacterInputVisualParent
	{
		get
		{
			return _previousCharacterInputVisualParent;
		}
		set
		{
			if (value == _previousCharacterInputVisualParent)
			{
				return;
			}
			_previousCharacterInputVisualParent = value;
			if (_previousCharacterInputVisualParent != null)
			{
				_previousCharacterInputKeyVisual = _previousCharacterInputVisualParent.Children.FirstOrDefault((Widget x) => x is InputKeyVisualWidget) as InputKeyVisualWidget;
			}
		}
	}

	public Widget NextCharacterInputVisualParent
	{
		get
		{
			return _nextCharacterInputVisualParent;
		}
		set
		{
			if (value == _nextCharacterInputVisualParent)
			{
				return;
			}
			_nextCharacterInputVisualParent = value;
			if (_nextCharacterInputVisualParent != null)
			{
				_nextCharacterInputKeyVisual = _nextCharacterInputVisualParent.Children.FirstOrDefault((Widget x) => x is InputKeyVisualWidget) as InputKeyVisualWidget;
			}
		}
	}

	[Editor(false)]
	public RichTextWidget TradeLabel
	{
		get
		{
			return _tradeLabel;
		}
		set
		{
			if (_tradeLabel != value)
			{
				if (_tradeLabel != null)
				{
					_tradeLabel.PropertyChanged -= TradeLabelOnPropertyChanged;
				}
				_tradeLabel = value;
				if (_tradeLabel != null)
				{
					_tradeLabel.PropertyChanged += TradeLabelOnPropertyChanged;
				}
				OnPropertyChanged(value, "TradeLabel");
			}
		}
	}

	[Editor(false)]
	public InventoryEquippedItemControlsBrushWidget EquippedItemControls
	{
		get
		{
			return _equippedItemControls;
		}
		set
		{
			if (_equippedItemControls != value)
			{
				if (_equippedItemControls != null)
				{
					_equippedItemControls.OnPreviewClick -= EquippedItemControlsOnPreviewClick;
					_equippedItemControls.OnHidePanel -= OnEquipmentControlsHidden;
				}
				_equippedItemControls = value;
				if (_equippedItemControls != null)
				{
					_equippedItemControls.OnPreviewClick += EquippedItemControlsOnPreviewClick;
					_equippedItemControls.OnHidePanel += OnEquipmentControlsHidden;
				}
				OnPropertyChanged(value, "EquippedItemControls");
			}
		}
	}

	[Editor(false)]
	public Widget InventoryTooltip
	{
		get
		{
			return _inventoryTooltip;
		}
		set
		{
			if (_inventoryTooltip != value)
			{
				_inventoryTooltip = value;
				OnPropertyChanged(value, "InventoryTooltip");
			}
		}
	}

	[Editor(false)]
	public InventoryItemPreviewWidget ItemPreviewWidget
	{
		get
		{
			return _itemPreviewWidget;
		}
		set
		{
			if (_itemPreviewWidget != value)
			{
				_itemPreviewWidget = value;
				OnPropertyChanged(value, "ItemPreviewWidget");
			}
		}
	}

	[Editor(false)]
	public int TransactionCount
	{
		get
		{
			return _transactionCount;
		}
		set
		{
			if (_transactionCount != value)
			{
				_transactionCount = value;
				OnPropertyChanged(value, "TransactionCount");
			}
		}
	}

	[Editor(false)]
	public bool IsInWarSet
	{
		get
		{
			return _isInWarSet;
		}
		set
		{
			if (_isInWarSet != value)
			{
				_isInWarSet = value;
				OnPropertyChanged(value, "IsInWarSet");
			}
		}
	}

	[Editor(false)]
	public int TargetEquipmentIndex
	{
		get
		{
			return _targetEquipmentIndex;
		}
		set
		{
			if (_targetEquipmentIndex != value)
			{
				_targetEquipmentIndex = value;
				OnPropertyChanged(value, "TargetEquipmentIndex");
			}
		}
	}

	[Editor(false)]
	public TextWidget OtherInventoryGoldText
	{
		get
		{
			return _otherInventoryGoldText;
		}
		set
		{
			if (value != _otherInventoryGoldText)
			{
				if (_otherInventoryGoldText != null)
				{
					_otherInventoryGoldText.intPropertyChanged -= OtherInventoryGoldTextOnPropertyChanged;
				}
				_otherInventoryGoldText = value;
				if (_otherInventoryGoldText != null)
				{
					_otherInventoryGoldText.intPropertyChanged += OtherInventoryGoldTextOnPropertyChanged;
				}
				OnPropertyChanged(value, "OtherInventoryGoldText");
			}
		}
	}

	[Editor(false)]
	public Widget OtherInventoryGoldImage
	{
		get
		{
			return _otherInventoryGoldImage;
		}
		set
		{
			if (value != _otherInventoryGoldImage)
			{
				_otherInventoryGoldImage = value;
				OnPropertyChanged(value, "OtherInventoryGoldImage");
			}
		}
	}

	[Editor(false)]
	public ScrollablePanel OtherInventoryListWidget
	{
		get
		{
			return _otherInventoryListWidget;
		}
		set
		{
			if (value != _otherInventoryListWidget)
			{
				_otherInventoryListWidget = value;
				OnPropertyChanged(value, "OtherInventoryListWidget");
			}
		}
	}

	[Editor(false)]
	public ScrollablePanel PlayerInventoryListWidget
	{
		get
		{
			return _playerInventoryListWidget;
		}
		set
		{
			if (value != _playerInventoryListWidget)
			{
				_playerInventoryListWidget = value;
				OnPropertyChanged(value, "PlayerInventoryListWidget");
			}
		}
	}

	[Editor(false)]
	public bool IsFocusedOnItemList
	{
		get
		{
			return _isFocusedOnItemList;
		}
		set
		{
			if (value != _isFocusedOnItemList)
			{
				_isFocusedOnItemList = value;
				OnPropertyChanged(value, "IsFocusedOnItemList");
			}
		}
	}

	[Editor(false)]
	public bool IsBannerTutorialActive
	{
		get
		{
			return _isBannerTutorialActive;
		}
		set
		{
			if (value != _isBannerTutorialActive)
			{
				_isBannerTutorialActive = value;
				OnPropertyChanged(value, "IsBannerTutorialActive");
				if (value)
				{
					_scrollToBannersInFrames = 1;
				}
			}
		}
	}

	[Editor(false)]
	public int BannerTypeCode
	{
		get
		{
			return _bannerTypeCode;
		}
		set
		{
			if (value != _bannerTypeCode)
			{
				_bannerTypeCode = value;
				OnPropertyChanged(value, "BannerTypeCode");
			}
		}
	}

	public InventoryScreenWidget(UIContext context)
		: base(context)
	{
	}

	private T IsWidgetChildOfType<T>(Widget currentWidget) where T : Widget
	{
		while (currentWidget != null)
		{
			if (currentWidget is T)
			{
				return (T)currentWidget;
			}
			currentWidget = currentWidget.ParentWidget;
		}
		return null;
	}

	private bool IsWidgetChildOf(Widget parentWidget, Widget currentWidget)
	{
		while (currentWidget != null)
		{
			if (currentWidget == parentWidget)
			{
				return true;
			}
			currentWidget = currentWidget.ParentWidget;
		}
		return false;
	}

	private bool IsWidgetChildOfId(string parentId, Widget currentWidget)
	{
		while (currentWidget != null)
		{
			if (currentWidget.Id == parentId)
			{
				return true;
			}
			currentWidget = currentWidget.ParentWidget;
		}
		return false;
	}

	private InventoryListPanel GetCurrentHoveredListPanel()
	{
		for (int i = 0; i < base.EventManager.MouseOveredViews.Count; i++)
		{
			if (base.EventManager.MouseOveredViews[i] is InventoryListPanel result)
			{
				return result;
			}
		}
		return null;
	}

	private Widget GetFirstBannerItem()
	{
		return ((OtherInventoryListWidget.InnerPanel as ListPanel)?.GetChild(0) as ListPanel)?.FindChild((Widget x) => (x as InventoryItemTupleWidget).ItemType == BannerTypeCode);
	}

	protected override void OnUpdate(float dt)
	{
		base.OnUpdate(dt);
		if (!_eventsRegistered)
		{
			((OtherInventoryListWidget.InnerPanel as ListPanel)?.GetChild(0) as ListPanel)?.ItemAddEventHandlers.Add(OnNewInventoryItemAdded);
			((PlayerInventoryListWidget.InnerPanel as ListPanel)?.GetChild(0) as ListPanel)?.ItemAddEventHandlers.Add(OnNewInventoryItemAdded);
			_eventsRegistered = true;
		}
		if (base.EventManager.DraggedWidget == null)
		{
			TargetEquipmentIndex = -1;
			_currentDraggedItemWidget = null;
		}
		Widget latestMouseDownWidget = base.EventManager.LatestMouseDownWidget;
		int num;
		if (latestMouseDownWidget != null)
		{
			if (!(latestMouseDownWidget is InventoryItemButtonWidget))
			{
				InventoryEquippedItemControlsBrushWidget equippedItemControls = EquippedItemControls;
				if (equippedItemControls == null || !equippedItemControls.CheckIsMyChildRecursive(latestMouseDownWidget))
				{
					InventoryItemButtonWidget currentSelectedItemWidget = _currentSelectedItemWidget;
					if (currentSelectedItemWidget == null || !currentSelectedItemWidget.CheckIsMyChildRecursive(latestMouseDownWidget))
					{
						num = ((_currentSelectedOtherItemWidget?.CheckIsMyChildRecursive(latestMouseDownWidget) ?? false) ? 1 : 0);
						goto IL_010a;
					}
				}
			}
			num = 1;
		}
		else
		{
			num = 0;
		}
		goto IL_010a;
		IL_010a:
		bool flag = (byte)num != 0;
		bool flag2 = IsWidgetChildOf(InventoryTooltip, latestMouseDownWidget);
		if (latestMouseDownWidget == null || (_currentSelectedItemWidget != null && !flag && !flag2 && !ItemPreviewWidget.IsVisible))
		{
			SetCurrentTuple(null, isLeftSide: false);
		}
		Widget hoveredView = base.EventManager.HoveredView;
		if (hoveredView != null)
		{
			InventoryItemButtonWidget inventoryItemButtonWidget = IsWidgetChildOfType<InventoryItemButtonWidget>(hoveredView);
			bool flag3 = IsWidgetChildOfId("InventoryTooltip", hoveredView);
			if (inventoryItemButtonWidget != null)
			{
				ItemWidgetHoverBegin(inventoryItemButtonWidget);
			}
			else if (flag3 && GauntletGamepadNavigationManager.Instance.IsCursorMovingForNavigation)
			{
				ItemWidgetHoverEnd(null);
			}
			else if (!flag3 && hoveredView.ParentWidget != null)
			{
				ItemWidgetHoverEnd(null);
			}
		}
		else
		{
			ItemWidgetHoverEnd(null);
		}
		UpdateControllerTransferKeyVisuals();
	}

	private void UpdateControllerTransferKeyVisuals()
	{
		InventoryListPanel currentHoveredListPanel = GetCurrentHoveredListPanel();
		IsFocusedOnItemList = currentHoveredListPanel != null;
		if (base.EventManager.IsControllerActive && IsFocusedOnItemList)
		{
			PreviousCharacterInputVisualParent.IsVisible = false;
			NextCharacterInputVisualParent.IsVisible = false;
			if (_currentHoveredItemWidget is InventoryItemTupleWidget { IsHovered: not false, IsTransferable: not false } inventoryItemTupleWidget)
			{
				TransferInputKeyVisualWidget.IsVisible = true;
				Vector2 vector;
				if (inventoryItemTupleWidget.IsRightSide)
				{
					TransferInputKeyVisualWidget.KeyID = _nextCharacterInputKeyVisual?.KeyID ?? "";
					vector = _currentHoveredItemWidget.GlobalPosition - new Vector2(base.EventManager.LeftUsableAreaStart, base.EventManager.TopUsableAreaStart + 20f * base._scaleToUse);
				}
				else
				{
					TransferInputKeyVisualWidget.KeyID = _previousCharacterInputKeyVisual?.KeyID ?? "";
					vector = _currentHoveredItemWidget.GlobalPosition - new Vector2(base.EventManager.LeftUsableAreaStart + 60f * base._scaleToUse - _currentHoveredItemWidget.Size.X, base.EventManager.TopUsableAreaStart + 20f * base._scaleToUse);
				}
				TransferInputKeyVisualWidget.ScaledPositionXOffset = vector.X;
				TransferInputKeyVisualWidget.ScaledPositionYOffset = vector.Y;
			}
			else
			{
				TransferInputKeyVisualWidget.IsVisible = false;
			}
		}
		else
		{
			PreviousCharacterInputVisualParent.IsVisible = true;
			NextCharacterInputVisualParent.IsVisible = true;
			TransferInputKeyVisualWidget.IsVisible = false;
		}
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (_newAddedItem != null)
		{
			if (_newAddedItem.ItemID == (_currentSelectedItemWidget as InventoryItemTupleWidget)?.ItemID)
			{
				_currentSelectedOtherItemWidget = _newAddedItem;
				(_currentSelectedOtherItemWidget as InventoryItemTupleWidget).TransferRequestHandlers.Add(OnTransferItemRequested);
				_newAddedItem.IsSelected = true;
				UpdateScrollTarget(_newAddedItem.IsRightSide);
			}
			_newAddedItem = null;
		}
		if (_scrollToBannersInFrames > -1)
		{
			if (_scrollToBannersInFrames == 0)
			{
				OtherInventoryListWidget.ScrollToChild(GetFirstBannerItem(), -1f, 0.2f, 0, 0, 0.35f);
			}
			_scrollToBannersInFrames--;
		}
		if (_focusLostThisFrame)
		{
			EventFired("OnFocusLose");
			_focusLostThisFrame = false;
		}
		UpdateTooltipPosition();
	}

	private void UpdateTooltipPosition()
	{
		if (base.EventManager.DraggedWidget != null)
		{
			InventoryTooltip.IsHidden = true;
		}
		if (_currentHoveredItemWidget?.ParentWidget != null)
		{
			if (_tooltipHiddenFrameCount < TooltipHideFrameLength)
			{
				_tooltipHiddenFrameCount++;
				InventoryTooltip.PositionXOffset = 5000f;
				InventoryTooltip.PositionYOffset = 5000f;
				return;
			}
			if (_currentHoveredItemWidget.IsRightSide)
			{
				InventoryTooltip.ScaledPositionXOffset = _currentHoveredItemWidget.ParentWidget.GlobalPosition.X - InventoryTooltip.Size.X + 10f * base._scaleToUse - base.EventManager.LeftUsableAreaStart;
			}
			else
			{
				InventoryTooltip.ScaledPositionXOffset = _currentHoveredItemWidget.ParentWidget.GlobalPosition.X + _currentHoveredItemWidget.ParentWidget.Size.X - 10f * base._scaleToUse - base.EventManager.LeftUsableAreaStart;
			}
			float max = base.EventManager.PageSize.Y - InventoryTooltip.MeasuredSize.Y;
			InventoryTooltip.ScaledPositionYOffset = Mathf.Clamp(_currentHoveredItemWidget.GlobalPosition.Y - base.EventManager.TopUsableAreaStart, 0f, max);
			_lastDisplayedTooltipItem = _currentHoveredItemWidget;
		}
		else
		{
			_lastDisplayedTooltipItem = null;
		}
	}

	public void SetCurrentTuple(InventoryItemButtonWidget itemWidget, bool isLeftSide)
	{
		_focusLostThisFrame = itemWidget == null;
		if (_currentSelectedItemWidget != null && _currentSelectedItemWidget != itemWidget)
		{
			_currentSelectedItemWidget.IsSelected = false;
			if (_currentSelectedItemWidget is InventoryItemTupleWidget inventoryItemTupleWidget)
			{
				inventoryItemTupleWidget.TransferRequestHandlers.Remove(OnTransferItemRequested);
			}
			if (_currentSelectedOtherItemWidget != null)
			{
				_currentSelectedOtherItemWidget.IsSelected = false;
			}
		}
		if (itemWidget == null || (itemWidget is InventoryItemTupleWidget inventoryItemTupleWidget2 && _currentSelectedOtherItemWidget is InventoryItemTupleWidget inventoryItemTupleWidget3 && inventoryItemTupleWidget2.ItemID == inventoryItemTupleWidget3.ItemID))
		{
			_equippedItemControls.HidePanel();
			if (_currentSelectedItemWidget != null)
			{
				_currentSelectedItemWidget.IsSelected = false;
			}
			_currentSelectedItemWidget = null;
			if (_currentSelectedOtherItemWidget != null)
			{
				_currentSelectedOtherItemWidget.IsSelected = false;
				(_currentSelectedOtherItemWidget as InventoryItemTupleWidget).TransferRequestHandlers.Remove(OnTransferItemRequested);
			}
			_currentSelectedOtherItemWidget = null;
			return;
		}
		if (_currentSelectedItemWidget == itemWidget)
		{
			SetCurrentTuple(null, isLeftSide: false);
			if (_currentSelectedOtherItemWidget != null)
			{
				_currentSelectedOtherItemWidget.IsSelected = false;
			}
			_currentSelectedOtherItemWidget = null;
			return;
		}
		_currentSelectedItemWidget = itemWidget;
		TargetEquipmentIndex = -1;
		TransactionCount = 1;
		if (_currentSelectedItemWidget is InventoryEquippedItemSlotWidget)
		{
			_equippedItemControls.ShowPanel(itemWidget);
			_currentSelectedOtherItemWidget = null;
		}
		else
		{
			_equippedItemControls.HidePanel();
			if (_currentSelectedItemWidget is InventoryItemTupleWidget inventoryItemTupleWidget4)
			{
				inventoryItemTupleWidget4.TransferRequestHandlers.Add(OnTransferItemRequested);
				if (isLeftSide)
				{
					foreach (Widget allChild in PlayerInventoryListWidget.AllChildren)
					{
						if (allChild is InventoryItemTupleWidget inventoryItemTupleWidget5 && inventoryItemTupleWidget5.ItemID == inventoryItemTupleWidget4.ItemID)
						{
							_currentSelectedOtherItemWidget = inventoryItemTupleWidget5;
							_currentSelectedOtherItemWidget.IsSelected = true;
							(_currentSelectedOtherItemWidget as InventoryItemTupleWidget).TransferRequestHandlers.Add(OnTransferItemRequested);
							break;
						}
					}
				}
				else
				{
					foreach (Widget allChild2 in OtherInventoryListWidget.AllChildren)
					{
						if (allChild2 is InventoryItemTupleWidget inventoryItemTupleWidget6 && inventoryItemTupleWidget6.ItemID == inventoryItemTupleWidget4.ItemID)
						{
							_currentSelectedOtherItemWidget = inventoryItemTupleWidget6;
							_currentSelectedOtherItemWidget.IsSelected = true;
							(_currentSelectedOtherItemWidget as InventoryItemTupleWidget).TransferRequestHandlers.Add(OnTransferItemRequested);
							break;
						}
					}
				}
			}
		}
		UpdateScrollTarget(isLeftSide);
	}

	private void OnEquipmentControlsHidden()
	{
		_currentSelectedItemWidget = null;
		if (_currentSelectedOtherItemWidget != null)
		{
			_currentSelectedOtherItemWidget.IsSelected = false;
			(_currentSelectedOtherItemWidget as InventoryItemTupleWidget).TransferRequestHandlers.Remove(OnTransferItemRequested);
		}
		_currentSelectedOtherItemWidget = null;
	}

	private void OnTransferItemRequested(InventoryItemTupleWidget owner)
	{
		UpdateScrollTarget(!owner.IsRightSide);
	}

	private void TradeLabelOnPropertyChanged(PropertyOwnerObject owner, string propertyName, object value)
	{
		if (propertyName == "Text")
		{
			TradeLabel.IsDisabled = string.IsNullOrEmpty(TradeLabel.Text);
		}
	}

	private void EquippedItemControlsOnPreviewClick(Widget itemwidget)
	{
		ItemPreviewWidget.SetLastFocusedItem(null);
	}

	private void ItemWidgetHoverBegin(InventoryItemButtonWidget itemWidget)
	{
		if (_currentHoveredItemWidget != itemWidget)
		{
			_currentHoveredItemWidget = itemWidget;
			_tooltipHiddenFrameCount = 0;
			Widget widget = InventoryTooltip.FindChild("TargetItemTooltip");
			if (_currentHoveredItemWidget.IsRightSide)
			{
				widget.SetSiblingIndex(1);
			}
			else
			{
				widget.SetSiblingIndex(0);
			}
			InventoryTooltip.IsHidden = false;
			EventFired("ItemHoverBegin", itemWidget);
		}
	}

	private void ItemWidgetHoverEnd(InventoryItemButtonWidget itemWidget)
	{
		if (_currentHoveredItemWidget != null && itemWidget == null)
		{
			_currentHoveredItemWidget = null;
			InventoryTooltip.IsHidden = true;
			EventFired("ItemHoverEnd");
		}
	}

	public void ItemWidgetDragBegin(InventoryItemButtonWidget itemWidget)
	{
		EquippedItemControls?.HidePanel();
		_currentDraggedItemWidget = itemWidget;
		if (itemWidget is InventoryEquippedItemSlotWidget inventoryEquippedItemSlotWidget)
		{
			TargetEquipmentIndex = inventoryEquippedItemSlotWidget.TargetEquipmentIndex;
		}
		else
		{
			TargetEquipmentIndex = itemWidget.EquipmentIndex;
		}
	}

	public void ItemWidgetDrop(InventoryItemButtonWidget itemWidget)
	{
		if (_currentDraggedItemWidget == itemWidget)
		{
			_currentDraggedItemWidget = null;
			TargetEquipmentIndex = -1;
		}
	}

	private void OtherInventoryGoldTextOnPropertyChanged(PropertyOwnerObject owner, string propertyName, int value)
	{
		if (propertyName == "IntText")
		{
			bool isVisible = OtherInventoryGoldText.IntText > 0;
			OtherInventoryGoldText.IsVisible = isVisible;
			OtherInventoryGoldImage.IsVisible = isVisible;
		}
	}

	private void UpdateScrollTarget(bool isLeftSide)
	{
		if (_currentSelectedOtherItemWidget != null)
		{
			if (isLeftSide)
			{
				PlayerInventoryListWidget.ScrollToChild(_currentSelectedOtherItemWidget, -1f, 1f, 0, 400, 0.35f);
			}
			else
			{
				OtherInventoryListWidget.ScrollToChild(_currentSelectedOtherItemWidget, -1f, 1f, 0, 400, 0.35f);
			}
		}
	}

	private void OnNewInventoryItemAdded(Widget parentWidget, Widget addedWidget)
	{
		if (_currentSelectedItemWidget != null && addedWidget is InventoryItemTupleWidget newAddedItem)
		{
			_newAddedItem = newAddedItem;
		}
	}
}
