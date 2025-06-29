using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.Actions;

public static class KillCharacterAction
{
	public enum KillCharacterActionDetail
	{
		None,
		Murdered,
		DiedInLabor,
		DiedOfOldAge,
		DiedInBattle,
		WoundedInBattle,
		Executed,
		Lost
	}

	private static void ApplyInternal(Hero victim, Hero killer, KillCharacterActionDetail actionDetail, bool showNotification, bool isForced = false)
	{
		if (!victim.CanDie(actionDetail) && !isForced)
		{
			return;
		}
		if (!victim.IsAlive)
		{
			Debug.FailedAssert(string.Concat("Victim: ", victim.Name, " is already dead!"), "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Actions\\KillCharacterAction.cs", "ApplyInternal", 40);
			return;
		}
		if (victim.IsNotable && victim.Issue?.IssueQuest != null)
		{
			Debug.FailedAssert("Trying to kill a notable that has quest!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Actions\\KillCharacterAction.cs", "ApplyInternal", 47);
		}
		if ((victim.PartyBelongedTo?.MapEvent != null || victim.PartyBelongedTo?.SiegeEvent != null) && victim.DeathMark == KillCharacterActionDetail.None)
		{
			victim.AddDeathMark(killer, actionDetail);
			return;
		}
		CampaignEventDispatcher.Instance.OnBeforeHeroKilled(victim, killer, actionDetail, showNotification);
		if (victim.IsHumanPlayerCharacter && !isForced)
		{
			CampaignEventDispatcher.Instance.OnBeforeMainCharacterDied(victim, killer, actionDetail, showNotification);
			return;
		}
		victim.AddDeathMark(killer, actionDetail);
		victim.EncyclopediaText = CreateObituary(victim, actionDetail);
		if (victim.Clan != null && (victim.Clan.Leader == victim || victim == Hero.MainHero))
		{
			if (!victim.Clan.IsEliminated && victim != Hero.MainHero && victim.Clan.Heroes.Any((Hero x) => !x.IsChild && x != victim && x.IsAlive && x.IsLord))
			{
				ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(victim.Clan);
			}
			if (victim.Clan.Kingdom != null && victim.Clan.Kingdom.RulingClan == victim.Clan)
			{
				List<Clan> list = victim.Clan.Kingdom.Clans.Where((Clan t) => !t.IsEliminated && t.Leader != victim && !t.IsUnderMercenaryService).ToList();
				if (list.IsEmpty())
				{
					if (!victim.Clan.Kingdom.IsEliminated)
					{
						DestroyKingdomAction.ApplyByKingdomLeaderDeath(victim.Clan.Kingdom);
					}
				}
				else if (!victim.Clan.Kingdom.IsEliminated)
				{
					if (list.Count > 1)
					{
						Clan clanToExclude = ((victim.Clan.Leader == victim || victim.Clan.Leader == null) ? victim.Clan : null);
						victim.Clan.Kingdom.AddDecision(new KingSelectionKingdomDecision(victim.Clan, clanToExclude), ignoreInfluenceCost: true);
						if (clanToExclude != null)
						{
							Clan randomElementWithPredicate = victim.Clan.Kingdom.Clans.GetRandomElementWithPredicate((Clan t) => t != clanToExclude && Campaign.Current.Models.DiplomacyModel.IsClanEligibleToBecomeRuler(t));
							ChangeRulingClanAction.Apply(victim.Clan.Kingdom, randomElementWithPredicate);
						}
					}
					else
					{
						ChangeRulingClanAction.Apply(victim.Clan.Kingdom, list[0]);
					}
				}
			}
		}
		if (victim.PartyBelongedTo != null && (victim.PartyBelongedTo.LeaderHero == victim || victim == Hero.MainHero))
		{
			MobileParty partyBelongedTo = victim.PartyBelongedTo;
			if (victim.PartyBelongedTo.Army != null)
			{
				if (victim.PartyBelongedTo.Army.LeaderParty == victim.PartyBelongedTo)
				{
					DisbandArmyAction.ApplyByArmyLeaderIsDead(victim.PartyBelongedTo.Army);
				}
				else
				{
					victim.PartyBelongedTo.Army = null;
				}
			}
			if (partyBelongedTo != MobileParty.MainParty)
			{
				partyBelongedTo.Ai.SetMoveModeHold();
				if (victim.Clan != null && victim.Clan.IsRebelClan)
				{
					DestroyPartyAction.Apply(null, partyBelongedTo);
				}
			}
		}
		MakeDead(victim);
		if (victim.GovernorOf != null)
		{
			ChangeGovernorAction.RemoveGovernorOf(victim);
		}
		if (actionDetail == KillCharacterActionDetail.Executed && killer == Hero.MainHero && victim.Clan != null)
		{
			if (victim.GetTraitLevel(DefaultTraits.Honor) >= 0)
			{
				TraitLevelingHelper.OnLordExecuted();
			}
			foreach (Clan item in Clan.All)
			{
				if (!item.IsEliminated && !item.IsBanditFaction && item != Clan.PlayerClan)
				{
					bool showQuickNotification;
					int relationChangeForExecutingHero = Campaign.Current.Models.ExecutionRelationModel.GetRelationChangeForExecutingHero(victim, item.Leader, out showQuickNotification);
					if (relationChangeForExecutingHero != 0)
					{
						ChangeRelationAction.ApplyPlayerRelation(item.Leader, relationChangeForExecutingHero, showQuickNotification);
					}
				}
			}
		}
		if (victim.Clan != null && !victim.Clan.IsEliminated && !victim.Clan.IsBanditFaction && victim.Clan != Clan.PlayerClan)
		{
			if (victim.Clan.Leader == victim)
			{
				DestroyClanAction.ApplyByClanLeaderDeath(victim.Clan);
			}
			else if (victim.Clan.Leader == null)
			{
				DestroyClanAction.Apply(victim.Clan);
			}
		}
		CampaignEventDispatcher.Instance.OnHeroKilled(victim, killer, actionDetail, showNotification);
		if (victim.Spouse != null)
		{
			victim.Spouse = null;
		}
		if (victim.CompanionOf != null)
		{
			RemoveCompanionAction.ApplyByDeath(victim.CompanionOf, victim);
		}
		if (victim.CurrentSettlement != null)
		{
			if (victim.CurrentSettlement == Settlement.CurrentSettlement)
			{
				LocationComplex.Current?.RemoveCharacterIfExists(victim);
			}
			if (victim.StayingInSettlement != null)
			{
				victim.StayingInSettlement = null;
			}
		}
	}

