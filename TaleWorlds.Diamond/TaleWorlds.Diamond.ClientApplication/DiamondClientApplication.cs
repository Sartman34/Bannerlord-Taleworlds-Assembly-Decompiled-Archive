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

	private Dictionary<string, ISessionlessClient> _sessionlessClientObjects;

	public ApplicationVersion ApplicationVersion { get; private set; }

	public ParameterContainer Parameters => _parameters;

	public IReadOnlyDictionary<string, string> ProxyAddressMap { get; private set; }

	public DiamondClientApplication(ApplicationVersion applicationVersion, ParameterContainer parameters)
	{
		ApplicationVersion = applicationVersion;
		_parameters = parameters;
		_clientApplicationObjects = new Dictionary<string, DiamondClientApplicationObject>();
		_clientObjects = new Dictionary<string, IClient>();
		_sessionlessClientObjects = new Dictionary<string, ISessionlessClient>();
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
		clients = applicationConfiguration.SessionlessClients;
		foreach (string clientConfiguration2 in clients)
		{
			CreateSessionlessClient(clientConfiguration2, applicationConfiguration.SessionProviderType);
		}
	}

	private void CreateClient(string clientConfiguration, SessionProviderType sessionProviderType)
	{
		Type type = FindType(clientConfiguration);
		object obj = CreateClientSessionProvider(clientConfiguration, type, sessionProviderType, _parameters);
		IClient value = (IClient)Activator.CreateInstance(type, this, obj);
		_clientObjects.Add(clientConfiguration, value);
	}

	private void CreateSessionlessClient(string clientConfiguration, SessionProviderType sessionProviderType)
	{
		Type type = FindType(clientConfiguration);
		object obj = CreateSessionlessClientDriverProvider(clientConfiguration, type, sessionProviderType, _parameters);
		ISessionlessClient value = (ISessionlessClient)Activator.CreateInstance(type, this, obj);
		_sessionlessClientObjects.Add(clientConfiguration, value);
	}

	public object CreateSessionlessClientDriverProvider(string clientName, Type clientType, SessionProviderType sessionProviderType, ParameterContainer parameters)
	{
		object obj = null;
		if (sessionProviderType == SessionProviderType.Rest || sessionProviderType == SessionProviderType.ThreadedRest)
		{
			Type type = typeof(GenericRestSessionlessClientDriverProvider<>).MakeGenericType(clientType);
			parameters.TryGetParameter(clientName + ".Address", out var outValue);
			parameters.TryGetParameterAsUInt16(clientName + ".Port", out var outValue2);
			parameters.TryGetParameterAsBool(clientName + ".IsSecure", out var outValue3);
			if (ServiceAddress.IsServiceAddress(outValue))
			{
				parameters.TryGetParameter(clientName + ".ServiceDiscovery.Address", out var outValue4);
				ServiceAddressManager.ResolveAddress(outValue4, outValue, ref outValue, ref outValue2, ref outValue3);
			}
			IHttpDriver httpDriver = null;
			httpDriver = ((!parameters.TryGetParameter(clientName + ".HttpDriver", out var outValue5)) ? HttpDriverManager.GetDefaultHttpDriver() : HttpDriverManager.GetHttpDriver(outValue5));
			return Activator.CreateInstance(type, outValue, outValue2, outValue3, httpDriver);
		}
		_ = 4;
		throw new NotImplementedException("Other session provider types are not supported yet.");
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
			parameters.TryGetParameterAsUInt16(clientName + ".Port", out var outValue3);
			parameters.TryGetParameterAsBool(clientName + ".IsSecure", out var outValue4);
			if (ServiceAddress.IsServiceAddress(outValue2))
			{
				parameters.TryGetParameter(clientName + ".ServiceDiscovery.Address", out var outValue5);
				ServiceAddressManager.ResolveAddress(outValue5, outValue2, ref outValue2, ref outValue3, ref outValue4);
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
			httpDriver = ((!parameters.TryGetParameter(clientName + ".HttpDriver", out var outValue6)) ? HttpDriverManager.GetDefaultHttpDriver() : HttpDriverManager.GetHttpDriver(outValue6));
			return Activator.CreateInstance(type2, outValue2, outValue3, outValue4, httpDriver);
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

	public T GetSessionlessClient<T>(string name) where T : class, ISessionlessClient
	{
		if (_sessionlessClientObjects.TryGetValue(name, out var value))
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
