using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;

namespace TaleWorlds.MountAndBlade;

public class AgentVictoryLogic : MissionLogic
{
	public enum CheerActionGroupEnum
	{
		None,
		LowCheerActions,
		MidCheerActions,
		HighCheerActions
	}

	public struct CheerReactionTimeSettings
	{
		public readonly float MinDuration;

		public readonly float MaxDuration;

		public CheerReactionTimeSettings(float minDuration, float maxDuration)
		{
			MinDuration = minDuration;
			MaxDuration = maxDuration;
		}
	}

	private class CheeringAgent
	{
		public readonly Agent Agent;

		public readonly bool IsCheeringOnRetreat;

		public bool GotOrderRecently { get; private set; }

		public CheeringAgent(Agent agent, bool isCheeringOnRetreat)
		{
			Agent = agent;
			IsCheeringOnRetreat = isCheeringOnRetreat;
		}

		public void OrderReceived()
		{
			GotOrderRecently = true;
		}
	}

	private const float HighCheerThreshold = 0.25f;

	private const float MidCheerThreshold = 0.75f;

	private const float YellOnCheerCancelProbability = 0.25f;

	private CheerActionGroupEnum _cheerActionGroup;

	private CheerReactionTimeSettings _cheerReactionTimerData;

	private readonly ActionIndexCache[] _lowCheerActions = new ActionIndexCache[10]
	{
		ActionIndexCache.Create("act_cheering_low_01"),
		ActionIndexCache.Create("act_cheering_low_02"),
		ActionIndexCache.Create("act_cheering_low_03"),
		ActionIndexCache.Create("act_cheering_low_04"),
		ActionIndexCache.Create("act_cheering_low_05"),
		ActionIndexCache.Create("act_cheering_low_06"),
		ActionIndexCache.Create("act_cheering_low_07"),
		ActionIndexCache.Create("act_cheering_low_08"),
		ActionIndexCache.Create("act_cheering_low_09"),
		ActionIndexCache.Create("act_cheering_low_10")
	};

	private readonly ActionIndexCache[] _midCheerActions = new ActionIndexCache[4]
	{
		ActionIndexCache.Create("act_cheer_1"),
		ActionIndexCache.Create("act_cheer_2"),
		ActionIndexCache.Create("act_cheer_3"),
		ActionIndexCache.Create("act_cheer_4")
	};

	private readonly ActionIndexCache[] _highCheerActions = new ActionIndexCache[8]
	{
		ActionIndexCache.Create("act_cheering_high_01"),
		ActionIndexCache.Create("act_cheering_high_02"),
		ActionIndexCache.Create("act_cheering_high_03"),
		ActionIndexCache.Create("act_cheering_high_04"),
		ActionIndexCache.Create("act_cheering_high_05"),
		ActionIndexCache.Create("act_cheering_high_06"),
		ActionIndexCache.Create("act_cheering_high_07"),
		ActionIndexCache.Create("act_cheering_high_08")
	};

	private ActionIndexCache[] _selectedCheerActions;

	private List<CheeringAgent> _cheeringAgents;

	private bool _isInRetreat;

	public CheerActionGroupEnum CheerActionGroup => _cheerActionGroup;

	public CheerReactionTimeSettings CheerReactionTimerData => _cheerReactionTimerData;

	public override void AfterStart()
	{
		base.Mission.MissionCloseTimeAfterFinish = 60f;
		_cheeringAgents = new List<CheeringAgent>();
		SetCheerReactionTimerSettings();
		if (base.Mission.PlayerTeam != null)
		{
			base.Mission.PlayerTeam.PlayerOrderController.OnOrderIssued += MasterOrderControllerOnOrderIssued;
		}
		Mission.Current.IsBattleInRetreatEvent += CheckIfIsInRetreat;
	}

	private void MasterOrderControllerOnOrderIssued(OrderType orderType, IEnumerable<Formation> appliedFormations, OrderController orderController, object[] delegateparams)
	{
		MBList<Formation> mBList = appliedFormations.ToMBList();
		for (int num = _cheeringAgents.Count - 1; num >= 0; num--)
		{
			Agent agent = _cheeringAgents[num].Agent;
			if (mBList.Contains(agent.Formation))
			{
				_cheeringAgents[num].OrderReceived();
			}
		}
	}

