using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class DeploymentHandler : MissionLogic
{
	protected MissionMode previousMissionMode;

	protected readonly bool isPlayerAttacker;

	private bool areDeploymentPointsInitialized;

	public Team team => base.Mission.PlayerTeam;

	public bool IsPlayerAttacker => isPlayerAttacker;

	public DeploymentHandler(bool isPlayerAttacker)
	{
		this.isPlayerAttacker = isPlayerAttacker;
	}

	public override void EarlyStart()
	{
	}

	public override void AfterStart()
	{
		base.AfterStart();
		previousMissionMode = base.Mission.Mode;
		base.Mission.SetMissionMode(MissionMode.Deployment, atStart: true);
		team.OnOrderIssued += OrderController_OnOrderIssued;
	}

	private void OrderController_OnOrderIssued(OrderType orderType, MBReadOnlyList<Formation> appliedFormations, OrderController orderController, params object[] delegateParams)
	{
		OrderController_OnOrderIssued_Aux(orderType, appliedFormations, orderController, delegateParams);
	}

	internal static void OrderController_OnOrderIssued_Aux(OrderType orderType, MBReadOnlyList<Formation> appliedFormations, OrderController orderController = null, params object[] delegateParams)
	{
		bool flag = false;
		foreach (Formation appliedFormation in appliedFormations)
		{
			if (appliedFormation.CountOfUnits > 0)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			switch (orderType)
			{
			case OrderType.None:
				Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\MissionLogics\\DeploymentHandler.cs", "OrderController_OnOrderIssued_Aux", 107);
				break;
			case OrderType.Move:
			case OrderType.MoveToLineSegment:
			case OrderType.MoveToLineSegmentWithHorizontalLayout:
			case OrderType.FollowMe:
			case OrderType.FollowEntity:
			case OrderType.Advance:
			case OrderType.FallBack:
				ForcePositioning();
				ForceUpdateFormationParams();
				break;
			case OrderType.StandYourGround:
				ForcePositioning();
				ForceUpdateFormationParams();
				break;
			case OrderType.Charge:
			case OrderType.ChargeWithTarget:
			case OrderType.GuardMe:
				ForcePositioning();
				ForceUpdateFormationParams();
				break;
			case OrderType.Retreat:
				ForcePositioning();
				ForceUpdateFormationParams();
				break;
			case OrderType.LookAtEnemy:
			case OrderType.LookAtDirection:
				ForcePositioning();
				ForceUpdateFormationParams();
				break;
			case OrderType.ArrangementLine:
			case OrderType.ArrangementCloseOrder:
			case OrderType.ArrangementLoose:
			case OrderType.ArrangementCircular:
			case OrderType.ArrangementSchiltron:
			case OrderType.ArrangementVee:
			case OrderType.ArrangementColumn:
			case OrderType.ArrangementScatter:
				ForceUpdateFormationParams();
				break;
			case OrderType.FormCustom:
			case OrderType.FormDeep:
			case OrderType.FormWide:
			case OrderType.FormWider:
				ForceUpdateFormationParams();
				break;
			case OrderType.Mount:
			case OrderType.Dismount:
				ForceUpdateFormationParams();
				break;
			case OrderType.AIControlOn:
			case OrderType.AIControlOff:
				ForcePositioning();
				ForceUpdateFormationParams();
				break;
			case OrderType.Transfer:
			case OrderType.Use:
			case OrderType.AttackEntity:
				ForceUpdateFormationParams();
				break;
			case OrderType.PointDefence:
				Debug.FailedAssert("will be removed", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\MissionLogics\\DeploymentHandler.cs", "OrderController_OnOrderIssued_Aux", 180);
				break;
			default:
				Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\MissionLogics\\DeploymentHandler.cs", "OrderController_OnOrderIssued_Aux", 183);
				break;
			case OrderType.CohesionHigh:
			case OrderType.CohesionMedium:
			case OrderType.CohesionLow:
			case OrderType.HoldFire:
			case OrderType.FireAtWill:
				break;
			}
		}
		void ForcePositioning()
		{
			foreach (Formation appliedFormation2 in appliedFormations)
			{
				if (appliedFormation2.CountOfUnits > 0)
				{
					Vec2 direction = appliedFormation2.FacingOrder.GetDirection(appliedFormation2);
					appliedFormation2.SetPositioning(appliedFormation2.GetReadonlyMovementOrderReference().CreateNewOrderWorldPosition(appliedFormation2, WorldPosition.WorldPositionEnforcedCache.None), direction);
				}
			}
		}
		void ForceUpdateFormationParams()
		{
			foreach (Formation appliedFormation3 in appliedFormations)
			{
				if (appliedFormation3.CountOfUnits > 0 && (orderController == null || orderController.FormationUpdateEnabledAfterSetOrder))
				{
					bool flag2 = false;
					if (appliedFormation3.IsPlayerTroopInFormation)
					{
						flag2 = appliedFormation3.GetReadonlyMovementOrderReference().OrderEnum == MovementOrder.MovementOrderEnum.Follow;
					}
					appliedFormation3.ApplyActionOnEachUnit(delegate(Agent agent)
					{
						agent.UpdateCachedAndFormationValues(updateOnlyMovement: true, arrangementChangeAllowed: false);
					}, flag2 ? Mission.Current.MainAgent : null);
				}
			}
		}
	}

	public void ForceUpdateAllUnits()
	{
		OrderController_OnOrderIssued_Aux(OrderType.Move, team.FormationsIncludingSpecialAndEmpty, null);
	}

	public virtual void FinishDeployment()
	{
	}

	public override void OnRemoveBehavior()
	{
		if (team != null)
		{
			team.OnOrderIssued -= OrderController_OnOrderIssued;
		}
		base.Mission.SetMissionMode(previousMissionMode, atStart: false);
		base.OnRemoveBehavior();
	}

	public void InitializeDeploymentPoints()
	{
		if (areDeploymentPointsInitialized)
		{
			return;
		}
		foreach (DeploymentPoint item in base.Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>())
		{
			item.Hide();
		}
		areDeploymentPointsInitialized = true;
	}
}
