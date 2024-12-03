using System;
using System.Runtime.InteropServices;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;

namespace ManagedCallbacks;

internal static class EngineCallbacksGenerated
{
	internal delegate UIntPtr CrashInformationCollector_CollectInformation_delegate();

	internal delegate UIntPtr EngineController_GetApplicationPlatformName_delegate();

	internal delegate UIntPtr EngineController_GetModulesVersionStr_delegate();

	internal delegate UIntPtr EngineController_GetVersionStr_delegate();

	internal delegate void EngineController_Initialize_delegate();

	internal delegate void EngineController_OnConfigChange_delegate();

	internal delegate void EngineController_OnConstrainedStateChange_delegate([MarshalAs(UnmanagedType.U1)] bool isConstrained);

	internal delegate void EngineController_OnControllerDisconnection_delegate();

	internal delegate void EngineManaged_CheckSharedStructureSizes_delegate();

	internal delegate void EngineManaged_EngineApiMethodInterfaceInitializer_delegate(int id, IntPtr pointer);

	internal delegate void EngineManaged_FillEngineApiPointers_delegate();

	internal delegate void EngineScreenManager_InitializeLastPressedKeys_delegate(NativeObjectPointer lastKeysPressed);

	internal delegate void EngineScreenManager_LateTick_delegate(float dt);

	internal delegate void EngineScreenManager_OnGameWindowFocusChange_delegate([MarshalAs(UnmanagedType.U1)] bool focusGained);

	internal delegate void EngineScreenManager_OnOnscreenKeyboardCanceled_delegate();

	internal delegate void EngineScreenManager_OnOnscreenKeyboardDone_delegate(IntPtr inputText);

	internal delegate void EngineScreenManager_PreTick_delegate(float dt);

	internal delegate void EngineScreenManager_Tick_delegate(float dt);

	internal delegate void EngineScreenManager_Update_delegate();

	internal delegate void ManagedExtensions_CollectCommandLineFunctions_delegate();

	internal delegate void ManagedExtensions_CopyObjectFieldsFrom_delegate(int dst, int src, IntPtr className, int callFieldChangeEventAsInteger);

	internal delegate int ManagedExtensions_CreateScriptComponentInstance_delegate(IntPtr className, NativeObjectPointer entity, NativeObjectPointer managedScriptComponent);

	internal delegate void ManagedExtensions_ForceGarbageCollect_delegate();

	[return: MarshalAs(UnmanagedType.U1)]
	internal delegate bool ManagedExtensions_GetEditorVisibilityOfField_delegate(IntPtr className, IntPtr fieldName);

	internal delegate void ManagedExtensions_GetObjectField_delegate(int managedObject, ref ScriptComponentFieldHolder scriptComponentFieldHolder, IntPtr fieldName, int type);

	internal delegate UIntPtr ManagedExtensions_GetScriptComponentClassNames_delegate();

	internal delegate int ManagedExtensions_GetTypeOfField_delegate(IntPtr className, IntPtr fieldName);

	internal delegate void ManagedExtensions_SetObjectField_delegate(int managedObject, IntPtr fieldName, ref ScriptComponentFieldHolder scriptComponentHolder, int type, int callFieldChangeEventAsInteger);

	internal delegate int ManagedScriptHolder_CreateManagedScriptHolder_delegate();

	internal delegate int ManagedScriptHolder_GetNumberOfScripts_delegate(int thisPointer);

	internal delegate void ManagedScriptHolder_RemoveScriptComponentFromAllTickLists_delegate(int thisPointer, int sc);

	internal delegate void ManagedScriptHolder_SetScriptComponentHolder_delegate(int thisPointer, int sc);

	internal delegate void ManagedScriptHolder_TickComponents_delegate(int thisPointer, float dt);

	internal delegate void ManagedScriptHolder_TickComponentsEditor_delegate(int thisPointer, float dt);

	internal delegate void MessageManagerBase_PostMessageLine_delegate(int thisPointer, IntPtr text, uint color);

	internal delegate void MessageManagerBase_PostMessageLineFormatted_delegate(int thisPointer, IntPtr text, uint color);

	internal delegate void MessageManagerBase_PostSuccessLine_delegate(int thisPointer, IntPtr text);

	internal delegate void MessageManagerBase_PostWarningLine_delegate(int thisPointer, IntPtr text);

	internal delegate void NativeParallelDriver_ParalelForLoopBodyCaller_delegate(long loopBodyKey, int localStartIndex, int localEndIndex);

	internal delegate void NativeParallelDriver_ParalelForLoopBodyWithDtCaller_delegate(long loopBodyKey, int localStartIndex, int localEndIndex);

	internal delegate int RenderTargetComponent_CreateRenderTargetComponent_delegate(NativeObjectPointer renderTarget);

	internal delegate void RenderTargetComponent_OnPaintNeeded_delegate(int thisPointer);

	[return: MarshalAs(UnmanagedType.U1)]
	internal delegate bool SceneProblemChecker_OnCheckForSceneProblems_delegate(NativeObjectPointer scene);

