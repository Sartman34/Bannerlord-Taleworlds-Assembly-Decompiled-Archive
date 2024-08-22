using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.SceneInformationPopupTypes;

public class FindingSecondBannerPieceSceneNotificationItem : SceneNotificationData
{
	private readonly CampaignTime _creationCampaignTime;

	public Hero PlayerHero { get; }

	public override string SceneID => "scn_second_banner_piece_notification";

	public override TextObject TitleText
	{
		get
		{
			GameTexts.SetVariable("DAY_OF_YEAR", CampaignSceneNotificationHelper.GetFormalDayAndSeasonText(_creationCampaignTime));
			GameTexts.SetVariable("YEAR", _creationCampaignTime.GetYear);
			return GameTexts.FindText("str_second_banner_piece_found");
		}
	}

	public override IEnumerable<Banner> GetBanners()
	{
		return new List<Banner> { PlayerHero.ClanBanner };
	}

	public FindingSecondBannerPieceSceneNotificationItem(Hero playerHero)
	{
		PlayerHero = playerHero;
		_creationCampaignTime = CampaignTime.Now;
	}
}
