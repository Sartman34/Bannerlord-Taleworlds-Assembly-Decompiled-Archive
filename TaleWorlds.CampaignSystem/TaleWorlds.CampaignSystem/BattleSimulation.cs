using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem;

public class BattleSimulation : IBattleObserver
{
	private enum SimulationState
	{
		Play,
		FastForward,
		Skip
	}

	private readonly MapEvent _mapEvent;

	public List<TroopRoster> tempRosterRefs;

	private IBattleObserver _battleObserver;

	private Timer _timer;

	public readonly FlattenedTroopRoster[] SelectedTroops = new FlattenedTroopRoster[2];

	private SimulationState _simulationState;

	private float _numTicks;

	public bool IsSimulationPaused { get; private set; }

	public bool IsSimulationFinished { get; private set; }

	private bool IsPlayerJoinedBattle => PlayerEncounter.Current.IsJoinedBattle;

	public MapEvent MapEvent => _mapEvent;

	public IBattleObserver BattleObserver
	{
		get
		{
			return _battleObserver;
		}
		set
		{
			_battleObserver = value;
		}
	}

	public List<List<BattleResultPartyData>> Teams { get; private set; }

	public BattleSimulation(FlattenedTroopRoster selectedTroopsForPlayerSide, FlattenedTroopRoster selectedTroopsForOtherSide)
	{
		IsSimulationPaused = true;
		float applicationTime = Game.Current.ApplicationTime;
		_timer = new Timer(applicationTime, 1f);
		_mapEvent = PlayerEncounter.Battle ?? PlayerEncounter.StartBattle();
		_mapEvent.IsPlayerSimulation = true;
		_mapEvent.BattleObserver = this;
		SelectedTroops[(int)_mapEvent.PlayerSide] = selectedTroopsForPlayerSide;
		SelectedTroops[(int)_mapEvent.GetOtherSide(_mapEvent.PlayerSide)] = selectedTroopsForOtherSide;
		_mapEvent.GetNumberOfInvolvedMen();
		if (_mapEvent.IsSiegeAssault)
		{
			PlayerSiege.StartPlayerSiege(MobileParty.MainParty.Party.Side, isSimulation: true, _mapEvent.MapEventSettlement);
		}
		List<List<BattleResultPartyData>> list = new List<List<BattleResultPartyData>>
		{
			new List<BattleResultPartyData>(),
			new List<BattleResultPartyData>()
		};
		foreach (PartyBase involvedParty in _mapEvent.InvolvedParties)
		{
			BattleResultPartyData item = default(BattleResultPartyData);
			bool flag = false;
			foreach (BattleResultPartyData item2 in list[(int)involvedParty.Side])
			{
				if (item2.Party == involvedParty)
				{
					flag = true;
					item = item2;
					break;
				}
			}
			if (!flag)
			{
				item = new BattleResultPartyData(involvedParty);
				list[(int)involvedParty.Side].Add(item);
			}
			for (int i = 0; i < involvedParty.MemberRoster.Count; i++)
			{
				TroopRosterElement elementCopyAtIndex = involvedParty.MemberRoster.GetElementCopyAtIndex(i);
				if (!item.Characters.Contains(elementCopyAtIndex.Character))
				{
					item.Characters.Add(elementCopyAtIndex.Character);
				}
			}
		}
		Teams = list;
		tempRosterRefs = new List<TroopRoster>();
		foreach (PartyBase involvedParty2 in _mapEvent.InvolvedParties)
		{
			tempRosterRefs.Add(involvedParty2.MemberRoster);
		}
	}

	public void Play()
	{
		_simulationState = SimulationState.Play;
	}

	public void FastForward()
	{
		_simulationState = SimulationState.FastForward;
	}

	public void Skip()
	{
		_simulationState = SimulationState.Skip;
	}

	public void OnReturn()
	{
		foreach (PartyBase involvedParty in _mapEvent.InvolvedParties)
		{
			involvedParty.MemberRoster.RemoveZeroCounts();
		}
		GameMenu.ActivateGameMenu("encounter");
	}

