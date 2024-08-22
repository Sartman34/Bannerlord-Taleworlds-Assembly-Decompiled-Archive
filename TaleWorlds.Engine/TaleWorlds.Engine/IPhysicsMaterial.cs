using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface IPhysicsMaterial
{
	[EngineMethod("get_index_with_name", false)]
	PhysicsMaterial GetIndexWithName(string materialName);

	[EngineMethod("get_material_count", false)]
	int GetMaterialCount();

	[EngineMethod("get_material_name_at_index", false)]
	string GetMaterialNameAtIndex(int index);

	[EngineMethod("get_material_flags_at_index", false)]
	PhysicsMaterialFlags GetFlagsAtIndex(int index);

	[EngineMethod("get_restitution_at_index", false)]
	float GetRestitutionAtIndex(int index);

	[EngineMethod("get_dynamic_friction_at_index", false)]
	float GetDynamicFrictionAtIndex(int index);

	[EngineMethod("get_static_friction_at_index", false)]
	float GetStaticFrictionAtIndex(int index);

	[EngineMethod("get_softness_at_index", false)]
	float GetSoftnessAtIndex(int index);
}
