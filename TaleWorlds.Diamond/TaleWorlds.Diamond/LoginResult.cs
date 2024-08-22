using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TaleWorlds.Diamond;

[Serializable]
[DataContract]
public sealed class LoginResult : FunctionResult
{
	[DataMember]
	public PeerId PeerId { get; private set; }

	[DataMember]
	public SessionKey SessionKey { get; private set; }

	[DataMember]
	public bool Successful { get; private set; }

	[DataMember]
	public string ErrorCode { get; private set; }

	[DataMember]
	public Dictionary<string, string> ErrorParameters { get; private set; }

	[DataMember]
	public string ProviderResponse { get; private set; }

	[DataMember]
	public LoginResultObject LoginResultObject { get; private set; }

	public LoginResult()
	{
	}

	public LoginResult(PeerId peerId, SessionKey sessionKey, LoginResultObject loginResultObject)
	{
		PeerId = peerId;
		SessionKey = sessionKey;
		Successful = true;
		ErrorCode = "";
		LoginResultObject = loginResultObject;
	}

	public LoginResult(PeerId peerId, SessionKey sessionKey)
		: this(peerId, sessionKey, null)
	{
	}

	public LoginResult(string errorCode, Dictionary<string, string> parameters = null)
	{
		ErrorCode = errorCode;
		Successful = false;
		ErrorParameters = parameters;
	}
}
