using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Library.Information;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;

public class MapInfoVM : ViewModel
{
	private int _latestTotalWage = -1;

	private float _latestSeeingRange = -1f;

	private float _latestSpeed = -1f;

	private float _latestMorale = -1f;

	private IViewDataTracker _viewDataTracker;

	private string _speed;

	private string _viewDistance;

	private string _trainingFactor;

	private string _troopWage;

	private string _healthTextWithPercentage;

	private string _denarsWithAbbrText = "";

	private string _influenceWithAbbrText = "";

	private string _availableTroopsText;

	private int _denars = -1;

	private int _influence = -1;

	private int _morale = -1;

	private int _totalFood;

	private int _health;

	private int _totalTroops;

	private bool _isInfoBarExtended;

	private bool _isInfoBarEnabled;

	private bool _isDenarTooltipWarning;

	private bool _isHealthTooltipWarning;

	private bool _isInfluenceTooltipWarning;

	private bool _isMoraleTooltipWarning;

	private bool _isDailyConsumptionTooltipWarning;

	private bool _isAvailableTroopsTooltipWarning;

	private bool _isMainHeroSick;

	private TooltipTriggerVM _denarTooltip;

	private BasicTooltipViewModel _influenceHint;

	private BasicTooltipViewModel _availableTroopsHint;

	private BasicTooltipViewModel _healthHint;

	private BasicTooltipViewModel _dailyConsumptionHint;

	private BasicTooltipViewModel _moraleHint;

	private BasicTooltipViewModel _trainingFactorHint;

	private BasicTooltipViewModel _troopWageHint;

	private BasicTooltipViewModel _speedHint;

	private BasicTooltipViewModel _viewDistanceHint;

	private HintViewModel _extendHint;

