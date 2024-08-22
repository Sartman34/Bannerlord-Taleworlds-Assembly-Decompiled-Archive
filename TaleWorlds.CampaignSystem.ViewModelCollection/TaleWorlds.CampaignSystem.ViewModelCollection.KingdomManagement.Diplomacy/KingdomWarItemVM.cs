using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;

public class KingdomWarItemVM : KingdomDiplomacyItemVM
{
	private readonly Action<KingdomWarItemVM> _onSelect;

	private readonly StanceLink _war;

	private readonly Action<KingdomWarItemVM> _onAction;

	private List<Hero> _prisonersCapturedByFaction1;

	private List<Hero> _prisonersCapturedByFaction2;

	private List<Settlement> _townsCapturedByFaction1;

	private List<Settlement> _townsCapturedByFaction2;

	private List<Settlement> _castlesCapturedByFaction1;

	private List<Settlement> _castlesCapturedByFaction2;

	private List<Settlement> _raidsMadeByFaction1;

	private List<Settlement> _raidsMadeByFaction2;

	private string _warName;

	private string _numberOfDaysSinceWarBegan;

	private int _score;

	private bool _isBehaviorSelectionEnabled;

	private int _casualtiesOfFaction1;

	private int _casualtiesOfFaction2;

	private MBBindingList<KingdomWarLogItemVM> _warLog;

	[DataSourceProperty]
	public string WarName
	{
		get
		{
			return _warName;
		}
		set
		{
			if (value != _warName)
			{
				_warName = value;
				OnPropertyChangedWithValue(value, "WarName");
			}
		}
	}

	[DataSourceProperty]
	public string NumberOfDaysSinceWarBegan
	{
		get
		{
			return _numberOfDaysSinceWarBegan;
		}
		set
		{
			if (value != _numberOfDaysSinceWarBegan)
			{
				_numberOfDaysSinceWarBegan = value;
				OnPropertyChangedWithValue(value, "NumberOfDaysSinceWarBegan");
			}
		}
	}

	[DataSourceProperty]
	public bool IsBehaviorSelectionEnabled
	{
		get
		{
			return _isBehaviorSelectionEnabled;
		}
		set
		{
			if (value != _isBehaviorSelectionEnabled)
			{
				_isBehaviorSelectionEnabled = value;
				OnPropertyChangedWithValue(value, "IsBehaviorSelectionEnabled");
			}
		}
	}

	[DataSourceProperty]
	public int Score
	{
		get
		{
			return _score;
		}
		set
		{
			if (value != _score)
			{
				_score = value;
				OnPropertyChangedWithValue(value, "Score");
			}
		}
	}

	[DataSourceProperty]
	public int CasualtiesOfFaction1
	{
		get
		{
			return _casualtiesOfFaction1;
		}
		set
		{
			if (value != _casualtiesOfFaction1)
			{
				_casualtiesOfFaction1 = value;
				OnPropertyChangedWithValue(value, "CasualtiesOfFaction1");
			}
		}
	}

