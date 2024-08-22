using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;

namespace ManagedCallbacks;

internal class ScriptingInterfaceOfIThumbnailCreatorView : IThumbnailCreatorView
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void CancelRequestDelegate(UIntPtr pointer, byte[] render_id);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void ClearRequestsDelegate(UIntPtr pointer);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate NativeObjectPointer CreateThumbnailCreatorViewDelegate();

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate int GetNumberOfPendingRequestsDelegate(UIntPtr pointer);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool IsMemoryClearedDelegate(UIntPtr pointer);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void RegisterEntityDelegate(UIntPtr pointer, UIntPtr scene_ptr, UIntPtr cam_ptr, UIntPtr texture_ptr, UIntPtr entity_ptr, byte[] render_id, int allocationGroupIndex);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void RegisterEntityWithoutTextureDelegate(UIntPtr pointer, UIntPtr scene_ptr, UIntPtr cam_ptr, UIntPtr entity_ptr, int width, int height, byte[] render_id, byte[] debug_name, int allocationGroupIndex);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void RegisterSceneDelegate(UIntPtr pointer, UIntPtr scene_ptr, [MarshalAs(UnmanagedType.U1)] bool use_postfx);

	private static readonly Encoding _utf8 = Encoding.UTF8;

	public static CancelRequestDelegate call_CancelRequestDelegate;

	public static ClearRequestsDelegate call_ClearRequestsDelegate;

	public static CreateThumbnailCreatorViewDelegate call_CreateThumbnailCreatorViewDelegate;

	public static GetNumberOfPendingRequestsDelegate call_GetNumberOfPendingRequestsDelegate;

	public static IsMemoryClearedDelegate call_IsMemoryClearedDelegate;

	public static RegisterEntityDelegate call_RegisterEntityDelegate;

	public static RegisterEntityWithoutTextureDelegate call_RegisterEntityWithoutTextureDelegate;

	public static RegisterSceneDelegate call_RegisterSceneDelegate;

	public void CancelRequest(UIntPtr pointer, string render_id)
	{
		byte[] array = null;
		if (render_id != null)
		{
			int byteCount = _utf8.GetByteCount(render_id);
			array = ((byteCount < 1024) ? CallbackStringBufferManager.StringBuffer0 : new byte[byteCount + 1]);
			_utf8.GetBytes(render_id, 0, render_id.Length, array, 0);
			array[byteCount] = 0;
		}
		call_CancelRequestDelegate(pointer, array);
	}

	public void ClearRequests(UIntPtr pointer)
	{
		call_ClearRequestsDelegate(pointer);
	}

	public ThumbnailCreatorView CreateThumbnailCreatorView()
	{
		NativeObjectPointer nativeObjectPointer = call_CreateThumbnailCreatorViewDelegate();
		ThumbnailCreatorView result = null;
		if (nativeObjectPointer.Pointer != UIntPtr.Zero)
		{
			result = new ThumbnailCreatorView(nativeObjectPointer.Pointer);
			LibraryApplicationInterface.IManaged.DecreaseReferenceCount(nativeObjectPointer.Pointer);
		}
		return result;
	}

	public int GetNumberOfPendingRequests(UIntPtr pointer)
	{
		return call_GetNumberOfPendingRequestsDelegate(pointer);
	}

	public bool IsMemoryCleared(UIntPtr pointer)
	{
		return call_IsMemoryClearedDelegate(pointer);
	}

	public void RegisterEntity(UIntPtr pointer, UIntPtr scene_ptr, UIntPtr cam_ptr, UIntPtr texture_ptr, UIntPtr entity_ptr, string render_id, int allocationGroupIndex)
	{
		byte[] array = null;
		if (render_id != null)
		{
			int byteCount = _utf8.GetByteCount(render_id);
			array = ((byteCount < 1024) ? CallbackStringBufferManager.StringBuffer0 : new byte[byteCount + 1]);
			_utf8.GetBytes(render_id, 0, render_id.Length, array, 0);
			array[byteCount] = 0;
		}
		call_RegisterEntityDelegate(pointer, scene_ptr, cam_ptr, texture_ptr, entity_ptr, array, allocationGroupIndex);
	}

	public void RegisterEntityWithoutTexture(UIntPtr pointer, UIntPtr scene_ptr, UIntPtr cam_ptr, UIntPtr entity_ptr, int width, int height, string render_id, string debug_name, int allocationGroupIndex)
	{
		byte[] array = null;
		if (render_id != null)
		{
			int byteCount = _utf8.GetByteCount(render_id);
			array = ((byteCount < 1024) ? CallbackStringBufferManager.StringBuffer0 : new byte[byteCount + 1]);
			_utf8.GetBytes(render_id, 0, render_id.Length, array, 0);
			array[byteCount] = 0;
		}
		byte[] array2 = null;
		if (debug_name != null)
		{
			int byteCount2 = _utf8.GetByteCount(debug_name);
			array2 = ((byteCount2 < 1024) ? CallbackStringBufferManager.StringBuffer1 : new byte[byteCount2 + 1]);
			_utf8.GetBytes(debug_name, 0, debug_name.Length, array2, 0);
			array2[byteCount2] = 0;
		}
		call_RegisterEntityWithoutTextureDelegate(pointer, scene_ptr, cam_ptr, entity_ptr, width, height, array, array2, allocationGroupIndex);
	}

	public void RegisterScene(UIntPtr pointer, UIntPtr scene_ptr, bool use_postfx)
	{
		call_RegisterSceneDelegate(pointer, scene_ptr, use_postfx);
	}
}
