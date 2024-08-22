using System;
using System.Collections.Generic;

namespace TaleWorlds.DotNet;

public abstract class ManagedObject
{
	private const int ManagedObjectFirstReferencesTickCount = 200;

	private static List<List<ManagedObject>> _managedObjectFirstReferences;

	private ManagedObjectOwner _managedObjectOwner;

	private int forcedMemory;

	internal ManagedObjectOwner ManagedObjectOwner => _managedObjectOwner;

	internal UIntPtr Pointer
	{
		get
		{
			return _managedObjectOwner.Pointer;
		}
		set
		{
			_managedObjectOwner.Pointer = value;
		}
	}

	internal static void FinalizeManagedObjects()
	{
		lock (_managedObjectFirstReferences)
		{
			_managedObjectFirstReferences.Clear();
		}
	}

	protected void AddUnmanagedMemoryPressure(int size)
	{
		GC.AddMemoryPressure(size);
		forcedMemory = size;
	}

	static ManagedObject()
	{
		_managedObjectFirstReferences = new List<List<ManagedObject>>();
		for (int i = 0; i < 200; i++)
		{
			_managedObjectFirstReferences.Add(new List<ManagedObject>());
		}
	}

	protected ManagedObject(UIntPtr ptr, bool createManagedObjectOwner)
	{
		if (createManagedObjectOwner)
		{
			SetOwnerManagedObject(ManagedObjectOwner.CreateManagedObjectOwner(ptr, this));
		}
		Initialize();
	}

	internal void SetOwnerManagedObject(ManagedObjectOwner owner)
	{
		_managedObjectOwner = owner;
	}

	private void Initialize()
	{
		ManagedObjectFetched(this);
	}

	~ManagedObject()
	{
		if (forcedMemory > 0)
		{
			GC.RemoveMemoryPressure(forcedMemory);
		}
		ManagedObjectOwner.ManagedObjectGarbageCollected(_managedObjectOwner, this);
		_managedObjectOwner = null;
	}

	internal static void HandleManagedObjects()
	{
		lock (_managedObjectFirstReferences)
		{
			List<ManagedObject> list = _managedObjectFirstReferences[199];
			for (int num = 199; num > 0; num--)
			{
				_managedObjectFirstReferences[num] = _managedObjectFirstReferences[num - 1];
			}
			list.Clear();
			_managedObjectFirstReferences[0] = list;
		}
	}

	internal static void ManagedObjectFetched(ManagedObject managedObject)
	{
		lock (_managedObjectFirstReferences)
		{
			if (!Managed.Closing)
			{
				_managedObjectFirstReferences[0].Add(managedObject);
			}
		}
	}

	internal static void FlushManagedObjects()
	{
		lock (_managedObjectFirstReferences)
		{
			for (int i = 0; i < 200; i++)
			{
				_managedObjectFirstReferences[i].Clear();
			}
		}
	}

	[LibraryCallback]
	internal static int GetAliveManagedObjectCount()
	{
		return ManagedObjectOwner.NumberOfAliveManagedObjects;
	}

	[LibraryCallback]
	internal static string GetAliveManagedObjectNames()
	{
		return ManagedObjectOwner.GetAliveManagedObjectNames();
	}

	[LibraryCallback]
	internal static string GetCreationCallstack(string name)
	{
		return ManagedObjectOwner.GetAliveManagedObjectCreationCallstacks(name);
	}

	public int GetManagedId()
	{
		return _managedObjectOwner.NativeId;
	}

	[LibraryCallback]
	internal string GetClassOfObject()
	{
		return GetType().Name;
	}

	public override int GetHashCode()
	{
		return _managedObjectOwner.NativeId;
	}
}
