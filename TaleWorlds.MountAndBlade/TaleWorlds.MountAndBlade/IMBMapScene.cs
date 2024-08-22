using System;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

[ScriptingInterfaceBase]
internal interface IMBMapScene
{
	[EngineMethod("get_accessible_point_near_position", false)]
	Vec3 GetAccessiblePointNearPosition(UIntPtr scenePointer, Vec2 position, float radius);

	[EngineMethod("remove_zero_corner_bodies", false)]
	void RemoveZeroCornerBodies(UIntPtr scenePointer);

	[EngineMethod("load_atmosphere_data", false)]
	void LoadAtmosphereData(UIntPtr scenePointer);

	[EngineMethod("get_face_index_for_multiple_positions", false)]
	void GetFaceIndexForMultiplePositions(UIntPtr scenePointer, int movedPartyCount, float[] positionArray, PathFaceRecord[] resultArray, bool check_if_disabled, bool check_height);

	[EngineMethod("tick_step_sound", false)]
	void TickStepSound(UIntPtr scenePointer, UIntPtr visualsPointer, int faceIndexterrainType, int soundType);

	[EngineMethod("tick_ambient_sounds", false)]
	void TickAmbientSounds(UIntPtr scenePointer, int terrainType);

	[EngineMethod("tick_visuals", false)]
	void TickVisuals(UIntPtr scenePointer, float tod, UIntPtr[] ticked_map_meshes, int tickedMapMeshesCount);

	[EngineMethod("validate_terrain_sound_ids", false)]
	void ValidateTerrainSoundIds();

	[EngineMethod("set_political_color", false)]
	void SetPoliticalColor(UIntPtr scenePointer, string value);

	[EngineMethod("set_frame_for_atmosphere", false)]
	void SetFrameForAtmosphere(UIntPtr scenePointer, float tod, float cameraElevation, bool forceLoadTextures);

	[EngineMethod("get_color_grade_grid_data", false)]
	void GetColorGradeGridData(UIntPtr scenePointer, byte[] snowData, string textureName);

	[EngineMethod("get_battle_scene_index_map_resolution", false)]
	void GetBattleSceneIndexMapResolution(UIntPtr scenePointer, ref int width, ref int height);

	[EngineMethod("get_battle_scene_index_map", false)]
	void GetBattleSceneIndexMap(UIntPtr scenePointer, byte[] indexData);

	[EngineMethod("set_terrain_dynamic_params", false)]
	void SetTerrainDynamicParams(UIntPtr scenePointer, Vec3 dynamic_params);

	[EngineMethod("set_season_time_factor", false)]
	void SetSeasonTimeFactor(UIntPtr scenePointer, float seasonTimeFactor);

	[EngineMethod("get_season_time_factor", false)]
	float GetSeasonTimeFactor(UIntPtr scenePointer);

	[EngineMethod("get_mouse_visible", false)]
	bool GetMouseVisible();

	[EngineMethod("send_mouse_key_down_event", false)]
	void SendMouseKeyEvent(int keyId, bool isDown);

	[EngineMethod("set_mouse_visible", false)]
	void SetMouseVisible(bool value);

	[EngineMethod("set_mouse_pos", false)]
	void SetMousePos(int posX, int posY);
}
