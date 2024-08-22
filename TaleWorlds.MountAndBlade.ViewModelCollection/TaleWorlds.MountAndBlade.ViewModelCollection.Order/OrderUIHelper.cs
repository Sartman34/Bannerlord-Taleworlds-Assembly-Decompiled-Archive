namespace TaleWorlds.MountAndBlade.ViewModelCollection.Order;

public static class OrderUIHelper
{
	internal static OrderType GetOrderOverrideForUI(Formation formation, OrderSetType setType)
	{
		OrderType overridenOrderType = formation.Team.PlayerOrderController.GetOverridenOrderType(formation);
		switch (overridenOrderType)
		{
		case OrderType.Move:
		case OrderType.Charge:
		case OrderType.StandYourGround:
		case OrderType.FollowMe:
		case OrderType.GuardMe:
		case OrderType.Retreat:
		case OrderType.Advance:
		case OrderType.FallBack:
			if (setType == OrderSetType.Movement)
			{
				return overridenOrderType;
			}
			break;
		case OrderType.ArrangementLine:
		case OrderType.ArrangementCloseOrder:
		case OrderType.ArrangementLoose:
		case OrderType.ArrangementCircular:
		case OrderType.ArrangementSchiltron:
		case OrderType.ArrangementVee:
		case OrderType.ArrangementColumn:
		case OrderType.ArrangementScatter:
			if (setType == OrderSetType.Form)
			{
				return overridenOrderType;
			}
			break;
		case OrderType.LookAtEnemy:
		case OrderType.LookAtDirection:
		case OrderType.HoldFire:
		case OrderType.FireAtWill:
		case OrderType.Mount:
		case OrderType.Dismount:
		case OrderType.AIControlOn:
		case OrderType.AIControlOff:
			if (setType == OrderSetType.Toggle)
			{
				return overridenOrderType;
			}
			break;
		}
		return OrderType.None;
	}

	internal static OrderSubType GetActiveMovementOrderOfFormation(Formation formation)
	{
		OrderType orderType = GetOrderOverrideForUI(formation, OrderSetType.Movement);
		if (orderType == OrderType.None)
		{
			orderType = OrderController.GetActiveMovementOrderOf(formation);
		}
		return GetOrderSubTypeFrom(orderType);
	}

	internal static OrderSubType GetOrderSubTypeFrom(OrderType orderType)
	{
		bool flag = BannerlordConfig.OrderLayoutType == 1;
		switch (orderType)
		{
		case OrderType.Move:
			return OrderSubType.MoveToPosition;
		case OrderType.Charge:
			return OrderSubType.Charge;
		case OrderType.FollowMe:
			return OrderSubType.FollowMe;
		case OrderType.StandYourGround:
			return OrderSubType.Stop;
		case OrderType.Retreat:
			return OrderSubType.Retreat;
		case OrderType.Advance:
			return OrderSubType.Advance;
		case OrderType.FallBack:
			return OrderSubType.Fallback;
		case OrderType.ChargeWithTarget:
			return OrderSubType.Charge;
		case OrderType.ArrangementLine:
			return OrderSubType.FormLine;
		case OrderType.ArrangementCloseOrder:
			return OrderSubType.FormClose;
		case OrderType.ArrangementLoose:
			return OrderSubType.FormLoose;
		case OrderType.ArrangementCircular:
			return OrderSubType.FormCircular;
		case OrderType.ArrangementSchiltron:
			return OrderSubType.FormSchiltron;
		case OrderType.ArrangementVee:
			return OrderSubType.FormV;
		case OrderType.ArrangementColumn:
			return OrderSubType.FormColumn;
		case OrderType.ArrangementScatter:
			return OrderSubType.FormScatter;
		case OrderType.Dismount:
			return OrderSubType.ToggleMount;
		case OrderType.HoldFire:
			return OrderSubType.ToggleFire;
		case OrderType.AIControlOn:
			return OrderSubType.ToggleAI;
		case OrderType.LookAtDirection:
			return OrderSubType.ActivationFaceDirection;
		case OrderType.LookAtEnemy:
			if (!flag)
			{
				return OrderSubType.ToggleFacing;
			}
			return OrderSubType.FaceEnemy;
		default:
			return OrderSubType.None;
		}
	}

	public static bool CanOrderHaveTarget(OrderSubType orderType)
	{
		switch (orderType)
		{
		case OrderSubType.Charge:
		case OrderSubType.Advance:
			return true;
		case OrderSubType.None:
		case OrderSubType.MoveToPosition:
		case OrderSubType.FollowMe:
		case OrderSubType.Fallback:
		case OrderSubType.Stop:
		case OrderSubType.Retreat:
		case OrderSubType.FormLine:
		case OrderSubType.FormClose:
		case OrderSubType.FormLoose:
		case OrderSubType.FormCircular:
		case OrderSubType.FormSchiltron:
		case OrderSubType.FormV:
		case OrderSubType.FormColumn:
		case OrderSubType.FormScatter:
		case OrderSubType.ToggleStart:
		case OrderSubType.ToggleFacing:
		case OrderSubType.ToggleFire:
		case OrderSubType.ToggleMount:
		case OrderSubType.ToggleAI:
		case OrderSubType.ToggleTransfer:
		case OrderSubType.ToggleEnd:
		case OrderSubType.ActivationFaceDirection:
		case OrderSubType.FaceEnemy:
		case OrderSubType.Return:
			return false;
		default:
			return false;
		}
	}
}
