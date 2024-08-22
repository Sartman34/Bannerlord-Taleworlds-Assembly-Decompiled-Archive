using System;
using System.Linq;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;

public class KingdomDiplomacyVM : KingdomCategoryVM
{
	private KingdomDecision _currentItemsUnresolvedDecision;

	private readonly Action<KingdomDecision> _forceDecision;

	private readonly Kingdom _playerKingdom;

	private int _dailyPeaceTributeToPay;

	private bool _isChangingDiplomacyItem;

	private MBBindingList<KingdomWarItemVM> _playerWars;

	private MBBindingList<KingdomTruceItemVM> _playerTruces;

	private KingdomWarSortControllerVM _warsSortController;

	private KingdomDiplomacyItemVM _currentSelectedItem;

	private SelectorVM<SelectorItemVM> _behaviorSelection;

	private HintViewModel _showStatBarsHint;

	private HintViewModel _showWarLogsHint;

	private HintViewModel _actionHint;

	private string _playerWarsText;

	private string _numOfPlayerWarsText;

	private string _otherWarsText;

	private string _numOfOtherWarsText;

	private string _warsText;

	private string _actionName;

	private string _proposeActionExplanationText;

	private string _behaviorSelectionTitle;

	private int _actionInfluenceCost;

	private bool _isActionEnabled;

	private bool _isDisplayingWarLogs;

	private bool _isDisplayingStatComparisons;

	private bool _isWar;

	[DataSourceProperty]
	public MBBindingList<KingdomWarItemVM> PlayerWars
	{
		get
		{
			return _playerWars;
		}
		set
		{
			if (value != _playerWars)
			{
				_playerWars = value;
				OnPropertyChangedWithValue(value, "PlayerWars");
			}
		}
	}

	[DataSourceProperty]
	public int ActionInfluenceCost
	{
		get
		{
			return _actionInfluenceCost;
		}
		set
		{
			if (value != _actionInfluenceCost)
			{
				_actionInfluenceCost = value;
				OnPropertyChangedWithValue(value, "ActionInfluenceCost");
			}
		}
	}

