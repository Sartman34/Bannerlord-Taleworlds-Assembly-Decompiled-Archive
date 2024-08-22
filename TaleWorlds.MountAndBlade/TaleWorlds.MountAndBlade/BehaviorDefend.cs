using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class BehaviorDefend : BehaviorComponent
{
	public WorldPosition DefensePosition = WorldPosition.Invalid;

	public TacticalPosition TacticalDefendPosition;

	public BehaviorDefend(Formation formation)
		: base(formation)
	{
		CalculateCurrentOrder();
	}

	protected override void CalculateCurrentOrder()
	{
		Vec2 vec = ((TacticalDefendPosition != null) ? ((!TacticalDefendPosition.IsInsurmountable) ? TacticalDefendPosition.Direction : (base.Formation.Team.QuerySystem.AverageEnemyPosition - TacticalDefendPosition.Position.AsVec2).Normalized()) : ((base.Formation.QuerySystem.ClosestEnemyFormation != null) ? ((base.Formation.Direction.DotProduct((base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized()) < 0.5f) ? (base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition) : base.Formation.Direction).Normalized() : base.Formation.Direction));
		if (TacticalDefendPosition != null)
		{
			if (!TacticalDefendPosition.IsInsurmountable)
			{
				base.CurrentOrder = MovementOrder.MovementOrderMove(TacticalDefendPosition.Position);
			}
			else
			{
				Vec2 vec2 = TacticalDefendPosition.Position.AsVec2 + TacticalDefendPosition.Width * 0.5f * vec;
				WorldPosition position = TacticalDefendPosition.Position;
				position.SetVec2(vec2);
				base.CurrentOrder = MovementOrder.MovementOrderMove(position);
			}
			if (!TacticalDefendPosition.IsInsurmountable)
			{
				CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(vec);
			}
			else
			{
				CurrentFacingOrder = FacingOrder.FacingOrderLookAtEnemy;
			}
		}
		else if (DefensePosition.IsValid)
		{
			base.CurrentOrder = MovementOrder.MovementOrderMove(DefensePosition);
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(vec);
		}
		else
		{
			WorldPosition medianPosition = base.Formation.QuerySystem.MedianPosition;
			medianPosition.SetVec2(base.Formation.QuerySystem.AveragePosition);
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(vec);
		}
	}

	public override void TickOccasionally()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		if (base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.CurrentOrder.GetPosition(base.Formation)) < 100f)
		{
			if (base.Formation.QuerySystem.HasShield)
			{
				base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderShieldWall;
			}
			else if (base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation != null && base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2) > 100f && base.Formation.QuerySystem.UnderRangedAttackRatio > 0.2f - ((base.Formation.ArrangementOrder.OrderEnum == ArrangementOrder.ArrangementOrderEnum.Loose) ? 0.1f : 0f))
			{
				base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLoose;
			}
			if (TacticalDefendPosition != null)
			{
				float customWidth;
				if (TacticalDefendPosition.TacticalPositionType == TacticalPosition.TacticalPositionTypeEnum.ChokePoint)
				{
					customWidth = TacticalDefendPosition.Width;
				}
				else
				{
					int countOfUnits = base.Formation.CountOfUnits;
					float num = base.Formation.Interval * (float)(countOfUnits - 1) + base.Formation.UnitDiameter * (float)countOfUnits;
					customWidth = MathF.Min(TacticalDefendPosition.Width, num / 3f);
				}
				base.Formation.FormOrder = FormOrder.FormOrderCustom(customWidth);
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
		base.Formation.FormOrder = FormOrder.FormOrderWide;
	}

	public override void ResetBehavior()
	{
		base.ResetBehavior();
		DefensePosition = WorldPosition.Invalid;
		TacticalDefendPosition = null;
	}

	protected override float GetAiWeight()
	{
		return 1f;
	}
}
