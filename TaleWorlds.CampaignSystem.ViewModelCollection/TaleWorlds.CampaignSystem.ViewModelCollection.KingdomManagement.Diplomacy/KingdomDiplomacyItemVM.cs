using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;

public abstract class KingdomDiplomacyItemVM : KingdomItemVM
{
	public readonly IFaction Faction1;

	public readonly IFaction Faction2;

	protected readonly string _faction1Color;

	protected readonly string _faction2Color;

	protected readonly IFaction _playerKingdom;

	protected List<Settlement> _faction1Towns;

	protected List<Settlement> _faction2Towns;

	protected List<Settlement> _faction1Castles;

	protected List<Settlement> _faction2Castles;

	private MBBindingList<KingdomWarComparableStatVM> _stats;

	private ImageIdentifierVM _faction1Visual;

	private ImageIdentifierVM _faction2Visual;

	private HeroVM _faction1Leader;

	private HeroVM _faction2Leader;

	private string _faction1Name;

	private string _faction2Name;

	private string _faction1TributeText;

	private string _faction2TributeText;

	private HintViewModel _faction1TributeHint;

	private HintViewModel _faction2TributeHint;

	private bool _isFaction2OtherWarsVisible;

	private MBBindingList<KingdomDiplomacyFactionItemVM> _faction1OwnedClans;

	private MBBindingList<KingdomDiplomacyFactionItemVM> _faction2OwnedClans;

	private MBBindingList<KingdomDiplomacyFactionItemVM> _faction2OtherWars;

