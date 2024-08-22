using System.Threading.Tasks;

namespace TaleWorlds.Diamond;

public interface ISessionlessClient
{
	Task<bool> CheckConnection();
}
