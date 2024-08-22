using System;
using TaleWorlds.Engine.InputSystem;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.Engine;

public static class EngineController
{
	public static event Action ConfigChange;

	public static event Action<bool> OnConstrainedStateChanged;

	[EngineCallback]
	internal static void Initialize()
	{
		IInputContext debugInput = null;
		Input.Initialize(new EngineInputManager(), debugInput);
		Common.PlatformFileHelper = new PlatformFileHelperPC(Utilities.GetApplicationName());
	}

	[EngineCallback]
	internal static void OnConfigChange()
	{
		NativeConfig.OnConfigChanged();
		if (EngineController.ConfigChange != null)
		{
			EngineController.ConfigChange();
		}
	}

	[EngineCallback]
	internal static void OnConstrainedStateChange(bool isConstrained)
	{
		EngineController.OnConstrainedStateChanged?.Invoke(isConstrained);
	}

	internal static void OnApplicationTick(float dt)
	{
		Input.Update();
		Screen.Update();
	}

	[EngineCallback]
	public static string GetVersionStr()
	{
		return ApplicationVersion.FromParametersFile().ToString();
	}

	[EngineCallback]
	public static string GetApplicationPlatformName()
	{
		return ApplicationPlatform.CurrentPlatform.ToString();
	}

	[EngineCallback]
	public static string GetModulesVersionStr()
	{
		string text = "";
		foreach (ModuleInfo module in ModuleHelper.GetModules())
		{
			text = string.Concat(text, module.Name, "#", module.Version, "\n");
		}
		return text;
	}

	[EngineCallback]
	internal static void OnControllerDisconnection()
	{
		ScreenManager.OnControllerDisconnect();
	}
}
