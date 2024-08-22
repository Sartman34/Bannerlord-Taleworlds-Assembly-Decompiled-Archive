using System;
using System.Collections.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(MissionOrderOfBattleUIHandler))]
public class MissionGauntletOrderOfBattleUIHandler : MissionView
{
	private OrderOfBattleVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private IGauntletMovie _movie;

	private SpriteCategory _orderOfBattleCategory;

	private DeploymentMissionView _deploymentMissionView;

	private MissionGauntletSingleplayerOrderUIHandler _orderUIHandler;

	private AssignPlayerRoleInTeamMissionController _playerRoleMissionController;

	private OrderTroopPlacer _orderTroopPlacer;

	private bool _isActive;

	private bool _isDeploymentFinished;

	private float _cachedOrderTypeSetting;

	public MissionGauntletOrderOfBattleUIHandler(OrderOfBattleVM dataSource)
	{
		_dataSource = dataSource;
		ViewOrderPriority = 13;
	}

	public override void OnMissionScreenInitialize()
	{
		base.OnMissionScreenInitialize();
		_deploymentMissionView = base.Mission.GetMissionBehavior<DeploymentMissionView>();
		DeploymentMissionView deploymentMissionView = _deploymentMissionView;
		deploymentMissionView.OnDeploymentFinish = (OnPlayerDeploymentFinishDelegate)Delegate.Combine(deploymentMissionView.OnDeploymentFinish, new OnPlayerDeploymentFinishDelegate(OnDeploymentFinish));
		_playerRoleMissionController = base.Mission.GetMissionBehavior<AssignPlayerRoleInTeamMissionController>();
		_playerRoleMissionController.OnPlayerTurnToChooseFormationToLead += OnPlayerTurnToChooseFormationToLead;
		_playerRoleMissionController.OnAllFormationsAssignedSergeants += OnAllFormationsAssignedSergeants;
		_orderUIHandler = base.Mission.GetMissionBehavior<MissionGauntletSingleplayerOrderUIHandler>();
		_orderUIHandler.OnCameraControlsToggled += OnCameraControlsToggled;
		_orderTroopPlacer = base.Mission.GetMissionBehavior<OrderTroopPlacer>();
		OrderTroopPlacer orderTroopPlacer = _orderTroopPlacer;
		orderTroopPlacer.OnUnitDeployed = (Action)Delegate.Combine(orderTroopPlacer.OnUnitDeployed, new Action(OnUnitDeployed));
		_gauntletLayer = new GauntletLayer(ViewOrderPriority);
		_movie = _gauntletLayer.LoadMovie("OrderOfBattle", _dataSource);
		SpriteData spriteData = UIResourceManager.SpriteData;
		TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
		ResourceDepot uIResourceDepot = UIResourceManager.UIResourceDepot;
		_orderOfBattleCategory = spriteData.SpriteCategories["ui_order_of_battle"];
		_orderOfBattleCategory.Load(resourceContext, uIResourceDepot);
		base.MissionScreen.AddLayer(_gauntletLayer);
		_gauntletLayer.InputRestrictions.SetInputRestrictions(isMouseVisible: false);
		_gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
	}

	public override bool IsReady()
	{
		if (!_isDeploymentFinished)
		{
			return _orderOfBattleCategory.IsLoaded;
		}
		return true;
	}

	public override void OnMissionTick(float dt)
	{
		base.OnMissionTick(dt);
		if (_isActive)
		{
			TickInput();
			_dataSource.Tick();
		}
	}

	private void TickInput()
	{
		HandleLayerFocus(out var isAnyHeroSelected, out var isClassSelectionEnabled, out var isFilterSelectionEnabled);
		if (_gauntletLayer.Input.IsHotKeyReleased("Exit"))
		{
			if (isClassSelectionEnabled)
			{
				_dataSource.ExecuteDisableAllClassSelections();
			}
			else if (isFilterSelectionEnabled)
			{
				_dataSource.ExecuteDisableAllFilterSelections();
			}
			else if (isAnyHeroSelected)
			{
				_dataSource.ExecuteClearHeroSelection();
			}
		}
		if (base.MissionScreen.SceneLayer.Input.IsKeyDown(InputKey.RightMouseButton) || base.MissionScreen.SceneLayer.Input.IsKeyDown(InputKey.ControllerLTrigger))
		{
			_gauntletLayer.InputRestrictions.SetMouseVisibility(isVisible: false);
		}
		else
		{
			_gauntletLayer.InputRestrictions.SetInputRestrictions();
		}
	}

	private void HandleLayerFocus(out bool isAnyHeroSelected, out bool isClassSelectionEnabled, out bool isFilterSelectionEnabled)
	{
		isAnyHeroSelected = _dataSource.HasSelectedHeroes;
		isClassSelectionEnabled = _dataSource.IsAnyClassSelectionEnabled();
		isFilterSelectionEnabled = _dataSource.IsAnyFilterSelectionEnabled();
		bool flag = isAnyHeroSelected | isClassSelectionEnabled | isFilterSelectionEnabled;
		if (_gauntletLayer.IsFocusLayer && !flag)
		{
			base.MissionScreen.SetDisplayDialog(value: false);
			_gauntletLayer.IsFocusLayer = false;
			ScreenManager.TryLoseFocus(_gauntletLayer);
		}
		else if (!_gauntletLayer.IsFocusLayer && flag)
		{
			base.MissionScreen.SetDisplayDialog(value: true);
			_gauntletLayer.IsFocusLayer = true;
			ScreenManager.TrySetFocus(_gauntletLayer);
		}
	}

