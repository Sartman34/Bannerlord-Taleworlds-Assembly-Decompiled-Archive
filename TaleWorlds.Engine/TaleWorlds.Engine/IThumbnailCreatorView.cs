using System;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface IThumbnailCreatorView
{
	[EngineMethod("create_thumbnail_creator_view", false)]
	ThumbnailCreatorView CreateThumbnailCreatorView();

	[EngineMethod("register_scene", false)]
	void RegisterScene(UIntPtr pointer, UIntPtr scene_ptr, bool use_postfx);

	[EngineMethod("clear_requests", false)]
	void ClearRequests(UIntPtr pointer);

	[EngineMethod("cancel_request", false)]
	void CancelRequest(UIntPtr pointer, string render_id);

	[EngineMethod("register_entity", false)]
	void RegisterEntity(UIntPtr pointer, UIntPtr scene_ptr, UIntPtr cam_ptr, UIntPtr texture_ptr, UIntPtr entity_ptr, string render_id, int allocationGroupIndex);

	[EngineMethod("register_entity_without_texture", false)]
	void RegisterEntityWithoutTexture(UIntPtr pointer, UIntPtr scene_ptr, UIntPtr cam_ptr, UIntPtr entity_ptr, int width, int height, string render_id, string debug_name, int allocationGroupIndex);

	[EngineMethod("get_number_of_pending_requests", false)]
	int GetNumberOfPendingRequests(UIntPtr pointer);

	[EngineMethod("is_memory_cleared", false)]
	bool IsMemoryCleared(UIntPtr pointer);
}
