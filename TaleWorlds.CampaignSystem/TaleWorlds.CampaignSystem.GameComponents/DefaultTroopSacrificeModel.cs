using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultTroopSacrificeModel : TroopSacrificeModel
{
	public override int BreakOutArmyLeaderRelationPenalty => -5;

	public override int BreakOutArmyMemberRelationPenalty => -1;

	public override int GetLostTroopCountForBreakingInBesiegedSettlement(MobileParty party, SiegeEvent siegeEvent)
	{
		return GetLostTroopCount(party, siegeEvent);
	}

	public override int GetLostTroopCountForBreakingOutOfBesiegedSettlement(MobileParty party, SiegeEvent siegeEvent)
	{
		return GetLostTroopCount(party, siegeEvent);
	}

	public override int GetNumberOfTroopsSacrificedForTryingToGetAway(BattleSideEnum battleSide, MapEvent mapEvent)
	{
		mapEvent.RecalculateStrengthOfSides();
		MapEventSide mapEventSide = mapEvent.GetMapEventSide(battleSide);
		float num = mapEvent.StrengthOfSide[(int)battleSide] + 1f;
		float a = mapEvent.StrengthOfSide[(int)battleSide.GetOppositeSide()] / num;
		int num2 = PartyBase.MainParty.NumberOfRegularMembers;
		if (MobileParty.MainParty.Army != null)
		{
			foreach (MobileParty attachedParty in MobileParty.MainParty.Army.LeaderParty.AttachedParties)
			{
				num2 += attachedParty.Party.NumberOfRegularMembers;
			}
		}
		int num3 = mapEventSide.CountTroops((FlattenedTroopRosterElement x) => x.State == RosterTroopState.Active && !x.Troop.IsHero);
		ExplainedNumber stat = new ExplainedNumber(1f);
		SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Tactics, DefaultSkillEffects.TacticsTroopSacrificeReduction, CharacterObject.PlayerCharacter, ref stat, -1, isBonusPositive: false);
		float num4 = (float)num2 * MathF.Pow(MathF.Min(a, 3f), 1.3f) * 0.1f + 5f;
		ExplainedNumber stat2 = new ExplainedNumber(MathF.Max(MathF.Round(num4 * stat.ResultNumber), 1));
		if (MobileParty.MainParty.HasPerk(DefaultPerks.Tactics.SwiftRegroup, checkSecondaryRole: true))
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Tactics.SwiftRegroup, MobileParty.MainParty, isPrimaryBonus: false, ref stat2);
		}
		if (stat2.ResultNumber <= (float)num3)
		{
			return MathF.Round(stat2.ResultNumber);
		}
		return -1;
	}

	private int GetLostTroopCount(MobileParty party, SiegeEvent siegeEvent)
	{
		int num = 5;
		ExplainedNumber stat = new ExplainedNumber(1f);
		SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Tactics, DefaultSkillEffects.TacticsTroopSacrificeReduction, CharacterObject.PlayerCharacter, ref stat);
		float num2 = stat.ResultNumber - 1f;
		float num3 = 0f;
		foreach (PartyBase item in siegeEvent.BesiegerCamp.GetInvolvedPartiesForEventType())
		{
			num3 += item.TotalStrength;
		}
		float num4;
		int num5;
		if (party.Army != null && party.Army.LeaderParty == party)
		{
			num4 = party.Army.LeaderParty.Party.TotalStrength;
			foreach (MobileParty attachedParty in party.Army.LeaderParty.AttachedParties)
			{
				num4 += attachedParty.Party.TotalStrength;
			}
			num5 = party.Army.TotalRegularCount;
		}
		else
		{
			num4 = party.Party.TotalStrength;
			num5 = party.MemberRoster.TotalRegulars;
		}
		float num6 = MathF.Clamp(0.12f * MathF.Pow((num3 + 1f) / (num4 + 1f), 0.25f), 0.12f, 0.24f);
		ExplainedNumber stat2 = new ExplainedNumber(num + (int)(num6 * MathF.Max(0f, 1f - num2) * (float)num5));
		if (MobileParty.MainParty.HasPerk(DefaultPerks.Tactics.Improviser, checkSecondaryRole: true))
		{
			PerkHelper.AddPerkBonusForParty(DefaultPerks.Tactics.Improviser, MobileParty.MainParty, isPrimaryBonus: false, ref stat2);
		}
		return MathF.Round(stat2.ResultNumber);
	}
}
