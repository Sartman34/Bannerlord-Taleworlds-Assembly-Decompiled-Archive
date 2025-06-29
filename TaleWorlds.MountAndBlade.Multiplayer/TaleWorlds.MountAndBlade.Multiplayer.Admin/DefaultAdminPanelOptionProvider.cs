using System.Collections.Generic;
using System.Linq;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.Admin.Internal;

namespace TaleWorlds.MountAndBlade.Multiplayer.Admin;

public class DefaultAdminPanelOptionProvider : IAdminPanelOptionProvider
{
	public static class DefaultOptionIds
	{
		public const string NextGameType = "next_game_type";

		public const string NextMap = "next_map";

		public const string NextCultureTeam1 = "next_culture_team_1";

		public const string NextCultureTeam2 = "next_culture_team_2";

		public const string NextNumberOfRounds = "next_number_of_rounds";

		public const string NextMinScoreToWinDuel = "next_min_score_to_win_duel";

		public const string NextMapTimeLimit = "next_map_time_limit";

		public const string NextRoundTimeLimit = "next_round_time_limit";

		public const string NextWarmupTimeLimit = "next_warmup_time_limit";

		public const string NextMaxNumberOfPlayers = "next_max_num_players";

		public const string ApplyAndStartMission = "apply_and_start";

		public const string WelcomeMessage = "welcome_message";

		public const string AutoTeamBalanceTreshold = "auto_balance_treshold";

		public const string FriendlyFireMeleePercent = "friendly_fire_melee_percent";

		public const string FriendlyFireMeleeReflectionPercent = "friendly_fire_melee_self_percent";

		public const string FriendlyFireRangedPercent = "friendly_fire_ranged_percent";

		public const string FriendlyFireRangedReflectionPercent = "friendly_fire_ranged_self_percent";

		public const string AllowInfantry = "allow_infantry";

		public const string AllowRanged = "allow_ranged";

		public const string AllowCavalry = "allow_cavalry";

		public const string AllowHorseArchers = "allow_horse_archers";

		public const string EndWarmup = "end_warmup";

		public const string MutePlayer = "mute_player";

		public const string KickPlayer = "kick_player";

		public const string BanPlayer = "ban_player";
	}

	private class AdminPanelVotableMultiSelectionOption : AdminPanelMultiSelectionOption
	{
		protected readonly IAdminPanelMultiSelectionItem _undecidedOption;

		public bool IsUndecided { get; private set; }

		public AdminPanelVotableMultiSelectionOption(string uniqueId)
			: base(uniqueId)
		{
			_undecidedOption = new AdminPanelMultiSelectionItem(null, new TextObject("{=*}Undecided"), isFallbackValue: true);
		}

		protected override void OnValueChanged(IAdminPanelMultiSelectionItem previousValue, IAdminPanelMultiSelectionItem newValue)
		{
			base.OnValueChanged(previousValue, newValue);
			IsUndecided = _selectedOption == _undecidedOption;
		}

		public override AdminPanelMultiSelectionOption BuildAvailableOptions(MBReadOnlyList<IAdminPanelMultiSelectionItem> options)
		{
			base.BuildAvailableOptions(options);
			AddUndecidedOption();
			if (!_availableOptions.Contains(base.CurrentValue) && _availableOptions.Count > 0)
			{
				BuildInitialValue(_availableOptions[0]);
				SetValue(_availableOptions[0]);
			}
			return this;
		}

		public override AdminPanelMultiSelectionOption BuildAvailableOptions(MultiplayerOptions.OptionType optionType, bool buildDefaultValue = true)
		{
			base.BuildAvailableOptions(optionType, buildDefaultValue: false);
			AddUndecidedOption();
			if (!_availableOptions.Contains(base.CurrentValue) && _availableOptions.Count > 0)
			{
				BuildInitialValue(_availableOptions[0]);
				SetValue(_availableOptions[0]);
			}
			return this;
		}

		protected void AddUndecidedOption()
		{
			for (int i = 0; i < _availableOptions.Count; i++)
			{
				if (_availableOptions[i] == _undecidedOption || _availableOptions[i].Value == _undecidedOption.Value)
				{
					return;
				}
			}
			if (!GetIsDisabled(out var _))
			{
				_availableOptions.Insert(0, _undecidedOption);
				BuildDefaultValue(_undecidedOption);
				BuildInitialValue(_undecidedOption);
				SetValue(_undecidedOption);
			}
		}

		protected void RemoveUndecidedOption()
		{
			bool flag = false;
			for (int i = 0; i < _availableOptions.Count; i++)
			{
				if (_availableOptions[i] == _undecidedOption || _availableOptions[i].Value == _undecidedOption.Value)
				{
					_availableOptions.RemoveAt(i);
					flag = true;
					break;
				}
			}
			if (flag && _availableOptions.Count > 0)
			{
				IAdminPanelMultiSelectionItem value = _availableOptions[0];
				BuildDefaultValue(value);
				BuildInitialValue(value);
				SetValue(value);
			}
		}
	}

