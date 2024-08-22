using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.InitialMenu;

public class InitialMenuVM : ViewModel
{
	private MBBindingList<InitialMenuOptionVM> _menuOptions;

	private bool _isProfileSelectionEnabled;

	private bool _isDownloadingContent;

	private string _selectProfileText;

	private string _profileName;

	private string _downloadingText;

	private string _gameVersionText;

	[DataSourceProperty]
	public MBBindingList<InitialMenuOptionVM> MenuOptions
	{
		get
		{
			return _menuOptions;
		}
		set
		{
			if (value != _menuOptions)
			{
				_menuOptions = value;
				OnPropertyChangedWithValue(value, "MenuOptions");
			}
		}
	}

	[DataSourceProperty]
	public string DownloadingText
	{
		get
		{
			return _downloadingText;
		}
		set
		{
			if (value != _downloadingText)
			{
				_downloadingText = value;
				OnPropertyChangedWithValue(value, "DownloadingText");
			}
		}
	}

	[DataSourceProperty]
	public string SelectProfileText
	{
		get
		{
			return _selectProfileText;
		}
		set
		{
			if (value != _selectProfileText)
			{
				_selectProfileText = value;
				OnPropertyChangedWithValue(value, "SelectProfileText");
			}
		}
	}

	[DataSourceProperty]
	public string ProfileName
	{
		get
		{
			return _profileName;
		}
		set
		{
			if (value != _profileName)
			{
				_profileName = value;
				OnPropertyChangedWithValue(value, "ProfileName");
			}
		}
	}

	[DataSourceProperty]
	public string GameVersionText
	{
		get
		{
			return _gameVersionText;
		}
		set
		{
			if (value != _gameVersionText)
			{
				_gameVersionText = value;
				OnPropertyChangedWithValue(value, "GameVersionText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsProfileSelectionEnabled
	{
		get
		{
			return _isProfileSelectionEnabled;
		}
		set
		{
			if (value != _isProfileSelectionEnabled)
			{
				_isProfileSelectionEnabled = value;
				OnPropertyChangedWithValue(value, "IsProfileSelectionEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDownloadingContent
	{
		get
		{
			return _isDownloadingContent;
		}
		set
		{
			if (value != _isDownloadingContent)
			{
				_isDownloadingContent = value;
				OnPropertyChangedWithValue(value, "IsDownloadingContent");
			}
		}
	}

	public InitialMenuVM(InitialState initialState)
	{
		SelectProfileText = new TextObject("{=wubDWOlh}Select Profile").ToString();
		DownloadingText = new TextObject("{=i4Oo6aoM}Downloading Content...").ToString();
		if (HotKeyManager.ShouldNotifyDocumentVersionDifferent())
		{
			MBInformationManager.AddQuickInformation(new TextObject("{=0Itt3bZM}Current keybind document version is outdated. Keybinds have been reverted to defaults."));
		}
		GameVersionText = Utilities.GetApplicationVersionWithBuildNumber().ToString();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		MenuOptions.ApplyActionOnAllItems(delegate(InitialMenuOptionVM o)
		{
			o.RefreshValues();
		});
	}

	public void RefreshMenuOptions()
	{
		MenuOptions = new MBBindingList<InitialMenuOptionVM>();
		_ = GameStateManager.Current.ActiveState;
		foreach (InitialStateOption initialStateOption in Module.CurrentModule.GetInitialStateOptions())
		{
			MenuOptions.Add(new InitialMenuOptionVM(initialStateOption));
		}
		IsDownloadingContent = Utilities.IsOnlyCoreContentEnabled();
	}

	public override void OnFinalize()
	{
		base.OnFinalize();
	}
}
