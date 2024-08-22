using System;

namespace TaleWorlds.Diamond.HelloWorld;

[Serializable]
public class HelloWorldTestFunctionResult : FunctionResult
{
	public string Message { get; private set; }

	public HelloWorldTestFunctionResult()
	{
	}

	public HelloWorldTestFunctionResult(string message)
	{
		Message = message;
	}
}
