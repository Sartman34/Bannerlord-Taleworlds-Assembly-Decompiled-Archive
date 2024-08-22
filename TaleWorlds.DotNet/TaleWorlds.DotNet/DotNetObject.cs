using System;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace TaleWorlds.DotNet;

public class DotNetObject
{
	private static readonly object Locker;

	private const int DotnetObjectFirstReferencesTickCount = 200;

	private static readonly List<Dictionary<int, DotNetObject>> DotnetObjectFirstReferences;

	private static readonly Dictionary<int, DotNetObjectReferenceCounter> DotnetObjectReferences;

	private static int _totalCreatedObjectCount;

	private readonly int _objectId;

	private static int _numberOfAliveDotNetObjects;

	internal static int NumberOfAliveDotNetObjects => _numberOfAliveDotNetObjects;

	static DotNetObject()
	{
		Locker = new object();
		DotnetObjectReferences = new Dictionary<int, DotNetObjectReferenceCounter>();
		DotnetObjectFirstReferences = new List<Dictionary<int, DotNetObject>>();
		for (int i = 0; i < 200; i++)
		{
			DotnetObjectFirstReferences.Add(new Dictionary<int, DotNetObject>());
		}
	}

	protected DotNetObject()
	{
		lock (Locker)
		{
			_totalCreatedObjectCount++;
			_objectId = _totalCreatedObjectCount;
			DotnetObjectFirstReferences[0].Add(_objectId, this);
			_numberOfAliveDotNetObjects++;
		}
	}

	~DotNetObject()
	{
		lock (Locker)
		{
			_numberOfAliveDotNetObjects--;
		}
	}

	[LibraryCallback]
	internal static int GetAliveDotNetObjectCount()
	{
		return _numberOfAliveDotNetObjects;
	}

	[LibraryCallback]
	internal static void IncreaseReferenceCount(int dotnetObjectId)
	{
		lock (Locker)
		{
			if (DotnetObjectReferences.ContainsKey(dotnetObjectId))
			{
				DotNetObjectReferenceCounter value = DotnetObjectReferences[dotnetObjectId];
				value.ReferenceCount++;
				DotnetObjectReferences[dotnetObjectId] = value;
			}
			else
			{
				DotNetObject dotNetObjectFromFirstReferences = GetDotNetObjectFromFirstReferences(dotnetObjectId);
				DotNetObjectReferenceCounter value2 = default(DotNetObjectReferenceCounter);
				value2.ReferenceCount = 1;
				value2.DotNetObject = dotNetObjectFromFirstReferences;
				DotnetObjectReferences.Add(dotnetObjectId, value2);
			}
		}
	}

	[LibraryCallback]
	internal static void DecreaseReferenceCount(int dotnetObjectId)
	{
		lock (Locker)
		{
			DotNetObjectReferenceCounter value = DotnetObjectReferences[dotnetObjectId];
			value.ReferenceCount--;
			if (value.ReferenceCount == 0)
			{
				DotnetObjectReferences.Remove(dotnetObjectId);
			}
			else
			{
				DotnetObjectReferences[dotnetObjectId] = value;
			}
		}
	}

	internal static DotNetObject GetManagedObjectWithId(int dotnetObjectId)
	{
		lock (Locker)
		{
			if (DotnetObjectReferences.TryGetValue(dotnetObjectId, out var value))
			{
				return value.DotNetObject;
			}
			return GetDotNetObjectFromFirstReferences(dotnetObjectId);
		}
	}

	private static DotNetObject GetDotNetObjectFromFirstReferences(int dotnetObjectId)
	{
		foreach (Dictionary<int, DotNetObject> dotnetObjectFirstReference in DotnetObjectFirstReferences)
		{
			if (dotnetObjectFirstReference.TryGetValue(dotnetObjectId, out var value))
			{
				return value;
			}
		}
		return null;
	}

	internal int GetManagedId()
	{
		return _objectId;
	}

	[LibraryCallback]
	internal static string GetAliveDotNetObjectNames()
	{
		MBStringBuilder mBStringBuilder = default(MBStringBuilder);
		mBStringBuilder.Initialize(16, "GetAliveDotNetObjectNames");
		lock (Locker)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (DotNetObjectReferenceCounter value in DotnetObjectReferences.Values)
			{
				Type type = value.DotNetObject.GetType();
				if (!dictionary.ContainsKey(type.Name))
				{
					dictionary.Add(type.Name, 1);
				}
				else
				{
					dictionary[type.Name] = dictionary[type.Name] + 1;
				}
			}
			foreach (string key in dictionary.Keys)
			{
				mBStringBuilder.Append(key + "," + dictionary[key] + "-");
			}
		}
		return mBStringBuilder.ToStringAndRelease();
	}

	internal static void HandleDotNetObjects()
	{
		lock (Locker)
		{
			Dictionary<int, DotNetObject> dictionary = DotnetObjectFirstReferences[199];
			for (int num = 199; num > 0; num--)
			{
				DotnetObjectFirstReferences[num] = DotnetObjectFirstReferences[num - 1];
			}
			dictionary.Clear();
			DotnetObjectFirstReferences[0] = dictionary;
		}
	}
}
