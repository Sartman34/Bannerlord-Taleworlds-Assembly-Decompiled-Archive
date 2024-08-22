namespace TaleWorlds.Diamond.InnerProcess;

internal class InnerProcessConnectionRequest
{
	public IInnerProcessClient Client { get; private set; }

	public int Port { get; private set; }

	public InnerProcessConnectionRequest(IInnerProcessClient client, int port)
	{
		Client = client;
		Port = port;
	}
}
