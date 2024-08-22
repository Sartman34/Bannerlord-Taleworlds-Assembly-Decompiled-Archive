using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class PoliticalStagnationAndBorderIncidentCampaignBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
		CampaignEvents.HourlyTickSettlementEvent.AddNonSerializedListener(this, HourlyTickSettlement);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	public void HourlyTickSettlement(Settlement settlement)
	{
		if (!settlement.IsFortification && !settlement.IsVillage)
		{
			return;
		}
		LocatableSearchData<MobileParty> data = MobileParty.StartFindingLocatablesAroundPosition(settlement.Position2D, 10f);
		for (MobileParty mobileParty = MobileParty.FindNextLocatable(ref data); mobileParty != null; mobileParty = MobileParty.FindNextLocatable(ref data))
		{
			if (!mobileParty.IsGarrison && !mobileParty.IsMilitia && mobileParty.Aggressiveness > 0f)
			{
				if (mobileParty.MapFaction == settlement.MapFaction && (mobileParty.IsCaravan || mobileParty.IsVillager) && mobileParty.Ai.IsAlerted)
				{
					settlement.NumberOfEnemiesSpottedAround += 0.2f;
				}
				if (mobileParty.CurrentSettlement == null && FactionManager.IsAtWarAgainstFaction(mobileParty.MapFaction, settlement.MapFaction))
				{
					float num = mobileParty.Party.TotalStrength;
					if (mobileParty == MobileParty.MainParty)
					{
						num *= 2f;
						num += 50f;
					}
					float num2 = MathF.Min(1f, num / 500f * MathF.Min(1f, mobileParty.Aggressiveness));
					if (!mobileParty.IsLordParty)
					{
						num2 *= 0.5f;
					}
					if (mobileParty.MapEvent != null && mobileParty.MapEvent.IsFieldBattle)
					{
						num2 = 3f * num2;
					}
					settlement.NumberOfEnemiesSpottedAround += num2;
				}
				else if (mobileParty.MapFaction == settlement.MapFaction)
				{
					float num3 = MathF.Min(1f, mobileParty.Party.TotalStrength / 500f * MathF.Min(1f, mobileParty.Aggressiveness));
					settlement.NumberOfAlliesSpottedAround += num3;
				}
			}
		}
		settlement.NumberOfEnemiesSpottedAround *= 0.95f;
		settlement.NumberOfAlliesSpottedAround *= 0.8f;
	}

	public void DailyTick()
	{
		foreach (Kingdom item in Kingdom.All)
		{
			UpdatePoliticallyStagnation(item);
		}
	}

	private static void UpdatePoliticallyStagnation(Kingdom kingdom)
	{
		float num = 1f + (float)MathF.Min(60, kingdom.Fiefs.Count) * 0.2f;
		float num2 = 2f + (float)MathF.Min(60, kingdom.Fiefs.Count) * 0.6f;
		int num3 = 1;
		foreach (Kingdom item in Kingdom.All)
		{
			if (FactionManager.IsAtWarAgainstFaction(kingdom, item))
			{
				if ((float)item.Fiefs.Count >= num2)
				{
					num3 = -2;
					break;
				}
				if ((float)item.Fiefs.Count >= num)
				{
					num3 = -1;
				}
			}
		}
		kingdom.PoliticalStagnation += num3;
		if (kingdom.PoliticalStagnation < 0)
		{
			kingdom.PoliticalStagnation = 0;
		}
		else if (kingdom.PoliticalStagnation > 300)
		{
			kingdom.PoliticalStagnation = 300;
		}
	}

	private void BorderIncidents()
	{
	}
}
