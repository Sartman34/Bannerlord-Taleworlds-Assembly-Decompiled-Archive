using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class LoadingWindowManager : GlobalLayer, ILoadingWindowManager
{
	private GauntletLayer _gauntletLayer;

	private LoadingWindowViewModel _loadingWindowViewModel;

	private SpriteCategory _sploadingCategory;

	private SpriteCategory _mpLoadingCategory;

	private SpriteCategory _mpBackgroundCategory;

	private bool _isMultiplayer;

	public LoadingWindowManager()
	{
		_sploadingCategory = UIResourceManager.SpriteData.SpriteCategories["ui_loading"];
		_sploadingCategory.InitializePartialLoad();
		_loadingWindowViewModel = new LoadingWindowViewModel(HandleSPPartialLoading);
		_loadingWindowViewModel.Enabled = false;
		_loadingWindowViewModel.SetTotalGenericImageCount(_sploadingCategory.SpriteSheetCount);
		bool shouldClear = false;
		_gauntletLayer = new GauntletLayer(100003, "GauntletLayer", shouldClear);
		_gauntletLayer.LoadMovie("LoadingWindow", _loadingWindowViewModel);
		base.Layer = _gauntletLayer;
		ScreenManager.AddGlobalLayer(this, isFocusable: false);
	}

	protected override void OnLateTick(float dt)
	{
		base.OnLateTick(dt);
		_loadingWindowViewModel.Update();
	}

	void ILoadingWindowManager.EnableLoadingWindow()
	{
		_loadingWindowViewModel.Enabled = true;
		base.Layer.IsFocusLayer = true;
		ScreenManager.TrySetFocus(base.Layer);
		base.Layer.InputRestrictions.SetInputRestrictions(isMouseVisible: false);
	}

	void ILoadingWindowManager.DisableLoadingWindow()
	{
		_loadingWindowViewModel.Enabled = false;
		base.Layer.IsFocusLayer = false;
		ScreenManager.TryLoseFocus(base.Layer);
		base.Layer.InputRestrictions.ResetInputRestrictions();
	}

	public void SetCurrentModeIsMultiplayer(bool isMultiplayer)
	{
		if (_isMultiplayer != isMultiplayer)
		{
			_isMultiplayer = isMultiplayer;
			_loadingWindowViewModel.IsMultiplayer = isMultiplayer;
			if (isMultiplayer)
			{
				_mpLoadingCategory = UIResourceManager.SpriteData.SpriteCategories["ui_mploading"];
				_mpLoadingCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);
				_mpBackgroundCategory = UIResourceManager.SpriteData.SpriteCategories["ui_mpbackgrounds"];
				_mpBackgroundCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);
			}
			else
			{
				_mpLoadingCategory.Unload();
				_mpBackgroundCategory.Unload();
			}
		}
	}

	private void HandleSPPartialLoading(bool isLoading, int index)
	{
		if (isLoading)
		{
			_sploadingCategory?.PartialLoadAtIndex(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot, index);
		}
		else
		{
			_sploadingCategory?.PartialUnloadAtIndex(index);
		}
	}
}
