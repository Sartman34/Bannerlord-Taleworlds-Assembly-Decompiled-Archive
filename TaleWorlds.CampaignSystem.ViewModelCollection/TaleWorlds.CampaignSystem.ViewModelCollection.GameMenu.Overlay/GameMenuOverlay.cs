using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;

public class GameMenuOverlay : ViewModel
{
	protected internal enum MenuOverlayContextList
	{
		Encyclopedia,
		Conversation,
		QuickConversation,
		ConverseWithLeader,
		ArmyDismiss,
		ManageGarrison,
		DonateTroops,
		JoinArmy,
		TakeToParty,
		ManageTroops
	}

	public string GameMenuOverlayName;

	private bool _closedHandled = true;

	private bool _isContextMenuEnabled;

	private int _currentOverlayType = -1;

	private bool _isInfoBarExtended;

	private bool _isInitializationOver;

	private MBBindingList<StringItemWithEnabledAndHintVM> _contextList;

	protected GameMenuPartyItemVM _contextMenuItem;

	[DataSourceProperty]
	public bool IsContextMenuEnabled
	{
		get
		{
			return _isContextMenuEnabled;
		}
		set
		{
			_isContextMenuEnabled = value;
			OnPropertyChangedWithValue(value, "IsContextMenuEnabled");
		}
	}

	[DataSourceProperty]
	public bool IsInitializationOver
	{
		get
		{
			return _isInitializationOver;
		}
		set
		{
			_isInitializationOver = value;
			OnPropertyChangedWithValue(value, "IsInitializationOver");
		}
	}

