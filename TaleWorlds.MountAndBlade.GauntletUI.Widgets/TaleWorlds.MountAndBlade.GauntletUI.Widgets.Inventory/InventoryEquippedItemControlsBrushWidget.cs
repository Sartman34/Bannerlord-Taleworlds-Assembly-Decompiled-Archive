using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

public class InventoryEquippedItemControlsBrushWidget : BrushWidget
{
	public delegate void ButtonClickEventHandler(Widget itemWidget);

	private InventoryItemButtonWidget _itemWidget;

	private Action<Widget> _previewClickHandler;

	private Action<Widget> _unequipClickHandler;

	private Action<Widget> _sellClickHandler;

	private float _lastTransitionStartTime;

	private bool _isScopeDirty;

	private bool _isControlsEnabled;

	private InventoryScreenWidget _screenWidget;

	private ButtonWidget _previewButton;

	private ButtonWidget _unequipButton;

	private ButtonWidget _sellButton;

	public NavigationForcedScopeCollectionTargeter ForcedScopeCollection { get; set; }

	public NavigationScopeTargeter NavigationScope { get; set; }

	public bool IsControlsEnabled
	{
		get
		{
			return _isControlsEnabled;
		}
		set
		{
			if (value != _isControlsEnabled)
			{
				_isControlsEnabled = value;
				OnPropertyChanged(value, "IsControlsEnabled");
			}
		}
	}

	[Editor(false)]
	public InventoryScreenWidget ScreenWidget
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

	[Editor(false)]
	public ButtonWidget PreviewButton
	{
		get
		{
			return _previewButton;
		}
		set
		{
			if (_previewButton != value)
			{
				_previewButton?.ClickEventHandlers.Remove(_previewClickHandler);
				_previewButton = value;
				_previewButton?.ClickEventHandlers.Add(_previewClickHandler);
				OnPropertyChanged(value, "PreviewButton");
			}
		}
	}

	[Editor(false)]
	public ButtonWidget UnequipButton
	{
		get
		{
			return _unequipButton;
		}
		set
		{
			if (_unequipButton != value)
			{
				_unequipButton?.ClickEventHandlers.Remove(_unequipClickHandler);
				_unequipButton = value;
				_unequipButton?.ClickEventHandlers.Add(_unequipClickHandler);
				OnPropertyChanged(value, "UnequipButton");
			}
		}
	}

	[Editor(false)]
	public ButtonWidget SellButton
	{
		get
		{
			return _sellButton;
		}
		set
		{
			if (_sellButton != value)
			{
				_sellButton?.ClickEventHandlers.Remove(_sellClickHandler);
				_sellButton = value;
				_sellButton?.ClickEventHandlers.Add(_sellClickHandler);
				OnPropertyChanged(value, "SellButton");
			}
		}
	}

	public event ButtonClickEventHandler OnPreviewClick;

	public event ButtonClickEventHandler OnUnequipClick;

	public event ButtonClickEventHandler OnSellClick;

	public event Action OnHidePanel;

	public InventoryEquippedItemControlsBrushWidget(UIContext context)
		: base(context)
	{
		_previewClickHandler = PreviewClicked;
		_unequipClickHandler = UnequipClicked;
		_sellClickHandler = SellClicked;
		AddState("LeftHidden");
		AddState("LeftVisible");
		AddState("RightHidden");
		AddState("RightVisible");
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (ScreenWidget == null)
		{
			Widget widget = this;
			while (widget != base.EventManager.Root && ScreenWidget == null)
			{
				if (widget is InventoryScreenWidget)
				{
					ScreenWidget = (InventoryScreenWidget)widget;
				}
				else
				{
					widget = widget.ParentWidget;
				}
			}
		}
		if (_isScopeDirty && base.EventManager.Time - _lastTransitionStartTime > base.VisualDefinition.TransitionDuration)
		{
			ForcedScopeCollection.IsCollectionDisabled = base.CurrentState == "RightHidden" || base.CurrentState == "LeftHidden";
			NavigationScope.IsScopeDisabled = ForcedScopeCollection.IsCollectionDisabled;
			_isScopeDirty = false;
		}
		if (!IsControlsEnabled && _itemWidget != null)
		{
			HidePanel();
		}
	}

	public void ShowPanel(InventoryItemButtonWidget itemWidget)
	{
		if (itemWidget.IsRightSide)
		{
			base.HorizontalAlignment = HorizontalAlignment.Right;
			base.Brush.HorizontalFlip = false;
			SetState("RightHidden");
			base.PositionXOffset = base.VisualDefinition.VisualStates["RightHidden"].PositionXOffset;
			SetState("RightVisible");
		}
		else
		{
			base.HorizontalAlignment = HorizontalAlignment.Left;
			base.Brush.HorizontalFlip = true;
			SetState("LeftHidden");
			base.PositionXOffset = base.VisualDefinition.VisualStates["LeftHidden"].PositionXOffset;
			SetState("LeftVisible");
		}
		base.ScaledPositionYOffset = itemWidget.GlobalPosition.Y + itemWidget.Size.Y - 10f * base._scaleToUse - base.EventManager.TopUsableAreaStart;
		base.IsVisible = true;
		_itemWidget = itemWidget;
		_isScopeDirty = true;
		_lastTransitionStartTime = base.Context.EventManager.Time;
		IsControlsEnabled = true;
	}

	public void HidePanel()
	{
		if (base.IsVisible && _itemWidget != null)
		{
			if (_itemWidget.IsRightSide)
			{
				SetState("RightHidden");
			}
			else
			{
				SetState("LeftHidden");
			}
			_itemWidget = null;
			this.OnHidePanel?.Invoke();
			_isScopeDirty = true;
			_lastTransitionStartTime = base.Context.EventManager.Time;
			IsControlsEnabled = false;
		}
	}

	private void PreviewClicked(Widget widget)
	{
		if (_itemWidget != null)
		{
			this.OnPreviewClick?.Invoke(_itemWidget);
			_itemWidget.PreviewItem();
		}
	}

	private void UnequipClicked(Widget widget)
	{
		if (_itemWidget != null)
		{
			this.OnUnequipClick?.Invoke(_itemWidget);
			_itemWidget.UnequipItem();
			HidePanel();
		}
	}

	private void SellClicked(Widget widget)
	{
		if (_itemWidget != null)
		{
			this.OnSellClick?.Invoke(_itemWidget);
			ScreenWidget.TransactionCount = 1;
			_itemWidget.SellItem();
			HidePanel();
		}
	}

	protected override void OnUpdate(float dt)
	{
		base.OnUpdate(dt);
		_ = _itemWidget;
	}
}
