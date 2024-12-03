using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.DotNet;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[EngineClass("rglScene")]
public sealed class Scene : NativeObject
{
	public const float AutoClimbHeight = 1.5f;

	public const float NavMeshHeightLimit = 1.5f;

	public static readonly TWSharedMutex PhysicsAndRayCastLock = new TWSharedMutex();

	public int RootEntityCount => EngineApplicationInterface.IScene.GetRootEntityCount(base.Pointer);

	public bool HasTerrainHeightmap => EngineApplicationInterface.IScene.HasTerrainHeightmap(base.Pointer);

	public bool ContainsTerrain => EngineApplicationInterface.IScene.ContainsTerrain(base.Pointer);

	public float TimeOfDay
	{
		get
		{
			return EngineApplicationInterface.IScene.GetTimeOfDay(base.Pointer);
		}
		set
		{
			EngineApplicationInterface.IScene.SetTimeOfDay(base.Pointer, value);
		}
	}

	public bool IsAtmosphereIndoor => EngineApplicationInterface.IScene.IsAtmosphereIndoor(base.Pointer);

	public Vec3 LastFinalRenderCameraPosition => EngineApplicationInterface.IScene.GetLastFinalRenderCameraPosition(base.Pointer);

	public MatrixFrame LastFinalRenderCameraFrame
	{
		get
		{
			MatrixFrame outFrame = default(MatrixFrame);
			EngineApplicationInterface.IScene.GetLastFinalRenderCameraFrame(base.Pointer, ref outFrame);
			return outFrame;
		}
	}

	public float TimeSpeed
	{
		get
		{
			return EngineApplicationInterface.IScene.GetTimeSpeed(base.Pointer);
		}
		set
		{
			EngineApplicationInterface.IScene.SetTimeSpeed(base.Pointer, value);
		}
	}

	private Scene()
	{
	}

	internal Scene(UIntPtr pointer)
	{
		Construct(pointer);
	}

	public bool IsDefaultEditorScene()
	{
		return EngineApplicationInterface.IScene.IsDefaultEditorScene(this);
	}

	public bool IsMultiplayerScene()
	{
		return EngineApplicationInterface.IScene.IsMultiplayerScene(this);
	}

	public string TakePhotoModePicture(bool saveAmbientOcclusionPass, bool savingObjectIdPass, bool saveShadowPass)
	{
		return EngineApplicationInterface.IScene.TakePhotoModePicture(this, saveAmbientOcclusionPass, savingObjectIdPass, saveShadowPass);
	}

	public string GetAllColorGradeNames()
	{
		return EngineApplicationInterface.IScene.GetAllColorGradeNames(this);
	}

	public string GetAllFilterNames()
	{
		return EngineApplicationInterface.IScene.GetAllFilterNames(this);
	}

	public float GetPhotoModeRoll()
	{
		return EngineApplicationInterface.IScene.GetPhotoModeRoll(this);
	}

	public bool GetPhotoModeOrbit()
	{
		return EngineApplicationInterface.IScene.GetPhotoModeOrbit(this);
	}

	public bool GetPhotoModeOn()
	{
		return EngineApplicationInterface.IScene.GetPhotoModeOn(this);
	}

	public void GetPhotoModeFocus(ref float focus, ref float focusStart, ref float focusEnd, ref float exposure, ref bool vignetteOn)
	{
		EngineApplicationInterface.IScene.GetPhotoModeFocus(this, ref focus, ref focusStart, ref focusEnd, ref exposure, ref vignetteOn);
	}

	public int GetSceneColorGradeIndex()
	{
		return EngineApplicationInterface.IScene.GetSceneColorGradeIndex(this);
	}

	public int GetSceneFilterIndex()
	{
		return EngineApplicationInterface.IScene.GetSceneFilterIndex(this);
	}

	public string GetLoadingStateName()
	{
		return EngineApplicationInterface.IScene.GetLoadingStateName(this);
	}

	public bool IsLoadingFinished()
	{
		return EngineApplicationInterface.IScene.IsLoadingFinished(this);
	}

	public void SetPhotoModeRoll(float roll)
	{
		EngineApplicationInterface.IScene.SetPhotoModeRoll(this, roll);
	}

	public void SetPhotoModeOrbit(bool orbit)
	{
		EngineApplicationInterface.IScene.SetPhotoModeOrbit(this, orbit);
	}

	public void SetPhotoModeOn(bool on)
	{
		EngineApplicationInterface.IScene.SetPhotoModeOn(this, on);
	}

	public void SetPhotoModeFocus(float focusStart, float focusEnd, float focus, float exposure)
	{
		EngineApplicationInterface.IScene.SetPhotoModeFocus(this, focusStart, focusEnd, focus, exposure);
	}

	public void SetPhotoModeFov(float verticalFov)
	{
		EngineApplicationInterface.IScene.SetPhotoModeFov(this, verticalFov);
	}

	public float GetPhotoModeFov()
	{
		return EngineApplicationInterface.IScene.GetPhotoModeFov(this);
	}

	public void SetPhotoModeVignette(bool vignetteOn)
	{
		EngineApplicationInterface.IScene.SetPhotoModeVignette(this, vignetteOn);
	}

	public void SetSceneColorGradeIndex(int index)
	{
		EngineApplicationInterface.IScene.SetSceneColorGradeIndex(this, index);
	}

	public int SetSceneFilterIndex(int index)
	{
		return EngineApplicationInterface.IScene.SetSceneFilterIndex(this, index);
	}

	public void SetSceneColorGrade(string textureName)
	{
		EngineApplicationInterface.IScene.SetSceneColorGrade(this, textureName);
	}

	public void SetUpgradeLevel(int level)
	{
		EngineApplicationInterface.IScene.SetUpgradeLevel(base.Pointer, level);
	}

	public void CreateBurstParticle(int particleId, MatrixFrame frame)
	{
		EngineApplicationInterface.IScene.CreateBurstParticle(this, particleId, ref frame);
	}

	public float[] GetTerrainHeightData(int nodeXIndex, int nodeYIndex)
	{
		float[] array = new float[EngineApplicationInterface.IScene.GetNodeDataCount(this, nodeXIndex, nodeYIndex)];
		EngineApplicationInterface.IScene.FillTerrainHeightData(this, nodeXIndex, nodeYIndex, array);
		return array;
	}

	public short[] GetTerrainPhysicsMaterialIndexData(int nodeXIndex, int nodeYIndex)
	{
		short[] array = new short[EngineApplicationInterface.IScene.GetNodeDataCount(this, nodeXIndex, nodeYIndex)];
		EngineApplicationInterface.IScene.FillTerrainPhysicsMaterialIndexData(this, nodeXIndex, nodeYIndex, array);
		return array;
	}

