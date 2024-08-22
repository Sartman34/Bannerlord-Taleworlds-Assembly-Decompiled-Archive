using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;

namespace TaleWorlds.MountAndBlade;

public class BehaviorDefensiveRing : BehaviorComponent
{
	public TacticalPosition TacticalDefendPosition;

	public BehaviorDefensiveRing(Formation formation)
		: base(formation)
	{
		CalculateCurrentOrder();
	}

	protected override void CalculateCurrentOrder()
	{
		Vec2 direction = ((TacticalDefendPosition != null) ? TacticalDefendPosition.Direction : ((base.Formation.QuerySystem.ClosestEnemyFormation != null) ? ((base.Formation.Direction.DotProduct((base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized()) < 0.5f) ? (base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition) : base.Formation.Direction).Normalized() : base.Formation.Direction));
		if (TacticalDefendPosition != null)
		{
			base.CurrentOrder = MovementOrder.MovementOrderMove(TacticalDefendPosition.Position);
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(direction);
			return;
		}
		WorldPosition medianPosition = base.Formation.QuerySystem.MedianPosition;
		medianPosition.SetVec2(base.Formation.QuerySystem.AveragePosition);
		base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
		CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(direction);
	}

	public override void TickOccasionally()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		if (base.Formation.QuerySystem.AveragePosition.Distance(base.CurrentOrder.GetPosition(base.Formation)) - base.Formation.Arrangement.Depth * 0.5f < 10f)
		{
			base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderCircle;
			if (base.Formation.Team.FormationsIncludingEmpty.AnyQ((Formation f) => f.CountOfUnits > 0 && f.QuerySystem.IsRangedFormation))
			{
				Formation formation = base.Formation.Team.FormationsIncludingEmpty.WhereQ((Formation f) => f.CountOfUnits > 0 && f.QuerySystem.IsRangedFormation).MaxBy((Formation f) => f.CountOfUnits);
				int num = (int)TaleWorlds.Library.MathF.Sqrt(formation.CountOfUnits);
				float num2 = ((float)num * formation.UnitDiameter + (float)(num - 1) * formation.Interval) * 0.5f * 1.414213f;
				int num3 = base.Formation.Arrangement.UnitCount;
				int num4 = 0;
				while (num3 > 0)
				{
					double a = (double)(num2 + base.Formation.Distance * (float)num4 + base.Formation.UnitDiameter * (float)(num4 + 1)) * Math.PI * 2.0 / (double)(base.Formation.UnitDiameter + base.Formation.Interval);
					num3 -= (int)Math.Ceiling(a);
					num4++;
				}
				float num5 = num2 + (float)num4 * base.Formation.UnitDiameter + (float)(num4 - 1) * base.Formation.Distance;
				base.Formation.FormOrder = FormOrder.FormOrderCustom(num5 * 2f);
			}
		}
		else
		{
			base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLoose;
		}
	}

	protected override void OnBehaviorActivatedAux()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLoose;
		base.Formation.FiringOrder = FiringOrder.FiringOrderFireAtWill;
		base.Formation.FormOrder = FormOrder.FormOrderDeep;
	}

	public override void ResetBehavior()
	{
		base.ResetBehavior();
		TacticalDefendPosition = null;
	}

	protected override float GetAiWeight()
	{
		if (TacticalDefendPosition == null)
		{
			return 0f;
		}
		return 1f;
	}
}
