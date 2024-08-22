using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;

public class CharacterCreationGenericStageVM : CharacterCreationStageBaseVM
{
	private readonly int _stageIndex;

	public Action OnOptionSelection;

	private CharacterCreationOption _selectedOption;

	private bool _isRefreshing;

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _doneInputKey;

	private MBBindingList<CharacterCreationOptionVM> _selectionList;

	private CharacterCreationGainedPropertiesVM _gainedPropertiesController;

	private string _positiveEffectText;

	private string _negativeEffectText;

	private string _descriptionText;

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
	public MBBindingList<CharacterCreationOptionVM> SelectionList
	{
		get
		{
			return _selectionList;
		}
		set
		{
			if (value != _selectionList)
			{
				_selectionList = value;
				OnPropertyChangedWithValue(value, "SelectionList");
			}
		}
	}

	[DataSourceProperty]
	public CharacterCreationGainedPropertiesVM GainedPropertiesController
	{
		get
		{
			return _gainedPropertiesController;
		}
		set
		{
			if (value != _gainedPropertiesController)
			{
				_gainedPropertiesController = value;
				OnPropertyChangedWithValue(value, "GainedPropertiesController");
			}
		}
	}

	[DataSourceProperty]
	public string PositiveEffectText
	{
		get
		{
			return _positiveEffectText;
		}
		set
		{
			if (value != _positiveEffectText)
			{
				_positiveEffectText = value;
				OnPropertyChangedWithValue(value, "PositiveEffectText");
			}
		}
	}

	[DataSourceProperty]
	public string NegativeEffectText
	{
		get
		{
			return _negativeEffectText;
		}
		set
		{
			if (value != _negativeEffectText)
			{
				_negativeEffectText = value;
				OnPropertyChangedWithValue(value, "NegativeEffectText");
			}
		}
	}

	[DataSourceProperty]
	public string DescriptionText
	{
		get
		{
			return _descriptionText;
		}
		set
		{
			if (value != _descriptionText)
			{
				_descriptionText = value;
				OnPropertyChangedWithValue(value, "DescriptionText");
			}
		}
	}

	public CharacterCreationGenericStageVM(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation characterCreationMenu, Action affirmativeAction, TextObject affirmativeActionText, Action negativeAction, TextObject negativeActionText, int stageIndex, int currentStageIndex, int totalStagesCount, int furthestIndex, Action<int> goToIndex)
		: base(characterCreationMenu, affirmativeAction, affirmativeActionText, negativeAction, negativeActionText, currentStageIndex, totalStagesCount, furthestIndex, goToIndex)
	{
		_stageIndex = stageIndex;
		SelectionList = new MBBindingList<CharacterCreationOptionVM>();
		_characterCreation.OnInit(stageIndex);
		base.Title = _characterCreation.GetCurrentMenuTitle(stageIndex).ToString();
		base.Description = _characterCreation.GetCurrentMenuText(stageIndex).ToString();
		GameTexts.SetVariable("SELECTION", base.Title);
		base.SelectionText = GameTexts.FindText("str_char_creation_generic_selection").ToString();
		foreach (CharacterCreationOption currentMenuOption in _characterCreation.GetCurrentMenuOptions(stageIndex))
		{
			CharacterCreationOptionVM item = new CharacterCreationOptionVM(ApplySelection, currentMenuOption.Text.ToString(), currentMenuOption);
			SelectionList.Add(item);
		}
		RefreshSelectedOptions();
		GainedPropertiesController = new CharacterCreationGainedPropertiesVM(_characterCreation, _stageIndex);
	}

	public void RefreshSelectedOptions()
	{
		_isRefreshing = true;
		IEnumerable<int> selectedOptions = _characterCreation.GetSelectedOptions(_stageIndex);
		foreach (CharacterCreationOptionVM selection in SelectionList)
		{
			CharacterCreationOption characterCreationOption = (CharacterCreationOption)selection.Identifier;
			selection.IsSelected = selectedOptions.Contains(characterCreationOption.Id);
			if (selection.IsSelected)
			{
				PositiveEffectText = characterCreationOption.PositiveEffectText.ToString();
				DescriptionText = characterCreationOption.DescriptionText.ToString();
				base.AnyItemSelected = true;
				_selectedOption = characterCreationOption;
				_characterCreation.RunConsequence(_selectedOption, _stageIndex, fromInit: false);
			}
		}
		_isRefreshing = false;
		OnPropertyChanged("CanAdvance");
	}

	public void ApplySelection(object optionObject)
	{
		if (optionObject is CharacterCreationOption characterCreationOption && !_isRefreshing && _selectedOption != characterCreationOption)
		{
			_selectedOption = characterCreationOption;
			_characterCreation.RunConsequence(_selectedOption, _stageIndex, fromInit: false);
			RefreshSelectedOptions();
			OnOptionSelection?.Invoke();
			base.AnyItemSelected = true;
			OnPropertyChanged("CanAdvance");
			GainedPropertiesController.UpdateValues();
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
		if (SelectionList.Count != 0)
		{
			return SelectionList.Any((CharacterCreationOptionVM s) => s.IsSelected);
		}
		return true;
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