	public void GetTerrainData(out Vec2i nodeDimension, out float nodeSize, out int layerCount, out int layerVersion)
	{
		EngineApplicationInterface.IScene.GetTerrainData(this, out nodeDimension, out nodeSize, out layerCount, out layerVersion);
	}

	public void GetTerrainNodeData(int xIndex, int yIndex, out int vertexCountAlongAxis, out float quadLength, out float minHeight, out float maxHeight)
	{
		EngineApplicationInterface.IScene.GetTerrainNodeData(this, xIndex, yIndex, out vertexCountAlongAxis, out quadLength, out minHeight, out maxHeight);
	}

	public PhysicsMaterial GetTerrainPhysicsMaterialAtLayer(int layerIndex)
	{
		int terrainPhysicsMaterialIndexAtLayer = EngineApplicationInterface.IScene.GetTerrainPhysicsMaterialIndexAtLayer(this, layerIndex);
		return new PhysicsMaterial(terrainPhysicsMaterialIndexAtLayer);
	}

	public void SetSceneColorGrade(Scene scene, string textureName)
	{
		EngineApplicationInterface.IScene.SetSceneColorGrade(scene, textureName);
	}

	public float GetWaterLevel()
	{
		return EngineApplicationInterface.IScene.GetWaterLevel(this);
	}

	public float GetWaterLevelAtPosition(Vec2 position, bool checkWaterBodyEntities)
	{
		return EngineApplicationInterface.IScene.GetWaterLevelAtPosition(this, position, checkWaterBodyEntities);
	}

	public bool GetPathBetweenAIFaces(UIntPtr startingFace, UIntPtr endingFace, Vec2 startingPosition, Vec2 endingPosition, float agentRadius, NavigationPath path, int[] excludedFaceIds = null)
	{
		int pathSize = path.PathPoints.Length;
		if (EngineApplicationInterface.IScene.GetPathBetweenAIFacePointers(base.Pointer, startingFace, endingFace, startingPosition, endingPosition, agentRadius, path.PathPoints, ref pathSize, excludedFaceIds, (excludedFaceIds != null) ? excludedFaceIds.Length : 0))
		{
			path.Size = pathSize;
			return true;
		}
		path.Size = 0;
		return false;
	}

	public bool GetPathBetweenAIFaces(int startingFace, int endingFace, Vec2 startingPosition, Vec2 endingPosition, float agentRadius, NavigationPath path, int[] excludedFaceIds = null, float extraCostMultiplier = 1f)
	{
		int pathSize = path.PathPoints.Length;
		if (EngineApplicationInterface.IScene.GetPathBetweenAIFaceIndices(base.Pointer, startingFace, endingFace, startingPosition, endingPosition, agentRadius, path.PathPoints, ref pathSize, excludedFaceIds, (excludedFaceIds != null) ? excludedFaceIds.Length : 0, extraCostMultiplier))
		{
			path.Size = pathSize;
			return true;
		}
		path.Size = 0;
		return false;
	}

	public bool GetPathDistanceBetweenAIFaces(int startingAiFace, int endingAiFace, Vec2 startingPosition, Vec2 endingPosition, float agentRadius, float distanceLimit, out float distance)
	{
		return EngineApplicationInterface.IScene.GetPathDistanceBetweenAIFaces(base.Pointer, startingAiFace, endingAiFace, startingPosition, endingPosition, agentRadius, distanceLimit, out distance);
	}

	public void GetNavMeshFaceIndex(ref PathFaceRecord record, Vec2 position, bool checkIfDisabled, bool ignoreHeight = false)
	{
		EngineApplicationInterface.IScene.GetNavMeshFaceIndex(base.Pointer, ref record, position, checkIfDisabled, ignoreHeight);
	}

	public void GetNavMeshFaceIndex(ref PathFaceRecord record, Vec3 position, bool checkIfDisabled)
	{
		EngineApplicationInterface.IScene.GetNavMeshFaceIndex3(base.Pointer, ref record, position, checkIfDisabled);
	}

	public static Scene CreateNewScene(bool initialize_physics = true, bool enable_decals = true, DecalAtlasGroup atlasGroup = DecalAtlasGroup.All, string sceneName = "mono_renderscene")
	{
		return EngineApplicationInterface.IScene.CreateNewScene(initialize_physics, enable_decals, (int)atlasGroup, sceneName);
	}

	public MetaMesh CreatePathMesh(string baseEntityName, bool isWaterPath)
	{
		return EngineApplicationInterface.IScene.CreatePathMesh(base.Pointer, baseEntityName, isWaterPath);
	}

	public void SetActiveVisibilityLevels(List<string> levelsToActivate)
	{
		string text = "";
		for (int i = 0; i < levelsToActivate.Count; i++)
		{
			if (!levelsToActivate[i].Contains("$"))
			{
				if (i != 0)
				{
					text += "$";
				}
				text += levelsToActivate[i];
			}
		}
		EngineApplicationInterface.IScene.SetActiveVisibilityLevels(base.Pointer, text);
	}

	public void SetDoNotWaitForLoadingStatesToRender(bool value)
	{
		EngineApplicationInterface.IScene.SetDoNotWaitForLoadingStatesToRender(base.Pointer, value);
	}

	public void GetSnowAmountData(byte[] snowData)
	{
		EngineApplicationInterface.IScene.GetSnowAmountData(base.Pointer, snowData);
	}

	public MetaMesh CreatePathMesh(IList<GameEntity> pathNodes, bool isWaterPath = false)
	{
		return EngineApplicationInterface.IScene.CreatePathMesh2(base.Pointer, pathNodes.Select((GameEntity e) => e.Pointer).ToArray(), pathNodes.Count, isWaterPath);
	}

	public GameEntity GetEntityWithGuid(string guid)
	{
		return EngineApplicationInterface.IScene.GetEntityWithGuid(base.Pointer, guid);
	}

	public bool IsEntityFrameChanged(string containsName)
	{
		return EngineApplicationInterface.IScene.CheckPathEntitiesFrameChanged(base.Pointer, containsName);
	}

	public void GetTerrainHeightAndNormal(Vec2 position, out float height, out Vec3 normal)
	{
		EngineApplicationInterface.IScene.GetTerrainHeightAndNormal(base.Pointer, position, out height, out normal);
	}

	public int GetFloraInstanceCount()
	{
		return EngineApplicationInterface.IScene.GetFloraInstanceCount(base.Pointer);
	}

	public int GetFloraRendererTextureUsage()
	{
		return EngineApplicationInterface.IScene.GetFloraRendererTextureUsage(base.Pointer);
	}

