using System;
using System.Collections.Generic;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

public class InventoryItemTupleWidget : InventoryItemButtonWidget
{
	private readonly Action<Widget> _viewClickHandler;

	private readonly Action<Widget> _equipClickHandler;

	private readonly Action<Widget> _transferClickHandler;

	private readonly Action<Widget> _sliderTransferClickHandler;

	public List<Action<InventoryItemTupleWidget>> TransferRequestHandlers = new List<Action<InventoryItemTupleWidget>>();

	private bool _isCivilianStateSet;

	private float _extendedUpdateTimer;

	private TextWidget _nameTextWidget;

	private TextWidget _countTextWidget;

	private TextWidget _costTextWidget;

	private int _profitState;

	private BrushListPanel _mainContainer;

	private InventoryTupleExtensionControlsWidget _extendedControlsContainer;

	private InventoryTwoWaySliderWidget _slider;

	private Widget _sliderParent;

	private TextWidget _sliderTextWidget;

	private bool _isTransferable;

	private ButtonWidget _equipButton;

	private ButtonWidget _viewButton;

	private InventoryTransferButtonWidget _transferButton;

	private ButtonWidget _sliderTransferButton;

	private int _transactionCount;

	private int _itemCount;

	private bool _isCivilian;

	private bool _isGenderDifferent;

	private bool _isEquipable;

	private bool _canCharacterUseItem;

	private bool _isNewlyAdded;

	private Brush _defaultBrush;

	private Brush _civilianDisabledBrush;

	private Brush _characterCantUseBrush;

	private string _itemID;

	public InventoryImageIdentifierWidget ItemImageIdentifier { get; set; }

	[Editor(false)]
	public string ItemID
	{
		get
		{
			return _itemID;
		}
		set
		{
			if (_itemID != value)
			{
				_itemID = value;
				OnPropertyChanged(value, "ItemID");
			}
		}
	}

	[Editor(false)]
	public TextWidget NameTextWidget
	{
		get
		{
			return _nameTextWidget;
		}
		set
		{
			if (_nameTextWidget != value)
			{
				_nameTextWidget = value;
				OnPropertyChanged(value, "NameTextWidget");
				NameTextWidget.AddState("Pressed");
			}
		}
	}

	[Editor(false)]
	public TextWidget CountTextWidget
	{
		get
		{
			return _countTextWidget;
		}
		set
		{
			if (_countTextWidget != value)
			{
				if (_countTextWidget != null)
				{
					_countTextWidget.intPropertyChanged -= CountTextWidgetOnPropertyChanged;
				}
				_countTextWidget = value;
				if (_countTextWidget != null)
				{
					_countTextWidget.intPropertyChanged += CountTextWidgetOnPropertyChanged;
				}
				OnPropertyChanged(value, "CountTextWidget");
				UpdateCountText();
			}
		}
	}

	[Editor(false)]
	public TextWidget CostTextWidget
	{
		get
		{
			return _costTextWidget;
		}
		set
		{
			if (_costTextWidget != value)
			{
				_costTextWidget = value;
				UpdateCostText();
				OnPropertyChanged(value, "CostTextWidget");
			}
		}
	}

	public int ProfitState
	{
		get
		{
			return _profitState;
		}
		set
		{
			if (value != _profitState)
			{
				_profitState = value;
				UpdateCostText();
				OnPropertyChanged(value, "ProfitState");
			}
		}
	}

	[Editor(false)]
	public BrushListPanel MainContainer
	{
		get
		{
			return _mainContainer;
		}
		set
		{
			if (_mainContainer != value)
			{
				_mainContainer = value;
				OnPropertyChanged(value, "MainContainer");
			}
		}
	}

	[Editor(false)]
	public InventoryTupleExtensionControlsWidget ExtendedControlsContainer
	{
		get
		{
			return _extendedControlsContainer;
		}
		set
		{
			if (_extendedControlsContainer != value)
			{
				_extendedControlsContainer = value;
				OnPropertyChanged(value, "ExtendedControlsContainer");
			}
		}
	}

	[Editor(false)]
	public InventoryTwoWaySliderWidget Slider
	{
		get
		{
			return _slider;
		}
		set
		{
			if (_slider != value)
			{
				if (_slider != null)
				{
					_slider.intPropertyChanged -= SliderIntPropertyChanged;
					_slider.PropertyChanged -= SliderValuePropertyChanged;
				}
				_slider = value;
				if (_slider != null)
				{
					_slider.intPropertyChanged += SliderIntPropertyChanged;
					_slider.PropertyChanged += SliderValuePropertyChanged;
				}
				OnPropertyChanged(value, "Slider");
				Slider.AddState("Selected");
				Slider.OverrideDefaultStateSwitchingEnabled = true;
			}
		}
	}

