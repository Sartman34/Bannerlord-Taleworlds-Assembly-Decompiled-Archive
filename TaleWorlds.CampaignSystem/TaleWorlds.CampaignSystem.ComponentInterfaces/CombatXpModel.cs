using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class CombatXpModel : GameModel
{
	public enum MissionTypeEnum
	{
		Battle,
		PracticeFight,
		Tournament,
		SimulationBattle,
		NoXp
	}

	public abstract float CaptainRadius { get; }

	public abstract SkillObject GetSkillForWeapon(WeaponComponentData weapon, bool isSiegeEngineHit);

	public abstract void GetXpFromHit(CharacterObject attackerTroop, CharacterObject captain, CharacterObject attackedTroop, PartyBase attackerParty, int damage, bool isFatal, MissionTypeEnum missionType, out int xpAmount);

	public abstract float GetXpMultiplierFromShotDifficulty(float shotDifficulty);
}
