using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Source.Missions;

public class DebugObjectDestroyerMissionController : MissionLogic
{
	private GameEntity _contouredEntity;

	public override void OnMissionTick(float dt)
	{
		base.OnMissionTick(dt);
		Vec3 lastFinalRenderCameraPosition = base.Mission.Scene.LastFinalRenderCameraPosition;
		Vec3 vec = -base.Mission.Scene.LastFinalRenderCameraFrame.rotation.u;
		float collisionDistance;
		GameEntity collidedEntity;
		bool flag = Mission.Current.Scene.RayCastForClosestEntityOrTerrain(lastFinalRenderCameraPosition, lastFinalRenderCameraPosition + vec * 100f, out collisionDistance, out collidedEntity, 0.01f, BodyFlags.OnlyCollideWithRaycast);
		if (Input.DebugInput.IsShiftDown() && Agent.Main != null && collidedEntity != null && !collidedEntity.HasScriptOfType<DestructableComponent>())
		{
			foreach (DestructableComponent item in Mission.Current.ActiveMissionObjects.Where((MissionObject x) => x is DestructableComponent))
			{
				if ((item.GameEntity.GlobalPosition - Agent.Main.Position).Length < 5f)
				{
					collidedEntity = item.GameEntity;
				}
			}
		}
		GameEntity gameEntity = null;
		if (flag && (Input.DebugInput.IsKeyDown(InputKey.MiddleMouseButton) || Input.DebugInput.IsKeyReleased(InputKey.MiddleMouseButton)))
		{
			Vec3 vec2 = lastFinalRenderCameraPosition + vec * collisionDistance;
			if (collidedEntity == null)
			{
				return;
			}
			bool flag2 = Input.DebugInput.IsKeyReleased(InputKey.MiddleMouseButton);
			int weaponKind = 0;
			if (flag2)
			{
				weaponKind = (int)(Input.DebugInput.IsAltDown() ? Game.Current.ObjectManager.GetObject<ItemObject>("boulder").Id.InternalValue : ((!Input.DebugInput.IsControlDown()) ? Game.Current.ObjectManager.GetObject<ItemObject>("ballista_projectile").Id.InternalValue : Game.Current.ObjectManager.GetObject<ItemObject>("pot").Id.InternalValue));
			}
			GameEntity gameEntity2 = collidedEntity;
			DestructableComponent destructableComponent2 = null;
			while (destructableComponent2 == null && gameEntity2 != null)
			{
				destructableComponent2 = gameEntity2.GetFirstScriptOfType<DestructableComponent>();
				gameEntity2 = gameEntity2.Parent;
			}
			if (destructableComponent2 != null && !destructableComponent2.IsDestroyed)
			{
				if (flag2)
				{
					if (Agent.Main != null)
					{
						DestructableComponent destructableComponent3 = destructableComponent2;
						Agent main = Agent.Main;
						Vec3 impactPosition = vec2 - vec * 0.1f;
						MissionWeapon weapon = new MissionWeapon(ItemObject.GetItemFromWeaponKind(weaponKind), null, null);
						destructableComponent3.TriggerOnHit(main, 400, impactPosition, vec, in weapon, null);
					}
				}
				else
				{
					gameEntity = destructableComponent2.GameEntity;
				}
			}
		}
		if (gameEntity != _contouredEntity && _contouredEntity != null)
		{
			_contouredEntity.SetContourColor(null);
		}
		_contouredEntity = gameEntity;
		if (_contouredEntity != null)
		{
			_contouredEntity.SetContourColor(4294967040u);
		}
	}
}
