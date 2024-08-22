using System;
using System.Collections.Generic;
using System.Linq;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class MissionBoundaryCrossingHandler : MissionLogic
{
	private const float LeewayTime = 10f;

	private Dictionary<Agent, MissionTimer> _agentTimers;

	private MissionTimer _mainAgentLeaveTimer;

	public event Action<float, float> StartTime;

	public event Action StopTime;

	public event Action<float> TimeCount;

	public override void OnBehaviorInitialize()
	{
		base.OnBehaviorInitialize();
		if (GameNetwork.IsSessionActive)
		{
			AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
		}
		if (GameNetwork.IsServer)
		{
			_agentTimers = new Dictionary<Agent, MissionTimer>();
		}
	}

	public override void OnRemoveBehavior()
	{
		if (GameNetwork.IsSessionActive)
		{
			AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
		}
		base.OnRemoveBehavior();
	}

	private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
	{
		GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
		if (GameNetwork.IsClient)
		{
			networkMessageHandlerRegisterer.Register<SetBoundariesState>(HandleServerEventSetPeerBoundariesState);
		}
	}

	private void OnAgentWentOut(Agent agent, float startTimeInSeconds)
	{
		MissionTimer missionTimer = (GameNetwork.IsClient ? MissionTimer.CreateSynchedTimerClient(startTimeInSeconds, 10f) : new MissionTimer(10f));
		if (GameNetwork.IsServer)
		{
			_agentTimers.Add(agent, missionTimer);
			NetworkCommunicator networkCommunicator = agent.MissionPeer?.GetNetworkPeer();
			if (networkCommunicator != null && !networkCommunicator.IsServerPeer)
			{
				GameNetwork.BeginModuleEventAsServer(networkCommunicator);
				GameNetwork.WriteMessage(new SetBoundariesState(isOutside: true, missionTimer.GetStartTime().NumberOfTicks));
				GameNetwork.EndModuleEventAsServer();
			}
		}
		if (base.Mission.MainAgent == agent)
		{
			_mainAgentLeaveTimer = missionTimer;
			this.StartTime?.Invoke(10f, 0f);
			MatrixFrame cameraFrame = Mission.Current.GetCameraFrame();
			Vec3 position = cameraFrame.origin + cameraFrame.rotation.u;
			if (Mission.Current.Mode == MissionMode.Battle)
			{
				MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString("event:/alerts/report/out_of_map"), position);
			}
		}
	}

	private void OnAgentWentInOrRemoved(Agent agent, bool isAgentRemoved)
	{
		if (GameNetwork.IsServer)
		{
			_agentTimers.Remove(agent);
			if (!isAgentRemoved)
			{
				NetworkCommunicator networkCommunicator = agent.MissionPeer?.GetNetworkPeer();
				if (networkCommunicator != null && !networkCommunicator.IsServerPeer)
				{
					GameNetwork.BeginModuleEventAsServer(networkCommunicator);
					GameNetwork.WriteMessage(new SetBoundariesState(isOutside: false));
					GameNetwork.EndModuleEventAsServer();
				}
			}
		}
		if (base.Mission.MainAgent == agent)
		{
			_mainAgentLeaveTimer = null;
			this.StopTime?.Invoke();
		}
	}

	private void HandleAgentPunishment(Agent agent)
	{
		if (GameNetwork.IsSessionActive)
		{
			if (GameNetwork.IsServer)
			{
				Blow b = new Blow(agent.Index);
				b.WeaponRecord.FillAsMeleeBlow(null, null, -1, 0);
				b.DamageType = DamageTypes.Blunt;
				b.BaseMagnitude = 10000f;
				b.WeaponRecord.WeaponClass = WeaponClass.Undefined;
				b.GlobalPosition = agent.Position;
				b.DamagedPercentage = 1f;
				agent.Die(b);
			}
		}
		else
		{
			base.Mission.RetreatMission();
		}
	}

	public override void OnClearScene()
	{
		if (GameNetwork.IsServer)
		{
			foreach (Agent item in _agentTimers.Keys.ToList())
			{
				OnAgentWentInOrRemoved(item, isAgentRemoved: true);
			}
			return;
		}
		if (_mainAgentLeaveTimer != null)
		{
			if (base.Mission.MainAgent != null)
			{
				OnAgentWentInOrRemoved(base.Mission.MainAgent, isAgentRemoved: true);
			}
			else
			{
				_mainAgentLeaveTimer = null;
			}
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		OnAgentWentInOrRemoved(affectedAgent, isAgentRemoved: true);
	}

	public override void OnMissionTick(float dt)
	{
		base.OnMissionTick(dt);
		if (GameNetwork.IsServer)
		{
			for (int num = base.Mission.Agents.Count - 1; num >= 0; num--)
			{
				Agent agent = base.Mission.Agents[num];
				if (agent.MissionPeer != null)
				{
					TickForAgentAsServer(agent);
				}
			}
		}
		else if (!GameNetwork.IsSessionActive && Agent.Main != null)
		{
			TickForMainAgent();
		}
		if (_mainAgentLeaveTimer != null)
		{
			_mainAgentLeaveTimer.Check();
			float obj = 1f - _mainAgentLeaveTimer.GetRemainingTimeInSeconds(synched: true) / 10f;
			this.TimeCount?.Invoke(obj);
		}
	}

	private void TickForMainAgent()
	{
		bool isAgentOutside = !base.Mission.IsPositionInsideBoundaries(Agent.Main.Position.AsVec2);
		bool isTimerActiveForAgent = _mainAgentLeaveTimer != null;
		HandleAgentStateChange(Agent.Main, isAgentOutside, isTimerActiveForAgent, _mainAgentLeaveTimer);
	}

	private void TickForAgentAsServer(Agent agent)
	{
		bool isAgentOutside = !base.Mission.IsPositionInsideBoundaries(agent.Position.AsVec2);
		bool flag = _agentTimers.ContainsKey(agent);
		HandleAgentStateChange(agent, isAgentOutside, flag, flag ? _agentTimers[agent] : null);
	}

	private void HandleAgentStateChange(Agent agent, bool isAgentOutside, bool isTimerActiveForAgent, MissionTimer timerInstance)
	{
		if (isAgentOutside && !isTimerActiveForAgent)
		{
			OnAgentWentOut(agent, 0f);
		}
		else if (!isAgentOutside && isTimerActiveForAgent)
		{
			OnAgentWentInOrRemoved(agent, isAgentRemoved: false);
		}
		else if (isAgentOutside && timerInstance.Check())
		{
			HandleAgentPunishment(agent);
		}
	}

	private void HandleServerEventSetPeerBoundariesState(SetBoundariesState message)
	{
		if (message.IsOutside)
		{
			OnAgentWentOut(base.Mission.MainAgent, message.StateStartTimeInSeconds);
		}
		else
		{
			OnAgentWentInOrRemoved(base.Mission.MainAgent, isAgentRemoved: false);
		}
	}
}
