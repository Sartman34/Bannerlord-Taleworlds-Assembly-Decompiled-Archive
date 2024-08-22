using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public class BehaviorSkirmishBehindFormation : BehaviorComponent
{
	public Formation ReferenceFormation;

	private bool _isFireAtWill = true;

	public BehaviorSkirmishBehindFormation(Formation formation)
		: base(formation)
	{
		_behaviorSide = formation.AI.Side;
		CalculateCurrentOrder();
	}

	protected override void CalculateCurrentOrder()
	{
		Vec2 vec = ((base.Formation.QuerySystem.ClosestEnemyFormation == null) ? base.Formation.Direction : ((base.Formation.Direction.DotProduct((base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized()) > 0.5f) ? (base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition) : base.Formation.Direction).Normalized());
		WorldPosition medianPosition;
		if (ReferenceFormation == null)
		{
			medianPosition = base.Formation.QuerySystem.MedianPosition;
			medianPosition.SetVec2(base.Formation.QuerySystem.AveragePosition);
		}
		else
		{
			medianPosition = ReferenceFormation.QuerySystem.MedianPosition;
			medianPosition.SetVec2(medianPosition.AsVec2 - vec * ((ReferenceFormation.Depth + base.Formation.Depth) * 0.5f));
		}
		if (base.CurrentOrder.GetPosition(base.Formation).IsValid)
		{
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
		}
		else
		{
			FormationQuerySystem closestSignificantlyLargeEnemyFormation = base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation;
			if ((closestSignificantlyLargeEnemyFormation != null && (!closestSignificantlyLargeEnemyFormation.IsRangedCavalryFormation || base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.GetNavMeshVec3().AsVec2) >= closestSignificantlyLargeEnemyFormation.MissileRangeAdjusted * closestSignificantlyLargeEnemyFormation.MissileRangeAdjusted)) || base.CurrentOrder.CreateNewOrderWorldPosition(base.Formation, WorldPosition.WorldPositionEnforcedCache.NavMeshVec3).GetNavMeshVec3().DistanceSquared(medianPosition.GetNavMeshVec3()) >= base.Formation.Depth * base.Formation.Depth)
			{
				base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
			}
		}
		if (!CurrentFacingOrder.GetDirection(base.Formation).IsValid || CurrentFacingOrder.OrderEnum == FacingOrder.FacingOrderEnum.LookAtEnemy || base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation == null || base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.GetNavMeshVec3().AsVec2) >= base.Formation.QuerySystem.MissileRangeAdjusted * base.Formation.QuerySystem.MissileRangeAdjusted || (!base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.IsRangedCavalryFormation && CurrentFacingOrder.GetDirection(base.Formation).DotProduct(vec) <= MBMath.Lerp(0.5f, 1f, 1f - MBMath.ClampFloat(base.Formation.Width, 1f, 20f) * 0.05f)))
		{
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(vec);
		}
	}

	public override void TickOccasionally()
	{
		CalculateCurrentOrder();
		bool flag = base.Formation.QuerySystem.ClosestEnemyFormation == null || ReferenceFormation.QuerySystem.MedianPosition.AsVec2.DistanceSquared(base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2) <= base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2) || base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.CurrentOrder.GetPosition(base.Formation)) <= (ReferenceFormation.Depth + base.Formation.Depth) * (ReferenceFormation.Depth + base.Formation.Depth) * 0.25f;
		if (flag != _isFireAtWill)
		{
			_isFireAtWill = flag;
			if (_isFireAtWill)
			{
				base.Formation.FiringOrder = FiringOrder.FiringOrderFireAtWill;
			}
			else
			{
				base.Formation.FiringOrder = FiringOrder.FiringOrderHoldYourFire;
			}
		}
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
		base.Formation.FormOrder = FormOrder.FormOrderWider;
	}

	public override TextObject GetBehaviorString()
	{
		TextObject behaviorString = base.GetBehaviorString();
		if (ReferenceFormation != null)
		{
			behaviorString.SetTextVariable("AI_SIDE", GameTexts.FindText("str_formation_ai_side_strings", ReferenceFormation.AI.Side.ToString()));
			behaviorString.SetTextVariable("CLASS", GameTexts.FindText("str_formation_class_string", ReferenceFormation.PhysicalClass.GetName()));
		}
		return behaviorString;
	}

	protected override float GetAiWeight()
	{
		return 10f;
	}
}
