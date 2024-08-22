using System;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CharacterDevelopment;

public class TraitLevelingHelper
{
	private const int LordExecutedHonorPenalty = -1000;

	private const int TroopsSacrificedValorPenalty = -30;

	private const int VillageRaidedMercyPenalty = -30;

	private const int PartyStarvingGenerosityPenalty = -20;

	private const int PartyTreatedWellGenerosityBonus = 20;

	private const int LordFreedCalculatingBonus = 20;

	private const int PersuasionDefectionCalculatingBonus = 20;

	private static void AddPlayerTraitXPAndLogEntry(TraitObject trait, int xpValue, ActionNotes context, Hero referenceHero)
	{
		int traitLevel = Hero.MainHero.GetTraitLevel(trait);
		Campaign.Current.PlayerTraitDeveloper.AddTraitXp(trait, xpValue);
		if (traitLevel != Hero.MainHero.GetTraitLevel(trait))
		{
			CampaignEventDispatcher.Instance.OnPlayerTraitChanged(trait, traitLevel);
		}
		if (TaleWorlds.Library.MathF.Abs(xpValue) >= 10)
		{
			LogEntry.AddLogEntry(new PlayerReputationChangesLogEntry(trait, referenceHero, context));
		}
	}

	public static void OnBattleWon(MapEvent mapEvent, float contribution)
	{
		float num = 0f;
		float strengthRatio = mapEvent.GetMapEventSide(PlayerEncounter.Current.PlayerSide).StrengthRatio;
		if (strengthRatio > 0.9f)
		{
			num = TaleWorlds.Library.MathF.Min(20f, TaleWorlds.Library.MathF.Sqrt(mapEvent.StrengthOfSide[(int)mapEvent.GetOtherSide(PlayerEncounter.Current.PlayerSide)]) * strengthRatio);
		}
		int xpValue = (int)(num * contribution);
		AddPlayerTraitXPAndLogEntry(DefaultTraits.Valor, xpValue, ActionNotes.BattleValor, null);
	}

	public static void OnTroopsSacrificed()
	{
		AddPlayerTraitXPAndLogEntry(DefaultTraits.Valor, -30, ActionNotes.SacrificedTroops, null);
	}

	public static void OnLordExecuted()
	{
		AddPlayerTraitXPAndLogEntry(DefaultTraits.Honor, -1000, ActionNotes.SacrificedTroops, null);
	}

	public static void OnVillageRaided()
	{
		AddPlayerTraitXPAndLogEntry(DefaultTraits.Mercy, -30, ActionNotes.VillageRaid, null);
	}

	public static void OnHostileAction(int amount)
	{
		AddPlayerTraitXPAndLogEntry(DefaultTraits.Honor, amount, ActionNotes.HostileAction, null);
		AddPlayerTraitXPAndLogEntry(DefaultTraits.Mercy, amount, ActionNotes.HostileAction, null);
	}

	public static void OnPartyTreatedWell()
	{
		AddPlayerTraitXPAndLogEntry(DefaultTraits.Generosity, 20, ActionNotes.PartyTakenCareOf, null);
	}

	public static void OnPartyStarved()
	{
		AddPlayerTraitXPAndLogEntry(DefaultTraits.Generosity, -20, ActionNotes.PartyHungry, null);
	}

	public static void OnIssueFailed(Hero targetHero, Tuple<TraitObject, int>[] effectedTraits)
	{
		foreach (Tuple<TraitObject, int> tuple in effectedTraits)
		{
			AddPlayerTraitXPAndLogEntry(tuple.Item1, tuple.Item2, ActionNotes.QuestFailed, targetHero);
		}
	}

	public static void OnIssueSolvedThroughQuest(Hero targetHero, Tuple<TraitObject, int>[] effectedTraits)
	{
		foreach (Tuple<TraitObject, int> tuple in effectedTraits)
		{
			AddPlayerTraitXPAndLogEntry(tuple.Item1, tuple.Item2, ActionNotes.QuestSuccess, targetHero);
		}
	}

	public static void OnIssueSolvedThroughAlternativeSolution(Hero targetHero, Tuple<TraitObject, int>[] effectedTraits)
	{
		foreach (Tuple<TraitObject, int> tuple in effectedTraits)
		{
			AddPlayerTraitXPAndLogEntry(tuple.Item1, tuple.Item2, ActionNotes.QuestSuccess, targetHero);
		}
	}

	public static void OnIssueSolvedThroughBetrayal(Hero targetHero, Tuple<TraitObject, int>[] effectedTraits)
	{
		foreach (Tuple<TraitObject, int> tuple in effectedTraits)
		{
			AddPlayerTraitXPAndLogEntry(tuple.Item1, tuple.Item2, ActionNotes.QuestBetrayal, targetHero);
		}
	}

	public static void OnLordFreed(Hero targetHero)
	{
		AddPlayerTraitXPAndLogEntry(DefaultTraits.Calculating, 20, ActionNotes.NPCFreed, targetHero);
	}

	public static void OnPersuasionDefection(Hero targetHero)
	{
		AddPlayerTraitXPAndLogEntry(DefaultTraits.Calculating, 20, ActionNotes.PersuadedToDefect, targetHero);
	}

	public static void OnSiegeAftermathApplied(Settlement settlement, SiegeAftermathAction.SiegeAftermath aftermathType, TraitObject[] effectedTraits)
	{
		foreach (TraitObject trait in effectedTraits)
		{
			AddPlayerTraitXPAndLogEntry(trait, Campaign.Current.Models.SiegeAftermathModel.GetSiegeAftermathTraitXpChangeForPlayer(trait, settlement, aftermathType), ActionNotes.SiegeAftermath, null);
		}
	}
}
