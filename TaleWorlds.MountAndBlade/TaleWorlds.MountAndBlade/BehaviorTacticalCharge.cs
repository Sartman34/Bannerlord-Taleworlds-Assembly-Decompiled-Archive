using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public class BehaviorTacticalCharge : BehaviorComponent
{
	private enum ChargeState
	{
		Undetermined,
		Charging,
		ChargingPast,
		Reforming,
		Bracing
	}

	private ChargeState _chargeState;

	private FormationQuerySystem _lastTarget;

	private Vec2 _initialChargeDirection;

	private float _desiredChargeStopDistance;

	private WorldPosition _lastReformDestination;

	private Timer _chargingPastTimer;

	private Timer _reformTimer;

	private Vec2 _bracePosition = Vec2.Invalid;

	public override float NavmeshlessTargetPositionPenalty => 1f;

	public BehaviorTacticalCharge(Formation formation)
		: base(formation)
	{
		_lastTarget = null;
		base.CurrentOrder = MovementOrder.MovementOrderCharge;
		CurrentFacingOrder = FacingOrder.FacingOrderLookAtEnemy;
		_chargeState = ChargeState.Undetermined;
		base.BehaviorCoherence = 0.5f;
		_desiredChargeStopDistance = 20f;
	}

	public override void TickOccasionally()
	{
		base.TickOccasionally();
		if (base.Formation.AI.ActiveBehavior == this)
		{
			CalculateCurrentOrder();
			base.Formation.SetMovementOrder(base.CurrentOrder);
			base.Formation.FacingOrder = CurrentFacingOrder;
		}
	}

	private ChargeState CheckAndChangeState()
	{
		ChargeState result = _chargeState;
		if (base.Formation.QuerySystem.ClosestEnemyFormation == null)
		{
			result = ChargeState.Undetermined;
		}
		else
		{
			switch (_chargeState)
			{
			case ChargeState.Undetermined:
				if (base.Formation.QuerySystem.ClosestEnemyFormation != null && ((!base.Formation.QuerySystem.IsCavalryFormation && !base.Formation.QuerySystem.IsRangedCavalryFormation) || base.Formation.QuerySystem.AveragePosition.Distance(base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2) / base.Formation.QuerySystem.MovementSpeedMaximum <= 5f))
				{
					result = ChargeState.Charging;
				}
				break;
			case ChargeState.Charging:
				if (_lastTarget == null || _lastTarget.Formation.CountOfUnits == 0)
				{
					result = ChargeState.Undetermined;
				}
				else if (!base.Formation.QuerySystem.IsCavalryFormation && !base.Formation.QuerySystem.IsRangedCavalryFormation)
				{
					if (!base.Formation.QuerySystem.IsInfantryFormation || !base.Formation.QuerySystem.ClosestEnemyFormation.IsCavalryFormation)
					{
						result = ChargeState.Charging;
						break;
					}
					Vec2 vec2 = base.Formation.QuerySystem.AveragePosition - base.Formation.QuerySystem.ClosestEnemyFormation.AveragePosition;
					float num3 = vec2.Normalize();
					Vec2 currentVelocity2 = base.Formation.QuerySystem.ClosestEnemyFormation.CurrentVelocity;
					float num4 = currentVelocity2.Normalize();
					if (num3 / num4 <= 6f && vec2.DotProduct(currentVelocity2) > 0.5f)
					{
						_chargeState = ChargeState.Bracing;
					}
				}
				else if (_initialChargeDirection.DotProduct(base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition) <= 0f)
				{
					result = ChargeState.ChargingPast;
				}
				break;
			case ChargeState.ChargingPast:
				if (_chargingPastTimer.Check(Mission.Current.CurrentTime) || base.Formation.QuerySystem.AveragePosition.Distance(base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2) >= _desiredChargeStopDistance)
				{
					result = ChargeState.Reforming;
				}
				break;
			case ChargeState.Reforming:
				if (_reformTimer.Check(Mission.Current.CurrentTime) || base.Formation.QuerySystem.AveragePosition.Distance(base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2) <= 30f)
				{
					result = ChargeState.Charging;
				}
				break;
			case ChargeState.Bracing:
			{
				bool flag = false;
				if (base.Formation.QuerySystem.IsInfantryFormation && base.Formation.QuerySystem.ClosestEnemyFormation.IsCavalryFormation)
				{
					Vec2 vec = base.Formation.QuerySystem.AveragePosition - base.Formation.QuerySystem.ClosestEnemyFormation.AveragePosition;
					float num = vec.Normalize();
					Vec2 currentVelocity = base.Formation.QuerySystem.ClosestEnemyFormation.CurrentVelocity;
					float num2 = currentVelocity.Normalize();
					if (num / num2 <= 8f && vec.DotProduct(currentVelocity) > 0.33f)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					_bracePosition = Vec2.Invalid;
					_chargeState = ChargeState.Charging;
				}
				break;
			}
			}
		}
		return result;
	}

	protected override void CalculateCurrentOrder()
	{
		if (base.Formation.QuerySystem.ClosestEnemyFormation == null || ((base.Formation.QuerySystem.IsCavalryFormation || base.Formation.QuerySystem.IsRangedCavalryFormation) && (base.Formation.QuerySystem.ClosestEnemyFormation.IsCavalryFormation || base.Formation.QuerySystem.ClosestEnemyFormation.IsRangedCavalryFormation)))
		{
			base.CurrentOrder = MovementOrder.MovementOrderCharge;
			return;
		}
		ChargeState chargeState = CheckAndChangeState();
		if (chargeState != _chargeState)
		{
			_chargeState = chargeState;
			switch (_chargeState)
			{
			case ChargeState.Undetermined:
				base.CurrentOrder = MovementOrder.MovementOrderCharge;
				break;
			case ChargeState.Charging:
				_lastTarget = base.Formation.QuerySystem.ClosestEnemyFormation;
				if (base.Formation.QuerySystem.IsCavalryFormation || base.Formation.QuerySystem.IsRangedCavalryFormation)
				{
					_initialChargeDirection = _lastTarget.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition;
					float value = _initialChargeDirection.Normalize();
					_desiredChargeStopDistance = MBMath.ClampFloat(value, 20f, 50f);
				}
				break;
			case ChargeState.ChargingPast:
				_chargingPastTimer = new Timer(Mission.Current.CurrentTime, 5f);
				break;
			case ChargeState.Reforming:
				_reformTimer = new Timer(Mission.Current.CurrentTime, 2f);
				break;
			case ChargeState.Bracing:
			{
				Vec2 vec = (base.Formation.QuerySystem.Team.MedianTargetFormationPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized();
				_bracePosition = base.Formation.QuerySystem.AveragePosition + vec * 5f;
				break;
			}
			}
		}
		switch (_chargeState)
		{
		case ChargeState.Undetermined:
			if (base.Formation.QuerySystem.ClosestEnemyFormation != null && (base.Formation.QuerySystem.IsCavalryFormation || base.Formation.QuerySystem.IsRangedCavalryFormation))
			{
				base.CurrentOrder = MovementOrder.MovementOrderMove(base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition);
			}
			else
			{
				base.CurrentOrder = MovementOrder.MovementOrderCharge;
			}
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtEnemy;
			break;
		case ChargeState.Charging:
			if (!base.Formation.QuerySystem.IsCavalryFormation && !base.Formation.QuerySystem.IsRangedCavalryFormation)
			{
				if (base.Formation.Width >= base.Formation.QuerySystem.ClosestEnemyFormation.Formation.Width * (1f + ((base.Formation.GetReadonlyMovementOrderReference().OrderEnum != MovementOrder.MovementOrderEnum.Charge) ? 0.1f : 0f)))
				{
					base.CurrentOrder = MovementOrder.MovementOrderCharge;
					CurrentFacingOrder = FacingOrder.FacingOrderLookAtEnemy;
				}
				else
				{
					WorldPosition medianPosition2 = base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition;
					base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition2);
					CurrentFacingOrder = FacingOrder.FacingOrderLookAtEnemy;
				}
			}
			else
			{
				Vec2 vec4 = (_lastTarget.MedianPosition.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized();
				WorldPosition medianPosition3 = _lastTarget.MedianPosition;
				Vec2 vec5 = medianPosition3.AsVec2 + vec4 * _desiredChargeStopDistance;
				medianPosition3.SetVec2(vec5);
				base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition3);
				CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(vec4);
			}
			break;
		case ChargeState.ChargingPast:
		{
			Vec2 vec2 = (base.Formation.QuerySystem.AveragePosition - _lastTarget.MedianPosition.AsVec2).Normalized();
			_lastReformDestination = _lastTarget.MedianPosition;
			Vec2 vec3 = _lastTarget.MedianPosition.AsVec2 + vec2 * _desiredChargeStopDistance;
			_lastReformDestination.SetVec2(vec3);
			base.CurrentOrder = MovementOrder.MovementOrderMove(_lastReformDestination);
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(vec2);
			break;
		}
		case ChargeState.Reforming:
			base.CurrentOrder = MovementOrder.MovementOrderMove(_lastReformDestination);
			CurrentFacingOrder = FacingOrder.FacingOrderLookAtEnemy;
			break;
		case ChargeState.Bracing:
		{
			WorldPosition medianPosition = base.Formation.QuerySystem.MedianPosition;
			medianPosition.SetVec2(_bracePosition);
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
			break;
		}
		}
	}

	protected override void OnBehaviorActivatedAux()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
		if (base.Formation.QuerySystem.IsCavalryFormation || base.Formation.QuerySystem.IsRangedCavalryFormation)
		{
			base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderSkein;
		}
		else
		{
			base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLine;
		}
		base.Formation.FiringOrder = FiringOrder.FiringOrderFireAtWill;
		base.Formation.FormOrder = FormOrder.FormOrderWide;
	}

	public override TextObject GetBehaviorString()
	{
		TextObject behaviorString = base.GetBehaviorString();
		if (base.Formation.QuerySystem.ClosestEnemyFormation != null)
		{
			behaviorString.SetTextVariable("AI_SIDE", GameTexts.FindText("str_formation_ai_side_strings", base.Formation.QuerySystem.ClosestEnemyFormation.Formation.AI.Side.ToString()));
			behaviorString.SetTextVariable("CLASS", GameTexts.FindText("str_formation_class_string", base.Formation.QuerySystem.ClosestEnemyFormation.Formation.PhysicalClass.GetName()));
		}
		return behaviorString;
	}

	private float CalculateAIWeight()
	{
		FormationQuerySystem querySystem = base.Formation.QuerySystem;
		if (querySystem.ClosestEnemyFormation == null)
		{
			return 0f;
		}
		float num = querySystem.AveragePosition.Distance(querySystem.ClosestEnemyFormation.MedianPosition.AsVec2) / querySystem.MovementSpeedMaximum;
		float num3;
		if (!querySystem.IsCavalryFormation && !querySystem.IsRangedCavalryFormation)
		{
			float num2 = MBMath.ClampFloat(num, 4f, 10f);
			num3 = MBMath.Lerp(0.8f, 1f, 1f - (num2 - 4f) / 6f);
		}
		else if (num <= 4f)
		{
			float num4 = MBMath.ClampFloat(num, 0f, 4f);
			num3 = MBMath.Lerp(0.8f, 1.2f, num4 / 4f);
		}
		else
		{
			float num5 = MBMath.ClampFloat(num, 4f, 10f);
			num3 = MBMath.Lerp(0.8f, 1.2f, 1f - (num5 - 4f) / 6f);
		}
		float num6 = 1f;
		if (num <= 4f)
		{
			float length = (querySystem.AveragePosition - querySystem.ClosestEnemyFormation.MedianPosition.AsVec2).Length;
			if (length > float.Epsilon)
			{
				WorldPosition medianPosition = querySystem.MedianPosition;
				medianPosition.SetVec2(querySystem.AveragePosition);
				float navMeshZ = medianPosition.GetNavMeshZ();
				if (!float.IsNaN(navMeshZ))
				{
					float value = (navMeshZ - querySystem.ClosestEnemyFormation.MedianPosition.GetNavMeshZ()) / length;
					num6 = MBMath.Lerp(0.9f, 1.1f, (MBMath.ClampFloat(value, -0.58f, 0.58f) + 0.58f) / 1.16f);
				}
			}
		}
		float num7 = 1f;
		if (num <= 4f && num >= 1.5f)
		{
			num7 = 1.2f;
		}
		float num8 = 1f;
		if (num <= 4f && querySystem.ClosestEnemyFormation.ClosestEnemyFormation != querySystem)
		{
			num8 = 1.2f;
		}
		float num9 = querySystem.GetClassWeightedFactor(1f, 1f, 1.5f, 1.5f) * querySystem.ClosestEnemyFormation.GetClassWeightedFactor(1f, 1f, 0.5f, 0.5f);
		return num3 * num6 * num7 * num8 * num9;
	}

	protected override float GetAiWeight()
	{
		float result = 0f;
		if (base.Formation.QuerySystem.ClosestEnemyFormation == null)
		{
			return 0f;
		}
		bool num = base.Formation.Team.TeamAI is TeamAISiegeComponent;
		bool flag = false;
		bool flag2 = false;
		if (!num)
		{
			flag = true;
		}
		else if ((base.Formation.Team.TeamAI as TeamAISiegeComponent).CalculateIsChargePastWallsApplicable(base.Formation.AI.Side))
		{
			flag = true;
		}
		else
		{
			flag2 = TeamAISiegeComponent.IsFormationInsideCastle(base.Formation.QuerySystem.ClosestEnemyFormation.Formation, includeOnlyPositionedUnits: true, 0.51f);
			flag = flag2 == TeamAISiegeComponent.IsFormationInsideCastle(base.Formation, includeOnlyPositionedUnits: true, flag2 ? 0.9f : 0.1f);
		}
		if (flag)
		{
			result = CalculateAIWeight();
		}
		return result;
	}
}
