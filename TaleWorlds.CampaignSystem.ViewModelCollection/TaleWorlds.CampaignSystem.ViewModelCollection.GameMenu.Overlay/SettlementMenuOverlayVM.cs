using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.ViewModelCollection.Quests;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Tutorial;
using TaleWorlds.Library;
using TaleWorlds.Library.EventSystem;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;

[MenuOverlay("SettlementMenuOverlay")]
public class SettlementMenuOverlayVM : GameMenuOverlay
{
	public class SettlementOverlayTalkPermissionEvent : EventBase
	{
		public Hero HeroToTalkTo;

		public Action<bool, TextObject> IsTalkAvailable { get; private set; }

		public SettlementOverlayTalkPermissionEvent(Hero heroToTalkTo, Action<bool, TextObject> isTalkAvailable)
		{
			HeroToTalkTo = heroToTalkTo;
			IsTalkAvailable = isTalkAvailable;
		}
	}

	public class SettlementOverylayQuickTalkPermissionEvent : EventBase
	{
		public Action<bool, TextObject> IsTalkAvailable { get; private set; }

		public SettlementOverylayQuickTalkPermissionEvent(Action<bool, TextObject> isTalkAvailable)
		{
			IsTalkAvailable = isTalkAvailable;
		}
	}

	public class SettlementOverlayLeaveCharacterPermissionEvent : EventBase
	{
		public Action<bool, TextObject> IsLeaveAvailable { get; private set; }

		public SettlementOverlayLeaveCharacterPermissionEvent(Action<bool, TextObject> isLeaveAvailable)
		{
			IsLeaveAvailable = isLeaveAvailable;
		}
	}

	public class CrimeValueInspectedInSettlementOverlayEvent : EventBase
	{
	}

	private class CharacterComparer : IComparer<GameMenuPartyItemVM>
	{
		public int Compare(GameMenuPartyItemVM x, GameMenuPartyItemVM y)
		{
			return CampaignUIHelper.GetHeroCompareSortIndex(x.Character.HeroObject, y.Character.HeroObject);
		}
	}

	private class PartyComparer : IComparer<GameMenuPartyItemVM>
	{
		public int Compare(GameMenuPartyItemVM x, GameMenuPartyItemVM y)
		{
			return CampaignUIHelper.MobilePartyPrecedenceComparerInstance.Compare(x.Party.MobileParty, y.Party.MobileParty);
		}
	}

	private readonly Settlement _settlement;

	private GameOverlays.MenuOverlayType _type;

	private GameMenuOverlayActionVM _overlayTalkItem;

	private GameMenuOverlayActionVM _overlayQuickTalkItem;

	private Tuple<bool, TextObject> _mostRecentOverlayTalkPermission;

	private Tuple<bool, TextObject> _mostRecentOverlayQuickTalkPermission;

	private Tuple<bool, TextObject> _mostRecentOverlayLeaveCharacterPermission;

	private string _latestTutorialElementID;

	private bool _isCompanionHighlightApplied;

	private bool _isQuestGiversHighlightApplied;

	private bool _isNotableHighlightApplied;

	private bool _isTalkItemHighlightApplied;

	private string _militasLbl;

	private string _garrisonLbl;

	private bool _isNoGarrisonWarning;

	private bool _isLoyaltyRebellionWarning;

	private bool _isCrimeLabelHighlightEnabled;

	private string _crimeLbl;

	private string _prosperityLbl;

	private string _loyaltyLbl;

	private string _securityLbl;

	private string _wallsLbl;

	private string _settlementNameLbl;

	private string _remainingFoodText = "";

	private int _wallsLevel;

	private int _prosperityChangeAmount;

	private int _militiaChangeAmount;

	private int _garrisonChangeAmount;

	private int _garrisonAmount;

	private int _crimeChangeAmount;

	private int _loyaltyChangeAmount;

	private int _securityChangeAmount;

	private int _foodChangeAmount;

	private MBBindingList<GameMenuPartyItemVM> _characterList;

	private MBBindingList<GameMenuPartyItemVM> _partyList;

	private MBBindingList<StringItemWithHintVM> _issueList;

	private bool _isFortification;

	private bool _isCrimeEnabled;

	private bool _canLeaveMembers;

	private BasicTooltipViewModel _remainingFoodHint;

	private BasicTooltipViewModel _militasHint;

	private BasicTooltipViewModel _garrisonHint;

	private BasicTooltipViewModel _prosperityHint;

	private BasicTooltipViewModel _loyaltyHint;

	private BasicTooltipViewModel _securityHint;

	private BasicTooltipViewModel _wallsHint;

	private BasicTooltipViewModel _crimeHint;

	private HintViewModel _characterFilterHint;

	private HintViewModel _partyFilterHint;

	private HintViewModel _leaveMembersHint;

	private ImageIdentifierVM _settlementOwnerBanner;

	[DataSourceProperty]
	public string RemainingFoodText
	{
		get
		{
			return _remainingFoodText;
		}
		set
		{
			if (value != _remainingFoodText)
			{
				_remainingFoodText = value;
				OnPropertyChangedWithValue(value, "RemainingFoodText");
			}
		}
	}

	[DataSourceProperty]
	public int ProsperityChangeAmount
	{
		get
		{
			return _prosperityChangeAmount;
		}
		set
		{
			if (value != _prosperityChangeAmount)
			{
				_prosperityChangeAmount = value;
				OnPropertyChangedWithValue(value, "ProsperityChangeAmount");
			}
		}
	}

	[DataSourceProperty]
	public int MilitiaChangeAmount
	{
		get
		{
			return _militiaChangeAmount;
		}
		set
		{
			if (value != _militiaChangeAmount)
			{
				_militiaChangeAmount = value;
				OnPropertyChangedWithValue(value, "MilitiaChangeAmount");
			}
		}
	}

	[DataSourceProperty]
	public int GarrisonChangeAmount
	{
		get
		{
			return _garrisonChangeAmount;
		}
		set
		{
			if (value != _garrisonChangeAmount)
			{
				_garrisonChangeAmount = value;
				OnPropertyChangedWithValue(value, "GarrisonChangeAmount");
			}
		}
	}

