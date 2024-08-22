namespace TaleWorlds.Diamond.InnerProcess;

public interface IInnerProcessClient
{
	void EnqueueMessage(Message message);

	void HandleConnected(InnerProcessServerSession serverSession);
}
