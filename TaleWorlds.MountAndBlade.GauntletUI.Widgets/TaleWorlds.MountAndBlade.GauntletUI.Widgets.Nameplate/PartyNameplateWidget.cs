using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Nameplate;

public class PartyNameplateWidget : Widget
{
	public enum TutorialAnimState
	{
		Idle,
		Start,
		FirstFrame,
		Playing
	}

	private bool _isFirstFrame = true;

	private float _screenWidth;

	private float _screenHeight;

	private bool _latestIsOutside;

	private float _initialDelayAmount = 2f;

	private int _defaultNameplateFontSize;

	private TutorialAnimState _tutorialAnimState;

	private Vec2 _position;

	private Vec2 _headPosition;

	private TextWidget _nameplateTextWidget;

	private TextWidget _nameplateFullNameTextWidget;

	private TextWidget _speedTextWidget;

	private Widget _speedIconWidget;

	private TextWidget _nameplateExtraInfoTextWidget;

	private Widget _trackerFrame;

	private Widget _mainPartyArrowWidget;

	private ListPanel _nameplateLayoutListPanel;

	private MaskedTextureWidget _partyBannerWidget;

	private bool _isVisibleOnMap;

	private bool _isMainParty;

	private bool _isInside;

	private bool _isBehind;

	private bool _isHigh;

	private bool _isInArmy;

	private bool _isInSettlement;

	private bool _isArmy;

	private bool _isTargetedByTutorial;

	private bool _shouldShowFullName;

	private bool _isPrisoner;

	private float _animSpeedModifier => 8f;

	private int _armyFontSizeOffset => 10;

	public Widget HeadGroupWidget { get; set; }

	public ListPanel NameplateLayoutListPanel
	{
		get
		{
			return _nameplateLayoutListPanel;
		}
		set
		{
			if (_nameplateLayoutListPanel != value)
			{
				_nameplateLayoutListPanel = value;
				OnPropertyChanged(value, "NameplateLayoutListPanel");
			}
		}
	}

	public MaskedTextureWidget PartyBannerWidget
	{
		get
		{
			return _partyBannerWidget;
		}
		set
		{
			if (_partyBannerWidget != value)
			{
				_partyBannerWidget = value;
				OnPropertyChanged(value, "PartyBannerWidget");
			}
		}
	}

	public Widget TrackerFrame
	{
		get
		{
			return _trackerFrame;
		}
		set
		{
			if (_trackerFrame != value)
			{
				_trackerFrame = value;
				OnPropertyChanged(value, "TrackerFrame");
			}
		}
	}

	public Vec2 Position
	{
		get
		{
			return _position;
		}
		set
		{
			if (_position != value)
			{
				_position = value;
				OnPropertyChanged(value, "Position");
			}
		}
	}

	public Vec2 HeadPosition
	{
		get
		{
			return _headPosition;
		}
		set
		{
			if (_headPosition != value)
			{
				_headPosition = value;
				OnPropertyChanged(value, "HeadPosition");
			}
		}
	}

	public bool ShouldShowFullName
	{
		get
		{
			return _shouldShowFullName;
		}
		set
		{
			if (_shouldShowFullName != value)
			{
				_shouldShowFullName = value;
				OnPropertyChanged(value, "ShouldShowFullName");
			}
		}
	}

	public bool IsTargetedByTutorial
	{
		get
		{
			return _isTargetedByTutorial;
		}
		set
		{
			if (_isTargetedByTutorial != value)
			{
				_isTargetedByTutorial = value;
				OnPropertyChanged(value, "IsTargetedByTutorial");
				_tutorialAnimState = TutorialAnimState.Start;
			}
		}
	}

	public bool IsInArmy
	{
		get
		{
			return _isInArmy;
		}
		set
		{
			if (_isInArmy != value)
			{
				_isInArmy = value;
				OnPropertyChanged(value, "IsInArmy");
			}
		}
	}

	public bool IsInSettlement
	{
		get
		{
			return _isInSettlement;
		}
		set
		{
			if (_isInSettlement != value)
			{
				_isInSettlement = value;
				OnPropertyChanged(value, "IsInSettlement");
			}
		}
	}

