using System;
using System.Threading.Tasks;

namespace TaleWorlds.Library;

public interface ICache
{
	TItem GetOrUpdate<TItem>(string key, Func<Task<TItem>> factory, TimeSpan absoluteExpirationRelativeToNow);
}
