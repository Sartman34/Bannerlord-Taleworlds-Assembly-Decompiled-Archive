using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(MissionOrderUIHandler))]
public class MissionGauntletSingleplayerOrderUIHandler : MissionView, ISiegeDeploymentView
{
	private const string _radialOrderMovieName = "OrderRadial";

	private const string _barOrderMovieName = "OrderBar";

	private const float _slowDownAmountWhileOrderIsOpen = 0.25f;

	private const int _missionTimeSpeedRequestID = 864;

	private float _holdTime;

	private DeploymentMissionView _deploymentMissionView;

	private List<DeploymentSiegeMachineVM> _deploymentPointDataSources;

	private OrderTroopPlacer _orderTroopPlacer;

	private GauntletLayer _gauntletLayer;

	private IGauntletMovie _movie;

	private SpriteCategory _spriteCategory;

	private MissionOrderVM _dataSource;

	private SiegeDeploymentHandler _siegeDeploymentHandler;

	private BattleDeploymentHandler _battleDeploymentHandler;

	private MissionFormationTargetSelectionHandler _formationTargetHandler;

	private MBReadOnlyList<Formation> _focusedFormationsCache;

	private bool _isReceivingInput;

	private bool _isInitialized;

	private bool _slowedDownMission;

	private float _latestDt;

	private bool _targetFormationOrderGivenWithActionButton;

	private bool _isTransferEnabled;

	private float _minHoldTimeForActivation => 0f;

	public bool IsSiegeDeployment { get; private set; }

	public bool IsBattleDeployment { get; private set; }

	private bool _isAnyDeployment
	{
		get
		{
			if (!IsSiegeDeployment)
			{
				return IsBattleDeployment;
			}
			return true;
		}
	}

	public bool IsOrderMenuActive => _dataSource?.IsToggleOrderShown ?? false;

	public MissionOrderVM.CursorState cursorState
	{
		get
		{
			if (_dataSource.IsFacingSubOrdersShown)
			{
				return MissionOrderVM.CursorState.Face;
			}
			return MissionOrderVM.CursorState.Move;
		}
	}

	public event Action<bool> OnCameraControlsToggled;

