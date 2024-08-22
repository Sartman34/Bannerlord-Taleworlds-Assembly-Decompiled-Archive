using System;

namespace TaleWorlds.Diamond.HelloWorld;

[Serializable]
public class HelloWorldTestMessage : Message
{
	public string Message { get; private set; }

	public HelloWorldTestMessage()
	{
	}

	public HelloWorldTestMessage(string message)
	{
		Message = message;
	}
}
