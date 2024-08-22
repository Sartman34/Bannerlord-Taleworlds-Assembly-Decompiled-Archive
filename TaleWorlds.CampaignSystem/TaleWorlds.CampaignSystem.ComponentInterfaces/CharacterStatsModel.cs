using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class CharacterStatsModel : GameModel
{
	public abstract int MaxCharacterTier { get; }

	public abstract ExplainedNumber MaxHitpoints(CharacterObject character, bool includeDescriptions = false);

	public abstract int GetTier(CharacterObject character);
}