	public MissionGauntletSingleplayerOrderUIHandler()
	{
		ViewOrderPriority = 14;
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		_latestDt = dt;
		_isReceivingInput = false;
		if (!base.MissionScreen.IsPhotoModeEnabled && (!base.MissionScreen.IsRadialMenuActive || _dataSource.IsToggleOrderShown))
		{
			TickInput(dt);
			_dataSource.Update();
			if (_dataSource.IsToggleOrderShown)
			{
				if (_targetFormationOrderGivenWithActionButton)
				{
					SetSuspendTroopPlacer(value: false);
					_targetFormationOrderGivenWithActionButton = false;
				}
				_orderTroopPlacer.IsDrawingForced = _dataSource.IsMovementSubOrdersShown;
				_orderTroopPlacer.IsDrawingFacing = _dataSource.IsFacingSubOrdersShown;
				_orderTroopPlacer.IsDrawingForming = false;
				if (cursorState == MissionOrderVM.CursorState.Face)
				{
					Vec2 orderLookAtDirection = OrderController.GetOrderLookAtDirection(base.Mission.MainAgent.Team.PlayerOrderController.SelectedFormations, base.MissionScreen.OrderFlag.Position.AsVec2);
					base.MissionScreen.OrderFlag.SetArrowVisibility(isVisible: true, orderLookAtDirection);
				}
				else
				{
					base.MissionScreen.OrderFlag.SetArrowVisibility(isVisible: false, Vec2.Invalid);
				}
				if (cursorState == MissionOrderVM.CursorState.Form)
				{
					float orderFormCustomWidth = OrderController.GetOrderFormCustomWidth(base.Mission.MainAgent.Team.PlayerOrderController.SelectedFormations, base.MissionScreen.OrderFlag.Position);
					base.MissionScreen.OrderFlag.SetWidthVisibility(isVisible: true, orderFormCustomWidth);
				}
				else
				{
					base.MissionScreen.OrderFlag.SetWidthVisibility(isVisible: false, -1f);
				}
				if (TaleWorlds.InputSystem.Input.IsGamepadActive)
				{
					OrderItemVM lastSelectedOrderItem = _dataSource.LastSelectedOrderItem;
					if (lastSelectedOrderItem == null || lastSelectedOrderItem.IsTitle)
					{
						if (_orderTroopPlacer.SuspendTroopPlacer && _dataSource.ActiveTargetState == 0)
						{
							_orderTroopPlacer.SuspendTroopPlacer = false;
						}
					}
					else if (!_orderTroopPlacer.SuspendTroopPlacer)
					{
						_orderTroopPlacer.SuspendTroopPlacer = true;
					}
				}
			}
			else if (_dataSource.TroopController.IsTransferActive || _isAnyDeployment)
			{
				_gauntletLayer.InputRestrictions.SetInputRestrictions();
			}
			else
			{
				if (!_dataSource.TroopController.IsTransferActive && !_orderTroopPlacer.SuspendTroopPlacer)
				{
					_orderTroopPlacer.SuspendTroopPlacer = true;
				}
				_gauntletLayer.InputRestrictions.ResetInputRestrictions();
			}
			if (_isAnyDeployment)
			{
				if (base.MissionScreen.SceneLayer.Input.IsKeyDown(InputKey.RightMouseButton) || base.MissionScreen.SceneLayer.Input.IsKeyDown(InputKey.ControllerLTrigger))
				{
					this.OnCameraControlsToggled?.Invoke(obj: true);
				}
				else
				{
					this.OnCameraControlsToggled?.Invoke(obj: false);
				}
			}
			base.MissionScreen.OrderFlag.IsTroop = _dataSource.ActiveTargetState == 0;
			TickOrderFlag(_latestDt, forceUpdate: false);
			bool flag = _dataSource.IsToggleOrderShown && _dataSource.OrderSets.Any((OrderSetVM x) => x.ShowOrders) && (_dataSource.IsHolding || base.Mission.Mode == MissionMode.Deployment);
			if (flag != base.MissionScreen.IsRadialMenuActive)
			{
				base.MissionScreen.SetRadialMenuActiveState(flag);
			}
		}
		_targetFormationOrderGivenWithActionButton = false;
		_dataSource.UpdateCanUseShortcuts(_isReceivingInput);
	}

	public override bool OnEscape()
	{
		bool isToggleOrderShown = _dataSource.IsToggleOrderShown;
		_dataSource.OnEscape();
		return isToggleOrderShown;
	}

	public override void OnMissionScreenActivate()
	{
		base.OnMissionScreenActivate();
		_dataSource.AfterInitialize();
		_isInitialized = true;
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		if (_isInitialized && agent.IsHuman)
		{
			_dataSource.TroopController.AddTroops(agent);
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);
		if (affectedAgent.IsHuman)
		{
			_dataSource.TroopController.RemoveTroops(affectedAgent);
		}
	}

