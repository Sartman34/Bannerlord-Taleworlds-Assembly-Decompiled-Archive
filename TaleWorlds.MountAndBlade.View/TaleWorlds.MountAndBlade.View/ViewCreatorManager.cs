using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View;

public static class ViewCreatorManager
{
	private static Dictionary<string, MethodInfo> _viewCreators;

	private static Dictionary<Type, Type> _actualViewTypes;

	private static Dictionary<Type, Type> _defaultTypes;

	static ViewCreatorManager()
	{
		_viewCreators = new Dictionary<string, MethodInfo>();
		_actualViewTypes = new Dictionary<Type, Type>();
		_defaultTypes = new Dictionary<Type, Type>();
		Assembly[] viewAssemblies = GetViewAssemblies();
		Assembly assembly = typeof(ViewCreatorModule).Assembly;
		CheckAssemblyScreens(assembly);
		Assembly[] array = viewAssemblies;
		for (int i = 0; i < array.Length; i++)
		{
			CheckAssemblyScreens(array[i]);
		}
		CollectDefaults(assembly);
		array = viewAssemblies;
		for (int i = 0; i < array.Length; i++)
		{
			CollectDefaults(array[i]);
		}
		array = viewAssemblies;
		for (int i = 0; i < array.Length; i++)
		{
			CheckOverridenViews(array[i]);
		}
	}

	private static void CheckAssemblyScreens(Assembly assembly)
	{
		foreach (Type item in assembly.GetTypesSafe())
		{
			object[] customAttributesSafe = item.GetCustomAttributesSafe(typeof(ViewCreatorModule), inherit: false);
			if (customAttributesSafe == null || customAttributesSafe.Length != 1 || !(customAttributesSafe[0] is ViewCreatorModule))
			{
				continue;
			}
			MethodInfo[] methods = item.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.GetCustomAttributesSafe(typeof(ViewMethod), inherit: false)[0] is ViewMethod viewMethod)
				{
					_viewCreators.Add(viewMethod.Name, methodInfo);
				}
			}
		}
	}

	private static Assembly[] GetViewAssemblies()
	{
		List<Assembly> list = new List<Assembly>();
		Assembly assembly = typeof(ViewCreatorModule).Assembly;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly2 in assemblies)
		{
			AssemblyName[] referencedAssemblies = assembly2.GetReferencedAssemblies();
			for (int j = 0; j < referencedAssemblies.Length; j++)
			{
				if (referencedAssemblies[j].ToString() == assembly.GetName().ToString())
				{
					list.Add(assembly2);
					break;
				}
			}
		}
		return list.ToArray();
	}

	internal static IEnumerable<MissionBehavior> CreateDefaultMissionBehaviors(Mission mission)
	{
		List<MissionBehavior> list = new List<MissionBehavior>();
		foreach (KeyValuePair<Type, Type> defaultType in _defaultTypes)
		{
			if (!defaultType.Value.IsAbstract)
			{
				MissionBehavior item = Activator.CreateInstance(defaultType.Value) as MissionBehavior;
				list.Add(item);
			}
		}
		return list;
	}

	internal static IEnumerable<MissionBehavior> CollectMissionBehaviors(string missionName, Mission mission, IEnumerable<MissionBehavior> behaviors)
	{
		List<MissionBehavior> list = new List<MissionBehavior>();
		if (_viewCreators.ContainsKey(missionName))
		{
			MissionBehavior[] collection = _viewCreators[missionName].Invoke(null, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new object[1] { mission }, null) as MissionBehavior[];
			list.AddRange(collection);
		}
		return behaviors.Concat(list);
	}

	public static ScreenBase CreateScreenView<T>() where T : ScreenBase, new()
	{
		if (_actualViewTypes.ContainsKey(typeof(T)))
		{
			return Activator.CreateInstance(_actualViewTypes[typeof(T)]) as ScreenBase;
		}
		return new T();
	}

	public static ScreenBase CreateScreenView<T>(params object[] parameters) where T : ScreenBase
	{
		Type type = typeof(T);
		if (_actualViewTypes.ContainsKey(typeof(T)))
		{
			type = _actualViewTypes[typeof(T)];
		}
		return Activator.CreateInstance(type, parameters) as ScreenBase;
	}

	public static MissionView CreateMissionView<T>(bool isNetwork = false, Mission mission = null, params object[] parameters) where T : MissionView, new()
	{
		if (_actualViewTypes.ContainsKey(typeof(T)))
		{
			return Activator.CreateInstance(_actualViewTypes[typeof(T)], parameters) as MissionView;
		}
		return new T();
	}

	public static MissionView CreateMissionViewWithArgs<T>(params object[] parameters) where T : MissionView
	{
		Type type = typeof(T);
		if (_actualViewTypes.ContainsKey(typeof(T)))
		{
			type = _actualViewTypes[typeof(T)];
		}
		return Activator.CreateInstance(type, parameters) as MissionView;
	}

	private static void CheckOverridenViews(Assembly assembly)
	{
		foreach (Type item in assembly.GetTypesSafe())
		{
			if (!typeof(MissionView).IsAssignableFrom(item) && !typeof(ScreenBase).IsAssignableFrom(item))
			{
				continue;
			}
			object[] customAttributesSafe = item.GetCustomAttributesSafe(typeof(OverrideView), inherit: false);
			if (customAttributesSafe != null && customAttributesSafe.Length == 1 && customAttributesSafe[0] is OverrideView overrideView)
			{
				_actualViewTypes[overrideView.BaseType] = item;
				if (_defaultTypes.ContainsKey(overrideView.BaseType))
				{
					_defaultTypes[overrideView.BaseType] = item;
				}
			}
		}
	}

	private static void CollectDefaults(Assembly assembly)
	{
		foreach (Type item in assembly.GetTypesSafe())
		{
			if (typeof(MissionBehavior).IsAssignableFrom(item) && item.GetCustomAttributesSafe(typeof(DefaultView), inherit: false).Length == 1)
			{
				_defaultTypes.Add(item, item);
			}
		}
	}
}
