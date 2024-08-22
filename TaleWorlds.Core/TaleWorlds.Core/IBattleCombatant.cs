using System;
using TaleWorlds.Localization;

namespace TaleWorlds.Core;

public interface IBattleCombatant
{
	TextObject Name { get; }

	BattleSideEnum Side { get; }

	BasicCultureObject BasicCulture { get; }

	BasicCharacterObject General { get; }

	Tuple<uint, uint> PrimaryColorPair { get; }

	Tuple<uint, uint> AlternativeColorPair { get; }

	Banner Banner { get; }

	int GetTacticsSkillAmount();
}
