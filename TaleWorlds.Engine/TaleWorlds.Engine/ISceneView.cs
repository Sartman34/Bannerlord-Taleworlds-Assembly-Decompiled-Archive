using System;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface ISceneView
{
	[EngineMethod("create_scene_view", false)]
	SceneView CreateSceneView();

	[EngineMethod("set_scene", false)]
	void SetScene(UIntPtr ptr, UIntPtr scenePtr);

	[EngineMethod("set_accept_global_debug_render_objects", false)]
	void SetAcceptGlobalDebugRenderObjects(UIntPtr ptr, bool value);

	[EngineMethod("set_render_with_postfx", false)]
	void SetRenderWithPostfx(UIntPtr ptr, bool value);

	[EngineMethod("set_force_shader_compilation", false)]
	void SetForceShaderCompilation(UIntPtr ptr, bool value);

	[EngineMethod("check_scene_ready_to_render", false)]
	bool CheckSceneReadyToRender(UIntPtr ptr);

	[EngineMethod("set_postfx_config_params", false)]
	void SetPostfxConfigParams(UIntPtr ptr, int value);

	[EngineMethod("set_camera", false)]
	void SetCamera(UIntPtr ptr, UIntPtr cameraPtr);

	[EngineMethod("set_resolution_scaling", false)]
	void SetResolutionScaling(UIntPtr ptr, bool value);

	[EngineMethod("set_postfx_from_config", false)]
	void SetPostfxFromConfig(UIntPtr ptr);

	[EngineMethod("world_point_to_screen_point", false)]
	Vec2 WorldPointToScreenPoint(UIntPtr ptr, Vec3 position);

	[EngineMethod("screen_point_to_viewport_point", false)]
	Vec2 ScreenPointToViewportPoint(UIntPtr ptr, float position_x, float position_y);

	[EngineMethod("projected_mouse_position_on_ground", false)]
	bool ProjectedMousePositionOnGround(UIntPtr pointer, out Vec3 groundPosition, out Vec3 groundNormal, bool mouseVisible, BodyFlags excludeBodyOwnerFlags, bool checkOccludedSurface);

	[EngineMethod("translate_mouse", false)]
	void TranslateMouse(UIntPtr pointer, ref Vec3 worldMouseNear, ref Vec3 worldMouseFar, float maxDistance);

	[EngineMethod("set_scene_uses_skybox", false)]
	void SetSceneUsesSkybox(UIntPtr pointer, bool value);

	[EngineMethod("set_scene_uses_shadows", false)]
	void SetSceneUsesShadows(UIntPtr pointer, bool value);

	[EngineMethod("set_scene_uses_contour", false)]
	void SetSceneUsesContour(UIntPtr pointer, bool value);

	[EngineMethod("do_not_clear", false)]
	void DoNotClear(UIntPtr pointer, bool value);

	[EngineMethod("add_clear_task", false)]
	void AddClearTask(UIntPtr ptr, bool clearOnlySceneview);

	[EngineMethod("ready_to_render", false)]
	bool ReadyToRender(UIntPtr pointer);

	[EngineMethod("set_clear_and_disable_after_succesfull_render", false)]
	void SetClearAndDisableAfterSucessfullRender(UIntPtr pointer, bool value);

	[EngineMethod("set_clear_gbuffer", false)]
	void SetClearGbuffer(UIntPtr pointer, bool value);

	[EngineMethod("set_shadowmap_resolution_multiplier", false)]
	void SetShadowmapResolutionMultiplier(UIntPtr pointer, float value);

	[EngineMethod("set_pointlight_resolution_multiplier", false)]
	void SetPointlightResolutionMultiplier(UIntPtr pointer, float value);

	[EngineMethod("set_clean_screen_until_loading_done", false)]
	void SetCleanScreenUntilLoadingDone(UIntPtr pointer, bool value);

	[EngineMethod("clear_all", false)]
	void ClearAll(UIntPtr pointer, bool clear_scene, bool remove_terrain);

	[EngineMethod("set_focused_shadowmap", false)]
	void SetFocusedShadowmap(UIntPtr ptr, bool enable, ref Vec3 center, float radius);

	[EngineMethod("get_scene", false)]
	Scene GetScene(UIntPtr ptr);

	[EngineMethod("ray_cast_for_closest_entity_or_terrain", false)]
	bool RayCastForClosestEntityOrTerrain(UIntPtr ptr, ref Vec3 sourcePoint, ref Vec3 targetPoint, float rayThickness, ref float collisionDistance, ref Vec3 closestPoint, ref UIntPtr entityIndex, BodyFlags bodyExcludeFlags);
}
