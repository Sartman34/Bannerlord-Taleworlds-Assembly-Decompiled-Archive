using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace TaleWorlds.MountAndBlade.GauntletUI.Widgets;

public class EquipmentTypeVisualBrushWidget : BrushWidget
{
	private bool _hasVisualDetermined;

	private int _type = -1;

	[Editor(false)]
	public int Type
	{
		get
		{
			return _type;
		}
		set
		{
			if (_type != value)
			{
				_type = value;
				OnPropertyChanged(value, "Type");
			}
		}
	}

	public EquipmentTypeVisualBrushWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		base.OnLateUpdate(dt);
		if (!_hasVisualDetermined)
		{
			this.RegisterBrushStatesOfWidget();
			UpdateVisual(Type);
			_hasVisualDetermined = true;
		}
	}

	private void UpdateVisual(int type)
	{
		switch (type)
		{
		case 0:
			SetState("Invalid");
			break;
		case 1:
			SetState("Horse");
			break;
		case 2:
			SetState("OneHandedWeapon");
			break;
		case 3:
			SetState("TwoHandedWeapon");
			break;
		case 4:
			SetState("Polearm");
			break;
		case 5:
			SetState("Arrows");
			break;
		case 6:
			SetState("Bolts");
			break;
		case 7:
			SetState("Shield");
			break;
		case 8:
			SetState("Bow");
			break;
		case 9:
			SetState("Crossbow");
			break;
		case 10:
			SetState("Thrown");
			break;
		case 11:
			SetState("Goods");
			break;
		case 12:
			SetState("HeadArmor");
			break;
		case 13:
			SetState("BodyArmor");
			break;
		case 14:
			SetState("LegArmor");
			break;
		case 15:
			SetState("HandArmor");
			break;
		case 16:
			SetState("Pistol");
			break;
		case 17:
			SetState("Musket");
			break;
		case 18:
			SetState("Bullets");
			break;
		case 19:
			SetState("Animal");
			break;
		case 20:
			SetState("Book");
			break;
		case 21:
			SetState("ChestArmor");
			break;
		case 22:
			SetState("Cape");
			break;
		case 23:
			SetState("HorseHarness");
			break;
		case 24:
			SetState("Banner");
			break;
		default:
			SetState("Invalid");
			break;
		}
	}
}