	[DataSourceProperty]
	public bool IsActionEnabled
	{
		get
		{
			return _isActionEnabled;
		}
		set
		{
			if (value != _isActionEnabled)
			{
				_isActionEnabled = value;
				OnPropertyChangedWithValue(value, "IsActionEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDisplayingWarLogs
	{
		get
		{
			return _isDisplayingWarLogs;
		}
		set
		{
			if (value != _isDisplayingWarLogs)
			{
				_isDisplayingWarLogs = value;
				OnPropertyChangedWithValue(value, "IsDisplayingWarLogs");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDisplayingStatComparisons
	{
		get
		{
			return _isDisplayingStatComparisons;
		}
		set
		{
			if (value != _isDisplayingStatComparisons)
			{
				_isDisplayingStatComparisons = value;
				OnPropertyChangedWithValue(value, "IsDisplayingStatComparisons");
			}
		}
	}

	[DataSourceProperty]
	public bool IsWar
	{
		get
		{
			return _isWar;
		}
		set
		{
			if (value != _isWar)
			{
				_isWar = value;
				if (!value)
				{
					ExecuteShowStatComparisons();
				}
				OnPropertyChangedWithValue(value, "IsWar");
			}
		}
	}

	[DataSourceProperty]
	public string ActionName
	{
		get
		{
			return _actionName;
		}
		set
		{
			if (value != _actionName)
			{
				_actionName = value;
				OnPropertyChangedWithValue(value, "ActionName");
			}
		}
	}

	[DataSourceProperty]
	public string ProposeActionExplanationText
	{
		get
		{
			return _proposeActionExplanationText;
		}
		set
		{
			if (value != _proposeActionExplanationText)
			{
				_proposeActionExplanationText = value;
				OnPropertyChangedWithValue(value, "ProposeActionExplanationText");
			}
		}
	}

	[DataSourceProperty]
	public string BehaviorSelectionTitle
	{
		get
		{
			return _behaviorSelectionTitle;
		}
		set
		{
			if (value != _behaviorSelectionTitle)
			{
				_behaviorSelectionTitle = value;
				OnPropertyChangedWithValue(value, "BehaviorSelectionTitle");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<KingdomTruceItemVM> PlayerTruces
	{
		get
		{
			return _playerTruces;
		}
		set
		{
			if (value != _playerTruces)
			{
				_playerTruces = value;
				OnPropertyChangedWithValue(value, "PlayerTruces");
			}
		}
	}

	[DataSourceProperty]
	public KingdomDiplomacyItemVM CurrentSelectedDiplomacyItem
	{
		get
		{
			return _currentSelectedItem;
		}
		set
		{
			if (value != _currentSelectedItem)
			{
				_isChangingDiplomacyItem = true;
				_currentSelectedItem = value;
				IsWar = value is KingdomWarItemVM;
				OnPropertyChangedWithValue(value, "CurrentSelectedDiplomacyItem");
				_isChangingDiplomacyItem = false;
			}
		}
	}

	[DataSourceProperty]
	public KingdomWarSortControllerVM WarsSortController
	{
		get
		{
			return _warsSortController;
		}
		set
		{
			if (value != _warsSortController)
			{
				_warsSortController = value;
				OnPropertyChangedWithValue(value, "WarsSortController");
			}
		}
	}

	[DataSourceProperty]
	public string PlayerWarsText
	{
		get
		{
			return _playerWarsText;
		}
		set
		{
			if (value != _playerWarsText)
			{
				_playerWarsText = value;
				OnPropertyChangedWithValue(value, "PlayerWarsText");
			}
		}
	}

	[DataSourceProperty]
	public string WarsText
	{
		get
		{
			return _warsText;
		}
		set
		{
			if (value != _warsText)
			{
				_warsText = value;
				OnPropertyChangedWithValue(value, "WarsText");
			}
		}
	}

	[DataSourceProperty]
	public string NumOfPlayerWarsText
	{
		get
		{
			return _numOfPlayerWarsText;
		}
		set
		{
			if (value != _numOfPlayerWarsText)
			{
				_numOfPlayerWarsText = value;
				OnPropertyChangedWithValue(value, "NumOfPlayerWarsText");
			}
		}
	}

	[DataSourceProperty]
	public string PlayerTrucesText
	{
		get
		{
			return _otherWarsText;
		}
		set
		{
			if (value != _otherWarsText)
			{
				_otherWarsText = value;
				OnPropertyChangedWithValue(value, "PlayerTrucesText");
			}
		}
	}

	[DataSourceProperty]
	public string NumOfPlayerTrucesText
	{
		get
		{
			return _numOfOtherWarsText;
		}
		set
		{
			if (value != _numOfOtherWarsText)
			{
				_numOfOtherWarsText = value;
				OnPropertyChangedWithValue(value, "NumOfPlayerTrucesText");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<SelectorItemVM> BehaviorSelection
	{
		get
		{
			return _behaviorSelection;
		}
		set
		{
			if (value != _behaviorSelection)
			{
				_behaviorSelection = value;
				OnPropertyChangedWithValue(value, "BehaviorSelection");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ShowStatBarsHint
	{
		get
		{
			return _showStatBarsHint;
		}
		set
		{
			if (value != _showStatBarsHint)
			{
				_showStatBarsHint = value;
				OnPropertyChangedWithValue(value, "ShowStatBarsHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ShowWarLogsHint
	{
		get
		{
			return _showWarLogsHint;
		}
		set
		{
			if (value != _showWarLogsHint)
			{
				_showWarLogsHint = value;
				OnPropertyChangedWithValue(value, "ShowWarLogsHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ActionHint
	{
		get
		{
			return _actionHint;
		}
		set
		{
			if (value != _actionHint)
			{
				_actionHint = value;
				OnPropertyChangedWithValue(value, "ActionHint");
			}
		}
	}

	public KingdomDiplomacyVM(Action<KingdomDecision> forceDecision)
	{
		_forceDecision = forceDecision;
		_playerKingdom = Hero.MainHero.MapFaction as Kingdom;
		PlayerWars = new MBBindingList<KingdomWarItemVM>();
		PlayerTruces = new MBBindingList<KingdomTruceItemVM>();
		WarsSortController = new KingdomWarSortControllerVM(ref _playerWars);
		ActionHint = new HintViewModel();
		ExecuteShowStatComparisons();
		RefreshValues();
		SetDefaultSelectedItem();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		BehaviorSelection = new SelectorVM<SelectorItemVM>(0, OnBehaviorSelectionChanged);
		BehaviorSelection.AddItem(new SelectorItemVM(GameTexts.FindText("str_kingdom_war_strategy_balanced"), GameTexts.FindText("str_kingdom_war_strategy_balanced_desc")));
		BehaviorSelection.AddItem(new SelectorItemVM(GameTexts.FindText("str_kingdom_war_strategy_defensive"), GameTexts.FindText("str_kingdom_war_strategy_defensive_desc")));
		BehaviorSelection.AddItem(new SelectorItemVM(GameTexts.FindText("str_kingdom_war_strategy_offensive"), GameTexts.FindText("str_kingdom_war_strategy_offensive_desc")));
		RefreshDiplomacyList();
		base.NotificationCount = Clan.PlayerClan.Kingdom?.UnresolvedDecisions.Count((KingdomDecision d) => !d.ShouldBeCancelled()) ?? 0;
		BehaviorSelectionTitle = GameTexts.FindText("str_kingdom_war_strategy").ToString();
		base.NoItemSelectedText = GameTexts.FindText("str_kingdom_no_war_selected").ToString();
		PlayerWarsText = GameTexts.FindText("str_kingdom_at_war").ToString();
		PlayerTrucesText = GameTexts.FindText("str_kingdom_at_peace").ToString();
		WarsText = GameTexts.FindText("str_diplomatic_group").ToString();
		ShowStatBarsHint = new HintViewModel(GameTexts.FindText("str_kingdom_war_show_comparison_bars"));
		ShowWarLogsHint = new HintViewModel(GameTexts.FindText("str_kingdom_war_show_war_logs"));
		PlayerWars.ApplyActionOnAllItems(delegate(KingdomWarItemVM x)
		{
			x.RefreshValues();
		});
		PlayerTruces.ApplyActionOnAllItems(delegate(KingdomTruceItemVM x)
		{
			x.RefreshValues();
		});
		CurrentSelectedDiplomacyItem?.RefreshValues();
	}

	public void RefreshDiplomacyList()
	{
		PlayerWars.Clear();
		PlayerTruces.Clear();
		foreach (StanceLink item in from x in _playerKingdom.Stances
			where x.IsAtWar
			select x into w
			orderby w.Faction1.Name.ToString() + w.Faction2.Name.ToString()
			select w)
		{
			if (item.Faction1.IsKingdomFaction && item.Faction2.IsKingdomFaction)
			{
				PlayerWars.Add(new KingdomWarItemVM(item, OnDiplomacyItemSelection, OnDeclarePeace));
			}
		}
		foreach (Kingdom item2 in Kingdom.All)
		{
			if (item2 != _playerKingdom && !item2.IsEliminated && (FactionManager.IsAlliedWithFaction(item2, _playerKingdom) || FactionManager.IsNeutralWithFaction(item2, _playerKingdom)))
			{
				PlayerTruces.Add(new KingdomTruceItemVM(_playerKingdom, item2, OnDiplomacyItemSelection, OnDeclareWar));
			}
		}
		GameTexts.SetVariable("STR", PlayerWars.Count);
		NumOfPlayerWarsText = GameTexts.FindText("str_STR_in_parentheses").ToString();
		GameTexts.SetVariable("STR", PlayerTruces.Count);
		NumOfPlayerTrucesText = GameTexts.FindText("str_STR_in_parentheses").ToString();
		SetDefaultSelectedItem();
	}

	public void SelectKingdom(Kingdom kingdom)
	{
		bool flag = false;
		foreach (KingdomWarItemVM playerWar in PlayerWars)
		{
			if (playerWar.Faction1 == kingdom || playerWar.Faction2 == kingdom)
			{
				OnSetCurrentDiplomacyItem(playerWar);
				flag = true;
				break;
			}
		}
		if (flag)
		{
			return;
		}
		foreach (KingdomTruceItemVM playerTruce in PlayerTruces)
		{
			if (playerTruce.Faction1 == kingdom || playerTruce.Faction2 == kingdom)
			{
				OnSetCurrentDiplomacyItem(playerTruce);
				flag = true;
				break;
			}
		}
	}

	private void OnSetCurrentDiplomacyItem(KingdomDiplomacyItemVM item)
	{
		if (item is KingdomWarItemVM)
		{
			OnSetWarItem(item as KingdomWarItemVM);
		}
		else if (item is KingdomTruceItemVM)
		{
			OnSetPeaceItem(item as KingdomTruceItemVM);
		}
		RefreshCurrentWarVisuals(item);
		UpdateBehaviorSelection();
	}

	private void OnSetWarItem(KingdomWarItemVM item)
	{
		_currentItemsUnresolvedDecision = Clan.PlayerClan.Kingdom.UnresolvedDecisions.FirstOrDefault((KingdomDecision d) => d is MakePeaceKingdomDecision makePeaceKingdomDecision && makePeaceKingdomDecision.FactionToMakePeaceWith == item.Faction2 && !d.ShouldBeCancelled());
		if (_currentItemsUnresolvedDecision != null)
		{
			_dailyPeaceTributeToPay = (_currentItemsUnresolvedDecision as MakePeaceKingdomDecision)?.DailyTributeToBePaid ?? 0;
			ActionName = GameTexts.FindText("str_resolve").ToString();
			ActionInfluenceCost = 0;
			IsActionEnabled = GetActionStatusForDiplomacyItemWithReason(item, isResolve: true, out var disabledReason);
			ActionHint.HintText = disabledReason;
			ProposeActionExplanationText = GameTexts.FindText("str_resolve_explanation").ToString();
			return;
		}
		ActionName = ((_playerKingdom.Clans.Count > 1) ? GameTexts.FindText("str_policy_propose").ToString() : GameTexts.FindText("str_policy_enact").ToString());
		ActionInfluenceCost = Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfProposingPeace(Clan.PlayerClan);
		PeaceBarterable peaceBarterable = new PeaceBarterable(_playerKingdom.Leader, _playerKingdom, item.Faction2, CampaignTime.Years(1f));
		int num = -peaceBarterable.GetValueForFaction(item.Faction2);
		if (item.Faction2 is Kingdom)
		{
			foreach (Clan clan in ((Kingdom)item.Faction2).Clans)
			{
				int num2 = -peaceBarterable.GetValueForFaction(clan);
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		int num3 = num;
		if (num3 > -5000 && num3 < 5000)
		{
			num3 = 0;
		}
		_dailyPeaceTributeToPay = Campaign.Current.Models.DiplomacyModel.GetDailyTributeForValue(num3);
		_dailyPeaceTributeToPay = 10 * (_dailyPeaceTributeToPay / 10);
		IsActionEnabled = GetActionStatusForDiplomacyItemWithReason(item, isResolve: false, out var disabledReason2);
		ActionHint.HintText = disabledReason2;
		TextObject textObject = ((_dailyPeaceTributeToPay == 0) ? GameTexts.FindText("str_propose_peace_explanation") : ((_dailyPeaceTributeToPay > 0) ? GameTexts.FindText("str_propose_peace_explanation_pay_tribute") : GameTexts.FindText("str_propose_peace_explanation_get_tribute")));
		ProposeActionExplanationText = textObject.SetTextVariable("SUPPORT", CalculatePeaceSupport(item, _dailyPeaceTributeToPay)).SetTextVariable("TRIBUTE", TaleWorlds.Library.MathF.Abs(_dailyPeaceTributeToPay)).ToString();
		base.NotificationCount = Clan.PlayerClan.Kingdom?.UnresolvedDecisions.Count ?? 0;
	}

	private void OnSetPeaceItem(KingdomTruceItemVM item)
	{
		_currentItemsUnresolvedDecision = Clan.PlayerClan.Kingdom.UnresolvedDecisions.FirstOrDefault((KingdomDecision d) => d is DeclareWarDecision declareWarDecision && declareWarDecision.FactionToDeclareWarOn == item.Faction2 && !d.ShouldBeCancelled());
		if (_currentItemsUnresolvedDecision != null)
		{
			ActionName = GameTexts.FindText("str_resolve").ToString();
			ActionInfluenceCost = 0;
			IsActionEnabled = GetActionStatusForDiplomacyItemWithReason(item, isResolve: true, out var disabledReason);
			ActionHint.HintText = disabledReason;
			ProposeActionExplanationText = GameTexts.FindText("str_resolve_explanation").ToString();
		}
		else
		{
			ActionName = ((_playerKingdom.Clans.Count > 1) ? GameTexts.FindText("str_policy_propose").ToString() : GameTexts.FindText("str_policy_enact").ToString());
			ActionInfluenceCost = Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfProposingWar(Clan.PlayerClan);
			IsActionEnabled = GetActionStatusForDiplomacyItemWithReason(item, isResolve: false, out var disabledReason2);
			ActionHint.HintText = disabledReason2;
			ProposeActionExplanationText = GameTexts.FindText("str_propose_war_explanation").SetTextVariable("SUPPORT", CalculateWarSupport(item.Faction2)).ToString();
		}
	}

	private bool GetActionStatusForDiplomacyItemWithReason(KingdomDiplomacyItemVM item, bool isResolve, out TextObject disabledReason)
	{
		if (!CampaignUIHelper.GetMapScreenActionIsEnabledWithReason(out var disabledReason2))
		{
			disabledReason = disabledReason2;
			return false;
		}
		if (!isResolve && Clan.PlayerClan.Influence < (float)ActionInfluenceCost)
		{
			disabledReason = GameTexts.FindText("str_warning_you_dont_have_enough_influence");
			return false;
		}
		if (Clan.PlayerClan.IsUnderMercenaryService)
		{
			disabledReason = GameTexts.FindText("str_cannot_propose_war_truce_while_mercenary");
			return false;
		}
		TextObject reason2;
		if (item is KingdomTruceItemVM kingdomTruceItemVM)
		{
			if (!Campaign.Current.Models.KingdomDecisionPermissionModel.IsWarDecisionAllowedBetweenKingdoms(kingdomTruceItemVM.Faction1 as Kingdom, kingdomTruceItemVM.Faction2 as Kingdom, out var reason))
			{
				disabledReason = reason;
				return false;
			}
		}
		else if (item is KingdomWarItemVM && !Campaign.Current.Models.KingdomDecisionPermissionModel.IsPeaceDecisionAllowedBetweenKingdoms(item.Faction1 as Kingdom, item.Faction2 as Kingdom, out reason2))
		{
			disabledReason = reason2;
			return false;
		}
		disabledReason = TextObject.Empty;
		return true;
	}

	private void RefreshCurrentWarVisuals(KingdomDiplomacyItemVM item)
	{
		if (item != null)
		{
			if (CurrentSelectedDiplomacyItem != null)
			{
				CurrentSelectedDiplomacyItem.IsSelected = false;
			}
			CurrentSelectedDiplomacyItem = item;
			if (CurrentSelectedDiplomacyItem != null)
			{
				CurrentSelectedDiplomacyItem.IsSelected = true;
			}
		}
	}

	private void OnDiplomacyItemSelection(KingdomDiplomacyItemVM item)
	{
		if (CurrentSelectedDiplomacyItem != item)
		{
			if (CurrentSelectedDiplomacyItem != null)
			{
				CurrentSelectedDiplomacyItem.IsSelected = false;
			}
			CurrentSelectedDiplomacyItem = item;
			base.IsAcceptableItemSelected = item != null;
			OnSetCurrentDiplomacyItem(item);
		}
	}

	private void OnDeclareWar(KingdomTruceItemVM item)
	{
		if (_currentItemsUnresolvedDecision != null)
		{
			_forceDecision(_currentItemsUnresolvedDecision);
			return;
		}
		DeclareWarDecision declareWarDecision = new DeclareWarDecision(Clan.PlayerClan, item.Faction2);
		Clan.PlayerClan.Kingdom.AddDecision(declareWarDecision);
		_forceDecision(declareWarDecision);
	}

	private void OnDeclarePeace(KingdomWarItemVM item)
	{
		if (_currentItemsUnresolvedDecision != null)
		{
			_forceDecision(_currentItemsUnresolvedDecision);
			return;
		}
		MakePeaceKingdomDecision makePeaceKingdomDecision = new MakePeaceKingdomDecision(Clan.PlayerClan, item.Faction2 as Kingdom, _dailyPeaceTributeToPay);
		Clan.PlayerClan.Kingdom.AddDecision(makePeaceKingdomDecision);
		_forceDecision(makePeaceKingdomDecision);
	}

	private void ExecuteAction()
	{
		if (CurrentSelectedDiplomacyItem != null)
		{
			if (CurrentSelectedDiplomacyItem is KingdomWarItemVM)
			{
				OnDeclarePeace(CurrentSelectedDiplomacyItem as KingdomWarItemVM);
			}
			else if (CurrentSelectedDiplomacyItem is KingdomTruceItemVM)
			{
				OnDeclareWar(CurrentSelectedDiplomacyItem as KingdomTruceItemVM);
			}
		}
	}

	private void ExecuteShowWarLogs()
	{
		IsDisplayingWarLogs = true;
		IsDisplayingStatComparisons = false;
	}

	private void ExecuteShowStatComparisons()
	{
		IsDisplayingWarLogs = false;
		IsDisplayingStatComparisons = true;
	}

	private void SetDefaultSelectedItem()
	{
		KingdomDiplomacyItemVM kingdomDiplomacyItemVM = PlayerWars.FirstOrDefault();
		KingdomDiplomacyItemVM kingdomDiplomacyItemVM2 = PlayerTruces.FirstOrDefault();
		OnDiplomacyItemSelection(kingdomDiplomacyItemVM ?? kingdomDiplomacyItemVM2);
	}

	private void UpdateBehaviorSelection()
	{
		if (Hero.MainHero.MapFaction.IsKingdomFaction && Hero.MainHero.MapFaction.Leader == Hero.MainHero && CurrentSelectedDiplomacyItem != null)
		{
			StanceLink stanceWith = Hero.MainHero.MapFaction.GetStanceWith(CurrentSelectedDiplomacyItem.Faction2);
			BehaviorSelection.SelectedIndex = stanceWith.BehaviorPriority;
		}
	}

	private void OnBehaviorSelectionChanged(SelectorVM<SelectorItemVM> s)
	{
		if (!_isChangingDiplomacyItem && Hero.MainHero.MapFaction.IsKingdomFaction && Hero.MainHero.MapFaction.Leader == Hero.MainHero && CurrentSelectedDiplomacyItem != null)
		{
			Hero.MainHero.MapFaction.GetStanceWith(CurrentSelectedDiplomacyItem.Faction2).BehaviorPriority = s.SelectedIndex;
		}
	}

	private static int CalculateWarSupport(IFaction faction)
	{
		return TaleWorlds.Library.MathF.Round(new KingdomElection(new DeclareWarDecision(Clan.PlayerClan, faction)).GetLikelihoodForSponsor(Clan.PlayerClan) * 100f);
	}

	private int CalculatePeaceSupport(KingdomWarItemVM policy, int dailyTributeToBePaid)
	{
		return TaleWorlds.Library.MathF.Round(new KingdomElection(new MakePeaceKingdomDecision(Clan.PlayerClan, policy.Faction2, dailyTributeToBePaid)).GetLikelihoodForSponsor(Clan.PlayerClan) * 100f);
	}
}
