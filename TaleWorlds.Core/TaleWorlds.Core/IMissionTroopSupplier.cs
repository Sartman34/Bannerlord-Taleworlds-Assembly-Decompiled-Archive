using System.Collections.Generic;

namespace TaleWorlds.Core;

public interface IMissionTroopSupplier
{
	int NumRemovedTroops { get; }

	int NumTroopsNotSupplied { get; }

	bool AnyTroopRemainsToBeSupplied { get; }

	IEnumerable<IAgentOriginBase> SupplyTroops(int numberToAllocate);

	IEnumerable<IAgentOriginBase> GetAllTroops();

	BasicCharacterObject GetGeneralCharacter();

	int GetNumberOfPlayerControllableTroops();
}
