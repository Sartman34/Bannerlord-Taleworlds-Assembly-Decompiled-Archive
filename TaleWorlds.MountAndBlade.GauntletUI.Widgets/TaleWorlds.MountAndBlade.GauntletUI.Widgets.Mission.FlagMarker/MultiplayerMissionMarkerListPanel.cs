using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Mission.FlagMarker;

public class MultiplayerMissionMarkerListPanel : ListPanel
{
	public enum MissionMarkerType
	{
		Flag,
		Peer,
		SiegeEngine
	}

	private const int FlagMarkerEdgeMargin = 10;

	private MissionMarkerType _activeMarkerType;

	private Widget _activeWidget;

	private bool _initialized;

	private int _distance;

	private Widget _flagWidget;

	private Widget _peerWidget;

	private Widget _siegeEngineWidget;

	private Widget _spawnFlagIconWidget;

	private Vec2 _position;

	private bool _isMarkerEnabled;

	private bool _isSpawnFlag;

	private int _markerType;

	private Widget _removalTimeVisiblityWidget;

	public float FarAlphaTarget { get; set; } = 0.2f;


	public float FarDistanceCutoff { get; set; } = 50f;


	public float CloseDistanceCutoff { get; set; } = 25f;


	public Widget FlagWidget
	{
		get
		{
			return _flagWidget;
		}
		set
		{
			if (_flagWidget != value)
			{
				_flagWidget = value;
				OnPropertyChanged(value, "FlagWidget");
				MarkerTypeUpdated();
			}
		}
	}

	public Widget RemovalTimeVisiblityWidget
	{
		get
		{
			return _removalTimeVisiblityWidget;
		}
		set
		{
			if (_removalTimeVisiblityWidget != value)
			{
				_removalTimeVisiblityWidget = value;
				OnPropertyChanged(value, "RemovalTimeVisiblityWidget");
			}
		}
	}

	public Widget SpawnFlagIconWidget
	{
		get
		{
			return _spawnFlagIconWidget;
		}
		set
		{
			if (_spawnFlagIconWidget != value)
			{
				_spawnFlagIconWidget = value;
				OnPropertyChanged(value, "SpawnFlagIconWidget");
			}
		}
	}

	public Widget PeerWidget
	{
		get
		{
			return _peerWidget;
		}
		set
		{
			if (_peerWidget != value)
			{
				_peerWidget = value;
				OnPropertyChanged(value, "PeerWidget");
				MarkerTypeUpdated();
			}
		}
	}

	public Widget SiegeEngineWidget
	{
		get
		{
			return _siegeEngineWidget;
		}
		set
		{
			if (value != _siegeEngineWidget)
			{
				_siegeEngineWidget = value;
				OnPropertyChanged(value, "SiegeEngineWidget");
				MarkerTypeUpdated();
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

	public int Distance
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

	public bool IsSpawnFlag
	{
		get
		{
			return _isSpawnFlag;
		}
		set
		{
			if (_isSpawnFlag != value)
			{
				_isSpawnFlag = value;
				OnPropertyChanged(value, "IsSpawnFlag");
			}
		}
	}

	public int MarkerType
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
				MarkerTypeUpdated();
			}
		}
	}

	public MultiplayerMissionMarkerListPanel(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		float delta = TaleWorlds.Library.MathF.Clamp(dt * 12f, 0f, 1f);
		if (!_initialized)
		{
			SetInitialAlphaValuesOnCreation();
			_initialized = true;
		}
		if (IsMarkerEnabled)
		{
			foreach (Widget allChildrenAndThi in base.AllChildrenAndThis)
			{
				if (allChildrenAndThi != this && allChildrenAndThi != _activeWidget)
				{
					Widget activeWidget = _activeWidget;
					if (activeWidget == null || !activeWidget.CheckIsMyChildRecursive(allChildrenAndThi))
					{
						if (allChildrenAndThi != RemovalTimeVisiblityWidget)
						{
							allChildrenAndThi.IsVisible = false;
						}
						continue;
					}
				}
				float distanceRelatedAlphaTarget = GetDistanceRelatedAlphaTarget(Distance);
				if (allChildrenAndThi == SpawnFlagIconWidget)
				{
					allChildrenAndThi.SetAlpha(IsSpawnFlag ? LocalLerp(allChildrenAndThi.AlphaFactor, distanceRelatedAlphaTarget, delta) : 0f);
				}
				else
				{
					allChildrenAndThi.SetAlpha(LocalLerp(allChildrenAndThi.AlphaFactor, distanceRelatedAlphaTarget, delta));
				}
				if (allChildrenAndThi != RemovalTimeVisiblityWidget)
				{
					allChildrenAndThi.IsVisible = (double)allChildrenAndThi.AlphaFactor > 0.05;
				}
			}
		}
		else
		{
			foreach (Widget allChildrenAndThi2 in base.AllChildrenAndThis)
			{
				if (allChildrenAndThi2 != this && allChildrenAndThi2 != _activeWidget)
				{
					Widget activeWidget2 = _activeWidget;
					if (activeWidget2 == null || !activeWidget2.CheckIsMyChildRecursive(allChildrenAndThi2))
					{
						if (allChildrenAndThi2 != RemovalTimeVisiblityWidget)
						{
							allChildrenAndThi2.IsVisible = false;
						}
						continue;
					}
				}
				if (allChildrenAndThi2 == SpawnFlagIconWidget)
				{
					allChildrenAndThi2.SetAlpha(IsSpawnFlag ? LocalLerp(allChildrenAndThi2.AlphaFactor, 0f, delta) : 0f);
				}
				else
				{
					allChildrenAndThi2.SetAlpha(LocalLerp(allChildrenAndThi2.AlphaFactor, 0f, delta));
				}
				if (allChildrenAndThi2 != RemovalTimeVisiblityWidget)
				{
					allChildrenAndThi2.IsVisible = (double)allChildrenAndThi2.AlphaFactor > 0.05;
				}
			}
		}
		Widget activeWidget3 = _activeWidget;
		if (activeWidget3 != null && activeWidget3.IsVisible)
		{
			if (_activeMarkerType == MissionMarkerType.Flag)
			{
				float x = base.Context.EventManager.PageSize.X;
				float y = base.Context.EventManager.PageSize.Y;
				base.ScaledPositionXOffset = TaleWorlds.Library.MathF.Clamp(Position.x - base.Size.X / 2f, 10f, x - base.Size.X - 10f);
				base.ScaledPositionYOffset = TaleWorlds.Library.MathF.Clamp(Position.y - base.Size.Y / 2f, 10f, y - base.Size.Y - 10f);
			}
			else
			{
				base.ScaledPositionYOffset = Position.y - base.Size.Y / 2f;
				base.ScaledPositionXOffset = Position.x - base.Size.X / 2f;
			}
		}
	}

