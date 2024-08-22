using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class PartyWageModel : GameModel
{
	public abstract int MaxWage { get; }

	public abstract int GetCharacterWage(CharacterObject character);

	public abstract ExplainedNumber GetTotalWage(MobileParty mobileParty, bool includeDescriptions = false);

	public abstract int GetTroopRecruitmentCost(CharacterObject troop, Hero buyerHero, bool withoutItemCost = false);
}
