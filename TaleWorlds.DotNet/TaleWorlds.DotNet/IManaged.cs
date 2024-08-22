using System;
using TaleWorlds.Library;

namespace TaleWorlds.DotNet;

[LibraryInterfaceBase]
internal interface IManaged
{
	[EngineMethod("increase_reference_count", false)]
	void IncreaseReferenceCount(UIntPtr ptr);

	[EngineMethod("decrease_reference_count", false)]
	void DecreaseReferenceCount(UIntPtr ptr);

	[EngineMethod("get_class_type_definition_count", false)]
	int GetClassTypeDefinitionCount();

	[EngineMethod("get_class_type_definition", false)]
	void GetClassTypeDefinition(int index, ref EngineClassTypeDefinition engineClassTypeDefinition);

	[EngineMethod("release_managed_object", false)]
	void ReleaseManagedObject(UIntPtr ptr);
}
