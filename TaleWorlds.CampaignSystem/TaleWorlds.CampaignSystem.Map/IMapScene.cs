using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.Map;

public interface IMapScene
{
	void Load();

	void Destroy();

	PathFaceRecord GetFaceIndex(Vec2 position);

	bool AreFacesOnSameIsland(PathFaceRecord startingFace, PathFaceRecord endFace, bool ignoreDisabled);

	TerrainType GetTerrainTypeAtPosition(Vec2 position);

	List<TerrainType> GetEnvironmentTerrainTypes(Vec2 position);

	List<TerrainType> GetEnvironmentTerrainTypesCount(Vec2 position, out TerrainType currentPositionTerrainType);

	MapPatchData GetMapPatchAtPosition(Vec2 position);

	TerrainType GetFaceTerrainType(PathFaceRecord faceIndex);

	Vec2 GetAccessiblePointNearPosition(Vec2 position, float radius);

	bool GetPathBetweenAIFaces(PathFaceRecord startingFace, PathFaceRecord endingFace, Vec2 startingPosition, Vec2 endingPosition, float agentRadius, NavigationPath path, int[] excludedFaceIds = null);

	bool GetPathDistanceBetweenAIFaces(PathFaceRecord startingAiFace, PathFaceRecord endingAiFace, Vec2 startingPosition, Vec2 endingPosition, float agentRadius, float distanceLimit, out float distance);

	bool IsLineToPointClear(PathFaceRecord startingFace, Vec2 position, Vec2 destination, float agentRadius);

	Vec2 GetLastPointOnNavigationMeshFromPositionToDestination(PathFaceRecord startingFace, Vec2 position, Vec2 destination);

	Vec2 GetNavigationMeshCenterPosition(PathFaceRecord face);

	int GetNumberOfNavigationMeshFaces();

	bool GetHeightAtPoint(Vec2 point, ref float height);

	float GetWinterTimeFactor();

	void GetTerrainHeightAndNormal(Vec2 position, out float height, out Vec3 normal);

	float GetFaceVertexZ(PathFaceRecord navMeshFace);

	Vec3 GetGroundNormal(Vec2 position);

	string GetTerrainTypeName(TerrainType type);

	Vec2 GetTerrainSize();

	uint GetSceneLevel(string name);

	void SetSceneLevels(List<string> levels);

	List<AtmosphereState> GetAtmosphereStates();

	void SetAtmosphereColorgrade(TerrainType terrainType);

	void AddNewEntityToMapScene(string entityId, Vec2 position);

	void GetFaceIndexForMultiplePositions(int movedPartyCount, float[] positionArray, PathFaceRecord[] resultArray);

	void GetMapBorders(out Vec2 minimumPosition, out Vec2 maximumPosition, out float maximumHeight);

	void GetSnowAmountData(byte[] snowData);
}
