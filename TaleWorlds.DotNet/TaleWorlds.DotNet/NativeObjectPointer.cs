using System;

namespace TaleWorlds.DotNet;

[EngineStruct("ftlNative_object_pointer", false)]
internal struct NativeObjectPointer
{
	public UIntPtr Pointer;

	public int TypeId;
}
