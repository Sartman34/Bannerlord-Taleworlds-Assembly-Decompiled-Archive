using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultVolunteerModel : VolunteerModel
{
	public override int MaxVolunteerTier => 4;

	public override int MaximumIndexHeroCanRecruitFromHero(Hero buyerHero, Hero sellerHero, int useValueAsRelation = -101)
	{
		Settlement currentSettlement = sellerHero.CurrentSettlement;
		int num = 1;
		int num2 = ((buyerHero == Hero.MainHero) ? Campaign.Current.Models.DifficultyModel.GetPlayerRecruitSlotBonus() : 0);
		int num3 = ((buyerHero != Hero.MainHero) ? 1 : 0);
		int num4 = ((currentSettlement != null && buyerHero.MapFaction == currentSettlement.MapFaction) ? 1 : 0);
		int num5 = ((currentSettlement != null && buyerHero.MapFaction.IsAtWarWith(currentSettlement.MapFaction)) ? (-(1 + num3)) : 0);
		if (buyerHero.IsMinorFactionHero && currentSettlement != null && currentSettlement.IsVillage)
		{
			num5 = 0;
		}
		int num6 = ((useValueAsRelation < -100) ? buyerHero.GetRelation(sellerHero) : useValueAsRelation);
		int num7 = ((num6 >= 100) ? 7 : ((num6 >= 80) ? 6 : ((num6 >= 60) ? 5 : ((num6 >= 40) ? 4 : ((num6 >= 20) ? 3 : ((num6 >= 10) ? 2 : ((num6 >= 5) ? 1 : ((num6 < 0) ? (-1) : 0))))))));
		int num8 = 0;
		if (sellerHero.IsGangLeader && currentSettlement != null && currentSettlement.OwnerClan == buyerHero.Clan)
		{
			if (currentSettlement.IsTown)
			{
				Hero governor = currentSettlement.Town.Governor;
				if (governor != null && governor.GetPerkValue(DefaultPerks.Roguery.OneOfTheFamily))
				{
					goto IL_014e;
				}
			}
			if (currentSettlement.IsVillage)
			{
				Hero governor2 = currentSettlement.Village.Bound.Town.Governor;
				if (governor2 != null && governor2.GetPerkValue(DefaultPerks.Roguery.OneOfTheFamily))
				{
					goto IL_014e;
				}
			}
		}
		goto IL_015e;
		IL_014e:
		num8 += (int)DefaultPerks.Roguery.OneOfTheFamily.SecondaryBonus;
		goto IL_015e;
		IL_015e:
		if (sellerHero.IsMerchant && buyerHero.GetPerkValue(DefaultPerks.Trade.ArtisanCommunity))
		{
			num8 += (int)DefaultPerks.Trade.ArtisanCommunity.SecondaryBonus;
		}
		if (sellerHero.Culture == buyerHero.Culture && buyerHero.GetPerkValue(DefaultPerks.Leadership.CombatTips))
		{
			num8 += (int)DefaultPerks.Leadership.CombatTips.SecondaryBonus;
		}
		if (sellerHero.IsRuralNotable && buyerHero.GetPerkValue(DefaultPerks.Charm.Firebrand))
		{
			num8 += (int)DefaultPerks.Charm.Firebrand.SecondaryBonus;
		}
		if (sellerHero.IsUrbanNotable && buyerHero.GetPerkValue(DefaultPerks.Charm.FlexibleEthics))
		{
			num8 += (int)DefaultPerks.Charm.FlexibleEthics.SecondaryBonus;
		}
		if (sellerHero.IsArtisan && buyerHero.PartyBelongedTo != null && buyerHero.PartyBelongedTo.EffectiveEngineer != null && buyerHero.PartyBelongedTo.EffectiveEngineer.GetPerkValue(DefaultPerks.Engineering.EngineeringGuilds))
		{
			num8 += (int)DefaultPerks.Engineering.EngineeringGuilds.PrimaryBonus;
		}
		return MathF.Min(6, MathF.Max(0, num + num3 + num7 + num2 + num4 + num5 + num8));
	}

	public override float GetDailyVolunteerProductionProbability(Hero hero, int index, Settlement settlement)
	{
		float num = 0.7f;
		int num2 = 0;
		foreach (Town fief in hero.CurrentSettlement.MapFaction.Fiefs)
		{
			num2 += (fief.IsTown ? (((fief.Prosperity < 3000f) ? 1 : ((fief.Prosperity < 6000f) ? 2 : 3)) + fief.Villages.Count) : fief.Villages.Count);
		}
		float num3 = ((num2 < 46) ? ((float)num2 / 46f * ((float)num2 / 46f)) : 1f);
		num += ((hero.CurrentSettlement != null && num3 < 1f) ? ((1f - num3) * 0.2f) : 0f);
		float baseNumber = 0.75f * MathF.Clamp(MathF.Pow(num, index + 1), 0f, 1f);
		ExplainedNumber explainedNumber = new ExplainedNumber(baseNumber);
		if (hero.Clan?.Kingdom != null && hero.Clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.Cantons))
		{
			explainedNumber.AddFactor(0.2f);
		}
		Town town = (settlement.IsTown ? settlement.Town : settlement.Village.TradeBound?.Town);
		if (town != null && hero.IsAlive && hero.VolunteerTypes[index] != null && hero.VolunteerTypes[index].IsMounted && PerkHelper.GetPerkValueForTown(DefaultPerks.Riding.CavalryTactics, town))
		{
			explainedNumber.AddFactor(DefaultPerks.Riding.CavalryTactics.PrimaryBonus);
		}
		return explainedNumber.ResultNumber;
	}

	public override CharacterObject GetBasicVolunteer(Hero sellerHero)
	{
		if (sellerHero.IsRuralNotable && sellerHero.CurrentSettlement.Village.Bound.IsCastle)
		{
			return sellerHero.Culture.EliteBasicTroop;
		}
		return sellerHero.Culture.BasicTroop;
	}

	public override bool CanHaveRecruits(Hero hero)
	{
		Occupation occupation = hero.Occupation;
		if (occupation == Occupation.Mercenary || (uint)(occupation - 17) <= 5u)
		{
			return true;
		}
		return false;
	}
}
