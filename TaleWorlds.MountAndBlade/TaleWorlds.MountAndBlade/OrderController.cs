using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class OrderController
{
	private static readonly ActionIndexCache act_command = ActionIndexCache.Create("act_command");

	private static readonly ActionIndexCache act_command_leftstance = ActionIndexCache.Create("act_command_leftstance");

	private static readonly ActionIndexCache act_command_unarmed = ActionIndexCache.Create("act_command_unarmed");

	private static readonly ActionIndexCache act_command_unarmed_leftstance = ActionIndexCache.Create("act_command_unarmed_leftstance");

	private static readonly ActionIndexCache act_command_2h = ActionIndexCache.Create("act_command_2h");

	private static readonly ActionIndexCache act_command_2h_leftstance = ActionIndexCache.Create("act_command_2h_leftstance");

	private static readonly ActionIndexCache act_command_bow = ActionIndexCache.Create("act_command_bow");

	private static readonly ActionIndexCache act_command_follow = ActionIndexCache.Create("act_command_follow");

	private static readonly ActionIndexCache act_command_follow_leftstance = ActionIndexCache.Create("act_command_follow_leftstance");

	private static readonly ActionIndexCache act_command_follow_unarmed = ActionIndexCache.Create("act_command_follow_unarmed");

	private static readonly ActionIndexCache act_command_follow_unarmed_leftstance = ActionIndexCache.Create("act_command_follow_unarmed_leftstance");

	private static readonly ActionIndexCache act_command_follow_2h = ActionIndexCache.Create("act_command_follow_2h");

	private static readonly ActionIndexCache act_command_follow_2h_leftstance = ActionIndexCache.Create("act_command_follow_2h_leftstance");

	private static readonly ActionIndexCache act_command_follow_bow = ActionIndexCache.Create("act_command_follow_bow");

	private static readonly ActionIndexCache act_horse_command = ActionIndexCache.Create("act_horse_command");

	private static readonly ActionIndexCache act_horse_command_unarmed = ActionIndexCache.Create("act_horse_command_unarmed");

	private static readonly ActionIndexCache act_horse_command_2h = ActionIndexCache.Create("act_horse_command_2h");

	private static readonly ActionIndexCache act_horse_command_bow = ActionIndexCache.Create("act_horse_command_bow");

	private static readonly ActionIndexCache act_horse_command_follow = ActionIndexCache.Create("act_horse_command_follow");

	private static readonly ActionIndexCache act_horse_command_follow_unarmed = ActionIndexCache.Create("act_horse_command_follow_unarmed");

	private static readonly ActionIndexCache act_horse_command_follow_2h = ActionIndexCache.Create("act_horse_command_follow_2h");

	private static readonly ActionIndexCache act_horse_command_follow_bow = ActionIndexCache.Create("act_horse_command_follow_bow");

	public const float FormationGapInLine = 1.5f;

	private readonly Mission _mission;

	private readonly Team _team;

	public Agent Owner;

	private readonly MBList<Formation> _selectedFormations;

	private Dictionary<Formation, float> actualWidths;

	private Dictionary<Formation, int> actualUnitSpacings;

	private List<Func<Formation, MovementOrder, MovementOrder>> orderOverrides;

	private List<(Formation, OrderType)> overridenOrders;

	private bool _gesturesEnabled;

	private bool _formationUpdateEnabledAfterSetOrder = true;

	public SiegeWeaponController SiegeWeaponController { get; private set; }

	public MBReadOnlyList<Formation> SelectedFormations => _selectedFormations;

	public bool FormationUpdateEnabledAfterSetOrder => _formationUpdateEnabledAfterSetOrder;

	public Dictionary<Formation, Formation> simulationFormations { get; private set; }

	public event OnOrderIssuedDelegate OnOrderIssued;

	public event Action OnSelectedFormationsChanged;

	public OrderController(Mission mission, Team team, Agent owner)
	{
		_mission = mission;
		_team = team;
		Owner = owner;
		_gesturesEnabled = true;
		_selectedFormations = new MBList<Formation>();
		SiegeWeaponController = new SiegeWeaponController(mission, _team);
		simulationFormations = new Dictionary<Formation, Formation>();
		actualWidths = new Dictionary<Formation, float>();
		actualUnitSpacings = new Dictionary<Formation, int>();
		foreach (Formation item in _team.FormationsIncludingEmpty)
		{
			item.OnWidthChanged += Formation_OnWidthChanged;
			item.OnUnitSpacingChanged += Formation_OnUnitSpacingChanged;
		}
		if (_team.IsPlayerGeneral)
		{
			foreach (Formation item2 in _team.FormationsIncludingEmpty)
			{
				item2.PlayerOwner = owner;
			}
		}
		CreateDefaultOrderOverrides();
	}

	private void Formation_OnUnitSpacingChanged(Formation formation)
	{
		actualUnitSpacings.Remove(formation);
	}

	private void Formation_OnWidthChanged(Formation formation)
	{
		actualWidths.Remove(formation);
	}

	private void OnSelectedFormationsCollectionChanged()
	{
		this.OnSelectedFormationsChanged?.Invoke();
		foreach (Formation item in SelectedFormations.Except(simulationFormations.Keys))
		{
			simulationFormations[item] = new Formation(null, -1);
		}
	}

	private void SelectFormation(Formation formation, Agent selectorAgent)
	{
		if (!_selectedFormations.Contains(formation) && IsFormationSelectable(formation, selectorAgent))
		{
			if (GameNetwork.IsClient)
			{
				GameNetwork.BeginModuleEventAsClient();
				GameNetwork.WriteMessage(new SelectFormation(formation.Index));
				GameNetwork.EndModuleEventAsClient();
			}
			if (selectorAgent != null && AreGesturesEnabled())
			{
				PlayFormationSelectedGesture(formation, selectorAgent);
			}
			MBDebug.Print(string.Concat(formation?.RepresentativeClass, " added to selected formations."));
			_selectedFormations.Add(formation);
			OnSelectedFormationsCollectionChanged();
		}
		else
		{
			TaleWorlds.Library.Debug.FailedAssert("Formation already selected or is not selectable", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "SelectFormation", 208);
		}
	}

	public void SelectFormation(Formation formation)
	{
		SelectFormation(formation, Owner);
	}

	public void DeselectFormation(Formation formation)
	{
		if (_selectedFormations.Contains(formation))
		{
			if (GameNetwork.IsClient)
			{
				GameNetwork.BeginModuleEventAsClient();
				GameNetwork.WriteMessage(new UnselectFormation(formation.Index));
				GameNetwork.EndModuleEventAsClient();
			}
			MBDebug.Print(string.Concat(formation?.RepresentativeClass, " is removed from selected formations."));
			_selectedFormations.Remove(formation);
			OnSelectedFormationsCollectionChanged();
		}
		else
		{
			TaleWorlds.Library.Debug.FailedAssert("Trying to deselect an unselected formation", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "DeselectFormation", 234);
		}
	}

	public bool IsFormationListening(Formation formation)
	{
		return SelectedFormations.Contains(formation);
	}

	public bool IsFormationSelectable(Formation formation)
	{
		return IsFormationSelectable(formation, Owner);
	}

	public bool BackupAndDisableGesturesEnabled()
	{
		bool gesturesEnabled = _gesturesEnabled;
		_gesturesEnabled = false;
		return gesturesEnabled;
	}

	public void RestoreGesturesEnabled(bool oldValue)
	{
		_gesturesEnabled = oldValue;
	}

	private bool IsFormationSelectable(Formation formation, Agent selectorAgent)
	{
		if (selectorAgent == null || formation.PlayerOwner == selectorAgent)
		{
			return formation.CountOfUnits > 0;
		}
		return false;
	}

	private bool AreGesturesEnabled()
	{
		if (_gesturesEnabled && _mission.IsOrderGesturesEnabled())
		{
			return !GameNetwork.IsClientOrReplay;
		}
		return false;
	}

	private void SelectAllFormations(Agent selectorAgent, bool uiFeedback)
	{
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage(new SelectAllFormations());
			GameNetwork.EndModuleEventAsClient();
		}
		if (uiFeedback && selectorAgent != null && AreGesturesEnabled())
		{
			selectorAgent.MakeVoice(SkinVoiceManager.VoiceType.Everyone, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
		}
		MBDebug.Print("Selected formations being cleared. Select all formations:");
		_selectedFormations.Clear();
		foreach (Formation item in _team.FormationsIncludingEmpty.Where((Formation f) => f.CountOfUnits > 0 && IsFormationSelectable(f, selectorAgent)))
		{
			MBDebug.Print(string.Concat(item.RepresentativeClass, " added to selected formations."));
			_selectedFormations.Add(item);
		}
		OnSelectedFormationsCollectionChanged();
	}

	public void SelectAllFormations(bool uiFeedback = false)
	{
		SelectAllFormations(Owner, uiFeedback);
	}

	public void ClearSelectedFormations()
	{
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage(new ClearSelectedFormations());
			GameNetwork.EndModuleEventAsClient();
		}
		MBDebug.Print("Selected formations being cleared.");
		_selectedFormations.Clear();
		OnSelectedFormationsCollectionChanged();
	}

	public void SetOrder(OrderType orderType)
	{
		MBDebug.Print(string.Concat("SetOrder ", orderType, "on team"));
		BeforeSetOrder(orderType);
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage(new ApplyOrder(orderType));
			GameNetwork.EndModuleEventAsClient();
		}
		switch (orderType)
		{
		case OrderType.Charge:
			foreach (Formation selectedFormation in SelectedFormations)
			{
				selectedFormation.SetMovementOrder(MovementOrder.MovementOrderCharge);
			}
			break;
		case OrderType.StandYourGround:
			foreach (Formation selectedFormation2 in SelectedFormations)
			{
				selectedFormation2.SetMovementOrder(MovementOrder.MovementOrderStop);
			}
			break;
		case OrderType.AdvanceTenPaces:
			foreach (Formation selectedFormation3 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation3);
				if (selectedFormation3.GetReadonlyMovementOrderReference().OrderEnum == MovementOrder.MovementOrderEnum.Move)
				{
					MovementOrder readonlyMovementOrderReference2 = selectedFormation3.GetReadonlyMovementOrderReference();
					readonlyMovementOrderReference2.Advance(selectedFormation3, 7f);
					selectedFormation3.SetMovementOrder(readonlyMovementOrderReference2);
				}
			}
			break;
		case OrderType.FallBackTenPaces:
			foreach (Formation selectedFormation4 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation4);
				if (selectedFormation4.GetReadonlyMovementOrderReference().OrderEnum == MovementOrder.MovementOrderEnum.Move)
				{
					MovementOrder readonlyMovementOrderReference = selectedFormation4.GetReadonlyMovementOrderReference();
					readonlyMovementOrderReference.FallBack(selectedFormation4, 7f);
					selectedFormation4.SetMovementOrder(readonlyMovementOrderReference);
				}
			}
			break;
		case OrderType.Advance:
			foreach (Formation selectedFormation5 in SelectedFormations)
			{
				selectedFormation5.SetMovementOrder(MovementOrder.MovementOrderAdvance);
			}
			break;
		case OrderType.FallBack:
			foreach (Formation selectedFormation6 in SelectedFormations)
			{
				selectedFormation6.SetMovementOrder(MovementOrder.MovementOrderFallBack);
			}
			break;
		case OrderType.Retreat:
			foreach (Formation selectedFormation7 in SelectedFormations)
			{
				selectedFormation7.SetMovementOrder(MovementOrder.MovementOrderRetreat);
			}
			break;
		case OrderType.LookAtEnemy:
		{
			FacingOrder facingOrderLookAtEnemy = FacingOrder.FacingOrderLookAtEnemy;
			foreach (Formation selectedFormation8 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation8);
				selectedFormation8.FacingOrder = facingOrderLookAtEnemy;
			}
			break;
		}
		case OrderType.HoldFire:
			foreach (Formation selectedFormation9 in SelectedFormations)
			{
				selectedFormation9.FiringOrder = FiringOrder.FiringOrderHoldYourFire;
			}
			break;
		case OrderType.FireAtWill:
			foreach (Formation selectedFormation10 in SelectedFormations)
			{
				selectedFormation10.FiringOrder = FiringOrder.FiringOrderFireAtWill;
			}
			break;
		case OrderType.Dismount:
			foreach (Formation selectedFormation11 in SelectedFormations)
			{
				if (selectedFormation11.PhysicalClass.IsMounted() || selectedFormation11.HasAnyMountedUnit)
				{
					TryCancelStopOrder(selectedFormation11);
				}
				selectedFormation11.RidingOrder = RidingOrder.RidingOrderDismount;
			}
			break;
		case OrderType.Mount:
			foreach (Formation selectedFormation12 in SelectedFormations)
			{
				if (selectedFormation12.PhysicalClass.IsMounted() || selectedFormation12.HasAnyMountedUnit)
				{
					TryCancelStopOrder(selectedFormation12);
				}
				selectedFormation12.RidingOrder = RidingOrder.RidingOrderMount;
			}
			break;
		case OrderType.AIControlOn:
			foreach (Formation selectedFormation13 in SelectedFormations)
			{
				selectedFormation13.SetControlledByAI(isControlledByAI: true);
			}
			break;
		case OrderType.AIControlOff:
			foreach (Formation selectedFormation14 in SelectedFormations)
			{
				selectedFormation14.SetControlledByAI(isControlledByAI: false);
			}
			break;
		case OrderType.ArrangementLine:
			foreach (Formation selectedFormation15 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation15);
				selectedFormation15.ArrangementOrder = ArrangementOrder.ArrangementOrderLine;
			}
			break;
		case OrderType.ArrangementCloseOrder:
			foreach (Formation selectedFormation16 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation16);
				selectedFormation16.ArrangementOrder = ArrangementOrder.ArrangementOrderShieldWall;
			}
			break;
		case OrderType.ArrangementLoose:
			foreach (Formation selectedFormation17 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation17);
				selectedFormation17.ArrangementOrder = ArrangementOrder.ArrangementOrderLoose;
			}
			break;
		case OrderType.ArrangementCircular:
			foreach (Formation selectedFormation18 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation18);
				selectedFormation18.ArrangementOrder = ArrangementOrder.ArrangementOrderCircle;
			}
			break;
		case OrderType.ArrangementSchiltron:
			foreach (Formation selectedFormation19 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation19);
				selectedFormation19.ArrangementOrder = ArrangementOrder.ArrangementOrderSquare;
			}
			break;
		case OrderType.ArrangementVee:
			foreach (Formation selectedFormation20 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation20);
				selectedFormation20.ArrangementOrder = ArrangementOrder.ArrangementOrderSkein;
			}
			break;
		case OrderType.ArrangementColumn:
			foreach (Formation selectedFormation21 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation21);
				selectedFormation21.ArrangementOrder = ArrangementOrder.ArrangementOrderColumn;
			}
			break;
		case OrderType.ArrangementScatter:
			foreach (Formation selectedFormation22 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation22);
				selectedFormation22.ArrangementOrder = ArrangementOrder.ArrangementOrderScatter;
			}
			break;
		case OrderType.FormDeep:
			foreach (Formation selectedFormation23 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation23);
				selectedFormation23.FormOrder = FormOrder.FormOrderDeep;
			}
			break;
		case OrderType.FormWide:
			foreach (Formation selectedFormation24 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation24);
				selectedFormation24.FormOrder = FormOrder.FormOrderWide;
			}
			break;
		case OrderType.FormWider:
			foreach (Formation selectedFormation25 in SelectedFormations)
			{
				TryCancelStopOrder(selectedFormation25);
				selectedFormation25.FormOrder = FormOrder.FormOrderWider;
			}
			break;
		default:
			TaleWorlds.Library.Debug.FailedAssert("[DEBUG]Invalid order type.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "SetOrder", 634);
			break;
		}
		AfterSetOrder(orderType);
		if (this.OnOrderIssued != null)
		{
			this.OnOrderIssued(orderType, SelectedFormations, this);
		}
	}

	private static void PlayOrderGestures(OrderType orderType, Agent agent, MBList<Formation> selectedFormations)
	{
		switch (orderType)
		{
		case OrderType.FireAtWill:
			agent.MakeVoice(SkinVoiceManager.VoiceType.FireAtWill, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.HoldFire:
			agent.MakeVoice(SkinVoiceManager.VoiceType.HoldFire, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.Mount:
			agent.MakeVoice(SkinVoiceManager.VoiceType.Mount, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.Dismount:
			agent.MakeVoice(SkinVoiceManager.VoiceType.Dismount, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.Move:
		case OrderType.MoveToLineSegment:
		case OrderType.MoveToLineSegmentWithHorizontalLayout:
			agent.MakeVoice(SkinVoiceManager.VoiceType.Move, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.Charge:
		case OrderType.ChargeWithTarget:
			agent.MakeVoice(SkinVoiceManager.VoiceType.Charge, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.FollowMe:
			agent.MakeVoice(SkinVoiceManager.VoiceType.Follow, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.Retreat:
			agent.MakeVoice(SkinVoiceManager.VoiceType.Retreat, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.AdvanceTenPaces:
		case OrderType.Advance:
			agent.MakeVoice(SkinVoiceManager.VoiceType.Advance, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.FallBackTenPaces:
		case OrderType.FallBack:
			agent.MakeVoice(SkinVoiceManager.VoiceType.FallBack, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.StandYourGround:
			agent.MakeVoice(SkinVoiceManager.VoiceType.Stop, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.ArrangementLine:
			agent.MakeVoice(SkinVoiceManager.VoiceType.FormLine, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.ArrangementCloseOrder:
			agent.MakeVoice(SkinVoiceManager.VoiceType.FormShieldWall, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.ArrangementLoose:
			agent.MakeVoice(SkinVoiceManager.VoiceType.FormLoose, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.ArrangementCircular:
			agent.MakeVoice(SkinVoiceManager.VoiceType.FormCircle, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.ArrangementSchiltron:
			agent.MakeVoice(SkinVoiceManager.VoiceType.FormSquare, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.ArrangementVee:
			agent.MakeVoice(SkinVoiceManager.VoiceType.FormSkein, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.ArrangementColumn:
			agent.MakeVoice(SkinVoiceManager.VoiceType.FormColumn, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.ArrangementScatter:
			agent.MakeVoice(SkinVoiceManager.VoiceType.FormScatter, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.AIControlOn:
			agent.MakeVoice(SkinVoiceManager.VoiceType.CommandDelegate, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.AIControlOff:
			agent.MakeVoice(SkinVoiceManager.VoiceType.CommandUndelegate, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.LookAtEnemy:
			agent.MakeVoice(SkinVoiceManager.VoiceType.FaceEnemy, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case OrderType.LookAtDirection:
			agent.MakeVoice(SkinVoiceManager.VoiceType.FaceDirection, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		}
		if (selectedFormations.Count <= 0 || agent == null || agent.Controller == Agent.ControllerType.AI)
		{
			return;
		}
		MissionWeapon wieldedWeapon = agent.WieldedWeapon;
		switch ((!wieldedWeapon.IsEmpty) ? wieldedWeapon.Item.PrimaryWeapon.WeaponClass : WeaponClass.Undefined)
		{
		case WeaponClass.Undefined:
		case WeaponClass.Stone:
			if (agent.MountAgent == null)
			{
				agent.SetActionChannel(1, (orderType != OrderType.FollowMe) ? (agent.GetIsLeftStance() ? act_command_unarmed_leftstance : act_command_unarmed) : (agent.GetIsLeftStance() ? act_command_follow_unarmed_leftstance : act_command_follow_unarmed), ignorePriority: false, 0uL);
			}
			else
			{
				agent.SetActionChannel(1, (orderType == OrderType.FollowMe) ? act_horse_command_follow_unarmed : act_horse_command_unarmed, ignorePriority: false, 0uL);
			}
			break;
		case WeaponClass.Dagger:
		case WeaponClass.OneHandedSword:
		case WeaponClass.OneHandedAxe:
		case WeaponClass.Mace:
		case WeaponClass.Pick:
		case WeaponClass.OneHandedPolearm:
		case WeaponClass.ThrowingAxe:
		case WeaponClass.ThrowingKnife:
			if (agent.MountAgent == null)
			{
				agent.SetActionChannel(1, (orderType != OrderType.FollowMe) ? (agent.GetIsLeftStance() ? act_command_leftstance : act_command) : (agent.GetIsLeftStance() ? act_command_follow_leftstance : act_command_follow), ignorePriority: false, 0uL);
			}
			else
			{
				agent.SetActionChannel(1, (orderType == OrderType.FollowMe) ? act_horse_command_follow : act_horse_command, ignorePriority: false, 0uL);
			}
			break;
		case WeaponClass.TwoHandedSword:
		case WeaponClass.TwoHandedAxe:
		case WeaponClass.TwoHandedMace:
		case WeaponClass.TwoHandedPolearm:
		case WeaponClass.LowGripPolearm:
		case WeaponClass.Crossbow:
		case WeaponClass.Javelin:
		case WeaponClass.Pistol:
		case WeaponClass.Musket:
			if (agent.MountAgent == null)
			{
				agent.SetActionChannel(1, (orderType != OrderType.FollowMe) ? (agent.GetIsLeftStance() ? act_command_2h_leftstance : act_command_2h) : (agent.GetIsLeftStance() ? act_command_follow_2h_leftstance : act_command_follow_2h), ignorePriority: false, 0uL);
			}
			else
			{
				agent.SetActionChannel(1, (orderType == OrderType.FollowMe) ? act_horse_command_follow_2h : act_horse_command_2h, ignorePriority: false, 0uL);
			}
			break;
		case WeaponClass.Bow:
			if (agent.MountAgent == null)
			{
				agent.SetActionChannel(1, (orderType == OrderType.FollowMe) ? act_command_follow_bow : act_command_bow, ignorePriority: false, 0uL);
			}
			else
			{
				agent.SetActionChannel(1, (orderType == OrderType.FollowMe) ? act_horse_command_follow_bow : act_horse_command_bow, ignorePriority: false, 0uL);
			}
			break;
		default:
			TaleWorlds.Library.Debug.FailedAssert("Unexpected weapon class.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "PlayOrderGestures", 811);
			break;
		case WeaponClass.Boulder:
			break;
		}
	}

	private static void PlayFormationSelectedGesture(Formation formation, Agent agent)
	{
		if (formation.SecondaryLogicalClasses.Any())
		{
			agent.MakeVoice(SkinVoiceManager.VoiceType.MixedFormation, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			return;
		}
		switch (formation.LogicalClass)
		{
		case FormationClass.Infantry:
			agent.MakeVoice(SkinVoiceManager.VoiceType.Infantry, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case FormationClass.Ranged:
			agent.MakeVoice(SkinVoiceManager.VoiceType.Archers, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case FormationClass.Cavalry:
			agent.MakeVoice(SkinVoiceManager.VoiceType.Cavalry, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		case FormationClass.HorseArcher:
			agent.MakeVoice(SkinVoiceManager.VoiceType.HorseArchers, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			break;
		default:
			TaleWorlds.Library.Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "PlayFormationSelectedGesture", 847);
			break;
		}
	}

	private void AfterSetOrder(OrderType orderType)
	{
		MBDebug.Print("After set order called, number of selected formations: " + SelectedFormations.Count);
		foreach (Formation selectedFormation in SelectedFormations)
		{
			MBDebug.Print(string.Concat(selectedFormation?.FormationIndex, " formation being processed."));
			if (_formationUpdateEnabledAfterSetOrder)
			{
				bool flag = false;
				if (selectedFormation.IsPlayerTroopInFormation)
				{
					flag = selectedFormation.GetReadonlyMovementOrderReference().OrderEnum == MovementOrder.MovementOrderEnum.Follow;
				}
				selectedFormation.ApplyActionOnEachUnit(delegate(Agent agent)
				{
					agent.UpdateCachedAndFormationValues(updateOnlyMovement: false, arrangementChangeAllowed: false);
				}, flag ? Mission.Current.MainAgent : null);
			}
			MBDebug.Print("Update cached and formation values on each agent complete, number of selected formations: " + SelectedFormations.Count);
			_mission.SetRandomDecideTimeOfAgentsWithIndices(selectedFormation.CollectUnitIndices());
			MBDebug.Print("Set random decide time of agents with indices complete, number of selected formations: " + SelectedFormations.Count);
		}
		MBDebug.Print("After set order loop complete, number of selected formations: " + SelectedFormations.Count);
		if (Owner != null && AreGesturesEnabled())
		{
			PlayOrderGestures(orderType, Owner, _selectedFormations);
		}
	}

	private void BeforeSetOrder(OrderType orderType)
	{
		foreach (Formation item in SelectedFormations.Where((Formation f) => !IsFormationSelectable(f, Owner)).ToList())
		{
			DeselectFormation(item);
		}
		if (GameNetwork.IsClientOrReplay || orderType == OrderType.AIControlOff || orderType == OrderType.AIControlOn)
		{
			return;
		}
		foreach (Formation selectedFormation in SelectedFormations)
		{
			if (selectedFormation.IsAIControlled && selectedFormation.PlayerOwner != null)
			{
				selectedFormation.SetControlledByAI(isControlledByAI: false);
			}
		}
	}

	public void SetOrderWithAgent(OrderType orderType, Agent agent)
	{
		MBDebug.Print(string.Concat("SetOrderWithAgent ", orderType, " ", agent.Name, "on team"));
		BeforeSetOrder(orderType);
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage(new ApplyOrderWithAgent(orderType, agent.Index));
			GameNetwork.EndModuleEventAsClient();
		}
		switch (orderType)
		{
		case OrderType.FollowMe:
			foreach (Formation selectedFormation in SelectedFormations)
			{
				selectedFormation.SetMovementOrder(MovementOrder.MovementOrderFollow(agent));
			}
			break;
		case OrderType.GuardMe:
			foreach (Formation selectedFormation2 in SelectedFormations)
			{
				selectedFormation2.SetMovementOrder(MovementOrder.MovementOrderGuard(agent));
			}
			break;
		default:
			TaleWorlds.Library.Debug.FailedAssert("[DEBUG]Invalid order type.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "SetOrderWithAgent", 947);
			break;
		}
		AfterSetOrder(orderType);
		this.OnOrderIssued?.Invoke(orderType, SelectedFormations, this, agent);
	}

	public void SetOrderWithPosition(OrderType orderType, WorldPosition orderPosition)
	{
		MBDebug.Print(string.Concat("SetOrderWithPosition ", orderType, " ", orderPosition, "on team"));
		BeforeSetOrder(orderType);
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage(new ApplyOrderWithPosition(orderType, orderPosition.GetGroundVec3()));
			GameNetwork.EndModuleEventAsClient();
		}
		switch (orderType)
		{
		case OrderType.Move:
			foreach (Formation selectedFormation in SelectedFormations)
			{
				selectedFormation.SetMovementOrder(MovementOrder.MovementOrderMove(orderPosition));
			}
			break;
		case OrderType.LookAtDirection:
		{
			FacingOrder facingOrder = FacingOrder.FacingOrderLookAtDirection(GetOrderLookAtDirection(SelectedFormations, orderPosition.AsVec2));
			foreach (Formation selectedFormation2 in SelectedFormations)
			{
				selectedFormation2.FacingOrder = facingOrder;
			}
			break;
		}
		case OrderType.FormCustom:
		{
			float orderFormCustomWidth = GetOrderFormCustomWidth(SelectedFormations, orderPosition.GetGroundVec3());
			foreach (Formation selectedFormation3 in SelectedFormations)
			{
				selectedFormation3.FormOrder = FormOrder.FormOrderCustom(orderFormCustomWidth);
			}
			break;
		}
		default:
			TaleWorlds.Library.Debug.FailedAssert("[DEBUG]Invalid order type.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "SetOrderWithPosition", 997);
			break;
		}
		AfterSetOrder(orderType);
		if (this.OnOrderIssued != null)
		{
			this.OnOrderIssued(orderType, SelectedFormations, this, orderPosition);
		}
	}

	public void SetOrderWithFormation(OrderType orderType, Formation orderFormation)
	{
		MBDebug.Print(string.Concat("SetOrderWithFormation ", orderType, " ", orderFormation, "on team"));
		BeforeSetOrder(orderType);
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage(new ApplyOrderWithFormation(orderType, orderFormation.Index));
			GameNetwork.EndModuleEventAsClient();
		}
		switch (orderType)
		{
		case OrderType.Charge:
			foreach (Formation selectedFormation in SelectedFormations)
			{
				selectedFormation.SetMovementOrder(MovementOrder.MovementOrderCharge);
				selectedFormation.SetTargetFormation(orderFormation);
			}
			break;
		case OrderType.Advance:
			foreach (Formation selectedFormation2 in SelectedFormations)
			{
				selectedFormation2.SetMovementOrder(MovementOrder.MovementOrderAdvance);
				selectedFormation2.SetTargetFormation(orderFormation);
			}
			break;
		default:
			TaleWorlds.Library.Debug.FailedAssert("Invalid order type", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "SetOrderWithFormation", 1040);
			break;
		}
		AfterSetOrder(orderType);
		if (this.OnOrderIssued != null)
		{
			this.OnOrderIssued(orderType, SelectedFormations, this, orderFormation);
		}
	}

	public void SetOrderWithFormationAndPercentage(OrderType orderType, Formation orderFormation, float percentage)
	{
		int value = (int)(percentage * 100f);
		value = MBMath.ClampInt(value, 0, 100);
		BeforeSetOrder(orderType);
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage(new ApplyOrderWithFormationAndPercentage(orderType, orderFormation.Index, value));
			GameNetwork.EndModuleEventAsClient();
		}
		TaleWorlds.Library.Debug.FailedAssert("Invalid order type", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "SetOrderWithFormationAndPercentage", 1081);
		AfterSetOrder(orderType);
		if (this.OnOrderIssued != null)
		{
			this.OnOrderIssued(orderType, SelectedFormations, this, orderFormation, percentage);
		}
	}

	public void TransferUnitWithPriorityFunction(Formation orderFormation, int number, bool hasShield, bool hasSpear, bool hasThrown, bool isHeavy, bool isRanged, bool isMounted, bool excludeBannerman, List<Agent> excludedAgents)
	{
		BeforeSetOrder(OrderType.Transfer);
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage(new ApplyOrderWithFormationAndNumber(OrderType.Transfer, orderFormation.Index, number));
			GameNetwork.EndModuleEventAsClient();
		}
		List<int> list = null;
		int num = SelectedFormations.Sum((Formation f) => f.CountOfUnits);
		int num2 = number;
		int num3 = 0;
		if (SelectedFormations.Count > 1)
		{
			list = new List<int>();
		}
		foreach (Formation selectedFormation in SelectedFormations)
		{
			int countOfUnits = selectedFormation.CountOfUnits;
			int num4 = num2 * countOfUnits / num;
			if (!GameNetwork.IsClientOrReplay)
			{
				selectedFormation.OnMassUnitTransferStart();
				orderFormation.OnMassUnitTransferStart();
				selectedFormation.TransferUnitsWithPriorityFunction(orderFormation, num4, priorityFunction, excludeBannerman, excludedAgents);
				selectedFormation.OnMassUnitTransferEnd();
				orderFormation.OnMassUnitTransferEnd();
			}
			list?.Add(num4);
			num2 -= num4;
			num -= countOfUnits;
			num3 += num4;
		}
		if (!GameNetwork.IsClientOrReplay)
		{
			orderFormation.QuerySystem.Expire();
		}
		AfterSetOrder(OrderType.Transfer);
		if (this.OnOrderIssued == null)
		{
			return;
		}
		if (list != null)
		{
			object[] array = new object[list.Count + 1];
			array[0] = number;
			for (int i = 0; i < list.Count; i++)
			{
				array[i + 1] = list[i];
			}
			this.OnOrderIssued(OrderType.Transfer, SelectedFormations, this, orderFormation, array);
		}
		else
		{
			this.OnOrderIssued(OrderType.Transfer, SelectedFormations, this, orderFormation, number);
		}
		int priorityFunction(Agent agent)
		{
			if (agent != null)
			{
				return ((hasShield && agent.HasShieldCached) ? 1 : 0) + ((hasSpear && agent.HasSpearCached) ? 1 : 0) + ((hasThrown && agent.HasThrownCached) ? 1 : 0) + ((isHeavy && MissionGameModels.Current.AgentStatCalculateModel.HasHeavyArmor(agent)) ? 1 : 0) + ((isRanged == agent.IsRangedCached) ? 10 : 0) + ((isMounted == agent.HasMount) ? 100 : 0);
			}
			return (hasShield ? 1 : 0) + (hasSpear ? 1 : 0) + (hasThrown ? 1 : 0) + (isHeavy ? 1 : 0) + 10 + 100;
		}
	}

	public void RearrangeFormationsAccordingToFilters(Team team, List<Tuple<Formation, int, Team.TroopFilter, List<Agent>>> MassTransferData)
	{
		team.RearrangeFormationsAccordingToFilters(MassTransferData);
	}

	public void SetOrderWithFormationAndNumber(OrderType orderType, Formation orderFormation, int number)
	{
		BeforeSetOrder(orderType);
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage(new ApplyOrderWithFormationAndNumber(orderType, orderFormation.Index, number));
			GameNetwork.EndModuleEventAsClient();
		}
		List<int> list = null;
		if (orderType == OrderType.Transfer)
		{
			int num = SelectedFormations.Sum((Formation f) => f.CountOfUnits);
			int num2 = number;
			int num3 = 0;
			if (SelectedFormations.Count > 1)
			{
				list = new List<int>();
			}
			foreach (Formation selectedFormation in SelectedFormations)
			{
				int countOfUnits = selectedFormation.CountOfUnits;
				int num4 = num2 * countOfUnits / num;
				if (!GameNetwork.IsClientOrReplay)
				{
					selectedFormation.OnMassUnitTransferStart();
					orderFormation.OnMassUnitTransferStart();
					selectedFormation.TransferUnitsAux(orderFormation, num4, isPlayerOrder: true, num4 < countOfUnits && orderFormation.CountOfUnits > 0 && orderFormation.OrderPositionIsValid);
					selectedFormation.OnMassUnitTransferEnd();
					orderFormation.OnMassUnitTransferEnd();
				}
				list?.Add(num4);
				num2 -= num4;
				num -= countOfUnits;
				num3 += num4;
			}
			if (!GameNetwork.IsClientOrReplay)
			{
				orderFormation.QuerySystem.Expire();
			}
		}
		else
		{
			TaleWorlds.Library.Debug.FailedAssert("[DEBUG]Invalid order type.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "SetOrderWithFormationAndNumber", 1330);
		}
		AfterSetOrder(orderType);
		if (this.OnOrderIssued == null)
		{
			return;
		}
		if (list != null)
		{
			object[] array = new object[list.Count + 1];
			array[0] = number;
			for (int i = 0; i < list.Count; i++)
			{
				array[i + 1] = list[i];
			}
			this.OnOrderIssued(orderType, SelectedFormations, this, orderFormation, array);
		}
		else
		{
			this.OnOrderIssued(orderType, SelectedFormations, this, orderFormation, number);
		}
	}

	public void SetOrderWithTwoPositions(OrderType orderType, WorldPosition position1, WorldPosition position2)
	{
		MBDebug.Print(string.Concat("SetOrderWithTwoPositions ", orderType, " ", position1, " ", position2, "on team"));
		BeforeSetOrder(orderType);
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage(new ApplyOrderWithTwoPositions(orderType, position1.GetGroundVec3(), position2.GetGroundVec3()));
			GameNetwork.EndModuleEventAsClient();
		}
		if ((uint)(orderType - 2) <= 1u)
		{
			bool isFormationLayoutVertical = orderType == OrderType.MoveToLineSegment;
			IEnumerable<Formation> enumerable = SelectedFormations.Where((Formation f) => f.CountOfUnitsWithoutDetachedOnes > 0);
			if (enumerable.Any())
			{
				MoveToLineSegment(enumerable, position1, position2, isFormationLayoutVertical);
			}
		}
		else
		{
			TaleWorlds.Library.Debug.FailedAssert("Invalid order type.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "SetOrderWithTwoPositions", 1384);
		}
		AfterSetOrder(orderType);
		if (this.OnOrderIssued != null)
		{
			this.OnOrderIssued(orderType, SelectedFormations, this, position1, position2);
		}
	}

	public void SetOrderWithOrderableObject(IOrderable target)
	{
		BattleSideEnum side = SelectedFormations[0].Team.Side;
		OrderType order = target.GetOrder(side);
		BeforeSetOrder(order);
		MissionObject missionObject = target as MissionObject;
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage(new ApplyOrderWithMissionObject(missionObject.Id));
			GameNetwork.EndModuleEventAsClient();
		}
		switch (order)
		{
		case OrderType.Use:
		{
			UsableMachine usable = target as UsableMachine;
			ToggleSideOrderUse(SelectedFormations, usable);
			break;
		}
		case OrderType.AttackEntity:
		{
			GameEntity gameEntity = missionObject.GameEntity;
			foreach (Formation selectedFormation in SelectedFormations)
			{
				selectedFormation.SetMovementOrder(MovementOrder.MovementOrderAttackEntity(gameEntity, !(missionObject is CastleGate)));
			}
			break;
		}
		case OrderType.PointDefence:
		{
			IPointDefendable pointDefendable = target as IPointDefendable;
			foreach (Formation selectedFormation2 in SelectedFormations)
			{
				selectedFormation2.SetMovementOrder(MovementOrder.MovementOrderMove(pointDefendable.MiddleFrame.Origin));
			}
			break;
		}
		case OrderType.Move:
		{
			WorldPosition position = new WorldPosition(_mission.Scene, UIntPtr.Zero, missionObject.GameEntity.GlobalPosition, hasValidZ: false);
			foreach (Formation selectedFormation3 in SelectedFormations)
			{
				selectedFormation3.SetMovementOrder(MovementOrder.MovementOrderMove(position));
			}
			break;
		}
		case OrderType.MoveToLineSegment:
		{
			IPointDefendable obj2 = target as IPointDefendable;
			Vec3 globalPosition3 = obj2.DefencePoints.Last().GameEntity.GlobalPosition;
			Vec3 globalPosition4 = obj2.DefencePoints.First().GameEntity.GlobalPosition;
			IEnumerable<Formation> enumerable2 = SelectedFormations.Where((Formation f) => f.CountOfUnitsWithoutDetachedOnes > 0);
			if (enumerable2.Any())
			{
				WorldPosition targetLineSegmentBegin2 = new WorldPosition(_mission.Scene, UIntPtr.Zero, globalPosition3, hasValidZ: false);
				WorldPosition targetLineSegmentEnd2 = new WorldPosition(_mission.Scene, UIntPtr.Zero, globalPosition4, hasValidZ: false);
				MoveToLineSegment(enumerable2, targetLineSegmentBegin2, targetLineSegmentEnd2);
			}
			break;
		}
		case OrderType.MoveToLineSegmentWithHorizontalLayout:
		{
			IPointDefendable obj = target as IPointDefendable;
			Vec3 globalPosition = obj.DefencePoints.Last().GameEntity.GlobalPosition;
			Vec3 globalPosition2 = obj.DefencePoints.First().GameEntity.GlobalPosition;
			IEnumerable<Formation> enumerable = SelectedFormations.Where((Formation f) => f.CountOfUnitsWithoutDetachedOnes > 0);
			if (enumerable.Any())
			{
				WorldPosition targetLineSegmentBegin = new WorldPosition(_mission.Scene, UIntPtr.Zero, globalPosition, hasValidZ: false);
				WorldPosition targetLineSegmentEnd = new WorldPosition(_mission.Scene, UIntPtr.Zero, globalPosition2, hasValidZ: false);
				MoveToLineSegment(enumerable, targetLineSegmentBegin, targetLineSegmentEnd, isFormationLayoutVertical: false);
			}
			break;
		}
		case OrderType.FollowEntity:
		{
			GameEntity waitEntity = (target as UsableMachine).WaitEntity;
			Vec2 direction = waitEntity.GetGlobalFrame().rotation.f.AsVec2.Normalized();
			foreach (Formation selectedFormation4 in SelectedFormations)
			{
				selectedFormation4.FacingOrder = FacingOrder.FacingOrderLookAtDirection(direction);
				selectedFormation4.SetMovementOrder(MovementOrder.MovementOrderFollowEntity(waitEntity));
			}
			break;
		}
		}
		AfterSetOrder(order);
		this.OnOrderIssued?.Invoke(order, SelectedFormations, this, target);
	}

	public static OrderType GetActiveMovementOrderOf(Formation formation)
	{
		switch (formation.GetReadonlyMovementOrderReference().MovementState)
		{
		case MovementOrder.MovementStateEnum.Charge:
			if (formation.GetReadonlyMovementOrderReference().OrderType == OrderType.GuardMe)
			{
				return OrderType.GuardMe;
			}
			return OrderType.Charge;
		case MovementOrder.MovementStateEnum.Hold:
			return formation.GetReadonlyMovementOrderReference().OrderType switch
			{
				OrderType.ChargeWithTarget => OrderType.Charge, 
				OrderType.FollowMe => OrderType.FollowMe, 
				OrderType.Advance => OrderType.Advance, 
				OrderType.FallBack => OrderType.FallBack, 
				_ => OrderType.Move, 
			};
		case MovementOrder.MovementStateEnum.Retreat:
			return OrderType.Retreat;
		case MovementOrder.MovementStateEnum.StandGround:
			return OrderType.StandYourGround;
		default:
			TaleWorlds.Library.Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "GetActiveMovementOrderOf", 1543);
			return OrderType.Move;
		}
	}

	public static OrderType GetActiveFacingOrderOf(Formation formation)
	{
		if (formation.FacingOrder.OrderType == OrderType.LookAtDirection)
		{
			return OrderType.LookAtDirection;
		}
		return OrderType.LookAtEnemy;
	}

	public static OrderType GetActiveRidingOrderOf(Formation formation)
	{
		OrderType orderType = formation.RidingOrder.OrderType;
		if (orderType == OrderType.RideFree)
		{
			return OrderType.Mount;
		}
		return orderType;
	}

	public static OrderType GetActiveArrangementOrderOf(Formation formation)
	{
		return formation.ArrangementOrder.OrderType;
	}

	public static OrderType GetActiveFormOrderOf(Formation formation)
	{
		return formation.FormOrder.OrderType;
	}

	public static OrderType GetActiveFiringOrderOf(Formation formation)
	{
		return formation.FiringOrder.OrderType;
	}

	public static OrderType GetActiveAIControlOrderOf(Formation formation)
	{
		if (formation.IsAIControlled)
		{
			return OrderType.AIControlOn;
		}
		return OrderType.AIControlOff;
	}

	public void SimulateNewOrderWithPositionAndDirection(WorldPosition formationLineBegin, WorldPosition formationLineEnd, out List<WorldPosition> simulationAgentFrames, bool isFormationLayoutVertical)
	{
		IEnumerable<Formation> enumerable = SelectedFormations.Where((Formation f) => f.CountOfUnitsWithoutDetachedOnes > 0);
		if (enumerable.Any())
		{
			SimulateNewOrderWithPositionAndDirection(enumerable, simulationFormations, formationLineBegin, formationLineEnd, out simulationAgentFrames, isFormationLayoutVertical);
		}
		else
		{
			simulationAgentFrames = new List<WorldPosition>();
		}
	}

	public void SimulateNewFacingOrder(Vec2 direction, out List<WorldPosition> simulationAgentFrames)
	{
		IEnumerable<Formation> enumerable = SelectedFormations.Where((Formation f) => f.CountOfUnitsWithoutDetachedOnes > 0);
		if (enumerable.Any())
		{
			SimulateNewFacingOrder(enumerable, simulationFormations, direction, out simulationAgentFrames);
		}
		else
		{
			simulationAgentFrames = new List<WorldPosition>();
		}
	}

	public void SimulateNewCustomWidthOrder(float width, out List<WorldPosition> simulationAgentFrames)
	{
		IEnumerable<Formation> enumerable = SelectedFormations.Where((Formation f) => f.CountOfUnitsWithoutDetachedOnes > 0);
		if (enumerable.Any())
		{
			SimulateNewCustomWidthOrder(enumerable, simulationFormations, width, out simulationAgentFrames);
		}
		else
		{
			simulationAgentFrames = new List<WorldPosition>();
		}
	}

	private static void SimulateNewOrderWithPositionAndDirectionAux(IEnumerable<Formation> formations, Dictionary<Formation, Formation> simulationFormations, WorldPosition formationLineBegin, WorldPosition formationLineEnd, bool isSimulatingAgentFrames, out List<WorldPosition> simulationAgentFrames, bool isSimulatingFormationChanges, out List<(Formation, int, float, WorldPosition, Vec2)> simulationFormationChanges, out bool isLineShort, bool isFormationLayoutVertical = true)
	{
		float length = (formationLineEnd.AsVec2 - formationLineBegin.AsVec2).Length;
		isLineShort = false;
		if (length < ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalRadius))
		{
			isLineShort = true;
		}
		else
		{
			float num = ((!isFormationLayoutVertical) ? formations.Max((Formation f) => f.Width) : (formations.Sum((Formation f) => f.MinimumWidth) + (float)(formations.Count() - 1) * 1.5f));
			if (length < num)
			{
				isLineShort = true;
			}
		}
		if (isLineShort)
		{
			float num2;
			if (isFormationLayoutVertical)
			{
				num2 = formations.Sum((Formation f) => f.Width);
				num2 += (float)(formations.Count() - 1) * 1.5f;
			}
			else
			{
				num2 = formations.Max((Formation f) => f.Width);
			}
			Vec2 direction = formations.MaxBy((Formation f) => f.CountOfUnitsWithoutDetachedOnes).Direction;
			direction.RotateCCW(-System.MathF.PI / 2f);
			direction.Normalize();
			formationLineEnd = Mission.Current.GetStraightPathToTarget(formationLineBegin.AsVec2 + num2 / 2f * direction, formationLineBegin);
			formationLineBegin = Mission.Current.GetStraightPathToTarget(formationLineBegin.AsVec2 - num2 / 2f * direction, formationLineBegin);
		}
		else
		{
			formationLineEnd = Mission.Current.GetStraightPathToTarget(formationLineEnd.AsVec2, formationLineBegin);
		}
		if (isFormationLayoutVertical)
		{
			SimulateNewOrderWithVerticalLayout(formations, simulationFormations, formationLineBegin, formationLineEnd, isSimulatingAgentFrames, out simulationAgentFrames, isSimulatingFormationChanges, out simulationFormationChanges);
		}
		else
		{
			SimulateNewOrderWithHorizontalLayout(formations, simulationFormations, formationLineBegin, formationLineEnd, isSimulatingAgentFrames, out simulationAgentFrames, isSimulatingFormationChanges, out simulationFormationChanges);
		}
	}

	private static Formation GetSimulationFormation(Formation formation, Dictionary<Formation, Formation> simulationFormations)
	{
		return simulationFormations?[formation];
	}

	private static void SimulateNewFacingOrder(IEnumerable<Formation> formations, Dictionary<Formation, Formation> simulationFormations, Vec2 direction, out List<WorldPosition> simulationAgentFrames)
	{
		simulationAgentFrames = new List<WorldPosition>();
		foreach (Formation formation in formations)
		{
			float formationWidth = formation.Width;
			WorldPosition formationPosition = formation.CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.None);
			int unitSpacingReduction = 0;
			DecreaseUnitSpacingAndWidthIfNotAllUnitsFit(formation, GetSimulationFormation(formation, simulationFormations), in formationPosition, in direction, ref formationWidth, ref unitSpacingReduction);
			SimulateNewOrderWithFrameAndWidth(formation, GetSimulationFormation(formation, simulationFormations), simulationAgentFrames, null, in formationPosition, in direction, formationWidth, unitSpacingReduction, simulateFormationDepth: false, out var _);
		}
	}

	private static void SimulateNewCustomWidthOrder(IEnumerable<Formation> formations, Dictionary<Formation, Formation> simulationFormations, float width, out List<WorldPosition> simulationAgentFrames)
	{
		simulationAgentFrames = new List<WorldPosition>();
		foreach (Formation formation in formations)
		{
			float a = width;
			a = TaleWorlds.Library.MathF.Min(a, formation.MaximumWidth);
			Mat3 identity = Mat3.Identity;
			identity.f = formation.Direction.ToVec3();
			identity.Orthonormalize();
			WorldPosition formationPosition = formation.CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.None);
			int unitSpacingReduction = 0;
			Formation simulationFormation = GetSimulationFormation(formation, simulationFormations);
			Vec2 formationDirection = formation.Direction;
			DecreaseUnitSpacingAndWidthIfNotAllUnitsFit(formation, simulationFormation, in formationPosition, in formationDirection, ref a, ref unitSpacingReduction);
			int count = simulationAgentFrames.Count;
			Formation simulationFormation2 = GetSimulationFormation(formation, simulationFormations);
			List<WorldPosition> simulationAgentFrames2 = simulationAgentFrames;
			formationDirection = formation.Direction;
			SimulateNewOrderWithFrameAndWidth(formation, simulationFormation2, simulationAgentFrames2, null, in formationPosition, in formationDirection, a, unitSpacingReduction, simulateFormationDepth: false, out var simulatedFormationDepth);
			float lastSimulatedFormationsOccupationWidthIfLesserThanActualWidth = Formation.GetLastSimulatedFormationsOccupationWidthIfLesserThanActualWidth(GetSimulationFormation(formation, simulationFormations));
			if (lastSimulatedFormationsOccupationWidthIfLesserThanActualWidth > 0f)
			{
				simulationAgentFrames.RemoveRange(count, simulationAgentFrames.Count - count);
				Formation simulationFormation3 = GetSimulationFormation(formation, simulationFormations);
				List<WorldPosition> simulationAgentFrames3 = simulationAgentFrames;
				formationDirection = formation.Direction;
				SimulateNewOrderWithFrameAndWidth(formation, simulationFormation3, simulationAgentFrames3, null, in formationPosition, in formationDirection, lastSimulatedFormationsOccupationWidthIfLesserThanActualWidth, unitSpacingReduction, simulateFormationDepth: false, out simulatedFormationDepth);
			}
		}
	}

	public static void SimulateNewOrderWithPositionAndDirection(IEnumerable<Formation> formations, Dictionary<Formation, Formation> simulationFormations, WorldPosition formationLineBegin, WorldPosition formationLineEnd, out List<WorldPosition> simulationAgentFrames, bool isFormationLayoutVertical = true)
	{
		SimulateNewOrderWithPositionAndDirectionAux(formations, simulationFormations, formationLineBegin, formationLineEnd, isSimulatingAgentFrames: true, out simulationAgentFrames, isSimulatingFormationChanges: false, out var _, out var _, isFormationLayoutVertical);
	}

	public static void SimulateNewOrderWithPositionAndDirection(IEnumerable<Formation> formations, Dictionary<Formation, Formation> simulationFormations, WorldPosition formationLineBegin, WorldPosition formationLineEnd, out List<(Formation, int, float, WorldPosition, Vec2)> formationChanges, out bool isLineShort, bool isFormationLayoutVertical = true)
	{
		SimulateNewOrderWithPositionAndDirectionAux(formations, simulationFormations, formationLineBegin, formationLineEnd, isSimulatingAgentFrames: false, out var _, isSimulatingFormationChanges: true, out formationChanges, out isLineShort, isFormationLayoutVertical);
	}

	private static void SimulateNewOrderWithVerticalLayout(IEnumerable<Formation> formations, Dictionary<Formation, Formation> simulationFormations, WorldPosition formationLineBegin, WorldPosition formationLineEnd, bool isSimulatingAgentFrames, out List<WorldPosition> simulationAgentFrames, bool isSimulatingFormationChanges, out List<(Formation, int, float, WorldPosition, Vec2)> simulationFormationChanges)
	{
		simulationAgentFrames = ((!isSimulatingAgentFrames) ? null : new List<WorldPosition>());
		simulationFormationChanges = ((!isSimulatingFormationChanges) ? null : new List<(Formation, int, float, WorldPosition, Vec2)>());
		Vec2 vec = formationLineEnd.AsVec2 - formationLineBegin.AsVec2;
		float length = vec.Length;
		vec.Normalize();
		float num = TaleWorlds.Library.MathF.Max(0f, length - (float)(formations.Count() - 1) * 1.5f);
		float num2 = formations.Sum((Formation f) => f.Width);
		bool flag = num.ApproximatelyEqualsTo(num2, 0.1f);
		float num3 = formations.Sum((Formation f) => f.MinimumWidth);
		formations.Count();
		Vec2 formationDirection = new Vec2(0f - vec.y, vec.x).Normalized();
		float num4 = 0f;
		foreach (Formation formation in formations)
		{
			float minimumWidth = formation.MinimumWidth;
			float a = (flag ? formation.Width : TaleWorlds.Library.MathF.Min((num < num2) ? formation.Width : float.MaxValue, num * (minimumWidth / num3)));
			a = TaleWorlds.Library.MathF.Min(a, formation.MaximumWidth);
			WorldPosition formationPosition = formationLineBegin;
			formationPosition.SetVec2(formationPosition.AsVec2 + vec * (a * 0.5f + num4));
			int unitSpacingReduction = 0;
			DecreaseUnitSpacingAndWidthIfNotAllUnitsFit(formation, GetSimulationFormation(formation, simulationFormations), in formationPosition, in formationDirection, ref a, ref unitSpacingReduction);
			SimulateNewOrderWithFrameAndWidth(formation, GetSimulationFormation(formation, simulationFormations), simulationAgentFrames, simulationFormationChanges, in formationPosition, in formationDirection, a, unitSpacingReduction, simulateFormationDepth: false, out var _);
			num4 += a + 1.5f;
		}
	}

	private static void DecreaseUnitSpacingAndWidthIfNotAllUnitsFit(Formation formation, Formation simulationFormation, in WorldPosition formationPosition, in Vec2 formationDirection, ref float formationWidth, ref int unitSpacingReduction)
	{
		if (simulationFormation.UnitSpacing != formation.UnitSpacing)
		{
			simulationFormation = new Formation(null, -1);
		}
		int unitIndex = formation.CountOfUnitsWithoutDetachedOnes - 1;
		float actualWidth = formationWidth;
		do
		{
			formation.GetUnitPositionWithIndexAccordingToNewOrder(simulationFormation, unitIndex, in formationPosition, in formationDirection, formationWidth, formation.UnitSpacing - unitSpacingReduction, out var unitSpawnPosition, out var _, out actualWidth);
			if (unitSpawnPosition.HasValue)
			{
				break;
			}
			unitSpacingReduction++;
		}
		while (formation.UnitSpacing - unitSpacingReduction >= 0);
		unitSpacingReduction = TaleWorlds.Library.MathF.Min(unitSpacingReduction, formation.UnitSpacing);
		if (unitSpacingReduction > 0)
		{
			formationWidth = actualWidth;
		}
	}

	private static float GetGapBetweenLinesOfFormation(Formation f, float unitSpacing)
	{
		float num = 0f;
		float num2 = 0.2f;
		if (f.HasAnyMountedUnit && !(f.RidingOrder == RidingOrder.RidingOrderDismount))
		{
			num = 2f;
			num2 = 0.6f;
		}
		return num + unitSpacing * num2;
	}

	private static void SimulateNewOrderWithHorizontalLayout(IEnumerable<Formation> formations, Dictionary<Formation, Formation> simulationFormations, WorldPosition formationLineBegin, WorldPosition formationLineEnd, bool isSimulatingAgentFrames, out List<WorldPosition> simulationAgentFrames, bool isSimulatingFormationChanges, out List<(Formation, int, float, WorldPosition, Vec2)> simulationFormationChanges)
	{
		simulationAgentFrames = ((!isSimulatingAgentFrames) ? null : new List<WorldPosition>());
		simulationFormationChanges = ((!isSimulatingFormationChanges) ? null : new List<(Formation, int, float, WorldPosition, Vec2)>());
		Vec2 vec = formationLineEnd.AsVec2 - formationLineBegin.AsVec2;
		float num = vec.Normalize();
		float num2 = formations.Max((Formation f) => f.MinimumWidth);
		if (num < num2)
		{
			num = num2;
		}
		Vec2 formationDirection = new Vec2(0f - vec.y, vec.x).Normalized();
		float num3 = 0f;
		foreach (Formation formation in formations)
		{
			float a = num;
			a = TaleWorlds.Library.MathF.Min(a, formation.MaximumWidth);
			WorldPosition formationPosition = formationLineBegin;
			formationPosition.SetVec2((formationLineEnd.AsVec2 + formationLineBegin.AsVec2) * 0.5f - formationDirection * num3);
			int unitSpacingReduction = 0;
			DecreaseUnitSpacingAndWidthIfNotAllUnitsFit(formation, GetSimulationFormation(formation, simulationFormations), in formationPosition, in formationDirection, ref a, ref unitSpacingReduction);
			SimulateNewOrderWithFrameAndWidth(formation, GetSimulationFormation(formation, simulationFormations), simulationAgentFrames, simulationFormationChanges, in formationPosition, in formationDirection, a, unitSpacingReduction, simulateFormationDepth: true, out var simulatedFormationDepth);
			num3 += simulatedFormationDepth + GetGapBetweenLinesOfFormation(formation, formation.UnitSpacing - unitSpacingReduction);
		}
	}

	private static void SimulateNewOrderWithFrameAndWidth(Formation formation, Formation simulationFormation, List<WorldPosition> simulationAgentFrames, List<(Formation, int, float, WorldPosition, Vec2)> simulationFormationChanges, in WorldPosition formationPosition, in Vec2 formationDirection, float formationWidth, int unitSpacingReduction, bool simulateFormationDepth, out float simulatedFormationDepth)
	{
		int num = 0;
		float num2 = (simulateFormationDepth ? 0f : float.NaN);
		bool flag = Mission.Current.Mode != MissionMode.Deployment || Mission.Current.IsOrderPositionAvailable(in formationPosition, formation.Team);
		foreach (Agent item2 in from u in formation.GetUnitsWithoutDetachedOnes()
			orderby MBCommon.Hash(u.Index, u)
			select u)
		{
			WorldPosition? unitSpawnPosition = null;
			Vec2? unitSpawnDirection = null;
			if (flag)
			{
				formation.GetUnitPositionWithIndexAccordingToNewOrder(simulationFormation, num, in formationPosition, in formationDirection, formationWidth, formation.UnitSpacing - unitSpacingReduction, out unitSpawnPosition, out unitSpawnDirection);
			}
			else
			{
				unitSpawnPosition = item2.GetWorldPosition();
				unitSpawnDirection = item2.GetMovementDirection();
			}
			if (unitSpawnPosition.HasValue)
			{
				simulationAgentFrames?.Add(unitSpawnPosition.Value);
				if (simulateFormationDepth)
				{
					float num3 = Vec2.DistanceToLine(formationPosition.AsVec2, formationPosition.AsVec2 + formationDirection.RightVec(), unitSpawnPosition.Value.AsVec2);
					if (num3 > num2)
					{
						num2 = num3;
					}
				}
			}
			num++;
		}
		if (flag)
		{
			simulationFormationChanges?.Add(ValueTuple.Create(formation, unitSpacingReduction, formationWidth, formationPosition, formationDirection));
		}
		else
		{
			WorldPosition item = formation.CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.None);
			simulationFormationChanges?.Add(ValueTuple.Create(formation, unitSpacingReduction, formationWidth, item, formation.Direction));
		}
		simulatedFormationDepth = num2 + formation.UnitDiameter;
	}

	public void SimulateDestinationFrames(out List<WorldPosition> simulationAgentFrames, float minDistance = 3f)
	{
		MBReadOnlyList<Formation> selectedFormations = SelectedFormations;
		simulationAgentFrames = new List<WorldPosition>(100);
		float minDistanceSq = minDistance * minDistance;
		foreach (Formation item in selectedFormations)
		{
			item.ApplyActionOnEachUnit(delegate(Agent agent, List<WorldPosition> localSimulationAgentFrames)
			{
				WorldPosition position = ((!_mission.IsTeleportingAgents || agent.CanTeleport()) ? agent.Formation.GetOrderPositionOfUnit(agent) : agent.GetWorldPosition());
				bool flag = position.IsValid;
				if (!GameNetwork.IsMultiplayer && _mission.Mode == MissionMode.Deployment)
				{
					MBSceneUtilities.ProjectPositionToDeploymentBoundaries(agent.Formation.Team.Side, ref position);
					flag = _mission.IsFormationUnitPositionAvailable(ref position, agent.Formation.Team);
				}
				if (flag && agent.Position.AsVec2.DistanceSquared(position.AsVec2) >= minDistanceSq)
				{
					localSimulationAgentFrames.Add(position);
				}
			}, simulationAgentFrames);
		}
	}

	private void ToggleSideOrderUse(IEnumerable<Formation> formations, UsableMachine usable)
	{
		IEnumerable<Formation> enumerable = formations.Where(usable.IsUsedByFormation);
		if (enumerable.IsEmpty())
		{
			foreach (Formation formation in formations)
			{
				formation.StartUsingMachine(usable, isPlayerOrder: true);
			}
			if (!usable.HasWaitFrame)
			{
				return;
			}
			{
				foreach (Formation formation2 in formations)
				{
					formation2.SetMovementOrder(MovementOrder.MovementOrderFollowEntity(usable.WaitEntity));
				}
				return;
			}
		}
		foreach (Formation item in enumerable)
		{
			item.StopUsingMachine(usable, isPlayerOrder: true);
		}
	}

	private static int GetLineOrderByClass(FormationClass formationClass)
	{
		return Array.IndexOf(new FormationClass[8]
		{
			FormationClass.HeavyInfantry,
			FormationClass.Infantry,
			FormationClass.HeavyCavalry,
			FormationClass.Cavalry,
			FormationClass.LightCavalry,
			FormationClass.NumberOfDefaultFormations,
			FormationClass.Ranged,
			FormationClass.HorseArcher
		}, formationClass);
	}

	public static IEnumerable<Formation> SortFormationsForHorizontalLayout(IEnumerable<Formation> formations)
	{
		return formations.OrderBy((Formation f) => GetLineOrderByClass(f.FormationIndex));
	}

	private static IEnumerable<Formation> GetSortedFormations(IEnumerable<Formation> formations, bool isFormationLayoutVertical)
	{
		if (isFormationLayoutVertical)
		{
			return formations;
		}
		return SortFormationsForHorizontalLayout(formations);
	}

	private void MoveToLineSegment(IEnumerable<Formation> formations, WorldPosition TargetLineSegmentBegin, WorldPosition TargetLineSegmentEnd, bool isFormationLayoutVertical = true)
	{
		foreach (Formation formation2 in formations)
		{
			if (actualUnitSpacings.TryGetValue(formation2, out var value))
			{
				formation2.SetPositioning(null, null, value);
			}
			if (actualWidths.TryGetValue(formation2, out var value2))
			{
				formation2.FormOrder = FormOrder.FormOrderCustom(value2);
			}
		}
		formations = GetSortedFormations(formations, isFormationLayoutVertical);
		SimulateNewOrderWithPositionAndDirection(formations, simulationFormations, TargetLineSegmentBegin, TargetLineSegmentEnd, out var formationChanges, out var isLineShort, isFormationLayoutVertical);
		if (!formations.Any())
		{
			return;
		}
		foreach (var item6 in formationChanges)
		{
			Formation item = item6.Item1;
			int item2 = item6.Item2;
			float item3 = item6.Item3;
			WorldPosition item4 = item6.Item4;
			Vec2 item5 = item6.Item5;
			int unitSpacing = item.UnitSpacing;
			float width = item.Width;
			if (item2 > 0)
			{
				int value3 = TaleWorlds.Library.MathF.Max(item.UnitSpacing - item2, 0);
				item.SetPositioning(null, null, value3);
				if (item.UnitSpacing != unitSpacing)
				{
					actualUnitSpacings[item] = unitSpacing;
				}
			}
			if (item.Width != item3 && item.ArrangementOrder.OrderEnum != ArrangementOrder.ArrangementOrderEnum.Column)
			{
				item.FormOrder = FormOrder.FormOrderCustom(item3);
				if (isLineShort)
				{
					actualWidths[item] = width;
				}
			}
			if (!isLineShort)
			{
				item.SetMovementOrder(MovementOrder.MovementOrderMove(item4));
				item.FacingOrder = FacingOrder.FacingOrderLookAtDirection(item5);
				item.FormOrder = FormOrder.FormOrderCustom(item3);
				if (this.OnOrderIssued != null)
				{
					MBList<Formation> appliedFormations = new MBList<Formation> { item };
					this.OnOrderIssued(OrderType.Move, appliedFormations, this, item4);
					this.OnOrderIssued(OrderType.LookAtDirection, appliedFormations, this, item5);
					this.OnOrderIssued(OrderType.FormCustom, appliedFormations, this, item3);
				}
				continue;
			}
			Formation formation = formations.MaxBy((Formation f) => f.CountOfUnitsWithoutDetachedOnes);
			switch (GetActiveFacingOrderOf(formation))
			{
			case OrderType.LookAtEnemy:
				item.SetMovementOrder(MovementOrder.MovementOrderMove(item4));
				if (this.OnOrderIssued != null)
				{
					MBList<Formation> appliedFormations3 = new MBList<Formation> { item };
					this.OnOrderIssued(OrderType.Move, appliedFormations3, this, item4);
					this.OnOrderIssued(OrderType.LookAtEnemy, appliedFormations3, this);
				}
				break;
			case OrderType.LookAtDirection:
				item.SetMovementOrder(MovementOrder.MovementOrderMove(item4));
				item.FacingOrder = FacingOrder.FacingOrderLookAtDirection(formation.Direction);
				if (this.OnOrderIssued != null)
				{
					MBList<Formation> appliedFormations2 = new MBList<Formation> { item };
					this.OnOrderIssued(OrderType.Move, appliedFormations2, this, item4);
					this.OnOrderIssued(OrderType.LookAtDirection, appliedFormations2, this, formation.Direction);
				}
				break;
			default:
				TaleWorlds.Library.Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "MoveToLineSegment", 2361);
				break;
			}
		}
	}

	public static Vec2 GetOrderLookAtDirection(IEnumerable<Formation> formations, Vec2 target)
	{
		if (!formations.Any())
		{
			TaleWorlds.Library.Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "GetOrderLookAtDirection", 2381);
			return Vec2.One;
		}
		Formation formation = formations.MaxBy((Formation f) => f.CountOfUnitsWithoutDetachedOnes);
		return (target - formation.OrderPosition).Normalized();
	}

	public static float GetOrderFormCustomWidth(IEnumerable<Formation> formations, Vec3 orderPosition)
	{
		return (Agent.Main.Position - orderPosition).Length;
	}

	public void TransferUnits(Formation source, Formation target, int count)
	{
		source.TransferUnitsAux(target, count, isPlayerOrder: false, count < source.CountOfUnits && target.CountOfUnits > 0);
		this.OnOrderIssued?.Invoke(OrderType.Transfer, new MBList<Formation> { source }, this, target, count);
	}

	public IEnumerable<Formation> SplitFormation(Formation formation, int count = 2)
	{
		if (!formation.IsSplittableByAI || formation.CountOfUnitsWithoutDetachedOnes < count)
		{
			return new List<Formation> { formation };
		}
		MBDebug.Print(((formation.Team.Side == BattleSideEnum.Attacker) ? "Attacker team" : "Defender team") + " formation " + (int)formation.FormationIndex + " split");
		List<Formation> list = new List<Formation> { formation };
		while (count > 1)
		{
			int num = formation.CountOfUnits / count;
			for (int i = 0; i < 8; i++)
			{
				Formation formation2 = formation.Team.GetFormation((FormationClass)i);
				if (formation2.CountOfUnits == 0)
				{
					formation.TransferUnitsAux(formation2, num, isPlayerOrder: false, useSelectivePop: false);
					list.Add(formation2);
					this.OnOrderIssued?.Invoke(OrderType.Transfer, new MBList<Formation> { formation }, this, formation2, num);
					break;
				}
			}
			count--;
		}
		return list;
	}

	[Conditional("DEBUG")]
	public void TickDebug()
	{
	}

	public void AddOrderOverride(Func<Formation, MovementOrder, MovementOrder> orderOverride)
	{
		if (orderOverrides == null)
		{
			orderOverrides = new List<Func<Formation, MovementOrder, MovementOrder>>();
			overridenOrders = new List<(Formation, OrderType)>();
		}
		orderOverrides.Add(orderOverride);
	}

	public OrderType GetOverridenOrderType(Formation formation)
	{
		if (overridenOrders == null)
		{
			return OrderType.None;
		}
		(Formation, OrderType) tuple = overridenOrders.FirstOrDefault(((Formation, OrderType) oo) => oo.Item1 == formation);
		if (tuple.Item1 != null)
		{
			return tuple.Item2;
		}
		return OrderType.None;
	}

	private void CreateDefaultOrderOverrides()
	{
		AddOrderOverride(delegate(Formation formation, MovementOrder order)
		{
			if (formation.ArrangementOrder.OrderType == OrderType.ArrangementCloseOrder && order.OrderType == OrderType.StandYourGround)
			{
				Vec2 averagePosition = formation.QuerySystem.AveragePosition;
				float movementSpeed = formation.QuerySystem.MovementSpeed;
				WorldPosition medianPosition = formation.QuerySystem.MedianPosition;
				medianPosition.SetVec2(averagePosition + formation.Direction * formation.Depth * (0.5f + movementSpeed));
				return MovementOrder.MovementOrderMove(medianPosition);
			}
			return MovementOrder.MovementOrderStop;
		});
	}

	internal void SetFormationUpdateEnabledAfterSetOrder(bool value)
	{
		_formationUpdateEnabledAfterSetOrder = value;
	}

	private static void TryCancelStopOrder(Formation formation)
	{
		if (!GameNetwork.IsClientOrReplay && formation.GetReadonlyMovementOrderReference().OrderEnum == MovementOrder.MovementOrderEnum.Stop)
		{
			WorldPosition position = formation.CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.None);
			if (position.IsValid)
			{
				formation.SetMovementOrder(MovementOrder.MovementOrderMove(position));
			}
		}
	}
}
