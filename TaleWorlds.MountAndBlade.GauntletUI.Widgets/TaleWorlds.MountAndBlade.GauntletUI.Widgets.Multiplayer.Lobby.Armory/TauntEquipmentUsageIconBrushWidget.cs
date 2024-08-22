using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Multiplayer.Lobby.Armory;

public class TauntEquipmentUsageIconBrushWidget : BrushWidget
{
	private Brush _iconsBrush;

	private string _usageName;

	public Brush IconsBrush
	{
		get
		{
			return _iconsBrush;
		}
		set
		{
			if (value != _iconsBrush)
			{
				_iconsBrush = value;
				OnPropertyChanged(value, "IconsBrush");
				OnUsageNameUpdated(UsageName);
			}
		}
	}

	public string UsageName
	{
		get
		{
			return _usageName;
		}
		set
		{
			if (value != _usageName)
			{
				_usageName = value;
				OnPropertyChanged(value, "UsageName");
				OnUsageNameUpdated(value);
			}
		}
	}

	public TauntEquipmentUsageIconBrushWidget(UIContext context)
		: base(context)
	{
	}

	private void OnUsageNameUpdated(string usageName)
	{
		Sprite sprite = ((usageName == null) ? null : IconsBrush?.GetLayer(usageName)?.Sprite);
		base.IsVisible = sprite != null;
		if (base.IsVisible)
		{
			base.Brush.Sprite = sprite;
			base.Brush.DefaultLayer.Sprite = sprite;
		}
	}
}
