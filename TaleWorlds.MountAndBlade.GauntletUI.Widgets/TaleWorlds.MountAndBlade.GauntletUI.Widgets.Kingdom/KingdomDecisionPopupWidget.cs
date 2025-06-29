using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Kingdom;

public class KingdomDecisionPopupWidget : Widget
{
	private float _kingDecisionDoneTime = -1f;

	private bool _isKingsDecisionDone;

	public int DelayAfterKingsDecision { get; set; } = 5;


	[Editor(false)]
	public bool IsKingsDecisionDone
	{
		get
		{
			return _isKingsDecisionDone;
		}
		set
		{
			if (_isKingsDecisionDone != value)
			{
				_isKingsDecisionDone = value;
				OnPropertyChanged(value, "IsKingsDecisionDone");
				if (value)
				{
					OnKingsDecisionDone();
				}
			}
		}
	}

	public KingdomDecisionPopupWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (_kingDecisionDoneTime != -1f && base.EventManager.Time - _kingDecisionDoneTime > (float)DelayAfterKingsDecision)
		{
			ExecuteFinalDone();
		}
	}

	private void ExecuteFinalDone()
	{
		EventFired("FinalDone");
		_kingDecisionDoneTime = -1f;
		foreach (Widget allChild in base.AllChildren)
		{
			if (allChild is KingdomDecisionOptionWidget kingdomDecisionOptionWidget)
			{
				kingdomDecisionOptionWidget.OnFinalDone();
			}
		}
	}

	private void OnKingsDecisionDone()
	{
		_kingDecisionDoneTime = base.EventManager.Time;
		foreach (Widget allChild in base.AllChildren)
		{
			if (allChild is KingdomDecisionOptionWidget kingdomDecisionOptionWidget)
			{
				kingdomDecisionOptionWidget.OnKingsDecisionDone();
			}
		}
	}
}