	public void SetCheerActionGroup(CheerActionGroupEnum cheerActionGroup = CheerActionGroupEnum.None)
	{
		_cheerActionGroup = cheerActionGroup;
		switch (_cheerActionGroup)
		{
		case CheerActionGroupEnum.LowCheerActions:
			_selectedCheerActions = _lowCheerActions;
			break;
		case CheerActionGroupEnum.MidCheerActions:
			_selectedCheerActions = _midCheerActions;
			break;
		case CheerActionGroupEnum.HighCheerActions:
			_selectedCheerActions = _highCheerActions;
			break;
		default:
			_selectedCheerActions = null;
			break;
		}
	}

	public void SetCheerReactionTimerSettings(float minDuration = 1f, float maxDuration = 8f)
	{
		_cheerReactionTimerData = new CheerReactionTimeSettings(minDuration, maxDuration);
	}

	public override void OnClearScene()
	{
		_cheeringAgents.Clear();
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		VictoryComponent component = affectedAgent.GetComponent<VictoryComponent>();
		if (component != null)
		{
			affectedAgent.RemoveComponent(component);
		}
		for (int i = 0; i < _cheeringAgents.Count; i++)
		{
			if (_cheeringAgents[i].Agent == affectedAgent)
			{
				_cheeringAgents.RemoveAt(i);
				break;
			}
		}
	}

	protected override void OnEndMission()
	{
		Mission.Current.IsBattleInRetreatEvent -= CheckIfIsInRetreat;
	}

	public override void OnMissionTick(float dt)
	{
		if (_cheeringAgents.Count > 0)
		{
			CheckAnimationAndVoice();
		}
	}

