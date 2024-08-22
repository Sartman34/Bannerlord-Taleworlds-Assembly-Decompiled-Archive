using Helpers;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;

public class AiPatrollingBehavior : CampaignBehaviorBase
{
	private IDisbandPartyCampaignBehavior _disbandPartyCampaignBehavior;

	public override void RegisterEvents()
	{
		CampaignEvents.AiHourlyTickEvent.AddNonSerializedListener(this, AiHourlyTick);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
	}

	private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		_disbandPartyCampaignBehavior = Campaign.Current.GetCampaignBehavior<IDisbandPartyCampaignBehavior>();
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void CalculatePatrollingScoreForSettlement(Settlement settlement, PartyThinkParams p, float patrollingScoreAdjustment)
	{
		MobileParty mobilePartyOf = p.MobilePartyOf;
		AIBehaviorTuple aiBehaviorTuple = new AIBehaviorTuple(settlement, AiBehavior.PatrolAroundPoint);
		float num = Campaign.Current.Models.TargetScoreCalculatingModel.CalculatePatrollingScoreForSettlement(settlement, mobilePartyOf);
		num *= patrollingScoreAdjustment;
		if (!mobilePartyOf.IsDisbanding)
		{
			IDisbandPartyCampaignBehavior disbandPartyCampaignBehavior = _disbandPartyCampaignBehavior;
			if (disbandPartyCampaignBehavior == null || !disbandPartyCampaignBehavior.IsPartyWaitingForDisband(mobilePartyOf))
			{
				goto IL_0052;
			}
		}
		num *= 0.25f;
		goto IL_0052;
		IL_0052:
		if (p.TryGetBehaviorScore(in aiBehaviorTuple, out var score))
		{
			p.SetBehaviorScore(in aiBehaviorTuple, score + num);
			return;
		}
		(AIBehaviorTuple, float) value = (aiBehaviorTuple, num);
		p.AddBehaviorScore(in value);
	}

	private void AiHourlyTick(MobileParty mobileParty, PartyThinkParams p)
	{
		if (mobileParty.IsMilitia || mobileParty.IsCaravan || mobileParty.IsVillager || mobileParty.IsBandit || mobileParty.IsDisbanding || (!mobileParty.MapFaction.IsMinorFaction && !mobileParty.MapFaction.IsKingdomFaction && !mobileParty.MapFaction.Leader.IsLord) || mobileParty.CurrentSettlement?.SiegeEvent != null)
		{
			return;
		}
		float num = 1f;
		if (mobileParty.Army != null)
		{
			float num2 = 0f;
			foreach (MobileParty party in mobileParty.Army.Parties)
			{
				float num3 = PartyBaseHelper.FindPartySizeNormalLimit(party);
				float num4 = party.PartySizeRatio / num3;
				num2 += num4;
			}
			num = num2 / (float)mobileParty.Army.Parties.Count;
		}
		else
		{
			float num5 = PartyBaseHelper.FindPartySizeNormalLimit(mobileParty);
			num = mobileParty.PartySizeRatio / num5;
		}
		float num6 = MathF.Sqrt(MathF.Min(1f, num));
		float num7 = ((mobileParty.CurrentSettlement != null && mobileParty.CurrentSettlement.IsFortification && mobileParty.CurrentSettlement.IsUnderSiege) ? 0f : 1f);
		num7 *= num6;
		if (mobileParty.Party.MapFaction.Settlements.Count > 0)
		{
			foreach (Settlement settlement in mobileParty.Party.MapFaction.Settlements)
			{
				if (settlement.IsTown || settlement.IsVillage || settlement.MapFaction.IsMinorFaction)
				{
					CalculatePatrollingScoreForSettlement(settlement, p, num7);
				}
			}
			return;
		}
		int num8 = -1;
		do
		{
			num8 = SettlementHelper.FindNextSettlementAroundMapPoint(mobileParty, Campaign.AverageDistanceBetweenTwoFortifications * 5f, num8);
			if (num8 >= 0 && Settlement.All[num8].IsTown)
			{
				CalculatePatrollingScoreForSettlement(Settlement.All[num8], p, num7);
			}
		}
		while (num8 >= 0);
	}
}
