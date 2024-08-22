using System;
using System.Runtime.Serialization;
using TaleWorlds.Diamond;

namespace Messages.FromBattleServerManager.ToBattleServer;

[Serializable]
[MessageDescription("BattleServerManager", "BattleServer")]
[DataContract]
public class BattleServerReadyResponseMessage : LoginResultObject
{
}