	public int GetTerrainMemoryUsage()
	{
		return EngineApplicationInterface.IScene.GetTerrainMemoryUsage(base.Pointer);
	}

	public void StallLoadingRenderingsUntilFurtherNotice()
	{
		EngineApplicationInterface.IScene.StallLoadingRenderingsUntilFurtherNotice(base.Pointer);
	}

	public int GetNavMeshFaceCount()
	{
		return EngineApplicationInterface.IScene.GetNavMeshFaceCount(base.Pointer);
	}

	public void ResumeLoadingRenderings()
	{
		EngineApplicationInterface.IScene.ResumeLoadingRenderings(base.Pointer);
	}

	public uint GetUpgradeLevelMask()
	{
		return EngineApplicationInterface.IScene.GetUpgradeLevelMask(base.Pointer);
	}

	public void SetUpgradeLevelVisibility(uint mask)
	{
		EngineApplicationInterface.IScene.SetUpgradeLevelVisibilityWithMask(base.Pointer, mask);
	}

	public void SetUpgradeLevelVisibility(List<string> levels)
	{
		string text = "";
		for (int i = 0; i < levels.Count - 1; i++)
		{
			text = text + levels[i] + "|";
		}
		text += levels[levels.Count - 1];
		EngineApplicationInterface.IScene.SetUpgradeLevelVisibility(base.Pointer, text);
	}

	public int GetIdOfNavMeshFace(int faceIndex)
	{
		return EngineApplicationInterface.IScene.GetIdOfNavMeshFace(base.Pointer, faceIndex);
	}

	public void SetClothSimulationState(bool state)
	{
		EngineApplicationInterface.IScene.SetClothSimulationState(base.Pointer, state);
	}

	public void GetNavMeshCenterPosition(int faceIndex, ref Vec3 centerPosition)
	{
		EngineApplicationInterface.IScene.GetNavMeshFaceCenterPosition(base.Pointer, faceIndex, ref centerPosition);
	}

	public GameEntity GetFirstEntityWithName(string name)
	{
		return EngineApplicationInterface.IScene.GetFirstEntityWithName(base.Pointer, name);
	}

	public GameEntity GetCampaignEntityWithName(string name)
	{
		return EngineApplicationInterface.IScene.GetCampaignEntityWithName(base.Pointer, name);
	}

	public void GetAllEntitiesWithScriptComponent<T>(ref List<GameEntity> entities) where T : ScriptComponentBehavior
	{
		NativeObjectArray nativeObjectArray = NativeObjectArray.Create();
		string name = typeof(T).Name;
		EngineApplicationInterface.IScene.GetAllEntitiesWithScriptComponent(base.Pointer, name, nativeObjectArray.Pointer);
		for (int i = 0; i < nativeObjectArray.Count; i++)
		{
			entities.Add(nativeObjectArray.GetElementAt(i) as GameEntity);
		}
	}

	public GameEntity GetFirstEntityWithScriptComponent<T>() where T : ScriptComponentBehavior
	{
		string name = typeof(T).Name;
		return EngineApplicationInterface.IScene.GetFirstEntityWithScriptComponent(base.Pointer, name);
	}

	public uint GetUpgradeLevelMaskOfLevelName(string levelName)
	{
		return EngineApplicationInterface.IScene.GetUpgradeLevelMaskOfLevelName(base.Pointer, levelName);
	}

	public string GetUpgradeLevelNameOfIndex(int index)
	{
		return EngineApplicationInterface.IScene.GetUpgradeLevelNameOfIndex(base.Pointer, index);
	}

	public int GetUpgradeLevelCount()
	{
		return EngineApplicationInterface.IScene.GetUpgradeLevelCount(base.Pointer);
	}

	public float GetWinterTimeFactor()
	{
		return EngineApplicationInterface.IScene.GetWinterTimeFactor(base.Pointer);
	}

	public float GetNavMeshFaceFirstVertexZ(int faceIndex)
	{
		return EngineApplicationInterface.IScene.GetNavMeshFaceFirstVertexZ(base.Pointer, faceIndex);
	}

	public void SetWinterTimeFactor(float winterTimeFactor)
	{
		EngineApplicationInterface.IScene.SetWinterTimeFactor(base.Pointer, winterTimeFactor);
	}

	public void SetDrynessFactor(float drynessFactor)
	{
		EngineApplicationInterface.IScene.SetDrynessFactor(base.Pointer, drynessFactor);
	}

	public float GetFog()
	{
		return EngineApplicationInterface.IScene.GetFog(base.Pointer);
	}

	public void SetFog(float fogDensity, ref Vec3 fogColor, float fogFalloff)
	{
		EngineApplicationInterface.IScene.SetFog(base.Pointer, fogDensity, ref fogColor, fogFalloff);
	}

	public void SetFogAdvanced(float fogFalloffOffset, float fogFalloffMinFog, float fogFalloffStartDist)
	{
		EngineApplicationInterface.IScene.SetFogAdvanced(base.Pointer, fogFalloffOffset, fogFalloffMinFog, fogFalloffStartDist);
	}

	public void SetFogAmbientColor(ref Vec3 fogAmbientColor)
	{
		EngineApplicationInterface.IScene.SetFogAmbientColor(base.Pointer, ref fogAmbientColor);
	}

	public void SetTemperature(float temperature)
	{
		EngineApplicationInterface.IScene.SetTemperature(base.Pointer, temperature);
	}

	public void SetHumidity(float humidity)
	{
		EngineApplicationInterface.IScene.SetHumidity(base.Pointer, humidity);
	}

	public void SetDynamicShadowmapCascadesRadiusMultiplier(float multiplier)
	{
		EngineApplicationInterface.IScene.SetDynamicShadowmapCascadesRadiusMultiplier(base.Pointer, multiplier);
	}

	public void SetEnvironmentMultiplier(bool useMultiplier, float multiplier)
	{
		EngineApplicationInterface.IScene.SetEnvironmentMultiplier(base.Pointer, useMultiplier, multiplier);
	}

	public void SetSkyRotation(float rotation)
	{
		EngineApplicationInterface.IScene.SetSkyRotation(base.Pointer, rotation);
	}

	public void SetSkyBrightness(float brightness)
	{
		EngineApplicationInterface.IScene.SetSkyBrightness(base.Pointer, brightness);
	}

	public void SetForcedSnow(bool value)
	{
		EngineApplicationInterface.IScene.SetForcedSnow(base.Pointer, value);
	}

	public void SetSunLight(ref Vec3 color, ref Vec3 direction)
	{
		EngineApplicationInterface.IScene.SetSunLight(base.Pointer, color, direction);
	}