	private class AdminPanelCultureOption : AdminPanelVotableMultiSelectionOption
	{
		private bool _shouldKeepUndecidedOption;

		private AdminPanelCultureOption _otherOption;

		public AdminPanelCultureOption(string uniqueId)
			: base(uniqueId)
		{
		}

		public AdminPanelCultureOption BuildOtherCultureOption(AdminPanelCultureOption otherOption)
		{
			_otherOption?.RemoveValueChangedCallback(OnOtherOptionValueChanged);
			_otherOption = otherOption;
			_otherOption?.AddValueChangedCallback(OnOtherOptionValueChanged);
			return this;
		}

		public override void OnFinalize()
		{
			base.OnFinalize();
			_otherOption?.RemoveValueChangedCallback(OnOtherOptionValueChanged);
		}

		protected override void OnValueChanged(IAdminPanelMultiSelectionItem previousValue, IAdminPanelMultiSelectionItem newValue)
		{
			bool isUndecided = base.IsUndecided;
			base.OnValueChanged(previousValue, newValue);
			if (isUndecided && !base.IsUndecided)
			{
				_shouldKeepUndecidedOption = true;
			}
			else if (!isUndecided && base.IsUndecided)
			{
				_shouldKeepUndecidedOption = false;
			}
		}

		private void OnOtherOptionValueChanged()
		{
			if (_otherOption.IsUndecided)
			{
				AddUndecidedOption();
			}
			else if (!_shouldKeepUndecidedOption)
			{
				RemoveUndecidedOption();
			}
		}
	}

	private class AdminPanelUsableMapsOption : AdminPanelVotableMultiSelectionOption
	{
		private const string _disabledOptionTag = "map_option_disabled";

		private const string _undecidedOptionTag = "map_option_undecided";

		private readonly Dictionary<string, MBList<IAdminPanelMultiSelectionItem>> _optionsByGameType;

		private readonly IAdminPanelMultiSelectionItem _disabledOption;

		private bool _isUpdatingOptions;

		private AdminPanelVotableMultiSelectionOption _gameTypeOption;

		public AdminPanelUsableMapsOption(string uniqueId)
			: base(uniqueId)
		{
			_optionsByGameType = new Dictionary<string, MBList<IAdminPanelMultiSelectionItem>>();
			_disabledOption = new AdminPanelMultiSelectionItem(null, new TextObject("{=*}Disabled"), isFallbackValue: false, isDisabled: true);
			_optionsByGameType["map_option_disabled"] = new MBList<IAdminPanelMultiSelectionItem> { _disabledOption };
			_optionsByGameType["map_option_undecided"] = new MBList<IAdminPanelMultiSelectionItem> { _undecidedOption };
		}

		public AdminPanelUsableMapsOption BuildGameTypeOption(AdminPanelVotableMultiSelectionOption gameTypeOption)
		{
			_gameTypeOption = gameTypeOption;
			_gameTypeOption?.AddValueChangedCallback(UpdateOptions);
			UpdateOptions();
			return this;
		}

		public override void OnFinalize()
		{
			base.OnFinalize();
			_gameTypeOption?.RemoveValueChangedCallback(UpdateOptions);
			_gameTypeOption = null;
		}

		public override bool GetIsDisabled(out string reason)
		{
			if (_availableOptions.Count == 1 && _availableOptions[0] == _disabledOption)
			{
				reason = new TextObject("{=*}No available maps added for game type").ToString();
				return true;
			}
			reason = string.Empty;
			return false;
		}

		private void UpdateOptions()
		{
			if (_isUpdatingOptions)
			{
				return;
			}
			_isUpdatingOptions = true;
			IAdminPanelMultiSelectionItem value = _gameTypeOption.GetValue();
			List<string> usableMaps = MultiplayerIntermissionVotingManager.Instance.GetUsableMaps(value.Value);
			FilterAvailableOptions(usableMaps);
			string key = (_gameTypeOption.IsUndecided ? "map_option_undecided" : ((usableMaps == null || usableMaps.Count <= 0) ? "map_option_disabled" : value.Value));
			if (_optionsByGameType.TryGetValue(key, out var value2))
			{
				if (!_availableOptions.SequenceEqual(value2))
				{
					BuildAvailableOptions(value2);
				}
				_isUpdatingOptions = false;
				return;
			}
			MBList<IAdminPanelMultiSelectionItem> mBList = new MBList<IAdminPanelMultiSelectionItem>();
			for (int i = 0; i < usableMaps.Count; i++)
			{
				AdminPanelMultiSelectionItem item = new AdminPanelMultiSelectionItem(usableMaps[i], null);
				mBList.Add(item);
			}
			BuildAvailableOptions(mBList);
			_optionsByGameType[key] = mBList;
			_isUpdatingOptions = false;
		}

