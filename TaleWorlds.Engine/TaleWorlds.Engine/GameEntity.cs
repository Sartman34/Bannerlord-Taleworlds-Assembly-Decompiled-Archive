using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.DotNet;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[EngineClass("rglEntity")]
public sealed class GameEntity : NativeObject
{
	[EngineStruct("rglEntity_component_type", false)]
	public enum ComponentType : uint
	{
		MetaMesh,
		Light,
		CompositeComponent,
		ClothSimulator,
		ParticleSystemInstanced,
		TownIcon,
		CustomType1,
		Decal
	}

	public enum Mobility
	{
		stationary,
		dynamic,
		dynamic_forced
	}

	[Flags]
	public enum UpgradeLevelMask
	{
		None = 0,
		Level0 = 1,
		Level1 = 2,
		Level2 = 4,
		Level3 = 8,
		LevelAll = 0xF
	}

	public Scene Scene => EngineApplicationInterface.IGameEntity.GetScene(base.Pointer);

	private int ScriptCount => EngineApplicationInterface.IGameEntity.GetScriptComponentCount(base.Pointer);

	public string Name
	{
		get
		{
			return EngineApplicationInterface.IGameEntity.GetName(base.Pointer);
		}
		set
		{
			EngineApplicationInterface.IGameEntity.SetName(base.Pointer, value);
		}
	}

	public EntityFlags EntityFlags
	{
		get
		{
			return (EntityFlags)EngineApplicationInterface.IGameEntity.GetEntityFlags(base.Pointer);
		}
		set
		{
			EngineApplicationInterface.IGameEntity.SetEntityFlags(base.Pointer, (uint)value);
		}
	}

	public EntityVisibilityFlags EntityVisibilityFlags
	{
		get
		{
			return (EntityVisibilityFlags)EngineApplicationInterface.IGameEntity.GetEntityVisibilityFlags(base.Pointer);
		}
		set
		{
			EngineApplicationInterface.IGameEntity.SetEntityVisibilityFlags(base.Pointer, (uint)value);
		}
	}

	public BodyFlags BodyFlag
	{
		get
		{
			return (BodyFlags)EngineApplicationInterface.IGameEntity.GetBodyFlags(base.Pointer);
		}
		set
		{
			EngineApplicationInterface.IGameEntity.SetBodyFlags(base.Pointer, (uint)value);
		}
	}

	public BodyFlags PhysicsDescBodyFlag => (BodyFlags)EngineApplicationInterface.IGameEntity.GetPhysicsDescBodyFlags(base.Pointer);

	public float Mass => EngineApplicationInterface.IGameEntity.GetMass(base.Pointer);

	public Vec3 CenterOfMass => EngineApplicationInterface.IGameEntity.GetCenterOfMass(base.Pointer);

	public Vec3 GlobalPosition => GetGlobalFrame().origin;

	public string[] Tags => EngineApplicationInterface.IGameEntity.GetTags(base.Pointer).Split(new char[1] { ' ' });

	public int ChildCount => EngineApplicationInterface.IGameEntity.GetChildCount(base.Pointer);

	public GameEntity Parent => EngineApplicationInterface.IGameEntity.GetParent(base.Pointer);

	public GameEntity Root
	{
		get
		{
			GameEntity gameEntity = this;
			while (gameEntity.Parent != null)
			{
				gameEntity = gameEntity.Parent;
			}
			return gameEntity;
		}
	}

	public int MultiMeshComponentCount => EngineApplicationInterface.IGameEntity.GetComponentCount(base.Pointer, ComponentType.MetaMesh);

	public int ClothSimulatorComponentCount => EngineApplicationInterface.IGameEntity.GetComponentCount(base.Pointer, ComponentType.ClothSimulator);

	public Vec3 GlobalBoxMax => EngineApplicationInterface.IGameEntity.GetGlobalBoxMax(base.Pointer);

	public Vec3 PhysicsGlobalBoxMax => EngineApplicationInterface.IGameEntity.GetPhysicsBoundingBoxMax(base.Pointer);

	public Vec3 PhysicsGlobalBoxMin => EngineApplicationInterface.IGameEntity.GetPhysicsBoundingBoxMin(base.Pointer);

	public Vec3 GlobalBoxMin => EngineApplicationInterface.IGameEntity.GetGlobalBoxMin(base.Pointer);

	public Skeleton Skeleton
	{
		get
		{
			return EngineApplicationInterface.IGameEntity.GetSkeleton(base.Pointer);
		}
		set
		{
			EngineApplicationInterface.IGameEntity.SetSkeleton(base.Pointer, value?.Pointer ?? UIntPtr.Zero);
		}
	}

	private GameEntity()
	{
	}

	internal GameEntity(UIntPtr pointer)
	{
		Construct(pointer);
	}

	public UIntPtr GetScenePointer()
	{
		return EngineApplicationInterface.IGameEntity.GetScenePointer(base.Pointer);
	}

	public override string ToString()
	{
		return base.Pointer.ToString();
	}

