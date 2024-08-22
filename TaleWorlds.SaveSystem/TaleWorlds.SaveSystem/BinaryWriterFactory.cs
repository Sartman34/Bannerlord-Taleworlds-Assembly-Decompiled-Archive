using System.Collections.Generic;
using System.Threading;
using TaleWorlds.Library;

namespace TaleWorlds.SaveSystem;

internal static class BinaryWriterFactory
{
	private const int WritersPerThread = 5;

	private static ThreadLocal<Stack<BinaryWriter>> _binaryWriters;

	public static BinaryWriter GetBinaryWriter()
	{
		if (_binaryWriters.Value == null)
		{
			_binaryWriters.Value = new Stack<BinaryWriter>();
			for (int i = 0; i < 5; i++)
			{
				BinaryWriter item = new BinaryWriter(4096);
				_binaryWriters.Value.Push(item);
			}
		}
		Stack<BinaryWriter> value = _binaryWriters.Value;
		if (value.Count != 0)
		{
			return value.Pop();
		}
		return new BinaryWriter(4096);
	}

	public static void ReleaseBinaryWriter(BinaryWriter writer)
	{
		if (_binaryWriters != null)
		{
			writer.Clear();
			_binaryWriters.Value.Push(writer);
		}
	}

	public static void Initialize()
	{
		_binaryWriters = new ThreadLocal<Stack<BinaryWriter>>();
	}

	public static void Release()
	{
		_binaryWriters = null;
	}
}