	private void BattleEndLogic()
	{
		if (PlayerEncounter.Battle != null)
		{
			BattleSideEnum side = PartyBase.MainParty.Side;
			if (PlayerEncounter.Battle.GetMapEventSide(side).NumRemainingSimulationTroops > 0)
			{
				GameMenu.SwitchToMenu("encounter");
			}
			else
			{
				PlayerEncounter.Finish();
			}
		}
		else
		{
			GameMenu.SwitchToMenu("encounter");
		}
	}

	private void TickSimulateBattle()
	{
		SimulateBattleFromUi();
	}

	public void Tick(float dt)
	{
		if (IsSimulationFinished)
		{
			return;
		}
		if (PlayerEncounter.Current == null)
		{
			Debug.FailedAssert("PlayerEncounter.Current == null", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\BattleSimulation.cs", "Tick", 222);
			IsSimulationFinished = true;
			return;
		}
		int num = 0;
		if (PlayerEncounter.BattleState == BattleState.None)
		{
			foreach (PartyBase involvedParty in MapEvent.InvolvedParties)
			{
				if (involvedParty.Side == MobileParty.MainParty.Party.Side && involvedParty != MobileParty.MainParty.Party)
				{
					num += involvedParty.NumberOfHealthyMembers;
				}
			}
		}
		if ((MobileParty.MainParty.MapEvent == MapEvent && MobileParty.MainParty.Party.NumberOfHealthyMembers == 1 && !Hero.MainHero.IsWounded && num == 0) || PlayerEncounter.BattleState == BattleState.AttackerVictory || PlayerEncounter.BattleState == BattleState.DefenderVictory)
		{
			IsSimulationFinished = true;
		}
		else if (_simulationState == SimulationState.Skip)
		{
			while (PlayerEncounter.BattleState == BattleState.None || PlayerEncounter.BattleState == BattleState.DefenderPullBack)
			{
				TickSimulateBattle();
				num = 0;
				if (PlayerEncounter.BattleState == BattleState.None || PlayerEncounter.BattleState == BattleState.DefenderPullBack)
				{
					foreach (PartyBase involvedParty2 in MapEvent.InvolvedParties)
					{
						if (involvedParty2.Side == MobileParty.MainParty.Party.Side && involvedParty2 != MobileParty.MainParty.Party)
						{
							num += involvedParty2.NumberOfHealthyMembers;
						}
					}
				}
				if (MobileParty.MainParty.MapEvent == MapEvent && MobileParty.MainParty.Party.NumberOfHealthyMembers <= 1 && num == 0)
				{
					break;
				}
			}
		}
		else
		{
			_numTicks += dt;
			if (_simulationState == SimulationState.FastForward)
			{
				_numTicks *= 3f;
			}
			while (_numTicks >= 1f)
			{
				TickSimulateBattle();
				_numTicks -= 1f;
			}
		}
	}

	public static void SimulateBattleFromUi()
	{
		PlayerEncounter.SimulateBattle();
	}

	public void ResetSimulation()
	{
		MapEvent.SimulateBattleSetup(PlayerEncounter.CurrentBattleSimulation.SelectedTroops);
	}

	public void TroopNumberChanged(BattleSideEnum side, IBattleCombatant battleCombatant, BasicCharacterObject character, int number = 0, int numberKilled = 0, int numberWounded = 0, int numberRouted = 0, int killCount = 0, int numberReadyToUpgrade = 0)
	{
		BattleObserver?.TroopNumberChanged(side, battleCombatant, character, number, numberKilled, numberWounded, numberRouted, killCount, numberReadyToUpgrade);
	}

	public void HeroSkillIncreased(BattleSideEnum side, IBattleCombatant battleCombatant, BasicCharacterObject heroCharacter, SkillObject skill)
	{
		BattleObserver?.HeroSkillIncreased(side, battleCombatant, heroCharacter, skill);
	}

	public void BattleResultsReady()
	{
		BattleObserver?.BattleResultsReady();
	}

	public void TroopSideChanged(BattleSideEnum prevSide, BattleSideEnum newSide, IBattleCombatant battleCombatant, BasicCharacterObject character)
	{
		BattleObserver?.TroopSideChanged(prevSide, newSide, battleCombatant, character);
	}
}
