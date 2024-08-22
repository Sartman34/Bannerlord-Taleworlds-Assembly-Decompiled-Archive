using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public class ExitDoor : UsableMachine
{
	private static readonly ActionIndexCache act_pickup_middle_begin = ActionIndexCache.Create("act_pickup_middle_begin");

	private static readonly ActionIndexCache act_pickup_middle_begin_left_stance = ActionIndexCache.Create("act_pickup_middle_begin_left_stance");

	private static readonly ActionIndexCache act_pickup_middle_end = ActionIndexCache.Create("act_pickup_middle_end");

	private static readonly ActionIndexCache act_pickup_middle_end_left_stance = ActionIndexCache.Create("act_pickup_middle_end_left_stance");

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		TextObject textObject = new TextObject("{=gqQPSAQZ}{KEY} Leave Area");
		textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
		return textObject;
	}

	public override string GetDescriptionText(GameEntity gameEntity = null)
	{
		return string.Empty;
	}

	protected internal override void OnInit()
	{
		base.OnInit();
		SetScriptComponentToTick(GetTickRequirement());
	}

	public override TickRequirement GetTickRequirement()
	{
		return TickRequirement.Tick | base.GetTickRequirement();
	}

	protected internal override void OnTick(float dt)
	{
		base.OnTick(dt);
		foreach (StandingPoint standingPoint in base.StandingPoints)
		{
			if (!standingPoint.HasUser)
			{
				continue;
			}
			Agent userAgent = standingPoint.UserAgent;
			ActionIndexValueCache currentActionValue = userAgent.GetCurrentActionValue(0);
			ActionIndexValueCache currentActionValue2 = userAgent.GetCurrentActionValue(1);
			if (!(currentActionValue2 == ActionIndexValueCache.act_none) || (!(currentActionValue == act_pickup_middle_begin) && !(currentActionValue == act_pickup_middle_begin_left_stance)))
			{
				if (currentActionValue2 == ActionIndexValueCache.act_none && (currentActionValue == act_pickup_middle_end || currentActionValue == act_pickup_middle_end_left_stance))
				{
					userAgent.StopUsingGameObject();
					Mission.Current.EndMission();
				}
				else if (currentActionValue2 != ActionIndexValueCache.act_none || !userAgent.SetActionChannel(0, userAgent.GetIsLeftStance() ? act_pickup_middle_begin_left_stance : act_pickup_middle_begin, ignorePriority: false, 0uL))
				{
					userAgent.StopUsingGameObject();
				}
			}
		}
	}
}