	public void ClearEntityComponents(bool resetAll, bool removeScripts, bool deleteChildEntities)
	{
		EngineApplicationInterface.IGameEntity.ClearEntityComponents(base.Pointer, resetAll, removeScripts, deleteChildEntities);
	}

	public void ClearComponents()
	{
		EngineApplicationInterface.IGameEntity.ClearComponents(base.Pointer);
	}

	public void ClearOnlyOwnComponents()
	{
		EngineApplicationInterface.IGameEntity.ClearOnlyOwnComponents(base.Pointer);
	}

	public bool CheckResources(bool addToQueue, bool checkFaceResources)
	{
		return EngineApplicationInterface.IGameEntity.CheckResources(base.Pointer, addToQueue, checkFaceResources);
	}

	public void SetMobility(Mobility mobility)
	{
		EngineApplicationInterface.IGameEntity.SetMobility(base.Pointer, (int)mobility);
	}

	public void AddMesh(Mesh mesh, bool recomputeBoundingBox = true)
	{
		EngineApplicationInterface.IGameEntity.AddMesh(base.Pointer, mesh.Pointer, recomputeBoundingBox);
	}

	public void AddMultiMeshToSkeleton(MetaMesh metaMesh)
	{
		EngineApplicationInterface.IGameEntity.AddMultiMeshToSkeleton(base.Pointer, metaMesh.Pointer);
	}

	public void AddMultiMeshToSkeletonBone(MetaMesh metaMesh, sbyte boneIndex)
	{
		EngineApplicationInterface.IGameEntity.AddMultiMeshToSkeletonBone(base.Pointer, metaMesh.Pointer, boneIndex);
	}

	public IEnumerable<Mesh> GetAllMeshesWithTag(string tag)
	{
		List<GameEntity> children = new List<GameEntity>();
		GetChildrenRecursive(ref children);
		children.Add(this);
		foreach (GameEntity entity in children)
		{
			for (int j = 0; j < entity.MultiMeshComponentCount; j++)
			{
				MetaMesh multiMesh2 = entity.GetMetaMesh(j);
				for (int l = 0; l < multiMesh2.MeshCount; l++)
				{
					Mesh meshAtIndex = multiMesh2.GetMeshAtIndex(l);
					if (meshAtIndex.HasTag(tag))
					{
						yield return meshAtIndex;
					}
				}
			}
			for (int j = 0; j < entity.ClothSimulatorComponentCount; j++)
			{
				ClothSimulatorComponent clothSimulator = entity.GetClothSimulator(j);
				MetaMesh multiMesh2 = clothSimulator.GetFirstMetaMesh();
				for (int l = 0; l < multiMesh2.MeshCount; l++)
				{
					Mesh meshAtIndex2 = multiMesh2.GetMeshAtIndex(l);
					if (meshAtIndex2.HasTag(tag))
					{
						yield return meshAtIndex2;
					}
				}
			}
		}
	}

	public void SetColor(uint color1, uint color2, string meshTag)
	{
		foreach (Mesh item in GetAllMeshesWithTag(meshTag))
		{
			item.Color = color1;
			item.Color2 = color2;
		}
	}

	public uint GetFactorColor()
	{
		return EngineApplicationInterface.IGameEntity.GetFactorColor(base.Pointer);
	}

	public void SetFactorColor(uint color)
	{
		EngineApplicationInterface.IGameEntity.SetFactorColor(base.Pointer, color);
	}

	public void SetAsReplayEntity()
	{
		EngineApplicationInterface.IGameEntity.SetAsReplayEntity(base.Pointer);
	}

	public void SetClothMaxDistanceMultiplier(float multiplier)
	{
		EngineApplicationInterface.IGameEntity.SetClothMaxDistanceMultiplier(base.Pointer, multiplier);
	}

	public void RemoveMultiMeshFromSkeleton(MetaMesh metaMesh)
	{
		EngineApplicationInterface.IGameEntity.RemoveMultiMeshFromSkeleton(base.Pointer, metaMesh.Pointer);
	}

	public void RemoveMultiMeshFromSkeletonBone(MetaMesh metaMesh, sbyte boneIndex)
	{
		EngineApplicationInterface.IGameEntity.RemoveMultiMeshFromSkeletonBone(base.Pointer, metaMesh.Pointer, boneIndex);
	}

	public bool RemoveComponentWithMesh(Mesh mesh)
	{
		return EngineApplicationInterface.IGameEntity.RemoveComponentWithMesh(base.Pointer, mesh.Pointer);
	}

	public void AddComponent(GameEntityComponent component)
	{
		EngineApplicationInterface.IGameEntity.AddComponent(base.Pointer, component.Pointer);
	}

	public bool HasComponent(GameEntityComponent component)
	{
		return EngineApplicationInterface.IGameEntity.HasComponent(base.Pointer, component.Pointer);
	}

	public bool RemoveComponent(GameEntityComponent component)
	{
		return EngineApplicationInterface.IGameEntity.RemoveComponent(base.Pointer, component.Pointer);
	}

