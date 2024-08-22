using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace NetworkMessages.FromServer;

[DefineGameNetworkMessageType(GameNetworkMessageSendType.FromServer)]
public sealed class MakeAgentDead : GameNetworkMessage
{
	public int AgentIndex { get; private set; }

	public bool IsKilled { get; private set; }

	public ActionIndexValueCache ActionCodeIndex { get; private set; }

	public MakeAgentDead(int agentIndex, bool isKilled, ActionIndexValueCache actionCodeIndex)
	{
		AgentIndex = agentIndex;
		IsKilled = isKilled;
		ActionCodeIndex = actionCodeIndex;
	}

	public MakeAgentDead()
	{
	}

	protected override bool OnRead()
	{
		bool bufferReadValid = true;
		AgentIndex = GameNetworkMessage.ReadAgentIndexFromPacket(ref bufferReadValid);
		IsKilled = GameNetworkMessage.ReadBoolFromPacket(ref bufferReadValid);
		ActionCodeIndex = new ActionIndexValueCache(GameNetworkMessage.ReadIntFromPacket(CompressionBasic.ActionCodeCompressionInfo, ref bufferReadValid));
		return bufferReadValid;
	}

	protected override void OnWrite()
	{
		GameNetworkMessage.WriteAgentIndexToPacket(AgentIndex);
		GameNetworkMessage.WriteBoolToPacket(IsKilled);
		GameNetworkMessage.WriteIntToPacket(ActionCodeIndex.Index, CompressionBasic.ActionCodeCompressionInfo);
	}

	protected override MultiplayerMessageFilter OnGetLogFilter()
	{
		return MultiplayerMessageFilter.EquipmentDetailed;
	}

	protected override string OnGetLogFormat()
	{
		return "Make Agent Dead on Agent with agent-index: " + AgentIndex;
	}
}
