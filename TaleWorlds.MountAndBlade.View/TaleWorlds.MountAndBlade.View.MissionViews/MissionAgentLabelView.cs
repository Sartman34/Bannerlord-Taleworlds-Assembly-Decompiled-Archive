using System;
using System.Collections.Generic;
using System.ComponentModel;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

public class MissionAgentLabelView : MissionView
{
	private const float _highlightedLabelScaleFactor = 30f;

	private const float _labelBannerWidth = 0.4f;

	private const float _labelBlackBorderWidth = 0.44f;

	private readonly Dictionary<Agent, MetaMesh> _agentMeshes;

	private readonly Dictionary<Texture, Material> _labelMaterials;

	private bool _wasOrderScreenVisible;

	private bool _wasSiegeControllerScreenVisible;

	private OrderController PlayerOrderController => base.Mission.PlayerTeam?.PlayerOrderController;

	private SiegeWeaponController PlayerSiegeWeaponController => base.Mission.PlayerTeam?.PlayerOrderController.SiegeWeaponController;

	public MissionAgentLabelView()
	{
		_agentMeshes = new Dictionary<Agent, MetaMesh>();
		_labelMaterials = new Dictionary<Texture, Material>();
	}

	public override void OnBehaviorInitialize()
	{
		base.OnBehaviorInitialize();
		base.Mission.Teams.OnPlayerTeamChanged += Mission_OnPlayerTeamChanged;
		base.Mission.OnMainAgentChanged += OnMainAgentChanged;
		ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Combine(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		base.MissionScreen.OnSpectateAgentFocusIn += HandleSpectateAgentFocusIn;
		base.MissionScreen.OnSpectateAgentFocusOut += HandleSpectateAgentFocusOut;
	}

	public override void AfterStart()
	{
		if (PlayerOrderController != null)
		{
			PlayerOrderController.OnSelectedFormationsChanged += OrderController_OnSelectedFormationsChanged;
			base.Mission.PlayerTeam.OnFormationsChanged += PlayerTeam_OnFormationsChanged;
		}
		BannerBearerLogic missionBehavior = base.Mission.GetMissionBehavior<BannerBearerLogic>();
		if (missionBehavior != null)
		{
			missionBehavior.OnBannerBearerAgentUpdated += BannerBearerLogic_OnBannerBearerAgentUpdated;
		}
	}

	public override void OnMissionTick(float dt)
	{
		bool flag = IsOrderScreenVisible();
		bool flag2 = IsSiegeControllerScreenVisible();
		if (!flag && _wasOrderScreenVisible)
		{
			SetHighlightForAgents(highlight: false, useSiegeMachineUsers: false, useAllTeamAgents: false);
		}
		if (!flag2 && _wasSiegeControllerScreenVisible)
		{
			SetHighlightForAgents(highlight: false, useSiegeMachineUsers: true, useAllTeamAgents: false);
		}
		if (flag && !_wasOrderScreenVisible)
		{
			SetHighlightForAgents(highlight: true, useSiegeMachineUsers: false, useAllTeamAgents: false);
		}
		if (flag2 && !_wasSiegeControllerScreenVisible)
		{
			SetHighlightForAgents(highlight: true, useSiegeMachineUsers: true, useAllTeamAgents: false);
		}
		_wasOrderScreenVisible = flag;
		_wasSiegeControllerScreenVisible = flag2;
	}

	public override void OnRemoveBehavior()
	{
		UnregisterEvents();
		base.OnRemoveBehavior();
	}

	public override void OnMissionScreenFinalize()
	{
		UnregisterEvents();
		base.OnMissionScreenFinalize();
	}

	private void UnregisterEvents()
	{
		if (base.Mission != null)
		{
			base.Mission.Teams.OnPlayerTeamChanged -= Mission_OnPlayerTeamChanged;
			base.Mission.OnMainAgentChanged -= OnMainAgentChanged;
		}
		ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Remove(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		if (base.MissionScreen != null)
		{
			base.MissionScreen.OnSpectateAgentFocusIn -= HandleSpectateAgentFocusIn;
			base.MissionScreen.OnSpectateAgentFocusOut -= HandleSpectateAgentFocusOut;
		}
		if (PlayerOrderController != null)
		{
			PlayerOrderController.OnSelectedFormationsChanged -= OrderController_OnSelectedFormationsChanged;
			if (base.Mission != null)
			{
				base.Mission.PlayerTeam.OnFormationsChanged -= PlayerTeam_OnFormationsChanged;
			}
		}
		BannerBearerLogic missionBehavior = base.Mission.GetMissionBehavior<BannerBearerLogic>();
		if (missionBehavior != null)
		{
			missionBehavior.OnBannerBearerAgentUpdated -= BannerBearerLogic_OnBannerBearerAgentUpdated;
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		RemoveAgentLabel(affectedAgent);
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		InitAgentLabel(agent, banner);
	}

	public override void OnAssignPlayerAsSergeantOfFormation(Agent agent)
	{
		float friendlyTroopsBannerOpacity = BannerlordConfig.FriendlyTroopsBannerOpacity;
		_agentMeshes[agent].SetVectorArgument2(30f, 0.4f, 0.44f, 1f * friendlyTroopsBannerOpacity);
	}

	public override void OnClearScene()
	{
		_agentMeshes.Clear();
		_labelMaterials.Clear();
	}

	private void PlayerTeam_OnFormationsChanged(Team team, Formation formation)
	{
		if (IsOrderScreenVisible())
		{
			DehighlightAllAgents();
			SetHighlightForAgents(highlight: true, useSiegeMachineUsers: false, useAllTeamAgents: false);
		}
	}

	private void Mission_OnPlayerTeamChanged(Team previousTeam, Team currentTeam)
	{
		DehighlightAllAgents();
		_wasOrderScreenVisible = false;
		if (previousTeam?.PlayerOrderController != null)
		{
			previousTeam.PlayerOrderController.OnSelectedFormationsChanged -= OrderController_OnSelectedFormationsChanged;
			previousTeam.PlayerOrderController.SiegeWeaponController.OnSelectedSiegeWeaponsChanged -= PlayerSiegeWeaponController_OnSelectedSiegeWeaponsChanged;
		}
		if (PlayerOrderController != null)
		{
			PlayerOrderController.OnSelectedFormationsChanged += OrderController_OnSelectedFormationsChanged;
			PlayerSiegeWeaponController.OnSelectedSiegeWeaponsChanged += PlayerSiegeWeaponController_OnSelectedSiegeWeaponsChanged;
		}
		SetHighlightForAgents(highlight: true, useSiegeMachineUsers: false, useAllTeamAgents: true);
		foreach (Agent agent in base.Mission.Agents)
		{
			UpdateVisibilityOfAgentMesh(agent);
		}
	}

	private void OrderController_OnSelectedFormationsChanged()
	{
		DehighlightAllAgents();
		if (IsOrderScreenVisible())
		{
			SetHighlightForAgents(highlight: true, useSiegeMachineUsers: false, useAllTeamAgents: false);
		}
	}

	private void PlayerSiegeWeaponController_OnSelectedSiegeWeaponsChanged()
	{
		DehighlightAllAgents();
		SetHighlightForAgents(highlight: true, useSiegeMachineUsers: true, useAllTeamAgents: false);
	}

	public void OnAgentListSelectionChanged(bool selectionMode, List<Agent> affectedAgents)
	{
		foreach (Agent affectedAgent in affectedAgents)
		{
			float num = (selectionMode ? 1f : (-1f));
			if (_agentMeshes.ContainsKey(affectedAgent))
			{
				MetaMesh metaMesh = _agentMeshes[affectedAgent];
				float friendlyTroopsBannerOpacity = BannerlordConfig.FriendlyTroopsBannerOpacity;
				metaMesh.SetVectorArgument2(30f, 0.4f, 0.44f, num * friendlyTroopsBannerOpacity);
			}
		}
	}

	private void BannerBearerLogic_OnBannerBearerAgentUpdated(Agent agent, bool isBannerBearer)
	{
		RemoveAgentLabel(agent);
		InitAgentLabel(agent);
	}

	private void RemoveAgentLabel(Agent agent)
	{
		if (agent.IsHuman && _agentMeshes.ContainsKey(agent))
		{
			if (agent.AgentVisuals != null)
			{
				agent.AgentVisuals.ReplaceMeshWithMesh(_agentMeshes[agent], null, BodyMeshTypes.Label);
			}
			_agentMeshes.Remove(agent);
		}
	}

	private void InitAgentLabel(Agent agent, Banner peerBanner = null)
	{
		if (!agent.IsHuman)
		{
			return;
		}
		Banner banner = peerBanner ?? agent.Origin.Banner;
		if (banner == null)
		{
			return;
		}
		Texture texture = null;
		MetaMesh copy = MetaMesh.GetCopy("troop_banner_selection", showErrors: false, mayReturnNull: true);
		Material tableauMaterial = Material.GetFromResource("agent_label_with_tableau");
		texture = banner.GetTableauTextureSmall(null);
		if (!(copy != null) || !(tableauMaterial != null))
		{
			return;
		}
		Texture fromResource = Texture.GetFromResource("banner_top_of_head");
		if (_labelMaterials.TryGetValue(texture ?? fromResource, out var value))
		{
			tableauMaterial = value;
		}
		else
		{
			tableauMaterial = tableauMaterial.CreateCopy();
			Action<Texture> setAction = delegate(Texture tex)
			{
				tableauMaterial.SetTexture(Material.MBTextureType.DiffuseMap, tex);
			};
			texture = banner.GetTableauTextureSmall(setAction);
			tableauMaterial.SetTexture(Material.MBTextureType.DiffuseMap2, fromResource);
			_labelMaterials.Add(texture, tableauMaterial);
		}
		copy.SetMaterial(tableauMaterial);
		copy.SetVectorArgument(0.5f, 0.5f, 0.25f, 0.25f);
		copy.SetVectorArgument2(30f, 0.4f, 0.44f, -1f);
		agent.AgentVisuals.AddMultiMesh(copy, BodyMeshTypes.Label);
		_agentMeshes.Add(agent, copy);
		UpdateVisibilityOfAgentMesh(agent);
		UpdateSelectionVisibility(agent, _agentMeshes[agent], false);
	}

	private void UpdateVisibilityOfAgentMesh(Agent agent)
	{
		if (agent.IsActive() && _agentMeshes.ContainsKey(agent))
		{
			bool flag = IsMeshVisibleForAgent(agent);
			_agentMeshes[agent].SetVisibilityMask(flag ? VisibilityMaskFlags.Final : ((VisibilityMaskFlags)0u));
		}
	}

	private bool IsMeshVisibleForAgent(Agent agent)
	{
		if (IsAllyInAllyTeam(agent) && base.MissionScreen.LastFollowedAgent != agent && BannerlordConfig.FriendlyTroopsBannerOpacity > 0f)
		{
			return !base.MissionScreen.IsPhotoModeEnabled;
		}
		return false;
	}

	private void OnUpdateOpacityValueOfAgentMesh(Agent agent)
	{
		if (agent.IsActive() && _agentMeshes.ContainsKey(agent))
		{
			_agentMeshes[agent].SetVectorArgument2(30f, 0.4f, 0.44f, 0f - BannerlordConfig.FriendlyTroopsBannerOpacity);
		}
	}

	private bool IsAllyInAllyTeam(Agent agent)
	{
		if (agent?.Team != null && base.Mission != null && agent != base.Mission.MainAgent)
		{
			Team team = null;
			Team team2;
			if (GameNetwork.IsSessionActive)
			{
				team2 = ((!GameNetwork.IsMyPeerReady) ? null : GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.Team);
			}
			else
			{
				team2 = base.Mission.PlayerTeam;
				team = base.Mission.PlayerAllyTeam;
			}
			if (agent.Team != team2)
			{
				return agent.Team == team;
			}
			return true;
		}
		return false;
	}

	private void OnMainAgentChanged(object sender, PropertyChangedEventArgs e)
	{
		foreach (Agent key in _agentMeshes.Keys)
		{
			UpdateVisibilityOfAgentMesh(key);
		}
	}

	private void HandleSpectateAgentFocusIn(Agent agent)
	{
		UpdateVisibilityOfAgentMesh(agent);
	}

	private void HandleSpectateAgentFocusOut(Agent agent)
	{
		UpdateVisibilityOfAgentMesh(agent);
	}

	private void OnManagedOptionChanged(ManagedOptions.ManagedOptionsType optionType)
	{
		if (optionType != ManagedOptions.ManagedOptionsType.FriendlyTroopsBannerOpacity)
		{
			return;
		}
		foreach (Agent agent in base.Mission.Agents)
		{
			if (agent.IsHuman)
			{
				UpdateVisibilityOfAgentMesh(agent);
				if (IsMeshVisibleForAgent(agent))
				{
					OnUpdateOpacityValueOfAgentMesh(agent);
				}
			}
		}
	}

	private bool IsAgentListeningToOrders(Agent agent)
	{
		if (IsOrderScreenVisible() && agent.Formation != null && IsAllyInAllyTeam(agent))
		{
			foreach (Formation selectedFormation in PlayerOrderController.SelectedFormations)
			{
				if (selectedFormation.HasUnitsWithCondition((Agent unit) => unit == agent))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	private void UpdateSelectionVisibility(Agent agent, MetaMesh mesh, bool? visibility = null)
	{
		if (!visibility.HasValue)
		{
			visibility = IsAgentListeningToOrders(agent);
		}
		float num = (visibility.Value ? 1f : (-1f));
		if (agent.MissionPeer == null)
		{
			float config = ManagedOptions.GetConfig(ManagedOptions.ManagedOptionsType.FriendlyTroopsBannerOpacity);
			mesh.SetVectorArgument2(30f, 0.4f, 0.44f, num * config);
		}
	}

	private bool IsOrderScreenVisible()
	{
		if (PlayerOrderController != null && base.MissionScreen.OrderFlag != null)
		{
			return base.MissionScreen.OrderFlag.IsVisible;
		}
		return false;
	}

	private bool IsSiegeControllerScreenVisible()
	{
		if (PlayerOrderController != null && base.MissionScreen.OrderFlag != null)
		{
			return base.MissionScreen.OrderFlag.IsVisible;
		}
		return false;
	}

	private void SetHighlightForAgents(bool highlight, bool useSiegeMachineUsers, bool useAllTeamAgents)
	{
		if (PlayerOrderController == null)
		{
			bool flag = base.Mission.PlayerTeam == null;
			Debug.Print($"PlayerOrderController is null and playerTeamIsNull: {flag}", 0, Debug.DebugColor.White, 17179869184uL);
		}
		if (useSiegeMachineUsers)
		{
			foreach (TaleWorlds.MountAndBlade.SiegeWeapon selectedWeapon in PlayerSiegeWeaponController.SelectedWeapons)
			{
				foreach (StandingPoint standingPoint in selectedWeapon.StandingPoints)
				{
					Agent userAgent = standingPoint.UserAgent;
					if (userAgent != null)
					{
						UpdateSelectionVisibility(userAgent, _agentMeshes[userAgent], highlight);
					}
				}
			}
			return;
		}
		if (useAllTeamAgents)
		{
			if (PlayerOrderController.Owner != null)
			{
				Team team = PlayerOrderController.Owner.Team;
				if (team == null)
				{
					Debug.Print("PlayerOrderController.Owner.Team is null, overriding with Mission.Current.PlayerTeam", 0, Debug.DebugColor.White, 17179869184uL);
					team = Mission.Current.PlayerTeam;
				}
				{
					foreach (Agent activeAgent in team.ActiveAgents)
					{
						UpdateSelectionVisibility(activeAgent, _agentMeshes[activeAgent], highlight);
					}
					return;
				}
			}
			Debug.Print("PlayerOrderController.Owner is null", 0, Debug.DebugColor.White, 17179869184uL);
			return;
		}
		foreach (Formation selectedFormation in PlayerOrderController.SelectedFormations)
		{
			selectedFormation.ApplyActionOnEachUnit(delegate(Agent agent)
			{
				UpdateSelectionVisibility(agent, _agentMeshes[agent], highlight);
			});
		}
	}

	private void DehighlightAllAgents()
	{
		foreach (KeyValuePair<Agent, MetaMesh> agentMesh in _agentMeshes)
		{
			UpdateSelectionVisibility(agentMesh.Key, agentMesh.Value, false);
		}
	}

	public override void OnAgentTeamChanged(Team prevTeam, Team newTeam, Agent agent)
	{
		UpdateVisibilityOfAgentMesh(agent);
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		foreach (Agent agent in base.Mission.Agents)
		{
			if (agent.IsHuman)
			{
				UpdateVisibilityOfAgentMesh(agent);
			}
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		foreach (Agent agent in base.Mission.Agents)
		{
			if (agent.IsHuman)
			{
				UpdateVisibilityOfAgentMesh(agent);
			}
		}
	}
}
