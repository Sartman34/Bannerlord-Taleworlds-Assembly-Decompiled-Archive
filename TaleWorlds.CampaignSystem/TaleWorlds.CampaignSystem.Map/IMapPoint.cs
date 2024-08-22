using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.Map;

public interface IMapPoint
{
	TextObject Name { get; }

	Vec2 Position2D { get; }

	PathFaceRecord CurrentNavigationFace { get; }

	IFaction MapFaction { get; }

	bool IsInspected { get; }

	bool IsVisible { get; }

	bool IsActive { get; set; }

	void OnGameInitialized();

	Vec3 GetLogicalPosition();
}
