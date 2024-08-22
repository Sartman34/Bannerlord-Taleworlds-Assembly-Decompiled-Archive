using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapNotificationTypes;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;

public class DeathNotificationItemVM : MapNotificationItemBaseVM
{
	public DeathNotificationItemVM(DeathMapNotification data)
		: base(data)
	{
		DeathNotificationItemVM deathNotificationItemVM = this;
		base.NotificationIdentifier = "death";
		if (data.VictimHero == Hero.MainHero)
		{
			_onInspect = delegate
			{
				deathNotificationItemVM.NavigationHandler?.OpenCharacterDeveloper(Hero.MainHero);
			};
		}
		else if (data.KillDetail == KillCharacterAction.KillCharacterActionDetail.DiedInBattle)
		{
			_onInspect = delegate
			{
				MBInformationManager.ShowSceneNotification(new ClanMemberWarDeathSceneNotificationItem(data.VictimHero, data.CreationTime));
			};
		}
		else
		{
			_onInspect = delegate
			{
				MBInformationManager.ShowSceneNotification(new ClanMemberPeaceDeathSceneNotificationItem(data.VictimHero, data.CreationTime));
			};
		}
	}
}
