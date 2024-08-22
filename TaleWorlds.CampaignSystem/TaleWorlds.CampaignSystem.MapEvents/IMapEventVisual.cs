using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.MapEvents;

public interface IMapEventVisual
{
	void Initialize(Vec2 position, int battleSizeValue, bool hasSound, bool isVisible);

	void OnMapEventEnd();

	void SetVisibility(bool isVisible);
}
