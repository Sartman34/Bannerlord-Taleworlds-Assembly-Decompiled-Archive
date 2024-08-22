using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class BehaviorHorseArcherSkirmish : BehaviorComponent
{
	private bool _rushMode;

	private bool _isEnemyReachable = true;

	public BehaviorHorseArcherSkirmish(Formation formation)
		: base(formation)
	{
		CalculateCurrentOrder();
		base.BehaviorCoherence = 0.5f;
	}

	protected override float GetAiWeight()
	{
		if (!_isEnemyReachable)
		{
			return 0.09f;
		}
		return 0.9f;
	}

	protected override void OnBehaviorActivatedAux()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLine;
		base.Formation.FacingOrder = FacingOrder.FacingOrderLookAtEnemy;
		base.Formation.FiringOrder = FiringOrder.FiringOrderFireAtWill;
		base.Formation.FormOrder = FormOrder.FormOrderDeep;
	}

	protected override void CalculateCurrentOrder()
	{
		WorldPosition position = base.Formation.QuerySystem.MedianPosition;
		_isEnemyReachable = base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation != null && (!(base.Formation.Team.TeamAI is TeamAISiegeComponent) || !TeamAISiegeComponent.IsFormationInsideCastle(base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.Formation, includeOnlyPositionedUnits: false));
		Vec2 averagePosition = base.Formation.QuerySystem.AveragePosition;
		if (!_isEnemyReachable)
		{
			position.SetVec2(averagePosition);
		}
		else
		{
			WorldPosition medianPosition = base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition;
			int num = 0;
			Vec2 vec = Vec2.Zero;
			foreach (Formation item in base.Formation.Team.FormationsIncludingSpecialAndEmpty)
			{
				if (item != base.Formation && item.CountOfUnits > 0)
				{
					num++;
					vec += item.QuerySystem.MedianPosition.AsVec2;
				}
			}
			if (num > 0)
			{
				vec /= (float)num;
			}
			else
			{
				vec = averagePosition;
			}
			WorldPosition medianTargetFormationPosition = base.Formation.QuerySystem.Team.MedianTargetFormationPosition;
			Vec2 vec2 = (medianTargetFormationPosition.AsVec2 - vec).Normalized();
			float missileRangeAdjusted = base.Formation.QuerySystem.MissileRangeAdjusted;
			if (_rushMode)
			{
				float num2 = averagePosition.DistanceSquared(medianPosition.AsVec2);
				if (num2 > base.Formation.QuerySystem.MissileRangeAdjusted * base.Formation.QuerySystem.MissileRangeAdjusted)
				{
					position = medianTargetFormationPosition;
					position.SetVec2(position.AsVec2 - vec2 * (missileRangeAdjusted - (10f + base.Formation.Depth * 0.5f)));
				}
				else if (base.Formation.QuerySystem.ClosestEnemyFormation.IsCavalryFormation || num2 <= 400f || base.Formation.QuerySystem.UnderRangedAttackRatio >= 0.4f)
				{
					position = base.Formation.QuerySystem.Team.MedianPosition;
					position.SetVec2(vec - (((num > 0) ? 30f : 80f) + base.Formation.Depth) * vec2);
					_rushMode = false;
				}
				else
				{
					position = base.Formation.QuerySystem.Team.MedianPosition;
					Vec2 vec3 = (medianPosition.AsVec2 - averagePosition).Normalized();
					position.SetVec2(medianPosition.AsVec2 - vec3 * (missileRangeAdjusted - (10f + base.Formation.Depth * 0.5f)));
				}
			}
			else
			{
				if (num > 0)
				{
					position = base.Formation.QuerySystem.Team.MedianPosition;
					position.SetVec2(vec - (30f + base.Formation.Depth) * vec2);
				}
				else
				{
					position = base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition;
					position.SetVec2(position.AsVec2 - 80f * vec2);
				}
				if (position.AsVec2.DistanceSquared(averagePosition) <= 400f)
				{
					position = medianTargetFormationPosition;
					position.SetVec2(position.AsVec2 - vec2 * (missileRangeAdjusted - (10f + base.Formation.Depth * 0.5f)));
					_rushMode = true;
				}
			}
		}
		base.CurrentOrder = MovementOrder.MovementOrderMove(position);
	}

	public override void TickOccasionally()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
	}
}