	[DataSourceProperty]
	public MBBindingList<KingdomDiplomacyFactionItemVM> Faction1OwnedClans
	{
		get
		{
			return _faction1OwnedClans;
		}
		set
		{
			if (value != _faction1OwnedClans)
			{
				_faction1OwnedClans = value;
				OnPropertyChangedWithValue(value, "Faction1OwnedClans");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<KingdomDiplomacyFactionItemVM> Faction2OwnedClans
	{
		get
		{
			return _faction2OwnedClans;
		}
		set
		{
			if (value != _faction2OwnedClans)
			{
				_faction2OwnedClans = value;
				OnPropertyChangedWithValue(value, "Faction2OwnedClans");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<KingdomDiplomacyFactionItemVM> Faction2OtherWars
	{
		get
		{
			return _faction2OtherWars;
		}
		set
		{
			if (value != _faction2OtherWars)
			{
				_faction2OtherWars = value;
				OnPropertyChangedWithValue(value, "Faction2OtherWars");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<KingdomWarComparableStatVM> Stats
	{
		get
		{
			return _stats;
		}
		set
		{
			if (value != _stats)
			{
				_stats = value;
				OnPropertyChangedWithValue(value, "Stats");
			}
		}
	}

	[DataSourceProperty]
	public ImageIdentifierVM Faction1Visual
	{
		get
		{
			return _faction1Visual;
		}
		set
		{
			if (value != _faction1Visual)
			{
				_faction1Visual = value;
				OnPropertyChangedWithValue(value, "Faction1Visual");
			}
		}
	}

	[DataSourceProperty]
	public ImageIdentifierVM Faction2Visual
	{
		get
		{
			return _faction2Visual;
		}
		set
		{
			if (value != _faction2Visual)
			{
				_faction2Visual = value;
				OnPropertyChangedWithValue(value, "Faction2Visual");
			}
		}
	}

	[DataSourceProperty]
	public string Faction1Name
	{
		get
		{
			return _faction1Name;
		}
		set
		{
			if (value != _faction1Name)
			{
				_faction1Name = value;
				OnPropertyChangedWithValue(value, "Faction1Name");
			}
		}
	}

	[DataSourceProperty]
	public string Faction2Name
	{
		get
		{
			return _faction2Name;
		}
		set
		{
			if (value != _faction2Name)
			{
				_faction2Name = value;
				OnPropertyChangedWithValue(value, "Faction2Name");
			}
		}
	}

	[DataSourceProperty]
	public string Faction1TributeText
	{
		get
		{
			return _faction1TributeText;
		}
		set
		{
			if (value != _faction1TributeText)
			{
				_faction1TributeText = value;
				OnPropertyChangedWithValue(value, "Faction1TributeText");
			}
		}
	}

	[DataSourceProperty]
	public string Faction2TributeText
	{
		get
		{
			return _faction2TributeText;
		}
		set
		{
			if (value != _faction2TributeText)
			{
				_faction2TributeText = value;
				OnPropertyChangedWithValue(value, "Faction2TributeText");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel Faction1TributeHint
	{
		get
		{
			return _faction1TributeHint;
		}
		set
		{
			if (value != _faction1TributeHint)
			{
				_faction1TributeHint = value;
				OnPropertyChangedWithValue(value, "Faction1TributeHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel Faction2TributeHint
	{
		get
		{
			return _faction2TributeHint;
		}
		set
		{
			if (value != _faction2TributeHint)
			{
				_faction2TributeHint = value;
				OnPropertyChangedWithValue(value, "Faction2TributeHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFaction2OtherWarsVisible
	{
		get
		{
			return _isFaction2OtherWarsVisible;
		}
		set
		{
			if (value != _isFaction2OtherWarsVisible)
			{
				_isFaction2OtherWarsVisible = value;
				OnPropertyChangedWithValue(value, "IsFaction2OtherWarsVisible");
			}
		}
	}

	[DataSourceProperty]
	public HeroVM Faction1Leader
	{
		get
		{
			return _faction1Leader;
		}
		set
		{
			if (value != _faction1Leader)
			{
				_faction1Leader = value;
				OnPropertyChangedWithValue(value, "Faction1Leader");
			}
		}
	}

	[DataSourceProperty]
	public HeroVM Faction2Leader
	{
		get
		{
			return _faction2Leader;
		}
		set
		{
			if (value != _faction2Leader)
			{
				_faction2Leader = value;
				OnPropertyChangedWithValue(value, "Faction2Leader");
			}
		}
	}

	protected KingdomDiplomacyItemVM(IFaction faction1, IFaction faction2)
	{
		_playerKingdom = Hero.MainHero.MapFaction;
		if (faction1 == _playerKingdom || faction2 == _playerKingdom)
		{
			Faction1 = _playerKingdom;
			Faction2 = ((faction1 != _playerKingdom) ? faction1 : faction2);
		}
		else
		{
			Faction1 = faction1;
			Faction2 = faction2;
		}
		_faction1Color = Color.FromUint(Faction1.Color).ToString();
		_faction2Color = Color.FromUint(Faction2.Color).ToString();
		Stats = new MBBindingList<KingdomWarComparableStatVM>();
		PopulateSettlements();
	}

	protected virtual void UpdateDiplomacyProperties()
	{
		Stats.Clear();
		Faction1Visual = new ImageIdentifierVM(BannerCode.CreateFrom(Faction1.Banner), nineGrid: true);
		Faction2Visual = new ImageIdentifierVM(BannerCode.CreateFrom(Faction2.Banner), nineGrid: true);
		int dailyTributePaid = _playerKingdom.GetStanceWith(Faction2).GetDailyTributePaid(_playerKingdom);
		TextObject textObject = new TextObject("{=SDhQWonF}Paying {DENAR}{GOLD_ICON} as tribute per day.");
		textObject.SetTextVariable("DENAR", MathF.Abs(dailyTributePaid));
		Faction1TributeText = ((dailyTributePaid > 0) ? textObject.ToString() : string.Empty);
		Faction2TributeText = ((dailyTributePaid < 0) ? textObject.ToString() : string.Empty);
		Faction1Name = Faction1.Name.ToString();
		Faction2Name = Faction2.Name.ToString();
		TextObject textObject2 = new TextObject("{=OyyJSyIX}{FACTION_1} is paying {DENAR}{GOLD_ICON} as tribute to {FACTION_2}");
		TextObject textObject3 = textObject2.CopyTextObject();
		Faction1TributeHint = ((dailyTributePaid > 0) ? new HintViewModel(textObject2.SetTextVariable("DENAR", MathF.Abs(dailyTributePaid)).SetTextVariable("FACTION_1", Faction1Name).SetTextVariable("FACTION_2", Faction2Name)) : new HintViewModel());
		Faction2TributeHint = ((dailyTributePaid < 0) ? new HintViewModel(textObject3.SetTextVariable("DENAR", MathF.Abs(dailyTributePaid)).SetTextVariable("FACTION_1", Faction2Name).SetTextVariable("FACTION_2", Faction1Name)) : new HintViewModel());
		Faction1Leader = new HeroVM(Faction1.Leader);
		Faction2Leader = new HeroVM(Faction2.Leader);
		Faction1OwnedClans = new MBBindingList<KingdomDiplomacyFactionItemVM>();
		if (Faction1.IsKingdomFaction)
		{
			foreach (Clan clan in (Faction1 as Kingdom).Clans)
			{
				Faction1OwnedClans.Add(new KingdomDiplomacyFactionItemVM(clan));
			}
		}
		Faction2OwnedClans = new MBBindingList<KingdomDiplomacyFactionItemVM>();
		if (Faction2.IsKingdomFaction)
		{
			foreach (Clan clan2 in (Faction2 as Kingdom).Clans)
			{
				Faction2OwnedClans.Add(new KingdomDiplomacyFactionItemVM(clan2));
			}
		}
		Faction2OtherWars = new MBBindingList<KingdomDiplomacyFactionItemVM>();
		foreach (StanceLink stance in Faction2.Stances)
		{
			if (stance.IsAtWar && stance.Faction1 != Faction1 && stance.Faction2 != Faction1 && (stance.Faction1.IsKingdomFaction || stance.Faction1.Leader == Hero.MainHero) && (stance.Faction2.IsKingdomFaction || stance.Faction2.Leader == Hero.MainHero) && !stance.Faction1.IsRebelClan && !stance.Faction2.IsRebelClan && !stance.Faction1.IsBanditFaction && !stance.Faction2.IsBanditFaction)
			{
				Faction2OtherWars.Add(new KingdomDiplomacyFactionItemVM((stance.Faction1 == Faction2) ? stance.Faction2 : stance.Faction1));
			}
		}
		IsFaction2OtherWarsVisible = Faction2OtherWars.Count > 0;
	}

	private void PopulateSettlements()
	{
		_faction1Towns = new List<Settlement>();
		_faction1Castles = new List<Settlement>();
		_faction2Towns = new List<Settlement>();
		_faction2Castles = new List<Settlement>();
		foreach (Settlement settlement in Faction1.Settlements)
		{
			if (settlement.IsTown)
			{
				_faction1Towns.Add(settlement);
			}
			else if (settlement.IsCastle)
			{
				_faction1Castles.Add(settlement);
			}
		}
		foreach (Settlement settlement2 in Faction2.Settlements)
		{
			if (settlement2.IsTown)
			{
				_faction2Towns.Add(settlement2);
			}
			else if (settlement2.IsCastle)
			{
				_faction2Castles.Add(settlement2);
			}
		}
	}

	protected abstract void ExecuteAction();
}
