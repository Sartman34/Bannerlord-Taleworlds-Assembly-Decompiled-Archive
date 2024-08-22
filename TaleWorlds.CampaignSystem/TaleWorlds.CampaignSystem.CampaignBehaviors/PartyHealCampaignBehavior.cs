using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class PartyHealCampaignBehavior : CampaignBehaviorBase
{
	private Dictionary<PartyBase, float> _overflowedHealingForRegulars = new Dictionary<PartyBase, float>();

	private Dictionary<PartyBase, float> _overflowedHealingForHeroes = new Dictionary<PartyBase, float>();

	public override void RegisterEvents()
	{
		CampaignEvents.HourlyTickClanEvent.AddNonSerializedListener(this, OnClanHourlyTick);
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
		CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, OnWeeklyTick);
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, OnMobilePartyDestroyed);
		CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
		CampaignEvents.OnQuarterDailyPartyTick.AddNonSerializedListener(this, OnQuarterDailyPartyTick);
		CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this, OnPlayerBattleEnd);
	}

	private void OnQuarterDailyPartyTick(MobileParty mobileParty)
	{
		if (!mobileParty.IsMainParty)
		{
			TryHealOrWoundParty(mobileParty, isCheckingForPlayerRelatedParty: false);
		}
	}

	private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
	{
		if (_overflowedHealingForRegulars.ContainsKey(mobileParty.Party))
		{
			_overflowedHealingForRegulars.Remove(mobileParty.Party);
			if (_overflowedHealingForHeroes.ContainsKey(mobileParty.Party))
			{
				_overflowedHealingForHeroes.Remove(mobileParty.Party);
			}
		}
	}

	private void OnWeeklyTick()
	{
		List<PartyBase> list = new List<PartyBase>();
		foreach (KeyValuePair<PartyBase, float> overflowedHealingForRegular in _overflowedHealingForRegulars)
		{
			PartyBase key = overflowedHealingForRegular.Key;
			if (!key.IsActive && !key.IsValid)
			{
				list.Add(key);
			}
		}
		foreach (PartyBase item in list)
		{
			_overflowedHealingForRegulars.Remove(item);
			if (_overflowedHealingForHeroes.ContainsKey(item))
			{
				_overflowedHealingForHeroes.Remove(item);
			}
		}
	}

	public void OnMapEventEnded(MapEvent mapEvent)
	{
		if (!mapEvent.IsPlayerMapEvent)
		{
			OnBattleEndCheckPerkEffects(mapEvent);
		}
	}

	private void OnPlayerBattleEnd(MapEvent mapEvent)
	{
		OnBattleEndCheckPerkEffects(mapEvent);
	}

	private void OnBattleEndCheckPerkEffects(MapEvent mapEvent)
	{
		if (!mapEvent.HasWinner)
		{
			return;
		}
		foreach (PartyBase involvedParty in mapEvent.InvolvedParties)
		{
			if (involvedParty.MemberRoster.TotalHeroes <= 0)
			{
				continue;
			}
			foreach (TroopRosterElement item in involvedParty.MemberRoster.GetTroopRoster())
			{
				if (item.Character.IsHero)
				{
					Hero heroObject = item.Character.HeroObject;
					int battleEndHealingAmount = Campaign.Current.Models.PartyHealingModel.GetBattleEndHealingAmount(involvedParty.MobileParty, heroObject);
					if (battleEndHealingAmount > 0)
					{
						heroObject.Heal(battleEndHealingAmount);
					}
				}
			}
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_overflowedHealingForRegulars", ref _overflowedHealingForRegulars);
		dataStore.SyncData("_overflowedHealingForHeroes", ref _overflowedHealingForHeroes);
	}

	private void OnClanHourlyTick(Clan clan)
	{
		foreach (Hero hero in clan.Heroes)
		{
			if (hero.PartyBelongedTo == null && hero.PartyBelongedToAsPrisoner == null)
			{
				int a = MBRandom.RoundRandomized(0.5f);
				if (hero.HitPoints < hero.MaxHitPoints)
				{
					int num = MathF.Min(a, hero.MaxHitPoints - hero.HitPoints);
					hero.HitPoints += num;
				}
			}
		}
	}

	private void OnHourlyTick()
	{
		TryHealOrWoundParty(MobileParty.MainParty, isCheckingForPlayerRelatedParty: true);
	}

	private void TryHealOrWoundParty(MobileParty mobileParty, bool isCheckingForPlayerRelatedParty)
	{
		if (mobileParty.IsActive && mobileParty.MapEvent == null)
		{
			if (!_overflowedHealingForHeroes.TryGetValue(mobileParty.Party, out var value))
			{
				_overflowedHealingForHeroes.Add(mobileParty.Party, 0f);
			}
			if (!_overflowedHealingForRegulars.TryGetValue(mobileParty.Party, out var value2))
			{
				_overflowedHealingForRegulars.Add(mobileParty.Party, 0f);
			}
			float num = (isCheckingForPlayerRelatedParty ? (mobileParty.HealingRateForHeroes / 24f) : mobileParty.HealingRateForHeroes);
			float num2 = (isCheckingForPlayerRelatedParty ? (mobileParty.HealingRateForRegulars / 24f) : mobileParty.HealingRateForRegulars);
			value += num;
			value2 += num2;
			if (value >= 1f)
			{
				HealHeroes(mobileParty, ref value);
			}
			else if (value <= -1f)
			{
				ReduceHpHeroes(mobileParty, ref value);
			}
			if (value2 >= 1f)
			{
				HealRegulars(mobileParty, ref value2);
			}
			else if (value2 <= -1f)
			{
				ReduceHpRegulars(mobileParty, ref value2);
			}
			_overflowedHealingForHeroes[mobileParty.Party] = value;
			_overflowedHealingForRegulars[mobileParty.Party] = value2;
		}
	}

	private static void HealHeroes(MobileParty mobileParty, ref float heroesHealingValue)
	{
		int num = MathF.Floor(heroesHealingValue);
		heroesHealingValue -= num;
		TroopRoster memberRoster = mobileParty.MemberRoster;
		if (memberRoster.TotalHeroes > 0)
		{
			for (int i = 0; i < memberRoster.Count; i++)
			{
				Hero heroObject = memberRoster.GetCharacterAtIndex(i).HeroObject;
				if (heroObject != null && !heroObject.IsHealthFull())
				{
					heroObject.Heal(num, addXp: true);
				}
			}
		}
		TroopRoster prisonRoster = mobileParty.PrisonRoster;
		if (prisonRoster.TotalHeroes <= 0)
		{
			return;
		}
		for (int j = 0; j < prisonRoster.Count; j++)
		{
			Hero heroObject2 = prisonRoster.GetCharacterAtIndex(j).HeroObject;
			if (heroObject2 != null && !heroObject2.IsHealthFull())
			{
				heroObject2.Heal(1);
			}
		}
	}

	private static void ReduceHpHeroes(MobileParty mobileParty, ref float heroesHealingValue)
	{
		int a = MathF.Ceiling(heroesHealingValue);
		heroesHealingValue = 0f - (0f - heroesHealingValue) % 1f;
		for (int i = 0; i < mobileParty.MemberRoster.Count; i++)
		{
			Hero heroObject = mobileParty.MemberRoster.GetCharacterAtIndex(i).HeroObject;
			if (heroObject != null && heroObject.HitPoints > 0)
			{
				int num = MathF.Min(a, heroObject.HitPoints);
				heroObject.HitPoints += num;
			}
		}
	}

	private static void HealRegulars(MobileParty mobileParty, ref float regularsHealingValue)
	{
		TroopRoster memberRoster = mobileParty.MemberRoster;
		if (memberRoster.TotalWoundedRegulars == 0)
		{
			regularsHealingValue = 0f;
			return;
		}
		int num = MathF.Floor(regularsHealingValue);
		regularsHealingValue -= num;
		int num2 = 0;
		float num3 = 0f;
		int num4 = MBRandom.RandomInt(memberRoster.Count);
		for (int i = 0; i < memberRoster.Count; i++)
		{
			if (num <= 0)
			{
				break;
			}
			int index = (num4 + i) % memberRoster.Count;
			CharacterObject characterAtIndex = memberRoster.GetCharacterAtIndex(index);
			if (characterAtIndex.IsRegular)
			{
				int num5 = MathF.Min(num, memberRoster.GetElementWoundedNumber(index));
				if (num5 > 0)
				{
					memberRoster.AddToCountsAtIndex(index, 0, -num5);
					num -= num5;
					num2 += num5;
					num3 += (float)(characterAtIndex.Tier * num5);
				}
			}
		}
		if (num2 > 0)
		{
			SkillLevelingManager.OnRegularTroopHealedWhileWaiting(mobileParty, num2, num3 / (float)num2);
		}
	}

	private static void ReduceHpRegulars(MobileParty mobileParty, ref float regularsHealingValue)
	{
		TroopRoster memberRoster = mobileParty.MemberRoster;
		if (memberRoster.TotalRegulars - memberRoster.TotalWoundedRegulars == 0)
		{
			regularsHealingValue = 0f;
			return;
		}
		int num = MathF.Floor(0f - regularsHealingValue);
		regularsHealingValue = 0f - (0f - regularsHealingValue) % 1f;
		int num2 = MBRandom.RandomInt(memberRoster.Count);
		for (int i = 0; i < memberRoster.Count; i++)
		{
			if (num <= 0)
			{
				break;
			}
			int index = (num2 + i) % memberRoster.Count;
			if (memberRoster.GetCharacterAtIndex(index).IsRegular)
			{
				int num3 = MathF.Min(memberRoster.GetElementNumber(index) - memberRoster.GetElementWoundedNumber(index), num);
				if (num3 > 0)
				{
					memberRoster.AddToCountsAtIndex(index, 0, num3);
					num -= num3;
				}
			}
		}
	}
}
