using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;

public class AiVisitSettlementBehavior : CampaignBehaviorBase
{
	private const float NumberOfHoursAtDay = 24f;

	private const float IdealTimePeriodForVisitingOwnedSettlement = 360f;

	private const float DefaultMoneyLimitForRecruiting = 2000f;

	private const float MaximumMeaningfulDistance = 250f;

	private const float MaximumFilteredDistance = 350f;

	private const float MeaningfulScoreThreshold = 0.025f;

	private const float GoodEnoughScore = 2.5f;

	private const float BaseVisitScore = 1.6f;

	private const int SearchRadius = 30;

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
		if (mobileParty.CurrentSettlement?.SiegeEvent != null)
		{
			return;
		}
		Settlement currentSettlementOfMobilePartyForAICalculation = MobilePartyHelper.GetCurrentSettlementOfMobilePartyForAICalculation(mobileParty);
		if (mobileParty.IsBandit)
		{
			CalculateVisitHideoutScoresForBanditParty(mobileParty, currentSettlementOfMobilePartyForAICalculation, p);
			return;
		}
		IFaction mapFaction = mobileParty.MapFaction;
		if (mobileParty.IsMilitia || mobileParty.IsCaravan || mobileParty.IsVillager || (!mapFaction.IsMinorFaction && !mapFaction.IsKingdomFaction && (mobileParty.LeaderHero == null || !mobileParty.LeaderHero.IsLord)) || (mobileParty.Army != null && mobileParty.Army.LeaderParty != mobileParty && !(mobileParty.Army.Cohesion < (float)mobileParty.Army.CohesionThresholdForDispersion)))
		{
			return;
		}
		Hero leaderHero = mobileParty.LeaderHero;
		(float, float, int, int) tuple = CalculatePartyParameters(mobileParty);
		float item = tuple.Item1;
		float item2 = tuple.Item2;
		int item3 = tuple.Item3;
		int item4 = tuple.Item4;
		float num = item2 / Math.Min(1f, Math.Max(0.1f, item));
		float num2 = ((num >= 1f) ? 0.33f : ((TaleWorlds.Library.MathF.Max(1f, TaleWorlds.Library.MathF.Min(2f, num)) - 0.5f) / 1.5f));
		float num3 = mobileParty.Food;
		float num4 = 0f - mobileParty.FoodChange;
		int num5 = leaderHero?.Gold ?? 0;
		if (mobileParty.Army != null && mobileParty == mobileParty.Army.LeaderParty)
		{
			foreach (MobileParty attachedParty in mobileParty.Army.LeaderParty.AttachedParties)
			{
				num3 += attachedParty.Food;
				num4 += 0f - attachedParty.FoodChange;
				num5 += attachedParty.LeaderHero?.Gold ?? 0;
			}
		}
		float num6 = 1f;
		if (leaderHero != null && mobileParty.IsLordParty)
		{
			num6 = CalculateSellItemScore(mobileParty);
		}
		int num7 = mobileParty.Party.PrisonerSizeLimit;
		if (mobileParty.Army != null)
		{
			foreach (MobileParty attachedParty2 in mobileParty.Army.LeaderParty.AttachedParties)
			{
				num7 += attachedParty2.Party.PrisonerSizeLimit;
			}
		}
		SortedList<(float, int), Settlement> sortedList = FindSettlementsToVisitWithDistances(mobileParty);
		float num8 = PartyBaseHelper.FindPartySizeNormalLimit(mobileParty);
		float num9 = Campaign.MapDiagonalSquared;
		if (num3 - num4 < 0f)
		{
			foreach (KeyValuePair<(float, int), Settlement> item7 in sortedList)
			{
				float item5 = item7.Key.Item1;
				Settlement value = item7.Value;
				if (item5 < 250f && item5 < num9 && (float)value.ItemRoster.TotalFood > num4 * 2f)
				{
					num9 = item5;
				}
			}
		}
		float num10 = 2000f;
		float num11 = 2000f;
		if (leaderHero != null)
		{
			num10 = HeroHelper.StartRecruitingMoneyLimitForClanLeader(leaderHero);
			num11 = HeroHelper.StartRecruitingMoneyLimit(leaderHero);
		}
		float num12 = Campaign.AverageDistanceBetweenTwoFortifications * 0.4f;
		float num13 = (84f + Campaign.AverageDistanceBetweenTwoFortifications * 1.5f) * 0.5f;
		float num14 = (424f + 7.57f * Campaign.AverageDistanceBetweenTwoFortifications) * 0.5f;
		foreach (KeyValuePair<(float, int), Settlement> item8 in sortedList)
		{
			Settlement value2 = item8.Value;
			float item6 = item8.Key.Item1;
			float num15 = 1.6f;
			if (!mobileParty.IsDisbanding)
			{
				IDisbandPartyCampaignBehavior disbandPartyCampaignBehavior = _disbandPartyCampaignBehavior;
				if (disbandPartyCampaignBehavior == null || !disbandPartyCampaignBehavior.IsPartyWaitingForDisband(mobileParty))
				{
					if (leaderHero == null)
					{
						AddBehaviorTupleWithScore(p, value2, CalculateMergeScoreForLeaderlessParty(mobileParty, value2, item6));
					}
					else
					{
						float num16 = item6;
						if (num16 >= 250f)
						{
							AddBehaviorTupleWithScore(p, value2, 0.025f);
							continue;
						}
						float num17 = num16;
						num16 = TaleWorlds.Library.MathF.Max(num12, num16);
						float num18 = TaleWorlds.Library.MathF.Max(0.1f, TaleWorlds.Library.MathF.Min(1f, num13 / (num13 - num12 + num16)));
						float num19 = num18;
						if (item < 0.6f)
						{
							num19 = TaleWorlds.Library.MathF.Pow(num18, TaleWorlds.Library.MathF.Pow(0.6f / TaleWorlds.Library.MathF.Max(0.15f, item), 0.3f));
						}
						bool flag = currentSettlementOfMobilePartyForAICalculation?.ItemRoster.TotalFood > item4 / Campaign.Current.Models.MobilePartyFoodConsumptionModel.NumberOfMenOnMapToEatOneFood * 3 || num3 > (float)(item4 / Campaign.Current.Models.MobilePartyFoodConsumptionModel.NumberOfMenOnMapToEatOneFood);
						float num20 = (float)item3 / (float)item4;
						float num21 = 1f + ((item4 > 0) ? (num20 * TaleWorlds.Library.MathF.Max(0.25f, num18 * num18) * TaleWorlds.Library.MathF.Pow(item3, 0.25f) * ((mobileParty.Army != null) ? 4f : 3f) * ((value2.IsFortification && flag) ? 18f : 0f)) : 0f);
						if (mobileParty.MapEvent != null || mobileParty.SiegeEvent != null)
						{
							num21 = TaleWorlds.Library.MathF.Sqrt(num21);
						}
						float num22 = 1f;
						if ((value2 == currentSettlementOfMobilePartyForAICalculation && currentSettlementOfMobilePartyForAICalculation.IsFortification) || (currentSettlementOfMobilePartyForAICalculation == null && value2 == mobileParty.TargetSettlement))
						{
							num22 = 1.2f;
						}
						else if (currentSettlementOfMobilePartyForAICalculation == null && value2 == mobileParty.LastVisitedSettlement)
						{
							num22 = 0.8f;
						}
						float num23 = 0.16f;
						float num24 = Math.Max(0f, num3) / num4;
						if (num4 > 0f && (mobileParty.BesiegedSettlement == null || num24 <= 1f) && num5 > 100 && (value2.IsTown || value2.IsVillage) && num24 < 4f)
						{
							int num25 = (int)(num4 * ((num24 < 1f && value2.IsVillage) ? Campaign.Current.Models.PartyFoodBuyingModel.MinimumDaysFoodToLastWhileBuyingFoodFromVillage : Campaign.Current.Models.PartyFoodBuyingModel.MinimumDaysFoodToLastWhileBuyingFoodFromTown)) + 1;
							float num26 = 3f - Math.Min(3f, Math.Max(0f, num24 - 1f));
							float num27 = (float)num25 + 20f * (float)((!value2.IsTown) ? 1 : 2) * ((num17 > 100f) ? 1f : (num17 / 100f));
							int val = (int)((float)(num5 - 100) / Campaign.Current.Models.PartyFoodBuyingModel.LowCostFoodPriceAverage);
							num23 += num26 * num26 * 0.093f * ((num24 < 2f) ? (1f + 0.5f * (2f - num24)) : 1f) * (float)Math.Pow(Math.Min(num27, Math.Min(val, value2.ItemRoster.TotalFood)) / num27, 0.5);
						}
						float num28 = 0f;
						int num29 = 0;
						int num30 = 0;
						if (item < 1f && mobileParty.CanPayMoreWage())
						{
							num29 = value2.NumberOfLordPartiesAt;
							num30 = value2.NumberOfLordPartiesTargeting;
							if (currentSettlementOfMobilePartyForAICalculation == value2)
							{
								num29 -= mobileParty.Army?.LeaderPartyAndAttachedPartiesCount ?? 1;
								if (num29 < 0)
								{
									num29 = 0;
								}
							}
							if (mobileParty.TargetSettlement == value2 || (mobileParty.Army != null && mobileParty.Army.LeaderParty.TargetSettlement == value2))
							{
								num30 -= mobileParty.Army?.LeaderPartyAndAttachedPartiesCount ?? 1;
								if (num30 < 0)
								{
									num30 = 0;
								}
							}
							if (!value2.IsCastle && !mobileParty.Party.IsStarving && (float)leaderHero.Gold > num11 && (leaderHero.Clan.Leader == leaderHero || (float)leaderHero.Clan.Gold > num10) && num8 > mobileParty.PartySizeRatio)
							{
								num28 = ApproximateNumberOfVolunteersCanBeRecruitedFromSettlement(leaderHero, value2);
								num28 = ((num28 > (float)(int)((num8 - mobileParty.PartySizeRatio) * 100f)) ? ((float)(int)((num8 - mobileParty.PartySizeRatio) * 100f)) : num28);
							}
						}
						float num31 = num28 * num18 / TaleWorlds.Library.MathF.Sqrt(1 + num29 + num30);
						float num32 = ((num31 < 1f) ? num31 : ((float)Math.Pow(num31, num2)));
						float num33 = Math.Max(Math.Min(1f, num23), Math.Max((mapFaction == value2.MapFaction) ? 0.25f : 0.16f, num * Math.Max(1f, Math.Min(2f, num)) * num32 * (1f - 0.9f * num20) * (1f - 0.9f * num20)));
						if (mobileParty.Army != null)
						{
							num33 /= (float)mobileParty.Army.LeaderPartyAndAttachedPartiesCount;
						}
						num15 *= num33 * num21 * num23 * num19;
						if (num15 >= 2.5f)
						{
							AddBehaviorTupleWithScore(p, value2, num15);
							break;
						}
						float num34 = 1f;
						if (num28 > 0f)
						{
							num34 = 1f + ((mobileParty.DefaultBehavior == AiBehavior.GoToSettlement && value2 != currentSettlementOfMobilePartyForAICalculation && num16 < num12) ? (0.1f * TaleWorlds.Library.MathF.Min(5f, num28) - 0.1f * TaleWorlds.Library.MathF.Min(5f, num28) * (num16 / num12) * (num16 / num12)) : 0f);
						}
						float num35 = (value2.IsCastle ? 1.4f : 1f);
						num15 *= (value2.IsTown ? num6 : 1f) * num34 * num35;
						if (num15 >= 2.5f)
						{
							AddBehaviorTupleWithScore(p, value2, num15);
							break;
						}
						int num36 = mobileParty.PrisonRoster.TotalRegulars;
						if (mobileParty.PrisonRoster.TotalHeroes > 0)
						{
							foreach (TroopRosterElement item9 in mobileParty.PrisonRoster.GetTroopRoster())
							{
								if (item9.Character.IsHero && item9.Character.HeroObject.Clan.IsAtWarWith(value2.MapFaction))
								{
									num36 += 6;
								}
							}
						}
						float num37 = 1f;
						float num38 = 1f;
						if (mobileParty.Army != null)
						{
							if (mobileParty.Army.LeaderParty != mobileParty)
							{
								num37 = ((float)mobileParty.Army.CohesionThresholdForDispersion - mobileParty.Army.Cohesion) / (float)mobileParty.Army.CohesionThresholdForDispersion;
							}
							num38 = ((MobileParty.MainParty != null && mobileParty.Army == MobileParty.MainParty.Army) ? 0.6f : 0.8f);
							foreach (MobileParty attachedParty3 in mobileParty.Army.LeaderParty.AttachedParties)
							{
								num36 += attachedParty3.PrisonRoster.TotalRegulars;
								if (attachedParty3.PrisonRoster.TotalHeroes <= 0)
								{
									continue;
								}
								foreach (TroopRosterElement item10 in attachedParty3.PrisonRoster.GetTroopRoster())
								{
									if (item10.Character.IsHero && item10.Character.HeroObject.Clan.IsAtWarWith(value2.MapFaction))
									{
										num36 += 6;
									}
								}
							}
						}
						float num39 = (value2.IsFortification ? (1f + 2f * (float)(num36 / num7)) : 1f);
						float num40 = 1f;
						if (mobileParty.Ai.NumberOfRecentFleeingFromAParty > 0)
						{
							Vec2 v = value2.Position2D - mobileParty.Position2D;
							v.Normalize();
							float num41 = mobileParty.AverageFleeTargetDirection.Distance(v);
							num40 = 1f - Math.Max(2f - num41, 0f) * 0.25f * (Math.Min(mobileParty.Ai.NumberOfRecentFleeingFromAParty, 10f) * 0.2f);
						}
						float num42 = 1f;
						float num43 = 1f;
						float num44 = 1f;
						float num45 = 1f;
						float num46 = 1f;
						if (num23 <= 0.5f)
						{
							(num42, num43, num44, num45) = CalculateBeingSettlementOwnerScores(mobileParty, value2, currentSettlementOfMobilePartyForAICalculation, -1f, num18, item);
						}
						else
						{
							float num47 = TaleWorlds.Library.MathF.Sqrt(num9);
							num46 = ((num47 > num14) ? (1f + 7f * TaleWorlds.Library.MathF.Min(1f, num23 - 0.5f)) : (1f + 7f * (num47 / num14) * TaleWorlds.Library.MathF.Min(1f, num23 - 0.5f)));
						}
						num15 *= num40 * num46 * num22 * num37 * num39 * num38 * num42 * num44 * num43 * num45;
					}
					goto IL_0cc7;
				}
			}
			AddBehaviorTupleWithScore(p, value2, CalculateMergeScoreForDisbandingParty(mobileParty, value2, item6));
			goto IL_0cc7;
			IL_0cc7:
			if (num15 > 0.025f)
			{
				AddBehaviorTupleWithScore(p, value2, num15);
			}
		}
	}

	private int ApproximateNumberOfVolunteersCanBeRecruitedFromSettlement(Hero hero, Settlement settlement)
	{
		int num = 4;
		if (hero.MapFaction != settlement.MapFaction)
		{
			num = 2;
		}
		int num2 = 0;
		foreach (Hero notable in settlement.Notables)
		{
			if (!notable.IsAlive)
			{
				continue;
			}
			for (int i = 0; i < num; i++)
			{
				if (notable.VolunteerTypes[i] != null)
				{
					num2++;
				}
			}
		}
		return num2;
	}

	private float CalculateSellItemScore(MobileParty mobileParty)
	{
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < mobileParty.ItemRoster.Count; i++)
		{
			ItemRosterElement itemRosterElement = mobileParty.ItemRoster[i];
			if (itemRosterElement.EquipmentElement.Item.IsMountable)
			{
				num2 += (float)(itemRosterElement.Amount * itemRosterElement.EquipmentElement.Item.Value);
			}
			else if (!itemRosterElement.EquipmentElement.Item.IsFood)
			{
				num += (float)(itemRosterElement.Amount * itemRosterElement.EquipmentElement.Item.Value);
			}
		}
		float num3 = ((num2 > (float)mobileParty.LeaderHero.Gold * 0.1f) ? TaleWorlds.Library.MathF.Min(3f, TaleWorlds.Library.MathF.Pow((num2 + 1000f) / ((float)mobileParty.LeaderHero.Gold * 0.1f + 1000f), 0.33f)) : 1f);
		float num4 = 1f + TaleWorlds.Library.MathF.Min(3f, TaleWorlds.Library.MathF.Pow(num / (((float)mobileParty.MemberRoster.TotalManCount + 5f) * 100f), 0.33f));
		float num5 = num3 * num4;
		if (mobileParty.Army != null)
		{
			num5 = TaleWorlds.Library.MathF.Sqrt(num5);
		}
		return num5;
	}

	private (float, float, int, int) CalculatePartyParameters(MobileParty mobileParty)
	{
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		float item;
		if (mobileParty.Army != null)
		{
			float num4 = 0f;
			foreach (MobileParty party in mobileParty.Army.Parties)
			{
				float partySizeRatio = party.PartySizeRatio;
				num4 += partySizeRatio;
				num2 += party.MemberRoster.TotalWounded;
				num3 += party.MemberRoster.TotalManCount;
				float num5 = PartyBaseHelper.FindPartySizeNormalLimit(party);
				num += num5;
			}
			item = num4 / (float)mobileParty.Army.Parties.Count;
			num /= (float)mobileParty.Army.Parties.Count;
		}
		else
		{
			item = mobileParty.PartySizeRatio;
			num2 += mobileParty.MemberRoster.TotalWounded;
			num3 += mobileParty.MemberRoster.TotalManCount;
			num += PartyBaseHelper.FindPartySizeNormalLimit(mobileParty);
		}
		return (item, num, num2, num3);
	}

	private void CalculateVisitHideoutScoresForBanditParty(MobileParty mobileParty, Settlement currentSettlement, PartyThinkParams p)
	{
		if (!mobileParty.MapFaction.Culture.CanHaveSettlement || (currentSettlement != null && currentSettlement.IsHideout))
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < mobileParty.ItemRoster.Count; i++)
		{
			ItemRosterElement itemRosterElement = mobileParty.ItemRoster[i];
			num += itemRosterElement.Amount * itemRosterElement.EquipmentElement.Item.Value;
		}
		float num2 = 1f + 4f * Math.Min(num, 1000f) / 1000f;
		int num3 = 0;
		MBReadOnlyList<Hideout> allHideouts = Campaign.Current.AllHideouts;
		foreach (Hideout item in allHideouts)
		{
			if (item.Settlement.Culture == mobileParty.Party.Culture && item.IsInfested)
			{
				num3++;
			}
		}
		float num4 = 1f + 4f * (float)Math.Sqrt(mobileParty.PrisonRoster.TotalManCount / mobileParty.Party.PrisonerSizeLimit);
		int numberOfMinimumBanditPartiesInAHideoutToInfestIt = Campaign.Current.Models.BanditDensityModel.NumberOfMinimumBanditPartiesInAHideoutToInfestIt;
		int numberOfMaximumBanditPartiesInEachHideout = Campaign.Current.Models.BanditDensityModel.NumberOfMaximumBanditPartiesInEachHideout;
		int numberOfMaximumHideoutsAtEachBanditFaction = Campaign.Current.Models.BanditDensityModel.NumberOfMaximumHideoutsAtEachBanditFaction;
		float num5 = (424f + 7.57f * Campaign.AverageDistanceBetweenTwoFortifications) / 2f;
		foreach (Hideout item2 in allHideouts)
		{
			Settlement settlement = item2.Settlement;
			if (settlement.Party.MapEvent != null || settlement.Culture != mobileParty.Party.Culture)
			{
				continue;
			}
			float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(mobileParty, settlement);
			distance = Math.Max(10f, distance);
			float num6 = num5 / (num5 + distance);
			int num7 = 0;
			foreach (MobileParty party in settlement.Parties)
			{
				if (party.IsBandit && !party.IsBanditBossParty)
				{
					num7++;
				}
			}
			float num9;
			if (num7 < numberOfMinimumBanditPartiesInAHideoutToInfestIt)
			{
				float num8 = (float)(numberOfMaximumHideoutsAtEachBanditFaction - num3) / (float)numberOfMaximumHideoutsAtEachBanditFaction;
				num9 = ((num3 < numberOfMaximumHideoutsAtEachBanditFaction) ? (0.25f + 0.75f * num8) : 0f);
			}
			else
			{
				num9 = Math.Max(0f, 1f * (1f - (float)(Math.Min(numberOfMaximumBanditPartiesInEachHideout, num7) - numberOfMinimumBanditPartiesInAHideoutToInfestIt) / (float)(numberOfMaximumBanditPartiesInEachHideout - numberOfMinimumBanditPartiesInAHideoutToInfestIt)));
			}
			float num10 = ((mobileParty.DefaultBehavior == AiBehavior.GoToSettlement && mobileParty.TargetSettlement == settlement) ? 1f : (MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat));
			float visitingNearbySettlementScore = num6 * num9 * num2 * num10 * num4;
			AddBehaviorTupleWithScore(p, item2.Settlement, visitingNearbySettlementScore);
		}
	}

	private (float, float, float, float) CalculateBeingSettlementOwnerScores(MobileParty mobileParty, Settlement settlement, Settlement currentSettlement, float idealGarrisonStrengthPerWalledCenter, float distanceScorePure, float averagePartySizeRatioToMaximumSize)
	{
		float num = 1f;
		float num2 = 1f;
		float num3 = 1f;
		float item = 1f;
		Hero leaderHero = mobileParty.LeaderHero;
		IFaction mapFaction = mobileParty.MapFaction;
		if (currentSettlement != settlement && (mobileParty.Army == null || mobileParty.Army.LeaderParty != mobileParty))
		{
			if (settlement.OwnerClan.Leader == leaderHero)
			{
				float currentTime = Campaign.CurrentTime;
				float lastVisitTimeOfOwner = settlement.LastVisitTimeOfOwner;
				float num4 = ((currentTime - lastVisitTimeOfOwner > 24f) ? (currentTime - lastVisitTimeOfOwner) : ((24f - (currentTime - lastVisitTimeOfOwner)) * 15f)) / 360f;
				num += num4;
			}
			if (MBRandom.RandomFloat < 0.1f && settlement.IsFortification && leaderHero.Clan != Clan.PlayerClan && (settlement.OwnerClan.Leader == leaderHero || settlement.OwnerClan == leaderHero.Clan))
			{
				if (idealGarrisonStrengthPerWalledCenter == -1f)
				{
					idealGarrisonStrengthPerWalledCenter = FactionHelper.FindIdealGarrisonStrengthPerWalledCenter(mapFaction as Kingdom);
				}
				int num5 = Campaign.Current.Models.SettlementGarrisonModel.FindNumberOfTroopsToTakeFromGarrison(mobileParty, settlement, idealGarrisonStrengthPerWalledCenter);
				if (num5 > 0)
				{
					num2 = 1f + TaleWorlds.Library.MathF.Pow(num5, 0.67f);
					if (mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty)
					{
						num2 = 1f + (num2 - 1f) / TaleWorlds.Library.MathF.Sqrt(mobileParty.Army.Parties.Count);
					}
				}
			}
		}
		if (settlement == leaderHero.HomeSettlement && mobileParty.Army == null)
		{
			float num6 = (leaderHero.HomeSettlement.IsCastle ? 1.5f : 1f);
			num3 = ((currentSettlement != settlement) ? (num3 + 1000f * num6 / (250f + leaderHero.PassedTimeAtHomeSettlement * leaderHero.PassedTimeAtHomeSettlement)) : (num3 + 3000f * num6 / (250f + leaderHero.PassedTimeAtHomeSettlement * leaderHero.PassedTimeAtHomeSettlement)));
		}
		if (settlement != currentSettlement)
		{
			float num7 = 1f;
			if (mobileParty.LastVisitedSettlement == settlement)
			{
				num7 = 0.25f;
			}
			if (settlement.IsFortification && settlement.MapFaction == mapFaction && settlement.OwnerClan != Clan.PlayerClan)
			{
				float num8 = ((settlement.Town.GarrisonParty != null) ? settlement.Town.GarrisonParty.Party.TotalStrength : 0f);
				float num9 = FactionHelper.OwnerClanEconomyEffectOnGarrisonSizeConstant(settlement.OwnerClan);
				float num10 = FactionHelper.SettlementProsperityEffectOnGarrisonSizeConstant(settlement.Town);
				float num11 = FactionHelper.SettlementFoodPotentialEffectOnGarrisonSizeConstant(settlement);
				if (idealGarrisonStrengthPerWalledCenter == -1f)
				{
					idealGarrisonStrengthPerWalledCenter = FactionHelper.FindIdealGarrisonStrengthPerWalledCenter(mapFaction as Kingdom);
				}
				float num12 = idealGarrisonStrengthPerWalledCenter;
				if (settlement.Town.GarrisonParty != null && settlement.Town.GarrisonParty.HasLimitedWage())
				{
					num12 = (float)settlement.Town.GarrisonParty.PaymentLimit / Campaign.Current.AverageWage;
				}
				else
				{
					if (mobileParty.Army != null)
					{
						num12 *= 0.75f;
					}
					num12 *= num9 * num10 * num11;
				}
				float num13 = num12;
				if (num8 < num13)
				{
					float num14 = ((settlement.OwnerClan == leaderHero.Clan) ? 149f : 99f);
					if (settlement.OwnerClan == Clan.PlayerClan)
					{
						num14 *= 0.5f;
					}
					float num15 = 1f - num8 / num13;
					item = 1f + num14 * distanceScorePure * distanceScorePure * averagePartySizeRatioToMaximumSize * num15 * num15 * num15 * num7;
				}
			}
		}
		return (num, num2, num3, item);
	}

	private float CalculateMergeScoreForDisbandingParty(MobileParty disbandParty, Settlement settlement, float distance)
	{
		float num = TaleWorlds.Library.MathF.Pow(3.5f - 0.95f * (Math.Min(Campaign.MapDiagonal, distance) / Campaign.MapDiagonal), 3f);
		float num2 = ((disbandParty.Party.Owner?.Clan == settlement.OwnerClan) ? 1f : ((disbandParty.Party.Owner?.MapFaction == settlement.MapFaction) ? 0.35f : 0.025f));
		float num3 = ((disbandParty.DefaultBehavior == AiBehavior.GoToSettlement && disbandParty.TargetSettlement == settlement) ? 1f : 0.3f);
		float num4 = (settlement.IsFortification ? 3f : 1f);
		float num5 = num * num2 * num3 * num4;
		if (num5 < 0.025f)
		{
			num5 = 0.035f;
		}
		return num5;
	}

	private float CalculateMergeScoreForLeaderlessParty(MobileParty leaderlessParty, Settlement settlement, float distance)
	{
		if (settlement.IsVillage)
		{
			return -1.6f;
		}
		float num = TaleWorlds.Library.MathF.Pow(3.5f - 0.95f * (Math.Min(Campaign.MapDiagonal, distance) / Campaign.MapDiagonal), 3f);
		float num2 = ((leaderlessParty.ActualClan == settlement.OwnerClan) ? 2f : ((leaderlessParty.ActualClan?.MapFaction == settlement.MapFaction) ? 0.35f : 0f));
		float num3 = ((leaderlessParty.DefaultBehavior == AiBehavior.GoToSettlement && leaderlessParty.TargetSettlement == settlement) ? 1f : 0.3f);
		float num4 = (settlement.IsFortification ? 3f : 0.5f);
		return num * num2 * num3 * num4;
	}

	private SortedList<(float, int), Settlement> FindSettlementsToVisitWithDistances(MobileParty mobileParty)
	{
		SortedList<(float, int), Settlement> sortedList = new SortedList<(float, int), Settlement>();
		MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
		if (mobileParty.LeaderHero != null && mobileParty.LeaderHero.MapFaction.IsKingdomFaction)
		{
			if (mobileParty.Army == null || mobileParty.Army.LeaderParty == mobileParty)
			{
				LocatableSearchData<Settlement> data = Settlement.StartFindingLocatablesAroundPosition(mobileParty.Position2D, 30f);
				for (Settlement settlement = Settlement.FindNextLocatable(ref data); settlement != null; settlement = Settlement.FindNextLocatable(ref data))
				{
					if (!settlement.IsCastle && settlement.MapFaction != mobileParty.MapFaction && IsSettlementSuitableForVisitingCondition(mobileParty, settlement))
					{
						float distance = mapDistanceModel.GetDistance(mobileParty, settlement);
						if (distance < 350f)
						{
							sortedList.Add((distance, settlement.GetHashCode()), settlement);
						}
					}
				}
			}
			foreach (Settlement settlement3 in mobileParty.MapFaction.Settlements)
			{
				if (IsSettlementSuitableForVisitingCondition(mobileParty, settlement3))
				{
					float distance2 = mapDistanceModel.GetDistance(mobileParty, settlement3);
					if (distance2 < 350f)
					{
						sortedList.Add((distance2, settlement3.GetHashCode()), settlement3);
					}
				}
			}
		}
		else
		{
			LocatableSearchData<Settlement> data2 = Settlement.StartFindingLocatablesAroundPosition(mobileParty.Position2D, 50f);
			for (Settlement settlement2 = Settlement.FindNextLocatable(ref data2); settlement2 != null; settlement2 = Settlement.FindNextLocatable(ref data2))
			{
				if (IsSettlementSuitableForVisitingCondition(mobileParty, settlement2))
				{
					float distance3 = mapDistanceModel.GetDistance(mobileParty, settlement2);
					if (distance3 < 350f)
					{
						sortedList.Add((distance3, settlement2.GetHashCode()), settlement2);
					}
				}
			}
		}
		return sortedList;
	}

	private void AddBehaviorTupleWithScore(PartyThinkParams p, Settlement settlement, float visitingNearbySettlementScore)
	{
		AIBehaviorTuple aiBehaviorTuple = new AIBehaviorTuple(settlement, AiBehavior.GoToSettlement);
		if (p.TryGetBehaviorScore(in aiBehaviorTuple, out var score))
		{
			p.SetBehaviorScore(in aiBehaviorTuple, score + visitingNearbySettlementScore);
			return;
		}
		(AIBehaviorTuple, float) value = (aiBehaviorTuple, visitingNearbySettlementScore);
		p.AddBehaviorScore(in value);
	}

	private bool IsSettlementSuitableForVisitingCondition(MobileParty mobileParty, Settlement settlement)
	{
		if (settlement.Party.MapEvent == null && settlement.Party.SiegeEvent == null && (!mobileParty.Party.Owner.MapFaction.IsAtWarWith(settlement.MapFaction) || (mobileParty.Party.Owner.MapFaction.IsMinorFaction && settlement.IsVillage)) && (settlement.IsVillage || settlement.IsFortification))
		{
			if (settlement.IsVillage)
			{
				return settlement.Village.VillageState == Village.VillageStates.Normal;
			}
			return true;
		}
		return false;
	}
}