	[DataSourceProperty]
	public bool IsHealthTooltipWarning
	{
		get
		{
			return _isHealthTooltipWarning;
		}
		set
		{
			if (value != _isHealthTooltipWarning)
			{
				_isHealthTooltipWarning = value;
				OnPropertyChangedWithValue(value, "IsHealthTooltipWarning");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMainHeroSick
	{
		get
		{
			return _isMainHeroSick;
		}
		set
		{
			if (value != _isMainHeroSick)
			{
				_isMainHeroSick = value;
				OnPropertyChangedWithValue(value, "IsMainHeroSick");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ExtendHint
	{
		get
		{
			return _extendHint;
		}
		set
		{
			if (value != _extendHint)
			{
				_extendHint = value;
				OnPropertyChangedWithValue(value, "ExtendHint");
			}
		}
	}

	[DataSourceProperty]
	public TooltipTriggerVM DenarTooltip
	{
		get
		{
			return _denarTooltip;
		}
		set
		{
			if (value != _denarTooltip)
			{
				_denarTooltip = value;
				OnPropertyChangedWithValue(value, "DenarTooltip");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel InfluenceHint
	{
		get
		{
			return _influenceHint;
		}
		set
		{
			if (value != _influenceHint)
			{
				_influenceHint = value;
				OnPropertyChangedWithValue(value, "InfluenceHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel AvailableTroopsHint
	{
		get
		{
			return _availableTroopsHint;
		}
		set
		{
			if (value != _availableTroopsHint)
			{
				_availableTroopsHint = value;
				OnPropertyChangedWithValue(value, "AvailableTroopsHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel HealthHint
	{
		get
		{
			return _healthHint;
		}
		set
		{
			if (value != _healthHint)
			{
				_healthHint = value;
				OnPropertyChangedWithValue(value, "HealthHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel DailyConsumptionHint
	{
		get
		{
			return _dailyConsumptionHint;
		}
		set
		{
			if (value != _dailyConsumptionHint)
			{
				_dailyConsumptionHint = value;
				OnPropertyChangedWithValue(value, "DailyConsumptionHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel MoraleHint
	{
		get
		{
			return _moraleHint;
		}
		set
		{
			if (value != _moraleHint)
			{
				_moraleHint = value;
				OnPropertyChangedWithValue(value, "MoraleHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel SpeedHint
	{
		get
		{
			return _speedHint;
		}
		set
		{
			if (value != _speedHint)
			{
				_speedHint = value;
				OnPropertyChangedWithValue(value, "SpeedHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel ViewDistanceHint
	{
		get
		{
			return _viewDistanceHint;
		}
		set
		{
			if (value != _viewDistanceHint)
			{
				_viewDistanceHint = value;
				OnPropertyChangedWithValue(value, "ViewDistanceHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel TrainingFactorHint
	{
		get
		{
			return _trainingFactorHint;
		}
		set
		{
			if (value != _trainingFactorHint)
			{
				_trainingFactorHint = value;
				OnPropertyChangedWithValue(value, "TrainingFactorHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel TroopWageHint
	{
		get
		{
			return _troopWageHint;
		}
		set
		{
			if (value != _troopWageHint)
			{
				_troopWageHint = value;
				OnPropertyChangedWithValue(value, "TroopWageHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDenarTooltipWarning
	{
		get
		{
			return _isDenarTooltipWarning;
		}
		set
		{
			if (value != _isDenarTooltipWarning)
			{
				_isDenarTooltipWarning = value;
				OnPropertyChangedWithValue(value, "IsDenarTooltipWarning");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInfluenceTooltipWarning
	{
		get
		{
			return _isInfluenceTooltipWarning;
		}
		set
		{
			if (value != _isInfluenceTooltipWarning)
			{
				_isInfluenceTooltipWarning = value;
				OnPropertyChangedWithValue(value, "IsInfluenceTooltipWarning");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMoraleTooltipWarning
	{
		get
		{
			return _isMoraleTooltipWarning;
		}
		set
		{
			if (value != _isMoraleTooltipWarning)
			{
				_isMoraleTooltipWarning = value;
				OnPropertyChangedWithValue(value, "IsMoraleTooltipWarning");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDailyConsumptionTooltipWarning
	{
		get
		{
			return _isDailyConsumptionTooltipWarning;
		}
		set
		{
			if (value != _isDailyConsumptionTooltipWarning)
			{
				_isDailyConsumptionTooltipWarning = value;
				OnPropertyChangedWithValue(value, "IsDailyConsumptionTooltipWarning");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAvailableTroopsTooltipWarning
	{
		get
		{
			return _isAvailableTroopsTooltipWarning;
		}
		set
		{
			if (value != _isAvailableTroopsTooltipWarning)
			{
				_isAvailableTroopsTooltipWarning = value;
				OnPropertyChangedWithValue(value, "IsAvailableTroopsTooltipWarning");
			}
		}
	}

	[DataSourceProperty]
	public string DenarsWithAbbrText
	{
		get
		{
			return _denarsWithAbbrText;
		}
		set
		{
			if (value != _denarsWithAbbrText)
			{
				_denarsWithAbbrText = value;
				OnPropertyChangedWithValue(value, "DenarsWithAbbrText");
			}
		}
	}

	[DataSourceProperty]
	public int Denars
	{
		get
		{
			return _denars;
		}
		set
		{
			if (value != _denars)
			{
				_denars = value;
				OnPropertyChangedWithValue(value, "Denars");
			}
		}
	}

	[DataSourceProperty]
	public int Influence
	{
		get
		{
			return _influence;
		}
		set
		{
			if (value != _influence)
			{
				_influence = value;
				OnPropertyChangedWithValue(value, "Influence");
			}
		}
	}

	[DataSourceProperty]
	public string InfluenceWithAbbrText
	{
		get
		{
			return _influenceWithAbbrText;
		}
		set
		{
			if (value != _influenceWithAbbrText)
			{
				_influenceWithAbbrText = value;
				OnPropertyChangedWithValue(value, "InfluenceWithAbbrText");
			}
		}
	}

	[DataSourceProperty]
	public int Morale
	{
		get
		{
			return _morale;
		}
		set
		{
			if (value != _morale)
			{
				_morale = value;
				OnPropertyChangedWithValue(value, "Morale");
			}
		}
	}

	[DataSourceProperty]
	public int TotalFood
	{
		get
		{
			return _totalFood;
		}
		set
		{
			if (value != _totalFood)
			{
				_totalFood = value;
				OnPropertyChangedWithValue(value, "TotalFood");
			}
		}
	}

	[DataSourceProperty]
	public int Health
	{
		get
		{
			return _health;
		}
		set
		{
			if (value != _health)
			{
				_health = value;
				OnPropertyChangedWithValue(value, "Health");
			}
		}
	}

	[DataSourceProperty]
	public string HealthTextWithPercentage
	{
		get
		{
			return _healthTextWithPercentage;
		}
		set
		{
			if (value != _healthTextWithPercentage)
			{
				_healthTextWithPercentage = value;
				OnPropertyChangedWithValue(value, "HealthTextWithPercentage");
			}
		}
	}

	[DataSourceProperty]
	public string AvailableTroopsText
	{
		get
		{
			return _availableTroopsText;
		}
		set
		{
			if (value != _availableTroopsText)
			{
				_availableTroopsText = value;
				OnPropertyChangedWithValue(value, "AvailableTroopsText");
			}
		}
	}

	[DataSourceProperty]
	public int TotalTroops
	{
		get
		{
			return _totalTroops;
		}
		set
		{
			if (value != _totalTroops)
			{
				_totalTroops = value;
				OnPropertyChangedWithValue(value, "TotalTroops");
			}
		}
	}

	[DataSourceProperty]
	public string Speed
	{
		get
		{
			return _speed;
		}
		set
		{
			if (value != _speed)
			{
				_speed = value;
				OnPropertyChangedWithValue(value, "Speed");
			}
		}
	}

	[DataSourceProperty]
	public string ViewDistance
	{
		get
		{
			return _viewDistance;
		}
		set
		{
			if (value != _viewDistance)
			{
				_viewDistance = value;
				OnPropertyChangedWithValue(value, "ViewDistance");
			}
		}
	}

	[DataSourceProperty]
	public string TrainingFactor
	{
		get
		{
			return _trainingFactor;
		}
		set
		{
			if (value != _trainingFactor)
			{
				_trainingFactor = value;
				OnPropertyChangedWithValue(value, "TrainingFactor");
			}
		}
	}

	[DataSourceProperty]
	public string TroopWage
	{
		get
		{
			return _troopWage;
		}
		set
		{
			if (value != _troopWage)
			{
				_troopWage = value;
				OnPropertyChangedWithValue(value, "TroopWage");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInfoBarExtended
	{
		get
		{
			return _isInfoBarExtended;
		}
		set
		{
			if (value != _isInfoBarExtended)
			{
				_isInfoBarExtended = value;
				_viewDataTracker.SetMapBarExtendedState(value);
				OnPropertyChangedWithValue(value, "IsInfoBarExtended");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInfoBarEnabled
	{
		get
		{
			return _isInfoBarEnabled;
		}
		set
		{
			if (value != _isInfoBarEnabled)
			{
				_isInfoBarEnabled = value;
				OnPropertyChangedWithValue(value, "IsInfoBarEnabled");
			}
		}
	}

	public MapInfoVM()
	{
		DenarTooltip = CampaignUIHelper.GetDenarTooltip();
		HealthHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetPlayerHitpointsTooltip());
		InfluenceHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetInfluenceTooltip(Clan.PlayerClan));
		AvailableTroopsHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetMainPartyHealthTooltip());
		ExtendHint = new HintViewModel(GameTexts.FindText("str_map_extend_bar_hint"));
		SpeedHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetPartySpeedTooltip());
		ViewDistanceHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetViewDistanceTooltip());
		TroopWageHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetPartyWageTooltip());
		MoraleHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetPartyMoraleTooltip(MobileParty.MainParty));
		DailyConsumptionHint = new BasicTooltipViewModel(() => CampaignUIHelper.GetPartyFoodTooltip(MobileParty.MainParty));
		_viewDataTracker = Campaign.Current.GetCampaignBehavior<IViewDataTracker>();
		IsInfoBarExtended = _viewDataTracker.GetMapBarExtendedState();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		UpdatePlayerInfo(updateForced: true);
	}

	public void Tick()
	{
		IsMainHeroSick = Hero.MainHero != null && Hero.IsMainHeroIll;
		IsInfoBarEnabled = Hero.MainHero?.IsAlive ?? false;
	}

	public void Refresh()
	{
		UpdatePlayerInfo(updateForced: false);
	}

	private void UpdatePlayerInfo(bool updateForced)
	{
		int totalWage = MobileParty.MainParty.TotalWage;
		ExplainedNumber explainedNumber = Campaign.Current.Models.ClanFinanceModel.CalculateClanGoldChange(Clan.PlayerClan, includeDescriptions: true, applyWithdrawals: false, includeDetails: true);
		IsDenarTooltipWarning = (float)Hero.MainHero.Gold + explainedNumber.ResultNumber < 0f;
		IsInfluenceTooltipWarning = Hero.MainHero.Clan.Influence < -100f;
		IsMoraleTooltipWarning = MobileParty.MainParty.Morale < (float)Campaign.Current.Models.PartyDesertionModel.GetMoraleThresholdForTroopDesertion(MobileParty.MainParty);
		int numDaysForFoodToLast = MobileParty.MainParty.GetNumDaysForFoodToLast();
		IsDailyConsumptionTooltipWarning = numDaysForFoodToLast < 1;
		IsAvailableTroopsTooltipWarning = PartyBase.MainParty.PartySizeLimit < PartyBase.MainParty.NumberOfAllMembers || PartyBase.MainParty.PrisonerSizeLimit < PartyBase.MainParty.NumberOfPrisoners;
		IsHealthTooltipWarning = Hero.MainHero.IsWounded;
		if (Denars != Hero.MainHero.Gold || updateForced)
		{
			Denars = Hero.MainHero.Gold;
			DenarsWithAbbrText = CampaignUIHelper.GetAbbreviatedValueTextFromValue(Denars);
		}
		if (Influence != (int)Hero.MainHero.Clan.Influence || updateForced)
		{
			Influence = (int)Hero.MainHero.Clan.Influence;
			InfluenceWithAbbrText = CampaignUIHelper.GetAbbreviatedValueTextFromValue(Influence);
		}
		Morale = (int)MobileParty.MainParty.Morale;
		TotalFood = (int)((MobileParty.MainParty.Food > 0f) ? MobileParty.MainParty.Food : 0f);
		TotalTroops = PartyBase.MainParty.MemberRoster.TotalManCount;
		AvailableTroopsText = CampaignUIHelper.GetPartyNameplateText(PartyBase.MainParty);
		int num = (int)MathF.Clamp(Hero.MainHero.HitPoints * 100 / CharacterObject.PlayerCharacter.MaxHitPoints(), 1f, 100f);
		if (Health != num || updateForced)
		{
			Health = num;
			GameTexts.SetVariable("NUMBER", Health);
			HealthTextWithPercentage = GameTexts.FindText("str_NUMBER_percent").ToString();
		}
		float num2 = MathF.Round(MobileParty.MainParty.Morale, 1);
		if (_latestMorale != num2 || updateForced)
		{
			_latestMorale = num2;
			MBTextManager.SetTextVariable("BASE_EFFECT", num2.ToString("0.0"));
		}
		float num3 = (MobileParty.MainParty.CurrentNavigationFace.IsValid() ? MobileParty.MainParty.Speed : 0f);
		if (_latestSpeed != num3 || updateForced)
		{
			_latestSpeed = num3;
			Speed = CampaignUIHelper.FloatToString(num3);
		}
		float seeingRange = MobileParty.MainParty.SeeingRange;
		if (_latestSeeingRange != seeingRange || updateForced)
		{
			_latestSeeingRange = seeingRange;
			ViewDistance = CampaignUIHelper.FloatToString(seeingRange);
		}
		if (_latestTotalWage != totalWage || updateForced)
		{
			_latestTotalWage = totalWage;
			TroopWage = totalWage.ToString();
		}
	}
}