	[DataSourceProperty]
	public int GarrisonAmount
	{
		get
		{
			return _garrisonAmount;
		}
		set
		{
			if (value != _garrisonAmount)
			{
				_garrisonAmount = value;
				OnPropertyChangedWithValue(value, "GarrisonAmount");
			}
		}
	}

	[DataSourceProperty]
	public int CrimeChangeAmount
	{
		get
		{
			return _crimeChangeAmount;
		}
		set
		{
			if (value != _crimeChangeAmount)
			{
				_crimeChangeAmount = value;
				OnPropertyChangedWithValue(value, "CrimeChangeAmount");
			}
		}
	}

	[DataSourceProperty]
	public int LoyaltyChangeAmount
	{
		get
		{
			return _loyaltyChangeAmount;
		}
		set
		{
			if (value != _loyaltyChangeAmount)
			{
				_loyaltyChangeAmount = value;
				OnPropertyChangedWithValue(value, "LoyaltyChangeAmount");
			}
		}
	}

	[DataSourceProperty]
	public int SecurityChangeAmount
	{
		get
		{
			return _securityChangeAmount;
		}
		set
		{
			if (value != _securityChangeAmount)
			{
				_securityChangeAmount = value;
				OnPropertyChangedWithValue(value, "SecurityChangeAmount");
			}
		}
	}

