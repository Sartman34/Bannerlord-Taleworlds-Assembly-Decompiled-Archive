using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.GauntletUI.ExtraWidgets;

public class ValueBasedVisibilityWidget : Widget
{
	public enum WatchTypes
	{
		Equal,
		BiggerThan,
		BiggerThanEqual,
		LessThan,
		LessThanEqual,
		NotEqual
	}

	private int _indexToBeVisible;

	private int _indexToWatch = -1;

	private float _indexToBeVisibleFloat;

	private float _indexToWatchFloat = -1f;

	public WatchTypes WatchType { get; set; }

	[Editor(false)]
	public int IndexToWatch
	{
		get
		{
			return _indexToWatch;
		}
		set
		{
			if (_indexToWatch != value)
			{
				_indexToWatch = value;
				OnPropertyChanged(value, "IndexToWatch");
				switch (WatchType)
				{
				case WatchTypes.Equal:
					base.IsVisible = value == IndexToBeVisible;
					break;
				case WatchTypes.BiggerThan:
					base.IsVisible = value > IndexToBeVisible;
					break;
				case WatchTypes.LessThan:
					base.IsVisible = value < IndexToBeVisible;
					break;
				case WatchTypes.BiggerThanEqual:
					base.IsVisible = value >= IndexToBeVisible;
					break;
				case WatchTypes.LessThanEqual:
					base.IsVisible = value <= IndexToBeVisible;
					break;
				case WatchTypes.NotEqual:
					base.IsVisible = value != IndexToBeVisible;
					break;
				}
			}
		}
	}

	[Editor(false)]
	public float IndexToWatchFloat
	{
		get
		{
			return _indexToWatchFloat;
		}
		set
		{
			if (_indexToWatchFloat != value)
			{
				_indexToWatchFloat = value;
				OnPropertyChanged(value, "IndexToWatchFloat");
				switch (WatchType)
				{
				case WatchTypes.Equal:
					base.IsVisible = value == IndexToBeVisibleFloat;
					break;
				case WatchTypes.BiggerThan:
					base.IsVisible = value > IndexToBeVisibleFloat;
					break;
				case WatchTypes.LessThan:
					base.IsVisible = value < IndexToBeVisibleFloat;
					break;
				case WatchTypes.BiggerThanEqual:
					base.IsVisible = value >= IndexToBeVisibleFloat;
					break;
				case WatchTypes.LessThanEqual:
					base.IsVisible = value <= IndexToBeVisibleFloat;
					break;
				case WatchTypes.NotEqual:
					base.IsVisible = value != IndexToBeVisibleFloat;
					break;
				}
			}
		}
	}

	[Editor(false)]
	public int IndexToBeVisible
	{
		get
		{
			return _indexToBeVisible;
		}
		set
		{
			if (_indexToBeVisible != value)
			{
				_indexToBeVisible = value;
				OnPropertyChanged(value, "IndexToBeVisible");
			}
		}
	}

	[Editor(false)]
	public float IndexToBeVisibleFloat
	{
		get
		{
			return _indexToBeVisibleFloat;
		}
		set
		{
			if (_indexToBeVisibleFloat != value)
			{
				_indexToBeVisibleFloat = value;
				OnPropertyChanged(value, "IndexToBeVisibleFloat");
			}
		}
	}

	public ValueBasedVisibilityWidget(UIContext context)
		: base(context)
	{
	}
}
