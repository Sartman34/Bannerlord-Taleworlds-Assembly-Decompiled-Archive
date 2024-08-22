using System.Collections.Generic;
using TaleWorlds.InputSystem;

namespace TaleWorlds.MountAndBlade;

public sealed class ConversationHotKeyCategory : GameKeyContext
{
	public const string CategoryId = "ConversationHotKeyCategory";

	public const string ContinueKey = "ContinueKey";

	public const string ContinueClick = "ContinueClick";

	public ConversationHotKeyCategory()
		: base("ConversationHotKeyCategory", 108)
	{
		RegisterHotKeys();
		RegisterGameKeys();
		RegisterGameAxisKeys();
	}

	private void RegisterHotKeys()
	{
		List<Key> keys = new List<Key>
		{
			new Key(InputKey.Space),
			new Key(InputKey.Enter),
			new Key(InputKey.NumpadEnter),
			new Key(InputKey.ControllerRDown)
		};
		RegisterHotKey(new HotKey("ContinueKey", "ConversationHotKeyCategory", keys));
		RegisterHotKey(new HotKey("ContinueClick", "ConversationHotKeyCategory", InputKey.LeftMouseButton));
	}

	private void RegisterGameKeys()
	{
	}

	private void RegisterGameAxisKeys()
	{
	}
}
