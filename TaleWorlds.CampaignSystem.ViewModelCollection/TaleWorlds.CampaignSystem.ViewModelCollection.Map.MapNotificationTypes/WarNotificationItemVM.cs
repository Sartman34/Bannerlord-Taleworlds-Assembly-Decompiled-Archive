using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapNotificationTypes;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;

public class WarNotificationItemVM : MapNotificationItemBaseVM
{
	public WarNotificationItemVM(WarMapNotification data)
		: base(data)
	{
		WarNotificationItemVM warNotificationItemVM = this;
		base.NotificationIdentifier = "battle";
		CampaignEvents.MakePeace.AddNonSerializedListener(this, OnPeaceMade);
		if (!data.FirstFaction.IsRebelClan && !data.SecondFaction.IsRebelClan)
		{
			_onInspect = delegate
			{
				warNotificationItemVM.NavigationHandler?.OpenKingdom((data.FirstFaction == Hero.MainHero.MapFaction) ? data.SecondFaction : data.FirstFaction);
			};
		}
		else
		{
			_onInspect = null;
		}
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		CampaignEvents.MakePeace.ClearListeners(this);
	}

	private void OnPeaceMade(IFaction faction1, IFaction faction2, MakePeaceAction.MakePeaceDetail detail)
	{
		if (faction1 == Hero.MainHero.Clan || (Hero.MainHero.MapFaction != null && (faction1 == Hero.MainHero.MapFaction || faction2 == Hero.MainHero.MapFaction)))
		{
			ExecuteRemove();
		}
	}
}
