using System;
using System.Collections;
using System.Collections.Generic;

namespace TaleWorlds.Library;

public class MBQueue<T> : IMBCollection, ICollection, IEnumerable, IEnumerable<T>
{
	private readonly Queue<T> _data;

	public int Count => _data.Count;

	public bool IsSynchronized => false;

	public object SyncRoot => null;

	public MBQueue()
	{
		_data = new Queue<T>();
	}

	public MBQueue(Queue<T> queue)
	{
		_data = new Queue<T>(queue);
	}

	public bool Contains(T item)
	{
		return _data.Contains(item);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _data.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Clear()
	{
		_data.Clear();
	}

	public void Enqueue(T item)
	{
		_data.Enqueue(item);
	}

	public T Dequeue()
	{
		return _data.Dequeue();
	}

	public void CopyTo(Array array, int index)
	{
		_data.CopyTo((T[])array, index);
	}
}
