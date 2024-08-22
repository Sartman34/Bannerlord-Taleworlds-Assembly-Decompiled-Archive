using System.Numerics;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.GamepadNavigation;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public class GamepadCursorWidget : BrushWidget
{
	private Widget _targetWidget;

	private bool _targetChangedThisFrame;

	private bool _targetPositionChangedThisFrame;

	private float _animationRatio;

	private float _animationRatioTimer;

	protected bool _isPressing;

	protected bool _areBrushesValidated;

	protected float _additionalOffset;

	protected float _additionalOffsetBeforeStateChange;

	protected float _leftOffset;

	protected float _rightOffset;

	protected float _topOffset;

	protected float _bottomOffset;

	private GamepadCursorParentWidget _cursorParentWidget;

	private GamepadCursorMarkerWidget _topLeftMarker;

	private GamepadCursorMarkerWidget _topRightMarker;

	private GamepadCursorMarkerWidget _bottomLeftMarker;

	private GamepadCursorMarkerWidget _bottomRightMarker;

	private bool _hasTarget;

	private bool _targetHasAction;

	private float _targetX;

	private float _targetY;

	private float _targetWidth;

	private float _targetHeight;

	private float _defaultOffset;

	private float _hoverOffset;

	private float _defaultTargetlessOffset;

	private float _pressOffset;

	private float _actionAnimationTime;

	protected float TransitionTimer { get; private set; }

	public GamepadCursorParentWidget CursorParentWidget
	{
		get
		{
			return _cursorParentWidget;
		}
		set
		{
			if (value != _cursorParentWidget)
			{
				_cursorParentWidget = value;
				OnPropertyChanged(value, "CursorParentWidget");
			}
		}
	}

	public GamepadCursorMarkerWidget TopLeftMarker
	{
		get
		{
			return _topLeftMarker;
		}
		set
		{
			if (value != _topLeftMarker)
			{
				_topLeftMarker = value;
				OnPropertyChanged(value, "TopLeftMarker");
			}
		}
	}

	public GamepadCursorMarkerWidget TopRightMarker
	{
		get
		{
			return _topRightMarker;
		}
		set
		{
			if (value != _topRightMarker)
			{
				_topRightMarker = value;
				OnPropertyChanged(value, "TopRightMarker");
			}
		}
	}

	public GamepadCursorMarkerWidget BottomLeftMarker
	{
		get
		{
			return _bottomLeftMarker;
		}
		set
		{
			if (value != _bottomLeftMarker)
			{
				_bottomLeftMarker = value;
				OnPropertyChanged(value, "BottomLeftMarker");
			}
		}
	}

	public GamepadCursorMarkerWidget BottomRightMarker
	{
		get
		{
			return _bottomRightMarker;
		}
		set
		{
			if (value != _bottomRightMarker)
			{
				_bottomRightMarker = value;
				OnPropertyChanged(value, "BottomRightMarker");
			}
		}
	}

	public bool HasTarget
	{
		get
		{
			return _hasTarget;
		}
		set
		{
			if (value != _hasTarget)
			{
				_hasTarget = value;
				OnPropertyChanged(value, "HasTarget");
				ResetAnimations();
				_animationRatioTimer = 0f;
			}
		}
	}

	public bool TargetHasAction
	{
		get
		{
			return _targetHasAction;
		}
		set
		{
			if (value != _targetHasAction)
			{
				_targetHasAction = value;
				OnPropertyChanged(value, "TargetHasAction");
				ResetAnimations();
			}
		}
	}

	public float TargetX
	{
		get
		{
			return _targetX;
		}
		set
		{
			if (value != _targetX)
			{
				_targetX = value;
				OnPropertyChanged(value, "TargetX");
				ResetAnimations();
				_targetPositionChangedThisFrame = true;
			}
		}
	}

	public float TargetY
	{
		get
		{
			return _targetY;
		}
		set
		{
			if (value != _targetY)
			{
				_targetY = value;
				OnPropertyChanged(value, "TargetY");
				ResetAnimations();
				_targetPositionChangedThisFrame = true;
			}
		}
	}

	public float TargetWidth
	{
		get
		{
			return _targetWidth;
		}
		set
		{
			if (value != _targetWidth)
			{
				_targetWidth = value;
				OnPropertyChanged(value, "TargetWidth");
				ResetAnimations();
			}
		}
	}

	public float TargetHeight
	{
		get
		{
			return _targetHeight;
		}
		set
		{
			if (value != _targetHeight)
			{
				_targetHeight = value;
				OnPropertyChanged(value, "TargetHeight");
				ResetAnimations();
			}
		}
	}

	public float DefaultOffset
	{
		get
		{
			return _defaultOffset;
		}
		set
		{
			if (value != _defaultOffset)
			{
				_defaultOffset = value;
				OnPropertyChanged(value, "DefaultOffset");
				ResetAnimations();
			}
		}
	}

	public float HoverOffset
	{
		get
		{
			return _hoverOffset;
		}
		set
		{
			if (value != _hoverOffset)
			{
				_hoverOffset = value;
				OnPropertyChanged(value, "HoverOffset");
				ResetAnimations();
			}
		}
	}

	public float DefaultTargetlessOffset
	{
		get
		{
			return _defaultTargetlessOffset;
		}
		set
		{
			if (value != _defaultTargetlessOffset)
			{
				_defaultTargetlessOffset = value;
				OnPropertyChanged(value, "DefaultTargetlessOffset");
				ResetAnimations();
			}
		}
	}

	public float PressOffset
	{
		get
		{
			return _pressOffset;
		}
		set
		{
			if (value != _pressOffset)
			{
				_pressOffset = value;
				OnPropertyChanged(value, "PressOffset");
				ResetAnimations();
			}
		}
	}

	public float ActionAnimationTime
	{
		get
		{
			return _actionAnimationTime;
		}
		set
		{
			if (value != _actionAnimationTime)
			{
				_actionAnimationTime = value;
				OnPropertyChanged(value, "ActionAnimationTime");
				ResetAnimations();
			}
		}
	}

	public GamepadCursorWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		if (base.IsVisible)
		{
			RefreshTarget();
			bool flag = Input.IsKeyDown(InputKey.ControllerRDown);
			if (flag != _isPressing)
			{
				_animationRatioTimer = 0f;
				TransitionTimer = 0f;
				_additionalOffsetBeforeStateChange = _additionalOffset;
			}
			_isPressing = flag;
			if (_animationRatioTimer < 1.4f)
			{
				_animationRatioTimer = MathF.Min(_animationRatioTimer + dt, 1.4f);
			}
			bool flag2 = !_targetChangedThisFrame && _targetPositionChangedThisFrame;
			_animationRatio = ((HasTarget && !_isPressing) ? MathF.Clamp(17f * dt, 0f, 1f) : MathF.Lerp(_animationRatio, 1f, _animationRatioTimer / 1.4f));
			UpdateAdditionalOffsets();
			UpdateTargetOffsets(flag2 ? 1f : _animationRatio);
			if (!flag2)
			{
				TransitionTimer += dt;
			}
		}
		_targetChangedThisFrame = false;
		_targetPositionChangedThisFrame = false;
	}

	private void RefreshTarget()
	{
		Widget widget = GauntletGamepadNavigationManager.Instance?.LastTargetedWidget;
		_targetChangedThisFrame = _targetWidget != widget;
		_targetWidget = widget;
		TargetHasAction = GauntletGamepadNavigationManager.Instance.TargetedWidgetHasAction;
		HasTarget = _targetWidget != null;
		CursorParentWidget.HasTarget = HasTarget;
		if (HasTarget)
		{
			Vector2 globalPosition = _targetWidget.GlobalPosition;
			Vector2 size = _targetWidget.Size;
			float num = (_targetWidget.DoNotUseCustomScale ? _targetWidget.EventManager.Context.Scale : _targetWidget.EventManager.Context.CustomScale);
			float num2 = _targetWidget.ExtendCursorAreaLeft * num;
			float num3 = _targetWidget.ExtendCursorAreaTop * num;
			float num4 = _targetWidget.ExtendCursorAreaRight * num;
			float num5 = _targetWidget.ExtendCursorAreaBottom * num;
			TargetX = globalPosition.X - num2;
			TargetY = globalPosition.Y - num3;
			TargetWidth = size.X + num2 + num4;
			TargetHeight = size.Y + num3 + num5;
		}
	}

	private void UpdateTargetOffsets(float ratio)
	{
		Vector2 vector = new Vector2(base.EventManager.LeftUsableAreaStart, base.EventManager.TopUsableAreaStart);
		float num;
		float num2;
		float num3;
		float num4;
		if (HasTarget)
		{
			num = TargetX - vector.X;
			num2 = TargetY - vector.Y;
			num3 = TargetWidth;
			num4 = TargetHeight;
		}
		else
		{
			num = CursorParentWidget.XOffset - (float)MathF.Floor(DefaultTargetlessOffset / 2f);
			num2 = CursorParentWidget.YOffset - (float)MathF.Floor(DefaultTargetlessOffset / 2f);
			num3 = DefaultTargetlessOffset;
			num4 = DefaultTargetlessOffset;
		}
		num -= _additionalOffset;
		num2 -= _additionalOffset;
		float num5 = 45f * base._scaleToUse;
		if (num3 < num5)
		{
			float num6 = num5;
			num += (num3 - num6) / 2f;
			num3 = num6;
		}
		if (num4 < num5)
		{
			float num7 = num5;
			num2 += (num4 - num7) / 2f;
			num4 = num7;
		}
		num3 += _additionalOffset * 2f;
		num4 += _additionalOffset * 2f;
		base.ScaledSuggestedWidth = MathF.Lerp(base.ScaledSuggestedWidth, num3, ratio);
		base.ScaledSuggestedHeight = MathF.Lerp(base.ScaledSuggestedHeight, num4, ratio);
		if (GauntletGamepadNavigationManager.Instance.IsCursorMovingForNavigation)
		{
			Vector2 vector2 = CursorParentWidget.CenterWidget.GlobalPosition + CursorParentWidget.CenterWidget.Size / 2f;
			base.ScaledPositionXOffset = vector2.X - base.ScaledSuggestedWidth / 2f - vector.X;
			base.ScaledPositionYOffset = vector2.Y - base.ScaledSuggestedHeight / 2f - vector.Y;
		}
		else
		{
			base.ScaledPositionXOffset = MathF.Lerp(base.ScaledPositionXOffset, num, ratio);
			base.ScaledPositionYOffset = MathF.Lerp(base.ScaledPositionYOffset, num2, ratio);
		}
		base.ScaledPositionXOffset = MathF.Clamp(base.ScaledPositionXOffset, 0f, Input.Resolution.X - num3 - vector.X * 2f);
		base.ScaledPositionYOffset = MathF.Clamp(base.ScaledPositionYOffset, 0f, Input.Resolution.Y - num4 - vector.Y * 2f);
		base.ScaledSuggestedWidth = MathF.Min(base.ScaledSuggestedWidth, Input.Resolution.X - base.ScaledPositionXOffset - vector.X * 2f);
		base.ScaledSuggestedHeight = MathF.Min(base.ScaledSuggestedHeight, Input.Resolution.Y - base.ScaledPositionYOffset - vector.Y * 2f);
	}

	private void UpdateAdditionalOffsets()
	{
		float num;
		if (TargetHasAction && !_isPressing)
		{
			float amount = (MathF.Sin(TransitionTimer / ActionAnimationTime * 1.6f) + 1f) / 2f;
			num = MathF.Lerp(DefaultOffset, HoverOffset, amount) - DefaultOffset;
		}
		else
		{
			num = 0f;
		}
		float num2 = (_isPressing ? (HasTarget ? PressOffset : (DefaultTargetlessOffset * 0.7f)) : ((!HasTarget) ? DefaultTargetlessOffset : DefaultOffset));
		_additionalOffset = (num2 + num) * base._scaleToUse;
	}

	private void ResetAnimations()
	{
		if (!_isPressing)
		{
			TransitionTimer = 0f;
			_additionalOffsetBeforeStateChange = _additionalOffset;
		}
	}
}
