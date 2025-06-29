using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class BehaviorMountedSkirmish : BehaviorComponent
{
	private struct Ellipse
	{
		private readonly Vec2 _center;

		private readonly float _radius;

		private readonly float _halfLength;

		private readonly Vec2 _direction;

		public Ellipse(Vec2 center, float radius, float halfLength, Vec2 direction)
		{
			_center = center;
			_radius = radius;
			_halfLength = halfLength;
			_direction = direction;
		}

		public Vec2 GetTargetPos(Vec2 position, float distance)
		{
			Vec2 vec = _direction.LeftVec();
			Vec2 vec2 = _center + vec * _halfLength;
			Vec2 vec3 = _center - vec * _halfLength;
			Vec2 vec4 = position - _center;
			bool flag = vec4.Normalized().DotProduct(_direction) > 0f;
			Vec2 vec5 = vec4.DotProduct(vec) * vec;
			bool flag2 = vec5.Length < _halfLength;
			bool flag3 = true;
			if (flag2)
			{
				position = _center + vec5 + _direction * (_radius * (float)(flag ? 1 : (-1)));
			}
			else
			{
				flag3 = vec5.DotProduct(vec) > 0f;
				Vec2 vec6 = (position - (flag3 ? vec2 : vec3)).Normalized();
				position = (flag3 ? vec2 : vec3) + vec6 * _radius;
			}
			Vec2 vec7 = _center + vec5;
			float num = (float)Math.PI * 2f * _radius;
			while (distance > 0f)
			{
				if (flag2 && flag)
				{
					float num2 = (((vec2 - vec7).Length < distance) ? (vec2 - vec7).Length : distance);
					position = vec7 + (vec2 - vec7).Normalized() * num2;
					position += _direction * _radius;
					distance -= num2;
					flag2 = false;
					flag3 = true;
				}
				else if (!flag2 && flag3)
				{
					Vec2 v = (position - vec2).Normalized();
					float num3 = TaleWorlds.Library.MathF.Acos(MBMath.ClampFloat(_direction.DotProduct(v), -1f, 1f));
					float num4 = (float)Math.PI * 2f * (distance / num);
					float num5 = ((num3 + num4 < (float)Math.PI) ? (num3 + num4) : ((float)Math.PI));
					float num6 = (num5 - num3) / (float)Math.PI * (num / 2f);
					Vec2 direction = _direction;
					direction.RotateCCW(num5);
					position = vec2 + direction * _radius;
					distance -= num6;
					flag2 = true;
					flag = false;
				}
				else if (flag2)
				{
					float num7 = (((vec3 - vec7).Length < distance) ? (vec3 - vec7).Length : distance);
					position = vec7 + (vec3 - vec7).Normalized() * num7;
					position -= _direction * _radius;
					distance -= num7;
					flag2 = false;
					flag3 = false;
				}
				else
				{
					Vec2 vec8 = (position - vec3).Normalized();
					float num8 = TaleWorlds.Library.MathF.Acos(MBMath.ClampFloat(_direction.DotProduct(vec8), -1f, 1f));
					float num9 = (float)Math.PI * 2f * (distance / num);
					float num10 = ((num8 - num9 > 0f) ? (num8 - num9) : 0f);
					float num11 = num8 - num10;
					float num12 = num11 / (float)Math.PI * (num / 2f);
					Vec2 vec9 = vec8;
					vec9.RotateCCW(num11);
					position = vec3 + vec9 * _radius;
					distance -= num12;
					flag2 = true;
					flag = true;
				}
			}
			return position;
		}
	}

	private bool _engaging = true;

	private bool _isEnemyReachable = true;

	public BehaviorMountedSkirmish(Formation formation)
		: base(formation)
	{
		CalculateCurrentOrder();
		base.BehaviorCoherence = 0.5f;
	}

	protected override void CalculateCurrentOrder()
	{
		WorldPosition position = base.Formation.QuerySystem.MedianPosition;
		_isEnemyReachable = base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation != null && (!(base.Formation.Team.TeamAI is TeamAISiegeComponent) || !TeamAISiegeComponent.IsFormationInsideCastle(base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.Formation, includeOnlyPositionedUnits: false));
		if (!_isEnemyReachable)
		{
			position.SetVec2(base.Formation.QuerySystem.AveragePosition);
		}
		else
		{
			bool num = (base.Formation.QuerySystem.AverageAllyPosition - base.Formation.Team.QuerySystem.AverageEnemyPosition).LengthSquared <= 3600f;
			bool engaging = _engaging;
			engaging = num || ((!_engaging) ? ((base.Formation.QuerySystem.AveragePosition - base.Formation.QuerySystem.AverageAllyPosition).LengthSquared <= 3600f) : (!(base.Formation.QuerySystem.UnderRangedAttackRatio > base.Formation.QuerySystem.MakingRangedAttackRatio) && ((!base.Formation.QuerySystem.FastestSignificantlyLargeEnemyFormation.IsCavalryFormation && !base.Formation.QuerySystem.FastestSignificantlyLargeEnemyFormation.IsRangedCavalryFormation) || (base.Formation.QuerySystem.AveragePosition - base.Formation.QuerySystem.FastestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2).LengthSquared / (base.Formation.QuerySystem.FastestSignificantlyLargeEnemyFormation.MovementSpeed * base.Formation.QuerySystem.FastestSignificantlyLargeEnemyFormation.MovementSpeed) >= 16f)));
			_engaging = engaging;
			if (_engaging)
			{
				Vec2 vec = (base.Formation.QuerySystem.AveragePosition - base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.AveragePosition).Normalized().LeftVec();
				FormationQuerySystem closestSignificantlyLargeEnemyFormation = base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation;
				float num2 = 50f + (base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.Formation.Width + base.Formation.Depth) * 0.5f;
				float num3 = 0f;
				Formation formation = base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.Formation;
				foreach (Team team in Mission.Current.Teams)
				{
					if (!team.IsEnemyOf(base.Formation.Team))
					{
						continue;
					}
					foreach (Formation item in team.FormationsIncludingSpecialAndEmpty)
					{
						if (item.CountOfUnits > 0 && item.QuerySystem != closestSignificantlyLargeEnemyFormation)
						{
							Vec2 v = item.QuerySystem.AveragePosition - closestSignificantlyLargeEnemyFormation.AveragePosition;
							float num4 = v.Normalize();
							if (vec.DotProduct(v) > 0.8f && num4 < num2 && num4 > num3)
							{
								num3 = num4;
								formation = item;
							}
						}
					}
				}
				if (!(base.Formation.Team.TeamAI is TeamAISiegeComponent) && base.Formation.QuerySystem.RangedCavalryUnitRatio > 0.95f && base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.Formation == formation)
				{
					base.CurrentOrder = MovementOrder.MovementOrderCharge;
					return;
				}
				bool flag = formation.QuerySystem.IsCavalryFormation || formation.QuerySystem.IsRangedCavalryFormation;
				float num5 = (flag ? 35f : 20f);
				num5 += (formation.Depth + base.Formation.Width) * 0.5f;
				num5 = TaleWorlds.Library.MathF.Min(num5, base.Formation.QuerySystem.MissileRangeAdjusted - base.Formation.Width * 0.5f);
				Ellipse ellipse = new Ellipse(formation.QuerySystem.MedianPosition.AsVec2, num5, formation.Width * 0.5f * (flag ? 1.5f : 1f), formation.Direction);
				position.SetVec2(ellipse.GetTargetPos(base.Formation.QuerySystem.AveragePosition, 20f));
			}
			else
			{
				position = new WorldPosition(Mission.Current.Scene, new Vec3(base.Formation.QuerySystem.AverageAllyPosition, base.Formation.Team.QuerySystem.MedianPosition.GetNavMeshZ() + 100f));
			}
		}
		base.CurrentOrder = MovementOrder.MovementOrderMove(position);
	}

	public override void TickOccasionally()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
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

	protected override float GetAiWeight()
	{
		if (!_isEnemyReachable)
		{
			return 0.1f;
		}
		return 1f;
	}
}
