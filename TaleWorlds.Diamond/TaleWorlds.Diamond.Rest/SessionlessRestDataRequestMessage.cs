using System;
using System.Runtime.Serialization;
using TaleWorlds.Library;

namespace TaleWorlds.Diamond.Rest;

[Serializable]
[DataContract]
public class SessionlessRestDataRequestMessage : SessionlessRestRequestMessage
{
	[DataMember]
	public MessageType MessageType { get; private set; }

	[DataMember]
	public byte[] MessageData { get; private set; }

	public Message GetMessage()
	{
		return (Message)Common.DeserializeObject(MessageData);
	}

	public SessionlessRestDataRequestMessage()
	{
	}

	public SessionlessRestDataRequestMessage(Message message, MessageType messageType)
	{
		MessageData = Common.SerializeObject(message);
		MessageType = messageType;
	}
}
