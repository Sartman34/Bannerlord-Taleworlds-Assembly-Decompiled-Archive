using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Map.MapBar;

public class MapBarCustomValueTextWidget : TextWidget
{
	private bool _isWarning;

	private Color _normalColor;

	private Color _warningColor;

	private int _totalTroops;

	[Editor(false)]
	public int ValueAsInt
	{
		get
		{
			return _totalTroops;
		}
		set
		{
			if (value != _totalTroops)
			{
				RefreshTextAnimation(value - _totalTroops);
				_totalTroops = value;
				OnPropertyChanged(value, "ValueAsInt");
			}
		}
	}

	[Editor(false)]
	public bool IsWarning
	{
		get
		{
			return _isWarning;
		}
		set
		{
			if (value == _isWarning)
			{
				return;
			}
			_isWarning = value;
			OnPropertyChanged(value, "IsWarning");
			base.ReadOnlyBrush.GetStyleOrDefault(base.CurrentState);
			Color black = Color.Black;
			black = ((!value) ? NormalColor : WarningColor);
			foreach (Style style in base.Brush.Styles)
			{
				style.FontColor = black;
			}
		}
	}

	[Editor(false)]
	public Color NormalColor
	{
		get
		{
			return _normalColor;
		}
		set
		{
			if (value.Alpha != _normalColor.Alpha || value.Blue != _normalColor.Blue || value.Red != _normalColor.Red || value.Green != _normalColor.Green)
			{
				_normalColor = value;
				OnPropertyChanged(value, "NormalColor");
			}
		}
	}

	[Editor(false)]
	public Color WarningColor
	{
		get
		{
			return _warningColor;
		}
		set
		{
			if (value.Alpha != _warningColor.Alpha || value.Blue != _warningColor.Blue || value.Red != _warningColor.Red || value.Green != _warningColor.Green)
			{
				_warningColor = value;
				OnPropertyChanged(value, "WarningColor");
			}
		}
	}

	public MapBarCustomValueTextWidget(UIContext context)
		: base(context)
	{
		base.OverrideDefaultStateSwitchingEnabled = true;
	}

	private void RefreshTextAnimation(int valueDifference)
	{
		if (valueDifference > 0)
		{
			if (base.CurrentState == "Positive")
			{
				base.BrushRenderer.RestartAnimation();
			}
			else
			{
				SetState("Positive");
			}
		}
		else if (valueDifference < 0)
		{
			if (base.CurrentState == "Negative")
			{
				base.BrushRenderer.RestartAnimation();
			}
			else
			{
				SetState("Negative");
			}
		}
		else
		{
			Debug.FailedAssert("Value change in party label cannot be 0", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Map\\MapBar\\MapBarCustomValueTextWidget.cs", "RefreshTextAnimation", 40);
		}
	}
}
