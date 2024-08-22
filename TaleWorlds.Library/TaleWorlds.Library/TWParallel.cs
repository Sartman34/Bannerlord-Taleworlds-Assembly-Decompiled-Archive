using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TaleWorlds.Library;

public static class TWParallel
{
	public class SingleThreadTestData
	{
		public static SingleThreadTestData GlobalData = new SingleThreadTestData();

		public int InsideThreadCount;
	}

	public struct SingleThreadTestBlock : IDisposable
	{
		private readonly SingleThreadTestData _data;

		public SingleThreadTestBlock(SingleThreadTestData data)
		{
			SingleThreadTestAssert(Interlocked.Increment(ref data.InsideThreadCount) == 1);
			_data = data;
		}

		public void Dispose()
		{
			SingleThreadTestAssert(Interlocked.Decrement(ref _data.InsideThreadCount) == 0);
		}

		private static void SingleThreadTestAssert(bool b)
		{
			if (!b)
			{
				Debugger.Break();
				Debug.FailedAssert("Single thread test have failed!", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\TWParallel.cs", "SingleThreadTestAssert", 89);
			}
		}
	}

	public class RecursiveSingleThreadTestData
	{
		public static RecursiveSingleThreadTestData GlobalData = new RecursiveSingleThreadTestData();

		public static RecursiveSingleThreadTestData ScriptComponentAddRemove = new RecursiveSingleThreadTestData();

		public int InsideCallCount;

		public int InsideThreadId = -1;
	}

	public struct RecursiveSingleThreadTestBlock : IDisposable
	{
		private readonly RecursiveSingleThreadTestData _data;

		public RecursiveSingleThreadTestBlock(RecursiveSingleThreadTestData data)
		{
			_data = data;
			int threadId = GetThreadId();
			lock (_data)
			{
				if (Interlocked.Increment(ref data.InsideCallCount) == 1)
				{
					data.InsideThreadId = threadId;
				}
			}
			SingleThreadTestAssert(data.InsideThreadId == threadId);
		}

		public void Dispose()
		{
			int threadId = GetThreadId();
			SingleThreadTestAssert(_data.InsideThreadId == threadId);
			lock (_data)
			{
				if (Interlocked.Decrement(ref _data.InsideCallCount) == 0)
				{
					_data.InsideThreadId = -1;
				}
			}
		}

		private static void SingleThreadTestAssert(bool b)
		{
			if (!b)
			{
				Debugger.Break();
				Debug.FailedAssert("Single thread test have failed!", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\Base\\TaleWorlds.Library\\TWParallel.cs", "SingleThreadTestAssert", 149);
			}
		}

		private int GetThreadId()
		{
			return Thread.CurrentThread.ManagedThreadId;
		}
	}

	public delegate void ParallelForAuxPredicate(int localStartIndex, int localEndIndex);

	public delegate void ParallelForWithDtAuxPredicate(int localStartIndex, int localEndIndex, float dt);

	private static IParallelDriver _parallelDriver = new DefaultParallelDriver();

	private static ulong _mainThreadId;

	public static bool IsInParallelFor = false;

	public static void InitializeAndSetImplementation(IParallelDriver parallelDriver)
	{
		_parallelDriver = parallelDriver;
		_mainThreadId = GetMainThreadId();
	}

	public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
	{
		return Parallel.ForEach(Partitioner.Create(source), Common.ParallelOptions, body);
	}

	[Obsolete("Please use For() not ForEach() for better Parallel Performance.", true)]
	public static void ForEach<TSource>(IList<TSource> source, Action<TSource> body)
	{
		Parallel.ForEach(Partitioner.Create(source), Common.ParallelOptions, body);
	}

	public static void For(int fromInclusive, int toExclusive, ParallelForAuxPredicate body, int grainSize = 16)
	{
		IsInParallelFor = true;
		if (toExclusive - fromInclusive < grainSize)
		{
			body(fromInclusive, toExclusive);
		}
		else
		{
			_parallelDriver.For(fromInclusive, toExclusive, body, grainSize);
		}
		IsInParallelFor = false;
	}

	public static void For(int fromInclusive, int toExclusive, float deltaTime, ParallelForWithDtAuxPredicate body, int grainSize = 16)
	{
		IsInParallelFor = true;
		if (toExclusive - fromInclusive < grainSize)
		{
			body(fromInclusive, toExclusive, deltaTime);
		}
		else
		{
			_parallelDriver.For(fromInclusive, toExclusive, deltaTime, body, grainSize);
		}
		IsInParallelFor = false;
	}

	[Conditional("_RGL_KEEP_ASSERTS")]
	public static void AssertIsMainThread()
	{
		GetCurrentThreadId();
	}

	public static bool IsMainThread()
	{
		return _mainThreadId == GetCurrentThreadId();
	}

	private static ulong GetMainThreadId()
	{
		return _parallelDriver.GetMainThreadId();
	}

	private static ulong GetCurrentThreadId()
	{
		return _parallelDriver.GetCurrentThreadId();
	}
}
