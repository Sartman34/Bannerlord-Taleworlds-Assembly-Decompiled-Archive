using System;
using System.Numerics;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Mission;

public class FormationMarkerListPanel : ListPanel
{
	private bool _isMarkersDirty = true;

	private float _distance;

	private TextWidget _nameTextWidget;

	private TextWidget _distanceTextWidget;

	private Vec2 _position;

	private bool _isMarkerEnabled;

	private bool _isTargetingAFormation;

	private bool _isInsideScreenBoundaries;

	private string _markerType;

	private int _teamType;

	private int _wSign;

	private Widget _formationTypeMarker;

	private Widget _teamTypeMarker;

	public float FarAlphaTarget { get; set; } = 0.2f;


	public float FarDistanceCutoff { get; set; } = 50f;


	public float CloseDistanceCutoff { get; set; } = 25f;


	public float ClosestFadeoutRange { get; set; } = 3f;


	[DataSourceProperty]
	public TextWidget NameTextWidget
	{
		get
		{
			return _nameTextWidget;
		}
		set
		{
			if (_nameTextWidget != value)
			{
				_nameTextWidget = value;
				OnPropertyChanged(value, "NameTextWidget");
			}
		}
	}

	[DataSourceProperty]
	public TextWidget DistanceTextWidget
	{
		get
		{
			return _distanceTextWidget;
		}
		set
		{
			if (_distanceTextWidget != value)
			{
				_distanceTextWidget = value;
				OnPropertyChanged(value, "DistanceTextWidget");
			}
		}
	}

	[DataSourceProperty]
	public Widget FormationTypeMarker
	{
		get
		{
			return _formationTypeMarker;
		}
		set
		{
			if (_formationTypeMarker != value)
			{
				_formationTypeMarker = value;
				OnPropertyChanged(value, "FormationTypeMarker");
				_isMarkersDirty = true;
			}
		}
	}

	[DataSourceProperty]
	public Widget TeamTypeMarker
	{
		get
		{
			return _teamTypeMarker;
		}
		set
		{
			if (_teamTypeMarker != value)
			{
				_teamTypeMarker = value;
				OnPropertyChanged(value, "TeamTypeMarker");
				_isMarkersDirty = true;
			}
		}
	}

	[DataSourceProperty]
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

	[DataSourceProperty]
	public float Distance
	{
		get
		{
			return _distance;
		}
		set
		{
			if (_distance != value)
			{
				_distance = value;
				OnPropertyChanged(value, "Distance");
			}
		}
	}

	[DataSourceProperty]
	public int TeamType
	{
		get
		{
			return _teamType;
		}
		set
		{
			if (_teamType != value)
			{
				_teamType = value;
				OnPropertyChanged(value, "TeamType");
				_isMarkersDirty = true;
			}
		}
	}

	[DataSourceProperty]
	public int WSign
	{
		get
		{
			return _wSign;
		}
		set
		{
			if (_teamType != value)
			{
				_wSign = value;
				OnPropertyChanged(value, "WSign");
			}
		}
	}

	[DataSourceProperty]
	public string MarkerType
	{
		get
		{
			return _markerType;
		}
		set
		{
			if (_markerType != value)
			{
				_markerType = value;
				OnPropertyChanged(value, "MarkerType");
				_isMarkersDirty = true;
			}
		}
	}

