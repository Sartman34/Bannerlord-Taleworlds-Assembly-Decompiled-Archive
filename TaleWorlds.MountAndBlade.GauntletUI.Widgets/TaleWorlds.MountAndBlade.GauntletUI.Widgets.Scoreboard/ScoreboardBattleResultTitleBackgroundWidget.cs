using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Scoreboard;

public class ScoreboardBattleResultTitleBackgroundWidget : Widget
{
	private int _battleResult;

	private Widget _victoryWidget;

	private Widget _defeatWidget;

	private Widget _retreatWidget;

	[Editor(false)]
	public int BattleResult
	{
		get
		{
			return _battleResult;
		}
		set
		{
			if (_battleResult != value)
			{
				_battleResult = value;
				OnPropertyChanged(value, "BattleResult");
				BattleResultUpdated();
			}
		}
	}

	[Editor(false)]
	public Widget VictoryWidget
	{
		get
		{
			return _victoryWidget;
		}
		set
		{
			if (_victoryWidget != value)
			{
				_victoryWidget = value;
				OnPropertyChanged(value, "VictoryWidget");
			}
		}
	}

	[Editor(false)]
	public Widget DefeatWidget
	{
		get
		{
			return _defeatWidget;
		}
		set
		{
			if (_defeatWidget != value)
			{
				_defeatWidget = value;
				OnPropertyChanged(value, "DefeatWidget");
			}
		}
	}

	[Editor(false)]
	public Widget RetreatWidget
	{
		get
		{
			return _retreatWidget;
		}
		set
		{
			if (_retreatWidget != value)
			{
				_retreatWidget = value;
				OnPropertyChanged(value, "RetreatWidget");
			}
		}
	}

	public ScoreboardBattleResultTitleBackgroundWidget(UIContext context)
		: base(context)
	{
	}

	private void BattleResultUpdated()
	{
		DefeatWidget.IsVisible = BattleResult == 0;
		VictoryWidget.IsVisible = BattleResult == 1;
		RetreatWidget.IsVisible = BattleResult == 2;
	}
}
