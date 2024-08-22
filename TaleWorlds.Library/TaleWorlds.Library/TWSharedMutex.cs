using System.Threading;

namespace TaleWorlds.Library;

public class TWSharedMutex
{
	private ReaderWriterLockSlim _mutex;

	public bool IsReadLockHeld => _mutex.IsReadLockHeld;

	public bool IsUpgradeableReadLockHeld => _mutex.IsUpgradeableReadLockHeld;

	public bool IsWriteLockHeld => _mutex.IsWriteLockHeld;

	public TWSharedMutex()
	{
		_mutex = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
	}

	public void EnterReadLock()
	{
		_mutex.EnterReadLock();
	}

	public void EnterUpgradeableReadLock()
	{
		_mutex.EnterUpgradeableReadLock();
	}

	public void EnterWriteLock()
	{
		_mutex.EnterWriteLock();
	}

	public void ExitReadLock()
	{
		_mutex.ExitReadLock();
	}

	public void ExitUpgradeableReadLock()
	{
		_mutex.ExitUpgradeableReadLock();
	}

	public void ExitWriteLock()
	{
		_mutex.ExitWriteLock();
	}
}
