using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories;

public class ClanPartiesVM : ViewModel
{
	private Action _onExpenseChange;

	private Action<Hero> _openPartyAsManage;

	private Action<ClanCardSelectionInfo> _openCardSelectionPopup;

	private readonly IDisbandPartyCampaignBehavior _disbandBehavior;

	private readonly ITeleportationCampaignBehavior _teleportationBehavior;

	private readonly Action _onRefresh;

	private readonly Clan _faction;

	private readonly IEnumerable<SkillObject> _leaderAssignmentRelevantSkills = new List<SkillObject>
	{
		DefaultSkills.Engineering,
		DefaultSkills.Steward,
		DefaultSkills.Scouting,
		DefaultSkills.Medicine
	};

	private MBBindingList<ClanPartyItemVM> _parties;

	private MBBindingList<ClanPartyItemVM> _garrisons;

	private MBBindingList<ClanPartyItemVM> _caravans;

	private ClanPartyItemVM _currentSelectedParty;

	private HintViewModel _createNewPartyActionHint;

	private bool _canCreateNewParty;

	private bool _isSelected;

	private string _nameText;

	private string _moraleText;

	private string _locationText;

	private string _sizeText;

	private string _createNewPartyText;

	private string _partiesText;

	private string _caravansText;

	private string _garrisonsText;

	private bool _isAnyValidPartySelected;

	private ClanPartiesSortControllerVM _sortController;

	public int TotalExpense { get; private set; }

	public int TotalIncome { get; private set; }

