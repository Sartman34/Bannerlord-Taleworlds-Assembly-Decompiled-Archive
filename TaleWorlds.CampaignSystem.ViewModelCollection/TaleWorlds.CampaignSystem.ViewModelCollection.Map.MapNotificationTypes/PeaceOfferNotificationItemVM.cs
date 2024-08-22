using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.MapNotificationTypes;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;

public class PeaceOfferNotificationItemVM : MapNotificationItemBaseVM
{
	private IFaction _opponentFaction;

	private int _tributeAmount;

	private bool _playerInspectedNotification;

	public PeaceOfferNotificationItemVM(PeaceOfferMapNotification data)
		: base(data)
	{
		PeaceOfferNotificationItemVM peaceOfferNotificationItemVM = this;
		_opponentFaction = data.OpponentFaction;
		_tributeAmount = data.TributeAmount;
		_onInspect = delegate
		{
			CampaignEventDispatcher.Instance.OnPeaceOfferedToPlayer(data.OpponentFaction, data.TributeAmount);
			peaceOfferNotificationItemVM._playerInspectedNotification = true;
			peaceOfferNotificationItemVM.ExecuteRemove();
		};
		CampaignEvents.OnPeaceOfferCancelledEvent.AddNonSerializedListener(this, OnPeaceOfferCancelled);
		base.NotificationIdentifier = "ransom";
	}

	private void OnPeaceOfferCancelled(IFaction opponentFaction)
	{
		if (Campaign.Current.CampaignInformationManager.InformationDataExists((PeaceOfferMapNotification x) => x == base.Data))
		{
			ExecuteRemove();
			_opponentFaction = null;
		}
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		CampaignEventDispatcher.Instance.RemoveListeners(this);
		if (_playerInspectedNotification || Hero.MainHero.MapFaction.Leader == Hero.MainHero)
		{
			return;
		}
		bool flag = false;
		foreach (KingdomDecision unresolvedDecision in ((Kingdom)Hero.MainHero.MapFaction).UnresolvedDecisions)
		{
			if (unresolvedDecision is MakePeaceKingdomDecision && ((MakePeaceKingdomDecision)unresolvedDecision).ProposerClan.MapFaction == Hero.MainHero.MapFaction && ((MakePeaceKingdomDecision)unresolvedDecision).FactionToMakePeaceWith == _opponentFaction)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			MakePeaceKingdomDecision kingdomDecision = new MakePeaceKingdomDecision(Hero.MainHero.MapFaction.Leader.Clan, _opponentFaction, -_tributeAmount);
			((Kingdom)Hero.MainHero.MapFaction).AddDecision(kingdomDecision);
		}
	}
}