	internal delegate void ScriptComponentBehavior_AddScriptComponentToTick_delegate(int thisPointer);

	internal delegate void ScriptComponentBehavior_DeregisterAsPrefabScriptComponent_delegate(int thisPointer);

	internal delegate void ScriptComponentBehavior_DeregisterAsUndoStackScriptComponent_delegate(int thisPointer);

	[return: MarshalAs(UnmanagedType.U1)]
	internal delegate bool ScriptComponentBehavior_DisablesOroCreation_delegate(int thisPointer);

	internal delegate int ScriptComponentBehavior_GetEditableFields_delegate(IntPtr className);

	internal delegate void ScriptComponentBehavior_HandleOnRemoved_delegate(int thisPointer, int removeReason);

	[return: MarshalAs(UnmanagedType.U1)]
	internal delegate bool ScriptComponentBehavior_IsOnlyVisual_delegate(int thisPointer);

	[return: MarshalAs(UnmanagedType.U1)]
	internal delegate bool ScriptComponentBehavior_MovesEntity_delegate(int thisPointer);

	[return: MarshalAs(UnmanagedType.U1)]
	internal delegate bool ScriptComponentBehavior_OnCheckForProblems_delegate(int thisPointer);

	internal delegate void ScriptComponentBehavior_OnEditModeVisibilityChanged_delegate(int thisPointer, [MarshalAs(UnmanagedType.U1)] bool currentVisibility);

	internal delegate void ScriptComponentBehavior_OnEditorInit_delegate(int thisPointer);

	internal delegate void ScriptComponentBehavior_OnEditorTick_delegate(int thisPointer, float dt);

	internal delegate void ScriptComponentBehavior_OnEditorValidate_delegate(int thisPointer);

	internal delegate void ScriptComponentBehavior_OnEditorVariableChanged_delegate(int thisPointer, IntPtr variableName);

	internal delegate void ScriptComponentBehavior_OnInit_delegate(int thisPointer);

	internal delegate void ScriptComponentBehavior_OnPhysicsCollision_delegate(int thisPointer, ref PhysicsContact contact);

	internal delegate void ScriptComponentBehavior_OnPreInit_delegate(int thisPointer);

	internal delegate void ScriptComponentBehavior_OnSceneSave_delegate(int thisPointer, IntPtr saveFolder);

	internal delegate void ScriptComponentBehavior_RegisterAsPrefabScriptComponent_delegate(int thisPointer);

	internal delegate void ScriptComponentBehavior_RegisterAsUndoStackScriptComponent_delegate(int thisPointer);

	internal delegate void ScriptComponentBehavior_SetScene_delegate(int thisPointer, NativeObjectPointer scene);

	internal delegate void ThumbnailCreatorView_OnThumbnailRenderComplete_delegate(IntPtr renderId, NativeObjectPointer renderTarget);

	internal static Delegate[] Delegates { get; private set; }

