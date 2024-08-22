using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace TaleWorlds.ServiceDiscovery.Client;

public static class ServiceAddressManager
{
	[Serializable]
	private class CachedServiceAddress
	{
		public string ServiceName { get; set; }

		public string EnvironmentId { get; set; }

		public ServiceResolvedAddress ResolvedAddress { get; set; }

		public DateTime SavedAt { get; set; }
	}

	private const string ParametersDirectoryName = "Parameters";

	private const string EnvironmentFileName = "Environment";

	private const string CacheDirectoryName = "Data";

	private const string CachedServiceAddressesFileName = "ServiceAddresses.dat";

	private const int ResolveAddressTaskTimeoutDurationInSeconds = 30;

	private const int ServiceAddressExpirationTimeInDays = 7;

	private const int MaxRetryCount = 5;

	private static List<CachedServiceAddress> _serviceAddressCache = new List<CachedServiceAddress>();

	private static string EnvironmentFilePath => Path.Combine(BasePath.Name, "Parameters", "Environment");

	public static void Initalize()
	{
		LoadCache();
	}

	public static void ResolveAddress(string serviceDiscoveryAddress, string serviceAddress, ref string address, ref ushort port, ref bool isSecure)
	{
		if (ServiceAddress.TryGetAddressName(serviceAddress, out var addressName))
		{
			string fileContent = VirtualFolders.GetFileContent(EnvironmentFilePath);
			ServiceResolvedAddress resolvedAddress2;
			if (TryGetCachedServiceAddress(addressName, fileContent, out var resolvedAddress))
			{
				SetServiceAddress(resolvedAddress, ref address, ref port, ref isSecure);
			}
			else if (TryGetRemoteServiceAddress(serviceDiscoveryAddress, addressName, fileContent, out resolvedAddress2))
			{
				SetServiceAddress(resolvedAddress2, ref address, ref port, ref isSecure);
				CacheServiceAddress(addressName, fileContent, resolvedAddress2);
			}
		}
	}

	private static bool TryGetRemoteServiceAddress(string remoteServiceDiscoveryAddress, string serviceName, string environmentId, out ServiceResolvedAddress resolvedAddress)
	{
		IDiscoveryService discoveryService = new RemoteDiscoveryService(remoteServiceDiscoveryAddress);
		for (int i = 0; i < 5; i++)
		{
			Task<ServiceAddress[]> task = Task.Run(() => discoveryService.ResolveService(serviceName, environmentId));
			task.Wait(30000);
			if (task.IsCompleted && task.Result != null)
			{
				resolvedAddress = task.Result?.FirstOrDefault()?.ResolvedAddresses?.FirstOrDefault();
				return resolvedAddress != null;
			}
			Debug.Print($"Couldn't resolve service address, retry count: {i + 1}");
		}
		resolvedAddress = null;
		return false;
	}

	private static bool TryGetCachedServiceAddress(string serviceName, string environmentId, out ServiceResolvedAddress resolvedAddress)
	{
		CachedServiceAddress cachedServiceAddress = _serviceAddressCache.FirstOrDefault((CachedServiceAddress address) => address.ServiceName == serviceName && address.EnvironmentId == environmentId);
		if (cachedServiceAddress != null)
		{
			if (DateTime.UtcNow - cachedServiceAddress.SavedAt < TimeSpan.FromDays(7.0))
			{
				resolvedAddress = cachedServiceAddress.ResolvedAddress;
				return true;
			}
			_serviceAddressCache.Remove(cachedServiceAddress);
		}
		resolvedAddress = null;
		return false;
	}

	private static void SetServiceAddress(ServiceResolvedAddress resolvedAddress, ref string address, ref ushort port, ref bool isSecure)
	{
		if (resolvedAddress != null)
		{
			address = resolvedAddress.Address;
			port = (ushort)resolvedAddress.Port;
			isSecure = resolvedAddress.IsSecure;
		}
	}

	private static void CacheServiceAddress(string serviceAddress, string environmentId, ServiceResolvedAddress resolvedAddress)
	{
		if (resolvedAddress != null)
		{
			_serviceAddressCache.Add(new CachedServiceAddress
			{
				ServiceName = serviceAddress,
				EnvironmentId = environmentId,
				ResolvedAddress = resolvedAddress,
				SavedAt = DateTime.UtcNow
			});
			SaveCache();
		}
	}

	private static void LoadCache()
	{
	}

	private static void SaveCache()
	{
	}
}
