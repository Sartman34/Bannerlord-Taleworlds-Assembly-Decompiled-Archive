using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade.Objects;

namespace TaleWorlds.MountAndBlade;

public class BehaviorSergeantMPRanged : BehaviorComponent
{
	private List<FlagCapturePoint> _flagpositions;

	private Formation _attachedInfantry;

	private MissionMultiplayerFlagDomination _flagDominationGameMode;

	public BehaviorSergeantMPRanged(Formation formation)
		: base(formation)
	{
		_flagpositions = base.Formation.Team.Mission.ActiveMissionObjects.FindAllWithType<FlagCapturePoint>().ToList();
		_flagDominationGameMode = base.Formation.Team.Mission.GetMissionBehavior<MissionMultiplayerFlagDomination>();
		CalculateCurrentOrder();
	}

	protected override void CalculateCurrentOrder()
	{
		bool flag = false;
		Formation formation = null;
		float num = float.MaxValue;
		foreach (Team team in base.Formation.Team.Mission.Teams)
		{
			if (!team.IsEnemyOf(base.Formation.Team))
			{
				continue;
			}
			for (int i = 0; i < Math.Min(team.FormationsIncludingSpecialAndEmpty.Count, 8); i++)
			{
				Formation formation2 = team.FormationsIncludingSpecialAndEmpty[i];
				if (formation2.CountOfUnits <= 0)
				{
					continue;
				}
				flag = true;
				if (formation2.QuerySystem.IsCavalryFormation || formation2.QuerySystem.IsRangedCavalryFormation)
				{
					float num2 = formation2.QuerySystem.MedianPosition.AsVec2.DistanceSquared(base.Formation.QuerySystem.AveragePosition);
					if (num2 < num)
					{
						num = num2;
						formation = formation2;
					}
				}
			}
		}
		if (base.Formation.Team.FormationsIncludingEmpty.AnyQ((Formation f) => f.CountOfUnits > 0 && f != base.Formation && f.QuerySystem.IsInfantryFormation))
		{
			_attachedInfantry = base.Formation.Team.FormationsIncludingEmpty.Where((Formation f) => f.CountOfUnits > 0 && f != base.Formation && f.QuerySystem.IsInfantryFormation).MinBy((Formation f) => f.QuerySystem.MedianPosition.AsVec2.DistanceSquared(base.Formation.QuerySystem.AveragePosition));
			Formation formation3 = null;
			if (flag)
			{
				if (base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2.DistanceSquared(base.Formation.QuerySystem.AveragePosition) <= 4900f)
				{
					formation3 = base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.Formation;
				}
				else if (formation != null)
				{
					formation3 = formation;
				}
			}
			Vec2 vec = ((formation3 == null) ? _attachedInfantry.Direction : (formation3.QuerySystem.MedianPosition.AsVec2 - _attachedInfantry.QuerySystem.MedianPosition.AsVec2).Normalized());
			WorldPosition medianPosition = _attachedInfantry.QuerySystem.MedianPosition;
			medianPosition.SetVec2(medianPosition.AsVec2 - vec * ((_attachedInfantry.Depth + base.Formation.Depth) / 2f));
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(vec);
		}
		else if (base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation != null && base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2.DistanceSquared(base.Formation.QuerySystem.AveragePosition) <= 4900f)
		{
			Vec2 vec2 = (base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized();
			float num3 = base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2.Distance(base.Formation.QuerySystem.AveragePosition);
			WorldPosition medianPosition2 = base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition;
			if (num3 > base.Formation.QuerySystem.MissileRangeAdjusted)
			{
				medianPosition2.SetVec2(medianPosition2.AsVec2 - vec2 * (base.Formation.QuerySystem.MissileRangeAdjusted - base.Formation.Depth * 0.5f));
			}
			else if (num3 < base.Formation.QuerySystem.MissileRangeAdjusted * 0.4f)
			{
				medianPosition2.SetVec2(medianPosition2.AsVec2 - vec2 * (base.Formation.QuerySystem.MissileRangeAdjusted * 0.4f));
			}
			else
			{
				medianPosition2.SetVec2(base.Formation.QuerySystem.AveragePosition);
			}
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition2);
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(vec2);
		}
		else if (_flagpositions.Any((FlagCapturePoint fp) => _flagDominationGameMode.GetFlagOwnerTeam(fp) != base.Formation.Team))
		{
			Vec3 position = _flagpositions.Where((FlagCapturePoint fp) => _flagDominationGameMode.GetFlagOwnerTeam(fp) != base.Formation.Team).MinBy((FlagCapturePoint fp) => fp.Position.AsVec2.DistanceSquared(base.Formation.QuerySystem.AveragePosition)).Position;
			if (base.CurrentOrder.OrderEnum == MovementOrder.MovementOrderEnum.Invalid || base.CurrentOrder.GetPosition(base.Formation) != position.AsVec2)
			{
				Vec2 direction = ((base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation != null) ? (base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized() : base.Formation.Direction);
				WorldPosition position2 = new WorldPosition(base.Formation.Team.Mission.Scene, UIntPtr.Zero, position, hasValidZ: false);
				base.CurrentOrder = MovementOrder.MovementOrderMove(position2);
				CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(direction);
			}
		}
		else if (_flagpositions.Any((FlagCapturePoint fp) => _flagDominationGameMode.GetFlagOwnerTeam(fp) == base.Formation.Team))
		{
			Vec3 position3 = _flagpositions.Where((FlagCapturePoint fp) => _flagDominationGameMode.GetFlagOwnerTeam(fp) == base.Formation.Team).MinBy((FlagCapturePoint fp) => fp.Position.AsVec2.DistanceSquared(base.Formation.QuerySystem.AveragePosition)).Position;
			base.CurrentOrder = MovementOrder.MovementOrderMove(new WorldPosition(base.Formation.Team.Mission.Scene, UIntPtr.Zero, position3, hasValidZ: false));
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtEnemy;
		}
		else
		{
			WorldPosition medianPosition3 = base.Formation.QuerySystem.MedianPosition;
			medianPosition3.SetVec2(base.Formation.QuerySystem.AveragePosition);
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition3);
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtEnemy;
		}
	}

	public override void TickOccasionally()
	{
		_flagpositions.RemoveAll((FlagCapturePoint fp) => fp.IsDeactivated);
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
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

	protected override float GetAiWeight()
	{
		if (base.Formation.QuerySystem.IsRangedFormation)
		{
			return 1.2f;
		}
		return 0f;
	}
}
