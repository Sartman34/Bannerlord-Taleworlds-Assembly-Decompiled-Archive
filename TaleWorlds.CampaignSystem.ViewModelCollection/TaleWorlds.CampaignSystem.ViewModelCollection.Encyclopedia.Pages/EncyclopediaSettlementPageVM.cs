using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Tutorial;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;

[EncyclopediaViewModel(typeof(Settlement))]
public class EncyclopediaSettlementPageVM : EncyclopediaContentPageVM
{
	private enum SettlementTypes
	{
		Town,
		LoneVillage,
		VillageWithCastle
	}

	private readonly Settlement _settlement;

	private int _settlementType;

	private MBBindingList<EncyclopediaHistoryEventVM> _history;

	private MBBindingList<EncyclopediaSettlementVM> _settlements;

	private EncyclopediaSettlementVM _boundSettlement;

	private MBBindingList<HeroVM> _notableCharacters;

	private EncyclopediaFactionVM _ownerBanner;

	private HintViewModel _showInMapHint;

	private BasicTooltipViewModel _militasHint;

	private BasicTooltipViewModel _prosperityHint;

	private BasicTooltipViewModel _loyaltyHint;

	private BasicTooltipViewModel _securityHint;

	private BasicTooltipViewModel _wallsHint;

	private BasicTooltipViewModel _garrisonHint;

	private BasicTooltipViewModel _foodHint;

	private HeroVM _owner;

	private string _ownerText;

	private string _militasText;

	private string _garrisonText;

	private string _prosperityText;

	private string _loyaltyText;

	private string _securityText;

	private string _wallsText;

	private string _foodText;

	private string _nameText;

	private string _cultureText;

	private string _villagesText;

	private string _notableCharactersText;

	private string _settlementPath;

	private string _settlementName;

	private string _informationText;

	private string _settlementImageID;

	private string _boundSettlementText;

	private string _trackText;

	private double _settlementCropPosition;

	private bool _isFortification;

	private bool _isVisualTrackerSelected;

	private bool _hasBoundSettlement;

	private bool _isTrackerButtonHighlightEnabled;

	[DataSourceProperty]
	public EncyclopediaFactionVM OwnerBanner
	{
		get
		{
			return _ownerBanner;
		}
		set
		{
			if (value != _ownerBanner)
			{
				_ownerBanner = value;
				OnPropertyChangedWithValue(value, "OwnerBanner");
			}
		}
	}

