using System;
using System.Linq;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Decisions.ItemTypes;

public class MakePeaceDecisionItemVM : DecisionItemBaseVM
{
	private readonly MakePeaceKingdomDecision _makePeaceDecision;

	private string _nameText;

	private string _peaceDescriptionText;

	private ImageIdentifierVM _sourceFactionBanner;

	private ImageIdentifierVM _targetFactionBanner;

	private string _leaderText;

	private HeroVM _sourceFactionLeader;

	private HeroVM _targetFactionLeader;

	private MBBindingList<KingdomWarComparableStatVM> _comparedStats;

	private Kingdom _sourceFaction => Hero.MainHero.Clan.Kingdom;

	public IFaction TargetFaction => (_decision as MakePeaceKingdomDecision).FactionToMakePeaceWith;

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
	public string PeaceDescriptionText
	{
		get
		{
			return _peaceDescriptionText;
		}
		set
		{
			if (value != _peaceDescriptionText)
			{
				_peaceDescriptionText = value;
				OnPropertyChangedWithValue(value, "PeaceDescriptionText");
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

	public MakePeaceDecisionItemVM(MakePeaceKingdomDecision decision, Action onDecisionOver)
		: base(decision, onDecisionOver)
	{
		_makePeaceDecision = decision;
		base.DecisionType = 5;
	}

	protected override void InitValues()
	{
		base.InitValues();
		TextObject textObject = GameTexts.FindText("str_kingdom_decision_make_peace");
		NameText = textObject.ToString();
		TextObject textObject2 = GameTexts.FindText("str_kingdom_decision_make_peace_desc");
		textObject2.SetTextVariable("FACTION", TargetFaction.Name);
		PeaceDescriptionText = textObject2.ToString();
		SourceFactionBanner = new ImageIdentifierVM(BannerCode.CreateFrom(_sourceFaction.Banner), nineGrid: true);
		TargetFactionBanner = new ImageIdentifierVM(BannerCode.CreateFrom(TargetFaction.Banner), nineGrid: true);
		LeaderText = GameTexts.FindText("str_leader").ToString();
		SourceFactionLeader = new HeroVM(_sourceFaction.Leader);
		TargetFactionLeader = new HeroVM(TargetFaction.Leader);
		ComparedStats = new MBBindingList<KingdomWarComparableStatVM>();
		Kingdom targetFaction = TargetFaction as Kingdom;
		string faction1Color = Color.FromUint(_sourceFaction.Color).ToString();
		string faction2Color = Color.FromUint(targetFaction.Color).ToString();
		StanceLink stanceLink = _sourceFaction.Stances.First((StanceLink s) => s.IsAtWar && (s.Faction2 == TargetFaction || s.Faction1 == TargetFaction));
		KingdomWarComparableStatVM item = new KingdomWarComparableStatVM((int)_sourceFaction.TotalStrength, (int)targetFaction.TotalStrength, GameTexts.FindText("str_strength"), faction1Color, faction2Color, 10000);
		ComparedStats.Add(item);
		int faction1Stat = targetFaction.Heroes.Count((Hero x) => x.IsPrisoner && x.PartyBelongedToAsPrisoner?.MapFaction == _sourceFaction);
		int faction2Stat = _sourceFaction.Heroes.Count((Hero x) => x.IsPrisoner && x.PartyBelongedToAsPrisoner?.MapFaction == targetFaction);
		KingdomWarComparableStatVM item2 = new KingdomWarComparableStatVM(faction1Stat, faction2Stat, GameTexts.FindText("str_party_category_prisoners_tooltip"), faction1Color, faction2Color, 10);
		ComparedStats.Add(item2);
		KingdomWarComparableStatVM item3 = new KingdomWarComparableStatVM(stanceLink.GetCasualties(targetFaction), stanceLink.GetCasualties(_sourceFaction), GameTexts.FindText("str_war_casualties_inflicted"), faction1Color, faction2Color, 5000);
		ComparedStats.Add(item3);
		KingdomWarComparableStatVM item4 = new KingdomWarComparableStatVM(stanceLink.GetSuccessfulSieges(_sourceFaction), stanceLink.GetSuccessfulSieges(targetFaction), GameTexts.FindText("str_war_successful_sieges"), faction1Color, faction2Color, 5);
		ComparedStats.Add(item4);
		KingdomWarComparableStatVM item5 = new KingdomWarComparableStatVM(stanceLink.GetSuccessfulRaids(_sourceFaction), stanceLink.GetSuccessfulRaids(targetFaction), GameTexts.FindText("str_war_successful_raids"), faction1Color, faction2Color, 10);
		ComparedStats.Add(item5);
	}
}
