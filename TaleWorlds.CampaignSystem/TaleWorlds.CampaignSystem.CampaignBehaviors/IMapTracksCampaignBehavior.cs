using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public interface IMapTracksCampaignBehavior : ICampaignBehavior
{
	MBReadOnlyList<Track> DetectedTracks { get; }

	void AddTrack(MobileParty target, Vec2 trackPosition, Vec2 trackDirection);

	void AddMapArrow(TextObject pointerName, Vec2 trackPosition, Vec2 trackDirection, float life);
}
