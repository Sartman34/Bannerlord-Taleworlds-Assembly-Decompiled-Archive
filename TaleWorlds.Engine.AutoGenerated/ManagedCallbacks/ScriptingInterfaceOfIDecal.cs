using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace ManagedCallbacks;

internal class ScriptingInterfaceOfIDecal : IDecal
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate NativeObjectPointer CreateCopyDelegate(UIntPtr pointer);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate NativeObjectPointer CreateDecalDelegate(byte[] name);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate uint GetFactor1Delegate(UIntPtr decalPointer);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void GetFrameDelegate(UIntPtr decalPointer, ref MatrixFrame outFrame);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate NativeObjectPointer GetMaterialDelegate(UIntPtr decalPointer);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetFactor1Delegate(UIntPtr decalPointer, uint factorColor1);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetFactor1LinearDelegate(UIntPtr decalPointer, uint linearFactorColor1);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetFrameDelegate(UIntPtr decalPointer, ref MatrixFrame decalFrame);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetMaterialDelegate(UIntPtr decalPointer, UIntPtr materialPointer);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetVectorArgumentDelegate(UIntPtr decalPointer, float vectorArgument0, float vectorArgument1, float vectorArgument2, float vectorArgument3);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetVectorArgument2Delegate(UIntPtr decalPointer, float vectorArgument0, float vectorArgument1, float vectorArgument2, float vectorArgument3);

	private static readonly Encoding _utf8 = Encoding.UTF8;

	public static CreateCopyDelegate call_CreateCopyDelegate;

	public static CreateDecalDelegate call_CreateDecalDelegate;

	public static GetFactor1Delegate call_GetFactor1Delegate;

	public static GetFrameDelegate call_GetFrameDelegate;

	public static GetMaterialDelegate call_GetMaterialDelegate;

	public static SetFactor1Delegate call_SetFactor1Delegate;

	public static SetFactor1LinearDelegate call_SetFactor1LinearDelegate;

	public static SetFrameDelegate call_SetFrameDelegate;

	public static SetMaterialDelegate call_SetMaterialDelegate;

	public static SetVectorArgumentDelegate call_SetVectorArgumentDelegate;

	public static SetVectorArgument2Delegate call_SetVectorArgument2Delegate;

	public Decal CreateCopy(UIntPtr pointer)
	{
		NativeObjectPointer nativeObjectPointer = call_CreateCopyDelegate(pointer);
		Decal result = null;
		if (nativeObjectPointer.Pointer != UIntPtr.Zero)
		{
			result = new Decal(nativeObjectPointer.Pointer);
			LibraryApplicationInterface.IManaged.DecreaseReferenceCount(nativeObjectPointer.Pointer);
		}
		return result;
	}

	public Decal CreateDecal(string name)
	{
		byte[] array = null;
		if (name != null)
		{
			int byteCount = _utf8.GetByteCount(name);
			array = ((byteCount < 1024) ? CallbackStringBufferManager.StringBuffer0 : new byte[byteCount + 1]);
			_utf8.GetBytes(name, 0, name.Length, array, 0);
			array[byteCount] = 0;
		}
		NativeObjectPointer nativeObjectPointer = call_CreateDecalDelegate(array);
		Decal result = null;
		if (nativeObjectPointer.Pointer != UIntPtr.Zero)
		{
			result = new Decal(nativeObjectPointer.Pointer);
			LibraryApplicationInterface.IManaged.DecreaseReferenceCount(nativeObjectPointer.Pointer);
		}
		return result;
	}

	public uint GetFactor1(UIntPtr decalPointer)
	{
		return call_GetFactor1Delegate(decalPointer);
	}

	public void GetFrame(UIntPtr decalPointer, ref MatrixFrame outFrame)
	{
		call_GetFrameDelegate(decalPointer, ref outFrame);
	}

	public Material GetMaterial(UIntPtr decalPointer)
	{
		NativeObjectPointer nativeObjectPointer = call_GetMaterialDelegate(decalPointer);
		Material result = null;
		if (nativeObjectPointer.Pointer != UIntPtr.Zero)
		{
			result = new Material(nativeObjectPointer.Pointer);
			LibraryApplicationInterface.IManaged.DecreaseReferenceCount(nativeObjectPointer.Pointer);
		}
		return result;
	}

	public void SetFactor1(UIntPtr decalPointer, uint factorColor1)
	{
		call_SetFactor1Delegate(decalPointer, factorColor1);
	}

	public void SetFactor1Linear(UIntPtr decalPointer, uint linearFactorColor1)
	{
		call_SetFactor1LinearDelegate(decalPointer, linearFactorColor1);
	}

	public void SetFrame(UIntPtr decalPointer, ref MatrixFrame decalFrame)
	{
		call_SetFrameDelegate(decalPointer, ref decalFrame);
	}

	public void SetMaterial(UIntPtr decalPointer, UIntPtr materialPointer)
	{
		call_SetMaterialDelegate(decalPointer, materialPointer);
	}

	public void SetVectorArgument(UIntPtr decalPointer, float vectorArgument0, float vectorArgument1, float vectorArgument2, float vectorArgument3)
	{
		call_SetVectorArgumentDelegate(decalPointer, vectorArgument0, vectorArgument1, vectorArgument2, vectorArgument3);
	}

	public void SetVectorArgument2(UIntPtr decalPointer, float vectorArgument0, float vectorArgument1, float vectorArgument2, float vectorArgument3)
	{
		call_SetVectorArgument2Delegate(decalPointer, vectorArgument0, vectorArgument1, vectorArgument2, vectorArgument3);
	}
}
