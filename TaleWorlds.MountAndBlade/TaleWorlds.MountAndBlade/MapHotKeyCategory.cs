using System.Collections.Generic;
using TaleWorlds.Engine.Options;
using TaleWorlds.InputSystem;

namespace TaleWorlds.MountAndBlade;

public sealed class MapHotKeyCategory : GameKeyContext
{
	public const string CategoryId = "MapHotKeyCategory";

	public const int QuickSave = 53;

	public const int PartyMoveUp = 49;

	public const int PartyMoveLeft = 52;

	public const int PartyMoveDown = 50;

	public const int PartyMoveRight = 51;

	public const int MapMoveUp = 45;

	public const int MapMoveDown = 46;

	public const int MapMoveLeft = 48;

	public const int MapMoveRight = 47;

	public const string MovementAxisX = "MapMovementAxisX";

	public const string MovementAxisY = "MapMovementAxisY";

	public const int MapFastMove = 54;

	public const int MapZoomIn = 55;

	public const int MapZoomOut = 56;

	public const int MapRotateLeft = 57;

	public const int MapRotateRight = 58;

	public const int MapCameraFollowMode = 63;

	public const int MapToggleFastForward = 64;

	public const int MapTrackSettlement = 65;

	public const int MapGoToEncylopedia = 66;

	public const string MapClick = "MapClick";

	public const string MapFollowModifier = "MapFollowModifier";

	public const string MapChangeCursorMode = "MapChangeCursorMode";

	public const int MapTimeStop = 59;

	public const int MapTimeNormal = 60;

	public const int MapTimeFastForward = 61;

	public const int MapTimeTogglePause = 62;

	public MapHotKeyCategory()
		: base("MapHotKeyCategory", 108)
	{
		RegisterHotKeys();
		RegisterGameKeys();
		RegisterGameAxisKeys();
	}

	private void RegisterHotKeys()
	{
		List<Key> list = new List<Key>
		{
			new Key(InputKey.LeftMouseButton),
			new Key(InputKey.ControllerRDown)
		};
		if (NativeOptions.GetConfig(NativeOptions.NativeOptionsType.EnableTouchpadMouse) != 0f)
		{
			list.Add(new Key(InputKey.ControllerLOptionTap));
		}
		RegisterHotKey(new HotKey("MapClick", "MapHotKeyCategory", list));
		List<Key> keys = new List<Key>
		{
			new Key(InputKey.LeftAlt),
			new Key(InputKey.ControllerLBumper)
		};
		RegisterHotKey(new HotKey("MapFollowModifier", "MapHotKeyCategory", keys));
		List<Key> keys2 = new List<Key>
		{
			new Key(InputKey.ControllerRRight)
		};
		RegisterHotKey(new HotKey("MapChangeCursorMode", "MapHotKeyCategory", keys2));
	}

	private void RegisterGameKeys()
	{
		RegisterGameKey(new GameKey(49, "PartyMoveUp", "MapHotKeyCategory", InputKey.Up, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(50, "PartyMoveDown", "MapHotKeyCategory", InputKey.Down, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(51, "PartyMoveRight", "MapHotKeyCategory", InputKey.Right, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(52, "PartyMoveLeft", "MapHotKeyCategory", InputKey.Left, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(53, "QuickSave", "MapHotKeyCategory", InputKey.F5, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(54, "MapFastMove", "MapHotKeyCategory", InputKey.LeftShift, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(55, "MapZoomIn", "MapHotKeyCategory", InputKey.MouseScrollUp, InputKey.ControllerRTrigger, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(56, "MapZoomOut", "MapHotKeyCategory", InputKey.MouseScrollDown, InputKey.ControllerLTrigger, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(57, "MapRotateLeft", "MapHotKeyCategory", InputKey.Q, InputKey.Invalid, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(58, "MapRotateRight", "MapHotKeyCategory", InputKey.E, InputKey.Invalid, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(59, "MapTimeStop", "MapHotKeyCategory", InputKey.D1, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(60, "MapTimeNormal", "MapHotKeyCategory", InputKey.D2, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(61, "MapTimeFastForward", "MapHotKeyCategory", InputKey.D3, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(62, "MapTimeTogglePause", "MapHotKeyCategory", InputKey.Space, InputKey.ControllerRLeft, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(63, "MapCameraFollowMode", "MapHotKeyCategory", InputKey.Invalid, InputKey.ControllerLThumb, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(64, "MapToggleFastForward", "MapHotKeyCategory", InputKey.Invalid, InputKey.ControllerRBumper, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(65, "MapTrackSettlement", "MapHotKeyCategory", InputKey.Invalid, InputKey.ControllerRThumb, GameKeyMainCategories.CampaignMapCategory));
		RegisterGameKey(new GameKey(66, "MapGoToEncylopedia", "MapHotKeyCategory", InputKey.Invalid, InputKey.ControllerLOption, GameKeyMainCategories.CampaignMapCategory));
	}

	private void RegisterGameAxisKeys()
	{
		GameKey gameKey = new GameKey(45, "MapMoveUp", "MapHotKeyCategory", InputKey.W, GameKeyMainCategories.CampaignMapCategory);
		GameKey gameKey2 = new GameKey(46, "MapMoveDown", "MapHotKeyCategory", InputKey.S, GameKeyMainCategories.CampaignMapCategory);
		GameKey gameKey3 = new GameKey(47, "MapMoveRight", "MapHotKeyCategory", InputKey.D, GameKeyMainCategories.CampaignMapCategory);
		GameKey gameKey4 = new GameKey(48, "MapMoveLeft", "MapHotKeyCategory", InputKey.A, GameKeyMainCategories.CampaignMapCategory);
		RegisterGameKey(gameKey);
		RegisterGameKey(gameKey2);
		RegisterGameKey(gameKey4);
		RegisterGameKey(gameKey3);
		RegisterGameAxisKey(new GameAxisKey("MapMovementAxisX", InputKey.ControllerLStick, gameKey3, gameKey4));
		RegisterGameAxisKey(new GameAxisKey("MapMovementAxisY", InputKey.ControllerLStick, gameKey, gameKey2, GameAxisKey.AxisType.Y));
	}
}
