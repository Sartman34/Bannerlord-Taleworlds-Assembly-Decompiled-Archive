using System.Collections.Generic;
using Steamworks;
using TaleWorlds.ModuleManager;

namespace TaleWorlds.PlatformService.Steam;

public class SteamModuleExtension : IPlatformModuleExtension
{
	private List<string> _modulePaths;

	public SteamModuleExtension()
	{
		_modulePaths = new List<string>();
	}

	public void Initialize()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		PublishedFileId_t[] array = null;
		if (numSubscribedItems == 0)
		{
			return;
		}
		array = (PublishedFileId_t[])(object)new PublishedFileId_t[numSubscribedItems];
		SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		ulong num = default(ulong);
		string item = default(string);
		uint num2 = default(uint);
		for (int i = 0; i < numSubscribedItems; i++)
		{
			if (SteamUGC.GetItemInstallInfo(array[i], ref num, ref item, 4096u, ref num2))
			{
				_modulePaths.Add(item);
			}
		}
	}

	public string[] GetModulePaths()
	{
		return _modulePaths.ToArray();
	}

	public void Destroy()
	{
		_modulePaths.Clear();
	}

	public void SetLauncherMode(bool isLauncherModeActive)
	{
		SteamUtils.SetGameLauncherMode(isLauncherModeActive);
	}
}
