using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public class BehaviorFlank : BehaviorComponent
{
	public BehaviorFlank(Formation formation)
		: base(formation)
	{
		base.BehaviorCoherence = 0.5f;
		CalculateCurrentOrder();
	}

	protected override void CalculateCurrentOrder()
	{
		WorldPosition position = ((base.Formation.AI.Side == FormationAI.BehaviorSide.Right) ? base.Formation.QuerySystem.Team.RightFlankEdgePosition : base.Formation.QuerySystem.Team.LeftFlankEdgePosition);
		Vec2 direction = (position.AsVec2 - base.Formation.QuerySystem.AveragePosition).Normalized();
		base.CurrentOrder = MovementOrder.MovementOrderMove(position);
		CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(direction);
	}

	public override void TickOccasionally()
	{
		CalculateCurrentOrder();
		base.Formation.SetMovementOrder(base.CurrentOrder);
		base.Formation.FacingOrder = CurrentFacingOrder;
	}

	public override TextObject GetBehaviorString()
	{
		TextObject behaviorString = base.GetBehaviorString();
		behaviorString.SetTextVariable("IS_GENERAL_SIDE", "0");
		TextObject variable = GameTexts.FindText("str_formation_ai_side_strings", base.Formation.AI.Side.ToString());
		behaviorString.SetTextVariable("SIDE_STRING", variable);
		return behaviorString;
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
		FormationQuerySystem querySystem = base.Formation.QuerySystem;
		if (querySystem.ClosestEnemyFormation == null || querySystem.ClosestEnemyFormation.ClosestEnemyFormation == querySystem)
		{
			return 0f;
		}
		Vec2 vec = (querySystem.ClosestEnemyFormation.MedianPosition.AsVec2 - querySystem.AveragePosition).Normalized();
		Vec2 v = (querySystem.ClosestEnemyFormation.ClosestEnemyFormation.MedianPosition.AsVec2 - querySystem.ClosestEnemyFormation.MedianPosition.AsVec2).Normalized();
		if (vec.DotProduct(v) > -0.5f)
		{
			return 0f;
		}
		if (Mission.Current.MissionTeamAIType != Mission.MissionTeamAITypeEnum.FieldBattle)
		{
			int faceGroupId = -1;
			Vec3 position = ((base.Formation.AI.Side == FormationAI.BehaviorSide.Right) ? base.Formation.QuerySystem.Team.RightFlankEdgePosition : base.Formation.QuerySystem.Team.LeftFlankEdgePosition).GetNavMeshVec3();
			Mission.Current.Scene.GetNavigationMeshForPosition(ref position, out faceGroupId);
			if (faceGroupId >= 0)
			{
				Agent medianAgent = base.Formation.GetMedianAgent(excludeDetachedUnits: true, excludePlayer: true, base.Formation.QuerySystem.AveragePosition);
				if ((medianAgent != null && medianAgent.GetCurrentNavigationFaceId() % 10 == 1) == (faceGroupId % 10 == 1))
				{
					goto IL_0158;
				}
			}
			return 0f;
		}
		goto IL_0158;
		IL_0158:
		return 1.2f;
	}
}
