using TaleWorlds.DotNet;

namespace TaleWorlds.Engine;

[EngineStruct("rglWorld_position::z_validity_state", false)]
public enum ZValidityState
{
	Invalid,
	BatchFormationUnitPosition,
	ValidAccordingToNavMesh,
	Valid
}
