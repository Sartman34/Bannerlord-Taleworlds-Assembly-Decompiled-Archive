namespace TaleWorlds.Core;

public abstract class RidingModel : GameModel
{
	public abstract float CalculateAcceleration(in EquipmentElement mountElement, in EquipmentElement harnessElement, int ridingSkill);
}
