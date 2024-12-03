using System;
using System.Numerics;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Map;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Nameplate;

public class SettlementNameplateWidget : Widget, IComparable<SettlementNameplateWidget>
{
	public enum TutorialAnimState
	{
		Idle,
		Start,
		FirstFrame,
		Playing
	}

	private float _positionTimer;

	private SettlementNameplateItemWidget _currentNameplate;

	private bool _updatePositionNextFrame;

	private TutorialAnimState _tutorialAnimState;

	private float _lerpThreshold = 5E-05f;

	private float _lerpModifier = 10f;

	private Vector2 _cachedItemSize;

	private bool _lateUpdateActionAdded;

	private Vec2 _position;

	private bool _isVisibleOnMap;

	private bool _isTracked;

	private bool _isInsideWindow;

	private bool _isTargetedByTutorial;

	private int _nameplateType = -1;

	private int _relationType = -1;

	private int _wSign;

	private float _wPos;

	private float _distanceToCamera;

	private bool _isInRange;

	private SettlementNameplateItemWidget _smallNameplateWidget;

	private SettlementNameplateItemWidget _normalNameplateWidget;

	private SettlementNameplateItemWidget _bigNameplateWidget;

	private ListPanel _notificationListPanel;

	private ListPanel _eventsListPanel;

	private float _screenEdgeAlphaTarget => 1f;

	private float _normalNeutralAlphaTarget => 0.35f;

	private float _normalAllyAlphaTarget => 0.5f;

	private float _normalEnemyAlphaTarget => 0.35f;

	private float _trackedAlphaTarget => 0.8f;

	private float _trackedColorFactorTarget => 1.3f;

