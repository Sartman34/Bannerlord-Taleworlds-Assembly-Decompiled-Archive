using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class AttackEntityOrderDetachment : IDetachment
{
	private const int Capacity = 8;

	private readonly List<Agent> _agents;

	private readonly MBList<Formation> _userFormations;

	private WorldFrame _frame;

	private readonly GameEntity _targetEntity;

	private readonly DestructableComponent _targetEntityDestructableComponent;

	private readonly bool _surroundEntity;

	private bool _isEvaluated;

	private float _cachedDetachmentWeight;

	public MBReadOnlyList<Formation> UserFormations => _userFormations;

	public bool IsLoose => true;

	public bool IsActive
	{
		get
		{
			if (_targetEntityDestructableComponent != null)
			{
				return !_targetEntityDestructableComponent.IsDestroyed;
			}
			return false;
		}
	}

	public AttackEntityOrderDetachment(GameEntity targetEntity)
	{
		_targetEntity = targetEntity;
		_targetEntityDestructableComponent = _targetEntity.GetFirstScriptOfType<DestructableComponent>();
		_surroundEntity = _targetEntity.GetFirstScriptOfType<CastleGate>() == null;
		_agents = new List<Agent>();
		_userFormations = new MBList<Formation>();
		MatrixFrame globalFrame = _targetEntity.GetGlobalFrame();
		_frame = new WorldFrame(globalFrame.rotation, new WorldPosition(targetEntity.GetScenePointer(), UIntPtr.Zero, globalFrame.origin, hasValidZ: false));
		_frame.Rotation.Orthonormalize();
	}

	public Vec3 GetPosition()
	{
		return _frame.Origin.GetGroundVec3();
	}

	public void AddAgent(Agent agent, int slotIndex)
	{
		_agents.Add(agent);
		agent.SetScriptedTargetEntityAndPosition(_targetEntity, new WorldPosition(agent.Mission.Scene, UIntPtr.Zero, _targetEntity.GlobalPosition, hasValidZ: false), _surroundEntity ? Agent.AISpecialCombatModeFlags.SurroundAttackEntity : Agent.AISpecialCombatModeFlags.None);
	}

	public void AddAgentAtSlotIndex(Agent agent, int slotIndex)
	{
		AddAgent(agent, slotIndex);
		agent.Formation?.DetachUnit(agent, isLoose: true);
		agent.Detachment = this;
		agent.DetachmentWeight = 1f;
	}

	void IDetachment.FormationStartUsing(Formation formation)
	{
		_userFormations.Add(formation);
	}

	void IDetachment.FormationStopUsing(Formation formation)
	{
		_userFormations.Remove(formation);
	}

	public bool IsUsedByFormation(Formation formation)
	{
		return _userFormations.Contains(formation);
	}

	Agent IDetachment.GetMovingAgentAtSlotIndex(int slotIndex)
	{
		return null;
	}

	void IDetachment.GetSlotIndexWeightTuples(List<(int, float)> slotIndexWeightTuples)
	{
		for (int i = _agents.Count; i < 8; i++)
		{
			slotIndexWeightTuples.Add((i, CalculateWeight(i)));
		}
	}

	bool IDetachment.IsSlotAtIndexAvailableForAgent(int slotIndex, Agent agent)
	{
		if (slotIndex < 8 && slotIndex >= _agents.Count && agent.CanBeAssignedForScriptedMovement())
		{
			return !IsAgentOnInconvenientNavmesh(agent);
		}
		return false;
	}

	private bool IsAgentOnInconvenientNavmesh(Agent agent)
	{
		if (Mission.Current.MissionTeamAIType != Mission.MissionTeamAITypeEnum.Siege)
		{
			return false;
		}
		int currentNavigationFaceId = agent.GetCurrentNavigationFaceId();
		if (agent.Team.TeamAI is TeamAISiegeComponent teamAISiegeComponent)
		{
			foreach (int difficultNavmeshID in teamAISiegeComponent.DifficultNavmeshIDs)
			{
				if (currentNavigationFaceId == difficultNavmeshID)
				{
					return true;
				}
			}
		}
		return false;
	}

	bool IDetachment.IsAgentEligible(Agent agent)
	{
		return true;
	}

	void IDetachment.UnmarkDetachment()
	{
	}

	bool IDetachment.IsDetachmentRecentlyEvaluated()
	{
		return false;
	}

	void IDetachment.MarkSlotAtIndex(int slotIndex)
	{
		Debug.FailedAssert("This should never have been called because this detachment does not seek to replace moving agents.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\TeamAI\\AttackEntityOrderDetachment.cs", "MarkSlotAtIndex", 149);
	}

	bool IDetachment.IsAgentUsingOrInterested(Agent agent)
	{
		return _agents.Contains(agent);
	}

	void IDetachment.OnFormationLeave(Formation formation)
	{
		for (int num = _agents.Count - 1; num >= 0; num--)
		{
			Agent agent = _agents[num];
			if (agent.Formation == formation && !agent.IsPlayerControlled)
			{
				((IDetachment)this).RemoveAgent(agent);
				formation.AttachUnit(agent);
			}
		}
	}

	public bool IsStandingPointAvailableForAgent(Agent agent)
	{
		return _agents.Count < 8;
	}

	public List<float> GetTemplateCostsOfAgent(Agent candidate, List<float> oldValue)
	{
		WorldPosition point = candidate.GetWorldPosition();
		WorldPosition point2 = _frame.Origin;
		point2.SetVec2(point2.AsVec2 + _frame.Rotation.f.AsVec2.Normalized() * 0.7f);
		float num = (Mission.Current.Scene.GetPathDistanceBetweenPositions(ref point, ref point2, candidate.Monster.BodyCapsuleRadius, out var pathDistance) ? pathDistance : float.MaxValue);
		num *= MissionGameModels.Current.AgentStatCalculateModel.GetDetachmentCostMultiplierOfAgent(candidate, this);
		List<float> list = oldValue ?? new List<float>(8);
		list.Clear();
		for (int i = 0; i < 8; i++)
		{
			list.Add(num);
		}
		return list;
	}

	float IDetachment.GetExactCostOfAgentAtSlot(Agent candidate, int slotIndex)
	{
		Debug.FailedAssert("This should never have been called because this detachment does not seek to replace moving agents.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\TeamAI\\AttackEntityOrderDetachment.cs", "GetExactCostOfAgentAtSlot", 205);
		return 0f;
	}

	public float GetTemplateWeightOfAgent(Agent candidate)
	{
		WorldPosition point = candidate.GetWorldPosition();
		WorldPosition point2 = _frame.Origin;
		if (!Mission.Current.Scene.GetPathDistanceBetweenPositions(ref point, ref point2, candidate.Monster.BodyCapsuleRadius, out var pathDistance))
		{
			return float.MaxValue;
		}
		return pathDistance;
	}

	public float? GetWeightOfAgentAtNextSlot(List<Agent> newAgents, out Agent match)
	{
		float? weightOfNextSlot = GetWeightOfNextSlot(newAgents[0].Team.Side);
		if (_agents.Count < 8)
		{
			Vec3 position = _targetEntity.GlobalPosition;
			match = newAgents.MinBy((Agent a) => a.Position.DistanceSquared(position));
			return weightOfNextSlot;
		}
		match = null;
		return null;
	}

	public float? GetWeightOfAgentAtNextSlot(List<(Agent, float)> agentTemplateScores, out Agent match)
	{
		float? weight = GetWeightOfNextSlot(agentTemplateScores[0].Item1.Team.Side);
		if (_agents.Count < 8)
		{
			IEnumerable<(Agent, float)> source = agentTemplateScores.Where(delegate((Agent, float) a)
			{
				var (agent, _) = a;
				return !agent.IsDetachedFromFormation || agent.DetachmentWeight < weight * 0.4f;
			});
			if (source.Any())
			{
				match = source.MinBy(((Agent, float) a) => a.Item2).Item1;
				return weight;
			}
		}
		match = null;
		return null;
	}

	public float? GetWeightOfAgentAtOccupiedSlot(Agent detachedAgent, List<Agent> newAgents, out Agent match)
	{
		float weightOfOccupiedSlot = GetWeightOfOccupiedSlot(detachedAgent);
		Vec3 position = _targetEntity.GlobalPosition;
		match = newAgents.MinBy((Agent a) => a.Position.DistanceSquared(position));
		return weightOfOccupiedSlot * 0.5f;
	}

	public void RemoveAgent(Agent agent)
	{
		_agents.Remove(agent);
		agent.DisableScriptedMovement();
		agent.DisableScriptedCombatMovement();
	}

	public int GetNumberOfUsableSlots()
	{
		return 8;
	}

	public WorldFrame? GetAgentFrame(Agent agent)
	{
		return null;
	}

	private static float CalculateWeight(int index)
	{
		return 1f + (1f - (float)index / 8f);
	}

	public float? GetWeightOfNextSlot(BattleSideEnum side)
	{
		if (_agents.Count < 8)
		{
			return CalculateWeight(_agents.Count);
		}
		return null;
	}

	public float GetWeightOfOccupiedSlot(Agent agent)
	{
		return CalculateWeight(_agents.IndexOf(agent));
	}

	float IDetachment.GetDetachmentWeight(BattleSideEnum side)
	{
		if (_agents.Count < 8)
		{
			return (float)(8 - _agents.Count) * 1f / 8f;
		}
		return float.MinValue;
	}

	void IDetachment.ResetEvaluation()
	{
		_isEvaluated = false;
	}

	bool IDetachment.IsEvaluated()
	{
		return _isEvaluated;
	}

	void IDetachment.SetAsEvaluated()
	{
		_isEvaluated = true;
	}

	float IDetachment.GetDetachmentWeightFromCache()
	{
		return _cachedDetachmentWeight;
	}

	float IDetachment.ComputeAndCacheDetachmentWeight(BattleSideEnum side)
	{
		_cachedDetachmentWeight = ((IDetachment)this).GetDetachmentWeight(side);
		return _cachedDetachmentWeight;
	}
}
