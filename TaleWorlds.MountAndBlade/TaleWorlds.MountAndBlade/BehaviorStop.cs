namespace TaleWorlds.MountAndBlade;

public class BehaviorStop : BehaviorComponent
{
	public override float NavmeshlessTargetPositionPenalty => 1f;

	public BehaviorStop(Formation formation)
		: base(formation)
	{
		base.CurrentOrder = MovementOrder.MovementOrderStop;
		base.BehaviorCoherence = 0f;
	}

	public override void TickOccasionally()
	{
		base.Formation.SetMovementOrder(base.CurrentOrder);
	}

	protected override void OnBehaviorActivatedAux()
	{
		base.Formation.ArrangementOrder = (base.Formation.QuerySystem.HasShield ? ArrangementOrder.ArrangementOrderShieldWall : ArrangementOrder.ArrangementOrderLine);
		base.Formation.FiringOrder = FiringOrder.FiringOrderFireAtWill;
		_lastPlayerInformTime = Mission.Current.CurrentTime;
	}

	protected override float GetAiWeight()
	{
		return 0.01f;
	}
}