	[DataSourceProperty]
	public int FoodChangeAmount
	{
		get
		{
			return _foodChangeAmount;
		}
		set
		{
			if (value != _foodChangeAmount)
			{
				_foodChangeAmount = value;
				OnPropertyChangedWithValue(value, "FoodChangeAmount");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel RemainingFoodHint
	{
		get
		{
			return _remainingFoodHint;
		}
		set
		{
			if (value != _remainingFoodHint)
			{
				_remainingFoodHint = value;
				OnPropertyChangedWithValue(value, "RemainingFoodHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel SecurityHint
	{
		get
		{
			return _securityHint;
		}
		set
		{
			if (value != _securityHint)
			{
				_securityHint = value;
				OnPropertyChangedWithValue(value, "SecurityHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel PartyFilterHint
	{
		get
		{
			return _partyFilterHint;
		}
		set
		{
			if (value != _partyFilterHint)
			{
				_partyFilterHint = value;
				OnPropertyChangedWithValue(value, "PartyFilterHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CharacterFilterHint
	{
		get
		{
			return _characterFilterHint;
		}
		set
		{
			if (value != _characterFilterHint)
			{
				_characterFilterHint = value;
				OnPropertyChangedWithValue(value, "CharacterFilterHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel MilitasHint
	{
		get
		{
			return _militasHint;
		}
		set
		{
			if (value != _militasHint)
			{
				_militasHint = value;
				OnPropertyChangedWithValue(value, "MilitasHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel GarrisonHint
	{
		get
		{
			return _garrisonHint;
		}
		set
		{
			if (value != _garrisonHint)
			{
				_garrisonHint = value;
				OnPropertyChangedWithValue(value, "GarrisonHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel ProsperityHint
	{
		get
		{
			return _prosperityHint;
		}
		set
		{
			if (value != _prosperityHint)
			{
				_prosperityHint = value;
				OnPropertyChangedWithValue(value, "ProsperityHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel LoyaltyHint
	{
		get
		{
			return _loyaltyHint;
		}
		set
		{
			if (value != _loyaltyHint)
			{
				_loyaltyHint = value;
				OnPropertyChangedWithValue(value, "LoyaltyHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel WallsHint
	{
		get
		{
			return _wallsHint;
		}
		set
		{
			if (value != _wallsHint)
			{
				_wallsHint = value;
				OnPropertyChangedWithValue(value, "WallsHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel CrimeHint
	{
		get
		{
			return _crimeHint;
		}
		set
		{
			if (value != _crimeHint)
			{
				_crimeHint = value;
				OnPropertyChangedWithValue(value, "CrimeHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel LeaveMembersHint
	{
		get
		{
			return _leaveMembersHint;
		}
		set
		{
			if (value != _leaveMembersHint)
			{
				_leaveMembersHint = value;
				OnPropertyChangedWithValue(value, "LeaveMembersHint");
			}
		}
	}

	[DataSourceProperty]
	public ImageIdentifierVM SettlementOwnerBanner
	{
		get
		{
			return _settlementOwnerBanner;
		}
		set
		{
			if (value != _settlementOwnerBanner)
			{
				_settlementOwnerBanner = value;
				OnPropertyChangedWithValue(value, "SettlementOwnerBanner");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<GameMenuPartyItemVM> CharacterList
	{
		get
		{
			return _characterList;
		}
		set
		{
			if (value != _characterList)
			{
				_characterList = value;
				OnPropertyChangedWithValue(value, "CharacterList");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<GameMenuPartyItemVM> PartyList
	{
		get
		{
			return _partyList;
		}
		set
		{
			if (value != _partyList)
			{
				_partyList = value;
				OnPropertyChangedWithValue(value, "PartyList");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringItemWithHintVM> IssueList
	{
		get
		{
			return _issueList;
		}
		set
		{
			if (value != _issueList)
			{
				_issueList = value;
				OnPropertyChangedWithValue(value, "IssueList");
			}
		}
	}

	[DataSourceProperty]
	public string MilitasLbl
	{
		get
		{
			return _militasLbl;
		}
		set
		{
			if (value != _militasLbl)
			{
				_militasLbl = value;
				OnPropertyChangedWithValue(value, "MilitasLbl");
			}
		}
	}

	[DataSourceProperty]
	public string GarrisonLbl
	{
		get
		{
			return _garrisonLbl;
		}
		set
		{
			if (value != _garrisonLbl)
			{
				_garrisonLbl = value;
				OnPropertyChangedWithValue(value, "GarrisonLbl");
			}
		}
	}

	[DataSourceProperty]
	public string CrimeLbl
	{
		get
		{
			return _crimeLbl;
		}
		set
		{
			if (value != _crimeLbl)
			{
				_crimeLbl = value;
				OnPropertyChangedWithValue(value, "CrimeLbl");
			}
		}
	}

	[DataSourceProperty]
	public bool CanLeaveMembers
	{
		get
		{
			return _canLeaveMembers;
		}
		set
		{
			if (value != _canLeaveMembers)
			{
				_canLeaveMembers = value;
				OnPropertyChangedWithValue(value, "CanLeaveMembers");
			}
		}
	}

	[DataSourceProperty]
	public string ProsperityLbl
	{
		get
		{
			return _prosperityLbl;
		}
		set
		{
			if (value != _prosperityLbl)
			{
				_prosperityLbl = value;
				OnPropertyChangedWithValue(value, "ProsperityLbl");
			}
		}
	}

	[DataSourceProperty]
	public string LoyaltyLbl
	{
		get
		{
			return _loyaltyLbl;
		}
		set
		{
			if (value != _loyaltyLbl)
			{
				_loyaltyLbl = value;
				OnPropertyChangedWithValue(value, "LoyaltyLbl");
			}
		}
	}

	[DataSourceProperty]
	public string SecurityLbl
	{
		get
		{
			return _securityLbl;
		}
		set
		{
			if (value != _securityLbl)
			{
				_securityLbl = value;
				OnPropertyChangedWithValue(value, "SecurityLbl");
			}
		}
	}

	[DataSourceProperty]
	public string WallsLbl
	{
		get
		{
			return _wallsLbl;
		}
		set
		{
			if (value != _wallsLbl)
			{
				_wallsLbl = value;
				OnPropertyChangedWithValue(value, "WallsLbl");
			}
		}
	}

	[DataSourceProperty]
	public int WallsLevel
	{
		get
		{
			return _wallsLevel;
		}
		set
		{
			if (value != _wallsLevel)
			{
				_wallsLevel = value;
				OnPropertyChangedWithValue(value, "WallsLevel");
			}
		}
	}

	[DataSourceProperty]
	public string SettlementNameLbl
	{
		get
		{
			return _settlementNameLbl;
		}
		set
		{
			if (value != _settlementNameLbl)
			{
				_settlementNameLbl = value;
				OnPropertyChangedWithValue(value, "SettlementNameLbl");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFortification
	{
		get
		{
			return _isFortification;
		}
		set
		{
			if (value != _isFortification)
			{
				_isFortification = value;
				OnPropertyChangedWithValue(value, "IsFortification");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCrimeEnabled
	{
		get
		{
			return _isCrimeEnabled;
		}
		set
		{
			if (value != _isCrimeEnabled)
			{
				_isCrimeEnabled = value;
				OnPropertyChangedWithValue(value, "IsCrimeEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsNoGarrisonWarning
	{
		get
		{
			return _isNoGarrisonWarning;
		}
		set
		{
			if (value != _isNoGarrisonWarning)
			{
				_isNoGarrisonWarning = value;
				OnPropertyChangedWithValue(value, "IsNoGarrisonWarning");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCrimeLabelHighlightEnabled
	{
		get
		{
			return _isCrimeLabelHighlightEnabled;
		}
		set
		{
			if (value != _isCrimeLabelHighlightEnabled)
			{
				_isCrimeLabelHighlightEnabled = value;
				OnPropertyChangedWithValue(value, "IsCrimeLabelHighlightEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsLoyaltyRebellionWarning
	{
		get
		{
			return _isLoyaltyRebellionWarning;
		}
		set
		{
			if (value != _isLoyaltyRebellionWarning)
			{
				_isLoyaltyRebellionWarning = value;
				OnPropertyChangedWithValue(value, "IsLoyaltyRebellionWarning");
			}
		}
	}

	public SettlementMenuOverlayVM(GameOverlays.MenuOverlayType type)
	{
		_type = type;
		_overlayTalkItem = null;
		base.IsInitializationOver = false;
		_settlement = Settlement.CurrentSettlement;
		CharacterList = new MBBindingList<GameMenuPartyItemVM>();
		PartyList = new MBBindingList<GameMenuPartyItemVM>();
		IssueList = new MBBindingList<StringItemWithHintVM>();
		base.CurrentOverlayType = 0;
		CrimeHint = new BasicTooltipViewModel(() => GetCrimeTooltip());
		if (Settlement.CurrentSettlement.IsFortification)
		{
			RemainingFoodHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownFoodTooltip(_settlement.Town));
			LoyaltyHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownLoyaltyTooltip(_settlement.Town));
			MilitasHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownMilitiaTooltip(_settlement.Town));
			ProsperityHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownProsperityTooltip(_settlement.Town));
			WallsHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownWallsTooltip(_settlement.Town));
			GarrisonHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownGarrisonTooltip(_settlement.Town));
			SecurityHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownSecurityTooltip(_settlement.Town));
		}
		else
		{
			MilitasHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetVillageMilitiaTooltip(_settlement.Village));
			LoyaltyHint = new BasicTooltipViewModel();
			WallsHint = new BasicTooltipViewModel();
			ProsperityHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetVillageProsperityTooltip(_settlement.Village));
		}
		UpdateSettlementOwnerBanner();
		_contextMenuItem = null;
		base.IsInitializationOver = true;
		CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
		CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
		CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener(this, OnQuestCompleted);
		CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
		CampaignEvents.MakePeace.AddNonSerializedListener(this, OnPeaceDeclared);
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
		CampaignEvents.TownRebelliosStateChanged.AddNonSerializedListener(this, OnTownRebelliousStateChanged);
		Game.Current.EventManager.RegisterEvent<TutorialNotificationElementChangeEvent>(OnTutorialNotificationElementIDChange);
		RefreshValues();
	}

	private List<TooltipProperty> GetCrimeTooltip()
	{
		Game.Current?.EventManager.TriggerEvent(new CrimeValueInspectedInSettlementOverlayEvent());
		return CampaignUIHelper.GetCrimeTooltip(_settlement);
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		PartyFilterHint = new HintViewModel(GameTexts.FindText("str_parties"));
		CharacterFilterHint = new HintViewModel(GameTexts.FindText("str_characters"));
		Refresh();
	}

	protected override void ExecuteOnSetAsActiveContextMenuItem(GameMenuPartyItemVM troop)
	{
		base.ExecuteOnSetAsActiveContextMenuItem(troop);
		base.ContextList.Clear();
		IssueList.Clear();
		if (_contextMenuItem.Character != null && (!_contextMenuItem.Character.IsHero || !_contextMenuItem.Character.HeroObject.IsPrisoner))
		{
			bool isEnabled = true;
			TextObject hint = TextObject.Empty;
			_mostRecentOverlayTalkPermission = null;
			Game.Current.EventManager.TriggerEvent(new SettlementOverlayTalkPermissionEvent(_contextMenuItem.Character.HeroObject, OnSettlementOverlayTalkPermissionResult));
			if (_mostRecentOverlayTalkPermission != null)
			{
				isEnabled = _mostRecentOverlayTalkPermission.Item1;
				hint = _mostRecentOverlayTalkPermission.Item2;
			}
			_overlayTalkItem = new GameMenuOverlayActionVM(base.ExecuteTroopAction, GameTexts.FindText("str_menu_overlay_context_list", "Conversation").ToString(), isEnabled, MenuOverlayContextList.Conversation, hint);
			base.ContextList.Add(_overlayTalkItem);
			bool isEnabled2 = true;
			TextObject hint2 = TextObject.Empty;
			_mostRecentOverlayQuickTalkPermission = null;
			Game.Current.EventManager.TriggerEvent(new SettlementOverylayQuickTalkPermissionEvent(OnSettlementOverlayQuickTalkPermissionResult));
			if (_mostRecentOverlayQuickTalkPermission != null)
			{
				isEnabled2 = _mostRecentOverlayQuickTalkPermission.Item1;
				hint2 = _mostRecentOverlayQuickTalkPermission.Item2;
			}
			_overlayQuickTalkItem = new GameMenuOverlayActionVM(base.ExecuteTroopAction, GameTexts.FindText("str_menu_overlay_context_list", "QuickConversation").ToString(), isEnabled2, MenuOverlayContextList.QuickConversation, hint2);
			base.ContextList.Add(_overlayQuickTalkItem);
			foreach (QuestMarkerVM quest in troop.Quests)
			{
				if (quest.IssueQuestFlag != 0)
				{
					GameTexts.SetVariable("STR2", quest.QuestTitle);
					string content = string.Empty;
					if (quest.IssueQuestFlag == CampaignUIHelper.IssueQuestFlags.ActiveIssue)
					{
						content = "{=!}<img src=\"General\\Icons\\icon_issue_active_square\" extend=\"4\">";
					}
					else if (quest.IssueQuestFlag == CampaignUIHelper.IssueQuestFlags.AvailableIssue)
					{
						content = "{=!}<img src=\"General\\Icons\\icon_issue_available_square\" extend=\"4\">";
					}
					else if (quest.IssueQuestFlag == CampaignUIHelper.IssueQuestFlags.ActiveStoryQuest)
					{
						content = "{=!}<img src=\"General\\Icons\\icon_story_quest_active_square\" extend=\"4\">";
					}
					else if (quest.IssueQuestFlag == CampaignUIHelper.IssueQuestFlags.TrackedIssue)
					{
						content = "{=!}<img src=\"General\\Icons\\issue_target_icon\" extend=\"4\">";
					}
					else if (quest.IssueQuestFlag == CampaignUIHelper.IssueQuestFlags.TrackedStoryQuest)
					{
						content = "{=!}<img src=\"General\\Icons\\quest_target_icon\" extend=\"4\">";
					}
					GameTexts.SetVariable("STR1", content);
					string text = GameTexts.FindText("str_STR1_STR2").ToString();
					IssueList.Add(new StringItemWithHintVM(text, quest.QuestHint.HintText));
				}
			}
			if (_contextMenuItem.Character.IsHero && _contextMenuItem.Character.HeroObject.PartyBelongedTo?.Army != null && _contextMenuItem.Character.HeroObject.PartyBelongedTo.Army.LeaderParty == _contextMenuItem.Character.HeroObject.PartyBelongedTo && MobileParty.MainParty.Army == null && FactionManager.IsAlliedWithFaction(_contextMenuItem.Character.HeroObject.MapFaction, Hero.MainHero.MapFaction))
			{
				GameMenuOverlayActionVM item = new GameMenuOverlayActionVM(base.ExecuteTroopAction, GameTexts.FindText("str_menu_overlay_context_list", "JoinArmy").ToString(), isEnabled: true, MenuOverlayContextList.JoinArmy);
				base.ContextList.Add(item);
			}
			if (_contextMenuItem.Character.IsHero && _contextMenuItem.Character.HeroObject.PartyBelongedTo == null && _contextMenuItem.Character.HeroObject.Clan == Clan.PlayerClan && _contextMenuItem.Character.HeroObject.Age > (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && !Campaign.Current.GetCampaignBehavior<IAlleyCampaignBehavior>().IsHeroAlleyLeaderOfAnyPlayerAlley(_contextMenuItem.Character.HeroObject))
			{
				GameMenuOverlayActionVM item2 = new GameMenuOverlayActionVM(base.ExecuteTroopAction, GameTexts.FindText("str_menu_overlay_context_list", "TakeToParty").ToString(), isEnabled: true, MenuOverlayContextList.TakeToParty);
				base.ContextList.Add(item2);
			}
			CampaignEventDispatcher.Instance.OnCharacterPortraitPopUpOpened(_contextMenuItem.Character);
			return;
		}
		if (_contextMenuItem.Party == null)
		{
			return;
		}
		if (_contextMenuItem.Party.Owner?.Clan == Hero.MainHero.Clan)
		{
			MobileParty mobileParty = _contextMenuItem.Party.MobileParty;
			if (mobileParty != null && !mobileParty.IsMainParty)
			{
				MobileParty mobileParty2 = _contextMenuItem.Party.MobileParty;
				if (mobileParty2 != null && mobileParty2.IsGarrison)
				{
					_overlayTalkItem = new GameMenuOverlayActionVM(base.ExecuteTroopAction, GameTexts.FindText("str_menu_overlay_context_list", "ManageGarrison").ToString(), isEnabled: true, MenuOverlayContextList.ManageGarrison);
					base.ContextList.Add(_overlayTalkItem);
					goto IL_065b;
				}
			}
		}
		if (_contextMenuItem.Party.MapFaction == Hero.MainHero.MapFaction)
		{
			MobileParty mobileParty3 = _contextMenuItem.Party.MobileParty;
			if (mobileParty3 != null && !mobileParty3.IsMainParty && (_contextMenuItem.Party.MobileParty == null || (!_contextMenuItem.Party.MobileParty.IsVillager && !_contextMenuItem.Party.MobileParty.IsCaravan && !_contextMenuItem.Party.MobileParty.IsMilitia)))
			{
				if (_contextMenuItem.Party.MobileParty.ActualClan == Clan.PlayerClan)
				{
					_overlayTalkItem = new GameMenuOverlayActionVM(base.ExecuteTroopAction, GameTexts.FindText("str_menu_overlay_context_list", "ManageTroops").ToString(), isEnabled: true, MenuOverlayContextList.ManageTroops);
					base.ContextList.Add(_overlayTalkItem);
				}
				else
				{
					_overlayTalkItem = new GameMenuOverlayActionVM(base.ExecuteTroopAction, GameTexts.FindText("str_menu_overlay_context_list", "DonateTroops").ToString(), isEnabled: true, MenuOverlayContextList.DonateTroops);
					base.ContextList.Add(_overlayTalkItem);
				}
			}
		}
		goto IL_065b;
		IL_065b:
		if (_contextMenuItem.Party.LeaderHero != null && _contextMenuItem.Party.LeaderHero != Hero.MainHero)
		{
			bool flag = CharacterList.Any((GameMenuPartyItemVM c) => c.Character == _contextMenuItem.Party.LeaderHero.CharacterObject);
			TextObject hintText = ((!flag) ? GameTexts.FindText("str_menu_overlay_cant_talk_to_party_leader") : TextObject.Empty);
			base.ContextList.Add(new StringItemWithEnabledAndHintVM(base.ExecuteTroopAction, GameTexts.FindText("str_menu_overlay_context_list", "ConverseWithLeader").ToString(), flag, MenuOverlayContextList.ConverseWithLeader, hintText));
		}
		CharacterObject visualPartyLeader = CampaignUIHelper.GetVisualPartyLeader(_contextMenuItem.Party);
		if (visualPartyLeader != null)
		{
			CampaignEventDispatcher.Instance.OnCharacterPortraitPopUpOpened(visualPartyLeader);
		}
	}

	private void OnSettlementOverlayTalkPermissionResult(bool isAvailable, TextObject reasonStr)
	{
		_mostRecentOverlayTalkPermission = new Tuple<bool, TextObject>(isAvailable, reasonStr);
	}

	private void OnSettlementOverlayQuickTalkPermissionResult(bool isAvailable, TextObject reasonStr)
	{
		_mostRecentOverlayQuickTalkPermission = new Tuple<bool, TextObject>(isAvailable, reasonStr);
	}

	private void OnSettlementOverlayLeaveCharacterPermissionResult(bool isAvailable, TextObject reasonStr)
	{
		_mostRecentOverlayLeaveCharacterPermission = new Tuple<bool, TextObject>(isAvailable, reasonStr);
	}

	public override void ExecuteOnOverlayClosed()
	{
		base.ExecuteOnOverlayClosed();
		InitLists();
		base.ContextList.Clear();
	}

	private void ExecuteCloseTooltip()
	{
		MBInformationManager.HideInformations();
	}

	private void ExecuteOpenTooltip()
	{
		InformationManager.ShowTooltip(typeof(Settlement), _settlement, true);
	}

	private void ExecuteSettlementLink()
	{
		Campaign.Current.EncyclopediaManager.GoToLink(_settlement.EncyclopediaLink);
	}

	private bool Contains(MBBindingList<GameMenuPartyItemVM> list, CharacterObject character)
	{
		foreach (GameMenuPartyItemVM item in list)
		{
			if (item.Character == character)
			{
				return true;
			}
		}
		return false;
	}

	public override void UpdateOverlayType(GameOverlays.MenuOverlayType newType)
	{
		_type = newType;
		base.UpdateOverlayType(newType);
	}

	private void InitLists()
	{
		UpdateCharacterList();
		UpdatePartyList();
	}

	private void UpdateCharacterList()
	{
		if (_type == GameOverlays.MenuOverlayType.SettlementWithCharacters || _type == GameOverlays.MenuOverlayType.SettlementWithBoth)
		{
			Dictionary<Hero, bool> dictionary = new Dictionary<Hero, bool>();
			foreach (LocationCharacter item2 in Campaign.Current.GameMenuManager.MenuLocations.SelectMany((Location l) => l.GetCharacterList()))
			{
				if (WillBeListed(item2) && !dictionary.ContainsKey(item2.Character.HeroObject))
				{
					dictionary.Add(item2.Character.HeroObject, item2.UseCivilianEquipment);
				}
			}
			for (int num = CharacterList.Count - 1; num >= 0; num--)
			{
				GameMenuPartyItemVM gameMenuPartyItemVM = CharacterList[num];
				if (!dictionary.ContainsKey(gameMenuPartyItemVM.Character.HeroObject))
				{
					CharacterList.RemoveAt(num);
				}
			}
			foreach (KeyValuePair<Hero, bool> heroKvp in dictionary)
			{
				if (!CharacterList.Any((GameMenuPartyItemVM x) => x.Character == heroKvp.Key.CharacterObject))
				{
					GameMenuPartyItemVM item = new GameMenuPartyItemVM(ExecuteOnSetAsActiveContextMenuItem, heroKvp.Key.CharacterObject, heroKvp.Value);
					CharacterList.Add(item);
				}
			}
			CharacterList.Sort(new CharacterComparer());
		}
		else
		{
			CharacterList.Clear();
		}
	}

	private void UpdatePartyList()
	{
		if (_type == GameOverlays.MenuOverlayType.SettlementWithBoth || _type == GameOverlays.MenuOverlayType.SettlementWithParties)
		{
			Settlement obj = MobileParty.MainParty.CurrentSettlement ?? MobileParty.MainParty.LastVisitedSettlement;
			List<MobileParty> partiesInSettlement = new List<MobileParty>();
			foreach (MobileParty party in obj.Parties)
			{
				if (WillBeListed(party))
				{
					partiesInSettlement.Add(party);
				}
			}
			for (int num = PartyList.Count - 1; num >= 0; num--)
			{
				GameMenuPartyItemVM gameMenuPartyItemVM = PartyList[num];
				if (!partiesInSettlement.Contains(gameMenuPartyItemVM.Party.MobileParty))
				{
					PartyList.RemoveAt(num);
				}
			}
			int i;
			for (i = 0; i < partiesInSettlement.Count; i++)
			{
				if (!PartyList.Any((GameMenuPartyItemVM x) => x.Party == partiesInSettlement[i].Party))
				{
					GameMenuPartyItemVM item = new GameMenuPartyItemVM(ExecuteOnSetAsActiveContextMenuItem, partiesInSettlement[i].Party, canShowQuest: false);
					PartyList.Add(item);
				}
			}
			PartyList.Sort(new PartyComparer());
		}
		else
		{
			PartyList.Clear();
		}
	}

	private void UpdateList<TListItem, TElement>(MBBindingList<TListItem> listToUpdate, IEnumerable<TElement> listInSettlement, IComparer<TListItem> comparer, Func<TListItem, TElement> getElementFromListItem, Func<TElement, bool> doesSettlementHasElement, Func<TElement, TListItem> createListItem)
	{
		HashSet<TElement> hashSet = new HashSet<TElement>();
		for (int i = 0; i < listToUpdate.Count; i++)
		{
			TListItem arg = listToUpdate[i];
			TElement val = getElementFromListItem(arg);
			if (doesSettlementHasElement(val))
			{
				hashSet.Add(val);
				continue;
			}
			listToUpdate.RemoveAt(i);
			i--;
		}
		foreach (TElement item in listInSettlement)
		{
			if (!hashSet.Contains(item))
			{
				listToUpdate.Add(createListItem(item));
				hashSet.Add(item);
			}
		}
		listToUpdate.Sort(comparer);
	}

	private bool WillBeListed(MobileParty mobileParty)
	{
		return mobileParty?.IsActive ?? false;
	}

	private bool WillBeListed(LocationCharacter locationCharacter)
	{
		Settlement settlement = ((MobileParty.MainParty.CurrentSettlement != null) ? MobileParty.MainParty.CurrentSettlement : MobileParty.MainParty.LastVisitedSettlement);
		if (locationCharacter.Character.IsHero && !locationCharacter.IsHidden && locationCharacter.Character.HeroObject.PartyBelongedTo != MobileParty.MainParty)
		{
			return locationCharacter.Character.HeroObject.CurrentSettlement == settlement;
		}
		return false;
	}

	private bool WillBeListed(CharacterObject character)
	{
		Settlement settlement = ((MobileParty.MainParty.CurrentSettlement != null) ? MobileParty.MainParty.CurrentSettlement : MobileParty.MainParty.LastVisitedSettlement);
		if (character.IsHero && character.HeroObject.PartyBelongedTo != MobileParty.MainParty)
		{
			return character.HeroObject.CurrentSettlement == settlement;
		}
		return false;
	}

	private void UpdateSettlementOwnerBanner()
	{
		Banner banner = null;
		IFaction mapFaction = _settlement.MapFaction;
		if (mapFaction != null && mapFaction.IsKingdomFaction && ((Kingdom)_settlement.MapFaction).RulingClan == _settlement.OwnerClan)
		{
			banner = _settlement.OwnerClan.Kingdom.Banner;
		}
		else if (_settlement.OwnerClan?.Banner != null)
		{
			banner = _settlement.OwnerClan.Banner;
		}
		if (banner != null)
		{
			SettlementOwnerBanner = new ImageIdentifierVM(BannerCode.CreateFrom(banner), nineGrid: true);
		}
		else
		{
			SettlementOwnerBanner = new ImageIdentifierVM();
		}
	}

	private void UpdateProperties()
	{
		Settlement currentSettlement = ((MobileParty.MainParty.CurrentSettlement != null) ? MobileParty.MainParty.CurrentSettlement : MobileParty.MainParty.LastVisitedSettlement);
		IsFortification = currentSettlement.IsFortification;
		IFaction mapFaction = currentSettlement.MapFaction;
		IsCrimeEnabled = mapFaction != null && mapFaction.MainHeroCrimeRating > 0f;
		CrimeLbl = ((int)(currentSettlement.MapFaction?.MainHeroCrimeRating).Value).ToString();
		CrimeChangeAmount = (int)(currentSettlement.MapFaction?.DailyCrimeRatingChange).Value;
		RemainingFoodText = (currentSettlement.IsFortification ? ((int)currentSettlement.Town.FoodStocks).ToString() : "-");
		FoodChangeAmount = ((currentSettlement.Town != null) ? ((int)currentSettlement.Town.FoodChange) : 0);
		MilitasLbl = ((int)currentSettlement.Militia).ToString();
		MilitiaChangeAmount = ((currentSettlement.Town != null) ? ((int)currentSettlement.Town.MilitiaChange) : ((int)currentSettlement.Village.MilitiaChange));
		IsLoyaltyRebellionWarning = currentSettlement.IsTown && currentSettlement.Town.Loyalty < (float)Campaign.Current.Models.SettlementLoyaltyModel.RebelliousStateStartLoyaltyThreshold;
		if (currentSettlement.IsFortification)
		{
			GarrisonAmount = currentSettlement.Town.GarrisonParty?.Party.NumberOfAllMembers ?? 0;
			IsNoGarrisonWarning = GarrisonAmount < 1;
		}
		if (currentSettlement.IsFortification)
		{
			GarrisonLbl = currentSettlement.Town.GarrisonParty?.Party.NumberOfAllMembers.ToString() ?? "0";
			GarrisonChangeAmount = currentSettlement.Town.GarrisonChange;
			WallsLbl = currentSettlement.Town.GetWallLevel().ToString();
			WallsLevel = currentSettlement.Town.GetWallLevel();
		}
		else
		{
			GarrisonChangeAmount = 0;
			WallsLbl = "-";
			GarrisonLbl = "-";
			WallsLevel = 1;
		}
		if (currentSettlement.IsFortification)
		{
			ProsperityLbl = ((int)currentSettlement.Town.Prosperity).ToString();
			ProsperityChangeAmount = (int)currentSettlement.Town.ProsperityChange;
		}
		else
		{
			ProsperityLbl = ((int)currentSettlement.Village.Hearth).ToString();
			ProsperityChangeAmount = (int)currentSettlement.Village.HearthChange;
		}
		SettlementNameLbl = string.Concat(currentSettlement.Name, (currentSettlement.IsVillage && currentSettlement.Village.VillageState != 0) ? ("(" + currentSettlement.Village.VillageState.ToString() + ")") : "");
		if (currentSettlement.IsFortification)
		{
			LoyaltyLbl = ((int)currentSettlement.Town.Loyalty).ToString();
			LoyaltyChangeAmount = (int)currentSettlement.Town.LoyaltyChange;
			SecurityLbl = ((int)currentSettlement.Town.Security).ToString();
			SecurityChangeAmount = (int)currentSettlement.Town.SecurityChange;
		}
		else
		{
			LoyaltyChangeAmount = 0;
			LoyaltyLbl = "-";
			SecurityChangeAmount = 0;
			SecurityLbl = "-";
		}
		Game.Current.EventManager.TriggerEvent(new SettlementOverlayLeaveCharacterPermissionEvent(OnSettlementOverlayLeaveCharacterPermissionResult));
		if (_mostRecentOverlayLeaveCharacterPermission != null)
		{
			CanLeaveMembers = _mostRecentOverlayLeaveCharacterPermission.Item1;
			LeaveMembersHint = (CanLeaveMembers ? new HintViewModel(new TextObject("{=aGFxIvqx}Leave Member(s)")) : new HintViewModel(_mostRecentOverlayLeaveCharacterPermission.Item2));
			return;
		}
		CanLeaveMembers = Clan.PlayerClan.Heroes.Any((Hero hero) => currentSettlement == hero.StayingInSettlement || (!hero.CharacterObject.IsPlayerCharacter && MobileParty.MainParty.MemberRoster.Contains(hero.CharacterObject)));
		if (!CanLeaveMembers)
		{
			LeaveMembersHint = new HintViewModel(new TextObject("{=d2K6gMsZ}Leave members. Need at least 1 companion."));
		}
		else
		{
			LeaveMembersHint = new HintViewModel(new TextObject("{=aGFxIvqx}Leave Member(s)"));
		}
	}

	private void OnTutorialNotificationElementIDChange(TutorialNotificationElementChangeEvent obj)
	{
		_latestTutorialElementID = obj.NewNotificationElementID;
		if (_latestTutorialElementID != null)
		{
			if (_latestTutorialElementID != "")
			{
				if (_latestTutorialElementID == "ApplicapleCompanion" && !_isCompanionHighlightApplied)
				{
					_isCompanionHighlightApplied = SetPartyItemHighlightState(_latestTutorialElementID, state: true);
				}
				else if (_latestTutorialElementID != "ApplicapleCompanion" && _isCompanionHighlightApplied)
				{
					_isCompanionHighlightApplied = SetPartyItemHighlightState("ApplicapleCompanion", state: false);
				}
				if (_latestTutorialElementID == "ApplicableQuestGivers" && !_isQuestGiversHighlightApplied)
				{
					_isQuestGiversHighlightApplied = SetPartyItemHighlightState(_latestTutorialElementID, state: true);
				}
				else if (_latestTutorialElementID != "ApplicableQuestGivers" && _isQuestGiversHighlightApplied)
				{
					_isCompanionHighlightApplied = SetPartyItemHighlightState("ApplicableQuestGivers", state: false);
				}
				if (_latestTutorialElementID == "ApplicableNotable" && !_isNotableHighlightApplied)
				{
					_isNotableHighlightApplied = SetPartyItemHighlightState(_latestTutorialElementID, state: true);
				}
				else if (_latestTutorialElementID != "ApplicableNotable" && _isNotableHighlightApplied)
				{
					_isNotableHighlightApplied = SetPartyItemHighlightState("ApplicableNotable", state: false);
				}
				if (_latestTutorialElementID == "CrimeLabel" && !IsCrimeLabelHighlightEnabled)
				{
					IsCrimeLabelHighlightEnabled = true;
				}
				else if (_latestTutorialElementID != "CrimeLabel" && IsCrimeLabelHighlightEnabled)
				{
					IsCrimeLabelHighlightEnabled = false;
				}
				if (_latestTutorialElementID == "OverlayTalkButton" && !_isTalkItemHighlightApplied)
				{
					if (_overlayTalkItem != null)
					{
						_overlayTalkItem.IsHiglightEnabled = true;
						_isTalkItemHighlightApplied = true;
					}
				}
				else if (_latestTutorialElementID != "OverlayTalkButton" && _isTalkItemHighlightApplied && _overlayTalkItem != null)
				{
					_overlayTalkItem.IsHiglightEnabled = false;
					_isTalkItemHighlightApplied = true;
				}
			}
			else
			{
				if (_isCompanionHighlightApplied)
				{
					_isCompanionHighlightApplied = !SetPartyItemHighlightState("ApplicapleCompanion", state: false);
				}
				if (_isNotableHighlightApplied)
				{
					_isNotableHighlightApplied = !SetPartyItemHighlightState("ApplicableNotable", state: false);
				}
				if (_isQuestGiversHighlightApplied)
				{
					_isQuestGiversHighlightApplied = !SetPartyItemHighlightState("ApplicableQuestGivers", state: false);
				}
				if (IsCrimeLabelHighlightEnabled)
				{
					IsCrimeLabelHighlightEnabled = false;
				}
				if (_isTalkItemHighlightApplied && _overlayTalkItem != null)
				{
					_overlayTalkItem.IsHiglightEnabled = false;
					_isTalkItemHighlightApplied = false;
				}
			}
		}
		else
		{
			if (_isCompanionHighlightApplied)
			{
				_isCompanionHighlightApplied = !SetPartyItemHighlightState("ApplicapleCompanion", state: false);
			}
			if (_isNotableHighlightApplied)
			{
				_isNotableHighlightApplied = !SetPartyItemHighlightState("ApplicableNotable", state: false);
			}
			if (_isQuestGiversHighlightApplied)
			{
				_isQuestGiversHighlightApplied = !SetPartyItemHighlightState("ApplicableQuestGivers", state: false);
			}
			if (_isTalkItemHighlightApplied && _overlayTalkItem != null)
			{
				_overlayTalkItem.IsHiglightEnabled = false;
				_isTalkItemHighlightApplied = false;
			}
			if (IsCrimeLabelHighlightEnabled)
			{
				IsCrimeLabelHighlightEnabled = false;
			}
		}
	}

	private bool SetPartyItemHighlightState(string condition, bool state)
	{
		bool result = false;
		foreach (GameMenuPartyItemVM character in CharacterList)
		{
			if (condition == "ApplicapleCompanion" && character.Character.IsHero && character.Character.HeroObject.IsWanderer && !character.Character.HeroObject.IsPlayerCompanion)
			{
				character.IsHighlightEnabled = state;
				result = true;
			}
			else if (condition == "ApplicableNotable" && character.Character.IsHero && character.Character.HeroObject.IsNotable && !character.Character.HeroObject.IsPlayerCompanion)
			{
				character.IsHighlightEnabled = state;
				result = true;
			}
		}
		return result;
	}

	public override void Refresh()
	{
		base.IsInitializationOver = false;
		InitLists();
		UpdateProperties();
		foreach (GameMenuPartyItemVM character in CharacterList)
		{
			character.RefreshProperties();
		}
		foreach (GameMenuPartyItemVM party in PartyList)
		{
			party.RefreshProperties();
		}
		base.IsInitializationOver = true;
		base.Refresh();
	}

	public void ExecuteAddCompanion()
	{
		List<InquiryElement> list = new List<InquiryElement>();
		foreach (TroopRosterElement item in from m in MobileParty.MainParty.MemberRoster.GetTroopRoster()
			where m.Character.IsHero && m.Character.HeroObject.CanMoveToSettlement()
			select m)
		{
			if (!item.Character.IsPlayerCharacter)
			{
				list.Add(new InquiryElement(item.Character.HeroObject, item.Character.Name.ToString(), new ImageIdentifier(CampaignUIHelper.GetCharacterCode(item.Character))));
			}
		}
		MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=aGFxIvqx}Leave Member(s)").ToString(), string.Empty, list, isExitShown: true, 1, 0, new TextObject("{=FBYFcrWo}Leave in settlement").ToString(), new TextObject("{=3CpNUnVl}Cancel").ToString(), OnLeaveMembersInSettlement, OnLeaveMembersInSettlement));
	}

	private void OnLeaveMembersInSettlement(List<InquiryElement> leftMembers)
	{
		Settlement settlement = ((MobileParty.MainParty.CurrentSettlement != null) ? MobileParty.MainParty.CurrentSettlement : MobileParty.MainParty.LastVisitedSettlement);
		foreach (InquiryElement leftMember in leftMembers)
		{
			Hero hero = leftMember.Identifier as Hero;
			PartyBase.MainParty.MemberRoster.RemoveTroop(hero.CharacterObject);
			if (hero.CharacterObject.IsHero && !settlement.HeroesWithoutParty.Contains(hero.CharacterObject.HeroObject))
			{
				EnterSettlementAction.ApplyForCharacterOnly(hero.CharacterObject.HeroObject, settlement);
			}
		}
		if (leftMembers.Count > 0)
		{
			InitLists();
		}
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		CampaignEvents.AfterSettlementEntered.ClearListeners(this);
		CampaignEvents.OnSettlementLeftEvent.ClearListeners(this);
		CampaignEvents.OnQuestCompletedEvent.ClearListeners(this);
		CampaignEvents.WarDeclared.ClearListeners(this);
		CampaignEvents.MakePeace.ClearListeners(this);
		CampaignEvents.OnSettlementOwnerChangedEvent.ClearListeners(this);
		CampaignEvents.TownRebelliosStateChanged.ClearListeners(this);
		Game.Current.EventManager.UnregisterEvent<TutorialNotificationElementChangeEvent>(OnTutorialNotificationElementIDChange);
	}

	private void OnSettlementEntered(MobileParty arg1, Settlement arg2, Hero arg3)
	{
		Settlement settlement = ((MobileParty.MainParty.CurrentSettlement != null) ? MobileParty.MainParty.CurrentSettlement : MobileParty.MainParty.LastVisitedSettlement);
		if (arg2 == settlement)
		{
			InitLists();
		}
	}

	private void OnSettlementLeft(MobileParty arg1, Settlement arg2)
	{
		Settlement settlement = ((MobileParty.MainParty.CurrentSettlement != null) ? MobileParty.MainParty.CurrentSettlement : MobileParty.MainParty.LastVisitedSettlement);
		if (arg2 == settlement)
		{
			InitLists();
		}
	}

	private void OnQuestCompleted(QuestBase arg1, QuestBase.QuestCompleteDetails arg2)
	{
		Settlement settlement = ((MobileParty.MainParty.CurrentSettlement != null) ? MobileParty.MainParty.CurrentSettlement : MobileParty.MainParty.LastVisitedSettlement);
		if (arg1.QuestGiver?.CurrentSettlement != null && arg1.QuestGiver.CurrentSettlement == settlement)
		{
			Refresh();
		}
	}

	private void OnPeaceDeclared(IFaction faction1, IFaction faction2, MakePeaceAction.MakePeaceDetail detail)
	{
		OnPeaceOrWarDeclared(faction1, faction2);
	}

	private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail reason)
	{
		OnPeaceOrWarDeclared(faction1, faction2);
	}

	private void OnPeaceOrWarDeclared(IFaction arg1, IFaction arg2)
	{
		bool num = Hero.MainHero?.CurrentSettlement?.MapFaction != null && (Hero.MainHero?.CurrentSettlement.MapFaction == arg1 || Hero.MainHero?.CurrentSettlement.MapFaction == arg2);
		bool flag = Hero.MainHero?.MapFaction == arg1 || Hero.MainHero?.MapFaction == arg2;
		if (num || flag)
		{
			InitLists();
		}
	}

	private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero previousOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
	{
		if (settlement == _settlement || (_settlement.IsVillage && settlement.BoundVillages.Contains(_settlement.Village)))
		{
			UpdateSettlementOwnerBanner();
		}
	}

	private void OnTownRebelliousStateChanged(Town town, bool isRebellious)
	{
		if (_settlement.IsTown && _settlement.Town == town)
		{
			IsLoyaltyRebellionWarning = isRebellious || town.Loyalty < (float)Campaign.Current.Models.SettlementLoyaltyModel.RebelliousStateStartLoyaltyThreshold;
		}
	}
}