	public static void Initialize()
	{
		Delegates = new Delegate[65];
		Delegates[0] = new CrashInformationCollector_CollectInformation_delegate(CrashInformationCollector_CollectInformation);
		Delegates[1] = new EngineController_GetApplicationPlatformName_delegate(EngineController_GetApplicationPlatformName);
		Delegates[2] = new EngineController_GetModulesVersionStr_delegate(EngineController_GetModulesVersionStr);
		Delegates[3] = new EngineController_GetVersionStr_delegate(EngineController_GetVersionStr);
		Delegates[4] = new EngineController_Initialize_delegate(EngineController_Initialize);
		Delegates[5] = new EngineController_OnConfigChange_delegate(EngineController_OnConfigChange);
		Delegates[6] = new EngineController_OnConstrainedStateChange_delegate(EngineController_OnConstrainedStateChange);
		Delegates[7] = new EngineController_OnControllerDisconnection_delegate(EngineController_OnControllerDisconnection);
		Delegates[8] = new EngineManaged_CheckSharedStructureSizes_delegate(EngineManaged_CheckSharedStructureSizes);
		Delegates[9] = new EngineManaged_EngineApiMethodInterfaceInitializer_delegate(EngineManaged_EngineApiMethodInterfaceInitializer);
		Delegates[10] = new EngineManaged_FillEngineApiPointers_delegate(EngineManaged_FillEngineApiPointers);
		Delegates[11] = new EngineScreenManager_InitializeLastPressedKeys_delegate(EngineScreenManager_InitializeLastPressedKeys);
		Delegates[12] = new EngineScreenManager_LateTick_delegate(EngineScreenManager_LateTick);
		Delegates[13] = new EngineScreenManager_OnGameWindowFocusChange_delegate(EngineScreenManager_OnGameWindowFocusChange);
		Delegates[14] = new EngineScreenManager_OnOnscreenKeyboardCanceled_delegate(EngineScreenManager_OnOnscreenKeyboardCanceled);
		Delegates[15] = new EngineScreenManager_OnOnscreenKeyboardDone_delegate(EngineScreenManager_OnOnscreenKeyboardDone);
		Delegates[16] = new EngineScreenManager_PreTick_delegate(EngineScreenManager_PreTick);
		Delegates[17] = new EngineScreenManager_Tick_delegate(EngineScreenManager_Tick);
		Delegates[18] = new EngineScreenManager_Update_delegate(EngineScreenManager_Update);
		Delegates[19] = new ManagedExtensions_CollectCommandLineFunctions_delegate(ManagedExtensions_CollectCommandLineFunctions);
		Delegates[20] = new ManagedExtensions_CopyObjectFieldsFrom_delegate(ManagedExtensions_CopyObjectFieldsFrom);
		Delegates[21] = new ManagedExtensions_CreateScriptComponentInstance_delegate(ManagedExtensions_CreateScriptComponentInstance);
		Delegates[22] = new ManagedExtensions_ForceGarbageCollect_delegate(ManagedExtensions_ForceGarbageCollect);
		Delegates[23] = new ManagedExtensions_GetEditorVisibilityOfField_delegate(ManagedExtensions_GetEditorVisibilityOfField);
		Delegates[24] = new ManagedExtensions_GetObjectField_delegate(ManagedExtensions_GetObjectField);
		Delegates[25] = new ManagedExtensions_GetScriptComponentClassNames_delegate(ManagedExtensions_GetScriptComponentClassNames);
		Delegates[26] = new ManagedExtensions_GetTypeOfField_delegate(ManagedExtensions_GetTypeOfField);
		Delegates[27] = new ManagedExtensions_SetObjectField_delegate(ManagedExtensions_SetObjectField);
		Delegates[28] = new ManagedScriptHolder_CreateManagedScriptHolder_delegate(ManagedScriptHolder_CreateManagedScriptHolder);
		Delegates[29] = new ManagedScriptHolder_GetNumberOfScripts_delegate(ManagedScriptHolder_GetNumberOfScripts);
		Delegates[30] = new ManagedScriptHolder_RemoveScriptComponentFromAllTickLists_delegate(ManagedScriptHolder_RemoveScriptComponentFromAllTickLists);
		Delegates[31] = new ManagedScriptHolder_SetScriptComponentHolder_delegate(ManagedScriptHolder_SetScriptComponentHolder);
		Delegates[32] = new ManagedScriptHolder_TickComponents_delegate(ManagedScriptHolder_TickComponents);
		Delegates[33] = new ManagedScriptHolder_TickComponentsEditor_delegate(ManagedScriptHolder_TickComponentsEditor);
		Delegates[34] = new MessageManagerBase_PostMessageLine_delegate(MessageManagerBase_PostMessageLine);
		Delegates[35] = new MessageManagerBase_PostMessageLineFormatted_delegate(MessageManagerBase_PostMessageLineFormatted);
		Delegates[36] = new MessageManagerBase_PostSuccessLine_delegate(MessageManagerBase_PostSuccessLine);
		Delegates[37] = new MessageManagerBase_PostWarningLine_delegate(MessageManagerBase_PostWarningLine);
		Delegates[38] = new NativeParallelDriver_ParalelForLoopBodyCaller_delegate(NativeParallelDriver_ParalelForLoopBodyCaller);
		Delegates[39] = new NativeParallelDriver_ParalelForLoopBodyWithDtCaller_delegate(NativeParallelDriver_ParalelForLoopBodyWithDtCaller);
		Delegates[40] = new RenderTargetComponent_CreateRenderTargetComponent_delegate(RenderTargetComponent_CreateRenderTargetComponent);
		Delegates[41] = new RenderTargetComponent_OnPaintNeeded_delegate(RenderTargetComponent_OnPaintNeeded);
		Delegates[42] = new SceneProblemChecker_OnCheckForSceneProblems_delegate(SceneProblemChecker_OnCheckForSceneProblems);
		Delegates[43] = new ScriptComponentBehavior_AddScriptComponentToTick_delegate(ScriptComponentBehavior_AddScriptComponentToTick);
		Delegates[44] = new ScriptComponentBehavior_DeregisterAsPrefabScriptComponent_delegate(ScriptComponentBehavior_DeregisterAsPrefabScriptComponent);
		Delegates[45] = new ScriptComponentBehavior_DeregisterAsUndoStackScriptComponent_delegate(ScriptComponentBehavior_DeregisterAsUndoStackScriptComponent);
		Delegates[46] = new ScriptComponentBehavior_DisablesOroCreation_delegate(ScriptComponentBehavior_DisablesOroCreation);
		Delegates[47] = new ScriptComponentBehavior_GetEditableFields_delegate(ScriptComponentBehavior_GetEditableFields);
		Delegates[48] = new ScriptComponentBehavior_HandleOnRemoved_delegate(ScriptComponentBehavior_HandleOnRemoved);
		Delegates[49] = new ScriptComponentBehavior_IsOnlyVisual_delegate(ScriptComponentBehavior_IsOnlyVisual);
		Delegates[50] = new ScriptComponentBehavior_MovesEntity_delegate(ScriptComponentBehavior_MovesEntity);
		Delegates[51] = new ScriptComponentBehavior_OnCheckForProblems_delegate(ScriptComponentBehavior_OnCheckForProblems);
		Delegates[52] = new ScriptComponentBehavior_OnEditModeVisibilityChanged_delegate(ScriptComponentBehavior_OnEditModeVisibilityChanged);
		Delegates[53] = new ScriptComponentBehavior_OnEditorInit_delegate(ScriptComponentBehavior_OnEditorInit);
		Delegates[54] = new ScriptComponentBehavior_OnEditorTick_delegate(ScriptComponentBehavior_OnEditorTick);
		Delegates[55] = new ScriptComponentBehavior_OnEditorValidate_delegate(ScriptComponentBehavior_OnEditorValidate);
		Delegates[56] = new ScriptComponentBehavior_OnEditorVariableChanged_delegate(ScriptComponentBehavior_OnEditorVariableChanged);
		Delegates[57] = new ScriptComponentBehavior_OnInit_delegate(ScriptComponentBehavior_OnInit);
		Delegates[58] = new ScriptComponentBehavior_OnPhysicsCollision_delegate(ScriptComponentBehavior_OnPhysicsCollision);
		Delegates[59] = new ScriptComponentBehavior_OnPreInit_delegate(ScriptComponentBehavior_OnPreInit);
		Delegates[60] = new ScriptComponentBehavior_OnSceneSave_delegate(ScriptComponentBehavior_OnSceneSave);
		Delegates[61] = new ScriptComponentBehavior_RegisterAsPrefabScriptComponent_delegate(ScriptComponentBehavior_RegisterAsPrefabScriptComponent);
		Delegates[62] = new ScriptComponentBehavior_RegisterAsUndoStackScriptComponent_delegate(ScriptComponentBehavior_RegisterAsUndoStackScriptComponent);
		Delegates[63] = new ScriptComponentBehavior_SetScene_delegate(ScriptComponentBehavior_SetScene);
		Delegates[64] = new ThumbnailCreatorView_OnThumbnailRenderComplete_delegate(ThumbnailCreatorView_OnThumbnailRenderComplete);
	}

