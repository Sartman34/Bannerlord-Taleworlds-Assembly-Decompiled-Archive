using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Helpers;

public static class SkillHelper
{
	private static readonly TextObject _textLeader = new TextObject("{=SrfYbg3x}Leader");

	private static readonly TextObject _textPersonal = new TextObject("{=UxAl9iyi}Personal");

	private static readonly TextObject _textScout = new TextObject("{=92M0Pb5T}Scout");

	private static readonly TextObject _textQuartermaster = new TextObject("{=redwEIlW}Quartermaster");

	private static readonly TextObject _textEngineer = new TextObject("{=7h6cXdW7}Engineer");

	private static readonly TextObject _textPartyLeader = new TextObject("{=ggpRTQQl}Party Leader");

	private static readonly TextObject _textSurgeon = new TextObject("{=QBPrRdQJ}Surgeon");

	private static readonly TextObject _textSergeant = new TextObject("{=g9VIbA9s}Sergeant");

	private static readonly TextObject _textGovernor = new TextObject("{=Fa2nKXxI}Governor");

	private static readonly TextObject _textClanLeader = new TextObject("{=pqfz386V}Clan Leader");

	public static void AddSkillBonusForParty(SkillObject skill, SkillEffect skillEffect, MobileParty party, ref ExplainedNumber stat)
	{
		Hero leaderHero = party.LeaderHero;
		if (leaderHero == null || skillEffect == null)
		{
			return;
		}
		if (skillEffect.PrimaryRole == SkillEffect.PerkRole.PartyLeader || skillEffect.SecondaryRole == SkillEffect.PerkRole.PartyLeader)
		{
			int skillValue = leaderHero.GetSkillValue(skill);
			float num = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.PartyLeader) ? skillEffect.PrimaryBonus : skillEffect.SecondaryBonus);
			num *= ((leaderHero.Clan != Clan.PlayerClan) ? 1.8f : 1f);
			AddToStat(ref stat, skillEffect.IncrementType, num * (float)skillValue, _textLeader);
		}
		if (skillEffect.PrimaryRole == SkillEffect.PerkRole.Engineer || skillEffect.SecondaryRole == SkillEffect.PerkRole.Engineer)
		{
			Hero effectiveEngineer = party.EffectiveEngineer;
			if (effectiveEngineer != null)
			{
				int skillValue2 = effectiveEngineer.GetSkillValue(skill);
				float num2 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Engineer) ? skillEffect.PrimaryBonus : skillEffect.SecondaryBonus);
				AddToStat(ref stat, skillEffect.IncrementType, num2 * (float)skillValue2, _textEngineer);
			}
		}
		if (skillEffect.PrimaryRole == SkillEffect.PerkRole.Scout || skillEffect.SecondaryRole == SkillEffect.PerkRole.Scout)
		{
			Hero effectiveScout = party.EffectiveScout;
			if (effectiveScout != null)
			{
				int skillValue3 = effectiveScout.GetSkillValue(skill);
				float num3 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Scout) ? skillEffect.PrimaryBonus : skillEffect.SecondaryBonus);
				AddToStat(ref stat, skillEffect.IncrementType, num3 * (float)skillValue3, _textScout);
			}
		}
		if (skillEffect.PrimaryRole == SkillEffect.PerkRole.Surgeon || skillEffect.SecondaryRole == SkillEffect.PerkRole.Surgeon)
		{
			Hero effectiveSurgeon = party.EffectiveSurgeon;
			if (effectiveSurgeon != null)
			{
				int skillValue4 = effectiveSurgeon.GetSkillValue(skill);
				float num4 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Surgeon) ? skillEffect.PrimaryBonus : skillEffect.SecondaryBonus);
				AddToStat(ref stat, skillEffect.IncrementType, num4 * (float)skillValue4, _textSurgeon);
			}
		}
		if (skillEffect.PrimaryRole == SkillEffect.PerkRole.Quartermaster || skillEffect.SecondaryRole == SkillEffect.PerkRole.Quartermaster)
		{
			Hero effectiveQuartermaster = party.EffectiveQuartermaster;
			if (effectiveQuartermaster != null)
			{
				int skillValue5 = effectiveQuartermaster.GetSkillValue(skill);
				float num5 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Quartermaster) ? skillEffect.PrimaryBonus : skillEffect.SecondaryBonus);
				AddToStat(ref stat, skillEffect.IncrementType, num5 * (float)skillValue5, _textQuartermaster);
			}
		}
	}

	public static void AddSkillBonusForTown(SkillObject skill, SkillEffect skillEffect, Town town, ref ExplainedNumber bonuses)
	{
		if (skillEffect.PrimaryRole == SkillEffect.PerkRole.ClanLeader || skillEffect.SecondaryRole == SkillEffect.PerkRole.ClanLeader)
		{
			Hero hero = town.Owner.Settlement.OwnerClan?.Leader;
			if (hero != null)
			{
				int skillValue = hero.GetSkillValue(skill);
				float num = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.ClanLeader) ? skillEffect.PrimaryBonus : skillEffect.SecondaryBonus);
				AddToStat(ref bonuses, skillEffect.IncrementType, num * (float)skillValue, _textClanLeader);
			}
		}
		if (skillEffect.PrimaryRole == SkillEffect.PerkRole.Governor || skillEffect.SecondaryRole == SkillEffect.PerkRole.Governor)
		{
			Hero governor = town.Governor;
			if (governor != null && governor.CurrentSettlement == town.Settlement)
			{
				int skillValue2 = governor.GetSkillValue(skill);
				float num2 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Governor) ? skillEffect.PrimaryBonus : skillEffect.SecondaryBonus);
				AddToStat(ref bonuses, skillEffect.IncrementType, num2 * (float)skillValue2, _textGovernor);
			}
		}
	}

	public static void AddSkillBonusForCharacter(SkillObject skill, SkillEffect skillEffect, CharacterObject character, ref ExplainedNumber stat, int baseSkillOverride = -1, bool isBonusPositive = true, int extraSkillValue = 0)
	{
		int skillLevel = ((baseSkillOverride >= 0) ? baseSkillOverride : character.GetSkillValue(skill)) + extraSkillValue;
		int num = (isBonusPositive ? 1 : (-1));
		if (skillEffect.PrimaryRole == SkillEffect.PerkRole.Personal || skillEffect.SecondaryRole == SkillEffect.PerkRole.Personal)
		{
			float num2 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Personal) ? skillEffect.GetPrimaryValue(skillLevel) : skillEffect.GetSecondaryValue(skillLevel));
			AddToStat(ref stat, skillEffect.IncrementType, (float)num * num2, _textPersonal);
		}
		Hero heroObject = character.HeroObject;
		if (heroObject != null)
		{
			if ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Engineer || skillEffect.SecondaryRole == SkillEffect.PerkRole.Engineer) && character.IsHero && heroObject.PartyBelongedTo?.EffectiveEngineer == heroObject)
			{
				float num3 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Engineer) ? skillEffect.GetPrimaryValue(skillLevel) : skillEffect.GetSecondaryValue(skillLevel));
				AddToStat(ref stat, skillEffect.IncrementType, (float)num * num3, _textEngineer);
			}
			if ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Quartermaster || skillEffect.SecondaryRole == SkillEffect.PerkRole.Quartermaster) && character.IsHero && heroObject.PartyBelongedTo?.EffectiveQuartermaster == heroObject)
			{
				float num4 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Quartermaster) ? skillEffect.GetPrimaryValue(skillLevel) : skillEffect.GetSecondaryValue(skillLevel));
				AddToStat(ref stat, skillEffect.IncrementType, (float)num * num4, _textQuartermaster);
			}
			if ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Scout || skillEffect.SecondaryRole == SkillEffect.PerkRole.Scout) && character.IsHero && heroObject.PartyBelongedTo?.EffectiveScout == heroObject)
			{
				float num5 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Scout) ? skillEffect.GetPrimaryValue(skillLevel) : skillEffect.GetSecondaryValue(skillLevel));
				AddToStat(ref stat, skillEffect.IncrementType, (float)num * num5, _textScout);
			}
			if ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Surgeon || skillEffect.SecondaryRole == SkillEffect.PerkRole.Surgeon) && character.IsHero && heroObject.PartyBelongedTo?.EffectiveSurgeon == heroObject)
			{
				float num6 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Surgeon) ? skillEffect.GetPrimaryValue(skillLevel) : skillEffect.GetSecondaryValue(skillLevel));
				AddToStat(ref stat, skillEffect.IncrementType, (float)num * num6, _textSurgeon);
			}
			if ((skillEffect.PrimaryRole == SkillEffect.PerkRole.PartyLeader || skillEffect.SecondaryRole == SkillEffect.PerkRole.PartyLeader) && character.IsHero && heroObject.PartyBelongedTo?.LeaderHero == heroObject)
			{
				float num7 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.PartyLeader) ? skillEffect.GetPrimaryValue(skillLevel) : skillEffect.GetSecondaryValue(skillLevel));
				AddToStat(ref stat, skillEffect.IncrementType, (float)num * num7, _textPartyLeader);
			}
		}
	}

	public static string GetEffectDescriptionForSkillLevel(SkillEffect effect, int level)
	{
		MBTextManager.SetTextVariable("a0", effect.GetPrimaryValue(level).ToString("0.0"));
		MBTextManager.SetTextVariable("a1", effect.GetSecondaryValue(level).ToString("0.0"));
		return effect.Description.ToString();
	}

	private static void AddToStat(ref ExplainedNumber stat, SkillEffect.EffectIncrementType effectIncrementType, float number, TextObject text)
	{
		switch (effectIncrementType)
		{
		case SkillEffect.EffectIncrementType.Add:
			stat.Add(number, text);
			break;
		case SkillEffect.EffectIncrementType.AddFactor:
			stat.AddFactor(number * 0.01f, text);
			break;
		}
	}

	public static CharacterObject GetEffectivePartyLeaderForSkill(PartyBase party)
	{
		if (party == null)
		{
			return null;
		}
		if (party.LeaderHero == null)
		{
			TroopRoster memberRoster = party.MemberRoster;
			if ((object)memberRoster == null || memberRoster.TotalManCount <= 0)
			{
				return null;
			}
			return party.MemberRoster.GetCharacterAtIndex(0);
		}
		return party.LeaderHero.CharacterObject;
	}
}