		private void FilterAvailableOptions(List<string> availableOptions)
		{
			if (availableOptions.Count == 0)
			{
				return;
			}
			MBReadOnlyList<MultiplayerGameTypeInfo> multiplayerGameTypes = Module.CurrentModule.GetMultiplayerGameTypes();
			List<string> list = new List<string>();
			MultiplayerGameTypeInfo multiplayerGameTypeInfo = multiplayerGameTypes.FirstOrDefault((MultiplayerGameTypeInfo x) => x.GameType == _gameTypeOption.GetValue()?.Value);
			if (multiplayerGameTypeInfo == null)
			{
				return;
			}
			IEnumerable<string> source = multiplayerGameTypes.SelectMany((MultiplayerGameTypeInfo g) => g.Scenes);
			for (int i = 0; i < availableOptions.Count; i++)
			{
				string text = availableOptions[i];
				if (source.Contains(text) && !multiplayerGameTypeInfo.Scenes.Contains(text))
				{
					list.Add(text);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				string item = list[j];
				availableOptions.Remove(item);
			}
		}
	}

	private class AdminPanelStartMissionAction : AdminPanelAction
	{
		private MBReadOnlyList<IAdminPanelOptionGroup> _optionGroups;

		public AdminPanelStartMissionAction(string uniqueId)
			: base(uniqueId)
		{
		}

		public AdminPanelStartMissionAction BuildOptionGroups(MBReadOnlyList<IAdminPanelOptionGroup> optionGroups)
		{
			_optionGroups = optionGroups;
			return this;
		}

		public override bool GetIsDisabled(out string reason)
		{
			reason = string.Empty;
			if (_optionGroups != null)
			{
				for (int i = 0; i < _optionGroups.Count; i++)
				{
					for (int j = 0; j < _optionGroups[i].Options.Count; j++)
					{
						IAdminPanelOption adminPanelOption = _optionGroups[i].Options[j];
						if (adminPanelOption.IsRequired && adminPanelOption.GetIsAvailable() && adminPanelOption.GetIsDisabled(out var _))
						{
							reason = new TextObject("{=*}Please select valid values for options.").ToString();
							return true;
						}
					}
				}
			}
			if (!MultiplayerIntermissionVotingManager.Instance.IsAutomatedBattleSwitchingEnabled)
			{
				reason = new TextObject("{=*}Server does not support automated battle switching.").ToString();
				return true;
			}
			return false;
		}

		public override void OnFinalize()
		{
			base.OnFinalize();
			_optionGroups = null;
		}
	}

	private class AdminPanelGameTypeDependentNumericOption : AdminPanelNumericOption
	{
		private AdminPanelVotableMultiSelectionOption _gameTypeOption;

		private List<string> _invalidGameTypes;

		private List<string> _requiredGameTypes;

		public AdminPanelGameTypeDependentNumericOption(string uniqueId)
			: base(uniqueId)
		{
		}

		public override bool GetIsAvailable()
		{
			if (_gameTypeOption == null)
			{
				Debug.Print("Game type option is not set for game type dependent option: " + base.Name);
				Debug.FailedAssert("Game type option is not set for game type dependent option: " + base.Name, "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Admin\\DefaultAdminPanelOptionProvider.cs", "GetIsAvailable", 994);
				return true;
			}
			if (_gameTypeOption.IsUndecided)
			{
				return true;
			}
			string value = _gameTypeOption.GetValue().Value;
			if (string.IsNullOrEmpty(value))
			{
				return true;
			}
			if (_invalidGameTypes != null)
			{
				return !_invalidGameTypes.Contains(value);
			}
			if (_requiredGameTypes != null)
			{
				return _requiredGameTypes.Contains(value);
			}
			return true;
		}

		public AdminPanelGameTypeDependentNumericOption BuildGameTypeOption(AdminPanelVotableMultiSelectionOption gameTypeOption)
		{
			_gameTypeOption = gameTypeOption;
			return this;
		}

		public AdminPanelGameTypeDependentNumericOption BuildInvalidGameTypes(string[] gameTypes)
		{
			_invalidGameTypes = new List<string>();
			if (gameTypes != null)
			{
				for (int i = 0; i < gameTypes.Length; i++)
				{
					_invalidGameTypes.Add(gameTypes[i]);
				}
			}
			return this;
		}

		public AdminPanelGameTypeDependentNumericOption BuildRequiredGameTypes(string[] gameTypes)
		{
			_requiredGameTypes = new List<string>();
			if (gameTypes != null)
			{
				for (int i = 0; i < gameTypes.Length; i++)
				{
					_requiredGameTypes.Add(gameTypes[i]);
				}
			}
			return this;
		}
	}

	private class AdminPanelGameTypeDependentAction : AdminPanelAction
	{
		private AdminPanelVotableMultiSelectionOption _gameTypeOption;

		private List<string> _invalidGameTypes;

		private List<string> _requiredGameTypes;

		public AdminPanelGameTypeDependentAction(string uniqueId)
			: base(uniqueId)
		{
		}

