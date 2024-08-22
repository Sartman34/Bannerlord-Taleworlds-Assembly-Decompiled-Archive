using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Party;

public class PartyManageTroopPopupWidget : Widget
{
	private bool _isPrimaryActionAvailable;

	private bool _isSecondaryActionAvailable;

	public Widget PrimaryInputKeyVisualParent { get; set; }

	public Widget SecondaryInputKeyVisualParent { get; set; }

	public bool IsPrimaryActionAvailable
	{
		get
		{
			return _isPrimaryActionAvailable;
		}
		set
		{
			if (value != _isPrimaryActionAvailable)
			{
				_isPrimaryActionAvailable = value;
				OnPropertyChanged(value, "IsPrimaryActionAvailable");
			}
		}
	}

	public bool IsSecondaryActionAvailable
	{
		get
		{
			return _isSecondaryActionAvailable;
		}
		set
		{
			if (value != _isSecondaryActionAvailable)
			{
				_isSecondaryActionAvailable = value;
				OnPropertyChanged(value, "IsSecondaryActionAvailable");
			}
		}
	}

	public PartyManageTroopPopupWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (!base.IsVisible)
		{
			return;
		}
		Widget hoveredView = base.EventManager.HoveredView;
		if (hoveredView == null || PrimaryInputKeyVisualParent == null || SecondaryInputKeyVisualParent == null)
		{
			return;
		}
		PartyTroopManagementItemButtonWidget firstParentTupleOfWidget = GetFirstParentTupleOfWidget(hoveredView);
		if (firstParentTupleOfWidget != null)
		{
			Widget actionButtonAtIndex = firstParentTupleOfWidget.GetActionButtonAtIndex(0);
			bool flag = false;
			if (IsPrimaryActionAvailable && actionButtonAtIndex != null)
			{
				PrimaryInputKeyVisualParent.IsVisible = true;
				PrimaryInputKeyVisualParent.ScaledPositionXOffset = actionButtonAtIndex.GlobalPosition.X - 10f;
				PrimaryInputKeyVisualParent.ScaledPositionYOffset = actionButtonAtIndex.GlobalPosition.Y - 10f;
				flag = true;
			}
			else
			{
				PrimaryInputKeyVisualParent.IsVisible = false;
			}
			Widget widget = (flag ? firstParentTupleOfWidget.GetActionButtonAtIndex(1) : actionButtonAtIndex);
			if (IsSecondaryActionAvailable && widget != null)
			{
				SecondaryInputKeyVisualParent.IsVisible = true;
				SecondaryInputKeyVisualParent.ScaledPositionXOffset = widget.GlobalPosition.X + widget.Size.X + 4f;
				SecondaryInputKeyVisualParent.ScaledPositionYOffset = widget.GlobalPosition.Y - 10f;
			}
			else
			{
				SecondaryInputKeyVisualParent.IsVisible = false;
			}
		}
		else
		{
			PrimaryInputKeyVisualParent.IsVisible = false;
			SecondaryInputKeyVisualParent.IsVisible = false;
		}
	}

	private PartyTroopManagementItemButtonWidget GetFirstParentTupleOfWidget(Widget widget)
	{
		for (Widget widget2 = widget; widget2 != null; widget2 = widget2.ParentWidget)
		{
			if (widget2 is PartyTroopManagementItemButtonWidget result)
			{
				return result;
			}
		}
		return null;
	}
}
