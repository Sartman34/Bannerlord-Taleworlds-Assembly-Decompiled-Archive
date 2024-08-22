using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.SceneInformationPopupTypes;

public class ClanMemberPeaceDeathSceneNotificationItem : SceneNotificationData
{
	private const int NumberOfAudienceHeroes = 5;

	private readonly CampaignTime _creationCampaignTime;

	public Hero DeadHero { get; }

	public override string SceneID => "scn_cutscene_family_member_death";

	public override TextObject TitleText
	{
		get
		{
			GameTexts.SetVariable("DAY_OF_YEAR", CampaignSceneNotificationHelper.GetFormalDayAndSeasonText(_creationCampaignTime));
			GameTexts.SetVariable("YEAR", _creationCampaignTime.GetYear);
			GameTexts.SetVariable("NAME", DeadHero.Name);
			return GameTexts.FindText("str_family_member_death");
		}
	}

	public override IEnumerable<SceneNotificationCharacter> GetSceneNotificationCharacters()
	{
		Equipment equipment = DeadHero.CivilianEquipment.Clone();
		List<SceneNotificationCharacter> list = new List<SceneNotificationCharacter>();
		CampaignSceneNotificationHelper.RemoveWeaponsFromEquipment(ref equipment);
		list.Add(CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(DeadHero, equipment));
		foreach (Hero item in CampaignSceneNotificationHelper.GetMilitaryAudienceForHero(DeadHero).Take(5))
		{
			Equipment equipment2 = item.CivilianEquipment.Clone();
			CampaignSceneNotificationHelper.RemoveWeaponsFromEquipment(ref equipment2);
			list.Add(CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(item, equipment2));
		}
		return list;
	}

	public override IEnumerable<Banner> GetBanners()
	{
		return new List<Banner> { DeadHero.ClanBanner };
	}

	public ClanMemberPeaceDeathSceneNotificationItem(Hero deadHero, CampaignTime creationTime)
	{
		DeadHero = deadHero;
		_creationCampaignTime = creationTime;
	}
}
