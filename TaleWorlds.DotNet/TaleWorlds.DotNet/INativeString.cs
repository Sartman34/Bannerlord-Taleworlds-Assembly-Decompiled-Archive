using TaleWorlds.Library;

namespace TaleWorlds.DotNet;

[LibraryInterfaceBase]
internal interface INativeString
{
	[EngineMethod("create", false)]
	NativeString Create();

	[EngineMethod("get_string", false)]
	string GetString(NativeString nativeString);

	[EngineMethod("set_string", false)]
	void SetString(NativeString nativeString, string newString);
}
