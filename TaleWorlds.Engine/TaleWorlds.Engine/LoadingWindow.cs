namespace TaleWorlds.Engine;

public static class LoadingWindow
{
	private static ILoadingWindowManager _loadingWindowManager;

	public static bool IsLoadingWindowActive { get; private set; }

	public static void Initialize(ILoadingWindowManager loadingWindowManager)
	{
		_loadingWindowManager = loadingWindowManager;
	}

	public static void Destroy()
	{
		if (IsLoadingWindowActive)
		{
			DisableGlobalLoadingWindow();
		}
		_loadingWindowManager = null;
	}

	public static void DisableGlobalLoadingWindow()
	{
		if (_loadingWindowManager != null)
		{
			if (IsLoadingWindowActive)
			{
				_loadingWindowManager.DisableLoadingWindow();
				Utilities.DisableGlobalLoadingWindow();
				Utilities.OnLoadingWindowDisabled();
			}
			IsLoadingWindowActive = false;
			Utilities.DebugSetGlobalLoadingWindowState(newState: false);
		}
	}

	public static bool GetGlobalLoadingWindowState()
	{
		return IsLoadingWindowActive;
	}

	public static void EnableGlobalLoadingWindow()
	{
		if (_loadingWindowManager != null)
		{
			IsLoadingWindowActive = true;
			Utilities.DebugSetGlobalLoadingWindowState(newState: true);
			if (IsLoadingWindowActive)
			{
				_loadingWindowManager.EnableLoadingWindow();
				Utilities.OnLoadingWindowEnabled();
			}
		}
	}
}
