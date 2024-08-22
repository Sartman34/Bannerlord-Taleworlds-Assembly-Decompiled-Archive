using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;

public class AiArmyMemberBehavior : CampaignBehaviorBase
{
	private const float FollowingArmyLeaderDefaultScore = 0.25f;

	public override void RegisterEvents()
	{
		CampaignEvents.AiHourlyTickEvent.AddNonSerializedListener(this, AiHourlyTick);
		CampaignEvents.OnSiegeEventStartedEvent.AddNonSerializedListener(this, OnSiegeEventStarted);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnSiegeEventStarted(SiegeEvent siegeEvent)
	{
		for (int i = 0; i < siegeEvent.BesiegedSettlement.Parties.Count; i++)
		{
			if (siegeEvent.BesiegedSettlement.Parties[i].IsLordParty)
			{
				siegeEvent.BesiegedSettlement.Parties[i].Ai.SetMoveModeHold();
			}
		}
	}

	public void AiHourlyTick(MobileParty mobileParty, PartyThinkParams p)
	{
		if (mobileParty.Army != null && mobileParty.Army.LeaderParty != mobileParty && (mobileParty.AttachedTo != null || ((mobileParty.Army.LeaderParty.CurrentSettlement == null || !mobileParty.Army.LeaderParty.CurrentSettlement.IsUnderSiege) && (mobileParty.CurrentSettlement == null || !mobileParty.CurrentSettlement.IsUnderSiege))))
		{
			AIBehaviorTuple item = new AIBehaviorTuple(mobileParty.Army.LeaderParty, AiBehavior.EscortParty);
			(AIBehaviorTuple, float) value = (item, 0.25f);
			p.AddBehaviorScore(in value);
		}
	}
}