	public bool IsArmy
	{
		get
		{
			return _isArmy;
		}
		set
		{
			if (_isArmy != value)
			{
				_isArmy = value;
				OnPropertyChanged(value, "IsArmy");
			}
		}
	}

	public bool IsVisibleOnMap
	{
		get
		{
			return _isVisibleOnMap;
		}
		set
		{
			if (_isVisibleOnMap != value)
			{
				_isVisibleOnMap = value;
				OnPropertyChanged(value, "IsVisibleOnMap");
			}
		}
	}

	public bool IsMainParty
	{
		get
		{
			return _isMainParty;
		}
		set
		{
			if (_isMainParty != value)
			{
				_isMainParty = value;
				OnPropertyChanged(value, "IsMainParty");
			}
		}
	}

	public bool IsInside
	{
		get
		{
			return _isInside;
		}
		set
		{
			if (_isInside != value)
			{
				_isInside = value;
				OnPropertyChanged(value, "IsInside");
			}
		}
	}

	public bool IsHigh
	{
		get
		{
			return _isHigh;
		}
		set
		{
			if (_isHigh != value)
			{
				_isHigh = value;
				OnPropertyChanged(value, "IsHigh");
			}
		}
	}

	public bool IsBehind
	{
		get
		{
			return _isBehind;
		}
		set
		{
			if (_isBehind != value)
			{
				_isBehind = value;
				OnPropertyChanged(value, "IsBehind");
			}
		}
	}

	public bool IsPrisoner
	{
		get
		{
			return _isPrisoner;
		}
		set
		{
			if (_isPrisoner != value)
			{
				_isPrisoner = value;
				OnPropertyChanged(value, "IsPrisoner");
			}
		}
	}

	public TextWidget NameplateTextWidget
	{
		get
		{
			return _nameplateTextWidget;
		}
		set
		{
			if (_nameplateTextWidget != value)
			{
				_nameplateTextWidget = value;
				OnPropertyChanged(value, "NameplateTextWidget");
			}
		}
	}

	public TextWidget NameplateExtraInfoTextWidget
	{
		get
		{
			return _nameplateExtraInfoTextWidget;
		}
		set
		{
			if (_nameplateExtraInfoTextWidget != value)
			{
				_nameplateExtraInfoTextWidget = value;
				OnPropertyChanged(value, "NameplateExtraInfoTextWidget");
			}
		}
	}

	public TextWidget NameplateFullNameTextWidget
	{
		get
		{
			return _nameplateFullNameTextWidget;
		}
		set
		{
			if (_nameplateFullNameTextWidget != value)
			{
				_nameplateFullNameTextWidget = value;
				OnPropertyChanged(value, "NameplateFullNameTextWidget");
			}
		}
	}

	public TextWidget SpeedTextWidget
	{
		get
		{
			return _speedTextWidget;
		}
		set
		{
			if (_speedTextWidget != value)
			{
				_speedTextWidget = value;
				OnPropertyChanged(value, "SpeedTextWidget");
			}
		}
	}

	public Widget SpeedIconWidget
	{
		get
		{
			return _speedIconWidget;
		}
		set
		{
			if (value != _speedIconWidget)
			{
				_speedIconWidget = value;
				OnPropertyChanged(value, "SpeedIconWidget");
			}
		}
	}

	public Widget MainPartyArrowWidget
	{
		get
		{
			return _mainPartyArrowWidget;
		}
		set
		{
			if (_mainPartyArrowWidget != value)
			{
				_mainPartyArrowWidget = value;
				OnPropertyChanged(value, "MainPartyArrowWidget");
			}
		}
	}

