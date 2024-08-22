using System.Collections.Generic;

namespace TaleWorlds.Diamond.InnerProcess;

public class InnerProcessManager
{
	private Dictionary<int, IInnerProcessServer> _activeServers;

	private List<InnerProcessConnectionRequest> _connectionRequests;

	public InnerProcessManager()
	{
		_activeServers = new Dictionary<int, IInnerProcessServer>();
		_connectionRequests = new List<InnerProcessConnectionRequest>();
	}

	internal void Activate(IInnerProcessServer server, int port)
	{
		_activeServers.Add(port, server);
	}

	internal void RequestConnection(IInnerProcessClient client, int port)
	{
		_connectionRequests.Add(new InnerProcessConnectionRequest(client, port));
	}

	public void Update()
	{
		for (int i = 0; i < _connectionRequests.Count; i++)
		{
			InnerProcessConnectionRequest innerProcessConnectionRequest = _connectionRequests[i];
			IInnerProcessClient client = innerProcessConnectionRequest.Client;
			int port = innerProcessConnectionRequest.Port;
			if (_activeServers.TryGetValue(port, out var value))
			{
				_connectionRequests.RemoveAt(i);
				i--;
				InnerProcessServerSession innerProcessServerSession = value.AddNewConnection(client);
				client.HandleConnected(innerProcessServerSession);
				innerProcessServerSession.HandleConnected(client);
			}
		}
		foreach (IInnerProcessServer value2 in _activeServers.Values)
		{
			value2.Update();
		}
	}
}
