using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.Order;

public class MissionOrderVM : ViewModel
{
	public enum CursorState
	{
		Move,
		Face,
		Form
	}

	public enum OrderTargets
	{
		Troops,
		SiegeMachines
	}

	public enum ActivationType
	{
		NotActive,
		Hold,
		Click
	}

	public InputRestrictions InputRestrictions;

	private ActivationType _currentActivationType;

	private Timer _updateTroopsTimer;

	internal readonly Dictionary<OrderSetType, OrderSetVM> OrderSetsWithOrdersByType;

	private readonly Camera _deploymentCamera;

	private bool _isTroopPlacingActive;

	private OrderSetType _lastSelectedOrderSetType;

	private bool _isPressedViewOrders;

	private readonly OnToggleActivateOrderStateDelegate _onActivateToggleOrder;

	private readonly OnToggleActivateOrderStateDelegate _onDeactivateToggleOrder;

	private readonly GetOrderFlagPositionDelegate _getOrderFlagPosition;

	private readonly ToggleOrderPositionVisibilityDelegate _toggleOrderPositionVisibility;

	private readonly OnRefreshVisualsDelegate _onRefreshVisuals;

	private readonly OnToggleActivateOrderStateDelegate _onTransferTroopsFinishedDelegate;

	private readonly OnBeforeOrderDelegate _onBeforeOrderDelegate;

	private readonly Action<bool> _toggleMissionInputs;

	private readonly List<DeploymentPoint> _deploymentPoints;

	private readonly bool _isMultiplayer;

	private OrderSetVM _movementSet;

	private MBReadOnlyList<Formation> _focusedFormationsCache;

	private OrderSetVM _facingSet;

	private int _delayValueForAIFormationModifications;

	private readonly List<Formation> _modifiedAIFormations = new List<Formation>();

	private List<(int, List<int>)> _filterData;

	private InputKeyItemVM _cancelInputKey;

	private MBBindingList<OrderSetVM> _orderSets;

	private MissionOrderTroopControllerVM _troopController;

	private MissionOrderDeploymentControllerVM _deploymentController;

	private bool _isDeployment;

	private int _activeTargetState;

	private bool _isToggleOrderShown;

	private bool _isTroopListShown;

	private bool _canUseShortcuts;

	private bool _isHolding;

	private bool _isAnyOrderSetActive;

	private string _returnText;

	private Team Team => Mission.Current.PlayerTeam;

	public OrderItemVM LastSelectedOrderItem { get; private set; }

	public OrderController OrderController => Team.PlayerOrderController;

	public bool IsMovementSubOrdersShown => _movementSet.ShowOrders;

	public bool IsFacingSubOrdersShown => _facingSet?.ShowOrders ?? false;

	public bool IsTroopPlacingActive
	{
		get
		{
			return _isTroopPlacingActive;
		}
		set
		{
			_isTroopPlacingActive = value;
			_toggleOrderPositionVisibility(!value);
		}
	}

	public OrderSetType LastSelectedOrderSetType
	{
		get
		{
			return _lastSelectedOrderSetType;
		}
		set
		{
			if (value != _lastSelectedOrderSetType)
			{
				_lastSelectedOrderSetType = value;
				IsAnyOrderSetActive = _lastSelectedOrderSetType != OrderSetType.None;
			}
		}
	}

	public OrderSetVM LastSelectedOrderSet => OrderSets.FirstOrDefault((OrderSetVM o) => o.OrderSetType == LastSelectedOrderSetType);

	public bool PlayerHasAnyTroopUnderThem => Team.FormationsIncludingEmpty.Any((Formation f) => f.PlayerOwner == Agent.Main && f.CountOfUnits > 0);

	private Mission Mission => Mission.Current;

