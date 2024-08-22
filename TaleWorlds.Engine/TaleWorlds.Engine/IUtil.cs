using System;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface IUtil
{
	[EngineMethod("output_benchmark_values_to_performance_reporter", false)]
	void OutputBenchmarkValuesToPerformanceReporter();

	[EngineMethod("set_loading_screen_percentage", false)]
	void SetLoadingScreenPercentage(float value);

	[EngineMethod("set_fixed_dt", false)]
	void SetFixedDt(bool enabled, float dt);

	[EngineMethod("set_benchmark_status", false)]
	void SetBenchmarkStatus(int status, string def);

	[EngineMethod("get_benchmark_status", false)]
	int GetBenchmarkStatus();

	[EngineMethod("is_benchmark_quited", false)]
	bool IsBenchmarkQuited();

	[EngineMethod("get_application_memory_statistics", false)]
	string GetApplicationMemoryStatistics();

	[EngineMethod("get_native_memory_statistics", false)]
	string GetNativeMemoryStatistics();

	[EngineMethod("command_line_argument_exits", false)]
	bool CommandLineArgumentExists(string str);

	[EngineMethod("export_nav_mesh_face_marks", false)]
	string ExportNavMeshFaceMarks(string file_name);

	[EngineMethod("take_ss_from_top", false)]
	string TakeSSFromTop(string file_name);

	[EngineMethod("check_if_assets_and_sources_are_same", false)]
	void CheckIfAssetsAndSourcesAreSame();

	[EngineMethod("gather_core_game_references", false)]
	void GatherCoreGameReferences(string scene_names);

	[EngineMethod("get_application_memory", false)]
	float GetApplicationMemory();

	[EngineMethod("disable_core_game", false)]
	void DisableCoreGame();

	[EngineMethod("find_meshes_without_lods", false)]
	void FindMeshesWithoutLods(string module_name);

	[EngineMethod("set_print_callstack_at_crashes", false)]
	void SetPrintCallstackAtCrahses(bool value);

	[EngineMethod("set_disable_dump_generation", false)]
	void SetDisableDumpGeneration(bool value);

	[EngineMethod("get_modules_code", false)]
	string GetModulesCode();

	[EngineMethod("get_full_module_path", false)]
	string GetFullModulePath(string moduleName);

	[EngineMethod("get_full_module_paths", false)]
	string GetFullModulePaths();

	[EngineMethod("get_full_file_path_of_scene", false)]
	string GetFullFilePathOfScene(string sceneName);

	[EngineMethod("pair_scene_name_to_module_name", false)]
	void PairSceneNameToModuleName(string sceneName, string moduleName);

	[EngineMethod("get_single_module_scenes_of_module", false)]
	string GetSingleModuleScenesOfModule(string moduleName);

	[EngineMethod("get_executable_working_directory", false)]
	string GetExecutableWorkingDirectory();

	[EngineMethod("add_main_thread_performance_query", false)]
	void AddMainThreadPerformanceQuery(string parent, string name, float seconds);

	[EngineMethod("set_dump_folder_path", false)]
	void SetDumpFolderPath(string path);

	[EngineMethod("check_scene_for_problems", false)]
	void CheckSceneForProblems(string path);

	[EngineMethod("set_screen_text_rendering_state", false)]
	void SetScreenTextRenderingState(bool value);

	[EngineMethod("set_message_line_rendering_state", false)]
	void SetMessageLineRenderingState(bool value);

	[EngineMethod("check_shader_compilation", false)]
	bool CheckShaderCompilation();

	[EngineMethod("set_crash_on_asserts", false)]
	void SetCrashOnAsserts(bool val);

	[EngineMethod("check_if_terrain_shader_header_generation_finished", false)]
	bool CheckIfTerrainShaderHeaderGenerationFinished();

	[EngineMethod("set_crash_on_warnings", false)]
	void SetCrashOnWarnings(bool val);

	[EngineMethod("generate_terrain_shader_headers", false)]
	void GenerateTerrainShaderHeaders(string targetPlatform, string targetConfig, string output_path);

	[EngineMethod("compile_terrain_shaders_dist", false)]
	void CompileTerrainShadersDist(string targetPlatform, string targetConfig, string output_path);

	[EngineMethod("compile_all_shaders", false)]
	void CompileAllShaders(string targetPlatform);

	[EngineMethod("toggle_render", false)]
	void ToggleRender();

	[EngineMethod("set_force_draw_entity_id", false)]
	void SetForceDrawEntityID(bool value);

	[EngineMethod("set_render_agents", false)]
	void SetRenderAgents(bool value);

	[EngineMethod("get_core_game_state", false)]
	int GetCoreGameState();

	[EngineMethod("set_core_game_state", false)]
	void SetCoreGameState(int state);

	[EngineMethod("execute_command_line_command", false)]
	string ExecuteCommandLineCommand(string command);

	[EngineMethod("quit_game", false)]
	void QuitGame();

	[EngineMethod("exit_process", false)]
	void ExitProcess(int exitCode);

	[EngineMethod("start_scene_performance_report", false)]
	void StartScenePerformanceReport(string folderPath);

	[EngineMethod("get_base_directory", false)]
	string GetBaseDirectory();

	[EngineMethod("get_visual_tests_test_files_path", false)]
	string GetVisualTestsTestFilesPath();

	[EngineMethod("get_visual_tests_validate_path", false)]
	string GetVisualTestsValidatePath();

	[EngineMethod("get_attachments_path", false)]
	string GetAttachmentsPath();

	[EngineMethod("is_scene_performance_report_finished", false)]
	bool IsSceneReportFinished();

	[EngineMethod("flush_managed_objects_memory", false)]
	void FlushManagedObjectsMemory();

	[EngineMethod("set_render_mode", false)]
	void SetRenderMode(int mode);

	[EngineMethod("add_performance_report_token", false)]
	void AddPerformanceReportToken(string performance_type, string name, float loading_time);

	[EngineMethod("add_scene_object_report", false)]
	void AddSceneObjectReport(string scene_name, string report_name, float report_value);

	[EngineMethod("output_performance_reports", false)]
	void OutputPerformanceReports();

	[EngineMethod("add_command_line_function", false)]
	void AddCommandLineFunction(string concatName);

	[EngineMethod("get_number_of_shader_compilations_in_progress", false)]
	int GetNumberOfShaderCompilationsInProgress();

	[EngineMethod("get_editor_selected_entity_count", false)]
	int GetEditorSelectedEntityCount();

	[EngineMethod("get_entity_count_of_selection_set", false)]
	int GetEntityCountOfSelectionSet(string name);

	[EngineMethod("get_build_number", false)]
	int GetBuildNumber();

	[EngineMethod("get_entities_of_selection_set", false)]
	void GetEntitiesOfSelectionSet(string name, UIntPtr[] gameEntitiesTemp);

	[EngineMethod("get_editor_selected_entities", false)]
	void GetEditorSelectedEntities(UIntPtr[] gameEntitiesTemp);

	[EngineMethod("delete_entities_in_editor_scene", false)]
	void DeleteEntitiesInEditorScene(UIntPtr[] gameEntities, int entityCount);

	[EngineMethod("create_selection_set_in_editor", false)]
	void CreateSelectionInEditor(UIntPtr[] gameEntities, int entityCount, string name);

	[EngineMethod("select_entities_in_editor", false)]
	void SelectEntities(UIntPtr[] gameEntities, int entityCount);

	[EngineMethod("get_current_cpu_memory_usage", false)]
	ulong GetCurrentCpuMemoryUsage();

	[EngineMethod("get_gpu_memory_stats", false)]
	void GetGPUMemoryStats(ref float totalMemory, ref float renderTargetMemory, ref float depthTargetMemory, ref float srvMemory, ref float bufferMemory);

	[EngineMethod("get_gpu_memory_of_allocation_group", false)]
	ulong GetGpuMemoryOfAllocationGroup(string allocationName);

	[EngineMethod("get_detailed_gpu_buffer_memory_stats", false)]
	void GetDetailedGPUBufferMemoryStats(ref int totalMemoryAllocated, ref int totalMemoryUsed, ref int emptyChunkCount);

	[EngineMethod("is_detailed_soung_log_on", false)]
	int IsDetailedSoundLogOn();

	[EngineMethod("get_main_fps", false)]
	float GetMainFps();

	[EngineMethod("on_loading_window_enabled", false)]
	void OnLoadingWindowEnabled();

	[EngineMethod("debug_set_global_loading_window_state", false)]
	void DebugSetGlobalLoadingWindowState(bool s);

	[EngineMethod("on_loading_window_disabled", false)]
	void OnLoadingWindowDisabled();

	[EngineMethod("disable_global_loading_window", false)]
	void DisableGlobalLoadingWindow();

	[EngineMethod("enable_global_loading_window", false)]
	void EnableGlobalLoadingWindow();

	[EngineMethod("enable_global_edit_data_cacher", false)]
	void EnableGlobalEditDataCacher();

	[EngineMethod("get_renderer_fps", false)]
	float GetRendererFps();

	[EngineMethod("disable_global_edit_data_cacher", false)]
	void DisableGlobalEditDataCacher();

	[EngineMethod("enable_single_gpu_query_per_frame", false)]
	void EnableSingleGPUQueryPerFrame();

	[EngineMethod("clear_decal_atlas", false)]
	void clear_decal_atlas(DecalAtlasGroup atlasGroup);

	[EngineMethod("get_fps", false)]
	float GetFps();

	[EngineMethod("get_full_command_line_string", false)]
	string GetFullCommandLineString();

	[EngineMethod("take_screenshot_from_string_path", false)]
	void TakeScreenshotFromStringPath(string path);

	[EngineMethod("take_screenshot_from_platform_path", false)]
	void TakeScreenshotFromPlatformPath(PlatformFilePath path);

	[EngineMethod("check_resource_modifications", false)]
	void CheckResourceModifications();

	[EngineMethod("set_graphics_preset", false)]
	void SetGraphicsPreset(int preset);

	[EngineMethod("clear_old_resources_and_objects", false)]
	void ClearOldResourcesAndObjects();

	[EngineMethod("load_virtual_texture_tileset", false)]
	void LoadVirtualTextureTileset(string name);

	[EngineMethod("get_delta_time", false)]
	float GetDeltaTime(int timerId);

	[EngineMethod("load_sky_boxes", false)]
	void LoadSkyBoxes();

	[EngineMethod("get_engine_frame_no", false)]
	int GetEngineFrameNo();

	[EngineMethod("set_allocation_always_valid_scene", false)]
	void SetAllocationAlwaysValidScene(UIntPtr scene);

	[EngineMethod("get_console_host_machine", false)]
	string GetConsoleHostMachine();

	[EngineMethod("is_edit_mode_enabled", false)]
	bool IsEditModeEnabled();

	[EngineMethod("get_pc_info", false)]
	string GetPCInfo();

	[EngineMethod("get_gpu_memory_mb", false)]
	int GetGPUMemoryMB();

	[EngineMethod("get_current_estimated_gpu_memory_cost_mb", false)]
	int GetCurrentEstimatedGPUMemoryCostMB();

	[EngineMethod("dump_gpu_memory_statistics", false)]
	void DumpGPUMemoryStatistics(string filePath);

	[EngineMethod("save_data_as_texture", false)]
	int SaveDataAsTexture(string path, int width, int height, float[] data);

	[EngineMethod("get_application_name", false)]
	string GetApplicationName();

	[EngineMethod("set_window_title", false)]
	void SetWindowTitle(string title);

	[EngineMethod("process_window_title", false)]
	string ProcessWindowTitle(string title);

	[EngineMethod("get_current_process_id", false)]
	uint GetCurrentProcessID();

	[EngineMethod("do_delayed_exit", false)]
	void DoDelayedexit(int returnCode);

	[EngineMethod("set_report_mode", false)]
	void SetReportMode(bool reportMode);

	[EngineMethod("set_assertions_and_warnings_set_exit_code", false)]
	void SetAssertionsAndWarningsSetExitCode(bool value);

	[EngineMethod("set_assertion_at_shader_compile", false)]
	void SetAssertionAtShaderCompile(bool value);

	[EngineMethod("did_automated_gi_bake_finished", false)]
	bool DidAutomatedGIBakeFinished();

	[EngineMethod("do_full_bake_all_levels_automated", false)]
	void DoFullBakeAllLevelsAutomated(string module, string sceneName);

	[EngineMethod("do_full_bake_single_level_automated", false)]
	void DoFullBakeSingleLevelAutomated(string module, string sceneName);

	[EngineMethod("get_return_code", false)]
	int GetReturnCode();

	[EngineMethod("do_light_only_bake_single_level_automated", false)]
	void DoLightOnlyBakeSingleLevelAutomated(string module, string sceneName);

	[EngineMethod("do_light_only_bake_all_levels_automated", false)]
	void DoLightOnlyBakeAllLevelsAutomated(string module, string sceneName);

	[EngineMethod("get_local_output_dir", false)]
	string GetLocalOutputPath();

	[EngineMethod("set_crash_report_custom_string", false)]
	void SetCrashReportCustomString(string customString);

	[EngineMethod("set_crash_report_custom_managed_stack", false)]
	void SetCrashReportCustomStack(string customStack);

	[EngineMethod("get_steam_appid", false)]
	int GetSteamAppId();

	[EngineMethod("set_force_vsync", false)]
	void SetForceVsync(bool value);

	[EngineMethod("get_system_language", false)]
	string GetSystemLanguage();

	[EngineMethod("managed_parallel_for", false)]
	void ManagedParallelFor(int fromInclusive, int toExclusive, long curKey, int grainSize);

	[EngineMethod("get_main_thread_id", false)]
	ulong GetMainThreadId();

	[EngineMethod("get_current_thread_id", false)]
	ulong GetCurrentThreadId();

	[EngineMethod("set_watchdog_value", false)]
	void SetWatchdogValue(string fileName, string groupName, string key, string value);

	[EngineMethod("detach_watchdog", false)]
	void DetachWatchdog();

	[EngineMethod("register_mesh_for_gpu_morph", false)]
	void RegisterMeshForGPUMorph(string metaMeshName);

	[EngineMethod("managed_parallel_for_with_dt", false)]
	void ManagedParallelForWithDt(int fromInclusive, int toExclusive, long curKey, int grainSize);

	[EngineMethod("clear_shader_memory", false)]
	void ClearShaderMemory();

	[EngineMethod("open_onscreen_keyboard", false)]
	void OpenOnscreenKeyboard(string initialText, string descriptionText, int maxLength, int keyboardTypeEnum);

	[EngineMethod("register_gpu_allocation_group", false)]
	int RegisterGPUAllocationGroup(string name);

	[EngineMethod("get_memory_usage_of_category", false)]
	int GetMemoryUsageOfCategory(int index);

	[EngineMethod("get_vertex_buffer_chunk_system_memory_usage", false)]
	int GetVertexBufferChunkSystemMemoryUsage();

	[EngineMethod("get_detailed_xbox_memory_info", false)]
	string GetDetailedXBOXMemoryInfo();

	[EngineMethod("set_frame_limiter_with_sleep", false)]
	void SetFrameLimiterWithSleep(bool value);

	[EngineMethod("get_frame_limiter_with_sleep", false)]
	bool GetFrameLimiterWithSleep();
}
