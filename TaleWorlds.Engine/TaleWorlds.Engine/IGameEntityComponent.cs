using TaleWorlds.Library;

namespace TaleWorlds.Engine;

[ApplicationInterfaceBase]
internal interface IGameEntityComponent
{
	[EngineMethod("get_entity", false)]
	GameEntity GetEntity(GameEntityComponent entityComponent);

	[EngineMethod("get_first_meta_mesh", false)]
	MetaMesh GetFirstMetaMesh(GameEntityComponent entityComponent);
}
