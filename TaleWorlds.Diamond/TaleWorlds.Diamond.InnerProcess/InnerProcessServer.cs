using System.Collections.Generic;

namespace TaleWorlds.Diamond.InnerProcess;

public abstract class InnerProcessServer<T> : IInnerProcessServer where T : InnerProcessServerSession, new()
{
	private List<T> _clientSessions;

	public InnerProcessManager InnerProcessManager { get; private set; }

	public IEnumerable<T> Sessions => _clientSessions;

	protected InnerProcessServer(InnerProcessManager innerProcessManager)
	{
		InnerProcessManager = innerProcessManager;
		_clientSessions = new List<T>();
	}

	public void Host(int port)
	{
		InnerProcessManager.Activate(this, port);
	}

	InnerProcessServerSession IInnerProcessServer.AddNewConnection(IInnerProcessClient client)
	{
		T val = new T();
		_clientSessions.Add(val);
		return val;
	}

	void IInnerProcessServer.Update()
	{
		foreach (T clientSession in _clientSessions)
		{
			while (clientSession.HasMessage)
			{
				InnerProcessMessageTask innerProcessMessageTask = clientSession.DequeueMessage();
				SessionCredentials sessionCredentials = innerProcessMessageTask.SessionCredentials;
				Message message = innerProcessMessageTask.Message;
				switch (innerProcessMessageTask.Type)
				{
				case InnerProcessMessageTaskType.Login:
				{
					LoginResult loginResult = Login(clientSession, (LoginMessage)message, new InnerProcessConnectionInformation());
					if (loginResult.Successful)
					{
						innerProcessMessageTask.SetFinishedAsSuccessful(loginResult);
					}
					break;
				}
				case InnerProcessMessageTaskType.Message:
					HandleMessage(clientSession, sessionCredentials, message);
					innerProcessMessageTask.SetFinishedAsSuccessful(null);
					break;
				case InnerProcessMessageTaskType.Function:
				{
					(HandlerResult, FunctionResult) tuple = CallFunction(clientSession, sessionCredentials, message);
					if (tuple.Item1.IsSuccessful)
					{
						innerProcessMessageTask.SetFinishedAsSuccessful(tuple.Item2);
					}
					break;
				}
				}
			}
		}
		OnUpdate();
	}

	protected abstract void HandleMessage(T serverSession, SessionCredentials sessionCredentials, Message message);

	protected abstract (HandlerResult, FunctionResult) CallFunction(T serverSession, SessionCredentials sessionCredentials, Message message);

	protected abstract LoginResult Login(T serverSession, LoginMessage message, InnerProcessConnectionInformation connectionInformation);

	protected virtual void OnUpdate()
	{
	}
}
