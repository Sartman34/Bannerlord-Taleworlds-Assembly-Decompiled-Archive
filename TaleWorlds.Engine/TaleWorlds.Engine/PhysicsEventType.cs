using TaleWorlds.DotNet;

namespace TaleWorlds.Engine;

[EngineStruct("Physics_event_type", false)]
public enum PhysicsEventType
{
	CollisionStart,
	CollisionStay,
	CollisionEnd
}
