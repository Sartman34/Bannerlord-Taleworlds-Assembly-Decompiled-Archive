using System;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;

public abstract class CharacterCreationStageBaseVM : ViewModel
{
	protected readonly TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation _characterCreation;

	protected readonly Action _affirmativeAction;

	protected readonly Action _negativeAction;

	protected readonly TextObject _affirmativeActionText;

	protected readonly TextObject _negativeActionText;

	private readonly Action<int> _goToIndex;

	private string _title = "";

	private string _description = "";

	private string _selectionText = "";

	private int _totalStageCount = -1;

	private int _currentStageIndex = -1;

	private int _furthestIndex = -1;

	private bool _anyItemSelected;

	public bool CanAdvance => CanAdvanceToNextStage();

	public string NextStageText => _affirmativeActionText.ToString();

	public string PreviousStageText => _negativeActionText.ToString();

	[DataSourceProperty]
	public string Title
	{
		get
		{
			return _title;
		}
		set
		{
			if (value != _title)
			{
				_title = value;
				OnPropertyChangedWithValue(value, "Title");
			}
		}
	}

	[DataSourceProperty]
	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (value != _description)
			{
				_description = value;
				OnPropertyChangedWithValue(value, "Description");
			}
		}
	}

	[DataSourceProperty]
	public string SelectionText
	{
		get
		{
			return _selectionText;
		}
		set
		{
			if (value != _selectionText)
			{
				_selectionText = value;
				OnPropertyChangedWithValue(value, "SelectionText");
			}
		}
	}

	[DataSourceProperty]
	public int TotalStageCount
	{
		get
		{
			return _totalStageCount;
		}
		set
		{
			if (value != _totalStageCount)
			{
				_totalStageCount = value;
				OnPropertyChangedWithValue(value, "TotalStageCount");
			}
		}
	}

	[DataSourceProperty]
	public int FurthestIndex
	{
		get
		{
			return _furthestIndex;
		}
		set
		{
			if (value != _furthestIndex)
			{
				_furthestIndex = value;
				OnPropertyChangedWithValue(value, "FurthestIndex");
			}
		}
	}

	[DataSourceProperty]
	public int CurrentStageIndex
	{
		get
		{
			return _currentStageIndex;
		}
		set
		{
			if (value != _currentStageIndex)
			{
				_currentStageIndex = value;
				OnPropertyChangedWithValue(value, "CurrentStageIndex");
			}
		}
	}

	[DataSourceProperty]
	public bool AnyItemSelected
	{
		get
		{
			return _anyItemSelected;
		}
		set
		{
			if (value != _anyItemSelected)
			{
				_anyItemSelected = value;
				OnPropertyChangedWithValue(value, "AnyItemSelected");
			}
		}
	}

	protected CharacterCreationStageBaseVM(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation characterCreation, Action affirmativeAction, TextObject affirmativeActionText, Action negativeAction, TextObject negativeActionText, int currentStageIndex, int totalStagesCount, int furthestIndex, Action<int> goToIndex)
	{
		_characterCreation = characterCreation;
		_goToIndex = goToIndex;
		_affirmativeAction = affirmativeAction;
		_negativeAction = negativeAction;
		_affirmativeActionText = affirmativeActionText;
		_negativeActionText = negativeActionText;
		TotalStageCount = totalStagesCount;
		CurrentStageIndex = currentStageIndex;
		FurthestIndex = furthestIndex;
	}

	public abstract void OnNextStage();

	public abstract void OnPreviousStage();

	public abstract bool CanAdvanceToNextStage();

	public virtual void ExecuteGoToIndex(int index)
	{
		_goToIndex(index);
	}
}
