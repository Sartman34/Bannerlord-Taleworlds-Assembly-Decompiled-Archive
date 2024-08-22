using System;

namespace TaleWorlds.Engine;

[Flags]
public enum VisibilityMaskFlags : uint
{
	Final = 1u,
	ShadowStatic = 0x10u,
	ShadowDynamic = 0x20u,
	Contour = 0x40u,
	EditModeAtmosphere = 0x10000000u,
	EditModeLight = 0x20000000u,
	EditModeParticleSystem = 0x40000000u,
	EditModeHelpers = 0x80000000u,
	EditModeTerrain = 0x1000000u,
	EditModeGameEntity = 0x2000000u,
	EditModeFloraEntity = 0x4000000u,
	EditModeLayerFlora = 0x8000000u,
	EditModeShadows = 0x100000u,
	EditModeBorders = 0x200000u,
	EditModeEditingEntity = 0x400000u,
	EditModeAnimations = 0x800000u,
	EditModeAny = 0xFFF00000u,
	Default = 1u,
	DefaultStatic = 0x31u,
	DefaultDynamic = 0x21u,
	DefaultStaticWithoutDynamic = 0x11u
}
