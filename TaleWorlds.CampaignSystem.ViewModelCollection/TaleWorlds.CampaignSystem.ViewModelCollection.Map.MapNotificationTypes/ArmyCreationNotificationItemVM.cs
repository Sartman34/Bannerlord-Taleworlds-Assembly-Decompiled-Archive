using TaleWorlds.CampaignSystem.MapNotificationTypes;
using TaleWorlds.CampaignSystem.Party;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;

public class ArmyCreationNotificationItemVM : MapNotificationItemBaseVM
{
	public Army Army { get; }

	public ArmyCreationNotificationItemVM(ArmyCreationMapNotification data)
		: base(data)
	{
		Army = data.CreatedArmy;
		base.NotificationIdentifier = "armycreation";
		_onInspect = delegate
		{
			GoToMapPosition(Army?.LeaderParty?.Position2D ?? MobileParty.MainParty.Position2D);
		};
		CampaignEvents.OnPartyJoinedArmyEvent.AddNonSerializedListener(this, OnPartyJoinedArmy);
		CampaignEvents.ArmyDispersed.AddNonSerializedListener(this, OnArmyDispersed);
	}

	private void OnArmyDispersed(Army arg1, Army.ArmyDispersionReason arg2, bool isPlayersArmy)
	{
		if (arg1 == Army)
		{
			ExecuteRemove();
		}
	}

	private void OnPartyJoinedArmy(MobileParty party)
	{
		if (party == MobileParty.MainParty && party.Army == Army)
		{
			ExecuteRemove();
		}
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		CampaignEvents.OnPartyJoinedArmyEvent.ClearListeners(this);
		CampaignEvents.ArmyDispersed.ClearListeners(this);
	}
}
