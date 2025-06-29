using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class BehaviorPullBack : BehaviorComponent
{
	public BehaviorPullBack(Formation formation)
		: base(formation)
	{
		CalculateCurrentOrder();
		base.BehaviorCoherence = 0.2f;
	}

	protected override void CalculateCurrentOrder()
	{
		WorldPosition medianPosition = base.Formation.QuerySystem.MedianPosition;
		if (base.Formation.QuerySystem.ClosestEnemyFormation == null)
		{
			medianPosition.SetVec2(base.Formation.QuerySystem.AveragePosition);
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
		}
		else
		{
			Vec2 vec = (base.Formation.QuerySystem.AveragePosition - base.Formation.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2).Normalized();
			medianPosition.SetVec2(base.Formation.QuerySystem.AveragePosition + 50f * vec);
			base.CurrentOrder = MovementOrder.MovementOrderMove(medianPosition);
		}
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
		base.Formation.ArrangementOrder = ArrangementOrder.ArrangementOrderLoose;
		base.Formation.FacingOrder = FacingOrder.FacingOrderLookAtEnemy;
		base.Formation.FiringOrder = FiringOrder.FiringOrderFireAtWill;
		base.Formation.FormOrder = FormOrder.FormOrderWide;
	}

	protected override float GetAiWeight()
	{
		if (base.Formation.Team.TeamAI is TeamAISiegeComponent && !(base.Formation.Team.TeamAI is TeamAISallyOutAttacker) && !(base.Formation.Team.TeamAI is TeamAISallyOutDefender))
		{
			return GetSiegeAIWeight();
		}
		FormationQuerySystem querySystem = base.Formation.QuerySystem;
		FormationQuerySystem formationQuerySystem = querySystem.ClosestSignificantlyLargeEnemyFormation;
		if (formationQuerySystem == null || formationQuerySystem.ClosestEnemyFormation != querySystem || formationQuerySystem.MovementSpeedMaximum - querySystem.MovementSpeedMaximum > 2f)
		{
			formationQuerySystem = querySystem.ClosestEnemyFormation;
			if (formationQuerySystem == null || formationQuerySystem.ClosestEnemyFormation != querySystem || formationQuerySystem.MovementSpeedMaximum - querySystem.MovementSpeedMaximum > 2f)
			{
				return 0f;
			}
		}
		float num = querySystem.AveragePosition.Distance(formationQuerySystem.MedianPosition.AsVec2) / formationQuerySystem.MovementSpeedMaximum;
		float num2 = MBMath.ClampFloat(num, 4f, 10f);
		float num3 = MBMath.Lerp(0.1f, 1f, 1f - (num2 - 4f) / 6f);
		float num4 = 0f;
		foreach (Team team in Mission.Current.Teams)
		{
			if (!team.IsEnemyOf(base.Formation.Team))
			{
				continue;
			}
			foreach (Formation item in team.FormationsIncludingSpecialAndEmpty)
			{
				if (item.CountOfUnits <= 0 || item == formationQuerySystem.Formation)
				{
					continue;
				}
				float num5 = item.QuerySystem.MedianPosition.AsVec2.Distance(formationQuerySystem.MedianPosition.AsVec2) / item.QuerySystem.MovementSpeedMaximum;
				if (!(num5 <= num + 4f) || (!(num > 8f) && item.QuerySystem.ClosestEnemyFormation != base.Formation.QuerySystem))
				{
					continue;
				}
				bool flag = false;
				if (num <= 8f)
				{
					foreach (Team team2 in base.Formation.Team.Mission.Teams)
					{
						if (!team2.IsFriendOf(base.Formation.Team))
						{
							continue;
						}
						foreach (Formation item2 in team2.FormationsIncludingSpecialAndEmpty)
						{
							if (item2.CountOfUnits > 0 && item2 != base.Formation && item2.QuerySystem.ClosestEnemyFormation == item.QuerySystem && item2.QuerySystem.MedianPosition.AsVec2.DistanceSquared(querySystem.AveragePosition) / item2.QuerySystem.MovementSpeedMaximum < num5 + 4f)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
				}
				if (!flag)
				{
					num4 += item.QuerySystem.FormationMeleeFightingPower * item.QuerySystem.GetClassWeightedFactor(1f, 1f, 1f, 1f);
				}
			}
		}
		float num6 = 0f;
		foreach (Team team3 in Mission.Current.Teams)
		{
			if (!team3.IsFriendOf(base.Formation.Team))
			{
				continue;
			}
			foreach (Formation item3 in team3.FormationsIncludingSpecialAndEmpty)
			{
				if (item3.CountOfUnits > 0 && item3 != base.Formation && item3.QuerySystem.ClosestEnemyFormation == formationQuerySystem && item3.QuerySystem.MedianPosition.AsVec2.Distance(item3.QuerySystem.ClosestEnemyFormation.MedianPosition.AsVec2) / item3.QuerySystem.MovementSpeedMaximum < 4f)
				{
					num6 += item3.QuerySystem.FormationMeleeFightingPower * item3.QuerySystem.GetClassWeightedFactor(1f, 1f, 1f, 1f);
				}
			}
		}
		return MBMath.ClampFloat((1f + num4 + formationQuerySystem.Formation.QuerySystem.FormationMeleeFightingPower * formationQuerySystem.GetClassWeightedFactor(1f, 1f, 1f, 1f)) / (base.Formation.GetFormationMeleeFightingPower() * querySystem.GetClassWeightedFactor(1f, 1f, 1f, 1f) + num6 + 1f) * querySystem.Team.RemainingPowerRatio / 3f, 0.1f, 1.21f) * num3;
	}

	private float GetSiegeAIWeight()
	{
		return 0f;
	}
}
