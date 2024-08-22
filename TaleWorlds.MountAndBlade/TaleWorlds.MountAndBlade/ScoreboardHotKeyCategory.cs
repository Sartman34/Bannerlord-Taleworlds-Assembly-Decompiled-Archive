using System.Collections.Generic;
using TaleWorlds.InputSystem;

namespace TaleWorlds.MountAndBlade;

public sealed class ScoreboardHotKeyCategory : GameKeyContext
{
	public const string CategoryId = "ScoreboardHotKeyCategory";

	public const int ShowMouse = 35;

	public const string HoldShow = "HoldShow";

	public const string ToggleFastForward = "ToggleFastForward";

	public const string MenuShowContextMenu = "MenuShowContextMenu";

	public ScoreboardHotKeyCategory()
		: base("ScoreboardHotKeyCategory", 108)
	{
		RegisterHotKeys();
		RegisterGameKeys();
		RegisterGameAxisKeys();
	}

	private void RegisterHotKeys()
	{
		List<Key> keys = new List<Key>
		{
			new Key(InputKey.F),
			new Key(InputKey.ControllerRUp)
		};
		RegisterHotKey(new HotKey("ToggleFastForward", "ScoreboardHotKeyCategory", keys));
		RegisterHotKey(new HotKey("MenuShowContextMenu", "ScoreboardHotKeyCategory", InputKey.RightMouseButton));
		List<Key> list = new List<Key>();
		list.Add(new Key(InputKey.Tab));
		list.Add(new Key(InputKey.ControllerRRight));
		RegisterHotKey(new HotKey("HoldShow", "ScoreboardHotKeyCategory", list));
	}

	private void RegisterGameKeys()
	{
		RegisterGameKey(new GameKey(35, "ShowMouse", "ScoreboardHotKeyCategory", InputKey.MiddleMouseButton, InputKey.ControllerLThumb, GameKeyMainCategories.ActionCategory));
	}

	private void RegisterGameAxisKeys()
	{
	}
}
