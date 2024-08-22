using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.HUD.FormationMarker;

public class MissionFormationMarkerVM : ViewModel
{
	public class FormationMarkerDistanceComparer : IComparer<MissionFormationMarkerTargetVM>
	{
		public int Compare(MissionFormationMarkerTargetVM x, MissionFormationMarkerTargetVM y)
		{
			return y.Distance.CompareTo(x.Distance);
		}
	}

	private readonly Mission _mission;

	private readonly Camera _missionCamera;

	private readonly FormationMarkerDistanceComparer _comparer;

	private readonly Vec3 _heightOffset = new Vec3(0f, 0f, 3f);

	private bool _prevIsEnabled;

	private bool _fadeOutTimerStarted;

	private float _fadeOutTimer;

	private MBReadOnlyList<Formation> _focusedFormations;

	private bool _isEnabled;

	private bool _isFormationTargetRelevant;

	private MBBindingList<MissionFormationMarkerTargetVM> _targets;

	[DataSourceProperty]
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value != _isEnabled)
			{
				_isEnabled = value;
				OnPropertyChangedWithValue(value, "IsEnabled");
				for (int i = 0; i < Targets.Count; i++)
				{
					Targets[i].IsEnabled = value;
				}
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
			if (value != _isFormationTargetRelevant)
			{
				_isFormationTargetRelevant = value;
				OnPropertyChangedWithValue(value, "IsFormationTargetRelevant");
				for (int i = 0; i < Targets.Count; i++)
				{
					Targets[i].IsFormationTargetRelevant = value;
				}
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MissionFormationMarkerTargetVM> Targets
	{
		get
		{
			return _targets;
		}
		set
		{
			if (value != _targets)
			{
				_targets = value;
				OnPropertyChangedWithValue(value, "Targets");
			}
		}
	}

	public MissionFormationMarkerVM(Mission mission, Camera missionCamera)
	{
		_mission = mission;
		_missionCamera = missionCamera;
		_comparer = new FormationMarkerDistanceComparer();
		Targets = new MBBindingList<MissionFormationMarkerTargetVM>();
	}

	public void Tick(float dt)
	{
		if (IsEnabled)
		{
			RefreshFormationListInMission();
			RefreshFormationPositions();
			RefreshFormationItemProperties();
			SortMarkersInList();
			RefreshTargetProperties();
			_fadeOutTimerStarted = false;
			_fadeOutTimer = 0f;
			_prevIsEnabled = IsEnabled;
		}
		else
		{
			if (_prevIsEnabled)
			{
				_fadeOutTimerStarted = true;
			}
			if (_fadeOutTimerStarted)
			{
				_fadeOutTimer += dt;
			}
			if (_fadeOutTimer < 2f)
			{
				RefreshFormationPositions();
			}
			else
			{
				_fadeOutTimerStarted = false;
			}
		}
		_prevIsEnabled = IsEnabled;
	}

	private void RefreshFormationListInMission()
	{
		IEnumerable<Formation> formationList = _mission.Teams.SelectMany((Team t) => t.FormationsIncludingEmpty.WhereQ((Formation f) => f.CountOfUnits > 0));
		foreach (Formation formation in formationList)
		{
			if (Targets.All((MissionFormationMarkerTargetVM t) => t.Formation != formation))
			{
				MissionFormationMarkerTargetVM missionFormationMarkerTargetVM = new MissionFormationMarkerTargetVM(formation);
				Targets.Add(missionFormationMarkerTargetVM);
				missionFormationMarkerTargetVM.IsEnabled = IsEnabled;
				missionFormationMarkerTargetVM.IsFormationTargetRelevant = IsFormationTargetRelevant;
			}
		}
		if (formationList.CountQ() >= Targets.Count)
		{
			return;
		}
		foreach (MissionFormationMarkerTargetVM item in Targets.WhereQ((MissionFormationMarkerTargetVM t) => !formationList.Contains(t.Formation)).ToList())
		{
			Targets.Remove(item);
		}
	}

	private void RefreshFormationPositions()
	{
		for (int i = 0; i < Targets.Count; i++)
		{
			MissionFormationMarkerTargetVM missionFormationMarkerTargetVM = Targets[i];
			float screenX = 0f;
			float screenY = 0f;
			float w = 0f;
			WorldPosition medianPosition = missionFormationMarkerTargetVM.Formation.QuerySystem.MedianPosition;
			medianPosition.SetVec2(missionFormationMarkerTargetVM.Formation.QuerySystem.AveragePosition);
			if (medianPosition.IsValid)
			{
				MBWindowManager.WorldToScreen(_missionCamera, medianPosition.GetGroundVec3() + _heightOffset, ref screenX, ref screenY, ref w);
				missionFormationMarkerTargetVM.IsInsideScreenBoundaries = !(screenX > Screen.RealScreenResolutionWidth) && !(screenY > Screen.RealScreenResolutionHeight) && !(screenX + 200f < 0f) && !(screenY + 100f < 0f);
				missionFormationMarkerTargetVM.WSign = ((!(w < 0f)) ? 1 : (-1));
			}
			if (!missionFormationMarkerTargetVM.IsTargetingAFormation && (!medianPosition.IsValid || w < 0f || !MathF.IsValidValue(screenX) || !MathF.IsValidValue(screenY)))
			{
				screenX = -10000f;
				screenY = -10000f;
				w = 0f;
			}
			if (_prevIsEnabled && IsEnabled)
			{
				missionFormationMarkerTargetVM.ScreenPosition = Vec2.Lerp(missionFormationMarkerTargetVM.ScreenPosition, new Vec2(screenX, screenY), 0.9f);
			}
			else
			{
				missionFormationMarkerTargetVM.ScreenPosition = new Vec2(screenX, screenY);
			}
			Agent main = Agent.Main;
			missionFormationMarkerTargetVM.Distance = ((main != null && main.IsActive()) ? Agent.Main.Position.Distance(medianPosition.GetGroundVec3()) : w);
		}
	}

	private void RefreshTargetProperties()
	{
		List<Formation> list = new List<Formation>();
		MBReadOnlyList<Formation> mBReadOnlyList = Agent.Main?.Team.PlayerOrderController?.SelectedFormations;
		if (mBReadOnlyList != null)
		{
			for (int i = 0; i < mBReadOnlyList.Count; i++)
			{
				if (mBReadOnlyList[i].TargetFormation != null && OrderUIHelper.CanOrderHaveTarget(OrderUIHelper.GetActiveMovementOrderOfFormation(mBReadOnlyList[i])))
				{
					list.Add(mBReadOnlyList[i].TargetFormation);
				}
			}
		}
		for (int j = 0; j < Targets.Count; j++)
		{
			MissionFormationMarkerTargetVM missionFormationMarkerTargetVM = Targets[j];
			if (missionFormationMarkerTargetVM.TeamType == 2)
			{
				bool isTargetingAFormation = list.Contains(missionFormationMarkerTargetVM.Formation);
				missionFormationMarkerTargetVM.SetTargetedState(_focusedFormations?.Contains(missionFormationMarkerTargetVM.Formation) ?? false, isTargetingAFormation);
			}
		}
	}

	private void SortMarkersInList()
	{
		Targets.Sort(_comparer);
	}

	private void RefreshFormationItemProperties()
	{
		foreach (MissionFormationMarkerTargetVM target in Targets)
		{
			target.Refresh();
		}
	}

	private void UpdateTargetStates(bool isEnabled, bool isFormationTargetRelevant)
	{
		foreach (MissionFormationMarkerTargetVM target in Targets)
		{
			target.IsEnabled = isEnabled;
			target.IsFormationTargetRelevant = isFormationTargetRelevant;
		}
	}

	public void SetFocusedFormations(MBReadOnlyList<Formation> focusedFormations)
	{
		_focusedFormations = focusedFormations;
	}
}
