using TaleWorlds.Diamond.InnerProcess;

namespace TaleWorlds.Diamond.ClientApplication;

public class GenericInnerProcessSessionProvider<T> : IClientSessionProvider<T> where T : Client<T>
{
	private InnerProcessManager _innerProcessManager;

	private ushort _port;

	public GenericInnerProcessSessionProvider(InnerProcessManager innerProcessManager, ushort port)
	{
		_innerProcessManager = innerProcessManager;
		_port = port;
	}

	public IClientSession CreateSession(T session)
	{
		return new InnerProcessClient(_innerProcessManager, session, _port);
	}
}
