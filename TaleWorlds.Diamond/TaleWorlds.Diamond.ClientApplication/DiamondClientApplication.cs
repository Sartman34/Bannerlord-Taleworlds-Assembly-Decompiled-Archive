using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using TaleWorlds.Diamond.InnerProcess;
using TaleWorlds.Library;
using TaleWorlds.Library.Http;
using TaleWorlds.ServiceDiscovery.Client;

namespace TaleWorlds.Diamond.ClientApplication;

public class DiamondClientApplication
{
	private ParameterContainer _parameters;

	private Dictionary<string, DiamondClientApplicationObject> _clientApplicationObjects;

	private Dictionary<string, IClient> _clientObjects;

	public ApplicationVersion ApplicationVersion { get; private set; }

	public ParameterContainer Parameters => _parameters;

	public IReadOnlyDictionary<string, string> ProxyAddressMap { get; private set; }

	public DiamondClientApplication(ApplicationVersion applicationVersion, ParameterContainer parameters)
	{
		ApplicationVersion = applicationVersion;
		_parameters = parameters;
		_clientApplicationObjects = new Dictionary<string, DiamondClientApplicationObject>();
		_clientObjects = new Dictionary<string, IClient>();
		ProxyAddressMap = new Dictionary<string, string>();
		ServicePointManager.DefaultConnectionLimit = 1000;
		ServicePointManager.Expect100Continue = false;
	}

	public DiamondClientApplication(ApplicationVersion applicationVersion)
		: this(applicationVersion, new ParameterContainer())
	{
	}

	public object GetObject(string name)
	{
		_clientApplicationObjects.TryGetValue(name, out var value);
		return value;
	}

	public void AddObject(string name, DiamondClientApplicationObject applicationObject)
	{
		_clientApplicationObjects.Add(name, applicationObject);
	}

	public void Initialize(ClientApplicationConfiguration applicationConfiguration)
	{
		_parameters = applicationConfiguration.Parameters;
		string[] clients = applicationConfiguration.Clients;
		foreach (string clientConfiguration in clients)
		{
			CreateClient(clientConfiguration, applicationConfiguration.SessionProviderType);
		}
	}

	private void CreateClient(string clientConfiguration, SessionProviderType sessionProviderType)
	{
		Type type = FindType(clientConfiguration);
		object obj = CreateClientSessionProvider(clientConfiguration, type, sessionProviderType, _parameters);
		IClient value = (IClient)Activator.CreateInstance(type, this, obj);
		_clientObjects.Add(clientConfiguration, value);
	}

	public object CreateClientSessionProvider(string clientName, Type clientType, SessionProviderType sessionProviderType, ParameterContainer parameters)
	{
		object obj = null;
		switch (sessionProviderType)
		{
		case SessionProviderType.Rest:
		case SessionProviderType.ThreadedRest:
		{
			Type type2 = ((sessionProviderType == SessionProviderType.Rest) ? typeof(GenericRestSessionProvider<>) : typeof(GenericThreadedRestSessionProvider<>)).MakeGenericType(clientType);
			parameters.TryGetParameter(clientName + ".Address", out var outValue2);
			if (ServiceAddress.IsServiceAddress(outValue2))
			{
				parameters.TryGetParameter(clientName + ".ServiceDiscovery.Address", out var outValue3);
				ServiceAddressManager.ResolveAddress(outValue3, ref outValue2);
			}
			string text = clientName + ".Proxy.";
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> item in parameters.Iterator)
			{
				if (item.Key.StartsWith(text) && item.Key.Length > text.Length)
				{
					dictionary[item.Key.Substring(text.Length)] = item.Value;
				}
			}
			ProxyAddressMap = dictionary;
			if (dictionary.TryGetValue(outValue2, out var value))
			{
				outValue2 = value;
			}
			IHttpDriver httpDriver = null;
			httpDriver = ((!parameters.TryGetParameter(clientName + ".HttpDriver", out var outValue4)) ? HttpDriverManager.GetDefaultHttpDriver() : HttpDriverManager.GetHttpDriver(outValue4));
			return Activator.CreateInstance(type2, outValue2, httpDriver);
		}
		case SessionProviderType.InnerProcess:
		{
			InnerProcessManager innerProcessManager = ((InnerProcessManagerClientObject)GetObject("InnerProcessManager")).InnerProcessManager;
			Type type = typeof(GenericInnerProcessSessionProvider<>).MakeGenericType(clientType);
			parameters.TryGetParameterAsUInt16(clientName + ".Port", out var outValue);
			return Activator.CreateInstance(type, innerProcessManager, outValue);
		}
		default:
			throw new NotImplementedException("Other session provider types are not supported yet.");
		}
	}

	private static Assembly[] GetDiamondAssemblies()
	{
		List<Assembly> list = new List<Assembly>();
		Assembly assembly = typeof(PeerId).Assembly;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		list.Add(assembly);
		Assembly[] array = assemblies;
		foreach (Assembly assembly2 in array)
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

	private static Type FindType(string name)
	{
		Assembly[] diamondAssemblies = GetDiamondAssemblies();
		Type result = null;
		Assembly[] array = diamondAssemblies;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Type item in array[i].GetTypesSafe())
			{
				if (item.Name == name)
				{
					result = item;
				}
			}
		}
		return result;
	}

	public T GetClient<T>(string name) where T : class, IClient
	{
		if (_clientObjects.TryGetValue(name, out var value))
		{
			return value as T;
		}
		return null;
	}

	public void Update()
	{
		foreach (IClient value in _clientObjects.Values)
		{
			_ = value;
		}
	}
}
