using System;
using System.Linq;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Armies;

public class KingdomArmyItemVM : KingdomItemVM
{
	public readonly Army Army;

	private readonly Action<KingdomArmyItemVM> _onSelect;

	private readonly IViewDataTracker _viewDataTracker;

	private HeroVM _leader;

	private MBBindingList<KingdomArmyPartyItemVM> _parties;

	private string _armyName;

	private int _strength;

	private int _cohesion;

	private string _strengthLabel;

	private int _lordCount;

	private string _location;

	private string _cohesionLabel;

	private bool _isMainArmy;

	public float DistanceToMainParty { get; set; }

	[DataSourceProperty]
	public MBBindingList<KingdomArmyPartyItemVM> Parties
	{
		get
		{
			return _parties;
		}
		set
		{
			if (value != _parties)
			{
				_parties = value;
				OnPropertyChangedWithValue(value, "Parties");
			}
		}
	}

	[DataSourceProperty]
	public HeroVM Leader
	{
		get
		{
			return _leader;
		}
		set
		{
			if (value != _leader)
			{
				_leader = value;
				OnPropertyChanged("Visual");
			}
		}
	}

	[DataSourceProperty]
	public string ArmyName
	{
		get
		{
			return _armyName;
		}
		set
		{
			if (value != _armyName)
			{
				_armyName = value;
				OnPropertyChangedWithValue(value, "ArmyName");
			}
		}
	}

	[DataSourceProperty]
	public int Cohesion
	{
		get
		{
			return _cohesion;
		}
		set
		{
			if (value != _cohesion)
			{
				_cohesion = value;
				OnPropertyChangedWithValue(value, "Cohesion");
			}
		}
	}

	[DataSourceProperty]
	public string CohesionLabel
	{
		get
		{
			return _cohesionLabel;
		}
		set
		{
			if (value != _cohesionLabel)
			{
				_cohesionLabel = value;
				OnPropertyChangedWithValue(value, "CohesionLabel");
			}
		}
	}

	[DataSourceProperty]
	public int LordCount
	{
		get
		{
			return _lordCount;
		}
		set
		{
			if (value != _lordCount)
			{
				_lordCount = value;
				OnPropertyChangedWithValue(value, "LordCount");
			}
		}
	}

	[DataSourceProperty]
	public int Strength
	{
		get
		{
			return _strength;
		}
		set
		{
			if (value != _strength)
			{
				_strength = value;
				OnPropertyChangedWithValue(value, "Strength");
			}
		}
	}

	[DataSourceProperty]
	public string StrengthLabel
	{
		get
		{
			return _strengthLabel;
		}
		set
		{
			if (value != _strengthLabel)
			{
				_strengthLabel = value;
				OnPropertyChangedWithValue(value, "StrengthLabel");
			}
		}
	}

	[DataSourceProperty]
	public string Location
	{
		get
		{
			return _location;
		}
		set
		{
			if (value != _location)
			{
				_location = value;
				OnPropertyChanged("Objective");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMainArmy
	{
		get
		{
			return _isMainArmy;
		}
		set
		{
			if (value != _isMainArmy)
			{
				_isMainArmy = value;
				OnPropertyChangedWithValue(value, "IsMainArmy");
			}
		}
	}

	public KingdomArmyItemVM(Army army, Action<KingdomArmyItemVM> onSelect)
	{
		Army = army;
		_onSelect = onSelect;
		_viewDataTracker = Campaign.Current.GetCampaignBehavior<IViewDataTracker>();
		CampaignUIHelper.GetCharacterCode(army.ArmyOwner.CharacterObject);
		Leader = new HeroVM(Army.LeaderParty.LeaderHero);
		LordCount = army.Parties.Count;
		Strength = army.Parties.Sum((MobileParty p) => p.Party.NumberOfAllMembers);
		Location = army.GetBehaviorText(setWithLink: true).ToString();
		UpdateIsNew();
		Cohesion = (int)Army.Cohesion;
		Parties = new MBBindingList<KingdomArmyPartyItemVM>();
		foreach (MobileParty party in Army.Parties)
		{
			Parties.Add(new KingdomArmyPartyItemVM(party));
		}
		DistanceToMainParty = Campaign.Current.Models.MapDistanceModel.GetDistance(army.LeaderParty, MobileParty.MainParty);
		IsMainArmy = army.LeaderParty == MobileParty.MainParty;
		RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		ArmyName = Army.Name.ToString();
		GameTexts.SetVariable("STR1", GameTexts.FindText("str_cohesion"));
		GameTexts.SetVariable("STR2", Cohesion.ToString());
		CohesionLabel = GameTexts.FindText("str_STR1_space_STR2").ToString();
		GameTexts.SetVariable("LEFT", GameTexts.FindText("str_men_count"));
		GameTexts.SetVariable("RIGHT", Strength.ToString());
		StrengthLabel = GameTexts.FindText("str_LEFT_colon_RIGHT_wSpaceAfterColon").ToString();
		Parties.ApplyActionOnAllItems(delegate(KingdomArmyPartyItemVM x)
		{
			x.RefreshValues();
		});
	}

	protected override void OnSelect()
	{
		base.OnSelect();
		_onSelect(this);
		ExecuteResetNew();
	}

	private void ExecuteResetNew()
	{
		if (base.IsNew)
		{
			_viewDataTracker.OnArmyExamined(Army);
			UpdateIsNew();
		}
	}

	private void UpdateIsNew()
	{
		base.IsNew = _viewDataTracker.UnExaminedArmies.Any((Army a) => a == Army);
	}

	protected void ExecuteLink(string link)
	{
		Campaign.Current.EncyclopediaManager.GoToLink(link);
	}
}
