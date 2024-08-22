namespace TaleWorlds.Core;

public abstract class ItemValueModel : GameModel
{
	public abstract float GetEquipmentValueFromTier(float itemTierf);

	public abstract float CalculateTier(ItemObject item);

	public abstract int CalculateValue(ItemObject item);
}