	[DataSourceProperty]
	public HintViewModel CreateNewPartyActionHint
	{
		get
		{
			return _createNewPartyActionHint;
		}
		set
		{
			if (value != _createNewPartyActionHint)
			{
				_createNewPartyActionHint = value;
				OnPropertyChangedWithValue(value, "CreateNewPartyActionHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAnyValidPartySelected
	{
		get
		{
			return _isAnyValidPartySelected;
		}
		set
		{
			if (value != _isAnyValidPartySelected)
			{
				_isAnyValidPartySelected = value;
				OnPropertyChangedWithValue(value, "IsAnyValidPartySelected");
			}
		}
	}

	[DataSourceProperty]
	public string NameText
	{
		get
		{
			return _nameText;
		}
		set
		{
			if (value != _nameText)
			{
				_nameText = value;
				OnPropertyChangedWithValue(value, "NameText");
			}
		}
	}

	[DataSourceProperty]
	public string CaravansText
	{
		get
		{
			return _caravansText;
		}
		set
		{
			if (value != _caravansText)
			{
				_caravansText = value;
				OnPropertyChangedWithValue(value, "CaravansText");
			}
		}
	}

	[DataSourceProperty]
	public string GarrisonsText
	{
		get
		{
			return _garrisonsText;
		}
		set
		{
			if (value != _garrisonsText)
			{
				_garrisonsText = value;
				OnPropertyChangedWithValue(value, "GarrisonsText");
			}
		}
	}

	[DataSourceProperty]
	public string PartiesText
	{
		get
		{
			return _partiesText;
		}
		set
		{
			if (value != _partiesText)
			{
				_partiesText = value;
				OnPropertyChangedWithValue(value, "PartiesText");
			}
		}
	}

	[DataSourceProperty]
	public string MoraleText
	{
		get
		{
			return _moraleText;
		}
		set
		{
			if (value != _moraleText)
			{
				_moraleText = value;
				OnPropertyChangedWithValue(value, "MoraleText");
			}
		}
	}

	[DataSourceProperty]
	public string LocationText
	{
		get
		{
			return _locationText;
		}
		set
		{
			if (value != _locationText)
			{
				_locationText = value;
				OnPropertyChangedWithValue(value, "LocationText");
			}
		}
	}

	[DataSourceProperty]
	public string CreateNewPartyText
	{
		get
		{
			return _createNewPartyText;
		}
		set
		{
			if (value != _createNewPartyText)
			{
				_createNewPartyText = value;
				OnPropertyChangedWithValue(value, "CreateNewPartyText");
			}
		}
	}

	[DataSourceProperty]
	public string SizeText
	{
		get
		{
			return _sizeText;
		}
		set
		{
			if (value != _sizeText)
			{
				_sizeText = value;
				OnPropertyChangedWithValue(value, "SizeText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (value != _isSelected)
			{
				_isSelected = value;
				OnPropertyChangedWithValue(value, "IsSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool CanCreateNewParty
	{
		get
		{
			return _canCreateNewParty;
		}
		set
		{
			if (value != _canCreateNewParty)
			{
				_canCreateNewParty = value;
				OnPropertyChangedWithValue(value, "CanCreateNewParty");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<ClanPartyItemVM> Parties
	{
		get
		{
			return _parties;
		}
		set
		{
			if (value != _parties)
			{
				_parties = value;
				OnPropertyChangedWithValue(value, "Parties");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<ClanPartyItemVM> Caravans
	{
		get
		{
			return _caravans;
		}
		set
		{
			if (value != _caravans)
			{
				_caravans = value;
				OnPropertyChangedWithValue(value, "Caravans");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<ClanPartyItemVM> Garrisons
	{
		get
		{
			return _garrisons;
		}
		set
		{
			if (value != _garrisons)
			{
				_garrisons = value;
				OnPropertyChangedWithValue(value, "Garrisons");
			}
		}
	}

	[DataSourceProperty]
	public ClanPartyItemVM CurrentSelectedParty
	{
		get
		{
			return _currentSelectedParty;
		}
		set
		{
			if (value != _currentSelectedParty)
			{
				_currentSelectedParty = value;
				OnPropertyChangedWithValue(value, "CurrentSelectedParty");
				IsAnyValidPartySelected = value != null;
			}
		}
	}

	[DataSourceProperty]
	public ClanPartiesSortControllerVM SortController
	{
		get
		{
			return _sortController;
		}
		set
		{
			if (value != _sortController)
			{
				_sortController = value;
				OnPropertyChangedWithValue(value, "SortController");
			}
		}
	}

	public ClanPartiesVM(Action onExpenseChange, Action<Hero> openPartyAsManage, Action onRefresh, Action<ClanCardSelectionInfo> openCardSelectionPopup)
	{
		_onExpenseChange = onExpenseChange;
		_onRefresh = onRefresh;
		_disbandBehavior = Campaign.Current.GetCampaignBehavior<IDisbandPartyCampaignBehavior>();
		_teleportationBehavior = Campaign.Current.GetCampaignBehavior<ITeleportationCampaignBehavior>();
		_openPartyAsManage = openPartyAsManage;
		_openCardSelectionPopup = openCardSelectionPopup;
		_faction = Hero.MainHero.Clan;
		Parties = new MBBindingList<ClanPartyItemVM>();
		Garrisons = new MBBindingList<ClanPartyItemVM>();
		Caravans = new MBBindingList<ClanPartyItemVM>();
		MBBindingList<MBBindingList<ClanPartyItemVM>> listsToControl = new MBBindingList<MBBindingList<ClanPartyItemVM>> { Parties, Garrisons, Caravans };
		SortController = new ClanPartiesSortControllerVM(listsToControl);
		CreateNewPartyActionHint = new HintViewModel();
		RefreshPartiesList();
		RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		SizeText = GameTexts.FindText("str_clan_party_size").ToString();
		MoraleText = GameTexts.FindText("str_morale").ToString();
		LocationText = GameTexts.FindText("str_tooltip_label_location").ToString();
		NameText = GameTexts.FindText("str_sort_by_name_label").ToString();
		CreateNewPartyText = GameTexts.FindText("str_clan_create_new_party").ToString();
		GarrisonsText = GameTexts.FindText("str_clan_garrisons").ToString();
		CaravansText = GameTexts.FindText("str_clan_caravans").ToString();
		RefreshPartiesList();
		Parties.ApplyActionOnAllItems(delegate(ClanPartyItemVM x)
		{
			x.RefreshValues();
		});
		Garrisons.ApplyActionOnAllItems(delegate(ClanPartyItemVM x)
		{
			x.RefreshValues();
		});
		Caravans.ApplyActionOnAllItems(delegate(ClanPartyItemVM x)
		{
			x.RefreshValues();
		});
		SortController.RefreshValues();
	}

	public void RefreshTotalExpense()
	{
		TotalExpense = (from p in Parties.Union(Garrisons).Union(Caravans)
			where p.ShouldPartyHaveExpense
			select p).Sum((ClanPartyItemVM p) => p.Expense);
		TotalIncome = Caravans.Sum((ClanPartyItemVM p) => p.Income);
	}

	public void RefreshPartiesList()
	{
		Parties.Clear();
		Garrisons.Clear();
		Caravans.Clear();
		SortController.ResetAllStates();
		foreach (WarPartyComponent warPartyComponent in _faction.WarPartyComponents)
		{
			if (warPartyComponent.MobileParty == MobileParty.MainParty)
			{
				Parties.Insert(0, new ClanPartyItemVM(warPartyComponent.Party, OnPartySelection, OnAnyExpenseChange, OnShowChangeLeaderPopup, ClanPartyItemVM.ClanPartyType.Main, _disbandBehavior, _teleportationBehavior));
			}
			else
			{
				Parties.Add(new ClanPartyItemVM(warPartyComponent.Party, OnPartySelection, OnAnyExpenseChange, OnShowChangeLeaderPopup, ClanPartyItemVM.ClanPartyType.Member, _disbandBehavior, _teleportationBehavior));
			}
		}
		foreach (CaravanPartyComponent party in _faction.Heroes.SelectMany((Hero h) => h.OwnedCaravans))
		{
			if (!Caravans.Any((ClanPartyItemVM c) => c.Party.MobileParty == party.MobileParty))
			{
				Caravans.Add(new ClanPartyItemVM(party.Party, OnPartySelection, OnAnyExpenseChange, OnShowChangeLeaderPopup, ClanPartyItemVM.ClanPartyType.Caravan, _disbandBehavior, _teleportationBehavior));
			}
		}
		foreach (MobileParty garrison in from a in _faction.Settlements
			where a.Town != null
			select a into s
			select s.Town.GarrisonParty)
		{
			if (garrison != null && !Garrisons.Any((ClanPartyItemVM c) => c.Party == garrison.Party))
			{
				Garrisons.Add(new ClanPartyItemVM(garrison.Party, OnPartySelection, OnAnyExpenseChange, OnShowChangeLeaderPopup, ClanPartyItemVM.ClanPartyType.Garrison, _disbandBehavior, _teleportationBehavior));
			}
		}
		int count = _faction.WarPartyComponents.Count;
		_faction.Heroes.Where((Hero h) => !h.IsDisabled).Union(_faction.Companions).Any((Hero h) => h.IsActive && h.PartyBelongedToAsPrisoner == null && !h.IsChild && h.CanLeadParty() && (h.PartyBelongedTo == null || h.PartyBelongedTo.LeaderHero != h));
		CanCreateNewParty = GetCanCreateNewParty(out var disabledReason);
		CreateNewPartyActionHint.HintText = disabledReason;
		GameTexts.SetVariable("CURRENT", count);
		GameTexts.SetVariable("LIMIT", _faction.CommanderLimit);
		PartiesText = GameTexts.FindText("str_clan_parties").ToString();
		GameTexts.SetVariable("CURRENT", Caravans.Count);
		CaravansText = GameTexts.FindText("str_clan_caravans").ToString();
		GameTexts.SetVariable("CURRENT", Garrisons.Count);
		GarrisonsText = GameTexts.FindText("str_clan_garrisons").ToString();
		OnPartySelection(GetDefaultMember());
	}

	private bool GetCanCreateNewParty(out TextObject disabledReason)
	{
		bool flag = _faction.Heroes.Where((Hero h) => !h.IsDisabled).Union(_faction.Companions).Any((Hero h) => h.IsActive && h.PartyBelongedToAsPrisoner == null && !h.IsChild && h.CanLeadParty() && (h.PartyBelongedTo == null || h.PartyBelongedTo.LeaderHero != h));
		if (!CampaignUIHelper.GetMapScreenActionIsEnabledWithReason(out var disabledReason2))
		{
			disabledReason = disabledReason2;
			return false;
		}
		if (_faction.CommanderLimit - _faction.WarPartyComponents.Count <= 0)
		{
			disabledReason = GameTexts.FindText("str_clan_doesnt_have_empty_party_slots");
			return false;
		}
		if (!flag)
		{
			disabledReason = GameTexts.FindText("str_clan_doesnt_have_available_heroes");
			return false;
		}
		disabledReason = TextObject.Empty;
		return true;
	}

	private void OnAnyExpenseChange()
	{
		RefreshTotalExpense();
		_onExpenseChange();
	}

	private ClanPartyItemVM GetDefaultMember()
	{
		return Parties.FirstOrDefault();
	}

	public void ExecuteCreateNewParty()
	{
		if (!CanCreateNewParty)
		{
			return;
		}
		List<InquiryElement> list = new List<InquiryElement>();
		foreach (Hero item in _faction.Heroes.Where((Hero h) => !h.IsDisabled).Union(_faction.Companions))
		{
			if ((item.IsActive || item.IsReleased || item.IsFugitive) && !item.IsChild && item != Hero.MainHero && item.CanLeadParty())
			{
				bool isEnabled = false;
				string hint = GetPartyLeaderAssignmentSkillsHint(item);
				if (item.PartyBelongedToAsPrisoner != null)
				{
					hint = new TextObject("{=vOojEcIf}You cannot assign a prisoner member as a new party leader").ToString();
				}
				else if (item.IsReleased)
				{
					hint = new TextObject("{=OhNYkblK}This hero has just escaped from captors and will be available after some time.").ToString();
				}
				else if (item.PartyBelongedTo != null && item.PartyBelongedTo.LeaderHero == item)
				{
					hint = new TextObject("{=aFYwbosi}This hero is already leading a party.").ToString();
				}
				else if (item.PartyBelongedTo != null && item.PartyBelongedTo.LeaderHero != Hero.MainHero)
				{
					hint = new TextObject("{=FjJi1DJb}This hero is already a part of an another party.").ToString();
				}
				else if (item.GovernorOf != null)
				{
					hint = new TextObject("{=Hz8XO8wk}Governors cannot lead a mobile party and be a governor at the same time.").ToString();
				}
				else if (item.HeroState == Hero.CharacterStates.Disabled)
				{
					hint = new TextObject("{=slzfQzl3}This hero is lost").ToString();
				}
				else if (item.HeroState == Hero.CharacterStates.Fugitive)
				{
					hint = new TextObject("{=dD3kRDHi}This hero is a fugitive and running from their captors. They will be available after some time.").ToString();
				}
				else
				{
					isEnabled = true;
				}
				list.Add(new InquiryElement(item, item.Name.ToString(), new ImageIdentifier(CampaignUIHelper.GetCharacterCode(item.CharacterObject)), isEnabled, hint));
			}
		}
		if (list.Count > 0)
		{
			MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=0Q4Xo2BQ}Select the Leader of the New Party").ToString(), string.Empty, list, isExitShown: true, 1, 1, GameTexts.FindText("str_done").ToString(), "", OnNewPartySelectionOver, OnNewPartySelectionOver));
		}
		else
		{
			MBInformationManager.AddQuickInformation(new TextObject("{=qZvNIVGV}There is no one available in your clan who can lead a party right now."));
		}
	}

	private void OnNewPartySelectionOver(List<InquiryElement> element)
	{
		if (element.Count != 0)
		{
			Hero hero = (Hero)element[0].Identifier;
			bool leaderCameFromMainParty = hero.PartyBelongedTo == MobileParty.MainParty;
			if (leaderCameFromMainParty)
			{
				_openPartyAsManage(hero);
				return;
			}
			MobilePartyHelper.CreateNewClanMobileParty(hero, _faction, out leaderCameFromMainParty);
			_onRefresh();
		}
	}

	public void SelectParty(PartyBase party)
	{
		foreach (ClanPartyItemVM party2 in Parties)
		{
			if (party2.Party == party)
			{
				OnPartySelection(party2);
				break;
			}
		}
		foreach (ClanPartyItemVM caravan in Caravans)
		{
			if (caravan.Party == party)
			{
				OnPartySelection(caravan);
				break;
			}
		}
	}

	private void OnPartySelection(ClanPartyItemVM party)
	{
		if (CurrentSelectedParty != null)
		{
			CurrentSelectedParty.IsSelected = false;
		}
		CurrentSelectedParty = party;
		if (party != null)
		{
			party.IsSelected = true;
		}
	}

	private string GetPartyLeaderAssignmentSkillsHint(Hero hero)
	{
		string text = "";
		int num = 0;
		foreach (SkillObject leaderAssignmentRelevantSkill in _leaderAssignmentRelevantSkills)
		{
			int skillValue = hero.GetSkillValue(leaderAssignmentRelevantSkill);
			GameTexts.SetVariable("LEFT", leaderAssignmentRelevantSkill.Name.ToString());
			GameTexts.SetVariable("RIGHT", skillValue);
			string text2 = GameTexts.FindText("str_LEFT_colon_RIGHT_wSpaceAfterColon").ToString();
			if (num == 0)
			{
				text = text2;
			}
			else
			{
				GameTexts.SetVariable("STR1", text);
				GameTexts.SetVariable("STR2", text2);
				text = GameTexts.FindText("str_string_newline_string").ToString();
			}
			num++;
		}
		return text;
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		Parties.ApplyActionOnAllItems(delegate(ClanPartyItemVM p)
		{
			p.OnFinalize();
		});
		Garrisons.ApplyActionOnAllItems(delegate(ClanPartyItemVM p)
		{
			p.OnFinalize();
		});
		Caravans.ApplyActionOnAllItems(delegate(ClanPartyItemVM p)
		{
			p.OnFinalize();
		});
	}

	public void OnShowChangeLeaderPopup()
	{
		if (CurrentSelectedParty?.Party?.MobileParty != null)
		{
			ClanCardSelectionInfo obj = new ClanCardSelectionInfo(GameTexts.FindText("str_change_party_leader"), GetChangeLeaderCandidates(), OnChangeLeaderOver, isMultiSelection: false);
			_openCardSelectionPopup?.Invoke(obj);
		}
	}

	private IEnumerable<ClanCardSelectionItemInfo> GetChangeLeaderCandidates()
	{
		TextObject cannotDisbandReason;
		bool canDisbandParty = GetCanDisbandParty(out cannotDisbandReason);
		yield return new ClanCardSelectionItemInfo(GameTexts.FindText("str_disband_party"), !canDisbandParty, cannotDisbandReason, null);
		foreach (Hero item in _faction.Heroes.Where((Hero h) => !h.IsDisabled).Union(_faction.Companions))
		{
			if ((item.IsActive || item.IsReleased || item.IsFugitive || item.IsTraveling) && !item.IsChild && item != Hero.MainHero && item.CanLeadParty() && item != CurrentSelectedParty.LeaderMember?.HeroObject)
			{
				TextObject explanation;
				bool flag = FactionHelper.IsMainClanMemberAvailableForPartyLeaderChange(item, isSend: true, CurrentSelectedParty.Party.MobileParty, out explanation);
				ImageIdentifier image = new ImageIdentifier(CampaignUIHelper.GetCharacterCode(item.CharacterObject));
				yield return new ClanCardSelectionItemInfo(item, item.Name, image, CardSelectionItemSpriteType.None, null, null, GetChangeLeaderCandidateProperties(item), !flag, explanation, null);
			}
		}
	}

	private IEnumerable<ClanCardSelectionItemPropertyInfo> GetChangeLeaderCandidateProperties(Hero hero)
	{
		TextObject teleportationDelayText = CampaignUIHelper.GetTeleportationDelayText(hero, CurrentSelectedParty.Party);
		yield return new ClanCardSelectionItemPropertyInfo(teleportationDelayText);
		TextObject textObject = new TextObject("{=hwrQqWir}No Skills");
		int num = 0;
		foreach (SkillObject leaderAssignmentRelevantSkill in _leaderAssignmentRelevantSkills)
		{
			TextObject textObject2 = new TextObject("{=!}{SKILL_VALUE}");
			textObject2.SetTextVariable("SKILL_VALUE", hero.GetSkillValue(leaderAssignmentRelevantSkill));
			TextObject textObject3 = ClanCardSelectionItemPropertyInfo.CreateLabeledValueText(leaderAssignmentRelevantSkill.Name, textObject2);
			if (num == 0)
			{
				textObject = textObject3;
			}
			else
			{
				TextObject textObject4 = GameTexts.FindText("str_string_newline_newline_string");
				textObject4.SetTextVariable("STR1", textObject);
				textObject4.SetTextVariable("STR2", textObject3);
				textObject = textObject4;
			}
			num++;
		}
		yield return new ClanCardSelectionItemPropertyInfo(GameTexts.FindText("str_skills"), textObject);
	}

	private void OnChangeLeaderOver(List<object> selectedItems, Action closePopup)
	{
		if (selectedItems.Count == 1)
		{
			Hero newLeader = selectedItems.FirstOrDefault() as Hero;
			bool isDisband = newLeader == null;
			MobileParty mobileParty = CurrentSelectedParty?.Party?.MobileParty;
			DelayedTeleportationModel delayedTeleportationModel = Campaign.Current.Models.DelayedTeleportationModel;
			int num = ((!isDisband && mobileParty != null) ? ((int)Math.Ceiling(delayedTeleportationModel.GetTeleportationDelayAsHours(newLeader, mobileParty.Party).ResultNumber)) : 0);
			MBTextManager.SetTextVariable("TRAVEL_DURATION", CampaignUIHelper.GetHoursAndDaysTextFromHourValue(num).ToString());
			if (newLeader?.CharacterObject != null)
			{
				StringHelpers.SetCharacterProperties("LEADER", newLeader.CharacterObject);
			}
			TextObject textObject = GameTexts.FindText(isDisband ? "str_disband_party" : "str_change_clan_party_leader");
			InformationManager.ShowInquiry(new InquiryData(text: GameTexts.FindText(isDisband ? "str_disband_party_inquiry" : ((num == 0) ? "str_change_clan_party_leader_instantly_inquiry" : "str_change_clan_party_leader_inquiry")).ToString(), titleText: textObject.ToString(), isAffirmativeOptionShown: true, isNegativeOptionShown: true, affirmativeText: GameTexts.FindText("str_yes").ToString(), negativeText: GameTexts.FindText("str_no").ToString(), affirmativeAction: delegate
			{
				closePopup?.Invoke();
				if (isDisband)
				{
					OnDisbandCurrentParty();
				}
				else
				{
					OnPartyLeaderChanged(newLeader);
				}
				_onRefresh?.Invoke();
			}, negativeAction: null));
		}
		else
		{
			closePopup?.Invoke();
		}
	}

	private void OnPartyLeaderChanged(Hero newLeader)
	{
		if (CurrentSelectedParty?.Party?.LeaderHero != null)
		{
			TeleportHeroAction.ApplyDelayedTeleportToParty(CurrentSelectedParty.Party.LeaderHero, MobileParty.MainParty);
		}
		TeleportHeroAction.ApplyDelayedTeleportToPartyAsPartyLeader(newLeader, CurrentSelectedParty.Party.MobileParty);
	}

	private void OnDisbandCurrentParty()
	{
		DisbandPartyAction.StartDisband(CurrentSelectedParty.Party.MobileParty);
	}

	private bool GetCanDisbandParty(out TextObject cannotDisbandReason)
	{
		bool result = false;
		cannotDisbandReason = TextObject.Empty;
		MobileParty mobileParty = CurrentSelectedParty?.Party?.MobileParty;
		if (mobileParty != null)
		{
			if (!CampaignUIHelper.GetMapScreenActionIsEnabledWithReason(out var disabledReason))
			{
				cannotDisbandReason = disabledReason;
			}
			else if (mobileParty.IsMilitia)
			{
				cannotDisbandReason = GameTexts.FindText("str_cannot_disband_milita_party");
			}
			else if (mobileParty.IsGarrison)
			{
				cannotDisbandReason = GameTexts.FindText("str_cannot_disband_garrison_party");
			}
			else if (mobileParty.IsMainParty)
			{
				cannotDisbandReason = GameTexts.FindText("str_cannot_disband_main_party");
			}
			else if (CurrentSelectedParty.IsDisbanding)
			{
				cannotDisbandReason = GameTexts.FindText("str_cannot_disband_already_disbanding_party");
			}
			else
			{
				result = true;
			}
		}
		return result;
	}
}