	private void CheckAnimationAndVoice()
	{
		for (int num = _cheeringAgents.Count - 1; num >= 0; num--)
		{
			Agent agent = _cheeringAgents[num].Agent;
			bool gotOrderRecently = _cheeringAgents[num].GotOrderRecently;
			bool isCheeringOnRetreat = _cheeringAgents[num].IsCheeringOnRetreat;
			VictoryComponent component = agent.GetComponent<VictoryComponent>();
			if (component != null)
			{
				bool flag = agent.GetComponent<HumanAIComponent>()?.GetCurrentlyMovingGameObject() != null;
				bool flag2 = agent.GetCurrentAnimationFlag(0).HasAnyFlag(AnimFlags.anf_synch_with_ladder_movement) || agent.GetCurrentAnimationFlag(1).HasAnyFlag(AnimFlags.anf_synch_with_ladder_movement);
				if (CheckIfIsInRetreat() && gotOrderRecently && !flag && !flag2)
				{
					agent.RemoveComponent(component);
					agent.SetActionChannel(1, ActionIndexCache.act_none, ignorePriority: false, (uint)agent.GetCurrentActionPriority(1));
					if (MBRandom.RandomFloat > 0.25f)
					{
						agent.MakeVoice(SkinVoiceManager.VoiceType.Yell, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					}
					if (isCheeringOnRetreat)
					{
						agent.ClearTargetFrame();
					}
					_cheeringAgents.RemoveAt(num);
				}
				else if (component.CheckTimer())
				{
					if (!agent.IsActive())
					{
						Debug.FailedAssert("Agent trying to cheer without being active", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\MissionLogics\\AgentVictoryLogic.cs", "CheckAnimationAndVoice", 234);
						Debug.Print("Agent trying to cheer without being active");
					}
					ChooseWeaponToCheerWithCheerAndUpdateTimer(agent, out var resetTimer);
					if (resetTimer)
					{
						component.ChangeTimerDuration(6f, 12f);
					}
				}
			}
		}
	}

	private void SelectVictoryCondition(BattleSideEnum side)
	{
		if (_cheerActionGroup != 0)
		{
			return;
		}
		BattleObserverMissionLogic missionBehavior = Mission.Current.GetMissionBehavior<BattleObserverMissionLogic>();
		if (missionBehavior != null)
		{
			float deathToBuiltAgentRatioForSide = missionBehavior.GetDeathToBuiltAgentRatioForSide(side);
			if (deathToBuiltAgentRatioForSide < 0.25f)
			{
				SetCheerActionGroup(CheerActionGroupEnum.HighCheerActions);
			}
			else if (deathToBuiltAgentRatioForSide < 0.75f)
			{
				SetCheerActionGroup(CheerActionGroupEnum.MidCheerActions);
			}
			else
			{
				SetCheerActionGroup(CheerActionGroupEnum.LowCheerActions);
			}
		}
		else
		{
			SetCheerActionGroup(CheerActionGroupEnum.MidCheerActions);
		}
	}

	public void SetTimersOfVictoryReactionsOnBattleEnd(BattleSideEnum side)
	{
		_isInRetreat = false;
		SelectVictoryCondition(side);
		foreach (Team team in base.Mission.Teams)
		{
			if (team.Side != side)
			{
				continue;
			}
			foreach (Formation item in team.FormationsIncludingSpecialAndEmpty)
			{
				if (item.CountOfUnits > 0)
				{
					item.SetMovementOrder(MovementOrder.MovementOrderStop);
				}
			}
		}
		foreach (Agent agent in base.Mission.Agents)
		{
			if (agent.IsHuman && agent.IsAIControlled && agent.Team != null && side == agent.Team.Side && agent.CurrentWatchState == Agent.WatchState.Alarmed && agent.GetComponent<VictoryComponent>() == null)
			{
				if (_cheeringAgents.AnyQ((CheeringAgent a) => a.Agent == agent))
				{
					Debug.FailedAssert("Adding a duplicate agent in _cheeringAgents", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\MissionLogics\\AgentVictoryLogic.cs", "SetTimersOfVictoryReactionsOnBattleEnd", 308);
					Debug.Print("Adding a duplicate agent in _cheeringAgents");
				}
				agent.AddComponent(new VictoryComponent(agent, new RandomTimer(base.Mission.CurrentTime, _cheerReactionTimerData.MinDuration, _cheerReactionTimerData.MaxDuration)));
				_cheeringAgents.Add(new CheeringAgent(agent, isCheeringOnRetreat: false));
			}
		}
	}

	public void SetTimersOfVictoryReactionsOnRetreat(BattleSideEnum side)
	{
		_isInRetreat = true;
		SelectVictoryCondition(side);
		List<Agent> list = base.Mission.Agents.Where((Agent agent) => agent.IsHuman && agent.IsAIControlled && agent.Team.Side == side).ToList();
		int num = (int)((float)list.Count * 0.5f);
		List<Agent> list2 = new List<Agent>();
		for (int i = 0; i < list.Count; i++)
		{
			if (list2.Count == num)
			{
				break;
			}
			Agent agent2 = list[i];
			EquipmentIndex wieldedItemIndex = agent2.GetWieldedItemIndex(Agent.HandIndex.MainHand);
			bool num2 = wieldedItemIndex != EquipmentIndex.None && agent2.Equipment[wieldedItemIndex].Item.ItemFlags.HasAnyFlag(ItemFlags.DropOnAnyAction);
			EquipmentIndex wieldedItemIndex2 = agent2.GetWieldedItemIndex(Agent.HandIndex.OffHand);
			bool flag = wieldedItemIndex2 != EquipmentIndex.None && agent2.Equipment[wieldedItemIndex2].Item.ItemFlags.HasAnyFlag(ItemFlags.DropOnAnyAction);
			bool flag2 = agent2.GetComponent<HumanAIComponent>()?.GetCurrentlyMovingGameObject() != null;
			bool flag3 = agent2.GetCurrentAnimationFlag(0).HasAnyFlag(AnimFlags.anf_synch_with_ladder_movement) || agent2.GetCurrentAnimationFlag(1).HasAnyFlag(AnimFlags.anf_synch_with_ladder_movement);
			if (num2 || flag || agent2.IsUsingGameObject || flag2 || flag3)
			{
				continue;
			}
			int num3 = list.Count - i;
			int num4 = num - list2.Count;
			int num5 = num3 - num4;
			float num6 = MBMath.ClampFloat((float)(num - num5) / (float)num, 0f, 1f);
			if (num6 < 1f && agent2.TryGetImmediateEnemyAgentMovementData(out var maximumForwardUnlimitedSpeed, out var position))
			{
				float maximumForwardUnlimitedSpeed2 = agent2.MaximumForwardUnlimitedSpeed;
				float num7 = maximumForwardUnlimitedSpeed;
				if (maximumForwardUnlimitedSpeed2 > num7)
				{
					float num8 = (agent2.Position - position).LengthSquared / (maximumForwardUnlimitedSpeed2 - num7);
					if (num8 < 900f)
					{
						float num9 = num6 - -1f;
						float num10 = num8 / 900f;
						num6 = -1f + num9 * num10;
					}
				}
			}
			if (MBRandom.RandomFloat <= 0.5f + 0.5f * num6)
			{
				list2.Add(agent2);
			}
		}
		foreach (Agent item in list2)
		{
			MatrixFrame frame = item.Frame;
			Vec2 targetPosition = frame.origin.AsVec2;
			Vec3 targetDirection = frame.rotation.f;
			item.SetTargetPositionAndDirectionSynched(ref targetPosition, ref targetDirection);
			SetTimersOfVictoryReactionsForSingleAgent(item, _cheerReactionTimerData.MinDuration, _cheerReactionTimerData.MaxDuration, isCheeringOnRetreat: true);
		}
	}

	public void SetTimersOfVictoryReactionsOnTournamentVictoryForAgent(Agent agent, float minStartTime, float maxStartTime)
	{
		_selectedCheerActions = _midCheerActions;
		SetTimersOfVictoryReactionsForSingleAgent(agent, minStartTime, maxStartTime, isCheeringOnRetreat: false);
	}

	private void SetTimersOfVictoryReactionsForSingleAgent(Agent agent, float minStartTime, float maxStartTime, bool isCheeringOnRetreat)
	{
		if (agent.IsActive() && agent.IsHuman && agent.IsAIControlled)
		{
			if (_cheeringAgents.AnyQ((CheeringAgent a) => a.Agent == agent))
			{
				Debug.FailedAssert("Adding a duplicate agent in _cheeringAgents", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\MissionLogics\\AgentVictoryLogic.cs", "SetTimersOfVictoryReactionsForSingleAgent", 412);
				Debug.Print("Adding a duplicate agent in _cheeringAgents");
			}
			agent.AddComponent(new VictoryComponent(agent, new RandomTimer(base.Mission.CurrentTime, minStartTime, maxStartTime)));
			_cheeringAgents.Add(new CheeringAgent(agent, isCheeringOnRetreat));
		}
	}

	private void ChooseWeaponToCheerWithCheerAndUpdateTimer(Agent cheerAgent, out bool resetTimer)
	{
		resetTimer = false;
		if (cheerAgent.GetCurrentActionType(1) == Agent.ActionCodeType.EquipUnequip)
		{
			return;
		}
		EquipmentIndex wieldedItemIndex = cheerAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
		bool flag = wieldedItemIndex != EquipmentIndex.None && !cheerAgent.Equipment[wieldedItemIndex].Item.ItemFlags.HasAnyFlag(ItemFlags.DropOnAnyAction);
		if (!flag)
		{
			EquipmentIndex equipmentIndex = EquipmentIndex.None;
			for (EquipmentIndex equipmentIndex2 = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex2 < EquipmentIndex.ExtraWeaponSlot; equipmentIndex2++)
			{
				if (!cheerAgent.Equipment[equipmentIndex2].IsEmpty && !cheerAgent.Equipment[equipmentIndex2].Item.ItemFlags.HasAnyFlag(ItemFlags.DropOnAnyAction))
				{
					equipmentIndex = equipmentIndex2;
					break;
				}
			}
			if (equipmentIndex == EquipmentIndex.None)
			{
				if (wieldedItemIndex != EquipmentIndex.None)
				{
					cheerAgent.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.WithAnimation);
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				cheerAgent.TryToWieldWeaponInSlot(equipmentIndex, Agent.WeaponWieldActionType.WithAnimation, isWieldedOnSpawn: false);
			}
		}
		if (flag)
		{
			ActionIndexCache[] array = _selectedCheerActions;
			if (cheerAgent.HasMount)
			{
				array = _midCheerActions;
			}
			cheerAgent.SetActionChannel(1, array[MBRandom.RandomInt(array.Length)], ignorePriority: false, 0uL);
			cheerAgent.MakeVoice(SkinVoiceManager.VoiceType.Victory, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			resetTimer = true;
		}
	}

	private bool CheckIfIsInRetreat()
	{
		return _isInRetreat;
	}
}
