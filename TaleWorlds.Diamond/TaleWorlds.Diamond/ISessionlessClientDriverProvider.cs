namespace TaleWorlds.Diamond;

public interface ISessionlessClientDriverProvider<T> where T : SessionlessClient<T>
{
	ISessionlessClientDriver CreateDriver(T client);
}
