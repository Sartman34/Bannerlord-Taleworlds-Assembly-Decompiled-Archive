using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Helpers;

public static class DiplomacyHelper
{
	public static bool IsWarCausedByPlayer(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail declareWarDetail)
	{
		switch (declareWarDetail)
		{
		case DeclareWarAction.DeclareWarDetail.CausedByPlayerHostility:
			return true;
		case DeclareWarAction.DeclareWarDetail.CausedByKingdomDecision:
			if (faction1 == Hero.MainHero.MapFaction && Hero.MainHero.MapFaction.Leader == Hero.MainHero)
			{
				return true;
			}
			return false;
		case DeclareWarAction.DeclareWarDetail.CausedByCrimeRatingChange:
			if (faction2 == Hero.MainHero.MapFaction && faction2.MainHeroCrimeRating > (float)Campaign.Current.Models.CrimeModel.DeclareWarCrimeRatingThreshold)
			{
				return true;
			}
			return false;
		case DeclareWarAction.DeclareWarDetail.CausedByKingdomCreation:
			if (faction1 == Hero.MainHero.MapFaction)
			{
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	private static bool IsLogInTimeRange(LogEntry entry, CampaignTime time)
	{
		return entry.GameTime.NumTicks >= time.NumTicks;
	}

	public static List<(LogEntry, IFaction, IFaction)> GetLogsForWar(StanceLink stance)
	{
		CampaignTime warStartDate = stance.WarStartDate;
		List<(LogEntry, IFaction, IFaction)> list = new List<(LogEntry, IFaction, IFaction)>();
		for (int num = Campaign.Current.LogEntryHistory.GameActionLogs.Count - 1; num >= 0; num--)
		{
			LogEntry logEntry = Campaign.Current.LogEntryHistory.GameActionLogs[num];
			if (IsLogInTimeRange(logEntry, warStartDate) && logEntry is IWarLog warLog && warLog.IsRelatedToWar(stance, out var effector, out var effected))
			{
				list.Add((logEntry, effector, effected));
			}
		}
		return list;
	}

	public static List<Settlement> GetSuccessfullSiegesInWarForFaction(IFaction capturerFaction, StanceLink stance, Func<Settlement, bool> condition = null)
	{
		CampaignTime warStartDate = stance.WarStartDate;
		List<Settlement> list = new List<Settlement>();
		for (int num = Campaign.Current.LogEntryHistory.GameActionLogs.Count - 1; num >= 0; num--)
		{
			LogEntry logEntry = Campaign.Current.LogEntryHistory.GameActionLogs[num];
			if (IsLogInTimeRange(logEntry, warStartDate) && logEntry is ChangeSettlementOwnerLogEntry changeSettlementOwnerLogEntry && (condition == null || condition(changeSettlementOwnerLogEntry.Settlement)) && !list.Contains(changeSettlementOwnerLogEntry.Settlement) && changeSettlementOwnerLogEntry.IsRelatedToWar(stance, out var effector, out var _) && effector == capturerFaction)
			{
				list.Add(changeSettlementOwnerLogEntry.Settlement);
			}
		}
		return list;
	}

	public static List<Settlement> GetRaidsInWar(IFaction faction, StanceLink stance, Func<Settlement, bool> condition = null)
	{
		CampaignTime warStartDate = stance.WarStartDate;
		List<Settlement> list = new List<Settlement>();
		for (int num = Campaign.Current.LogEntryHistory.GameActionLogs.Count - 1; num >= 0; num--)
		{
			LogEntry logEntry = Campaign.Current.LogEntryHistory.GameActionLogs[num];
			if (IsLogInTimeRange(logEntry, warStartDate) && logEntry is VillageStateChangedLogEntry villageStateChangedLogEntry && (condition == null || condition(villageStateChangedLogEntry.Village.Settlement)) && villageStateChangedLogEntry.IsRelatedToWar(stance, out var effector, out var _) && effector == faction && !list.Contains(villageStateChangedLogEntry.Village.Settlement))
			{
				list.Add(villageStateChangedLogEntry.Village.Settlement);
			}
		}
		return list;
	}

	public static List<Hero> GetPrisonersOfWarTakenByFaction(IFaction capturerFaction, IFaction prisonerFaction)
	{
		List<Hero> list = new List<Hero>();
		foreach (Hero lord in prisonerFaction.Lords)
		{
			if (lord.IsPrisoner && lord.PartyBelongedToAsPrisoner?.MapFaction == capturerFaction)
			{
				list.Add(lord);
			}
		}
		return list;
	}

	public static bool DidMainHeroSwornNotToAttackFaction(IFaction faction, out TextObject explanation)
	{
		explanation = TextObject.Empty;
		if (faction.NotAttackableByPlayerUntilTime.IsFuture)
		{
			explanation = GameTexts.FindText("str_enemy_not_attackable_tooltip");
			return true;
		}
		return false;
	}
}
