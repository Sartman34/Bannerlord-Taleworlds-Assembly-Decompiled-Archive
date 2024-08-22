using System.Collections.Generic;

namespace TaleWorlds.CampaignSystem;

public class BattleSimulationResultArgs
{
	public List<BattleSimulationResult> RoundResults;

	public BattleSimulationResultArgs()
	{
		RoundResults = new List<BattleSimulationResult>();
	}
}
