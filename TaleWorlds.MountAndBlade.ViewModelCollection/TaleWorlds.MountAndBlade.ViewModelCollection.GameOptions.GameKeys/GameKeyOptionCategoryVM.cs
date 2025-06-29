using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions.AuxiliaryKeys;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions.GameKeys;

public class GameKeyOptionCategoryVM : ViewModel
{
	private readonly Action<KeyOptionVM> _onKeybindRequest;

	private Dictionary<string, List<GameKey>> _gameKeyCategories;

	private Dictionary<string, List<HotKey>> _auxiliaryKeyCategories;

	private Dictionary<GameKey, InputKey> _keysToChangeOnDone = new Dictionary<GameKey, InputKey>();

	private string _name;

	private string _resetText;

	private bool _isEnabled;

	private MBBindingList<GameKeyGroupVM> _gameKeyGroups;

	private MBBindingList<AuxiliaryKeyGroupVM> _auxiliaryKeyGroups;

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
				OnPropertyChangedWithValue(value, "Name");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value != _isEnabled)
			{
				_isEnabled = value;
				OnPropertyChangedWithValue(value, "IsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public string ResetText
	{
		get
		{
			return _resetText;
		}
		set
		{
			if (value != _resetText)
			{
				_resetText = value;
				OnPropertyChangedWithValue(value, "ResetText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<GameKeyGroupVM> GameKeyGroups
	{
		get
		{
			return _gameKeyGroups;
		}
		set
		{
			if (value != _gameKeyGroups)
			{
				_gameKeyGroups = value;
				OnPropertyChangedWithValue(value, "GameKeyGroups");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<AuxiliaryKeyGroupVM> AuxiliaryKeyGroups
	{
		get
		{
			return _auxiliaryKeyGroups;
		}
		set
		{
			if (value != _auxiliaryKeyGroups)
			{
				_auxiliaryKeyGroups = value;
				OnPropertyChangedWithValue(value, "AuxiliaryKeyGroups");
			}
		}
	}

	public GameKeyOptionCategoryVM(Action<KeyOptionVM> onKeybindRequest, IEnumerable<string> gameKeyCategories)
	{
		_gameKeyCategories = new Dictionary<string, List<GameKey>>();
		foreach (string gameKeyCategory in gameKeyCategories)
		{
			_gameKeyCategories.Add(gameKeyCategory, new List<GameKey>());
		}
		_onKeybindRequest = onKeybindRequest;
		GameKeyGroups = new MBBindingList<GameKeyGroupVM>();
		_auxiliaryKeyCategories = new Dictionary<string, List<HotKey>>();
		AuxiliaryKeyGroups = new MBBindingList<AuxiliaryKeyGroupVM>();
		foreach (GameKeyContext allCategory in HotKeyManager.GetAllCategories())
		{
			if (allCategory.Type == GameKeyContext.GameKeyContextType.AuxiliarySerializedAndShownInOptions)
			{
				_auxiliaryKeyCategories.Add(allCategory.GameKeyCategoryId, new List<HotKey>());
				foreach (HotKey registeredHotKey in allCategory.RegisteredHotKeys)
				{
					if (registeredHotKey != null && _auxiliaryKeyCategories.TryGetValue(registeredHotKey.GroupId, out var value) && !value.Contains(registeredHotKey))
					{
						value.Add(registeredHotKey);
					}
				}
			}
			else
			{
				if (allCategory.Type != 0)
				{
					continue;
				}
				foreach (GameKey registeredGameKey in allCategory.RegisteredGameKeys)
				{
					if (registeredGameKey != null && _gameKeyCategories.TryGetValue(registeredGameKey.MainCategoryId, out var value2) && !value2.Contains(registeredGameKey))
					{
						value2.Add(registeredGameKey);
					}
				}
			}
		}
		foreach (KeyValuePair<string, List<GameKey>> gameKeyCategory2 in _gameKeyCategories)
		{
			if (gameKeyCategory2.Value.Count > 0)
			{
				GameKeyGroups.Add(new GameKeyGroupVM(gameKeyCategory2.Key, gameKeyCategory2.Value, _onKeybindRequest, UpdateKeysOfGamekeysWithID));
			}
		}
		foreach (KeyValuePair<string, List<HotKey>> auxiliaryKeyCategory in _auxiliaryKeyCategories)
		{
			if (auxiliaryKeyCategory.Value.Count > 0)
			{
				AuxiliaryKeyGroups.Add(new AuxiliaryKeyGroupVM(auxiliaryKeyCategory.Key, auxiliaryKeyCategory.Value, _onKeybindRequest));
			}
		}
		RefreshValues();
		TaleWorlds.InputSystem.Input.OnGamepadActiveStateChanged = (Action)Delegate.Combine(TaleWorlds.InputSystem.Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
		IsEnabled = !TaleWorlds.InputSystem.Input.IsGamepadActive;
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		Name = new TextObject("{=qmNeO8FG}Keybindings").ToString();
		ResetText = new TextObject("{=RVIKFCno}Reset to Defaults").ToString();
		GameKeyGroups.ApplyActionOnAllItems(delegate(GameKeyGroupVM x)
		{
			x.RefreshValues();
		});
		AuxiliaryKeyGroups.ApplyActionOnAllItems(delegate(AuxiliaryKeyGroupVM x)
		{
			x.RefreshValues();
		});
	}

	private void OnGamepadActiveStateChanged()
	{
		GameKeyGroups?.ApplyActionOnAllItems(delegate(GameKeyGroupVM g)
		{
			g.OnGamepadActiveStateChanged();
		});
		AuxiliaryKeyGroups.ApplyActionOnAllItems(delegate(AuxiliaryKeyGroupVM x)
		{
			x.OnGamepadActiveStateChanged();
		});
		IsEnabled = !TaleWorlds.InputSystem.Input.IsGamepadActive;
		Debug.Print("KEYBINDS TAB ENABLED: " + IsEnabled);
	}

	public bool IsChanged()
	{
		if (!GameKeyGroups.Any((GameKeyGroupVM x) => x.IsChanged()))
		{
			return AuxiliaryKeyGroups.Any((AuxiliaryKeyGroupVM x) => x.IsChanged());
		}
		return true;
	}

	public void ExecuteResetToDefault()
	{
		InformationManager.ShowInquiry(new InquiryData(new TextObject("{=4gCU2ykB}Reset all keys to default").ToString(), new TextObject("{=YjbNtFcw}This will reset ALL keys to their default states. You won't be able to undo this action. {newline} {newline}Are you sure?").ToString(), isAffirmativeOptionShown: true, isNegativeOptionShown: true, new TextObject("{=aeouhelq}Yes").ToString(), new TextObject("{=8OkPHu4f}No").ToString(), delegate
		{
			ResetToDefault();
		}, null));
	}

	public void OnDone()
	{
		GameKeyGroups.ApplyActionOnAllItems(delegate(GameKeyGroupVM x)
		{
			x.OnDone();
		});
		AuxiliaryKeyGroups.ApplyActionOnAllItems(delegate(AuxiliaryKeyGroupVM x)
		{
			x.OnDone();
		});
		foreach (KeyValuePair<GameKey, InputKey> item in _keysToChangeOnDone)
		{
			FindValidInputKey(item.Key)?.ChangeKey(item.Value);
		}
	}

	private void ResetToDefault()
	{
		HotKeyManager.Reset();
		GameKeyGroups.ApplyActionOnAllItems(delegate(GameKeyGroupVM x)
		{
			x.Update();
		});
		AuxiliaryKeyGroups.ApplyActionOnAllItems(delegate(AuxiliaryKeyGroupVM x)
		{
			x.Update();
		});
		_keysToChangeOnDone.Clear();
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
		TaleWorlds.InputSystem.Input.OnGamepadActiveStateChanged = (Action)Delegate.Remove(TaleWorlds.InputSystem.Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
	}

	private Key FindValidInputKey(GameKey gameKey)
	{
		if (!TaleWorlds.InputSystem.Input.IsGamepadActive)
		{
			return gameKey.KeyboardKey;
		}
		return gameKey.ControllerKey;
	}

	private void UpdateKeysOfGamekeysWithID(int givenId, InputKey newKey)
	{
		foreach (GameKeyContext allCategory in HotKeyManager.GetAllCategories())
		{
			if (allCategory.Type != 0)
			{
				continue;
			}
			foreach (GameKey item in allCategory.RegisteredGameKeys.Where((GameKey k) => k != null && k.Id == givenId))
			{
				if (_keysToChangeOnDone.ContainsKey(item))
				{
					_keysToChangeOnDone[item] = newKey;
				}
				else
				{
					_keysToChangeOnDone.Add(item, newKey);
				}
			}
		}
	}

	public void Cancel()
	{
		GameKeyGroups.ApplyActionOnAllItems(delegate(GameKeyGroupVM g)
		{
			g.Cancel();
		});
	}

	public void ApplyValues()
	{
		GameKeyGroups.ApplyActionOnAllItems(delegate(GameKeyGroupVM g)
		{
			g.ApplyValues();
		});
	}
}
