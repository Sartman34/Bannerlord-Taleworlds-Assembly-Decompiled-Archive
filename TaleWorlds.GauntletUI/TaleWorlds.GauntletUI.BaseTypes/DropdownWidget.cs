using System;
using System.Numerics;
using TaleWorlds.GauntletUI.GamepadNavigation;
using TaleWorlds.InputSystem;

namespace TaleWorlds.GauntletUI.BaseTypes;

public class DropdownWidget : Widget
{
	public Action<DropdownWidget> OnOpenStateChanged;

	private readonly Action<Widget> _clickHandler;

	private readonly Action<Widget> _listSelectionHandler;

	private readonly Action<Widget, Widget> _listItemRemovedHandler;

	private readonly Action<Widget, Widget> _listItemAddedHandler;

	private Vector2 _listPanelOpenPosition;

	private int _openFrameCounter;

	private bool _isSelectedItemDirty = true;

	private bool _changedByControllerNavigation;

	private GamepadNavigationScope _navigationScope;

	private GamepadNavigationForcedScopeCollection _scopeCollection;

	private ScrollablePanel _scrollablePanel;

	private ButtonWidget _button;

	private ListPanel _listPanel;

	private int _currentSelectedIndex;

	private bool _closeNextFrame;

	private bool _isOpen;

	private bool _buttonClicked;

	private Vector2 ListPanelPositionInsideUsableArea => ListPanel.GlobalPosition - new Vector2(base.EventManager.LeftUsableAreaStart, base.EventManager.TopUsableAreaStart);

	[Editor(false)]
	public RichTextWidget RichTextWidget { get; set; }

	[Editor(false)]
	public bool DoNotHandleDropdownListPanel { get; set; }

	[Editor(false)]
	public ScrollablePanel ScrollablePanel
	{
		get
		{
			return _scrollablePanel;
		}
		set
		{
			if (value != _scrollablePanel)
			{
				_scrollablePanel = value;
				OnPropertyChanged(value, "ScrollablePanel");
			}
		}
	}

	[Editor(false)]
	public ButtonWidget Button
	{
		get
		{
			return _button;
		}
		set
		{
			_button?.ClickEventHandlers.Remove(_clickHandler);
			_button = value;
			_button?.ClickEventHandlers.Add(_clickHandler);
			_isSelectedItemDirty = true;
		}
	}

	[Editor(false)]
	public ListPanel ListPanel
	{
		get
		{
			return _listPanel;
		}
		set
		{
			if (_listPanel != null)
			{
				_listPanel.SelectEventHandlers.Remove(_listSelectionHandler);
				_listPanel.ItemAddEventHandlers.Remove(_listItemAddedHandler);
				_listPanel.ItemRemoveEventHandlers.Remove(_listItemRemovedHandler);
			}
			_listPanel = value;
			if (_listPanel != null)
			{
				if (!DoNotHandleDropdownListPanel)
				{
					_listPanel.ParentWidget = base.EventManager.Root;
					_listPanel.HorizontalAlignment = HorizontalAlignment.Left;
					_listPanel.VerticalAlignment = VerticalAlignment.Top;
				}
				_listPanel.SelectEventHandlers.Add(_listSelectionHandler);
				_listPanel.ItemAddEventHandlers.Add(_listItemAddedHandler);
				_listPanel.ItemRemoveEventHandlers.Add(_listItemRemovedHandler);
			}
			_isSelectedItemDirty = true;
		}
	}

	public bool IsOpen
	{
		get
		{
			return _isOpen;
		}
		set
		{
			if (value != _isOpen && !_buttonClicked)
			{
				if (_isOpen)
				{
					ClosePanel();
				}
				else
				{
					OpenPanel();
				}
			}
		}
	}

	[Editor(false)]
	public int ListPanelValue
	{
		get
		{
			if (ListPanel != null)
			{
				return ListPanel.IntValue;
			}
			return -1;
		}
		set
		{
			if (ListPanel != null && ListPanel.IntValue != value)
			{
				ListPanel.IntValue = value;
			}
		}
	}

	[Editor(false)]
	public int CurrentSelectedIndex
	{
		get
		{
			return _currentSelectedIndex;
		}
		set
		{
			if (_currentSelectedIndex != value)
			{
				_currentSelectedIndex = value;
				_isSelectedItemDirty = true;
			}
		}
	}

	public DropdownWidget(UIContext context)
		: base(context)
	{
		_clickHandler = OnButtonClick;
		_listSelectionHandler = OnSelectionChanged;
		_listItemRemovedHandler = OnListItemRemoved;
		_listItemAddedHandler = OnListItemAdded;
		base.UsedNavigationMovements = GamepadNavigationTypes.Horizontal;
	}

