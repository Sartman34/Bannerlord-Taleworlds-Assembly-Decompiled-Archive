using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Objects;

namespace TaleWorlds.MountAndBlade;

public class BehaviorSergeantMPInfantry : BehaviorComponent
{
	private enum BehaviorState
	{
		GoingToFlag,
		Attacking,
		Unset
	}

	private BehaviorState _behaviorState;

	private List<FlagCapturePoint> _flagpositions;

	private MissionMultiplayerFlagDomination _flagDominationGameMode;

	public BehaviorSergeantMPInfantry(Formation formation)
		: base(formation)
	{
		_behaviorState = BehaviorState.Unset;
		_flagpositions = base.Formation.Team.Mission.ActiveMissionObjects.FindAllWithType<FlagCapturePoint>().ToList();
		_flagDominationGameMode = base.Formation.Team.Mission.GetMissionBehavior<MissionMultiplayerFlagDomination>();
		CalculateCurrentOrder();
	}

	protected override void CalculateCurrentOrder()
	{
		BehaviorState behaviorState = ((base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation != null && ((base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.IsRangedFormation && base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2) <= ((_behaviorState == BehaviorState.Attacking) ? 3600f : 2500f)) || (base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.IsInfantryFormation && base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2) <= ((_behaviorState == BehaviorState.Attacking) ? 900f : 400f)))) ? BehaviorState.Attacking : BehaviorState.GoingToFlag);
		if (behaviorState == BehaviorState.Attacking && (_behaviorState != BehaviorState.Attacking || base.CurrentOrder.OrderEnum != MovementOrder.MovementOrderEnum.ChargeToTarget || base.CurrentOrder.TargetFormation.QuerySystem != base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation))
		{
			_behaviorState = BehaviorState.Attacking;
			base.CurrentOrder = MovementOrder.MovementOrderChargeToTarget(base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.Formation);
		}
		if (behaviorState != 0)
		{
			return;
		}
		_behaviorState = behaviorState;
		WorldPosition position;
		if (_flagpositions.Any((FlagCapturePoint fp) => _flagDominationGameMode.GetFlagOwnerTeam(fp) != base.Formation.Team))
		{
			position = new WorldPosition(base.Formation.Team.Mission.Scene, UIntPtr.Zero, _flagpositions.Where((FlagCapturePoint fp) => _flagDominationGameMode.GetFlagOwnerTeam(fp) != base.Formation.Team).MinBy((FlagCapturePoint fp) => fp.Position.AsVec2.DistanceSquared(base.Formation.QuerySystem.AveragePosition)).Position, hasValidZ: false);
		}
		else if (_flagpositions.Any((FlagCapturePoint fp) => _flagDominationGameMode.GetFlagOwnerTeam(fp) == base.Formation.Team))
		{
			position = new WorldPosition(base.Formation.Team.Mission.Scene, UIntPtr.Zero, _flagpositions.Where((FlagCapturePoint fp) => _flagDominationGameMode.GetFlagOwnerTeam(fp) == base.Formation.Team).MinBy((FlagCapturePoint fp) => fp.Position.AsVec2.DistanceSquared(base.Formation.QuerySystem.AveragePosition)).Position, hasValidZ: false);
		}
		else
		{
			position = base.Formation.QuerySystem.MedianPosition;
			position.SetVec2(base.Formation.QuerySystem.AveragePosition);
		}
		if (base.CurrentOrder.OrderEnum == MovementOrder.MovementOrderEnum.Invalid || base.CurrentOrder.GetPosition(base.Formation) != position.AsVec2)
		{
			Vec2 direction = ((base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation != null) ? (base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized() : base.Formation.Direction);
			base.CurrentOrder = MovementOrder.MovementOrderMove(position);
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(direction);
		}
	}

	public override void TickOccasionally()
	{
		_flagpositions.RemoveAll((FlagCapturePoint fp) => fp.IsDeactivated);
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		if (base.Formation.QuerySystem.HasShield && (_behaviorState == BehaviorState.Attacking || (_behaviorState == BehaviorState.GoingToFlag && base.CurrentOrder.GetPosition(base.Formation).IsValid && base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.CurrentOrder.GetPosition(base.Formation)) <= 225f)))
		{
			base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderShieldWall;
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

	protected override float GetAiWeight()
	{
		if (base.Formation.QuerySystem.IsInfantryFormation)
		{
			return 1.2f;
		}
		return 0f;
	}
}
