using TaleWorlds.Library;

namespace TaleWorlds.Core.ViewModelCollection.Information;

public class GameNotificationItemVM : ViewModel
{
	private string _gameNotificationText;

	private string _characterNameText;

	private string _notificationSoundId;

	private ImageIdentifierVM _announcer;

	private int _extraTimeInMs;

	[DataSourceProperty]
	public ImageIdentifierVM Announcer
	{
		get
		{
			return _announcer;
		}
		set
		{
			if (value != _announcer)
			{
				_announcer = value;
				OnPropertyChangedWithValue(value, "Announcer");
			}
		}
	}

	[DataSourceProperty]
	public int ExtraTimeInMs
	{
		get
		{
			return _extraTimeInMs;
		}
		set
		{
			if (value != _extraTimeInMs)
			{
				_extraTimeInMs = value;
				OnPropertyChangedWithValue(value, "ExtraTimeInMs");
			}
		}
	}

	[DataSourceProperty]
	public string GameNotificationText
	{
		get
		{
			return _gameNotificationText;
		}
		set
		{
			if (value != _gameNotificationText)
			{
				_gameNotificationText = value;
				OnPropertyChangedWithValue(value, "GameNotificationText");
			}
		}
	}

	[DataSourceProperty]
	public string CharacterNameText
	{
		get
		{
			return _characterNameText;
		}
		set
		{
			if (value != _characterNameText)
			{
				_characterNameText = value;
				OnPropertyChangedWithValue(value, "CharacterNameText");
			}
		}
	}

	[DataSourceProperty]
	public string NotificationSoundId
	{
		get
		{
			return _notificationSoundId;
		}
		set
		{
			if (value != _notificationSoundId)
			{
				_notificationSoundId = value;
				OnPropertyChangedWithValue(value, "NotificationSoundId");
			}
		}
	}

	public GameNotificationItemVM(string notificationText, int extraTimeInMs, BasicCharacterObject announcerCharacter, string soundId)
	{
		GameNotificationText = notificationText;
		NotificationSoundId = soundId;
		Announcer = ((announcerCharacter != null) ? new ImageIdentifierVM(CharacterCode.CreateFrom(announcerCharacter)) : new ImageIdentifierVM());
		CharacterNameText = ((announcerCharacter != null) ? announcerCharacter.Name.ToString() : "");
		ExtraTimeInMs = extraTimeInMs;
	}
}
