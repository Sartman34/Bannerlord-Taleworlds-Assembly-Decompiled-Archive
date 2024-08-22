using System;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

[ScriptingInterfaceBase]
internal interface IMBBannerlordTableauManager
{
	[EngineMethod("request_character_tableau_render", false)]
	void RequestCharacterTableauRender(int characterCodeId, string path, UIntPtr poseEntity, UIntPtr cameraObject, int tableauType);

	[EngineMethod("initialize_character_tableau_render_system", false)]
	void InitializeCharacterTableauRenderSystem();

	[EngineMethod("get_number_of_pending_tableau_requests", false)]
	int GetNumberOfPendingTableauRequests();
}
