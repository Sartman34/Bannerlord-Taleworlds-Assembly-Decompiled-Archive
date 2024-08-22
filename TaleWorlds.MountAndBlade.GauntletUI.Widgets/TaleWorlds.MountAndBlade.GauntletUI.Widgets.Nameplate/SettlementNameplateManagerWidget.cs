using System.Collections.Generic;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Nameplate;

public class SettlementNameplateManagerWidget : Widget
{
	private readonly List<SettlementNameplateWidget> _visibleNameplates = new List<SettlementNameplateWidget>();

	private List<SettlementNameplateWidget> _allChildrenNameplates = new List<SettlementNameplateWidget>();

	public SettlementNameplateManagerWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
	{
		_visibleNameplates.Clear();
		foreach (SettlementNameplateWidget allChildrenNameplate in _allChildrenNameplates)
		{
			if (allChildrenNameplate != null && allChildrenNameplate.IsVisibleOnMap)
			{
				_visibleNameplates.Add(allChildrenNameplate);
			}
		}
		_visibleNameplates.Sort();
		foreach (SettlementNameplateWidget visibleNameplate in _visibleNameplates)
		{
			visibleNameplate.DisableRender = false;
			visibleNameplate.Render(twoDimensionContext, drawContext);
			visibleNameplate.DisableRender = true;
		}
	}

	protected override void OnChildAdded(Widget child)
	{
		base.OnChildAdded(child);
		child.DisableRender = true;
		_allChildrenNameplates.Add(child as SettlementNameplateWidget);
	}

	protected override void OnChildRemoved(Widget child)
	{
		base.OnChildRemoved(child);
		_allChildrenNameplates.Remove(child as SettlementNameplateWidget);
	}

	protected override void OnDisconnectedFromRoot()
	{
		base.OnDisconnectedFromRoot();
		_allChildrenNameplates.Clear();
		_allChildrenNameplates = null;
	}
}
