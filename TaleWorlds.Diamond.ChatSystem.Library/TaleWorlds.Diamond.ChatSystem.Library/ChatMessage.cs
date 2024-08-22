using System;

namespace TaleWorlds.Diamond.ChatSystem.Library;

public class ChatMessage
{
	public string Name { get; set; }

	public Guid RoomId { get; set; }

	public string Text { get; set; }

	public MessageType Type { get; set; }
}
