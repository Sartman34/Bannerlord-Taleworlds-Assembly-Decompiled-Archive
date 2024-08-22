using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

[OverrideView(typeof(MissionBoundaryCrossingView))]
public class MissionGauntletBoundaryCrossingView : MissionGauntletBattleUIBase
{
	private GauntletLayer _gauntletLayer;

	private BoundaryCrossingVM _dataSource;

	protected override void OnCreateView()
	{
		_dataSource = new BoundaryCrossingVM(base.Mission, OnEscapeMenuToggled);
		_gauntletLayer = new GauntletLayer(47);
		_gauntletLayer.LoadMovie("BoundaryCrossing", _dataSource);
		base.MissionScreen.AddLayer(_gauntletLayer);
	}

	protected override void OnDestroyView()
	{
		_gauntletLayer = null;
		_dataSource.OnFinalize();
		_dataSource = null;
	}

	private void OnEscapeMenuToggled(bool isOpened)
	{
		if (base.IsViewActive)
		{
			ScreenManager.SetSuspendLayer(_gauntletLayer, !isOpened);
		}
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		if (base.IsViewActive)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		if (base.IsViewActive)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}
}
