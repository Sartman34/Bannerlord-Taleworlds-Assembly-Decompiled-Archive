using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Multiplayer.ClassLoadout;

public class MultiplayerClassLoadoutTroopTupleVisualWidget : Widget
{
	private bool _initialized;

	private string _factionCode;

	private string _troopTypeCode;

	private bool _useSecondary;

	public string FactionCode
	{
		get
		{
			return _factionCode;
		}
		set
		{
			if (value != _factionCode)
			{
				_factionCode = value;
				OnPropertyChanged(value, "FactionCode");
			}
		}
	}

	public string TroopTypeCode
	{
		get
		{
			return _troopTypeCode;
		}
		set
		{
			if (value != _troopTypeCode)
			{
				_troopTypeCode = value;
				OnPropertyChanged(value, "TroopTypeCode");
			}
		}
	}

	public bool UseSecondary
	{
		get
		{
			return _useSecondary;
		}
		set
		{
			if (value != _useSecondary)
			{
				_useSecondary = value;
				OnPropertyChanged(value, "UseSecondary");
			}
		}
	}

	public MultiplayerClassLoadoutTroopTupleVisualWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (!_initialized)
		{
			base.Sprite = base.Context.SpriteData.GetSprite("MPClassLoadout\\TroopTupleImages\\" + TroopTypeCode + "1");
			base.Sprite = base.Sprite;
			base.SuggestedWidth = base.Sprite.Width;
			base.SuggestedHeight = base.Sprite.Height;
			_initialized = true;
		}
	}
}
