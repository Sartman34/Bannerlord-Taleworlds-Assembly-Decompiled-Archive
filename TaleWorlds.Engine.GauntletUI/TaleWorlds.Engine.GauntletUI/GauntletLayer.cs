using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.Engine.GauntletUI;

public class GauntletLayer : ScreenLayer
{
	public readonly TwoDimensionView TwoDimensionView;

	public readonly UIContext UIContext;

	public readonly IGamepadNavigationContext GamepadNavigationContext;

	public readonly List<Tuple<IGauntletMovie, ViewModel>> MoviesAndDataSources;

	public readonly TwoDimensionEnginePlatform TwoDimensionEnginePlatform;

	public readonly EngineInputService EngineInputService;

	public readonly WidgetFactory WidgetFactory;

	public GauntletLayer(int localOrder, string categoryId = "GauntletLayer", bool shouldClear = false)
		: base(localOrder, categoryId)
	{
		MoviesAndDataSources = new List<Tuple<IGauntletMovie, ViewModel>>();
		WidgetFactory = UIResourceManager.WidgetFactory;
		ResourceDepot uIResourceDepot = UIResourceManager.UIResourceDepot;
		TwoDimensionView = TwoDimensionView.CreateTwoDimension();
		if (shouldClear)
		{
			TwoDimensionView.SetClearColor(255u);
			TwoDimensionView.SetRenderOption(View.ViewRenderOptions.ClearColor, value: true);
		}
		TwoDimensionEnginePlatform = new TwoDimensionEnginePlatform(TwoDimensionView);
		TwoDimensionContext twoDimensionContext = new TwoDimensionContext(TwoDimensionEnginePlatform, UIResourceManager.ResourceContext, uIResourceDepot);
		EngineInputService = new EngineInputService(base.Input);
		UIContext = new UIContext(twoDimensionContext, base.Input, EngineInputService, UIResourceManager.SpriteData, UIResourceManager.FontFactory, UIResourceManager.BrushFactory);
		UIContext.ScaleModifier = base.Scale;
		UIContext.Initialize();
		GamepadNavigationContext = new GauntletGamepadNavigationContext(GetIsBlockedAtPosition, GetLastScreenOrder, GetIsAvailableForGamepadNavigation);
		UIContext.InitializeGamepadNavigation(GamepadNavigationContext);
		base.MouseEnabled = true;
		UIContext.EventManager.LoseFocus += EventManagerOnLoseFocus;
		UIContext.EventManager.GainFocus += EventManagerOnGainFocus;
		UIContext.EventManager.OnGetIsHitThisFrame = GetIsHitThisFrame;
		UIContext.EventManager.UsableArea = base.UsableArea;
	}

	private void EventManagerOnLoseFocus()
	{
		if (!base.IsFocusLayer)
		{
			ScreenManager.TryLoseFocus(this);
		}
	}

	private void EventManagerOnGainFocus()
	{
		ScreenManager.TrySetFocus(this);
	}

	public IGauntletMovie LoadMovie(string movieName, ViewModel dataSource)
	{
		bool isUsingGeneratedPrefabs = UIConfig.GetIsUsingGeneratedPrefabs();
		bool isHotReloadEnabled = UIConfig.GetIsHotReloadEnabled();
		IGauntletMovie gauntletMovie = GauntletMovie.Load(UIContext, WidgetFactory, movieName, dataSource, !isUsingGeneratedPrefabs, isHotReloadEnabled);
		MoviesAndDataSources.Add(new Tuple<IGauntletMovie, ViewModel>(gauntletMovie, dataSource));
		return gauntletMovie;
	}

	public void ReleaseMovie(IGauntletMovie movie)
	{
		Tuple<IGauntletMovie, ViewModel> item = MoviesAndDataSources.SingleOrDefault((Tuple<IGauntletMovie, ViewModel> t) => t.Item1 == movie);
		MoviesAndDataSources.Remove(item);
		movie.Release();
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		TwoDimensionView.SetEnable(value: true);
	}

	protected override void OnDeactivate()
	{
		TwoDimensionView.Clear();
		TwoDimensionView.SetEnable(value: false);
		base.OnDeactivate();
	}

	protected override void Tick(float dt)
	{
		base.Tick(dt);
		TwoDimensionEnginePlatform.Reset();
		UIContext.Update(dt);
		foreach (Tuple<IGauntletMovie, ViewModel> moviesAndDataSource in MoviesAndDataSources)
		{
			moviesAndDataSource.Item1.Update();
		}
		base.ActiveCursor = (CursorType)UIContext.ActiveCursorOfContext;
	}

