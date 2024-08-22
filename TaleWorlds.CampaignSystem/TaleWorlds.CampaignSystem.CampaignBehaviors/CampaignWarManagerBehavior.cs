using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class CampaignWarManagerBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.MapEventEnded.AddNonSerializedListener(this, MapEventEnded);
	}

	private void MapEventEnded(MapEvent mapEvent)
	{
		if (mapEvent.AttackerSide.LeaderParty.MapFaction == null || mapEvent.AttackerSide.LeaderParty.MapFaction.IsBanditFaction || mapEvent.DefenderSide.LeaderParty.MapFaction == null || mapEvent.DefenderSide.LeaderParty.MapFaction.IsBanditFaction)
		{
			return;
		}
		IFaction mapFaction = mapEvent.AttackerSide.MapFaction;
		IFaction mapFaction2 = mapEvent.DefenderSide.MapFaction;
		if (mapFaction.MapFaction == mapFaction2.MapFaction)
		{
			return;
		}
		StanceLink stanceWith = mapFaction.GetStanceWith(mapFaction2);
		stanceWith.Casualties1 += ((stanceWith.Faction1 == mapFaction) ? mapEvent.AttackerSide.Casualties : mapEvent.DefenderSide.Casualties);
		stanceWith.Casualties2 += ((stanceWith.Faction2 == mapFaction) ? mapEvent.AttackerSide.Casualties : mapEvent.DefenderSide.Casualties);
		if (mapEvent.MapEventSettlement == null || mapEvent.BattleState != BattleState.AttackerVictory)
		{
			return;
		}
		if (mapEvent.MapEventSettlement.IsVillage && mapEvent.MapEventSettlement.Village.VillageState == Village.VillageStates.Looted)
		{
			if (mapFaction == stanceWith.Faction1)
			{
				stanceWith.SuccessfulRaids1++;
			}
			else
			{
				stanceWith.SuccessfulRaids2++;
			}
		}
		else if (mapEvent.MapEventSettlement.IsFortification && mapEvent.EventType == MapEvent.BattleTypes.Siege)
		{
			if (mapFaction == stanceWith.Faction1)
			{
				stanceWith.SuccessfulSieges1++;
			}
			else
			{
				stanceWith.SuccessfulSieges2++;
			}
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