	public void SetSunDirection(ref Vec3 direction)
	{
		EngineApplicationInterface.IScene.SetSunDirection(base.Pointer, direction);
	}

	public void SetSun(ref Vec3 color, float altitude, float angle, float intensity)
	{
		EngineApplicationInterface.IScene.SetSun(base.Pointer, color, altitude, angle, intensity);
	}

	public void SetSunAngleAltitude(float angle, float altitude)
	{
		EngineApplicationInterface.IScene.SetSunAngleAltitude(base.Pointer, angle, altitude);
	}

	public void SetSunSize(float size)
	{
		EngineApplicationInterface.IScene.SetSunSize(base.Pointer, size);
	}

	public void SetSunShaftStrength(float strength)
	{
		EngineApplicationInterface.IScene.SetSunShaftStrength(base.Pointer, strength);
	}

	public float GetRainDensity()
	{
		return EngineApplicationInterface.IScene.GetRainDensity(base.Pointer);
	}

	public void SetRainDensity(float density)
	{
		EngineApplicationInterface.IScene.SetRainDensity(base.Pointer, density);
	}

	public float GetSnowDensity()
	{
		return EngineApplicationInterface.IScene.GetSnowDensity(base.Pointer);
	}

	public void SetSnowDensity(float density)
	{
		EngineApplicationInterface.IScene.SetSnowDensity(base.Pointer, density);
	}

	public void AddDecalInstance(Decal decal, string decalSetID, bool deletable)
	{
		EngineApplicationInterface.IScene.AddDecalInstance(base.Pointer, decal.Pointer, decalSetID, deletable);
	}

	public void SetShadow(bool shadowEnabled)
	{
		EngineApplicationInterface.IScene.SetShadow(base.Pointer, shadowEnabled);
	}

	public int AddPointLight(ref Vec3 position, float radius)
	{
		return EngineApplicationInterface.IScene.AddPointLight(base.Pointer, position, radius);
	}

	public int AddDirectionalLight(ref Vec3 position, ref Vec3 direction, float radius)
	{
		return EngineApplicationInterface.IScene.AddDirectionalLight(base.Pointer, position, direction, radius);
	}

	public void SetLightPosition(int lightIndex, ref Vec3 position)
	{
		EngineApplicationInterface.IScene.SetLightPosition(base.Pointer, lightIndex, position);
	}

	public void SetLightDiffuseColor(int lightIndex, ref Vec3 diffuseColor)
	{
		EngineApplicationInterface.IScene.SetLightDiffuseColor(base.Pointer, lightIndex, diffuseColor);
	}

	public void SetLightDirection(int lightIndex, ref Vec3 direction)
	{
		EngineApplicationInterface.IScene.SetLightDirection(base.Pointer, lightIndex, direction);
	}

	public void SetMieScatterFocus(float strength)
	{
		EngineApplicationInterface.IScene.SetMieScatterFocus(base.Pointer, strength);
	}

	public void SetMieScatterStrength(float strength)
	{
		EngineApplicationInterface.IScene.SetMieScatterStrength(base.Pointer, strength);
	}

	public void SetBrightpassThreshold(float threshold)
	{
		EngineApplicationInterface.IScene.SetBrightpassTreshold(base.Pointer, threshold);
	}

	public void SetLensDistortion(float amount)
	{
		EngineApplicationInterface.IScene.SetLensDistortion(base.Pointer, amount);
	}

	public void SetHexagonVignetteAlpha(float amount)
	{
		EngineApplicationInterface.IScene.SetHexagonVignetteAlpha(base.Pointer, amount);
	}

	public void SetMinExposure(float minExposure)
	{
		EngineApplicationInterface.IScene.SetMinExposure(base.Pointer, minExposure);
	}

	public void SetMaxExposure(float maxExposure)
	{
		EngineApplicationInterface.IScene.SetMaxExposure(base.Pointer, maxExposure);
	}

	public void SetTargetExposure(float targetExposure)
	{
		EngineApplicationInterface.IScene.SetTargetExposure(base.Pointer, targetExposure);
	}

	public void SetMiddleGray(float middleGray)
	{
		EngineApplicationInterface.IScene.SetMiddleGray(base.Pointer, middleGray);
	}

	public void SetBloomStrength(float bloomStrength)
	{
		EngineApplicationInterface.IScene.SetBloomStrength(base.Pointer, bloomStrength);
	}

	public void SetBloomAmount(float bloomAmount)
	{
		EngineApplicationInterface.IScene.SetBloomAmount(base.Pointer, bloomAmount);
	}

	public void SetGrainAmount(float grainAmount)
	{
		EngineApplicationInterface.IScene.SetGrainAmount(base.Pointer, grainAmount);
	}

	public GameEntity AddItemEntity(ref MatrixFrame placementFrame, MetaMesh metaMesh)
	{
		return EngineApplicationInterface.IScene.AddItemEntity(base.Pointer, ref placementFrame, metaMesh.Pointer);
	}

	public void RemoveEntity(GameEntity entity, int removeReason)
	{
		EngineApplicationInterface.IScene.RemoveEntity(base.Pointer, entity.Pointer, removeReason);
	}

	public bool AttachEntity(GameEntity entity, bool showWarnings = false)
	{
		return EngineApplicationInterface.IScene.AttachEntity(base.Pointer, entity.Pointer, showWarnings);
	}

	public void AddEntityWithMesh(Mesh mesh, ref MatrixFrame frame)
	{
		EngineApplicationInterface.IScene.AddEntityWithMesh(base.Pointer, mesh.Pointer, ref frame);
	}

	public void AddEntityWithMultiMesh(MetaMesh mesh, ref MatrixFrame frame)
	{
		EngineApplicationInterface.IScene.AddEntityWithMultiMesh(base.Pointer, mesh.Pointer, ref frame);
	}

	public void Tick(float dt)
	{
		EngineApplicationInterface.IScene.Tick(base.Pointer, dt);
	}

	public void ClearAll()
	{
		EngineApplicationInterface.IScene.ClearAll(base.Pointer);
	}

	public void SetDefaultLighting()
	{
		Vec3 color = new Vec3(1.15f, 1.2f, 1.25f);
		Vec3 direction = new Vec3(1f, -1f, -1f);
		direction.Normalize();
		SetSunLight(ref color, ref direction);
		SetShadow(shadowEnabled: false);
	}

	public bool CalculateEffectiveLighting()
	{
		return EngineApplicationInterface.IScene.CalculateEffectiveLighting(base.Pointer);
	}

