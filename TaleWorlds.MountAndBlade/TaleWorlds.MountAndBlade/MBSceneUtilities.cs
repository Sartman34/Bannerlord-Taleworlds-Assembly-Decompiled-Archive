using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public static class MBSceneUtilities
{
	public const int MaxNumberOfSpawnPaths = 32;

	public const string SpawnPathPrefix = "spawn_path_";

	public const string SoftBorderVertexTag = "walk_area_vertex";

	public const string SoftBoundaryName = "walk_area";

	public const string SceneBoundaryName = "scene_boundary";

	private const string DeploymentBoundaryTag = "deployment_castle_boundary";

	private const string DeploymentBoundaryTagExpression = "deployment_castle_boundary(_\\d+)*";

	public static MBList<Path> GetAllSpawnPaths(Scene scene)
	{
		MBList<Path> mBList = new MBList<Path>();
		for (int i = 0; i < 32; i++)
		{
			string name = "spawn_path_" + i.ToString("D2");
			Path pathWithName = scene.GetPathWithName(name);
			if (pathWithName != null && pathWithName.NumberOfPoints > 1)
			{
				mBList.Add(pathWithName);
			}
		}
		return mBList;
	}

	public static List<Vec2> GetSceneBoundaryPoints(Scene scene, out string boundaryName)
	{
		List<Vec2> list = new List<Vec2>();
		int softBoundaryVertexCount = scene.GetSoftBoundaryVertexCount();
		if (softBoundaryVertexCount >= 3)
		{
			boundaryName = "walk_area";
			for (int i = 0; i < softBoundaryVertexCount; i++)
			{
				Vec2 softBoundaryVertex = scene.GetSoftBoundaryVertex(i);
				list.Add(softBoundaryVertex);
			}
		}
		else
		{
			boundaryName = "scene_boundary";
			scene.GetBoundingBox(out var min, out var max);
			float num = TaleWorlds.Library.MathF.Min(2f, max.x - min.x);
			float num2 = TaleWorlds.Library.MathF.Min(2f, max.y - min.y);
			List<Vec2> collection = new List<Vec2>
			{
				new Vec2(min.x + num, min.y + num2),
				new Vec2(max.x - num, min.y + num2),
				new Vec2(max.x - num, max.y - num2),
				new Vec2(min.x + num, max.y - num2)
			};
			list.AddRange(collection);
		}
		return list;
	}

	public static List<(string tag, List<Vec2> boundaryPoints, bool insideAllowance)> GetDeploymentBoundaries(BattleSideEnum battleSide)
	{
		IEnumerable<GameEntity> enumerable = Mission.Current.Scene.FindEntitiesWithTagExpression("deployment_castle_boundary(_\\d+)*");
		List<(string, List<GameEntity>)> list = new List<(string, List<GameEntity>)>();
		foreach (GameEntity item4 in enumerable)
		{
			if (!item4.HasTag(battleSide.ToString()))
			{
				continue;
			}
			string[] tags = item4.Tags;
			foreach (string tag in tags)
			{
				if (tag.Contains("deployment_castle_boundary"))
				{
					(string, List<GameEntity>) item = list.FirstOrDefault<(string, List<GameEntity>)>(((string tag, List<GameEntity> boundaryEntities) tuple) => tuple.tag.Equals(tag));
					if (item.Item1 == null)
					{
						item = (tag, new List<GameEntity>());
						list.Add(item);
					}
					item.Item2.Add(item4);
					break;
				}
			}
		}
		List<(string, List<Vec2>, bool)> list2 = new List<(string, List<Vec2>, bool)>();
		foreach (var item5 in list)
		{
			string item2 = item5.Item1;
			bool item3 = !item5.Item2.Any((GameEntity e) => e.HasTag("out"));
			List<Vec2> boundary = item5.Item2.Select((GameEntity bp) => bp.GlobalPosition.AsVec2).ToList();
			RadialSortBoundary(ref boundary);
			list2.Add((item2, boundary, item3));
		}
		return list2;
	}

	public static void ProjectPositionToDeploymentBoundaries(BattleSideEnum side, ref WorldPosition position)
	{
		Mission current = Mission.Current;
		IMissionDeploymentPlan deploymentPlan = current.DeploymentPlan;
		if (deploymentPlan.HasDeploymentBoundaries(side))
		{
			Vec2 position2 = position.AsVec2;
			if (!deploymentPlan.IsPositionInsideDeploymentBoundaries(side, in position2))
			{
				position2 = position.AsVec2;
				Vec2 closestDeploymentBoundaryPosition = deploymentPlan.GetClosestDeploymentBoundaryPosition(side, in position2, withNavMesh: true, position.GetGroundZ());
				position = new WorldPosition(current.Scene, new Vec3(closestDeploymentBoundaryPosition, position.GetGroundZ()));
			}
		}
	}

	public static void FindConvexHull(ref List<Vec2> boundary)
	{
		Vec2[] array = boundary.ToArray();
		int convexPointCount = 0;
		MBAPI.IMBMission.FindConvexHull(array, boundary.Count, ref convexPointCount);
		boundary = array.ToList();
		boundary.RemoveRange(convexPointCount, boundary.Count - convexPointCount);
	}

	public static void RadialSortBoundary(ref List<Vec2> boundary)
	{
		if (boundary.Count == 0)
		{
			return;
		}
		Vec2 boundaryCenter = Vec2.Zero;
		foreach (Vec2 item in boundary)
		{
			boundaryCenter += item;
		}
		boundaryCenter.x /= boundary.Count;
		boundaryCenter.y /= boundary.Count;
		boundary = boundary.OrderBy((Vec2 b) => (b - boundaryCenter).RotationInRadians).ToList();
	}

	public static bool IsPointInsideBoundaries(in Vec2 point, List<Vec2> boundaries, float acceptanceThreshold = 0.05f)
	{
		if (boundaries.Count <= 2)
		{
			return false;
		}
		acceptanceThreshold = TaleWorlds.Library.MathF.Max(0f, acceptanceThreshold);
		bool result = true;
		for (int i = 0; i < boundaries.Count; i++)
		{
			Vec2 vec = boundaries[i];
			Vec2 vec2 = boundaries[(i + 1) % boundaries.Count] - vec;
			Vec2 vec3 = point - vec;
			if (vec2.x * vec3.y - vec2.y * vec3.x < 0f)
			{
				vec2.Normalize();
				Vec2 vec4 = vec3.DotProduct(vec2) * vec2;
				if ((vec3 - vec4).LengthSquared > acceptanceThreshold * acceptanceThreshold)
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	public static float FindClosestPointToBoundaries(in Vec2 position, List<Vec2> boundaries, out Vec2 closestPoint)
	{
		closestPoint = position;
		float num = float.MaxValue;
		for (int i = 0; i < boundaries.Count; i++)
		{
			Vec2 segmentA = boundaries[i];
			Vec2 segmentB = boundaries[(i + 1) % boundaries.Count];
			Vec2 closest;
			float closestPointOnLineSegment = MBMath.GetClosestPointOnLineSegment(position, segmentA, segmentB, out closest);
			if (closestPointOnLineSegment <= num)
			{
				num = closestPointOnLineSegment;
				closestPoint = closest;
			}
		}
		return num;
	}

	public static float FindClosestPointWithNavMeshToBoundaries(in Vec2 position, float positionZ, List<Vec2> boundaries, out Vec2 closestPoint)
	{
		closestPoint = position;
		float num = float.MaxValue;
		for (int i = 0; i < boundaries.Count; i++)
		{
			Vec2 vec = boundaries[i];
			Vec2 vec2 = boundaries[(i + 1) % boundaries.Count];
			Vec2 closest;
			float num2 = MBMath.GetClosestPointOnLineSegment(position, vec, vec2, out closest);
			if (num2 > num)
			{
				continue;
			}
			Vec2 vec3 = (vec2 - vec).Normalized() * 1f;
			WorldPosition worldPosition = new WorldPosition(Mission.Current.Scene, new Vec3(closest, positionZ));
			int num3 = 0;
			while (worldPosition.GetNavMesh() == UIntPtr.Zero && num3 < 30)
			{
				Vec2 vec4 = (num3 / 2 + 1) * ((num3++ % 2 == 0) ? 1 : (-1)) * vec3;
				Vec2 vec5 = closest + vec4;
				if (vec5.X > TaleWorlds.Library.MathF.Min(vec.X, vec2.X) && vec5.X < TaleWorlds.Library.MathF.Max(vec.X, vec2.X) && vec5.Y > TaleWorlds.Library.MathF.Min(vec.Y, vec2.Y) && vec5.Y < TaleWorlds.Library.MathF.Max(vec.Y, vec2.Y))
				{
					worldPosition.SetVec2(closest + vec4);
				}
			}
			bool flag = worldPosition.GetNavMesh() != UIntPtr.Zero;
			if (flag)
			{
				num2 = worldPosition.AsVec2.Distance(position);
			}
			if (num2 <= num)
			{
				num = num2;
				closestPoint = (flag ? worldPosition.AsVec2 : closest);
			}
		}
		return num;
	}
}