	[Editor(false)]
	public Widget SliderParent
	{
		get
		{
			return _sliderParent;
		}
		set
		{
			if (_sliderParent != value)
			{
				_sliderParent = value;
				OnPropertyChanged(value, "SliderParent");
				SliderParent.AddState("Selected");
			}
		}
	}

	[Editor(false)]
	public TextWidget SliderTextWidget
	{
		get
		{
			return _sliderTextWidget;
		}
		set
		{
			if (_sliderTextWidget != value)
			{
				_sliderTextWidget = value;
				OnPropertyChanged(value, "SliderTextWidget");
				SliderTextWidget.AddState("Selected");
			}
		}
	}

	[Editor(false)]
	public bool IsTransferable
	{
		get
		{
			return _isTransferable;
		}
		set
		{
			if (_isTransferable != value)
			{
				_isTransferable = value;
				OnPropertyChanged(value, "IsTransferable");
				UpdateDragAvailability();
			}
		}
	}

	[Editor(false)]
	public ButtonWidget EquipButton
	{
		get
		{
			return _equipButton;
		}
		set
		{
			if (_equipButton != value)
			{
				_equipButton?.ClickEventHandlers.Remove(_equipClickHandler);
				_equipButton = value;
				_equipButton?.ClickEventHandlers.Add(_equipClickHandler);
				OnPropertyChanged(value, "EquipButton");
			}
		}
	}

	[Editor(false)]
	public ButtonWidget ViewButton
	{
		get
		{
			return _viewButton;
		}
		set
		{
			if (_viewButton != value)
			{
				_viewButton?.ClickEventHandlers.Remove(_viewClickHandler);
				_viewButton = value;
				_viewButton?.ClickEventHandlers.Add(_viewClickHandler);
				OnPropertyChanged(value, "ViewButton");
			}
		}
	}

	[Editor(false)]
	public InventoryTransferButtonWidget TransferButton
	{
		get
		{
			return _transferButton;
		}
		set
		{
			if (_transferButton != value)
			{
				_transferButton?.ClickEventHandlers.Remove(_transferClickHandler);
				_transferButton = value;
				_transferButton?.ClickEventHandlers.Add(_transferClickHandler);
				OnPropertyChanged(value, "TransferButton");
			}
		}
	}

