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
				"TaleWorlds.Engine.IAsyncTask",
				new ScriptingInterfaceOfIAsyncTask()
			},
			{
				"TaleWorlds.Engine.IBodyPart",
				new ScriptingInterfaceOfIBodyPart()
			},
			{
				"TaleWorlds.Engine.ICamera",
				new ScriptingInterfaceOfICamera()
			},
			{
				"TaleWorlds.Engine.IClothSimulatorComponent",
				new ScriptingInterfaceOfIClothSimulatorComponent()
			},
			{
				"TaleWorlds.Engine.ICompositeComponent",
				new ScriptingInterfaceOfICompositeComponent()
			},
			{
				"TaleWorlds.Engine.IConfig",
				new ScriptingInterfaceOfIConfig()
			},
			{
				"TaleWorlds.Engine.IDebug",
				new ScriptingInterfaceOfIDebug()
			},
			{
				"TaleWorlds.Engine.IDecal",
				new ScriptingInterfaceOfIDecal()
			},
			{
				"TaleWorlds.Engine.IEngineSizeChecker",
				new ScriptingInterfaceOfIEngineSizeChecker()
			},
			{
				"TaleWorlds.Engine.IGameEntity",
				new ScriptingInterfaceOfIGameEntity()
			},
			{
				"TaleWorlds.Engine.IGameEntityComponent",
				new ScriptingInterfaceOfIGameEntityComponent()
			},
			{
				"TaleWorlds.Engine.IHighlights",
				new ScriptingInterfaceOfIHighlights()
			},
			{
				"TaleWorlds.Engine.IImgui",
				new ScriptingInterfaceOfIImgui()
			},
			{
				"TaleWorlds.Engine.IInput",
				new ScriptingInterfaceOfIInput()
			},
			{
				"TaleWorlds.Engine.ILight",
				new ScriptingInterfaceOfILight()
			},
			{
				"TaleWorlds.Engine.IManagedMeshEditOperations",
				new ScriptingInterfaceOfIManagedMeshEditOperations()
			},
			{
				"TaleWorlds.Engine.IMaterial",
				new ScriptingInterfaceOfIMaterial()
			},
			{
				"TaleWorlds.Engine.IMesh",
				new ScriptingInterfaceOfIMesh()
			},
			{
				"TaleWorlds.Engine.IMeshBuilder",
				new ScriptingInterfaceOfIMeshBuilder()
			},
			{
				"TaleWorlds.Engine.IMetaMesh",
				new ScriptingInterfaceOfIMetaMesh()
			},
			{
				"TaleWorlds.Engine.IMouseManager",
				new ScriptingInterfaceOfIMouseManager()
			},
			{
				"TaleWorlds.Engine.IMusic",
				new ScriptingInterfaceOfIMusic()
			},
			{
				"TaleWorlds.Engine.IParticleSystem",
				new ScriptingInterfaceOfIParticleSystem()
			},
			{
				"TaleWorlds.Engine.IPath",
				new ScriptingInterfaceOfIPath()
			},
			{
				"TaleWorlds.Engine.IPhysicsMaterial",
				new ScriptingInterfaceOfIPhysicsMaterial()
			},
			{
				"TaleWorlds.Engine.IPhysicsShape",
				new ScriptingInterfaceOfIPhysicsShape()
			},
			{
				"TaleWorlds.Engine.IScene",
				new ScriptingInterfaceOfIScene()
			},
			{
				"TaleWorlds.Engine.ISceneView",
				new ScriptingInterfaceOfISceneView()
			},
			{
				"TaleWorlds.Engine.IScreen",
				new ScriptingInterfaceOfIScreen()
			},
			{
				"TaleWorlds.Engine.IScriptComponent",
				new ScriptingInterfaceOfIScriptComponent()
			},
			{
				"TaleWorlds.Engine.IShader",
				new ScriptingInterfaceOfIShader()
			},
			{
				"TaleWorlds.Engine.ISkeleton",
				new ScriptingInterfaceOfISkeleton()
			},
			{
				"TaleWorlds.Engine.ISoundEvent",
				new ScriptingInterfaceOfISoundEvent()
			},
			{
				"TaleWorlds.Engine.ISoundManager",
				new ScriptingInterfaceOfISoundManager()
			},
			{
				"TaleWorlds.Engine.ITableauView",
				new ScriptingInterfaceOfITableauView()
			},
			{
				"TaleWorlds.Engine.ITexture",
				new ScriptingInterfaceOfITexture()
			},
			{
				"TaleWorlds.Engine.ITextureView",
				new ScriptingInterfaceOfITextureView()
			},
			{
				"TaleWorlds.Engine.IThumbnailCreatorView",
				new ScriptingInterfaceOfIThumbnailCreatorView()
			},
			{
				"TaleWorlds.Engine.ITime",
				new ScriptingInterfaceOfITime()
			},
			{
				"TaleWorlds.Engine.ITwoDimensionView",
				new ScriptingInterfaceOfITwoDimensionView()
			},
			{
				"TaleWorlds.Engine.IUtil",
				new ScriptingInterfaceOfIUtil()
			},
			{
				"TaleWorlds.Engine.IVideoPlayerView",
				new ScriptingInterfaceOfIVideoPlayerView()
			},
			{
				"TaleWorlds.Engine.IView",
				new ScriptingInterfaceOfIView()
			}
		};
	}

	public static void SetFunctionPointer(int id, IntPtr pointer)
	{
		switch (id)
		{
		case 0:
			ScriptingInterfaceOfIAsyncTask.call_CreateWithDelegateDelegate = (ScriptingInterfaceOfIAsyncTask.CreateWithDelegateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIAsyncTask.CreateWithDelegateDelegate));
			break;
		case 1:
			ScriptingInterfaceOfIAsyncTask.call_InvokeDelegate = (ScriptingInterfaceOfIAsyncTask.InvokeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIAsyncTask.InvokeDelegate));
			break;
		case 2:
			ScriptingInterfaceOfIAsyncTask.call_WaitDelegate = (ScriptingInterfaceOfIAsyncTask.WaitDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIAsyncTask.WaitDelegate));
			break;
		case 3:
			ScriptingInterfaceOfIBodyPart.call_DoSegmentsIntersectDelegate = (ScriptingInterfaceOfIBodyPart.DoSegmentsIntersectDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIBodyPart.DoSegmentsIntersectDelegate));
			break;
		case 4:
			ScriptingInterfaceOfICamera.call_CheckEntityVisibilityDelegate = (ScriptingInterfaceOfICamera.CheckEntityVisibilityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.CheckEntityVisibilityDelegate));
			break;
		case 5:
			ScriptingInterfaceOfICamera.call_ConstructCameraFromPositionElevationBearingDelegate = (ScriptingInterfaceOfICamera.ConstructCameraFromPositionElevationBearingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.ConstructCameraFromPositionElevationBearingDelegate));
			break;
		case 6:
			ScriptingInterfaceOfICamera.call_CreateCameraDelegate = (ScriptingInterfaceOfICamera.CreateCameraDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.CreateCameraDelegate));
			break;
		case 7:
			ScriptingInterfaceOfICamera.call_EnclosesPointDelegate = (ScriptingInterfaceOfICamera.EnclosesPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.EnclosesPointDelegate));
			break;
		case 8:
			ScriptingInterfaceOfICamera.call_FillParametersFromDelegate = (ScriptingInterfaceOfICamera.FillParametersFromDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.FillParametersFromDelegate));
			break;
		case 9:
			ScriptingInterfaceOfICamera.call_GetAspectRatioDelegate = (ScriptingInterfaceOfICamera.GetAspectRatioDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.GetAspectRatioDelegate));
			break;
		case 10:
			ScriptingInterfaceOfICamera.call_GetEntityDelegate = (ScriptingInterfaceOfICamera.GetEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.GetEntityDelegate));
			break;
		case 11:
			ScriptingInterfaceOfICamera.call_GetFarDelegate = (ScriptingInterfaceOfICamera.GetFarDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.GetFarDelegate));
			break;
		case 12:
			ScriptingInterfaceOfICamera.call_GetFovHorizontalDelegate = (ScriptingInterfaceOfICamera.GetFovHorizontalDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.GetFovHorizontalDelegate));
			break;
		case 13:
			ScriptingInterfaceOfICamera.call_GetFovVerticalDelegate = (ScriptingInterfaceOfICamera.GetFovVerticalDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.GetFovVerticalDelegate));
			break;
		case 14:
			ScriptingInterfaceOfICamera.call_GetFrameDelegate = (ScriptingInterfaceOfICamera.GetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.GetFrameDelegate));
			break;
		case 15:
			ScriptingInterfaceOfICamera.call_GetHorizontalFovDelegate = (ScriptingInterfaceOfICamera.GetHorizontalFovDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.GetHorizontalFovDelegate));
			break;
		case 16:
			ScriptingInterfaceOfICamera.call_GetNearDelegate = (ScriptingInterfaceOfICamera.GetNearDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.GetNearDelegate));
			break;
		case 17:
			ScriptingInterfaceOfICamera.call_GetNearPlanePointsDelegate = (ScriptingInterfaceOfICamera.GetNearPlanePointsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.GetNearPlanePointsDelegate));
			break;
		case 18:
			ScriptingInterfaceOfICamera.call_GetNearPlanePointsStaticDelegate = (ScriptingInterfaceOfICamera.GetNearPlanePointsStaticDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.GetNearPlanePointsStaticDelegate));
			break;
		case 19:
			ScriptingInterfaceOfICamera.call_GetViewProjMatrixDelegate = (ScriptingInterfaceOfICamera.GetViewProjMatrixDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.GetViewProjMatrixDelegate));
			break;
		case 20:
			ScriptingInterfaceOfICamera.call_LookAtDelegate = (ScriptingInterfaceOfICamera.LookAtDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.LookAtDelegate));
			break;
		case 21:
			ScriptingInterfaceOfICamera.call_ReleaseDelegate = (ScriptingInterfaceOfICamera.ReleaseDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.ReleaseDelegate));
			break;
		case 22:
			ScriptingInterfaceOfICamera.call_ReleaseCameraEntityDelegate = (ScriptingInterfaceOfICamera.ReleaseCameraEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.ReleaseCameraEntityDelegate));
			break;
		case 23:
			ScriptingInterfaceOfICamera.call_RenderFrustrumDelegate = (ScriptingInterfaceOfICamera.RenderFrustrumDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.RenderFrustrumDelegate));
			break;
		case 24:
			ScriptingInterfaceOfICamera.call_ScreenSpaceRayProjectionDelegate = (ScriptingInterfaceOfICamera.ScreenSpaceRayProjectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.ScreenSpaceRayProjectionDelegate));
			break;
		case 25:
			ScriptingInterfaceOfICamera.call_SetEntityDelegate = (ScriptingInterfaceOfICamera.SetEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.SetEntityDelegate));
			break;
		case 26:
			ScriptingInterfaceOfICamera.call_SetFovHorizontalDelegate = (ScriptingInterfaceOfICamera.SetFovHorizontalDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.SetFovHorizontalDelegate));
			break;
		case 27:
			ScriptingInterfaceOfICamera.call_SetFovVerticalDelegate = (ScriptingInterfaceOfICamera.SetFovVerticalDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.SetFovVerticalDelegate));
			break;
		case 28:
			ScriptingInterfaceOfICamera.call_SetFrameDelegate = (ScriptingInterfaceOfICamera.SetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.SetFrameDelegate));
			break;
		case 29:
			ScriptingInterfaceOfICamera.call_SetPositionDelegate = (ScriptingInterfaceOfICamera.SetPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.SetPositionDelegate));
			break;
		case 30:
			ScriptingInterfaceOfICamera.call_SetViewVolumeDelegate = (ScriptingInterfaceOfICamera.SetViewVolumeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.SetViewVolumeDelegate));
			break;
		case 31:
			ScriptingInterfaceOfICamera.call_ViewportPointToWorldRayDelegate = (ScriptingInterfaceOfICamera.ViewportPointToWorldRayDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.ViewportPointToWorldRayDelegate));
			break;
		case 32:
			ScriptingInterfaceOfICamera.call_WorldPointToViewportPointDelegate = (ScriptingInterfaceOfICamera.WorldPointToViewportPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICamera.WorldPointToViewportPointDelegate));
			break;
		case 33:
			ScriptingInterfaceOfIClothSimulatorComponent.call_SetMaxDistanceMultiplierDelegate = (ScriptingInterfaceOfIClothSimulatorComponent.SetMaxDistanceMultiplierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIClothSimulatorComponent.SetMaxDistanceMultiplierDelegate));
			break;
		case 34:
			ScriptingInterfaceOfICompositeComponent.call_AddComponentDelegate = (ScriptingInterfaceOfICompositeComponent.AddComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.AddComponentDelegate));
			break;
		case 35:
			ScriptingInterfaceOfICompositeComponent.call_AddMultiMeshDelegate = (ScriptingInterfaceOfICompositeComponent.AddMultiMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.AddMultiMeshDelegate));
			break;
		case 36:
			ScriptingInterfaceOfICompositeComponent.call_AddPrefabEntityDelegate = (ScriptingInterfaceOfICompositeComponent.AddPrefabEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.AddPrefabEntityDelegate));
			break;
		case 37:
			ScriptingInterfaceOfICompositeComponent.call_CreateCompositeComponentDelegate = (ScriptingInterfaceOfICompositeComponent.CreateCompositeComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.CreateCompositeComponentDelegate));
			break;
		case 38:
			ScriptingInterfaceOfICompositeComponent.call_CreateCopyDelegate = (ScriptingInterfaceOfICompositeComponent.CreateCopyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.CreateCopyDelegate));
			break;
		case 39:
			ScriptingInterfaceOfICompositeComponent.call_GetBoundingBoxDelegate = (ScriptingInterfaceOfICompositeComponent.GetBoundingBoxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.GetBoundingBoxDelegate));
			break;
		case 40:
			ScriptingInterfaceOfICompositeComponent.call_GetFactor1Delegate = (ScriptingInterfaceOfICompositeComponent.GetFactor1Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.GetFactor1Delegate));
			break;
		case 41:
			ScriptingInterfaceOfICompositeComponent.call_GetFactor2Delegate = (ScriptingInterfaceOfICompositeComponent.GetFactor2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.GetFactor2Delegate));
			break;
		case 42:
			ScriptingInterfaceOfICompositeComponent.call_GetFirstMetaMeshDelegate = (ScriptingInterfaceOfICompositeComponent.GetFirstMetaMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.GetFirstMetaMeshDelegate));
			break;
		case 43:
			ScriptingInterfaceOfICompositeComponent.call_GetFrameDelegate = (ScriptingInterfaceOfICompositeComponent.GetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.GetFrameDelegate));
			break;
		case 44:
			ScriptingInterfaceOfICompositeComponent.call_GetVectorUserDataDelegate = (ScriptingInterfaceOfICompositeComponent.GetVectorUserDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.GetVectorUserDataDelegate));
			break;
		case 45:
			ScriptingInterfaceOfICompositeComponent.call_IsVisibleDelegate = (ScriptingInterfaceOfICompositeComponent.IsVisibleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.IsVisibleDelegate));
			break;
		case 46:
			ScriptingInterfaceOfICompositeComponent.call_ReleaseDelegate = (ScriptingInterfaceOfICompositeComponent.ReleaseDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.ReleaseDelegate));
			break;
		case 47:
			ScriptingInterfaceOfICompositeComponent.call_SetFactor1Delegate = (ScriptingInterfaceOfICompositeComponent.SetFactor1Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.SetFactor1Delegate));
			break;
		case 48:
			ScriptingInterfaceOfICompositeComponent.call_SetFactor2Delegate = (ScriptingInterfaceOfICompositeComponent.SetFactor2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.SetFactor2Delegate));
			break;
		case 49:
			ScriptingInterfaceOfICompositeComponent.call_SetFrameDelegate = (ScriptingInterfaceOfICompositeComponent.SetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.SetFrameDelegate));
			break;
		case 50:
			ScriptingInterfaceOfICompositeComponent.call_SetMaterialDelegate = (ScriptingInterfaceOfICompositeComponent.SetMaterialDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.SetMaterialDelegate));
			break;
		case 51:
			ScriptingInterfaceOfICompositeComponent.call_SetVectorArgumentDelegate = (ScriptingInterfaceOfICompositeComponent.SetVectorArgumentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.SetVectorArgumentDelegate));
			break;
		case 52:
			ScriptingInterfaceOfICompositeComponent.call_SetVectorUserDataDelegate = (ScriptingInterfaceOfICompositeComponent.SetVectorUserDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.SetVectorUserDataDelegate));
			break;
		case 53:
			ScriptingInterfaceOfICompositeComponent.call_SetVisibilityMaskDelegate = (ScriptingInterfaceOfICompositeComponent.SetVisibilityMaskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.SetVisibilityMaskDelegate));
			break;
		case 54:
			ScriptingInterfaceOfICompositeComponent.call_SetVisibleDelegate = (ScriptingInterfaceOfICompositeComponent.SetVisibleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfICompositeComponent.SetVisibleDelegate));
			break;
		case 55:
			ScriptingInterfaceOfIConfig.call_ApplyDelegate = (ScriptingInterfaceOfIConfig.ApplyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.ApplyDelegate));
			break;
		case 56:
			ScriptingInterfaceOfIConfig.call_ApplyConfigChangesDelegate = (ScriptingInterfaceOfIConfig.ApplyConfigChangesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.ApplyConfigChangesDelegate));
			break;
		case 57:
			ScriptingInterfaceOfIConfig.call_AutoSaveInMinutesDelegate = (ScriptingInterfaceOfIConfig.AutoSaveInMinutesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.AutoSaveInMinutesDelegate));
			break;
		case 58:
			ScriptingInterfaceOfIConfig.call_CheckGFXSupportStatusDelegate = (ScriptingInterfaceOfIConfig.CheckGFXSupportStatusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.CheckGFXSupportStatusDelegate));
			break;
		case 59:
			ScriptingInterfaceOfIConfig.call_GetAutoGFXQualityDelegate = (ScriptingInterfaceOfIConfig.GetAutoGFXQualityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetAutoGFXQualityDelegate));
			break;
		case 60:
			ScriptingInterfaceOfIConfig.call_GetCharacterDetailDelegate = (ScriptingInterfaceOfIConfig.GetCharacterDetailDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetCharacterDetailDelegate));
			break;
		case 61:
			ScriptingInterfaceOfIConfig.call_GetCheatModeDelegate = (ScriptingInterfaceOfIConfig.GetCheatModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetCheatModeDelegate));
			break;
		case 62:
			ScriptingInterfaceOfIConfig.call_GetCurrentSoundDeviceIndexDelegate = (ScriptingInterfaceOfIConfig.GetCurrentSoundDeviceIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetCurrentSoundDeviceIndexDelegate));
			break;
		case 63:
			ScriptingInterfaceOfIConfig.call_GetDebugLoginPasswordDelegate = (ScriptingInterfaceOfIConfig.GetDebugLoginPasswordDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetDebugLoginPasswordDelegate));
			break;
		case 64:
			ScriptingInterfaceOfIConfig.call_GetDebugLoginUserNameDelegate = (ScriptingInterfaceOfIConfig.GetDebugLoginUserNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetDebugLoginUserNameDelegate));
			break;
		case 65:
			ScriptingInterfaceOfIConfig.call_GetDefaultRGLConfigDelegate = (ScriptingInterfaceOfIConfig.GetDefaultRGLConfigDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetDefaultRGLConfigDelegate));
			break;
		case 66:
			ScriptingInterfaceOfIConfig.call_GetDesktopResolutionDelegate = (ScriptingInterfaceOfIConfig.GetDesktopResolutionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetDesktopResolutionDelegate));
			break;
		case 67:
			ScriptingInterfaceOfIConfig.call_GetDevelopmentModeDelegate = (ScriptingInterfaceOfIConfig.GetDevelopmentModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetDevelopmentModeDelegate));
			break;
		case 68:
			ScriptingInterfaceOfIConfig.call_GetDisableGuiMessagesDelegate = (ScriptingInterfaceOfIConfig.GetDisableGuiMessagesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetDisableGuiMessagesDelegate));
			break;
		case 69:
			ScriptingInterfaceOfIConfig.call_GetDisableSoundDelegate = (ScriptingInterfaceOfIConfig.GetDisableSoundDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetDisableSoundDelegate));
			break;
		case 70:
			ScriptingInterfaceOfIConfig.call_GetDlssOptionCountDelegate = (ScriptingInterfaceOfIConfig.GetDlssOptionCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetDlssOptionCountDelegate));
			break;
		case 71:
			ScriptingInterfaceOfIConfig.call_GetDlssTechniqueDelegate = (ScriptingInterfaceOfIConfig.GetDlssTechniqueDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetDlssTechniqueDelegate));
			break;
		case 72:
			ScriptingInterfaceOfIConfig.call_GetDoLocalizationCheckAtStartupDelegate = (ScriptingInterfaceOfIConfig.GetDoLocalizationCheckAtStartupDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetDoLocalizationCheckAtStartupDelegate));
			break;
		case 73:
			ScriptingInterfaceOfIConfig.call_GetEnableClothSimulationDelegate = (ScriptingInterfaceOfIConfig.GetEnableClothSimulationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetEnableClothSimulationDelegate));
			break;
		case 74:
			ScriptingInterfaceOfIConfig.call_GetEnableEditModeDelegate = (ScriptingInterfaceOfIConfig.GetEnableEditModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetEnableEditModeDelegate));
			break;
		case 75:
			ScriptingInterfaceOfIConfig.call_GetInvertMouseDelegate = (ScriptingInterfaceOfIConfig.GetInvertMouseDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetInvertMouseDelegate));
			break;
		case 76:
			ScriptingInterfaceOfIConfig.call_GetLastOpenedSceneDelegate = (ScriptingInterfaceOfIConfig.GetLastOpenedSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetLastOpenedSceneDelegate));
			break;
		case 77:
			ScriptingInterfaceOfIConfig.call_GetLocalizationDebugModeDelegate = (ScriptingInterfaceOfIConfig.GetLocalizationDebugModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetLocalizationDebugModeDelegate));
			break;
		case 78:
			ScriptingInterfaceOfIConfig.call_GetMonitorDeviceCountDelegate = (ScriptingInterfaceOfIConfig.GetMonitorDeviceCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetMonitorDeviceCountDelegate));
			break;
		case 79:
			ScriptingInterfaceOfIConfig.call_GetMonitorDeviceNameDelegate = (ScriptingInterfaceOfIConfig.GetMonitorDeviceNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetMonitorDeviceNameDelegate));
			break;
		case 80:
			ScriptingInterfaceOfIConfig.call_GetRefreshRateAtIndexDelegate = (ScriptingInterfaceOfIConfig.GetRefreshRateAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetRefreshRateAtIndexDelegate));
			break;
		case 81:
			ScriptingInterfaceOfIConfig.call_GetRefreshRateCountDelegate = (ScriptingInterfaceOfIConfig.GetRefreshRateCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetRefreshRateCountDelegate));
			break;
		case 82:
			ScriptingInterfaceOfIConfig.call_GetResolutionDelegate = (ScriptingInterfaceOfIConfig.GetResolutionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetResolutionDelegate));
			break;
		case 83:
			ScriptingInterfaceOfIConfig.call_GetResolutionAtIndexDelegate = (ScriptingInterfaceOfIConfig.GetResolutionAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetResolutionAtIndexDelegate));
			break;
		case 84:
			ScriptingInterfaceOfIConfig.call_GetResolutionCountDelegate = (ScriptingInterfaceOfIConfig.GetResolutionCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetResolutionCountDelegate));
			break;
		case 85:
			ScriptingInterfaceOfIConfig.call_GetRGLConfigDelegate = (ScriptingInterfaceOfIConfig.GetRGLConfigDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetRGLConfigDelegate));
			break;
		case 86:
			ScriptingInterfaceOfIConfig.call_GetRGLConfigForDefaultSettingsDelegate = (ScriptingInterfaceOfIConfig.GetRGLConfigForDefaultSettingsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetRGLConfigForDefaultSettingsDelegate));
			break;
		case 87:
			ScriptingInterfaceOfIConfig.call_GetSoundDeviceCountDelegate = (ScriptingInterfaceOfIConfig.GetSoundDeviceCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetSoundDeviceCountDelegate));
			break;
		case 88:
			ScriptingInterfaceOfIConfig.call_GetSoundDeviceNameDelegate = (ScriptingInterfaceOfIConfig.GetSoundDeviceNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetSoundDeviceNameDelegate));
			break;
		case 89:
			ScriptingInterfaceOfIConfig.call_GetTableauCacheModeDelegate = (ScriptingInterfaceOfIConfig.GetTableauCacheModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetTableauCacheModeDelegate));
			break;
		case 90:
			ScriptingInterfaceOfIConfig.call_GetUIDebugModeDelegate = (ScriptingInterfaceOfIConfig.GetUIDebugModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetUIDebugModeDelegate));
			break;
		case 91:
			ScriptingInterfaceOfIConfig.call_GetUIDoNotUseGeneratedPrefabsDelegate = (ScriptingInterfaceOfIConfig.GetUIDoNotUseGeneratedPrefabsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetUIDoNotUseGeneratedPrefabsDelegate));
			break;
		case 92:
			ScriptingInterfaceOfIConfig.call_GetVideoDeviceCountDelegate = (ScriptingInterfaceOfIConfig.GetVideoDeviceCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetVideoDeviceCountDelegate));
			break;
		case 93:
			ScriptingInterfaceOfIConfig.call_GetVideoDeviceNameDelegate = (ScriptingInterfaceOfIConfig.GetVideoDeviceNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.GetVideoDeviceNameDelegate));
			break;
		case 94:
			ScriptingInterfaceOfIConfig.call_Is120HzAvailableDelegate = (ScriptingInterfaceOfIConfig.Is120HzAvailableDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.Is120HzAvailableDelegate));
			break;
		case 95:
			ScriptingInterfaceOfIConfig.call_IsDlssAvailableDelegate = (ScriptingInterfaceOfIConfig.IsDlssAvailableDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.IsDlssAvailableDelegate));
			break;
		case 96:
			ScriptingInterfaceOfIConfig.call_ReadRGLConfigFilesDelegate = (ScriptingInterfaceOfIConfig.ReadRGLConfigFilesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.ReadRGLConfigFilesDelegate));
			break;
		case 97:
			ScriptingInterfaceOfIConfig.call_RefreshOptionsDataDelegate = (ScriptingInterfaceOfIConfig.RefreshOptionsDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.RefreshOptionsDataDelegate));
			break;
		case 98:
			ScriptingInterfaceOfIConfig.call_SaveRGLConfigDelegate = (ScriptingInterfaceOfIConfig.SaveRGLConfigDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.SaveRGLConfigDelegate));
			break;
		case 99:
			ScriptingInterfaceOfIConfig.call_SetAutoConfigWrtHardwareDelegate = (ScriptingInterfaceOfIConfig.SetAutoConfigWrtHardwareDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.SetAutoConfigWrtHardwareDelegate));
			break;
		case 100:
			ScriptingInterfaceOfIConfig.call_SetBrightnessDelegate = (ScriptingInterfaceOfIConfig.SetBrightnessDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.SetBrightnessDelegate));
			break;
		case 101:
			ScriptingInterfaceOfIConfig.call_SetCustomResolutionDelegate = (ScriptingInterfaceOfIConfig.SetCustomResolutionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.SetCustomResolutionDelegate));
			break;
		case 102:
			ScriptingInterfaceOfIConfig.call_SetDefaultGameConfigDelegate = (ScriptingInterfaceOfIConfig.SetDefaultGameConfigDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.SetDefaultGameConfigDelegate));
			break;
		case 103:
			ScriptingInterfaceOfIConfig.call_SetRGLConfigDelegate = (ScriptingInterfaceOfIConfig.SetRGLConfigDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.SetRGLConfigDelegate));
			break;
		case 104:
			ScriptingInterfaceOfIConfig.call_SetSharpenAmountDelegate = (ScriptingInterfaceOfIConfig.SetSharpenAmountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.SetSharpenAmountDelegate));
			break;
		case 105:
			ScriptingInterfaceOfIConfig.call_SetSoundDeviceDelegate = (ScriptingInterfaceOfIConfig.SetSoundDeviceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIConfig.SetSoundDeviceDelegate));
			break;
		case 106:
			ScriptingInterfaceOfIDebug.call_AbortGameDelegate = (ScriptingInterfaceOfIDebug.AbortGameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.AbortGameDelegate));
			break;
		case 107:
			ScriptingInterfaceOfIDebug.call_AssertMemoryUsageDelegate = (ScriptingInterfaceOfIDebug.AssertMemoryUsageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.AssertMemoryUsageDelegate));
			break;
		case 108:
			ScriptingInterfaceOfIDebug.call_ClearAllDebugRenderObjectsDelegate = (ScriptingInterfaceOfIDebug.ClearAllDebugRenderObjectsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.ClearAllDebugRenderObjectsDelegate));
			break;
		case 109:
			ScriptingInterfaceOfIDebug.call_ContentWarningDelegate = (ScriptingInterfaceOfIDebug.ContentWarningDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.ContentWarningDelegate));
			break;
		case 110:
			ScriptingInterfaceOfIDebug.call_EchoCommandWindowDelegate = (ScriptingInterfaceOfIDebug.EchoCommandWindowDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.EchoCommandWindowDelegate));
			break;
		case 111:
			ScriptingInterfaceOfIDebug.call_ErrorDelegate = (ScriptingInterfaceOfIDebug.ErrorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.ErrorDelegate));
			break;
		case 112:
			ScriptingInterfaceOfIDebug.call_FailedAssertDelegate = (ScriptingInterfaceOfIDebug.FailedAssertDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.FailedAssertDelegate));
			break;
		case 113:
			ScriptingInterfaceOfIDebug.call_GetDebugVectorDelegate = (ScriptingInterfaceOfIDebug.GetDebugVectorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.GetDebugVectorDelegate));
			break;
		case 114:
			ScriptingInterfaceOfIDebug.call_GetShowDebugInfoDelegate = (ScriptingInterfaceOfIDebug.GetShowDebugInfoDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.GetShowDebugInfoDelegate));
			break;
		case 115:
			ScriptingInterfaceOfIDebug.call_IsErrorReportModeActiveDelegate = (ScriptingInterfaceOfIDebug.IsErrorReportModeActiveDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.IsErrorReportModeActiveDelegate));
			break;
		case 116:
			ScriptingInterfaceOfIDebug.call_IsErrorReportModePauseMissionDelegate = (ScriptingInterfaceOfIDebug.IsErrorReportModePauseMissionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.IsErrorReportModePauseMissionDelegate));
			break;
		case 117:
			ScriptingInterfaceOfIDebug.call_IsTestModeDelegate = (ScriptingInterfaceOfIDebug.IsTestModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.IsTestModeDelegate));
			break;
		case 118:
			ScriptingInterfaceOfIDebug.call_MessageBoxDelegate = (ScriptingInterfaceOfIDebug.MessageBoxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.MessageBoxDelegate));
			break;
		case 119:
			ScriptingInterfaceOfIDebug.call_PostWarningLineDelegate = (ScriptingInterfaceOfIDebug.PostWarningLineDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.PostWarningLineDelegate));
			break;
		case 120:
			ScriptingInterfaceOfIDebug.call_RenderDebugBoxObjectDelegate = (ScriptingInterfaceOfIDebug.RenderDebugBoxObjectDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.RenderDebugBoxObjectDelegate));
			break;
		case 121:
			ScriptingInterfaceOfIDebug.call_RenderDebugBoxObjectWithFrameDelegate = (ScriptingInterfaceOfIDebug.RenderDebugBoxObjectWithFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.RenderDebugBoxObjectWithFrameDelegate));
			break;
		case 122:
			ScriptingInterfaceOfIDebug.call_RenderDebugCapsuleDelegate = (ScriptingInterfaceOfIDebug.RenderDebugCapsuleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.RenderDebugCapsuleDelegate));
			break;
		case 123:
			ScriptingInterfaceOfIDebug.call_RenderDebugDirectionArrowDelegate = (ScriptingInterfaceOfIDebug.RenderDebugDirectionArrowDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.RenderDebugDirectionArrowDelegate));
			break;
		case 124:
			ScriptingInterfaceOfIDebug.call_RenderDebugFrameDelegate = (ScriptingInterfaceOfIDebug.RenderDebugFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.RenderDebugFrameDelegate));
			break;
		case 125:
			ScriptingInterfaceOfIDebug.call_RenderDebugLineDelegate = (ScriptingInterfaceOfIDebug.RenderDebugLineDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.RenderDebugLineDelegate));
			break;
		case 126:
			ScriptingInterfaceOfIDebug.call_RenderDebugRectDelegate = (ScriptingInterfaceOfIDebug.RenderDebugRectDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.RenderDebugRectDelegate));
			break;
		case 127:
			ScriptingInterfaceOfIDebug.call_RenderDebugRectWithColorDelegate = (ScriptingInterfaceOfIDebug.RenderDebugRectWithColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.RenderDebugRectWithColorDelegate));
			break;
		case 128:
			ScriptingInterfaceOfIDebug.call_RenderDebugSphereDelegate = (ScriptingInterfaceOfIDebug.RenderDebugSphereDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.RenderDebugSphereDelegate));
			break;
		case 129:
			ScriptingInterfaceOfIDebug.call_RenderDebugTextDelegate = (ScriptingInterfaceOfIDebug.RenderDebugTextDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.RenderDebugTextDelegate));
			break;
		case 130:
			ScriptingInterfaceOfIDebug.call_RenderDebugText3dDelegate = (ScriptingInterfaceOfIDebug.RenderDebugText3dDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.RenderDebugText3dDelegate));
			break;
		case 131:
			ScriptingInterfaceOfIDebug.call_SetDumpGenerationDisabledDelegate = (ScriptingInterfaceOfIDebug.SetDumpGenerationDisabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.SetDumpGenerationDisabledDelegate));
			break;
		case 132:
			ScriptingInterfaceOfIDebug.call_SetErrorReportSceneDelegate = (ScriptingInterfaceOfIDebug.SetErrorReportSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.SetErrorReportSceneDelegate));
			break;
		case 133:
			ScriptingInterfaceOfIDebug.call_SetShowDebugInfoDelegate = (ScriptingInterfaceOfIDebug.SetShowDebugInfoDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.SetShowDebugInfoDelegate));
			break;
		case 134:
			ScriptingInterfaceOfIDebug.call_SilentAssertDelegate = (ScriptingInterfaceOfIDebug.SilentAssertDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.SilentAssertDelegate));
			break;
		case 135:
			ScriptingInterfaceOfIDebug.call_WarningDelegate = (ScriptingInterfaceOfIDebug.WarningDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.WarningDelegate));
			break;
		case 136:
			ScriptingInterfaceOfIDebug.call_WriteDebugLineOnScreenDelegate = (ScriptingInterfaceOfIDebug.WriteDebugLineOnScreenDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.WriteDebugLineOnScreenDelegate));
			break;
		case 137:
			ScriptingInterfaceOfIDebug.call_WriteLineDelegate = (ScriptingInterfaceOfIDebug.WriteLineDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDebug.WriteLineDelegate));
			break;
		case 138:
			ScriptingInterfaceOfIDecal.call_CreateCopyDelegate = (ScriptingInterfaceOfIDecal.CreateCopyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDecal.CreateCopyDelegate));
			break;
		case 139:
			ScriptingInterfaceOfIDecal.call_CreateDecalDelegate = (ScriptingInterfaceOfIDecal.CreateDecalDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDecal.CreateDecalDelegate));
			break;
		case 140:
			ScriptingInterfaceOfIDecal.call_GetFactor1Delegate = (ScriptingInterfaceOfIDecal.GetFactor1Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDecal.GetFactor1Delegate));
			break;
		case 141:
			ScriptingInterfaceOfIDecal.call_GetFrameDelegate = (ScriptingInterfaceOfIDecal.GetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDecal.GetFrameDelegate));
			break;
		case 142:
			ScriptingInterfaceOfIDecal.call_GetMaterialDelegate = (ScriptingInterfaceOfIDecal.GetMaterialDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDecal.GetMaterialDelegate));
			break;
		case 143:
			ScriptingInterfaceOfIDecal.call_SetFactor1Delegate = (ScriptingInterfaceOfIDecal.SetFactor1Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDecal.SetFactor1Delegate));
			break;
		case 144:
			ScriptingInterfaceOfIDecal.call_SetFactor1LinearDelegate = (ScriptingInterfaceOfIDecal.SetFactor1LinearDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDecal.SetFactor1LinearDelegate));
			break;
		case 145:
			ScriptingInterfaceOfIDecal.call_SetFrameDelegate = (ScriptingInterfaceOfIDecal.SetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDecal.SetFrameDelegate));
			break;
		case 146:
			ScriptingInterfaceOfIDecal.call_SetMaterialDelegate = (ScriptingInterfaceOfIDecal.SetMaterialDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDecal.SetMaterialDelegate));
			break;
		case 147:
			ScriptingInterfaceOfIDecal.call_SetVectorArgumentDelegate = (ScriptingInterfaceOfIDecal.SetVectorArgumentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDecal.SetVectorArgumentDelegate));
			break;
		case 148:
			ScriptingInterfaceOfIDecal.call_SetVectorArgument2Delegate = (ScriptingInterfaceOfIDecal.SetVectorArgument2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIDecal.SetVectorArgument2Delegate));
			break;
		case 149:
			ScriptingInterfaceOfIEngineSizeChecker.call_GetEngineStructMemberOffsetDelegate = (ScriptingInterfaceOfIEngineSizeChecker.GetEngineStructMemberOffsetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIEngineSizeChecker.GetEngineStructMemberOffsetDelegate));
			break;
		case 150:
			ScriptingInterfaceOfIEngineSizeChecker.call_GetEngineStructSizeDelegate = (ScriptingInterfaceOfIEngineSizeChecker.GetEngineStructSizeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIEngineSizeChecker.GetEngineStructSizeDelegate));
			break;
		case 151:
			ScriptingInterfaceOfIGameEntity.call_ActivateRagdollDelegate = (ScriptingInterfaceOfIGameEntity.ActivateRagdollDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.ActivateRagdollDelegate));
			break;
		case 152:
			ScriptingInterfaceOfIGameEntity.call_AddAllMeshesOfGameEntityDelegate = (ScriptingInterfaceOfIGameEntity.AddAllMeshesOfGameEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddAllMeshesOfGameEntityDelegate));
			break;
		case 153:
			ScriptingInterfaceOfIGameEntity.call_AddChildDelegate = (ScriptingInterfaceOfIGameEntity.AddChildDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddChildDelegate));
			break;
		case 154:
			ScriptingInterfaceOfIGameEntity.call_AddComponentDelegate = (ScriptingInterfaceOfIGameEntity.AddComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddComponentDelegate));
			break;
		case 155:
			ScriptingInterfaceOfIGameEntity.call_AddDistanceJointDelegate = (ScriptingInterfaceOfIGameEntity.AddDistanceJointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddDistanceJointDelegate));
			break;
		case 156:
			ScriptingInterfaceOfIGameEntity.call_AddEditDataUserToAllMeshesDelegate = (ScriptingInterfaceOfIGameEntity.AddEditDataUserToAllMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddEditDataUserToAllMeshesDelegate));
			break;
		case 157:
			ScriptingInterfaceOfIGameEntity.call_AddLightDelegate = (ScriptingInterfaceOfIGameEntity.AddLightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddLightDelegate));
			break;
		case 158:
			ScriptingInterfaceOfIGameEntity.call_AddMeshDelegate = (ScriptingInterfaceOfIGameEntity.AddMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddMeshDelegate));
			break;
		case 159:
			ScriptingInterfaceOfIGameEntity.call_AddMeshToBoneDelegate = (ScriptingInterfaceOfIGameEntity.AddMeshToBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddMeshToBoneDelegate));
			break;
		case 160:
			ScriptingInterfaceOfIGameEntity.call_AddMultiMeshDelegate = (ScriptingInterfaceOfIGameEntity.AddMultiMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddMultiMeshDelegate));
			break;
		case 161:
			ScriptingInterfaceOfIGameEntity.call_AddMultiMeshToSkeletonDelegate = (ScriptingInterfaceOfIGameEntity.AddMultiMeshToSkeletonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddMultiMeshToSkeletonDelegate));
			break;
		case 162:
			ScriptingInterfaceOfIGameEntity.call_AddMultiMeshToSkeletonBoneDelegate = (ScriptingInterfaceOfIGameEntity.AddMultiMeshToSkeletonBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddMultiMeshToSkeletonBoneDelegate));
			break;
		case 163:
			ScriptingInterfaceOfIGameEntity.call_AddParticleSystemComponentDelegate = (ScriptingInterfaceOfIGameEntity.AddParticleSystemComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddParticleSystemComponentDelegate));
			break;
		case 164:
			ScriptingInterfaceOfIGameEntity.call_AddPhysicsDelegate = (ScriptingInterfaceOfIGameEntity.AddPhysicsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddPhysicsDelegate));
			break;
		case 165:
			ScriptingInterfaceOfIGameEntity.call_AddSphereAsBodyDelegate = (ScriptingInterfaceOfIGameEntity.AddSphereAsBodyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddSphereAsBodyDelegate));
			break;
		case 166:
			ScriptingInterfaceOfIGameEntity.call_AddTagDelegate = (ScriptingInterfaceOfIGameEntity.AddTagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AddTagDelegate));
			break;
		case 167:
			ScriptingInterfaceOfIGameEntity.call_ApplyAccelerationToDynamicBodyDelegate = (ScriptingInterfaceOfIGameEntity.ApplyAccelerationToDynamicBodyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.ApplyAccelerationToDynamicBodyDelegate));
			break;
		case 168:
			ScriptingInterfaceOfIGameEntity.call_ApplyForceToDynamicBodyDelegate = (ScriptingInterfaceOfIGameEntity.ApplyForceToDynamicBodyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.ApplyForceToDynamicBodyDelegate));
			break;
		case 169:
			ScriptingInterfaceOfIGameEntity.call_ApplyLocalForceToDynamicBodyDelegate = (ScriptingInterfaceOfIGameEntity.ApplyLocalForceToDynamicBodyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.ApplyLocalForceToDynamicBodyDelegate));
			break;
		case 170:
			ScriptingInterfaceOfIGameEntity.call_ApplyLocalImpulseToDynamicBodyDelegate = (ScriptingInterfaceOfIGameEntity.ApplyLocalImpulseToDynamicBodyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.ApplyLocalImpulseToDynamicBodyDelegate));
			break;
		case 171:
			ScriptingInterfaceOfIGameEntity.call_AttachNavigationMeshFacesDelegate = (ScriptingInterfaceOfIGameEntity.AttachNavigationMeshFacesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.AttachNavigationMeshFacesDelegate));
			break;
		case 172:
			ScriptingInterfaceOfIGameEntity.call_BreakPrefabDelegate = (ScriptingInterfaceOfIGameEntity.BreakPrefabDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.BreakPrefabDelegate));
			break;
		case 173:
			ScriptingInterfaceOfIGameEntity.call_BurstEntityParticleDelegate = (ScriptingInterfaceOfIGameEntity.BurstEntityParticleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.BurstEntityParticleDelegate));
			break;
		case 174:
			ScriptingInterfaceOfIGameEntity.call_CallScriptCallbacksDelegate = (ScriptingInterfaceOfIGameEntity.CallScriptCallbacksDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.CallScriptCallbacksDelegate));
			break;
		case 175:
			ScriptingInterfaceOfIGameEntity.call_ChangeMetaMeshOrRemoveItIfNotExistsDelegate = (ScriptingInterfaceOfIGameEntity.ChangeMetaMeshOrRemoveItIfNotExistsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.ChangeMetaMeshOrRemoveItIfNotExistsDelegate));
			break;
		case 176:
			ScriptingInterfaceOfIGameEntity.call_CheckPointWithOrientedBoundingBoxDelegate = (ScriptingInterfaceOfIGameEntity.CheckPointWithOrientedBoundingBoxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.CheckPointWithOrientedBoundingBoxDelegate));
			break;
		case 177:
			ScriptingInterfaceOfIGameEntity.call_CheckResourcesDelegate = (ScriptingInterfaceOfIGameEntity.CheckResourcesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.CheckResourcesDelegate));
			break;
		case 178:
			ScriptingInterfaceOfIGameEntity.call_ClearComponentsDelegate = (ScriptingInterfaceOfIGameEntity.ClearComponentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.ClearComponentsDelegate));
			break;
		case 179:
			ScriptingInterfaceOfIGameEntity.call_ClearEntityComponentsDelegate = (ScriptingInterfaceOfIGameEntity.ClearEntityComponentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.ClearEntityComponentsDelegate));
			break;
		case 180:
			ScriptingInterfaceOfIGameEntity.call_ClearOnlyOwnComponentsDelegate = (ScriptingInterfaceOfIGameEntity.ClearOnlyOwnComponentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.ClearOnlyOwnComponentsDelegate));
			break;
		case 181:
			ScriptingInterfaceOfIGameEntity.call_ComputeTrajectoryVolumeDelegate = (ScriptingInterfaceOfIGameEntity.ComputeTrajectoryVolumeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.ComputeTrajectoryVolumeDelegate));
			break;
		case 182:
			ScriptingInterfaceOfIGameEntity.call_CopyComponentsToSkeletonDelegate = (ScriptingInterfaceOfIGameEntity.CopyComponentsToSkeletonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.CopyComponentsToSkeletonDelegate));
			break;
		case 183:
			ScriptingInterfaceOfIGameEntity.call_CopyFromPrefabDelegate = (ScriptingInterfaceOfIGameEntity.CopyFromPrefabDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.CopyFromPrefabDelegate));
			break;
		case 184:
			ScriptingInterfaceOfIGameEntity.call_CopyScriptComponentFromAnotherEntityDelegate = (ScriptingInterfaceOfIGameEntity.CopyScriptComponentFromAnotherEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.CopyScriptComponentFromAnotherEntityDelegate));
			break;
		case 185:
			ScriptingInterfaceOfIGameEntity.call_CreateAndAddScriptComponentDelegate = (ScriptingInterfaceOfIGameEntity.CreateAndAddScriptComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.CreateAndAddScriptComponentDelegate));
			break;
		case 186:
			ScriptingInterfaceOfIGameEntity.call_CreateEmptyDelegate = (ScriptingInterfaceOfIGameEntity.CreateEmptyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.CreateEmptyDelegate));
			break;
		case 187:
			ScriptingInterfaceOfIGameEntity.call_CreateEmptyWithoutSceneDelegate = (ScriptingInterfaceOfIGameEntity.CreateEmptyWithoutSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.CreateEmptyWithoutSceneDelegate));
			break;
		case 188:
			ScriptingInterfaceOfIGameEntity.call_CreateFromPrefabDelegate = (ScriptingInterfaceOfIGameEntity.CreateFromPrefabDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.CreateFromPrefabDelegate));
			break;
		case 189:
			ScriptingInterfaceOfIGameEntity.call_CreateFromPrefabWithInitialFrameDelegate = (ScriptingInterfaceOfIGameEntity.CreateFromPrefabWithInitialFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.CreateFromPrefabWithInitialFrameDelegate));
			break;
		case 190:
			ScriptingInterfaceOfIGameEntity.call_DeselectEntityOnEditorDelegate = (ScriptingInterfaceOfIGameEntity.DeselectEntityOnEditorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.DeselectEntityOnEditorDelegate));
			break;
		case 191:
			ScriptingInterfaceOfIGameEntity.call_DisableContourDelegate = (ScriptingInterfaceOfIGameEntity.DisableContourDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.DisableContourDelegate));
			break;
		case 192:
			ScriptingInterfaceOfIGameEntity.call_DisableDynamicBodySimulationDelegate = (ScriptingInterfaceOfIGameEntity.DisableDynamicBodySimulationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.DisableDynamicBodySimulationDelegate));
			break;
		case 193:
			ScriptingInterfaceOfIGameEntity.call_DisableGravityDelegate = (ScriptingInterfaceOfIGameEntity.DisableGravityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.DisableGravityDelegate));
			break;
		case 194:
			ScriptingInterfaceOfIGameEntity.call_EnableDynamicBodyDelegate = (ScriptingInterfaceOfIGameEntity.EnableDynamicBodyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.EnableDynamicBodyDelegate));
			break;
		case 195:
			ScriptingInterfaceOfIGameEntity.call_FindWithNameDelegate = (ScriptingInterfaceOfIGameEntity.FindWithNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.FindWithNameDelegate));
			break;
		case 196:
			ScriptingInterfaceOfIGameEntity.call_FreezeDelegate = (ScriptingInterfaceOfIGameEntity.FreezeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.FreezeDelegate));
			break;
		case 197:
			ScriptingInterfaceOfIGameEntity.call_GetBodyFlagsDelegate = (ScriptingInterfaceOfIGameEntity.GetBodyFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetBodyFlagsDelegate));
			break;
		case 198:
			ScriptingInterfaceOfIGameEntity.call_GetBodyShapeDelegate = (ScriptingInterfaceOfIGameEntity.GetBodyShapeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetBodyShapeDelegate));
			break;
		case 199:
			ScriptingInterfaceOfIGameEntity.call_GetBoneCountDelegate = (ScriptingInterfaceOfIGameEntity.GetBoneCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetBoneCountDelegate));
			break;
		case 200:
			ScriptingInterfaceOfIGameEntity.call_GetBoneEntitialFrameWithIndexDelegate = (ScriptingInterfaceOfIGameEntity.GetBoneEntitialFrameWithIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetBoneEntitialFrameWithIndexDelegate));
			break;
		case 201:
			ScriptingInterfaceOfIGameEntity.call_GetBoneEntitialFrameWithNameDelegate = (ScriptingInterfaceOfIGameEntity.GetBoneEntitialFrameWithNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetBoneEntitialFrameWithNameDelegate));
			break;
		case 202:
			ScriptingInterfaceOfIGameEntity.call_GetBoundingBoxMaxDelegate = (ScriptingInterfaceOfIGameEntity.GetBoundingBoxMaxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetBoundingBoxMaxDelegate));
			break;
		case 203:
			ScriptingInterfaceOfIGameEntity.call_GetBoundingBoxMinDelegate = (ScriptingInterfaceOfIGameEntity.GetBoundingBoxMinDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetBoundingBoxMinDelegate));
			break;
		case 204:
			ScriptingInterfaceOfIGameEntity.call_GetCameraParamsFromCameraScriptDelegate = (ScriptingInterfaceOfIGameEntity.GetCameraParamsFromCameraScriptDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetCameraParamsFromCameraScriptDelegate));
			break;
		case 205:
			ScriptingInterfaceOfIGameEntity.call_GetCenterOfMassDelegate = (ScriptingInterfaceOfIGameEntity.GetCenterOfMassDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetCenterOfMassDelegate));
			break;
		case 206:
			ScriptingInterfaceOfIGameEntity.call_GetChildDelegate = (ScriptingInterfaceOfIGameEntity.GetChildDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetChildDelegate));
			break;
		case 207:
			ScriptingInterfaceOfIGameEntity.call_GetChildCountDelegate = (ScriptingInterfaceOfIGameEntity.GetChildCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetChildCountDelegate));
			break;
		case 208:
			ScriptingInterfaceOfIGameEntity.call_GetComponentAtIndexDelegate = (ScriptingInterfaceOfIGameEntity.GetComponentAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetComponentAtIndexDelegate));
			break;
		case 209:
			ScriptingInterfaceOfIGameEntity.call_GetComponentCountDelegate = (ScriptingInterfaceOfIGameEntity.GetComponentCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetComponentCountDelegate));
			break;
		case 210:
			ScriptingInterfaceOfIGameEntity.call_GetEditModeLevelVisibilityDelegate = (ScriptingInterfaceOfIGameEntity.GetEditModeLevelVisibilityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetEditModeLevelVisibilityDelegate));
			break;
		case 211:
			ScriptingInterfaceOfIGameEntity.call_GetEntityFlagsDelegate = (ScriptingInterfaceOfIGameEntity.GetEntityFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetEntityFlagsDelegate));
			break;
		case 212:
			ScriptingInterfaceOfIGameEntity.call_GetEntityVisibilityFlagsDelegate = (ScriptingInterfaceOfIGameEntity.GetEntityVisibilityFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetEntityVisibilityFlagsDelegate));
			break;
		case 213:
			ScriptingInterfaceOfIGameEntity.call_GetFactorColorDelegate = (ScriptingInterfaceOfIGameEntity.GetFactorColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetFactorColorDelegate));
			break;
		case 214:
			ScriptingInterfaceOfIGameEntity.call_GetFirstEntityWithTagDelegate = (ScriptingInterfaceOfIGameEntity.GetFirstEntityWithTagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetFirstEntityWithTagDelegate));
			break;
		case 215:
			ScriptingInterfaceOfIGameEntity.call_GetFirstEntityWithTagExpressionDelegate = (ScriptingInterfaceOfIGameEntity.GetFirstEntityWithTagExpressionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetFirstEntityWithTagExpressionDelegate));
			break;
		case 216:
			ScriptingInterfaceOfIGameEntity.call_GetFirstMeshDelegate = (ScriptingInterfaceOfIGameEntity.GetFirstMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetFirstMeshDelegate));
			break;
		case 217:
			ScriptingInterfaceOfIGameEntity.call_GetFrameDelegate = (ScriptingInterfaceOfIGameEntity.GetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetFrameDelegate));
			break;
		case 218:
			ScriptingInterfaceOfIGameEntity.call_GetGlobalBoxMaxDelegate = (ScriptingInterfaceOfIGameEntity.GetGlobalBoxMaxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetGlobalBoxMaxDelegate));
			break;
		case 219:
			ScriptingInterfaceOfIGameEntity.call_GetGlobalBoxMinDelegate = (ScriptingInterfaceOfIGameEntity.GetGlobalBoxMinDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetGlobalBoxMinDelegate));
			break;
		case 220:
			ScriptingInterfaceOfIGameEntity.call_GetGlobalFrameDelegate = (ScriptingInterfaceOfIGameEntity.GetGlobalFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetGlobalFrameDelegate));
			break;
		case 221:
			ScriptingInterfaceOfIGameEntity.call_GetGlobalScaleDelegate = (ScriptingInterfaceOfIGameEntity.GetGlobalScaleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetGlobalScaleDelegate));
			break;
		case 222:
			ScriptingInterfaceOfIGameEntity.call_GetGuidDelegate = (ScriptingInterfaceOfIGameEntity.GetGuidDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetGuidDelegate));
			break;
		case 223:
			ScriptingInterfaceOfIGameEntity.call_GetLightDelegate = (ScriptingInterfaceOfIGameEntity.GetLightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetLightDelegate));
			break;
		case 224:
			ScriptingInterfaceOfIGameEntity.call_GetLinearVelocityDelegate = (ScriptingInterfaceOfIGameEntity.GetLinearVelocityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetLinearVelocityDelegate));
			break;
		case 225:
			ScriptingInterfaceOfIGameEntity.call_GetLodLevelForDistanceSqDelegate = (ScriptingInterfaceOfIGameEntity.GetLodLevelForDistanceSqDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetLodLevelForDistanceSqDelegate));
			break;
		case 226:
			ScriptingInterfaceOfIGameEntity.call_GetMassDelegate = (ScriptingInterfaceOfIGameEntity.GetMassDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetMassDelegate));
			break;
		case 227:
			ScriptingInterfaceOfIGameEntity.call_GetMeshBendedPositionDelegate = (ScriptingInterfaceOfIGameEntity.GetMeshBendedPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetMeshBendedPositionDelegate));
			break;
		case 228:
			ScriptingInterfaceOfIGameEntity.call_GetNameDelegate = (ScriptingInterfaceOfIGameEntity.GetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetNameDelegate));
			break;
		case 229:
			ScriptingInterfaceOfIGameEntity.call_GetNextEntityWithTagDelegate = (ScriptingInterfaceOfIGameEntity.GetNextEntityWithTagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetNextEntityWithTagDelegate));
			break;
		case 230:
			ScriptingInterfaceOfIGameEntity.call_GetNextEntityWithTagExpressionDelegate = (ScriptingInterfaceOfIGameEntity.GetNextEntityWithTagExpressionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetNextEntityWithTagExpressionDelegate));
			break;
		case 231:
			ScriptingInterfaceOfIGameEntity.call_GetNextPrefabDelegate = (ScriptingInterfaceOfIGameEntity.GetNextPrefabDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetNextPrefabDelegate));
			break;
		case 232:
			ScriptingInterfaceOfIGameEntity.call_GetOldPrefabNameDelegate = (ScriptingInterfaceOfIGameEntity.GetOldPrefabNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetOldPrefabNameDelegate));
			break;
		case 233:
			ScriptingInterfaceOfIGameEntity.call_GetParentDelegate = (ScriptingInterfaceOfIGameEntity.GetParentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetParentDelegate));
			break;
		case 234:
			ScriptingInterfaceOfIGameEntity.call_GetPhysicsBoundingBoxMaxDelegate = (ScriptingInterfaceOfIGameEntity.GetPhysicsBoundingBoxMaxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetPhysicsBoundingBoxMaxDelegate));
			break;
		case 235:
			ScriptingInterfaceOfIGameEntity.call_GetPhysicsBoundingBoxMinDelegate = (ScriptingInterfaceOfIGameEntity.GetPhysicsBoundingBoxMinDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetPhysicsBoundingBoxMinDelegate));
			break;
		case 236:
			ScriptingInterfaceOfIGameEntity.call_GetPhysicsDescBodyFlagsDelegate = (ScriptingInterfaceOfIGameEntity.GetPhysicsDescBodyFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetPhysicsDescBodyFlagsDelegate));
			break;
		case 237:
			ScriptingInterfaceOfIGameEntity.call_GetPhysicsMinMaxDelegate = (ScriptingInterfaceOfIGameEntity.GetPhysicsMinMaxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetPhysicsMinMaxDelegate));
			break;
		case 238:
			ScriptingInterfaceOfIGameEntity.call_GetPhysicsStateDelegate = (ScriptingInterfaceOfIGameEntity.GetPhysicsStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetPhysicsStateDelegate));
			break;
		case 239:
			ScriptingInterfaceOfIGameEntity.call_GetPrefabNameDelegate = (ScriptingInterfaceOfIGameEntity.GetPrefabNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetPrefabNameDelegate));
			break;
		case 240:
			ScriptingInterfaceOfIGameEntity.call_GetPreviousGlobalFrameDelegate = (ScriptingInterfaceOfIGameEntity.GetPreviousGlobalFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetPreviousGlobalFrameDelegate));
			break;
		case 241:
			ScriptingInterfaceOfIGameEntity.call_GetQuickBoneEntitialFrameDelegate = (ScriptingInterfaceOfIGameEntity.GetQuickBoneEntitialFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetQuickBoneEntitialFrameDelegate));
			break;
		case 242:
			ScriptingInterfaceOfIGameEntity.call_GetRadiusDelegate = (ScriptingInterfaceOfIGameEntity.GetRadiusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetRadiusDelegate));
			break;
		case 243:
			ScriptingInterfaceOfIGameEntity.call_GetSceneDelegate = (ScriptingInterfaceOfIGameEntity.GetSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetSceneDelegate));
			break;
		case 244:
			ScriptingInterfaceOfIGameEntity.call_GetScenePointerDelegate = (ScriptingInterfaceOfIGameEntity.GetScenePointerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetScenePointerDelegate));
			break;
		case 245:
			ScriptingInterfaceOfIGameEntity.call_GetScriptComponentDelegate = (ScriptingInterfaceOfIGameEntity.GetScriptComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetScriptComponentDelegate));
			break;
		case 246:
			ScriptingInterfaceOfIGameEntity.call_GetScriptComponentAtIndexDelegate = (ScriptingInterfaceOfIGameEntity.GetScriptComponentAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetScriptComponentAtIndexDelegate));
			break;
		case 247:
			ScriptingInterfaceOfIGameEntity.call_GetScriptComponentCountDelegate = (ScriptingInterfaceOfIGameEntity.GetScriptComponentCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetScriptComponentCountDelegate));
			break;
		case 248:
			ScriptingInterfaceOfIGameEntity.call_GetSkeletonDelegate = (ScriptingInterfaceOfIGameEntity.GetSkeletonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetSkeletonDelegate));
			break;
		case 249:
			ScriptingInterfaceOfIGameEntity.call_GetTagsDelegate = (ScriptingInterfaceOfIGameEntity.GetTagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetTagsDelegate));
			break;
		case 250:
			ScriptingInterfaceOfIGameEntity.call_GetUpgradeLevelMaskDelegate = (ScriptingInterfaceOfIGameEntity.GetUpgradeLevelMaskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetUpgradeLevelMaskDelegate));
			break;
		case 251:
			ScriptingInterfaceOfIGameEntity.call_GetUpgradeLevelMaskCumulativeDelegate = (ScriptingInterfaceOfIGameEntity.GetUpgradeLevelMaskCumulativeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetUpgradeLevelMaskCumulativeDelegate));
			break;
		case 252:
			ScriptingInterfaceOfIGameEntity.call_GetVisibilityExcludeParentsDelegate = (ScriptingInterfaceOfIGameEntity.GetVisibilityExcludeParentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetVisibilityExcludeParentsDelegate));
			break;
		case 253:
			ScriptingInterfaceOfIGameEntity.call_GetVisibilityLevelMaskIncludingParentsDelegate = (ScriptingInterfaceOfIGameEntity.GetVisibilityLevelMaskIncludingParentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.GetVisibilityLevelMaskIncludingParentsDelegate));
			break;
		case 254:
			ScriptingInterfaceOfIGameEntity.call_HasBodyDelegate = (ScriptingInterfaceOfIGameEntity.HasBodyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.HasBodyDelegate));
			break;
		case 255:
			ScriptingInterfaceOfIGameEntity.call_HasComplexAnimTreeDelegate = (ScriptingInterfaceOfIGameEntity.HasComplexAnimTreeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.HasComplexAnimTreeDelegate));
			break;
		case 256:
			ScriptingInterfaceOfIGameEntity.call_HasComponentDelegate = (ScriptingInterfaceOfIGameEntity.HasComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.HasComponentDelegate));
			break;
		case 257:
			ScriptingInterfaceOfIGameEntity.call_HasFrameChangedDelegate = (ScriptingInterfaceOfIGameEntity.HasFrameChangedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.HasFrameChangedDelegate));
			break;
		case 258:
			ScriptingInterfaceOfIGameEntity.call_HasPhysicsBodyDelegate = (ScriptingInterfaceOfIGameEntity.HasPhysicsBodyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.HasPhysicsBodyDelegate));
			break;
		case 259:
			ScriptingInterfaceOfIGameEntity.call_HasPhysicsDefinitionDelegate = (ScriptingInterfaceOfIGameEntity.HasPhysicsDefinitionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.HasPhysicsDefinitionDelegate));
			break;
		case 260:
			ScriptingInterfaceOfIGameEntity.call_HasSceneDelegate = (ScriptingInterfaceOfIGameEntity.HasSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.HasSceneDelegate));
			break;
		case 261:
			ScriptingInterfaceOfIGameEntity.call_HasScriptComponentDelegate = (ScriptingInterfaceOfIGameEntity.HasScriptComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.HasScriptComponentDelegate));
			break;
		case 262:
			ScriptingInterfaceOfIGameEntity.call_HasTagDelegate = (ScriptingInterfaceOfIGameEntity.HasTagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.HasTagDelegate));
			break;
		case 263:
			ScriptingInterfaceOfIGameEntity.call_IsDynamicBodyStationaryDelegate = (ScriptingInterfaceOfIGameEntity.IsDynamicBodyStationaryDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.IsDynamicBodyStationaryDelegate));
			break;
		case 264:
			ScriptingInterfaceOfIGameEntity.call_IsEngineBodySleepingDelegate = (ScriptingInterfaceOfIGameEntity.IsEngineBodySleepingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.IsEngineBodySleepingDelegate));
			break;
		case 265:
			ScriptingInterfaceOfIGameEntity.call_IsEntitySelectedOnEditorDelegate = (ScriptingInterfaceOfIGameEntity.IsEntitySelectedOnEditorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.IsEntitySelectedOnEditorDelegate));
			break;
		case 266:
			ScriptingInterfaceOfIGameEntity.call_IsFrozenDelegate = (ScriptingInterfaceOfIGameEntity.IsFrozenDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.IsFrozenDelegate));
			break;
		case 267:
			ScriptingInterfaceOfIGameEntity.call_IsGhostObjectDelegate = (ScriptingInterfaceOfIGameEntity.IsGhostObjectDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.IsGhostObjectDelegate));
			break;
		case 268:
			ScriptingInterfaceOfIGameEntity.call_IsGuidValidDelegate = (ScriptingInterfaceOfIGameEntity.IsGuidValidDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.IsGuidValidDelegate));
			break;
		case 269:
			ScriptingInterfaceOfIGameEntity.call_IsVisibleIncludeParentsDelegate = (ScriptingInterfaceOfIGameEntity.IsVisibleIncludeParentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.IsVisibleIncludeParentsDelegate));
			break;
		case 270:
			ScriptingInterfaceOfIGameEntity.call_PauseParticleSystemDelegate = (ScriptingInterfaceOfIGameEntity.PauseParticleSystemDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.PauseParticleSystemDelegate));
			break;
		case 271:
			ScriptingInterfaceOfIGameEntity.call_PrefabExistsDelegate = (ScriptingInterfaceOfIGameEntity.PrefabExistsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.PrefabExistsDelegate));
			break;
		case 272:
			ScriptingInterfaceOfIGameEntity.call_RecomputeBoundingBoxDelegate = (ScriptingInterfaceOfIGameEntity.RecomputeBoundingBoxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RecomputeBoundingBoxDelegate));
			break;
		case 273:
			ScriptingInterfaceOfIGameEntity.call_ReleaseEditDataUserToAllMeshesDelegate = (ScriptingInterfaceOfIGameEntity.ReleaseEditDataUserToAllMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.ReleaseEditDataUserToAllMeshesDelegate));
			break;
		case 274:
			ScriptingInterfaceOfIGameEntity.call_RemoveDelegate = (ScriptingInterfaceOfIGameEntity.RemoveDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemoveDelegate));
			break;
		case 275:
			ScriptingInterfaceOfIGameEntity.call_RemoveAllChildrenDelegate = (ScriptingInterfaceOfIGameEntity.RemoveAllChildrenDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemoveAllChildrenDelegate));
			break;
		case 276:
			ScriptingInterfaceOfIGameEntity.call_RemoveAllParticleSystemsDelegate = (ScriptingInterfaceOfIGameEntity.RemoveAllParticleSystemsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemoveAllParticleSystemsDelegate));
			break;
		case 277:
			ScriptingInterfaceOfIGameEntity.call_RemoveChildDelegate = (ScriptingInterfaceOfIGameEntity.RemoveChildDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemoveChildDelegate));
			break;
		case 278:
			ScriptingInterfaceOfIGameEntity.call_RemoveComponentDelegate = (ScriptingInterfaceOfIGameEntity.RemoveComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemoveComponentDelegate));
			break;
		case 279:
			ScriptingInterfaceOfIGameEntity.call_RemoveComponentWithMeshDelegate = (ScriptingInterfaceOfIGameEntity.RemoveComponentWithMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemoveComponentWithMeshDelegate));
			break;
		case 280:
			ScriptingInterfaceOfIGameEntity.call_RemoveEnginePhysicsDelegate = (ScriptingInterfaceOfIGameEntity.RemoveEnginePhysicsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemoveEnginePhysicsDelegate));
			break;
		case 281:
			ScriptingInterfaceOfIGameEntity.call_RemoveFromPredisplayEntityDelegate = (ScriptingInterfaceOfIGameEntity.RemoveFromPredisplayEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemoveFromPredisplayEntityDelegate));
			break;
		case 282:
			ScriptingInterfaceOfIGameEntity.call_RemoveMultiMeshDelegate = (ScriptingInterfaceOfIGameEntity.RemoveMultiMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemoveMultiMeshDelegate));
			break;
		case 283:
			ScriptingInterfaceOfIGameEntity.call_RemoveMultiMeshFromSkeletonDelegate = (ScriptingInterfaceOfIGameEntity.RemoveMultiMeshFromSkeletonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemoveMultiMeshFromSkeletonDelegate));
			break;
		case 284:
			ScriptingInterfaceOfIGameEntity.call_RemoveMultiMeshFromSkeletonBoneDelegate = (ScriptingInterfaceOfIGameEntity.RemoveMultiMeshFromSkeletonBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemoveMultiMeshFromSkeletonBoneDelegate));
			break;
		case 285:
			ScriptingInterfaceOfIGameEntity.call_RemovePhysicsDelegate = (ScriptingInterfaceOfIGameEntity.RemovePhysicsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemovePhysicsDelegate));
			break;
		case 286:
			ScriptingInterfaceOfIGameEntity.call_RemoveScriptComponentDelegate = (ScriptingInterfaceOfIGameEntity.RemoveScriptComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemoveScriptComponentDelegate));
			break;
		case 287:
			ScriptingInterfaceOfIGameEntity.call_RemoveTagDelegate = (ScriptingInterfaceOfIGameEntity.RemoveTagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.RemoveTagDelegate));
			break;
		case 288:
			ScriptingInterfaceOfIGameEntity.call_ResumeParticleSystemDelegate = (ScriptingInterfaceOfIGameEntity.ResumeParticleSystemDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.ResumeParticleSystemDelegate));
			break;
		case 289:
			ScriptingInterfaceOfIGameEntity.call_SelectEntityOnEditorDelegate = (ScriptingInterfaceOfIGameEntity.SelectEntityOnEditorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SelectEntityOnEditorDelegate));
			break;
		case 290:
			ScriptingInterfaceOfIGameEntity.call_SetAlphaDelegate = (ScriptingInterfaceOfIGameEntity.SetAlphaDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetAlphaDelegate));
			break;
		case 291:
			ScriptingInterfaceOfIGameEntity.call_SetAnimationSoundActivationDelegate = (ScriptingInterfaceOfIGameEntity.SetAnimationSoundActivationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetAnimationSoundActivationDelegate));
			break;
		case 292:
			ScriptingInterfaceOfIGameEntity.call_SetAnimTreeChannelParameterDelegate = (ScriptingInterfaceOfIGameEntity.SetAnimTreeChannelParameterDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetAnimTreeChannelParameterDelegate));
			break;
		case 293:
			ScriptingInterfaceOfIGameEntity.call_SetAsContourEntityDelegate = (ScriptingInterfaceOfIGameEntity.SetAsContourEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetAsContourEntityDelegate));
			break;
		case 294:
			ScriptingInterfaceOfIGameEntity.call_SetAsPredisplayEntityDelegate = (ScriptingInterfaceOfIGameEntity.SetAsPredisplayEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetAsPredisplayEntityDelegate));
			break;
		case 295:
			ScriptingInterfaceOfIGameEntity.call_SetAsReplayEntityDelegate = (ScriptingInterfaceOfIGameEntity.SetAsReplayEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetAsReplayEntityDelegate));
			break;
		case 296:
			ScriptingInterfaceOfIGameEntity.call_SetBodyFlagsDelegate = (ScriptingInterfaceOfIGameEntity.SetBodyFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetBodyFlagsDelegate));
			break;
		case 297:
			ScriptingInterfaceOfIGameEntity.call_SetBodyFlagsRecursiveDelegate = (ScriptingInterfaceOfIGameEntity.SetBodyFlagsRecursiveDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetBodyFlagsRecursiveDelegate));
			break;
		case 298:
			ScriptingInterfaceOfIGameEntity.call_SetBodyShapeDelegate = (ScriptingInterfaceOfIGameEntity.SetBodyShapeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetBodyShapeDelegate));
			break;
		case 299:
			ScriptingInterfaceOfIGameEntity.call_SetBoundingboxDirtyDelegate = (ScriptingInterfaceOfIGameEntity.SetBoundingboxDirtyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetBoundingboxDirtyDelegate));
			break;
		case 300:
			ScriptingInterfaceOfIGameEntity.call_SetClothComponentKeepStateDelegate = (ScriptingInterfaceOfIGameEntity.SetClothComponentKeepStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetClothComponentKeepStateDelegate));
			break;
		case 301:
			ScriptingInterfaceOfIGameEntity.call_SetClothComponentKeepStateOfAllMeshesDelegate = (ScriptingInterfaceOfIGameEntity.SetClothComponentKeepStateOfAllMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetClothComponentKeepStateOfAllMeshesDelegate));
			break;
		case 302:
			ScriptingInterfaceOfIGameEntity.call_SetClothMaxDistanceMultiplierDelegate = (ScriptingInterfaceOfIGameEntity.SetClothMaxDistanceMultiplierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetClothMaxDistanceMultiplierDelegate));
			break;
		case 303:
			ScriptingInterfaceOfIGameEntity.call_SetContourStateDelegate = (ScriptingInterfaceOfIGameEntity.SetContourStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetContourStateDelegate));
			break;
		case 304:
			ScriptingInterfaceOfIGameEntity.call_SetCullModeDelegate = (ScriptingInterfaceOfIGameEntity.SetCullModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetCullModeDelegate));
			break;
		case 305:
			ScriptingInterfaceOfIGameEntity.call_SetDampingDelegate = (ScriptingInterfaceOfIGameEntity.SetDampingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetDampingDelegate));
			break;
		case 306:
			ScriptingInterfaceOfIGameEntity.call_SetEnforcedMaximumLodLevelDelegate = (ScriptingInterfaceOfIGameEntity.SetEnforcedMaximumLodLevelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetEnforcedMaximumLodLevelDelegate));
			break;
		case 307:
			ScriptingInterfaceOfIGameEntity.call_SetEntityEnvMapVisibilityDelegate = (ScriptingInterfaceOfIGameEntity.SetEntityEnvMapVisibilityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetEntityEnvMapVisibilityDelegate));
			break;
		case 308:
			ScriptingInterfaceOfIGameEntity.call_SetEntityFlagsDelegate = (ScriptingInterfaceOfIGameEntity.SetEntityFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetEntityFlagsDelegate));
			break;
		case 309:
			ScriptingInterfaceOfIGameEntity.call_SetEntityVisibilityFlagsDelegate = (ScriptingInterfaceOfIGameEntity.SetEntityVisibilityFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetEntityVisibilityFlagsDelegate));
			break;
		case 310:
			ScriptingInterfaceOfIGameEntity.call_SetExternalReferencesUsageDelegate = (ScriptingInterfaceOfIGameEntity.SetExternalReferencesUsageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetExternalReferencesUsageDelegate));
			break;
		case 311:
			ScriptingInterfaceOfIGameEntity.call_SetFactor2ColorDelegate = (ScriptingInterfaceOfIGameEntity.SetFactor2ColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetFactor2ColorDelegate));
			break;
		case 312:
			ScriptingInterfaceOfIGameEntity.call_SetFactorColorDelegate = (ScriptingInterfaceOfIGameEntity.SetFactorColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetFactorColorDelegate));
			break;
		case 313:
			ScriptingInterfaceOfIGameEntity.call_SetFrameDelegate = (ScriptingInterfaceOfIGameEntity.SetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetFrameDelegate));
			break;
		case 314:
			ScriptingInterfaceOfIGameEntity.call_SetFrameChangedDelegate = (ScriptingInterfaceOfIGameEntity.SetFrameChangedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetFrameChangedDelegate));
			break;
		case 315:
			ScriptingInterfaceOfIGameEntity.call_SetGlobalFrameDelegate = (ScriptingInterfaceOfIGameEntity.SetGlobalFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetGlobalFrameDelegate));
			break;
		case 316:
			ScriptingInterfaceOfIGameEntity.call_SetLocalPositionDelegate = (ScriptingInterfaceOfIGameEntity.SetLocalPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetLocalPositionDelegate));
			break;
		case 317:
			ScriptingInterfaceOfIGameEntity.call_SetMassDelegate = (ScriptingInterfaceOfIGameEntity.SetMassDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetMassDelegate));
			break;
		case 318:
			ScriptingInterfaceOfIGameEntity.call_SetMassSpaceInertiaDelegate = (ScriptingInterfaceOfIGameEntity.SetMassSpaceInertiaDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetMassSpaceInertiaDelegate));
			break;
		case 319:
			ScriptingInterfaceOfIGameEntity.call_SetMaterialForAllMeshesDelegate = (ScriptingInterfaceOfIGameEntity.SetMaterialForAllMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetMaterialForAllMeshesDelegate));
			break;
		case 320:
			ScriptingInterfaceOfIGameEntity.call_SetMobilityDelegate = (ScriptingInterfaceOfIGameEntity.SetMobilityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetMobilityDelegate));
			break;
		case 321:
			ScriptingInterfaceOfIGameEntity.call_SetMorphFrameOfComponentsDelegate = (ScriptingInterfaceOfIGameEntity.SetMorphFrameOfComponentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetMorphFrameOfComponentsDelegate));
			break;
		case 322:
			ScriptingInterfaceOfIGameEntity.call_SetNameDelegate = (ScriptingInterfaceOfIGameEntity.SetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetNameDelegate));
			break;
		case 323:
			ScriptingInterfaceOfIGameEntity.call_SetPhysicsStateDelegate = (ScriptingInterfaceOfIGameEntity.SetPhysicsStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetPhysicsStateDelegate));
			break;
		case 324:
			ScriptingInterfaceOfIGameEntity.call_SetPreviousFrameInvalidDelegate = (ScriptingInterfaceOfIGameEntity.SetPreviousFrameInvalidDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetPreviousFrameInvalidDelegate));
			break;
		case 325:
			ScriptingInterfaceOfIGameEntity.call_SetReadyToRenderDelegate = (ScriptingInterfaceOfIGameEntity.SetReadyToRenderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetReadyToRenderDelegate));
			break;
		case 326:
			ScriptingInterfaceOfIGameEntity.call_SetRuntimeEmissionRateMultiplierDelegate = (ScriptingInterfaceOfIGameEntity.SetRuntimeEmissionRateMultiplierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetRuntimeEmissionRateMultiplierDelegate));
			break;
		case 327:
			ScriptingInterfaceOfIGameEntity.call_SetSkeletonDelegate = (ScriptingInterfaceOfIGameEntity.SetSkeletonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetSkeletonDelegate));
			break;
		case 328:
			ScriptingInterfaceOfIGameEntity.call_SetUpgradeLevelMaskDelegate = (ScriptingInterfaceOfIGameEntity.SetUpgradeLevelMaskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetUpgradeLevelMaskDelegate));
			break;
		case 329:
			ScriptingInterfaceOfIGameEntity.call_SetVectorArgumentDelegate = (ScriptingInterfaceOfIGameEntity.SetVectorArgumentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetVectorArgumentDelegate));
			break;
		case 330:
			ScriptingInterfaceOfIGameEntity.call_SetVisibilityExcludeParentsDelegate = (ScriptingInterfaceOfIGameEntity.SetVisibilityExcludeParentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.SetVisibilityExcludeParentsDelegate));
			break;
		case 331:
			ScriptingInterfaceOfIGameEntity.call_UpdateGlobalBoundsDelegate = (ScriptingInterfaceOfIGameEntity.UpdateGlobalBoundsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.UpdateGlobalBoundsDelegate));
			break;
		case 332:
			ScriptingInterfaceOfIGameEntity.call_UpdateTriadFrameForEditorDelegate = (ScriptingInterfaceOfIGameEntity.UpdateTriadFrameForEditorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.UpdateTriadFrameForEditorDelegate));
			break;
		case 333:
			ScriptingInterfaceOfIGameEntity.call_UpdateVisibilityMaskDelegate = (ScriptingInterfaceOfIGameEntity.UpdateVisibilityMaskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.UpdateVisibilityMaskDelegate));
			break;
		case 334:
			ScriptingInterfaceOfIGameEntity.call_ValidateBoundingBoxDelegate = (ScriptingInterfaceOfIGameEntity.ValidateBoundingBoxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntity.ValidateBoundingBoxDelegate));
			break;
		case 335:
			ScriptingInterfaceOfIGameEntityComponent.call_GetEntityDelegate = (ScriptingInterfaceOfIGameEntityComponent.GetEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntityComponent.GetEntityDelegate));
			break;
		case 336:
			ScriptingInterfaceOfIGameEntityComponent.call_GetFirstMetaMeshDelegate = (ScriptingInterfaceOfIGameEntityComponent.GetFirstMetaMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIGameEntityComponent.GetFirstMetaMeshDelegate));
			break;
		case 337:
			ScriptingInterfaceOfIHighlights.call_AddHighlightDelegate = (ScriptingInterfaceOfIHighlights.AddHighlightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIHighlights.AddHighlightDelegate));
			break;
		case 338:
			ScriptingInterfaceOfIHighlights.call_CloseGroupDelegate = (ScriptingInterfaceOfIHighlights.CloseGroupDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIHighlights.CloseGroupDelegate));
			break;
		case 339:
			ScriptingInterfaceOfIHighlights.call_InitializeDelegate = (ScriptingInterfaceOfIHighlights.InitializeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIHighlights.InitializeDelegate));
			break;
		case 340:
			ScriptingInterfaceOfIHighlights.call_OpenGroupDelegate = (ScriptingInterfaceOfIHighlights.OpenGroupDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIHighlights.OpenGroupDelegate));
			break;
		case 341:
			ScriptingInterfaceOfIHighlights.call_OpenSummaryDelegate = (ScriptingInterfaceOfIHighlights.OpenSummaryDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIHighlights.OpenSummaryDelegate));
			break;
		case 342:
			ScriptingInterfaceOfIHighlights.call_RemoveHighlightDelegate = (ScriptingInterfaceOfIHighlights.RemoveHighlightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIHighlights.RemoveHighlightDelegate));
			break;
		case 343:
			ScriptingInterfaceOfIHighlights.call_SaveScreenshotDelegate = (ScriptingInterfaceOfIHighlights.SaveScreenshotDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIHighlights.SaveScreenshotDelegate));
			break;
		case 344:
			ScriptingInterfaceOfIHighlights.call_SaveVideoDelegate = (ScriptingInterfaceOfIHighlights.SaveVideoDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIHighlights.SaveVideoDelegate));
			break;
		case 345:
			ScriptingInterfaceOfIImgui.call_BeginDelegate = (ScriptingInterfaceOfIImgui.BeginDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.BeginDelegate));
			break;
		case 346:
			ScriptingInterfaceOfIImgui.call_BeginMainThreadScopeDelegate = (ScriptingInterfaceOfIImgui.BeginMainThreadScopeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.BeginMainThreadScopeDelegate));
			break;
		case 347:
			ScriptingInterfaceOfIImgui.call_BeginWithCloseButtonDelegate = (ScriptingInterfaceOfIImgui.BeginWithCloseButtonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.BeginWithCloseButtonDelegate));
			break;
		case 348:
			ScriptingInterfaceOfIImgui.call_ButtonDelegate = (ScriptingInterfaceOfIImgui.ButtonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.ButtonDelegate));
			break;
		case 349:
			ScriptingInterfaceOfIImgui.call_CheckboxDelegate = (ScriptingInterfaceOfIImgui.CheckboxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.CheckboxDelegate));
			break;
		case 350:
			ScriptingInterfaceOfIImgui.call_CollapsingHeaderDelegate = (ScriptingInterfaceOfIImgui.CollapsingHeaderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.CollapsingHeaderDelegate));
			break;
		case 351:
			ScriptingInterfaceOfIImgui.call_ColumnsDelegate = (ScriptingInterfaceOfIImgui.ColumnsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.ColumnsDelegate));
			break;
		case 352:
			ScriptingInterfaceOfIImgui.call_ComboDelegate = (ScriptingInterfaceOfIImgui.ComboDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.ComboDelegate));
			break;
		case 353:
			ScriptingInterfaceOfIImgui.call_EndDelegate = (ScriptingInterfaceOfIImgui.EndDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.EndDelegate));
			break;
		case 354:
			ScriptingInterfaceOfIImgui.call_EndMainThreadScopeDelegate = (ScriptingInterfaceOfIImgui.EndMainThreadScopeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.EndMainThreadScopeDelegate));
			break;
		case 355:
			ScriptingInterfaceOfIImgui.call_InputFloatDelegate = (ScriptingInterfaceOfIImgui.InputFloatDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.InputFloatDelegate));
			break;
		case 356:
			ScriptingInterfaceOfIImgui.call_InputFloat2Delegate = (ScriptingInterfaceOfIImgui.InputFloat2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.InputFloat2Delegate));
			break;
		case 357:
			ScriptingInterfaceOfIImgui.call_InputFloat3Delegate = (ScriptingInterfaceOfIImgui.InputFloat3Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.InputFloat3Delegate));
			break;
		case 358:
			ScriptingInterfaceOfIImgui.call_InputFloat4Delegate = (ScriptingInterfaceOfIImgui.InputFloat4Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.InputFloat4Delegate));
			break;
		case 359:
			ScriptingInterfaceOfIImgui.call_InputIntDelegate = (ScriptingInterfaceOfIImgui.InputIntDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.InputIntDelegate));
			break;
		case 360:
			ScriptingInterfaceOfIImgui.call_IsItemHoveredDelegate = (ScriptingInterfaceOfIImgui.IsItemHoveredDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.IsItemHoveredDelegate));
			break;
		case 361:
			ScriptingInterfaceOfIImgui.call_NewFrameDelegate = (ScriptingInterfaceOfIImgui.NewFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.NewFrameDelegate));
			break;
		case 362:
			ScriptingInterfaceOfIImgui.call_NewLineDelegate = (ScriptingInterfaceOfIImgui.NewLineDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.NewLineDelegate));
			break;
		case 363:
			ScriptingInterfaceOfIImgui.call_NextColumnDelegate = (ScriptingInterfaceOfIImgui.NextColumnDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.NextColumnDelegate));
			break;
		case 364:
			ScriptingInterfaceOfIImgui.call_PlotLinesDelegate = (ScriptingInterfaceOfIImgui.PlotLinesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.PlotLinesDelegate));
			break;
		case 365:
			ScriptingInterfaceOfIImgui.call_PopStyleColorDelegate = (ScriptingInterfaceOfIImgui.PopStyleColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.PopStyleColorDelegate));
			break;
		case 366:
			ScriptingInterfaceOfIImgui.call_ProgressBarDelegate = (ScriptingInterfaceOfIImgui.ProgressBarDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.ProgressBarDelegate));
			break;
		case 367:
			ScriptingInterfaceOfIImgui.call_PushStyleColorDelegate = (ScriptingInterfaceOfIImgui.PushStyleColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.PushStyleColorDelegate));
			break;
		case 368:
			ScriptingInterfaceOfIImgui.call_RadioButtonDelegate = (ScriptingInterfaceOfIImgui.RadioButtonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.RadioButtonDelegate));
			break;
		case 369:
			ScriptingInterfaceOfIImgui.call_RenderDelegate = (ScriptingInterfaceOfIImgui.RenderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.RenderDelegate));
			break;
		case 370:
			ScriptingInterfaceOfIImgui.call_SameLineDelegate = (ScriptingInterfaceOfIImgui.SameLineDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.SameLineDelegate));
			break;
		case 371:
			ScriptingInterfaceOfIImgui.call_SeparatorDelegate = (ScriptingInterfaceOfIImgui.SeparatorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.SeparatorDelegate));
			break;
		case 372:
			ScriptingInterfaceOfIImgui.call_SetTooltipDelegate = (ScriptingInterfaceOfIImgui.SetTooltipDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.SetTooltipDelegate));
			break;
		case 373:
			ScriptingInterfaceOfIImgui.call_SliderFloatDelegate = (ScriptingInterfaceOfIImgui.SliderFloatDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.SliderFloatDelegate));
			break;
		case 374:
			ScriptingInterfaceOfIImgui.call_SmallButtonDelegate = (ScriptingInterfaceOfIImgui.SmallButtonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.SmallButtonDelegate));
			break;
		case 375:
			ScriptingInterfaceOfIImgui.call_TextDelegate = (ScriptingInterfaceOfIImgui.TextDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.TextDelegate));
			break;
		case 376:
			ScriptingInterfaceOfIImgui.call_TreeNodeDelegate = (ScriptingInterfaceOfIImgui.TreeNodeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.TreeNodeDelegate));
			break;
		case 377:
			ScriptingInterfaceOfIImgui.call_TreePopDelegate = (ScriptingInterfaceOfIImgui.TreePopDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIImgui.TreePopDelegate));
			break;
		case 378:
			ScriptingInterfaceOfIInput.call_ClearKeysDelegate = (ScriptingInterfaceOfIInput.ClearKeysDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.ClearKeysDelegate));
			break;
		case 379:
			ScriptingInterfaceOfIInput.call_GetClipboardTextDelegate = (ScriptingInterfaceOfIInput.GetClipboardTextDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetClipboardTextDelegate));
			break;
		case 380:
			ScriptingInterfaceOfIInput.call_GetControllerTypeDelegate = (ScriptingInterfaceOfIInput.GetControllerTypeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetControllerTypeDelegate));
			break;
		case 381:
			ScriptingInterfaceOfIInput.call_GetGyroXDelegate = (ScriptingInterfaceOfIInput.GetGyroXDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetGyroXDelegate));
			break;
		case 382:
			ScriptingInterfaceOfIInput.call_GetGyroYDelegate = (ScriptingInterfaceOfIInput.GetGyroYDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetGyroYDelegate));
			break;
		case 383:
			ScriptingInterfaceOfIInput.call_GetGyroZDelegate = (ScriptingInterfaceOfIInput.GetGyroZDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetGyroZDelegate));
			break;
		case 384:
			ScriptingInterfaceOfIInput.call_GetKeyStateDelegate = (ScriptingInterfaceOfIInput.GetKeyStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetKeyStateDelegate));
			break;
		case 385:
			ScriptingInterfaceOfIInput.call_GetMouseDeltaZDelegate = (ScriptingInterfaceOfIInput.GetMouseDeltaZDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetMouseDeltaZDelegate));
			break;
		case 386:
			ScriptingInterfaceOfIInput.call_GetMouseMoveXDelegate = (ScriptingInterfaceOfIInput.GetMouseMoveXDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetMouseMoveXDelegate));
			break;
		case 387:
			ScriptingInterfaceOfIInput.call_GetMouseMoveYDelegate = (ScriptingInterfaceOfIInput.GetMouseMoveYDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetMouseMoveYDelegate));
			break;
		case 388:
			ScriptingInterfaceOfIInput.call_GetMousePositionXDelegate = (ScriptingInterfaceOfIInput.GetMousePositionXDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetMousePositionXDelegate));
			break;
		case 389:
			ScriptingInterfaceOfIInput.call_GetMousePositionYDelegate = (ScriptingInterfaceOfIInput.GetMousePositionYDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetMousePositionYDelegate));
			break;
		case 390:
			ScriptingInterfaceOfIInput.call_GetMouseScrollValueDelegate = (ScriptingInterfaceOfIInput.GetMouseScrollValueDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetMouseScrollValueDelegate));
			break;
		case 391:
			ScriptingInterfaceOfIInput.call_GetMouseSensitivityDelegate = (ScriptingInterfaceOfIInput.GetMouseSensitivityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetMouseSensitivityDelegate));
			break;
		case 392:
			ScriptingInterfaceOfIInput.call_GetVirtualKeyCodeDelegate = (ScriptingInterfaceOfIInput.GetVirtualKeyCodeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.GetVirtualKeyCodeDelegate));
			break;
		case 393:
			ScriptingInterfaceOfIInput.call_IsAnyTouchActiveDelegate = (ScriptingInterfaceOfIInput.IsAnyTouchActiveDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.IsAnyTouchActiveDelegate));
			break;
		case 394:
			ScriptingInterfaceOfIInput.call_IsControllerConnectedDelegate = (ScriptingInterfaceOfIInput.IsControllerConnectedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.IsControllerConnectedDelegate));
			break;
		case 395:
			ScriptingInterfaceOfIInput.call_IsKeyDownDelegate = (ScriptingInterfaceOfIInput.IsKeyDownDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.IsKeyDownDelegate));
			break;
		case 396:
			ScriptingInterfaceOfIInput.call_IsKeyDownImmediateDelegate = (ScriptingInterfaceOfIInput.IsKeyDownImmediateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.IsKeyDownImmediateDelegate));
			break;
		case 397:
			ScriptingInterfaceOfIInput.call_IsKeyPressedDelegate = (ScriptingInterfaceOfIInput.IsKeyPressedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.IsKeyPressedDelegate));
			break;
		case 398:
			ScriptingInterfaceOfIInput.call_IsKeyReleasedDelegate = (ScriptingInterfaceOfIInput.IsKeyReleasedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.IsKeyReleasedDelegate));
			break;
		case 399:
			ScriptingInterfaceOfIInput.call_IsMouseActiveDelegate = (ScriptingInterfaceOfIInput.IsMouseActiveDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.IsMouseActiveDelegate));
			break;
		case 400:
			ScriptingInterfaceOfIInput.call_PressKeyDelegate = (ScriptingInterfaceOfIInput.PressKeyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.PressKeyDelegate));
			break;
		case 401:
			ScriptingInterfaceOfIInput.call_SetClipboardTextDelegate = (ScriptingInterfaceOfIInput.SetClipboardTextDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.SetClipboardTextDelegate));
			break;
		case 402:
			ScriptingInterfaceOfIInput.call_SetCursorFrictionValueDelegate = (ScriptingInterfaceOfIInput.SetCursorFrictionValueDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.SetCursorFrictionValueDelegate));
			break;
		case 403:
			ScriptingInterfaceOfIInput.call_SetCursorPositionDelegate = (ScriptingInterfaceOfIInput.SetCursorPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.SetCursorPositionDelegate));
			break;
		case 404:
			ScriptingInterfaceOfIInput.call_SetLightbarColorDelegate = (ScriptingInterfaceOfIInput.SetLightbarColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.SetLightbarColorDelegate));
			break;
		case 405:
			ScriptingInterfaceOfIInput.call_SetRumbleEffectDelegate = (ScriptingInterfaceOfIInput.SetRumbleEffectDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.SetRumbleEffectDelegate));
			break;
		case 406:
			ScriptingInterfaceOfIInput.call_SetTriggerFeedbackDelegate = (ScriptingInterfaceOfIInput.SetTriggerFeedbackDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.SetTriggerFeedbackDelegate));
			break;
		case 407:
			ScriptingInterfaceOfIInput.call_SetTriggerVibrationDelegate = (ScriptingInterfaceOfIInput.SetTriggerVibrationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.SetTriggerVibrationDelegate));
			break;
		case 408:
			ScriptingInterfaceOfIInput.call_SetTriggerWeaponEffectDelegate = (ScriptingInterfaceOfIInput.SetTriggerWeaponEffectDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.SetTriggerWeaponEffectDelegate));
			break;
		case 409:
			ScriptingInterfaceOfIInput.call_UpdateKeyDataDelegate = (ScriptingInterfaceOfIInput.UpdateKeyDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIInput.UpdateKeyDataDelegate));
			break;
		case 410:
			ScriptingInterfaceOfILight.call_CreatePointLightDelegate = (ScriptingInterfaceOfILight.CreatePointLightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.CreatePointLightDelegate));
			break;
		case 411:
			ScriptingInterfaceOfILight.call_EnableShadowDelegate = (ScriptingInterfaceOfILight.EnableShadowDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.EnableShadowDelegate));
			break;
		case 412:
			ScriptingInterfaceOfILight.call_GetFrameDelegate = (ScriptingInterfaceOfILight.GetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.GetFrameDelegate));
			break;
		case 413:
			ScriptingInterfaceOfILight.call_GetIntensityDelegate = (ScriptingInterfaceOfILight.GetIntensityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.GetIntensityDelegate));
			break;
		case 414:
			ScriptingInterfaceOfILight.call_GetLightColorDelegate = (ScriptingInterfaceOfILight.GetLightColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.GetLightColorDelegate));
			break;
		case 415:
			ScriptingInterfaceOfILight.call_GetRadiusDelegate = (ScriptingInterfaceOfILight.GetRadiusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.GetRadiusDelegate));
			break;
		case 416:
			ScriptingInterfaceOfILight.call_IsShadowEnabledDelegate = (ScriptingInterfaceOfILight.IsShadowEnabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.IsShadowEnabledDelegate));
			break;
		case 417:
			ScriptingInterfaceOfILight.call_ReleaseDelegate = (ScriptingInterfaceOfILight.ReleaseDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.ReleaseDelegate));
			break;
		case 418:
			ScriptingInterfaceOfILight.call_SetFrameDelegate = (ScriptingInterfaceOfILight.SetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.SetFrameDelegate));
			break;
		case 419:
			ScriptingInterfaceOfILight.call_SetIntensityDelegate = (ScriptingInterfaceOfILight.SetIntensityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.SetIntensityDelegate));
			break;
		case 420:
			ScriptingInterfaceOfILight.call_SetLightColorDelegate = (ScriptingInterfaceOfILight.SetLightColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.SetLightColorDelegate));
			break;
		case 421:
			ScriptingInterfaceOfILight.call_SetLightFlickerDelegate = (ScriptingInterfaceOfILight.SetLightFlickerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.SetLightFlickerDelegate));
			break;
		case 422:
			ScriptingInterfaceOfILight.call_SetRadiusDelegate = (ScriptingInterfaceOfILight.SetRadiusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.SetRadiusDelegate));
			break;
		case 423:
			ScriptingInterfaceOfILight.call_SetShadowsDelegate = (ScriptingInterfaceOfILight.SetShadowsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.SetShadowsDelegate));
			break;
		case 424:
			ScriptingInterfaceOfILight.call_SetVisibilityDelegate = (ScriptingInterfaceOfILight.SetVisibilityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.SetVisibilityDelegate));
			break;
		case 425:
			ScriptingInterfaceOfILight.call_SetVolumetricPropertiesDelegate = (ScriptingInterfaceOfILight.SetVolumetricPropertiesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfILight.SetVolumetricPropertiesDelegate));
			break;
		case 426:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddFaceDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddFaceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddFaceDelegate));
			break;
		case 427:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddFaceCorner1Delegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddFaceCorner1Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddFaceCorner1Delegate));
			break;
		case 428:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddFaceCorner2Delegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddFaceCorner2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddFaceCorner2Delegate));
			break;
		case 429:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddLineDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddLineDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddLineDelegate));
			break;
		case 430:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddMeshDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshDelegate));
			break;
		case 431:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddMeshAuxDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshAuxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshAuxDelegate));
			break;
		case 432:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddMeshToBoneDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshToBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshToBoneDelegate));
			break;
		case 433:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddMeshWithColorDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshWithColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshWithColorDelegate));
			break;
		case 434:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddMeshWithFixedNormalsDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshWithFixedNormalsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshWithFixedNormalsDelegate));
			break;
		case 435:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddMeshWithFixedNormalsWithHeightGradientColorDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshWithFixedNormalsWithHeightGradientColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshWithFixedNormalsWithHeightGradientColorDelegate));
			break;
		case 436:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddMeshWithSkinDataDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshWithSkinDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddMeshWithSkinDataDelegate));
			break;
		case 437:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddRectDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddRectDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddRectDelegate));
			break;
		case 438:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddRectangle3Delegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddRectangle3Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddRectangle3Delegate));
			break;
		case 439:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddRectangleWithInverseUVDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddRectangleWithInverseUVDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddRectangleWithInverseUVDelegate));
			break;
		case 440:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddRectWithZUpDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddRectWithZUpDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddRectWithZUpDelegate));
			break;
		case 441:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddSkinnedMeshWithColorDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddSkinnedMeshWithColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddSkinnedMeshWithColorDelegate));
			break;
		case 442:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddTriangle1Delegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddTriangle1Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddTriangle1Delegate));
			break;
		case 443:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddTriangle2Delegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddTriangle2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddTriangle2Delegate));
			break;
		case 444:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_AddVertexDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.AddVertexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.AddVertexDelegate));
			break;
		case 445:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_ApplyCPUSkinningDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.ApplyCPUSkinningDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.ApplyCPUSkinningDelegate));
			break;
		case 446:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_ClearAllDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.ClearAllDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.ClearAllDelegate));
			break;
		case 447:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_ComputeCornerNormalsDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.ComputeCornerNormalsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.ComputeCornerNormalsDelegate));
			break;
		case 448:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_ComputeCornerNormalsWithSmoothingDataDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.ComputeCornerNormalsWithSmoothingDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.ComputeCornerNormalsWithSmoothingDataDelegate));
			break;
		case 449:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_ComputeTangentsDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.ComputeTangentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.ComputeTangentsDelegate));
			break;
		case 450:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_CreateDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.CreateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.CreateDelegate));
			break;
		case 451:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_EnsureTransformedVerticesDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.EnsureTransformedVerticesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.EnsureTransformedVerticesDelegate));
			break;
		case 452:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_FinalizeEditingDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.FinalizeEditingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.FinalizeEditingDelegate));
			break;
		case 453:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_GenerateGridDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.GenerateGridDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.GenerateGridDelegate));
			break;
		case 454:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_GetPositionOfVertexDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.GetPositionOfVertexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.GetPositionOfVertexDelegate));
			break;
		case 455:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_GetVertexColorDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.GetVertexColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.GetVertexColorDelegate));
			break;
		case 456:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_GetVertexColorAlphaDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.GetVertexColorAlphaDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.GetVertexColorAlphaDelegate));
			break;
		case 457:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_InvertFacesWindingOrderDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.InvertFacesWindingOrderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.InvertFacesWindingOrderDelegate));
			break;
		case 458:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_MoveVerticesAlongNormalDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.MoveVerticesAlongNormalDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.MoveVerticesAlongNormalDelegate));
			break;
		case 459:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_RemoveDuplicatedCornersDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.RemoveDuplicatedCornersDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.RemoveDuplicatedCornersDelegate));
			break;
		case 460:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_RemoveFaceDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.RemoveFaceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.RemoveFaceDelegate));
			break;
		case 461:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_RescaleMesh2dDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.RescaleMesh2dDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.RescaleMesh2dDelegate));
			break;
		case 462:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_RescaleMesh2dRepeatXDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.RescaleMesh2dRepeatXDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.RescaleMesh2dRepeatXDelegate));
			break;
		case 463:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_RescaleMesh2dRepeatXWithTilingDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.RescaleMesh2dRepeatXWithTilingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.RescaleMesh2dRepeatXWithTilingDelegate));
			break;
		case 464:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_RescaleMesh2dRepeatYDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.RescaleMesh2dRepeatYDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.RescaleMesh2dRepeatYDelegate));
			break;
		case 465:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_RescaleMesh2dRepeatYWithTilingDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.RescaleMesh2dRepeatYWithTilingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.RescaleMesh2dRepeatYWithTilingDelegate));
			break;
		case 466:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_RescaleMesh2dWithoutChangingUVDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.RescaleMesh2dWithoutChangingUVDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.RescaleMesh2dWithoutChangingUVDelegate));
			break;
		case 467:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_ReserveFaceCornersDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.ReserveFaceCornersDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.ReserveFaceCornersDelegate));
			break;
		case 468:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_ReserveFacesDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.ReserveFacesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.ReserveFacesDelegate));
			break;
		case 469:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_ReserveVerticesDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.ReserveVerticesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.ReserveVerticesDelegate));
			break;
		case 470:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_ScaleVertices1Delegate = (ScriptingInterfaceOfIManagedMeshEditOperations.ScaleVertices1Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.ScaleVertices1Delegate));
			break;
		case 471:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_ScaleVertices2Delegate = (ScriptingInterfaceOfIManagedMeshEditOperations.ScaleVertices2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.ScaleVertices2Delegate));
			break;
		case 472:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_SetCornerUVDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.SetCornerUVDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.SetCornerUVDelegate));
			break;
		case 473:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_SetCornerVertexColorDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.SetCornerVertexColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.SetCornerVertexColorDelegate));
			break;
		case 474:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_SetPositionOfVertexDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.SetPositionOfVertexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.SetPositionOfVertexDelegate));
			break;
		case 475:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_SetTangentsOfFaceCornerDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.SetTangentsOfFaceCornerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.SetTangentsOfFaceCornerDelegate));
			break;
		case 476:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_SetVertexColorDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.SetVertexColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.SetVertexColorDelegate));
			break;
		case 477:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_SetVertexColorAlphaDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.SetVertexColorAlphaDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.SetVertexColorAlphaDelegate));
			break;
		case 478:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_TransformVerticesToLocalDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.TransformVerticesToLocalDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.TransformVerticesToLocalDelegate));
			break;
		case 479:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_TransformVerticesToParentDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.TransformVerticesToParentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.TransformVerticesToParentDelegate));
			break;
		case 480:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_TranslateVerticesDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.TranslateVerticesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.TranslateVerticesDelegate));
			break;
		case 481:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_UpdateOverlappedVertexNormalsDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.UpdateOverlappedVertexNormalsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.UpdateOverlappedVertexNormalsDelegate));
			break;
		case 482:
			ScriptingInterfaceOfIManagedMeshEditOperations.call_WeldDelegate = (ScriptingInterfaceOfIManagedMeshEditOperations.WeldDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIManagedMeshEditOperations.WeldDelegate));
			break;
		case 483:
			ScriptingInterfaceOfIMaterial.call_AddMaterialShaderFlagDelegate = (ScriptingInterfaceOfIMaterial.AddMaterialShaderFlagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.AddMaterialShaderFlagDelegate));
			break;
		case 484:
			ScriptingInterfaceOfIMaterial.call_CreateCopyDelegate = (ScriptingInterfaceOfIMaterial.CreateCopyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.CreateCopyDelegate));
			break;
		case 485:
			ScriptingInterfaceOfIMaterial.call_GetAlphaBlendModeDelegate = (ScriptingInterfaceOfIMaterial.GetAlphaBlendModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.GetAlphaBlendModeDelegate));
			break;
		case 486:
			ScriptingInterfaceOfIMaterial.call_GetAlphaTestValueDelegate = (ScriptingInterfaceOfIMaterial.GetAlphaTestValueDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.GetAlphaTestValueDelegate));
			break;
		case 487:
			ScriptingInterfaceOfIMaterial.call_GetDefaultMaterialDelegate = (ScriptingInterfaceOfIMaterial.GetDefaultMaterialDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.GetDefaultMaterialDelegate));
			break;
		case 488:
			ScriptingInterfaceOfIMaterial.call_GetFlagsDelegate = (ScriptingInterfaceOfIMaterial.GetFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.GetFlagsDelegate));
			break;
		case 489:
			ScriptingInterfaceOfIMaterial.call_GetFromResourceDelegate = (ScriptingInterfaceOfIMaterial.GetFromResourceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.GetFromResourceDelegate));
			break;
		case 490:
			ScriptingInterfaceOfIMaterial.call_GetNameDelegate = (ScriptingInterfaceOfIMaterial.GetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.GetNameDelegate));
			break;
		case 491:
			ScriptingInterfaceOfIMaterial.call_GetOutlineMaterialDelegate = (ScriptingInterfaceOfIMaterial.GetOutlineMaterialDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.GetOutlineMaterialDelegate));
			break;
		case 492:
			ScriptingInterfaceOfIMaterial.call_GetShaderDelegate = (ScriptingInterfaceOfIMaterial.GetShaderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.GetShaderDelegate));
			break;
		case 493:
			ScriptingInterfaceOfIMaterial.call_GetShaderFlagsDelegate = (ScriptingInterfaceOfIMaterial.GetShaderFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.GetShaderFlagsDelegate));
			break;
		case 494:
			ScriptingInterfaceOfIMaterial.call_GetTextureDelegate = (ScriptingInterfaceOfIMaterial.GetTextureDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.GetTextureDelegate));
			break;
		case 495:
			ScriptingInterfaceOfIMaterial.call_ReleaseDelegate = (ScriptingInterfaceOfIMaterial.ReleaseDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.ReleaseDelegate));
			break;
		case 496:
			ScriptingInterfaceOfIMaterial.call_SetAlphaBlendModeDelegate = (ScriptingInterfaceOfIMaterial.SetAlphaBlendModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.SetAlphaBlendModeDelegate));
			break;
		case 497:
			ScriptingInterfaceOfIMaterial.call_SetAlphaTestValueDelegate = (ScriptingInterfaceOfIMaterial.SetAlphaTestValueDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.SetAlphaTestValueDelegate));
			break;
		case 498:
			ScriptingInterfaceOfIMaterial.call_SetAreaMapScaleDelegate = (ScriptingInterfaceOfIMaterial.SetAreaMapScaleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.SetAreaMapScaleDelegate));
			break;
		case 499:
			ScriptingInterfaceOfIMaterial.call_SetEnableSkinningDelegate = (ScriptingInterfaceOfIMaterial.SetEnableSkinningDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.SetEnableSkinningDelegate));
			break;
		case 500:
			ScriptingInterfaceOfIMaterial.call_SetFlagsDelegate = (ScriptingInterfaceOfIMaterial.SetFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.SetFlagsDelegate));
			break;
		case 501:
			ScriptingInterfaceOfIMaterial.call_SetMeshVectorArgumentDelegate = (ScriptingInterfaceOfIMaterial.SetMeshVectorArgumentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.SetMeshVectorArgumentDelegate));
			break;
		case 502:
			ScriptingInterfaceOfIMaterial.call_SetNameDelegate = (ScriptingInterfaceOfIMaterial.SetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.SetNameDelegate));
			break;
		case 503:
			ScriptingInterfaceOfIMaterial.call_SetShaderDelegate = (ScriptingInterfaceOfIMaterial.SetShaderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.SetShaderDelegate));
			break;
		case 504:
			ScriptingInterfaceOfIMaterial.call_SetShaderFlagsDelegate = (ScriptingInterfaceOfIMaterial.SetShaderFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.SetShaderFlagsDelegate));
			break;
		case 505:
			ScriptingInterfaceOfIMaterial.call_SetTextureDelegate = (ScriptingInterfaceOfIMaterial.SetTextureDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.SetTextureDelegate));
			break;
		case 506:
			ScriptingInterfaceOfIMaterial.call_SetTextureAtSlotDelegate = (ScriptingInterfaceOfIMaterial.SetTextureAtSlotDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.SetTextureAtSlotDelegate));
			break;
		case 507:
			ScriptingInterfaceOfIMaterial.call_UsingSkinningDelegate = (ScriptingInterfaceOfIMaterial.UsingSkinningDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMaterial.UsingSkinningDelegate));
			break;
		case 508:
			ScriptingInterfaceOfIMesh.call_AddEditDataUserDelegate = (ScriptingInterfaceOfIMesh.AddEditDataUserDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.AddEditDataUserDelegate));
			break;
		case 509:
			ScriptingInterfaceOfIMesh.call_AddFaceDelegate = (ScriptingInterfaceOfIMesh.AddFaceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.AddFaceDelegate));
			break;
		case 510:
			ScriptingInterfaceOfIMesh.call_AddFaceCornerDelegate = (ScriptingInterfaceOfIMesh.AddFaceCornerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.AddFaceCornerDelegate));
			break;
		case 511:
			ScriptingInterfaceOfIMesh.call_AddMeshToMeshDelegate = (ScriptingInterfaceOfIMesh.AddMeshToMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.AddMeshToMeshDelegate));
			break;
		case 512:
			ScriptingInterfaceOfIMesh.call_AddTriangleDelegate = (ScriptingInterfaceOfIMesh.AddTriangleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.AddTriangleDelegate));
			break;
		case 513:
			ScriptingInterfaceOfIMesh.call_AddTriangleWithVertexColorsDelegate = (ScriptingInterfaceOfIMesh.AddTriangleWithVertexColorsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.AddTriangleWithVertexColorsDelegate));
			break;
		case 514:
			ScriptingInterfaceOfIMesh.call_ClearMeshDelegate = (ScriptingInterfaceOfIMesh.ClearMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.ClearMeshDelegate));
			break;
		case 515:
			ScriptingInterfaceOfIMesh.call_ComputeNormalsDelegate = (ScriptingInterfaceOfIMesh.ComputeNormalsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.ComputeNormalsDelegate));
			break;
		case 516:
			ScriptingInterfaceOfIMesh.call_ComputeTangentsDelegate = (ScriptingInterfaceOfIMesh.ComputeTangentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.ComputeTangentsDelegate));
			break;
		case 517:
			ScriptingInterfaceOfIMesh.call_CreateMeshDelegate = (ScriptingInterfaceOfIMesh.CreateMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.CreateMeshDelegate));
			break;
		case 518:
			ScriptingInterfaceOfIMesh.call_CreateMeshCopyDelegate = (ScriptingInterfaceOfIMesh.CreateMeshCopyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.CreateMeshCopyDelegate));
			break;
		case 519:
			ScriptingInterfaceOfIMesh.call_CreateMeshWithMaterialDelegate = (ScriptingInterfaceOfIMesh.CreateMeshWithMaterialDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.CreateMeshWithMaterialDelegate));
			break;
		case 520:
			ScriptingInterfaceOfIMesh.call_DisableContourDelegate = (ScriptingInterfaceOfIMesh.DisableContourDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.DisableContourDelegate));
			break;
		case 521:
			ScriptingInterfaceOfIMesh.call_GetBaseMeshDelegate = (ScriptingInterfaceOfIMesh.GetBaseMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetBaseMeshDelegate));
			break;
		case 522:
			ScriptingInterfaceOfIMesh.call_GetBillboardDelegate = (ScriptingInterfaceOfIMesh.GetBillboardDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetBillboardDelegate));
			break;
		case 523:
			ScriptingInterfaceOfIMesh.call_GetBoundingBoxHeightDelegate = (ScriptingInterfaceOfIMesh.GetBoundingBoxHeightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetBoundingBoxHeightDelegate));
			break;
		case 524:
			ScriptingInterfaceOfIMesh.call_GetBoundingBoxMaxDelegate = (ScriptingInterfaceOfIMesh.GetBoundingBoxMaxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetBoundingBoxMaxDelegate));
			break;
		case 525:
			ScriptingInterfaceOfIMesh.call_GetBoundingBoxMinDelegate = (ScriptingInterfaceOfIMesh.GetBoundingBoxMinDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetBoundingBoxMinDelegate));
			break;
		case 526:
			ScriptingInterfaceOfIMesh.call_GetBoundingBoxWidthDelegate = (ScriptingInterfaceOfIMesh.GetBoundingBoxWidthDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetBoundingBoxWidthDelegate));
			break;
		case 527:
			ScriptingInterfaceOfIMesh.call_GetColorDelegate = (ScriptingInterfaceOfIMesh.GetColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetColorDelegate));
			break;
		case 528:
			ScriptingInterfaceOfIMesh.call_GetColor2Delegate = (ScriptingInterfaceOfIMesh.GetColor2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetColor2Delegate));
			break;
		case 529:
			ScriptingInterfaceOfIMesh.call_GetEditDataFaceCornerCountDelegate = (ScriptingInterfaceOfIMesh.GetEditDataFaceCornerCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetEditDataFaceCornerCountDelegate));
			break;
		case 530:
			ScriptingInterfaceOfIMesh.call_GetEditDataFaceCornerVertexColorDelegate = (ScriptingInterfaceOfIMesh.GetEditDataFaceCornerVertexColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetEditDataFaceCornerVertexColorDelegate));
			break;
		case 531:
			ScriptingInterfaceOfIMesh.call_GetFaceCornerCountDelegate = (ScriptingInterfaceOfIMesh.GetFaceCornerCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetFaceCornerCountDelegate));
			break;
		case 532:
			ScriptingInterfaceOfIMesh.call_GetFaceCountDelegate = (ScriptingInterfaceOfIMesh.GetFaceCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetFaceCountDelegate));
			break;
		case 533:
			ScriptingInterfaceOfIMesh.call_GetLocalFrameDelegate = (ScriptingInterfaceOfIMesh.GetLocalFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetLocalFrameDelegate));
			break;
		case 534:
			ScriptingInterfaceOfIMesh.call_GetMaterialDelegate = (ScriptingInterfaceOfIMesh.GetMaterialDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetMaterialDelegate));
			break;
		case 535:
			ScriptingInterfaceOfIMesh.call_GetMeshFromResourceDelegate = (ScriptingInterfaceOfIMesh.GetMeshFromResourceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetMeshFromResourceDelegate));
			break;
		case 536:
			ScriptingInterfaceOfIMesh.call_GetNameDelegate = (ScriptingInterfaceOfIMesh.GetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetNameDelegate));
			break;
		case 537:
			ScriptingInterfaceOfIMesh.call_GetRandomMeshWithVdeclDelegate = (ScriptingInterfaceOfIMesh.GetRandomMeshWithVdeclDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetRandomMeshWithVdeclDelegate));
			break;
		case 538:
			ScriptingInterfaceOfIMesh.call_GetSecondMaterialDelegate = (ScriptingInterfaceOfIMesh.GetSecondMaterialDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetSecondMaterialDelegate));
			break;
		case 539:
			ScriptingInterfaceOfIMesh.call_GetVisibilityMaskDelegate = (ScriptingInterfaceOfIMesh.GetVisibilityMaskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.GetVisibilityMaskDelegate));
			break;
		case 540:
			ScriptingInterfaceOfIMesh.call_HasTagDelegate = (ScriptingInterfaceOfIMesh.HasTagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.HasTagDelegate));
			break;
		case 541:
			ScriptingInterfaceOfIMesh.call_HintIndicesDynamicDelegate = (ScriptingInterfaceOfIMesh.HintIndicesDynamicDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.HintIndicesDynamicDelegate));
			break;
		case 542:
			ScriptingInterfaceOfIMesh.call_HintVerticesDynamicDelegate = (ScriptingInterfaceOfIMesh.HintVerticesDynamicDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.HintVerticesDynamicDelegate));
			break;
		case 543:
			ScriptingInterfaceOfIMesh.call_LockEditDataWriteDelegate = (ScriptingInterfaceOfIMesh.LockEditDataWriteDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.LockEditDataWriteDelegate));
			break;
		case 544:
			ScriptingInterfaceOfIMesh.call_PreloadForRenderingDelegate = (ScriptingInterfaceOfIMesh.PreloadForRenderingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.PreloadForRenderingDelegate));
			break;
		case 545:
			ScriptingInterfaceOfIMesh.call_RecomputeBoundingBoxDelegate = (ScriptingInterfaceOfIMesh.RecomputeBoundingBoxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.RecomputeBoundingBoxDelegate));
			break;
		case 546:
			ScriptingInterfaceOfIMesh.call_ReleaseEditDataUserDelegate = (ScriptingInterfaceOfIMesh.ReleaseEditDataUserDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.ReleaseEditDataUserDelegate));
			break;
		case 547:
			ScriptingInterfaceOfIMesh.call_ReleaseResourcesDelegate = (ScriptingInterfaceOfIMesh.ReleaseResourcesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.ReleaseResourcesDelegate));
			break;
		case 548:
			ScriptingInterfaceOfIMesh.call_SetAsNotEffectedBySeasonDelegate = (ScriptingInterfaceOfIMesh.SetAsNotEffectedBySeasonDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetAsNotEffectedBySeasonDelegate));
			break;
		case 549:
			ScriptingInterfaceOfIMesh.call_SetBillboardDelegate = (ScriptingInterfaceOfIMesh.SetBillboardDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetBillboardDelegate));
			break;
		case 550:
			ScriptingInterfaceOfIMesh.call_SetColorDelegate = (ScriptingInterfaceOfIMesh.SetColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetColorDelegate));
			break;
		case 551:
			ScriptingInterfaceOfIMesh.call_SetColor2Delegate = (ScriptingInterfaceOfIMesh.SetColor2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetColor2Delegate));
			break;
		case 552:
			ScriptingInterfaceOfIMesh.call_SetColorAlphaDelegate = (ScriptingInterfaceOfIMesh.SetColorAlphaDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetColorAlphaDelegate));
			break;
		case 553:
			ScriptingInterfaceOfIMesh.call_SetColorAndStrokeDelegate = (ScriptingInterfaceOfIMesh.SetColorAndStrokeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetColorAndStrokeDelegate));
			break;
		case 554:
			ScriptingInterfaceOfIMesh.call_SetContourColorDelegate = (ScriptingInterfaceOfIMesh.SetContourColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetContourColorDelegate));
			break;
		case 555:
			ScriptingInterfaceOfIMesh.call_SetCullingModeDelegate = (ScriptingInterfaceOfIMesh.SetCullingModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetCullingModeDelegate));
			break;
		case 556:
			ScriptingInterfaceOfIMesh.call_SetEditDataFaceCornerVertexColorDelegate = (ScriptingInterfaceOfIMesh.SetEditDataFaceCornerVertexColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetEditDataFaceCornerVertexColorDelegate));
			break;
		case 557:
			ScriptingInterfaceOfIMesh.call_SetEditDataPolicyDelegate = (ScriptingInterfaceOfIMesh.SetEditDataPolicyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetEditDataPolicyDelegate));
			break;
		case 558:
			ScriptingInterfaceOfIMesh.call_SetExternalBoundingBoxDelegate = (ScriptingInterfaceOfIMesh.SetExternalBoundingBoxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetExternalBoundingBoxDelegate));
			break;
		case 559:
			ScriptingInterfaceOfIMesh.call_SetLocalFrameDelegate = (ScriptingInterfaceOfIMesh.SetLocalFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetLocalFrameDelegate));
			break;
		case 560:
			ScriptingInterfaceOfIMesh.call_SetMaterialDelegate = (ScriptingInterfaceOfIMesh.SetMaterialDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetMaterialDelegate));
			break;
		case 561:
			ScriptingInterfaceOfIMesh.call_SetMaterialByNameDelegate = (ScriptingInterfaceOfIMesh.SetMaterialByNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetMaterialByNameDelegate));
			break;
		case 562:
			ScriptingInterfaceOfIMesh.call_SetMeshRenderOrderDelegate = (ScriptingInterfaceOfIMesh.SetMeshRenderOrderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetMeshRenderOrderDelegate));
			break;
		case 563:
			ScriptingInterfaceOfIMesh.call_SetMorphTimeDelegate = (ScriptingInterfaceOfIMesh.SetMorphTimeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetMorphTimeDelegate));
			break;
		case 564:
			ScriptingInterfaceOfIMesh.call_SetNameDelegate = (ScriptingInterfaceOfIMesh.SetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetNameDelegate));
			break;
		case 565:
			ScriptingInterfaceOfIMesh.call_SetVectorArgumentDelegate = (ScriptingInterfaceOfIMesh.SetVectorArgumentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetVectorArgumentDelegate));
			break;
		case 566:
			ScriptingInterfaceOfIMesh.call_SetVectorArgument2Delegate = (ScriptingInterfaceOfIMesh.SetVectorArgument2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetVectorArgument2Delegate));
			break;
		case 567:
			ScriptingInterfaceOfIMesh.call_SetVisibilityMaskDelegate = (ScriptingInterfaceOfIMesh.SetVisibilityMaskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.SetVisibilityMaskDelegate));
			break;
		case 568:
			ScriptingInterfaceOfIMesh.call_UnlockEditDataWriteDelegate = (ScriptingInterfaceOfIMesh.UnlockEditDataWriteDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.UnlockEditDataWriteDelegate));
			break;
		case 569:
			ScriptingInterfaceOfIMesh.call_UpdateBoundingBoxDelegate = (ScriptingInterfaceOfIMesh.UpdateBoundingBoxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMesh.UpdateBoundingBoxDelegate));
			break;
		case 570:
			ScriptingInterfaceOfIMeshBuilder.call_CreateTilingButtonMeshDelegate = (ScriptingInterfaceOfIMeshBuilder.CreateTilingButtonMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMeshBuilder.CreateTilingButtonMeshDelegate));
			break;
		case 571:
			ScriptingInterfaceOfIMeshBuilder.call_CreateTilingWindowMeshDelegate = (ScriptingInterfaceOfIMeshBuilder.CreateTilingWindowMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMeshBuilder.CreateTilingWindowMeshDelegate));
			break;
		case 572:
			ScriptingInterfaceOfIMeshBuilder.call_FinalizeMeshBuilderDelegate = (ScriptingInterfaceOfIMeshBuilder.FinalizeMeshBuilderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMeshBuilder.FinalizeMeshBuilderDelegate));
			break;
		case 573:
			ScriptingInterfaceOfIMetaMesh.call_AddEditDataUserDelegate = (ScriptingInterfaceOfIMetaMesh.AddEditDataUserDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.AddEditDataUserDelegate));
			break;
		case 574:
			ScriptingInterfaceOfIMetaMesh.call_AddMeshDelegate = (ScriptingInterfaceOfIMetaMesh.AddMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.AddMeshDelegate));
			break;
		case 575:
			ScriptingInterfaceOfIMetaMesh.call_AddMetaMeshDelegate = (ScriptingInterfaceOfIMetaMesh.AddMetaMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.AddMetaMeshDelegate));
			break;
		case 576:
			ScriptingInterfaceOfIMetaMesh.call_AssignClothBodyFromDelegate = (ScriptingInterfaceOfIMetaMesh.AssignClothBodyFromDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.AssignClothBodyFromDelegate));
			break;
		case 577:
			ScriptingInterfaceOfIMetaMesh.call_BatchMultiMeshesDelegate = (ScriptingInterfaceOfIMetaMesh.BatchMultiMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.BatchMultiMeshesDelegate));
			break;
		case 578:
			ScriptingInterfaceOfIMetaMesh.call_BatchMultiMeshesMultipleDelegate = (ScriptingInterfaceOfIMetaMesh.BatchMultiMeshesMultipleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.BatchMultiMeshesMultipleDelegate));
			break;
		case 579:
			ScriptingInterfaceOfIMetaMesh.call_CheckMetaMeshExistenceDelegate = (ScriptingInterfaceOfIMetaMesh.CheckMetaMeshExistenceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.CheckMetaMeshExistenceDelegate));
			break;
		case 580:
			ScriptingInterfaceOfIMetaMesh.call_CheckResourcesDelegate = (ScriptingInterfaceOfIMetaMesh.CheckResourcesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.CheckResourcesDelegate));
			break;
		case 581:
			ScriptingInterfaceOfIMetaMesh.call_ClearEditDataDelegate = (ScriptingInterfaceOfIMetaMesh.ClearEditDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.ClearEditDataDelegate));
			break;
		case 582:
			ScriptingInterfaceOfIMetaMesh.call_ClearMeshesDelegate = (ScriptingInterfaceOfIMetaMesh.ClearMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.ClearMeshesDelegate));
			break;
		case 583:
			ScriptingInterfaceOfIMetaMesh.call_ClearMeshesForLodDelegate = (ScriptingInterfaceOfIMetaMesh.ClearMeshesForLodDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.ClearMeshesForLodDelegate));
			break;
		case 584:
			ScriptingInterfaceOfIMetaMesh.call_ClearMeshesForLowerLodsDelegate = (ScriptingInterfaceOfIMetaMesh.ClearMeshesForLowerLodsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.ClearMeshesForLowerLodsDelegate));
			break;
		case 585:
			ScriptingInterfaceOfIMetaMesh.call_ClearMeshesForOtherLodsDelegate = (ScriptingInterfaceOfIMetaMesh.ClearMeshesForOtherLodsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.ClearMeshesForOtherLodsDelegate));
			break;
		case 586:
			ScriptingInterfaceOfIMetaMesh.call_CopyToDelegate = (ScriptingInterfaceOfIMetaMesh.CopyToDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.CopyToDelegate));
			break;
		case 587:
			ScriptingInterfaceOfIMetaMesh.call_CreateCopyDelegate = (ScriptingInterfaceOfIMetaMesh.CreateCopyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.CreateCopyDelegate));
			break;
		case 588:
			ScriptingInterfaceOfIMetaMesh.call_CreateCopyFromNameDelegate = (ScriptingInterfaceOfIMetaMesh.CreateCopyFromNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.CreateCopyFromNameDelegate));
			break;
		case 589:
			ScriptingInterfaceOfIMetaMesh.call_CreateMetaMeshDelegate = (ScriptingInterfaceOfIMetaMesh.CreateMetaMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.CreateMetaMeshDelegate));
			break;
		case 590:
			ScriptingInterfaceOfIMetaMesh.call_DrawTextWithDefaultFontDelegate = (ScriptingInterfaceOfIMetaMesh.DrawTextWithDefaultFontDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.DrawTextWithDefaultFontDelegate));
			break;
		case 591:
			ScriptingInterfaceOfIMetaMesh.call_GetAllMultiMeshesDelegate = (ScriptingInterfaceOfIMetaMesh.GetAllMultiMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetAllMultiMeshesDelegate));
			break;
		case 592:
			ScriptingInterfaceOfIMetaMesh.call_GetBoundingBoxDelegate = (ScriptingInterfaceOfIMetaMesh.GetBoundingBoxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetBoundingBoxDelegate));
			break;
		case 593:
			ScriptingInterfaceOfIMetaMesh.call_GetFactor1Delegate = (ScriptingInterfaceOfIMetaMesh.GetFactor1Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetFactor1Delegate));
			break;
		case 594:
			ScriptingInterfaceOfIMetaMesh.call_GetFactor2Delegate = (ScriptingInterfaceOfIMetaMesh.GetFactor2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetFactor2Delegate));
			break;
		case 595:
			ScriptingInterfaceOfIMetaMesh.call_GetFrameDelegate = (ScriptingInterfaceOfIMetaMesh.GetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetFrameDelegate));
			break;
		case 596:
			ScriptingInterfaceOfIMetaMesh.call_GetLodMaskForMeshAtIndexDelegate = (ScriptingInterfaceOfIMetaMesh.GetLodMaskForMeshAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetLodMaskForMeshAtIndexDelegate));
			break;
		case 597:
			ScriptingInterfaceOfIMetaMesh.call_GetMeshAtIndexDelegate = (ScriptingInterfaceOfIMetaMesh.GetMeshAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetMeshAtIndexDelegate));
			break;
		case 598:
			ScriptingInterfaceOfIMetaMesh.call_GetMeshCountDelegate = (ScriptingInterfaceOfIMetaMesh.GetMeshCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetMeshCountDelegate));
			break;
		case 599:
			ScriptingInterfaceOfIMetaMesh.call_GetMeshCountWithTagDelegate = (ScriptingInterfaceOfIMetaMesh.GetMeshCountWithTagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetMeshCountWithTagDelegate));
			break;
		case 600:
			ScriptingInterfaceOfIMetaMesh.call_GetMorphedCopyDelegate = (ScriptingInterfaceOfIMetaMesh.GetMorphedCopyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetMorphedCopyDelegate));
			break;
		case 601:
			ScriptingInterfaceOfIMetaMesh.call_GetMultiMeshDelegate = (ScriptingInterfaceOfIMetaMesh.GetMultiMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetMultiMeshDelegate));
			break;
		case 602:
			ScriptingInterfaceOfIMetaMesh.call_GetMultiMeshCountDelegate = (ScriptingInterfaceOfIMetaMesh.GetMultiMeshCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetMultiMeshCountDelegate));
			break;
		case 603:
			ScriptingInterfaceOfIMetaMesh.call_GetNameDelegate = (ScriptingInterfaceOfIMetaMesh.GetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetNameDelegate));
			break;
		case 604:
			ScriptingInterfaceOfIMetaMesh.call_GetTotalGpuSizeDelegate = (ScriptingInterfaceOfIMetaMesh.GetTotalGpuSizeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetTotalGpuSizeDelegate));
			break;
		case 605:
			ScriptingInterfaceOfIMetaMesh.call_GetVectorArgument2Delegate = (ScriptingInterfaceOfIMetaMesh.GetVectorArgument2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetVectorArgument2Delegate));
			break;
		case 606:
			ScriptingInterfaceOfIMetaMesh.call_GetVectorUserDataDelegate = (ScriptingInterfaceOfIMetaMesh.GetVectorUserDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetVectorUserDataDelegate));
			break;
		case 607:
			ScriptingInterfaceOfIMetaMesh.call_GetVisibilityMaskDelegate = (ScriptingInterfaceOfIMetaMesh.GetVisibilityMaskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.GetVisibilityMaskDelegate));
			break;
		case 608:
			ScriptingInterfaceOfIMetaMesh.call_HasAnyGeneratedLodsDelegate = (ScriptingInterfaceOfIMetaMesh.HasAnyGeneratedLodsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.HasAnyGeneratedLodsDelegate));
			break;
		case 609:
			ScriptingInterfaceOfIMetaMesh.call_HasAnyLodsDelegate = (ScriptingInterfaceOfIMetaMesh.HasAnyLodsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.HasAnyLodsDelegate));
			break;
		case 610:
			ScriptingInterfaceOfIMetaMesh.call_HasClothDataDelegate = (ScriptingInterfaceOfIMetaMesh.HasClothDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.HasClothDataDelegate));
			break;
		case 611:
			ScriptingInterfaceOfIMetaMesh.call_HasVertexBufferOrEditDataOrPackageItemDelegate = (ScriptingInterfaceOfIMetaMesh.HasVertexBufferOrEditDataOrPackageItemDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.HasVertexBufferOrEditDataOrPackageItemDelegate));
			break;
		case 612:
			ScriptingInterfaceOfIMetaMesh.call_MergeMultiMeshesDelegate = (ScriptingInterfaceOfIMetaMesh.MergeMultiMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.MergeMultiMeshesDelegate));
			break;
		case 613:
			ScriptingInterfaceOfIMetaMesh.call_PreloadForRenderingDelegate = (ScriptingInterfaceOfIMetaMesh.PreloadForRenderingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.PreloadForRenderingDelegate));
			break;
		case 614:
			ScriptingInterfaceOfIMetaMesh.call_PreloadShadersDelegate = (ScriptingInterfaceOfIMetaMesh.PreloadShadersDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.PreloadShadersDelegate));
			break;
		case 615:
			ScriptingInterfaceOfIMetaMesh.call_RecomputeBoundingBoxDelegate = (ScriptingInterfaceOfIMetaMesh.RecomputeBoundingBoxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.RecomputeBoundingBoxDelegate));
			break;
		case 616:
			ScriptingInterfaceOfIMetaMesh.call_ReleaseDelegate = (ScriptingInterfaceOfIMetaMesh.ReleaseDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.ReleaseDelegate));
			break;
		case 617:
			ScriptingInterfaceOfIMetaMesh.call_ReleaseEditDataUserDelegate = (ScriptingInterfaceOfIMetaMesh.ReleaseEditDataUserDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.ReleaseEditDataUserDelegate));
			break;
		case 618:
			ScriptingInterfaceOfIMetaMesh.call_RemoveMeshesWithoutTagDelegate = (ScriptingInterfaceOfIMetaMesh.RemoveMeshesWithoutTagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.RemoveMeshesWithoutTagDelegate));
			break;
		case 619:
			ScriptingInterfaceOfIMetaMesh.call_RemoveMeshesWithTagDelegate = (ScriptingInterfaceOfIMetaMesh.RemoveMeshesWithTagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.RemoveMeshesWithTagDelegate));
			break;
		case 620:
			ScriptingInterfaceOfIMetaMesh.call_SetBillboardingDelegate = (ScriptingInterfaceOfIMetaMesh.SetBillboardingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetBillboardingDelegate));
			break;
		case 621:
			ScriptingInterfaceOfIMetaMesh.call_SetContourColorDelegate = (ScriptingInterfaceOfIMetaMesh.SetContourColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetContourColorDelegate));
			break;
		case 622:
			ScriptingInterfaceOfIMetaMesh.call_SetContourStateDelegate = (ScriptingInterfaceOfIMetaMesh.SetContourStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetContourStateDelegate));
			break;
		case 623:
			ScriptingInterfaceOfIMetaMesh.call_SetCullModeDelegate = (ScriptingInterfaceOfIMetaMesh.SetCullModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetCullModeDelegate));
			break;
		case 624:
			ScriptingInterfaceOfIMetaMesh.call_SetEditDataPolicyDelegate = (ScriptingInterfaceOfIMetaMesh.SetEditDataPolicyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetEditDataPolicyDelegate));
			break;
		case 625:
			ScriptingInterfaceOfIMetaMesh.call_SetFactor1Delegate = (ScriptingInterfaceOfIMetaMesh.SetFactor1Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetFactor1Delegate));
			break;
		case 626:
			ScriptingInterfaceOfIMetaMesh.call_SetFactor1LinearDelegate = (ScriptingInterfaceOfIMetaMesh.SetFactor1LinearDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetFactor1LinearDelegate));
			break;
		case 627:
			ScriptingInterfaceOfIMetaMesh.call_SetFactor2Delegate = (ScriptingInterfaceOfIMetaMesh.SetFactor2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetFactor2Delegate));
			break;
		case 628:
			ScriptingInterfaceOfIMetaMesh.call_SetFactor2LinearDelegate = (ScriptingInterfaceOfIMetaMesh.SetFactor2LinearDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetFactor2LinearDelegate));
			break;
		case 629:
			ScriptingInterfaceOfIMetaMesh.call_SetFactorColorToSubMeshesWithTagDelegate = (ScriptingInterfaceOfIMetaMesh.SetFactorColorToSubMeshesWithTagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetFactorColorToSubMeshesWithTagDelegate));
			break;
		case 630:
			ScriptingInterfaceOfIMetaMesh.call_SetFrameDelegate = (ScriptingInterfaceOfIMetaMesh.SetFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetFrameDelegate));
			break;
		case 631:
			ScriptingInterfaceOfIMetaMesh.call_SetGlossMultiplierDelegate = (ScriptingInterfaceOfIMetaMesh.SetGlossMultiplierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetGlossMultiplierDelegate));
			break;
		case 632:
			ScriptingInterfaceOfIMetaMesh.call_SetLodBiasDelegate = (ScriptingInterfaceOfIMetaMesh.SetLodBiasDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetLodBiasDelegate));
			break;
		case 633:
			ScriptingInterfaceOfIMetaMesh.call_SetMaterialDelegate = (ScriptingInterfaceOfIMetaMesh.SetMaterialDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetMaterialDelegate));
			break;
		case 634:
			ScriptingInterfaceOfIMetaMesh.call_SetMaterialToSubMeshesWithTagDelegate = (ScriptingInterfaceOfIMetaMesh.SetMaterialToSubMeshesWithTagDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetMaterialToSubMeshesWithTagDelegate));
			break;
		case 635:
			ScriptingInterfaceOfIMetaMesh.call_SetNumLodsDelegate = (ScriptingInterfaceOfIMetaMesh.SetNumLodsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetNumLodsDelegate));
			break;
		case 636:
			ScriptingInterfaceOfIMetaMesh.call_SetVectorArgumentDelegate = (ScriptingInterfaceOfIMetaMesh.SetVectorArgumentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetVectorArgumentDelegate));
			break;
		case 637:
			ScriptingInterfaceOfIMetaMesh.call_SetVectorArgument2Delegate = (ScriptingInterfaceOfIMetaMesh.SetVectorArgument2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetVectorArgument2Delegate));
			break;
		case 638:
			ScriptingInterfaceOfIMetaMesh.call_SetVectorUserDataDelegate = (ScriptingInterfaceOfIMetaMesh.SetVectorUserDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetVectorUserDataDelegate));
			break;
		case 639:
			ScriptingInterfaceOfIMetaMesh.call_SetVisibilityMaskDelegate = (ScriptingInterfaceOfIMetaMesh.SetVisibilityMaskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.SetVisibilityMaskDelegate));
			break;
		case 640:
			ScriptingInterfaceOfIMetaMesh.call_UseHeadBoneFaceGenScalingDelegate = (ScriptingInterfaceOfIMetaMesh.UseHeadBoneFaceGenScalingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMetaMesh.UseHeadBoneFaceGenScalingDelegate));
			break;
		case 641:
			ScriptingInterfaceOfIMouseManager.call_ActivateMouseCursorDelegate = (ScriptingInterfaceOfIMouseManager.ActivateMouseCursorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMouseManager.ActivateMouseCursorDelegate));
			break;
		case 642:
			ScriptingInterfaceOfIMouseManager.call_LockCursorAtCurrentPositionDelegate = (ScriptingInterfaceOfIMouseManager.LockCursorAtCurrentPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMouseManager.LockCursorAtCurrentPositionDelegate));
			break;
		case 643:
			ScriptingInterfaceOfIMouseManager.call_LockCursorAtPositionDelegate = (ScriptingInterfaceOfIMouseManager.LockCursorAtPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMouseManager.LockCursorAtPositionDelegate));
			break;
		case 644:
			ScriptingInterfaceOfIMouseManager.call_SetMouseCursorDelegate = (ScriptingInterfaceOfIMouseManager.SetMouseCursorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMouseManager.SetMouseCursorDelegate));
			break;
		case 645:
			ScriptingInterfaceOfIMouseManager.call_ShowCursorDelegate = (ScriptingInterfaceOfIMouseManager.ShowCursorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMouseManager.ShowCursorDelegate));
			break;
		case 646:
			ScriptingInterfaceOfIMouseManager.call_UnlockCursorDelegate = (ScriptingInterfaceOfIMouseManager.UnlockCursorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMouseManager.UnlockCursorDelegate));
			break;
		case 647:
			ScriptingInterfaceOfIMusic.call_GetFreeMusicChannelIndexDelegate = (ScriptingInterfaceOfIMusic.GetFreeMusicChannelIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMusic.GetFreeMusicChannelIndexDelegate));
			break;
		case 648:
			ScriptingInterfaceOfIMusic.call_IsClipLoadedDelegate = (ScriptingInterfaceOfIMusic.IsClipLoadedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMusic.IsClipLoadedDelegate));
			break;
		case 649:
			ScriptingInterfaceOfIMusic.call_IsMusicPlayingDelegate = (ScriptingInterfaceOfIMusic.IsMusicPlayingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMusic.IsMusicPlayingDelegate));
			break;
		case 650:
			ScriptingInterfaceOfIMusic.call_LoadClipDelegate = (ScriptingInterfaceOfIMusic.LoadClipDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMusic.LoadClipDelegate));
			break;
		case 651:
			ScriptingInterfaceOfIMusic.call_PauseMusicDelegate = (ScriptingInterfaceOfIMusic.PauseMusicDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMusic.PauseMusicDelegate));
			break;
		case 652:
			ScriptingInterfaceOfIMusic.call_PlayDelayedDelegate = (ScriptingInterfaceOfIMusic.PlayDelayedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMusic.PlayDelayedDelegate));
			break;
		case 653:
			ScriptingInterfaceOfIMusic.call_PlayMusicDelegate = (ScriptingInterfaceOfIMusic.PlayMusicDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMusic.PlayMusicDelegate));
			break;
		case 654:
			ScriptingInterfaceOfIMusic.call_SetVolumeDelegate = (ScriptingInterfaceOfIMusic.SetVolumeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMusic.SetVolumeDelegate));
			break;
		case 655:
			ScriptingInterfaceOfIMusic.call_StopMusicDelegate = (ScriptingInterfaceOfIMusic.StopMusicDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMusic.StopMusicDelegate));
			break;
		case 656:
			ScriptingInterfaceOfIMusic.call_UnloadClipDelegate = (ScriptingInterfaceOfIMusic.UnloadClipDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIMusic.UnloadClipDelegate));
			break;
		case 657:
			ScriptingInterfaceOfIParticleSystem.call_CreateParticleSystemAttachedToBoneDelegate = (ScriptingInterfaceOfIParticleSystem.CreateParticleSystemAttachedToBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIParticleSystem.CreateParticleSystemAttachedToBoneDelegate));
			break;
		case 658:
			ScriptingInterfaceOfIParticleSystem.call_CreateParticleSystemAttachedToEntityDelegate = (ScriptingInterfaceOfIParticleSystem.CreateParticleSystemAttachedToEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIParticleSystem.CreateParticleSystemAttachedToEntityDelegate));
			break;
		case 659:
			ScriptingInterfaceOfIParticleSystem.call_GetLocalFrameDelegate = (ScriptingInterfaceOfIParticleSystem.GetLocalFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIParticleSystem.GetLocalFrameDelegate));
			break;
		case 660:
			ScriptingInterfaceOfIParticleSystem.call_GetRuntimeIdByNameDelegate = (ScriptingInterfaceOfIParticleSystem.GetRuntimeIdByNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIParticleSystem.GetRuntimeIdByNameDelegate));
			break;
		case 661:
			ScriptingInterfaceOfIParticleSystem.call_RestartDelegate = (ScriptingInterfaceOfIParticleSystem.RestartDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIParticleSystem.RestartDelegate));
			break;
		case 662:
			ScriptingInterfaceOfIParticleSystem.call_SetEnableDelegate = (ScriptingInterfaceOfIParticleSystem.SetEnableDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIParticleSystem.SetEnableDelegate));
			break;
		case 663:
			ScriptingInterfaceOfIParticleSystem.call_SetLocalFrameDelegate = (ScriptingInterfaceOfIParticleSystem.SetLocalFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIParticleSystem.SetLocalFrameDelegate));
			break;
		case 664:
			ScriptingInterfaceOfIParticleSystem.call_SetParticleEffectByNameDelegate = (ScriptingInterfaceOfIParticleSystem.SetParticleEffectByNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIParticleSystem.SetParticleEffectByNameDelegate));
			break;
		case 665:
			ScriptingInterfaceOfIParticleSystem.call_SetRuntimeEmissionRateMultiplierDelegate = (ScriptingInterfaceOfIParticleSystem.SetRuntimeEmissionRateMultiplierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIParticleSystem.SetRuntimeEmissionRateMultiplierDelegate));
			break;
		case 666:
			ScriptingInterfaceOfIPath.call_AddPathPointDelegate = (ScriptingInterfaceOfIPath.AddPathPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.AddPathPointDelegate));
			break;
		case 667:
			ScriptingInterfaceOfIPath.call_DeletePathPointDelegate = (ScriptingInterfaceOfIPath.DeletePathPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.DeletePathPointDelegate));
			break;
		case 668:
			ScriptingInterfaceOfIPath.call_GetArcLengthDelegate = (ScriptingInterfaceOfIPath.GetArcLengthDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.GetArcLengthDelegate));
			break;
		case 669:
			ScriptingInterfaceOfIPath.call_GetHermiteFrameAndColorForDistanceDelegate = (ScriptingInterfaceOfIPath.GetHermiteFrameAndColorForDistanceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.GetHermiteFrameAndColorForDistanceDelegate));
			break;
		case 670:
			ScriptingInterfaceOfIPath.call_GetHermiteFrameForDistanceDelegate = (ScriptingInterfaceOfIPath.GetHermiteFrameForDistanceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.GetHermiteFrameForDistanceDelegate));
			break;
		case 671:
			ScriptingInterfaceOfIPath.call_GetHermiteFrameForDtDelegate = (ScriptingInterfaceOfIPath.GetHermiteFrameForDtDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.GetHermiteFrameForDtDelegate));
			break;
		case 672:
			ScriptingInterfaceOfIPath.call_GetNameDelegate = (ScriptingInterfaceOfIPath.GetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.GetNameDelegate));
			break;
		case 673:
			ScriptingInterfaceOfIPath.call_GetNearestHermiteFrameWithValidAlphaForDistanceDelegate = (ScriptingInterfaceOfIPath.GetNearestHermiteFrameWithValidAlphaForDistanceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.GetNearestHermiteFrameWithValidAlphaForDistanceDelegate));
			break;
		case 674:
			ScriptingInterfaceOfIPath.call_GetNumberOfPointsDelegate = (ScriptingInterfaceOfIPath.GetNumberOfPointsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.GetNumberOfPointsDelegate));
			break;
		case 675:
			ScriptingInterfaceOfIPath.call_GetPointsDelegate = (ScriptingInterfaceOfIPath.GetPointsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.GetPointsDelegate));
			break;
		case 676:
			ScriptingInterfaceOfIPath.call_GetTotalLengthDelegate = (ScriptingInterfaceOfIPath.GetTotalLengthDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.GetTotalLengthDelegate));
			break;
		case 677:
			ScriptingInterfaceOfIPath.call_GetVersionDelegate = (ScriptingInterfaceOfIPath.GetVersionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.GetVersionDelegate));
			break;
		case 678:
			ScriptingInterfaceOfIPath.call_HasValidAlphaAtPathPointDelegate = (ScriptingInterfaceOfIPath.HasValidAlphaAtPathPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.HasValidAlphaAtPathPointDelegate));
			break;
		case 679:
			ScriptingInterfaceOfIPath.call_SetFrameOfPointDelegate = (ScriptingInterfaceOfIPath.SetFrameOfPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.SetFrameOfPointDelegate));
			break;
		case 680:
			ScriptingInterfaceOfIPath.call_SetTangentPositionOfPointDelegate = (ScriptingInterfaceOfIPath.SetTangentPositionOfPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPath.SetTangentPositionOfPointDelegate));
			break;
		case 681:
			ScriptingInterfaceOfIPhysicsMaterial.call_GetDynamicFrictionAtIndexDelegate = (ScriptingInterfaceOfIPhysicsMaterial.GetDynamicFrictionAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsMaterial.GetDynamicFrictionAtIndexDelegate));
			break;
		case 682:
			ScriptingInterfaceOfIPhysicsMaterial.call_GetFlagsAtIndexDelegate = (ScriptingInterfaceOfIPhysicsMaterial.GetFlagsAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsMaterial.GetFlagsAtIndexDelegate));
			break;
		case 683:
			ScriptingInterfaceOfIPhysicsMaterial.call_GetIndexWithNameDelegate = (ScriptingInterfaceOfIPhysicsMaterial.GetIndexWithNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsMaterial.GetIndexWithNameDelegate));
			break;
		case 684:
			ScriptingInterfaceOfIPhysicsMaterial.call_GetMaterialCountDelegate = (ScriptingInterfaceOfIPhysicsMaterial.GetMaterialCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsMaterial.GetMaterialCountDelegate));
			break;
		case 685:
			ScriptingInterfaceOfIPhysicsMaterial.call_GetMaterialNameAtIndexDelegate = (ScriptingInterfaceOfIPhysicsMaterial.GetMaterialNameAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsMaterial.GetMaterialNameAtIndexDelegate));
			break;
		case 686:
			ScriptingInterfaceOfIPhysicsMaterial.call_GetRestitutionAtIndexDelegate = (ScriptingInterfaceOfIPhysicsMaterial.GetRestitutionAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsMaterial.GetRestitutionAtIndexDelegate));
			break;
		case 687:
			ScriptingInterfaceOfIPhysicsMaterial.call_GetSoftnessAtIndexDelegate = (ScriptingInterfaceOfIPhysicsMaterial.GetSoftnessAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsMaterial.GetSoftnessAtIndexDelegate));
			break;
		case 688:
			ScriptingInterfaceOfIPhysicsMaterial.call_GetStaticFrictionAtIndexDelegate = (ScriptingInterfaceOfIPhysicsMaterial.GetStaticFrictionAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsMaterial.GetStaticFrictionAtIndexDelegate));
			break;
		case 689:
			ScriptingInterfaceOfIPhysicsShape.call_AddCapsuleDelegate = (ScriptingInterfaceOfIPhysicsShape.AddCapsuleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.AddCapsuleDelegate));
			break;
		case 690:
			ScriptingInterfaceOfIPhysicsShape.call_AddPreloadQueueWithNameDelegate = (ScriptingInterfaceOfIPhysicsShape.AddPreloadQueueWithNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.AddPreloadQueueWithNameDelegate));
			break;
		case 691:
			ScriptingInterfaceOfIPhysicsShape.call_AddSphereDelegate = (ScriptingInterfaceOfIPhysicsShape.AddSphereDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.AddSphereDelegate));
			break;
		case 692:
			ScriptingInterfaceOfIPhysicsShape.call_CapsuleCountDelegate = (ScriptingInterfaceOfIPhysicsShape.CapsuleCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.CapsuleCountDelegate));
			break;
		case 693:
			ScriptingInterfaceOfIPhysicsShape.call_clearDelegate = (ScriptingInterfaceOfIPhysicsShape.clearDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.clearDelegate));
			break;
		case 694:
			ScriptingInterfaceOfIPhysicsShape.call_CreateBodyCopyDelegate = (ScriptingInterfaceOfIPhysicsShape.CreateBodyCopyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.CreateBodyCopyDelegate));
			break;
		case 695:
			ScriptingInterfaceOfIPhysicsShape.call_GetBoundingBoxCenterDelegate = (ScriptingInterfaceOfIPhysicsShape.GetBoundingBoxCenterDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.GetBoundingBoxCenterDelegate));
			break;
		case 696:
			ScriptingInterfaceOfIPhysicsShape.call_GetCapsuleDelegate = (ScriptingInterfaceOfIPhysicsShape.GetCapsuleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.GetCapsuleDelegate));
			break;
		case 697:
			ScriptingInterfaceOfIPhysicsShape.call_GetCapsuleWithMaterialDelegate = (ScriptingInterfaceOfIPhysicsShape.GetCapsuleWithMaterialDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.GetCapsuleWithMaterialDelegate));
			break;
		case 698:
			ScriptingInterfaceOfIPhysicsShape.call_GetDominantMaterialForTriangleMeshDelegate = (ScriptingInterfaceOfIPhysicsShape.GetDominantMaterialForTriangleMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.GetDominantMaterialForTriangleMeshDelegate));
			break;
		case 699:
			ScriptingInterfaceOfIPhysicsShape.call_GetFromResourceDelegate = (ScriptingInterfaceOfIPhysicsShape.GetFromResourceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.GetFromResourceDelegate));
			break;
		case 700:
			ScriptingInterfaceOfIPhysicsShape.call_GetNameDelegate = (ScriptingInterfaceOfIPhysicsShape.GetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.GetNameDelegate));
			break;
		case 701:
			ScriptingInterfaceOfIPhysicsShape.call_GetSphereDelegate = (ScriptingInterfaceOfIPhysicsShape.GetSphereDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.GetSphereDelegate));
			break;
		case 702:
			ScriptingInterfaceOfIPhysicsShape.call_GetSphereWithMaterialDelegate = (ScriptingInterfaceOfIPhysicsShape.GetSphereWithMaterialDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.GetSphereWithMaterialDelegate));
			break;
		case 703:
			ScriptingInterfaceOfIPhysicsShape.call_GetTriangleDelegate = (ScriptingInterfaceOfIPhysicsShape.GetTriangleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.GetTriangleDelegate));
			break;
		case 704:
			ScriptingInterfaceOfIPhysicsShape.call_InitDescriptionDelegate = (ScriptingInterfaceOfIPhysicsShape.InitDescriptionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.InitDescriptionDelegate));
			break;
		case 705:
			ScriptingInterfaceOfIPhysicsShape.call_PrepareDelegate = (ScriptingInterfaceOfIPhysicsShape.PrepareDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.PrepareDelegate));
			break;
		case 706:
			ScriptingInterfaceOfIPhysicsShape.call_ProcessPreloadQueueDelegate = (ScriptingInterfaceOfIPhysicsShape.ProcessPreloadQueueDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.ProcessPreloadQueueDelegate));
			break;
		case 707:
			ScriptingInterfaceOfIPhysicsShape.call_SetCapsuleDelegate = (ScriptingInterfaceOfIPhysicsShape.SetCapsuleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.SetCapsuleDelegate));
			break;
		case 708:
			ScriptingInterfaceOfIPhysicsShape.call_SphereCountDelegate = (ScriptingInterfaceOfIPhysicsShape.SphereCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.SphereCountDelegate));
			break;
		case 709:
			ScriptingInterfaceOfIPhysicsShape.call_TransformDelegate = (ScriptingInterfaceOfIPhysicsShape.TransformDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.TransformDelegate));
			break;
		case 710:
			ScriptingInterfaceOfIPhysicsShape.call_TriangleCountInTriangleMeshDelegate = (ScriptingInterfaceOfIPhysicsShape.TriangleCountInTriangleMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.TriangleCountInTriangleMeshDelegate));
			break;
		case 711:
			ScriptingInterfaceOfIPhysicsShape.call_TriangleMeshCountDelegate = (ScriptingInterfaceOfIPhysicsShape.TriangleMeshCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.TriangleMeshCountDelegate));
			break;
		case 712:
			ScriptingInterfaceOfIPhysicsShape.call_UnloadDynamicBodiesDelegate = (ScriptingInterfaceOfIPhysicsShape.UnloadDynamicBodiesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIPhysicsShape.UnloadDynamicBodiesDelegate));
			break;
		case 713:
			ScriptingInterfaceOfIScene.call_AddDecalInstanceDelegate = (ScriptingInterfaceOfIScene.AddDecalInstanceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.AddDecalInstanceDelegate));
			break;
		case 714:
			ScriptingInterfaceOfIScene.call_AddDirectionalLightDelegate = (ScriptingInterfaceOfIScene.AddDirectionalLightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.AddDirectionalLightDelegate));
			break;
		case 715:
			ScriptingInterfaceOfIScene.call_AddEntityWithMeshDelegate = (ScriptingInterfaceOfIScene.AddEntityWithMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.AddEntityWithMeshDelegate));
			break;
		case 716:
			ScriptingInterfaceOfIScene.call_AddEntityWithMultiMeshDelegate = (ScriptingInterfaceOfIScene.AddEntityWithMultiMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.AddEntityWithMultiMeshDelegate));
			break;
		case 717:
			ScriptingInterfaceOfIScene.call_AddItemEntityDelegate = (ScriptingInterfaceOfIScene.AddItemEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.AddItemEntityDelegate));
			break;
		case 718:
			ScriptingInterfaceOfIScene.call_AddPathDelegate = (ScriptingInterfaceOfIScene.AddPathDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.AddPathDelegate));
			break;
		case 719:
			ScriptingInterfaceOfIScene.call_AddPathPointDelegate = (ScriptingInterfaceOfIScene.AddPathPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.AddPathPointDelegate));
			break;
		case 720:
			ScriptingInterfaceOfIScene.call_AddPointLightDelegate = (ScriptingInterfaceOfIScene.AddPointLightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.AddPointLightDelegate));
			break;
		case 721:
			ScriptingInterfaceOfIScene.call_AttachEntityDelegate = (ScriptingInterfaceOfIScene.AttachEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.AttachEntityDelegate));
			break;
		case 722:
			ScriptingInterfaceOfIScene.call_BoxCastDelegate = (ScriptingInterfaceOfIScene.BoxCastDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.BoxCastDelegate));
			break;
		case 723:
			ScriptingInterfaceOfIScene.call_BoxCastOnlyForCameraDelegate = (ScriptingInterfaceOfIScene.BoxCastOnlyForCameraDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.BoxCastOnlyForCameraDelegate));
			break;
		case 724:
			ScriptingInterfaceOfIScene.call_CalculateEffectiveLightingDelegate = (ScriptingInterfaceOfIScene.CalculateEffectiveLightingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.CalculateEffectiveLightingDelegate));
			break;
		case 725:
			ScriptingInterfaceOfIScene.call_CheckPathEntitiesFrameChangedDelegate = (ScriptingInterfaceOfIScene.CheckPathEntitiesFrameChangedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.CheckPathEntitiesFrameChangedDelegate));
			break;
		case 726:
			ScriptingInterfaceOfIScene.call_CheckPointCanSeePointDelegate = (ScriptingInterfaceOfIScene.CheckPointCanSeePointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.CheckPointCanSeePointDelegate));
			break;
		case 727:
			ScriptingInterfaceOfIScene.call_CheckResourcesDelegate = (ScriptingInterfaceOfIScene.CheckResourcesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.CheckResourcesDelegate));
			break;
		case 728:
			ScriptingInterfaceOfIScene.call_ClearAllDelegate = (ScriptingInterfaceOfIScene.ClearAllDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.ClearAllDelegate));
			break;
		case 729:
			ScriptingInterfaceOfIScene.call_ClearDecalsDelegate = (ScriptingInterfaceOfIScene.ClearDecalsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.ClearDecalsDelegate));
			break;
		case 730:
			ScriptingInterfaceOfIScene.call_ContainsTerrainDelegate = (ScriptingInterfaceOfIScene.ContainsTerrainDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.ContainsTerrainDelegate));
			break;
		case 731:
			ScriptingInterfaceOfIScene.call_CreateBurstParticleDelegate = (ScriptingInterfaceOfIScene.CreateBurstParticleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.CreateBurstParticleDelegate));
			break;
		case 732:
			ScriptingInterfaceOfIScene.call_CreateNewSceneDelegate = (ScriptingInterfaceOfIScene.CreateNewSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.CreateNewSceneDelegate));
			break;
		case 733:
			ScriptingInterfaceOfIScene.call_CreatePathMeshDelegate = (ScriptingInterfaceOfIScene.CreatePathMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.CreatePathMeshDelegate));
			break;
		case 734:
			ScriptingInterfaceOfIScene.call_CreatePathMesh2Delegate = (ScriptingInterfaceOfIScene.CreatePathMesh2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.CreatePathMesh2Delegate));
			break;
		case 735:
			ScriptingInterfaceOfIScene.call_DeletePathWithNameDelegate = (ScriptingInterfaceOfIScene.DeletePathWithNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.DeletePathWithNameDelegate));
			break;
		case 736:
			ScriptingInterfaceOfIScene.call_DisableStaticShadowsDelegate = (ScriptingInterfaceOfIScene.DisableStaticShadowsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.DisableStaticShadowsDelegate));
			break;
		case 737:
			ScriptingInterfaceOfIScene.call_DoesPathExistBetweenFacesDelegate = (ScriptingInterfaceOfIScene.DoesPathExistBetweenFacesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.DoesPathExistBetweenFacesDelegate));
			break;
		case 738:
			ScriptingInterfaceOfIScene.call_DoesPathExistBetweenPositionsDelegate = (ScriptingInterfaceOfIScene.DoesPathExistBetweenPositionsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.DoesPathExistBetweenPositionsDelegate));
			break;
		case 739:
			ScriptingInterfaceOfIScene.call_EnsurePostfxSystemDelegate = (ScriptingInterfaceOfIScene.EnsurePostfxSystemDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.EnsurePostfxSystemDelegate));
			break;
		case 740:
			ScriptingInterfaceOfIScene.call_FillEntityWithHardBorderPhysicsBarrierDelegate = (ScriptingInterfaceOfIScene.FillEntityWithHardBorderPhysicsBarrierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.FillEntityWithHardBorderPhysicsBarrierDelegate));
			break;
		case 741:
			ScriptingInterfaceOfIScene.call_FillTerrainHeightDataDelegate = (ScriptingInterfaceOfIScene.FillTerrainHeightDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.FillTerrainHeightDataDelegate));
			break;
		case 742:
			ScriptingInterfaceOfIScene.call_FillTerrainPhysicsMaterialIndexDataDelegate = (ScriptingInterfaceOfIScene.FillTerrainPhysicsMaterialIndexDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.FillTerrainPhysicsMaterialIndexDataDelegate));
			break;
		case 743:
			ScriptingInterfaceOfIScene.call_FinishSceneSoundsDelegate = (ScriptingInterfaceOfIScene.FinishSceneSoundsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.FinishSceneSoundsDelegate));
			break;
		case 744:
			ScriptingInterfaceOfIScene.call_ForceLoadResourcesDelegate = (ScriptingInterfaceOfIScene.ForceLoadResourcesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.ForceLoadResourcesDelegate));
			break;
		case 745:
			ScriptingInterfaceOfIScene.call_GenerateContactsWithCapsuleDelegate = (ScriptingInterfaceOfIScene.GenerateContactsWithCapsuleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GenerateContactsWithCapsuleDelegate));
			break;
		case 746:
			ScriptingInterfaceOfIScene.call_GetAllColorGradeNamesDelegate = (ScriptingInterfaceOfIScene.GetAllColorGradeNamesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetAllColorGradeNamesDelegate));
			break;
		case 747:
			ScriptingInterfaceOfIScene.call_GetAllEntitiesWithScriptComponentDelegate = (ScriptingInterfaceOfIScene.GetAllEntitiesWithScriptComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetAllEntitiesWithScriptComponentDelegate));
			break;
		case 748:
			ScriptingInterfaceOfIScene.call_GetAllFilterNamesDelegate = (ScriptingInterfaceOfIScene.GetAllFilterNamesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetAllFilterNamesDelegate));
			break;
		case 749:
			ScriptingInterfaceOfIScene.call_GetBoundingBoxDelegate = (ScriptingInterfaceOfIScene.GetBoundingBoxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetBoundingBoxDelegate));
			break;
		case 750:
			ScriptingInterfaceOfIScene.call_GetCampaignEntityWithNameDelegate = (ScriptingInterfaceOfIScene.GetCampaignEntityWithNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetCampaignEntityWithNameDelegate));
			break;
		case 751:
			ScriptingInterfaceOfIScene.call_GetEntitiesDelegate = (ScriptingInterfaceOfIScene.GetEntitiesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetEntitiesDelegate));
			break;
		case 752:
			ScriptingInterfaceOfIScene.call_GetEntityCountDelegate = (ScriptingInterfaceOfIScene.GetEntityCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetEntityCountDelegate));
			break;
		case 753:
			ScriptingInterfaceOfIScene.call_GetEntityWithGuidDelegate = (ScriptingInterfaceOfIScene.GetEntityWithGuidDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetEntityWithGuidDelegate));
			break;
		case 754:
			ScriptingInterfaceOfIScene.call_GetFirstEntityWithNameDelegate = (ScriptingInterfaceOfIScene.GetFirstEntityWithNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetFirstEntityWithNameDelegate));
			break;
		case 755:
			ScriptingInterfaceOfIScene.call_GetFirstEntityWithScriptComponentDelegate = (ScriptingInterfaceOfIScene.GetFirstEntityWithScriptComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetFirstEntityWithScriptComponentDelegate));
			break;
		case 756:
			ScriptingInterfaceOfIScene.call_GetFloraInstanceCountDelegate = (ScriptingInterfaceOfIScene.GetFloraInstanceCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetFloraInstanceCountDelegate));
			break;
		case 757:
			ScriptingInterfaceOfIScene.call_GetFloraRendererTextureUsageDelegate = (ScriptingInterfaceOfIScene.GetFloraRendererTextureUsageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetFloraRendererTextureUsageDelegate));
			break;
		case 758:
			ScriptingInterfaceOfIScene.call_GetFogDelegate = (ScriptingInterfaceOfIScene.GetFogDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetFogDelegate));
			break;
		case 759:
			ScriptingInterfaceOfIScene.call_GetGroundHeightAndNormalAtPositionDelegate = (ScriptingInterfaceOfIScene.GetGroundHeightAndNormalAtPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetGroundHeightAndNormalAtPositionDelegate));
			break;
		case 760:
			ScriptingInterfaceOfIScene.call_GetGroundHeightAtPositionDelegate = (ScriptingInterfaceOfIScene.GetGroundHeightAtPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetGroundHeightAtPositionDelegate));
			break;
		case 761:
			ScriptingInterfaceOfIScene.call_GetHardBoundaryVertexDelegate = (ScriptingInterfaceOfIScene.GetHardBoundaryVertexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetHardBoundaryVertexDelegate));
			break;
		case 762:
			ScriptingInterfaceOfIScene.call_GetHardBoundaryVertexCountDelegate = (ScriptingInterfaceOfIScene.GetHardBoundaryVertexCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetHardBoundaryVertexCountDelegate));
			break;
		case 763:
			ScriptingInterfaceOfIScene.call_GetHeightAtPointDelegate = (ScriptingInterfaceOfIScene.GetHeightAtPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetHeightAtPointDelegate));
			break;
		case 764:
			ScriptingInterfaceOfIScene.call_GetIdOfNavMeshFaceDelegate = (ScriptingInterfaceOfIScene.GetIdOfNavMeshFaceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetIdOfNavMeshFaceDelegate));
			break;
		case 765:
			ScriptingInterfaceOfIScene.call_GetLastFinalRenderCameraFrameDelegate = (ScriptingInterfaceOfIScene.GetLastFinalRenderCameraFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetLastFinalRenderCameraFrameDelegate));
			break;
		case 766:
			ScriptingInterfaceOfIScene.call_GetLastFinalRenderCameraPositionDelegate = (ScriptingInterfaceOfIScene.GetLastFinalRenderCameraPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetLastFinalRenderCameraPositionDelegate));
			break;
		case 767:
			ScriptingInterfaceOfIScene.call_GetLastPointOnNavigationMeshFromPositionToDestinationDelegate = (ScriptingInterfaceOfIScene.GetLastPointOnNavigationMeshFromPositionToDestinationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetLastPointOnNavigationMeshFromPositionToDestinationDelegate));
			break;
		case 768:
			ScriptingInterfaceOfIScene.call_GetLastPointOnNavigationMeshFromWorldPositionToDestinationDelegate = (ScriptingInterfaceOfIScene.GetLastPointOnNavigationMeshFromWorldPositionToDestinationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetLastPointOnNavigationMeshFromWorldPositionToDestinationDelegate));
			break;
		case 769:
			ScriptingInterfaceOfIScene.call_GetLoadingStateNameDelegate = (ScriptingInterfaceOfIScene.GetLoadingStateNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetLoadingStateNameDelegate));
			break;
		case 770:
			ScriptingInterfaceOfIScene.call_GetModulePathDelegate = (ScriptingInterfaceOfIScene.GetModulePathDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetModulePathDelegate));
			break;
		case 771:
			ScriptingInterfaceOfIScene.call_GetNameDelegate = (ScriptingInterfaceOfIScene.GetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetNameDelegate));
			break;
		case 772:
			ScriptingInterfaceOfIScene.call_GetNavigationMeshFaceForPositionDelegate = (ScriptingInterfaceOfIScene.GetNavigationMeshFaceForPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetNavigationMeshFaceForPositionDelegate));
			break;
		case 773:
			ScriptingInterfaceOfIScene.call_GetNavMeshFaceCenterPositionDelegate = (ScriptingInterfaceOfIScene.GetNavMeshFaceCenterPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetNavMeshFaceCenterPositionDelegate));
			break;
		case 774:
			ScriptingInterfaceOfIScene.call_GetNavMeshFaceCountDelegate = (ScriptingInterfaceOfIScene.GetNavMeshFaceCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetNavMeshFaceCountDelegate));
			break;
		case 775:
			ScriptingInterfaceOfIScene.call_GetNavMeshFaceFirstVertexZDelegate = (ScriptingInterfaceOfIScene.GetNavMeshFaceFirstVertexZDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetNavMeshFaceFirstVertexZDelegate));
			break;
		case 776:
			ScriptingInterfaceOfIScene.call_GetNavMeshFaceIndexDelegate = (ScriptingInterfaceOfIScene.GetNavMeshFaceIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetNavMeshFaceIndexDelegate));
			break;
		case 777:
			ScriptingInterfaceOfIScene.call_GetNavMeshFaceIndex3Delegate = (ScriptingInterfaceOfIScene.GetNavMeshFaceIndex3Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetNavMeshFaceIndex3Delegate));
			break;
		case 778:
			ScriptingInterfaceOfIScene.call_GetNodeDataCountDelegate = (ScriptingInterfaceOfIScene.GetNodeDataCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetNodeDataCountDelegate));
			break;
		case 779:
			ScriptingInterfaceOfIScene.call_GetNormalAtDelegate = (ScriptingInterfaceOfIScene.GetNormalAtDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetNormalAtDelegate));
			break;
		case 780:
			ScriptingInterfaceOfIScene.call_GetNorthAngleDelegate = (ScriptingInterfaceOfIScene.GetNorthAngleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetNorthAngleDelegate));
			break;
		case 781:
			ScriptingInterfaceOfIScene.call_GetNumberOfPathsWithNamePrefixDelegate = (ScriptingInterfaceOfIScene.GetNumberOfPathsWithNamePrefixDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetNumberOfPathsWithNamePrefixDelegate));
			break;
		case 782:
			ScriptingInterfaceOfIScene.call_GetPathBetweenAIFaceIndicesDelegate = (ScriptingInterfaceOfIScene.GetPathBetweenAIFaceIndicesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetPathBetweenAIFaceIndicesDelegate));
			break;
		case 783:
			ScriptingInterfaceOfIScene.call_GetPathBetweenAIFacePointersDelegate = (ScriptingInterfaceOfIScene.GetPathBetweenAIFacePointersDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetPathBetweenAIFacePointersDelegate));
			break;
		case 784:
			ScriptingInterfaceOfIScene.call_GetPathDistanceBetweenAIFacesDelegate = (ScriptingInterfaceOfIScene.GetPathDistanceBetweenAIFacesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetPathDistanceBetweenAIFacesDelegate));
			break;
		case 785:
			ScriptingInterfaceOfIScene.call_GetPathDistanceBetweenPositionsDelegate = (ScriptingInterfaceOfIScene.GetPathDistanceBetweenPositionsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetPathDistanceBetweenPositionsDelegate));
			break;
		case 786:
			ScriptingInterfaceOfIScene.call_GetPathsWithNamePrefixDelegate = (ScriptingInterfaceOfIScene.GetPathsWithNamePrefixDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetPathsWithNamePrefixDelegate));
			break;
		case 787:
			ScriptingInterfaceOfIScene.call_GetPathWithNameDelegate = (ScriptingInterfaceOfIScene.GetPathWithNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetPathWithNameDelegate));
			break;
		case 788:
			ScriptingInterfaceOfIScene.call_GetPhotoModeFocusDelegate = (ScriptingInterfaceOfIScene.GetPhotoModeFocusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetPhotoModeFocusDelegate));
			break;
		case 789:
			ScriptingInterfaceOfIScene.call_GetPhotoModeFovDelegate = (ScriptingInterfaceOfIScene.GetPhotoModeFovDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetPhotoModeFovDelegate));
			break;
		case 790:
			ScriptingInterfaceOfIScene.call_GetPhotoModeOnDelegate = (ScriptingInterfaceOfIScene.GetPhotoModeOnDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetPhotoModeOnDelegate));
			break;
		case 791:
			ScriptingInterfaceOfIScene.call_GetPhotoModeOrbitDelegate = (ScriptingInterfaceOfIScene.GetPhotoModeOrbitDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetPhotoModeOrbitDelegate));
			break;
		case 792:
			ScriptingInterfaceOfIScene.call_GetPhotoModeRollDelegate = (ScriptingInterfaceOfIScene.GetPhotoModeRollDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetPhotoModeRollDelegate));
			break;
		case 793:
			ScriptingInterfaceOfIScene.call_GetPhysicsMinMaxDelegate = (ScriptingInterfaceOfIScene.GetPhysicsMinMaxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetPhysicsMinMaxDelegate));
			break;
		case 794:
			ScriptingInterfaceOfIScene.call_GetRainDensityDelegate = (ScriptingInterfaceOfIScene.GetRainDensityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetRainDensityDelegate));
			break;
		case 795:
			ScriptingInterfaceOfIScene.call_GetRootEntitiesDelegate = (ScriptingInterfaceOfIScene.GetRootEntitiesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetRootEntitiesDelegate));
			break;
		case 796:
			ScriptingInterfaceOfIScene.call_GetRootEntityCountDelegate = (ScriptingInterfaceOfIScene.GetRootEntityCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetRootEntityCountDelegate));
			break;
		case 797:
			ScriptingInterfaceOfIScene.call_GetSceneColorGradeIndexDelegate = (ScriptingInterfaceOfIScene.GetSceneColorGradeIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetSceneColorGradeIndexDelegate));
			break;
		case 798:
			ScriptingInterfaceOfIScene.call_GetSceneFilterIndexDelegate = (ScriptingInterfaceOfIScene.GetSceneFilterIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetSceneFilterIndexDelegate));
			break;
		case 799:
			ScriptingInterfaceOfIScene.call_GetScriptedEntityDelegate = (ScriptingInterfaceOfIScene.GetScriptedEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetScriptedEntityDelegate));
			break;
		case 800:
			ScriptingInterfaceOfIScene.call_GetScriptedEntityCountDelegate = (ScriptingInterfaceOfIScene.GetScriptedEntityCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetScriptedEntityCountDelegate));
			break;
		case 801:
			ScriptingInterfaceOfIScene.call_GetSkyboxMeshDelegate = (ScriptingInterfaceOfIScene.GetSkyboxMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetSkyboxMeshDelegate));
			break;
		case 802:
			ScriptingInterfaceOfIScene.call_GetSnowAmountDataDelegate = (ScriptingInterfaceOfIScene.GetSnowAmountDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetSnowAmountDataDelegate));
			break;
		case 803:
			ScriptingInterfaceOfIScene.call_GetSnowDensityDelegate = (ScriptingInterfaceOfIScene.GetSnowDensityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetSnowDensityDelegate));
			break;
		case 804:
			ScriptingInterfaceOfIScene.call_GetSoftBoundaryVertexDelegate = (ScriptingInterfaceOfIScene.GetSoftBoundaryVertexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetSoftBoundaryVertexDelegate));
			break;
		case 805:
			ScriptingInterfaceOfIScene.call_GetSoftBoundaryVertexCountDelegate = (ScriptingInterfaceOfIScene.GetSoftBoundaryVertexCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetSoftBoundaryVertexCountDelegate));
			break;
		case 806:
			ScriptingInterfaceOfIScene.call_GetSunDirectionDelegate = (ScriptingInterfaceOfIScene.GetSunDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetSunDirectionDelegate));
			break;
		case 807:
			ScriptingInterfaceOfIScene.call_GetTerrainDataDelegate = (ScriptingInterfaceOfIScene.GetTerrainDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetTerrainDataDelegate));
			break;
		case 808:
			ScriptingInterfaceOfIScene.call_GetTerrainHeightDelegate = (ScriptingInterfaceOfIScene.GetTerrainHeightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetTerrainHeightDelegate));
			break;
		case 809:
			ScriptingInterfaceOfIScene.call_GetTerrainHeightAndNormalDelegate = (ScriptingInterfaceOfIScene.GetTerrainHeightAndNormalDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetTerrainHeightAndNormalDelegate));
			break;
		case 810:
			ScriptingInterfaceOfIScene.call_GetTerrainMemoryUsageDelegate = (ScriptingInterfaceOfIScene.GetTerrainMemoryUsageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetTerrainMemoryUsageDelegate));
			break;
		case 811:
			ScriptingInterfaceOfIScene.call_GetTerrainMinMaxHeightDelegate = (ScriptingInterfaceOfIScene.GetTerrainMinMaxHeightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetTerrainMinMaxHeightDelegate));
			break;
		case 812:
			ScriptingInterfaceOfIScene.call_GetTerrainNodeDataDelegate = (ScriptingInterfaceOfIScene.GetTerrainNodeDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetTerrainNodeDataDelegate));
			break;
		case 813:
			ScriptingInterfaceOfIScene.call_GetTerrainPhysicsMaterialIndexAtLayerDelegate = (ScriptingInterfaceOfIScene.GetTerrainPhysicsMaterialIndexAtLayerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetTerrainPhysicsMaterialIndexAtLayerDelegate));
			break;
		case 814:
			ScriptingInterfaceOfIScene.call_GetTimeOfDayDelegate = (ScriptingInterfaceOfIScene.GetTimeOfDayDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetTimeOfDayDelegate));
			break;
		case 815:
			ScriptingInterfaceOfIScene.call_GetTimeSpeedDelegate = (ScriptingInterfaceOfIScene.GetTimeSpeedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetTimeSpeedDelegate));
			break;
		case 816:
			ScriptingInterfaceOfIScene.call_GetUpgradeLevelCountDelegate = (ScriptingInterfaceOfIScene.GetUpgradeLevelCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetUpgradeLevelCountDelegate));
			break;
		case 817:
			ScriptingInterfaceOfIScene.call_GetUpgradeLevelMaskDelegate = (ScriptingInterfaceOfIScene.GetUpgradeLevelMaskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetUpgradeLevelMaskDelegate));
			break;
		case 818:
			ScriptingInterfaceOfIScene.call_GetUpgradeLevelMaskOfLevelNameDelegate = (ScriptingInterfaceOfIScene.GetUpgradeLevelMaskOfLevelNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetUpgradeLevelMaskOfLevelNameDelegate));
			break;
		case 819:
			ScriptingInterfaceOfIScene.call_GetUpgradeLevelNameOfIndexDelegate = (ScriptingInterfaceOfIScene.GetUpgradeLevelNameOfIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetUpgradeLevelNameOfIndexDelegate));
			break;
		case 820:
			ScriptingInterfaceOfIScene.call_GetWaterLevelDelegate = (ScriptingInterfaceOfIScene.GetWaterLevelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetWaterLevelDelegate));
			break;
		case 821:
			ScriptingInterfaceOfIScene.call_GetWaterLevelAtPositionDelegate = (ScriptingInterfaceOfIScene.GetWaterLevelAtPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetWaterLevelAtPositionDelegate));
			break;
		case 822:
			ScriptingInterfaceOfIScene.call_GetWinterTimeFactorDelegate = (ScriptingInterfaceOfIScene.GetWinterTimeFactorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.GetWinterTimeFactorDelegate));
			break;
		case 823:
			ScriptingInterfaceOfIScene.call_HasTerrainHeightmapDelegate = (ScriptingInterfaceOfIScene.HasTerrainHeightmapDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.HasTerrainHeightmapDelegate));
			break;
		case 824:
			ScriptingInterfaceOfIScene.call_InvalidateTerrainPhysicsMaterialsDelegate = (ScriptingInterfaceOfIScene.InvalidateTerrainPhysicsMaterialsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.InvalidateTerrainPhysicsMaterialsDelegate));
			break;
		case 825:
			ScriptingInterfaceOfIScene.call_IsAnyFaceWithIdDelegate = (ScriptingInterfaceOfIScene.IsAnyFaceWithIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.IsAnyFaceWithIdDelegate));
			break;
		case 826:
			ScriptingInterfaceOfIScene.call_IsAtmosphereIndoorDelegate = (ScriptingInterfaceOfIScene.IsAtmosphereIndoorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.IsAtmosphereIndoorDelegate));
			break;
		case 827:
			ScriptingInterfaceOfIScene.call_IsDefaultEditorSceneDelegate = (ScriptingInterfaceOfIScene.IsDefaultEditorSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.IsDefaultEditorSceneDelegate));
			break;
		case 828:
			ScriptingInterfaceOfIScene.call_IsEditorSceneDelegate = (ScriptingInterfaceOfIScene.IsEditorSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.IsEditorSceneDelegate));
			break;
		case 829:
			ScriptingInterfaceOfIScene.call_IsLineToPointClearDelegate = (ScriptingInterfaceOfIScene.IsLineToPointClearDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.IsLineToPointClearDelegate));
			break;
		case 830:
			ScriptingInterfaceOfIScene.call_IsLineToPointClear2Delegate = (ScriptingInterfaceOfIScene.IsLineToPointClear2Delegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.IsLineToPointClear2Delegate));
			break;
		case 831:
			ScriptingInterfaceOfIScene.call_IsLoadingFinishedDelegate = (ScriptingInterfaceOfIScene.IsLoadingFinishedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.IsLoadingFinishedDelegate));
			break;
		case 832:
			ScriptingInterfaceOfIScene.call_IsMultiplayerSceneDelegate = (ScriptingInterfaceOfIScene.IsMultiplayerSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.IsMultiplayerSceneDelegate));
			break;
		case 833:
			ScriptingInterfaceOfIScene.call_LoadNavMeshPrefabDelegate = (ScriptingInterfaceOfIScene.LoadNavMeshPrefabDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.LoadNavMeshPrefabDelegate));
			break;
		case 834:
			ScriptingInterfaceOfIScene.call_MarkFacesWithIdAsLadderDelegate = (ScriptingInterfaceOfIScene.MarkFacesWithIdAsLadderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.MarkFacesWithIdAsLadderDelegate));
			break;
		case 835:
			ScriptingInterfaceOfIScene.call_MergeFacesWithIdDelegate = (ScriptingInterfaceOfIScene.MergeFacesWithIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.MergeFacesWithIdDelegate));
			break;
		case 836:
			ScriptingInterfaceOfIScene.call_OptimizeSceneDelegate = (ScriptingInterfaceOfIScene.OptimizeSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.OptimizeSceneDelegate));
			break;
		case 837:
			ScriptingInterfaceOfIScene.call_PauseSceneSoundsDelegate = (ScriptingInterfaceOfIScene.PauseSceneSoundsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.PauseSceneSoundsDelegate));
			break;
		case 838:
			ScriptingInterfaceOfIScene.call_PreloadForRenderingDelegate = (ScriptingInterfaceOfIScene.PreloadForRenderingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.PreloadForRenderingDelegate));
			break;
		case 839:
			ScriptingInterfaceOfIScene.call_RayCastForClosestEntityOrTerrainDelegate = (ScriptingInterfaceOfIScene.RayCastForClosestEntityOrTerrainDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.RayCastForClosestEntityOrTerrainDelegate));
			break;
		case 840:
			ScriptingInterfaceOfIScene.call_ReadDelegate = (ScriptingInterfaceOfIScene.ReadDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.ReadDelegate));
			break;
		case 841:
			ScriptingInterfaceOfIScene.call_ReadAndCalculateInitialCameraDelegate = (ScriptingInterfaceOfIScene.ReadAndCalculateInitialCameraDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.ReadAndCalculateInitialCameraDelegate));
			break;
		case 842:
			ScriptingInterfaceOfIScene.call_RemoveEntityDelegate = (ScriptingInterfaceOfIScene.RemoveEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.RemoveEntityDelegate));
			break;
		case 843:
			ScriptingInterfaceOfIScene.call_ResumeLoadingRenderingsDelegate = (ScriptingInterfaceOfIScene.ResumeLoadingRenderingsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.ResumeLoadingRenderingsDelegate));
			break;
		case 844:
			ScriptingInterfaceOfIScene.call_ResumeSceneSoundsDelegate = (ScriptingInterfaceOfIScene.ResumeSceneSoundsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.ResumeSceneSoundsDelegate));
			break;
		case 845:
			ScriptingInterfaceOfIScene.call_SelectEntitiesCollidedWithDelegate = (ScriptingInterfaceOfIScene.SelectEntitiesCollidedWithDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SelectEntitiesCollidedWithDelegate));
			break;
		case 846:
			ScriptingInterfaceOfIScene.call_SelectEntitiesInBoxWithScriptComponentDelegate = (ScriptingInterfaceOfIScene.SelectEntitiesInBoxWithScriptComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SelectEntitiesInBoxWithScriptComponentDelegate));
			break;
		case 847:
			ScriptingInterfaceOfIScene.call_SeparateFacesWithIdDelegate = (ScriptingInterfaceOfIScene.SeparateFacesWithIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SeparateFacesWithIdDelegate));
			break;
		case 848:
			ScriptingInterfaceOfIScene.call_SetAberrationOffsetDelegate = (ScriptingInterfaceOfIScene.SetAberrationOffsetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetAberrationOffsetDelegate));
			break;
		case 849:
			ScriptingInterfaceOfIScene.call_SetAberrationSizeDelegate = (ScriptingInterfaceOfIScene.SetAberrationSizeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetAberrationSizeDelegate));
			break;
		case 850:
			ScriptingInterfaceOfIScene.call_SetAberrationSmoothDelegate = (ScriptingInterfaceOfIScene.SetAberrationSmoothDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetAberrationSmoothDelegate));
			break;
		case 851:
			ScriptingInterfaceOfIScene.call_SetAbilityOfFacesWithIdDelegate = (ScriptingInterfaceOfIScene.SetAbilityOfFacesWithIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetAbilityOfFacesWithIdDelegate));
			break;
		case 852:
			ScriptingInterfaceOfIScene.call_SetActiveVisibilityLevelsDelegate = (ScriptingInterfaceOfIScene.SetActiveVisibilityLevelsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetActiveVisibilityLevelsDelegate));
			break;
		case 853:
			ScriptingInterfaceOfIScene.call_SetAntialiasingModeDelegate = (ScriptingInterfaceOfIScene.SetAntialiasingModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetAntialiasingModeDelegate));
			break;
		case 854:
			ScriptingInterfaceOfIScene.call_SetAtmosphereWithNameDelegate = (ScriptingInterfaceOfIScene.SetAtmosphereWithNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetAtmosphereWithNameDelegate));
			break;
		case 855:
			ScriptingInterfaceOfIScene.call_SetBloomDelegate = (ScriptingInterfaceOfIScene.SetBloomDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetBloomDelegate));
			break;
		case 856:
			ScriptingInterfaceOfIScene.call_SetBloomAmountDelegate = (ScriptingInterfaceOfIScene.SetBloomAmountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetBloomAmountDelegate));
			break;
		case 857:
			ScriptingInterfaceOfIScene.call_SetBloomStrengthDelegate = (ScriptingInterfaceOfIScene.SetBloomStrengthDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetBloomStrengthDelegate));
			break;
		case 858:
			ScriptingInterfaceOfIScene.call_SetBrightpassTresholdDelegate = (ScriptingInterfaceOfIScene.SetBrightpassTresholdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetBrightpassTresholdDelegate));
			break;
		case 859:
			ScriptingInterfaceOfIScene.call_SetClothSimulationStateDelegate = (ScriptingInterfaceOfIScene.SetClothSimulationStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetClothSimulationStateDelegate));
			break;
		case 860:
			ScriptingInterfaceOfIScene.call_SetColorGradeBlendDelegate = (ScriptingInterfaceOfIScene.SetColorGradeBlendDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetColorGradeBlendDelegate));
			break;
		case 861:
			ScriptingInterfaceOfIScene.call_SetDLSSModeDelegate = (ScriptingInterfaceOfIScene.SetDLSSModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetDLSSModeDelegate));
			break;
		case 862:
			ScriptingInterfaceOfIScene.call_SetDofFocusDelegate = (ScriptingInterfaceOfIScene.SetDofFocusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetDofFocusDelegate));
			break;
		case 863:
			ScriptingInterfaceOfIScene.call_SetDofModeDelegate = (ScriptingInterfaceOfIScene.SetDofModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetDofModeDelegate));
			break;
		case 864:
			ScriptingInterfaceOfIScene.call_SetDofParamsDelegate = (ScriptingInterfaceOfIScene.SetDofParamsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetDofParamsDelegate));
			break;
		case 865:
			ScriptingInterfaceOfIScene.call_SetDoNotWaitForLoadingStatesToRenderDelegate = (ScriptingInterfaceOfIScene.SetDoNotWaitForLoadingStatesToRenderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetDoNotWaitForLoadingStatesToRenderDelegate));
			break;
		case 866:
			ScriptingInterfaceOfIScene.call_SetDrynessFactorDelegate = (ScriptingInterfaceOfIScene.SetDrynessFactorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetDrynessFactorDelegate));
			break;
		case 867:
			ScriptingInterfaceOfIScene.call_SetDynamicShadowmapCascadesRadiusMultiplierDelegate = (ScriptingInterfaceOfIScene.SetDynamicShadowmapCascadesRadiusMultiplierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetDynamicShadowmapCascadesRadiusMultiplierDelegate));
			break;
		case 868:
			ScriptingInterfaceOfIScene.call_SetEnvironmentMultiplierDelegate = (ScriptingInterfaceOfIScene.SetEnvironmentMultiplierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetEnvironmentMultiplierDelegate));
			break;
		case 869:
			ScriptingInterfaceOfIScene.call_SetExternalInjectionTextureDelegate = (ScriptingInterfaceOfIScene.SetExternalInjectionTextureDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetExternalInjectionTextureDelegate));
			break;
		case 870:
			ScriptingInterfaceOfIScene.call_SetFogDelegate = (ScriptingInterfaceOfIScene.SetFogDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetFogDelegate));
			break;
		case 871:
			ScriptingInterfaceOfIScene.call_SetFogAdvancedDelegate = (ScriptingInterfaceOfIScene.SetFogAdvancedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetFogAdvancedDelegate));
			break;
		case 872:
			ScriptingInterfaceOfIScene.call_SetFogAmbientColorDelegate = (ScriptingInterfaceOfIScene.SetFogAmbientColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetFogAmbientColorDelegate));
			break;
		case 873:
			ScriptingInterfaceOfIScene.call_SetForcedSnowDelegate = (ScriptingInterfaceOfIScene.SetForcedSnowDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetForcedSnowDelegate));
			break;
		case 874:
			ScriptingInterfaceOfIScene.call_SetGrainAmountDelegate = (ScriptingInterfaceOfIScene.SetGrainAmountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetGrainAmountDelegate));
			break;
		case 875:
			ScriptingInterfaceOfIScene.call_SetHexagonVignetteAlphaDelegate = (ScriptingInterfaceOfIScene.SetHexagonVignetteAlphaDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetHexagonVignetteAlphaDelegate));
			break;
		case 876:
			ScriptingInterfaceOfIScene.call_SetHexagonVignetteColorDelegate = (ScriptingInterfaceOfIScene.SetHexagonVignetteColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetHexagonVignetteColorDelegate));
			break;
		case 877:
			ScriptingInterfaceOfIScene.call_SetHumidityDelegate = (ScriptingInterfaceOfIScene.SetHumidityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetHumidityDelegate));
			break;
		case 878:
			ScriptingInterfaceOfIScene.call_SetLandscapeRainMaskDataDelegate = (ScriptingInterfaceOfIScene.SetLandscapeRainMaskDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLandscapeRainMaskDataDelegate));
			break;
		case 879:
			ScriptingInterfaceOfIScene.call_SetLensDistortionDelegate = (ScriptingInterfaceOfIScene.SetLensDistortionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLensDistortionDelegate));
			break;
		case 880:
			ScriptingInterfaceOfIScene.call_SetLensFlareAberrationOffsetDelegate = (ScriptingInterfaceOfIScene.SetLensFlareAberrationOffsetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLensFlareAberrationOffsetDelegate));
			break;
		case 881:
			ScriptingInterfaceOfIScene.call_SetLensFlareAmountDelegate = (ScriptingInterfaceOfIScene.SetLensFlareAmountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLensFlareAmountDelegate));
			break;
		case 882:
			ScriptingInterfaceOfIScene.call_SetLensFlareBlurSigmaDelegate = (ScriptingInterfaceOfIScene.SetLensFlareBlurSigmaDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLensFlareBlurSigmaDelegate));
			break;
		case 883:
			ScriptingInterfaceOfIScene.call_SetLensFlareBlurSizeDelegate = (ScriptingInterfaceOfIScene.SetLensFlareBlurSizeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLensFlareBlurSizeDelegate));
			break;
		case 884:
			ScriptingInterfaceOfIScene.call_SetLensFlareDiffractionWeightDelegate = (ScriptingInterfaceOfIScene.SetLensFlareDiffractionWeightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLensFlareDiffractionWeightDelegate));
			break;
		case 885:
			ScriptingInterfaceOfIScene.call_SetLensFlareDirtWeightDelegate = (ScriptingInterfaceOfIScene.SetLensFlareDirtWeightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLensFlareDirtWeightDelegate));
			break;
		case 886:
			ScriptingInterfaceOfIScene.call_SetLensFlareGhostSamplesDelegate = (ScriptingInterfaceOfIScene.SetLensFlareGhostSamplesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLensFlareGhostSamplesDelegate));
			break;
		case 887:
			ScriptingInterfaceOfIScene.call_SetLensFlareGhostWeightDelegate = (ScriptingInterfaceOfIScene.SetLensFlareGhostWeightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLensFlareGhostWeightDelegate));
			break;
		case 888:
			ScriptingInterfaceOfIScene.call_SetLensFlareHaloWeightDelegate = (ScriptingInterfaceOfIScene.SetLensFlareHaloWeightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLensFlareHaloWeightDelegate));
			break;
		case 889:
			ScriptingInterfaceOfIScene.call_SetLensFlareHaloWidthDelegate = (ScriptingInterfaceOfIScene.SetLensFlareHaloWidthDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLensFlareHaloWidthDelegate));
			break;
		case 890:
			ScriptingInterfaceOfIScene.call_SetLensFlareStrengthDelegate = (ScriptingInterfaceOfIScene.SetLensFlareStrengthDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLensFlareStrengthDelegate));
			break;
		case 891:
			ScriptingInterfaceOfIScene.call_SetLensFlareThresholdDelegate = (ScriptingInterfaceOfIScene.SetLensFlareThresholdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLensFlareThresholdDelegate));
			break;
		case 892:
			ScriptingInterfaceOfIScene.call_SetLightDiffuseColorDelegate = (ScriptingInterfaceOfIScene.SetLightDiffuseColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLightDiffuseColorDelegate));
			break;
		case 893:
			ScriptingInterfaceOfIScene.call_SetLightDirectionDelegate = (ScriptingInterfaceOfIScene.SetLightDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLightDirectionDelegate));
			break;
		case 894:
			ScriptingInterfaceOfIScene.call_SetLightPositionDelegate = (ScriptingInterfaceOfIScene.SetLightPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetLightPositionDelegate));
			break;
		case 895:
			ScriptingInterfaceOfIScene.call_SetMaxExposureDelegate = (ScriptingInterfaceOfIScene.SetMaxExposureDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetMaxExposureDelegate));
			break;
		case 896:
			ScriptingInterfaceOfIScene.call_SetMiddleGrayDelegate = (ScriptingInterfaceOfIScene.SetMiddleGrayDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetMiddleGrayDelegate));
			break;
		case 897:
			ScriptingInterfaceOfIScene.call_SetMieScatterFocusDelegate = (ScriptingInterfaceOfIScene.SetMieScatterFocusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetMieScatterFocusDelegate));
			break;
		case 898:
			ScriptingInterfaceOfIScene.call_SetMieScatterStrengthDelegate = (ScriptingInterfaceOfIScene.SetMieScatterStrengthDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetMieScatterStrengthDelegate));
			break;
		case 899:
			ScriptingInterfaceOfIScene.call_SetMinExposureDelegate = (ScriptingInterfaceOfIScene.SetMinExposureDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetMinExposureDelegate));
			break;
		case 900:
			ScriptingInterfaceOfIScene.call_SetMotionBlurModeDelegate = (ScriptingInterfaceOfIScene.SetMotionBlurModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetMotionBlurModeDelegate));
			break;
		case 901:
			ScriptingInterfaceOfIScene.call_SetNameDelegate = (ScriptingInterfaceOfIScene.SetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetNameDelegate));
			break;
		case 902:
			ScriptingInterfaceOfIScene.call_SetOcclusionModeDelegate = (ScriptingInterfaceOfIScene.SetOcclusionModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetOcclusionModeDelegate));
			break;
		case 903:
			ScriptingInterfaceOfIScene.call_SetOwnerThreadDelegate = (ScriptingInterfaceOfIScene.SetOwnerThreadDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetOwnerThreadDelegate));
			break;
		case 904:
			ScriptingInterfaceOfIScene.call_SetPhotoModeFocusDelegate = (ScriptingInterfaceOfIScene.SetPhotoModeFocusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetPhotoModeFocusDelegate));
			break;
		case 905:
			ScriptingInterfaceOfIScene.call_SetPhotoModeFovDelegate = (ScriptingInterfaceOfIScene.SetPhotoModeFovDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetPhotoModeFovDelegate));
			break;
		case 906:
			ScriptingInterfaceOfIScene.call_SetPhotoModeOnDelegate = (ScriptingInterfaceOfIScene.SetPhotoModeOnDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetPhotoModeOnDelegate));
			break;
		case 907:
			ScriptingInterfaceOfIScene.call_SetPhotoModeOrbitDelegate = (ScriptingInterfaceOfIScene.SetPhotoModeOrbitDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetPhotoModeOrbitDelegate));
			break;
		case 908:
			ScriptingInterfaceOfIScene.call_SetPhotoModeRollDelegate = (ScriptingInterfaceOfIScene.SetPhotoModeRollDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetPhotoModeRollDelegate));
			break;
		case 909:
			ScriptingInterfaceOfIScene.call_SetPhotoModeVignetteDelegate = (ScriptingInterfaceOfIScene.SetPhotoModeVignetteDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetPhotoModeVignetteDelegate));
			break;
		case 910:
			ScriptingInterfaceOfIScene.call_SetPlaySoundEventsAfterReadyToRenderDelegate = (ScriptingInterfaceOfIScene.SetPlaySoundEventsAfterReadyToRenderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetPlaySoundEventsAfterReadyToRenderDelegate));
			break;
		case 911:
			ScriptingInterfaceOfIScene.call_SetRainDensityDelegate = (ScriptingInterfaceOfIScene.SetRainDensityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetRainDensityDelegate));
			break;
		case 912:
			ScriptingInterfaceOfIScene.call_SetSceneColorGradeDelegate = (ScriptingInterfaceOfIScene.SetSceneColorGradeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetSceneColorGradeDelegate));
			break;
		case 913:
			ScriptingInterfaceOfIScene.call_SetSceneColorGradeIndexDelegate = (ScriptingInterfaceOfIScene.SetSceneColorGradeIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetSceneColorGradeIndexDelegate));
			break;
		case 914:
			ScriptingInterfaceOfIScene.call_SetSceneFilterIndexDelegate = (ScriptingInterfaceOfIScene.SetSceneFilterIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetSceneFilterIndexDelegate));
			break;
		case 915:
			ScriptingInterfaceOfIScene.call_SetShadowDelegate = (ScriptingInterfaceOfIScene.SetShadowDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetShadowDelegate));
			break;
		case 916:
			ScriptingInterfaceOfIScene.call_SetSkyBrightnessDelegate = (ScriptingInterfaceOfIScene.SetSkyBrightnessDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetSkyBrightnessDelegate));
			break;
		case 917:
			ScriptingInterfaceOfIScene.call_SetSkyRotationDelegate = (ScriptingInterfaceOfIScene.SetSkyRotationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetSkyRotationDelegate));
			break;
		case 918:
			ScriptingInterfaceOfIScene.call_SetSnowDensityDelegate = (ScriptingInterfaceOfIScene.SetSnowDensityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetSnowDensityDelegate));
			break;
		case 919:
			ScriptingInterfaceOfIScene.call_SetStreakAmountDelegate = (ScriptingInterfaceOfIScene.SetStreakAmountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetStreakAmountDelegate));
			break;
		case 920:
			ScriptingInterfaceOfIScene.call_SetStreakIntensityDelegate = (ScriptingInterfaceOfIScene.SetStreakIntensityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetStreakIntensityDelegate));
			break;
		case 921:
			ScriptingInterfaceOfIScene.call_SetStreakStrengthDelegate = (ScriptingInterfaceOfIScene.SetStreakStrengthDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetStreakStrengthDelegate));
			break;
		case 922:
			ScriptingInterfaceOfIScene.call_SetStreakStretchDelegate = (ScriptingInterfaceOfIScene.SetStreakStretchDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetStreakStretchDelegate));
			break;
		case 923:
			ScriptingInterfaceOfIScene.call_SetStreakThresholdDelegate = (ScriptingInterfaceOfIScene.SetStreakThresholdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetStreakThresholdDelegate));
			break;
		case 924:
			ScriptingInterfaceOfIScene.call_SetStreakTintDelegate = (ScriptingInterfaceOfIScene.SetStreakTintDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetStreakTintDelegate));
			break;
		case 925:
			ScriptingInterfaceOfIScene.call_SetSunDelegate = (ScriptingInterfaceOfIScene.SetSunDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetSunDelegate));
			break;
		case 926:
			ScriptingInterfaceOfIScene.call_SetSunAngleAltitudeDelegate = (ScriptingInterfaceOfIScene.SetSunAngleAltitudeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetSunAngleAltitudeDelegate));
			break;
		case 927:
			ScriptingInterfaceOfIScene.call_SetSunDirectionDelegate = (ScriptingInterfaceOfIScene.SetSunDirectionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetSunDirectionDelegate));
			break;
		case 928:
			ScriptingInterfaceOfIScene.call_SetSunLightDelegate = (ScriptingInterfaceOfIScene.SetSunLightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetSunLightDelegate));
			break;
		case 929:
			ScriptingInterfaceOfIScene.call_SetSunshaftModeDelegate = (ScriptingInterfaceOfIScene.SetSunshaftModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetSunshaftModeDelegate));
			break;
		case 930:
			ScriptingInterfaceOfIScene.call_SetSunShaftStrengthDelegate = (ScriptingInterfaceOfIScene.SetSunShaftStrengthDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetSunShaftStrengthDelegate));
			break;
		case 931:
			ScriptingInterfaceOfIScene.call_SetSunSizeDelegate = (ScriptingInterfaceOfIScene.SetSunSizeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetSunSizeDelegate));
			break;
		case 932:
			ScriptingInterfaceOfIScene.call_SetTargetExposureDelegate = (ScriptingInterfaceOfIScene.SetTargetExposureDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetTargetExposureDelegate));
			break;
		case 933:
			ScriptingInterfaceOfIScene.call_SetTemperatureDelegate = (ScriptingInterfaceOfIScene.SetTemperatureDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetTemperatureDelegate));
			break;
		case 934:
			ScriptingInterfaceOfIScene.call_SetTerrainDynamicParamsDelegate = (ScriptingInterfaceOfIScene.SetTerrainDynamicParamsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetTerrainDynamicParamsDelegate));
			break;
		case 935:
			ScriptingInterfaceOfIScene.call_SetTimeOfDayDelegate = (ScriptingInterfaceOfIScene.SetTimeOfDayDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetTimeOfDayDelegate));
			break;
		case 936:
			ScriptingInterfaceOfIScene.call_SetTimeSpeedDelegate = (ScriptingInterfaceOfIScene.SetTimeSpeedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetTimeSpeedDelegate));
			break;
		case 937:
			ScriptingInterfaceOfIScene.call_SetUpgradeLevelDelegate = (ScriptingInterfaceOfIScene.SetUpgradeLevelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetUpgradeLevelDelegate));
			break;
		case 938:
			ScriptingInterfaceOfIScene.call_SetUpgradeLevelVisibilityDelegate = (ScriptingInterfaceOfIScene.SetUpgradeLevelVisibilityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetUpgradeLevelVisibilityDelegate));
			break;
		case 939:
			ScriptingInterfaceOfIScene.call_SetUpgradeLevelVisibilityWithMaskDelegate = (ScriptingInterfaceOfIScene.SetUpgradeLevelVisibilityWithMaskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetUpgradeLevelVisibilityWithMaskDelegate));
			break;
		case 940:
			ScriptingInterfaceOfIScene.call_SetUseConstantTimeDelegate = (ScriptingInterfaceOfIScene.SetUseConstantTimeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetUseConstantTimeDelegate));
			break;
		case 941:
			ScriptingInterfaceOfIScene.call_SetVignetteInnerRadiusDelegate = (ScriptingInterfaceOfIScene.SetVignetteInnerRadiusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetVignetteInnerRadiusDelegate));
			break;
		case 942:
			ScriptingInterfaceOfIScene.call_SetVignetteOpacityDelegate = (ScriptingInterfaceOfIScene.SetVignetteOpacityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetVignetteOpacityDelegate));
			break;
		case 943:
			ScriptingInterfaceOfIScene.call_SetVignetteOuterRadiusDelegate = (ScriptingInterfaceOfIScene.SetVignetteOuterRadiusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetVignetteOuterRadiusDelegate));
			break;
		case 944:
			ScriptingInterfaceOfIScene.call_SetWinterTimeFactorDelegate = (ScriptingInterfaceOfIScene.SetWinterTimeFactorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SetWinterTimeFactorDelegate));
			break;
		case 945:
			ScriptingInterfaceOfIScene.call_StallLoadingRenderingsUntilFurtherNoticeDelegate = (ScriptingInterfaceOfIScene.StallLoadingRenderingsUntilFurtherNoticeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.StallLoadingRenderingsUntilFurtherNoticeDelegate));
			break;
		case 946:
			ScriptingInterfaceOfIScene.call_SwapFaceConnectionsWithIdDelegate = (ScriptingInterfaceOfIScene.SwapFaceConnectionsWithIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.SwapFaceConnectionsWithIdDelegate));
			break;
		case 947:
			ScriptingInterfaceOfIScene.call_TakePhotoModePictureDelegate = (ScriptingInterfaceOfIScene.TakePhotoModePictureDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.TakePhotoModePictureDelegate));
			break;
		case 948:
			ScriptingInterfaceOfIScene.call_TickDelegate = (ScriptingInterfaceOfIScene.TickDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.TickDelegate));
			break;
		case 949:
			ScriptingInterfaceOfIScene.call_WorldPositionComputeNearestNavMeshDelegate = (ScriptingInterfaceOfIScene.WorldPositionComputeNearestNavMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.WorldPositionComputeNearestNavMeshDelegate));
			break;
		case 950:
			ScriptingInterfaceOfIScene.call_WorldPositionValidateZDelegate = (ScriptingInterfaceOfIScene.WorldPositionValidateZDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScene.WorldPositionValidateZDelegate));
			break;
		case 951:
			ScriptingInterfaceOfISceneView.call_AddClearTaskDelegate = (ScriptingInterfaceOfISceneView.AddClearTaskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.AddClearTaskDelegate));
			break;
		case 952:
			ScriptingInterfaceOfISceneView.call_CheckSceneReadyToRenderDelegate = (ScriptingInterfaceOfISceneView.CheckSceneReadyToRenderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.CheckSceneReadyToRenderDelegate));
			break;
		case 953:
			ScriptingInterfaceOfISceneView.call_ClearAllDelegate = (ScriptingInterfaceOfISceneView.ClearAllDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.ClearAllDelegate));
			break;
		case 954:
			ScriptingInterfaceOfISceneView.call_CreateSceneViewDelegate = (ScriptingInterfaceOfISceneView.CreateSceneViewDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.CreateSceneViewDelegate));
			break;
		case 955:
			ScriptingInterfaceOfISceneView.call_DoNotClearDelegate = (ScriptingInterfaceOfISceneView.DoNotClearDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.DoNotClearDelegate));
			break;
		case 956:
			ScriptingInterfaceOfISceneView.call_GetSceneDelegate = (ScriptingInterfaceOfISceneView.GetSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.GetSceneDelegate));
			break;
		case 957:
			ScriptingInterfaceOfISceneView.call_ProjectedMousePositionOnGroundDelegate = (ScriptingInterfaceOfISceneView.ProjectedMousePositionOnGroundDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.ProjectedMousePositionOnGroundDelegate));
			break;
		case 958:
			ScriptingInterfaceOfISceneView.call_RayCastForClosestEntityOrTerrainDelegate = (ScriptingInterfaceOfISceneView.RayCastForClosestEntityOrTerrainDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.RayCastForClosestEntityOrTerrainDelegate));
			break;
		case 959:
			ScriptingInterfaceOfISceneView.call_ReadyToRenderDelegate = (ScriptingInterfaceOfISceneView.ReadyToRenderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.ReadyToRenderDelegate));
			break;
		case 960:
			ScriptingInterfaceOfISceneView.call_ScreenPointToViewportPointDelegate = (ScriptingInterfaceOfISceneView.ScreenPointToViewportPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.ScreenPointToViewportPointDelegate));
			break;
		case 961:
			ScriptingInterfaceOfISceneView.call_SetAcceptGlobalDebugRenderObjectsDelegate = (ScriptingInterfaceOfISceneView.SetAcceptGlobalDebugRenderObjectsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetAcceptGlobalDebugRenderObjectsDelegate));
			break;
		case 962:
			ScriptingInterfaceOfISceneView.call_SetCameraDelegate = (ScriptingInterfaceOfISceneView.SetCameraDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetCameraDelegate));
			break;
		case 963:
			ScriptingInterfaceOfISceneView.call_SetCleanScreenUntilLoadingDoneDelegate = (ScriptingInterfaceOfISceneView.SetCleanScreenUntilLoadingDoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetCleanScreenUntilLoadingDoneDelegate));
			break;
		case 964:
			ScriptingInterfaceOfISceneView.call_SetClearAndDisableAfterSucessfullRenderDelegate = (ScriptingInterfaceOfISceneView.SetClearAndDisableAfterSucessfullRenderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetClearAndDisableAfterSucessfullRenderDelegate));
			break;
		case 965:
			ScriptingInterfaceOfISceneView.call_SetClearGbufferDelegate = (ScriptingInterfaceOfISceneView.SetClearGbufferDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetClearGbufferDelegate));
			break;
		case 966:
			ScriptingInterfaceOfISceneView.call_SetFocusedShadowmapDelegate = (ScriptingInterfaceOfISceneView.SetFocusedShadowmapDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetFocusedShadowmapDelegate));
			break;
		case 967:
			ScriptingInterfaceOfISceneView.call_SetForceShaderCompilationDelegate = (ScriptingInterfaceOfISceneView.SetForceShaderCompilationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetForceShaderCompilationDelegate));
			break;
		case 968:
			ScriptingInterfaceOfISceneView.call_SetPointlightResolutionMultiplierDelegate = (ScriptingInterfaceOfISceneView.SetPointlightResolutionMultiplierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetPointlightResolutionMultiplierDelegate));
			break;
		case 969:
			ScriptingInterfaceOfISceneView.call_SetPostfxConfigParamsDelegate = (ScriptingInterfaceOfISceneView.SetPostfxConfigParamsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetPostfxConfigParamsDelegate));
			break;
		case 970:
			ScriptingInterfaceOfISceneView.call_SetPostfxFromConfigDelegate = (ScriptingInterfaceOfISceneView.SetPostfxFromConfigDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetPostfxFromConfigDelegate));
			break;
		case 971:
			ScriptingInterfaceOfISceneView.call_SetRenderWithPostfxDelegate = (ScriptingInterfaceOfISceneView.SetRenderWithPostfxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetRenderWithPostfxDelegate));
			break;
		case 972:
			ScriptingInterfaceOfISceneView.call_SetResolutionScalingDelegate = (ScriptingInterfaceOfISceneView.SetResolutionScalingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetResolutionScalingDelegate));
			break;
		case 973:
			ScriptingInterfaceOfISceneView.call_SetSceneDelegate = (ScriptingInterfaceOfISceneView.SetSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetSceneDelegate));
			break;
		case 974:
			ScriptingInterfaceOfISceneView.call_SetSceneUsesContourDelegate = (ScriptingInterfaceOfISceneView.SetSceneUsesContourDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetSceneUsesContourDelegate));
			break;
		case 975:
			ScriptingInterfaceOfISceneView.call_SetSceneUsesShadowsDelegate = (ScriptingInterfaceOfISceneView.SetSceneUsesShadowsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetSceneUsesShadowsDelegate));
			break;
		case 976:
			ScriptingInterfaceOfISceneView.call_SetSceneUsesSkyboxDelegate = (ScriptingInterfaceOfISceneView.SetSceneUsesSkyboxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetSceneUsesSkyboxDelegate));
			break;
		case 977:
			ScriptingInterfaceOfISceneView.call_SetShadowmapResolutionMultiplierDelegate = (ScriptingInterfaceOfISceneView.SetShadowmapResolutionMultiplierDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.SetShadowmapResolutionMultiplierDelegate));
			break;
		case 978:
			ScriptingInterfaceOfISceneView.call_TranslateMouseDelegate = (ScriptingInterfaceOfISceneView.TranslateMouseDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.TranslateMouseDelegate));
			break;
		case 979:
			ScriptingInterfaceOfISceneView.call_WorldPointToScreenPointDelegate = (ScriptingInterfaceOfISceneView.WorldPointToScreenPointDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISceneView.WorldPointToScreenPointDelegate));
			break;
		case 980:
			ScriptingInterfaceOfIScreen.call_GetAspectRatioDelegate = (ScriptingInterfaceOfIScreen.GetAspectRatioDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScreen.GetAspectRatioDelegate));
			break;
		case 981:
			ScriptingInterfaceOfIScreen.call_GetDesktopHeightDelegate = (ScriptingInterfaceOfIScreen.GetDesktopHeightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScreen.GetDesktopHeightDelegate));
			break;
		case 982:
			ScriptingInterfaceOfIScreen.call_GetDesktopWidthDelegate = (ScriptingInterfaceOfIScreen.GetDesktopWidthDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScreen.GetDesktopWidthDelegate));
			break;
		case 983:
			ScriptingInterfaceOfIScreen.call_GetMouseVisibleDelegate = (ScriptingInterfaceOfIScreen.GetMouseVisibleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScreen.GetMouseVisibleDelegate));
			break;
		case 984:
			ScriptingInterfaceOfIScreen.call_GetRealScreenResolutionHeightDelegate = (ScriptingInterfaceOfIScreen.GetRealScreenResolutionHeightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScreen.GetRealScreenResolutionHeightDelegate));
			break;
		case 985:
			ScriptingInterfaceOfIScreen.call_GetRealScreenResolutionWidthDelegate = (ScriptingInterfaceOfIScreen.GetRealScreenResolutionWidthDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScreen.GetRealScreenResolutionWidthDelegate));
			break;
		case 986:
			ScriptingInterfaceOfIScreen.call_GetUsableAreaPercentagesDelegate = (ScriptingInterfaceOfIScreen.GetUsableAreaPercentagesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScreen.GetUsableAreaPercentagesDelegate));
			break;
		case 987:
			ScriptingInterfaceOfIScreen.call_IsEnterButtonCrossDelegate = (ScriptingInterfaceOfIScreen.IsEnterButtonCrossDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScreen.IsEnterButtonCrossDelegate));
			break;
		case 988:
			ScriptingInterfaceOfIScreen.call_SetMouseVisibleDelegate = (ScriptingInterfaceOfIScreen.SetMouseVisibleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScreen.SetMouseVisibleDelegate));
			break;
		case 989:
			ScriptingInterfaceOfIScriptComponent.call_GetScriptComponentBehaviorDelegate = (ScriptingInterfaceOfIScriptComponent.GetScriptComponentBehaviorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScriptComponent.GetScriptComponentBehaviorDelegate));
			break;
		case 990:
			ScriptingInterfaceOfIScriptComponent.call_SetVariableEditorWidgetStatusDelegate = (ScriptingInterfaceOfIScriptComponent.SetVariableEditorWidgetStatusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIScriptComponent.SetVariableEditorWidgetStatusDelegate));
			break;
		case 991:
			ScriptingInterfaceOfIShader.call_GetFromResourceDelegate = (ScriptingInterfaceOfIShader.GetFromResourceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIShader.GetFromResourceDelegate));
			break;
		case 992:
			ScriptingInterfaceOfIShader.call_GetMaterialShaderFlagMaskDelegate = (ScriptingInterfaceOfIShader.GetMaterialShaderFlagMaskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIShader.GetMaterialShaderFlagMaskDelegate));
			break;
		case 993:
			ScriptingInterfaceOfIShader.call_GetNameDelegate = (ScriptingInterfaceOfIShader.GetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIShader.GetNameDelegate));
			break;
		case 994:
			ScriptingInterfaceOfIShader.call_ReleaseDelegate = (ScriptingInterfaceOfIShader.ReleaseDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIShader.ReleaseDelegate));
			break;
		case 995:
			ScriptingInterfaceOfISkeleton.call_ActivateRagdollDelegate = (ScriptingInterfaceOfISkeleton.ActivateRagdollDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.ActivateRagdollDelegate));
			break;
		case 996:
			ScriptingInterfaceOfISkeleton.call_AddComponentDelegate = (ScriptingInterfaceOfISkeleton.AddComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.AddComponentDelegate));
			break;
		case 997:
			ScriptingInterfaceOfISkeleton.call_AddComponentToBoneDelegate = (ScriptingInterfaceOfISkeleton.AddComponentToBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.AddComponentToBoneDelegate));
			break;
		case 998:
			ScriptingInterfaceOfISkeleton.call_AddMeshDelegate = (ScriptingInterfaceOfISkeleton.AddMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.AddMeshDelegate));
			break;
		case 999:
			ScriptingInterfaceOfISkeleton.call_AddMeshToBoneDelegate = (ScriptingInterfaceOfISkeleton.AddMeshToBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.AddMeshToBoneDelegate));
			break;
		case 1000:
			ScriptingInterfaceOfISkeleton.call_AddPrefabEntityToBoneDelegate = (ScriptingInterfaceOfISkeleton.AddPrefabEntityToBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.AddPrefabEntityToBoneDelegate));
			break;
		case 1001:
			ScriptingInterfaceOfISkeleton.call_ClearComponentsDelegate = (ScriptingInterfaceOfISkeleton.ClearComponentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.ClearComponentsDelegate));
			break;
		case 1002:
			ScriptingInterfaceOfISkeleton.call_ClearMeshesDelegate = (ScriptingInterfaceOfISkeleton.ClearMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.ClearMeshesDelegate));
			break;
		case 1003:
			ScriptingInterfaceOfISkeleton.call_ClearMeshesAtBoneDelegate = (ScriptingInterfaceOfISkeleton.ClearMeshesAtBoneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.ClearMeshesAtBoneDelegate));
			break;
		case 1004:
			ScriptingInterfaceOfISkeleton.call_CreateFromModelDelegate = (ScriptingInterfaceOfISkeleton.CreateFromModelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.CreateFromModelDelegate));
			break;
		case 1005:
			ScriptingInterfaceOfISkeleton.call_CreateFromModelWithNullAnimTreeDelegate = (ScriptingInterfaceOfISkeleton.CreateFromModelWithNullAnimTreeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.CreateFromModelWithNullAnimTreeDelegate));
			break;
		case 1006:
			ScriptingInterfaceOfISkeleton.call_ForceUpdateBoneFramesDelegate = (ScriptingInterfaceOfISkeleton.ForceUpdateBoneFramesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.ForceUpdateBoneFramesDelegate));
			break;
		case 1007:
			ScriptingInterfaceOfISkeleton.call_FreezeDelegate = (ScriptingInterfaceOfISkeleton.FreezeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.FreezeDelegate));
			break;
		case 1008:
			ScriptingInterfaceOfISkeleton.call_GetAllMeshesDelegate = (ScriptingInterfaceOfISkeleton.GetAllMeshesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetAllMeshesDelegate));
			break;
		case 1009:
			ScriptingInterfaceOfISkeleton.call_GetAnimationAtChannelDelegate = (ScriptingInterfaceOfISkeleton.GetAnimationAtChannelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetAnimationAtChannelDelegate));
			break;
		case 1010:
			ScriptingInterfaceOfISkeleton.call_GetAnimationIndexAtChannelDelegate = (ScriptingInterfaceOfISkeleton.GetAnimationIndexAtChannelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetAnimationIndexAtChannelDelegate));
			break;
		case 1011:
			ScriptingInterfaceOfISkeleton.call_GetBoneBodyDelegate = (ScriptingInterfaceOfISkeleton.GetBoneBodyDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneBodyDelegate));
			break;
		case 1012:
			ScriptingInterfaceOfISkeleton.call_GetBoneChildAtIndexDelegate = (ScriptingInterfaceOfISkeleton.GetBoneChildAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneChildAtIndexDelegate));
			break;
		case 1013:
			ScriptingInterfaceOfISkeleton.call_GetBoneChildCountDelegate = (ScriptingInterfaceOfISkeleton.GetBoneChildCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneChildCountDelegate));
			break;
		case 1014:
			ScriptingInterfaceOfISkeleton.call_GetBoneComponentAtIndexDelegate = (ScriptingInterfaceOfISkeleton.GetBoneComponentAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneComponentAtIndexDelegate));
			break;
		case 1015:
			ScriptingInterfaceOfISkeleton.call_GetBoneComponentCountDelegate = (ScriptingInterfaceOfISkeleton.GetBoneComponentCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneComponentCountDelegate));
			break;
		case 1016:
			ScriptingInterfaceOfISkeleton.call_GetBoneCountDelegate = (ScriptingInterfaceOfISkeleton.GetBoneCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneCountDelegate));
			break;
		case 1017:
			ScriptingInterfaceOfISkeleton.call_GetBoneEntitialFrameDelegate = (ScriptingInterfaceOfISkeleton.GetBoneEntitialFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneEntitialFrameDelegate));
			break;
		case 1018:
			ScriptingInterfaceOfISkeleton.call_GetBoneEntitialFrameAtChannelDelegate = (ScriptingInterfaceOfISkeleton.GetBoneEntitialFrameAtChannelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneEntitialFrameAtChannelDelegate));
			break;
		case 1019:
			ScriptingInterfaceOfISkeleton.call_GetBoneEntitialFrameWithIndexDelegate = (ScriptingInterfaceOfISkeleton.GetBoneEntitialFrameWithIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneEntitialFrameWithIndexDelegate));
			break;
		case 1020:
			ScriptingInterfaceOfISkeleton.call_GetBoneEntitialFrameWithNameDelegate = (ScriptingInterfaceOfISkeleton.GetBoneEntitialFrameWithNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneEntitialFrameWithNameDelegate));
			break;
		case 1021:
			ScriptingInterfaceOfISkeleton.call_GetBoneEntitialRestFrameDelegate = (ScriptingInterfaceOfISkeleton.GetBoneEntitialRestFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneEntitialRestFrameDelegate));
			break;
		case 1022:
			ScriptingInterfaceOfISkeleton.call_GetBoneIndexFromNameDelegate = (ScriptingInterfaceOfISkeleton.GetBoneIndexFromNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneIndexFromNameDelegate));
			break;
		case 1023:
			ScriptingInterfaceOfISkeleton.call_GetBoneLocalRestFrameDelegate = (ScriptingInterfaceOfISkeleton.GetBoneLocalRestFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneLocalRestFrameDelegate));
			break;
		case 1024:
			ScriptingInterfaceOfISkeleton.call_GetBoneNameDelegate = (ScriptingInterfaceOfISkeleton.GetBoneNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetBoneNameDelegate));
			break;
		case 1025:
			ScriptingInterfaceOfISkeleton.call_GetComponentAtIndexDelegate = (ScriptingInterfaceOfISkeleton.GetComponentAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetComponentAtIndexDelegate));
			break;
		case 1026:
			ScriptingInterfaceOfISkeleton.call_GetComponentCountDelegate = (ScriptingInterfaceOfISkeleton.GetComponentCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetComponentCountDelegate));
			break;
		case 1027:
			ScriptingInterfaceOfISkeleton.call_GetCurrentRagdollStateDelegate = (ScriptingInterfaceOfISkeleton.GetCurrentRagdollStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetCurrentRagdollStateDelegate));
			break;
		case 1028:
			ScriptingInterfaceOfISkeleton.call_GetNameDelegate = (ScriptingInterfaceOfISkeleton.GetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetNameDelegate));
			break;
		case 1029:
			ScriptingInterfaceOfISkeleton.call_GetParentBoneIndexDelegate = (ScriptingInterfaceOfISkeleton.GetParentBoneIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetParentBoneIndexDelegate));
			break;
		case 1030:
			ScriptingInterfaceOfISkeleton.call_GetSkeletonAnimationParameterAtChannelDelegate = (ScriptingInterfaceOfISkeleton.GetSkeletonAnimationParameterAtChannelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetSkeletonAnimationParameterAtChannelDelegate));
			break;
		case 1031:
			ScriptingInterfaceOfISkeleton.call_GetSkeletonAnimationSpeedAtChannelDelegate = (ScriptingInterfaceOfISkeleton.GetSkeletonAnimationSpeedAtChannelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetSkeletonAnimationSpeedAtChannelDelegate));
			break;
		case 1032:
			ScriptingInterfaceOfISkeleton.call_GetSkeletonBoneMappingDelegate = (ScriptingInterfaceOfISkeleton.GetSkeletonBoneMappingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.GetSkeletonBoneMappingDelegate));
			break;
		case 1033:
			ScriptingInterfaceOfISkeleton.call_HasBoneComponentDelegate = (ScriptingInterfaceOfISkeleton.HasBoneComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.HasBoneComponentDelegate));
			break;
		case 1034:
			ScriptingInterfaceOfISkeleton.call_HasComponentDelegate = (ScriptingInterfaceOfISkeleton.HasComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.HasComponentDelegate));
			break;
		case 1035:
			ScriptingInterfaceOfISkeleton.call_IsFrozenDelegate = (ScriptingInterfaceOfISkeleton.IsFrozenDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.IsFrozenDelegate));
			break;
		case 1036:
			ScriptingInterfaceOfISkeleton.call_RemoveBoneComponentDelegate = (ScriptingInterfaceOfISkeleton.RemoveBoneComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.RemoveBoneComponentDelegate));
			break;
		case 1037:
			ScriptingInterfaceOfISkeleton.call_RemoveComponentDelegate = (ScriptingInterfaceOfISkeleton.RemoveComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.RemoveComponentDelegate));
			break;
		case 1038:
			ScriptingInterfaceOfISkeleton.call_ResetClothsDelegate = (ScriptingInterfaceOfISkeleton.ResetClothsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.ResetClothsDelegate));
			break;
		case 1039:
			ScriptingInterfaceOfISkeleton.call_ResetFramesDelegate = (ScriptingInterfaceOfISkeleton.ResetFramesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.ResetFramesDelegate));
			break;
		case 1040:
			ScriptingInterfaceOfISkeleton.call_SetBoneLocalFrameDelegate = (ScriptingInterfaceOfISkeleton.SetBoneLocalFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.SetBoneLocalFrameDelegate));
			break;
		case 1041:
			ScriptingInterfaceOfISkeleton.call_SetSkeletonAnimationParameterAtChannelDelegate = (ScriptingInterfaceOfISkeleton.SetSkeletonAnimationParameterAtChannelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.SetSkeletonAnimationParameterAtChannelDelegate));
			break;
		case 1042:
			ScriptingInterfaceOfISkeleton.call_SetSkeletonAnimationSpeedAtChannelDelegate = (ScriptingInterfaceOfISkeleton.SetSkeletonAnimationSpeedAtChannelDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.SetSkeletonAnimationSpeedAtChannelDelegate));
			break;
		case 1043:
			ScriptingInterfaceOfISkeleton.call_SetSkeletonUptoDateDelegate = (ScriptingInterfaceOfISkeleton.SetSkeletonUptoDateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.SetSkeletonUptoDateDelegate));
			break;
		case 1044:
			ScriptingInterfaceOfISkeleton.call_SetUsePreciseBoundingVolumeDelegate = (ScriptingInterfaceOfISkeleton.SetUsePreciseBoundingVolumeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.SetUsePreciseBoundingVolumeDelegate));
			break;
		case 1045:
			ScriptingInterfaceOfISkeleton.call_SkeletonModelExistDelegate = (ScriptingInterfaceOfISkeleton.SkeletonModelExistDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.SkeletonModelExistDelegate));
			break;
		case 1046:
			ScriptingInterfaceOfISkeleton.call_TickAnimationsDelegate = (ScriptingInterfaceOfISkeleton.TickAnimationsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.TickAnimationsDelegate));
			break;
		case 1047:
			ScriptingInterfaceOfISkeleton.call_TickAnimationsAndForceUpdateDelegate = (ScriptingInterfaceOfISkeleton.TickAnimationsAndForceUpdateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.TickAnimationsAndForceUpdateDelegate));
			break;
		case 1048:
			ScriptingInterfaceOfISkeleton.call_UpdateEntitialFramesFromLocalFramesDelegate = (ScriptingInterfaceOfISkeleton.UpdateEntitialFramesFromLocalFramesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISkeleton.UpdateEntitialFramesFromLocalFramesDelegate));
			break;
		case 1049:
			ScriptingInterfaceOfISoundEvent.call_CreateEventDelegate = (ScriptingInterfaceOfISoundEvent.CreateEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.CreateEventDelegate));
			break;
		case 1050:
			ScriptingInterfaceOfISoundEvent.call_CreateEventFromExternalFileDelegate = (ScriptingInterfaceOfISoundEvent.CreateEventFromExternalFileDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.CreateEventFromExternalFileDelegate));
			break;
		case 1051:
			ScriptingInterfaceOfISoundEvent.call_CreateEventFromSoundBufferDelegate = (ScriptingInterfaceOfISoundEvent.CreateEventFromSoundBufferDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.CreateEventFromSoundBufferDelegate));
			break;
		case 1052:
			ScriptingInterfaceOfISoundEvent.call_CreateEventFromStringDelegate = (ScriptingInterfaceOfISoundEvent.CreateEventFromStringDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.CreateEventFromStringDelegate));
			break;
		case 1053:
			ScriptingInterfaceOfISoundEvent.call_GetEventIdFromStringDelegate = (ScriptingInterfaceOfISoundEvent.GetEventIdFromStringDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.GetEventIdFromStringDelegate));
			break;
		case 1054:
			ScriptingInterfaceOfISoundEvent.call_GetEventMinMaxDistanceDelegate = (ScriptingInterfaceOfISoundEvent.GetEventMinMaxDistanceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.GetEventMinMaxDistanceDelegate));
			break;
		case 1055:
			ScriptingInterfaceOfISoundEvent.call_GetTotalEventCountDelegate = (ScriptingInterfaceOfISoundEvent.GetTotalEventCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.GetTotalEventCountDelegate));
			break;
		case 1056:
			ScriptingInterfaceOfISoundEvent.call_IsPausedDelegate = (ScriptingInterfaceOfISoundEvent.IsPausedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.IsPausedDelegate));
			break;
		case 1057:
			ScriptingInterfaceOfISoundEvent.call_IsPlayingDelegate = (ScriptingInterfaceOfISoundEvent.IsPlayingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.IsPlayingDelegate));
			break;
		case 1058:
			ScriptingInterfaceOfISoundEvent.call_IsValidDelegate = (ScriptingInterfaceOfISoundEvent.IsValidDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.IsValidDelegate));
			break;
		case 1059:
			ScriptingInterfaceOfISoundEvent.call_PauseEventDelegate = (ScriptingInterfaceOfISoundEvent.PauseEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.PauseEventDelegate));
			break;
		case 1060:
			ScriptingInterfaceOfISoundEvent.call_PlayExtraEventDelegate = (ScriptingInterfaceOfISoundEvent.PlayExtraEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.PlayExtraEventDelegate));
			break;
		case 1061:
			ScriptingInterfaceOfISoundEvent.call_PlaySound2DDelegate = (ScriptingInterfaceOfISoundEvent.PlaySound2DDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.PlaySound2DDelegate));
			break;
		case 1062:
			ScriptingInterfaceOfISoundEvent.call_ReleaseEventDelegate = (ScriptingInterfaceOfISoundEvent.ReleaseEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.ReleaseEventDelegate));
			break;
		case 1063:
			ScriptingInterfaceOfISoundEvent.call_ResumeEventDelegate = (ScriptingInterfaceOfISoundEvent.ResumeEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.ResumeEventDelegate));
			break;
		case 1064:
			ScriptingInterfaceOfISoundEvent.call_SetEventMinMaxDistanceDelegate = (ScriptingInterfaceOfISoundEvent.SetEventMinMaxDistanceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.SetEventMinMaxDistanceDelegate));
			break;
		case 1065:
			ScriptingInterfaceOfISoundEvent.call_SetEventParameterAtIndexDelegate = (ScriptingInterfaceOfISoundEvent.SetEventParameterAtIndexDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.SetEventParameterAtIndexDelegate));
			break;
		case 1066:
			ScriptingInterfaceOfISoundEvent.call_SetEventParameterFromStringDelegate = (ScriptingInterfaceOfISoundEvent.SetEventParameterFromStringDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.SetEventParameterFromStringDelegate));
			break;
		case 1067:
			ScriptingInterfaceOfISoundEvent.call_SetEventPositionDelegate = (ScriptingInterfaceOfISoundEvent.SetEventPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.SetEventPositionDelegate));
			break;
		case 1068:
			ScriptingInterfaceOfISoundEvent.call_SetEventVelocityDelegate = (ScriptingInterfaceOfISoundEvent.SetEventVelocityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.SetEventVelocityDelegate));
			break;
		case 1069:
			ScriptingInterfaceOfISoundEvent.call_SetSwitchDelegate = (ScriptingInterfaceOfISoundEvent.SetSwitchDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.SetSwitchDelegate));
			break;
		case 1070:
			ScriptingInterfaceOfISoundEvent.call_StartEventDelegate = (ScriptingInterfaceOfISoundEvent.StartEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.StartEventDelegate));
			break;
		case 1071:
			ScriptingInterfaceOfISoundEvent.call_StartEventInPositionDelegate = (ScriptingInterfaceOfISoundEvent.StartEventInPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.StartEventInPositionDelegate));
			break;
		case 1072:
			ScriptingInterfaceOfISoundEvent.call_StopEventDelegate = (ScriptingInterfaceOfISoundEvent.StopEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.StopEventDelegate));
			break;
		case 1073:
			ScriptingInterfaceOfISoundEvent.call_TriggerCueDelegate = (ScriptingInterfaceOfISoundEvent.TriggerCueDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundEvent.TriggerCueDelegate));
			break;
		case 1074:
			ScriptingInterfaceOfISoundManager.call_AddSoundClientWithIdDelegate = (ScriptingInterfaceOfISoundManager.AddSoundClientWithIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.AddSoundClientWithIdDelegate));
			break;
		case 1075:
			ScriptingInterfaceOfISoundManager.call_AddXBOXRemoteUserDelegate = (ScriptingInterfaceOfISoundManager.AddXBOXRemoteUserDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.AddXBOXRemoteUserDelegate));
			break;
		case 1076:
			ScriptingInterfaceOfISoundManager.call_ApplyPushToTalkDelegate = (ScriptingInterfaceOfISoundManager.ApplyPushToTalkDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.ApplyPushToTalkDelegate));
			break;
		case 1077:
			ScriptingInterfaceOfISoundManager.call_ClearDataToBeSentDelegate = (ScriptingInterfaceOfISoundManager.ClearDataToBeSentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.ClearDataToBeSentDelegate));
			break;
		case 1078:
			ScriptingInterfaceOfISoundManager.call_ClearXBOXSoundManagerDelegate = (ScriptingInterfaceOfISoundManager.ClearXBOXSoundManagerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.ClearXBOXSoundManagerDelegate));
			break;
		case 1079:
			ScriptingInterfaceOfISoundManager.call_CompressDataDelegate = (ScriptingInterfaceOfISoundManager.CompressDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.CompressDataDelegate));
			break;
		case 1080:
			ScriptingInterfaceOfISoundManager.call_CreateVoiceEventDelegate = (ScriptingInterfaceOfISoundManager.CreateVoiceEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.CreateVoiceEventDelegate));
			break;
		case 1081:
			ScriptingInterfaceOfISoundManager.call_DecompressDataDelegate = (ScriptingInterfaceOfISoundManager.DecompressDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.DecompressDataDelegate));
			break;
		case 1082:
			ScriptingInterfaceOfISoundManager.call_DeleteSoundClientWithIdDelegate = (ScriptingInterfaceOfISoundManager.DeleteSoundClientWithIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.DeleteSoundClientWithIdDelegate));
			break;
		case 1083:
			ScriptingInterfaceOfISoundManager.call_DestroyVoiceEventDelegate = (ScriptingInterfaceOfISoundManager.DestroyVoiceEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.DestroyVoiceEventDelegate));
			break;
		case 1084:
			ScriptingInterfaceOfISoundManager.call_FinalizeVoicePlayEventDelegate = (ScriptingInterfaceOfISoundManager.FinalizeVoicePlayEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.FinalizeVoicePlayEventDelegate));
			break;
		case 1085:
			ScriptingInterfaceOfISoundManager.call_GetAttenuationPositionDelegate = (ScriptingInterfaceOfISoundManager.GetAttenuationPositionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.GetAttenuationPositionDelegate));
			break;
		case 1086:
			ScriptingInterfaceOfISoundManager.call_GetDataToBeSentAtDelegate = (ScriptingInterfaceOfISoundManager.GetDataToBeSentAtDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.GetDataToBeSentAtDelegate));
			break;
		case 1087:
			ScriptingInterfaceOfISoundManager.call_GetGlobalIndexOfEventDelegate = (ScriptingInterfaceOfISoundManager.GetGlobalIndexOfEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.GetGlobalIndexOfEventDelegate));
			break;
		case 1088:
			ScriptingInterfaceOfISoundManager.call_GetListenerFrameDelegate = (ScriptingInterfaceOfISoundManager.GetListenerFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.GetListenerFrameDelegate));
			break;
		case 1089:
			ScriptingInterfaceOfISoundManager.call_GetSizeOfDataToBeSentAtDelegate = (ScriptingInterfaceOfISoundManager.GetSizeOfDataToBeSentAtDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.GetSizeOfDataToBeSentAtDelegate));
			break;
		case 1090:
			ScriptingInterfaceOfISoundManager.call_GetVoiceDataDelegate = (ScriptingInterfaceOfISoundManager.GetVoiceDataDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.GetVoiceDataDelegate));
			break;
		case 1091:
			ScriptingInterfaceOfISoundManager.call_HandleStateChangesDelegate = (ScriptingInterfaceOfISoundManager.HandleStateChangesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.HandleStateChangesDelegate));
			break;
		case 1092:
			ScriptingInterfaceOfISoundManager.call_InitializeVoicePlayEventDelegate = (ScriptingInterfaceOfISoundManager.InitializeVoicePlayEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.InitializeVoicePlayEventDelegate));
			break;
		case 1093:
			ScriptingInterfaceOfISoundManager.call_InitializeXBOXSoundManagerDelegate = (ScriptingInterfaceOfISoundManager.InitializeXBOXSoundManagerDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.InitializeXBOXSoundManagerDelegate));
			break;
		case 1094:
			ScriptingInterfaceOfISoundManager.call_LoadEventFileAuxDelegate = (ScriptingInterfaceOfISoundManager.LoadEventFileAuxDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.LoadEventFileAuxDelegate));
			break;
		case 1095:
			ScriptingInterfaceOfISoundManager.call_ProcessDataToBeReceivedDelegate = (ScriptingInterfaceOfISoundManager.ProcessDataToBeReceivedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.ProcessDataToBeReceivedDelegate));
			break;
		case 1096:
			ScriptingInterfaceOfISoundManager.call_ProcessDataToBeSentDelegate = (ScriptingInterfaceOfISoundManager.ProcessDataToBeSentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.ProcessDataToBeSentDelegate));
			break;
		case 1097:
			ScriptingInterfaceOfISoundManager.call_RemoveXBOXRemoteUserDelegate = (ScriptingInterfaceOfISoundManager.RemoveXBOXRemoteUserDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.RemoveXBOXRemoteUserDelegate));
			break;
		case 1098:
			ScriptingInterfaceOfISoundManager.call_ResetDelegate = (ScriptingInterfaceOfISoundManager.ResetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.ResetDelegate));
			break;
		case 1099:
			ScriptingInterfaceOfISoundManager.call_SetGlobalParameterDelegate = (ScriptingInterfaceOfISoundManager.SetGlobalParameterDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.SetGlobalParameterDelegate));
			break;
		case 1100:
			ScriptingInterfaceOfISoundManager.call_SetListenerFrameDelegate = (ScriptingInterfaceOfISoundManager.SetListenerFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.SetListenerFrameDelegate));
			break;
		case 1101:
			ScriptingInterfaceOfISoundManager.call_SetStateDelegate = (ScriptingInterfaceOfISoundManager.SetStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.SetStateDelegate));
			break;
		case 1102:
			ScriptingInterfaceOfISoundManager.call_StartOneShotEventDelegate = (ScriptingInterfaceOfISoundManager.StartOneShotEventDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.StartOneShotEventDelegate));
			break;
		case 1103:
			ScriptingInterfaceOfISoundManager.call_StartOneShotEventWithParamDelegate = (ScriptingInterfaceOfISoundManager.StartOneShotEventWithParamDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.StartOneShotEventWithParamDelegate));
			break;
		case 1104:
			ScriptingInterfaceOfISoundManager.call_StartVoiceRecordDelegate = (ScriptingInterfaceOfISoundManager.StartVoiceRecordDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.StartVoiceRecordDelegate));
			break;
		case 1105:
			ScriptingInterfaceOfISoundManager.call_StopVoiceRecordDelegate = (ScriptingInterfaceOfISoundManager.StopVoiceRecordDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.StopVoiceRecordDelegate));
			break;
		case 1106:
			ScriptingInterfaceOfISoundManager.call_UpdateVoiceToPlayDelegate = (ScriptingInterfaceOfISoundManager.UpdateVoiceToPlayDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.UpdateVoiceToPlayDelegate));
			break;
		case 1107:
			ScriptingInterfaceOfISoundManager.call_UpdateXBOXChatCommunicationFlagsDelegate = (ScriptingInterfaceOfISoundManager.UpdateXBOXChatCommunicationFlagsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.UpdateXBOXChatCommunicationFlagsDelegate));
			break;
		case 1108:
			ScriptingInterfaceOfISoundManager.call_UpdateXBOXLocalUserDelegate = (ScriptingInterfaceOfISoundManager.UpdateXBOXLocalUserDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfISoundManager.UpdateXBOXLocalUserDelegate));
			break;
		case 1109:
			ScriptingInterfaceOfITableauView.call_CreateTableauViewDelegate = (ScriptingInterfaceOfITableauView.CreateTableauViewDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITableauView.CreateTableauViewDelegate));
			break;
		case 1110:
			ScriptingInterfaceOfITableauView.call_SetContinousRenderingDelegate = (ScriptingInterfaceOfITableauView.SetContinousRenderingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITableauView.SetContinousRenderingDelegate));
			break;
		case 1111:
			ScriptingInterfaceOfITableauView.call_SetDeleteAfterRenderingDelegate = (ScriptingInterfaceOfITableauView.SetDeleteAfterRenderingDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITableauView.SetDeleteAfterRenderingDelegate));
			break;
		case 1112:
			ScriptingInterfaceOfITableauView.call_SetDoNotRenderThisFrameDelegate = (ScriptingInterfaceOfITableauView.SetDoNotRenderThisFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITableauView.SetDoNotRenderThisFrameDelegate));
			break;
		case 1113:
			ScriptingInterfaceOfITableauView.call_SetSortingEnabledDelegate = (ScriptingInterfaceOfITableauView.SetSortingEnabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITableauView.SetSortingEnabledDelegate));
			break;
		case 1114:
			ScriptingInterfaceOfITexture.call_CheckAndGetFromResourceDelegate = (ScriptingInterfaceOfITexture.CheckAndGetFromResourceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.CheckAndGetFromResourceDelegate));
			break;
		case 1115:
			ScriptingInterfaceOfITexture.call_CreateDepthTargetDelegate = (ScriptingInterfaceOfITexture.CreateDepthTargetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.CreateDepthTargetDelegate));
			break;
		case 1116:
			ScriptingInterfaceOfITexture.call_CreateFromByteArrayDelegate = (ScriptingInterfaceOfITexture.CreateFromByteArrayDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.CreateFromByteArrayDelegate));
			break;
		case 1117:
			ScriptingInterfaceOfITexture.call_CreateFromMemoryDelegate = (ScriptingInterfaceOfITexture.CreateFromMemoryDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.CreateFromMemoryDelegate));
			break;
		case 1118:
			ScriptingInterfaceOfITexture.call_CreateRenderTargetDelegate = (ScriptingInterfaceOfITexture.CreateRenderTargetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.CreateRenderTargetDelegate));
			break;
		case 1119:
			ScriptingInterfaceOfITexture.call_CreateTextureFromPathDelegate = (ScriptingInterfaceOfITexture.CreateTextureFromPathDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.CreateTextureFromPathDelegate));
			break;
		case 1120:
			ScriptingInterfaceOfITexture.call_GetCurObjectDelegate = (ScriptingInterfaceOfITexture.GetCurObjectDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.GetCurObjectDelegate));
			break;
		case 1121:
			ScriptingInterfaceOfITexture.call_GetFromResourceDelegate = (ScriptingInterfaceOfITexture.GetFromResourceDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.GetFromResourceDelegate));
			break;
		case 1122:
			ScriptingInterfaceOfITexture.call_GetHeightDelegate = (ScriptingInterfaceOfITexture.GetHeightDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.GetHeightDelegate));
			break;
		case 1123:
			ScriptingInterfaceOfITexture.call_GetMemorySizeDelegate = (ScriptingInterfaceOfITexture.GetMemorySizeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.GetMemorySizeDelegate));
			break;
		case 1124:
			ScriptingInterfaceOfITexture.call_GetNameDelegate = (ScriptingInterfaceOfITexture.GetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.GetNameDelegate));
			break;
		case 1125:
			ScriptingInterfaceOfITexture.call_GetRenderTargetComponentDelegate = (ScriptingInterfaceOfITexture.GetRenderTargetComponentDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.GetRenderTargetComponentDelegate));
			break;
		case 1126:
			ScriptingInterfaceOfITexture.call_GetTableauViewDelegate = (ScriptingInterfaceOfITexture.GetTableauViewDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.GetTableauViewDelegate));
			break;
		case 1127:
			ScriptingInterfaceOfITexture.call_GetWidthDelegate = (ScriptingInterfaceOfITexture.GetWidthDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.GetWidthDelegate));
			break;
		case 1128:
			ScriptingInterfaceOfITexture.call_IsLoadedDelegate = (ScriptingInterfaceOfITexture.IsLoadedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.IsLoadedDelegate));
			break;
		case 1129:
			ScriptingInterfaceOfITexture.call_IsRenderTargetDelegate = (ScriptingInterfaceOfITexture.IsRenderTargetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.IsRenderTargetDelegate));
			break;
		case 1130:
			ScriptingInterfaceOfITexture.call_LoadTextureFromPathDelegate = (ScriptingInterfaceOfITexture.LoadTextureFromPathDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.LoadTextureFromPathDelegate));
			break;
		case 1131:
			ScriptingInterfaceOfITexture.call_ReleaseDelegate = (ScriptingInterfaceOfITexture.ReleaseDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.ReleaseDelegate));
			break;
		case 1132:
			ScriptingInterfaceOfITexture.call_ReleaseGpuMemoriesDelegate = (ScriptingInterfaceOfITexture.ReleaseGpuMemoriesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.ReleaseGpuMemoriesDelegate));
			break;
		case 1133:
			ScriptingInterfaceOfITexture.call_ReleaseNextFrameDelegate = (ScriptingInterfaceOfITexture.ReleaseNextFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.ReleaseNextFrameDelegate));
			break;
		case 1134:
			ScriptingInterfaceOfITexture.call_RemoveContinousTableauTextureDelegate = (ScriptingInterfaceOfITexture.RemoveContinousTableauTextureDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.RemoveContinousTableauTextureDelegate));
			break;
		case 1135:
			ScriptingInterfaceOfITexture.call_SaveTextureAsAlwaysValidDelegate = (ScriptingInterfaceOfITexture.SaveTextureAsAlwaysValidDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.SaveTextureAsAlwaysValidDelegate));
			break;
		case 1136:
			ScriptingInterfaceOfITexture.call_SaveToFileDelegate = (ScriptingInterfaceOfITexture.SaveToFileDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.SaveToFileDelegate));
			break;
		case 1137:
			ScriptingInterfaceOfITexture.call_SetNameDelegate = (ScriptingInterfaceOfITexture.SetNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.SetNameDelegate));
			break;
		case 1138:
			ScriptingInterfaceOfITexture.call_SetTableauViewDelegate = (ScriptingInterfaceOfITexture.SetTableauViewDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.SetTableauViewDelegate));
			break;
		case 1139:
			ScriptingInterfaceOfITexture.call_TransformRenderTargetToResourceTextureDelegate = (ScriptingInterfaceOfITexture.TransformRenderTargetToResourceTextureDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITexture.TransformRenderTargetToResourceTextureDelegate));
			break;
		case 1140:
			ScriptingInterfaceOfITextureView.call_CreateTextureViewDelegate = (ScriptingInterfaceOfITextureView.CreateTextureViewDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITextureView.CreateTextureViewDelegate));
			break;
		case 1141:
			ScriptingInterfaceOfITextureView.call_SetTextureDelegate = (ScriptingInterfaceOfITextureView.SetTextureDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITextureView.SetTextureDelegate));
			break;
		case 1142:
			ScriptingInterfaceOfIThumbnailCreatorView.call_CancelRequestDelegate = (ScriptingInterfaceOfIThumbnailCreatorView.CancelRequestDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIThumbnailCreatorView.CancelRequestDelegate));
			break;
		case 1143:
			ScriptingInterfaceOfIThumbnailCreatorView.call_ClearRequestsDelegate = (ScriptingInterfaceOfIThumbnailCreatorView.ClearRequestsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIThumbnailCreatorView.ClearRequestsDelegate));
			break;
		case 1144:
			ScriptingInterfaceOfIThumbnailCreatorView.call_CreateThumbnailCreatorViewDelegate = (ScriptingInterfaceOfIThumbnailCreatorView.CreateThumbnailCreatorViewDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIThumbnailCreatorView.CreateThumbnailCreatorViewDelegate));
			break;
		case 1145:
			ScriptingInterfaceOfIThumbnailCreatorView.call_GetNumberOfPendingRequestsDelegate = (ScriptingInterfaceOfIThumbnailCreatorView.GetNumberOfPendingRequestsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIThumbnailCreatorView.GetNumberOfPendingRequestsDelegate));
			break;
		case 1146:
			ScriptingInterfaceOfIThumbnailCreatorView.call_IsMemoryClearedDelegate = (ScriptingInterfaceOfIThumbnailCreatorView.IsMemoryClearedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIThumbnailCreatorView.IsMemoryClearedDelegate));
			break;
		case 1147:
			ScriptingInterfaceOfIThumbnailCreatorView.call_RegisterEntityDelegate = (ScriptingInterfaceOfIThumbnailCreatorView.RegisterEntityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIThumbnailCreatorView.RegisterEntityDelegate));
			break;
		case 1148:
			ScriptingInterfaceOfIThumbnailCreatorView.call_RegisterEntityWithoutTextureDelegate = (ScriptingInterfaceOfIThumbnailCreatorView.RegisterEntityWithoutTextureDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIThumbnailCreatorView.RegisterEntityWithoutTextureDelegate));
			break;
		case 1149:
			ScriptingInterfaceOfIThumbnailCreatorView.call_RegisterSceneDelegate = (ScriptingInterfaceOfIThumbnailCreatorView.RegisterSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIThumbnailCreatorView.RegisterSceneDelegate));
			break;
		case 1150:
			ScriptingInterfaceOfITime.call_GetApplicationTimeDelegate = (ScriptingInterfaceOfITime.GetApplicationTimeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITime.GetApplicationTimeDelegate));
			break;
		case 1151:
			ScriptingInterfaceOfITwoDimensionView.call_AddCachedTextMeshDelegate = (ScriptingInterfaceOfITwoDimensionView.AddCachedTextMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITwoDimensionView.AddCachedTextMeshDelegate));
			break;
		case 1152:
			ScriptingInterfaceOfITwoDimensionView.call_AddNewMeshDelegate = (ScriptingInterfaceOfITwoDimensionView.AddNewMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITwoDimensionView.AddNewMeshDelegate));
			break;
		case 1153:
			ScriptingInterfaceOfITwoDimensionView.call_AddNewQuadMeshDelegate = (ScriptingInterfaceOfITwoDimensionView.AddNewQuadMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITwoDimensionView.AddNewQuadMeshDelegate));
			break;
		case 1154:
			ScriptingInterfaceOfITwoDimensionView.call_AddNewTextMeshDelegate = (ScriptingInterfaceOfITwoDimensionView.AddNewTextMeshDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITwoDimensionView.AddNewTextMeshDelegate));
			break;
		case 1155:
			ScriptingInterfaceOfITwoDimensionView.call_BeginFrameDelegate = (ScriptingInterfaceOfITwoDimensionView.BeginFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITwoDimensionView.BeginFrameDelegate));
			break;
		case 1156:
			ScriptingInterfaceOfITwoDimensionView.call_ClearDelegate = (ScriptingInterfaceOfITwoDimensionView.ClearDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITwoDimensionView.ClearDelegate));
			break;
		case 1157:
			ScriptingInterfaceOfITwoDimensionView.call_CreateTwoDimensionViewDelegate = (ScriptingInterfaceOfITwoDimensionView.CreateTwoDimensionViewDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITwoDimensionView.CreateTwoDimensionViewDelegate));
			break;
		case 1158:
			ScriptingInterfaceOfITwoDimensionView.call_EndFrameDelegate = (ScriptingInterfaceOfITwoDimensionView.EndFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfITwoDimensionView.EndFrameDelegate));
			break;
		case 1159:
			ScriptingInterfaceOfIUtil.call_AddCommandLineFunctionDelegate = (ScriptingInterfaceOfIUtil.AddCommandLineFunctionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.AddCommandLineFunctionDelegate));
			break;
		case 1160:
			ScriptingInterfaceOfIUtil.call_AddMainThreadPerformanceQueryDelegate = (ScriptingInterfaceOfIUtil.AddMainThreadPerformanceQueryDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.AddMainThreadPerformanceQueryDelegate));
			break;
		case 1161:
			ScriptingInterfaceOfIUtil.call_AddPerformanceReportTokenDelegate = (ScriptingInterfaceOfIUtil.AddPerformanceReportTokenDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.AddPerformanceReportTokenDelegate));
			break;
		case 1162:
			ScriptingInterfaceOfIUtil.call_AddSceneObjectReportDelegate = (ScriptingInterfaceOfIUtil.AddSceneObjectReportDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.AddSceneObjectReportDelegate));
			break;
		case 1163:
			ScriptingInterfaceOfIUtil.call_CheckIfAssetsAndSourcesAreSameDelegate = (ScriptingInterfaceOfIUtil.CheckIfAssetsAndSourcesAreSameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.CheckIfAssetsAndSourcesAreSameDelegate));
			break;
		case 1164:
			ScriptingInterfaceOfIUtil.call_CheckIfTerrainShaderHeaderGenerationFinishedDelegate = (ScriptingInterfaceOfIUtil.CheckIfTerrainShaderHeaderGenerationFinishedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.CheckIfTerrainShaderHeaderGenerationFinishedDelegate));
			break;
		case 1165:
			ScriptingInterfaceOfIUtil.call_CheckResourceModificationsDelegate = (ScriptingInterfaceOfIUtil.CheckResourceModificationsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.CheckResourceModificationsDelegate));
			break;
		case 1166:
			ScriptingInterfaceOfIUtil.call_CheckSceneForProblemsDelegate = (ScriptingInterfaceOfIUtil.CheckSceneForProblemsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.CheckSceneForProblemsDelegate));
			break;
		case 1167:
			ScriptingInterfaceOfIUtil.call_CheckShaderCompilationDelegate = (ScriptingInterfaceOfIUtil.CheckShaderCompilationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.CheckShaderCompilationDelegate));
			break;
		case 1168:
			ScriptingInterfaceOfIUtil.call_clear_decal_atlasDelegate = (ScriptingInterfaceOfIUtil.clear_decal_atlasDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.clear_decal_atlasDelegate));
			break;
		case 1169:
			ScriptingInterfaceOfIUtil.call_ClearOldResourcesAndObjectsDelegate = (ScriptingInterfaceOfIUtil.ClearOldResourcesAndObjectsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.ClearOldResourcesAndObjectsDelegate));
			break;
		case 1170:
			ScriptingInterfaceOfIUtil.call_ClearShaderMemoryDelegate = (ScriptingInterfaceOfIUtil.ClearShaderMemoryDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.ClearShaderMemoryDelegate));
			break;
		case 1171:
			ScriptingInterfaceOfIUtil.call_CommandLineArgumentExistsDelegate = (ScriptingInterfaceOfIUtil.CommandLineArgumentExistsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.CommandLineArgumentExistsDelegate));
			break;
		case 1172:
			ScriptingInterfaceOfIUtil.call_CompileAllShadersDelegate = (ScriptingInterfaceOfIUtil.CompileAllShadersDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.CompileAllShadersDelegate));
			break;
		case 1173:
			ScriptingInterfaceOfIUtil.call_CompileTerrainShadersDistDelegate = (ScriptingInterfaceOfIUtil.CompileTerrainShadersDistDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.CompileTerrainShadersDistDelegate));
			break;
		case 1174:
			ScriptingInterfaceOfIUtil.call_CreateSelectionInEditorDelegate = (ScriptingInterfaceOfIUtil.CreateSelectionInEditorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.CreateSelectionInEditorDelegate));
			break;
		case 1175:
			ScriptingInterfaceOfIUtil.call_DebugSetGlobalLoadingWindowStateDelegate = (ScriptingInterfaceOfIUtil.DebugSetGlobalLoadingWindowStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.DebugSetGlobalLoadingWindowStateDelegate));
			break;
		case 1176:
			ScriptingInterfaceOfIUtil.call_DeleteEntitiesInEditorSceneDelegate = (ScriptingInterfaceOfIUtil.DeleteEntitiesInEditorSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.DeleteEntitiesInEditorSceneDelegate));
			break;
		case 1177:
			ScriptingInterfaceOfIUtil.call_DetachWatchdogDelegate = (ScriptingInterfaceOfIUtil.DetachWatchdogDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.DetachWatchdogDelegate));
			break;
		case 1178:
			ScriptingInterfaceOfIUtil.call_DidAutomatedGIBakeFinishedDelegate = (ScriptingInterfaceOfIUtil.DidAutomatedGIBakeFinishedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.DidAutomatedGIBakeFinishedDelegate));
			break;
		case 1179:
			ScriptingInterfaceOfIUtil.call_DisableCoreGameDelegate = (ScriptingInterfaceOfIUtil.DisableCoreGameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.DisableCoreGameDelegate));
			break;
		case 1180:
			ScriptingInterfaceOfIUtil.call_DisableGlobalEditDataCacherDelegate = (ScriptingInterfaceOfIUtil.DisableGlobalEditDataCacherDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.DisableGlobalEditDataCacherDelegate));
			break;
		case 1181:
			ScriptingInterfaceOfIUtil.call_DisableGlobalLoadingWindowDelegate = (ScriptingInterfaceOfIUtil.DisableGlobalLoadingWindowDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.DisableGlobalLoadingWindowDelegate));
			break;
		case 1182:
			ScriptingInterfaceOfIUtil.call_DoDelayedexitDelegate = (ScriptingInterfaceOfIUtil.DoDelayedexitDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.DoDelayedexitDelegate));
			break;
		case 1183:
			ScriptingInterfaceOfIUtil.call_DoFullBakeAllLevelsAutomatedDelegate = (ScriptingInterfaceOfIUtil.DoFullBakeAllLevelsAutomatedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.DoFullBakeAllLevelsAutomatedDelegate));
			break;
		case 1184:
			ScriptingInterfaceOfIUtil.call_DoFullBakeSingleLevelAutomatedDelegate = (ScriptingInterfaceOfIUtil.DoFullBakeSingleLevelAutomatedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.DoFullBakeSingleLevelAutomatedDelegate));
			break;
		case 1185:
			ScriptingInterfaceOfIUtil.call_DoLightOnlyBakeAllLevelsAutomatedDelegate = (ScriptingInterfaceOfIUtil.DoLightOnlyBakeAllLevelsAutomatedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.DoLightOnlyBakeAllLevelsAutomatedDelegate));
			break;
		case 1186:
			ScriptingInterfaceOfIUtil.call_DoLightOnlyBakeSingleLevelAutomatedDelegate = (ScriptingInterfaceOfIUtil.DoLightOnlyBakeSingleLevelAutomatedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.DoLightOnlyBakeSingleLevelAutomatedDelegate));
			break;
		case 1187:
			ScriptingInterfaceOfIUtil.call_DumpGPUMemoryStatisticsDelegate = (ScriptingInterfaceOfIUtil.DumpGPUMemoryStatisticsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.DumpGPUMemoryStatisticsDelegate));
			break;
		case 1188:
			ScriptingInterfaceOfIUtil.call_EnableGlobalEditDataCacherDelegate = (ScriptingInterfaceOfIUtil.EnableGlobalEditDataCacherDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.EnableGlobalEditDataCacherDelegate));
			break;
		case 1189:
			ScriptingInterfaceOfIUtil.call_EnableGlobalLoadingWindowDelegate = (ScriptingInterfaceOfIUtil.EnableGlobalLoadingWindowDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.EnableGlobalLoadingWindowDelegate));
			break;
		case 1190:
			ScriptingInterfaceOfIUtil.call_EnableSingleGPUQueryPerFrameDelegate = (ScriptingInterfaceOfIUtil.EnableSingleGPUQueryPerFrameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.EnableSingleGPUQueryPerFrameDelegate));
			break;
		case 1191:
			ScriptingInterfaceOfIUtil.call_ExecuteCommandLineCommandDelegate = (ScriptingInterfaceOfIUtil.ExecuteCommandLineCommandDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.ExecuteCommandLineCommandDelegate));
			break;
		case 1192:
			ScriptingInterfaceOfIUtil.call_ExitProcessDelegate = (ScriptingInterfaceOfIUtil.ExitProcessDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.ExitProcessDelegate));
			break;
		case 1193:
			ScriptingInterfaceOfIUtil.call_ExportNavMeshFaceMarksDelegate = (ScriptingInterfaceOfIUtil.ExportNavMeshFaceMarksDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.ExportNavMeshFaceMarksDelegate));
			break;
		case 1194:
			ScriptingInterfaceOfIUtil.call_FindMeshesWithoutLodsDelegate = (ScriptingInterfaceOfIUtil.FindMeshesWithoutLodsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.FindMeshesWithoutLodsDelegate));
			break;
		case 1195:
			ScriptingInterfaceOfIUtil.call_FlushManagedObjectsMemoryDelegate = (ScriptingInterfaceOfIUtil.FlushManagedObjectsMemoryDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.FlushManagedObjectsMemoryDelegate));
			break;
		case 1196:
			ScriptingInterfaceOfIUtil.call_GatherCoreGameReferencesDelegate = (ScriptingInterfaceOfIUtil.GatherCoreGameReferencesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GatherCoreGameReferencesDelegate));
			break;
		case 1197:
			ScriptingInterfaceOfIUtil.call_GenerateTerrainShaderHeadersDelegate = (ScriptingInterfaceOfIUtil.GenerateTerrainShaderHeadersDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GenerateTerrainShaderHeadersDelegate));
			break;
		case 1198:
			ScriptingInterfaceOfIUtil.call_GetApplicationMemoryDelegate = (ScriptingInterfaceOfIUtil.GetApplicationMemoryDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetApplicationMemoryDelegate));
			break;
		case 1199:
			ScriptingInterfaceOfIUtil.call_GetApplicationMemoryStatisticsDelegate = (ScriptingInterfaceOfIUtil.GetApplicationMemoryStatisticsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetApplicationMemoryStatisticsDelegate));
			break;
		case 1200:
			ScriptingInterfaceOfIUtil.call_GetApplicationNameDelegate = (ScriptingInterfaceOfIUtil.GetApplicationNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetApplicationNameDelegate));
			break;
		case 1201:
			ScriptingInterfaceOfIUtil.call_GetAttachmentsPathDelegate = (ScriptingInterfaceOfIUtil.GetAttachmentsPathDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetAttachmentsPathDelegate));
			break;
		case 1202:
			ScriptingInterfaceOfIUtil.call_GetBaseDirectoryDelegate = (ScriptingInterfaceOfIUtil.GetBaseDirectoryDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetBaseDirectoryDelegate));
			break;
		case 1203:
			ScriptingInterfaceOfIUtil.call_GetBenchmarkStatusDelegate = (ScriptingInterfaceOfIUtil.GetBenchmarkStatusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetBenchmarkStatusDelegate));
			break;
		case 1204:
			ScriptingInterfaceOfIUtil.call_GetBuildNumberDelegate = (ScriptingInterfaceOfIUtil.GetBuildNumberDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetBuildNumberDelegate));
			break;
		case 1205:
			ScriptingInterfaceOfIUtil.call_GetConsoleHostMachineDelegate = (ScriptingInterfaceOfIUtil.GetConsoleHostMachineDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetConsoleHostMachineDelegate));
			break;
		case 1206:
			ScriptingInterfaceOfIUtil.call_GetCoreGameStateDelegate = (ScriptingInterfaceOfIUtil.GetCoreGameStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetCoreGameStateDelegate));
			break;
		case 1207:
			ScriptingInterfaceOfIUtil.call_GetCurrentCpuMemoryUsageDelegate = (ScriptingInterfaceOfIUtil.GetCurrentCpuMemoryUsageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetCurrentCpuMemoryUsageDelegate));
			break;
		case 1208:
			ScriptingInterfaceOfIUtil.call_GetCurrentEstimatedGPUMemoryCostMBDelegate = (ScriptingInterfaceOfIUtil.GetCurrentEstimatedGPUMemoryCostMBDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetCurrentEstimatedGPUMemoryCostMBDelegate));
			break;
		case 1209:
			ScriptingInterfaceOfIUtil.call_GetCurrentProcessIDDelegate = (ScriptingInterfaceOfIUtil.GetCurrentProcessIDDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetCurrentProcessIDDelegate));
			break;
		case 1210:
			ScriptingInterfaceOfIUtil.call_GetCurrentThreadIdDelegate = (ScriptingInterfaceOfIUtil.GetCurrentThreadIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetCurrentThreadIdDelegate));
			break;
		case 1211:
			ScriptingInterfaceOfIUtil.call_GetDeltaTimeDelegate = (ScriptingInterfaceOfIUtil.GetDeltaTimeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetDeltaTimeDelegate));
			break;
		case 1212:
			ScriptingInterfaceOfIUtil.call_GetDetailedGPUBufferMemoryStatsDelegate = (ScriptingInterfaceOfIUtil.GetDetailedGPUBufferMemoryStatsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetDetailedGPUBufferMemoryStatsDelegate));
			break;
		case 1213:
			ScriptingInterfaceOfIUtil.call_GetDetailedXBOXMemoryInfoDelegate = (ScriptingInterfaceOfIUtil.GetDetailedXBOXMemoryInfoDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetDetailedXBOXMemoryInfoDelegate));
			break;
		case 1214:
			ScriptingInterfaceOfIUtil.call_GetEditorSelectedEntitiesDelegate = (ScriptingInterfaceOfIUtil.GetEditorSelectedEntitiesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetEditorSelectedEntitiesDelegate));
			break;
		case 1215:
			ScriptingInterfaceOfIUtil.call_GetEditorSelectedEntityCountDelegate = (ScriptingInterfaceOfIUtil.GetEditorSelectedEntityCountDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetEditorSelectedEntityCountDelegate));
			break;
		case 1216:
			ScriptingInterfaceOfIUtil.call_GetEngineFrameNoDelegate = (ScriptingInterfaceOfIUtil.GetEngineFrameNoDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetEngineFrameNoDelegate));
			break;
		case 1217:
			ScriptingInterfaceOfIUtil.call_GetEntitiesOfSelectionSetDelegate = (ScriptingInterfaceOfIUtil.GetEntitiesOfSelectionSetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetEntitiesOfSelectionSetDelegate));
			break;
		case 1218:
			ScriptingInterfaceOfIUtil.call_GetEntityCountOfSelectionSetDelegate = (ScriptingInterfaceOfIUtil.GetEntityCountOfSelectionSetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetEntityCountOfSelectionSetDelegate));
			break;
		case 1219:
			ScriptingInterfaceOfIUtil.call_GetExecutableWorkingDirectoryDelegate = (ScriptingInterfaceOfIUtil.GetExecutableWorkingDirectoryDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetExecutableWorkingDirectoryDelegate));
			break;
		case 1220:
			ScriptingInterfaceOfIUtil.call_GetFpsDelegate = (ScriptingInterfaceOfIUtil.GetFpsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetFpsDelegate));
			break;
		case 1221:
			ScriptingInterfaceOfIUtil.call_GetFrameLimiterWithSleepDelegate = (ScriptingInterfaceOfIUtil.GetFrameLimiterWithSleepDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetFrameLimiterWithSleepDelegate));
			break;
		case 1222:
			ScriptingInterfaceOfIUtil.call_GetFullCommandLineStringDelegate = (ScriptingInterfaceOfIUtil.GetFullCommandLineStringDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetFullCommandLineStringDelegate));
			break;
		case 1223:
			ScriptingInterfaceOfIUtil.call_GetFullFilePathOfSceneDelegate = (ScriptingInterfaceOfIUtil.GetFullFilePathOfSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetFullFilePathOfSceneDelegate));
			break;
		case 1224:
			ScriptingInterfaceOfIUtil.call_GetFullModulePathDelegate = (ScriptingInterfaceOfIUtil.GetFullModulePathDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetFullModulePathDelegate));
			break;
		case 1225:
			ScriptingInterfaceOfIUtil.call_GetFullModulePathsDelegate = (ScriptingInterfaceOfIUtil.GetFullModulePathsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetFullModulePathsDelegate));
			break;
		case 1226:
			ScriptingInterfaceOfIUtil.call_GetGPUMemoryMBDelegate = (ScriptingInterfaceOfIUtil.GetGPUMemoryMBDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetGPUMemoryMBDelegate));
			break;
		case 1227:
			ScriptingInterfaceOfIUtil.call_GetGpuMemoryOfAllocationGroupDelegate = (ScriptingInterfaceOfIUtil.GetGpuMemoryOfAllocationGroupDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetGpuMemoryOfAllocationGroupDelegate));
			break;
		case 1228:
			ScriptingInterfaceOfIUtil.call_GetGPUMemoryStatsDelegate = (ScriptingInterfaceOfIUtil.GetGPUMemoryStatsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetGPUMemoryStatsDelegate));
			break;
		case 1229:
			ScriptingInterfaceOfIUtil.call_GetLocalOutputPathDelegate = (ScriptingInterfaceOfIUtil.GetLocalOutputPathDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetLocalOutputPathDelegate));
			break;
		case 1230:
			ScriptingInterfaceOfIUtil.call_GetMainFpsDelegate = (ScriptingInterfaceOfIUtil.GetMainFpsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetMainFpsDelegate));
			break;
		case 1231:
			ScriptingInterfaceOfIUtil.call_GetMainThreadIdDelegate = (ScriptingInterfaceOfIUtil.GetMainThreadIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetMainThreadIdDelegate));
			break;
		case 1232:
			ScriptingInterfaceOfIUtil.call_GetMemoryUsageOfCategoryDelegate = (ScriptingInterfaceOfIUtil.GetMemoryUsageOfCategoryDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetMemoryUsageOfCategoryDelegate));
			break;
		case 1233:
			ScriptingInterfaceOfIUtil.call_GetModulesCodeDelegate = (ScriptingInterfaceOfIUtil.GetModulesCodeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetModulesCodeDelegate));
			break;
		case 1234:
			ScriptingInterfaceOfIUtil.call_GetNativeMemoryStatisticsDelegate = (ScriptingInterfaceOfIUtil.GetNativeMemoryStatisticsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetNativeMemoryStatisticsDelegate));
			break;
		case 1235:
			ScriptingInterfaceOfIUtil.call_GetNumberOfShaderCompilationsInProgressDelegate = (ScriptingInterfaceOfIUtil.GetNumberOfShaderCompilationsInProgressDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetNumberOfShaderCompilationsInProgressDelegate));
			break;
		case 1236:
			ScriptingInterfaceOfIUtil.call_GetPCInfoDelegate = (ScriptingInterfaceOfIUtil.GetPCInfoDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetPCInfoDelegate));
			break;
		case 1237:
			ScriptingInterfaceOfIUtil.call_GetRendererFpsDelegate = (ScriptingInterfaceOfIUtil.GetRendererFpsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetRendererFpsDelegate));
			break;
		case 1238:
			ScriptingInterfaceOfIUtil.call_GetReturnCodeDelegate = (ScriptingInterfaceOfIUtil.GetReturnCodeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetReturnCodeDelegate));
			break;
		case 1239:
			ScriptingInterfaceOfIUtil.call_GetSingleModuleScenesOfModuleDelegate = (ScriptingInterfaceOfIUtil.GetSingleModuleScenesOfModuleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetSingleModuleScenesOfModuleDelegate));
			break;
		case 1240:
			ScriptingInterfaceOfIUtil.call_GetSteamAppIdDelegate = (ScriptingInterfaceOfIUtil.GetSteamAppIdDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetSteamAppIdDelegate));
			break;
		case 1241:
			ScriptingInterfaceOfIUtil.call_GetSystemLanguageDelegate = (ScriptingInterfaceOfIUtil.GetSystemLanguageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetSystemLanguageDelegate));
			break;
		case 1242:
			ScriptingInterfaceOfIUtil.call_GetVertexBufferChunkSystemMemoryUsageDelegate = (ScriptingInterfaceOfIUtil.GetVertexBufferChunkSystemMemoryUsageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetVertexBufferChunkSystemMemoryUsageDelegate));
			break;
		case 1243:
			ScriptingInterfaceOfIUtil.call_GetVisualTestsTestFilesPathDelegate = (ScriptingInterfaceOfIUtil.GetVisualTestsTestFilesPathDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetVisualTestsTestFilesPathDelegate));
			break;
		case 1244:
			ScriptingInterfaceOfIUtil.call_GetVisualTestsValidatePathDelegate = (ScriptingInterfaceOfIUtil.GetVisualTestsValidatePathDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.GetVisualTestsValidatePathDelegate));
			break;
		case 1245:
			ScriptingInterfaceOfIUtil.call_IsBenchmarkQuitedDelegate = (ScriptingInterfaceOfIUtil.IsBenchmarkQuitedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.IsBenchmarkQuitedDelegate));
			break;
		case 1246:
			ScriptingInterfaceOfIUtil.call_IsDetailedSoundLogOnDelegate = (ScriptingInterfaceOfIUtil.IsDetailedSoundLogOnDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.IsDetailedSoundLogOnDelegate));
			break;
		case 1247:
			ScriptingInterfaceOfIUtil.call_IsEditModeEnabledDelegate = (ScriptingInterfaceOfIUtil.IsEditModeEnabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.IsEditModeEnabledDelegate));
			break;
		case 1248:
			ScriptingInterfaceOfIUtil.call_IsSceneReportFinishedDelegate = (ScriptingInterfaceOfIUtil.IsSceneReportFinishedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.IsSceneReportFinishedDelegate));
			break;
		case 1249:
			ScriptingInterfaceOfIUtil.call_LoadSkyBoxesDelegate = (ScriptingInterfaceOfIUtil.LoadSkyBoxesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.LoadSkyBoxesDelegate));
			break;
		case 1250:
			ScriptingInterfaceOfIUtil.call_LoadVirtualTextureTilesetDelegate = (ScriptingInterfaceOfIUtil.LoadVirtualTextureTilesetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.LoadVirtualTextureTilesetDelegate));
			break;
		case 1251:
			ScriptingInterfaceOfIUtil.call_ManagedParallelForDelegate = (ScriptingInterfaceOfIUtil.ManagedParallelForDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.ManagedParallelForDelegate));
			break;
		case 1252:
			ScriptingInterfaceOfIUtil.call_ManagedParallelForWithDtDelegate = (ScriptingInterfaceOfIUtil.ManagedParallelForWithDtDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.ManagedParallelForWithDtDelegate));
			break;
		case 1253:
			ScriptingInterfaceOfIUtil.call_OnLoadingWindowDisabledDelegate = (ScriptingInterfaceOfIUtil.OnLoadingWindowDisabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.OnLoadingWindowDisabledDelegate));
			break;
		case 1254:
			ScriptingInterfaceOfIUtil.call_OnLoadingWindowEnabledDelegate = (ScriptingInterfaceOfIUtil.OnLoadingWindowEnabledDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.OnLoadingWindowEnabledDelegate));
			break;
		case 1255:
			ScriptingInterfaceOfIUtil.call_OpenOnscreenKeyboardDelegate = (ScriptingInterfaceOfIUtil.OpenOnscreenKeyboardDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.OpenOnscreenKeyboardDelegate));
			break;
		case 1256:
			ScriptingInterfaceOfIUtil.call_OutputBenchmarkValuesToPerformanceReporterDelegate = (ScriptingInterfaceOfIUtil.OutputBenchmarkValuesToPerformanceReporterDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.OutputBenchmarkValuesToPerformanceReporterDelegate));
			break;
		case 1257:
			ScriptingInterfaceOfIUtil.call_OutputPerformanceReportsDelegate = (ScriptingInterfaceOfIUtil.OutputPerformanceReportsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.OutputPerformanceReportsDelegate));
			break;
		case 1258:
			ScriptingInterfaceOfIUtil.call_PairSceneNameToModuleNameDelegate = (ScriptingInterfaceOfIUtil.PairSceneNameToModuleNameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.PairSceneNameToModuleNameDelegate));
			break;
		case 1259:
			ScriptingInterfaceOfIUtil.call_ProcessWindowTitleDelegate = (ScriptingInterfaceOfIUtil.ProcessWindowTitleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.ProcessWindowTitleDelegate));
			break;
		case 1260:
			ScriptingInterfaceOfIUtil.call_QuitGameDelegate = (ScriptingInterfaceOfIUtil.QuitGameDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.QuitGameDelegate));
			break;
		case 1261:
			ScriptingInterfaceOfIUtil.call_RegisterGPUAllocationGroupDelegate = (ScriptingInterfaceOfIUtil.RegisterGPUAllocationGroupDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.RegisterGPUAllocationGroupDelegate));
			break;
		case 1262:
			ScriptingInterfaceOfIUtil.call_RegisterMeshForGPUMorphDelegate = (ScriptingInterfaceOfIUtil.RegisterMeshForGPUMorphDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.RegisterMeshForGPUMorphDelegate));
			break;
		case 1263:
			ScriptingInterfaceOfIUtil.call_SaveDataAsTextureDelegate = (ScriptingInterfaceOfIUtil.SaveDataAsTextureDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SaveDataAsTextureDelegate));
			break;
		case 1264:
			ScriptingInterfaceOfIUtil.call_SelectEntitiesDelegate = (ScriptingInterfaceOfIUtil.SelectEntitiesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SelectEntitiesDelegate));
			break;
		case 1265:
			ScriptingInterfaceOfIUtil.call_SetAllocationAlwaysValidSceneDelegate = (ScriptingInterfaceOfIUtil.SetAllocationAlwaysValidSceneDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetAllocationAlwaysValidSceneDelegate));
			break;
		case 1266:
			ScriptingInterfaceOfIUtil.call_SetAssertionAtShaderCompileDelegate = (ScriptingInterfaceOfIUtil.SetAssertionAtShaderCompileDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetAssertionAtShaderCompileDelegate));
			break;
		case 1267:
			ScriptingInterfaceOfIUtil.call_SetAssertionsAndWarningsSetExitCodeDelegate = (ScriptingInterfaceOfIUtil.SetAssertionsAndWarningsSetExitCodeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetAssertionsAndWarningsSetExitCodeDelegate));
			break;
		case 1268:
			ScriptingInterfaceOfIUtil.call_SetBenchmarkStatusDelegate = (ScriptingInterfaceOfIUtil.SetBenchmarkStatusDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetBenchmarkStatusDelegate));
			break;
		case 1269:
			ScriptingInterfaceOfIUtil.call_SetCoreGameStateDelegate = (ScriptingInterfaceOfIUtil.SetCoreGameStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetCoreGameStateDelegate));
			break;
		case 1270:
			ScriptingInterfaceOfIUtil.call_SetCrashOnAssertsDelegate = (ScriptingInterfaceOfIUtil.SetCrashOnAssertsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetCrashOnAssertsDelegate));
			break;
		case 1271:
			ScriptingInterfaceOfIUtil.call_SetCrashOnWarningsDelegate = (ScriptingInterfaceOfIUtil.SetCrashOnWarningsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetCrashOnWarningsDelegate));
			break;
		case 1272:
			ScriptingInterfaceOfIUtil.call_SetCrashReportCustomStackDelegate = (ScriptingInterfaceOfIUtil.SetCrashReportCustomStackDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetCrashReportCustomStackDelegate));
			break;
		case 1273:
			ScriptingInterfaceOfIUtil.call_SetCrashReportCustomStringDelegate = (ScriptingInterfaceOfIUtil.SetCrashReportCustomStringDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetCrashReportCustomStringDelegate));
			break;
		case 1274:
			ScriptingInterfaceOfIUtil.call_SetDisableDumpGenerationDelegate = (ScriptingInterfaceOfIUtil.SetDisableDumpGenerationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetDisableDumpGenerationDelegate));
			break;
		case 1275:
			ScriptingInterfaceOfIUtil.call_SetDumpFolderPathDelegate = (ScriptingInterfaceOfIUtil.SetDumpFolderPathDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetDumpFolderPathDelegate));
			break;
		case 1276:
			ScriptingInterfaceOfIUtil.call_SetFixedDtDelegate = (ScriptingInterfaceOfIUtil.SetFixedDtDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetFixedDtDelegate));
			break;
		case 1277:
			ScriptingInterfaceOfIUtil.call_SetForceDrawEntityIDDelegate = (ScriptingInterfaceOfIUtil.SetForceDrawEntityIDDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetForceDrawEntityIDDelegate));
			break;
		case 1278:
			ScriptingInterfaceOfIUtil.call_SetForceVsyncDelegate = (ScriptingInterfaceOfIUtil.SetForceVsyncDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetForceVsyncDelegate));
			break;
		case 1279:
			ScriptingInterfaceOfIUtil.call_SetFrameLimiterWithSleepDelegate = (ScriptingInterfaceOfIUtil.SetFrameLimiterWithSleepDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetFrameLimiterWithSleepDelegate));
			break;
		case 1280:
			ScriptingInterfaceOfIUtil.call_SetGraphicsPresetDelegate = (ScriptingInterfaceOfIUtil.SetGraphicsPresetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetGraphicsPresetDelegate));
			break;
		case 1281:
			ScriptingInterfaceOfIUtil.call_SetLoadingScreenPercentageDelegate = (ScriptingInterfaceOfIUtil.SetLoadingScreenPercentageDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetLoadingScreenPercentageDelegate));
			break;
		case 1282:
			ScriptingInterfaceOfIUtil.call_SetMessageLineRenderingStateDelegate = (ScriptingInterfaceOfIUtil.SetMessageLineRenderingStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetMessageLineRenderingStateDelegate));
			break;
		case 1283:
			ScriptingInterfaceOfIUtil.call_SetPrintCallstackAtCrahsesDelegate = (ScriptingInterfaceOfIUtil.SetPrintCallstackAtCrahsesDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetPrintCallstackAtCrahsesDelegate));
			break;
		case 1284:
			ScriptingInterfaceOfIUtil.call_SetRenderAgentsDelegate = (ScriptingInterfaceOfIUtil.SetRenderAgentsDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetRenderAgentsDelegate));
			break;
		case 1285:
			ScriptingInterfaceOfIUtil.call_SetRenderModeDelegate = (ScriptingInterfaceOfIUtil.SetRenderModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetRenderModeDelegate));
			break;
		case 1286:
			ScriptingInterfaceOfIUtil.call_SetReportModeDelegate = (ScriptingInterfaceOfIUtil.SetReportModeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetReportModeDelegate));
			break;
		case 1287:
			ScriptingInterfaceOfIUtil.call_SetScreenTextRenderingStateDelegate = (ScriptingInterfaceOfIUtil.SetScreenTextRenderingStateDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetScreenTextRenderingStateDelegate));
			break;
		case 1288:
			ScriptingInterfaceOfIUtil.call_SetWatchdogValueDelegate = (ScriptingInterfaceOfIUtil.SetWatchdogValueDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetWatchdogValueDelegate));
			break;
		case 1289:
			ScriptingInterfaceOfIUtil.call_SetWindowTitleDelegate = (ScriptingInterfaceOfIUtil.SetWindowTitleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.SetWindowTitleDelegate));
			break;
		case 1290:
			ScriptingInterfaceOfIUtil.call_StartScenePerformanceReportDelegate = (ScriptingInterfaceOfIUtil.StartScenePerformanceReportDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.StartScenePerformanceReportDelegate));
			break;
		case 1291:
			ScriptingInterfaceOfIUtil.call_TakeScreenshotFromPlatformPathDelegate = (ScriptingInterfaceOfIUtil.TakeScreenshotFromPlatformPathDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.TakeScreenshotFromPlatformPathDelegate));
			break;
		case 1292:
			ScriptingInterfaceOfIUtil.call_TakeScreenshotFromStringPathDelegate = (ScriptingInterfaceOfIUtil.TakeScreenshotFromStringPathDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.TakeScreenshotFromStringPathDelegate));
			break;
		case 1293:
			ScriptingInterfaceOfIUtil.call_TakeSSFromTopDelegate = (ScriptingInterfaceOfIUtil.TakeSSFromTopDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.TakeSSFromTopDelegate));
			break;
		case 1294:
			ScriptingInterfaceOfIUtil.call_ToggleRenderDelegate = (ScriptingInterfaceOfIUtil.ToggleRenderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIUtil.ToggleRenderDelegate));
			break;
		case 1295:
			ScriptingInterfaceOfIVideoPlayerView.call_CreateVideoPlayerViewDelegate = (ScriptingInterfaceOfIVideoPlayerView.CreateVideoPlayerViewDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIVideoPlayerView.CreateVideoPlayerViewDelegate));
			break;
		case 1296:
			ScriptingInterfaceOfIVideoPlayerView.call_FinalizeDelegate = (ScriptingInterfaceOfIVideoPlayerView.FinalizeDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIVideoPlayerView.FinalizeDelegate));
			break;
		case 1297:
			ScriptingInterfaceOfIVideoPlayerView.call_IsVideoFinishedDelegate = (ScriptingInterfaceOfIVideoPlayerView.IsVideoFinishedDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIVideoPlayerView.IsVideoFinishedDelegate));
			break;
		case 1298:
			ScriptingInterfaceOfIVideoPlayerView.call_PlayVideoDelegate = (ScriptingInterfaceOfIVideoPlayerView.PlayVideoDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIVideoPlayerView.PlayVideoDelegate));
			break;
		case 1299:
			ScriptingInterfaceOfIVideoPlayerView.call_StopVideoDelegate = (ScriptingInterfaceOfIVideoPlayerView.StopVideoDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIVideoPlayerView.StopVideoDelegate));
			break;
		case 1300:
			ScriptingInterfaceOfIView.call_SetAutoDepthTargetCreationDelegate = (ScriptingInterfaceOfIView.SetAutoDepthTargetCreationDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetAutoDepthTargetCreationDelegate));
			break;
		case 1301:
			ScriptingInterfaceOfIView.call_SetClearColorDelegate = (ScriptingInterfaceOfIView.SetClearColorDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetClearColorDelegate));
			break;
		case 1302:
			ScriptingInterfaceOfIView.call_SetDebugRenderFunctionalityDelegate = (ScriptingInterfaceOfIView.SetDebugRenderFunctionalityDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetDebugRenderFunctionalityDelegate));
			break;
		case 1303:
			ScriptingInterfaceOfIView.call_SetDepthTargetDelegate = (ScriptingInterfaceOfIView.SetDepthTargetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetDepthTargetDelegate));
			break;
		case 1304:
			ScriptingInterfaceOfIView.call_SetEnableDelegate = (ScriptingInterfaceOfIView.SetEnableDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetEnableDelegate));
			break;
		case 1305:
			ScriptingInterfaceOfIView.call_SetFileNameToSaveResultDelegate = (ScriptingInterfaceOfIView.SetFileNameToSaveResultDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetFileNameToSaveResultDelegate));
			break;
		case 1306:
			ScriptingInterfaceOfIView.call_SetFilePathToSaveResultDelegate = (ScriptingInterfaceOfIView.SetFilePathToSaveResultDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetFilePathToSaveResultDelegate));
			break;
		case 1307:
			ScriptingInterfaceOfIView.call_SetFileTypeToSaveDelegate = (ScriptingInterfaceOfIView.SetFileTypeToSaveDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetFileTypeToSaveDelegate));
			break;
		case 1308:
			ScriptingInterfaceOfIView.call_SetOffsetDelegate = (ScriptingInterfaceOfIView.SetOffsetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetOffsetDelegate));
			break;
		case 1309:
			ScriptingInterfaceOfIView.call_SetRenderOnDemandDelegate = (ScriptingInterfaceOfIView.SetRenderOnDemandDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetRenderOnDemandDelegate));
			break;
		case 1310:
			ScriptingInterfaceOfIView.call_SetRenderOptionDelegate = (ScriptingInterfaceOfIView.SetRenderOptionDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetRenderOptionDelegate));
			break;
		case 1311:
			ScriptingInterfaceOfIView.call_SetRenderOrderDelegate = (ScriptingInterfaceOfIView.SetRenderOrderDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetRenderOrderDelegate));
			break;
		case 1312:
			ScriptingInterfaceOfIView.call_SetRenderTargetDelegate = (ScriptingInterfaceOfIView.SetRenderTargetDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetRenderTargetDelegate));
			break;
		case 1313:
			ScriptingInterfaceOfIView.call_SetSaveFinalResultToDiskDelegate = (ScriptingInterfaceOfIView.SetSaveFinalResultToDiskDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetSaveFinalResultToDiskDelegate));
			break;
		case 1314:
			ScriptingInterfaceOfIView.call_SetScaleDelegate = (ScriptingInterfaceOfIView.SetScaleDelegate)Marshal.GetDelegateForFunctionPointer(pointer, typeof(ScriptingInterfaceOfIView.SetScaleDelegate));
			break;
		}
	}
}
