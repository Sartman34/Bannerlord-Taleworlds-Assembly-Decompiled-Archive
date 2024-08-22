using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public class BehaviorCavalryScreen : BehaviorComponent
{
	private Formation _mainFormation;

	private Formation _flankingEnemyCavalryFormation;

	private float _threatFormationCacheTime;

	private const float _threatFormationCacheExpireTime = 5f;

	public BehaviorCavalryScreen(Formation formation)
		: base(formation)
	{
		_behaviorSide = formation.AI.Side;
		_mainFormation = formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
		CalculateCurrentOrder();
	}

	public override void OnValidBehaviorSideChanged()
	{
		base.OnValidBehaviorSideChanged();
		_mainFormation = base.Formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
	}

	protected override void CalculateCurrentOrder()
	{
		if (_mainFormation == null || base.Formation.AI.IsMainFormation || (base.Formation.AI.Side != 0 && base.Formation.AI.Side != FormationAI.BehaviorSide.Right))
		{
			_flankingEnemyCavalryFormation = null;
			WorldPosition medianPosition = base.Formation.QuerySystem.MedianPosition;
			medianPosition.SetVec2(base.Formation.QuerySystem.AveragePosition);
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
			return;
		}
		float currentTime = Mission.Current.CurrentTime;
		if (_threatFormationCacheTime + 5f < currentTime)
		{
			_threatFormationCacheTime = currentTime;
			Vec2 vec = ((base.Formation.AI.Side == FormationAI.BehaviorSide.Left) ? _mainFormation.Direction.LeftVec() : _mainFormation.Direction.RightVec()).Normalized() - _mainFormation.Direction.Normalized();
			_flankingEnemyCavalryFormation = null;
			float num = float.MinValue;
			foreach (Team team in Mission.Current.Teams)
			{
				if (!team.IsEnemyOf(base.Formation.Team))
				{
					continue;
				}
				foreach (Formation item in team.FormationsIncludingSpecialAndEmpty)
				{
					if (item.CountOfUnits <= 0)
					{
						continue;
					}
					Vec2 vec2 = item.QuerySystem.MedianPosition.AsVec2 - _mainFormation.QuerySystem.MedianPosition.AsVec2;
					if (vec.Normalized().DotProduct(vec2.Normalized()) > 0.9238795f)
					{
						float formationPower = item.QuerySystem.FormationPower;
						if (formationPower > num)
						{
							num = formationPower;
							_flankingEnemyCavalryFormation = item;
						}
					}
				}
			}
		}
		WorldPosition medianPosition2;
		if (_flankingEnemyCavalryFormation == null)
		{
			medianPosition2 = base.Formation.QuerySystem.MedianPosition;
			medianPosition2.SetVec2(base.Formation.QuerySystem.AveragePosition);
		}
		else
		{
			Vec2 vec3 = _flankingEnemyCavalryFormation.QuerySystem.MedianPosition.AsVec2 - _mainFormation.QuerySystem.MedianPosition.AsVec2;
			float num2 = vec3.Normalize() * 0.5f;
			medianPosition2 = _mainFormation.QuerySystem.MedianPosition;
			medianPosition2.SetVec2(medianPosition2.AsVec2 + num2 * vec3);
		}
		base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition2);
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
		base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderSkein;
		base.Formation.FacingOrder = FacingOrder.FacingOrderLookAtEnemy;
		base.Formation.FiringOrder = FiringOrder.FiringOrderFireAtWill;
		base.Formation.FormOrder = FormOrder.FormOrderDeep;
	}

	public override TextObject GetBehaviorString()
	{
		TextObject behaviorString = base.GetBehaviorString();
		TextObject variable = GameTexts.FindText("str_formation_ai_side_strings", base.Formation.AI.Side.ToString());
		behaviorString.SetTextVariable("SIDE_STRING", variable);
		if (_mainFormation != null)
		{
			behaviorString.SetTextVariable("AI_SIDE", GameTexts.FindText("str_formation_ai_side_strings", _mainFormation.AI.Side.ToString()));
			behaviorString.SetTextVariable("CLASS", GameTexts.FindText("str_formation_class_string", _mainFormation.PhysicalClass.GetName()));
		}
		return behaviorString;
	}

	protected override float GetAiWeight()
	{
		if (_mainFormation == null || !_mainFormation.AI.IsMainFormation)
		{
			_mainFormation = base.Formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
		}
		if (_flankingEnemyCavalryFormation == null)
		{
			return 0f;
		}
		return 1.2f;
	}
}
