using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public class NavigatableGridWidget : GridWidget
{
	private bool _areIndicesDirty;

	private int _minIndex;

	private int _maxIndex = int.MaxValue;

	private int _stepSize = 1;

	private bool _useSelfIndexForMinimum;

	private Widget _emptyNavigationWidget;

	public ScrollablePanel ParentPanel { get; set; }

	public int AutoScrollXOffset { get; set; }

	public int AutoScrollYOffset { get; set; }

	public int MinIndex
	{
		get
		{
			return _minIndex;
		}
		set
		{
			if (value != _minIndex)
			{
				_minIndex = value;
				RefreshChildNavigationIndices();
			}
		}
	}

	public int MaxIndex
	{
		get
		{
			return _maxIndex;
		}
		set
		{
			if (value != _maxIndex)
			{
				_maxIndex = value;
				RefreshChildNavigationIndices();
			}
		}
	}

	public int StepSize
	{
		get
		{
			return _stepSize;
		}
		set
		{
			if (value != _stepSize)
			{
				_stepSize = value;
				RefreshChildNavigationIndices();
			}
		}
	}

	public bool UseSelfIndexForMinimum
	{
		get
		{
			return _useSelfIndexForMinimum;
		}
		set
		{
			if (value != _useSelfIndexForMinimum)
			{
				_useSelfIndexForMinimum = value;
				if (_useSelfIndexForMinimum && base.GamepadNavigationIndex != -1)
				{
					SetNavigationIndicesFromSelf();
				}
			}
		}
	}

	public Widget EmptyNavigationWidget
	{
		get
		{
			return _emptyNavigationWidget;
		}
		set
		{
			if (value != _emptyNavigationWidget)
			{
				if (_emptyNavigationWidget != null)
				{
					_emptyNavigationWidget.GamepadNavigationIndex = -1;
				}
				_emptyNavigationWidget = value;
				UpdateEmptyNavigationWidget();
			}
		}
	}

	public NavigatableGridWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		if (_areIndicesDirty)
		{
			for (int i = 0; i < base.ChildCount; i++)
			{
				base.Children[i].GamepadNavigationIndex = -1;
			}
			RefreshChildNavigationIndices();
			_areIndicesDirty = false;
		}
	}

	protected override void OnConnectedToRoot()
	{
		base.OnConnectedToRoot();
		if (ParentPanel == null)
		{
			ParentPanel = FindParentPanel();
		}
	}

	protected override void OnChildAdded(Widget child)
	{
		base.OnChildAdded(child);
		SetNavigationIndexForChild(child);
		child.OnGamepadNavigationFocusGained = OnWidgetGainedGamepadFocus;
		child.EventFire += OnChildSiblingIndexChanged;
		child.boolPropertyChanged += OnChildVisibilityChanged;
		UpdateEmptyNavigationWidget();
	}

	protected override void OnAfterChildRemoved(Widget child)
	{
		base.OnAfterChildRemoved(child);
		child.OnGamepadNavigationFocusGained = null;
		child.EventFire -= OnChildSiblingIndexChanged;
		child.boolPropertyChanged -= OnChildVisibilityChanged;
		child.GamepadNavigationIndex = -1;
		UpdateEmptyNavigationWidget();
	}

	protected override void OnDisconnectedFromRoot()
	{
		base.OnDisconnectedFromRoot();
		for (int i = 0; i < base.Children.Count; i++)
		{
			base.Children[i].OnGamepadNavigationFocusGained = null;
			base.Children[i].EventFire -= OnChildSiblingIndexChanged;
			base.Children[i].boolPropertyChanged -= OnChildVisibilityChanged;
		}
	}

	private void OnChildVisibilityChanged(PropertyOwnerObject child, string propertyName, bool value)
	{
		if (propertyName == "IsVisible")
		{
			Widget widget = (Widget)child;
			if (!value)
			{
				widget.GamepadNavigationIndex = -1;
			}
			else
			{
				SetNavigationIndexForChild(widget);
			}
		}
	}

	private void OnWidgetGainedGamepadFocus(Widget widget)
	{
		if (ParentPanel != null)
		{
			ParentPanel.ScrollToChild(widget, -1f, -1f, AutoScrollXOffset, AutoScrollYOffset);
		}
	}

	private void OnChildSiblingIndexChanged(Widget widget, string eventName, object[] parameters)
	{
		if (eventName == "SiblingIndexChanged")
		{
			_areIndicesDirty = true;
		}
	}

	private void SetNavigationIndexForChild(Widget widget)
	{
		int num = MinIndex + widget.GetSiblingIndex() * StepSize;
		if (num <= MaxIndex)
		{
			widget.GamepadNavigationIndex = num;
		}
	}

	protected override void OnGamepadNavigationIndexUpdated(int newIndex)
	{
		if (newIndex != -1 && UseSelfIndexForMinimum)
		{
			SetNavigationIndicesFromSelf();
		}
	}

	private void SetNavigationIndicesFromSelf()
	{
		MinIndex = base.GamepadNavigationIndex;
		base.GamepadNavigationIndex = -1;
		_areIndicesDirty = true;
	}

	private void UpdateEmptyNavigationWidget()
	{
		if (_emptyNavigationWidget != null)
		{
			if (base.Children.Count == 0)
			{
				EmptyNavigationWidget.GamepadNavigationIndex = MinIndex;
			}
			else
			{
				EmptyNavigationWidget.GamepadNavigationIndex = -1;
			}
		}
	}

	protected void RefreshChildNavigationIndices()
	{
		for (int i = 0; i < base.Children.Count; i++)
		{
			SetNavigationIndexForChild(base.Children[i]);
		}
	}
}
