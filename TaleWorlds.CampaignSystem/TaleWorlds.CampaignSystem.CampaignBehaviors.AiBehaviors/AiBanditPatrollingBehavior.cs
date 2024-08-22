using Helpers;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;

public class AiBanditPatrollingBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.AiHourlyTickEvent.AddNonSerializedListener(this, AiHourlyTick);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	public void AiHourlyTick(MobileParty mobileParty, PartyThinkParams p)
	{
		if (!mobileParty.IsBandit || mobileParty.IsBanditBossParty)
		{
			return;
		}
		int num = 0;
		if (mobileParty.CurrentSettlement != null && mobileParty.CurrentSettlement.IsHideout && mobileParty.CurrentSettlement.Parties.CountQ((MobileParty x) => x.IsBandit && !x.IsBanditBossParty) <= Campaign.Current.Models.BanditDensityModel.NumberOfMinimumBanditPartiesInAHideoutToInfestIt + 1)
		{
			return;
		}
		if (mobileParty.MapFaction.Culture.CanHaveSettlement && (mobileParty.Ai.NeedTargetReset || (mobileParty.HomeSettlement.IsHideout && !mobileParty.HomeSettlement.Hideout.IsInfested)))
		{
			Settlement settlement = SettlementHelper.FindNearestHideout((Settlement x) => x.Culture == mobileParty.MapFaction.Culture && x.Hideout.IsInfested);
			if (settlement != null)
			{
				mobileParty.BanditPartyComponent.SetHomeHideout(settlement.Hideout);
			}
		}
		AIBehaviorTuple item = new AIBehaviorTuple(mobileParty.HomeSettlement, AiBehavior.PatrolAroundPoint);
		float num2 = 1f;
		if (mobileParty.CurrentSettlement != null && mobileParty.CurrentSettlement.IsHideout && (mobileParty.CurrentSettlement.MapFaction == mobileParty.MapFaction || mobileParty.CurrentSettlement.Hideout.IsInfested))
		{
			int numberOfMinimumBanditPartiesInAHideoutToInfestIt = Campaign.Current.Models.BanditDensityModel.NumberOfMinimumBanditPartiesInAHideoutToInfestIt;
			int numberOfMaximumBanditPartiesInEachHideout = Campaign.Current.Models.BanditDensityModel.NumberOfMaximumBanditPartiesInEachHideout;
			num2 = (float)(num - numberOfMinimumBanditPartiesInAHideoutToInfestIt) / (float)(numberOfMaximumBanditPartiesInEachHideout - numberOfMinimumBanditPartiesInAHideoutToInfestIt);
		}
		float num3 = ((mobileParty.CurrentSettlement != null) ? (MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat) : 0.5f);
		float item2 = 0.5f * num2 * num3;
		(AIBehaviorTuple, float) value = (item, item2);
		p.AddBehaviorScore(in value);
	}
}
