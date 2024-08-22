using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;

namespace TaleWorlds.Core.ViewModelCollection.Information;

public class GameNotificationVM : ViewModel
{
	private readonly List<GameNotificationItemVM> _items;

	private bool _gotNotification;

	private const float MinimumDisplayTimeInSeconds = 1f;

	private const float ExtraDisplayTimeInSeconds = 1f;

	private float _timer;

	private int _notificationId;

	private GameNotificationItemVM _currentNotification;

	private float _totalTime;

	private float CurrentNotificationOnScreenTime
	{
		get
		{
			float num = 1f;
			num += (float)CurrentNotification.ExtraTimeInMs / 1000f;
			int numberOfWords = GetNumberOfWords(CurrentNotification.GameNotificationText);
			if (numberOfWords > 4)
			{
				num += (float)(numberOfWords - 4) / 5f;
			}
			return num + 1f / (float)(_items.Count + 1);
		}
	}

	public GameNotificationItemVM CurrentNotification
	{
		get
		{
			return _currentNotification;
		}
		set
		{
			if (_currentNotification != value)
			{
				_currentNotification = value;
				NotificationId++;
				OnPropertyChangedWithValue(value, "CurrentNotification");
				if (value != null)
				{
					this.ReceiveNewNotification?.Invoke(CurrentNotification);
				}
			}
		}
	}

	[DataSourceProperty]
	public bool GotNotification
	{
		get
		{
			return _gotNotification;
		}
		set
		{
			if (value != _gotNotification)
			{
				_gotNotification = value;
				OnPropertyChangedWithValue(value, "GotNotification");
			}
		}
	}

	[DataSourceProperty]
	public int NotificationId
	{
		get
		{
			return _notificationId;
		}
		set
		{
			if (value != _notificationId)
			{
				_notificationId = value;
				OnPropertyChangedWithValue(value, "NotificationId");
			}
		}
	}

	[DataSourceProperty]
	public float TotalTime
	{
		get
		{
			return _totalTime;
		}
		set
		{
			if (value != _totalTime)
			{
				_totalTime = value;
				OnPropertyChangedWithValue(value, "TotalTime");
			}
		}
	}

	public event Action<GameNotificationItemVM> ReceiveNewNotification;

	public GameNotificationVM()
	{
		MBInformationManager.FiringQuickInformation += AddGameNotification;
		_items = new List<GameNotificationItemVM>();
		CurrentNotification = new GameNotificationItemVM("NULL", 0, null, "NULL");
		GotNotification = false;
	}

	public void ClearNotifications()
	{
		_items.Clear();
		GotNotification = false;
		_timer = CurrentNotificationOnScreenTime * 2f;
	}

	public void Tick(float dt)
	{
		_timer += dt;
		if (GotNotification && _timer >= CurrentNotificationOnScreenTime)
		{
			_timer = 0f;
			if (_items.Count > 0)
			{
				CurrentNotification = _items[0];
				_items.RemoveAt(0);
			}
			else
			{
				GotNotification = false;
			}
		}
	}

	public void AddGameNotification(string notificationText, int extraTimeInMs, BasicCharacterObject announcerCharacter, string soundId)
	{
		GameNotificationItemVM gameNotificationItemVM = new GameNotificationItemVM(notificationText, extraTimeInMs, announcerCharacter, soundId);
		if (!_items.Any((GameNotificationItemVM i) => i.GameNotificationText == notificationText) && (!GotNotification || CurrentNotification.GameNotificationText != notificationText))
		{
			if (GotNotification)
			{
				_items.Add(gameNotificationItemVM);
				return;
			}
			CurrentNotification = gameNotificationItemVM;
			TotalTime = CurrentNotificationOnScreenTime;
			GotNotification = true;
			_timer = 0f;
		}
	}

	private int GetNumberOfWords(string text)
	{
		string text2 = text.Trim();
		int num = 0;
		int i = 0;
		while (i < text2.Length)
		{
			for (; i < text2.Length && !char.IsWhiteSpace(text2[i]); i++)
			{
			}
			num++;
			for (; i < text2.Length && char.IsWhiteSpace(text2[i]); i++)
			{
			}
		}
		return num;
	}
}