	[MonoPInvokeCallback(typeof(CrashInformationCollector_CollectInformation_delegate))]
	internal static UIntPtr CrashInformationCollector_CollectInformation()
	{
		string text = CrashInformationCollector.CollectInformation();
		UIntPtr threadLocalCachedRglVarString = NativeStringHelper.GetThreadLocalCachedRglVarString();
		NativeStringHelper.SetRglVarString(threadLocalCachedRglVarString, text);
		return threadLocalCachedRglVarString;
	}

	[MonoPInvokeCallback(typeof(EngineController_GetApplicationPlatformName_delegate))]
	internal static UIntPtr EngineController_GetApplicationPlatformName()
	{
		string applicationPlatformName = EngineController.GetApplicationPlatformName();
		UIntPtr threadLocalCachedRglVarString = NativeStringHelper.GetThreadLocalCachedRglVarString();
		NativeStringHelper.SetRglVarString(threadLocalCachedRglVarString, applicationPlatformName);
		return threadLocalCachedRglVarString;
	}

	[MonoPInvokeCallback(typeof(EngineController_GetModulesVersionStr_delegate))]
	internal static UIntPtr EngineController_GetModulesVersionStr()
	{
		string modulesVersionStr = EngineController.GetModulesVersionStr();
		UIntPtr threadLocalCachedRglVarString = NativeStringHelper.GetThreadLocalCachedRglVarString();
		NativeStringHelper.SetRglVarString(threadLocalCachedRglVarString, modulesVersionStr);
		return threadLocalCachedRglVarString;
	}

	[MonoPInvokeCallback(typeof(EngineController_GetVersionStr_delegate))]
	internal static UIntPtr EngineController_GetVersionStr()
	{
		string versionStr = EngineController.GetVersionStr();
		UIntPtr threadLocalCachedRglVarString = NativeStringHelper.GetThreadLocalCachedRglVarString();
		NativeStringHelper.SetRglVarString(threadLocalCachedRglVarString, versionStr);
		return threadLocalCachedRglVarString;
	}

	[MonoPInvokeCallback(typeof(EngineController_Initialize_delegate))]
	internal static void EngineController_Initialize()
	{
		EngineController.Initialize();
	}

	[MonoPInvokeCallback(typeof(EngineController_OnConfigChange_delegate))]
	internal static void EngineController_OnConfigChange()
	{
		EngineController.OnConfigChange();
	}

	[MonoPInvokeCallback(typeof(EngineController_OnConstrainedStateChange_delegate))]
	internal static void EngineController_OnConstrainedStateChange(bool isConstrained)
	{
		EngineController.OnConstrainedStateChange(isConstrained);
	}

