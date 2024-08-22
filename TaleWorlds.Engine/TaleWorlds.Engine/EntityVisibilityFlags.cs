using System;

namespace TaleWorlds.Engine;

[Flags]
public enum EntityVisibilityFlags
{
	None = 0,
	VisibleOnlyWhenEditing = 2,
	NoShadow = 4,
	VisibleOnlyForEnvmap = 8,
	NotVisibleForEnvmap = 0x10
}
