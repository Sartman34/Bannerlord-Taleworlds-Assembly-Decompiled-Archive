using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public class BehaviorScreenedSkirmish : BehaviorComponent
{
	private Formation _mainFormation;

	private bool _isFireAtWill = true;

	public BehaviorScreenedSkirmish(Formation formation)
		: base(formation)
	{
		_behaviorSide = formation.AI.Side;
		_mainFormation = formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
		CalculateCurrentOrder();
	}

	protected override void CalculateCurrentOrder()
	{
		Vec2 vec2;
		if (base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation != null && _mainFormation != null)
		{
			Vec2 vec = (base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized();
			Vec2 v = (_mainFormation.QuerySystem.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized();
			vec2 = ((!(vec.DotProduct(v) > 0.5f)) ? vec : _mainFormation.FacingOrder.GetDirection(_mainFormation));
		}
		else
		{
			vec2 = base.Formation.Direction;
		}
		WorldPosition medianPosition;
		if (_mainFormation == null)
		{
			medianPosition = base.Formation.QuerySystem.MedianPosition;
			medianPosition.SetVec2(base.Formation.QuerySystem.AveragePosition);
		}
		else
		{
			medianPosition = _mainFormation.QuerySystem.MedianPosition;
			medianPosition.SetVec2(medianPosition.AsVec2 - vec2 * ((_mainFormation.Depth + base.Formation.Depth) * 0.5f));
		}
		if (!base.CurrentOrder.CreateNewOrderWorldPosition(base.Formation, WorldPosition.WorldPositionEnforcedCache.None).IsValid || (base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation != null && (!base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.IsRangedCavalryFormation || base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.GetNavMeshVec3().AsVec2) >= base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MissileRangeAdjusted * base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MissileRangeAdjusted || base.CurrentOrder.CreateNewOrderWorldPosition(base.Formation, WorldPosition.WorldPositionEnforcedCache.NavMeshVec3).GetNavMeshVec3().DistanceSquared(medianPosition.GetNavMeshVec3()) >= base.Formation.Depth * base.Formation.Depth)))
		{
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
		}
		if (!CurrentFacingOrder.GetDirection(base.Formation).IsValid || CurrentFacingOrder.OrderEnum == FacingOrder.FacingOrderEnum.LookAtEnemy || base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation == null || base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.GetNavMeshVec3().AsVec2) >= base.Formation.QuerySystem.MissileRangeAdjusted * base.Formation.QuerySystem.MissileRangeAdjusted || (!base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.IsRangedCavalryFormation && CurrentFacingOrder.GetDirection(base.Formation).DotProduct(vec2) <= MBMath.Lerp(0.5f, 1f, 1f - MBMath.ClampFloat(base.Formation.Width, 1f, 20f) * 0.05f)))
		{
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(vec2);
		}
	}

	public override void TickOccasionally()
	{
		CalculateCurrentOrder();
		bool flag = base.Formation.QuerySystem.ClosestEnemyFormation == null || _mainFormation.QuerySystem.MedianPosition.AsVec2.DistanceSquared(base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2) <= base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2) || base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.CurrentOrder.GetPosition(base.Formation)) <= (_mainFormation.Depth + base.Formation.Depth) * (_mainFormation.Depth + base.Formation.Depth) * 0.25f;
		if (flag != _isFireAtWill)
		{
			_isFireAtWill = flag;
			base.Formation.FiringOrder = (_isFireAtWill ? FiringOrder.FiringOrderFireAtWill : FiringOrder.FiringOrderHoldYourFire);
		}
		if (_mainFormation != null && MathF.Abs(_mainFormation.Width - base.Formation.Width) > 10f)
		{
			base.Formation.FormOrder = FormOrder.FormOrderCustom(_mainFormation.Width);
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
		base.Formation.FormOrder = FormOrder.FormOrderWide;
	}

	public override TextObject GetBehaviorString()
	{
		TextObject behaviorString = base.GetBehaviorString();
		if (_mainFormation != null)
		{
			behaviorString.SetTextVariable("AI_SIDE", GameTexts.FindText("str_formation_ai_side_strings", _mainFormation.AI.Side.ToString()));
			behaviorString.SetTextVariable("CLASS", GameTexts.FindText("str_formation_class_string", _mainFormation.PhysicalClass.GetName()));
		}
		return behaviorString;
	}

	protected override float GetAiWeight()
	{
		MovementOrder m = base.CurrentOrder;
		if (m == MovementOrder.MovementOrderStop)
		{
			CalculateCurrentOrder();
		}
		if (_mainFormation == null || !_mainFormation.AI.IsMainFormation)
		{
			_mainFormation = base.Formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
		}
		if (_behaviorSide != base.Formation.AI.Side)
		{
			_behaviorSide = base.Formation.AI.Side;
		}
		if (_mainFormation == null || base.Formation.AI.IsMainFormation || base.Formation.QuerySystem.ClosestEnemyFormation == null)
		{
			return 0f;
		}
		FormationQuerySystem querySystem = base.Formation.QuerySystem;
		float num = MBMath.Lerp(0.1f, 1f, MBMath.ClampFloat(querySystem.RangedUnitRatio + querySystem.RangedCavalryUnitRatio, 0f, 0.5f) * 2f);
		float num2 = _mainFormation.Direction.Normalized().DotProduct((base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - _mainFormation.QuerySystem.MedianPosition.AsVec2).Normalized());
		float num3 = MBMath.LinearExtrapolation(0.5f, 1.1f, (num2 + 1f) / 2f);
		float value = base.Formation.QuerySystem.AveragePosition.Distance(querySystem.ClosestEnemyFormation.MedianPosition.AsVec2) / querySystem.ClosestEnemyFormation.MovementSpeedMaximum;
		float num4 = MBMath.Lerp(0.5f, 1.2f, (8f - MBMath.ClampFloat(value, 4f, 8f)) / 4f);
		return num * base.Formation.QuerySystem.MainFormationReliabilityFactor * num3 * num4;
	}
}
