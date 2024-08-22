using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;

namespace TaleWorlds.CampaignSystem.Actions;

public static class ChangeKingdomAction
{
	public enum ChangeKingdomActionDetail
	{
		JoinAsMercenary,
		JoinKingdom,
		JoinKingdomByDefection,
		LeaveKingdom,
		LeaveWithRebellion,
		LeaveAsMercenary,
		LeaveByClanDestruction,
		CreateKingdom,
		LeaveByKingdomDestruction
	}

	public const float PotentialSettlementsPerNobleEffect = 0.2f;

	public const float NewGainedFiefsValueForKingdomConstant = 0.1f;

	public const float LordsUnitStrengthValue = 20f;

	public const float MercenaryUnitStrengthValue = 5f;

	public const float MinimumNeededGoldForRecruitingMercenaries = 20000f;

	private static void ApplyInternal(Clan clan, Kingdom newKingdom, ChangeKingdomActionDetail detail, int awardMultiplier = 0, bool byRebellion = false, bool showNotification = true)
	{
		Kingdom kingdom = clan.Kingdom;
		_ = PlayerSiege.PlayerSiegeEvent;
		_ = MobileParty.MainParty.MapEvent;
		_ = PlayerEncounter.Current;
		clan.DebtToKingdom = 0;
		if (detail == ChangeKingdomActionDetail.JoinKingdom || detail == ChangeKingdomActionDetail.JoinAsMercenary || detail == ChangeKingdomActionDetail.JoinKingdomByDefection)
		{
			FactionHelper.AdjustFactionStancesForClanJoiningKingdom(clan, newKingdom);
		}
		switch (detail)
		{
		case ChangeKingdomActionDetail.JoinKingdom:
		case ChangeKingdomActionDetail.JoinKingdomByDefection:
		case ChangeKingdomActionDetail.CreateKingdom:
			if (clan.IsUnderMercenaryService)
			{
				clan.EndMercenaryService(isByLeavingKingdom: false);
			}
			if (kingdom != null)
			{
				clan.ClanLeaveKingdom(!byRebellion);
			}
			clan.Kingdom = newKingdom;
			if (newKingdom != null && detail == ChangeKingdomActionDetail.CreateKingdom)
			{
				ChangeRulingClanAction.Apply(newKingdom, clan);
			}
			break;
		case ChangeKingdomActionDetail.JoinAsMercenary:
			if (clan.IsUnderMercenaryService)
			{
				clan.EndMercenaryService(isByLeavingKingdom: true);
			}
			clan.MercenaryAwardMultiplier = awardMultiplier;
			clan.Kingdom = newKingdom;
			clan.StartMercenaryService();
			if (clan == Clan.PlayerClan)
			{
				Campaign.Current.KingdomManager.PlayerMercenaryServiceNextRenewDay = Campaign.CurrentTime + 720f;
			}
			break;
		case ChangeKingdomActionDetail.LeaveKingdom:
		case ChangeKingdomActionDetail.LeaveWithRebellion:
		case ChangeKingdomActionDetail.LeaveAsMercenary:
		case ChangeKingdomActionDetail.LeaveByClanDestruction:
		case ChangeKingdomActionDetail.LeaveByKingdomDestruction:
			clan.Kingdom = null;
			if (clan.IsUnderMercenaryService)
			{
				clan.EndMercenaryService(isByLeavingKingdom: true);
			}
			switch (detail)
			{
			case ChangeKingdomActionDetail.LeaveWithRebellion:
				if (clan != Clan.PlayerClan)
				{
					break;
				}
				foreach (Clan clan2 in kingdom.Clans)
				{
					ChangeRelationAction.ApplyRelationChangeBetweenHeroes(clan.Leader, clan2.Leader, -40);
				}
				DeclareWarAction.ApplyByRebellion(kingdom, clan);
				break;
			case ChangeKingdomActionDetail.LeaveKingdom:
				if (clan == Clan.PlayerClan && !clan.IsEliminated)
				{
					foreach (Clan clan3 in kingdom.Clans)
					{
						ChangeRelationAction.ApplyRelationChangeBetweenHeroes(clan.Leader, clan3.Leader, -20);
					}
				}
				foreach (Settlement item in new List<Settlement>(clan.Settlements))
				{
					ChangeOwnerOfSettlementAction.ApplyByLeaveFaction(kingdom.Leader, item);
					foreach (Hero item2 in new List<Hero>(item.HeroesWithoutParty))
					{
						if (item2.CurrentSettlement != null && item2.Clan == clan)
						{
							if (item2.PartyBelongedTo != null)
							{
								LeaveSettlementAction.ApplyForParty(item2.PartyBelongedTo);
								EnterSettlementAction.ApplyForParty(item2.PartyBelongedTo, clan.Leader.HomeSettlement);
							}
							else
							{
								LeaveSettlementAction.ApplyForCharacterOnly(item2);
								EnterSettlementAction.ApplyForCharacterOnly(item2, clan.Leader.HomeSettlement);
							}
						}
					}
				}
				break;
			case ChangeKingdomActionDetail.LeaveByKingdomDestruction:
				foreach (StanceLink stance in kingdom.Stances)
				{
					if (stance.IsAtWar && !stance.IsAtConstantWar)
					{
						IFaction faction = ((stance.Faction1 == kingdom) ? stance.Faction2 : stance.Faction1);
						if (faction != clan && !clan.GetStanceWith(faction).IsAtWar)
						{
							DeclareWarAction.ApplyByDefault(clan, faction);
						}
					}
				}
				break;
			}
			break;
		}
		if (detail == ChangeKingdomActionDetail.LeaveAsMercenary || detail == ChangeKingdomActionDetail.LeaveKingdom || detail == ChangeKingdomActionDetail.LeaveWithRebellion)
		{
			foreach (StanceLink item3 in new List<StanceLink>(clan.Stances))
			{
				if (item3.IsAtWar && !item3.IsAtConstantWar)
				{
					IFaction faction2 = ((item3.Faction1 == clan) ? item3.Faction2 : item3.Faction1);
					if (detail != ChangeKingdomActionDetail.LeaveWithRebellion || clan != Clan.PlayerClan || faction2 != kingdom)
					{
						MakePeaceAction.Apply(clan, faction2);
						FactionHelper.FinishAllRelatedHostileActionsOfFactionToFaction(clan, faction2);
						FactionHelper.FinishAllRelatedHostileActionsOfFactionToFaction(faction2, clan);
					}
				}
			}
			CheckIfPartyIconIsDirty(clan, kingdom);
		}
		foreach (WarPartyComponent warPartyComponent in clan.WarPartyComponents)
		{
			if (warPartyComponent.MobileParty.MapEvent == null)
			{
				warPartyComponent.MobileParty.Ai.SetMoveModeHold();
			}
		}
		CampaignEventDispatcher.Instance.OnClanChangedKingdom(clan, kingdom, newKingdom, detail, showNotification);
	}