	public bool GetPathDistanceBetweenPositions(ref WorldPosition point0, ref WorldPosition point1, float agentRadius, out float pathDistance)
	{
		pathDistance = 0f;
		return EngineApplicationInterface.IScene.GetPathDistanceBetweenPositions(base.Pointer, ref point0, ref point1, agentRadius, ref pathDistance);
	}

	public bool IsLineToPointClear(ref WorldPosition position, ref WorldPosition destination, float agentRadius)
	{
		return EngineApplicationInterface.IScene.IsLineToPointClear2(base.Pointer, position.GetNavMesh(), position.AsVec2, destination.AsVec2, agentRadius);
	}

	public bool IsLineToPointClear(int startingFace, Vec2 position, Vec2 destination, float agentRadius)
	{
		return EngineApplicationInterface.IScene.IsLineToPointClear(base.Pointer, startingFace, position, destination, agentRadius);
	}

	public Vec2 GetLastPointOnNavigationMeshFromPositionToDestination(int startingFace, Vec2 position, Vec2 destination)
	{
		return EngineApplicationInterface.IScene.GetLastPointOnNavigationMeshFromPositionToDestination(base.Pointer, startingFace, position, destination);
	}

	public Vec3 GetLastPointOnNavigationMeshFromWorldPositionToDestination(ref WorldPosition position, Vec2 destination)
	{
		return EngineApplicationInterface.IScene.GetLastPointOnNavigationMeshFromWorldPositionToDestination(base.Pointer, ref position, destination);
	}

	public bool DoesPathExistBetweenFaces(int firstNavMeshFace, int secondNavMeshFace, bool ignoreDisabled)
	{
		return EngineApplicationInterface.IScene.DoesPathExistBetweenFaces(base.Pointer, firstNavMeshFace, secondNavMeshFace, ignoreDisabled);
	}

	public bool GetHeightAtPoint(Vec2 point, BodyFlags excludeBodyFlags, ref float height)
	{
		return EngineApplicationInterface.IScene.GetHeightAtPoint(base.Pointer, point, excludeBodyFlags, ref height);
	}

	public Vec3 GetNormalAt(Vec2 position)
	{
		return EngineApplicationInterface.IScene.GetNormalAt(base.Pointer, position);
	}

	public void GetEntities(ref List<GameEntity> entities)
	{
		NativeObjectArray nativeObjectArray = NativeObjectArray.Create();
		EngineApplicationInterface.IScene.GetEntities(base.Pointer, nativeObjectArray.Pointer);
		for (int i = 0; i < nativeObjectArray.Count; i++)
		{
			entities.Add(nativeObjectArray.GetElementAt(i) as GameEntity);
		}
	}

	public void GetRootEntities(NativeObjectArray entities)
	{
		EngineApplicationInterface.IScene.GetRootEntities(this, entities);
	}

	public int SelectEntitiesInBoxWithScriptComponent<T>(ref Vec3 boundingBoxMin, ref Vec3 boundingBoxMax, GameEntity[] entitiesOutput, UIntPtr[] entityIds) where T : ScriptComponentBehavior
	{
		string name = typeof(T).Name;
		int num = EngineApplicationInterface.IScene.SelectEntitiesInBoxWithScriptComponent(base.Pointer, ref boundingBoxMin, ref boundingBoxMax, entityIds, entitiesOutput.Length, name);
		for (int i = 0; i < num; i++)
		{
			entitiesOutput[i] = new GameEntity(entityIds[i]);
		}
		return num;
	}

	public int SelectEntitiesCollidedWith(ref Ray ray, Intersection[] intersectionsOutput, UIntPtr[] entityIds)
	{
		return EngineApplicationInterface.IScene.SelectEntitiesCollidedWith(base.Pointer, ref ray, entityIds, intersectionsOutput);
	}

	public int GenerateContactsWithCapsule(ref CapsuleData capsule, BodyFlags exclude_flags, Intersection[] intersectionsOutput)
	{
		return EngineApplicationInterface.IScene.GenerateContactsWithCapsule(base.Pointer, ref capsule, exclude_flags, intersectionsOutput);
	}

	public void InvalidateTerrainPhysicsMaterials()
	{
		EngineApplicationInterface.IScene.InvalidateTerrainPhysicsMaterials(base.Pointer);
	}

	public void Read(string sceneName)
	{
		SceneInitializationData initData = new SceneInitializationData(initializeWithDefaults: true);
		EngineApplicationInterface.IScene.Read(base.Pointer, sceneName, ref initData, "");
	}

	public void Read(string sceneName, ref SceneInitializationData initData, string forcedAtmoName = "")
	{
		EngineApplicationInterface.IScene.Read(base.Pointer, sceneName, ref initData, forcedAtmoName);
	}

	public MatrixFrame ReadAndCalculateInitialCamera()
	{
		MatrixFrame outFrame = default(MatrixFrame);
		EngineApplicationInterface.IScene.ReadAndCalculateInitialCamera(base.Pointer, ref outFrame);
		return outFrame;
	}

	public void OptimizeScene(bool optimizeFlora = true, bool optimizeOro = false)
	{
		EngineApplicationInterface.IScene.OptimizeScene(base.Pointer, optimizeFlora, optimizeOro);
	}

	public float GetTerrainHeight(Vec2 position, bool checkHoles = true)
	{
		return EngineApplicationInterface.IScene.GetTerrainHeight(base.Pointer, position, checkHoles);
	}

	public void CheckResources()
	{
		EngineApplicationInterface.IScene.CheckResources(base.Pointer);
	}

	public void ForceLoadResources()
	{
		EngineApplicationInterface.IScene.ForceLoadResources(base.Pointer);
	}

	public void SetDepthOfFieldParameters(float depthOfFieldFocusStart, float depthOfFieldFocusEnd, bool isVignetteOn)
	{
		EngineApplicationInterface.IScene.SetDofParams(base.Pointer, depthOfFieldFocusStart, depthOfFieldFocusEnd, isVignetteOn);
	}

	public void SetDepthOfFieldFocus(float depthOfFieldFocus)
	{
		EngineApplicationInterface.IScene.SetDofFocus(base.Pointer, depthOfFieldFocus);
	}

	public void ResetDepthOfFieldParams()
	{
		EngineApplicationInterface.IScene.SetDofFocus(base.Pointer, 0f);
		EngineApplicationInterface.IScene.SetDofParams(base.Pointer, 0f, 0f, isVignetteOn: true);
	}

	public void PreloadForRendering()
	{
		EngineApplicationInterface.IScene.PreloadForRendering(base.Pointer);
	}

	public void SetColorGradeBlend(string texture1, string texture2, float alpha)
	{
		EngineApplicationInterface.IScene.SetColorGradeBlend(base.Pointer, texture1, texture2, alpha);
	}