	public static void ApplyByOldAge(Hero victim, bool showNotification = true)
	{
		ApplyInternal(victim, null, KillCharacterActionDetail.DiedOfOldAge, showNotification);
	}

	public static void ApplyByWounds(Hero victim, bool showNotification = true)
	{
		ApplyInternal(victim, null, KillCharacterActionDetail.WoundedInBattle, showNotification);
	}

	public static void ApplyByBattle(Hero victim, Hero killer, bool showNotification = true)
	{
		ApplyInternal(victim, killer, KillCharacterActionDetail.DiedInBattle, showNotification);
	}

	public static void ApplyByMurder(Hero victim, Hero killer = null, bool showNotification = true)
	{
		ApplyInternal(victim, killer, KillCharacterActionDetail.Murdered, showNotification);
	}

	public static void ApplyInLabor(Hero lostMother, bool showNotification = true)
	{
		ApplyInternal(lostMother, null, KillCharacterActionDetail.DiedInLabor, showNotification);
	}

	public static void ApplyByExecution(Hero victim, Hero executer, bool showNotification = true, bool isForced = false)
	{
		ApplyInternal(victim, executer, KillCharacterActionDetail.Executed, showNotification, isForced);
	}

	public static void ApplyByRemove(Hero victim, bool showNotification = false, bool isForced = true)
	{
		ApplyInternal(victim, null, KillCharacterActionDetail.Lost, showNotification, isForced);
	}

	public static void ApplyByDeathMark(Hero victim, bool showNotification = false)
	{
		ApplyInternal(victim, victim.DeathMarkKillerHero, victim.DeathMark, showNotification);
	}

	public static void ApplyByDeathMarkForced(Hero victim, bool showNotification = false)
	{
		ApplyInternal(victim, victim.DeathMarkKillerHero, victim.DeathMark, showNotification, isForced: true);
	}

	public static void ApplyByPlayerIllness()
	{
		ApplyInternal(Hero.MainHero, null, KillCharacterActionDetail.DiedOfOldAge, showNotification: true, isForced: true);
	}

