using System;

namespace TaleWorlds.Core;

[Flags]
public enum EquipmentFlags : uint
{
	None = 0u,
	IsWandererEquipment = 1u,
	IsGentryEquipment = 2u,
	IsRebelHeroEquipment = 4u,
	IsNoncombatantTemplate = 8u,
	IsCombatantTemplate = 0x10u,
	IsCivilianTemplate = 0x20u,
	IsNobleTemplate = 0x40u,
	IsFemaleTemplate = 0x80u,
	IsMediumTemplate = 0x100u,
	IsHeavyTemplate = 0x200u,
	IsFlamboyantTemplate = 0x400u,
	IsStoicTemplate = 0x800u,
	IsNomadTemplate = 0x1000u,
	IsWoodlandTemplate = 0x2000u,
	IsChildEquipmentTemplate = 0x4000u,
	IsTeenagerEquipmentTemplate = 0x8000u
}
