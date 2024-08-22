using System;
using System.Diagnostics;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

public static class GameEntityPhysicsExtensions
{
	[Conditional("_RGL_KEEP_ASSERTS")]
	private static void AssertSingleThreadRead()
	{
	}

	[Conditional("_RGL_KEEP_ASSERTS")]
	private static void AssertSingleThreadWrite()
	{
	}

	[Conditional("_RGL_KEEP_ASSERTS")]
	private static void AssertMultiThreadRead()
	{
	}

	[Conditional("_RGL_KEEP_ASSERTS")]
	private static void AssertMultiThreadWrite()
	{
	}

	public static bool HasBody(this GameEntity gameEntity)
	{
		return EngineApplicationInterface.IGameEntity.HasBody(gameEntity.Pointer);
	}

	public static void AddSphereAsBody(this GameEntity gameEntity, Vec3 sphere, float radius, BodyFlags bodyFlags)
	{
		EngineApplicationInterface.IGameEntity.AddSphereAsBody(gameEntity.Pointer, sphere, radius, (uint)bodyFlags);
	}

	public static void RemovePhysics(this GameEntity gameEntity, bool clearingTheScene = false)
	{
		EngineApplicationInterface.IGameEntity.RemovePhysics(gameEntity.Pointer, clearingTheScene);
	}

	public static void RemovePhysicsMT(this GameEntity gameEntity, bool clearingTheScene = false)
	{
		EngineApplicationInterface.IGameEntity.RemovePhysics(gameEntity.Pointer, clearingTheScene);
	}

	public static bool GetPhysicsState(this GameEntity gameEntity)
	{
		return EngineApplicationInterface.IGameEntity.GetPhysicsState(gameEntity.Pointer);
	}

	public static void AddDistanceJoint(this GameEntity gameEntity, GameEntity otherGameEntity, float minDistance, float maxDistance)
	{
		EngineApplicationInterface.IGameEntity.AddDistanceJoint(gameEntity.Pointer, otherGameEntity.Pointer, minDistance, maxDistance);
	}

	public static bool HasPhysicsDefinitionWithoutFlags(this GameEntity gameEntity, int excludeFlags)
	{
		return EngineApplicationInterface.IGameEntity.HasPhysicsDefinition(gameEntity.Pointer, excludeFlags);
	}

	public static void SetPhysicsState(this GameEntity gameEntity, bool isEnabled, bool setChildren)
	{
		EngineApplicationInterface.IGameEntity.SetPhysicsState(gameEntity.Pointer, isEnabled, setChildren);
	}

	public static void RemoveEnginePhysics(this GameEntity gameEntity)
	{
		EngineApplicationInterface.IGameEntity.RemoveEnginePhysics(gameEntity.Pointer);
	}

	public static bool IsEngineBodySleeping(this GameEntity gameEntity)
	{
		return EngineApplicationInterface.IGameEntity.IsEngineBodySleeping(gameEntity.Pointer);
	}

	public static bool IsDynamicBodyStationary(this GameEntity gameEntity)
	{
		return EngineApplicationInterface.IGameEntity.IsDynamicBodyStationary(gameEntity.Pointer);
	}

	public static bool IsDynamicBodyStationaryMT(this GameEntity gameEntity)
	{
		return EngineApplicationInterface.IGameEntity.IsDynamicBodyStationary(gameEntity.Pointer);
	}

	public static PhysicsShape GetBodyShape(this GameEntity gameEntity)
	{
		return EngineApplicationInterface.IGameEntity.GetBodyShape(gameEntity);
	}

	public static void SetBodyShape(this GameEntity gameEntity, PhysicsShape shape)
	{
		EngineApplicationInterface.IGameEntity.SetBodyShape(gameEntity.Pointer, (shape == null) ? ((UIntPtr)0uL) : shape.Pointer);
	}

	public static void AddPhysics(this GameEntity gameEntity, float mass, Vec3 localCenterOfMass, PhysicsShape body, Vec3 initialVelocity, Vec3 angularVelocity, PhysicsMaterial physicsMaterial, bool isStatic, int collisionGroupID)
	{
		EngineApplicationInterface.IGameEntity.AddPhysics(gameEntity.Pointer, (body != null) ? body.Pointer : UIntPtr.Zero, mass, ref localCenterOfMass, ref initialVelocity, ref angularVelocity, physicsMaterial.Index, isStatic, collisionGroupID);
		gameEntity.BodyFlag |= BodyFlags.Moveable;
	}

	public static void ApplyLocalImpulseToDynamicBody(this GameEntity gameEntity, Vec3 localPosition, Vec3 impulse)
	{
		EngineApplicationInterface.IGameEntity.ApplyLocalImpulseToDynamicBody(gameEntity.Pointer, ref localPosition, ref impulse);
	}

	public static void ApplyForceToDynamicBody(this GameEntity gameEntity, Vec3 force)
	{
		EngineApplicationInterface.IGameEntity.ApplyForceToDynamicBody(gameEntity.Pointer, ref force);
	}

	public static void ApplyLocalForceToDynamicBody(this GameEntity gameEntity, Vec3 localPosition, Vec3 force)
	{
		EngineApplicationInterface.IGameEntity.ApplyLocalForceToDynamicBody(gameEntity.Pointer, ref localPosition, ref force);
	}

	public static void ApplyAccelerationToDynamicBody(this GameEntity gameEntity, Vec3 acceleration)
	{
		EngineApplicationInterface.IGameEntity.ApplyAccelerationToDynamicBody(gameEntity.Pointer, ref acceleration);
	}

	public static void DisableDynamicBodySimulation(this GameEntity gameEntity)
	{
		EngineApplicationInterface.IGameEntity.DisableDynamicBodySimulation(gameEntity.Pointer);
	}

	public static void DisableDynamicBodySimulationMT(this GameEntity gameEntity)
	{
		EngineApplicationInterface.IGameEntity.DisableDynamicBodySimulation(gameEntity.Pointer);
	}

	public static void EnableDynamicBody(this GameEntity gameEntity)
	{
		EngineApplicationInterface.IGameEntity.EnableDynamicBody(gameEntity.Pointer);
	}

	public static float GetMass(this GameEntity gameEntity)
	{
		return EngineApplicationInterface.IGameEntity.GetMass(gameEntity.Pointer);
	}

	public static void SetMass(this GameEntity gameEntity, float mass)
	{
		EngineApplicationInterface.IGameEntity.SetMass(gameEntity.Pointer, mass);
	}

	public static void SetMassSpaceInertia(this GameEntity gameEntity, Vec3 inertia)
	{
		EngineApplicationInterface.IGameEntity.SetMassSpaceInertia(gameEntity.Pointer, ref inertia);
	}

	public static void SetDamping(this GameEntity gameEntity, float linearDamping, float angularDamping)
	{
		EngineApplicationInterface.IGameEntity.SetDamping(gameEntity.Pointer, linearDamping, angularDamping);
	}

	public static void DisableGravity(this GameEntity gameEntity)
	{
		EngineApplicationInterface.IGameEntity.DisableGravity(gameEntity.Pointer);
	}

	public static Vec3 GetLinearVelocity(this GameEntity gameEntity)
	{
		return EngineApplicationInterface.IGameEntity.GetLinearVelocity(gameEntity.Pointer);
	}

	public static Vec3 GetLinearVelocityMT(this GameEntity gameEntity)
	{
		return EngineApplicationInterface.IGameEntity.GetLinearVelocity(gameEntity.Pointer);
	}
}
