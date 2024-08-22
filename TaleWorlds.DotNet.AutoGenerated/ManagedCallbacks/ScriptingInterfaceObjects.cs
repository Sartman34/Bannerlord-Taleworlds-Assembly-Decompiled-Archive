using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ManagedCallbacks;

internal static class ScriptingInterfaceObjects
{
	public static Dictionary<string, object> GetObjects()
	{
		return new Dictionary<string, object>
		{
			{
				"TaleWorlds.DotNet.ILibrarySizeChecker",
				new ScriptingInterfaceOfILibrarySizeChecker()
			},
			{
				"TaleWorlds.DotNet.IManaged",
				new ScriptingInterfaceOfIManaged()
			},
			{
				"TaleWorlds.DotNet.INativeArray",
				new ScriptingInterfaceOfINativeArray()
			},
			{
				"TaleWorlds.DotNet.INativeObjectArray",
				new ScriptingInterfaceOfINativeObjectArray()
			},
			{
				"TaleWorlds.DotNet.INativeString",
				new ScriptingInterfaceOfINativeString()
			},
			{
				"TaleWorlds.DotNet.INativeStringHelper",
				new ScriptingInterfaceOfINativeStringHelper()
			},
			{
				"TaleWorlds.DotNet.ITelemetry",
				new ScriptingInterfaceOfITelemetry()
			}
		};
	}

