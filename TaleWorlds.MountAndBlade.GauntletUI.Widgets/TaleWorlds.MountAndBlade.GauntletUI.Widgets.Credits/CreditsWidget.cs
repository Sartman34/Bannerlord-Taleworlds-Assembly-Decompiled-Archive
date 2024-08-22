using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Credits;

public class CreditsWidget : Widget
{
	private float _currentOffset = 1080f;

	private Widget _rootItemWidget;

	[Editor(false)]
	public Widget RootItemWidget
	{
		get
		{
			return _rootItemWidget;
		}
		set
		{
			if (_rootItemWidget != value)
			{
				_rootItemWidget = value;
				OnPropertyChanged(value, "RootItemWidget");
			}
		}
	}

	public CreditsWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (RootItemWidget != null)
		{
			RootItemWidget.PositionYOffset = _currentOffset;
			_currentOffset -= dt * 75f;
			if (_currentOffset < (0f - RootItemWidget.Size.Y) * base._inverseScaleToUse)
			{
				_currentOffset = 1080f;
			}
		}
	}
}
