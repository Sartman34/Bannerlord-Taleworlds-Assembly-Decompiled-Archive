using System.Collections.Generic;
using System.IO;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultMapDistanceModel : MapDistanceModel
{
	private readonly Dictionary<(Settlement, Settlement), float> _settlementDistanceCache = new Dictionary<(Settlement, Settlement), float>();

	private readonly Dictionary<int, Settlement> _navigationMeshClosestSettlementCache = new Dictionary<int, Settlement>();

	private readonly List<Settlement> _settlementsToConsider = new List<Settlement>();

	public override float MaximumDistanceBetweenTwoSettlements { get; set; }

	public void LoadCacheFromFile(System.IO.BinaryReader reader)
	{
		_settlementDistanceCache.Clear();
		if (reader == null)
		{
			for (int i = 0; i < Settlement.All.Count; i++)
			{
				Settlement settlement = Settlement.All[i];
				_settlementsToConsider.Add(settlement);
				for (int j = i + 1; j < Settlement.All.Count; j++)
				{
					Settlement settlement2 = Settlement.All[j];
					float distance = GetDistance(settlement.GatePosition, settlement2.GatePosition, settlement.CurrentNavigationFace, settlement2.CurrentNavigationFace);
					if (settlement.Id.InternalValue <= settlement2.Id.InternalValue)
					{
						AddNewPairToDistanceCache((settlement, settlement2), distance);
					}
					else
					{
						AddNewPairToDistanceCache((settlement2, settlement), distance);
					}
				}
			}
			int numberOfNavigationMeshFaces = Campaign.Current.MapSceneWrapper.GetNumberOfNavigationMeshFaces();
			for (int k = 0; k < numberOfNavigationMeshFaces; k++)
			{
				PathFaceRecord face = new PathFaceRecord(k, -1, -1);
				Vec2 navigationMeshCenterPosition = Campaign.Current.MapSceneWrapper.GetNavigationMeshCenterPosition(face);
				face = Campaign.Current.MapSceneWrapper.GetFaceIndex(navigationMeshCenterPosition);
				TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(face);
				if (faceTerrainType == TerrainType.Mountain || faceTerrainType == TerrainType.Lake || faceTerrainType == TerrainType.Water || faceTerrainType == TerrainType.River || faceTerrainType == TerrainType.Canyon || faceTerrainType == TerrainType.RuralArea)
				{
					continue;
				}
				float num = float.MaxValue;
				Settlement settlement3 = null;
				foreach (Settlement item in Settlement.All)
				{
					if ((settlement3 == null || navigationMeshCenterPosition.DistanceSquared(item.GatePosition) < num * num) && Campaign.Current.MapSceneWrapper.GetPathDistanceBetweenAIFaces(face, item.CurrentNavigationFace, navigationMeshCenterPosition, item.GatePosition, 0.1f, num, out var distance2) && distance2 < num)
					{
						num = distance2;
						settlement3 = item;
					}
				}
				if (settlement3 != null)
				{
					_navigationMeshClosestSettlementCache[k] = settlement3;
				}
			}
			return;
		}
		int num2 = reader.ReadInt32();
		for (int l = 0; l < num2; l++)
		{
			Settlement settlement4 = Settlement.Find(reader.ReadString());
			_settlementsToConsider.Add(settlement4);
			for (int m = l + 1; m < num2; m++)
			{
				Settlement settlement5 = Settlement.Find(reader.ReadString());
				float distance3 = reader.ReadSingle();
				if (settlement4.Id.InternalValue <= settlement5.Id.InternalValue)
				{
					AddNewPairToDistanceCache((settlement4, settlement5), distance3);
				}
				else
				{
					AddNewPairToDistanceCache((settlement5, settlement4), distance3);
				}
			}
		}
		for (int num3 = reader.ReadInt32(); num3 >= 0; num3 = reader.ReadInt32())
		{
			Settlement value = Settlement.Find(reader.ReadString());
			_navigationMeshClosestSettlementCache[num3] = value;
		}
	}

	public override float GetDistance(Settlement fromSettlement, Settlement toSettlement)
	{
		float value;
		if (fromSettlement == toSettlement)
		{
			value = 0f;
		}
		else if (fromSettlement.Id.InternalValue <= toSettlement.Id.InternalValue)
		{
			(Settlement, Settlement) tuple = (fromSettlement, toSettlement);
			if (!_settlementDistanceCache.TryGetValue(tuple, out value))
			{
				value = GetDistance(fromSettlement.GatePosition, toSettlement.GatePosition, fromSettlement.CurrentNavigationFace, toSettlement.CurrentNavigationFace);
				AddNewPairToDistanceCache(tuple, value);
			}
		}
		else
		{
			(Settlement, Settlement) tuple2 = (toSettlement, fromSettlement);
			if (!_settlementDistanceCache.TryGetValue(tuple2, out value))
			{
				value = GetDistance(toSettlement.GatePosition, fromSettlement.GatePosition, toSettlement.CurrentNavigationFace, fromSettlement.CurrentNavigationFace);
				AddNewPairToDistanceCache(tuple2, value);
			}
		}
		return value;
	}

	public override float GetDistance(MobileParty fromParty, Settlement toSettlement)
	{
		if (fromParty.CurrentSettlement != null)
		{
			return GetDistance(fromParty.CurrentSettlement, toSettlement);
		}
		if (fromParty.CurrentNavigationFace.FaceIndex == toSettlement.CurrentNavigationFace.FaceIndex)
		{
			return fromParty.Position2D.Distance(toSettlement.GatePosition);
		}
		Settlement closestSettlementForNavigationMesh = GetClosestSettlementForNavigationMesh(fromParty.CurrentNavigationFace);
		return fromParty.Position2D.Distance(toSettlement.GatePosition) - closestSettlementForNavigationMesh.GatePosition.Distance(toSettlement.GatePosition) + GetDistance(closestSettlementForNavigationMesh, toSettlement);
	}

	public override float GetDistance(MobileParty fromParty, MobileParty toParty)
	{
		if (fromParty.CurrentNavigationFace.FaceIndex == toParty.CurrentNavigationFace.FaceIndex)
		{
			return fromParty.Position2D.Distance(toParty.Position2D);
		}
		Settlement settlement = fromParty.CurrentSettlement ?? GetClosestSettlementForNavigationMesh(fromParty.CurrentNavigationFace);
		Settlement settlement2 = toParty.CurrentSettlement ?? GetClosestSettlementForNavigationMesh(toParty.CurrentNavigationFace);
		return fromParty.Position2D.Distance(toParty.Position2D) - settlement.GatePosition.Distance(settlement2.GatePosition) + GetDistance(settlement, settlement2);
	}

	public override bool GetDistance(Settlement fromSettlement, Settlement toSettlement, float maximumDistance, out float distance)
	{
		bool flag;
		if (fromSettlement == toSettlement)
		{
			distance = 0f;
			flag = true;
		}
		else if (fromSettlement.CurrentNavigationFace.FaceIndex == toSettlement.CurrentNavigationFace.FaceIndex)
		{
			distance = fromSettlement.GatePosition.Distance(toSettlement.GatePosition);
			flag = distance <= maximumDistance;
		}
		else if (fromSettlement.Id.InternalValue <= toSettlement.Id.InternalValue)
		{
			(Settlement, Settlement) tuple = (fromSettlement, toSettlement);
			if (_settlementDistanceCache.TryGetValue(tuple, out distance))
			{
				flag = distance <= maximumDistance;
			}
			else
			{
				flag = GetDistanceWithDistanceLimit(fromSettlement.GatePosition, toSettlement.GatePosition, Campaign.Current.MapSceneWrapper.GetFaceIndex(fromSettlement.GatePosition), Campaign.Current.MapSceneWrapper.GetFaceIndex(toSettlement.GatePosition), maximumDistance, out distance);
				if (flag)
				{
					AddNewPairToDistanceCache(tuple, distance);
				}
			}
		}
		else
		{
			(Settlement, Settlement) tuple2 = (toSettlement, fromSettlement);
			if (_settlementDistanceCache.TryGetValue(tuple2, out distance))
			{
				flag = distance <= maximumDistance;
			}
			else
			{
				flag = GetDistanceWithDistanceLimit(toSettlement.GatePosition, fromSettlement.GatePosition, Campaign.Current.MapSceneWrapper.GetFaceIndex(toSettlement.GatePosition), Campaign.Current.MapSceneWrapper.GetFaceIndex(fromSettlement.GatePosition), maximumDistance, out distance);
				if (flag)
				{
					AddNewPairToDistanceCache(tuple2, distance);
				}
			}
		}
		return flag;
	}

	public override bool GetDistance(MobileParty fromParty, Settlement toSettlement, float maximumDistance, out float distance)
	{
		bool result = false;
		if (fromParty.CurrentSettlement != null)
		{
			result = GetDistance(fromParty.CurrentSettlement, toSettlement, maximumDistance, out distance);
		}
		else if (fromParty.CurrentNavigationFace.FaceIndex == toSettlement.CurrentNavigationFace.FaceIndex)
		{
			distance = fromParty.Position2D.Distance(toSettlement.GatePosition);
			result = distance <= maximumDistance;
		}
		else
		{
			Settlement closestSettlementForNavigationMesh = GetClosestSettlementForNavigationMesh(fromParty.CurrentNavigationFace);
			if (GetDistance(closestSettlementForNavigationMesh, toSettlement, maximumDistance, out distance))
			{
				distance += fromParty.Position2D.Distance(toSettlement.GatePosition) - closestSettlementForNavigationMesh.GatePosition.Distance(toSettlement.GatePosition);
				result = distance <= maximumDistance;
			}
		}
		return result;
	}

	public override bool GetDistance(IMapPoint fromMapPoint, MobileParty toParty, float maximumDistance, out float distance)
	{
		bool result = false;
		if (fromMapPoint.CurrentNavigationFace.FaceIndex == toParty.CurrentNavigationFace.FaceIndex)
		{
			distance = fromMapPoint.Position2D.Distance(toParty.Position2D);
			result = distance <= maximumDistance;
		}
		else
		{
			Settlement closestSettlementForNavigationMesh = GetClosestSettlementForNavigationMesh(fromMapPoint.CurrentNavigationFace);
			Settlement settlement = toParty.CurrentSettlement ?? GetClosestSettlementForNavigationMesh(toParty.CurrentNavigationFace);
			if (GetDistance(closestSettlementForNavigationMesh, settlement, maximumDistance, out distance))
			{
				distance += fromMapPoint.Position2D.Distance(toParty.Position2D) - closestSettlementForNavigationMesh.GatePosition.Distance(settlement.GatePosition);
				result = distance <= maximumDistance;
			}
		}
		return result;
	}

	public override bool GetDistance(IMapPoint fromMapPoint, Settlement toSettlement, float maximumDistance, out float distance)
	{
		bool result = false;
		if (fromMapPoint.CurrentNavigationFace.FaceIndex == toSettlement.CurrentNavigationFace.FaceIndex)
		{
			distance = fromMapPoint.Position2D.Distance(toSettlement.GatePosition);
			result = distance <= maximumDistance;
		}
		else
		{
			distance = 100f;
			Settlement closestSettlementForNavigationMesh = GetClosestSettlementForNavigationMesh(fromMapPoint.CurrentNavigationFace);
			if (GetDistance(closestSettlementForNavigationMesh, toSettlement, maximumDistance, out distance))
			{
				distance += fromMapPoint.Position2D.Distance(toSettlement.GatePosition) - closestSettlementForNavigationMesh.GatePosition.Distance(toSettlement.GatePosition);
				result = distance <= maximumDistance;
			}
		}
		return result;
	}

	public override bool GetDistance(IMapPoint fromMapPoint, in Vec2 toPoint, float maximumDistance, out float distance)
	{
		bool result = false;
		PathFaceRecord faceIndex = Campaign.Current.MapSceneWrapper.GetFaceIndex(toPoint);
		if (fromMapPoint.CurrentNavigationFace.FaceIndex == faceIndex.FaceIndex)
		{
			distance = fromMapPoint.Position2D.Distance(toPoint);
			result = distance <= maximumDistance;
		}
		else
		{
			Settlement closestSettlementForNavigationMesh = GetClosestSettlementForNavigationMesh(fromMapPoint.CurrentNavigationFace);
			Settlement closestSettlementForNavigationMesh2 = GetClosestSettlementForNavigationMesh(faceIndex);
			if (GetDistance(closestSettlementForNavigationMesh, closestSettlementForNavigationMesh2, maximumDistance, out distance))
			{
				distance += fromMapPoint.Position2D.Distance(toPoint) - closestSettlementForNavigationMesh.GatePosition.Distance(closestSettlementForNavigationMesh2.GatePosition);
				result = distance <= maximumDistance;
			}
		}
		return result;
	}

	private float GetDistance(Vec2 pos1, Vec2 pos2, PathFaceRecord faceIndex1, PathFaceRecord faceIndex2)
	{
		Campaign.Current.MapSceneWrapper.GetPathDistanceBetweenAIFaces(faceIndex1, faceIndex2, pos1, pos2, 0.1f, float.MaxValue, out var distance);
		return distance;
	}

	private bool GetDistanceWithDistanceLimit(Vec2 pos1, Vec2 pos2, PathFaceRecord faceIndex1, PathFaceRecord faceIndex2, float distanceLimit, out float distance)
	{
		if (pos1.DistanceSquared(pos2) > distanceLimit * distanceLimit)
		{
			distance = float.MaxValue;
			return false;
		}
		return Campaign.Current.MapSceneWrapper.GetPathDistanceBetweenAIFaces(faceIndex1, faceIndex2, pos1, pos2, 0.1f, distanceLimit, out distance);
	}

	public override Settlement GetClosestSettlementForNavigationMesh(PathFaceRecord face)
	{
		if (!_navigationMeshClosestSettlementCache.TryGetValue(face.FaceIndex, out var value))
		{
			Vec2 navigationMeshCenterPosition = Campaign.Current.MapSceneWrapper.GetNavigationMeshCenterPosition(face);
			float num = float.MaxValue;
			foreach (Settlement item in _settlementsToConsider)
			{
				float num2 = item.GatePosition.DistanceSquared(navigationMeshCenterPosition);
				if (num > num2)
				{
					num = num2;
					value = item;
				}
			}
			_navigationMeshClosestSettlementCache[face.FaceIndex] = value;
		}
		return value;
	}

	private void AddNewPairToDistanceCache((Settlement, Settlement) pair, float distance)
	{
		_settlementDistanceCache.Add(pair, distance);
		if (distance > MaximumDistanceBetweenTwoSettlements)
		{
			MaximumDistanceBetweenTwoSettlements = distance;
			Campaign.Current.UpdateMaximumDistanceBetweenTwoSettlements();
		}
	}
}
