using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.HUD.FormationMarker;

public class MissionFormationMarkerTargetVM : ViewModel
{
	public enum TeamTypes
	{
		PlayerTeam,
		PlayerAllyTeam,
		EnemyTeam
	}

	private Vec2 _screenPosition;

	private float _distance;

	private bool _isEnabled;

	private bool _isInsideScreenBoundaries;

	private bool _isCenterOfFocus;

	private bool _isFormationTargetRelevant;

	private bool _isTargetingAFormation;

	private int _teamType;

	private int _size;

	private int _wSign;

	private string _formationType;

	public Formation Formation { get; private set; }

	[DataSourceProperty]
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (_isEnabled != value)
			{
				_isEnabled = value;
				OnPropertyChangedWithValue(value, "IsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCenterOfFocus
	{
		get
		{
			return _isCenterOfFocus;
		}
		set
		{
			if (_isCenterOfFocus != value)
			{
				_isCenterOfFocus = value;
				OnPropertyChangedWithValue(value, "IsCenterOfFocus");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFormationTargetRelevant
	{
		get
		{
			return _isFormationTargetRelevant;
		}
		set
		{
			if (_isFormationTargetRelevant != value)
			{
				_isFormationTargetRelevant = value;
				OnPropertyChangedWithValue(value, "IsFormationTargetRelevant");
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
				OnPropertyChangedWithValue(value, "IsTargetingAFormation");
			}
		}
	}

	[DataSourceProperty]
	public string FormationType
	{
		get
		{
			return _formationType;
		}
		set
		{
			if (_formationType != value)
			{
				_formationType = value;
				OnPropertyChangedWithValue(value, "FormationType");
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
				OnPropertyChangedWithValue(value, "TeamType");
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
				OnPropertyChangedWithValue(value, "IsInsideScreenBoundaries");
			}
		}
	}

	[DataSourceProperty]
	public Vec2 ScreenPosition
	{
		get
		{
			return _screenPosition;
		}
		set
		{
			if (value.x != _screenPosition.x || value.y != _screenPosition.y)
			{
				_screenPosition = value;
				OnPropertyChangedWithValue(value, "ScreenPosition");
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
			if (_distance != value && !float.IsNaN(value))
			{
				_distance = value;
				OnPropertyChangedWithValue(value, "Distance");
			}
		}
	}

	[DataSourceProperty]
	public int Size
	{
		get
		{
			return _size;
		}
		set
		{
			if (_size != value)
			{
				_size = value;
				OnPropertyChangedWithValue(value, "Size");
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
			if (_wSign != value)
			{
				_wSign = value;
				OnPropertyChangedWithValue(value, "WSign");
			}
		}
	}

	public MissionFormationMarkerTargetVM(Formation formation)
	{
		Formation = formation;
		TeamType = ((!Formation.Team.IsPlayerTeam) ? (Formation.Team.IsPlayerAlly ? 1 : 2) : 0);
		FormationType = GetFormationType(Formation.RepresentativeClass);
	}

	public void Refresh()
	{
		Size = Formation.CountOfUnits;
	}

	public void SetTargetedState(bool isFocused, bool isTargetingAFormation)
	{
		IsCenterOfFocus = isFocused;
		IsTargetingAFormation = isTargetingAFormation;
	}

	public static string GetFormationType(FormationClass formationType)
	{
		switch (formationType)
		{
		case FormationClass.Infantry:
			return "Infantry_Light";
		case FormationClass.Ranged:
			return "Archer_Light";
		case FormationClass.Cavalry:
			return "Cavalry_Light";
		case FormationClass.HorseArcher:
			return "HorseArcher_Light";
		case FormationClass.LightCavalry:
			return "Cavalry_Light";
		case FormationClass.HeavyCavalry:
			return "Cavalry_Heavy";
		case FormationClass.NumberOfDefaultFormations:
		case FormationClass.HeavyInfantry:
		case FormationClass.NumberOfRegularFormations:
		case FormationClass.Bodyguard:
		case FormationClass.NumberOfAllFormations:
			return "Infantry_Heavy";
		default:
			return "None";
		}
	}
}
