using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public class InitialMenuAnimControllerWidget : Widget
{
	private bool _isInitialized;

	private bool _isFinalized;

	private int _currentOptionIndex;

	private int _totalOptionCount;

	private float _timer;

	private Widget _optionsList;

	private float _initialWaitTime;

	private float _waitTimeBetweenOptions;

	private float _optionFadeInTime;

	public bool IsAnimEnabled { get; set; }

	[Editor(false)]
	public Widget OptionsList
	{
		get
		{
			return _optionsList;
		}
		set
		{
			if (_optionsList != value)
			{
				_optionsList = value;
				OnPropertyChanged(value, "OptionsList");
			}
		}
	}

	[Editor(false)]
	public float InitialWaitTime
	{
		get
		{
			return _initialWaitTime;
		}
		set
		{
			if (_initialWaitTime != value)
			{
				_initialWaitTime = value;
				OnPropertyChanged(value, "InitialWaitTime");
			}
		}
	}

	[Editor(false)]
	public float WaitTimeBetweenOptions
	{
		get
		{
			return _waitTimeBetweenOptions;
		}
		set
		{
			if (_waitTimeBetweenOptions != value)
			{
				_waitTimeBetweenOptions = value;
				OnPropertyChanged(value, "WaitTimeBetweenOptions");
			}
		}
	}

	[Editor(false)]
	public float OptionFadeInTime
	{
		get
		{
			return _optionFadeInTime;
		}
		set
		{
			if (_optionFadeInTime != value)
			{
				_optionFadeInTime = value;
				OnPropertyChanged(value, "OptionFadeInTime");
			}
		}
	}

	public InitialMenuAnimControllerWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (!IsAnimEnabled)
		{
			return;
		}
		if (!_isInitialized)
		{
			Widget optionsList = OptionsList;
			if (optionsList != null && optionsList.Children?.Count > 0)
			{
				OptionsList.Children.ForEach(delegate(Widget x)
				{
					x.SetGlobalAlphaRecursively(0f);
				});
				_totalOptionCount = OptionsList.Children.Count;
				_isInitialized = true;
			}
		}
		if (!_isInitialized || _isFinalized || OptionsList == null)
		{
			return;
		}
		_timer += dt;
		if (_timer >= InitialWaitTime + (float)_currentOptionIndex * WaitTimeBetweenOptions)
		{
			OptionsList.GetChild(_currentOptionIndex)?.SetState("Activated");
			_currentOptionIndex++;
		}
		for (int i = 0; i < _currentOptionIndex; i++)
		{
			float num = InitialWaitTime + WaitTimeBetweenOptions * (float)i;
			float num2 = num + OptionFadeInTime;
			if (_timer < num2)
			{
				float alphaFactor = MathF.Clamp((_timer - num) / (num2 - num), 0f, 1f);
				OptionsList.GetChild(i)?.SetGlobalAlphaRecursively(alphaFactor);
			}
		}
		_isFinalized = _timer > InitialWaitTime + WaitTimeBetweenOptions * (float)(_totalOptionCount - 1) + OptionFadeInTime;
	}
}
