using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Mission.OrderOfBattle;

public class OrderOfBattleHeroButtonWidget : ButtonWidget
{
	private bool _isMainHero;

	private int _mainHeroHueFactor;

	public bool IsMainHero
	{
		get
		{
			return _isMainHero;
		}
		set
		{
			if (value != _isMainHero)
			{
				_isMainHero = value;
				OnPropertyChanged(value, "IsMainHero");
				OnHeroTypeChanged();
			}
		}
	}

	public int MainHeroHueFactor
	{
		get
		{
			return _mainHeroHueFactor;
		}
		set
		{
			if (value != _mainHeroHueFactor)
			{
				_mainHeroHueFactor = value;
				OnPropertyChanged(value, "MainHeroHueFactor");
			}
		}
	}

	public OrderOfBattleHeroButtonWidget(UIContext context)
		: base(context)
	{
	}

	private void OnHeroTypeChanged()
	{
		foreach (BrushLayer layer in base.Brush.Layers)
		{
			layer.HueFactor = (IsMainHero ? MainHeroHueFactor : 0);
		}
	}
}
