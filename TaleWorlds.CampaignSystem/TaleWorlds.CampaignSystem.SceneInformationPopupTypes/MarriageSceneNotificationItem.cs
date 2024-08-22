using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.CampaignSystem.SceneInformationPopupTypes;

public class MarriageSceneNotificationItem : SceneNotificationData
{
	private const int NumberOfAudienceHeroes = 6;

	private readonly CampaignTime _creationCampaignTime;

	public Hero GroomHero { get; }

	public Hero BrideHero { get; }

	public override string SceneID => "scn_cutscene_wedding";

	public override TextObject TitleText
	{
		get
		{
			GameTexts.SetVariable("DAY_OF_YEAR", CampaignSceneNotificationHelper.GetFormalDayAndSeasonText(_creationCampaignTime));
			GameTexts.SetVariable("YEAR", _creationCampaignTime.GetYear);
			Hero hero = ((GroomHero == Hero.MainHero) ? GroomHero : BrideHero);
			Hero hero2 = ((hero == GroomHero) ? BrideHero : GroomHero);
			GameTexts.SetVariable("FIRST_HERO", hero.Name);
			GameTexts.SetVariable("SECOND_HERO", hero2.Name);
			return GameTexts.FindText("str_marriage_notification");
		}
	}

	public override RelevantContextType RelevantContext { get; }

	public override IEnumerable<Banner> GetBanners()
	{
		return new List<Banner>
		{
			(GroomHero.Father != null) ? GroomHero.Father.ClanBanner : GroomHero.ClanBanner,
			(BrideHero.Father != null) ? BrideHero.Father.ClanBanner : BrideHero.ClanBanner,
			(GroomHero.Father != null) ? GroomHero.Father.ClanBanner : GroomHero.ClanBanner,
			(BrideHero.Father != null) ? BrideHero.Father.ClanBanner : BrideHero.ClanBanner
		};
	}

	public override IEnumerable<SceneNotificationCharacter> GetSceneNotificationCharacters()
	{
		List<SceneNotificationCharacter> list = new List<SceneNotificationCharacter>();
		Equipment equipment = GroomHero.CivilianEquipment.Clone();
		CampaignSceneNotificationHelper.RemoveWeaponsFromEquipment(ref equipment);
		list.Add(CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(GroomHero, equipment));
		string brideEquipmentIDFromCulture = GetBrideEquipmentIDFromCulture(BrideHero.Culture);
		Equipment equipment2 = MBObjectManager.Instance.GetObject<MBEquipmentRoster>(brideEquipmentIDFromCulture).DefaultEquipment.Clone();
		CampaignSceneNotificationHelper.RemoveWeaponsFromEquipment(ref equipment2);
		list.Add(CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(BrideHero, equipment2));
		CharacterObject @object = MBObjectManager.Instance.GetObject<CharacterObject>("cutscene_monk");
		Equipment equipment3 = @object.Equipment.Clone();
		CampaignSceneNotificationHelper.RemoveWeaponsFromEquipment(ref equipment3);
		list.Add(new SceneNotificationCharacter(@object, equipment3));
		List<Hero> audienceMembers = GetAudienceMembers(BrideHero, GroomHero);
		for (int i = 0; i < audienceMembers.Count; i++)
		{
			Hero hero = audienceMembers[i];
			if (hero != null)
			{
				Equipment equipment4 = hero.CivilianEquipment.Clone();
				CampaignSceneNotificationHelper.RemoveWeaponsFromEquipment(ref equipment4);
				list.Add(CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(hero, equipment4));
			}
			else
			{
				list.Add(new SceneNotificationCharacter(null));
			}
		}
		return list;
	}

	public MarriageSceneNotificationItem(Hero groomHero, Hero brideHero, CampaignTime creationTime, RelevantContextType relevantContextType = RelevantContextType.Any)
	{
		GroomHero = groomHero;
		BrideHero = brideHero;
		RelevantContext = relevantContextType;
		_creationCampaignTime = creationTime;
	}

	private List<Hero> GetAudienceMembers(Hero brideHero, Hero groomHero)
	{
		Queue<Hero> groomSide = new Queue<Hero>();
		Queue<Hero> brideSide = new Queue<Hero>();
		List<Hero> list = new List<Hero>();
		Hero mother = groomHero.Mother;
		if (mother != null && mother.IsAlive)
		{
			groomSide.Enqueue(groomHero.Mother);
		}
		Hero father = groomHero.Father;
		if (father != null && father.IsAlive)
		{
			groomSide.Enqueue(groomHero.Father);
		}
		if (groomHero.Siblings != null)
		{
			foreach (Hero item in groomHero.Siblings.Where((Hero s) => s.IsAlive && !s.IsChild))
			{
				groomSide.Enqueue(item);
			}
		}
		if (groomHero.Children != null)
		{
			foreach (Hero item2 in groomHero.Children.Where((Hero s) => s.IsAlive && !s.IsChild))
			{
				groomSide.Enqueue(item2);
			}
		}
		Hero mother2 = brideHero.Mother;
		if (mother2 != null && mother2.IsAlive)
		{
			brideSide.Enqueue(brideHero.Mother);
		}
		Hero father2 = brideHero.Father;
		if (father2 != null && father2.IsAlive)
		{
			brideSide.Enqueue(brideHero.Father);
		}
		if (brideHero.Siblings != null)
		{
			foreach (Hero item3 in brideHero.Siblings.Where((Hero s) => s.IsAlive && !s.IsChild))
			{
				brideSide.Enqueue(item3);
			}
		}
		if (brideHero.Children != null)
		{
			foreach (Hero item4 in brideHero.Children.Where((Hero s) => s.IsAlive && !s.IsChild))
			{
				brideSide.Enqueue(item4);
			}
		}
		if (groomSide.Count < 3)
		{
			foreach (Hero item5 in Hero.AllAliveHeroes.Where((Hero h) => h.IsLord && !h.IsChild && h != groomHero && h != brideHero && h.IsFriend(groomHero) && !brideSide.Contains(h)).Take(MathF.Ceiling(3f - (float)groomSide.Count)))
			{
				groomSide.Enqueue(item5);
			}
		}
		if (brideSide.Count < 3)
		{
			foreach (Hero item6 in Hero.AllAliveHeroes.Where((Hero h) => h.IsLord && !h.IsChild && h != brideHero && h != groomHero && h.IsFriend(brideHero) && !groomSide.Contains(h)).Take(MathF.Ceiling(3f - (float)brideSide.Count)))
			{
				brideSide.Enqueue(item6);
			}
		}
		for (int i = 0; i < 6; i++)
		{
			bool flag = false;
			flag = (((uint)i <= 1u || i == 4) ? true : false);
			Queue<Hero> queue = (flag ? brideSide : groomSide);
			if (queue.Count > 0 && queue.Peek() != null)
			{
				list.Add(queue.Dequeue());
			}
			else
			{
				list.Add(null);
			}
		}
		return list;
	}

	private static string GetBrideEquipmentIDFromCulture(CultureObject brideCulture)
	{
		return brideCulture.StringId switch
		{
			"empire" => "marriage_female_emp_cutscene_template", 
			"aserai" => "marriage_female_ase_cutscene_template", 
			"battania" => "marriage_female_bat_cutscene_template", 
			"khuzait" => "marriage_female_khu_cutscene_template", 
			"sturgia" => "marriage_female_stu_cutscene_template", 
			"vlandia" => "marriage_female_vla_cutscene_template", 
			_ => "marriage_female_emp_cutscene_template", 
		};
	}
}
