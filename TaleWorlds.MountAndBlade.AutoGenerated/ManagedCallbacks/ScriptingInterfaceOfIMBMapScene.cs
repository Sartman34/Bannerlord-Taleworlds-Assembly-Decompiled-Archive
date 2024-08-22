using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using TaleWorlds.DotNet;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace ManagedCallbacks;

internal class ScriptingInterfaceOfIMBMapScene : IMBMapScene
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate Vec3 GetAccessiblePointNearPositionDelegate(UIntPtr scenePointer, Vec2 position, float radius);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void GetBattleSceneIndexMapDelegate(UIntPtr scenePointer, ManagedArray indexData);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void GetBattleSceneIndexMapResolutionDelegate(UIntPtr scenePointer, ref int width, ref int height);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void GetColorGradeGridDataDelegate(UIntPtr scenePointer, ManagedArray snowData, byte[] textureName);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void GetFaceIndexForMultiplePositionsDelegate(UIntPtr scenePointer, int movedPartyCount, IntPtr positionArray, IntPtr resultArray, [MarshalAs(UnmanagedType.U1)] bool check_if_disabled, [MarshalAs(UnmanagedType.U1)] bool check_height);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	[return: MarshalAs(UnmanagedType.U1)]
	public delegate bool GetMouseVisibleDelegate();

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate float GetSeasonTimeFactorDelegate(UIntPtr scenePointer);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void LoadAtmosphereDataDelegate(UIntPtr scenePointer);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void RemoveZeroCornerBodiesDelegate(UIntPtr scenePointer);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SendMouseKeyEventDelegate(int keyId, [MarshalAs(UnmanagedType.U1)] bool isDown);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetFrameForAtmosphereDelegate(UIntPtr scenePointer, float tod, float cameraElevation, [MarshalAs(UnmanagedType.U1)] bool forceLoadTextures);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetMousePosDelegate(int posX, int posY);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetMouseVisibleDelegate([MarshalAs(UnmanagedType.U1)] bool value);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetPoliticalColorDelegate(UIntPtr scenePointer, byte[] value);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetSeasonTimeFactorDelegate(UIntPtr scenePointer, float seasonTimeFactor);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void SetTerrainDynamicParamsDelegate(UIntPtr scenePointer, Vec3 dynamic_params);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void TickAmbientSoundsDelegate(UIntPtr scenePointer, int terrainType);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void TickStepSoundDelegate(UIntPtr scenePointer, UIntPtr visualsPointer, int faceIndexterrainType, int soundType);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void TickVisualsDelegate(UIntPtr scenePointer, float tod, IntPtr ticked_map_meshes, int tickedMapMeshesCount);

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	[SuppressUnmanagedCodeSecurity]
	[MonoNativeFunctionWrapper]
	public delegate void ValidateTerrainSoundIdsDelegate();

	private static readonly Encoding _utf8 = Encoding.UTF8;

	public static GetAccessiblePointNearPositionDelegate call_GetAccessiblePointNearPositionDelegate;

	public static GetBattleSceneIndexMapDelegate call_GetBattleSceneIndexMapDelegate;

	public static GetBattleSceneIndexMapResolutionDelegate call_GetBattleSceneIndexMapResolutionDelegate;

	public static GetColorGradeGridDataDelegate call_GetColorGradeGridDataDelegate;

	public static GetFaceIndexForMultiplePositionsDelegate call_GetFaceIndexForMultiplePositionsDelegate;

	public static GetMouseVisibleDelegate call_GetMouseVisibleDelegate;

	public static GetSeasonTimeFactorDelegate call_GetSeasonTimeFactorDelegate;

	public static LoadAtmosphereDataDelegate call_LoadAtmosphereDataDelegate;

	public static RemoveZeroCornerBodiesDelegate call_RemoveZeroCornerBodiesDelegate;

	public static SendMouseKeyEventDelegate call_SendMouseKeyEventDelegate;

	public static SetFrameForAtmosphereDelegate call_SetFrameForAtmosphereDelegate;

	public static SetMousePosDelegate call_SetMousePosDelegate;

	public static SetMouseVisibleDelegate call_SetMouseVisibleDelegate;

	public static SetPoliticalColorDelegate call_SetPoliticalColorDelegate;

	public static SetSeasonTimeFactorDelegate call_SetSeasonTimeFactorDelegate;

	public static SetTerrainDynamicParamsDelegate call_SetTerrainDynamicParamsDelegate;

	public static TickAmbientSoundsDelegate call_TickAmbientSoundsDelegate;

	public static TickStepSoundDelegate call_TickStepSoundDelegate;

	public static TickVisualsDelegate call_TickVisualsDelegate;

	public static ValidateTerrainSoundIdsDelegate call_ValidateTerrainSoundIdsDelegate;

	public Vec3 GetAccessiblePointNearPosition(UIntPtr scenePointer, Vec2 position, float radius)
	{
		return call_GetAccessiblePointNearPositionDelegate(scenePointer, position, radius);
	}

	public void GetBattleSceneIndexMap(UIntPtr scenePointer, byte[] indexData)
	{
		PinnedArrayData<byte> pinnedArrayData = new PinnedArrayData<byte>(indexData);
		IntPtr pointer = pinnedArrayData.Pointer;
		ManagedArray indexData2 = new ManagedArray(pointer, (indexData != null) ? indexData.Length : 0);
		call_GetBattleSceneIndexMapDelegate(scenePointer, indexData2);
		pinnedArrayData.Dispose();
	}

	public void GetBattleSceneIndexMapResolution(UIntPtr scenePointer, ref int width, ref int height)
	{
		call_GetBattleSceneIndexMapResolutionDelegate(scenePointer, ref width, ref height);
	}

	public void GetColorGradeGridData(UIntPtr scenePointer, byte[] snowData, string textureName)
	{
		PinnedArrayData<byte> pinnedArrayData = new PinnedArrayData<byte>(snowData);
		IntPtr pointer = pinnedArrayData.Pointer;
		ManagedArray snowData2 = new ManagedArray(pointer, (snowData != null) ? snowData.Length : 0);
		byte[] array = null;
		if (textureName != null)
		{
			int byteCount = _utf8.GetByteCount(textureName);
			array = ((byteCount < 1024) ? CallbackStringBufferManager.StringBuffer0 : new byte[byteCount + 1]);
			_utf8.GetBytes(textureName, 0, textureName.Length, array, 0);
			array[byteCount] = 0;
		}
		call_GetColorGradeGridDataDelegate(scenePointer, snowData2, array);
		pinnedArrayData.Dispose();
	}

	public void GetFaceIndexForMultiplePositions(UIntPtr scenePointer, int movedPartyCount, float[] positionArray, PathFaceRecord[] resultArray, bool check_if_disabled, bool check_height)
	{
		PinnedArrayData<float> pinnedArrayData = new PinnedArrayData<float>(positionArray);
		IntPtr pointer = pinnedArrayData.Pointer;
		PinnedArrayData<PathFaceRecord> pinnedArrayData2 = new PinnedArrayData<PathFaceRecord>(resultArray);
		IntPtr pointer2 = pinnedArrayData2.Pointer;
		call_GetFaceIndexForMultiplePositionsDelegate(scenePointer, movedPartyCount, pointer, pointer2, check_if_disabled, check_height);
		pinnedArrayData.Dispose();
		pinnedArrayData2.Dispose();
	}

	public bool GetMouseVisible()
	{
		return call_GetMouseVisibleDelegate();
	}

	public float GetSeasonTimeFactor(UIntPtr scenePointer)
	{
		return call_GetSeasonTimeFactorDelegate(scenePointer);
	}

	public void LoadAtmosphereData(UIntPtr scenePointer)
	{
		call_LoadAtmosphereDataDelegate(scenePointer);
	}

	public void RemoveZeroCornerBodies(UIntPtr scenePointer)
	{
		call_RemoveZeroCornerBodiesDelegate(scenePointer);
	}

	public void SendMouseKeyEvent(int keyId, bool isDown)
	{
		call_SendMouseKeyEventDelegate(keyId, isDown);
	}

	public void SetFrameForAtmosphere(UIntPtr scenePointer, float tod, float cameraElevation, bool forceLoadTextures)
	{
		call_SetFrameForAtmosphereDelegate(scenePointer, tod, cameraElevation, forceLoadTextures);
	}

	public void SetMousePos(int posX, int posY)
	{
		call_SetMousePosDelegate(posX, posY);
	}

	public void SetMouseVisible(bool value)
	{
		call_SetMouseVisibleDelegate(value);
	}

	public void SetPoliticalColor(UIntPtr scenePointer, string value)
	{
		byte[] array = null;
		if (value != null)
		{
			int byteCount = _utf8.GetByteCount(value);
			array = ((byteCount < 1024) ? CallbackStringBufferManager.StringBuffer0 : new byte[byteCount + 1]);
			_utf8.GetBytes(value, 0, value.Length, array, 0);
			array[byteCount] = 0;
		}
		call_SetPoliticalColorDelegate(scenePointer, array);
	}

	public void SetSeasonTimeFactor(UIntPtr scenePointer, float seasonTimeFactor)
	{
		call_SetSeasonTimeFactorDelegate(scenePointer, seasonTimeFactor);
	}

	public void SetTerrainDynamicParams(UIntPtr scenePointer, Vec3 dynamic_params)
	{
		call_SetTerrainDynamicParamsDelegate(scenePointer, dynamic_params);
	}

	public void TickAmbientSounds(UIntPtr scenePointer, int terrainType)
	{
		call_TickAmbientSoundsDelegate(scenePointer, terrainType);
	}

	public void TickStepSound(UIntPtr scenePointer, UIntPtr visualsPointer, int faceIndexterrainType, int soundType)
	{
		call_TickStepSoundDelegate(scenePointer, visualsPointer, faceIndexterrainType, soundType);
	}

	public void TickVisuals(UIntPtr scenePointer, float tod, UIntPtr[] ticked_map_meshes, int tickedMapMeshesCount)
	{
		PinnedArrayData<UIntPtr> pinnedArrayData = new PinnedArrayData<UIntPtr>(ticked_map_meshes);
		IntPtr pointer = pinnedArrayData.Pointer;
		call_TickVisualsDelegate(scenePointer, tod, pointer, tickedMapMeshesCount);
		pinnedArrayData.Dispose();
	}

	public void ValidateTerrainSoundIds()
	{
		call_ValidateTerrainSoundIdsDelegate();
	}
}
