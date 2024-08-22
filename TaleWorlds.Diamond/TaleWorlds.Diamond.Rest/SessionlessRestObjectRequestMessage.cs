using System;
using System.Runtime.Serialization;

namespace TaleWorlds.Diamond.Rest;

[Serializable]
[DataContract]
public class SessionlessRestObjectRequestMessage : SessionlessRestRequestMessage
{
	[DataMember]
	public MessageType MessageType { get; private set; }

	[DataMember]
	public Message Message { get; private set; }

	public SessionlessRestObjectRequestMessage()
	{
	}

	public SessionlessRestObjectRequestMessage(Message message, MessageType messageType)
	{
		Message = message;
		MessageType = messageType;
	}
}
