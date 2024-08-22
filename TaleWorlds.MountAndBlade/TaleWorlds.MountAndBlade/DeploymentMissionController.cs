using System.Diagnostics;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Missions.Handlers;

namespace TaleWorlds.MountAndBlade;

public class DeploymentMissionController : MissionLogic
{
	private BattleDeploymentHandler _battleDeploymentHandler;

	protected MissionAgentSpawnLogic MissionAgentSpawnLogic;

	private readonly bool _isPlayerAttacker;

	protected bool TeamSetupOver;

	private bool _isPlayerControllerSetToAI;

	public DeploymentMissionController(bool isPlayerAttacker)
	{
		_isPlayerAttacker = isPlayerAttacker;
	}

	public override void OnBehaviorInitialize()
	{
		base.OnBehaviorInitialize();
		_battleDeploymentHandler = base.Mission.GetMissionBehavior<BattleDeploymentHandler>();
		MissionAgentSpawnLogic = base.Mission.GetMissionBehavior<MissionAgentSpawnLogic>();
	}

	public override void AfterStart()
	{
		base.AfterStart();
		base.Mission.AllowAiTicking = false;
		for (int i = 0; i < 2; i++)
		{
			MissionAgentSpawnLogic.SetSpawnTroops((BattleSideEnum)i, spawnTroops: false);
		}
		MissionAgentSpawnLogic.SetReinforcementsSpawnEnabled(value: false);
	}

	private void SetupTeams()
	{
		Utilities.SetLoadingScreenPercentage(0.92f);
		base.Mission.DisableDying = true;
		BattleSideEnum battleSideEnum = ((!_isPlayerAttacker) ? BattleSideEnum.Attacker : BattleSideEnum.Defender);
		BattleSideEnum side = (_isPlayerAttacker ? BattleSideEnum.Attacker : BattleSideEnum.Defender);
		SetupTeamsOfSide(battleSideEnum);
		OnSideDeploymentFinished(battleSideEnum);
		if (_isPlayerAttacker)
		{
			foreach (Agent agent in base.Mission.Agents)
			{
				if (agent.IsHuman && agent.Team != null && agent.Team.Side == battleSideEnum)
				{
					agent.SetRenderCheckEnabled(value: false);
					agent.AgentVisuals.SetVisible(value: false);
					agent.MountAgent?.SetRenderCheckEnabled(value: false);
					agent.MountAgent?.AgentVisuals.SetVisible(value: false);
				}
			}
		}
		SetupTeamsOfSide(side);
		base.Mission.IsTeleportingAgents = true;
		Utilities.SetLoadingScreenPercentage(0.96f);
		if (!MissionGameModels.Current.BattleInitializationModel.CanPlayerSideDeployWithOrderOfBattle())
		{
			FinishDeployment();
		}
	}

	public override void OnAgentControllerSetToPlayer(Agent agent)
	{
		if (!_isPlayerControllerSetToAI)
		{
			agent.Controller = Agent.ControllerType.AI;
			agent.SetIsAIPaused(isPaused: true);
			agent.SetDetachableFromFormation(value: false);
			_isPlayerControllerSetToAI = true;
		}
	}

	public override void OnMissionTick(float dt)
	{
		base.OnMissionTick(dt);
		if (!TeamSetupOver && base.Mission.Scene != null)
		{
			SetupTeams();
			TeamSetupOver = true;
		}
	}

	[Conditional("DEBUG")]
	private void DebugTick()
	{
		if (Input.DebugInput.IsHotKeyPressed("SwapToEnemy"))
		{
			base.Mission.MainAgent.Controller = Agent.ControllerType.AI;
			base.Mission.PlayerEnemyTeam.Leader.Controller = Agent.ControllerType.Player;
			SwapTeams();
		}
	}

	private void SwapTeams()
	{
		base.Mission.PlayerTeam = base.Mission.PlayerEnemyTeam;
	}

