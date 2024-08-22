using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade;

public class MissionCombatantsLogic : MissionLogic
{
	private readonly IEnumerable<IBattleCombatant> _battleCombatants;

	private readonly IBattleCombatant _playerBattleCombatant;

	private readonly IBattleCombatant _defenderLeaderBattleCombatant;

	private readonly IBattleCombatant _attackerLeaderBattleCombatant;

	private readonly Mission.MissionTeamAITypeEnum _teamAIType;

	private readonly bool _isPlayerSergeant;

	public BattleSideEnum PlayerSide
	{
		get
		{
			if (_playerBattleCombatant == null)
			{
				return BattleSideEnum.None;
			}
			if (_playerBattleCombatant != _defenderLeaderBattleCombatant)
			{
				return BattleSideEnum.Attacker;
			}
			return BattleSideEnum.Defender;
		}
	}

	public MissionCombatantsLogic(IEnumerable<IBattleCombatant> battleCombatants, IBattleCombatant playerBattleCombatant, IBattleCombatant defenderLeaderBattleCombatant, IBattleCombatant attackerLeaderBattleCombatant, Mission.MissionTeamAITypeEnum teamAIType, bool isPlayerSergeant)
	{
		if (battleCombatants == null)
		{
			battleCombatants = new IBattleCombatant[2] { defenderLeaderBattleCombatant, attackerLeaderBattleCombatant };
		}
		_battleCombatants = battleCombatants;
		_playerBattleCombatant = playerBattleCombatant;
		_defenderLeaderBattleCombatant = defenderLeaderBattleCombatant;
		_attackerLeaderBattleCombatant = attackerLeaderBattleCombatant;
		_teamAIType = teamAIType;
		_isPlayerSergeant = isPlayerSergeant;
	}

	public Banner GetBannerForSide(BattleSideEnum side)
	{
		if (side != 0)
		{
			return _attackerLeaderBattleCombatant.Banner;
		}
		return _defenderLeaderBattleCombatant.Banner;
	}

	public override void OnBehaviorInitialize()
	{
		base.OnBehaviorInitialize();
		if (!base.Mission.Teams.IsEmpty())
		{
			throw new MBIllegalValueException("Number of teams is not 0.");
		}
		BattleSideEnum side = _playerBattleCombatant.Side;
		BattleSideEnum oppositeSide = side.GetOppositeSide();
		if (side == BattleSideEnum.Defender)
		{
			AddPlayerTeam(side);
		}
		else
		{
			AddEnemyTeam(oppositeSide);
		}
		if (side == BattleSideEnum.Attacker)
		{
			AddPlayerTeam(side);
		}
		else
		{
			AddEnemyTeam(oppositeSide);
		}
		AddPlayerAllyTeam(side);
	}

