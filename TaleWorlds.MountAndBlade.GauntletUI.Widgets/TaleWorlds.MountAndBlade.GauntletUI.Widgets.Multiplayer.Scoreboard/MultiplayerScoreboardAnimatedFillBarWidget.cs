using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.ExtraWidgets;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Multiplayer.Scoreboard;

public class MultiplayerScoreboardAnimatedFillBarWidget : FillBarWidget
{
	public delegate void FullFillFinishedHandler();

	private float _currentTargetRatioOfChange;

	private float _finalRatio;

	private float _ratioOfChange;

	private bool _isStarted;

	private bool _inFinalFillState;

	private bool _inFirstFillState = true;

	private float _timePassed;

	private float _ratioOfChangePerTick;

	private string _xpBarSoundEventName = "multiplayer/xpbar";

	private string _xpBarStopSoundEventName = "multiplayer/xpbar_stop";

	private bool _isStartRequested;

	private float _animationDelay;

	private float _animationDuration;

	private Widget _changeOverlayWidget;

	private int _timesOfFullFill;

	[Editor(false)]
	public bool IsStartRequested
	{
		get
		{
			return _isStartRequested;
		}
		set
		{
			if (value != _isStartRequested)
			{
				_isStartRequested = value;
				OnPropertyChanged(value, "IsStartRequested");
			}
		}
	}

	[Editor(false)]
	public float AnimationDelay
	{
		get
		{
			return _animationDelay;
		}
		set
		{
			if (_animationDelay != value)
			{
				_animationDelay = value;
				OnPropertyChanged(value, "AnimationDelay");
			}
		}
	}

	[Editor(false)]
	public float AnimationDuration
	{
		get
		{
			return _animationDuration;
		}
		set
		{
			if (_animationDuration != value)
			{
				_animationDuration = value;
				OnPropertyChanged(value, "AnimationDuration");
			}
		}
	}

	[Editor(false)]
	public int TimesOfFullFill
	{
		get
		{
			return _timesOfFullFill;
		}
		set
		{
			if (_timesOfFullFill != value)
			{
				_timesOfFullFill = value;
				OnPropertyChanged(value, "TimesOfFullFill");
			}
		}
	}

	[Editor(false)]
	public Widget ChangeOverlayWidget
	{
		get
		{
			return _changeOverlayWidget;
		}
		set
		{
			if (_changeOverlayWidget != value)
			{
				_changeOverlayWidget = value;
				OnPropertyChanged(value, "ChangeOverlayWidget");
			}
		}
	}

	public event FullFillFinishedHandler OnFullFillFinished;

	public MultiplayerScoreboardAnimatedFillBarWidget(UIContext context)
		: base(context)
	{
	}

	public void StartAnimation()
	{
		if (base.FillWidget != null && base.ChangeWidget != null && !(MathF.Abs(AnimationDuration) <= float.Epsilon))
		{
			float num = Mathf.Clamp(Mathf.Clamp(base.InitialAmount, 0f, base.MaxAmount) / (float)base.MaxAmount, 0f, 1f);
			float num2 = Mathf.Clamp((float)(base.CurrentAmount - base.InitialAmount), (float)(-base.MaxAmount), (float)base.MaxAmount);
			_finalRatio = num + Mathf.Clamp(num2 / (float)base.MaxAmount, -1f, 1f);
			if (!_isStarted)
			{
				base.Context.TwoDimensionContext.CreateSoundEvent(_xpBarSoundEventName);
				base.Context.TwoDimensionContext.PlaySoundEvent(_xpBarSoundEventName);
			}
			if (TimesOfFullFill > 0)
			{
				_currentTargetRatioOfChange = 1f;
			}
			else
			{
				_currentTargetRatioOfChange = _finalRatio;
				_inFinalFillState = true;
			}
			_inFirstFillState = true;
			_ratioOfChange = num;
			_isStarted = true;
			_timePassed = 0f;
			_ratioOfChangePerTick = AnimationDuration / ((float)_timesOfFullFill + _finalRatio);
		}
	}

	public void Reset()
	{
		_timePassed = 0f;
		_ratioOfChange = 0f;
		_currentTargetRatioOfChange = 0f;
		_ratioOfChangePerTick = 0f;
		_inFirstFillState = true;
		_isStarted = false;
		base.Context.TwoDimensionContext.StopAndRemoveSoundEvent(_xpBarSoundEventName);
	}

	protected override void OnUpdate(float dt)
	{
		if (IsStartRequested && !_isStarted && !_inFinalFillState)
		{
			StartAnimation();
		}
		if (!_isStarted)
		{
			return;
		}
		_timePassed += dt;
		if (!(_timePassed >= AnimationDelay))
		{
			return;
		}
		_ratioOfChange += dt / _ratioOfChangePerTick;
		if (!(_ratioOfChange > _currentTargetRatioOfChange))
		{
			return;
		}
		if (_timesOfFullFill > 0)
		{
			_currentTargetRatioOfChange = 1f;
			_ratioOfChange = 0f;
			_timesOfFullFill--;
			_inFirstFillState = false;
			if (_timesOfFullFill == 0)
			{
				_currentTargetRatioOfChange = _finalRatio;
				_inFinalFillState = true;
			}
			this.OnFullFillFinished?.Invoke();
		}
		else if (_inFinalFillState || _timesOfFullFill == 0)
		{
			_inFinalFillState = true;
			_ratioOfChange = _finalRatio;
			_isStarted = false;
			base.Context.TwoDimensionContext.StopAndRemoveSoundEvent(_xpBarSoundEventName);
			base.Context.TwoDimensionContext.PlaySound(_xpBarStopSoundEventName);
		}
	}

	protected override void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
	{
		if (base.FillWidget == null)
		{
			return;
		}
		float x = base.FillWidget.ParentWidget.Size.X;
		float num = Mathf.Clamp(Mathf.Clamp(base.InitialAmount, 0f, base.MaxAmount) / (float)base.MaxAmount, 0f, 1f);
		base.FillWidget.ScaledSuggestedWidth = num * x;
		base.FillWidget.IsVisible = _inFirstFillState;
		if (base.ChangeWidget == null)
		{
			return;
		}
		if (_ratioOfChange >= 0f)
		{
			base.ChangeWidget.ScaledSuggestedWidth = _ratioOfChange * x;
			base.ChangeWidget.Color = new Color(1f, 1f, 1f);
		}
		if (base.DividerWidget != null)
		{
			if (_ratioOfChange > 0f)
			{
				base.DividerWidget.ScaledPositionXOffset = base.FillWidget.ScaledSuggestedWidth - base.DividerWidget.Size.X;
			}
			base.DividerWidget.IsVisible = _ratioOfChange != 0f;
		}
		if (ChangeOverlayWidget != null)
		{
			ChangeOverlayWidget.ScaledPositionXOffset = base.ChangeWidget.ScaledMarginLeft + base.ChangeWidget.ScaledSuggestedWidth - ChangeOverlayWidget.Size.X;
			ChangeOverlayWidget.IsVisible = _ratioOfChange != 0f;
		}
	}
}