	protected void SetupTeamsOfSideAux(BattleSideEnum side)
	{
		Team team = ((side == BattleSideEnum.Attacker) ? base.Mission.AttackerTeam : base.Mission.DefenderTeam);
		foreach (Formation item in team.FormationsIncludingSpecialAndEmpty)
		{
			if (item.CountOfUnits <= 0)
			{
				continue;
			}
			item.ApplyActionOnEachUnit(delegate(Agent agent)
			{
				if (agent.IsAIControlled)
				{
					agent.AIStateFlags &= ~Agent.AIStateFlag.Alarmed;
					agent.SetIsAIPaused(isPaused: true);
				}
			});
		}
		Team team2 = ((side == BattleSideEnum.Attacker) ? base.Mission.AttackerAllyTeam : base.Mission.DefenderAllyTeam);
		if (team2 != null)
		{
			foreach (Formation item2 in team2.FormationsIncludingSpecialAndEmpty)
			{
				if (item2.CountOfUnits <= 0)
				{
					continue;
				}
				item2.ApplyActionOnEachUnit(delegate(Agent agent)
				{
					if (agent.IsAIControlled)
					{
						agent.AIStateFlags &= ~Agent.AIStateFlag.Alarmed;
						agent.SetIsAIPaused(isPaused: true);
					}
				});
			}
		}
		MissionAgentSpawnLogic.OnBattleSideDeployed(team.Side);
	}

	protected virtual void SetupTeamsOfSide(BattleSideEnum side)
	{
		MissionAgentSpawnLogic.SetSpawnTroops(side, spawnTroops: true, enforceSpawning: true);
		SetupTeamsOfSideAux(side);
	}

	protected void OnSideDeploymentFinished(BattleSideEnum side)
	{
		Team team = ((side == BattleSideEnum.Attacker) ? base.Mission.AttackerTeam : base.Mission.DefenderTeam);
		if (side != base.Mission.PlayerTeam.Side)
		{
			base.Mission.IsTeleportingAgents = true;
			DeployFormationsOfTeam(team);
			Team team2 = ((side == BattleSideEnum.Attacker) ? base.Mission.AttackerAllyTeam : base.Mission.DefenderAllyTeam);
			if (team2 != null)
			{
				DeployFormationsOfTeam(team2);
			}
			base.Mission.IsTeleportingAgents = false;
		}
	}

	protected void DeployFormationsOfTeam(Team team)
	{
		foreach (Formation item in team.FormationsIncludingEmpty)
		{
			if (item.CountOfUnits > 0)
			{
				item.SetControlledByAI(isControlledByAI: true);
			}
		}
		team.QuerySystem.Expire();
		base.Mission.AllowAiTicking = true;
		base.Mission.ForceTickOccasionally = true;
		team.ResetTactic();
		bool isTeleportingAgents = Mission.Current.IsTeleportingAgents;
		base.Mission.IsTeleportingAgents = true;
		team.Tick(0f);
		base.Mission.IsTeleportingAgents = isTeleportingAgents;
		base.Mission.AllowAiTicking = false;
		base.Mission.ForceTickOccasionally = false;
	}

	public void FinishDeployment()
	{
		OnBeforeDeploymentFinished();
		if (_isPlayerAttacker)
		{
			foreach (Agent agent in base.Mission.Agents)
			{
				if (agent.IsHuman && agent.Team != null && agent.Team.Side == BattleSideEnum.Defender)
				{
					agent.SetRenderCheckEnabled(value: true);
					agent.AgentVisuals.SetVisible(value: true);
					agent.MountAgent?.SetRenderCheckEnabled(value: true);
					agent.MountAgent?.AgentVisuals.SetVisible(value: true);
				}
			}
		}
		base.Mission.IsTeleportingAgents = false;
		Mission.Current.OnDeploymentFinished();
		foreach (Agent agent2 in base.Mission.Agents)
		{
			if (agent2.IsAIControlled)
			{
				agent2.AIStateFlags |= Agent.AIStateFlag.Alarmed;
				agent2.SetIsAIPaused(isPaused: false);
				if (agent2.GetAgentFlags().HasAnyFlag(AgentFlag.CanWieldWeapon))
				{
					agent2.ResetEnemyCaches();
				}
				agent2.HumanAIComponent?.SyncBehaviorParamsIfNecessary();
			}
		}
		Agent mainAgent = base.Mission.MainAgent;
		if (mainAgent != null)
		{
			mainAgent.SetDetachableFromFormation(value: true);
			mainAgent.Controller = Agent.ControllerType.Player;
		}
		base.Mission.AllowAiTicking = true;
		base.Mission.DisableDying = false;
		MissionAgentSpawnLogic.SetReinforcementsSpawnEnabled(value: true);
		OnAfterDeploymentFinished();
		base.Mission.RemoveMissionBehavior(this);
	}

	public virtual void OnBeforeDeploymentFinished()
	{
		OnSideDeploymentFinished(base.Mission.PlayerTeam.Side);
	}

	public virtual void OnAfterDeploymentFinished()
	{
		base.Mission.RemoveMissionBehavior(_battleDeploymentHandler);
	}
}
