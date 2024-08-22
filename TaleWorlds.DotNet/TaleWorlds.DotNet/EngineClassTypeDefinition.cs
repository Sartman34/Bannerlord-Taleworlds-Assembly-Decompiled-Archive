using System.Runtime.InteropServices;

namespace TaleWorlds.DotNet;

[EngineStruct("ftlObject_type_definition", false)]
internal struct EngineClassTypeDefinition
{
	public int TypeId;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string TypeName;
}
