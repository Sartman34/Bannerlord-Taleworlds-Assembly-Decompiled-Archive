using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem;

public class BattleSimulationResult
{
	public UniqueTroopDescriptor TroopDescriptor { get; private set; }

	public BattleSideEnum Side { get; private set; }

	public TroopProperty TroopProperty { get; private set; }

	public BattleSimulationResult(UniqueTroopDescriptor troopDescriptor, BattleSideEnum side, TroopProperty troopProperty)
	{
		TroopDescriptor = troopDescriptor;
		Side = side;
		TroopProperty = troopProperty;
	}
}
