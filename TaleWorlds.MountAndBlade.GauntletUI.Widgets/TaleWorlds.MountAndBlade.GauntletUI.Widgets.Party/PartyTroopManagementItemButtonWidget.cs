using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Party;

public class PartyTroopManagementItemButtonWidget : ButtonWidget
{
	private Widget _actionButtonsContainer;

	public Widget ActionButtonsContainer
	{
		get
		{
			return _actionButtonsContainer;
		}
		set
		{
			if (value != _actionButtonsContainer)
			{
				_actionButtonsContainer = value;
				OnPropertyChanged(value, "ActionButtonsContainer");
			}
		}
	}

	public PartyTroopManagementItemButtonWidget(UIContext context)
		: base(context)
	{
	}

	public Widget GetActionButtonAtIndex(int index)
	{
		if (ActionButtonsContainer != null)
		{
			int num = 0;
			foreach (Widget allChild in ActionButtonsContainer.AllChildren)
			{
				if (allChild.Id == "ActionButton")
				{
					if (num == index)
					{
						return allChild;
					}
					num++;
				}
			}
		}
		return null;
	}
}