	private float _normalColorFactorTarget => 1f;

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
				if (_isVisibleOnMap && !value)
				{
					_positionTimer = 0f;
				}
				_isVisibleOnMap = value;
				OnPropertyChanged(value, "IsVisibleOnMap");
			}
		}
	}

	public bool IsTracked
	{
		get
		{
			return _isTracked;
		}
		set
		{
			if (_isTracked != value)
			{
				_isTracked = value;
				OnPropertyChanged(value, "IsTracked");
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
				if (value)
				{
					_tutorialAnimState = TutorialAnimState.Start;
				}
			}
		}
	}

	public bool IsInsideWindow
	{
		get
		{
			return _isInsideWindow;
		}
		set
		{
			if (_isInsideWindow != value)
			{
				_isInsideWindow = value;
				OnPropertyChanged(value, "IsInsideWindow");
			}
		}
	}

	public bool IsInRange
	{
		get
		{
			return _isInRange;
		}
		set
		{
			if (_isInRange != value)
			{
				_isInRange = value;
			}
		}
	}

	public int NameplateType
	{
		get
		{
			return _nameplateType;
		}
		set
		{
			if (_nameplateType != value)
			{
				_nameplateType = value;
				OnPropertyChanged(value, "NameplateType");
				SetNameplateTypeVisual(value);
			}
		}
	}

	public int RelationType
	{
		get
		{
			return _relationType;
		}
		set
		{
			if (_relationType != value)
			{
				_relationType = value;
				OnPropertyChanged(value, "RelationType");
				SetNameplateRelationType(value);
			}
		}
	}

	public int WSign
	{
		get
		{
			return _wSign;
		}
		set
		{
			if (_wSign != value)
			{
				_wSign = value;
				OnPropertyChanged(value, "WSign");
			}
		}
	}

	public float WPos
	{
		get
		{
			return _wPos;
		}
		set
		{
			if (_wPos != value)
			{
				_wPos = value;
				OnPropertyChanged(value, "WPos");
			}
		}
	}

	public float DistanceToCamera
	{
		get
		{
			return _distanceToCamera;
		}
		set
		{
			if (_distanceToCamera != value)
			{
				_distanceToCamera = value;
				OnPropertyChanged(value, "DistanceToCamera");
			}
		}
	}

	public ListPanel NotificationListPanel
	{
		get
		{
			return _notificationListPanel;
		}
		set
		{
			if (_notificationListPanel != value)
			{
				_notificationListPanel = value;
				OnPropertyChanged(value, "NotificationListPanel");
				_notificationListPanel.ItemAddEventHandlers.Add(OnNotificationListUpdated);
				_notificationListPanel.ItemAfterRemoveEventHandlers.Add(OnNotificationListUpdated);
			}
		}
	}

	public ListPanel EventsListPanel
	{
		get
		{
			return _eventsListPanel;
		}
		set
		{
			if (value != _eventsListPanel)
			{
				_eventsListPanel = value;
				OnPropertyChanged(value, "EventsListPanel");
			}
		}
	}

	public SettlementNameplateItemWidget SmallNameplateWidget
	{
		get
		{
			return _smallNameplateWidget;
		}
		set
		{
			if (_smallNameplateWidget != value)
			{
				_smallNameplateWidget = value;
				OnPropertyChanged(value, "SmallNameplateWidget");
			}
		}
	}

	public SettlementNameplateItemWidget NormalNameplateWidget
	{
		get
		{
			return _normalNameplateWidget;
		}
		set
		{
			if (_normalNameplateWidget != value)
			{
				_normalNameplateWidget = value;
				OnPropertyChanged(value, "NormalNameplateWidget");
			}
		}
	}

	public SettlementNameplateItemWidget BigNameplateWidget
	{
		get
		{
			return _bigNameplateWidget;
		}
		set
		{
			if (_bigNameplateWidget != value)
			{
				_bigNameplateWidget = value;
				OnPropertyChanged(value, "BigNameplateWidget");
			}
		}
	}

	public SettlementNameplateWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnParallelUpdate(float dt)
	{
		base.OnParallelUpdate(dt);
		SettlementNameplateItemWidget currentNameplate = _currentNameplate;
		currentNameplate?.ParallelUpdate(dt);
		if (currentNameplate != null && _cachedItemSize != currentNameplate.Size)
		{
			_cachedItemSize = currentNameplate.Size;
			ListPanel eventsListPanel = _eventsListPanel;
			ListPanel notificationListPanel = _notificationListPanel;
			if (eventsListPanel != null)
			{
				eventsListPanel.ScaledPositionXOffset = _cachedItemSize.X;
			}
			if (notificationListPanel != null)
			{
				notificationListPanel.ScaledPositionYOffset = 0f - _cachedItemSize.Y;
			}
			base.SuggestedWidth = _cachedItemSize.X * base._inverseScaleToUse;
			base.SuggestedHeight = _cachedItemSize.Y * base._inverseScaleToUse;
			base.ScaledSuggestedWidth = _cachedItemSize.X;
			base.ScaledSuggestedHeight = _cachedItemSize.Y;
		}
		base.IsEnabled = IsVisibleOnMap;
		UpdateNameplateTransparencyAndBrightness(dt);
		UpdatePosition(dt);
		UpdateTutorialState();
	}

	private void UpdatePosition(float dt)
	{
		SettlementNameplateItemWidget currentNameplate = _currentNameplate;
		MapEventVisualBrushWidget mapEventVisualBrushWidget = currentNameplate?.MapEventVisualWidget;
		if (currentNameplate == null || mapEventVisualBrushWidget == null)
		{
			Debug.FailedAssert("Related widget null on UpdatePosition!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Nameplate\\SettlementNameplateWidget.cs", "UpdatePosition", 105);
			return;
		}
		bool flag = false;
		_positionTimer += dt;
		if (IsVisibleOnMap || _positionTimer < 2f)
		{
			float x = base.Context.EventManager.PageSize.X;
			float y = base.Context.EventManager.PageSize.Y;
			Vec2 position = Position;
			if (IsTracked)
			{
				if (WSign > 0 && position.x - base.Size.X / 2f > 0f && position.x + base.Size.X / 2f < x && position.y > 0f && position.y + base.Size.Y < y)
				{
					base.ScaledPositionXOffset = position.x - base.Size.X / 2f;
					base.ScaledPositionYOffset = position.y - base.Size.Y;
				}
				else
				{
					Vec2 vec = new Vec2(x / 2f, y / 2f);
					position -= vec;
					if (WSign < 0)
					{
						position *= -1f;
					}
					float radian = Mathf.Atan2(position.y, position.x) - System.MathF.PI / 2f;
					float num = Mathf.Cos(radian);
					float num2 = Mathf.Sin(radian);
					float num3 = num / num2;
					Vec2 vec2 = vec * 1f;
					position = ((num > 0f) ? new Vec2((0f - vec2.y) / num3, vec.y) : new Vec2(vec2.y / num3, 0f - vec.y));
					if (position.x > vec2.x)
					{
						position = new Vec2(vec2.x, (0f - vec2.x) * num3);
					}
					else if (position.x < 0f - vec2.x)
					{
						position = new Vec2(0f - vec2.x, vec2.x * num3);
					}
					position += vec;
					flag = position.y - base.Size.Y - mapEventVisualBrushWidget.Size.Y <= 0f;
					base.ScaledPositionXOffset = Mathf.Clamp(position.x - base.Size.X / 2f, 0f, x - currentNameplate.Size.X);
					base.ScaledPositionYOffset = Mathf.Clamp(position.y - base.Size.Y, 0f, y - (currentNameplate.Size.Y + 55f));
				}
			}
			else
			{
				base.ScaledPositionXOffset = position.x - base.Size.X / 2f;
				base.ScaledPositionYOffset = position.y - base.Size.Y;
			}
		}
		if (flag)
		{
			mapEventVisualBrushWidget.VerticalAlignment = VerticalAlignment.Bottom;
			mapEventVisualBrushWidget.ScaledPositionYOffset = mapEventVisualBrushWidget.Size.Y;
		}
		else
		{
			mapEventVisualBrushWidget.VerticalAlignment = VerticalAlignment.Top;
			mapEventVisualBrushWidget.ScaledPositionYOffset = 0f - mapEventVisualBrushWidget.Size.Y;
		}
	}

	private void OnNotificationListUpdated(Widget widget)
	{
		_updatePositionNextFrame = true;
		AddLateUpdateAction();
	}

	private void OnNotificationListUpdated(Widget parentWidget, Widget addedWidget)
	{
		_updatePositionNextFrame = true;
		AddLateUpdateAction();
	}

	private void AddLateUpdateAction()
	{
		if (!_lateUpdateActionAdded)
		{
			base.EventManager.AddLateUpdateAction(this, CustomLateUpdate, 1);
			_lateUpdateActionAdded = true;
		}
	}

	private void CustomLateUpdate(float dt)
	{
		if (_updatePositionNextFrame)
		{
			UpdatePosition(dt);
			_updatePositionNextFrame = false;
		}
		_lateUpdateActionAdded = false;
	}

	private void UpdateTutorialState()
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

	private void SetNameplateTypeVisual(int type)
	{
		if (_currentNameplate == null)
		{
			SmallNameplateWidget.IsVisible = false;
			NormalNameplateWidget.IsVisible = false;
			BigNameplateWidget.IsVisible = false;
			switch (type)
			{
			case 0:
				_currentNameplate = SmallNameplateWidget;
				SmallNameplateWidget.IsVisible = true;
				break;
			case 1:
				_currentNameplate = NormalNameplateWidget;
				NormalNameplateWidget.IsVisible = true;
				break;
			case 2:
				_currentNameplate = BigNameplateWidget;
				BigNameplateWidget.IsVisible = true;
				break;
			}
		}
	}

	private void SetNameplateRelationType(int type)
	{
		if (_currentNameplate != null)
		{
			switch (type)
			{
			case 0:
				_currentNameplate.Color = Color.Black;
				break;
			case 1:
				_currentNameplate.Color = Color.ConvertStringToColor("#245E05FF");
				break;
			case 2:
				_currentNameplate.Color = Color.ConvertStringToColor("#870707FF");
				break;
			}
		}
	}

	private void UpdateNameplateTransparencyAndBrightness(float dt)
	{
		SettlementNameplateItemWidget currentNameplate = _currentNameplate;
		TextWidget textWidget = currentNameplate?.SettlementNameTextWidget;
		MaskedTextureWidget maskedTextureWidget = currentNameplate?.SettlementBannerWidget;
		GridWidget gridWidget = currentNameplate?.SettlementPartiesGridWidget;
		Widget widget = currentNameplate?.SettlementNameplateInspectedWidget;
		ListPanel eventsListPanel = _eventsListPanel;
		if (currentNameplate == null || textWidget == null || maskedTextureWidget == null || gridWidget == null || widget == null || eventsListPanel == null)
		{
			Debug.FailedAssert("Related widget null on UpdateNameplateTransparencyAndBrightness!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Nameplate\\SettlementNameplateWidget.cs", "UpdateNameplateTransparencyAndBrightness", 342);
			return;
		}
		float amount = dt * _lerpModifier;
		if (IsVisibleOnMap)
		{
			base.IsVisible = true;
			float valueTo = DetermineTargetAlphaValue();
			float valueTo2 = DetermineTargetColorFactor();
			float alphaFactor = TaleWorlds.Library.MathF.Lerp(currentNameplate.AlphaFactor, valueTo, amount);
			float colorFactor = TaleWorlds.Library.MathF.Lerp(currentNameplate.ColorFactor, valueTo2, amount);
			float num = TaleWorlds.Library.MathF.Lerp(textWidget.ReadOnlyBrush.GlobalAlphaFactor, 1f, amount);
			currentNameplate.AlphaFactor = alphaFactor;
			currentNameplate.ColorFactor = colorFactor;
			textWidget.Brush.GlobalAlphaFactor = num;
			maskedTextureWidget.Brush.GlobalAlphaFactor = num;
			gridWidget.SetGlobalAlphaRecursively(num);
			eventsListPanel.SetGlobalAlphaRecursively(num);
		}
		else if (currentNameplate.AlphaFactor > _lerpThreshold)
		{
			float num3 = (currentNameplate.AlphaFactor = TaleWorlds.Library.MathF.Lerp(currentNameplate.AlphaFactor, 0f, amount));
			textWidget.Brush.GlobalAlphaFactor = num3;
			maskedTextureWidget.Brush.GlobalAlphaFactor = num3;
			gridWidget.SetGlobalAlphaRecursively(num3);
			eventsListPanel.SetGlobalAlphaRecursively(num3);
		}
		else
		{
			base.IsVisible = false;
		}
		if (IsInRange && IsVisibleOnMap)
		{
			if (Math.Abs(widget.AlphaFactor - 1f) > _lerpThreshold)
			{
				widget.AlphaFactor = TaleWorlds.Library.MathF.Lerp(widget.AlphaFactor, 1f, amount);
			}
		}
		else if (currentNameplate.AlphaFactor - 0f > _lerpThreshold)
		{
			widget.AlphaFactor = TaleWorlds.Library.MathF.Lerp(widget.AlphaFactor, 0f, amount);
		}
	}

	private float DetermineTargetAlphaValue()
	{
		if (IsInsideWindow)
		{
			if (IsTracked)
			{
				return _trackedAlphaTarget;
			}
			if (RelationType == 0)
			{
				return _normalNeutralAlphaTarget;
			}
			if (RelationType == 1)
			{
				return _normalAllyAlphaTarget;
			}
			return _normalEnemyAlphaTarget;
		}
		if (IsTracked)
		{
			return _screenEdgeAlphaTarget;
		}
		return 0f;
	}

	private float DetermineTargetColorFactor()
	{
		if (IsTracked)
		{
			return _trackedColorFactorTarget;
		}
		return _normalColorFactorTarget;
	}

	public int CompareTo(SettlementNameplateWidget other)
	{
		return other.DistanceToCamera.CompareTo(DistanceToCamera);
	}
}
