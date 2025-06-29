using System.Collections.Generic;
using System.Linq;
using TaleWorlds.InputSystem;

namespace TaleWorlds.MountAndBlade;

public sealed class CraftingHotkeyCategory : GameKeyContext
{
	public const string CategoryId = "CraftingHotkeyCategory";

	public const string Zoom = "Zoom";

	public const string Rotate = "Rotate";

	public const string Ascend = "Ascend";

	public const string ResetCamera = "ResetCamera";

	public const string Copy = "Copy";

	public const string Paste = "Paste";

	public const string Exit = "Exit";

	public const string Confirm = "Confirm";

	public const string SwitchToPreviousTab = "SwitchToPreviousTab";

	public const string SwitchToNextTab = "SwitchToNextTab";

	public const string ControllerRotationAxisX = "CameraAxisX";

	public const string ControllerRotationAxisY = "CameraAxisY";

	public const int ControllerZoomIn = 55;

	public const int ControllerZoomOut = 56;

	public CraftingHotkeyCategory()
		: base("CraftingHotkeyCategory", 108)
	{
		RegisterHotKeys();
		RegisterGameKeys();
		RegisterGameAxisKeys();
	}

	private void RegisterHotKeys()
	{
		List<Key> keys = new List<Key>
		{
			new Key(InputKey.Escape),
			new Key(InputKey.ControllerRRight)
		};
		List<Key> keys2 = new List<Key>
		{
			new Key(InputKey.Space),
			new Key(InputKey.ControllerRLeft)
		};
		List<Key> keys3 = new List<Key>
		{
			new Key(InputKey.Q),
			new Key(InputKey.ControllerLBumper)
		};
		List<Key> keys4 = new List<Key>
		{
			new Key(InputKey.E),
			new Key(InputKey.ControllerRBumper)
		};
		RegisterHotKey(new HotKey("Exit", "CraftingHotkeyCategory", keys));
		RegisterHotKey(new HotKey("Confirm", "CraftingHotkeyCategory", keys2));
		RegisterHotKey(new HotKey("SwitchToPreviousTab", "CraftingHotkeyCategory", keys3));
		RegisterHotKey(new HotKey("SwitchToNextTab", "CraftingHotkeyCategory", keys4));
		RegisterHotKey(new HotKey("Ascend", "CraftingHotkeyCategory", InputKey.MiddleMouseButton));
		RegisterHotKey(new HotKey("Rotate", "CraftingHotkeyCategory", InputKey.LeftMouseButton));
		RegisterHotKey(new HotKey("Zoom", "CraftingHotkeyCategory", InputKey.RightMouseButton));
		RegisterHotKey(new HotKey("Copy", "CraftingHotkeyCategory", InputKey.C));
		RegisterHotKey(new HotKey("Paste", "CraftingHotkeyCategory", InputKey.V));
	}

	private void RegisterGameKeys()
	{
		RegisterGameKey(new GameKey(55, "ControllerZoomIn", "CraftingHotkeyCategory", InputKey.Invalid, InputKey.ControllerRTrigger));
		RegisterGameKey(new GameKey(56, "ControllerZoomOut", "CraftingHotkeyCategory", InputKey.Invalid, InputKey.ControllerLTrigger));
	}

	private void RegisterGameAxisKeys()
	{
		GameAxisKey gameKey = GenericGameKeyContext.Current.RegisteredGameAxisKeys.First((GameAxisKey g) => g.Id.Equals("CameraAxisX"));
		GameAxisKey gameKey2 = GenericGameKeyContext.Current.RegisteredGameAxisKeys.First((GameAxisKey g) => g.Id.Equals("CameraAxisY"));
		RegisterGameAxisKey(gameKey);
		RegisterGameAxisKey(gameKey2);
	}
}
