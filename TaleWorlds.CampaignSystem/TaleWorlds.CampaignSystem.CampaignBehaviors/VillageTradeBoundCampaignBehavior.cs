using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

internal class VillageTradeBoundCampaignBehavior : CampaignBehaviorBase
{
	public const float TradeBoundDistanceLimit = 150f;

	public override void RegisterEvents()
	{
		CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
		CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
		CampaignEvents.WarDeclared.AddNonSerializedListener(this, WarDeclared);
		CampaignEvents.MakePeace.AddNonSerializedListener(this, OnMakePeace);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, ClanChangedKingdom);
		CampaignEvents.OnClanDestroyedEvent.AddNonSerializedListener(this, OnClanDestroyed);
	}

	private void OnClanDestroyed(Clan obj)
	{
		UpdateTradeBounds();
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void ClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
	{
		UpdateTradeBounds();
	}

	private void OnGameLoaded(CampaignGameStarter obj)
	{
		UpdateTradeBounds();
	}

	private void OnMakePeace(IFaction faction1, IFaction faction2, MakePeaceAction.MakePeaceDetail detail)
	{
		UpdateTradeBounds();
	}

	private void WarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail declareWarDetail)
	{
		UpdateTradeBounds();
	}

	private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
	{
		UpdateTradeBounds();
	}

	public void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
	{
		UpdateTradeBounds();
	}

	private void UpdateTradeBounds()
	{
		foreach (Town allCastle in Campaign.Current.AllCastles)
		{
			foreach (Village village in allCastle.Villages)
			{
				TryToAssignTradeBoundForVillage(village);
			}
		}
	}

	private void TryToAssignTradeBoundForVillage(Village village)
	{
		Settlement settlement = SettlementHelper.FindNearestSettlement((Settlement x) => x.IsTown && x.Town.MapFaction == village.Settlement.MapFaction, village.Settlement);
		if (settlement != null && Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, village.Settlement) < 150f)
		{
			village.TradeBound = settlement;
			return;
		}
		Settlement settlement2 = SettlementHelper.FindNearestSettlement((Settlement x) => x.IsTown && x.Town.MapFaction != village.Settlement.MapFaction && !x.Town.MapFaction.IsAtWarWith(village.Settlement.MapFaction) && Campaign.Current.Models.MapDistanceModel.GetDistance(x, village.Settlement) <= 150f, village.Settlement);
		if (settlement2 != null && Campaign.Current.Models.MapDistanceModel.GetDistance(settlement2, village.Settlement) < 150f)
		{
			village.TradeBound = settlement2;
		}
		else
		{
			village.TradeBound = null;
		}
	}
}
