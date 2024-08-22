using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public class DimensionSyncWidget : Widget
{
	public enum Dimensions
	{
		None,
		Horizontal,
		Vertical,
		HorizontalAndVertical
	}

	private Widget _widgetToCopyHeightFrom;

	private Dimensions _dimensionToSync;

	private int _paddingAmount;

	public Widget WidgetToCopyHeightFrom
	{
		get
		{
			return _widgetToCopyHeightFrom;
		}
		set
		{
			if (_widgetToCopyHeightFrom != value)
			{
				_widgetToCopyHeightFrom = value;
				OnPropertyChanged(value, "WidgetToCopyHeightFrom");
			}
		}
	}

	public int PaddingAmount
	{
		get
		{
			return _paddingAmount;
		}
		set
		{
			if (_paddingAmount != value)
			{
				_paddingAmount = value;
			}
		}
	}

	public Dimensions DimensionToSync
	{
		get
		{
			return _dimensionToSync;
		}
		set
		{
			if (_dimensionToSync != value)
			{
				_dimensionToSync = value;
			}
		}
	}

	public DimensionSyncWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (DimensionToSync != 0 && WidgetToCopyHeightFrom != null)
		{
			if (DimensionToSync == Dimensions.Horizontal || DimensionToSync == Dimensions.HorizontalAndVertical)
			{
				base.ScaledSuggestedWidth = WidgetToCopyHeightFrom.Size.X + (float)PaddingAmount * base._scaleToUse;
			}
			if (DimensionToSync == Dimensions.Vertical || DimensionToSync == Dimensions.HorizontalAndVertical)
			{
				base.ScaledSuggestedHeight = WidgetToCopyHeightFrom.Size.Y + (float)PaddingAmount * base._scaleToUse;
			}
		}
	}
}
