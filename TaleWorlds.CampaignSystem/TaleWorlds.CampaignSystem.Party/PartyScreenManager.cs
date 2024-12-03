using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.Party;

public class PartyScreenManager
{
	private PartyScreenMode _currentMode;

	private PartyScreenLogic _partyScreenLogic;

	private static readonly int _countToAddForEachTroopCheatMode = 10;

	public bool IsDonating { get; private set; }

	public PartyScreenMode CurrentMode => _currentMode;

	public static PartyScreenManager Instance => Campaign.Current.PartyScreenManager;

	public static PartyScreenLogic PartyScreenLogic => Instance._partyScreenLogic;

	private void OpenPartyScreen()
	{
		Game current = Game.Current;
		_partyScreenLogic = new PartyScreenLogic();
		PartyScreenLogicInitializationData initializationData = new PartyScreenLogicInitializationData
		{
			LeftOwnerParty = null,
			RightOwnerParty = PartyBase.MainParty,
			LeftMemberRoster = TroopRoster.CreateDummyTroopRoster(),
			LeftPrisonerRoster = TroopRoster.CreateDummyTroopRoster(),
			RightMemberRoster = PartyBase.MainParty.MemberRoster,
			RightPrisonerRoster = PartyBase.MainParty.PrisonRoster,
			LeftLeaderHero = null,
			RightLeaderHero = PartyBase.MainParty.LeaderHero,
			LeftPartyMembersSizeLimit = 0,
			LeftPartyPrisonersSizeLimit = 0,
			RightPartyMembersSizeLimit = PartyBase.MainParty.PartySizeLimit,
			RightPartyPrisonersSizeLimit = PartyBase.MainParty.PrisonerSizeLimit,
			LeftPartyName = null,
			RightPartyName = PartyBase.MainParty.Name,
			TroopTransferableDelegate = TroopTransferableDelegate,
			PartyPresentationDoneButtonDelegate = DefaultDoneHandler,
			PartyPresentationDoneButtonConditionDelegate = null,
			PartyPresentationCancelButtonActivateDelegate = null,
			PartyPresentationCancelButtonDelegate = null,
			IsDismissMode = true,
			IsTroopUpgradesDisabled = false,
			Header = null,
			PartyScreenClosedDelegate = null,
			TransferHealthiesGetWoundedsFirst = false,
			ShowProgressBar = false,
			MemberTransferState = PartyScreenLogic.TransferState.Transferable,
			PrisonerTransferState = PartyScreenLogic.TransferState.Transferable,
			AccompanyingTransferState = PartyScreenLogic.TransferState.NotTransferable
		};
		_partyScreenLogic.Initialize(initializationData);
		PartyState partyState = current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(_partyScreenLogic);
		_currentMode = PartyScreenMode.Normal;
		current.GameStateManager.PushState(partyState);
	}

	public static void CloseScreen(bool isForced, bool fromCancel = false)
	{
		Instance.ClosePartyPresentation(isForced, fromCancel);
	}

	private void ClosePartyPresentation(bool isForced, bool fromCancel)
	{
		if (_partyScreenLogic == null)
		{
			Debug.FailedAssert("Trying to close party screen when it's already closed!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Party\\PartyScreenManager.cs", "ClosePartyPresentation", 101);
			return;
		}
		bool flag = true;
		if (!fromCancel)
		{
			flag = _partyScreenLogic.DoneLogic(isForced);
		}
		if (flag)
		{
			_partyScreenLogic.OnPartyScreenClosed(fromCancel);
			_partyScreenLogic = null;
			Game.Current.GameStateManager.PopState();
		}
	}