	public float GetGroundHeightAtPosition(Vec3 position, BodyFlags excludeFlags = BodyFlags.CommonCollisionExcludeFlags)
	{
		return EngineApplicationInterface.IScene.GetGroundHeightAtPosition(base.Pointer, position, (uint)excludeFlags);
	}

	public float GetGroundHeightAtPositionMT(Vec3 position, BodyFlags excludeFlags = BodyFlags.CommonCollisionExcludeFlags)
	{
		return EngineApplicationInterface.IScene.GetGroundHeightAtPosition(base.Pointer, position, (uint)excludeFlags);
	}

	public float GetGroundHeightAtPosition(Vec3 position, out Vec3 normal, BodyFlags excludeFlags = BodyFlags.CommonCollisionExcludeFlags)
	{
		normal = Vec3.Invalid;
		return EngineApplicationInterface.IScene.GetGroundHeightAtPosition(base.Pointer, position, (uint)excludeFlags);
	}

	public float GetGroundHeightAtPositionMT(Vec3 position, out Vec3 normal, BodyFlags excludeFlags = BodyFlags.CommonCollisionExcludeFlags)
	{
		normal = Vec3.Invalid;
		return EngineApplicationInterface.IScene.GetGroundHeightAndNormalAtPosition(base.Pointer, position, ref normal, (uint)excludeFlags);
	}

	public void PauseSceneSounds()
	{
		EngineApplicationInterface.IScene.PauseSceneSounds(base.Pointer);
	}

	public void ResumeSceneSounds()
	{
		EngineApplicationInterface.IScene.ResumeSceneSounds(base.Pointer);
	}

	public void FinishSceneSounds()
	{
		EngineApplicationInterface.IScene.FinishSceneSounds(base.Pointer);
	}

	public bool BoxCastOnlyForCamera(Vec3[] boxPoints, Vec3 centerPoint, bool castSupportRay, Vec3 supportRaycastPoint, Vec3 dir, float distance, out float collisionDistance, out Vec3 closestPoint, out GameEntity collidedEntity, bool preFilter = true, bool postFilter = true, BodyFlags excludedBodyFlags = BodyFlags.CameraCollisionRayCastExludeFlags | BodyFlags.DontCollideWithCamera)
	{
		collisionDistance = float.NaN;
		closestPoint = Vec3.Invalid;
		UIntPtr entityIndex = UIntPtr.Zero;
		bool flag = castSupportRay && EngineApplicationInterface.IScene.RayCastForClosestEntityOrTerrain(base.Pointer, ref supportRaycastPoint, ref centerPoint, 0f, ref collisionDistance, ref closestPoint, ref entityIndex, excludedBodyFlags);
		if (!flag)
		{
			flag = EngineApplicationInterface.IScene.BoxCastOnlyForCamera(base.Pointer, boxPoints, ref centerPoint, ref dir, distance, ref collisionDistance, ref closestPoint, ref entityIndex, excludedBodyFlags, preFilter, postFilter);
		}
		if (flag && entityIndex != UIntPtr.Zero)
		{
			collidedEntity = new GameEntity(entityIndex);
		}
		else
		{
			collidedEntity = null;
		}
		return flag;
	}

	public bool BoxCast(Vec3 boxMin, Vec3 boxMax, bool castSupportRay, Vec3 supportRaycastPoint, Vec3 dir, float distance, out float collisionDistance, out Vec3 closestPoint, out GameEntity collidedEntity, BodyFlags excludedBodyFlags = BodyFlags.CameraCollisionRayCastExludeFlags)
	{
		collisionDistance = float.NaN;
		closestPoint = Vec3.Invalid;
		UIntPtr entityIndex = UIntPtr.Zero;
		Vec3 targetPoint = (boxMin + boxMax) * 0.5f;
		bool flag = castSupportRay && EngineApplicationInterface.IScene.RayCastForClosestEntityOrTerrain(base.Pointer, ref supportRaycastPoint, ref targetPoint, 0f, ref collisionDistance, ref closestPoint, ref entityIndex, excludedBodyFlags);
		if (!flag)
		{
			flag = EngineApplicationInterface.IScene.BoxCast(base.Pointer, ref boxMin, ref boxMax, ref dir, distance, ref collisionDistance, ref closestPoint, ref entityIndex, excludedBodyFlags);
		}
		if (flag && entityIndex != UIntPtr.Zero)
		{
			collidedEntity = new GameEntity(entityIndex);
		}
		else
		{
			collidedEntity = null;
		}
		return flag;
	}

	public bool RayCastForClosestEntityOrTerrain(Vec3 sourcePoint, Vec3 targetPoint, out float collisionDistance, out Vec3 closestPoint, out GameEntity collidedEntity, float rayThickness = 0.01f, BodyFlags excludeBodyFlags = BodyFlags.CommonFocusRayCastExcludeFlags)
	{
		collisionDistance = float.NaN;
		closestPoint = Vec3.Invalid;
		UIntPtr entityIndex = UIntPtr.Zero;
		bool num = EngineApplicationInterface.IScene.RayCastForClosestEntityOrTerrain(base.Pointer, ref sourcePoint, ref targetPoint, rayThickness, ref collisionDistance, ref closestPoint, ref entityIndex, excludeBodyFlags);
		if (num && entityIndex != UIntPtr.Zero)
		{
			collidedEntity = new GameEntity(entityIndex);
			return num;
		}
		collidedEntity = null;
		return num;
	}

	public bool RayCastForClosestEntityOrTerrainMT(Vec3 sourcePoint, Vec3 targetPoint, out float collisionDistance, out Vec3 closestPoint, out GameEntity collidedEntity, float rayThickness = 0.01f, BodyFlags excludeBodyFlags = BodyFlags.CommonFocusRayCastExcludeFlags)
	{
		collisionDistance = float.NaN;
		closestPoint = Vec3.Invalid;
		UIntPtr entityIndex = UIntPtr.Zero;
		bool num = EngineApplicationInterface.IScene.RayCastForClosestEntityOrTerrain(base.Pointer, ref sourcePoint, ref targetPoint, rayThickness, ref collisionDistance, ref closestPoint, ref entityIndex, excludeBodyFlags);
		if (num && entityIndex != UIntPtr.Zero)
		{
			collidedEntity = new GameEntity(entityIndex);
			return num;
		}
		collidedEntity = null;
		return num;
	}

