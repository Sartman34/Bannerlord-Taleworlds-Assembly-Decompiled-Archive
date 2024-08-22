using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;

namespace TaleWorlds.MountAndBlade;

public class BehaviorFireFromInfantryCover : BehaviorComponent
{
	private Formation _mainFormation;

	private bool _isFireAtWill = true;

	public BehaviorFireFromInfantryCover(Formation formation)
		: base(formation)
	{
		_mainFormation = formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
		CalculateCurrentOrder();
	}

	protected override void CalculateCurrentOrder()
	{
		WorldPosition medianPosition = base.Formation.QuerySystem.MedianPosition;
		Vec2 vec = base.Formation.Direction;
		if (_mainFormation == null)
		{
			medianPosition.SetVec2(base.Formation.QuerySystem.AveragePosition);
		}
		else
		{
			Vec2 position = _mainFormation.GetReadonlyMovementOrderReference().GetPosition(_mainFormation);
			if (position.IsValid)
			{
				vec = (position - _mainFormation.QuerySystem.AveragePosition).Normalized();
				Vec2 vec2 = position - vec * _mainFormation.Depth * 0.33f;
				medianPosition.SetVec2(vec2);
			}
			else
			{
				medianPosition.SetVec2(base.Formation.QuerySystem.AveragePosition);
			}
		}
		base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
		CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(vec);
	}

	public override void TickOccasionally()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		if (base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.CurrentOrder.GetPosition(base.Formation)) < 100f)
		{
			base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderSquare;
		}
		Vec2 position = base.CurrentOrder.GetPosition(base.Formation);
		bool flag = base.Formation.QuerySystem.ClosestEnemyFormation == null || _mainFormation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.AveragePosition) <= base.Formation.Depth * base.Formation.Width || base.Formation.QuerySystem.AveragePosition.DistanceSquared(position) <= (_mainFormation.Depth + base.Formation.Depth) * (_mainFormation.Depth + base.Formation.Depth) * 0.25f;
		if (flag != _isFireAtWill)
		{
			_isFireAtWill = flag;
			base.Formation.FiringOrder = (_isFireAtWill ? FiringOrder.FiringOrderFireAtWill : FiringOrder.FiringOrderHoldYourFire);
		}
	}

	protected override void OnBehaviorActivatedAux()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLine;
		base.Formation.FiringOrder = FiringOrder.FiringOrderFireAtWill;
		int num = (int)MathF.Sqrt(base.Formation.CountOfUnits);
		float customWidth = (float)num * base.Formation.UnitDiameter + (float)(num - 1) * base.Formation.Interval;
		base.Formation.FormOrder = FormOrder.FormOrderCustom(customWidth);
	}

	protected override float GetAiWeight()
	{
		if (_mainFormation == null || !_mainFormation.AI.IsMainFormation)
		{
			_mainFormation = base.Formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
		}
		if (_mainFormation == null || base.Formation.AI.IsMainFormation || base.Formation.QuerySystem.ClosestEnemyFormation == null || !base.Formation.QuerySystem.IsRangedFormation)
		{
			return 0f;
		}
		return 2f;
	}
}
