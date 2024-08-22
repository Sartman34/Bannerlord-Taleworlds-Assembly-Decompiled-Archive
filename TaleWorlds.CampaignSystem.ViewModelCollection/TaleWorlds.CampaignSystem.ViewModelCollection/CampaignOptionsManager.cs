using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection;

public static class CampaignOptionsManager
{
	private static readonly List<ICampaignOptionProvider> _optionProviders = new List<ICampaignOptionProvider>();

	private static List<ICampaignOptionData> _currentOptions = new List<ICampaignOptionData>();

	public static bool GetOptionWithIdExists(string identifier)
	{
		if (!string.IsNullOrEmpty(identifier))
		{
			return _currentOptions.Any((ICampaignOptionData x) => x.GetIdentifier() == identifier);
		}
		return false;
	}

	public static void Initialize()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			List<Type> typesSafe = assemblies[i].GetTypesSafe();
			for (int j = 0; j < typesSafe.Count; j++)
			{
				Type type = typesSafe[j];
				if (type != null && type != typeof(ICampaignOptionProvider) && typeof(ICampaignOptionProvider).IsAssignableFrom(type))
				{
					ICampaignOptionProvider item = Activator.CreateInstance(type) as ICampaignOptionProvider;
					_optionProviders.Add(item);
				}
			}
		}
	}

	public static void ClearCachedOptions()
	{
		_currentOptions.Clear();
	}

	public static List<ICampaignOptionData> GetGameplayCampaignOptions()
	{
		_currentOptions.Clear();
		for (int num = _optionProviders.Count - 1; num >= 0; num--)
		{
			IEnumerable<ICampaignOptionData> gameplayCampaignOptions = _optionProviders[num].GetGameplayCampaignOptions();
			if (gameplayCampaignOptions != null)
			{
				foreach (ICampaignOptionData item in gameplayCampaignOptions)
				{
					_currentOptions.Add(item);
				}
			}
		}
		return _currentOptions;
	}

	public static List<ICampaignOptionData> GetCharacterCreationCampaignOptions()
	{
		_currentOptions.Clear();
		for (int num = _optionProviders.Count - 1; num >= 0; num--)
		{
			IEnumerable<ICampaignOptionData> characterCreationCampaignOptions = _optionProviders[num].GetCharacterCreationCampaignOptions();
			if (characterCreationCampaignOptions != null)
			{
				foreach (ICampaignOptionData item in characterCreationCampaignOptions)
				{
					_currentOptions.Add(item);
				}
			}
		}
		return _currentOptions;
	}
}
