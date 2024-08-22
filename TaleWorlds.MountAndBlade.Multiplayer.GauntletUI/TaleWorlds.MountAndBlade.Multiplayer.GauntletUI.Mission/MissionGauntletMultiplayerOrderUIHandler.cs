using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MultiplayerMissionOrderUIHandler))]
public class MissionGauntletMultiplayerOrderUIHandler : MissionView
{
	private const string _radialOrderMovieName = "OrderRadial";

	private const string _barOrderMovieName = "OrderBar";

	private float _holdTime;

	private OrderTroopPlacer _orderTroopPlacer;

	private GauntletLayer _gauntletLayer;

	private MissionOrderVM _dataSource;

	private IGauntletMovie _viewMovie;

	private IRoundComponent _roundComponent;

	private SpriteCategory _spriteCategory;

	private SiegeDeploymentHandler _siegeDeploymentHandler;

	private bool _isValid;

	private bool _isInitialized;

	private bool IsDeployment;

	private bool _shouldTick;

	private bool _shouldInitializeFormationInfo;

	private float _latestDt;

	private bool _isTransferEnabled;

	private float _minHoldTimeForActivation => 0f;

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

	public MissionGauntletMultiplayerOrderUIHandler()
	{
		ViewOrderPriority = 19;
	}

	public override void AfterStart()
	{
		base.AfterStart();
		MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.NumberOfBotsPerFormation).GetValue(out int value);
		_shouldTick = value > 0;
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		_latestDt = dt;
		if (!_shouldTick || (base.MissionScreen.IsRadialMenuActive && !_dataSource.IsToggleOrderShown))
		{
			return;
		}
		if (!_isInitialized)
		{
			Team team = (GameNetwork.IsMyPeerReady ? GameNetwork.MyPeer.GetComponent<MissionPeer>().Team : null);
			if (team != null && (team == base.Mission.AttackerTeam || team == base.Mission.DefenderTeam))
			{
				InitializeInADisgustingManner();
			}
		}
		if (!_isValid)
		{
			Team team2 = (GameNetwork.IsMyPeerReady ? GameNetwork.MyPeer.GetComponent<MissionPeer>().Team : null);
			if (team2 != null && (team2 == base.Mission.AttackerTeam || team2 == base.Mission.DefenderTeam))
			{
				ValidateInADisgustingManner();
			}
			return;
		}
		if (_shouldInitializeFormationInfo)
		{
			Team team3 = (GameNetwork.IsMyPeerReady ? GameNetwork.MyPeer.GetComponent<MissionPeer>().Team : null);
			if (_dataSource != null && team3 != null)
			{
				_dataSource.AfterInitialize();
				_shouldInitializeFormationInfo = false;
			}
		}
		TickInput(dt);
		_dataSource.Update();
		if (_dataSource.IsToggleOrderShown)
		{
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
					base.MissionScreen.SetRadialMenuActiveState(isActive: false);
					if (_orderTroopPlacer.SuspendTroopPlacer && _dataSource.ActiveTargetState == 0)
					{
						_orderTroopPlacer.SuspendTroopPlacer = false;
					}
				}
				else
				{
					base.MissionScreen.SetRadialMenuActiveState(isActive: true);
					if (!_orderTroopPlacer.SuspendTroopPlacer)
					{
						_orderTroopPlacer.SuspendTroopPlacer = true;
					}
				}
			}
		}
		else if (_dataSource.TroopController.IsTransferActive)
		{
			_gauntletLayer.InputRestrictions.SetInputRestrictions();
		}
		else
		{
			if (!_orderTroopPlacer.SuspendTroopPlacer)
			{
				_orderTroopPlacer.SuspendTroopPlacer = true;
			}
			_gauntletLayer.InputRestrictions.ResetInputRestrictions();
		}
		base.MissionScreen.OrderFlag.IsTroop = _dataSource.ActiveTargetState == 0;
		TickOrderFlag(dt, forceUpdate: false);
	}

	public override bool OnEscape()
	{
		if (_dataSource != null)
		{
			_dataSource.OnEscape();
			return _dataSource.IsToggleOrderShown;
		}
		return false;
	}

	public override void OnMissionScreenActivate()
	{
		base.OnMissionScreenActivate();
		if (_dataSource != null)
		{
			_dataSource.AfterInitialize();
			_isInitialized = true;
		}
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		if (_isValid && agent.IsHuman)
		{
			_dataSource?.TroopController.AddTroops(agent);
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);
		if (affectedAgent.IsHuman)
		{
			_dataSource?.TroopController.RemoveTroops(affectedAgent);
		}
	}

	public override void OnMissionScreenInitialize()
	{
		base.OnMissionScreenInitialize();
		base.MissionScreen.SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("MissionOrderHotkeyCategory"));
		_siegeDeploymentHandler = null;
		IsDeployment = false;
		ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Combine(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		_roundComponent = base.Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>()?.RoundComponent;
		if (_roundComponent != null)
		{
			_roundComponent.OnRoundStarted += OnRoundStarted;
			_roundComponent.OnPreparationEnded += OnPreparationEnded;
		}
	}

	private void OnRoundStarted()
	{
		_dataSource?.AfterInitialize();
	}

	private void OnPreparationEnded()
	{
		_shouldInitializeFormationInfo = true;
	}

	private void OnManagedOptionChanged(ManagedOptions.ManagedOptionsType changedManagedOptionsType)
	{
		switch (changedManagedOptionsType)
		{
		case ManagedOptions.ManagedOptionsType.OrderType:
			if (_gauntletLayer != null && _viewMovie != null)
			{
				_gauntletLayer.ReleaseMovie(_viewMovie);
				string movieName = ((BannerlordConfig.OrderType == 0) ? "OrderBar" : "OrderRadial");
				_viewMovie = _gauntletLayer.LoadMovie(movieName, _dataSource);
			}
			break;
		case ManagedOptions.ManagedOptionsType.OrderLayoutType:
			_dataSource?.OnOrderLayoutTypeChanged();
			break;
		}
	}

	public override void OnMissionScreenFinalize()
	{
		Clear();
		_orderTroopPlacer = null;
		MissionPeer.OnTeamChanged -= TeamChange;
		ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Remove(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		if (_roundComponent != null)
		{
			_roundComponent.OnRoundStarted -= OnRoundStarted;
			_roundComponent.OnPreparationEnded -= OnPreparationEnded;
		}
		base.OnMissionScreenFinalize();
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
	}

	private void SetLayerEnabled(bool isEnabled)
	{
		if (isEnabled)
		{
			if (_dataSource == null || _dataSource.ActiveTargetState == 0)
			{
				_orderTroopPlacer.SuspendTroopPlacer = false;
			}
			base.MissionScreen.SetOrderFlagVisibility(value: true);
			if (_gauntletLayer != null)
			{
				ScreenManager.SetSuspendLayer(_gauntletLayer, isSuspended: false);
			}
			Game.Current.EventManager.TriggerEvent(new MissionPlayerToggledOrderViewEvent(newIsEnabledState: true));
		}
		else
		{
			_orderTroopPlacer.SuspendTroopPlacer = true;
			base.MissionScreen.SetOrderFlagVisibility(value: false);
			if (_gauntletLayer != null)
			{
				ScreenManager.SetSuspendLayer(_gauntletLayer, isSuspended: true);
			}
			base.MissionScreen.SetRadialMenuActiveState(isActive: false);
			Game.Current.EventManager.TriggerEvent(new MissionPlayerToggledOrderViewEvent(newIsEnabledState: false));
		}
	}

	public void InitializeInADisgustingManner()
	{
		base.AfterStart();
		base.MissionScreen.OrderFlag = new OrderFlag(base.Mission, base.MissionScreen);
		_orderTroopPlacer = base.Mission.GetMissionBehavior<OrderTroopPlacer>();
		base.MissionScreen.SetOrderFlagVisibility(value: false);
		MissionPeer.OnTeamChanged += TeamChange;
		_isInitialized = true;
	}

	public void ValidateInADisgustingManner()
	{
		_dataSource = new MissionOrderVM(base.MissionScreen.CombatCamera, IsDeployment ? _siegeDeploymentHandler.PlayerDeploymentPoints.ToList() : new List<DeploymentPoint>(), ToggleScreenRotation, IsDeployment, base.MissionScreen.GetOrderFlagPosition, RefreshVisuals, SetSuspendTroopPlacer, OnActivateToggleOrder, OnDeactivateToggleOrder, OnTransferFinished, OnBeforeOrder, isMultiplayer: true);
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("ToggleEscapeMenu"));
		_dataSource.TroopController.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.TroopController.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.TroopController.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
		_gauntletLayer = new GauntletLayer(ViewOrderPriority);
		SpriteData spriteData = UIResourceManager.SpriteData;
		TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
		ResourceDepot uIResourceDepot = UIResourceManager.UIResourceDepot;
		_spriteCategory = spriteData.SpriteCategories["ui_order"];
		_spriteCategory.Load(resourceContext, uIResourceDepot);
		string movieName = ((BannerlordConfig.OrderType == 0) ? "OrderBar" : "OrderRadial");
		_viewMovie = _gauntletLayer.LoadMovie(movieName, _dataSource);
		_dataSource.InputRestrictions = _gauntletLayer.InputRestrictions;
		base.MissionScreen.AddLayer(_gauntletLayer);
		_dataSource.AfterInitialize();
		_isValid = true;
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

	private void RefreshVisuals()
	{
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

	private void ToggleScreenRotation(bool isLocked)
	{
		MissionScreen.SetFixedMissionCameraActive(isLocked);
	}

	private void Clear()
	{
		if (_gauntletLayer != null)
		{
			base.MissionScreen.RemoveLayer(_gauntletLayer);
		}
		if (_dataSource != null)
		{
			_dataSource.OnFinalize();
		}
		_gauntletLayer = null;
		_dataSource = null;
		_viewMovie = null;
		if (_isValid)
		{
			_spriteCategory.Unload();
		}
	}

	private void TeamChange(NetworkCommunicator peer, Team previousTeam, Team newTeam)
	{
		if (peer.IsMine)
		{
			Clear();
			_isValid = false;
		}
	}

	[Conditional("DEBUG")]
	private void TickInputDebug()
	{
		if (_dataSource.IsToggleOrderShown && base.DebugInput.IsKeyPressed(InputKey.F11) && !base.DebugInput.IsKeyPressed(InputKey.LeftControl) && !base.DebugInput.IsKeyPressed(InputKey.RightControl))
		{
			OrderType activeArrangementOrderOf = OrderController.GetActiveArrangementOrderOf(_dataSource.OrderController.SelectedFormations[0]);
			OrderType orderType = OrderType.ArrangementLine;
			OrderType orderType2 = OrderType.ArrangementScatter;
			activeArrangementOrderOf++;
			if (activeArrangementOrderOf > orderType2)
			{
				activeArrangementOrderOf = orderType;
			}
			_dataSource.OrderController.SetOrder(activeArrangementOrderOf);
			MBTextManager.SetTextVariable("ARRANGEMENT_ORDER", activeArrangementOrderOf.ToString());
			InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=RGzyUTzm}Formation arrangement switching to {ARRANGEMENT_ORDER}").ToString()));
		}
	}

	private void TickInput(float dt)
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
						IOrderable focusedOrderableObject = GetFocusedOrderableObject();
						if (focusedOrderableObject != null)
						{
							_dataSource.OrderController.SetOrderWithOrderableObject(focusedOrderableObject);
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
						TaleWorlds.Library.Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.GauntletUI\\Mission\\MissionGauntletMultiplayerOrderUIHandler.cs", "TickInput", 560);
						break;
					}
				}
			}
			if (base.Input.IsKeyReleased(InputKey.RightMouseButton))
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