	protected override void LateTick(float dt)
	{
		base.LateTick(dt);
		TwoDimensionView.BeginFrame();
		UIContext.LateUpdate(dt);
		TwoDimensionView.EndFrame();
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		EngineInputService.UpdateInputDevices(base.KeyboardEnabled, base.MouseEnabled, base.GamepadEnabled);
	}

	protected override void Update(IReadOnlyList<int> lastKeysPressed)
	{
		UIContext.EventManager.FocusedWidget?.HandleInput(lastKeysPressed);
	}

	protected override void OnFinalize()
	{
		foreach (Tuple<IGauntletMovie, ViewModel> moviesAndDataSource in MoviesAndDataSources)
		{
			moviesAndDataSource.Item1.Release();
		}
		UIContext.EventManager.LoseFocus -= EventManagerOnLoseFocus;
		UIContext.EventManager.GainFocus -= EventManagerOnGainFocus;
		UIContext.EventManager.OnGetIsHitThisFrame = null;
		UIContext.OnFinalize();
		base.OnFinalize();
	}

	protected override void RefreshGlobalOrder(ref int currentOrder)
	{
		TwoDimensionView.SetRenderOrder(currentOrder);
		currentOrder++;
	}

	public override void ProcessEvents()
	{
		base.ProcessEvents();
		UIContext.UpdateInput(base._usedInputs);
	}

	public override bool HitTest(Vector2 position)
	{
		foreach (Tuple<IGauntletMovie, ViewModel> moviesAndDataSource in MoviesAndDataSources)
		{
			if (UIContext.HitTest(moviesAndDataSource.Item1.RootWidget, position))
			{
				return true;
			}
		}
		return false;
	}

	private bool GetIsBlockedAtPosition(Vector2 position)
	{
		return ScreenManager.IsLayerBlockedAtPosition(this, position);
	}

	public override bool HitTest()
	{
		foreach (Tuple<IGauntletMovie, ViewModel> moviesAndDataSource in MoviesAndDataSources)
		{
			if (UIContext.HitTest(moviesAndDataSource.Item1.RootWidget))
			{
				return true;
			}
		}
		UIContext.EventManager.SetHoveredView(null);
		return false;
	}

	public override bool FocusTest()
	{
		foreach (Tuple<IGauntletMovie, ViewModel> moviesAndDataSource in MoviesAndDataSources)
		{
			if (UIContext.FocusTest(moviesAndDataSource.Item1.RootWidget))
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsFocusedOnInput()
	{
		return UIContext.EventManager.FocusedWidget is EditableTextWidget;
	}

	protected override void OnLoseFocus()
	{
		UIContext.EventManager.ClearFocus();
	}

	public override void OnOnScreenKeyboardDone(string inputText)
	{
		base.OnOnScreenKeyboardDone(inputText);
		UIContext.OnOnScreenkeyboardTextInputDone(inputText);
	}

	public override void OnOnScreenKeyboardCanceled()
	{
		base.OnOnScreenKeyboardCanceled();
		UIContext.OnOnScreenKeyboardCanceled();
	}

	public override void UpdateLayout()
	{
		base.UpdateLayout();
		UIContext.ScaleModifier = base.Scale;
		UIContext.EventManager.UsableArea = base.UsableArea;
		MoviesAndDataSources.ForEach(delegate(Tuple<IGauntletMovie, ViewModel> m)
		{
			m.Item2.RefreshValues();
		});
		MoviesAndDataSources.ForEach(delegate(Tuple<IGauntletMovie, ViewModel> m)
		{
			m.Item1.RefreshBindingWithChildren();
		});
		UIContext.EventManager.UpdateLayout();
	}

	public bool GetIsAvailableForGamepadNavigation()
	{
		if (base.LastActiveState && base.IsActive && (base.MouseEnabled || base.GamepadEnabled))
		{
			if (!base.IsFocusLayer)
			{
				return (base.InputRestrictions.InputUsageMask & InputUsageMask.Mouse) != 0;
			}
			return true;
		}
		return false;
	}

	private bool GetIsHitThisFrame()
	{
		return base.IsHitThisFrame;
	}

	private int GetLastScreenOrder()
	{
		return base.ScreenOrderInLastFrame;
	}

	public override void DrawDebugInfo()
	{
		foreach (Tuple<IGauntletMovie, ViewModel> moviesAndDataSource in MoviesAndDataSources)
		{
			IGauntletMovie item = moviesAndDataSource.Item1;
			ViewModel item2 = moviesAndDataSource.Item2;
			Imgui.Text("Movie: " + item.MovieName);
			Imgui.Text("Data Source: " + (item2?.GetType().Name ?? "No Datasource"));
		}
		base.DrawDebugInfo();
		Imgui.Text("Press 'Shift+F' to take widget hierarchy snapshot.");
		UIContext.DrawWidgetDebugInfo();
	}
}
