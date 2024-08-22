using System.Collections.Generic;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.Engine.GauntletUI;

public static class UIResourceManager
{
	private static bool _latestUIDebugModeState;

	public static ResourceDepot UIResourceDepot { get; private set; }

	public static WidgetFactory WidgetFactory { get; private set; }

	public static SpriteData SpriteData { get; private set; }

	public static BrushFactory BrushFactory { get; private set; }

	public static FontFactory FontFactory { get; private set; }

	public static TwoDimensionEngineResourceContext ResourceContext { get; private set; }

	private static bool _uiDebugMode
	{
		get
		{
			if (!UIConfig.DebugModeEnabled)
			{
				return NativeConfig.GetUIDebugMode;
			}
			return true;
		}
	}

	public static void Initialize(ResourceDepot resourceDepot, List<string> assemblyOrder)
	{
		UIResourceDepot = resourceDepot;
		WidgetFactory = new WidgetFactory(UIResourceDepot, "Prefabs");
		WidgetFactory.PrefabExtensionContext.AddExtension(new PrefabDatabindingExtension());
		WidgetFactory.Initialize(assemblyOrder);
		SpriteData = new SpriteData("SpriteData");
		SpriteData.Load(UIResourceDepot);
		FontFactory = new FontFactory(UIResourceDepot);
		FontFactory.LoadAllFonts(SpriteData);
		BrushFactory = new BrushFactory(UIResourceDepot, "Brushes", SpriteData, FontFactory);
		BrushFactory.Initialize();
		ResourceContext = new TwoDimensionEngineResourceContext();
	}

	public static void Update()
	{
		if (_latestUIDebugModeState != _uiDebugMode)
		{
			if (_uiDebugMode)
			{
				UIResourceDepot.StartWatchingChangesInDepot();
			}
			else
			{
				UIResourceDepot.StopWatchingChangesInDepot();
			}
			_latestUIDebugModeState = _uiDebugMode;
		}
		if (_uiDebugMode)
		{
			UIResourceDepot.CheckForChanges();
		}
	}

	public static void OnLanguageChange(string newLanguageCode)
	{
		FontFactory.OnLanguageChange(newLanguageCode);
	}

	public static void Clear()
	{
		WidgetFactory = null;
		SpriteData = null;
		BrushFactory = null;
		FontFactory = null;
	}
}
