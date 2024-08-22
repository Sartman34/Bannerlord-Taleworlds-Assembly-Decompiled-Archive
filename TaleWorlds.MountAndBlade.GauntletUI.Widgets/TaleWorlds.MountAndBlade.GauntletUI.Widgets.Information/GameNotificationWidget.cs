using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Information;

public class GameNotificationWidget : BrushWidget
{
	private bool _textWidgetAlignmentDirty = true;

	private float _totalDt;

	private int _notificationId;

	private RichTextWidget _textWidget;

	private ImageIdentifierWidget _announcerImageIdentifier;

	private float _totalTime;

	public float RampUpInSeconds { get; set; }

	public float RampDownInSeconds { get; set; }

	public ImageIdentifierWidget AnnouncerImageIdentifier
	{
		get
		{
			return _announcerImageIdentifier;
		}
		set
		{
			if (_announcerImageIdentifier != value)
			{
				_announcerImageIdentifier = value;
				OnPropertyChanged(value, "AnnouncerImageIdentifier");
			}
		}
	}

	public int NotificationId
	{
		get
		{
			return _notificationId;
		}
		set
		{
			if (_notificationId != value)
			{
				_notificationId = value;
				OnPropertyChanged(value, "NotificationId");
				_textWidgetAlignmentDirty = true;
				_totalDt = 0f;
			}
		}
	}

	public float TotalTime
	{
		get
		{
			return _totalTime;
		}
		set
		{
			if (_totalTime != value)
			{
				_totalTime = value;
			}
		}
	}

	public RichTextWidget TextWidget
	{
		get
		{
			return _textWidget;
		}
		set
		{
			if (_textWidget != value)
			{
				_textWidget = value;
				OnPropertyChanged(value, "TextWidget");
			}
		}
	}

	public GameNotificationWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (_textWidgetAlignmentDirty)
		{
			if (AnnouncerImageIdentifier.ImageTypeCode != 0)
			{
				TextWidget.Brush.TextHorizontalAlignment = TextHorizontalAlignment.Left;
			}
			else
			{
				TextWidget.Brush.TextHorizontalAlignment = TextHorizontalAlignment.Center;
			}
		}
		if (TotalTime > 0f && _totalDt <= TotalTime)
		{
			if (_totalDt <= RampUpInSeconds)
			{
				float alphaFactor = Mathf.Lerp(0f, 1f, _totalDt / RampUpInSeconds);
				this.SetGlobalAlphaRecursively(alphaFactor);
			}
			else if (_totalDt > RampUpInSeconds && _totalDt < TotalTime - RampDownInSeconds)
			{
				this.SetGlobalAlphaRecursively(1f);
			}
			else if (TotalTime - _totalDt < RampDownInSeconds)
			{
				float alphaFactor2 = Mathf.Lerp(1f, 0f, 1f - (TotalTime - _totalDt) / RampUpInSeconds);
				this.SetGlobalAlphaRecursively(alphaFactor2);
			}
			else
			{
				this.SetGlobalAlphaRecursively(0f);
			}
			_totalDt += dt;
		}
	}
}
