using System;
using System.IO;
using System.Reflection;

namespace TaleWorlds.Library;

public class VirtualFolders
{
	[VirtualDirectory("..")]
	public class Win64_Shipping_Client
	{
		[VirtualDirectory("..")]
		public class bin
		{
			[VirtualDirectory("Parameters")]
			public class Parameters
			{
				[VirtualDirectory("ClientProfiles")]
				public class ClientProfiles
				{
					[VirtualDirectory("DigitalOcean.Discovery")]
					public class DigitalOceanDiscovery
					{
						[VirtualFile("LobbyClient.xml", "<Configuration>\t<SessionProvider Type=\"ThreadedRest\" />\t<Clients>\t\t<Client Type=\"LobbyClient\" />\t</Clients>\t<Parameters>\t\t<Parameter Name=\"LobbyClient.ServiceDiscovery.Address\" Value=\"https://bannerlord-service-discovery.bannerlord-services-3.net/\" />\t\t<Parameter Name=\"LobbyClient.Address\" Value=\"service://bannerlord.lobby/\" />\t</Parameters></Configuration>")]
						public string LobbyClient;
					}
				}

				[VirtualFile("Environment", "pi.YnxRn5TJgdaZMQxsjgeRuS3KMRTs_uY9PEqkN.ZdecHsLpnnAJSb62ZviTNXlmZoCpuWiYB.6_2LgG1bGL6LGPR4SGevSI3cFXduynLR1wK91Lz70zMqaSO6hXx36I.50O1BcjijTgGMu9sy57K9Cagk43L3qoLW9q0Bt27M-")]
				public string Environment;

				[VirtualFile("Version.xml", "<Version>\t<Singleplayer Value=\"v1.2.12\" /></Version>")]
				public string Version;

				[VirtualFile("ClientProfile.xml", "<ClientProfile Value=\"DigitalOcean.Discovery\"/>")]
				public string ClientProfile;
			}
		}
	}

	private static readonly bool _useVirtualFolders = true;

	public static string GetFileContent(string filePath)
	{
		if (!_useVirtualFolders)
		{
			if (!File.Exists(filePath))
			{
				return "";
			}
			return File.ReadAllText(filePath);
		}
		return GetVirtualFileContent(filePath);
	}

	private static string GetVirtualFileContent(string filePath)
	{
		string fileName = Path.GetFileName(filePath);
		string[] array = Path.GetDirectoryName(filePath).Split(new char[1] { Path.DirectorySeparatorChar });
		Type type = typeof(VirtualFolders);
		int num = 0;
		while (type != null && num != array.Length)
		{
			if (!string.IsNullOrEmpty(array[num]))
			{
				type = GetNestedDirectory(array[num], type);
			}
			num++;
		}
		if (type != null)
		{
			FieldInfo[] fields = type.GetFields();
			for (int i = 0; i < fields.Length; i++)
			{
				VirtualFileAttribute[] array2 = (VirtualFileAttribute[])fields[i].GetCustomAttributesSafe(typeof(VirtualFileAttribute), inherit: false);
				if (array2[0].Name == fileName)
				{
					return array2[0].Content;
				}
			}
		}
		return "";
	}

	private static Type GetNestedDirectory(string name, Type type)
	{
		Type[] nestedTypes = type.GetNestedTypes();
		foreach (Type type2 in nestedTypes)
		{
			if (((VirtualDirectoryAttribute[])type2.GetCustomAttributesSafe(typeof(VirtualDirectoryAttribute), inherit: false))[0].Name == name)
			{
				return type2;
			}
		}
		return null;
	}
}