	public static void ApplyByJoinToKingdom(Clan clan, Kingdom newKingdom, bool showNotification = true)
	{
		ApplyInternal(clan, newKingdom, ChangeKingdomActionDetail.JoinKingdom, 0, byRebellion: false, showNotification);
	}

	public static void ApplyByJoinToKingdomByDefection(Clan clan, Kingdom newKingdom, bool showNotification = true)
	{
		ApplyInternal(clan, newKingdom, ChangeKingdomActionDetail.JoinKingdomByDefection, 0, byRebellion: false, showNotification);
	}

	public static void ApplyByCreateKingdom(Clan clan, Kingdom newKingdom, bool showNotification = true)
	{
		ApplyInternal(clan, newKingdom, ChangeKingdomActionDetail.CreateKingdom, 0, byRebellion: false, showNotification);
	}

	public static void ApplyByLeaveByKingdomDestruction(Clan clan, bool showNotification = true)
	{
		ApplyInternal(clan, null, ChangeKingdomActionDetail.LeaveByKingdomDestruction, 0, byRebellion: false, showNotification);
	}

	public static void ApplyByLeaveKingdom(Clan clan, bool showNotification = true)
	{
		ApplyInternal(clan, null, ChangeKingdomActionDetail.LeaveKingdom, 0, byRebellion: false, showNotification);
	}

	public static void ApplyByLeaveWithRebellionAgainstKingdom(Clan clan, bool showNotification = true)
	{
		ApplyInternal(clan, null, ChangeKingdomActionDetail.LeaveWithRebellion, 0, byRebellion: false, showNotification);
	}

	public static void ApplyByJoinFactionAsMercenary(Clan clan, Kingdom newKingdom, int awardMultiplier = 50, bool showNotification = true)
	{
		ApplyInternal(clan, newKingdom, ChangeKingdomActionDetail.JoinAsMercenary, awardMultiplier, byRebellion: false, showNotification);
	}

	public static void ApplyByLeaveKingdomAsMercenary(Clan mercenaryClan, bool showNotification = true)
	{
		ApplyInternal(mercenaryClan, null, ChangeKingdomActionDetail.LeaveAsMercenary, 0, byRebellion: false, showNotification);
	}

	public static void ApplyByLeaveKingdomByClanDestruction(Clan clan, bool showNotification = true)
	{
		ApplyInternal(clan, null, ChangeKingdomActionDetail.LeaveByClanDestruction, 0, byRebellion: false, showNotification);
	}

	private static void CheckIfPartyIconIsDirty(Clan clan, Kingdom oldKingdom)
	{
		IFaction faction2;
		IFaction faction;
		if (clan.Kingdom == null)
		{
			faction = clan;
			faction2 = faction;
		}
		else
		{
			faction = clan.Kingdom;
			faction2 = faction;
		}
		IFaction faction3 = faction2;
		faction = oldKingdom;
		IFaction faction4 = faction ?? clan;
		foreach (MobileParty item in MobileParty.All)
		{
			if (item.IsVisible && ((item.Party.Owner != null && item.Party.Owner.Clan == clan) || (clan == Clan.PlayerClan && ((!FactionManager.IsAtWarAgainstFaction(item.MapFaction, faction3) && FactionManager.IsAtWarAgainstFaction(item.MapFaction, faction4)) || (FactionManager.IsAtWarAgainstFaction(item.MapFaction, faction3) && !FactionManager.IsAtWarAgainstFaction(item.MapFaction, faction4))))))
			{
				item.Party.SetVisualAsDirty();
			}
		}
		foreach (Settlement settlement in clan.Settlements)
		{
			settlement.Party.SetVisualAsDirty();
		}
	}
}
