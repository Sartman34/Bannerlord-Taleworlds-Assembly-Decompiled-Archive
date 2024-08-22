using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public class BehaviorVanguard : BehaviorComponent
{
	private Formation _mainFormation;

	public BehaviorVanguard(Formation formation)
		: base(formation)
	{
		_behaviorSide = formation.AI.Side;
		_mainFormation = formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
		CalculateCurrentOrder();
	}

	protected override void CalculateCurrentOrder()
	{
		if (_mainFormation != null && _mainFormation.CountOfUnits == 0)
		{
			_mainFormation = base.Formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
		}
		Vec2 direction;
		WorldPosition medianPosition;
		if (_mainFormation != null)
		{
			direction = _mainFormation.Direction;
			Vec2 vec = (base.Formation.QuerySystem.Team.MedianTargetFormationPosition.AsVec2 - _mainFormation.QuerySystem.MedianPosition.AsVec2).Normalized();
			medianPosition = _mainFormation.QuerySystem.MedianPosition;
			medianPosition.SetVec2(_mainFormation.CurrentPosition + vec * ((_mainFormation.Depth + base.Formation.Depth) * 0.5f + 10f));
		}
		else
		{
			direction = base.Formation.Direction;
			medianPosition = base.Formation.QuerySystem.MedianPosition;
			medianPosition.SetVec2(base.Formation.QuerySystem.AveragePosition);
		}
		base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
		CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(direction);
	}

	public override void OnValidBehaviorSideChanged()
	{
		base.OnValidBehaviorSideChanged();
		_mainFormation = base.Formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
	}

	public override void TickOccasionally()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		if (base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation != null && base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2) > 1600f && base.Formation.QuerySystem.UnderRangedAttackRatio > 0.2f - ((base.Formation.ArrangementOrder.OrderEnum == ArrangementOrder.ArrangementOrderEnum.Loose) ? 0.1f : 0f))
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
		base.Formation.FormOrder = FormOrder.FormOrderWide;
	}

	public override TextObject GetBehaviorString()
	{
		TextObject behaviorString = base.GetBehaviorString();
		TextObject variable = GameTexts.FindText("str_formation_ai_side_strings", base.Formation.AI.Side.ToString());
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
