using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaleWorlds.Library;

namespace TaleWorlds.ModuleManager;

public static class ModuleHelper
{
	private static Dictionary<string, ModuleInfo> _allFoundModules;

	private static IPlatformModuleExtension _platformModuleExtension;

	private static string _pathPrefix => BasePath.Name + "Modules/";

	public static string GetModuleFullPath(string moduleId)
	{
		EnsureModuleInfosAreLoaded();
		return _allFoundModules[moduleId.ToLower()].FolderPath + "/";
	}

	public static ModuleInfo GetModuleInfo(string moduleId)
	{
		EnsureModuleInfosAreLoaded();
		string key = moduleId.ToLower();
		if (_allFoundModules.ContainsKey(key))
		{
			return _allFoundModules[key];
		}
		return null;
	}

	public static void InitializePlatformModuleExtension(IPlatformModuleExtension moduleExtension)
	{
		_platformModuleExtension = moduleExtension;
		_platformModuleExtension.Initialize();
	}

	public static void ClearPlatformModuleExtension()
	{
		if (_platformModuleExtension != null)
		{
			_platformModuleExtension.Destroy();
			_platformModuleExtension = null;
		}
		_allFoundModules = null;
	}

	private static void EnsureModuleInfosAreLoaded()
	{
		if (_allFoundModules == null)
		{
			GetModules();
		}
	}

	private static IEnumerable<string> GetModulePaths(string directoryPath, int searchDepth)
	{
		string[] array;
		if (searchDepth > 0)
		{
			string[] directories = Directory.GetDirectories(directoryPath);
			array = directories;
			foreach (string directoryPath2 in array)
			{
				foreach (string item in GetModulePaths(directoryPath2, searchDepth - 1).ToList())
				{
					yield return item;
				}
			}
		}
		string[] files = Directory.GetFiles(directoryPath, "SubModule.xml");
		array = files;
		for (int i = 0; i < array.Length; i++)
		{
			yield return array[i];
		}
	}

	private static List<ModuleInfo> GetPhysicalModules()
	{
		List<ModuleInfo> list = new List<ModuleInfo>();
		string[] array = GetModulePaths(_pathPrefix, 1).ToArray();
		foreach (string text in array)
		{
			ModuleInfo moduleInfo = new ModuleInfo();
			try
			{
				string directoryName = Path.GetDirectoryName(text);
				moduleInfo.LoadWithFullPath(directoryName);
				list.Add(moduleInfo);
			}
			catch (Exception ex)
			{
				string lpText = "Module " + text + " can't be loaded, there are some errors." + Environment.NewLine + Environment.NewLine + ex.Message;
				string lpCaption = "ERROR";
				Debug.ShowMessageBox(lpText, lpCaption, 4u);
			}
		}
		return list;
	}

	private static List<ModuleInfo> GetPlatformModules()
	{
		List<ModuleInfo> list = new List<ModuleInfo>();
		if (_platformModuleExtension != null)
		{
			string[] modulePaths = _platformModuleExtension.GetModulePaths();
			foreach (string text in modulePaths)
			{
				ModuleInfo moduleInfo = new ModuleInfo();
				try
				{
					moduleInfo.LoadWithFullPath(text);
					list.Add(moduleInfo);
				}
				catch (Exception ex)
				{
					string lpText = "Module " + text + " can't be loaded, there are some errors." + Environment.NewLine + Environment.NewLine + ex.Message;
					string lpCaption = "ERROR";
					Debug.ShowMessageBox(lpText, lpCaption, 4u);
				}
			}
		}
		return list;
	}

	public static List<ModuleInfo> GetModuleInfos(string[] moduleNames)
	{
		List<ModuleInfo> list = new List<ModuleInfo>();
		for (int i = 0; i < moduleNames.Length; i++)
		{
			ModuleInfo moduleInfo = GetModuleInfo(moduleNames[i]);
			list.Add(moduleInfo);
		}
		return list;
	}

