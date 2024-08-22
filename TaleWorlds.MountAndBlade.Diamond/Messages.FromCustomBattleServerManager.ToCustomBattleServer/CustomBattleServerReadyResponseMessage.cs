using System;
using System.Runtime.Serialization;
using TaleWorlds.Diamond;

namespace Messages.FromCustomBattleServerManager.ToCustomBattleServer;

[Serializable]
[MessageDescription("CustomBattleServerManager", "CustomBattleServer")]
[DataContract]
public class CustomBattleServerReadyResponseMessage : LoginResultObject
{
}
