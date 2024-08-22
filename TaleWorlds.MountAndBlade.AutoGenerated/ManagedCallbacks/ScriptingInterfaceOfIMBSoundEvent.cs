using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace ManagedCallbacks;

internal class ScriptingInterfaceOfIMBSoundEvent : IMBSoundEvent
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate int CreateEventFromExternalFileDelegate(byte[] programmerSoundEventName, byte[] filePath, UIntPtr scene);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate int CreateEventFromSoundBufferDelegate(byte[] programmerSoundEventName, ManagedArray soundBuffer, UIntPtr scene);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool PlaySoundDelegate(int fmodEventIndex, ref Vec3 position);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool PlaySoundWithIntParamDelegate(int fmodEventIndex, int paramIndex, float paramVal, ref Vec3 position);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool PlaySoundWithParamDelegate(int soundCodeId, SoundEventParameter parameter, ref Vec3 position);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool PlaySoundWithStrParamDelegate(int fmodEventIndex, byte[] paramName, float paramVal, ref Vec3 position);

	private static readonly Encoding _utf8 = Encoding.UTF8;

	public static CreateEventFromExternalFileDelegate call_CreateEventFromExternalFileDelegate;

	public static CreateEventFromSoundBufferDelegate call_CreateEventFromSoundBufferDelegate;

	public static PlaySoundDelegate call_PlaySoundDelegate;

	public static PlaySoundWithIntParamDelegate call_PlaySoundWithIntParamDelegate;

	public static PlaySoundWithParamDelegate call_PlaySoundWithParamDelegate;

	public static PlaySoundWithStrParamDelegate call_PlaySoundWithStrParamDelegate;

	public int CreateEventFromExternalFile(string programmerSoundEventName, string filePath, UIntPtr scene)
	{
		byte[] array = null;
		if (programmerSoundEventName != null)
		{
			int byteCount = _utf8.GetByteCount(programmerSoundEventName);
			array = ((byteCount < 1024) ? CallbackStringBufferManager.StringBuffer0 : new byte[byteCount + 1]);
			_utf8.GetBytes(programmerSoundEventName, 0, programmerSoundEventName.Length, array, 0);
			array[byteCount] = 0;
		}
		byte[] array2 = null;
		if (filePath != null)
		{
			int byteCount2 = _utf8.GetByteCount(filePath);
			array2 = ((byteCount2 < 1024) ? CallbackStringBufferManager.StringBuffer1 : new byte[byteCount2 + 1]);
			_utf8.GetBytes(filePath, 0, filePath.Length, array2, 0);
			array2[byteCount2] = 0;
		}
		return call_CreateEventFromExternalFileDelegate(array, array2, scene);
	}

	public int CreateEventFromSoundBuffer(string programmerSoundEventName, byte[] soundBuffer, UIntPtr scene)
	{
		byte[] array = null;
		if (programmerSoundEventName != null)
		{
			int byteCount = _utf8.GetByteCount(programmerSoundEventName);
			array = ((byteCount < 1024) ? CallbackStringBufferManager.StringBuffer0 : new byte[byteCount + 1]);
			_utf8.GetBytes(programmerSoundEventName, 0, programmerSoundEventName.Length, array, 0);
			array[byteCount] = 0;
		}
		PinnedArrayData<byte> pinnedArrayData = new PinnedArrayData<byte>(soundBuffer);
		IntPtr pointer = pinnedArrayData.Pointer;
		ManagedArray soundBuffer2 = new ManagedArray(pointer, (soundBuffer != null) ? soundBuffer.Length : 0);
		int result = call_CreateEventFromSoundBufferDelegate(array, soundBuffer2, scene);
		pinnedArrayData.Dispose();
		return result;
	}

	public bool PlaySound(int fmodEventIndex, ref Vec3 position)
	{
		return call_PlaySoundDelegate(fmodEventIndex, ref position);
	}

	public bool PlaySoundWithIntParam(int fmodEventIndex, int paramIndex, float paramVal, ref Vec3 position)
	{
		return call_PlaySoundWithIntParamDelegate(fmodEventIndex, paramIndex, paramVal, ref position);
	}

	public bool PlaySoundWithParam(int soundCodeId, SoundEventParameter parameter, ref Vec3 position)
	{
		return call_PlaySoundWithParamDelegate(soundCodeId, parameter, ref position);
	}

	public bool PlaySoundWithStrParam(int fmodEventIndex, string paramName, float paramVal, ref Vec3 position)
	{
		byte[] array = null;
		if (paramName != null)
		{
			int byteCount = _utf8.GetByteCount(paramName);
			array = ((byteCount < 1024) ? CallbackStringBufferManager.StringBuffer0 : new byte[byteCount + 1]);
			_utf8.GetBytes(paramName, 0, paramName.Length, array, 0);
			array[byteCount] = 0;
		}
		return call_PlaySoundWithStrParamDelegate(fmodEventIndex, array, paramVal, ref position);
	}
}
