using System;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Party.PartyTroopManagerPopUp;

public abstract class PartyTroopManagerVM : ViewModel
{
	protected PartyVM _partyVM;

	protected bool _hasMadeChanges;

	protected TextObject _openButtonEnabledHint = TextObject.Empty;

	protected TextObject _openButtonNoTroopsHint = TextObject.Empty;

	protected TextObject _openButtonIrrelevantScreenHint = TextObject.Empty;

	protected TextObject _openButtonUpgradesDisabledHint = TextObject.Empty;

	private int _initialGoldChange;

	private int _initialHorseChange;

	private int _initialMoraleChange;

	private InputKeyItemVM _doneInputKey;

	private InputKeyItemVM _primaryActionInputKey;

	private InputKeyItemVM _secondaryActionInputKey;

	private bool _isFocusedOnACharacter;

	private bool _isOpen;

	private bool _isUpgradePopUp;

	private bool _isPrimaryActionAvailable;

	private bool _isSecondaryActionAvailable;

	private PartyTroopManagerItemVM _focusedTroop;

	private MBBindingList<PartyTroopManagerItemVM> _troops;

	private HintViewModel _openButtonHint;

	private string _titleText;

	private string _avatarText;

	private string _nameText;

	private string _countText;

	private string _goldChangeText;

	private string _horseChangeText;

	private string _moraleChangeText;

	private string _doneLbl;

