using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultPrisonerDonationModel : PrisonerDonationModel
{
	public override float CalculateRelationGainAfterHeroPrisonerDonate(PartyBase donatingParty, Hero donatedHero, Settlement donatedSettlement)
	{
		float result = 0f;
		int num = Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(donatedHero.CharacterObject, donatingParty.LeaderHero);
		int relation = donatedHero.GetRelation(donatedSettlement.OwnerClan.Leader);
		if (relation <= 0)
		{
			float num2 = 1f - (float)relation / 200f;
			result = (donatedHero.IsKingdomLeader ? (MathF.Min(40f, MathF.Pow(num, 0.5f) * 0.5f) * num2) : ((donatedHero.Clan.Leader != donatedHero) ? (MathF.Min(20f, MathF.Pow(num, 0.5f) * 0.1f) * num2) : (MathF.Min(30f, MathF.Pow(num, 0.5f) * 0.25f) * num2)));
		}
		return result;
	}

	public override float CalculateInfluenceGainAfterPrisonerDonation(PartyBase donatingParty, CharacterObject character, Settlement donatedSettlement)
	{
		float num = 0f;
		if (donatingParty.LeaderHero == Hero.MainHero)
		{
			if (character.IsHero)
			{
				Hero heroObject = character.HeroObject;
				float num2 = MathF.Pow(Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(heroObject.CharacterObject, donatingParty.LeaderHero), 0.4f);
				if (heroObject.IsKingdomLeader)
				{
					return num2;
				}
				if (heroObject.Clan.Leader == heroObject)
				{
					return num2 * 0.5f;
				}
				return num2 * 0.2f;
			}
			return num + character.GetPower() / 20f;
		}
		int tier = character.Tier;
		return (float)((2 + tier) * (8 + tier)) * 0.02f;
	}

	public override float CalculateInfluenceGainAfterTroopDonation(PartyBase donatingParty, CharacterObject donatedCharacter, Settlement donatedSettlement)
	{
		Hero leaderHero = donatingParty.LeaderHero;
		ExplainedNumber stat = new ExplainedNumber(donatedCharacter.GetPower() / 3f);
		if (leaderHero != null && leaderHero.GetPerkValue(DefaultPerks.Steward.Relocation))
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.Relocation, donatingParty.MobileParty, isPrimaryBonus: true, ref stat);
		}
		return stat.ResultNumber;
	}
}
