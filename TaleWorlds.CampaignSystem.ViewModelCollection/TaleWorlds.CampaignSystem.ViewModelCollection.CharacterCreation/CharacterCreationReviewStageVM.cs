using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;

public class CharacterCreationReviewStageVM : CharacterCreationStageBaseVM
{
	private readonly CharacterCreationContentBase _currentContent;

	private bool _isBannerAndClanNameSet;

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _doneInputKey;

	private string _name = "";

	private string _nameTextQuestion = "";

	private MBBindingList<CharacterCreationReviewStageItemVM> _reviewList;

	private CharacterCreationGainedPropertiesVM _gainedPropertiesController;

	private ImageIdentifierVM _clanBanner;

	private HintViewModel _cannotAdvanceReasonHint;

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
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != _name)
			{
				_name = value;
				_characterCreation.Name = value;
				OnPropertyChangedWithValue(value, "Name");
				OnRefresh();
			}
		}
	}

	[DataSourceProperty]
	public string NameTextQuestion
	{
		get
		{
			return _nameTextQuestion;
		}
		set
		{
			if (value != _nameTextQuestion)
			{
				_nameTextQuestion = value;
				OnPropertyChangedWithValue(value, "NameTextQuestion");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<CharacterCreationReviewStageItemVM> ReviewList
	{
		get
		{
			return _reviewList;
		}
		set
		{
			if (value != _reviewList)
			{
				_reviewList = value;
				OnPropertyChangedWithValue(value, "ReviewList");
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
	public ImageIdentifierVM ClanBanner
	{
		get
		{
			return _clanBanner;
		}
		set
		{
			if (value != _clanBanner)
			{
				_clanBanner = value;
				OnPropertyChangedWithValue(value, "ClanBanner");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CannotAdvanceReasonHint
	{
		get
		{
			return _cannotAdvanceReasonHint;
		}
		set
		{
			if (value != _cannotAdvanceReasonHint)
			{
				_cannotAdvanceReasonHint = value;
				OnPropertyChangedWithValue(value, "CannotAdvanceReasonHint");
			}
		}
	}

	public CharacterCreationReviewStageVM(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation characterCreation, Action affirmativeAction, TextObject affirmativeActionText, Action negativeAction, TextObject negativeActionText, int currentStageIndex, int totalStagesCount, int furthestIndex, Action<int> goToIndex, bool isBannerAndClanNameSet)
		: base(characterCreation, affirmativeAction, affirmativeActionText, negativeAction, negativeActionText, currentStageIndex, totalStagesCount, furthestIndex, goToIndex)
	{
		ReviewList = new MBBindingList<CharacterCreationReviewStageItemVM>();
		base.Title = new TextObject("{=txjiykNa}Review").ToString();
		base.Description = CharacterCreationContentBase.Instance.ReviewPageDescription.ToString();
		_currentContent = (GameStateManager.Current.ActiveState as CharacterCreationState).CurrentCharacterCreationContent;
		_isBannerAndClanNameSet = isBannerAndClanNameSet;
		Name = _characterCreation.Name;
		NameTextQuestion = new TextObject("{=mHVmrwRQ}Enter your name").ToString();
		AddReviewedItems();
		GainedPropertiesController = new CharacterCreationGainedPropertiesVM(_characterCreation, -1);
		ClanBanner = new ImageIdentifierVM(Clan.PlayerClan.Banner);
		CannotAdvanceReasonHint = new HintViewModel();
	}

	private void AddReviewedItems()
	{
		string text = string.Empty;
		CultureObject selectedCulture = _currentContent.GetSelectedCulture();
		IEnumerable<FeatObject> culturalFeats = selectedCulture.GetCulturalFeats((FeatObject x) => x.IsPositive);
		IEnumerable<FeatObject> culturalFeats2 = selectedCulture.GetCulturalFeats((FeatObject x) => !x.IsPositive);
		foreach (FeatObject item3 in culturalFeats)
		{
			GameTexts.SetVariable("STR1", text);
			GameTexts.SetVariable("STR2", item3.Description);
			text = GameTexts.FindText("str_string_newline_string").ToString();
		}
		foreach (FeatObject item4 in culturalFeats2)
		{
			GameTexts.SetVariable("STR1", text);
			GameTexts.SetVariable("STR2", item4.Description);
			text = GameTexts.FindText("str_string_newline_string").ToString();
		}
		CharacterCreationReviewStageItemVM item = new CharacterCreationReviewStageItemVM(new TextObject("{=K6GYskvJ}Culture:").ToString(), _currentContent.GetSelectedCulture().Name.ToString(), text);
		ReviewList.Add(item);
		for (int i = 0; i < _characterCreation.CharacterCreationMenuCount; i++)
		{
			IEnumerable<int> selectedOptions = _characterCreation.GetSelectedOptions(i);
			foreach (CharacterCreationOption item5 in from s in _characterCreation.GetCurrentMenuOptions(i)
				where selectedOptions.Contains(s.Id)
				select s)
			{
				item = new CharacterCreationReviewStageItemVM(_characterCreation.GetCurrentMenuTitle(i).ToString(), item5.Text.ToString(), item5.PositiveEffectText.ToString());
				ReviewList.Add(item);
			}
		}
		if (_isBannerAndClanNameSet)
		{
			CharacterCreationReviewStageItemVM item2 = new CharacterCreationReviewStageItemVM(new ImageIdentifierVM(BannerCode.CreateFrom(Clan.PlayerClan.Banner), nineGrid: true), GameTexts.FindText("str_clan").ToString(), Clan.PlayerClan.Name.ToString(), null);
			ReviewList.Add(item2);
		}
	}

	public void ExecuteRandomizeName()
	{
		CharacterCreationContentBase currentCharacterCreationContent = (GameStateManager.Current.ActiveState as CharacterCreationState).CurrentCharacterCreationContent;
		Name = NameGenerator.Current.GenerateFirstNameForPlayer(currentCharacterCreationContent.GetSelectedCulture(), Hero.MainHero.IsFemale).ToString();
	}

	private void OnRefresh()
	{
		TextObject textObject = GameTexts.FindText("str_generic_character_firstname");
		textObject.SetTextVariable("CHARACTER_FIRSTNAME", new TextObject(Name));
		TextObject textObject2 = GameTexts.FindText("str_generic_character_name");
		textObject2.SetTextVariable("CHARACTER_NAME", new TextObject(Name));
		textObject2.SetTextVariable("CHARACTER_GENDER", Hero.MainHero.IsFemale ? 1 : 0);
		textObject.SetTextVariable("CHARACTER_GENDER", Hero.MainHero.IsFemale ? 1 : 0);
		Hero.MainHero.SetName(textObject2, textObject);
		OnPropertyChanged("CanAdvance");
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
		TextObject hintText = TextObject.Empty;
		bool result = true;
		if (string.IsNullOrEmpty(Name) || string.IsNullOrWhiteSpace(Name))
		{
			hintText = new TextObject("{=IRcy3pWJ}Name cannot be empty");
			result = false;
		}
		Tuple<bool, string> tuple = CampaignUIHelper.IsStringApplicableForHeroName(Name);
		if (!tuple.Item1)
		{
			if (!string.IsNullOrEmpty(tuple.Item2))
			{
				hintText = new TextObject("{=!}" + tuple.Item2);
			}
			result = false;
		}
		CannotAdvanceReasonHint.HintText = hintText;
		return result;
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
