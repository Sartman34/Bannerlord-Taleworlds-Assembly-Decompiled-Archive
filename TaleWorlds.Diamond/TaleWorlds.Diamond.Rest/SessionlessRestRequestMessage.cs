using System;
using System.Runtime.Serialization;

namespace TaleWorlds.Diamond.Rest;

[Serializable]
[DataContract]
public abstract class SessionlessRestRequestMessage : RestData
{
}
