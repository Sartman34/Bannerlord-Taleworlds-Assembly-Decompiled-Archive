using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultRansomValueCalculationModel : RansomValueCalculationModel
{
	public override int PrisonerRansomValue(CharacterObject prisoner, Hero sellerHero = null)
	{
		int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(prisoner, null);
		float num = 0f;
		float num2 = 0f;
		float num3 = 1f;
		if (prisoner.HeroObject?.Clan != null)
		{
			num = (float)((prisoner.HeroObject.Clan.Tier + 2) * 200) * ((prisoner.HeroObject.Clan.Leader == prisoner.HeroObject) ? 2f : 1f);
			num2 = MathF.Sqrt(MathF.Max(0, prisoner.HeroObject.Gold)) * 6f;
			if (prisoner.HeroObject.Clan.Kingdom != null)
			{
				int count = prisoner.HeroObject.Clan.Kingdom.Fiefs.Count;
				num3 = ((!prisoner.HeroObject.MapFaction.IsKingdomFaction) ? 1f : ((count < 8) ? (((float)count + 1f) / 9f) : (1f + MathF.Sqrt(count - 8) * 0.1f)));
			}
			else
			{
				num3 = 0.5f;
			}
		}
		float num4 = ((prisoner.HeroObject != null) ? (num + num2) : 0f);
		int num5 = (int)(((float)troopRecruitmentCost + num4) * ((!prisoner.IsHero) ? 0.25f : 1f) * num3);
		if (sellerHero != null)
		{
			if (!prisoner.IsHero)
			{
				if (sellerHero.GetPerkValue(DefaultPerks.Roguery.Manhunter))
				{
					num5 = MathF.Round((float)num5 + (float)num5 * DefaultPerks.Roguery.Manhunter.PrimaryBonus);
				}
			}
			else if (sellerHero.IsPartyLeader && sellerHero.GetPerkValue(DefaultPerks.Roguery.RansomBroker))
			{
				num5 = MathF.Round((float)num5 + (float)num5 * DefaultPerks.Roguery.RansomBroker.PrimaryBonus);
			}
		}
		if (num5 != 0)
		{
			return num5;
		}
		return 1;
	}
}
