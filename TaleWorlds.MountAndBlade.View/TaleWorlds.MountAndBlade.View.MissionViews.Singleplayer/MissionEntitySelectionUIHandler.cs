using System;
using System.Diagnostics;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

public class MissionEntitySelectionUIHandler : MissionView
{
	private Action<GameEntity> onSelect;

	private Action<GameEntity> onHover;

	public MissionEntitySelectionUIHandler(Action<GameEntity> onSelect = null, Action<GameEntity> onHover = null)
	{
		this.onSelect = onSelect;
		this.onHover = onHover;
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		GameEntity value = new Lazy<GameEntity>(GetCollidedEntity).Value;
		onHover?.Invoke(value);
		if (base.Input.IsKeyReleased(InputKey.LeftMouseButton))
		{
			onSelect?.Invoke(value);
		}
	}

	private GameEntity GetCollidedEntity()
	{
		Vec2 mousePositionRanged = base.Input.GetMousePositionRanged();
		base.MissionScreen.ScreenPointToWorldRay(mousePositionRanged, out var rayBegin, out var rayEnd);
		using (new TWSharedMutexUpgradeableReadLock(Scene.PhysicsAndRayCastLock))
		{
			if (Mission.Current != null)
			{
				Mission.Current.Scene.RayCastForClosestEntityOrTerrainMT(rayBegin, rayEnd, out float _, out GameEntity collidedEntity, 0.3f, BodyFlags.CommonFocusRayCastExcludeFlags);
				while (collidedEntity != null && collidedEntity.Parent != null)
				{
					collidedEntity = collidedEntity.Parent;
				}
				return collidedEntity;
			}
			return null;
		}
	}

	public override void OnRemoveBehavior()
	{
		onSelect = null;
		onHover = null;
		base.OnRemoveBehavior();
	}

	[Conditional("DEBUG")]
	public void TickDebug()
	{
		GameEntity collidedEntity = GetCollidedEntity();
		if (!(collidedEntity == null))
		{
			_ = collidedEntity.Name;
		}
	}
}
