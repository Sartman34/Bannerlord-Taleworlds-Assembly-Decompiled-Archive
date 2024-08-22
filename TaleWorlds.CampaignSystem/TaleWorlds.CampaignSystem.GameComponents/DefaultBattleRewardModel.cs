using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultBattleRewardModel : BattleRewardModel
{
	private static readonly int[] _indices = new int[12];

	public override float DestroyHideoutBannerLootChance => 0.1f;

	public override float CaptureSettlementBannerLootChance => 0.5f;

	public override float DefeatRegularHeroBannerLootChance => 0.5f;

	public override float DefeatClanLeaderBannerLootChance => 0.25f;

	public override float DefeatKingdomRulerBannerLootChance => 0.1f;

	public override int GetPlayerGainedRelationAmount(MapEvent mapEvent, Hero hero)
	{
		MapEventSide mapEventSide = (mapEvent.AttackerSide.IsMainPartyAmongParties() ? mapEvent.AttackerSide : mapEvent.DefenderSide);
		float playerPartyContributionRate = mapEventSide.GetPlayerPartyContributionRate();
		float num = (mapEvent.StrengthOfSide[(int)PartyBase.MainParty.Side] - PlayerEncounter.Current.PlayerPartyInitialStrength) / (mapEvent.StrengthOfSide[(int)PartyBase.MainParty.OpponentSide] + 1f);
		float num2 = ((num < 1f) ? (1f + (1f - num)) : ((num < 3f) ? (0.5f * (3f - num)) : 0f));
		float renownValueAtMapEventEnd = mapEvent.GetRenownValueAtMapEventEnd((mapEventSide == mapEvent.AttackerSide) ? BattleSideEnum.Attacker : BattleSideEnum.Defender);
		ExplainedNumber explainedNumber = new ExplainedNumber(0.75f + MathF.Pow(playerPartyContributionRate * 1.3f * (num2 + renownValueAtMapEventEnd), 0.67f));
		if (Hero.MainHero.GetPerkValue(DefaultPerks.Charm.Camaraderie))
		{
			explainedNumber.AddFactor(DefaultPerks.Charm.Camaraderie.PrimaryBonus, DefaultPerks.Charm.Camaraderie.Name);
		}
		return (int)explainedNumber.ResultNumber;
	}

	public override ExplainedNumber CalculateRenownGain(PartyBase party, float renownValueOfBattle, float contributionShare)
	{
		ExplainedNumber stat = new ExplainedNumber(renownValueOfBattle * contributionShare, includeDescriptions: true);
		if (party.IsMobile)
		{
			if (party.MobileParty.HasPerk(DefaultPerks.Throwing.LongReach, checkSecondaryRole: true))
			{
				PerkHelper.AddPerkBonusForParty(DefaultPerks.Throwing.LongReach, party.MobileParty, isPrimaryBonus: false, ref stat);
			}
			if (party.MobileParty.HasPerk(DefaultPerks.Charm.PublicSpeaker))
			{
				stat.AddFactor(DefaultPerks.Charm.PublicSpeaker.PrimaryBonus, DefaultPerks.Charm.PublicSpeaker.Name);
			}
			if (party.LeaderHero != null)
			{
				PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Leadership.FamousCommander, party.LeaderHero.CharacterObject, isPrimaryBonus: true, ref stat);
			}
			if (PartyBaseHelper.HasFeat(party, DefaultCulturalFeats.VlandianRenownMercenaryFeat))
			{
				stat.AddFactor(DefaultCulturalFeats.VlandianRenownMercenaryFeat.EffectBonus, GameTexts.FindText("str_culture"));
			}
		}
		return stat;
	}

	public override ExplainedNumber CalculateInfluenceGain(PartyBase party, float influenceValueOfBattle, float contributionShare)
	{
		ExplainedNumber result = new ExplainedNumber(party.MapFaction.IsKingdomFaction ? (influenceValueOfBattle * contributionShare) : 0f, includeDescriptions: true);
		Hero leaderHero = party.LeaderHero;
		if (leaderHero != null && leaderHero.GetPerkValue(DefaultPerks.Charm.Warlord))
		{
			result.AddFactor(DefaultPerks.Charm.Warlord.PrimaryBonus, DefaultPerks.Charm.Warlord.Name);
		}
		return result;
	}

	public override ExplainedNumber CalculateMoraleGainVictory(PartyBase party, float renownValueOfBattle, float contributionShare)
	{
		ExplainedNumber stat = new ExplainedNumber(0.5f + renownValueOfBattle * contributionShare * 0.5f, includeDescriptions: true);
		if (party.IsMobile && party.MobileParty.HasPerk(DefaultPerks.Throwing.LongReach, checkSecondaryRole: true))
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Throwing.LongReach, party.MobileParty, isPrimaryBonus: false, ref stat);
		}
		if (party.IsMobile && party.MobileParty.HasPerk(DefaultPerks.Leadership.CitizenMilitia, checkSecondaryRole: true))
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Leadership.CitizenMilitia, party.MobileParty, isPrimaryBonus: false, ref stat);
		}
		return stat;
	}

	public override int CalculateGoldLossAfterDefeat(Hero partyLeaderHero)
	{
		float num = (float)partyLeaderHero.Gold * 0.05f;
		if (num > 10000f)
		{
			num = 10000f;
		}
		return (int)num;
	}

	public override EquipmentElement GetLootedItemFromTroop(CharacterObject character, float targetValue)
	{
		bool num = MobileParty.MainParty.HasPerk(DefaultPerks.Engineering.Metallurgy);
		Equipment randomElement = character.AllEquipments.GetRandomElement();
		EquipmentElement result = GetRandomItem(randomElement, targetValue);
		if (num && result.ItemModifier != null && result.ItemModifier.PriceMultiplier < 1f && MBRandom.RandomFloat < DefaultPerks.Engineering.Metallurgy.PrimaryBonus)
		{
			result = new EquipmentElement(result.Item);
		}
		return result;
	}

	private EquipmentElement GetRandomItem(Equipment equipment, float targetValue = 0f)
	{
		int num = 0;
		for (int i = 0; i < 12; i++)
		{
			if (equipment[i].Item != null && !equipment[i].Item.NotMerchandise)
			{
				_indices[num] = i;
				num++;
			}
		}
		for (int j = 0; j < num - 1; j++)
		{
			int num2 = j;
			int value = equipment[_indices[j]].Item.Value;
			for (int k = j + 1; k < num; k++)
			{
				if (equipment[_indices[k]].Item.Value > value)
				{
					num2 = k;
					value = equipment[_indices[k]].Item.Value;
				}
			}
			int num3 = _indices[j];
			_indices[j] = _indices[num2];
			_indices[num2] = num3;
		}
		if (num > 0)
		{
			for (int l = 0; l < num; l++)
			{
				int index = _indices[l];
				EquipmentElement result = equipment[index];
				if (result.Item == null || equipment[index].Item.NotMerchandise)
				{
					continue;
				}
				float b = (float)result.Item.Value + 0.1f;
				float num4 = 0.6f * (targetValue / (MathF.Max(targetValue, b) * (float)(num - l)));
				if (MBRandom.RandomFloat < num4)
				{
					ItemModifier itemModifier = result.Item.ItemComponent?.ItemModifierGroup?.GetRandomItemModifierLootScoreBased();
					if (itemModifier != null)
					{
						result = new EquipmentElement(result.Item, itemModifier);
					}
					return result;
				}
			}
		}
		return default(EquipmentElement);
	}

	public override float GetPartySavePrisonerAsMemberShareProbability(PartyBase winnerParty, float lootAmount)
	{
		float result = lootAmount;
		if (winnerParty.IsMobile && (winnerParty.MobileParty.IsVillager || winnerParty.MobileParty.IsCaravan || winnerParty.MobileParty.IsMilitia || (winnerParty.MobileParty.IsBandit && winnerParty.MobileParty.CurrentSettlement != null && winnerParty.MobileParty.CurrentSettlement.IsHideout)))
		{
			result = 0f;
		}
		return result;
	}

	public override float GetExpectedLootedItemValue(CharacterObject character)
	{
		return 6f * (float)(character.Level * character.Level);
	}

	public override float GetAITradePenalty()
	{
		return 1f / 55f;
	}
}