	public PartyNameplateWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (_isFirstFrame)
		{
			NameplateFullNameTextWidget.Brush.GlobalAlphaFactor = 0f;
			NameplateTextWidget.Brush.GlobalAlphaFactor = 0f;
			NameplateExtraInfoTextWidget.Brush.GlobalAlphaFactor = 0f;
			PartyBannerWidget.Brush.GlobalAlphaFactor = 0f;
			SpeedTextWidget.AlphaFactor = 0f;
			_defaultNameplateFontSize = NameplateTextWidget.ReadOnlyBrush.FontSize;
			_isFirstFrame = false;
		}
		int num = (IsArmy ? (_defaultNameplateFontSize + _armyFontSizeOffset) : _defaultNameplateFontSize);
		if (NameplateTextWidget.Brush.FontSize != num)
		{
			NameplateTextWidget.Brush.FontSize = num;
		}
		UpdateNameplatesScreenPosition();
		UpdateNameplatesVisibility(dt);
		UpdateTutorialStatus();
	}

	private void UpdateNameplatesVisibility(float dt)
	{
		float num = 0f;
		float end = 0f;
		if (IsMainParty)
		{
			_latestIsOutside = IsNameplateOutsideScreen();
			MainPartyArrowWidget.IsVisible = _latestIsOutside;
			NameplateTextWidget.IsVisible = !_latestIsOutside && !IsInArmy && !IsPrisoner && !IsInSettlement;
			NameplateFullNameTextWidget.IsVisible = !_latestIsOutside && !IsInArmy && !IsPrisoner && !IsInSettlement;
			SpeedTextWidget.IsVisible = !_latestIsOutside && !IsInArmy && !IsPrisoner && !IsInSettlement;
			SpeedIconWidget.IsVisible = !_latestIsOutside && !IsInArmy && !IsPrisoner && !IsInSettlement;
			TrackerFrame.IsVisible = _latestIsOutside;
			num = ((!_latestIsOutside && !IsInArmy && !IsPrisoner && !IsInSettlement) ? 1 : 0);
			PartyBannerWidget.IsVisible = !_latestIsOutside && !IsInArmy && !IsPrisoner && !IsInSettlement;
			NameplateExtraInfoTextWidget.IsVisible = !_latestIsOutside && !IsInArmy && !IsPrisoner && !IsInSettlement;
			base.IsEnabled = _latestIsOutside;
		}
		else
		{
			MainPartyArrowWidget.IsVisible = false;
			NameplateTextWidget.IsVisible = true;
			NameplateFullNameTextWidget.IsVisible = true;
			SpeedTextWidget.IsVisible = true;
			SpeedIconWidget.IsVisible = true;
			TrackerFrame.IsVisible = false;
			PartyBannerWidget.IsVisible = true;
			num = 1f;
			base.IsEnabled = false;
		}
		if (!IsVisibleOnMap && !IsMainParty)
		{
			NameplateTextWidget.IsVisible = false;
			NameplateFullNameTextWidget.IsVisible = false;
			SpeedTextWidget.IsVisible = false;
			SpeedIconWidget.IsVisible = false;
			num = 0f;
		}
		else
		{
			_initialDelayAmount -= dt;
			end = ((!(_initialDelayAmount <= 0f)) ? 1f : ((float)(ShouldShowFullName ? 1 : 0)));
		}
		NameplateTextWidget.Brush.GlobalAlphaFactor = LocalLerp(NameplateTextWidget.ReadOnlyBrush.GlobalAlphaFactor, num, dt * _animSpeedModifier);
		NameplateFullNameTextWidget.Brush.GlobalAlphaFactor = LocalLerp(NameplateFullNameTextWidget.ReadOnlyBrush.GlobalAlphaFactor, end, dt * _animSpeedModifier);
		SpeedTextWidget.Brush.GlobalAlphaFactor = LocalLerp(SpeedTextWidget.ReadOnlyBrush.GlobalAlphaFactor, end, dt * _animSpeedModifier);
		SpeedIconWidget.AlphaFactor = LocalLerp(SpeedIconWidget.AlphaFactor, end, dt * _animSpeedModifier);
		NameplateExtraInfoTextWidget.Brush.GlobalAlphaFactor = LocalLerp(NameplateExtraInfoTextWidget.ReadOnlyBrush.GlobalAlphaFactor, ShouldShowFullName ? 1 : 0, dt * _animSpeedModifier);
		PartyBannerWidget.Brush.GlobalAlphaFactor = LocalLerp(PartyBannerWidget.ReadOnlyBrush.GlobalAlphaFactor, num, dt * _animSpeedModifier);
	}

	private void UpdateNameplatesScreenPosition()
	{
		_screenWidth = base.Context.EventManager.PageSize.X;
		_screenHeight = base.Context.EventManager.PageSize.Y;
		if (!IsVisibleOnMap && !IsMainParty)
		{
			base.IsHidden = true;
			return;
		}
		if (IsMainParty)
		{
			if (!IsBehind && !(Position.x + base.Size.X > _screenWidth) && !(Position.y - base.Size.Y > _screenHeight) && !(Position.x < 0f) && !(Position.y < 0f))
			{
				float num = HeadGroupWidget?.Size.Y ?? 0f;
				NameplateLayoutListPanel.ScaledPositionYOffset = Position.y - HeadPosition.y + num;
				if (IsHigh)
				{
					base.ScaledPositionXOffset = TaleWorlds.Library.MathF.Clamp(HeadPosition.x - base.Size.X / 2f, 0f, _screenWidth - base.Size.X);
				}
				else
				{
					base.ScaledPositionXOffset = TaleWorlds.Library.MathF.Clamp(HeadPosition.x - base.Size.X / 2f, 0f, _screenWidth - base.Size.X);
				}
				base.ScaledPositionYOffset = HeadPosition.y - num;
			}
			else
			{
				Vec2 vec = new Vec2(base.Context.EventManager.PageSize.X / 2f, base.Context.EventManager.PageSize.Y / 2f);
				Vec2 headPosition = HeadPosition;
				headPosition -= vec;
				if (IsBehind)
				{
					headPosition *= -1f;
				}
				float radian = Mathf.Atan2(headPosition.y, headPosition.x) - (float)Math.PI / 2f;
				float num2 = Mathf.Cos(radian);
				float num3 = Mathf.Sin(radian);
				headPosition = vec + new Vec2(num3 * 150f, num2 * 150f);
				float num4 = num2 / num3;
				Vec2 vec2 = vec * 1f;
				headPosition = ((num2 > 0f) ? new Vec2((0f - vec2.y) / num4, vec.y) : new Vec2(vec2.y / num4, 0f - vec.y));
				if (headPosition.x > vec2.x)
				{
					headPosition = new Vec2(vec2.x, (0f - vec2.x) * num4);
				}
				else if (headPosition.x < 0f - vec2.x)
				{
					headPosition = new Vec2(0f - vec2.x, vec2.x * num4);
				}
				headPosition += vec;
				base.ScaledPositionXOffset = TaleWorlds.Library.MathF.Clamp(headPosition.x - base.Size.X / 2f, 0f, _screenWidth - base.Size.X);
				base.ScaledPositionYOffset = TaleWorlds.Library.MathF.Clamp(headPosition.y - base.Size.Y / 2f, 0f, _screenHeight - base.Size.Y);
			}
		}
		else
		{
			float num5 = HeadGroupWidget?.Size.Y ?? 0f;
			NameplateLayoutListPanel.ScaledPositionYOffset = Position.y - HeadPosition.y + num5;
			base.ScaledPositionXOffset = HeadPosition.x - base.Size.X / 2f;
			base.ScaledPositionYOffset = HeadPosition.y - num5;
			base.IsHidden = base.ScaledPositionXOffset > base.Context.TwoDimensionContext.Width || base.ScaledPositionYOffset > base.Context.TwoDimensionContext.Height || base.ScaledPositionXOffset + base.Size.X < 0f || base.ScaledPositionYOffset + base.Size.Y < 0f;
		}
		NameplateLayoutListPanel.PositionXOffset = (base.Size.X / 2f - PartyBannerWidget.Size.X) * base._inverseScaleToUse;
	}

	private void UpdateTutorialStatus()
	{
		if (_tutorialAnimState == TutorialAnimState.Start)
		{
			_tutorialAnimState = TutorialAnimState.FirstFrame;
		}
		else
		{
			_ = _tutorialAnimState;
			_ = 2;
		}
		if (IsTargetedByTutorial)
		{
			SetState("Default");
		}
		else
		{
			SetState("Disabled");
		}
	}

	private bool IsNameplateOutsideScreen()
	{
		if (!(Position.x + base.Size.X > _screenWidth) && !(Position.y - base.Size.Y > _screenHeight) && !(Position.x < 0f) && !(Position.y < 0f) && !IsBehind)
		{
			return IsHigh;
		}
		return true;
	}

	private float LocalLerp(float start, float end, float delta)
	{
		if (Math.Abs(start - end) > float.Epsilon)
		{
			return (end - start) * delta + start;
		}
		return end;
	}
}