		public override bool GetIsAvailable()
		{
			if (_gameTypeOption.IsUndecided)
			{
				return true;
			}
			string value = _gameTypeOption.GetValue().Value;
			if (string.IsNullOrEmpty(value))
			{
				return true;
			}
			if (_invalidGameTypes != null)
			{
				return !_invalidGameTypes.Contains(value);
			}
			if (_requiredGameTypes != null)
			{
				return _requiredGameTypes.Contains(value);
			}
			return true;
		}

		public AdminPanelGameTypeDependentAction BuildGameTypeOption(AdminPanelVotableMultiSelectionOption gameTypeOption)
		{
			_gameTypeOption = gameTypeOption;
			return this;
		}

		public AdminPanelGameTypeDependentAction BuildInvalidGameTypes(string[] gameTypes)
		{
			_invalidGameTypes = new List<string>();
			if (gameTypes != null)
			{
				for (int i = 0; i < gameTypes.Length; i++)
				{
					_invalidGameTypes.Add(gameTypes[i]);
				}
			}
			return this;
		}

		public AdminPanelGameTypeDependentAction BuildRequiredGameTypes(string[] gameTypes)
		{
			_requiredGameTypes = new List<string>();
			if (gameTypes != null)
			{
				for (int i = 0; i < gameTypes.Length; i++)
				{
					_requiredGameTypes.Add(gameTypes[i]);
				}
			}
			return this;
		}
	}

	private readonly MultiplayerAdminComponent _multiplayerAdminComponent;

	private readonly MissionLobbyComponent _missionLobbyComponent;

	private MBList<IAdminPanelOptionGroup> _optionGroups;

	private AdminPanelVotableMultiSelectionOption _gameTypeOption;

	public DefaultAdminPanelOptionProvider(MultiplayerAdminComponent adminComponent, MissionLobbyComponent missionLobbyComponent)
	{
		_multiplayerAdminComponent = adminComponent;
		_missionLobbyComponent = missionLobbyComponent;
		_optionGroups = new MBList<IAdminPanelOptionGroup>();
	}

	public void OnTick(float dt)
	{
		for (int i = 0; i < _optionGroups.Count; i++)
		{
			if (_optionGroups[i] is IAdminPanelTickable adminPanelTickable)
			{
				adminPanelTickable.OnTick(dt);
			}
		}
	}

	public void OnFinalize()
	{
		if (_optionGroups != null)
		{
			for (int i = 0; i < _optionGroups.Count; i++)
			{
				_optionGroups[i].OnFinalize();
			}
		}
		_gameTypeOption = null;
	}

	public IAdminPanelOption GetOptionWithId(string id)
	{
		foreach (IAdminPanelOptionGroup optionGroup in _optionGroups)
		{
			foreach (IAdminPanelOption option in optionGroup.Options)
			{
				if (option.UniqueId == id)
				{
					return option;
				}
			}
		}
		return null;
	}

	public IAdminPanelAction GetActionWithId(string id)
	{
		foreach (IAdminPanelOptionGroup optionGroup in _optionGroups)
		{
			foreach (IAdminPanelAction action in optionGroup.Actions)
			{
				if (action.UniqueId == id)
				{
					return action;
				}
			}
		}
		return null;
	}

	public void ApplyOptions()
	{
		AdminUpdateMultiplayerOptions adminUpdateMultiplayerOptions = new AdminUpdateMultiplayerOptions();
		IEnumerable<IAdminPanelOption> enumerable = _optionGroups.SelectMany((IAdminPanelOptionGroup x) => x.Options);
		foreach (IAdminPanelOption item in enumerable)
		{
			if (!(item is IAdminPanelOptionInternal adminPanelOptionInternal))
			{
				continue;
			}
			MultiplayerOptions.OptionType optionType = adminPanelOptionInternal.GetOptionType();
			MultiplayerOptions.MultiplayerOptionsAccessMode optionAccessMode = adminPanelOptionInternal.GetOptionAccessMode();
			if (optionType != MultiplayerOptions.OptionType.NumOfSlots && optionAccessMode != MultiplayerOptions.MultiplayerOptionsAccessMode.NumAccessModes)
			{
				if (item is IAdminPanelOption<bool> adminPanelOption)
				{
					adminUpdateMultiplayerOptions.AddMultiplayerOption(optionType, optionAccessMode, adminPanelOption.GetValue());
				}
				if (item is IAdminPanelOption<int> adminPanelOption2)
				{
					adminUpdateMultiplayerOptions.AddMultiplayerOption(optionType, optionAccessMode, adminPanelOption2.GetValue());
				}
				if (item is IAdminPanelOption<string> adminPanelOption3)
				{
					adminUpdateMultiplayerOptions.AddMultiplayerOption(optionType, optionAccessMode, adminPanelOption3.GetValue());
				}
				if (item is IAdminPanelMultiSelectionOption adminPanelMultiSelectionOption)
				{
					adminUpdateMultiplayerOptions.AddMultiplayerOption(optionType, optionAccessMode, adminPanelMultiSelectionOption.GetValue().Value);
				}
			}
		}
		GameNetwork.BeginModuleEventAsClient();
		GameNetwork.WriteMessage(adminUpdateMultiplayerOptions);
		GameNetwork.EndModuleEventAsClient();
		foreach (IAdminPanelOption item2 in enumerable)
		{
			if (item2 is IAdminPanelOptionInternal adminPanelOptionInternal2)
			{
				adminPanelOptionInternal2.OnApplyChanges();
			}
		}
	}

