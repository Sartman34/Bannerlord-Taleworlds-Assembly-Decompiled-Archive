using System;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.Core;

public static class MetaDataExtensions
{
	public static DateTime GetCreationTime(this MetaData metaData)
	{
		string text = metaData?["CreationTime"];
		if (text != null)
		{
			if (DateTime.TryParse(text, out var result))
			{
				return result;
			}
			if (long.TryParse(text, out var result2))
			{
				return new DateTime(result2);
			}
		}
		return DateTime.MinValue;
	}

	public static ApplicationVersion GetApplicationVersion(this MetaData metaData)
	{
		string text = metaData?["ApplicationVersion"];
		if (text == null)
		{
			return ApplicationVersion.Empty;
		}
		return ApplicationVersion.FromString(text);
	}

	public static string[] GetModules(this MetaData metaData)
	{
		if (metaData == null || !metaData.TryGetValue("Modules", out var value))
		{
			return new string[0];
		}
		return value.Split(new char[1] { ';' });
	}

	public static ApplicationVersion GetModuleVersion(this MetaData metaData, string moduleName)
	{
		string key = "Module_" + moduleName;
		if (metaData != null && metaData.TryGetValue(key, out var value))
		{
			try
			{
				return ApplicationVersion.FromString(value);
			}
			catch (Exception ex)
			{
				Debug.FailedAssert(ex.Message, "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.Core\\MetaDataExtensions.cs", "GetModuleVersion", 53);
			}
		}
		return ApplicationVersion.Empty;
	}
}
