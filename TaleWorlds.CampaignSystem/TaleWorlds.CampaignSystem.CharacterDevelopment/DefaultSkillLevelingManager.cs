using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CharacterDevelopment;

public class DefaultSkillLevelingManager : ISkillLevelingManager
{
	private const float TacticsXpCoefficient = 0.02f;

	public void OnCombatHit(CharacterObject affectorCharacter, CharacterObject affectedCharacter, CharacterObject captain, Hero commander, float speedBonusFromMovement, float shotDifficulty, WeaponComponentData affectorWeapon, float hitPointRatio, CombatXpModel.MissionTypeEnum missionType, bool isAffectorMounted, bool isTeamKill, bool isAffectorUnderCommand, float damageAmount, bool isFatal, bool isSiegeEngineHit, bool isHorseCharge)
	{
		if (isTeamKill)
		{
			return;
		}
		float num = 1f;
		if (affectorCharacter.IsHero)
		{
			Hero heroObject = affectorCharacter.HeroObject;
			Campaign.Current.Models.CombatXpModel.GetXpFromHit(heroObject.CharacterObject, captain, affectedCharacter, heroObject.PartyBelongedTo?.Party, (int)damageAmount, isFatal, missionType, out var xpAmount);
			num = xpAmount;
			SkillObject skillObject = null;
			if (affectorWeapon != null)
			{
				skillObject = Campaign.Current.Models.CombatXpModel.GetSkillForWeapon(affectorWeapon, isSiegeEngineHit);
				float num2 = ((skillObject == DefaultSkills.Bow) ? 0.5f : 1f);
				if (shotDifficulty > 0f)
				{
					num += (float)MathF.Floor(num * num2 * Campaign.Current.Models.CombatXpModel.GetXpMultiplierFromShotDifficulty(shotDifficulty));
				}
			}
			else
			{
				skillObject = (isHorseCharge ? DefaultSkills.Riding : DefaultSkills.Athletics);
			}
			heroObject.AddSkillXp(skillObject, MBRandom.RoundRandomized(num));
			if (!isSiegeEngineHit && !isHorseCharge)
			{
				float num3 = shotDifficulty * 0.15f;
				if (isAffectorMounted)
				{
					float num4 = 0.5f;
					if (num3 > 0f)
					{
						num4 += num3;
					}
					if (speedBonusFromMovement > 0f)
					{
						num4 *= 1f + speedBonusFromMovement;
					}
					if (num4 > 0f)
					{
						OnGainingRidingExperience(heroObject, MBRandom.RoundRandomized(num4 * num), heroObject.CharacterObject.Equipment.Horse.Item);
					}
				}
				else
				{
					float num5 = 1f;
					if (num3 > 0f)
					{
						num5 += num3;
					}
					if (speedBonusFromMovement > 0f)
					{
						num5 += 1.5f * speedBonusFromMovement;
					}
					if (num5 > 0f)
					{
						heroObject.AddSkillXp(DefaultSkills.Athletics, MBRandom.RoundRandomized(num5 * num));
					}
				}
			}
		}
		if (commander != null && commander != affectorCharacter.HeroObject && commander.PartyBelongedTo != null)
		{
			OnTacticsUsed(commander.PartyBelongedTo, MathF.Ceiling(0.02f * num));
		}
	}

	public void OnSiegeEngineDestroyed(MobileParty party, SiegeEngineType destroyedSiegeEngine)
	{
		if (party?.EffectiveEngineer != null)
		{
			float skillXp = (float)destroyedSiegeEngine.ManDayCost * 20f;
			OnPartySkillExercised(party, DefaultSkills.Engineering, skillXp, SkillEffect.PerkRole.Engineer);
		}
	}

