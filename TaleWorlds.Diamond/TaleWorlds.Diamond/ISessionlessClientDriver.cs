using System.Threading.Tasks;

namespace TaleWorlds.Diamond;

public interface ISessionlessClientDriver
{
	void SendMessage(Message message);

	Task<T> CallFunction<T>(Message message) where T : FunctionResult;

	Task<bool> CheckConnection();
}
