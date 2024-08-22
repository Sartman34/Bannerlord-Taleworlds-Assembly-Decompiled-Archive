using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;

namespace ManagedCallbacks;

internal class ScriptingInterfaceOfIScriptComponent : IScriptComponent
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate int GetScriptComponentBehaviorDelegate(UIntPtr pointer);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetVariableEditorWidgetStatusDelegate(UIntPtr pointer, byte[] field, [MarshalAs(UnmanagedType.U1)] bool enabled);

	private static readonly Encoding _utf8 = Encoding.UTF8;

	public static GetScriptComponentBehaviorDelegate call_GetScriptComponentBehaviorDelegate;

	public static SetVariableEditorWidgetStatusDelegate call_SetVariableEditorWidgetStatusDelegate;

	public ScriptComponentBehavior GetScriptComponentBehavior(UIntPtr pointer)
	{
		return DotNetObject.GetManagedObjectWithId(call_GetScriptComponentBehaviorDelegate(pointer)) as ScriptComponentBehavior;
	}

	public void SetVariableEditorWidgetStatus(UIntPtr pointer, string field, bool enabled)
	{
		byte[] array = null;
		if (field != null)
		{
			int byteCount = _utf8.GetByteCount(field);
			array = ((byteCount < 1024) ? CallbackStringBufferManager.StringBuffer0 : new byte[byteCount + 1]);
			_utf8.GetBytes(field, 0, field.Length, array, 0);
			array[byteCount] = 0;
		}
		call_SetVariableEditorWidgetStatusDelegate(pointer, array, enabled);
	}
}
