using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class BanditsCampaignBehavior : CampaignBehaviorBase
{
	public class BanditsCampaignBehaviorTypeDefiner : SaveableTypeDefiner
	{
		public BanditsCampaignBehaviorTypeDefiner()
			: base(70000)
		{
		}

		protected override void DefineEnumTypes()
		{
			AddEnumDefinition(typeof(PlayerInteraction), 1);
		}

		protected override void DefineContainerDefinitions()
		{
			ConstructContainerDefinition(typeof(Dictionary<MobileParty, PlayerInteraction>));
		}
	}

	private enum PlayerInteraction
	{
		None,
		Friendly,
		PaidOffParty,
		Hostile
	}

	private const float BanditSpawnRadius = 45f;

	private const float BanditStartGoldPerBandit = 10f;

	private const float BanditLongTermGoldPerBandit = 50f;

	private const int HideoutInfestCooldownAfterFightAsHours = 36;

	private bool _hideoutsAndBanditsAreInitialized;

	private Dictionary<MobileParty, PlayerInteraction> _interactedBandits = new Dictionary<MobileParty, PlayerInteraction>();

	private static int _goldAmount;

	private int IdealBanditPartyCount => _numberOfMaxHideoutsAtEachBanditFaction * (_numberOfMaxBanditPartiesAroundEachHideout + _numberOfMaximumBanditPartiesInEachHideout) + _numberOfMaximumLooterParties;

	private int _numberOfMaximumLooterParties => Campaign.Current.Models.BanditDensityModel.NumberOfMaximumLooterParties;

	private float _radiusAroundPlayerPartySquared => MobileParty.MainParty.SeeingRange * MobileParty.MainParty.SeeingRange * 1.25f;

	private float _numberOfMinimumBanditPartiesInAHideoutToInfestIt => Campaign.Current.Models.BanditDensityModel.NumberOfMinimumBanditPartiesInAHideoutToInfestIt;

	private int _numberOfMaxBanditPartiesAroundEachHideout => Campaign.Current.Models.BanditDensityModel.NumberOfMaximumBanditPartiesAroundEachHideout;

	private int _numberOfMaxHideoutsAtEachBanditFaction => Campaign.Current.Models.BanditDensityModel.NumberOfMaximumHideoutsAtEachBanditFaction;

	private int _numberOfInitialHideoutsAtEachBanditFaction => Campaign.Current.Models.BanditDensityModel.NumberOfInitialHideoutsAtEachBanditFaction;

	private int _numberOfMaximumBanditPartiesInEachHideout => Campaign.Current.Models.BanditDensityModel.NumberOfMaximumBanditPartiesInEachHideout;

	public override void RegisterEvents()
	{
		CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
		CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, WeeklyTick);
		CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HourlyTick);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
		CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
		CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener(this, OnNewGameCreatedPartialFollowUp);
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, OnPartyDestroyed);
	}

	private void OnNewGameCreated(CampaignGameStarter starter)
	{
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_interactedBandits", ref _interactedBandits);
		dataStore.SyncData("_hideoutsAndBanditsAreInited", ref _hideoutsAndBanditsAreInitialized);
	}

	private void OnNewGameCreatedPartialFollowUp(CampaignGameStarter starter, int i)
	{
		if (i == 0)
		{
			MakeBanditFactionsEnemyToKingdomFactions();
			_hideoutsAndBanditsAreInitialized = false;
		}
		if (i < 10)
		{
			if (_numberOfMaxHideoutsAtEachBanditFaction > 0)
			{
				int num = Clan.BanditFactions.Count((Clan x) => !IsLooterFaction(x));
				int num2 = num / 10 + ((num % 10 > i) ? 1 : 0);
				int num3 = num / 10 * i;
				for (int j = 0; j < i; j++)
				{
					num3 += ((num % 10 > j) ? 1 : 0);
				}
				for (int k = 0; k < num2; k++)
				{
					SpawnHideoutsAndBanditsPartiallyOnNewGame(Clan.BanditFactions.ElementAt(num3 + k));
				}
			}
		}
		else
		{
			int num4 = i - 10;
			int idealBanditPartyCount = IdealBanditPartyCount;
			int num5 = idealBanditPartyCount / 90 + ((idealBanditPartyCount % 90 > num4) ? 1 : 0);
			int num6 = idealBanditPartyCount / 90 * num4;
			for (int l = 0; l < num4; l++)
			{
				num6 += ((idealBanditPartyCount % 90 > l) ? 1 : 0);
			}
			for (int m = 0; m < num5; m++)
			{
				SpawnBanditOrLooterPartiesAroundAHideoutOrSettlement(num6 + m);
			}
		}
	}

	private void SpawnHideoutsAndBanditsPartiallyOnNewGame(Clan banditClan)
	{
		if (!IsLooterFaction(banditClan))
		{
			for (int i = 0; i < _numberOfInitialHideoutsAtEachBanditFaction; i++)
			{
				FillANewHideoutWithBandits(banditClan);
			}
		}
	}

	private void MakeBanditFactionsEnemyToKingdomFactions()
	{
		foreach (Clan banditFaction in Clan.BanditFactions)
		{
			if (!banditFaction.IsBanditFaction || banditFaction.IsMinorFaction)
			{
				continue;
			}
			foreach (Kingdom item in Kingdom.All)
			{
				FactionManager.DeclareWar(item, banditFaction);
			}
			FactionManager.DeclareWar(Hero.MainHero.Clan, banditFaction);
		}
	}

	private void OnPartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
	{
		if (_interactedBandits.ContainsKey(mobileParty))
		{
			_interactedBandits.Remove(mobileParty);
		}
	}

	private void SetPlayerInteraction(MobileParty mobileParty, PlayerInteraction interaction)
	{
		if (_interactedBandits.ContainsKey(mobileParty))
		{
			_interactedBandits[mobileParty] = interaction;
		}
		else
		{
			_interactedBandits.Add(mobileParty, interaction);
		}
	}

	private PlayerInteraction GetPlayerInteraction(MobileParty mobileParty)
	{
		if (_interactedBandits.TryGetValue(mobileParty, out var value))
		{
			return value;
		}
		return PlayerInteraction.None;
	}

	public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
	{
		CheckForSpawningBanditBoss(settlement, mobileParty);
		if (!Campaign.Current.GameStarted || mobileParty == null || !mobileParty.IsBandit || !settlement.IsHideout)
		{
			return;
		}
		if (!settlement.Hideout.IsSpotted && settlement.Hideout.IsInfested)
		{
			float lengthSquared = (MobileParty.MainParty.Position2D - settlement.Position2D).LengthSquared;
			float seeingRange = MobileParty.MainParty.SeeingRange;
			float num = seeingRange * seeingRange / lengthSquared;
			float partySpottingDifficulty = Campaign.Current.Models.MapVisibilityModel.GetPartySpottingDifficulty(MobileParty.MainParty, mobileParty);
			if (num / partySpottingDifficulty >= 1f)
			{
				settlement.Hideout.IsSpotted = true;
				settlement.Party.UpdateVisibilityAndInspected();
				CampaignEventDispatcher.Instance.OnHideoutSpotted(MobileParty.MainParty.Party, settlement.Party);
			}
		}
		int num2 = 0;
		foreach (ItemRosterElement item in mobileParty.ItemRoster)
		{
			int num3 = (item.EquipmentElement.Item.IsFood ? MBRandom.RoundRandomized((float)mobileParty.MemberRoster.TotalManCount * ((3f + 6f * MBRandom.RandomFloat) / (float)item.EquipmentElement.Item.Value)) : 0);
			if (item.Amount > num3)
			{
				int num4 = item.Amount - num3;
				num2 += num4 * item.EquipmentElement.Item.Value;
			}
		}
		if (num2 > 0)
		{
			if (mobileParty.IsPartyTradeActive)
			{
				mobileParty.PartyTradeGold += (int)(0.25f * (float)num2);
			}
			settlement.SettlementComponent.ChangeGold((int)(0.25f * (float)num2));
		}
	}

	private void CheckForSpawningBanditBoss(Settlement settlement, MobileParty mobileParty)
	{
		if (settlement.IsHideout && settlement.Hideout.IsSpotted && settlement.Parties.Any((MobileParty x) => x.IsBandit || x.IsBanditBossParty))
		{
			CultureObject culture = settlement.Culture;
			MobileParty mobileParty2 = settlement.Parties.FirstOrDefault((MobileParty x) => x.IsBanditBossParty);
			if (mobileParty2 == null)
			{
				AddBossParty(settlement, culture);
			}
			else if (!mobileParty2.MemberRoster.Contains(culture.BanditBoss))
			{
				mobileParty2.MemberRoster.AddToCounts(culture.BanditBoss, 1);
			}
		}
	}

	private void AddBossParty(Settlement settlement, CultureObject culture)
	{
		PartyTemplateObject banditBossPartyTemplate = culture.BanditBossPartyTemplate;
		if (banditBossPartyTemplate != null)
		{
			AddBanditToHideout(settlement.Hideout, banditBossPartyTemplate, isBanditBossParty: true).Ai.DisableAi();
		}
	}

	public void DailyTick()
	{
		foreach (MobileParty allBanditParty in MobileParty.AllBanditParties)
		{
			if (!allBanditParty.IsPartyTradeActive)
			{
				continue;
			}
			allBanditParty.PartyTradeGold = (int)((double)allBanditParty.PartyTradeGold * 0.95 + (double)(50f * (float)allBanditParty.Party.MemberRoster.TotalManCount * 0.05f));
			if (!(MBRandom.RandomFloat < 0.03f) || allBanditParty.MapEvent == null)
			{
				continue;
			}
			foreach (ItemObject item in Items.All)
			{
				if (item.IsFood)
				{
					int num = (IsLooterFaction(allBanditParty.MapFaction) ? 8 : 16);
					int num2 = MBRandom.RoundRandomized((float)allBanditParty.MemberRoster.TotalManCount * (1f / (float)item.Value) * (float)num * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat);
					if (num2 > 0)
					{
						allBanditParty.ItemRoster.AddToCounts(item, num2);
					}
				}
			}
		}
	}

	private void TryToSpawnHideoutAndBanditHourly()
	{
		_hideoutsAndBanditsAreInitialized = true;
		int num = 0;
		foreach (Clan banditFaction in Clan.BanditFactions)
		{
			if (!IsLooterFaction(banditFaction))
			{
				for (int i = 0; i < _numberOfInitialHideoutsAtEachBanditFaction; i++)
				{
					FillANewHideoutWithBandits(banditFaction);
					num++;
				}
			}
		}
		int num2 = (int)(0.5f * (float)(_numberOfMaxBanditPartiesAroundEachHideout * num + _numberOfMaximumLooterParties));
		if (num2 > 0)
		{
			SpawnBanditOrLooterPartiesAroundAHideoutOrSettlement(num2);
		}
	}

	public void HourlyTick()
	{
		if (!_hideoutsAndBanditsAreInitialized && _numberOfMaxHideoutsAtEachBanditFaction > 0)
		{
			TryToSpawnHideoutAndBanditHourly();
		}
		if (!Campaign.Current.IsNight)
		{
			return;
		}
		int num = 0;
		foreach (Clan banditFaction in Clan.BanditFactions)
		{
			num += banditFaction.WarPartyComponents.Count;
		}
		int num2 = MBRandom.RoundRandomized((float)(IdealBanditPartyCount - num) * 0.01f);
		if (num2 > 0)
		{
			SpawnBanditOrLooterPartiesAroundAHideoutOrSettlement(num2);
		}
	}

	public void WeeklyTick()
	{
		AddNewHideouts();
	}

	public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddDialogs(campaignGameStarter);
	}

	private void AddNewHideouts()
	{
		foreach (Clan banditFaction in Clan.BanditFactions)
		{
			if (IsLooterFaction(banditFaction))
			{
				continue;
			}
			int num = 0;
			foreach (Hideout allHideout in Campaign.Current.AllHideouts)
			{
				if (allHideout.IsInfested && allHideout.Settlement.Culture == banditFaction.Culture)
				{
					num++;
				}
			}
			float num2 = 0f;
			if ((float)num < (float)_numberOfMaxHideoutsAtEachBanditFaction * 0.5f)
			{
				num2 = 1f - (float)num / (float)TaleWorlds.Library.MathF.Ceiling((float)_numberOfMaxHideoutsAtEachBanditFaction * 0.5f);
				num2 = TaleWorlds.Library.MathF.Max(0f, num2 * num2);
			}
			if (MBRandom.RandomFloat < num2)
			{
				FillANewHideoutWithBandits(banditFaction);
			}
		}
	}

	private void FillANewHideoutWithBandits(Clan faction)
	{
		Hideout hideout = SelectARandomHideout(faction, isInfestedHideoutNeeded: false, sameFactionIsNeeded: true, selectingFurtherToOthersNeeded: true);
		if (hideout != null)
		{
			for (int i = 0; (float)i < _numberOfMinimumBanditPartiesInAHideoutToInfestIt; i++)
			{
				AddBanditToHideout(hideout);
			}
		}
	}

	public MobileParty AddBanditToHideout(Hideout hideoutComponent, PartyTemplateObject overridenPartyTemplate = null, bool isBanditBossParty = false)
	{
		if (hideoutComponent.Owner.Settlement.Culture.IsBandit)
		{
			Clan clan = null;
			foreach (Clan banditFaction in Clan.BanditFactions)
			{
				if (hideoutComponent.Owner.Settlement.Culture == banditFaction.Culture)
				{
					clan = banditFaction;
				}
			}
			PartyTemplateObject pt = overridenPartyTemplate ?? clan.DefaultPartyTemplate;
			MobileParty mobileParty = BanditPartyComponent.CreateBanditParty(clan.StringId + "_1", clan, hideoutComponent, isBanditBossParty);
			mobileParty.InitializeMobilePartyAtPosition(pt, hideoutComponent.Owner.Settlement.Position2D);
			InitBanditParty(mobileParty, clan, hideoutComponent.Owner.Settlement);
			mobileParty.Ai.SetMoveGoToSettlement(hideoutComponent.Owner.Settlement);
			mobileParty.Ai.RecalculateShortTermAi();
			EnterSettlementAction.ApplyForParty(mobileParty, hideoutComponent.Owner.Settlement);
			return mobileParty;
		}
		return null;
	}

	private Hideout SelectARandomHideout(Clan faction, bool isInfestedHideoutNeeded, bool sameFactionIsNeeded, bool selectingFurtherToOthersNeeded = false)
	{
		float num = Campaign.AverageDistanceBetweenTwoFortifications * 0.33f * Campaign.AverageDistanceBetweenTwoFortifications * 0.33f;
		List<(Hideout, float)> list = new List<(Hideout, float)>();
		foreach (Hideout item in Hideout.All)
		{
			if ((sameFactionIsNeeded && item.Settlement.Culture != faction.Culture) || (isInfestedHideoutNeeded && !item.IsInfested))
			{
				continue;
			}
			int num2 = 1;
			if (item.Settlement.LastThreatTime.ElapsedHoursUntilNow > 36f && selectingFurtherToOthersNeeded)
			{
				float num3 = Campaign.MapDiagonalSquared;
				float num4 = Campaign.MapDiagonalSquared;
				foreach (Hideout item2 in Hideout.All)
				{
					if (item != item2 && item2.IsInfested)
					{
						float num5 = item.Settlement.Position2D.DistanceSquared(item2.Settlement.Position2D);
						if (item.Settlement.Culture == item2.Settlement.Culture && num5 < num3)
						{
							num3 = num5;
						}
						if (num5 < num4)
						{
							num4 = num5;
						}
					}
				}
				num2 = (int)TaleWorlds.Library.MathF.Max(1f, num3 / num + 5f * (num4 / num));
			}
			list.Add((item, num2));
		}
		return MBRandom.ChooseWeighted(list);
	}

	private void SpawnBanditOrLooterPartiesAroundAHideoutOrSettlement(int numberOfBanditsWillBeSpawned)
	{
		List<Clan> list = Clan.BanditFactions.ToList();
		Dictionary<Clan, int> dictionary = new Dictionary<Clan, int>(list.Count);
		foreach (Clan item in list)
		{
			dictionary.Add(item, 0);
		}
		foreach (Hideout allHideout in Campaign.Current.AllHideouts)
		{
			if (allHideout.IsInfested && allHideout.MapFaction is Clan)
			{
				dictionary[allHideout.MapFaction as Clan]++;
			}
		}
		int num = _numberOfMaxBanditPartiesAroundEachHideout + _numberOfMaximumBanditPartiesInEachHideout + 1;
		int num2 = _numberOfMaxHideoutsAtEachBanditFaction * num;
		int num3 = 0;
		foreach (Clan item2 in list)
		{
			num3 += item2.WarPartyComponents.Count;
		}
		numberOfBanditsWillBeSpawned = TaleWorlds.Library.MathF.Max(0, TaleWorlds.Library.MathF.Min(numberOfBanditsWillBeSpawned, list.Count((Clan f) => !IsLooterFaction(f)) * num2 + _numberOfMaximumLooterParties - num3));
		numberOfBanditsWillBeSpawned = TaleWorlds.Library.MathF.Ceiling((float)numberOfBanditsWillBeSpawned * 0.667f) + MBRandom.RandomInt(numberOfBanditsWillBeSpawned / 3);
		for (int i = 0; i < numberOfBanditsWillBeSpawned; i++)
		{
			Clan clan = null;
			float num4 = 1f;
			for (int j = 0; j < list.Count; j++)
			{
				float num5 = 1f;
				if (IsLooterFaction(list[j]))
				{
					num5 = (float)list[j].WarPartyComponents.Count / (float)_numberOfMaximumLooterParties;
				}
				else
				{
					int num6 = dictionary[list[j]];
					if (num6 > 0)
					{
						num5 = (float)list[j].WarPartyComponents.Count / (float)(num6 * num);
					}
				}
				if (num5 < 1f && (clan == null || num5 < num4))
				{
					clan = list[j];
					num4 = num5;
				}
			}
			if (clan == null)
			{
				break;
			}
			SpawnAPartyInFaction(clan);
		}
	}

	private void SpawnAPartyInFaction(Clan selectedFaction)
	{
		PartyTemplateObject defaultPartyTemplate = selectedFaction.DefaultPartyTemplate;
		Settlement settlement = null;
		if (IsLooterFaction(selectedFaction))
		{
			settlement = SelectARandomSettlementForLooterParty();
		}
		else
		{
			settlement = SelectARandomHideout(selectedFaction, isInfestedHideoutNeeded: true, sameFactionIsNeeded: true)?.Owner.Settlement;
			if (settlement == null)
			{
				settlement = SelectARandomHideout(selectedFaction, isInfestedHideoutNeeded: false, sameFactionIsNeeded: true)?.Owner.Settlement;
				if (settlement == null)
				{
					settlement = SelectARandomHideout(selectedFaction, isInfestedHideoutNeeded: false, sameFactionIsNeeded: false)?.Owner.Settlement;
				}
			}
		}
		MobileParty mobileParty = (settlement.IsHideout ? BanditPartyComponent.CreateBanditParty(selectedFaction.StringId + "_1", selectedFaction, settlement.Hideout, isBossParty: false) : BanditPartyComponent.CreateLooterParty(selectedFaction.StringId + "_1", selectedFaction, settlement, isBossParty: false));
		if (settlement == null)
		{
			return;
		}
		float num = 45f * (IsLooterFaction(selectedFaction) ? 1.5f : 1f);
		mobileParty.InitializeMobilePartyAroundPosition(defaultPartyTemplate, settlement.GatePosition, num);
		Vec2 vec = mobileParty.Position2D;
		float radiusAroundPlayerPartySquared = _radiusAroundPlayerPartySquared;
		for (int i = 0; i < 15; i++)
		{
			Vec2 vec2 = MobilePartyHelper.FindReachablePointAroundPosition(vec, num);
			if (vec2.DistanceSquared(MobileParty.MainParty.Position2D) > radiusAroundPlayerPartySquared)
			{
				vec = vec2;
				break;
			}
		}
		if (vec != mobileParty.Position2D)
		{
			mobileParty.Position2D = vec;
		}
		InitBanditParty(mobileParty, selectedFaction, settlement);
		mobileParty.Aggressiveness = 1f - 0.2f * MBRandom.RandomFloat;
		mobileParty.Ai.SetMovePatrolAroundPoint(settlement.IsTown ? settlement.GatePosition : settlement.Position2D);
	}

	private static bool IsLooterFaction(IFaction faction)
	{
		return !faction.Culture.CanHaveSettlement;
	}

	private Settlement SelectARandomSettlementForLooterParty()
	{
		int num = 0;
		foreach (Settlement item in Settlement.All)
		{
			if (item.IsTown || item.IsVillage)
			{
				int num2 = CalculateDistanceScore(item.Position2D.DistanceSquared(MobileParty.MainParty.Position2D));
				num += num2;
			}
		}
		int num3 = MBRandom.RandomInt(num);
		foreach (Settlement item2 in Settlement.All)
		{
			if (item2.IsTown || item2.IsVillage)
			{
				int num4 = CalculateDistanceScore(item2.Position2D.DistanceSquared(MobileParty.MainParty.Position2D));
				num3 -= num4;
				if (num3 <= 0)
				{
					return item2;
				}
			}
		}
		return null;
	}

	private void InitBanditParty(MobileParty banditParty, Clan faction, Settlement homeSettlement)
	{
		banditParty.Party.SetVisualAsDirty();
		banditParty.ActualClan = faction;
		CreatePartyTrade(banditParty);
		foreach (ItemObject item in Items.All)
		{
			if (item.IsFood)
			{
				int num = (IsLooterFaction(banditParty.MapFaction) ? 8 : 16);
				int num2 = MBRandom.RoundRandomized((float)banditParty.MemberRoster.TotalManCount * (1f / (float)item.Value) * (float)num * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat);
				if (num2 > 0)
				{
					banditParty.ItemRoster.AddToCounts(item, num2);
				}
			}
		}
	}

	private static void CreatePartyTrade(MobileParty banditParty)
	{
		int initialGold = (int)(10f * (float)banditParty.Party.MemberRoster.TotalManCount * (0.5f + 1f * MBRandom.RandomFloat));
		banditParty.InitializePartyTrade(initialGold);
	}

	private static int CalculateDistanceScore(float distance)
	{
		int result = 2;
		if (distance < 10000f)
		{
			result = 8;
		}
		else if (distance < 40000f)
		{
			result = 6;
		}
		else if (distance < 160000f)
		{
			result = 4;
		}
		else if (distance < 420000f)
		{
			result = 3;
		}
		return result;
	}

	protected void AddDialogs(CampaignGameStarter campaignGameSystemStarter)
	{
		campaignGameSystemStarter.AddDialogLine("bandit_start_defender", "start", "bandit_defender", "{=!}{ROBBERY_THREAT}", bandit_start_defender_condition, null);
		campaignGameSystemStarter.AddPlayerLine("bandit_start_defender_1", "bandit_defender", "bandit_start_fight", "{=DEnFOGhS}Fight me if you dare!", null, null);
		campaignGameSystemStarter.AddPlayerLine("bandit_start_defender_2", "bandit_defender", "barter_with_bandit_prebarter", "{=aQYMefHU}Maybe we can work out something.", bandit_start_barter_condition, null);
		campaignGameSystemStarter.AddDialogLine("bandit_start_fight", "bandit_start_fight", "close_window", "{=!}{ROBBERY_START_FIGHT}[ib:aggressive]", null, conversation_bandit_set_hostile_on_consequence);
		campaignGameSystemStarter.AddDialogLine("barter_with_bandit_prebarter", "barter_with_bandit_prebarter", "barter_with_bandit_screen", "{=!}{ROBBERY_PAY_AGREEMENT}", null, null);
		campaignGameSystemStarter.AddDialogLine("barter_with_bandit_screen", "barter_with_bandit_screen", "barter_with_bandit_postbarter", "{=!}Barter screen goes here", null, bandit_start_barter_consequence);
		campaignGameSystemStarter.AddDialogLine("barter_with_bandit_postbarter_1", "barter_with_bandit_postbarter", "close_window", "{=!}{ROBBERY_CONCLUSION}", bandit_barter_successful_condition, bandit_barter_successful_on_consequence);
		campaignGameSystemStarter.AddDialogLine("barter_with_bandit_postbarter_2", "barter_with_bandit_postbarter", "close_window", "{=!}{ROBBERY_START_FIGHT}", () => !bandit_barter_successful_condition(), conversation_bandit_set_hostile_on_consequence);
		campaignGameSystemStarter.AddDialogLine("bandit_start_attacker", "start", "bandit_attacker", "{=!}{BANDIT_NEUTRAL_GREETING}", bandit_neutral_greet_on_condition, bandit_neutral_greet_on_consequence);
		campaignGameSystemStarter.AddPlayerLine("common_encounter_ultimatum", "bandit_attacker", "common_encounter_ultimatum_answer", "{=!}{BANDIT_ULTIMATUM}", null, null);
		campaignGameSystemStarter.AddPlayerLine("common_encounter_fight", "bandit_attacker", "bandit_attacker_leave", "{=3W3eEIIZ}Never mind. You can go.", null, null);
		campaignGameSystemStarter.AddDialogLine("common_encounter_ultimatum_we_can_join_you", "common_encounter_ultimatum_answer", "bandits_we_can_join_you", "{=B5UMlqHc}I'll be honest... We don't want to die. Would you take us on as hired fighters? That way everyone gets what they want.", conversation_bandits_will_join_player_on_condition, null);
		campaignGameSystemStarter.AddDialogLine("common_encounter_ultimatum_war", "common_encounter_ultimatum_answer", "close_window", "{=n99VA8KP}You'll never take us alive![if:convo_angry][ib:aggressive]", null, conversation_bandit_set_hostile_on_consequence);
		campaignGameSystemStarter.AddPlayerLine("common_bandit_join_player_accepted", "bandits_we_can_join_you", "close_window", "{=XdKCuzg1}Very well. You may join us. But I'll be keeping an eye on you lot.", null, delegate
		{
			MobileParty party2 = MobileParty.ConversationParty;
			Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
			{
				conversation_bandits_join_player_party_on_consequence(party2);
			};
		});
		campaignGameSystemStarter.AddPlayerLine("common_bandit_join_player_declined_1", "bandits_we_can_join_you", "player_do_not_let_bandits_to_join", "{=JZvywHNy}You think I'm daft? I'm not trusting you an inch.", null, null);
		campaignGameSystemStarter.AddPlayerLine("common_bandit_join_player_declined_2", "bandits_we_can_join_you", "player_do_not_let_bandits_to_join", "{=z0WacPaW}No, justice demands you pay for your crimes.", null, conversation_bandit_set_hostile_on_consequence);
		campaignGameSystemStarter.AddPlayerLine("common_bandit_join_player_leave", "bandits_we_can_join_you", "bandit_attacker_leave", "{=D33fIGQe}Never mind.", null, null);
		campaignGameSystemStarter.AddDialogLine("common_encounter_declined_looters_to_join_war_surrender", "player_do_not_let_bandits_to_join", "close_window", "{=ji2eenPE}All right - we give up. We can't fight you. Maybe the likes of us don't deserve mercy, but... show what mercy you can.", conversation_bandits_surrender_on_condition, delegate
		{
			MobileParty party = MobileParty.ConversationParty;
			Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
			{
				conversation_bandits_surrender_on_consequence(party);
			};
		});
		campaignGameSystemStarter.AddDialogLine("common_encounter_ultimatum_war_2", "player_do_not_let_bandits_to_join", "close_window", "{=LDhU5urT}So that's how it is, is it? Right then - I'll make one of you bleed before I go down.[if:convo_angry][ib:aggressive]", null, null);
		campaignGameSystemStarter.AddDialogLine("bandit_attacker_try_leave_success", "bandit_attacker_leave", "close_window", "{=IDdyHef9}We'll be on our way, then!", bandit_attacker_try_leave_condition, delegate
		{
			PlayerEncounter.LeaveEncounter = true;
		});
		campaignGameSystemStarter.AddDialogLine("bandit_attacker_try_leave_fail", "bandit_attacker_leave", "bandit_defender", "{=6Wc1XErN}Wait, wait... You're not going anywhere just yet.", () => !bandit_attacker_try_leave_condition(), null);
		campaignGameSystemStarter.AddPlayerLine("common_encounter_cheat1", "bandit_attacker", "close_window", "{=4Wvdk30M}Cheat: Follow me", bandit_cheat_conversations_condition, delegate
		{
			PlayerEncounter.EncounteredMobileParty.Ai.SetMoveEscortParty(MobileParty.MainParty);
			PlayerEncounter.EncounteredMobileParty.Ai.SetInitiative(0f, 0f, 48f);
			PlayerEncounter.LeaveEncounter = true;
		});
		campaignGameSystemStarter.AddPlayerLine("common_encounter_cheat2", "bandit_attacker", "close_window", "{=ORj5F5il}Cheat: Besiege Town", bandit_cheat_conversations_condition, delegate
		{
			PlayerEncounter.EncounteredMobileParty.Ai.SetMoveBesiegeSettlement(Settlement.FindFirst((Settlement s) => s.IsTown && (double)s.Position2D.Distance(PlayerEncounter.EncounteredMobileParty.Position2D) < 80.0));
			PlayerEncounter.EncounteredMobileParty.Ai.SetInitiative(0f, 0f, 48f);
			PlayerEncounter.LeaveEncounter = true;
		});
		campaignGameSystemStarter.AddPlayerLine("common_encounter_cheat3", "bandit_attacker", "close_window", "{=RxuM5RzJ}Cheat: Raid Nearby Village", bandit_cheat_conversations_condition, delegate
		{
			PlayerEncounter.EncounteredMobileParty.Ai.SetMoveRaidSettlement(Settlement.FindFirst((Settlement s) => s.IsVillage && (double)s.Position2D.Distance(PlayerEncounter.EncounteredMobileParty.Position2D) < 50.0));
			PlayerEncounter.LeaveEncounter = true;
		});
		campaignGameSystemStarter.AddPlayerLine("common_encounter_cheat4", "bandit_attacker", "close_window", "{=DIfTkzJJ}Cheat: Besiege Nearby Town", bandit_cheat_conversations_condition, delegate
		{
			PlayerEncounter.EncounteredMobileParty.Ai.SetMoveBesiegeSettlement(Settlement.FindFirst((Settlement s) => s.IsTown && (double)s.Position2D.Distance(PlayerEncounter.EncounteredMobileParty.Position2D) < 50.0));
			PlayerEncounter.LeaveEncounter = true;
			PlayerEncounter.EncounteredMobileParty.Ai.SetInitiative(0f, 0f, 72f);
		});
		campaignGameSystemStarter.AddPlayerLine("common_encounter_cheat5", "bandit_attacker", "close_window", "{=eXaiRXF9}Cheat: Besiege Nearby Castle", bandit_cheat_conversations_condition, delegate
		{
			PlayerEncounter.EncounteredMobileParty.Ai.SetMoveBesiegeSettlement(Settlement.FindFirst((Settlement s) => s.IsCastle && (double)s.Position2D.Distance(PlayerEncounter.EncounteredMobileParty.Position2D) < 50.0));
			PlayerEncounter.LeaveEncounter = true;
			PlayerEncounter.EncounteredMobileParty.Ai.SetInitiative(0f, 0f, 72f);
		});
		campaignGameSystemStarter.AddDialogLine("minor_faction_hostile", "start", "minor_faction_talk_hostile_response", "{=!}{MINOR_FACTION_ENCOUNTER}", conversation_minor_faction_hostile_on_condition, null);
		campaignGameSystemStarter.AddPlayerLine("minor_faction_talk_hostile_response_1", "minor_faction_talk_hostile_response", "close_window", "{=aaf5R99a}I'll give you nothing but cold steel, you scum!", null, null);
		campaignGameSystemStarter.AddPlayerLine("minor_faction_talk_hostile_response_2", "minor_faction_talk_hostile_response", "minor_faction_talk_background", "{=EVLzPv1t}Hold - tell me more about yourselves.", null, null);
		campaignGameSystemStarter.AddDialogLine("minor_faction_talk_background", "minor_faction_talk_background", "minor_faction_talk_background_next", "{=!}{MINOR_FACTION_SELFDESCRIPTION}", conversation_minor_faction_set_selfdescription, null);
		campaignGameSystemStarter.AddPlayerLine("minor_faction_talk_background_next_1", "minor_faction_talk_background_next", "minor_faction_talk_how_to_befriend", "{=vEsmC6M6}Is there any way we could not be enemies?", null, null);
		campaignGameSystemStarter.AddPlayerLine("minor_faction_talk_background_next_2", "minor_faction_talk_background_next", "close_window", "{=p2WPU1CU}Very good then. Now I know whom I slay.", null, null);
		campaignGameSystemStarter.AddDialogLine("minor_faction_talk_how_to_befriend", "minor_faction_talk_how_to_befriend", "minor_faction_talk_background_repeat_threat", "{=!}{MINOR_FACTION_HOWTOBEFRIEND}", conversation_minor_faction_set_how_to_befriend, null);
		campaignGameSystemStarter.AddDialogLine("minor_faction_talk_background_repeat_threat", "minor_faction_talk_background_repeat_threat", "minor_faction_talk_hostile_response", "{=ByOYHslS}That's enough talking for now. Make your choice.[if:convo_angry][[ib:aggressive]", null, null);
	}

	private bool bandit_barter_successful_condition()
	{
		return Campaign.Current.BarterManager.LastBarterIsAccepted;
	}

	private bool bandit_cheat_conversations_condition()
	{
		return Game.Current.IsDevelopmentMode;
	}

	private bool conversation_bandits_will_join_player_on_condition()
	{
		if (Hero.MainHero.GetPerkValue(DefaultPerks.Roguery.PartnersInCrime))
		{
			return true;
		}
		int num = (PartyBaseHelper.DoesSurrenderIsLogicalForParty(MobileParty.ConversationParty, MobileParty.MainParty, 0.06f) ? 33 : 67);
		if (Hero.MainHero.GetPerkValue(DefaultPerks.Roguery.Scarface))
		{
			num = TaleWorlds.Library.MathF.Round((float)num * (1f + DefaultPerks.Roguery.Scarface.PrimaryBonus));
		}
		if (MobileParty.ConversationParty.Party.RandomIntWithSeed(3u, 100) > 100 - num)
		{
			return false;
		}
		return PartyBaseHelper.DoesSurrenderIsLogicalForParty(MobileParty.ConversationParty, MobileParty.MainParty, 0.09f);
	}

	private bool conversation_bandits_surrender_on_condition()
	{
		int num = (PartyBaseHelper.DoesSurrenderIsLogicalForParty(MobileParty.ConversationParty, MobileParty.MainParty, 0.04f) ? 33 : 67);
		if (Hero.MainHero.GetPerkValue(DefaultPerks.Roguery.Scarface))
		{
			num = TaleWorlds.Library.MathF.Round((float)num * (1f + DefaultPerks.Roguery.Scarface.PrimaryBonus));
		}
		if (MobileParty.ConversationParty.Party.RandomIntWithSeed(4u, 100) > 100 - num)
		{
			return false;
		}
		return PartyBaseHelper.DoesSurrenderIsLogicalForParty(MobileParty.ConversationParty, MobileParty.MainParty, 0.06f);
	}

	private bool bandit_neutral_greet_on_condition()
	{
		if (Campaign.Current.CurrentConversationContext == ConversationContext.PartyEncounter && PlayerEncounter.Current != null && PlayerEncounter.EncounteredMobileParty != null && PlayerEncounter.EncounteredMobileParty.MapFaction.IsBanditFaction && PlayerEncounter.PlayerIsAttacker && MobileParty.ConversationParty != null)
		{
			MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=ZPj0ZAO7}Yeah? What do you want with us?");
			MBTextManager.SetTextVariable("BANDIT_ULTIMATUM", "{=5zUIQtTa}I want you to surrender or die, brigand!");
			int num = MBRandom.RandomInt(8);
			switch (GetPlayerInteraction(MobileParty.ConversationParty))
			{
			case PlayerInteraction.PaidOffParty:
				MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=Bm7U7TgG}If you're going to keep pestering us, traveller, we might need to take a bit more coin from you.");
				MBTextManager.SetTextVariable("BANDIT_ULTIMATUM", "{=KRfcro26}We're here to fight. Surrender or die!");
				break;
			default:
				if (PlayerEncounter.PlayerIsAttacker)
				{
					MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=38DvG2ba}Yeah? What is it now?");
				}
				else
				{
					MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=5laJ37D8}Back for more, are you?");
				}
				MBTextManager.SetTextVariable("BANDIT_ULTIMATUM", "{=KRfcro26}We're here to fight. Surrender or die!");
				break;
			case PlayerInteraction.None:
				switch (num)
				{
				case 1:
					MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=cO61R3va}We've got no quarrel with you.");
					MBTextManager.SetTextVariable("BANDIT_ULTIMATUM", "{=oJ6lpXmp}But I have one with you, brigand! Give up now.");
					break;
				case 2:
					MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=6XdHP9Pv}We're not looking for a fight.");
					MBTextManager.SetTextVariable("BANDIT_ULTIMATUM", "{=fiLWg11t}Neither am I, if you surrender. Otherwise...");
					break;
				case 3:
					MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=GUiT211X}You got a problem?");
					MBTextManager.SetTextVariable("BANDIT_ULTIMATUM", "{=idwOxnX5}Not if you give up now. If not, prepare to fight!");
					break;
				case 4:
					MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=mHBHKacJ}We're just harmless travellers...");
					MBTextManager.SetTextVariable("BANDIT_ULTIMATUM", "{=A5IJmN0X}I think not, brigand. Surrender or die!");
					if (PlayerEncounter.EncounteredMobileParty.MapFaction.StringId == "mountain_bandits")
					{
						MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=8rgH8CGc}We're just harmless shepherds...");
					}
					else if (PlayerEncounter.EncounteredMobileParty.MapFaction.StringId == "forest_bandits")
					{
						MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=kRASveAC}We're just harmless foresters...");
					}
					else if (PlayerEncounter.EncounteredMobileParty.MapFaction.StringId == "sea_raiders")
					{
						MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=k96R57KM}We're just harmless traders...");
					}
					else if (PlayerEncounter.EncounteredMobileParty.MapFaction.StringId == "steppe_bandits")
					{
						MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=odzS6rhH}We're just harmless herdsmen...");
					}
					else if (PlayerEncounter.EncounteredMobileParty.MapFaction.StringId == "desert_bandits")
					{
						MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=Vttb0P15}We're just harmless nomads...");
					}
					break;
				case 5:
					MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=wSwzyr6M}Mess with us and we'll sell our lives dearly.");
					MBTextManager.SetTextVariable("BANDIT_ULTIMATUM", "{=GLqb67cg}I don't care, brigand. Surrender or die!");
					break;
				case 6:
					MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=xQ0aBavD}Back off, stranger, unless you want trouble.");
					MBTextManager.SetTextVariable("BANDIT_ULTIMATUM", "{=BwIT8F0k}I don't mind, brigand. Surrender or die!");
					break;
				case 7:
					MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=8yPqbZmm}You best back off. There's dozens more of us hiding, just waiting for our signal.");
					MBTextManager.SetTextVariable("BANDIT_ULTIMATUM", "{=ASRpFaGF}Nice try, brigand. Surrender or die!");
					if (PlayerEncounter.EncounteredMobileParty.MapFaction.StringId == "mountain_bandits")
					{
						MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=TXzZwb7n}You best back off. Scores of our brothers are just over that ridge over there, waiting for our signal.");
					}
					else if (PlayerEncounter.EncounteredMobileParty.MapFaction.StringId == "forest_bandits")
					{
						MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=lZj61xTm}You don't know who you're messing with. There are scores of our brothers hiding in the woods, just waiting for our signal to pepper you with arrows.");
					}
					else if (PlayerEncounter.EncounteredMobileParty.MapFaction.StringId == "sea_raiders")
					{
						MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=7Sp6aNYo}You best let us be. There's dozens more of us hiding here, just waiting for our signal.");
					}
					else if (PlayerEncounter.EncounteredMobileParty.MapFaction.StringId == "steppe_bandits")
					{
						MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=EUbdov2r}Back off, stranger. There's dozens more of us hiding in that gully over there, just waiting for our signal.");
					}
					else if (PlayerEncounter.EncounteredMobileParty.MapFaction.StringId == "desert_bandits")
					{
						MBTextManager.SetTextVariable("BANDIT_NEUTRAL_GREETING", "{=RWxYalkR}Be warned, stranger. There's dozens more of us hiding in that wadi over there, just waiting for our signal.");
					}
					break;
				}
				break;
			}
			return true;
		}
		return false;
	}

	private void bandit_barter_successful_on_consequence()
	{
		SetPlayerInteraction(MobileParty.ConversationParty, PlayerInteraction.PaidOffParty);
	}

	private void bandit_neutral_greet_on_consequence()
	{
		if (GetPlayerInteraction(MobileParty.ConversationParty) != PlayerInteraction.PaidOffParty)
		{
			SetPlayerInteraction(MobileParty.ConversationParty, PlayerInteraction.Friendly);
		}
	}

	private void conversation_bandit_set_hostile_on_consequence()
	{
		SetPlayerInteraction(MobileParty.ConversationParty, PlayerInteraction.Hostile);
	}

	private void GetMemberAndPrisonerRostersFromParties(List<MobileParty> parties, ref TroopRoster troopsTakenAsMember, ref TroopRoster troopsTakenAsPrisoner, bool doBanditsJoinPlayerSide)
	{
		foreach (MobileParty party in parties)
		{
			for (int i = 0; i < party.MemberRoster.Count; i++)
			{
				if (!party.MemberRoster.GetCharacterAtIndex(i).IsHero)
				{
					if (doBanditsJoinPlayerSide)
					{
						troopsTakenAsMember.AddToCounts(party.MemberRoster.GetCharacterAtIndex(i), party.MemberRoster.GetElementNumber(i));
					}
					else
					{
						troopsTakenAsPrisoner.AddToCounts(party.MemberRoster.GetCharacterAtIndex(i), party.MemberRoster.GetElementNumber(i));
					}
				}
			}
			for (int num = party.PrisonRoster.Count - 1; num > -1; num--)
			{
				CharacterObject characterAtIndex = party.PrisonRoster.GetCharacterAtIndex(num);
				if (!characterAtIndex.IsHero)
				{
					troopsTakenAsMember.AddToCounts(party.PrisonRoster.GetCharacterAtIndex(num), party.PrisonRoster.GetElementNumber(num));
				}
				else if (characterAtIndex.HeroObject.Clan == Clan.PlayerClan)
				{
					if (doBanditsJoinPlayerSide)
					{
						EndCaptivityAction.ApplyByPeace(characterAtIndex.HeroObject);
					}
					else
					{
						EndCaptivityAction.ApplyByReleasedAfterBattle(characterAtIndex.HeroObject);
					}
					characterAtIndex.HeroObject.ChangeState(Hero.CharacterStates.Active);
					AddHeroToPartyAction.Apply(characterAtIndex.HeroObject, MobileParty.MainParty);
				}
				else if (Clan.PlayerClan.IsAtWarWith(characterAtIndex.HeroObject.Clan))
				{
					TransferPrisonerAction.Apply(characterAtIndex, party.Party, PartyBase.MainParty);
				}
			}
		}
	}

	private void OpenRosterScreenAfterBanditEncounter(MobileParty conversationParty, bool doBanditsJoinPlayerSide)
	{
		List<MobileParty> partiesToJoinPlayerSide = new List<MobileParty> { MobileParty.MainParty };
		List<MobileParty> partiesToJoinEnemySide = new List<MobileParty>();
		if (PlayerEncounter.EncounteredMobileParty != null)
		{
			partiesToJoinEnemySide.Add(PlayerEncounter.EncounteredMobileParty);
		}
		if (PlayerEncounter.Current != null)
		{
			PlayerEncounter.Current.FindAllNpcPartiesWhoWillJoinEvent(ref partiesToJoinPlayerSide, ref partiesToJoinEnemySide);
		}
		TroopRoster troopsTakenAsPrisoner = TroopRoster.CreateDummyTroopRoster();
		TroopRoster troopsTakenAsMember = TroopRoster.CreateDummyTroopRoster();
		GetMemberAndPrisonerRostersFromParties(partiesToJoinEnemySide, ref troopsTakenAsMember, ref troopsTakenAsPrisoner, doBanditsJoinPlayerSide);
		if (!doBanditsJoinPlayerSide)
		{
			Dictionary<PartyBase, ItemRoster> dictionary = new Dictionary<PartyBase, ItemRoster>();
			ItemRoster itemRoster = new ItemRoster();
			int num = 0;
			foreach (MobileParty item in partiesToJoinEnemySide)
			{
				num += item.PartyTradeGold;
				itemRoster.Add(item.ItemRoster);
			}
			GiveGoldAction.ApplyForPartyToCharacter(conversationParty.Party, Hero.MainHero, num);
			dictionary.Add(PartyBase.MainParty, itemRoster);
			if (itemRoster.Count > 0)
			{
				InventoryManager.OpenScreenAsLoot(dictionary);
				for (int i = 0; i < partiesToJoinEnemySide.Count - 1; i++)
				{
					partiesToJoinEnemySide[i].ItemRoster.Clear();
				}
			}
			PartyScreenManager.OpenScreenWithCondition(IsTroopTransferable, DoneButtonCondition, OnDoneClicked, null, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.Transferable, PlayerEncounter.EncounteredParty.Name, troopsTakenAsMember.TotalManCount, showProgressBar: false, isDonating: false, PartyScreenMode.Loot, troopsTakenAsMember, troopsTakenAsPrisoner);
			for (int num2 = partiesToJoinEnemySide.Count - 1; num2 >= 0; num2--)
			{
				MobileParty destroyedParty = partiesToJoinEnemySide[num2];
				DestroyPartyAction.Apply(MobileParty.MainParty.Party, destroyedParty);
			}
		}
		else
		{
			PartyScreenManager.OpenScreenWithCondition(IsTroopTransferable, DoneButtonCondition, OnDoneClicked, null, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.Transferable, PlayerEncounter.EncounteredParty.Name, troopsTakenAsMember.TotalManCount, showProgressBar: false, isDonating: false, PartyScreenMode.TroopsManage, troopsTakenAsMember);
			for (int num3 = partiesToJoinEnemySide.Count - 1; num3 >= 0; num3--)
			{
				MobileParty mobileParty = partiesToJoinEnemySide[num3];
				CampaignEventDispatcher.Instance.OnBanditPartyRecruited(mobileParty);
				DestroyPartyAction.Apply(MobileParty.MainParty.Party, mobileParty);
			}
		}
	}

	private void conversation_bandits_surrender_on_consequence(MobileParty conversationParty)
	{
		OpenRosterScreenAfterBanditEncounter(conversationParty, doBanditsJoinPlayerSide: false);
		PlayerEncounter.LeaveEncounter = true;
	}

	private void conversation_bandits_join_player_party_on_consequence(MobileParty conversationParty)
	{
		OpenRosterScreenAfterBanditEncounter(conversationParty, doBanditsJoinPlayerSide: true);
		PlayerEncounter.LeaveEncounter = true;
	}

	private bool OnDoneClicked(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty, PartyBase rightParty)
	{
		return true;
	}

	private Tuple<bool, TextObject> DoneButtonCondition(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, int leftLimitNum, int rightLimitNum)
	{
		foreach (TroopRosterElement item in rightMemberRoster.GetTroopRoster())
		{
			if (item.Character.IsHero && item.Character.HeroObject.HeroState == Hero.CharacterStates.Fugitive)
			{
				item.Character.HeroObject.ChangeState(Hero.CharacterStates.Active);
			}
		}
		return new Tuple<bool, TextObject>(item1: true, null);
	}

	private bool IsTroopTransferable(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty)
	{
		return true;
	}

	private bool bandit_start_defender_condition()
	{
		PartyBase encounteredParty = PlayerEncounter.EncounteredParty;
		if ((Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.MapFaction != null && !Hero.OneToOneConversationHero.MapFaction.IsBanditFaction) || encounteredParty == null || !encounteredParty.IsMobile || !encounteredParty.MapFaction.IsBanditFaction)
		{
			return false;
		}
		List<TextObject> list = new List<TextObject>();
		List<TextObject> list2 = new List<TextObject>();
		List<TextObject> list3 = new List<TextObject>();
		List<TextObject> list4 = new List<TextObject>();
		for (int i = 1; i <= 6; i++)
		{
			if (GameTexts.TryGetText("str_robbery_threat", out var textObject, i.ToString()))
			{
				list.Add(textObject);
				list2.Add(GameTexts.FindText("str_robbery_pay_agreement", i.ToString()));
				list3.Add(GameTexts.FindText("str_robbery_conclusion", i.ToString()));
				list4.Add(GameTexts.FindText("str_robbery_start_fight", i.ToString()));
			}
		}
		for (int j = 1; j <= 6; j++)
		{
			string variation = encounteredParty.MapFaction.StringId + "_" + j;
			if (GameTexts.TryGetText("str_robbery_threat", out var textObject2, variation))
			{
				for (int k = 0; k < 3; k++)
				{
					list.Add(textObject2);
					list2.Add(GameTexts.FindText("str_robbery_pay_agreement", variation));
					list3.Add(GameTexts.FindText("str_robbery_conclusion", variation));
					list4.Add(GameTexts.FindText("str_robbery_start_fight", variation));
				}
			}
		}
		int index = MBRandom.RandomInt(0, list.Count);
		MBTextManager.SetTextVariable("ROBBERY_THREAT", list[index]);
		MBTextManager.SetTextVariable("ROBBERY_PAY_AGREEMENT", list2[index]);
		MBTextManager.SetTextVariable("ROBBERY_CONCLUSION", list3[index]);
		MBTextManager.SetTextVariable("ROBBERY_START_FIGHT", list4[index]);
		List<MobileParty> partiesToJoinPlayerSide = new List<MobileParty> { MobileParty.MainParty };
		List<MobileParty> partiesToJoinEnemySide = new List<MobileParty>();
		if (MobileParty.ConversationParty != null)
		{
			partiesToJoinEnemySide.Add(MobileParty.ConversationParty);
		}
		if (PlayerEncounter.Current != null)
		{
			PlayerEncounter.Current.FindAllNpcPartiesWhoWillJoinEvent(ref partiesToJoinPlayerSide, ref partiesToJoinEnemySide);
		}
		float num = 0f;
		foreach (MobileParty item in partiesToJoinPlayerSide)
		{
			num += item.Party.TotalStrength;
		}
		float num2 = 0f;
		foreach (MobileParty item2 in partiesToJoinEnemySide)
		{
			num2 += item2.Party.TotalStrength;
		}
		float num3 = (num2 + 1f) / (num + 1f);
		int num4 = Hero.MainHero.Gold / 100;
		double num5 = 2.0 * (double)TaleWorlds.Library.MathF.Max(0f, TaleWorlds.Library.MathF.Min(6f, num3 - 1f));
		float num6 = 0f;
		Settlement settlement = SettlementHelper.FindNearestSettlement((Settlement x) => x.IsTown || x.IsVillage);
		SettlementComponent settlementComponent = ((!settlement.IsTown) ? ((SettlementComponent)settlement.Village) : ((SettlementComponent)settlement.Town));
		foreach (ItemRosterElement item3 in MobileParty.MainParty.ItemRoster)
		{
			num6 += (float)(settlementComponent.GetItemPrice(item3.EquipmentElement, MobileParty.MainParty, isSelling: true) * item3.Amount);
		}
		float num7 = num6 / 100f;
		float num8 = 1f + 2f * TaleWorlds.Library.MathF.Max(0f, TaleWorlds.Library.MathF.Min(6f, num3 - 1f));
		_goldAmount = (int)((double)num4 * num5 + (double)(num7 * num8) + 100.0);
		MBTextManager.SetTextVariable("AMOUNT", _goldAmount.ToString());
		if (encounteredParty.IsMobile && encounteredParty.MapFaction.IsBanditFaction)
		{
			return PlayerEncounter.PlayerIsDefender;
		}
		return false;
	}

	private bool bandit_start_barter_condition()
	{
		if (PlayerEncounter.Current != null)
		{
			return PlayerEncounter.Current.PlayerSide == BattleSideEnum.Defender;
		}
		return false;
	}

	private void bandit_start_barter_consequence()
	{
		BarterManager.Instance.StartBarterOffer(Hero.MainHero, Hero.OneToOneConversationHero, PartyBase.MainParty, MobileParty.ConversationParty?.Party, null, BarterManager.Instance.InitializeSafePassageBarterContext, 0, isAIBarter: false, new Barterable[1]
		{
			new SafePassageBarterable(null, Hero.MainHero, MobileParty.ConversationParty?.Party, PartyBase.MainParty)
		});
	}

	private bool conversation_minor_faction_hostile_on_condition()
	{
		if (MapEvent.PlayerMapEvent != null)
		{
			foreach (PartyBase involvedParty in MapEvent.PlayerMapEvent.InvolvedParties)
			{
				if (PartyBase.MainParty.Side == BattleSideEnum.Attacker && involvedParty.IsMobile && involvedParty.MobileParty.IsBandit && involvedParty.MapFaction.IsMinorFaction)
				{
					string text = involvedParty.MapFaction.StringId + "_encounter";
					text = ((!FactionManager.IsAtWarAgainstFaction(involvedParty.MapFaction, Hero.MainHero.MapFaction)) ? (text + "_neutral") : (text + "_hostile"));
					MBTextManager.SetTextVariable("MINOR_FACTION_ENCOUNTER", GameTexts.FindText(text));
					return true;
				}
			}
		}
		return false;
	}

	private bool conversation_minor_faction_set_selfdescription()
	{
		foreach (PartyBase involvedParty in MapEvent.PlayerMapEvent.InvolvedParties)
		{
			if (PartyBase.MainParty.Side == BattleSideEnum.Attacker && involvedParty.IsMobile && involvedParty.MobileParty.IsBandit && involvedParty.MapFaction.IsMinorFaction)
			{
				string id = involvedParty.MapFaction.StringId + "_selfdescription";
				MBTextManager.SetTextVariable("MINOR_FACTION_SELFDESCRIPTION", GameTexts.FindText(id));
				return true;
			}
		}
		return true;
	}

	private bool conversation_minor_faction_set_how_to_befriend()
	{
		foreach (PartyBase involvedParty in MapEvent.PlayerMapEvent.InvolvedParties)
		{
			if (PartyBase.MainParty.Side == BattleSideEnum.Attacker && involvedParty.IsMobile && involvedParty.MobileParty.IsBandit && involvedParty.MapFaction.IsMinorFaction)
			{
				string id = involvedParty.MapFaction.StringId + "_how_to_befriend";
				MBTextManager.SetTextVariable("MINOR_FACTION_HOWTOBEFRIEND", GameTexts.FindText(id));
				return true;
			}
		}
		return true;
	}

	private bool bandit_attacker_try_leave_condition()
	{
		if (PlayerEncounter.EncounteredParty != null)
		{
			if (!(PlayerEncounter.EncounteredParty.TotalStrength <= PartyBase.MainParty.TotalStrength) && GetPlayerInteraction(PlayerEncounter.EncounteredMobileParty) != PlayerInteraction.PaidOffParty)
			{
				return GetPlayerInteraction(PlayerEncounter.EncounteredMobileParty) == PlayerInteraction.Friendly;
			}
			return true;
		}
		return false;
	}
}
