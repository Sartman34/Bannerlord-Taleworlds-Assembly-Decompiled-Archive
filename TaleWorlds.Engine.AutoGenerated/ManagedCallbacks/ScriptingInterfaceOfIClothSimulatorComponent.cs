using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using TaleWorlds.Engine;

namespace ManagedCallbacks;

internal class ScriptingInterfaceOfIClothSimulatorComponent : IClothSimulatorComponent
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetMaxDistanceMultiplierDelegate(UIntPtr cloth_pointer, float multiplier);

	private static readonly Encoding _utf8 = Encoding.UTF8;

	public static SetMaxDistanceMultiplierDelegate call_SetMaxDistanceMultiplierDelegate;

	public void SetMaxDistanceMultiplier(UIntPtr cloth_pointer, float multiplier)
	{
		call_SetMaxDistanceMultiplierDelegate(cloth_pointer, multiplier);
	}
}
