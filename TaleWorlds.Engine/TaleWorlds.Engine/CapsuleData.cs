using TaleWorlds.DotNet;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[EngineStruct("rglCapsule_data", false)]
public struct CapsuleData
{
	private FtlCapsuleData _globalData;

	private FtlCapsuleData _localData;

	public Vec3 P1
	{
		get
		{
			return _globalData.P1;
		}
		set
		{
			_globalData.P1 = value;
		}
	}

	public Vec3 P2
	{
		get
		{
			return _globalData.P2;
		}
		set
		{
			_globalData.P2 = value;
		}
	}

	public float Radius
	{
		get
		{
			return _globalData.Radius;
		}
		set
		{
			_globalData.Radius = value;
		}
	}

	internal float LocalRadius
	{
		get
		{
			return _localData.Radius;
		}
		set
		{
			_localData.Radius = value;
		}
	}

	internal Vec3 LocalP1
	{
		get
		{
			return _localData.P1;
		}
		set
		{
			_localData.P1 = value;
		}
	}

	internal Vec3 LocalP2
	{
		get
		{
			return _localData.P2;
		}
		set
		{
			_localData.P2 = value;
		}
	}

	public CapsuleData(float radius, Vec3 p1, Vec3 p2)
	{
		_globalData = new FtlCapsuleData(radius, p1, p2);
		_localData = new FtlCapsuleData(radius, p1, p2);
	}

	public Vec3 GetBoxMin()
	{
		return new Vec3(MathF.Min(P1.x, P2.x) - Radius, MathF.Min(P1.y, P2.y) - Radius, MathF.Min(P1.z, P2.z) - Radius);
	}

	public Vec3 GetBoxMax()
	{
		return new Vec3(MathF.Max(P1.x, P2.x) + Radius, MathF.Max(P1.y, P2.y) + Radius, MathF.Max(P1.z, P2.z) + Radius);
	}
}
