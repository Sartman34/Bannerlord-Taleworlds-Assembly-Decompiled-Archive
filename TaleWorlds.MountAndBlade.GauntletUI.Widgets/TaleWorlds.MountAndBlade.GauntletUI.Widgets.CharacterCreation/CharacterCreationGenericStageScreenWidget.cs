using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.CharacterCreation;

public class CharacterCreationGenericStageScreenWidget : Widget
{
	public ButtonWidget NextButton { get; set; }

	public ButtonWidget PreviousButton { get; set; }

	public ListPanel ItemList { get; set; }

	public CharacterCreationGenericStageScreenWidget(UIContext context)
		: base(context)
	{
	}
}
