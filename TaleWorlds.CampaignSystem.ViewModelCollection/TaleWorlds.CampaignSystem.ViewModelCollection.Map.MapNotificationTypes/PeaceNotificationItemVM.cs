using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapNotificationTypes;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;

public class PeaceNotificationItemVM : MapNotificationItemBaseVM
{
	private IFaction _otherFaction;

	public PeaceNotificationItemVM(PeaceMapNotification data)
		: base(data)
	{
		base.NotificationIdentifier = "peace";
		CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
		_otherFaction = ((data.FirstFaction == Hero.MainHero.MapFaction) ? data.SecondFaction : data.FirstFaction);
		_onInspect = delegate
		{
			base.NavigationHandler?.OpenKingdom(_otherFaction);
		};
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		CampaignEvents.MakePeace.ClearListeners(this);
	}

	private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail)
	{
		if ((faction1 == Hero.MainHero.Clan && _otherFaction == faction2) || (faction2 == Hero.MainHero.Clan && _otherFaction == faction1))
		{
			ExecuteRemove();
		}
	}
}
