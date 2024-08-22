using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public class SceneWidget : TextureWidget
{
	private bool _isInClickToContinueState;

	private bool _prevIsClickToContinueActive;

	private bool _initialized;

	private float _clickToContinueStartTime = -1f;

	private string _currentTitleTextToUpdateTo = string.Empty;

	private float _titleChangeStartTime = -1f;

	private float _titleChangeTotalTimeInSeconds = 2f;

	private object _scene;

	private ButtonWidget _cancelButton;

	private ButtonWidget _affirmativeButton;

	private RichTextWidget _clickToContinueTextWidget;

	private Widget _fadeImageWidget;

	private TextWidget _titleTextWidget;

	private float _endProgress;

	private string _affirmativeTitleText;

	private string _negativeTitleText;

	private Widget _preparingVisualWidget;

	private bool _isCancelShown;

	private bool _isOkShown;

	private bool _isReady;

	private bool _isClickToContinueActive
	{
		get
		{
			if (_clickToContinueStartTime != -1f)
			{
				return base.EventManager.Time - _clickToContinueStartTime >= _clickToContinueDelayInSeconds;
			}
			return false;
		}
	}

	private float _clickToContinueDelayInSeconds => 2f;

	[Editor(false)]
	public object Scene
	{
		get
		{
			return _scene;
		}
		set
		{
			if (value != _scene)
			{
				_scene = value;
				OnPropertyChanged(value, "Scene");
				SetTextureProviderProperty("Scene", value);
				if (value != null)
				{
					_isTargetSizeDirty = true;
					ResetStates();
				}
			}
		}
	}

	[Editor(false)]
	public ButtonWidget AffirmativeButton
	{
		get
		{
			return _affirmativeButton;
		}
		set
		{
			if (value != _affirmativeButton)
			{
				_affirmativeButton = value;
				OnPropertyChanged(value, "AffirmativeButton");
				_affirmativeButton?.ClickEventHandlers.Add(OnAffirmativeButtonClick);
			}
		}
	}

	[Editor(false)]
	public ButtonWidget CancelButton
	{
		get
		{
			return _cancelButton;
		}
		set
		{
			if (value != _cancelButton)
			{
				_cancelButton = value;
				OnPropertyChanged(value, "CancelButton");
				_cancelButton?.ClickEventHandlers.Add(OnCancelButtonClick);
			}
		}
	}

	[Editor(false)]
	public RichTextWidget ClickToContinueTextWidget
	{
		get
		{
			return _clickToContinueTextWidget;
		}
		set
		{
			if (value != _clickToContinueTextWidget)
			{
				_clickToContinueTextWidget = value;
				OnPropertyChanged(value, "ClickToContinueTextWidget");
			}
		}
	}

	[Editor(false)]
	public TextWidget TitleTextWidget
	{
		get
		{
			return _titleTextWidget;
		}
		set
		{
			if (value != _titleTextWidget)
			{
				_titleTextWidget = value;
				OnPropertyChanged(value, "TitleTextWidget");
			}
		}
	}

	[Editor(false)]
	public Widget FadeImageWidget
	{
		get
		{
			return _fadeImageWidget;
		}
		set
		{
			if (value != _fadeImageWidget)
			{
				_fadeImageWidget = value;
				OnPropertyChanged(value, "FadeImageWidget");
			}
		}
	}

	[Editor(false)]
	public Widget PreparingVisualWidget
	{
		get
		{
			return _preparingVisualWidget;
		}
		set
		{
			if (value != _preparingVisualWidget)
			{
				_preparingVisualWidget = value;
				OnPropertyChanged(value, "PreparingVisualWidget");
			}
		}
	}

	[Editor(false)]
	public float EndProgress
	{
		get
		{
			return _endProgress;
		}
		set
		{
			if (value != _endProgress)
			{
				_endProgress = value;
				OnPropertyChanged(value, "EndProgress");
			}
		}
	}

	[Editor(false)]
	public bool IsOkShown
	{
		get
		{
			return _isOkShown;
		}
		set
		{
			if (value != _isOkShown)
			{
				_isOkShown = value;
				OnPropertyChanged(value, "IsOkShown");
				DetermineInitContinueState();
			}
		}
	}

	[Editor(false)]
	public bool IsCancelShown
	{
		get
		{
			return _isCancelShown;
		}
		set
		{
			if (value != _isCancelShown)
			{
				_isCancelShown = value;
				OnPropertyChanged(value, "IsCancelShown");
				DetermineInitContinueState();
			}
		}
	}

	[Editor(false)]
	public bool IsReady
	{
		get
		{
			return _isReady;
		}
		set
		{
			if (value != _isReady)
			{
				_isReady = value;
				OnPropertyChanged(value, "IsReady");
			}
		}
	}

	[Editor(false)]
	public string AffirmativeTitleText
	{
		get
		{
			return _affirmativeTitleText;
		}
		set
		{
			if (value != _affirmativeTitleText)
			{
				_affirmativeTitleText = value;
				OnPropertyChanged(value, "AffirmativeTitleText");
			}
		}
	}

	[Editor(false)]
	public string NegativeTitleText
	{
		get
		{
			return _negativeTitleText;
		}
		set
		{
			if (value != _negativeTitleText)
			{
				_negativeTitleText = value;
				OnPropertyChanged(value, "NegativeTitleText");
			}
		}
	}

	public SceneWidget(UIContext context)
		: base(context)
	{
		base.TextureProviderName = "SceneTextureProvider";
		_isRenderRequestedPreviousFrame = true;
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (Scene != null && !_initialized)
		{
			DetermineInitContinueState();
			_initialized = true;
		}
		if (Scene != null && !IsReady)
		{
			IsReady = (bool?)GetTextureProviderProperty("IsReady") == true;
		}
		if (_isInClickToContinueState)
		{
			if (_isClickToContinueActive)
			{
				ClickToContinueTextWidget.SetGlobalAlphaRecursively(Mathf.Lerp(ClickToContinueTextWidget.ReadOnlyBrush.GlobalAlphaFactor, 1f, dt * 10f));
				if (!_prevIsClickToContinueActive)
				{
					ClickToContinueTextWidget.BrushRenderer.RestartAnimation();
				}
			}
			CancelButton.SetGlobalAlphaRecursively(Mathf.Lerp(CancelButton.ReadOnlyBrush.GlobalAlphaFactor, 0f, dt * 10f));
			AffirmativeButton.SetGlobalAlphaRecursively(Mathf.Lerp(AffirmativeButton.ReadOnlyBrush.GlobalAlphaFactor, 0f, dt * 10f));
		}
		else
		{
			ClickToContinueTextWidget.SetGlobalAlphaRecursively(Mathf.Lerp(ClickToContinueTextWidget.ReadOnlyBrush.GlobalAlphaFactor, 0f, dt * 10f));
			CancelButton.SetGlobalAlphaRecursively(Mathf.Lerp(CancelButton.ReadOnlyBrush.GlobalAlphaFactor, IsCancelShown ? 1 : 0, dt * 10f));
			AffirmativeButton.SetGlobalAlphaRecursively(Mathf.Lerp(AffirmativeButton.ReadOnlyBrush.GlobalAlphaFactor, IsOkShown ? 1 : 0, dt * 10f));
		}
		UpdateVisibilityOfWidgetBasedOnAlpha(ClickToContinueTextWidget);
		UpdateVisibilityOfWidgetBasedOnAlpha(CancelButton);
		UpdateVisibilityOfWidgetBasedOnAlpha(AffirmativeButton);
		HandleTitleTextChange();
		FadeImageWidget.AlphaFactor = (IsReady ? EndProgress : 1f);
		PreparingVisualWidget.IsVisible = !IsReady;
		_prevIsClickToContinueActive = _isClickToContinueActive;
	}

	private void UpdateVisibilityOfWidgetBasedOnAlpha(BrushWidget widget)
	{
		widget.IsVisible = !widget.ReadOnlyBrush.GlobalAlphaFactor.ApproximatelyEqualsTo(0f, 0.01f);
	}

	protected override void OnMousePressed()
	{
		base.OnMousePressed();
		if (_isClickToContinueActive)
		{
			EventFired("Close");
			ResetStates();
		}
	}

	private void OnAnyActionButtonClick()
	{
		_isInClickToContinueState = true;
		base.DoNotAcceptEvents = false;
		base.DoNotPassEventsToChildren = true;
		ClickToContinueTextWidget.BrushRenderer.RestartAnimation();
		_clickToContinueStartTime = base.EventManager.Time;
	}

	private void ResetStates()
	{
		_isInClickToContinueState = false;
		base.DoNotAcceptEvents = true;
		base.DoNotPassEventsToChildren = false;
		_initialized = false;
		IsReady = false;
		_titleChangeStartTime = -1f;
		_currentTitleTextToUpdateTo = string.Empty;
		TitleTextWidget.SetAlpha(1f);
	}

	private void OnAffirmativeButtonClick(Widget obj)
	{
		SetNewTitleText(AffirmativeTitleText);
		OnAnyActionButtonClick();
	}

	private void OnCancelButtonClick(Widget obj)
	{
		SetNewTitleText(NegativeTitleText);
		OnAnyActionButtonClick();
	}

	private void SetNewTitleText(string newText)
	{
		if (!string.IsNullOrEmpty(newText))
		{
			_currentTitleTextToUpdateTo = newText;
			_titleChangeStartTime = base.EventManager.Time;
		}
	}

	private void DetermineInitContinueState()
	{
		CancelButton.IsVisible = IsCancelShown;
		CancelButton.SetGlobalAlphaRecursively(IsCancelShown ? 1 : 0);
		AffirmativeButton.IsVisible = IsOkShown;
		AffirmativeButton.SetGlobalAlphaRecursively(IsOkShown ? 1 : 0);
		ClickToContinueTextWidget.SetGlobalAlphaRecursively(0f);
		_isInClickToContinueState = !IsCancelShown && !IsOkShown;
		if (_isInClickToContinueState)
		{
			_clickToContinueStartTime = base.EventManager.Time;
		}
		base.DoNotAcceptEvents = !_isInClickToContinueState;
		base.DoNotPassEventsToChildren = _isInClickToContinueState;
	}

	private void HandleTitleTextChange()
	{
		if (_titleChangeStartTime == -1f)
		{
			return;
		}
		if (!string.IsNullOrEmpty(_currentTitleTextToUpdateTo) && TitleTextWidget != null && base.EventManager.Time - _titleChangeStartTime < _titleChangeTotalTimeInSeconds)
		{
			if (base.EventManager.Time - _titleChangeStartTime >= _titleChangeTotalTimeInSeconds / 2f)
			{
				TitleTextWidget.Text = _currentTitleTextToUpdateTo;
			}
			float alphaFactor = 1f - MathF.PingPong(0f, 1f, base.EventManager.Time - _titleChangeStartTime);
			TitleTextWidget.SetAlpha(alphaFactor);
		}
		else
		{
			TitleTextWidget.SetAlpha(1f);
		}
	}
}
