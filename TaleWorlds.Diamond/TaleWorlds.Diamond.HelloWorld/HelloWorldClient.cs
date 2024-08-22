using System.Threading.Tasks;
using TaleWorlds.Diamond.ClientApplication;

namespace TaleWorlds.Diamond.HelloWorld;

public class HelloWorldClient : SessionlessClient<HelloWorldClient>
{
	public HelloWorldClient(DiamondClientApplication diamondClientApplication, ISessionlessClientDriverProvider<HelloWorldClient> driverProvider)
		: base(diamondClientApplication, driverProvider)
	{
	}

	public void SendTestMessage(HelloWorldTestMessage message)
	{
		SendMessage(message);
	}

	public async Task<HelloWorldTestFunctionResult> CallTestFunction(HelloWorldTestFunctionMessage message)
	{
		return await CallFunction<HelloWorldTestFunctionResult>(message);
	}
}