	public static void OpenScreenAsCheat()
	{
		if (!Game.Current.CheatMode)
		{
			MBInformationManager.AddQuickInformation(new TextObject("{=!}Cheat mode is not enabled!"));
			return;
		}
		Instance.IsDonating = false;
		Game current = Game.Current;
		Instance._partyScreenLogic = new PartyScreenLogic();
		PartyScreenLogicInitializationData initializationData = new PartyScreenLogicInitializationData
		{
			LeftOwnerParty = null,
			RightOwnerParty = PartyBase.MainParty,
			LeftMemberRoster = GetRosterWithAllGameTroops(),
			LeftPrisonerRoster = TroopRoster.CreateDummyTroopRoster(),
			RightMemberRoster = PartyBase.MainParty.MemberRoster,
			RightPrisonerRoster = PartyBase.MainParty.PrisonRoster,
			LeftLeaderHero = null,
			RightLeaderHero = PartyBase.MainParty.LeaderHero,
			LeftPartyMembersSizeLimit = 0,
			LeftPartyPrisonersSizeLimit = 0,
			RightPartyMembersSizeLimit = PartyBase.MainParty.PartySizeLimit,
			RightPartyPrisonersSizeLimit = PartyBase.MainParty.PrisonerSizeLimit,
			LeftPartyName = null,
			RightPartyName = PartyBase.MainParty.Name,
			TroopTransferableDelegate = TroopTransferableDelegate,
			PartyPresentationDoneButtonDelegate = DefaultDoneHandler,
			PartyPresentationDoneButtonConditionDelegate = null,
			PartyPresentationCancelButtonActivateDelegate = null,
			PartyPresentationCancelButtonDelegate = null,
			IsDismissMode = true,
			IsTroopUpgradesDisabled = false,
			Header = null,
			PartyScreenClosedDelegate = null,
			TransferHealthiesGetWoundedsFirst = false,
			ShowProgressBar = false,
			MemberTransferState = PartyScreenLogic.TransferState.Transferable,
			PrisonerTransferState = PartyScreenLogic.TransferState.Transferable,
			AccompanyingTransferState = PartyScreenLogic.TransferState.NotTransferable
		};
		Instance._partyScreenLogic.Initialize(initializationData);
		PartyState partyState = current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Instance._currentMode = PartyScreenMode.Normal;
		current.GameStateManager.PushState(partyState);
	}

	private static TroopRoster GetRosterWithAllGameTroops()
	{
		TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
		List<CharacterObject> list = new List<CharacterObject>();
		EncyclopediaPage pageOf = Campaign.Current.EncyclopediaManager.GetPageOf(typeof(CharacterObject));
		for (int i = 0; i < CharacterObject.All.Count; i++)
		{
			CharacterObject characterObject = CharacterObject.All[i];
			if (pageOf.IsValidEncyclopediaItem(characterObject))
			{
				list.Add(characterObject);
			}
		}
		list.Sort((CharacterObject a, CharacterObject b) => a.Name.ToString().CompareTo(b.Name.ToString()));
		for (int j = 0; j < list.Count; j++)
		{
			CharacterObject character = list[j];
			troopRoster.AddToCounts(character, _countToAddForEachTroopCheatMode);
		}
		return troopRoster;
	}

	public static void OpenScreenAsNormal()
	{
		if (Game.Current.CheatMode)
		{
			OpenScreenAsCheat();
			return;
		}
		Instance.IsDonating = false;
		Instance.OpenPartyScreen();
	}

	public static void OpenScreenAsRansom()
	{
		Instance._currentMode = PartyScreenMode.Ransom;
		Instance._partyScreenLogic = new PartyScreenLogic();
		Instance.IsDonating = false;
		PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainParty(TroopRoster.CreateDummyTroopRoster(), TroopRoster.CreateDummyTroopRoster(), PartyScreenLogic.TransferState.NotTransferable, PartyScreenLogic.TransferState.TransferableWithTrade, PartyScreenLogic.TransferState.NotTransferable, TroopTransferableDelegate, null, partyPresentationDoneButtonDelegate: SellPrisonersDoneHandler, header: new TextObject("{=SvahUNo6}Ransom Prisoners"), leftPartyName: GameTexts.FindText("str_ransom_broker"));
		Instance._partyScreenLogic.Initialize(initializationData);
		PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Game.Current.GameStateManager.PushState(partyState);
	}

	public static void OpenScreenAsLoot(TroopRoster leftMemberRoster, TroopRoster leftPrisonerRoster, TextObject leftPartyName, int leftPartySizeLimit, PartyScreenClosedDelegate partyScreenClosedDelegate = null)
	{
		Instance._currentMode = PartyScreenMode.Loot;
		Instance._partyScreenLogic = new PartyScreenLogic();
		PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainParty(leftMemberRoster, leftPrisonerRoster, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.Transferable, TroopTransferableDelegate, null, partyScreenClosedDelegate: partyScreenClosedDelegate, leftPartyName: leftPartyName, leftPartyMembersSizeLimit: leftPartySizeLimit, partyPresentationDoneButtonDelegate: DefaultDoneHandler, header: new TextObject("{=EOQcQa5l}Aftermath"));
		Instance._partyScreenLogic.Initialize(initializationData);
		Instance.IsDonating = false;
		PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Game.Current.GameStateManager.PushState(partyState);
	}

