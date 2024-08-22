using System;
using TaleWorlds.Library;

namespace TaleWorlds.DotNet;

[LibraryInterfaceBase]
internal interface INativeStringHelper
{
	[EngineMethod("create_rglVarString", false)]
	UIntPtr CreateRglVarString(string text);

	[EngineMethod("get_thread_local_cached_rglVarString", false)]
	UIntPtr GetThreadLocalCachedRglVarString();

	[EngineMethod("set_rglVarString", false)]
	void SetRglVarString(UIntPtr pointer, string text);

	[EngineMethod("delete_rglVarString", false)]
	void DeleteRglVarString(UIntPtr pointer);
}
