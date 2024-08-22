using System.Collections.Generic;
using Steamworks;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;

namespace TaleWorlds.MountAndBlade.Launcher.Steam;

public class SteamLauncherModuleExtension : IPlatformModuleExtension
{
	private bool _steamInitialized;

	private List<string> _modulePaths;

	public SteamLauncherModuleExtension()
	{
		_modulePaths = new List<string>();
	}

	public void Initialize()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		_steamInitialized = SteamAPI.Init();
		if (_steamInitialized)
		{
			if (SteamUser.BLoggedOn())
			{
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
			else
			{
				Debug.Print("Steam user is not logged in. Please log in to Steam");
			}
		}
		else
		{
			Debug.Print("Could not initialize Steam");
		}
	}

	public string[] GetModulePaths()
	{
		return _modulePaths.ToArray();
	}

	public void Destroy()
	{
		if (_steamInitialized)
		{
			SteamAPI.Shutdown();
		}
	}

	public void SetLauncherMode(bool isLauncherModeActive)
	{
		if (_steamInitialized)
		{
			SteamUtils.SetGameLauncherMode(isLauncherModeActive);
		}
	}
}
