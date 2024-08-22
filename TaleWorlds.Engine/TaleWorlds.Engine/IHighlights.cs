using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface IHighlights
{
	[EngineMethod("initialize", false)]
	void Initialize();

	[EngineMethod("open_group", false)]
	void OpenGroup(string id);

	[EngineMethod("close_group", false)]
	void CloseGroup(string id, bool destroy = false);

	[EngineMethod("save_screenshot", false)]
	void SaveScreenshot(string highlightId, string groupId);

	[EngineMethod("save_video", false)]
	void SaveVideo(string highlightId, string groupId, int startDelta, int endDelta);

	[EngineMethod("open_summary", false)]
	void OpenSummary(string groups);

	[EngineMethod("add_highlight", false)]
	void AddHighlight(string id, string name);

	[EngineMethod("remove_highlight", false)]
	void RemoveHighlight(string id);
}