	[MonoPInvokeCallback(typeof(EngineController_OnControllerDisconnection_delegate))]
	internal static void EngineController_OnControllerDisconnection()
	{
		EngineController.OnControllerDisconnection();
	}

	[MonoPInvokeCallback(typeof(EngineManaged_CheckSharedStructureSizes_delegate))]
	internal static void EngineManaged_CheckSharedStructureSizes()
	{
		EngineManaged.CheckSharedStructureSizes();
	}

	[MonoPInvokeCallback(typeof(EngineManaged_EngineApiMethodInterfaceInitializer_delegate))]
	internal static void EngineManaged_EngineApiMethodInterfaceInitializer(int id, IntPtr pointer)
	{
		EngineManaged.EngineApiMethodInterfaceInitializer(id, pointer);
	}

	[MonoPInvokeCallback(typeof(EngineManaged_FillEngineApiPointers_delegate))]
	internal static void EngineManaged_FillEngineApiPointers()
	{
		EngineManaged.FillEngineApiPointers();
	}

	[MonoPInvokeCallback(typeof(EngineScreenManager_InitializeLastPressedKeys_delegate))]
	internal static void EngineScreenManager_InitializeLastPressedKeys(NativeObjectPointer lastKeysPressed)
	{
		NativeArray lastKeysPressed2 = null;
		if (lastKeysPressed.Pointer != UIntPtr.Zero)
		{
			lastKeysPressed2 = new NativeArray(lastKeysPressed.Pointer);
		}
		EngineScreenManager.InitializeLastPressedKeys(lastKeysPressed2);
	}

	[MonoPInvokeCallback(typeof(EngineScreenManager_LateTick_delegate))]
	internal static void EngineScreenManager_LateTick(float dt)
	{
		EngineScreenManager.LateTick(dt);
	}

	[MonoPInvokeCallback(typeof(EngineScreenManager_OnGameWindowFocusChange_delegate))]
	internal static void EngineScreenManager_OnGameWindowFocusChange(bool focusGained)
	{
		EngineScreenManager.OnGameWindowFocusChange(focusGained);
	}

	[MonoPInvokeCallback(typeof(EngineScreenManager_OnOnscreenKeyboardCanceled_delegate))]
	internal static void EngineScreenManager_OnOnscreenKeyboardCanceled()
	{
		EngineScreenManager.OnOnscreenKeyboardCanceled();
	}

	[MonoPInvokeCallback(typeof(EngineScreenManager_OnOnscreenKeyboardDone_delegate))]
	internal static void EngineScreenManager_OnOnscreenKeyboardDone(IntPtr inputText)
	{
		EngineScreenManager.OnOnscreenKeyboardDone(Marshal.PtrToStringAnsi(inputText));
	}

	[MonoPInvokeCallback(typeof(EngineScreenManager_PreTick_delegate))]
	internal static void EngineScreenManager_PreTick(float dt)
	{
		EngineScreenManager.PreTick(dt);
	}

	[MonoPInvokeCallback(typeof(EngineScreenManager_Tick_delegate))]
	internal static void EngineScreenManager_Tick(float dt)
	{
		EngineScreenManager.Tick(dt);
	}

	[MonoPInvokeCallback(typeof(EngineScreenManager_Update_delegate))]
	internal static void EngineScreenManager_Update()
	{
		EngineScreenManager.Update();
	}

	[MonoPInvokeCallback(typeof(ManagedExtensions_CollectCommandLineFunctions_delegate))]
	internal static void ManagedExtensions_CollectCommandLineFunctions()
	{
		ManagedExtensions.CollectCommandLineFunctions();
	}

	[MonoPInvokeCallback(typeof(ManagedExtensions_CopyObjectFieldsFrom_delegate))]
	internal static void ManagedExtensions_CopyObjectFieldsFrom(int dst, int src, IntPtr className, int callFieldChangeEventAsInteger)
	{
		DotNetObject managedObjectWithId = DotNetObject.GetManagedObjectWithId(dst);
		DotNetObject managedObjectWithId2 = DotNetObject.GetManagedObjectWithId(src);
		string className2 = Marshal.PtrToStringAnsi(className);
		ManagedExtensions.CopyObjectFieldsFrom(managedObjectWithId, managedObjectWithId2, className2, callFieldChangeEventAsInteger);
	}

	[MonoPInvokeCallback(typeof(ManagedExtensions_CreateScriptComponentInstance_delegate))]
	internal static int ManagedExtensions_CreateScriptComponentInstance(IntPtr className, NativeObjectPointer entity, NativeObjectPointer managedScriptComponent)
	{
		string? className2 = Marshal.PtrToStringAnsi(className);
		GameEntity entity2 = null;
		if (entity.Pointer != UIntPtr.Zero)
		{
			entity2 = new GameEntity(entity.Pointer);
		}
		ManagedScriptComponent managedScriptComponent2 = null;
		if (managedScriptComponent.Pointer != UIntPtr.Zero)
		{
			managedScriptComponent2 = new ManagedScriptComponent(managedScriptComponent.Pointer);
		}
		return ManagedExtensions.CreateScriptComponentInstance(className2, entity2, managedScriptComponent2).GetManagedId();
	}

