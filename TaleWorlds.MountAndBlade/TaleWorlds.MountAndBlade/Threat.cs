using System.Diagnostics;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class Threat
{
	public ITargetable WeaponEntity;

	public Formation Formation;

	public Agent Agent;

	public float ThreatValue;

	public string Name
	{
		get
		{
			if (WeaponEntity != null)
			{
				return WeaponEntity.Entity().Name;
			}
			if (Agent != null)
			{
				return Agent.Name.ToString();
			}
			if (Formation != null)
			{
				return Formation.ToString();
			}
			TaleWorlds.Library.Debug.FailedAssert("Invalid threat", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\Threat.cs", "Name", 38);
			return "Invalid";
		}
	}

	public Vec3 Position
	{
		get
		{
			if (WeaponEntity != null)
			{
				return (WeaponEntity.GetTargetEntity().PhysicsGlobalBoxMax + WeaponEntity.GetTargetEntity().PhysicsGlobalBoxMin) * 0.5f + WeaponEntity.GetTargetingOffset();
			}
			if (Agent != null)
			{
				return Agent.CollisionCapsuleCenter;
			}
			if (Formation != null)
			{
				return Formation.GetMedianAgent(excludeDetachedUnits: false, excludePlayer: false, Formation.GetAveragePositionOfUnits(excludeDetachedUnits: false, excludePlayer: false)).Position;
			}
			TaleWorlds.Library.Debug.FailedAssert("Invalid threat", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\Threat.cs", "Position", 62);
			return Vec3.Invalid;
		}
	}

	public Vec3 BoundingBoxMin
	{
		get
		{
			if (WeaponEntity != null)
			{
				return WeaponEntity.GetTargetEntity().PhysicsGlobalBoxMin + WeaponEntity.GetTargetingOffset();
			}
			if (Agent != null)
			{
				return Agent.CollisionCapsule.GetBoxMin();
			}
			if (Formation != null)
			{
				TaleWorlds.Library.Debug.FailedAssert("Nobody should be requesting a bounding box for a formation", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\Threat.cs", "BoundingBoxMin", 82);
				return Vec3.Invalid;
			}
			return Vec3.Invalid;
		}
	}

	public Vec3 BoundingBoxMax
	{
		get
		{
			if (WeaponEntity != null)
			{
				return WeaponEntity.GetTargetEntity().PhysicsGlobalBoxMax + WeaponEntity.GetTargetingOffset();
			}
			if (Agent != null)
			{
				return Agent.CollisionCapsule.GetBoxMax();
			}
			if (Formation != null)
			{
				TaleWorlds.Library.Debug.FailedAssert("Nobody should be requesting a bounding box for a formation", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\Threat.cs", "BoundingBoxMax", 106);
				return Vec3.Invalid;
			}
			return Vec3.Invalid;
		}
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public Vec3 GetVelocity()
	{
		if (WeaponEntity != null)
		{
			_ = Vec3.Zero;
			if (WeaponEntity is IMoveableSiegeWeapon moveableSiegeWeapon)
			{
				return moveableSiegeWeapon.MovementComponent.Velocity;
			}
		}
		return Vec3.Zero;
	}

	public override bool Equals(object obj)
	{
		if (obj is Threat threat)
		{
			if (WeaponEntity == threat.WeaponEntity)
			{
				return Formation == threat.Formation;
			}
			return false;
		}
		return false;
	}

	[Conditional("DEBUG")]
	public void DisplayDebugInfo()
	{
	}
}
