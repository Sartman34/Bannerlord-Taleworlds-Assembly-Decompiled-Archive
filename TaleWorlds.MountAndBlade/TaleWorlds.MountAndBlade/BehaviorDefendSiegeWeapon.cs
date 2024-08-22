using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public class BehaviorDefendSiegeWeapon : BehaviorComponent
{
	private WorldPosition _defensePosition = WorldPosition.Invalid;

	private TacticalPosition _tacticalDefendPosition;

	private SiegeWeapon _defendedSiegeWeapon;

	public BehaviorDefendSiegeWeapon(Formation formation)
		: base(formation)
	{
		CalculateCurrentOrder();
	}

	public void SetDefensePositionFromTactic(WorldPosition defensePosition)
	{
		_defensePosition = defensePosition;
	}

	public void SetDefendedSiegeWeaponFromTactic(SiegeWeapon siegeWeapon)
	{
		_defendedSiegeWeapon = siegeWeapon;
	}

	public override TextObject GetBehaviorString()
	{
		TextObject behaviorString = base.GetBehaviorString();
		TextObject variable = GameTexts.FindText("str_formation_ai_side_strings", base.Formation.AI.Side.ToString());
		behaviorString.SetTextVariable("SIDE_STRING", variable);
		behaviorString.SetTextVariable("IS_GENERAL_SIDE", "0");
		return behaviorString;
	}

	protected override void CalculateCurrentOrder()
	{
		float num = 5f;
		Vec2 vec;
		if (_tacticalDefendPosition != null)
		{
			vec = (_tacticalDefendPosition.IsInsurmountable ? (base.Formation.Team.QuerySystem.AverageEnemyPosition - _tacticalDefendPosition.Position.AsVec2).Normalized() : _tacticalDefendPosition.Direction);
		}
		else if (base.Formation.QuerySystem.ClosestEnemyFormation == null)
		{
			vec = base.Formation.Direction;
		}
		else if (_defendedSiegeWeapon != null)
		{
			vec = base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - _defendedSiegeWeapon.GameEntity.GlobalPosition.AsVec2;
			num = vec.Normalize();
			num = MathF.Min(num, 5f);
			float num2 = 0f;
			num2 = ((!(_defendedSiegeWeapon.WaitEntity != null)) ? 3f : (_defendedSiegeWeapon.WaitEntity.GlobalPosition - _defendedSiegeWeapon.GameEntity.GlobalPosition).Length);
			num = MathF.Max(num, num2);
		}
		else
		{
			vec = ((base.Formation.Direction.DotProduct((base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized()) < 0.5f) ? (base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition) : base.Formation.Direction).Normalized();
		}
		if (_tacticalDefendPosition != null)
		{
			if (!_tacticalDefendPosition.IsInsurmountable)
			{
				base.CurrentOrder = MovementOrder.MovementOrderMove(_tacticalDefendPosition.Position);
			}
			else
			{
				Vec2 vec2 = _tacticalDefendPosition.Position.AsVec2 + _tacticalDefendPosition.Width * 0.5f * vec;
				WorldPosition position = _tacticalDefendPosition.Position;
				position.SetVec2(vec2);
				base.CurrentOrder = MovementOrder.MovementOrderMove(position);
			}
			if (!_tacticalDefendPosition.IsInsurmountable)
			{
				CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(vec);
			}
			else
			{
				CurrentFacingOrder = FacingOrder.FacingOrderLookAtEnemy;
			}
		}
		else if (_defensePosition.IsValid)
		{
			WorldPosition defensePosition = _defensePosition;
			defensePosition.SetVec2(_defensePosition.AsVec2 + vec * num);
			base.CurrentOrder = MovementOrder.MovementOrderMove(defensePosition);
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(vec);
		}
		else
		{
			WorldPosition medianPosition = base.Formation.QuerySystem.MedianPosition;
			medianPosition.SetVec2(base.Formation.QuerySystem.AveragePosition);
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(vec);
		}
	}

	public override void TickOccasionally()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		if (base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.CurrentOrder.GetPosition(base.Formation)) < 100f)
		{
			if (base.Formation.QuerySystem.HasShield)
			{
				base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLine;
			}
			else if (base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation != null && base.Formation.QuerySystem.AveragePosition.DistanceSquared(base.Formation.QuerySystem.ClosestSignificantlyLargeEnemyFormation.MedianPosition.AsVec2) > 100f && base.Formation.QuerySystem.UnderRangedAttackRatio > 0.2f - ((base.Formation.ArrangementOrder.OrderEnum == ArrangementOrder.ArrangementOrderEnum.Loose) ? 0.1f : 0f))
			{
				base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLoose;
			}
			if (_tacticalDefendPosition != null)
			{
				float customWidth;
				if (_tacticalDefendPosition.TacticalPositionType == TacticalPosition.TacticalPositionTypeEnum.ChokePoint)
				{
					customWidth = _tacticalDefendPosition.Width;
				}
				else
				{
					int countOfUnits = base.Formation.CountOfUnits;
					float num = base.Formation.Interval * (float)(countOfUnits - 1) + base.Formation.UnitDiameter * (float)countOfUnits;
					customWidth = MathF.Min(_tacticalDefendPosition.Width, num / 3f);
				}
				base.Formation.FormOrder = FormOrder.FormOrderCustom(customWidth);
			}
		}
		else
		{
			base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLoose;
		}
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

	public override void ResetBehavior()
	{
		base.ResetBehavior();
		_defensePosition = WorldPosition.Invalid;
		_tacticalDefendPosition = null;
	}

	protected override float GetAiWeight()
	{
		return 1f;
	}
}