	public static IEnumerable<ModuleInfo> GetModules()
	{
		if (_allFoundModules == null)
		{
			List<ModuleInfo> list = new List<ModuleInfo>();
			List<ModuleInfo> physicalModules = GetPhysicalModules();
			List<ModuleInfo> platformModules = GetPlatformModules();
			list.AddRange(physicalModules);
			list.AddRange(platformModules);
			List<ModuleInfo> list2 = new List<ModuleInfo>();
			_allFoundModules = new Dictionary<string, ModuleInfo>();
			foreach (ModuleInfo item in list)
			{
				if (item.IsOfficial)
				{
					list2.Add(item);
					item.UpdateVersionChangeSet();
				}
				if (!_allFoundModules.ContainsKey(item.Id.ToLower()))
				{
					_allFoundModules.Add(item.Id.ToLower(), item);
				}
			}
			foreach (ModuleInfo item2 in list)
			{
				foreach (DependedModule dependedModule in item2.DependedModules)
				{
					if (list2.Any((ModuleInfo m) => m.Id == dependedModule.ModuleId))
					{
						dependedModule.UpdateVersionChangeSet();
					}
				}
			}
		}
		return _allFoundModules.Select((KeyValuePair<string, ModuleInfo> m) => m.Value);
	}

	public static string GetMbprojPath(string id)
	{
		EnsureModuleInfosAreLoaded();
		string key = id.ToLower();
		if (_allFoundModules.ContainsKey(key))
		{
			return _allFoundModules[key].FolderPath + "/ModuleData/project.mbproj";
		}
		return "";
	}

	public static string GetXmlPathForNative(string moduleId, string xmlName)
	{
		return GetModuleFullPath(moduleId) + xmlName;
	}

	public static string GetXmlPathForNativeWBase(string moduleId, string xmlName)
	{
		return "$BASE/Modules/" + moduleId + "/" + xmlName;
	}

	public static string GetXsltPathForNative(string moduleId, string xsltName)
	{
		xsltName = xsltName.Remove(xsltName.Length - 4);
		return GetModuleFullPath(moduleId) + xsltName + ".xsl";
	}

	public static string GetPath(string id)
	{
		return GetModuleFullPath(id) + "SubModule.xml";
	}

	public static string GetXmlPath(string moduleId, string xmlName)
	{
		return GetModuleFullPath(moduleId) + "ModuleData/" + xmlName + ".xml";
	}

	public static string GetXsltPath(string moduleId, string xmlName)
	{
		return GetModuleFullPath(moduleId) + "ModuleData/" + xmlName + ".xsl";
	}

	public static string GetXsdPath(string xmlInfoId)
	{
		return BasePath.Name + "XmlSchemas/" + xmlInfoId + ".xsd";
	}

	public static IEnumerable<ModuleInfo> GetDependentModulesOf(IEnumerable<ModuleInfo> source, ModuleInfo module)
	{
		foreach (DependedModule item in module.DependedModules)
		{
			ModuleInfo moduleInfo = source.FirstOrDefault((ModuleInfo i) => i.Id == item.ModuleId);
			if (moduleInfo != null)
			{
				yield return moduleInfo;
			}
		}
		foreach (ModuleInfo item2 in source)
		{
			if (item2.ModulesToLoadAfterThis.Any((DependedModule m) => m.ModuleId == module.Id))
			{
				yield return item2;
			}
		}
	}

	public static List<ModuleInfo> GetSortedModules(string[] moduleIDs)
	{
		List<ModuleInfo> modules = GetModuleInfos(moduleIDs);
		IList<ModuleInfo> list = MBMath.TopologySort(modules, (ModuleInfo module) => GetDependentModulesOf(modules, module));
		if (!(list is List<ModuleInfo> result))
		{
			return list.ToList();
		}
		return result;
	}
}
