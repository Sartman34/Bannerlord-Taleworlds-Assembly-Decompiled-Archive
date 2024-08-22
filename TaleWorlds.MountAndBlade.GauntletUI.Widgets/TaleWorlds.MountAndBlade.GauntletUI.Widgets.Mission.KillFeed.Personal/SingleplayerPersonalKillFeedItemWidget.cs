using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Mission.KillFeed.Personal;

public class SingleplayerPersonalKillFeedItemWidget : Widget
{
	private bool _initialized;

	private float _speedModifier;

	private int _itemType;

	private int _amount;

	private string _typeID;

	private string _message;

	private Widget _troopTypeWidget;

	private bool _isDamage;

	public Widget NotificationTypeIconWidget { get; set; }

	public Widget NotificationBackgroundWidget { get; set; }

	public TextWidget AmountTextWidget { get; set; }

	public RichTextWidget MessageTextWidget { get; set; }

	public float FadeInTime { get; set; } = 0.2f;


	public float StayTime { get; set; } = 2f;


	public float FadeOutTime { get; set; } = 0.2f;


	private float CurrentAlpha => base.AlphaFactor;

	public float TimeSinceCreation { get; private set; }

	public bool IsDamage
	{
		get
		{
			return _isDamage;
		}
		set
		{
			if (value != _isDamage)
			{
				_isDamage = value;
				OnPropertyChanged(value, "IsDamage");
			}
		}
	}

	public int Amount
	{
		get
		{
			return _amount;
		}
		set
		{
			if (value != _amount)
			{
				_amount = value;
				OnPropertyChanged(value, "Amount");
			}
		}
	}

	public int ItemType
	{
		get
		{
			return _itemType;
		}
		set
		{
			if (value != _itemType)
			{
				_itemType = value;
				OnPropertyChanged(value, "ItemType");
			}
		}
	}

	public string Message
	{
		get
		{
			return _message;
		}
		set
		{
			if (value != _message)
			{
				_message = value;
				OnPropertyChanged(value, "Message");
			}
		}
	}

	public string TypeID
	{
		get
		{
			return _typeID;
		}
		set
		{
			if (value != _typeID)
			{
				_typeID = value;
				OnPropertyChanged(value, "TypeID");
			}
		}
	}

	public Widget TroopTypeWidget
	{
		get
		{
			return _troopTypeWidget;
		}
		set
		{
			if (value != _troopTypeWidget)
			{
				_troopTypeWidget = value;
				if (!string.IsNullOrEmpty(_typeID))
				{
					_troopTypeWidget.Sprite = _troopTypeWidget.Context.SpriteData.GetSprite("General\\compass\\" + _typeID);
				}
			}
		}
	}

	public SingleplayerPersonalKillFeedItemWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (!_initialized)
		{
			this.SetGlobalAlphaRecursively(0f);
			UpdateNotificationBackgroundWidget();
			UpdateNotificationTypeIconWidget();
			UpdateNotificationMessageWidget();
			UpdateNotificationAmountWidget();
			UpdateTroopTypeVisualWidget();
			_initialized = true;
		}
		UpdateAlphaValues(dt);
	}

	private void UpdateAlphaValues(float dt)
	{
		TimeSinceCreation += dt;
		if (TimeSinceCreation <= FadeInTime)
		{
			this.SetGlobalAlphaRecursively(Mathf.Lerp(CurrentAlpha, 1f, TimeSinceCreation / FadeInTime));
		}
		else if (TimeSinceCreation - FadeInTime <= StayTime)
		{
			this.SetGlobalAlphaRecursively(1f);
		}
		else if (TimeSinceCreation - (FadeInTime + StayTime) <= FadeOutTime)
		{
			this.SetGlobalAlphaRecursively(Mathf.Lerp(CurrentAlpha, 0f, (TimeSinceCreation - (FadeInTime + StayTime)) / FadeOutTime));
			if (CurrentAlpha <= 0.1f)
			{
				EventFired("OnRemove");
			}
		}
		else
		{
			EventFired("OnRemove");
		}
	}

	public void SetSpeedModifier(float newSpeed)
	{
		if (newSpeed > _speedModifier)
		{
			_speedModifier = newSpeed;
		}
	}

	private void UpdateNotificationTypeIconWidget()
	{
		if (ItemType == 0)
		{
			NotificationTypeIconWidget.IsVisible = false;
			return;
		}
		switch (ItemType)
		{
		case 1:
			NotificationTypeIconWidget.SetState("FriendlyFireDamage");
			break;
		case 2:
			NotificationTypeIconWidget.SetState("FriendlyFireKill");
			break;
		case 3:
			NotificationTypeIconWidget.SetState("MountDamage");
			break;
		case 4:
			NotificationTypeIconWidget.SetState("NormalKill");
			break;
		case 5:
			NotificationTypeIconWidget.SetState("Assist");
			break;
		case 6:
			NotificationTypeIconWidget.SetState("MakeUnconscious");
			break;
		case 7:
			NotificationTypeIconWidget.SetState("NormalKillHeadshot");
			break;
		case 8:
			NotificationTypeIconWidget.SetState("MakeUnconsciousHeadshot");
			break;
		default:
			Debug.FailedAssert("Undefined personal feed notification type", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Mission\\KillFeed\\Personal\\SingleplayerPersonalKillFeedItemWidget.cs", "UpdateNotificationTypeIconWidget", 126);
			NotificationTypeIconWidget.IsVisible = false;
			break;
		}
	}

	private void UpdateNotificationAmountWidget()
	{
		if (ItemType != 6 && Amount == -1)
		{
			AmountTextWidget.IsVisible = false;
			return;
		}
		switch (ItemType)
		{
		case 1:
		case 2:
			AmountTextWidget.SetState("FriendlyFire");
			AmountTextWidget.IntText = Amount;
			break;
		case 0:
		case 3:
		case 4:
		case 6:
		case 7:
		case 8:
			AmountTextWidget.SetState("Normal");
			AmountTextWidget.IntText = Amount;
			break;
		case 5:
			AmountTextWidget.IsVisible = false;
			break;
		default:
			Debug.FailedAssert("Undefined personal feed notification type", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Mission\\KillFeed\\Personal\\SingleplayerPersonalKillFeedItemWidget.cs", "UpdateNotificationAmountWidget", 163);
			AmountTextWidget.IsVisible = false;
			break;
		}
	}

	private void UpdateNotificationMessageWidget()
	{
		MessageTextWidget.Text = Message;
		if (string.IsNullOrEmpty(Message))
		{
			MessageTextWidget.IsVisible = false;
			return;
		}
		switch (ItemType)
		{
		case 1:
		case 2:
			MessageTextWidget.SetState("FriendlyFire");
			break;
		case 0:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
			MessageTextWidget.SetState("Normal");
			break;
		default:
			Debug.FailedAssert("Undefined personal feed notification type", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Mission\\KillFeed\\Personal\\SingleplayerPersonalKillFeedItemWidget.cs", "UpdateNotificationMessageWidget", 196);
			MessageTextWidget.IsVisible = false;
			break;
		}
	}

	private void UpdateNotificationBackgroundWidget()
	{
		switch (ItemType)
		{
		case 0:
		case 1:
		case 3:
			NotificationBackgroundWidget.SetState("Hidden");
			break;
		case 2:
			NotificationBackgroundWidget.SetState("FriendlyFire");
			break;
		case 4:
		case 6:
		case 7:
		case 8:
			NotificationBackgroundWidget.SetState("Normal");
			break;
		default:
			Debug.FailedAssert("Undefined personal feed notification type", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Mission\\KillFeed\\Personal\\SingleplayerPersonalKillFeedItemWidget.cs", "UpdateNotificationBackgroundWidget", 224);
			NotificationBackgroundWidget.SetState("Hidden");
			break;
		case 5:
			break;
		}
	}

	private void UpdateTroopTypeVisualWidget()
	{
		if (_troopTypeWidget != null)
		{
			if (string.IsNullOrEmpty(TypeID))
			{
				_troopTypeWidget.IsVisible = false;
			}
			else
			{
				_troopTypeWidget.Sprite = _troopTypeWidget.Context.SpriteData.GetSprite("General\\compass\\" + _typeID);
			}
		}
	}
}