	[DataSourceProperty]
	public InputKeyItemVM CancelInputKey
	{
		get
		{
			return _cancelInputKey;
		}
		set
		{
			if (value != _cancelInputKey)
			{
				_cancelInputKey = value;
				OnPropertyChangedWithValue(value, "CancelInputKey");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<OrderSetVM> OrderSets
	{
		get
		{
			return _orderSets;
		}
		set
		{
			if (value != _orderSets)
			{
				_orderSets = value;
				OnPropertyChangedWithValue(value, "OrderSets");
			}
		}
	}

	[DataSourceProperty]
	public MissionOrderTroopControllerVM TroopController
	{
		get
		{
			return _troopController;
		}
		set
		{
			if (value != _troopController)
			{
				_troopController = value;
				OnPropertyChangedWithValue(value, "TroopController");
			}
		}
	}

	[DataSourceProperty]
	public MissionOrderDeploymentControllerVM DeploymentController
	{
		get
		{
			return _deploymentController;
		}
		set
		{
			if (value != _deploymentController)
			{
				_deploymentController = value;
				OnPropertyChangedWithValue(value, "DeploymentController");
			}
		}
	}

	[DataSourceProperty]
	public int ActiveTargetState
	{
		get
		{
			return _activeTargetState;
		}
		set
		{
			if (value == _activeTargetState)
			{
				return;
			}
			_activeTargetState = value;
			OnPropertyChangedWithValue(value, "ActiveTargetState");
			IsTroopPlacingActive = value == 0;
			foreach (OrderSetVM value2 in OrderSetsWithOrdersByType.Values)
			{
				value2.ShowOrders = false;
				value2.TitleOrder.IsActive = value2.TitleOrder.SelectionState != 0;
			}
			_onRefreshVisuals();
		}
	}

	[DataSourceProperty]
	public bool IsDeployment
	{
		get
		{
			return _isDeployment;
		}
		set
		{
			_isDeployment = value;
			OnPropertyChangedWithValue(value, "IsDeployment");
		}
	}

	[DataSourceProperty]
	public bool IsToggleOrderShown
	{
		get
		{
			return _isToggleOrderShown;
		}
		set
		{
			if (value != _isToggleOrderShown)
			{
				_isToggleOrderShown = value;
				OnOrderShownToggle();
				OnPropertyChangedWithValue(value, "IsToggleOrderShown");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTroopListShown
	{
		get
		{
			return _isTroopListShown;
		}
		set
		{
			if (value != _isTroopListShown)
			{
				_isTroopListShown = value;
				OnPropertyChangedWithValue(value, "IsTroopListShown");
			}
		}
	}

	[DataSourceProperty]
	public bool CanUseShortcuts
	{
		get
		{
			return _canUseShortcuts;
		}
		set
		{
			if (value != _canUseShortcuts)
			{
				_canUseShortcuts = value;
				OnPropertyChangedWithValue(value, "CanUseShortcuts");
			}
		}
	}

	[DataSourceProperty]
	public bool IsHolding
	{
		get
		{
			return _isHolding;
		}
		set
		{
			if (value != _isHolding)
			{
				_isHolding = value;
				OnPropertyChangedWithValue(value, "IsHolding");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAnyOrderSetActive
	{
		get
		{
			return _isAnyOrderSetActive;
		}
		set
		{
			if (value != _isAnyOrderSetActive)
			{
				_isAnyOrderSetActive = value;
				OnPropertyChangedWithValue(value, "IsAnyOrderSetActive");
			}
		}
	}

	[DataSourceProperty]
	public string ReturnText
	{
		get
		{
			return _returnText;
		}
		set
		{
			if (value != _returnText)
			{
				_returnText = value;
				OnPropertyChangedWithValue(value, "ReturnText");
			}
		}
	}

	public MissionOrderVM(Camera deploymentCamera, List<DeploymentPoint> deploymentPoints, Action<bool> toggleMissionInputs, bool isDeployment, GetOrderFlagPositionDelegate getOrderFlagPosition, OnRefreshVisualsDelegate refreshVisuals, ToggleOrderPositionVisibilityDelegate setSuspendTroopPlacer, OnToggleActivateOrderStateDelegate onActivateToggleOrder, OnToggleActivateOrderStateDelegate onDeactivateToggleOrder, OnToggleActivateOrderStateDelegate onTransferTroopsFinishedDelegate, OnBeforeOrderDelegate onBeforeOrderDelegate, bool isMultiplayer)
	{
		_deploymentCamera = deploymentCamera;
		_toggleMissionInputs = toggleMissionInputs;
		_deploymentPoints = deploymentPoints;
		_getOrderFlagPosition = getOrderFlagPosition;
		_onRefreshVisuals = refreshVisuals;
		_toggleOrderPositionVisibility = setSuspendTroopPlacer;
		_onActivateToggleOrder = onActivateToggleOrder;
		_onDeactivateToggleOrder = onDeactivateToggleOrder;
		_onTransferTroopsFinishedDelegate = onTransferTroopsFinishedDelegate;
		_onBeforeOrderDelegate = onBeforeOrderDelegate;
		_isMultiplayer = isMultiplayer;
		IsDeployment = isDeployment;
		OrderSetsWithOrdersByType = new Dictionary<OrderSetType, OrderSetVM>();
		OrderSets = new MBBindingList<OrderSetVM>();
		DeploymentController = new MissionOrderDeploymentControllerVM(_deploymentPoints, this, _deploymentCamera, _toggleMissionInputs, _onRefreshVisuals);
		TroopController = new MissionOrderTroopControllerVM(this, _isMultiplayer, IsDeployment, OnTransferFinished);
		PopulateOrderSets();
		Team.OnFormationAIActiveBehaviorChanged += TeamOnFormationAIActiveBehaviorChanged;
		OrderTroopItemVM.OnSelectionChange += OnTroopItemSelectionStateChanged;
		RefreshValues();
		Mission.OnMainAgentChanged += MissionOnMainAgentChanged;
		UpdateCanUseShortcuts(_isMultiplayer);
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		ReturnText = new TextObject("{=EmVbbIUc}Return").ToString();
		foreach (OrderSetVM value in OrderSetsWithOrdersByType.Values)
		{
			value.RefreshValues();
		}
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		OrderTroopItemVM.OnSelectionChange -= OnTroopItemSelectionStateChanged;
		Mission.OnMainAgentChanged -= MissionOnMainAgentChanged;
		DeploymentController.OnFinalize();
		TroopController.OnFinalize();
		_deploymentPoints.Clear();
		foreach (OrderSetVM orderSet in _orderSets)
		{
			orderSet.OnFinalize();
		}
		InputRestrictions = null;
	}

	private void PopulateOrderSets()
	{
		_movementSet = new OrderSetVM(OrderSetType.Movement, OnOrder, _isMultiplayer);
		OrderSetVM orderSetVM = new OrderSetVM(OrderSetType.Form, OnOrder, _isMultiplayer);
		bool num = BannerlordConfig.OrderLayoutType == 1;
		OrderSets.Add(_movementSet);
		OrderSetsWithOrdersByType.Add(OrderSetType.Movement, _movementSet);
		if (num)
		{
			_facingSet = new OrderSetVM(OrderSetType.Facing, OnOrder, _isMultiplayer);
			OrderSets.Add(_facingSet);
			OrderSets.Add(orderSetVM);
			OrderSetsWithOrdersByType.Add(OrderSetType.Facing, _facingSet);
			OrderSetsWithOrdersByType.Add(OrderSetType.Form, orderSetVM);
		}
		else
		{
			OrderSetVM orderSetVM2 = new OrderSetVM(OrderSetType.Toggle, OnOrder, _isMultiplayer);
			OrderSets.Add(orderSetVM);
			OrderSets.Add(orderSetVM2);
			OrderSetsWithOrdersByType.Add(OrderSetType.Form, orderSetVM);
			OrderSetsWithOrdersByType.Add(OrderSetType.Toggle, orderSetVM2);
		}
		OrderSetVM item = new OrderSetVM(OrderSubType.ToggleFire, OrderSets.Count, OnOrder, _isMultiplayer);
		OrderSets.Add(item);
		OrderSetVM item2 = new OrderSetVM(OrderSubType.ToggleMount, OrderSets.Count, OnOrder, _isMultiplayer);
		OrderSets.Add(item2);
		OrderSetVM item3 = new OrderSetVM(OrderSubType.ToggleAI, OrderSets.Count, OnOrder, _isMultiplayer);
		OrderSets.Add(item3);
		if (num)
		{
			if (!_isMultiplayer)
			{
				OrderSetVM item4 = new OrderSetVM(OrderSubType.ToggleTransfer, OrderSets.Count, OnOrder, _isMultiplayer);
				OrderSets.Add(item4);
			}
		}
		else
		{
			OrderSetVM item5 = new OrderSetVM(OrderSubType.ActivationFaceDirection, OrderSets.Count, OnOrder, _isMultiplayer);
			OrderSets.Add(item5);
			OrderSetVM item6 = new OrderSetVM(OrderSubType.FormClose, OrderSets.Count, OnOrder, _isMultiplayer);
			OrderSets.Add(item6);
			OrderSetVM item7 = new OrderSetVM(OrderSubType.FormLine, OrderSets.Count, OnOrder, _isMultiplayer);
			OrderSets.Add(item7);
		}
	}

	private void TeamOnFormationAIActiveBehaviorChanged(Formation formation)
	{
		if (formation.IsAIControlled)
		{
			if (_modifiedAIFormations.IndexOf(formation) < 0)
			{
				_modifiedAIFormations.Add(formation);
			}
			_delayValueForAIFormationModifications = 3;
		}
	}

	private void DisplayFormationAIFeedback()
	{
		_delayValueForAIFormationModifications = Math.Max(0, _delayValueForAIFormationModifications - 1);
		if (_delayValueForAIFormationModifications != 0 || _modifiedAIFormations.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < _modifiedAIFormations.Count; i++)
		{
			Formation formation = _modifiedAIFormations[i];
			if (formation?.AI.ActiveBehavior != null && formation.FormationIndex < FormationClass.NumberOfRegularFormations)
			{
				DisplayFormationAIFeedbackAux(_modifiedAIFormations);
			}
			else
			{
				_modifiedAIFormations[i] = null;
			}
		}
		_modifiedAIFormations.Clear();
	}

	private static void DisplayFormationAIFeedbackAux(List<Formation> formations)
	{
		Dictionary<FormationClass, TextObject> dictionary = new Dictionary<FormationClass, TextObject>();
		Type type = null;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < formations.Count; i++)
		{
			Formation formation = formations[i];
			if (formation?.AI.ActiveBehavior != null && (type == null || type == formation.AI.ActiveBehavior.GetType()))
			{
				type = formation.AI.ActiveBehavior.GetType();
				switch (formation.AI.Side)
				{
				case FormationAI.BehaviorSide.Left:
					flag = true;
					break;
				case FormationAI.BehaviorSide.Right:
					flag2 = true;
					break;
				case FormationAI.BehaviorSide.Middle:
					flag3 = true;
					break;
				}
				if (!dictionary.ContainsKey(formation.PhysicalClass))
				{
					TextObject localizedName = formation.PhysicalClass.GetLocalizedName();
					TextObject textObject = GameTexts.FindText("str_troop_group_name_definite");
					textObject.SetTextVariable("FORMATION_CLASS", localizedName);
					dictionary.Add(formation.PhysicalClass, textObject);
				}
				formations[i] = null;
			}
		}
		if (dictionary.Count == 1)
		{
			MBTextManager.SetTextVariable("IS_PLURAL", 0);
			MBTextManager.SetTextVariable("TROOP_NAMES_BEGIN", TextObject.Empty);
			MBTextManager.SetTextVariable("TROOP_NAMES_END", dictionary.First().Value);
		}
		else
		{
			MBTextManager.SetTextVariable("IS_PLURAL", 1);
			TextObject value = dictionary.Last().Value;
			TextObject textObject2;
			if (dictionary.Count == 2)
			{
				textObject2 = dictionary.First().Value;
			}
			else
			{
				textObject2 = GameTexts.FindText("str_LEFT_comma_RIGHT");
				textObject2.SetTextVariable("LEFT", dictionary.First().Value);
				textObject2.SetTextVariable("RIGHT", dictionary.Last().Value);
				for (int j = 2; j < dictionary.Count - 1; j++)
				{
					TextObject textObject3 = GameTexts.FindText("str_LEFT_comma_RIGHT");
					textObject3.SetTextVariable("LEFT", textObject2);
					textObject3.SetTextVariable("RIGHT", dictionary.Values.ElementAt(j));
					textObject2 = textObject3;
				}
			}
			MBTextManager.SetTextVariable("TROOP_NAMES_BEGIN", textObject2);
			MBTextManager.SetTextVariable("TROOP_NAMES_END", value);
		}
		bool flag4 = (flag ? 1 : 0) + (flag3 ? 1 : 0) + (flag2 ? 1 : 0) > 1;
		MBTextManager.SetTextVariable("IS_LEFT", flag4 ? 2 : (flag ? 1 : 0));
		MBTextManager.SetTextVariable("IS_MIDDLE", (!flag4 && flag3) ? 1 : 0);
		MBTextManager.SetTextVariable("IS_RIGHT", (!flag4 && flag2) ? 1 : 0);
		string name = type.Name;
		InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("str_formation_ai_behavior_text", name).ToString()));
	}

	private void OnTroopItemSelectionStateChanged(OrderTroopItemVM troopItem, bool isSelected)
	{
		for (int i = 0; i < TroopController.TroopList.Count; i++)
		{
			TroopController.TroopList[i].IsTargetRelevant = TroopController.TroopList[i].IsSelected;
		}
	}

	public void OnOrderLayoutTypeChanged()
	{
		TroopController = new MissionOrderTroopControllerVM(this, _isMultiplayer, IsDeployment, OnTransferFinished);
		OrderSets.Clear();
		OrderSetsWithOrdersByType.Clear();
		PopulateOrderSets();
		TroopController.UpdateTroops();
		TroopController.TroopList.ApplyActionOnAllItems(delegate(OrderTroopItemVM x)
		{
			TroopController.SetTroopActiveOrders(x);
		});
		TroopController.OnFiltersSet(_filterData);
	}

	public void OpenToggleOrder(bool fromHold, bool displayMessage = true)
	{
		if (!IsToggleOrderShown && CheckCanBeOpened(displayMessage))
		{
			Mission.Current.IsOrderMenuOpen = true;
			_currentActivationType = (fromHold ? ActivationType.Hold : ActivationType.Click);
			IsToggleOrderShown = true;
			TroopController.IsTransferActive = false;
			DeploymentController.ProcessSiegeMachines();
			if (OrderController.SelectedFormations.IsEmpty())
			{
				TroopController.SelectAllFormations();
			}
			SetActiveOrders();
		}
	}

	private bool CheckCanBeOpened(bool displayMessage = false)
	{
		if (Agent.Main == null)
		{
			if (displayMessage)
			{
				InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=GMhOZGnb}Cannot issue order while dead.").ToString()));
			}
			return false;
		}
		if (Mission.Current.Mode != MissionMode.Deployment && !Agent.Main.IsPlayerControlled)
		{
			if (displayMessage)
			{
				InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=b1DHZsaH}Cannot issue order right now.").ToString()));
			}
			return false;
		}
		if (!Team.HasBots || !PlayerHasAnyTroopUnderThem || (!Team.IsPlayerGeneral && !Team.IsPlayerSergeant))
		{
			if (displayMessage)
			{
				InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=DQvGNQ0g}There isn't any unit under command.").ToString()));
			}
			return false;
		}
		if (Mission.Current.IsMissionEnding)
		{
			return Mission.Current.CheckIfBattleInRetreat();
		}
		return true;
	}

	public bool TryCloseToggleOrder(bool dontApplySelected = false)
	{
		if (IsToggleOrderShown)
		{
			Mission.Current.IsOrderMenuOpen = false;
			if (LastSelectedOrderItem != null && !dontApplySelected)
			{
				ApplySelectedOrder();
			}
			LastSelectedOrderSetType = OrderSetType.None;
			LastSelectedOrderItem = null;
			OrderSets.ApplyActionOnAllItems(delegate(OrderSetVM s)
			{
				s.Orders.ApplyActionOnAllItems(delegate(OrderItemVM o)
				{
					o.IsSelected = false;
				});
			});
			_ = _isDeployment;
			IsToggleOrderShown = false;
			UpdateTitleOrdersKeyVisualVisibility();
			if (!IsDeployment)
			{
				InputRestrictions.ResetInputRestrictions();
				return true;
			}
		}
		return false;
	}

	internal void SetActiveOrders()
	{
		bool flag = ActiveTargetState == 1;
		if (flag)
		{
			DeploymentController.SetCurrentActiveOrders();
		}
		else
		{
			TroopController.SetCurrentActiveOrders();
		}
		List<OrderSubjectVM> list = (flag ? DeploymentController.SiegeMachineList.Cast<OrderSubjectVM>().ToList() : TroopController.TroopList.Cast<OrderSubjectVM>().ToList()).Where((OrderSubjectVM item) => item.IsSelected && item.IsSelectable).ToList();
		OrderSetsWithOrdersByType[OrderSetType.Movement].ResetActiveStatus();
		OrderSetsWithOrdersByType[OrderSetType.Form].ResetActiveStatus(flag && list.IsEmpty());
		if (OrderSetsWithOrdersByType.ContainsKey(OrderSetType.Toggle))
		{
			OrderSetsWithOrdersByType[OrderSetType.Toggle].ResetActiveStatus(flag);
		}
		if (OrderSetsWithOrdersByType.ContainsKey(OrderSetType.Facing))
		{
			OrderSetsWithOrdersByType[OrderSetType.Facing].ResetActiveStatus(flag);
		}
		foreach (OrderSetVM item in OrderSets.Where((OrderSetVM s) => !s.ContainsOrders))
		{
			item.ResetActiveStatus();
		}
		if (list.Count > 0)
		{
			List<OrderItemVM> list2 = new List<OrderItemVM>();
			foreach (OrderSubjectVM item2 in list)
			{
				foreach (OrderItemVM activeOrder in item2.ActiveOrders)
				{
					if (!list2.Contains(activeOrder))
					{
						list2.Add(activeOrder);
					}
				}
			}
			foreach (OrderItemVM item3 in list2)
			{
				item3.SelectionState = 2;
				if (item3.IsTitle)
				{
					item3.SetActiveState(isActive: true);
				}
				if (item3.OrderSetType != OrderSetType.None)
				{
					OrderSetsWithOrdersByType[item3.OrderSetType].SetActiveOrder(OrderSetsWithOrdersByType[item3.OrderSetType].TitleOrder);
				}
			}
			list2 = list[0].ActiveOrders;
			foreach (OrderSubjectVM item4 in list)
			{
				list2 = list2.Intersect(item4.ActiveOrders).ToList();
				if (list2.IsEmpty())
				{
					break;
				}
			}
			foreach (OrderItemVM item5 in list2)
			{
				item5.SelectionState = 3;
				if (item5.OrderSetType != OrderSetType.None)
				{
					OrderSetsWithOrdersByType[item5.OrderSetType].SetActiveOrder(item5);
				}
			}
		}
		OrderSetsWithOrdersByType[OrderSetType.Movement].FinalizeActiveStatus();
		OrderSetsWithOrdersByType[OrderSetType.Form].FinalizeActiveStatus(flag && list.IsEmpty());
		if (OrderSetsWithOrdersByType.ContainsKey(OrderSetType.Toggle))
		{
			OrderSetsWithOrdersByType[OrderSetType.Toggle].FinalizeActiveStatus(flag);
		}
		if (OrderSetsWithOrdersByType.ContainsKey(OrderSetType.Facing))
		{
			OrderSetsWithOrdersByType[OrderSetType.Facing].FinalizeActiveStatus(flag);
		}
		foreach (OrderSetVM item6 in OrderSets.Where((OrderSetVM s) => !s.ContainsOrders))
		{
			item6.FinalizeActiveStatus();
		}
	}

	private void OnOrder(OrderItemVM orderItem, OrderSetType orderSetType, bool fromSelection)
	{
		if (LastSelectedOrderItem == orderItem && fromSelection)
		{
			return;
		}
		_onBeforeOrderDelegate();
		if (LastSelectedOrderItem != null)
		{
			LastSelectedOrderItem.IsSelected = false;
		}
		if (orderItem.IsTitle)
		{
			LastSelectedOrderSetType = orderSetType;
		}
		LastSelectedOrderItem = orderItem;
		if (LastSelectedOrderItem != null)
		{
			LastSelectedOrderItem.IsSelected = true;
			if (LastSelectedOrderItem.OrderSubType == OrderSubType.None)
			{
				OrderSetsWithOrdersByType[LastSelectedOrderSetType].ShowOrders = false;
				LastSelectedOrderSetType = orderSetType;
				OrderSetsWithOrdersByType[LastSelectedOrderSetType].ShowOrders = true;
			}
		}
		if (LastSelectedOrderItem != null && LastSelectedOrderItem.OrderSubType != OrderSubType.None && !fromSelection)
		{
			if (LastSelectedOrderItem.OrderSubType == OrderSubType.Return && OrderSetsWithOrdersByType.TryGetValue(LastSelectedOrderSetType, out var value))
			{
				value.ShowOrders = false;
				UpdateTitleOrdersKeyVisualVisibility();
				LastSelectedOrderSetType = OrderSetType.None;
			}
			else if (_currentActivationType == ActivationType.Hold && LastSelectedOrderSetType != OrderSetType.None)
			{
				ApplySelectedOrder();
				if (LastSelectedOrderItem != null && LastSelectedOrderSetType != OrderSetType.None)
				{
					OrderSetsWithOrdersByType[LastSelectedOrderSetType].ShowOrders = false;
					LastSelectedOrderSetType = OrderSetType.None;
					LastSelectedOrderItem = null;
				}
				OrderSets.ApplyActionOnAllItems(delegate(OrderSetVM s)
				{
					s.Orders.ApplyActionOnAllItems(delegate(OrderItemVM o)
					{
						o.IsSelected = false;
					});
				});
			}
			else if (IsDeployment)
			{
				ApplySelectedOrder();
			}
			else
			{
				TryCloseToggleOrder();
			}
		}
		if (!fromSelection)
		{
			UpdateTitleOrdersKeyVisualVisibility();
		}
	}

	private void UpdateTitleOrdersKeyVisualVisibility()
	{
		bool num = OrderSets.Any((OrderSetVM o) => o.ShowOrders && o.Orders.Count > 0);
		bool? forcedVisibility = null;
		if (num)
		{
			forcedVisibility = false;
		}
		for (int i = 0; i < OrderSets.Count; i++)
		{
			OrderSets[i].TitleOrder.ShortcutKey.SetForcedVisibility(forcedVisibility);
		}
	}

	public void SetFocusedFormations(MBReadOnlyList<Formation> focusedFormationsCache)
	{
		_focusedFormationsCache = focusedFormationsCache;
	}

	public void ApplySelectedOrder()
	{
		bool flag = _isPressedViewOrders || (LastSelectedOrderItem.OrderSetType == OrderSetType.None && LastSelectedOrderItem.IsTitle) || !LastSelectedOrderItem.IsTitle;
		if (LastSelectedOrderItem == null)
		{
			return;
		}
		if (LastSelectedOrderItem.OrderSubType == OrderSubType.Return)
		{
			if (OrderSetsWithOrdersByType.TryGetValue(LastSelectedOrderSetType, out var value))
			{
				value.ShowOrders = false;
				UpdateTitleOrdersKeyVisualVisibility();
				LastSelectedOrderSetType = OrderSetType.None;
			}
			else
			{
				TryCloseToggleOrder(dontApplySelected: true);
			}
			LastSelectedOrderItem = null;
			return;
		}
		List<TextObject> list = new List<TextObject>();
		if (OrderController.SelectedFormations.Count == 0 && ActiveTargetState == 0)
		{
			TroopController.UpdateTroops();
			TroopController.SelectAllFormations();
		}
		if (LastSelectedOrderItem.OrderSubType != OrderSubType.ToggleTransfer && flag)
		{
			if (ActiveTargetState == 1)
			{
				foreach (OrderSiegeMachineVM item in DeploymentController.SiegeMachineList.Where((OrderSiegeMachineVM item) => item.IsSelected && ((LastSelectedOrderItem.OrderSetType != OrderSetType.Toggle && LastSelectedOrderItem.OrderSubType != OrderSubType.ToggleFacing) || !item.IsPrimarySiegeMachine)))
				{
					list.Add(GameTexts.FindText("str_siege_engine", item.MachineClass));
				}
			}
			else
			{
				foreach (OrderTroopItemVM item2 in TroopController.TroopList.Where((OrderTroopItemVM item) => item.IsSelected))
				{
					list.Add(GameTexts.FindText("str_formation_class_string", item2.Formation.PhysicalClass.GetName()));
				}
			}
		}
		if (!list.IsEmpty())
		{
			TextObject textObject = new TextObject("{=ApD0xQXT}{STR1}: {STR2}");
			textObject.SetTextVariable("STR1", GameTexts.GameTextHelper.MergeTextObjectsWithComma(list, includeAnd: false));
			textObject.SetTextVariable("STR2", LastSelectedOrderItem.MainTitle);
			InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
		}
		if (LastSelectedOrderSetType != OrderSetType.None)
		{
			OrderSetsWithOrdersByType[LastSelectedOrderSetType].ShowOrders = false;
			foreach (OrderSetVM value2 in OrderSetsWithOrdersByType.Values)
			{
				value2.TitleOrder.IsActive = value2.TitleOrder.SelectionState != 0;
			}
		}
		if (ActiveTargetState == 0)
		{
			switch (LastSelectedOrderItem.OrderSubType)
			{
			case OrderSubType.MoveToPosition:
			{
				Vec3 position = _getOrderFlagPosition();
				WorldPosition unitPosition = new WorldPosition(Mission.Scene, UIntPtr.Zero, position, hasValidZ: false);
				if (Mission.IsFormationUnitPositionAvailable(ref unitPosition, Team))
				{
					OrderController.SetOrderWithTwoPositions(OrderType.MoveToLineSegment, unitPosition, unitPosition);
				}
				break;
			}
			case OrderSubType.Charge:
			{
				MBReadOnlyList<Formation> focusedFormationsCache2 = _focusedFormationsCache;
				if (focusedFormationsCache2 != null && focusedFormationsCache2.Count > 0)
				{
					OrderController.SetOrderWithFormation(OrderType.Charge, _focusedFormationsCache[0]);
				}
				else
				{
					OrderController.SetOrder(OrderType.Charge);
				}
				break;
			}
			case OrderSubType.FollowMe:
				OrderController.SetOrderWithAgent(OrderType.FollowMe, Agent.Main);
				break;
			case OrderSubType.Stop:
				OrderController.SetOrder(OrderType.StandYourGround);
				break;
			case OrderSubType.Retreat:
				OrderController.SetOrder(OrderType.Retreat);
				break;
			case OrderSubType.Advance:
			{
				MBReadOnlyList<Formation> focusedFormationsCache = _focusedFormationsCache;
				if (focusedFormationsCache != null && focusedFormationsCache.Count > 0)
				{
					OrderController.SetOrderWithFormation(OrderType.Advance, _focusedFormationsCache[0]);
				}
				else
				{
					OrderController.SetOrder(OrderType.Advance);
				}
				break;
			}
			case OrderSubType.Fallback:
				OrderController.SetOrder(OrderType.FallBack);
				break;
			case OrderSubType.FormLine:
				OrderController.SetOrder(OrderType.ArrangementLine);
				break;
			case OrderSubType.FormClose:
				OrderController.SetOrder(OrderType.ArrangementCloseOrder);
				break;
			case OrderSubType.FormLoose:
				OrderController.SetOrder(OrderType.ArrangementLoose);
				break;
			case OrderSubType.FormCircular:
				OrderController.SetOrder(OrderType.ArrangementCircular);
				break;
			case OrderSubType.FormSchiltron:
				OrderController.SetOrder(OrderType.ArrangementSchiltron);
				break;
			case OrderSubType.FormV:
				OrderController.SetOrder(OrderType.ArrangementVee);
				break;
			case OrderSubType.FormColumn:
				OrderController.SetOrder(OrderType.ArrangementColumn);
				break;
			case OrderSubType.FormScatter:
				OrderController.SetOrder(OrderType.ArrangementScatter);
				break;
			case OrderSubType.ToggleFacing:
				if (OrderController.GetActiveFacingOrderOf(OrderController.SelectedFormations.FirstOrDefault()) == OrderType.LookAtDirection)
				{
					OrderController.SetOrder(OrderType.LookAtEnemy);
				}
				else
				{
					OrderController.SetOrderWithPosition(OrderType.LookAtDirection, new WorldPosition(Mission.Scene, UIntPtr.Zero, _getOrderFlagPosition(), hasValidZ: false));
				}
				break;
			case OrderSubType.ActivationFaceDirection:
				OrderController.SetOrderWithPosition(OrderType.LookAtDirection, new WorldPosition(Mission.Scene, UIntPtr.Zero, _getOrderFlagPosition(), hasValidZ: false));
				break;
			case OrderSubType.FaceEnemy:
				OrderController.SetOrder(OrderType.LookAtEnemy);
				break;
			case OrderSubType.ToggleFire:
				if (LastSelectedOrderItem.SelectionState == 3)
				{
					OrderController.SetOrder(OrderType.FireAtWill);
				}
				else
				{
					OrderController.SetOrder(OrderType.HoldFire);
				}
				break;
			case OrderSubType.ToggleMount:
				if (LastSelectedOrderItem.SelectionState == 3)
				{
					OrderController.SetOrder(OrderType.Mount);
				}
				else
				{
					OrderController.SetOrder(OrderType.Dismount);
				}
				break;
			case OrderSubType.ToggleAI:
				if (LastSelectedOrderItem.SelectionState == 3)
				{
					OrderController.SetOrder(OrderType.AIControlOff);
					break;
				}
				OrderController.SetOrder(OrderType.AIControlOn);
				foreach (Formation selectedFormation in OrderController.SelectedFormations)
				{
					TeamOnFormationAIActiveBehaviorChanged(selectedFormation);
				}
				break;
			case OrderSubType.ToggleTransfer:
			{
				if (IsDeployment)
				{
					break;
				}
				foreach (OrderTroopItemVM transferTarget in TroopController.TransferTargetList)
				{
					transferTarget.IsSelected = false;
					transferTarget.IsSelectable = !OrderController.IsFormationListening(transferTarget.Formation);
				}
				OrderTroopItemVM orderTroopItemVM = TroopController.TransferTargetList.FirstOrDefault((OrderTroopItemVM t) => t.IsSelectable);
				if (orderTroopItemVM != null)
				{
					orderTroopItemVM.IsSelected = true;
					TroopController.IsTransferValid = true;
					GameTexts.SetVariable("{FORMATION_INDEX}", Common.ToRoman(orderTroopItemVM.Formation.Index + 1));
					TroopController.TransferTitleText = new TextObject("{=DvnRkWQg}Transfer Troops To {FORMATION_INDEX}").ToString();
					TroopController.IsTransferActive = true;
					TroopController.IsTransferValid = false;
					TroopController.TransferMaxValue = TroopController.TroopList.Where((OrderTroopItemVM t) => t.IsSelected).Sum((OrderTroopItemVM t) => t.CurrentMemberCount);
					TroopController.TransferValue = TroopController.TransferMaxValue;
					InputRestrictions.SetInputRestrictions();
				}
				else
				{
					MBInformationManager.AddQuickInformation(new TextObject("{=SLY8z9fP}All formations are selected!"));
				}
				break;
			}
			case OrderSubType.None:
			case OrderSubType.Return:
				return;
			}
		}
		else
		{
			switch (LastSelectedOrderItem.OrderSubType)
			{
			case OrderSubType.MoveToPosition:
				OrderController.SiegeWeaponController.SetOrder(SiegeWeaponOrderType.Attack);
				break;
			case OrderSubType.Stop:
				OrderController.SiegeWeaponController.SetOrder(SiegeWeaponOrderType.Stop);
				break;
			case OrderSubType.ToggleFacing:
				OrderController.SiegeWeaponController.SetOrder(SiegeWeaponOrderType.FireAtWalls);
				break;
			case OrderSubType.Return:
				return;
			}
		}
		if (ActiveTargetState == 0)
		{
			IEnumerable<OrderTroopItemVM> enumerable = TroopController.TroopList.Where((OrderTroopItemVM item) => item.IsSelected);
			enumerable.Count();
			foreach (OrderTroopItemVM item3 in enumerable)
			{
				TroopController.SetTroopActiveOrders(item3);
				item3.IsTargetRelevant = item3.IsSelected;
			}
		}
		else
		{
			foreach (OrderSiegeMachineVM item4 in DeploymentController.SiegeMachineList.Where((OrderSiegeMachineVM item) => item.IsSelected))
			{
				DeploymentController.SetSiegeMachineActiveOrders(item4);
			}
		}
		UpdateTitleOrdersKeyVisualVisibility();
		LastSelectedOrderItem = null;
		LastSelectedOrderSetType = OrderSetType.None;
	}

	public void AfterInitialize()
	{
		TroopController.UpdateTroops();
		if (!IsDeployment)
		{
			TroopController.SelectAllFormations(uiFeedback: false);
		}
		DeploymentController.SetCurrentActiveOrders();
	}

	public void Update()
	{
		if (IsToggleOrderShown)
		{
			if (!CheckCanBeOpened())
			{
				if (IsToggleOrderShown)
				{
					TryCloseToggleOrder();
				}
			}
			else if (_updateTroopsTimer.Check(MBCommon.GetApplicationTime()))
			{
				TroopController.IntervalUpdate();
			}
			TroopController.RefreshTroopFormationTargetVisuals();
		}
		DeploymentController.Update();
		DisplayFormationAIFeedback();
	}

	public void OnEscape()
	{
		if (!IsToggleOrderShown)
		{
			return;
		}
		if (_currentActivationType == ActivationType.Hold)
		{
			if (LastSelectedOrderItem != null)
			{
				UpdateTitleOrdersKeyVisualVisibility();
				OrderSetsWithOrdersByType[LastSelectedOrderSetType].ShowOrders = false;
				LastSelectedOrderItem = null;
			}
		}
		else if (_currentActivationType == ActivationType.Click)
		{
			LastSelectedOrderItem = null;
			if (LastSelectedOrderSetType != OrderSetType.None)
			{
				UpdateTitleOrdersKeyVisualVisibility();
				OrderSetsWithOrdersByType[LastSelectedOrderSetType].ShowOrders = false;
				LastSelectedOrderSetType = OrderSetType.None;
			}
			else
			{
				LastSelectedOrderSetType = OrderSetType.None;
				TryCloseToggleOrder();
			}
		}
	}

	public void ViewOrders()
	{
		_isPressedViewOrders = true;
		if (!IsToggleOrderShown)
		{
			TroopController.UpdateTroops();
			OpenToggleOrder(fromHold: false);
		}
		else
		{
			TryCloseToggleOrder();
		}
		_isPressedViewOrders = false;
	}

	public void OnSelect(int formationTroopIndex)
	{
		if (CheckCanBeOpened(displayMessage: true))
		{
			if (ActiveTargetState == 0)
			{
				TroopController.OnSelectFormationWithIndex(formationTroopIndex);
			}
			else if (ActiveTargetState == 1)
			{
				DeploymentController.OnSelectFormationWithIndex(formationTroopIndex);
			}
			OpenToggleOrder(fromHold: false);
		}
	}

	public void OnGiveOrder(int pressedIndex)
	{
		if (!CheckCanBeOpened(displayMessage: true))
		{
			return;
		}
		OrderSetVM orderSetVM = OrderSetsWithOrdersByType.Values.FirstOrDefault((OrderSetVM o) => o.ShowOrders);
		if (orderSetVM != null)
		{
			if (orderSetVM.Orders.Count > pressedIndex)
			{
				orderSetVM.Orders[pressedIndex].ExecuteAction();
			}
			else if (pressedIndex == 8)
			{
				orderSetVM.Orders[orderSetVM.Orders.Count - 1].ExecuteAction();
			}
		}
		else
		{
			int num = (int)TaleWorlds.Library.MathF.Clamp(pressedIndex, 0f, OrderSets.Count - 1);
			if (num >= 0 && OrderSets.Count > num && OrderSets[num].TitleOrder.SelectionState != 0)
			{
				OpenToggleOrder(fromHold: false);
				OrderSets[num].TitleOrder.ExecuteAction();
			}
		}
	}

	private void MissionOnMainAgentChanged(object sender, PropertyChangedEventArgs e)
	{
		if (Mission.MainAgent == null)
		{
			TryCloseToggleOrder();
			Mission.IsOrderMenuOpen = false;
		}
	}

	internal void OnDeployAll()
	{
		TroopController.UpdateTroops();
		foreach (OrderTroopItemVM troop in TroopController.TroopList)
		{
			TroopController.SetTroopActiveOrders(troop);
		}
		if (!IsDeployment)
		{
			TroopController.SelectAllFormations(uiFeedback: false);
		}
	}

	private void OnOrderShownToggle()
	{
		IsTroopListShown = IsToggleOrderShown && !IsDeployment;
		if (!_isDeployment)
		{
			if (IsToggleOrderShown)
			{
				_onActivateToggleOrder();
			}
			else
			{
				_onDeactivateToggleOrder();
			}
		}
		_updateTroopsTimer = (IsToggleOrderShown ? new Timer(MBCommon.GetApplicationTime() - 2f, 2f) : null);
		IsTroopPlacingActive = IsToggleOrderShown && ActiveTargetState == 0;
		foreach (OrderSetVM value in OrderSetsWithOrdersByType.Values)
		{
			value.ShowOrders = false;
			value.TitleOrder.IsActive = value.TitleOrder.SelectionState != 0;
		}
		if (!IsDeployment && TroopController.TroopList.FirstOrDefault() != null)
		{
			TroopController.TroopList.ApplyActionOnAllItems(delegate(OrderTroopItemVM t)
			{
				t.IsSelectionActive = false;
			});
			TroopController.TroopList[0].IsSelectionActive = true;
		}
		_onRefreshVisuals();
		if (!IsToggleOrderShown)
		{
			_currentActivationType = ActivationType.NotActive;
		}
	}

	public void SelectNextTroop(int direction)
	{
		if (!CheckCanBeOpened(displayMessage: true) || TroopController.TroopList.Count <= 0)
		{
			return;
		}
		OrderTroopItemVM orderTroopItemVM = TroopController.TroopList.FirstOrDefault((OrderTroopItemVM t) => t.IsSelectionActive);
		if (orderTroopItemVM != null)
		{
			int num = ((direction > 0) ? 1 : (-1));
			orderTroopItemVM.IsSelectionActive = false;
			int num2 = TroopController.TroopList.IndexOf(orderTroopItemVM) + num;
			for (int i = 0; i < TroopController.TroopList.Count; i++)
			{
				int num3 = (num2 + i * num) % TroopController.TroopList.Count;
				if (num3 < 0)
				{
					num3 += TroopController.TroopList.Count;
				}
				if (TroopController.TroopList[num3].IsSelectable)
				{
					TroopController.TroopList[num3].IsSelectionActive = true;
					break;
				}
			}
		}
		else
		{
			TroopController.TroopList.FirstOrDefault().IsSelectionActive = true;
		}
	}

	public void ToggleSelectionForCurrentTroop()
	{
		if (!CheckCanBeOpened(displayMessage: true))
		{
			return;
		}
		OrderTroopItemVM orderTroopItemVM = TroopController.TroopList.FirstOrDefault((OrderTroopItemVM t) => t.IsSelectionActive);
		if (orderTroopItemVM != null)
		{
			if (orderTroopItemVM.IsSelected)
			{
				TroopController.OnDeselectFormation(orderTroopItemVM);
			}
			else
			{
				TroopController.OnSelectFormation(orderTroopItemVM);
			}
		}
	}

	private void OnTransferFinished()
	{
		_onTransferTroopsFinishedDelegate.DynamicInvokeWithLog();
	}

	internal OrderSetType GetOrderSetWithShortcutIndex(int index)
	{
		return index switch
		{
			0 => OrderSetType.Movement, 
			1 => OrderSetType.Form, 
			2 => OrderSetType.Toggle, 
			3 => OrderSetType.Facing, 
			_ => (OrderSetType)index, 
		};
	}

	internal IEnumerable<OrderItemVM> GetAllOrderItemsForSubType(OrderSubType orderSubType)
	{
		IEnumerable<OrderItemVM> first = OrderSets.Select((OrderSetVM s) => s.Orders).SelectMany((MBBindingList<OrderItemVM> o) => o.Where((OrderItemVM l) => l.OrderSubType == orderSubType));
		IEnumerable<OrderItemVM> second = from s in OrderSets
			where s.TitleOrder.OrderSubType == orderSubType
			select s into t
			select t.TitleOrder;
		return first.Union(second);
	}

	[Conditional("DEBUG")]
	private void DebugTick()
	{
		if (!IsToggleOrderShown)
		{
			return;
		}
		string text = "SelectedFormations (" + OrderController.SelectedFormations.Count + ") :";
		foreach (Formation selectedFormation in OrderController.SelectedFormations)
		{
			text = text + " " + selectedFormation.FormationIndex.GetName();
		}
	}

	public void OnDeploymentFinished()
	{
		TroopController.OnDeploymentFinished();
		DeploymentController.FinalizeDeployment();
		IsDeployment = false;
	}

	public void OnFiltersSet(List<(int, List<int>)> filterData)
	{
		_filterData = filterData;
		TroopController.OnFiltersSet(filterData);
	}

	public void UpdateCanUseShortcuts(bool value)
	{
		CanUseShortcuts = value;
		for (int i = 0; i < OrderSets.Count; i++)
		{
			OrderSets[i].UpdateCanUseShortcuts(value);
		}
	}

	public void SetCancelInputKey(HotKey hotKey)
	{
		CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}
}
