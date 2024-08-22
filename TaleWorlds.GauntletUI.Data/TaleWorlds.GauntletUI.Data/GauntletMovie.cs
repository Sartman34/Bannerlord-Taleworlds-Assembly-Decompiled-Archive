using System.Collections.Generic;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.Library;

namespace TaleWorlds.GauntletUI.Data;

public class GauntletMovie : IGauntletMovie
{
	private WidgetPrefab _moviePrefab;

	private IViewModel _viewModel;

	private Widget _movieRootNode;

	public WidgetFactory WidgetFactory { get; private set; }

	public BrushFactory BrushFactory { get; private set; }

	public UIContext Context { get; private set; }

	public IViewModel ViewModel => _viewModel;

	public string MovieName { get; private set; }

	public GauntletView RootView { get; private set; }

	public Widget RootWidget => RootView.Target;

	public bool IsReleased { get; private set; }

	private GauntletMovie(string movieName, UIContext context, WidgetFactory widgetFactory, IViewModel viewModel, bool hotReloadEnabled)
	{
		WidgetFactory = widgetFactory;
		BrushFactory = context.BrushFactory;
		Context = context;
		if (hotReloadEnabled)
		{
			WidgetFactory.PrefabChange += OnResourceChanged;
			BrushFactory.BrushChange += OnResourceChanged;
		}
		_viewModel = viewModel;
		MovieName = movieName;
		_movieRootNode = new Widget(Context);
		Context.Root.AddChild(_movieRootNode);
		_movieRootNode.WidthSizePolicy = SizePolicy.Fixed;
		_movieRootNode.HeightSizePolicy = SizePolicy.Fixed;
		_movieRootNode.ScaledSuggestedWidth = Context.TwoDimensionContext.Width;
		_movieRootNode.ScaledSuggestedHeight = Context.TwoDimensionContext.Height;
		_movieRootNode.DoNotAcceptEvents = true;
		IsReleased = false;
	}

	public void RefreshDataSource(IViewModel dataSourve)
	{
		_viewModel = dataSourve;
		RootView.RefreshBindingWithChildren();
	}

	private void OnResourceChanged()
	{
		RootView.ClearEventHandlersWithChildren();
		RootView = null;
		_movieRootNode.RemoveAllChildren();
		Context.OnMovieReleased(MovieName);
		LoadMovie();
	}

	private void LoadMovie()
	{
		_moviePrefab = WidgetFactory.GetCustomType(MovieName);
		WidgetCreationData widgetCreationData = new WidgetCreationData(Context, WidgetFactory);
		widgetCreationData.AddExtensionData(this);
		WidgetInstantiationResult widgetInstantiationResult = _moviePrefab.Instantiate(widgetCreationData);
		RootView = widgetInstantiationResult.GetGauntletView();
		Widget target = RootView.Target;
		_movieRootNode.AddChild(target);
		RootView.RefreshBindingWithChildren();
		Context.OnMovieLoaded(MovieName);
	}

	public void Release()
	{
		IsReleased = true;
		_movieRootNode.OnBeforeRemovedChild(_movieRootNode);
		RootView?.ReleaseBindingWithChildren();
		_moviePrefab.OnRelease();
		WidgetFactory.OnUnload(MovieName);
		WidgetFactory.PrefabChange -= OnResourceChanged;
		BrushFactory.BrushChange -= OnResourceChanged;
		Context.OnMovieReleased(MovieName);
		_movieRootNode.ParentWidget = null;
	}

	internal void OnItemRemoved(string type)
	{
		WidgetFactory.OnUnload(type);
	}

	public void Update()
	{
		_movieRootNode.ScaledSuggestedWidth = Context.TwoDimensionContext.Width;
		_movieRootNode.ScaledSuggestedHeight = Context.TwoDimensionContext.Height;
	}

	internal object GetViewModelAtPath(BindingPath path, bool isListExpected)
	{
		if (_viewModel != null && path != null)
		{
			BindingPath path2 = path.Simplify();
			return _viewModel.GetViewModelAtPath(path2, isListExpected);
		}
		return null;
	}

	public static IGauntletMovie Load(UIContext context, WidgetFactory widgetFactory, string movieName, IViewModel datasource, bool doNotUseGeneratedPrefabs, bool hotReloadEnabled)
	{
		IGauntletMovie gauntletMovie = null;
		if (!doNotUseGeneratedPrefabs)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			string variantName = "Default";
			if (datasource != null)
			{
				dictionary.Add("DataSource", datasource);
				variantName = datasource.GetType().FullName;
			}
			GeneratedPrefabInstantiationResult generatedPrefabInstantiationResult = widgetFactory.GeneratedPrefabContext.InstantiatePrefab(context, movieName, variantName, dictionary);
			if (generatedPrefabInstantiationResult != null)
			{
				gauntletMovie = generatedPrefabInstantiationResult.GetExtensionData("Movie") as IGauntletMovie;
				context.OnMovieLoaded(movieName);
			}
		}
		if (gauntletMovie == null)
		{
			GauntletMovie gauntletMovie2 = new GauntletMovie(movieName, context, widgetFactory, datasource, hotReloadEnabled);
			gauntletMovie2.LoadMovie();
			gauntletMovie = gauntletMovie2;
		}
		return gauntletMovie;
	}

	public void RefreshBindingWithChildren()
	{
		RootView.RefreshBindingWithChildren();
	}

	public GauntletView FindViewOf(Widget widget)
	{
		return widget.GetComponent<GauntletView>();
	}
}
