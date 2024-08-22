using TaleWorlds.Diamond.Rest;
using TaleWorlds.Library.Http;

namespace TaleWorlds.Diamond.ClientApplication;

public class GenericRestSessionlessClientDriverProvider<T> : ISessionlessClientDriverProvider<T> where T : SessionlessClient<T>
{
	private string _address;

	private ushort _port;

	private bool _isSecure;

	private IHttpDriver _httpDriver;

	public GenericRestSessionlessClientDriverProvider(string address, ushort port, bool isSecure, IHttpDriver httpDriver)
	{
		_address = address;
		_port = port;
		_isSecure = isSecure;
		_httpDriver = httpDriver;
	}

	public ISessionlessClientDriver CreateDriver(T sessionlessClient)
	{
		return new SessionlessClientRestDriver(_address, _port, _isSecure, _httpDriver);
	}
}
