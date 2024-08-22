using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CharacterCreationContent;

public class CharacterCreationState : PlayerGameState
{
	private CharacterCreation _characterCreation;

	private ICharacterCreationStateHandler _handler;

	private readonly List<KeyValuePair<int, Type>> _stages;

	private int _stageIndex = -1;

	private int _furthestStageIndex;

	public readonly CharacterCreationContentBase CurrentCharacterCreationContent;

	public CharacterCreation CharacterCreation
	{
		get
		{
			return _characterCreation;
		}
		private set
		{
			_characterCreation = value;
		}
	}

	public ICharacterCreationStateHandler Handler
	{
		get
		{
			return _handler;
		}
		set
		{
			_handler = value;
		}
	}

	public CharacterCreationStageBase CurrentStage { get; private set; }

	public CharacterCreationState(CharacterCreationContentBase baseContent)
	{
		CharacterCreation = new CharacterCreation();
		CurrentCharacterCreationContent = baseContent;
		CurrentCharacterCreationContent.Initialize(CharacterCreation);
		_stages = new List<KeyValuePair<int, Type>>();
		int key = 0;
		foreach (Type characterCreationStage in CurrentCharacterCreationContent.CharacterCreationStages)
		{
			if (characterCreationStage.IsSubclassOf(typeof(CharacterCreationStageBase)))
			{
				_stages.Add(new KeyValuePair<int, Type>(key, characterCreationStage));
			}
			else
			{
				Debug.FailedAssert("Invalid character creation stage type: " + characterCreationStage.Name, "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CharacterCreationContent\\CharacterCreationState.cs", ".ctor", 54);
			}
		}
	}

	public CharacterCreationState()
	{
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		Game.Current.GameStateManager.RegisterActiveStateDisableRequest(this);
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		if (_stageIndex == -1 && CharacterCreation != null)
		{
			NextStage();
		}
	}

	public void FinalizeCharacterCreation()
	{
		CharacterCreation.ApplyFinalEffects();
		Game.Current.GameStateManager.UnregisterActiveStateDisableRequest(this);
		Game.Current.GameStateManager.CleanAndPushState(Game.Current.GameStateManager.CreateState<MapState>());
		PartyBase.MainParty.SetVisualAsDirty();
		_handler?.OnCharacterCreationFinalized();
		CurrentCharacterCreationContent.OnCharacterCreationFinalized();
		CampaignEventDispatcher.Instance.OnCharacterCreationIsOver();
	}

	public void NextStage()
	{
		_stageIndex++;
		CurrentStage?.OnFinalize();
		_furthestStageIndex = TaleWorlds.Library.MathF.Max(_furthestStageIndex, _stageIndex);
		if (_stageIndex == _stages.Count)
		{
			FinalizeCharacterCreation();
			return;
		}
		Type value = _stages[_stageIndex].Value;
		CreateStage(value);
		Refresh();
	}

	public void PreviousStage()
	{
		CurrentStage?.OnFinalize();
		_stageIndex--;
		Type value = _stages[_stageIndex].Value;
		CreateStage(value);
		Refresh();
	}

	private void CreateStage(Type type)
	{
		CurrentStage = Activator.CreateInstance(type, this) as CharacterCreationStageBase;
		_handler?.OnStageCreated(CurrentStage);
	}

	public void Refresh()
	{
		if (CurrentStage == null || _stageIndex < 0 || _stageIndex >= _stages.Count)
		{
			Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\CharacterCreationContent\\CharacterCreationState.cs", "Refresh", 139);
		}
		else
		{
			_handler?.OnRefresh();
		}
	}

	public int GetTotalStagesCount()
	{
		return _stages.Count;
	}

	public int GetIndexOfCurrentStage()
	{
		return _stageIndex;
	}

	public int GetFurthestIndex()
	{
		return _furthestStageIndex;
	}

	public void GoToStage(int stageIndex)
	{
		if (stageIndex >= 0 && stageIndex < _stages.Count && stageIndex != _stageIndex && stageIndex <= _furthestStageIndex)
		{
			_stageIndex = stageIndex + 1;
			PreviousStage();
		}
	}
}
