using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Mission;

public class FormationMarkerParentWidget : Widget
{
	private bool _isMarkersDirty = true;

	private string _markerType;

	private int _teamType;

	private Widget _formationTypeMarker;

	private Widget _teamTypeMarker;

	public float FarAlphaTarget { get; set; } = 0.2f;


	public float FarDistanceCutoff { get; set; } = 50f;


	public float CloseDistanceCutoff { get; set; } = 25f;


	public float ClosestFadeoutRange { get; set; } = 3f;


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

	public FormationMarkerParentWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		MathF.Clamp(dt * 12f, 0f, 1f);
		if (!_isMarkersDirty)
		{
			return;
		}
		Sprite sprite = null;
		if (!string.IsNullOrEmpty(MarkerType))
		{
			sprite = base.Context.SpriteData.GetSprite("General\\compass\\" + MarkerType);
		}
		if (sprite != null && FormationTypeMarker != null)
		{
			FormationTypeMarker.Sprite = sprite;
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
}
