using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade;

public abstract class AgentComponent
{
	protected readonly Agent Agent;

	protected AgentComponent(Agent agent)
	{
		Agent = agent;
	}

	public virtual void Initialize()
	{
	}

	public virtual void OnTickAsAI(float dt)
	{
	}

	public virtual float GetMoraleAddition()
	{
		return 0f;
	}

	public virtual float GetMoraleDecreaseConstant()
	{
		return 1f;
	}

	public virtual void OnItemPickup(SpawnedItemEntity item)
	{
	}

	public virtual void OnWeaponDrop(MissionWeapon droppedWeapon)
	{
	}

	public virtual void OnStopUsingGameObject()
	{
	}

	public virtual void OnWeaponHPChanged(ItemObject item, int hitPoints)
	{
	}

	public virtual void OnRetreating()
	{
	}

	public virtual void OnMount(Agent mount)
	{
	}

	public virtual void OnDismount(Agent mount)
	{
	}

	public virtual void OnHit(Agent affectorAgent, int damage, in MissionWeapon affectorWeapon)
	{
	}

	public virtual void OnDisciplineChanged()
	{
	}

	public virtual void OnAgentRemoved()
	{
	}

	public virtual void OnComponentRemoved()
	{
	}
}