	public static void SetFunctionPointer(int id, IntPtr pointer)
	{
		switch (id)
		{
		case 0:
			ScriptingInterfaceOfILibrarySizeChecker.call_GetEngineStructMemberOffsetDelegate = (ScriptingInterfaceOfILibrarySizeChecker.GetEngineStructMemberOffsetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILibrarySizeChecker.GetEngineStructMemberOffsetDelegate));
			break;
		case 1:
			ScriptingInterfaceOfILibrarySizeChecker.call_GetEngineStructSizeDelegate = (ScriptingInterfaceOfILibrarySizeChecker.GetEngineStructSizeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILibrarySizeChecker.GetEngineStructSizeDelegate));
			break;
		case 2:
			ScriptingInterfaceOfIManaged.call_DecreaseReferenceCountDelegate = (ScriptingInterfaceOfIManaged.DecreaseReferenceCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManaged.DecreaseReferenceCountDelegate));
			break;
		case 3:
			ScriptingInterfaceOfIManaged.call_GetClassTypeDefinitionDelegate = (ScriptingInterfaceOfIManaged.GetClassTypeDefinitionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManaged.GetClassTypeDefinitionDelegate));
			break;
		case 4:
			ScriptingInterfaceOfIManaged.call_GetClassTypeDefinitionCountDelegate = (ScriptingInterfaceOfIManaged.GetClassTypeDefinitionCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManaged.GetClassTypeDefinitionCountDelegate));
			break;
		case 5:
			ScriptingInterfaceOfIManaged.call_IncreaseReferenceCountDelegate = (ScriptingInterfaceOfIManaged.IncreaseReferenceCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManaged.IncreaseReferenceCountDelegate));
			break;
		case 6:
			ScriptingInterfaceOfIManaged.call_ReleaseManagedObjectDelegate = (ScriptingInterfaceOfIManaged.ReleaseManagedObjectDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManaged.ReleaseManagedObjectDelegate));
			break;
		case 7:
			ScriptingInterfaceOfINativeArray.call_AddElementDelegate = (ScriptingInterfaceOfINativeArray.AddElementDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeArray.AddElementDelegate));
			break;
		case 8:
			ScriptingInterfaceOfINativeArray.call_AddFloatElementDelegate = (ScriptingInterfaceOfINativeArray.AddFloatElementDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeArray.AddFloatElementDelegate));
			break;
		case 9:
			ScriptingInterfaceOfINativeArray.call_AddIntegerElementDelegate = (ScriptingInterfaceOfINativeArray.AddIntegerElementDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeArray.AddIntegerElementDelegate));
			break;
		case 10:
			ScriptingInterfaceOfINativeArray.call_ClearDelegate = (ScriptingInterfaceOfINativeArray.ClearDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeArray.ClearDelegate));
			break;
		case 11:
			ScriptingInterfaceOfINativeArray.call_CreateDelegate = (ScriptingInterfaceOfINativeArray.CreateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeArray.CreateDelegate));
			break;
		case 12:
			ScriptingInterfaceOfINativeArray.call_GetDataPointerDelegate = (ScriptingInterfaceOfINativeArray.GetDataPointerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeArray.GetDataPointerDelegate));
			break;
		case 13:
			ScriptingInterfaceOfINativeArray.call_GetDataPointerOffsetDelegate = (ScriptingInterfaceOfINativeArray.GetDataPointerOffsetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeArray.GetDataPointerOffsetDelegate));
			break;
		case 14:
			ScriptingInterfaceOfINativeArray.call_GetDataSizeDelegate = (ScriptingInterfaceOfINativeArray.GetDataSizeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeArray.GetDataSizeDelegate));
			break;
		case 15:
			ScriptingInterfaceOfINativeObjectArray.call_AddElementDelegate = (ScriptingInterfaceOfINativeObjectArray.AddElementDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeObjectArray.AddElementDelegate));
			break;
		case 16:
			ScriptingInterfaceOfINativeObjectArray.call_ClearDelegate = (ScriptingInterfaceOfINativeObjectArray.ClearDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeObjectArray.ClearDelegate));
			break;
		case 17:
			ScriptingInterfaceOfINativeObjectArray.call_CreateDelegate = (ScriptingInterfaceOfINativeObjectArray.CreateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeObjectArray.CreateDelegate));
			break;
		case 18:
			ScriptingInterfaceOfINativeObjectArray.call_GetCountDelegate = (ScriptingInterfaceOfINativeObjectArray.GetCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeObjectArray.GetCountDelegate));
			break;
		case 19:
			ScriptingInterfaceOfINativeObjectArray.call_GetElementAtIndexDelegate = (ScriptingInterfaceOfINativeObjectArray.GetElementAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeObjectArray.GetElementAtIndexDelegate));
			break;
		case 20:
			ScriptingInterfaceOfINativeString.call_CreateDelegate = (ScriptingInterfaceOfINativeString.CreateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeString.CreateDelegate));
			break;
		case 21:
			ScriptingInterfaceOfINativeString.call_GetStringDelegate = (ScriptingInterfaceOfINativeString.GetStringDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeString.GetStringDelegate));
			break;
		case 22:
			ScriptingInterfaceOfINativeString.call_SetStringDelegate = (ScriptingInterfaceOfINativeString.SetStringDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeString.SetStringDelegate));
			break;
		case 23:
			ScriptingInterfaceOfINativeStringHelper.call_CreateRglVarStringDelegate = (ScriptingInterfaceOfINativeStringHelper.CreateRglVarStringDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeStringHelper.CreateRglVarStringDelegate));
			break;
		case 24:
			ScriptingInterfaceOfINativeStringHelper.call_DeleteRglVarStringDelegate = (ScriptingInterfaceOfINativeStringHelper.DeleteRglVarStringDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeStringHelper.DeleteRglVarStringDelegate));
			break;
		case 25:
			ScriptingInterfaceOfINativeStringHelper.call_GetThreadLocalCachedRglVarStringDelegate = (ScriptingInterfaceOfINativeStringHelper.GetThreadLocalCachedRglVarStringDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeStringHelper.GetThreadLocalCachedRglVarStringDelegate));
			break;
		case 26:
			ScriptingInterfaceOfINativeStringHelper.call_SetRglVarStringDelegate = (ScriptingInterfaceOfINativeStringHelper.SetRglVarStringDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfINativeStringHelper.SetRglVarStringDelegate));
			break;
		case 27:
			ScriptingInterfaceOfITelemetry.call_BeginTelemetryScopeDelegate = (ScriptingInterfaceOfITelemetry.BeginTelemetryScopeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITelemetry.BeginTelemetryScopeDelegate));
			break;
		case 28:
			ScriptingInterfaceOfITelemetry.call_EndTelemetryScopeDelegate = (ScriptingInterfaceOfITelemetry.EndTelemetryScopeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITelemetry.EndTelemetryScopeDelegate));
			break;
		case 29:
			ScriptingInterfaceOfITelemetry.call_GetTelemetryLevelMaskDelegate = (ScriptingInterfaceOfITelemetry.GetTelemetryLevelMaskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITelemetry.GetTelemetryLevelMaskDelegate));
			break;
		case 30:
			ScriptingInterfaceOfITelemetry.call_HasTelemetryConnectionDelegate = (ScriptingInterfaceOfITelemetry.HasTelemetryConnectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITelemetry.HasTelemetryConnectionDelegate));
			break;
		case 31:
			ScriptingInterfaceOfITelemetry.call_StartTelemetryConnectionDelegate = (ScriptingInterfaceOfITelemetry.StartTelemetryConnectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITelemetry.StartTelemetryConnectionDelegate));
			break;
		case 32:
			ScriptingInterfaceOfITelemetry.call_StopTelemetryConnectionDelegate = (ScriptingInterfaceOfITelemetry.StopTelemetryConnectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITelemetry.StopTelemetryConnectionDelegate));
			break;
		}
	}
}
