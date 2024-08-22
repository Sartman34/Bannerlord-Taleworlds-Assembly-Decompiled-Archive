using System;
using System.Linq;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Decisions.ItemTypes;

public class DeclareWarDecisionItemVM : DecisionItemBaseVM
{
	private readonly DeclareWarDecision _declareWarDecision;

	private string _nameText;

	private string _warDescriptionText;

	private ImageIdentifierVM _sourceFactionBanner;

	private ImageIdentifierVM _targetFactionBanner;

	private string _leaderText;

	private HeroVM _sourceFactionLeader;

	private HeroVM _targetFactionLeader;

	private MBBindingList<KingdomWarComparableStatVM> _comparedStats;

	private Kingdom _sourceFaction => Hero.MainHero.Clan.Kingdom;

	public IFaction TargetFaction => (_decision as DeclareWarDecision).FactionToDeclareWarOn;

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
	public string WarDescriptionText
	{
		get
		{
			return _warDescriptionText;
		}
		set
		{
			if (value != _warDescriptionText)
			{
				_warDescriptionText = value;
				OnPropertyChangedWithValue(value, "WarDescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public ImageIdentifierVM SourceFactionBanner
	{
		get
		{
			return _sourceFactionBanner;
		}
		set
		{
			if (value != _sourceFactionBanner)
			{
				_sourceFactionBanner = value;
				OnPropertyChangedWithValue(value, "SourceFactionBanner");
			}
		}
	}

	[DataSourceProperty]
	public ImageIdentifierVM TargetFactionBanner
	{
		get
		{
			return _targetFactionBanner;
		}
		set
		{
			if (value != _targetFactionBanner)
			{
				_targetFactionBanner = value;
				OnPropertyChangedWithValue(value, "TargetFactionBanner");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<KingdomWarComparableStatVM> ComparedStats
	{
		get
		{
			return _comparedStats;
		}
		set
		{
			if (value != _comparedStats)
			{
				_comparedStats = value;
				OnPropertyChangedWithValue(value, "ComparedStats");
			}
		}
	}

	[DataSourceProperty]
	public string LeaderText
	{
		get
		{
			return _leaderText;
		}
		set
		{
			if (value != _leaderText)
			{
				_leaderText = value;
				OnPropertyChangedWithValue(value, "LeaderText");
			}
		}
	}

	[DataSourceProperty]
	public HeroVM SourceFactionLeader
	{
		get
		{
			return _sourceFactionLeader;
		}
		set
		{
			if (value != _sourceFactionLeader)
			{
				_sourceFactionLeader = value;
				OnPropertyChangedWithValue(value, "SourceFactionLeader");
			}
		}
	}

	[DataSourceProperty]
	public HeroVM TargetFactionLeader
	{
		get
		{
			return _targetFactionLeader;
		}
		set
		{
			if (value != _targetFactionLeader)
			{
				_targetFactionLeader = value;
				OnPropertyChangedWithValue(value, "TargetFactionLeader");
			}
		}
	}

	public DeclareWarDecisionItemVM(DeclareWarDecision decision, Action onDecisionOver)
		: base(decision, onDecisionOver)
	{
		_declareWarDecision = decision;
		base.DecisionType = 4;
	}

	protected override void InitValues()
	{
		base.InitValues();
		TextObject textObject = GameTexts.FindText("str_kingdom_decision_declare_war");
		NameText = textObject.ToString();
		TextObject textObject2 = GameTexts.FindText("str_kingdom_decision_declare_war_desc");
		textObject2.SetTextVariable("FACTION", TargetFaction.Name);
		WarDescriptionText = textObject2.ToString();
		SourceFactionBanner = new ImageIdentifierVM(BannerCode.CreateFrom(_sourceFaction.Banner), nineGrid: true);
		TargetFactionBanner = new ImageIdentifierVM(BannerCode.CreateFrom(TargetFaction.Banner), nineGrid: true);
		LeaderText = GameTexts.FindText("str_leader").ToString();
		SourceFactionLeader = new HeroVM(_sourceFaction.Leader);
		TargetFactionLeader = new HeroVM(TargetFaction.Leader);
		ComparedStats = new MBBindingList<KingdomWarComparableStatVM>();
		Kingdom kingdom = TargetFaction as Kingdom;
		string faction1Color = Color.FromUint(_sourceFaction.Color).ToString();
		string faction2Color = Color.FromUint(kingdom.Color).ToString();
		KingdomWarComparableStatVM item = new KingdomWarComparableStatVM((int)_sourceFaction.TotalStrength, (int)kingdom.TotalStrength, GameTexts.FindText("str_strength"), faction1Color, faction2Color, 10000);
		ComparedStats.Add(item);
		KingdomWarComparableStatVM item2 = new KingdomWarComparableStatVM(_sourceFaction.Armies.Count, kingdom.Armies.Count, GameTexts.FindText("str_armies"), faction1Color, faction2Color, 5);
		ComparedStats.Add(item2);
		int faction1Stat = _sourceFaction.Settlements.Count((Settlement settlement) => settlement.IsTown);
		int faction2Stat = kingdom.Settlements.Count((Settlement settlement) => settlement.IsTown);
		KingdomWarComparableStatVM item3 = new KingdomWarComparableStatVM(faction1Stat, faction2Stat, GameTexts.FindText("str_towns"), faction1Color, faction2Color, 50);
		ComparedStats.Add(item3);
		int faction1Stat2 = _sourceFaction.Settlements.Count((Settlement settlement) => settlement.IsCastle);
		int faction2Stat2 = TargetFaction.Settlements.Count((Settlement settlement) => settlement.IsCastle);
		KingdomWarComparableStatVM item4 = new KingdomWarComparableStatVM(faction1Stat2, faction2Stat2, GameTexts.FindText("str_castles"), faction1Color, faction2Color, 50);
		ComparedStats.Add(item4);
	}
}
