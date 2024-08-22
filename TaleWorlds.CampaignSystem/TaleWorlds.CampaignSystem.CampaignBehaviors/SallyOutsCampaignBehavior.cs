using System.Linq;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class SallyOutsCampaignBehavior : CampaignBehaviorBase
{
	private const int SallyOutCheckPeriodInHours = 4;

	private const float SallyOutPowerRatioForHelpingReliefForce = 1.5f;

	private const float SallyOutPowerRatio = 2f;

	public override void RegisterEvents()
	{
		CampaignEvents.HourlyTickSettlementEvent.AddNonSerializedListener(this, HourlyTickSettlement);
		CampaignEvents.MapEventStarted.AddNonSerializedListener(this, OnMapEventStarted);
	}

	private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
	{
		if (defenderParty.SiegeEvent != null)
		{
			CheckForSettlementSallyOut(defenderParty.SiegeEvent.BesiegedSettlement);
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	public void HourlyTickSettlement(Settlement settlement)
	{
		CheckForSettlementSallyOut(settlement);
	}

	private void CheckForSettlementSallyOut(Settlement settlement, bool forceForCheck = false)
	{
		if (!settlement.IsFortification || settlement.SiegeEvent == null || settlement.Party.MapEvent != null || settlement.Town.GarrisonParty == null || settlement.Town.GarrisonParty.MapEvent != null)
		{
			return;
		}
		bool flag = settlement.SiegeEvent.BesiegerCamp.LeaderParty.MapEvent != null && settlement.SiegeEvent.BesiegerCamp.LeaderParty.MapEvent.IsSiegeOutside;
		if ((!flag && MathF.Floor(CampaignTime.Now.ToHours) % 4 != 0) || (Hero.MainHero.CurrentSettlement == settlement && Campaign.Current.Models.EncounterModel.GetLeaderOfSiegeEvent(settlement.SiegeEvent, BattleSideEnum.Defender) == Hero.MainHero))
		{
			return;
		}
		MobileParty leaderParty = settlement.SiegeEvent.BesiegerCamp.LeaderParty;
		float num = 0f;
		float num2 = 0f;
		float num3 = settlement.GetInvolvedPartiesForEventType(MapEvent.BattleTypes.SallyOut).Sum((PartyBase x) => x.TotalStrength);
		LocatableSearchData<MobileParty> data = MobileParty.StartFindingLocatablesAroundPosition(settlement.SiegeEvent.BesiegerCamp.LeaderParty.Position2D, 3f);
		for (MobileParty mobileParty = MobileParty.FindNextLocatable(ref data); mobileParty != null; mobileParty = MobileParty.FindNextLocatable(ref data))
		{
			if (mobileParty.CurrentSettlement == null && mobileParty.Aggressiveness > 0f)
			{
				float num4 = ((mobileParty.Aggressiveness > 0.5f) ? 1f : (mobileParty.Aggressiveness * 2f));
				if (mobileParty.MapFaction.IsAtWarWith(settlement.Party.MapFaction))
				{
					num += num4 * mobileParty.Party.TotalStrength;
				}
				else if (mobileParty.MapFaction == settlement.MapFaction)
				{
					num2 += num4 * mobileParty.Party.TotalStrength;
				}
			}
		}
		float num5 = num3 + num2;
		float num6 = (flag ? 1.5f : 2f);
		if (!(num5 > num * num6))
		{
			return;
		}
		if (flag)
		{
			foreach (PartyBase item in settlement.GetInvolvedPartiesForEventType(MapEvent.BattleTypes.SallyOut))
			{
				if (item.IsMobile && !item.MobileParty.IsMainParty && item.MapEventSide == null)
				{
					item.MapEventSide = settlement.SiegeEvent.BesiegerCamp.LeaderParty.MapEvent.AttackerSide;
				}
			}
			return;
		}
		EncounterManager.StartPartyEncounter(settlement.Town.GarrisonParty.Party, leaderParty.Party);
	}
}
