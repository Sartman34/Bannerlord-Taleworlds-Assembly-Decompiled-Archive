using TaleWorlds.Library;

namespace TaleWorlds.DotNet;

public class NativeTelemetryManager : ITelemetryManager
{
	public static uint TelemetryLevelMask { get; private set; }

	public uint GetTelemetryLevelMask()
	{
		return TelemetryLevelMask;
	}

	public NativeTelemetryManager()
	{
		TelemetryLevelMask = 4096u;
	}

	internal void Update()
	{
		TelemetryLevelMask = LibraryApplicationInterface.ITelemetry.GetTelemetryLevelMask();
	}

	public void StartTelemetryConnection(bool showErrors)
	{
		LibraryApplicationInterface.ITelemetry.StartTelemetryConnection(showErrors);
	}

	public void StopTelemetryConnection()
	{
		LibraryApplicationInterface.ITelemetry.StopTelemetryConnection();
	}

	public void BeginTelemetryScopeInternal(TelemetryLevelMask levelMask, string scopeName)
	{
		if ((TelemetryLevelMask & (uint)levelMask) != 0)
		{
			LibraryApplicationInterface.ITelemetry.BeginTelemetryScope(levelMask, scopeName);
		}
	}

	public void EndTelemetryScopeInternal()
	{
		LibraryApplicationInterface.ITelemetry.EndTelemetryScope();
	}

	public void BeginTelemetryScopeBaseLevelInternal(TelemetryLevelMask levelMask, string scopeName)
	{
		if ((TelemetryLevelMask & (uint)levelMask) != 0)
		{
			LibraryApplicationInterface.ITelemetry.BeginTelemetryScope(levelMask, scopeName);
		}
	}

	public void EndTelemetryScopeBaseLevelInternal()
	{
		LibraryApplicationInterface.ITelemetry.EndTelemetryScope();
	}
}