	public bool RayCastForClosestEntityOrTerrain(Vec3 sourcePoint, Vec3 targetPoint, out float collisionDistance, out GameEntity collidedEntity, float rayThickness = 0.01f, BodyFlags excludeBodyFlags = BodyFlags.CommonFocusRayCastExcludeFlags)
	{
		Vec3 closestPoint;
		return RayCastForClosestEntityOrTerrain(sourcePoint, targetPoint, out collisionDistance, out closestPoint, out collidedEntity, rayThickness, excludeBodyFlags);
	}

	public bool RayCastForClosestEntityOrTerrainMT(Vec3 sourcePoint, Vec3 targetPoint, out float collisionDistance, out GameEntity collidedEntity, float rayThickness = 0.01f, BodyFlags excludeBodyFlags = BodyFlags.CommonFocusRayCastExcludeFlags)
	{
		Vec3 closestPoint;
		return RayCastForClosestEntityOrTerrainMT(sourcePoint, targetPoint, out collisionDistance, out closestPoint, out collidedEntity, rayThickness, excludeBodyFlags);
	}

	public bool RayCastForClosestEntityOrTerrain(Vec3 sourcePoint, Vec3 targetPoint, out float collisionDistance, out Vec3 closestPoint, float rayThickness = 0.01f, BodyFlags excludeBodyFlags = BodyFlags.CommonFocusRayCastExcludeFlags)
	{
		collisionDistance = float.NaN;
		closestPoint = Vec3.Invalid;
		UIntPtr entityIndex = UIntPtr.Zero;
		return EngineApplicationInterface.IScene.RayCastForClosestEntityOrTerrain(base.Pointer, ref sourcePoint, ref targetPoint, rayThickness, ref collisionDistance, ref closestPoint, ref entityIndex, excludeBodyFlags);
	}

	public bool RayCastForClosestEntityOrTerrainMT(Vec3 sourcePoint, Vec3 targetPoint, out float collisionDistance, out Vec3 closestPoint, float rayThickness = 0.01f, BodyFlags excludeBodyFlags = BodyFlags.CommonFocusRayCastExcludeFlags)
	{
		collisionDistance = float.NaN;
		closestPoint = Vec3.Invalid;
		UIntPtr entityIndex = UIntPtr.Zero;
		return EngineApplicationInterface.IScene.RayCastForClosestEntityOrTerrain(base.Pointer, ref sourcePoint, ref targetPoint, rayThickness, ref collisionDistance, ref closestPoint, ref entityIndex, excludeBodyFlags);
	}

	public bool RayCastForClosestEntityOrTerrain(Vec3 sourcePoint, Vec3 targetPoint, out float collisionDistance, float rayThickness = 0.01f, BodyFlags excludeBodyFlags = BodyFlags.CommonFocusRayCastExcludeFlags)
	{
		Vec3 closestPoint;
		return RayCastForClosestEntityOrTerrain(sourcePoint, targetPoint, out collisionDistance, out closestPoint, rayThickness, excludeBodyFlags);
	}

	public bool RayCastForClosestEntityOrTerrainMT(Vec3 sourcePoint, Vec3 targetPoint, out float collisionDistance, float rayThickness = 0.01f, BodyFlags excludeBodyFlags = BodyFlags.CommonFocusRayCastExcludeFlags)
	{
		Vec3 closestPoint;
		return RayCastForClosestEntityOrTerrainMT(sourcePoint, targetPoint, out collisionDistance, out closestPoint, rayThickness, excludeBodyFlags);
	}

	public void ImportNavigationMeshPrefab(string navMeshPrefabName, int navMeshGroupShift)
	{
		EngineApplicationInterface.IScene.LoadNavMeshPrefab(base.Pointer, navMeshPrefabName, navMeshGroupShift);
	}

	public void MarkFacesWithIdAsLadder(int faceGroupId, bool isLadder)
	{
		EngineApplicationInterface.IScene.MarkFacesWithIdAsLadder(base.Pointer, faceGroupId, isLadder);
	}

	public void SetAbilityOfFacesWithId(int faceGroupId, bool isEnabled)
	{
		EngineApplicationInterface.IScene.SetAbilityOfFacesWithId(base.Pointer, faceGroupId, isEnabled);
	}

	public void SwapFaceConnectionsWithID(int hubFaceGroupID, int toBeSeparatedFaceGroupId, int toBeMergedFaceGroupId)
	{
		EngineApplicationInterface.IScene.SwapFaceConnectionsWithId(base.Pointer, hubFaceGroupID, toBeSeparatedFaceGroupId, toBeMergedFaceGroupId);
	}

	public void MergeFacesWithId(int faceGroupId0, int faceGroupId1, int newFaceGroupId)
	{
		EngineApplicationInterface.IScene.MergeFacesWithId(base.Pointer, faceGroupId0, faceGroupId1, newFaceGroupId);
	}

	public void SeparateFacesWithId(int faceGroupId0, int faceGroupId1)
	{
		EngineApplicationInterface.IScene.SeparateFacesWithId(base.Pointer, faceGroupId0, faceGroupId1);
	}

	public bool IsAnyFaceWithId(int faceGroupId)
	{
		return EngineApplicationInterface.IScene.IsAnyFaceWithId(base.Pointer, faceGroupId);
	}

	public bool GetNavigationMeshForPosition(ref Vec3 position)
	{
		int faceGroupId;
		return GetNavigationMeshForPosition(ref position, out faceGroupId);
	}

	public bool GetNavigationMeshForPosition(ref Vec3 position, out int faceGroupId, float heightDifferenceLimit = 1.5f)
	{
		faceGroupId = int.MinValue;
		return EngineApplicationInterface.IScene.GetNavigationMeshFaceForPosition(base.Pointer, ref position, ref faceGroupId, heightDifferenceLimit);
	}

	public bool DoesPathExistBetweenPositions(WorldPosition position, WorldPosition destination)
	{
		return EngineApplicationInterface.IScene.DoesPathExistBetweenPositions(base.Pointer, position, destination);
	}

	public void SetLandscapeRainMaskData(byte[] data)
	{
		EngineApplicationInterface.IScene.SetLandscapeRainMaskData(base.Pointer, data);
	}

	public void EnsurePostfxSystem()
	{
		EngineApplicationInterface.IScene.EnsurePostfxSystem(base.Pointer);
	}

	public void SetBloom(bool mode)
	{
		EngineApplicationInterface.IScene.SetBloom(base.Pointer, mode);
	}

	public void SetDofMode(bool mode)
	{
		EngineApplicationInterface.IScene.SetDofMode(base.Pointer, mode);
	}

	public void SetOcclusionMode(bool mode)
	{
		EngineApplicationInterface.IScene.SetOcclusionMode(base.Pointer, mode);
	}

