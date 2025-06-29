using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.MapNotificationTypes;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class MarriageOfferCampaignBehavior : CampaignBehaviorBase, IMarriageOfferCampaignBehavior, ICampaignBehavior
{
	private const int MarriageOfferCooldownDurationAsWeeks = 1;

	private const int OfferRelationGainAmountWithTheMarriageClan = 10;

	private const float MapNotificationAutoDeclineDurationInHours = 48f;

	private readonly TextObject MarriageOfferPanelExplanationText = new TextObject("{=CZwrlJMJ}A courier with a marriage offer for {CLAN_MEMBER.NAME} from {OFFERING_CLAN_NAME} has arrived.");

	private Hero _currentOfferedPlayerClanHero;

	private Hero _currentOfferedOtherClanHero;

	private CampaignTime _lastMarriageOfferTime = CampaignTime.Zero;

	internal bool IsThereActiveMarriageOffer
	{
		get
		{
			if (_currentOfferedPlayerClanHero != null)
			{
				return _currentOfferedOtherClanHero != null;
			}
			return false;
		}
	}

	public override void RegisterEvents()
	{
		CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, DailyTickClan);
		CampaignEvents.OnMarriageOfferedToPlayerEvent.AddNonSerializedListener(this, OnMarriageOfferedToPlayer);
		CampaignEvents.OnMarriageOfferCanceledEvent.AddNonSerializedListener(this, OnMarriageOfferCanceled);
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HourlyTick);
		CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this, OnHeroPrisonerTaken);
		CampaignEvents.HeroesMarried.AddNonSerializedListener(this, OnHeroesMarried);
		CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
		CampaignEvents.ArmyCreated.AddNonSerializedListener(this, OnArmyCreated);
		CampaignEvents.MapEventStarted.AddNonSerializedListener(this, OnMapEventStarted);
		CampaignEvents.CharacterBecameFugitive.AddNonSerializedListener(this, CharacterBecameFugitive);
		CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
		CampaignEvents.HeroRelationChanged.AddNonSerializedListener(this, OnHeroRelationChanged);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData("_currentOfferedPlayerClanHero", ref _currentOfferedPlayerClanHero);
		dataStore.SyncData("_currentOfferedOtherClanHero", ref _currentOfferedOtherClanHero);
		dataStore.SyncData("_lastMarriageOfferTime", ref _lastMarriageOfferTime);
	}

	public void CreateMarriageOffer(Hero currentOfferedPlayerClanHero, Hero currentOfferedOtherClanHero)
	{
		_currentOfferedPlayerClanHero = currentOfferedPlayerClanHero;
		_currentOfferedOtherClanHero = currentOfferedOtherClanHero;
		_lastMarriageOfferTime = CampaignTime.Now;
		MarriageOfferPanelExplanationText.SetCharacterProperties("CLAN_MEMBER", _currentOfferedPlayerClanHero.CharacterObject);
		MarriageOfferPanelExplanationText.SetTextVariable("OFFERING_CLAN_NAME", _currentOfferedOtherClanHero.Clan.Name);
		Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new MarriageOfferMapNotification(_currentOfferedPlayerClanHero, _currentOfferedOtherClanHero, MarriageOfferPanelExplanationText));
	}

	public MBBindingList<TextObject> GetMarriageAcceptedConsequences()
	{
		MBBindingList<TextObject> mBBindingList = new MBBindingList<TextObject>();
		TextObject textObject = GameTexts.FindText("str_marriage_consequence_hero_join_clan");
		if (Campaign.Current.Models.MarriageModel.GetClanAfterMarriage(_currentOfferedPlayerClanHero, _currentOfferedOtherClanHero) == _currentOfferedPlayerClanHero.Clan)
		{
			textObject.SetCharacterProperties("HERO", _currentOfferedOtherClanHero.CharacterObject);
			textObject.SetTextVariable("CLAN_NAME", _currentOfferedPlayerClanHero.Clan.Name);
		}
		else
		{
			textObject.SetCharacterProperties("HERO", _currentOfferedPlayerClanHero.CharacterObject);
			textObject.SetTextVariable("CLAN_NAME", _currentOfferedOtherClanHero.Clan.Name);
		}
		mBBindingList.Add(textObject);
		TextObject textObject2 = GameTexts.FindText("str_marriage_consequence_clan_relation");
		textObject2.SetTextVariable("CLAN_NAME", _currentOfferedOtherClanHero.Clan.Name);
		textObject2.SetTextVariable("AMOUNT", 10.ToString("+0;-#"));
		mBBindingList.Add(textObject2);
		return mBBindingList;
	}

	public void OnMarriageOfferAcceptedOnPopUp()
	{
		if (_currentOfferedPlayerClanHero != Hero.MainHero)
		{
			Hero groomHero = (_currentOfferedPlayerClanHero.IsFemale ? _currentOfferedOtherClanHero : _currentOfferedPlayerClanHero);
			Hero brideHero = (_currentOfferedPlayerClanHero.IsFemale ? _currentOfferedPlayerClanHero : _currentOfferedOtherClanHero);
			MBInformationManager.ShowSceneNotification(new MarriageSceneNotificationItem(groomHero, brideHero, CampaignTime.Now));
		}
		ChangeRelationAction.ApplyPlayerRelation(_currentOfferedOtherClanHero.Clan.Leader, 10);
		MarriageAction.Apply(_currentOfferedPlayerClanHero, _currentOfferedOtherClanHero);
		FinalizeMarriageOffer();
	}

	public void OnMarriageOfferDeclinedOnPopUp()
	{
		CampaignEventDispatcher.Instance.OnMarriageOfferCanceled(_currentOfferedPlayerClanHero.IsFemale ? _currentOfferedOtherClanHero : _currentOfferedPlayerClanHero, _currentOfferedPlayerClanHero.IsFemale ? _currentOfferedPlayerClanHero : _currentOfferedOtherClanHero);
	}

	public void OnMarriageOfferedToPlayer(Hero suitor, Hero maiden)
	{
	}

	public void OnMarriageOfferCanceled(Hero suitor, Hero maiden)
	{
		FinalizeMarriageOffer();
	}

	private void DailyTickClan(Clan consideringClan)
	{
		if (!CanOfferMarriageForClan(consideringClan))
		{
			return;
		}
		float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(Clan.PlayerClan.FactionMidSettlement, consideringClan.FactionMidSettlement);
		if (!(MBRandom.RandomFloat >= distance / Campaign.MaximumDistanceBetweenTwoSettlements - 0.5f))
		{
			return;
		}
		foreach (Hero hero in Clan.PlayerClan.Heroes)
		{
			if (hero != Hero.MainHero && hero.CanMarry() && ConsiderMarriageForPlayerClanMember(hero, consideringClan))
			{
				break;
			}
		}
	}

	private void HourlyTick()
	{
		if (IsThereActiveMarriageOffer && _lastMarriageOfferTime.ElapsedHoursUntilNow >= 48f)
		{
			CampaignEventDispatcher.Instance.OnMarriageOfferCanceled(_currentOfferedPlayerClanHero.IsFemale ? _currentOfferedOtherClanHero : _currentOfferedPlayerClanHero, _currentOfferedPlayerClanHero.IsFemale ? _currentOfferedPlayerClanHero : _currentOfferedOtherClanHero);
		}
	}

	private void OnHeroPrisonerTaken(PartyBase capturer, Hero prisoner)
	{
		if (IsThereActiveMarriageOffer && (prisoner == Hero.MainHero || prisoner == _currentOfferedPlayerClanHero || prisoner == _currentOfferedOtherClanHero))
		{
			CampaignEventDispatcher.Instance.OnMarriageOfferCanceled(_currentOfferedPlayerClanHero.IsFemale ? _currentOfferedOtherClanHero : _currentOfferedPlayerClanHero, _currentOfferedPlayerClanHero.IsFemale ? _currentOfferedPlayerClanHero : _currentOfferedOtherClanHero);
		}
	}

	private void OnHeroesMarried(Hero hero1, Hero hero2, bool showNotification = true)
	{
		if (IsThereActiveMarriageOffer && ((hero1 == _currentOfferedPlayerClanHero && hero2 == _currentOfferedOtherClanHero) || (hero1 == _currentOfferedOtherClanHero && hero2 == _currentOfferedPlayerClanHero)))
		{
			CampaignEventDispatcher.Instance.OnMarriageOfferCanceled(_currentOfferedPlayerClanHero.IsFemale ? _currentOfferedOtherClanHero : _currentOfferedPlayerClanHero, _currentOfferedPlayerClanHero.IsFemale ? _currentOfferedPlayerClanHero : _currentOfferedOtherClanHero);
		}
	}

	private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification = true)
	{
		if (IsThereActiveMarriageOffer && (victim == Hero.MainHero || victim == _currentOfferedPlayerClanHero || victim == _currentOfferedOtherClanHero))
		{
			CampaignEventDispatcher.Instance.OnMarriageOfferCanceled(_currentOfferedPlayerClanHero.IsFemale ? _currentOfferedOtherClanHero : _currentOfferedPlayerClanHero, _currentOfferedPlayerClanHero.IsFemale ? _currentOfferedPlayerClanHero : _currentOfferedOtherClanHero);
		}
	}

	private void OnArmyCreated(Army army)
	{
		if (IsThereActiveMarriageOffer && (_currentOfferedPlayerClanHero.PartyBelongedTo?.Army != null || _currentOfferedOtherClanHero.PartyBelongedTo?.Army != null))
		{
			CampaignEventDispatcher.Instance.OnMarriageOfferCanceled(_currentOfferedPlayerClanHero.IsFemale ? _currentOfferedOtherClanHero : _currentOfferedPlayerClanHero, _currentOfferedPlayerClanHero.IsFemale ? _currentOfferedPlayerClanHero : _currentOfferedOtherClanHero);
		}
	}

	private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
	{
		if (IsThereActiveMarriageOffer && (_currentOfferedPlayerClanHero.PartyBelongedTo?.MapEvent != null || _currentOfferedOtherClanHero.PartyBelongedTo?.MapEvent != null))
		{
			CampaignEventDispatcher.Instance.OnMarriageOfferCanceled(_currentOfferedPlayerClanHero.IsFemale ? _currentOfferedOtherClanHero : _currentOfferedPlayerClanHero, _currentOfferedPlayerClanHero.IsFemale ? _currentOfferedPlayerClanHero : _currentOfferedOtherClanHero);
		}
	}

	private void CharacterBecameFugitive(Hero hero)
	{
		if (IsThereActiveMarriageOffer && (!_currentOfferedPlayerClanHero.IsActive || !_currentOfferedOtherClanHero.IsActive))
		{
			CampaignEventDispatcher.Instance.OnMarriageOfferCanceled(_currentOfferedPlayerClanHero.IsFemale ? _currentOfferedOtherClanHero : _currentOfferedPlayerClanHero, _currentOfferedPlayerClanHero.IsFemale ? _currentOfferedPlayerClanHero : _currentOfferedOtherClanHero);
		}
	}

	private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail declareWarDetail)
	{
		if (IsThereActiveMarriageOffer && (!Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(_currentOfferedPlayerClanHero, _currentOfferedOtherClanHero) || !Campaign.Current.Models.MarriageModel.ShouldNpcMarriageBetweenClansBeAllowed(Clan.PlayerClan, _currentOfferedOtherClanHero.Clan)))
		{
			CampaignEventDispatcher.Instance.OnMarriageOfferCanceled(_currentOfferedPlayerClanHero.IsFemale ? _currentOfferedOtherClanHero : _currentOfferedPlayerClanHero, _currentOfferedPlayerClanHero.IsFemale ? _currentOfferedPlayerClanHero : _currentOfferedOtherClanHero);
		}
	}

	private void OnHeroRelationChanged(Hero effectiveHero, Hero effectiveHeroGainedRelationWith, int relationChange, bool showNotification, ChangeRelationAction.ChangeRelationDetail detail, Hero originalHero, Hero originalGainedRelationWith)
	{
		if (IsThereActiveMarriageOffer && (effectiveHero.Clan == _currentOfferedPlayerClanHero.Clan || effectiveHero.Clan == _currentOfferedOtherClanHero.Clan) && (effectiveHeroGainedRelationWith.Clan == _currentOfferedPlayerClanHero.Clan || effectiveHeroGainedRelationWith.Clan == _currentOfferedOtherClanHero.Clan) && !Campaign.Current.Models.MarriageModel.ShouldNpcMarriageBetweenClansBeAllowed(_currentOfferedPlayerClanHero.Clan, _currentOfferedOtherClanHero.Clan))
		{
			CampaignEventDispatcher.Instance.OnMarriageOfferCanceled(_currentOfferedPlayerClanHero.IsFemale ? _currentOfferedOtherClanHero : _currentOfferedPlayerClanHero, _currentOfferedPlayerClanHero.IsFemale ? _currentOfferedPlayerClanHero : _currentOfferedOtherClanHero);
		}
	}

	private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
	{
		if (IsThereActiveMarriageOffer && (_currentOfferedPlayerClanHero.Clan == clan || _currentOfferedOtherClanHero.Clan == clan) && !Campaign.Current.Models.MarriageModel.ShouldNpcMarriageBetweenClansBeAllowed(_currentOfferedPlayerClanHero.Clan, _currentOfferedOtherClanHero.Clan))
		{
			CampaignEventDispatcher.Instance.OnMarriageOfferCanceled(_currentOfferedPlayerClanHero.IsFemale ? _currentOfferedOtherClanHero : _currentOfferedPlayerClanHero, _currentOfferedPlayerClanHero.IsFemale ? _currentOfferedPlayerClanHero : _currentOfferedOtherClanHero);
		}
	}

	private bool CanOfferMarriageForClan(Clan consideringClan)
	{
		if (!IsThereActiveMarriageOffer && _lastMarriageOfferTime.ElapsedWeeksUntilNow >= 1f && !Hero.MainHero.IsPrisoner && consideringClan != Clan.PlayerClan && Campaign.Current.Models.MarriageModel.IsClanSuitableForMarriage(consideringClan))
		{
			return Campaign.Current.Models.MarriageModel.ShouldNpcMarriageBetweenClansBeAllowed(Clan.PlayerClan, consideringClan);
		}
		return false;
	}

	private bool ConsiderMarriageForPlayerClanMember(Hero playerClanHero, Clan consideringClan)
	{
		MarriageModel marriageModel = Campaign.Current.Models.MarriageModel;
		foreach (Hero hero in consideringClan.Heroes)
		{
			float num = marriageModel.NpcCoupleMarriageChance(playerClanHero, hero);
			if (!(num > 0f) || !(MBRandom.RandomFloat < num))
			{
				continue;
			}
			foreach (Romance.RomanticState romanticState in Romance.RomanticStateList)
			{
				if (romanticState.Level >= Romance.RomanceLevelEnum.MatchMadeByFamily && (romanticState.Person1 == playerClanHero || romanticState.Person2 == playerClanHero || romanticState.Person1 == hero || romanticState.Person2 == hero))
				{
					return false;
				}
			}
			CreateMarriageOffer(playerClanHero, hero);
			return true;
		}
		return false;
	}

	private void FinalizeMarriageOffer()
	{
		_currentOfferedPlayerClanHero = null;
		_currentOfferedOtherClanHero = null;
	}
}
