using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.Map;

public interface IMapEntity
{
	Vec2 InteractionPosition { get; }

	TextObject Name { get; }

	bool IsMobileEntity { get; }

	bool ShowCircleAroundEntity { get; }

	bool OnMapClick(bool followModifierUsed);

	void OnHover();

	void OnOpenEncyclopedia();

	bool IsEnemyOf(IFaction faction);

	bool IsAllyOf(IFaction faction);

	void GetMountAndHarnessVisualIdsForPartyIcon(out string mountStringId, out string harnessStringId);

	void OnPartyInteraction(MobileParty mobileParty);
}