	protected override void OnUpdate(float dt)
	{
		base.OnUpdate(dt);
		if (!DoNotHandleDropdownListPanel)
		{
			UpdateListPanelPosition();
		}
		if (_buttonClicked)
		{
			if (ListPanel != null && !_changedByControllerNavigation)
			{
				if (_isOpen)
				{
					ClosePanel();
				}
				else
				{
					OpenPanel();
				}
			}
			_buttonClicked = false;
		}
		else if (_closeNextFrame && _isOpen)
		{
			ClosePanel();
			_closeNextFrame = false;
		}
		else if (base.EventManager.LatestMouseUpWidget != _button && _isOpen)
		{
			if (ListPanel.IsVisible)
			{
				_closeNextFrame = true;
			}
		}
		else if (_isOpen)
		{
			_openFrameCounter++;
			if (_openFrameCounter > 5)
			{
				if (Vector2.Distance(ListPanelPositionInsideUsableArea, _listPanelOpenPosition) > 20f && !DoNotHandleDropdownListPanel)
				{
					_closeNextFrame = true;
				}
			}
			else
			{
				_listPanelOpenPosition = ListPanelPositionInsideUsableArea;
			}
		}
		RefreshSelectedItem();
	}

	protected override void OnConnectedToRoot()
	{
		base.OnConnectedToRoot();
		ScrollablePanel = GetParentScrollablePanelOfWidget(this);
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (!DoNotHandleDropdownListPanel)
		{
			UpdateListPanelPosition();
		}
		UpdateGamepadNavigationControls();
	}

	private void UpdateGamepadNavigationControls()
	{
		if (_isOpen && base.EventManager.IsControllerActive && (Input.IsKeyPressed(InputKey.ControllerLBumper) || Input.IsKeyPressed(InputKey.ControllerLTrigger) || Input.IsKeyPressed(InputKey.ControllerRBumper) || Input.IsKeyPressed(InputKey.ControllerRTrigger)))
		{
			ClosePanel();
		}
		if (!_isOpen && (base.IsPressed || _button.IsPressed) && IsRecursivelyVisible() && base.EventManager.GetIsHitThisFrame())
		{
			if (Input.IsKeyReleased(InputKey.ControllerLLeft))
			{
				if (CurrentSelectedIndex > 0)
				{
					CurrentSelectedIndex--;
				}
				else
				{
					CurrentSelectedIndex = ListPanel.ChildCount - 1;
				}
				_isSelectedItemDirty = true;
				_changedByControllerNavigation = true;
			}
			else if (Input.IsKeyReleased(InputKey.ControllerLRight))
			{
				if (CurrentSelectedIndex < ListPanel.ChildCount - 1)
				{
					CurrentSelectedIndex++;
				}
				else
				{
					CurrentSelectedIndex = 0;
				}
				_isSelectedItemDirty = true;
				_changedByControllerNavigation = true;
			}
			base.IsUsingNavigation = true;
		}
		else
		{
			_changedByControllerNavigation = false;
			base.IsUsingNavigation = false;
		}
	}

	private void UpdateListPanelPosition()
	{
		ListPanel.HorizontalAlignment = HorizontalAlignment.Left;
		ListPanel.VerticalAlignment = VerticalAlignment.Top;
		float num = (base.Size.X - _listPanel.Size.X) * 0.5f;
		ListPanel.MarginTop = (base.GlobalPosition.Y + Button.Size.Y - base.EventManager.TopUsableAreaStart) * base._inverseScaleToUse;
		ListPanel.MarginLeft = (base.GlobalPosition.X + num - base.EventManager.LeftUsableAreaStart) * base._inverseScaleToUse;
	}

	protected virtual void OpenPanel()
	{
		if (Button != null)
		{
			Button.IsSelected = true;
		}
		ListPanel.IsVisible = true;
		_listPanelOpenPosition = ListPanelPositionInsideUsableArea;
		_openFrameCounter = 0;
		_isOpen = true;
		OnOpenStateChanged?.Invoke(this);
		CreateGamepadNavigationScopeData();
	}

	protected virtual void ClosePanel()
	{
		if (Button != null)
		{
			Button.IsSelected = false;
		}
		ListPanel.IsVisible = false;
		_buttonClicked = false;
		_isOpen = false;
		OnOpenStateChanged?.Invoke(this);
		ClearGamepadScopeData();
	}

