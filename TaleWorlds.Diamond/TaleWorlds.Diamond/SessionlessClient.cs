using System.Threading.Tasks;
using TaleWorlds.Diamond.ClientApplication;

namespace TaleWorlds.Diamond;

public abstract class SessionlessClient<T> : DiamondClientApplicationObject, ISessionlessClient where T : SessionlessClient<T>
{
	private ISessionlessClientDriver _clientDriver;

	protected SessionlessClient(DiamondClientApplication diamondClientApplication, ISessionlessClientDriverProvider<T> driverProvider)
		: base(diamondClientApplication)
	{
		_clientDriver = driverProvider.CreateDriver((T)this);
	}

	protected void SendMessage(Message message)
	{
		_clientDriver.SendMessage(message);
	}

	protected async Task<TResult> CallFunction<TResult>(Message message) where TResult : FunctionResult
	{
		return await _clientDriver.CallFunction<TResult>(message);
	}

	public Task<bool> CheckConnection()
	{
		return _clientDriver.CheckConnection();
	}
}
