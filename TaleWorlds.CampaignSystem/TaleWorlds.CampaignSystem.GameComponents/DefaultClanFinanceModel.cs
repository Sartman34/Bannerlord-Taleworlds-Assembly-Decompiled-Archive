using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultClanFinanceModel : ClanFinanceModel
{
	private enum TransactionType
	{
		Income = 1,
		Both = 0,
		Expense = -1
	}

	public enum AssetIncomeType
	{
		Workshop,
		Caravan,
		Taxes,
		TributesEarned,
		TributesPaid
	}

	private static readonly TextObject _townTaxStr = new TextObject("{=TLuaPAIO}{A0} Taxes");

	private static readonly TextObject _townTradeTaxStr = new TextObject("{=dfwCjiRx}Trade Tax from {A0}");

	private static readonly TextObject _partyIncomeStr = new TextObject("{=uuyso3mg}Income from Parties");

	private static readonly TextObject _financialHelpStr = new TextObject("{=E3BsEDav}Financial Help for Parties");

	private static readonly TextObject _scutageTaxStr = new TextObject("{=RuHaC2Ck}Scutage Tax");

	private static readonly TextObject _caravanIncomeStr = new TextObject("{=qyahMgD3}Caravan ({A0})");

	private static readonly TextObject _projectsIncomeStr = new TextObject("{=uixuohBp}Settlement Projects");

	private static readonly TextObject _partyExpensesStr = new TextObject("{=dZDFxUvU}{A0}");

	private static readonly TextObject _shopIncomeStr = new TextObject("{=0g7MZCAK}Workshop Income");

	private static readonly TextObject _shopExpenseStr = new TextObject("{=cSuNR48H}Workshop Expense");

	private static readonly TextObject _mercenaryStr = new TextObject("{=qcaaJLhx}Mercenary Contract");

	private static readonly TextObject _mercenaryExpensesStr = new TextObject("{=5aElrlUt}Payment to Mercenaries");

	private static readonly TextObject _tributeExpensesStr = new TextObject("{=AtFv5RMW}Tribute Payments");

	private static readonly TextObject _tributeIncomeStr = new TextObject("{=rhfgzKtA}Tribute from {A0}");

	private static readonly TextObject _tributeIncomes = new TextObject("{=tributeIncome}Tribute Income");

	private static readonly TextObject _settlementIncome = new TextObject("{=AewK9qME}Settlement Income");

	private static readonly TextObject _mainPartywageStr = new TextObject("{=YkZKXsIn}Main party wages");

	private static readonly TextObject _caravanAndPartyIncome = new TextObject("{=8iLzK3Y4}Caravan and Party Income");

	private static readonly TextObject _garrisonAndPartyExpenses = new TextObject("{=ChUDSiJw}Garrison and Party Expense");

	private static readonly TextObject _debtStr = new TextObject("{=U3LdMEXb}Debts");

	private static readonly TextObject _kingdomSupport = new TextObject("{=essaRvXP}King's support");

	private static readonly TextObject _supportKing = new TextObject("{=WrJSUsBe}Support to king");

	private static readonly TextObject _workshopExpenseStr = new TextObject("{=oNgwQTTV}Workshop Expense");

	private static readonly TextObject _kingdomBudgetStr = new TextObject("{=7uzvI8e8}Kingdom Budget Expense");

	private static readonly TextObject _tariffTaxStr = new TextObject("{=wVMPdc8J}{A0}'s tariff");

	private static readonly TextObject _autoRecruitmentStr = new TextObject("{=6gvDrbe7}Recruitment Expense");

	private static readonly TextObject _alley = new TextObject("{=UQc6zg1Q}Owned Alleys");

	private const int PartyGoldIncomeThreshold = 10000;

	private const int payGarrisonWagesTreshold = 8000;

	private const int payClanPartiesTreshold = 4000;

	private const int payLeaderPartyWageTreshold = 2000;

	public override int PartyGoldLowerThreshold => 5000;

	public override ExplainedNumber CalculateClanGoldChange(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false, bool includeDetails = false)
	{
		ExplainedNumber goldChange = new ExplainedNumber(0f, includeDescriptions);
		CalculateClanIncomeInternal(clan, ref goldChange, applyWithdrawals, includeDetails);
		CalculateClanExpensesInternal(clan, ref goldChange, applyWithdrawals, includeDetails);
		return goldChange;
	}

	public override ExplainedNumber CalculateClanIncome(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false, bool includeDetails = false)
	{
		ExplainedNumber goldChange = new ExplainedNumber(0f, includeDescriptions);
		CalculateClanIncomeInternal(clan, ref goldChange, applyWithdrawals, includeDetails);
		return goldChange;
	}

	private void CalculateClanIncomeInternal(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false, bool includeDetails = false)
	{
		if (!clan.IsEliminated)
		{
			if (clan.Kingdom?.RulingClan == clan)
			{
				AddRulingClanIncome(clan, ref goldChange, applyWithdrawals, includeDetails);
			}
			if (clan != Clan.PlayerClan && (!clan.MapFaction.IsKingdomFaction || clan.IsUnderMercenaryService) && clan.Fiefs.Count == 0)
			{
				int num = clan.Tier * (80 + (clan.IsUnderMercenaryService ? 40 : 0));
				goldChange.Add(num);
			}
			AddMercenaryIncome(clan, ref goldChange, applyWithdrawals);
			AddSettlementIncome(clan, ref goldChange, applyWithdrawals, includeDetails);
			CalculateHeroIncomeFromWorkshops(clan.Leader, ref goldChange, applyWithdrawals);
			AddIncomeFromParties(clan, ref goldChange, applyWithdrawals, includeDetails);
			if (clan == Clan.PlayerClan)
			{
				AddPlayerClanIncomeFromOwnedAlleys(ref goldChange);
			}
			if (!clan.IsUnderMercenaryService)
			{
				AddIncomeFromTribute(clan, ref goldChange, applyWithdrawals, includeDetails);
			}
			if (clan.Gold < 30000 && clan.Kingdom != null && clan.Leader != Hero.MainHero && !clan.IsUnderMercenaryService)
			{
				AddIncomeFromKingdomBudget(clan, ref goldChange, applyWithdrawals);
			}
			Hero leader = clan.Leader;
			if (leader != null && leader.GetPerkValue(DefaultPerks.Trade.SpringOfGold))
			{
				int num2 = MathF.Min(1000, MathF.Round((float)clan.Leader.Gold * DefaultPerks.Trade.SpringOfGold.PrimaryBonus));
				goldChange.Add(num2, DefaultPerks.Trade.SpringOfGold.Name);
			}
		}
	}

	public void CalculateClanExpensesInternal(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false, bool includeDetails = false)
	{
		AddExpensesFromPartiesAndGarrisons(clan, ref goldChange, applyWithdrawals, includeDetails);
		if (!clan.IsUnderMercenaryService)
		{
			AddExpensesForHiredMercenaries(clan, ref goldChange, applyWithdrawals);
			AddExpensesForTributes(clan, ref goldChange, applyWithdrawals);
		}
		AddExpensesForAutoRecruitment(clan, ref goldChange, applyWithdrawals);
		if (clan.Gold > 100000 && clan.Kingdom != null && clan.Leader != Hero.MainHero && !clan.IsUnderMercenaryService)
		{
			int num = (int)(((float)clan.Gold - 100000f) * 0.01f);
			if (applyWithdrawals)
			{
				clan.Kingdom.KingdomBudgetWallet += num;
			}
			goldChange.Add(-num, _kingdomBudgetStr);
		}
		if (clan.DebtToKingdom > 0)
		{
			AddPaymentForDebts(clan, ref goldChange, applyWithdrawals);
		}
		if (Clan.PlayerClan == clan)
		{
			AddPlayerExpenseForWorkshops(ref goldChange);
		}
	}

	private void AddPlayerExpenseForWorkshops(ref ExplainedNumber goldChange)
	{
		int num = 0;
		foreach (Workshop ownedWorkshop in Hero.MainHero.OwnedWorkshops)
		{
			if (ownedWorkshop.Capital < Campaign.Current.Models.WorkshopModel.CapitalLowLimit)
			{
				num -= ownedWorkshop.Expense;
			}
		}
		goldChange.Add(num, _shopExpenseStr);
	}

	public override ExplainedNumber CalculateClanExpenses(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false, bool includeDetails = false)
	{
		ExplainedNumber goldChange = new ExplainedNumber(0f, includeDescriptions);
		CalculateClanExpensesInternal(clan, ref goldChange, applyWithdrawals, includeDetails);
		return goldChange;
	}

	private void AddPaymentForDebts(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
	{
		if (clan.Kingdom != null && clan.DebtToKingdom > 0)
		{
			int num = clan.DebtToKingdom;
			if (applyWithdrawals)
			{
				num = MathF.Min(num, (int)((float)clan.Gold + goldChange.ResultNumber));
				clan.DebtToKingdom -= num;
			}
			goldChange.Add(-num, _debtStr);
		}
	}

	private void AddRulingClanIncome(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals, bool includeDetails)
	{
		ExplainedNumber explainedNumber = new ExplainedNumber(0f, goldChange.IncludeDescriptions);
		int num = 0;
		int num2 = 0;
		bool flag = clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.LandTax);
		float num3 = 0f;
		foreach (Town fief in clan.Fiefs)
		{
			num += (int)Campaign.Current.Models.SettlementTaxModel.CalculateTownTax(fief).ResultNumber;
			num2++;
		}
		if (flag)
		{
			foreach (Village village in clan.Kingdom.Villages)
			{
				if (!village.IsOwnerUnassigned && village.Settlement.OwnerClan != clan && village.VillageState != Village.VillageStates.Looted && village.VillageState != Village.VillageStates.BeingRaided)
				{
					int num4 = (int)((float)village.TradeTaxAccumulated / RevenueSmoothenFraction());
					num3 += (float)num4 * 0.05f;
				}
			}
			if (num3 > 1E-05f)
			{
				explainedNumber.Add(num3, DefaultPolicies.LandTax.Name);
			}
		}
		Kingdom kingdom = clan.Kingdom;
		if (kingdom.RulingClan == clan)
		{
			if (kingdom.ActivePolicies.Contains(DefaultPolicies.WarTax))
			{
				int num5 = (int)((float)num * 0.05f);
				explainedNumber.Add(num5, DefaultPolicies.WarTax.Name);
			}
			if (kingdom.ActivePolicies.Contains(DefaultPolicies.DebasementOfTheCurrency))
			{
				explainedNumber.Add(num2 * 100, DefaultPolicies.DebasementOfTheCurrency.Name);
			}
		}
		int num6 = 0;
		int num7 = 0;
		foreach (Settlement settlement in clan.Settlements)
		{
			if (!settlement.IsTown)
			{
				continue;
			}
			if (kingdom.ActivePolicies.Contains(DefaultPolicies.RoadTolls))
			{
				int num8 = settlement.Town.TradeTaxAccumulated / 30;
				if (applyWithdrawals)
				{
					settlement.Town.TradeTaxAccumulated -= num8;
				}
				num6 += num8;
			}
			if (kingdom.ActivePolicies.Contains(DefaultPolicies.StateMonopolies))
			{
				num7 += (int)((float)settlement.Town.Workshops.Sum((Workshop t) => t.ProfitMade) * 0.05f);
			}
			if (num6 > 0)
			{
				explainedNumber.Add(num6, DefaultPolicies.RoadTolls.Name);
			}
			if (num7 > 0)
			{
				explainedNumber.Add(num7, DefaultPolicies.StateMonopolies.Name);
			}
		}
		if (!explainedNumber.ResultNumber.ApproximatelyEqualsTo(0f))
		{
			if (!includeDetails)
			{
				goldChange.Add(explainedNumber.ResultNumber, GameTexts.FindText("str_policies"));
			}
			else
			{
				goldChange.AddFromExplainedNumber(explainedNumber, GameTexts.FindText("str_policies"));
			}
		}
	}

	private void AddExpensesForHiredMercenaries(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
	{
		Kingdom kingdom = clan.Kingdom;
		if (kingdom == null)
		{
			return;
		}
		float num = CalculateShareFactor(clan);
		if (kingdom.MercenaryWallet < 0)
		{
			int num2 = (int)((float)(-kingdom.MercenaryWallet) * num);
			ApplyShareForExpenses(clan, ref goldChange, applyWithdrawals, num2, _mercenaryExpensesStr);
			if (applyWithdrawals)
			{
				kingdom.MercenaryWallet += num2;
			}
		}
	}

	private void AddExpensesForTributes(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
	{
		Kingdom kingdom = clan.Kingdom;
		if (kingdom == null)
		{
			return;
		}
		float num = CalculateShareFactor(clan);
		if (kingdom.TributeWallet >= 0)
		{
			return;
		}
		int num2 = (int)((float)(-kingdom.TributeWallet) * num);
		ApplyShareForExpenses(clan, ref goldChange, applyWithdrawals, num2, _tributeExpensesStr);
		if (applyWithdrawals)
		{
			kingdom.TributeWallet += num2;
			if (clan == Clan.PlayerClan)
			{
				CampaignEventDispatcher.Instance.OnPlayerEarnedGoldFromAsset(AssetIncomeType.TributesPaid, num2);
			}
		}
	}

	private static void ApplyShareForExpenses(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals, int expenseShare, TextObject mercenaryExpensesStr)
	{
		if (applyWithdrawals)
		{
			int num = (int)((float)clan.Gold + goldChange.ResultNumber);
			if (expenseShare > num)
			{
				int num2 = expenseShare - num;
				expenseShare = num;
				clan.DebtToKingdom += num2;
			}
		}
		goldChange.Add(-expenseShare, mercenaryExpensesStr);
	}

	private void AddSettlementIncome(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals, bool includeDetails)
	{
		ExplainedNumber explainedNumber = new ExplainedNumber(0f, goldChange.IncludeDescriptions);
		foreach (Town fief in clan.Fiefs)
		{
			ExplainedNumber explainedNumber2 = Campaign.Current.Models.SettlementTaxModel.CalculateTownTax(fief);
			ExplainedNumber explainedNumber3 = CalculateTownIncomeFromTariffs(clan, fief, applyWithdrawals);
			int num = CalculateTownIncomeFromProjects(fief);
			explainedNumber.Add((int)explainedNumber2.ResultNumber, _townTaxStr, fief.Name);
			explainedNumber.Add((int)explainedNumber3.ResultNumber, _tariffTaxStr, fief.Name);
			explainedNumber.Add(num, _projectsIncomeStr);
			foreach (Village village in fief.Villages)
			{
				int num2 = CalculateVillageIncome(clan, village, applyWithdrawals);
				explainedNumber.Add(num2, village.Name);
			}
		}
		if (!includeDetails)
		{
			goldChange.Add(explainedNumber.ResultNumber, _settlementIncome);
		}
		else
		{
			goldChange.AddFromExplainedNumber(explainedNumber, _settlementIncome);
		}
	}

	public override ExplainedNumber CalculateTownIncomeFromTariffs(Clan clan, Town town, bool applyWithdrawals = false)
	{
		ExplainedNumber bonuses = new ExplainedNumber((int)((float)town.TradeTaxAccumulated / RevenueSmoothenFraction()));
		int num = MathF.Round(bonuses.ResultNumber);
		PerkHelper.AddPerkBonusForTown(DefaultPerks.Trade.ContentTrades, town, ref bonuses);
		PerkHelper.AddPerkBonusForTown(DefaultPerks.Crossbow.Steady, town, ref bonuses);
		PerkHelper.AddPerkBonusForTown(DefaultPerks.Roguery.SaltTheEarth, town, ref bonuses);
		PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.GivingHands, town, ref bonuses);
		if (applyWithdrawals)
		{
			town.TradeTaxAccumulated -= num;
			if (clan == Clan.PlayerClan)
			{
				CampaignEventDispatcher.Instance.OnPlayerEarnedGoldFromAsset(AssetIncomeType.Taxes, (int)bonuses.ResultNumber);
			}
		}
		return bonuses;
	}

	public override int CalculateTownIncomeFromProjects(Town town)
	{
		if (town.CurrentDefaultBuilding != null && town.Governor != null && town.Governor.GetPerkValue(DefaultPerks.Engineering.ArchitecturalCommisions))
		{
			return (int)DefaultPerks.Engineering.ArchitecturalCommisions.SecondaryBonus;
		}
		return 0;
	}

	public override int CalculateVillageIncome(Clan clan, Village village, bool applyWithdrawals = false)
	{
		int num = ((village.VillageState != Village.VillageStates.Looted && village.VillageState != Village.VillageStates.BeingRaided) ? ((int)((float)village.TradeTaxAccumulated / RevenueSmoothenFraction())) : 0);
		int num2 = num;
		if (clan.Kingdom != null && clan.Kingdom.RulingClan != clan && clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.LandTax))
		{
			num -= (int)(0.05f * (float)num);
		}
		if (village.Bound.Town != null && village.Bound.Town.Governor != null && village.Bound.Town.Governor.GetPerkValue(DefaultPerks.Scouting.ForestKin))
		{
			num += MathF.Round((float)num * DefaultPerks.Scouting.ForestKin.SecondaryBonus);
		}
		if (village.Bound?.Town?.Governor != null && village.Bound.Town.Governor.GetPerkValue(DefaultPerks.Steward.Logistician))
		{
			num += MathF.Round((float)num * DefaultPerks.Steward.Logistician.SecondaryBonus);
		}
		if (applyWithdrawals)
		{
			village.TradeTaxAccumulated -= num2;
			if (clan == Clan.PlayerClan)
			{
				CampaignEventDispatcher.Instance.OnPlayerEarnedGoldFromAsset(AssetIncomeType.Taxes, num);
			}
		}
		return num;
	}

	private static float CalculateShareFactor(Clan clan)
	{
		Kingdom kingdom = clan.Kingdom;
		int num = kingdom.Fiefs.Sum((Town x) => x.IsCastle ? 1 : 3) + 1 + kingdom.Clans.Count;
		return (float)(clan.Fiefs.Sum((Town x) => x.IsCastle ? 1 : 3) + ((clan == kingdom.RulingClan) ? 1 : 0) + 1) / (float)num;
	}

	private void AddMercenaryIncome(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
	{
		if (clan.IsUnderMercenaryService && clan.Leader != null && clan.Kingdom != null)
		{
			int num = MathF.Ceiling(clan.Influence * (1f / Campaign.Current.Models.ClanFinanceModel.RevenueSmoothenFraction())) * clan.MercenaryAwardMultiplier;
			if (applyWithdrawals)
			{
				clan.Kingdom.MercenaryWallet -= num;
			}
			goldChange.Add(num, _mercenaryStr);
		}
	}

	private void AddIncomeFromKingdomBudget(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
	{
		int num = ((clan.Gold < 5000) ? 2000 : ((clan.Gold < 10000) ? 1500 : ((clan.Gold < 20000) ? 1000 : 500)));
		num *= ((clan.Kingdom.KingdomBudgetWallet <= 1000000) ? 1 : 2);
		num *= ((clan.Leader != clan.Kingdom.Leader) ? 1 : 2);
		int num2 = MathF.Min(clan.Kingdom.KingdomBudgetWallet, num);
		if (applyWithdrawals)
		{
			clan.Kingdom.KingdomBudgetWallet -= num2;
		}
		goldChange.Add(num2, _kingdomSupport);
	}

	private void AddPlayerClanIncomeFromOwnedAlleys(ref ExplainedNumber goldChange)
	{
		int num = 0;
		foreach (Alley ownedAlley in Hero.MainHero.OwnedAlleys)
		{
			num += Campaign.Current.Models.AlleyModel.GetDailyIncomeOfAlley(ownedAlley);
		}
		goldChange.Add(num, _alley);
	}

	private void AddIncomeFromTribute(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals, bool includeDetails)
	{
		ExplainedNumber explainedNumber = new ExplainedNumber(0f, goldChange.IncludeDescriptions);
		IFaction mapFaction = clan.MapFaction;
		float num = 1f;
		if (clan.Kingdom != null)
		{
			num = CalculateShareFactor(clan);
		}
		foreach (StanceLink stance in mapFaction.Stances)
		{
			if (!stance.IsNeutral || stance.GetDailyTributePaid(mapFaction) >= 0)
			{
				continue;
			}
			int num2 = (int)((float)stance.GetDailyTributePaid(mapFaction) * num);
			IFaction faction = ((stance.Faction1 == mapFaction) ? stance.Faction2 : stance.Faction1);
			if (applyWithdrawals)
			{
				faction.TributeWallet += num2;
				if (stance.Faction1 == mapFaction)
				{
					stance.TotalTributePaidby2 += -num2;
				}
				if (stance.Faction2 == mapFaction)
				{
					stance.TotalTributePaidby1 += -num2;
				}
				if (clan == Clan.PlayerClan)
				{
					CampaignEventDispatcher.Instance.OnPlayerEarnedGoldFromAsset(AssetIncomeType.TributesEarned, num2);
				}
			}
			explainedNumber.Add(-num2, _tributeIncomeStr, faction.InformalName);
		}
		if (!includeDetails)
		{
			goldChange.Add(explainedNumber.ResultNumber, _tributeIncomes);
		}
		else
		{
			goldChange.AddFromExplainedNumber(explainedNumber, _tributeIncomes);
		}
	}

	private void AddIncomeFromParties(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals, bool includeDetails)
	{
		ExplainedNumber explainedNumber = new ExplainedNumber(0f, goldChange.IncludeDescriptions);
		foreach (Hero lord in clan.Lords)
		{
			foreach (CaravanPartyComponent ownedCaravan in lord.OwnedCaravans)
			{
				if (ownedCaravan.MobileParty.IsActive && ownedCaravan.MobileParty.LeaderHero != clan.Leader && (ownedCaravan.MobileParty.IsLordParty || ownedCaravan.MobileParty.IsGarrison || ownedCaravan.MobileParty.IsCaravan))
				{
					int num = AddIncomeFromParty(ownedCaravan.MobileParty, clan, ref goldChange, applyWithdrawals);
					explainedNumber.Add(num, _caravanIncomeStr, (ownedCaravan.Leader != null) ? ownedCaravan.Leader.Name : ownedCaravan.Name);
				}
			}
		}
		foreach (Hero companion in clan.Companions)
		{
			foreach (CaravanPartyComponent ownedCaravan2 in companion.OwnedCaravans)
			{
				if (ownedCaravan2.MobileParty.IsActive && ownedCaravan2.MobileParty.LeaderHero != clan.Leader && (ownedCaravan2.MobileParty.IsLordParty || ownedCaravan2.MobileParty.IsGarrison || ownedCaravan2.MobileParty.IsCaravan))
				{
					int num2 = AddIncomeFromParty(ownedCaravan2.MobileParty, clan, ref goldChange, applyWithdrawals);
					explainedNumber.Add(num2, _caravanIncomeStr, (ownedCaravan2.Leader != null) ? ownedCaravan2.Leader.Name : ownedCaravan2.Name);
				}
			}
		}
		foreach (WarPartyComponent warPartyComponent in clan.WarPartyComponents)
		{
			if (warPartyComponent.MobileParty.IsActive && warPartyComponent.MobileParty.LeaderHero != clan.Leader && (warPartyComponent.MobileParty.IsLordParty || warPartyComponent.MobileParty.IsGarrison || warPartyComponent.MobileParty.IsCaravan))
			{
				int num3 = AddIncomeFromParty(warPartyComponent.MobileParty, clan, ref goldChange, applyWithdrawals);
				explainedNumber.Add(num3, _partyIncomeStr, warPartyComponent.MobileParty.Name);
			}
		}
		if (!includeDetails)
		{
			goldChange.Add(explainedNumber.ResultNumber, _caravanAndPartyIncome);
		}
		else
		{
			goldChange.AddFromExplainedNumber(explainedNumber, _caravanAndPartyIncome);
		}
	}

	private int AddIncomeFromParty(MobileParty party, Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
	{
		int num = 0;
		if (party.IsActive && party.LeaderHero != clan.Leader && (party.IsLordParty || party.IsGarrison || party.IsCaravan))
		{
			int num2 = ((party.IsLordParty && party.LeaderHero != null) ? party.LeaderHero.Gold : party.PartyTradeGold);
			if (num2 > 10000)
			{
				num = (num2 - 10000) / 10;
				if (applyWithdrawals)
				{
					RemovePartyGold(party, num);
					if (party.LeaderHero != null && num > 0)
					{
						SkillLevelingManager.OnTradeProfitMade(party.LeaderHero, num);
					}
					if (party.Party.Owner?.Clan?.Leader != null && party.IsCaravan && party.Party.Owner.Clan.Leader.GetPerkValue(DefaultPerks.Trade.GreatInvestor) && num > 0)
					{
						party.Party.Owner.Clan.AddRenown(DefaultPerks.Trade.GreatInvestor.PrimaryBonus);
					}
					if (clan == Clan.PlayerClan && party.IsCaravan)
					{
						CampaignEventDispatcher.Instance.OnPlayerEarnedGoldFromAsset(AssetIncomeType.Caravan, num);
					}
				}
			}
		}
		return num;
	}

	private void AddExpensesFromPartiesAndGarrisons(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals, bool includeDetails)
	{
		ExplainedNumber explainedNumber = new ExplainedNumber(0f, goldChange.IncludeDescriptions);
		int num = AddExpenseFromLeaderParty(clan, goldChange, applyWithdrawals);
		explainedNumber.Add(num, _mainPartywageStr);
		foreach (Hero lord in clan.Lords)
		{
			foreach (CaravanPartyComponent ownedCaravan in lord.OwnedCaravans)
			{
				if (ownedCaravan.MobileParty.IsActive && ownedCaravan.MobileParty.LeaderHero != clan.Leader)
				{
					int num2 = AddPartyExpense(ownedCaravan.MobileParty, clan, goldChange, applyWithdrawals);
					explainedNumber.Add(num2, _partyExpensesStr, ownedCaravan.Name);
				}
			}
		}
		foreach (Hero companion in clan.Companions)
		{
			foreach (CaravanPartyComponent ownedCaravan2 in companion.OwnedCaravans)
			{
				int num3 = AddPartyExpense(ownedCaravan2.MobileParty, clan, goldChange, applyWithdrawals);
				explainedNumber.Add(num3, _partyExpensesStr, ownedCaravan2.Name);
			}
		}
		foreach (WarPartyComponent warPartyComponent in clan.WarPartyComponents)
		{
			if (warPartyComponent.MobileParty.IsActive && warPartyComponent.MobileParty.LeaderHero != clan.Leader)
			{
				int num4 = AddPartyExpense(warPartyComponent.MobileParty, clan, goldChange, applyWithdrawals);
				explainedNumber.Add(num4, _partyExpensesStr, warPartyComponent.Name);
			}
		}
		foreach (Town fief in clan.Fiefs)
		{
			if (fief.GarrisonParty != null && fief.GarrisonParty.IsActive)
			{
				int num5 = AddPartyExpense(fief.GarrisonParty, clan, goldChange, applyWithdrawals);
				TextObject textObject = new TextObject("{=fsTBcLvA}{SETTLEMENT} Garrison");
				textObject.SetTextVariable("SETTLEMENT", fief.Name);
				explainedNumber.Add(num5, _partyExpensesStr, textObject);
			}
		}
		if (!includeDetails)
		{
			goldChange.Add(explainedNumber.ResultNumber, _garrisonAndPartyExpenses);
		}
		else
		{
			goldChange.AddFromExplainedNumber(explainedNumber, _garrisonAndPartyExpenses);
		}
	}

	private void AddExpensesForAutoRecruitment(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false)
	{
		int num = clan.AutoRecruitmentExpenses / 5;
		if (applyWithdrawals)
		{
			clan.AutoRecruitmentExpenses -= num;
		}
		goldChange.Add(-num, _autoRecruitmentStr);
	}

	private int AddExpenseFromLeaderParty(Clan clan, ExplainedNumber goldChange, bool applyWithdrawals)
	{
		MobileParty mobileParty = clan.Leader?.PartyBelongedTo;
		if (mobileParty != null)
		{
			int num = clan.Gold + (int)goldChange.ResultNumber;
			if (num < 2000 && applyWithdrawals && clan != Clan.PlayerClan)
			{
				num = 0;
			}
			return -CalculatePartyWage(mobileParty, num, applyWithdrawals);
		}
		return 0;
	}

	private int AddPartyExpense(MobileParty party, Clan clan, ExplainedNumber goldChange, bool applyWithdrawals)
	{
		int num = clan.Gold + (int)goldChange.ResultNumber;
		int num2 = num;
		if (num < (party.IsGarrison ? 8000 : 4000) && applyWithdrawals && clan != Clan.PlayerClan)
		{
			num2 = ((party.LeaderHero != null && party.LeaderHero.Gold < 500) ? MathF.Min(num, 250) : 0);
		}
		int num3 = CalculatePartyWage(party, num2, applyWithdrawals);
		int num4 = ((party.IsLordParty && party.LeaderHero != null) ? party.LeaderHero.Gold : party.PartyTradeGold);
		if (applyWithdrawals)
		{
			if (party.IsLordParty)
			{
				if (party.LeaderHero != null)
				{
					party.LeaderHero.Gold -= num3;
				}
				else
				{
					party.ActualClan.Leader.Gold -= num3;
				}
			}
			else
			{
				party.PartyTradeGold -= num3;
			}
		}
		num4 -= num3;
		if (num4 < PartyGoldLowerThreshold)
		{
			int num5 = PartyGoldLowerThreshold - num4;
			if (applyWithdrawals)
			{
				num5 = MathF.Min(num5, num2);
				if (party.IsLordParty && party.LeaderHero != null)
				{
					party.LeaderHero.Gold += num5;
				}
				else
				{
					party.PartyTradeGold += num5;
				}
			}
			return -num5;
		}
		return 0;
	}

	public override int CalculateOwnerIncomeFromCaravan(MobileParty caravan)
	{
		return (int)((float)MathF.Max(0, caravan.PartyTradeGold - 10000) / RevenueSmoothenFraction());
	}

	private void RemovePartyGold(MobileParty party, int share)
	{
		if (party.IsLordParty && party.LeaderHero != null)
		{
			party.LeaderHero.Gold -= share;
		}
		else
		{
			party.PartyTradeGold -= share;
		}
	}

	public override int CalculateOwnerIncomeFromWorkshop(Workshop workshop)
	{
		return (int)((float)MathF.Max(0, workshop.ProfitMade) / RevenueSmoothenFraction());
	}

	private void CalculateHeroIncomeFromAssets(Hero hero, ref ExplainedNumber goldChange, bool applyWithdrawals)
	{
		int num = 0;
		foreach (CaravanPartyComponent ownedCaravan in hero.OwnedCaravans)
		{
			if (ownedCaravan.MobileParty.PartyTradeGold > 10000)
			{
				int num2 = Campaign.Current.Models.ClanFinanceModel.CalculateOwnerIncomeFromCaravan(ownedCaravan.MobileParty);
				if (applyWithdrawals)
				{
					ownedCaravan.MobileParty.PartyTradeGold -= num2;
					SkillLevelingManager.OnTradeProfitMade(hero, num2);
				}
				if (num2 > 0)
				{
					num += num2;
				}
			}
		}
		goldChange.Add(num, _caravanIncomeStr);
		CalculateHeroIncomeFromWorkshops(hero, ref goldChange, applyWithdrawals);
		if (hero.CurrentSettlement == null)
		{
			return;
		}
		foreach (Alley alley in hero.CurrentSettlement.Alleys)
		{
			if (alley.Owner == hero)
			{
				goldChange.Add(30f, alley.Name);
			}
		}
	}

	private void CalculateHeroIncomeFromWorkshops(Hero hero, ref ExplainedNumber goldChange, bool applyWithdrawals)
	{
		int num = 0;
		int num2 = 0;
		foreach (Workshop ownedWorkshop in hero.OwnedWorkshops)
		{
			int num3 = Campaign.Current.Models.ClanFinanceModel.CalculateOwnerIncomeFromWorkshop(ownedWorkshop);
			num += num3;
			if (applyWithdrawals && num3 > 0)
			{
				ownedWorkshop.ChangeGold(-num3);
				if (hero == Hero.MainHero)
				{
					CampaignEventDispatcher.Instance.OnPlayerEarnedGoldFromAsset(AssetIncomeType.Workshop, num3);
				}
			}
			if (num3 > 0)
			{
				num2++;
			}
		}
		goldChange.Add(num, _shopIncomeStr);
		if (hero.Clan != null && (hero.Clan.Leader?.GetPerkValue(DefaultPerks.Trade.ArtisanCommunity) ?? false) && applyWithdrawals && num2 > 0)
		{
			hero.Clan.AddRenown((float)num2 * DefaultPerks.Trade.ArtisanCommunity.PrimaryBonus);
		}
	}

	public override float RevenueSmoothenFraction()
	{
		return 5f;
	}

	private int CalculatePartyWage(MobileParty mobileParty, int budget, bool applyWithdrawals)
	{
		int totalWage = mobileParty.TotalWage;
		int num = totalWage;
		if (applyWithdrawals)
		{
			num = MathF.Min(totalWage, budget);
			ApplyMoraleEffect(mobileParty, totalWage, num);
		}
		return num;
	}

	public override int CalculateNotableDailyGoldChange(Hero hero, bool applyWithdrawals)
	{
		ExplainedNumber goldChange = new ExplainedNumber(0f, includeDescriptions: false, null);
		CalculateHeroIncomeFromAssets(hero, ref goldChange, applyWithdrawals);
		return (int)goldChange.ResultNumber;
	}

	private static void ApplyMoraleEffect(MobileParty mobileParty, int wage, int paymentAmount)
	{
		if (paymentAmount < wage && wage > 0)
		{
			float num = 1f - (float)paymentAmount / (float)wage;
			float num2 = (float)Campaign.Current.Models.PartyMoraleModel.GetDailyNoWageMoralePenalty(mobileParty) * num;
			if (mobileParty.HasUnpaidWages < num)
			{
				num2 += (float)Campaign.Current.Models.PartyMoraleModel.GetDailyNoWageMoralePenalty(mobileParty) * (num - mobileParty.HasUnpaidWages);
			}
			mobileParty.RecentEventsMorale += num2;
			mobileParty.HasUnpaidWages = num;
			MBTextManager.SetTextVariable("reg1", MathF.Round(MathF.Abs(num2), 1));
			if (mobileParty == MobileParty.MainParty)
			{
				MBInformationManager.AddQuickInformation(GameTexts.FindText("str_party_loses_moral_due_to_insufficent_funds"));
			}
		}
		else
		{
			mobileParty.HasUnpaidWages = 0f;
		}
	}
}
