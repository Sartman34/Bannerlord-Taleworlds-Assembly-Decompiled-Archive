using System.Collections.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.FormationMarker;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(MissionFormationMarkerUIHandler))]
public class MissionGauntletFormationMarker : MissionGauntletBattleUIBase
{
	private MissionFormationMarkerVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private List<CompassItemUpdateParams> _formationTargets;

	private MBReadOnlyList<Formation> _focusedFormationsCache;

	private MissionGauntletSingleplayerOrderUIHandler _orderHandler;

	private MissionFormationTargetSelectionHandler _formationTargetHandler;

	protected override void OnCreateView()
	{
		_formationTargets = new List<CompassItemUpdateParams>();
		_dataSource = new MissionFormationMarkerVM(base.Mission, base.MissionScreen.CombatCamera);
		_gauntletLayer = new GauntletLayer(ViewOrderPriority);
		_gauntletLayer.LoadMovie("FormationMarker", _dataSource);
		base.MissionScreen.AddLayer(_gauntletLayer);
		_orderHandler = base.Mission.GetMissionBehavior<MissionGauntletSingleplayerOrderUIHandler>();
		_formationTargetHandler = base.Mission.GetMissionBehavior<MissionFormationTargetSelectionHandler>();
		if (_formationTargetHandler != null)
		{
			_formationTargetHandler.OnFormationFocused += OnFormationFocusedFromHandler;
		}
	}

	protected override void OnDestroyView()
	{
		if (_formationTargetHandler != null)
		{
			_formationTargetHandler.OnFormationFocused -= OnFormationFocusedFromHandler;
		}
		base.MissionScreen.RemoveLayer(_gauntletLayer);
		_gauntletLayer = null;
		_dataSource.OnFinalize();
		_dataSource = null;
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		if (!base.IsViewActive)
		{
			return;
		}
		if (!_orderHandler.IsBattleDeployment)
		{
			_dataSource.IsEnabled = base.Input.IsGameKeyDown(5) || (_orderHandler?.IsOrderMenuActive ?? false);
			if (_formationTargetHandler != null)
			{
				_dataSource.SetFocusedFormations(_focusedFormationsCache);
			}
		}
		_dataSource.IsFormationTargetRelevant = _formationTargetHandler != null && (_orderHandler?.IsOrderMenuActive ?? false);
		_dataSource.Tick(dt);
	}

	private void OnFormationFocusedFromHandler(MBReadOnlyList<Formation> obj)
	{
		_focusedFormationsCache = obj;
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
