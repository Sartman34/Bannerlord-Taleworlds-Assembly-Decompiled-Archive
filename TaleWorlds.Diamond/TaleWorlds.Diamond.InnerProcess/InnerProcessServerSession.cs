using System.Collections.Generic;

namespace TaleWorlds.Diamond.InnerProcess;

public class InnerProcessServerSession
{
	private Queue<InnerProcessMessageTask> _messageTasks;

	private IInnerProcessClient _associatedClientSession;

	internal bool HasMessage => _messageTasks.Count > 0;

	public PeerId PeerId { get; private set; }

	public SessionKey SessionKey { get; private set; }

	public InnerProcessServerSession()
	{
		_messageTasks = new Queue<InnerProcessMessageTask>();
	}

	public void SendMessage(Message message)
	{
		_associatedClientSession.EnqueueMessage(message);
	}

	internal void EnqueueMessageTask(InnerProcessMessageTask messageTask)
	{
		_messageTasks.Enqueue(messageTask);
	}

	internal InnerProcessMessageTask DequeueMessage()
	{
		InnerProcessMessageTask result = null;
		if (_messageTasks.Count > 0)
		{
			result = _messageTasks.Dequeue();
		}
		return result;
	}

	internal void HandleConnected(IInnerProcessClient clientSession)
	{
		_associatedClientSession = clientSession;
	}

	public void AssignSession(PeerId peerId, SessionKey sessionKey)
	{
		PeerId = peerId;
		SessionKey = sessionKey;
	}
}
