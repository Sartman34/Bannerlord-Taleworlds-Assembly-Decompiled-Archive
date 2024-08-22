using System;
using System.Runtime.Serialization;
using TaleWorlds.Library;

namespace TaleWorlds.Diamond.Rest;

[Serializable]
[DataContract]
public class RestDataFunctionResult : RestFunctionResult
{
	[DataMember]
	public byte[] FunctionResultData { get; private set; }

	public override FunctionResult GetFunctionResult()
	{
		return (FunctionResult)Common.DeserializeObject(FunctionResultData);
	}

	public RestDataFunctionResult()
	{
	}

	public RestDataFunctionResult(FunctionResult functionResult)
	{
		FunctionResultData = Common.SerializeObject(functionResult);
	}
}