	public string GetGuid()
	{
		return EngineApplicationInterface.IGameEntity.GetGuid(base.Pointer);
	}

	public bool IsGuidValid()
	{
		return EngineApplicationInterface.IGameEntity.IsGuidValid(base.Pointer);
	}

	public void SetEnforcedMaximumLodLevel(int lodLevel)
	{
		EngineApplicationInterface.IGameEntity.SetEnforcedMaximumLodLevel(base.Pointer, lodLevel);
	}

	public float GetLodLevelForDistanceSq(float distSq)
	{
		return EngineApplicationInterface.IGameEntity.GetLodLevelForDistanceSq(base.Pointer, distSq);
	}

	public void GetQuickBoneEntitialFrame(sbyte index, ref MatrixFrame frame)
	{
		EngineApplicationInterface.IGameEntity.GetQuickBoneEntitialFrame(base.Pointer, index, ref frame);
	}

	public void UpdateVisibilityMask()
	{
		EngineApplicationInterface.IGameEntity.UpdateVisibilityMask(base.Pointer);
	}

	public static GameEntity CreateEmpty(Scene scene, bool isModifiableFromEditor = true)
	{
		return EngineApplicationInterface.IGameEntity.CreateEmpty(scene.Pointer, isModifiableFromEditor, (UIntPtr)0uL);
	}

	public static GameEntity CreateEmptyDynamic(Scene scene, bool isModifiableFromEditor = true)
	{
		GameEntity gameEntity = CreateEmpty(scene, isModifiableFromEditor);
		gameEntity.SetMobility(Mobility.dynamic);
		return gameEntity;
	}

	public static GameEntity CreateEmptyWithoutScene()
	{
		return EngineApplicationInterface.IGameEntity.CreateEmptyWithoutScene();
	}

	public static GameEntity CopyFrom(Scene scene, GameEntity entity)
	{
		return EngineApplicationInterface.IGameEntity.CreateEmpty(scene.Pointer, isModifiableFromEditor: false, entity.Pointer);
	}

	public static GameEntity Instantiate(Scene scene, string prefabName, bool callScriptCallbacks)
	{
		if (scene != null)
		{
			return EngineApplicationInterface.IGameEntity.CreateFromPrefab(scene.Pointer, prefabName, callScriptCallbacks);
		}
		return EngineApplicationInterface.IGameEntity.CreateFromPrefab(new UIntPtr(0u), prefabName, callScriptCallbacks);
	}

	public void CallScriptCallbacks()
	{
		EngineApplicationInterface.IGameEntity.CallScriptCallbacks(base.Pointer);
	}

	public static GameEntity Instantiate(Scene scene, string prefabName, MatrixFrame frame)
	{
		return EngineApplicationInterface.IGameEntity.CreateFromPrefabWithInitialFrame(scene.Pointer, prefabName, ref frame);
	}

	public bool IsGhostObject()
	{
		return EngineApplicationInterface.IGameEntity.IsGhostObject(base.Pointer);
	}

	public void CreateAndAddScriptComponent(string name)
	{
		EngineApplicationInterface.IGameEntity.CreateAndAddScriptComponent(base.Pointer, name);
	}

	public static bool PrefabExists(string name)
	{
		return EngineApplicationInterface.IGameEntity.PrefabExists(name);
	}

	public void RemoveScriptComponent(UIntPtr scriptComponent, int removeReason)
	{
		EngineApplicationInterface.IGameEntity.RemoveScriptComponent(base.Pointer, scriptComponent, removeReason);
	}

	public void SetEntityEnvMapVisibility(bool value)
	{
		EngineApplicationInterface.IGameEntity.SetEntityEnvMapVisibility(base.Pointer, value);
	}

	internal ScriptComponentBehavior GetScriptAtIndex(int index)
	{
		return EngineApplicationInterface.IGameEntity.GetScriptComponentAtIndex(base.Pointer, index);
	}

	public bool HasScene()
	{
		return EngineApplicationInterface.IGameEntity.HasScene(base.Pointer);
	}

	public bool HasScriptComponent(string scName)
	{
		return EngineApplicationInterface.IGameEntity.HasScriptComponent(base.Pointer, scName);
	}

	public IEnumerable<ScriptComponentBehavior> GetScriptComponents()
	{
		int count = ScriptCount;
		for (int i = 0; i < count; i++)
		{
			yield return GetScriptAtIndex(i);
		}
	}

	public IEnumerable<T> GetScriptComponents<T>() where T : ScriptComponentBehavior
	{
		int count = ScriptCount;
		for (int i = 0; i < count; i++)
		{
			if (GetScriptAtIndex(i) is T val)
			{
				yield return val;
			}
		}
	}

