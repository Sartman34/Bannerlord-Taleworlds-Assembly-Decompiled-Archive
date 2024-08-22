using System;
using TaleWorlds.Diamond;

namespace Messages.FromBattleServerManager.ToBattleServer;

[Serializable]
[MessageDescription("BattleServerManager", "BattleServer")]
public class TerminateOperationMatchmakingMessage : Message
{
}