	public void OnSimulationCombatKill(CharacterObject affectorCharacter, CharacterObject affectedCharacter, PartyBase affectorParty, PartyBase commanderParty)
	{
		int xpReward = Campaign.Current.Models.PartyTrainingModel.GetXpReward(affectedCharacter);
		if (affectorCharacter.IsHero)
		{
			ItemObject defaultWeapon = CharacterHelper.GetDefaultWeapon(affectorCharacter);
			Hero heroObject = affectorCharacter.HeroObject;
			if (defaultWeapon != null)
			{
				SkillObject skillForWeapon = Campaign.Current.Models.CombatXpModel.GetSkillForWeapon(defaultWeapon.GetWeaponWithUsageIndex(0), isSiegeEngineHit: false);
				heroObject.AddSkillXp(skillForWeapon, xpReward);
			}
			if (affectorCharacter.IsMounted)
			{
				float f = (float)xpReward * 0.3f;
				OnGainingRidingExperience(heroObject, MBRandom.RoundRandomized(f), heroObject.CharacterObject.Equipment.Horse.Item);
			}
			else
			{
				float f2 = (float)xpReward * 0.3f;
				heroObject.AddSkillXp(DefaultSkills.Athletics, MBRandom.RoundRandomized(f2));
			}
		}
		if (commanderParty != null && commanderParty.IsMobile && commanderParty.LeaderHero != null && commanderParty.LeaderHero != affectedCharacter.HeroObject)
		{
			OnTacticsUsed(commanderParty.MobileParty, MathF.Ceiling(0.02f * (float)xpReward));
		}
	}

	public void OnTradeProfitMade(PartyBase party, int tradeProfit)
	{
		if (tradeProfit > 0)
		{
			float skillXp = (float)tradeProfit * 0.5f;
			OnPartySkillExercised(party.MobileParty, DefaultSkills.Trade, skillXp);
		}
	}

	public void OnTradeProfitMade(Hero hero, int tradeProfit)
	{
		if (tradeProfit > 0)
		{
			float skillXp = (float)tradeProfit * 0.5f;
			OnPersonalSkillExercised(hero, DefaultSkills.Trade, skillXp, hero == Hero.MainHero);
		}
	}

	public void OnSettlementProjectFinished(Settlement settlement)
	{
		OnSettlementSkillExercised(settlement, DefaultSkills.Steward, 1000f);
	}

	public void OnSettlementGoverned(Hero governor, Settlement settlement)
	{
		float prosperityChange = settlement.Town.ProsperityChange;
		if (prosperityChange > 0f)
		{
			float skillXp = prosperityChange * 30f;
			OnPersonalSkillExercised(governor, DefaultSkills.Steward, skillXp);
		}
	}

	public void OnInfluenceSpent(Hero hero, float amountSpent)
	{
		if (hero.PartyBelongedTo != null)
		{
			float skillXp = 10f * amountSpent;
			OnPartySkillExercised(hero.PartyBelongedTo, DefaultSkills.Steward, skillXp);
		}
	}

	public void OnGainRelation(Hero hero, Hero gainedRelationWith, float relationChange, ChangeRelationAction.ChangeRelationDetail detail = ChangeRelationAction.ChangeRelationDetail.Default)
	{
		if ((hero.PartyBelongedTo != null || detail == ChangeRelationAction.ChangeRelationDetail.Emissary) && !(relationChange <= 0f))
		{
			int charmExperienceFromRelationGain = Campaign.Current.Models.DiplomacyModel.GetCharmExperienceFromRelationGain(gainedRelationWith, relationChange, detail);
			if (hero.PartyBelongedTo != null)
			{
				OnPartySkillExercised(hero.PartyBelongedTo, DefaultSkills.Charm, charmExperienceFromRelationGain);
			}
			else
			{
				OnPersonalSkillExercised(hero, DefaultSkills.Charm, charmExperienceFromRelationGain);
			}
		}
	}

	public void OnTroopRecruited(Hero hero, int amount, int tier)
	{
		if (amount > 0)
		{
			int num = amount * tier * 2;
			OnPersonalSkillExercised(hero, DefaultSkills.Leadership, num);
		}
	}

