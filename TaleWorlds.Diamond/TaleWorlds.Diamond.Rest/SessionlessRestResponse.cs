using System;
using System.Runtime.Serialization;

namespace TaleWorlds.Diamond.Rest;

[Serializable]
[DataContract]
public sealed class SessionlessRestResponse : RestData
{
	[DataMember]
	public bool Successful { get; private set; }

	[DataMember]
	public string SuccessfulReason { get; private set; }

	[DataMember]
	public RestFunctionResult FunctionResult { get; set; }

	public void SetSuccessful(bool successful, string succressfulReason)
	{
		Successful = successful;
		SuccessfulReason = succressfulReason;
	}
}