	private float GetDistanceRelatedAlphaTarget(int distance)
	{
		if ((float)distance > FarDistanceCutoff)
		{
			return FarAlphaTarget;
		}
		if ((float)distance <= FarDistanceCutoff && (float)distance >= CloseDistanceCutoff)
		{
			float amount = (float)Math.Pow(((float)distance - CloseDistanceCutoff) / (FarDistanceCutoff - CloseDistanceCutoff), 1.0 / 3.0);
			return TaleWorlds.Library.MathF.Clamp(TaleWorlds.Library.MathF.Lerp(1f, FarAlphaTarget, amount), FarAlphaTarget, 1f);
		}
		return 1f;
	}

	private void SetInitialAlphaValuesOnCreation()
	{
		if (IsMarkerEnabled)
		{
			foreach (Widget allChildrenAndThi in base.AllChildrenAndThis)
			{
				if (allChildrenAndThi != this && allChildrenAndThi != _activeWidget)
				{
					Widget activeWidget = _activeWidget;
					if (activeWidget == null || !activeWidget.CheckIsMyChildRecursive(allChildrenAndThi))
					{
						if (allChildrenAndThi != RemovalTimeVisiblityWidget)
						{
							allChildrenAndThi.IsVisible = false;
						}
						continue;
					}
				}
				if (allChildrenAndThi == SpawnFlagIconWidget)
				{
					allChildrenAndThi.SetAlpha(IsSpawnFlag ? 1 : 0);
				}
				else
				{
					allChildrenAndThi.SetAlpha(1f);
				}
				if (allChildrenAndThi != RemovalTimeVisiblityWidget)
				{
					allChildrenAndThi.IsVisible = (double)allChildrenAndThi.AlphaFactor > 0.05;
				}
			}
			return;
		}
		foreach (Widget allChildrenAndThi2 in base.AllChildrenAndThis)
		{
			if (allChildrenAndThi2 != this && allChildrenAndThi2 != _activeWidget)
			{
				Widget activeWidget2 = _activeWidget;
				if (activeWidget2 == null || !activeWidget2.CheckIsMyChildRecursive(allChildrenAndThi2))
				{
					if (allChildrenAndThi2 != RemovalTimeVisiblityWidget)
					{
						allChildrenAndThi2.IsVisible = false;
					}
					continue;
				}
			}
			allChildrenAndThi2.SetAlpha(0f);
			if (allChildrenAndThi2 != RemovalTimeVisiblityWidget)
			{
				allChildrenAndThi2.IsVisible = false;
			}
		}
	}

	private float LocalLerp(float start, float end, float delta)
	{
		if (Math.Abs(start - end) > float.Epsilon)
		{
			return (end - start) * delta + start;
		}
		return end;
	}

	private void MarkerTypeUpdated()
	{
		_activeMarkerType = (MissionMarkerType)MarkerType;
		switch (_activeMarkerType)
		{
		case MissionMarkerType.Flag:
			_activeWidget = FlagWidget;
			break;
		case MissionMarkerType.Peer:
			_activeWidget = PeerWidget;
			break;
		case MissionMarkerType.SiegeEngine:
			_activeWidget = SiegeEngineWidget;
			break;
		}
	}
}