	public override void OnMissionScreenFinalize()
	{
		_dataSource.OnFinalize();
		_dataSource = null;
		_gauntletLayer.ReleaseMovie(_movie);
		base.MissionScreen.RemoveLayer(_gauntletLayer);
		_gauntletLayer = null;
		_orderOfBattleCategory.Unload();
		base.OnMissionScreenFinalize();
	}

	public override bool OnEscape()
	{
		bool flag = false;
		if (_orderUIHandler != null && _orderUIHandler.IsOrderMenuActive)
		{
			flag = _orderUIHandler.OnEscape();
		}
		if (!flag)
		{
			flag = _dataSource.OnEscape();
		}
		return flag;
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

	public override bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		return !_isActive;
	}

	private void OnPlayerTurnToChooseFormationToLead(Dictionary<int, Agent> lockedFormationIndicesAndSergeants, List<int> remainingFormationIndices)
	{
		if (base.Mission.PlayerTeam == null)
		{
			Debug.FailedAssert("Player team must be initialized before OOB", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\Mission\\Singleplayer\\MissionGauntletOrderOfBattleUIHandler.cs", "OnPlayerTurnToChooseFormationToLead", 199);
		}
		_cachedOrderTypeSetting = ManagedOptions.GetConfig(ManagedOptions.ManagedOptionsType.OrderType);
		ManagedOptions.SetConfig(ManagedOptions.ManagedOptionsType.OrderType, 1f);
		_dataSource.Initialize(base.Mission, base.MissionScreen.CombatCamera, SelectFormationAtIndex, DeselectFormationAtIndex, ClearFormationSelection, OnAutoDeploy, OnBeginMission, lockedFormationIndicesAndSergeants);
		_orderUIHandler.SetIsOrderPreconfigured(_dataSource.IsOrderPreconfigured);
		_gauntletLayer.InputRestrictions.SetInputRestrictions();
		_isActive = true;
	}

	private void OnAllFormationsAssignedSergeants(Dictionary<int, Agent> formationsWithLooselyAssignedSergeants)
	{
		_dataSource.OnAllFormationsAssignedSergeants(formationsWithLooselyAssignedSergeants);
	}

	private void OnDeploymentFinish()
	{
		bool playerDeployed = MissionGameModels.Current.BattleInitializationModel.CanPlayerSideDeployWithOrderOfBattle();
		_dataSource.OnDeploymentFinalized(playerDeployed);
		if (_isActive)
		{
			ManagedOptions.SetConfig(ManagedOptions.ManagedOptionsType.OrderType, _cachedOrderTypeSetting);
			_isActive = false;
			_gauntletLayer.InputRestrictions.ResetInputRestrictions();
		}
		_isDeploymentFinished = true;
		DeploymentMissionView deploymentMissionView = _deploymentMissionView;
		deploymentMissionView.OnDeploymentFinish = (OnPlayerDeploymentFinishDelegate)Delegate.Remove(deploymentMissionView.OnDeploymentFinish, new OnPlayerDeploymentFinishDelegate(OnDeploymentFinish));
		_playerRoleMissionController.OnPlayerTurnToChooseFormationToLead -= OnPlayerTurnToChooseFormationToLead;
		_playerRoleMissionController.OnAllFormationsAssignedSergeants -= OnAllFormationsAssignedSergeants;
		OrderTroopPlacer orderTroopPlacer = _orderTroopPlacer;
		orderTroopPlacer.OnUnitDeployed = (Action)Delegate.Remove(orderTroopPlacer.OnUnitDeployed, new Action(OnUnitDeployed));
		_orderUIHandler.OnCameraControlsToggled -= OnCameraControlsToggled;
		_orderOfBattleCategory.Unload();
		ScreenManager.TryLoseFocus(_gauntletLayer);
		_gauntletLayer.IsFocusLayer = false;
		base.MissionScreen.SetDisplayDialog(value: false);
	}

	private void OnCameraControlsToggled(bool isEnabled)
	{
		_dataSource.AreCameraControlsEnabled = isEnabled;
	}

	private void SelectFormationAtIndex(int index)
	{
		_orderUIHandler?.SelectFormationAtIndex(index);
	}

	private void DeselectFormationAtIndex(int index)
	{
		_orderUIHandler?.DeselectFormationAtIndex(index);
	}

	private void ClearFormationSelection()
	{
		_orderUIHandler?.ClearFormationSelection();
	}

	private void OnAutoDeploy()
	{
		_orderUIHandler.OnAutoDeploy();
	}

	private void OnBeginMission()
	{
		_orderUIHandler.OnBeginMission();
		_orderUIHandler.OnFiltersSet(_dataSource.CurrentConfiguration);
	}

	private void OnUnitDeployed()
	{
		_dataSource.OnUnitDeployed();
	}
}
