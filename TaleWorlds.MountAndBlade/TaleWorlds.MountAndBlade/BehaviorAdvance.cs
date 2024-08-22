using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public sealed class BehaviorAdvance : BehaviorComponent
{
	private bool _isInShieldWallDistance;

	private bool _switchedToShieldWallRecently;

	private Timer _switchedToShieldWallTimer;

	private Vec2 _reformPosition = Vec2.Invalid;

	public BehaviorAdvance(Formation formation)
		: base(formation)
	{
		base.BehaviorCoherence = 0.8f;
		_switchedToShieldWallTimer = new Timer(0f, 0f);
		CalculateCurrentOrder();
	}

	protected override void CalculateCurrentOrder()
	{
		FormationQuerySystem.FormationIntegrityDataGroup formationIntegrityData = base.Formation.QuerySystem.FormationIntegrityData;
		if (_switchedToShieldWallRecently && !_switchedToShieldWallTimer.Check(Mission.Current.CurrentTime) && formationIntegrityData.DeviationOfPositionsExcludeFarAgents > formationIntegrityData.AverageMaxUnlimitedSpeedExcludeFarAgents * 0.5f)
		{
			WorldPosition medianPosition = base.Formation.QuerySystem.MedianPosition;
			if (_reformPosition.IsValid)
			{
				medianPosition.SetVec2(_reformPosition);
			}
			else
			{
				Vec2 vec = (base.Formation.QuerySystem.Team.MedianTargetFormationPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized();
				_reformPosition = base.Formation.QuerySystem.AveragePosition + vec * 5f;
				medianPosition.SetVec2(_reformPosition);
			}
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
			return;
		}
		_switchedToShieldWallRecently = false;
		bool flag = false;
		if (base.Formation.QuerySystem.ClosestEnemyFormation != null && base.Formation.QuerySystem.ClosestEnemyFormation.IsCavalryFormation)
		{
			Vec2 vec2 = base.Formation.QuerySystem.AveragePosition - base.Formation.QuerySystem.ClosestEnemyFormation.AveragePosition;
			float num = vec2.Normalize();
			Vec2 currentVelocity = base.Formation.QuerySystem.ClosestEnemyFormation.CurrentVelocity;
			float num2 = currentVelocity.Normalize();
			if (num < 30f && num2 > 2f && vec2.DotProduct(currentVelocity) > 0.5f)
			{
				flag = true;
				WorldPosition medianPosition2 = base.Formation.QuerySystem.MedianPosition;
				if (_reformPosition.IsValid)
				{
					medianPosition2.SetVec2(_reformPosition);
				}
				else
				{
					Vec2 vec3 = (base.Formation.QuerySystem.Team.MedianTargetFormationPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized();
					_reformPosition = base.Formation.QuerySystem.AveragePosition + vec3 * 5f;
					medianPosition2.SetVec2(_reformPosition);
				}
				base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition2);
			}
		}
		if (flag)
		{
			return;
		}
		_reformPosition = Vec2.Invalid;
		int num3 = 0;
		bool flag2 = false;
		foreach (Team team in Mission.Current.Teams)
		{
			if (!team.IsEnemyOf(base.Formation.Team))
			{
				continue;
			}
			foreach (Formation item in team.FormationsIncludingSpecialAndEmpty)
			{
				if (item.CountOfUnits > 0)
				{
					num3++;
					flag2 = num3 == 1;
					if (num3 > 1)
					{
						break;
					}
				}
			}
		}
		FormationQuerySystem formationQuerySystem = (flag2 ? base.Formation.QuerySystem.ClosestEnemyFormation : base.Formation.QuerySystem.Team.MedianTargetFormation);
		if (formationQuerySystem != null)
		{
			WorldPosition medianPosition3 = formationQuerySystem.MedianPosition;
			medianPosition3.SetVec2(medianPosition3.AsVec2 + formationQuerySystem.Formation.Direction * formationQuerySystem.Formation.Depth * 0.5f);
			Vec2 direction = -formationQuerySystem.Formation.Direction;
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition3);
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(direction);
		}
		else
		{
			WorldPosition position = (flag2 ? base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition : base.Formation.QuerySystem.Team.MedianTargetFormationPosition);
			Vec2 direction2 = (base.Formation.QuerySystem.Team.MedianTargetFormationPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized();
			base.CurrentOrder = MovementOrder.MovementOrderMove(position);
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(direction2);
		}
	}

	protected override void OnBehaviorActivatedAux()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		_isInShieldWallDistance = false;
		base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLine;
		base.Formation.FiringOrder = FiringOrder.FiringOrderFireAtWill;
		base.Formation.FormOrder = FormOrder.FormOrderWide;
	}

	public override void TickOccasionally()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		if (base.Formation.PhysicalClass.IsMeleeInfantry())
		{
			bool flag = false;
			if (base.Formation.QuerySystem.ClosestEnemyFormation != null && base.Formation.QuerySystem.IsUnderRangedAttack)
			{
				float num = base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2);
				if (num < 6400f + (_isInShieldWallDistance ? 3600f : 0f) && num > 100f - (_isInShieldWallDistance ? 75f : 0f))
				{
					flag = true;
				}
			}
			if (flag != _isInShieldWallDistance)
			{
				_isInShieldWallDistance = flag;
				if (_isInShieldWallDistance)
				{
					if (base.Formation.QuerySystem.HasShield)
					{
						base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderShieldWall;
					}
					else
					{
						base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLoose;
					}
					_switchedToShieldWallRecently = true;
					_switchedToShieldWallTimer.Reset(Mission.Current.CurrentTime, 5f);
				}
				else
				{
					base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLine;
				}
			}
		}
		base.Formation.SetMovementOrder(base.CurrentOrder);
	}

	protected override float GetAiWeight()
	{
		return 1f;
	}
}
