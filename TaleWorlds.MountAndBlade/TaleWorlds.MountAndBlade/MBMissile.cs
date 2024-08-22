using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public abstract class MBMissile
{
	private readonly Mission _mission;

	public int Index { get; set; }

	protected MBMissile(Mission mission)
	{
		_mission = mission;
	}

	public Vec3 GetPosition()
	{
		return MBAPI.IMBMission.GetPositionOfMissile(_mission.Pointer, Index);
	}

	public Vec3 GetVelocity()
	{
		return MBAPI.IMBMission.GetVelocityOfMissile(_mission.Pointer, Index);
	}

	public bool GetHasRigidBody()
	{
		return MBAPI.IMBMission.GetMissileHasRigidBody(_mission.Pointer, Index);
	}
}