	[MonoPInvokeCallback(typeof(ManagedExtensions_ForceGarbageCollect_delegate))]
	internal static void ManagedExtensions_ForceGarbageCollect()
	{
		ManagedExtensions.ForceGarbageCollect();
	}

	[MonoPInvokeCallback(typeof(ManagedExtensions_GetEditorVisibilityOfField_delegate))]
	internal static bool ManagedExtensions_GetEditorVisibilityOfField(IntPtr className, IntPtr fieldName)
	{
		string? className2 = Marshal.PtrToStringAnsi(className);
		string fieldName2 = Marshal.PtrToStringAnsi(fieldName);
		return ManagedExtensions.GetEditorVisibilityOfField(className2, fieldName2);
	}

	[MonoPInvokeCallback(typeof(ManagedExtensions_GetObjectField_delegate))]
	internal static void ManagedExtensions_GetObjectField(int managedObject, ref ScriptComponentFieldHolder scriptComponentFieldHolder, IntPtr fieldName, int type)
	{
		DotNetObject managedObjectWithId = DotNetObject.GetManagedObjectWithId(managedObject);
		string fieldName2 = Marshal.PtrToStringAnsi(fieldName);
		ManagedExtensions.GetObjectField(managedObjectWithId, ref scriptComponentFieldHolder, fieldName2, type);
	}

	[MonoPInvokeCallback(typeof(ManagedExtensions_GetScriptComponentClassNames_delegate))]
	internal static UIntPtr ManagedExtensions_GetScriptComponentClassNames()
	{
		string scriptComponentClassNames = ManagedExtensions.GetScriptComponentClassNames();
		UIntPtr threadLocalCachedRglVarString = NativeStringHelper.GetThreadLocalCachedRglVarString();
		NativeStringHelper.SetRglVarString(threadLocalCachedRglVarString, scriptComponentClassNames);
		return threadLocalCachedRglVarString;
	}

	[MonoPInvokeCallback(typeof(ManagedExtensions_GetTypeOfField_delegate))]
	internal static int ManagedExtensions_GetTypeOfField(IntPtr className, IntPtr fieldName)
	{
		string? className2 = Marshal.PtrToStringAnsi(className);
		string fieldName2 = Marshal.PtrToStringAnsi(fieldName);
		return ManagedExtensions.GetTypeOfField(className2, fieldName2);
	}

	[MonoPInvokeCallback(typeof(ManagedExtensions_SetObjectField_delegate))]
	internal static void ManagedExtensions_SetObjectField(int managedObject, IntPtr fieldName, ref ScriptComponentFieldHolder scriptComponentHolder, int type, int callFieldChangeEventAsInteger)
	{
		DotNetObject managedObjectWithId = DotNetObject.GetManagedObjectWithId(managedObject);
		string fieldName2 = Marshal.PtrToStringAnsi(fieldName);
		ManagedExtensions.SetObjectField(managedObjectWithId, fieldName2, ref scriptComponentHolder, type, callFieldChangeEventAsInteger);
	}

	[MonoPInvokeCallback(typeof(ManagedScriptHolder_CreateManagedScriptHolder_delegate))]
	internal static int ManagedScriptHolder_CreateManagedScriptHolder()
	{
		return ManagedScriptHolder.CreateManagedScriptHolder().GetManagedId();
	}

	[MonoPInvokeCallback(typeof(ManagedScriptHolder_GetNumberOfScripts_delegate))]
	internal static int ManagedScriptHolder_GetNumberOfScripts(int thisPointer)
	{
		return (DotNetObject.GetManagedObjectWithId(thisPointer) as ManagedScriptHolder).GetNumberOfScripts();
	}

	[MonoPInvokeCallback(typeof(ManagedScriptHolder_RemoveScriptComponentFromAllTickLists_delegate))]
	internal static void ManagedScriptHolder_RemoveScriptComponentFromAllTickLists(int thisPointer, int sc)
	{
		ManagedScriptHolder obj = DotNetObject.GetManagedObjectWithId(thisPointer) as ManagedScriptHolder;
		ScriptComponentBehavior sc2 = DotNetObject.GetManagedObjectWithId(sc) as ScriptComponentBehavior;
		obj.RemoveScriptComponentFromAllTickLists(sc2);
	}

	[MonoPInvokeCallback(typeof(ManagedScriptHolder_SetScriptComponentHolder_delegate))]
	internal static void ManagedScriptHolder_SetScriptComponentHolder(int thisPointer, int sc)
	{
		ManagedScriptHolder obj = DotNetObject.GetManagedObjectWithId(thisPointer) as ManagedScriptHolder;
		ScriptComponentBehavior scriptComponentHolder = DotNetObject.GetManagedObjectWithId(sc) as ScriptComponentBehavior;
		obj.SetScriptComponentHolder(scriptComponentHolder);
	}

