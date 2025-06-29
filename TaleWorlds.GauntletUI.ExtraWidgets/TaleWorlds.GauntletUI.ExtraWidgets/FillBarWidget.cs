using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.ExtraWidgets;

public class FillBarWidget : Widget
{
	private Widget _fillWidget;

	private Widget _changeWidget;

	private Widget _dividerWidget;

	private float _maxAmount;

	private float _currentAmount;

	private float _initialAmount;

	private bool _completelyFillChange;

	private bool _showNegativeChange;

	[Editor(false)]
	public int CurrentAmount
	{
		get
		{
			return (int)_currentAmount;
		}
		set
		{
			if (_currentAmount != (float)value)
			{
				_currentAmount = value;
				OnPropertyChanged(value, "CurrentAmount");
			}
		}
	}

	[Editor(false)]
	public int MaxAmount
	{
		get
		{
			return (int)_maxAmount;
		}
		set
		{
			if (_maxAmount != (float)value)
			{
				_maxAmount = value;
				OnPropertyChanged(value, "MaxAmount");
			}
		}
	}

	[Editor(false)]
	public int InitialAmount
	{
		get
		{
			return (int)_initialAmount;
		}
		set
		{
			if (_initialAmount != (float)value)
			{
				_initialAmount = value;
				OnPropertyChanged(value, "InitialAmount");
			}
		}
	}

	[Editor(false)]
	public float MaxAmountAsFloat
	{
		get
		{
			return _maxAmount;
		}
		set
		{
			if (_maxAmount != value)
			{
				_maxAmount = value;
				OnPropertyChanged(value, "MaxAmountAsFloat");
			}
		}
	}

	[Editor(false)]
	public float CurrentAmountAsFloat
	{
		get
		{
			return _currentAmount;
		}
		set
		{
			if (_currentAmount != value)
			{
				_currentAmount = value;
				OnPropertyChanged(value, "CurrentAmountAsFloat");
			}
		}
	}

	[Editor(false)]
	public float InitialAmountAsFloat
	{
		get
		{
			return _initialAmount;
		}
		set
		{
			if (_initialAmount != value)
			{
				_initialAmount = value;
				OnPropertyChanged(value, "InitialAmountAsFloat");
			}
		}
	}

	[Editor(false)]
	public bool CompletelyFillChange
	{
		get
		{
			return _completelyFillChange;
		}
		set
		{
			if (_completelyFillChange != value)
			{
				_completelyFillChange = value;
				OnPropertyChanged(value, "CompletelyFillChange");
			}
		}
	}

	[Editor(false)]
	public bool ShowNegativeChange
	{
		get
		{
			return _showNegativeChange;
		}
		set
		{
			if (_showNegativeChange != value)
			{
				_showNegativeChange = value;
				OnPropertyChanged(value, "ShowNegativeChange");
			}
		}
	}

	public Widget FillWidget
	{
		get
		{
			return _fillWidget;
		}
		set
		{
			if (_fillWidget != value)
			{
				_fillWidget = value;
				OnPropertyChanged(value, "FillWidget");
			}
		}
	}

	public Widget ChangeWidget
	{
		get
		{
			return _changeWidget;
		}
		set
		{
			if (_changeWidget != value)
			{
				_changeWidget = value;
				OnPropertyChanged(value, "ChangeWidget");
			}
		}
	}

	public Widget DividerWidget
	{
		get
		{
			return _dividerWidget;
		}
		set
		{
			if (_dividerWidget != value)
			{
				_dividerWidget = value;
				OnPropertyChanged(value, "DividerWidget");
			}
		}
	}

	public FillBarWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
	{
		if (FillWidget != null)
		{
			float x = FillWidget.ParentWidget.Size.X;
			float num = Mathf.Clamp(Mathf.Clamp(_initialAmount, 0f, _maxAmount) / _maxAmount, 0f, 1f);
			FillWidget.ScaledSuggestedWidth = num * x;
			if (ChangeWidget != null)
			{
				float num2 = Mathf.Clamp(Mathf.Clamp(_currentAmount - _initialAmount, 0f - _maxAmount, _maxAmount) / _maxAmount, -1f, 1f);
				if (num2 > 0f)
				{
					if (CompletelyFillChange)
					{
						float num3 = Mathf.Clamp(Mathf.Clamp(_currentAmount, 0f, _maxAmount) / _maxAmount, 0f, 1f);
						ChangeWidget.ScaledSuggestedWidth = num3 * x;
					}
					else
					{
						ChangeWidget.ScaledSuggestedWidth = Mathf.Clamp(num2 * x, 0f, x - FillWidget.ScaledSuggestedWidth);
						ChangeWidget.ScaledPositionXOffset = FillWidget.ScaledSuggestedWidth;
					}
					ChangeWidget.Color = new Color(1f, 1f, 1f);
				}
				else if (num2 < 0f && ShowNegativeChange)
				{
					ChangeWidget.ScaledSuggestedWidth = num2 * x * -1f;
					ChangeWidget.ScaledPositionXOffset = FillWidget.ScaledSuggestedWidth - ChangeWidget.ScaledSuggestedWidth;
					ChangeWidget.Color = new Color(1f, 0f, 0f);
				}
				else
				{
					ChangeWidget.ScaledSuggestedWidth = 0f;
				}
				if (DividerWidget != null)
				{
					if (num2 > 0f)
					{
						DividerWidget.ScaledPositionXOffset = ChangeWidget.ScaledPositionXOffset - DividerWidget.Size.X;
					}
					else if (num2 < 0f)
					{
						DividerWidget.ScaledPositionXOffset = FillWidget.ScaledSuggestedWidth - DividerWidget.Size.X;
					}
					DividerWidget.IsVisible = ChangeWidget != null && num2 != 0f;
				}
			}
		}
		base.OnRender(twoDimensionContext, drawContext);
	}
}
