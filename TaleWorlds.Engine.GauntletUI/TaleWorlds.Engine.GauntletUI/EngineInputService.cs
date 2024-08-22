using TaleWorlds.GauntletUI;
using TaleWorlds.InputSystem;

namespace TaleWorlds.Engine.GauntletUI;

public class EngineInputService : IInputService
{
	private IInputContext _inputContext;

	private bool _mouseEnabled;

	private bool _keyboardEnabled;

	private bool _gamepadEnabled;

	bool IInputService.MouseEnabled => _mouseEnabled;

	bool IInputService.KeyboardEnabled => _keyboardEnabled;

	bool IInputService.GamepadEnabled => _gamepadEnabled;

	public EngineInputService(IInputContext inputContext)
	{
		_inputContext = inputContext;
	}

	public void UpdateInputDevices(bool keyboardEnabled, bool mouseEnabled, bool gamepadEnabled)
	{
		_mouseEnabled = mouseEnabled;
		_keyboardEnabled = keyboardEnabled;
		_gamepadEnabled = gamepadEnabled;
	}
}