	[DataSourceProperty]
	public EncyclopediaSettlementVM BoundSettlement
	{
		get
		{
			return _boundSettlement;
		}
		set
		{
			if (value != _boundSettlement)
			{
				_boundSettlement = value;
				OnPropertyChangedWithValue(value, "BoundSettlement");
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
	public bool IsTrackerButtonHighlightEnabled
	{
		get
		{
			return _isTrackerButtonHighlightEnabled;
		}
		set
		{
			if (value != _isTrackerButtonHighlightEnabled)
			{
				_isTrackerButtonHighlightEnabled = value;
				OnPropertyChangedWithValue(value, "IsTrackerButtonHighlightEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool HasBoundSettlement
	{
		get
		{
			return _hasBoundSettlement;
		}
		set
		{
			if (value != _hasBoundSettlement)
			{
				_hasBoundSettlement = value;
				OnPropertyChangedWithValue(value, "HasBoundSettlement");
			}
		}
	}

	[DataSourceProperty]
	public double SettlementCropPosition
	{
		get
		{
			return _settlementCropPosition;
		}
		set
		{
			if (value != _settlementCropPosition)
			{
				_settlementCropPosition = value;
				OnPropertyChangedWithValue(value, "SettlementCropPosition");
			}
		}
	}

	[DataSourceProperty]
	public string BoundSettlementText
	{
		get
		{
			return _boundSettlementText;
		}
		set
		{
			if (value != _boundSettlementText)
			{
				_boundSettlementText = value;
				OnPropertyChangedWithValue(value, "BoundSettlementText");
			}
		}
	}

	[DataSourceProperty]
	public string TrackText
	{
		get
		{
			return _trackText;
		}
		set
		{
			if (value != _trackText)
			{
				_trackText = value;
				OnPropertyChangedWithValue(value, "TrackText");
			}
		}
	}

	[DataSourceProperty]
	public string SettlementPath
	{
		get
		{
			return _settlementPath;
		}
		set
		{
			if (value != _settlementPath)
			{
				_settlementPath = value;
				OnPropertyChangedWithValue(value, "SettlementPath");
			}
		}
	}

	[DataSourceProperty]
	public string SettlementName
	{
		get
		{
			return _settlementName;
		}
		set
		{
			if (value != _settlementName)
			{
				_settlementName = value;
				OnPropertyChangedWithValue(value, "SettlementName");
			}
		}
	}

	[DataSourceProperty]
	public string InformationText
	{
		get
		{
			return _informationText;
		}
		set
		{
			if (value != _informationText)
			{
				_informationText = value;
				OnPropertyChangedWithValue(value, "InformationText");
			}
		}
	}

	[DataSourceProperty]
	public HeroVM Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			if (value != _owner)
			{
				_owner = value;
				OnPropertyChangedWithValue(value, "Owner");
			}
		}
	}

	[DataSourceProperty]
	public string SettlementsText
	{
		get
		{
			return _villagesText;
		}
		set
		{
			if (value != _villagesText)
			{
				_villagesText = value;
				OnPropertyChanged("VillagesText");
			}
		}
	}

	[DataSourceProperty]
	public string SettlementImageID
	{
		get
		{
			return _settlementImageID;
		}
		set
		{
			if (value != _settlementImageID)
			{
				_settlementImageID = value;
				OnPropertyChangedWithValue(value, "SettlementImageID");
			}
		}
	}

	[DataSourceProperty]
	public string NotableCharactersText
	{
		get
		{
			return _notableCharactersText;
		}
		set
		{
			if (value != _notableCharactersText)
			{
				_notableCharactersText = value;
				OnPropertyChangedWithValue(value, "NotableCharactersText");
			}
		}
	}

	[DataSourceProperty]
	public int SettlementType
	{
		get
		{
			return _settlementType;
		}
		set
		{
			if (value != _settlementType)
			{
				_settlementType = value;
				OnPropertyChangedWithValue(value, "SettlementType");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<EncyclopediaHistoryEventVM> History
	{
		get
		{
			return _history;
		}
		set
		{
			if (value != _history)
			{
				_history = value;
				OnPropertyChangedWithValue(value, "History");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<EncyclopediaSettlementVM> Settlements
	{
		get
		{
			return _settlements;
		}
		set
		{
			if (value != _settlements)
			{
				_settlements = value;
				OnPropertyChanged("Villages");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<HeroVM> NotableCharacters
	{
		get
		{
			return _notableCharacters;
		}
		set
		{
			if (value != _notableCharacters)
			{
				_notableCharacters = value;
				OnPropertyChangedWithValue(value, "NotableCharacters");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ShowInMapHint
	{
		get
		{
			return _showInMapHint;
		}
		set
		{
			if (value != _showInMapHint)
			{
				_showInMapHint = value;
				OnPropertyChangedWithValue(value, "ShowInMapHint");
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
	public BasicTooltipViewModel FoodHint
	{
		get
		{
			return _foodHint;
		}
		set
		{
			if (value != _foodHint)
			{
				_foodHint = value;
				OnPropertyChangedWithValue(value, "FoodHint");
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
	public string MilitasText
	{
		get
		{
			return _militasText;
		}
		set
		{
			if (value != _militasText)
			{
				_militasText = value;
				OnPropertyChangedWithValue(value, "MilitasText");
			}
		}
	}

	[DataSourceProperty]
	public string ProsperityText
	{
		get
		{
			return _prosperityText;
		}
		set
		{
			if (value != _prosperityText)
			{
				_prosperityText = value;
				OnPropertyChangedWithValue(value, "ProsperityText");
			}
		}
	}

	[DataSourceProperty]
	public string LoyaltyText
	{
		get
		{
			return _loyaltyText;
		}
		set
		{
			if (value != _loyaltyText)
			{
				_loyaltyText = value;
				OnPropertyChangedWithValue(value, "LoyaltyText");
			}
		}
	}

	[DataSourceProperty]
	public string SecurityText
	{
		get
		{
			return _securityText;
		}
		set
		{
			if (value != _securityText)
			{
				_securityText = value;
				OnPropertyChangedWithValue(value, "SecurityText");
			}
		}
	}

	[DataSourceProperty]
	public string WallsText
	{
		get
		{
			return _wallsText;
		}
		set
		{
			if (value != _wallsText)
			{
				_wallsText = value;
				OnPropertyChangedWithValue(value, "WallsText");
			}
		}
	}

	[DataSourceProperty]
	public string FoodText
	{
		get
		{
			return _foodText;
		}
		set
		{
			if (value != _foodText)
			{
				_foodText = value;
				OnPropertyChangedWithValue(value, "FoodText");
			}
		}
	}

	[DataSourceProperty]
	public string GarrisonText
	{
		get
		{
			return _garrisonText;
		}
		set
		{
			if (value != _garrisonText)
			{
				_garrisonText = value;
				OnPropertyChangedWithValue(value, "GarrisonText");
			}
		}
	}

	[DataSourceProperty]
	public string NameText
	{
		get
		{
			return _nameText;
		}
		set
		{
			if (value != _nameText)
			{
				_nameText = value;
				OnPropertyChangedWithValue(value, "NameText");
			}
		}
	}

	[DataSourceProperty]
	public string CultureText
	{
		get
		{
			return _cultureText;
		}
		set
		{
			if (value != _cultureText)
			{
				_cultureText = value;
				OnPropertyChangedWithValue(value, "CultureText");
			}
		}
	}

	[DataSourceProperty]
	public string OwnerText
	{
		get
		{
			return _ownerText;
		}
		set
		{
			if (value != _ownerText)
			{
				_ownerText = value;
				OnPropertyChangedWithValue(value, "OwnerText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsVisualTrackerSelected
	{
		get
		{
			return _isVisualTrackerSelected;
		}
		set
		{
			if (value != _isVisualTrackerSelected)
			{
				_isVisualTrackerSelected = value;
				OnPropertyChangedWithValue(value, "IsVisualTrackerSelected");
			}
		}
	}

	public EncyclopediaSettlementPageVM(EncyclopediaPageArgs args)
		: base(args)
	{
		_settlement = base.Obj as Settlement;
		NotableCharacters = new MBBindingList<HeroVM>();
		Settlements = new MBBindingList<EncyclopediaSettlementVM>();
		History = new MBBindingList<EncyclopediaHistoryEventVM>();
		_isVisualTrackerSelected = Campaign.Current.VisualTrackerManager.CheckTracked(_settlement);
		IsFortification = _settlement.IsFortification;
		SettlementImageID = _settlement.SettlementComponent.WaitMeshName;
		base.IsBookmarked = Campaign.Current.EncyclopediaManager.ViewDataTracker.IsEncyclopediaBookmarked(_settlement);
		Game.Current.EventManager.RegisterEvent<TutorialNotificationElementChangeEvent>(OnTutorialNotificationElementIDChange);
		RefreshValues();
		if (CampaignUIHelper.IsSettlementInformationHidden(_settlement, out var _))
		{
			Game.Current.EventManager.TriggerEvent(new EncyclopediaPageChangedEvent(EncyclopediaPages.Settlement, hasHiddenInformation: true));
		}
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		SettlementName = _settlement.Name.ToString();
		SettlementsText = GameTexts.FindText("str_villages").ToString();
		NotableCharactersText = GameTexts.FindText("str_notable_characters").ToString();
		OwnerText = GameTexts.FindText("str_owner").ToString();
		TrackText = GameTexts.FindText("str_settlement_track").ToString();
		ShowInMapHint = new HintViewModel(GameTexts.FindText("str_show_on_map"));
		InformationText = _settlement.EncyclopediaText.ToString();
		UpdateBookmarkHintText();
		Refresh();
	}

	public override void Refresh()
	{
		base.IsLoadingOver = false;
		SettlementComponent settlementComponent = _settlement.SettlementComponent;
		NotableCharacters.Clear();
		Settlements.Clear();
		History.Clear();
		IsFortification = _settlement.IsFortification;
		if (_settlement.IsFortification)
		{
			SettlementType = 0;
			EncyclopediaPage pageOf = Campaign.Current.EncyclopediaManager.GetPageOf(typeof(Settlement));
			foreach (Village boundVillage in _settlement.BoundVillages)
			{
				if (pageOf.IsValidEncyclopediaItem(boundVillage.Owner.Settlement))
				{
					Settlements.Add(new EncyclopediaSettlementVM(boundVillage.Owner.Settlement));
				}
			}
		}
		else if (_settlement.IsVillage)
		{
			SettlementType = 1;
		}
		if (!_settlement.IsCastle)
		{
			EncyclopediaPage pageOf2 = Campaign.Current.EncyclopediaManager.GetPageOf(typeof(Hero));
			foreach (Hero notable in _settlement.Notables)
			{
				if (pageOf2.IsValidEncyclopediaItem(notable))
				{
					NotableCharacters.Add(new HeroVM(notable));
				}
			}
		}
		GameTexts.SetVariable("STR1", GameTexts.FindText("str_enc_sf_culture").ToString());
		GameTexts.SetVariable("STR2", _settlement.Culture.Name.ToString());
		CultureText = GameTexts.FindText("str_STR1_space_STR2").ToString();
		OwnerText = GameTexts.FindText("str_owner").ToString();
		Owner = new HeroVM(_settlement.OwnerClan.Leader);
		OwnerBanner = new EncyclopediaFactionVM(_settlement.OwnerClan);
		SettlementPath = settlementComponent.BackgroundMeshName;
		SettlementCropPosition = settlementComponent.BackgroundCropPosition;
		HasBoundSettlement = _settlement.IsVillage;
		BoundSettlement = (HasBoundSettlement ? new EncyclopediaSettlementVM(_settlement.Village.Bound) : null);
		BoundSettlementText = "";
		if (HasBoundSettlement)
		{
			GameTexts.SetVariable("SETTLEMENT_LINK", _settlement.Village.Bound.EncyclopediaLinkWithName);
			BoundSettlementText = GameTexts.FindText("str_bound_settlement_encyclopedia").ToString();
		}
		int num = (int)_settlement.Militia;
		TextObject disableReason;
		bool flag = CampaignUIHelper.IsSettlementInformationHidden(_settlement, out disableReason);
		string text = GameTexts.FindText("str_missing_info_indicator").ToString();
		MilitasText = (flag ? text : num.ToString());
		if (_settlement.IsFortification)
		{
			FoodHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownFoodTooltip(_settlement.Town));
			LoyaltyHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownLoyaltyTooltip(_settlement.Town));
			MilitasHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownMilitiaTooltip(_settlement.Town));
			ProsperityHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownProsperityTooltip(_settlement.Town));
			WallsHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownWallsTooltip(_settlement.Town));
			GarrisonHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownGarrisonTooltip(_settlement.Town));
			SecurityHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetTownSecurityTooltip(_settlement.Town));
			ProsperityText = (flag ? text : ((int)_settlement.Town.Prosperity).ToString());
			LoyaltyText = (flag ? text : ((int)_settlement.Town.Loyalty).ToString());
			SecurityText = (flag ? text : ((int)_settlement.Town.Security).ToString());
			GarrisonText = (flag ? text : (_settlement.Town.GarrisonParty?.Party.NumberOfAllMembers.ToString() ?? "0"));
			FoodText = (flag ? text : ((int)_settlement.Town.FoodStocks).ToString());
			WallsText = (flag ? text : _settlement.Town.GetWallLevel().ToString());
		}
		else
		{
			MilitasHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetVillageMilitiaTooltip(_settlement.Village));
			ProsperityHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetVillageProsperityTooltip(_settlement.Village));
			LoyaltyHint = new BasicTooltipViewModel();
			WallsHint = new BasicTooltipViewModel();
			ProsperityText = (flag ? text : ((int)_settlement.Village.Hearth).ToString());
			LoyaltyText = "-";
			SecurityText = "-";
			FoodText = "-";
			GarrisonText = "-";
			WallsText = "-";
		}
		NameText = _settlement.Name.ToString();
		for (int num2 = Campaign.Current.LogEntryHistory.GameActionLogs.Count - 1; num2 >= 0; num2--)
		{
			if (Campaign.Current.LogEntryHistory.GameActionLogs[num2] is IEncyclopediaLog encyclopediaLog && encyclopediaLog.IsVisibleInEncyclopediaPageOf(_settlement))
			{
				History.Add(new EncyclopediaHistoryEventVM(encyclopediaLog));
			}
		}
		IsVisualTrackerSelected = Campaign.Current.VisualTrackerManager.CheckTracked(_settlement);
		base.IsLoadingOver = true;
	}

	public override string GetName()
	{
		return _settlement.Name.ToString();
	}

	public void ExecuteTrack()
	{
		if (!IsVisualTrackerSelected)
		{
			Campaign.Current.VisualTrackerManager.RegisterObject(_settlement);
			IsVisualTrackerSelected = true;
		}
		else
		{
			Campaign.Current.VisualTrackerManager.RemoveTrackedObject(_settlement);
			IsVisualTrackerSelected = false;
		}
		Game.Current.EventManager.TriggerEvent(new PlayerToggleTrackSettlementFromEncyclopediaEvent(_settlement, IsVisualTrackerSelected));
	}

	public override string GetNavigationBarURL()
	{
		return string.Concat(string.Concat(string.Concat(HyperlinkTexts.GetGenericHyperlinkText("Home", GameTexts.FindText("str_encyclopedia_home").ToString()) + " \\ ", HyperlinkTexts.GetGenericHyperlinkText("ListPage-Settlements", GameTexts.FindText("str_encyclopedia_settlements").ToString())), " \\ "), GetName());
	}

	public void ExecuteBoundSettlementLink()
	{
		if (HasBoundSettlement)
		{
			Campaign.Current.EncyclopediaManager.GoToLink(_settlement.Village.Bound.EncyclopediaLink);
		}
	}

	public override void ExecuteSwitchBookmarkedState()
	{
		base.ExecuteSwitchBookmarkedState();
		if (base.IsBookmarked)
		{
			Campaign.Current.EncyclopediaManager.ViewDataTracker.AddEncyclopediaBookmarkToItem(_settlement);
		}
		else
		{
			Campaign.Current.EncyclopediaManager.ViewDataTracker.RemoveEncyclopediaBookmarkFromItem(_settlement);
		}
	}

	private void OnTutorialNotificationElementIDChange(TutorialNotificationElementChangeEvent evnt)
	{
		IsTrackerButtonHighlightEnabled = evnt.NewNotificationElementID == "EncyclopediaItemTrackButton";
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		Game.Current.EventManager.UnregisterEvent<TutorialNotificationElementChangeEvent>(OnTutorialNotificationElementIDChange);
	}
}