	[DataSourceProperty]
	public int CasualtiesOfFaction2
	{
		get
		{
			return _casualtiesOfFaction2;
		}
		set
		{
			if (value != _casualtiesOfFaction2)
			{
				_casualtiesOfFaction2 = value;
				OnPropertyChangedWithValue(value, "CasualtiesOfFaction2");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<KingdomWarLogItemVM> WarLog
	{
		get
		{
			return _warLog;
		}
		set
		{
			if (value != _warLog)
			{
				_warLog = value;
				OnPropertyChangedWithValue(value, "WarLog");
			}
		}
	}

	public KingdomWarItemVM(StanceLink war, Action<KingdomWarItemVM> onSelect, Action<KingdomWarItemVM> onAction)
		: base(war.Faction1, war.Faction2)
	{
		_war = war;
		_onSelect = onSelect;
		_onAction = onAction;
		IsBehaviorSelectionEnabled = Faction1.IsKingdomFaction && Faction1.Leader == Hero.MainHero;
		_prisonersCapturedByFaction1 = DiplomacyHelper.GetPrisonersOfWarTakenByFaction(Faction1, Faction2);
		_prisonersCapturedByFaction2 = DiplomacyHelper.GetPrisonersOfWarTakenByFaction(Faction2, Faction1);
		_townsCapturedByFaction1 = DiplomacyHelper.GetSuccessfullSiegesInWarForFaction(Faction1, war, (Settlement x) => x.IsTown);
		_townsCapturedByFaction2 = DiplomacyHelper.GetSuccessfullSiegesInWarForFaction(Faction2, war, (Settlement x) => x.IsTown);
		_castlesCapturedByFaction1 = DiplomacyHelper.GetSuccessfullSiegesInWarForFaction(Faction1, war, (Settlement x) => x.IsCastle);
		_castlesCapturedByFaction2 = DiplomacyHelper.GetSuccessfullSiegesInWarForFaction(Faction2, war, (Settlement x) => x.IsCastle);
		_raidsMadeByFaction1 = DiplomacyHelper.GetRaidsInWar(Faction1, war);
		_raidsMadeByFaction2 = DiplomacyHelper.GetRaidsInWar(Faction2, war);
		RefreshValues();
		WarLog = new MBBindingList<KingdomWarLogItemVM>();
		foreach (var (logEntry, effectorFaction, _) in DiplomacyHelper.GetLogsForWar(war))
		{
			if (logEntry is IEncyclopediaLog log)
			{
				WarLog.Add(new KingdomWarLogItemVM(log, effectorFaction));
			}
		}
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		UpdateDiplomacyProperties();
	}

	protected override void OnSelect()
	{
		UpdateDiplomacyProperties();
		_onSelect(this);
		base.IsSelected = true;
	}

	protected override void UpdateDiplomacyProperties()
	{
		base.UpdateDiplomacyProperties();
		GameTexts.SetVariable("FACTION_1_NAME", Faction1.Name.ToString());
		GameTexts.SetVariable("FACTION_2_NAME", Faction2.Name.ToString());
		WarName = GameTexts.FindText("str_war_faction_versus_faction").ToString();
		StanceLink stanceWith = Faction1.GetStanceWith(Faction2);
		Score = stanceWith.GetSuccessfulSieges(Faction1) + stanceWith.GetSuccessfulRaids(Faction1);
		CasualtiesOfFaction1 = stanceWith.GetCasualties(Faction1);
		CasualtiesOfFaction2 = stanceWith.GetCasualties(Faction2);
		int num = TaleWorlds.Library.MathF.Ceiling(_war.WarStartDate.ElapsedDaysUntilNow + 0.01f);
		TextObject textObject = GameTexts.FindText("str_for_DAY_days");
		textObject.SetTextVariable("DAY", num.ToString());
		textObject.SetTextVariable("DAY_IS_PLURAL", (num > 1) ? 1 : 0);
		NumberOfDaysSinceWarBegan = textObject.ToString();
		base.Stats.Add(new KingdomWarComparableStatVM((int)Faction1.TotalStrength, (int)Faction2.TotalStrength, GameTexts.FindText("str_total_strength"), _faction1Color, _faction2Color, 10000));
		base.Stats.Add(new KingdomWarComparableStatVM(stanceWith.GetCasualties(Faction2), stanceWith.GetCasualties(Faction1), GameTexts.FindText("str_war_casualties_inflicted"), _faction1Color, _faction2Color, 5000));
		base.Stats.Add(new KingdomWarComparableStatVM(_prisonersCapturedByFaction1.Count, _prisonersCapturedByFaction2.Count, GameTexts.FindText("str_party_category_prisoners_tooltip"), _faction1Color, _faction2Color, 10, new BasicTooltipViewModel(() => CampaignUIHelper.GetWarPrisonersTooltip(_prisonersCapturedByFaction1, Faction1.Name)), new BasicTooltipViewModel(() => CampaignUIHelper.GetWarPrisonersTooltip(_prisonersCapturedByFaction2, Faction2.Name))));
		base.Stats.Add(new KingdomWarComparableStatVM(_faction1Towns.Count, _faction2Towns.Count, GameTexts.FindText("str_towns"), _faction1Color, _faction2Color, 25, new BasicTooltipViewModel(() => CampaignUIHelper.GetWarSuccessfulSiegesTooltip(_townsCapturedByFaction1, Faction1.Name, isTown: true)), new BasicTooltipViewModel(() => CampaignUIHelper.GetWarSuccessfulSiegesTooltip(_townsCapturedByFaction2, Faction2.Name, isTown: true))));
		base.Stats.Add(new KingdomWarComparableStatVM(_faction1Castles.Count, _faction2Castles.Count, GameTexts.FindText("str_castles"), _faction1Color, _faction2Color, 25, new BasicTooltipViewModel(() => CampaignUIHelper.GetWarSuccessfulSiegesTooltip(_castlesCapturedByFaction1, Faction1.Name, isTown: false)), new BasicTooltipViewModel(() => CampaignUIHelper.GetWarSuccessfulSiegesTooltip(_castlesCapturedByFaction2, Faction2.Name, isTown: false))));
		base.Stats.Add(new KingdomWarComparableStatVM(stanceWith.GetSuccessfulRaids(Faction1), stanceWith.GetSuccessfulRaids(Faction2), GameTexts.FindText("str_war_successful_raids"), _faction1Color, _faction2Color, 10, new BasicTooltipViewModel(() => CampaignUIHelper.GetWarSuccessfulRaidsTooltip(_raidsMadeByFaction1, Faction1.Name)), new BasicTooltipViewModel(() => CampaignUIHelper.GetWarSuccessfulRaidsTooltip(_raidsMadeByFaction2, Faction2.Name))));
	}

	protected override void ExecuteAction()
	{
		_onAction(this);
	}
}
