using TaleWorlds.DotNet;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.Engine;

internal class EngineScreenManager
{
	private static NativeArrayEnumerator<int> _lastPressedKeys;

	static EngineScreenManager()
	{
	}

	[EngineCallback]
	internal static void PreTick(float dt)
	{
		ScreenManager.EarlyUpdate(EngineApplicationInterface.IScreen.GetUsableAreaPercentages());
	}

	[EngineCallback]
	public static void Tick(float dt)
	{
		bool mouseVisible = EngineApplicationInterface.IScreen.GetMouseVisible();
		ScreenManager.Tick(dt, mouseVisible);
	}

	[EngineCallback]
	internal static void LateTick(float dt)
	{
		ScreenManager.LateTick(dt);
	}

	[EngineCallback]
	internal static void OnOnscreenKeyboardDone(string inputText)
	{
		ScreenManager.OnOnscreenKeyboardDone(inputText);
	}

	[EngineCallback]
	internal static void OnOnscreenKeyboardCanceled()
	{
		ScreenManager.OnOnscreenKeyboardCanceled();
	}

	[EngineCallback]
	internal static void OnGameWindowFocusChange(bool focusGained)
	{
		ScreenManager.OnGameWindowFocusChange(focusGained);
	}

	[EngineCallback]
	internal static void Update()
	{
		ScreenManager.Update(_lastPressedKeys);
	}

	[EngineCallback]
	internal static void InitializeLastPressedKeys(NativeArray lastKeysPressed)
	{
		_lastPressedKeys = new NativeArrayEnumerator<int>(lastKeysPressed);
	}

	internal static void Initialize()
	{
		ScreenManager.Initialize(new ScreenManagerEngineConnection());
	}
}
