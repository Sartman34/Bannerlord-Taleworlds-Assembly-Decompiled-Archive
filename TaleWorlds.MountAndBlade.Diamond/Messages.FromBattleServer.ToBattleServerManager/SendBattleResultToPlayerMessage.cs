using System;
using TaleWorlds.Diamond;

namespace Messages.FromBattleServer.ToBattleServerManager;

[Serializable]
[MessageDescription("BattleServer", "BattleServerManager")]
public class SendBattleResultToPlayerMessage : Message
{
}
