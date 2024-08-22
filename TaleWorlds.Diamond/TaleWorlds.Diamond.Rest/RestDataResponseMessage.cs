using System;
using System.Runtime.Serialization;
using TaleWorlds.Library;

namespace TaleWorlds.Diamond.Rest;

[Serializable]
[DataContract]
public class RestDataResponseMessage : RestResponseMessage
{
	[DataMember]
	public byte[] MessageData { get; private set; }

	public override Message GetMessage()
	{
		return (Message)Common.DeserializeObject(MessageData);
	}

	public RestDataResponseMessage()
	{
	}

	public RestDataResponseMessage(Message message)
	{
		MessageData = Common.SerializeObject(message);
	}
}
