using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets.Menu.TownManagement;

public class DevelopmentItemVisualWidget : Widget
{
	private const string _defaultBuildingSpriteName = "building_default";

	private bool _changedVisualToSmallVariant;

	private bool _useSmallVariant;

	private bool _isDaily;

	private string _spriteCode;

	private Widget _developmentFrontVisualWidget;

	[Editor(false)]
	public bool UseSmallVariant
	{
		get
		{
			return _useSmallVariant;
		}
		set
		{
			if (_useSmallVariant != value)
			{
				_useSmallVariant = value;
				OnPropertyChanged(value, "UseSmallVariant");
			}
		}
	}

	[Editor(false)]
	public bool IsDaily
	{
		get
		{
			return _isDaily;
		}
		set
		{
			if (_isDaily != value)
			{
				_isDaily = value;
				OnPropertyChanged(value, "IsDaily");
			}
		}
	}

	[Editor(false)]
	public string SpriteCode
	{
		get
		{
			return _spriteCode;
		}
		set
		{
			if (_spriteCode != value)
			{
				_spriteCode = value;
				OnPropertyChanged(value, "SpriteCode");
				_changedVisualToSmallVariant = false;
			}
		}
	}

	[Editor(false)]
	public Widget DevelopmentFrontVisualWidget
	{
		get
		{
			return _developmentFrontVisualWidget;
		}
		set
		{
			if (_developmentFrontVisualWidget != value)
			{
				_developmentFrontVisualWidget = value;
				OnPropertyChanged(value, "DevelopmentFrontVisualWidget");
			}
		}
	}

	public DevelopmentItemVisualWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (!_changedVisualToSmallVariant)
		{
			string text = DetermineSpriteImageFromSpriteCode(SpriteCode, UseSmallVariant);
			base.Sprite = base.Context.SpriteData.GetSprite((!string.IsNullOrEmpty(text)) ? text : "building_default");
			if (!IsDaily && DevelopmentFrontVisualWidget != null)
			{
				DevelopmentFrontVisualWidget.Sprite = base.Sprite;
			}
			_changedVisualToSmallVariant = true;
		}
	}

	private string DetermineSpriteImageFromSpriteCode(string spriteCode, bool useSmallVariant)
	{
		string text = "";
		switch (spriteCode)
		{
		case "building_fortifications":
		case "building_wall":
			text = "building_fortifications";
			break;
		case "building_settlement_garrison_barracks":
		case "building_castle_barracks":
			text = "building_garrison_barracks";
			break;
		case "building_settlement_training_fields":
		case "building_castle_training_fields":
			text = "building_training_fields";
			break;
		case "building_settlement_granary":
		case "building_castle_granary":
			text = "building_granary";
			break;
		case "building_settlement_fairgrounds":
		case "building_castle_fairgrounds":
			text = "building_settlement_fairgrounds";
			break;
		case "building_settlement_marketplace":
			text = "building_marketplace";
			break;
		case "building_settlement_aquaducts":
			text = "building_aquaduct";
			break;
		case "building_settlement_forum":
			text = "building_forum";
			break;
		case "building_settlement_workshop":
		case "building_castle_workshops":
			text = "building_workshop";
			break;
		case "building_settlement_militia_barracks":
		case "building_castle_militia_barracks":
			text = "building_militia_barracks";
			break;
		case "building_siege_workshop":
		case "building_castle_siege_workshop":
			text = "building_siege_workshop";
			break;
		case "building_castle_lime_kilns":
			text = "building_lime_kilns";
			break;
		case "building_castle_gardens":
		case "building_settlement_lime_kilns":
			text = "building_gardens";
			break;
		case "building_castle_castallans_office":
			text = "building_wardens_office";
			break;
		case "building_daily_build_house":
			text = "building_daily_build_house";
			break;
		case "building_daily_train_militia":
			text = "building_daily_train_militia";
			break;
		case "building_festivals_and_games":
			text = "building_daily_festivals_and_games";
			break;
		case "building_irrigation":
			text = "building_daily_irrigation";
			break;
		default:
			return "";
		}
		if (useSmallVariant)
		{
			text += "_t";
		}
		return text;
	}
}
