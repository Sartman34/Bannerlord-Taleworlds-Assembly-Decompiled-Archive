using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade.ComponentInterfaces;

public abstract class MissionDifficultyModel : GameModel
{
	public abstract float GetDamageMultiplierOfCombatDifficulty(Agent victimAgent, Agent attackerAgent = null);
}
