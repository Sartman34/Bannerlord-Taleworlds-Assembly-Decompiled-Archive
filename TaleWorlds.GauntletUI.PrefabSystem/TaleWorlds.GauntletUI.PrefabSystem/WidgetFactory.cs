using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace TaleWorlds.GauntletUI.PrefabSystem;

public class WidgetFactory
{
	private Dictionary<string, Type> _builtinTypes;

	private Dictionary<string, string> _customTypePaths;

	private Dictionary<string, CustomWidgetType> _liveCustomTypes;

	private Dictionary<string, int> _liveInstanceTracker;

	private ResourceDepot _resourceDepot;

	private readonly string _resourceFolder;

	public PrefabExtensionContext PrefabExtensionContext { get; private set; }

	public WidgetAttributeContext WidgetAttributeContext { get; private set; }

	public GeneratedPrefabContext GeneratedPrefabContext { get; private set; }

	public event Action PrefabChange;

	public WidgetFactory(ResourceDepot resourceDepot, string resourceFolder)
	{
		_resourceDepot = resourceDepot;
		_resourceDepot.OnResourceChange += OnResourceChange;
		_resourceFolder = resourceFolder;
		_builtinTypes = new Dictionary<string, Type>();
		_liveCustomTypes = new Dictionary<string, CustomWidgetType>();
		_customTypePaths = new Dictionary<string, string>();
		_liveInstanceTracker = new Dictionary<string, int>();
		PrefabExtensionContext = new PrefabExtensionContext();
		WidgetAttributeContext = new WidgetAttributeContext();
		GeneratedPrefabContext = new GeneratedPrefabContext();
	}

	private void OnResourceChange()
	{
		CheckForUpdates();
	}

	public void Initialize(List<string> assemblyOrder = null)
	{
		foreach (PrefabExtension prefabExtension in PrefabExtensionContext.PrefabExtensions)
		{
			prefabExtension.RegisterAttributeTypes(WidgetAttributeContext);
		}
		foreach (Type item in WidgetInfo.CollectWidgetTypes())
		{
			bool flag = true;
			if (_builtinTypes.ContainsKey(item.Name) && assemblyOrder != null)
			{
				flag = assemblyOrder.IndexOf(item.Assembly.GetName().Name + ".dll") > assemblyOrder.IndexOf(_builtinTypes[item.Name].Assembly.GetName().Name + ".dll");
			}
			if (flag)
			{
				_builtinTypes[item.Name] = item;
			}
		}
		foreach (KeyValuePair<string, string> item2 in GetPrefabNamesAndPathsFromCurrentPath())
		{
			AddCustomType(item2.Key, item2.Value);
		}
	}

	private Dictionary<string, string> GetPrefabNamesAndPathsFromCurrentPath()
	{
		string[] files = _resourceDepot.GetFiles(_resourceFolder, ".xml");
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] array = files;
		foreach (string path in array)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			string directoryName = Path.GetDirectoryName(path);
			if (!dictionary.ContainsKey(fileNameWithoutExtension))
			{
				dictionary.Add(fileNameWithoutExtension, directoryName + "\\");
				continue;
			}
			Debug.FailedAssert("This prefab has already been added: " + fileNameWithoutExtension + ". Previous Directory: " + dictionary[fileNameWithoutExtension] + " | New Directory: " + directoryName, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI.PrefabSystem\\WidgetFactory.cs", "GetPrefabNamesAndPathsFromCurrentPath", 94);
			dictionary[fileNameWithoutExtension] = directoryName + "\\";
		}
		return dictionary;
	}

	public void AddCustomType(string name, string path)
	{
		_customTypePaths.Add(name, path);
	}

	public IEnumerable<string> GetPrefabNames()
	{
		return _customTypePaths.Keys;
	}

	public IEnumerable<string> GetWidgetTypes()
	{
		return _builtinTypes.Keys.Concat(_customTypePaths.Keys);
	}

	public bool IsBuiltinType(string name)
	{
		return _builtinTypes.ContainsKey(name);
	}

	public Type GetBuiltinType(string name)
	{
		return _builtinTypes[name];
	}

	public bool IsCustomType(string typeName)
	{
		return _customTypePaths.ContainsKey(typeName);
	}

	public string GetCustomTypePath(string name)
	{
		if (_customTypePaths.TryGetValue(name, out var value))
		{
			return value;
		}
		Debug.FailedAssert("false", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI.PrefabSystem\\WidgetFactory.cs", "GetCustomTypePath", 139);
		return "";
	}

	public Widget CreateBuiltinWidget(UIContext context, string typeName)
	{
		Widget widget = null;
		if (_builtinTypes.TryGetValue(typeName, out var value))
		{
			widget = (Widget)value.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, new Type[1] { typeof(UIContext) }, null).InvokeWithLog(context);
		}
		else
		{
			widget = new Widget(context);
			Debug.FailedAssert("builtin widget type not found in CreateBuiltinWidget(" + typeName + ")", "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI.PrefabSystem\\WidgetFactory.cs", "CreateBuiltinWidget", 160);
		}
		return widget;
	}

	public WidgetPrefab GetCustomType(string typeName)
	{
		if (_liveCustomTypes.TryGetValue(typeName, out var value))
		{
			_liveInstanceTracker[typeName]++;
			return value.WidgetPrefab;
		}
		if (_customTypePaths.TryGetValue(typeName, out var value2))
		{
			CustomWidgetType customWidgetType = new CustomWidgetType(this, value2, typeName);
			_liveCustomTypes[typeName] = customWidgetType;
			_liveInstanceTracker[typeName] = 1;
			return customWidgetType.WidgetPrefab;
		}
		Debug.FailedAssert("Couldn't find Custom Widget type: " + typeName, "C:\\Develop\\MB3\\TaleWorlds.Shared\\Source\\GauntletUI\\TaleWorlds.GauntletUI.PrefabSystem\\WidgetFactory.cs", "GetCustomType", 183);
		return null;
	}

	public void OnUnload(string typeName)
	{
		if (_liveCustomTypes.ContainsKey(typeName))
		{
			_liveInstanceTracker[typeName]--;
			if (_liveInstanceTracker[typeName] == 0)
			{
				_liveCustomTypes.Remove(typeName);
				_liveInstanceTracker.Remove(typeName);
			}
		}
	}

	public void CheckForUpdates()
	{
		bool flag = false;
		Dictionary<string, string> prefabNamesAndPathsFromCurrentPath = GetPrefabNamesAndPathsFromCurrentPath();
		foreach (KeyValuePair<string, string> item in prefabNamesAndPathsFromCurrentPath)
		{
			if (!_customTypePaths.ContainsKey(item.Key))
			{
				AddCustomType(item.Key, item.Value);
			}
		}
		List<string> list = null;
		foreach (string key in _customTypePaths.Keys)
		{
			if (!prefabNamesAndPathsFromCurrentPath.ContainsKey(key))
			{
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(key);
				flag = true;
			}
		}
		if (list != null)
		{
			foreach (string item2 in list)
			{
				_customTypePaths.Remove(item2);
			}
		}
		foreach (CustomWidgetType value in _liveCustomTypes.Values)
		{
			flag = flag || value.CheckForUpdate();
		}
		if (flag)
		{
			this.PrefabChange?.Invoke();
		}
	}
}
