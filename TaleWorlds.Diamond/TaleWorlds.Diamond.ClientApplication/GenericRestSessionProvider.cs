using TaleWorlds.Diamond.Rest;
using TaleWorlds.Library.Http;

namespace TaleWorlds.Diamond.ClientApplication;

public class GenericRestSessionProvider<T> : IClientSessionProvider<T> where T : Client<T>
{
	private string _address;

	private ushort _port;

	private bool _isSecure;

	private IHttpDriver _httpDriver;

	public GenericRestSessionProvider(string address, ushort port, bool isSecure, IHttpDriver httpDriver)
	{
		_address = address;
		_port = port;
		_isSecure = isSecure;
		_httpDriver = httpDriver;
	}

	public IClientSession CreateSession(T session)
	{
		return new ClientRestSession(session, _address, _port, _isSecure, _httpDriver);
	}
}