	[MonoPInvokeCallback(typeof(ManagedScriptHolder_TickComponents_delegate))]
	internal static void ManagedScriptHolder_TickComponents(int thisPointer, float dt)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ManagedScriptHolder).TickComponents(dt);
	}

	[MonoPInvokeCallback(typeof(ManagedScriptHolder_TickComponentsEditor_delegate))]
	internal static void ManagedScriptHolder_TickComponentsEditor(int thisPointer, float dt)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ManagedScriptHolder).TickComponentsEditor(dt);
	}

	[MonoPInvokeCallback(typeof(MessageManagerBase_PostMessageLine_delegate))]
	internal static void MessageManagerBase_PostMessageLine(int thisPointer, IntPtr text, uint color)
	{
		MessageManagerBase obj = DotNetObject.GetManagedObjectWithId(thisPointer) as MessageManagerBase;
		string text2 = Marshal.PtrToStringAnsi(text);
		obj.PostMessageLine(text2, color);
	}

	[MonoPInvokeCallback(typeof(MessageManagerBase_PostMessageLineFormatted_delegate))]
	internal static void MessageManagerBase_PostMessageLineFormatted(int thisPointer, IntPtr text, uint color)
	{
		MessageManagerBase obj = DotNetObject.GetManagedObjectWithId(thisPointer) as MessageManagerBase;
		string text2 = Marshal.PtrToStringAnsi(text);
		obj.PostMessageLineFormatted(text2, color);
	}

	[MonoPInvokeCallback(typeof(MessageManagerBase_PostSuccessLine_delegate))]
	internal static void MessageManagerBase_PostSuccessLine(int thisPointer, IntPtr text)
	{
		MessageManagerBase obj = DotNetObject.GetManagedObjectWithId(thisPointer) as MessageManagerBase;
		string text2 = Marshal.PtrToStringAnsi(text);
		obj.PostSuccessLine(text2);
	}

	[MonoPInvokeCallback(typeof(MessageManagerBase_PostWarningLine_delegate))]
	internal static void MessageManagerBase_PostWarningLine(int thisPointer, IntPtr text)
	{
		MessageManagerBase obj = DotNetObject.GetManagedObjectWithId(thisPointer) as MessageManagerBase;
		string text2 = Marshal.PtrToStringAnsi(text);
		obj.PostWarningLine(text2);
	}

	[MonoPInvokeCallback(typeof(NativeParallelDriver_ParalelForLoopBodyCaller_delegate))]
	internal static void NativeParallelDriver_ParalelForLoopBodyCaller(long loopBodyKey, int localStartIndex, int localEndIndex)
	{
		NativeParallelDriver.ParalelForLoopBodyCaller(loopBodyKey, localStartIndex, localEndIndex);
	}

	[MonoPInvokeCallback(typeof(NativeParallelDriver_ParalelForLoopBodyWithDtCaller_delegate))]
	internal static void NativeParallelDriver_ParalelForLoopBodyWithDtCaller(long loopBodyKey, int localStartIndex, int localEndIndex)
	{
		NativeParallelDriver.ParalelForLoopBodyWithDtCaller(loopBodyKey, localStartIndex, localEndIndex);
	}

	[MonoPInvokeCallback(typeof(RenderTargetComponent_CreateRenderTargetComponent_delegate))]
	internal static int RenderTargetComponent_CreateRenderTargetComponent(NativeObjectPointer renderTarget)
	{
		Texture renderTarget2 = null;
		if (renderTarget.Pointer != UIntPtr.Zero)
		{
			renderTarget2 = new Texture(renderTarget.Pointer);
		}
		return RenderTargetComponent.CreateRenderTargetComponent(renderTarget2).GetManagedId();
	}

	[MonoPInvokeCallback(typeof(RenderTargetComponent_OnPaintNeeded_delegate))]
	internal static void RenderTargetComponent_OnPaintNeeded(int thisPointer)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as RenderTargetComponent).OnPaintNeeded();
	}

	[MonoPInvokeCallback(typeof(SceneProblemChecker_OnCheckForSceneProblems_delegate))]
	internal static bool SceneProblemChecker_OnCheckForSceneProblems(NativeObjectPointer scene)
	{
		Scene scene2 = null;
		if (scene.Pointer != UIntPtr.Zero)
		{
			scene2 = new Scene(scene.Pointer);
		}
		return SceneProblemChecker.OnCheckForSceneProblems(scene2);
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_AddScriptComponentToTick_delegate))]
	internal static void ScriptComponentBehavior_AddScriptComponentToTick(int thisPointer)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).AddScriptComponentToTick();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_DeregisterAsPrefabScriptComponent_delegate))]
	internal static void ScriptComponentBehavior_DeregisterAsPrefabScriptComponent(int thisPointer)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).DeregisterAsPrefabScriptComponent();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_DeregisterAsUndoStackScriptComponent_delegate))]
	internal static void ScriptComponentBehavior_DeregisterAsUndoStackScriptComponent(int thisPointer)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).DeregisterAsUndoStackScriptComponent();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_DisablesOroCreation_delegate))]
	internal static bool ScriptComponentBehavior_DisablesOroCreation(int thisPointer)
	{
		return (DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).DisablesOroCreation();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_GetEditableFields_delegate))]
	internal static int ScriptComponentBehavior_GetEditableFields(IntPtr className)
	{
		return Managed.AddCustomParameter(ScriptComponentBehavior.GetEditableFields(Marshal.PtrToStringAnsi(className))).GetManagedId();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_HandleOnRemoved_delegate))]
	internal static void ScriptComponentBehavior_HandleOnRemoved(int thisPointer, int removeReason)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).HandleOnRemoved(removeReason);
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_IsOnlyVisual_delegate))]
	internal static bool ScriptComponentBehavior_IsOnlyVisual(int thisPointer)
	{
		return (DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).IsOnlyVisual();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_MovesEntity_delegate))]
	internal static bool ScriptComponentBehavior_MovesEntity(int thisPointer)
	{
		return (DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).MovesEntity();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_OnCheckForProblems_delegate))]
	internal static bool ScriptComponentBehavior_OnCheckForProblems(int thisPointer)
	{
		return (DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).OnCheckForProblems();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_OnEditModeVisibilityChanged_delegate))]
	internal static void ScriptComponentBehavior_OnEditModeVisibilityChanged(int thisPointer, bool currentVisibility)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).OnEditModeVisibilityChanged(currentVisibility);
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_OnEditorInit_delegate))]
	internal static void ScriptComponentBehavior_OnEditorInit(int thisPointer)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).OnEditorInit();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_OnEditorTick_delegate))]
	internal static void ScriptComponentBehavior_OnEditorTick(int thisPointer, float dt)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).OnEditorTick(dt);
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_OnEditorValidate_delegate))]
	internal static void ScriptComponentBehavior_OnEditorValidate(int thisPointer)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).OnEditorValidate();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_OnEditorVariableChanged_delegate))]
	internal static void ScriptComponentBehavior_OnEditorVariableChanged(int thisPointer, IntPtr variableName)
	{
		ScriptComponentBehavior obj = DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior;
		string variableName2 = Marshal.PtrToStringAnsi(variableName);
		obj.OnEditorVariableChanged(variableName2);
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_OnInit_delegate))]
	internal static void ScriptComponentBehavior_OnInit(int thisPointer)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).OnInit();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_OnPhysicsCollision_delegate))]
	internal static void ScriptComponentBehavior_OnPhysicsCollision(int thisPointer, ref PhysicsContact contact)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).OnPhysicsCollision(ref contact);
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_OnPreInit_delegate))]
	internal static void ScriptComponentBehavior_OnPreInit(int thisPointer)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).OnPreInit();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_OnSceneSave_delegate))]
	internal static void ScriptComponentBehavior_OnSceneSave(int thisPointer, IntPtr saveFolder)
	{
		ScriptComponentBehavior obj = DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior;
		string saveFolder2 = Marshal.PtrToStringAnsi(saveFolder);
		obj.OnSceneSave(saveFolder2);
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_RegisterAsPrefabScriptComponent_delegate))]
	internal static void ScriptComponentBehavior_RegisterAsPrefabScriptComponent(int thisPointer)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).RegisterAsPrefabScriptComponent();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_RegisterAsUndoStackScriptComponent_delegate))]
	internal static void ScriptComponentBehavior_RegisterAsUndoStackScriptComponent(int thisPointer)
	{
		(DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior).RegisterAsUndoStackScriptComponent();
	}

	[MonoPInvokeCallback(typeof(ScriptComponentBehavior_SetScene_delegate))]
	internal static void ScriptComponentBehavior_SetScene(int thisPointer, NativeObjectPointer scene)
	{
		ScriptComponentBehavior obj = DotNetObject.GetManagedObjectWithId(thisPointer) as ScriptComponentBehavior;
		Scene scene2 = null;
		if (scene.Pointer != UIntPtr.Zero)
		{
			scene2 = new Scene(scene.Pointer);
		}
		obj.SetScene(scene2);
	}

	[MonoPInvokeCallback(typeof(ThumbnailCreatorView_OnThumbnailRenderComplete_delegate))]
	internal static void ThumbnailCreatorView_OnThumbnailRenderComplete(IntPtr renderId, NativeObjectPointer renderTarget)
	{
		string? renderId2 = Marshal.PtrToStringAnsi(renderId);
		Texture renderTarget2 = null;
		if (renderTarget.Pointer != UIntPtr.Zero)
		{
			renderTarget2 = new Texture(renderTarget.Pointer);
		}
		ThumbnailCreatorView.OnThumbnailRenderComplete(renderId2, renderTarget2);
	}
}
