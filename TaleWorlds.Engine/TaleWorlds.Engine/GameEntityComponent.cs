using System;
using TaleWorlds.DotNet;

namespace TaleWorlds.Engine;

[EngineClass("rglEntity_component")]
public abstract class GameEntityComponent : NativeObject
{
	internal GameEntityComponent(UIntPtr pointer)
	{
		Construct(pointer);
	}

	public GameEntity GetEntity()
	{
		return EngineApplicationInterface.IGameEntityComponent.GetEntity(this);
	}

	public virtual MetaMesh GetFirstMetaMesh()
	{
		return EngineApplicationInterface.IGameEntityComponent.GetFirstMetaMesh(this);
	}
}