	public static void OpenScreenAsManageTroopsAndPrisoners(MobileParty leftParty, PartyScreenClosedDelegate onPartyScreenClosed = null)
	{
		Instance._partyScreenLogic = new PartyScreenLogic();
		Instance._currentMode = PartyScreenMode.Normal;
		IsTroopTransferableDelegate troopTransferableDelegate = ClanManageTroopAndPrisonerTransferableDelegate;
		PartyPresentationDoneButtonDelegate partyPresentationDoneButtonDelegate = ManageTroopsAndPrisonersDoneHandler;
		PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainPartyAndOther(leftParty, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.Transferable, troopTransferableDelegate, new TextObject("{=uQgNPJnc}Manage Troops"), partyPresentationDoneButtonDelegate, null, null, null, onPartyScreenClosed);
		Instance._partyScreenLogic.Initialize(initializationData);
		Instance.IsDonating = false;
		PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Game.Current.GameStateManager.PushState(partyState);
	}

	public static void OpenScreenAsReceiveTroops(TroopRoster leftMemberParty, TextObject leftPartyName, PartyScreenClosedDelegate partyScreenClosedDelegate = null)
	{
		Instance._partyScreenLogic = new PartyScreenLogic();
		Instance._currentMode = PartyScreenMode.TroopsManage;
		PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainParty(leftMemberParty, TroopRoster.CreateDummyTroopRoster(), PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.NotTransferable, PartyScreenLogic.TransferState.Transferable, TroopTransferableDelegate, null, partyScreenClosedDelegate: partyScreenClosedDelegate, leftPartyName: leftPartyName, leftPartyMembersSizeLimit: leftMemberParty.TotalManCount, partyPresentationDoneButtonDelegate: DefaultDoneHandler, header: new TextObject("{=uQgNPJnc}Manage Troops"));
		Instance._partyScreenLogic.Initialize(initializationData);
		Instance.IsDonating = false;
		PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Game.Current.GameStateManager.PushState(partyState);
	}

	public static void OpenScreenAsManageTroops(MobileParty leftParty)
	{
		Instance._partyScreenLogic = new PartyScreenLogic();
		Instance._currentMode = PartyScreenMode.TroopsManage;
		PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainPartyAndOther(leftParty, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.NotTransferable, PartyScreenLogic.TransferState.Transferable, ClanManageTroopTransferableDelegate, new TextObject("{=uQgNPJnc}Manage Troops"), DefaultDoneHandler);
		Instance._partyScreenLogic.Initialize(initializationData);
		Instance.IsDonating = false;
		PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Game.Current.GameStateManager.PushState(partyState);
	}

	public static void OpenScreenAsDonateTroops(MobileParty leftParty)
	{
		Instance._partyScreenLogic = new PartyScreenLogic();
		Instance._currentMode = PartyScreenMode.TroopsManage;
		Instance.IsDonating = leftParty.Owner.Clan != Clan.PlayerClan;
		PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainPartyAndOther(leftParty, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.NotTransferable, PartyScreenLogic.TransferState.Transferable, DonateModeTroopTransferableDelegate, partyPresentationDoneButtonConditionDelegate: DonateDonePossibleDelegate, header: new TextObject("{=4YfjgtO2}Donate Troops"));
		Instance._partyScreenLogic.Initialize(initializationData);
		PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Game.Current.GameStateManager.PushState(partyState);
	}

	public static void OpenScreenAsDonateGarrisonWithCurrentSettlement()
	{
		Instance._partyScreenLogic = new PartyScreenLogic();
		Instance._currentMode = PartyScreenMode.TroopsManage;
		Instance.IsDonating = true;
		if (Hero.MainHero.CurrentSettlement.Town.GarrisonParty == null)
		{
			Hero.MainHero.CurrentSettlement.AddGarrisonParty();
		}
		MobileParty garrisonParty = Hero.MainHero.CurrentSettlement.Town.GarrisonParty;
		int num = Math.Max(garrisonParty.Party.PartySizeLimit - garrisonParty.Party.NumberOfAllMembers, 0);
		PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainParty(TroopRoster.CreateDummyTroopRoster(), TroopRoster.CreateDummyTroopRoster(), PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.NotTransferable, PartyScreenLogic.TransferState.Transferable, TroopTransferableDelegate, null, garrisonParty.Name, leftPartyMembersSizeLimit: num, partyPresentationDoneButtonDelegate: DonateGarrisonDoneHandler, header: new TextObject("{=uQgNPJnc}Manage Troops"));
		Instance._partyScreenLogic.Initialize(initializationData);
		PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Game.Current.GameStateManager.PushState(partyState);
	}

