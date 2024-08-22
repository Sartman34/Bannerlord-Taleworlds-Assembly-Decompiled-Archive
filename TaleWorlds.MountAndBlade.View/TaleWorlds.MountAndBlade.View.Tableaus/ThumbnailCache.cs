using System.Collections.Generic;
using TaleWorlds.Engine;

namespace TaleWorlds.MountAndBlade.View.Tableaus;

public class ThumbnailCache
{
	private int _capacity;

	private ThumbnailCreatorView _thumbnailCreatorView;

	private Dictionary<string, ThumbnailCacheNode> _map;

	private NodeComparer _nodeComparer = new NodeComparer();

	public int Count { get; private set; }

	public ThumbnailCache(int capacity, ThumbnailCreatorView thumbnailCreatorView)
	{
		_capacity = capacity;
		_map = new Dictionary<string, ThumbnailCacheNode>(capacity);
		_thumbnailCreatorView = thumbnailCreatorView;
		Count = 0;
	}

	public bool GetValue(string key, out Texture texture)
	{
		texture = null;
		if (_map.TryGetValue(key, out var value))
		{
			value.FrameNo = Utilities.EngineFrameNo;
			texture = value.Value;
			return true;
		}
		return false;
	}

	public bool AddReference(string key)
	{
		if (_map.TryGetValue(key, out var value))
		{
			value.ReferenceCount++;
			return true;
		}
		return false;
	}

	public bool MarkForDeletion(string key)
	{
		if (_map.TryGetValue(key, out var value))
		{
			value.ReferenceCount--;
			return true;
		}
		return false;
	}

	public void ForceDelete(string key)
	{
		if (_map.TryGetValue(key, out var value))
		{
			value.ReferenceCount = 0;
		}
	}

	public void Tick()
	{
		if (Count <= _capacity)
		{
			return;
		}
		List<ThumbnailCacheNode> list = new List<ThumbnailCacheNode>();
		foreach (KeyValuePair<string, ThumbnailCacheNode> item in _map)
		{
			if (item.Value.ReferenceCount == 0)
			{
				list.Add(item.Value);
			}
		}
		list.Sort(_nodeComparer);
		int num = 0;
		while (Count > _capacity && num < list.Count)
		{
			_map.Remove(list[num].Key);
			_thumbnailCreatorView.CancelRequest(list[num].Key);
			if (list[num].Value != null)
			{
				list[num].Value.Release();
			}
			num++;
			Count--;
		}
		list.RemoveRange(0, num);
	}

	public void Add(string key, Texture value)
	{
		if (_map.TryGetValue(key, out var value2))
		{
			value2.Value = value;
			value2.FrameNo = Utilities.EngineFrameNo;
		}
		else
		{
			ThumbnailCacheNode value3 = new ThumbnailCacheNode(key, value, Utilities.EngineFrameNo);
			_map[key] = value3;
			Count++;
		}
	}

	public int GetTotalMemorySize()
	{
		int num = 0;
		foreach (ThumbnailCacheNode value in _map.Values)
		{
			num += value.Value.MemorySize;
		}
		return num;
	}
}