	public bool HasScriptOfType<T>() where T : ScriptComponentBehavior
	{
		int scriptCount = ScriptCount;
		for (int i = 0; i < scriptCount; i++)
		{
			if (GetScriptAtIndex(i) is T)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasScriptOfType(Type t)
	{
		return GetScriptComponents().Any((ScriptComponentBehavior sc) => sc.GetType().IsAssignableFrom(t));
	}

	public T GetFirstScriptOfTypeInFamily<T>() where T : ScriptComponentBehavior
	{
		T firstScriptOfType = GetFirstScriptOfType<T>();
		GameEntity gameEntity = this;
		while (firstScriptOfType == null && gameEntity.Parent != null)
		{
			gameEntity = gameEntity.Parent;
			firstScriptOfType = gameEntity.GetFirstScriptOfType<T>();
		}
		return firstScriptOfType;
	}

	public T GetFirstScriptOfType<T>() where T : ScriptComponentBehavior
	{
		int scriptCount = ScriptCount;
		for (int i = 0; i < scriptCount; i++)
		{
			if (GetScriptAtIndex(i) is T result)
			{
				return result;
			}
		}
		return null;
	}

	internal static GameEntity GetFirstEntityWithName(Scene scene, string entityName)
	{
		return EngineApplicationInterface.IGameEntity.FindWithName(scene.Pointer, entityName);
	}

	public void SetAlpha(float alpha)
	{
		EngineApplicationInterface.IGameEntity.SetAlpha(base.Pointer, alpha);
	}

	public void SetVisibilityExcludeParents(bool visible)
	{
		EngineApplicationInterface.IGameEntity.SetVisibilityExcludeParents(base.Pointer, visible);
	}

	public void SetReadyToRender(bool ready)
	{
		EngineApplicationInterface.IGameEntity.SetReadyToRender(base.Pointer, ready);
	}

	public bool GetVisibilityExcludeParents()
	{
		return EngineApplicationInterface.IGameEntity.GetVisibilityExcludeParents(base.Pointer);
	}

	public bool IsVisibleIncludeParents()
	{
		return EngineApplicationInterface.IGameEntity.IsVisibleIncludeParents(base.Pointer);
	}

	public uint GetVisibilityLevelMaskIncludingParents()
	{
		return EngineApplicationInterface.IGameEntity.GetVisibilityLevelMaskIncludingParents(base.Pointer);
	}

	public bool GetEditModeLevelVisibility()
	{
		return EngineApplicationInterface.IGameEntity.GetEditModeLevelVisibility(base.Pointer);
	}

	public void Remove(int removeReason)
	{
		EngineApplicationInterface.IGameEntity.Remove(base.Pointer, removeReason);
	}

	internal static GameEntity GetFirstEntityWithTag(Scene scene, string tag)
	{
		return EngineApplicationInterface.IGameEntity.GetFirstEntityWithTag(scene.Pointer, tag);
	}

	internal static GameEntity GetNextEntityWithTag(Scene scene, GameEntity startEntity, string tag)
	{
		if (!(startEntity == null))
		{
			return EngineApplicationInterface.IGameEntity.GetNextEntityWithTag(startEntity.Pointer, tag);
		}
		return GetFirstEntityWithTag(scene, tag);
	}

	internal static GameEntity GetFirstEntityWithTagExpression(Scene scene, string tagExpression)
	{
		return EngineApplicationInterface.IGameEntity.GetFirstEntityWithTagExpression(scene.Pointer, tagExpression);
	}

	internal static GameEntity GetNextEntityWithTagExpression(Scene scene, GameEntity startEntity, string tagExpression)
	{
		if (!(startEntity == null))
		{
			return EngineApplicationInterface.IGameEntity.GetNextEntityWithTagExpression(startEntity.Pointer, tagExpression);
		}
		return GetFirstEntityWithTagExpression(scene, tagExpression);
	}

	internal static GameEntity GetNextPrefab(GameEntity current)
	{
		return EngineApplicationInterface.IGameEntity.GetNextPrefab((current == null) ? new UIntPtr(0u) : current.Pointer);
	}

	public static GameEntity CopyFromPrefab(GameEntity prefab)
	{
		if (!(prefab != null))
		{
			return null;
		}
		return EngineApplicationInterface.IGameEntity.CopyFromPrefab(prefab.Pointer);
	}

	public void SetUpgradeLevelMask(UpgradeLevelMask mask)
	{
		EngineApplicationInterface.IGameEntity.SetUpgradeLevelMask(base.Pointer, (uint)mask);
	}

	public UpgradeLevelMask GetUpgradeLevelMask()
	{
		return (UpgradeLevelMask)EngineApplicationInterface.IGameEntity.GetUpgradeLevelMask(base.Pointer);
	}

	public UpgradeLevelMask GetUpgradeLevelMaskCumulative()
	{
		return (UpgradeLevelMask)EngineApplicationInterface.IGameEntity.GetUpgradeLevelMaskCumulative(base.Pointer);
	}

	public int GetUpgradeLevelOfEntity()
	{
		int upgradeLevelMask = (int)GetUpgradeLevelMask();
		if ((upgradeLevelMask & 1) > 0)
		{
			return 0;
		}
		if ((upgradeLevelMask & 2) > 0)
		{
			return 1;
		}
		if ((upgradeLevelMask & 4) > 0)
		{
			return 2;
		}
		if ((upgradeLevelMask & 8) > 0)
		{
			return 3;
		}
		return -1;
	}

	public string GetOldPrefabName()
	{
		return EngineApplicationInterface.IGameEntity.GetOldPrefabName(base.Pointer);
	}

	public string GetPrefabName()
	{
		return EngineApplicationInterface.IGameEntity.GetPrefabName(base.Pointer);
	}

	public void CopyScriptComponentFromAnotherEntity(GameEntity otherEntity, string scriptName)
	{
		EngineApplicationInterface.IGameEntity.CopyScriptComponentFromAnotherEntity(base.Pointer, otherEntity.Pointer, scriptName);
	}

	internal static IEnumerable<GameEntity> GetEntitiesWithTag(Scene scene, string tag)
	{
		GameEntity entity = GetFirstEntityWithTag(scene, tag);
		while (entity != null)
		{
			yield return entity;
			entity = GetNextEntityWithTag(scene, entity, tag);
		}
	}

	internal static IEnumerable<GameEntity> GetEntitiesWithTagExpression(Scene scene, string tagExpression)
	{
		GameEntity entity = GetFirstEntityWithTagExpression(scene, tagExpression);
		while (entity != null)
		{
			yield return entity;
			entity = GetNextEntityWithTagExpression(scene, entity, tagExpression);
		}
	}

	public void SetFrame(ref MatrixFrame frame)
	{
		EngineApplicationInterface.IGameEntity.SetFrame(base.Pointer, ref frame);
	}

	public void SetClothComponentKeepState(MetaMesh metaMesh, bool state)
	{
		EngineApplicationInterface.IGameEntity.SetClothComponentKeepState(base.Pointer, metaMesh.Pointer, state);
	}

	public void SetClothComponentKeepStateOfAllMeshes(bool state)
	{
		EngineApplicationInterface.IGameEntity.SetClothComponentKeepStateOfAllMeshes(base.Pointer, state);
	}

	public void SetPreviousFrameInvalid()
	{
		EngineApplicationInterface.IGameEntity.SetPreviousFrameInvalid(base.Pointer);
	}

	public MatrixFrame GetFrame()
	{
		MatrixFrame outFrame = default(MatrixFrame);
		EngineApplicationInterface.IGameEntity.GetFrame(base.Pointer, ref outFrame);
		return outFrame;
	}

	public void GetFrame(out MatrixFrame frame)
	{
		frame = GetFrame();
	}

	public void UpdateTriadFrameForEditor()
	{
		EngineApplicationInterface.IGameEntity.UpdateTriadFrameForEditor(base.Pointer);
	}

	public void UpdateTriadFrameForEditorForAllChildren()
	{
		UpdateTriadFrameForEditor();
		List<GameEntity> children = new List<GameEntity>();
		GetChildrenRecursive(ref children);
		foreach (GameEntity item in children)
		{
			EngineApplicationInterface.IGameEntity.UpdateTriadFrameForEditor(item.Pointer);
		}
	}

	public MatrixFrame GetGlobalFrame()
	{
		EngineApplicationInterface.IGameEntity.GetGlobalFrame(base.Pointer, out var outFrame);
		return outFrame;
	}

	public void SetGlobalFrame(in MatrixFrame frame)
	{
		EngineApplicationInterface.IGameEntity.SetGlobalFrame(base.Pointer, in frame);
	}

	public void SetGlobalFrameMT(in MatrixFrame frame)
	{
		using (new TWSharedMutexReadLock(Scene.PhysicsAndRayCastLock))
		{
			EngineApplicationInterface.IGameEntity.SetGlobalFrame(base.Pointer, in frame);
		}
	}

	public MatrixFrame GetPreviousGlobalFrame()
	{
		EngineApplicationInterface.IGameEntity.GetPreviousGlobalFrame(base.Pointer, out var frame);
		return frame;
	}

	public bool HasPhysicsBody()
	{
		return EngineApplicationInterface.IGameEntity.HasPhysicsBody(base.Pointer);
	}

	public void SetLocalPosition(Vec3 position)
	{
		EngineApplicationInterface.IGameEntity.SetLocalPosition(base.Pointer, position);
	}

	public void SetBodyFlags(BodyFlags bodyFlags)
	{
		BodyFlag = bodyFlags;
	}

	public void SetBodyFlagsRecursive(BodyFlags bodyFlags)
	{
		EngineApplicationInterface.IGameEntity.SetBodyFlagsRecursive(base.Pointer, (uint)bodyFlags);
	}

	public void AddBodyFlags(BodyFlags bodyFlags, bool applyToChildren = true)
	{
		BodyFlag |= bodyFlags;
		if (!applyToChildren)
		{
			return;
		}
		foreach (GameEntity child in GetChildren())
		{
			child.AddBodyFlags(bodyFlags);
		}
	}

	public void RemoveBodyFlags(BodyFlags bodyFlags, bool applyToChildren = true)
	{
		BodyFlag &= ~bodyFlags;
		if (!applyToChildren)
		{
			return;
		}
		foreach (GameEntity child in GetChildren())
		{
			child.RemoveBodyFlags(bodyFlags);
		}
	}

	public Vec3 GetGlobalScale()
	{
		return EngineApplicationInterface.IGameEntity.GetGlobalScale(this);
	}

	public Vec3 GetLocalScale()
	{
		return GetFrame().rotation.GetScaleVector();
	}

	public void SetAnimationSoundActivation(bool activate)
	{
		EngineApplicationInterface.IGameEntity.SetAnimationSoundActivation(base.Pointer, activate);
		foreach (GameEntity child in GetChildren())
		{
			child.SetAnimationSoundActivation(activate);
		}
	}

	public void CopyComponentsToSkeleton()
	{
		EngineApplicationInterface.IGameEntity.CopyComponentsToSkeleton(base.Pointer);
	}

	public void AddMeshToBone(sbyte boneIndex, Mesh mesh)
	{
		EngineApplicationInterface.IGameEntity.AddMeshToBone(base.Pointer, mesh.Pointer, boneIndex);
	}

	public void ActivateRagdoll()
	{
		EngineApplicationInterface.IGameEntity.ActivateRagdoll(base.Pointer);
	}

	public void PauseSkeletonAnimation()
	{
		EngineApplicationInterface.IGameEntity.Freeze(base.Pointer, isFrozen: true);
	}

	public void ResumeSkeletonAnimation()
	{
		EngineApplicationInterface.IGameEntity.Freeze(base.Pointer, isFrozen: false);
	}

	public bool IsSkeletonAnimationPaused()
	{
		return EngineApplicationInterface.IGameEntity.IsFrozen(base.Pointer);
	}

	public sbyte GetBoneCount()
	{
		return EngineApplicationInterface.IGameEntity.GetBoneCount(base.Pointer);
	}

	public MatrixFrame GetBoneEntitialFrameWithIndex(sbyte boneIndex)
	{
		MatrixFrame outEntitialFrame = default(MatrixFrame);
		EngineApplicationInterface.IGameEntity.GetBoneEntitialFrameWithIndex(base.Pointer, boneIndex, ref outEntitialFrame);
		return outEntitialFrame;
	}

	public MatrixFrame GetBoneEntitialFrameWithName(string boneName)
	{
		MatrixFrame outEntitialFrame = default(MatrixFrame);
		EngineApplicationInterface.IGameEntity.GetBoneEntitialFrameWithName(base.Pointer, boneName, ref outEntitialFrame);
		return outEntitialFrame;
	}

	public void AddTag(string tag)
	{
		EngineApplicationInterface.IGameEntity.AddTag(base.Pointer, tag);
	}

	public void RemoveTag(string tag)
	{
		EngineApplicationInterface.IGameEntity.RemoveTag(base.Pointer, tag);
	}

	public bool HasTag(string tag)
	{
		return EngineApplicationInterface.IGameEntity.HasTag(base.Pointer, tag);
	}

	public void AddChild(GameEntity gameEntity, bool autoLocalizeFrame = false)
	{
		EngineApplicationInterface.IGameEntity.AddChild(base.Pointer, gameEntity.Pointer, autoLocalizeFrame);
	}

	public void RemoveChild(GameEntity childEntity, bool keepPhysics, bool keepScenePointer, bool callScriptCallbacks, int removeReason)
	{
		EngineApplicationInterface.IGameEntity.RemoveChild(base.Pointer, childEntity.Pointer, keepPhysics, keepScenePointer, callScriptCallbacks, removeReason);
	}

	public void BreakPrefab()
	{
		EngineApplicationInterface.IGameEntity.BreakPrefab(base.Pointer);
	}

	public GameEntity GetChild(int index)
	{
		return EngineApplicationInterface.IGameEntity.GetChild(base.Pointer, index);
	}

	public bool HasComplexAnimTree()
	{
		return EngineApplicationInterface.IGameEntity.HasComplexAnimTree(base.Pointer);
	}

	public void AddMultiMesh(MetaMesh metaMesh, bool updateVisMask = true)
	{
		EngineApplicationInterface.IGameEntity.AddMultiMesh(base.Pointer, metaMesh.Pointer, updateVisMask);
	}

	public bool RemoveMultiMesh(MetaMesh metaMesh)
	{
		return EngineApplicationInterface.IGameEntity.RemoveMultiMesh(base.Pointer, metaMesh.Pointer);
	}

	public int GetComponentCount(ComponentType componentType)
	{
		return EngineApplicationInterface.IGameEntity.GetComponentCount(base.Pointer, componentType);
	}

	public void AddAllMeshesOfGameEntity(GameEntity gameEntity)
	{
		EngineApplicationInterface.IGameEntity.AddAllMeshesOfGameEntity(base.Pointer, gameEntity.Pointer);
	}

	public void SetFrameChanged()
	{
		EngineApplicationInterface.IGameEntity.SetFrameChanged(base.Pointer);
	}

	public GameEntityComponent GetComponentAtIndex(int index, ComponentType componentType)
	{
		return EngineApplicationInterface.IGameEntity.GetComponentAtIndex(base.Pointer, componentType, index);
	}

	public MetaMesh GetMetaMesh(int metaMeshIndex)
	{
		return (MetaMesh)EngineApplicationInterface.IGameEntity.GetComponentAtIndex(base.Pointer, ComponentType.MetaMesh, metaMeshIndex);
	}

	public ClothSimulatorComponent GetClothSimulator(int clothSimulatorIndex)
	{
		return (ClothSimulatorComponent)EngineApplicationInterface.IGameEntity.GetComponentAtIndex(base.Pointer, ComponentType.ClothSimulator, clothSimulatorIndex);
	}

	public void SetVectorArgument(float vectorArgument0, float vectorArgument1, float vectorArgument2, float vectorArgument3)
	{
		EngineApplicationInterface.IGameEntity.SetVectorArgument(base.Pointer, vectorArgument0, vectorArgument1, vectorArgument2, vectorArgument3);
	}

	public void SetMaterialForAllMeshes(Material material)
	{
		EngineApplicationInterface.IGameEntity.SetMaterialForAllMeshes(base.Pointer, material.Pointer);
	}

	public bool AddLight(Light light)
	{
		return EngineApplicationInterface.IGameEntity.AddLight(base.Pointer, light.Pointer);
	}

	public Light GetLight()
	{
		return EngineApplicationInterface.IGameEntity.GetLight(base.Pointer);
	}

	public void AddParticleSystemComponent(string particleid)
	{
		EngineApplicationInterface.IGameEntity.AddParticleSystemComponent(base.Pointer, particleid);
	}

	public void RemoveAllParticleSystems()
	{
		EngineApplicationInterface.IGameEntity.RemoveAllParticleSystems(base.Pointer);
	}

	public bool CheckPointWithOrientedBoundingBox(Vec3 point)
	{
		return EngineApplicationInterface.IGameEntity.CheckPointWithOrientedBoundingBox(base.Pointer, point);
	}

	public void PauseParticleSystem(bool doChildren)
	{
		EngineApplicationInterface.IGameEntity.PauseParticleSystem(base.Pointer, doChildren);
	}

	public void ResumeParticleSystem(bool doChildren)
	{
		EngineApplicationInterface.IGameEntity.ResumeParticleSystem(base.Pointer, doChildren);
	}

	public void BurstEntityParticle(bool doChildren)
	{
		EngineApplicationInterface.IGameEntity.BurstEntityParticle(base.Pointer, doChildren);
	}

	public void SetRuntimeEmissionRateMultiplier(float emissionRateMultiplier)
	{
		EngineApplicationInterface.IGameEntity.SetRuntimeEmissionRateMultiplier(base.Pointer, emissionRateMultiplier);
	}

	public Vec3 GetBoundingBoxMin()
	{
		return EngineApplicationInterface.IGameEntity.GetBoundingBoxMin(base.Pointer);
	}

	public float GetBoundingBoxRadius()
	{
		return EngineApplicationInterface.IGameEntity.GetRadius(base.Pointer);
	}

	public Vec3 GetBoundingBoxMax()
	{
		return EngineApplicationInterface.IGameEntity.GetBoundingBoxMax(base.Pointer);
	}

	public Vec3 GetPhysicsBoundingBoxMax()
	{
		return EngineApplicationInterface.IGameEntity.GetPhysicsBoundingBoxMax(base.Pointer);
	}

	public Vec3 GetPhysicsBoundingBoxMin()
	{
		return EngineApplicationInterface.IGameEntity.GetPhysicsBoundingBoxMin(base.Pointer);
	}

	public void UpdateGlobalBounds()
	{
		EngineApplicationInterface.IGameEntity.UpdateGlobalBounds(base.Pointer);
	}

	public void RecomputeBoundingBox()
	{
		EngineApplicationInterface.IGameEntity.RecomputeBoundingBox(this);
	}

	public void ValidateBoundingBox()
	{
		EngineApplicationInterface.IGameEntity.ValidateBoundingBox(base.Pointer);
	}

	public bool GetHasFrameChanged()
	{
		return EngineApplicationInterface.IGameEntity.HasFrameChanged(base.Pointer);
	}

	public void SetBoundingboxDirty()
	{
		EngineApplicationInterface.IGameEntity.SetBoundingboxDirty(base.Pointer);
	}

	public Mesh GetFirstMesh()
	{
		return EngineApplicationInterface.IGameEntity.GetFirstMesh(base.Pointer);
	}

	public void SetContourColor(uint? color, bool alwaysVisible = true)
	{
		if (color.HasValue)
		{
			EngineApplicationInterface.IGameEntity.SetAsContourEntity(base.Pointer, color.Value);
			EngineApplicationInterface.IGameEntity.SetContourState(base.Pointer, alwaysVisible);
		}
		else
		{
			EngineApplicationInterface.IGameEntity.DisableContour(base.Pointer);
		}
	}

	public void SetExternalReferencesUsage(bool value)
	{
		EngineApplicationInterface.IGameEntity.SetExternalReferencesUsage(base.Pointer, value);
	}

	public void SetMorphFrameOfComponents(float value)
	{
		EngineApplicationInterface.IGameEntity.SetMorphFrameOfComponents(base.Pointer, value);
	}

	public void AddEditDataUserToAllMeshes(bool entityComponents, bool skeletonComponents)
	{
		EngineApplicationInterface.IGameEntity.AddEditDataUserToAllMeshes(base.Pointer, entityComponents, skeletonComponents);
	}

	public void ReleaseEditDataUserToAllMeshes(bool entityComponents, bool skeletonComponents)
	{
		EngineApplicationInterface.IGameEntity.ReleaseEditDataUserToAllMeshes(base.Pointer, entityComponents, skeletonComponents);
	}

	public void GetCameraParamsFromCameraScript(Camera cam, ref Vec3 dofParams)
	{
		EngineApplicationInterface.IGameEntity.GetCameraParamsFromCameraScript(base.Pointer, cam.Pointer, ref dofParams);
	}

	public void GetMeshBendedFrame(MatrixFrame worldSpacePosition, ref MatrixFrame output)
	{
		EngineApplicationInterface.IGameEntity.GetMeshBendedPosition(base.Pointer, ref worldSpacePosition, ref output);
	}

	public void ComputeTrajectoryVolume(float missileSpeed, float verticalAngleMaxInDegrees, float verticalAngleMinInDegrees, float horizontalAngleRangeInDegrees, float airFrictionConstant)
	{
		EngineApplicationInterface.IGameEntity.ComputeTrajectoryVolume(base.Pointer, missileSpeed, verticalAngleMaxInDegrees, verticalAngleMinInDegrees, horizontalAngleRangeInDegrees, airFrictionConstant);
	}

	public void SetAnimTreeChannelParameterForceUpdate(float phase, int channelNo)
	{
		EngineApplicationInterface.IGameEntity.SetAnimTreeChannelParameter(base.Pointer, phase, channelNo);
	}

	public void ChangeMetaMeshOrRemoveItIfNotExists(MetaMesh entityMetaMesh, MetaMesh newMetaMesh)
	{
		EngineApplicationInterface.IGameEntity.ChangeMetaMeshOrRemoveItIfNotExists(base.Pointer, (entityMetaMesh != null) ? entityMetaMesh.Pointer : UIntPtr.Zero, (newMetaMesh != null) ? newMetaMesh.Pointer : UIntPtr.Zero);
	}

	public void AttachNavigationMeshFaces(int faceGroupId, bool isConnected, bool isBlocker = false, bool autoLocalize = false)
	{
		EngineApplicationInterface.IGameEntity.AttachNavigationMeshFaces(base.Pointer, faceGroupId, isConnected, isBlocker, autoLocalize);
	}

	public void RemoveSkeleton()
	{
		Skeleton = null;
	}

	public void RemoveAllChildren()
	{
		EngineApplicationInterface.IGameEntity.RemoveAllChildren(base.Pointer);
	}

	public IEnumerable<GameEntity> GetChildren()
	{
		int count = ChildCount;
		for (int i = 0; i < count; i++)
		{
			yield return GetChild(i);
		}
	}

	public IEnumerable<GameEntity> GetEntityAndChildren()
	{
		yield return this;
		int count = ChildCount;
		for (int i = 0; i < count; i++)
		{
			yield return GetChild(i);
		}
	}

	public void GetChildrenRecursive(ref List<GameEntity> children)
	{
		int childCount = ChildCount;
		for (int i = 0; i < childCount; i++)
		{
			GameEntity child = GetChild(i);
			children.Add(child);
			child.GetChildrenRecursive(ref children);
		}
	}

	public bool IsSelectedOnEditor()
	{
		return EngineApplicationInterface.IGameEntity.IsEntitySelectedOnEditor(base.Pointer);
	}

	public void SelectEntityOnEditor()
	{
		EngineApplicationInterface.IGameEntity.SelectEntityOnEditor(base.Pointer);
	}

	public void DeselectEntityOnEditor()
	{
		EngineApplicationInterface.IGameEntity.DeselectEntityOnEditor(base.Pointer);
	}

	public void SetAsPredisplayEntity()
	{
		EngineApplicationInterface.IGameEntity.SetAsPredisplayEntity(base.Pointer);
	}

	public void RemoveFromPredisplayEntity()
	{
		EngineApplicationInterface.IGameEntity.RemoveFromPredisplayEntity(base.Pointer);
	}

	public void GetPhysicsMinMax(bool includeChildren, out Vec3 bbmin, out Vec3 bbmax, bool returnLocal)
	{
		bbmin = Vec3.Zero;
		bbmax = Vec3.Zero;
		EngineApplicationInterface.IGameEntity.GetPhysicsMinMax(base.Pointer, includeChildren, ref bbmin, ref bbmax, returnLocal);
	}

	public void SetCullMode(MBMeshCullingMode cullMode)
	{
		EngineApplicationInterface.IGameEntity.SetCullMode(base.Pointer, cullMode);
	}
}
