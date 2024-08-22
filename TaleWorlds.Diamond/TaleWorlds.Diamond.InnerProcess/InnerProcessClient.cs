using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaleWorlds.Diamond.InnerProcess;

public class InnerProcessClient : IClientSession, IInnerProcessClient
{
	private InnerProcessServerSession _associatedServerSession;

	private Queue<Message> _receivedMessages;

	private SessionCredentials _sessionCredentials;

	private IClient _client;

	private int _port;

	public InnerProcessManager InnerProcessManager { get; private set; }

	internal bool HasMessage => _receivedMessages.Count > 0;

	public InnerProcessClient(InnerProcessManager innerProcessManager, IClient client, int port)
	{
		InnerProcessManager = innerProcessManager;
		_receivedMessages = new Queue<Message>();
		_associatedServerSession = null;
		_client = client;
		_port = port;
	}

	void IClientSession.Connect()
	{
		InnerProcessManager.RequestConnection(this, _port);
	}

	void IClientSession.Disconnect()
	{
	}

	void IClientSession.Tick()
	{
		while (HasMessage)
		{
			Message message = DequeueMessage();
			HandleMessage(message);
		}
	}

	void IInnerProcessClient.EnqueueMessage(Message message)
	{
		_receivedMessages.Enqueue(message);
	}

	internal Message DequeueMessage()
	{
		Message result = null;
		if (_receivedMessages.Count > 0)
		{
			result = _receivedMessages.Dequeue();
		}
		return result;
	}

	async Task<LoginResult> IClientSession.Login(LoginMessage message)
	{
		InnerProcessMessageTask innerProcessMessageTask = new InnerProcessMessageTask(_sessionCredentials, message, InnerProcessMessageTaskType.Login);
		_associatedServerSession.EnqueueMessageTask(innerProcessMessageTask);
		await innerProcessMessageTask.WaitUntilFinished();
		LoginResult loginResult = (LoginResult)innerProcessMessageTask.FunctionResult;
		_sessionCredentials = new SessionCredentials(loginResult.PeerId, loginResult.SessionKey);
		return loginResult;
	}

	void IClientSession.SendMessage(Message message)
	{
		InnerProcessMessageTask messageTask = new InnerProcessMessageTask(_sessionCredentials, message, InnerProcessMessageTaskType.Message);
		_associatedServerSession.EnqueueMessageTask(messageTask);
	}

	async Task<TResult> IClientSession.CallFunction<TResult>(Message message)
	{
		InnerProcessMessageTask innerProcessMessageTask = new InnerProcessMessageTask(_sessionCredentials, message, InnerProcessMessageTaskType.Function);
		_associatedServerSession.EnqueueMessageTask(innerProcessMessageTask);
		await innerProcessMessageTask.WaitUntilFinished();
		return (TResult)innerProcessMessageTask.FunctionResult;
	}

	void IInnerProcessClient.HandleConnected(InnerProcessServerSession serverSession)
	{
		_associatedServerSession = serverSession;
		OnConnected();
	}

	private void HandleMessage(Message message)
	{
		_client.HandleMessage(message);
	}

	private void OnConnected()
	{
		_client.OnConnected();
	}

	private void OnCantConnect()
	{
		_client.OnCantConnect();
	}

	private void OnDisconnected()
	{
		_client.OnDisconnected();
	}

	public Task<bool> CheckConnection()
	{
		return _client.CheckConnection();
	}
}
