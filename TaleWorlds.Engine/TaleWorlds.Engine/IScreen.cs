using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface IScreen
{
	[EngineMethod("get_real_screen_resolution_width", false)]
	float GetRealScreenResolutionWidth();

	[EngineMethod("get_real_screen_resolution_height", false)]
	float GetRealScreenResolutionHeight();

	[EngineMethod("get_desktop_width", false)]
	float GetDesktopWidth();

	[EngineMethod("get_desktop_height", false)]
	float GetDesktopHeight();

	[EngineMethod("get_aspect_ratio", false)]
	float GetAspectRatio();

	[EngineMethod("get_mouse_visible", false)]
	bool GetMouseVisible();

	[EngineMethod("set_mouse_visible", false)]
	void SetMouseVisible(bool value);

	[EngineMethod("get_usable_area_percentages", false)]
	Vec2 GetUsableAreaPercentages();

	[EngineMethod("is_enter_button_cross", false)]
	bool IsEnterButtonCross();
}