	public void OnBribeGiven(int amount)
	{
		if (amount > 0)
		{
			float skillXp = (float)amount * 0.1f;
			OnPartySkillExercised(MobileParty.MainParty, DefaultSkills.Roguery, skillXp);
		}
	}

	public void OnBanditsRecruited(MobileParty mobileParty, CharacterObject bandit, int count)
	{
		if (count > 0)
		{
			OnPersonalSkillExercised(mobileParty.LeaderHero, DefaultSkills.Roguery, count * 2 * bandit.Tier);
		}
	}

	public void OnMainHeroReleasedFromCaptivity(float captivityTime)
	{
		float skillXp = captivityTime * 0.5f;
		OnPersonalSkillExercised(Hero.MainHero, DefaultSkills.Roguery, skillXp);
	}

	public void OnMainHeroTortured()
	{
		float skillXp = MBRandom.RandomFloatRanged(50f, 100f);
		OnPersonalSkillExercised(Hero.MainHero, DefaultSkills.Roguery, skillXp);
	}

	public void OnMainHeroDisguised(bool isNotCaught)
	{
		float skillXp = (isNotCaught ? MBRandom.RandomFloatRanged(10f, 25f) : MBRandom.RandomFloatRanged(1f, 10f));
		OnPartySkillExercised(MobileParty.MainParty, DefaultSkills.Roguery, skillXp);
	}

	public void OnRaid(MobileParty attackerParty, ItemRoster lootedItems)
	{
		if (attackerParty.LeaderHero != null)
		{
			float skillXp = (float)lootedItems.TradeGoodsTotalValue * 0.5f;
			OnPersonalSkillExercised(attackerParty.LeaderHero, DefaultSkills.Roguery, skillXp);
		}
	}

	public void OnLoot(MobileParty attackerParty, MobileParty forcedParty, ItemRoster lootedItems, bool attacked)
	{
		if (attackerParty.LeaderHero != null)
		{
			float num = 0f;
			if (forcedParty.IsVillager)
			{
				num = (attacked ? 0.75f : 0.5f);
			}
			else if (forcedParty.IsCaravan)
			{
				num = (attacked ? 0.15f : 0.1f);
			}
			float skillXp = (float)lootedItems.TradeGoodsTotalValue * num;
			OnPersonalSkillExercised(attackerParty.LeaderHero, DefaultSkills.Roguery, skillXp);
		}
	}

	public void OnPrisonerSell(MobileParty mobileParty, in TroopRoster prisonerRoster)
	{
		int num = 0;
		for (int i = 0; i < prisonerRoster.Count; i++)
		{
			num += prisonerRoster.data[i].Character.Tier * prisonerRoster.data[i].Number;
		}
		int num2 = num * 2;
		OnPartySkillExercised(mobileParty, DefaultSkills.Roguery, num2);
	}

	public void OnSurgeryApplied(MobileParty party, bool surgerySuccess, int troopTier)
	{
		float skillXp = (surgerySuccess ? (10 * troopTier) : (5 * troopTier));
		OnPartySkillExercised(party, DefaultSkills.Medicine, skillXp, SkillEffect.PerkRole.Surgeon);
	}

	public void OnTacticsUsed(MobileParty party, float xp)
	{
		if (xp > 0f)
		{
			OnPartySkillExercised(party, DefaultSkills.Tactics, xp);
		}
	}

	public void OnHideoutSpotted(MobileParty party, PartyBase spottedParty)
	{
		OnPartySkillExercised(party, DefaultSkills.Scouting, 100f, SkillEffect.PerkRole.Scout);
	}

	public void OnTrackDetected(Track track)
	{
		float skillFromTrackDetected = Campaign.Current.Models.MapTrackModel.GetSkillFromTrackDetected(track);
		OnPartySkillExercised(MobileParty.MainParty, DefaultSkills.Scouting, skillFromTrackDetected, SkillEffect.PerkRole.Scout);
	}

	public void OnTravelOnFoot(Hero hero, float speed)
	{
		hero.AddSkillXp(DefaultSkills.Athletics, MBRandom.RoundRandomized(0.2f * speed) + 1);
	}

