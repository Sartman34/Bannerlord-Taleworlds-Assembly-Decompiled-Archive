using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MissionMultiplayerFreeForAllUIHandler))]
public class MissionGauntletMultiplayerFFAView : MissionView
{
	private SpriteCategory _mpMissionCategory;

	public override void OnMissionScreenInitialize()
	{
		base.OnMissionScreenInitialize();
		ViewOrderPriority = 15;
		SpriteData spriteData = UIResourceManager.SpriteData;
		TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
		ResourceDepot uIResourceDepot = UIResourceManager.UIResourceDepot;
		_mpMissionCategory = spriteData.SpriteCategories["ui_mpmission"];
		_mpMissionCategory.Load(resourceContext, uIResourceDepot);
	}

	public override void OnMissionScreenFinalize()
	{
		base.OnMissionScreenFinalize();
		_mpMissionCategory?.Unload();
	}
}