	public static void OpenScreenAsDonatePrisoners()
	{
		Instance._partyScreenLogic = new PartyScreenLogic();
		Instance._currentMode = PartyScreenMode.PrisonerManage;
		Instance.IsDonating = true;
		if (Hero.MainHero.CurrentSettlement.Town.GarrisonParty == null)
		{
			Hero.MainHero.CurrentSettlement.AddGarrisonParty();
		}
		TroopRoster prisonRoster = Hero.MainHero.CurrentSettlement.Party.PrisonRoster;
		int num = Math.Max(Hero.MainHero.CurrentSettlement.Party.PrisonerSizeLimit - prisonRoster.Count, 0);
		TextObject textObject = new TextObject("{=SDzIAtiA}Prisoners of {SETTLEMENT_NAME}");
		textObject.SetTextVariable("SETTLEMENT_NAME", Hero.MainHero.CurrentSettlement.Name);
		PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainParty(TroopRoster.CreateDummyTroopRoster(), prisonRoster, PartyScreenLogic.TransferState.NotTransferable, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.NotTransferable, DonatePrisonerTransferableDelegate, null, textObject, leftPartyPrisonersSizeLimit: num, partyPresentationDoneButtonDelegate: DonatePrisonersDoneHandler, partyPresentationDoneButtonConditionDelegate: DonateDonePossibleDelegate, header: new TextObject("{=Z212GSiV}Leave Prisoners"));
		Instance._partyScreenLogic.Initialize(initializationData);
		PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Game.Current.GameStateManager.PushState(partyState);
	}

	private static Tuple<bool, TextObject> DonateDonePossibleDelegate(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, int leftLimitNum, int rightLimitNum)
	{
		if (PartyScreenLogic.CurrentData.TransferredPrisonersHistory.Any((Tuple<CharacterObject, int> p) => p.Item2 > 0))
		{
			return new Tuple<bool, TextObject>(item1: false, new TextObject("{=hI7eDbXs}You cannot take prisoners."));
		}
		if (PartyScreenLogic.HaveRightSideGainedTroops())
		{
			return new Tuple<bool, TextObject>(item1: false, new TextObject("{=pvkl6pZh}You cannot take troops."));
		}
		if ((PartyScreenLogic.MemberTransferState != 0 || PartyScreenLogic.AccompanyingTransferState != 0) && PartyScreenLogic.LeftPartyMembersSizeLimit < PartyScreenLogic.MemberRosters[0].TotalManCount)
		{
			return new Tuple<bool, TextObject>(item1: false, new TextObject("{=R7wiHjcL}Donated troops exceed party capacity."));
		}
		if (PartyScreenLogic.PrisonerTransferState != 0 && PartyScreenLogic.LeftPartyPrisonersSizeLimit < PartyScreenLogic.PrisonerRosters[0].TotalManCount)
		{
			return new Tuple<bool, TextObject>(item1: false, new TextObject("{=3nfPGbN0}Donated prisoners exceed party capacity."));
		}
		return new Tuple<bool, TextObject>(item1: true, TextObject.Empty);
	}

	public static bool DonatePrisonerTransferableDelegate(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty)
	{
		if (side == PartyScreenLogic.PartyRosterSide.Right)
		{
			return type == PartyScreenLogic.TroopType.Prisoner;
		}
		return false;
	}

