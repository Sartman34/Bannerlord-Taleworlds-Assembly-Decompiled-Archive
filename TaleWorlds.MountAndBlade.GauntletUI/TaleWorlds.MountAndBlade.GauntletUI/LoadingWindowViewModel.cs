using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class LoadingWindowViewModel : ViewModel
{
	private int _currentImage;

	private int _totalGenericImageCount;

	private Action<bool, int> _handleSPPartialLoading;

	private bool _enabled;

	private bool _isDevelopmentMode;

	private bool _isMultiplayer;

	private string _loadingImageName;

	private string _titleText;

	private string _descriptionText;

	private string _gameModeText;

	public bool CurrentlyShowingMultiplayer { get; private set; }

	[DataSourceProperty]
	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			if (_enabled != value)
			{
				_enabled = value;
				OnPropertyChangedWithValue(value, "Enabled");
				if (value)
				{
					HandleEnable();
				}
			}
		}
	}

	[DataSourceProperty]
	public bool IsDevelopmentMode
	{
		get
		{
			return _isDevelopmentMode;
		}
		set
		{
			if (_isDevelopmentMode != value)
			{
				_isDevelopmentMode = value;
				OnPropertyChangedWithValue(value, "IsDevelopmentMode");
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
			if (_titleText != value)
			{
				_titleText = value;
				OnPropertyChangedWithValue(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string GameModeText
	{
		get
		{
			return _gameModeText;
		}
		set
		{
			if (_gameModeText != value)
			{
				_gameModeText = value;
				OnPropertyChangedWithValue(value, "GameModeText");
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
			if (_descriptionText != value)
			{
				_descriptionText = value;
				OnPropertyChangedWithValue(value, "DescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMultiplayer
	{
		get
		{
			return _isMultiplayer;
		}
		set
		{
			if (_isMultiplayer != value)
			{
				_isMultiplayer = value;
				OnPropertyChangedWithValue(value, "IsMultiplayer");
			}
		}
	}

	[DataSourceProperty]
	public string LoadingImageName
	{
		get
		{
			return _loadingImageName;
		}
		set
		{
			if (_loadingImageName != value)
			{
				_loadingImageName = value;
				OnPropertyChangedWithValue(value, "LoadingImageName");
			}
		}
	}

	public LoadingWindowViewModel(Action<bool, int> handleSPPartialLoading)
	{
		_handleSPPartialLoading = handleSPPartialLoading;
		_handleSPPartialLoading?.Invoke(arg1: true, _currentImage + 1);
	}

	internal void Update()
	{
		if (Enabled)
		{
			bool flag = IsEligableForMultiplayerLoading();
			if (flag && !CurrentlyShowingMultiplayer)
			{
				SetForMultiplayer();
			}
			else if (!flag && CurrentlyShowingMultiplayer)
			{
				SetForEmpty();
			}
		}
	}

	private void HandleEnable()
	{
		if (IsEligableForMultiplayerLoading())
		{
			SetForMultiplayer();
		}
		else
		{
			SetForEmpty();
		}
	}

	private bool IsEligableForMultiplayerLoading()
	{
		if (_isMultiplayer && TaleWorlds.MountAndBlade.Mission.Current != null)
		{
			return Game.Current.GameStateManager.ActiveState is MissionState;
		}
		return false;
	}

	private void SetForMultiplayer()
	{
		MissionState missionState = (MissionState)Game.Current.GameStateManager.ActiveState;
		string text = missionState.MissionName switch
		{
			"MultiplayerTeamDeathmatch" => "TeamDeathmatch", 
			"MultiplayerSiege" => "Siege", 
			"MultiplayerBattle" => "Battle", 
			"MultiplayerCaptain" => "Captain", 
			"MultiplayerSkirmish" => "Skirmish", 
			"MultiplayerDuel" => "Duel", 
			_ => missionState.MissionName, 
		};
		if (!string.IsNullOrEmpty(text))
		{
			DescriptionText = GameTexts.FindText("str_multiplayer_official_game_type_explainer", text).ToString();
		}
		else
		{
			DescriptionText = "";
		}
		GameModeText = GameTexts.FindText("str_multiplayer_official_game_type_name", text).ToString();
		if (GameTexts.TryGetText("str_multiplayer_scene_name", out var textObject, missionState.CurrentMission.SceneName))
		{
			TitleText = textObject.ToString();
		}
		else
		{
			TitleText = missionState.CurrentMission.SceneName;
		}
		LoadingImageName = missionState.CurrentMission.SceneName;
		CurrentlyShowingMultiplayer = true;
	}

	private void SetForEmpty()
	{
		DescriptionText = "";
		TitleText = "";
		GameModeText = "";
		SetNextGenericImage();
		CurrentlyShowingMultiplayer = false;
	}

	private void SetNextGenericImage()
	{
		int arg = ((_currentImage >= 1) ? _currentImage : _totalGenericImageCount);
		_currentImage = ((_currentImage >= _totalGenericImageCount) ? 1 : (_currentImage + 1));
		int arg2 = ((_currentImage >= _totalGenericImageCount) ? 1 : (_currentImage + 1));
		_handleSPPartialLoading?.Invoke(arg1: false, arg);
		_handleSPPartialLoading?.Invoke(arg1: true, arg2);
		IsDevelopmentMode = NativeConfig.IsDevelopmentMode;
		LoadingImageName = "loading_" + _currentImage.ToString("00");
	}

	public void SetTotalGenericImageCount(int totalGenericImageCount)
	{
		_totalGenericImageCount = totalGenericImageCount;
	}
}
