using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.InputSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Order;

public class OrderReturnButtonWidget : OrderItemButtonWidget
{
	private bool _isHolding;

	private bool _canUseShortcuts;

	private bool _isAnyOrderSetActive;

	private bool _isDeployment;

	public Widget InputVisualParent { get; set; }

	public bool IsHolding
	{
		get
		{
			return _isHolding;
		}
		set
		{
			if (value != _isHolding)
			{
				_isHolding = value;
				OnPropertyChanged(value, "IsHolding");
				UpdateVisibility();
			}
		}
	}

	public bool CanUseShortcuts
	{
		get
		{
			return _canUseShortcuts;
		}
		set
		{
			if (value != _canUseShortcuts)
			{
				_canUseShortcuts = value;
				OnPropertyChanged(value, "CanUseShortcuts");
				UpdateInputVisualVisibility();
			}
		}
	}

	public bool IsAnyOrderSetActive
	{
		get
		{
			return _isAnyOrderSetActive;
		}
		set
		{
			if (value != _isAnyOrderSetActive)
			{
				_isAnyOrderSetActive = value;
				OnPropertyChanged(value, "IsAnyOrderSetActive");
				UpdateInputVisualVisibility();
			}
		}
	}

	public bool IsDeployment
	{
		get
		{
			return _isDeployment;
		}
		set
		{
			if (value != _isDeployment)
			{
				_isDeployment = value;
				OnPropertyChanged(value, "IsDeployment");
				UpdateVisibility();
			}
		}
	}

	public OrderReturnButtonWidget(UIContext context)
		: base(context)
	{
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Combine(Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
	}

	private void OnGamepadActiveStateChanged()
	{
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		base.IsVisible = IsDeployment && Input.IsGamepadActive && !IsHolding;
	}

	private void UpdateInputVisualVisibility()
	{
		if (InputVisualParent != null)
		{
			InputVisualParent.IsVisible = CanUseShortcuts && !IsAnyOrderSetActive;
		}
	}

	protected override void OnDisconnectedFromRoot()
	{
		base.OnDisconnectedFromRoot();
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Remove(Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
	}
}
