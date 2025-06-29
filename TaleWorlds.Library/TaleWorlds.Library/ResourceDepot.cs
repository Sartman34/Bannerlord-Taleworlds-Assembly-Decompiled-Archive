using System;
using System.Collections.Generic;
using System.IO;

namespace TaleWorlds.Library;

public class ResourceDepot
{
	private readonly MBList<ResourceDepotLocation> _resourceLocations;

	private readonly Dictionary<string, ResourceDepotFile> _files;

	private bool _isThereAnyUnhandledChange;

	public MBReadOnlyList<ResourceDepotLocation> ResourceLocations => _resourceLocations;

	public event ResourceChangeEvent OnResourceChange;

	public ResourceDepot()
	{
		_resourceLocations = new MBList<ResourceDepotLocation>();
		_files = new Dictionary<string, ResourceDepotFile>();
	}

	public void AddLocation(string basePath, string location)
	{
		ResourceDepotLocation item = new ResourceDepotLocation(basePath, location, Path.GetFullPath(basePath + location));
		_resourceLocations.Add(item);
	}

	public void CollectResources()
	{
		_files.Clear();
		foreach (ResourceDepotLocation resourceLocation in _resourceLocations)
		{
			string fullPath = Path.GetFullPath(resourceLocation.BasePath + resourceLocation.Path);
			string[] files = Directory.GetFiles(resourceLocation.BasePath + resourceLocation.Path, "*", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				string fullPath2 = Path.GetFullPath(files[i]);
				fullPath2 = fullPath2.Replace('\\', '/');
				string text = fullPath2.Substring(fullPath.Length);
				string key = text.ToLower();
				ResourceDepotFile value = new ResourceDepotFile(resourceLocation, text, fullPath2);
				if (_files.ContainsKey(key))
				{
					_files[key] = value;
				}
				else
				{
					_files.Add(key, value);
				}
			}
		}
	}

	public string[] GetFiles(string subDirectory, string extension, bool excludeSubContents = false)
	{
		string value = extension.ToLower();
		List<string> list = new List<string>();
		foreach (ResourceDepotFile value2 in _files.Values)
		{
			string text = Path.GetFullPath(value2.BasePath + value2.Location + subDirectory).Replace('\\', '/').ToLower();
			string fullPath = value2.FullPath;
			string fullPathLowerCase = value2.FullPathLowerCase;
			bool num = (!excludeSubContents && fullPathLowerCase.StartsWith(text)) || (excludeSubContents && string.Equals(Directory.GetParent(text).FullName, text, StringComparison.CurrentCultureIgnoreCase));
			bool flag = fullPathLowerCase.EndsWith(value, StringComparison.OrdinalIgnoreCase);
			if (num && flag)
			{
				list.Add(fullPath);
			}
		}
		return list.ToArray();
	}

	public string GetFilePath(string file)
	{
		file = file.Replace('\\', '/');
		return _files[file.ToLower()].FullPath;
	}

	public IEnumerable<string> GetFilesEndingWith(string fileEndName)
	{
		fileEndName = fileEndName.Replace('\\', '/');
		foreach (KeyValuePair<string, ResourceDepotFile> file in _files)
		{
			if (file.Key.EndsWith(fileEndName.ToLower()))
			{
				yield return file.Value.FullPath;
			}
		}
	}

	public void StartWatchingChangesInDepot()
	{
		foreach (ResourceDepotLocation resourceLocation in _resourceLocations)
		{
			resourceLocation.StartWatchingChanges(OnAnyChangeInDepotLocations, OnAnyRenameInDepotLocations);
		}
	}

	public void StopWatchingChangesInDepot()
	{
		foreach (ResourceDepotLocation resourceLocation in _resourceLocations)
		{
			resourceLocation.StopWatchingChanges();
		}
	}

	private void OnAnyChangeInDepotLocations(object source, FileSystemEventArgs e)
	{
		_isThereAnyUnhandledChange = true;
	}

	private void OnAnyRenameInDepotLocations(object source, RenamedEventArgs e)
	{
		_isThereAnyUnhandledChange = true;
	}

	public void CheckForChanges()
	{
		if (_isThereAnyUnhandledChange)
		{
			CollectResources();
			this.OnResourceChange?.Invoke();
			_isThereAnyUnhandledChange = false;
		}
	}
}
