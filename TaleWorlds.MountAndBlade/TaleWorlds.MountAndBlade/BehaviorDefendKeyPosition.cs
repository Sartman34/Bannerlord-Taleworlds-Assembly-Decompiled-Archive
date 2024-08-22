using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class BehaviorDefendKeyPosition : BehaviorComponent
{
	private WorldPosition _defensePosition = WorldPosition.Invalid;

	public WorldPosition EnemyClusterPosition = WorldPosition.Invalid;

	private readonly QueryData<WorldPosition> _behaviorPosition;

	public WorldPosition DefensePosition
	{
		get
		{
			return _behaviorPosition.Value;
		}
		set
		{
			_defensePosition = value;
		}
	}

	public BehaviorDefendKeyPosition(Formation formation)
		: base(formation)
	{
		_behaviorPosition = new QueryData<WorldPosition>(() => Mission.Current.FindBestDefendingPosition(EnemyClusterPosition, _defensePosition), 5f);
		CalculateCurrentOrder();
	}

	protected override void CalculateCurrentOrder()
	{
		Vec2 direction = ((base.Formation.QuerySystem.ClosestEnemyFormation != null) ? ((base.Formation.Direction.DotProduct((base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized()) < 0.5f) ? (base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition) : base.Formation.Direction).Normalized() : base.Formation.Direction);
		if (DefensePosition.IsValid)
		{
			base.CurrentOrder = MovementOrder.MovementOrderMove(DefensePosition);
		}
		else
		{
			WorldPosition medianPosition = base.Formation.QuerySystem.MedianPosition;
			medianPosition.SetVec2(base.Formation.QuerySystem.AveragePosition);
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
		}
		CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(direction);
	}

	public override void TickOccasionally()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		if (base.Formation.QuerySystem.HasShield && base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.CurrentOrder.GetPosition(base.Formation)) < base.Formation.Depth * base.Formation.Depth * 4f)
		{
			base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderShieldWall;
		}
		else
		{
			base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLoose;
		}
	}

	protected override void OnBehaviorActivatedAux()
	{
		base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLoose;
		base.Formation.FiringOrder = FiringOrder.FiringOrderFireAtWill;
		base.Formation.FormOrder = FormOrder.FormOrderWide;
	}

	protected override float GetAiWeight()
	{
		return 10f;
	}
}
