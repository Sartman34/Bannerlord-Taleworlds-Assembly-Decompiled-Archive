using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class BehaviorHoldHighGround : BehaviorComponent
{
	public Formation RangedAllyFormation;

	private bool _isAllowedToChangePosition;

	private WorldPosition _lastChosenPosition;

	public BehaviorHoldHighGround(Formation formation)
		: base(formation)
	{
		_isAllowedToChangePosition = true;
		RangedAllyFormation = null;
		CalculateCurrentOrder();
	}

	protected override void CalculateCurrentOrder()
	{
		WorldPosition worldPosition;
		Vec2 direction;
		if (base.Formation.QuerySystem.ClosestEnemyFormation != null)
		{
			worldPosition = base.Formation.QuerySystem.MedianPosition;
			if (base.Formation.AI.ActiveBehavior != this)
			{
				_isAllowedToChangePosition = true;
			}
			else
			{
				float num = Math.Max((RangedAllyFormation != null) ? (RangedAllyFormation.QuerySystem.MissileRangeAdjusted * 0.8f) : 0f, 30f);
				_isAllowedToChangePosition = base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2) > num * num;
			}
			if (_isAllowedToChangePosition)
			{
				worldPosition.SetVec2(base.Formation.QuerySystem.HighGroundCloseToForeseenBattleGround);
				_lastChosenPosition = worldPosition;
			}
			else
			{
				worldPosition = _lastChosenPosition;
			}
			direction = ((base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.HighGroundCloseToForeseenBattleGround) > 25f) ? (base.Formation.QuerySystem.Team.MedianTargetFormationPosition.AsVec2 - worldPosition.AsVec2).Normalized() : ((base.Formation.Direction.DotProduct((base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized()) < 0.5f) ? (base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition) : base.Formation.Direction).Normalized());
		}
		else
		{
			direction = base.Formation.Direction;
			worldPosition = base.Formation.QuerySystem.MedianPosition;
			worldPosition.SetVec2(base.Formation.QuerySystem.AveragePosition);
		}
		base.CurrentOrder = MovementOrder.MovementOrderMove(worldPosition);
		CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(direction);
	}

	public override void TickOccasionally()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
	}

	protected override void OnBehaviorActivatedAux()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLine;
		base.Formation.FiringOrder = FiringOrder.FiringOrderFireAtWill;
		base.Formation.FormOrder = FormOrder.FormOrderDeep;
	}

	protected override float GetAiWeight()
	{
		if (base.Formation.QuerySystem.ClosestEnemyFormation == null)
		{
			return 0f;
		}
		return 1f;
	}
}