	public static void OpenScreenAsManagePrisoners()
	{
		if (Hero.MainHero?.CurrentSettlement?.Party == null)
		{
			Debug.FailedAssert("Trying to open prisoner management in an invalid state", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Party\\PartyScreenManager.cs", "OpenScreenAsManagePrisoners", 474);
			Debug.Print("Trying to open prisoner management in an invalid state");
			return;
		}
		Instance._partyScreenLogic = new PartyScreenLogic();
		Instance._currentMode = PartyScreenMode.PrisonerManage;
		TroopRoster prisonRoster = Hero.MainHero.CurrentSettlement.Party.PrisonRoster;
		TextObject textObject = new TextObject("{=SDzIAtiA}Prisoners of {SETTLEMENT_NAME}");
		textObject.SetTextVariable("SETTLEMENT_NAME", Hero.MainHero.CurrentSettlement.Name);
		PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainParty(TroopRoster.CreateDummyTroopRoster(), prisonRoster, PartyScreenLogic.TransferState.NotTransferable, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.NotTransferable, TroopTransferableDelegate, null, textObject, leftPartyPrisonersSizeLimit: Hero.MainHero.CurrentSettlement.Party.PrisonerSizeLimit, partyPresentationDoneButtonDelegate: ManageGarrisonDoneHandler, header: new TextObject("{=aadTnAEg}Manage Prisoners"));
		Instance._partyScreenLogic.Initialize(initializationData);
		Instance.IsDonating = false;
		PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Game.Current.GameStateManager.PushState(partyState);
	}

	public static bool TroopTransferableDelegate(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase leftOwnerParty)
	{
		Hero hero = leftOwnerParty?.LeaderHero;
		bool flag = (hero != null && hero.Clan == Clan.PlayerClan) || (leftOwnerParty != null && leftOwnerParty.IsMobile && leftOwnerParty.MobileParty.IsCaravan && leftOwnerParty.Owner == Hero.MainHero) || (leftOwnerParty != null && leftOwnerParty.IsMobile && leftOwnerParty.MobileParty.IsGarrison && leftOwnerParty.MobileParty.CurrentSettlement?.OwnerClan == Clan.PlayerClan);
		if (character.IsHero)
		{
			if (character.IsHero && character.HeroObject.Clan != Clan.PlayerClan)
			{
				if (character.HeroObject.IsPlayerCompanion)
				{
					return character.HeroObject.IsPlayerCompanion && flag;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool ClanManageTroopAndPrisonerTransferableDelegate(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty)
	{
		if (character.IsHero)
		{
			return character.HeroObject.IsPrisoner;
		}
		return true;
	}

	public static bool ClanManageTroopTransferableDelegate(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty)
	{
		return !character.IsHero;
	}

	public static bool DonateModeTroopTransferableDelegate(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty)
	{
		if (!character.IsHero)
		{
			return side == PartyScreenLogic.PartyRosterSide.Right;
		}
		return false;
	}

	public static void OpenScreenWithCondition(IsTroopTransferableDelegate isTroopTransferable, PartyPresentationDoneButtonConditionDelegate doneButtonCondition, PartyPresentationDoneButtonDelegate onDoneClicked, PartyPresentationCancelButtonDelegate onCancelClicked, PartyScreenLogic.TransferState memberTransferState, PartyScreenLogic.TransferState prisonerTransferState, TextObject leftPartyName, int limit, bool showProgressBar, bool isDonating, PartyScreenMode screenMode = PartyScreenMode.Normal, TroopRoster memberRosterLeft = null, TroopRoster prisonerRosterLeft = null)
	{
		if (memberRosterLeft == null)
		{
			memberRosterLeft = TroopRoster.CreateDummyTroopRoster();
		}
		if (prisonerRosterLeft == null)
		{
			prisonerRosterLeft = TroopRoster.CreateDummyTroopRoster();
		}
		Instance._currentMode = screenMode;
		Instance.IsDonating = isDonating;
		Instance._partyScreenLogic = new PartyScreenLogic();
		TroopRoster leftMemberRoster = memberRosterLeft;
		TroopRoster leftPrisonerRoster = prisonerRosterLeft;
		bool showProgressBar2 = showProgressBar;
		PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainParty(leftMemberRoster, leftPrisonerRoster, memberTransferState, prisonerTransferState, PartyScreenLogic.TransferState.NotTransferable, isTroopTransferable, null, leftPartyName, new TextObject("{=nZaeTlj8}Exchange Troops"), null, limit, 0, onDoneClicked, doneButtonCondition, onCancelClicked, null, null, isDismissMode: false, transferHealthiesGetWoundedsFirst: false, isTroopUpgradesDisabled: false, showProgressBar2);
		Instance._partyScreenLogic.Initialize(initializationData);
		PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Game.Current.GameStateManager.PushState(partyState);
	}

	public static void OpenScreenForManagingAlley(TroopRoster memberRosterLeft, IsTroopTransferableDelegate isTroopTransferable, PartyPresentationDoneButtonConditionDelegate doneButtonCondition, PartyPresentationDoneButtonDelegate onDoneClicked, TextObject leftPartyName, PartyPresentationCancelButtonDelegate onCancelButtonClicked)
	{
		Instance._partyScreenLogic = new PartyScreenLogic();
		PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainParty(memberRosterLeft, TroopRoster.CreateDummyTroopRoster(), PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.NotTransferable, PartyScreenLogic.TransferState.NotTransferable, isTroopTransferable, null, leftPartyName, null, null, Campaign.Current.Models.AlleyModel.MaximumTroopCountInPlayerOwnedAlley + 1, 0, onDoneClicked, doneButtonCondition, onCancelButtonClicked, null, null, isDismissMode: false, transferHealthiesGetWoundedsFirst: false, isTroopUpgradesDisabled: false, showProgressBar: true);
		Instance._currentMode = PartyScreenMode.TroopsManage;
		Instance._partyScreenLogic.Initialize(initializationData);
		PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Game.Current.GameStateManager.PushState(partyState);
	}

	public static void OpenScreenAsQuest(TroopRoster leftMemberRoster, TextObject leftPartyName, int leftPartySizeLimit, int questDaysMultiplier, PartyPresentationDoneButtonConditionDelegate doneButtonCondition, PartyScreenClosedDelegate onPartyScreenClosed, IsTroopTransferableDelegate isTroopTransferable, PartyPresentationCancelButtonActivateDelegate partyPresentationCancelButtonActivateDelegate = null)
	{
		Debug.Print("PartyScreenManager::OpenScreenAsQuest");
		Instance._partyScreenLogic = new PartyScreenLogic();
		Instance._currentMode = PartyScreenMode.QuestTroopManage;
		TroopRoster leftPrisonerRoster = TroopRoster.CreateDummyTroopRoster();
		PartyPresentationDoneButtonDelegate partyPresentationDoneButtonDelegate = ManageTroopsAndPrisonersDoneHandler;
		PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainParty(leftMemberRoster, leftPrisonerRoster, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.NotTransferable, PartyScreenLogic.TransferState.Transferable, isTroopTransferable, null, leftPartyName, new TextObject("{=nZaeTlj8}Exchange Troops"), null, leftPartySizeLimit, 0, partyPresentationDoneButtonDelegate, doneButtonCondition, null, partyPresentationCancelButtonActivateDelegate, onPartyScreenClosed, isDismissMode: false, transferHealthiesGetWoundedsFirst: true, isTroopUpgradesDisabled: false, showProgressBar: true, questDaysMultiplier);
		Instance._partyScreenLogic.Initialize(initializationData);
		Instance.IsDonating = false;
		PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Game.Current.GameStateManager.PushState(partyState);
	}

	public static void OpenScreenWithDummyRoster(TroopRoster leftMemberRoster, TroopRoster leftPrisonerRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonerRoster, TextObject leftPartyName, TextObject rightPartyName, int leftPartySizeLimit, int rightPartySizeLimit, PartyPresentationDoneButtonConditionDelegate doneButtonCondition, PartyScreenClosedDelegate onPartyScreenClosed, IsTroopTransferableDelegate isTroopTransferable, PartyPresentationCancelButtonActivateDelegate partyPresentationCancelButtonActivateDelegate = null)
	{
		Debug.Print("PartyScreenManager::OpenScreenWithDummyRoster");
		Instance._partyScreenLogic = new PartyScreenLogic();
		Instance._currentMode = PartyScreenMode.TroopsManage;
		PartyScreenLogicInitializationData partyScreenLogicInitializationData = default(PartyScreenLogicInitializationData);
		partyScreenLogicInitializationData.LeftOwnerParty = null;
		partyScreenLogicInitializationData.RightOwnerParty = MobileParty.MainParty.Party;
		partyScreenLogicInitializationData.LeftMemberRoster = leftMemberRoster;
		partyScreenLogicInitializationData.LeftPrisonerRoster = leftPrisonerRoster;
		partyScreenLogicInitializationData.RightMemberRoster = rightMemberRoster;
		partyScreenLogicInitializationData.RightPrisonerRoster = rightPrisonerRoster;
		partyScreenLogicInitializationData.LeftLeaderHero = null;
		partyScreenLogicInitializationData.RightLeaderHero = PartyBase.MainParty.LeaderHero;
		partyScreenLogicInitializationData.LeftPartyMembersSizeLimit = leftPartySizeLimit;
		partyScreenLogicInitializationData.LeftPartyPrisonersSizeLimit = 0;
		partyScreenLogicInitializationData.RightPartyMembersSizeLimit = rightPartySizeLimit;
		partyScreenLogicInitializationData.RightPartyPrisonersSizeLimit = 0;
		partyScreenLogicInitializationData.LeftPartyName = leftPartyName;
		partyScreenLogicInitializationData.RightPartyName = rightPartyName;
		partyScreenLogicInitializationData.TroopTransferableDelegate = isTroopTransferable;
		partyScreenLogicInitializationData.PartyPresentationDoneButtonDelegate = ManageTroopsAndPrisonersDoneHandler;
		partyScreenLogicInitializationData.PartyPresentationDoneButtonConditionDelegate = doneButtonCondition;
		partyScreenLogicInitializationData.PartyPresentationCancelButtonActivateDelegate = partyPresentationCancelButtonActivateDelegate;
		partyScreenLogicInitializationData.PartyPresentationCancelButtonDelegate = null;
		partyScreenLogicInitializationData.PartyScreenClosedDelegate = onPartyScreenClosed;
		partyScreenLogicInitializationData.IsDismissMode = true;
		partyScreenLogicInitializationData.IsTroopUpgradesDisabled = true;
		partyScreenLogicInitializationData.Header = null;
		partyScreenLogicInitializationData.TransferHealthiesGetWoundedsFirst = true;
		partyScreenLogicInitializationData.ShowProgressBar = false;
		partyScreenLogicInitializationData.MemberTransferState = PartyScreenLogic.TransferState.Transferable;
		partyScreenLogicInitializationData.PrisonerTransferState = PartyScreenLogic.TransferState.NotTransferable;
		partyScreenLogicInitializationData.AccompanyingTransferState = PartyScreenLogic.TransferState.Transferable;
		PartyScreenLogicInitializationData initializationData = partyScreenLogicInitializationData;
		Instance._partyScreenLogic.Initialize(initializationData);
		Instance.IsDonating = false;
		PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
		partyState.InitializeLogic(Instance._partyScreenLogic);
		Game.Current.GameStateManager.PushState(partyState);
	}

	public static void OpenScreenWithDummyRosterWithMainParty(TroopRoster leftMemberRoster, TroopRoster leftPrisonerRoster, TextObject leftPartyName, int leftPartySizeLimit, PartyPresentationDoneButtonConditionDelegate doneButtonCondition, PartyScreenClosedDelegate onPartyScreenClosed, IsTroopTransferableDelegate isTroopTransferable, PartyPresentationCancelButtonActivateDelegate partyPresentationCancelButtonActivateDelegate = null)
	{
		Debug.Print("PartyScreenManager::OpenScreenWithDummyRosterWithMainParty");
		OpenScreenWithDummyRoster(leftMemberRoster, leftPrisonerRoster, MobileParty.MainParty.MemberRoster, MobileParty.MainParty.PrisonRoster, leftPartyName, MobileParty.MainParty.Name, leftPartySizeLimit, MobileParty.MainParty.Party.PartySizeLimit, doneButtonCondition, onPartyScreenClosed, isTroopTransferable, partyPresentationCancelButtonActivateDelegate);
	}

	public static void OpenScreenAsCreateClanPartyForHero(Hero hero, PartyScreenClosedDelegate onScreenClosed = null, IsTroopTransferableDelegate isTroopTransferable = null, bool isHeroRescued = false)
	{
		TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
		TroopRoster leftPrisonerRoster = TroopRoster.CreateDummyTroopRoster();
		TroopRoster troopRoster2 = MobileParty.MainParty.MemberRoster.CloneRosterData();
		TroopRoster rightPrisonerRoster = MobileParty.MainParty.PrisonRoster.CloneRosterData();
		troopRoster.AddToCounts(hero.CharacterObject, 1);
		if (!isHeroRescued)
		{
			troopRoster2.AddToCounts(hero.CharacterObject, -1);
		}
		TextObject textObject = GameTexts.FindText("str_lord_party_name");
		textObject.SetCharacterProperties("TROOP", hero.CharacterObject);
		OpenScreenWithDummyRoster(troopRoster, leftPrisonerRoster, troopRoster2, rightPrisonerRoster, textObject, MobileParty.MainParty.Name, Campaign.Current.Models.PartySizeLimitModel.GetAssumedPartySizeForLordParty(hero, hero.Clan.MapFaction, hero.Clan), MobileParty.MainParty.LimitedPartySize, null, onScreenClosed ?? new PartyScreenClosedDelegate(OpenScreenAsCreateClanPartyForHeroPartyScreenClosed), isTroopTransferable ?? new IsTroopTransferableDelegate(OpenScreenAsCreateClanPartyForHeroTroopTransferableDelegate));
	}

	private static void OpenScreenAsCreateClanPartyForHeroPartyScreenClosed(PartyBase leftOwnerParty, TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, PartyBase rightOwnerParty, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, bool fromCancel)
	{
		if (fromCancel)
		{
			return;
		}
		Hero hero = null;
		for (int i = 0; i < leftMemberRoster.data.Length; i++)
		{
			CharacterObject character = leftMemberRoster.data[i].Character;
			if (character != null && character.IsHero)
			{
				hero = leftMemberRoster.data[i].Character.HeroObject;
			}
		}
		MobileParty mobileParty = hero.Clan.CreateNewMobileParty(hero);
		foreach (TroopRosterElement item in leftMemberRoster.GetTroopRoster())
		{
			if (item.Character != hero.CharacterObject)
			{
				mobileParty.MemberRoster.Add(item);
				rightOwnerParty.MemberRoster.AddToCounts(item.Character, -item.Number, insertAtFront: false, -item.WoundedNumber, -item.Xp);
			}
		}
		foreach (TroopRosterElement item2 in leftPrisonRoster.GetTroopRoster())
		{
			mobileParty.PrisonRoster.Add(item2);
			rightOwnerParty.PrisonRoster.AddToCounts(item2.Character, -item2.Number, insertAtFront: false, -item2.WoundedNumber, -item2.Xp);
		}
	}

	private static bool OpenScreenAsCreateClanPartyForHeroTroopTransferableDelegate(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty)
	{
		return !character.IsHero;
	}

	private static bool SellPrisonersDoneHandler(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty = null, PartyBase rightParty = null)
	{
		SellPrisonersAction.ApplyByPartyScreen(leftPrisonRoster);
		return true;
	}

	private static bool DonateGarrisonDoneHandler(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty = null, PartyBase rightParty = null)
	{
		Settlement currentSettlement = Hero.MainHero.CurrentSettlement;
		MobileParty garrisonParty = currentSettlement.Town.GarrisonParty;
		if (garrisonParty == null)
		{
			currentSettlement.AddGarrisonParty();
			garrisonParty = currentSettlement.Town.GarrisonParty;
		}
		for (int i = 0; i < leftMemberRoster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = leftMemberRoster.GetElementCopyAtIndex(i);
			garrisonParty.AddElementToMemberRoster(elementCopyAtIndex.Character, elementCopyAtIndex.Number);
			if (elementCopyAtIndex.Character.IsHero)
			{
				EnterSettlementAction.ApplyForCharacterOnly(elementCopyAtIndex.Character.HeroObject, currentSettlement);
			}
		}
		return true;
	}

	private static bool DonatePrisonersDoneHandler(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster leftSideTransferredPrisonerRoster, FlattenedTroopRoster rightSideTransferredPrisonerRoster, bool isForced, PartyBase leftParty = null, PartyBase rightParty = null)
	{
		if (!rightSideTransferredPrisonerRoster.IsEmpty())
		{
			Settlement currentSettlement = Hero.MainHero.CurrentSettlement;
			foreach (CharacterObject troop in rightSideTransferredPrisonerRoster.Troops)
			{
				if (troop.IsHero)
				{
					EnterSettlementAction.ApplyForPrisoner(troop.HeroObject, currentSettlement);
				}
			}
			CampaignEventDispatcher.Instance.OnPrisonerDonatedToSettlement(rightParty.MobileParty, rightSideTransferredPrisonerRoster, currentSettlement);
		}
		return true;
	}

	private static bool ManageGarrisonDoneHandler(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty = null, PartyBase rightParty = null)
	{
		Settlement currentSettlement = Hero.MainHero.CurrentSettlement;
		for (int i = 0; i < leftMemberRoster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = leftMemberRoster.GetElementCopyAtIndex(i);
			if (elementCopyAtIndex.Character.IsHero)
			{
				EnterSettlementAction.ApplyForCharacterOnly(elementCopyAtIndex.Character.HeroObject, currentSettlement);
			}
		}
		for (int j = 0; j < leftPrisonRoster.Count; j++)
		{
			TroopRosterElement elementCopyAtIndex2 = leftPrisonRoster.GetElementCopyAtIndex(j);
			if (elementCopyAtIndex2.Character.IsHero)
			{
				EnterSettlementAction.ApplyForPrisoner(elementCopyAtIndex2.Character.HeroObject, currentSettlement);
			}
		}
		return true;
	}

	private static bool ManageTroopsAndPrisonersDoneHandler(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty = null, PartyBase rightParty = null)
	{
		return true;
	}

	private static bool DefaultDoneHandler(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty = null, PartyBase rightParty = null)
	{
		HandleReleasedAndTakenPrisoners(takenPrisonerRoster, releasedPrisonerRoster);
		return true;
	}

	private static void HandleReleasedAndTakenPrisoners(FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster)
	{
		if (!releasedPrisonerRoster.IsEmpty())
		{
			EndCaptivityAction.ApplyByReleasedByChoice(releasedPrisonerRoster);
		}
		if (!takenPrisonerRoster.IsEmpty())
		{
			TakePrisonerAction.ApplyByTakenFromPartyScreen(takenPrisonerRoster);
		}
	}
}
