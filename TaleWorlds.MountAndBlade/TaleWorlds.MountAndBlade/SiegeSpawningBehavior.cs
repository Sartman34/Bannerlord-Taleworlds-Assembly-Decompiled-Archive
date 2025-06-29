using System.Collections.Generic;
using System.Linq;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade;

public class SiegeSpawningBehavior : SpawningBehaviorBase
{
	public override void Initialize(SpawnComponent spawnComponent)
	{
		base.Initialize(spawnComponent);
		base.OnAllAgentsFromPeerSpawnedFromVisuals += OnAllAgentsFromPeerSpawnedFromVisuals;
		if (GameMode.WarmupComponent == null)
		{
			RequestStartSpawnSession();
		}
	}

	public override void Clear()
	{
		base.Clear();
		base.OnAllAgentsFromPeerSpawnedFromVisuals -= OnAllAgentsFromPeerSpawnedFromVisuals;
	}

	public override void OnTick(float dt)
	{
		if (IsSpawningEnabled && _spawnCheckTimer.Check(base.Mission.CurrentTime))
		{
			SpawnAgents();
		}
		base.OnTick(dt);
	}

	protected override void SpawnAgents()
	{
		BasicCultureObject @object = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
		BasicCultureObject object2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
		foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
		{
			if (!networkPeer.IsSynchronized)
			{
				continue;
			}
			MissionPeer component = networkPeer.GetComponent<MissionPeer>();
			if (component == null || component.ControlledAgent != null || component.HasSpawnedAgentVisuals || component.Team == null || component.Team == base.Mission.SpectatorTeam || !component.TeamInitialPerkInfoReady || !component.SpawnTimer.Check(base.Mission.CurrentTime))
			{
				continue;
			}
			BasicCultureObject basicCultureObject = ((component.Team.Side == BattleSideEnum.Attacker) ? @object : object2);
			MultiplayerClassDivisions.MPHeroClass mPHeroClassForPeer = MultiplayerClassDivisions.GetMPHeroClassForPeer(component);
			if (mPHeroClassForPeer == null || mPHeroClassForPeer.TroopCasualCost > GameMode.GetCurrentGoldForPeer(component))
			{
				if (component.SelectedTroopIndex != 0)
				{
					component.SelectedTroopIndex = 0;
					GameNetwork.BeginBroadcastModuleEvent();
					GameNetwork.WriteMessage(new UpdateSelectedTroopIndex(networkPeer, 0));
					GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.ExcludeOtherTeamPlayers, networkPeer);
				}
				continue;
			}
			BasicCharacterObject heroCharacter = mPHeroClassForPeer.HeroCharacter;
			Equipment equipment = heroCharacter.Equipment.Clone();
			IEnumerable<(EquipmentIndex, EquipmentElement)> enumerable = MPPerkObject.GetOnSpawnPerkHandler(component)?.GetAlternativeEquipments(isPlayer: true);
			if (enumerable != null)
			{
				foreach (var item in enumerable)
				{
					equipment[item.Item1] = item.Item2;
				}
			}
			AgentBuildData agentBuildData = new AgentBuildData(heroCharacter).MissionPeer(component).Equipment(equipment).Team(component.Team)
				.TroopOrigin(new BasicBattleAgentOrigin(heroCharacter))
				.IsFemale(component.Peer.IsFemale)
				.BodyProperties(GetBodyProperties(component, (component.Team == base.Mission.AttackerTeam) ? @object : object2))
				.VisualsIndex(0)
				.ClothingColor1((component.Team == base.Mission.AttackerTeam) ? basicCultureObject.Color : basicCultureObject.ClothAlternativeColor)
				.ClothingColor2((component.Team == base.Mission.AttackerTeam) ? basicCultureObject.Color2 : basicCultureObject.ClothAlternativeColor2);
			if (GameMode.ShouldSpawnVisualsForServer(networkPeer))
			{
				base.AgentVisualSpawnComponent.SpawnAgentVisualsForPeer(component, agentBuildData, component.SelectedTroopIndex);
				if (agentBuildData.AgentVisualsIndex == 0)
				{
					component.HasSpawnedAgentVisuals = true;
					component.EquipmentUpdatingExpired = false;
				}
			}
			GameMode.HandleAgentVisualSpawning(networkPeer, agentBuildData);
		}
	}

	public override bool AllowEarlyAgentVisualsDespawning(MissionPeer lobbyPeer)
	{
		return true;
	}

	public override int GetMaximumReSpawnPeriodForPeer(MissionPeer peer)
	{
		if (GameMode.WarmupComponent != null && GameMode.WarmupComponent.IsInWarmup)
		{
			return 3;
		}
		if (peer.Team != null)
		{
			if (peer.Team.Side == BattleSideEnum.Attacker)
			{
				return MultiplayerOptions.OptionType.RespawnPeriodTeam1.GetIntValue();
			}
			if (peer.Team.Side == BattleSideEnum.Defender)
			{
				return MultiplayerOptions.OptionType.RespawnPeriodTeam2.GetIntValue();
			}
		}
		return -1;
	}

	protected override bool IsRoundInProgress()
	{
		return Mission.Current.CurrentState == Mission.State.Continuing;
	}

	private new void OnAllAgentsFromPeerSpawnedFromVisuals(MissionPeer peer)
	{
		bool flag = peer.Team == base.Mission.AttackerTeam;
		_ = base.Mission.DefenderTeam;
		MultiplayerClassDivisions.MPHeroClass mPHeroClass = MultiplayerClassDivisions.GetMPHeroClasses(MBObjectManager.Instance.GetObject<BasicCultureObject>(flag ? MultiplayerOptions.OptionType.CultureTeam1.GetStrValue() : MultiplayerOptions.OptionType.CultureTeam2.GetStrValue())).ElementAt(peer.SelectedTroopIndex);
		GameMode.ChangeCurrentGoldForPeer(peer, GameMode.GetCurrentGoldForPeer(peer) - mPHeroClass.TroopCasualCost);
	}
}
