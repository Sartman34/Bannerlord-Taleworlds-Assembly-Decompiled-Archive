using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class DifficultyModel : GameModel
{
	public abstract float GetPlayerTroopsReceivedDamageMultiplier();

	public abstract float GetDamageToPlayerMultiplier();

	public abstract int GetPlayerRecruitSlotBonus();

	public abstract float GetPlayerMapMovementSpeedBonusMultiplier();

	public abstract float GetCombatAIDifficultyMultiplier();

	public abstract float GetPersuasionBonusChance();

	public abstract float GetClanMemberDeathChanceMultiplier();
}
