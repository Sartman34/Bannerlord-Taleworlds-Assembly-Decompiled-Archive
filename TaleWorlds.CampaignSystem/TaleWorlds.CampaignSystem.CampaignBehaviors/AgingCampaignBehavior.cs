using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class AgingCampaignBehavior : CampaignBehaviorBase
{
	private Dictionary<Hero, int> _extraLivesContainer = new Dictionary<Hero, int>();

	private Dictionary<Hero, int> _heroesYoungerThanHeroComesOfAge = new Dictionary<Hero, int>();

	private int _gameStartDay;

	public override void RegisterEvents()
	{
		CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, DailyTickHero);
		CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, OnCharacterCreationIsOver);
		CampaignEvents.HeroComesOfAgeEvent.AddNonSerializedListener(this, OnHeroComesOfAge);
		CampaignEvents.HeroReachesTeenAgeEvent.AddNonSerializedListener(this, OnHeroReachesTeenAge);
		CampaignEvents.HeroGrowsOutOfInfancyEvent.AddNonSerializedListener(this, OnHeroGrowsOutOfInfancy);
		CampaignEvents.PerkOpenedEvent.AddNonSerializedListener(this, OnPerkOpened);
		CampaignEvents.HeroCreated.AddNonSerializedListener(this, OnHeroCreated);
		CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
		CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_extraLivesContainer", ref _extraLivesContainer);
		dataStore.SyncData("_heroesYoungerThanHeroComesOfAge", ref _heroesYoungerThanHeroComesOfAge);
	}

	private void OnHeroCreated(Hero hero, bool isBornNaturally)
	{
		int num = (int)hero.Age;
		if (num < Campaign.Current.Models.AgeModel.HeroComesOfAge)
		{
			_heroesYoungerThanHeroComesOfAge.Add(hero, num);
		}
	}

	private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
	{
		if (_heroesYoungerThanHeroComesOfAge.ContainsKey(victim))
		{
			_heroesYoungerThanHeroComesOfAge.Remove(victim);
		}
	}

	private void AddExtraLife(Hero hero)
	{
		if (hero.IsAlive)
		{
			if (_extraLivesContainer.ContainsKey(hero))
			{
				_extraLivesContainer[hero]++;
			}
			else
			{
				_extraLivesContainer.Add(hero, 1);
			}
		}
	}

	private void OnPerkOpened(Hero hero, PerkObject perk)
	{
		if (perk == DefaultPerks.Medicine.CheatDeath)
		{
			AddExtraLife(hero);
		}
		if (perk != DefaultPerks.Medicine.HealthAdvise || hero.Clan?.Leader != hero)
		{
			return;
		}
		foreach (Hero item in hero.Clan.Heroes.Where((Hero x) => x.IsAlive))
		{
			AddExtraLife(item);
		}
	}

	private void DailyTickHero(Hero hero)
	{
		bool flag = (int)CampaignTime.Now.ToDays == _gameStartDay;
		if (CampaignOptions.IsLifeDeathCycleDisabled || flag || hero.IsTemplate)
		{
			return;
		}
		if (hero.IsAlive && hero.CanDie(KillCharacterAction.KillCharacterActionDetail.DiedOfOldAge))
		{
			if (hero.DeathMark != 0 && (hero.PartyBelongedTo == null || (hero.PartyBelongedTo.MapEvent == null && hero.PartyBelongedTo.SiegeEvent == null)))
			{
				KillCharacterAction.ApplyByDeathMark(hero);
			}
			else
			{
				IsItTimeOfDeath(hero);
			}
		}
		if (_heroesYoungerThanHeroComesOfAge.TryGetValue(hero, out var value))
		{
			int num = (int)hero.Age;
			if (value != num)
			{
				if (num >= Campaign.Current.Models.AgeModel.HeroComesOfAge)
				{
					_heroesYoungerThanHeroComesOfAge.Remove(hero);
					CampaignEventDispatcher.Instance.OnHeroComesOfAge(hero);
				}
				else
				{
					_heroesYoungerThanHeroComesOfAge[hero] = num;
					if (num == Campaign.Current.Models.AgeModel.BecomeTeenagerAge)
					{
						CampaignEventDispatcher.Instance.OnHeroReachesTeenAge(hero);
					}
					else if (num == Campaign.Current.Models.AgeModel.BecomeChildAge)
					{
						CampaignEventDispatcher.Instance.OnHeroGrowsOutOfInfancy(hero);
					}
				}
			}
		}
		if (hero != Hero.MainHero || !Hero.IsMainHeroIll || Hero.MainHero.HeroState == Hero.CharacterStates.Dead)
		{
			return;
		}
		Campaign.Current.MainHeroIllDays++;
		if (Campaign.Current.MainHeroIllDays <= 3)
		{
			return;
		}
		Hero.MainHero.HitPoints -= MathF.Ceiling((float)Hero.MainHero.HitPoints * (0.05f * (float)Campaign.Current.MainHeroIllDays));
		if (Hero.MainHero.HitPoints > 1 || Hero.MainHero.DeathMark != 0)
		{
			return;
		}
		if (_extraLivesContainer.TryGetValue(Hero.MainHero, out var value2))
		{
			if (value2 == 0)
			{
				KillMainHeroWithIllness();
				return;
			}
			Campaign.Current.MainHeroIllDays = -1;
			_extraLivesContainer[Hero.MainHero] = value2 - 1;
			if (_extraLivesContainer[Hero.MainHero] == 0)
			{
				_extraLivesContainer.Remove(Hero.MainHero);
			}
		}
		else
		{
			KillMainHeroWithIllness();
		}
	}

	private void KillMainHeroWithIllness()
	{
		Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
		Hero.MainHero.AddDeathMark(null, KillCharacterAction.KillCharacterActionDetail.DiedOfOldAge);
		KillCharacterAction.ApplyByOldAge(Hero.MainHero);
	}

	private void OnGameLoaded(CampaignGameStarter obj)
	{
		CheckYoungHeroes();
	}

	private void OnCharacterCreationIsOver()
	{
		_gameStartDay = (int)CampaignTime.Now.ToDays;
		if (!CampaignOptions.IsLifeDeathCycleDisabled)
		{
			InitializeHeroesYoungerThanHeroComesOfAge();
		}
	}

	private void OnHeroGrowsOutOfInfancy(Hero hero)
	{
		if (hero.Clan != Clan.PlayerClan)
		{
			hero.HeroDeveloper.InitializeHeroDeveloper(isByNaturalGrowth: true);
		}
	}

	private void OnHeroReachesTeenAge(Hero hero)
	{
		MBEquipmentRoster randomElementInefficiently = Campaign.Current.Models.EquipmentSelectionModel.GetEquipmentRostersForHeroReachesTeenAge(hero).GetRandomElementInefficiently();
		if (randomElementInefficiently != null)
		{
			Equipment randomElementInefficiently2 = randomElementInefficiently.GetCivilianEquipments().GetRandomElementInefficiently();
			EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, randomElementInefficiently2);
			new Equipment(isCivilian: false).FillFrom(randomElementInefficiently2, useSourceEquipmentType: false);
			EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, randomElementInefficiently2);
		}
		else
		{
			Debug.FailedAssert("Cant find child equipment template for " + hero.Name, "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CampaignBehaviors\\AgingCampaignBehavior.cs", "OnHeroReachesTeenAge", 217);
		}
		if (hero.Clan == Clan.PlayerClan)
		{
			return;
		}
		foreach (TraitObject item in DefaultTraits.Personality)
		{
			int num = hero.GetTraitLevel(item);
			if (hero.Father == null && hero.Mother == null && hero.Template != null)
			{
				hero.SetTraitLevel(item, hero.Template.GetTraitLevel(item));
				continue;
			}
			float randomFloat = MBRandom.RandomFloat;
			float randomFloat2 = MBRandom.RandomFloat;
			if ((double)randomFloat < 0.2 && hero.Father != null)
			{
				num = hero.Father.GetTraitLevel(item);
			}
			else if ((double)randomFloat < 0.6 && !hero.CharacterObject.IsFemale && hero.Father != null)
			{
				num = hero.Father.GetTraitLevel(item);
			}
			else if ((double)randomFloat < 0.6 && hero.Mother != null)
			{
				num = hero.Mother.GetTraitLevel(item);
			}
			else if ((double)randomFloat < 0.7 && hero.Mother != null)
			{
				num = hero.Mother.GetTraitLevel(item);
			}
			else if ((double)randomFloat2 < 0.3)
			{
				num--;
			}
			else if ((double)randomFloat2 >= 0.7)
			{
				num++;
			}
			num = MBMath.ClampInt(num, item.MinValue, item.MaxValue);
			if (num != hero.GetTraitLevel(item))
			{
				hero.SetTraitLevel(item, num);
			}
		}
		hero.HeroDeveloper.InitializeHeroDeveloper(isByNaturalGrowth: true);
	}

	private void OnHeroComesOfAge(Hero hero)
	{
		if (hero.HeroState == Hero.CharacterStates.Active)
		{
			return;
		}
		bool flag = !hero.IsFemale || hero.Clan == Hero.MainHero.Clan || (hero.Mother != null && !hero.Mother.IsNoncombatant) || (hero.RandomIntWithSeed(17u, 0, 1) == 0 && hero.GetTraitLevel(DefaultTraits.Valor) == 1);
		if (hero.Clan != Clan.PlayerClan)
		{
			foreach (TraitObject skillCategory in DefaultTraits.SkillCategories)
			{
				hero.SetTraitLevel(skillCategory, 0);
			}
			if (flag)
			{
				hero.SetTraitLevel(DefaultTraits.CavalryFightingSkills, 5);
				int value = MathF.Max(DefaultTraits.Commander.MinValue, 3 + hero.GetTraitLevel(DefaultTraits.Valor) + hero.GetTraitLevel(DefaultTraits.Generosity) + hero.RandomIntWithSeed(18u, -1, 2));
				hero.SetTraitLevel(DefaultTraits.Commander, value);
			}
			int value2 = MathF.Max(DefaultTraits.Manager.MinValue, 3 + hero.GetTraitLevel(DefaultTraits.Honor) + hero.RandomIntWithSeed(19u, -1, 2));
			hero.SetTraitLevel(DefaultTraits.Manager, value2);
			int value3 = MathF.Max(DefaultTraits.Politician.MinValue, 3 + hero.GetTraitLevel(DefaultTraits.Calculating) + hero.RandomIntWithSeed(20u, -1, 2));
			hero.SetTraitLevel(DefaultTraits.Politician, value3);
			hero.HeroDeveloper.InitializeHeroDeveloper(isByNaturalGrowth: true);
		}
		else
		{
			hero.HeroDeveloper.SetInitialLevel(hero.Level);
		}
		MBList<MBEquipmentRoster> equipmentRostersForHeroComeOfAge = Campaign.Current.Models.EquipmentSelectionModel.GetEquipmentRostersForHeroComeOfAge(hero, isCivilian: false);
		MBList<MBEquipmentRoster> equipmentRostersForHeroComeOfAge2 = Campaign.Current.Models.EquipmentSelectionModel.GetEquipmentRostersForHeroComeOfAge(hero, isCivilian: true);
		MBEquipmentRoster randomElement = equipmentRostersForHeroComeOfAge.GetRandomElement();
		MBEquipmentRoster randomElement2 = equipmentRostersForHeroComeOfAge2.GetRandomElement();
		Equipment randomElement3 = randomElement.AllEquipments.GetRandomElement();
		Equipment randomElement4 = randomElement2.AllEquipments.GetRandomElement();
		EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, randomElement3);
		EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, randomElement4);
	}

	private void IsItTimeOfDeath(Hero hero)
	{
		if (!hero.IsAlive || !(hero.Age >= (float)Campaign.Current.Models.AgeModel.BecomeOldAge) || CampaignOptions.IsLifeDeathCycleDisabled || hero.DeathMark != 0 || !(MBRandom.RandomFloat < hero.ProbabilityOfDeath))
		{
			return;
		}
		if (_extraLivesContainer.TryGetValue(hero, out var value) && value > 0)
		{
			_extraLivesContainer[hero] = value - 1;
			if (_extraLivesContainer[hero] == 0)
			{
				_extraLivesContainer.Remove(hero);
			}
		}
		else if (hero == Hero.MainHero && !Hero.IsMainHeroIll)
		{
			Campaign.Current.MainHeroIllDays++;
			Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
			InformationManager.ShowInquiry(new InquiryData(new TextObject("{=2duoimiP}Caught Illness").ToString(), new TextObject("{=vo3MqtMn}You are at death's door, wracked by fever, drifting in and out of consciousness. The healers do not believe that you can recover. You should resolve your final affairs and determine a heir for your clan while you still have the strength to speak.").ToString(), isAffirmativeOptionShown: true, isNegativeOptionShown: false, new TextObject("{=yQtzabbe}Close").ToString(), "", null, null, "event:/ui/notification/quest_fail"));
		}
		else if (hero != Hero.MainHero)
		{
			KillCharacterAction.ApplyByOldAge(hero);
		}
	}

	private void MainHeroHealCheck()
	{
		if (MBRandom.RandomFloat <= 0.05f && Hero.MainHero.IsAlive)
		{
			Campaign.Current.MainHeroIllDays = -1;
			InformationManager.ShowInquiry(new InquiryData(new TextObject("{=M5eUjgQl}Cured").ToString(), new TextObject("{=T5H3L9Kw}The fever has broken. You are weak but you feel you will recover. You rise from your bed from the first time in days, blinking in the sunlight.").ToString(), isAffirmativeOptionShown: true, isNegativeOptionShown: false, new TextObject("{=yQtzabbe}Close").ToString(), "", null, null, "event:/ui/notification/quest_finished"));
			Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
		}
	}

	private void InitializeHeroesYoungerThanHeroComesOfAge()
	{
		foreach (Hero allAliveHero in Hero.AllAliveHeroes)
		{
			int num = (int)allAliveHero.Age;
			if (num < Campaign.Current.Models.AgeModel.HeroComesOfAge && !_heroesYoungerThanHeroComesOfAge.ContainsKey(allAliveHero))
			{
				_heroesYoungerThanHeroComesOfAge.Add(allAliveHero, num);
			}
		}
		foreach (Hero deadOrDisabledHero in Hero.DeadOrDisabledHeroes)
		{
			if (!deadOrDisabledHero.IsDead && deadOrDisabledHero.Age < (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && !_heroesYoungerThanHeroComesOfAge.ContainsKey(deadOrDisabledHero))
			{
				_heroesYoungerThanHeroComesOfAge.Add(deadOrDisabledHero, (int)deadOrDisabledHero.Age);
			}
		}
	}

	private void CheckYoungHeroes()
	{
		foreach (Hero item in Hero.FindAll((Hero x) => !x.IsDead && x.Age < (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && !_heroesYoungerThanHeroComesOfAge.ContainsKey(x)))
		{
			_heroesYoungerThanHeroComesOfAge.Add(item, (int)item.Age);
			if (!item.IsDisabled && !_heroesYoungerThanHeroComesOfAge.ContainsKey(item))
			{
				if (item.Age > (float)Campaign.Current.Models.AgeModel.BecomeChildAge)
				{
					CampaignEventDispatcher.Instance.OnHeroGrowsOutOfInfancy(item);
				}
				if (item.Age > (float)Campaign.Current.Models.AgeModel.BecomeTeenagerAge)
				{
					CampaignEventDispatcher.Instance.OnHeroReachesTeenAge(item);
				}
			}
		}
	}
}
