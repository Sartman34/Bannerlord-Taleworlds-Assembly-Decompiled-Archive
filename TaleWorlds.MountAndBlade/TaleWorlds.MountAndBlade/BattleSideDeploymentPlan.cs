using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class BattleSideDeploymentPlan
{
	public const float DeployZoneMinimumWidth = 100f;

	public const float DeployZoneForwardMargin = 10f;

	public const float DeployZoneExtraWidthPerTroop = 1.5f;

	public readonly BattleSideEnum Side;

	private readonly Mission _mission;

	private readonly DeploymentPlan _initialPlan;

	private bool _spawnWithHorses;

	private bool _reinforcementPlansCreated;

	private readonly List<DeploymentPlan> _reinforcementPlans;

	private DeploymentPlan _currentReinforcementPlan;

	private readonly MBList<(string id, List<Vec2> points)> _deploymentBoundaries = new MBList<(string, List<Vec2>)>();

	private MatrixFrame _deploymentFrame;

	public bool SpawnWithHorses => _spawnWithHorses;

	public MBReadOnlyList<(string id, List<Vec2> points)> DeploymentBoundaries => _deploymentBoundaries;

	public BattleSideDeploymentPlan(Mission mission, BattleSideEnum side)
	{
		_mission = mission;
		Side = side;
		_spawnWithHorses = false;
		_initialPlan = DeploymentPlan.CreateInitialPlan(_mission, side);
		_reinforcementPlans = new List<DeploymentPlan>();
		_reinforcementPlansCreated = false;
		_currentReinforcementPlan = _initialPlan;
		_deploymentBoundaries.Clear();
	}

	public void CreateReinforcementPlans()
	{
		if (_reinforcementPlansCreated)
		{
			return;
		}
		if (_mission.HasSpawnPath)
		{
			foreach (SpawnPathData item2 in _mission.GetReinforcementPathsDataOfSide(Side))
			{
				DeploymentPlan item = DeploymentPlan.CreateReinforcementPlanWithSpawnPath(_mission, Side, item2);
				_reinforcementPlans.Add(item);
			}
			_currentReinforcementPlan = _reinforcementPlans[0];
		}
		else
		{
			DeploymentPlan deploymentPlan = DeploymentPlan.CreateReinforcementPlan(_mission, Side);
			_reinforcementPlans.Add(deploymentPlan);
			_currentReinforcementPlan = deploymentPlan;
		}
		_reinforcementPlansCreated = true;
	}

	public void SetSpawnWithHorses(bool value)
	{
		_spawnWithHorses = value;
		_initialPlan.SetSpawnWithHorses(value);
		foreach (DeploymentPlan reinforcementPlan in _reinforcementPlans)
		{
			reinforcementPlan.SetSpawnWithHorses(value);
		}
	}

	public void PlanBattleDeployment(FormationSceneSpawnEntry[,] formationSceneSpawnEntries, DeploymentPlanType planType, float spawnPathOffset)
	{
		switch (planType)
		{
		case DeploymentPlanType.Initial:
			if (!_initialPlan.IsPlanMade)
			{
				_initialPlan.PlanBattleDeployment(formationSceneSpawnEntries, spawnPathOffset);
			}
			PlanDeploymentZone();
			break;
		case DeploymentPlanType.Reinforcement:
		{
			foreach (DeploymentPlan reinforcementPlan in _reinforcementPlans)
			{
				if (!reinforcementPlan.IsPlanMade)
				{
					reinforcementPlan.PlanBattleDeployment(formationSceneSpawnEntries);
				}
			}
			break;
		}
		}
	}

	public void UpdateReinforcementPlans()
	{
		if (!_reinforcementPlansCreated || _reinforcementPlans.Count <= 1)
		{
			return;
		}
		foreach (DeploymentPlan reinforcementPlan in _reinforcementPlans)
		{
			reinforcementPlan.UpdateSafetyScore();
		}
		if (!_currentReinforcementPlan.IsSafeToDeploy)
		{
			_currentReinforcementPlan = _reinforcementPlans.MaxBy((DeploymentPlan plan) => plan.SafetyScore);
		}
	}

	public void ClearPlans(DeploymentPlanType planType)
	{
		switch (planType)
		{
		case DeploymentPlanType.Initial:
			_initialPlan.ClearPlan();
			break;
		case DeploymentPlanType.Reinforcement:
		{
			foreach (DeploymentPlan reinforcementPlan in _reinforcementPlans)
			{
				reinforcementPlan.ClearPlan();
			}
			break;
		}
		}
	}

	public void ClearAddedTroops(DeploymentPlanType planType)
	{
		if (planType == DeploymentPlanType.Initial)
		{
			_initialPlan.ClearAddedTroops();
			return;
		}
		foreach (DeploymentPlan reinforcementPlan in _reinforcementPlans)
		{
			reinforcementPlan.ClearAddedTroops();
		}
	}

	public void AddTroops(FormationClass formationClass, int footTroopCount, int mountedTroopCount, DeploymentPlanType planType)
	{
		if (planType == DeploymentPlanType.Initial)
		{
			_initialPlan.AddTroops(formationClass, footTroopCount, mountedTroopCount);
			return;
		}
		foreach (DeploymentPlan reinforcementPlan in _reinforcementPlans)
		{
			reinforcementPlan.AddTroops(formationClass, footTroopCount, mountedTroopCount);
		}
	}

	public bool IsFirstPlan(DeploymentPlanType planType)
	{
		if (planType == DeploymentPlanType.Initial)
		{
			return _initialPlan.PlanCount == 1;
		}
		if (_reinforcementPlansCreated)
		{
			return _currentReinforcementPlan.PlanCount == 1;
		}
		return false;
	}

	public bool IsPlanMade(DeploymentPlanType planType)
	{
		if (planType == DeploymentPlanType.Initial)
		{
			return _initialPlan.IsPlanMade;
		}
		if (_reinforcementPlansCreated)
		{
			return _currentReinforcementPlan.IsPlanMade;
		}
		return false;
	}

	public float GetSpawnPathOffset(DeploymentPlanType planType)
	{
		if (planType == DeploymentPlanType.Initial)
		{
			return _initialPlan.SpawnPathOffset;
		}
		if (!_reinforcementPlansCreated)
		{
			return 0f;
		}
		return _currentReinforcementPlan.SpawnPathOffset;
	}

	public int GetTroopCount(DeploymentPlanType planType)
	{
		if (planType == DeploymentPlanType.Initial)
		{
			return _initialPlan.TroopCount;
		}
		if (!_reinforcementPlansCreated)
		{
			return 0;
		}
		return _currentReinforcementPlan.TroopCount;
	}

	public MatrixFrame GetDeploymentFrame()
	{
		return _deploymentFrame;
	}

	public bool HasDeploymentBoundaries()
	{
		return !_deploymentBoundaries.IsEmpty();
	}

	public IFormationDeploymentPlan GetFormationPlan(FormationClass fClass, DeploymentPlanType planType)
	{
		if (planType == DeploymentPlanType.Initial)
		{
			return _initialPlan.GetFormationPlan(fClass);
		}
		return _currentReinforcementPlan.GetFormationPlan(fClass);
	}

	public Vec3 GetMeanPositionOfPlan(DeploymentPlanType planType)
	{
		if (planType == DeploymentPlanType.Initial)
		{
			return _initialPlan.MeanPosition;
		}
		return _currentReinforcementPlan.MeanPosition;
	}

	public bool IsInitialPlanSuitableForFormations((int, int)[] troopDataPerFormationClass)
	{
		return _initialPlan.IsPlanSuitableForFormations(troopDataPerFormationClass);
	}

	public bool IsPositionInsideDeploymentBoundaries(in Vec2 position)
	{
		bool result = false;
		foreach (var deploymentBoundary in _deploymentBoundaries)
		{
			List<Vec2> item = deploymentBoundary.points;
			if (MBSceneUtilities.IsPointInsideBoundaries(in position, item))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public Vec2 GetClosestDeploymentBoundaryPosition(in Vec2 position, bool withNavMesh = false, float positionZ = 0f)
	{
		Vec2 result = position;
		float num = float.MaxValue;
		foreach (var deploymentBoundary in _deploymentBoundaries)
		{
			List<Vec2> item = deploymentBoundary.points;
			if (item.Count > 2)
			{
				Vec2 closestPoint;
				float num2 = ((!withNavMesh) ? MBSceneUtilities.FindClosestPointToBoundaries(in position, item, out closestPoint) : MBSceneUtilities.FindClosestPointWithNavMeshToBoundaries(in position, positionZ, item, out closestPoint));
				if (num2 < num)
				{
					num = num2;
					result = closestPoint;
				}
			}
		}
		return result;
	}

	private void PlanDeploymentZone()
	{
		if (_mission.HasSpawnPath || _mission.IsFieldBattle)
		{
			ComputeDeploymentZone();
		}
		else if (_mission.IsSiegeBattle)
		{
			SetDeploymentZoneFromMissionBoundaries();
		}
		else
		{
			_deploymentBoundaries.Clear();
		}
	}

	private void ComputeDeploymentZone()
	{
		_initialPlan.GetFormationDeploymentFrame(FormationClass.Infantry, out _deploymentFrame);
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < 10; i++)
		{
			FormationClass fClass = (FormationClass)i;
			FormationDeploymentPlan formationPlan = _initialPlan.GetFormationPlan(fClass);
			if (formationPlan.HasFrame())
			{
				MatrixFrame matrixFrame = _deploymentFrame.TransformToLocal(formationPlan.GetGroundFrame());
				num = Math.Max(matrixFrame.origin.y, num);
				num2 = Math.Max(Math.Abs(matrixFrame.origin.x), num2);
			}
		}
		num += 10f;
		_deploymentFrame.Advance(num);
		_deploymentBoundaries.Clear();
		float val = 2f * num2 + 1.5f * (float)_initialPlan.TroopCount;
		val = Math.Max(val, 100f);
		foreach (KeyValuePair<string, ICollection<Vec2>> boundary in _mission.Boundaries)
		{
			string key = boundary.Key;
			List<Vec2> item = ComputeDeploymentBoundariesFromMissionBoundaries(boundary.Value, ref _deploymentFrame, val);
			_deploymentBoundaries.Add((key, item));
		}
	}

	private void SetDeploymentZoneFromMissionBoundaries()
	{
		_deploymentBoundaries.Clear();
		foreach (var deploymentBoundary in MBSceneUtilities.GetDeploymentBoundaries(Side))
		{
			List<Vec2> boundary = new List<Vec2>(deploymentBoundary.boundaryPoints);
			MBSceneUtilities.RadialSortBoundary(ref boundary);
			MBSceneUtilities.FindConvexHull(ref boundary);
			_deploymentBoundaries.Add((deploymentBoundary.tag, boundary));
		}
	}

	private static List<Vec2> ComputeDeploymentBoundariesFromMissionBoundaries(ICollection<Vec2> missionBoundaries, ref MatrixFrame deploymentFrame, float desiredWidth)
	{
		List<Vec2> boundary = new List<Vec2>();
		float maxLength = desiredWidth / 2f;
		if (missionBoundaries.Count > 2)
		{
			Vec2 asVec = deploymentFrame.origin.AsVec2;
			Vec2 vec = deploymentFrame.rotation.s.AsVec2.Normalized();
			Vec2 vec2 = deploymentFrame.rotation.f.AsVec2.Normalized();
			List<Vec2> boundaries = missionBoundaries.ToList();
			List<(Vec2, Vec2)> list = new List<(Vec2, Vec2)>();
			Vec2 vec3 = ClampRayToMissionBoundaries(boundaries, asVec, vec, maxLength);
			AddDeploymentBoundaryPoint(boundary, vec3);
			Vec2 vec4 = ClampRayToMissionBoundaries(boundaries, asVec, -vec, maxLength);
			AddDeploymentBoundaryPoint(boundary, vec4);
			if (MBMath.IntersectRayWithBoundaryList(vec3, -vec2, boundaries, out var intersectionPoint) && (intersectionPoint - vec3).Length > 0.1f)
			{
				list.Add((intersectionPoint, vec3));
				AddDeploymentBoundaryPoint(boundary, intersectionPoint);
			}
			list.Add((vec3, vec4));
			if (MBMath.IntersectRayWithBoundaryList(vec4, -vec2, boundaries, out var intersectionPoint2) && (intersectionPoint2 - vec4).Length > 0.1f)
			{
				list.Add((vec4, intersectionPoint2));
				AddDeploymentBoundaryPoint(boundary, intersectionPoint2);
			}
			foreach (Vec2 missionBoundary in missionBoundaries)
			{
				bool flag = true;
				foreach (var item in list)
				{
					Vec2 vec5 = missionBoundary - item.Item1;
					Vec2 vec6 = item.Item2 - item.Item1;
					if (vec6.x * vec5.y - vec6.y * vec5.x <= 0f)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					AddDeploymentBoundaryPoint(boundary, missionBoundary);
				}
			}
			MBSceneUtilities.RadialSortBoundary(ref boundary);
		}
		return boundary;
	}

	private static void AddDeploymentBoundaryPoint(List<Vec2> deploymentBoundaries, Vec2 point)
	{
		if (!deploymentBoundaries.Exists((Vec2 boundaryPoint) => boundaryPoint.Distance(point) <= 0.1f))
		{
			deploymentBoundaries.Add(point);
		}
	}

	private static Vec2 ClampRayToMissionBoundaries(List<Vec2> boundaries, Vec2 origin, Vec2 direction, float maxLength)
	{
		if (Mission.Current.IsPositionInsideBoundaries(origin))
		{
			Vec2 vec = origin + direction * maxLength;
			if (Mission.Current.IsPositionInsideBoundaries(vec))
			{
				return vec;
			}
		}
		if (MBMath.IntersectRayWithBoundaryList(origin, direction, boundaries, out var intersectionPoint))
		{
			return intersectionPoint;
		}
		return origin;
	}
}