	public void OnTravelOnHorse(Hero hero, float speed)
	{
		ItemObject item = hero.CharacterObject.Equipment.Horse.Item;
		OnGainingRidingExperience(hero, MBRandom.RoundRandomized(0.3f * speed), item);
	}

	public void OnHeroHealedWhileWaiting(Hero hero, int healingAmount)
	{
		if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.EffectiveSurgeon != null)
		{
			float num = Campaign.Current.Models.PartyHealingModel.GetSkillXpFromHealingTroop(hero.PartyBelongedTo.Party);
			float num2 = ((hero.PartyBelongedTo.CurrentSettlement != null && !hero.PartyBelongedTo.CurrentSettlement.IsCastle) ? 0.2f : 0.1f);
			num *= (float)healingAmount * num2 * (1f + (float)hero.PartyBelongedTo.EffectiveSurgeon.Level * 0.1f);
			OnPartySkillExercised(hero.PartyBelongedTo, DefaultSkills.Medicine, num, SkillEffect.PerkRole.Surgeon);
		}
	}

	public void OnRegularTroopHealedWhileWaiting(MobileParty mobileParty, int healedTroopCount, float averageTier)
	{
		float num = (float)(Campaign.Current.Models.PartyHealingModel.GetSkillXpFromHealingTroop(mobileParty.Party) * healedTroopCount) * averageTier;
		float num2 = ((mobileParty.CurrentSettlement != null && !mobileParty.CurrentSettlement.IsCastle) ? 2f : 1f);
		num *= num2;
		OnPartySkillExercised(mobileParty, DefaultSkills.Medicine, num, SkillEffect.PerkRole.Surgeon);
	}

	public void OnLeadingArmy(MobileParty mobileParty)
	{
		float skillXp = mobileParty.GetTotalStrengthWithFollowers() * 0.0004f * mobileParty.Army.Morale;
		OnPartySkillExercised(mobileParty, DefaultSkills.Leadership, skillXp);
	}

	public void OnSieging(MobileParty mobileParty)
	{
		int num = mobileParty.MemberRoster.TotalManCount;
		if (mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty)
		{
			foreach (MobileParty party in mobileParty.Army.Parties)
			{
				if (party != mobileParty)
				{
					num += party.MemberRoster.TotalManCount;
				}
			}
		}
		float skillXp = 0.25f * MathF.Sqrt(num);
		OnPartySkillExercised(mobileParty, DefaultSkills.Engineering, skillXp, SkillEffect.PerkRole.Engineer);
	}

	public void OnSiegeEngineBuilt(MobileParty mobileParty, SiegeEngineType siegeEngine)
	{
		float skillXp = 30f + 2f * (float)siegeEngine.Difficulty;
		OnPartySkillExercised(mobileParty, DefaultSkills.Engineering, skillXp, SkillEffect.PerkRole.Engineer);
	}

	public void OnUpgradeTroops(PartyBase party, CharacterObject troop, CharacterObject upgrade, int numberOfTroops)
	{
		Hero hero = party.LeaderHero ?? party.Owner;
		if (hero != null)
		{
			SkillObject skill = DefaultSkills.Leadership;
			float num = 0.025f;
			if (troop.Occupation == Occupation.Bandit)
			{
				skill = DefaultSkills.Roguery;
				num = 0.05f;
			}
			float xpAmount = (float)Campaign.Current.Models.PartyTroopUpgradeModel.GetXpCostForUpgrade(party, troop, upgrade) * num * (float)numberOfTroops;
			hero.AddSkillXp(skill, xpAmount);
		}
	}

	public void OnPersuasionSucceeded(Hero targetHero, SkillObject skill, PersuasionDifficulty difficulty, int argumentDifficultyBonusCoefficient)
	{
		float num = Campaign.Current.Models.PersuasionModel.GetSkillXpFromPersuasion(difficulty, argumentDifficultyBonusCoefficient);
		if (num > 0f)
		{
			targetHero.AddSkillXp(skill, num);
		}
	}

	public void OnPrisonBreakEnd(Hero prisonerHero, bool isSucceeded)
	{
		float rogueryRewardOnPrisonBreak = Campaign.Current.Models.PrisonBreakModel.GetRogueryRewardOnPrisonBreak(prisonerHero, isSucceeded);
		if (rogueryRewardOnPrisonBreak > 0f)
		{
			Hero.MainHero.AddSkillXp(DefaultSkills.Roguery, rogueryRewardOnPrisonBreak);
		}
	}

	public void OnWallBreached(MobileParty party)
	{
		if (party?.EffectiveEngineer != null)
		{
			OnPartySkillExercised(party, DefaultSkills.Engineering, 250f, SkillEffect.PerkRole.Engineer);
		}
	}

	public void OnForceVolunteers(MobileParty attackerParty, PartyBase forcedParty)
	{
		if (attackerParty.LeaderHero != null)
		{
			int num = MathF.Ceiling(forcedParty.Settlement.Village.Hearth / 10f);
			OnPersonalSkillExercised(attackerParty.LeaderHero, DefaultSkills.Roguery, num);
		}
	}

	public void OnForceSupplies(MobileParty attackerParty, ItemRoster lootedItems, bool attacked)
	{
		if (attackerParty.LeaderHero != null)
		{
			float num = (attacked ? 0.75f : 0.5f);
			float skillXp = (float)lootedItems.TradeGoodsTotalValue * num;
			OnPersonalSkillExercised(attackerParty.LeaderHero, DefaultSkills.Roguery, skillXp);
		}
	}

	public void OnAIPartiesTravel(Hero hero, bool isCaravanParty, TerrainType currentTerrainType)
	{
		int num = ((currentTerrainType == TerrainType.Forest) ? MBRandom.RoundRandomized(5f) : MBRandom.RoundRandomized(3f));
		hero.AddSkillXp(DefaultSkills.Scouting, isCaravanParty ? ((float)num / 2f) : ((float)num));
	}

	public void OnTraverseTerrain(MobileParty mobileParty, TerrainType currentTerrainType)
	{
		float num = 0f;
		float speed = mobileParty.Speed;
		if (speed > 1f)
		{
			bool flag = currentTerrainType == TerrainType.Desert || currentTerrainType == TerrainType.Dune || currentTerrainType == TerrainType.Forest || currentTerrainType == TerrainType.Snow;
			num = speed * (1f + MathF.Pow(mobileParty.MemberRoster.TotalManCount, 0.66f)) * (flag ? 0.25f : 0.15f);
		}
		if (mobileParty.IsCaravan)
		{
			num *= 0.5f;
		}
		if (num >= 5f)
		{
			OnPartySkillExercised(mobileParty, DefaultSkills.Scouting, num, SkillEffect.PerkRole.Scout);
		}
	}

	public void OnBattleEnd(PartyBase party, FlattenedTroopRoster flattenedTroopRoster)
	{
		Hero hero = party.LeaderHero ?? party.Owner;
		if (hero == null || !hero.IsAlive)
		{
			return;
		}
		Dictionary<SkillObject, float> dictionary = new Dictionary<SkillObject, float>();
		foreach (FlattenedTroopRosterElement item in flattenedTroopRoster)
		{
			CharacterObject troop = item.Troop;
			int gainableMaxXp;
			bool flag = MobilePartyHelper.CanTroopGainXp(party, troop, out gainableMaxXp);
			if (!item.IsKilled && item.XpGained > 0 && !flag)
			{
				float num = ((troop.Occupation == Occupation.Bandit) ? 0.05f : 0.025f);
				float num2 = (float)item.XpGained * num;
				SkillObject key = ((troop.Occupation == Occupation.Bandit) ? DefaultSkills.Roguery : DefaultSkills.Leadership);
				if (dictionary.TryGetValue(key, out var value))
				{
					dictionary[key] = value + num2;
				}
				else
				{
					dictionary[key] = num2;
				}
			}
		}
		foreach (SkillObject key2 in dictionary.Keys)
		{
			if (dictionary[key2] > 0f)
			{
				hero.AddSkillXp(key2, dictionary[key2]);
			}
		}
	}

	public void OnFoodConsumed(MobileParty mobileParty, bool wasStarving)
	{
		if (!wasStarving && mobileParty.ItemRoster.FoodVariety > 3 && mobileParty.EffectiveQuartermaster != null)
		{
			float skillXp = MathF.Round((0f - mobileParty.BaseFoodChange) * 100f) * (mobileParty.ItemRoster.FoodVariety - 2) / 3;
			OnPartySkillExercised(mobileParty, DefaultSkills.Steward, skillXp, SkillEffect.PerkRole.Quartermaster);
		}
	}

	public void OnAlleyCleared(Alley alley)
	{
		Hero.MainHero.AddSkillXp(DefaultSkills.Roguery, Campaign.Current.Models.AlleyModel.GetInitialXpGainForMainHero());
	}

	public void OnDailyAlleyTick(Alley alley, Hero alleyLeader)
	{
		Hero.MainHero.AddSkillXp(DefaultSkills.Roguery, Campaign.Current.Models.AlleyModel.GetDailyXpGainForMainHero());
		if (alleyLeader != null && !alleyLeader.IsDead)
		{
			alleyLeader.AddSkillXp(DefaultSkills.Roguery, Campaign.Current.Models.AlleyModel.GetDailyXpGainForAssignedClanMember(alleyLeader));
		}
	}

	public void OnBoardGameWonAgainstLord(Hero lord, BoardGameHelper.AIDifficulty difficulty, bool extraXpGain)
	{
		switch (difficulty)
		{
		case BoardGameHelper.AIDifficulty.Easy:
			Hero.MainHero.AddSkillXp(DefaultSkills.Steward, 20f);
			break;
		case BoardGameHelper.AIDifficulty.Normal:
			Hero.MainHero.AddSkillXp(DefaultSkills.Steward, 50f);
			break;
		case BoardGameHelper.AIDifficulty.Hard:
			Hero.MainHero.AddSkillXp(DefaultSkills.Steward, 100f);
			break;
		}
		if (extraXpGain)
		{
			lord.AddSkillXp(DefaultSkills.Steward, 100f);
		}
	}

	public void OnWarehouseProduction(EquipmentElement production)
	{
		Hero.MainHero.AddSkillXp(DefaultSkills.Trade, Campaign.Current.Models.WorkshopModel.GetTradeXpPerWarehouseProduction(production));
	}

	private static void OnPersonalSkillExercised(Hero hero, SkillObject skill, float skillXp, bool shouldNotify = true)
	{
		hero?.HeroDeveloper.AddSkillXp(skill, skillXp, isAffectedByFocusFactor: true, shouldNotify);
	}

	private static void OnSettlementSkillExercised(Settlement settlement, SkillObject skill, float skillXp)
	{
		(settlement.Town?.Governor ?? ((settlement.OwnerClan.Leader.CurrentSettlement == settlement) ? settlement.OwnerClan.Leader : null))?.AddSkillXp(skill, skillXp);
	}

	private static void OnGainingRidingExperience(Hero hero, float baseXpAmount, ItemObject horse)
	{
		if (horse != null)
		{
			float num = 1f + (float)horse.Difficulty * 0.02f;
			hero.AddSkillXp(DefaultSkills.Riding, baseXpAmount * num);
		}
	}

	private static void OnPartySkillExercised(MobileParty party, SkillObject skill, float skillXp, SkillEffect.PerkRole perkRole = SkillEffect.PerkRole.PartyLeader)
	{
		party.GetEffectiveRoleHolder(perkRole)?.AddSkillXp(skill, skillXp);
	}

	void ISkillLevelingManager.OnPrisonerSell(MobileParty mobileParty, in TroopRoster prisonerRoster)
	{
		OnPrisonerSell(mobileParty, in prisonerRoster);
	}
}
