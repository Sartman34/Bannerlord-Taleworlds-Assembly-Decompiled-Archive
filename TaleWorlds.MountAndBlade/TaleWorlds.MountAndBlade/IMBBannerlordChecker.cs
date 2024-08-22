using System;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

[ScriptingInterfaceBase]
internal interface IMBBannerlordChecker
{
	[EngineMethod("get_engine_struct_size", false)]
	int GetEngineStructSize(string str);

	[EngineMethod("get_engine_struct_member_offset", false)]
	IntPtr GetEngineStructMemberOffset(string className, string memberName);
}
