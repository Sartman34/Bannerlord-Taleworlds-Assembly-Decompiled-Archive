using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Mission.OrderOfBattle;

public class OrderOfBattleScreenWidget : Widget
{
	private float _alphaChangeTimeElapsed;

	private float _initialAlpha = 1f;

	private float _targetAlpha;

	private float _currentAlpha = 1f;

	private bool _isTransitioning;

	private bool _areCameraControlsEnabled;

	private float _cameraEnabledAlpha = 0.2f;

	private ListPanel _leftSideFormations;

	private ListPanel _rightSideFormations;

	private ListPanel _commanderPool;

	private Widget _markers;

	public float AlphaChangeDuration { get; set; } = 0.15f;


	[Editor(false)]
	public bool AreCameraControlsEnabled
	{
		get
		{
			return _areCameraControlsEnabled;
		}
		set
		{
			if (value != _areCameraControlsEnabled)
			{
				_areCameraControlsEnabled = value;
				OnPropertyChanged(value, "AreCameraControlsEnabled");
				OnCameraControlsEnabledChanged();
			}
		}
	}

	[Editor(false)]
	public float CameraEnabledAlpha
	{
		get
		{
			return _cameraEnabledAlpha;
		}
		set
		{
			if (value != _cameraEnabledAlpha)
			{
				_cameraEnabledAlpha = value;
				OnPropertyChanged(value, "CameraEnabledAlpha");
			}
		}
	}

	[Editor(false)]
	public ListPanel LeftSideFormations
	{
		get
		{
			return _leftSideFormations;
		}
		set
		{
			if (value != _leftSideFormations)
			{
				_leftSideFormations = value;
				OnPropertyChanged(value, "LeftSideFormations");
			}
		}
	}

	[Editor(false)]
	public ListPanel RightSideFormations
	{
		get
		{
			return _rightSideFormations;
		}
		set
		{
			if (value != _rightSideFormations)
			{
				_rightSideFormations = value;
				OnPropertyChanged(value, "RightSideFormations");
			}
		}
	}

	[Editor(false)]
	public ListPanel CommanderPool
	{
		get
		{
			return _commanderPool;
		}
		set
		{
			if (value != _commanderPool)
			{
				_commanderPool = value;
				OnPropertyChanged(value, "CommanderPool");
			}
		}
	}

	[Editor(false)]
	public Widget Markers
	{
		get
		{
			return _markers;
		}
		set
		{
			if (value != _markers)
			{
				_markers = value;
				OnPropertyChanged(value, "Markers");
			}
		}
	}

	public OrderOfBattleScreenWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		if (_isTransitioning)
		{
			if (_alphaChangeTimeElapsed < AlphaChangeDuration)
			{
				_currentAlpha = MathF.Lerp(_initialAlpha, _targetAlpha, _alphaChangeTimeElapsed / AlphaChangeDuration);
				LeftSideFormations?.SetGlobalAlphaRecursively(_currentAlpha);
				RightSideFormations?.SetGlobalAlphaRecursively(_currentAlpha);
				CommanderPool?.SetGlobalAlphaRecursively(_currentAlpha);
				Markers?.SetGlobalAlphaRecursively(_currentAlpha);
				_alphaChangeTimeElapsed += dt;
			}
			else
			{
				_isTransitioning = false;
			}
		}
	}

	protected void OnCameraControlsEnabledChanged()
	{
		_alphaChangeTimeElapsed = 0f;
		_targetAlpha = (AreCameraControlsEnabled ? CameraEnabledAlpha : 1f);
		_initialAlpha = _currentAlpha;
		_isTransitioning = true;
	}
}
