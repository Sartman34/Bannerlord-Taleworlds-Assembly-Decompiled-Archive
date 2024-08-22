namespace TaleWorlds.Diamond.InnerProcess;

public interface IInnerProcessServer
{
	InnerProcessServerSession AddNewConnection(IInnerProcessClient client);

	void Update();
}
