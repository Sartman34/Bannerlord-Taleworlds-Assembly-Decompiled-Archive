using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.Order;

public class MissionOrderTroopControllerVM : ViewModel
{
	private class TroopItemFormationIndexComparer : IComparer<OrderTroopItemVM>
	{
		public int Compare(OrderTroopItemVM x, OrderTroopItemVM y)
		{
			int index = x.Formation.Index;
			return index.CompareTo(y.Formation.Index);
		}
	}

	private readonly MissionOrderVM _missionOrder;

	private readonly Action _onTransferFinised;

	private readonly bool _isMultiplayer;

	private List<(int, List<int>)> _filterData;

	private bool _isDeployment;

	private TroopItemFormationIndexComparer _formationIndexComparer;

	private MBBindingList<OrderTroopItemVM> _troopList;

	private bool _isTransferActive;

	private MBBindingList<OrderTroopItemVM> _transferTargetList;

	private int _transferValue;

	private int _transferMaxValue;

	private string _transferTitleText;

	private string _acceptText;

	private string _cancelText;

	private bool _isTransferValid;

	private InputKeyItemVM _doneInputKey;

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _resetInputKey;

	private Mission Mission => Mission.Current;

	private Team Team => Mission.Current.PlayerTeam;

	public OrderController OrderController => Team.PlayerOrderController;

	[DataSourceProperty]
	public bool IsTransferActive
	{
		get
		{
			return _isTransferActive;
		}
		set
		{
			if (value == _isTransferActive)
			{
				return;
			}
			_isTransferActive = value;
			OnPropertyChanged("IsTransferActive");
			_missionOrder.IsTroopPlacingActive = !value;
			if (_missionOrder.OrderSetsWithOrdersByType.ContainsKey(OrderSetType.Toggle))
			{
				_missionOrder.OrderSetsWithOrdersByType[OrderSetType.Toggle].GetOrder(OrderSubType.ToggleTransfer).SelectionState = ((!value) ? 1 : 3);
				_missionOrder.OrderSetsWithOrdersByType[OrderSetType.Toggle].FinalizeActiveStatus();
			}
			if (_isTransferActive)
			{
				foreach (OrderTroopItemVM transferTarget in TransferTargetList)
				{
					transferTarget.SetFormationClassFromFormation(transferTarget.Formation);
					transferTarget.Morale = (int)MissionGameModels.Current.BattleMoraleModel.GetAverageMorale(transferTarget.Formation);
					transferTarget.IsAmmoAvailable = transferTarget.Formation.QuerySystem.RangedUnitRatio > 0f || transferTarget.Formation.QuerySystem.RangedCavalryUnitRatio > 0f;
				}
			}
			if (Mission != null)
			{
				Mission.IsTransferMenuOpen = value;
			}
		}
	}

