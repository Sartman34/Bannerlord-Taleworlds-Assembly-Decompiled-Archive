using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.GameComponents;

public class DefaultRaidModel : RaidModel
{
	private MBReadOnlyList<(ItemObject, float)> _commonLootItems;

	private MBReadOnlyList<(ItemObject, float)> CommonLootItemSpawnChances
	{
		get
		{
			if (_commonLootItems == null)
			{
				List<(ItemObject, float)> list = new List<(ItemObject, float)>
				{
					(DefaultItems.Hides, 1f),
					(DefaultItems.HardWood, 1f),
					(DefaultItems.Tools, 1f),
					(DefaultItems.Grain, 1f),
					(Campaign.Current.ObjectManager.GetObject<ItemObject>("linen"), 1f),
					(Campaign.Current.ObjectManager.GetObject<ItemObject>("sheep"), 1f),
					(Campaign.Current.ObjectManager.GetObject<ItemObject>("mule"), 1f),
					(Campaign.Current.ObjectManager.GetObject<ItemObject>("pottery"), 1f)
				};
				for (int num = list.Count - 1; num >= 0; num--)
				{
					ItemObject item = list[num].Item1;
					float item2 = 100f / ((float)item.Value + 1f);
					list[num] = (item, item2);
				}
				_commonLootItems = new MBReadOnlyList<(ItemObject, float)>(list);
			}
			return _commonLootItems;
		}
	}

	public override int GoldRewardForEachLostHearth => 4;

	public override float CalculateHitDamage(MapEventSide attackerSide, float settlementHitPoints)
	{
		float num = (MathF.Sqrt(attackerSide.TroopCount) + 5f) / 900f * (float)CampaignTime.DeltaTime.ToHours;
		float num2 = 0f;
		foreach (MapEventParty party in attackerSide.Parties)
		{
			if (party.Party.MobileParty?.LeaderHero != null && party.Party.MobileParty.LeaderHero.GetPerkValue(DefaultPerks.Roguery.NoRestForTheWicked))
			{
				num2 += DefaultPerks.Roguery.NoRestForTheWicked.SecondaryBonus;
			}
		}
		return num + num * num2;
	}

	public override MBReadOnlyList<(ItemObject, float)> GetCommonLootItemScores()
	{
		return CommonLootItemSpawnChances;
	}
}
