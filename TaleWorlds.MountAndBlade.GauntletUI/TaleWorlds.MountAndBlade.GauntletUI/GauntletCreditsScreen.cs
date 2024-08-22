using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection.Credits;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI;

[OverrideView(typeof(CreditsScreen))]
public class GauntletCreditsScreen : ScreenBase
{
	private CreditsVM _datasource;

	private GauntletLayer _gauntletLayer;

	private IGauntletMovie _movie;

	private SpriteCategory _creditsCategory;

	protected override void OnInitialize()
	{
		base.OnInitialize();
		SpriteData spriteData = UIResourceManager.SpriteData;
		TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
		ResourceDepot uIResourceDepot = UIResourceManager.UIResourceDepot;
		_creditsCategory = spriteData.SpriteCategories["ui_credits"];
		_creditsCategory.Load(resourceContext, uIResourceDepot);
		_datasource = new CreditsVM();
		string path = string.Concat(ModuleHelper.GetModuleFullPath("Native") + "ModuleData/", "Credits.xml");
		_datasource.FillFromFile(path);
		_gauntletLayer = new GauntletLayer(100);
		_gauntletLayer.IsFocusLayer = true;
		AddLayer(_gauntletLayer);
		_gauntletLayer.InputRestrictions.SetInputRestrictions();
		ScreenManager.TrySetFocus(_gauntletLayer);
		_movie = _gauntletLayer.LoadMovie("CreditsScreen", _datasource);
		_gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		_creditsCategory.Unload();
		_datasource.OnFinalize();
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		if (_gauntletLayer.Input.IsHotKeyPressed("Exit"))
		{
			ScreenManager.PopScreen();
		}
	}
}
