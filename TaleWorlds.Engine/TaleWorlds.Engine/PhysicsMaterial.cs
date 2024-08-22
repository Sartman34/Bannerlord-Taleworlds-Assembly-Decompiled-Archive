using TaleWorlds.DotNet;

namespace TaleWorlds.Engine;

[EngineStruct("int", false)]
public struct PhysicsMaterial
{
	[CustomEngineStructMemberData("ignoredMember", true)]
	public readonly int Index;

	public static readonly PhysicsMaterial InvalidPhysicsMaterial = new PhysicsMaterial(-1);

	public bool IsValid => Index >= 0;

	public string Name => GetNameAtIndex(Index);

	internal PhysicsMaterial(int index)
	{
		this = default(PhysicsMaterial);
		Index = index;
	}

	public PhysicsMaterialFlags GetFlags()
	{
		return GetFlagsAtIndex(Index);
	}

	public float GetDynamicFriction()
	{
		return GetDynamicFrictionAtIndex(Index);
	}

	public float GetStaticFriction()
	{
		return GetStaticFrictionAtIndex(Index);
	}

	public float GetSoftness()
	{
		return GetSoftnessAtIndex(Index);
	}

	public float GetRestitution()
	{
		return GetRestitutionAtIndex(Index);
	}

	public bool Equals(PhysicsMaterial m)
	{
		return Index == m.Index;
	}

	public static int GetMaterialCount()
	{
		return EngineApplicationInterface.IPhysicsMaterial.GetMaterialCount();
	}

	public static PhysicsMaterial GetFromName(string id)
	{
		return EngineApplicationInterface.IPhysicsMaterial.GetIndexWithName(id);
	}

	public static string GetNameAtIndex(int index)
	{
		return EngineApplicationInterface.IPhysicsMaterial.GetMaterialNameAtIndex(index);
	}

	public static PhysicsMaterialFlags GetFlagsAtIndex(int index)
	{
		return EngineApplicationInterface.IPhysicsMaterial.GetFlagsAtIndex(index);
	}

	public static float GetRestitutionAtIndex(int index)
	{
		return EngineApplicationInterface.IPhysicsMaterial.GetRestitutionAtIndex(index);
	}

	public static float GetSoftnessAtIndex(int index)
	{
		return EngineApplicationInterface.IPhysicsMaterial.GetSoftnessAtIndex(index);
	}

	public static float GetDynamicFrictionAtIndex(int index)
	{
		return EngineApplicationInterface.IPhysicsMaterial.GetDynamicFrictionAtIndex(index);
	}

	public static float GetStaticFrictionAtIndex(int index)
	{
		return EngineApplicationInterface.IPhysicsMaterial.GetStaticFrictionAtIndex(index);
	}

	public static PhysicsMaterial GetFromIndex(int index)
	{
		return new PhysicsMaterial(index);
	}
}
