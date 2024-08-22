using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class KingdomDecisionProposalBehavior : CampaignBehaviorBase
{
	private delegate KingdomDecision KingdomDecisionCreatorDelegate(Clan sponsorClan);

	private const int KingdomDecisionProposalCooldownInDays = 1;

	private const float ClanInterestModifier = 1f;

	private const float DecisionSuccessChanceModifier = 1f;

	private List<KingdomDecision> _kingdomDecisionsList;

	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, SessionLaunched);
		CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, DailyTickClan);
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HourlyTick);
		CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
		CampaignEvents.MakePeace.AddNonSerializedListener(this, OnPeaceMade);
		CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
		CampaignEvents.KingdomDestroyedEvent.AddNonSerializedListener(this, OnKingdomDestroyed);
	}

	private void OnKingdomDestroyed(Kingdom kingdom)
	{
		UpdateKingdomDecisions(kingdom);
	}

	private void DailyTickClan(Clan clan)
	{
		if ((float)(int)Campaign.Current.CampaignStartTime.ElapsedDaysUntilNow < 5f || clan.IsEliminated || clan == Clan.PlayerClan || clan.TotalStrength <= 0f || clan.IsBanditFaction || clan.Kingdom == null || clan.Influence < 100f)
		{
			return;
		}
		KingdomDecision kingdomDecision = null;
		float randomFloat = MBRandom.RandomFloat;
		int num = ((Kingdom)clan.MapFaction).Clans.Count((Clan x) => x.Influence > 100f);
		float num2 = MathF.Min(0.33f, 1f / ((float)num + 2f));
		num2 *= ((clan.Kingdom != Hero.MainHero.MapFaction || Hero.MainHero.Clan.IsUnderMercenaryService) ? 1f : ((clan.Kingdom.Leader == Hero.MainHero) ? 0.5f : 0.75f));
		DiplomacyModel diplomacyModel = Campaign.Current.Models.DiplomacyModel;
		if (randomFloat < num2 && clan.Influence > (float)diplomacyModel.GetInfluenceCostOfProposingPeace(clan))
		{
			kingdomDecision = GetRandomPeaceDecision(clan);
		}
		else if (randomFloat < num2 * 2f && clan.Influence > (float)diplomacyModel.GetInfluenceCostOfProposingWar(clan))
		{
			kingdomDecision = GetRandomWarDecision(clan);
		}
		else if (randomFloat < num2 * 2.5f && clan.Influence > (float)(diplomacyModel.GetInfluenceCostOfPolicyProposalAndDisavowal(clan) * 4))
		{
			kingdomDecision = GetRandomPolicyDecision(clan);
		}
		else if (randomFloat < num2 * 3f && clan.Influence > 700f)
		{
			kingdomDecision = GetRandomAnnexationDecision(clan);
		}
		if (kingdomDecision != null)
		{
			if (_kingdomDecisionsList == null)
			{
				_kingdomDecisionsList = new List<KingdomDecision>();
			}
			bool flag = false;
			if (kingdomDecision is MakePeaceKingdomDecision && ((MakePeaceKingdomDecision)kingdomDecision).FactionToMakePeaceWith == Hero.MainHero.MapFaction)
			{
				foreach (KingdomDecision kingdomDecisions in _kingdomDecisionsList)
				{
					if (kingdomDecisions is MakePeaceKingdomDecision && kingdomDecisions.Kingdom == Hero.MainHero.MapFaction && ((MakePeaceKingdomDecision)kingdomDecisions).FactionToMakePeaceWith == clan.Kingdom && kingdomDecisions.TriggerTime.IsFuture)
					{
						flag = true;
					}
					if (kingdomDecisions is MakePeaceKingdomDecision && kingdomDecisions.Kingdom == clan.Kingdom && ((MakePeaceKingdomDecision)kingdomDecisions).FactionToMakePeaceWith == Hero.MainHero.MapFaction && kingdomDecisions.TriggerTime.IsFuture)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				return;
			}
			bool flag2 = false;
			foreach (KingdomDecision kingdomDecisions2 in _kingdomDecisionsList)
			{
				if (kingdomDecisions2 is DeclareWarDecision declareWarDecision && kingdomDecision is DeclareWarDecision declareWarDecision2 && declareWarDecision.FactionToDeclareWarOn == declareWarDecision2.FactionToDeclareWarOn && declareWarDecision.ProposerClan.MapFaction == declareWarDecision2.ProposerClan.MapFaction)
				{
					flag2 = true;
				}
				else if (kingdomDecisions2 is MakePeaceKingdomDecision makePeaceKingdomDecision && kingdomDecision is MakePeaceKingdomDecision makePeaceKingdomDecision2 && makePeaceKingdomDecision.FactionToMakePeaceWith == makePeaceKingdomDecision2.FactionToMakePeaceWith && makePeaceKingdomDecision.ProposerClan.MapFaction == makePeaceKingdomDecision2.ProposerClan.MapFaction)
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				_kingdomDecisionsList.Add(kingdomDecision);
				new KingdomElection(kingdomDecision);
				clan.Kingdom.AddDecision(kingdomDecision);
			}
		}
		else
		{
			UpdateKingdomDecisions(clan.Kingdom);
		}
	}

	private void HourlyTick()
	{
		if (Clan.PlayerClan.Kingdom != null)
		{
			UpdateKingdomDecisions(Clan.PlayerClan.Kingdom);
		}
	}

	private void DailyTick()
	{
		if (_kingdomDecisionsList == null)
		{
			return;
		}
		int count = _kingdomDecisionsList.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (_kingdomDecisionsList[i - num].TriggerTime.ElapsedDaysUntilNow > 15f)
			{
				_kingdomDecisionsList.RemoveAt(i - num);
				num++;
			}
		}
	}

	public void UpdateKingdomDecisions(Kingdom kingdom)
	{
		List<KingdomDecision> list = new List<KingdomDecision>();
		List<KingdomDecision> list2 = new List<KingdomDecision>();
		foreach (KingdomDecision unresolvedDecision in kingdom.UnresolvedDecisions)
		{
			if (unresolvedDecision.ShouldBeCancelled())
			{
				list.Add(unresolvedDecision);
			}
			else if (unresolvedDecision.TriggerTime.IsPast && !unresolvedDecision.NeedsPlayerResolution)
			{
				list2.Add(unresolvedDecision);
			}
		}
		foreach (KingdomDecision item in list)
		{
			kingdom.RemoveDecision(item);
			bool isPlayerInvolved = item.DetermineChooser().Leader.IsHumanPlayerCharacter || item.DetermineSupporters().Any((Supporter x) => x.IsPlayer);
			CampaignEventDispatcher.Instance.OnKingdomDecisionCancelled(item, isPlayerInvolved);
		}
		foreach (KingdomDecision item2 in list2)
		{
			new KingdomElection(item2).StartElectionWithoutPlayer();
		}
	}

	private void OnPeaceMade(IFaction side1Faction, IFaction side2Faction, MakePeaceAction.MakePeaceDetail detail)
	{
		HandleDiplomaticChangeBetweenFactions(side1Faction, side2Faction);
	}

	private void OnWarDeclared(IFaction side1Faction, IFaction side2Faction, DeclareWarAction.DeclareWarDetail detail)
	{
		HandleDiplomaticChangeBetweenFactions(side1Faction, side2Faction);
	}

	private void HandleDiplomaticChangeBetweenFactions(IFaction side1Faction, IFaction side2Faction)
	{
		if (side1Faction.IsKingdomFaction && side2Faction.IsKingdomFaction)
		{
			UpdateKingdomDecisions((Kingdom)side1Faction);
			UpdateKingdomDecisions((Kingdom)side2Faction);
		}
	}

	private KingdomDecision GetRandomWarDecision(Clan clan)
	{
		KingdomDecision result = null;
		Kingdom kingdom = clan.Kingdom;
		if (kingdom.UnresolvedDecisions.FirstOrDefault((KingdomDecision x) => x is DeclareWarDecision) != null)
		{
			return null;
		}
		Kingdom randomElementWithPredicate = Kingdom.All.GetRandomElementWithPredicate((Kingdom x) => !x.IsEliminated && x != kingdom && !x.IsAtWarWith(kingdom) && x.GetStanceWith(kingdom).PeaceDeclarationDate.ElapsedDaysUntilNow > 20f);
		if (randomElementWithPredicate != null && ConsiderWar(clan, kingdom, randomElementWithPredicate))
		{
			result = new DeclareWarDecision(clan, randomElementWithPredicate);
		}
		return result;
	}

	private KingdomDecision GetRandomPeaceDecision(Clan clan)
	{
		KingdomDecision result = null;
		Kingdom kingdom = clan.Kingdom;
		if (kingdom.UnresolvedDecisions.FirstOrDefault((KingdomDecision x) => x is MakePeaceKingdomDecision) != null)
		{
			return null;
		}
		Kingdom randomElementWithPredicate = Kingdom.All.GetRandomElementWithPredicate((Kingdom x) => x.IsAtWarWith(kingdom));
		if (randomElementWithPredicate != null && ConsiderPeace(clan, randomElementWithPredicate.RulingClan, kingdom, randomElementWithPredicate, out var decision))
		{
			result = decision;
		}
		return result;
	}

	private bool ConsiderWar(Clan clan, Kingdom kingdom, IFaction otherFaction)
	{
		int num = Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfProposingWar(clan) / 2;
		if (clan.Influence < (float)num)
		{
			return false;
		}
		DeclareWarDecision declareWarDecision = new DeclareWarDecision(clan, otherFaction);
		if (declareWarDecision.CalculateSupport(clan) > 50f)
		{
			float kingdomSupportForDecision = GetKingdomSupportForDecision(declareWarDecision);
			if (MBRandom.RandomFloat < 1.4f * kingdomSupportForDecision - 0.55f)
			{
				return true;
			}
		}
		return false;
	}

	private float GetKingdomSupportForWar(Clan clan, Kingdom kingdom, IFaction otherFaction)
	{
		return new KingdomElection(new DeclareWarDecision(clan, otherFaction)).GetLikelihoodForSponsor(clan);
	}

	private bool ConsiderPeace(Clan clan, Clan otherClan, Kingdom kingdom, IFaction otherFaction, out MakePeaceKingdomDecision decision)
	{
		decision = null;
		int influenceCostOfProposingPeace = Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfProposingPeace(clan);
		if (clan.Influence < (float)influenceCostOfProposingPeace)
		{
			return false;
		}
		int num = new PeaceBarterable(clan.Leader, kingdom, otherFaction, CampaignTime.Years(1f)).GetValueForFaction(otherFaction);
		int num2 = -num;
		if (clan.MapFaction == Hero.MainHero.MapFaction && otherFaction is Kingdom)
		{
			foreach (Clan clan2 in ((Kingdom)otherFaction).Clans)
			{
				if (clan2.Leader != clan2.MapFaction.Leader)
				{
					int valueForFaction = new PeaceBarterable(clan2.Leader, kingdom, otherFaction, CampaignTime.Years(1f)).GetValueForFaction(clan2);
					if (valueForFaction < num)
					{
						num = valueForFaction;
					}
				}
			}
			num2 = -num;
		}
		else
		{
			num2 += 30000;
		}
		if (otherFaction is Clan && num2 < 0)
		{
			num2 = 0;
		}
		float num3 = 0.5f;
		if (otherFaction == Hero.MainHero.MapFaction)
		{
			PeaceBarterable peaceBarterable = new PeaceBarterable(clan.MapFaction.Leader, kingdom, otherFaction, CampaignTime.Years(1f));
			int num4 = peaceBarterable.GetValueForFaction(clan.MapFaction);
			int num5 = 0;
			int num6 = 1;
			if (clan.MapFaction is Kingdom)
			{
				foreach (Clan clan3 in ((Kingdom)clan.MapFaction).Clans)
				{
					if (clan3.Leader != clan3.MapFaction.Leader)
					{
						int valueForFaction2 = peaceBarterable.GetValueForFaction(clan3);
						if (valueForFaction2 < num4)
						{
							num4 = valueForFaction2;
						}
						num5 += valueForFaction2;
						num6++;
					}
				}
			}
			float num7 = (float)num5 / (float)num6;
			int num8 = (int)(0.65f * num7 + 0.35f * (float)num4);
			if (num8 > num2)
			{
				num2 = num8;
				num3 = 0.2f;
			}
		}
		int num9 = num2;
		if (num2 > -5000 && num2 < 5000)
		{
			num2 = 0;
		}
		int dailyTributeForValue = Campaign.Current.Models.DiplomacyModel.GetDailyTributeForValue(num2);
		decision = new MakePeaceKingdomDecision(clan, otherFaction, dailyTributeForValue);
		if (decision.CalculateSupport(clan) > 5f)
		{
			float kingdomSupportForDecision = GetKingdomSupportForDecision(decision);
			if (MBRandom.RandomFloat < 2f * (kingdomSupportForDecision - num3))
			{
				if (otherFaction == Hero.MainHero.MapFaction)
				{
					num2 = num9 + 15000;
					if (num2 > -5000 && num2 < 5000)
					{
						num2 = 0;
					}
					int dailyTributeForValue2 = Campaign.Current.Models.DiplomacyModel.GetDailyTributeForValue(num2);
					decision = new MakePeaceKingdomDecision(clan, otherFaction, dailyTributeForValue2);
				}
				return true;
			}
		}
		return false;
	}

	private float GetKingdomSupportForPeace(Clan clan, Clan otherClan, Kingdom kingdom, IFaction otherFaction)
	{
		_ = Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfProposingPeace(clan) / 2;
		int num = -new PeaceBarterable(clan.Leader, kingdom, otherFaction, CampaignTime.Years(1f)).GetValueForFaction(otherFaction);
		if (otherFaction is Clan && num < 0)
		{
			num = 0;
		}
		if (num > -5000 && num < 5000)
		{
			num = 0;
		}
		int dailyTributeForValue = Campaign.Current.Models.DiplomacyModel.GetDailyTributeForValue(num);
		return new KingdomElection(new MakePeaceKingdomDecision(clan, otherFaction, dailyTributeForValue)).GetLikelihoodForSponsor(clan);
	}

	private KingdomDecision GetRandomPolicyDecision(Clan clan)
	{
		KingdomDecision result = null;
		Kingdom kingdom = clan.Kingdom;
		if (kingdom.UnresolvedDecisions.FirstOrDefault((KingdomDecision x) => x is KingdomPolicyDecision) != null)
		{
			return null;
		}
		if (clan.Influence < 200f)
		{
			return null;
		}
		PolicyObject randomElement = PolicyObject.All.GetRandomElement();
		bool flag = kingdom.ActivePolicies.Contains(randomElement);
		if (ConsiderPolicy(clan, kingdom, randomElement, flag))
		{
			result = new KingdomPolicyDecision(clan, randomElement, flag);
		}
		return result;
	}

	private bool ConsiderPolicy(Clan clan, Kingdom kingdom, PolicyObject policy, bool invert)
	{
		int influenceCostOfPolicyProposalAndDisavowal = Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfPolicyProposalAndDisavowal(clan);
		if (clan.Influence < (float)influenceCostOfPolicyProposalAndDisavowal)
		{
			return false;
		}
		KingdomPolicyDecision kingdomPolicyDecision = new KingdomPolicyDecision(clan, policy, invert);
		if (kingdomPolicyDecision.CalculateSupport(clan) > 50f)
		{
			float kingdomSupportForDecision = GetKingdomSupportForDecision(kingdomPolicyDecision);
			if ((double)MBRandom.RandomFloat < (double)kingdomSupportForDecision - 0.55)
			{
				return true;
			}
		}
		return false;
	}

	private float GetKingdomSupportForPolicy(Clan clan, Kingdom kingdom, PolicyObject policy, bool invert)
	{
		Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfPolicyProposalAndDisavowal(clan);
		return new KingdomElection(new KingdomPolicyDecision(clan, policy, invert)).GetLikelihoodForSponsor(clan);
	}

	private KingdomDecision GetRandomAnnexationDecision(Clan clan)
	{
		KingdomDecision result = null;
		Kingdom kingdom = clan.Kingdom;
		if (kingdom.UnresolvedDecisions.FirstOrDefault((KingdomDecision x) => x is KingdomPolicyDecision) != null)
		{
			return null;
		}
		if (clan.Influence < 300f)
		{
			return null;
		}
		Clan randomElement = kingdom.Clans.GetRandomElement();
		if (randomElement != null && randomElement != clan && randomElement.GetRelationWithClan(clan) < -25)
		{
			if (randomElement.Fiefs.Count == 0)
			{
				return null;
			}
			Town randomElement2 = randomElement.Fiefs.GetRandomElement();
			if (ConsiderAnnex(clan, randomElement2))
			{
				result = new SettlementClaimantPreliminaryDecision(clan, randomElement2.Settlement);
			}
		}
		return result;
	}

	private bool ConsiderAnnex(Clan clan, Town targetSettlement)
	{
		int influenceCostOfAnnexation = Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfAnnexation(clan);
		if (clan.Influence < (float)influenceCostOfAnnexation)
		{
			return false;
		}
		SettlementClaimantPreliminaryDecision settlementClaimantPreliminaryDecision = new SettlementClaimantPreliminaryDecision(clan, targetSettlement.Settlement);
		if (settlementClaimantPreliminaryDecision.CalculateSupport(clan) > 50f)
		{
			float kingdomSupportForDecision = GetKingdomSupportForDecision(settlementClaimantPreliminaryDecision);
			if ((double)MBRandom.RandomFloat < (double)kingdomSupportForDecision - 0.6)
			{
				return true;
			}
		}
		return false;
	}

	private float GetKingdomSupportForDecision(KingdomDecision decision)
	{
		return new KingdomElection(decision).GetLikelihoodForOutcome(0);
	}

	private void SessionLaunched(CampaignGameStarter starter)
	{
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_kingdomDecisionsList", ref _kingdomDecisionsList);
	}
}
