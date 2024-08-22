using System;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.GauntletUI.ExtraWidgets;

public class ScrollingRichTextWidget : RichTextWidget
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

	public ScrollingRichTextWidget(UIContext context)
		: base(context)
	{
		ScrollOnHoverWidget = this;
		DefaultTextHorizontalAlignment = base.Brush.TextHorizontalAlignment;
		base.ClipContents = true;
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
			_isHovering = true;
			if (!IsAutoScrolling)
			{
				base.Text = ActualText;
				_shouldScroll = _wordWidth > base.Size.X;
			}
		}
		else if (base.EventManager.HoveredView != ScrollOnHoverWidget && _isHovering)
		{
			_isHovering = false;
			ResetScroll();
			UpdateScrollable();
		}
		_renderXOffset = 0f - _currentScrollAmount;
	}

	public override void OnBrushChanged()
	{
		base.OnBrushChanged();
		DefaultTextHorizontalAlignment = base.Brush.TextHorizontalAlignment;
		UpdateScrollable();
	}

	protected override void SetText(string value)
	{
		base.SetText(value);
		_richText.SkipLineOnContainerExceeded = false;
		ActualText = _richText.Value;
		UpdateScrollable();
	}

	private void UpdateScrollable()
	{
		UpdateWordWidth();
		if (_wordWidth > base.Size.X)
		{
			_shouldScroll = IsAutoScrolling;
			_totalScrollAmount = _wordWidth - base.Size.X;
			base.Brush.TextHorizontalAlignment = TextHorizontalAlignment.Left;
			Font mappedFontForLocalization = base.Context.FontFactory.GetMappedFontForLocalization(base.Brush?.Font?.Name);
			if (IsAutoScrolling || _isHovering)
			{
				return;
			}
			bool flag = false;
			for (int num = _richText.Value.Length; num > 3; num--)
			{
				if (_richText.Value[num - 1] == '>')
				{
					flag = true;
				}
				else if (_richText.Value[num - 1] == '<')
				{
					flag = false;
				}
				if (!flag && mappedFontForLocalization.GetWordWidth(_richText.Value.Substring(0, num - 3) + "...", 0.25f) * ((float)base.Brush.FontSize / (float)mappedFontForLocalization.Size) * base._scaleToUse < base.Size.X)
				{
					_richText.Value = _richText.Value.Substring(0, num - 3) + "...";
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
		_wordWidth = mappedFontForLocalization.GetWordWidth(_richText.Value, 0.5f) * ((float)base.Brush.FontSize / (float)mappedFontForLocalization.Size) * base._scaleToUse;
	}

	private void ResetScroll()
	{
		_shouldScroll = false;
		_scrollTimeElapsed = 0f;
		_currentScrollAmount = 0f;
		base.Brush.TextHorizontalAlignment = DefaultTextHorizontalAlignment;
	}
}
