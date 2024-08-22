using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade;

public class BasicBattleAgentOrigin : IAgentOriginBase
{
	private BasicCharacterObject _troop;

	bool IAgentOriginBase.IsUnderPlayersCommand => false;

	uint IAgentOriginBase.FactionColor => 0u;

	uint IAgentOriginBase.FactionColor2 => 0u;

	IBattleCombatant IAgentOriginBase.BattleCombatant => null;

	int IAgentOriginBase.UniqueSeed => 0;

	int IAgentOriginBase.Seed => 0;

	Banner IAgentOriginBase.Banner => null;

	BasicCharacterObject IAgentOriginBase.Troop => _troop;

	public BasicBattleAgentOrigin(BasicCharacterObject troop)
	{
		_troop = troop;
	}

	void IAgentOriginBase.SetWounded()
	{
	}

	void IAgentOriginBase.SetKilled()
	{
	}

	void IAgentOriginBase.SetRouted()
	{
	}

	void IAgentOriginBase.OnAgentRemoved(float agentHealth)
	{
	}

	void IAgentOriginBase.OnScoreHit(BasicCharacterObject victim, BasicCharacterObject captain, int damage, bool isFatal, bool isTeamKill, WeaponComponentData attackerWeapon)
	{
	}

	void IAgentOriginBase.SetBanner(Banner banner)
	{
	}
}
