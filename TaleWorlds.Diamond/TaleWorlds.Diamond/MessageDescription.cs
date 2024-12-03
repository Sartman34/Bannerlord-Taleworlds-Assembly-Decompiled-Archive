using System;

namespace TaleWorlds.Diamond;

public class MessageDescription : Attribute
{
	public string To { get; private set; }

	public string From { get; private set; }

	public MessageDescription(string from, string to)
	{
		From = from;
		To = to;
	}
}
