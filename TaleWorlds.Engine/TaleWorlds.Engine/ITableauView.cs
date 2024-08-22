using System;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface ITableauView
{
	[EngineMethod("create_tableau_view", false)]
	TableauView CreateTableauView();

	[EngineMethod("set_sort_meshes", false)]
	void SetSortingEnabled(UIntPtr pointer, bool value);

	[EngineMethod("set_continous_rendering", false)]
	void SetContinousRendering(UIntPtr pointer, bool value);

	[EngineMethod("set_do_not_render_this_frame", false)]
	void SetDoNotRenderThisFrame(UIntPtr pointer, bool value);

	[EngineMethod("set_delete_after_rendering", false)]
	void SetDeleteAfterRendering(UIntPtr pointer, bool value);
}
