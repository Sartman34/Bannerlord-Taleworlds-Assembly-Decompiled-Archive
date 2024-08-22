using System.Linq;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class PrisonerCaptureCampaignBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
		CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification)
	{
		foreach (Settlement item in clan.Settlements.Where((Settlement x) => x.IsFortification))
		{
			HandleSettlementHeroes(item);
		}
	}

	private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail)
	{
		foreach (Settlement item in faction1.Settlements.Where((Settlement x) => x.IsFortification))
		{
			HandleSettlementHeroes(item);
		}
		foreach (Settlement item2 in faction2.Settlements.Where((Settlement x) => x.IsFortification))
		{
			HandleSettlementHeroes(item2);
		}
	}

	private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
	{
		if (settlement.IsFortification)
		{
			HandleSettlementHeroes(settlement);
		}
	}

	private void HandleSettlementHeroes(Settlement settlement)
	{
		foreach (Hero item in settlement.HeroesWithoutParty.Where(SettlementHeroCaptureCommonCondition).ToList())
		{
			TakePrisonerAction.Apply(item.CurrentSettlement.Party, item);
		}
		foreach (MobileParty item2 in settlement.Parties.Where((MobileParty x) => x.IsLordParty && (x.Army == null || (x.Army != null && x.Army.LeaderParty == x && !x.Army.Parties.Contains(MobileParty.MainParty))) && x.MapEvent == null && SettlementHeroCaptureCommonCondition(x.LeaderHero)).ToList())
		{
			LeaveSettlementAction.ApplyForParty(item2);
			SetPartyAiAction.GetActionForPatrollingAroundSettlement(item2, settlement);
		}
	}

	private bool SettlementHeroCaptureCommonCondition(Hero hero)
	{
		if (hero != null && hero != Hero.MainHero && !hero.IsWanderer && !hero.IsNotable && hero.HeroState != Hero.CharacterStates.Prisoner && hero.HeroState != Hero.CharacterStates.Dead && hero.MapFaction != null && hero.CurrentSettlement != null)
		{
			return hero.MapFaction.IsAtWarWith(hero.CurrentSettlement.MapFaction);
		}
		return false;
	}
}
