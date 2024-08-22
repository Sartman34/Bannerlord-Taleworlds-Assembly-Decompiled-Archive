using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.Scoreboard;

public class SPScoreboardSideVM : ViewModel
{
	private TextObject _nameTextObject;

	private MBBindingList<SPScoreboardPartyVM> _parties;

	private SPScoreboardStatsVM _score;

	private ImageIdentifierVM _bannerVisual;

	private ImageIdentifierVM _bannerVisualSmall;

	private SPScoreboardSortControllerVM _sortController;

	public float CurrentPower { get; private set; }

	public float InitialPower { get; private set; }

	[DataSourceProperty]
	public ImageIdentifierVM BannerVisual
	{
		get
		{
			return _bannerVisual;
		}
		set
		{
			if (value != _bannerVisual)
			{
				_bannerVisual = value;
				OnPropertyChangedWithValue(value, "BannerVisual");
			}
		}
	}

	[DataSourceProperty]
	public ImageIdentifierVM BannerVisualSmall
	{
		get
		{
			return _bannerVisualSmall;
		}
		set
		{
			if (value != _bannerVisualSmall)
			{
				_bannerVisualSmall = value;
				OnPropertyChangedWithValue(value, "BannerVisualSmall");
			}
		}
	}

	[DataSourceProperty]
	public SPScoreboardStatsVM Score
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
	public MBBindingList<SPScoreboardPartyVM> Parties
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
	public SPScoreboardSortControllerVM SortController
	{
		get
		{
			return _sortController;
		}
		set
		{
			if (value != _sortController)
			{
				_sortController = value;
				OnPropertyChanged("SortController");
			}
		}
	}

	public SPScoreboardSideVM(TextObject name, Banner sideFlag)
	{
		_nameTextObject = name;
		Parties = new MBBindingList<SPScoreboardPartyVM>();
		Score = new SPScoreboardStatsVM(_nameTextObject);
		MBBindingList<SPScoreboardPartyVM> listToControl = Parties;
		SortController = new SPScoreboardSortControllerVM(ref listToControl);
		Parties = listToControl;
		if (sideFlag != null)
		{
			BannerCode bannerCode = BannerCode.CreateFrom(sideFlag);
			BannerVisual = new ImageIdentifierVM(bannerCode, nineGrid: true);
			BannerVisualSmall = new ImageIdentifierVM(bannerCode);
		}
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		Score.RefreshValues();
		Parties.ApplyActionOnAllItems(delegate(SPScoreboardPartyVM x)
		{
			x.RefreshValues();
		});
	}

	public void UpdateScores(IBattleCombatant battleCombatant, bool isPlayerParty, BasicCharacterObject character, int numberRemaining, int numberDead, int numberWounded, int numberRouted, int numberKilled, int numberReadyToUpgrade)
	{
		GetPartyAddIfNotExists(battleCombatant, isPlayerParty).UpdateScores(character, numberRemaining, numberDead, numberWounded, numberRouted, numberKilled, numberReadyToUpgrade);
		Score.UpdateScores(numberRemaining, numberDead, numberWounded, numberRouted, numberKilled, numberReadyToUpgrade);
		RefreshPower();
	}

	public void UpdateHeroSkills(IBattleCombatant battleCombatant, bool isPlayerParty, BasicCharacterObject heroCharacter, SkillObject upgradedSkill)
	{
		GetPartyAddIfNotExists(battleCombatant, isPlayerParty).UpdateHeroSkills(heroCharacter, upgradedSkill);
	}

	public SPScoreboardPartyVM GetPartyAddIfNotExists(IBattleCombatant battleCombatant, bool isPlayerParty)
	{
		SPScoreboardPartyVM sPScoreboardPartyVM = Parties.FirstOrDefault((SPScoreboardPartyVM p) => p.BattleCombatant == battleCombatant);
		if (sPScoreboardPartyVM == null)
		{
			sPScoreboardPartyVM = new SPScoreboardPartyVM(battleCombatant);
			if (isPlayerParty)
			{
				Parties.Insert(0, sPScoreboardPartyVM);
			}
			else
			{
				Parties.Add(sPScoreboardPartyVM);
			}
		}
		return sPScoreboardPartyVM;
	}

	public SPScoreboardPartyVM GetParty(IBattleCombatant battleCombatant)
	{
		return Parties.FirstOrDefault((SPScoreboardPartyVM p) => p.BattleCombatant == battleCombatant);
	}

	public SPScoreboardStatsVM RemoveTroop(IBattleCombatant battleCombatant, BasicCharacterObject troop)
	{
		SPScoreboardPartyVM sPScoreboardPartyVM = Parties.FirstOrDefault((SPScoreboardPartyVM p) => p.BattleCombatant == battleCombatant);
		SPScoreboardStatsVM sPScoreboardStatsVM = sPScoreboardPartyVM.RemoveUnit(troop);
		if (sPScoreboardPartyVM.Members.Count == 0)
		{
			Parties.Remove(sPScoreboardPartyVM);
		}
		Score.UpdateScores(-sPScoreboardStatsVM.Remaining, -sPScoreboardStatsVM.Dead, -sPScoreboardStatsVM.Wounded, -sPScoreboardStatsVM.Routed, -sPScoreboardStatsVM.Kill, -sPScoreboardStatsVM.ReadyToUpgrade);
		return sPScoreboardStatsVM;
	}

	public void AddTroop(IBattleCombatant battleCombatant, BasicCharacterObject currentTroop, SPScoreboardStatsVM scoreToBringOver)
	{
		Parties.FirstOrDefault((SPScoreboardPartyVM p) => p.BattleCombatant == battleCombatant).AddUnit(currentTroop, scoreToBringOver);
		Score.UpdateScores(scoreToBringOver.Remaining, scoreToBringOver.Dead, scoreToBringOver.Wounded, scoreToBringOver.Routed, scoreToBringOver.Kill, scoreToBringOver.ReadyToUpgrade);
	}

	private void RefreshPower()
	{
		CurrentPower = 0f;
		InitialPower = 0f;
		foreach (SPScoreboardPartyVM party in _parties)
		{
			InitialPower += party.InitialPower;
			CurrentPower += party.CurrentPower;
		}
	}
}
