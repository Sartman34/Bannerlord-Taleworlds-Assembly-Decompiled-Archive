using TaleWorlds.MountAndBlade.View.MissionViews;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

public abstract class MissionGauntletBattleUIBase : MissionView
{
	protected bool IsViewActive { get; private set; }

	protected abstract void OnCreateView();

	protected abstract void OnDestroyView();

	private void OnEnableView()
	{
		OnCreateView();
		IsViewActive = true;
	}

	private void OnDisableView()
	{
		OnDestroyView();
		IsViewActive = false;
	}

	public override void OnMissionScreenInitialize()
	{
		base.OnMissionScreenInitialize();
		if (GameNetwork.IsMultiplayer)
		{
			OnEnableView();
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		if (!GameNetwork.IsMultiplayer && !MBCommon.IsPaused)
		{
			if (!IsViewActive && !BannerlordConfig.HideBattleUI)
			{
				OnEnableView();
			}
			else if (IsViewActive && BannerlordConfig.HideBattleUI)
			{
				OnDisableView();
			}
		}
	}

	public override void OnMissionScreenFinalize()
	{
		base.OnMissionScreenFinalize();
		if (IsViewActive)
		{
			OnDisableView();
		}
	}
}
