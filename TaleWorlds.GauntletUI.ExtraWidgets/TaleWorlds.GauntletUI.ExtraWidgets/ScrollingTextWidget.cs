using System;
using System.Numerics;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.ExtraWidgets;

public class ScrollingTextWidget : TextWidget
{
	private bool _shouldScroll;

	private float _scrollTimeNeeded;

	private float _scrollTimeElapsed;

	private float _totalScrollAmount;

	private float _currentScrollAmount;

	private Vec2 _currentSize;

	private bool _isHovering;

	private float _wordWidth;

	private Widget _scrollOnHoverWidget;

	private bool _isAutoScrolling = true;

	private float _scrollPerTick = 30f;

	private float _inbetweenScrollDuration = 1f;

	private TextHorizontalAlignment _defaultTextHorizontalAlignment;

	public string ActualText { get; private set; }

	[Editor(false)]
	public Widget ScrollOnHoverWidget
	{
		get
		{
			return _scrollOnHoverWidget;
		}
		set
		{
			if (value != _scrollOnHoverWidget)
			{
				_scrollOnHoverWidget = value;
				OnPropertyChanged(value, "ScrollOnHoverWidget");
			}
		}
	}

	[Editor(false)]
	public bool IsAutoScrolling
	{
		get
		{
			return _isAutoScrolling;
		}
		set
		{
			if (value != _isAutoScrolling)
			{
				_isAutoScrolling = value;
				OnPropertyChanged(value, "IsAutoScrolling");
			}
		}
	}

	[Editor(false)]
	public float ScrollPerTick
	{
		get
		{
			return _scrollPerTick;
		}
		set
		{
			if (value != _scrollPerTick)
			{
				_scrollPerTick = value;
				OnPropertyChanged(value, "ScrollPerTick");
			}
		}
	}

	[Editor(false)]
	public float InbetweenScrollDuration
	{
		get
		{
			return _inbetweenScrollDuration;
		}
		set
		{
			if (value != _inbetweenScrollDuration)
			{
				_inbetweenScrollDuration = value;
				OnPropertyChanged(value, "InbetweenScrollDuration");
			}
		}
	}

	[Editor(false)]
	public TextHorizontalAlignment DefaultTextHorizontalAlignment
	{
		get
		{
			return _defaultTextHorizontalAlignment;
		}
		set
		{
			if (value != _defaultTextHorizontalAlignment)
			{
				_defaultTextHorizontalAlignment = value;
				OnPropertyChanged(Enum.GetName(typeof(TextHorizontalAlignment), value), "DefaultTextHorizontalAlignment");
			}
		}
	}

	public ScrollingTextWidget(UIContext context)
		: base(context)
	{
		ScrollOnHoverWidget = this;
		DefaultTextHorizontalAlignment = base.Brush.TextHorizontalAlignment;
		base.ClipHorizontalContent = true;
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (base.Size != _currentSize)
		{
			_currentSize = base.Size;
			UpdateScrollable();
		}
		if (_shouldScroll)
		{
			_scrollTimeElapsed += dt;
			if (_scrollTimeElapsed < InbetweenScrollDuration)
			{
				_currentScrollAmount = 0f;
			}
			else if (_scrollTimeElapsed >= InbetweenScrollDuration && _currentScrollAmount < _totalScrollAmount)
			{
				_currentScrollAmount += dt * ScrollPerTick;
			}
			else if (_currentScrollAmount >= _totalScrollAmount)
			{
				if (_scrollTimeNeeded.ApproximatelyEqualsTo(0f))
				{
					_scrollTimeNeeded = _scrollTimeElapsed;
				}
				if (_scrollTimeElapsed < _scrollTimeNeeded + InbetweenScrollDuration)
				{
					_currentScrollAmount = _totalScrollAmount;
				}
				else
				{
					_scrollTimeNeeded = 0f;
					_scrollTimeElapsed = 0f;
				}
			}
		}
		if (base.EventManager.HoveredView == ScrollOnHoverWidget && !_isHovering)
		{
			if (!IsAutoScrolling)
			{
				_text.Value = ActualText;
				_shouldScroll = _wordWidth > base.Size.X;
			}
			_isHovering = true;
		}
		else if (base.EventManager.HoveredView != ScrollOnHoverWidget && _isHovering)
		{
			ResetScroll();
			UpdateScrollable();
			_isHovering = false;
		}
	}

	public override void OnBrushChanged()
	{
		DefaultTextHorizontalAlignment = base.Brush.TextHorizontalAlignment;
		UpdateScrollable();
	}

	protected override void SetText(string value)
	{
		base.SetText(value);
		_text.SkipLineOnContainerExceeded = false;
		ActualText = _text.Value;
		UpdateScrollable();
	}

	protected override void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
	{
		RefreshTextParameters();
		TextMaterial textMaterial = base.BrushRenderer.CreateTextMaterial(drawContext);
		textMaterial.AlphaFactor *= base.Context.ContextAlpha;
		Vector2 cachedGlobalPosition = _cachedGlobalPosition;
		drawContext.Draw(_text, textMaterial, cachedGlobalPosition.X - _currentScrollAmount, cachedGlobalPosition.Y, base.Size.X, base.Size.Y);
	}

	private void UpdateScrollable()
	{
		UpdateWordWidth();
		if (_wordWidth > base.Size.X)
		{
			_shouldScroll = IsAutoScrolling;
			_totalScrollAmount = _wordWidth - base.Size.X;
			base.Brush.TextHorizontalAlignment = TextHorizontalAlignment.Left;
			if (IsAutoScrolling)
			{
				return;
			}
			Font mappedFontForLocalization = base.Context.FontFactory.GetMappedFontForLocalization(base.Brush?.Font?.Name);
			for (int num = _text.Value.Length; num > 3; num--)
			{
				if (mappedFontForLocalization.GetWordWidth(_text.Value.Substring(0, num - 3) + "...", 0.25f) * ((float)base.Brush.FontSize / (float)mappedFontForLocalization.Size) * base._scaleToUse < base.Size.X)
				{
					_text.Value = _text.Value.Substring(0, num - 3) + "...";
					break;
				}
			}
		}
		else
		{
			ResetScroll();
		}
	}

	private void UpdateWordWidth()
	{
		Font mappedFontForLocalization = base.Context.FontFactory.GetMappedFontForLocalization(base.Brush?.Font?.Name);
		_wordWidth = mappedFontForLocalization.GetWordWidth(_text.Value, 0.5f) * ((float)base.Brush.FontSize / (float)mappedFontForLocalization.Size) * base._scaleToUse;
	}

	private void ResetScroll()
	{
		_shouldScroll = false;
		_scrollTimeElapsed = 0f;
		_currentScrollAmount = 0f;
		base.Brush.TextHorizontalAlignment = DefaultTextHorizontalAlignment;
	}
}
