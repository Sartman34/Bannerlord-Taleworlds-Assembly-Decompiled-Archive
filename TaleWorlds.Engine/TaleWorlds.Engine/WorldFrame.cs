using TaleWorlds.Library;

namespace TaleWorlds.Engine;

public struct WorldFrame
{
	public Mat3 Rotation;

	public WorldPosition Origin;

	public static readonly WorldFrame Invalid = new WorldFrame(Mat3.Identity, WorldPosition.Invalid);

	public bool IsValid => Origin.IsValid;

	public WorldFrame(Mat3 rotation, WorldPosition origin)
	{
		Rotation = rotation;
		Origin = origin;
	}

	public MatrixFrame ToGroundMatrixFrame()
	{
		return new MatrixFrame(Rotation, Origin.GetGroundVec3());
	}

	public MatrixFrame ToNavMeshMatrixFrame()
	{
		return new MatrixFrame(Rotation, Origin.GetNavMeshVec3());
	}
}