	[DataSourceProperty]
	public bool IsMarkerEnabled
	{
		get
		{
			return _isMarkerEnabled;
		}
		set
		{
			if (_isMarkerEnabled != value)
			{
				_isMarkerEnabled = value;
				OnPropertyChanged(value, "IsMarkerEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTargetingAFormation
	{
		get
		{
			return _isTargetingAFormation;
		}
		set
		{
			if (_isTargetingAFormation != value)
			{
				_isTargetingAFormation = value;
				OnPropertyChanged(value, "IsTargetingAFormation");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInsideScreenBoundaries
	{
		get
		{
			return _isInsideScreenBoundaries;
		}
		set
		{
			if (_isInsideScreenBoundaries != value)
			{
				_isInsideScreenBoundaries = value;
				OnPropertyChanged(value, "IsInsideScreenBoundaries");
			}
		}
	}

	public FormationMarkerListPanel(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		float delta = TaleWorlds.Library.MathF.Clamp(dt * 12f, 0f, 1f);
		if (_isMarkersDirty)
		{
			Sprite sprite = null;
			if (!string.IsNullOrEmpty(MarkerType))
			{
				sprite = base.Context.SpriteData.GetSprite("General\\compass\\" + MarkerType);
			}
			if (sprite != null && FormationTypeMarker != null)
			{
				FormationTypeMarker.Sprite = sprite;
			}
			else
			{
				Debug.FailedAssert("Couldn't find formation marker type image", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Mission\\FormationMarkerListPanel.cs", "OnLateUpdate", 48);
			}
			if (TeamTypeMarker != null)
			{
				TeamTypeMarker.RegisterBrushStatesOfWidget();
				if (TeamType == 0)
				{
					TeamTypeMarker.SetState("Player");
				}
				else if (TeamType == 1)
				{
					TeamTypeMarker.SetState("Ally");
				}
				else
				{
					TeamTypeMarker.SetState("Enemy");
				}
			}
			_isMarkersDirty = false;
		}
		if (IsMarkerEnabled)
		{
			float distanceRelatedAlphaTarget = GetDistanceRelatedAlphaTarget(Distance);
			this.SetGlobalAlphaRecursively(distanceRelatedAlphaTarget);
			base.IsVisible = (double)distanceRelatedAlphaTarget > 0.05;
		}
		else
		{
			float alphaFactor = LocalLerp(base.AlphaFactor, 0f, delta);
			this.SetGlobalAlphaRecursively(alphaFactor);
			base.IsVisible = (double)base.AlphaFactor > 0.05;
		}
		UpdateVisibility();
		if (base.IsVisible)
		{
			UpdateScreenPosition();
			DistanceTextWidget.Text = ((int)Distance).ToString();
		}
	}

	private void UpdateVisibility()
	{
		base.IsVisible = IsInsideScreenBoundaries || IsTargetingAFormation;
	}

	private void UpdateScreenPosition()
	{
		float num = Position.X - base.Size.X / 2f;
		float num2 = Position.Y - base.Size.Y / 2f;
		if (WSign > 0 && num - base.Size.X / 2f > 0f && num + base.Size.X / 2f < base.Context.EventManager.PageSize.X && num2 > 0f && num2 + base.Size.Y < base.Context.EventManager.PageSize.Y)
		{
			base.IsVisible = true;
			base.ScaledPositionXOffset = num;
			base.ScaledPositionYOffset = num2;
		}
		else if (IsTargetingAFormation)
		{
			base.IsVisible = true;
			Vec2 position = Position;
			Vector2 pageSize = base.Context.EventManager.PageSize;
			Vec2 vec = new Vec2(base.Context.EventManager.PageSize.X / 2f, base.Context.EventManager.PageSize.Y / 2f);
			position -= vec;
			if (WSign < 0)
			{
				position *= -1f;
			}
			float radian = Mathf.Atan2(position.y, position.x) - System.MathF.PI / 2f;
			float num3 = Mathf.Cos(radian);
			float num4 = Mathf.Sin(radian);
			float num5 = num3 / num4;
			Vec2 vec2 = vec * 1f;
			position = ((num3 > 0f) ? new Vec2((0f - vec2.y) / num5, vec.y) : new Vec2(vec2.y / num5, 0f - vec.y));
			if (position.x > vec2.x)
			{
				position = new Vec2(vec2.x, (0f - vec2.x) * num5);
			}
			else if (position.x < 0f - vec2.x)
			{
				position = new Vec2(0f - vec2.x, vec2.x * num5);
			}
			position += vec;
			base.ScaledPositionXOffset = Mathf.Clamp(position.x - base.Size.X / 2f, 0f, pageSize.X - base.Size.X);
			base.ScaledPositionYOffset = Mathf.Clamp(position.y - base.Size.Y, 0f, pageSize.Y - base.Size.Y);
		}
		else
		{
			base.IsVisible = false;
		}
	}

	private float GetDistanceRelatedAlphaTarget(float distance)
	{
		if (distance > FarDistanceCutoff)
		{
			return FarAlphaTarget;
		}
		if (distance <= FarDistanceCutoff && distance >= CloseDistanceCutoff)
		{
			float amount = (float)Math.Pow((distance - CloseDistanceCutoff) / (FarDistanceCutoff - CloseDistanceCutoff), 1.0 / 3.0);
			return TaleWorlds.Library.MathF.Clamp(TaleWorlds.Library.MathF.Lerp(1f, FarAlphaTarget, amount), FarAlphaTarget, 1f);
		}
		if (distance < CloseDistanceCutoff && distance > CloseDistanceCutoff - ClosestFadeoutRange)
		{
			float amount2 = (distance - (CloseDistanceCutoff - ClosestFadeoutRange)) / ClosestFadeoutRange;
			return TaleWorlds.Library.MathF.Lerp(0f, 1f, amount2);
		}
		return 0f;
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