	[DataSourceProperty]
	public InputKeyItemVM DoneInputKey
	{
		get
		{
			return _doneInputKey;
		}
		set
		{
			if (value != _doneInputKey)
			{
				_doneInputKey = value;
				OnPropertyChangedWithValue(value, "DoneInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM PrimaryActionInputKey
	{
		get
		{
			return _primaryActionInputKey;
		}
		set
		{
			if (value != _primaryActionInputKey)
			{
				_primaryActionInputKey = value;
				OnPropertyChangedWithValue(value, "PrimaryActionInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM SecondaryActionInputKey
	{
		get
		{
			return _secondaryActionInputKey;
		}
		set
		{
			if (value != _secondaryActionInputKey)
			{
				_secondaryActionInputKey = value;
				OnPropertyChangedWithValue(value, "SecondaryActionInputKey");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFocusedOnACharacter
	{
		get
		{
			return _isFocusedOnACharacter;
		}
		set
		{
			if (value != _isFocusedOnACharacter)
			{
				_isFocusedOnACharacter = value;
				OnPropertyChangedWithValue(value, "IsFocusedOnACharacter");
			}
		}
	}

	[DataSourceProperty]
	public bool IsOpen
	{
		get
		{
			return _isOpen;
		}
		set
		{
			if (value != _isOpen)
			{
				_isOpen = value;
				OnPropertyChangedWithValue(value, "IsOpen");
			}
		}
	}

	[DataSourceProperty]
	public bool IsUpgradePopUp
	{
		get
		{
			return _isUpgradePopUp;
		}
		set
		{
			if (value != _isUpgradePopUp)
			{
				_isUpgradePopUp = value;
				OnPropertyChangedWithValue(value, "IsUpgradePopUp");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPrimaryActionAvailable
	{
		get
		{
			return _isPrimaryActionAvailable;
		}
		set
		{
			if (value != _isPrimaryActionAvailable)
			{
				_isPrimaryActionAvailable = value;
				OnPropertyChangedWithValue(value, "IsPrimaryActionAvailable");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSecondaryActionAvailable
	{
		get
		{
			return _isSecondaryActionAvailable;
		}
		set
		{
			if (value != _isSecondaryActionAvailable)
			{
				_isSecondaryActionAvailable = value;
				OnPropertyChangedWithValue(value, "IsSecondaryActionAvailable");
			}
		}
	}

	[DataSourceProperty]
	public PartyTroopManagerItemVM FocusedTroop
	{
		get
		{
			return _focusedTroop;
		}
		set
		{
			if (value != _focusedTroop)
			{
				_focusedTroop = value;
				OnPropertyChangedWithValue(value, "FocusedTroop");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<PartyTroopManagerItemVM> Troops
	{
		get
		{
			return _troops;
		}
		set
		{
			if (value != _troops)
			{
				_troops = value;
				OnPropertyChangedWithValue(value, "Troops");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel OpenButtonHint
	{
		get
		{
			return _openButtonHint;
		}
		set
		{
			if (value != _openButtonHint)
			{
				_openButtonHint = value;
				OnPropertyChangedWithValue(value, "OpenButtonHint");
			}
		}
	}

	[DataSourceProperty]
	public string TitleText
	{
		get
		{
			return _titleText;
		}
		set
		{
			if (value != _titleText)
			{
				_titleText = value;
				OnPropertyChangedWithValue(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string AvatarText
	{
		get
		{
			return _avatarText;
		}
		set
		{
			if (value != _avatarText)
			{
				_avatarText = value;
				OnPropertyChangedWithValue(value, "AvatarText");
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
	public string CountText
	{
		get
		{
			return _countText;
		}
		set
		{
			if (value != _countText)
			{
				_countText = value;
				OnPropertyChangedWithValue(value, "CountText");
			}
		}
	}

	[DataSourceProperty]
	public string GoldChangeText
	{
		get
		{
			return _goldChangeText;
		}
		set
		{
			if (value != _goldChangeText)
			{
				_goldChangeText = value;
				OnPropertyChangedWithValue(value, "GoldChangeText");
			}
		}
	}

	[DataSourceProperty]
	public string HorseChangeText
	{
		get
		{
			return _horseChangeText;
		}
		set
		{
			if (value != _horseChangeText)
			{
				_horseChangeText = value;
				OnPropertyChangedWithValue(value, "HorseChangeText");
			}
		}
	}

	[DataSourceProperty]
	public string MoraleChangeText
	{
		get
		{
			return _moraleChangeText;
		}
		set
		{
			if (value != _moraleChangeText)
			{
				_moraleChangeText = value;
				OnPropertyChangedWithValue(value, "MoraleChangeText");
			}
		}
	}

	[DataSourceProperty]
	public string DoneLbl
	{
		get
		{
			return _doneLbl;
		}
		set
		{
			if (value != _doneLbl)
			{
				_doneLbl = value;
				OnPropertyChangedWithValue(value, "DoneLbl");
			}
		}
	}

	public abstract void ExecuteItemPrimaryAction();

	public abstract void ExecuteItemSecondaryAction();

	public PartyTroopManagerVM(PartyVM partyVM)
	{
		_partyVM = partyVM;
		Troops = new MBBindingList<PartyTroopManagerItemVM>();
		OpenButtonHint = new HintViewModel();
		RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		AvatarText = new TextObject("{=5tbWdY1j}Avatar").ToString();
		NameText = new TextObject("{=PDdh1sBj}Name").ToString();
		CountText = new TextObject("{=zFDoDbNj}Count").ToString();
		DoneLbl = GameTexts.FindText("str_done").ToString();
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		DoneInputKey.OnFinalize();
		PrimaryActionInputKey?.OnFinalize();
		SecondaryActionInputKey?.OnFinalize();
	}

	public virtual void OpenPopUp()
	{
		_partyVM.PartyScreenLogic.SavePartyScreenData();
		_initialGoldChange = _partyVM.PartyScreenLogic.CurrentData.PartyGoldChangeAmount;
		_initialHorseChange = _partyVM.PartyScreenLogic.CurrentData.PartyHorseChangeAmount;
		_initialMoraleChange = _partyVM.PartyScreenLogic.CurrentData.PartyMoraleChangeAmount;
		UpdateLabels();
		_hasMadeChanges = false;
		IsOpen = true;
	}

	public virtual void ExecuteDone()
	{
		IsOpen = false;
	}

	protected virtual void ConfirmCancel()
	{
		_partyVM.PartyScreenLogic.ResetToLastSavedPartyScreenData(fromCancel: false);
		IsOpen = false;
	}

	public void UpdateOpenButtonHint(bool isDisabled, bool isIrrelevant, bool isUpgradesDisabled)
	{
		TextObject textObject = null;
		textObject = (isIrrelevant ? _openButtonIrrelevantScreenHint : (isUpgradesDisabled ? _openButtonUpgradesDisabledHint : ((!isDisabled) ? _openButtonEnabledHint : _openButtonNoTroopsHint)));
		OpenButtonHint.HintText = textObject;
	}

	public abstract void ExecuteCancel();

	protected void ShowCancelInquiry(Action confirmCancel)
	{
		if (_hasMadeChanges)
		{
			string text = new TextObject("{=a8NoW1Q2}Are you sure you want to cancel your changes?").ToString();
			InformationManager.ShowInquiry(new InquiryData("", text, isAffirmativeOptionShown: true, isNegativeOptionShown: true, GameTexts.FindText("str_yes").ToString(), GameTexts.FindText("str_no").ToString(), delegate
			{
				confirmCancel();
			}, null));
		}
		else
		{
			confirmCancel();
		}
	}

	protected void UpdateLabels()
	{
		MBTextManager.SetTextVariable("PAY_OR_GET", 0);
		int num = _partyVM.PartyScreenLogic.CurrentData.PartyGoldChangeAmount - _initialGoldChange;
		int num2 = _partyVM.PartyScreenLogic.CurrentData.PartyHorseChangeAmount - _initialHorseChange;
		int num3 = _partyVM.PartyScreenLogic.CurrentData.PartyMoraleChangeAmount - _initialMoraleChange;
		MBTextManager.SetTextVariable("LABEL_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
		MBTextManager.SetTextVariable("TRADE_AMOUNT", TaleWorlds.Library.MathF.Abs(num));
		GoldChangeText = ((num == 0) ? "" : GameTexts.FindText("str_party_generic_label").ToString());
		MBTextManager.SetTextVariable("LABEL_ICON", "{=!}<img src=\"StdAssets\\ItemIcons\\Mount\" extend=\"16\">");
		MBTextManager.SetTextVariable("TRADE_AMOUNT", TaleWorlds.Library.MathF.Abs(num2));
		HorseChangeText = ((num2 == 0) ? "" : GameTexts.FindText("str_party_generic_label").ToString());
		MBTextManager.SetTextVariable("LABEL_ICON", "{=!}<img src=\"General\\Icons\\Morale@2x\" extend=\"8\">");
		MBTextManager.SetTextVariable("TRADE_AMOUNT", TaleWorlds.Library.MathF.Abs(num3));
		MoraleChangeText = ((num3 == 0) ? "" : GameTexts.FindText("str_party_generic_label").ToString());
	}

	protected void SetFocusedCharacter(PartyTroopManagerItemVM troop)
	{
		FocusedTroop = troop;
		IsFocusedOnACharacter = troop != null;
		if (FocusedTroop == null)
		{
			IsPrimaryActionAvailable = false;
			IsSecondaryActionAvailable = false;
		}
		else if (IsUpgradePopUp)
		{
			MBBindingList<UpgradeTargetVM> upgrades = FocusedTroop.PartyCharacter.Upgrades;
			IsPrimaryActionAvailable = upgrades.Count > 0 && upgrades[0].IsAvailable;
			IsSecondaryActionAvailable = upgrades.Count > 1 && upgrades[1].IsAvailable;
		}
		else
		{
			IsPrimaryActionAvailable = false;
			IsSecondaryActionAvailable = FocusedTroop.IsTroopRecruitable;
		}
	}

	public void SetDoneInputKey(HotKey hotKey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}

	public void SetPrimaryActionInputKey(HotKey hotKey)
	{
		PrimaryActionInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}

	public void SetSecondaryActionInputKey(HotKey hotKey)
	{
		SecondaryActionInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}
}
