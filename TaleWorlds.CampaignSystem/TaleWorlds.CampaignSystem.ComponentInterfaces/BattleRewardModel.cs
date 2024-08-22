using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class BattleRewardModel : GameModel
{
	public abstract float DestroyHideoutBannerLootChance { get; }

	public abstract float CaptureSettlementBannerLootChance { get; }

	public abstract float DefeatRegularHeroBannerLootChance { get; }

	public abstract float DefeatClanLeaderBannerLootChance { get; }

	public abstract float DefeatKingdomRulerBannerLootChance { get; }

	public abstract int GetPlayerGainedRelationAmount(MapEvent mapEvent, Hero hero);

	public abstract ExplainedNumber CalculateRenownGain(PartyBase party, float renownValueOfBattle, float contributionShare);

	public abstract ExplainedNumber CalculateInfluenceGain(PartyBase party, float influenceValueOfBattle, float contributionShare);

	public abstract ExplainedNumber CalculateMoraleGainVictory(PartyBase party, float renownValueOfBattle, float contributionShare);

	public abstract int CalculateGoldLossAfterDefeat(Hero partyLeaderHero);

	public abstract EquipmentElement GetLootedItemFromTroop(CharacterObject character, float targetValue);

	public abstract float GetPartySavePrisonerAsMemberShareProbability(PartyBase winnerParty, float lootAmount);

	public abstract float GetExpectedLootedItemValue(CharacterObject character);

	public abstract float GetAITradePenalty();
}