	public override void OnMissionScreenInitialize()
	{
		base.OnMissionScreenInitialize();
		base.MissionScreen.SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("MissionOrderHotkeyCategory"));
		base.MissionScreen.OrderFlag = new OrderFlag(base.Mission, base.MissionScreen);
		_orderTroopPlacer = base.Mission.GetMissionBehavior<OrderTroopPlacer>();
		base.MissionScreen.SetOrderFlagVisibility(value: false);
		_siegeDeploymentHandler = base.Mission.GetMissionBehavior<SiegeDeploymentHandler>();
		_battleDeploymentHandler = base.Mission.GetMissionBehavior<BattleDeploymentHandler>();
		_formationTargetHandler = base.Mission.GetMissionBehavior<MissionFormationTargetSelectionHandler>();
		if (_formationTargetHandler != null)
		{
			_formationTargetHandler.OnFormationFocused += OnFormationFocused;
		}
		IsSiegeDeployment = _siegeDeploymentHandler != null;
		IsBattleDeployment = _battleDeploymentHandler != null;
		if (_isAnyDeployment)
		{
			_deploymentMissionView = base.Mission.GetMissionBehavior<DeploymentMissionView>();
			if (_deploymentMissionView != null)
			{
				DeploymentMissionView deploymentMissionView = _deploymentMissionView;
				deploymentMissionView.OnDeploymentFinish = (OnPlayerDeploymentFinishDelegate)Delegate.Combine(deploymentMissionView.OnDeploymentFinish, new OnPlayerDeploymentFinishDelegate(OnDeploymentFinish));
			}
			_deploymentPointDataSources = new List<DeploymentSiegeMachineVM>();
		}
		_dataSource = new MissionOrderVM(base.MissionScreen.CombatCamera, IsSiegeDeployment ? _siegeDeploymentHandler.PlayerDeploymentPoints.ToList() : new List<DeploymentPoint>(), ToggleScreenRotation, _isAnyDeployment, base.MissionScreen.GetOrderFlagPosition, RefreshVisuals, SetSuspendTroopPlacer, OnActivateToggleOrder, OnDeactivateToggleOrder, OnTransferFinished, OnBeforeOrder, isMultiplayer: false);
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("ToggleEscapeMenu"));
		_dataSource.TroopController.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.TroopController.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.TroopController.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
		if (IsSiegeDeployment)
		{
			foreach (DeploymentPoint playerDeploymentPoint in _siegeDeploymentHandler.PlayerDeploymentPoints)
			{
				DeploymentSiegeMachineVM deploymentSiegeMachineVM = new DeploymentSiegeMachineVM(playerDeploymentPoint, null, base.MissionScreen.CombatCamera, _dataSource.DeploymentController.OnRefreshSelectedDeploymentPoint, _dataSource.DeploymentController.OnEntityHover, isSelected: false);
				Vec3 origin = playerDeploymentPoint.GameEntity.GetFrame().origin;
				for (int i = 0; i < playerDeploymentPoint.GameEntity.ChildCount; i++)
				{
					if (playerDeploymentPoint.GameEntity.GetChild(i).HasTag("deployment_point_icon_target"))
					{
						origin += playerDeploymentPoint.GameEntity.GetChild(i).GetFrame().origin;
						break;
					}
				}
				_deploymentPointDataSources.Add(deploymentSiegeMachineVM);
				deploymentSiegeMachineVM.RemainingCount = 0;
			}
		}
		_gauntletLayer = new GauntletLayer(ViewOrderPriority);
		_gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		string movieName = ((BannerlordConfig.OrderType == 0) ? "OrderBar" : "OrderRadial");
		SpriteData spriteData = UIResourceManager.SpriteData;
		TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
		ResourceDepot uIResourceDepot = UIResourceManager.UIResourceDepot;
		_spriteCategory = spriteData.SpriteCategories["ui_order"];
		_spriteCategory.Load(resourceContext, uIResourceDepot);
		_movie = _gauntletLayer.LoadMovie(movieName, _dataSource);
		base.MissionScreen.AddLayer(_gauntletLayer);
		if (!_isAnyDeployment && BannerlordConfig.HideBattleUI)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
		if (!_isAnyDeployment && !_dataSource.IsToggleOrderShown)
		{
			ScreenManager.SetSuspendLayer(_gauntletLayer, isSuspended: true);
		}
		_dataSource.InputRestrictions = _gauntletLayer.InputRestrictions;
		ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Combine(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
	}

	public override bool IsReady()
	{
		return _spriteCategory.IsCategoryFullyLoaded();
	}

	private void OnManagedOptionChanged(ManagedOptions.ManagedOptionsType changedManagedOptionsType)
	{
		switch (changedManagedOptionsType)
		{
		case ManagedOptions.ManagedOptionsType.OrderType:
		{
			_gauntletLayer.ReleaseMovie(_movie);
			string movieName = ((BannerlordConfig.OrderType == 0) ? "OrderBar" : "OrderRadial");
			_movie = _gauntletLayer.LoadMovie(movieName, _dataSource);
			break;
		}
		case ManagedOptions.ManagedOptionsType.OrderLayoutType:
			_dataSource?.OnOrderLayoutTypeChanged();
			break;
		case ManagedOptions.ManagedOptionsType.HideBattleUI:
			if (!_isAnyDeployment)
			{
				_gauntletLayer.UIContext.ContextAlpha = (BannerlordConfig.HideBattleUI ? 0f : 1f);
			}
			break;
		case ManagedOptions.ManagedOptionsType.SlowDownOnOrder:
			if (!BannerlordConfig.SlowDownOnOrder && _slowedDownMission)
			{
				base.Mission.RemoveTimeSpeedRequest(864);
			}
			break;
		}
	}

	public override void OnMissionScreenFinalize()
	{
		base.OnMissionScreenFinalize();
		ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Remove(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		if (_formationTargetHandler != null)
		{
			_formationTargetHandler.OnFormationFocused -= OnFormationFocused;
		}
		_deploymentPointDataSources = null;
		_orderTroopPlacer = null;
		_movie = null;
		_gauntletLayer = null;
		_dataSource.OnFinalize();
		_dataSource = null;
		_siegeDeploymentHandler = null;
		_spriteCategory.Unload();
		_battleDeploymentHandler = null;
		_formationTargetHandler = null;
	}

	public OrderSetVM GetLastSelectedOrderSet()
	{
		return _dataSource.LastSelectedOrderSet;
	}

	public override void OnConversationBegin()
	{
		base.OnConversationBegin();
		_dataSource?.TryCloseToggleOrder(dontApplySelected: true);
	}

	public void OnActivateToggleOrder()
	{
		SetLayerEnabled(isEnabled: true);
	}

	public void OnDeactivateToggleOrder()
	{
		if (!_dataSource.TroopController.IsTransferActive)
		{
			SetLayerEnabled(isEnabled: false);
		}
	}

	private void OnTransferFinished()
	{
		if (!_isAnyDeployment)
		{
			SetLayerEnabled(isEnabled: false);
		}
	}

	public void OnAutoDeploy()
	{
		_dataSource.DeploymentController.ExecuteAutoDeploy();
	}

	public void OnBeginMission()
	{
		_dataSource.DeploymentController.ExecuteBeginSiege();
	}

	private void SetLayerEnabled(bool isEnabled)
	{
		if (isEnabled)
		{
			if (!base.MissionScreen.IsRadialMenuActive)
			{
				if (_dataSource == null || _dataSource.ActiveTargetState == 0)
				{
					_orderTroopPlacer.SuspendTroopPlacer = false;
				}
				if (!_slowedDownMission && BannerlordConfig.SlowDownOnOrder)
				{
					base.Mission.AddTimeSpeedRequest(new TaleWorlds.MountAndBlade.Mission.TimeSpeedRequest(0.25f, 864));
					_slowedDownMission = true;
				}
				base.MissionScreen.SetOrderFlagVisibility(value: true);
				if (_gauntletLayer != null)
				{
					ScreenManager.SetSuspendLayer(_gauntletLayer, isSuspended: false);
				}
				Game.Current.EventManager.TriggerEvent(new MissionPlayerToggledOrderViewEvent(newIsEnabledState: true));
			}
		}
		else
		{
			SetSuspendTroopPlacer(value: true);
			if (_gauntletLayer != null)
			{
				ScreenManager.SetSuspendLayer(_gauntletLayer, isSuspended: true);
			}
			if (_slowedDownMission)
			{
				base.Mission.RemoveTimeSpeedRequest(864);
				_slowedDownMission = false;
			}
			Game.Current.EventManager.TriggerEvent(new MissionPlayerToggledOrderViewEvent(newIsEnabledState: false));
		}
	}

	private void OnDeploymentFinish()
	{
		IsSiegeDeployment = false;
		IsBattleDeployment = false;
		_dataSource.OnDeploymentFinished();
		_deploymentPointDataSources.Clear();
		SetSuspendTroopPlacer(value: true);
		_gauntletLayer.IsFocusLayer = false;
		ScreenManager.TryLoseFocus(_gauntletLayer);
		base.MissionScreen.SetRadialMenuActiveState(isActive: false);
		_gauntletLayer.UIContext.ContextAlpha = (BannerlordConfig.HideBattleUI ? 0f : 1f);
		if (_deploymentMissionView != null)
		{
			DeploymentMissionView deploymentMissionView = _deploymentMissionView;
			deploymentMissionView.OnDeploymentFinish = (OnPlayerDeploymentFinishDelegate)Delegate.Remove(deploymentMissionView.OnDeploymentFinish, new OnPlayerDeploymentFinishDelegate(OnDeploymentFinish));
		}
	}

	private void RefreshVisuals()
	{
		if (!IsSiegeDeployment)
		{
			return;
		}
		foreach (DeploymentSiegeMachineVM deploymentPointDataSource in _deploymentPointDataSources)
		{
			deploymentPointDataSource.RefreshWithDeployedWeapon();
		}
	}

	private IOrderable GetFocusedOrderableObject()
	{
		return base.MissionScreen.OrderFlag.FocusedOrderableObject;
	}

	private void SetSuspendTroopPlacer(bool value)
	{
		_orderTroopPlacer.SuspendTroopPlacer = value;
		base.MissionScreen.SetOrderFlagVisibility(!value);
	}

	public void SelectFormationAtIndex(int index)
	{
		_dataSource.OnSelect(index);
	}

	public void DeselectFormationAtIndex(int index)
	{
		_dataSource.TroopController.OnDeselectFormation(index);
	}

	public void ClearFormationSelection()
	{
		_dataSource?.DeploymentController.ExecuteCancelSelectedDeploymentPoint();
		_dataSource?.OrderController.ClearSelectedFormations();
		_dataSource?.TryCloseToggleOrder();
	}

	public void OnFiltersSet(List<(int, List<int>)> filterData)
	{
		_dataSource.OnFiltersSet(filterData);
	}

	public void SetIsOrderPreconfigured(bool isOrderPreconfigured)
	{
		_dataSource.DeploymentController.SetIsOrderPreconfigured(isOrderPreconfigured);
	}

	private void OnBeforeOrder()
	{
		TickOrderFlag(_latestDt, forceUpdate: true);
	}

	private void TickOrderFlag(float dt, bool forceUpdate)
	{
		if ((base.MissionScreen.OrderFlag.IsVisible || forceUpdate) && Utilities.EngineFrameNo != base.MissionScreen.OrderFlag.LatestUpdateFrameNo)
		{
			base.MissionScreen.OrderFlag.Tick(_latestDt);
		}
	}

	private void OnFormationFocused(MBReadOnlyList<Formation> focusedFormations)
	{
		_focusedFormationsCache = focusedFormations;
		_dataSource.SetFocusedFormations(_focusedFormationsCache);
	}

	void ISiegeDeploymentView.OnEntityHover(GameEntity hoveredEntity)
	{
		if (!_gauntletLayer.IsHitThisFrame)
		{
			_dataSource.DeploymentController.OnEntityHover(hoveredEntity);
		}
	}

	void ISiegeDeploymentView.OnEntitySelection(GameEntity selectedEntity)
	{
		_dataSource.DeploymentController.OnEntitySelect(selectedEntity);
	}

	private void ToggleScreenRotation(bool isLocked)
	{
		MissionScreen.SetFixedMissionCameraActive(isLocked);
	}

	private void TickInput(float dt)
	{
		bool displayDialog = ((IMissionScreen)base.MissionScreen).GetDisplayDialog();
		bool flag = base.MissionScreen.SceneLayer.IsHitThisFrame || _gauntletLayer.IsHitThisFrame;
		if (displayDialog || (TaleWorlds.InputSystem.Input.IsGamepadActive && !flag))
		{
			_isReceivingInput = false;
			_dataSource.UpdateCanUseShortcuts(value: false);
			return;
		}
		_isReceivingInput = true;
		if (!IsSiegeDeployment && !IsBattleDeployment)
		{
			if (base.Input.IsGameKeyDown(86) && !_dataSource.IsToggleOrderShown)
			{
				_holdTime += dt;
				if (_holdTime >= _minHoldTimeForActivation)
				{
					_dataSource.OpenToggleOrder(fromHold: true, !_dataSource.IsHolding);
					_dataSource.IsHolding = true;
				}
			}
			else if (!base.Input.IsGameKeyDown(86))
			{
				if (_dataSource.IsHolding && _dataSource.IsToggleOrderShown)
				{
					_dataSource.TryCloseToggleOrder();
				}
				_dataSource.IsHolding = false;
				_holdTime = 0f;
			}
		}
		if (_dataSource.IsToggleOrderShown)
		{
			if (_dataSource.ActiveTargetState == 0 && (base.Input.IsKeyReleased(InputKey.LeftMouseButton) || base.Input.IsKeyReleased(InputKey.ControllerRTrigger)))
			{
				OrderItemVM lastSelectedOrderItem = _dataSource.LastSelectedOrderItem;
				if (lastSelectedOrderItem != null && !lastSelectedOrderItem.IsTitle && TaleWorlds.InputSystem.Input.IsGamepadActive)
				{
					_dataSource.ApplySelectedOrder();
				}
				else
				{
					switch (cursorState)
					{
					case MissionOrderVM.CursorState.Move:
					{
						MBReadOnlyList<Formation> focusedFormationsCache = _focusedFormationsCache;
						if (focusedFormationsCache != null && focusedFormationsCache.Count > 0)
						{
							_dataSource.OrderController.SetOrderWithFormation(OrderType.Charge, _focusedFormationsCache[0]);
							SetSuspendTroopPlacer(value: true);
							_targetFormationOrderGivenWithActionButton = true;
							break;
						}
						IOrderable focusedOrderableObject = GetFocusedOrderableObject();
						if (focusedOrderableObject != null)
						{
							if (_dataSource.OrderController.SelectedFormations.Count > 0)
							{
								_dataSource.OrderController.SetOrderWithOrderableObject(focusedOrderableObject);
							}
							else
							{
								Debug.FailedAssert("No selected formations when issuing order", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\Mission\\Singleplayer\\MissionGauntletSingleplayerOrderUIHandler.cs", "TickInput", 681);
							}
						}
						break;
					}
					case MissionOrderVM.CursorState.Face:
						_dataSource.OrderController.SetOrderWithPosition(OrderType.LookAtDirection, new WorldPosition(TaleWorlds.MountAndBlade.Mission.Current.Scene, UIntPtr.Zero, base.MissionScreen.GetOrderFlagPosition(), hasValidZ: false));
						break;
					case MissionOrderVM.CursorState.Form:
						_dataSource.OrderController.SetOrderWithPosition(OrderType.FormCustom, new WorldPosition(TaleWorlds.MountAndBlade.Mission.Current.Scene, UIntPtr.Zero, base.MissionScreen.GetOrderFlagPosition(), hasValidZ: false));
						break;
					default:
						Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\Mission\\Singleplayer\\MissionGauntletSingleplayerOrderUIHandler.cs", "TickInput", 696);
						break;
					}
				}
			}
			if (base.Input.IsKeyReleased(InputKey.RightMouseButton) && !_isAnyDeployment)
			{
				_dataSource.OnEscape();
			}
		}
		else if (_dataSource.TroopController.IsTransferActive != _isTransferEnabled)
		{
			_isTransferEnabled = _dataSource.TroopController.IsTransferActive;
			if (!_isTransferEnabled)
			{
				_gauntletLayer.UIContext.ContextAlpha = (BannerlordConfig.HideBattleUI ? 0f : 1f);
				_gauntletLayer.IsFocusLayer = false;
				ScreenManager.TryLoseFocus(_gauntletLayer);
			}
			else
			{
				_gauntletLayer.UIContext.ContextAlpha = 1f;
				_gauntletLayer.IsFocusLayer = true;
				ScreenManager.TrySetFocus(_gauntletLayer);
			}
		}
		else if (_dataSource.TroopController.IsTransferActive)
		{
			if (_gauntletLayer.Input.IsHotKeyReleased("Exit"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.TroopController.ExecuteCancelTransfer();
			}
			else if (_gauntletLayer.Input.IsHotKeyReleased("Confirm"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.TroopController.ExecuteConfirmTransfer();
			}
			else if (_gauntletLayer.Input.IsHotKeyReleased("Reset"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.TroopController.ExecuteReset();
			}
		}
		int num = -1;
		if ((!TaleWorlds.InputSystem.Input.IsGamepadActive || _dataSource.IsToggleOrderShown) && !base.DebugInput.IsControlDown())
		{
			if (base.Input.IsGameKeyPressed(68))
			{
				num = 0;
			}
			else if (base.Input.IsGameKeyPressed(69))
			{
				num = 1;
			}
			else if (base.Input.IsGameKeyPressed(70))
			{
				num = 2;
			}
			else if (base.Input.IsGameKeyPressed(71))
			{
				num = 3;
			}
			else if (base.Input.IsGameKeyPressed(72))
			{
				num = 4;
			}
			else if (base.Input.IsGameKeyPressed(73))
			{
				num = 5;
			}
			else if (base.Input.IsGameKeyPressed(74))
			{
				num = 6;
			}
			else if (base.Input.IsGameKeyPressed(75))
			{
				num = 7;
			}
			else if (base.Input.IsGameKeyPressed(76))
			{
				num = 8;
			}
		}
		if (num > -1)
		{
			_dataSource.OnGiveOrder(num);
		}
		int num2 = -1;
		if (base.Input.IsGameKeyPressed(77))
		{
			num2 = 100;
		}
		else if (base.Input.IsGameKeyPressed(78))
		{
			num2 = 0;
		}
		else if (base.Input.IsGameKeyPressed(79))
		{
			num2 = 1;
		}
		else if (base.Input.IsGameKeyPressed(80))
		{
			num2 = 2;
		}
		else if (base.Input.IsGameKeyPressed(81))
		{
			num2 = 3;
		}
		else if (base.Input.IsGameKeyPressed(82))
		{
			num2 = 4;
		}
		else if (base.Input.IsGameKeyPressed(83))
		{
			num2 = 5;
		}
		else if (base.Input.IsGameKeyPressed(84))
		{
			num2 = 6;
		}
		else if (base.Input.IsGameKeyPressed(85))
		{
			num2 = 7;
		}
		if (!IsBattleDeployment && !IsSiegeDeployment && _dataSource.IsToggleOrderShown)
		{
			if (base.Input.IsGameKeyPressed(87))
			{
				_dataSource.SelectNextTroop(1);
			}
			else if (base.Input.IsGameKeyPressed(88))
			{
				_dataSource.SelectNextTroop(-1);
			}
			else if (base.Input.IsGameKeyPressed(89))
			{
				_dataSource.ToggleSelectionForCurrentTroop();
			}
		}
		if (num2 != -1)
		{
			_dataSource.OnSelect(num2);
		}
		if (base.Input.IsGameKeyPressed(67))
		{
			_dataSource.ViewOrders();
		}
	}
}
