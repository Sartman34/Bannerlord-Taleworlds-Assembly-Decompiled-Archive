using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ComponentInterfaces;

public abstract class MilitaryPowerModel : GameModel
{
	public abstract float GetTroopPower(float defaultTroopPower, float leaderModifier = 0f, float contextModifier = 0f);

	public abstract float GetTroopPower(CharacterObject troop, BattleSideEnum side, MapEvent.PowerCalculationContext context, float leaderModifier);

	public abstract float GetContextModifier(CharacterObject troop, BattleSideEnum battleSideEnum, MapEvent.PowerCalculationContext context);

	public abstract float GetLeaderModifierInMapEvent(MapEvent mapEvent, BattleSideEnum battleSideEnum);

	public abstract float GetDefaultTroopPower(CharacterObject troop);
}