	[Editor(false)]
	public ButtonWidget SliderTransferButton
	{
		get
		{
			return _sliderTransferButton;
		}
		set
		{
			if (_sliderTransferButton != value)
			{
				_sliderTransferButton?.ClickEventHandlers.Remove(_sliderTransferClickHandler);
				_sliderTransferButton = value;
				_sliderTransferButton?.ClickEventHandlers.Add(_sliderTransferClickHandler);
				OnPropertyChanged(value, "SliderTransferButton");
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
	public int ItemCount
	{
		get
		{
			return _itemCount;
		}
		set
		{
			if (_itemCount != value)
			{
				_itemCount = value;
				OnPropertyChanged(value, "ItemCount");
				UpdateDragAvailability();
			}
		}
	}

	[Editor(false)]
	public bool IsCivilian
	{
		get
		{
			return _isCivilian;
		}
		set
		{
			if (_isCivilian != value || !_isCivilianStateSet)
			{
				_isCivilian = value;
				OnPropertyChanged(value, "IsCivilian");
				_isCivilianStateSet = true;
				UpdateCivilianState();
			}
		}
	}

	[Editor(false)]
	public bool IsGenderDifferent
	{
		get
		{
			return _isGenderDifferent;
		}
		set
		{
			if (_isGenderDifferent != value)
			{
				_isGenderDifferent = value;
				OnPropertyChanged(value, "IsGenderDifferent");
				UpdateCivilianState();
			}
		}
	}

	[Editor(false)]
	public bool IsEquipable
	{
		get
		{
			return _isEquipable;
		}
		set
		{
			if (_isEquipable != value)
			{
				_isEquipable = value;
				OnPropertyChanged(value, "IsEquipable");
				UpdateDragAvailability();
			}
		}
	}

	[Editor(false)]
	public bool IsNewlyAdded
	{
		get
		{
			return _isNewlyAdded;
		}
		set
		{
			if (_isNewlyAdded != value)
			{
				_isNewlyAdded = value;
				OnPropertyChanged(value, "IsNewlyAdded");
				ItemImageIdentifier.SetRenderRequestedPreviousFrame(value);
			}
		}
	}

	[Editor(false)]
	public bool CanCharacterUseItem
	{
		get
		{
			return _canCharacterUseItem;
		}
		set
		{
			if (_canCharacterUseItem != value)
			{
				_canCharacterUseItem = value;
				OnPropertyChanged(value, "CanCharacterUseItem");
				UpdateCivilianState();
			}
		}
	}

	[Editor(false)]
	public Brush DefaultBrush
	{
		get
		{
			return _defaultBrush;
		}
		set
		{
			if (_defaultBrush != value)
			{
				_defaultBrush = value;
				OnPropertyChanged(value, "DefaultBrush");
			}
		}
	}

	[Editor(false)]
	public Brush CivilianDisabledBrush
	{
		get
		{
			return _civilianDisabledBrush;
		}
		set
		{
			if (_civilianDisabledBrush != value)
			{
				_civilianDisabledBrush = value;
				OnPropertyChanged(value, "CivilianDisabledBrush");
			}
		}
	}

	[Editor(false)]
	public Brush CharacterCantUseBrush
	{
		get
		{
			return _characterCantUseBrush;
		}
		set
		{
			if (_characterCantUseBrush != value)
			{
				_characterCantUseBrush = value;
				OnPropertyChanged(value, "CharacterCantUseBrush");
			}
		}
	}

	public InventoryItemTupleWidget(UIContext context)
		: base(context)
	{
		_viewClickHandler = OnViewClick;
		_equipClickHandler = OnEquipClick;
		_transferClickHandler = delegate(Widget widget)
		{
			OnTransferClick(widget, 1);
		};
		_sliderTransferClickHandler = delegate
		{
			OnSliderTransferClick(TransactionCount);
		};
		base.OverrideDefaultStateSwitchingEnabled = false;
		AddState("Selected");
	}

	private void SetWidgetsState(string state)
	{
		SetState(state);
		string currentState = ExtendedControlsContainer.CurrentState;
		ExtendedControlsContainer.SetState(base.IsSelected ? "Selected" : "Default");
		MainContainer.SetState(state);
		NameTextWidget.SetState((state == "Pressed") ? state : "Default");
		if (currentState == "Default" && base.IsSelected)
		{
			EventFired("Opened");
			Slider.IsExtended = true;
		}
		else if (currentState == "Selected" && !base.IsSelected)
		{
			EventFired("Closed");
			Slider.IsExtended = false;
		}
	}

	private void OnExtendedHiddenUpdate(float dt)
	{
		if (!base.IsSelected)
		{
			_extendedUpdateTimer += dt;
			if (_extendedUpdateTimer > 2f)
			{
				ExtendedControlsContainer.IsVisible = false;
			}
			else
			{
				base.EventManager.AddLateUpdateAction(this, OnExtendedHiddenUpdate, 1);
			}
		}
	}

	protected override void RefreshState()
	{
		base.RefreshState();
		_ = ExtendedControlsContainer.IsVisible;
		ExtendedControlsContainer.IsExtended = base.IsSelected;
		if (base.IsSelected)
		{
			ExtendedControlsContainer.IsVisible = true;
		}
		else if (ExtendedControlsContainer.IsVisible)
		{
			_extendedUpdateTimer = 0f;
			base.EventManager.AddLateUpdateAction(this, OnExtendedHiddenUpdate, 1);
		}
		if (base.IsDisabled)
		{
			SetWidgetsState("Disabled");
		}
		else if (base.IsPressed)
		{
			SetWidgetsState("Pressed");
		}
		else if (base.IsHovered)
		{
			SetWidgetsState("Hovered");
		}
		else if (base.IsSelected)
		{
			SetWidgetsState("Selected");
		}
		else
		{
			SetWidgetsState("Default");
		}
	}

	private void UpdateCivilianState()
	{
		if (base.ScreenWidget == null)
		{
			return;
		}
		bool flag = !base.ScreenWidget.IsInWarSet && !IsCivilian;
		if (!CanCharacterUseItem)
		{
			if (!MainContainer.Brush.IsCloneRelated(CharacterCantUseBrush))
			{
				MainContainer.Brush = CharacterCantUseBrush;
				EquipButton.IsVisible = true;
				EquipButton.IsEnabled = false;
			}
		}
		else if (flag)
		{
			if (!MainContainer.Brush.IsCloneRelated(CivilianDisabledBrush))
			{
				MainContainer.Brush = CivilianDisabledBrush;
				EquipButton.IsVisible = true;
				EquipButton.IsEnabled = false;
			}
		}
		else if (!MainContainer.Brush.IsCloneRelated(DefaultBrush))
		{
			MainContainer.Brush = DefaultBrush;
			EquipButton.IsVisible = IsEquipable;
			EquipButton.IsEnabled = IsEquipable;
		}
	}

	private void OnViewClick(Widget widget)
	{
		if (base.ScreenWidget != null)
		{
			base.ScreenWidget.ItemPreviewWidget.SetLastFocusedItem(this);
		}
	}

	private void OnEquipClick(Widget widget)
	{
		EquipItem();
	}

	private void OnTransferClick(Widget widget, int count)
	{
		foreach (Action<InventoryItemTupleWidget> transferRequestHandler in TransferRequestHandlers)
		{
			transferRequestHandler(this);
		}
		if (base.IsRightSide)
		{
			ProcessBuyItem(playSound: true, count);
		}
		else
		{
			ProcessSellItem(playSound: true, count);
		}
	}

	private void OnSliderTransferClick(int count)
	{
	}

	public void ProcessBuyItem(bool playSound, int count = -1)
	{
		if (count == -1)
		{
			count = TransactionCount;
		}
		TransactionCount = count;
		base.ScreenWidget.TransactionCount = count;
		base.ScreenWidget.TargetEquipmentIndex = -1;
		TransferButton.FireClickEvent();
	}

	public void ProcessSellItem(bool playSound, int count = -1)
	{
		if (count == -1)
		{
			count = TransactionCount;
		}
		TransactionCount = count;
		base.ScreenWidget.TransactionCount = count;
		base.ScreenWidget.TargetEquipmentIndex = -1;
		TransferButton.FireClickEvent();
	}

	private void ProcessSelectItem()
	{
		if (base.ScreenWidget != null)
		{
			base.IsSelected = true;
			base.ScreenWidget.SetCurrentTuple(this, !base.IsRightSide);
		}
	}

	protected override void OnMouseReleased()
	{
		base.OnMouseReleased();
		ProcessSelectItem();
	}

	protected override void OnMouseAlternateReleased()
	{
		base.OnMouseAlternateReleased();
		EventFired("OnAlternateRelease");
	}

	protected override void OnConnectedToRoot()
	{
		base.OnConnectedToRoot();
		base.ScreenWidget.boolPropertyChanged += InventoryScreenWidgetOnPropertyChanged;
	}

	protected override void OnDisconnectedFromRoot()
	{
		if (base.ScreenWidget != null)
		{
			base.ScreenWidget.boolPropertyChanged -= InventoryScreenWidgetOnPropertyChanged;
			base.ScreenWidget.SetCurrentTuple(null, isLeftSide: false);
		}
	}

	private void SliderIntPropertyChanged(PropertyOwnerObject owner, string propertyName, int value)
	{
		if (propertyName == "ValueInt")
		{
			TransactionCount = _slider.ValueInt;
		}
	}

	private void SliderValuePropertyChanged(PropertyOwnerObject owner, string propertyName, object value)
	{
		if (!(propertyName == "OnMousePressed"))
		{
			return;
		}
		foreach (Action<InventoryItemTupleWidget> transferRequestHandler in TransferRequestHandlers)
		{
			transferRequestHandler(this);
		}
	}

	private void CountTextWidgetOnPropertyChanged(PropertyOwnerObject owner, string propertyName, int value)
	{
		if (propertyName == "IntText")
		{
			UpdateCountText();
		}
	}

	private void InventoryScreenWidgetOnPropertyChanged(PropertyOwnerObject owner, string propertyName, bool value)
	{
		if (propertyName == "IsInWarSet")
		{
			UpdateCivilianState();
		}
	}

	private void UpdateCountText()
	{
		if (SliderTextWidget != null)
		{
			SliderTextWidget.IsHidden = CountTextWidget.IsHidden;
		}
	}

	private void UpdateCostText()
	{
		if (CostTextWidget != null)
		{
			switch (ProfitState)
			{
			case -2:
				CostTextWidget.SetState("VeryBad");
				break;
			case -1:
				CostTextWidget.SetState("Bad");
				break;
			case 0:
				CostTextWidget.SetState("Default");
				break;
			case 1:
				CostTextWidget.SetState("Good");
				break;
			case 2:
				CostTextWidget.SetState("VeryGood");
				break;
			}
		}
	}

	private void UpdateDragAvailability()
	{
		base.AcceptDrag = ItemCount > 0 && (IsTransferable || IsEquipable);
	}
}
