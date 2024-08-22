using System;
using System.Collections.Generic;
using System.Reflection;

namespace TaleWorlds.Library;

public class TypeCollector<T> where T : class
{
	private Dictionary<string, Type> _types;

	private Assembly _currentAssembly;

	public Type BaseType { get; private set; }

	public TypeCollector()
	{
		BaseType = typeof(T);
		_types = new Dictionary<string, Type>();
		_currentAssembly = BaseType.Assembly;
	}

	public void Collect()
	{
		List<Type> list = CollectTypes();
		_types.Clear();
		foreach (Type item in list)
		{
			_types.Add(item.Name, item);
		}
	}

	public T Instantiate(string typeName, params object[] parameters)
	{
		T result = null;
		if (_types.TryGetValue(typeName, out var value))
		{
			return (T)value.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, new Type[0], null).Invoke(parameters);
		}
		return result;
	}

	public Type GetType(string typeName)
	{
		if (_types.TryGetValue(typeName, out var value))
		{
			return value;
		}
		return null;
	}

	private bool CheckAssemblyReferencesThis(Assembly assembly)
	{
		AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
		for (int i = 0; i < referencedAssemblies.Length; i++)
		{
			if (referencedAssemblies[i].Name == _currentAssembly.GetName().Name)
			{
				return true;
			}
		}
		return false;
	}

	private List<Type> CollectTypes()
	{
		List<Type> list = new List<Type>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (!CheckAssemblyReferencesThis(assembly) && !(assembly == _currentAssembly))
			{
				continue;
			}
			foreach (Type item in assembly.GetTypesSafe())
			{
				if (BaseType.IsAssignableFrom(item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}
}
