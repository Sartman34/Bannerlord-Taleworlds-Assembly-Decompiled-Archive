using System;

namespace TaleWorlds.Library;

public class EngineMethod : Attribute
{
	public string EngineMethodName { get; private set; }

	public bool ActivateTelemetryProfiling { get; private set; }

	public EngineMethod(string engineMethodName, bool activateTelemetryProfiling = false)
	{
		EngineMethodName = engineMethodName;
		ActivateTelemetryProfiling = activateTelemetryProfiling;
	}
}
