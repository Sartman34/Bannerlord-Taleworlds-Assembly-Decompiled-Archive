using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation.OptionsStage;

public class CharacterCreationOptionsStageVM : CharacterCreationStageBaseVM
{
	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _doneInputKey;

	private CampaignOptionsControllerVM _optionsController;

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
	public CampaignOptionsControllerVM OptionsController
	{
		get
		{
			return _optionsController;
		}
		set
		{
			if (value != _optionsController)
			{
				_optionsController = value;
				OnPropertyChangedWithValue(value, "OptionsController");
			}
		}
	}

	public CharacterCreationOptionsStageVM(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation characterCreation, Action affirmativeAction, TextObject affirmativeActionText, Action negativeAction, TextObject negativeActionText, int currentStageIndex, int totalStagesCount, int furthestIndex, Action<int> goToIndex)
		: base(characterCreation, affirmativeAction, affirmativeActionText, negativeAction, negativeActionText, currentStageIndex, totalStagesCount, furthestIndex, goToIndex)
	{
		base.Title = GameTexts.FindText("str_difficulty").ToString();
		base.Description = GameTexts.FindText("str_determine_difficulty").ToString();
		MBBindingList<CampaignOptionItemVM> mBBindingList = new MBBindingList<CampaignOptionItemVM>();
		List<ICampaignOptionData> characterCreationCampaignOptions = CampaignOptionsManager.GetCharacterCreationCampaignOptions();
		for (int i = 0; i < characterCreationCampaignOptions.Count; i++)
		{
			mBBindingList.Add(new CampaignOptionItemVM(characterCreationCampaignOptions[i]));
		}
		OptionsController = new CampaignOptionsControllerVM(mBBindingList);
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		OptionsController.RefreshValues();
	}

	private void OnOptionChange(string identifier)
	{
	}

	public override bool CanAdvanceToNextStage()
	{
		return true;
	}

	public override void OnNextStage()
	{
		_affirmativeAction();
	}

	public override void OnPreviousStage()
	{
		_negativeAction();
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
