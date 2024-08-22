using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.SceneNotification;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletGameNotification : GlobalLayer
{
	private GameNotificationVM _dataSource;

	private GauntletLayer _layer;

	private IGauntletMovie _movie;

	private bool _isSuspended;

	public static GauntletGameNotification Current { get; private set; }

	private GauntletGameNotification()
	{
		_dataSource = new GameNotificationVM();
		_dataSource.ReceiveNewNotification += OnReceiveNewNotification;
		_layer = new GauntletLayer(4007);
		LoadMovie(forMultiplayer: false);
		base.Layer = _layer;
		_layer.InputRestrictions.SetInputRestrictions(isMouseVisible: false, InputUsageMask.Mouse);
	}

	private void OnReceiveNewNotification(GameNotificationItemVM notification)
	{
		if (!string.IsNullOrEmpty(notification.NotificationSoundId))
		{
			SoundEvent.PlaySound2D(notification.NotificationSoundId);
		}
	}

	public static void Initialize()
	{
		if (Current == null)
		{
			Current = new GauntletGameNotification();
			ScreenManager.AddGlobalLayer(Current, isFocusable: false);
		}
	}

	public static void OnFinalize()
	{
		Current?._dataSource?.ClearNotifications();
	}

	public void LoadMovie(bool forMultiplayer)
	{
		if (_movie != null)
		{
			_layer.ReleaseMovie(_movie);
		}
		if (forMultiplayer)
		{
			_movie = _layer.LoadMovie("MultiplayerGameNotificationUI", _dataSource);
		}
		else
		{
			_movie = _layer.LoadMovie("GameNotificationUI", _dataSource);
		}
	}

	protected override void OnTick(float dt)
	{
		base.OnTick(dt);
		bool isLoadingWindowActive = LoadingWindow.IsLoadingWindowActive;
		bool isActive = GauntletSceneNotification.Current.IsActive;
		if (isActive != _isSuspended)
		{
			ScreenManager.SetSuspendLayer(Current._layer, isActive);
			_isSuspended = isActive;
		}
		if (isLoadingWindowActive)
		{
			dt = 0f;
		}
		if (!_isSuspended)
		{
			_dataSource.Tick(dt);
		}
	}
}
