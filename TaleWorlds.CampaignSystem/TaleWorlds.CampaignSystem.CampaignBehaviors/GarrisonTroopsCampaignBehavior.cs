using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.LinQuick;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class GarrisonTroopsCampaignBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
		CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener(this, OnNewGameCreatedPartialFollowUpEvent);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	public void OnNewGameCreatedPartialFollowUpEvent(CampaignGameStarter starter, int i)
	{
		List<Settlement> list = Campaign.Current.Settlements.WhereQ((Settlement x) => x.IsFortification).ToList();
		int count = list.Count;
		int num = count / 100 + ((count % 100 > i) ? 1 : 0);
		int num2 = count / 100 * i;
		for (int j = 0; j < i; j++)
		{
			num2 += ((count % 100 > j) ? 1 : 0);
		}
		for (int k = 0; k < num; k++)
		{
			list[num2 + k].AddGarrisonParty(addInitialGarrison: true);
		}
	}

	public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
	{
		if (!Campaign.Current.GameStarted || mobileParty == null || !mobileParty.IsLordParty || mobileParty.IsDisbanding || mobileParty.LeaderHero == null || !settlement.IsFortification || !FactionManager.IsAlliedWithFaction(mobileParty.MapFaction, settlement.MapFaction) || (settlement.OwnerClan == Clan.PlayerClan && !settlement.Town.IsOwnerUnassigned))
		{
			return;
		}
		if (mobileParty.Army != null)
		{
			if (mobileParty.Army.LeaderParty == mobileParty)
			{
				TryLeaveOrTakeTroopsFromGarrisonForArmy(mobileParty);
			}
		}
		else if (!mobileParty.IsMainParty)
		{
			(int, int) garrisonLeaveOrTakeDataOfParty = GetGarrisonLeaveOrTakeDataOfParty(mobileParty);
			ApplyTroopLeaveOrTakeData(mobileParty, garrisonLeaveOrTakeDataOfParty.Item1, garrisonLeaveOrTakeDataOfParty.Item2);
		}
	}

	private void TryLeaveOrTakeTroopsFromGarrisonForArmy(MobileParty mobileParty)
	{
		List<(MobileParty, int, int)> list = new List<(MobileParty, int, int)>();
		(int, int) garrisonLeaveOrTakeDataOfParty = GetGarrisonLeaveOrTakeDataOfParty(mobileParty);
		list.Add((mobileParty, garrisonLeaveOrTakeDataOfParty.Item1, garrisonLeaveOrTakeDataOfParty.Item2));
		foreach (MobileParty attachedParty in mobileParty.AttachedParties)
		{
			(int, int) garrisonLeaveOrTakeDataOfParty2 = GetGarrisonLeaveOrTakeDataOfParty(attachedParty);
			list.Add((attachedParty, garrisonLeaveOrTakeDataOfParty2.Item1, garrisonLeaveOrTakeDataOfParty2.Item2));
		}
		foreach (var (mobileParty2, numberOfTroopsToLeave, numberOfTroopToTake) in list)
		{
			if (mobileParty2 != MobileParty.MainParty)
			{
				ApplyTroopLeaveOrTakeData(mobileParty2, numberOfTroopsToLeave, numberOfTroopToTake);
			}
		}
	}

	private (int, int) GetGarrisonLeaveOrTakeDataOfParty(MobileParty mobileParty)
	{
		Settlement currentSettlement = mobileParty.CurrentSettlement;
		int num = Campaign.Current.Models.SettlementGarrisonModel.FindNumberOfTroopsToLeaveToGarrison(mobileParty, currentSettlement);
		int item = 0;
		if (num <= 0 && mobileParty.LeaderHero.Clan == currentSettlement.OwnerClan && !mobileParty.IsWageLimitExceeded())
		{
			item = Campaign.Current.Models.SettlementGarrisonModel.FindNumberOfTroopsToTakeFromGarrison(mobileParty, mobileParty.CurrentSettlement);
		}
		return (num, item);
	}

	private void ApplyTroopLeaveOrTakeData(MobileParty party, int numberOfTroopsToLeave, int numberOfTroopToTake)
	{
		if (numberOfTroopsToLeave > 0)
		{
			LeaveTroopsToSettlementAction.Apply(party, party.CurrentSettlement, numberOfTroopsToLeave, archersAreHighPriority: true);
		}
		else if (numberOfTroopToTake > 0)
		{
			LeaveTroopsToSettlementAction.Apply(party, party.CurrentSettlement, -numberOfTroopToTake, archersAreHighPriority: false);
		}
	}
}
