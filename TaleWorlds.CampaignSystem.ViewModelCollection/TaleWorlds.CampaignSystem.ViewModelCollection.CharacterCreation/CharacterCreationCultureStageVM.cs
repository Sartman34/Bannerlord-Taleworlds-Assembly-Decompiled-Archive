using System;
using System.Linq;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;

public class CharacterCreationCultureStageVM : CharacterCreationStageBaseVM
{
	private Action<CultureObject> _onCultureSelected;

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _doneInputKey;

	private bool _isActive;

	private MBBindingList<CharacterCreationCultureVM> _cultures;

	private CharacterCreationCultureVM _currentSelectedCulture;

	[DataSourceProperty]
	public InputKeyItemVM CancelInputKey
	{
		get
		{
			return _cancelInputKey;
		}
		set
		{
			if (value != _cancelInputKey)
			{
				_cancelInputKey = value;
				OnPropertyChangedWithValue(value, "CancelInputKey");
			}
		}
	}

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
	public bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			if (value != _isActive)
			{
				_isActive = value;
				OnPropertyChangedWithValue(value, "IsActive");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<CharacterCreationCultureVM> Cultures
	{
		get
		{
			return _cultures;
		}
		set
		{
			if (value != _cultures)
			{
				_cultures = value;
				OnPropertyChangedWithValue(value, "Cultures");
			}
		}
	}

	[DataSourceProperty]
	public CharacterCreationCultureVM CurrentSelectedCulture
	{
		get
		{
			return _currentSelectedCulture;
		}
		set
		{
			if (value != _currentSelectedCulture)
			{
				_currentSelectedCulture = value;
				OnPropertyChangedWithValue(value, "CurrentSelectedCulture");
			}
		}
	}

	public CharacterCreationCultureStageVM(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation characterCreation, Action affirmativeAction, TextObject affirmativeActionText, Action negativeAction, TextObject negativeActionText, int currentStageIndex, int totalStagesCount, int furthestIndex, Action<int> goToIndex, Action<CultureObject> onCultureSelected)
		: base(characterCreation, affirmativeAction, affirmativeActionText, negativeAction, negativeActionText, currentStageIndex, totalStagesCount, furthestIndex, goToIndex)
	{
		_onCultureSelected = onCultureSelected;
		CharacterCreationContentBase currentContent = (GameStateManager.Current.ActiveState as CharacterCreationState).CurrentCharacterCreationContent;
		Cultures = new MBBindingList<CharacterCreationCultureVM>();
		base.Title = GameTexts.FindText("str_culture").ToString();
		base.Description = new TextObject("{=fz2kQjFS}Choose your character's culture:").ToString();
		base.SelectionText = new TextObject("{=MaHMOzL2}Character Culture").ToString();
		foreach (CultureObject culture in currentContent.GetCultures())
		{
			CharacterCreationCultureVM item = new CharacterCreationCultureVM(culture, OnCultureSelection);
			Cultures.Add(item);
		}
		SortCultureList(Cultures);
		if (currentContent.GetSelectedCulture() != null)
		{
			CharacterCreationCultureVM characterCreationCultureVM = Cultures.FirstOrDefault((CharacterCreationCultureVM c) => c.Culture == currentContent.GetSelectedCulture());
			if (characterCreationCultureVM != null)
			{
				OnCultureSelection(characterCreationCultureVM);
			}
		}
	}

	private void SortCultureList(MBBindingList<CharacterCreationCultureVM> listToWorkOn)
	{
		int swapFromIndex = listToWorkOn.IndexOf(listToWorkOn.Single((CharacterCreationCultureVM i) => i.CultureID.Contains("vlan")));
		Swap(listToWorkOn, swapFromIndex, 0);
		int swapFromIndex2 = listToWorkOn.IndexOf(listToWorkOn.Single((CharacterCreationCultureVM i) => i.CultureID.Contains("stur")));
		Swap(listToWorkOn, swapFromIndex2, 1);
		int swapFromIndex3 = listToWorkOn.IndexOf(listToWorkOn.Single((CharacterCreationCultureVM i) => i.CultureID.Contains("empi")));
		Swap(listToWorkOn, swapFromIndex3, 2);
		int swapFromIndex4 = listToWorkOn.IndexOf(listToWorkOn.Single((CharacterCreationCultureVM i) => i.CultureID.Contains("aser")));
		Swap(listToWorkOn, swapFromIndex4, 3);
		int swapFromIndex5 = listToWorkOn.IndexOf(listToWorkOn.Single((CharacterCreationCultureVM i) => i.CultureID.Contains("khuz")));
		Swap(listToWorkOn, swapFromIndex5, 4);
	}

	public void OnCultureSelection(CharacterCreationCultureVM selectedCulture)
	{
		InitializePlayersFaceKeyAccordingToCultureSelection(selectedCulture);
		foreach (CharacterCreationCultureVM item in Cultures.Where((CharacterCreationCultureVM c) => c.IsSelected))
		{
			item.IsSelected = false;
		}
		selectedCulture.IsSelected = true;
		CurrentSelectedCulture = selectedCulture;
		base.AnyItemSelected = true;
		(GameStateManager.Current.ActiveState as CharacterCreationState).CurrentCharacterCreationContent.SetSelectedCulture(selectedCulture.Culture, _characterCreation);
		OnPropertyChanged("CanAdvance");
		_onCultureSelected?.Invoke(selectedCulture.Culture);
	}

	private void InitializePlayersFaceKeyAccordingToCultureSelection(CharacterCreationCultureVM selectedCulture)
	{
		string text = "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000'  key='000BAC088000100DB976648E6774B835537D86629511323BDCB177278A84F667017776140748B49500000000000000000000000000000000000000003EFC5002'/>";
		string text2 = "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000'  key='000500000000000D797664884754DCBAA35E866295A0967774414A498C8336860F7776F20BA7B7A500000000000000000000000000000000000000003CFC2002'/>";
		string text3 = "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000'  key='001CB80CC000300D7C7664876753888A7577866254C69643C4B647398C95A0370077760307A7497300000000000000000000000000000000000000003AF47002'/>";
		string text4 = "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000'  key='0028C80FC000100DBA756445533377873CD1833B3101B44A21C3C5347CA32C260F7776F20BBC35E8000000000000000000000000000000000000000042F41002'/>";
		string text5 = "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000'   key='0016F80E4000200EB8708BD6CDC85229D3698B3ABDFE344CD22D3DD5388988680F7776F20B96723B00000000000000000000000000000000000000003EF41002'/>";
		string text6 = "<BodyProperties version='4' age='25.84' weight='0.5000' build='0.5000'  key='000000058000200D79766434475CDCBAC34E866255A096777441DA49838BF6A50F7776F20BA7B7A500000000000000000000000000000000000000003CFC0002'/>";
		string text7 = "";
		text7 = ((selectedCulture.Culture.StringId == "aserai") ? text4 : ((selectedCulture.Culture.StringId == "khuzait") ? text5 : ((selectedCulture.Culture.StringId == "vlandia") ? text : ((selectedCulture.Culture.StringId == "sturgia") ? text2 : ((selectedCulture.Culture.StringId == "battania") ? text6 : ((!(selectedCulture.Culture.StringId == "empire")) ? text3 : text3))))));
		if (BodyProperties.FromString(text7, out var bodyProperties))
		{
			CharacterObject.PlayerCharacter.UpdatePlayerCharacterBodyProperties(bodyProperties, CharacterObject.PlayerCharacter.Race, CharacterObject.PlayerCharacter.IsFemale);
		}
		CharacterObject.PlayerCharacter.Culture = selectedCulture.Culture;
	}

	private void Swap(MBBindingList<CharacterCreationCultureVM> listToWorkOn, int swapFromIndex, int swapToIndex)
	{
		if (swapFromIndex != swapToIndex)
		{
			CharacterCreationCultureVM value = listToWorkOn[swapToIndex];
			listToWorkOn[swapToIndex] = listToWorkOn[swapFromIndex];
			listToWorkOn[swapFromIndex] = value;
		}
	}

	public override void OnNextStage()
	{
		_affirmativeAction();
	}

	public override void OnPreviousStage()
	{
		_negativeAction();
	}

	public override bool CanAdvanceToNextStage()
	{
		return Cultures.Any((CharacterCreationCultureVM s) => s.IsSelected);
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		CancelInputKey?.OnFinalize();
		DoneInputKey?.OnFinalize();
	}

	public void SetCancelInputKey(HotKey hotKey)
	{
		CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}

	public void SetDoneInputKey(HotKey hotKey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}
}
