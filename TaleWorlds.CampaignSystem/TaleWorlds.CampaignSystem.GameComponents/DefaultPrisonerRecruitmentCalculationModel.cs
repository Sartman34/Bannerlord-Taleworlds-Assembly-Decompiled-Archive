using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultPrisonerRecruitmentCalculationModel : PrisonerRecruitmentCalculationModel
{
	public override int GetConformityNeededToRecruitPrisoner(CharacterObject character)
	{
		return (character.Level + 6) * (character.Level + 6) - 10;
	}

	public override int GetConformityChangePerHour(PartyBase party, CharacterObject troopToBoost)
	{
		ExplainedNumber explainedNumber = new ExplainedNumber(10f);
		if (party.LeaderHero != null)
		{
			explainedNumber.Add((float)party.LeaderHero.GetSkillValue(DefaultSkills.Leadership) * 0.05f);
		}
		if (troopToBoost.Tier <= 3 && party.MobileParty.HasPerk(DefaultPerks.Leadership.FerventAttacker, checkSecondaryRole: true))
		{
			explainedNumber.AddFactor(DefaultPerks.Leadership.FerventAttacker.SecondaryBonus);
		}
		if (troopToBoost.Tier >= 4 && party.MobileParty.HasPerk(DefaultPerks.Leadership.StoutDefender, checkSecondaryRole: true))
		{
			explainedNumber.AddFactor(DefaultPerks.Leadership.StoutDefender.SecondaryBonus);
		}
		if (troopToBoost.Occupation != Occupation.Bandit && party.MobileParty.HasPerk(DefaultPerks.Leadership.LoyaltyAndHonor, checkSecondaryRole: true))
		{
			explainedNumber.AddFactor(DefaultPerks.Leadership.LoyaltyAndHonor.SecondaryBonus);
		}
		if (troopToBoost.IsInfantry && party.MobileParty.HasPerk(DefaultPerks.Leadership.LeadByExample))
		{
			explainedNumber.AddFactor(DefaultPerks.Leadership.LeadByExample.PrimaryBonus);
		}
		if (troopToBoost.IsRanged && party.MobileParty.HasPerk(DefaultPerks.Leadership.TrustedCommander))
		{
			explainedNumber.AddFactor(DefaultPerks.Leadership.TrustedCommander.PrimaryBonus);
		}
		if (troopToBoost.Occupation == Occupation.Bandit && party.MobileParty.HasPerk(DefaultPerks.Roguery.Promises, checkSecondaryRole: true))
		{
			explainedNumber.AddFactor(DefaultPerks.Roguery.Promises.SecondaryBonus);
		}
		return MathF.Round(explainedNumber.ResultNumber);
	}

	public override int GetPrisonerRecruitmentMoraleEffect(PartyBase party, CharacterObject character, int num)
	{
		if (character.Culture == party.LeaderHero?.Culture)
		{
			MobileParty mobileParty = party.MobileParty;
			if (mobileParty != null && mobileParty.HasPerk(DefaultPerks.Leadership.Presence, checkSecondaryRole: true))
			{
				goto IL_0058;
			}
		}
		if (character.Occupation == Occupation.Bandit)
		{
			MobileParty mobileParty2 = party.MobileParty;
			if (mobileParty2 != null && mobileParty2.HasPerk(DefaultPerks.Roguery.TwoFaced, checkSecondaryRole: true))
			{
				goto IL_0058;
			}
		}
		int num2 = ((character.Occupation != Occupation.Bandit) ? (-1) : (-2));
		return num2 * num;
		IL_0058:
		return 0;
	}

	public override bool IsPrisonerRecruitable(PartyBase party, CharacterObject character, out int conformityNeeded)
	{
		if (!character.IsRegular || character.Tier > Campaign.Current.Models.CharacterStatsModel.MaxCharacterTier)
		{
			conformityNeeded = 0;
			return false;
		}
		int elementXp = party.MobileParty.PrisonRoster.GetElementXp(character);
		conformityNeeded = GetConformityNeededToRecruitPrisoner(character);
		return elementXp >= conformityNeeded;
	}

	public override bool ShouldPartyRecruitPrisoners(PartyBase party)
	{
		if (party.MobileParty.Morale > 30f || party.MobileParty.HasPerk(DefaultPerks.Leadership.Presence, checkSecondaryRole: true))
		{
			return party.PartySizeLimit > party.MobileParty.MemberRoster.TotalManCount;
		}
		return false;
	}

	public override int CalculateRecruitableNumber(PartyBase party, CharacterObject character)
	{
		if (character.IsHero || party.PrisonRoster.Count == 0 || party.PrisonRoster.TotalRegulars <= 0)
		{
			return 0;
		}
		int conformityNeededToRecruitPrisoner = Campaign.Current.Models.PrisonerRecruitmentCalculationModel.GetConformityNeededToRecruitPrisoner(character);
		int elementXp = party.PrisonRoster.GetElementXp(character);
		return MathF.Min(b: party.PrisonRoster.GetElementNumber(character), a: elementXp / conformityNeededToRecruitPrisoner);
	}
}
