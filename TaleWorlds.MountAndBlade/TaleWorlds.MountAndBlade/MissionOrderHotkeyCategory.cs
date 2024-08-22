using TaleWorlds.InputSystem;

namespace TaleWorlds.MountAndBlade;

public sealed class MissionOrderHotkeyCategory : GameKeyContext
{
	public const string CategoryId = "MissionOrderHotkeyCategory";

	public const int ViewOrders = 67;

	public const int SelectOrder1 = 68;

	public const int SelectOrder2 = 69;

	public const int SelectOrder3 = 70;

	public const int SelectOrder4 = 71;

	public const int SelectOrder5 = 72;

	public const int SelectOrder6 = 73;

	public const int SelectOrder7 = 74;

	public const int SelectOrder8 = 75;

	public const int SelectOrderReturn = 76;

	public const int EveryoneHear = 77;

	public const int Group0Hear = 78;

	public const int Group1Hear = 79;

	public const int Group2Hear = 80;

	public const int Group3Hear = 81;

	public const int Group4Hear = 82;

	public const int Group5Hear = 83;

	public const int Group6Hear = 84;

	public const int Group7Hear = 85;

	public const int HoldOrder = 86;

	public const int SelectNextGroup = 87;

	public const int SelectPreviousGroup = 88;

	public const int ToggleGroupSelection = 89;

	public MissionOrderHotkeyCategory()
		: base("MissionOrderHotkeyCategory", 108)
	{
		RegisterHotKeys();
		RegisterGameKeys();
		RegisterGameAxisKeys();
	}

	private void RegisterHotKeys()
	{
	}

	private void RegisterGameKeys()
	{
		RegisterGameKey(new GameKey(67, "ViewOrders", "MissionOrderHotkeyCategory", InputKey.BackSpace, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(68, "SelectOrder1", "MissionOrderHotkeyCategory", InputKey.F1, InputKey.ControllerRLeft, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(69, "SelectOrder2", "MissionOrderHotkeyCategory", InputKey.F2, InputKey.ControllerRDown, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(70, "SelectOrder3", "MissionOrderHotkeyCategory", InputKey.F3, InputKey.ControllerRRight, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(71, "SelectOrder4", "MissionOrderHotkeyCategory", InputKey.F4, InputKey.ControllerRUp, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(72, "SelectOrder5", "MissionOrderHotkeyCategory", InputKey.F5, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(73, "SelectOrder6", "MissionOrderHotkeyCategory", InputKey.F6, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(74, "SelectOrder7", "MissionOrderHotkeyCategory", InputKey.F7, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(75, "SelectOrder8", "MissionOrderHotkeyCategory", InputKey.F8, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(76, "SelectOrderReturn", "MissionOrderHotkeyCategory", InputKey.F9, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(77, "EveryoneHear", "MissionOrderHotkeyCategory", InputKey.D0, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(78, "Group0Hear", "MissionOrderHotkeyCategory", InputKey.D1, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(79, "Group1Hear", "MissionOrderHotkeyCategory", InputKey.D2, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(80, "Group2Hear", "MissionOrderHotkeyCategory", InputKey.D3, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(81, "Group3Hear", "MissionOrderHotkeyCategory", InputKey.D4, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(82, "Group4Hear", "MissionOrderHotkeyCategory", InputKey.D5, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(83, "Group5Hear", "MissionOrderHotkeyCategory", InputKey.D6, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(84, "Group6Hear", "MissionOrderHotkeyCategory", InputKey.D7, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(85, "Group7Hear", "MissionOrderHotkeyCategory", InputKey.D8, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(86, "HoldOrder", "MissionOrderHotkeyCategory", InputKey.Invalid, InputKey.ControllerLBumper, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(87, "SelectNextGroup", "MissionOrderHotkeyCategory", InputKey.Invalid, InputKey.ControllerLRight, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(88, "SelectPreviousGroup", "MissionOrderHotkeyCategory", InputKey.Invalid, InputKey.ControllerLLeft, GameKeyMainCategories.OrderMenuCategory));
		RegisterGameKey(new GameKey(89, "ToggleGroupSelection", "MissionOrderHotkeyCategory", InputKey.Invalid, InputKey.ControllerLDown, GameKeyMainCategories.OrderMenuCategory));
	}

	private void RegisterGameAxisKeys()
	{
	}
}
