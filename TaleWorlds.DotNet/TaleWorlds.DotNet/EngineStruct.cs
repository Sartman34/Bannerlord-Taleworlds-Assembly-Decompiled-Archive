using System;

namespace TaleWorlds.DotNet;

public class EngineStruct : Attribute
{
	public string EngineType { get; set; }

	public string AlternateDotNetType { get; set; }

	public bool IgnoreMemberOffsetTest { get; set; }

	public EngineStruct(string engineType, bool ignoreMemberOffsetTest = false)
	{
		EngineType = engineType;
		AlternateDotNetType = null;
		IgnoreMemberOffsetTest = ignoreMemberOffsetTest;
	}

	public EngineStruct(string engineType, string alternateDotNetType, bool ignoreMemberOffsetTest = false)
	{
		EngineType = engineType;
		AlternateDotNetType = alternateDotNetType;
		IgnoreMemberOffsetTest = ignoreMemberOffsetTest;
	}
}
