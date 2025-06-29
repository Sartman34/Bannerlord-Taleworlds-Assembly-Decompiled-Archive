using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using TaleWorlds.Library;
using TaleWorlds.Starter.Library;
using TaleWorlds.TwoDimension.Standalone;
using TaleWorlds.TwoDimension.Standalone.Native.Windows;

namespace TaleWorlds.MountAndBlade.Launcher.Library;

public class Program
{
	private static string StarterExecutable;

	private const string _pathToDigitalCompanionExe = "..\\..\\DigitalCompanion\\Mount & Blade II Bannerlord - Digital Companion.exe";

	private static WindowsFramework _windowsFramework;

	private static GraphicsForm _graphicsForm;

	private static List<string> _args;

	private static string _additionalArgs;

	private static bool _hasUnofficialModulesSelected;

	private static StandaloneUIDomain _standaloneUIDomain;

	private static bool _isTestMode;

	private static bool _gameStarted;

	static Program()
	{
		StarterExecutable = "Bannerlord.exe";
		Common.PlatformFileHelper = new PlatformFileHelperPC("Mount and Blade II Bannerlord");
		TaleWorlds.Library.Debug.DebugManager = new LauncherDebugManager();
		AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
	}

	public static void NativeMain(string commandLine)
	{
		_isTestMode = commandLine.ToLower().Contains("/runtest");
		Main(commandLine.Split().ToArray());
	}

	public static void Main(string[] args)
	{
		_args = args.ToList();
		if (!_isTestMode)
		{
			try
			{
				Common.PlatformFileHelper = new PlatformFileHelperPC("Mount and Blade II Bannerlord");
				Common.SetInvariantCulture();
				LauncherPlatform.Initialize();
				LauncherPlatform.SetLauncherMode(isLauncherModeActive: true);
				ResourceDepot resourceDepot = new ResourceDepot();
				resourceDepot.AddLocation(BasePath.Name, "Modules/Native/LauncherGUI/");
				resourceDepot.CollectResources();
				resourceDepot.StartWatchingChangesInDepot();
				string name = "M&B II: Bannerlord";
				_graphicsForm = new GraphicsForm(1154, 701, resourceDepot, borderlessWindow: true, enableWindowBlur: true, layeredWindow: true, name);
				_windowsFramework = new WindowsFramework();
				_windowsFramework.ThreadConfig = WindowsFrameworkThreadConfig.NoThread;
				_standaloneUIDomain = new StandaloneUIDomain(_graphicsForm, resourceDepot);
				_windowsFramework.Initialize(new FrameworkDomain[1] { _standaloneUIDomain });
				_windowsFramework.RegisterMessageCommunicator(_graphicsForm);
				_windowsFramework.Start();
				LauncherPlatform.SetLauncherMode(isLauncherModeActive: false);
				LauncherPlatform.Destroy();
			}
			catch (Exception ex)
			{
				TaleWorlds.Library.Debug.Print(ex.Message);
				TaleWorlds.Library.Debug.Print(ex.StackTrace);
				throw;
			}
		}
		else
		{
			_gameStarted = true;
		}
		if (_gameStarted)
		{
			LauncherPlatform.SetLauncherMode(isLauncherModeActive: false);
			TaleWorlds.Starter.Library.Program.Main(_args.ToArray());
		}
	}

	public static void StartGame()
	{
		_additionalArgs = _standaloneUIDomain.AdditionalArgs;
		_args.Add(_additionalArgs);
		_hasUnofficialModulesSelected = _standaloneUIDomain.HasUnofficialModulesSelected;
		_gameStarted = true;
		AuxFinalize();
	}

	public static void StartDigitalCompanion()
	{
		AuxFinalize();
		Process.Start(new ProcessStartInfo("..\\..\\DigitalCompanion\\Mount & Blade II Bannerlord - Digital Companion.exe"));
	}

	private static void AuxFinalize()
	{
		_windowsFramework.UnRegisterMessageCommunicator(_graphicsForm);
		_graphicsForm.Destroy();
		_windowsFramework.Stop();
		_windowsFramework = null;
		_graphicsForm = null;
		(TaleWorlds.Library.Debug.DebugManager as LauncherDebugManager)?.OnFinalize();
		User32.SetForegroundWindow(Kernel32.GetConsoleWindow());
	}

	private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
	{
		TaleWorlds.Library.Debug.Print("Resolving: " + args.Name);
		if (args.Name.Contains("ManagedStarter"))
		{
			return Assembly.LoadFrom(StarterExecutable);
		}
		return null;
	}

	public static bool IsDigitalCompanionAvailable()
	{
		return File.Exists("..\\..\\DigitalCompanion\\Mount & Blade II Bannerlord - Digital Companion.exe");
	}
}
