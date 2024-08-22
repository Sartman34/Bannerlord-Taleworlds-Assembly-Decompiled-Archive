using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Mission.KillFeed.Personal;

public class SingleplayerPersonalKillFeedWidget : Widget
{
	private float _normalWidgetHeight = -1f;

	private int _speedUpWidgetLimit = 3;

	public SingleplayerPersonalKillFeedWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (_normalWidgetHeight == -1f && base.ChildCount > 1)
		{
			_normalWidgetHeight = GetChild(0).ScaledSuggestedHeight * base._inverseScaleToUse;
		}
		for (int i = 0; i < base.ChildCount; i++)
		{
			Widget child = GetChild(i);
			child.PositionYOffset = Mathf.Lerp(child.PositionYOffset, GetVerticalPositionOfChildByIndex(i, base.ChildCount), 0.2f);
		}
	}

	protected override void OnChildAdded(Widget child)
	{
		base.OnChildAdded(child);
		child.PositionYOffset = GetVerticalPositionOfChildByIndex(child.GetSiblingIndex(), base.ChildCount);
		UpdateSpeedModifiers();
	}

	private float GetVerticalPositionOfChildByIndex(int indexOfChild, int numOfTotalChild)
	{
		return 0f - _normalWidgetHeight * (float)indexOfChild;
	}

	private void UpdateSpeedModifiers()
	{
		if (base.ChildCount > _speedUpWidgetLimit)
		{
			float speedModifier = (float)(base.ChildCount - _speedUpWidgetLimit) / 3f + 1f;
			for (int i = 0; i < base.ChildCount - _speedUpWidgetLimit; i++)
			{
				(GetChild(i) as SingleplayerPersonalKillFeedItemWidget).SetSpeedModifier(speedModifier);
			}
		}
	}
}
