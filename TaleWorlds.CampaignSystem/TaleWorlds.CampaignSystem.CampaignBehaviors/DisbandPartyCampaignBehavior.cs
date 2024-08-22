using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class DisbandPartyCampaignBehavior : CampaignBehaviorBase, IDisbandPartyCampaignBehavior, ICampaignBehavior
{
	private const int DisbandDelayTimeAsHours = 24;

	private const int RemoveDisbandingPartyAfterHoldForHours = 3;

	private const int DisbandPartySizeLimitForAIParties = 10;

	private Dictionary<MobileParty, CampaignTime> _partiesThatWaitingToDisband = new Dictionary<MobileParty, CampaignTime>();

	public override void RegisterEvents()
	{
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoadFinished);
		CampaignEvents.OnPartyDisbandStartedEvent.AddNonSerializedListener(this, OnPartyDisbandStarted);
		CampaignEvents.OnPartyDisbandCanceledEvent.AddNonSerializedListener(this, OnPartyDisbandCanceled);
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HourlyTick);
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, OnMobilePartyDestroyed);
		CampaignEvents.OnHeroTeleportationRequestedEvent.AddNonSerializedListener(this, OnHeroTeleportationRequested);
		CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this, OnHeroPrisonerTaken);
		CampaignEvents.OnPartyDisbandedEvent.AddNonSerializedListener(this, OnPartyDisbanded);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
		CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener(this, DailyTickParty);
		CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
		CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, HourlyTickParty);
	}

	public bool IsPartyWaitingForDisband(MobileParty party)
	{
		return _partiesThatWaitingToDisband.ContainsKey(party);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_partiesThatWaitingToDisband", ref _partiesThatWaitingToDisband);
	}

	private void OnGameLoadFinished()
	{
		foreach (Kingdom item in Kingdom.All)
		{
			for (int num = item.Armies.Count - 1; num >= 0; num--)
			{
				Army army = item.Armies[num];
				for (int num2 = army.Parties.Count - 1; num2 >= 0; num2--)
				{
					MobileParty mobileParty = army.Parties[num2];
					if (army.LeaderParty != mobileParty && mobileParty.LeaderHero == null)
					{
						DisbandPartyAction.StartDisband(mobileParty);
						mobileParty.Army = null;
					}
				}
				if (army.LeaderParty.LeaderHero == null)
				{
					DisbandPartyAction.StartDisband(army.LeaderParty);
				}
			}
		}
	}

	private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddDialogs(campaignGameStarter);
	}

	private void OnPartyDisbandStarted(MobileParty party)
	{
		if (party.ActualClan != Clan.PlayerClan && party.MemberRoster.Count >= 10)
		{
			Hero hero = null;
			foreach (Hero hero2 in party.ActualClan.Heroes)
			{
				if (hero2.PartyBelongedTo == null && hero2.IsActive && hero2.DeathMark == KillCharacterAction.KillCharacterActionDetail.None && hero2.CurrentSettlement != null && hero2.GovernorOf == null && (!hero2.CurrentSettlement.IsUnderSiege || !hero2.CurrentSettlement.IsUnderRaid))
				{
					hero = hero2;
					break;
				}
			}
			if (hero != null)
			{
				TeleportHeroAction.ApplyDelayedTeleportToPartyAsPartyLeader(hero, party);
			}
			else
			{
				_partiesThatWaitingToDisband.Add(party, CampaignTime.HoursFromNow(24f));
			}
			return;
		}
		if (party.IsCaravan && party.ActualClan == Clan.PlayerClan)
		{
			party.Ai.SetDoNotMakeNewDecisions(doNotMakeNewDecisions: true);
			GetTargetSettlementForDisbandingPlayerClanCaravan(party, out var targetSettlement);
			if (targetSettlement != null)
			{
				party.Ai.SetMoveGoToSettlement(targetSettlement);
			}
		}
		_partiesThatWaitingToDisband.Add(party, CampaignTime.HoursFromNow(24f));
	}

	private void OnPartyDisbandCanceled(MobileParty party)
	{
		if (_partiesThatWaitingToDisband.ContainsKey(party))
		{
			_partiesThatWaitingToDisband.Remove(party);
		}
	}

	private void HourlyTick()
	{
		List<MobileParty> list = new List<MobileParty>();
		foreach (KeyValuePair<MobileParty, CampaignTime> item in _partiesThatWaitingToDisband)
		{
			if (item.Value.IsPast)
			{
				item.Key.IsDisbanding = true;
				list.Add(item.Key);
			}
		}
		foreach (MobileParty item2 in list)
		{
			_partiesThatWaitingToDisband.Remove(item2);
		}
		list = null;
	}

	private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
	{
		if (_partiesThatWaitingToDisband.ContainsKey(mobileParty))
		{
			_partiesThatWaitingToDisband.Remove(mobileParty);
		}
	}

	private void OnHeroTeleportationRequested(Hero hero, Settlement targetSettlement, MobileParty targetParty, TeleportHeroAction.TeleportationDetail detail)
	{
		if (targetParty != null && detail == TeleportHeroAction.TeleportationDetail.DelayedTeleportToPartyAsPartyLeader && _partiesThatWaitingToDisband.ContainsKey(targetParty))
		{
			_partiesThatWaitingToDisband.Remove(targetParty);
		}
	}

	private void OnHeroPrisonerTaken(PartyBase capturer, Hero prisoner)
	{
		if (prisoner != Hero.MainHero)
		{
			return;
		}
		foreach (WarPartyComponent warPartyComponent in Clan.PlayerClan.WarPartyComponents)
		{
			if (warPartyComponent.MobileParty != null && warPartyComponent.MobileParty.LeaderHero == null)
			{
				CampaignEventDispatcher.Instance.OnPartyLeaderChangeOfferCanceled(warPartyComponent.MobileParty);
			}
		}
	}

	private void DailyTickParty(MobileParty mobileParty)
	{
		if (mobileParty.IsDisbanding && mobileParty.MapEvent == null && mobileParty.IsActive && mobileParty.TargetSettlement != null)
		{
			CheckDisbandedPartyDaily(mobileParty, mobileParty.TargetSettlement);
		}
	}

	private void OnSettlementLeft(MobileParty mobileParty, Settlement settlement)
	{
		if (mobileParty.IsCaravan && mobileParty.ActualClan == Clan.PlayerClan && !mobileParty.IsDisbanding && _partiesThatWaitingToDisband.ContainsKey(mobileParty) && mobileParty.CurrentSettlement == null && mobileParty.TargetSettlement != null)
		{
			GetTargetSettlementForDisbandingPlayerClanCaravan(mobileParty, out var targetSettlement);
			mobileParty.Ai.SetMoveGoToSettlement(targetSettlement);
		}
	}

	private void HourlyTickParty(MobileParty party)
	{
		if (party.Ai.DefaultBehavior == AiBehavior.Hold && party.Ai.DoNotMakeNewDecisions && (party.IsDisbanding || _partiesThatWaitingToDisband.ContainsKey(party)))
		{
			GetTargetSettlementForDisbandingPlayerClanCaravan(party, out var targetSettlement);
			if (targetSettlement != null)
			{
				party.Ai.SetMoveGoToSettlement(targetSettlement);
			}
		}
	}

	private void GetTargetSettlementForDisbandingPlayerClanCaravan(MobileParty mobileParty, out Settlement targetSettlement)
	{
		float num = 0f;
		targetSettlement = null;
		foreach (Settlement settlement2 in mobileParty.MapFaction.Settlements)
		{
			if (settlement2.IsFortification)
			{
				float num2 = CalculateTargetSettlementScore(mobileParty, settlement2);
				if (num2 > num)
				{
					targetSettlement = settlement2;
					num = num2;
				}
			}
		}
		if (targetSettlement != null)
		{
			return;
		}
		int num3 = -1;
		do
		{
			num3 = SettlementHelper.FindNextSettlementAroundMapPoint(mobileParty, 40f, num3);
			if (num3 < 0)
			{
				continue;
			}
			Settlement settlement = Settlement.All[num3];
			if (settlement.OwnerClan != null && !settlement.OwnerClan.IsAtWarWith(mobileParty.MapFaction) && settlement.IsFortification)
			{
				float num4 = CalculateTargetSettlementScore(mobileParty, settlement);
				if (num4 > num)
				{
					targetSettlement = settlement;
					num = num4;
				}
			}
			if (targetSettlement == null)
			{
				targetSettlement = SettlementHelper.FindNearestFortification((Settlement s) => s.OwnerClan != null && !s.OwnerClan.IsAtWarWith(mobileParty.MapFaction));
			}
		}
		while (num3 >= 0);
	}

	private static float CalculateTargetSettlementScore(MobileParty disbandParty, Settlement settlement)
	{
		float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(disbandParty, settlement);
		float num = MathF.Pow(1f - 0.95f * (MathF.Min(Campaign.MapDiagonal, distance) / Campaign.MapDiagonal), 3f);
		float num2 = ((disbandParty.Party.Owner?.Clan == settlement.OwnerClan) ? 1f : ((disbandParty.Party.Owner?.MapFaction == settlement.MapFaction) ? 0.1f : 0.01f));
		float num3 = ((disbandParty.DefaultBehavior == AiBehavior.GoToSettlement && disbandParty.TargetSettlement == settlement) ? 1f : 0.3f);
		return num * num2 * num3;
	}

	private void OnPartyDisbanded(MobileParty disbandParty, Settlement relatedSettlement)
	{
		if (relatedSettlement == null)
		{
			return;
		}
		if (relatedSettlement.IsFortification)
		{
			if (relatedSettlement.Town.GarrisonParty != null && relatedSettlement.Town.GarrisonParty.MapEvent == null)
			{
				MergeDisbandPartyToFortification(disbandParty, relatedSettlement.Town.GarrisonParty.Party, relatedSettlement);
			}
		}
		else if (relatedSettlement.IsVillage && relatedSettlement.Village.VillageState != Village.VillageStates.BeingRaided)
		{
			MergeDisbandPartyToVillage(disbandParty, relatedSettlement);
		}
	}

	private void MergeDisbandPartyToFortification(MobileParty disbandParty, PartyBase mergeToParty, Settlement settlement)
	{
		foreach (TroopRosterElement item in disbandParty.PrisonRoster.GetTroopRoster())
		{
			if (item.Character.IsHero)
			{
				if (!mergeToParty.MapFaction.IsAtWarWith(item.Character.HeroObject.MapFaction))
				{
					EndCaptivityAction.ApplyByPeace(item.Character.HeroObject);
				}
				else
				{
					TransferPrisonerAction.Apply(item.Character, disbandParty.Party, settlement.Party);
				}
			}
			else
			{
				mergeToParty.AddPrisoner(item.Character, item.Number);
			}
		}
		foreach (TroopRosterElement item2 in disbandParty.MemberRoster.GetTroopRoster())
		{
			disbandParty.MemberRoster.RemoveTroop(item2.Character, item2.Number);
			if (item2.Character.IsHero && item2.Character.HeroObject.IsAlive)
			{
				EnterSettlementAction.ApplyForCharacterOnly(item2.Character.HeroObject, settlement);
			}
			else
			{
				mergeToParty.MemberRoster.AddToCounts(item2.Character, item2.Number, insertAtFront: false, item2.WoundedNumber, item2.Xp);
			}
		}
	}

	private void MergeDisbandPartyToVillage(MobileParty disbandParty, Settlement settlement)
	{
		foreach (TroopRosterElement item in disbandParty.PrisonRoster.GetTroopRoster())
		{
			if (item.Character.IsHero)
			{
				EndCaptivityAction.ApplyByEscape(item.Character.HeroObject);
			}
		}
		foreach (TroopRosterElement item2 in disbandParty.MemberRoster.GetTroopRoster())
		{
			if (item2.Character.IsHero && item2.Character.HeroObject.IsAlive)
			{
				disbandParty.MemberRoster.RemoveTroop(item2.Character);
				EnterSettlementAction.ApplyForCharacterOnly(item2.Character.HeroObject, settlement);
			}
		}
		float num = (float)disbandParty.MemberRoster.TotalManCount * 0.5f;
		settlement.Militia += num;
	}

	private void CheckDisbandedPartyDaily(MobileParty disbandParty, Settlement settlement)
	{
		if (disbandParty.MemberRoster.Count == 0)
		{
			DestroyPartyAction.Apply(null, disbandParty);
		}
		else if (settlement == null && disbandParty.StationaryStartTime.ElapsedHoursUntilNow >= 3f)
		{
			foreach (TroopRosterElement item in disbandParty.MemberRoster.GetTroopRoster())
			{
				if (item.Character.IsHero && !item.Character.IsPlayerCharacter && !item.Character.HeroObject.IsDead)
				{
					MakeHeroFugitiveAction.Apply(item.Character.HeroObject);
				}
			}
			DestroyPartyAction.Apply(null, disbandParty);
		}
		else if (settlement != null && settlement == disbandParty.CurrentSettlement)
		{
			DestroyPartyAction.ApplyForDisbanding(disbandParty, settlement);
		}
	}

	private void AddDialogs(CampaignGameStarter campaignGameStarter)
	{
		campaignGameStarter.AddDialogLine("disbanding_leaderless_party_start", "start", "disbanding_leaderless_party_start_response", "{=!}{EXPLANATION}", disbanding_leaderless_party_start_on_condition, null, 500);
		campaignGameStarter.AddPlayerLine("disbanding_leaderless_party_answer_take_party", "disbanding_leaderless_party_start_response", "close_window", "{=eyZo8ZTk}Let me inspect the party troops.", disbanding_leaderless_party_join_main_party_answer_condition, disbanding_leaderless_party_join_main_party_answer_on_consequence);
		campaignGameStarter.AddPlayerLine("disbanding_leaderless_party_answer_attack_neutral", "disbanding_leaderless_party_start_response", "attack_disbanding_party_neutral_response", "{=SXgm2b1M}You're not going anywhere. Not with your valuables, anyway.", attack_neutral_disbanding_party_condition, null);
		campaignGameStarter.AddDialogLine("disbanding_leaderless_party_answer_attack_neutral_di", "attack_disbanding_party_neutral_response", "attack_disbanding_party_neutral_player_response", "{=CgS44dOE}Are you mad? We're not your enemy.", null, null);
		campaignGameStarter.AddPlayerLine("disbanding_leaderless_party_answer_attack_neutral_2", "attack_disbanding_party_neutral_player_response", "close_window", "{=Mt5F4wE2}No, you're my prey. Prepare to fight!", null, attack_disbanding_party_consequence);
		campaignGameStarter.AddPlayerLine("disbanding_leaderless_party_answer_attack_neutral_3", "attack_disbanding_party_neutral_player_response", "close_window", "{=XrQBTVis}I don't know what I was thinking. Go on, then...", null, disbanding_leaderless_party_answer_on_consequence);
		campaignGameStarter.AddPlayerLine("disbanding_leaderless_party_answer_attack_enemy", "disbanding_leaderless_party_start_response", "attack_disbanding_enemy_response", "{=WwLy9Src}You know we're at war. I can't just let you go.", attack_enemy_disbanding_party_condition, null);
		campaignGameStarter.AddDialogLine("disbanding_leaderless_party_answer", "attack_disbanding_enemy_response", "close_window", "{=jBN2LlgF}We'll fight to our last drop of blood!", null, attack_disbanding_party_consequence);
		campaignGameStarter.AddPlayerLine("disbanding_leaderless_party_answer_2", "disbanding_leaderless_party_start_response", "close_window", "{=disband_party_campaign_behaviorbdisbanding_leaderless_party_answer}Well... Go on, then.", null, disbanding_leaderless_party_answer_on_consequence);
	}

	private bool disbanding_leaderless_party_start_on_condition()
	{
		int num;
		if (MobileParty.ConversationParty != null && MobileParty.ConversationParty.IsLordParty)
		{
			if (MobileParty.ConversationParty.LeaderHero != null && !MobileParty.ConversationParty.IsDisbanding)
			{
				num = (IsPartyWaitingForDisband(MobileParty.ConversationParty) ? 1 : 0);
				if (num == 0)
				{
					goto IL_00fb;
				}
			}
			else
			{
				num = 1;
			}
			if (MobileParty.ConversationParty.LeaderHero == null)
			{
				if (MobileParty.ConversationParty.TargetSettlement != null)
				{
					TextObject textObject = new TextObject("{=9IwzVbJf}We recently lost our leader, now we are traveling to {TARGET_SETTLEMENT}. We will rejoin the garrison unless we are assigned a new leader.");
					textObject.SetTextVariable("TARGET_SETTLEMENT", MobileParty.ConversationParty.TargetSettlement.EncyclopediaLinkWithName);
					MBTextManager.SetTextVariable("EXPLANATION", textObject);
					return (byte)num != 0;
				}
				MBTextManager.SetTextVariable("EXPLANATION", new TextObject("{=COEifaao}We recently lost our leader. We are now waiting for new orders."));
				return (byte)num != 0;
			}
			if (MobileParty.ConversationParty.TargetSettlement != null)
			{
				TextObject textObject2 = new TextObject("{=uZIlfFa2}We're disbanding. We're all going to {TARGET_SETTLEMENT_LINK}, then we're going our separate ways.");
				textObject2.SetTextVariable("TARGET_SETTLEMENT_LINK", MobileParty.ConversationParty.TargetSettlement.EncyclopediaLinkWithName);
				MBTextManager.SetTextVariable("EXPLANATION", textObject2);
				return (byte)num != 0;
			}
			MBTextManager.SetTextVariable("EXPLANATION", new TextObject("{=G1PN6ku4}We're disbanding."));
		}
		else
		{
			num = 0;
		}
		goto IL_00fb;
		IL_00fb:
		return (byte)num != 0;
	}

	private bool disbanding_leaderless_party_join_main_party_answer_condition()
	{
		if (MobileParty.ConversationParty != null && MobileParty.ConversationParty.Party.Owner != null)
		{
			return MobileParty.ConversationParty.Party.Owner.Clan == Clan.PlayerClan;
		}
		return false;
	}

	private void disbanding_leaderless_party_join_main_party_answer_on_consequence()
	{
		PlayerEncounter.LeaveEncounter = true;
		PartyScreenManager.OpenScreenAsManageTroopsAndPrisoners(MobileParty.ConversationParty, OnPartyScreenClosed);
	}

	private void OnPartyScreenClosed(PartyBase leftOwnerParty, TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, PartyBase rightOwnerParty, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, bool fromCancel)
	{
		if (leftOwnerParty.MemberRoster.TotalManCount <= 0)
		{
			DestroyPartyAction.Apply(null, leftOwnerParty.MobileParty);
		}
	}

	private void disbanding_leaderless_party_answer_on_consequence()
	{
		PlayerEncounter.LeaveEncounter = true;
	}

	private bool attack_neutral_disbanding_party_condition()
	{
		if (MobileParty.ConversationParty != null && MobileParty.ConversationParty.MapFaction != Clan.PlayerClan.MapFaction)
		{
			return !FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, MobileParty.ConversationParty.MapFaction);
		}
		return false;
	}

	private bool attack_enemy_disbanding_party_condition()
	{
		if (MobileParty.ConversationParty != null && MobileParty.ConversationParty.MapFaction != Clan.PlayerClan.MapFaction)
		{
			return FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, MobileParty.ConversationParty.MapFaction);
		}
		return false;
	}

	private void attack_disbanding_party_consequence()
	{
		PlayerEncounter.Current.IsEnemy = true;
		BeHostileAction.ApplyEncounterHostileAction(PartyBase.MainParty, MobileParty.ConversationParty.Party);
	}
}