	public MBReadOnlyList<IAdminPanelOptionGroup> GetOptionGroups()
	{
		_optionGroups.Clear();
		if (MultiplayerIntermissionVotingManager.Instance.IsAutomatedBattleSwitchingEnabled)
		{
			_optionGroups.Add(GetMissionOptions());
		}
		_optionGroups.Add(GetImmediateEffectOptions());
		_optionGroups.Add(GetActions());
		return _optionGroups;
	}

	private T GetValueFromOption<T>(string optionId)
	{
		if (((IAdminPanelOptionProvider)this).GetOptionWithId(optionId) is IAdminPanelOption<T> adminPanelOption)
		{
			return adminPanelOption.GetValue();
		}
		Debug.FailedAssert($"Failed to find \"{typeof(T)}\" type option with id: {optionId}", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Admin\\DefaultAdminPanelOptionProvider.cs", "GetValueFromOption", 185);
		return default(T);
	}

	private AdminPanelOptionGroup GetMissionOptions()
	{
		AdminPanelOptionGroup adminPanelOptionGroup = new AdminPanelOptionGroup("mission_options", new TextObject("{=*}Mission Options"), requiresRestart: true);
		AdminPanelOption<IAdminPanelMultiSelectionItem> adminPanelOption = new AdminPanelVotableMultiSelectionOption("next_game_type").BuildAvailableOptions(MultiplayerOptions.OptionType.GameType).BuildOptionType(MultiplayerOptions.OptionType.GameType, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions, buildDefaultValue: false, buildInitialValue: false).BuildName(new TextObject("{=*}Game Type"))
			.BuildDescription(new TextObject("{=*}Next game type."))
			.BuildIsRequired(isRequired: true);
		_gameTypeOption = adminPanelOption as AdminPanelVotableMultiSelectionOption;
		adminPanelOptionGroup.AddOption(adminPanelOption);
		adminPanelOptionGroup.AddOption(new AdminPanelUsableMapsOption("next_map").BuildGameTypeOption(adminPanelOption as AdminPanelVotableMultiSelectionOption).BuildOptionType(MultiplayerOptions.OptionType.Map, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions, buildDefaultValue: false, buildInitialValue: false).BuildName(new TextObject("{=*}Map"))
			.BuildDescription(new TextObject("{=*}Next map to play."))
			.BuildIsRequired(isRequired: true));
		AdminPanelCultureOption adminPanelCultureOption = new AdminPanelCultureOption("next_culture_team_1").BuildAvailableOptions(MultiplayerOptions.OptionType.CultureTeam1).BuildOptionType(MultiplayerOptions.OptionType.CultureTeam1, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions, buildDefaultValue: false, buildInitialValue: false).BuildName(new TextObject("{=*}Attacker Culture"))
			.BuildDescription(new TextObject("{=*}Culture of the attacker team in the next game."))
			.BuildIsRequired(isRequired: true) as AdminPanelCultureOption;
		AdminPanelCultureOption adminPanelCultureOption2 = new AdminPanelCultureOption("next_culture_team_2").BuildAvailableOptions(MultiplayerOptions.OptionType.CultureTeam2).BuildOptionType(MultiplayerOptions.OptionType.CultureTeam2, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions, buildDefaultValue: false, buildInitialValue: false).BuildName(new TextObject("{=*}Defender Culture"))
			.BuildDescription(new TextObject("{=*}Culture of the defender team in the next game."))
			.BuildIsRequired(isRequired: true) as AdminPanelCultureOption;
		adminPanelCultureOption.BuildOtherCultureOption(adminPanelCultureOption2);
		adminPanelCultureOption2.BuildOtherCultureOption(adminPanelCultureOption);
		adminPanelOptionGroup.AddOption(adminPanelCultureOption);
		adminPanelOptionGroup.AddOption(adminPanelCultureOption2);
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("next_number_of_rounds").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[3]
		{
			MultiplayerGameType.TeamDeathmatch.ToString(),
			MultiplayerGameType.Duel.ToString(),
			MultiplayerGameType.Siege.ToString()
		}).SetMinimumAndMaximumFrom(MultiplayerOptions.OptionType.RoundTotal)
			.BuildOptionType(MultiplayerOptions.OptionType.RoundTotal, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions)
			.BuildName(new TextObject("{=*}Number of Rounds"))
			.BuildDescription(new TextObject("{=*}Total number of rounds in the next game."))
			.BuildIsRequired(isRequired: true));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("next_min_score_to_win_duel").BuildGameTypeOption(_gameTypeOption).BuildRequiredGameTypes(new string[1] { MultiplayerGameType.Duel.ToString() }).SetMinimumAndMaximumFrom(MultiplayerOptions.OptionType.MinScoreToWinDuel)
			.BuildOptionType(MultiplayerOptions.OptionType.MinScoreToWinDuel, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions)
			.BuildName(new TextObject("{=*}Minimum Score to Win Duel"))
			.BuildDescription(new TextObject("{=*}Minimum score required to win duels."))
			.BuildIsRequired(isRequired: true));
		adminPanelOptionGroup.AddOption(new AdminPanelNumericOption("next_map_time_limit").SetMinimumAndMaximumFrom(MultiplayerOptions.OptionType.MapTimeLimit).BuildOptionType(MultiplayerOptions.OptionType.MapTimeLimit, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions).BuildName(new TextObject("{=*}Map Time Limit"))
			.BuildDescription(new TextObject("{=*}Time limit in the next game."))
			.BuildIsRequired(isRequired: true));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("next_round_time_limit").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[3]
		{
			MultiplayerGameType.TeamDeathmatch.ToString(),
			MultiplayerGameType.Duel.ToString(),
			MultiplayerGameType.Siege.ToString()
		}).SetMinimumAndMaximumFrom(MultiplayerOptions.OptionType.RoundTimeLimit)
			.BuildOptionType(MultiplayerOptions.OptionType.RoundTimeLimit, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions)
			.BuildName(new TextObject("{=*}Round Time Limit"))
			.BuildDescription(new TextObject("{=*}Round time limit in the next game."))
			.BuildIsRequired(isRequired: true));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("next_warmup_time_limit").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[2]
		{
			MultiplayerGameType.TeamDeathmatch.ToString(),
			MultiplayerGameType.Duel.ToString()
		}).SetMinimumAndMaximumFrom(MultiplayerOptions.OptionType.WarmupTimeLimit)
			.BuildOptionType(MultiplayerOptions.OptionType.WarmupTimeLimit, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions)
			.BuildName(new TextObject("{=*}Warmup Time Limit"))
			.BuildDescription(new TextObject("{=*}Warmup time limit in the next game."))
			.BuildIsRequired(isRequired: true));
		adminPanelOptionGroup.AddOption(new AdminPanelNumericOption("next_max_num_players").SetMinimumAndMaximumFrom(MultiplayerOptions.OptionType.MaxNumberOfPlayers).BuildOptionType(MultiplayerOptions.OptionType.MaxNumberOfPlayers, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions).BuildName(new TextObject("{=*}Maximum Number of Players"))
			.BuildDescription(new TextObject("{=*}Maximum number of players in the next game."))
			.BuildIsRequired(isRequired: true));
		adminPanelOptionGroup.AddAction(new AdminPanelStartMissionAction("apply_and_start").BuildOptionGroups(_optionGroups).BuildName(new TextObject("{=*}Apply and Start Mission")).BuildDescription(new TextObject("{=*}Apply all changes and start a new mission."))
			.BuildOnActionExecutedCallback(delegate
			{
				ApplyOptions();
				_multiplayerAdminComponent.ChangeAdminMenuActiveState(isActive: false);
				_multiplayerAdminComponent.AdminEndMission();
			}));
		return adminPanelOptionGroup;
	}

	private AdminPanelOptionGroup GetImmediateEffectOptions()
	{
		AdminPanelOptionGroup adminPanelOptionGroup = new AdminPanelOptionGroup("immediate_effects", new TextObject("{=*}Immediate Effects"));
		adminPanelOptionGroup.AddOption(new AdminPanelOption<string>("welcome_message").BuildOptionType(MultiplayerOptions.OptionType.WelcomeMessage).BuildName(new TextObject("{=*}Welcome Message")).BuildDescription(new TextObject("{=*}Change the server welcome message.")));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("auto_balance_treshold").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[1] { MultiplayerGameType.Duel.ToString() }).SetMinimumAndMaximumFrom(MultiplayerOptions.OptionType.AutoTeamBalanceThreshold)
			.BuildOptionType(MultiplayerOptions.OptionType.AutoTeamBalanceThreshold)
			.BuildName(new TextObject("{=*}Team Balance Threshold"))
			.BuildDescription(new TextObject("{=*}Change the team balance threshold value.")));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("friendly_fire_melee_percent").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[1] { MultiplayerGameType.Duel.ToString() }).SetMinimumAndMaximumFrom(MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent)
			.BuildOptionType(MultiplayerOptions.OptionType.FriendlyFireDamageMeleeFriendPercent)
			.BuildName(new TextObject("{=*}Friendly Melee Damage"))
			.BuildDescription(new TextObject("{=*}Change the value of friendly melee damage.")));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("friendly_fire_melee_self_percent").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[1] { MultiplayerGameType.Duel.ToString() }).SetMinimumAndMaximumFrom(MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent)
			.BuildOptionType(MultiplayerOptions.OptionType.FriendlyFireDamageMeleeSelfPercent)
			.BuildName(new TextObject("{=*}Friendly Reflective Melee Damage"))
			.BuildDescription(new TextObject("{=*}Change the value of reflective friendly melee damage.")));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("friendly_fire_ranged_percent").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[1] { MultiplayerGameType.Duel.ToString() }).SetMinimumAndMaximumFrom(MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent)
			.BuildOptionType(MultiplayerOptions.OptionType.FriendlyFireDamageRangedFriendPercent)
			.BuildName(new TextObject("{=*}Friendly Ranged Damage"))
			.BuildDescription(new TextObject("{=*}Change the value of friendly ranged damage.")));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("friendly_fire_ranged_self_percent").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[1] { MultiplayerGameType.Duel.ToString() }).SetMinimumAndMaximumFrom(MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent)
			.BuildOptionType(MultiplayerOptions.OptionType.FriendlyFireDamageRangedSelfPercent)
			.BuildName(new TextObject("{=*}Friendly Reflective Ranged Damage"))
			.BuildDescription(new TextObject("{=*}Change the value of reflective friendly ranged damage.")));
		adminPanelOptionGroup.AddOption(new AdminPanelOption<bool>("allow_infantry").BuildName(new TextObject("{=*}Allow Infantry")).BuildDescription(new TextObject("{=*}Allow usage of infantry troops in game.")).BuildDefaultValue(_missionLobbyComponent.IsClassAvailable(FormationClass.Infantry))
			.BuildInitialValue(_missionLobbyComponent.IsClassAvailable(FormationClass.Infantry))
			.BuildOnAppliedCallback(delegate(bool val)
			{
				_multiplayerAdminComponent.ChangeClassRestriction(FormationClass.Infantry, !val);
			}));
		adminPanelOptionGroup.AddOption(new AdminPanelOption<bool>("allow_ranged").BuildName(new TextObject("{=*}Allow Archers")).BuildDescription(new TextObject("{=*}Allow usage of archer troops in game.")).BuildDefaultValue(_missionLobbyComponent.IsClassAvailable(FormationClass.Ranged))
			.BuildInitialValue(_missionLobbyComponent.IsClassAvailable(FormationClass.Ranged))
			.BuildOnAppliedCallback(delegate(bool val)
			{
				_multiplayerAdminComponent.ChangeClassRestriction(FormationClass.Ranged, !val);
			}));
		adminPanelOptionGroup.AddOption(new AdminPanelOption<bool>("allow_cavalry").BuildName(new TextObject("{=*}Allow Cavalry")).BuildDescription(new TextObject("{=*}Allow usage of cavalry troops in game.")).BuildDefaultValue(_missionLobbyComponent.IsClassAvailable(FormationClass.Cavalry))
			.BuildInitialValue(_missionLobbyComponent.IsClassAvailable(FormationClass.Cavalry))
			.BuildOnAppliedCallback(delegate(bool val)
			{
				_multiplayerAdminComponent.ChangeClassRestriction(FormationClass.Cavalry, !val);
			}));
		adminPanelOptionGroup.AddOption(new AdminPanelOption<bool>("allow_horse_archers").BuildName(new TextObject("{=*}Allow Horse Archers")).BuildDescription(new TextObject("{=*}Allow usage of horse archer troops in game.")).BuildDefaultValue(_missionLobbyComponent.IsClassAvailable(FormationClass.HorseArcher))
			.BuildInitialValue(_missionLobbyComponent.IsClassAvailable(FormationClass.HorseArcher))
			.BuildOnAppliedCallback(delegate(bool val)
			{
				_multiplayerAdminComponent.ChangeClassRestriction(FormationClass.HorseArcher, !val);
			}));
		return adminPanelOptionGroup;
	}

	private AdminPanelOptionGroup GetActions()
	{
		AdminPanelOptionGroup adminPanelOptionGroup = new AdminPanelOptionGroup("actions", new TextObject("{=*}Actions"));
		adminPanelOptionGroup.AddAction(new AdminPanelGameTypeDependentAction("end_warmup").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[2]
		{
			MultiplayerGameType.TeamDeathmatch.ToString(),
			MultiplayerGameType.Duel.ToString()
		}).BuildName(new TextObject("{=*}End Warmup"))
			.BuildDescription(new TextObject("{=*}Set warmup timer to maximum of 30 seconds."))
			.BuildOnActionExecutedCallback(delegate
			{
				_multiplayerAdminComponent.EndWarmup();
			}));
		adminPanelOptionGroup.AddAction(new AdminPanelAction("mute_player").BuildName(new TextObject("{=*}Mute Players")).BuildDescription(new TextObject("{=*}Select players to mute.")).BuildOnActionExecutedCallback(delegate
		{
			List<InquiryElement> list4 = new List<InquiryElement>();
			foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
			{
				if (!CustomGameMutedPlayerManager.IsUserMuted(networkPeer.VirtualPlayer.Id))
				{
					list4.Add(new InquiryElement(networkPeer, networkPeer.UserName, null));
				}
			}
			MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=*}Mute Players").ToString(), new TextObject("{=*}Select players to mute.").ToString(), list4, isExitShown: true, 0, 1, new TextObject("{=*}Mute").ToString(), new TextObject("{=*}Cancel").ToString(), delegate(List<InquiryElement> selectedPlayers)
			{
				if (selectedPlayers != null && selectedPlayers.Count == 1)
				{
					NetworkCommunicator networkCommunicator4 = (NetworkCommunicator)selectedPlayers[0].Identifier;
					if (networkCommunicator4 != null)
					{
						_multiplayerAdminComponent.GlobalMuteUnmutePlayer(networkCommunicator4, unmute: false);
					}
				}
			}, null, string.Empty, isSeachAvailable: true));
		}));
		adminPanelOptionGroup.AddAction(new AdminPanelAction("mute_player").BuildName(new TextObject("{=*}Unmute Players")).BuildDescription(new TextObject("{=*}Select players to unmute.")).BuildOnActionExecutedCallback(delegate
		{
			List<InquiryElement> list3 = new List<InquiryElement>();
			foreach (NetworkCommunicator networkPeer2 in GameNetwork.NetworkPeers)
			{
				if (CustomGameMutedPlayerManager.IsUserMuted(networkPeer2.VirtualPlayer.Id))
				{
					list3.Add(new InquiryElement(networkPeer2, networkPeer2.UserName, null));
				}
			}
			MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=*}Unmute Players").ToString(), new TextObject("{=*}Select players to unmute.").ToString(), list3, isExitShown: true, 0, 1, new TextObject("{=*}Unmute").ToString(), new TextObject("{=*}Cancel").ToString(), delegate(List<InquiryElement> selectedPlayers)
			{
				if (selectedPlayers != null && selectedPlayers.Count == 1)
				{
					NetworkCommunicator networkCommunicator3 = (NetworkCommunicator)selectedPlayers[0].Identifier;
					if (networkCommunicator3 != null)
					{
						_multiplayerAdminComponent.GlobalMuteUnmutePlayer(networkCommunicator3, unmute: true);
					}
				}
			}, null, string.Empty, isSeachAvailable: true));
		}));
		adminPanelOptionGroup.AddAction(new AdminPanelAction("kick_player").BuildName(new TextObject("{=*}Kick Player")).BuildDescription(new TextObject("{=*}Select a player to kick.")).BuildOnActionExecutedCallback(delegate
		{
			List<InquiryElement> list2 = new List<InquiryElement>();
			foreach (NetworkCommunicator networkPeer3 in GameNetwork.NetworkPeers)
			{
				list2.Add(new InquiryElement(networkPeer3, networkPeer3.UserName, null));
			}
			MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=*}Kick Player").ToString(), new TextObject("{=*}Select player to kick").ToString(), list2, isExitShown: true, 0, 1, new TextObject("{=*}Kick").ToString(), new TextObject("{=*}Cancel").ToString(), delegate(List<InquiryElement> selectedPlayers)
			{
				if (selectedPlayers != null && selectedPlayers.Count == 1)
				{
					NetworkCommunicator networkCommunicator2 = (NetworkCommunicator)selectedPlayers[0].Identifier;
					if (networkCommunicator2 != null)
					{
						_multiplayerAdminComponent.KickPlayer(networkCommunicator2, banPlayer: false);
					}
				}
			}, null, string.Empty, isSeachAvailable: true));
		}));
		adminPanelOptionGroup.AddAction(new AdminPanelAction("ban_player").BuildName(new TextObject("{=*}Ban Player")).BuildDescription(new TextObject("{=*}Select a player to ban.")).BuildOnActionExecutedCallback(delegate
		{
			List<InquiryElement> list = new List<InquiryElement>();
			foreach (NetworkCommunicator networkPeer4 in GameNetwork.NetworkPeers)
			{
				list.Add(new InquiryElement(networkPeer4, networkPeer4.UserName, null));
			}
			MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=*}Ban Player").ToString(), new TextObject("{=*}Select player to ban").ToString(), list, isExitShown: true, 0, 1, new TextObject("{=*}Ban").ToString(), new TextObject("{=*}Cancel").ToString(), delegate(List<InquiryElement> selectedPlayers)
			{
				if (selectedPlayers != null && selectedPlayers.Count == 1)
				{
					NetworkCommunicator networkCommunicator = (NetworkCommunicator)selectedPlayers[0].Identifier;
					if (networkCommunicator != null)
					{
						_multiplayerAdminComponent.KickPlayer(networkCommunicator, banPlayer: true);
					}
				}
			}, null, string.Empty, isSeachAvailable: true));
		}));
		return adminPanelOptionGroup;
	}
}
