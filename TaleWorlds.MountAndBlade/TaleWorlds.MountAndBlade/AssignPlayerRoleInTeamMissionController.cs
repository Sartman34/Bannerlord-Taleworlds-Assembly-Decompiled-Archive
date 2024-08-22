using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public class AssignPlayerRoleInTeamMissionController : MissionLogic
{
	private bool _isPlayerSergeant;

	private FormationClass _preassignedFormationClass;

	private List<string> _charactersInPlayerSideByPriority = new List<string>();

	private Queue<string> _characterNamesInPlayerSideByPriorityQueue;

	private List<Formation> _remainingFormationsToAssignSergeantsTo;

	private Dictionary<int, Agent> _formationsLockedWithSergeants;

	private Dictionary<int, Agent> _formationsWithLooselyChosenSergeants;

	private int _playerChosenIndex = -1;

	public bool IsPlayerInArmy { get; }

	public bool IsPlayerGeneral { get; }

	public event PlayerTurnToChooseFormationToLeadEvent OnPlayerTurnToChooseFormationToLead;

	public event AllFormationsAssignedSergeantsEvent OnAllFormationsAssignedSergeants;

	public AssignPlayerRoleInTeamMissionController(bool isPlayerGeneral, bool isPlayerSergeant, bool isPlayerInArmy, List<string> charactersInPlayerSideByPriority = null, FormationClass preassignedFormationClass = FormationClass.NumberOfRegularFormations)
	{
		IsPlayerGeneral = isPlayerGeneral;
		_isPlayerSergeant = isPlayerSergeant;
		IsPlayerInArmy = isPlayerInArmy;
		_charactersInPlayerSideByPriority = charactersInPlayerSideByPriority;
		_preassignedFormationClass = preassignedFormationClass;
	}

	public override void AfterStart()
	{
		Mission.Current.PlayerTeam.SetPlayerRole(IsPlayerGeneral, _isPlayerSergeant);
	}

	private Formation ChooseFormationToLead(IEnumerable<Formation> formationsToChooseFrom, Agent agent)
	{
		bool hasMount = agent.HasMount;
		bool flag = agent.HasRangedWeapon();
		List<Formation> list = formationsToChooseFrom.ToList();
		while (list.Count > 0)
		{
			Formation formation = list.MaxBy((Formation ftcf) => ftcf.QuerySystem.FormationPower);
			list.Remove(formation);
			if ((flag || (!formation.QuerySystem.IsRangedFormation && !formation.QuerySystem.IsRangedCavalryFormation)) && (hasMount || (!formation.QuerySystem.IsCavalryFormation && !formation.QuerySystem.IsRangedCavalryFormation)))
			{
				return formation;
			}
		}
		return null;
	}

	private void AssignSergeant(Formation formationToLead, Agent sergeant)
	{
		sergeant.Formation = formationToLead;
		if (!sergeant.IsAIControlled || sergeant == Agent.Main)
		{
			formationToLead.PlayerOwner = sergeant;
		}
		formationToLead.Captain = sergeant;
	}

	public void OnPlayerChoiceMade(int chosenIndex, bool isFinal)
	{
		if (_playerChosenIndex != chosenIndex)
		{
			_playerChosenIndex = chosenIndex;
			_formationsWithLooselyChosenSergeants.Clear();
			List<Formation> list = base.Mission.PlayerTeam.FormationsIncludingEmpty.WhereQ((Formation f) => f.CountOfUnits > 0 && !_formationsLockedWithSergeants.ContainsKey(f.Index)).ToList();
			if (chosenIndex != -1)
			{
				Formation item = list.FirstOrDefault((Formation fr) => fr.Index == chosenIndex);
				_formationsWithLooselyChosenSergeants.Add(chosenIndex, base.Mission.PlayerTeam.PlayerOrderController.Owner);
				list.Remove(item);
			}
			Queue<string> queue = new Queue<string>(_characterNamesInPlayerSideByPriorityQueue);
			while (list.Count > 0 && queue.Count > 0)
			{
				string nextAgentNameToProcess = queue.Dequeue();
				Agent agent = base.Mission.PlayerTeam.ActiveAgents.FirstOrDefault((Agent aa) => aa.Character.StringId.Equals(nextAgentNameToProcess));
				if (agent != null)
				{
					Formation formation = ChooseFormationToLead(list, agent);
					if (formation != null)
					{
						_formationsWithLooselyChosenSergeants.Add(formation.Index, agent);
						list.Remove(formation);
					}
				}
			}
			if (this.OnAllFormationsAssignedSergeants != null)
			{
				this.OnAllFormationsAssignedSergeants(_formationsWithLooselyChosenSergeants);
			}
		}
		else
		{
			if (!isFinal)
			{
				return;
			}
			foreach (KeyValuePair<int, Agent> formationsLockedWithSergeant in _formationsLockedWithSergeants)
			{
				AssignSergeant(formationsLockedWithSergeant.Value.Team.GetFormation((FormationClass)formationsLockedWithSergeant.Key), formationsLockedWithSergeant.Value);
			}
			foreach (KeyValuePair<int, Agent> formationsWithLooselyChosenSergeant in _formationsWithLooselyChosenSergeants)
			{
				AssignSergeant(formationsWithLooselyChosenSergeant.Value.Team.GetFormation((FormationClass)formationsWithLooselyChosenSergeant.Key), formationsWithLooselyChosenSergeant.Value);
			}
		}
	}

	public void OnPlayerTeamDeployed()
	{
		if (!MissionGameModels.Current.BattleInitializationModel.CanPlayerSideDeployWithOrderOfBattle())
		{
			return;
		}
		Team playerTeam = Mission.Current.PlayerTeam;
		_formationsLockedWithSergeants = new Dictionary<int, Agent>();
		_formationsWithLooselyChosenSergeants = new Dictionary<int, Agent>();
		if (playerTeam.IsPlayerGeneral)
		{
			_characterNamesInPlayerSideByPriorityQueue = new Queue<string>();
			_remainingFormationsToAssignSergeantsTo = new List<Formation>();
		}
		else
		{
			_characterNamesInPlayerSideByPriorityQueue = ((_charactersInPlayerSideByPriority != null) ? new Queue<string>(_charactersInPlayerSideByPriority) : new Queue<string>());
			_remainingFormationsToAssignSergeantsTo = playerTeam.FormationsIncludingSpecialAndEmpty.WhereQ((Formation f) => f.CountOfUnits > 0).ToList();
			while (_remainingFormationsToAssignSergeantsTo.Count > 0 && _characterNamesInPlayerSideByPriorityQueue.Count > 0)
			{
				string nextAgentNameToProcess = _characterNamesInPlayerSideByPriorityQueue.Dequeue();
				Agent agent = playerTeam.ActiveAgents.FirstOrDefault((Agent aa) => aa.Character.StringId.Equals(nextAgentNameToProcess));
				if (agent != null)
				{
					if (agent == Agent.Main)
					{
						break;
					}
					Formation formation = ChooseFormationToLead(_remainingFormationsToAssignSergeantsTo, agent);
					if (formation != null)
					{
						_formationsLockedWithSergeants.Add(formation.Index, agent);
						_remainingFormationsToAssignSergeantsTo.Remove(formation);
					}
				}
			}
		}
		this.OnPlayerTurnToChooseFormationToLead?.Invoke(_formationsLockedWithSergeants, _remainingFormationsToAssignSergeantsTo.Select((Formation ftcsf) => ftcsf.Index).ToList());
	}

	public override void OnTeamDeployed(Team team)
	{
		base.OnTeamDeployed(team);
		if (team != base.Mission.PlayerTeam)
		{
			return;
		}
		team.PlayerOrderController.Owner = Agent.Main;
		if (team.IsPlayerGeneral)
		{
			foreach (Formation item in team.FormationsIncludingEmpty)
			{
				item.PlayerOwner = Agent.Main;
			}
		}
		team.PlayerOrderController.SelectAllFormations();
	}

	public void OnPlayerChoiceMade(FormationClass chosenFormationClass, FormationAI.BehaviorSide formationBehaviorSide = FormationAI.BehaviorSide.Middle)
	{
		Team playerTeam = base.Mission.PlayerTeam;
		Formation formation = playerTeam.FormationsIncludingEmpty.WhereQ((Formation f) => f.CountOfUnits > 0 && f.PhysicalClass == chosenFormationClass && f.AI.Side == formationBehaviorSide).MaxBy((Formation f) => f.QuerySystem.FormationPower);
		if (playerTeam.IsPlayerSergeant)
		{
			formation.PlayerOwner = Agent.Main;
			formation.SetControlledByAI(isControlledByAI: false);
		}
		if (formation != null && formation != Agent.Main.Formation)
		{
			MBTextManager.SetTextVariable("SIDE_STRING", formation.AI.Side.ToString());
			MBTextManager.SetTextVariable("CLASS_NAME", formation.PhysicalClass.GetName());
			MBInformationManager.AddQuickInformation(GameTexts.FindText("str_formation_soldier_join_text"));
		}
		Agent.Main.Formation = formation;
		playerTeam.TriggerOnFormationsChanged(formation);
	}
}