	public void SetExternalInjectionTexture(Texture texture)
	{
		EngineApplicationInterface.IScene.SetExternalInjectionTexture(base.Pointer, texture.Pointer);
	}

	public void SetSunshaftMode(bool mode)
	{
		EngineApplicationInterface.IScene.SetSunshaftMode(base.Pointer, mode);
	}

	public Vec3 GetSunDirection()
	{
		return EngineApplicationInterface.IScene.GetSunDirection(base.Pointer);
	}

	public float GetNorthAngle()
	{
		return EngineApplicationInterface.IScene.GetNorthAngle(base.Pointer);
	}

	public float GetNorthRotation()
	{
		float northAngle = GetNorthAngle();
		return System.MathF.PI / 180f * (0f - northAngle);
	}

	public bool GetTerrainMinMaxHeight(out float minHeight, out float maxHeight)
	{
		minHeight = 0f;
		maxHeight = 0f;
		return EngineApplicationInterface.IScene.GetTerrainMinMaxHeight(this, ref minHeight, ref maxHeight);
	}

	public void GetPhysicsMinMax(ref Vec3 min_max)
	{
		EngineApplicationInterface.IScene.GetPhysicsMinMax(base.Pointer, ref min_max);
	}

	public bool IsEditorScene()
	{
		return EngineApplicationInterface.IScene.IsEditorScene(base.Pointer);
	}

	public void SetMotionBlurMode(bool mode)
	{
		EngineApplicationInterface.IScene.SetMotionBlurMode(base.Pointer, mode);
	}

	public void SetAntialiasingMode(bool mode)
	{
		EngineApplicationInterface.IScene.SetAntialiasingMode(base.Pointer, mode);
	}

	public void SetDLSSMode(bool mode)
	{
		EngineApplicationInterface.IScene.SetDLSSMode(base.Pointer, mode);
	}

	public IEnumerable<GameEntity> FindEntitiesWithTag(string tag)
	{
		return GameEntity.GetEntitiesWithTag(this, tag);
	}

	public GameEntity FindEntityWithTag(string tag)
	{
		return GameEntity.GetFirstEntityWithTag(this, tag);
	}

	public GameEntity FindEntityWithName(string name)
	{
		return GameEntity.GetFirstEntityWithName(this, name);
	}

	public IEnumerable<GameEntity> FindEntitiesWithTagExpression(string expression)
	{
		return GameEntity.GetEntitiesWithTagExpression(this, expression);
	}

	public int GetSoftBoundaryVertexCount()
	{
		return EngineApplicationInterface.IScene.GetSoftBoundaryVertexCount(base.Pointer);
	}

	public int GetHardBoundaryVertexCount()
	{
		return EngineApplicationInterface.IScene.GetHardBoundaryVertexCount(base.Pointer);
	}

	public Vec2 GetSoftBoundaryVertex(int index)
	{
		return EngineApplicationInterface.IScene.GetSoftBoundaryVertex(base.Pointer, index);
	}

	public Vec2 GetHardBoundaryVertex(int index)
	{
		return EngineApplicationInterface.IScene.GetHardBoundaryVertex(base.Pointer, index);
	}

	public Path GetPathWithName(string name)
	{
		return EngineApplicationInterface.IScene.GetPathWithName(base.Pointer, name);
	}

	public void DeletePathWithName(string name)
	{
		EngineApplicationInterface.IScene.DeletePathWithName(base.Pointer, name);
	}

	public void AddPath(string name)
	{
		EngineApplicationInterface.IScene.AddPath(base.Pointer, name);
	}

	public void AddPathPoint(string name, MatrixFrame frame)
	{
		EngineApplicationInterface.IScene.AddPathPoint(base.Pointer, name, ref frame);
	}

	public void GetBoundingBox(out Vec3 min, out Vec3 max)
	{
		min = Vec3.Invalid;
		max = Vec3.Invalid;
		EngineApplicationInterface.IScene.GetBoundingBox(base.Pointer, ref min, ref max);
	}

	public void SetName(string name)
	{
		EngineApplicationInterface.IScene.SetName(base.Pointer, name);
	}

	public string GetName()
	{
		return EngineApplicationInterface.IScene.GetName(base.Pointer);
	}

	public string GetModulePath()
	{
		return EngineApplicationInterface.IScene.GetModulePath(base.Pointer);
	}

	public void SetOwnerThread()
	{
		EngineApplicationInterface.IScene.SetOwnerThread(base.Pointer);
	}

	public Path[] GetPathsWithNamePrefix(string prefix)
	{
		int numberOfPathsWithNamePrefix = EngineApplicationInterface.IScene.GetNumberOfPathsWithNamePrefix(base.Pointer, prefix);
		UIntPtr[] array = new UIntPtr[numberOfPathsWithNamePrefix];
		EngineApplicationInterface.IScene.GetPathsWithNamePrefix(base.Pointer, array, prefix);
		Path[] array2 = new Path[numberOfPathsWithNamePrefix];
		for (int i = 0; i < numberOfPathsWithNamePrefix; i++)
		{
			UIntPtr pointer = array[i];
			array2[i] = new Path(pointer);
		}
		return array2;
	}

	public void SetUseConstantTime(bool value)
	{
		EngineApplicationInterface.IScene.SetUseConstantTime(base.Pointer, value);
	}

	public bool CheckPointCanSeePoint(Vec3 source, Vec3 target, float? distanceToCheck = null)
	{
		if (!distanceToCheck.HasValue)
		{
			distanceToCheck = source.Distance(target);
		}
		return EngineApplicationInterface.IScene.CheckPointCanSeePoint(base.Pointer, source, target, distanceToCheck.Value);
	}

	public void SetPlaySoundEventsAfterReadyToRender(bool value)
	{
		EngineApplicationInterface.IScene.SetPlaySoundEventsAfterReadyToRender(base.Pointer, value);
	}

	public void DisableStaticShadows(bool value)
	{
		EngineApplicationInterface.IScene.DisableStaticShadows(base.Pointer, value);
	}

	public Mesh GetSkyboxMesh()
	{
		return EngineApplicationInterface.IScene.GetSkyboxMesh(base.Pointer);
	}

	public void SetAtmosphereWithName(string name)
	{
		EngineApplicationInterface.IScene.SetAtmosphereWithName(base.Pointer, name);
	}

	public void FillEntityWithHardBorderPhysicsBarrier(GameEntity entity)
	{
		EngineApplicationInterface.IScene.FillEntityWithHardBorderPhysicsBarrier(base.Pointer, entity.Pointer);
	}

	public void ClearDecals()
	{
		EngineApplicationInterface.IScene.ClearDecals(base.Pointer);
	}
}
