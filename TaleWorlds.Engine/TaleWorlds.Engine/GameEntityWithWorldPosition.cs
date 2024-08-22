using System;
using TaleWorlds.DotNet;
using TaleWorlds.Library;

namespace TaleWorlds.Engine;

public class GameEntityWithWorldPosition
{
	private readonly WeakNativeObjectReference _gameEntity;

	private WorldPosition _worldPosition;

	private Mat3 _orthonormalRotation;

	public GameEntity GameEntity => _gameEntity?.GetNativeObject() as GameEntity;

	public WorldPosition WorldPosition
	{
		get
		{
			Vec3 origin = GameEntity.GetGlobalFrame().origin;
			if (!_worldPosition.AsVec2.NearlyEquals(origin.AsVec2))
			{
				_worldPosition.SetVec3(UIntPtr.Zero, origin, hasValidZ: false);
			}
			return _worldPosition;
		}
	}

	public WorldFrame WorldFrame
	{
		get
		{
			Mat3 rotation = GameEntity.GetGlobalFrame().rotation;
			if (!rotation.NearlyEquals(_orthonormalRotation))
			{
				_orthonormalRotation = rotation;
				_orthonormalRotation.Orthonormalize();
			}
			return new WorldFrame(_orthonormalRotation, WorldPosition);
		}
	}

	public GameEntityWithWorldPosition(GameEntity gameEntity)
	{
		_gameEntity = new WeakNativeObjectReference(gameEntity);
		Scene scene = gameEntity.Scene;
		float groundHeightAtPosition = scene.GetGroundHeightAtPosition(gameEntity.GlobalPosition);
		_worldPosition = new WorldPosition(scene, UIntPtr.Zero, new Vec3(gameEntity.GlobalPosition.AsVec2, groundHeightAtPosition), hasValidZ: false);
		_worldPosition.GetGroundVec3();
		_orthonormalRotation = gameEntity.GetGlobalFrame().rotation;
		_orthonormalRotation.Orthonormalize();
	}

	public void InvalidateWorldPosition()
	{
		_worldPosition.State = ZValidityState.Invalid;
	}
}
