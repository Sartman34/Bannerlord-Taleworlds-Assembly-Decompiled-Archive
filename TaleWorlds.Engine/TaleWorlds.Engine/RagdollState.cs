using TaleWorlds.DotNet;

namespace TaleWorlds.Engine;

[EngineStruct("rglRagdoll::Ragdoll_state", false)]
public enum RagdollState : ushort
{
	Disabled,
	NeedsActivation,
	ActiveFirstTick,
	Active,
	NeedsDeactivation
}