	private void CreateGamepadNavigationScopeData()
	{
		if (_navigationScope != null)
		{
			base.GamepadNavigationContext.RemoveNavigationScope(_navigationScope);
		}
		_scopeCollection = new GamepadNavigationForcedScopeCollection();
		_scopeCollection.ParentWidget = base.ParentWidget ?? this;
		_scopeCollection.CollectionOrder = 999;
		_navigationScope = BuildGamepadNavigationScopeData();
		base.GamepadNavigationContext.AddNavigationScope(_navigationScope, initialize: true);
		_button.GamepadNavigationIndex = 0;
		_navigationScope.AddWidgetAtIndex(_button, 0);
		ButtonWidget button = _button;
		button.OnGamepadNavigationFocusGained = (Action<Widget>)Delegate.Combine(button.OnGamepadNavigationFocusGained, new Action<Widget>(OnWidgetGainedNavigationFocus));
		for (int i = 0; i < ListPanel.Children.Count; i++)
		{
			ListPanel.Children[i].GamepadNavigationIndex = i + 1;
			_navigationScope.AddWidgetAtIndex(ListPanel.Children[i], i + 1);
			Widget widget = ListPanel.Children[i];
			widget.OnGamepadNavigationFocusGained = (Action<Widget>)Delegate.Combine(widget.OnGamepadNavigationFocusGained, new Action<Widget>(OnWidgetGainedNavigationFocus));
		}
		base.GamepadNavigationContext.AddForcedScopeCollection(_scopeCollection);
	}

	private void OnWidgetGainedNavigationFocus(Widget widget)
	{
		ScrollablePanel?.ScrollToChild(widget);
	}

	private ScrollablePanel GetParentScrollablePanelOfWidget(Widget widget)
	{
		for (Widget widget2 = widget; widget2 != null; widget2 = widget2.ParentWidget)
		{
			if (widget2 is ScrollablePanel result)
			{
				return result;
			}
		}
		return null;
	}

	private GamepadNavigationScope BuildGamepadNavigationScopeData()
	{
		return new GamepadNavigationScope
		{
			ScopeMovements = GamepadNavigationTypes.Vertical,
			DoNotAutomaticallyFindChildren = true,
			DoNotAutoNavigateAfterSort = true,
			HasCircularMovement = true,
			ParentWidget = (base.ParentWidget ?? this),
			ScopeID = "DropdownScope"
		};
	}

	private void ClearGamepadScopeData()
	{
		if (_navigationScope != null)
		{
			base.GamepadNavigationContext.RemoveNavigationScope(_navigationScope);
			for (int i = 0; i < ListPanel.Children.Count; i++)
			{
				ListPanel.Children[i].GamepadNavigationIndex = -1;
				Widget widget = ListPanel.Children[i];
				widget.OnGamepadNavigationFocusGained = (Action<Widget>)Delegate.Remove(widget.OnGamepadNavigationFocusGained, new Action<Widget>(OnWidgetGainedNavigationFocus));
			}
			_button.GamepadNavigationIndex = -1;
			ButtonWidget button = _button;
			button.OnGamepadNavigationFocusGained = (Action<Widget>)Delegate.Remove(button.OnGamepadNavigationFocusGained, new Action<Widget>(OnWidgetGainedNavigationFocus));
			_navigationScope = null;
		}
		if (_scopeCollection != null)
		{
			base.GamepadNavigationContext.RemoveForcedScopeCollection(_scopeCollection);
		}
	}

	public void OnButtonClick(Widget widget)
	{
		_buttonClicked = true;
		_closeNextFrame = false;
	}

	public void UpdateButtonText(string text)
	{
		if (RichTextWidget != null)
		{
			RichTextWidget.Text = ((!string.IsNullOrEmpty(text)) ? text : " ");
		}
	}

	public void OnListItemAdded(Widget parentWidget, Widget newChild)
	{
		_isSelectedItemDirty = true;
	}

	public void OnListItemRemoved(Widget removedItem, Widget removedChild)
	{
		_isSelectedItemDirty = true;
	}

	public void OnSelectionChanged(Widget widget)
	{
		CurrentSelectedIndex = ListPanelValue;
		_isSelectedItemDirty = true;
		OnPropertyChanged(CurrentSelectedIndex, "CurrentSelectedIndex");
	}

	private void RefreshSelectedItem()
	{
		if (!_isSelectedItemDirty)
		{
			return;
		}
		ListPanelValue = CurrentSelectedIndex;
		if (ListPanelValue >= 0)
		{
			string text = "";
			Widget widget = ListPanel?.GetChild(ListPanelValue);
			if (widget != null)
			{
				foreach (Widget allChild in widget.AllChildren)
				{
					if (allChild is RichTextWidget richTextWidget)
					{
						text = richTextWidget.Text;
					}
				}
			}
			UpdateButtonText(text);
		}
		if (ListPanel != null)
		{
			for (int i = 0; i < ListPanel.ChildCount; i++)
			{
				if (ListPanel.GetChild(i) is ButtonWidget buttonWidget)
				{
					buttonWidget.IsSelected = CurrentSelectedIndex == i;
				}
			}
		}
		_isSelectedItemDirty = false;
	}
}
