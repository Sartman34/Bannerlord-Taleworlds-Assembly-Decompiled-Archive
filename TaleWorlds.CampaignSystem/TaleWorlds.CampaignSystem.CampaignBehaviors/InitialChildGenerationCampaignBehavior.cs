using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class InitialChildGenerationCampaignBehavior : CampaignBehaviorBase
{
	private const float FemaleChildrenChance = 0.49f;

	public override void RegisterEvents()
	{
		CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener(this, OnNewGameCreatedPartialFollowUp);
	}

	private void OnNewGameCreatedPartialFollowUp(CampaignGameStarter starter, int index)
	{
		if (index != 0)
		{
			return;
		}
		foreach (Clan clan in Clan.All)
		{
			if (clan.IsBanditFaction || clan.IsMinorFaction || clan.IsEliminated || clan == Clan.PlayerClan)
			{
				continue;
			}
			List<Hero> list = new List<Hero>();
			MBList<Hero> mBList = new MBList<Hero>();
			MBList<Hero> mBList2 = new MBList<Hero>();
			foreach (Hero lord in clan.Lords)
			{
				if (lord.IsAlive)
				{
					if (lord.IsChild)
					{
						list.Add(lord);
					}
					else if (lord.IsFemale)
					{
						mBList.Add(lord);
					}
					else
					{
						mBList2.Add(lord);
					}
				}
			}
			int num = MathF.Ceiling((float)(mBList2.Count + mBList.Count) / 2f) - list.Count;
			float num2 = 0.49f;
			if (mBList2.Count == 0)
			{
				num2 = -1f;
			}
			for (int i = 0; i < num; i++)
			{
				bool isFemale = MBRandom.RandomFloat <= num2;
				Hero hero = (isFemale ? mBList.GetRandomElement() : mBList2.GetRandomElement());
				if (hero == null)
				{
					MBList<Clan> e = Clan.NonBanditFactions.Where((Clan t) => t != clan && t.Culture == clan.Culture).ToMBList();
					for (int j = 0; j < 10; j++)
					{
						hero = e.GetRandomElement().Lords.Where((Hero t) => t.IsAlive && t.IsFemale == isFemale).ToMBList().GetRandomElement();
						if (hero != null)
						{
							break;
						}
					}
				}
				if (hero != null)
				{
					int age = MBRandom.RandomInt(2, 18);
					Hero hero2 = HeroCreator.CreateSpecialHero(hero.CharacterObject, clan.HomeSettlement, clan, null, age);
					hero2.UpdateHomeSettlement();
					hero2.HeroDeveloper.InitializeHeroDeveloper(isByNaturalGrowth: true);
					MBEquipmentRoster randomElementInefficiently = Campaign.Current.Models.EquipmentSelectionModel.GetEquipmentRostersForInitialChildrenGeneration(hero2).GetRandomElementInefficiently();
					if (randomElementInefficiently != null)
					{
						Equipment randomCivilianEquipment = randomElementInefficiently.GetRandomCivilianEquipment();
						EquipmentHelper.AssignHeroEquipmentFromEquipment(hero2, randomCivilianEquipment);
						Equipment equipment = new Equipment(isCivilian: false);
						equipment.FillFrom(randomCivilianEquipment, useSourceEquipmentType: false);
						EquipmentHelper.AssignHeroEquipmentFromEquipment(hero2, equipment);
					}
				}
				if (num2 <= 0f)
				{
					num2 = 0.49f;
				}
			}
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