	public override void EarlyStart()
	{
		Mission.Current.MissionTeamAIType = _teamAIType;
		switch (_teamAIType)
		{
		case Mission.MissionTeamAITypeEnum.FieldBattle:
			foreach (Team team2 in Mission.Current.Teams)
			{
				team2.AddTeamAI(new TeamAIGeneral(base.Mission, team2));
			}
			break;
		case Mission.MissionTeamAITypeEnum.Siege:
			foreach (Team team3 in Mission.Current.Teams)
			{
				if (team3.Side == BattleSideEnum.Attacker)
				{
					team3.AddTeamAI(new TeamAISiegeAttacker(base.Mission, team3, 5f, 1f));
				}
				if (team3.Side == BattleSideEnum.Defender)
				{
					team3.AddTeamAI(new TeamAISiegeDefender(base.Mission, team3, 5f, 1f));
				}
			}
			break;
		case Mission.MissionTeamAITypeEnum.SallyOut:
			foreach (Team team4 in Mission.Current.Teams)
			{
				if (team4.Side == BattleSideEnum.Attacker)
				{
					team4.AddTeamAI(new TeamAISallyOutDefender(base.Mission, team4, 5f, 1f));
				}
				else
				{
					team4.AddTeamAI(new TeamAISallyOutAttacker(base.Mission, team4, 5f, 1f));
				}
			}
			break;
		}
		if (Mission.Current.Teams.Count <= 0)
		{
			return;
		}
		switch (Mission.Current.MissionTeamAIType)
		{
		case Mission.MissionTeamAITypeEnum.NoTeamAI:
			foreach (Team team5 in Mission.Current.Teams)
			{
				if (team5.HasTeamAi)
				{
					team5.AddTacticOption(new TacticCharge(team5));
				}
			}
			break;
		case Mission.MissionTeamAITypeEnum.FieldBattle:
			foreach (Team team in Mission.Current.Teams)
			{
				if (!team.HasTeamAi)
				{
					continue;
				}
				int num = _battleCombatants.Where((IBattleCombatant bc) => bc.Side == team.Side).Max((IBattleCombatant bcs) => bcs.GetTacticsSkillAmount());
				team.AddTacticOption(new TacticCharge(team));
				if ((float)num >= 20f)
				{
					team.AddTacticOption(new TacticFullScaleAttack(team));
					if (team.Side == BattleSideEnum.Defender)
					{
						team.AddTacticOption(new TacticDefensiveEngagement(team));
						team.AddTacticOption(new TacticDefensiveLine(team));
					}
					if (team.Side == BattleSideEnum.Attacker)
					{
						team.AddTacticOption(new TacticRangedHarrassmentOffensive(team));
					}
				}
				if ((float)num >= 50f)
				{
					team.AddTacticOption(new TacticFrontalCavalryCharge(team));
					if (team.Side == BattleSideEnum.Defender)
					{
						team.AddTacticOption(new TacticDefensiveRing(team));
						team.AddTacticOption(new TacticHoldChokePoint(team));
					}
					if (team.Side == BattleSideEnum.Attacker)
					{
						team.AddTacticOption(new TacticCoordinatedRetreat(team));
					}
				}
			}
			break;
		case Mission.MissionTeamAITypeEnum.Siege:
			foreach (Team team6 in Mission.Current.Teams)
			{
				if (team6.HasTeamAi)
				{
					if (team6.Side == BattleSideEnum.Attacker)
					{
						team6.AddTacticOption(new TacticBreachWalls(team6));
					}
					if (team6.Side == BattleSideEnum.Defender)
					{
						team6.AddTacticOption(new TacticDefendCastle(team6));
					}
				}
			}
			break;
		case Mission.MissionTeamAITypeEnum.SallyOut:
			foreach (Team team7 in Mission.Current.Teams)
			{
				if (team7.HasTeamAi)
				{
					if (team7.Side == BattleSideEnum.Defender)
					{
						team7.AddTacticOption(new TacticSallyOutHitAndRun(team7));
					}
					if (team7.Side == BattleSideEnum.Attacker)
					{
						team7.AddTacticOption(new TacticSallyOutDefense(team7));
					}
					team7.AddTacticOption(new TacticCharge(team7));
				}
			}
			break;
		}
		foreach (Team team8 in base.Mission.Teams)
		{
			team8.QuerySystem.Expire();
			team8.ResetTactic();
		}
	}

	public override void AfterStart()
	{
		base.Mission.SetMissionMode(MissionMode.Battle, atStart: true);
	}

	public IEnumerable<IBattleCombatant> GetAllCombatants()
	{
		foreach (IBattleCombatant battleCombatant in _battleCombatants)
		{
			yield return battleCombatant;
		}
	}

	private void AddPlayerTeam(BattleSideEnum playerSide)
	{
		base.Mission.Teams.Add(playerSide, _playerBattleCombatant.PrimaryColorPair.Item1, _playerBattleCombatant.PrimaryColorPair.Item2, _playerBattleCombatant.Banner);
		base.Mission.PlayerTeam = ((playerSide == BattleSideEnum.Attacker) ? base.Mission.AttackerTeam : base.Mission.DefenderTeam);
	}

	private void AddEnemyTeam(BattleSideEnum enemySide)
	{
		IBattleCombatant battleCombatant = ((enemySide == BattleSideEnum.Attacker) ? _attackerLeaderBattleCombatant : _defenderLeaderBattleCombatant);
		base.Mission.Teams.Add(enemySide, battleCombatant.PrimaryColorPair.Item1, battleCombatant.PrimaryColorPair.Item2, battleCombatant.Banner);
	}

	private void AddPlayerAllyTeam(BattleSideEnum playerSide)
	{
		if (_battleCombatants == null)
		{
			return;
		}
		foreach (IBattleCombatant battleCombatant in _battleCombatants)
		{
			if (battleCombatant != _playerBattleCombatant && battleCombatant.Side == playerSide && !_isPlayerSergeant)
			{
				base.Mission.Teams.Add(playerSide, battleCombatant.PrimaryColorPair.Item1, battleCombatant.PrimaryColorPair.Item2, battleCombatant.Banner);
				if (playerSide != BattleSideEnum.Attacker)
				{
					_ = base.Mission.DefenderAllyTeam;
				}
				else
				{
					_ = base.Mission.AttackerAllyTeam;
				}
				break;
			}
		}
	}
}
