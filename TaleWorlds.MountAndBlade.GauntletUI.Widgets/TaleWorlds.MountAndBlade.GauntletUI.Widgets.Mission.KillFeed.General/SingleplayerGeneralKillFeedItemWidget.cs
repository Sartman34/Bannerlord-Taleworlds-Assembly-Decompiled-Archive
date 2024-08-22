using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Mission.KillFeed.General;

public class SingleplayerGeneralKillFeedItemWidget : Widget
{
	private const char _seperator = '\0';

	private string _murdererType;

	private string _victimType;

	private string _murdererName;

	private string _victimName;

	private bool _isUnconscious;

	private bool _isHeadshot;

	private Color _color;

	private float _speedModifier = 1f;

	private bool _initialized;

	private string _message;

	public Widget MurdererTypeWidget { get; set; }

	public Widget VictimTypeWidget { get; set; }

	public Widget ActionIconWidget { get; set; }

	public Widget BackgroundWidget { get; set; }

	public AutoHideTextWidget VictimNameWidget { get; set; }

	public AutoHideTextWidget MurdererNameWidget { get; set; }

	public float FadeInTime { get; set; } = 0.7f;


	public float StayTime { get; set; } = 3f;


	public float FadeOutTime { get; set; } = 0.7f;


	private float CurrentAlpha => base.AlphaFactor;

	public float TimeSinceCreation { get; private set; }

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
				HandleMessage(value);
			}
		}
	}

	public SingleplayerGeneralKillFeedItemWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (!_initialized)
		{
			MurdererTypeWidget.Sprite = MurdererTypeWidget.Context.SpriteData.GetSprite("General\\compass\\" + _murdererType);
			VictimTypeWidget.Sprite = MurdererTypeWidget.Context.SpriteData.GetSprite("General\\compass\\" + _victimType);
			ActionIconWidget.Sprite = ActionIconWidget.Context.SpriteData.GetSprite("General\\Mission\\PersonalKillfeed\\" + (_isHeadshot ? "headshot_kill_icon" : "kill_feed_skull"));
			this.SetGlobalAlphaRecursively(0f);
			ActionIconWidget.Color = (_isUnconscious ? new Color(1f, 1f, 1f) : new Color(1f, 0f, 0f));
			BackgroundWidget.Color = _color;
			VictimNameWidget.Text = _victimName;
			MurdererNameWidget.Text = _murdererName;
			if (_victimName.Length == 0)
			{
				ActionIconWidget.MarginRight = 0f;
				VictimTypeWidget.MarginLeft = 5f;
			}
			if (_murdererName.Length == 0)
			{
				ActionIconWidget.MarginLeft = 0f;
				MurdererTypeWidget.MarginRight = 5f;
			}
			_initialized = true;
		}
		TimeSinceCreation += dt * _speedModifier;
		if (TimeSinceCreation <= FadeInTime)
		{
			this.SetGlobalAlphaRecursively(Mathf.Lerp(CurrentAlpha, 0.5f, TimeSinceCreation / FadeInTime));
		}
		else if (TimeSinceCreation - FadeInTime <= StayTime)
		{
			this.SetGlobalAlphaRecursively(0.5f);
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

	private void HandleMessage(string value)
	{
		string[] array = value.Split(new char[1]);
		_murdererName = array[0];
		_murdererType = array[1];
		_victimName = array[2];
		_victimType = array[3];
		_isUnconscious = array[4].Equals("1");
		_isHeadshot = array[5].Equals("1");
		_color = Color.FromUint(uint.Parse(array[6]));
	}
}
