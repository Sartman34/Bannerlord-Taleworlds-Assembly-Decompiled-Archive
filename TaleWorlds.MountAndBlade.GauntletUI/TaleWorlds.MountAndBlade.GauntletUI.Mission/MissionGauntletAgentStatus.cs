using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

[OverrideView(typeof(MissionAgentStatusUIHandler))]
public class MissionGauntletAgentStatus : MissionGauntletBattleUIBase
{
	private GauntletLayer _gauntletLayer;

	private MissionAgentStatusVM _dataSource;

	private MissionMainAgentController _missionMainAgentController;

	private MissionGauntletMainAgentEquipmentControllerView _missionMainAgentEquipmentControllerView;

	private DeploymentMissionView _deploymentMissionView;

	private bool _isInDeployement;

	public override void OnMissionStateActivated()
	{
		base.OnMissionStateActivated();
		_dataSource?.OnMainAgentWeaponChange();
	}

	public override void EarlyStart()
	{
		base.EarlyStart();
		_dataSource = new MissionAgentStatusVM(base.Mission, base.MissionScreen.CombatCamera, base.MissionScreen.GetCameraToggleProgress);
		_gauntletLayer = new GauntletLayer(ViewOrderPriority);
		_gauntletLayer.LoadMovie("MainAgentHUD", _dataSource);
		base.MissionScreen.AddLayer(_gauntletLayer);
		_dataSource.TakenDamageController.SetIsEnabled(BannerlordConfig.EnableDamageTakenVisuals);
		RegisterInteractionEvents();
		CombatLogManager.OnGenerateCombatLog += OnGenerateCombatLog;
		ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Combine(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
	}

	protected override void OnCreateView()
	{
		_dataSource.IsAgentStatusAvailable = true;
	}

	protected override void OnDestroyView()
	{
		_dataSource.IsAgentStatusAvailable = false;
	}

	private void OnManagedOptionChanged(ManagedOptions.ManagedOptionsType changedManagedOptionsType)
	{
		if (changedManagedOptionsType == ManagedOptions.ManagedOptionsType.EnableDamageTakenVisuals)
		{
			_dataSource?.TakenDamageController.SetIsEnabled(BannerlordConfig.EnableDamageTakenVisuals);
		}
	}

	public override void AfterStart()
	{
		base.AfterStart();
		_dataSource?.InitializeMainAgentPropterties();
	}

	public override void OnMissionScreenInitialize()
	{
		base.OnMissionScreenInitialize();
		_isInDeployement = base.Mission.GetMissionBehavior<BattleDeploymentHandler>() != null;
		if (_isInDeployement)
		{
			_deploymentMissionView = base.Mission.GetMissionBehavior<DeploymentMissionView>();
			if (_deploymentMissionView != null)
			{
				DeploymentMissionView deploymentMissionView = _deploymentMissionView;
				deploymentMissionView.OnDeploymentFinish = (OnPlayerDeploymentFinishDelegate)Delegate.Combine(deploymentMissionView.OnDeploymentFinish, new OnPlayerDeploymentFinishDelegate(OnDeploymentFinish));
			}
		}
	}

	private void OnDeploymentFinish()
	{
		_isInDeployement = false;
		DeploymentMissionView deploymentMissionView = _deploymentMissionView;
		deploymentMissionView.OnDeploymentFinish = (OnPlayerDeploymentFinishDelegate)Delegate.Remove(deploymentMissionView.OnDeploymentFinish, new OnPlayerDeploymentFinishDelegate(OnDeploymentFinish));
	}

	public override void OnMissionScreenFinalize()
	{
		base.OnMissionScreenFinalize();
		UnregisterInteractionEvents();
		ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Remove(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		CombatLogManager.OnGenerateCombatLog -= OnGenerateCombatLog;
		base.MissionScreen.RemoveLayer(_gauntletLayer);
		_gauntletLayer = null;
		_dataSource?.OnFinalize();
		_dataSource = null;
		_missionMainAgentController = null;
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		_dataSource.IsInDeployement = _isInDeployement;
		_dataSource.Tick(dt);
	}

	public override void OnFocusGained(Agent mainAgent, IFocusable focusableObject, bool isInteractable)
	{
		base.OnFocusGained(mainAgent, focusableObject, isInteractable);
		_dataSource?.OnFocusGained(mainAgent, focusableObject, isInteractable);
	}

	public override void OnAgentInteraction(Agent userAgent, Agent agent)
	{
		base.OnAgentInteraction(userAgent, agent);
		_dataSource?.OnAgentInteraction(userAgent, agent);
	}

	public override void OnFocusLost(Agent agent, IFocusable focusableObject)
	{
		base.OnFocusLost(agent, focusableObject);
		_dataSource?.OnFocusLost(agent, focusableObject);
	}

	public override void OnAgentDeleted(Agent affectedAgent)
	{
		_dataSource?.OnAgentDeleted(affectedAgent);
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		_dataSource?.OnAgentRemoved(affectedAgent);
	}

	private void OnGenerateCombatLog(CombatLogData logData)
	{
		if (logData.IsVictimAgentMine && logData.TotalDamage > 0 && logData.BodyPartHit != BoneBodyPartType.None)
		{
			_dataSource?.OnMainAgentHit(logData.TotalDamage, logData.IsRangedAttack ? 1 : 0);
		}
	}

	private void RegisterInteractionEvents()
	{
		_missionMainAgentController = base.Mission.GetMissionBehavior<MissionMainAgentController>();
		if (_missionMainAgentController != null)
		{
			_missionMainAgentController.InteractionComponent.OnFocusGained += _dataSource.OnSecondaryFocusGained;
			_missionMainAgentController.InteractionComponent.OnFocusLost += _dataSource.OnSecondaryFocusLost;
			_missionMainAgentController.InteractionComponent.OnFocusHealthChanged += _dataSource.InteractionInterface.OnFocusedHealthChanged;
		}
		_missionMainAgentEquipmentControllerView = base.Mission.GetMissionBehavior<MissionGauntletMainAgentEquipmentControllerView>();
		if (_missionMainAgentEquipmentControllerView != null)
		{
			_missionMainAgentEquipmentControllerView.OnEquipmentDropInteractionViewToggled += _dataSource.OnEquipmentInteractionViewToggled;
			_missionMainAgentEquipmentControllerView.OnEquipmentEquipInteractionViewToggled += _dataSource.OnEquipmentInteractionViewToggled;
		}
	}

	private void UnregisterInteractionEvents()
	{
		if (_missionMainAgentController != null)
		{
			_missionMainAgentController.InteractionComponent.OnFocusGained -= _dataSource.OnSecondaryFocusGained;
			_missionMainAgentController.InteractionComponent.OnFocusLost -= _dataSource.OnSecondaryFocusLost;
			_missionMainAgentController.InteractionComponent.OnFocusHealthChanged -= _dataSource.InteractionInterface.OnFocusedHealthChanged;
		}
		if (_missionMainAgentEquipmentControllerView != null)
		{
			_missionMainAgentEquipmentControllerView.OnEquipmentDropInteractionViewToggled -= _dataSource.OnEquipmentInteractionViewToggled;
			_missionMainAgentEquipmentControllerView.OnEquipmentEquipInteractionViewToggled -= _dataSource.OnEquipmentInteractionViewToggled;
		}
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		_gauntletLayer.UIContext.ContextAlpha = 0f;
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		_gauntletLayer.UIContext.ContextAlpha = 1f;
	}
}
