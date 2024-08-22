using TaleWorlds.Engine;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade;

public interface IFocusable
{
	FocusableObjectType FocusableObjectType { get; }

	void OnFocusGain(Agent userAgent);

	void OnFocusLose(Agent userAgent);

	TextObject GetInfoTextForBeingNotInteractable(Agent userAgent);

	string GetDescriptionText(GameEntity gameEntity = null);
}
