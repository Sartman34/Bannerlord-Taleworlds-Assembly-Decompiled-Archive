using System;

namespace TaleWorlds.Engine;

[Flags]
public enum BodyFlags : uint
{
	None = 0u,
	Disabled = 1u,
	NotDestructible = 2u,
	TwoSided = 4u,
	Dynamic = 8u,
	Moveable = 0x10u,
	DynamicConvexHull = 0x20u,
	Ladder = 0x40u,
	OnlyCollideWithRaycast = 0x80u,
	AILimiter = 0x100u,
	Barrier = 0x200u,
	Barrier3D = 0x400u,
	HasSteps = 0x800u,
	Ragdoll = 0x1000u,
	RagdollLimiter = 0x2000u,
	DestructibleDoor = 0x4000u,
	DroppedItem = 0x8000u,
	DoNotCollideWithRaycast = 0x10000u,
	DontTransferToPhysicsEngine = 0x20000u,
	DontCollideWithCamera = 0x40000u,
	ExcludePathSnap = 0x80000u,
	IsOpoed = 0x100000u,
	AfterAddFlags = 0x100000u,
	AgentOnly = 0x200000u,
	MissileOnly = 0x400000u,
	HasMaterial = 0x800000u,
	BodyFlagFilter = 0xFFFFFFu,
	CommonCollisionExcludeFlags = 0x61B189u,
	CameraCollisionRayCastExludeFlags = 0x61B7C9u,
	CommonCollisionExcludeFlagsForAgent = 0x41B189u,
	CommonCollisionExcludeFlagsForMissile = 0x21B789u,
	CommonCollisionExcludeFlagsForCombat = 0x21B189u,
	CommonCollisionExcludeFlagsForEditor = 0x21B189u,
	CommonFlagsThatDoNotBlocksRay = 0xFF3F3Fu,
	CommonFocusRayCastExcludeFlags = 0x13701u,
	BodyOwnerNone = 0u,
	BodyOwnerEntity = 0x1000000u,
	BodyOwnerTerrain = 0x2000000u,
	BodyOwnerFlora = 0x4000000u,
	BodyOwnerFilter = 0xF000000u,
	IgnoreSoundOcclusion = 0x10000000u
}