	[DataSourceProperty]
	public bool IsInfoBarExtended
	{
		get
		{
			return _isInfoBarExtended;
		}
		set
		{
			_isInfoBarExtended = value;
			OnPropertyChangedWithValue(value, "IsInfoBarExtended");
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringItemWithEnabledAndHintVM> ContextList
	{
		get
		{
			return _contextList;
		}
		set
		{
			if (value != _contextList)
			{
				_contextList = value;
				OnPropertyChangedWithValue(value, "ContextList");
			}
		}
	}

	[DataSourceProperty]
	public int CurrentOverlayType
	{
		get
		{
			return _currentOverlayType;
		}
		set
		{
			if (value != _currentOverlayType)
			{
				_currentOverlayType = value;
				OnPropertyChangedWithValue(value, "CurrentOverlayType");
			}
		}
	}

	public GameMenuOverlay()
	{
		ContextList = new MBBindingList<StringItemWithEnabledAndHintVM>();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		_contextMenuItem?.RefreshValues();
	}

	protected virtual void ExecuteOnSetAsActiveContextMenuItem(GameMenuPartyItemVM troop)
	{
		_contextMenuItem = troop;
	}

	public virtual void ExecuteOnOverlayClosed()
	{
		if (!_closedHandled)
		{
			CampaignEventDispatcher.Instance.OnCharacterPortraitPopUpClosed();
			_closedHandled = true;
		}
	}

	public virtual void ExecuteOnOverlayOpened()
	{
		_closedHandled = false;
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		if (!_closedHandled)
		{
			ExecuteOnOverlayClosed();
		}
	}

	protected void ExecuteTroopAction(object o)
	{
		switch ((MenuOverlayContextList)o)
		{
		case MenuOverlayContextList.Encyclopedia:
			if (_contextMenuItem.Character != null)
			{
				if (_contextMenuItem.Character.IsHero)
				{
					Campaign.Current.EncyclopediaManager.GoToLink(_contextMenuItem.Character.HeroObject.EncyclopediaLink);
					break;
				}
				Debug.FailedAssert("Character object in menu overlay", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem.ViewModelCollection\\GameMenu\\Overlay\\GameMenuOverlay.cs", "ExecuteTroopAction", 101);
				Campaign.Current.EncyclopediaManager.GoToLink(_contextMenuItem.Character.EncyclopediaLink);
			}
			else if (_contextMenuItem.Party != null)
			{
				CharacterObject visualPartyLeader = CampaignUIHelper.GetVisualPartyLeader(_contextMenuItem.Party);
				if (visualPartyLeader != null)
				{
					Campaign.Current.EncyclopediaManager.GoToLink(visualPartyLeader.EncyclopediaLink);
				}
			}
			else if (_contextMenuItem.Settlement != null)
			{
				Campaign.Current.EncyclopediaManager.GoToLink(_contextMenuItem.Settlement.EncyclopediaLink);
			}
			break;
		case MenuOverlayContextList.Conversation:
			if (_contextMenuItem.Character == null)
			{
				break;
			}
			if (_contextMenuItem.Character.IsHero)
			{
				if (PlayerEncounter.Current != null || LocationComplex.Current != null || Campaign.Current.CurrentMenuContext != null)
				{
					Location location = LocationComplex.Current.GetLocationOfCharacter(_contextMenuItem.Character.HeroObject);
					if (location.StringId == "alley")
					{
						location = LocationComplex.Current.GetLocationWithId("center");
					}
					CampaignEventDispatcher.Instance.OnPlayerStartTalkFromMenu(_contextMenuItem.Character.HeroObject);
					PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(location, null, _contextMenuItem.Character);
				}
				else
				{
					EncounterManager.StartPartyEncounter(PartyBase.MainParty, _contextMenuItem.Character.HeroObject.PartyBelongedTo.Party);
				}
			}
			else
			{
				Debug.FailedAssert("Character object in menu overlay", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem.ViewModelCollection\\GameMenu\\Overlay\\GameMenuOverlay.cs", "ExecuteTroopAction", 145);
			}
			break;
		case MenuOverlayContextList.QuickConversation:
			if (_contextMenuItem.Character == null)
			{
				break;
			}
			if (_contextMenuItem.Character.IsHero)
			{
				if (PlayerEncounter.Current != null || LocationComplex.Current != null || Campaign.Current.CurrentMenuContext != null)
				{
					CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, null, noHorse: false, noWeapon: false, spawnAfterFight: false, isCivilianEquipmentRequiredForLeader: true), new ConversationCharacterData(_contextMenuItem.Character, null, noHorse: false, noWeapon: false, spawnAfterFight: false, isCivilianEquipmentRequiredForLeader: true));
				}
				else
				{
					EncounterManager.StartPartyEncounter(PartyBase.MainParty, _contextMenuItem.Character.HeroObject.PartyBelongedTo.Party);
				}
			}
			else
			{
				Debug.FailedAssert("Character object in menu overlay", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem.ViewModelCollection\\GameMenu\\Overlay\\GameMenuOverlay.cs", "ExecuteTroopAction", 168);
			}
			break;
		case MenuOverlayContextList.ConverseWithLeader:
			if (_contextMenuItem.Party?.LeaderHero != null)
			{
				if (Settlement.CurrentSettlement != null || LocationComplex.Current != null || Campaign.Current.CurrentMenuContext != null)
				{
					ConverseWithLeader(PartyBase.MainParty, _contextMenuItem.Party);
				}
				else
				{
					EncounterManager.StartPartyEncounter(PartyBase.MainParty, _contextMenuItem.Party);
				}
			}
			break;
		case MenuOverlayContextList.ArmyDismiss:
			if (_contextMenuItem.Party?.MobileParty.Army != null && _contextMenuItem.Party.MapEvent == null && _contextMenuItem.Party.MobileParty.Army.LeaderParty != _contextMenuItem.Party.MobileParty)
			{
				if (_contextMenuItem.Party.MobileParty.Army.LeaderParty == MobileParty.MainParty && _contextMenuItem.Party.MobileParty.Army.Parties.Count <= 2)
				{
					DisbandArmyAction.ApplyByNotEnoughParty(_contextMenuItem.Party.MobileParty.Army);
					break;
				}
				_contextMenuItem.Party.MobileParty.Army = null;
				_contextMenuItem.Party.MobileParty.Ai.SetMoveModeHold();
			}
			break;
		case MenuOverlayContextList.ManageGarrison:
			if (_contextMenuItem.Party != null)
			{
				PartyScreenManager.OpenScreenAsManageTroops(_contextMenuItem.Party.MobileParty);
			}
			break;
		case MenuOverlayContextList.DonateTroops:
			if (_contextMenuItem.Party != null)
			{
				if (_contextMenuItem.Party.MobileParty.IsGarrison)
				{
					PartyScreenManager.OpenScreenAsDonateGarrisonWithCurrentSettlement();
				}
				else
				{
					PartyScreenManager.OpenScreenAsDonateTroops(_contextMenuItem.Party.MobileParty);
				}
			}
			break;
		case MenuOverlayContextList.ManageTroops:
			if (_contextMenuItem.Party?.MobileParty != null && _contextMenuItem.Party.MobileParty.ActualClan == Clan.PlayerClan)
			{
				PartyScreenManager.OpenScreenAsManageTroopsAndPrisoners(_contextMenuItem.Party.MobileParty);
			}
			break;
		case MenuOverlayContextList.JoinArmy:
		{
			CharacterObject character2 = _contextMenuItem.Character;
			if (character2 != null && character2.IsHero && _contextMenuItem.Character.HeroObject.PartyBelongedTo != null)
			{
				MobileParty.MainParty.Army = _contextMenuItem.Character.HeroObject.PartyBelongedTo.Army;
				MobileParty.MainParty.Army.AddPartyToMergedParties(MobileParty.MainParty);
				Campaign.Current.CurrentMenuContext?.Refresh();
			}
			break;
		}
		case MenuOverlayContextList.TakeToParty:
		{
			CharacterObject character = _contextMenuItem.Character;
			if (character != null && character.IsHero && _contextMenuItem.Character.HeroObject.PartyBelongedTo == null)
			{
				Settlement currentSettlement = _contextMenuItem.Character.HeroObject.CurrentSettlement;
				if (currentSettlement != null && currentSettlement.Notables?.Contains(_contextMenuItem.Character.HeroObject) == true)
				{
					LeaveSettlementAction.ApplyForCharacterOnly(_contextMenuItem.Character.HeroObject);
				}
				AddHeroToPartyAction.Apply(_contextMenuItem.Character.HeroObject, MobileParty.MainParty);
			}
			break;
		}
		}
		if (!_closedHandled)
		{
			CampaignEventDispatcher.Instance.OnCharacterPortraitPopUpClosed();
			_closedHandled = true;
		}
	}

	private void ConverseWithLeader(PartyBase mainParty1, PartyBase party2)
	{
		int num;
		if (mainParty1.Side != BattleSideEnum.Attacker)
		{
			PlayerEncounter current = PlayerEncounter.Current;
			num = ((current != null && current.PlayerSide == BattleSideEnum.Attacker) ? 1 : 0);
		}
		else
		{
			num = 1;
		}
		bool flag = (byte)num != 0;
		if (LocationComplex.Current == null || flag)
		{
			CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, mainParty1), new ConversationCharacterData(ConversationHelper.GetConversationCharacterPartyLeader(party2), party2));
			return;
		}
		Location locationOfCharacter = LocationComplex.Current.GetLocationOfCharacter(party2.LeaderHero);
		CampaignEventDispatcher.Instance.OnPlayerStartTalkFromMenu(party2.LeaderHero);
		PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(locationOfCharacter, null, party2.LeaderHero.CharacterObject);
	}

	public void ClearOverlay()
	{
	}

	public static GameMenuOverlay GetOverlay(GameOverlays.MenuOverlayType menuOverlayType)
	{
		switch (menuOverlayType)
		{
		case GameOverlays.MenuOverlayType.Encounter:
			return new EncounterMenuOverlayVM();
		case GameOverlays.MenuOverlayType.SettlementWithParties:
		case GameOverlays.MenuOverlayType.SettlementWithCharacters:
		case GameOverlays.MenuOverlayType.SettlementWithBoth:
			return new SettlementMenuOverlayVM(menuOverlayType);
		default:
			Debug.FailedAssert("Game menu overlay: " + menuOverlayType.ToString() + " could not be found", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem.ViewModelCollection\\GameMenu\\Overlay\\GameMenuOverlay.cs", "GetOverlay", 295);
			return null;
		}
	}

	public virtual void Refresh()
	{
	}

	public virtual void UpdateOverlayType(GameOverlays.MenuOverlayType newType)
	{
		Refresh();
	}

	public virtual void OnFrameTick(float dt)
	{
	}

	public void HourlyTick()
	{
		Refresh();
	}
}
