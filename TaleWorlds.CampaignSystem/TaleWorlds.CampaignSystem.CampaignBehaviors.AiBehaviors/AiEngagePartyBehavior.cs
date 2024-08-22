using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;

public class AiEngagePartyBehavior : CampaignBehaviorBase
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

	private void AiHourlyTick(MobileParty mobileParty, PartyThinkParams p)
	{
		if (mobileParty.CurrentSettlement != null && mobileParty.CurrentSettlement.SiegeEvent != null)
		{
			return;
		}
		float num = 25f;
		if ((!mobileParty.MapFaction.IsKingdomFaction && mobileParty.MapFaction != Hero.MainHero.MapFaction) || mobileParty.IsCaravan || (mobileParty.Army != null && mobileParty.Army.LeaderParty != mobileParty) || mobileParty.LeaderHero == null)
		{
			return;
		}
		float num2 = Campaign.MapDiagonalSquared;
		LocatableSearchData<Settlement> data = Settlement.StartFindingLocatablesAroundPosition(mobileParty.Position2D, 50f);
		for (Settlement settlement = Settlement.FindNextLocatable(ref data); settlement != null; settlement = Settlement.FindNextLocatable(ref data))
		{
			if (settlement.MapFaction == mobileParty.MapFaction)
			{
				float num3 = settlement.Position2D.DistanceSquared(mobileParty.Position2D);
				if (num3 < num2)
				{
					num2 = num3;
				}
			}
		}
		float num4 = MathF.Sqrt(num2);
		float num5 = ((num4 < 50f) ? (1f - MathF.Max(0f, num4 - 15f) / 35f) : 0f);
		if (!(num5 > 0f))
		{
			return;
		}
		float num6 = mobileParty.PartySizeRatio;
		foreach (MobileParty attachedParty in mobileParty.AttachedParties)
		{
			num6 += attachedParty.PartySizeRatio;
		}
		float num7 = MathF.Min(1f, num6 / ((float)mobileParty.AttachedParties.Count + 1f));
		float num8 = num7 * ((num7 <= 0.5f) ? num7 : (0.5f + 0.707f * MathF.Sqrt(num7 - 0.5f)));
		LocatableSearchData<MobileParty> data2 = MobileParty.StartFindingLocatablesAroundPosition(mobileParty.Position2D, num);
		for (MobileParty mobileParty2 = MobileParty.FindNextLocatable(ref data2); mobileParty2 != null; mobileParty2 = MobileParty.FindNextLocatable(ref data2))
		{
			if (!mobileParty2.IsActive)
			{
				continue;
			}
			IFaction mapFaction = mobileParty2.MapFaction;
			if (mapFaction == null || !mapFaction.IsAtWarWith(mobileParty.MapFaction) || (mobileParty2.Army != null && mobileParty2 != mobileParty2.Army.LeaderParty))
			{
				continue;
			}
			IFaction mapFaction2 = mobileParty2.MapFaction;
			if (((mapFaction2 == null || !mapFaction2.IsKingdomFaction) && mobileParty2.MapFaction != Hero.MainHero.MapFaction) || (mobileParty2.CurrentSettlement != null && mobileParty2.CurrentSettlement.IsFortification) || !(mobileParty2.Aggressiveness > 0.1f))
			{
				continue;
			}
			float num9 = mobileParty2.Army?.TotalStrength ?? mobileParty2.Party.TotalStrength;
			float num10 = 1f - 0.5f * mobileParty2.Position2D.DistanceSquared(mobileParty.Position2D) / (num * num);
			float num11 = 1f;
			if (mobileParty2.LeaderHero != null)
			{
				int relation = mobileParty2.LeaderHero.GetRelation(mobileParty.LeaderHero);
				num11 = ((relation >= 0) ? (1f - MathF.Sqrt(relation) / 10f) : (1f + MathF.Sqrt(-relation) / 20f));
			}
			float num12 = 0f;
			LocatableSearchData<MobileParty> data3 = MobileParty.StartFindingLocatablesAroundPosition(mobileParty.Position2D, num);
			for (MobileParty mobileParty3 = MobileParty.FindNextLocatable(ref data3); mobileParty3 != null; mobileParty3 = MobileParty.FindNextLocatable(ref data3))
			{
				if (mobileParty3 != mobileParty && mobileParty3.MapFaction == mobileParty.MapFaction && (mobileParty3.Army == null || mobileParty3.Army.LeaderParty == mobileParty3) && ((mobileParty3.DefaultBehavior == AiBehavior.GoAroundParty && mobileParty3.TargetParty == mobileParty2) || (mobileParty3.ShortTermBehavior == AiBehavior.EngageParty && mobileParty3.ShortTermTargetParty == mobileParty2)))
				{
					num12 += mobileParty3.Army?.TotalStrength ?? mobileParty3.Party.TotalStrength;
				}
			}
			float num13 = mobileParty.Army?.TotalStrength ?? mobileParty.Party.TotalStrength;
			float num14 = (num12 + num13) / num9;
			float num15 = ((mobileParty2.CurrentSettlement != null && mobileParty2.CurrentSettlement.IsFortification && mobileParty2.CurrentSettlement.MapFaction != mobileParty.MapFaction) ? 0.25f : 1f);
			float num16 = 1f;
			if (num12 + (num13 + 30f) > num9 * 1.5f)
			{
				float num17 = num9 * 1.5f + 10f + ((mobileParty2.MapEvent != null || mobileParty2.SiegeEvent != null) ? 30f : 0f);
				float num18 = num12 + (num13 + 30f);
				num16 = MathF.Pow(num17 / num18, 0.8f);
			}
			float speed = mobileParty.Speed;
			float speed2 = mobileParty2.Speed;
			float num19 = speed / speed2;
			float num20 = num19 * num19 * num19 * num19;
			float num21 = ((speed > speed2 && mobileParty.Army == null) ? 1f : ((num12 + num13 > num9) ? (0.5f + 0.5f * num20 * num16) : (0.5f * num20)));
			float num22 = ((mobileParty.DefaultBehavior == AiBehavior.GoAroundParty && mobileParty2 == mobileParty.TargetParty) ? 1.1f : 1f);
			float num23 = ((mobileParty.Army != null) ? 0.9f : 1f);
			float num24 = ((mobileParty2 == MobileParty.MainParty) ? 1.2f : 1f);
			float num25 = 1f;
			if (mobileParty.Objective == MobileParty.PartyObjective.Defensive)
			{
				num25 = 1.2f;
			}
			float num26 = 1f;
			if (mobileParty.MapFaction != null && mobileParty.MapFaction.IsKingdomFaction && mobileParty.MapFaction.Leader == Hero.MainHero)
			{
				StanceLink stanceWith = Hero.MainHero.MapFaction.GetStanceWith(mobileParty2.MapFaction);
				if (stanceWith != null && stanceWith.BehaviorPriority == 1)
				{
					num26 = 1.2f;
				}
			}
			float num27 = num10 * num5 * num11 * num14 * num24 * num8 * num21 * num16 * num15 * num22 * num23 * num25 * num26 * 2f;
			if (num27 > 0.05f && mobileParty2.CurrentSettlement == null)
			{
				float num28 = Campaign.MapDiagonalSquared;
				LocatableSearchData<Settlement> data4 = Settlement.StartFindingLocatablesAroundPosition(mobileParty2.Position2D, 25f);
				for (Settlement settlement2 = Settlement.FindNextLocatable(ref data4); settlement2 != null; settlement2 = Settlement.FindNextLocatable(ref data4))
				{
					if (settlement2.MapFaction == mobileParty2.MapFaction)
					{
						float num29 = settlement2.Position2D.DistanceSquared(mobileParty.Position2D);
						if (num29 < num28)
						{
							num28 = num29;
						}
					}
				}
				if (num28 < 625f)
				{
					float num30 = MathF.Sqrt(num28);
					num27 *= 0.25f + 0.75f * (MathF.Max(0f, num30 - 5f) / 20f);
					if (!mobileParty.IsDisbanding)
					{
						IDisbandPartyCampaignBehavior disbandPartyCampaignBehavior = _disbandPartyCampaignBehavior;
						if (disbandPartyCampaignBehavior == null || !disbandPartyCampaignBehavior.IsPartyWaitingForDisband(mobileParty))
						{
							goto IL_068d;
						}
					}
					num27 *= 0.25f;
				}
			}
			goto IL_068d;
			IL_068d:
			p.CurrentObjectiveValue = num27;
			AiBehavior aiBehavior = AiBehavior.GoAroundParty;
			AIBehaviorTuple item = new AIBehaviorTuple(mobileParty2, aiBehavior);
			(AIBehaviorTuple, float) value = (item, num27);
			p.AddBehaviorScore(in value);
		}
	}
}
