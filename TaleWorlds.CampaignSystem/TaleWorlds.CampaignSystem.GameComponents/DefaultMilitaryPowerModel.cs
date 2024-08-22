using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultMilitaryPowerModel : MilitaryPowerModel
{
	[Flags]
	public enum PowerFlags
	{
		Invalid = 0,
		Attacker = 1,
		Defender = 2,
		Siege = 4,
		Village = 8,
		RiverCrossing = 0x10,
		Forest = 0x20,
		Flat = 0x40,
		Infantry = 0x80,
		Archer = 0x100,
		Cavalry = 0x200,
		HorseArcher = 0x400
	}

	private const float LowTierCaptainPerkPowerBoost = 0.01f;

	private const float MidTierCaptainPerkPowerBoost = 0.02f;

	private const float HighTierCaptainPerkPowerBoost = 0.03f;

	private const float UltraTierCaptainPerkPowerBoost = 0.06f;

	private static Dictionary<PowerFlags, float> _battleModifiers = new Dictionary<PowerFlags, float>
	{
		{
			PowerFlags.Attacker | PowerFlags.Siege | PowerFlags.Infantry,
			0f
		},
		{
			PowerFlags.Attacker | PowerFlags.Village | PowerFlags.Infantry,
			0.05f
		},
		{
			PowerFlags.Attacker | PowerFlags.RiverCrossing | PowerFlags.Infantry,
			0f
		},
		{
			PowerFlags.Attacker | PowerFlags.Forest | PowerFlags.Infantry,
			0.05f
		},
		{
			PowerFlags.Attacker | PowerFlags.Flat | PowerFlags.Infantry,
			0f
		},
		{
			PowerFlags.Defender | PowerFlags.Siege | PowerFlags.Infantry,
			0f
		},
		{
			PowerFlags.Defender | PowerFlags.Village | PowerFlags.Infantry,
			0.05f
		},
		{
			PowerFlags.Defender | PowerFlags.RiverCrossing | PowerFlags.Infantry,
			0.05f
		},
		{
			PowerFlags.Defender | PowerFlags.Forest | PowerFlags.Infantry,
			0.05f
		},
		{
			PowerFlags.Defender | PowerFlags.Flat | PowerFlags.Infantry,
			0f
		},
		{
			PowerFlags.Attacker | PowerFlags.Siege | PowerFlags.Archer,
			-0.2f
		},
		{
			PowerFlags.Attacker | PowerFlags.Village | PowerFlags.Archer,
			-0.1f
		},
		{
			PowerFlags.Attacker | PowerFlags.RiverCrossing | PowerFlags.Archer,
			0f
		},
		{
			PowerFlags.Attacker | PowerFlags.Forest | PowerFlags.Archer,
			-0.1f
		},
		{
			PowerFlags.Attacker | PowerFlags.Flat | PowerFlags.Archer,
			0f
		},
		{
			PowerFlags.Defender | PowerFlags.Siege | PowerFlags.Archer,
			0.3f
		},
		{
			PowerFlags.Defender | PowerFlags.Village | PowerFlags.Archer,
			0.05f
		},
		{
			PowerFlags.Defender | PowerFlags.RiverCrossing | PowerFlags.Archer,
			0.1f
		},
		{
			PowerFlags.Defender | PowerFlags.Forest | PowerFlags.Archer,
			-0.5f
		},
		{
			PowerFlags.Defender | PowerFlags.Flat | PowerFlags.Archer,
			0f
		},
		{
			PowerFlags.Attacker | PowerFlags.Siege | PowerFlags.Cavalry,
			-0.1f
		},
		{
			PowerFlags.Attacker | PowerFlags.Village | PowerFlags.Cavalry,
			0f
		},
		{
			PowerFlags.Attacker | PowerFlags.RiverCrossing | PowerFlags.Cavalry,
			-0.15f
		},
		{
			PowerFlags.Attacker | PowerFlags.Forest | PowerFlags.Cavalry,
			-0.2f
		},
		{
			PowerFlags.Attacker | PowerFlags.Flat | PowerFlags.Cavalry,
			0.25f
		},
		{
			PowerFlags.Defender | PowerFlags.Siege | PowerFlags.Cavalry,
			-0.1f
		},
		{
			PowerFlags.Defender | PowerFlags.Village | PowerFlags.Cavalry,
			-0.1f
		},
		{
			PowerFlags.Defender | PowerFlags.RiverCrossing | PowerFlags.Cavalry,
			-0.05f
		},
		{
			PowerFlags.Defender | PowerFlags.Forest | PowerFlags.Cavalry,
			-0.15f
		},
		{
			PowerFlags.Defender | PowerFlags.Flat | PowerFlags.Cavalry,
			0.1f
		},
		{
			PowerFlags.Attacker | PowerFlags.Siege | PowerFlags.HorseArcher,
			-0.2f
		},
		{
			PowerFlags.Attacker | PowerFlags.Village | PowerFlags.HorseArcher,
			0.1f
		},
		{
			PowerFlags.Attacker | PowerFlags.RiverCrossing | PowerFlags.HorseArcher,
			-0.1f
		},
		{
			PowerFlags.Attacker | PowerFlags.Forest | PowerFlags.HorseArcher,
			-0.3f
		},
		{
			PowerFlags.Attacker | PowerFlags.Flat | PowerFlags.HorseArcher,
			0.3f
		},
		{
			PowerFlags.Defender | PowerFlags.Siege | PowerFlags.HorseArcher,
			0.3f
		},
		{
			PowerFlags.Defender | PowerFlags.Village | PowerFlags.HorseArcher,
			0f
		},
		{
			PowerFlags.Defender | PowerFlags.RiverCrossing | PowerFlags.HorseArcher,
			0f
		},
		{
			PowerFlags.Defender | PowerFlags.Forest | PowerFlags.HorseArcher,
			-0.25f
		},
		{
			PowerFlags.Defender | PowerFlags.Flat | PowerFlags.HorseArcher,
			0.15f
		}
	};

	public static void ChangeExistingBattleModifiers(List<(PowerFlags, float)> newBattleModifiers)
	{
		foreach (var newBattleModifier in newBattleModifiers)
		{
			if (_battleModifiers.ContainsKey(newBattleModifier.Item1))
			{
				_battleModifiers[newBattleModifier.Item1] = newBattleModifier.Item2;
			}
		}
	}

	public override float GetTroopPower(float defaultTroopPower, float leaderModifier = 0f, float contextModifier = 0f)
	{
		return defaultTroopPower * (1f + leaderModifier + contextModifier);
	}

	public override float GetTroopPower(CharacterObject troop, BattleSideEnum side, MapEvent.PowerCalculationContext context, float leaderModifier)
	{
		float defaultTroopPower = Campaign.Current.Models.MilitaryPowerModel.GetDefaultTroopPower(troop);
		float contextModifier = Campaign.Current.Models.MilitaryPowerModel.GetContextModifier(troop, side, context);
		return Campaign.Current.Models.MilitaryPowerModel.GetTroopPower(defaultTroopPower, leaderModifier, contextModifier);
	}

	public override float GetLeaderModifierInMapEvent(MapEvent mapEvent, BattleSideEnum battleSideEnum)
	{
		Hero hero = ((battleSideEnum == BattleSideEnum.Attacker) ? mapEvent.AttackerSide.LeaderParty.LeaderHero : mapEvent.DefenderSide.LeaderParty.LeaderHero);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		if (hero != null)
		{
			foreach (PerkObject item in PerkObject.All)
			{
				if (item.PrimaryRole == SkillEffect.PerkRole.Captain && hero.GetPerkValue(item))
				{
					float num5 = item.RequiredSkillValue / (float)Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus;
					if (num5 <= 0.3f)
					{
						num++;
					}
					else if (num5 <= 0.6f)
					{
						num2++;
					}
					else if (num5 <= 0.9f)
					{
						num3++;
					}
					else
					{
						num4++;
					}
				}
			}
		}
		return (float)num * 0.01f + (float)num2 * 0.02f + (float)num3 * 0.03f + (float)num4 * 0.06f;
	}

	public override float GetContextModifier(CharacterObject troop, BattleSideEnum battleSideEnum, MapEvent.PowerCalculationContext context)
	{
		PowerFlags powerFlags = PowerFlags.Invalid;
		powerFlags = (troop.HasMount() ? (powerFlags | (troop.IsRanged ? PowerFlags.HorseArcher : PowerFlags.Cavalry)) : ((!troop.IsRanged) ? PowerFlags.Infantry : PowerFlags.Archer));
		switch (context)
		{
		case MapEvent.PowerCalculationContext.Default:
		case MapEvent.PowerCalculationContext.PlainBattle:
		case MapEvent.PowerCalculationContext.SteppeBattle:
		case MapEvent.PowerCalculationContext.DesertBattle:
		case MapEvent.PowerCalculationContext.DuneBattle:
		case MapEvent.PowerCalculationContext.SnowBattle:
			powerFlags |= PowerFlags.Flat;
			break;
		case MapEvent.PowerCalculationContext.ForestBattle:
			powerFlags |= PowerFlags.Forest;
			break;
		case MapEvent.PowerCalculationContext.RiverCrossingBattle:
			powerFlags |= PowerFlags.RiverCrossing;
			break;
		case MapEvent.PowerCalculationContext.Village:
			powerFlags |= PowerFlags.Village;
			break;
		case MapEvent.PowerCalculationContext.Siege:
			powerFlags |= PowerFlags.Siege;
			break;
		}
		powerFlags |= ((battleSideEnum == BattleSideEnum.Attacker) ? PowerFlags.Attacker : PowerFlags.Defender);
		return _battleModifiers[powerFlags];
	}

	public override float GetDefaultTroopPower(CharacterObject troop)
	{
		int num = (troop.IsHero ? (troop.HeroObject.Level / 4 + 1) : troop.Tier);
		return (float)((2 + num) * (10 + num)) * 0.02f * (troop.IsHero ? 1.5f : (troop.IsMounted ? 1.2f : 1f));
	}
}
