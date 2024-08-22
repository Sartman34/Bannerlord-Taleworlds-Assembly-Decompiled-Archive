using System;
using TaleWorlds.Library;

namespace TaleWorlds.DotNet;

[LibraryInterfaceBase]
internal interface INativeObjectArray
{
	[EngineMethod("create", false)]
	NativeObjectArray Create();

	[EngineMethod("get_count", false)]
	int GetCount(UIntPtr pointer);

	[EngineMethod("add_element", false)]
	void AddElement(UIntPtr pointer, UIntPtr nativeObject);

	[EngineMethod("get_element_at_index", false)]
	NativeObject GetElementAtIndex(UIntPtr pointer, int index);

	[EngineMethod("clear", false)]
	void Clear(UIntPtr pointer);
}
