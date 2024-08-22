using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public class BehaviorProtectFlank : BehaviorComponent
{
	private enum BehaviorState
	{
		HoldingFlank,
		Charging,
		Returning
	}

	private Formation _mainFormation;

	public FormationAI.BehaviorSide FlankSide;

	private BehaviorState _protectFlankState;

	private MovementOrder _movementOrder;

	private MovementOrder _chargeToTargetOrder;

	public BehaviorProtectFlank(Formation formation)
		: base(formation)
	{
		_protectFlankState = BehaviorState.HoldingFlank;
		_behaviorSide = formation.AI.Side;
		_mainFormation = formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
		CalculateCurrentOrder();
		base.CurrentOrder = _movementOrder;
	}

	protected override void CalculateCurrentOrder()
	{
		if (_mainFormation == null || base.Formation.AI.IsMainFormation || base.Formation.QuerySystem.ClosestEnemyFormation == null)
		{
			base.CurrentOrder = MovementOrder.MovementOrderStop;
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtEnemy;
		}
		else if (_protectFlankState == BehaviorState.HoldingFlank || _protectFlankState == BehaviorState.Returning)
		{
			Vec2 direction = _mainFormation.Direction;
			Vec2 vec = (base.Formation.QuerySystem.Team.MedianTargetFormationPosition.AsVec2 - _mainFormation.QuerySystem.MedianPosition.AsVec2).Normalized();
			Vec2 vec2;
			if (_behaviorSide == FormationAI.BehaviorSide.Right || FlankSide == FormationAI.BehaviorSide.Right)
			{
				vec2 = _mainFormation.CurrentPosition + vec.RightVec().Normalized() * (_mainFormation.Width + base.Formation.Width + 10f);
				vec2 -= vec * (_mainFormation.Depth + base.Formation.Depth);
			}
			else if (_behaviorSide == FormationAI.BehaviorSide.Left || FlankSide == FormationAI.BehaviorSide.Left)
			{
				vec2 = _mainFormation.CurrentPosition + vec.LeftVec().Normalized() * (_mainFormation.Width + base.Formation.Width + 10f);
				vec2 -= vec * (_mainFormation.Depth + base.Formation.Depth);
			}
			else
			{
				vec2 = _mainFormation.CurrentPosition + vec * ((_mainFormation.Depth + base.Formation.Depth) * 0.5f + 10f);
			}
			WorldPosition medianPosition = _mainFormation.QuerySystem.MedianPosition;
			medianPosition.SetVec2(vec2);
			_movementOrder = MovementOrder.MovementOrderMove(medianPosition);
			base.CurrentOrder = _movementOrder;
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(direction);
		}
	}

	private void CheckAndChangeState()
	{
		Vec2 position = _movementOrder.GetPosition(base.Formation);
		switch (_protectFlankState)
		{
		case BehaviorState.HoldingFlank:
			if (base.Formation.QuerySystem.ClosestEnemyFormation != null)
			{
				float num = 50f + (base.Formation.Depth + base.Formation.QuerySystem.ClosestEnemyFormation.Formation.Depth) / 2f;
				if (base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2.DistanceSquared(position) < num * num)
				{
					_chargeToTargetOrder = MovementOrder.MovementOrderChargeToTarget(base.Formation.QuerySystem.ClosestEnemyFormation.Formation);
					base.CurrentOrder = _chargeToTargetOrder;
					_protectFlankState = BehaviorState.Charging;
				}
			}
			break;
		case BehaviorState.Charging:
		{
			if (base.Formation.QuerySystem.ClosestEnemyFormation == null)
			{
				base.CurrentOrder = _movementOrder;
				_protectFlankState = BehaviorState.Returning;
				break;
			}
			float num2 = 60f + (base.Formation.Depth + base.Formation.QuerySystem.ClosestEnemyFormation.Formation.Depth) / 2f;
			if (base.Formation.QuerySystem.AveragePosition.DistanceSquared(position) > num2 * num2)
			{
				base.CurrentOrder = _movementOrder;
				_protectFlankState = BehaviorState.Returning;
			}
			break;
		}
		case BehaviorState.Returning:
			if (base.Formation.QuerySystem.AveragePosition.DistanceSquared(position) < 400f)
			{
				_protectFlankState = BehaviorState.HoldingFlank;
			}
			break;
		}
	}

	public override void OnValidBehaviorSideChanged()
	{
		base.OnValidBehaviorSideChanged();
		_mainFormation = base.Formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
	}

	public override void TickOccasionally()
	{
		CheckAndChangeState();
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		if (_protectFlankState == BehaviorState.HoldingFlank && base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation != null && base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2) > 1600f && base.Formation.QuerySystem.UnderRangedAttackRatio > 0.2f - ((base.Formation.ArrangementOrder.OrderEnum == ArrangementOrder.ArrangementOrderEnum.Loose) ? 0.1f : 0f))
		{
			base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLoose;
		}
		else
		{
			base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLine;
		}
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

	public override TextObject GetBehaviorString()
	{
		TextObject behaviorString = base.GetBehaviorString();
		TextObject variable = GameTexts.FindText("str_formation_ai_side_strings", base.Formation.AI.Side.ToString());
		behaviorString.SetTextVariable("IS_GENERAL_SIDE", "0");
		behaviorString.SetTextVariable("SIDE_STRING", variable);
		if (_mainFormation != null)
		{
			behaviorString.SetTextVariable("AI_SIDE", GameTexts.FindText("str_formation_ai_side_strings", _mainFormation.AI.Side.ToString()));
			behaviorString.SetTextVariable("CLASS", GameTexts.FindText("str_formation_class_string", _mainFormation.PhysicalClass.GetName()));
		}
		return behaviorString;
	}

	protected override float GetAiWeight()
	{
		if (_mainFormation == null || !_mainFormation.AI.IsMainFormation)
		{
			_mainFormation = base.Formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
		}
		if (_mainFormation == null || base.Formation.AI.IsMainFormation)
		{
			return 0f;
		}
		return 1.2f;
	}
}
