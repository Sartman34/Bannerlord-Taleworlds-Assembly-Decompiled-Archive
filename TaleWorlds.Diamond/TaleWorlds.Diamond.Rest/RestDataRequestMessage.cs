using System;
using System.Runtime.Serialization;
using TaleWorlds.Library;

namespace TaleWorlds.Diamond.Rest;

[Serializable]
[DataContract]
public class RestDataRequestMessage : RestRequestMessage
{
	[DataMember]
	public MessageType MessageType { get; private set; }

	[DataMember]
	public SessionCredentials SessionCredentials { get; private set; }

	[DataMember]
	public byte[] MessageData { get; private set; }

	[DataMember]
	public string MessageName { get; private set; }

	public Message GetMessage()
	{
		return (Message)Common.DeserializeObject(MessageData);
	}

	public RestDataRequestMessage()
	{
	}

	public RestDataRequestMessage(SessionCredentials sessionCredentials, Message message, MessageType messageType)
	{
		MessageData = Common.SerializeObject(message);
		MessageType = messageType;
		SessionCredentials = sessionCredentials;
		MessageName = message.GetType().Name;
	}

	public override string ToString()
	{
		return "Rest Data Request Message: " + MessageName + "-" + MessageType;
	}
}