	private static void MakeDead(Hero victim, bool disbandVictimParty = true)
	{
		victim.ChangeState(Hero.CharacterStates.Dead);
		victim.DeathDay = CampaignTime.Now;
		if (!victim.IsHumanPlayerCharacter)
		{
			victim.OnDeath();
		}
		if (victim.PartyBelongedToAsPrisoner != null)
		{
			EndCaptivityAction.ApplyByDeath(victim);
		}
		if (victim.PartyBelongedTo != null)
		{
			MobileParty partyBelongedTo = victim.PartyBelongedTo;
			if (partyBelongedTo.LeaderHero == victim)
			{
				bool flag = false;
				if (!partyBelongedTo.IsMainParty)
				{
					foreach (TroopRosterElement item in partyBelongedTo.MemberRoster.GetTroopRoster())
					{
						if (item.Character.IsHero && item.Character != victim.CharacterObject)
						{
							partyBelongedTo.ChangePartyLeader(item.Character.HeroObject);
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					if (!partyBelongedTo.IsMainParty)
					{
						partyBelongedTo.RemovePartyLeader();
					}
					if (partyBelongedTo.IsActive && disbandVictimParty)
					{
						if (partyBelongedTo.Party.Owner?.CompanionOf == Clan.PlayerClan)
						{
							partyBelongedTo.Party.SetCustomOwner(Hero.MainHero);
						}
						partyBelongedTo.MemberRoster.RemoveTroop(victim.CharacterObject);
						DisbandPartyAction.StartDisband(partyBelongedTo);
					}
				}
			}
			if (victim.PartyBelongedTo != null)
			{
				partyBelongedTo.MemberRoster.RemoveTroop(victim.CharacterObject);
			}
			if (partyBelongedTo.IsActive && partyBelongedTo.MemberRoster.TotalManCount == 0)
			{
				DestroyPartyAction.Apply(null, partyBelongedTo);
			}
		}
		else if (victim.IsHumanPlayerCharacter && !MobileParty.MainParty.IsActive)
		{
			DestroyPartyAction.Apply(null, MobileParty.MainParty);
		}
	}

	private static Clan SelectHeirClanForKingdom(Kingdom kingdom, bool exceptRulingClan)
	{
		Clan rulingClan = kingdom.RulingClan;
		Clan result = null;
		float num = 0f;
		foreach (Clan item in kingdom.Clans.Where((Clan t) => t.Heroes.Any((Hero h) => h.IsAlive) && !t.IsMinorFaction && t != rulingClan))
		{
			float clanStrength = Campaign.Current.Models.DiplomacyModel.GetClanStrength(item);
			if (num <= clanStrength)
			{
				num = clanStrength;
				result = item;
			}
		}
		return result;
	}

	private static TextObject CreateObituary(Hero hero, KillCharacterActionDetail detail)
	{
		TextObject textObject;
		if (hero.IsLord)
		{
			if (hero.Clan != null && hero.Clan.IsMinorFaction)
			{
				textObject = new TextObject("{=L7qd6qfv}{CHARACTER.FIRSTNAME} was a member of the {CHARACTER.FACTION}. {FURTHER_DETAILS}.");
			}
			else
			{
				textObject = new TextObject("{=mfYzCeGR}{CHARACTER.NAME} was {TITLE} of the {CHARACTER_FACTION_SHORT}. {FURTHER_DETAILS}.");
				textObject.SetTextVariable("CHARACTER_FACTION_SHORT", hero.MapFaction.InformalName);
				textObject.SetTextVariable("TITLE", HeroHelper.GetTitleInIndefiniteCase(hero));
			}
		}
		else if (hero.HomeSettlement != null)
		{
			textObject = new TextObject("{=YNXK352h}{CHARACTER.NAME} was a prominent {.%}{PROFESSION}{.%} from {HOMETOWN}. {FURTHER_DETAILS}.");
			textObject.SetTextVariable("PROFESSION", HeroHelper.GetCharacterTypeName(hero));
			textObject.SetTextVariable("HOMETOWN", hero.HomeSettlement.Name);
		}
		else
		{
			textObject = new TextObject("{=!}{FURTHER_DETAILS}.");
		}
		StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, textObject, includeDetails: true);
		TextObject empty = TextObject.Empty;
		empty = detail switch
		{
			KillCharacterActionDetail.DiedInBattle => new TextObject("{=6pCABUme}{?CHARACTER.GENDER}She{?}He{\\?} died in battle in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"), 
			KillCharacterActionDetail.DiedInLabor => new TextObject("{=7Vw6iYNI}{?CHARACTER.GENDER}She{?}He{\\?} died in childbirth in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"), 
			KillCharacterActionDetail.Executed => new TextObject("{=9Tq3IAiz}{?CHARACTER.GENDER}She{?}He{\\?} was executed in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"), 
			KillCharacterActionDetail.Lost => new TextObject("{=SausWqM5}{?CHARACTER.GENDER}She{?}He{\\?} disappeared in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"), 
			KillCharacterActionDetail.Murdered => new TextObject("{=TUDAvcTR}{?CHARACTER.GENDER}She{?}He{\\?} was assassinated in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"), 
			KillCharacterActionDetail.WoundedInBattle => new TextObject("{=LsBCQtVX}{?CHARACTER.GENDER}She{?}He{\\?} died of war-wounds in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"), 
			_ => new TextObject("{=HU5n5KTW}{?CHARACTER.GENDER}She{?}He{\\?} died of natural causes in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"), 
		};
		StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, empty, includeDetails: true);
		empty.SetTextVariable("REPUTATION", CharacterHelper.GetReputationDescription(hero.CharacterObject));
		empty.SetTextVariable("YEAR", CampaignTime.Now.GetYear.ToString());
		textObject.SetTextVariable("FURTHER_DETAILS", empty);
		return textObject;
	}
}
