using System;

namespace TaleWorlds.Library;

public struct TWSharedMutexUpgradeableReadLock : IDisposable
{
	private readonly TWSharedMutex _mtx;

	public TWSharedMutexUpgradeableReadLock(TWSharedMutex mtx)
	{
		mtx.EnterUpgradeableReadLock();
		_mtx = mtx;
	}

	public void Dispose()
	{
		_mtx.ExitUpgradeableReadLock();
	}
}
