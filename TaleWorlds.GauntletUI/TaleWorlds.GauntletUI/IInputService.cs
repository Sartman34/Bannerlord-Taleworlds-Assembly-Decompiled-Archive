namespace TaleWorlds.GauntletUI;

public interface IInputService
{
	bool MouseEnabled { get; }

	bool KeyboardEnabled { get; }

	bool GamepadEnabled { get; }
}