	[DataSourceProperty]
	public bool IsTransferValid
	{
		get
		{
			return _isTransferValid;
		}
		set
		{
			if (value != _isTransferValid)
			{
				_isTransferValid = value;
				OnPropertyChanged("IsTransferValid");
				if (!value)
				{
					TransferTitleText = "";
				}
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<OrderTroopItemVM> TransferTargetList
	{
		get
		{
			return _transferTargetList;
		}
		set
		{
			if (value != _transferTargetList)
			{
				_transferTargetList = value;
				OnPropertyChanged("TransferTargetList");
			}
		}
	}

	[DataSourceProperty]
	public int TransferMaxValue
	{
		get
		{
			return _transferMaxValue;
		}
		set
		{
			if (value != _transferMaxValue)
			{
				_transferMaxValue = value;
				OnPropertyChanged("TransferMaxValue");
			}
		}
	}

	[DataSourceProperty]
	public int TransferValue
	{
		get
		{
			return _transferValue;
		}
		set
		{
			if (value != _transferValue)
			{
				_transferValue = value;
				OnPropertyChanged("TransferValue");
			}
		}
	}

	[DataSourceProperty]
	public string TransferTitleText
	{
		get
		{
			return _transferTitleText;
		}
		set
		{
			if (value != _transferTitleText)
			{
				_transferTitleText = value;
				OnPropertyChanged("TransferTitleText");
			}
		}
	}

	[DataSourceProperty]
	public string AcceptText
	{
		get
		{
			return _acceptText;
		}
		set
		{
			if (value != _acceptText)
			{
				_acceptText = value;
				OnPropertyChanged("AcceptText");
			}
		}
	}

	[DataSourceProperty]
	public string CancelText
	{
		get
		{
			return _cancelText;
		}
		set
		{
			if (value != _cancelText)
			{
				_cancelText = value;
				OnPropertyChanged("CancelText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<OrderTroopItemVM> TroopList
	{
		get
		{
			return _troopList;
		}
		set
		{
			if (value != _troopList)
			{
				_troopList = value;
				OnPropertyChanged("TroopList");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM DoneInputKey
	{
		get
		{
			return _doneInputKey;
		}
		set
		{
			if (value != _doneInputKey)
			{
				_doneInputKey = value;
				OnPropertyChangedWithValue(value, "DoneInputKey");
			}
		}
	}

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
	public InputKeyItemVM ResetInputKey
	{
		get
		{
			return _resetInputKey;
		}
		set
		{
			if (value != _resetInputKey)
			{
				_resetInputKey = value;
				OnPropertyChangedWithValue(value, "ResetInputKey");
			}
		}
	}

	public MissionOrderTroopControllerVM(MissionOrderVM missionOrder, bool isMultiplayer, bool isDeployment, Action onTransferFinised)
	{
		_missionOrder = missionOrder;
		_onTransferFinised = onTransferFinised;
		_isMultiplayer = isMultiplayer;
		_isDeployment = isDeployment;
		TroopList = new MBBindingList<OrderTroopItemVM>();
		TransferTargetList = new MBBindingList<OrderTroopItemVM>();
		TroopList.Clear();
		TransferTargetList.Clear();
		for (int i = 0; i < 8; i++)
		{
			OrderTroopItemVM orderTroopItemVM = new OrderTroopItemVM(Team.GetFormation((FormationClass)i), ExecuteSelectTransferTroop, GetFormationMorale);
			TransferTargetList.Add(orderTroopItemVM);
			orderTroopItemVM.IsSelected = false;
		}
		Team.OnOrderIssued += OrderController_OnTroopOrderIssued;
		if (TroopList.Count > 0)
		{
			OnSelectFormation(TroopList[0]);
		}
		_formationIndexComparer = new TroopItemFormationIndexComparer();
		SortFormations();
		RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		_troopList.ApplyActionOnAllItems(delegate(OrderTroopItemVM x)
		{
			x.RefreshValues();
		});
		AcceptText = GameTexts.FindText("str_selection_widget_accept").ToString();
		CancelText = GameTexts.FindText("str_selection_widget_cancel").ToString();
	}

	public void ExecuteSelectAll()
	{
		SelectAllFormations();
	}

	public void ExecuteSelectTransferTroop(OrderTroopItemVM targetTroop)
	{
		foreach (OrderTroopItemVM transferTarget in TransferTargetList)
		{
			transferTarget.IsSelected = false;
		}
		targetTroop.IsSelected = targetTroop.IsSelectable;
		IsTransferValid = targetTroop.IsSelectable;
		GameTexts.SetVariable("FORMATION_INDEX", Common.ToRoman(targetTroop.Formation.Index + 1));
		TransferTitleText = new TextObject("{=DvnRkWQg}Transfer Troops To {FORMATION_INDEX}").ToString();
	}

	public void ExecuteConfirmTransfer()
	{
		IsTransferActive = false;
		OrderTroopItemVM orderTroopItemVM = TransferTargetList.Single((OrderTroopItemVM t) => t.IsSelected);
		int transferValue = TransferValue;
		int b = TroopList.Where((OrderTroopItemVM t) => t.IsSelected).Sum((OrderTroopItemVM t) => t.CurrentMemberCount);
		transferValue = TaleWorlds.Library.MathF.Min(transferValue, b);
		OrderController.SetOrderWithFormationAndNumber(OrderType.Transfer, orderTroopItemVM.Formation, transferValue);
		for (int i = 0; i < TroopList.Count; i++)
		{
			OrderTroopItemVM orderTroopItemVM2 = TroopList[i];
			if (!orderTroopItemVM2.ContainsDeadTroop && orderTroopItemVM2.CurrentMemberCount == 0)
			{
				TroopList.RemoveAt(i);
				i--;
			}
		}
		_onTransferFinised?.DynamicInvokeWithLog();
	}

	public void ExecuteCancelTransfer()
	{
		IsTransferActive = false;
		_onTransferFinised?.DynamicInvokeWithLog();
	}

	public void ExecuteReset()
	{
		RefreshValues();
	}

	internal void SetTroopActiveOrders(OrderTroopItemVM item)
	{
		bool flag = BannerlordConfig.OrderLayoutType == 1;
		item.ActiveOrders.Clear();
		List<OrderSubType> list = new List<OrderSubType>();
		OrderType orderType = OrderUIHelper.GetOrderOverrideForUI(item.Formation, OrderSetType.Movement);
		if (orderType == OrderType.None)
		{
			orderType = OrderController.GetActiveMovementOrderOf(item.Formation);
		}
		switch (orderType)
		{
		case OrderType.Move:
			list.Add(OrderSubType.MoveToPosition);
			break;
		case OrderType.Charge:
			list.Add(OrderSubType.Charge);
			break;
		case OrderType.FollowMe:
			list.Add(OrderSubType.FollowMe);
			break;
		case OrderType.StandYourGround:
			list.Add(OrderSubType.Stop);
			break;
		case OrderType.Retreat:
			list.Add(OrderSubType.Retreat);
			break;
		case OrderType.Advance:
			list.Add(OrderSubType.Advance);
			break;
		case OrderType.FallBack:
			list.Add(OrderSubType.Fallback);
			break;
		case OrderType.ChargeWithTarget:
			list.Add(OrderSubType.Charge);
			break;
		default:
			Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.ViewModelCollection\\Order\\MissionOrderTroopControllerVM.cs", "SetTroopActiveOrders", 175);
			break;
		}
		OrderType orderType2 = OrderUIHelper.GetOrderOverrideForUI(item.Formation, OrderSetType.Form);
		if (orderType2 == OrderType.None)
		{
			orderType2 = OrderController.GetActiveArrangementOrderOf(item.Formation);
		}
		switch (orderType2)
		{
		case OrderType.ArrangementLine:
			list.Add(OrderSubType.FormLine);
			break;
		case OrderType.ArrangementCloseOrder:
			list.Add(OrderSubType.FormClose);
			break;
		case OrderType.ArrangementLoose:
			list.Add(OrderSubType.FormLoose);
			break;
		case OrderType.ArrangementCircular:
			list.Add(OrderSubType.FormCircular);
			break;
		case OrderType.ArrangementSchiltron:
			list.Add(OrderSubType.FormSchiltron);
			break;
		case OrderType.ArrangementVee:
			list.Add(OrderSubType.FormV);
			break;
		case OrderType.ArrangementColumn:
			list.Add(OrderSubType.FormColumn);
			break;
		case OrderType.ArrangementScatter:
			list.Add(OrderSubType.FormScatter);
			break;
		default:
			Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.ViewModelCollection\\Order\\MissionOrderTroopControllerVM.cs", "SetTroopActiveOrders", 220);
			break;
		}
		switch (OrderController.GetActiveRidingOrderOf(item.Formation))
		{
		case OrderType.Dismount:
			list.Add(OrderSubType.ToggleMount);
			break;
		default:
			Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.ViewModelCollection\\Order\\MissionOrderTroopControllerVM.cs", "SetTroopActiveOrders", 234);
			break;
		case OrderType.Mount:
			break;
		}
		switch (OrderController.GetActiveFiringOrderOf(item.Formation))
		{
		case OrderType.HoldFire:
			list.Add(OrderSubType.ToggleFire);
			break;
		default:
			Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.ViewModelCollection\\Order\\MissionOrderTroopControllerVM.cs", "SetTroopActiveOrders", 248);
			break;
		case OrderType.FireAtWill:
			break;
		}
		if (!_isMultiplayer)
		{
			switch (OrderController.GetActiveAIControlOrderOf(item.Formation))
			{
			case OrderType.AIControlOn:
				list.Add(OrderSubType.ToggleAI);
				break;
			default:
				Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.ViewModelCollection\\Order\\MissionOrderTroopControllerVM.cs", "SetTroopActiveOrders", 264);
				break;
			case OrderType.AIControlOff:
				break;
			}
		}
		switch (OrderController.GetActiveFacingOrderOf(item.Formation))
		{
		case OrderType.LookAtDirection:
			list.Add(OrderSubType.ActivationFaceDirection);
			break;
		case OrderType.LookAtEnemy:
			list.Add(flag ? OrderSubType.FaceEnemy : OrderSubType.ToggleFacing);
			break;
		default:
			Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.ViewModelCollection\\Order\\MissionOrderTroopControllerVM.cs", "SetTroopActiveOrders", 280);
			break;
		}
		foreach (OrderSubType item2 in list)
		{
			item.ActiveOrders.AddRange(_missionOrder.GetAllOrderItemsForSubType(item2));
		}
	}

	internal void SelectAllFormations(bool uiFeedback = true)
	{
		foreach (OrderSetVM value in _missionOrder.OrderSetsWithOrdersByType.Values)
		{
			value.ShowOrders = false;
		}
		if (TroopList.Any((OrderTroopItemVM t) => t.IsSelectable))
		{
			OrderController.SelectAllFormations(uiFeedback);
			if (uiFeedback && OrderController.SelectedFormations.Count > 0)
			{
				InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=xTv4tCbZ}Everybody!! Listen to me").ToString()));
			}
		}
		foreach (OrderTroopItemVM troop in TroopList)
		{
			troop.IsSelected = troop.IsSelectable;
		}
		_missionOrder.SetActiveOrders();
	}

	internal void AddSelectedFormation(OrderTroopItemVM item)
	{
		if (item.IsSelectable)
		{
			Formation formation = Team.GetFormation(item.InitialFormationClass);
			OrderController.SelectFormation(formation);
			item.IsSelected = true;
			_missionOrder.SetActiveOrders();
		}
	}

	internal void SetSelectedFormation(OrderTroopItemVM item)
	{
		UpdateTroops();
		if (!item.IsSelectable)
		{
			return;
		}
		OrderController.ClearSelectedFormations();
		foreach (OrderTroopItemVM troop in TroopList)
		{
			troop.IsSelected = false;
		}
		AddSelectedFormation(item);
	}

	public void OnDeselectFormation(int index)
	{
		OrderTroopItemVM item = TroopList.FirstOrDefault((OrderTroopItemVM t) => t.Formation.Index == index);
		OnDeselectFormation(item);
	}

	internal void OnDeselectFormation(OrderTroopItemVM item)
	{
		if (item == null)
		{
			return;
		}
		Formation formation = Team.GetFormation(item.InitialFormationClass);
		if (OrderController.SelectedFormations.Contains(formation))
		{
			OrderController.DeselectFormation(formation);
		}
		item.IsSelected = false;
		if (_isDeployment)
		{
			if (TroopList.Count((OrderTroopItemVM t) => t.IsSelected) != 0)
			{
				_missionOrder.SetActiveOrders();
				return;
			}
			_missionOrder.TryCloseToggleOrder(dontApplySelected: true);
			_missionOrder.IsTroopPlacingActive = false;
		}
		else
		{
			_missionOrder.SetActiveOrders();
		}
	}

	internal void OnSelectFormation(OrderTroopItemVM item)
	{
		foreach (OrderSetVM value in _missionOrder.OrderSetsWithOrdersByType.Values)
		{
			value.ShowOrders = false;
		}
		UpdateTroops();
		_missionOrder.IsTroopPlacingActive = true;
		if (TaleWorlds.InputSystem.Input.IsKeyDown(InputKey.LeftControl))
		{
			if (item.IsSelected)
			{
				OnDeselectFormation(item);
			}
			else
			{
				AddSelectedFormation(item);
			}
		}
		else
		{
			SetSelectedFormation(item);
		}
		if (!IsTransferActive)
		{
			return;
		}
		foreach (OrderTroopItemVM transferTarget in TransferTargetList)
		{
			transferTarget.IsSelectable = !OrderController.IsFormationListening(transferTarget.Formation);
		}
		IsTransferValid = TransferTargetList.Any((OrderTroopItemVM t) => t.IsSelected && t.IsSelectable);
		TransferMaxValue = TroopList.Where((OrderTroopItemVM t) => t.IsSelected).Sum((OrderTroopItemVM t) => t.CurrentMemberCount);
		TransferValue = TransferMaxValue;
	}

	internal void CheckSelectableFormations()
	{
		foreach (OrderTroopItemVM troop in TroopList)
		{
			Formation formation = Team.GetFormation(troop.InitialFormationClass);
			if (formation != null)
			{
				bool isSelectable = OrderController.IsFormationSelectable(formation);
				troop.IsSelectable = isSelectable;
				if (!troop.IsSelectable && troop.IsSelected)
				{
					OnDeselectFormation(troop);
				}
			}
		}
	}

	internal void UpdateTroops()
	{
		List<Formation> list = ((Mission.MainAgent == null || Mission.MainAgent.Controller == Agent.ControllerType.Player) ? Team.FormationsIncludingEmpty.Where((Formation f) => f.CountOfUnits > 0 && (!f.IsPlayerTroopInFormation || f.CountOfUnits > 1)).ToList() : Team.FormationsIncludingEmpty.Where((Formation f) => f.CountOfUnits > 0).ToList());
		foreach (OrderTroopItemVM troop in TroopList)
		{
			SetTroopActiveOrders(troop);
			troop.IsSelectable = OrderController.IsFormationSelectable(troop.Formation);
			if (troop.IsSelectable && OrderController.IsFormationListening(troop.Formation))
			{
				troop.IsSelected = true;
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			Formation formation = list[i];
			if (formation != null && TroopList.All((OrderTroopItemVM item) => item.Formation != formation))
			{
				OrderTroopItemVM troopItem = new OrderTroopItemVM(formation, OnSelectFormation, GetFormationMorale);
				troopItem = AddTroopItemIfNotExist(troopItem);
				SetTroopActiveOrders(troopItem);
				troopItem.IsSelectable = OrderController.IsFormationSelectable(formation);
				if (troopItem.IsSelectable && OrderController.IsFormationListening(formation))
				{
					troopItem.IsSelected = true;
				}
				SortFormations();
			}
		}
	}

	public void AddTroops(Agent agent)
	{
		if (agent.Team != Team || agent.Formation == null || agent.IsPlayerControlled)
		{
			return;
		}
		Formation formation = agent.Formation;
		OrderTroopItemVM orderTroopItemVM = TroopList.FirstOrDefault((OrderTroopItemVM item) => item.Formation.FormationIndex == formation.FormationIndex);
		if (orderTroopItemVM == null)
		{
			OrderTroopItemVM troopItem = new OrderTroopItemVM(formation, OnSelectFormation, GetFormationMorale);
			troopItem = AddTroopItemIfNotExist(troopItem);
			SetTroopActiveOrders(troopItem);
			troopItem.IsSelectable = OrderController.IsFormationSelectable(formation);
			if (troopItem.IsSelectable && OrderController.IsFormationListening(formation))
			{
				troopItem.IsSelected = true;
			}
		}
		else
		{
			orderTroopItemVM.SetFormationClassFromFormation(formation);
			bool isSelectable = OrderController.IsFormationSelectable(formation);
			orderTroopItemVM.IsSelectable = isSelectable;
		}
	}

	public void RemoveTroops(Agent agent)
	{
		if (agent.Team != Team || agent.Formation == null)
		{
			return;
		}
		Formation formation = agent.Formation;
		OrderTroopItemVM orderTroopItemVM = TroopList.FirstOrDefault((OrderTroopItemVM item) => item.Formation.FormationIndex == formation.FormationIndex);
		if (orderTroopItemVM != null)
		{
			orderTroopItemVM.OnFormationAgentRemoved(agent);
			orderTroopItemVM.SetFormationClassFromFormation(formation);
			orderTroopItemVM.IsSelectable = OrderController.IsFormationSelectable(formation);
			if (!orderTroopItemVM.IsSelectable && orderTroopItemVM.IsSelected)
			{
				OnDeselectFormation(orderTroopItemVM);
			}
		}
	}

	private void OrderController_OnTroopOrderIssued(OrderType orderType, IEnumerable<Formation> appliedFormations, OrderController orderController, params object[] delegateParams)
	{
		foreach (OrderSetVM value in _missionOrder.OrderSetsWithOrdersByType.Values)
		{
			value.TitleOrder.IsActive = value.TitleOrder.SelectionState != 0;
		}
		_missionOrder.OrderSetsWithOrdersByType[OrderSetType.Movement].ShowOrders = false;
		if (orderType == OrderType.Transfer)
		{
			if (!(delegateParams[1] is object[]))
			{
				_ = (int)delegateParams[1];
			}
			Formation formation = delegateParams[0] as Formation;
			OrderTroopItemVM orderTroopItemVM = TroopList.FirstOrDefault((OrderTroopItemVM item) => item.Formation == formation);
			if (orderTroopItemVM == null)
			{
				int index = -1;
				for (int i = 0; i < TroopList.Count; i++)
				{
					if (TroopList[i].Formation.Index > formation.Index)
					{
						index = i;
						break;
					}
				}
				OrderTroopItemVM troopItem = new OrderTroopItemVM(formation, OnSelectFormation, GetFormationMorale);
				troopItem = AddTroopItemIfNotExist(troopItem, index);
				SetTroopActiveOrders(troopItem);
				troopItem.IsSelectable = OrderController.IsFormationSelectable(formation);
				if (troopItem.IsSelectable && OrderController.IsFormationListening(formation))
				{
					troopItem.IsSelected = true;
				}
				OnFiltersSet(_filterData);
			}
			else
			{
				orderTroopItemVM.SetFormationClassFromFormation(formation);
			}
			foreach (Formation sourceFormation2 in appliedFormations)
			{
				OrderTroopItemVM orderTroopItemVM2 = TroopList.FirstOrDefault((OrderTroopItemVM item) => item.Formation == sourceFormation2);
				if (orderTroopItemVM2 == null)
				{
					int index2 = -1;
					for (int j = 0; j < TroopList.Count; j++)
					{
						if (TroopList[j].Formation.Index > sourceFormation2.Index)
						{
							index2 = j;
							break;
						}
					}
					OrderTroopItemVM troopItem2 = new OrderTroopItemVM(sourceFormation2, OnSelectFormation, GetFormationMorale);
					troopItem2 = AddTroopItemIfNotExist(troopItem2, index2);
					SetTroopActiveOrders(troopItem2);
					troopItem2.IsSelectable = OrderController.IsFormationSelectable(sourceFormation2);
					if (troopItem2.IsSelectable && OrderController.IsFormationListening(sourceFormation2))
					{
						troopItem2.IsSelected = true;
					}
					OnFiltersSet(_filterData);
				}
				else
				{
					orderTroopItemVM2.SetFormationClassFromFormation(sourceFormation2);
				}
			}
			int num = 1;
			foreach (Formation sourceFormation in appliedFormations)
			{
				TroopList.FirstOrDefault((OrderTroopItemVM item) => item.Formation.Index == sourceFormation.Index).SetFormationClassFromFormation(sourceFormation);
				num++;
			}
		}
		UpdateTroops();
		SortFormations();
		foreach (OrderTroopItemVM item in TroopList.Where((OrderTroopItemVM item) => item.IsSelected))
		{
			SetTroopActiveOrders(item);
		}
		_missionOrder.SetActiveOrders();
		CheckSelectableFormations();
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		Team.OnOrderIssued -= OrderController_OnTroopOrderIssued;
		foreach (OrderTroopItemVM troop in TroopList)
		{
			troop.OnFinalize();
		}
		_transferTargetList = null;
	}

	internal void IntervalUpdate()
	{
		for (int num = TroopList.Count - 1; num >= 0; num--)
		{
			OrderTroopItemVM orderTroopItemVM = TroopList[num];
			Formation formation = Team.GetFormation(orderTroopItemVM.InitialFormationClass);
			if (formation != null && formation.CountOfUnits > 0)
			{
				orderTroopItemVM.UnderAttackOfType = (int)formation.GetUnderAttackTypeOfUnits();
				orderTroopItemVM.BehaviorType = (int)formation.GetMovementTypeOfUnits();
				if (!_isDeployment)
				{
					orderTroopItemVM.Morale = (int)MissionGameModels.Current.BattleMoraleModel.GetAverageMorale(formation);
					if (orderTroopItemVM.SetFormationClassFromFormation(formation))
					{
						UpdateTroops();
					}
					orderTroopItemVM.IsAmmoAvailable = formation.QuerySystem.RangedUnitRatio > 0f || formation.QuerySystem.RangedCavalryUnitRatio > 0f;
					if (orderTroopItemVM.IsAmmoAvailable)
					{
						int totalCurrentAmmo = 0;
						int totalMaxAmmo = 0;
						orderTroopItemVM.Formation.ApplyActionOnEachUnit(delegate(Agent agent)
						{
							if (!agent.IsMainAgent)
							{
								GetMaxAndCurrentAmmoOfAgent(agent, out var currentAmmo, out var maxAmmo);
								totalCurrentAmmo += currentAmmo;
								totalMaxAmmo += maxAmmo;
							}
						});
						orderTroopItemVM.AmmoPercentage = (float)totalCurrentAmmo / (float)totalMaxAmmo;
					}
				}
			}
			else if (formation != null && formation.CountOfUnits == 0)
			{
				orderTroopItemVM.Morale = 0;
				orderTroopItemVM.SetFormationClassFromFormation(formation);
			}
		}
	}

	internal void RefreshTroopFormationTargetVisuals()
	{
		for (int i = 0; i < TroopList.Count; i++)
		{
			TroopList[i].RefreshTargetedOrderVisual();
		}
	}

	internal void OnSelectFormationWithIndex(int formationTroopIndex)
	{
		UpdateTroops();
		OrderTroopItemVM orderTroopItemVM = TroopList.SingleOrDefault((OrderTroopItemVM t) => t.Formation.Index == formationTroopIndex);
		if (orderTroopItemVM != null)
		{
			if (orderTroopItemVM.IsSelectable)
			{
				OnSelectFormation(orderTroopItemVM);
			}
		}
		else
		{
			SelectAllFormations();
		}
	}

	internal void SetCurrentActiveOrders()
	{
		List<OrderSubjectVM> list = (from item in TroopList.Cast<OrderSubjectVM>().ToList()
			where item.IsSelected && item.IsSelectable
			select item).ToList();
		if (!list.IsEmpty())
		{
			return;
		}
		OrderController.SelectAllFormations();
		foreach (OrderTroopItemVM item in TroopList.Where((OrderTroopItemVM s) => OrderController.SelectedFormations.Contains(s.Formation)))
		{
			item.IsSelected = true;
			item.IsSelectable = true;
			SetTroopActiveOrders(item);
			list.Add(item);
		}
	}

	private void GetMaxAndCurrentAmmoOfAgent(Agent agent, out int currentAmmo, out int maxAmmo)
	{
		currentAmmo = 0;
		maxAmmo = 0;
		for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.ExtraWeaponSlot; equipmentIndex++)
		{
			if (!agent.Equipment[equipmentIndex].IsEmpty && agent.Equipment[equipmentIndex].CurrentUsageItem.IsRangedWeapon)
			{
				currentAmmo = agent.Equipment.GetAmmoAmount(equipmentIndex);
				maxAmmo = agent.Equipment.GetMaxAmmo(equipmentIndex);
				break;
			}
		}
	}

	public void OnFiltersSet(List<(int, List<int>)> filterData)
	{
		if (filterData == null)
		{
			return;
		}
		_filterData = filterData;
		foreach (var filter in filterData)
		{
			TroopList.FirstOrDefault((OrderTroopItemVM f) => f.Formation.Index == filter.Item1)?.UpdateFilterData(filter.Item2);
			TransferTargetList.FirstOrDefault((OrderTroopItemVM f) => f.Formation.Index == filter.Item1)?.UpdateFilterData(filter.Item2);
		}
	}

	public void OnDeploymentFinished()
	{
		_isDeployment = false;
		SortFormations();
		for (int num = TroopList.Count - 1; num >= 0; num--)
		{
			if (TroopList[num].CurrentMemberCount <= 0)
			{
				TroopList.RemoveAt(num);
			}
		}
		SelectAllFormations(uiFeedback: false);
	}

	private void SortFormations()
	{
		TroopList.Sort(_formationIndexComparer);
		TransferTargetList.Sort(_formationIndexComparer);
	}

	private int GetFormationMorale(Formation formation)
	{
		if (!_isDeployment)
		{
			return (int)MissionGameModels.Current.BattleMoraleModel.GetAverageMorale(formation);
		}
		return 0;
	}

	private OrderTroopItemVM AddTroopItemIfNotExist(OrderTroopItemVM troopItem, int index = -1)
	{
		OrderTroopItemVM orderTroopItemVM = null;
		if (troopItem != null)
		{
			bool flag = true;
			orderTroopItemVM = TroopList.FirstOrDefault((OrderTroopItemVM t) => t.Formation.Index == troopItem.Formation.Index);
			if (orderTroopItemVM == null)
			{
				flag = false;
				orderTroopItemVM = troopItem;
			}
			if (flag)
			{
				TroopList.Remove(orderTroopItemVM);
			}
			if (index == -1)
			{
				TroopList.Add(troopItem);
			}
			else
			{
				TroopList.Insert(index, troopItem);
			}
		}
		else
		{
			Debug.FailedAssert("Added troop item is null!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.ViewModelCollection\\Order\\MissionOrderTroopControllerVM.cs", "AddTroopItemIfNotExist", 882);
		}
		return orderTroopItemVM;
	}

	public void SetDoneInputKey(HotKey hotKey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}

	public void SetCancelInputKey(HotKey hotKey)
	{
		CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}

	public void SetResetInputKey(HotKey hotKey)
	{
		ResetInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}
}
